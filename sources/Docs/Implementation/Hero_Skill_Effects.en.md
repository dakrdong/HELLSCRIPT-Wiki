# Hero Skill Effects

Updated: 2026-09-28 · [한국어](Hero_Skill_Effects.md)

Hero skills now wear effects from the effects library. This is the seventh item of stage 2 of the [dark gothic overhaul](Dark_Gothic_Overhaul_Stage1.en.md). No game rule or value changed. Every effect follows the fog of war (nothing shows outside sight), freezes with the game when it pauses, and takes its size from the skill's own radius, range or length.

## The original skills (slots 1–6 of each class and the basic attacks)

The existing lines and rings stay, with effects layered on top.

| Skill | Added |
| --- | --- |
| W01 Whirlwind | a 330° blade trail, dust |
| W02 Leap Slam | landing shockwave, impact debris, a floor crack, dust |
| W03 Crushing Blow | a 100° trail, impact |
| W04 Ground Slam | a 3 m shockwave, dust, a crack |
| W05 Iron Wall | a brass flash |
| W06 Battle Shout | an ember shockwave, embers |
| A04 Retreat Leap | dust |
| A06 Shadow Arrow | shadow motes |
| M01 Fireball explosion | an explosion, a shockwave, scorch marks |
| M03 Chain Lightning | lightning bolts |
| M04 Teleport | arcane motes where the blink starts and ends |
| M05 Elemental Shield | a frost flash |
| M06 Frost Nova | a frost shockwave, ice shards, a frost mark |
| Warrior basic attack | an 85° trail |

## The 36 class skills (slots 7–18)

Until now these showed nothing but the sparks of their hits. Each now plays an effect once when its cast is released, and its lasting effects stay on screen while they exist.

- **Release effects**: Brand of Challenge marks its target with blood and a sigil; Impaling Hook throws a chain; Raking Wound cuts a 90° blood trail; Blood Reclamation bursts in a blood shockwave with healing motes; Resolute Advance leaves a dash streak and a landing shock; Iron Stance sweeps its front; Battlefield Ring sends a shockwave; Tremor Wave runs dust and cracks forward; Guardian's Vow flashes; War of the Ancestors sends a spirit shockwave; Titan's Judgment lands a large shockwave, an explosion, a crack and a camera shake. Patient Shot fires a beam with an impact; Smoke Cover billows smoke; Ember Lance, Glacial Lance and Storm Spear reach their full range as fire, ice and lightning beams; Elemental Compass shows motes of all three elements; Magnetic Vortex rings with lightning; Sage Incarnate flashes with arcane motes.
- **Lasting effects**: arrows from Successive Shots, Venom Arrow and Forking Arrow, and the Frost Globe fly along their real positions; Briar Trap, Frost Snare, Rift Ward and Magnetic Vortex mark the ground; Firewall burns along its length; Killing Rain and Triune Collapse strike on every tick (the collapse goes fire, then cold, then lightning); Capacitor Orb and Magnetic Vortex arc lightning to the nearest enemy on each tick; the ancestor spirits and the decoy stand as figures; Watch Ballista stands where it was set.

## Verification

Checked on 2026-09-28 in a cloned project and a macOS development build.

- **Focused Edit Mode checks after integration**: all 157 passed, covering environment, audio, new-hero defaults, shared UI, world art, shaders and effects. [Integration evidence](CompletedIntegrationEvidence/editmode-summary.txt)
- **`-hellscriptArtGallerySmoke` passed (59 captures)**: two new scenes write class skill state directly and show the effects. Neither changed the game state. [Result](HeroSkillEffectsEvidence/runtime-art-gallery-smoke.txt)
  - [Lasting and release effects](HeroSkillEffectsEvidence/class-skill-lasting-and-releases.png): on the left all eleven lasting effects show (Firewall, Frost Snare, Rift Ward, Magnetic Vortex, Capacitor Orb, Killing Rain, an ancestor spirit, the decoy, Watch Ballista, an arrow and the Frost Globe; 11 views) with their tick lightning, strikes and swing. On the right, Titan's Judgment, Ember Lance (east), Storm Spear (north-west) and Raking Wound (south) 0.14 s after release.
  - [The 16 original skill kinds](HeroSkillEffectsEvidence/legacy-skill-kinds.png): trails, shockwaves, element bursts and bolts on top of the original lines and rings.
- **`-hellscriptAttackFxSmoke` passed**: the enemy attack effect smoke still passes. [Result](HeroSkillEffectsEvidence/runtime-attack-fx-smoke.txt)
- **Fixed on the way**: large explosions drew a straight edge where they met the floor. The explosion's big billboards now fade out near the ground (PC), and the skills' explosions sit a little higher.

## Not verified

- Nothing was seen on a physical device or in an Android build.
- The 36 skills were not used one by one in real fights. The lasting and release effects were checked in the art gallery by writing their state directly.
- The mobile cost of many overlapping effects was not measured.
- This is not a human quality review.
