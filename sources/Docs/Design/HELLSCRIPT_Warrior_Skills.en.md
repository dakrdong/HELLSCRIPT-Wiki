# HELLSCRIPT Warrior — 37 skills

Updated: 2026-09-23

Status: approved design. Ability and equipment implementation status is tracked in [the implementation record](../Implementation/Class_Skill_Runtime.en.md). General release and UI integration are pending. Values below are for rank one.

[Rules and sources](HELLSCRIPT_Class_Skills.en.md) · [Equipment matrix](HELLSCRIPT_Skill_Equipment.en.md) · [한국어](HELLSCRIPT_Warrior_Skills.md)

16 normal actives, 19 passives and 2 ultimates. Proposed loadout: up to 4 normal actives, up to 3 passives and exactly 1 ultimate after unlock. BASIC is separate. Entries 36 and 37 share the final tier and are mutually exclusive.

## Equipment-led build examples

Each example wears the full named set. Listed legendaries occupy different slots; use rares elsewhere. Configure the sequence in Hunt Edict, using the per-skill automatic-use suggestions below.

### Executioner Discipline · `REF_SW01`

Normal actives: Brand of Challenge (`W07`), Impaling Hook (`W08`), Tremor Wave (`W14`), Guardian's Vow (`W16`).

Passives: Brand Pursuit (`WP07`), Weapon Sequence (`WP15`), Hardened Will (`WP05`). Selected ultimate: Titan's Judgment (`W18`).

Compatible legendaries: Pursuer's Chain (`DES_LW41`), Ancestral King's Crown (`DES_LW46`).

Brand, pull, then hit with the wave; Guardian's Vow provides a short defensive window.

### Ironwall Oath · `REF_SW02`

Normal actives: Iron Wall (`W05`), Resolute Advance (`W11`), Iron Stance (`W12`), Crushing Blow (`W03`).

Passives: Hardened Will (`WP05`), Counter Rhythm (`WP09`), Stone Landing (`WP11`). Selected ultimate: Titan's Judgment (`W18`).

Compatible legendaries: Solitary Execution (`LW03`), Deep-Carved Scar (`LW19`).

Advance behind Iron Wall, gain resource from successful blocks and spend it on Crush.

### Vanguard March · `REF_SW03`

Normal actives: Leap Slam (`W02`), Whirlwind (`W01`), Battlefield Ring (`W13`), Breath Before Battle (`W15`).

Passives: Endless Spin (`WP02`), Landing Stance (`WP03`), Ragged Breathing (`WP13`). Selected ultimate: War of the Ancestors (`W17`).

Compatible legendaries: Ancestral King's Crown (`DES_LW46`), Fang of the Maelstrom (`LW01`).

Group enemies, leap and channel Whirlwind; Breath Before Battle refuels the next pack.

### Trifold Armament · `REF_SW04`

Normal actives: Whirlwind (`W01`), Crushing Blow (`W03`), Brand of Challenge (`W07`), Iron Wall (`W05`).

Passives: Into the Crowd (`WP01`), Brand Pursuit (`WP07`), Hold Formation (`WP16`). Selected ultimate: War of the Ancestors (`W17`).

Compatible legendaries: Solitary Execution (`LW03`), Bloodied Writ (`LW20`).

Prepare Brand and Iron Wall, then use original Whirlwind and Crush hits to enable elemental set procs.

### Crimson Scars · `REF_SW05`

Normal actives: Crushing Blow (`W03`), Ground Slam (`W04`), Raking Wound (`W09`), Blood Reclamation (`W10`).

Passives: Wound Tracking (`WP08`), Battlefield Pressure (`WP14`), Executioner's Eye (`WP06`). Selected ultimate: Titan's Judgment (`W18`).

Compatible legendaries: Ancestral King's Crown (`DES_LW46`), Ankle-Binding Rift (`LW27`).

Slam cashes out Crush's set bleed; Blood Reclamation cashes out Raking Wound. Keep the two ledgers separate.

