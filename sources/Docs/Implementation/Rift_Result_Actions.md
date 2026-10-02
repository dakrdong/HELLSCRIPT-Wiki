# 균열 결과 행동과 전투 그래프

갱신일: 2026-10-03 · [English](Rift_Result_Actions.en.md)

균열 결과의 보상 수령, 획득 장비 교체, 그래프 조회와 다시 도전을 실제 저장 거래에 연결했다.

- **보상 받기**: `GameStore.ClaimRiftFirstRewards`가 저장한 실제 상자 지급 내역을 검은 딤 위 중앙에 표시한다. 각 보상의 아이콘·이름·수량과 **아무곳이나 눌러서 닫기**를 제공한다. 수령 전 안내·확인 창이나 연출 뒤 추가 창을 열지 않는다. 기존 자동 지급 골드·XP·전리품을 다시 지급하거나 상자를 자동 개봉하지 않는다. 저장 실패는 성공 연출을 열지 않는다.
- **빈 스킬**: 액티브와 궁극기 모두 **스킬 미지정** 텍스트만 표시한다. 흰 원과 아이콘 틀을 만들지 않는다. 훈련장과 전투 상세도 같은 `RiftSkillShareView`를 공유한다.
- **획득 장비**: 획득 당시 상세 스냅샷을 유지하고, 현재 같은 ID로 소유한 장비에 **착용 아이템과 비교**·**장착**을 제공한다. 비교는 `EquipmentComparisonView`/`ItemComparison`을 재사용하며, 장착은 기존 `EquipmentSlots`와 `GameStore` 거래로 확정한다. 두 반지·쌍수의 교체 기준은 공통 비교창에서 선택한다. 자동 창고 입고 장비는 기존 `Storage.Move`와 장착을 한 거래로 처리한다. 분해·판매·버림·소유권 상실 장비를 복구하거나 장착하지 않는다.
- **그래프 확인**: 이전 기록 텍스트 비교를 이번 전투의 그래프로 교체했다. 훈련장의 기존 `TrainingDpsPanel`, `TrainingChartView`, `TrainingDpsChart`, 3초 이동 DPS와 스킬 사용 시점 조사 기능을 그대로 공유한다. 피해는 기존 훈련장과 같은 실제 HP 감소 기준이며, 스킬 아이콘 표시와 툴팁의 모든 사용 스킬 표기도 같은 규칙이다. 균열 체크포인트와 완료 기록에 초별 피해·사용 시점을 저장하고 과거 전투를 현재 장비로 재계산하지 않는다. 기록이 없는 과거 저장은 새 전투부터 기록한다는 안내만 표시한다. 같은 창에서 공통 [전투 활동 패널](Rift_Activity_Time.md)로 이동·공격·회피·파밍의 최종 시간·비율·곡선도 확인한다.
- **반복 사냥 설정**: 결과에 표시된 균열 단계가 14 이하이면 설정 여부와 관계없이 흐리게 표시한다. 누르면 **반복 설정은 균열 15레벨 부터 가능합니다** 안내만 2초 표시하고, 설정 바로가기나 반복 안내 창을 열지 않는다. 15단계부터 현재 결과의 반복이 꺼져 있으면 흐리게 표시한다. 눌렀을 때 **반복 사냥 설정이 되어있지 않습니다** 말풍선을 2초 표시한다. 안내 아래의 작은 **설정하러 가기** 버튼은 기존 사냥 칙령의 **반복 사냥** 탭을 연다. 탭을 닫으면 같은 결과창·전투 기록·획득 아이템 위치로 돌아온다. 설정 창이 열려 있으면 자동 반복과 수동 재도전 카운트를 멈춘다. 설정된 결과의 안내 페이지에서는 머리글의 설정 버튼을 숨긴다.
- **다음 레벨**: 보상과 재도전 사이에서 완료한 균열의 다음 단계로 진입한다. 예를 들어 최고 클리어가 30단계여도 24단계 결과에서는 25단계로 이동한다. 저장된 성공 결과에서 다음 단계가 개방되고 1,000단계를 넘지 않을 때 활성화한다. 기존 `PrepareResultRetry`·`ReturnTown`·`Begin` 경로로 현재 반복 예약과 수동 재도전 카운트를 취소하고 정상 입장·피로도·물약·저장 검증을 사용한다. 넓은 화면은 보상→다음 레벨→재도전→귀환 순서의 레일이며, 세로 화면은 보상·다음 레벨 / 재도전·귀환 두 줄로 표시한다.
- **행동 버튼 중앙 정렬**: 넓은 화면의 네 버튼은 아이콘과 실제 줄바꿈을 반영한 글자 높이를 합쳐 세로 중앙에 배치한다. 기존 글자 영역의 불필요한 아래 여백을 제거하고, 한국어·영어 모두 같은 공통 버튼 생성 경로를 사용한다.
- **다시 도전**: 결과 창을 유지한 채 버튼에 5·4·3·2·1과 **눌러서 취소**를 표시한다. 다시 누르면 진입을 취소한다. 수동 도전은 현재 결과의 자동 반복 예약을 취소하고, 영웅의 반복 설정은 새 전투에 보존한다. 중첩 창과 앱 포커스 해제 동안 수동 카운트를 멈추고, 저장 실패나 창 닫기 후 진입하지 않는다.

