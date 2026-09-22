# HELLSCRIPT Mage — 36 skills

Updated: 2026-09-22

Status: approved design. Ability and equipment implementation status is tracked in [the implementation record](../Implementation/Class_Skill_Runtime.en.md). General release and UI integration are pending. Values below are for rank one.

[Rules and sources](HELLSCRIPT_Class_Skills.en.md) · [Equipment matrix](HELLSCRIPT_Skill_Equipment.en.md) · [한국어](HELLSCRIPT_Mage_Skills.md)

16 normal actives, 18 passives and 2 ultimates. Proposed loadout: up to 4 normal actives, up to 3 passives and exactly 1 ultimate after unlock. BASIC is separate. Entries 35 and 36 share the final tier and are mutually exclusive.

## Equipment-led build examples

Each example wears the full named set. Listed legendaries occupy different slots; use rares elsewhere. Configure the sequence in Hunt Edict, using the per-skill automatic-use suggestions below.

### Warding Aegis · `REF_SM01`

Normal actives: Elemental Shield (`M05`), Mana Reclaim (`M13`), Rift Ward (`M14`), Ember Lance (`M07`).

Passives: Stable Ward (`MP04`), Ward Breathing (`MP13`), Steady Casting (`MP15`). Selected ultimate: Triune Collapse (`M17`).

Compatible legendaries: Threefold Crownstone (`DES_LM46`), Oath of Restraint (`LC02`).

Apply Elemental Barrier, recover resource inside the ward, clear ranged targets with Ember Lance and use the ultimate on clusters.

### Deep Scholarship · `REF_SM02`

Normal actives: Fireball (`M01`), Blizzard (`M02`), Chain Lightning (`M03`), Frost Nova (`M06`).

Passives: Mana Circulation (`MP05`), Element crossover (`MP06`), Threefold Memory (`MP16`). Selected ultimate: Sage Incarnate (`M18`).

Compatible legendaries: Threefold Crownstone (`DES_LM46`), Flameroot (`LM08`).

Equip all four rank-boosted skills. Cold control and the ultimate barrier cover the absence of a separate mobility skill.

### Rift Pilgrimage · `REF_SM03`

Normal actives: Teleport (`M04`), Frost Globe (`M10`), Magnetic Vortex (`M16`), Fireball (`M01`).

Passives: Dense Burn (`MP01`), Mana Thrift (`MP14`), Overflowing Mana (`MP17`). Selected ultimate: Triune Collapse (`M17`).

Compatible legendaries: Echoing Ember (`LM02`), Lingering Ember (`LM05`).

Teleport for a firing line, group targets with Vortex and hit with Globe/Fireball. Farming bonuses retain the set's kill-reward rules.

### Capricious Pact · `REF_SM04`

Normal actives: Fireball (`M01`), Chain Lightning (`M03`), Frost Nova (`M06`), Elemental Compass (`M15`).

Passives: Element crossover (`MP06`), Opening in the Cold (`MP09`), Threefold Memory (`MP16`). Selected ultimate: Sage Incarnate (`M18`).

Compatible legendaries: Threefold Crownstone (`DES_LM46`), Flameroot (`LM08`).

Prepare three elemental discounts with Compass, then trigger the set using original Fireball, Chain Lightning and Frost Nova.

### Overheat Crucible · `REF_SM05`

Normal actives: Fireball (`M01`), Ember Lance (`M07`), Firewall (`M08`), Mana Reclaim (`M13`).

Passives: Dense Burn (`MP01`), Ignition Feedback (`MP07`), Scorched Armor (`MP08`). Selected ultimate: Triune Collapse (`M17`).

Compatible legendaries: Threefold Crownstone (`DES_LM46`), Unblemished Furnace (`LM06`).

Maintain Burning with Firewall/Ember Lance and build Heat with repeated Fireball. New fire skills do not generate set Heat.

### Frozen Deep · `REF_SM06`

Normal actives: Blizzard (`M02`), Frost Nova (`M06`), Glacial Lance (`M09`), Frost Globe (`M10`).

