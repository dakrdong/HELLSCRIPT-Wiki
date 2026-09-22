# Legendary-power emblem production record

Updated: 2026-09-22

## Scope and authority

Produce 141 symbolic power emblems mapped to the legendary equipment IDs in the handoff. Game files use `aspect-ID.png`. This is an art task only: it introduces no extraction, acquisition, equipment, combat, scene, prefab or UI integration system.

The read-only handoff came from `Docs/Art/SkillExpansionBrief` in the `class-skill-runtime` workspace. Local copies live in `brief/`; current and previous SHA-256 values are in `source-provenance.json`. A mid-production handoff update was compared and adopted. IDs and names were unchanged; before generating DES_LM44, its trigger was updated to sustaining Mana Reclaim uninterrupted for at least 1.5s inside the caster's Rift Ward. Handoff IDs, bilingual names, descriptions and linked skills take precedence over older planning in this checkout.

## Visual direction and generation

The original project's `EquipmentAtlas.png` and the GlobalHUD icons `passive-WP04.png`, `passive-MP02.png` and `passive-AP02.png` were opened and inspected. Worn dark materials and bright edges inform the rendering; the HUD's clear silhouettes inform thumbnail readability. Existing icons are references only, with no copied or palette-swapped deliverables.

Each item receives a separate built-in `image_gen` request. The concepts distinguish triggers and results such as pull, recovery, root, defense and cooldown reduction through shape as well as color. `concepts.json` contains all 141 concepts; the manifest records the exact prompt for each request.

The requested default is `gpt-image-2`, but the callable interface exposes neither a model selector nor returned model provenance. Actual model identity is therefore **unknown**. Every generated image remains a **candidate asset**, even if its image QA passes. QA does not establish model identity or production adoption.

## Native masters and QA copies

Only native transparent PNG results are accepted. The updated handoff targets 1024px and also accepts native 1254px output. Each asset-folder `aspect-ID.png` and its archival copy in `native/` are byte-identical to the returned image. Actual dimensions, alpha measurements and matching source/asset SHA-256 values are in the [manifest](manifest.json). Display size and Unity import settings remain decisions for UI integration.

Only conservative QA copies in `qa/previews/` are normalized: crop empty alpha bounds, proportionally resample with premultiplied alpha to a maximum content dimension of 756px, then center on a transparent 1024px square canvas. This changes size and padding only and never replaces the delivered native master. No chroma keying, background removal, recoloring, repainting or duplicate-based variation is used. A result without genuine alpha remains incomplete. Early 1024px asset copies were replaced with native originals after the revised handoff was confirmed; subsequent normalized images are QA-only.

## Validation and status

The manifest covers every handoff ID with bilingual names and effects, linked skills, asset paths, concepts, prompts, generation attempts, provenance limits, reuse decisions, source and master hashes, automated alpha/padding checks and visual review. Per-item generation receipts are stored in `generation/`.

Automated checks cover native RGBA, fully transparent and opaque pixels, accepted square dimensions, byte identity with returned originals, complete unique IDs, duplicate file hashes and GUID collisions. Native border alpha is recorded without modification. Separate QA-copy checks cover 1024px dimensions, clear borders and content within the central 76%. Contact sheets in `qa/contact-*.png` show these conservative-fit copies at 192px alongside actual 64px composites on light and dark backgrounds. Visual review checks meaningful silhouette differences, readable actions and the absence of visible matte or unwanted borders.

New PNGs and their folder have unique GUIDs. GUIDs assigned earlier in this task remain unchanged after replacing asset copies with native masters. Initial provisional Sprite import settings were removed, leaving minimal metadata for UI integration to configure. Assets and metadata that predated this task were not changed. Unity import, game UI integration, macOS runtime and physical-mobile validation are not claimed by this art-only task.

Current counts are recorded in the [validation report](qa/validation.json) and [production progress](Class_Aspect_Icons_Progress.md). Generation, automated QA, visual review and model verification remain separate statuses. Main integration and public wiki deployment are outside this handoff's scope.

## Contact sheets

Each sheet pairs 192px previews with actual 64px emblems on light and dark backgrounds. Use the [production progress](Class_Aspect_Icons_Progress.md) for each ID's generation and review status.

| Sheet | Included IDs |
| --- | --- |
| [01](qa/contact-01.png) | LW01–04 · LA01–04 · LM01–04 |
| [02](qa/contact-02.png) | LC01–03 · LW05–13 |
| [03](qa/contact-03.png) | LW14–25 |
| [04](qa/contact-04.png) | LW26–37 |
| [05](qa/contact-05.png) | LW38–40 · LA05–13 |
| [06](qa/contact-06.png) | LA14–25 |
| [07](qa/contact-07.png) | LA26–37 |
| [08](qa/contact-08.png) | LA38–40 · LM05–13 |
| [09](qa/contact-09.png) | LM14–25 |
| [10](qa/contact-10.png) | LM26–37 |
| [11](qa/contact-11.png) | LM38–40 |
| 12 · Pending production | DES_LW41–46 · DES_LA41–46 · DES_LM41–46 |

## Re-running validation

From the repository root, run `python3 Docs/Art/ClassAspectIcons/prepare_assets.py --contacts`. It preserves native originals, prepares only missing or changed QA copies and refreshes contact sheets and the automated report. Existing metadata and reviews for unchanged originals are retained. When a regenerated source changes its hash, its previous visual review moves to history and the new original requires another review.

See [Class_Aspect_Icons.md](Class_Aspect_Icons.md) for the Korean production record.