## 전투 중 실시간 DPS, 2026-10-02

균열 전투에서도 훈련장의 기존 DPS 패널을 사용한다. `GameUI.TrainingGround`의 같은 생성·배치·갱신 경로와 `TrainingChartView`/`TrainingDpsChart`를 공유하며, 별도 그래프나 피해 집계를 만들지 않는다.

- 최근 3초 DPS, 평균·최고·총 피해, 시간별 그래프와 쿨타임 스킬·궁극기 사용 아이콘을 같은 규칙으로 표시한다. 데이터는 현재 균열의 `RunState.dps`다. 전투 일시정지 동안 시료와 시간은 진행하지 않는다.
- 비교선과 증감은 같은 영웅·같은 단계의 가장 최근 성공 기록 중 실제 DPS 시료가 남은 기록을 읽는다. 현재 판·다른 캐릭터·다른 단계·실패·시료 없는 과거 기록은 섞지 않는다. 비교 기록이 없으면 기존 ‘비교할 직전 판 없음’을 표시한다.
- 화살표로 실시간 DPS 한 줄만 남기거나 그래프를 펼친다. 접기 상태는 훈련과 균열이 공유하는 세션 상태이며 화면 재구성 후에도 유지한다. 넓은 화면에서는 미니맵 옆에, 좁은 띠 배치에서는 지도·머리글 아래에 둔다. 기본 배치는 보스 상태와 겹치지 않는다. 사용자가 직접 옮긴 위치는 자동 배치보다 우선한다.
- 과거 중단 저장에 시료 자체가 없으면 DPS는 `—`, 안내는 ‘기록 없음’으로 표시한다. 현재 장비로 과거 피해를 재계산하지 않는다. 마을·결과·필수 튜토리얼에는 실시간 패널을 남기지 않는다. 훈련 전용 중단·일시정지 창은 기존 훈련 경로를 유지하며, 균열 일시정지는 기존 관찰 메뉴를 사용한다.

검증: 관련 Edit Mode 66/66, 공통 UI 소유 검사 및 계약 테스트 11/11, macOS 개발 빌드와 실행 검증을 통과했다. 한국어·영어 × 세로 440×956·가로 956×440·PC 1600×900·1600×1000·1680×720 × 미니맵 3개 모드의 30가지 조합에서 안전 영역, 보스·지도·로그와의 분리, 포인터 접기·펼치기, 화면 재구성 후 상태 유지와 데이터 일치를 확인했다. 기존 훈련장의 그래프와 일시정지·재개도 확인했다. 모바일 안전 영역과 포인터 입력은 macOS에서 모사했으며 실제 iOS·Android 검증은 아니다. 첫 실행은 비동기 입장 완료를 기다리지 않은 검증 코드 때문에 중단되어, 대기를 수정한 뒤 중단된 실행 검증만 다시 수행했다. 전체 게임 테스트는 반복하지 않았다. URP 후처리 셰이더 경고 4건은 기록에 남겼으며 관리 코드 예외는 없었다.

[검증 요약](RiftResultActionsEvidence/live-dps-validation.json) · [Edit Mode](RiftResultActionsEvidence/live-dps-editmode.xml) · [공통 UI](RiftResultActionsEvidence/live-dps-ui-contract.txt) · [실행 기록](RiftResultActionsEvidence/live-dps-runtime.txt)

