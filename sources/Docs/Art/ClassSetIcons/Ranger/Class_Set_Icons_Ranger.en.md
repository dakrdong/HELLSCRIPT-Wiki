# Ranger New Set Icon Production Record

Updated: 2026-09-22

[한국어](Class_Set_Icons_Ranger.md)

Created 35 equipment pieces and 8 emblems for the 8 new Ranger sets. All 43 assets have `generated_candidate` status. The built-in image-generation tool did not return a model name, so the actual model remains `unknown`. The requested default was `gpt-image-2`; its use is not claimed as verified.

## Scope and authority

IDs, bilingual names and effects follow the read-only handoff copies [sets.json](../brief/sets.json) and [README](../brief/README.md). Emblem `name` preserves the exact set name; an expanded emblem label is stored separately in `displayName`. The Ranger's 20 bonus thresholds share these 8 set emblems. No threshold numbers are baked into the artwork.

The existing `EquipmentAtlas.png`, `GlobalHUD/menu-character.png` and `GlobalHUD/status-sword.png` were visually inspected for worn leather, aged metal and tonal weight. Existing icons were not copied. Each ID was created through a separate built-in request. UI layout, integration code, combat data, scenes and prefabs were outside this production scope.

## Shared set identities

| ID | Set | Materials, motif and main forms |
| --- | --- | --- |
| REF_SA01 | Wandering Hunter | Tawny waxed leather, weathered brass, split-leaf arrows and lean folds. |
| REF_SA02 | Survivor Mantle | Midnight-blue quilting, smoked silver, folded swallow-wing shields and enclosing forms. |
| REF_SA03 | Marksman Manual | Umber leather, ash wood, blue-gray steel, open diamonds and straight arrows. |
| REF_SA04 | Colossus Hunt | Oxblood hide, black iron, ivory horn, cleft V motifs and thick triangular planes. |
| REF_SA05 | Vengeful Sight | Charcoal leather, blackened steel, crimson eyes and swept forked-feather plates. |
| REF_SA06 | Alchemical Hunting Ground | Moss leather, corroded copper, triangular yellow-green vials and trap teeth. |
| REF_SA07 | Threefold Venom | Blue-black iron, snakescale, three-pronged fangs and restrained violet, teal and moss accents. |
| REF_SA08 | Sightless Shadow | Indigo cloth, obsidian, smoked silver, blind slits and two offset crescents. |

The source concepts, gameplay loops, bonuses and piece-specific subjects are preserved together in [sets.design.json](sets.design.json).

## Native generations and normalized exports

The built-in tool was asked for 1024×1024 transparent PNGs with central padding, but all 43 selected native generations were returned at 1254×1254. Their bytes are preserved in `originals/<ID>.png`, with the returned generation path and SHA-256 recorded.

The game path `Assets/HELLSCRIPT/Resources/Art/ClassSetIcons/<ID>.png` contains a 1024×1024 export made by fitting the original alpha bounds proportionally and adding transparent padding. Resampling uses premultiplied alpha, so exported pixels differ from native pixels. No background removal, chroma key, alpha reconstruction or semantic content edit was performed. The raw generation and normalized export remain separate records.

## QA results

| Check | Result |
| --- | --- |
| Handoff IDs and names | 43/43 match, with no missing or duplicate IDs. |
| PNG, RGBA and transparent pixels | 43/43 selected originals and exports pass. |
| 1024px and central 76% | 43/43 exports pass. |
| Source preservation and export replay | Preserved sources match their generated files; replayed normalization matches the exports pixel for pixel. |
| Duplicate images | No duplicates in file, RGBA or visible-pixel hashes for originals or exports. |
| 64px and edges | All 43 were visually reviewed on light and dark backgrounds. |

The [QA contact sheet](qa-contact.png) shows each asset at 64px on dark and light backgrounds, with a 112px reference below. Equipment categories and set emblems remain distinguishable. Thin cords and scratches recede at small sizes while the primary forms remain readable.

The 43 final candidates were selected from 45 individual generation requests. The first `REF_SA03_P04` necklace clipped its cord and was regenerated. The first `REF_SA08_P01` bow had weak contrast on the dark 64px background and was regenerated with broader limbs and brighter silver bevels. Rejected outputs, reasons and prompts are preserved in `rejected/` and the item manifest. Existing `.meta` files and their GUIDs were not changed during these replacements.

## Records and verification scope

- [manifest.json](manifest.json) contains all 43 IDs, paths, bilingual effects, actual prompts, model evidence, hashes, alpha metrics, visual reviews and regeneration history.
- `evidence/<ID>.json` records measured source/export properties and the normalization procedure.
- [validation.json](validation.json) contains the Ranger file checks, with zero errors, pending checks or warnings.
- [progress.json](progress.json) records 43 generated candidates, zero reused assets and zero incomplete assets.

This evidence covers file checks and the producer's visual review. Unity import, gameplay, actual UI integration and physical-device checks were not run. The image work makes no claim that the set effects are implemented at runtime.