Passives: Deep Chill (`MP02`), Opening in the Cold (`MP09`), Ice Reverberation (`MP10`). Selected ultimate: Triune Collapse (`M17`).

Compatible legendaries: Glassglacier Staff (`DES_LM42`), Freezing Sigil (`LM15`).

Frost Nova cashes out set Frostbite built by Blizzard. Lance and Globe supply control/direct damage without consuming that ledger.

### Wild Thunder · `REF_SM07`

Normal actives: Chain Lightning (`M03`), Capacitor Orb (`M11`), Storm Spear (`M12`), Elemental Shield (`M05`).

Passives: Chain of Lightning (`MP03`), Overcharge (`MP11`), Reclaim Charge (`MP12`). Selected ultimate: Triune Collapse (`M17`).

Compatible legendaries: Thunder Collector (`DES_LM43`), Clinging Lightning (`LM21`).

Roll the set with Chain Lightning and charge Capacitor Orb. Storm Spear provides linear damage and its dedicated orb-item interaction.

### Threefold Cycle · `REF_SM08`

Normal actives: Fireball (`M01`), Blizzard (`M02`), Chain Lightning (`M03`), Elemental Compass (`M15`).

Passives: Element crossover (`MP06`), Threefold Memory (`MP16`), Archmage's Testament (`MP18`). Selected ultimate: Sage Incarnate (`M18`).

Compatible legendaries: Unquenched Heart (`LC03`).

After Compass, complete the set cycle with the three original skills. Sage Incarnate tracks a separate cycle and cannot fill the set's elements by itself.

### Winter Covenant · `SM`

Normal actives: Blizzard (`M02`), Fireball (`M01`), Teleport (`M04`), Elemental Shield (`M05`).

Passives: Dense Burn (`MP01`), Deep Chill (`MP02`), Stable Ward (`MP04`). Selected ultimate: Triune Collapse (`M17`).

Keep the original two/four-piece skill triggers. The new ultimate uses a separate slot and does not impersonate a set-bound skill.

### Resonating Storm · `SMB`

Normal actives: Frost Nova (`M06`), Chain Lightning (`M03`), Elemental Shield (`M05`), Mana Reclaim (`M13`).

Passives: Chain of Lightning (`MP03`), Mana Circulation (`MP05`), Ward Breathing (`MP13`). Selected ultimate: Triune Collapse (`M17`).

Keep the original two/four-piece skill triggers. The new ultimate uses a separate slot and does not impersonate a set-bound skill.

## Normal actives · 01–16

### 01. Fireball · `M01`

Existing runtime retained · Lv.1 · Cooldown 0s · Resource 25

Deals blast damage at the point of impact.

**Suggested automatic use:** Keep the existing Hunt Edict conditions, placement and target settings.

**Equipment links:** Echoing Ember (`LM02`), Lingering Ember (`LM05`), Unblemished Furnace (`LM06`), Ember Succession (`LM07`), Flameroot (`LM08`) and 4 more.

**Mechanic reference:** Existing HELLSCRIPT definition.

### 02. Blizzard · `M02`

Existing runtime retained · Lv.3 · Cooldown 6s · Resource 30

Deals cold damage and slows by 35% for 6 seconds. Up to 2 at once.

**Suggested automatic use:** Keep the existing Hunt Edict conditions, placement and target settings.

**Equipment links:** Winter's Trail (`LM01`), Splintered Snowflake (`LM11`), Judgment of Distant Winter (`LM12`), Frost Husk (`LM13`), Cold Circulation (`LM14`) and 5 more.

**Mechanic reference:** Existing HELLSCRIPT definition.

### 03. Chain Lightning · `M03`

Existing runtime retained · Lv.6 · Cooldown 2s · Resource 25

Links lightning across 4 enemies, each hit dealing ×0.8 of the previous one.

**Suggested automatic use:** Keep the existing Hunt Edict conditions, placement and target settings.

