# Tutorial opening, new-skill guide and flow and timing audit

Updated: 2026-10-08 · Korean: [튜토리얼 도입부·새 기술 안내와 진행·타이밍 점검](Tutorial_Opening_20261008.md)

Related records: [Native monster patterns](Puzzle_Tutorial_Native_Patterns.en.md) (level design), [Tutorial forge](Tutorial_Forge_20261008.en.md) (the forge level), [Puzzle tutorial core](Puzzle_Tutorial_Core.en.md) (runner and saves).

## What the user saw

1. Right after the game starts the waiting room shows a guide, "Open the hunt edict and learn your first power on the Skills tab". The commander's story (the hero's past, what this pit is) only comes after the skill is learned and the start button is pressed. Being told to pick a skill with no context felt abrupt.
2. Level 2 has the commander's lines but no guide. Pressing Challenge without learning the new skill looked like the game had stopped.
3. A request to review all tutorial progression and timing again and fix what is wrong.

## Causes

| Symptom | Cause in code |
| --- | --- |
| Guide before the story at level 1 | `BeginTutorial` opened the waiting room at once and `TickPrologueLesson` raised the `hub-menu-1` guide the moment it was open. The commander's card only opened after the hub's start button (`PresentTutorialStep`). |
| No guide from level 2 | The guide condition was fixed to `run.puzzleLevel==1`. Levels 2, 4, 5 and 7 (ranger, mage) hand out a skill but only had one "New skill · …" fact line on the card. |
| Looked stopped | `puzzle-play` closed the card first and, if `ContinueTutorial` refused (a required skill not learned), only showed a toast. The battle screen's `ShowToast` sends a long toast to the HUD brief line only (`GameUI.cs`), so no toast was visible in a tutorial. The card never reopened. |

## New flow

**Level 1 (a new hero).** Choosing the character opens the battle screen (the pit) instead of the waiting room: fade in from darkness, chapter card ("Final trial"), then eight lines between the commander and the hero (the final trial, the closed pit, the hero's past and resolve, "the door opens only for those who finish the trial", "the edict leads the fight · you hold no power now", "I grant you one point"), then "Accept the trial". The edict opens and the guide leads one control at a time (skill → `+` → equip → save → close), then the commander's card ("Good. Whether the power I handed you is real, we learn in the fight. Go.") and Challenge. The waiting room first appears after level 1 is cleared. The opening plays only until the first skill is slotted (`PuzzleTutorial.OpeningPending`), never on replays, resumes or from level 2.

**Level 2 onward.** Waiting room start, then the commander's card. If a skill is not learned or not slotted, the primary button is "Learn the new skill" and there is no Challenge. It starts the same guide (`PuzzleSkillStep`) in the edict: learn each skill (passives need only `+`), equip it, save. On close the card returns with Challenge. Replays are unchanged.

## Audit result

A read-only audit (four segments, each finding re-checked by a skeptic) reported 49 findings: 2 blockers, 15 major, 30 minor (10 verified real, 20 unverified), 2 not a problem. All 17 blockers and majors were confirmed and fixed.

| Problem | Fix |
| --- | --- |
| Challenge silently refused without a required skill, and the card disappeared (blocker) | The card makes "Learn the new skill" primary and hides Challenge. A refusal shows the toast and reopens the card. The HUD pause button opens the card on a waiting tutorial (`TogglePause`). |
| The maintenance puzzle's trial items could be dismantled, making level 10 unstartable (blocker) | The items are locked, and a missing one is given back (`PreparePuzzleMaintenance` is idempotent). The measured starting numbers are kept. |
| After a time-out or goal loss (health left), opening and closing the edict never brought the card back | `TickTutorialUI` treats a puzzle's `Fallen` as a card moment too. |
| Tutorial refusals and errors were invisible as toasts on the battle screen | A tutorial run keeps the toast frame (`ShowToast`). |
| Level 11's default edict (portal when the bag is full) froze in the portal bag screen | When the portal opens the operation level ends as a missed goal on the same tick (`PortalForBag`). An ended run never opens the portal bag. |
| Level 14: spending the trial gold on the weapon made the level unwinnable with no recovery | In the forge lesson only worn armor can be tempered (`ForgeTarget`: the list and `GearEnhancement.Apply`), and the window opens on gear enhancement with the worn armor chosen. |
| A learned but unslotted skill made level 8's skill presets throw, so the level could not start | Challenge checks that every required active skill is slotted on every level. `StartPowers` slots a learned but unslotted kit skill itself. |
| Hints showed raw skill codes such as `W02` or `A04` | Hints are read through `ClassSkillTree.Display`. Two particles fixed. A test checks no code is left. |
| A leftover failure hint ended a winning card · on the 4th loss "I wrote it" appeared before it was written | Hints are not appended to a win card. On edict levels the automatic rise stops at step 3 and step 4 needs the "Prepare with the commander's settings" button (the forge level is unchanged). |
| The level 1 card told the player to learn a skill they had learned, and repeated the backstory | The backstory moved to the opening and the card now reads "Good … Go". |
| First cards of operation levels 9-13 did not say which edict tab | The commander's lines name the tab (Loot, Explore, Bag & Cleanup, Auto Equip, Repeat settings). |
| The edict window's guide line was the commander's but labelled "Voice of the Sky" | `EdictCaption.Speaker` names the commander. |
| The last level's win card said "Next level" and never mentioned the graduation reward | "To the town" and a "Graduation reward · 1,000 gold · 1 rare item" line (paid when going to the town). |
| An operation-level loss added "no damage taken, check attack conditions" | That line is not added for operation failures. |

**Not fixed (deferred).** The maintenance puzzle's entrance cue (start button label, confirmation line), per-cause numbers on operation-loss cards, result card staging (`VictoryBeat`), language-change refresh of the card's fact lines, hints readable before an attempt, level 12's hook versus the forge, level 16's "use everything you learned" versus the level 9-13 edicts, the post-graduation reward staging. All are minor and none blocks progress.

## Verification

- **Full Edit Mode (final code, once):** 5,673 tests, 5,668 passed, 4 skipped (authoring tools), 1 failed. The failure was the new `TheForgeLessonOnlyTempersWornArmor`, whose assumption about the starting gear was wrong. It now equips its own gear and the seven `PuzzleOpeningTests` pass; the full suite was not rerun. New checks: the opening condition, maintenance item lock and re-issue, the forge's worn-armor limit, and no skill codes in hints. The level 7 `unequipped` case now expects a refused start when a required skill is not slotted.
- **UI contract and refresh:** `python3 tools/check_ui_contract.py --verify` 15 tests passed, `python3 tools/check_ui_refresh.py` passed.
- **macOS development-build smoke (real pointer input, disposable save path):** warrior · Korean · 1280×720 and ranger · English · 440×956 both ended with 51 PASS. They cover the new opening (the commander's story on the battle screen, then the guide, then the level card), the new-skill lessons at levels 2, 4 and 5 (and the ranger's A04 at level 7), the forge level's +30 armor, graduation and replay.
- **Not verified:** the mage on screen (the lessons use the same code and class kits), physical mobile, how a person reads and understands it, the deferred minor items.