### Arsenal Manual · `REF_SW06`

Normal actives: Whirlwind (`W01`), Leap Slam (`W02`), Crushing Blow (`W03`), Iron Wall (`W05`).

Passives: Weapon Sequence (`WP15`), Combat Preparation (`WP12`), Blood Recovery (`WP04`). Selected ultimate: Titan's Judgment (`W18`).

Compatible legendaries: Red Rampart (`LW08`), Parched Fury (`LW10`).

Connect the three different original skills within 4s. Use BASIC for resource preparation, outside the skill sequence.

### Ancestral Vanguard · `REF_SW07`

Normal actives: Battle Shout (`W06`), Leap Slam (`W02`), Crushing Blow (`W03`), Guardian's Vow (`W16`).

Passives: Lingering Shout (`WP10`), Stone Landing (`WP11`), Ancestral Legacy (`WP18`). Selected ultimate: War of the Ancestors (`W17`).

Compatible legendaries: Solitary Execution (`LW03`), Returning Leap (`LW12`).

After Shout, the first Leap or Crush hit releases the set echo; the ultimate ancestors remain a separate source.

### Colossal Fury · `REF_SW08`

Normal actives: Crushing Blow (`W03`), Ground Slam (`W04`), Battle Shout (`W06`), Breath Before Battle (`W15`).

Passives: Battle Reserve (`WP17`), Lingering Shout (`WP10`), Titan's Bulwark (`WP19`). Selected ultimate: Titan's Judgment (`W18`).

Compatible legendaries: Ankle-Binding Rift (`LW27`), Horn of Resentment (`LW34`).

Use Breath and Shout to reach 80% pre-cost resource, then release slow heavy attacks and time the ultimate for elites.

### Whirlwind Watcher · `SW`

Normal actives: Whirlwind (`W01`), Leap Slam (`W02`), Iron Wall (`W05`), Battle Shout (`W06`).

Passives: Endless Spin (`WP02`), Landing Stance (`WP03`), Hold Formation (`WP16`). Selected ultimate: War of the Ancestors (`W17`).

Keep the original two/four-piece skill triggers. The new ultimate uses a separate slot and does not impersonate a set-bound skill.

### Falling Star Executor · `SWB`

Normal actives: Leap Slam (`W02`), Crushing Blow (`W03`), Iron Wall (`W05`), Brand of Challenge (`W07`).

Passives: Landing Stance (`WP03`), Brand Pursuit (`WP07`), Stone Landing (`WP11`). Selected ultimate: Titan's Judgment (`W18`).

Keep the original two/four-piece skill triggers. The new ultimate uses a separate slot and does not impersonate a set-bound skill.

## Normal actives · 01–16

### 01. Whirlwind · `W01`

Existing runtime retained · Lv.1 · Cooldown 0s · Resource 6 per paid 0.25s tick

Damages nearby enemies every 0.25 seconds. Can be held while moving.

**Suggested automatic use:** Keep the existing Hunt Edict conditions, placement and target settings.

**Equipment links:** Fang of the Maelstrom (`LW01`), Turning Rack (`LW05`), Rising Ire (`LW06`), Flesh-Cutting Wind (`LW07`), Red Rampart (`LW08`) and 3 more.

**Mechanic reference:** Existing HELLSCRIPT definition.

### 02. Leap Slam · `W02`

Existing runtime retained · Lv.3 · Cooldown 8s · Resource 0

Leap to a valid landing and deal impact damage. In the new-ability validation path, each actual meter beyond 2m adds 15% direct damage, capped at +90% at 8m. The spacing policy may retreat for up to 2s first.

**Suggested automatic use:** Keep the existing Hunt Edict conditions, placement and target settings.

**Equipment links:** Stride of the Falling Star (`LW02`), Final Order (`LW04`), Scorched Landing (`LW11`), Returning Leap (`LW12`), Execution Foretold (`LW13`) and 4 more.

