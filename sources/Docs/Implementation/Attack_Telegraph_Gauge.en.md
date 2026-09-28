# Enemy and Boss Attack Warning Gauge

Updated: 2026-09-28 · [한국어](Attack_Telegraph_Gauge.md)

Every enemy and boss attack in a rift first shows its footprint on the ground and then fills it like a gauge. The damage applies on the tick the gauge is full, and the footprint flashes once at that moment. The aim is that a player can read where and when an attack lands at a glance and dodge it. Combat rules such as preparation times, areas and damage are unchanged. This is the first item of stage 2 of the [dark gothic overhaul](Dark_Gothic_Overhaul_Stage1.en.md).

## How it looks

- The whole footprint shows faintly first. The filled part is drawn stronger and the edge of the fill is a bright line.
- Circles and sectors fill from the centre outward, rings from the inner edge outward, and lines (charge paths, arrows, beams) from the start to the end.
- Past 70% the fill turns hotter and the outline beats faster.
- On the tick the gauge is full the damage applies and the whole footprint flashes nearly white for 0.3 s before it disappears. Projectile warnings (arrows, bolts, the hook, orbs, shards, venom) do not flash; they continue as the projectile in flight.
- Boss warnings are brighter and bolder than regular ones. Lingering zones and a charge path while the charger runs read full. Safe spots are shown separately as blue circles.

## Timing

- The gauge is the fraction of time from the start of the warning to the moment the attack lands (now plus the remaining delay). It uses the real preparation time, so the gauge on screen and the damage never disagree.
- Circles a boss drops one after another after its release (Grave Toll, Triple Eruption, the blast chain) count one gauge from the moment the boss started warning until each circle lands. They do not start over when they turn into separate ground effects at the release.
- An attack that pulls first and bites later, such as the Devouring Pull, keeps one unbroken gauge up to the bite.
- A followup warned after the first hit has landed, such as the slam's second hit or the second charge, starts a new gauge.
- A warning interrupted by stun or freeze disappears without a flash, and a lingering zone does not flash when it expires.
- Only the presentation is computed; combat state and the combat random stream are never touched.

## Coverage

| Group | Attacks |
| --- | --- |
| Regular enemies | Every attack of N01–N12 (melee sectors, charge lines, arrow and bolt lines, ground circles). N13–N20 use N01's melee sector until their own attacks land. |
| Elites | E01 chasing fire, E02 ice ring, E05 corpse burst |
| Deaths | N06 death burst, N12 shard burst |
| Bosses | The basic attack, all 30 patterns and the phase-2 roar of the five bosses |

## Implementation

- [`TelegraphGauge.cs`](../../Assets/HELLSCRIPT/Runtime/Presentation/TelegraphGauge.cs) computes each warning's gauge from its start and landing times and reports the warnings that completed this frame (released, activated or rewound by a followup). It remembers the preparation of every attack it saw, so the chained circles created at release continue the boss's warning.
- [`WorldView.Enemies.cs`](../../Assets/HELLSCRIPT/Runtime/Presentation/WorldView.Enemies.cs) rents a fill for each warning, passes it the gauge and flashes completed warnings. The existing outlines and the keys smokes read are unchanged.
- [`WorldFx.Telegraph.cs`](../../Assets/HELLSCRIPT/Runtime/Presentation/WorldFx.Telegraph.cs) holds the circle, ring, sector and line fill meshes and the release flash; [`WorldTelegraph.shader`](../../Assets/HELLSCRIPT/Resources/WorldTelegraph.shader) draws the gauge and the flash.
- The VFX core workstream is merged with it: the pooled effects library `WorldFx*.cs` and 34 procedural effect textures, whose provenance is recorded in [asset provenance](Asset_Provenance.md).

## Verification

Checked on 2026-09-28 in a cloned project and a macOS development build (`91821b37`).

- **EditMode `TelegraphGaugeTests`, 73 tests passing**, covering the attacks of 18 regular enemies, both death bursts, three elite hazards, and the basic attack, six patterns and roar of all five bosses.
  - Each gauge starts empty, only rises, and is full and completes on the tick its attack lands.
  - Damage applies only on completion ticks.
  - Chained boss circles keep filling after the release.
  - Interrupted warnings and expiring zones are not counted as completions.
- **Nine related suites**: 393 of 395 passed. The two failures (`EnemyTests.DeathPayloadRestoresIndependentlyAndNeverRepeatsItsReward`) are in the pre-overhaul baseline.
- **`-hellscriptTelegraphSmoke`**: for all 85 warnings shown, the fill's `_Progress` matched the gauge. Gauges passed 25, 50 and 75% before release, every landed attack flashed, and chained boss circles kept filling after the release. [Result](AttackTelegraphGaugeEvidence/runtime-telegraph-smoke.txt)
  - Regular enemies: [N01 melee](AttackTelegraphGaugeEvidence/gauge-n01-swing.png), [N02 charge](AttackTelegraphGaugeEvidence/gauge-n02-charge.png), [N09 triple bolts](AttackTelegraphGaugeEvidence/gauge-n09-arrows.png), [N04 poison pool](AttackTelegraphGaugeEvidence/gauge-n04-pool.png), [many shapes at once](AttackTelegraphGaugeEvidence/every-shape-at-once.png)
  - Bosses: [Reaping Sweep](AttackTelegraphGaugeEvidence/gauge-b01-sweep.png), [Grave Toll chain](AttackTelegraphGaugeEvidence/gauge-b01-grave-toll.png), [Dirge Ring](AttackTelegraphGaugeEvidence/gauge-b02-dirge-ring.png), [Hymn of Silence with safe spots](AttackTelegraphGaugeEvidence/gauge-b02-hymn-refuges.png), [blast chain](AttackTelegraphGaugeEvidence/gauge-b03-blasts.png), [Triple Eruption](AttackTelegraphGaugeEvidence/gauge-b04-triple-eruption.png), [Frost Nova](AttackTelegraphGaugeEvidence/gauge-b05-frost-nova.png), [Glacial Spikes](AttackTelegraphGaugeEvidence/gauge-b05-glacial-spikes.png). Each sheet shows 25%, 50%, 75% and the release.
- **Boss kit smoke** (`-hellscriptBossResume 4`): all 30 patterns and five roars released on their preparation time. [Result](AttackTelegraphGaugeEvidence/runtime-boss-kit-smoke.txt)
- **Smokes that already failed before this change**: the enemy smoke, the restart stages (1–3) of the boss smoke and the battle layout smoke fail the same way on the stage 1 build without this change. The enemy smoke misses the setting that keeps a fresh account out of the tutorial; the others stop while preparing their scenes. The enemy smoke's setup is fixed in the next item.

## Not verified

- Nothing was seen on a physical device or in an Android build.
- The gauge on screen is computed per displayed frame. The last frame before an attack lands is short of full by one frame's interval, and the landing itself shows as the flash.
- Overlapping warnings stack their fills and look stronger. The overlap was not tuned separately.
- This is not a human quality review; it was checked from captures and measurements.

## Next items

Windup poses and heating rims before attacks, landing effects and hit reactions, the boss pattern redesign, and the own attacks of N13–N20 follow in that order.
