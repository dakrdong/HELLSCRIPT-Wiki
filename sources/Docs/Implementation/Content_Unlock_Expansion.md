# 콘텐츠 공개·해금 구현 및 검증 기록

작성일: 2026-09-12. HELLSCRIPT 테스트 초안 v0.1. 외부 게임의 확정 수치가 아니다.

## 실제 반영 범위

현재 Work 프로젝트 `/Users/t8g-2410-pn-005/Documents/MyProject/HELLSCRIPT/HELLSCRIPT`의 코드·기존 GDD·기존 엑셀을 직접 수정했다. 첨부 상세 명세는 `Docs/Design/HELLSCRIPT_Content_Unlock_Spec_v0.1.md`에 보존했다. 참고 GDD로 현재 GDD를 교체하지 않았고 새 절과 영어 요약만 병합했다. 엑셀은 기존 25개 시트와 23개 표, 3,397개 기존 셀의 값·수식, 서식·병합·필터·검증·행열 크기를 보존하고 `25_콘텐츠해금`, `26_해금검증`을 추가했다.

공통 원본은 `Assets/HELLSCRIPT/Resources/ContentUnlocks.json`이며 실행 시 판정과 화면 예고가 이 데이터를 읽는다. 엑셀의 조건표도 같은 JSON을 읽어 내보냈다. 엑셀을 수정한다고 실행 데이터가 자동으로 바뀌지는 않는다. 변경 시 JSON을 먼저 수정하고 엑셀을 다시 내보내 대조한다. 내보내기 작업 파일과 결과 대조는 `Artifacts/Validation/ContentUnlock/Workbook/`에 보존한다.

## 적용 조건

| 기능 | 계정 공통 조건 |
|---|---|
| 훈련장 | 첫 실제 시도 종료, 실패 포함 |
| 장비 강화 | A ≥ 1 |
| 오프라인 보상 | A ≥ 1 |
| 부위 선택 기본 제작 | A ≥ 3 |
| 보석 장착·교체·합성 | A ≥ 5 / 또는 첫 보석 정상 획득 |
| 옵션 재설정 | A ≥ 8 |
| 미확인 장비 상점 | A ≥ 10 |
| 소탕 | A ≥ 12 |
| 전설 코어 제작 | A ≥ 15 / 또는 같은 부위 코어 10개 달성 |

A는 계정 내 영웅 최고 실제 클리어의 최댓값이다. 보스 보상이 확정되는 `BossClear` 이후 재평가하며 입장은 기록을 올리지 않는다. H는 선택 영웅의 개인 최고 클리어이며 소탕은 H≥1이 필요하고 H의 보상·등급 확률을 사용한다. R은 실행에 저장된 단계이며 현재 전투의 생성·보상에 사용한다. P는 영웅 레벨이다.

신규 패시브 슬롯은 P=3/6/10에서 1/2/3개다. 액티브의 기존 1/3/6/10/15/20레벨 조건과 최대 4개 장착은 유지했다. 3직업·상세 스크립트·프리셋·분해·판매·보호·기존 창고·획득 필터·포탈·재도전은 초기 권한을 유지한다. 미구매 탭을 개방하지 않는다. 전설·세트 착용에 별도 단계 제한을 추가하지 않았다.

| 현재 실행 R | 실제 생성 규칙 |
|---|---|
| 1~5 | 테마 0 ‘잊힌 묘지’, 기본 근접 적 N01, 첫 보스 BOSS01. 기존 정예·방·처치·보상 예산 유지 |
| 6~10 | 같은 테마의 기존 적 역할 N01~N06 조합 확대 |
| 11 이상 | 테마 1 ‘붕괴한 성채’ 및 해당 적 N07~N12, BOSS02를 기존 후보에 추가 |
| 20 이상 | 정예 최대 2특성. 기존 금지 조합과 짝 정예 규칙 유지 |
| 21 이상 | BOSS03 추가. 이전 보스·테마 후보와 2특성 규칙 유지 |

