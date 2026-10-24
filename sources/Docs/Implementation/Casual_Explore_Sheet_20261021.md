# 사냥 칙령 탐색 탭 캐주얼 적용 — 2026-10-21

[English](Casual_Explore_Sheet_20261021.en.md)

기준: 캔버스 "HELLSCRIPT 사냥 칙령 · 탐색 · 캐주얼"(98a2LCvDqd1ftBG8uGt4CW)의 구성 명세 g~m절. 시작 `origin/main` `c6354cbf`. 전투 HUD 사냥 칙령 시트(`GameUI.EdictSheet`)의 **탐색 탭 표현만** 어두운 레거시 시트에서 캐주얼 압축 카드 팝업으로 바꿨다. 옵션 의미, 저장 키(`explore.*`), 저장·되돌리기·재생·거래(`SheetApply`/`SheetCommit`/`CommitHuntEdict`), 튜토리얼, 전투 일시정지 규칙, 큰 창(`HuntEdictWindow`)은 그대로다. 반복 설정 탭은 아직 레거시 시트, 프리셋 탭은 안내 토스트 그대로다.

## 적용

- 탭 열의 탐색을 누르면 압축 카드(`BuildCompactEdictSurface`, 생존 팝업과 같은 판·이름표·×·되돌리기·재생)가 열린다. 카드 안에 그룹 칩(탐색·균열 진행 / 일반·봉인 상자 / 저주 상자 / 성소), 그룹 설명 한 줄, 스크롤되는 옵션 카드가 있다. 하위 메뉴 목록 단계는 없다(전투·생존과 달리 지금도 그룹 칩 방식이었다).
- 새 `GameUI.EdictExplore.cs`: `ExplorePanel` · `BuildExplorePanel` · `BuildExploreBody`. `BuildEdictSheet`/`BuildSheetRows`에 `sheetTab=="explore"` 분기 두 줄.
- 새 `GameUI.EdictCasualRows.cs`(다음 작업이 재사용): `BuildCasualSheetFrame`(이름표·×·본문·되돌리기·재생) · `BuildCasualGroupChips` · `AddCasualRow`(카드 한 장) · `AddCasualChips`(선택·토글·세트·스킬 칩, 줄 수에 맞춰 칩 높이가 늘어남) · `AddCasualNumber`(`AddSheetNumber` 그대로 + 최소·최대 글자) · `AddCasualOrder`(순위 원·이름·한 줄 설명·▲▼·≡가 있는 **끌어서 옮기는 카드**, 전투 탭 행동 우선순위와 같은 `HuntEdictOrderList`/`HuntEdictOrderCard`) · `CasualSheetHeight`(화면이 허락하는 카드 높이). `AddCasualRow`의 `attached` 인자는 같은 기능의 설정을 부모 카드 안(구분선 아래)에 붙인다.
- 기존 코드 변경: `AddSheetNumber`가 탐색도 compact로 취급(`ExplorePanel`)하고 접미사(초·m·%)를 `CompactNumberSuffix`로 정한다. `SurvivalSection`이 부모를 받을 수 있다(자식 옵션 들여쓰기용, 기본값은 이전과 같음).
- 사용자 피드백(2026-10-21, 탐색·균열 진행 그룹): ① 발견물 순서는 카드 없이 제목 + 끌어서 바꾸는 순서 카드(설명: 마지막 적 위치를 찾아감 / 적보다 먼저 챙김 / 적을 조사한 뒤 챙김) ② 시간 부족 기준은 시간 부족 스위치와 한 카드 ③ `시간 부족 시 회수 예외`는 팝업에서 뺌(저장값·시뮬레이션·사냥 칙령 창은 그대로) ④ `시간 부족 시 우회 중단` → `시간 부족 시 클리어 우선`. 일반·봉인 상자 그룹: `보스 후에도 상자 개봉` → `보스전 중에도 상자 탐색`. 이름은 옵션 정의의 라벨이라 사냥 칙령 창도 같은 이름을 쓴다(`en.txt` 갱신).
- 옵션 카드 이름은 레거시와 같다(`edict-sheet-row-*`, `edict-sheet-choice-*`, `edict-sheet-minus/plus/value-*`, `edict-sheet-up/down-*`, `edict-sheet-group-*`).
- 새 문구(`en.txt` 영어 포함): 순서 카드 설명 3개와 이름 변경 2개. 나머지는 기존 키다.

## 시안과 다른 점·결정 필요

