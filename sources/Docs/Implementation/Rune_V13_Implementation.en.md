# Rune board v13 implementation

갱신일: 2026-09-20 · [한국어](Rune_V13_Implementation.md) · [Current design](../Design/HELLSCRIPT_Rune_Mastery.en.md)

## Reference and implementation

The supplied v13 package now drives the actual Unity rune content. Its six boards contain exactly 259 slots each, in seven 37-slot regions. Coordinates, abilities, values, colors and tiers come directly from the reference. This replaces the generated 427-slot boards, clear-stage unlocking and grade-as-color placement. The [original archive](../Design/RuneV13/reference-v13.zip) and [provenance](../Design/RuneV13/provenance.json) are preserved. Operational instructions embedded in the archive were treated as reference content; publication follows the repository's standing authorization.

Each weapon starts at mastery level 1 with 19 open central slots, gains six points per level and expands from the connected open frontier. Complete the current region before choosing another. The outer region's free elite center is not a new connection origin. Five independent colors connect from the shared central origin; only matching colors activate normal abilities. A mismatched slot can relay a connection, and elites accept any color. All effects apply only while the corresponding weapon category is equipped.

Existing ownership, 34 PackBound shapes, G0–G6 loot and fusion are retained. Color is independent of acquisition grade. Fusion pairs matching colors first; unmatched mixed pairs roll each color at 20%. Five presets store all weapon layouts together, referencing physical rune IDs and never creating ownership. Preset edits remain drafts until Save Changes.

The reference did not implement combat mastery rewards. The initial integration awards 5/25/100 XP for normal/elite/boss kills, multiplied by the documented stage factor. This is a tuning choice, not a verified long-term progression curve.

## Native UI and combat

The existing UGUI screen was replaced with the reference's landscape weapon rail, board and storage arrangement, and its portrait weapon row, board and storage arrangement. Wide portrait windows use a bounded content width. Storage and detail bodies scroll independently while actions remain fixed. Six supplied weapon images and 328 SVG glyphs were extracted and converted into transparent runtime assets; no new AI-generated artwork was used. Shared internal edges are removed to draw solid blocks and continuous region boundaries.

The screen provides pan, zoom, fit, full map, a seven-region minimap, anchored region previews, view/edit modes, color and multi-size filters, per-color active effects, searchable codex with weapon/region choices, assigned-slot navigation, rotation, recovery, 50-step undo, revert and save. Korean, English, landscape, portrait and 140% interface size are checked. Unity and browser font rendering can differ. Reference screenshots use isolated fixture ownership; real new accounts do not receive the demo's 160 runes.

Conditional bonuses are consumed by existing combat paths: direct/basic/elite/area/multi-target damage, shielded damage, outgoing control duration, additional basic resource, potion cooldown and post-mobility protection. Skill radius, duration, target count and cooldown modifiers reach their actual skill execution. Direct damage excludes periodic, thorn and triggered hits. Multi-target damage requires three distinct enemies within one strike, not separate chain-lightning hops. Boss stagger does not increase with control duration; field-bound slows retain their existing exit/expiry rules. Exposed Weakness uses the reference's four seconds and 10%; Cascade uses 30% of the attack basis.

Changes are validated on a copy and applied to combat only after persistence succeeds. Loading a preset validates all layouts atomically. Missing runes are not duplicated and partial layouts are not accepted. Existing attack snapshots, actions, cooldowns, health and resources retain their preservation rules. Migration keeps owned IDs, grades, shapes, counts and pending fusion results, preserves previously earned region access, keeps legal placements and returns others to storage. Original layouts and presets remain archived in `legacyV1`.

## Verification

The [initial full run](RuneV13Evidence/editmode-initial-full.xml) passed 2,743 of 2,746 tests. Three failures were addressed: missing CanvasRenderer declarations, three missing translations and a test fixture that incorrectly assumed every ability could be reached using single-hex paths alone. Both the [affected regression run](RuneV13Evidence/editmode-fixed.xml) and [final affected run](RuneV13Evidence/editmode-final-focused.xml) passed **146/146**. These are separate runs, not additive coverage.

The **full rerun passed 2,747/2,747** ([report](RuneV13Evidence/editmode-full.xml)). After merging the latest main HUD and hunt-edict changes, **342/342 affected tests passed** ([integration report](RuneV13Evidence/editmode-merged.xml)). The merged macOS build and native workflow, including 14 screenshots, also passed. The full pre-integration run and post-integration affected run are separate scopes.

The [macOS development build](RuneV13Evidence/build.txt) and [native workflow](RuneV13Evidence/result.txt) passed. The workflow covers placement/save, cross-weapon recovery, slot-point draft/save, five preset drafts/file reload/load, undo, per-color effects and codex search/weapon/region choices. Korean, English and large text produced zero missing translations. After the last minimap sizing adjustment, the build and native workflow were repeated.

[Separate desktop input verification](RuneV13Evidence/manual-desktop.txt) used the actual game window: opening the content dock, switching weapons and selecting/recovering a rune by mouse changed storage 155→156 and active slots 6→5. Undo restored both; all five saved presets were visible. The [asset reproduction check](RuneV13Evidence/import-validation.txt) confirmed identical regenerated content and metadata. [RGBA and transparent-pixel validation](RuneV13Evidence/art-validation.json) also passed. [Source hashes](RuneV13Evidence/source-hashes.json) identify the verified files.

[Landscape](RuneV13Evidence/02-reference-landscape-ko.png) · [Portrait](RuneV13Evidence/04-reference-portrait-ko.png) · [English](RuneV13Evidence/08-reference-landscape-en.png) · [140% text](RuneV13Evidence/10-large-type-portrait-ko.png) · [Presets](RuneV13Evidence/03-global-presets-ko.png) · [Region preview](RuneV13Evidence/06-region-dialog-ko.png) · [Codex](RuneV13Evidence/12-codex-search-ko.png)

Native UI automation invokes UGUI buttons and pointer events. It is distinct from physical input testing. Physical mobile touch/pinch behavior, device performance, long-term progression and all equipment combinations are outside this macOS verification scope.

## Maintenance

[Runtime reference data](../../Assets/HELLSCRIPT/Resources/Runes/V13/catalog.json) is checked against the [Unity export](Rune_Mastery_Catalog.json) and wiki databases. The [importer](../../tools/runes/import_v13.py) checks the supplied archive hash without executing embedded scripts. The [art converter](../../tools/runes/render_v13.cjs) regenerates assets from supplied SVGs and images.

Separate modules own [progression](../../Assets/HELLSCRIPT/Runtime/Core/RuneMasteryProgress.cs), [placement rules](../../Assets/HELLSCRIPT/Runtime/Runes/RuneV13Validation.cs), [combat bonuses](../../Assets/HELLSCRIPT/Runtime/Core/RuneCombatBonuses.cs), [UI](../../Assets/HELLSCRIPT/Runtime/Presentation/GameUI.Runes.cs) and [native verification](../../Assets/HELLSCRIPT/Runtime/Presentation/RuntimeRuneV13Smoke.cs). Older smoke harnesses and reports remain historical evidence and are not presented as validation of v13.
