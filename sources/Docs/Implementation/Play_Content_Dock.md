# HELLSCRIPT 플레이 화면 콘텐츠 독

작성일: 2026-09-15

## 목표

플레이 화면의 오른쪽 위 설정 버튼 아래에 콘텐츠 바로가기 묶음을 둔다. 묶음은 접었다 펼 수 있다. 접힌 상태에는 설정 버튼 아래에 아래 화살표 버튼 하나만 보인다. 아래 화살표를 누르면 화살표가 아래로 내려가면서 그 위로 바로가기 버튼이 차례로 드러나고, 화살표가 끝까지 내려오면 180도 돌아 위 화살표가 된다. 위 화살표를 누르면 반대 순서로 화살표가 먼저 돌아온 뒤 위로 올라가며 바로가기를 다시 숨긴다.

바로가기는 세 개다.

| 순서 | 버튼 | 연결 |
|---|---|---|
| 1 | 캐릭터 (가방과 장착 아이템) | 화면이 아직 없다. 누르면 `준비 중인 콘텐츠입니다.` 안내만 표시한다. |
| 2 | 사냥 칙령 (스킬 관리와 칙령 편집) | 기존 사냥 칙령 편집 진입점 `ShowEdictEditor`를 그대로 연다. |
| 3 | 룬 보드 (룬 블럭 장착과 관리) | 화면이 아직 없다. 누르면 `준비 중인 콘텐츠입니다.` 안내만 표시한다. |

캐릭터와 룬 보드 화면은 이번 작업 범위가 아니다. 버튼과 그림만 두고 기능은 연결하지 않는다.

## 적용 화면

플레이 화면은 마을(`plaza`)과 균열 전투(`battle`)다. 두 화면 모두 헤더 오른쪽 위에 설정 톱니바퀴가 있고, 독은 그 바로 아래 같은 오른쪽 여백 12에 붙는다.

| 화면 | 설정 버튼 | 독 버튼 크기 | 독 시작 위치 |
|---|---|---|---|
| 마을 | 52×52, 위 12 | 52 | 위 70 |
| 균열 전투 | 44×44, 위 8 | 44 | 위 58 |

버튼 사이 간격은 6이다. 독은 오른쪽 가장자리에 고정하므로 창 크기나 비율이 바뀌어도 설정 버튼 아래에 머문다. 펼친 상태는 화면이 바뀌어도 기억한다. 마을에서 펼친 채 균열에 들어가면 전투 화면도 펼친 상태로 시작하며, 이때는 연출 없이 바로 그 상태를 그린다.

균열 전투의 가로 화면에서 오른쪽 위에 있던 균열 미니맵은 독 열과 겹치지 않도록 왼쪽으로 56만큼 옮겼다. 미니맵의 크기와 위치 기준은 그대로다. 제목·게이지·시간·관찰 메뉴·설정 버튼의 위치는 바꾸지 않았다.

성소 메뉴, 장비, 설정 같은 목록형 화면에는 독을 두지 않는다. 그 화면들은 헤더 바로 아래에 스크롤 내용이 시작되어 펼친 독이 내용을 가리게 되고, 같은 진입 버튼이 이미 목록 안에 있다.

## 연출

`ContentDockView` 하나가 두 값을 가진다. `slide`는 화살표의 이동 비율이고 `turn`은 회전 비율이다. 펼칠 때는 `slide`가 0에서 1로 먼저 움직이고, 끝나면 `turn`이 0에서 1로 움직여 180도 회전한다. 접을 때는 `turn`이 먼저 1에서 0으로 돌아오고, 끝나면 `slide`가 1에서 0으로 돌아간다. 각 단계는 실시간 0.22초이며 전투가 일시정지된 동안에도 진행된다.

바로가기 버튼은 `RectMask2D`가 있는 영역 안에 있고, 이 영역의 높이는 `slide × 전체 이동 거리`다. 화살표가 내려온 만큼만 영역이 커지므로 버튼이 화살표를 따라 드러난다. 가려진 부분은 그려지지 않고 눌리지도 않는다. 위 화살표 그림을 따로 두지 않고 아래 화살표 그림을 180도 돌려 쓴다. 테두리의 네 방향 장식이 대칭이므로 회전해도 같은 모양이다.

