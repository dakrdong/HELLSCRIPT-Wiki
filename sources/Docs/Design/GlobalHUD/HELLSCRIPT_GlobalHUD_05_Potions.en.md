# Global HUD 05: potions and hunting edicts

Created: 2026-09-14 · Updated: 2026-09-23

Three slots display distinct potions assigned in the inventory and their actual per-character stock without taking equipment bag space. Initial assignments are HP, resource and Iron; existing saves retain their edict utility selection. Selecting a HUD slot opens information; it never manually consumes a potion. Bottle art is 45 logical units in landscape and 60 in portrait. Counts and cooldown text are separate.

## Default data

| ID | Type | Effect | Duration | Cooldown | Gold |
|---|---|---|---|---|---|
| PH01 | HP | Restore 35% maximum HP with existing healing and potion modifiers | Instant | 20s | 5 |
| PM01 | Resource | Restore 35% maximum resource | Instant | 20s | 5 |
| PU01 | Sprint | Movement speed bonus +20 percentage points | 8s | 30s | 15 |
| PU02 | Assault | Attack power +15% | 8s | 30s | 15 |
| PU03 | Resistance | All elemental resistance ratings +20 + hero level×3 | 8s | 30s | 15 |
| PU04 | Iron | Armor +30% | 8s | 30s | 15 |
| PU05 | Haste | Attack speed bonus +15 percentage points | 8s | 30s | 15 |
| PU06 | Focus | Critical strike chance +10 percentage points | 8s | 30s | 15 |

The source is `Assets/HELLSCRIPT/Resources/Data/Potions.json`. Each slot accepts an owned potion, with no duplicate IDs and no fixed slot category. Utility types use the amber bottle plus a distinct glyph and appear in the effect strip when active. All eight current definitions have grade 0; no new tier balance has been introduced.

Retain existing movement bonus 50%, attack speed bonus 50% and critical chance 75% caps. Resistance adds to the five elemental ratings, not physical armor, and retains existing mitigation caps. Do not stack or consume while the utility effect remains active, or when caps prevent any effective change.

## Automatic use and equipment

Preserve the existing HP threshold; new defaults are HP 40% and resource 30%. Each recovery type has its own enable switch and threshold in the hunting edict. The default utility is Iron. The default condition maps Sprint to actual movement, Assault/Haste/Focus to combat, and Resistance/Iron to HP at or below 60% or existing danger detection. Explicit conditions are off, moving, combat, elite/boss combat and danger.

Change assigned potions through the inventory slots in town. Until an explicit assignment is saved, the existing edict utility selection applies. Afterwards the inventory loadout owns the assignments. Saving an assignment change during an active real rift is rejected. In training, the utility inspector changes only the training copy; real stock stays unchanged. An active utility effect must expire before training equipment changes.

## Shared exhaustion policy

The three potion cells are smaller than equipment: 32 in portrait and 28 in landscape. They occupy the right corner of the weapon row. Clicking a cell opens its potion picker. One shared gear beside the group opens a fixed-size speech bubble with “When the assigned potion runs out” and four exclusive choices. The selected row has a check and highlighted color; changing it never moves the inventory or resizes the bubble.

- Use higher-grade potions: descending grade within the same effect.
- Use lower-grade potions: ascending grade within the same effect.
- Use the most recently acquired potions first: newest remaining acquisition batch.
- Use the oldest acquired potions first: oldest remaining acquisition batch.

The assigned potion always takes priority while available. After exhaustion, candidates must have the **same effect** and must not be assigned to another slot or already resolved as another fallback. Grade ties use the oldest remaining acquisition sequence, then ID. With no candidate, the slot stops using potions. Current data has no alternative tiers for the same effect, so real grade-based replacement becomes available when such data is added.

A single policy persists per character, applies to all three slots and can change during battle. Older per-slot settings migrate once from the first assigned cell. Multiple utility assignments still share the existing cooldown and single active utility effect. Purchases and grants record acquisition batches. Old saves without acquisition history migrate once using stored stack order; this is not a reconstruction of historical timestamps. See the [implementation and validation](../../Implementation/Potion_Slots.en.md).

## Restocking and departure

Targets apply to assigned effects: 20 HP, 20 resource and 10 of each utility potion. Empty slots and unassigned types are not purchased. Purchase in HP, resource, utility order, respecting both the cumulative 500-gold visit budget and the 200-gold reserve. The edict exposes automatic purchasing, effect-specific targets, budget and reserve. Automatic use, purchase targets and departure rules are independent. Disabled automatic use does not suppress restocking; set a target to zero to stop buying that type.

Restock on actual sanctuary arrival and preparation for the next repeated run. Persist visit identity and spending; redraws, opening settings and retries do not reset the budget. The existing staged transaction writes inventory, gold and visit state before adopting them in memory. Disk failure leaves all unchanged.

| Shortage policy | Behavior |
|---|---|
| Wait if no HP potions | Wait when an HP slot is assigned and no same-effect potion is available, regardless of automatic-use toggles. New default. |
| Wait for stock targets | Wait if any of the three slots has less than its saved target. |
| Depart with insufficient supplies | Depart regardless of missing stock. |

Always obey the saved choice. Shortage never forcibly ends an active battle. Reevaluate waits when gold, stock or the edict changes. Reopening the app does not silently resume automatic departure.

## Persistence and migration

Grant each hero 20 HP, 20 resource and 10 Iron potions once, recorded by `PotionInventory.version`. Zero stock never triggers another grant. Account schema is 5, inventory/runtime potion version is 1, and edict global-option version is 2.

An old suspended battle retains potion runtime version 0 and its unlimited HP contract (`∞`); resource/utility slots remain locked. Switch at sanctuary or next departure preparation after that run. Preserve HP thresholds, three passives, unlocks, presets and edict enabled state. Legacy HED1/HED2 import validates old values and supplements only the new options.

Training and A/B comparison copy inventory along with the account. HUD rendering/inspection never changes stock, gold or combat time. Effect application, stock decrement and cooldown start form one simulation step. Keep the HP potion's LC03 shield interaction.

한국어: [물약과 사냥 칙령](HELLSCRIPT_GlobalHUD_05_Potions.md).
