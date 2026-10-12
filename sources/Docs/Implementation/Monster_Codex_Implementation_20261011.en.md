# Monster Codex implementation record

Date: 2026-10-11 · [한국어](Monster_Codex_Implementation_20261011.md)

## Scope and base

Start from `23ed052c` (`origin/claude/monster-registry`) on `codex/monster-codex`, as explicitly authorized by the user. This replaces the plan's main-first prerequisite. Use the registry contracts and skills(tags/damage/icon), without hard-coded roster counts. Combat balance is unchanged.

Defaults: exclude training and tutorial replays; include real tutorials, the guide and puzzles; place the dock icon immediately below Hunt Edict, including the prologue. Merge main and merge this PR only after the registry has reached main.

## Progress

1. Save and kill tracking: complete. Persist ID counters and a settlement flag in the run snapshot; settle through existing GameStore writes and tutorial transactions. Restore codex data and flags on failed disk writes. Ignore unknown IDs, training and replays.
2. List, landscape detail and localization: complete. Read entries, field tabs and denominators from the registry, lock undiscovered cards, and share EnemyStats and combat skill coefficients. Convert sustained damage using its actual tick interval; update values without rebuilding the slider during drags. Both requested Dark translations now use Shadow. The current combat implementation converts boss slows into stagger, which the view describes.
3. Portrait layout, boss icons and roar: complete. Skill chips use registry tags; boss icons use skills[].icon. The shared roar separates the phases. Short safe areas keep the fixed controls and scroll the body.
4. Selected dock icon and entry: complete. Preserve the approved A PNG bytes/alpha; use the shared menu texture budget. The shared content host opens the codex immediately below Hunt Edict in the plaza, battle and prologue.
5. Integration, final validation, captures and PR: main integration, conflict resolution and focused checks complete. Full EditMode, native macOS smoke, actual KO/EN captures and merge to main remain with the final integration owner.

## Validation

Run directly related EditMode tests at each step. Run full EditMode and the macOS development-player smoke batch once after the final code and main integration. Include UI contract/refresh checks, failed writes, restore/duplicate settlement, registry expansion, artPending and actual stats/skill damage.

Capture dock/list/normal/boss views and tier interactions in KO/EN at 440×956, 956×440 and PC 16:9/16:10/21:9. Use the default font size and distinguish native Mac evidence from physical devices. Keep task outputs and their lifecycle manifest in `Builds/Validation/MonsterCodex20261011`. Step 1 passed 16 related EditMode cases and 15 UI-contract regressions. Manual town return and character switching settle discarded runs; failed writes preserve account and live-run state. Restored the URP global settings changed by the first Unity import. Tests ran after the other full EditMode job exited.

Step 2: 150 related EditMode cases passed (preview values, registry growth, English, legacy enemy/boss attacks and effects), plus 15 UI-contract regressions and refresh ownership. Stopped the first launch after noticing another task had begun follow-up tests; resumed only after all batch jobs exited. No full suite or native smoke has run yet.

Step 3: 55 related EditMode cases passed, reusing 53 successful cases and rerunning the 12 affected window cases after fixing a clipped English resistance label. Coverage includes ten ratio/locale layouts, real tier and elite values, undiscovered-card guards, pause/back ownership, pending portraits, locale scroll restoration, registry boss icons/phases and instantaneous versus lasting zones. Full EditMode and native smoke remain pending.

Step 4: 1 related EditMode cases passed for the A icon and shared texture budget. Native interaction/capture and main integration are still pending.

## Native acceptance recipe (not yet executed)

`RuntimeMonsterCodexSmoke` runs only in a development player with `-hellscriptMonsterCodexSmoke`. Use an empty task-owned save folder, `-hellscriptOfflineQa -hellscriptServerBase invalid`, and `-hellscriptScreenshots <task-owned captures folder>`. Audio is muted before scene load. The batch checks prologue/plaza/battle icon placement, real death/snapshot/return/reload settlement, pause/back/repeat blocking, and pointer input for normal/elite/tier/boss/placeholder views. It captures KO/EN at 440×956, 956×440, 1280×720, 1280×800 and 1680×720. Unit layout checks are not native captures.

