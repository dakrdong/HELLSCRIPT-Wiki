# 공통 API 적용 diff 리뷰 / Common API adoption diff review

시각: 2026-10-14T07:30:32.665511+00:00

검토 기준 HEAD **d5b8c8898cb2bc140c4c736426b961704f57ac09** (`cdb9c140fbf8b778bd3c0bbc0fbcc68a5d9e3fa8` 공통 API를 부모가 cherry-pick한 local commit). staged 삭제와 unstaged 변경을 모두 포함한 `git diff HEAD` SHA-256: **bf14c21472e1b824afbecd53e5f0c8f869d88ec5b3a5e26efe651aebf4b3fcdb**. 소스·index·branch 변경 없이 읽었습니다. Unity/컴파일/테스트는 실행하지 않았습니다.

## 발견사항 / Findings

**P2 — Casual Quiet가 자동 사용 OFF 경고 테두리를 가림.** `TrainingGroundSkin.cs:40`은 모든 UiButton에 Shared.ApplyChrome을 적용합니다. `TrainingGroundWindow.Skills.cs:50–56`은 on&&!auto일 때 Danger 테두리의 부모 row를 만든 뒤 `training-skill-*` Quiet 버튼을 x=0,y=0,pickW,h로 겹칩니다. 공통 `UiButtonFace.cs:81–85`의 Casual 분기는 Quiet도 평상시 불투명 면과 기본 테두리를 그립니다. 이전 local observer는 Quiet idle의 painter를 껐습니다. 새 기본 면이 부모 경고 테두리의 왼쪽과 상·하 대부분을 덮고 오른쪽 pen 영역 일부와 경고 텍스트만 남습니다. 입력/상태 문자열/checked 로직은 유지되지만 경고 외형이 회귀합니다. 공통 소유자와 **Casual + Quiet idle 무면 정책**을 조율할 대상입니다. CasualClose는 기존 Quiet 역할이므로 공통 수정 시 반드시 별도 제외해야 합니다. 로컬 observer 재도입, Role 변경, focus/navigation 비활성화로 우회하지 않습니다. 이것은 소스 겹침/렌더 분기에서 확인한 결과이며 픽셀 캡처는 아직 없습니다.

**해결 — Hero 하단 border/shadow 겹침.** 마지막 후속 변경에서 Hero의 기존 StorageSurface에 `SetSkin(TrainingGroundSkin.Shared,shadow:0)`을 적용했습니다. 이전에 명시하던 neutral border는 Shared.Navy였고 기본 border도 Navy이므로 의미색은 바뀌지 않습니다. radius12/border2는 유지되며, 이제 face 하단은 r.yMin, inner fill 하단은 r.yMin+2로 Shared.Clip 기본 inset2와 일치합니다. art와 veil은 같은 clip 아래에 남습니다. 이전 shadow3/clip2 하단 불일치는 소스상 해결됐습니다. 위치·크기·문구·게임 규칙 변경은 없으며 실제 GPU 렌더 확인은 Unity 대기입니다.

## 통과한 정적 확인 / Static checks without a discovered issue

- Open3곳은 기존 source/max-size/back/blocksGameplay 인자를 유지하고 끝에 skin:UiSkin.Casual만 추가했습니다. lobby/picker/result 각각 opt-in하며 다른 창에 자동 전파하지 않습니다.
- Shell 호출/구현 및 D4 root painter가 삭제됐습니다. `TrainingGroundButtonSkin.cs/.meta`가 staged 삭제되어 있고, C#에 삭제 클래스·Training casual button·Shell/Paint 참조가 없습니다. 공유 UiWindowFrame/Face를 숨기는 local SetAlpha(0) observer가 제거됐습니다.
- 공통 close는 이번 diff에서 다시 스타일링하지 않습니다. Quiet + CasualClose, White Noto400 ×, 기존 close/뒤로가기 경로가 유지됩니다.
- 전용 Button helper는 Button text 한 개만 Noto700/Navy로 정합니다. 의미색/본문 자식 전체를 덮지 않습니다. 기존 clickable Rect/targetGraphic/역할, picker Choice/selected, 아이콘 이벤트는 유지됩니다. ApplyChrome의 중복 호출은 같은 chrome이면 no-op입니다.
- Start note는 Navy 고정으로 바뀌고 생성 직후 interactable에 따른 alpha1/.55가 유지됩니다. 기존 UiButton availability 경로와 충돌하는 추가 observer는 없습니다.
- 일반 panel은 StorageSurface.SetSkin으로 공통 radius12/border2/shadow3을 받습니다. 별도 Surface 경로는 checkbox/portrait 색을 유지하고 veil의 alpha도 보존합니다.
- portrait는 frame의 SetSkin(radius:8,shadow:0)과 Shared.Clip(frame,radius:8,inset:2) 하나를 사용합니다. RawImage는 그 자식이며 기존2px 위치와 Contain UV/aspect를 유지합니다. shadow0이므로 clip2와 frame border2의 하단이 일치합니다.
- 앞서 보고한 dropdown 둥근 clip 부재는 수정됐습니다. 기존 ScrollRect.viewport만 Shared.Clip(template,8,2) 아래 SetParent(false)하고 ScrollRect viewport/content 참조, 기존 anchors/sizeDelta, template options/callback은 그대로 둡니다. parent==template 조건으로 두 번째 적용은 clip을 추가하지 않습니다. template/clip의 전체 bounds가 같아 기존 viewport 좌표계가 유지됩니다. 새 테스트에 identity/bounds/단일 clip 단정이 추가됐으며 실행은 대기입니다.
- 수정 테스트는 삭제 observer/D4 painter 내부명 단정을 제거하고 공통 chrome, 원래 Face/target, 의미색 자식 불변을 검증하도록 바뀌었습니다. dropdown callbacks/options, portrait ratio/registry-readiness 기존 검증은 남습니다. 공통 UiButtonTests/CasualUiSkinTests의 상태 회귀 검증은 별도로 실행 대기입니다.
- `git diff HEAD --check` 통과. 아래 공유파일은 HEAD와 바이트가 같아 추가 수정이 없습니다.

