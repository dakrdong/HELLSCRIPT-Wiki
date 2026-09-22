# Class skill build comparisons

Level 40, rank 1 skills, item level 30, eight legal equipment slots, seed 9137 and 60 seconds. Each build retains its equipment and ranks across one enemy, eight enemies and a boss. High enemy health keeps the observation period consistent. Different builds use their prescribed equipment, not identical item types. This is functional evidence, not a kill-speed or final-balance certification.

[JSON](build-matrix.json) · [Implementation](../Class_Skill_Runtime.en.md)

| Build | Single DPS | Group DPS | Boss DPS | Max. resource starvation | Scenarios survived | Ultimate casts |
|---|---|---|---|---|---|---|
| BUILD_REF_SA01 | 258.86 | 262.17 | 431.12 | 3.05s | 3/3 | 1 |
| BUILD_REF_SA02 | 93.84 | 107.69 | 95.85 | 0.00s | 3/3 | 1 |
| BUILD_REF_SA03 | 93.61 | 1465.69 | 340.09 | 48.95s | 3/3 | 1 |
| BUILD_REF_SA04 | 90.68 | 1258.47 | 286.05 | 50.25s | 3/3 | 1 |
| BUILD_REF_SA05 | 95.10 | 1100.07 | 254.56 | 48.90s | 3/3 | 1 |
| BUILD_REF_SA06 | 100.34 | 607.75 | 155.33 | 0.00s | 3/3 | 1 |
| BUILD_REF_SA07 | 111.26 | 939.04 | 227.52 | 50.95s | 3/3 | 1 |
| BUILD_REF_SA08 | 89.07 | 847.54 | 166.31 | 51.45s | 3/3 | 1 |
| BUILD_REF_SM01 | 42.66 | 133.17 | 59.84 | 0.00s | 3/3 | 2 |
| BUILD_REF_SM02 | 260.71 | 1970.58 | 266.32 | 47.60s | 3/3 | 1 |
| BUILD_REF_SM03 | 294.45 | 2803.16 | 275.54 | 38.40s | 3/3 | 2 |
| BUILD_REF_SM04 | 257.69 | 2021.84 | 245.39 | 42.25s | 3/3 | 1 |
| BUILD_REF_SM05 | 344.90 | 2873.56 | 332.57 | 47.90s | 3/3 | 2 |
| BUILD_REF_SM06 | 205.95 | 1148.65 | 202.81 | 0.00s | 3/3 | 2 |
| BUILD_REF_SM07 | 179.98 | 532.85 | 207.16 | 0.00s | 3/3 | 2 |
| BUILD_REF_SM08 | 222.40 | 1660.87 | 223.80 | 47.55s | 3/3 | 1 |
| BUILD_REF_SW01 | 100.23 | 323.88 | 129.78 | 0.00s | 3/3 | 2 |
| BUILD_REF_SW02 | 315.29 | 1744.14 | 307.52 | 37.05s | 3/3 | 0 |
| BUILD_REF_SW03 | 216.58 | 2079.58 | 206.85 | 28.35s | 3/3 | 0 |
| BUILD_REF_SW04 | 234.20 | 2492.16 | 241.65 | 43.70s | 3/3 | 0 |
| BUILD_REF_SW05 | 318.34 | 2518.62 | 321.53 | 41.10s | 3/3 | 0 |
| BUILD_REF_SW06 | 235.89 | 2213.50 | 221.43 | 40.90s | 3/3 | 0 |
| BUILD_REF_SW07 | 335.43 | 1599.61 | 310.65 | 39.40s | 3/3 | 0 |
| BUILD_REF_SW08 | 298.32 | 2739.70 | 327.10 | 28.55s | 3/3 | 2 |
| BUILD_SA | 95.25 | 1148.81 | 319.84 | 44.20s | 3/3 | 2 |
| BUILD_SAB | 98.54 | 690.99 | 245.72 | 40.55s | 3/3 | 1 |
| BUILD_SM | 86.15 | 634.50 | 89.36 | 49.05s | 3/3 | 2 |
| BUILD_SMB | 47.48 | 137.11 | 48.99 | 0.00s | 3/3 | 2 |
| BUILD_SW | 178.17 | 1250.30 | 169.01 | 10.35s | 3/3 | 0 |
| BUILD_SWB | 212.99 | 1158.16 | 199.70 | 41.25s | 3/3 | 0 |

Resource starvation counts time when at least one equipped paid skill is unaffordable. Zero ultimate casts can mean its automatic condition was never satisfied; direct ultimate behavior and rejection of an unselected ultimate are verified separately. The JSON retains incoming damage, actual cast IDs/counts and secondary-hit counts.
