# Tutorial v3 replay smoke repair

Updated: 2026-10-05 · [한국어](Tutorial_Replay_Smoke_V3_20261005.md)

> Historical record: these results cover the v3 tutorial on 2026-10-05. It was later replaced by the v4 pit trial and the fifteen-level puzzle tutorial. See the [current puzzle tutorial](Puzzle_Tutorial_Final_Acceptance.en.md).

The final replay left open by the [previous stall investigation](Tutorial_Smoke_Stall_20261005.en.md) is repaired. Both original TutorialSmoke launches pass, with `HELLSCRIPT_TUTORIAL_RUNTIME_SMOKE_OK` and the existing `EconomySnapshot(a)==replayBefore` assertion reached. One application defect and one full EditMode timeout remain unresolved. This is not proof that replay guidance alone completes the lesson.

Only two development smoke files change. Application tutorial, save, Hunt Edict UI, transactions, tests and packages are unchanged. `codex/fix-tutorial-v3-replay-smoke` starts at `27c0d39e` on `claude/fix-tutorial-v2-survival-picker`, which includes `origin/main` at `17dc75e9` as observed at task start. The primary checkout and the other session's Hunt Edict UI changes are preserved.

## Cause and shared lesson following

The actual smoke account has `edictLegacyAccess=true` for v2 compatibility acceptance. `GameController.BeginTutorial(true)` creates a fresh non-legacy account to isolate a level 1 replay, so the replay uses v3. Two opening choices or arbitrary pointer following cannot complete its A/B observation and numeric input.

The existing skill and survival pointer loops in [RuntimeTutorialSmoke.Progression.cs](TutorialReplaySmokeEvidence20261005/RuntimeTutorialSmoke.Progression.cs.txt) become `FollowProgressiveSkillLesson` and `FollowProgressiveSurvivalLesson`. Skill assertions read the executing hero, `game.Combat.Hero`. The fresh-account acceptance retains its saved B observation checkpoint, initial process exit and independent restart.

The replay in [RuntimeTutorialSmoke.cs](../../Assets/HELLSCRIPT/Runtime/Presentation/RuntimeTutorialSmoke.cs) takes one opening choice and follows the helpers through learning, first-slot equipment and save, actual A/B observations, pause/restart, and final A selection/save. The survival lesson enters 60 in the actual numeric field, applies and saves it, and closes the window. The executing hero's HP threshold and actual automatic potion use, `tutorialLessonStep==5`, are checked before boss defeat and return to town. Existing fight deadlines and the final economy assertion are not increased or weakened.

The existing snapshot compares gold, materials, premium currency, cores, runes, level, XP, highest clear, record count and inventory items. It does not compare every account byte. The [completed report](TutorialReplaySmokeEvidence20261005/runtime-tutorial-smoke.txt) and [success marker](TutorialReplaySmokeEvidence20261005/tutorial-success-marker.txt) prove this path executed.

## Remaining application defect and actual smoke interaction

A legacy account's v3 replay survival guidance still points to a missing control. `DrawSelectedGroup` in [HuntEdictWindow.Summary.cs](../../Assets/HELLSCRIPT/Runtime/Presentation/HuntEdictWindow.Summary.cs) uses the original `store.Data` legacy state to display a quick picker. [DrawQuickPresetPicker](../../Assets/HELLSCRIPT/Runtime/Presentation/HuntEdictWindow.QuickPresets.cs) returns false until Custom is expanded, preventing numeric option rows from being created. [ProgressiveSurvivalLessonStep](../../Assets/HELLSCRIPT/Runtime/Presentation/GameUI.TutorialComparison.cs) instead uses the isolated v3 executing hero and requests `edict-option-survival.potionHpPercent`.

The screen has `edict-quick-picker-global/survival.potion` and lacks the numeric target. The [failure screenshot](TutorialReplaySmokeEvidence20261005/replay-hp-timeout.png), [failure trace](TutorialReplaySmokeEvidence20261005/replay-hp-failure.txt) and [active button dump](TutorialReplaySmokeEvidence20261005/replay-hp-timeout-controls.txt) show the mismatch. Application code is not repaired in this task.

Only for this combination, the smoke records the [missing target screen](TutorialReplaySmokeEvidence20261005/replay-hp-hidden.png) and [controls](TutorialReplaySmokeEvidence20261005/replay-hp-hidden-controls.txt), waits for the gate's normal two-second missing-target grace to release input, and uses real pointer events to open the quick picker and `edict-quick-choice-custom`. It asserts that the executing hero's complete `HuntEdictLoadout` JSON is unchanged by expanding the view. The normal numeric input, apply, save and actual potion-use lesson then continues. It does not disable the gate, mutate save data directly or force a lesson step.

This interaction restores the complete replay economy-isolation acceptance. It does not certify complete guidance. An application repair needs separate verification of the boundary between the legacy saved account and the v3 executing hero.

## Other stale smoke expectations

The fresh-account acceptance that shares the helpers had three stale expectations:

- The combat-tab fixture changes highest clear from 3 to 4. Current progression data adds survival options at 3 and first discloses combat targeting at 4. This fixture does not prove natural progression.
- The smoke selects the actual `edict-group-survival.potion` group before checking its automatic potion row.
- After arrival, it uses the compatibility smoke's device attendance-popup suppression and closes any open attendance panel. The [masked notice failure screenshot](TutorialReplaySmokeEvidence20261005/progression-attendance-timeout.png) is preserved. Optional edict guidance and Later acceptance remain.

