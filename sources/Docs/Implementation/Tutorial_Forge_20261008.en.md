# Tutorial forge and the removed summary tab

> 2026-10-08 (PR #77): Maintenance puzzle P2 (the +1 enhancement with 110 trial gold) is replaced by the equipment-enhancement level L14 and gone; gear enhancement also opens after the first loss at L14, through the same `ForgeOpen` gate. Three faces of danger and graduation became L15 and L16. "L15" in the table below means the last level (`PuzzleTutorial.Count`). See [Native monster patterns](Puzzle_Tutorial_Native_Patterns.en.md).

Updated: 2026-10-08 · [한국어](Tutorial_Forge_20261008.md)

One PR answers three requests. (1) Remove the `Summary` tab from the Hunt Edict window in the tutorial and fix what was connected to it. (2) Decide the order in which the blacksmith's services appear in the tutorial, simplest first. (3) Hand out the materials for enhancement as tutorial rewards. The earlier record is [Tutorial hub follow-up and per-hero tutorials](Tutorial_Hub_Followup_20261008.en.md).

## 1. The tutorial's Hunt Edict window has no summary tab

A tutorial account's window shows only `Skills · Combat · Survival` (then loot, bag and exploration as they open). The combat style cards the summary used to hold (aggressive, balanced, careful, `edict-style-*`) now sit at the top of the first group of the `Combat` tab (combat position and distance) from L4, under a "Combat style" heading, followed by the group's own title and settings. The summary's other content (the teaser for the next sentence) was already hidden in the tutorial, so there is nothing else to move.

- `HuntEdictWindow.TabDisclosed`: in a puzzle lesson only `Skills`, `Combat` once the styles open, and tabs that hold an open settings group show. `FirstTab` has no summary branch: `Skills` until the level's skills are learned, then the first rules tab. Opening the window after learning the L4 skills lands on the style cards.
- `HuntEdictWindow.DrawSummaryEditor`: on the `Combat` tab of a puzzle lesson, with the first group selected and the styles open, it draws the cards with `DrawStyles`. An ordinary account's window (after the tutorial) keeps its `Summary` tab.
- Smokes: the hub menu check requires that `edict-tab-overview` is absent. At L4, `PuzzleDirtyNavigation` and `PuzzleSaveFailure` press `edict-tab-combat`, confirm the cards are on the combat tab, then press `edict-style-careful`.

## 2. The order of the forge services and their materials

The forge has four services (option change, equipment slot upgrades, gear enhancement, core crafting). The tutorial opens only the two "enhancement" services, simplest first.

| Order | Opens when | Service | Materials the clearing level pays | Why this place |
|---|---|---|---|---|
| 1 | Level 8 cleared | Equipment slot upgrades (`forge-tab-1`) | 20 enhancement stones | With stones, one press on a slot starts it: the simplest. 20 is two stations' worth of a level 1 → 2 upgrade (10 each). |
| 2 | Level 15 cleared | Gear enhancement (`forge-tab-2`) | 600 gold | It needs a judgement about which item gets the gold (maintenance puzzle P2 before level 12 teaches that). At item level 1, +1 to +4 cost 110+116+123+129 = 478 gold and +8 to +10 on the +7 weapon that level 12 hands over cost 162+172+182 = 516, so 600 covers three or four steps either way. |
| — | Not opened | Option change, core crafting | None | Both need rift gold and cores and equipment with affixes. The rift unlocks (stages 35 and 50) keep them. |

Gear enhancement waits for level 15 for two reasons. P2 starts only while the starting weapon and armor are +0 (`PreparePuzzleMaintenance`), and levels 12 to 15 are tuned for the weapon that level 12 hands over (+7), so an enhancement in between would change what they teach. The measurement in section 4 is the evidence.

## 3. The opening is never saved

A service opens because `ContentUnlocks.Has` also asks `PuzzleTutorial.ForgeOpen` (has that level been cleared, and is the tutorial still running). Nothing is written to the account's `contentUnlocks.unlocked`.

- Each hero's tutorial progresses on its own, so the opening is per hero (another hero is sealed again at L1).
- When the tutorial ends (`mapComplete`) the rift unlock rules return (gear enhancement at stage 1, slot upgrades at stage 5). A slot upgrade already started keeps running and settles (job settlement runs every second whatever the unlock).
- So the stage text does not mislead inside the tutorial, a locked tab's hint (`ContentUnlocks.Condition(account, id)`) says "Opens when you clear tutorial level N". Services the tutorial does not open keep the rift condition text.
- A forge enhancement is recorded through `Tutorials.ObserveTransaction` as the guide having been tried, so the same guide does not appear again after graduation.

## 4. Slot upgrades count in combat after graduation

Puzzle levels are tuned for the hero the tutorial hands out and their results are sensitive to the hero's numbers. During testing, a hero with two attack slots (main hand, hands) at level 2 and the equipped weapon's enhancement moved from +7 to +4, so that the total attack was almost the same (about +1), ran the L14 answer. Only the Warrior, who clears in 71.7 s with the standard hero, failed to finish within 90 s and lost to the time limit (one enemy left at 34/450 HP). The other classes and the defence-side combination passed. A small difference can lose the answer on some levels, so no test can cover every slot and gear combination a player may choose. The level side is fixed instead.

- Puzzle combat (`CombatSimulation.IsPuzzle`) reads every slot level as 1 (`State.slotLevels` is fixed when the run starts and all four stat rebuilds use it).
- So the same hero numbers show on screen, the hub's HP and resource bars and a result's HP share use `PuzzleTutorial.BaselineHero` (the hero copied with slot levels 1).
- The levels the hero bought are saved as they are, show in the inventory and character info, and count in combat after graduation. The level 8 result card says so in one line.
- Gear enhancement opens only after the last level, so it cannot touch a puzzle and is not pinned.

## 5. The materials are paid once, inside the level's winning transaction

`PuzzleTutorial.GrantForge` adds the materials inside the transaction in which `RecordPuzzleAttempt` records the level's win. The receipt (`puzzle-result:<run id>`) is shared with the XP, so it pays once and resending the same result pays nothing more. At the limit (`int.MaxValue`) it adds up to the limit instead of blocking. Each hero's tutorial is separate, so each hero is paid once. The table is in one place, `PuzzleTutorial.ForgeSteps`.

## 6. On screen

- Levels 8 and 15 gain a line on their clear card. Level 8: "Forge opened · Slot upgrades · Granted 20 enhancement stones" and the sentence about when it counts. Level 15: "Forge opened · Equipment enhancement · Granted 600 gold" (`PuzzleTutorial.ForgeNote`, one line in `GameUI.ShowPuzzleCard`).
- Pressing a locked service tab shows "Unlock requirement: Opens when you clear tutorial level N" for two seconds (`BlacksmithWindow.DrawServiceHint`).
- The three new lines are at the end of `en.txt` with their English text.
- The hub's inventory (`InventoryWindow`) has no forge shortcut, so there is nothing to route. The "Enhance at the forge" button of the old inventory layout (`GameUI.InventoryLayout`) opens only in town.

## Verification

- Edit Mode, whole suite once on the final code (on top of the merge target `main` 72823daf): 5,655 of 5,657 passed, 0 failed, 2 skipped (`MeasureEveryDesignBuild` and `Capture`, which always skip), about 50 minutes.
- New tests: 11 in `PuzzleForgeTests` and 3 `TheFinalTrialPaysTheEnhancementGoldOnceAndOnlyThenOpensGearEnhancement`: the opening order and each service opening only after its level without saving an unlock; the fall-back to rift unlocks after graduation (stages 1 and 5); the material payments and the limit; the locked tab's text; the clear card's line; a real level 8 win paying 20 stones once and a slot upgrade starting through the forge's own store path; the real final-trial win paying 600 gold once and gear enhancement then working through the same path; and slot levels not changing the puzzle hero's numbers, nor those after a saved edict change (three classes).
- The existing puzzle suites (L1 to L15 answers and wrong answers, maintenance P1 and P2, graduation) all passed unchanged, which is the evidence that the hero baseline did not move.
- `check_ui_contract.py --verify` 15 tests and `check_ui_refresh.py` 14 bindings pass.
- macOS development player (real pointer input): the whole tutorial smoke (L1 to graduation) passed to the end for the Warrior in Korean at 1600×900 (including the hub layout profiles), the Ranger in English at 956×440 and the Mage in Korean at 440×956. The new scenario presses the forge's slot upgrade right after L8 (including the locked gear tab's hint text and the real start button) and gear enhancement just before graduation (the real +1 button), and checks that the graduation receipts and the graduation gold are intact. The screens of the same run (the combat style cards on the `Combat` tab, the slot upgrade and gear enhancement screens) were looked at in all three sizes.