## Clip 재검토 상태 / Clip follow-up

부모 수정 후 Hero의 radius10/inset1은 사라졌고, Shared.Clip 기본12/2 아래에 art와 veil이 함께 생성됩니다. corner 반경/단일 clip 구조는 정리됐습니다. 후속 Hero 전용 shadow0 적용으로 하단 border/clip 경계도 소스상 정렬됐습니다. veil의 명시적 top/bottom alpha와 선택 checkbox의 별도 Surface는 유지됩니다. 드롭다운의 직사각형 viewport 문제는 공통 clip 추가로 소스상 해결됐고 portrait는 shadow0으로 정렬됐습니다. 실제 corner/scroll 픽셀 렌더 확인은 Unity 대기입니다.

## 범위와 실행 증거 / Scope and execution evidence

공유 DPS/차트/기여카드/HUD는 이번 diff에 없습니다. `Open(...skin)`만으로 그 자식들의 기존 색/OS font가 바뀌지는 않습니다. 부모의 기존15개 UI 계약·refresh PASS는 이전 입력의 증거이며 이번 수정의 Unity/컴파일 통과로 재사용하지 않습니다. 이 리뷰에서 새 테스트 결과는 만들지 않았습니다. KO/EN, empty/disabled/selected, touch/click/scroll/저장, 2868×1320/portrait와 corner rendering은 부모의 단일 Unity 검증 대기입니다.

## 입력 해시 / Input hashes

다음은 최종 Hero shadow0 수정 후 스냅샷이며 앞선 입력 해시를 대체합니다. 시각: 2026-10-14T07:33:37.406659+00:00. `git diff HEAD` SHA-256: `bf14c21472e1b824afbecd53e5f0c8f869d88ec5b3a5e26efe651aebf4b3fcdb`.

| 상태 | 수정/삭제 경로 | 현재 SHA-256 |
| --- | --- | --- |
| D | `Assets/HELLSCRIPT/Runtime/Presentation/TrainingGroundButtonSkin.cs` | `DELETED` |
| D | `Assets/HELLSCRIPT/Runtime/Presentation/TrainingGroundButtonSkin.cs.meta` | `DELETED` |
| M | `Assets/HELLSCRIPT/Runtime/Presentation/TrainingGroundResultWindow.cs` | `de48fe463bbefeca5a6fe20c58f603dd7f8e8e155ca48de8f1a3e60dbaca25ed` |
| M | `Assets/HELLSCRIPT/Runtime/Presentation/TrainingGroundSkin.cs` | `6c4fbb83e8180e2534cc008ae41fd0fb7680b5d41f1aca9bc75e3b8f33e42b17` |
| M | `Assets/HELLSCRIPT/Runtime/Presentation/TrainingGroundWindow.Hero.cs` | `5f80a3223fc0664d39ca225fc340e90262c7b126138381b0ceca5a0bd8792230` |
| M | `Assets/HELLSCRIPT/Runtime/Presentation/TrainingGroundWindow.Picker.cs` | `ad3ce2131d4752d599b4dad4d07b5cc9b1e9ef42dd80510851d2d29edaec9e6c` |
| M | `Assets/HELLSCRIPT/Runtime/Presentation/TrainingGroundWindow.cs` | `9585d21949d505a930fc34e86431e68df721b4fa4f15e6c3fe781147d758a6fe` |
| M | `Assets/HELLSCRIPT/Tests/Editor/TrainingGroundSkinTests.cs` | `93f8163a4b1d447cf974c6ddbd7199b28ae9301b4a020a2e4e23f05d665eb6a6` |
| M | `Docs/Implementation/Casual_Training_Ground_Skin_20261014.en.md` | `6f0ab93c64f57beb2d497760ce0d838647b26b30bf2803b788e8c1f4763ec713` |
| M | `Docs/Implementation/Casual_Training_Ground_Skin_20261014.md` | `515432a04d43f239a4d139f948a95636aaf8a3dc5cbbc33299bc63d23cd19f66` |

공유 UiSkin/ContentWindowView/UiWindowFrame/UiButton/UiButtonFace/StorageSurface/UiDropdown은 HEAD와 추가 diff가 없습니다.

## English handoff

The follow-up fixes the dropdown's missing rounded child clip while preserving viewport/content identities and bounds; portrait now uses SetSkin(radius8,shadow0)+Clip(8,2). Hero art and veil now share Clip defaults12/2. The Hero-only SetSkin(shadow:0) follow-up now aligns the face inner border with Clip inset2, resolving its bottom-art overlap in source without changing layout or text. One static visual issue remains: Casual Quiet overlays the Danger outline on auto-off skill rows. Quiet remains a common-owner policy decision. Input/domain/save semantics and shared-file ownership are preserved. No Unity/C# test execution was performed by this reviewer.

마지막 좁은 재검토: Hero shadow0 한 줄과 그 주석, 하단 경계 계산만 재확인했습니다. `git diff HEAD --check` PASS; Unity/컴파일/테스트 재실행 없음.
