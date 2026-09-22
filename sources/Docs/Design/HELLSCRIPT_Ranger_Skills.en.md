# HELLSCRIPT Ranger — 36 skills

Updated: 2026-09-22

Status: approved design. Ability and equipment implementation status is tracked in [the implementation record](../Implementation/Class_Skill_Runtime.en.md). General release and UI integration are pending. Values below are for rank one.

[Rules and sources](HELLSCRIPT_Class_Skills.en.md) · [Equipment matrix](HELLSCRIPT_Skill_Equipment.en.md) · [한국어](HELLSCRIPT_Ranger_Skills.md)

16 normal actives, 18 passives and 2 ultimates. Proposed loadout: up to 4 normal actives, up to 3 passives and exactly 1 ultimate after unlock. BASIC is separate. Entries 35 and 36 share the final tier and are mutually exclusive.

## Equipment-led build examples

Each example wears the full named set. Listed legendaries occupy different slots; use rares elsewhere. Configure the sequence in Hunt Edict, using the per-skill automatic-use suggestions below.

### Wandering Hunter · `REF_SA01`

Normal actives: Successive Shots (`A07`), Patient Shot (`A08`), Hunter's Mark (`A05`), Hunt Preparation (`A16`).

Passives: Marked Opening (`AP07`), Drawing Again (`AP09`), Focused Aim (`AP13`). Selected ultimate: Shadow Pursuit (`A18`).

Compatible legendaries: Patient Hunter's Bow (`DES_LA41`), Oath of Two Hunts (`DES_LA46`).

Reposition for a clear line, focus successive and aimed shots on the mark, and refuel with Preparation and BASIC.

### Survivor Mantle · `REF_SA02`

Normal actives: Retreat Leap (`A04`), Decoy Projection (`A11`), Smoke Cover (`A12`), Forking Arrow (`A14`).

Passives: Escaping Step (`AP02`), Decoy Tactics (`AP14`), Seamless Reload (`AP16`). Selected ultimate: Shadow Pursuit (`A18`).

Compatible legendaries: Venomcloud Landing (`LA23`), Nimble Reload (`LA24`).

Gather normal enemies around a decoy, fire splitting arrows, and alternate Retreat and Smoke Cover for survival.

### Marksman Manual · `REF_SA03`

Normal actives: Piercing Shot (`A01`), Multishot (`A02`), Hunter's Mark (`A05`), Venom Arrow (`A13`).

Passives: Piercing Gaze (`AP03`), Marked Opening (`AP07`), Venom Cycle (`AP10`). Selected ultimate: Killing Rain (`A17`).

Compatible legendaries: Oath of Two Hunts (`DES_LA46`), Veintracker (`LA31`).

Poison the mark first, then use Pierce and Multishot for the set's restoration and direct-damage bonuses.

### Colossus Hunt · `REF_SA04`

Normal actives: Piercing Shot (`A01`), Multishot (`A02`), Hunter's Mark (`A05`), Patient Shot (`A08`).

Passives: Long Reach (`AP01`), Finishing Shot (`AP06`), Silent Execution (`AP18`). Selected ultimate: Killing Rain (`A17`).

Compatible legendaries: Endless Trajectory (`LA01`), Narrowed Line (`LA02`).

Build three Pierce/Multishot casts on the same marked elite. Patient Shot and the ultimate do not substitute for those casts.

### Vengeful Sight · `REF_SA05`

Normal actives: Piercing Shot (`A01`), Multishot (`A02`), Watch Ballista (`A15`), Hunt Preparation (`A16`).

Passives: Drawing Again (`AP09`), Focused Aim (`AP13`), Pathfinder (`AP17`). Selected ultimate: Shadow Pursuit (`A18`).

Compatible legendaries: Prepared Outpost (`DES_LA45`), Oath of the Far Hunt (`LA06`).

Build five BASIC stacks before primary shots. The distant ballista provides support without generating BASIC stacks.

