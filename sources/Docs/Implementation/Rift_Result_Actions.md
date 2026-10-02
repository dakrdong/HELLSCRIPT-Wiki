# 균열 결과 행동과 전투 그래프

갱신일: 2026-10-02 · [English](Rift_Result_Actions.en.md)

균열 결과의 보상 수령, 획득 장비 교체, 그래프 조회와 다시 도전을 실제 저장 거래에 연결했다.

- **보상 받기**: `GameStore.ClaimRiftFirstRewards`가 저장한 실제 상자 지급 내역을 검은 딤 위 중앙에 표시한다. 각 보상의 아이콘·이름·수량과 **아무곳이나 눌러서 닫기**를 제공한다. 수령 전 안내·확인 창이나 연출 뒤 추가 창을 열지 않는다. 기존 자동 지급 골드·XP·전리품을 다시 지급하거나 상자를 자동 개봉하지 않는다. 저장 실패는 성공 연출을 열지 않는다.
- **빈 스킬**: 액티브와 궁극기 모두 **스킬 미지정** 텍스트만 표시한다. 흰 원과 아이콘 틀을 만들지 않는다. 훈련장과 전투 상세도 같은 `RiftSkillShareView`를 공유한다.
- **획득 장비**: 획득 당시 상세 스냅샷을 유지하고, 현재 같은 ID로 소유한 장비에 **착용 아이템과 비교**·**장착**을 제공한다. 비교는 `EquipmentComparisonView`/`ItemComparison`을 재사용하며, 장착은 기존 `EquipmentSlots`와 `GameStore` 거래로 확정한다. 두 반지·쌍수의 교체 기준은 공통 비교창에서 선택한다. 자동 창고 입고 장비는 기존 `Storage.Move`와 장착을 한 거래로 처리한다. 분해·판매·버림·소유권 상실 장비를 복구하거나 장착하지 않는다.
- **그래프 확인**: 이전 기록 텍스트 비교를 이번 전투의 그래프로 교체했다. 훈련장의 기존 `TrainingDpsPanel`, `TrainingChartView`, `TrainingDpsChart`, 3초 이동 DPS와 스킬 사용 시점 조사 기능을 그대로 공유한다. 피해는 기존 훈련장과 같은 실제 HP 감소 기준이며, 스킬 아이콘 표시와 툴팁의 모든 사용 스킬 표기도 같은 규칙이다. 균열 체크포인트와 완료 기록에 초별 피해·사용 시점을 저장하고 과거 전투를 현재 장비로 재계산하지 않는다. 기록이 없는 과거 저장은 새 전투부터 기록한다는 안내만 표시한다.
- **반복 사냥 설정**: 현재 결과의 반복이 꺼져 있으면 흐리게 표시한다. 눌렀을 때 **반복 사냥 설정이 되어있지 않습니다** 말풍선을 2초 표시한다. 설정된 결과의 안내 페이지에서는 머리글의 설정 버튼을 숨긴다.
- **다시 도전**: 결과 창을 유지한 채 버튼에 5·4·3·2·1과 **눌러서 취소**를 표시한다. 다시 누르면 진입을 취소한다. 수동 도전은 현재 결과의 자동 반복 예약을 취소하고, 영웅의 반복 설정은 새 전투에 보존한다. 중첩 창과 앱 포커스 해제 동안 수동 카운트를 멈추고, 저장 실패나 창 닫기 후 진입하지 않는다.

## 공통 UI 어댑터

`RiftRewardRevealWindow`와 `RiftCombatGraphWindow`는 `tools/new_content_ui.py`에서 시작했다. 두 창 모두 `ContentWindowView`와 창 관리자의 안전 영역·입력·일시정지 임대를 사용한다. 수령 연출은 사용자 요청에 따라 표준 창틀·제목줄·행동줄을 숨기고 검은 딤과 중앙 수령 내역만 표시하는 어댑터다. 별도 캔버스·장비 계산·저장 소유자를 만들지 않는다. 표시 데이터는 이미 저장된 `RewardSnapshot`이며 클릭은 닫기만 수행한다.

## 검증

### 2026-10-02: 에디터에서 보스 처치 후 결과 창 생성 오류

반복 사냥 설정 버튼에 `CanvasGroup`이 없을 때 `GetComponent<CanvasGroup>() ?? AddComponent<CanvasGroup>()`가 Unity 에디터의 null 객체를 올바르게 판별하지 못했다. `StyleRepeatSettings`의 투명도 지정에서 `MissingComponentException`이 먼저 발생해 `ContentWindowView.Open`이 반환되지 않았고, 이후 `Update` 47행에서 비어 있는 결과 창 참조를 갱신하며 `NullReferenceException`이 반복됐다.

공통 창 관리자의 기존 방식과 같은 `TryGetComponent` 검사로 바꿨다. 세로 행동줄과 가로 행동 레일이 공유하는 한 곳에서 없으면 추가하고 기존 컴포넌트는 재사용한다. 반복 미설정의 흐린 표시·말풍선 입력과 설정된 반복의 정상 표시는 유지한다.

누락된 컴포넌트의 첫 생성, 설정 활성화, 취소 후 비활성 표시, 단일 컴포넌트 재사용과 흐린 버튼의 클릭 가능 상태를 실제 Unity 에디터 회귀 검사에 추가했다. `RiftResultTests` **19/19**와 공통 UI 계약·11개 계약 검사가 통과했고 새 C# 컴파일 오류는 없었다. [이번 에디터 검사 원본](RiftResultActionsEvidence/canvas-null-editmode.json).

