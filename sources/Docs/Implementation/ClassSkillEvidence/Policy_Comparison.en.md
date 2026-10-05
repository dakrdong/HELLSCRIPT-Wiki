# Skill-use policy experiments

Eighteen fights change only use policies within the same build. Warrior BUILD_SWB compares leaping from the current distance with deliberate spacing. Mage BUILD_REF_SM05 compares holding versus retreating and ending recovery at 50% versus 90% mana. All runs use level 40, rank 1 skills, item level 30, seed 9137 and 60 seconds. Equipment, ranks and enemies are fixed within each scenario. This Mage build does not carry Quiet Rift, so recovery ends at the mana goal; the optional 1.5s shield preparation with that item and a ward is tested separately.

[JSON](policy-experiments.json) · [Rules and tradeoffs](../../Design/Class_Skill_Use_Policies.en.md)

## Leap Slam

| Scenario | Choice | DPS | Incoming damage | Resource starvation | Spacing movement | Casts |
|---|---|---|---|---|---|---|
| single | Current distance | 221.76 | 92.87 | 43.10s | 0.00m | 1 |
| single | Create space | 227.29 | 73.45 | 33.80s | 40.94m | 7 |
| group | Current distance | 423.13 | 796.84 | 38.80s | 0.00m | 1 |
| group | Create space | 1290.06 | 785.30 | 41.55s | 7.50m | 1 |
| boss | Current distance | 221.32 | 282.98 | 39.20s | 0.00m | 7 |
| boss | Create space | 203.07 | 240.52 | 33.00s | 48.12m | 7 |

## Mana Reclaim

| Scenario | Choice | DPS | Incoming damage | Resource starvation | Spacing movement | Casts | Charging time | Mana recovered |
|---|---|---|---|---|---|---|---|---|
| single | Hold / 50% | 388.00 | 100.02 | 27.05s | 0.00m | 13 | 3.65s | 500.78 |
| single | Hold / 90% | 400.05 | 102.72 | 14.55s | 0.00m | 7 | 4.05s | 555.66 |
| single | Retreat / 50% | 316.01 | 64.92 | 33.95s | 29.84m | 11 | 1.25s | 171.50 |
| single | Retreat / 90% | 356.58 | 67.62 | 19.00s | 25.69m | 6 | 2.75s | 377.30 |
| group | Hold / 50% | 3571.86 | 795.54 | 25.60s | 0.00m | 5 | 1.50s | 205.80 |
| group | Hold / 90% | 3712.70 | 798.24 | 14.95s | 0.00m | 3 | 1.75s | 240.10 |
| group | Retreat / 50% | 3318.65 | 675.39 | 40.60s | 19.87m | 6 | 0.30s | 41.16 |
| group | Retreat / 90% | 3318.65 | 675.39 | 40.60s | 19.87m | 6 | 0.30s | 41.16 |
| boss | Hold / 50% | 349.57 | 337.66 | 26.50s | 0.00m | 14 | 4.00s | 548.80 |
| boss | Hold / 90% | 355.61 | 325.59 | 15.15s | 0.00m | 8 | 4.35s | 596.82 |
| boss | Retreat / 50% | 303.80 | 273.61 | 47.50s | 16.88m | 14 | 0.95s | 130.34 |
| boss | Retreat / 90% | 306.85 | 302.49 | 45.25s | 14.46m | 13 | 1.10s | 150.92 |

## Interpretation

The character survived 18 of 18 fights. Leap spacing had higher DPS in: single, group. It received less damage in: single, group, boss. Retreat suspends other attacks, so narrow paths, faster pursuit and short fights require separate comparisons.

Of six comparisons with equal recovery goals, holding had higher DPS in 6; retreating received less damage in 6. Against eight enemies, actual retreat-channel time was 0.30s for the 50% goal and 0.30s for the 90% goal.

For hold recovery, the 90% goal had higher DPS in: single, group, boss; it received more damage in: single, group.

For retreat recovery, the 90% goal had higher DPS in: single, boss; it received more damage in: single, boss.

These are controlled observations from the current combat code. Earlier values and rankings are not imposed on new observations, and this is not a universal optimum or final-balance certification. Spacing counts deliberate retreat; charging time counts actual channel time; recovered mana is the real gain after the cap. Per-skill waits can overlap. Total movement, damage, health and ultimate/other casts remain available in the JSON.
