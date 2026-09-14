# Global HUD image-resource guide

Date: 2026-09-14

[Overview](HELLSCRIPT_GlobalHUD_Overview.en.md) · [Korean](HELLSCRIPT_GlobalHUD_Resources.md) · [ZIP](Resources/HELLSCRIPT-GlobalHUD-v1.zip)

## Import workflow

Originals remain archived in `Docs/Design/GlobalHUD/Resources/v1`. The game imports 67 PNGs into `Assets/HELLSCRIPT/Resources/Art/GlobalHUD` through the folder-scoped, idempotent `GlobalHudImporter`. The [v2 runtime profile](Resources/v2/layout-profile.json) supersedes the initial layout proposal. PNGs require no vector package.

1. Read the [manifest](Resources/v1/resource-manifest.json) and [runtime layout profile](Resources/v2/layout-profile.json).
2. Start with Sprite (2D and UI), Input Texture Alpha, Alpha Is Transparency, Bilinear, Clamp, mipmaps off, read/write off, Full Rect and PPU 100. Disable compression for initial review; compare compression on the target device later.
3. The importer uses a maximum size of 2048 and disables NPOT rescaling for all 67 assets. Preserve source files and use Unity import settings for device-specific reductions.
4. Existing SkillAtlas is 1536×1024 in six columns/four rows of 256×256 cells. Manifest rectangles use Unity bottom-left coordinates. At runtime cells use the imported texture dimensions, so existing atlas import rescaling does not change cell identity. The packaged copy is byte-identical; reuse the existing project texture rather than loading a duplicate.
5. Borders use left/bottom/right/top order: 14 for `frame-vital`, 10 for `fill-vital`. For sliced fills, adjust the parent width; a single Image cannot simultaneously be Sliced and Filled.
6. Compose new frame/content/cooldown layers separately. Existing active tiles already include borders; do not double-frame them. Center and scale the mage face inside a circular mask non-destructively.

## Contents and scope

68 PNG files comprise 4 painted sprites, 21 common elements, 24 semantic status glyphs, 18 passive icons and 1 reused atlas. The atlas supplies 18 actives and 3 legacy portraits. Semantic status glyphs may be shared across actual effects; passive filenames map 1:1 to existing definition IDs. These simple glyphs favor readability at small sizes. Connect real names/descriptions/source information to distinguish related effects. Use `unknown` for missing mappings and record them; artwork availability never creates a nonexistent player effect.

HP/MP/XP tint, timers, counts, level, stacks and descriptions remain dynamic using existing project fonts. No complete HUD backdrop, dark fade overlay or baked gameplay numbers are supplied. `mask-edge-smooth` is a white alpha lookup requiring an actual fade material; Mask/RectMask2D alone does not implement the gradient.

## Provenance and validation

Three bottles and the mage portrait were generated with built-in image_gen and copied without modifying PNG bytes. Metadata reports `softwareAgent.name=gpt-image`, `version=2.0`; the tool exposes no model-selection argument and signatures were not verified. Do not infer the old atlas's model. Existing usage follows [Asset Provenance](../../Implementation/Asset_Provenance.md). New paintings were generated for this project; no exclusivity or completed independent legal review is claimed.

Retain [prompts](Resources/v1/generation-prompts.json), [native IDs](Resources/v1/native-sources.json), [vector builder](Resources/v1/build-vectors.cjs), [validator](Resources/v1/validate-resources.py), [pixel QA](Resources/v1/resource-qa.json) and [vector list](Resources/v1/vector-manifest.json). Checks cover real alpha, transparent pixels, dimensions, rectangles and hashes. Rendering was verified in a Unity 6000.6 macOS development player. Mobile-device touch and compression quality remain release checks.

Open the [gallery](Resources/v1/gallery.html) locally or from a static folder server. Public wiki HTML attachments may be displayed as source text; use PNG links or the ZIP there.

## File inventory