**Mechanic reference:** Existing HELLSCRIPT definition.

### 03. Crushing Blow · `W03`

Existing runtime retained · Lv.6 · Cooldown 0s · Resource 25

Deals heavy physical damage to enemies in front.

**Suggested automatic use:** Keep the existing Hunt Edict conditions, placement and target settings.

**Equipment links:** Solitary Execution (`LW03`), Execution Foretold (`LW13`), Crushed Crown (`LW17`), Skull Echo (`LW18`), Deep-Carved Scar (`LW19`) and 5 more.

**Mechanic reference:** Existing HELLSCRIPT definition.

### 04. Ground Slam · `W04`

Existing runtime retained · Lv.10 · Cooldown 10s · Resource 0

Damages nearby enemies and stuns them for 1.5 seconds.

**Suggested automatic use:** Keep the existing Hunt Edict conditions, placement and target settings.

**Equipment links:** Twice-Ringing Earth (`LW23`), Binder's Cord (`LW24`), Stormgate (`LW25`), Bastion of Remains (`LW26`), Ankle-Binding Rift (`LW27`) and 1 more.

**Mechanic reference:** Existing HELLSCRIPT definition.

### 05. Iron Wall · `W05`

Existing runtime retained · Lv.15 · Cooldown 14s · Resource 0

Holds a shield worth 30% of max HP for 4 seconds.

**Suggested automatic use:** Keep the existing Hunt Edict conditions, placement and target settings.

**Equipment links:** Ironwall Shards (`LW29`), Mending Ironcore (`LW30`), Unbending Vow (`LW32`), Unblinking Watch (`LW31`).

**Mechanic reference:** Existing HELLSCRIPT definition.

### 06. Battle Shout · `W06`

Existing runtime retained · Lv.20 · Cooldown 16s · Resource 0

Restores 40 resource and adds +20% damage for 6 seconds.

**Suggested automatic use:** Keep the existing Hunt Edict conditions, placement and target settings.

**Equipment links:** Battlefield Recall (`LW21`), Horn of Resentment (`LW34`), Comrade's Last Wish (`LW35`), Bloodbeat (`LW36`), Edict of Assault (`LW37`) and 2 more.

**Mechanic reference:** Existing HELLSCRIPT definition.

### 07. Brand of Challenge · `W07`

New proposal · Lv.22 · Cooldown 10s · Resource 0

Brand one enemy within 8m for 6s and restore 10 resource. Take 10% less damage from that enemy. The brand supplies Marked without a separate damage bonus. Only one Warrior brand may exist.

**Suggested automatic use:** Prefer elites or bosses; cast when the brand has at most 1s left.

**Equipment links:** Pursuer's Chain (`DES_LW41`), Brandbreaker (`LW22`).

**Companion skills:** Crushing Blow (`W03`), Ground Slam (`W04`).

**Mechanic reference:** D2 Taunt / Battle Cry.

### 08. Impaling Hook · `W08`

New proposal · Lv.22 · Cooldown 7s · Resource 0

Deal D65% physical damage to one enemy within 6m and pull it up to 2m. Restore 10 resource on a direct hit. Bosses receive stagger credit instead of movement.

**Suggested automatic use:** Pull a target that is outside the main melee skill's reach.

**Equipment links:** Pursuer's Chain (`DES_LW41`).

**Companion skills:** Whirlwind (`W01`), Crushing Blow (`W03`).

**Mechanic reference:** D3 Ancient Spear.

### 09. Raking Wound · `W09`

New proposal · Lv.26 · Cooldown 4s · Resource 15

Hit up to 5 enemies in a 3m, 90-degree cone for D50% physical damage and D120% bleed over 4s. Refresh only the W09 ledger, keeping the greater remaining total.

**Suggested automatic use:** Prefer an unbled elite or a cluster of at least 3 enemies.

**Equipment links:** Wound Reaper's Grasp (`DES_LW42`).

**Companion skills:** Crushing Blow (`W03`), Ground Slam (`W04`).

