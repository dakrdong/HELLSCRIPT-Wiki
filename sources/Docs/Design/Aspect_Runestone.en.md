# Aspect Runestone and progression

Updated: 2026-09-22
Korean: [위상 각인석과 위상 성장](Aspect_Runestone.md)

## Flow and field placement

A free-standing dark rock, 5m wide, approximately 7m tall and 2m deep, stands at town coordinate `(-34, 4)` with an interaction point at `(-34, 1)`. Jade inscriptions glow on its face. Its footprint blocks movement without blocking the central road. The game has no field placement tab.

Select equipped gear, choose an aspect, then imprint. Portrait uses three steps; landscape presents three columns. The swap button opens an inline equipped-item list on confirmation and retains the chosen aspect.

## Collection and manual levels

The collection is shared by the account. The existing salvage transaction automatically grants one copy of the item's **original Legendary aspect**. Imprinting a Rare item cannot create collection copies. Reimprinting a natural Legendary retains its original salvage aspect. This closes the free imprint/salvage duplication loop while preserving original Legendary salvage rewards. Item details and salvage confirmation show the result. Set items grant no aspect.

| Cumulative copies | Available level | Activation |
| ---: | ---: | --- |
| 0 | Inactive | Name and effect remain visible |
| 1 | Lv.1 | First salvage activates automatically |
| 2 | Lv.2 | Manual level-up |
| 4 | Lv.3 | Manual level-up |
| 8 | Lv.4 | Manual level-up |
| 16 | Lv.5 | Manual level-up |

Each click raises exactly one level. Neither leveling nor imprinting consumes copies. Counts continue above 16. Red dots on the library entry, filter, aspect and upgrade button remain until every currently available upgrade has been claimed. Viewing an aspect never dismisses its notification. All 123 aspects, including unregistered ones, have a stable rune-stone glyph and inspectable effects.

## Imprinting and persistence

- Equipped Rare and Legendary items are eligible; Set items are excluded. Aspect categories do not restrict equipment slots and there are no weapon/amulet multipliers. Class-specific powers only activate for their class, with a class notice in the UI.
- One aspect applies per item. A new imprint replaces the previous ability. Rare becomes Legendary while retaining all three affixes, base identity, name, level, upgrades, sockets, quality and preset references. Natural Legendary items retain four affixes.
- Equipment stores the library level at imprint time. Reimprint after upgrading to update an item. Existing equipment and historical combat/training snapshots never change retroactively.
- Duplicate equipped aspects apply once, at their highest equipped level.
- Imprinting spends no additional resources. There is no cost UI; the action reads **Imprint**.
- The atomic transaction rechecks ownership, equipped state, the complete item fingerprint and library level. Stale previews, another selected hero and an active run are rejected. Save failures leave gear and collection intact; receipt retries do not duplicate rewards.
- Save schema 9 adds an initially empty collection to legacy accounts. Existing gear is neither salvaged nor credited retroactively. Older clients preserve unsupported saves instead of overwriting them.

## Effect growth

Level one preserves the existing effects. The main magnitude uses **100% / 105% / 110% / 115% / 120%** of its original value. Stun, Root and Freeze increase duration. Last Command's internal cooldown and Narrowed Line's spread instead use **100% / 95% / 90% / 85% / 80%**. Vow of Restraint extends charge lifetime so its upgrade remains useful under the existing resource-reduction cap.

Trigger conditions, hit counts, target counts, radii and internal cooldowns remain unchanged except for named exceptions below. Existing final caps remain in force. `D` is the existing attack basis; `%p` denotes percentage points. Projectiles, delayed hits and periodic damage preserve their captured aspect level. These are initial tuning values, not a long-term balance certification.

All 123 current Legendary powers have explicit level values below. Existing effect descriptions still define every condition not listed in the table.


## Warrior

