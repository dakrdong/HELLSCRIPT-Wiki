# Skill-use policy experiments

Eighteen fights change only use policies within the same build. Warrior BUILD_SWB compares leaping from the current distance with deliberate spacing. Mage BUILD_REF_SM05 compares holding versus retreating and ending recovery at 50% versus 90% mana. All runs use level 40, rank 1 skills, item level 30, seed 9137 and 60 seconds. Equipment, ranks and enemies are fixed within each scenario. This Mage build does not carry Quiet Rift, so recovery ends at the mana goal; the optional 1.5s shield preparation with that item and a ward is tested separately.

[JSON](policy-experiments.json) · [Rules and tradeoffs](../../Design/Class_Skill_Use_Policies.en.md)

## Leap Slam

| Scenario | Choice | DPS | Incoming damage | Resource starvation | Spacing movement | Casts |
|---|---|---|---|---|---|---|
| single | Current distance | 212.99 | 93.42 | 37.40s | 0.00m | 1 |
| single | Create space | 221.47 | 75.27 | 34.25s | 41.43m | 7 |
| group | Current distance | 400.41 | 802.61 | 41.50s | 0.00m | 1 |
| group | Create space | 1191.14 | 722.15 | 33.70s | 21.62m | 3 |
| boss | Current distance | 195.14 | 258.34 | 40.35s | 0.00m | 7 |
| boss | Create space | 189.65 | 281.81 | 32.50s | 51.49m | 7 |

## Mana Reclaim

| Scenario | Choice | DPS | Incoming damage | Resource starvation | Spacing movement | Casts | Charging time | Mana recovered |
|---|---|---|---|---|---|---|---|---|
| single | Hold / 50% | 388.00 | 100.02 | 27.05s | 0.00m | 13 | 3.65s | 500.78 |
| single | Hold / 90% | 400.05 | 102.72 | 14.55s | 0.00m | 7 | 4.05s | 555.66 |
| single | Retreat / 50% | 316.01 | 64.92 | 33.95s | 29.84m | 11 | 1.25s | 171.50 |
| single | Retreat / 90% | 356.58 | 67.62 | 19.00s | 25.69m | 6 | 2.75s | 377.30 |
| group | Hold / 50% | 3219.75 | 780.24 | 25.65s | 0.00m | 7 | 2.05s | 281.26 |
| group | Hold / 90% | 3224.74 | 794.94 | 14.45s | 0.00m | 4 | 2.30s | 315.56 |
| group | Retreat / 50% | 2969.14 | 606.66 | 38.70s | 29.25m | 5 | 0.45s | 61.74 |
| group | Retreat / 90% | 3137.00 | 661.38 | 24.95s | 21.57m | 3 | 1.00s | 137.20 |
| boss | Hold / 50% | 348.26 | 340.51 | 27.45s | 0.00m | 14 | 4.10s | 562.52 |
| boss | Hold / 90% | 347.09 | 328.82 | 15.00s | 0.00m | 8 | 4.55s | 624.26 |
| boss | Retreat / 50% | 328.54 | 285.39 | 43.95s | 21.73m | 13 | 1.10s | 150.92 |
| boss | Retreat / 90% | 315.63 | 310.97 | 31.40s | 26.91m | 9 | 2.10s | 288.12 |

## Interpretation

The character survived 18 of 18 fights. Leap spacing had higher DPS in: single, group. It received less damage in: single, group. Retreat suspends other attacks, so narrow paths, faster pursuit and short fights require separate comparisons.

Of six comparisons with equal recovery goals, holding had higher DPS in 6; retreating received less damage in 6. Against eight enemies, actual retreat-channel time was 0.45s for the 50% goal and 1.00s for the 90% goal.

For hold recovery, the 90% goal had higher DPS in: single, group; it received more damage in: single, group.

For retreat recovery, the 90% goal had higher DPS in: single, group; it received more damage in: single, group, boss.

These are controlled observations from the current combat code. Earlier values and rankings are not imposed on new observations, and this is not a universal optimum or final-balance certification. Spacing counts deliberate retreat; charging time counts actual channel time; recovered mana is the real gain after the cap. Per-skill waits can overlap. Total movement, damage, health and ultimate/other casts remain available in the JSON.