**Mechanic reference:** D3 Rend; D4 Rend variants.

### 10. Blood Reclamation · `W10`

New proposal · Lv.26 · Cooldown 8s · Resource 0

Deal D80% physical damage to up to 5 enemies within 4m and cash out 100% of only your W09 remaining bleed. Heal 1% maximum HP per cashed-out enemy, up to 5% per cast. Never consume set or item ledgers.

**Suggested automatic use:** Use with at least 2s of W09 bleed remaining, or below 60% HP with a bleeding target.

**Equipment links:** Wound Reaper's Grasp (`DES_LW42`).

**Companion skills:** Raking Wound (`W09`), Ground Slam (`W04`).

**Mechanic reference:** D3 Bloodthirst / Rend.

### 11. Resolute Advance · `W11`

New proposal · Lv.30 · Cooldown 9s · Resource 0

Advance up to 4m to a valid point, hitting up to 5 enemies along the path for D90% physical damage. Gain 15% damage reduction for 3s after arrival. Restore 15 resource once if a target is hit.

**Suggested automatic use:** Approach an enemy only when the destination is no more dangerous than the current position.

**Equipment links:** Moving Rampart (`DES_LW43`).

**Companion skills:** Leap Slam (`W02`), Iron Wall (`W05`).

**Mechanic reference:** D3 Furious Charge.

### 12. Iron Stance · `W12`

New proposal · Lv.30 · Cooldown 12s · Resource 10

Gain +15 percentage points of block chance for 4s. A successful block produces a D80% counter in a 3m front arc, at most twice per stance with a 1s interval. Counters are secondary damage.

**Suggested automatic use:** Use against a nearby winding-up attacker or below 60% HP.

**Equipment links:** Moving Rampart (`DES_LW43`).

**Companion skills:** Iron Wall (`W05`).

**Mechanic reference:** D3 Revenge / Sword and Board.

### 13. Battlefield Ring · `W13`

New proposal · Lv.34 · Cooldown 10s · Resource 15

Deal D70% physical damage to up to 5 enemies within 4m and pull them up to 1.5m without crossing walls or bodies. Bosses receive stagger credit without moving.

**Suggested automatic use:** Use when at least 3 enemies can be grouped for Whirlwind or Slam.

**Equipment links:** Mountain Tremor (`DES_LW44`).

**Companion skills:** Whirlwind (`W01`), Ground Slam (`W04`).

**Mechanic reference:** D3 Ground Stomp.

### 14. Tremor Wave · `W14`

New proposal · Lv.34 · Cooldown 6s · Resource 20

Send a 7m-long, 1m-wide wave through up to 5 enemies for D150% physical damage and a 30% slow for 3s. Attack reach does not grant movement.

**Suggested automatic use:** Use against at least 2 aligned enemies or a difficult-to-reach elite.

**Equipment links:** Mountain Tremor (`DES_LW44`), Crushed Crown (`LW17`), Binder's Cord (`LW24`).

**Companion skills:** Crushing Blow (`W03`), Ground Slam (`W04`).

**Mechanic reference:** D3 Seismic Slam.

### 15. Breath Before Battle · `W15`

New proposal · Lv.38 · Cooldown 18s · Resource 0

Restore 8 resource each second for 4s, up to the resource cap. Attacks and movement remain available. This is not a Shout.

**Suggested automatic use:** Use at 60% resource or less while preparing the next attack.

**Equipment links:** Unspent Resolve (`DES_LW45`), Rising Ire (`LW06`).

**Companion skills:** Crushing Blow (`W03`), Battle Shout (`W06`).

**Mechanic reference:** D2 Battle Orders; D3 Animosity.

### 16. Guardian's Vow · `W16`

New proposal · Lv.38 · Cooldown 16s · Resource 0

Gain a 12% maximum-HP barrier for 5s and remove current slow and root effects. Become immune to slow and root for 2s. This does not cast Iron Wall.

