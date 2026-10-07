# Puzzle tutorial core v5

Updated: 2026-10-07

Korean: [퍼즐 튜토리얼 코어](Puzzle_Tutorial_Core.md)

New mandatory entry now uses a data-authored puzzle runner. This PR covers levels 1–2, persistence, fresh retries, detached replays, first-clear XP and the skill track. The remaining 15-level content, hub, operation/maintenance puzzles and graduation follow in later stages.

## Flow and ownership

`PuzzleTutorial.json` defines class skill tracks, scout/review/four hints, cumulative option permissions, timed spawns and class-specific health/attack/movement/preparation/elites/XP. `CombatSimulation.Puzzle.cs` saves its wave cursor and fights through the real enemy, dodge, skill and damage owners. Timeout remains distinct from death. Forced-loss enemy health floors, rooted movement and escalating trial attacks have been removed.

`AccountGuide.puzzle` owns the hero, current level, per-level attempts/preparation/clear/hints/result and initial kit. `GameStore.StartPuzzleLevel` accepts only the current level and returns its checkpoint when already running. A terminal attempt must be recorded before retrying. A fresh retry resets health, hazards, actions, cooldowns and AI memory while retaining saved answers.

`GameStore.RecordPuzzleAttempt` validates the owned checkpoint, hero and real terminal conditions. A receipt tied to the run ID grants first-clear XP once. L1 +120 XP produces Lv2 with XP20; L2 +140 produces Lv2 with XP160. Losses and replays grant no XP/gold. Graduation rewards are not granted by this core PR.

`PuzzleTutorial.CreateReplay` copies the initial kit of that level without changing the original account, currency, equipment or level. Previously completed accounts remain exempt and can replay the first puzzle on a detached account.

The skill window shows only the level’s authored nodes and active slots. Each tutorial node receives one point; further investment and other nodes remain available after graduation. Permissions apply in both presentation and `CommitHuntEdict` validation. L2 movement/dodge opens after its first real loss. Puzzle permissions take priority over ordinary rift disclosure.

## Level data

| Level | Starting Lv | Enemies | Outcome |
| --- | --- | --- | --- |
| 1 Your First Power | 1 | N02 ×4, HP65/attack3/speed2.4 | Learn and equip the real first skill, then defeat all foes |
| 2 One Crushing Blow | 2 | N02 ×5 + N14; worm HP550/attack1.3×hero max HP/preparation1.8s | Baseline dies. Saved All Warnings wins; Warrior also uses balanced survival. Warrior EDGE is an approved additional answer |

L2 follows the [79-row prototype gate](Puzzle_Level_Prototypes.en.md). Its preparation is applied when the real action begins, aiming at the hero’s position. Pending waves prevent premature clearing.

## Migration and restore

Completed and legacy-exempt accounts remain exempt. Entering the new runner migrates unfinished v4 checkpoints to level 1, retaining equipment, level and XP. First-skill allocation resets for its lesson, with the former build retained in `edictLegacyBuild`. The reset tool keeps its existing full new-account path.

A v5 checkpoint must match `tutorial-v5`, its owner/current level/wave cursor and mandatory training mode. Invalid progress is rejected while preserving the original. Empty puzzle progress and objective-free empty altars are normalized to handle JsonUtility materializing absent nested objects.

## Checks

- Sixteen core cases cover three-class L1/L2 learning/results, one-time XP/disk restart, fresh retry state, detached replay, tick-identical restore, ownership/closed settings, migration and catalog translations.
- Related objective chain/progress29, edict progression34, replacement pit data/restore/exempt replay8, localization33 and store-view binding13 passed: 133 distinct accepted cases, none skipped.
- The initial15 empty-progress failures and subsequent3 empty-altar restore failures remain recorded. After the correction, only the failed3 restores and directly affected29 objective cases were rerun. The12 unchanged passing core cases were reused.
- Shared UI ownership, UI contract11 and refresh checks passed. Scout/XP cards reuse `StoryDialogueWindow`; editing reuses `HuntEdictWindow`.
- The full EditMode suite and macOS development-player smoke/interaction validation are consolidated at final stage9. This evidence does not claim player, browser, physical-mobile or performance acceptance.

Evidence: `Artifacts/PuzzleTutorial/20261006/stage4-validation.json`, `stage4-core-ui.xml`, `stage4-checkpoint-objectives.xml`, `stage4-domain-ui.xml`. Failed reports and synthetic saves stay in the task directory; permanent deletion has not been authorized. PRs remain unmerged. The merger publishes the public wiki from integrated main.