## 리소스

사용자가 대화에 첨부한 PNG 4장을 사용했다. 원본은 저장소 밖에 두고, 아래 표와 [출처 기록](PlayContentDockEvidence/provenance.json)에 원본 해시와 변환 방법을 남겼다. 게임용 파일은 `Assets/HELLSCRIPT/Resources/Art/GlobalHUD/`에 넣어 기존 전역 HUD 가져오기 설정(스프라이트, 투명도 유지, 밉맵 없음, 무압축, Clamp)을 그대로 받는다.

| 원본 파일 | 원본 크기 | 게임용 파일 | 처리 |
|---|---|---|---|
| 캐릭터.png | 1254×1254 | menu-character.png | 512×512로 축소 |
| 사냥 칙령.png | 1254×1254 | menu-hunt-edict.png | 512×512로 축소 |
| 룬 보드.png | 1254×1254 | menu-rune-board.png | 512×512로 축소 |
| 화살표 버튼.png | 1774×887, 아래·위 화살표 2개 | menu-toggle.png | 왼쪽 절반(아래 화살표)을 잘라 512×512로 축소 |

축소는 Pillow 11.3.0의 LANCZOS로 했고 배경 투명도는 원본 그대로다. 네 모서리의 알파는 모두 0이다. 512 크기는 마을의 52 단위 버튼이 기기 배율 4배와 글자 크기 150%를 함께 적용받아도 원본보다 큰 픽셀로 그려지지 않는 크기다. 전역 HUD 리소스 검사의 파일 수 기대값을 67에서 71로 올렸다.

## 문구

새 문구는 한국어 원문을 키로 두고 영어 표에 함께 추가했다. `캐릭터`, `룬 보드`, `콘텐츠 메뉴`, `준비 중인 콘텐츠입니다.` 네 줄이며 `사냥 칙령`은 기존 항목을 쓴다. 버튼의 오브젝트 이름은 한국어 원문을 유지하고 화면에는 글자 대신 그림만 그린다.

## 바꾼 파일

| 파일 | 내용 |
|---|---|
| `Assets/HELLSCRIPT/Runtime/Presentation/GameUI.ContentDock.cs` | 새 파일. 독 구성 `AddContentDock`, 그림 버튼 `Emblem`, 연출 `ContentDockView` |
| `Assets/HELLSCRIPT/Runtime/Presentation/GameUI.BattleLayout.cs` | 전투 헤더에 독 추가, 가로 미니맵을 왼쪽으로 56 이동 |
| `Assets/HELLSCRIPT/Runtime/Presentation/GameUI.Plaza.cs` | 마을 헤더에 독 추가 |
| `Assets/HELLSCRIPT/Runtime/Presentation/RuntimeContentDockSmoke.cs` | 새 파일. 개발 빌드 런타임 스모크 `-hellscriptContentDockSmoke` |
| `Assets/HELLSCRIPT/Resources/Art/GlobalHUD/menu-*.png` | 새 스프라이트 4장 |
| `Assets/HELLSCRIPT/Resources/Localization/en.txt` | 새 문구 4줄 |
| `Assets/HELLSCRIPT/Tests/Editor/GlobalHudResourceTests.cs` | 파일 수 67 → 71 |

## 검증

검증 결과는 아래 표와 [증거 폴더](PlayContentDockEvidence/)에 있다. 실기기에서는 확인하지 않았다.

