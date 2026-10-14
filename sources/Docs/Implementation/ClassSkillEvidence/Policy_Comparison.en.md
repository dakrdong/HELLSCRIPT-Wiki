# Skill-use policy experiments

Eighteen fights change only use policies within the same build. Warrior BUILD_SWB compares leaping from the current distance with deliberate spacing. Mage BUILD_REF_SM05 compares holding versus retreating and ending recovery at 50% versus 90% mana. All runs use level 40, rank 1 skills, item level 30, seed 9137 and 60 seconds. Equipment, ranks and enemies are fixed within each scenario. This Mage build does not carry Quiet Rift, so recovery ends at the mana goal; the optional 1.5s shield preparation with that item and a ward is tested separately.

[JSON](policy-experiments.json) · [Rules and tradeoffs](../../Design/Class_Skill_Use_Policies.en.md)

## Leap Slam

| Scenario | Choice | DPS | Incoming damage | Resource starvation | Spacing movement | Casts |
|---|---|---|---|---|---|---|
| single | Current distance | 221.76 | 92.87 | 43.10s | 0.00m | 1 |
| single | Create space | 223.99 | 68.23 | 35.80s | 41.78m | 7 |
| group | Current distance | 1223.11 | 758.38 | 42.70s | 0.00m | 0 |
| group | Create space | 1119.47 | 757.55 | 41.50s | 5.01m | 1 |
| boss | Current distance | 192.66 | 308.23 | 42.15s | 0.00m | 7 |
| boss | Create space | 206.77 | 248.60 | 35.70s | 32.59m | 7 |

## Mana Reclaim

| Scenario | Choice | DPS | Incoming damage | Resource starvation | Spacing movement | Casts | Charging time | Mana recovered |
|---|---|---|---|---|---|---|---|---|
| single | Hold / 50% | 389.31 | 94.56 | 27.35s | 0.00m | 14 | 3.65s | 500.78 |
| single | Hold / 90% | 401.37 | 97.26 | 14.60s | 0.00m | 7 | 4.10s | 562.52 |
| single | Retreat / 50% | 343.25 | 81.06 | 45.80s | 16.90m | 13 | 1.00s | 137.20 |
| single | Retreat / 90% | 346.47 | 75.66 | 33.55s | 17.46m | 10 | 1.85s | 253.82 |
| group | Hold / 50% | 3588.81 | 784.41 | 25.65s | 0.00m | 7 | 2.00s | 274.40 |
| group | Hold / 90% | 3586.30 | 765.06 | 11.90s | 0.00m | 4 | 2.25s | 308.70 |
| group | Retreat / 50% | 3304.02 | 676.41 | 45.25s | 16.73m | 7 | 0.40s | 54.88 |
| group | Retreat / 90% | 3304.02 | 676.41 | 45.25s | 16.73m | 7 | 0.40s | 54.88 |
| boss | Hold / 50% | 350.45 | 332.67 | 26.40s | 0.00m | 14 | 4.05s | 555.66 |
| boss | Hold / 90% | 342.15 | 333.46 | 13.70s | 0.00m | 8 | 4.60s | 631.12 |
| boss | Retreat / 50% | 261.71 | 293.30 | 43.10s | 32.82m | 13 | 0.90s | 123.48 |
| boss | Retreat / 90% | 263.02 | 303.54 | 41.60s | 28.43m | 12 | 1.05s | 144.06 |

## Interpretation

The character survived 18 of 18 fights. Leap spacing had higher DPS in: single, boss. It received less damage in: single, group, boss. Retreat suspends other attacks, so narrow paths, faster pursuit and short fights require separate comparisons.

Of six comparisons with equal recovery goals, holding had higher DPS in 6; retreating received less damage in 6. Against eight enemies, actual retreat-channel time was 0.40s for the 50% goal and 0.40s for the 90% goal.

For hold recovery, the 90% goal had higher DPS in: single; it received more damage in: single, boss.

For retreat recovery, the 90% goal had higher DPS in: single, boss; it received more damage in: boss.

These are controlled observations from the current combat code. Earlier values and rankings are not imposed on new observations, and this is not a universal optimum or final-balance certification. Spacing counts deliberate retreat; charging time counts actual channel time; recovered mana is the real gain after the cap. Per-skill waits can overlap. Total movement, damage, health and ultimate/other casts remain available in the JSON.
