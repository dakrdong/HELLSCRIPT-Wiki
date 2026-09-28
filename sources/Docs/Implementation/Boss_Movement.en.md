# Bosses That Move

Updated: 2026-09-28 · [한국어](Boss_Movement.md)

Bosses no longer stand in place while they attack; they move as they fight. Each of the five bosses has at least one travelling pattern in each phase, twelve in total, which leap, dash, tunnel, glide, blink or spin forward and then strike where they land. While waiting for a pattern they circle the hero. This is the third item of stage 2 of the [dark gothic overhaul](Dark_Gothic_Overhaul_Stage1.en.md). The combat rules are in [boss combat detail](../Design/HELLSCRIPT_Boss_Combat_Detail.en.md).

## Changed patterns

| Boss | Pattern | Movement |
| --- | --- | --- |
| Graveyard Executioner | Reaping Sweep (phase 1) | Dashes up to 7 m past the hero (D80% on body contact), then sweeps 220° where it stopped |
| Graveyard Executioner | Execution Leap (phase 2) | A 0.55 s arc; a 3 m radius as it lands |
| Graveyard Executioner | Chain Whirl (phase 2) | Spins down a warned 7.5 m lane at 3 m/s; the whirl rides with the boss |
| Graveyard Executioner | Grave Toll (phase 2) | Hops onto three points along the aim, 0.4 s each; a 2 m radius on every landing |
| Chorister of the End | Dirge Ring (phase 1) | Blinks to a spot 3.2 m beside the hero, then the ring |
| Chorister of the End | Lament Orbs (phase 2) | Blinks 5.5 m beside the hero, then orbs in all directions |
| Rift Devourer | Maw Snap (phase 1) | Lunges up to 4 m, then bites |
| Rift Devourer | Rift Tear (phase 2) | Runs up to 12 m and leaves a rift along its path |
| Dune Tyrant | Burrow Strike (phase 1) | Tunnels at 9 m/s and bursts up in front of the hero |
| Dune Tyrant | Triple Eruption (phase 2) | Tunnels from point to point and bursts up at each |
| Hoarfrost Matriarch | Frost Nova (phase 1) | Glides up to 5 m over ice, then bursts around itself |
| Hoarfrost Matriarch | Shatter Fan (phase 2) | Blinks 4.5 m back, then fires the ice shards |

The other eighteen patterns (slam, hook, summons, beam, charge, blast chain and so on) keep their rules. The existing charge, the hook's pull and the Devouring Pull already move the boss or the hero.

## Rules

- Paths and landings are planned around walls and obstacles when preparation starts and are saved. The boss sets off on the tick after the release. Without a place to land the pattern is not used and is reconsidered a second later.
- Each landing's warning gauge is full the moment the boss lands there. A dash that hits with its body also warns the path it will run, which is full at the release.
- The boss can be hit while it travels. Staggered, it stops where it is; ground effects it already made stay.
- Gap-closing patterns got longer ranges: Reaping Sweep and Maw Snap 7 m, Frost Nova 6.5 m, Chain Whirl 6 m, Dirge Ring 9 m.
- While no pattern is ready, the boss circles the hero at 55% of its speed, turning the other way every three seconds; the same seed circles the same way.
- The save uses combat action version 7 and stores the leg in progress, its elapsed time and which whirl rides with its boss. A travelling pattern from an older save that was preparing without planned landings lands on its aim.
- The forecast computes the legs, the dash body hit and every pulse of the riding whirl exactly as the fight does.

## On screen

- Leaps arc into the air (about 3.7 m at the top); burrows sink under the floor, travel throwing up sand and burst out.
- Dashes and glides lean into the run and leave dust or frost; the chain whirl spins fast and throws sparks.
- Blinks dissolve away in dark sparks and reappear at the new spot.
- Phase-2 bosses burn along their rim with flames at their feet. The roar throws a second, wide shockwave and shakes the screen hard.

## Verification

Checked on 2026-09-28 in a cloned project and a macOS development build (`b6f0eadc`, with the latest main merged).

- **Full EditMode suite**: 4,519 of 4,566 passed. All 47 failures are in the pre-overhaul baseline; there are no new failures. [Summary](BossMovementEvidence/editmode-full-summary.txt)
- **`BossMovementTests` (15)**:
  - Every boss has a travelling pattern in each phase.
  - All twelve travelling patterns land on planned ground, not in walls or obstacles.
  - Leaps and blinks take their fixed time; dashes, burrows and glides run at their pace.
  - The whirl rides with its boss.
  - A waiting boss circles the hero, and the same seed circles the same way.
- **Forecast**: for all 30 announced boss attacks the forecast equals the real damage (`IncomingForecastTests`).
- **Warning gauge**: the basic attack, the 30 patterns and the roar fill their gauges and complete on the tick they land (`TelegraphGaugeTests`).
- **Server replay**: `AuthoritativeRiftTests` pass.
- **`-hellscriptBossMotionSmoke` passed** in the running game: all twelve travelling patterns left their spot and landed on open ground at their planned end, a waiting boss circled the hero, and the roar opened phase 2. [Result](BossMovementEvidence/runtime-boss-motion-smoke.txt)
  - Captures (windup, mid-travel, landing): [Executioner](BossMovementEvidence/boss-1-executioner.png), [Chorister](BossMovementEvidence/boss-2-chorister.png), [Devourer](BossMovementEvidence/boss-3-devourer.png), [Dune Tyrant](BossMovementEvidence/boss-4-dune-tyrant.png), [Hoarfrost Matriarch](BossMovementEvidence/boss-5-hoarfrost-matriarch.png), [circling and roar](BossMovementEvidence/boss-circling-and-roar.png)
- **Boss kit smoke** (`-hellscriptBossResume 4`), the **warning gauge smoke**, the **attack effects smoke** and the **boss smoke's first stage** also passed on the same build. [Kit](BossMovementEvidence/runtime-boss-kit-smoke.txt), [gauge](BossMovementEvidence/runtime-telegraph-smoke.txt)

## Not verified

- Nothing was seen on a physical device or in an Android build.
- Boss bodies are still placeholder shapes. Leap height, burrow depth and the blink's fade are tuned again when the models are wired in.
- The boss smoke's restart stages (1–3) stop at the hook scene, as they did before this change.
- More travelling patterns can change how hard boss fights are. Difficulty in real play was not measured.
- This is not a human quality review.
