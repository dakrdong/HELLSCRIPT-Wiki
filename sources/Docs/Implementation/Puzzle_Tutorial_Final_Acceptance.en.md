# Puzzle tutorial final integration — stage 9

Written 2026-10-07. [한국어](Puzzle_Tutorial_Final_Acceptance.md) · [Evidence](Evidence/Puzzle_Final_Acceptance.json)

Stages 1–8 include the skill-tree improvements from `main` (`57bfd1ac`) and the latest workflow/validator cache (`47f98fbb`, merge `09d54317`). The latter has no game, asset or package changes, so existing game results are reused. The unfiltered Edit Mode suite ran once; all eight reported failures were repaired and verified through focused checks. macOS journey acceptance and three OFF/three ON effects measurements are complete. This execution ends at commits, pushes and PRs. The merger owns merging and public wiki deployment.

## Final solution policy and balance

The instructor offers an efficient counter, not the only accepted solution. Every lawful actual combat or operation victory receives the same XP, progress and rewards regardless of its settings. Outcomes and HP are not forced. The owner's 100-point example was not expanded into an unrequested score feature or save schema.

L4 retains the approved N09 crossbowman in wave one. The second wave's N01 health for Warrior/Ranger/Mage changed from 1950/500/1800 to 1000/350/1000. Final checks use genuine accounts that cleared the preceding levels; balanced matches the initial settings.

| Class | Initial settings | Always aggressive | Always careful | Recommended careful → aggressive |
| --- | --- | --- | --- | --- |
| Warrior | Win 64.80s, 3 potions | Win 85.40s, 4 potions | Timeout 90s | Win 56.30s, 1 potion |
| Ranger | Win 69.35s, 3 potions | Timeout 90s | Timeout 90s | Win 65.75s, 2 potions |
| Mage | Timeout 90s | Win 71.60s, 4 potions | Timeout 90s | Win 56.25s, 1 potion |

The recommended path is fastest and consumes the fewest potions in all three classes. Mage ending HP is lower than the aggressive alternative, so this is not dominance on every metric. Every actual L4 victory was checked for L5 access and level 4/XP45. Historical unique-answer tuning remains as history; this final owner decision supersedes it.

The Warrior fights through damage: base HP is `450 + 50 × (level−1)` and the base armor term is `STR × 2 + 60 + 4 × (level−1)`. Ranger/Mage bases and XP remain unchanged. Equipment and bonus stats use the existing calculation. See the actual graduated Rift 1–5 outcomes in [stage 8](Puzzle_Tutorial_Graduation.en.md).

Three focused checks use byte-identical accounts captured immediately after native graduation in the existing combat simulator. All classes clear Rifts 1–5 and reach level 10 without added XP or skill investment. This does not claim native-player completion of those Rifts. The native Warrior’s `target.chaseTime` value (90 versus 20 in the earlier simulation-prepared Warrior account) is verified through the actual inputs.

| Class | Rift 1 / 2 / 3 / 4 / 5 clear seconds | After Rift 5 |
| --- | --- | --- |
| Warrior | 102.40 / 104.90 / 118.60 / 123.00 / 139.45 | Lv10 · XP 1326 |
| Ranger | 123.30 / 142.00 / 125.45 / 251.81 / 158.76 | Lv10 · XP 1451 |
| Mage | 132.50 / 146.40 / 152.50 / 148.55 / 186.71 | Lv10 · XP 1438 |

## Final implementation

- L1 guides actual skill learning, equipping, saving and closing from the waiting room. It reuses the skill window and input gate; independent review does not force the first lesson again.
- Fixed navigation uses the shared window host and respects skill bubbles and save modals. It becomes usable in the frame the slide completes. Fixed status bars, background brightness/saturation, light flow, torches, rays, background motion and reused embers are connected.
- Existing device reduced-motion settings and the macOS system setting apply. Other OS integration is unverified. Level and progress fit the plate's black opening; its title uses the shared dark caption theme.
- Gate masks read the final layout in `LateUpdate`. Edict and rune-board guidance share this owner. Combat and save tick order are unchanged.
- Hint transactions retain the live combat reference before committed notifications. The same hint click can save instructor settings into that run, and later losses preserve opened hints. Six focused checks passed.
- Presentation keys distinguish initial scouting from intermission. Heroes without a skill tree may receive only the exact existing owner-produced migration, without new editing permission. All five related checks passed, including preservation of the actual after-L8 Mage account and rejection of unrelated edits.
- Graduation reuses the existing arrival cinematic so the first greeting precedes growth guides. Reopening before graduation establishes the existing game page, preventing character-selection cleanup from cancelling arrival.
- Native acceptance reveals a single spoken line through its actual next button. Long English dialogue no longer trips a reading-time timeout; combat and dialogue speed stay unchanged.
- The existing common review-close button uses the guarded fixed navigation layer so the background cannot cover it. The journal selects the independent review lobby. Closing or winning review after graduation returns to town and preserves owned gear, edict, economy and the single graduation reward.

## Results and acceptance scope

Unfiltered Edit Mode: **5614 cases, 5604 passed, 8 failed, 2 explicit measurement skips**, 3177.92 seconds. The full suite was not repeated. Later small changes received directly affected checks.

