# Class skill build comparisons

Level 40, rank 1 skills, item level 30, eight legal equipment slots, seed 9137 and 60 seconds. Each build retains its equipment and ranks across one enemy, eight enemies and a boss. High enemy health keeps the observation period consistent. Different builds use their prescribed equipment, not identical item types. This is functional evidence, not a kill-speed or final-balance certification.

[JSON](build-matrix.json) · [Implementation](../Class_Skill_Runtime.en.md)

| Build | Single DPS | Group DPS | Boss DPS | Max. resource starvation | Scenarios survived | Ultimate casts |
|---|---|---|---|---|---|---|
| BUILD_REF_SA01 | 258.86 | 262.17 | 395.63 | 3.20s | 3/3 | 1 |
| BUILD_REF_SA02 | 41.83 | 47.68 | 37.34 | 0.00s | 3/3 | 1 |
| BUILD_REF_SA03 | 93.61 | 1465.69 | 311.18 | 49.45s | 3/3 | 1 |
| BUILD_REF_SA04 | 90.68 | 1258.47 | 271.82 | 48.60s | 3/3 | 1 |
| BUILD_REF_SA05 | 95.10 | 1100.07 | 239.53 | 48.65s | 3/3 | 1 |
| BUILD_REF_SA06 | 34.83 | 255.14 | 53.80 | 0.00s | 3/3 | 1 |
| BUILD_REF_SA07 | 111.26 | 939.04 | 232.24 | 52.10s | 3/3 | 1 |
| BUILD_REF_SA08 | 89.07 | 840.95 | 162.50 | 49.55s | 3/3 | 1 |
| BUILD_REF_SM01 | 42.19 | 131.58 | 49.06 | 0.00s | 3/3 | 2 |
| BUILD_REF_SM02 | 260.71 | 1970.58 | 242.70 | 47.60s | 3/3 | 1 |
| BUILD_REF_SM03 | 294.45 | 2913.81 | 264.90 | 38.10s | 3/3 | 2 |
| BUILD_REF_SM04 | 257.69 | 2021.84 | 228.81 | 42.25s | 3/3 | 1 |
| BUILD_REF_SM05 | 344.90 | 2885.42 | 279.39 | 48.20s | 3/3 | 2 |
| BUILD_REF_SM06 | 225.59 | 1157.08 | 203.61 | 0.00s | 3/3 | 2 |
| BUILD_REF_SM07 | 156.43 | 430.70 | 161.98 | 0.00s | 3/3 | 2 |
| BUILD_REF_SM08 | 222.40 | 1660.87 | 198.33 | 47.55s | 3/3 | 1 |
| BUILD_REF_SW01 | 100.23 | 323.88 | 100.28 | 0.00s | 3/3 | 2 |
| BUILD_REF_SW02 | 315.29 | 1904.24 | 290.25 | 36.30s | 3/3 | 0 |
| BUILD_REF_SW03 | 209.43 | 2079.58 | 169.96 | 37.80s | 3/3 | 0 |
| BUILD_REF_SW04 | 234.20 | 2492.16 | 227.38 | 49.10s | 3/3 | 0 |
| BUILD_REF_SW05 | 318.34 | 2518.62 | 290.29 | 43.20s | 3/3 | 0 |
| BUILD_REF_SW06 | 235.89 | 2213.50 | 205.97 | 42.85s | 3/3 | 0 |
| BUILD_REF_SW07 | 335.43 | 1599.61 | 234.46 | 42.50s | 3/3 | 0 |
| BUILD_REF_SW08 | 292.79 | 2750.70 | 319.10 | 31.80s | 3/3 | 2 |
| BUILD_SA | 32.16 | 469.95 | 114.28 | 39.55s | 3/3 | 2 |
| BUILD_SAB | 34.09 | 291.97 | 89.32 | 38.15s | 3/3 | 1 |
| BUILD_SM | 86.87 | 631.08 | 81.37 | 48.55s | 3/3 | 2 |
| BUILD_SMB | 48.41 | 138.50 | 49.02 | 0.00s | 3/3 | 2 |
| BUILD_SW | 178.17 | 1250.30 | 156.67 | 13.75s | 3/3 | 0 |
| BUILD_SWB | 212.99 | 1158.16 | 155.22 | 43.25s | 3/3 | 0 |

Resource starvation counts time when at least one equipped paid skill is unaffordable. Zero ultimate casts can mean its automatic condition was never satisfied; direct ultimate behavior and rejection of an unselected ultimate are verified separately. The JSON retains incoming damage, actual cast IDs/counts and secondary-hit counts.
