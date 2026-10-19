# Guide tutorial rework, second pass — combat by the real rules, the gear lessons, wording and staging (2026-10-19)

[한국어](Guide_Tutorial_Real_Rules_20261019.md). After playing the previous rework ([zone and arrow dodging, the level-up fireworks, the boss and Whirlwind](Guide_Tutorial_Hazard_Yard_20261019.en.md)) nine points came back, together with the principle that "the only things hard-coded in the tutorial are the order, the guidance and the Hunt Edict settings". The rule changes below apply to the whole game, not just the tutorial.

## Principle: combat follows the real rules in the tutorial too

The tutorial decides **the order (what is said at which step and when it ends)**, **the guidance (lines, the gate, highlights)** and **the Hunt Edict settings (starting values and the values a lesson asks for)**. Everything below was removed.

| Removed | Why |
| --- | --- |
| Speed 0 for the zone mage and archers, fixed cast intervals and zone time | The hero that only dodged is solved by a rule (change 1 below) |
| HP floors that kept enemies alive until their lesson (mage, archers, the surrounding pack, the boss's pack) and the boss HP floor | Lessons open on progress conditions only: time and events |
| The maximum-range hook pull and 0.4 s retreat, the retreating archer, the formation movement of the front swarm | Hero and enemy movement come from the ordinary AI |
| The scripted skill choice that forced Spinning Rend and the standing-still front fight | The Hunt Edict chooses the attack |
| Enemies that always know where the hero is, the clearing of zones and arrows after a lesson and the forced early cast | Ordinary sight and casting rules apply |
| The treasure goblin's own flight (5 m, toward the boss) and the boss that followed without attacking and could not be hurt | The ordinary flight AI and the ordinary boss AI are used |

Kept: ① the floor that stops the hero from reaching 0 (HP 1) stays as the tutorial's safety net. ② The goblin cannot flee away for good (so progress cannot stall). ③ Enemy health and attack are **stats** tuned to the numbers the lessons mention (7% per second for the zone, 5% for the arrow); they change no rule.

## Real rule changes (the whole game)

1. **No standing still after a dodge.** A hero who reaches safety no longer waits for the end of the attack. He attacks what is in reach and, if it is far, closes in, but **never steps into the warned attack** (a blocked step slides along the edge and, if that gets him no closer, he holds). Leaps and teleports that would leave the safe spot are not used while dodging, and he attacks nothing while still walking to safety. (`MoveHero`/`KeepDodgeStepClear` in `CombatSimulation.cs`; the "in-range targets only" restriction was removed from target choice)
2. **Basic attacks when the resource is short.** Whirlwind started on one tick of resource and ended in a blink, so the hero looped between starting and stopping. It now starts only when it can **run for a second (tick cost × ticks per second)** and until then he uses basic attacks. A spin that runs dry ends normally (`Whirlwind ran out of resource`), not as an interruption. (`WhirlwindStartCost`)
3. **A started attack is finished.** The tutorial's baseline edict sets `dodge.interrupt=FINISH` (dodge after the current attack), so a dodge never cuts a swing short. Nothing interrupts a running skill because another one became ready (external control such as the boss's pull is the only exception).
4. **Pack centre.** The diagnosis showed that the real `pack centre` rule goes into the pack (0.6–1.9 m from its centre, 3–4 enemies around). The earlier "edge-fighting" look came from the always-aware enemies and the fixed HP. The rule was not changed.
5. **Ordinary flight for the goblin, the boss keeps attacking.** The goblin flees to the room farthest from the hero (opposite the boss as well). The boss chases and hits as usual throughout.
6. **Basic reach rounding.** When an enemy stops exactly at the 2.0 m reach, float arithmetic leaves it at 2.0000x: the edict said "out of reach" while the actual hit said "in reach". `BasicReachTolerance (0.0001)` makes them agree (this is why an enemy pulled by the hook could not be hit).

## The nine points and what was done

| # | Point | Done |
| --- | --- | --- |
| 1 | The zone and arrow lines said "7% / 5% of your HP", which felt cheap | No damage numbers; the lines say "Standing still and taking hits is dangerous. Let's dodge so nothing lands!". Only the values to set (7% or less, 5% or less) stay in the instruction |
| 2 | The hero stood still after dodging an arrow | Real rule 1 |
| 3 | The level-up text was poor and overlapped the button | One short line (`HP·MP restored  /  Skill point +1`); ribbon, level, line and confirm button are stacked top to bottom and sized to the window height (portrait and landscape) |
| 4 | A navy frame appeared during the priority setting | See "The navy frame" below |
| 5 | Only the up/down buttons were highlighted, so the item was unclear | The **card** is highlighted and the line says "Drag the card to the very top". A dotted arrow and a finger repeat the drag from the card to its place (`TutorialDragHint`). The ▲▼ on the card still work |
| 6 | The hero did not seem to fight in the pack centre | Real rule 4 (the rule already fights at the centre) |
| 7 | The hero waited for Whirlwind without mana / skills were cut mid-way | Real rules 2 and 3 |
| 8 | The goblin ran toward the boss and the boss waited | Real rule 5 |
| 9 | The recommended-gear lesson with the goblin's loot was missing | See "The gear lessons" (the [plan](Guide_Tutorial_Segment4_Plan_20261013.en.md), implemented) |