| ID | Aspect | Growing value | Lv.1 | Lv.2 | Lv.3 | Lv.4 | Lv.5 |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: |
| LW01 | Fang of the Maelstrom | Maximum pull distance | 1.5m | 1.575m | 1.65m | 1.725m | 1.8m |
| LW02 | Stride of the Falling Star | Landing shockwave damage | D90% | D94.5% | D99% | D103.5% | D108% |
| LW03 | Solitary Execution | Single-target damage bonus | 50% | 52.5% | 55% | 57.5% | 60% |
| LW04 | Final Order | Leap reset internal cooldown | 30s | 28.5s | 27s | 25.5s | 24s |
| LW05 | Turning Rack | Damage bonus | 25% | 26.25% | 27.5% | 28.75% | 30% |
| LW06 | Rising Ire | Critical chance bonus | 12%p | 12.6%p | 13.2%p | 13.8%p | 14.4%p |
| LW07 | Flesh-Cutting Wind | Total damage over time | D60% | D63% | D66% | D69% | D72% |
| LW08 | Red Rampart | Barrier fraction of maximum HP | 8% | 8.4% | 8.8% | 9.2% | 9.6% |
| LW09 | Hastening Grasp | Attack speed bonus | 12%p | 12.6%p | 13.2%p | 13.8%p | 14.4%p |
| LW10 | Parched Fury | Resource restored | 8 | 8.4 | 8.8 | 9.2 | 9.6 |
| LW11 | Scorched Landing | Extra damage | D65% | D68.25% | D71.5% | D74.75% | D78% |
| LW12 | Returning Leap | Remaining cooldown reduction | 2s | 2.1s | 2.2s | 2.3s | 2.4s |
| LW13 | Execution Foretold | Damage bonus | 35% | 36.75% | 38.5% | 40.25% | 42% |
| LW14 | Impact Cuirass | Damage reduction | 15% | 15.75% | 16.5% | 17.25% | 18% |
| LW15 | Devouring Heel | Resource restored | 18 | 18.9 | 19.8 | 20.7 | 21.6 |
| LW16 | Silencing Descent | Stun duration | 1s | 1.05s | 1.1s | 1.15s | 1.2s |
| LW17 | Crushed Crown | Damage bonus | 40% | 42% | 44% | 46% | 48% |
| LW18 | Skull Echo | Extra damage | D55% | D57.75% | D60.5% | D63.25% | D66% |
| LW19 | Deep-Carved Scar | Total damage over time | D90% | D94.5% | D99% | D103.5% | D108% |
| LW20 | Bloodied Writ | Healing fraction of maximum HP | 4% | 4.2% | 4.4% | 4.6% | 4.8% |
| LW21 | Battlefield Recall | Remaining cooldown reduction | 2s | 2.1s | 2.2s | 2.3s | 2.4s |
| LW22 | Brandbreaker | Stun duration | 1s | 1.05s | 1.1s | 1.15s | 1.2s |
| LW23 | Twice-Ringing Earth | Echo damage | D70% | D73.5% | D77% | D80.5% | D84% |
| LW24 | Binder's Cord | Resource restored | 12 | 12.6 | 13.2 | 13.8 | 14.4 |
| LW25 | Stormgate | Damage bonus | 30% | 31.5% | 33% | 34.5% | 36% |
| LW26 | Bastion of Remains | Barrier fraction of maximum HP | 12% | 12.6% | 13.2% | 13.8% | 14.4% |
| LW27 | Ankle-Binding Rift | Root duration | 1.5s | 1.575s | 1.65s | 1.725s | 1.8s |
| LW28 | Cry-Bound Strike | Cost reduction | 20%p | 21%p | 22%p | 23%p | 24%p |
| LW29 | Ironwall Shards | Extra damage | D75% | D78.75% | D82.5% | D86.25% | D90% |
| LW30 | Mending Ironcore | Healing fraction of maximum HP | 6% | 6.3% | 6.6% | 6.9% | 7.2% |
| LW31 | Unblinking Watch | Damage reduction | 15% | 15.75% | 16.5% | 17.25% | 18% |
| LW32 | Unbending Vow | Remaining cooldown reduction | 1s | 1.05s | 1.1s | 1.15s | 1.2s |
| LW33 | Answer of the Defender | Extra damage | D50% | D52.5% | D55% | D57.5% | D60% |
| LW34 | Horn of Resentment | Resource restored | 12 | 12.6 | 13.2 | 13.8 | 14.4 |
| LW35 | Comrade's Last Wish | Healing fraction of maximum HP | 8% | 8.4% | 8.8% | 9.2% | 9.6% |
| LW36 | Bloodbeat | Attack speed bonus | 15%p | 15.75%p | 16.5%p | 17.25%p | 18%p |
| LW37 | Edict of Assault | Damage bonus | 40% | 42% | 44% | 46% | 48% |
| LW38 | Unquenched Resolve | Resource restored | 5 | 5.25 | 5.5 | 5.75 | 6 |
| LW39 | Third Sentence | Damage bonus | 30% | 31.5% | 33% | 34.5% | 36% |
| LW40 | Bitter Draught Resolve | Damage reduction | 18% | 18.9% | 19.8% | 20.7% | 21.6% |

