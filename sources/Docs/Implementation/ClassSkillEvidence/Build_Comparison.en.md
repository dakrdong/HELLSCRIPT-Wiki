# Class skill build comparisons

Level 40, rank 1 skills, item level 30, eight legal equipment slots, seed 9137 and 60 seconds. Each build retains its equipment and ranks across one enemy, eight enemies and a boss. High enemy health keeps the observation period consistent. Different builds use their prescribed equipment, not identical item types. This is functional evidence, not a kill-speed or final-balance certification.

[JSON](build-matrix.json) · [Implementation](../Class_Skill_Runtime.en.md)

| Build | Single DPS | Group DPS | Boss DPS | Max. resource starvation | Scenarios survived | Ultimate casts |
|---|---|---|---|---|---|---|
| BUILD_REF_SA01 | 266.19 | 265.16 | 414.11 | 3.15s | 3/3 | 1 |
| BUILD_REF_SA02 | 41.83 | 47.68 | 37.34 | 0.00s | 3/3 | 1 |
| BUILD_REF_SA03 | 93.61 | 1465.69 | 323.41 | 49.45s | 3/3 | 1 |
| BUILD_REF_SA04 | 90.68 | 1258.47 | 271.82 | 48.60s | 3/3 | 1 |
| BUILD_REF_SA05 | 95.10 | 1262.58 | 259.10 | 46.70s | 3/3 | 1 |
| BUILD_REF_SA06 | 34.83 | 254.49 | 54.02 | 0.00s | 3/3 | 1 |
| BUILD_REF_SA07 | 112.55 | 962.98 | 237.15 | 52.10s | 3/3 | 1 |
| BUILD_REF_SA08 | 89.07 | 840.95 | 162.50 | 49.55s | 3/3 | 1 |
| BUILD_REF_SM01 | 42.19 | 131.58 | 49.06 | 0.00s | 3/3 | 2 |
| BUILD_REF_SM02 | 260.71 | 1970.58 | 242.70 | 47.60s | 3/3 | 1 |
| BUILD_REF_SM03 | 301.00 | 3187.25 | 268.73 | 39.45s | 3/3 | 2 |
| BUILD_REF_SM04 | 257.69 | 2021.84 | 228.81 | 42.25s | 3/3 | 1 |
| BUILD_REF_SM05 | 344.90 | 3282.71 | 285.26 | 48.25s | 3/3 | 2 |
| BUILD_REF_SM06 | 229.44 | 1158.36 | 201.40 | 0.00s | 3/3 | 2 |
| BUILD_REF_SM07 | 158.91 | 441.78 | 166.32 | 0.00s | 3/3 | 2 |
| BUILD_REF_SM08 | 222.40 | 1660.87 | 198.33 | 47.55s | 3/3 | 1 |
| BUILD_REF_SW01 | 100.23 | 329.12 | 100.28 | 0.00s | 3/3 | 2 |
| BUILD_REF_SW02 | 331.51 | 2071.31 | 284.30 | 37.20s | 3/3 | 0 |
| BUILD_REF_SW03 | 224.39 | 2079.36 | 184.62 | 32.70s | 3/3 | 0 |
| BUILD_REF_SW04 | 243.67 | 2670.92 | 238.32 | 46.20s | 3/3 | 0 |
| BUILD_REF_SW05 | 333.10 | 2643.94 | 301.42 | 43.20s | 3/3 | 0 |
| BUILD_REF_SW06 | 239.83 | 2213.50 | 205.97 | 41.45s | 3/3 | 0 |
| BUILD_REF_SW07 | 335.43 | 1599.61 | 234.46 | 42.50s | 3/3 | 0 |
| BUILD_REF_SW08 | 300.17 | 2824.42 | 327.71 | 31.80s | 3/3 | 2 |
| BUILD_SA | 32.16 | 467.73 | 109.28 | 39.55s | 3/3 | 2 |
| BUILD_SAB | 33.60 | 291.97 | 91.93 | 38.15s | 3/3 | 1 |
| BUILD_SM | 86.87 | 686.92 | 82.33 | 48.55s | 3/3 | 2 |
| BUILD_SMB | 48.41 | 138.50 | 49.02 | 0.00s | 3/3 | 2 |
| BUILD_SW | 184.99 | 1378.93 | 175.58 | 9.85s | 3/3 | 0 |
| BUILD_SWB | 221.76 | 1276.26 | 153.96 | 44.35s | 3/3 | 0 |

Resource starvation counts time when at least one equipped paid skill is unaffordable. Zero ultimate casts can mean its automatic condition was never satisfied; direct ultimate behavior and rejection of an unselected ultimate are verified separately. The JSON retains incoming damage, actual cast IDs/counts and secondary-hit counts.
