# Class skill build comparisons

Level 40, rank 1 skills, item level 30, eight legal equipment slots, seed 9137 and 60 seconds. Each build retains its equipment and ranks across one enemy, eight enemies and a boss. High enemy health keeps the observation period consistent. Different builds use their prescribed equipment, not identical item types. This is functional evidence, not a kill-speed or final-balance certification.

[JSON](build-matrix.json) · [Implementation](../Class_Skill_Runtime.en.md)

| Build | Single DPS | Group DPS | Boss DPS | Max. resource starvation | Scenarios survived | Ultimate casts |
|---|---|---|---|---|---|---|
| BUILD_REF_SA01 | 265.37 | 267.02 | 349.66 | 2.95s | 3/3 | 1 |
| BUILD_REF_SA02 | 41.83 | 51.47 | 37.34 | 0.00s | 3/3 | 1 |
| BUILD_REF_SA03 | 94.80 | 1351.14 | 320.17 | 49.40s | 3/3 | 1 |
| BUILD_REF_SA04 | 91.63 | 1239.88 | 272.29 | 48.50s | 3/3 | 1 |
| BUILD_REF_SA05 | 96.47 | 1186.78 | 285.74 | 45.35s | 3/3 | 1 |
| BUILD_REF_SA06 | 36.36 | 252.81 | 53.42 | 0.00s | 3/3 | 1 |
| BUILD_REF_SA07 | 112.55 | 977.45 | 234.25 | 51.15s | 3/3 | 1 |
| BUILD_REF_SA08 | 89.07 | 835.07 | 154.83 | 50.10s | 3/3 | 2 |
| BUILD_REF_SM01 | 42.19 | 137.26 | 49.06 | 0.00s | 3/3 | 2 |
| BUILD_REF_SM02 | 260.75 | 1997.53 | 227.58 | 47.50s | 3/3 | 1 |
| BUILD_REF_SM03 | 296.10 | 3143.79 | 269.51 | 39.20s | 3/3 | 2 |
| BUILD_REF_SM04 | 259.77 | 2041.85 | 224.94 | 38.95s | 3/3 | 1 |
| BUILD_REF_SM05 | 349.67 | 3298.93 | 297.22 | 48.90s | 3/3 | 2 |
| BUILD_REF_SM06 | 231.28 | 1061.04 | 186.01 | 0.10s | 3/3 | 2 |
| BUILD_REF_SM07 | 158.10 | 403.94 | 161.71 | 0.00s | 3/3 | 2 |
| BUILD_REF_SM08 | 228.55 | 1702.04 | 205.54 | 45.35s | 3/3 | 1 |
| BUILD_REF_SW01 | 98.74 | 245.19 | 31.32 | 0.00s | 3/3 | 2 |
| BUILD_REF_SW02 | 333.12 | 1995.52 | 242.12 | 51.55s | 3/3 | 2 |
| BUILD_REF_SW03 | 225.08 | 2167.03 | 166.05 | 40.65s | 3/3 | 0 |
| BUILD_REF_SW04 | 243.67 | 2660.47 | 233.88 | 49.55s | 3/3 | 0 |
| BUILD_REF_SW05 | 333.10 | 2512.28 | 262.63 | 52.55s | 3/3 | 2 |
| BUILD_REF_SW06 | 239.83 | 2271.22 | 200.59 | 50.85s | 3/3 | 2 |
| BUILD_REF_SW07 | 335.43 | 1644.09 | 206.42 | 44.60s | 3/3 | 0 |
| BUILD_REF_SW08 | 300.17 | 2240.21 | 260.88 | 37.05s | 3/3 | 3 |
| BUILD_SA | 32.64 | 397.94 | 110.95 | 39.55s | 3/3 | 2 |
| BUILD_SAB | 34.09 | 249.62 | 85.78 | 35.50s | 3/3 | 1 |
| BUILD_SM | 87.32 | 693.00 | 75.88 | 48.55s | 3/3 | 2 |
| BUILD_SMB | 48.53 | 132.95 | 48.11 | 0.00s | 3/3 | 2 |
| BUILD_SW | 184.99 | 1375.04 | 142.03 | 19.25s | 3/3 | 0 |
| BUILD_SWB | 221.76 | 1223.11 | 90.30 | 43.10s | 3/3 | 2 |

Resource starvation counts time when at least one equipped paid skill is unaffordable. Zero ultimate casts can mean its automatic condition was never satisfied; direct ultimate behavior and rejection of an unselected ultimate are verified separately. The JSON retains incoming damage, actual cast IDs/counts and secondary-hit counts.
