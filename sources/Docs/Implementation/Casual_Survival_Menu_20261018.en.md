# Hunt Edict survival sub-menu casual — 2026-10-18

> **2026-10-28 A2 correction:** References to the large edict window, ≡ menu and their dedicated components or smokes are historical. Use the [current entry points](../../Assets/HELLSCRIPT/Runtime/Presentation/GameUI.HuntEdict.cs) and [replacement screens and source owners](Hunt_Edict_Window_Removal_20261028.en.md). Earlier captures and test counts describe the earlier implementation.

[한국어](Casual_Survival_Menu_20261018.md)

Basis: sections g–m of the canvas "HELLSCRIPT 사냥 칙령 · 생존" (RydEW2cMjvHRr2dPXU7H2a). Started from `origin/main` `3d3e0f4f`. Only the survival tab of the battle HUD Hunt Edict sheet (`GameUI.EdictSheet`) changed. Saving, undo, play and transactions (`SheetApply`/`SheetCommit`/`CommitHuntEdict`), option ids and values are unchanged.

## What changed

- Pressing Survival in the tab column opens a sub-menu to its left, the way the Combat tab does. It has two entries, `Avoidance by damage type` and `Special dangers & avoidance` (`GameUI.CasualEdictMenu.BuildCombatEntryList` is shared by combat and survival). The emergency response, auto potion and preserved-means groups are left out of this menu as requested and are changed in the Hunt Edict window only. At a tutorial step where none of the two menu groups is shown yet, the previous sheet still opens (`SurvivalMenu`).
- The new `GameUI.EdictSurvival.cs` builds both popups on the compact card of the position and target popups (`BuildCompactEdictSurface`): same plate, name plate, ‹ ×, Revert and Play. A long title now pushes the live dot and text along instead of running under them.
- **Avoidance by damage type**: one 0–100 slider each for lingering damage zones and projectiles (`AddSheetNumber` is reused, so drag, − +, and save-once-on-release behave the same). 0 never dodges (`policy=OFF`), 100 always dodges (`ALWAYS`), 1–99 dodges when HP loss exceeds N% per second (`DAMAGE` + `hpPercent`). Policy and threshold are saved together through `SheetApplyMany`, so one is never saved without the other. `EdictDodgeLevel` (Core) converts between the slider and the options. The status line (grey, gold, coral) changes live while dragging.
- **Special dangers & avoidance**: chips for stunned, frozen, root and pull (multi-select, `dodge.control`) and an effect line that names the chosen dangers and the current dodge method (`dodge.method`).
- 14 new strings were added to `en.txt` (Korean key plus English).

## Differences from the mockup (decisions needed)

- **The dodge-skill registration checklist is not built (m-4).** There is no option to save the registered skills (`dodge.controlSkills`) and no simulation rule that reads them. A checkbox that saves nothing would be a false screen, so the effect line only shows the existing `dodge.method` (walk first, escape skill first, escape skill when walking will not do). Once the registration rule is decided it is added together with the option, share/preset versions and the simulation.
- **Area attacks (`dodge.area.*`) are not in the popup (m-2).** The simulation keeps reading the last value; the Hunt Edict window can still change it.
- Slider 100 reads two cases: `ALWAYS` and the old `DAMAGE`+100 (only when one hit takes all HP). Touching the slider turns the latter into `ALWAYS` (m-1 ②). For projectiles "per second" is really one hit's loss (m-1 ④). "Exceeds" versus "at least" (m-1 ③) does not change the check and was left alone.
- The 25/50/75 ticks and diamond marker, the gold plate and tail on the selected tab, the list chevrons and section gems (m-11) are not built. The current tab selection (icon turns gold) and the shared slider look are used.
- Card heights are 410/300 units per popup and shrink to what the screen allows with the body scrolling (m-5). On portrait the card position is the same as the position and target popups (m-6).
- The big window's survival tab (`HuntEdictWindow`), group descriptions (m-9) and touch heights (m-12) are unchanged.

## Verification

- 68 focused EditMode tests pass: `EdictSurvivalMenuTests` (14 new), `CasualEdictSurfaceTests`, `CasualEdictTabArtTests`, `LocalizationTests`. The new tests check that every slider position 0–100 reads back as a committable edict, that the sub-menu draws only the two groups (even with the other survival groups shown), entry → popup → ‹ navigation, that slider +/−/Revert change policy and threshold in one save, that the two sliders do not touch each other, chip toggling and the effect line, and the live-text position under a long title.
- Static UI contract (15) and refresh guard (14 bindings) pass.
- The real sub-menu and both popups were rendered to PNG in the Editor (PC, landscape, portrait: list, slider at 0, middle and 100, no danger and chosen dangers) and checked by eye (`Assets/HELLSCRIPT/Editor/EdictSurvivalEvidence.cs`, 640×480).
- Added motion: sheet cards (list, position, target, order and survival popups) rise and fade in once when they open or change kind (`CasualNoticeEntrance` reused, 0.28 s). Switching tabs inside the same popup, a save and a rotation do not replay it. The Editor has no frame clock, so this motion was not checked.
- Not verified: the look inside a real battle screen, development-player smokes (including the part of `RuntimeBattleQuickEditSmoke` that captured the survival tab as the old sheet list), a macOS build, physical devices and touch, long English strings, how the motion feels. The full EditMode suite was not run.
