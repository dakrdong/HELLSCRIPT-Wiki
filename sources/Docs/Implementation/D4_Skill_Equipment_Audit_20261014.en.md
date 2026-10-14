# Equipment consistency after the D4 skill changes

Date: 2026-10-14. [한국어](D4_Skill_Equipment_Audit_20261014.md).

Audited 141 legendary definitions (123 ordinary, 18 additional), 129 set pieces, 30 set families and 72 bonus tiers against current skills, modifiers and combat code. Item/aspect IDs, slots, drop weights and the additional definitions' disabled ordinary-drop gate remain stable.

Removed Mana Recovery dependencies now use Elemental Spear inside Rift Ward. Mutually exclusive modifier combinations now use Branded Hook + Tremor Wave, Poison Imbuement + Frost Snare, or a decoy's own attraction extension. Debt of Strife uses its current regeneration pause and actual resource spending. Existing charge, turret preparation, aimed shot, fork, ancestor/form/clone and collapse effects are connected to current releases and hits. Glacial Spear prepares a 3s Returning Frost window only after a real freeze, allowing both skills' action timing while excluding boss stagger.

Fireball echoes, imbuement spread, Double Leap recharge and seven-arrow Barrage narrowing use current shapes and shared recursion/snapshot/persistence owners. Frostbite includes original Returning Frost hits; set afterimages explode at the first hit position. Descriptions state field-only requirements. Set rank bonuses follow actual learned skills instead of historical level gates.

Updated both languages, the [equipment matrix](../Design/HELLSCRIPT_Skill_Equipment.en.md) and the [legendary catalogue](../Design/HELLSCRIPT_Legendary_Class_Expansion.en.md). Current authoring is `Docs/Design/ClassSkills/current_runtime_parameters.json`, compiled by `tools/class_skill_runtime.py build`.

The wiki collector now reads the current 144 D4 definitions and modifier scopes rather than the removed historical tree. Its equipment drop field stays disabled and the old 18-row catalogues are explicitly labelled as retained records.

## Verification

Ran the complete EditMode suite once after main integration at `87e7e064`: 5,742 passed, one failed and three skipped out of 5,746. All 31 new equipment cases passed. The skips are two existing authoring probes and a measurement requiring an explicit output path. The failure came from aspect-tier description token replacement. Corrected both languages and removed a conflicting base 50% sentence from upgraded imbuement spread descriptions. The two focused checks passed after repair, covering all 123 aspects at five levels in both languages and actual power growth parameters. Unchanged combat results were reused.

The macOS development player passed controlled validation for three real owned/equipped items, one per class: effects, item identity after disk reload, run checkpoint and 0.5s resumed RNG/resource/damage parity. The first attempt failed to cast before the harness matched generated-map coordinates and progression access; the second build/run passed after fixing those inputs. This was a headless combat/save execution. Screen interaction, physical mobile, WebGL and long-term balance are outside this proof.

Data generation parity, stable identities/drop gates, removed-ID validation and 15 UI contract regressions passed. The additional legendary/set definitions keep their ordinary-drop gate disabled. The task-owned player, cache and generated wiki are archived with the managed checkout after compact evidence and the reproduction recipe are preserved.

| Evidence | Coverage |
| --- | --- |
| [audit.json](D4EquipmentEvidence20261014/audit.json) | Audited scope, changes, stable identities and drop gates |
| [validation.json](D4EquipmentEvidence20261014/validation.json) | Source/binary/save hashes, results, reuse boundary and recipe |
| [Full EditMode XML](D4EquipmentEvidence20261014/full-editmode.xml) | Original full result, including failure and skips |
| [First description repair](D4EquipmentEvidence20261014/aspect-description-repair.xml), [final repair](D4EquipmentEvidence20261014/aspect-description-final-repair.xml) | Preserved failure and final 2/2 pass |
| [macOS summary](D4EquipmentEvidence20261014/native/result.json), [Warrior](D4EquipmentEvidence20261014/native/class-0-result.json), [Ranger](D4EquipmentEvidence20261014/native/class-1-result.json), [Mage](D4EquipmentEvidence20261014/native/class-2-result.json) | 3/3 effect, disk reload and deterministic resume |
| [UI contract](D4EquipmentEvidence20261014/ui-contract-repaired-receipt.json) | 15/15 pass |
| [Diagnostic index](D4EquipmentEvidence20261014/diagnostic-runs.json), [first 30](D4EquipmentEvidence20261014/focused-current-gear.xml), [failed subset repair](D4EquipmentEvidence20261014/failed-current-gear-repair.xml), [last four](D4EquipmentEvidence20261014/failed-gear-final-repair.xml), [magnetic repair](D4EquipmentEvidence20261014/failed-magnetic-repair.xml), [narrowing failure](D4EquipmentEvidence20261014/narrowed-line.xml), [narrowing repair](D4EquipmentEvidence20261014/narrowed-line-repair.xml) | Overlapping diagnostic runs are not added to the full count |
| [Historic wiki input restoration](D4EquipmentEvidence20261014/wiki-source-restoration.json) | Only 291 inputs with hashes matching the existing published commit |
