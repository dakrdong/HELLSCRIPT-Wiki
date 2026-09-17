# HELLSCRIPT Hunt Edict — Shared UI resources

작성일: 2026-09-15

This is the original approved resource-pack guide. Its package paths describe the original delivery structure. Game PNGs have now been copied to `Assets/HELLSCRIPT/Resources/Art/HuntEdict/`; the Hunt Edict Game UI implementation record describes the current integration status.

This pack covers the 119 non-skill options and the preset/share interface using the approved ink-blue, charcoal, aged-brass and jade visual language.

## Contents

- 24 opaque UI skins in one 1536 × 1024 atlas: Textures/edict-ui-skins.png.
- 24 existing tab/action icons, each as a transparent PNG and editable SVG in Glyphs/.
- 8 checkbox/switch/slider parts, each as a transparent PNG and editable SVG in Controls/.
- 33 game PNG files representing 56 reusable resources. Preview artwork is additional documentation.
- index.html provides a local 9-slice preview and icon gallery.
- resource-manifest.json contains English/Korean labels, paths, slice rectangles and text colors.
- unity-sprite-rects.csv uses Unity bottom-left coordinates.
- generation-prompt.txt records the built-in generation prompt; Licenses/LUCIDE.txt covers the reused icon geometry.

## Tab coverage

| Tab | Options | Components |
| --- | ---: | --- |
| Combat | 24 | Disclosure headers, dropdown/input wells, chips, sliders, policy switches, order cards and drag grip |
| Survival | 27 | Disclosure headers, HP values/sliders, policy switches, skill choices and survival order |
| Loot | 19 | Rarity/slot filters, checkboxes, value fields and exception switches |
| Bag/cleanup | 18 | Cleanup choices, protection checks/lock, free-slot values and failure notices |
| Exploration | 19 | Discovery order cards, chest/shrine headers, conditions and time/distance values |
| Repeat hunt | 12 | Repeat policy switch, stop conditions, numeric fields and preparation choices |
| Presets/share | 5 slots | Beige stored preset, green current preset, empty slot, code input/copy/import and overwrite dialog |

Policy switches are for ordinary settings. Do not add master edict activation or skill auto-use switches.

## Atlas order

Six columns, left to right, then the next row:

1. Main panel; option card; input well; dialog; collapsed header; expanded header.
2. Secondary button; hover; pressed; disabled; primary button; primary hover.
3. Order card; dragging card; drop target; stored preset; current preset; empty preset.
4. Inactive tab; selected tab; unchecked chip; checked chip; empty selection slot; error/overwrite notice.

The skins are opaque rectangular material surfaces. The 32 individual icon and control exports preserve native alpha.

## Unity import

Use the existing uGUI components and localization path. This delivery does not automatically import files or connect game screens.

Atlas: Sprite (2D and UI), Multiple, Full Rect, Bilinear, Clamp, mipmaps off, NPOT scaling None, maximum size at least 2048, preserve 1536 × 1024, use uncompressed during initial visual review. Apply the named rectangles from unity-sprite-rects.csv. Border is 32px on all sides; Pixels Per Unit is 400; Image Type is Sliced. Keep Image.color white for the precolored skins.

Do not use automatic 256 × 256 grid slicing: the generated artwork's row boundaries differ slightly. The measured rectangles avoid neighboring-frame bleed. No cropping or re-encoding of the atlas is required. With a Canvas Reference Pixels Per Unit of 100, a 32px border occupies 8 UI units.

Individual PNGs: Sprite (2D and UI), Single, Alpha Is Transparency on, Bilinear, Clamp, mipmaps off. Tab/action glyphs are white at 128 × 128 for Image.color tinting. Controls are precolored; keep their tint white. Preserve icon/thumb aspect ratios. Drive switch thumb positions, checkbox state and slider fill through the real UI components.

Render text, values, preset names and slot numbers through the existing Korean/English localization and data paths, not inside sprite pixels.

## States and validation

Current preset is green; other occupied presets are beige. Use #211B14 text on the beige preset, #FFFFFF on the bright primary hover state and #E8E3D4 by default. The manifest provides per-skin text colors. Collapsed headers are slate-blue, expanded headers dark bronze. Dragging and drop targets have separate skins; the existing handler remains responsible for ordering.

Preserve fixed navigation, fixed skill detail placement, compact floating undo/save actions, unsaved-change guards and overwrite confirmation.

The skin atlas was generated with Codex built-in image_gen; its PNG metadata identifies gpt-image / 2.0. Native icons reuse Lucide 1.8.0 geometry. Simple UI control shapes were authored as vectors and directly rasterized with native alpha, without background removal.

Validation covered 24 slice bounds and browser 9-slice rendering, 32 RGBA files with transparent and antialiased pixels, image loading, browser errors and sampled central text contrast. This is not Unity runtime/device/input validation or a full accessibility certification. Scenes, prefabs and game saves were not modified.
