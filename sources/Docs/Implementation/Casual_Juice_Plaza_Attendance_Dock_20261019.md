# 광장·출석·도크 연출 보강 — 2026-10-19

[English](Casual_Juice_Plaza_Attendance_Dock_20261019.en.md)

기준: 병합된 `main`(`8e57a374`) 위에서 #144·#146이 정리한 광장·도크·출석 화면에 절제된 연출 세 가지를 더했다. 색·글꼴·문구·저장 키·거래·레이아웃 규칙은 바꾸지 않았다. 확인창·토스트는 [별도 기록](Casual_Notice_Juice_20261019.md)에서 이미 다뤘다.

## 적용

- **광장 상호작용 카드**: 주민·시설 가까이 가면 카드가 올라오며 나타나고(`CasualNoticeEntrance`, .28초), 다른 시설로 옮겨 가도 한 번 더 나타난다. 위치·배율은 카드 재배치 때마다 `SetRestingPosition`/`SetRestingScale`로 넘겨 재생 시계를 끊지 않는다(공통 부품 규칙).
- **출석 수령 도장**: 수령한 날의 `수령 완료` 도장이 크게 찍히며 나타난다(`UiPopIn`, .32초, 약간의 되튐). 모두 받기는 날짜 순서대로 .07초씩 어긋나게(최대 8칸) 찍힌다. 수령 직후 창이 다시 그려져도 시계를 기억해 이어서 재생하며, 이미 받은 날과 다른 쪽(7일/28일) 탭에는 재생하지 않는다.
- **콘텐츠 도크**: 바로가기가 펼쳐질 때 감속하며 자리 잡고, 화살표는 살짝 지나쳤다 돌아온다(`ContentDockView.Decelerate`, `UiPopIn.EaseOutBack`). 펼침 순서·시간(.22초)·`Settled` 판정은 그대로다.
- 새 부품 `UiPopIn`은 한 번만 재생하고 끝나면 `enabled=false`가 되어 이후 프레임 비용이 없다(`AgentRules/CPU_Performance.md`). 시계는 실시간(unscaled)이며 Editor에서는 재생하지 않고 최종 모습만 그린다.

## 일부러 하지 않은 것

- **도크 `!` 배지 팝**: `ReflowEventButton`이 매 프레임 배지 위치·피벗을 다시 놓아 배율 연출이 튄다. 재배치 규칙을 바꿔야 해서 따로 정한다.
- **보상 수량 올라가기·광장 말풍선 팝·창 닫힘 연출**: 각각 다른 갱신 경로(저장 후 갱신, 월드 말풍선 배치, `Close`)를 건드려 이번 범위를 넘는다.
- 출석 창 전체의 열림 연출은 공통 `ContentWindowView`를 바꿔야 해서 넣지 않았다.

## 검증

- 집중 EditMode 72개 통과: `UiPopInTests`(신규 7) · `TownPlazaLayoutTests`(19) · `AttendanceSkinTests` · `AttendanceTests` · `AttendanceArtTests` · `CasualNoticeLayoutTests`(15). 신규 테스트는 되튐 곡선의 양 끝과 넘침, 시작 크기·투명도와 안착·`enabled=false`, 지연된 도장, 재그려진 창에서 이어 재생, 재시작 때 원래 배율 유지, Editor에서 재생하지 않음, 도크 감속 곡선을 확인한다.
- 정적 UI 계약 15개와 갱신 14개 바인딩 통과.
- 미검증: 실제 재생 모습과 체감(Editor에는 프레임 시계가 없다), 개발 플레이어 스모크(`RuntimePlazaSmoke`·`RuntimeContentDockSmoke`·`RuntimeAttendanceSmoke`), macOS 빌드, 실기기·터치. 전체 EditMode는 실행하지 않았다.
