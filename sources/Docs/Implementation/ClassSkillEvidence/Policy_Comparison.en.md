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
| boss | Current distance | 206.83 | 394.62 | 40.05s | 0.00m | 1 |
| boss | Create space | 210.61 | 311.36 | 35.05s | 43.28m | 7 |

## Mana Reclaim

| Scenario | Choice | DPS | Incoming damage | Resource starvation | Spacing movement | Casts | Charging time | Mana recovered |
|---|---|---|---|---|---|---|---|---|
| single | Hold / 50% | 388.00 | 100.02 | 27.05s | 0.00m | 13 | 3.65s | 500.78 |
| single | Hold / 90% | 400.05 | 102.72 | 14.55s | 0.00m | 7 | 4.05s | 555.66 |
| single | Retreat / 50% | 324.63 | 56.82 | 36.20s | 40.04m | 8 | 1.30s | 178.36 |
| single | Retreat / 90% | 335.23 | 64.92 | 27.45s | 31.25m | 4 | 1.90s | 260.68 |
| group | Hold / 50% | 3196.04 | 780.24 | 25.65s | 0.00m | 7 | 2.05s | 281.26 |
| group | Hold / 90% | 3201.02 | 794.94 | 14.45s | 0.00m | 4 | 2.30s | 315.56 |
| group | Retreat / 50% | 2970.86 | 638.82 | 32.40s | 29.17m | 2 | 0.40s | 54.88 |
| group | Retreat / 90% | 3030.81 | 679.32 | 23.15s | 15.88m | 2 | 0.90s | 123.48 |
| boss | Hold / 50% | 375.28 | 374.88 | 27.15s | 0.00m | 14 | 4.10s | 562.52 |
| boss | Hold / 90% | 381.77 | 383.28 | 16.65s | 0.00m | 8 | 4.60s | 631.12 |
| boss | Retreat / 50% | 318.02 | 369.95 | 45.30s | 19.84m | 1 | 0.15s | 20.58 |
| boss | Retreat / 90% | 332.23 | 384.30 | 41.90s | 13.12m | 1 | 0.45s | 61.74 |

## Interpretation

Every character survived. In the integrated build, leap spacing increased DPS in all three scenarios, only slightly against the boss. Incoming damage fell in all three scenarios. Retreat suspends other attacks, so narrow paths, faster pursuit and short fights remain useful conditions to compare.

Holding produced higher DPS than retreating with the same recovery goal. Retreating usually reduced incoming damage; against the boss with the 90% goal the two were about equal. Against eight enemies, retreating did begin charging: 0.40 seconds at the 50% goal and 0.90 seconds at the 90% goal. The earlier isolated-branch observation of zero charging time does not describe this integrated build.

The 90% recovery goal raised DPS in all three scenarios for both holding and retreating, but it also raised incoming damage in each of them, so it is not universally better. These are controlled observations, not a universal optimum or a final balance conclusion.

Spacing movement counts deliberate retreat only. Charging time counts actual channel time; recovered mana is the real increase after the resource cap. Per-skill waits may overlap. The JSON also preserves total movement, total damage, remaining health and ultimate/other skill cast counts.