## Ranger

| ID | Aspect | Growing value | Lv.1 | Lv.2 | Lv.3 | Lv.4 | Lv.5 |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: |
| LA01 | Endless Trajectory | Per-target bonus / maximum bonus | 15%p / 60%p | 15.75%p / 63%p | 16.5%p / 66%p | 17.25%p / 69%p | 18%p / 72%p |
| LA02 | Narrowed Line | Total Multi-Shot spread | 20° | 19° | 18° | 17° | 16° |
| LA03 | Viper's Molt | Retreat trap damage per 0.5s | D30% | D31.5% | D33% | D34.5% | D36% |
| LA04 | Black Contagion | Spread fraction of extra Shadow damage | 50% | 52.5% | 55% | 57.5% | 60% |
| LA05 | Trailing Arrowhead | Echo damage | D60% | D63% | D66% | D69% | D72% |
| LA06 | Oath of the Far Hunt | Damage bonus | 30% | 31.5% | 33% | 34.5% | 36% |
| LA07 | Brand-Reading Eye | Resource restored | 6 | 6.3 | 6.6 | 6.9 | 7.2 |
| LA08 | Shadow Rupture | Extra damage | D50% | D52.5% | D55% | D57.5% | D60% |
| LA09 | Mirestring | Slow fraction | 40% | 42% | 44% | 46% | 48% |
| LA10 | Escape-Opening Arrow | Remaining cooldown reduction | 1.5s | 1.575s | 1.65s | 1.725s | 1.8s |
| LA11 | Seed of Arrow Rain | Extra damage | D35% | D36.75% | D38.5% | D40.25% | D42% |
| LA12 | Death Draws Near | Damage bonus | 30% | 31.5% | 33% | 34.5% | 36% |
| LA13 | Relentless Loading | Attack speed bonus | 15%p | 15.75%p | 16.5%p | 17.25%p | 18%p |
| LA14 | Venom-Lacquered Fletching | Total damage over time | D60% | D63% | D66% | D69% | D72% |
| LA15 | Branded Concussion | Stun duration | 1s | 1.05s | 1.1s | 1.15s | 1.2s |
| LA16 | Unhurried Archer | Cost reduction | 20%p | 21%p | 22%p | 23%p | 24%p |
| LA17 | Trapper's Aim | Damage bonus | 25% | 26.25% | 27.5% | 28.75% | 30% |
| LA18 | Venom-Harvesting Knot | Resource restored | 5 | 5.25 | 5.5 | 5.75 | 6 |
| LA19 | Trap-Leaving Step | Movement speed bonus | 20% | 21% | 22% | 23% | 24% |
| LA20 | Black Root | Root duration | 1s | 1.05s | 1.1s | 1.15s | 1.2s |
| LA21 | Brand-Seeping Venom | Damage bonus | 35% | 36.75% | 38.5% | 40.25% | 42% |
| LA22 | Last Venom Haze | Extra damage | D65% | D68.25% | D71.5% | D74.75% | D78% |
| LA23 | Venomcloud Landing | Extra damage | D60% | D63% | D66% | D69% | D72% |
| LA24 | Nimble Reload | Attack speed bonus | 15%p | 15.75%p | 16.5%p | 17.25%p | 18%p |
| LA25 | Fading Wound | Healing fraction of maximum HP | 5% | 5.25% | 5.5% | 5.75% | 6% |
| LA26 | Resetting Noose | Remaining cooldown reduction | 2s | 2.1s | 2.2s | 2.3s | 2.4s |
| LA27 | Retreater's Riposte | Damage bonus | 35% | 36.75% | 38.5% | 40.25% | 42% |
| LA28 | Shed Husk | Barrier fraction of maximum HP | 10% | 10.5% | 11% | 11.5% | 12% |
| LA29 | Quarry's Pledge | Resource restored | 10 | 10.5 | 11 | 11.5 | 12 |
| LA30 | Foreseen End | Damage bonus | 30% | 31.5% | 33% | 34.5% | 36% |
| LA31 | Veintracker | Healing fraction of maximum HP | 3% | 3.15% | 3.3% | 3.45% | 3.6% |
| LA32 | Rewritten Hunt Ledger | Remaining cooldown reduction | 3s | 3.15s | 3.3s | 3.45s | 3.6s |
| LA33 | Binding Gaze | Slow fraction | 45% | 47.25% | 49.5% | 51.75% | 54% |
| LA34 | Gloom Bloom | Extra damage | D70% | D73.5% | D77% | D80.5% | D84% |
| LA35 | Venomshade Erosion | Damage bonus | 40% | 42% | 44% | 46% | 48% |
| LA36 | Shadow Shackles | Root duration | 1s | 1.05s | 1.1s | 1.15s | 1.2s |
| LA37 | Alchemist's Pulse | Attack speed bonus | 12%p | 12.6%p | 13.2%p | 13.8%p | 14.4%p |
| LA38 | Thrice-Drawn String | Damage bonus | 25% | 26.25% | 27.5% | 28.75% | 30% |
| LA39 | Venom-Weathered Hide | Damage reduction | 15% | 15.75% | 16.5% | 17.25% | 18% |
| LA40 | Mist-Drinking Boots | Movement speed bonus | 25% | 26.25% | 27.5% | 28.75% | 30% |

