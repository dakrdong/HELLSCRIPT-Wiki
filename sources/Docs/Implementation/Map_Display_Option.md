# 지도 표시 방식 설정

작성일: 2026-09-28 · [English](Map_Display_Option.en.md)

옵션 → 화면 설정에 있던 ‘오버레이 지도’ 켜기·끄기 토글을 ‘지도 표시 방식’ 단일 선택으로 바꾸었다. 선택지는 세 가지이며, 전투 화면에는 선택한 방식의 지도 하나만 그린다. 선택은 이 기기에만 저장하고 계정 저장에는 섞지 않는다. 열린 전투에서도 선택하는 즉시 적용되며 전투를 다시 시작하지 않는다.

## 선택지

| 선택지 | 전투 화면에 그리는 것 | 설정에 표시하는 설명 |
| --- | --- | --- |
| 겹침 지도 / Overlay map | 기존 플레이어 중심 자동 지도(`GameUI.OverlayMap`의 `riftOverlay`, `RiftAutomap`)를 전투 화면 가운데에 겹쳐 그린다. | 플레이어를 중심으로 발견한 던전 구조를 화면 가운데에 겹쳐 표시합니다. |
| 미니맵 창 / Corner minimap | 기존 작은 지도 창(`GameUI.Rift`의 `AddRiftMinimap`, `RiftMinimap`)을 화면 모서리에 단독으로 그린다. | 화면 모서리의 작은 창에 지도를 따로 표시합니다. |
| 표시 안 함 / Hidden | 지도를 그리지 않는다. 탐색 기록은 계속 쌓인다. | 지도를 표시하지 않습니다. 탐색 기록은 계속 쌓입니다. |

모든 문구는 `Resources/Localization/en.txt`에 한국어와 영어로 등록했다. 기존 토글에만 쓰이던 문구 세 줄은 원본 코드에서 사라졌으므로 문구 표에서도 지웠다. 기기 저장 안내와 저장 실패 문구는 그대로 옮겨 사용한다.

## 설정 화면

`GameUI.ScreenSettings`의 화면 설정 본문에서 가시거리 다음, 화면 비율 앞에 배치한다. 제목 아래에 세 선택 버튼을 두고 각 버튼 아래에 설명을 붙인다. 버튼은 언어 선택과 같은 `BigButton`이며, 선택 상태는 화면 비율·언어 선택과 같은 `UiTheme.Choice`의 선택 역할로 표시한다. 새로운 색상이나 글꼴을 만들지 않았다. 버튼을 누르면 곧바로 기기 파일에 저장하고 전투 화면을 다시 배치한다.

저장 실패 안내와 저장 재시도 동작은 기존 토글과 같다. 기기에 쓰지 못하면 선택은 화면에 적용한 채로 “지도 표시를 적용했지만 기기에 저장하지 못했습니다. 저장을 다시 시도해 주세요.”를 표시하고 저장 재시도 버튼을 보인다. 재시도는 현재 선택을 다시 저장하며, 성공하면 안내와 버튼이 사라진다. 파일을 읽지 못한 경우에는 기본값으로 시작하고 원본 파일을 보존한다는 안내를 표시한다.


## 전투 화면의 모서리 창

오른쪽 위의 모서리 창은 세 방식 모두에서 유지한다. 창 안의 상자 수와 길찾기 오류 문구는 지도가 아닌 요소이므로 항상 보이고, 창을 누르면 확대 지도가 열린다. 지도는 미니맵 창 방식에서만 창 안에 그린다. 겹침 지도와 표시 안 함에서는 창이 문구 높이로 줄어든다.

이전 `main`에서는 이 창이 가로이면서 폭이 720 이상인 화면에서만 보였다. 세로 화면에서는 지도와 상자 수가 모두 없었다. 미니맵 창을 선택한 사용자는 방향과 관계없이 지도를 보아야 하므로, 이제 세로 화면에서도 창을 표시한다. 배치 규칙은 다음과 같으며, 단위는 화면 배율을 적용한 UI 단위다. 영웅은 카메라 화면의 가운데에 서 있으므로, 영웅의 위치는 안전 영역을 기준으로 그 가운데 점을 계산해서 사용한다.

### 가로 위치와 폭

