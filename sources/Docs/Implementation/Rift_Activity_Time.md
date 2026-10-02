# 균열 활동 시간 그래프

갱신일: 2026-10-03 · [English](Rift_Activity_Time.en.md)

균열 전투의 활동 시간을 **이동·공격·회피·파밍**으로 나누어 실시간으로 표시한다. 별도 HUD의 위쪽은 네 색의 합계 100% 막대, 아래쪽은 활동별 누적 초·비율과 네 누적 그래프다. 가로축은 균열의 경과 시간, 세로축은 해당 시점까지 그 활동에 쓴 누적 초다. 예를 들어 0.4초 파밍한 뒤 다른 활동을 하다가 다시 0.4초 파밍하면 파밍 선은 0.4초에서 수평으로 유지되다가 0.8초로 올라간다. 최근 30초만 자르지 않고 이번 플레이의 관찰 시작부터 현재까지 표시한다. 그래프 위 안내에 표시 구간의 시작·끝 시간을 쓴다.

| 항목 | 집계 기준 |
| --- | --- |
| 이동 | 미탐색 지역·보스 방·다른 목표 위치로 실제 보행하는 시간 |
| 공격 | 기본 공격·스킬의 준비/유지/마무리, 적 추격·모으기·전투 거리 조정 |
| 회피 | 실제 생존 대응의 회피 스킬과 보행 후퇴. 같은 이동 스킬도 생존 목적이면 회피 |
| 파밍 | 전리품을 회수하러 접근하거나 상자에 접근·개봉하는 시간 |

한 시뮬레이션 구간은 한 항목에만 들어간다. 같은 구간의 행동 전환은 회피→공격→파밍→이동 우선순위를 쓴다. 공격 중 자동으로 줍는 전리품이나 투사체·지속 피해는 별도의 파밍·공격 시간을 만들지 않는다. 대기·비활동 상호작용은 네 항목의 분모에서 제외한다. 일시정지·마을·포탈 정비·완료 후 즉시 회수도 시간을 추가하지 않는다. 관찰한 네 활동의 시간이 있으면 정수 비율은 최대 나머지 방식으로 항상 정확히 100%다. 기록 전에는 빈 막대와 대기 안내를 표시한다.

제목줄을 끌어 이동하고 왼쪽 화살표로 접거나 펼친다. **DPS와 접기·위치가 독립적**이다. 화면 재구성과 방향·언어 변경에도 위치와 접힘을 유지하며, 위치는 기기 파일 `hellscript-activity-position-v1.json`에 저장한다. 재실행에서는 마지막 위치를 읽고 기본적으로 펼친다. 안전 영역과 하단 로그·고정 조작 위로 위치를 제한한다. 그래프·막대는 입력을 차단하지 않는다.

기존 `CombatFeedback`이 실제 행동 구간을 단일 집계한다. 누계는 `RunState.statistics.feedback`와 완료 기록에 보존하고, 진행 중 그래프 표본은 메모리에 둔다. 완료 시 `CombatHistory.Capture`가 최종 틱까지의 실제 시각·누적 값 표본을 복사해 `CombatReview.activityHistory`에 저장한다. 중단 체크포인트의 표본 저장 규칙은 유지하며, 완료 기록은 파일을 다시 읽어도 곡선을 보존한다. 활동 전환과 초 경계를 기록하며, 오래 플레이하면 중간 표본을 간추려 최대 257개를 유지한다. 관찰 시작점·현재점·누계는 보존하고 실제 시간 간격에 맞춰 선을 그린다. 재접속하면 저장된 누계에서 그래프를 시작하고 재접속 이전의 세부 곡선은 추정하지 않는다. 이전 버전의 일반 이동 집계는 목적을 복원할 근거가 없으므로, 스킬 근거는 보존하면서 새 시간 관찰을 재개 시점부터 시작한다.

결과창의 **그래프 확인**은 기존 DPS·스킬 사용 시점 패널 아래에서 네 활동의 최종 누적 시간·정수 비율·곡선을 보여 준다. 활동 합계와 비율에서 제외된 대기 시간을 함께 표시한다. 화면을 열 때 완료 기록을 복사하므로 창 재배치와 언어 변경으로 원본 기록을 바꾸지 않는다. 과거 기록에 곡선이 없으면 최종 합계만 보여 주고 안내하며, 활동 집계 자체가 없거나 이전 집계 버전이면 기록 없음으로 표시한다. 재접속 이전의 곡선을 새로 추정하지 않는다.