### Alchemical Hunting Ground · `REF_SA06`

Normal actives: Venom Trap (`A03`), Retreat Leap (`A04`), Briar Trap (`A09`), Frost Snare (`A10`).

Passives: Trap Hunter (`AP04`), Alchemical Practice (`AP11`), Prepared Escape (`AP12`). Selected ultimate: Killing Rain (`A17`).

Compatible legendaries: Oath of Two Hunts (`DES_LA46`), Viper's Molt (`LA03`).

Layer root and frost traps inside poison. New traps do not replace the set's Poison Trap count or first-poison-tick trigger.

### Threefold Venom · `REF_SA07`

Normal actives: Shadow Arrow (`A06`), Piercing Shot (`A01`), Multishot (`A02`), Venom Arrow (`A13`).

Passives: Quarry Tracks (`AP08`), Venom Cycle (`AP10`), Venom and Frost (`AP15`). Selected ultimate: Shadow Pursuit (`A18`).

Compatible legendaries: Narrowed Line (`LA02`), Black Contagion (`LA04`).

Pre-poison with Venom Arrow, prepare Shadow Arrow, then consume charges with Pierce or Multishot. Venom Arrow does not consume A06 charges.

### Sightless Shadow · `REF_SA08`

Normal actives: Retreat Leap (`A04`), Piercing Shot (`A01`), Multishot (`A02`), Smoke Cover (`A12`).

Passives: Escaping Step (`AP02`), Saved focus (`AP05`), Seamless Reload (`AP16`). Selected ultimate: Shadow Pursuit (`A18`).

Compatible legendaries: Oath of Two Hunts (`DES_LA46`), Unquenched Heart (`LC03`).

Use original shots during the 6s post-Retreat window. Smoke Cover supports survival without substituting for Retreat.

### Poison Gravekeeper · `SA`

Normal actives: Venom Trap (`A03`), Piercing Shot (`A01`), Retreat Leap (`A04`), Hunter's Mark (`A05`).

Passives: Trap Hunter (`AP04`), Venom Cycle (`AP10`), Prepared Escape (`AP12`). Selected ultimate: Killing Rain (`A17`).

Keep the original two/four-piece skill triggers. The new ultimate uses a separate slot and does not impersonate a set-bound skill.

### Long Shadow Tracker · `SAB`

Normal actives: Retreat Leap (`A04`), Piercing Shot (`A01`), Hunter's Mark (`A05`), Shadow Arrow (`A06`).

Passives: Long Reach (`AP01`), Escaping Step (`AP02`), Marked Opening (`AP07`). Selected ultimate: Shadow Pursuit (`A18`).

Keep the original two/four-piece skill triggers. The new ultimate uses a separate slot and does not impersonate a set-bound skill.

## Normal actives · 01–16

### 01. Piercing Shot · `A01`

Existing runtime retained · Lv.1 · Cooldown 0s · Resource 20

Pierces up to 5 enemies in a straight line.

**Suggested automatic use:** Keep the existing Hunt Edict conditions, placement and target settings.

**Equipment links:** Endless Trajectory (`LA01`), Trailing Arrowhead (`LA05`), Oath of the Far Hunt (`LA06`), Brand-Reading Eye (`LA07`), Shadow Rupture (`LA08`) and 5 more.

**Mechanic reference:** Existing HELLSCRIPT definition.

### 02. Multishot · `A02`

Existing runtime retained · Lv.3 · Cooldown 3s · Resource 30

Fires 3 arrows in a 60 degree arc.

**Suggested automatic use:** Keep the existing Hunt Edict conditions, placement and target settings.

**Equipment links:** Narrowed Line (`LA02`), Seed of Arrow Rain (`LA11`), Death Draws Near (`LA12`), Relentless Loading (`LA13`), Venom-Lacquered Fletching (`LA14`) and 3 more.

**Mechanic reference:** Existing HELLSCRIPT definition.

### 03. Venom Trap · `A03`

