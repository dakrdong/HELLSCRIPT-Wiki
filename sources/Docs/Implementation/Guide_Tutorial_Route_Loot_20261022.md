# 가이드 튜토리얼 — 되돌아감·원거리 무리·보물 고블린 수업 재수정 (2026-10-22)

[English](Guide_Tutorial_Route_Loot_20261022.en.md). 사용자 테스트(main `c6354cbf`)의 보고 1(왔던 길을 되돌아감), 5(헬멧을 곧바로 먹고 골드를 한참 뒤에 먹음), 6(고블린 처치 2초 뒤 전리품 수업이 시작되지 않음), 7(원거리 몬스터가 따로따로 나옴)을 다룬다. 앞선 수정(2026-10-20·21, `RoadStart`·`GuideForward`·`LootLessonSeconds`)은 가설이었고 사용자 화면에서 다시 나타났다. 이번에는 코드를 바꾸기 전에 실제 플레이스루를 틱마다 기록하는 도구를 만들고, 그 기록으로 원인을 확정한 뒤 고쳤다.

## 결정적 재현 도구

- `GuideRouteTrace`(테스트): 실제 단계 플레이스루(`GuideHazardYardTests.PlayToTheHazardYard` → … → `GuideTutorialTests.PlayBossRoomToGraduation`)의 **모든 틱**에서 영웅 위치·단계·`guideWaypoint`·행동 문구·이동 사유 트리거·목표 ID·원거리 3마리 각각의 위치/체력/시야/행동 단계·무리 퍼짐·바닥 보상 수(열림/획득)·가장 가까운 보상 거리·보스 위치를 읽는다. 읽기 훅은 `GuideTutorialTests.Probe`이고 시뮬레이션은 바꾸지 않는다.
- 진행도(`Along`)는 길 전체의 최근접점이 아니라 **지금까지 도달한 지점 근처의 구간**만 보고 재서, 길을 되돌아가는 것이 짧은 점프로 보이지 않게 했다. 되돌아감 = 도달한 최대 진행도 − 현재 진행도.
- `HELLSCRIPT_GUIDE_TRACE_DIR`을 지정하면 두 테스트가 틱 기록을 CSV로 저장한다(`gallery.csv`, `whole-stage.csv`). 같은 시드(`GuideTutorial.Seed`)·0.05초 틱이라 결과가 매번 같다.

## 측정 (같은 플레이스루, 수정 전 → 수정 후)

| 항목 | 수정 전 (main `c6354cbf` 코드) | 수정 후 |
| --- | --- | --- |
| 갤러리에서 길을 되돌아간 최대 거리 | **15.9 m** (`Approach`, 위치 수업 직후) | 0.36 m |
| `Approach` 단계 시간 | 19.9 s | 3.5 s |
| 원거리 3마리 최대 상호 거리(위치 수업~전투) | **16.8 m** (한 마리만 걸어 나옴) | 2.7 m |
| 무리 중심과 영웅의 최소 거리(`FinalFight`) | 1.6 m | 1.7 m (기존 "무리 중심" 칙령 동작 그대로) |
| 보상이 떨어진 자리 (고블린이 영웅을 보스 방 밖으로 끌고 나간 경우) | 영웅 발밑 (가장 가까운 보상 0.0 m, 보상 무리 중심 0.0 m) | 영웅 뒤 (수업 시작 때 가장 가까운 보상 6.4 m) |
| 드롭 → 수업 시작 | +2.00 s | +2.00 s |
| 수업 전 영웅 이동 | 제자리 (보스와의 거리 11.9 → 8.5 m는 보스가 걸어온 것) | 보스 쪽으로 이동 (보스와의 거리 11.9 → 4.8 m) |
| `Pickup` 시작 → `LootDoneTalk` | **55.5 s**, 걸은 거리 161.8 m (마지막 금화 2개를 찾아 맵을 한 바퀴) | 6.8 s, 14.5 m |
| `FinalBoss` → 보스 처치 | 7.0 s | 5.2 s |

영상 대신 같은 CSV를 위에서 내려다본 **시뮬레이션 재생**을 만들었다([갤러리](GuideRouteEvidence20261022/gallery-replay.mp4), [보물 고블린→보상 회수](GuideRouteEvidence20261022/treasure-replay.mp4); 좌 수정 전, 우 수정 후, 시뮬레이션 시간 1배속, 화면에 시간 로그). 게임 화면 녹화가 아니다. 원자료는 `GuideRouteEvidence20261022/*.csv.gz`.

## 원인 (재현으로 확인한 것)

