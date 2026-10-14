# Puzzle tutorial retirement

Updated: 2026-10-14

[Korean](Puzzle_Tutorial_Retirement_20261014.md)

On 2026-10-14 the user retired the puzzle-level tutorial (15 levels and the graduation trial, the waiting room, the operation and maintenance puzzles, and replays), and its code, tests and shipped assets were deleted. New accounts already enter only through the [guide tutorial stage](Guide_Tutorial_Script_20261011.en.md). As the user asked, the tutorial replay feature was removed without a trace as well. Earlier implementation pages stay as the record of their time, with a retirement note at the top.

## Deleted

- **16 runtime files:** `PuzzleTutorial*.cs`, `GameStore.Puzzle*.cs`, `CombatSimulation.Puzzle*.cs`, `GameUI.Puzzle*.cs`, `PuzzleHubLayout`, `PuzzleHubWindow`, `PuzzleMaintenanceWindow`, `RuntimeTutorialSmoke.Puzzle.cs`, plus `GameUI.LiveIncoming.cs`, the live damage-taken graph shown from puzzle level 2.
- **Tests:** the 7 `Puzzle*Tests` fixtures (84 cases), `TutorialPitTests` (4 cases on puzzle levels 1–2) and 2 D4 puzzle-kit checks.
- **Assets:** `Resources/PuzzleTutorial.json`, `Resources/Art/PuzzleHub/` (13 MB), `PuzzleHubTone.shader`, `Editor/PuzzleHubArtImporter.cs`, 2 texture-budget rows, and the waiting-room branch and tone control of the title atmosphere shader.
- **Replays:** `RunState.tutorialReplay`, the journal's P00 "Practice" (a puzzle level 1 replay), the level replay list in the journal's Completed tab, the pit replay lines and their smoke.
- **Shared parts only the puzzle used:** `UiSafeArea.Reserve`/`Release`/`Unreserved`, `ContentWindowView`'s `fullArea`, `CombatStatistics.TopIncomingLive`, the trial-bag arguments of `Economy.AddItem`, the puzzle comparison in `CombatPreviewSession`, and the `hub` performance profile mode (including `tools/profile_native_cpu.py`).
- **Localization:** 246 `en.txt` rows used only by the deleted code and the puzzle JSON.
- **Smokes:** the 15-level puzzle acceptance smoke, the puzzle branch of the tutorial smoke, the pit replay that no longer worked, and the replay prompt capture of the first-play acceptance. The tutorial smoke now requires a saved pit checkpoint; `RuntimeGuideTutorialSmoke` covers the new-account flow.

## Moved or changed

- `OwnedRun`, `StagedRun` and `AdoptPuzzleRun`, which the guide stage used, moved to `OwnedRun`, `StagedRun` and `AdoptTutorialRun` in `GameStore.cs`.
- The battle HUD clock reads `State.time` instead of `ElapsedTime`.
- Journal entry P00 and chapter C00 are renamed and described as "The Edict's Road" (the guide stage). P00's Practice button is disabled.
- `SkillTreeD4Loadout.Kit` became `PrologueKit`, which builds only the prologue kit. `ReleaseKit` clears the kit marks before resetting the allocation.
- Kept: `Docs/Art/PuzzleHub`, `Prototypes/PuzzleHub`, the `Artifacts/PuzzleTutorial` evidence and the puzzle record pages.

## Save compatibility

- Save keys and file names are unchanged. `AccountGuide.puzzle`, `TutorialSlot.puzzle`, the puzzle fields of `RunState` and `tutorialReplay` were removed from the code. JsonUtility ignores unknown fields, so old saves load as they are and only those fields drop out on the next save.
- A D4 skill kit (`kitKind="puzzle"`) left on a hero who stopped partway through is released on load with its skill points refunded (`Tutorials.Normalize`). Left in place, the kit owner check would stop the load with "original preserved".
- A puzzle checkpoint (flow 5) is dropped like any other flow's checkpoint. If that hero is the warrior, the next start enters the guide stage; another class enters the town through the existing exemption.
- What was saved (level, XP, equipment, gold, edict unlocks, content unlocks) stays. What was only computed from puzzle progress (repeat-hunt settings opened by clearing level 13, the trial scroll page count and level replays) is gone, as the user approved.
- Check: `SkillTreeD4TutorialKitTests.RetiredPuzzleSavesStillLoad` loads the three archived real after-L8 puzzle saves through the real read path. It then confirms that a save carrying a puzzle kit has the kit released and the points refunded, and that the warrior can start the guide stage while the other classes are exempted.

## Verification

Verification results are recorded on the PR.