병합된 `main`의 macOS 개발 빌드는 오류 0개로 완료됐다. 격리된 저장에서 보스 처치·결과 표시 이후 한국어·영어 10개 화면 조합, 반복 미설정 말풍선, 보상 수령·장착·그래프·수동 재도전을 한 번의 기존 결과 행동 스모크로 확인했다. 성공 표식 `HELLSCRIPT_RIFT_RESULT_ACTIONS_OK`, 종료 코드 0, 해당 예외 0개와 5→4→3→2→1 이후 새 전투 진입을 확인했다. 전체 게임 검사·모바일 실기기 검사는 반복하지 않았다. [이번 검증 범위와 소스 해시](RiftResultActionsEvidence/canvas-null-validation.json) · [개발 빌드 결과](RiftResultActionsEvidence/canvas-null-build.json) · [런타임 조작 결과](RiftResultActionsEvidence/canvas-null-runtime.txt) · [재도전 입력 기록](RiftResultActionsEvidence/canvas-null-retry.txt) · [가로 한국어 결과와 반복 말풍선](RiftResultActionsEvidence/canvas-null-result-wide-ko.png) · [세로 영어 결과](RiftResultActionsEvidence/canvas-null-result-portrait-en.png).

공통 UI 계약 검사와 11개 계약 검사, `RiftResultTests`·`TrainingGroundTests`·`TrainingGroundUiTests`의 **64/64** 검사가 통과했다. 최신 `main`의 장비 추천 거래 연결 뒤 직접 영향을 받는 결과창 장착 **2/2**만 다시 확인했다. 나머지 통과 결과는 관련 코드와 동작이 유지되어 재사용했다.

macOS 개발 플레이어에서 기본 글자 크기와 한국어·영어로 **440×956, 956×440, 1600×900, 1600×1000, 2100×900**을 확인했다. 모바일 안전 영역은 macOS에서 모의했다. 실제 UI 레이캐스트 뒤 합성 포인터로 그래프 조사·비교·장착·보상 수령·닫기·취소를 조작했고, 격리된 저장 파일에서 장착 상태와 최초 보상 상자 1개를 다시 읽었다. 결과 화면의 그래프 검사용 수치는 대표 픽스처이며, 별도 훈련장 검사는 실제 전투 시뮬레이션을 진행하고 그래프·스킬 시점·다시 시작·칙령 변경을 확인했다. 재도전은 실제 포커스 상태에서 **5→4→3→2→1**, 중간 취소와 기존 온라인 설정 확인 후 새 전투 진입까지 통과했다. 빌드 오류는 0개다. 모바일 실기기는 검증하지 않았다.

전체 Edit Mode 검사는 한 번 실행되어 4,960개가 종료됐지만 **전체 통과는 아니다**. MCP에서 확인할 수 있는 실패 25개는 모두 기존 실패 목록에 포함된다. 최종 결과 객체가 반환되지 않고 도메인 재시작에서 상세 집계가 사라져 전체 통과·실패·건너뜀 수는 확정하지 않았다. 전체 검사를 반복하지 않고 이 작업과 직접 관련된 64개 검사 결과를 XML로 확보했다.

[검증 범위와 소스 해시](RiftResultActionsEvidence/validation.json), [관련 검사 XML](RiftResultActionsEvidence/related-editmode.xml), [최신 통합 장착 검사](RiftResultActionsEvidence/integrated-equipment.xml), [화면 검사 진행](RiftResultActionsEvidence/layout-progress.txt), [재도전 입력 결과](RiftResultActionsEvidence/runtime-actions.txt), [5→1과 입장 기록](RiftResultActionsEvidence/retry-diagnostic.txt), [훈련장 회귀 결과](RiftResultActionsEvidence/training-runtime.txt), [저장 파일 재확인](RiftResultActionsEvidence/save-readback.json), [전체 검사 원본 상태](RiftResultActionsEvidence/full-editmode-job.json), [기존 실패 목록](RiftResultActionsEvidence/baseline-failures.json).

![검은 딤 위 실제 보상 수령 내역](RiftResultActionsEvidence/reward-wide-ko.png)

![훈련장과 공유하는 DPS 그래프와 스킬 시점 조사](RiftResultActionsEvidence/graph-wide-ko.png)

[세로 결과](RiftResultActionsEvidence/result-portrait-ko.png) · [영문 결과와 반복 안내](RiftResultActionsEvidence/result-wide-en.png) · [세로 영문 장비 상세](RiftResultActionsEvidence/loot-detail-portrait-en.png) · [가로 영문 장비 비교](RiftResultActionsEvidence/loot-comparison-landscape-en.png) · [장착 완료](RiftResultActionsEvidence/loot-equipped-ko.png) · [세로 영문 보상](RiftResultActionsEvidence/reward-portrait-en.png) · [세로 영문 재도전](RiftResultActionsEvidence/retry-portrait-en.png) · [재도전 4와 취소](RiftResultActionsEvidence/retry-four.png) · [세로 영문 그래프](RiftResultActionsEvidence/graph-portrait-en.png).

균열 15단계의 신규 룬 해방 튜토리얼에서는 인젤 미르가 보상 수령을 강제로 안내하고 **룬 블럭 세트**만 같은 거래에서 개봉한다. 저장 후 인장이 콘텐츠 메뉴로 날아가고 NPC가 매 단계 설명하는 기존 5단계 플레이어블 가이드로 이어진다. 다른 결과의 보상 수령 규칙은 위와 같다. [룬 보드 해방 튜토리얼](Rune_Board_Unlock_Tutorial.md)을 참고한다.
