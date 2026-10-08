# Waiting-room tour, level 2 start card and live graphs

Updated: 2026-10-08 · Korean: [대기실 첫 방문 안내, 2레벨 시작 카드, 실시간 그래프](Tutorial_Hub_Tour_And_Graphs_20261008.md)

Follows [Tutorial opening, new-skill guide and flow audit](Tutorial_Opening_20261008.en.md) (PR #80). This record covers the requests made after it (1-3: start card, waiting-room tour and live graphs; 4-6: level 2 speech and edict guide, lost-card button, monster models).

## 1. The buttons of the level 2 start card

**Seen.** On the start card of a level with a skill to learn (level 2) the centre button read "Try again" before the level had started, and pressing it went out to the waiting room.

**Cause.** When PR #80 changed `puzzle-play`'s condition from `!terminal` to `!terminal&&!learn`, the `else` that belonged to that `if` (the lost card's "Try again") attached to the "a skill is still to be learned" state. The smoke only pressed `puzzle-edict`, so it did not catch it.

**Now.** While a skill is still to be learned before the level, only "Learn the new skill" can be pressed. "Challenge" and "Show hint" are shown but locked. Once every skill of the level is learned and slotted (two at level 2) "Challenge" switches on. "Try again" appears only on the card of a lost run. The smoke checks this state on the level 2 card.

## 2. The waiting room's first visit

The first time the hero reaches the waiting room after level 1, the commander guides them (`PuzzleTutorial.HubTourPending`: level 1 cleared, current level 2, not given yet).

1. A dialogue: this is the waiting room, only after proving yourself enough can you go outside, first let me show what you can use.
2. The gate points at the inventory. Opening it brings a dialogue (a sword, bow or staff only for now, but better gear will come through the trials); after confirming, the gate points at the close button.
3. The gate points at the Hunt Edict. Opening it brings a dialogue (check and set your tactics, they must change with the enemies' attacks, for now only skills can be changed but experience will be written here, keep watching); after confirming, the gate points at close.
4. A dialogue: not much left to prepare, straight on to the next test. Confirming starts level 2 by itself (the same path as the hub's start button, `PlayPuzzleLevel`).

The tour is recorded once in the save (`PuzzleProgress.hubTourDone`; a save without the field reads as not given). Leaving halfway shows it again from the start at the next waiting-room entry. It does not appear on replays, in the graduated waiting room, or at any level other than 2. A repaint (`ShowPuzzleHub`) does not close the tour's dialogue or gate.

## 3. Live graphs from level 2

From level 2 on (replays included) the battle screen shows two panels. Level 1 has none.

- **Live DPS graph:** the existing panel from rifts and the training ground (`Training DPS panel`). A damage-per-second line, with a skill icon at the time of every cast of a skill that has a cooldown. The tutorial has no previous run to compare with, so the comparison line is left empty. For this, runs from level 2 on also record DPS samples (`RunState.dps`, in `CombatSimulation`).
- **Damage-taken graph:** a new panel under it (`Live incoming panel`, a child of the DPS panel, so dragging or folding it carries this one along). The damage taken so far, summed per monster kind, highest first, at most five bars (the boss counts as one kind, summed over its attacks; damage that cannot be pinned on a monster, such as floor effects, is one `Other · unknown source` bar). It reads the battle statistics' existing `incomingSources` (damage after defence, shield absorption included) through `CombatStatistics.TopIncomingLive`. It refreshes once a second and only changes its rows when the ranking changes. Its rows are built once and reused.

Performance was not measured (there is no before and after at the same conditions for adding two panels to the tutorial battle screen from level 2).

## 4. Level 2: the opening speech and a forced Hunt Edict guide

**Request.** Replace level 2's commander speech with "Focus and you can read the enemy's attacks … do exactly as I say in the Hunt Edict and you will win easily", and force a guide through the Hunt Edict as far as the big-attack avoidance setting.

- **Speech.** Level 2's `scout` is now three paragraphs (read the attacks, this foe hits hard but winds up slowly, do as the edict says) shown as three lines (paragraphs are split on `\n` and the card raises each as a `StoryLine`; English exists for each paragraph in the string table).
- **No more losing first.** Level 2's avoidance options used to open only after a first loss (`openAfterLoss`). They are open from the start now (`openAfterLoss:false`) and the player edits the edict without losing. Level 7 keeps its first-loss design.
- **Guide.** A level with `guideAnswer` in its data (`PuzzleLevelDef.guideAnswer`, level 2 only) has the same guide continue after the new skills, leading the player to pick the commander's answer by hand (`PuzzleTutorial.GuidedPicks`: the class answer's global quick presets, in order). At level 2: Survival tab → "Avoidance by damage type" → the quick-preset picker → "Avoid all warnings" → (warrior only) "Emergencies & re-engagement" → "Balanced survival" → Save → Close. The step is read from the open edict draft, so a repaint continues it (`GameUI.PuzzleAnswerStep`).
- **Card.** While a skill is still to be learned only "Learn the new skill" can be pressed; once the skills are done but the answer is not set, only "Hunt Edict setup"; "Challenge" and "See hint" stay locked. When both are done (`PuzzleTutorial.GuidedDone`, read from the saved edict) the challenge switches on. Replays have no guide.
- **Which answer.** The hints and the level's tests use "Avoid all warnings" as the answer, so that is what the guide asks for. "Avoid heavy damage" (only attacks expected to cost 10% HP or more) also exists but is not the level's answer.

## 5. The last button of a lost card

On a lost card (`terminal`, not a win), after the hints have run out (the stage where "Prepare with the commander's settings" used to appear; forge levels after four hints) the third button becomes "Leave for the waiting room" (`puzzle-leave`) and leaves for the waiting room (in a replay it calls `FinishTutorial`; a non-mandatory replay reads "Leave for the town"). Preparing with the commander's settings is still on the start card of the same stage. The smoke checks it on level 7's lost card.

## 6. The level 2 monsters that were not drawn

**Seen.** On levels 1 and 2 the Grave Hound (N02) bodies were not drawn; only their health bars and shadows showed. The models are not missing: N02 (`HunyuanReady/Characters/N02`) and the Sandworm (N14) both exist.

**Cause.** The hound is a three-LOD prefab whose last LOD has `screenRelativeHeight` 0.1 (below it everything is hidden). The game camera is orthographic and shows about 26 m of height, so a 1.3 m hound is 5% of the screen height and was culled. In the smoke diagnosis every LOD renderer had `isVisible` false (`rel=0.050`).

**Now.** `HunyuanReadyArt.Spawn` sets the last LOD's cull height to 0 when it builds a model (no culling; the last LOD keeps drawing). The same path builds the other Hunyuan models (N12, N13, BOSS03). The smoke reads whether the body models on screen were really drawn in the level 1-3 fights (`CheckBodiesDrawn`).

## Verification

- **Full Edit Mode (final code, one string-table entry added afterwards):** 5,679 tests: 5,673 passed, 4 skipped, 2 failed. (1) `LocalizationTests.EveryKoreanLiteralInTheRuntimeHasAnEntry`: `'{0}'을(를) 택하라.` in `GameUI.PuzzleOpening.cs` had no English; the entry was added. (2) `CoreTests.GeneratedDungeonContainsEnoughPointsAndUniqueEnemyIds(6,150)`: 151 instead of 150. Dungeon generation was not touched and its seed comes from the account's GUID, so this reads as a randomly failing check. The related checks (`LocalizationTests`, `CoreTests`, `PuzzleTutorialCoreTests`, `PuzzleOpeningTests`) were run again and passed 78/78. The full suite was not rerun.
- **UI contract and refresh checks:** `check_ui_contract.py --verify` (15 tests) and `check_ui_refresh.py` (14 bindings) pass.
- **macOS development-build tutorial smoke (real pointer input):** warrior · Korean · 440×956 (pass, 67 PASS lines) and ranger · English · 1280×720 (pass, 68 PASS lines). They covered the opening, the waiting-room tour, level 2's new skills plus the edict-answer guide (two steps for the warrior, one for the ranger) with the challenge switching on afterwards, a level 2 win, the leave button on level 7's lost card, the body models actually drawn in the level 1-3 fights (`CheckBodiesDrawn`), and the live DPS and damage-taken panels from level 2. Both smokes ran on the final builds (h9/h11); one string-table entry (`'{0}'을(를) 택하라.`, a fallback line that never shows on screen) was added after them.
- **Not verified:** the mage on screen, physical devices, performance (no before/after for the two live panels or the LOD change; the hound now draws its last LOD), the `-hellscriptTutorialProfiles` smoke (the hub profiles moved to level 3 and were not switched on in this run), whether "Avoid heavy damage" also clears level 2, and the live DPS graph's skill marks follow the rifts' rule (cooldown of 3 s or more, or the ultimate; level 2's first skill gets no mark).
