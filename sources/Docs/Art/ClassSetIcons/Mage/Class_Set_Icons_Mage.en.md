# Mage expansion set icon production record

Updated: 2026-09-22
[한국어](Class_Set_Icons_Mage.md)

On September 22, 2026, this task created **35 equipment icons and 8 set emblems, 43 images total**, for `REF_SM01` through `REF_SM08`. Every image came from a separate built-in image-generation request. No existing game icon was copied or recolored to fill the count. No assigned item remains incomplete.

The callable tool did not return a generation model name. The requested default was `gpt-image-2`, but there is no returned evidence that it was used. Every item therefore records `model.actual: unknown` and `status: generated_candidate`. These records do not certify a model or constitute final production approval.

## Scope and authority

IDs, bilingual names, and effects follow the [handoff README](../brief/README.md) and [frozen sets.json](../brief/sets.json). Equipment slots and names, plus set-emblem names, match that handoff. The Mage sets' 20 bonus thresholds reuse the 8 images named by set ID. No threshold number appears in the art.

The original `EquipmentAtlas.png`, `GlobalHUD/menu-character.png`, and `GlobalHUD/status-sword.png` were visually inspected. They informed worn material rendering, light/shadow balance, and large readable silhouettes. HUD panels and circular frames were not copied. Read-only source reference paths are recorded in the manifest.

| Set | Pieces | Emblem | Shared materials and shapes |
| --- | ---: | ---: | --- |
| Warding Aegis (`REF_SM01`) | 3 | 1 | Pewter, pale jade, navy cloth, and nested pointed arches. |
| Deep Scholarship (`REF_SM02`) | 3 | 1 | Brass, walnut, vellum, and open-folio wings. |
| Rift Pilgrimage (`REF_SM03`) | 4 | 1 | Obsidian, oxidized silver, violet cloth, and split lozenges. |
| Capricious Pact (`REF_SM04`) | 4 | 1 | Charred bronze, three unequal stones, and asymmetric hooks. |
| Overheat Crucible (`REF_SM05`) | 5 | 1 | Blackened iron, thick copper, crucible bowls, and orange vent slits. |
| Frozen Deep (`REF_SM06`) | 5 | 1 | Ocean blue, clouded silver, thick ice teardrops, and shell ribs. |
| Wild Thunder (`REF_SM07`) | 5 | 1 | Graphite metal, electrum, lightning wedges, and forked contacts. |
| Threefold Cycle (`REF_SM08`) | 6 | 1 | Black porcelain, aged gold, ivory cloth, and balanced rotating petals. |

Related equipment repeats the set's materials and motif while retaining recognizable crown, armor, gauntlet, boot, belt, amulet, ring, or weapon outlines. Capricious Pact and Threefold Cycle share elemental colors but use different geometry: unequal hooks versus balanced rotating petals. [Set design records](sets.design.json) contain the bilingual design language and the original effects.

## Generated originals and normalized masters

The built-in generator returned **1254×1254 RGBA PNGs** despite the prompt's 1024px request. The original dimensions are recorded accurately. All 43 generated originals are preserved byte-for-byte at `originals/<ID>.png`, with the returned tool path and SHA-256.

The game-project masters at `Assets/HELLSCRIPT/Resources/Art/ClassSetIcons/<ID>.png` are **1024×1024 RGBA PNGs**. The complete native-alpha bounds were proportionally reduced and centered on transparent padding. Resampling means normalized master pixels differ from source pixels. No chroma keying, background removal, alpha reconstruction, or semantic repainting was performed.

## QA results

All 43 images passed the [file validation report](validation.json):

- PNG/RGBA format and actual fully transparent and opaque pixels were verified.
- Every normalized master is 1024×1024 with all nontransparent pixels inside the central 76%.
- Preserved originals match the generated source files byte-for-byte.
- Replaying the documented export reproduces the normalized image pixels.
- All 43 output file hashes are unique; there are no missing or duplicate IDs.

The producer visually inspected each generated result and the final [readability contact sheet](qa-contact.png). The sheet shows each icon at native 64px against both dark and light backgrounds, plus a 112px supporting preview. Equipment slots and set identities remain distinguishable; no rectangular matte or conspicuous fringe appears at icon size. Per-item verdicts and bilingual notes are stored in `qa.readability64` and `qa.edges`. These are producer review records, not independent image judgments made by the validator.

## Handoff and limits

- [manifest.json](manifest.json) links all 43 IDs to bilingual names/effects, exact prompts, files, generation provenance, hashes, and alpha/readability/edge checks.
- [sets.design.json](sets.design.json) records the 8 visual languages and set-threshold emblem reuse.
- [progress.json](progress.json) records all 43 generated and reviewed items.
- `evidence/<ID>.json` records original/master dimensions, alpha bounds, transparency counts, and hashes.

This work changed only assigned images and production records. It did not modify UI layout, scenes, prefabs, combat code, or icon-connection code. Unity import, runtime, and physical-device checks were not performed. The integration task owns `.meta` creation and the combined commit/push. Merging main and publishing the public wiki are outside this production task.