**Equipment links:** Circuit of the End (`LM04`), Overflowing Charge (`LM17`), Close-Quarters Judgment (`LM18`), Storm Continuum (`LM19`), Reclaimed Current (`LM20`) and 3 more.

**Mechanic reference:** Existing HELLSCRIPT definition.

### 04. Teleport · `M04`

Existing runtime retained · Lv.10 · Cooldown 9s · Resource 0

Moves to a valid spot with less danger.

**Suggested automatic use:** Keep the existing Hunt Edict conditions, placement and target settings.

**Equipment links:** Rift-Opening Spark (`LM10`), Spatial Discharge (`LM23`), Thunderbolt Arrival (`LM24`), Weightless Boundary (`LM25`), Leaping Flame (`LM26`) and 3 more.

**Mechanic reference:** Existing HELLSCRIPT definition.

### 05. Elemental Shield · `M05`

Existing runtime retained · Lv.15 · Cooldown 14s · Resource 0

Holds a shield worth 35% of max HP for 4 seconds.

**Suggested automatic use:** Keep the existing Hunt Edict conditions, placement and target settings.

**Equipment links:** Knot of Feedback (`LM03`), Mending Crystal Veil (`LM30`), Charged Shell (`LM31`), Chill-Wrapped Knot (`LM33`), Unbroken Escape Route (`LM32`).

**Mechanic reference:** Existing HELLSCRIPT definition.

### 06. Frost Nova · `M06`

Existing runtime retained · Lv.20 · Cooldown 10s · Resource 20

Deals cold damage to nearby enemies and freezes them for 1.5 seconds.

**Suggested automatic use:** Keep the existing Hunt Edict conditions, placement and target settings.

**Equipment links:** Wintercalling Thunder (`LM22`), Second Winter (`LM34`), Bound Mana (`LM35`), Deepening Blizzard (`LM36`), Icebound Memory (`LM16`) and 1 more.

**Mechanic reference:** Existing HELLSCRIPT definition.

### 07. Ember Lance · `M07`

New proposal · Lv.22 · Cooldown 3s · Resource 18

Fire a 10m-long, 0.6m-wide lance through up to 3 enemies for D140% direct fire damage plus D60% burn over 3s. Refresh only this skill's stronger remaining burn total.

**Suggested automatic use:** Use against aligned enemies or an elite that is not burning.

**Equipment links:** Kindling Grasp (`DES_LM41`).

**Companion skills:** Fireball (`M01`).

**Mechanic reference:** D2 Fireball; D3 Magic Missile.

### 08. Firewall · `M08`

New proposal · Lv.22 · Cooldown 8s · Resource 25

Create a 5m-long, 1m-wide fire line within 8m for 4s. Deal D60% fire damage each second to up to 5 enemies and mark occupants as Burning. It does not block movement. Limit 1.

**Suggested automatic use:** Place across a Blizzard cluster or an enemy approach path.

**Equipment links:** Kindling Grasp (`DES_LM41`).

**Companion skills:** Fireball (`M01`), Blizzard (`M02`).

**Mechanic reference:** D2 Firewall.

### 09. Glacial Lance · `M09`

New proposal · Lv.26 · Cooldown 4s · Resource 18

Hit up to 3 enemies with a 9m-long, 0.7m-wide lance for D130% cold damage. Freeze already-slowed enemies for 0.8s; otherwise slow them by 35% for 3s.

**Suggested automatic use:** Use against Blizzard-slowed targets or approaching enemies.

**Equipment links:** Glassglacier Staff (`DES_LM42`), Cold Circulation (`LM14`), Icebound Memory (`LM16`), Bound Mana (`LM35`), Icebreaking Oath (`LM37`).

**Companion skills:** Blizzard (`M02`), Frost Nova (`M06`).

**Mechanic reference:** D2 Glacial Spike.

### 10. Frost Globe · `M10`

New proposal · Lv.26 · Cooldown 6s · Resource 25

Send a globe up to 8m at 4m/s. At its destination or first wall, explode for D180% cold damage and a 35% slow for 3s to up to 5 enemies within 3m. Travel itself deals no damage.