직전 보스 제외 규칙은 후보가 둘 이상이면 유지한다. 보스가 하나뿐인 초반에는 BOSS01을 반복한다. 방·보스 공간의 기존 검증과 생성 실패 대체 경로는 유지했다. 개발용 강제 테마·배치 인자는 기존 검증용으로 남겼다. 기존 진행 중인 균열은 다시 생성하지 않는다. 20단 이후 새 성장 메뉴를 추가하지 않는다.

## 저장·조기 해금·실행 권한

`AccountSave.contentUnlocks`에 버전, 영구 권한 목록, 안내 완료 목록, 획득·시도 이력, 오프라인 활성 시각을 별도로 저장한다. 본 저장의 schema 1/2 이관 규칙은 유지하고 해금 상태의 version=0을 한 번 이관한다. 이전 버전에서 누구나 이용 가능했던 훈련·강화·기본 제작·재설정·상점·소탕·코어 제작과 패시브 3개 슬롯을 보존한다. 기존 오프라인 이용자는 기존 기준 시각을 유지하고, 새로 열린 오프라인 보상은 해금 이전 시간을 지급하지 않는다. 기존 보석 구현·권한은 없었으므로 임의로 부여하지 않는다.

부위별 코어 중 하나가 10개가 되면 해금 이력을 저장한다. 서로 다른 부위는 합산하지 않고 해금 자체로 소비하지 않는다. 실제 제작 때 한 번 차감하며 이후 0개여도 권한은 유지한다. 전설 분해는 잠금과 무관하게 기존 코어를 지급한다. 보석은 정상 획득 경로에 연결할 수 있는 이력 훅과 권한·저장을 구현했지만, 기존 프로젝트에 보석 아이템·획득·소비 데이터가 없어 실제 보석 획득 경로 연결은 남았다.

실제 강화·재설정·제작·구매·소탕·훈련 시작에서 권한을 검사한다. 상점의 구매 로직을 Core 서비스로 옮기고 가격·재료·소유 영웅·가방·포탈 상태를 다시 계산한다. UI 값으로 비용을 전달해 우회하지 않는다. 포탈 정리 중 강화·장비 교체 금지 경로는 보존했다. NPC 전체는 잠그지 않는다.

안내는 요약 목록에서 선택해 다시 열 수 있으며 연속 강제 팝업이 없다. 확인·건너뛰기는 권한을 제거하지 않는다. 첫 전설·세트 획득 이력과 도감 안내도 별도로 저장한다. 기존 첫 전투·실패 로그 안내는 유지했다.

`MergeVerified`는 이미 인증·검증된 계정 권한을 합집합으로 받아들이는 연결 경계다. 실제 계정 서버·클라우드·다른 기기 동기화는 아직 없으며, 새 동기화 시스템을 구현했다고 보고하지 않는다. 클라이언트 병합 시 오프라인 시각을 과거로 당기지 않는다. 기존 로컬 보상 플래그·거래 영수증을 유지한다.

## 기존 작업 보존

작업 시작 당시의 소스·문서·설정 712개를 별도 보존하고 그 이후 변경분을 비교했다. `RiftRarity.cs`, `RiftRarityBalance.cs`는 시작 시점과 SHA256이 같다. 기존 확률 시트도 모든 셀과 서식이 같다. `Economy.RerollGold`는 동일하며 `500×L+50×L×(L−1)`, L=max(1,아이템 레벨)이다. L=8의 1·10·100번째 시도 모두 6,800골드를 실제 차감하는 검사가 통과했다. 기존 상점 2% 전설/38% 희귀/60% 마법, 제작 비용과 코어 10개 조건을 유지했다.

기존 던전·칙령·관문·목표 변경을 되돌리지 않았다. `ProjectSettings.asset`, `UnityConnectSettings.asset`도 작업 시작 시점의 변경된 파일과 동일하다. Unity 검사는 별도 복제 프로젝트에서 수행했다. 작업 중 다른 작업이 수정한 `Localization.cs` 및 `RuntimeObjectiveChainSmoke.cs`도 보존했으며 이번 수정 파일 목록에는 포함하지 않았다. 최종 개발 빌드의 모든 HELLSCRIPT C#·JSON·문구 표는 현재 프로젝트와 해시가 일치한다. 기존 파일 삭제, 패키지 변경, 씬·프리팹 변경, 초기 구현 검증 단계에서는 원격 게시·푸시를 수행하지 않았다. 후속 커밋 검증은 아래 절에 구분한다.

