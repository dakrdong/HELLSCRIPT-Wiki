# 튜토리얼 딤·전투 HUD 글자 가독성 (U3) — 2026-10-21

[English](Guide_Dim_And_HUD_Readability_20261021.en.md). 사용자 테스트(main c6354cbf)에서 나온 보고 3·4를 한 묶음으로 고친 기록이다.

## 보고와 사용자 답변
- 3: 튜토리얼 중 왼쪽 위 글씨(목표·시간·표적·준비)와 중앙 위 보스 줄이 밝은 바닥에 묻혀 읽히지 않는다. 글자를 줄이지 말고 읽히게 한다.
- 4: 세로 화면의 보물 고블린 우선순위 수업이 딤 처리되어 있지 않다. 사용자 답변: 딤은 **옮길 카드와 도착 위치(드래그 안내 영역)와 설명 패널만 밝게** 하고 나머지 전투 영역은 어둡게 한다. 고블린 행은 창 아래에 잘리지 않고 완전히 보여야 한다.

## 확인한 사실
1. **튜토리얼은 옛 전투 HUD를 쓴다.** `GameUI.CasualCombatHud`는 `!tutorial`일 때만 참이라 튜토리얼과 훈련장은 외곽선 없는 옛 줄(`GameUI.BattleLayout.cs`)을 그린다. 사용자 스크린샷의 탄색 글자가 그것이다. 일반 전투의 캐주얼 HUD에는 이미 남색 이중 외곽선(`CasualBattleHudStyle.WorldOutline`)이 있다.
2. **밝은 바닥에서 옛 줄의 대비는 1.2~1.4 : 1이다.** `Docs/Implementation/GuideReadability20261021Evidence/`의 측정(글자색과 글자 둘레 1~3픽셀의 WCAG 대비, 밝은·중간·어두운·타일·사용자 스크린샷 바닥)에서 어두운 바닥만 8.7 : 1이다. 남색 외곽선 + 밝은 글자(게임에 넣은 굵기: 17 이상 1.0, 미만 0.8)는 같은 바닥에서 중앙값 8.6~8.7 : 1, 아래쪽 10%가 2.7~4.0 : 1이다(측정 영역이 얇은 외곽선보다 넓어 아래쪽 10%는 보수적이다). 받침판만 깔면 1.4~2.2 : 1로 모자라고, 외곽선 + 받침판은 아래쪽 10%가 4.3~5.7 : 1이지만 화면 한쪽을 어둡게 덮는다. 그래서 외곽선만 쓴다.
3. **딤이 없는 상태가 있었다.** `PrologueGate`는 열린 구멍(조작 대상)이 없으면 딤 조각을 만들지 않는다. 보물 카드가 나타나는 0.6초 동안 `GuideTargetStep`이 대상을 null로 돌려주므로 그때 화면은 딤도 안내도 없이 막기만 했다. 사용자 스크린샷 2가 그 상태(안내 문구가 "몬스터 등급에서 '보물 몬스터'를…" 중간 문구)이고 보물 행이 목록 아래에서 잘려 있다. 대기 중에는 `EnsureVisible`도 부르지 않았다.
4. **설명 단계는 화면 전체를 밝은 영역으로 넘겼다.** `ShowMessage(..., root, ...)`의 `bright`가 루트라서 설명 중에는 딤이 사실상 없었다.
5. **에디터 전용 함정이 보물 연출에 있었다.** `GuideTreasureReveal.Awake`와 드래그 유령(`HuntEdictOrderCard`)이 `GetComponent<CanvasGroup>()??AddComponent`를 썼다. 에디터에서는 없는 컴포넌트가 Unity의 `==`에만 null로 보이므로 `??`가 추가하지 않고 다음 접근에서 `MissingComponentException`이 난다. `CasualEdictSurfaceTests.GuideTreasureReveal…`가 main에서 이 예외로 실패하고, 기존 `EditorNullPatternTests`가 같은 함정을 가리킨다. 플레이어 빌드에서는 실제 null이라 스모크가 통과했다.

## 가설 (실행으로 확인하지 못함)
- 사용자의 에디터 실행에서 대기 문구가 오래 남았다면 5의 `Awake` 예외가 연출 시계를 멈췄을 가능성이 있다. 에디터 플레이 모드로 재현하지는 않았다. 어느 쪽이든 아래 수정으로 대기는 1.5초를 넘기지 못한다.