**Suggested automatic use:** Aim at the observed center of an approaching group.

**Equipment links:** Glassglacier Staff (`DES_LM42`), Cold Circulation (`LM14`).

**Companion skills:** Blizzard (`M02`), Frost Nova (`M06`).

**Mechanic reference:** D2 Frozen Orb.

### 11. Capacitor Orb · `M11`

New proposal · Lv.30 · Cooldown 10s · Resource 25

Place one orb within 8m for 6s. Each second, deal D30% lightning damage to up to 3 enemies within 3m. Each valid M03 cast adds one charge, up to 3. Natural expiry causes a secondary lightning blast for D80% plus D30% per charge.

**Suggested automatic use:** Place for sustained elite combat with enough resource to cast Chain Lightning.

**Equipment links:** Thunder Collector (`DES_LM43`).

**Companion skills:** Chain Lightning (`M03`).

**Mechanic reference:** D2 Thunder Storm; D3 Storm Armor.

### 12. Storm Spear · `M12`

New proposal · Lv.30 · Cooldown 5s · Resource 22

Pierce up to 5 enemies with a 12m-long, 0.8m-wide spear for D170% lightning damage. Restore 4 resource once per cast if a direct target was hit by your M03 within 3s.

**Suggested automatic use:** Use against a recently chained elite or aligned enemies.

**Equipment links:** Thunder Collector (`DES_LM43`).

**Companion skills:** Chain Lightning (`M03`).

**Mechanic reference:** D2 Lightning; D3 Electrocute.

### 13. Mana Reclaim · `M13`

New proposal · Lv.34 · Cooldown 0s · Resource 0

Channel mana while standing still. Rank 1 regenerates at ten times the ordinary rate; each rank increases this multiplier by 10%. No cooldown or maximum duration. Moving or starting another valid attack cancels it; taking damage alone does not. Automatic charging begins at 35% mana or less and stops at the selected 50% or 90% goal. Grants no invulnerability.

**Suggested automatic use:** Charge at 35% mana or less. The default requires a safe position; choose to stay through hits or spend up to 2s creating distance. Stop at the selected 50% or 90% goal.

**Equipment links:** Quiet Rift (`DES_LM44`).

**Companion skills:** Fireball (`M01`), Blizzard (`M02`), Chain Lightning (`M03`).

**Mechanic reference:** D2 Warmth; D3 Prodigy.

### 14. Rift Ward · `M14`

New proposal · Lv.34 · Cooldown 16s · Resource 10

Create a 3m ward at the current position for 6s and a 10% maximum-HP barrier for 4s. Take 10% less damage while inside. Limit 1; it does not count as casting M05.

**Suggested automatic use:** Use at a hazard-free casting position when no barrier is active.

**Equipment links:** Quiet Rift (`DES_LM44`), Unbroken Escape Route (`LM32`).

**Companion skills:** Teleport (`M04`), Elemental Shield (`M05`).

**Mechanic reference:** D2 Energy Shield; D3 Slow Time.

### 15. Elemental Compass · `M15`

New proposal · Lv.38 · Cooldown 18s · Resource 0

Restore 20 resource. Within 8s, grant +15 percentage points of cost reduction to the next normal fire, cold and lightning active, once per element and up to 3 casts. Repeating an element does not consume another element's charge.

**Suggested automatic use:** Use at 70% resource or less with at least 2 paid elemental skills of different elements equipped.

**Equipment links:** Compass Center (`DES_LM45`).

**Companion skills:** Fireball (`M01`), Blizzard (`M02`), Chain Lightning (`M03`).

**Mechanic reference:** D3 Elemental Exposure; D4 skill variants.

### 16. Magnetic Vortex · `M16`

New proposal · Lv.38 · Cooldown 12s · Resource 20

Create a 3m vortex within 8m for 4s. On creation, pull up to 5 enemies 1.5m toward the center; deal D30% lightning damage each second. Limit 1. Bosses receive stagger credit without moving.

