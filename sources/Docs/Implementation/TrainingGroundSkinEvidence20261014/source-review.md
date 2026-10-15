# 훈련장 후보 소스 리뷰 / Training candidate source review

정적 검토 완료 시각: 2026-10-14T07:15:06.867557+00:00

대상: `codex/casual-training-skin`, checkout HEAD **2e193c336c6176d80b9ddbdff7269246bf2590fb**. 게임/공통 코드 기준 **4139a77e2df495cee009c07c6094b2eea9b1aeaa** 위 문서 커밋2e193c33. 검토 대상은 아직 미커밋된 전용 스킨 diff입니다. 리뷰 시작/종료 HEAD는 같았으며, 아래 파일 해시로 정확한 입력을 고정했습니다. 기준2e193c33 대비 Presentation diff SHA-256: `bafd05b02432e2450ae215b1e98b2a7dfe4f807bf44d7ec2d24725cbd60621bd`. 새 adapter/test 파일은 아래 개별 해시에 포함됩니다. 해시 기록 중 부모가 파일을 stage했으므로 `git diff 2e193c33 -- Assets/HELLSCRIPT/Runtime/Presentation`으로 staged/unstaged를 함께 고정했습니다.

## 결론 / Conclusion

확정적인 C# 컴파일 불가, 입력·저장 회귀, 공유 파일 소유권 위반은 정적 검토에서 발견하지 못했습니다. **실제 Unity 컴파일 또는 플레이 PASS를 뜻하지 않습니다.** 전용 adapter는 4139 기반 후보이고, 공통 담당이 확정 SHA/API를 인계하면 중복 역할을 확인한 뒤 치환할 대상입니다.

- `TrainingGroundButtonSkin.Bind`는 기존 UiButton 및 targetGraphic을 보존합니다. 새 painter는 raycastTarget=false이고 추가 click/submit handler가 없습니다. dirty-vertices observer는 중복 Bind를 막고 OnDestroy에서 해제됩니다. `UiButtonFace.Show`가 상태 필드를 갱신한 뒤 SetVerticesDirty를 호출하므로 callback은 갱신된 disabled/chosen/focus/hover/pressed 상태를 읽습니다.
- 비활성 Start note는 생성 직후 interactable 값으로 색·알파(.55)를 정하며, 이후 callback이 Button text와 Start note 색을 함께 처리합니다. 본래 UiButton이 availability에 따라 직접 자식 Text의 alpha를 처리하는 경로도 유지됩니다. 기본 읽기 배율은 GameUI.InterfaceFactor=1입니다.
- 초상화와 배경의 Mask는 Image를 상속하는 StorageSurface의 둥근 메시를 사용합니다. RawImage는 raycastTarget=false입니다. Contain은 UV 전체(0,0,1,1)를 유지하며 축 비율에 맞춰 Rect 크기를 조정합니다. 실제 GPU stencil/모서리 픽셀과 주변 반투명 경계는 아직 미검증입니다.
- 화면 전용 변경은 TrainingGroundWindow 다섯 partial, TrainingGroundResultWindow, 두 local adapter, 전용 테스트·문서·training-yard에 한정됩니다. 본 리뷰에서 해당 파일 또는 원본/공통 파일을 수정하지 않았습니다. N23/B06 표시 경로 fallback은 Registry의 artPending/gameplay availability를 바꾸지 않습니다.

## 공유 DPS 미완료 / Shared DPS remains incomplete

`TrainingGroundResultWindow.cs:121`은 여전히 `TrainingDpsPanel.Draw`를, `:143`은 `RiftSkillShareView.Card`를 호출합니다. 이 공유 구현은 기존 어두운 panel 색, OS 본문 폰트 및 편집 버튼을 직접 생성합니다. 따라서 **훈련 결과 전체의 캐주얼 스킨 통일은 미완료**입니다. 공유 배틀 HUD `GameUI.TrainingGround`, TrainingDpsPanel/ChartView/DpsChart, RiftSkillShareView는 훈련 전용 소유 파일로 편입하지 않았으며 공통 소유자 인계/통합 검증이 필요합니다. 전투6 아트보드 역시 이 책임 경계와 분리해 기록해야 합니다.

