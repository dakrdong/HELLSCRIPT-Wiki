# 공통 API 연결 사전 검토 / Common API integration preflight

읽기 기준: 공통 **cdb9c140fbf8b778bd3c0bbc0fbcc68a5d9e3fa8**, 훈련 후보 **09ce5e5aefa900f0a17455c41f679e540af4bdf2**. 공통은 `task-6/skill-skin`의 `git show` 객체로 읽었습니다. 소스 수정·cherry-pick·Unity 실행은 하지 않았습니다. 이 문서는 적용 전 제안이며 수정 후 diff 검토 결과를 대신하지 않습니다.

## 최소 연결 / Minimal connection

`ContentWindowView.Open` 끝의 실제 인자는 `UiSkin skin=null`입니다. 아래 기존 인자/닫기/뒤로가기/입력 차단/크기는 모두 그대로 두고 named argument만 추가할 수 있습니다.

1. `TrainingGroundWindow.cs:31` 로비: `..., window.Dispose, skin:UiSkin.Casual)`.
2. `TrainingGroundWindow.Picker.cs:15` picker: `..., ()=>picker=null, new Vector2(560,660), skin:UiSkin.Casual)`.
3. `TrainingGroundResultWindow.cs:24` 결과: `..., back:game.LeaveTrainingGround, blocksGameplay:false, skin:UiSkin.Casual)`.

`view.Skin`은 Repaint에서도 유지되고, 공통 셸은 UiWindowFrame.SetSkin, Button/Tab의 casual chrome/라벨, close를 처리합니다. 그러므로 세 Render의 `TrainingGroundSkin.Shell(v)`와 local Shell/Paint 구현은 제거 대상입니다. UiWindowFrame.enabled=false, 별도 D4 root painter, Face.canvasRenderer.SetAlpha(0)를 남기면 새 공통 외형을 숨기거나 중복 렌더합니다.

`TrainingGroundButtonSkin.cs/.meta` 및 해당 컴포넌트/dirty-vertices observer/"Training casual button" painter 의존성을 제거합니다. 전용 wrapper가 필요한 별도 UiIconButton에는 `view.Skin.ApplyChrome(button)` 또는 `UiSkin.Casual.ApplyChrome(button)`을 명시할 수 있습니다. `UiTheme.Choice/Configure` 뒤에도 chrome이 유지되므로 observer는 필요하지 않습니다. 기존 tests의 TrainingGroundButtonSkin, D4Look.id, painter 이름 단정은 공통 Chrome/Face 상태와 기존 입력 보존 검증으로 변경해야 합니다.

## 주의점 / Concrete integration risks

### 라벨과 기호

공통 `LabelFont(text)`는 Jua에 없는 문자를 발견하면 Noto400을 선택합니다. `ContentWindowView.Button/Tab`에 실제 적용되며, `×` close는 명시적으로 BodyFont를 사용합니다. `ApplyChrome`은 라벨을 바꾸지 않고 `ApplyLabel`은 한 Text에만 적용합니다. 의미색을 가진 모든 자식 Text를 재귀 순회해 덮지 않습니다. 기존 본문400/명시적 강조700·900의 전용 Type은 유지하거나 같은 UiFonts 호출로 치환할 수 있습니다. 공통 ApplyLabel의 display:false는400이며, 강조700/900 선택 API가 추가된 것은 아닙니다.

3개 창 제목은 DisplayFont를 직접 사용하지만 훈련장/적 추가/훈련 결과의 KO/EN 문자는 기존 cmap 감사에서 모두 포함됩니다. 동적으로 바뀌는 버튼 text를 직접 대입하는 경우 LabelFont가 자동 재평가되는 observer는 없습니다. 해당 기호가 있는 라벨을 생성/변경할 때 다시 폰트를 결정해야 합니다. `▶`는 Noto에도 없으므로 LabelFont fallback만으로 해결하지 못합니다. 해당 사용처는 이번 연결 범위 밖 공용 훈련 HUD입니다. 공통 폰트는 수정하지 않습니다.

### 닫기 역할과 Icon/Quiet

공통 셸 close는 `Quiet`를 유지하면서 `CasualClose` chrome, 흰색 Noto400 `×`를 설정합니다. 전용 코드에서 `Destructive`로 재분류하거나 기본 ApplyChrome(close:false)로 덮지 않아야 기존 입력·오디오 역할과 원형 coral close가 유지됩니다.