| 항목 | 결과 |
|---|---|
| Edit Mode 검사 (현지화·전역 HUD 리소스·전투 배치, 72개) | 71개 통과. 실패 1개 `EveryKoreanLiteralInTheRuntimeHasAnEntry`는 같은 브랜치에 커밋 전인 사냥 칙령 작업 파일 `CombatSimulation.HuntEdictChanges.cs`, `SkillProgressionInfo.cs`의 미번역 82줄 때문이며 이번 파일에서 나온 항목은 없다. |
| macOS 개발 빌드 | Unity 6000.6.0f1 배치 빌드 [오류 0](PlayContentDockEvidence/build.txt). 실행본은 저장소 밖 작업 폴더에 두었다. |
| 런타임 스모크 `-hellscriptContentDockSmoke` | 격리 저장 폴더로 실행해 [통과](PlayContentDockEvidence/runtime.txt)했다. 종료 코드 0, `HELLSCRIPT_CONTENT_DOCK_SMOKE_OK`. |

런타임 스모크가 확인한 내용은 다음과 같다. 1600×900 창에서 마을에 들어가 독이 접혀 있고 가려진 `캐릭터` 버튼 중심으로 실제 UI 레이캐스트를 쏘아도 그 버튼이 잡히지 않는 것을 확인했다. 아래 화살표를 누른 뒤 0.11초 시점에 이동 영역 높이가 0과 전체 거리 사이에 있고 회전은 0도인 것을 확인해 이동이 회전보다 먼저임을 검사했다. 연출이 끝난 뒤 회전이 180도이고 세 버튼이 모두 레이캐스트로 잡히는 것을 확인했다. `캐릭터`를 누르면 안내 문구가, `룬 보드`를 누르면 화면 변화 없이 안내만 나타났다. 위 화살표를 누른 뒤 0.11초 시점에 회전은 중간값이고 이동 영역은 아직 전체 높이인 것을 확인해 접을 때는 회전이 먼저임을 검사했다. 접힌 뒤 다시 펼쳐 `사냥 칙령`을 누르면 사냥 칙령 창이 열리고 닫기로 마을에 돌아왔다. 펼친 채 균열에 들어가면 전투 독이 펼친 상태로 시작했고 미니맵의 오른쪽 끝이 독 왼쪽 가장자리를 넘지 않았다. 전투에서도 사냥 칙령이 열리고 닫힌 뒤 전투 화면으로 돌아왔으며, 독을 접은 뒤 900×1600 세로 창에서도 다시 펼쳤다.

| 화면 캡처 | 내용 |
|---|---|
| [01](PlayContentDockEvidence/01-plaza-folded.png) | 마을, 접힌 독 |
| [02](PlayContentDockEvidence/02-plaza-opening.png) | 마을, 펼치는 중. 캡처 대기 0.2초 동안 이동이 끝나 화살표가 돌아가는 순간이 찍혔다. 이동 우선 판정은 캡처 전에 코드로 확인했다. |
| [03](PlayContentDockEvidence/03-plaza-open.png) | 마을, 펼친 독과 위 화살표 |
| [05](PlayContentDockEvidence/05-plaza-hunt-edict.png) | 마을 독에서 연 사냥 칙령 창 |
| [06](PlayContentDockEvidence/06-battle-open.png) | 균열 전투 가로, 펼친 독과 왼쪽으로 옮긴 미니맵 |
| [08](PlayContentDockEvidence/08-battle-folded.png) | 균열 전투 가로, 접힌 독 |
| [09](PlayContentDockEvidence/09-battle-portrait-open.png) | 균열 전투 세로, 펼친 독 |

Edit Mode 결과 원본은 [editmode-focused.xml](PlayContentDockEvidence/editmode-focused.xml)이다. 전체 회귀 검사는 이번 작업에서 다시 돌리지 않았다. 사냥 칙령 창의 내용은 같은 브랜치에서 진행 중인 별도 작업의 상태를 그대로 보여 주며 이번 작업이 바꾼 것은 없다.

## 남은 일

캐릭터와 룬 보드 화면을 만들면 두 버튼의 안내 문구를 실제 진입으로 바꾼다. 마을 가로 화면이 매우 작을 때(높이 360 근처) 펼친 독의 아래쪽이 오른쪽 NPC 대화 카드와 겹칠 수 있다. 독을 접으면 겹치지 않으며, 필요하면 그 카드의 위치를 조정한다.
