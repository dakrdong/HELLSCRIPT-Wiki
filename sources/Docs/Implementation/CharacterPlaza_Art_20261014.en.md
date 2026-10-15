# Character Plaza art handoff

2026-10-14 · [한국어](CharacterPlaza_Art_20261014.md)

The [design README](../Design/CharacterPlaza/README.md) and [source art README](../Art/CasualConcept/2026-10-13/CharacterPlaza/README.md) remain authoritative. This handoff covers source assets and static checks. Unity import, actual presentation, interaction and memory measurements await the single integration run.

## Delivered files and provenance

`Assets/HELLSCRIPT/Resources/Art/CharacterPlaza/` contains 41 PNG textures, two scene JSON files and `art.json`. Two background PNGs and twelve pose PNGs preserve their original bytes. Twelve subject cutouts, twelve clean-plate patches and three crest crops decode the existing authored WebP pixels into PNG. Full clean plates and the obsolete equipment atlas are excluded from Resources.

Run [prepare_character_plaza_art.py](../../tools/prepare_character_plaza_art.py) with `--from-webp` to reproduce this delivery, or `--verify` for read-only checks. The [receipt](../../tools/character_plaza_art_receipt.json) records all original/output SHA256 values, paths, GUIDs and processing.

Native regeneration was attempted first. `lift.swift` compiled successfully, but the first landscape `warrior_m` extraction failed with macOS Vision Code 9: `Could not build inference plan / failed to load ANE model`. Portrait extraction was not started. The authorized authored-WebP fallback was then used. PNG decoding cannot recover detail already lost in WebP compression; check cutout/background edge seams in Unity. No new image generation occurred.

## Runtime contract

Scene coordinates use a top-left origin. `scene_landscape` and `scene_portrait` provide `width`, `height`, `background`, `heroes[]` and `focus[]`. Each hero contains `artId`, resource and x/y/width/height in `cut` and `patch`, `headX/Y`, `feetX/Y` and `cheerHeight`. All twelve hero rectangles, anchors and portrait focus entries match authored `scene_data.json` exactly. Art IDs named `archer_*` map to `HeroClass.Ranger`.

`art.json` provides `poses[]` containing six art IDs and their cheer/fight resource, UV rectangle and trimmed dimensions; `crests[]` contains `heroClass` and `resource`. Pose UV bounds use alpha > 8 because very faint source pixels extend to the image edge. PNG bytes remain unchanged. UV y uses Unity's bottom origin.

Twenty-four baked 64×64 hit masks cover twelve cutouts and twelve poses. Base64 rows run from top to bottom; bits are little-endian within each byte. Runtime texture CPU reads are unnecessary. New deterministic UUIDv5 GUIDs and `.meta` files specify GPU-only textures, disabled mipmaps, Clamp and Bilinear sampling.

## Budget requested from the common owner

PNG files total **31,524,964 bytes (30.07 MiB)**. Full-size uncompressed RGBA would total 95,228,440 bytes. The capped ASTC6×6 estimate is 5,956,496 bytes (5.68 MiB), which is not a measured Unity resident-memory result.

The common `ResourceTextureBudget.cs` was not edited. Request these prefixes in order: `landscape_bg` and `portrait_bg` at 2048; `cheer_` and `fight_` at 1024; `crest_` at 256; remaining `Art/CharacterPlaza/` at 512. New metadata already uses these caps with Android ASTC6×6, Standalone BC7 and WebGL ETC2_RGBA8.

## Pixel review and remaining differences

All 28 reference JPEGs were visually inspected through four contact sheets, including landscape/portrait/PC, KO/EN and every supplied browse/new/created/gear/entering/rift state. Both source scenes and all twelve pose images were also viewed. The contacts are review aids, not replacement reference images.

The source composition retains warriors at the tavern, rangers at the fruit stall, and mages at the distant magic shop. Portrait focus uses Mage/Ranger/Warrior crests, a cream top mist and pose swaps. The documented 1.2× mage focus softness remains. The six cheer poses are unarmed; the six fight poses have fixed illustrated swords, bows or staves. They do not dynamically depict arbitrary equipped-item appearances. The female warrior fight pose uses a sword rather than her model sheet's axe. All twelve poses have maximum alpha 254; faint alpha speckles and existing edge contacts remain in the originals.

Vision-derived cutouts are not native alpha. Every record remains `productionApproved:false`; the female warrior outfit retains the source brief's age-rating review requirement.

The separately approved six standing portraits under `2026-10-13/task-4/output/hellscript-main-characters-6/delivery/` were verified against `DIRECTOR_STATUS.json` and `FINAL_MAIN_CHARACTERS_QA.json`: six exact hashes matched, and the light-background contact pixels were viewed. Their exact hashes are listed in the Korean companion. These portraits were not substituted for the new natural town activities.

Old `Art/CharacterSelection/{Warrior,Mage,Ranger}` atlases remain shared through `CharacterEquipmentView` and `CharacterFigure.Load`, including blacksmith consumers. Preserve them. Preserve `SelectionCourtyard` until reference and actual-screen replacement checks finish. No old resource, metadata, history or 3D hero asset was moved or deleted.

## Validation and retention

Static checks passed for 17 original hashes, 41 runtime hashes/GUIDs, 12 scene layouts, 24 masks, six pose pairs, three crests, GPU-only/mipmap settings and exclusion of full clean plates. Unity runs: zero. Integration owns the final build, save and input checks.

The temporary Swift binary, module cache, failure input, review contacts and results remain under `validation/character-plaza-art/` with an artifact lifecycle record. No Unity/game/web build, commit, push, merge or permanent deletion was performed.
