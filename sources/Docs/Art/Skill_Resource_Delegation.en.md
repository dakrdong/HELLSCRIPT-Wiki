# Skill and equipment image handoff

2026-09-22 · [한국어](Skill_Resource_Delegation.md)

The user confirmed four tasks grouped by asset type rather than one task per individual item. Each task started in its own worktree with item IDs, Korean/English names, effects, companion skills and visual concepts. The [shared production brief](SkillExpansionBrief/README.md) links the authoritative JSON lists.

| Type | Assigned scope | Work branch |
| --- | --- | --- |
| Skills | 36 per class, 108 icons | `codex/class-skill-icons` |
| Legendary equipment | 123 existing plus 18 added, 141 equipment icons | `codex/class-legendary-icons` |
| Set equipment | 105 pieces and 24 set emblems for the new 24 sets | `codex/class-set-icons` |
| Aspects | 141 legendary-effect emblems | `codex/class-aspect-icons` |

The integration verified files and existing visual-review records for 108 skill icons, 141 legendary equipment icons, 105 set pieces and 24 set emblems. It includes 123 generated aspect emblems, of which 122 passed visual review. `LM29` remains visually rejected, and the 18 new equipment-linked emblems `DES_LW41–46`, `DES_LA41–46` and `DES_LM41–46` remain ungenerated. The user confirmed there are no additional completed files elsewhere.

All four asset families retain model `unknown` and approval `candidate`. Native pixels, alpha, GUIDs and existing edge warnings are preserved. Merging does not imply game-screen adoption or release approval. Each task owns its production and validation records under `Docs/Art/ClassSkillIcons`, `ClassLegendaryIcons`, `ClassSetIcons` or `ClassAspectIcons`.

## Shared production requirements

Equipment icons depict the object; aspect emblems depict its effect. Active skills use a recognizable action and silhouette, while passives symbolize a persistent property. Produce original HELLSCRIPT imagery without copying finished game artwork or logos.

Deliver background-free PNGs with real alpha. Record dimensions, hashes, transparent pixels, edge checks, readability at small sizes and stable item IDs. The shared master target is 1024px, but sufficiently large native outputs such as 1254px are acceptable. No separate image editing is required merely to force the target size. Preserve the original with its actual dimensions; UI integration determines display size.

Record generation models only when supported by verifiable provenance. If the tool provides none, retain `unknown`; do not label the result as `gpt-image-2` or a release-approved asset. Keep unresolved checks in the relevant production record.

## Ownership and integration

Each task edits only its own image folder and production records. It does not change combat calculations, equipment effects, saves, scenes, UI layout, animation or visual-effect code. Skill-slot placement and public availability of new items belong to UI integration.

M13 Mana Reclaim depicts stationary charging that is not broken by damage alone. The current DES_LM44 effect grants a 10% maximum-health shield for 4s after at least 1.5s of uninterrupted charging inside the caster's ward. Its 6s retrigger cooldown survives channel restarts. Legendary and aspect tasks share the effect ID but produce imagery for distinct uses.

Do not deploy the public wiki from these feature branches. The merger validates and publishes documents, databases, images and source evidence together from merged `main`.