Existing runtime retained · Lv.6 · Cooldown 8s · Resource 20

Places a poison zone for 5 seconds. Up to 2 at once.

**Suggested automatic use:** Keep the existing Hunt Edict conditions, placement and target settings.

**Equipment links:** Viper's Molt (`LA03`), Trapper's Aim (`LA17`), Venom-Harvesting Knot (`LA18`), Trap-Leaving Step (`LA19`), Black Root (`LA20`) and 6 more.

**Mechanic reference:** Existing HELLSCRIPT definition.

### 04. Retreat Leap · `A04`

Existing runtime retained · Lv.10 · Cooldown 8s · Resource 0

Leaps to a spot that opens distance from enemies.

**Suggested automatic use:** Keep the existing Hunt Edict conditions, placement and target settings.

**Equipment links:** Viper's Molt (`LA03`), Escape-Opening Arrow (`LA10`), Venomcloud Landing (`LA23`), Nimble Reload (`LA24`), Fading Wound (`LA25`) and 3 more.

**Mechanic reference:** Existing HELLSCRIPT definition.

### 05. Hunter's Mark · `A05`

Existing runtime retained · Lv.15 · Cooldown 10s · Resource 0

Adds +25% damage against the marked target.

**Suggested automatic use:** Keep the existing Hunt Edict conditions, placement and target settings.

**Equipment links:** Quarry's Pledge (`LA29`), Rewritten Hunt Ledger (`LA32`), Binding Gaze (`LA33`), Brand-Reading Eye (`LA07`), Branded Concussion (`LA15`) and 3 more.

**Mechanic reference:** Existing HELLSCRIPT definition.

### 06. Shadow Arrow · `A06`

Existing runtime retained · Lv.20 · Cooldown 12s · Resource 10

Adds dark damage to the next 3 shots.

**Suggested automatic use:** Keep the existing Hunt Edict conditions, placement and target settings.

**Equipment links:** Black Contagion (`LA04`), Gloom Bloom (`LA34`), Venomshade Erosion (`LA35`), Shadow Shackles (`LA36`), Alchemist's Pulse (`LA37`).

**Mechanic reference:** Existing HELLSCRIPT definition.

### 07. Successive Shots · `A07`

New proposal · Lv.22 · Cooldown 0s · Resource 12

Fire 3 D45% physical arrows at one target within 10m, 0.15s apart. This is one cast, not three basic attacks. Each arrow collides with walls.

**Suggested automatic use:** Focus the marked target while keeping at least 25% resource in reserve.

**Equipment links:** Patient Hunter's Bow (`DES_LA41`).

**Companion skills:** Piercing Shot (`A01`), Hunter's Mark (`A05`).

**Mechanic reference:** D2 Strafe; D3 Rapid Fire.

### 08. Patient Shot · `A08`

New proposal · Lv.22 · Cooldown 5s · Resource 30

Aim for 0.9s, then deal D240% physical damage to one enemy within 12m. Gain +25% additive direct damage if the target is marked at impact. Moving during aim cancels the cast.

**Suggested automatic use:** Use on a marked target at least 7m away, with no immediate melee threat.

**Equipment links:** Patient Hunter's Bow (`DES_LA41`).

**Companion skills:** Piercing Shot (`A01`), Hunter's Mark (`A05`).

**Mechanic reference:** D2 Guided Arrow; D3 Impale.

### 09. Briar Trap · `A09`

New proposal · Lv.26 · Cooldown 8s · Resource 15

Place a trap within 6m. It arms after 0.4s and waits up to 6s. On enemy entry, hit up to 5 enemies in 2.5m for D80% physical damage and a 1.2s root, then remove it. Limit 1.

**Suggested automatic use:** Place inside Poison Trap or on an observed enemy approach path.

**Equipment links:** Winterthorn Grasp (`DES_LA42`).

**Companion skills:** Venom Trap (`A03`), Retreat Leap (`A04`).

