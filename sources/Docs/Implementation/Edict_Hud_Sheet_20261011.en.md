# Battle HUD Hunt Edict sheet (2026-10-11)

[한국어](Edict_Hud_Sheet_20261011.md)

Hunt Edict settings are changed inside the battle screen without leaving to a menu. The Hunt Edict button above the seal pauses the battle and fans out five tab icons (combat, survival, explore, repeat, presets). A tab opens one sheet above the icon row.

- **Sheet**: the tab name with `Undo`, `Play` and close on top, the group chips below (five for combat), then the chosen group's options in a scrolling list. No other popup or screen change.
- **Controls**: choices, switches and skill references are chips; numbers are a slider with minus/plus (the slider saves when released); multiple choices are toggle chips; orders are an up/down list. "?" unfolds the explanation in place.
- **Saved at once**: a change goes through the existing `CommitHuntEdict` transaction and the saved value is read back, so a child option disappears when its parent is switched off and returns when it is on (`HuntEdictUiCatalog` conditions). `Undo` reverts up to 30 changes made in the sheet.
- **Watching the effect**: the battle stays paused while the sheet is open; `Play` lets it run to see what the change does (press again to pause). Closing restores the pause state from before. In the position group the floor shows the hero's forecast walk for the saved settings.
- **Disclosure**: follows `HuntEdictProgression`. Options the tutorial has not shown never appear, and options not open for direct editing do not change. The repeat tab only shows its lock note until repeat settings are unlocked.
- **Data**: `HuntEdictHudRows` computes groups, rows and choices (pure functions, `HuntEdictHudRowsTests`); the screen is `GameUI.EdictSheet.cs`.
- **Icons**: the five tab icons are Codex candidates registered as `Resources/Art/GlobalHUD/menu-edict-*.png` (prompts and hashes in `Docs/Art/EdictHudIcons/`).
- **Presets tab**: not settable here yet; it only shows a note. The next step adds slots, share code and import.
- **Scope**: ordinary rift battles. The earlier four position tiles and their first-tap selection are replaced by this sheet.
- **Verified**: four data-layer tests and 93 related tests, and the macOS development-build smoke `RuntimeBattleQuickEditSmoke` (choice chip, stepper, undo, order, parent-child rows, four tabs, play, close, five screen sizes). Not verified: full Edit Mode, the bag, pickup and skill moves of later steps, physical devices, long English text clipping.

## Buttons always shown (added 2026-10-11)

The Hunt Edict button and the potion threshold button are shown wherever the HUD is (rifts, tutorial, training, town). In a rift fight and in the guide tutorial stage a tap pauses the fight and changes go straight into it; elsewhere (town, training) nothing pauses, changes are saved to the hero only, and the play button and the walk forecast are off. The tab icons that fan out are 80% of the Hunt Edict button. Verified by `RuntimeGuideTutorialSmoke` (440×956 Korean): both buttons, the pause and an icon ratio of 0.80 in the tutorial and in town.

## Latest-main integration preparation (2026-10-11)

Resolved PR #91 potion/sheet conflicts while preserving the current potion/retreat tabs and dedicated target/position panels. Town settings save to the hero without reading stale combat vitals, forecasting or playback. The native guide smoke is muted and now checks disabled town position playback and full potion bars without combat numbers. Shared UI static validation, 15 regressions and refresh ownership checks passed. Unity compilation/runtime verification of this integrated source and the final main merge remain pending.
