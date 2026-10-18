# 캐주얼 UI 공통 표시 API — 2026-10-17

기준 `dbec4dbed084596471675f375c1d1ac517b4b86c`. ① 광장·② 출석/일일 퀘스트·④ 확인/알림의 화면별 opt-in 표현을 지원한다. 전역 스킨·폰트·배율·입력·저장·거래 규칙을 바꾸지 않는다.

- `UiButton.SetCompleted(bool)`: Casual 완료 보상은 `interactable=false`를 유지하면서 흰/크림 면을 표시한다. 일반 Disabled/Locked, 기존 label alpha `.55`, legacy/close/Google 표현은 유지한다.
- `UiButton.SetCasualAppearance(float? radius=null,float? borderWidth=null,float? shadow=null,Color? edge=null)`: 해당 Casual 버튼의 모서리·선·그림자·활성 테두리만 선택한다. 값은 Canvas 로컬 단위다. 인자 없는 호출은 기존 기본값을 복원한다. Disabled/Locked의 의미 색과 hit rect는 유지한다.
- `StorageSurface.SetSkin`의 기존 인자 뒤에 `Color? fill=null, Color? fillEnd=null`을 추가했다. 생략 시 기존 크림 그라데이션, fill만 전달하면 단색, 두 색은 그라데이션이며 null skin은 legacy를 복원한다.
- `UiSkin.Casual.AttendanceReadyFill`/`AttendanceMilestoneEdge`는 승인 색 `#e9f9e4`/`#e0a400`이다. `PlazaSecondaryInk`/`PlazaDetailInk`는 `#697190`/`#4a5272`를 OnLight로 제공한다. 기존 D4 토큰의 중복 색은 추가하지 않는다.

동일한 값의 재설정은 기존 전환을 재시작하지 않는다. 공통 Face를 재사용하며 화면별 가짜 활성 버튼이나 덧그린 별도 face를 요구하지 않는다.

검증: 실제 Unity Bee 참조와 analyzer 7개를 유지한 독립 Roslyn Runtime 851소스/296참조·Tests 268소스/308참조 컴파일 오류 0, 변경 소스 경고 0. Tests는 이번 Runtime 참조를 사용했다. 집중 NUnit 회귀 8개는 컴파일했으며 **실행은 아직 하지 않았다**. Runtime의 기존 경고 225개는 구분 보존했다. 입력 해시 전후 동일. Unity/전체 NUnit/게임 빌드/원본 쓰기 0.

정적 근거: 통합 작업공간 `ui-six-integration-20261017/validation/common-api/`의 completion·assembly receipts·input pins. 실제 실행 검증은 전체 화면 동결 후 승인된 단일 통합 묶음에 포함한다.

## 전문 대조 후속 계약

초기 `559abd4f` 이후 전문 c/f/l 대조로 다음 인스턴스 API를 보완했다. 위 초기 컴파일 결과를 이 후속의 실행/타입 통과로 재사용하지 않는다. 최종 타입 검사와 실행은 후속 입력 동결을 기다린다.

- `UiWindowFrame.SetSkin(UiSkin skin,float? borderWidth=null,float? shadow=null)`: null은 기존 배율 기반 line×3/4, 명시값은 Canvas local 고정값이다. 광장 축소 카드가3/4를 소비한다.
- `SetCasualAppearance`의 명시 radius/borderWidth/shadow는 CasualClose도 지원한다. 인자를 생략하면 기존 원형/선/그림자를 유지하며 위험 의미 색은 덮지 않는다.
- `UiSkin.WithInteractionStates()`는 불변 scope다. Action hover `#fff6c2`, Primary hover `#a2e891→#5cc865`,3local 눌림과 그림자0, Disabled 그림자0을 적용한다. `ApplyChrome`이 해당 scope를 Face에 전달하며 기본 Casual/legacy는 바꾸지 않는다. 직접 자식의 글자/그림도 눌림 시 이동하고 해제·비활성 때 복구한다.
- `WithButtonTops(selected,danger)`/`SelectedTop`/`DangerTop`는 선택/위험 상단색을 창별로 지정한다. 사냥 칙령만 `#ffe26b`/`#ff8b8e`를 쓰며 장비 아트 scope를 합쳐도 보존된다.
- ⑤ 전용 공개 토큰: `EdictCoinShade`는 기존 `GoldF08A00A1000`, `EdictHover`는 `GoldFFF6C2A1000`을 재사용한다. `EdictChangedInk #b86200`, `EdictFactInk #8a5600`, `EdictSuccessFill #e9f9e3`, `EdictScrollTrack #efe6cf`, `EdictOffFill #eef0f6`, `EdictQuietFill #f4f6fa`, `EdictEmptyFill #f7f8fb`, `EdictSocketFill #e9edf6`만 중앙 UiTheme에 추가했다. 작은 글자의 대비는 호출부의 OnLight 규칙을 유지한다.

English: [Shared presentation APIs](Casual_UI_Shared_APIs_20261017.en.md).
