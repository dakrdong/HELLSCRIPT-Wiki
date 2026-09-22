# Skill-use rules and experiments

[한국어](Class_Skill_Use_Policies.md)

The fun of combat configuration comes from choosing conditional behavior, observing its result, and revising it. Good choices should depend on enemies and equipment. These options change timing, targets, positions and waiting; they grant no free stats merely for being selected.

There is one use-policy group for each of the 54 active/ultimate skills, plus a separate Mana Reclaim recovery goal: 55 groups and 164 choices. The 54 passives compete for three passive slots rather than automatic-cast conditions. Existing detailed edict values are preserved; legacy choices below project their selected value onto the existing edict. Defaults keep the original conditions.

## Two reference cases

Leap Slam gains 15% additive direct damage per actual meter beyond 2m, capped at 90% at 8m. Walking backward itself deals no damage. Spacing spends up to 2s seeking 6m from the target through existing wall, body and observed-danger checks. A blocked or timed-out attempt tries a legal leap from the current distance, then yields to other actions if none is available. This extension applies only in the new-ability validation path.

Mana Reclaim regenerates at ten times the ordinary rate at rank 1, rising to fourteen times at rank 5. It has no cooldown or fixed duration. A movement request, actual movement, or another valid attack interrupts it; rejected attack requests and damage alone do not. Automatic charging starts at 35% mana or less and stops at the chosen 50% or 90%. Retreating spends up to 2s creating 5m of space, then charges; it interrupts if an enemy enters 3m or observed danger appears. Holding accepts ordinary hits while honoring the configured global emergency policy. Full mana and death never duplicate recovery.

DES_LM44 grants a 10% maximum-health shield for 4s after at least 1.5s of uninterrupted charging inside the caster's ward. Deep preparation waits for that shield even after reaching 90% mana when the equipment is ready; short recovery does not add this wait. A 6s equipment cooldown survives channel restarts, preventing repeated shield creation from a no-cooldown skill.

## Controlled comparisons

Change one choice while holding equipment, ranks, enemies, seed and duration fixed. Compare total damage, DPS, survival, incoming damage, resource starvation and actual cast counts. Record per-skill waiting, spacing distance, charging duration and mana recovered. Waiting intervals for several skills can overlap and must not be summed into total fight duration.

Repeat against stationary enemies, pursuing groups and bosses with movement or telegraphed danger. Higher damage is not enough if survival or offensive uptime worsens. Values are initial experimental settings, not a claim that every choice is equally strong.

## Warrior

### W01 · Whirlwind use policy

| Choice | Gain | Cost |
| --- | --- | --- |
| Default edict | Keep the existing Hunt Edict conditions, placement and target settings. | Opportunities that fail these conditions are left to other actions. |
| Follow while spinning | Keeps pressure on moving enemies. | Following can lead into dangerous ground. |
| Stationary spin | Holds the chosen position and defensive zone. | Loses enemies that move away. |

### W02 · Leap Slam use policy

| Choice | Gain | Cost |
| --- | --- | --- |
| Default edict | Keep the existing Hunt Edict conditions, placement and target settings. | Opportunities that fail these conditions are left to other actions. |
| Create distance | Retreats for up to 2s to gain 6m from the target and seek a longer leap. | Stops attacking while retreating; a blocked route falls back to the available distance. |
| Use current distance | Uses landing opportunities from 2m without spending time retreating. | Short leaps gain less extra damage. |

### W03 · Crushing Blow use policy

| Choice | Gain | Cost |
| --- | --- | --- |
| Default edict | Keep the existing Hunt Edict conditions, placement and target settings. | Opportunities that fail these conditions are left to other actions. |
| Crush when ready | Attacks single targets immediately. | May spend resource before an empowerment. |
| Wait for empowerment | Attacks when Crush empowerment is ready. | Misses opportunities without the enabling gear or proc. |

### W04 · Ground Slam use policy