폰트 감사는 `font-coverage-audit.md/.json`에 별도로 기록했습니다. Noto400/700/900의 실제 cmap 누락은 공용 HUD 재개 기호 ▶ 하나, Jua는 ±·×–—’→−▶ 9기호입니다. 전체 한글 음절/35개 몬스터 명칭 누락은 없습니다. 이 전용 adapter는 버튼을 Noto700, 본문을 Noto400, 제목만 Jua로 선택해 기존 유한 코퍼스 기호 누락을 전용 화면에 추가하지 않습니다. 공유 HUD 기호의 해결은 공유 소유자 범위입니다.

## 검증 상태 / Validation status

- 이 리뷰에서 실행: read-only diff/call-chain/소유 경계 확인, 입력 SHA-256 고정, `git diff --check` PASS.
- 부모가 실행한 별도 검증 로그를 읽어 확인: `Docs/Implementation/TrainingGroundSkinEvidence20261014/static-checks.json`에 두 명령 exitCode=0, sourceHead=2e193c33, dirtyInput=true, unityInvoked=false가 기록되어 있습니다. `ui-contract.log`는 15 tests / OK, `ui-refresh.log`는 PASS: 16 bindings guarded; 191 UI source files inventoried입니다. 본 리뷰는 테스트를 재실행하지 않았습니다.
- Unity EditMode/runtime 및 2868×1320/portrait/KO/EN, click/touch/scroll/저장 실패 검증은 **부모 single Unity slot 대기**입니다. 기존 `Docs/Implementation/TrainingGroundSkinEvidence20261014/validation-queue.json`의 unityExecuted=false를 확인했습니다. 무음·분리 세이브·Mac simulation 표기를 유지해야 합니다.
- 실제 원형/둥근 모서리, Jua/Noto glyph 렌더링, EN wrap/줄임 없음, 비활성·선택·빈 상태는 실행 증거가 아직 없습니다. 소스 정적 검토를 실제 iOS 검증으로 부르지 않습니다.

## 검토 입력 SHA-256 / Reviewed inputs

변경/추가 후보:

| 파일 | SHA-256 |
| --- | --- |
| `Assets/HELLSCRIPT/Runtime/Presentation/TrainingGroundSkin.cs` | `ff5710e3e73de3fce6bd5c7d5a7b7109aa8fe3e9010767e62f6bd628faeb5222` |
| `Assets/HELLSCRIPT/Runtime/Presentation/TrainingGroundButtonSkin.cs` | `b1fe3fb5c849b15ff4ec4db0ca407e359af5504f456c14c359b70dbc7aa05d68` |
| `Assets/HELLSCRIPT/Runtime/Presentation/TrainingGroundWindow.cs` | `3e66b6597b2ec613c15e61f9ed8f52de88ecfd78d2615bc71aa8f1d56a6a9c58` |
| `Assets/HELLSCRIPT/Runtime/Presentation/TrainingGroundWindow.Hero.cs` | `a98a3d22fea929de52ea0ea38201e48277aa95eec241b4c1898ca5997cdcf707` |
| `Assets/HELLSCRIPT/Runtime/Presentation/TrainingGroundWindow.Skills.cs` | `0ac2b3c61d0254a4370ccbbde5f7af27c54547e866171d9d95636e54342a9d42` |
| `Assets/HELLSCRIPT/Runtime/Presentation/TrainingGroundWindow.Enemies.cs` | `5b754eb9fda317beb8c473073e08d62623bdfbbc7a2b96c0e304feea14885314` |
| `Assets/HELLSCRIPT/Runtime/Presentation/TrainingGroundWindow.Picker.cs` | `8779a03fab926f418c61b82e113dcad08fdb51fd3af80bc597fede726c96d10a` |
| `Assets/HELLSCRIPT/Runtime/Presentation/TrainingGroundResultWindow.cs` | `5f9440d41a63af3c56c6ff233b556f152bec4688c1dcc3a13ddfa0eab92f3e0b` |
| `Assets/HELLSCRIPT/Tests/Editor/TrainingGroundSkinTests.cs` | `c698160e0f3ccef72f7a189813c05a199c82f7a5b434772287de686f627a106e` |