**Suggested automatic use:** Use when slowed or rooted, or below 55% HP without a barrier.

**Equipment links:** Unspent Resolve (`DES_LW45`), Unblinking Watch (`LW31`), Unbending Vow (`LW32`).

**Companion skills:** Iron Wall (`W05`), Battle Shout (`W06`).

**Mechanic reference:** D3 Ignore Pain.

## Passives · 17–35

### 17. Into the Crowd · `WP01`

Existing runtime retained

Adds +15% damage while three or more living enemies stand within 3m.

**Equipment links:** Fang of the Maelstrom (`LW01`), Turning Rack (`LW05`), Rising Ire (`LW06`), Flesh-Cutting Wind (`LW07`), Red Rampart (`LW08`) and 7 more.

**Mechanic reference:** Existing HELLSCRIPT definition.

### 18. Endless Spin · `WP02`

Existing runtime retained

Holding Whirlwind for 2 seconds applies a 20% cost reduction. Interrupting it clears the bonus.

**Equipment links:** Fang of the Maelstrom (`LW01`), Turning Rack (`LW05`), Rising Ire (`LW06`), Flesh-Cutting Wind (`LW07`), Red Rampart (`LW08`) and 3 more.

**Mechanic reference:** Existing HELLSCRIPT definition.

### 19. Landing Stance · `WP03`

Existing runtime retained

Reduces incoming damage by 15% for 2 seconds after a leap lands.

**Equipment links:** Stride of the Falling Star (`LW02`), Final Order (`LW04`), Scorched Landing (`LW11`), Returning Leap (`LW12`), Execution Foretold (`LW13`) and 4 more.

**Mechanic reference:** Existing HELLSCRIPT definition.

### 20. Blood Recovery · `WP04`

Existing runtime retained

Restores 2% of max HP on a kill, then waits 1 second before it can trigger again.

**Equipment links:** Fang of the Maelstrom (`LW01`), Solitary Execution (`LW03`), Turning Rack (`LW05`), Rising Ire (`LW06`), Flesh-Cutting Wind (`LW07`) and 13 more.

**Mechanic reference:** Existing HELLSCRIPT definition.

### 21. Hardened Will · `WP05`

Existing runtime retained

Adds +20%p to shield strength, stacking with the bonus from Willpower.

**Equipment links:** Ironwall Shards (`LW29`), Mending Ironcore (`LW30`), Unbending Vow (`LW32`).

**Mechanic reference:** Existing HELLSCRIPT definition.

### 22. Executioner's Eye · `WP06`

Existing runtime retained

Adds +20% damage against enemies at or below 30% HP.

**Equipment links:** Solitary Execution (`LW03`), Execution Foretold (`LW13`), Crushed Crown (`LW17`), Skull Echo (`LW18`), Deep-Carved Scar (`LW19`) and 10 more.

**Mechanic reference:** Existing HELLSCRIPT definition.

### 23. Brand Pursuit · `WP07`

New proposal · Lv.22

The first direct normal-active hit against a marked target restores 2 resource. Once per cast, with a 1s interval.

**Equipment links:** Solitary Execution (`LW03`), Execution Foretold (`LW13`), Crushed Crown (`LW17`), Skull Echo (`LW18`), Deep-Carved Scar (`LW19`) and 6 more.

**Companion skills:** Brand of Challenge (`W07`), Crushing Blow (`W03`).

**Mechanic reference:** D3 conditional passive structure; HELLSCRIPT equipment requirements.

### 24. Wound Tracking · `WP08`

New proposal · Lv.22

Gain +10% additive normal-active direct damage against enemies with your W09 or Red Scars set bleed. Other physical damage over time is not automatically classified as bleed.

**Equipment links:** Solitary Execution (`LW03`), Execution Foretold (`LW13`), Crushed Crown (`LW17`), Skull Echo (`LW18`), Deep-Carved Scar (`LW19`) and 11 more.

