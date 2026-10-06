# The Final Trial — the Pit Tutorial

Updated: 2026-10-06

[한국어](Pit_Trial_Tutorial.md)

The mandatory first-run tutorial (P00) is rebuilt from the "Voice of the Edict" prologue into **the apprentice warrior's final trial**. An apprentice who has finished training takes the last test before becoming a full warrior, in a walled pit. This milestone ends when the second stage is retried and won; after that the existing reward and town arrival (Anton Jindark's first conversation) follow. Code for the earlier prologue (checkpoints `tutorial-v2`, `tutorial-v3`) remains in the project but the new checkpoint `tutorial-v4` does not use it.

## Flow

| Step | State (`ProloguePhase`) | What happens |
| --- | --- | --- |
| 1 | `Surrounded` (paused) | The commander announces the final trial. Each class has its own apprentice backstory, followed by the hero steeling themselves. |
| 2 | `EdictLesson` (paused) | The Hunt Edict opens. A forced lesson learns the first skill (Warrior W01, Ranger A01, Mage M01), equips it in slot 1 and saves. |
| 3 | `Encircled` | Stage 1. The hero walks to the middle of the pit, stops, and attacks only from there. Four monsters arrive together and fall. |
| 4 | `StageTwoIntro` (paused) | The commander announces the next opponent. |
| 5 | `StageTwo` | Same pit, same spot. Three fast, charging monsters arrive together. The hero walks to the middle again, attacks only from there and eventually falls. |
| 6 | `Fallen` (paused) | The commander speaks about movement and the Hunt Edict opens movement (notice "New Feature Unlocked"). |
| 7 | `MovementLesson` (paused) | A forced lesson: Hunt Edict → Combat tab → Default combat position, choose any movement except standing still, then save. The pit is empty and the hero is back on their feet. |
| 8 | `StageTwoRetry` | Stage 2 again. Fighting with the saved movement, the hero defeats the charging monster. A fall here restarts the stage. |
| 9 | `Cleared` | Victory staging, 1,000 gold, arrival in town. |

## The pit and its rules

- **The pit**: one round room 30 m across. A 64-sided floor outline (`outline`) defines both the walkable area and the wall. The hero starts at (−8.5, −8.5), the monster pack near (7, 7) (`CombatSimulation.PitHero`, `PitFoe`).
- **Standing still**: during stages 1 and 2 `MoveHero` only walks to the pit centre (0, 0) and then stays (`PitRooted`). The starting edict also saves `position.mode` as Stand (`GameStore.ProloguePowers`) so the movement lesson changes a real value.
- **The monster knows where the hero is from the start**: enemy sight is 14 m, so an opponent across the pit would never move. Inside the pit its last-seen position is refreshed every tick.
- **Pit sight**: in the tutorial the sight radius is 40 m, so the whole pit is visible from the start (`RiftVisibility`).
- **Stage 1**: four ordinary monsters (`N01`). Health 90 each, attack 3, speed 2.4.
- **Stage 2**: three chargers (`N02`, role Charger). Health 60 each, attack 12, speed 5.5. On the first attempt its attack grows with the time spent standing (+15% per second) and each one's health never drops below half, so no class can win standing still and the hero falls within a fixed time.
- **Movement unlock**: `GameStore.UnlockPitMovement` adds `position.mode` to the disclosed list. Stand is removed from the choices. The lesson's save check passes only when `position.mode` is something other than Stand.
- **Restart**: closing the app resumes from the checkpoint (`guide.tutorialRun`). Checkpoints from the earlier prologue are discarded and the trial starts fresh.

## Balance (Edit Mode, `TutorialPitTests`)

| Class | Stage 1 cleared | Falls standing | Stage 2 won, by movement |
| --- | --- | --- | --- |
| Warrior | 11 s | 45 s | Circle, pack center, edge 52 s; keep distance 84 s |
| Ranger | 8 s | 33 s | All four 37 s |
| Mage | 8 s | 32 s | All four 38 s |

Times are simulation seconds from the start of the run. All three classes win with each of the four movement modes.

## Staging assets

| Asset | Location | Note |
| --- | --- | --- |
| Commander portrait | `Resources/Art/NpcPortraits/pit-commander(.png/-mirror.png)` | Codex built-in image generation, model unknown. Request and record in `Docs/Art/PitTrial`. |
| Class backstories | `GameUI.TutorialPit.cs` | Warrior: a Wall Village blacksmith's son who lost his brother and carries his blade. Ranger: last of a forest hunting clan that lost its forest. Mage: a tower apprentice who lost their master reading a forbidden book. |

## Validation

- Edit Mode `TutorialPitTests`: for all three classes, stage 1 is won standing, stage 2 is lost standing and won with each of four movement modes, and the reward is paid once.
- Not checked on a physical mobile device.
