# Guide tutorial — walk-back, ranged pack and treasure-goblin lesson, fixed again (2026-10-22)

[한국어](Guide_Tutorial_Route_Loot_20261022.md). Covers reports 1 (the hero walks back along the road), 5 (takes the helmet at once, takes the gold long after), 6 (the loot lesson does not start 2 s after the goblin falls) and 7 (the ranged monsters still appear one by one) from the user's playtest of main `c6354cbf`. The earlier fixes (2026-10-20/21: `RoadStart`, `GuideForward`, `LootLessonSeconds`) were hypotheses and the symptoms came back on the user's screen. This time a tool that records the real playthrough tick by tick came first; the causes were confirmed from its records and then fixed.

## Deterministic reproduction tool

- `GuideRouteTrace` (test code) reads **every tick** of the real stage playthrough (`GuideHazardYardTests.PlayToTheHazardYard` → … → `GuideTutorialTests.PlayBossRoomToGraduation`): hero position, phase, `guideWaypoint`, action text, movement-reason trigger, target id, each of the three ranged foes' position / health / sight / action stage, pack spread, floor reward counts (open / taken), nearest reward distance and boss position. The read hook is `GuideTutorialTests.Probe`; it does not change the simulation.
- Progress (`Along`) is measured on the road legs **near the farthest point already reached**, not on the nearest point of the whole road, so a walk back cannot read as a short hop. Walk-back = maximum progress reached − current progress.
- With `HELLSCRIPT_GUIDE_TRACE_DIR` set, two tests save the tick records as CSV (`gallery.csv`, `whole-stage.csv`). Same seed (`GuideTutorial.Seed`) and 0.05 s ticks, so the result is identical every run.

## Measurements (same playthrough, before → after)

