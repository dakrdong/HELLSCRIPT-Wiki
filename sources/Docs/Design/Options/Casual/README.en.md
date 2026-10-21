# Options (Settings) Window — Casual Mockup Source of Truth

Written: 2026-10-15 · [한국어](README.md)

This redraws the shared settings window (the one the gear button opens: Screen, Audio, Language, Character and Account tabs, plus the game guide and the combat state) in the casual StyleD art concept. Structure and order follow the current game (`GameUI.ScreenSettings` and friends). This folder is mockup material and changes no game code. The brief for applying it to the game is the [Codex task prompt](../../../Implementation/Casual_Options_Window_Codex_Prompt_20261015.en.md).

- Design canvas (private, 24 boards): https://claude.ai/artifact/ARTvBnCWdj5yPkCovn6XFo. If it cannot be opened, the copy in this folder is the source of truth.
- Other mockups in the same look: potion and retreat settings (Pejw6sDmiP48Aa3ewfneYx, the origin of the tab, slider, check row and confirm button looks), skill tree (JsCKnJUbKkzfBYyJDR3493), battle HUD and graphs (PK5gEQdR1rrbW8Jzx3nDaA, the hero seals).
- "Options" here means the shared settings window. Hunt Edict options (`HuntEdictWindow.Options`), the gear recommendation settings (`RiftGearSettingsWindow`) and the shop auto options (`EquipmentShopWindow.Settings`) are other screens and are not in this mockup.

2026-10-15 implementation: the user rejected the SVG quality and requested freshly drawn icons and map thumbnails. `assets/icons/*.svg` is retained only as mockup history. The game uses PNG derivatives of the native generated originals in `assets/generated/`. See [generation requests and model limitation](assets/image-generation.json), [runtime derivatives](assets/sprite-manifest.json), and [implementation evidence](../../../Implementation/Casual_Options_Window_20261015.en.md).

## Files

| Path | Contents |
|---|---|
| `canvas/Main.dc.html` | One component that draws every board (it is also the PC Korean Screen-tab board). Props `locale` (ko, en), `orient` (pc, landscape, portrait), `tab` (screen, sound, language, character, account, help, combat), `state` (normal, saveError, riftLock, tutorial). The CSS variables own the measurements and all text lives in the script's `T` and `C` objects |
| `canvas/*.dc.html`, `canvas/canvas.json` | 23 thin wrapper boards and the canvas index (board positions, notes) |
| `reference/*.jpg` | 24 reference screens. PC is 1600×900 at 1×; landscape (956×440) and portrait (440×956) are scaled to 1.5× |
| `assets/icons/*.svg` | The 25 icons the mockup uses (24×24, ink stroke). The bundled fonts have no ♂♀ glyphs, so `gender-male` and `gender-female` are included |
| `tools/smoke.js` | Mockup rules check (`node tools/smoke.js`): every bound value, tab, language, state and input rule. It is the source for the rules to move into game tests |

The `/_blob/<id>` pictures in the mockup source are these files. Only the originals are kept in the repository; there are no copies.