The development-only `-hellscriptTutorialReplayOnly` flag is added alongside `-hellscriptTutorialSmoke`. It requires a completed disposable tutorial save and allows focused replay reproduction. The final TutorialSmoke pass used the original two launches without that shortcut.

## Validation and limits

Unity 6000.6.0f1, a macOS Metal development player and synthetic EventSystem pointers were used. The [validation summary](TutorialReplaySmokeEvidence20261005/validation-summary.json) records failed attempts, commands, binary hashes and exact result reuse.

| Check | Result |
| --- | --- |
| Original TutorialSmoke initial / resume | Pass 49.4s / 535.4s; final marker and economy assertion reached |
| Fresh EdictProgressionSmoke initial | Pass 28.6s; actual saved B checkpoint |
| Only the failed fresh resume retried | Pass 108.5s; B restoration, final A, HP 60, automatic potion, town, save focus, disclosed tab, Later and manual restock |
| Full EditMode, once | 5,178 total: 5,177 passed, 1 failed, 0 skipped; XML test duration 2,578.3s |
| Only the failed EditMode case retried | One failure, same 180-second timeout |
| Shared UI contract | Check passed; 11 tests passed |
| macOS development builds | Final source build succeeded; hashes retained for the tutorial-pass player and last fresh-account retry player |

The EditMode failure is `Hellscript.Tests.ObjectiveProgressTests.ExplorationWalksToEveryKnownOfferingAndDeliversWithoutTeleporting`. Both [full XML](TutorialReplaySmokeEvidence20261005/editmode.xml) and [isolated retry XML](TutorialReplaySmokeEvidence20261005/failed-objective.xml) report the same 180,000ms timeout. Its cause is not established. This task changes neither that simulation nor its test or timeout. The full suite is not reported as passing.

After the first final batch failed, only failed checks and directly affected coverage were repeated. Only the two smoke files differ after the full EditMode run; application, test and package sources are unchanged. The [full-suite source manifest](TutorialReplaySmokeEvidence20261005/tested-source-manifest.json) and [final source manifest](TutorialReplaySmokeEvidence20261005/final-source-manifest.json) are preserved. There was no second full suite.

The only change after the TutorialSmoke pass is three attendance-popup lines inside `ProgressiveAcceptance`. The tutorial entrypoint and shared helpers are identical. [Reuse evidence](TutorialReplaySmokeEvidence20261005/tutorial-result-reuse.json) records hashes and the exact difference, so the already-passing complete tutorial was not repeated. The fresh initial launch was also reused through its preserved B checkpoint; only its failed resume was rerun.

Layout checks used default text size at 440×956, 956×440, 1600×900, 1600×1000 and 2100×900 in KO/EN, ten profiles. Retained evidence includes [A observation](TutorialReplaySmokeEvidence20261005/comparison-a.png), [B observation](TutorialReplaySmokeEvidence20261005/comparison-b.png), [English comparison](TutorialReplaySmokeEvidence20261005/starter-comparison-440x956-en.png), [final replay scene](TutorialReplaySmokeEvidence20261005/rewardless-replay.png), the [fresh-account report](TutorialReplaySmokeEvidence20261005/progression-result.txt) and [manual restock](TutorialReplaySmokeEvidence20261005/entry-manual-restock-440x956-ko.png). No whole-screen human visual approval, physical input, physical mobile, WebGL, other classes or unrelated runtime smokes are claimed.

Local wiki build/check and Python/Node tests use the scratch project below. Exact commands, results and logs are recorded in `evidence/wiki-validation-results.json` under the task root. Merging into main, public wiki publication and browser delivery checks are excluded by the user's instruction. The later merge owner publishes the branch content.

## Artifacts and retention

The dedicated root is `/private/tmp/hellscript-tutorial-replay-v3-20261005-6hihv708`. Branch Assets, Packages and ProjectSettings plus the primary checkout's warm Library were APFS-cloned. The long full test run held the first clone's lock, so one independent player-build clone was added. Primary Temp, existing player outputs, other task checkouts and full-project backups were excluded. Only referenced historical Artifacts/Validation and Builds files were separately materialized for wiki checks.

Directory sizes measured before wiki generation follow. APFS shared blocks mean their sum is not this task's exclusive physical storage.

| Subdirectory | Reported size (KiB) | Purpose |
| --- | ---: | --- |
| project | 10,628,320 | Full tests, isolated failure reproduction and local wiki |
| player-project | 8,309,776 | Independent player builds |
| player | 2,476,180 | Four apps: initial, diagnostic, tutorial pass and last fresh-account retry |
| evidence | 224,772 | Logs, XML, screenshots, commands, disposable saves and checkpoints |

The root's `artifact-lifecycle.json` records exact paths, generation commands, source hashes, final measured sizes, retention and approval state. Evidence and binaries are due for retention review on 2026-10-12; this date does not authorize deletion. Temporary outputs are retained pending explicit approval and no cleanup is performed. All Unity/player launches use a task-owned `-hellscriptSavePath`. The ordinary player save directory is unused, and no other session's process is terminated.

Only the compact reproduction material in the [evidence file/hash manifest](TutorialReplaySmokeEvidence20261005/evidence-manifest.json) is committed. The primary checkout's untracked runner and plan are unchanged. Commands are arrays in [initial launch](TutorialReplaySmokeEvidence20261005/tutorial-phase-one-command.json), [resume](TutorialReplaySmokeEvidence20261005/tutorial-resume-command.json) and [fresh-account retry](TutorialReplaySmokeEvidence20261005/progression-resume-command.json). Runner exit alone is not treated as passing; per-launch results and markers are checked.
