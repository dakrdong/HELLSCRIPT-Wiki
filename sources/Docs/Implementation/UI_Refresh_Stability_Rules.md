# HELLSCRIPT UI 갱신 재발 방지

확인일: 2026-10-03

기존 Shared UI 계약과 CPU Performance Review Rules에 추가하는 이번 조사 원인만 기록한다. 상세 범위와 미검증 항목은 [UI 갱신 조사](UI_Refresh_Stability.md)를 따른다.

1. **저장 성공과 화면 변경을 구분한다.** 주기적 `Save`도 `Committed`를 발행한다. 새 저장 subscriber는 표시 의존 데이터를 비교한 뒤 갱신한다. 일반 Save에도 실제 화면 변경이 포함될 수 있으므로 모든 `save`를 무시하지 않는다. inventory/build/presets/보호 참조/해금/가용 상태 등 해당 화면의 읽기를 감사하되 timestamp·다른 콘텐츠 상태를 무조건 키에 넣지 않는다. domain 거래는 기존 live 상태를 다시 검사한다.
2. **동일 버튼 상태 지정은 시각 전환을 다시 시작하지 않는다.** role/chosen/locked/chrome/availability/hover/pressed/focus가 같으면 기존 진행률과 mesh를 유지한다. 첫 mesh 전에는 최종 상태로 시작한다. 실제 입력/선택/가용 상태의 변화를 제거해서 해결하지 않는다.
3. **이미 반영한 저장 알림을 다시 그리지 않는다.** 명시적 거래·탭·locale·layout repaint 후 기준 상태를 기록한다. 비동기 저장 알림은 모아서 한 번만 반영하고, 누름·터치·드래그·입력 필드 편집·dropdown·child modal 중에는 배경 컨트롤 교체를 보류한다. 다시 그린 뒤 이름 경로가 남아 있으면 이전 입력 modality의 focus와 owner의 스크롤 복원을 보장한다.

변경 리뷰에서 `python3 tools/check_ui_refresh.py`와 공통 `StoreViewBindingTests`를 실행한다. guard 존재 검사는 실제 의존 키의 정확성이나 전 페이지 시각 검사 대신이 아니다. 새로운 창/탭/팝업을 source checklist에 넣고 무변경 자동 저장, 실제 값 변경 일반 Save, 누름 중 저장, 중첩 창, 반복 탭 이동/재진입, 빠른 클릭, KO/EN, 저장 실패·중복 receipt를 해당 runtime에서 확인한다.

성능을 주장할 때 동일 source/fixture/화면 조건에서 최소 3회 실제 전후 binary를 실행하고 repaint·button 생성/제거·CPU·GC 원자료를 남긴다. 동일 수정 binary에서 dependency key만 끈 합성 대조군은 별도 표시한다. 값이 아직 없으면 비용 증가/감소 여부는 미측정으로 기록한다.

최종 native 통합 검사에서 `AccountGuide.tutorialRun.realTime` 증가가 장비 키를 바꾸어 자동저장 때 컨트롤을 교체하는 원인을 추가 확인했다. 장비 표시 키에는 안내 객체 전체나 살아 있는 전투 기록을 넣지 않는다. 실제 판매·보관·해제 보호에 사용하는 `Tutorials.Mandatory(account)`와 `armorId`만 유지한다. `LiveTutorialTimeKeepsEquipmentControlsButArmorProtectionStillRefreshes`는 실제 튜토리얼 저장·시간 증가가 버튼을 유지하면서 갑옷 보호 변경은 반영하는지 확인한다.