| Choice | Gain | Cost |
| --- | --- | --- |
| Default edict | Keep the existing Hunt Edict conditions, placement and target settings. | Opportunities that fail these conditions are left to other actions. |
| Attack the group | Combines damage and control against groups. | May be unavailable for a dangerous cast. |
| Reserve for interrupts | Waits to interrupt an observed dangerous cast. | Reduces routine damage and cast count. |

### W05 · Iron Wall use policy

| Choice | Gain | Cost |
| --- | --- | --- |
| Default edict | Keep the existing Hunt Edict conditions, placement and target settings. | Opportunities that fail these conditions are left to other actions. |
| Stack shields | Builds protection against a damage burst. | Shields can expire together or reach the cap. |
| Stagger shields | Extends coverage after the previous shield. | Has less protection against a sudden burst. |

### W06 · Battle Shout use policy

| Choice | Gain | Cost |
| --- | --- | --- |
| Default edict | Keep the existing Hunt Edict conditions, placement and target settings. | Opportunities that fail these conditions are left to other actions. |
| Resource recovery | Prioritizes recovery when resource is low. | May miss empowerment before a major attack. |
| Damage window | Aligns the buff with an offensive engagement. | Recovery is wasted when resource is already high. |

### W07 · Brand of Challenge use policy

| Choice | Gain | Cost |
| --- | --- | --- |
| Default edict | Prefer elites or bosses; cast when the brand has at most 1s left. | Opportunities that fail these conditions are left to other actions. |
| Below 50% resource | Quickly refills resource for hooks and attacks. | Can prioritize resource timing over the best defensive mark. |
| React to a windup | Marks an enemy preparing danger for defensive value. | Delays recovery and routine mark uptime. |

### W08 · Impaling Hook use policy

| Choice | Gain | Cost |
| --- | --- | --- |
| Default edict | Pull a target that is outside the main melee skill's reach. | Opportunities that fail these conditions are left to other actions. |
| Use when available | Hits even nearby enemies to regain resource. | Spends the cooldown when a pull is unnecessary. |
| Use against distant enemies | Pulls a distant enemy into melee follow-ups. | Gives up resource gains against nearby enemies. |

### W09 · Raking Wound use policy

| Choice | Gain | Cost |
| --- | --- | --- |
| Default edict | Prefer an unbled elite or a cluster of at least 3 enemies. | Opportunities that fail these conditions are left to other actions. |
| Spread to new targets | Spreads damage to enemies without its bleed. | Defers refreshing bleed on the priority target. |
| Use against elites and bosses | Concentrates bleed on durable enemies. | Reduces bleed coverage on ordinary groups. |

### W10 · Blood Reclamation use policy

| Choice | Gain | Cost |
| --- | --- | --- |
| Default edict | Use with at least 2s of W09 bleed remaining, or below 60% HP with a bleeding target. | Opportunities that fail these conditions are left to other actions. |
| Cash bleed out early | Cashes its own bleed early for damage and healing. | Removes bleed-dependent opportunities for other attacks. |
| Time bleed expiry or healing | Cashes out near bleed expiry or below 40% health. | Delays healing and burst damage. |

### W11 · Resolute Advance use policy

| Choice | Gain | Cost |
| --- | --- | --- |
| Default edict | Approach an enemy only when the destination is no more dangerous than the current position. | Opportunities that fail these conditions are left to other actions. |
| Use when available | Uses even a short advance for mitigation. | Spends mobility early and changes position. |
| Use against distant enemies | Advances only to close a gap. | Loses mitigation and resource opportunities while close. |

### W12 · Iron Stance use policy

| Choice | Gain | Cost |
| --- | --- | --- |
| Default edict | Use against a nearby winding-up attacker or below 60% HP. | Opportunities that fail these conditions are left to other actions. |
| React to a windup | Times counters for observed preparations. | Wastes counter opportunity if the incoming hit misses. |
| Use at low health | Reserves defense for low health. | Gives up counter damage while healthy. |

