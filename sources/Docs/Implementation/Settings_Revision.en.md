# Settings menu revision

Date: 2026-09-14 · [한국어](Settings_Revision.md)

## Layout and controls

The wide settings/help button is now a square gear icon throughout the game. The base header height decreases from 110 to 80, with town guidance and navigation space adjusted accordingly.

In landscape, settings occupy the right half of the usable screen. The actual town or hunting area remains visible, with the existing player centered in the left half; tapping that background closes settings. In portrait, settings fill the usable screen and the game world is covered by the menu. A fixed X in the upper right closes either layout. Only the body scrolls, leaving X available at every reading position. Notches, the home indicator and a visible keyboard are excluded from the interactive area.

The initial separate character, preview camera and render texture have been removed. The existing gameplay camera temporarily changes its viewport and center while preserving the actual player’s position, facing and pose. Camera angle and apparent character size remain unchanged. Closing restores gameplay framing and UI. Before entering the town, the title retains its existing background with dimming.

## Four categories

| Category | Controls |
|---|---|
| Screen | Fit display; landscape 16:9, 16:10, 4:3, 20:9, 21:9; portrait 9:16, 10:16, 3:4, 9:20, 9:21 |
| View distance | At the top of Screen, 50–150% in 5% steps using a slider, wheel or − / + buttons. 50% is closer, 100% is the default, and 150% is wider. |
| Reading size | Within Screen, 50–150% in 5% steps using a slider, mouse wheel or − / + buttons. |
| Sound | Master, music and effects volume; mute all; effects preview; defaults |
| Language | English / 한국어, immediately repainting the current screen |
| Character | Warrior → Mage → Ranger; the selected character cannot be selected again |

Ratios apply immediately. Desktop windows resize to fit the monitor; mobile requests the corresponding orientation and fits the game into the selected ratio. Unused space is black. Fit display uses the current window or device screen.

View distance scales the orthographic camera’s field of view while preserving its angle, player position, movement speed and UI size. It applies immediately to the actual town or hunt in landscape settings; in portrait, the result is visible after closing. The independent `hellscript-view-distance-v1.json` device file preserves invalid original data and falls back to 100%. Failed saves still apply the selected view immediately and offer retry.

Reading size retains the existing proportional scaling of text and controls. While the slider is held, the settings canvas keeps its size; releasing applies the chosen scale, keeping the track stable under the pointer. Reading position is preserved. Ratio, reading size, language and audio use device preference files separate from the account. Invalid preferences preserve their original file and use defaults; failed saves provide feedback and a retry path.

## Character transitions and state

Combat, training and repeat-hunt countdowns stop while settings are open. Closing with X or the background resumes the previous state; an already paused run stays paused.

Changing characters ends the current rift and repeat hunt, then arrives at the town spawn with the new character. Already earned currency, equipment, experience and combat records follow the existing run-completion rules. Unopened chests and other unearned rewards are not granted. Ending training does not create rewards or a real rift record.

The transition finishes the run and changes the selected hero in an account copy, then switches the live account and screen only after saving succeeds. A failed save preserves the old character, run and file, leaving the menu open with an error. Retrying does not duplicate records or rewards.

## Ownership and verification

`GameUI.ScreenSettings.cs` owns the menu, `WorldView.Settings.cs` frames the actual world, `GameUI.ViewDistance.cs` and `ViewDistance.cs` own view-distance controls and persistence, `DisplayAspect.cs` owns ratio preferences and `GameStore.CharacterSwitch.cs` owns the saved character transition. No scenes or prefabs are replaced. Existing entry points for help and combat information remain available.

All 174 relevant Edit Mode tests passed in Unity 6000.6.0f1. Coverage includes preference preservation/retry, successful and failed character saves, record deduplication, ratio-grid reflow after width changes, localization and existing layout checks. View-distance tests cover the default, all 21 saved steps, bounds, malformed-file preservation, failed saves and retry. Validation uses an isolated checkout of `297018b` plus this actual-world and view-distance revision, separating the concurrent rift-terrain work.

The native macOS development player verifies gears on title/town/battle, four categories, all ten ratios and 21 reading sizes, wheel and stable dragging, pointer hit testing for background and X, landscape half-screen/portrait full-screen placement, actual town/rift player identity, position and pose, unchanged camera angle and apparent scale, all 21 live view distances and restored camera/UI after closing, training pause/resume and preservation of an existing pause, plus real-rift earned experience, one completion record and town arrival. A separate launch verifies restored ratio, size, view distance, language and selected character. All runs use an isolated save directory. Orientation changes and touch on physical iOS/Android devices remain separate verification work.

[Validation summary](SettingsWorldEvidence/validation.json) · [Edit Mode results](SettingsWorldEvidence/editmode-results.xml) · [Runtime result](SettingsWorldEvidence/settings-runtime.txt) · [Restart result](SettingsWorldEvidence/settings-restart.txt)

## App screenshots

- [Actual town player and right-hand settings: 100% view distance](SettingsWorldEvidence/town-100.png)
- [50% view distance: closer](SettingsWorldEvidence/town-50.png)
- [150% view distance: wider](SettingsWorldEvidence/town-150.png)
- [Settings over the actual hunting area](SettingsWorldEvidence/rift-100.png)
- [Portrait: full-screen menu](SettingsWorldEvidence/portrait.png)
- [125% view distance restored after restart](SettingsWorldEvidence/restart.png)