공통 UiButtonFace는 **Icon/Item을 Casual 일반 panel 분기보다 먼저 처리**합니다. 프레임 없는 portrait remove Icon은 그림/원형 backdrop과 focus bracket을 보존하고, framed pencil Action은 casual 면을 얻습니다. local observer가 이전에 Icon에도 더했던 일반 panel을 제거하는 것은 실제 외형 차이입니다. 기존 tint/backdrop을 바꿀 경우 해당 Icon만 대상으로 합니다.

반대로 `Quiet`는 Casual panel 분기에서 제외하지 않습니다. 따라서 `training-skill-*`의 전체 행 Quiet 버튼이 idle에서도 항상 밝은 면을 그립니다. 이전 local observer는 Quiet idle painter를 껐으므로 **조용한 행의 평상시 채움이 달라지는 것이 확인된 API 차이**입니다. 이 시각 변화가 허용되는지 통합 캡처에 드러내야 합니다. 이를 숨기려고 Role을 Icon으로 바꾸거나 UiTheme.Plain으로 focus/navigation을 끄는 것은 최소 skin 연결이 아닙니다. idle 무면 정책이 필수라면 정확한 Quiet 역할 요구를 공통 소유자에게 요청합니다.

### Disabled 상태

기존 UiButton이 interactable/CanvasGroup 및 click/submit을 계속 소유합니다. Casual palette는 disabled/locked를 회색으로 만들고 기존 UiButton은 직접 자식 Text alpha를 availability에 따라1/.55로 조절합니다. 색 자체를 바꾸는 새 observer는 없습니다.

기존 Start note는 active일 때 Paper(흰색)로 생성되어 local observer가 상태별로 재색칠했습니다. observer 제거 뒤에는 공통 기본 green 버튼의 라벨 정책인 **Navy 고정색**으로 함께 바꿔야 합니다. 생성 전에 start.interactable=false를 설정한 뒤 추가한 note에는 생성 직후 `.55` alpha를 계속 명시해야 첫 disabled frame도 맞습니다. 이후 상태 갱신은 원래 UiButton 경로가 담당합니다. 닫기 glyph의 White는 공통 셸이 처리하는 별도 역할입니다.

### 패널·반투명·둥근 마스크

실제 API는 `StorageSurface.SetSkin(skin, border:null, radius:12, borderWidth:2, shadow:3)`와 `skin.Clip(parent,radius:12,inset:2)`입니다. SetSkin의 geometry는 **현재 Canvas 단위**이며 D4 painter의 2배 단위를 가져오지 않습니다. SetSkin(null)로 legacy paint 경로 복귀가 가능하고 Paint는 Skin을 해제하지 않습니다.

SetSkin이 켜진 면은 기존 top/bottom/edge/radius/Paint를 읽지 않고 Skin.Cream/CreamShade와 별도 border/radius/shadow를 사용합니다. 따라서 보통 panel은 SetSkin으로 치환할 수 있지만, **선택 checkbox 색과 훈련장 veil의 위/아래 alpha(.65/.94)는 일괄 SetSkin 적용하면 사라집니다.** 용도가 다른 generic Surface 호출을 한꺼번에 바꾸지 말고 이러한 단색/반투명 구조는 전용 paint로 보존합니다.

Panel은 자식을 자동 clip하지 않습니다. 기존 portrait의 frame Mask를 공통 clip으로 옮길 경우 frame 아래 `skin.Clip`을 한 번 생성하고 RawImage를 그 자식으로 둡니다. Clip Rect는 원래 좌표/크기를 유지하므로 기존 x/y와 Contain의 2px inset을 중복 이동하지 않습니다. frame Mask와 새 Clip을 중첩하지 않습니다. clip stencil은 border 안쪽을 자르지만 SetSkin의 하단 shadow 높이를 자동으로 빼지는 않으므로, 가장자리까지 이미지를 채우는 portrait/배경은 shadow와 clip의 하단 경계를 명시적으로 맞춰야 합니다(예: 해당 art frame만 shadow:0). 실제 픽셀 모서리 확인은 Unity 대기입니다.

## 명확한 미지원 범위 / Explicitly absent APIs

