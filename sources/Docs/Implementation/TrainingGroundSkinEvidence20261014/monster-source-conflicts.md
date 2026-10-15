# 몬스터 도감·공유 초상화 충돌 감사 / Monster codex and shared portrait audit

2026-10-14. 읽기 전용 소스·Git 객체 감사. 이 문서 외 파일을 쓰지 않았으며 원본/skill/활성 monsters 체크아웃·인덱스·브랜치는 수정하지 않았다. Unity·이미지 생성·이미지 편집·테스트·빌드·merge를 실행하지 않았다.

## 확인한 기준과 이력

| 대상 | 확인한 commit |
|---|---|
| 요청된 skill 기준 | `4139a77e2df495cee009c07c6094b2eea9b1aeaa` |
| 현재 skill-skin HEAD | `df2e366c18ba01eaf8a2ceaa145dfb8c940e5f7a` |
| 초상화/보스 배경 통합 | `57bba034dad1659e8a36dc9645b1291d2554e497` |
| 감사 시 local main | `04d8e38bf97486af2b97ced24f4c0be795d3d506` |
| 활성 monsters HEAD | `b732010a991cd0b1b2c732dac31de0917376b1bc` |
| 원본 checkout HEAD | `6378320b975117d7ba6c7a6b48bda0e853d4d124` |

활성 경로: `/Users/t8g-2410-pn-005/Documents/MyProject/HELLSCRIPT/HELLSCRIPT/.claude/worktrees/monsters-20261014`, branch `claude/monsters-recast`. 감사 시 `git status --short`는 비어 있었다. skill-skin도 상태가 깨끗했고, 원본의 기존 미커밋 Art Transition/Art Concept/AssetDeliverables는 그대로였다. 깨끗한 상태라는 사실은 작업 종료나 수정 권한을 뜻하지 않는다.

직접 확인한 merge-base:

- `main`과 `57bba034…`의 merge-base는 `57bba034…`: 최신 local main에 초상화 통합이 포함된다.
- 활성 `b732010a…`와 `57bba034…`의 merge-base는 `b732010a…`: 활성 몬스터 브랜치 이력도 이미 초상화 통합의 조상이다.
- skill `4139a77e…`와 `57bba034…`의 merge-base는 `53d6a3e13304a781f9823134ba0f497bafb78621`: skill 기준 자체에는 후속 초상화 변경이 없다.
- `57bba034…..main`에서 아래 도감 3개 파일, 추가 스모크 및 Portraits 경로의 후속 변경은 없다. 현재 main의 해당 blob은 57과 같다.

관련 이력:

1. `5b54fa2b0e79df9030b3fee7cbcbda590e7dd73d` — 35종 StyleD 초상화, ID fallback, 비율 유지/전체 UV/상단 정렬, 도감 라벨 위치·폭, 관련 검사.
2. `d0bccf65cf878d96de1d215b56e4f154525858c0` — 밝은 그림 위 라벨의 불투명 공통 배경·겹침 검사, 네이티브 fixture 격리와 차단 진단.
3. `57bba034dad1659e8a36dc9645b1291d2554e497` — PR #125 병합. 부모 `a113f4ff79ceed6a820b88556349dced09445edf`, `af95064ab794a4007f1ac4c53770e4aeef26413e`. 이 merge 자체는 B01–B06 배경/근거/프롬프트를 갱신한다. 얼굴 비율 코드는 앞선 두 commit에서 이미 들어왔다.

## 정확한 3개 파일과 delta

`MonsterCodexWindow*` runtime 소스는 두 개다. 요청의 3개 파일은 runtime 두 개와 관련 Editor 검사 `MonsterCodexWindowTests.cs`로 특정했다. 별도의 runtime smoke 변경도 함께 감사했다.