- 창은 왼쪽 버튼 열(이벤트 18~108, 모험 안내 18~132)의 오른쪽인 140 이후에 둔다. 보통은 창의 오른쪽 끝을 화면 오른쪽에서 68(메뉴 버튼 옆에서는 120) 떨어뜨려서 콘텐츠 메뉴 열(폭 44, 오른쪽 여백 12)과 겹치지 않게 한다. 폭은 168이며 화면이 좁으면 줄어든다.
- 세로 화면에서 두 버튼 열 사이의 폭이 상자 수를 담지 못하거나, 세로 지도의 상한(화면 폭의 34%)만큼 지도를 그리지 못하면 창을 콘텐츠 메뉴 열 아래까지 넓힌다. 이때 창의 오른쪽 끝은 화면 오른쪽에서 12 떨어지고, 왼쪽 끝은 영웅보다 20 이상 오른쪽에 있다. 폭이 약 327보다 좁은 세로 화면이 여기에 해당한다. 휴대폰에서는 글자 크기를 키웠을 때 생기며, 글자 크기는 5% 단위로 바뀌며, 폭 360 기기는 약 110%, 폭 390 기기는 120%, 폭 430 기기는 135%부터 해당한다. 이 배치로 440×956·150%의 지도는 77에서 99로 커졌다. 이전 배치 식으로 계산하면 360×780·150%에서는 지도가 없고 상자 수가 잘렸으며, 390×844·150%에서는 지도가 44였다. 지금은 각각 80과 88이다.
- 이 배치에서는 창을 머리글보다 먼저 그린다. 그래서 콘텐츠 메뉴를 펼치면 펼친 바로가기 버튼이 창의 오른쪽 부분(지도 일부와 상자 수 끝)을 덮고, 포인터 입력도 바로가기가 받는다. 메뉴를 접으면 창 전체가 다시 보인다.
- 상자 수는 줄바꿈하지 않는다. 창의 폭은 이 균열에서 상자 수가 가질 수 있는 가장 긴 문구의 폭에 8을 더한 값 이상이다.
- 가로 화면에서는 창이 영웅과 같은 높이에 놓이므로, 필요하면 폭을 줄여서 창을 영웅보다 20 이상 오른쪽에 둔다. 스모크에서 확인한 가로 크기에서는 이 조건 때문에 폭이 줄어든 경우가 없다.

### 세로 위치와 지도 크기

- 창의 위쪽은 머리글 글줄 아래(가로 64, 세로 122, 폭 360 미만의 세로 158)에 있다. 아래쪽은 실시간 전투 기록보다 8(메뉴 버튼 옆에서는 4) 위에서 멈춘다. 이전 코드는 이 간격을 3까지 허용했으므로, 이번에 기록된 8에 맞게 고쳤다. 그 결과 2100×900·150%의 지도는 67에서 62로, 1600×900·150%의 지도는 116에서 111로 작아졌다. 전투 기록을 펼치거나 접으면 창을 다시 배치한다.
- 지도는 최대 160이며 창 폭, 화면 높이의 40%(가로) 또는 폭의 34%(세로), 남은 높이 안에 맞춘다. 40보다 작아지면 세로 화면에서는 지도를 그리지 않고, 가로 화면에서는 다음 규칙에 따라 창을 올린다.
- 가로 휴대폰에서 글자 크기가 커서 머리글 아래에 상자 수나 지도를 둘 공간이 없으면(956×440, 150%) 창을 메뉴 버튼 왼쪽 위로 올린다. 이때 지도는 상자 수 오른쪽에 두며, 956×440·150%에서는 지도 한 변이 47 단위(70.5픽셀)이다. 창과 겹치는 머리글 줄은 창 앞에서 끝난다.
- 창은 영웅을 가리지 않는다. 창이 영웅보다 20 이상 오른쪽에 있지 않으면, 창의 아래쪽이 영웅의 발보다 56 위에서 끝나도록 지도를 줄인다. 영웅의 머리가 발보다 약 48 위에 있으므로 8만큼 간격을 더 둔 값이다.

### 보스 상태 표시

보스 상태 표시는 창과 겹치지 않으며, 다음 순서로 자리를 정한다.

1. 왼쪽 버튼 열(140)과 창 사이에 240 이상이 남으면 보스 상태를 창 왼쪽에 둔다. 이때에도 왼쪽 끝은 140 이상이다. 이전 코드는 왼쪽 끝을 18까지 허용했기 때문에, 폭이 약 530~660인 가로 화면에서는 보스 상태가 이벤트·모험 안내 버튼 아래로 들어갔다. 이제 640×360에서는 보스 상태가 140에서 시작한다.
2. 보스전 중에 지도를 40 이상으로 남긴 채 창을 보스 상태 줄보다 8 위에서 끝낼 수 있으면, 지도를 그만큼 줄이고 보스 상태는 제자리에 둔다. 폭 600 미만의 가로 화면(보스 상태 줄 176)이 여기에 해당하며, 568×320에서는 지도 한 변이 71이다.
3. 두 조건이 모두 맞지 않으면 보스 상태를 창 아래로 내린다. 보스전 중에는 보스 상태가 영웅의 발보다 56 위에서 끝나도록 지도를 줄이며, 이때 보스 상태의 실제 높이를 사용한다. 보스 이름이 여러 줄로 나뉘면 보스 상태가 그만큼 높아진다. 보스 상태의 문구는 전투 중에 바뀌므로, 같은 화면 크기에서는 그 보스전에서 가장 높았던 높이를 기억한다. 문구가 늘어나서 보스 상태가 높아지면 지도를 한 번 더 줄이고, 문구가 줄어들어도 지도를 다시 키우지 않는다. 그래서 문구가 바뀔 때마다 지도 크기가 달라지지는 않는다. 줄인 지도가 40보다 작아질 때에만 그 보스전 동안 지도를 빼고 상자 수만 남긴다. 이때 보스 상태는 `main`과 같은 자리에 있으며, 보스전이 끝나면 지도를 다시 그린다.