### W13 · Battlefield Ring use policy

| Choice | Gain | Cost |
| --- | --- | --- |
| Default edict | Use when at least 3 enemies can be grouped for Whirlwind or Slam. | Opportunities that fail these conditions are left to other actions. |
| Use when available | Pulls even one target for positioning synergies. | Leaves fewer opportunities to gather a group. |
| Wait for several targets | Gathers three or more for follow-up area attacks. | Can wait a long time while enemies are scattered. |

### W14 · Tremor Wave use policy

| Choice | Gain | Cost |
| --- | --- | --- |
| Default edict | Use against at least 2 aligned enemies or a difficult-to-reach elite. | Opportunities that fail these conditions are left to other actions. |
| Use when available | Attacks one enemy at range with the wave. | May hit few enemies per cast. |
| Wait for several targets | Casts when the wave can reach at least three. | Delays casts against unaligned enemies. |

### W15 · Breath Before Battle use policy

| Choice | Gain | Cost |
| --- | --- | --- |
| Default edict | Use at 60% resource or less while preparing the next attack. | Opportunities that fail these conditions are left to other actions. |
| Below 30% resource | Waits for low resource to reduce overflow. | Attacks can stop before recovery starts. |
| Below 75% resource | Starts recovery early below 75% resource. | Wastes recovery if combat stops. |

### W16 · Guardian's Vow use policy

| Choice | Gain | Cost |
| --- | --- | --- |
| Default edict | Use when slowed or rooted, or below 55% HP without a barrier. | Opportunities that fail these conditions are left to other actions. |
| When no shield remains | Restores protection as soon as shields expire. | May spend the cooldown just before control arrives. |
| Reserve for cleansing slow | Reserves the skill for cleansing slow and immunity. | Forgoes shield opportunities without control. |

### W17 · War of the Ancestors use policy

| Choice | Gain | Cost |
| --- | --- | --- |
| Default edict | Use in elite or boss combat when at least 3 enemies are near or HP is at most 65%. | Opportunities that fail these conditions are left to other actions. |
| Use against elites and bosses | Summons ancestors as soon as an elite or boss is engaged. | Wastes summon duration if targets move away. |
| Use at low health | Combines ancestors with their defense below half health. | Can miss strong offensive windows while healthy. |

### W18 · Titan's Judgment use policy

| Choice | Gain | Cost |
| --- | --- | --- |
| Default edict | Use on an elite, boss or 4-enemy cluster while Battle Shout is active or resource is at least 80%. | Opportunities that fail these conditions are left to other actions. |
| Wait for several targets | Aims the large hit at four or more clustered targets. | Holds the long cooldown against isolated targets. |
| Align with the shout | Times Judgment during the shout buff. | Delays the ultimate without a timely shout. |

## Ranger

### A01 · Piercing Shot use policy

| Choice | Gain | Cost |
| --- | --- | --- |
| Default edict | Keep the existing Hunt Edict conditions, placement and target settings. | Opportunities that fail these conditions are left to other actions. |
| Allow one target | Reduces downtime without waiting for alignment. | Gets fewer hits per shot. |
| Wait for two | Fires when one shot can hit several enemies. | Holds the skill against isolated enemies. |

### A02 · Multishot use policy

| Choice | Gain | Cost |
| --- | --- | --- |
| Default edict | Keep the existing Hunt Edict conditions, placement and target settings. | Opportunities that fail these conditions are left to other actions. |
| Clear groups | Spreads arrows through a group. | Reduces focus on the priority target. |
| Focus one target | Concentrates hits on a priority target. | Delays clearing surrounding enemies. |

### A03 · Venom Trap use policy

