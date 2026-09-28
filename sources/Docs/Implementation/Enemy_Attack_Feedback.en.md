# Enemy Attack Windups and Hit Reactions

Updated: 2026-09-28 · [한국어](Enemy_Attack_Feedback.md)

Enemy attacks now read as attacks before they land, and landing and being hit get clear reactions. Where the [warning gauge](Attack_Telegraph_Gauge.en.md) tells where and when, this item shows who attacks and with what, through bodies and effects. Combat rules are unchanged. This is the second item of stage 2 of the [dark gothic overhaul](Dark_Gothic_Overhaul_Stage1.en.md).

## Before the attack

- Enemies move their bodies according to the attack. Melee attackers lean back and raise the weapon overhead, chargers crouch low, shooters aim and ease back, casters lift off the ground and raise their arms.
- The body's rim heats red as the warning gauge fills (bosses glow orange), and past 80% the body trembles.
- Casters and bosses gather sparks of their element at the hand (fire, frost, lightning, poison, shadow or plain sparks), faster towards the end.

## When it lands

- A melee attacker lunges and brings the weapon down while a slash sweeps the sector. A charger leaves dust and a shockwave where it sets off and trails dust as it runs. A shooter recoils with a muzzle flash.
- The landed footprint plays an effect by shape and element: circles a shockwave, burst and ground mark; rings bursts along the ring; lines a chain of bursts (a bolt for cold, lightning and shadow). A landing next to the hero shakes the camera, harder for bosses.
- Projectiles carry their element's colour in flight and burst where they stop.
- A lingering zone that has landed turns from the red gauge to its element's colour: green poison, orange fire, blue cold, violet shadow. Fire burns with flames; the others bubble and smoke. Warnings that have not landed stay red, so "about to land" and "already on the ground" differ by colour.

## When something is hit

- The hero flashes red when hit, with sparks or blood of the element that hit, and the camera shakes with the share of maximum health the hit took. The shake is off when the device's reduce-motion setting (`hellscript.reduce-motion`) is on. A dodge kicks up dust.
- A struck enemy flashes white and throws debris by material (flesh, bone, metal, crystal, ice, ichor, spirit); a critical hit flashes bigger.
- A killed enemy dissolves where it fell with a glowing edge and leaves a death burst and ground mark for its material.

## Implementation

- [`WorldView.ActorMotion.cs`](../../Assets/HELLSCRIPT/Runtime/Presentation/WorldView.ActorMotion.cs): attack poses, rim glow, hit flashes, death dissolve and the hero-hit shake. Parts of the placeholder bodies move around the feet, so the actor hierarchy and every lookup by name or child index are unchanged.
- [`WorldView.AttackFx.cs`](../../Assets/HELLSCRIPT/Runtime/Presentation/WorldView.AttackFx.cs): landing effects (played when the gauge completes), caster sparks and charge trails, projectile flashes and impacts, and lingering zone effects.
- [`WorldView.Enemies.cs`](../../Assets/HELLSCRIPT/Runtime/Presentation/WorldView.Enemies.cs) tints landed lingering zones by element. The old red hit spheres are replaced by the new reactions, which read the damage log.
- [`RuntimeEnemySmoke.cs`](../../Assets/HELLSCRIPT/Runtime/Presentation/RuntimeEnemySmoke.cs): fixes the setup that sent a fresh account into the tutorial and built illegal loadouts, so the smoke never reached its first scene.

## Verification

Checked on 2026-09-28 in a cloned project and a macOS development build (`180c9c36`).

- **EditMode**: 249 of 251 tests in five related suites passed. The two failures are `EnemyTests.DeathPayloadRestoresIndependentlyAndNeverRepeatsItsReward` from the pre-overhaul baseline.
- **`-hellscriptAttackFxSmoke` passed**, confirming in the running game: [Result](EnemyAttackFeedbackEvidence/runtime-attack-fx-smoke.txt)
  - Every winding-up enemy heated its rim.
  - A landed melee attack spawned effects and hit the hero, and the struck hero flashed.
  - The charge trail, a fire patch, arrows in flight and their impact, a poison pool and an elite ice ring were shown.
  - A struck enemy flashed and a killed one dissolved.
  - Captures: [windups and a strike](EnemyAttackFeedbackEvidence/windup-and-strike.png), [charge and fire patch](EnemyAttackFeedbackEvidence/charge-and-fire-patch.png), [shots and impact](EnemyAttackFeedbackEvidence/shots-and-impact.png), [poison pool and ice ring](EnemyAttackFeedbackEvidence/poison-pool-and-ice-ring.png), [enemy hit and death](EnemyAttackFeedbackEvidence/enemy-hit-and-death.png)
- **Warning gauge and boss kit smokes** passed on the same build.
- **Enemy smoke**: with the setup fixed, its first launch (the charge scene) passed for the first time. The relaunch stops in the death-burst scene, which waits for three delayed payloads at once. A build without this item's presentation changes stops at the same step, so this is recorded as a pre-existing scene setup problem. [Comparison](EnemyAttackFeedbackEvidence/enemy-smoke-comparison.txt)

## Not verified

- Nothing was seen on a physical device or in an Android build. On mobile the effects library halves particle counts, but performance was not measured.
- Bodies are still placeholder shapes, so poses and dissolves look small and simple. Once the 3D models are wired in, the same inputs drive their rig animation.
- There is no settings screen yet to switch the reduce-motion flag.
- This is not a human quality review.