## Mage

| ID | Aspect | Growing value | Lv.1 | Lv.2 | Lv.3 | Lv.4 | Lv.5 |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: |
| LM01 | Winter's Trail | Blizzard follow speed | 1m/s | 1.05m/s | 1.1m/s | 1.15m/s | 1.2m/s |
| LM02 | Echoing Ember | Echo explosion damage | D70% | D73.5% | D77% | D80.5% | D84% |
| LM03 | Knot of Feedback | Resource restored after barrier absorption | 20 | 21 | 22 | 23 | 24 |
| LM04 | Circuit of the End | Chain revisit damage fraction | 50% | 52.5% | 55% | 57.5% | 60% |
| LM05 | Lingering Ember | Total damage over time | D90% | D94.5% | D99% | D103.5% | D108% |
| LM06 | Unblemished Furnace | Damage bonus | 30% | 31.5% | 33% | 34.5% | 36% |
| LM07 | Ember Succession | Damage bonus | 25% | 26.25% | 27.5% | 28.75% | 30% |
| LM08 | Flameroot | Root duration | 1s | 1.05s | 1.1s | 1.15s | 1.2s |
| LM09 | Mana Drawn from Ash | Resource restored | 12 | 12.6 | 13.2 | 13.8 | 14.4 |
| LM10 | Rift-Opening Spark | Remaining cooldown reduction | 1s | 1.05s | 1.1s | 1.15s | 1.2s |
| LM11 | Splintered Snowflake | Echo damage | D50% | D52.5% | D55% | D57.5% | D60% |
| LM12 | Judgment of Distant Winter | Damage bonus | 35% | 36.75% | 38.5% | 40.25% | 42% |
| LM13 | Frost Husk | Barrier fraction of maximum HP | 8% | 8.4% | 8.8% | 9.2% | 9.6% |
| LM14 | Cold Circulation | Resource restored | 6 | 6.3 | 6.6 | 6.9 | 7.2 |
| LM15 | Freezing Sigil | Freeze duration | 1s | 1.05s | 1.1s | 1.15s | 1.2s |
| LM16 | Icebound Memory | Damage bonus | 40% | 42% | 44% | 46% | 48% |
| LM17 | Overflowing Charge | Extra damage | D60% | D63% | D66% | D69% | D72% |
| LM18 | Close-Quarters Judgment | Damage bonus | 35% | 36.75% | 38.5% | 40.25% | 42% |
| LM19 | Storm Continuum | Damage bonus | 20% | 21% | 22% | 23% | 24% |
| LM20 | Reclaimed Current | Resource restored | 6 | 6.3 | 6.6 | 6.9 | 7.2 |
| LM21 | Clinging Lightning | Slow fraction | 40% | 42% | 44% | 46% | 48% |
| LM22 | Wintercalling Thunder | Remaining cooldown reduction | 2s | 2.1s | 2.2s | 2.3s | 2.4s |
| LM23 | Spatial Discharge | Extra damage | D75% | D78.75% | D82.5% | D86.25% | D90% |
| LM24 | Thunderbolt Arrival | Stun duration | 1s | 1.05s | 1.1s | 1.15s | 1.2s |
| LM25 | Weightless Boundary | Movement speed bonus | 20% | 21% | 22% | 23% | 24% |
| LM26 | Leaping Flame | Damage bonus | 35% | 36.75% | 38.5% | 40.25% | 42% |
| LM27 | Riftwoven Mantle | Barrier fraction of maximum HP | 10% | 10.5% | 11% | 11.5% | 12% |
| LM28 | Mana from the Between | Resource restored | 10 | 10.5 | 11 | 11.5 | 12 |
| LM29 | Winter Refuge | Damage reduction | 15% | 15.75% | 16.5% | 17.25% | 18% |
| LM30 | Mending Crystal Veil | Healing fraction of maximum HP | 6% | 6.3% | 6.6% | 6.9% | 7.2% |
| LM31 | Charged Shell | Damage bonus | 30% | 31.5% | 33% | 34.5% | 36% |
| LM32 | Unbroken Escape Route | Remaining cooldown reduction | 1s | 1.05s | 1.1s | 1.15s | 1.2s |
| LM33 | Chill-Wrapped Knot | Slow fraction | 50% | 52.5% | 55% | 57.5% | 60% |
| LM34 | Second Winter | Echo damage | D70% | D73.5% | D77% | D80.5% | D84% |
| LM35 | Bound Mana | Resource restored | 12 | 12.6 | 13.2 | 13.8 | 14.4 |
| LM36 | Deepening Blizzard | Damage bonus | 30% | 31.5% | 33% | 34.5% | 36% |
| LM37 | Icebreaking Oath | Damage bonus | 25% | 26.25% | 27.5% | 28.75% | 30% |
| LM38 | Wellspring of Fundamentals | Resource restored | 6 | 6.3 | 6.6 | 6.9 | 7.2 |
| LM39 | Reserved Spark | Cost reduction | 20%p | 21%p | 22%p | 23%p | 24%p |
| LM40 | Crystal in the Vial | Barrier fraction of maximum HP | 12% | 12.6% | 13.2% | 13.8% | 14.4% |