| Choice | Gain | Cost |
| --- | --- | --- |
| Default edict | Keep the existing Hunt Edict conditions, placement and target settings. | Opportunities that fail these conditions are left to other actions. |
| Keep existing traps | Preserves spent resource and remaining duration. | Cannot relocate a trap enemies have left. |
| Replace oldest trap | Refreshes traps at the current fight. | Gives up remaining duration and field damage. |

### A04 · Retreat Leap use policy

| Choice | Gain | Cost |
| --- | --- | --- |
| Default edict | Keep the existing Hunt Edict conditions, placement and target settings. | Opportunities that fail these conditions are left to other actions. |
| Prefer safety | Finds a landing with less observed danger. | Can disrupt attack range and direction. |
| Lead into own trap | Leads enemies toward an existing trap. | May give up the safest short route. |

### A05 · Hunter's Mark use policy

| Choice | Gain | Cost |
| --- | --- | --- |
| Default edict | Keep the existing Hunt Edict conditions, placement and target settings. | Opportunities that fail these conditions are left to other actions. |
| Keep the mark | Concentrates mark synergies on one enemy. | Transfers amplification to a new priority later. |
| Transfer to new target | Transfers the mark to the new combat target. | Discards remaining mark duration. |

### A06 · Shadow Arrow use policy

| Choice | Gain | Cost |
| --- | --- | --- |
| Default edict | Keep the existing Hunt Edict conditions, placement and target settings. | Opportunities that fail these conditions are left to other actions. |
| Empower routine shots | Uses charges frequently for repeated attacks. | May be unavailable when an elite appears. |
| Reserve for elites | Concentrates charges on elite and boss fights. | Forgoes empowered attacks against ordinary enemies. |

### A07 · Successive Shots use policy

| Choice | Gain | Cost |
| --- | --- | --- |
| Default edict | Focus the marked target while keeping at least 25% resource in reserve. | Opportunities that fail these conditions are left to other actions. |
| Use when available | Keeps firing whenever resource permits. | Can starve the next expensive skill. |
| Leave 40% resource after casting | Fires only while leaving 40% resource afterward. | Increases basic attacks and waiting. |

### A08 · Patient Shot use policy

| Choice | Gain | Cost |
| --- | --- | --- |
| Default edict | Use on a marked target at least 7m away, with no immediate melee threat. | Opportunities that fail these conditions are left to other actions. |
| Use when available | Uses the powerful aimed shot without a mark. | Loses mark amplification and distance synergies. |
| Require a mark and distance | Aims only with a mark and at least 7m distance. | Holds fire against approaching enemies. |

### A09 · Briar Trap use policy

| Choice | Gain | Cost |
| --- | --- | --- |
| Default edict | Place inside Poison Trap or on an observed enemy approach path. | Opportunities that fail these conditions are left to other actions. |
| Use when available | Quickly places a trap near enemies. | Can overlap traps and control. |
| Prefer new control | Roots an enemy that is not yet controlled. | Holds damage against an already controlled group. |

### A10 · Frost Snare use policy

| Choice | Gain | Cost |
| --- | --- | --- |
| Default edict | Place when poisoned enemies are approaching. | Opportunities that fail these conditions are left to other actions. |
| Use when available | Applies slow before poison is ready. | Gives up the poison-to-freeze combo. |
| Freeze after poison | Waits for poison to enable a freeze. | Holds placement without a poison source. |

### A11 · Decoy Projection use policy

| Choice | Gain | Cost |
| --- | --- | --- |
| Default edict | Place away from the firing position at a valid low-risk point. | Opportunities that fail these conditions are left to other actions. |
| React to nearby enemies | Distracts enemies as they approach. | The decoy may die quickly. |
| Wait for several targets | Seeks a decoy explosion against a group. | Reserves the decoy against a lone pursuer. |

### A12 · Smoke Cover use policy

