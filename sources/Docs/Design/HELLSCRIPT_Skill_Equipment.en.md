# HELLSCRIPT skill and equipment matrix

Updated: 2026-09-23

[Design rules](HELLSCRIPT_Class_Skills.en.md) · [JSON](ClassSkills/catalog.json)

Links 120 existing class legendaries, 3 shared items, 18 new legendary concepts, 6 existing sets and 24 set concepts. **Direct** retains exact bound skill IDs. **Condition enablers** provide a mark, barrier or similar state without replacing an item’s bound skill. The data’s supportSkillIds has the same support-only meaning.

## 18 additional legendary concepts

Six items per class link new skill pairs. These are not registered for drops, saves or combat; weights are undecided. Ultimate items enhance only the already-selected branch.

### Warrior

| ID | Name | Slot | Skills | Unique power |
|---|---|---|---|---|
| DES_LW41 | Pursuer's Chain | Amulet | W07, W08 | Directly hooking your branded target extends the brand by 2s and restores 8 extra resource. Never extend beyond 8s from initial application; 4s interval. |
| DES_LW42 | Wound Reaper's Grasp | Hands | W09, W10 | Increase new Raking Wound bleed totals by 20%. Blood Reclamation stuns each enemy whose remaining bleed it actually cashes out for 1s, up to 5 per cast; bosses receive stagger credit. |
| DES_LW43 | Moving Rampart | Body | W11, W12 | After Resolute Advance, the next Iron Stance within 5s grants an 8% maximum-HP barrier for 4s. Once per completed advance, not refreshed by stance recasts alone. |
| DES_LW44 | Mountain Tremor | Waist | W13, W14 | An actual Battlefield Ring hit empowers the next Tremor Wave within 5s: width increases from 1m to 1.5m and direct damage gains +25% in the legendary pool. |
| DES_LW45 | Unspent Resolve | Ring | W15, W16 | Using Guardian's Vow during Breath Before Battle immediately grants and consumes its remaining fixed restoration. Add 0.25% maximum HP to Vow's barrier per resource actually restored, capped at 8%; 8s interval. |
| DES_LW46 | Ancestral King's Crown | Head | W17, W18 | With War of the Ancestors selected, the first ancestor hit grants +20% legendary-pool Whirlwind damage for 4s. With Titan's Judgment selected, restore 20 resource once on an actual hit, regardless of target count. Never unlock or cast the other ultimate. |

### Ranger

| ID | Name | Slot | Skills | Unique power |
|---|---|---|---|---|
| DES_LA41 | Patient Hunter's Bow | Weapon | A07, A08 | The first direct Successive Shots or Patient Shot hit on a marked target adds D50% secondary physical damage and restores 4 resource. Both share a 1s interval. |
| DES_LA42 | Winterthorn Grasp | Hands | A09, A10 | If Frost Snare actually hits an enemy rooted by Briar Trap, apply D90% secondary poison damage over 3s. Root or cold alone cannot retrigger it; 4s per target. |
| DES_LA43 | Decoy Mantle | Body | A11, A12 | Using Smoke Cover while your decoy lives refreshes its normal-enemy lure to 2s from now. Once per decoy; no elite or boss taunt and no extra explosion. |
| DES_LA44 | Forked Fang | Amulet | A13, A14 | If Forking Arrow's first target has your Venom Arrow poison, increase its secondary target count from 2 to 3. Secondary arrows remain D50% and copy no poison, mark or shadow charge. |
| DES_LA45 | Prepared Outpost | Waist | A15, A16 | The next Watch Ballista placed within 6s of Hunt Preparation gains 2s duration and 2 allowed shots. It cannot extend an existing ballista; one enhanced ballista per preparation. |
| DES_LA46 | Oath of Two Hunts | Ring | A17, A18 | With Killing Rain selected, its first pulse slows a marked target it actually hits by 30% for 2s. With Shadow Pursuit selected, the first valid echo grants a 12% maximum-HP barrier for 4s. Once per ultimate cast. |

### Mage