Failures concerned Unity `GetComponent` null operators in two UI owners, the deterministic HP golden predating Warrior bases, three tests overwriting existing read-only edict evidence, and three old Warrior L2/L5 prototype conditions. Repairs use `TryGetComponent`, change only the HP column while keeping strict equality, isolate evidence per invocation, and use the learned EDGE response and current L5 attack condition. No assertions were removed or tolerances widened.

Focused rechecks passed 164 of 165 before the remaining HP golden was updated; strict field equality and refusal to overwrite evidence then passed both checks. All 21 gate/binding checks passed. Shared UI contract validation, 15 final contract tests, refresh static validation and 42 skill-tree checks passed. Unchanged successful coverage was reused.

Final macOS acceptance passed. Scope: three classes × KO/EN × portrait 440×956/landscape 956×440, 12 journeys with independent-process L7 restore. Coverage includes L1–15, actual first losses/retries, scouting/review/instructor, P1/P2 equipment and enhancement transactions, real write failure/retry, graduation/independent review, normal Rift admission and portal return. Three classes × KO/EN × five resolutions (440×956, 956×440, 1600×900, 1600×1000, 2100×900) produce 30 hub layouts plus 12 safe-area simulations. Default text size only. Completed spans are reused; only failed or unfinished spans were rerun. Warrior KO portrait completion continues its exact ready-to-graduate account; Warrior EN portrait continues its actual L2 loss account. Only Mage EN landscape was retried after an exit without completion evidence. `stage9-final-verification-audit.json` verifies 95 source inputs, 180 real victory records and the evidence for each span.

The native driver uses actual EventSystem pointers and accelerated fixed combat ticks. With no matching live Editor connection, existing batch tooling ran against the verified worktree. These results do not establish physical-mobile operation, ordinary-speed combat, human comprehension or native normal-Rift victory.

Effects were measured with two actual macOS development binaries from the same final source (`9b7a8e4c`) and genuine after-L8 Mage fixture, three launches each. Conditions: M3 Pro, Unity 6000.6.0f1, Mono/Metal, PC quality, EN, 1280×720, target 60 FPS, VSync 0, 3-second warmup and 20-second sample. Account, gear, map, seed and quality inputs match; system reduced motion was off. The one-line OFF patch, complete file manifests for both apps and hashes for 84 raw files are retained.

| Metric | Effects OFF | Effects ON |
| --- | --- | --- |
| CPU ms/frame, three launches | 5.56 / 5.64 / 5.81 | 5.60 / 5.55 / 6.41 |
| CPU utilization range | 32.66–34.20% | 32.78–37.70% |
| Actual FPS range | 58.73–58.88 | 58.83–59.03 |
| Frame p95 range | 19.90–20.49ms | 19.11–20.51ms |
| Per-launch maximum frame time | 52.95–78.15ms | 50.84–52.67ms |
| Mean GC allocation range | 87,098–87,799 B/frame | 86,952–87,789 B/frame |
| Process RSS during samples | 536.58–616.53 MiB | 542.33–607.11 MiB |

CPU medians are 5.64→5.60ms, but ranges overlap and the third ON launch reaches 6.41ms. This does not establish zero effect cost or a performance improvement. Main Thread timing includes target-FPS waits (median OFF 14.61ms/ON 14.68ms), so it is not CPU work time. GC, spikes and memory above are whole-process observations including seven autosaves per launch. No whole-game, release-build, browser or mobile performance claim is made. [Raw measurement summary](../../Artifacts/PuzzleTutorial/20261006/stage9-effects-comparison.json).

The explicit task-plan section 2.4 and user-provided rules require one final local wiki build/check and Python/UI check set. This task-specific exception takes precedence over the repository’s newer general branch-validation omission rule; it does not authorize public deployment.

## Evidence and artifact disposition

The full suite's old fixed output paths overwrote **41 raw JSON/TXT files from stages 7–8**. Prior XML, logs, metrics and recorded hashes remain; the prior raw files were not recovered. `stage9-evidence-supersession.json` records paths, old/current hashes and that limitation. New raw versions are also retained separately. Subsequent tests isolate every invocation and refuse existing evidence filenames.

XML, logs, captures, states and binaries remain in `Artifacts/PuzzleTutorial/20261006`. `artifact-lifecycle.json` records owner, exact paths, recipes, sources, measured sizes, dependencies and the **2026-10-13 review date**. Failed native reproductions remain separately identified. Permanent deletion is not approved; no cleanup completion or reclaimed-space claim is made. Dirty user work in the original checkout remains protected.

Retention snapshot, 2026-10-07 16:17 (logical file sizes; overlapping parent paths are not added): task evidence directory 13.60 GB, this worktree’s `Library` 3.49 GB and existing `Wiki/site` 6.36 GB. The final ON/OFF apps, included in the evidence total, are approximately 727 MB each and retained for comparison/reproduction. Data-volume free space was approximately 355.56 GB. The [artifact inventory](../../Artifacts/PuzzleTutorial/20261006/stage9-artifact-inventory.json) records exact paths; the lifecycle record will update sizes after local wiki generation. No paths were deleted.