| Picture in the source | File |
|---|---|
| `/_blob/8833523329ad04766c5398e05a59fa28`, `fee9c3bcda90fadb0eaf213816788bb8`, `37337ab3f01c4668070c64c375812c54`, `25442f46b2abb08289b90e9b6e391992` | `Resources/Art/GlobalHUD/Casual/seal-warrior-m`, `-warrior-f`, `-mage-m`, `-ranger-f` (the mockup uses 192 px versions) |
| `/_blob/44675b99fef4381d2039ef2190311be1` | `Docs/Art/CasualConcept/2026-10-13/Town/town_overview_gameview_v01.png` (the mockup's left backdrop, 1280 px version). The game shows the real town or hunting ground there |

## Reference screens

| File | State |
|---|---|
| `PC-Screen-KO`, `PC-Screen-EN` | PC Screen tab (view distance, map display, screen ratio; the rest scrolls) in Korean and English |
| `PC-Sound-KO`, `PC-Language-KO`, `PC-Character-KO`, `PC-Account-KO` | PC Audio, Language, Character and Account tabs |
| `PC-Help-KO`, `PC-Combat-KO` | Game guide and combat state (only the title changes; no tab is selected) |
| `PC-SaveError-KO` | Save failed: coral notice plus Retry the save |
| `PC-Rift-KO` | During a rift hunt: the other character cards are locked, coral notice |
| `PC-Tutorial-KO` | During the tutorial: no Character tab; Return to title plus a notice at the top of the Account tab |
| `Land-Screen-KO`, `Land-Sound-KO`, `Land-Character-KO`, `Land-Language-EN`, `Land-Account-EN` | Landscape 956×440 (tab labels only, no icons) |
| `Port-Screen-KO`, `Port-Sound-KO`, `Port-Language-KO`, `Port-Character-KO`, `Port-Account-KO`, `Port-Help-KO`, `Port-Screen-EN`, `Port-Rift-EN` | Portrait 440×956 (tab icon above the label) |

- The references were taken in Chrome on this Mac with the fonts bundled in the game (Jua and Noto Sans KR subsets). The ♂♀ signs were drawn by a system font fallback.
- The Screen and Account bodies are long and scroll; the references show only the top. Landscape boards have a short body, so only the first card shows.
- The scrollbar in the body (yellow thumb with a navy outline) is a mockup treatment.

## Look

The same rules as the other casual mockups.

- **Plate:** cream `#fff6e3` with a white sheen at the top, navy outline `#2a3150` 3 px, drop `0 5px 0 ink` plus `0 14px 24px rgba(42,49,80,.22)`, a `2px dashed rgba(42,49,80,.13)` stitch 5 px inside, a round yellow gem on the top edge (diameter 18, hidden in landscape). Corners 26 (PC), 18 (landscape), 24 (portrait). The plate floats 12 (PC) or 8 (landscape, portrait) in from the screen edge.
- **Title:** Jua, yellow `#ffd23f` with an 8-direction ink outline and a drop, centred. The close button is a coral round button at the top right (`#ff8b8e→#ff5a5f`, ink 3 px, white ×).
- **Tabs:** white `#ffffff`, edge `#8a93b5` 2 px, drop `0 2px 0 #8a93b5`, Jua. Selected is the yellow gradient `#ffe26b→#ffc21a` with ink 3 px and a yellow diamond tail below. Tabs sit on one row at equal width (5, or 4 during the tutorial). PC puts icon and label side by side, portrait puts the icon above the label, landscape shows the label only.
- **Cards (`sec`):** white, edge `#c9d0e2` 2 px, corners 16, drop `0 2px 0 #c9d0e2`. The head row is an icon gem (rounded white square with an ink ring and a coloured icon), a Jua title and a value on the right.
- **Slider:** the bar-and-marker of the potion window. The bar has an ink 2 px edge and a top sheen; the fill is a yellow (view distance, master volume), sky (music) or green (effects) gradient; the position is two diamonds with a centre line. The value is Jua on the right (orange `#c46d00`, sky `#1f6fb5`, green `#26722f`). View distance has round −/+ buttons (white, ink 2 px), ticks at 50, 100 and 150%, and a tick on the bar at 100%.
- **Choice chips (screen ratio):** the same white and yellow states as the tabs. Each chip draws a small rectangle in the ratio's shape (long side 22 px).
- **Choice cards (map display, language):** white card; selected is a yellow-to-white horizontal gradient with ink 2 px, a drop and a yellow check circle on the right. Map display has a mini battlefield thumbnail on the left (overlay map, corner minimap, hidden); language has a blue EN or orange 한 badge.
- **Hero card:** seal (round, ink ring), Jua class name, gender mark and a level pill. The current character looks like a selected card with a check at the bottom right; a card locked during a rift is grey with a padlock.
- **Check row (mute all):** the check-box row of the potion window. Turning it on lights the card yellow and fades the three volume bars.
- **Buttons:** white (`#ffffff`, ink 2 px, drop) is the default, confirm is the green gradient `#8ee07a→#4cbf55` with white text and an ink outline, Retry the save is coral. Heights are 56 (PC), 50 (portrait), 40 (landscape).
- **Notice boxes:** info is light blue (`#eef7ff`, edge `#bfe0ff`, text `#1f4f7d`), warning and lock are light coral (`#fff0f0`, edge `#ff5a5f`, text `#8c2327`), guide notes are light yellow (`#fff9dc`, edge `#ffd23f`).
- **Fonts:** titles, tabs, numbers, buttons and card titles in Jua; body in Noto Sans KR 400. Sizes are in the measurement table below.
- **Colour summary:** ink `#2a3150`, cream `#fff6e3`, yellow `#ffd23f`, `#ffc21a`, `#ffe26b`, orange `#ff9f1c`, sky `#35a7ff`, green `#4cbf55`, `#8ee07a`, coral `#ff5a5f`, `#ff8b8e`, mint `#5ee0d4`, empty bar `#e6e9f0`, secondary text `#4a5272`, `#6b7393`. Small orange text is darkened to `#b86200` or `#c46d00` for contrast on white.

## Measurements (mockup px)

| Element | PC 1600×900 | Landscape 956×440 | Portrait 440×956 |
|---|---|---|---|
| Plate edge gap / inner padding | 12 / 22 | 8 / 12 | 8 / 14 |
| Card padding / gap | 18 / 14 | 11 / 8 | 14 / 12 |
| Title / card title / choice-card title | 32 / 22 / 19 | 20 / 15.5 / 14 | 25 / 19 / 16.5 |
| Body / small text | 15.5 / 13.5 | 12.5 / 11.5 | 14 / 12.5 |
| Tab height / text | 56 / 18 | 36 / 13 | 62 / 12.5 |
| Chip / button height | 48 / 56 | 36 / 40 | 44 / 50 |
| Bar height / −+ button / close | 26 / 46 / 46 | 18 / 36 / 36 | 22 / 42 / 42 |
| Icon gem / seal / hero card height / thumbnail height | 40 / 62 / 88 / 72 | 28 / 40 / 58 / 44 | 34 / 52 / 76 / 56 |
| Plate position | right half (game view on the left) | right half | whole screen |

The numbers are in board pixels. For the text size and touch height rule when porting to the game, follow Decision 1 in the Codex task prompt.

## Tab bodies (the order is the game's order)

1. **Screen:** view distance (−/+, bar, ticks, saved line) → map display (3 cards, saved line) → screen ratio (Fit display chip, 5 landscape ratios, 5 portrait ratios, current ratio, saved line).
2. **Audio:** audio settings card (master, music and effects bars) → Mute all audio check row → Preview sound effect and Restore default volumes buttons → notice. When saving failed, a coral notice with Retry the save sits above the bars.
3. **Language:** English and 한국어 choice cards (choosing one switches the whole mockup to that language).
4. **Character:** Return to title button and its description → notice box → cards for the heroes that exist (warrior male and female, mage male and female, ranger male and female). During a rift a warning box shows, only the current hero stays lit and the rest are locked.
5. **Account:** (during the tutorial, Return to title plus a warning at the top) → UID row with the copy button → Link Google card (pending-uploads pill, confirm button) → three notice boxes.
6. **Game guide:** one-line intro, six numbered-gem cards and two yellow notes. **Combat state:** rift stage, HP, resource and shield bars, current behavior, speed, kills and gauge, skill rows (state pills), chests and shrines.

## What the mockup proposes differently from the game (a decision is needed to apply it)

- Five tabs on one row (the game puts 5 tabs on two rows below a width of 620).
- A card floating 8 to 12 in from the screen edge with a centred title (the game fills the plate to the edge with a left-aligned title).
- Tab icons, icon gems on card heads, ratio-shaped icons on the ratio chips, map-display thumbnails, view-distance ticks with the default marked, seal and level pills on hero cards and badges on the language cards are new elements.
- Hero gender is drawn with `assets/icons/gender-*.svg` because the bundled fonts have no ♂♀.

## What was checked

- `node tools/smoke.js` checks every bound value and the tab, language, state and input rules (3 orientations × 2 languages × 7 tabs × 4 states).
- All 24 boards were rendered in Chrome and looked at; the audio bar overlap, the UID row and the seal badge overlap were fixed.
- The canvas host, touch, a real device, keyboard avoidance and how it looks in the game were not checked. It is not applied to the game.