## 실행 검증과 남은 한계

- Unity 6000.6.0f1 / Test Framework 1.8.0의 최종 전체 Edit Mode 검사 **1362개 통과, 실패 0개**. 해금 전용 37개를 포함한다. 별도 집중 검사 165개도 통과했다. 기존 훈련 기반 테스트는 실제 기능 잠금을 제거하는 대신 첫 시도 종료 이력을 준비 계정에 명시했다.
- 최종 macOS 개발 빌드 성공, 오류 0개. 한국어·영어와 720×1280·1280×720에서 실제 앱 화면 검사 통과. 잠긴 상점 기능, 첫 실패 후 훈련, 계정 공통 권한, H=0 소탕 거절, 안내 건너뛰기·다시 보기, 보석 의존 화면을 검사했다. 7개 화면을 저장하고 확인했다. 마지막 UI 배치 정리는 전체 검사 뒤 최종 빌드와 UI 스모크로 다시 검증했다.
- 기존 등급 곡선·상한·추첨·소탕 검사를 실행했다. 이것은 자동 난수·전투 코드 검사이며 사람의 장기 파밍 통계가 아니다.
- 엑셀은 Artifact 계산 엔진의 수식 오류 0개, 조건 9행과 패시브·전투 경계 대조, 경제 계산 및 렌더 검사를 완료했다. Microsoft Excel 앱 자체 재계산은 실행하지 않았다.

| 3직업 신규 1단 자동 실행 | 결과 | 경과 시간 | 처치 / 종료 레벨 |
|---|---|---|---|
| 전사 | 실패 | 80.45초 | 45 / Lv.3 |
| 궁수 | 실패 | 300.00초 | 80 / Lv.5 |
| 마법사 | 실패 | 42.15초 | 24 / Lv.3 |

공통 seed=112358이며 실제 시뮬레이션 Tick으로 종료까지 실행했다. 경로 오류는 없었다. **세 직업 모두 이 기본 설정의 첫 시도에서 실패했으므로 초반 난도·기본 설정·패턴 공정성의 플레이 검증이 끝났다고 볼 수 없다.** 사람의 조작·설정 변경을 포함한 플레이테스트, Android/iOS 실기기, 실제 다른 계정·기기의 동기화와 충돌 테스트는 미실행이다. 실패를 감추기 위해 무료 재화·능력치·스킬을 추가하지 않았다.

첫 강화는 레벨 1 장비 기준 200골드·재료 20개다. 1단 보스+초회 보상은 1,950골드·재료 15개이므로 재료 5개를 상자·분해 등 기존 경로로 추가 확보해야 한다. 3단 기본 제작은 1,500골드·재료 50개이고, 이전 소비에 따라 즉시 사용이 보장되지 않는다. 8레벨 아이템 재설정은 6,800골드와 유효 접사가 필요하다. 15단 메뉴 공개는 같은 부위 코어 10개 보유를 보장하지 않는다. 보석·소켓·레시피 및 결과 데이터는 미구현 상태를 메뉴에 표시했다.

### U01–U30 결과

