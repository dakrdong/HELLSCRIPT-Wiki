# Adding 20 Monsters a Day

Updated: 2026-10-11 · [한국어](Monster_Daily_Pipeline.md)

This is the runbook for the daily routine that adds the 138 monsters of the [monster roster per field](../Design/HELLSCRIPT_Monster_Roster_20261011.en.md) to the game, 20 per day. The routine follows it as written. The user's rules (2026-10-11): use the 20 free Hunyuan generations every day; do reduction, Codex illustration, game integration and Monster Codex entries in one pass; a monster spawns in rifts from the day it is added; when checks pass, merge into main automatically and publish the wiki.

## 0. Inputs and locations

| What | Where |
| --- | --- |
| Generation order | `tools/monster_pipeline/day_order.json` (roster §10.1 order, 134 monsters) |
| Prompt, archetype, height | `tools/monster_pipeline/monster_specs.json` |
| Progress | `AssetDeliverables/Hunyuan/monster_state.json` (outside git), `tools/monster_pipeline/state.py` |
| Originals per day | `AssetDeliverables/Hunyuan/<YYYY-MM-DD>/` (`urls.txt`, `previews/`, `raw/`, `selection.json`, `portraits/out/`) |
| Browser script | `tools/monster_pipeline/hy3d_runner.js` |
| Monster registry | `Assets/HELLSCRIPT/Resources/Monsters/MonsterRegistry.json`, source `Docs/Design/MonsterRoster/roster.json`, tool `tools/monster_registry.py` |
| Rig and install | `tools/hunyuan_monster_ready.py`, `tools/hunyuan_monster_install.py` |

## 1. Start checks

1. `python3 tools/monster_pipeline/state.py show` shows backlog. Finish earlier days' `generated`, `picked`, `rigged` and `portrait` stages before today's generation.
2. Create the work branch `claude/monsters-<date>` as a worktree from the latest `origin/main`. Other sessions use the main checkout, so never work there directly.
3. Open `https://hy3d.tencent.ai/` in the in-app browser. If signed out, skip generation and tell the user (never type a password). Read the remaining allowance from the page's own `POST /api/3d/quotainfo` response (`remainQuota`) with `read_network_requests`. The allowance resets around 01:00 KST.

## 2. Hunyuan generation

1. Today's targets: `state.py next <remainQuota>`.
2. AI Creation → Text-to-3D, model `3DGeneration-V3.1`, 50k faces (PBR is on by default).
3. Run the contents of `hy3d_runner.js` with the javascript tool, set `window.__queue=[[ID, prompt from monster_specs.json], …]`, then call `window.__start()`. Three run at once; a fourth is refused without using the allowance. Record submitted IDs with `state.py set submitted …`.
4. The javascript tool stops at 45 s, so wait with a background `sleep` (10–15 min). One job takes 5–10 minutes; 20 take about 1.5–2 hours.
5. For finished jobs, feed `await window.__compact(window.__ready())` into `python3 tools/monster_pipeline/expand_compact.py <day_dir>` to get `urls.txt` (signed URLs are moved in compressed form; the browser cannot post to a local address). `state.py set generated …`.

## 3. Picking candidates

1. `cd <day_dir> && python3 <repo>/tools/monster_pipeline/fetch_previews.py <subset of urls>` builds a sheet of the 4 candidates per ID (`previews/<ID>_sheet.jpg`); look at it.
2. Criteria: the silhouette reads from top-down; limbs apart from the body for rigging; no ground or base (note it if present); the roster's weapon and features are visible; no gore such as wounds or exposed flesh.
3. `python3 <repo>/tools/monster_pipeline/pick.py urls.txt "ID:candidate:reason" …` downloads only the chosen GLB (`raw/<ID>.glb`, hash and reason in `selection.json`). `state.py set picked ID=candidate`.
4. If no candidate passes, fix the prompt in `monster_specs.json` and regenerate with a later day's spare allowance.

## 4. Reduction and rigging

Run `python3 tools/hunyuan_monster_ready.py --glb raw/<ID>.glb --id <ID> --archetype <specs.archetype> --height <specs.height> [--boss] --out <work dir>`, then look at the QA sheet and the automatic verdict. On failure retry with another archetype, height or candidate. On success `python3 tools/hunyuan_monster_install.py --id <ID> --from <work dir>` installs into `Resources/World/HunyuanReady/`. When the gates pass but the sheet's `front` cell shows a profile, the body faces sideways: rig again with `--yaw -90` (or `90`). `tools/monster_pipeline/retry_rig.py` tries candidates in the given order. Reject a pass whenever the windup or walk cell shows a piece torn loose, a weapon leaving the hand or cloth lifting, whatever the verdict. When all four candidates are rejected, record the reason in the state file's `_deferred` and in `selection.json`, clear the ID's state so `state.py next` returns it first the next day, and rewrite its `monster_specs.json` prompt (for example thin long legs → short thick legs, ground slab → flat feet). `state.py set rigged …`. Detailed rules are in both tools' help and the contract in `Editor/HunyuanReady/HunyuanReadyImporter.cs`.

## 5. Codex illustration

1. `python3 tools/monster_pipeline/run_portraits.py <stage> <day_dir> ID …` (5–7 per run, up to 3 runs at once). The appearance comes from `monster_specs.json`, the accent colour from the field; the chosen candidate render is the design reference and two existing portraits are the style references. About 1–5 minutes per image.
2. `python3 tools/monster_pipeline/review_sheet.py <stage> <sheet.jpg> ID …` shows model and portrait side by side; regenerate only IDs whose design differs or that show text, borders or gore.
3. Keep them with `collect_portraits.py <stage> <day_dir>`, then on the work branch run `install_portraits.py <repo> <day_dir>/portraits ID …` to install `Resources/Art/TrainingGround/Portraits/<ID>.png` and update `Docs/Art/Monsters/portrait-manifest.json`. `state.py set portrait …`.

## 6. Game integration and the Monster Codex

1. New monsters: `python3 tools/monster_registry.py add <ID…>` moves the roster entries into `MonsterRegistry.json` and writes Korean and English text into `en.txt`. The existing 25 only change model and portrait; their registry entries stay.
2. `python3 tools/monster_registry.py validate` must pass (missing text, damage-per-second budget, control and slow limits, resources present).
3. The Monster Codex reads `MonsterRegistry.All`, so registered monsters appear in it directly (once the codex is on main). Confirm with the codex screen smoke that nothing else is needed.
4. Run only the related EditMode tests first (registry, combat primitives, HunyuanReady, codex).

## 7. Checks, merge and publish

1. Merge the latest `origin/main` into the work branch.
2. Once, after the last code change and the main merge: full EditMode, the macOS development build runtime smoke (including a rift scene that shows the new monsters), and `python3 tools/check_ui_contract.py --verify`. Separate checks that already failed on main from new failures.
3. If everything passes, open the PR and merge it. With a new failure, do not merge; report the PR and the cause to the user.
4. After the merge, build, check and publish following the wiki publishing procedure (`AgentRules/Wiki_Publish.md`) and confirm on the public site. `state.py set shipped …`.
5. Clean build clones and smoke output per the cleanup rules (`AgentRules/Artifact_Cleanup.md`). Keep original GLBs, previews and portrait originals.

## 8. Report

Report briefly in Korean: IDs and names added today, chosen candidates and why, failures and retries, check results (pass and fail counts), PR, merge and wiki state, remaining allowance and tomorrow's targets, and what was not verified (physical devices, performance). Public distribution approval of Hunyuan output is still open (`productionApproved: false`).