`RiftActivityPanel`은 실시간 HUD와 결과 그래프의 공통 표시 소유자다. `GameUI.RiftActivity`는 기존 전투 캔버스에 붙는 읽기 전용 HUD 어댑터다. 새 콘텐츠 창·캔버스·차트 엔진을 만들지 않는다. `TrainingDpsChart`, `DpsHudDrag`, `DpsHudPosition`, `UiTheme`, `UiFonts`, `Loc`를 공유하고 DPS 위치 파일을 그대로 유지한다. 표시 조작은 계정·전투 집계를 변경하지 않는다.

원본 근거: [공통 활동 패널](../../Assets/HELLSCRIPT/Runtime/Presentation/RiftActivityPanel.cs), [결과 그래프](../../Assets/HELLSCRIPT/Runtime/Presentation/RiftCombatGraphWindow.cs), [완료 기록](../../Assets/HELLSCRIPT/Runtime/Core/CombatHistory.cs), [시간 집계](../../Assets/HELLSCRIPT/Runtime/Core/CombatFeedback.cs), [HUD 표시](../../Assets/HELLSCRIPT/Runtime/Presentation/GameUI.RiftActivity.cs), [관련 검사](../../Assets/HELLSCRIPT/Tests/Editor/CombatActivityTests.cs), [macOS 조작 검사](../../Assets/HELLSCRIPT/Runtime/Presentation/RuntimeTrainingGroundSmoke.Activity.cs).

## 2026-10-03: 결과 활동 기록과 반복 설정 바로가기 검증

완료 기록에 실제 마지막 틱까지의 이동·공격·회피·파밍 곡선을 저장하고, 결과창의 **그래프 확인**에서 기존 DPS·스킬 조사와 함께 표시한다. 균열 결과 단계가 14 이하이면 설정 여부와 관계없이 흐린 버튼과 2초 제한 안내만 표시한다. 15단계 이상에서 반복 미설정 안내의 작은 **설정하러 가기** 버튼은 기존 사냥 칙령의 반복 탭을 열고, 닫으면 같은 결과창으로 돌아온다.

관련 Edit Mode 범위 107개가 종료됐다. 초회 실패 5개는 신규 UI 테스트가 Edit Mode에서 런타임 `Destroy`를 호출한 정리 오류였다. 기존 `DestroyImmediate` 종료 정리를 사용하도록 테스트만 고쳐 해당 5개를 재검사했고 **5 통과·0 실패·0 건너뜀**이었다. 초회 MCP 상세 집계가 반환되지 않았으므로 107/107 통과로 표기하지 않는다. 공통 UI 소유 계약과 계약 검사 **11/11**은 통과했다. 전체 게임 검사는 반복하지 않았다.

macOS 개발 빌드는 오류 0·경고 191로 성공했다. 한 번의 통합 조작 검사에서 기존 HUD 30개 조건, 결과 활동·설정 바로가기의 한국어·영어 10개 화면 조합을 확인했다. 실제 균열이 자연 종료된 88.6초의 표본 114개가 완료 기록·최종 시간·비율과 같고, 별도 `GameStore`로 저장 파일을 다시 읽어도 같았다. 실제 레이캐스트 뒤 합성 포인터로 그래프와 설정 탭을 열고 닫아 같은 결과·전투·아이템 위치가 유지됨을 확인했다. 마을·훈련에는 활동 HUD가 남지 않고 기존 훈련 DPS를 유지했다. 성공 표식 `HELLSCRIPT_RIFT_ACTIVITY_SMOKE_OK`, 종료 코드 0, 관리 예외 0개였다. 실사용 저장·설정 파일 28개는 해시가 유지됐다. Unity 분석 캐시와 공유 Editor 테스트 출력은 별도다. 모바일 안전 영역은 macOS 모의이며 실기기·WebGL은 이번에 검증하지 않았다.

추가 요청의 15단계 경계는 **관련 6/6**(14·15·16단계 설정 상태와 영어 표 검사)으로 확인했다. 마지막 개발 빌드는 오류 0·경고 193이다. 기존 HUD 30개 조건은 그대로 재사용하고 변경된 결과창 10개 조건만 따로 확인했다. 14단계에서는 설정이 저장되어 있어도 흐리게 표시되고 제한 안내만 2초 뒤 사라지며 바로가기·설정 안내 페이지는 열리지 않는다. 15·16단계의 설정된 상태는 정상 밝기이고, 15단계의 미설정 상태는 작은 설정 버튼과 같은 결과로 돌아오는 흐름을 유지한다. 경계 UI에만 14·15·16단계 픽스처를 사용했고 활동 시간·피해·스킬 시점은 자연 종료한 실제 1단계 전투 기록이다.