**Mechanic reference:** D3 Caltrops / Spike Trap.

### 10. Frost Snare · `A10`

New proposal · Lv.26 · Cooldown 10s · Resource 15

Place one snare within 6m; arm after 0.4s and wait up to 6s. On entry, hit up to 5 enemies in 2.5m for D60% cold damage and a 40% slow for 3s. Enemies poisoned at impact are frozen for 0.75s.

**Suggested automatic use:** Place when poisoned enemies are approaching.

**Equipment links:** Winterthorn Grasp (`DES_LA42`).

**Companion skills:** Venom Trap (`A03`), Hunter's Mark (`A05`).

**Mechanic reference:** D2 Freezing Arrow; D3 Caltrops.

### 11. Decoy Projection · `A11`

New proposal · Lv.30 · Cooldown 14s · Resource 10

Create one decoy within 7m with 10% of hero maximum HP for 6s. Normal enemies prefer it for the first 2.5s. Destruction or expiry causes D80% secondary physical damage to up to 5 enemies within 2.5m. It cannot force elite or boss aggro.

**Suggested automatic use:** Place away from the firing position at a valid low-risk point.

**Equipment links:** Decoy Mantle (`DES_LA43`).

**Companion skills:** Retreat Leap (`A04`), Hunter's Mark (`A05`).

**Mechanic reference:** D2 Decoy.

### 12. Smoke Cover · `A12`

New proposal · Lv.30 · Cooldown 14s · Resource 0

Gain 20% damage reduction and 20% movement speed for 3s. This grants no invisibility, invulnerability or target break, and does not count as Retreat.

**Suggested automatic use:** Use below 55% HP or when at least 2 melee enemies are near.

**Equipment links:** Decoy Mantle (`DES_LA43`).

**Companion skills:** Retreat Leap (`A04`), Shadow Arrow (`A06`).

**Mechanic reference:** D3 Smoke Screen.

### 13. Venom Arrow · `A13`

New proposal · Lv.34 · Cooldown 4s · Resource 15

Deal D80% direct physical damage and D100% poison damage over 4s to one enemy within 10m. Keep only the greater remaining total for this skill's poison ledger.

**Suggested automatic use:** Prefer an unpoisoned marked or elite target outside Poison Trap.

**Equipment links:** Forked Fang (`DES_LA44`), Venom-Lacquered Fletching (`LA14`), Venom-Harvesting Knot (`LA18`), Venomshade Erosion (`LA35`), Venom-Weathered Hide (`LA39`).

**Companion skills:** Venom Trap (`A03`), Shadow Arrow (`A06`).

**Mechanic reference:** D2 Poison Javelin; D3 Elemental Arrow.

### 14. Forking Arrow · `A14`

New proposal · Lv.34 · Cooldown 5s · Resource 20

Deal D100% direct physical damage to a first target within 10m, then send D50% secondary arrows to 2 different enemies within 3m. No return to the first target or repeated splitting.

**Suggested automatic use:** Use when at least 2 other enemies are near the marked target.

**Equipment links:** Forked Fang (`DES_LA44`).

**Companion skills:** Piercing Shot (`A01`), Multishot (`A02`).

**Mechanic reference:** D2 Multiple Shot; D3 Hungering Arrow.

### 15. Watch Ballista · `A15`

New proposal · Lv.38 · Cooldown 12s · Resource 25

Place one ballista within 6m for 6s. Fire at one enemy within 9m each second for D35% secondary physical damage, up to 6 shots. Prefer a visible marked target. Shots are neither basic attacks nor A01.

**Suggested automatic use:** Place outside hazards for sustained elite combat.

**Equipment links:** Prepared Outpost (`DES_LA45`).

**Companion skills:** Venom Trap (`A03`), Hunter's Mark (`A05`).

**Mechanic reference:** D3 Sentry.

### 16. Hunt Preparation · `A16`

New proposal · Lv.38 · Cooldown 18s · Resource 0