[PC 한국어](RiftResultActionsEvidence/live-dps-rift-dps-1600x900-ko.png) · [PC 영어](RiftResultActionsEvidence/live-dps-rift-dps-1600x900-en.png) · [세로 한국어](RiftResultActionsEvidence/live-dps-rift-dps-440x956-ko.png) · [세로 영어](RiftResultActionsEvidence/live-dps-rift-dps-440x956-en.png) · [가로 한국어](RiftResultActionsEvidence/live-dps-rift-dps-956x440-ko.png) · [가로 영어](RiftResultActionsEvidence/live-dps-rift-dps-956x440-en.png) · [접은 상태](RiftResultActionsEvidence/live-dps-rift-dps-folded-1600x900-ko.png) · [공통 훈련장 패널](RiftResultActionsEvidence/live-dps-training-shared-dps-1600x900-ko.png)

## DPS 위치와 전투 상단, 2026-10-02

- 실시간 DPS 제목 영역을 마우스나 터치로 끌어 이동한다. 접기 화살표는 이동 손잡이와 분리되어 기존 동작을 유지한다.
- 놓은 위치는 `DpsHudPosition`이 기기의 `hellscript-dps-position-v1.json`에 원자적으로 저장한다. 균열·훈련장이 같은 위치를 쓰며 화면 재구성·게임 재실행 후 복원한다. 계정·캐릭터·전투 저장은 변경하지 않는다. 저장 실패는 안내하고 다음 드래그로 재시도한다.
- 안전 영역을 기준으로 좌상단 위치의 비율을 보관한다. 해상도·방향 변경 및 접기·펼치기는 화면 밖으로 나간 부분과 하단 스킬·물약·전투 로그에 겹친 부분만 보정하고 원래 저장 위치를 덮지 않는다.
- 보스 이름·행동·체력바는 상단 중앙에 고정한다. 좁은 화면에서는 머리글 바로 아래의 중앙 줄을 사용한다. 그래프를 옮겨도 보스 체력바가 밀리지 않는다.
- 절전 모드와 포탈 복귀는 설정 버튼 왼쪽에 나란히 두고 관찰 메뉴는 그 왼쪽에 둔다. 지도는 버튼 설명 아래에서 시작한다.

검증: 관련 Edit Mode **13/13**과 공통 UI 계약 **11/11**이 통과했다. 하단 스킬바 뒤에 손잡이가 가려지는 첫 실행 실패를 수정한 뒤 직접 영향을 받는 드래그 회귀 **1/1**과 macOS 실행 검증을 다시 수행했다. 최종 빌드 오류와 실행 예외는 0개다. 한국어·영어 × 5개 비율 × 3개 지도 방식 30개 조합에서 드래그·접기·화면 재구성·상단 정렬을 확인했고, 별도 새 프로세스로 위치 복원을 확인했다. 입장 도움말은 10개 화면 조합에서 실제 다음 해금 단계로 이동하고 선택 단계 4를 보존했다. 테스트 계정의 다음 해금은 5단계였으며, 실 계정에서는 같은 조회가 실제 다음 단계(예: 10)를 전달한다. 전체 게임 검사와 모바일 실기기 검증은 수행하지 않았다.

번역 표 구문·현재 런타임 문구 누락 검사는 **2/2** 통과했다. 별도 사용하지 않는 번역 키 검사는 이전 기준 커밋에도 존재하는 `{0}단계 · {1}`, `기록 목록` 2개 때문에 실패했으며 이번 변경과 구분해 기록했다.

[범위·소스 해시](RiftResultActionsEvidence/dps-drag-validation.json) · [실행 기록](RiftResultActionsEvidence/dps-drag-runtime.txt) · [새 프로세스 복원](RiftResultActionsEvidence/dps-drag-relaunch.txt) · [번역 기준 비교](RiftResultActionsEvidence/dps-drag-localization-baseline.json)

![상단 중앙 보스와 오른쪽 바로가기](RiftResultActionsEvidence/dps-hud-default-1600x900-ko.png)

[이동한 그래프](RiftResultActionsEvidence/dps-drag-1600x900-en.png) · [세로](RiftResultActionsEvidence/dps-drag-440x956-en.png) · [가로](RiftResultActionsEvidence/dps-drag-956x440-ko.png) · [재실행](RiftResultActionsEvidence/dps-drag-relaunch-1600x900-ko.png)

## 공통 UI 어댑터

`RiftRewardRevealWindow`와 `RiftCombatGraphWindow`는 `tools/new_content_ui.py`에서 시작했다. 두 창 모두 `ContentWindowView`와 창 관리자의 안전 영역·입력·일시정지 임대를 사용한다. 수령 연출은 사용자 요청에 따라 표준 창틀·제목줄·행동줄을 숨기고 검은 딤과 중앙 수령 내역만 표시하는 어댑터다. 별도 캔버스·장비 계산·저장 소유자를 만들지 않는다. 표시 데이터는 이미 저장된 `RewardSnapshot`이며 클릭은 닫기만 수행한다.