[경계 검사](RiftResultActivityEvidence20261003/editmode-repeat-gate.json) · [최종 빌드](RiftResultActivityEvidence20261003/native-build-gated.json) · [초기 HUD 검사 재사용](RiftResultActivityEvidence20261003/runtime-initial.txt) · [한국어 14단계 안내](RiftResultActivityEvidence20261003/repeat-locked-1600x900-ko.png) · [세로 영어 14단계 안내](RiftResultActivityEvidence20261003/repeat-locked-440x956-en.png).

[검증 범위와 소스 해시](RiftResultActivityEvidence20261003/validation.json) · [초회 검사](RiftResultActivityEvidence20261003/editmode-first.json) · [실패 항목 재검사](RiftResultActivityEvidence20261003/editmode-retest.json) · [빌드](RiftResultActivityEvidence20261003/native-build.json) · [조작 기록](RiftResultActivityEvidence20261003/runtime.txt) · [최종 누계](RiftResultActivityEvidence20261003/final-feedback.json) · [실제 곡선](RiftResultActivityEvidence20261003/final-activity-samples.json).

[세로 한국어 결과 그래프](RiftResultActivityEvidence20261003/result-activity-440x956-ko.png) · [가로 한국어 결과 그래프](RiftResultActivityEvidence20261003/result-activity-956x440-ko.png) · [PC 영어 결과 그래프](RiftResultActivityEvidence20261003/result-activity-1600x1000-en.png) · [한국어 설정 바로가기](RiftResultActivityEvidence20261003/repeat-shortcut-1600x900-ko.png) · [세로 영어 설정 바로가기](RiftResultActivityEvidence20261003/repeat-shortcut-440x956-en.png) · [한국어 반복 탭](RiftResultActivityEvidence20261003/repeat-tab-1600x900-ko.png) · [가로 영어 반복 탭](RiftResultActivityEvidence20261003/repeat-tab-956x440-en.png).

## 누적 그래프 변경 검증

누적 그래프 변경은 관련 Edit Mode **61개 범위**에서 확인했다. 0.4초 파밍 두 번의 0.8초 합산과 그 사이 수평 구간, 초 경계, 장시간 표본 제한과 시작점 보존, 실제 시간축, 저장 누계에서 재개, 기존 DPS의 균등 간격 및 전투 결과 불변을 포함한다. 초회 60개는 통과했고, 새 좌표 검사의 Reflection 오버로드 호출 오류 1개는 테스트 호출만 고쳐 해당 1개 재실행에서 통과했다. 성공한 나머지 검사는 반복하지 않았다. 이번 수정은 관련 범위 검사로 검증했으며 전체 Edit Mode 검사는 다시 실행하지 않았다. 아래 초기 전체 검사 기록은 당시 결과다.

macOS 개발 빌드는 오류 0·경고 225로 성공했다. 실제 균열의 그래프 끝점이 누계와 같고 시간·누적 값이 단조 증가하는지 확인했다. 한국어·영어 × 세로 440×956·가로 956×440·PC 16:9/16:10/21:9 × 지도 3개 모드의 30개 조건에서 100% 막대·접기·드래그·별도 위치 저장·재구성 유지·안전 영역·잘림을 한 번 확인했다. 표시 조작은 전투 누계를 바꾸지 않았고 기존 훈련 DPS도 유지됐다. 합성 macOS uGUI 입력과 안전 영역 시뮬레이션이며 모바일 실기기는 미검증이다.

[관련 검사](RiftActivityCumulativeEvidence20261002/focused-tests.json) · [빌드](RiftActivityCumulativeEvidence20261002/native-build.json) · [조작 기록](RiftActivityCumulativeEvidence20261002/runtime.txt)