## 변경
- `PrologueGate`: 대상이 없어도 딤을 유지하는 옵션(`dimWithoutTarget`; 조작 막기·딤은 유지하고 `MissingGrace` 뒤에는 함께 풀린다), 밝게 둘 영역 목록(열린 구멍, 띠, 드래그 길, 합친 추가 영역), 카드와 도착 위치를 잇는 **드래그 길**(`Lane`: 두 사각형의 합집합 + 6픽셀), 딤 조각 32개, 한 프레임 상태를 적는 `Describe()`, 프레임당 부재 시간 0.25초 제한(긴 프레임 하나가 유예를 한 번에 소진하지 못하게).
- `GameUI.GuideTargetLesson`: 모든 대상 단계는 딤 유지. 보물 카드 연출 중에는 그 카드만 밝게 두고 목록 안에 완전히 보이게 한 뒤 기다린다. 기다림은 연출 시계 또는 1.5초 중 먼저 끝나는 쪽까지다. 드래그 단계는 카드와 도착 위치를 한 화면에 넣고(`UiVisibility.EnsureVisible(target, also)`), 들어가지 않으면 카드를 먼저 완전히 보여 준다. 설명 단계는 설명 패널과 몬스터 위 순서 표시만 밝게 한다.
- `GuideTreasureReveal`, `HuntEdictOrderCard`: `TryGetComponent`로 교체.
- 옛 전투 HUD(`InkLegacyBattleText`): 줄마다 캐주얼 금색·흰색 글자와 남색 이중 외곽선. 17 미만 크기는 얇은 외곽선(0.8)이다. 글자 크기·배치는 그대로. 한 번 만든 줄에 한 번만 적용한다.

## 검사
- 집중 EditMode: `PrologueGateTests`(세로 5장 목록 재현: 5번째 카드가 잘림 → 스크롤 뒤 카드와 도착 위치가 함께 완전히 보임, 들어가지 않을 때 카드 우선, 다른 목록 폴백, 대기 중 딤과 유예 뒤 풀림, 드래그 길·설명 밝은 영역·딤 조각 수 한도, 기존 입력 차단·설명 넘김 유지), `CasualEdictSurfaceTests`, `EditorNullPatternTests`.
- `RuntimeGuideTutorialSmoke`: 대상 수업에서 `gate-trace.txt`(0.25초 간격: 단계·연출 시계·카드 노출 비율·열린 구멍·밝은 영역·딤 조각)를 남기고, 기다리는 동안 딤이 있는지, 기다림이 3초를 넘지 않는지, 카드가 완전히 보이는지, 드래그 길이 있는지 확인한다.

## 남은 일 · 사용자 결정
- 에디터 플레이 모드에서 대기 상태가 실제로 `Awake` 예외 때문이었는지는 확인하지 않았다.
- `RiftGearSettingsWindow.cs:22`에 같은 에디터 함정이 남아 있다(자동 선택 설정 담당 세션에 알림).
- 일반 전투 HUD의 보스 줄은 이미 캐주얼 외곽선이라 바꾸지 않았다.

## 실행 결과
- macOS 개발 빌드(`ProjectBuilder.BuildMac`) 성공. `RuntimeGuideTutorialSmoke` 통과(결과 파일 `smoke-*.txt`, 실패 파일 없음): 한국어 942×1510(사용자 스크린샷 크기), 한국어 440×956, 영어 956×440. 합성 포인터와 가속 틱이라 실제 속도·사람의 이해·실기기 주장은 하지 않는다.
- 보물 수업 `gate-trace-ko-942x1510.txt`: 연출 시계 0.35→0.57초, 기다림 0.57초로 끝나고, 기다리는 동안 딤 조각 4개·밝은 띠는 보물 카드, 카드 노출 비율 1.00. 설명 단계는 딤 조각 13~15개(설명 패널과 몬스터 위 순서 표시만 밝음). 플레이어 빌드는 `Awake` 예외가 없어 이 환경에서 대기 정체는 재현되지 않았다.
- 캡처(`GuideReadability20261021Evidence/`): `ko-942x1510-treasure-waiting.jpg`(대기 중 딤 + 카드 완전 노출), `ko-440x956-treasure-drag.jpg`(드래그 길만 밝음, 1번~5번 행이 한 화면), `en-956x440-treasure-waiting.jpg`, `ko-942x1510-treasure-explanation.jpg`, `hud-after-fight-crop.jpg`(보스 줄이 바닥 위에서 남색 외곽선), `hud-current-vs-ring.jpg`와 `contrast.txt`(바닥별 대비).
- 집중 EditMode: 첫 실행 133개 중 131개 통과. 실패 1은 `EditorNullPatternTests`가 가리키는 장비 담당 파일 두 곳(`GameUI.GuideGearLesson.cs:72`, `RiftGearSettingsWindow.cs:22`)이고 이 작업이 만든 것이 아니다. 실패 2는 대기 딤 테스트가 프레임 길이에 따라 흔들린 것이라 프레임당 부재 시간을 0.25초로 제한하고(`PrologueGate`) 다시 실행했다. 결과: `PrologueGateTests`와 `CasualEdictSurfaceTests` 49개 전부 통과(보물 연출 테스트 포함). `EditorNullPatternTests`는 위 장비 담당 두 곳 때문에 계속 실패한다(이 묶음의 검사 범위 밖).
- 정적 검사: `check_ui_contract.py --verify` 통과(15개), `check_ui_refresh.py` 통과(14개 바인딩 보호, UI 소스 235개).