| ID | Category | Size | PNG | Editable source |
|---|---|---|---|---|
| frame-seal | common | 256×256 | [PNG](Resources/v1/png/frame-seal.png) | [SVG](Resources/v1/svg/frame-seal.svg) |
| mask-circle | mask | 256×256 | [PNG](Resources/v1/png/mask-circle.png) | [SVG](Resources/v1/svg/mask-circle.svg) |
| mask-square | mask | 256×256 | [PNG](Resources/v1/png/mask-square.png) | [SVG](Resources/v1/svg/mask-square.svg) |
| mask-diamond | mask | 256×256 | [PNG](Resources/v1/png/mask-diamond.png) | [SVG](Resources/v1/svg/mask-diamond.svg) |
| frame-active | common | 256×256 | [PNG](Resources/v1/png/frame-active.png) | [SVG](Resources/v1/svg/frame-active.svg) |
| frame-passive | common | 256×256 | [PNG](Resources/v1/png/frame-passive.png) | [SVG](Resources/v1/svg/frame-passive.svg) |
| frame-status | common | 256×256 | [PNG](Resources/v1/png/frame-status.png) | [SVG](Resources/v1/svg/frame-status.svg) |
| plate-icon | common | 256×256 | [PNG](Resources/v1/png/plate-icon.png) | [SVG](Resources/v1/svg/plate-icon.svg) |
| frame-vital | common | 512×48 | [PNG](Resources/v1/png/frame-vital.png) | [SVG](Resources/v1/svg/frame-vital.svg) |
| fill-white | common | 16×16 | [PNG](Resources/v1/png/fill-white.png) | [SVG](Resources/v1/svg/fill-white.svg) |
| mask-edge-smooth | mask | 256×8 | [PNG](Resources/v1/png/mask-edge-smooth.png) | [SVG](Resources/v1/svg/mask-edge-smooth.svg) |
| fill-vital | common | 512×32 | [PNG](Resources/v1/png/fill-vital.png) | [SVG](Resources/v1/svg/fill-vital.svg) |
| badge-level | common | 192×64 | [PNG](Resources/v1/png/badge-level.png) | [SVG](Resources/v1/svg/badge-level.svg) |
| control-plus | control | 64×64 | [PNG](Resources/v1/png/control-plus.png) | [SVG](Resources/v1/svg/control-plus.svg) |
| control-minus | control | 64×64 | [PNG](Resources/v1/png/control-minus.png) | [SVG](Resources/v1/svg/control-minus.svg) |
| badge-buff | common | 32×32 | [PNG](Resources/v1/png/badge-buff.png) | [SVG](Resources/v1/svg/badge-buff.svg) |
| badge-debuff | common | 32×32 | [PNG](Resources/v1/png/badge-debuff.png) | [SVG](Resources/v1/svg/badge-debuff.svg) |
| xp-tick | common | 8×16 | [PNG](Resources/v1/png/xp-tick.png) | [SVG](Resources/v1/svg/xp-tick.svg) |
| xp-tick-major | common | 8×24 | [PNG](Resources/v1/png/xp-tick-major.png) | [SVG](Resources/v1/svg/xp-tick-major.svg) |
| xp-cap | common | 16×20 | [PNG](Resources/v1/png/xp-cap.png) | [SVG](Resources/v1/svg/xp-cap.svg) |
| portrait-placeholder | common | 256×256 | [PNG](Resources/v1/png/portrait-placeholder.png) | [SVG](Resources/v1/svg/portrait-placeholder.svg) |
| status-shield | status | 256×256 | [PNG](Resources/v1/png/status-shield.png) | [SVG](Resources/v1/svg/status-shield.svg) |
| status-power | status | 256×256 | [PNG](Resources/v1/png/status-power.png) | [SVG](Resources/v1/svg/status-power.svg) |
| status-poison | status | 256×256 | [PNG](Resources/v1/png/status-poison.png) | [SVG](Resources/v1/svg/status-poison.svg) |
| status-lightning | status | 256×256 | [PNG](Resources/v1/png/status-lightning.png) | [SVG](Resources/v1/svg/status-lightning.svg) |
| status-leaf | status | 256×256 | [PNG](Resources/v1/png/status-leaf.png) | [SVG](Resources/v1/svg/status-leaf.svg) |
| status-sword | status | 256×256 | [PNG](Resources/v1/png/status-sword.png) | [SVG](Resources/v1/svg/status-sword.svg) |
| status-frost | status | 256×256 | [PNG](Resources/v1/png/status-frost.png) | [SVG](Resources/v1/svg/status-frost.svg) |
| status-hourglass | status | 256×256 | [PNG](Resources/v1/png/status-hourglass.png) | [SVG](Resources/v1/svg/status-hourglass.svg) |
| status-flame | status | 256×256 | [PNG](Resources/v1/png/status-flame.png) | [SVG](Resources/v1/svg/status-flame.svg) |
| status-eye | status | 256×256 | [PNG](Resources/v1/png/status-eye.png) | [SVG](Resources/v1/svg/status-eye.svg) |
| status-wind | status | 256×256 | [PNG](Resources/v1/png/status-wind.png) | [SVG](Resources/v1/svg/status-wind.svg) |
| status-heart | status | 256×256 | [PNG](Resources/v1/png/status-heart.png) | [SVG](Resources/v1/svg/status-heart.svg) |
| status-rune | status | 256×256 | [PNG](Resources/v1/png/status-rune.png) | [SVG](Resources/v1/svg/status-rune.svg) |
| status-chain | status | 256×256 | [PNG](Resources/v1/png/status-chain.png) | [SVG](Resources/v1/svg/status-chain.svg) |
| status-broken | status | 256×256 | [PNG](Resources/v1/png/status-broken.png) | [SVG](Resources/v1/svg/status-broken.svg) |
| status-unknown | status | 256×256 | [PNG](Resources/v1/png/status-unknown.png) | [SVG](Resources/v1/svg/status-unknown.svg) |
| status-whirl | status | 256×256 | [PNG](Resources/v1/png/status-whirl.png) | [SVG](Resources/v1/svg/status-whirl.svg) |
| status-arrow | status | 256×256 | [PNG](Resources/v1/png/status-arrow.png) | [SVG](Resources/v1/svg/status-arrow.svg) |
| status-trap | status | 256×256 | [PNG](Resources/v1/png/status-trap.png) | [SVG](Resources/v1/svg/status-trap.svg) |
| status-mana | status | 256×256 | [PNG](Resources/v1/png/status-mana.png) | [SVG](Resources/v1/svg/status-mana.svg) |
| status-target | status | 256×256 | [PNG](Resources/v1/png/status-target.png) | [SVG](Resources/v1/svg/status-target.svg) |
| status-elements | status | 256×256 | [PNG](Resources/v1/png/status-elements.png) | [SVG](Resources/v1/svg/status-elements.svg) |
| status-focus | status | 256×256 | [PNG](Resources/v1/png/status-focus.png) | [SVG](Resources/v1/svg/status-focus.svg) |
| status-people | status | 256×256 | [PNG](Resources/v1/png/status-people.png) | [SVG](Resources/v1/svg/status-people.svg) |
| passive-WP01 | passive | 256×256 | [PNG](Resources/v1/png/passive-WP01.png) | [SVG](Resources/v1/svg/passive-WP01.svg) |
| passive-WP02 | passive | 256×256 | [PNG](Resources/v1/png/passive-WP02.png) | [SVG](Resources/v1/svg/passive-WP02.svg) |
| passive-WP03 | passive | 256×256 | [PNG](Resources/v1/png/passive-WP03.png) | [SVG](Resources/v1/svg/passive-WP03.svg) |
| passive-WP04 | passive | 256×256 | [PNG](Resources/v1/png/passive-WP04.png) | [SVG](Resources/v1/svg/passive-WP04.svg) |
| passive-WP05 | passive | 256×256 | [PNG](Resources/v1/png/passive-WP05.png) | [SVG](Resources/v1/svg/passive-WP05.svg) |
| passive-WP06 | passive | 256×256 | [PNG](Resources/v1/png/passive-WP06.png) | [SVG](Resources/v1/svg/passive-WP06.svg) |
| passive-AP01 | passive | 256×256 | [PNG](Resources/v1/png/passive-AP01.png) | [SVG](Resources/v1/svg/passive-AP01.svg) |
| passive-AP02 | passive | 256×256 | [PNG](Resources/v1/png/passive-AP02.png) | [SVG](Resources/v1/svg/passive-AP02.svg) |
| passive-AP03 | passive | 256×256 | [PNG](Resources/v1/png/passive-AP03.png) | [SVG](Resources/v1/svg/passive-AP03.svg) |
| passive-AP04 | passive | 256×256 | [PNG](Resources/v1/png/passive-AP04.png) | [SVG](Resources/v1/svg/passive-AP04.svg) |
| passive-AP05 | passive | 256×256 | [PNG](Resources/v1/png/passive-AP05.png) | [SVG](Resources/v1/svg/passive-AP05.svg) |
| passive-AP06 | passive | 256×256 | [PNG](Resources/v1/png/passive-AP06.png) | [SVG](Resources/v1/svg/passive-AP06.svg) |
| passive-MP01 | passive | 256×256 | [PNG](Resources/v1/png/passive-MP01.png) | [SVG](Resources/v1/svg/passive-MP01.svg) |
| passive-MP02 | passive | 256×256 | [PNG](Resources/v1/png/passive-MP02.png) | [SVG](Resources/v1/svg/passive-MP02.svg) |
| passive-MP03 | passive | 256×256 | [PNG](Resources/v1/png/passive-MP03.png) | [SVG](Resources/v1/svg/passive-MP03.svg) |
| passive-MP04 | passive | 256×256 | [PNG](Resources/v1/png/passive-MP04.png) | [SVG](Resources/v1/svg/passive-MP04.svg) |
| passive-MP05 | passive | 256×256 | [PNG](Resources/v1/png/passive-MP05.png) | [SVG](Resources/v1/svg/passive-MP05.svg) |
| passive-MP06 | passive | 256×256 | [PNG](Resources/v1/png/passive-MP06.png) | [SVG](Resources/v1/svg/passive-MP06.svg) |
| potion-hp | potion | 1254×1254 | [PNG](Resources/v1/png/potion-hp.png) | — |
| potion-mp | potion | 1254×1254 | [PNG](Resources/v1/png/potion-mp.png) | — |
| potion-utility | potion | 1254×1254 | [PNG](Resources/v1/png/potion-utility.png) | — |
| portrait-mage | portrait | 1254×1254 | [PNG](Resources/v1/png/portrait-mage.png) | — |
| skill-atlas-existing | reused | 1536×1024 | [PNG](Resources/v1/png/skill-atlas-existing.png) | — |
