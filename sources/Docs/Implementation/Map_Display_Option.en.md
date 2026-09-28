# Map display option

Bottom-layout update, 2026-09-28: the live journal is at the bottom, with the full HUD above it. The minimap reserves space above that stack and recalculates on journal expansion/collapse. See [integration validation](All_Work_Integration_20260928.en.md).

Updated: 2026-09-28 · [한국어](Map_Display_Option.md)

The on/off “Overlay map” toggle in Settings → Screen is now a single-choice “Map display” selector. It has three options, and the battle view draws only the map of the chosen style. The choice is saved on this device only and never in the account save. It applies immediately in an open battle without restarting it.

## Options

| Option | What the battle view draws | Description shown in Settings |
| --- | --- | --- |
| 겹침 지도 / Overlay map | The existing player-centred automap (`riftOverlay` and `RiftAutomap` in `GameUI.OverlayMap`) over the middle of the battle view. | Draws the explored dungeon over the middle of the screen, centred on the player. |
| 미니맵 창 / Corner minimap | The existing small map window (`AddRiftMinimap` and `RiftMinimap` in `GameUI.Rift`) on its own in the screen corner. | Shows the map on its own in a small window in the corner of the screen. |
| 표시 안 함 / Hidden | No map. Exploration is still recorded. | No map is shown. Exploration is still recorded. |

Every line is in `Resources/Localization/en.txt` in Korean and English. The three lines used only by the old toggle no longer exist in the source, so their entries were removed. The device-save notice and the save-failure lines are reused unchanged.

## Settings pane

The selector sits in the Screen pane of `GameUI.ScreenSettings`, after view distance and before the screen ratio. Three choice buttons follow the heading, each with its description below it. The buttons are the same `BigButton` as the language choices, and the selected state uses the same `UiTheme.Choice` option role as the ratio and language choices. No new colour or font was added. A tap saves to the device file at once and lays out the battle view again.

The save-failure message and retry keep the old toggle's behaviour. If the device write fails, the choice stays applied on screen, “Applied the map preference but could not save it on this device. Please retry saving.” appears, and the retry button is shown. The retry saves the current choice again; on success the message and button disappear. If the file cannot be read, the game starts with the default and says that the original file was preserved.


## The corner panel in battle

The corner panel at the top right stays in all three styles. Its chest counter and navigation-fault text are not map elements, so they always show, and tapping the panel opens the expanded map. Only the Corner minimap style draws the map inside it. In Overlay and Hidden the panel shrinks to its text.

On the previous `main` the panel appeared only in landscape at a width of at least 720, so portrait showed neither the map nor the chest counter. Players who choose the corner minimap need it in either orientation, so the panel now also shows in portrait. The layout rules are below, in UI units after the interface scale. The hero stands at the centre of the camera frame, and the layout computes that point relative to the safe area.

### Horizontal position and width

- The panel starts at 140 or more, right of the left button column (events 18–108, adventure journal 18–132). Normally its right edge is 68 from the right side of the screen (120 beside the menu button), clear of the content dock column (44 wide, 12 from the edge). It is 168 wide and narrows on small screens.
- In portrait, when the lane between the two button columns cannot hold the chest counter or a map as large as the portrait cap (34% of the width), the panel reaches under the content dock column instead. Its right edge is then 12 from the screen edge and its left edge at least 20 right of the hero. This applies to portrait screens narrower than about 327 units, which on phones means a larger text size: with the text size in 5% steps, from about 110% on 360-wide devices, 120% on 390-wide devices and 135% on 430-wide devices. With it the 440×956 map at 150% grew from 77 to 99. By the previous layout formulas, 360×780 at 150% had no map and a cut chest counter, and 390×844 at 150% had a 44 map; they are now 80 and 88.
- In this layout the panel is drawn before the header. When the content dock folds out, its shortcut buttons cover the right part of the panel (part of the map and the end of the chest counter) and take the pointer. Folding the dock shows the whole panel again.
- The chest counter never wraps: the panel is at least as wide as the longest counter text this rift can show, plus 8.
- In landscape the panel shares rows with the hero, so it narrows when needed to stay at least 20 right of the hero. None of the landscape sizes in the smoke needed that.

### Vertical position and map size