Restore 25 resource and grant +20 percentage points of cost reduction to the next paid normal active within 6s. Free skills and ultimates do not consume the charge.

**Suggested automatic use:** Use at 50% resource or less shortly before a paid skill.

**Equipment links:** Prepared Outpost (`DES_LA45`), Unhurried Archer (`LA16`).

**Companion skills:** Piercing Shot (`A01`), Multishot (`A02`), Venom Trap (`A03`).

**Mechanic reference:** D3 Preparation.

## Passives · 17–34

### 17. Long Reach · `AP01`

Existing runtime retained

Adds +15% damage when the target is 7m or further away on hit.

**Equipment links:** Endless Trajectory (`LA01`), Trailing Arrowhead (`LA05`), Oath of the Far Hunt (`LA06`), Brand-Reading Eye (`LA07`), Shadow Rupture (`LA08`) and 5 more.

**Mechanic reference:** Existing HELLSCRIPT definition.

### 18. Escaping Step · `AP02`

Existing runtime retained

Adds +20%p movement speed for 2 seconds after Retreat Leap lands.

**Equipment links:** Viper's Molt (`LA03`), Escape-Opening Arrow (`LA10`), Venomcloud Landing (`LA23`), Nimble Reload (`LA24`), Fading Wound (`LA25`) and 3 more.

**Mechanic reference:** Existing HELLSCRIPT definition.

### 19. Piercing Gaze · `AP03`

Existing runtime retained

Piercing Shot hits up to 7 targets instead of 5.

**Equipment links:** Endless Trajectory (`LA01`), Trailing Arrowhead (`LA05`), Oath of the Far Hunt (`LA06`), Brand-Reading Eye (`LA07`), Shadow Rupture (`LA08`) and 5 more.

**Mechanic reference:** Existing HELLSCRIPT definition.

### 20. Trap Hunter · `AP04`

Existing runtime retained

Extends the root from placed traps and retreat traps by 25%.

**Equipment links:** Viper's Molt (`LA03`), Escape-Opening Arrow (`LA10`), Trapper's Aim (`LA17`), Venom-Harvesting Knot (`LA18`), Trap-Leaving Step (`LA19`) and 9 more.

**Mechanic reference:** Existing HELLSCRIPT definition.

### 21. Saved focus · `AP05`

Existing runtime retained

After walking for 1 second the next skill that costs resource is 25% cheaper. The effect then waits 4 seconds.

**Equipment links:** Endless Trajectory (`LA01`), Narrowed Line (`LA02`), Trailing Arrowhead (`LA05`), Oath of the Far Hunt (`LA06`), Brand-Reading Eye (`LA07`) and 13 more.

**Mechanic reference:** Existing HELLSCRIPT definition.

### 22. Finishing Shot · `AP06`

Existing runtime retained

Adds +10%p critical chance against enemies at or below 30% HP, capped at 75%.

**Equipment links:** Endless Trajectory (`LA01`), Narrowed Line (`LA02`), Trailing Arrowhead (`LA05`), Oath of the Far Hunt (`LA06`), Brand-Reading Eye (`LA07`) and 13 more.

**Mechanic reference:** Existing HELLSCRIPT definition.

### 23. Marked Opening · `AP07`

New proposal · Lv.22

Gain +5 percentage points of normal-active direct critical chance against marked enemies, sharing the 75% global cap.

**Equipment links:** Endless Trajectory (`LA01`), Trailing Arrowhead (`LA05`), Oath of the Far Hunt (`LA06`), Brand-Reading Eye (`LA07`), Shadow Rupture (`LA08`) and 9 more.

**Companion skills:** Hunter's Mark (`A05`), Piercing Shot (`A01`), Successive Shots (`A07`), Patient Shot (`A08`).

**Mechanic reference:** D3 conditional passive structure; HELLSCRIPT equipment requirements.

### 24. Quarry Tracks · `AP08`

New proposal · Lv.22

Gain 10% movement speed while an observed poisoned enemy is within 10m. This does not reveal enemies outside observation.