| Choice | Gain | Cost |
| --- | --- | --- |
| Default edict | Use below 55% HP or when at least 2 melee enemies are near. | Opportunities that fail these conditions are left to other actions. |
| React to nearby enemies | Gains speed when enemies close in. | Can spend mitigation before heavy damage arrives. |
| Use at low health | Reserves mitigation for an emergency. | Delays spacing recovery while healthy. |

### A13 · Venom Arrow use policy

| Choice | Gain | Cost |
| --- | --- | --- |
| Default edict | Prefer an unpoisoned marked or elite target outside Poison Trap. | Opportunities that fail these conditions are left to other actions. |
| Spread to new targets | Spreads poison to enemies without its own poison arrow. | Reduces repeat shots at the priority target. |
| Focus the marked target | Focuses direct damage and poison on the marked target. | Holds opportunities without a mark. |

### A14 · Forking Arrow use policy

| Choice | Gain | Cost |
| --- | --- | --- |
| Default edict | Use when at least 2 other enemies are near the marked target. | Opportunities that fail these conditions are left to other actions. |
| Use when available | Uses the first arrow even against a lone enemy. | Can waste the split arrows. |
| Wait for several targets | Waits for the first target and two split targets. | Casts less when enemies spread out. |

### A15 · Watch Ballista use policy

| Choice | Gain | Cost |
| --- | --- | --- |
| Default edict | Place outside hazards for sustained elite combat. | Opportunities that fail these conditions are left to other actions. |
| Use when available | Places the ballista as soon as a target appears. | Short fights waste remaining shots. |
| Use against elites and bosses | Deploys for longer elite or boss fights. | Loses extra shots during ordinary clearing. |

### A16 · Hunt Preparation use policy

| Choice | Gain | Cost |
| --- | --- | --- |
| Default edict | Use at 50% resource or less shortly before a paid skill. | Opportunities that fail these conditions are left to other actions. |
| Below 30% resource | Uses the recovery fully at low resource. | Resource may run dry before the next shot. |
| Before a paid skill | Prepares the discount before an available paid shot. | Wastes recovery when resource is high. |

### A17 · Killing Rain use policy

| Choice | Gain | Cost |
| --- | --- | --- |
| Default edict | Use on a marked elite or boss, or at least 4 rooted or slowed enemies. | Opportunities that fail these conditions are left to other actions. |
| Use against elites and bosses | Starts arrow rain promptly against an elite or boss. | A moving target can leave the fixed area. |
| Wait for a controlled group | Waits for at least three controlled enemies. | Holds the ultimate without a controlled group. |

### A18 · Shadow Pursuit use policy

| Choice | Gain | Cost |
| --- | --- | --- |
| Default edict | Use in mobile elite or boss combat with enough resource to fire. | Opportunities that fail these conditions are left to other actions. |
| Use when available | Starts repeating echoes early in the engagement. | Leaves charges unused if resource or targets disappear. |
| Leave 70% resource after casting | Starts pursuit with at least 70% resource. | Misses damage windows while waiting for resource. |

## Mage

### M01 · Fireball use policy

| Choice | Gain | Cost |
| --- | --- | --- |
| Default edict | Keep the existing Hunt Edict conditions, placement and target settings. | Opportunities that fail these conditions are left to other actions. |
| Steady fire | Does not wait for a clustered group. | Spends resource on smaller explosions. |
| Prefer blast efficiency | Waits for multi-target explosions. | Casts less against isolated targets. |

### M02 · Blizzard use policy

| Choice | Gain | Cost |
| --- | --- | --- |
| Default edict | Keep the existing Hunt Edict conditions, placement and target settings. | Opportunities that fail these conditions are left to other actions. |
| Keep the fields | Uses the remaining damage of existing fields. | Cannot readily follow moving enemies. |
| Refresh placement | Replaces the oldest field at the current target. | Discards the old field's remaining damage. |

### M03 · Chain Lightning use policy

