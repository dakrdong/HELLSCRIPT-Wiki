# Settings menu revision

Date: 2026-09-14 · [한국어](Settings_Revision.md)

## Layout and controls

The wide settings/help button is now a square gear icon throughout the game. The base header height decreases from 110 to 80, with town guidance and navigation space adjusted accordingly.

In landscape, settings occupy the right half of the usable screen. The current character stands in the center of the left half; tapping that background closes settings. In portrait, settings fill the usable screen and the character preview is hidden. A fixed X in the upper right closes either layout. Only the body scrolls, leaving X available at every reading position. Notches, the home indicator and a visible keyboard are excluded from the interactive area.

The preview uses a separate copy of the existing character model. It never moves the gameplay character or camera. Closing the menu or switching to portrait stops preview rendering and releases its texture. The current character is the existing replaceable primitive 3D model.

## Four categories

| Category | Controls |
|---|---|
| Screen | Fit display; landscape 16:9, 16:10, 4:3, 20:9, 21:9; portrait 9:16, 10:16, 3:4, 9:20, 9:21 |
| Reading size | Within Screen, 50–150% in 5% steps using a slider, mouse wheel or − / + buttons. |
| Sound | Master, music and effects volume; mute all; effects preview; defaults |
| Language | English / 한국어, immediately repainting the current screen |
| Character | Warrior → Mage → Ranger; the selected character cannot be selected again |

Ratios apply immediately. Desktop windows resize to fit the monitor; mobile requests the corresponding orientation and fits the game into the selected ratio. Unused space is black. Fit display uses the current window or device screen.

Reading size retains the existing proportional scaling of text and controls. While the slider is held, the settings canvas keeps its size; releasing applies the chosen scale, keeping the track stable under the pointer. Reading position is preserved. Ratio, reading size, language and audio use device preference files separate from the account. Invalid preferences preserve their original file and use defaults; failed saves provide feedback and a retry path.

## Character transitions and state

Combat, training and repeat-hunt countdowns stop while settings are open. Closing with X or the background resumes the previous state; an already paused run stays paused.

Changing characters ends the current rift and repeat hunt, then arrives at the town spawn with the new character. Already earned currency, equipment, experience and combat records follow the existing run-completion rules. Unopened chests and other unearned rewards are not granted. Ending training does not create rewards or a real rift record.

The transition finishes the run and changes the selected hero in an account copy, then switches the live account and screen only after saving succeeds. A failed save preserves the old character, run and file, leaving the menu open with an error. Retrying does not duplicate records or rewards.

## Ownership and verification

`GameUI.ScreenSettings.cs` owns the menu, `SettingsCharacterPreview.cs` owns the preview, `DisplayAspect.cs` owns ratio preferences and `GameStore.CharacterSwitch.cs` owns the saved character transition. No scenes or prefabs are replaced. Existing entry points for help and combat information remain available.

All 161 relevant Edit Mode tests passed in Unity 6000.6.0f1. Coverage includes preference preservation/retry, successful and failed character saves, record deduplication, ratio-grid reflow after width changes, localization and existing layout checks. Validation uses an isolated checkout of `8e066af` plus this settings revision, separating the concurrent rift-terrain work.

The native macOS development player verifies gears on title/town/battle, four categories, all ten ratios and 21 reading sizes, wheel and stable dragging, pointer hit testing for background and X, landscape half-screen/portrait full-screen placement, character preview, training pause/resume and preservation of an existing pause, plus real-rift earned experience, one completion record and town arrival. A separate launch verifies restored ratio, size, language and selected character. All runs use an isolated save directory. Orientation changes and touch on physical iOS/Android devices remain separate verification work.

[Validation summary](SettingsEvidence/validation.json) · [Edit Mode results](SettingsEvidence/editmode-results.xml) · [Runtime result](SettingsEvidence/settings-runtime.txt) · [Restart result](SettingsEvidence/settings-restart.txt)

## App screenshots

- [Landscape: character on the left and settings on the right](SettingsEvidence/settings-landscape.png)
- [Portrait: full-screen settings](SettingsEvidence/settings-portrait.png)
- [150% reading size](SettingsEvidence/settings-reading-150.png)
- [Audio settings](SettingsEvidence/settings-sound.png)
- [Character selection in English](SettingsEvidence/settings-character-en.png)
- [English settings restored after relaunch](SettingsEvidence/settings-restart-en.png)