이전 코드는 3번에서 지도를 줄이지 않고 곧바로 뺐다. 그래서 흔한 휴대폰의 세로 화면에서는 글자 크기가 100%여도 보스전 내내 지도가 없었다. 확인한 결과는 다음과 같다.

- 390×844(노치 모의) 100%: 지도를 한국어에서 75, 영어에서 52(보스 이름이 세 줄)로 줄여서 유지한다.
- 360×780 100% 영어: 지도를 67로 줄여서 유지한다.
- 440×956 100% 한국어: 지도 149를 그대로 두고 보스 상태를 창 아래에 둔다.
- 440×956 150% 영어: 폭 360 미만의 세로 화면이어서 보스 상태 줄이 212에 있고, 40짜리 지도를 둘 공간이 없으므로 지도를 뺀다.
- 휴대폰 세로 화면에서는 폭이 좁을수록, 위쪽 안전 영역이 클수록, 글자 크기가 클수록 지도를 줄일 공간이 줄어든다. 배치 식을 옮긴 계산에 따르면, 폭이 360보다 좁아지는 125% 이상에서는 거의 모든 휴대폰 크기에서 보스전 동안 지도를 뺀다. 100%에서도 폭 360에 위쪽 안전 영역이 47인 화면이라면 지도를 뺀다. 실행해서 확인한 크기는 위 목록의 크기뿐이다.
- 폭이 600~624인 가로 화면(예: 640×360의 105%, 667×375의 110%, 옆쪽 안전 영역이 24~32인 640×360의 100%)은 1번과 2번 조건을 모두 만족하지 못하므로 보스전 동안 지도를 뺀다. 이 경우도 계산으로만 확인했다.

### 길찾기 오류 문구

- 오류 문구는 문구 표(`Loc.T`)를 거쳐 선택한 언어로 표시한다. 이전 코드는 `RefreshHud`에서 원문을 그대로 넣었기 때문에 영어에서도 한국어 문구가 보였다. 이 문제는 `main`의 가로 화면에도 있었고, 이번에 세로 화면에도 창을 표시하면서 더 넓게 드러났다.
- 오류 줄은 먼저 한 줄로 배치한 다음, 상자 수와 실시간 전투 기록 사이에 들어가는 만큼 여러 줄을 쓴다. 문구가 다 들어가지 않으면 줄 끝에서 자르고 말줄임표(…)를 붙인다. 전체 문구와 상태 재확인 버튼은 확대 지도에 항상 있다.
- 오류 줄이 차지하는 만큼 지도가 줄어든다. 메뉴 버튼 옆으로 올린 가로 배치에서는 오류 문구에 창 폭 전체를 쓰므로 지도를 그리지 않는다. 이 동작은 이전과 같다.
- 440×956·150%에서는 전체 문구가 들어가고(한국어와 영어 모두 5줄) 지도는 99를 유지한다. 956×440·150%에서는 한 줄만 들어가서 “보스 등장 공간을 확보하지 …”와 “No room could be secur…”로 표시된다.

## 확대 지도로 들어가는 경로

세 방식 모두에서 확대 지도(`ShowRiftMap`, 전투 일시정지)에 두 경로로 들어갈 수 있다. 하나는 모서리 창을 누르는 경로이고, 다른 하나는 ☰ 관찰 메뉴의 ‘지도’다. 겹침 지도와 표시 안 함에서도 모서리 창을 남긴 이유는 상자 수를 계속 보여 주고, 기존의 누르는 동작을 그대로 쓰기 위해서다.

## 기기 저장과 이전 설정의 이전