| Choice | Gain | Cost |
| --- | --- | --- |
| Default edict | Keep the existing Hunt Edict conditions, placement and target settings. | Opportunities that fail these conditions are left to other actions. |
| Allow isolated target | Attacks even without a chain partner. | Loses chain hits and resource efficiency. |
| Wait for links | Casts when lightning can link several enemies. | Can hold the spell against an isolated boss. |

### M04 · Teleport use policy

| Choice | Gain | Cost |
| --- | --- | --- |
| Default edict | Keep the existing Hunt Edict conditions, placement and target settings. | Opportunities that fail these conditions are left to other actions. |
| Reserve for survival | Keeps teleport available for a major threat. | Leaves routine spacing to walking. |
| Use for spacing | Quickly restores combat range and position. | May be on cooldown when danger arrives. |

### M05 · Elemental Shield use policy

| Choice | Gain | Cost |
| --- | --- | --- |
| Default edict | Keep the existing Hunt Edict conditions, placement and target settings. | Opportunities that fail these conditions are left to other actions. |
| Stack shields | Concentrates protection on a short danger window. | Protection can expire at the same time. |
| Extend coverage | Follows the previous shield. | Can leave less protection against a burst. |

### M06 · Frost Nova use policy

| Choice | Gain | Cost |
| --- | --- | --- |
| Default edict | Keep the existing Hunt Edict conditions, placement and target settings. | Opportunities that fail these conditions are left to other actions. |
| Cast in place | Avoids approaching solely for a freeze. | Cannot control distant enemies. |
| Approach to freeze | Moves engaged enemies into freeze range. | Costs approach time and exposure to melee hits. |

### M07 · Ember Lance use policy

| Choice | Gain | Cost |
| --- | --- | --- |
| Default edict | Use against aligned enemies or an elite that is not burning. | Opportunities that fail these conditions are left to other actions. |
| Use when available | Attacks even one enemy with direct damage and burn. | Gets less value from piercing. |
| Wait for several targets | Waits for a line through three enemies. | Relies on other attacks until enemies align. |

### M08 · Firewall use policy

| Choice | Gain | Cost |
| --- | --- | --- |
| Default edict | Place across a Blizzard cluster or an enemy approach path. | Opportunities that fail these conditions are left to other actions. |
| Use when available | Places fire at the current enemy promptly. | Moving enemies can leave the fire. |
| Wait for control | Concentrates damage on a controlled enemy. | Delays placement without control. |

### M09 · Glacial Lance use policy

| Choice | Gain | Cost |
| --- | --- | --- |
| Default edict | Use against Blizzard-slowed targets or approaching enemies. | Opportunities that fail these conditions are left to other actions. |
| Use when available | Uses the spear to apply its own initial slow. | Can start with slow instead of freeze. |
| Freeze after a slow | Hits an already slowed enemy for a freeze. | Waits for a source of slow. |

### M10 · Frost Globe use policy

| Choice | Gain | Cost |
| --- | --- | --- |
| Default edict | Aim at the observed center of an approaching group. | Opportunities that fail these conditions are left to other actions. |
| Use when available | Sends the globe even at one enemy. | Can miss during travel time. |
| Wait for control | Targets a controlled enemy for a reliable explosion. | Can lose the opportunity as control expires. |

### M11 · Capacitor Orb use policy

| Choice | Gain | Cost |
| --- | --- | --- |
| Default edict | Place for sustained elite combat with enough resource to cast Chain Lightning. | Opportunities that fail these conditions are left to other actions. |
| Use when available | Places the orb even without Chain Lightning. | Gives up full charged-explosion power. |
| After Chain Lightning is ready | Waits for an available Chain Lightning and its cost. | Loses standalone orb damage and deployments. |

### M12 · Storm Spear use policy

| Choice | Gain | Cost |
| --- | --- | --- |
| Default edict | Use against a recently chained elite or aligned enemies. | Opportunities that fail these conditions are left to other actions. |
| Use when available | Attacks whenever a firing line is open. | Can miss the resource refund after Chain Lightning. |
| After Chain Lightning hits | Targets an enemy recently hit by its Chain Lightning. | Waits for Chain Lightning to land. |

