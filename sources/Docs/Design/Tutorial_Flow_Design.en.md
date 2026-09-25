# Tutorial progression and content guidance

Updated: 2026-09-25

This design implements the approved flow: dedicated map → real armor acquisition and equipment → actual boss → town → rifts → Hunt Edict editing → new skill equipment → guidance at each content unlock.

## Fixed decisions

- Only the dedicated map is mandatory, once per account. Other guidance supports read, defer, hide and reopen independently of practice.
- Real level 1 common body armor remains owned after the first map. No XP, gold, random loot, fatigue, rift records, first clears or offline baseline are granted by tutorial combat.
- Other characters use per-hero core guidance. Replay uses a separate level 1 account copy and transfers no rewards.
- First-practice support is account-wide, ten kinds, one use each. Support is added only inside the staged execution transaction and committed with the result. Cancelling, invalid selection or failed disk saving retains the entitlement.
- Normal domain prerequisites, prices, probabilities, capacities, protection and NPC proximity remain enforced. No prerequisite legendary, aspect, level, enhancement or sweep attempts are invented.
- Existing accounts are exempt from the mandatory map. Old read/skip acknowledgements migrate as read only; semantically equivalent actual first-play facts are preserved. Existing account guidance is contextual.

## Dedicated map

| Step | Trigger and player action | Completion / recovery |
| --- | --- | --- |
| P01 | Start the first account character; short road introduction. | Map owner and run ID saved before launch. |
| P02 | Continue and observe automatic travel. | Uses the same movement and navigation owners as actual combat. |
| P03 | Defeat three real enemies with actual skills. | Enemy deaths are simulation outcomes; no manufactured victory. |
| P04 | Recover fixed common armor after the first wave. | Armor and run checkpoint committed once. Failed saving keeps the step pending. |
| P05 | Open native inventory, inspect/compare and equip the armor. | Actual owned equipment required. Armor cannot be unlocked or stored away before completion. |
| P06 | Read guardian preparation and continue. | Equipment stats refresh; original combat engine spawns a real boss. |
| P07 | Defeat the guardian. | Actual boss death; death offers section retry without repeated rewards. |
| P08 | Confirm town arrival. | Account completion and run removal saved atomically; armor remains owned. |

## All 38 guide groups