`Core/MapDisplaySettings.cs`가 선택을 기기 설정 폴더의 `hellscript-map-display-v1.json`에 `{"version":1,"mode":"corner|overlay|hidden"}` 형식으로 저장한다. 저장 위치와 임시 파일을 거쳐 교체하는 쓰기 방식은 기존 겹침 지도 설정과 같고, 계정 저장 파일에는 쓰지 않는다.

기존 토글의 파일 `hellscript-overlay-map-v1.json`은 읽기만 한다. 새 파일이 없을 때 이전 값이 켜짐이면 겹침 지도, 꺼짐이면 미니맵 창으로 시작한다. 새 선택을 저장하면 새 파일이 우선하며, 이전 파일은 지우거나 고치지 않는다.

새 기기의 기본값은 미니맵 창이다. [첫 플레이 개선](First_Play_Improvements.md)의 D08에서 신규 기기의 겹침 지도가 기본으로 꺼지도록 바뀌었기 때문에, 오늘 새 기기에서 보이는 모습(겹침 지도 없음, 가로 화면의 모서리 지도)을 유지한다. [실시간 시야와 탐색 지도](Rift_Visibility_Expansion.md)에는 당시의 기본 켜짐이 기록되어 있지만, 이 값은 D08 이후의 동작과 다르다.


## 코드 변경 범위

- `Core/MapDisplaySettings.cs`: 세 방식, 기기 저장, 이전 파일 이전, 읽기·쓰기 실패 처리를 담당한다. `OverlayMapSettings.cs`를 대체했다.
- `GameController.Display.cs`: `OverlayMap` 대신 `MapDisplay`를 초기화한다.
- `GameUI.OverlayMap.cs`: 설정 선택 버튼과 즉시 적용을 담당하며, 겹침 지도는 겹침 지도 방식에서만 켠다.
- `GameUI.Rift.cs`: 모서리 창을 배치한다. 콘텐츠 메뉴 열 아래로 넓히는 배치, 상자 수의 폭, 영웅과의 간격, 오류 문구의 번역과 줄 수, 창을 그리는 순서를 다룬다.
- `GameUI.BattleLayout.cs`: 머리글 줄의 끝 위치와 보스 상태 배치를 다룬다. 영웅 위치(`HeroOnPage`)와 영웅 간격 상수(`HeroClearance` 56, `HeroSideClearance` 20)를 두고, 보스전에서 가장 높았던 보스 상태 높이를 기억한다. 페이지를 다시 만드는 프레임에 삭제 예정인 이전 창을 배치하지 않도록 새 창을 참조로 보관한다.
- `GameUI.cs`: `RefreshHud`가 오류 원문을 창에 쓰지 않는다. 번역하고 줄 수를 맞춘 문구는 창을 배치할 때 쓴다.
- `RuntimeVisibilitySmoke.cs`: 이전 토글 API 대신 새 선택 API와 버튼을 사용한다. 튜토리얼을 마친 계정 조건과 새 균열의 입장 대기를 추가했다.
- `RuntimeMapDisplaySmoke.cs`: 이 기능의 macOS 개발 빌드 검증을 수행한다.
- `Tests/Editor/MapDisplaySettingsTests.cs`: 설정 클래스를 검사한다. 이전 토글 검사는 `RiftVisibilityTests`에서 지우고 이 파일의 검사로 대체했다.

## 검증

### 자동 검사

`python3 tools/check_ui_contract.py`와 `python3 tools/test_ui_contract.py`(9개 검사)가 통과했다.

### Unity Edit Mode

`MapDisplaySettingsTests` 11개가 설정 클래스를 검사한다. 새 기기는 미니맵 창으로 시작하며 파일을 만들지 않는다. 세 방식은 각각 새 인스턴스에서도 유지된다. 이전 파일의 켜짐은 겹침 지도로, 꺼짐은 미니맵 창으로 이전되고 이전 파일은 바뀌지 않는다. 저장 실패 시에는 선택을 유지한 채 재시도할 수 있고, 재시도가 성공하면 안내가 사라진다. 손상된 새 파일이나 이전 파일은 기본값으로 시작하며 원본을 보존한다.

전체 Edit Mode 검사 4,107개를 복제 프로젝트에서 실행했다. 4,058개가 통과하고 49개가 실패했다. 47개는 기준 목록(main 24694040)과 같은 기존 실패다. `CoreTests.GeneratedDungeonContainsEnoughPointsAndUniqueEnemyIds(6,150)`은 시계 값을 시드로 쓰기 때문에 기준 코드에서도 300번 중 약 26번 실패하며, 이번 변경과 관계없다. `LocalizationTests.EveryKoreanLiteralInTheRuntimeHasAnEntry`는 origin/main에서 이미 실패하던 검사다(`RuntimeFirstPlayAcceptance.cs`의 두 문구). 이 브랜치에서 en.txt 항목 두 줄을 추가해 고쳤고, 이후 `LocalizationTests`·`StoredLocalizationTests`·`MapDisplaySettingsTests` 63개가 모두 통과했다. 요약은 [전체 Edit Mode 검사 요약](MapDisplayEvidence/editmode-full.txt)에 있다.