- The top is below the header text lines (64 in landscape, 122 in portrait, 158 in portrait narrower than 360). The bottom stops 8 above the live combat log (4 beside the menu button). The previous code let the gap shrink to 3, so it now matches the documented 8; this made the 2100×900 map at 150% 62 instead of 67 and the 1600×900 map at 150% 111 instead of 116. Expanding or collapsing the log lays the panel out again.
- The map is at most 160 and fits the panel width, 40% of the height (landscape) or 34% of the width (portrait), and the remaining height. Below 40, portrait draws no map and landscape moves the panel up as the next rule describes.
- When a landscape phone at a large text size has no room under the header for the chest counter or the map (956×440 at 150%), the panel moves up beside the menu button with the map right of the chest counter. At 956×440 and 150% the map is 47 units (70.5 px) square. Header lines sharing rows with the panel stop before it.
- The panel never covers the hero. Unless it stays at least 20 right of the hero, its map shrinks so that the panel ends 56 above the hero's feet. The hero's head is about 48 above its feet, and 8 more keep a gap.

### Boss status

The boss status never overlaps the panel. Its place is chosen in this order.

1. When at least 240 remains between the left button column (140) and the panel, the boss status goes left of the panel, starting at 140 or more. The previous code allowed its left edge down to 18, so on landscape screens about 530–660 wide it slid under the events and adventure journal buttons. At 640×360 it now starts at 140.
2. In a boss fight, when a map of 40 or more lets the panel end 8 above the boss status's own row, the map shrinks to that and the boss status stays in place. This applies to landscape screens narrower than 600 (boss row 176); at 568×320 the map is 71 square.
3. Otherwise the boss status moves below the panel. In a boss fight the map shrinks so that the boss status ends 56 above the hero's feet, using the boss status's real height, which grows when the boss name wraps. The boss status text changes during the fight, so at a given screen size the tallest height it has had in this fight is kept. When the text grows, the map shrinks once more; when it shrinks, the map does not grow back, so the map size does not change with every text update. Only when the shrunk map would be under 40 does the panel leave the map out for that boss fight and keep the chest counter. The boss status then sits where `main` puts it, and the map returns when the fight ends.

The previous code skipped the shrink in step 3 and left the map out at once, so on common phones in portrait the map was missing for the whole boss fight even at 100% text. The checked results are:

- 390×844 (simulated notch) at 100%: the map shrinks to 75 in Korean and 52 in English (the boss name wraps to three lines) and stays.
- 360×780 at 100% in English: the map shrinks to 67 and stays.
- 440×956 at 100% in Korean: the 149 map stays and the boss status sits below the panel.
- 440×956 at 150% in English: this portrait is narrower than 360, the boss row is 212, and a 40 map does not fit, so the map is left out.
- On phone portraits the room for a shrunk map gets smaller with a narrower width, a taller top safe area and a larger text size. By the layout formulas, from 125%, where phones get narrower than 360, almost every phone size leaves the map out during boss fights. At 100% a 360-wide screen with a 47 top safe area leaves it out as well. Only the sizes listed above were run.
- Landscape screens 600–624 wide (for example 640×360 at 105%, 667×375 at 110%, or 640×360 at 100% with a 24–32 side safe area) meet neither step 1 nor step 2, so the map is left out during boss fights. This was only computed.

### Navigation-fault text

- The fault text goes through the text table (`Loc.T`) and shows in the chosen language. The previous code put the raw source string in from `RefreshHud`, so English showed Korean. That bug already existed in landscape on `main`; showing the panel in portrait made it visible in more places.
- The fault row is laid out as one line first, then takes as many lines as fit between the chest counter and the live combat log. When the text does not fit, it is cut at a line end with an ellipsis (…). The whole message and the state recheck button are always on the expanded map.
- The map shrinks by the height of the fault row. In the landscape layout beside the menu button the fault takes the whole panel width, so no map is drawn, as before.
- At 440×956 and 150% the whole message fits (five lines in both Korean and English) and the map stays 99. At 956×440 and 150% only one line fits: “보스 등장 공간을 확보하지 …” and “No room could be secur…”.

## Reaching the expanded map

In every style the expanded map (`ShowRiftMap`, which pauses the battle) has two entries: tapping the corner panel, and ☰ observation menu → Map. The panel stays in Overlay and Hidden so that the chest counter remains visible and the existing tap keeps working.

## Device save and migration

`Core/MapDisplaySettings.cs` saves the choice to `hellscript-map-display-v1.json` in the device settings folder as `{"version":1,"mode":"corner|overlay|hidden"}`. The location and the write through a temporary file are the same as the old overlay setting, and nothing is written to the account save.