| ID | 실행 범위와 결과 | 증거·제한 |
|---|---|---|
| U01 | 조건·자동 UI 통과 | 신규 3영웅·기본 장비·스크립트 유지, 성장 메뉴 잠금. macOS 신규 계정 화면 확인. |
| U02 | 조건·자동 UI 통과 | 실제 균열 생성 후 포기/실패 처리. A=0, 훈련장만 개방. 자동 앱 흐름 확인. |
| U03 | 조건·보상 경로 통과 | 공개 Tick으로 보스 보상 확정·해금. 해금 전 오프라인 시간 소급 없음. 재접속 중복 없음. |
| U04 | 조건 단위 검사 통과 | ContentUnlockTests / final-focused.xml. 계정·영웅·실행 단계를 분리해 검사했습니다. |
| U05 | 권한 검사 통과 / 이용 미구현 | 보석·소켓·합성 데이터가 없어 실제 장착·교체·합성은 미실행. 의존 조건을 메뉴에 표시함. |
| U06 | 획득 훅·저장 검사 통과 | 정상 획득 연결용 이벤트와 영구 저장을 검사함. 실제 보석 획득·소비 경로는 기존 프로젝트에 없음. |
| U07 | 조건·경제 계산 통과 | L=8에서 1·10·100번째 재설정은 모두 6,800골드. 기존 RerollGold와 등급 확률 코드 보존. |
| U08 | 조건·자동 UI 통과 | 10단 기준과 개별 구매 잠금 확인. 기존 상점 2/38/60% 및 가격을 서비스 함수로 옮겨 유지함. |
| U09 | 조건 단위 검사 통과 | ContentUnlockTests / final-focused.xml. 계정·영웅·실행 단계를 분리해 검사했습니다. |
| U10 | 조건·자동 UI 통과 | 계정 메뉴가 열려도 선택 영웅 H=0이면 실제 요청 거절. 난수·보상 불변. |
| U11 | 보상 난수 대조 통과 | 전사 H=50/궁수 H=3. 궁수의 3단 보스 확률·레벨·난수 소비와 950골드 대조. |
| U12 | 조건 단위 검사 통과 | ContentUnlockTests / final-focused.xml. 계정·영웅·실행 단계를 분리해 검사했습니다. |
| U13 | 조건 단위 검사 통과 | ContentUnlockTests / final-focused.xml. 계정·영웅·실행 단계를 분리해 검사했습니다. |
| U14 | 조건 단위 검사 통과 | ContentUnlockTests / final-focused.xml. 계정·영웅·실행 단계를 분리해 검사했습니다. |
| U15 | 조건 단위 검사 통과 | ContentUnlockTests / final-focused.xml. 계정·영웅·실행 단계를 분리해 검사했습니다. |
| U16 | 장착·분해·기존 검사 통과 | 전설 장착 조건과 잠금 전 코어 지급 확인. 기존 세트·장비 검사 유지. 획득 안내는 권한과 별도 기록. |
| U17 | 조건·자동 UI 통과 | 계정 권한을 유지한 채 다른 영웅으로 전환. 개인 H와 패시브 슬롯은 공유하지 않음. |
| U18 | 조건 단위 검사 통과 | ContentUnlockTests / final-focused.xml. 계정·영웅·실행 단계를 분리해 검사했습니다. |
| U19 | 마이그레이션·저장 검사 통과 | 구버전에서 모두 사용 가능했던 기능과 3개 패시브 슬롯 보존. 보석·미구매 권한 추가 없음. |
| U20 | 생성·기존 보상 검사 통과 | R=5의 테마/보스/적 후보를 검사하고 최고 기록과 실제 보상 단계 분리 회귀 검사를 실행함. |
| U21 | 경계·난수 생성 검사 통과 | 5/10/11/19/20/21단 각 6시드. 이전 보스 후보와 금지 정예 조합 검증 유지. |
| U22 | 저장·자동 UI 통과 | 안내 건너뛰기 후 권한 유지. 요약 목록에서 개별 안내를 다시 열 수 있음. |
| U23 | 로컬 재실행 검사 통과 | 보스 보상 확정 후 저장·복원·Tick에서 재지급 없음. 실제 서버 이벤트 재전송은 미연결. |
| U24 | 단계 점프·저장 검사 통과 | A=50과 재접속에서 >= 판정으로 보충하며 이미 열린 권한을 회수하지 않음. |
| U25 | 실행 차단 검사 통과 | 자동 포탈 정리 중 강화·제작·소탕 요청 거절. 장비 변경의 기존 실행 검사도 유지함. |
| U26 | 기존 등급 회귀 검사 통과 | RiftRarityTests 28개. 곡선·상한·실제 전투·소탕 난수 및 재설정 비용 검사. 기존 확률 시트 변경 없음. |
| U27 | 경제 계산 완료 / 부족 조건 확인 | 1단 보스+초회 재료 15개, 첫 강화 20개로 5개 부족. 3단 제작 1,500골드·50재료. 보석 의존 미구현. 무료 지급 없음. |
| U28 | 3직업 자동 실행 / 수동 미실행 | seed=112358, 1단: 전사 80.45초·궁수 300초·마법사 42.15초에 모두 실패. 경로 오류 없음. 난도·패턴 공정성은 추가 검토 필요. |
| U29 | 로컬·병합 검사만 통과 | 저장 재접속, 검증된 권한 합집합·안내 병합·중복 방지 검사. 실제 계정 서버·다른 기기·오프라인 충돌 검증은 미연결로 미실행. |
| U30 | 데이터 대조 완료 | 공통 ContentUnlocks.json에서 조건표를 생성. 기존 25개 시트·23개 표·셀 값·수식·서식 보존. 변경 2개 시트 렌더·수식 검사. |