The registry has reached main. Full EditMode, macOS build/smoke and captures remain for the final integration candidate through the coordination owner. The Mac is currently locked; actual KO/EN screen validation is not complete. No public wiki generation or deployment is being performed in this coordination step.

Final preparation: 19 focused preview/window cases passed (18 reused, only the failed translation case retried). Confirmed actual view growth on a registry reload and English for all skill chips, including Summon/Projectile. The native acceptance code compiled; it has not run.

## Integration handoff

Draft PR: https://github.com/dakrdong/HELLSCRIPT/pull/96; branch codex/monster-codex. Worktree: /Users/t8g-2410-pn-005/.codex/worktrees/monster-codex/HELLSCRIPT. Evidence/lifecycle: Builds/Validation/MonsterCodex20261011. The coordination task reads these directly; no separate message transfer is needed.

Verified MonsterRegistry on origin/main `0082abdb91eed5afdad5d1ec7c3856dacf1dd36f` through PR #99. Recorded the original codex HEAD `2de4ac1108e061aae136833ae6b2f29f20eb5267` and clean worktree in the lifecycle manifest before merging main into the same branch. Pushed integration commit `91e1952232e7528aacfe825fc8f6938d4a7357ba` and retargeted PR #96 to main; GitHub reports CLEAN/Draft. PR #97 does not need a separate merge to satisfy this dependency. Run final full EditMode and one macOS smoke/capture batch through the sole integration owner. Do not merge this Draft PR or generate/publish the wiki beforehand.

Conflict resolution preserves both sets of en.txt additions and the two Shadow translations. N33's latest Self-Destruct Rush skill and the Exploder target category use distinct keys. The localization delta from main contains codex keys, the shared stagger-duration template and the two Shadow lines; unused legacy TARGET keys are not reintroduced. CombatSimulation.Deal retains damage reactions and fall survival before the final-death codex record. Registry data, models, attack range fixes and Type/Grade compatibility remain as shipped on main. Focused regressions passed for damage reactions, fall survival followed by exactly one final kill, destructible-object exclusion, N33 wording, and GameStore failed-write rollback.

Integration checks: Unity 6000.6.0f1 compiled Runtime/Tests and ran 63 focused EditMode cases: 61 passed and 2 locale checks failed. Replaced the stale fixed three-second stagger key with the runtime `{0}` template, then reran only the 2 failures and 3 directly affected locale cases; all 5 passed. Reuse the other successful results. UI-contract regressions (15) and refresh ownership (15 bindings, 180 UI files) passed. Full EditMode, native smoke and actual KO/EN screen validation have not run.

Evidence in the existing task validation directory: `integration-20261012/run.json`, `focused.xml`, `locale-retry-run.json`, `locale-retry.xml`, and `ui-contract.json` retain commands, merge parents, uncommitted input trees, PID, start/end and results. First run PID 14019, 2026-10-12 04:07:21.850–04:08:32.277 KST (70.4 s); locale retry PID 14612, 04:10:15.995–04:10:33.678 KST (17.7 s). Slot waiting: 0 s. Two focused Unity launches including the retry; one UI-contract invocation. Verified zero MCP instances and no active Unity/Blender/native heavy processes before launch. Reused the worktree and cache.

The growth-test fixture now chooses the next free kind/ID from Registry.All so the forthcoming 138-entry roster cannot collide with N120. The two affected tests passed after the heavy job finished; unrelated successful checks were not rerun.

Read-only merge-tree inspection on 2026-10-12 against prerequisite PR #97's registry e4a3e03c found two conflicts: appended keys in en.txt and CombatSimulation.Deal. Preserve both sets of English keys and the two Shadow translations. N33's Self-Destruct skill and the Exploder target category retain separate localization keys; the codex reads the registry skill key directly. Preserve DataSurvivesFall/OnDataDamaged before recording the final death next to State.kills++. Other combat files and GameStore auto-merge. Preserve registry Type/Grade compatibility and MonsterEdictTargetTests. The current registry JSON has 30 entries (5 artPending), distinct from the planned 138. This inspection did not change either branch, main or the roster, and no execution checks were repeated.

The native acceptance helper now scrolls horizontal field tabs and offscreen cards before checking actual pointer reachability. Its kill fixture uses an enemy naturally spawned in a rift. Compilation was checked; native smoke remains reserved for final integration.