| ID | Name | Slot | Skills | Unique power |
|---|---|---|---|---|
| DES_LM41 | Kindling Grasp | Hands | M07, M08 | Ember Lance gains +30% burn total against a direct target inside your Firewall. Firewall ticks cannot retrigger this effect. |
| DES_LM42 | Glassglacier Staff | Weapon | M09, M10 | If Frost Globe hits an enemy directly frozen by Glacial Lance, cause a D60% secondary cold burst within 2m, up to 5 targets. Once per globe, with a 3s interval. Boss stagger alone is not Freeze. |
| DES_LM43 | Thunder Collector | Amulet | M11, M12 | A direct Storm Spear hit inside Capacitor Orb adds 1 charge to that orb, once per cast, sharing its cap of 3. The expiry blast is never reclassified as Storm Spear. |
| DES_LM44 | Quiet Rift | Waist | M13, M14 | After channeling Mana Reclaim uninterrupted for at least 1.5s inside your own Rift Ward, gain a 10% maximum-health shield for 4s. Internal cooldown: 6s; restarting the channel does not reset it. |
| DES_LM45 | Compass Center | Body | M15, M16 | Spending Elemental Compass's lightning charge on Magnetic Vortex increases its initial pull search radius from 3m to 4m. Damage radius, pull distance and target cap are unchanged. |
| DES_LM46 | Threefold Crownstone | Ring | M17, M18 | With Triune Collapse selected, its third stage's first hit restores 15 resource once without consuming elemental charges. With Sage Incarnate selected, three-element completion bursts grow from 2.5m to 3m. Never cast the other ultimate. |

## Existing 123 legendary links

Items without bound skills retain their original BASIC, potion or global conditions. Enabler lists are alternatives, not requirements to equip every listed skill. Full effects remain in the existing legendary document and JSON.

[Existing full legendary effects](HELLSCRIPT_Legendary_Class_Expansion.en.md)

### Warrior

| ID | Name | Slot | Direct | Enablers |
|---|---|---|---|---|
| LW01 | Fang of the Maelstrom | Weapon | W01 | — |
| LW02 | Stride of the Falling Star | Feet | W02 | — |
| LW03 | Solitary Execution | Hands | W03 | — |
| LW04 | Final Order | Amulet | W02 | — |
| LW05 | Turning Rack | Weapon | W01 | W01 |
| LW06 | Rising Ire | Hands | W01 | W06, W15 |
| LW07 | Flesh-Cutting Wind | Weapon | W01 | — |
| LW08 | Red Rampart | Body | W01 | — |
| LW09 | Hastening Grasp | Hands | W01 | — |
| LW10 | Parched Fury | Ring | W01 | — |
| LW11 | Scorched Landing | Feet | W02 | — |
| LW12 | Returning Leap | Ring | W02 | — |
| LW13 | Execution Foretold | Amulet | W02, W03 | — |
| LW14 | Impact Cuirass | Body | W02 | — |
| LW15 | Devouring Heel | Feet | W02 | — |
| LW16 | Silencing Descent | Head | W02 | — |
| LW17 | Crushed Crown | Weapon | W03 | W04, W14 |
| LW18 | Skull Echo | Hands | W03 | — |
| LW19 | Deep-Carved Scar | Weapon | W03 | — |
| LW20 | Bloodied Writ | Waist | W03 | — |
| LW21 | Battlefield Recall | Ring | W03, W06 | — |
| LW22 | Brandbreaker | Hands | W03 | W07 |
| LW23 | Twice-Ringing Earth | Weapon | W04 | — |
| LW24 | Binder's Cord | Waist | W04 | W04, W14 |
| LW25 | Stormgate | Amulet | W04, W01 | — |
| LW26 | Bastion of Remains | Body | W04 | — |
| LW27 | Ankle-Binding Rift | Feet | W04 | — |
| LW28 | Cry-Bound Strike | Head | W03 | W06 |
| LW29 | Ironwall Shards | Body | W05 | — |
| LW30 | Mending Ironcore | Waist | W05 | — |
| LW31 | Unblinking Watch | Head | * | W05, W16 |
| LW32 | Unbending Vow | Ring | W05 | W05, W16 |
| LW33 | Answer of the Defender | Hands | * | — |
| LW34 | Horn of Resentment | Amulet | W06 | — |
| LW35 | Comrade's Last Wish | Body | W06 | — |
| LW36 | Bloodbeat | Hands | W06 | — |
| LW37 | Edict of Assault | Feet | W06, W02 | — |
| LW38 | Unquenched Resolve | Ring | BASIC | — |
| LW39 | Third Sentence | Amulet | W03 | — |
| LW40 | Bitter Draught Resolve | Waist | * | — |

### Ranger