| 화면 | 한국어 | 영어 |
| --- | --- | --- |
| 세로 440×956 | [누적 화면](RiftActivityCumulativeEvidence20261002/activity-440x956-ko.png) | [누적 화면](RiftActivityCumulativeEvidence20261002/activity-440x956-en.png) |
| 가로 956×440 | [누적 화면](RiftActivityCumulativeEvidence20261002/activity-956x440-ko.png) | [누적 화면](RiftActivityCumulativeEvidence20261002/activity-956x440-en.png) |
| PC 16:9 | [누적 화면](RiftActivityCumulativeEvidence20261002/activity-1600x900-ko.png) | [누적 화면](RiftActivityCumulativeEvidence20261002/activity-1600x900-en.png) |
| PC 16:10 | [누적 화면](RiftActivityCumulativeEvidence20261002/activity-1600x1000-ko.png) | [누적 화면](RiftActivityCumulativeEvidence20261002/activity-1600x1000-en.png) |
| PC 21:9 | [누적 화면](RiftActivityCumulativeEvidence20261002/activity-1680x720-ko.png) | [누적 화면](RiftActivityCumulativeEvidence20261002/activity-1680x720-en.png) |

![실제 균열의 시간순 누적 그래프](RiftActivityCumulativeEvidence20261002/activity-natural-1600x900-ko.png)

## 초기 구현 검증

관련 Edit Mode 검사 **46/46**, 공통 UI 검사 **11/11**과 최종 소유 계약 검사를 통과했다. 실제 틱의 네 목적 구분, 파밍 접근의 이동 오분류 방지, 일시정지·포탈 제외, 100% 반올림, 초 경계와 표본 제한, 저장과 이전 버전 재개, 기존 전투와 동일한 결과 및 DPS와 독립된 위치 저장을 포함한다.

최종 전체 Edit Mode 검사는 한 번 실행해 **5,037개 중 4,909 통과·128 실패·0 건너뜀**이었다. 실패 128개 모두 이전 검증 파일의 실패 항목과 일치한다. 전체 통과로 보고하지 않는다. [전체 결과](RiftActivityTimeEvidence20261002/editmode-final.json)와 [기존 실패 대조](RiftActivityTimeEvidence20261002/failure-baseline-comparison.json)를 보존했다.

macOS 개발 빌드는 오류 0·경고 190으로 성공했다. 실제 균열의 자연 틱·실시간 갱신·일시정지 제외, 합계 100% 막대, 그래프 입력 비차단, 제목줄 레이캐스트와 드래그·접기, DPS 독립성, 위치 파일 재조회, 화면 재구성 유지와 경계 제한을 확인했다. 한국어·영어 × 아래 5개 화면 × 지도 3개 모드를 통과했다. UI 조작 전후 전투 집계가 같고 마을·훈련에 활동 패널이 남지 않으며 훈련 DPS도 유지했다. 초기 영어 항목 잘림은 항목명과 시간·비율을 두 줄로 나눠 고쳤고, 직접 영향을 받는 화면 조작 검사를 재실행했다. 집계 코드가 그대로여서 전체 검사는 반복하지 않았다.

[관련 검사](RiftActivityTimeEvidence20261002/focused-tests.json) · [빌드](RiftActivityTimeEvidence20261002/native-build.json) · [런타임 기록](RiftActivityTimeEvidence20261002/runtime.txt). macOS의 합성 uGUI 입력과 안전 영역 시뮬레이션이며 모바일 실기기 검증은 하지 않았다.

| 화면 | 한국어 | 영어 |
| --- | --- | --- |
| 세로 440×956 | [화면](RiftActivityTimeEvidence20261002/activity-440x956-ko.png) | [화면](RiftActivityTimeEvidence20261002/activity-440x956-en.png) |
| 가로 956×440 | [화면](RiftActivityTimeEvidence20261002/activity-956x440-ko.png) | [화면](RiftActivityTimeEvidence20261002/activity-956x440-en.png) |
| PC 16:9 | [화면](RiftActivityTimeEvidence20261002/activity-1600x900-ko.png) | [화면](RiftActivityTimeEvidence20261002/activity-1600x900-en.png) |
| PC 16:10 | [화면](RiftActivityTimeEvidence20261002/activity-1600x1000-ko.png) | [화면](RiftActivityTimeEvidence20261002/activity-1600x1000-en.png) |
| PC 21:9 | [화면](RiftActivityTimeEvidence20261002/activity-1680x720-ko.png) | [화면](RiftActivityTimeEvidence20261002/activity-1680x720-en.png) |

![실제 균열에서 이동·공격·회피·파밍 그래프와 100% 비율 막대](RiftActivityTimeEvidence20261002/activity-natural-1600x900-ko.png)