## 실제 수정 파일

아래는 이번 변경이 들어간 파일이다. 새 Assets 파일의 `.meta`도 함께 추가했다. 전체 목록과 해시는 `Artifacts/Validation/ContentUnlock/owned-files.json`, `verified-build-source-hashes.json`에 보존한다.

- [Assets/HELLSCRIPT/Resources/ContentUnlocks.json](../../Assets/HELLSCRIPT/Resources/ContentUnlocks.json)
- [Assets/HELLSCRIPT/Resources/Localization/en.txt](../../Assets/HELLSCRIPT/Resources/Localization/en.txt)
- [Assets/HELLSCRIPT/Runtime/Core/BehaviorPresets.cs](../../Assets/HELLSCRIPT/Runtime/Core/BehaviorPresets.cs)
- [Assets/HELLSCRIPT/Runtime/Core/CombatSimulation.BuildChanges.cs](../../Assets/HELLSCRIPT/Runtime/Core/CombatSimulation.BuildChanges.cs)
- [Assets/HELLSCRIPT/Runtime/Core/CombatSimulation.cs](../../Assets/HELLSCRIPT/Runtime/Core/CombatSimulation.cs)
- [Assets/HELLSCRIPT/Runtime/Core/ContentUnlocks.cs](../../Assets/HELLSCRIPT/Runtime/Core/ContentUnlocks.cs)
- [Assets/HELLSCRIPT/Runtime/Core/Economy.ContentServices.cs](../../Assets/HELLSCRIPT/Runtime/Core/Economy.ContentServices.cs)
- [Assets/HELLSCRIPT/Runtime/Core/Economy.cs](../../Assets/HELLSCRIPT/Runtime/Core/Economy.cs)
- [Assets/HELLSCRIPT/Runtime/Core/GameCatalog.cs](../../Assets/HELLSCRIPT/Runtime/Core/GameCatalog.cs)
- [Assets/HELLSCRIPT/Runtime/Core/GameStore.BuildChanges.cs](../../Assets/HELLSCRIPT/Runtime/Core/GameStore.BuildChanges.cs)
- [Assets/HELLSCRIPT/Runtime/Core/GameStore.cs](../../Assets/HELLSCRIPT/Runtime/Core/GameStore.cs)
- [Assets/HELLSCRIPT/Runtime/Core/InventoryManagement.cs](../../Assets/HELLSCRIPT/Runtime/Core/InventoryManagement.cs)
- [Assets/HELLSCRIPT/Runtime/Core/RiftGenerator.cs](../../Assets/HELLSCRIPT/Runtime/Core/RiftGenerator.cs)
- [Assets/HELLSCRIPT/Runtime/Core/RiftLayout.cs](../../Assets/HELLSCRIPT/Runtime/Core/RiftLayout.cs)
- [Assets/HELLSCRIPT/Runtime/Core/TrainingComparison.cs](../../Assets/HELLSCRIPT/Runtime/Core/TrainingComparison.cs)
- [Assets/HELLSCRIPT/Runtime/Presentation/GameController.cs](../../Assets/HELLSCRIPT/Runtime/Presentation/GameController.cs)
- [Assets/HELLSCRIPT/Runtime/Presentation/GameUI.Comparison.cs](../../Assets/HELLSCRIPT/Runtime/Presentation/GameUI.Comparison.cs)
- [Assets/HELLSCRIPT/Runtime/Presentation/GameUI.ContentUnlocks.cs](../../Assets/HELLSCRIPT/Runtime/Presentation/GameUI.ContentUnlocks.cs)
- [Assets/HELLSCRIPT/Runtime/Presentation/GameUI.InventoryLayout.cs](../../Assets/HELLSCRIPT/Runtime/Presentation/GameUI.InventoryLayout.cs)
- [Assets/HELLSCRIPT/Runtime/Presentation/GameUI.Items.cs](../../Assets/HELLSCRIPT/Runtime/Presentation/GameUI.Items.cs)
- [Assets/HELLSCRIPT/Runtime/Presentation/GameUI.RiftKeeper.cs](../../Assets/HELLSCRIPT/Runtime/Presentation/GameUI.RiftKeeper.cs)
- [Assets/HELLSCRIPT/Runtime/Presentation/GameUI.cs](../../Assets/HELLSCRIPT/Runtime/Presentation/GameUI.cs)
- [Assets/HELLSCRIPT/Runtime/Presentation/RuntimeContentUnlockSmoke.cs](../../Assets/HELLSCRIPT/Runtime/Presentation/RuntimeContentUnlockSmoke.cs)
- [Assets/HELLSCRIPT/Tests/Editor/CombatStatisticsTests.cs](../../Assets/HELLSCRIPT/Tests/Editor/CombatStatisticsTests.cs)
- [Assets/HELLSCRIPT/Tests/Editor/ContentTestAccounts.cs](../../Assets/HELLSCRIPT/Tests/Editor/ContentTestAccounts.cs)
- [Assets/HELLSCRIPT/Tests/Editor/ContentUnlockTests.cs](../../Assets/HELLSCRIPT/Tests/Editor/ContentUnlockTests.cs)
- [Assets/HELLSCRIPT/Tests/Editor/CoreTests.cs](../../Assets/HELLSCRIPT/Tests/Editor/CoreTests.cs)
- [Assets/HELLSCRIPT/Tests/Editor/DefeatAnalysisTests.cs](../../Assets/HELLSCRIPT/Tests/Editor/DefeatAnalysisTests.cs)
- [Assets/HELLSCRIPT/Tests/Editor/EdictAimTests.cs](../../Assets/HELLSCRIPT/Tests/Editor/EdictAimTests.cs)
- [Assets/HELLSCRIPT/Tests/Editor/EdictDriveTests.cs](../../Assets/HELLSCRIPT/Tests/Editor/EdictDriveTests.cs)
- [Assets/HELLSCRIPT/Tests/Editor/EdictPolicyBridgeTests.cs](../../Assets/HELLSCRIPT/Tests/Editor/EdictPolicyBridgeTests.cs)
- [Assets/HELLSCRIPT/Tests/Editor/EdictResponseTests.cs](../../Assets/HELLSCRIPT/Tests/Editor/EdictResponseTests.cs)
- [Assets/HELLSCRIPT/Tests/Editor/EdictRuleOrderTests.cs](../../Assets/HELLSCRIPT/Tests/Editor/EdictRuleOrderTests.cs)
- [Assets/HELLSCRIPT/Tests/Editor/GrowthTests.cs](../../Assets/HELLSCRIPT/Tests/Editor/GrowthTests.cs)
- [Assets/HELLSCRIPT/Tests/Editor/IncomingForecastTests.cs](../../Assets/HELLSCRIPT/Tests/Editor/IncomingForecastTests.cs)
- [Assets/HELLSCRIPT/Tests/Editor/ItemizationTests.cs](../../Assets/HELLSCRIPT/Tests/Editor/ItemizationTests.cs)
- [Assets/HELLSCRIPT/Tests/Editor/OutcomeTests.cs](../../Assets/HELLSCRIPT/Tests/Editor/OutcomeTests.cs)
- [Assets/HELLSCRIPT/Tests/Editor/PlayerTrainingTests.cs](../../Assets/HELLSCRIPT/Tests/Editor/PlayerTrainingTests.cs)
- [Assets/HELLSCRIPT/Tests/Editor/RiftRarityTests.cs](../../Assets/HELLSCRIPT/Tests/Editor/RiftRarityTests.cs)
- [Assets/HELLSCRIPT/Tests/Editor/RiftTests.cs](../../Assets/HELLSCRIPT/Tests/Editor/RiftTests.cs)
- [Assets/HELLSCRIPT/Tests/Editor/TrainingComparisonTests.cs](../../Assets/HELLSCRIPT/Tests/Editor/TrainingComparisonTests.cs)
- [Docs/Data/Content_Unlock_Validation_v0.1.json](../../Docs/Data/Content_Unlock_Validation_v0.1.json)
- [Docs/Data/HELLSCRIPT_Content_v0.1.xlsx](../../Docs/Data/HELLSCRIPT_Content_v0.1.xlsx)
- [Docs/Design/HELLSCRIPT_Content_Unlock_Spec_v0.1.md](../../Docs/Design/HELLSCRIPT_Content_Unlock_Spec_v0.1.md)
- [Docs/Design/HELLSCRIPT_GDD_v0.1.md](../../Docs/Design/HELLSCRIPT_GDD_v0.1.md)