**Companion skills:** Raking Wound (`W09`), Crushing Blow (`W03`), Ground Slam (`W04`).

**Mechanic reference:** D3 conditional passive structure; HELLSCRIPT equipment requirements.

### 25. Counter Rhythm · `WP09`

New proposal · Lv.22

Restore 4 resource after surviving a successful block. A 1s interval applies; counter damage cannot trigger it.

**Equipment links:** Ironwall Shards (`LW29`), Mending Ironcore (`LW30`), Unbending Vow (`LW32`), Moving Rampart (`DES_LW43`).

**Companion skills:** Iron Stance (`W12`), Iron Wall (`W05`).

**Mechanic reference:** D3 conditional passive structure; HELLSCRIPT equipment requirements.

### 26. Lingering Shout · `WP10`

New proposal · Lv.26

After casting Battle Shout, heal 1% maximum HP each second for 3s. Refresh without stacking; 6s interval.

**Equipment links:** Battlefield Recall (`LW21`), Horn of Resentment (`LW34`), Comrade's Last Wish (`LW35`), Bloodbeat (`LW36`), Edict of Assault (`LW37`).

**Companion skills:** Battle Shout (`W06`).

**Mechanic reference:** D3 conditional passive structure; HELLSCRIPT equipment requirements.

### 27. Stone Landing · `WP11`

New proposal · Lv.26

After completing Leap or Resolute Advance, gain a 5% maximum-HP barrier for 3s. Both skills share a 3s interval.

**Equipment links:** Stride of the Falling Star (`LW02`), Final Order (`LW04`), Scorched Landing (`LW11`), Returning Leap (`LW12`), Execution Foretold (`LW13`) and 5 more.

**Companion skills:** Leap Slam (`W02`), Resolute Advance (`W11`).

**Mechanic reference:** D3 conditional passive structure; HELLSCRIPT equipment requirements.

### 28. Combat Preparation · `WP12`

New proposal · Lv.26

Three basic hits on one enemy prepare +15 percentage points of cost reduction for the next Crush within 4s. One charge; counting pauses for 3s after triggering.

**Equipment links:** Solitary Execution (`LW03`), Execution Foretold (`LW13`), Crushed Crown (`LW17`), Skull Echo (`LW18`), Deep-Carved Scar (`LW19`) and 5 more.

**Companion skills:** Crushing Blow (`W03`).

**Mechanic reference:** D3 conditional passive structure; HELLSCRIPT equipment requirements.

### 29. Ragged Breathing · `WP13`

New proposal · Lv.30

Below 50% HP, increase per-second resource regeneration by 20%. Do not multiply instant on-hit or shout restoration.

**Equipment links:** Fang of the Maelstrom (`LW01`), Turning Rack (`LW05`), Rising Ire (`LW06`), Flesh-Cutting Wind (`LW07`), Red Rampart (`LW08`) and 4 more.

**Companion skills:** Whirlwind (`W01`), Breath Before Battle (`W15`).

**Mechanic reference:** D3 conditional passive structure; HELLSCRIPT equipment requirements.

### 30. Battlefield Pressure · `WP14`

New proposal · Lv.30

The first direct normal-active hit on a controlled enemy reduces equipped Slam's remaining cooldown by 0.5s. A 2s interval applies; never reduce below zero.

**Equipment links:** Twice-Ringing Earth (`LW23`), Binder's Cord (`LW24`), Stormgate (`LW25`), Bastion of Remains (`LW26`), Ankle-Binding Rift (`LW27`) and 1 more.

**Companion skills:** Ground Slam (`W04`), Battlefield Ring (`W13`), Tremor Wave (`W14`).

**Mechanic reference:** D3 conditional passive structure; HELLSCRIPT equipment requirements.

### 31. Weapon Sequence · `WP15`

New proposal · Lv.30

Casting a different normal offensive skill from the one used within the previous 4s grants that cast +10% additive direct damage. BASIC, ultimates and secondary damage do not update the record.