| ID | Name | Slot | Direct | Enablers |
|---|---|---|---|---|
| LA01 | Endless Trajectory | Weapon | A01 | — |
| LA02 | Narrowed Line | Hands | A02 | — |
| LA03 | Viper's Molt | Feet | A03, A04 | — |
| LA04 | Black Contagion | Amulet | A06 | — |
| LA05 | Trailing Arrowhead | Weapon | A01 | — |
| LA06 | Oath of the Far Hunt | Amulet | A01 | — |
| LA07 | Brand-Reading Eye | Head | A01 | A05 |
| LA08 | Shadow Rupture | Hands | A01 | — |
| LA09 | Mirestring | Weapon | A01 | — |
| LA10 | Escape-Opening Arrow | Feet | A01, A04 | — |
| LA11 | Seed of Arrow Rain | Weapon | A02 | — |
| LA12 | Death Draws Near | Hands | A02 | — |
| LA13 | Relentless Loading | Hands | A02 | — |
| LA14 | Venom-Lacquered Fletching | Weapon | A02 | A03, A13 |
| LA15 | Branded Concussion | Head | A02 | A05 |
| LA16 | Unhurried Archer | Ring | A02 | A16, AP09 |
| LA17 | Trapper's Aim | Amulet | A03, A01 | — |
| LA18 | Venom-Harvesting Knot | Waist | A03 | A03, A13 |
| LA19 | Trap-Leaving Step | Feet | A03 | — |
| LA20 | Black Root | Body | A03 | — |
| LA21 | Brand-Seeping Venom | Weapon | A03 | A05 |
| LA22 | Last Venom Haze | Ring | A03 | — |
| LA23 | Venomcloud Landing | Feet | A04 | — |
| LA24 | Nimble Reload | Hands | A04 | — |
| LA25 | Fading Wound | Body | A04 | — |
| LA26 | Resetting Noose | Waist | A04, A03 | — |
| LA27 | Retreater's Riposte | Amulet | A04, A02 | — |
| LA28 | Shed Husk | Body | A04 | — |
| LA29 | Quarry's Pledge | Ring | A05 | — |
| LA30 | Foreseen End | Weapon | A01 | A05 |
| LA31 | Veintracker | Waist | * | A05 |
| LA32 | Rewritten Hunt Ledger | Head | A05 | A05 |
| LA33 | Binding Gaze | Amulet | A05 | — |
| LA34 | Gloom Bloom | Weapon | A06 | — |
| LA35 | Venomshade Erosion | Hands | A06 | A03, A13 |
| LA36 | Shadow Shackles | Body | A06 | — |
| LA37 | Alchemist's Pulse | Ring | A06 | — |
| LA38 | Thrice-Drawn String | Amulet | A01 | — |
| LA39 | Venom-Weathered Hide | Body | * | A03, A13 |
| LA40 | Mist-Drinking Boots | Feet | * | — |

### Mage

| ID | Name | Slot | Direct | Enablers |
|---|---|---|---|---|
| LM01 | Winter's Trail | Weapon | M02 | — |
| LM02 | Echoing Ember | Hands | M01 | — |
| LM03 | Knot of Feedback | Waist | M05 | — |
| LM04 | Circuit of the End | Amulet | M03 | — |
| LM05 | Lingering Ember | Weapon | M01 | — |
| LM06 | Unblemished Furnace | Amulet | M01 | — |
| LM07 | Ember Succession | Hands | M01 | — |
| LM08 | Flameroot | Head | M01 | — |
| LM09 | Mana Drawn from Ash | Ring | M01 | — |
| LM10 | Rift-Opening Spark | Feet | M01, M04 | — |
| LM11 | Splintered Snowflake | Weapon | M02 | — |
| LM12 | Judgment of Distant Winter | Amulet | M02 | — |
| LM13 | Frost Husk | Body | M02 | — |
| LM14 | Cold Circulation | Ring | M02 | M02, M09, M10 |
| LM15 | Freezing Sigil | Hands | M02 | — |
| LM16 | Icebound Memory | Head | M02 | M06, M09 |
| LM17 | Overflowing Charge | Weapon | M03 | — |
| LM18 | Close-Quarters Judgment | Amulet | M03 | — |
| LM19 | Storm Continuum | Hands | M03 | — |
| LM20 | Reclaimed Current | Ring | M03 | — |
| LM21 | Clinging Lightning | Head | M03 | — |
| LM22 | Wintercalling Thunder | Waist | M03, M06 | — |
| LM23 | Spatial Discharge | Feet | M04 | — |
| LM24 | Thunderbolt Arrival | Weapon | M04 | — |
| LM25 | Weightless Boundary | Feet | M04 | — |
| LM26 | Leaping Flame | Amulet | M04, M01 | — |
| LM27 | Riftwoven Mantle | Body | M04 | — |
| LM28 | Mana from the Between | Ring | M04 | — |
| LM29 | Winter Refuge | Body | * | M02 |
| LM30 | Mending Crystal Veil | Waist | M05 | — |
| LM31 | Charged Shell | Hands | M05, M03 | — |
| LM32 | Unbroken Escape Route | Feet | M04 | M05, M14 |
| LM33 | Chill-Wrapped Knot | Waist | M05 | — |
| LM34 | Second Winter | Weapon | M06 | — |
| LM35 | Bound Mana | Ring | M06 | M02, M06, M09 |
| LM36 | Deepening Blizzard | Amulet | M06, M02 | — |
| LM37 | Icebreaking Oath | Hands | * | M06, M09 |
| LM38 | Wellspring of Fundamentals | Ring | BASIC | — |
| LM39 | Reserved Spark | Head | M01 | — |
| LM40 | Crystal in the Vial | Waist | * | — |