| 파일 | 4139 → 57/main delta | 의미 |
|---|---|---|
| `Assets/HELLSCRIPT/Runtime/Presentation/MonsterCodexWindow.cs` | +13 / −5 | 초상화 로딩·fit, 목록 라벨·칩 helper |
| `Assets/HELLSCRIPT/Runtime/Presentation/MonsterCodexWindow.Detail.cs` | 0 | 파일 자체는 동일하지만 변경된 Portrait/Chip/LabelWidth helper를 사용하므로 표시 결과에는 영향 |
| `Assets/HELLSCRIPT/Tests/Editor/MonsterCodexWindowTests.cs` | +39 / −2 | 비율·전체 UV·상단/중앙 위치·준비된 초상화·라벨 대비/겹침·실제 누락 fallback 보존 |
| 추가: `Assets/HELLSCRIPT/Runtime/Presentation/RuntimeMonsterCodexSmoke.cs` | +9 / −3 | 35장 존재·world pending 초상화 표시, 오프라인 전용 fixture 방해 제거·차단 화면 증거 |

세부 변경은 다음과 같다.

- 목록 `Boss ribbon`을 얼굴 위쪽에서 아래쪽으로 이동하고 boss/ID 폭을 실제 문자열 너비로 계산한다. `LabelWidth`는 `Ceil(preferredWidth)+2`가 된다.
- `Chip` 배경은 색상 12% alpha에서 `UiTheme.Background`로 바뀐다. 이 helper는 목록뿐 아니라 Detail의 등급·원소·스킬 tag에도 사용된다. 새 스킨이 밝은 판으로 바꾸더라도 불투명성·읽기 대비·얼굴을 가리지 않는 위치를 보존해야 한다.
- `Portrait`는 `artPending`을 로딩 금지 조건으로 쓰지 않는다. registry portrait가 비어 있으면 `Art/TrainingGround/Portraits/<id>`로 찾고, 실제 텍스처가 없을 때만 공통 placeholder를 사용한다. world 모델 대기와 2D 초상화 준비 상태를 분리한 것이다.
- 전체 UV를 유지하고 `size / max(texture.width, texture.height)`로 비율을 보존해 맞춘다. 가로 중앙 정렬, 세로 상단 정렬이다. 정사각형 강제 변형이나 중앙 crop으로 되돌리면 안 된다.
- 테스트는 세 형태의 텍스처(기존 NPC, training-yard, N01)를 통해 폭/높이 비율, 원본 전체 UV, 상단 좌표, 가로 중심과 raycast=false를 확인한다. world pending인데 준비된 초상화는 표시하고, 실제로 없는 경로는 placeholder가 나오는 두 경우를 별도로 검사한다.

## Git blob 및 SHA256

아래 A는 skill4139/활성 monsters b732의 동일한 구형 내용, B는 57/current main의 동일한 내용이다. `.Detail.cs`는 A/B 모두 같다.

| 파일 | A Git blob | B Git blob |
|---|---|---|
| MonsterCodexWindow.cs | `462a0ebb1cbec7ddde6bc72dd903e4a539213e73` | `613f17791650efd18fbfdd983f791e52b845af5d` |
| MonsterCodexWindow.Detail.cs | `eb039efffffa998cf3a58843692ad2b9cb4d0ab3` | `eb039efffffa998cf3a58843692ad2b9cb4d0ab3` |
| MonsterCodexWindowTests.cs | `f86e50424f78fa7d6fadbae4636bc9ac86b92a01` | `f004dd6cc6f35d25119299512c619b655dc8ca8c` |
| RuntimeMonsterCodexSmoke.cs | `fce03dfcc78e8cc8e315a17710fd02d2c206b8d3` | `cf7c8b2da8da5a4827e2ae22987a88bca1df6497` |

| 파일 | A SHA256 | B SHA256 |
|---|---|---|
| MonsterCodexWindow.cs | `7fd6fcad004555f0875293a3e214995e64b4022ca6c5d33e14073ccb2bf2cdb9` | `5287b768178da49d62a6221a9ee8328dfb76b6b110e5b3cd817a938a15308e15` |
| MonsterCodexWindow.Detail.cs | `ee4d47c4ed124871902916031dea7b010c24688d14d06d4b42c0908b40546655` | `ee4d47c4ed124871902916031dea7b010c24688d14d06d4b42c0908b40546655` |
| MonsterCodexWindowTests.cs | `2860023768602cacb241229e425dd24160e9a35743e318ad5e4bc20251579d11` | `f21a3d2c0bef6c0f0880681c059352cb4df669d6e6d5686680e9b2f3a2eed300` |
| RuntimeMonsterCodexSmoke.cs | `2ce51d4d598ee8d6c24e576597530542665517c9d93eedddd816ea8afb749956` | `1e8fa4d08684246d3b71128866fabd9aa5a453040caf37b141124745c3ae58f5` |