## The gear lessons (the plan, implemented)

Phases are appended at the end of the numbering: `HelmetTalk=46`, `RareTalk=47`, `MagicTalk=48`, `BossLoot=49` (`Last=49`). Earlier checkpoints stay valid.

- **Fixed loot**: gear is never rolled. `GuideTutorial.Gear` always makes the same item from the hero id and kind. The treasure goblin leaves **five gold piles (200–400 each, random) + three enhancement-stone piles (3, 3 and 4) + a magic helmet**; the boss leaves **a rare greatsword + a magic greatsword**. Scores: starting weapon 100 → helmet 149.9 → rare 176.8 → magic 149.8 (once the rare one is worn the magic sword's card is negative). Both swords are two-handed so a new one always replaces the worn one and a difference is shown (a one-handed sword would simply take the free hand).
- **Start settings**: the stage starts with the recommendation rule set to "every wearable piece" and no timer. A restart clears the previous attempt's fixed gear from the bag.
- **Helmet (`HelmetTalk`)**: it stops when the helmet's card first appears during pickup → card icon → `Equip` in the detail window (the magnifier opens the same window) → once worn the rest of the pickup continues.
- **After the boss (`BossLoot`)**: the boss's death no longer ends the stage; two swords drop (the rare one nearer the hero). **Rare card (`RareTalk`)** → `Equip` → the **magic card (`MagicTalk`)** that waited behind it (same slot) shows with a minus → the card's gear icon → `Gear with a higher score than what is worn` → `Save` → explanation → close the card with × → graduation (`Cleared`).
- **Saving and sessions**: the cards also show on the guide run. The offer decision is made on the staged copy, so it is carried onto the guide run's live drops (`RiftGearRecommendations.Sync`). Wearing gear and saving settings swap in a staged account, but the live run the simulation holds is kept. If an older save never shows a card, that lesson is skipped.

## The navy frame

On every beat where she speaks the guide tutorial lays a pale blue edge wash over the screen (a red one on the danger lessons) to give the feel of time standing still (`cinematic.Vignette` in `GameUI.GuideTutorial.cs`). When a Hunt Edict sheet opens the world shrinks into a small window at the top and the rest of the screen is black, but the wash covers the whole screen and painted that black navy (dark red on the danger lessons). That was the "navy frame that appears after setting".

First guess was that the world camera left that area unpainted, and a black helper camera was added; it changed nothing. Dumping the cameras and the full-screen UI in the player, and turning the world camera's background red, showed that the camera paints only its own window and the navy came from the wash. The helper camera change was reverted, and the wash is switched off while the world is shrunk into the sheet window (`SettingsWorldOpen`). Closing the sheet brings it back at full screen as before. The smoke checks on every sheet lesson step that the wash is off.

## Verification

Scope is the area this change touches. The full Edit Mode run was not used.

- **Combat and Hunt Edict rules batch** (1023 tests, right after the rule changes): 1020 passed, 3 failed. The failures were tests that expected the old rules (`StationaryDodgeAttacksAnotherReachableEnemyAndDoesNotChaseTheOldTarget`, `ResourceFailureStopsBeforeTheUnfundedTick`, `AnAffordableEarlierAttackPreventsUnnecessaryRecoveryForALaterOne`) and were updated to the new rules. `Runtime/Core` has not changed since.
- **Guide, localization and Hunt Edict rules batch** (382 tests, final sources): 382 passed. It contains the classes of the three tests above (`EdictWarriorAttackTests`, `EdictWarriorSupportTests`), `GuideTutorialTests` (a full run through the reward, helmet, rare and magic lessons), `GuideGearTests` (the fixed gear score relations), `GuideHazardYardTests`, and `LocalizationTests` / `StoredLocalizationTests` (new lines in both languages, both directions).
- **UI contract**: `python3 tools/check_ui_contract.py --verify` passed (15 tests).
- **macOS development player smoke** (offline, Korean 440×956 and English 956×440): both languages ran the opening → hook and Spinning Rend lessons → zone and arrow dodging → level-up (`HP·MP restored / Skill point +1`, not overlapping the confirm button) → boss and Whirlwind → the priority and treasure-goblin card drags → the helmet, rare sword and magic sword lessons with the settings save → graduation → town. On every sheet lesson step the smoke checks that the edge wash is off, and the area outside the world window was black (0,0,0) (navy 29,42,68 / dark red 80,19,15 on the danger lessons before the fix).
- **Existing failure (unrelated to this change)**: the smoke's last `CheckHudButtons("town")` fails with "Pointer blocked at 사냥 칙령". The same failure occurs on `origin/main`.

Not verified: a real device (iPhone), performance, public wiki publishing. This is Mac development-player and Editor evidence.

## Good to know

- The only combat adjustments left in the tutorial are the hero's HP-1 floor, the block that keeps the goblin from escaping for good, and the enemies' health and attack numbers. The boss no longer survives because of an HP floor.
- Point 6 (pack centre) changed no rule. The diagnosis shows the real rule already fights inside the pack (0.6–1.9 m from its centre).
- Screenshots: `Builds/Validation/GuideRealRules/smoke-ko` and `smoke-en` (local evidence, not committed).
