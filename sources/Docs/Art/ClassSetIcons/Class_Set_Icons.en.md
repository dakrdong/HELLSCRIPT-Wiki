# New Class Set Icon Production Record

Updated: 2026-09-22

[한국어](Class_Set_Icons.md)

This batch contains **105 equipment icons and 24 emblems for 24 new sets: 129 images total**. Every item has a PNG, bilingual name and effects, its actual prompt, a preserved generated original, hashes, and QA records. The six existing sets are excluded.

The built-in image tool did not return a model identity. All images therefore remain **`generated_candidate`** with actual model **`unknown`**. The configured default, `gpt-image-2`, cannot be verified, so these are not approved production assets. Image creation and file QA are complete; model evidence and acceptance for game use remain separate concerns.

## Scope and evidence

| Class | Sets | Equipment | Emblems | Production record | 64px QA sheet |
| --- | ---: | ---: | ---: | --- | --- |
| Warrior | 8 | 35 | 8 | [Warrior record](Warrior/Class_Set_Icons_Warrior.en.md) | [Warrior sheet](Warrior/qa-contact.png) |
| Mage | 8 | 35 | 8 | [Mage record](Mage/Class_Set_Icons_Mage.en.md) | [Mage sheet](Mage/qa-contact.png) |
| Ranger | 8 | 35 | 8 | [Ranger record](Ranger/Class_Set_Icons_Ranger.en.md) | [Ranger sheet](Ranger/qa-contact.png) |
| Total | 24 | 105 | 24 | [Combined manifest](manifest.json) | [Full validation report](validation.json) |

IDs, names, slots, and effects follow the frozen [sets.json](brief/sets.json) and [handoff instructions](brief/README.md), copied from the read-only handoff. The source worktree was not changed. The JSON SHA-256 is `242ef72d0a64a5615d2b69c549be0962c53d1081a348f2e9290f3f0596d1da51`; the instructions SHA-256 is `d611a706e89edfd17561ac967ec346f7ca948b851e2ffa760ac23bfb614146ab`.

During production, the source instructions were clarified to **accept native 1254px originals without requiring edits solely to reach the target size**. The [later handoff snapshot](brief/Class_Set_Icons_Handoff_Clarification.md) has SHA-256 `8ae3399779419e2c67bd0fb19c9b5bee3dc8367ba939cdec7692f704aaafd4e4`. It is stored separately from the initial snapshot. `sets.json` is unchanged, and all 129 native originals are delivered so integration can select them directly.

The 60 set bonus thresholds map to the 24 emblems through `manifest.json`'s `bonusMappings`. For example, `REF_SW01_B2` and `REF_SW01_B3` both use `REF_SW01.png`. Threshold numbers and separate threshold emblems are not embedded in the art.

## Visual and file contract

The original `EquipmentAtlas.png` and sword, character, and passive icons under `Art/GlobalHUD` were inspected directly. Worn iron and leather, restrained metal highlights, and broad value shapes informed the painterly dark gothic treatment. Each set repeats its material and motif; sets differ in silhouette and construction as well as color. No recolored copy was used to satisfy another ID.

Each piece and emblem used an independent generation request. Images contain no text, numbers, UI border, floor, or background. Originals have native PNG alpha. Chroma keys, background removal, and reconstructed alpha were not used.

| Item | Storage contract |
| --- | --- |
| Export | `Assets/HELLSCRIPT/Resources/Art/ClassSetIcons/<ID>.png`, 1024×1024 RGBA PNG. |
| Identity | Equipment uses its source piece ID; emblems use the source set ID. |
| Resources key | `Art/ClassSetIcons/<ID>` is recorded in the manifest, without adding runtime loading code. |
| Generated original | Class folders preserve the tool-returned bytes under `originals/<ID>.png`. |
| Evidence | Class `evidence/<ID>.json` files record returned paths, original/export hashes, alpha, and dimensions. |
| Revisions | Replaced candidates and reasons are preserved in class `rejected/` folders and manifests. |
| Unity metadata | New PNGs have unique GUIDs and single Sprite importer settings. Existing unrelated metadata was preserved. |

Despite the 1024px prompt, the tool returned **1254×1254 PNGs**. Generated originals and delivery exports are recorded separately. Exports use the full existing alpha bounds, proportional resizing, and transparent padding. Premultiplied-alpha resampling limits edge color bleed, and all nontransparent pixels fit within the central 76%. Resampling changes dimensions and pixel values; it does not repaint semantic content.

## Verification and reproduction

File validation checks all 129 IDs for omissions and duplicates, the 105/24 equipment/emblem counts, all 60 bonus mappings, source names and slots, PNG/alpha/dimensions/padding, original hashes, reproducible export pixels, and duplicate GUIDs. It also compares paths, status, and hashes between class manifests and the combined manifest.

Producers inspected every icon at actual 64px on both light and dark backgrounds, with larger supporting views in the contact sheets. Review covered equipment and emblem identity, incomplete forms, rectangular mattes, and conspicuous edge contamination. These are human production-review findings, not independent automated art judgments. An open Warrior necklace cord, a cut Ranger necklace cord, and a low-contrast Ranger bow were corrected through separate generation or edit requests.

Run from the repository root with Python and Pillow installed:

```text
python3 Docs/Art/ClassSetIcons/build_catalog.py
python3 Docs/Art/ClassSetIcons/validate_assets.py --require-complete
```

The validator does not modify PNGs. Its optional `--write-missing-meta` flag creates only missing metadata and preserves existing GUIDs. The [validation report](validation.json) records the exact scope and per-item findings.

## Integration and publication status

Combat data, UI layout, prefabs, scenes, and runtime icon wiring are outside this batch. Unity import, Edit Mode tests, macOS gameplay, and physical mobile verification were not run. File QA and offline visual review do not establish game integration.

This batch is delivered on `codex/class-set-icons`. Local wiki generation and checks cover the new documentation. Per the requested scope, it does not merge main or publish the public wiki; publication remains with the eventual merge owner.

The first wiki build stopped because the new worktree lacked attachments referenced by existing documents. A total of 272 historical files were copied from the read-only source into local Git-ignored paths after every hash matched the existing wiki dataset. The [recovery record](wiki-input-recovery.json) lists those hashes. These restore old document links and do not represent newly executed game tests.