**Suggested automatic use:** Use when at least 3 enemies can be grouped inside Blizzard or Firewall.

**Equipment links:** Compass Center (`DES_LM45`).

**Companion skills:** Fireball (`M01`), Blizzard (`M02`), Frost Nova (`M06`).

**Mechanic reference:** D3 Black Hole / Energy Twister.

## Passives · 17–34

### 17. Dense Burn · `MP01`

Existing runtime retained

Adds +15% damage to the Fireball blast itself when it catches three or more enemies.

**Equipment links:** Echoing Ember (`LM02`), Lingering Ember (`LM05`), Unblemished Furnace (`LM06`), Ember Succession (`LM07`), Flameroot (`LM08`) and 4 more.

**Mechanic reference:** Existing HELLSCRIPT definition.

### 18. Deep Chill · `MP02`

Existing runtime retained

Strengthens the Blizzard slow from 35% to 55%. Only the strongest slow applies.

**Equipment links:** Winter's Trail (`LM01`), Splintered Snowflake (`LM11`), Judgment of Distant Winter (`LM12`), Frost Husk (`LM13`), Cold Circulation (`LM14`) and 3 more.

**Mechanic reference:** Existing HELLSCRIPT definition.

### 19. Chain of Lightning · `MP03`

Existing runtime retained

Chain Lightning strikes 5 times instead of 4.

**Equipment links:** Circuit of the End (`LM04`), Overflowing Charge (`LM17`), Close-Quarters Judgment (`LM18`), Storm Continuum (`LM19`), Reclaimed Current (`LM20`) and 3 more.

**Mechanic reference:** Existing HELLSCRIPT definition.

### 20. Stable Ward · `MP04`

Existing runtime retained

Adds +20%p to shield strength, stacking with the bonus from Willpower.

**Equipment links:** Knot of Feedback (`LM03`), Mending Crystal Veil (`LM30`), Charged Shell (`LM31`), Chill-Wrapped Knot (`LM33`).

**Mechanic reference:** Existing HELLSCRIPT definition.

### 21. Mana Circulation · `MP05`

Existing runtime retained

Increases resource regeneration per second by 20%, including flat regeneration affixes.

**Equipment links:** Winter's Trail (`LM01`), Echoing Ember (`LM02`), Circuit of the End (`LM04`), Lingering Ember (`LM05`), Unblemished Furnace (`LM06`) and 20 more.

**Mechanic reference:** Existing HELLSCRIPT definition.

### 22. Element crossover · `MP06`

Existing runtime retained

Dealing damage with two different elements within 4 seconds grants +10% damage for 4 seconds. It does not stack.

**Equipment links:** Winter's Trail (`LM01`), Echoing Ember (`LM02`), Circuit of the End (`LM04`), Lingering Ember (`LM05`), Unblemished Furnace (`LM06`) and 20 more.

**Mechanic reference:** Existing HELLSCRIPT definition.

### 23. Ignition Feedback · `MP07`

New proposal · Lv.22

The first direct hit of a normal fire skill on a burning enemy restores 3 resource, with a 2s interval. Burn ticks cannot retrigger it.

**Equipment links:** Echoing Ember (`LM02`), Lingering Ember (`LM05`), Unblemished Furnace (`LM06`), Ember Succession (`LM07`), Flameroot (`LM08`) and 5 more.

**Companion skills:** Fireball (`M01`), Ember Lance (`M07`), Firewall (`M08`).

**Mechanic reference:** D3 conditional passive structure; HELLSCRIPT equipment requirements.

### 24. Scorched Armor · `MP08`

New proposal · Lv.22

Take 8% less damage from an attacker with your M07 burn or currently inside your M08.

**Equipment links:** Knot of Feedback (`LM03`), Mending Crystal Veil (`LM30`), Charged Shell (`LM31`), Chill-Wrapped Knot (`LM33`), Kindling Grasp (`DES_LM41`).

**Companion skills:** Ember Lance (`M07`), Firewall (`M08`), Elemental Shield (`M05`).

