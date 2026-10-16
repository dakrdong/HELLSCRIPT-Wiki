# Options (Settings) Window Casual Skin — Codex Task Prompt

Written: 2026-10-15 · [한국어](Casual_Options_Window_Codex_Prompt_20261015.md)

Paste the whole code block below into Codex as it is. The mockup material is on the `claude/casual-options` branch (pushed to the remote).

| What to hand over | Where |
|---|---|
| Design canvas (24 boards, private) | https://claude.ai/artifact/ARTvBnCWdj5yPkCovn6XFo. The user has to share it for Codex to open it. The repository copy below is enough if it cannot be opened |
| Mockup source of truth (sources, 24 reference screens, measurement table, 25 icon SVGs, check tool) | `Docs/Design/Options/Casual/` ([README](../Design/Options/Casual/README.en.md)) |
| This prompt | This document |

```text
Rebuild HELLSCRIPT's shared settings window (the options window the gear button opens) to match the casual StyleD mockup. The mockup source of truth is Docs/Design/Options/Casual/ (README, 24 reference screens, mockup source, icon SVGs, check tool). Keep what each setting means and how it is saved and applied; change the look and composition only (one row of tabs, cards, icons, thumbnails, hero cards).

## Background
- The game's art is moving from dark gothic to bright casual (StyleD, ages 12+). Screen-by-screen skinning is approved and this task is the shared settings window. The skill window, battle HUD, training ground, potion settings and others already have the casual skin (Docs/Implementation/Casual_UI_Skin_Rules_20261014.md, Casual_UI_Integration_20261014.md).
- The mockup spreads the look of the potion and retreat settings window (cream plate, navy outline, yellow selected tab, bar-and-marker slider, green confirm button, coral close and warnings) to every tab of this window. README "Look" is the source of truth for colours and fonts.
- The target is only the shared settings window that GameUI.ScreenSettings opens. Hunt Edict options (HuntEdictWindow.Options), gear recommendation settings (RiftGearSettingsWindow) and shop auto options (EquipmentShopWindow.Settings) are other screens.

## Do first
1. Rules: AGENTS.md, CLAUDE.md, AgentRules/ (Shared_UI, UI_Refresh, Workflow, CPU_Performance, Unity_MCP, Wiki_Publish, Artifact_Cleanup), Docs/Implementation/Casual_UI_Skin_Rules_20261014.md. One task, one agent. Conversation and development records are Korean; code comments, commit messages and logs are English.
2. Create a new worktree and the branch codex/casual-options from the latest origin/main (other sessions share the same checkout). Bring the mockup in like this:
   git fetch origin claude/casual-options && git checkout origin/claude/casual-options -- Docs/Design/Options/Casual Docs/Implementation/Casual_Options_Window_Codex_Prompt_20261015.md Docs/Implementation/Casual_Options_Window_Codex_Prompt_20261015.en.md
3. Read Docs/Design/Options/Casual/README.en.md to the end and open the 24 screens in reference/. The measurements, part states and tab body order are owned by that document and the CSS variables in canvas/Main.dc.html; what follows is a summary. node Docs/Design/Options/Casual/tools/smoke.js is the mockup rules check and the source of the rules to move into game tests.

## Current state (as of main af716fba, code I read)
- Owning files (Assets/HELLSCRIPT/Runtime/Presentation):
  - GameUI.ScreenSettings.cs: window frame, tabs, the Screen, Language and Character bodies, ReflowCommonPanel (layout), CloseCommonPanel.
  - GameUI.Audio.cs (audio, VolumeSlider), GameUI.ViewDistance.cs, GameUI.OverlayMap.cs (BuildMapDisplayControl), GameUI.Accounts.cs (account), GameUI.SettingsInformation.cs (game guide and combat state).
  - SettingsControls.cs (SettingsChoiceGrid, SettingsGearGraphic, SettingsStepSlider), GameUI.SettingsFrame.cs (the black bars of the screen ratio, MakeSettingsIcon), WorldView.Settings.cs (ShowSettingsWorld and RestoreSettingsWorld, which show the real town or hunting ground on the left in landscape).
  - Entry: the gear on the title, town and battle screens calls ShowScreenSettings, ShowHelp and ShowCombatOverview.
- Frame: ShowCommonPanel builds the modal with a ScreenSpaceOverlay Canvas (sorting 120), a CanvasScaler and ContentWindowHost.Attach.
  - A settings-backdrop button, a hint on the left half ("Tap the background to close."), the plate (commonCard), the title (26pt), five tabs (settings-tab-Screen/Sound/Language/Character/Account) and the close button settings-close (UiButtonRole.Quiet, "×").
  - Bodies use CommonScroll (ScrollRect + VerticalLayoutGroup, spacing 14, padding 18) and text from CommonNote at 18 to 24pt.
  - During the tutorial (not EntryPage and Tutorials.Mandatory) the Character tab and body do not exist and the Return to title button moves to the top of the Account body.
- Layout: ReflowCommonPanel sets the title at 56, the tab height at 48 (40 when the window is under 400 high) and the tab columns (2 below a width of 620, otherwise 4, so 5 tabs take two rows), and places the plate on the right half of the safe area (the left half shows the real game) in landscape or across everything in portrait.
  - It subtracts the keyboard and safe areas, restores the reading position with DialogReadingAnchor and redraws slider handles that were clipped.
- Colours and fonts: the old Obsidian & Gilt values (gold, pale, muted, panel, ink) and the default UiFonts font. BigButton is Button plus a LayoutElement (TouchHeight 80). Selected state is shown by UiTheme.Choice as a UiButton role (Tab, Option).
- Names that tests and smokes use (do not change): settings-backdrop, settings-close, settings-tab-<Section>, settings-aspect-auto, settings-aspect-<ratio>, settings-language-<code>, settings-character-<index>, settings-title, settings-copy-uid, settings-google-link, settings-distance-slider (SettingsStepSlider), settings-distance-up, settings-distance-down, settings-map-<mode>, settings-map-retry. The audio slider names are "전체 음량", "음악" and "효과음", and the mute button is named "전체 음소거" (same as its text).
- Owners of the setting rules (do not touch): game.ApplyAspect, ApplyLanguage, ApplyViewDistance, MapDisplay, Audio (Preferences, Change, Defaults, SavePreferences), CanChangeCharacterFromSettings, ChangeCharacterFromSettings, ReturnToTitle, GoogleLogin, SupportUid, the on-device files and their retry.
- Shared casual parts (already on main):
  - UiSkin.Casual: colour tokens (Cream, CreamShade, Gold, GoldShade, Green, GreenShade, Navy, Muted, Edge, Disabled, Coral, White), fonts (CasualDisplay = Jua, CasualBody = Noto Sans KR), ApplyChrome(UiButton, close), ApplyLabel, Clip, OnLight (contrast correction for small text), and the button palette (white default, Primary green, close and Destructive coral, chosen yellow, locked grey).
  - UiButtonChrome.Casual and CasualClose. CasualBattleHudStyle: Outline(Text) (text outline, SkillTreeD4TextShadow), the Orange and Sky tokens, Panel (the Resources Art/GlobalHUD/Casual/panel-cream and panel-white 9-slices).
  - Hero seals: GlobalHudView.Portrait(heroClass, gender) = Art/GlobalHUD/Casual/seal-*. Round button art: Art/GlobalHUD/Casual/control-minus and control-plus. Fonts: Resources/Fonts/CasualSkill (Jua-SkillSubset, NotoSansKR-Skill400/700/900).
  - The bundled fonts have no ♂♀ glyphs (checked directly). Today's Character buttons put ♂♀ in the text. Drawn in the casual fonts they would drop out, so draw them as icons as the mockup does.
  - Other windows pass UiSkin.Casual explicitly to ContentWindowView.Open(..., skin:). The settings window is built directly by GameUI, not by ContentWindowView, so pass the instance skin explicitly the same way and leave the global UiTheme values alone (other windows would be affected).
- Related tests: SettingsRevisionTests (SettingsChoiceGrid layout, saving ratio, view distance and character change), CasualUiSkinTests, LocalizationTests, StoreViewBindingTests (UI refresh), the UI contract check.
- Related smokes (grep ShowScreenSettings, commonModal and settings- for the full list): RuntimeScreenSettingsSmoke (.RiftTitle), RuntimeAudioSkillsSmoke, RuntimeLanguageSmoke, RuntimePlayerAccountSmoke, RuntimeMapDisplaySmoke, RuntimeCharacterSelectionSmoke, RuntimeTutorialSmoke, RuntimeTouchLayoutSmoke, RuntimeUiStyleSmoke.
- Related records: Docs/Implementation/Settings_Revision(.en).md (still says "four settings" but there are five tabs now), Map_Display_Option, Screen_Settings_Expansion, Shared_UI_Contract.

## To do (summary; numbers are in the README)
1. Style owner: use the mockup colours as named UiSkin values. Reuse the tokens that exist first (Cream, Gold, Green, Coral, Navy, Muted, Edge, Disabled, CasualBattleHudStyle.Orange and Sky) and add only the missing values (card edge #c9d0e2, the three notice-box colours, mint and so on) to the same owner with names. Do not scatter colour numbers through the window code. Pass UiSkin.Casual to this window explicitly and do not change the global UiTheme.
2. Frame (GameUI.ScreenSettings):
   - Plate: reuse the panel-cream 9-slice; make the inner dashed stitch and the gem on the top edge as a variant of the same source or with the same generator. The plate floats 8 to 12 in from the edge (Decision 3).
   - Title in Jua yellow plus CasualBattleHudStyle.Outline, centred. Close is a coral round button (ApplyChrome close:true) with a touch area of at least 44.
   - Tabs sit on one row at equal width (the selected UiButtonRole.Tab is yellow). PC shows icon plus label, portrait puts the icon above the label, landscape shows the label only. Do not shrink the text; wrap it to two lines.
   - Landscape uses the right half (the real game on the left), portrait uses everything; keep today's logic for the safe area, keyboard, reading-position restore and slider handle redraw. Do not change the plate placement rules (ReflowCommonPanel); change the look only.
3. Parts (collect them in one reusable class or helper, not copied per tab):
   - Card (white, edge, drop) and head row (icon gem, Jua title, value).
   - Bar-and-marker slider: keep the Slider and SettingsStepSlider behaviour (5% steps, wheel, drag, handle redraw) and change only the graphics. The bar has an ink outline with yellow, sky or green fill; the handle is a yellow diamond marker. The grab area must be at least today's handle.
   - Round −/+ buttons (consider reusing control-minus and control-plus first), choice chips with a ratio-shaped icon (a rectangle sized to the ratio, drawn with uGUI Image and no new art), choice cards (map display, language), check row, three notice boxes, pill, hero card.
   - Create each new graphic once and update it only when the state changes (AgentRules/UI_Refresh.md). UpdateCommonPanel keeps calling RefreshScreenSettings while the window is open (check the call rate in the code), so do not rewrite sprites, colours or text when the value is the same.
4. Rebuild the tab bodies in the mockup's order and composition: Screen (view distance → map display → screen ratio), Audio, Language, Character, Account, Game guide, Combat state. The name contract and what happens inside the body (what a press does, disabled conditions, save retry) stay as they are.
   - Map display is three choice cards with thumbnails (overlay map, corner minimap, hidden), the ratio chips carry ratio icons, and view distance has ticks (50, 100, 150%) with 100% marked.
   - Hero cards show only heroes that exist, in today's order (0,3,1,4,2,5). The seal is GlobalHudView.Portrait, the class name and level use existing text, gender is an icon, the current hero is lit with a check (still not pressable), and during a rift the others are locked with a padlock (CanChangeCharacterFromSettings is false).
5. Art:
   - The 25 icons come from Docs/Design/Options/Casual/assets/icons/*.svg. Make sprites with an existing vector generation approach such as Docs/Design/GlobalHUD/Resources/v1/build-vectors.cjs, or draw them as uGUI meshes (Decision 5). Hand-drawn raster art is forbidden.
   - Make the three map thumbnails the same way (uGUI shapes or vector sprites). The mockup's SVG shapes are the source of truth.
   - Add a row to Editor/ResourceTextureBudget.cs for any new texture and report the size change.
6. States (as in the reference screens): save failed (coral notice plus Retry the save, reusing the existing button), character change locked during a rift hunt, and no Character tab during the tutorial with Return to title and a notice at the top of the Account body.
7. Text: every string in the mockup is an existing key. The default is to create no new keys. If one is needed (for example the level pill "Lv.{0}"), look for an existing key first and add Korean and English together only when there is none. A missing key shows Korean and is recorded, and never an empty string or a key name. The mockup's view-distance save-failed line is a mockup assumption, so use the game's ViewDistance.Message as it is.

## Decisions (go with these defaults and report)
- Decision 1 — Size: keep text sizes and touch heights at today's code values (the same acceptance rule as the skill window: keep position, size, text size and function; change colour, line and corner). The mockup numbers direct composition and proportion. The composition changes the mockup allows are one row of tabs, cards, icons, thumbnails and hero cards. If 5 tabs cannot fit on one row on the narrowest portrait screen without shrinking text, wrap to two lines (keeping the text size); if even that fails, keep today's 2-column layout and report why and the actual width.
- Decision 2 — Slider: keep Slider and SettingsStepSlider and change only the graphics.
- Decision 3 — Plate shape: as in the mockup, a card floating 8 to 12 in from the edge. If it conflicts with the safe area, ContentWindowHost or the tap-the-background-to-close area, reduce the edge gap to 0 and report.
- Decision 4 — Real game on the left: keep today's ShowSettingsWorld and do not add the mockup's dark gradient (veil). If it is hard to read, use an alpha of 0.3 or less.
- Decision 5 — Making icons and thumbnails: build sprites from the SVG sources with an existing vector generation approach. Simple shapes (ratio icons, check, close) may be drawn with uGUI shapes.
- Decision 6 — Scrollbar: if the window has none today, do not add one (the mockup's yellow scrollbar is a treatment).

## Not to do
Change what a setting means, its save format or how it is applied; change the look of the gear buttons on the title, town and HUD (owned by the HUD task); touch the Hunt Edict options, gear recommendation settings or shop auto options windows; change the black bars of the screen ratio; change account, login or telemetry logic; add a new font (draw glyphs the bundled fonts lack as icons); bring back the removed text-size option or its 50 to 150% checks and enlarged-text smokes; switch the global UiTheme (other windows would be affected); hand-drawn art.

## Verification (only related checks while working)
- Related EditMode: run SettingsRevisionTests, CasualUiSkinTests, LocalizationTests and StoreViewBindingTests. Add tests for:
  - The one-row tab layout (5 tabs and 4 tabs) not overlapping and staying inside the safe area on five screen shapes (portrait 440×956, landscape 956×440, PC 1280×720, 1600×900, 21:9) in Korean and English.
  - The hero card check and padlock not overlapping the name, the UID row wrapping at a narrow width, and the three audio bars not overlapping (all three were real defects found while making the mockup).
  - Card, chip and choice-card colours coming only from UiSkin values (in the style of CasualUiSkinTests), and the selected, disabled and locked state colours.
  - The slider's 5% steps, wheel and −/+ unchanged, save-failed notice and retry, no Character tab during the tutorial, and the character change refused during a rift.
  - Move the rules of tools/smoke.js (tab order, language switch, mute fading the bars, retry clearing the notice) into EditMode.
- Smokes: fix the related smokes above for the new layout. If the name contract is unchanged most should pass as they are. Check real input (raycast) hits on close, tabs and the background, and the right-half landscape and full portrait layouts.
- Screens: capture portrait 440×956, landscape 956×440 and PC 1280×720 and 1600×900 in Korean and English. The 5 tabs plus game guide and combat state, and three states (save failed, during a rift, during the tutorial). Put each capture beside the same state in reference/ and compare. Check safe area, 44pt touch, overlap and text clipping. Phone ratios are Mac windows with simulated margins, so do not call them real-device results.
- Pass python3 tools/check_ui_contract.py --verify and python3 tools/check_ui_refresh.py, and follow the UI refresh rules (AgentRules/UI_Refresh.md).
- Run the full EditMode and the macOS development-build runtime smokes once, after the last code change and the latest main merge. Separate failures that already existed from new ones. If only documents change, do not rerun game execution checks.
- Performance: build the window once when it opens and do not rebuild every frame. A performance claim needs at least three real before and after binary runs under the same conditions with raw CPU and GC data. Without it write "not measured". Report any texture memory change as sizes.

## Deliverables
- One PR (codex/casual-options). Do not merge it; merging waits for the user's approval. Include the mockup package (Docs/Design/Options/Casual) and put the related checks and records in the same commit.
- Write the bilingual record Docs/Implementation/Casual_Options_Window_20261015(.en).md (what changed, the decision results, where it differs from the mockup, check results, what was not checked). Also fix the stale tab count in Settings_Revision(.en).md. Keep the evidence screenshots as jpg in Docs/Implementation/CasualOptionsEvidence/ and link each file.
- If the wiki source changes, follow AgentRules/Wiki_Publish.md; follow AgentRules/Artifact_Cleanup.md for cleanup.

## Report (Korean, short)
The actual changes, the results of Decisions 1 to 6, where it differs from the mockup and why, what was actually checked and what was not (real devices, performance), size changes and what is left. Do not present macOS results as real-device results.
```