**Equipment links:** Viper's Molt (`LA03`), Trapper's Aim (`LA17`), Venom-Harvesting Knot (`LA18`), Trap-Leaving Step (`LA19`), Black Root (`LA20`) and 4 more.

**Companion skills:** Venom Trap (`A03`), Venom Arrow (`A13`).

**Mechanic reference:** D3 conditional passive structure; HELLSCRIPT equipment requirements.

### 25. Drawing Again · `AP09`

New proposal · Lv.22

A direct basic hit restores 2 resource, with a 0.5s interval. Splits, ballista shots and shadow echoes are not basic attacks.

**Equipment links:** Endless Trajectory (`LA01`), Narrowed Line (`LA02`), Trailing Arrowhead (`LA05`), Oath of the Far Hunt (`LA06`), Brand-Reading Eye (`LA07`) and 13 more.

**Companion skills:** Piercing Shot (`A01`), Multishot (`A02`).

**Mechanic reference:** D3 conditional passive structure; HELLSCRIPT equipment requirements.

### 26. Venom Cycle · `AP10`

New proposal · Lv.26

A direct attack killing a poisoned enemy restores 6 resource, with a 2s interval. Poison ticks and item secondary kills are excluded.

**Equipment links:** Viper's Molt (`LA03`), Trapper's Aim (`LA17`), Venom-Harvesting Knot (`LA18`), Trap-Leaving Step (`LA19`), Black Root (`LA20`) and 4 more.

**Companion skills:** Venom Trap (`A03`), Venom Arrow (`A13`).

**Mechanic reference:** D3 conditional passive structure; HELLSCRIPT equipment requirements.

### 27. Alchemical Practice · `AP11`

New proposal · Lv.26

When a directly placed Poison Trap, Briar Trap or Frost Snare arms, gain 8% damage reduction for 3s. Multiple traps refresh the same buff.

**Equipment links:** Viper's Molt (`LA03`), Trapper's Aim (`LA17`), Venom-Harvesting Knot (`LA18`), Trap-Leaving Step (`LA19`), Black Root (`LA20`) and 4 more.

**Companion skills:** Venom Trap (`A03`), Briar Trap (`A09`), Frost Snare (`A10`).

**Mechanic reference:** D3 conditional passive structure; HELLSCRIPT equipment requirements.

### 28. Prepared Escape · `AP12`

New proposal · Lv.26

Directly placing Poison Trap, Briar Trap or Frost Snare reduces equipped Retreat's remaining cooldown by 1s, with a 4s interval.

**Equipment links:** Viper's Molt (`LA03`), Escape-Opening Arrow (`LA10`), Trapper's Aim (`LA17`), Venom-Harvesting Knot (`LA18`), Trap-Leaving Step (`LA19`) and 10 more.

**Companion skills:** Venom Trap (`A03`), Retreat Leap (`A04`), Briar Trap (`A09`), Frost Snare (`A10`).

**Mechanic reference:** D3 conditional passive structure; HELLSCRIPT equipment requirements.

### 29. Focused Aim · `AP13`

New proposal · Lv.30

Pierce and Patient Shot gain +15% additive direct damage if stationary for at least 0.8s before cast start. Movement resets the timer.

**Equipment links:** Endless Trajectory (`LA01`), Trailing Arrowhead (`LA05`), Oath of the Far Hunt (`LA06`), Brand-Reading Eye (`LA07`), Shadow Rupture (`LA08`) and 6 more.

**Companion skills:** Piercing Shot (`A01`), Patient Shot (`A08`).

**Mechanic reference:** D3 conditional passive structure; HELLSCRIPT equipment requirements.

### 30. Decoy Tactics · `AP14`

New proposal · Lv.30

Gain +10% additive normal-active direct damage against enemies within 3m of your living decoy. Does not amplify the decoy's explosion.

**Equipment links:** Decoy Mantle (`DES_LA43`), Forked Fang (`DES_LA44`).