The old toggle's file `hellscript-overlay-map-v1.json` is only read. Without a new file, on starts as Overlay map and off starts as Corner minimap. Once a choice is saved the new file wins; the old file is never deleted or rewritten.

A fresh device starts with the corner minimap. [First-play improvements](First_Play_Improvements.en.md) D08 made the overlay default off for new devices, so this keeps what a new device shows today (no overlay, the corner map in landscape). [Real-time visibility and exploration maps](Rift_Visibility_Expansion.en.md) records the earlier default of on, which D08 has since changed.


## Code changes

- `Core/MapDisplaySettings.cs`: the three styles, the device save, the legacy migration, and read/write failures. It replaces `OverlayMapSettings.cs`.
- `GameController.Display.cs`: initialises `MapDisplay` instead of `OverlayMap`.
- `GameUI.OverlayMap.cs`: the settings choices and the immediate apply; the overlay is enabled only in the Overlay style.
- `GameUI.Rift.cs`: the corner panel layout, including the layout under the content dock column, the chest counter width, the hero clearance, the translated and fitted fault row, and the drawing order.
- `GameUI.BattleLayout.cs`: header line ends and the boss status position, the hero point (`HeroOnPage`), the hero clearance constants (`HeroClearance` 56, `HeroSideClearance` 20) and the tallest boss status height of the current fight. The new panel is kept by reference so the rebuild frame never lays out the old panel that is about to be destroyed.
- `GameUI.cs`: `RefreshHud` no longer writes the raw fault string into the panel; the panel layout writes the translated, fitted text.
- `RuntimeVisibilitySmoke.cs`: uses the new selection API and buttons instead of the old toggle API, with a tutorial-complete fixture and a wait for the new rift's admission.
- `RuntimeMapDisplaySmoke.cs`: the macOS development-build check for this feature.
- `Tests/Editor/MapDisplaySettingsTests.cs`: tests for the settings class. The old toggle test was removed from `RiftVisibilityTests` and replaced by these tests.

## Verification

### Automatic checks

`python3 tools/check_ui_contract.py` and `python3 tools/test_ui_contract.py` (9 checks) passed.

### Unity Edit Mode

The 11 `MapDisplaySettingsTests` cover the settings class. A fresh device starts with the corner minimap and writes no file. Each style survives a new instance. The legacy on migrates to Overlay map and off to Corner minimap, and the legacy file stays unchanged. A failed save keeps the choice with a retry, and a successful retry clears the message. A damaged new or legacy file starts with the default and is preserved.

The full EditMode suite (4,107 tests) ran in the cloned project: 4,058 passed and 49 failed. 47 are the known baseline failures (main 24694040). `CoreTests.GeneratedDungeonContainsEnoughPointsAndUniqueEnemyIds(6,150)` seeds itself from the clock and fails for about 26 of 300 seeds on the base as well, so it is unrelated to this change. `LocalizationTests.EveryKoreanLiteralInTheRuntimeHasAnEntry` already failed on origin/main (two literals in `RuntimeFirstPlayAcceptance.cs`). This branch fixes it with two en.txt rows; afterwards `LocalizationTests`, `StoredLocalizationTests` and `MapDisplaySettingsTests` passed 63/63. Summary: [full EditMode summary](MapDisplayEvidence/editmode-full.txt).

### macOS development-build smoke

The development player was built from the clone with `ProjectBuilder.BuildMac` with 0 build errors. `RuntimeMapDisplaySmoke` (`-hellscriptMapDisplaySmoke`) uses a tutorial-complete account and resumes a synthetic two-room rift. It checks that each real Settings → Screen choice button is the top EventSystem raycast hit and then sends pointer down, up and click events to it.

The three styles were chosen in turn under 24 conditions: five screen sizes in Korean and English at 100% and 150% text, plus 360×780 and 390×844 in Korean and English at 150%. At 390×844 `UiSafeArea.Simulate` stood in for a 47-pixel top notch and a 34-pixel home bar. Each condition checked the following.

