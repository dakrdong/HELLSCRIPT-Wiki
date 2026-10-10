# Battle HUD quick edit (2026-10-09)

[한국어](Battle_Quick_Edit_20261009.md)

Hunt Edict, potions and skills can be changed from the battle HUD without opening a menu. The battle is paused while a panel is open and returns to its previous pause state on close. Ordinary rift battles only; tutorial, training, puzzle and comparison battles keep their existing paths.

- **Hunt Edict button** (above the seal): the combat position group (first engagement, default position, distance, orbit direction). One tap on a choice draws the hero's forecast walk (3 s, a detached simulation, `CombatSimulation.ForecastHeroPath`) on the floor; a second tap saves through `CommitHuntEdict`. Visibility follows `HuntEdictProgression.Direct`.
- **Potion settings button** (right of the HP/resource bars): drag bars for the HP and resource thresholds, saved with `변경` (`survival.potionHpPercent`, `potion.resourcePercent`, 0-100%).
- **Skill, ultimate and potion slots**: a tap lists the candidates (nothing opens when there are none); choosing one equips it (`ClassSkillTree.Equip` + `CommitHuntEdict`; potions use `SelectPotionSlot`). Holding fills a gauge after 0.3 s over 0.7 s and opens a centred details popup (`HoldPress`); the skill popup also changes the skill's Hunt Edict quick presets.
- **Decision**: potion slots could only change in town or at a portal checkpoint; changing them during battle is now allowed. A buff potion cannot be swapped while its effect runs.
- **Live combat log removed** from the battle screen (the records screens are unchanged). Two smokes now assert its absence. The panel code in `GameUI.CombatJournal.cs` remains but is no longer called.
- **Verified**: `HuntEdictQuickMenuTests` (8), HUD, localization and shared-UI tests, and `check_ui_contract.py --verify`. A test confirms the forecast ends where the battle actually walks after the same choice is saved.
- **macOS development-build smoke** (`RuntimeBattleQuickEditSmoke`, a copy of the graduated Warrior account, 440x956 / 956x440 / 1600x900): Hunt Edict save, potion thresholds save, skill equip, buff potion equip, hold gauge and popups, and no live combat log all pass. `RuntimeCombatJournalSmoke` (HUD only) passes too.
- **Not verified**: `RuntimeFirstPlayAcceptance`, real finger input and physical devices (macOS synthetic input only). Feature tiles use text, not icon art.

## Follow-up (2026-10-09)

- The potion threshold panel is redrawn from the real HUD HP/resource bar pieces (track, fill, border, same colours and labels). The fill is the current state, a gilded notch is the threshold, and the zone at or below it is lit.
- The portrait bottom HUD is larger: only in portrait the composition width shrinks, so the vitals and settings button area meets the potion area (`GlobalHudLayout`). Landscape is unchanged, and the "same composition on rotation" test now allows only the portrait close-up of the potion group.
- The potion settings button uses a new button-shaped painting (`menu-potion-settings`, made with Codex, `Docs/Art/PotionSettingsButton/`).

The integrated full Edit Mode result and focused failure repair are recorded in [Battle damage graphs](Damage_Graphs_20261010.en.md#integration-validation-2026-10-10). Five unapproved Edict icon candidates are preserved in `Docs/Art/EdictHudIcons/Candidates/` and are not loaded by the game.