공유 의존성(읽기만):

| 파일 | SHA-256 |
| --- | --- |
| `Assets/HELLSCRIPT/Runtime/Presentation/UiButton.cs` | `00ba7effcdd29a2de64c71dd0b0515d7507b468ecac295fd5fd2a8fe44de06a8` |
| `Assets/HELLSCRIPT/Runtime/Presentation/UiButtonFace.cs` | `fc7b5050fd2bafc13ea0edf13de7e9dad4055a9779c8489df6a3f011416e2685` |
| `Assets/HELLSCRIPT/Runtime/Presentation/StorageSurface.cs` | `a42dbcba961d906aa76f70525ac3b6c0a5a5bac4a497f90c8a38a3809a995f0d` |
| `Assets/HELLSCRIPT/Runtime/Presentation/ContentWindowView.cs` | `f9566a9c9f4b048860a695688c7c7d1c2d60707db1b0e683b9786e062dbfe196` |
| `Assets/HELLSCRIPT/Runtime/Presentation/UiTheme.cs` | `bc331fa0e87e8324abc90ca526353e8832e1c92e1f117a912eb48ca924c72af2` |
| `Assets/HELLSCRIPT/Runtime/Presentation/TrainingDpsPanel.cs` | `2fe2be7ff1530324fd5683edceb556cdb6a1ab1e9a29e11680c13eb6b1077eff` |
| `Assets/HELLSCRIPT/Runtime/Presentation/TrainingChartView.cs` | `dde480681fdd87d6b232e1fb1498f950a313934c2df10f66142b6d7e1496e90c` |
| `Assets/HELLSCRIPT/Runtime/Presentation/TrainingDpsChart.cs` | `d237fc72a441d97dd167f2393f2f61adf9f2464358bc0d0f98e27403e81382e0` |
| `Assets/HELLSCRIPT/Runtime/Presentation/RiftSkillShareView.cs` | `6647d627c93276b261c81c0d7771a43a591b89d0ee430858946477f90b6899c9` |
| `Assets/HELLSCRIPT/Runtime/Presentation/GameUI.TrainingGround.cs` | `2bf7eba07fe746992c109ffe411db9cfb02e65c817e3089824aa014b5cb479c9` |
| `Assets/HELLSCRIPT/Runtime/Presentation/SkillTreeD4/SkillTreeD4PaintGraphic.cs` | `9552886b93206f9fa428a5c0508d42f93bebc7695cc06365ac272b630db722a9` |

## English handoff

No definite compile blocker, input/save regression, or shared-file ownership violation was found by static review of the hashed candidate. The existing UiButton remains the input/state owner; its dirty-vertices callback drives a non-raycast local painter and unregisters on destruction. Disabled note alpha/color and rounded Mask geometry are present in source. Rendering, Unity compilation, and actual interaction still require the parent's serialized validation slot. The result DPS chart and contribution cards remain on shared legacy surfaces/fonts, so complete screen-wide visual unification is pending the common owner. The font and monster audits are separate deliverables and do not authorize shared source or font changes. No reviewed source, Unity state, worktree index, branch, or user save was changed by this reviewer.

검증 로그 SHA-256:

- `static-checks.json`: `e3ab85e625705f147cbf2467f446c4997c9dbd5bfb2818204d733ec95a077d4c`
- `ui-contract.log`: `9e9b85ea1a78ddb89f11f06bfa65dd10403b67d277290d4df38fe33f1f3a6e01`
- `ui-refresh.log`: `49d3663b8bcb7ca118075ce13a4b966972e8dc915e31f227ffd8eae2ed26b25e`