### M13 · Mana Reclaim use policy

| Choice | Gain | Cost |
| --- | --- | --- |
| Default edict | Charges below 35% mana at a safe current position. | Opportunities that fail these conditions are left to other actions. |
| Charge through hits | Recovers immediately without travel and resumes offense sooner. | Continues taking damage while stationary. |
| Retreat before charging | Retreats for up to 2s to gain 5m from observed enemies. | Does not charge while fleeing and defers if no safe position is available. |

### M13:goal · When to stop charging

| Choice | Gain | Cost |
| --- | --- | --- |
| Short recovery | Resumes offense at 50% mana. | May need to charge again soon. |
| Deep preparation | Builds 90% mana. With Quiet Rift equipped inside your own ward, also waits for its shield when the equipment cooldown is ready. | Waiting for the shield keeps you channeling until 1.5s after cast start, even at full mana, delaying attacks. |

### M14 · Rift Ward use policy

| Choice | Gain | Cost |
| --- | --- | --- |
| Default edict | Use at a hazard-free casting position when no barrier is active. | Opportunities that fail these conditions are left to other actions. |
| When no shield remains | Creates a ward when no shield remains. | Moving away loses ward mitigation. |
| Use at low health | Saves defensive duration for a dangerous fight. | Loses routine defense for stationary or charging builds. |

### M15 · Elemental Compass use policy

| Choice | Gain | Cost |
| --- | --- | --- |
| Default edict | Use at 70% resource or less with at least 2 paid elemental skills of different elements equipped. | Opportunities that fail these conditions are left to other actions. |
| Below 30% resource | Prioritizes recovery after resource is depleted. | Does not prepare discounts in advance. |
| Use with three elements | Reserves it for a loadout containing all three elements. | Does not cast in a one- or two-element build. |

### M16 · Magnetic Vortex use policy

| Choice | Gain | Cost |
| --- | --- | --- |
| Default edict | Use when at least 3 enemies can be grouped inside Blizzard or Firewall. | Opportunities that fail these conditions are left to other actions. |
| Use when available | Uses control and periodic damage even on one enemy. | Spends the cooldown without gathering a group. |
| Wait for several targets | Gathers three or more for elemental area follow-ups. | Delays attacking scattered enemies. |

### M17 · Triune Collapse use policy

| Choice | Gain | Cost |
| --- | --- | --- |
| Default edict | If the loadout can generate an elemental-cycle buff, wait for it. Otherwise do not wait for an unavailable buff. In both cases, target an elite, boss, or at least four enemies. | Opportunities that fail these conditions are left to other actions. |
| Use against elites and bosses | Uses Collapse promptly on an elite or boss. | Can cast before elemental empowerment is ready. |
| Wait for elemental empowerment | Waits until elemental empowerment is active. | Does not cast without an empowerment source. |

### M18 · Sage Incarnate use policy

| Choice | Gain | Cost |
| --- | --- | --- |
| Default edict | Use in elite or boss combat with three different elemental skills equipped. | Opportunities that fail these conditions are left to other actions. |
| Use against elites and bosses | Empowers offense and defense early in elite fights. | May fail to complete an elemental cycle. |
| Prepare a three-element cycle | Transforms with three elements ready and sufficient resource. | Has no empowerment while preparing. |

## Integration

`ClassSkillOptions.For` returns bilingual choices, gains, costs and observation keys. `WithOption` edits a detached draft and `CommitClassSkills` persists it in town. HED4, presets and combat saves retain option IDs and values. Unknown data is rejected while the original is preserved. `InspectClassSkillAutomatic` explains current deferrals; `ReadClassSkillPolicies` returns observations. This task does not connect icons or change UI layout.
