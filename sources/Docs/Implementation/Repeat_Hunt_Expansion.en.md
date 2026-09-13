# Repeat hunts and saved result recovery

Date: 2026-09-13 · [한국어](Repeat_Hunt_Expansion.md)

Status: **local repeat limits, transactional cleanup and saved result recovery passed all 2,198 final Editor tests and four native macOS launches/restarts.**

## Player flow

The sanctuary and rift keeper show the actual repeat owner, victory/defeat behavior, loss streak, attempt/time/target limits, required bag space and result delay. An enabled core edict owns these settings; otherwise the base behavior configuration does. The result screen reads the same policy. Stopping after a victory does not disable an independently configured retry after defeat.

Results display the next stage, remaining countdown, session attempts, consecutive defeats, foreground session time, earned gold and cleanup or stop information. Stop immediately cancels the session and disables repetition in its actual configuration owner. Disabling the core edict's repetition does not overwrite unrelated base behavior settings.

Viewing combat history, repeat conditions, equipment or storage pauses the next-run countdown. Returning to results resumes its remaining time, including the new results return route from inventory. Common settings, confirmation dialogs and background state prevent another run from starting, as required by the [screen layout specification](../Design/HELLSCRIPT_Screen_Layout_Detail.md).

## Counting and limits

| Setting | Semantics |
|---|---|
| Victory | Same stage, next stage or stop. The next stage stays within actual unlocks. |
| Defeat | Same stage, one stage lower or stop. The minimum stage is one. |
| Consecutive defeats | A victory resets the streak; relaunching the same session does not. |
| Attempts | Both victories and defeats count, including the initial manual entry. Zero is unlimited. A runId is counted once. |
| Session time | A monotonic clock counts foreground combat, pauses and result/equipment viewing. Common settings, background time, app closure and town waiting before restoring a result are excluded. Combat speed is not applied. |
| Time limit | Finish the current battle before stopping. Reaching the limit while viewing results also prevents another run. |
| Gold | Count actual combat, boss, character first-clear and chest awards in this session. Spending does not reduce progress; sales, sweeps and offline rewards do not complete the target. |
| Legendary/set target | Actually acquire any item carrying a selected effect or set. A full set is not required. Generated or unclaimed rewards do not count; subsequent disposal does not undo acquisition. |
| Stage target | Successfully finish the specified stage or higher. Entry or defeat alone does not count. |
| Result delay | Base behavior retains five seconds. Core edicts use their 0–30 second setting in 0.5-second steps, with a three-second default. |

When multiple limits apply, the representative reason follows cancellation, disabled repetition, outcome policy, loss streak, attempt limit, time, gold, equipment and stage order. Any satisfied stop condition prevents another attempt. A new manual entry starts a new session. Completed results retain their own policy rather than being rewritten when current settings change.

## Cleanup and readiness

Configured portal cleanup and cleanup after each rift now use the existing staged save transaction. Each grade independently keeps, sells, salvages or moves equipment to storage; the set row overrides the ordinary legendary row. Ownership changes only after the staged transaction is saved. A failed batch rolls back earlier operations and retries cannot award the same sale twice.

Locked, equipped and current-build or any character's preset-referenced items are protected. Optional enhanced-item, effect and set protection is honored. Legendary/set sale or salvage requires its separate enabled permission. Storage uses the existing 400-slot limit. Full storage keeps the item in the bag and follows the selected keep/stop behavior without falling back to sale or salvage.

Insufficient configured bag space, save/navigation/edict faults or unfinished cleanup prevent the next run. The player can free space or recover the fault and check readiness again. The continue-with-limited-loot option leaves new equipment unclaimed without deleting owned gear. Pickup of new drops resumes when space becomes available. Explicit starting-space requirements still apply.

## Save ownership and relaunch

The additive version-one `AccountSave.repeatHunt` stores session/hero/run identities, the last counted run, counters, time, earned gold, policy, countdown, cleanup report and one completed result. Active battles retain the existing `suspendedRun` field. Completed results are separate from active battles. `RunState.earnedGold` is updated at the same award boundary as actual rift currency, including staged chest receipts.

Relaunch opens the sanctuary. Reviewing a saved result does not start another run; the player resumes its remaining countdown explicitly. Resuming an active battle retains the same session counters and streak. Completion and the candidate next battle must both be saved before the new battle is exposed to simulation. Failed saves retain the old result and rewards.

Old saves do not invent a session. Gold earned before this code observed a resumed legacy battle is not inferred from the balance. Unsupported session versions preserve the file and stop loading. Mismatched hero/run identities and invalid policies cannot become runnable sessions.

## Validation and remaining scope

All **2,198** Editor tests on the fixed final source, the macOS development build and **four** independent launches/restarts passed. See the [validation summary](../../Artifacts/Validation/RepeatHunt/validation-summary.json), [complete XML](../../Artifacts/Validation/RepeatHunt/Full4.xml) and [source/binary hashes](../../Artifacts/Validation/RepeatHunt/validated-source.json). Inputs are `7aa3312` plus this phase's source changes. Scenes, prefabs and packages were unchanged; 19 pre-existing user files were preserved. An earlier focused run passed 162 tests; the final complete run includes the later clock callbacks and error-message refinement.

A fresh Warrior entered stage one with seed 551 using normal controller updates at 1x. No health, enemy, terrain, reward or outcome injection was used. The hero died at 67.1 simulation seconds and approximately 67.1 actual combat seconds, reaching level three, 35 kills and 234 gold. The session measured about 72.8 seconds including result reading; the next-run countdown paused during detail review. No chest was opened in that natural run. Editor transaction tests separately cover actual chest gold, failed commits, retries and reload without duplicate credit.

The four processes checked restored core-edict results, a new automatically generated layout, active-battle recovery with one-time counting and base-configuration result recovery/cancellation. Stage lowering, count/time/gold limits, cleanup, bag capacity and disk failures use explicit boundary fixtures. Cleanup sales did not complete the hunting gold goal, and readiness had to be checked again after resolving a blocker. Background exclusion used a directly invoked `OnApplicationPause` callback.

All **14** final Korean/English portrait/landscape screenshots, including 140% text, were captured and directly reviewed. See the [visual review](../../Artifacts/Validation/RepeatHunt/visual-review.json), [large English guide](../../Artifacts/Validation/RepeatHunt/Native4/initial-02-details-en-portrait.png), [restored result](../../Artifacts/Validation/RepeatHunt/Native4/restart-01-restored-result-en.png) and [English save error](../../Artifacts/Validation/RepeatHunt/Native4/resume-active-06-save-blocked-en.png). The existing responsive review layout fixes subtitle clipping; lower result rows scroll above fixed footer actions.

Save normalization fixes Unity's materialization of missing nested objects, which could invalidate a base-configuration session or display a cleanup report that never occurred. Internal save paths remain in diagnostics; player-facing errors provide storage/access and retry guidance. This does not validate physical mobile touch, every device safe area or long-running device performance.

This advances local CHK-F05 and CHK-S02 repetition/persistence. Online disconnections, authoritative sessions/time and multi-device conflicts need the future online adapter. Actual gem ownership/protection, additional free-space cleanup triggers and replacement ranking options remain separate work, as do long device performance runs and detailed combat/movement/editing efficiency breakdowns.
