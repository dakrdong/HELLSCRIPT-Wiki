# HELLSCRIPT skill icon production record

Updated on: 2026-09-22

[한국어](Class_Skill_Icons.md)

Created and reviewed **108 independent skill PNGs** across Warrior, Ranger and Mage: 48 normal actives, 6 ultimates and 54 passives. No existing icons were reused or duplicated. Every image remains **candidate art with unknown model provenance**.

| Class | Normal actives | Ultimates | Passives | Total |
| --- | ---: | ---: | ---: | ---: |
| Warrior | 16 | 2 | 18 | 36 |
| Ranger | 16 | 2 | 18 | 36 |
| Mage | 16 | 2 | 18 | 36 |

## Masters and provenance

The frozen handoff controls IDs, bilingual names, effects, `visualConcept`, tags and equipment associations. The source workspace's `Docs/Art/SkillExpansionBrief` was read only. Its [copied brief](handoff/Class_Skill_Icons_Handoff.md), [skill JSON](handoff/skills.json) and [snapshot hashes](handoff/snapshot.json) are preserved. EquipmentAtlas and GlobalHUD images were visually inspected for worn metal, leather, strong light and readable forms.

All masters are **1254×1254 native RGBA PNGs**. The [revised handoff](handoff/Class_Skill_Icons_Spec_Revision_2.md) accepts the built-in tool's 1254px output in addition to the initial 1024px target. Central 76% is a padding guideline; actual clipping, missing primary forms and opaque backgrounds fail review.

Each ID used its own built-in `image_gen` request. Responses exposed `image_url` and `output_hint`, without verifiable model identity. The preferred model was `gpt-image-2`, but the callable surface did not expose selection or provenance. All manifests therefore retain `model: unknown`, `modelEvidence: null` and `approval: candidate`. No external API or CLI was used.

Returned PNG bytes were preserved. There was no resizing of masters, background removal, chroma key, alpha reconstruction, recoloring, SVG substitution or copied game artwork. Only QA thumbnails add contrasting backgrounds and ID labels.

## Deliverables

- The [combined manifest](manifest.json) maps all 108 IDs to files, bilingual effects, actual prompts, equipment associations, hashes, GUIDs, provenance and QA.
- The [production checklist](Class_Skill_Icons_Progress.md) links individual manifests.
- Game files reside at `Assets/HELLSCRIPT/Resources/Art/ClassSkillIcons/{Warrior,Ranger,Mage}/{ID}.png`; the manifest also records `Resources` keys.
- Class records are available for [Warrior](Warrior/Class_Skill_Icons_Warrior.en.md), [Ranger](Ranger/Class_Skill_Icons_Ranger.en.md) and [Mage](Mage/Class_Skill_Icons_Mage.en.md).
- Each class's `raw/` directory preserves original returned PNGs, with per-ID manifests and actual prompts alongside them.

W02 depicts a long leap arc ending in a heavy landing impact. M13 depicts stationary hands gathering blue magic inward. Its current description specifies no cooldown, ten times base mana regeneration while stationary, cancellation on movement/attacking, and persistence through damage. The frozen JSON's older `cooldownSeconds: 14` remains unchanged as source evidence.

## Verification

The [combined report](qa/validation.json) records **108 passed, zero missing, zero global errors**.

- PNG format, square 1254px dimensions, native RGBA layout and fully transparent pixels were measured.
- All 108 master/raw SHA-256 pairs match. No duplicate images or GUIDs were found across IDs.
- Originals and actual 64px thumbnails were visually reviewed against dark and light backgrounds for form, distinction, clipping, mattes and halos.
- Warnings remain explicit: 108 unknown-model notices, 56 low-alpha border notices, 72 central-core padding notices and 72 faint-effect padding notices. Masters were not modified to erase warnings.
- New metadata comprises 108 PNG files and 4 folders with unique GUIDs. PNG settings use single Sprite, source alpha, Clamp, no mipmaps and a 2048 default import-size limit. Existing asset GUIDs were unchanged.

The [Warrior](qa/Warrior-64px-dark.png), [Ranger](qa/Ranger-64px-dark.png) and [Mage](qa/Mage-64px-dark.png) contact sheets show actual 64px previews; matching `-light.png` files use a light backdrop. They are QA sheets, not runtime atlases.

Re-run from the repository root with `python3 -B Docs/Art/ClassSkillIcons/validate_assets.py --report Docs/Art/ClassSkillIcons/qa/validation.json`. After a passing check, `assemble_manifest.py` rebuilds the combined manifest and checklist. Neither tool edits game-master pixels.

## Scope

This is image production and file-level verification. UI layout, scenes, prefabs, combat code and icon-binding code were unchanged. Live Unity importing, runtime behavior and physical mobile devices were not tested. The later UI task owns display size and final importer choices. This task does not merge `main` or publish the public wiki.