## 충돌 판단과 통합 경계

현재 확인한 도감 소스는 skill과 활성 monsters가 같은 A, main은 후속 B다. 이 범위에서 두 분기가 서로 다르게 고친 동시 코드 conflict는 발견하지 않았다. Git merge/cherry-pick을 실행하지 않았으므로 실제 merge 무충돌을 판정한 것은 아니다. 앞으로 skin 변경이 `RenderList`, `LabelWidth`, `Chip`, `Portrait` 및 해당 검사를 만지면 B 변경과 같은 줄/동작을 건드린다.

따라서 오래된 활성 worktree의 A 파일을 통째로 복사해 새 스킨을 만들면 이미 통합된 얼굴 fit/ID fallback/라벨 대비/검사를 잃는다. 최신 main/57의 B 동작을 보존하는 변경 단위가 필요하다. 활성 monsters 작업 폴더는 동의 없이 checkout·reset·수정하지 않는다. `.Detail.cs`에 textual delta가 없다는 이유로 helper 변경의 영향까지 없다고 판단하지 않는다.

skill4139와 main57은 공통 조상 뒤 각자 변경이 있는 분기다. `git diff 4139 57` 전체를 patch로 적용하면 skill 스킨 파일 변경을 역방향으로 되돌릴 위험이 있으므로 전체 diff를 통합 수단으로 사용하지 않는다. 화면별 변경과 이미 병합된 초상화 이력을 부모의 통합 절차에서 조합해야 한다.

## 공유 초상화 소비자 영향

`57` Git 객체의 PNG header/내용을 직접 읽어 **35 PNG, 전부 1254×1254, 합계 63,338,362바이트**를 확인했다. 일반 29장+보스 6장이다. 정렬된 `id + 공백 + SHA256` 행을 줄바꿈으로 연결한 인벤토리 SHA256은 `d2673179e09c6904b6690f1773ca199b8d3ebd98d76310b9c71ed1c28622948c`다. 이후 main에서 해당 경로 변경은 없다. 이미지를 새로 생성하거나 복제하지 않았다.

| ID | 57/main PNG SHA256 |
|---|---|
| B01 | `09bcd33be19c7f3eb6b66356b16ec69dfab0e8abc4312fab8f6a6c3627b6b78d` |
| B06 | `c1c6b7ee11ff0f5d53c4eb539470361949c8cfa72790a94f06dfd986f0139e34` |
| N01 | `dc925d19ec93b167f6554ccdbb6419a482575e449cead1cdc8e2be0b4c547d24` |
| N23 | `80341cdf7618e6315dcb294665ce3623b38f254b14c5c8e63ec9952114e3ea58` |

자세한 provenance는 commit57의 `Docs/Art/Monsters/Casual20261014/generation-manifest.json`, `boss-background-manifest.json`, `Docs/Art/Monsters/portrait-manifest.json`을 따른다. 문서의 과거 검사 통과는 이번 audit에서 재실행한 결과가 아니다. 문서에는 생성 모델 식별자 unknown, productionApproved:false도 기록돼 있으며 이 audit가 그 메타데이터를 바꾸거나 별도 출시 승인을 선언하지 않는다.

도감과 훈련장은 같은 `Resources/Art/TrainingGround/Portraits` 파일을 소비한다. 등록된 path가 있는 기존 33종은 공유 PNG 교체만으로 두 화면의 그림이 함께 바뀐다. 이 원화는 도감 담당의 화면 전용 복사본이 아니다. 전역 리소스를 훈련 스킨에서 다시 생성·교체하지 않는다.

주의할 실제 연결 차이:

- `MonsterRegistry.json`은 4139/b732/57/main에서 byte-identical이다. Git blob `d60e5986f92eb55b00e0c58133a2b7ec0dc5d10b`, SHA256 `f9703d0d6e26cd3874e7b632fc85fef34a0f38e707e64a4d70a12357aaa3093c`.
- N23와 B06은 여전히 `artPending:true`, `portrait:""`다. 도감 B helper는 ID fallback으로 준비된 두 초상화를 표시한다.
- `TrainingGroundWindow.cs:99–107`의 public Portrait helper는 registry portrait만 읽고 빈 path면 RawImage를 숨긴다. `TrainingGroundWindow.Enemies.cs:99`와 `.Picker.cs:32`가 이 helper를 쓴다. 따라서 새 PNG가 존재해도 **훈련장에서는 N23/B06이 계속 빈 프레임**으로 남는다. 이는 공유 리소스 이관만으로 해결되지 않는다.
- 훈련 helper는 이미지를 정사각형 RawImage에 넣는다. 도감의 비율 보존 보정은 여기에 적용되지 않았다. 다만 확인한 새35PNG가 모두 정사각형이므로 이 파일 세트에서 현재 비율 왜곡이 발생한다고 주장하지 않는다. 비정사각형 fallback/향후 이미지에는 차이가 생긴다.
- `TrainingGroundWindow.cs` 자체는 네 기준에서 동일한 blob `fb960f6ac28d0fa40a1288a7642d8d5e21285250`, SHA256 `7878497cf035f3502df2d7f87318295954d1ce004cb67e1e6803017ea9dfa9a3`다. 훈련 담당이 승인받은 전용 파일 안에서 ID fallback/contain fit을 적용할지, 공통 portrait helper로 옮길지는 부모가 정한다. registry의 world-pending 상태를 거짓으로 해제해서 해결하지 않는다.

이식 후 최소 확인은 도감17개 검사의 현재 버전 보존과 관련 실행, 훈련 로비/적 선택에서 N23/B06 표시, 일반·보스/미발견·누락 fallback, KO/EN 라벨과 네 모서리·얼굴/뿔 영역이다. UI/폰트 변경 후에는 과거 native evidence만으로 새 표시를 통과 처리하지 않는다. Unity 실행은 부모의 단일 실행 순서를 기다린다.

## English handoff

The requested three files are the two runtime partials, MonsterCodexWindow.cs / MonsterCodexWindow.Detail.cs, plus Tests/Editor/MonsterCodexWindowTests.cs. RuntimeMonsterCodexSmoke.cs was audited additionally. Relative to skill4139, main57 changes the first by +13/−5 and the tests by +39/−2; Detail is byte-identical but calls the changed portrait/chip/label helpers. The smoke changes by +9/−3.

Local main `04d8e38…` contains `57bba034…`; there have been no later changes to these files or the portrait resource directory. The active monsters worktree remains clean at `b732010a…`, an ancestor of 57, and its inspected source is the older baseline content. No active worktree was edited. The skill branch has advanced to `df2e366c…`; this audit compares the explicitly requested `4139a77e…` baseline.

Preserve the already merged changes from `5b54fa2b…` and `d0bccf65…`: load ready portraits independently of world-model pending state, fall back to the ID resource path, preserve full UV/aspect ratio with horizontal centering and top alignment, keep readable opaque badges below faces, and retain the corresponding tests. The later 57 merge changes the six boss backgrounds and supporting provenance. Copying the old active-worktree files would regress this work. No merge was performed, so this is a content/ancestry analysis rather than a completed merge-conflict test. New skin changes overlapping list construction or Portrait/Chip/LabelWidth need careful integration; Detail can change behavior even when its text has not changed.

All 35 committed PNGs at 57 are 1254×1254 and total 63,338,362 bytes. Their paths are shared by codex and training. The unchanged registry still has blank portrait paths and artPending=true for N23/B06. Codex now uses the ID fallback, but TrainingGroundWindow.Portrait does not: the training roster/picker still hide these two ready portraits. Training also retains a square RawImage; the new square assets do not currently distort, but the codex contain-fit behavior is not shared. Decide a scoped training helper update or shared API through the parent; do not alter world-model availability to force portrait display, replace global art again, or overwrite active worktrees.

Only this supplement was created. No source edits, generated images, Unity, tests, commits, pushes, merges, or active-worktree mutations occurred. No tool action was denied or left with an uncertain result. Source/asset hashes above are direct Git-object checks; historical validation receipts were not replayed.