**Equipment links:** Fang of the Maelstrom (`LW01`), Stride of the Falling Star (`LW02`), Solitary Execution (`LW03`), Final Order (`LW04`), Turning Rack (`LW05`) and 23 more.

**Companion skills:** Whirlwind (`W01`), Leap Slam (`W02`), Crushing Blow (`W03`), Impaling Hook (`W08`), Tremor Wave (`W14`).

**Mechanic reference:** D3 conditional passive structure; HELLSCRIPT equipment requirements.

### 32. Hold Formation · `WP16`

New proposal · Lv.34

Take 8% less damage while channeling Whirlwind with an active barrier. End immediately when channeling or the barrier ends.

**Equipment links:** Fang of the Maelstrom (`LW01`), Turning Rack (`LW05`), Rising Ire (`LW06`), Flesh-Cutting Wind (`LW07`), Red Rampart (`LW08`) and 7 more.

**Companion skills:** Whirlwind (`W01`), Iron Wall (`W05`), Guardian's Vow (`W16`).

**Mechanic reference:** D3 conditional passive structure; HELLSCRIPT equipment requirements.

### 33. Battle Reserve · `WP17`

New proposal · Lv.34

Crush and Slam gain +10% additive direct damage when pre-cast resource is at least 80%. Check before paying the cost.

**Equipment links:** Solitary Execution (`LW03`), Execution Foretold (`LW13`), Crushed Crown (`LW17`), Skull Echo (`LW18`), Deep-Carved Scar (`LW19`) and 11 more.

**Companion skills:** Crushing Blow (`W03`), Ground Slam (`W04`), Breath Before Battle (`W15`).

**Mechanic reference:** D3 conditional passive structure; HELLSCRIPT equipment requirements.

### 34. Ancestral Legacy · `WP18`

New proposal · Lv.34

During War of the Ancestors, an actual ancestor hit restores 1 resource per second, up to 8 per cast.

**Equipment links:** Ancestral King's Crown (`DES_LW46`).

**Companion skills:** War of the Ancestors (`W17`).

**Mechanic reference:** D3 conditional passive structure; HELLSCRIPT equipment requirements.

### 35. Titan's Bulwark · `WP19`

New proposal · Lv.40

Each enemy directly hit by Titan's Judgment grants a 4s barrier of 3% maximum HP, capped at 15%.

**Equipment links:** Ancestral King's Crown (`DES_LW46`).

**Companion skills:** Titan's Judgment (`W18`).

**Mechanic reference:** D3 conditional passive structure; HELLSCRIPT equipment requirements.

## Final tier: ultimates · 36–37 · choose one

### 36. War of the Ancestors · `W17`

New proposal · Lv.40 · Cooldown 60s · Resource 0

Call 2 ancestors for 8s. Each attacks one enemy within 6m each second for D65% secondary physical damage, up to 8 attacks each. Take 15% less damage while active. Ancestors have no blocking body or taunt.

**Suggested automatic use:** Use in elite or boss combat when at least 3 enemies are near or HP is at most 65%.

**Equipment links:** Ancestral King's Crown (`DES_LW46`).

**Companion skills:** Whirlwind (`W01`), Battle Shout (`W06`).

**Mechanic reference:** D3 Call of the Ancients.

### 37. Titan's Judgment · `W18`

New proposal · Lv.40 · Cooldown 60s · Resource 0

After a 0.9s wind-up, deal D600% physical damage and a 2s stun to up to 8 enemies within 4m. For 6s, Crush and Slam gain +25% additive direct damage. Boss stun becomes stagger credit.

**Suggested automatic use:** Use on an elite, boss or 4-enemy cluster while Battle Shout is active or resource is at least 80%.

**Equipment links:** Ancestral King's Crown (`DES_LW46`).

**Companion skills:** Crushing Blow (`W03`), Ground Slam (`W04`), Battle Shout (`W06`).

**Mechanic reference:** D3 Earthquake / Wrath of the Berserker.