검증 증거: [전체 테스트](../../Artifacts/Validation/ContentUnlock/final-all.xml), [집중 테스트](../../Artifacts/Validation/ContentUnlock/final-focused.xml), [빌드 로그](../../Artifacts/Validation/ContentUnlock/build-final.log), [앱 실행 로그](../../Artifacts/Validation/ContentUnlock/player-final.log), [UI 결과](../../Artifacts/Validation/ContentUnlock/evidence-final/runtime-unlock-smoke.txt), [엑셀 보존 결과](../../Artifacts/Validation/ContentUnlock/Workbook/preservation-final.json).

실행 빌드: [HELLSCRIPT.app](../../Builds/ContentUnlock/HELLSCRIPT.app). 기존 다른 빌드를 덮어쓰지 않고 별도 폴더에 보존했다.

## 커밋·위키 후속 검증

이번 커밋에는 콘텐츠 해금 변경과 위키 갱신만 선별했다. 진행 중인 던전·칙령·현지화 변경은 작업 폴더에 그대로 남긴다. Git의 커밋 대상에서 내보낸 별도 프로젝트로 전체 Edit Mode 검사 **1,258개 통과, 실패 0개**를 확인했다. 앞의 1,362개 결과는 다른 진행 변경을 포함한 Work 전체 스냅샷의 결과이며 두 검사를 구분한다.

위키에는 공통 JSON에서 직접 읽는 해금 9개 DB, 영웅 레벨별 패시브 슬롯, 현재 개발 현황, 상세 명세와 한·영 구현 기록을 반영했다. 위키 검사 6개와 커밋 소스 기준 문서 91개·DB 18개의 검증이 통과했다. 실제 로컬 위키 화면에서 코어 제작 조건과 연결 문서도 확인했다. 기존 위키가 참조하는 빌드·검사 증거는 Git 제외 규칙에 따라 로컬에 유지한다. 후속 검사 파일은 `Artifacts/Validation/ContentUnlock/Commit/`에 보존한다.

커밋 소스로 만든 macOS 개발 빌드도 성공했으며 오류는 0개다. 같은 빌드에서 한국어·영어, 세로·가로 해금 화면과 첫 실패·영웅 전환·안내 건너뛰기 흐름을 실행해 `HELLSCRIPT_UNLOCK_RUNTIME_OK`를 확인했다. 화면 7장을 보존했고 신규 한국어 화면과 영어 해금 목록도 직접 확인했다.