**Companion skills:** Decoy Projection (`A11`), Forking Arrow (`A14`).

**Mechanic reference:** D3 conditional passive structure; HELLSCRIPT equipment requirements.

### 31. Venom and Frost · `AP15`

New proposal · Lv.30

Gain +12% additive normal-active direct damage against enemies that are both poisoned and slowed. Freeze alone does not imply Slow.

**Equipment links:** Viper's Molt (`LA03`), Trapper's Aim (`LA17`), Venom-Harvesting Knot (`LA18`), Trap-Leaving Step (`LA19`), Black Root (`LA20`) and 5 more.

**Companion skills:** Venom Trap (`A03`), Frost Snare (`A10`), Venom Arrow (`A13`).

**Mechanic reference:** D3 conditional passive structure; HELLSCRIPT equipment requirements.

### 32. Seamless Reload · `AP16`

New proposal · Lv.34

Smoke Cover or Hunt Preparation prepares +15 percentage points of cost reduction for the next paid normal active within 6s. One charge; both share a 6s interval.

**Equipment links:** Decoy Mantle (`DES_LA43`), Prepared Outpost (`DES_LA45`).

**Companion skills:** Smoke Cover (`A12`), Hunt Preparation (`A16`).

**Mechanic reference:** D3 conditional passive structure; HELLSCRIPT equipment requirements.

### 33. Pathfinder · `AP17`

New proposal · Lv.34

Watch Ballista gains +10% additive damage while the hero is at least 5m away. Check distance when each shot is created.

**Equipment links:** Viper's Molt (`LA03`), Escape-Opening Arrow (`LA10`), Venomcloud Landing (`LA23`), Nimble Reload (`LA24`), Fading Wound (`LA25`) and 4 more.

**Companion skills:** Watch Ballista (`A15`), Retreat Leap (`A04`).

**Mechanic reference:** D3 conditional passive structure; HELLSCRIPT equipment requirements.

### 34. Silent Execution · `AP18`

New proposal · Lv.34

If Killing Rain's first pulse hits a marked target, deal D100% extra physical damage once to that target. With Shadow Pursuit selected, add 2s duration and 2 total allowed procs. Apply only the selected branch.

**Equipment links:** Oath of Two Hunts (`DES_LA46`).

**Companion skills:** Killing Rain (`A17`), Shadow Pursuit (`A18`).

**Mechanic reference:** D3 conditional passive structure; HELLSCRIPT equipment requirements.

## Final tier: ultimates · 35–36 · choose one

### 35. Killing Rain · `A17`

New proposal · Lv.40 · Cooldown 60s · Resource 0

Rain arrows at a fixed point within 10m, in a 4m radius. Six pulses 0.5s apart deal D80% physical damage to up to 8 enemies each. These are not A02 casts or A06 charged shots.

**Suggested automatic use:** Use on a marked elite or boss, or at least 4 rooted or slowed enemies.

**Equipment links:** Oath of Two Hunts (`DES_LA46`).

**Companion skills:** Piercing Shot (`A01`), Multishot (`A02`), Hunter's Mark (`A05`).

**Mechanic reference:** D3 Rain of Vengeance.

### 36. Shadow Pursuit · `A18`

New proposal · Lv.40 · Cooldown 60s · Resource 0

Gain 20% movement speed for 8s. After the first direct hit of BASIC, A01, A02, A07, A08, A13 or A14, deal D60% secondary shadow damage to that target. Limit once per cast, once per second and 8 procs total; echoes consume no charges and trigger no further procs.

**Suggested automatic use:** Use in mobile elite or boss combat with enough resource to fire.

**Equipment links:** Oath of Two Hunts (`DES_LA46`).

**Companion skills:** Piercing Shot (`A01`), Multishot (`A02`), Retreat Leap (`A04`), Shadow Arrow (`A06`).

**Mechanic reference:** D3 Vengeance / Shadow Power.