1. **갤러리 되돌아감·무리 분리(보고 1·7).** `Road`의 수업 시작 조건은 원거리 적이 12.5 m 이내인데 영웅의 인지 거리(`SenseRange`)는 12 m라서, 위치 수업이 끝난 `Approach` 첫 순간에 영웅에게 대상이 없었다. `Approach`에는 길 이동도 `GuideForward`도 없어 일반 탐색이 "미탐색 지점"을 길 뒤쪽(맵 시작 쪽)으로 잡았고 영웅은 약 16 m 되돌아갔다. 한편 갤러리 궁수 3마리는 각자 영웅이 14 m 안에 보일 때에야 깨어나서, 영웅을 따라 걸어 나온 한 마리만 17 m 떨어진 나머지와 따로 싸웠다. 두 증상은 같은 원인이다. 스폰 좌표(2.7 m 안 한 덩어리)나 개별 적 AI의 거리 유지가 원인이 아니다.
2. **전리품 수업이 시작되지 않음(보고 6).** `ReturnToBoss`가 `State.drops`(헬멧뿐)가 모두 획득/무시 상태이면 영원히 `false`를 돌려줬다. `All()`은 빈 목록에서도 참이라, 헬멧이 이미 인벤토리에 있어 떨어지지 않은 경우(`DropGuideGear`는 중복을 건너뛴다)·획득/무시된 경우 수업이 영영 시작되지 않는다. 단위 테스트로 재현(수정 전 코드에서 단계가 `ReturnToBoss`에 머묾). `Pickup`도 `drops.Count==0`이면 끝나지 못했다.
3. **보상이 발밑에 떨어짐·헬멧을 곧바로 먹음(보고 5 일부).** `GuideLootSpot`은 후보가 "보스에서 11 m 이내"여야 해서, 고블린이 영웅을 보스 방 밖으로 끌고 나가면(실제 17 m) 모든 후보가 탈락하고 영웅 위치를 그대로 썼다. 수업 후 `Pickup`이 열리는 순간 헬멧·금화가 발밑에서 즉시 회수됐다. 또 `ReturnToBoss`의 영웅은 보스 쪽으로 가지 않고 제자리에 있었다.
4. **마지막 금화를 한참 뒤에 먹음(보고 5 일부).** 일반 칙령에서 금화는 "지나가다"(`Passing`)라서 반경 1.5 m 밖에 흩어진 금화는 영웅이 가지 않는다. 영웅은 일반 탐색으로 맵을 돌다 우연히 지나가야 주웠다(재현: 55 s, 161.8 m). 보스가 영웅을 따라다니며 곁에 서 있으면 더 나쁘다: 수정 중 중간 실험에서 영웅이 1.1 m 앞의 헬멧을 줍지 않고 보스를 계속 때려 `Pickup`이 끝나지 않았다. 헬멧의 전리품 설정 값을 직접 읽지는 않았으나, 전투 중에는 줍지 않는 "전투 후" 설정이 원인으로 보인다(추정).
5. **보스가 고블린보다 먼저 죽을 수 있음.** 가이드 보스 보호가 없었다. 단위 테스트로 재현.

확인하지 못한 것: 사용자의 실제 플레이에서 5·6이 위 원인 중 어느 조합으로 일어났는지는 사용자 로그가 없어 단정하지 않는다. 위 원인들은 모두 재현했고 수정했다. 사용자 세이브·테스트본은 읽거나 바꾸지 않았다.

## 바뀐 것 (`CombatSimulation.Guide.cs` 중심, 게임 규칙 전체는 그대로)

