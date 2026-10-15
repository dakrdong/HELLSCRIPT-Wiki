# Casual skin mockup for the skill window

Updated: 2026-10-14

[한국어](README.md)

The game's art concept moved from dark gothic ("Obsidian & Gilt") to a bright, cute, casual medieval fantasy for ages 12+ (style D; see the [concept record](../../../Art/CasualConcept/2026-10-13/Casual_Art_Concept_20261013.en.md)). This folder holds the **skill window (`../SkillWindow.dc.html`) with the same composition in the new look**: rules, layout, copy and interactions are unchanged; only colours, fonts, borders, shadows and decorations differ.

- Design canvas (approved by the user): https://claude.ai/artifact/JsCKnJUbKkzfBYyJDR3493 (24 boards: 3 classes × portrait / landscape / PC × KO / EN, plus the passives view)
- The original dark mockup is `../SkillWindow.dc.html`, identical to commit `86cfeff1`.

## Files

| File | Content |
|---|---|
| `SkillWindow.casual.locked.dc.html` | **The reference for the game.** The approved canvas mockup with the few layout-affecting font sizes and the rank-badge size reverted to the original, so every rectangle and font size equals the original. With this file as the mockup, `golden/layout`, `viewmodel`, `interaction` and `rules_golden` must regenerate unchanged. |
| `SkillWindow.casual.dc.html` | The mockup exactly as the user approved it on the canvas. Titles, tabs, panel names, section headings, buttons and badge text are slightly larger than the original (table below). Whether the game adopts those sizes is a separate user decision. |
| `tools/restyle.py` | Original → casual transform (every replacement count-checked, so it fails loudly if the original changes). |
| `tools/lock_geometry.py` | Casual → locked transform (font sizes reverted). |
| `tools/geo_compare.js` | Runs both mockups through 36 combinations (3 classes × 3 layouts × 2 views × 2 languages) and lists rectangle and font-size differences. `node tools/geo_compare.js <original> <locked>` must report 0 font-size differences. |

## What changes visually

- Background: a sky-blue to cream gradient with clouds. Plates are cream (`#fff6e3`) with a 3 px ink (`#2a3150`) outline, a hard bottom shadow (`0 5px 0`), a dashed stitch inset and a round gem on top. The blurred art backdrop (`artWrap`) read as a smudge and was removed.
- Type: Jua for titles, tabs, numbers, buttons and sub-headings; Noto Sans KR 700/900 for body text. Titles are yellow with an 8-direction ink outline.
- Colour roles: invested, selected and key passives `#ffd23f / #ffc21a / #ff9f1c` (text `#e08a00`); passives and info `#35a7ff / #2f8fd8 / #1f6fb5`; available `#8a93b5`; locked `#e6e9f0` dashed; warnings `#ff5a5f`; invest button green `#8ee07a → #4cbf55`; close button coral.
- Lines: 3 px, lit lines get a 7 px ink under-stroke; locked lines are pale grey dashes.
- Skill icons (the sprite sheets) are unchanged.

## Canvas mockup vs locked file (layout-affecting values)

| Element | Canvas mockup | Locked (original size) |
|---|---|---|
| Window title (landscape / portrait / PC) | 28 / 31 / 30 | 24 / 27 / 26 |
| Class tab text (landscape + PC / portrait) | 15 / 16 | 13 / 14 |
| Buttons (refund, invest) text | 16 | 13 |
| Panel name (landscape + PC / portrait) | 18 / 22 | 16 / 19 |
| Panel sub-heading (landscape + PC / portrait) | 14 / 16 | 12.5 / 14.5 |
| Tier badge numeral (first 3 / others) | 14 / 12 | 13 / 11 |
| Tier badge name (landscape / portrait) | 13 / 14 | 12 / 13 |
| Level and points-spent values | 15 | surrounding size |
| Rank badge circle | 18×18 | 16×16 |

Some interior values still differ in the locked file (padding of the passive medal ring, rank-badge line height, right-hand chip padding of panel modifier rows). They are inside elements, not element rectangles.

## Rebuilding

```bash
python3 -I tools/restyle.py <original SkillWindow.dc.html> SkillWindow.casual.dc.html
python3 -I tools/lock_geometry.py SkillWindow.casual.dc.html SkillWindow.casual.locked.dc.html
node tools/geo_compare.js <original> SkillWindow.casual.locked.dc.html   # 0 font-size differences
```