공통 인계는 TrainingDpsChart/TrainingChartView/TrainingDpsPanel/RiftSkillShareView와 GameUI.TrainingGround 등 전투/결과 공용 카드의 skin API를 **포함하지 않았다고 명시**합니다. 이번 Open3곳 연결만으로 그 자식이 스킨을 물려받는다고 가정하지 않습니다. EquipmentArtProvider는 장비 공급 계약이며 monster portrait 또는 training-yard 공급 API가 아닙니다. 승인된 몬스터35와 전용 배경의 기존 읽기 경로/매핑을 이 API로 임의 대체하지 않습니다.

공통 receipt는 Runtime/Tests 정적 compile 오류0(47/2 warnings), UI contract15 PASS, refresh PASS를 기록하지만 새 Unity 사례6개는 실행 전입니다. 이번 검토에서도 Unity/실기기/플레이 결과를 추가하지 않았습니다. 수정 diff는 동일 ownership 범위로 재검토한 뒤 부모 검증 큐로 넘겨야 합니다.

## 고정한 공통 입력 / Pinned common inputs

| Path | Git blob | SHA-256 |
| --- | --- | --- |
| `Assets/HELLSCRIPT/Runtime/Presentation/UiSkin.cs` | `dea1e5231b525bd4137193b7bfac91348ea49bfe` | `a09ab288dfac62fafe7004d497358ce1f66b0471b2397d97029ad1c3e09ce970` |
| `Assets/HELLSCRIPT/Runtime/Presentation/ContentWindowView.cs` | `eb8a67304c9492c23847e35a929d66ea68032367` | `2dbe0731e2e21278e9cbe837d5518c2a4fa614952d94c42e685328d0de5ad541` |
| `Assets/HELLSCRIPT/Runtime/Presentation/StorageSurface.cs` | `771849e3e9f17b43aed3cad742cc65d61f9315c1` | `aa2a6c8e91e122689529f69c11c13fb3fff9ab422a7340b063c1629372b1cd30` |
| `Assets/HELLSCRIPT/Runtime/Presentation/UiButton.cs` | `55153be437f27c198f0fd061c75c8e5cfc13a783` | `4c4f433ddbef987e4e11939b5c1e8fecbd06da7a0983c9403a65076abc62019f` |
| `Assets/HELLSCRIPT/Runtime/Presentation/UiButtonFace.cs` | `0450a301ae0ff6a56dc4210ed7e3d4d6e71bf480` | `45290875746760dc5cb8a72fa46bb81eb07def038ec53e04dfbe9ac83bff8f99` |
| `Assets/HELLSCRIPT/Runtime/Presentation/UiWindowFrame.cs` | `d26361dee0be95a3618d0722feca3624ac2d540d` | `d548fea8ff557a5d7258fb6b2701efaa9bb3eb101fcf6beb6a87fd9053996d53` |
| `Assets/HELLSCRIPT/Runtime/Presentation/UiIconButton.cs` | `5f355d25944da9d732bc444d6d176e41b0fa4741` | `90e61b122d2aa1719f7710303cf062603456400a680e0dcbb754222666229ad3` |
| `Docs/Implementation/Casual_UI_Parallel_Handoff_20261014.md` | `c18d0a5c83000522fbcc63d1b89134a275991415` | `5f7b40a0860dfa5130c0ded83f1565e886696a8734f56ba705ea0e05e073f4dd` |
| `Docs/Implementation/Casual_UI_Parallel_Handoff_20261014.en.md` | `702ade8b31a8b107e5e47fad0f9d7b5e39e8f40d` | `28b8bc3c5ff12b06f77333622284258bf34718b6d883ee42034639722e40a715` |
| `Docs/Implementation/Casual_UI_Common_API_Validation_20261014.json` | `954db9a212f3767453a5408e52c917b7c70c1a2e` | `63eb50a46a34486ed273d04497480d9ab883794edba31a1b04c21283e08adeec` |

## English handoff

Consume the exact cdb9c140 opt-in API by adding skin:UiSkin.Casual to the lobby, picker and result Open calls. Remove local shell suppression, the dirty-vertices observer and its extra painter; preserve existing input roles and commands. Common close retains Quiet with CasualClose and Noto ×. Explicitly use Navy for the Start note after removing the local color observer, while retaining initial disabled alpha. SetSkin overrides legacy fill/alpha/radius, so do not blanket-apply it to checkbox fills or the transparent art veil. Use one inner Clip for edge-reaching art and align its bounds with any shadow. Casual Quiet paints an idle panel; this differs from the old local quiet-row policy and needs visible review. No chart, battle-HUD or monster artwork API is supplied by this commit. Rendering remains pending; no source or Unity state was changed by this audit.