- **원거리 무리 깨우기:** 위치 수업이 시작될 때(`Road` → `OpeningTalk`) 갤러리의 살아 있는 원거리 적 전원이 영웅을 알아본다(`lastSeen`; 위험 구역 적의 `ArriveGuideFoe`와 같은 연출). 이후 이동·공격은 일반 적 AI다. 영웅은 그대로 실제 `position.mode=CENTER` 동작으로 무리 중심에서 싸운다.
- **가이드 이동 목표(`GuideSeekGoal`):** `Approach`·`FinalFight`·`ReturnToBoss`·`FinalBoss`에서 대상이 없으면 일반 탐색 대신 가장 가까운 살아 있는 적(보스)으로 간다. `Pickup`에서 칙령의 회수 대상이 없으면 가장 가까운 남은 보상으로 간다. 회피·대상이 있을 때의 일반 전투·고블린 추격은 그대로다. 전역 AI 정지나 난이도 변경은 없다.
- **수업 시작 조건:** `ReturnToBoss` → `LootTalk`는 드롭 시점(`guideGoblinKilledAt`, `RewardGuideTreasure`가 보상을 놓을 때 기록; 새 저장 필드 없음)부터 시뮬레이션 시간 2초(`LootLessonSeconds`, 허용 오차 1e-4 s)에서 한 번만 열린다. 헬멧이 바닥에 있는지와 무관하다. 수업이 열릴 때까지 아무것도 주울 수 없다는 규칙(`Loot`의 `GuideTreasureScene` 보호)은 그대로라 "아직 바닥의 보상을 줍지 않았어요"는 항상 참이다. 한 틱은 정산을 두 번 하므로(이동 전·후) 수업은 시계가 2초에 닿는 틱에 열린다.
- **보상 위치:** 선호 위치(보스 방, 11 m 이내)가 없으면 보스 방 밖이어도 영웅 뒤 4 m를 쓴다. 발밑은 최후의 수단이다.
- **회수 수업(`GuideGathering`, 가이드 `Pickup` 단계 한정):** 고블린 보상은 헬멧·금화·강화석 모두 `Priority`로 취급하고 위험 대기를 건너뛴다. 일반 전리품 규칙(다른 단계·일반 균열)은 바꾸지 않는다. `Pickup`은 보상이 지급되었고 바닥에 남은 것이 없을 때 끝난다.
- **보스 보호:** 보스가 소개된 뒤(`MixedFight`)부터 `LootDoneTalk`까지 보스 체력이 1 아래로 내려가지 않는다(`GuideBossHeld`, `Deal`). `FinalBoss`부터는 죽을 수 있다. 기존 영웅 체력 하한과 같은 가이드 전용 보호다.

## 테스트

- `GuideHazardYardTests.TheGalleryPackWakesTogetherAndTheHeroNeverWalksBack`: 되돌아감 < 1.5 m, 갤러리에서 일반 탐색 이동 0틱, 무리 퍼짐 ≤ 4 m, 3마리 모두 공격 자세, 무리 중심 < 2.5 m, `Approach` < 10 s. 기존 `packBackedUp`·`packCentreClosest` 검사와 `ARoadEnteredHalfWayGoesOnFromWhereTheHeroStands`는 그대로다.
- `GuideHazardYardTests.TheTreasureLessonKeepsItsTimingAndTheRewardIsFetchedAtOnce`: 드롭+2초, 보스 쪽 이동 > 2.5 m, 수업 시작 때 보상이 반경 밖, 수업 전 획득 0, 회수 < 20 s·걸음 < 40 m, 보스는 `LootDoneTalk`까지 생존, 보스전 < 20 s.
- `GuideTreasureLessonTests`(보스 방 고정 상황): 보상이 발밑이 아님, 틱 간격 0.05/0.04/0.1/0.0125 s에서 +2초(너무 이르거나 늦지 않음), 일시정지·재개·중복 진입·중복 지급 없음, 헬멧 바닥/획득/무시/이미 소유일 때도 수업 시작, 보상 위에 서 있어도 수업 전에는 줍지 않음, 보스 쪽 이동, 보스 보호, 헬멧이 이미 있고 자원만 남았을 때 `Pickup` 종료.
- 수정 전 게임 코드에 새 테스트를 돌리면 16건 중 9건이 실패한다(되돌아감 15.9 m, 보상 발밑, 보스 조기 사망, 수업 미시작 3건과 `Pickup` 미종료, 보스 쪽 이동 없음, 수업 때 보상 옆).

실제 실행(Unity 6000.6.0f1 배치 EditMode, 개발 worktree의 일회용 RAM 세이브 `GameStore.Sandbox`): `GuideTutorialTests`·`GuideHazardYardTests`·`GuideTreasureLessonTests`·`GuideGearTests`·`GuidePotionLessonTests`·`GuideModifierLessonTests`·`ResourceLootTests`·`EdictLootIntegrationTests` 117건 통과·실패 0·건너뜀 0(253 s). `python3 tools/check_ui_contract.py --verify`(15건 OK)와 `python3 tools/check_ui_refresh.py`(PASS)를 돌렸다. 전체 Edit Mode·플레이어 빌드·가이드 스모크(ko 440×956 / en 956×440)는 실행하지 않았고, 실기기·실제 화면 검증도 하지 않았다. 문구 변경이 없어 `en.txt` 항목은 추가하지 않았다(기존 "다음 적을 찾아 이동"·"전리품 회수" 재사용).