| Item | Before (main `c6354cbf` code) | After |
| --- | --- | --- |
| Largest walk back along the road in the gallery | **15.9 m** (`Approach`, right after the position lesson) | 0.36 m |
| `Approach` duration | 19.9 s | 3.5 s |
| Largest distance between the three ranged foes (position lessons to the fight) | **16.8 m** (one foe walked out alone) | 2.7 m |
| Closest hero distance to the pack centre (`FinalFight`) | 1.6 m | 1.7 m (the existing "pack centre" edict behaviour, unchanged) |
| Where the reward falls (the goblin led the hero out of the boss's arena) | Under the hero's feet (nearest piece 0.0 m, middle of the reward 0.0 m) | Behind the hero (nearest piece 6.4 m when the lesson opens) |
| Drop → lesson | +2.00 s | +2.00 s |
| Hero between drop and lesson | Stands still (distance to the boss 11.9 → 8.5 m: the boss walked up) | Walks toward the boss (11.9 → 4.8 m) |
| `Pickup` start → `LootDoneTalk` | **55.5 s**, 161.8 m walked (a tour of the map for the last two gold piles) | 6.8 s, 14.5 m |
| `FinalBoss` → boss down | 7.0 s | 5.2 s |

Instead of video there is a top-down **simulation replay** of the same CSV ([gallery](GuideRouteEvidence20261022/gallery-replay.mp4), [goblin → reward](GuideRouteEvidence20261022/treasure-replay.mp4); before on the left, after on the right, simulation time at 1×, time log on screen). It is not a recording of the game screen. Raw data: `GuideRouteEvidence20261022/*.csv.gz`.

## Causes (confirmed by reproduction)

1. **Gallery walk-back and split pack (reports 1, 7).** `Road` opens the lesson when a ranged foe is within 12.5 m, but the hero's sense range (`SenseRange`) is 12 m, so in the first moment of `Approach`, after the position lesson, the hero had no target. `Approach` had neither road movement nor `GuideForward`, so the ordinary search chose an "unexplored spot" behind on the road (toward the map start) and the hero walked back about 16 m. Meanwhile each of the three gallery archers woke only when it saw the hero within 14 m, so the one that followed the retreating hero fought alone, 17 m from the other two. Both symptoms are one cause. The spawn coordinates (one knot within 2.7 m) and the individual foe AI's spacing are not the cause.
2. **The loot lesson never starts (report 6).** `ReturnToBoss` returned `false` forever when `State.drops` (the helmet only) was all taken or ignored. `All()` is also true for an empty list, so when the helmet is already in the inventory and none dropped (`DropGuideGear` skips duplicates), or when it was taken or ignored, the lesson never began. Reproduced by unit tests (on the old code the phase stays `ReturnToBoss`). `Pickup` also could not end when `drops.Count==0`.
3. **Reward under the feet, helmet taken at once (part of report 5).** `GuideLootSpot` required candidates to be "within 11 m of the boss", so when the goblin led the hero out of the arena (17 m in the real stage) every candidate failed and the hero's own position was used. The helmet and gold were collected from under his feet the instant `Pickup` opened. The hero in `ReturnToBoss` also did not head for the boss; he stood still.
4. **The last gold long after (part of report 5).** In the ordinary edict, gold is "passing" (`Passing`), so piles scattered beyond the 1.5 m radius are not walked to. The hero collected them only when the ordinary search happened to pass them (reproduced: 55 s, 161.8 m). With the boss following the hero and standing next to him it is worse: in an intermediate experiment during this work the hero did not pick up a helmet 1.1 m away and kept hitting the boss, so `Pickup` never ended. I did not read the helmet's loot setting itself; the "after combat" setting, which does not pick up during a fight, looks like the cause (inference).
5. **The boss could die before the goblin.** The guide had no boss protection. Reproduced by a unit test.

Not established: which combination of these produced 5 and 6 in the user's own play; there is no user log, so it is not asserted. Every cause above was reproduced and fixed. The user's saves and the playtest build were not read or changed.

## What changed (mainly `CombatSimulation.Guide.cs`; the general game rules are untouched)

- **Wake the ranged pack together:** when the position lesson opens (`Road` → `OpeningTalk`) every living ranged foe in the gallery notices the hero (`lastSeen`; the same staging as `ArriveGuideFoe` for the hazard-yard foes). After that movement and attacks are the ordinary foe AI. The hero still fights from the pack centre through the real `position.mode=CENTER` behaviour.
- **Guide movement goal (`GuideSeekGoal`):** in `Approach`, `FinalFight`, `ReturnToBoss` and `FinalBoss`, with no target, the hero heads for the nearest living foe (the boss) instead of the ordinary search. In `Pickup`, when the edict's loot logic finds nothing, he heads for the nearest piece left. Dodging, ordinary combat while there is a target and the goblin chase are unchanged. No global AI pause or difficulty change.
- **Lesson start:** `ReturnToBoss` → `LootTalk` opens once, 2 s of simulation time (`LootLessonSeconds`, tolerance 1e-4 s) after the drop moment (`guideGoblinKilledAt`, now also written by `RewardGuideTreasure` when it lays the reward down; no new save field), whether or not the helmet is on the floor. The rule that nothing can be picked up before it (`Loot`'s `GuideTreasureScene` guard) is unchanged, so "you have not picked up the floor reward yet" is always true. A tick settles the lesson twice (before and after movement), so it opens on the tick that brings the clock to 2 s.
- **Reward position:** without the preferred spot (boss arena, within 11 m) the reward lands 4 m behind the hero even outside the arena; his feet are the last resort.
- **Gathering lesson (`GuideGathering`, guide `Pickup` phase only):** the goblin's reward (helmet, gold, stones) is treated as `Priority` and the danger wait is skipped. Other phases and ordinary rifts keep their loot rules. `Pickup` ends when the reward has been paid and nothing is left on the floor.
- **Boss protection:** from the boss's introduction (`MixedFight`) to `LootDoneTalk` the boss's HP does not drop below 1 (`GuideBossHeld`, `Deal`). From `FinalBoss` on he can die. It is a guide-only protection like the existing hero HP floor.

## Tests

- `GuideHazardYardTests.TheGalleryPackWakesTogetherAndTheHeroNeverWalksBack`: walk-back < 1.5 m, no ordinary-search movement tick in the gallery, pack spread ≤ 4 m, all three archers take an attacking stance, pack centre < 2.5 m, `Approach` < 10 s. The existing `packBackedUp` / `packCentreClosest` checks and `ARoadEnteredHalfWayGoesOnFromWhereTheHeroStands` stay.
- `GuideHazardYardTests.TheTreasureLessonKeepsItsTimingAndTheRewardIsFetchedAtOnce`: drop + 2 s, move toward the boss > 2.5 m, reward outside reach when the lesson opens, nothing taken before it, fetch < 20 s and < 40 m walked, boss alive through `LootDoneTalk`, boss fight < 20 s.
- `GuideTreasureLessonTests` (fixed boss-arena situation): reward not under the feet; +2 s at tick intervals 0.05 / 0.04 / 0.1 / 0.0125 s (neither early nor late); pause, resume, repeated entry and double payment; the lesson opens with the helmet on the floor, taken, ignored or already owned; nothing taken before the lesson even with the hero standing on the reward; move toward the boss; boss protection; `Pickup` ends when the helmet is already owned and only resources were left.
- Run against the old game code the new tests fail 9 of 16 (walk-back 15.9 m, reward under the feet, early boss death, lesson never opening ×3 plus `Pickup` never ending, no move toward the boss, reward beside the hero at the lesson).

Actually run (Unity 6000.6.0f1 batch EditMode, throw-away in-RAM saves via `GameStore.Sandbox` in the development worktree): `GuideTutorialTests`, `GuideHazardYardTests`, `GuideTreasureLessonTests`, `GuideGearTests`, `GuidePotionLessonTests`, `GuideModifierLessonTests`, `ResourceLootTests`, `EdictLootIntegrationTests`: 117 passed, 0 failed, 0 skipped (253 s). `python3 tools/check_ui_contract.py --verify` (15 OK) and `python3 tools/check_ui_refresh.py` (PASS) were run. Full Edit Mode, a player build and the guide smoke (ko 440×956 / en 956×440) were not run, and nothing was verified on a real device or screen. No text changed, so no `en.txt` entry was added (the existing "Heading for the next foe" and "Looting" strings are reused).
