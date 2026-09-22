# HELLSCRIPT 24 class set concepts and references

Updated / 정리 기준일: 2026-09-22

**Status: design draft; not in the game; tuning untested**

[한국어](HELLSCRIPT_Class_Set_Reference.md)

[Set catalog (JSON)](ClassSetReference/catalog.json) · [Existing item rules](HELLSCRIPT_Itemization_Detail.md)

This document is generated from the JSON catalog. Edit that source first, then regenerate using the maintenance commands below.

## Composition

| Class | 3 pieces | 4 pieces | 5 pieces | 6 pieces | Sets / pieces |
|---|---:|---:|---:|---:|---:|
| Warrior | 2 | 2 | 3 | 1 | 8 / 35 |
| Mage | 2 | 2 | 3 | 1 | 8 / 35 |
| Ranger | 2 | 2 | 3 | 1 | 8 / 35 |
| Total | 6 | 6 | 9 | 3 | **24 / 105** |

There are 60 bonus tiers. Warrior maps to Barbarian, Mage to Sorcerer, and Ranger to Rogue. Shared source sets may appear across classes, but each class uses eight distinct sources.

### Concepts by set size

| Pieces | Warrior | Mage | Ranger |
|---|---|---|---|
| 3 | Executioner Discipline (Elite hunting); Ironwall Oath (Sustained defense) | Warding Aegis (Barrier survival); Deep Scholarship (Skill ranks) | Wandering Hunter (Movement and collection); Survivor Mantle (Hit recovery) |
| 4 | Vanguard March (Mobility and farming); Trifold Armament (Elemental procs) | Rift Pilgrimage (Teleport and exploration); Capricious Pact (Lucky elemental procs) | Marksman Manual (Shooting mastery); Colossus Hunt (Elite and boss focus) |
| 5 | Crimson Scars (Bleed accumulation and cash-out); Arsenal Manual (Alternating skill combos); Ancestral Vanguard (Shout and ancestral support) | Overheat Crucible (Sustained fire casting); Frozen Deep (Frostbite and freeze cash-out); Wild Thunder (Random lightning spikes) | Vengeful Sight (Basic-shot barrage); Alchemical Hunting Ground (Traps and crowd control); Threefold Venom (Imbued shooting) |
| 6 | Colossal Fury (High-resource heavy attacks) | Threefold Cycle (Three-element rotation) | Sightless Shadow (Retreat and shadow echoes) |

## Shared design rules

### Scope

These are 24 new candidates, eight per class, separate from the six existing SW/SA/SM/SWB/SAB/SMB sets. Replacement or integration is undecided. Existing definitions, combat, drops, and saves are unchanged. Each candidate includes pieces, bonus thresholds, bilingual text, and provenance.

### Source and adaptation

The sources are charm sets equipped in the Diablo IV: Lord of Hatred Talisman, structurally different from HELLSCRIPT equipment sets. Five shared sets and twelve class sets inform the 24 candidates. Proposed names, slots, 3/4/5/6-piece compositions, and numbers are HELLSCRIPT adaptations, not official translations or copied Diablo IV tuning.

### Source verification limits

On 2026-09-22, Blizzard expansion and 3.1 patch pages confirmed the Talisman set system; InfinityBuilds class filters and 17 individual pages supplied set names, counts, and mechanics. Individual summaries rely on that community database. No exhaustive live-client comparison or current-season build validation was performed.

### Equipment and counting

Only the eight existing slot categories are used: weapon, head, chest, hands, feet, waist, amulet, and ring. Slots never repeat within a set. Weapons use valid existing bases for their class. One two-handed item counts as one piece even if it occupies two equip positions. Count distinct definition IDs actually owned and equipped by the correct class; exclude storage and previews. Proposed Legendary rarity follows existing set classification. Base stats, affixes, and quality follow existing item rules.

### Bonus thresholds

Three-piece sets use 2/3 thresholds, four-piece sets 2/4, five-piece sets 2/3/5, and six-piece sets 2/4/6. All reached thresholds apply cumulatively, except explicitly replaced values. Colossal Fury at six replaces four-piece damage/area bonuses but retains 20% longer preparation/recovery. Same-size candidates within each class emphasize different actions.

### Damage calculation and caps

D is the existing attack value after primary attributes; 80% D means D×0.8, not 80% of final damage. Ordinary damage increases enter the existing additive bucket once. Shared caps remain: 50% cost reduction, 40% cooldown reduction, 50% attack/movement speed increase, 75% critical chance, 50% buff mitigation, 6 m pickup radius, and +5 bonus skill ranks. Armor/resistance increases are not mitigation percentage points; keep physical 75% and nonphysical 70% mitigation caps. Area bonuses scale the original radius/length/width, not angle, target limit, or travel distance.

### Hit and secondary-effect rules

Direct damage means the original basic attack or skill hit, excluding ground ticks, bleeding, and secondary set damage. Direct shooting means basic shots, A01, and A02. Per-cast effects do not repeat for pierce, multiple arrows, or chain targets. New secondary damage never becomes a basic/direct/Lucky Hit event and cannot recursively trigger sets, legendaries, resource/healing procs, or A06 charge consumption. New secondary damage and DOTs do not roll critical hits unless explicitly specified. Existing original-skill and A06 critical rules remain unchanged.

