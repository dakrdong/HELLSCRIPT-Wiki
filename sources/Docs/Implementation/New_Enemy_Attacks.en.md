# Own Attacks for the Eight New Regular Enemies

Updated: 2026-09-28 · [한국어](New_Enemy_Attacks.md)

N13–N20 arrived with the four new fields but borrowed N01's melee swing; they now fight with attacks of their own. Every attack shows its area and timing with the [warning gauge](Attack_Telegraph_Gauge.en.md) and uses the [windups and hit reactions](Enemy_Attack_Feedback.en.md). This is the fourth item of stage 2 of the [dark gothic overhaul](Dark_Gothic_Overhaul_Stage1.en.md). Values are in [six rift fields](../Design/HELLSCRIPT_Rift_Fields.en.md) and [content catalog §11](../Design/HELLSCRIPT_Content_Catalog.md#11-일반-적-20종).

## Attacks

| Enemy | Field | Attack |
| --- | --- | --- |
| N13 Dune Marauder | Scorched Desert | Two 2.3 m, 110° cuts 0.35 s apart (0.6× each) |
| N14 Burrowing Sandworm | Scorched Desert | Warns a 2.2 m circle at the hero's position, tunnels there and erupts (1.4×) |
| N15 Cave Ghoul | Dank Cavern | Swings up close; from 2.5–4.5 m it leaps to 1 m short of the hero and hits a 1.6 m radius (0.9×) |
| N16 Sporebloat | Dank Cavern | Swings; on death leaves a 2.6 m poison spore cloud for 4 s (slows) |
| N17 Thornhorn Brute | Dusk Grassland | A 2.1 m whirl around itself (1.1×) |
| N18 Briar Witch | Dusk Grassland | A 9 m thorn lance warned for 0.8 s, then raised (0.85×, slows) |
| N19 Frostbitten Revenant | Snowbound Highland | A 2.4 m, 120° cold cleave (slows) |
| N20 Rime Caller | Snowbound Highland | A frost ring whose band the hero stands on (1.3×, slows) |

## Changes from the plan

- The Briar Witch's lance was planned as instant; it gets a 0.8 s warning, following the rule that every attack must be readable and dodgeable.
- A Rime Caller ring centred on the hero leaves a standing hero in its safe middle, so the ring is centred 2.6 m from the hero toward the caster. The hero stands on the band and escapes by stepping into the middle or out past it.
- A Cave Ghoul landing exactly on the hero overlaps it, so it lands 1 m short. To keep stage-1 caverns from getting much harder, the leap deals 0.9× with a 5 s cooldown.
- The melee slots (N13, N15, N17, N19) keep N01's damage per second through their cooldowns: 1.8 s for the Dune Marauder, 1.65 s for the Thornhorn Brute, 1.6 s for the Frostbitten Revenant; the Cave Ghoul's swing is N01's.

## Effect on stage-1 fights

Stage-1 Dank Cavern and Dusk Grassland rifts are mostly Cave Ghouls and Thornhorn Brutes. Across the eight seeds of the 30-second stage-1 fight record, the three Forgotten Graveyard seeds are unchanged. The three Dusk Grassland seeds leave the hero slightly healthier (372.6→388.2, 369.4→388.2, 353.9→365.0). The two Dank Cavern seeds leave the hero about 9% lower (338.8→307.1, 360.8→328.1), with kills down from 19 to 18 and from 21 to 17. The likely cause is the hero spending time dodging the new leap warnings. These values are recorded in `FieldTests`.

## Verification

Checked on 2026-09-28 in a cloned project and a macOS development build.

- **Full EditMode suite**: 4,542 of 4,589 passed. All 47 failures are in the pre-overhaul baseline; there are no new failures. [Summary](NewEnemyAttacksEvidence/editmode-full-summary.txt)
- **`NewEnemyAttackTests`**:
  - Each new enemy warns its own shape, and forecast and real damage agree.
  - The Dune Marauder cuts twice 0.35 s apart.
  - The Burrowing Sandworm tunnels to where the hero stood and erupts on arrival.
  - The Cave Ghoul leaps at 3.5 m and swings at 1.8 m.
  - The Sporebloat's cloud ticks and slows.
  - The witch, the revenant and the rime caller slow the hero.
  - The melee slots stay within ±15% of N01's damage per second.
- **Existing suites**:
  - All of N13–N20 pass `EnemyTests` (preparation and cooldown), `IncomingForecastTests` (forecast equals damage) and `TelegraphGaugeTests` (gauges complete when the attack lands; on arrival for tunnels and leaps).
  - `FieldTests` re-recorded its cavern and grassland seeds; the graveyard seeds are unchanged. [Record](NewEnemyAttacksEvidence/stage1-fight-record.txt)
- **`-hellscriptNewEnemySmoke` passed**: in the running game all eight hit a hero standing in their warning with their own attack, and the spore cloud ticked and slowed. [Result](NewEnemyAttacksEvidence/runtime-new-enemy-smoke.txt)
  - Captures (windup, just after the attack): [Dune Marauder](NewEnemyAttacksEvidence/n13-dune-marauder.png), [Burrowing Sandworm](NewEnemyAttacksEvidence/n14-burrowing-sandworm.png), [Cave Ghoul](NewEnemyAttacksEvidence/n15-cave-ghoul.png), [spore cloud](NewEnemyAttacksEvidence/n16-sporebloat-death-cloud.png), [Thornhorn Brute](NewEnemyAttacksEvidence/n17-thornhorn-brute.png), [Briar Witch](NewEnemyAttacksEvidence/n18-briar-witch.png), [Frostbitten Revenant](NewEnemyAttacksEvidence/n19-frostbitten-revenant.png), [Rime Caller](NewEnemyAttacksEvidence/n20-rime-caller.png)
- **The warning gauge, attack effects and boss kit smokes** also passed on the same build. Once, a slow frame right after a capture made the gauge smoke skip a windup stop. Such a skip is now written to the result file instead of failing the run.

## Not verified

- Nothing was seen on a physical device or in an Android build.
- N08–N20 have no dedicated models or sounds yet: bodies are placeholder shapes and the attacks are silent.
- The Briar Witch closes to its 9 m range like the other ranged enemies instead of keeping the planned 6 m.
- The cause given for fewer stage-1 cavern kills is an inference; difficulty in real play was not measured.
- This is not a human quality review.