## 2026-10-03: 결과 활동 기록과 반복 설정 바로가기 검증

완료 기록에 실제 마지막 틱까지의 이동·공격·회피·파밍 곡선을 저장하고, 결과창의 **그래프 확인**에서 기존 DPS·스킬 조사와 함께 표시한다. 균열 결과 단계가 14 이하이면 설정 여부와 관계없이 흐린 버튼과 2초 제한 안내만 표시한다. 15단계 이상에서 반복 미설정 안내의 작은 **설정하러 가기** 버튼은 기존 사냥 칙령의 반복 탭을 열고, 닫으면 같은 결과창으로 돌아온다.

관련 Edit Mode 범위 107개가 종료됐다. 초회 실패 5개는 신규 UI 테스트가 Edit Mode에서 런타임 `Destroy`를 호출한 정리 오류였다. 기존 `DestroyImmediate` 종료 정리를 사용하도록 테스트만 고쳐 해당 5개를 재검사했고 **5 통과·0 실패·0 건너뜀**이었다. 초회 MCP 상세 집계가 반환되지 않았으므로 107/107 통과로 표기하지 않는다. 공통 UI 소유 계약과 계약 검사 **11/11**은 통과했다. 전체 게임 검사는 반복하지 않았다.

macOS 개발 빌드는 오류 0·경고 191로 성공했다. 한 번의 통합 조작 검사에서 기존 HUD 30개 조건, 결과 활동·설정 바로가기의 한국어·영어 10개 화면 조합을 확인했다. 실제 균열이 자연 종료된 88.6초의 표본 114개가 완료 기록·최종 시간·비율과 같고, 별도 `GameStore`로 저장 파일을 다시 읽어도 같았다. 실제 레이캐스트 뒤 합성 포인터로 그래프와 설정 탭을 열고 닫아 같은 결과·전투·아이템 위치가 유지됨을 확인했다. 마을·훈련에는 활동 HUD가 남지 않고 기존 훈련 DPS를 유지했다. 성공 표식 `HELLSCRIPT_RIFT_ACTIVITY_SMOKE_OK`, 종료 코드 0, 관리 예외 0개였다. 실사용 저장·설정 파일 28개는 해시가 유지됐다. Unity 분석 캐시와 공유 Editor 테스트 출력은 별도다. 모바일 안전 영역은 macOS 모의이며 실기기·WebGL은 이번에 검증하지 않았다.

추가 요청의 15단계 경계는 **관련 6/6**(14·15·16단계 설정 상태와 영어 표 검사)으로 확인했다. 마지막 개발 빌드는 오류 0·경고 193이다. 기존 HUD 30개 조건은 그대로 재사용하고 변경된 결과창 10개 조건만 따로 확인했다. 14단계에서는 설정이 저장되어 있어도 흐리게 표시되고 제한 안내만 2초 뒤 사라지며 바로가기·설정 안내 페이지는 열리지 않는다. 15·16단계의 설정된 상태는 정상 밝기이고, 15단계의 미설정 상태는 작은 설정 버튼과 같은 결과로 돌아오는 흐름을 유지한다. 경계 UI에만 14·15·16단계 픽스처를 사용했고 활동 시간·피해·스킬 시점은 자연 종료한 실제 1단계 전투 기록이다.

[경계 검사](RiftResultActivityEvidence20261003/editmode-repeat-gate.json) · [최종 빌드](RiftResultActivityEvidence20261003/native-build-gated.json) · [초기 HUD 검사 재사용](RiftResultActivityEvidence20261003/runtime-initial.txt) · [한국어 14단계 안내](RiftResultActivityEvidence20261003/repeat-locked-1600x900-ko.png) · [세로 영어 14단계 안내](RiftResultActivityEvidence20261003/repeat-locked-440x956-en.png).

[검증 범위와 소스 해시](RiftResultActivityEvidence20261003/validation.json) · [초회 검사](RiftResultActivityEvidence20261003/editmode-first.json) · [실패 항목 재검사](RiftResultActivityEvidence20261003/editmode-retest.json) · [빌드](RiftResultActivityEvidence20261003/native-build.json) · [조작 기록](RiftResultActivityEvidence20261003/runtime.txt) · [최종 누계](RiftResultActivityEvidence20261003/final-feedback.json) · [실제 곡선](RiftResultActivityEvidence20261003/final-activity-samples.json).