### macOS 개발 빌드 스모크

복제 프로젝트에서 `ProjectBuilder.BuildMac`으로 개발 빌드를 만들었고 빌드 오류는 0개였다. `RuntimeMapDisplaySmoke`(`-hellscriptMapDisplaySmoke`)는 튜토리얼을 마친 계정 조건에서 두 방과 통로로 된 합성 균열을 재개한다. 이후 설정 → 화면의 실제 선택 버튼을 EventSystem 레이캐스트의 맨 위 대상으로 확인한 뒤 누름·뗌·클릭 이벤트로 조작한다.

5개 화면 크기와 한국어·영어, 글자 크기 100%·150%를 조합한 20가지 조건에, 360×780과 390×844의 한국어·영어 150% 4가지 조건을 더해 모두 24가지 조건에서 세 방식을 차례로 선택했다. 390×844에서는 `UiSafeArea.Simulate`로 위쪽 노치 47픽셀과 아래쪽 홈 막대 34픽셀을 모의했다. 조건마다 다음을 확인했다.

- 선택 즉시 기기 파일에 저장되고, 같은 전투 객체와 전투 시간이 유지된다. 전투를 다시 시작하지 않는다.
- 선택한 방식의 지도만 그린다. 겹침 지도는 플레이어 위치를 중심으로 그리고, 미니맵 창은 최소 크기 이상으로 실제 도형을 그린다. 최소 크기는 64이며, 메뉴 버튼 옆으로 올린 창에서는 36, 보스전이나 오류 표시 중이거나 창이 실시간 전투 기록 8 위에서 끝날 때에는 40이다.
- 상자 수가 세 방식 모두에서 보이고 줄바꿈하지 않는다. 모서리 창과 설정 문구가 잘리지 않으며, 영어에서 한국어가 남지 않는다.
- 모서리 창은 안전 영역 안에 있고 머리글 글줄, 메뉴·설정·콘텐츠 메뉴·이벤트·모험 안내 버튼, 보스 상태, 실시간 전투 기록, 지속 HUD와 겹치지 않는다. 또한 영웅의 발보다 56 위에서 끝나거나 영웅보다 20 이상 옆에 있어서 영웅을 가리지 않는다.
- 언어와 글자 크기를 바꿔 전투 화면을 다시 만든 뒤에도 마지막 선택대로 그린다.

그 밖에 다음 조건을 확인했다.

- 창이 콘텐츠 메뉴 열 아래까지 넓어진 6가지 조건(440×956, 360×780, 390×844의 150%, 한국어·영어)에서 콘텐츠 메뉴를 실제 포인터 입력으로 펼쳤다. 네 바로가기가 모두 레이캐스트의 맨 위 대상인지 확인한 뒤 메뉴를 다시 접었다.
- 보스전 12가지 조건을 확인했다. 크기는 440×956(한국어 100%, 영어 150%), 390×844 노치 모의(한국어·영어 100%), 360×780(영어 100%), 640×360(한국어·영어 100%), 568×320(영어 100%), 956×440·1600×900·1600×1000·2100×900(영어 150%)이다. 스모크는 지도가 빠진 경우가 규칙이 허용하는 경우인지 실제 좌표로 계산해서 확인한다. 40짜리 지도가 보스 상태 줄 위에도 들어가지 않고, 영웅 위 간격을 지킨 채 창 아래에도 들어가지 않을 때에만 지도를 뺄 수 있다. 이전 스모크처럼 특정 조건 이름으로 예외를 두지 않는다. 보스 상태가 지도가 있는 창 아래에 있으면 영웅의 발보다 56 위에서 끝나는지 확인하고, 가로 화면에서는 이벤트·모험 안내 버튼과 겹치지 않는지 확인한다. 390×844 노치 모의의 영어 100% 보스전에서는 보스를 시야 밖으로 옮겼다가 되돌린 뒤 다시 시야 밖으로 옮겼다. 이렇게 보스 상태의 문구와 높이를 바꾸어도 지도가 한 번만 줄어들고 다시 커지지 않는지 확인했다.
- 길찾기 오류 4가지 조건을 확인했다. 440×956과 956×440의 한국어·영어 150%에서 미니맵 창과 표시 안 함 방식에 오류를 넣은 뒤 번역, 잘림, 겹침을 확인했다. 이어서 오류를 지우고 오류 줄이 사라지는지 확인했다.
- 기기 저장 실패와 재시도 2가지 조건, 세 방식 각각에서 모서리 창과 관찰 메뉴로 확대 지도에 들어가는 경로도 확인했다.