**Mechanic reference:** D3 conditional passive structure; HELLSCRIPT equipment requirements.

### 25. Opening in the Cold · `MP09`

New proposal · Lv.22

The first direct hit of a normal cold skill on a controlled target restores 3 resource, with a 2s interval. Blizzard ticks are excluded.

**Equipment links:** Winter's Trail (`LM01`), Splintered Snowflake (`LM11`), Judgment of Distant Winter (`LM12`), Frost Husk (`LM13`), Cold Circulation (`LM14`) and 7 more.

**Companion skills:** Blizzard (`M02`), Frost Nova (`M06`), Glacial Lance (`M09`).

**Mechanic reference:** D3 conditional passive structure; HELLSCRIPT equipment requirements.

### 26. Ice Reverberation · `MP10`

New proposal · Lv.26

Successfully applying direct freeze with M06 or M09 grants a 5% maximum-HP barrier for 3s. Valid boss stagger credit also qualifies; 4s interval.

**Equipment links:** Knot of Feedback (`LM03`), Wintercalling Thunder (`LM22`), Mending Crystal Veil (`LM30`), Charged Shell (`LM31`), Chill-Wrapped Knot (`LM33`) and 4 more.

**Companion skills:** Frost Nova (`M06`), Glacial Lance (`M09`), Elemental Shield (`M05`).

**Mechanic reference:** D3 conditional passive structure; HELLSCRIPT equipment requirements.

### 27. Overcharge · `MP11`

New proposal · Lv.26

Three normal lightning casts within 5s prepare +15% direct damage for the next M03 or M12 within 4s. One charge, not applied retroactively to the completing third cast.

**Equipment links:** Circuit of the End (`LM04`), Overflowing Charge (`LM17`), Close-Quarters Judgment (`LM18`), Storm Continuum (`LM19`), Reclaimed Current (`LM20`) and 4 more.

**Companion skills:** Chain Lightning (`M03`), Capacitor Orb (`M11`), Storm Spear (`M12`).

**Mechanic reference:** D3 conditional passive structure; HELLSCRIPT equipment requirements.

### 28. Reclaim Charge · `MP12`

New proposal · Lv.26

Natural expiry of Capacitor Orb restores 10 resource. Replacement, scene exit or equipment-removal cleanup grants none.

**Equipment links:** Circuit of the End (`LM04`), Overflowing Charge (`LM17`), Close-Quarters Judgment (`LM18`), Storm Continuum (`LM19`), Reclaimed Current (`LM20`) and 4 more.

**Companion skills:** Capacitor Orb (`M11`), Chain Lightning (`M03`).

**Mechanic reference:** D3 conditional passive structure; HELLSCRIPT equipment requirements.

### 29. Ward Breathing · `MP13`

New proposal · Lv.30

While a barrier remains, increase per-second resource regeneration by 15%. Do not multiply Mana Reclaim's fixed restoration.

**Equipment links:** Knot of Feedback (`LM03`), Mending Crystal Veil (`LM30`), Charged Shell (`LM31`), Chill-Wrapped Knot (`LM33`), Quiet Rift (`DES_LM44`).

**Companion skills:** Elemental Shield (`M05`), Mana Reclaim (`M13`), Rift Ward (`M14`).

**Mechanic reference:** D3 conditional passive structure; HELLSCRIPT equipment requirements.

### 30. Mana Thrift · `MP14`

New proposal · Lv.30

Two direct basic hits prepare +15 percentage points of cost reduction for the next paid normal active within 4s. One charge; pause counting for 3s after triggering.

**Equipment links:** Winter's Trail (`LM01`), Echoing Ember (`LM02`), Circuit of the End (`LM04`), Lingering Ember (`LM05`), Unblemished Furnace (`LM06`) and 20 more.

**Companion skills:** Fireball (`M01`), Blizzard (`M02`), Chain Lightning (`M03`).

**Mechanic reference:** D3 conditional passive structure; HELLSCRIPT equipment requirements.

### 31. Steady Casting · `MP15`