## Common

| ID | Aspect | Growing value | Lv.1 | Lv.2 | Lv.3 | Lv.4 | Lv.5 |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: |
| LC01 | Watchman's Ring | Pickup radius / loot recovery movement | 2.5m / 15% | 2.625m / 15.75% | 2.75m / 16.5% | 2.875m / 17.25% | 3m / 18% |
| LC02 | Oath of Restraint | Restraint charge lifetime | 5s | 5.25s | 5.5s | 5.75s | 6s |
| LC03 | Unquenched Heart | Potion barrier fraction of maximum HP | 25% | 26.25% | 27.5% | 28.75% | 30% |

## Implementation ownership

- [AspectStone.cs](../../Assets/HELLSCRIPT/Runtime/Core/AspectStone.cs): account collection and imprint transactions.
- [AspectGrowth.cs](../../Assets/HELLSCRIPT/Runtime/Core/AspectGrowth.cs): level scaling and localized effect descriptions.
- [LegendaryPowers.cs](../../Assets/HELLSCRIPT/Runtime/Core/LegendaryPowers.cs): immutable scaled combat definitions for the expanded powers.
- [Shared UI contract](../Implementation/Shared_UI_Contract.en.md)
- [Implementation and validation](../Implementation/Aspect_Runestone_Implementation.en.md)