첫 실행은 `HELLSCRIPT_MAP_DISPLAY_SMOKE_OK`로 끝났고 확인 줄 140개를 남겼다. 두 번째 실행(`-hellscriptMapDisplayRelaunch`)은 별도 프로세스에서 기기 파일의 겹침 지도를 복원했고, 설정에서도 선택된 상태로 보여 준 뒤 `HELLSCRIPT_MAP_DISPLAY_RELAUNCH_OK`로 끝났다.

`RuntimeVisibilitySmoke`는 2026-09-26 `main` 기준 기록에서 튜토리얼 필수 조건 때문에 NullReferenceException으로 실패했다. 계정 조건과 새 균열의 입장 대기를 추가한 뒤에는 첫 실행(`HELLSCRIPT_VISIBILITY_SMOKE_OK`)과 재실행(`HELLSCRIPT_VISIBILITY_RESTART_OK`)이 모두 통과했다.

`RuntimeTutorialSmoke`는 `main`에서 통과하는 스모크 중 하나다. 튜토리얼 균열에도 모서리 창이 나타나므로 회귀 여부를 확인하려고 다시 실행했고, 첫 실행과 재개 실행이 모두 통과했다(`HELLSCRIPT_TUTORIAL_RUNTIME_SMOKE_OK`). 두 스모크는 이번 수정을 반영한 개발 빌드로 다시 실행했다.

```
Unity -batchmode -projectPath <복제 프로젝트> -executeMethod Hellscript.Editor.ProjectBuilder.BuildMac -hellscriptBuildOutput <빌드>/HELLSCRIPT.app -quit
HELLSCRIPT -hellscriptMapDisplaySmoke -hellscriptSavePath <저장> -hellscriptScreenshots <그림> -screen-fullscreen 0 -screen-width 1600 -screen-height 900
HELLSCRIPT -hellscriptMapDisplaySmoke -hellscriptMapDisplayRelaunch -hellscriptSavePath <저장> -hellscriptScreenshots <그림2> -screen-fullscreen 0 -screen-width 1600 -screen-height 900
HELLSCRIPT -hellscriptVisibilitySmoke [-hellscriptVisibilityResume] -hellscriptSavePath <저장> -hellscriptScreenshots <그림> -screen-fullscreen 0 -screen-width 1600 -screen-height 900
HELLSCRIPT -hellscriptTutorialSmoke [-hellscriptTutorialResume] -hellscriptSavePath <저장> -hellscriptEvidencePath <그림> -screen-fullscreen 0 -screen-width 1600 -screen-height 900
```

### 증거 자료

각 그림은 같은 조건의 세 방식(겹침 지도 · 미니맵 창 · 표시 안 함)을 한 장에 모았다. PC 크기는 50%로 줄였고 휴대폰 크기는 원래 크기다.

설정 → 화면의 지도 표시 방식:

- 세로 440×956: [한국어 100%](MapDisplayEvidence/settings-440x956-ko-100.png), [영어 100%](MapDisplayEvidence/settings-440x956-en-100.png), [한국어 150%](MapDisplayEvidence/settings-440x956-ko-150.png), [영어 150%](MapDisplayEvidence/settings-440x956-en-150.png)
- 가로 956×440: [한국어 100%](MapDisplayEvidence/settings-956x440-ko-100.png), [영어 100%](MapDisplayEvidence/settings-956x440-en-100.png), [한국어 150%](MapDisplayEvidence/settings-956x440-ko-150.png), [영어 150%](MapDisplayEvidence/settings-956x440-en-150.png)
- PC 16:9 1600×900: [한국어 100%](MapDisplayEvidence/settings-1600x900-ko-100.png), [영어 100%](MapDisplayEvidence/settings-1600x900-en-100.png), [한국어 150%](MapDisplayEvidence/settings-1600x900-ko-150.png), [영어 150%](MapDisplayEvidence/settings-1600x900-en-150.png)
- PC 16:10 1600×1000: [한국어 100%](MapDisplayEvidence/settings-1600x1000-ko-100.png), [영어 100%](MapDisplayEvidence/settings-1600x1000-en-100.png), [한국어 150%](MapDisplayEvidence/settings-1600x1000-ko-150.png), [영어 150%](MapDisplayEvidence/settings-1600x1000-en-150.png)
- PC 21:9 2100×900: [한국어 100%](MapDisplayEvidence/settings-2100x900-ko-100.png), [영어 100%](MapDisplayEvidence/settings-2100x900-en-100.png), [한국어 150%](MapDisplayEvidence/settings-2100x900-ko-150.png), [영어 150%](MapDisplayEvidence/settings-2100x900-en-150.png)
- 세로 360×780: [한국어 150%](MapDisplayEvidence/settings-360x780-ko-150.png), [영어 150%](MapDisplayEvidence/settings-360x780-en-150.png)
- 세로 390×844(노치 47·홈 막대 34 모의): [한국어 150%](MapDisplayEvidence/settings-390x844-notch-ko-150.png), [영어 150%](MapDisplayEvidence/settings-390x844-notch-en-150.png)