### Not verified

- Nothing was seen on a physical phone or tablet. The screens were checked only at the macOS development player's window sizes.
- No person saw the screen five minutes after a slot upgrade finishes, or slot upgrades taking effect in combat after graduation. The rule is covered by unit tests.
- Switching to another hero and doing that hero's tutorial, with the forge opening for each hero separately, was checked only by unit tests (`ForgeOpen` reads only the selected hero's progress), not in the real screen flow.
- No new sound file was made and no listening test was done.
- The content dock and battle escape smokes, which already failed on the baseline build, were not run this time.

## UI changes to pass on to whoever writes the tutorial lines

1. The Hunt Edict window has no `Summary` (`edict-tab-overview`). The combat style cards (`edict-style-*`) are at the top of the first group of the `Combat` tab (`edict-tab-combat`) from L4. Press `edict-tab-combat` before a card.
2. In the bottom menu, `Blacksmith` (`hub-menu-3`) opens `Equipment slot upgrades` (`forge-tab-1`, start `forge-slot-start`) after L8 and `Gear enhancement` (`forge-tab-2`, +1 enhancement `forge-upgrade-0`) after L15. Option change and core crafting stay locked.
3. The L8 and L15 clear cards gain one line about the forge opening and what was handed out (the 1,000 gold graduation reward is unchanged).
4. Three new `en.txt` lines were added at the end, and one line (`ForgeNote`) in `ShowPuzzleCard`. If #77 edited the same function, keep just this one line when merging.
