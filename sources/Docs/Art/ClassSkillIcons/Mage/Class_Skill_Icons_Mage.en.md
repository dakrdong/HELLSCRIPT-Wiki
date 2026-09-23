# Mage skill icon production record

[Sage's Ward MP19](manifests/MP19.json) was added on 2026-09-23, bringing the class total to 37. The record below describes the initial 36 icons. New Sprite import and HTML verification are recorded in the [combined report](../Class_Skill_Icons.en.md).

Updated on: 2026-09-22

[한국어](Class_Skill_Icons_Mage.md)

The full Mage handoff contains 16 normal actives, 2 ultimates and 18 passives, for 36 IDs. This production group generated and reviewed the 27 native masters `M01`–`M18` and `MP01`–`MP09`. A parallel producer completed the remaining 9 IDs, `MP10`–`MP18`. Use the parent integration report and individual manifests to determine full-class completion.

## Full 36-item completion

The parallel producer also completed and reviewed MP10–MP18. The final integration report passes all 36 Mage items. The 27-item records and sheets below preserve the first production group; use the [combined validation](../qa/validation.json) and [final 64px sheet](../qa/Mage-64px-dark.png) for the complete class.

## Sources and specification

- The [frozen handoff JSON](skills.json) is authoritative for IDs, bilingual names, descriptions, `visualConcept`, tags and equipment associations. Older combat definitions in this checkout were not used as the art specification.
- The original handoff workspace was read only: `/Users/t8g-2410-pn-005/.codex/worktrees/class-skill-runtime/HELLSCRIPT`.
- The original project's `EquipmentAtlas.png`, `GlobalHUD/status-flame.png`, `status-mana.png`, `passive-MP03.png` and `passive-MP01.png` were opened and visually inspected. Dark tactile equipment materials, pale edges and readable HUD silhouettes informed the art.
- Existing passive images contain 256px line symbols and UI plates, so they were not reused as these painterly masters. All 27 items in this group were generated through separate requests.

Initial prompts requested 1024px masters and central 76% padding. A later handoff instruction accepted the built-in tool's **1254×1254 square native RGBA outputs** as valid masters and treated central 76% as a clipping-review guideline. Every master in this group is 1254px. The returned PNG bytes were preserved without resizing or padding. Actual prompts remain unchanged as request evidence.

## Files and provenance

- Imported masters reside at `Assets/HELLSCRIPT/Resources/Art/ClassSkillIcons/Mage/ID.png`.
- [raw](raw/) stores the corresponding native returned PNGs. Their SHA-256 hashes match the imported copies.
- [prompts](prompts/) contains actual per-item requests. [manifests](manifests/) records bilingual effects, paths, provenance, model evidence, alpha checks and visual QA.
- Only built-in `image_gen` was used. The returned keys were `image_url` and `output_hint`, without a model name. Every item therefore remains `model: unknown`, `modelEvidence: null`, and `approval: candidate`.
- No background removal, chroma key, recoloring, SVG substitution, copied game icons, or external API/CLI generation was used.
- Missing `.meta` files use the shared validator's single-Sprite configuration and new unique GUIDs. Existing GUIDs were neither reused nor changed.

## Semantic direction

Actives depict their main action; passives symbolize sustained combat tendencies. Distinct large forms include a round fireball blast and trailing flames, falling ice inside a blizzard, connected lightning impacts, and broad serrated ice lances. The ultimates use a dense tri-element collision and an empowered sage bust for stronger central mass. No text, numbers, frames or UI panels were baked into the masters.

`M13` Mana Reclaim shows steady hands with blue energy converging inward into a stationary core. It follows the latest description: no cooldown, ten times base mana regeneration while stationary, cancellation on movement or attacking, and persistence through damage. The frozen JSON still has an older `parameters.cooldownSeconds: 14`; the description and `visualConcept` take precedence for art. Neither the frozen JSON nor combat code was changed.

## Verification

The 27 masters directly produced here passed:

- Native PNG RGBA layout, 1254×1254 dimensions, actual fully transparent pixels and a visible alpha core.
- Imported/native SHA-256 equality, distinct image hashes and unique GUIDs.
- Visual readability and distinction at actual 64px in the [dark contact sheet](qa/Mage-64px-dark.png) and [light contact sheet](qa/Mage-64px-light.png).
- Inspection of all native outputs and six 256px edge-review sheets. No opaque matte, painted backdrop pattern or visibly clipped main form was observed.
- Per-ID observations are in [the 27-item manual review](qa/manual-review-27.json). Pixel metrics are in [the class report](qa/report.json). The class report also includes the 9 IDs being produced in parallel, so group completion and full-class completion are separate.

Shapes extending outside central 76% and occasional outermost `alpha=1` pixels remain recorded warnings. No crop or alpha cleanup was applied. Actual clipping and visible mattes were reviewed separately against light and dark backgrounds. Unknown model provenance also remains a warning.

No UI integration, scenes, prefabs, combat code or icon-binding code was changed. Live Unity importing, runtime behavior and physical mobile devices were not tested in this art-only review. The parent production task performs commits, push and integration checks across all 108 skills.