전투 화면:

- 세로 440×956: [한국어 100%](MapDisplayEvidence/battle-440x956-ko-100.png), [영어 100%](MapDisplayEvidence/battle-440x956-en-100.png), [한국어 150%](MapDisplayEvidence/battle-440x956-ko-150.png), [영어 150%](MapDisplayEvidence/battle-440x956-en-150.png)
- 가로 956×440: [한국어 100%](MapDisplayEvidence/battle-956x440-ko-100.png), [영어 100%](MapDisplayEvidence/battle-956x440-en-100.png), [한국어 150%](MapDisplayEvidence/battle-956x440-ko-150.png), [영어 150%](MapDisplayEvidence/battle-956x440-en-150.png)
- PC 16:9 1600×900: [한국어 100%](MapDisplayEvidence/battle-1600x900-ko-100.png), [영어 100%](MapDisplayEvidence/battle-1600x900-en-100.png), [한국어 150%](MapDisplayEvidence/battle-1600x900-ko-150.png), [영어 150%](MapDisplayEvidence/battle-1600x900-en-150.png)
- PC 16:10 1600×1000: [한국어 100%](MapDisplayEvidence/battle-1600x1000-ko-100.png), [영어 100%](MapDisplayEvidence/battle-1600x1000-en-100.png), [한국어 150%](MapDisplayEvidence/battle-1600x1000-ko-150.png), [영어 150%](MapDisplayEvidence/battle-1600x1000-en-150.png)
- PC 21:9 2100×900: [한국어 100%](MapDisplayEvidence/battle-2100x900-ko-100.png), [영어 100%](MapDisplayEvidence/battle-2100x900-en-100.png), [한국어 150%](MapDisplayEvidence/battle-2100x900-ko-150.png), [영어 150%](MapDisplayEvidence/battle-2100x900-en-150.png)
- 세로 360×780: [한국어 150%](MapDisplayEvidence/battle-360x780-ko-150.png), [영어 150%](MapDisplayEvidence/battle-360x780-en-150.png)
- 세로 390×844(노치 47·홈 막대 34 모의): [한국어 150%](MapDisplayEvidence/battle-390x844-notch-ko-150.png), [영어 150%](MapDisplayEvidence/battle-390x844-notch-en-150.png)

그 밖의 그림:

- 보스전의 미니맵 창: [세로 휴대폰](MapDisplayEvidence/boss-portrait.png), [가로와 PC](MapDisplayEvidence/boss-landscape.png), [작은 가로 화면 640×360·568×320](MapDisplayEvidence/boss-small-landscape.png), [보스 상태 문구가 바뀔 때](MapDisplayEvidence/boss-status-change.png)
- 콘텐츠 메뉴 열 아래로 넓어진 창 위에 펼친 콘텐츠 메뉴: [150% 6가지 조건](MapDisplayEvidence/dock-over-panel.png)
- 길찾기 오류 문구: [세로 440×956](MapDisplayEvidence/fault-portrait.png), [가로 956×440](MapDisplayEvidence/fault-landscape.png)
- 기기 저장 실패와 재시도: [한국어 100%와 영어 150%](MapDisplayEvidence/save-failure.png)
- 확대 지도로 들어가는 두 경로: [세 방식](MapDisplayEvidence/expanded-map.png)
- 별도 프로세스 재실행: [겹침 지도 복원](MapDisplayEvidence/relaunch-overlay-1600x900-ko-100.png), [설정의 선택 표시](MapDisplayEvidence/relaunch-settings-1600x900-ko-100.png)
- 시야 스모크: [가로](MapDisplayEvidence/visibility-landscape.png), [세로](MapDisplayEvidence/visibility-portrait.png)
- 실행 결과: [첫 실행](MapDisplayEvidence/runtime.txt), [재실행](MapDisplayEvidence/relaunch.txt), [시야 스모크](MapDisplayEvidence/visibility-runtime.txt), [시야 스모크 재실행](MapDisplayEvidence/visibility-restart.txt), [튜토리얼 스모크](MapDisplayEvidence/tutorial-runtime.txt), [전체 Edit Mode 검사 요약](MapDisplayEvidence/editmode-full.txt)

