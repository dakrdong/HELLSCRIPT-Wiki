# Class skill build comparisons

Level 40, rank 1 skills, item level 30, eight legal equipment slots, seed 9137 and 60 seconds. Each build retains its equipment and ranks across one enemy, eight enemies and a boss. High enemy health keeps the observation period consistent. Different builds use their prescribed equipment, not identical item types. This is functional evidence, not a kill-speed or final-balance certification.

[JSON](build-matrix.json) · [Implementation](../Class_Skill_Runtime.en.md)

| Build | Single DPS | Group DPS | Boss DPS | Max. resource starvation | Scenarios survived | Ultimate casts |
|---|---|---|---|---|---|---|
| BUILD_REF_SA01 | 258.86 | 262.17 | 438.12 | 3.15s | 3/3 | 1 |
| BUILD_REF_SA02 | 93.84 | 107.69 | 94.75 | 0.00s | 3/3 | 1 |
| BUILD_REF_SA03 | 93.61 | 1465.69 | 338.89 | 49.60s | 3/3 | 1 |
| BUILD_REF_SA04 | 90.68 | 1258.47 | 284.58 | 49.30s | 3/3 | 1 |
| BUILD_REF_SA05 | 95.10 | 1100.07 | 261.86 | 49.60s | 3/3 | 1 |
| BUILD_REF_SA06 | 100.34 | 607.75 | 151.82 | 0.00s | 3/3 | 1 |
| BUILD_REF_SA07 | 111.26 | 939.04 | 231.38 | 51.85s | 3/3 | 1 |
| BUILD_REF_SA08 | 89.07 | 840.95 | 175.95 | 50.10s | 3/3 | 1 |
| BUILD_REF_SM01 | 42.66 | 133.17 | 60.46 | 0.00s | 3/3 | 2 |
| BUILD_REF_SM02 | 260.71 | 1970.58 | 275.01 | 47.60s | 3/3 | 1 |
| BUILD_REF_SM03 | 294.45 | 2803.16 | 274.87 | 38.10s | 3/3 | 2 |
| BUILD_REF_SM04 | 257.69 | 2021.84 | 245.79 | 42.25s | 3/3 | 1 |
| BUILD_REF_SM05 | 344.90 | 2873.56 | 329.14 | 48.60s | 3/3 | 2 |
| BUILD_REF_SM06 | 205.95 | 1148.65 | 205.23 | 0.00s | 3/3 | 2 |
| BUILD_REF_SM07 | 179.98 | 532.85 | 205.59 | 0.00s | 3/3 | 2 |
| BUILD_REF_SM08 | 222.40 | 1660.87 | 217.72 | 47.55s | 3/3 | 1 |
| BUILD_REF_SW01 | 100.23 | 323.88 | 133.10 | 0.00s | 3/3 | 2 |
| BUILD_REF_SW02 | 315.29 | 1744.14 | 304.25 | 37.05s | 3/3 | 0 |
| BUILD_REF_SW03 | 216.58 | 2079.58 | 203.90 | 30.75s | 3/3 | 0 |
| BUILD_REF_SW04 | 234.20 | 2492.16 | 225.35 | 43.75s | 3/3 | 0 |
| BUILD_REF_SW05 | 318.34 | 2518.62 | 320.25 | 41.10s | 3/3 | 0 |
| BUILD_REF_SW06 | 235.89 | 2213.50 | 218.09 | 41.25s | 3/3 | 0 |
| BUILD_REF_SW07 | 335.43 | 1599.61 | 301.91 | 39.40s | 3/3 | 0 |
| BUILD_REF_SW08 | 298.32 | 2739.70 | 311.29 | 27.85s | 3/3 | 2 |
| BUILD_SA | 95.25 | 1148.81 | 315.75 | 44.20s | 3/3 | 2 |
| BUILD_SAB | 98.54 | 690.99 | 236.51 | 40.55s | 3/3 | 1 |
| BUILD_SM | 86.15 | 628.90 | 86.17 | 48.55s | 3/3 | 2 |
| BUILD_SMB | 47.48 | 137.11 | 49.71 | 0.00s | 3/3 | 2 |
| BUILD_SW | 178.17 | 1250.30 | 169.01 | 10.35s | 3/3 | 0 |
| BUILD_SWB | 212.99 | 1158.16 | 195.20 | 41.90s | 3/3 | 0 |

Resource starvation counts time when at least one equipped paid skill is unaffordable. Zero ultimate casts can mean its automatic condition was never satisfied; direct ultimate behavior and rejection of an unselected ultimate are verified separately. The JSON retains incoming damage, actual cast IDs/counts and secondary-hit counts.