[세로 한국어 결과 그래프](RiftResultActivityEvidence20261003/result-activity-440x956-ko.png) · [가로 한국어 결과 그래프](RiftResultActivityEvidence20261003/result-activity-956x440-ko.png) · [PC 영어 결과 그래프](RiftResultActivityEvidence20261003/result-activity-1600x1000-en.png) · [한국어 설정 바로가기](RiftResultActivityEvidence20261003/repeat-shortcut-1600x900-ko.png) · [세로 영어 설정 바로가기](RiftResultActivityEvidence20261003/repeat-shortcut-440x956-en.png) · [한국어 반복 탭](RiftResultActivityEvidence20261003/repeat-tab-1600x900-ko.png) · [가로 영어 반복 탭](RiftResultActivityEvidence20261003/repeat-tab-956x440-en.png).

## 검증

### 2026-10-02: 다음 레벨 진입과 행동 버튼 중앙 정렬

`RiftResultTests` **27/27**과 공통 UI 소유 검사·계약 **11/11**이 통과했다. 번역 검사는 **32/33**이며, 실패 1개는 기준 커밋에도 남아 있는 미사용 번역 키 `{0}단계 · {1}`, `기록 목록` 검사다. 신규 문구 누락은 없고 C# 컴파일과 최종 macOS 개발 빌드 오류는 0개다. 에디터의 Unity AI 구독 오류와 플레이어의 기존 URP 후처리 셰이더 경고 4건은 별도로 기록했다.

한국어·영어와 세로 440×956, 가로 956×440, PC 1600×900·1600×1000·2100×900의 10개 화면 조합에서 버튼 순서·안전 영역·문구 잘림을 확인했다. 중앙 정렬 수정 후 직접 영향을 받는 가로·PC 8개 조합만 다시 측정했으며 아이콘·글자 묶음과 버튼 중심의 차이는 1.5픽셀 이내다. 바뀌지 않은 세로 2개 조합과 결과창 보상·장착·그래프 조작의 성공 결과를 재사용했다.

최고 클리어 30단계에서 실제 24단계 전투를 완료한 뒤 재도전 카운트를 시작하고 **다음 레벨**을 눌렀다. 기존 입장 경로가 카운트를 취소하고 새로운 25단계 전투·저장 체크포인트를 만들었으며 반복 설정은 보존됐다. 실패 결과에서 버튼이 비활성화되는 것도 실제 결과창으로 확인했다. 최종 실행은 `HELLSCRIPT_RIFT_RESULT_ACTIONS_OK`와 종료 코드 0으로 완료됐고 관리 코드 예외는 없었다. macOS의 uGUI 레이캐스트·합성 포인터 검증이며 모바일 실기기 검증은 아니다.

초기 실행의 기존 재도전 검증은 포커스·자동 반복 대기 문제로 중단됐으며, 이후 포커스를 확보한 실행에서 5→4→3→2→1과 진입을 확인했다. 중간 빌드의 URP 초기화 오류는 에셋 새로고침·별도 출력의 클린 빌드로 해결했다. 다음 단계 진입 후 반복 설정을 무조건 켜짐으로 기대하던 픽스처도 실제 진입 전 설정과 비교하도록 바로잡았다. 최종에는 바뀐 버튼 배치와 다음 단계 진입만 확인하는 범위를 사용했다. 전체 게임 검사는 반복하지 않았다.

[검증 범위와 소스 해시](RiftResultActionsEvidence/next-level-validation.json) · [관련 검사 XML](RiftResultActionsEvidence/next-level-editmode.xml) · [번역 기준 비교](RiftResultActionsEvidence/next-level-localization-baseline.json) · [개발 빌드](RiftResultActionsEvidence/next-level-build.json) · [실행 기록](RiftResultActionsEvidence/next-level-runtime.txt) · [저장 재확인](RiftResultActionsEvidence/next-level-save-readback.json)

![중앙 정렬된 행동 버튼과 다음 레벨](RiftResultActionsEvidence/next-level-centered-result-1600x900-ko.png)

[PC 영어](RiftResultActionsEvidence/next-level-centered-result-1600x900-en.png) · [가로 영어](RiftResultActionsEvidence/next-level-centered-result-956x440-en.png) · [세로 한국어](RiftResultActionsEvidence/next-level-result-440x956-ko.png) · [세로 영어](RiftResultActionsEvidence/next-level-result-440x956-en.png) · [25단계 실제 진입](RiftResultActionsEvidence/next-level-battle.png)

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