- **일반·봉인 상자 그룹은 시안만 바뀌었다(m-7).** 사용자 요청으로 시안은 일반 상자·봉인 상자의 3지선다와 공용 `상자 우회 거리`를 종류별 `탐색 반경` 슬라이더(0~50m, 0m = 무시) 두 개로 바꿨다. 게임은 새 저장 옵션·종류별 반경 시뮬레이션·기존 값 이전·공유 코드/프리셋/진행 단계 변경이 필요해서 별도 작업으로 두었고, 지금 게임의 해당 그룹은 이전 칩+우회 거리 그대로다(이름 변경만 반영).

- 시안의 주석(옵션 설명)은 게임 안에 넣지 않았다(m-2). HUD 전용 설명 19문장과 영어 번역이 필요하고, `HuntEdictUi.json` help에 넣으면 전체 칙령 창에도 `?`가 생긴다.
- 코드 확인 결과 라벨만 다르고 동작이 같은 값 3쌍이 있다(m-1): 탐색 방식 `감지한 적` = `가까운 미탐색 지역`, 상자 `이동 경로에서 개봉` = `발견하면 우선 개봉`(일반·봉인), 성소 `발견하면 이용` = `필요한 효과일 때 이용`. 의미를 바꾸지 않는 범위라 그대로 두었다.
- 저주 상자 `전투 후 개봉`에서도 저장된 `최소 남은 시간`이 적용되지만 그 행은 `조건부 개봉`일 때만 보인다(m-3, 현행 유지).
- 세로 카드는 기존 압축 카드 자리(폭 416, 가운데)를 그대로 쓴다. 탭 열 위치는 별도 작업(m-5).
- 그룹 칩·옵션 카드에 아이콘은 없다(글자만). 자식 옵션 카드는 같은 칠에 왼쪽 14 들여쓰기만 다르다.

## 검증

- 집중 EditMode 32개 통과(배치, `-nographics`, Unity 6000.6.0f1, 마지막 실행): `EdictExploreMenuTests`(신규 14) · `EdictSurvivalMenuTests` 14 · `HuntEdictHudRowsTests` 4. 신규 테스트는 그룹 칩 4개와 보이는 옵션 전부의 카드(회수 예외 제외), 칩 한 번에 한 거래·되돌리기, 스위치가 자식 행(시간 부족 기준)을 같은 카드 안에 나타내고 숨김, 회수 예외의 저장값 보존, 순서 카드(끌기 부품 14개 확인·`KeyboardMove` 한 번에 한 거래·설명이 순서를 따라감)와 ▲▼ 이동, 숫자 단위(초·m·%), 칩 이름 유지, 자식 카드 안쪽에 컨트롤이 들어오는지, 이름 변경, 재생·되돌리기 초기 상태를 본다. 이전 실행에서 `LocalizationTests` 34개도 통과했다(en.txt 새 줄 포함).
- 첫 실행에서 `CasualEdictSurfaceTests`도 함께 돌렸다. `GuideTreasureRevealKeepsFadeAndScaleWithOptionalLegacyBorder` 2건이 실패했다(`GameUI.GuideTreasureReveal.Awake`의 `GetComponent<CanvasGroup>()??AddComponent` — Unity 객체에 `??`를 쓴 기존 코드). 이 변경은 그 코드와 테스트를 건드리지 않았고(`GameUI.GuideTargetLesson.cs` 변경 없음) 깨끗한 기준선에서 따로 돌려 보지는 않았다. 별도 확인이 필요하다.
- 캡처에서 자식 카드가 들여쓴 만큼 오른쪽으로 넘치는 문제를 발견해 고치고(`AddCasualRow`가 자식 카드 폭으로 컨트롤을 잰다) 테스트로 막았다.
- 정적 검사: UI 계약 `check_ui_contract.py --verify` 15개 통과, `check_ui_refresh.py` 바인딩 14개 통과.
- Editor에서 실제 팝업을 PNG로 그려(`Assets/HELLSCRIPT/Editor/EdictExploreEvidence.cs`, 640×480, 한국어) PC·가로·세로의 네 그룹과 시간 부족 켬·저주 상자 열지 않음/조건부 상태를 눈으로 확인했다.
- 미검증: 실제 전투 화면과 개발 플레이어 스모크(`RuntimeBattleQuickEditSmoke`의 탐색 경로), 라이브 전투 중 저장·재생, macOS 빌드, 실기기·터치, 영어 긴 문구 잘림(영어 캡처 안 함), 카드 등장 연출의 체감. 전체 EditMode는 실행하지 않았다.
