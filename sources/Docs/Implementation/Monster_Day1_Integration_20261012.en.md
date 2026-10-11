# Monster Day 1: Registry, Data Combat and Hunyuan Models

Updated: 2026-10-12 · [한국어](Monster_Day1_Integration_20261012.md)

The foundation for adding the [monster roster per field](../Design/HELLSCRIPT_Monster_Roster_20261011.en.md) to the game 20 a day, and the day-1 result. The daily procedure is [adding 20 monsters a day](Monster_Daily_Pipeline.en.md); model conversion rules are in the [Hunyuan monster READY pipeline](Hunyuan_Monster_Ready_Pipeline.en.md).

## What landed

- **Monster registry** `Resources/Monsters/MonsterRegistry.json` and `MonsterRegistry`. The existing 25 (N01–N20, B01–B05) are described with their current kinds, patterns and numbers; new monsters fight from registry data. The Monster Codex reads this list (plan: [Monster Codex plan](Monster_Codex_Plan_20261011.en.md)).
- **Data combat**: attack parts (melee, projectiles, charge, leap, burrow, zones, support, summons), traits (auras, frontal reduction, retaliation, ambush, treasure and more), breakable structures (totems, banners, lanterns, cages, walls) and hero states (skill seal, curse, poison stacks, reduced vision). All 138 roster entries are written in this form with nothing deferred. Hunt Edict target types also come from the registry role.
- **Shipped**: N21, N22, N23, N33 and B06 are registered and spawn in rifts. They have no model or portrait yet (`artPending`) and show the primitive figure; the daily routine fills them from the day-2 generation.
- **20 Hunyuan models** (day 1): N01 N03 N04 N05 N06 N07 N09 N10 N11 N14 N15 N16 N17 N18 N19 N20, B01 B02 B04 B05, after automatic reduction (LOD 6,000/3,000/1,500, bosses 12,000/6,000/3,000), automatic rigging and a reviewed QA sheet. N08 was left out because its tunic lifted with the arm; it keeps its old model and will be regenerated with a new prompt.
- **19 Codex portraits** painted from the chosen model render, replacing the same IDs in `Resources/Art/TrainingGround/Portraits/`; records in `Docs/Art/Monsters/portrait-manifest.json`.
- **Daily routine tools** in `tools/monster_pipeline/` and the routine prompt `ROUTINE_PROMPT.md`.

## Fixed problems

- Five melee attacks (N21, N35, N42, N73, N85) started at a distance longer than their reach and always missed; their start distance now equals the reach. The registry tool rejects such data and `MonsterEngagementTests` checks that every data monster and boss of the full roster actually hits a standing hero.
- The skill text key `자폭` collided with the Hunt Edict target label (`EXPLODER`); it is now `자폭 돌진` (Self-Destruct Rush).

## Verification

| Check | Result |
| --- | --- |
| Full EditMode (merge candidate after merging main) | 5,949 tests: 5,945 passed, 0 failed, 4 skipped (54 min) |
| Related EditMode for the later changes (registry, combat, soak, field, server replay, READY, Hunt Edict targets + new `MonsterEngagementTests`) | 236 of 238 passed. The two new tests failed on their own setup (ambushers placed outside their wake radius, a boss threshold of two hits); after fixing the test `MonsterEngagementTests` passed 2/2 (the other 236 were not rerun because only the test file changed) |
| `tools/test_monster_registry.py`, `monster_registry.py validate` | 10/10, OK |
| `check_ui_contract.py --verify` | 15 OK |
| macOS development build runtime smoke `-hellscriptMonsterModelsSmoke` (1600×900) | Pass: 25/25 READY models shown as models; the four data monsters and boss B06 landed warned attacks (evidence: [boss-B01](MonsterModelsEvidence20261012/boss-B01.jpg) · [boss-B02](MonsterModelsEvidence20261012/boss-B02.jpg) · [boss-B03](MonsterModelsEvidence20261012/boss-B03.jpg) · [boss-B04](MonsterModelsEvidence20261012/boss-B04.jpg) · [boss-B05](MonsterModelsEvidence20261012/boss-B05.jpg) · [boss-B06](MonsterModelsEvidence20261012/boss-B06.jpg) · [fight-B06](MonsterModelsEvidence20261012/fight-B06.jpg) · [fight-N21](MonsterModelsEvidence20261012/fight-N21.jpg) · [fight-N22](MonsterModelsEvidence20261012/fight-N22.jpg) · [fight-N23](MonsterModelsEvidence20261012/fight-N23.jpg) · [fight-N33](MonsterModelsEvidence20261012/fight-N33.jpg) · [lineup-N01-N05](MonsterModelsEvidence20261012/lineup-N01-N05.jpg) · [lineup-N06-N10](MonsterModelsEvidence20261012/lineup-N06-N10.jpg) · [lineup-N11-N15](MonsterModelsEvidence20261012/lineup-N11-N15.jpg) · [lineup-N16-N20](MonsterModelsEvidence20261012/lineup-N16-N20.jpg) · [lineup-N21-N33](MonsterModelsEvidence20261012/lineup-N21-N33.jpg) · [runtime-monster-models-smoke](MonsterModelsEvidence20261012/runtime-monster-models-smoke.txt)) |

## Not verified

Physical devices (Android, iOS), APK and web build size (the HunyuanReady source PNGs in the repo are 197 MB; build size is after compression and must be measured separately), frame time and draw calls, the full runtime smoke bundle (only the new smoke related to this change ran), and public distribution approval of Hunyuan output (`productionApproved: false`).