### Casts, timing, and retriggering

A valid cast passes resource, cooldown, and execution checks; free movement/defense casts can qualify. Durations use gameplay time and pause with combat. Cast conditions, Heat, Vengeance, and resource ratios are captured before casting; newly earned stacks do not retroactively affect that cast. Internal cooldowns start on actual procs, ignore ordinary cooldown reduction, and cannot reset through recasting or re-equipping. Random outcomes are drawn once from the stored cast ID and combat RNG sequence.

### Charges and delayed attacks

Ancestral Vanguard and Threefold Cycle reserve/consume a next-attack charge at the next eligible valid cast start. Expiry during preparation does not cancel a reservation, but interruption, a miss, or scene end gives no refund. Hit-dependent extra attacks require an actual hit. Threefold charges cannot affect their generating third cast or an already active ground effect. Sightless Shadow stores release origin/direction and fires even when the original misses, but each target takes one echo hit regardless of arrow count.

### Bleeding, Frostbite, and poison

DOT ledgers store pre-defense damage with attacker modifiers applied once. On damage or cash-out, apply target mitigation once without reapplying attacker bonuses. Crimson Scars and Threefold Venom keep the higher of incoming total and existing remaining damage, refresh duration, and preserve the tick phase. Each Frostbite contribution drains over six seconds. Reject new overflow above 90% D using the new cast snapshot; a weaker cast cannot erase existing stored damage. Overlapping same-family Blizzards contribute only their strongest valid tick. Cash-out removes only the owning set ledger.

### Combos, barriers, and target state

Combo windows begin with the first valid step and clear on completion/expiry. Colossus Hunt clears unfinished counts on marked-target change, death, or mark expiry. Threefold adds 5% current maximum Life to its remaining barrier for each newly recorded element, capped at 15%; repeated elements refresh neither amount nor duration. Other same-source barriers follow existing refresh rules. Stun, freeze, and slow retain existing boss stagger/immunity handling. Life/resource restoration cannot exceed their maxima.

### Equipment changes and persistence

Losing the required count, changing class, or ending the combat scene removes the affected tier buffs, charges, delayed attacks, and DOTs. Internal cooldowns survive unequip/re-equip during combat. Never auto-map candidate IDs to legacy sets or inject owned items. This file lives outside runtime Resources with runtimeEnabled=false. Future implementation must validate in-progress combat restoration for casts, reservations, RNG, and DOTs.

### Rewards, drops, and balance

Gold/XP bonuses apply only to monster kill rewards. Keep hero XP separate from rune mastery; exclude sales, crafting, salvage, quests, and duplicate payouts. Drop weights are undecided (null), and candidates are not registered for drops. Numbers are comparison drafts requiring equal item level, quality, upgrades, and combat duration against existing sets and rare/legendary mixes. More pieces are not automatically stronger.

## Official system context