## 확인하지 않은 것과 남은 문제

- 모바일 실기기에서는 보지 않았다. 실제 터치, 기기의 안전 영역(노치), 기기 DPI 배율, Android 빌드와 성능은 확인하지 않았다. 포인터 조작은 macOS 개발 빌드에서 EventSystem으로 합성한 이벤트다. 노치와 홈 막대는 `UiSafeArea.Simulate`로 모의했을 뿐이다.
- Unity 에디터의 Play Mode와 MCP는 사용하지 않았다. 에디터가 원본 프로젝트를 잠그고 있어서 복제 프로젝트의 배치 모드와 개발 빌드로 검증했다.
- 스모크에서 실행하지 않은 크기에 대한 설명은 배치 식을 옮긴 계산으로만 확인했다. 이 계산은 이전 스모크가 기록한 창 좌표를 모두 같은 값으로 재현했지만, 보스 상태의 높이는 어림값을 썼다.
- 956×440·150%의 미니맵 창은 한 변이 47 단위(70.5픽셀)로 작다. 머리글 아래에 공간이 없기 때문이며, 더 크게 그리려면 실시간 전투 기록이나 HUD 배치를 바꾸어야 한다.
- 휴대폰 세로 화면의 보스전에서는 글자 크기가 크거나 위쪽 안전 영역이 크면 지도를 보스전 동안 뺀다. 폭 600~624의 가로 화면도 보스전 동안 지도를 뺀다. 두 경우 모두 위의 보스 상태 규칙을 따른 결과다.
- 콘텐츠 메뉴 열 아래로 넓어진 창에서는 콘텐츠 메뉴를 펼친 동안 바로가기 버튼이 지도 일부와 상자 수 끝을 덮는다. 메뉴를 접으면 다시 보인다.
- 956×440·150%에서는 오류 문구가 한 줄로 줄어들고 말줄임표가 붙는다. 전체 문구는 확대 지도에서 볼 수 있다.
- 계산으로만 확인한 극단적인 크기가 두 가지 있다. 폭 320 기기의 145~150%(루트 폭 약 213~221)에서는 상자 수의 최소 폭 때문에 창이 모험 안내 버튼 열까지 넓어진다. 780×360 가로 화면의 150%에 아래쪽 안전 영역 48이 있으면(루트 488×224) 실시간 전투 기록이 너무 높아서, 상자 수만 있는 창도 전투 기록과 약 7 겹친다.
- 왼쪽 버튼 열의 예약 폭(140)은 현재 `main`의 이벤트·모험 안내 버튼 위치에 맞춘 값이다. 미병합 PR #22는 모험 안내 버튼을 이벤트 버튼 아래로 옮기지만 가로 위치와 폭은 바꾸지 않으므로, 이 예약 폭은 그대로 맞는다. 이 판단은 PR의 변경 내용만 보고 내렸으며, 병합된 결과를 실행해 보지는 않았다.
- 이 작업에서 바꾸지 않은 기존 배치 문제가 증거 그림에 보인다. 폭이 좁은 세로 화면과 150% 화면에서 ‘모험 안내’ 버튼이 ‘이벤트’ 버튼, 보스 상태, 초상화 또는 실시간 전투 기록과 겹친다. 956×440·150%에서는 실시간 전투 기록이 머리글의 시간·처치·일시정지 줄과 콘텐츠 메뉴 버튼을 가리고, 가로 보스전의 보스 상태가 영웅 위에 놓인다. 568×320에서는 실시간 전투 기록이 보스 이름을 가리고, 1600×1000·150%에서는 실시간 전투 기록이 영웅의 머리를 가린다. 세로 150%에서는 머리글 부제목이 메뉴 버튼 아래로 이어진다. 설정 창을 닫은 직후에 찍은 일부 그림에는 ‘모험 안내’ 버튼이 아직 다시 나타나지 않았다. 이 현상의 원인은 조사하지 않았다.
- 가로 화면의 보스 상태는 창과 겹치지 않으면 `main`과 같은 자리에 있다. 창과 겹칠 때에만 위의 규칙에 따라 창 왼쪽으로 옮기거나, 지도를 줄이거나 뺀다.
- `LocalizationTests.EveryKoreanLiteralInTheRuntimeHasAnEntry`는 `origin/main`에서도 같은 두 문구 때문에 실패한다. 이 작업의 담당 범위가 아니어서 고치지 않았다.
- 공개 위키에는 게시하지 않았다. `AGENTS.md`에 따라 병합된 `main`에서만 게시하며, 이 작업에서는 PR을 열거나 병합하지 않았다.
