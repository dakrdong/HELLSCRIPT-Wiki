# Class legendary equipment: 40 items per class

Updated on: 2026-09-22. Reference checked on the same date. The values below are a HELLSCRIPT balance proposal.

## Scope and ownership

There are 40 Warrior, 40 Ranger and 40 Mage items, plus the existing 3 shared legendaries. The 24 set pieces are counted separately. This change adds 36 items per class, preserving all 12 existing class legendary IDs. Each new item has a class, equipment slot, effect, Korean and English text, and an executable combat rule in [LegendaryPowers.json](../../Assets/HELLSCRIPT/Resources/Data/LegendaryPowers.json).

The reference is [D4 Builds' Diablo IV aspect database](https://d4builds.gg/database/legendary-aspects/), inspected in the Codex in-app browser. Warrior draws on Barbarian patterns, Ranger on Rogue patterns, and Mage on Sorcerer patterns, with shared and cross-class patterns where appropriate. The reference column identifies inspiration. These are adaptations to HELLSCRIPT's 18 skills, not a one-to-one import of Diablo IV wording, values, skill rules or current balance. Names and descriptions were written for this project. Physical damage over time replaces bleed; retreat healing replaces stealth healing; the existing barrier replaces fortification. A source entry can inform several explicitly different skill interactions.

## Shared combat rules

- New item weight is 20, within the existing class-and-slot draw. This is a relative weight, not a drop percentage. Existing rarity, item-quality, random affix and acquisition rules remain the owners. Expanding the pool changes the relative chances of named items and sets. New art is not included; items use their base equipment icons.
- `D` means the attack power captured for the triggering attack. Damage bonuses from these powers share one pool capped at +150%, which multiplies the existing damage calculation. Legendary guard bonuses add within their own 50% cap, then combine multiplicatively with existing reductions. Critical chance retains the 75% cap, cost reduction the 50% cap, and speed the existing caps. These are initial tuning values, not a claim of balance validation.
- A hit is one actual target damage event. Area attacks count each target; channel and field damage count their paid or active ticks. Counts reset after 5 seconds without a counted hit. During internal cooldown, hits neither count nor queue a proc. Critical triggers need an actual critical result; kill conditions are checked before the lethal source hit.
- Cast triggers run after release or landing, following preparation and destination validation. A miss can spend a cast trigger, while cancelled preparation cannot grant one. On-hit powers require actual HP damage. HP-potion effects need successful healing. Incoming effects require a surviving, non-dodged hit; a block effect also requires a successful block.
- Extra legendary damage does not roll critical, overpower or lucky hit and cannot trigger this catalogue again. An extra-damage kill cannot be misattributed as a source-skill kill. Bursts and crowd control use line of sight and the nearest 5 living targets, with enemy ID as the tie-breaker. A cast burst is centered on the hero's position after release, including the landing point of a mobility skill.
- Delayed area explosions keep their original position. Single-target echoes and damage over time follow the original living target and fizzle on its death. The same power on the same target refreshes its pending effect without stacking. Damage-over-time refreshes update duration and captured power while preserving the next one-second tick. Identical timed buffs refresh without stacking. Barriers reuse existing amount, duration, replacement and total-HP caps.
- Stun, freeze and root use existing boss-stagger credit instead of immobilizing bosses. Frozen-only bonuses therefore do not apply merely because a boss is staggered. Target predicates for damage are checked at impact; temporary offensive bonuses and equipped damage powers are captured with an attack.
- Counts, cooldown deadlines, active buffs and pending damage are serialized with the run. Pause stops simulation time. Deadlines use the existing simulation tick to avoid floating-point drift. Equipment removal clears active buffs and counts but retains cooldown deadlines. Already-created damage keeps its snapshot; on-hit procs additionally require the power to remain equipped. Equipping a power after firing cannot add a proc to that old attack.
- Heal effects use the existing healing multiplier; resource and HP restoration cannot exceed their maximum. Cooldown effects apply only to an equipped affected skill and cannot reduce a remaining cooldown below zero. When HP/resource is full or the affected cooldown cannot be reduced, no proc is spent.

## Catalogue

The first four rows in each class retain their prior implementation and validation scope. New rows list their specific Diablo IV reference. `*` in the executable data means any source or affected attack; `BASIC` means the basic attack.

### Warrior · 40

| ID | Name | Slot | Effect | Reference |
|---|---|---|---|---|
| LW01 | Fang of the Maelstrom | Weapon | After Whirlwind has been held for 1 second, each paid tick pulls enemies within 3.5m up to 1.5m closer, once per second. They stop at walls and bodies, and bosses and immobile enemies are not pulled. | [Existing implementation](../Implementation/Legendary_Expansion.md) |
| LW02 | Stride of the Falling Star | Feet | Adds a 4m radius D90% physical shockwave to a leap landing. | [Existing implementation](../Implementation/Legendary_Expansion.md) |
| LW03 | Solitary Execution | Hands | A Crushing Blow that hits exactly one enemy adds +50% to its direct damage. | [Existing implementation](../Implementation/Legendary_Expansion.md) |
| LW04 | Final Order | Amulet | When a hit takes you from above 30% HP to 30% or below, the cooldown on your equipped leap resets. It applies only if you survive and waits 30 seconds between triggers. | [Existing implementation](../Implementation/Legendary_Expansion.md) |
| LW05 | Turning Rack | Weapon | While channeling whirlwind for at least 2s, Whirlwind damage gains +25% in the legendary damage pool. | Aspect of Channeling |
| LW06 | Rising Ire | Hands | While resource is at least 75%, Whirlwind critical chance gains 12 percentage points. | Aspect of Anger Management |
| LW07 | Flesh-Cutting Wind | Weapon | After every 3 qualifying Whirlwind target hits, deal a total of D60% Physical damage to the hit target in one-second ticks over 3s. Internal cooldown: 1s. The hit count resets after 5s without another counted hit. | Aspect of Berserk Ripping |
| LW08 | Red Rampart | Body | After every 4 qualifying Whirlwind target hits, gain a barrier equal to 8% maximum HP for 3s. Internal cooldown: 4s. The hit count resets after 5s without another counted hit. | Steadfast Berserker's Aspect |
| LW09 | Hastening Grasp | Hands | On Whirlwind critical hit, gain 12 percentage points of attack speed for new attacks for 4s. Channel and field tick intervals are unchanged. Internal cooldown: 6s. | Accelerating Aspect |
| LW10 | Parched Fury | Ring | On Whirlwind hit, while resource is at most 30%, restore 8 resource. Internal cooldown: 2s. | Aspect of Berserk Fury |
| LW11 | Scorched Landing | Feet | On Leap release, deal D65% extra Fire damage to up to 5 enemies within 3m of you. Internal cooldown: 4s. | Aspect of Incendiary Fissures |
| LW12 | Returning Leap | Ring | On Leap hit, with at least 3 living enemies within 3m of you, reduce equipped Leap's remaining cooldown by 2s. Internal cooldown: 4s. | Bear Clan Berserker's Aspect |
| LW13 | Execution Foretold | Amulet | On Leap release, gain +35% Crush damage in the legendary pool for 4s. Internal cooldown: 4s. | Aspect of Encroaching Wrath |
| LW14 | Impact Cuirass | Body | On Leap release, take 15% less damage for 3s. Internal cooldown: 4s. | Battle Fervor's Aspect |
| LW15 | Devouring Heel | Feet | On Leap kill, restore 18 resource. Internal cooldown: 2s. | Aspect of Voracious Rage |
| LW16 | Silencing Descent | Head | On Leap release, apply stun to up to 5 enemies within 3m of you for 1s. Internal cooldown: 8s. | Aspect of Audacity |
| LW17 | Crushed Crown | Weapon | Against a stunned, frozen, rooted or staggered target, Crush damage gains +40% in the legendary damage pool. | Aspect of Walloping |
| LW18 | Skull Echo | Hands | On Crush critical hit, deal D55% extra Physical damage to up to 5 enemies within 2.5m of the hit target. Internal cooldown: 2s. | Skullbreaker's Aspect |
| LW19 | Deep-Carved Scar | Weapon | On Crush hit, deal a total of D90% Physical damage to the hit target in one-second ticks over 4s. Internal cooldown: 4s. | Cut to the Bone Aspect |
| LW20 | Bloodied Writ | Waist | On Crush hit, while your HP is at most 35%, heal for 4% of maximum HP. Internal cooldown: 4s. | Aspect of Siphoning Strikes |
| LW21 | Battlefield Recall | Ring | On Crush kill, reduce equipped War Cry's remaining cooldown by 2s. Internal cooldown: 2s. | Bear Clan Berserker's Aspect |
| LW22 | Brandbreaker | Hands | On Crush hit, against a marked target, apply stun to the hit target for 1s. Internal cooldown: 5s. | Aspect of Anemia |
| LW23 | Twice-Ringing Earth | Weapon | On Slam release, deal D70% extra Physical damage to up to 5 enemies within 3m of you after 0.4s. Internal cooldown: 5s. | Heavy Hitting Aspect |
| LW24 | Binder's Cord | Waist | On Slam hit, against a stunned, frozen, rooted or staggered target, restore 12 resource. Internal cooldown: 3s. | Aspect of the Umbral |
| LW25 | Stormgate | Amulet | On Slam release, gain +30% Whirlwind damage in the legendary pool for 5s. Internal cooldown: 6s. | Unrelenting Aspect |
| LW26 | Bastion of Remains | Body | On Slam kill, gain a barrier equal to 12% maximum HP for 4s. Internal cooldown: 6s. | Irrepressible Aspect |
| LW27 | Ankle-Binding Rift | Feet | On Slam hit, apply root to the hit target for 1.5s. Internal cooldown: 5s. | Weapon Master's Aspect |
| LW28 | Cry-Bound Strike | Head | While war cry is active, Crush resource cost reduction gains 20 percentage points, within the overall 50% cap. | Aspect of Booming Voice |
| LW29 | Ironwall Shards | Body | On Iron Wall release, deal D75% extra Physical damage to up to 5 enemies within 3m of you. Internal cooldown: 5s. | Sticker-thought Aspect |
| LW30 | Mending Ironcore | Waist | On Iron Wall release, heal for 6% of maximum HP. Internal cooldown: 6s. | Raid Leader's Aspect |
| LW31 | Unblinking Watch | Head | While you have a barrier, take 15% less damage. | Snowveiled Aspect |
| LW32 | Unbending Vow | Ring | On surviving a hit, while you have a barrier, reduce equipped Iron Wall's remaining cooldown by 1s. Internal cooldown: 3s. | Aspect of Heavenly Strength |
| LW33 | Answer of the Defender | Hands | On surviving a blocked hit, deal D50% extra Physical damage to up to 5 enemies within 3m of you. Internal cooldown: 3s. | Aspect of Akarat's Blessing |
| LW34 | Horn of Resentment | Amulet | On War Cry release, restore 12 resource. Internal cooldown: 6s. | Aspect of Vocalized Empowerment |
| LW35 | Comrade's Last Wish | Body | On War Cry release, heal for 8% of maximum HP. Internal cooldown: 8s. | Raid Leader's Aspect |
| LW36 | Bloodbeat | Hands | On War Cry release, gain 15 percentage points of attack speed for new attacks for 5s. Channel and field tick intervals are unchanged. Internal cooldown: 8s. | Battle-Mad Aspect |
| LW37 | Edict of Assault | Feet | On War Cry release, gain +40% Leap damage in the legendary pool for 5s. Internal cooldown: 8s. | Aspect of Encroaching Wrath |
| LW38 | Unquenched Resolve | Ring | On Basic Attack hit, restore 5 resource. Internal cooldown: 0.5s. | Aspect of Adaptability |
| LW39 | Third Sentence | Amulet | After every 3 qualifying Basic Attack target hits, gain +30% Crush damage in the legendary pool for 4s. Internal cooldown: 3s. The hit count resets after 5s without another counted hit. | Aspect of the Expectant |
| LW40 | Bitter Draught Resolve | Waist | On actual healing from an HP potion, take 18% less damage for 5s. Internal cooldown: 12s. | Aspect of the Fortress |

### Ranger · 40

| ID | Name | Slot | Effect | Reference |
|---|---|---|---|---|
| LA01 | Endless Trajectory | Weapon | Each further target of a Piercing Shot takes +15%p damage, up to +60%p. | [Existing implementation](../Implementation/Legendary_Expansion.md) |
| LA02 | Narrowed Line | Hands | Multishot narrows to 20 degrees, landing up to 3 arrows on the same enemy. | [Existing implementation](../Implementation/Legendary_Expansion.md) |
| LA03 | Viper's Molt | Feet | Retreat Leap leaves a trap where it started. It arms in 0.5 seconds, waits up to 8 seconds and deals damage for 3 seconds once triggered. Venom Trap has to be equipped, and placed traps and these count together toward the limit of 2. | [Existing implementation](../Implementation/Legendary_Expansion.md) |
| LA04 | Black Contagion | Amulet | The Shadow Arrow bonus damage on a poisoned target spreads to nearby enemies. It does not chain again. | [Existing implementation](../Implementation/Legendary_Expansion.md) |
| LA05 | Trailing Arrowhead | Weapon | On Piercing Shot hit, deal D60% extra Physical damage to the hit target after 0.4s. Internal cooldown: 1.5s. | Aspect of Arrow Storms |
| LA06 | Oath of the Far Hunt | Amulet | Against a target at least 7m away, Piercing Shot damage gains +30% in the legendary damage pool. | Aspect of Retribution |
| LA07 | Brand-Reading Eye | Head | On Piercing Shot hit, against a marked target, restore 6 resource. Internal cooldown: 2s. | Aspect of True Sight |
| LA08 | Shadow Rupture | Hands | On Piercing Shot critical hit, deal D50% extra Shadow damage to up to 5 enemies within 2.5m of the hit target. Internal cooldown: 2s. | Aspect of Unstable Imbuements |
| LA09 | Mirestring | Weapon | On Piercing Shot hit, apply 40% slow to the hit target for 3s. Internal cooldown: 3s. | Aspect of Binding Morass |
| LA10 | Escape-Opening Arrow | Feet | On Piercing Shot kill, reduce equipped Retreat Leap's remaining cooldown by 1.5s. Internal cooldown: 2s. | Aspect of Synergy |
| LA11 | Seed of Arrow Rain | Weapon | After every 3 qualifying Multi-Shot target hits, deal D35% extra Physical damage to up to 5 enemies within 2.5m of the hit target. Internal cooldown: 1.5s. The hit count resets after 5s without another counted hit. | Aspect of Arrow Storms |
| LA12 | Death Draws Near | Hands | Against a target within 3m, Multi-Shot damage gains +30% in the legendary damage pool. | Aspect of Encircling Blades |
| LA13 | Relentless Loading | Hands | On Multi-Shot critical hit, gain 15 percentage points of attack speed for new attacks for 3s. Channel and field tick intervals are unchanged. Internal cooldown: 5s. | Accelerating Aspect |
| LA14 | Venom-Lacquered Fletching | Weapon | On Multi-Shot hit, against a poisoned target, deal a total of D60% Poison damage to the hit target in one-second ticks over 3s. Internal cooldown: 2s. | Aspect of Corruption |
| LA15 | Branded Concussion | Head | On Multi-Shot hit, against a marked target, apply stun to the hit target for 1s. Internal cooldown: 5s. | Aspect of Concussive Blend |
| LA16 | Unhurried Archer | Ring | While resource is at least 75%, Multi-Shot resource cost reduction gains 20 percentage points, within the overall 50% cap. | Aspect of Efficiency |
| LA17 | Trapper's Aim | Amulet | On Poison Trap hit, gain +25% Piercing Shot damage in the legendary pool for 4s. Internal cooldown: 5s. | Aspect of Entrapment |
| LA18 | Venom-Harvesting Knot | Waist | On Poison Trap hit, against a poisoned target, restore 5 resource. Internal cooldown: 2s. | Sapping Aspect |
| LA19 | Trap-Leaving Step | Feet | On Poison Trap release, gain 20% movement speed for 5s, within the overall 50% bonus cap. Internal cooldown: 6s. | Aspect of Explosive Verve |
| LA20 | Black Root | Body | After every 3 qualifying Poison Trap target hits, apply root to the hit target for 1s. Internal cooldown: 4s. The hit count resets after 5s without another counted hit. | Aspect of Crippling Darkness |
| LA21 | Brand-Seeping Venom | Weapon | Against a marked target, Poison Trap damage gains +35% in the legendary damage pool. | Aspect of Malice |
| LA22 | Last Venom Haze | Ring | On Poison Trap kill, deal D65% extra Poison damage to up to 5 enemies within 3m of the hit target. Internal cooldown: 2s. | Aspect of Contamination |
| LA23 | Venomcloud Landing | Feet | On Retreat Leap release, deal D60% extra Poison damage to up to 5 enemies within 3m of you. Internal cooldown: 5s. | Aspect of Poisonous Clouds |
| LA24 | Nimble Reload | Hands | On Retreat Leap release, gain 15 percentage points of attack speed for new attacks for 4s. Channel and field tick intervals are unchanged. Internal cooldown: 5s. | Agile Aspect |
| LA25 | Fading Wound | Body | On Retreat Leap release, heal for 5% of maximum HP. Internal cooldown: 6s. | Aspect of Mending Obscurity |
| LA26 | Resetting Noose | Waist | On Retreat Leap release, reduce equipped Poison Trap's remaining cooldown by 2s. Internal cooldown: 4s. | Aspect of Synergy |
| LA27 | Retreater's Riposte | Amulet | On Retreat Leap release, gain +35% Multi-Shot damage in the legendary pool for 4s. Internal cooldown: 6s. | Aspect of Stolen Vigor |
| LA28 | Shed Husk | Body | On Retreat Leap release, gain a barrier equal to 10% maximum HP for 3s. Internal cooldown: 6s. | Aspect of Coagulation |
| LA29 | Quarry's Pledge | Ring | On Hunter's Mark release, restore 10 resource. Internal cooldown: 3s. | Aspect of Aftermath |
| LA30 | Foreseen End | Weapon | Against a marked target, Piercing Shot damage gains +30% in the legendary damage pool. | Aspect of True Sight |
| LA31 | Veintracker | Waist | On all attacks hit, against a marked target, heal for 3% of maximum HP. Internal cooldown: 3s. | Aspect of Siphoning Strikes |
| LA32 | Rewritten Hunt Ledger | Head | On all attacks kill, if the target was marked before the lethal hit, reduce equipped Hunter's Mark's remaining cooldown by 3s. Internal cooldown: 3s. | Aspect of the Orange Herald |
| LA33 | Binding Gaze | Amulet | On Hunter's Mark release, apply 45% slow to up to 5 enemies within 3m of you for 4s. Internal cooldown: 4s. | Aspect of Binding Morass |
| LA34 | Gloom Bloom | Weapon | On Shadow Arrow release, deal D70% extra Shadow damage to up to 5 enemies within 3m of you. Internal cooldown: 6s. | Aspect of Unstable Imbuements |
| LA35 | Venomshade Erosion | Hands | Against a poisoned target, Shadow Arrow damage gains +40% in the legendary damage pool. | Aspect of Bitter Infection |
| LA36 | Shadow Shackles | Body | On Shadow Arrow hit, apply root to the hit target for 1s. Internal cooldown: 4s. | Aspect of Crippling Darkness |
| LA37 | Alchemist's Pulse | Ring | On Shadow Arrow hit, gain 12 percentage points of attack speed for new attacks for 4s. Channel and field tick intervals are unchanged. Internal cooldown: 5s. | Aspect of Alchemical Advantage |
| LA38 | Thrice-Drawn String | Amulet | After every 3 qualifying Basic Attack target hits, gain +25% Piercing Shot damage in the legendary pool for 4s. Internal cooldown: 3s. The hit count resets after 5s without another counted hit. | Aspect of the Expectant |
| LA39 | Venom-Weathered Hide | Body | On surviving a hit, if the attacker is poisoned, take 15% less damage for 4s. Internal cooldown: 8s. | Aspect of Debilitating Toxins |
| LA40 | Mist-Drinking Boots | Feet | On actual healing from an HP potion, gain 25% movement speed for 6s, within the overall 50% bonus cap. Internal cooldown: 12s. | Aspect of Nebulous Brews |

### Mage · 40

| ID | Name | Slot | Effect | Reference |
|---|---|---|---|---|
| LM01 | Winter's Trail | Weapon | Lets Blizzard either stay in place or follow the target. Following moves 1m per second up to 4m in total, and overlapping zones apply only the strongest damage. | [Existing implementation](../Implementation/Legendary_Expansion.md) |
| LM02 | Echoing Ember | Hands | 0.4 seconds after a Fireball lands, a second D70% fire blast covers a 3.5m radius. | [Existing implementation](../Implementation/Legendary_Expansion.md) |
| LM03 | Knot of Feedback | Waist | Once an Elemental Shield has absorbed 15% of the max HP it was made with, it restores 20 resource. Once per shield, waiting 4 seconds between triggers. | [Existing implementation](../Implementation/Legendary_Expansion.md) |
| LM04 | Circuit of the End | Amulet | Chain Lightning can visit the same enemy twice, the second hit dealing half damage. The total hit count stays the same and it never revisits the enemy it just left. | [Existing implementation](../Implementation/Legendary_Expansion.md) |
| LM05 | Lingering Ember | Weapon | On Fireball hit, deal a total of D90% Fire damage to the hit target in one-second ticks over 4s. Internal cooldown: 3s. | Aspect of Searing Impact |
| LM06 | Unblemished Furnace | Amulet | While your hp is at least 80%, Fireball damage gains +30% in the legendary damage pool. | Aspect of the Untarnished Blaze |
| LM07 | Ember Succession | Hands | On Fireball critical hit, gain +25% Fireball damage in the legendary pool for 4s. Internal cooldown: 5s. | Aspect of Combustion |
| LM08 | Flameroot | Head | On Fireball hit, apply root to the hit target for 1s. Internal cooldown: 5s. | Aspect of Mind's Awakening |
| LM09 | Mana Drawn from Ash | Ring | On Fireball kill, restore 12 resource. Internal cooldown: 2s. | Aspect of Voracious Rage |
| LM10 | Rift-Opening Spark | Feet | On Fireball hit, reduce equipped Teleport's remaining cooldown by 1s. Internal cooldown: 3s. | Aspect of Elemental Attunement |
| LM11 | Splintered Snowflake | Weapon | After every 3 qualifying Blizzard target hits, deal D50% extra Cold damage to up to 5 enemies within 2.5m of the hit target after 0.4s. Internal cooldown: 1.5s. The hit count resets after 5s without another counted hit. | Aspect of the Frozen Wake |
| LM12 | Judgment of Distant Winter | Amulet | Against a target at least 7m away, Blizzard damage gains +35% in the legendary damage pool. | Aspect of Cold Judgement |
| LM13 | Frost Husk | Body | On Blizzard hit, gain a barrier equal to 8% maximum HP for 3s. Internal cooldown: 4s. | Aspect of Mind's Awakening |
| LM14 | Cold Circulation | Ring | On Blizzard hit, against a slowed target, restore 6 resource. Internal cooldown: 2s. | Aspect Of Elemental Acuity |
| LM15 | Freezing Sigil | Hands | After every 4 qualifying Blizzard target hits, apply freeze to the hit target for 1s. Internal cooldown: 5s. The hit count resets after 5s without another counted hit. | Aspect of Piercing Cold |
| LM16 | Icebound Memory | Head | Against a frozen target, Blizzard damage gains +40% in the legendary damage pool. | Aspect of Frozen Memories |
| LM17 | Overflowing Charge | Weapon | On Chain Lightning critical hit, deal D60% extra Lightning damage to up to 5 enemies within 2.5m of the hit target. Internal cooldown: 2s. | Aspect of Abundant Energy |
| LM18 | Close-Quarters Judgment | Amulet | Against a target within 3m, Chain Lightning damage gains +35% in the legendary damage pool. | Aspect of Arrogance |
| LM19 | Storm Continuum | Hands | On Chain Lightning critical hit, gain +20% Chain Lightning damage in the legendary pool for 4s. Internal cooldown: 4s. | Aspect of Overwhelming Currents |
| LM20 | Reclaimed Current | Ring | On Chain Lightning hit, restore 6 resource. Internal cooldown: 2s. | Aspect Of Elemental Acuity |
| LM21 | Clinging Lightning | Head | On Chain Lightning hit, apply 40% slow to the hit target for 3s. Internal cooldown: 3s. | Aspect of Splintering Energy |
| LM22 | Wintercalling Thunder | Waist | On Chain Lightning kill, reduce equipped Frost Nova's remaining cooldown by 2s. Internal cooldown: 3s. | Aspect of the Orange Herald |
| LM23 | Spatial Discharge | Feet | On Teleport release, deal D75% extra Lightning damage to up to 5 enemies within 3m of you. Internal cooldown: 5s. | Aspect of Metamorphosis |
| LM24 | Thunderbolt Arrival | Weapon | On Teleport release, apply stun to up to 5 enemies within 3m of you for 1s. Internal cooldown: 8s. | Aspect of Audacity |
| LM25 | Weightless Boundary | Feet | On Teleport release, gain 20% movement speed for 4s, within the overall 50% bonus cap. Internal cooldown: 6s. | Aspect Of Tenuous Agility |
| LM26 | Leaping Flame | Amulet | On Teleport release, gain +35% Fireball damage in the legendary pool for 4s. Internal cooldown: 6s. | Aspect of Armageddon |
| LM27 | Riftwoven Mantle | Body | On Teleport release, gain a barrier equal to 10% maximum HP for 3s. Internal cooldown: 6s. | Aspect of Concentration |
| LM28 | Mana from the Between | Ring | On Teleport release, restore 10 resource. Internal cooldown: 4s. | Aspect Of Elemental Acuity |
| LM29 | Winter Refuge | Body | While your blizzard field is active, take 15% less damage. | Aspect of Arcane Ward |
| LM30 | Mending Crystal Veil | Waist | On Elemental Shield release, heal for 6% of maximum HP. Internal cooldown: 8s. | Aspect of Shelter |
| LM31 | Charged Shell | Hands | On Elemental Shield release, gain +30% Chain Lightning damage in the legendary pool for 4s. Internal cooldown: 6s. | Aspect of Armageddon |
| LM32 | Unbroken Escape Route | Feet | On surviving a hit, while you have a barrier, reduce equipped Teleport's remaining cooldown by 1s. Internal cooldown: 3s. | Aspect of Refutation |
| LM33 | Chill-Wrapped Knot | Waist | On Elemental Shield release, apply 50% slow to up to 5 enemies within 3m of you for 3s. Internal cooldown: 5s. | Aspect of Binding Morass |
| LM34 | Second Winter | Weapon | On Frost Nova release, deal D70% extra Cold damage to up to 5 enemies within 3m of you after 0.4s. Internal cooldown: 6s. | Aspect of the Frozen Wake |
| LM35 | Bound Mana | Ring | On Frost Nova hit, against a stunned, frozen, rooted or staggered target, restore 12 resource. Internal cooldown: 4s. | Aspect of the Umbral |
| LM36 | Deepening Blizzard | Amulet | On Frost Nova release, gain +30% Blizzard damage in the legendary pool for 5s. Internal cooldown: 8s. | Aspect of Merciless Cold |
| LM37 | Icebreaking Oath | Hands | Against a frozen target, all attacks damage gains +25% in the legendary damage pool. | Aspect of Frozen Memories |
| LM38 | Wellspring of Fundamentals | Ring | On Basic Attack hit, restore 6 resource. Internal cooldown: 0.5s. | Aspect of Adaptability |
| LM39 | Reserved Spark | Head | After every 2 qualifying Basic Attack target hits, gain 20 percentage points of Fireball cost reduction for 4s, within the overall 50% cap. Internal cooldown: 3s. The hit count resets after 5s without another counted hit. | Aspect of Efficiency |
| LM40 | Crystal in the Vial | Waist | On actual healing from an HP potion, gain a barrier equal to 12% maximum HP for 4s. Internal cooldown: 12s. | Aspect of the Protector |

## Validation

See [implementation and verification](../Implementation/Legendary_Powers_Implementation.en.md) for the exact tests, native-player proof and remaining limitations.