| ID | Guide | Player-facing design | First-practice support |
| --- | --- | --- | --- |
| P00 | Road to the sanctuary | Watch automatic movement and combat. Compare and equip the recovered armor in your inventory, then defeat the boss to reach the sanctuary. | None |
| F01 | Town movement and residents | Tap a destination or use the movement pad / WASD. Interact near the rift keeper to prepare your run. | None |
| F02 | Prepare your first rift | Review equipment, skills, potions and stage. Normal admission uses 1x speed; fatigue continues during a paused rift. Enter when ready. | None |
| F03 | Explore a rift | Travel, attacks and collection are automatic. Watch the kill meter, the time limit fixed on entry and the map. Complete the objective to face the boss. | None |
| F04 | Results and first-clear rewards | Review success or failure and the final actions. Collected rewards remain yours. Claim first-clear boxes separately in the rift entry screen. | None |
| F05 | Change one Hunt Edict setting | Change the potion HP threshold under Survival in Hunt Edict and save it. Opening an explanation or cancelling does not complete the practice. | None |
| F06 | Equip a newly unlocked skill | When leveling unlocks a new active skill, equip it in a real slot and save. Passives apply without occupying a slot. | None |
| F07 | Test and retry | Test your changed edict in training, then enter a new rift with that setup. Training grants no rewards and consumes no fatigue. | None |
| UL01_TRAIN | Training | Test your owned equipment and skills for 60 combat seconds. Review damage, survival and decision reasons in the result. | None |
| UL02_ENHANCE | Equipment enhancement | Choose an owned +0 item, review the cost and enhance it to +1. First practice covers gold for this one step. | Yes |
| UL03_OFFLINE | Offline supplies | Review supplies settled from actual rift performance. Tutorial combat never creates an offline reward baseline. | None |
| UL04_RARE_CRAFT | Rare equipment crafting | Choose a slot and craft one rare item. First practice covers its gold and material cost. | Yes |
| UL10_SLOT_ENHANCE | Equipment slot upgrade | Upgrade a level 1 equipment slot to level 2. First practice covers stones; the normal five-minute duration still applies. | Yes |
| UL05_GEM | Gem fusion | Fuse five tier 1 gems of the same kind into one tier 2 gem. First practice supplies the five selected tier 1 gems. | Yes |
| UL11_RUNE | Rune purchase and placement | Buy one G0 single-cell rune, then inspect its placement on the rune board. First practice covers one rune purchase. | Yes |
| UL07_UNIDENTIFIED_SHOP | Unidentified equipment | Buy one unidentified item for your chosen slot. First practice covers gold and uses normal rarity probabilities. | Yes |
| UL06_REROLL | Affix reroll | Choose one affix on an owned item and reroll it. First practice covers one attempt; its result remains random. | Yes |
| UL12_ASPECT | Aspect imprinting | Select an actually owned aspect and eligible equipment. Perform the free imprint; no legendary or aspect is invented for practice. | None |
| UL08_SWEEP | Rift sweep | Review your cleared stage and remaining daily attempts, then sweep. Guidance grants no extra attempts. | None |
| UL13_ELIXIR | Gem elixirs | Craft one elixir using a selected tier 1 gem. First practice supplies one gem. Review usage in your potion slots afterward. | Yes |
| UL14_MASTERWORK | Masterworking | Masterwork an actually owned enhancement +5 or higher item from 0 to 1. First practice covers gold and materials; normal prerequisite enhancement is required. | Yes |
| UL09_CORE_CRAFT | Core crafting | Choose a recipe and craft one item with ten slot cores. First practice covers those cores with premium investment fixed at zero. | Yes |
| H01 | Equipment details and protection | Compare equipped items on the left and candidates on the right. Review option ranges and protection. Comparison alone does not equip items. | None |
| H02 | Shop and buyback | Review protection and prices before selling. Sold equipment can be bought back while retained in the buyback list. | None |
| H03 | Salvage and automatic selection | Check rewards and selected items in the salvage preview. Equipped, protected, socketed and preset-linked equipment follows shared protection rules. | None |
| H04 | Storage and equipment presets | Distinguish account storage from the hero bag. Presets reference owned equipment instead of duplicating it. | None |
| H05 | Potion slots and order | Assign owned potions to slots and review the shared usage order. Configure survival conditions in Hunt Edict. | None |
| H06 | Potion replenishment | Review replenishment costs and stock before departure. If stock or gold is insufficient, adjust the policy or cancel replenishment. | None |
| H07 | Skill points and passives | Invest earned points to improve skills. Unlocked passives apply without being equipped in active slots. | None |
| H08 | Ultimate skill | At actual level 40, inspect the ultimate and equip it in its dedicated slot. Review its conditions and resource cost. | None |
| H09 | Edicts and decision reasons | Inspect conditions, range and resource shortages in decision records. Edit the relevant setting from Current settings. | None |
| H10 | Presets and sharing | Review changes when saving and loading presets. Imported configurations still validate ownership and unlock requirements. | None |
| H11 | Repeat hunting and idle display | Configure actions after success or failure and when the bag is full. Dimmed display does not provide server-side progression. | None |
| H12 | Failure and resume | Review the cause of failure. Resume an interrupted rift from its saved state; this differs from starting a new run. | None |
| H13 | Bag capacity and pending rewards | When the bag is full, make space while respecting protection. Distinguish pending rewards from collected items and verify the actual claim. | None |
| H14 | Legendary, set and awakened items | Inspect unique effects, set thresholds and awakened properties in shared item details. The first guide for each kind is tracked separately. | None |
| H15 | Rift objectives and encounters | Inspect seal, carrier and offering objectives and chest, shrine, goblin, elite and boss encounters. In-run tips do not block combat; detailed guidance is available in town. | None |
| H16 | Attendance events | Review attendance days and available rewards, then claim them. Closing or hiding automatic display preserves claim records. | None |

## Unlock authority and variants

The current new-account unlock schedule is below. Migrated accounts retain previously unlocked access.

| Guide ID | Content | Highest account rift clear |
| --- | --- | --- |
| UL01_TRAIN | Training ground | Town arrival |
| UL02_ENHANCE | Equipment enhancement | Stage 1 |
| UL03_OFFLINE | Offline Supplies | Stage 1 |
| UL04_RARE_CRAFT | Slot-based rare crafting | Stage 3 |
| UL10_SLOT_ENHANCE | Slot upgrades | Stage 5 |
| UL05_GEM | Gem socketing, replacement and synthesis | Stage 10 |
| UL11_RUNE | Rune boards, fusion and weapon mastery | Stage 15 |
| UL07_UNIDENTIFIED_SHOP | Unidentified equipment shop | Stage 25 |
| UL06_REROLL | Affix reroll | Stage 35 |
| UL12_ASPECT | Aspect upgrades and imprinting | Stage 40 |
| UL08_SWEEP | Sweep | Stage 60 |
| UL13_ELIXIR | Gem potion crafting | Stage 60 |
| UL14_MASTERWORK | Masterworking | Stage 80 |
| UL09_CORE_CRAFT | Legendary core crafting | Stage 120 |

Unlock stages come directly from `Resources/ContentUnlocks.json`; tutorial code does not duplicate stage thresholds. `H14` independently tracks legendary, set and awakened equipment. `H15` independently tracks seals, carriers, offerings, chests, shrines, goblins, elites and bosses.

The 4–6 minute first-map duration is a usability target, not a validated human measurement. Completion gates use actual actions rather than time spent reading.

Implementation and validation: [record](../Implementation/Tutorial_Progression.en.md).