### Shared

| ID | Name | Slot | Direct | Enablers |
|---|---|---|---|---|
| LC01 | Watchman's Ring | Ring | BASIC / potion / global | — |
| LC02 | Oath of Restraint | Amulet | BASIC / potion / global | — |
| LC03 | Unquenched Heart | Waist | BASIC / potion / global | — |

## 30 sets and bonus tiers

All 60 new-set bonus tiers are linked. Sharing an element or shape never automatically substitutes for the original skill IDs bound by a set.

| Set | Pieces | Required original skills | Effect IDs |
|---|---|---|---|
| Executioner Discipline / REF_SW01 | 3 | — | REF_SW01_B2, REF_SW01_B3 |
| Ironwall Oath / REF_SW02 | 3 | W05 | REF_SW02_B2, REF_SW02_B3 |
| Vanguard March / REF_SW03 | 4 | W02 | REF_SW03_B2, REF_SW03_B4 |
| Trifold Armament / REF_SW04 | 4 | W01, W03 | REF_SW04_B2, REF_SW04_B4 |
| Crimson Scars / REF_SW05 | 5 | W03, W04 | REF_SW05_B2, REF_SW05_B3, REF_SW05_B5 |
| Arsenal Manual / REF_SW06 | 5 | W01, W02, W03 | REF_SW06_B2, REF_SW06_B3, REF_SW06_B5 |
| Ancestral Vanguard / REF_SW07 | 5 | W06, W02, W03 | REF_SW07_B2, REF_SW07_B3, REF_SW07_B5 |
| Colossal Fury / REF_SW08 | 6 | W03, W04, W06 | REF_SW08_B2, REF_SW08_B4, REF_SW08_B6 |
| Wandering Hunter / REF_SA01 | 3 | — | REF_SA01_B2, REF_SA01_B3 |
| Survivor Mantle / REF_SA02 | 3 | A04 | REF_SA02_B2, REF_SA02_B3 |
| Marksman Manual / REF_SA03 | 4 | A01, A02, A05 | REF_SA03_B2, REF_SA03_B4 |
| Colossus Hunt / REF_SA04 | 4 | A01, A02, A05 | REF_SA04_B2, REF_SA04_B4 |
| Vengeful Sight / REF_SA05 | 5 | A01, A02 | REF_SA05_B2, REF_SA05_B3, REF_SA05_B5 |
| Alchemical Hunting Ground / REF_SA06 | 5 | A03, A04 | REF_SA06_B2, REF_SA06_B3, REF_SA06_B5 |
| Threefold Venom / REF_SA07 | 5 | A06, A01, A02 | REF_SA07_B2, REF_SA07_B3, REF_SA07_B5 |
| Sightless Shadow / REF_SA08 | 6 | A04, A01, A02 | REF_SA08_B2, REF_SA08_B4, REF_SA08_B6 |
| Warding Aegis / REF_SM01 | 3 | M05 | REF_SM01_B2, REF_SM01_B3 |
| Deep Scholarship / REF_SM02 | 3 | M01, M02, M03, M06 | REF_SM02_B2, REF_SM02_B3 |
| Rift Pilgrimage / REF_SM03 | 4 | M04 | REF_SM03_B2, REF_SM03_B4 |
| Capricious Pact / REF_SM04 | 4 | M01, M03, M06 | REF_SM04_B2, REF_SM04_B4 |
| Overheat Crucible / REF_SM05 | 5 | M01 | REF_SM05_B2, REF_SM05_B3, REF_SM05_B5 |
| Frozen Deep / REF_SM06 | 5 | M02, M06 | REF_SM06_B2, REF_SM06_B3, REF_SM06_B5 |
| Wild Thunder / REF_SM07 | 5 | M03 | REF_SM07_B2, REF_SM07_B3, REF_SM07_B5 |
| Threefold Cycle / REF_SM08 | 6 | M01, M02, M03 | REF_SM08_B2, REF_SM08_B4, REF_SM08_B6 |
| Whirlwind Watcher / SW | 4 | W01, W02 | SW_2, SW_4 |
| Falling Star Executor / SWB | 4 | W02, W03 | SWB_2, SWB_4 |
| Poison Gravekeeper / SA | 4 | A03, A01 | SA_2, SA_4 |
| Long Shadow Tracker / SAB | 4 | A04, A01 | SAB_2, SAB_4 |
| Winter Covenant / SM | 4 | M02, M01 | SM_2, SM_4 |
| Resonating Storm / SMB | 4 | M06, M03 | SMB_2, SMB_4 |