New proposal · Lv.30

After standing still for 1s, take 8% less damage while preparing or channeling a normal active. End on movement or completion.

**Equipment links:** Echoing Ember (`LM02`), Lingering Ember (`LM05`), Unblemished Furnace (`LM06`), Ember Succession (`LM07`), Flameroot (`LM08`) and 6 more.

**Companion skills:** Fireball (`M01`), Firewall (`M08`), Mana Reclaim (`M13`).

**Mechanic reference:** D3 conditional passive structure; HELLSCRIPT equipment requirements.

### 32. Threefold Memory · `MP16`

New proposal · Lv.34

Cast one normal fire, cold and lightning active within 6s to gain an 8% maximum-HP barrier for 4s. Clear the record on completion or expiry; 8s interval.

**Equipment links:** Winter's Trail (`LM01`), Echoing Ember (`LM02`), Circuit of the End (`LM04`), Lingering Ember (`LM05`), Unblemished Furnace (`LM06`) and 21 more.

**Companion skills:** Fireball (`M01`), Blizzard (`M02`), Chain Lightning (`M03`), Elemental Compass (`M15`).

**Mechanic reference:** D3 conditional passive structure; HELLSCRIPT equipment requirements.

### 33. Overflowing Mana · `MP17`

New proposal · Lv.34

At 80% pre-cast resource or more, a normal active's first direct hit gains +10% additive damage. Exclude field ticks and secondary damage.

**Equipment links:** Echoing Ember (`LM02`), Circuit of the End (`LM04`), Lingering Ember (`LM05`), Unblemished Furnace (`LM06`), Ember Succession (`LM07`) and 13 more.

**Companion skills:** Fireball (`M01`), Chain Lightning (`M03`), Mana Reclaim (`M13`).

**Mechanic reference:** D3 conditional passive structure; HELLSCRIPT equipment requirements.

### 34. Archmage's Testament · `MP18`

New proposal · Lv.34

If Triune Collapse's third stage actually hits, restore 15 resource once. With Sage Incarnate selected, add 10% maximum HP to its barrier. Apply only the selected ultimate branch.

**Equipment links:** Threefold Crownstone (`DES_LM46`).

**Companion skills:** Triune Collapse (`M17`), Sage Incarnate (`M18`).

**Mechanic reference:** D3 conditional passive structure; HELLSCRIPT equipment requirements.

## Final tier: ultimates · 35–36 · choose one

### 35. Triune Collapse · `M17`

New proposal · Lv.40 · Cooldown 60s · Resource 0

Strike a 4m area within 10m for D200% fire, then D150% cold after 0.5s, then D150% lightning after another 0.5s. Each stage hits up to 8 enemies; cold freezes for 1s. These stages do not count as M01, M02 or M03 casts.

**Suggested automatic use:** If the loadout can generate an elemental-cycle buff, wait for it. Otherwise do not wait for an unavailable buff. In both cases, target an elite, boss, or at least four enemies.

**Equipment links:** Threefold Crownstone (`DES_LM46`).

**Companion skills:** Fireball (`M01`), Blizzard (`M02`), Chain Lightning (`M03`), Frost Nova (`M06`).

**Mechanic reference:** D3 Meteor / Frost Nova / Electrocute.

### 36. Sage Incarnate · `M18`

New proposal · Lv.40 · Cooldown 60s · Resource 0

Gain a 25% maximum-HP barrier for 8s, +20% normal-active damage and +20 percentage points of normal-active cost reduction. Completing a three-element normal-active cast cycle triggers D40% per element within 2.5m, up to 5 targets, at the last skill's first impact. Once per completed cycle, at most twice per ultimate.

**Suggested automatic use:** Use in elite or boss combat with three different elemental skills equipped.

**Equipment links:** Threefold Crownstone (`DES_LM46`).

**Companion skills:** Fireball (`M01`), Blizzard (`M02`), Chain Lightning (`M03`), Elemental Shield (`M05`).

**Mechanic reference:** D3 Archon / Elemental Exposure.