- The choice is saved to the device file at once, and the battle keeps the same run object and battle time. The battle does not restart.
- Only the chosen style's map is drawn. The overlay is centred on the player, and the corner minimap draws real geometry at no less than its minimum size: 64, or 36 in the panel beside the menu button, or 40 during a boss fight or a fault or when the panel ends 8 above the live combat log.
- The chest counter shows in all three styles and does not wrap. Panel and settings text is not clipped, and no Korean remains in English.
- The panel stays inside the safe area and clear of the header text lines, the menu, settings, content dock, events and adventure journal buttons, the boss status, the live combat log and the persistent HUD. It does not cover the hero: it ends 56 above the hero's feet or stays at least 20 beside the hero.
- After a language or text-size change rebuilds the battle view, it draws the last choice.

Further checks:

- Under the six conditions where the panel reaches under the content dock column (440×956, 360×780 and 390×844 at 150%, Korean and English), the content dock was folded out with real pointer input. All four shortcuts were confirmed as the top raycast hit, and the dock was folded again.
- Twelve boss-fight conditions: 440×956 (Korean 100%, English 150%), 390×844 with the simulated notch (Korean and English 100%), 360×780 (English 100%), 640×360 (Korean and English 100%), 568×320 (English 100%), and 956×440, 1600×900, 1600×1000 and 2100×900 (English 150%). The smoke computes from the real rectangles whether a left-out map is allowed: only when a 40 map fits neither above the boss status's row nor with the boss status below the panel clear of the hero. It has no exception keyed to a condition name, unlike the previous smoke. When the boss status sits below a panel with a map, it must end 56 above the hero's feet, and in landscape it must not overlap the events and adventure journal buttons. In the 390×844 simulated-notch English 100% boss fight the boss was moved out of sight, back into sight and out of sight again, so that the boss status text and height changed; the map had to shrink at most once and never grow back.
- Four navigation-fault conditions: at 440×956 and 956×440 in Korean and English at 150%, a fault was set in the Corner minimap and Hidden styles and checked for translation, clipping and overlaps. The fault was then cleared and the fault row had to disappear.
- Device save failure and retry under two conditions, and both entries to the expanded map in each style.

The first launch ended with `HELLSCRIPT_MAP_DISPLAY_SMOKE_OK` and 140 check lines. The second launch (`-hellscriptMapDisplayRelaunch`) restored Overlay map from the device file in a separate process, showed it selected in Settings, and ended with `HELLSCRIPT_MAP_DISPLAY_RELAUNCH_OK`.

`RuntimeVisibilitySmoke` failed with a NullReferenceException in the 2026-09-26 `main` baseline because fresh accounts must play the tutorial first. With the account fixture and the admission wait added, its first launch (`HELLSCRIPT_VISIBILITY_SMOKE_OK`) and restart (`HELLSCRIPT_VISIBILITY_RESTART_OK`) both passed.

`RuntimeTutorialSmoke` is one of the smokes that pass on `main`. The tutorial rift also shows the corner panel, so it was rerun as a regression check; its first launch and its resume launch both passed (`HELLSCRIPT_TUTORIAL_RUNTIME_SMOKE_OK`). Both smokes were rerun with the development build that includes this fix.

```
Unity -batchmode -projectPath <clone> -executeMethod Hellscript.Editor.ProjectBuilder.BuildMac -hellscriptBuildOutput <build>/HELLSCRIPT.app -quit
HELLSCRIPT -hellscriptMapDisplaySmoke -hellscriptSavePath <save> -hellscriptScreenshots <shots> -screen-fullscreen 0 -screen-width 1600 -screen-height 900
HELLSCRIPT -hellscriptMapDisplaySmoke -hellscriptMapDisplayRelaunch -hellscriptSavePath <save> -hellscriptScreenshots <shots2> -screen-fullscreen 0 -screen-width 1600 -screen-height 900
HELLSCRIPT -hellscriptVisibilitySmoke [-hellscriptVisibilityResume] -hellscriptSavePath <save> -hellscriptScreenshots <shots> -screen-fullscreen 0 -screen-width 1600 -screen-height 900
HELLSCRIPT -hellscriptTutorialSmoke [-hellscriptTutorialResume] -hellscriptSavePath <save> -hellscriptEvidencePath <shots> -screen-fullscreen 0 -screen-width 1600 -screen-height 900
```

### Evidence

Each image puts the three styles (Overlay map · Corner minimap · Hidden) of one condition side by side. PC sizes are scaled to 50%; phone sizes are at full size.

Settings → Screen, map display:

- Portrait 440×956: [Korean 100%](MapDisplayEvidence/settings-440x956-ko-100.png), [English 100%](MapDisplayEvidence/settings-440x956-en-100.png), [Korean 150%](MapDisplayEvidence/settings-440x956-ko-150.png), [English 150%](MapDisplayEvidence/settings-440x956-en-150.png)
- Landscape 956×440: [Korean 100%](MapDisplayEvidence/settings-956x440-ko-100.png), [English 100%](MapDisplayEvidence/settings-956x440-en-100.png), [Korean 150%](MapDisplayEvidence/settings-956x440-ko-150.png), [English 150%](MapDisplayEvidence/settings-956x440-en-150.png)
- PC 16:9 1600×900: [Korean 100%](MapDisplayEvidence/settings-1600x900-ko-100.png), [English 100%](MapDisplayEvidence/settings-1600x900-en-100.png), [Korean 150%](MapDisplayEvidence/settings-1600x900-ko-150.png), [English 150%](MapDisplayEvidence/settings-1600x900-en-150.png)
- PC 16:10 1600×1000: [Korean 100%](MapDisplayEvidence/settings-1600x1000-ko-100.png), [English 100%](MapDisplayEvidence/settings-1600x1000-en-100.png), [Korean 150%](MapDisplayEvidence/settings-1600x1000-ko-150.png), [English 150%](MapDisplayEvidence/settings-1600x1000-en-150.png)
- PC 21:9 2100×900: [Korean 100%](MapDisplayEvidence/settings-2100x900-ko-100.png), [English 100%](MapDisplayEvidence/settings-2100x900-en-100.png), [Korean 150%](MapDisplayEvidence/settings-2100x900-ko-150.png), [English 150%](MapDisplayEvidence/settings-2100x900-en-150.png)
- Portrait 360×780: [Korean 150%](MapDisplayEvidence/settings-360x780-ko-150.png), [English 150%](MapDisplayEvidence/settings-360x780-en-150.png)
- Portrait 390×844 (simulated 47 notch and 34 home bar): [Korean 150%](MapDisplayEvidence/settings-390x844-notch-ko-150.png), [English 150%](MapDisplayEvidence/settings-390x844-notch-en-150.png)

Battle view:

- Portrait 440×956: [Korean 100%](MapDisplayEvidence/battle-440x956-ko-100.png), [English 100%](MapDisplayEvidence/battle-440x956-en-100.png), [Korean 150%](MapDisplayEvidence/battle-440x956-ko-150.png), [English 150%](MapDisplayEvidence/battle-440x956-en-150.png)
- Landscape 956×440: [Korean 100%](MapDisplayEvidence/battle-956x440-ko-100.png), [English 100%](MapDisplayEvidence/battle-956x440-en-100.png), [Korean 150%](MapDisplayEvidence/battle-956x440-ko-150.png), [English 150%](MapDisplayEvidence/battle-956x440-en-150.png)
- PC 16:9 1600×900: [Korean 100%](MapDisplayEvidence/battle-1600x900-ko-100.png), [English 100%](MapDisplayEvidence/battle-1600x900-en-100.png), [Korean 150%](MapDisplayEvidence/battle-1600x900-ko-150.png), [English 150%](MapDisplayEvidence/battle-1600x900-en-150.png)
- PC 16:10 1600×1000: [Korean 100%](MapDisplayEvidence/battle-1600x1000-ko-100.png), [English 100%](MapDisplayEvidence/battle-1600x1000-en-100.png), [Korean 150%](MapDisplayEvidence/battle-1600x1000-ko-150.png), [English 150%](MapDisplayEvidence/battle-1600x1000-en-150.png)
- PC 21:9 2100×900: [Korean 100%](MapDisplayEvidence/battle-2100x900-ko-100.png), [English 100%](MapDisplayEvidence/battle-2100x900-en-100.png), [Korean 150%](MapDisplayEvidence/battle-2100x900-ko-150.png), [English 150%](MapDisplayEvidence/battle-2100x900-en-150.png)
- Portrait 360×780: [Korean 150%](MapDisplayEvidence/battle-360x780-ko-150.png), [English 150%](MapDisplayEvidence/battle-360x780-en-150.png)
- Portrait 390×844 (simulated 47 notch and 34 home bar): [Korean 150%](MapDisplayEvidence/battle-390x844-notch-ko-150.png), [English 150%](MapDisplayEvidence/battle-390x844-notch-en-150.png)

Other evidence:

- Corner minimap in boss fights: [phone portraits](MapDisplayEvidence/boss-portrait.png), [landscape and PC](MapDisplayEvidence/boss-landscape.png), [small landscape 640×360 and 568×320](MapDisplayEvidence/boss-small-landscape.png), [boss status text changes](MapDisplayEvidence/boss-status-change.png)
- Content dock folded out over a panel that reaches under its column: [six conditions at 150%](MapDisplayEvidence/dock-over-panel.png)
- Navigation-fault text: [portrait 440×956](MapDisplayEvidence/fault-portrait.png), [landscape 956×440](MapDisplayEvidence/fault-landscape.png)
- Device save failure and retry: [Korean 100% and English 150%](MapDisplayEvidence/save-failure.png)
- Both entries to the expanded map: [three styles](MapDisplayEvidence/expanded-map.png)
- Separate-process relaunch: [overlay restored](MapDisplayEvidence/relaunch-overlay-1600x900-ko-100.png), [choice shown in Settings](MapDisplayEvidence/relaunch-settings-1600x900-ko-100.png)
- Visibility smoke: [landscape](MapDisplayEvidence/visibility-landscape.png), [portrait](MapDisplayEvidence/visibility-portrait.png)
- Run results: [first launch](MapDisplayEvidence/runtime.txt), [relaunch](MapDisplayEvidence/relaunch.txt), [visibility smoke](MapDisplayEvidence/visibility-runtime.txt), [visibility restart](MapDisplayEvidence/visibility-restart.txt), [tutorial smoke](MapDisplayEvidence/tutorial-runtime.txt), [full Edit Mode summary](MapDisplayEvidence/editmode-full.txt)

## Not verified and open issues

- Not seen on a physical mobile device. Real touch, device safe areas (notches), device DPI scaling, the Android build and performance were not checked. Pointer input was synthesised through the EventSystem in the macOS development build. The notch and home bar were only simulated with `UiSafeArea.Simulate`.
- Unity Editor Play Mode and MCP were not used. The Editor holds the original project's lock, so verification used batch mode and the development build of a cloned project.
- Statements about sizes the smoke did not run come only from the layout formulas transcribed into a calculation. That calculation reproduced every panel rectangle the earlier smoke recorded, but it uses an estimate for the boss status height.
- At 956×440 and 150% the corner map is small (47 units, 70.5 px square) because there is no room under the header; a larger map there needs a different live-log or HUD layout.
- In phone-portrait boss fights with a large text size or a tall top safe area, the map is left out for the fight. Landscape screens 600–624 wide leave it out during boss fights as well. Both follow from the boss status rules above.
- When the panel reaches under the content dock column, the dock's shortcuts cover part of the map and the end of the chest counter while the dock is open. Folding the dock shows them again.
- At 956×440 and 150% the fault text is cut to one line with an ellipsis. The whole message is on the expanded map.
- Two extreme sizes were only computed. On a 320-wide device at 145–150% (root about 213–221 wide) the chest counter's minimum width pushes the panel into the adventure journal column. On a 780×360 landscape at 150% with a 48 bottom safe area (root 488×224) the live combat log sits so high that even the counter-only panel overlaps it by about 7.
- The left column reservation (140) matches the events and adventure journal buttons on the current `main`. The unmerged PR #22 moves the adventure journal button below the events button but keeps its horizontal position and width, so the reservation still holds. That conclusion comes from reading the PR's changes; the merged result was not run.
- Existing layout issues not changed by this work are visible in the evidence. On narrow portrait and at 150%, the adventure journal button overlaps the events button, the boss status, the portrait or the live log. At 956×440 and 150% the live log covers the header's timer, kills and pause lines and the content dock button, and the landscape boss status sits over the hero. At 568×320 the live log covers the boss name, and at 1600×1000 and 150% it covers the hero's head. At 150% in portrait the header subtitle runs under the menu button. In some captures taken right after Settings closed, the adventure journal button had not reappeared yet; the cause was not investigated.
- In landscape the boss status keeps its `main` position unless it would overlap the panel. Only then does it move left of the panel, or the map shrinks or is left out, following the rules above.
- `LocalizationTests.EveryKoreanLiteralInTheRuntimeHasAnEntry` fails on `origin/main` too because of the same two lines. It is outside this work and was not fixed.
- The public wiki was not published. `AGENTS.md` publishes only from merged `main`, and this work did not open or merge a PR.