- [Diablo IV: Lord of Hatred](https://diablo4.blizzard.com/en-us/lord-of-hatred)
- [Diablo IV Patch Notes (3.1)](https://news.blizzard.com/en-us/article/24287406/diablo-iv-patch-notes-3-1)

## Warrior

### Executioner Discipline · 3 pieces

`REF_SW01` · Elite hunting

**Source:** [Slaughter](https://tools.infinitybuilds.gg/en/database/talismans/phoba-of-slaughter) · All classes · 3 charms; thresholds 2/3.

Combines offense and defense with an additional advantage against elites.

**Gameplay:** Focus elites for an advantage in short engagements.

**Adaptation:** Moves the shared elite specialization to three Warrior equipment pieces.

| Piece ID | Proposed name | Slot |
|---|---|---|
| `REF_SW01_P01` | Executioner Discipline Armament | Weapon |
| `REF_SW01_P02` | Executioner Discipline Grasp | Hands |
| `REF_SW01_P03` | Executioner Discipline Signet | Ring |

| Equipped | Proposed bonus |
|---:|---|
| 2 | Direct damage to elites and bosses +12%. |
| 3 | Take 10% less damage from elites and bosses; gain another 13% direct damage against them. |

**Related skills:** No specific skill requirement.

**Implementation needs:** Requires elite/boss classification and conditional damage and mitigation.

### Ironwall Oath · 3 pieces

`REF_SW02` · Sustained defense

**Source:** [Survival](https://tools.infinitybuilds.gg/en/database/talismans/phoba-of-cruel-fate) · All classes · 3 charms; thresholds 2/3.

Raises armor, all resistances, and all attributes.

**Gameplay:** Use Shield to survive dangerous windows and remain in melee.

**Adaptation:** Extends the original defensive attributes into Shield uptime.

| Piece ID | Proposed name | Slot |
|---|---|---|
| `REF_SW02_P01` | Ironwall Oath Crown | Head |
| `REF_SW02_P02` | Ironwall Oath Armor | Chest |
| `REF_SW02_P03` | Ironwall Oath Belt | Waist |

| Equipped | Proposed bonus |
|---:|---|
| 2 | Armor and all elemental resistances +15%. |
| 3 | Shield barrier generation +25%, duration +1 second, and maximum Life +10%. |

**Related skills:** `W05`

**Implementation needs:** Requires W05 barrier amount and duration hooks.

### Vanguard March · 4 pieces

`REF_SW03` · Mobility and farming

**Source:** [Practiced Technique](https://tools.infinitybuilds.gg/en/database/talismans/phoba-of-lethality) · All classes · 3 charms; thresholds 2/3.

Adds attack and movement speed, gold find, and kill experience.

**Gameplay:** Leap between groups and collect kill rewards quickly.

**Adaptation:** Keeps speed and farming as its identity with separately tuned gold and experience values.

| Piece ID | Proposed name | Slot |
|---|---|---|
| `REF_SW03_P01` | Vanguard March Grasp | Hands |
| `REF_SW03_P02` | Vanguard March Treads | Feet |
| `REF_SW03_P03` | Vanguard March Belt | Waist |
| `REF_SW03_P04` | Vanguard March Amulet | Amulet |

| Equipped | Proposed bonus |
|---:|---|
| 2 | Movement speed +8% and pickup radius +1 m. |
| 4 | Completing Leap grants 15% attack speed for 5 seconds. Monster gold +10% and hero kill XP +5%. |

**Related skills:** `W02`

**Implementation needs:** Requires gold and hero kill-XP bonuses and a post-Leap buff; excludes rune mastery XP.

### Trifold Armament · 4 pieces

`REF_SW04` · Elemental procs

**Source:** [Dark Pact](https://tools.infinitybuilds.gg/en/database/talismans/phoba-of-deceptive-insight) · All classes · 2 charms; thresholds 2.

Lucky hits can trigger cold, fire, and lightning damage.

**Gameplay:** Maintain physical direct attacks to trigger periodic elemental bursts.

**Adaptation:** Simplifies Lucky Hit into one roll per qualifying direct-hit cast.

| Piece ID | Proposed name | Slot |
|---|---|---|
| `REF_SW04_P01` | Trifold Armament Armament | Weapon |
| `REF_SW04_P02` | Trifold Armament Crown | Head |
| `REF_SW04_P03` | Trifold Armament Armor | Chest |
| `REF_SW04_P04` | Trifold Armament Signet | Ring |

| Equipped | Proposed bonus |
|---:|---|
| 2 | Physical direct damage +10%. |
| 4 | A direct-hit cast has a 20% chance to deal 60% D as one equally random fire, cold, or lightning hit. Internal cooldown: 1 second; Whirlwind rolls at most once per second. |

**Related skills:** `W01`, `W03`

**Implementation needs:** Requires secondary elemental damage with cast and channel proc limits.

### Crimson Scars · 5 pieces

`REF_SW05` · Bleed accumulation and cash-out

**Source:** [Bloodletter's Flow](https://tools.infinitybuilds.gg/en/database/talismans/phoba-of-the-bloodletter) · Barbarian · 5 charms; thresholds 2/3/5.

Extends bleeding, applies Vulnerable, and lets bleeding skills trigger Rupture.

**Gameplay:** Apply bleeding with Crush, then cash out its remaining damage with Slam.

**Adaptation:** Converts bleeding and Rupture into an ordered W03/W04 interaction.

| Piece ID | Proposed name | Slot |
|---|---|---|
| `REF_SW05_P01` | Crimson Scars Armament | Weapon |
| `REF_SW05_P02` | Crimson Scars Armor | Chest |
| `REF_SW05_P03` | Crimson Scars Grasp | Hands |
| `REF_SW05_P04` | Crimson Scars Belt | Waist |
| `REF_SW05_P05` | Crimson Scars Signet | Ring |

| Equipped | Proposed bonus |
|---:|---|
| 2 | Crush applies 80% D bleeding over 4 seconds. This set has one bleed per target; stronger applications replace it. |
| 3 | Direct damage to enemies bleeding from this set +15%. |
| 5 | Slam consumes this set's remaining bleed to deal 150% of it immediately, at most once per target every 3 seconds. |

**Related skills:** `W03`, `W04`

**Implementation needs:** Requires a new physical bleed ledger and remaining-damage consumption.

### Arsenal Manual · 5 pieces

`REF_SW06` · Alternating skill combos

**Source:** [Arms of Arreat](https://tools.infinitybuilds.gg/en/database/talismans/phoba-of-arreat) · Barbarian · 5 charms; thresholds 2/3/5.

Weapon swaps drive damage, barriers, and triggered Weapon Mastery skills.

**Gameplay:** Alternate Whirlwind, Leap, and Crush to cycle offense and barriers.

**Adaptation:** Replaces real-time weapon swaps with direct hits from different skills; adds no automatic gear swapping.

| Piece ID | Proposed name | Slot |
|---|---|---|
| `REF_SW06_P01` | Arsenal Manual Armament | Weapon |
| `REF_SW06_P02` | Arsenal Manual Crown | Head |
| `REF_SW06_P03` | Arsenal Manual Grasp | Hands |
| `REF_SW06_P04` | Arsenal Manual Treads | Feet |
| `REF_SW06_P05` | Arsenal Manual Amulet | Amulet |

| Equipped | Proposed bonus |
|---:|---|
| 2 | A different W01/W02/W03 direct hit within 4 seconds gains 15% cast damage. Only the first valid tick of a Whirlwind channel counts as a combo step. |
| 3 | A valid combo hit grants an 8% maximum-Life barrier for 3 seconds, at most once every 2 seconds; it does not stack. |
| 5 | Hit with all three skills within 4 seconds to create one 120% D physical shockwave within 2 m of the final target, then reset the combo. Internal cooldown: 4 seconds. |

**Related skills:** `W01`, `W02`, `W03`

**Implementation needs:** Requires previous-skill tracking, combo steps, and cast deduplication.

### Ancestral Vanguard · 5 pieces

`REF_SW07` · Shout and ancestral support

**Source:** [Bul-Kathos' Pride](https://tools.infinitybuilds.gg/en/database/talismans/phoba-of-bul-kathos-pride) · Barbarian · 5 charms; thresholds 2/3/5.

Reduces Ancient skill costs and cooldowns and summons an additional Ancient.

**Gameplay:** Prepare ancestral support with Shout, then enhance a core strike.

**Adaptation:** Adapts persistent summons into one-use ancestral echoes after Battle Shout.

| Piece ID | Proposed name | Slot |
|---|---|---|
| `REF_SW07_P01` | Ancestral Vanguard Crown | Head |
| `REF_SW07_P02` | Ancestral Vanguard Armor | Chest |
| `REF_SW07_P03` | Ancestral Vanguard Treads | Feet |
| `REF_SW07_P04` | Ancestral Vanguard Belt | Waist |
| `REF_SW07_P05` | Ancestral Vanguard Amulet | Amulet |

| Equipped | Proposed bonus |
|---:|---|
| 2 | Battle Shout cooldown reduction +15 percentage points. |
| 3 | After Battle Shout, take 10% less damage for 6 seconds. |
| 5 | The next Leap or Crush hit within 6 seconds after Shout creates one ancestral echo after 0.25 seconds, dealing 120% D physical damage in the same area. One echo per Shout. |

**Related skills:** `W06`, `W02`, `W03`

**Implementation needs:** Requires Shout charges and an echo that reuses the originating attack shape.

### Colossal Fury · 6 pieces

`REF_SW08` · High-resource heavy attacks

**Source:** [Sescheron's Fury](https://tools.infinitybuilds.gg/en/database/talismans/phoba-of-sescheron-s-fury) · Barbarian · 5 charms; thresholds 2/3/5.

High Fury trades cast speed for a larger character, larger skills, and more damage.

**Gameplay:** Preserve a high resource reserve to deliver slower, wider heavy strikes.

**Adaptation:** Uses a resource ratio instead of the original fixed Fury threshold and snapshots the state per cast.

| Piece ID | Proposed name | Slot |
|---|---|---|
| `REF_SW08_P01` | Colossal Fury Armament | Weapon |
| `REF_SW08_P02` | Colossal Fury Crown | Head |
| `REF_SW08_P03` | Colossal Fury Armor | Chest |
| `REF_SW08_P04` | Colossal Fury Grasp | Hands |
| `REF_SW08_P05` | Colossal Fury Belt | Waist |
| `REF_SW08_P06` | Colossal Fury Signet | Ring |

| Equipped | Proposed bonus |
|---:|---|
| 2 | Maximum resource +20. |
| 4 | If resource is at least 80% before a W03/W04 cast, that cast gains 45% damage and 20% area size, but preparation and recovery take 20% longer. |
| 6 | Under the Colossal condition, replace W03/W04 bonuses with +100% damage and +35% area size. Battle Shout restores 20 additional resource. |

**Related skills:** `W03`, `W04`, `W06`

**Implementation needs:** Requires a pre-cost resource snapshot and preparation/recovery duration modifiers.

## Mage

### Warding Aegis · 3 pieces

`REF_SM01` · Barrier survival

**Source:** [Survival](https://tools.infinitybuilds.gg/en/database/talismans/phoba-of-cruel-fate) · All classes · 3 charms; thresholds 2/3.

Raises armor, all resistances, and all attributes.

**Gameplay:** Strengthen Elemental Shield to survive close pressure.

**Adaptation:** Focuses shared defense on the Mage barrier.

| Piece ID | Proposed name | Slot |
|---|---|---|
| `REF_SM01_P01` | Warding Aegis Crown | Head |
| `REF_SM01_P02` | Warding Aegis Armor | Chest |
| `REF_SM01_P03` | Warding Aegis Belt | Waist |

| Equipped | Proposed bonus |
|---:|---|
| 2 | All elemental resistances +15%. |
| 3 | Elemental Shield generation +30% and maximum Life +10%. |

**Related skills:** `M05`

**Implementation needs:** Requires an M05 barrier-generation hook.

### Deep Scholarship · 3 pieces

`REF_SM02` · Skill ranks

**Source:** [Mastery](https://tools.infinitybuilds.gg/en/database/talismans/phoba-of-bone-breaking) · All classes · 2 charms; thresholds 2.

Raises the ranks of all skills.

**Gameplay:** Improve learned offensive skills without conditional procs.

**Adaptation:** Restricts universal skill ranks to learned offensive skills.

| Piece ID | Proposed name | Slot |
|---|---|---|
| `REF_SM02_P01` | Deep Scholarship Armament | Weapon |
| `REF_SM02_P02` | Deep Scholarship Grasp | Hands |
| `REF_SM02_P03` | Deep Scholarship Amulet | Amulet |

| Equipped | Proposed bonus |
|---:|---|
| 2 | Learned Fireball, Blizzard, Chain Lightning, and Frost Nova gain 1 rank. |
| 3 | Learned Fireball, Blizzard, and Chain Lightning gain 1 additional rank; unlearned skills remain locked. |

**Related skills:** `M01`, `M02`, `M03`, `M06`

**Implementation needs:** Must share the existing +5 bonus-rank cap.

### Rift Pilgrimage · 4 pieces

`REF_SM03` · Teleport and exploration

**Source:** [Practiced Technique](https://tools.infinitybuilds.gg/en/database/talismans/phoba-of-lethality) · All classes · 3 charms; thresholds 2/3.

Adds attack and movement speed, gold find, and kill experience.

**Gameplay:** Reduce movement and collection time in repeated exploration.

**Adaptation:** Reworks shared speed and farming around Teleport.

| Piece ID | Proposed name | Slot |
|---|---|---|
| `REF_SM03_P01` | Rift Pilgrimage Crown | Head |
| `REF_SM03_P02` | Rift Pilgrimage Treads | Feet |
| `REF_SM03_P03` | Rift Pilgrimage Belt | Waist |
| `REF_SM03_P04` | Rift Pilgrimage Signet | Ring |

| Equipped | Proposed bonus |
|---:|---|
| 2 | Movement speed +8% and pickup radius +1 m. |
| 4 | Teleport cooldown reduction +15 percentage points, monster gold +10%, and hero kill XP +5%. |

**Related skills:** `M04`

**Implementation needs:** Requires M04 cooldown, gold, and hero XP hooks.

### Capricious Pact · 4 pieces

`REF_SM04` · Lucky elemental procs

**Source:** [Dark Pact](https://tools.infinitybuilds.gg/en/database/talismans/phoba-of-deceptive-insight) · All classes · 2 charms; thresholds 2.

Lucky hits can trigger cold, fire, and lightning damage.

**Gameplay:** Repeated single-element attacks can trigger another element.

**Adaptation:** Uses one roll per cast that lands a direct hit.

| Piece ID | Proposed name | Slot |
|---|---|---|
| `REF_SM04_P01` | Capricious Pact Armament | Weapon |
| `REF_SM04_P02` | Capricious Pact Armor | Chest |
| `REF_SM04_P03` | Capricious Pact Grasp | Hands |
| `REF_SM04_P04` | Capricious Pact Amulet | Amulet |

| Equipped | Proposed bonus |
|---:|---|
| 2 | Direct magic damage +10%. |
| 4 | Each direct-hit magic cast has a 25% chance to deal 70% D of an equally random fire, cold, or lightning element to its first target. Internal cooldown: 1.5 seconds; excludes Blizzard ticks. |

**Related skills:** `M01`, `M03`, `M06`

**Implementation needs:** Requires direct/DOT distinction and per-cast proc limits.

### Overheat Crucible · 5 pieces

`REF_SM05` · Sustained fire casting

**Source:** [Habacalva's Cauldron](https://tools.infinitybuilds.gg/en/database/talismans/phoba-of-the-cauldron) · Sorcerer · 5 charms; thresholds 2/3/5.

Pyromancy builds Heat; Overheat improves cost efficiency and area damage.

**Gameplay:** Chain Fireballs to maintain Heat and trigger Overheat bursts.

**Adaptation:** Replaces free casts with a discount under the existing cost-reduction cap.

| Piece ID | Proposed name | Slot |
|---|---|---|
| `REF_SM05_P01` | Overheat Crucible Armament | Weapon |
| `REF_SM05_P02` | Overheat Crucible Crown | Head |
| `REF_SM05_P03` | Overheat Crucible Armor | Chest |
| `REF_SM05_P04` | Overheat Crucible Grasp | Hands |
| `REF_SM05_P05` | Overheat Crucible Belt | Waist |

| Equipped | Proposed bonus |
|---:|---|
| 2 | Each valid Fireball grants one Heat stack: +3% Fireball damage per stack, up to five. After 3 seconds without new Heat, lose one stack per second. |
| 3 | Take 2% less damage per Heat stack, up to 10%. |
| 5 | At five Heat before casting, Fireball gains 20 percentage points of cost reduction and an 80% D fire burst within 2 m of impact. One burst per cast with a 2-second cooldown. |

**Related skills:** `M01`

**Implementation needs:** Requires Heat stacks, decay, and secondary Overheat bursts.

### Frozen Deep · 5 pieces

`REF_SM06` · Frostbite and freeze cash-out

**Source:** [Breath of the Frozen Sea](https://tools.infinitybuilds.gg/en/database/talismans/phoba-of-the-frozen-sea) · Sorcerer · 5 charms; thresholds 2/3/5.

Direct frost damage adds Frostbite, which is cashed out by a freeze attempt.

**Gameplay:** Build Frostbite with Blizzard, then cash it out with Frost Nova.

**Adaptation:** Uses M02/M06; bosses cash out on a hit without actual freezing.

| Piece ID | Proposed name | Slot |
|---|---|---|
| `REF_SM06_P01` | Frozen Deep Crown | Head |
| `REF_SM06_P02` | Frozen Deep Armor | Chest |
| `REF_SM06_P03` | Frozen Deep Treads | Feet |
| `REF_SM06_P04` | Frozen Deep Amulet | Amulet |
| `REF_SM06_P05` | Frozen Deep Signet | Ring |

| Equipped | Proposed bonus |
|---:|---|
| 2 | Each damaging Blizzard tick adds 15% D Frostbite, draining over 6 seconds; remaining damage is capped at 90% D. |
| 3 | Take 12% less damage inside your own Blizzard. |
| 5 | Frost Nova consumes this set's remaining Frostbite for 150% of its damage immediately, once per target every 4 seconds. It does not force bosses to freeze. |

**Related skills:** `M02`, `M06`

**Implementation needs:** Requires per-target Frostbite storage and control-immune cash-out handling.

### Wild Thunder · 5 pieces

`REF_SM07` · Random lightning spikes

**Source:** [Cain's Wild Lightning](https://tools.infinitybuilds.gg/en/database/talismans/phoba-of-wild-lightning) · Sorcerer · 5 charms; thresholds 2/3/5.

Shock damage rolls a random bonus, with extra lightning on a maximum result.

**Gameplay:** Roll each Chain Lightning bonus and seek a maximum-result bolt.

**Adaptation:** Uses four smaller outcomes shared by the entire chain.

| Piece ID | Proposed name | Slot |
|---|---|---|
| `REF_SM07_P01` | Wild Thunder Armament | Weapon |
| `REF_SM07_P02` | Wild Thunder Grasp | Hands |
| `REF_SM07_P03` | Wild Thunder Treads | Feet |
| `REF_SM07_P04` | Wild Thunder Belt | Waist |
| `REF_SM07_P05` | Wild Thunder Signet | Ring |

| Equipped | Proposed bonus |
|---:|---|
| 2 | Each Chain Lightning rolls an equally likely 0%, 10%, 20%, or 30% damage bonus shared by every jump. |
| 3 | A 30% result attempts a 0.5-second stun on the first target, once per cast; boss immunity remains intact. |
| 5 | Roll twice and use the higher bonus. A final 30% result adds one 80% D lightning hit to the first target. |

**Related skills:** `M03`

**Implementation needs:** Requires reproducible per-cast rolls and first-target secondary bolts.

### Threefold Cycle · 6 pieces

`REF_SM08` · Three-element rotation

**Source:** [Tal Rasha's Threefold Way](https://tools.infinitybuilds.gg/en/database/talismans/phoba-of-the-threefold) · Sorcerer · 5 charms; thresholds 2/3/5.

Different elements trigger Mastery skills and strengthen a different element.

**Gameplay:** Rotate Fireball, Blizzard, and Chain Lightning, then release a combined burst.

**Adaptation:** Replaces Meteor and Ball Lightning triggers with existing skills and elemental bursts.

| Piece ID | Proposed name | Slot |
|---|---|---|
| `REF_SM08_P01` | Threefold Cycle Armament | Weapon |
| `REF_SM08_P02` | Threefold Cycle Crown | Head |
| `REF_SM08_P03` | Threefold Cycle Armor | Chest |
| `REF_SM08_P04` | Threefold Cycle Grasp | Hands |
| `REF_SM08_P05` | Threefold Cycle Amulet | Amulet |
| `REF_SM08_P06` | Threefold Cycle Signet | Ring |

| Equipped | Proposed bonus |
|---:|---|
| 2 | Casting a different M01/M02/M03 within 8 seconds grants that skill 12% damage. |
| 4 | A new element grants a 5% maximum-Life barrier for 4 seconds. Different elements in the same rotation add up to 15%; repeated elements add nothing. |
| 6 | All three elements within 8 seconds reset the history and grant one 6-second charge. The next offensive skill's first hit releases one 2.5 m burst for 40% D each of fire, cold, and lightning. Charge creation cooldown: 6 seconds. |

**Related skills:** `M01`, `M02`, `M03`

**Implementation needs:** Requires element history, charges, and next-cast reservation.

## Ranger

### Wandering Hunter · 3 pieces

`REF_SA01` · Movement and collection

**Source:** [Practiced Technique](https://tools.infinitybuilds.gg/en/database/talismans/phoba-of-lethality) · All classes · 3 charms; thresholds 2/3.

Adds attack and movement speed, gold find, and kill experience.

**Gameplay:** Improve spacing and loot collection speed.

**Adaptation:** Adapts shared speed and farming into Ranger mobility support.

| Piece ID | Proposed name | Slot |
|---|---|---|
| `REF_SA01_P01` | Wandering Hunter Treads | Feet |
| `REF_SA01_P02` | Wandering Hunter Belt | Waist |
| `REF_SA01_P03` | Wandering Hunter Amulet | Amulet |

| Equipped | Proposed bonus |
|---:|---|
| 2 | Movement speed +10%. |
| 3 | Attack speed +5%, monster gold +10%, and hero kill XP +5%. |

**Related skills:** No specific skill requirement.

**Implementation needs:** Requires movement and kill-reward hooks; excludes rune mastery XP.

### Survivor Mantle · 3 pieces

`REF_SA02` · Hit recovery

**Source:** [Survival](https://tools.infinitybuilds.gg/en/database/talismans/phoba-of-cruel-fate) · All classes · 3 charms; thresholds 2/3.

Raises armor, all resistances, and all attributes.

**Gameplay:** Survive pursuit with a defensive window after Retreat.

**Adaptation:** Extends shared defensive attributes into a post-Retreat window.

| Piece ID | Proposed name | Slot |
|---|---|---|
| `REF_SA02_P01` | Survivor Mantle Crown | Head |
| `REF_SA02_P02` | Survivor Mantle Armor | Chest |
| `REF_SA02_P03` | Survivor Mantle Signet | Ring |

| Equipped | Proposed bonus |
|---:|---|
| 2 | Armor and all elemental resistances +12%. |
| 3 | Completing Retreat grants 15% damage reduction for 4 seconds. |

**Related skills:** `A04`

**Implementation needs:** Requires Retreat-completion and mitigation hooks.

### Marksman Manual · 4 pieces

`REF_SA03` · Shooting mastery

**Source:** [Mastery](https://tools.infinitybuilds.gg/en/database/talismans/phoba-of-bone-breaking) · All classes · 2 charms; thresholds 2.

Raises the ranks of all skills.

**Gameplay:** Improve Pierce/Multi ranks and resource efficiency against marked targets.

**Adaptation:** Restricts universal ranks to existing Ranger shooting skills.

| Piece ID | Proposed name | Slot |
|---|---|---|
| `REF_SA03_P01` | Marksman Manual Armament | Weapon |
| `REF_SA03_P02` | Marksman Manual Crown | Head |
| `REF_SA03_P03` | Marksman Manual Grasp | Hands |
| `REF_SA03_P04` | Marksman Manual Amulet | Amulet |

| Equipped | Proposed bonus |
|---:|---|
| 2 | Learned Pierce and Multi gain 1 rank. |
| 4 | A01/A02 direct hits on a marked target restore 4 resource per cast and deal 15% more direct damage to marked targets. |

**Related skills:** `A01`, `A02`, `A05`

**Implementation needs:** Requires shared rank caps and per-cast marked-target refunds.

### Colossus Hunt · 4 pieces

`REF_SA04` · Elite and boss focus

**Source:** [Slaughter](https://tools.infinitybuilds.gg/en/database/talismans/phoba-of-slaughter) · All classes · 3 charms; thresholds 2/3.

Combines offense and defense with an additional advantage against elites.

**Gameplay:** Focus one strong target for an extra hit on every third qualifying cast.

**Adaptation:** Extends elite specialization into repeated hits on the same target.

| Piece ID | Proposed name | Slot |
|---|---|---|
| `REF_SA04_P01` | Colossus Hunt Armor | Chest |
| `REF_SA04_P02` | Colossus Hunt Treads | Feet |
| `REF_SA04_P03` | Colossus Hunt Belt | Waist |
| `REF_SA04_P04` | Colossus Hunt Signet | Ring |

| Equipped | Proposed bonus |
|---:|---|
| 2 | Direct shooting damage to elites and bosses +15%. |
| 4 | Land three A01/A02 casts on the same marked elite or boss within 6 seconds to add 100% D physical damage on the third cast, then reset. Count once per cast. |

**Related skills:** `A01`, `A02`, `A05`

**Implementation needs:** Requires same-target cast counting and reset on target changes.

### Vengeful Sight · 5 pieces

`REF_SA05` · Basic-shot barrage

**Source:** [Nilfur's Narrow Eye](https://tools.infinitybuilds.gg/en/database/talismans/phoba-of-the-narrow-eye) · Rogue · 5 charms; thresholds 2/3/5.

Marksman basic skills build Vengeance and Marksman casts trigger basic attacks.

**Gameplay:** Build Vengeance with basic shots, then attach extra arrows to core shots.

**Adaptation:** Maps Marksman basics to the existing basic shot.

| Piece ID | Proposed name | Slot |
|---|---|---|
| `REF_SA05_P01` | Vengeful Sight Armament | Weapon |
| `REF_SA05_P02` | Vengeful Sight Crown | Head |
| `REF_SA05_P03` | Vengeful Sight Grasp | Hands |
| `REF_SA05_P04` | Vengeful Sight Treads | Feet |
| `REF_SA05_P05` | Vengeful Sight Signet | Ring |

| Equipped | Proposed bonus |
|---:|---|
| 2 | Basic-shot direct hits grant one Vengeance stack for 6 seconds, up to five. Each adds 3% A01/A02 direct damage; new stacks refresh the full duration. |
| 3 | Movement speed +2% per Vengeance stack, up to 10%. |
| 5 | At five Vengeance, an A01/A02 cast adds three 25% D physical arrows at 0.1-second intervals to its first target. Once per cast with a 2-second cooldown; Vengeance is retained. |

**Related skills:** `A01`, `A02`

**Implementation needs:** Requires basic-shot stacks and non-recursive secondary arrows.

### Alchemical Hunting Ground · 5 pieces

`REF_SA06` · Traps and crowd control

**Source:** [Applied Alchemy](https://tools.infinitybuilds.gg/en/database/talismans/phoba-of-applied-alchemy) · Rogue · 5 charms; thresholds 2/3/5.

Trap and grenade casts reduce cooldowns and trigger stun grenades.

**Gameplay:** Control enemies with traps, then retreat into a new firing position.

**Adaptation:** Converts trap/grenade combinations into a one-time stun burst from Poison Trap.

| Piece ID | Proposed name | Slot |
|---|---|---|
| `REF_SA06_P01` | Alchemical Hunting Ground Crown | Head |
| `REF_SA06_P02` | Alchemical Hunting Ground Armor | Chest |
| `REF_SA06_P03` | Alchemical Hunting Ground Grasp | Hands |
| `REF_SA06_P04` | Alchemical Hunting Ground Belt | Waist |
| `REF_SA06_P05` | Alchemical Hunting Ground Amulet | Amulet |

| Equipped | Proposed bonus |
|---:|---|
| 2 | Poison Trap cooldown reduction +15 percentage points. |
| 3 | Take 5% less damage per active Poison Trap, up to 10%. |
| 5 | The first damaging tick of each Poison Trap triggers one 80% D physical burst and a 0.75-second stun attempt within 2 m. Targets hit take 20% more poison damage from that trap. Bosses resist the stun only. |

**Related skills:** `A03`, `A04`

**Implementation needs:** Requires first-damage tracking per trap, the two-trap limit, and boss immunity handling.

### Threefold Venom · 5 pieces

`REF_SA07` · Imbued shooting

**Source:** [Spellbound Steel](https://tools.infinitybuilds.gg/en/database/talismans/phoba-of-spellbound-steel) · Rogue · 5 charms; thresholds 2/3/5.

Links Imbuements with Inner Sight and applies multiple Imbuements together.

**Gameplay:** Use Shadow Arrow charges to add cold and poison to shooting.

**Adaptation:** Restricts combined Imbuements to A06's existing three charges; does not add free potions.

| Piece ID | Proposed name | Slot |
|---|---|---|
| `REF_SA07_P01` | Threefold Venom Armament | Weapon |
| `REF_SA07_P02` | Threefold Venom Armor | Chest |
| `REF_SA07_P03` | Threefold Venom Treads | Feet |
| `REF_SA07_P04` | Threefold Venom Belt | Waist |
| `REF_SA07_P05` | Threefold Venom Signet | Ring |

| Equipped | Proposed bonus |
|---:|---|
| 2 | Shadow Arrow secondary shadow damage +20%. |
| 3 | A hit from an A06-charged cast restores 2% maximum Life, once per cast with a 1-second cooldown. |
| 5 | Each target hit by an A06-charged shot takes 30% D cold damage and 45% D poison over 3 seconds, plus a 20% slow for 2 seconds. Once per target per cast; stronger set poison replaces weaker poison. |

**Related skills:** `A06`, `A01`, `A02`

**Implementation needs:** Requires A06 cast-charge tracking and separate set poison/slow families.

### Sightless Shadow · 6 pieces

`REF_SA08` · Retreat and shadow echoes

**Source:** [Legacy of the Sightless](https://tools.infinitybuilds.gg/en/database/talismans/phoba-of-the-sightless) · Rogue · 5 charms; thresholds 2/3/5.

Cycles Stealth and Ultimate skills and supplies a permanent Shadow Clone.

**Gameplay:** Attack after Retreat so a shadow repeats your core shots.

**Adaptation:** Uses temporary post-Retreat shot echoes instead of new Stealth, Ultimate, or permanent-pet systems.

| Piece ID | Proposed name | Slot |
|---|---|---|
| `REF_SA08_P01` | Sightless Shadow Armament | Weapon |
| `REF_SA08_P02` | Sightless Shadow Crown | Head |
| `REF_SA08_P03` | Sightless Shadow Armor | Chest |
| `REF_SA08_P04` | Sightless Shadow Grasp | Hands |
| `REF_SA08_P05` | Sightless Shadow Treads | Feet |
| `REF_SA08_P06` | Sightless Shadow Amulet | Amulet |

| Equipped | Proposed bonus |
|---:|---|
| 2 | Retreat cooldown reduction +15 percentage points. |
| 4 | Completing Retreat grants 20% A01/A02 direct damage for 6 seconds. |
| 6 | For 6 seconds after Retreat, A01/A02 casts create one echo after 0.25 seconds with the same origin, direction, and shape. It deals 70% D physical damage per target to at most five targets. Once per cast with a 2-second cooldown. |

**Related skills:** `A04`, `A01`, `A02`

**Implementation needs:** Requires a post-Retreat buff and delayed copies of shot origin, direction, and shape.

## Data maintenance and future validation

The JSON separates source facts from HELLSCRIPT proposals: `sources` holds original names, charm counts, and summaries; `sets` holds proposed sets, pieces, and numeric parameters. Keep `runtimeEnabled=false` and `proposedDropWeight=null`. This catalog does not increase the runtime set DB count.

```sh
python3 tools/class_set_reference.py build
python3 tools/class_set_reference.py check
```

Current checks cover requested counts, ID uniqueness, class skill ownership, slot conflicts, thresholds, provenance, and bilingual document parity. They do not prove combat balance or save restoration. Before runtime adoption, validate equal-investment boss/elite/group performance, equip thresholds, duplicate-hit/recursive-proc prevention, unequip/interruption/scene-end/save behavior, and slot competition with existing legendaries.
