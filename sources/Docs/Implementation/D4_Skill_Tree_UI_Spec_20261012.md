# 스킬창 화면 구현 명세 (Codex 전달용)

작성일: 2026-10-12 · 대상: Codex 구현 작업 · [English](D4_Skill_Tree_UI_Spec_20261012.en.md)

작성: Claude (명세와 검증 자료). 구현·검증·커밋은 Codex가 한다.

이 문서는 계획 문서 [`D4_Skill_Tree_Plan_20261012.md`](D4_Skill_Tree_Plan_20261012.md)의 9·10·11·13장을 **구현할 수 있는 수준**으로 풀어 쓴 것이다. 목표는 하나다: **게임 안의 스킬창이 시안과 같은 모양, 같은 동작이다.** 규칙·결정은 계획 문서가, **수치는 `Docs/Design/D4SkillTree/golden/`의 정답 파일이** 우선한다. 문서끼리 충돌하면 그 순서로 따른다.

## 0. 자료 지도와 읽는 순서

| 자료 | 내용 | 우선순위 |
|---|---|---|
| 계획 `D4_Skill_Tree_Plan_20261012.md` | 규칙, 결정(17장), 단계(14장) | 규칙·결정 |
| `golden/*.json` | 시안을 실제로 돌려 얻은 수치: 규칙, 배치, 문구, 상호작용, 내부 치수, 모양 | **수치** |
| `golden/csharp/` | 위 수치를 모두 통과하는 C# 이식본(규칙·배치·화면 정보·세션) | 출발 코드 |
| 이 문서 | 화면을 uGUI로 옮기는 방법 | 방법 |
| `golden/anatomy/OUTLINE.md` `TEXT_STYLES.md` | 상세 패널과 위젯 내부의 실측 사각형, 글자 스타일 129종 | 읽기 자료 |
| `research/ui_infrastructure.md` | 지금 그리기 도구·창 구조·입력·시험의 사실과 격차 | 근거 |
| `reference/*.jpg` | 기준 렌더 24장(2배) | 눈으로 확인 |
| `SkillWindow.dc.html` | 시안 원본 | 마지막 수단 |

읽는 순서: `golden/README.md` → 이 문서 1~3장 → 맡은 작업 묶음(13장) → 필요한 장.

## 1. 충실도 계약과 검증 층

"똑같이"를 사람의 눈에만 맡기지 않는다. 여섯 층으로 나눠 **기계가 확인하는 층을 먼저, 눈으로 보는 층을 마지막에** 둔다.

| 층 | 무엇을 | 기준 자료 | 어떻게 | 통과 기준 |
|---|---|---|---|---|
| L0 규칙 | 투자·회수 판정, 막힌 이유, 배분 해시 | `rules_golden.json` | `SkillTreeD4RulesTests` | 32,208개 일치 |
| L1 배치 | 모든 요소의 사각형·상태·글자, 그리는 순서 | `layout/*.json`, `layout_scenarios/*.json` | `SkillTreeD4LayoutTests` (순수 함수) | 2,975 + 15,034개 일치 |
| L2 문구 | 패널·탭·머리 수치·접근성 문구 | `viewmodel/*.json` | `SkillTreeD4ViewModelTests` | 600개 일치 |
| L3 상호작용 | 선택·투자·회수·탭 60건의 걸음마다 상태와 연출 신호 | `interaction/*.json` | `SkillTreeD4InteractionTests`, 스모크가 같은 60건을 **실제 클릭**으로 재생 | 4,986개 일치, 클릭 재생 일치 |
| L4 내부 치수 | 상세 패널 6가지 상태와 위젯(카드·배지·패시브·탭·범례 …)의 안쪽 사각형, 글자 크기 | `anatomy/*.json` | `SkillTreeD4AnatomyTests`: 실제 uGUI 사각형을 재서 비교 | 사각형 ±1pt, 글자 상자 ±2pt, 글자 크기는 격자(0.5pt)에 맞춘 값 |
| L5 모양 | 색·그라데이션·테두리·번짐 | `styles.json`, `reference/*.jpg` | 스모크 스크린샷을 기준 렌더와 비교 (14장) | 합격선 안 |

L0~L3는 이식본(`golden/csharp`)을 복사하면 **이미 통과한다**(55,803개 검사, `cd Docs/Design/D4SkillTree/golden/csharp && dotnet run`). 구현 작업은 이를 Unity EditMode 테스트로 옮기는 것으로 시작한다. L4·L5가 UI 구현의 몫이다.

**같아야 하는 것**
- 규칙 판정과 막힌 이유(해시까지), 요소 수와 그리는 순서, 모든 사각형(±0.01pt, 화면에서는 ±1px), 상태 이름, 카드 글자 크기, 문구(한국어·영어), 상호작용 결과.
- 상세 패널의 구성과 요소 위치(L4), 설명 자리의 예약 높이(세로·가로 2줄, PC 3줄).
- 색은 `styles.json`의 값 그대로(토큰으로 옮긴 뒤 hex가 같아야 한다).

**달라도 되는 것 — 허용 근사 (이 목록 밖의 차이는 결함이다)**

| 번호 | 항목 | 허용 범위 | 이유 |
|---|---|---|---|
| A1 | 글꼴 모양 | 17장 D6의 결정에 따른다. 글자 폭이 달라 줄바꿈 위치가 달라질 수 있다. | 시안은 Noto Serif/Sans KR, Cinzel. 게임은 `UiFonts`. |
| A2 | 상세 패널 배경 그림의 블러 | 셰이더 블러(권장) 또는 같은 위치·크기·불투명도의 흐림 없는 그림 | 아이콘은 읽을 수 없는 텍스처라 CPU 블러가 불가능(`research/ui_infrastructure.md` 갭 6). |
| A3 | 그라데이션 글자(제목·이름) | 위 → 아래 2색 그라데이션. 중간 색 정지는 생략 가능 | 글리프 사각형마다 정점 색이 2개뿐. |
| A4 | 가장자리 부드러움 | 직접 그린 메시는 ±0.5px 거칠 수 있다. 모서리 둥근 도형은 1px 페더를 권장 | `UiMesh`에 안티에일리어싱이 없다. |
| A5 | 바깥 번짐·안쪽 그림자 | 같은 색·퍼짐을 3~6단 링으로 근사 | CSS `box-shadow`/`inset`은 엔진에 없다. |
| A6 | 연출 | 길이·곡선은 `fx.json`을 따르되 프레임 단위 일치는 요구하지 않는다(±10%). | |
| A7 | 투명도 합성 | 프로젝트가 Linear 색공간이라(`ProjectSettings` `m_ActiveColorSpace: 1`) 번짐이 시안보다 밝거나 어두울 수 있다. 전체 보정 상수 1개로 맞춘다. | CSS는 감마 합성. |
| A8 | 작은 도형의 안티에일리어싱 | 6px 미만 점·마름모는 정수 픽셀에 맞춰도 된다. | |

## 2. 코드 구조와 데이터 흐름

### 2.1 파일

| 위치 | 파일 | 책임 | 출처 |
|---|---|---|---|
| `Runtime/Core/SkillTreeD4/` | `SkillTreeD4.Model.cs`, `SkillTreeD4.Rules.cs` | 데이터 모델, 배분 문자열·해시, 규칙 엔진 | `golden/csharp` 복사 (**1단계**에서 이미 들어와 있다) |
| `Runtime/Core/SkillTreeD4/` | `SkillTreeD4.Data.cs` | `Resources/Data/SkillTreeD4.json` → 모델 (`JsonUtility` DTO) | 새로 작성(1단계) |
| `Runtime/Presentation/SkillTreeD4/` | `SkillTreeD4.Layout.cs`, `.ViewModel.cs`, `.Session.cs`, `.Fx.cs` | 순수 배치, 화면 정보, 상태기계, 연출 신호 (UnityEngine 없음) | `golden/csharp` 복사 (**UI-1**) |
| 같은 폴더 | `SkillTreeD4Units.cs` | 디자인 점 ↔ 화면 단위(3장) | 새로 작성 |
| 같은 폴더 | `SkillTreeD4Window.cs` + `.Scene.cs` `.Panel.cs` `.Header.cs` `.Input.cs` `.Fx.cs` `.Look.cs` | uGUI 창 | 새로 작성 |
| 같은 폴더 | `SkillTreeD4Graphics.cs` | 새 그리기 요소(5장) | 새로 작성 |
| `Runtime/Presentation/` | `UiButtonFace.cs`(`UiMesh`), `UiTheme.cs` | 그리기 함수 추가, 색 토큰 추가 | 수정 |
| `Tests/Editor/` | `SkillTreeD4{Rules,Layout,ViewModel,Interaction,Anatomy,Graphics}Tests.cs`, `GoldenJson.cs`, `GoldenShapes.cs` | 시험 | `golden/csharp/Tests` 복사 후 EditMode로 |
| `Presentation/` | `RuntimeSkillTreeD4Smoke*.cs` | 스모크 `-hellscriptSkillTreeD4Smoke` | 새로 작성 |

`golden/csharp`의 파일은 `namespace Hellscript`, C# 9, `System`만 쓴다. 이름과 폴더는 게임 규칙에 맞게 바꿔도 되지만 **동작은 L0~L3 시험을 계속 통과해야 한다.** 좌표는 `double`이지만 오차가 0.01pt이므로 `float`로 바꿔도 된다.

### 2.2 흐름

```
GameStore(영웅별 레벨, 배분)
   │  읽기                         ▲ 커밋(검증 후 저장)
   ▼                               │
ISkillTreeD4Host ── 창이 쓰는 유일한 저장소 경계 (아래 2.3)
   │
SkillTreeD4Session  (탭, 영웅별 선택, 영웅별 배분 사본)  ← 사용자 입력: Select / Invest / Refund / Tab
   │ Context()
   ├─► SkillTreeD4Layout.Build   → SkillTreeD4Scene   (사각형·상태·글자, 그리는 순서)
   └─► SkillTreeD4ViewBuilder    → SkillTreeD4View    (패널·탭·머리 문구, 접근성 문구)
                │
          SkillTreeD4Window (uGUI)  ← 장면의 키 비교로 **상태만** 갱신, 지우고 다시 만들지 않는다
```

- 장면과 화면 정보는 입력이 같으면 항상 같은 결과를 내는 순수 함수다. 창은 선택·배분·언어·레벨이 바뀔 때마다 다시 계산하고 **노드 키로 차이만 반영**한다(4.4).
- 저장소가 진실이다. `Session`의 배분은 창이 열려 있는 동안의 사본이고, **투자·회수가 받아들여지면 즉시 `host.Commit`**으로 저장한다. 커밋이 실패하거나 `visibleState` 키가 바뀌면(다른 창·전투 빠른 편집이 바꿈) 호스트 값으로 다시 맞추고 **선택은 유지**한다.

### 2.3 호스트 경계

창은 `GameStore`를 직접 알지 않는다. 시험과 스모크가 저장소 없이 창을 띄우려면 이 경계가 필요하다.

```csharp
public interface ISkillTreeD4Host
{
    int Level(string hero);                                   // 영웅 레벨 (1..60)
    SkillTreeD4Alloc Alloc(string hero);                      // 저장된 배분 (읽기 전용 사본)
    bool Commit(string hero, SkillTreeD4Alloc alloc);         // 규칙 검증 뒤 저장. 거부하면 false
    string VisibleState();                                    // StoreViewBinding의 visibleState 키 (배분·레벨·언어에 의존하는 값)
    bool PlayerCanEdit(string hero);                          // 비교·훈련·튜토리얼 안에서 막을 때 false
}
```

- 운영 구현은 6단계에서 `GameStore` 위에 만든다. 4단계(UI)는 메모리 구현(`InMemorySkillTreeD4Host`)으로 개발·시험한다.
- 영웅마다 레벨이 다르다. 시안은 24 하나로 고정했지만 **세 수치와 패시브 해금은 탭에 있는 영웅의 값**이다(계획 9.3).

### 2.4 이름 규칙 (시험·스모크·튜토리얼이 찾는 이름)

옛 창은 `edict-…` 이름을 썼다. 새 창은 아래 이름을 **GameObject 이름**으로 쓴다. 튜토리얼 가리킴(6단계)과 스모크가 이 이름을 쓴다.

| 요소 | 이름 |
|---|---|
| 창 루트 / 상세 패널 | `d4-window`, `d4-panel` |
| 머리 | `d4-title`, `d4-tab-<0..2>`, `d4-stat-<level\|spent\|left>`, `d4-close` |
| 트리 판 / 스크롤 내용 / 범례 | `d4-plate`, `d4-content`, `d4-legend` |
| 티어 표지 | `d4-badge-<0..4>` |
| 액티브 카드 | `d4-card-<id>` (예: `d4-card-W09`) |
| 보조 스킬 칸 | `d4-mod-<id>` (예: `d4-mod-W09-2`) |
| 패시브 | `d4-passive-<id>` |
| 태그(랭크 칩 등) | `d4-tag-<for>-<id>` |
| 연결선 묶음 | `d4-lines`, `d4-arrows` |
| 패널 안 | `d4-panel-name`, `d4-panel-desc`, `d4-panel-mod-<0..2>`, `d4-panel-stat-<n>`, `d4-panel-invest`, `d4-panel-refund`, `d4-panel-reason`, `d4-panel-link-<n>` |

접근성·시험 문구는 `SkillTreeD4View`가 준다(`CardLabel.Aria` 등). 버튼의 `UiButton` 이름과 시험용 설명에 그 문구를 쓴다.

## 3. 창 껍데기, 단위, 진입점

### 3.1 단위: 디자인 점을 화면에 맞춘다 (fit-and-centre)

시안 좌표는 디자인 점이다. 틀은 세 가지다.

| 배치 | 틀 | 안전 영역(틀 안, 점) | 틀을 쓰는 조건 |
|---|---|---|---|
| 세로 | 440×956 | (0, 62) – 440×860 | 모바일이고 안전 영역 높이 > 너비 |
| 가로 | 956×440 | (62, 0) – 832×419 | 모바일이고 그 외 |
| PC | 1152×648 | 틀 전체 | 데스크톱(웹 데스크톱 포함) |

모바일/데스크톱 판단은 프로젝트에 이미 있는 플랫폼 판단을 쓴다(`Application.isMobilePlatform`을 `grep`으로 찾아 같은 방식). 새로 만들지 않는다.

```
S  = UiSafeArea.Current (픽셀 사각형)
P  = min(S.width / safe.w, S.height / safe.h)            // 디자인 점 1개 = 화면 P 픽셀
틀의 왼쪽 위(픽셀) = S.center − safe.center × P            // 틀의 안전 영역이 S 한가운데에 맞는다
```

- **캔버스 단위 = 디자인 점의 절반.** `CanvasScaler`는 `ConstantPixelSize`, `scaleFactor = P / 2`. 모든 좌표·크기·글자 크기를 디자인 점 × 2로 쓴다. 시안에 반 포인트 글자(10.5·12.5·14.5pt)가 많아서, 단위를 반 점으로 잡으면 글자 크기가 모두 정수가 된다(`Text.fontSize`는 정수다).
- PC 확대(`plate.inner.scale`, 0.99~1.18)가 곱해진 글자는 정수로 반올림하며 오차는 0.25점 이하다. **확대는 `localScale`이 아니라 좌표·크기·글자 크기에 곱해서** 적용한다. `localScale`로 키우면 글자가 흐려진다.
- 배경(어두운 바탕과 방사 그라데이션)은 틀 밖까지 화면 전체를 칠한다. 틀의 안전 영역 밖(예: 세로 위 62pt)은 노치 아래라 배경만 보인다.
- 크기만 바뀔 때(창 크기 조절, 안전 영역 변화)는 `scaleFactor`와 틀 위치만 바꾼다. **다시 만들지 않는다.** 배치 종류·언어가 바뀔 때만 장면을 다시 만든다.
- 시험 벡터(EditMode, `SkillTreeD4UnitsTests`):

| 화면(px) / 안전 영역 | 배치 | P | 틀 왼쪽 위(px) |
|---|---|---|---|
| 956×440, 안전 (62,0)–832×419 | 가로 | 1.0 | (0, 0) |
| 440×956, 안전 (0,62)–440×860 | 세로 | 1.0 | (0, 0) |
| 1080×2400, 안전 없음 | 세로 | 2.4545 | (0, −7.6) |
| 2400×1080, 안전 없음(모바일) | 가로 | min(2400/832, 1080/419) = 2.5776 | (−32.1, 0) |
| 1920×1080 데스크톱 | PC | 1.6667 | (0, 0) |
| 2560×1080 데스크톱 | PC | 1.6667 | (320, 0) |

### 3.2 창 껍데기

- 어댑터 창이다. 시안 좌표를 그대로 쓰는 특수 배치라 `ContentWindowView` 대신 어댑터로 둔다. 대장간 창(`BlacksmithWindow`)이 선례다: 자기 `Canvas`(`ScreenSpaceOverlay`, `CanvasScaler`, `GraphicRaycaster`)를 GameUI 아래에 만들고 `ContentWindowHost.Attach(this, Escape)`로 창 스택·뒤로 가기·일시정지에 들어간다. 정렬 순서는 호스트가 다시 쓴다.
- `StoreViewBinding.Attach(this, store, Repaint, ready, visibleState)`에 **visibleState 키를 반드시** 준다(`tools/check_ui_refresh.py`가 요구). 키에 넣을 것: 영웅 3명의 레벨과 배분, 언어, 계정 포인트 관련 값. 누르는 동안·스크롤하는 동안에는 `ready`가 false를 돌려 다시 그리기를 미룬다.
- 다시 맞추기(`Reflow`)는 대장간처럼 폴링이다: `UiSafeArea.Current`, `Loc.Language`, 배치 종류가 바뀌었는지 `Update`에서 본다.
- **`tools/check_ui_contract.py`의 `ADAPTERS`에 등록한다.** 필수 토큰은 실제로 쓰는 것으로 정한다: `ContentWindowHost.Attach`, `SkillTreeD4Units`, `UiSafeArea.Current`. 계획 9.1의 `UiTheme.Scale`은 이 창이 쓰지 않는 값이므로 **토큰만 맞추려고 호출하지 않는다**(`research/ui_infrastructure.md` 위험 7). 이유와 검증 방법을 `Docs/Implementation/Shared_UI_Contract(.en).md`에 어댑터 한 절로 쓴다(`AgentRules/Shared_UI.md:12`). 지금 있는 "스킬 트리 어댑터" 절은 옛 트리를 설명하므로 UI-3에서 새 어댑터 절을 더하고, 7단계에서 옛 절을 새 창 설명으로 바꾼다.
- 색 리터럴은 `UiTheme`에만 둔다(6장). 한국어 문구 리터럴은 `en.txt` 항목이 있어야 하고(`LocalizationTests`), `GetComponent … ?.`는 금지다(`EditorNullPatternTests`): `TryGetComponent`를 쓴다.

### 3.3 진입점

- 옛 스킬 트리 페이지를 직접 여는 호출만 새 창으로 바꾼다: `GameUI.ShowEdictSkillTree()`와 `HuntEdictWindow.SelectSkillPage(true)`. 호출하는 곳(구현 전에 `grep`으로 다시 확인): `RiftEntryWindow.cs`(빈 스킬 칸), `GameUI.FirstPlay.cs`, `HuntEdictWindow.SkillPresets.cs`("스킬 트리로" 버튼), `HuntEdictWindow.cs`(뒤로 가기), `HuntEdictWindow.Presets.cs`(저장된 프리셋이 검증에 실패하면 스킬 탭으로 이동). 자세한 목록과 삭제·유지 구분은 `research/dependents.md` A장.
- 칙령 창 전체를 여는 진입점은 그대로 둔다. 새 창은 칙령 창 "skills" 탭의 "스킬 트리" 버튼으로 들어간다(계획 D1).
- **부모 창 갱신 위험(계획에 없던 것):** 칙령 창의 `HuntEdictEditSession`은 창을 열 때 한 번 만들어지고 자기 커밋 뒤에만 갱신된다. 위에 뜬 스킬창이 커밋하면 부모의 저장 버튼이 옛 값으로 되돌릴 수 있고 장착 바에 옛 랭크가 보인다. 스킬창이 커밋할 때 `Committed` 이벤트를 올리고, 부모는 그때 `AcceptSaved`/재생성한다.

## 4. 장면을 uGUI로 옮기기

### 4.1 계층(그리는 순서)

장면의 `Primitives`는 **그리는 순서**로 나열되어 있다(뒤가 위). 같은 순서를 계층으로 옮긴다.

```
d4-window (Canvas)
 ├ Backdrop                     화면 전체. 바탕색 + 방사 그라데이션 3개 + 비네트 (styles.json frame.root.*)
 └ Frame                        틀 좌표계 (3.1)
    ├ Header                    제목, 탭 3, 머리 수치 3, 닫기
    ├ Plate                     트리 판: 테두리·배경(plate.*) ← 아래에 깔림
    │   ├ Viewport (RectMask2D) 스크롤 보기 영역
    │   │   └ Content           크기 = 장면 plate.inner (PC는 확대·이동 적용)
    │   │       ├ Bands         패시브 선반·머리글
    │   │       ├ Trays         보조 스킬 홈(세로)
    │   │       ├ Lines         모든 연결선을 한 메시에, 장면 순서대로
    │   │       ├ Arrowheads    화살촉 한 메시
    │   │       ├ Badges        티어 표지 5
    │   │       ├ Cards         액티브 카드 10
    │   │       ├ Modifiers     보조 스킬 칸 28(궁극기는 2칸)
    │   │       ├ Passives      패시브 10
    │   │       ├ Tags          랭크 칩·레벨 칩·해금 레벨·패시브 랭크
    │   │       └ FxContent     선 연출(점화·반짝임)은 Lines 위, 노드 연출은 각 노드 위
    │   ├ Legend                범례 띠(아래 24pt, 판 안쪽, 입력 받지 않음)
    │   └ CornerCaps            모서리 장식: **마스크 밖** 오버레이(판 가장자리 바깥으로 나옴)
    └ Panel                     상세 패널(8장). 모서리 장식도 마스크 밖
```

- 같은 종류 요소는 순서가 상관없다(겹치지 않음). 종류 사이의 순서는 위 표를 지킨다. **선은 카드·칸·패시브 아래, 태그는 모두 위다.**
- 선은 상태 색별로 나누지 않고 **장면 순서 그대로 한 메시**에 넣는다. 선마다 바탕 빛(밑줄)과 본 선을 이 순서로 그려야 겹침이 시안과 같다(SVG 그리기 순서). 선분마다 GameObject를 만들지 않는다.
- 모서리 장식(`.gx-cap-l`, `.gx-cap-t`, `.gx-pan-*`)은 `fx.json`의 `rules`에 CSS 그대로 있다. 판 가장자리에서 6pt 바깥으로 나오는 장식이 있으므로 `RectMask2D` 안에 두면 잘린다.

### 4.2 요소 종류별 구현

| 장면 요소 | 뷰 | 그리는 방법 | 상태가 바뀌면 |
|---|---|---|---|
| `Band` passive-shelf / passive-heading | `BandView` | 둥근 사각형 그라데이션+테두리(`band.*`) / 글자 | 색만 |
| `Tray` | `TrayView` | 둥근 사각형, 안쪽 그림자(`tray.learned`·`tray.unlearned`) | 테두리 실선↔점선 |
| `Line` + `Arrowhead` | `LinesGraphic`, `ArrowsGraphic` | `PolylineGraphic`(5장): 가로·세로 선분, 둥근 끝, 점선(`k`), 바탕 빛(`line.*.glow`) | 선 상태 색(정점 색만 바꿈) |
| `Badge` | `BadgeView` | 육각 메달(로마 숫자, `Cinzel`), 이름, 조건 글자, 잠김 자물쇠(세로), **다음 잠긴 티어의 진행 막대**(`Bar`, `bar.tier`, 너비 = `Bar`%) | 열림/다음 잠김/잠김 |
| `Card` | `CardView` | 둥근 사각형(`card.box.*`), 아이콘 소켓+아이콘, 이름(맞춤 글자 크기), **랭크 막대**(`Bars`: `rank`·`ghost`, 아래 가장자리 2pt), 세로의 알약 `n/max` | 상태 l/o/k, 선택, 보조 포커스, hot(ghost 막대 있음) |
| `ModifierSocket` | `ModifierView` | 판정 영역(`Hit`)과 얼굴(`FaceW×FaceH`), 아이콘(`modifier.icon.*`) | chosen/available/idle/unlearned/tier-locked, focused |
| `PassiveNode` | `PassiveView` | 은 동전 메달 + **랭크 호**(`Arc`) + 아이콘 / 핵심은 금 육각 / 세로는 캡슐(이름·"Lv2 해금 · 3 / 5") | l/o/k, 선택, **avail**(투자 가능 번짐) |
| `Tag` | `TagView` | 알약·원형 칩 + 글자(`tag.*`) | 글자, 켜짐 |

- **상태→모양**은 `styles.json`의 이름으로 정해진다. 장면 요소의 필드(상태, 선택, hot, avail …)로 이름을 만드는 규칙은 `golden/lib/scene.js`의 `S(label, …)` 호출에 있다. 모양은 이름 하나로 정해진다(생성기가 확인). 상세 패널 안쪽(단추·보조 행·패시브 블록)의 이름은 노드마다 `golden/panel_parts.json`에 있다(6장). `{pct}`·`{arc …}` 자리표시는 요소마다 다른 값이다(카드·배지 막대 너비, 랭크 호의 켜진 칸 수).
- **랭크 호**: 칸 수 `Total`(5, 궁극기 3), 켜진 칸 `Lit`. 칸 k의 각도는 12시부터 시계 방향으로 `[g/2 + k·seg, g/2 + k·seg + seg − g]`, `seg = 360/Total`, 틈 `g = Total > 3 ? 8° : 10°`. 켜진 칸 `#eaf6ff`(동전) 또는 `#f2d9a4`(상세 패널 액티브), 꺼진 칸 `#1a2126` 또는 `#2a241c`. 호의 두께는 동전 2pt, 상세 패널 패시브 3pt.
- **육각형**: 꼭짓점 `(50% 0, 93% 25%, 93% 75%, 50% 100%, 7% 75%, 7% 25%)`, 뾰족한 위쪽. 배지 메달 27×30, 핵심 패시브 38×38.
- 요소 안쪽 사각형(아이콘 위치, 글자 상자)은 **L4 기준**(`anatomy/OUTLINE.md`의 `widgets.*`)이다. 장면은 바깥 사각형만 준다.

### 4.3 PC, 스크롤, 클리핑

- 세로: 트리가 **위아래로 스크롤**된다(내용 420×1460, 판 안쪽 약 422×404). "패시브 보기"로 열면 `views.passives.initialScrollTop`(`passTop − 200`)에서 시작한다. 탭을 바꿔도 스크롤은 유지(범위 안으로 보정)한다.
- 가로: **좌우로 스크롤**(내용 718(마법사 854)×332, 판 548×357). 내용 높이 332가 보기 높이 355 안에 들어가므로 세로 스크롤은 없다. 범례 띠는 아래 24pt 위에 겹쳐 그린다. 판 오른쪽 가장자리에 28pt 페이드(`.gx-cap-l::after`)가 있다.
- PC: **스크롤 없음.** 내용 전체를 `plate.inner.scale`배(최대 1.5)로 키워 `plate.inner.offsetX/offsetY`에 둔다. 좌표·크기·글자 크기에 곱한다(3.1).
- 스크롤은 한 축만 쓴다. 두 축 스크롤은 프로젝트에 선례가 없다(`research/ui_infrastructure.md` 3장). 다시 그릴 때 스크롤 위치를 저장하고 `Canvas.ForceUpdateCanvases()` 뒤 보정해서 되돌린다(`ContentWindowView`, `StorageWindow`의 선례).
- 카드·칸·패시브를 누르는 동작은 `UiButton` 규칙을 따른다. 끌면(스크롤) 누름이 취소되도록 `ScrollRect`와 같은 `EventSystem` 경로를 쓴다.

### 4.4 상태만 갱신하기 (지우고 다시 만들지 않는다)

- 장면은 `Build`를 다시 불러서 얻는다(작다). 창은 요소마다 **안정된 키**로 이전 뷰와 맞춘다:

| 종류 | 키 |
|---|---|
| 카드 / 보조 칸 / 패시브 | `card:<id>` / `mod:<id>` / `passive:<id>` |
| 배지 / 트레이 / 밴드 | `badge:<tier>` / `tray:<u>` / `band:<for>:<tier>` |
| 태그 | `tag:<for>:<u 또는 n>:<j>` |
| 선·화살촉 | 목록 안의 순서(구조가 같으면 개수와 순서가 변하지 않는다) |

- 새 키는 뷰를 만들고, 사라진 키는 숨긴다(풀에 반납). 같은 키는 **바뀐 필드만** 적용한다(상태, 막대, 글자, 선택). 선 정점 색은 바뀐 선분만 갱신한다.
- 이 규칙으로 투자 한 번에 일어나는 갱신은 카드 1~2개, 칸 0~1개, 태그 몇 개, 선 몇 개, 머리 수치, 패널이다. 이 갱신은 클릭 거래 프레임에서 한 번만 일어나고, 이어지는 프레임은 할당도 UI 재구성도 하지 않는다(4.5, 계획 9.7).
- (영웅, 배치 종류, 언어)가 바뀌면 뷰 세트를 새로 만든다(탭 전환 시 6pt 아래에서 올라오는 연출 0.26초, 11장).

### 4.5 할당 기준: 0 할당은 어디까지인가
"할당 0"은 **클릭 거래 프레임 전체에 대한 기준이 아니다.** 투자·회수·보조 선택 한 번은 `host.Commit`(저장은 기존대로 `JsonUtility` 직렬화·사본), `SkillTreeD4Layout.Build`(새 목록), 뷰모델 재계산을 한 프레임에서 한 번 거친다. 이 일은 할당해도 된다. 기준은 세 층이다.

| 층 | 대상 | 기준 | 확인 |
|---|---|---|---|
| **0 할당** | ① 연출 갱신 `FxPlayer.Tick`(연출이 진행 중이든 아니든) ② 상태가 바뀌지 않은 프레임의 창 갱신(키 비교가 "변경 없음"으로 끝나면 배치·뷰모델·UI를 다시 만들지 않는다) ③ 새 `Graphic`의 `OnPopulateMesh`(5.8) | GC 할당 0바이트 | 위 세 곳의 호출을 델리게이트로 만들어 `Is.Not.AllocatingGCMemory()`를 건다(`ForgePresentationAllocationTests` 선례) |
| **한 번만 허용** | 클릭 거래 프레임: `host.Commit` → 규칙 검사 → 배치 → 뷰모델 → 키 비교 갱신 | 할당해도 된다. 단 거래 하나에 한 번씩만 부르고(같은 프레임에 `Commit`·`Build`를 두 번 하지 않는다) 이어지는 프레임은 0 할당 층이다. 계층은 다시 만들지 않는다: 새 키는 풀에서 빌려 쓰고, 풀이 비었을 때만 새로 만든다 | 클릭 재생 스모크가 클릭 프레임의 `GC.Alloc` 바이트·횟수와 그다음 프레임의 0을 보고에 적는다(예: `ProfilerRecorder`의 `GC Allocated In Frame`). 클릭 프레임에는 합격 한도를 두지 않는다. 첫 실측 뒤 사용자와 정한다 |
| **성능 주장** | "빨라졌다", "줄었다" 같은 말 | 실제 이전·이후 바이너리 각각 3회, CPU·GC 원자료 | 프로젝트 규칙 |

- 연타해서 거래가 N번이면 클릭 프레임이 N번일 뿐이다. 거래 사이의 프레임은 0 할당 층이다.
- 합격은 "클릭 프레임 0"이 아니라 "0 할당 층 세 곳이 0이고, 클릭 프레임이 계층을 다시 만들지 않으며, 그 프레임의 할당량이 보고에 적혀 있다"이다.

## 5. 그리기 도구 확장

지금 있는 그리기 도구(`UiMesh`, `UiButtonFace`, `SkillTreeGraphic`, `HuntEdictSurface`, `StorageSurface`, `RuneGemMesh` 등)로 되는 것과 **안 되는 것**은 `research/ui_infrastructure.md` 1장에 있다. 시안이 쓰는 CSS 기능을 세어 보면 다음과 같다(`node golden/tools/css_features.js`, 모양 428종과 공통 규칙 56개).

| 기능 | 쓰는 모양 수 | 지금 | 필요한 것 |
|---|---|---|---|
| 세로·비스듬한 그라데이션 채움 | 142 | 있음(`UiMesh.Gradient/RoundedBox`) | |
| **그라데이션 테두리** | 143 | **없음**(테두리는 모두 단색) | 5.1 |
| 방사 그라데이션 | 93 | 일부 | 5.5 |
| **각도(conic) 그라데이션** — 랭크 호, 동전 테두리 | 46 | **없음** | 5.2 |
| 바깥 번짐 / 안쪽 그림자 | 160 / 100 | 일부(1px 페더) | 5.5 |
| 점선 테두리·선 | 32 | 직선만 | 5.3, 5.4 |
| 육각형 | 39 | 쓸 수 있는 공개 함수 없음 | 5.2 |
| 글자 그림자 | 86 | 있음(`Shadow`, 어긋남 고정) | |
| 밝기·채도 필터(잠긴 아이콘) | 23 | 없음 | 5.6 |
| 블러 | 9 | 없음 | 5.6 |
| 그라데이션 글자 | 2(제목·이름) | 없음 | 5.7 |

새 함수는 기존 소유자에 이어서 쓴다(`UiMesh`는 `UiButtonFace.cs`). 색 정지 배열은 `UiTheme`에 둔다(색 리터럴 검사에서 제외된다).

### 5.1 그라데이션 테두리
`UiMesh.RoundedRing(m, rect, radius, width, Func<float, Color> byY)` — `Gradient`와 같은 모양의 오버로드. 시안의 `GILT_RIM`(5색)·`STEEL_RIM`이 쓴다. 안쪽 채움은 기존 `RoundedBox`.

### 5.2 육각형, 호, 각도 그라데이션
- `UiMesh.Hex(m, rect, Func<float,Color> fill)`, `HexRing(...)`: 꼭짓점 `(50% 0, 93% 25%, 93% 75%, 50% 100%, 7% 75%, 7% 25%)`. 배지 메달(27×30), 핵심 패시브(38×38 + 안쪽 24).
- `UiMesh.Arc(m, center, rIn, rOut, startDeg, sweepDeg, Func<float,Color> byAngle, segments)`: 랭크 호(4.2)와 메달의 **각도 그라데이션 테두리**(`conic-gradient(from 210deg, …)`, 색 정지는 `styles.json`). 부드럽게 보이려면 호 1칸당 12분할 이상.
- 랭크 호는 `RankArcGraphic`(칸 수, 켜진 칸, 두 색)으로 묶는다. 투자·회수 연출에서 켜진 칸이 늘어나는 순간 1.18배·밝기 2.2배가 되었다가 가라앉는다(`gxArc`, 0.55초).

### 5.3 선: `PolylineGraphic`
- 장면의 선분 목록(가로·세로 선분, 폭 2pt, **둥근 끝**)을 **한 메시**로 낸다. 선마다 `바탕 빛(glow)` → `본 선` 순서(시안은 SVG `<path>` 3개: 빛, 본 선, 연출).
- 상태별 모양(폭 2pt): `g` 금 `#e9c886` + 빛 7pt `rgba(242,180,96,.30)`, `o` 갈색 `#8a7556` + **검은 홈** 5pt `rgba(0,0,0,.8)`, `k` 점선 `#4a4238`(대시 4·4, 빛 없음), `s1` 은색 켜짐 `#b9c6d0` + 빛 7pt `rgba(150,200,235,.28)`, `s2` 은색 `#6f8291` + 검은 홈 5pt, `s3` 어두운 은색 점선 `#3e4852`(대시 4·4, 빛 없음). 화살촉은 같은 상태의 색(`s2` `#6f8291`, `s3` `#3e4852`)으로 채운다. 값은 `styles.json`의 `line.*`, `arrowhead.*`.
- 연출(점화·반짝임)은 같은 선분에 **부분 구간** `SetRange(t0, t1)`로 흰 선을 덧그린다(11장). 선분 길이를 0~1로 정규화한 값이다. 할당 없는 버퍼를 쓴다.
- 화살촉은 8×8 삼각형(`0,0 8,4 0,8` 등)이며 상태 색.
- 선분이 1pt 미만이면 건너뛴다(장면이 이미 건너뛴다).

### 5.4 둥근 점선 테두리
`UiMesh.DashedRing(m, rect, radius, width, ink, dash, gap)` — `RoundedContour`의 둘레를 따라 걷는다. 잠긴 카드·칸·트레이·티어 표지가 쓴다(`border: 1px dashed`, 대시 약 3·3).

### 5.5 번짐과 그림자
- `UiMesh.Glow(m, rect, radius, spread, color)`: 바깥으로 알파가 줄어드는 링. 3~6단. 아래로 번지는 그림자(`0 3px 6px`)는 중심을 아래로 옮긴다.
- `UiMesh.Inner(m, rect, radius, spread, color)`: 안쪽 번짐. 위쪽 1px 빛(`inset 0 1px 0`)은 선 1개로 충분하다.
- 방사 채움은 타원 팬(`SelectionGroundShadow`의 방식).
- 바깥 번짐은 `RectMask2D`에 잘린다. 판 안쪽 요소의 번짐은 판 안에서 잘려도 시안과 같다(시안도 `overflow:hidden`). 판 밖으로 나오는 장식만 마스크 밖 계층(4.1).

### 5.6 블러와 필터
- 상세 패널 배경 그림: 220×220 아이콘, `opacity .4`, `blur(18px) saturate(1.15) brightness(.6)`, 방사 마스크(가운데 30% 불투명 → 62% 반투명 → 투명). **UI 셰이더 한 개**로 한다: `Resources/GlobalHudFade.shader`를 바탕으로 하고(스텐실·`_ClipRect` 지원), `PuzzleHubTone.shader`의 `_Tone`(밝기·채도) 식을 합친다. `PuzzleHubTone`은 스텐실·클립이 없어서 스크롤 판 안에서 쓰면 안 된다. 창이 `Material`을 만들고 닫을 때 파괴한다(`PuzzleHubWindow`의 선례).
- 잠긴 아이콘의 `grayscale/brightness/opacity`: 같은 셰이더의 `_Tone`, 불투명도는 `CanvasRenderer.SetAlpha`(메시 재구성 없음). 값은 4.2의 `modifier.icon.*`, `card.icon.*`.
- 아이콘은 1254² 읽을 수 없는 텍스처, 밉맵 없음이다. CPU·굽기 블러는 불가능하고, 계획 16장이 아이콘을 다시 만지는 것을 금지한다. 블러가 끝내 안 되면 허용 근사 A2(흐림 없는 그림)로 간다.

### 5.7 그라데이션 글자
`TextGradient : BaseMeshEffect` — 글리프 사각형의 위쪽 정점과 아래쪽 정점에 다른 색을 준다(`DialogueReveal.Conceal`이 글리프별 정점 색을 다시 쓰는 선례). `Shadow`보다 먼저 붙여서 그림자는 검게 남긴다. 제목은 `#fff3d6 → #9b743c`, 은색 이름은 `#ffffff → #9fb0bd`(허용 근사 A3).

### 5.8 품질 규칙
- 새 `Graphic`은 `[RequireComponent(typeof(CanvasRenderer))]`를 붙이고, 설정 없이 `AddComponent`해도 예외가 없어야 한다(`GraphicComponentTests`가 모든 `Graphic` 하위 클래스를 검사한다).
- `OnPopulateMesh`는 호출마다 할당하지 않는다(4.5의 0 할당 층). 델리게이트로 불러 `Is.Not.AllocatingGCMemory()`를 확인하는 시험을 둔다(`ForgePresentationAllocationTests`가 선례). `UiMesh`의 기존 도우미(`RoundedContour`, `Gradient`, `Brackets`)는 호출마다 배열을 만든다. 새 함수는 버퍼를 재사용한다.
- 결정적인 도형(육각형, 호, 링)은 `OnPopulateMesh` 결과를 JSON으로 저장한 **메시 골든**과 비교한다(`forge-ring-baseline` 선례).
- 1px 헤어라인은 `1 / canvas.scaleFactor`에 맞춘다(`UiButtonFace`의 선례).
- 그림은 코드로 그린다. 틀·마스크·선택 효과를 이미지로 만들지 않는다(`Docs/Implementation/Generated_Game_Art.md`). 아이콘 92장만 이미지다.

## 6. 모양 카탈로그와 토큰

`golden/styles.json`이 **모양의 기준**이다. 428종이며, 각각 CSS 선언 목록과 "어디서 쓰였는지"가 있다.

- **모양은 이름 하나로 정해진다.** 장면 요소의 필드(상태, 선택, hot, avail …)가 이름을 만들고, 이름이 모양을 정한다. 생성기는 같은 이름에 다른 모양이 나오면 실패한다. 그러므로 구현은 "필드 → 이름 → 모양" 표 하나(`SkillTreeD4Look`)로 끝난다.
- 이름 규칙(`og` = `land` | `port`):

| 요소 | 이름 | 상태 축 |
|---|---|---|
| 카드 | `card.box.<og>.<l\|o\|k>[.selected][.mod-focused][.hot]`, `card.icon.<og>.<state>`, `card.text.<og>`, `card.pill.port.<state>[.maxed]` | 투자함/가능/잠김, 선택, 보조 포커스, 다음 랭크 가능(`hot`) |
| 랭크 칩(가로) | `tag.card-rank.<state>[.maxed]` | |
| 보조 칸 | `modifier.hit.<og>`, `modifier.face.<og>.<chosen\|available\|idle\|unlearned\|tier-locked>[.focus]`, `modifier.icon.<og>.<…>`, `tag.mod-level.any` | |
| 패시브 | `passive.node.<og>.<coin\|key>.<l\|o\|k>[.avail][.sel]`, `passive.medal.<og>.<coin\|key>.<state>[.lit][.avail][.sel]`, `passive.ring.<coin\|key>.<state>`, `passive.icon.<coin\|key>.<state>`, `passive.text.name.<state>`, `passive.text.sub.<state>[.key]`, `tag.passive-unlock.<state>[.key]`, `tag.passive-rank.<state>[.key]` | |
| 배지 | `badge.box.<og>.<open\|next-locked\|locked>`, `badge.medal.<open\|closed>.<og>`, `badge.medal.inner.<open\|closed>.<og>.<large-numeral\|small-numeral>`, `badge.name.<og>.<state>`, `badge.gate.<og>.<state>`, `badge.wrap.<og>` | |
| 선 | `line.<g\|o\|k\|s1\|s2\|s3>` + `.glow`, `arrowhead.<s1\|s2\|s3>` | |
| 밴드·트레이 | `band.passive-shelf.<plain\|key>`, `band.passive-heading.<plain\|key>`, `tray.<learned\|unlearned>` | 밴드 `key` = `tier`가 4인 줄(핵심 패시브 줄). 트레이 `learned` = 그 액티브가 Lv1 이상(장면의 트레이에는 이 값이 없으니 배분에서 읽는다) |
| 머리 | `header.title.<layout>`, `header.stats.<layout>`, `header.stat.<level\|spent\|left.zero\|left.hot>`, `header.close`, `tab.<idle\|selected>.<og>` | |
| 판 | `plate.<layout>`, `plate.scroll`, `plate.legend` | |
| 상세 패널 | `panel.<layout>.<active\|passive\|key>`, `panel.medal.<kind>.<layout>.<state>[.lit][.max3]`, `panel.medal.ring.<kind>.<state>`, `panel.medal.icon.<kind>.<layout>.<state>`, `panel.art.wrap`, `panel.art.<kind>.<layout>`, `panel.name/sub/level/stats/heading/pick/desc.*` | |
| 패널 단추·이유 | `panel.btn.<sq\|bar>.refund.<on\|off>`, `panel.btn.<sq\|bar>.invest.<gold\|off>[.reason]`, `panel.why.<red\|grey>` | `sq` = 세로 배치의 머리 오른쪽 정사각 단추(44pt), `bar` = 가로·PC의 아래 막대 단추. 투자는 가능(`gold`)·불가(`off`). `.reason` = 막대 투자 단추가 막혀서 막힌 이유가 그 글자가 될 때. `panel.why` = `sq` 아래 한 줄(`red` = 티어·레벨 때문, `grey` = 그 밖) |
| 패널 보조 행 | `panel.mod.row.<land\|port\|pc>.<chosen\|idle>[.focus]`, `panel.mod.socket.<land\|port\|pc>.<l\|o>`, `panel.mod.name.<og>.<chosen\|idle>`, `panel.mod.eff.<og>.<chosen\|idle>`, `panel.mod.effTo.<chosen\|idle>`, `panel.mod.right.<og>.<chosen\|idle>` | 고른 행·안 고른 행, `focus` = 지금 선택된 보조 스킬 행(밝은 2pt 링) |
| 패널 패시브 블록 | `panel.unlock.<ok\|lock>`, `panel.unlock.bar`, `panel.effect.<og>`, `panel.effect.to`, `panel.linked.<og>`, `panel.link.<og>`, `panel.link.lv.<on\|off>`, `panel.link.icon` | `lock` = 영웅 레벨이 모자람(붉은 글자) |
| 막대 | `bar.rank`, `bar.ghost`, `bar.tier` | 너비는 요소의 `Bars`/`Bar` |

- **패널 안쪽의 모양**은 `scene.panel.parts`가 이름으로 가리킨다(`buttons`, `reason`, `modifiers[]`, `passive`). 글자와 켜짐·꺼짐은 화면 정보(`panel.buttons`, `panel.modifiers`, `panel.unlock` …)에서 온다. 템플릿에 박힌 고정 스타일(스탯 줄의 글자색, 구분선, 머리글 등)은 시안 `SkillWindow.dc.html`의 135~177행과 `anatomy/TEXT_STYLES.md`가 기준이다.
- **이름 시험.** `SkillTreeD4LookTests`는 정답 `layout/*.json`의 모든 요소 `style` 이름(그리고 `panel.parts`·`panel.styles`·`panel.medal`의 이름)이 `SkillTreeD4Look`이 필드에서 만든 이름과 같고, 그 이름이 `styles.json`에 있음을 확인한다. 이름을 만드는 식은 생성기 `golden/lib/scene.js`에 읽기 쉽게 있다. 상세 패널 안쪽의 이름은 선택한 노드마다 `golden/panel_parts.json`에 있다(3직업 × 노드 48 × 배치 3, 투자 가능 상태를 보려고 레벨 60 두 상태를 더했다). 생성기 점검에서 모든 요소의 이름은 요소의 상태 필드와 배치 종류만으로 정해졌고, 예외는 위 표의 밴드·트레이 둘뿐이다(둘 다 데이터에서 온다).
- **한 가지 모양을 읽는 법.** 예: `card.box.land.l`.
  - `background: <바 층>, linear-gradient(180deg,#27211a,#17130f 55%,#120f0c) padding-box, linear-gradient(180deg,#e2c07e,#8a6b3c 40%,#4e3e26 70%,#a5844e) border-box` + `border: 1px solid transparent` = **그라데이션 테두리**(`border-box` 층이 투명한 테두리 사이로 보인다). 채움과 테두리를 따로 그린다.
  - `box-shadow: 0 0 0 1px #000, 0 3px 6px rgba(0,0,0,.6), inset 0 1px 0 rgba(255,230,180,.14)` = 바깥 검은 1px, 아래 번짐, 위 안쪽 빛.
- CSS → 그리기 대응:

| CSS | 그리는 방법 |
|---|---|
| `linear-gradient(180deg, A, B 55%, C)` | `UiMesh.Gradient`/`RoundedBox` 색 정지 |
| `… padding-box, … border-box` + 투명 테두리 | 채움 + `RoundedRing(byY)` (5.1) |
| `radial-gradient(circle at 50% 60%, c1, c2 70%)` | 타원 팬 (5.5) |
| `box-shadow: 0 0 0 1px #000` | 한 겹 큰 링 |
| `box-shadow: 0 3px 6px …` / `inset …` | `Glow` / `Inner` (5.5) |
| `border: 1px dashed` | `DashedRing` (5.4) |
| `conic-gradient(…)` | `Arc` (5.2) |
| `clip-path: polygon(…)` | `Hex` (5.2) |
| `filter: grayscale/brightness`, `opacity` | `_Tone` 셰이더, `SetAlpha` (5.6) |
| `text-shadow: 0 1px 0 #000` | `Shadow` 한 개, 어긋남 `(0, −2)`(캔버스 단위, 절반 점) |
| `background-clip: text` | `TextGradient` (5.7) |
| `word-break: keep-all` | 7.4 |

- **색 토큰.** 색 리터럴은 239종이다(`node golden/tools/css_features.js --colors`). 화면 코드에 hex를 쓰면 `check_ui_contract`의 색 리터럴 검사(따뜻한·어두운·상아색 hex를 걸러낸다)에 걸린다. **모두 `UiTheme`에 토큰으로 둔다.** 기존 Obsidian & Gilt 토큰(금·흑요석)이 이미 덮는 색은 그것을 쓰고, 새로 필요한 계열만 추가한다: 패시브 은색(`#b9c6d0 #6f8291 #3e4852 #4a5862 #24303a #1a2126`, 글자 `#c9d5de #9fb0bd #66727c`, 테두리 `STEEL_RIM`), 선 색(`g o k s1 s2 s3`), 불씨(`#e07a32` 계열), 막힌 이유 붉은색 `#df927b`. 토큰 이름은 계열별(`D4Gold…`, `D4Steel…`, `D4Ember…`)로 짓되 hex 값은 시안과 같아야 한다.
- **RGBA 가로 줄이기:** 알파가 있는 색(`rgba(0,0,0,.65)` 등)은 hex+알파 토큰으로 둔다. `Color`의 알파로 곱하지 말고 토큰에 포함한다.

## 7. 글자

### 7.1 글꼴 (사용자 결정 D6)
시안은 Noto Serif KR(제목·이름·머리글), Noto Sans KR(본문), Cinzel(로마 숫자)을 쓴다. 게임은 OS 글꼴(`UiFonts.Body`)과 `UiFonts.Display`(Baskerville·Georgia·Times New Roman·Noto Serif, WebGL은 본문 글꼴)다. 새 글꼴을 번들하면 용량이 늘고 다른 창과 모양이 달라진다.
- **기본(권장):** 기존 `UiFonts`를 쓴다. 허용 근사 A1이다. 글자 폭 차이로 줄바꿈이 달라질 수 있으므로 7.3의 실측 확인을 반드시 한다.
- **대안:** Noto Serif KR·Cinzel을 번들한다(부분 집합). 용량과 다른 창의 일관성을 사용자가 정해야 한다(계획 17장 D6).
- 글꼴을 새로 만드는 코드는 `UiTheme.cs` 밖에서 금지(`check_ui_contract.py`).

### 7.2 글자 스타일
- 시안에서 실측한 글자 스타일 129종은 `golden/anatomy/TEXT_STYLES.md`에 있다(배치, 글꼴, 크기, 굵기, 줄 높이, 색, 예시 문구). 핵심 규칙:
  1. **크기는 디자인 점**이고 캔버스에서는 2배(3.1). 0.5pt 단위 크기(10.5, 12.5, 14.5 …)는 정수가 된다.
  2. 줄 높이가 `13px`처럼 정해진 곳은 줄 간격을 그 값에 맞춘다(`Text.lineSpacing`을 조정해 두 줄의 간격을 잰다). `normal`은 글꼴 기본.
  3. 색은 그 행의 값. `rgba(0, 0, 0, 0)`은 **그라데이션 채움 글자**(제목과 패널 이름)라서 색이 투명으로 찍힌다(5.7). 소제목 "보조 스킬"(`#e2c07e`)은 단색이다.
  4. 자간(`letter-spacing: .03em`)은 패시브 머리글에만 있다. 무시한다(허용 근사).
- 역할별 크기(점): 제목 27(세로)/24(가로)/26(PC), 탭 14/13/13, 머리 수치 12(PC 12.5)와 값 12~13 굵게, 카드 이름 12.5→10.5(세로)/11→9.5(가로, 맞춤), 랭크 칩 10, 해금 레벨 9, 패널 이름 19(세로)/16(가로·PC), 패널 부제 11.5/10.5, 패널 레벨 12.5/11, 설명 12/10.5, 능력치 행 12.5/11, 보조 스킬 이름 13/11.5, 효과 11/10, 버튼 13, 범례 10.5. 정확한 값은 TEXT_STYLES.md.

### 7.3 카드 이름 맞춤과 실측 확인
- 장면의 카드 `FontSize`(`FitFontSize`)는 시안의 글자 폭 **추정**으로 정해졌다. 이식본이 그대로 낸다(L1 검증).
- 게임 글꼴의 실제 폭은 다르다. 그려 놓은 뒤 `Text.preferredWidth`로 **가장 긴 어절**을 재서 상자 폭을 넘으면 0.5pt씩 줄인다(최저 `min − 1`). 그래도 넘으면 어절 안에서 줄바꿈한다. **글자를 자르지 않는다.** 이 확인은 한국어·영어, 모든 카드 이름(직업별 10개)에서 EditMode로 돈다(모든 카드가 상자 안에 있는지).
- 카드 이름은 어절 단위로 줄바꿈하고 최대 2줄, 세로 가운데 맞춤이다.

### 7.4 줄바꿈 (`keep-all`)
- 카드 이름(`card.text.*`)과 설명 자리(`panel.desc.*`)는 `word-break: keep-all`이다: **한국어는 공백에서만 줄바꿈**한다. 프로젝트에는 keep-all 선례가 없고(`research/ui_infrastructure.md` 4장), Unity 레거시 `Text`가 한글을 공백에서만 끊는지는 이 문서가 확인하지 못했다.
- 구현 단계에서 **가장 긴 설명 문장으로 스모크가 확인한다.** 어절 중간에서 끊기면 `SkillTreeD4Wrap`(글자 폭 추정 `TextWidth`로 어절 단위로 직접 줄바꿈하고 `\n`을 넣음)으로 바꾼다.
- 설명 자리의 **예약 높이**: 세로 32pt(2줄×16), 가로 28pt(2줄×14), PC 42pt(3줄×14). 선택이 바뀌어도 패널이 움직이지 않는다. 가장 긴 설명은 한국어 43자·영어 70자, 가장 긴 형태 문장은 한국어 43자·영어 64자(보조 스킬 이름과 " — " 포함)이며 이 안에 들어간다. 스모크가 모든 액티브와 보조 스킬(3직업 × 2언어 × 3배치)을 차례로 골라 **넘침 없음**을 확인한다.
- 설명 자리에서 보조 스킬 이름은 **굵게 금색**(`#f2d9a4`)이고 같은 문단 안에서 이어진다: `<b><color=#f2d9a4>이름 — </color></b>문장`. 부분 굵게는 레거시 `Text`의 `<b>`로 되는지 확인한다(프로젝트에는 `<b>` 선례가 없다). 안 되면 두 `Text`를 한 줄 흐름으로 배치한다.

## 8. 상세 패널

### 8.1 구조
상세 패널은 장면 밖이다(장면은 사각형만 준다). **화면 정보**(`SkillTreeD4View.Panel`)가 문구와 켜짐을 주고, **`anatomy`가 위치와 크기**를 준다.

| 선택 | `anatomy.panel` 상태 | 내용 |
|---|---|---|
| 액티브 | `skillSelected`, `lockedUltimate` | 메달(랭크 호), 이름, 부제 "T2 핵심 · 액티브", 레벨 "Lv 5 / 5 · 수치 Lv1 → Lv5", **설명 자리**, 능력치 행 n개, 소제목 "보조 스킬"+힌트, 보조 스킬 행 3(궁극기 2), 버튼 |
| 보조 스킬 | `modifierSelected`, `skillWithInvestedModifierRows` | 위와 같고 설명 자리가 "보조 스킬 이름 — 형태 문장", 고른 보조 스킬 행 강조 |
| 패시브 | `passiveWithoutLinks`, `passiveWithLinks` | 메달(은색 호), 이름(은색), 부제, 레벨, 해금 줄(+진행 막대), 효과 상자, **연계 스킬 칩**(연계가 없으면 칩 영역도 안내 문구도 없다), 버튼 |
| 핵심 패시브 | (같은 구조) | 금색 육각 메달, 금색 이름, 부제 "핵심 패시브 · 1종만" |

### 8.2 배치 방법
- 세로 흐름(flex column)이다. 안쪽 여백과 간격은 `SkillTreeD4Frame`(`PanelPad`, `PanelGap`)과 `panel.*` 모양의 `padding`/`gap`이다: 세로 16/5, 가로 12/4, PC 14/10.
- 자식 높이: 머리 블록(메달 크기 + 이름 3줄), 설명 자리(예약 높이), 능력치 구역(구분선 1px + 위 여백 4 + 행 × 줄 높이 + 행 간격 3), 소제목 줄, 보조 스킬 행(세로는 남는 높이를 나눠 갖고 최소 40, 가로는 고정 최소 36/40), 버튼 줄 44.
- 위치·크기의 **정답은 `anatomy/OUTLINE.md`의 해당 상태**다. 직접 계산한 사각형이 정답과 ±1pt 안에 들어와야 한다(L4). 상태별로 GameObject 이름을 2.4의 규칙으로 붙여서 시험이 이름으로 찾는다.
- 모서리 장식(`gx-pan-p`, `gx-pan-l`)은 패널 위쪽·왼쪽 가장자리에서 6pt 바깥으로 나온다(마스크 밖). 배경 그림은 패널 모서리 반지름으로 잘린다(`overflow:hidden; border-radius:inherit`).

### 8.3 문구와 규칙
- 능력치 행: 왼쪽 라벨, 오른쪽 "from → **to**". 화살표 `#8a6b3c`, from은 어둡게, to는 밝게 굵게(`SplitEffect`가 나눈다). 보조 스킬 행의 효과 문장도 같다.
- 보조 스킬 행 오른쪽: 안 고름 `0/5`, 고름 `3/5`(가로) 또는 `선택됨 3/5`(세로, 알약). 고른 행은 금 테두리·불씨 바탕.
- 패시브 해금 줄: 해금했으면 `Lv15 ✓`(은색), 아니면 `Lv25 · 현재 Lv24`(붉은색 `#df927b`) + 붉은 진행 막대(`UnlockBar`%, 4pt 높이, 둥근 끝). 막대는 길 `#1c1512` 위에 `#8a3e2a → #df927b` 채움과 붉은 번짐이다. 레벨이 부족하면 막대가 있고 충분하면 없다.
- **버튼:** `Buttons.Layout == "header"`(세로)이면 패널 머리 오른쪽에 44pt 정사각 − / + 버튼 두 개와 아래 막힌 이유(티어·레벨 때문이면 붉은 글자 `#df927b`, 그 밖이면 회색 `#8f877b`), `"bottom"`(가로·PC)이면 패널 아래 "회수"(철) / "투자"(금) 버튼 두 개. 막혔을 때 버튼 글자는 `InvestLabel`(이유)로 바뀐다. 포인트가 없어서 막힌 경우는 이유 문구가 없다(헤더 수치가 이미 0). 켜진 투자 버튼 위로 7.8초 주기로 빛이 지나간다(11장).
- 막힌 이유 문구·순서·`aria`는 화면 정보가 준다. 창은 그 문자열만 쓴다.

## 9. 머리, 탭, 범례, 닫기

| 요소 | 규격 (디자인 점) |
|---|---|
| 제목 | 세로 (16,62) 27pt / 가로 (62,10) 24pt / PC (24,12) 26pt, 높이 44, 세리프 굵게, 금 그라데이션 |
| 탭 | 높이 44, 둥근 6. 세로: (16,112) 폭 408을 3등분 간격 8, 14pt. 가로·PC: 76폭 간격 6, 13pt. **고른 탭**: 불씨 방사 + 금 테두리 + 굵은 `#fbe9c0` 글자 + 아래 금 마름모 9×9(`bottom:-5`). **투자 가능 포인트가 있는 직업**: 오른쪽 위 6,6에 8×8 불씨 점 |
| 머리 수치 | 가로 (400,10, 442×44) 간격 16, PC (560,12, 510×44), 세로 (100,62, 270×44) 간격 10. "레벨 24", "사용 24 / 60", "남은 포인트 ⓪". 남은 포인트 값은 알약(높이 26, 최소폭 26, 반지름 13): 0이면 어두운 구슬(`header.stat.left.zero`), 0보다 크면 불씨 구슬(`left.hot`)이고 6.4초 주기로 테두리 빛이 맥동 |
| 닫기 | 44 원형, 금 테두리 `#8a6b3c`, 안쪽 X 16×16(선 2pt, 둥근 끝) |
| 범례 | 판 아래 24pt 띠(투명→`#090807` 62%), 안쪽 여백 10, 항목 간격 12, 글자 10.5pt. 항목: 금 선 22×8(`#e9c886`+빛) "투자함", 갈색 선 "투자 가능", 점선 "잠김", 은 화살표 "패시브 연계" |
| 판 | 가로 (62,62) 548×357 r8 / 세로 (8,164) 424×406 r10 / PC (24,64) 856×564 r8. 흑요석 바탕, 금 그라데이션 테두리, 안쪽 어둠 |

- 탭 전환: 직업을 바꾸면 **선택·배분은 직업별로 남는다**. 스크롤 위치는 유지(범위 안으로 보정). 트리가 6pt 아래에서 올라오며 나타난다(0.26초).
- 닫기·뒤로 가기: `ContentWindowHost`의 뒤로 가기에 맡긴다.
- 수치는 `SkillTreeD4Scene.Header`·`Plate`·`SkillTreeD4Frame`이 준다. 위 표의 숫자를 코드에 다시 적지 않는다(이식본 값을 쓴다).

## 10. 입력과 스크롤

- **선택**: 카드, 보조 스킬 칸, 패시브, 패널의 보조 스킬 행을 **눌렀다 뗄 때**(클릭) `Session.Select`가 불린다. 누르는 동안의 눌림 효과: 카드·패시브 `scale(.97)`, 보조 칸 `scale(.92)`, 탭·닫기 `translateY(1px)`(점 단위). 눌림 효과는 감소 모션 설정과 상관없이 유지한다(시안의 `prefers-reduced-motion`은 `gx-*` 연출만 끈다).
- **투자·회수 버튼**: `Session.Invest()` / `Refund()`. 받아들여지면 즉시 `host.Commit`(2.3)한다. `Buttons.InvestEnabled`/`RefundEnabled`가 false인 버튼의 클릭은 무시한다(소리·떨림 없음). 막힌 이유는 항상 보인다(8.3).
- **직업 탭**: `Session.Tab(i)`. 같은 영웅의 탭을 누르면 아무 일도 없다.
- **닫기·뒤로**: 호스트의 뒤로 가기(`ContentWindowHost`). Esc도 같다.
- **스크롤**: 가로 배치는 가로, 세로 배치는 세로 한 축만이다. PC는 스크롤이 없다. 카드·칸·패시브 위에서 끌면 스크롤이 되고 **누름은 취소**된다(`UiButton`은 드래그 논리가 없고 `ScrollRect`가 끌기를 가져가면 누름이 취소된다). 끌기 임계값은 기본값이다.
- 길게 누르기, 더블 탭, 오른쪽 클릭, 마우스 올림(hover) 효과는 시안에 **없다**. 키보드·패드 이동도 범위 밖이다(비목표).
- 첫 선택: 시안은 `designSelection`(전사 회오리, 궁수 관통 사격, 마법사 화염구)이다. 실제 게임은 마지막으로 고른 대상, 없으면 첫 투자 가능 액티브다(계획 9.5). "패시브 보기"로 열면 그 직업의 연계가 있는 첫 패시브가 선택되고 세로 스크롤이 `initialScrollTop`으로 간다.
- 투자 버튼을 빠르게 연타하면 눌린 만큼 커밋한다. 연출은 마지막 것이 앞의 것을 대체한다(같은 노드의 연출은 다시 시작).
- 판정 영역은 장면의 사각형(카드 `Rect`, 칸 `Hit`, 패시브 `Rect`) 그대로다. 터치 영역이 44px 미만이 되는지는 `SkillTreeD4UnitsTests`의 화면 크기 표로 확인한다(`MonsterCodexWindowTests`의 "탭 대상 ≥ 44px" 선례).

## 11. 연출

**언제 무엇이 움직이는가**는 이식본이 정한다(`SkillTreeD4.Fx.cs`, 정답 파일의 `events`로 검증됨). 창은 그 결과를 재생만 한다.

- `SkillTreeD4Session.Fx` — 투자·회수가 받아들여진 직후의 효과: `Action`(투자/회수), `Id`(대상 노드 `a:<액티브>:<보조>` 또는 `p:<패시브>:-1`), `Max`(최대 랭크), `Gate`(이 투자로 새로 열린 티어, 없으면 −1), `IgnU/IgnM/IgnP`(점화할 대상: 액티브 첫 랭크 `IgnM = −2`, 보조 스킬 첫 포인트 `IgnM = 보조 번호`, 패시브 첫 포인트 `IgnP`).
- `SkillTreeD4Session.Overlays()` — 연결선 위 연출: `Ignite`(선 인덱스 목록, 투자가 방금 켠 선), `Glint`(선 인덱스, 지연 ms, 거꾸로 여부). 같은 선에서는 점화가 반짝임보다 앞선다. **반짝임은 선택이 시작하고**, 투자·회수는 다시 시작하지 않는다. 탭과 선택은 `Fx`를 지운다.

**어떻게 보이는가**(`fx.json`의 키프레임, 시간은 초):

| 계기 | 대상 | 효과 | 시간 · 곡선 |
|---|---|---|---|
| 투자 성공 | 대상 노드(카드·칸 얼굴·패시브) | 튄다: 크기 0.94 → 1.06(55%) → 1 (`gxPunch`) | 0.52 · cubic-bezier(.2,1.6,.4,1) |
| 〃 | 노드 위 오버레이 | 빛 고리: 크기 0.3 → 1.55, 불투명 1 → .55(70%) → 0 (`gxBloom`) | 0.56 · cubic-bezier(.15,.8,.25,1) |
| 〃 | 〃 | 별꼴 빛살 12갈래: 크기 0.35 → 1.9, 18° 회전, 불투명 1 → 0 (`gxStar`) | 0.6 · 위와 같음 |
| 〃 | 〃 | "+1": 불투명 0 → 1(25%) → 0, 위로 4 → −4(25%) → −16pt, 크기 0.6 → 1.15 → 1 (`gxNumUp`) | 0.75 · cubic-bezier(.2,.8,.3,1) |
| 최대 랭크(`Max`) | 〃 | 별꼴이 더 크게(0.35 → 2.6, 30° 회전, `gxStarMax`), 글자는 "MAX"(11pt) | 0.8 |
| 투자·회수 | 노드의 랭크 칩·알약·레벨 칩 | 1 → 1.32(35%, 밝기 1.7) → 0.95(70%) → 1 (`gxPop`) | 0.38 · cubic-bezier(.3,1.6,.5,1) |
| 투자·회수 | 머리 "사용 n/60", "남은 포인트" | 1 → 1.12(35%, 밝기 1.6) → 1 (`gxTick`) | 0.38 · 위와 같음 |
| 투자·회수 | 상세 패널 메달(랭크 호) | 1.18배, 밝기 2.2 → 1 (`gxArc`) | 0.55 · cubic-bezier(.2,.9,.3,1.2) |
| 투자로 새 티어가 열림(`Gate ≥ 0`) | 그 티어의 표지 | 테두리 링이 0 → 14pt로 번지며 사라짐(불투명 .95 → 0)과 26pt 번짐이 사라짐(`gxGate`), 빛줄기(폭 40%)가 왼쪽(−120%)에서 오른쪽(320%)으로 지나감(`gxSweep`, 기울기 −20°) | 0.8 · ease-out |
| 투자가 선을 새로 켬 | `Ignite` 선 | 흰 선(금 `#fff3d6`, 은 `#f4f8fb`, 폭 4)이 선 길이를 따라 0 → 1로 그려지고(70%) 사라진다(100%) (`gxIgnite`) | 0.65 · cubic-bezier(.3,.7,.3,1) |
| 회수 성공 | 대상 노드 | 채도·밝기가 빠졌다 돌아오며(채도 .2, 밝기 .8 → 원래) 좌우로 −2 → +2 → 0pt 흔들림 (`gxDrain`) | 0.34 · ease-out |
| 〃 | 오버레이 | 재 고리가 안으로 빨려듦: 1.7 → 0.35, 불투명 .9 → 0 (`gxImplode`) | 0.4 · ease-in |
| 〃 | 〃 | "−1"(`#c8bfae`): 불투명 0 → 1(25%) → 0, 아래로 −6 → +10pt (`gxNumDown`) | 0.7 · ease-out |
| 사용자가 선택 | `Glint` 선 | 흰 `#fffaf0` 폭 4, 선 길이의 14% 길이 조각이 선을 따라 지나감(거꾸로면 반대로), 선마다 지연(아래), **3번** (`gxGlint`) | 1.3 · linear |
| 〃 | 선택된 노드 | 선택 테두리 빛이 .35 ↔ 1로 숨 쉼, **3번 왕복** 뒤 켜진 채 머문다 (`gxBreath`). 금(액티브·핵심) `rgba(242,180,96,.55)` 14pt, 은(동전) `rgba(170,210,240,.5)` | 2.4 · ease-in-out |
| 〃 | 상세 패널 머리 | 불투명 .35 → 1, 아래서 4 → 0pt (`gxRise`) | 0.2 · ease-out |
| 직업 탭 변경 | 트리 내용 | 불투명 0 → 1, 아래서 6 → 0pt (`gxTree`) | 0.26 · ease-out |
| 반복 | "남은 포인트" 구슬(포인트가 있을 때만) | 테두리 빛이 .3 ↔ 1로 **4번** 맥동(1.6초마다) (`gxOrb`) | 6.4 · 반복 |
| 반복 | 투자 버튼(투자 가능할 때만) | 흰 빛 막대(폭 38%, 기울기 −20°)가 왼쪽(−160%)에서 오른쪽(360%)으로 **한 주기에 3번**(2.6초마다, 한 번에 약 0.9초) 지나감 (`gxShine`) | 7.8 · 반복 |

- **반짝임 지연**(선 조각마다, ms). `T` = 선택한 액티브의 티어(패시브를 골랐으면 0): 티어 연결선 `tc` → (그 티어 번호 − 1) × 90, 티어 줄기 `s` → 티어 번호 × 90 + 50, 카드로 가는 선 `c` → T × 90 + 200, 줄기→칸 `f` → T × 90 + 300, 칸 가지 `m` → T × 90 + 360, 패시브 연결 `l`과 그 밖 → T × 90 + 300(패시브를 골랐으면 0). 계산은 `LineOverlays`가 이미 해 둔다(`SkillTreeD4Glint.DelayMs`).
- **노드 오버레이의 크기와 색**(시안): 카드 64(가로)·76(세로)pt, 아이콘 중심에 둔다. 보조 칸 52(가로)·58(세로). 패시브 노드 58, 세로 캡슐의 메달 60. 색은 금 `255,214,150`(액티브·핵심 패시브), 은 `214,232,245`(동전 패시브), 재 `160,151,138`(회수). 빛 고리와 별꼴의 모양은 `fx.json`의 `.gx-fx*` 규칙에 CSS 그대로 있다.
- **감소 모션**: `StoryDialogueWindow.ReduceMotion`(기기 설정 `hellscript.reduce-motion` 또는 OS 설정)이 켜지면 위 연출을 **모두 끈다**(최종 상태는 즉시 보인다). 눌림 효과는 유지한다. 반복 연출(구슬 맥동, 투자 버튼 빛)도 끈다. 이 키를 쓰는 설정 화면이 없다는 점(`research/ui_infrastructure.md` 위험 9)은 이 작업의 범위가 아니다.
- **구현 틀**: 창마다 갱신기 하나(`Update`, `Time.unscaledTime`)가 연출 구조체 풀을 돌린다. 트윈 라이브러리는 없다. 연출 하나는 상태 구조체(대상, 시작 시각, 종류)이고 **할당이 없어야 한다**(4.5). 같은 상태로 다시 그릴 때는 연출을 다시 시작하지 않는다(시안의 A/B 클래스 교대는 CSS가 애니메이션을 다시 시작하려는 장치이니 게임에서는 효과를 다시 재생하면 된다).
- 선 연출은 `PolylineGraphic.SetRange(t0, t1)`(5.3)로 한다: 점화는 0 → 1 → 사라짐, 반짝임은 조각이 −0.14 → 1을 지나간다(선분마다 길이 0~1로 정규화).

## 12. 문구와 접근성

- **모든 화면 글자는 데이터에서 온다**: 스킬·보조 스킬·패시브의 이름·효과·설명은 `SkillTreeD4.json`(한국어·영어 쌍), 화면 문구는 `strings.ko/en`을 `en.txt`의 새 구역(`# D4 skill tree`)에 넣는다(계획 10장). 코드에 한국어 리터럴을 쓰지 않는다(`LocalizationTests`가 `Runtime` 안 한국어 리터럴마다 `en.txt` 항목을 요구한다).
- 이식본이 쓰는 문구 키: 제목, 레벨, 사용, 남은 포인트, 직업 이름 3, 조건("투자 {n} 이상"), "기본 해금", "잠김", "선택됨", 투자·회수(긴 형태와 짧은 형태), 액티브·패시브·핵심 패시브, 패시브 머리글·핵심 머리글, "Lv{n} 필요"·"Lv{n} 해금"·"현재 Lv{level}", 범례 4, "수치 Lv1 → Lv{n}", 막힌 이유(`max tier base swap cap key`, 영어 `tier`는 단수·복수), 그리고 이식본에만 있는 **`KeyPrefix`**(세로 핵심 패시브 줄의 "핵심 · " / "Key · ")는 새로 추가한다.
- **조합 문구**는 `Loc.F`의 자리표시(`{0}`)로 바꿔 넣는다. 시안의 `{n}`·`{level}` 이름은 이식본의 `Fill`이 쓰는 것이다. 구멍으로 시작하는 템플릿("{n}pt 더 필요")은 저장된 로그 문장을 삼킬 수 있다(`Loc.StoredText`). `StoredLocalizationTests`가 실패하면 앞뒤에 고정 글자를 붙인다. 고친 문구는 사용자에게 알린다(계획 10장).
- 영어가 비면 한국어로 대신 보이고 `Loc.Missing`에 기록한다. 빈 문자열이나 키 이름을 화면에 내보내지 않는다.
- **접근성 문구**(`CardLabel.Aria` 등, 예: "회오리 · 5/5", "빠른 회전 · 선택됨 3/5", "피의 회복 · Lv15 해금 · 0/5")는 요소의 이름·설명으로 쓰고 시험이 읽는다. 시안에서 `aria-label`로 쓰인 문구와 같다.
- 영어 `open` 문구는 시안 그대로 "Unlocked Lv15"다(잠긴 패시브의 접근성 문구에서 이미 해금된 것처럼 읽힌다). 시안 문구를 바꾸기 전에는 그대로 옮기고, 바꾸려면 사용자에게 먼저 묻는다.
- 스킬 이름 옆의 id 치환: `ClassSkillTree.Display`의 정규식이 `A06-2`의 `A06`을 먼저 잡는다. 새 창은 데이터의 이름을 직접 쓰므로 이 함수를 거치지 않는다. 다른 곳(빠른 편집)에서 보조 스킬 id를 쓰기 전에 고친다(`research/dependents.md` B장).

## 13. 작업 묶음 (UI-1 ~ UI-6)

계획 14장의 **4단계(스킬창)**를 여섯 개의 작은 PR로 나눈다. 순서대로 진행하고 PR마다 관련 검사만 돌린다.

| 묶음 | 내용 | 주요 파일 | 완료 기준 |
|---|---|---|---|
| **UI-1** 화면 계산 코어와 정답 시험 | 1단계가 옮겨 둔 모델·규칙 위에, `golden/csharp`의 배치·화면 정보·세션·연출 신호(`Layout`, `ViewModel`, `Session`, `Fx`)를 `Presentation/SkillTreeD4/`로 옮긴다. 시험 도우미(`GoldenJson`, `GoldenShapes`)를 `Tests/Editor/`로 옮겨 **EditMode 시험 4종**(배치·화면 정보·상호작용·단위)을 만든다(규칙 시험은 1단계에 있다). `SkillTreeD4Units`와 3.1의 시험 벡터. | `SkillTreeD4.*.cs`, `SkillTreeD4Units.cs`, `SkillTreeD4*Tests.cs` | **55,803개 검사가 EditMode에서 통과**(1단계의 규칙 32,208개와 합쳐서). 정답 파일은 `Application.dataPath/../Docs/Design/D4SkillTree/golden/…`에서 읽고 복사하지 않는다. 1단계의 데이터 읽기(`SkillTreeD4.Data.cs`)가 시안 내보내기와 같은 모델을 채우면 그것으로 같은 정답을 통과해야 한다. |
| **UI-2** 그리기 도구 | 5장: `UiMesh` 확장, `PolylineGraphic`, `RankArcGraphic`, `TextGradient`, 블러·톤 셰이더, `UiTheme` 색 토큰. | `UiButtonFace.cs`(`UiMesh`), `SkillTreeD4Graphics.cs`, `Resources/*.shader`, `UiTheme.cs` | `GraphicComponentTests`, 메시 골든 시험, 할당 시험 통과. 도구마다 **샘플 판**(육각, 호 5·3칸, 그라데이션 테두리, 점선, 번짐, 선 상태 6종)을 한 장에 그린 스크린샷. |
| **UI-3** 창 껍데기와 정적 장면 | 3·4장: 어댑터 창, 단위, 계층, 요소별 뷰, 스크롤, `InMemorySkillTreeD4Host`(디자인 예시 배분: 전 영웅 레벨 24), `ADAPTERS` 등록과 문서, 이름 규칙(2.4). 패널 자리에는 자리표시만. | `SkillTreeD4Window*.cs`, `check_ui_contract.py`, `Shared_UI_Contract(.en).md` | 기준 렌더 **24장과 같은 상태**(3직업 × 가로·세로·세로 패시브 보기·PC × 2언어)를 정적으로 띄운 스크린샷 24장. `compare_screens.py --scene`의 요소별 점수가 합격선 안. `SkillTreeD4AnatomyTests`의 위젯 부분(카드·칸·패시브·배지·탭·범례) ±1pt. **이름 시험**(6장): `SkillTreeD4LookTests`가 정답의 요소별 `style` 이름과 맞는다. |
| **UI-4** 상세 패널과 머리 | 8·9장: 패널 6상태 + 핵심·잠김, 버튼, 막힌 이유, 설명 자리 예약, 머리·탭·범례·닫기, 글자(7장: 크기·맞춤·줄바꿈·그라데이션). | `SkillTreeD4Window.Panel.cs`, `.Header.cs`, `SkillTreeD4Wrap.cs`(필요하면) | `SkillTreeD4AnatomyTests`의 패널 6상태 ±1pt. 이름 시험이 `panel.parts`·패널 이름도 확인한다. **넘침 스모크**: 모든 액티브·보조 스킬(직업당 10 + 28)을 3직업 × 2언어 × 5화면 크기에서 차례로 골라 패널 밖 넘침 0, 설명 줄 수 ≤ 2(PC 3). 카드 이름이 모두 상자 안(7.3). |
| **UI-5** 입력·커밋·연출 | 10·11장: 선택·투자·회수·탭, `host.Commit`, 키 비교 갱신(4.4), 스크롤 유지, `SkillTreeD4Fx`를 재생하는 `FxPlayer`, 감소 모션. | `SkillTreeD4Window.Input.cs`, `.Fx.cs` | **클릭 재생 스모크**: `interaction/*.json`의 60건을 **실제 클릭**으로 재생해 걸음마다 화면 상태가 정답과 같다(한국어·영어). 연출 신호가 정답의 `events`와 같다. **할당 시험**(4.5): 연출 갱신·변화 없는 프레임·메시 그리기는 0 할당이고, 클릭 거래 프레임은 계층을 다시 만들지 않고 할당량을 보고에 적는다. 저장 후 다시 켜도 유지(`-hellscriptSkillTreeResume`). |
| **UI-6** 연결과 마감 | 계획 9.1의 진입점 4곳, `StoreViewBinding.Attach`의 `visibleState`, 부모 창 갱신(3.3), `SkillTreeD4.Enabled` 플래그 뒤에서 열기, 최종 L5 비교표. | `GameUI.*`, `HuntEdictWindow*.cs`, `RiftEntryWindow.cs` | 24장 기준 렌더와 **캡처를 모두 비교한 표**(14장)가 PR에 있다. 옛 창은 아직 그대로 동작한다(7단계 전). `check_ui_contract.py --verify`, `check_ui_refresh.py` 통과. |

- 각 묶음은 `claude/` 가 아닌 `codex/d4-skill-tree-ui-<n>` 브랜치에서 시작하고(계획 14장), 영어 커밋 메시지·영어 코드 주석을 쓴다.
- **하지 말 것**: 규칙을 창 코드에 다시 쓰지 않는다. 장면 사각형을 창 코드에서 다시 계산하지 않는다(L1이 깨진다). `localScale`로 PC 확대를 하지 않는다. 시안 hex를 코드에 쓰지 않는다. 아이콘마다 스텐실 `Mask`를 쓰지 않는다.

## 14. 눈으로 비교하는 절차와 합격선 (L5)

기준 렌더 24장은 `reference/<직업>-<배치>-<KO|EN>[-Passive].jpg`이다(세로 880×1912, 가로 1912×880, PC 2880×1620, 모두 시안 틀의 2배).

**1. 같은 상태로 만든다.** 렌더는 디자인 예시 상태(레벨 24, 예시 배분, 직업별 기본 선택, "패시브 보기"는 연계 패시브 선택)다. 스모크가 `InMemorySkillTreeD4Host.DesignSample()`로 같은 상태를 만든다(`SkillTreeD4Alloc.Sample`, 모든 영웅 레벨 24).

**2. 틀 그대로 캡처한다.** 창 크기를 시안 틀의 정수 배로 둔다: 세로 880×1912, 가로 1912×880, PC 2880×1620(모두 2배). 안전 영역은 `UiSafeArea.Simulate(왼쪽, 아래, 오른쪽, 위)`로 시안의 값을 같은 배율로 준다: 세로 (0, 68, 0, 124), 가로 (124, 42, 124, 0), PC 없음. 그러면 `P = 2`이고 캡처가 시안 틀과 1:1이다. `Capture(name)`(`ScreenCapture.CaptureScreenshotAsTexture` → PNG)로 `Temp/D4Shots/<직업>-<배치>-<언어>.png`에 쓴다. 맥 개발 빌드에서 창이 화면보다 크면 잘릴 수 있으니(`resizableWindow: 0`) 화면에 맞는 가장 큰 정수 배(1배 포함)로 하고 `--scale`을 맞춘다.

**3. 비교한다.**
```bash
python3 Docs/Design/D4SkillTree/golden/tools/compare_screens.py \
  --ref Docs/Design/D4SkillTree/reference/Warrior-Landscape-KO.jpg --game Temp/D4Shots/Warrior-landscape-ko.png \
  --scene Docs/Design/D4SkillTree/golden/layout/Warrior-landscape.json --scale 2 --out Temp/D4Compare/Warrior-landscape-ko
```
PC는 `--scale 2.5`(틀 1.25배 × 2배)이다. 도구는 PIL과 numpy만 쓴다. 결과: `report.json`, `diff.png`(열지도), `side_by_side.png`(기준 | 캡처 | 차이), 종료 코드(합격 0).

| 측정 | 뜻 | 합격선 | 틀렸을 때 볼 곳 |
|---|---|---|---|
| `colour` | 4pt로 흐린 그림의 평균 색 차이(0~255) | ≤ 6.0 | 전체 톤, 번짐·빛의 세기 |
| `edges` | 테두리·모양이 제자리에 있는가(상관) | ≥ 0.70 | 위치·크기, 둥근 정도 |
| `worst` | 8pt로 흐린 그림에서 가장 어긋난 칸 2%의 평균 | ≤ 32 | 통째로 틀린 영역 |
| `element` | 카드·칸·패시브·배지·트레이·선반의 이름 붙은 사각형 안 차이 중 최악 | ≤ 20 | **어느 요소의 모양이 틀렸는지**(이름·위치가 나온다) |

합격선은 기준 렌더끼리 비교해 맞췄다(`python3 tools/compare_screens.py --selftest`): 한국어 ↔ 영어(글자만 다름)는 통과, 1점 이동·잡음은 통과, **3점 이동·12% 밝기·다른 직업·카드 하나 누락은 실패**. 글꼴이 달라서(허용 근사 A1) 글자가 많은 요소(카드, 패널)의 점수가 조금 높을 수 있다. 그런 경우는 눈으로 확인한 뒤 표에 "A1" 등 이유와 함께 적는다. **작은 요소의 누락은 이 도구의 일이 아니다**(L1·L4가 잡는다). 이 도구는 **모양**(색·그라데이션·테두리·번짐)을 위한 것이다.

**4. 눈으로 본다.** `side_by_side.png`를 열어 다음을 하나씩 확인한다: ① 위치·크기 ② 색조 ③ 테두리 그라데이션(금 테두리의 밝고 어두운 변화) ④ 번짐·그림자 ⑤ 어둡게 처리한 아이콘(투명 .78 / .45, 채도) ⑥ 글자 줄바꿈과 잘림.

**5. 표를 PR에 붙인다.** 24장 각각: 네 측정값, 합격 여부, 불합격이면 이유(허용 근사 번호 또는 고칠 것). 고칠 것이 남으면 병합하지 않는다.

## 15. 알려진 함정

1. **단위 혼동**: 디자인 점(정답·시험) ≠ 캔버스 단위(점 × 2) ≠ 화면 픽셀(점 × P). 시험은 항상 점으로 말한다.
2. **레거시 `Text`**: 글자 크기가 정수, 줄 높이는 글꼴 배수(`lineSpacing`), 자간 없음, 한국어 `keep-all` 선례 없음(7장).
3. **마스크**: `RectMask2D`는 번짐과 판 밖 장식을 자른다(4.1). 장식은 마스크 밖 계층에 둔다.
4. **PC 확대**: `localScale`로 키우면 글자가 흐려진다. 좌표·크기·글자 크기에 곱한다.
5. **그리기 순서**: 선은 카드 아래, 태그는 모두 위. 선은 장면 순서 그대로 한 메시(바탕 빛 → 본 선).
6. **번짐의 색공간**: 프로젝트가 Linear 색공간이라 알파 번짐이 시안과 다르게 보일 수 있다. 전체 보정 상수 하나로 맞춘다(A7).
7. **드래그와 클릭**: 스크롤하다가 카드가 눌리면 안 된다. 클릭은 뗄 때 판정한다.
8. **새 id 아이콘**: `SkillIconAssets.TryResolve`가 보조 스킬 id(`W09-1` 등)를 모른다. 새 창은 아이콘 경로(`Art/ClassSkillIcons/<직업>/<iconId>`)를 직접 읽거나 `TryResolve`를 `<기본>-<n>` 형태로 넓힌다.
9. **A06·M07 아이콘 덮어쓰기**: 기존 아이콘과 같은 파일 이름이다. `.meta`의 guid가 그대로인지 확인하고 함께 커밋한다(3단계 일이지만 UI 시험이 새 그림을 가정한다).
10. **스모크 파일 이름**: `Runtime*Smoke*.cs`는 UI 계약 검사·갱신 검사·`LocalizationTests`가 건너뛴다. 새 스모크는 이 이름을 쓰고 `-hellscriptSkillTreeD4Smoke` 플래그로 켠다. 옛 스모크의 `ExistingTrees`/`OpenEdict`(v3 하네스)는 공유 픽스처로 옮긴 뒤 지운다(`research/dependents.md` F장).
11. **부모 창**: 칙령 창의 `HuntEdictEditSession`이 낡는다(3.3).
12. **`GetComponent … ?.` 금지**: `TryGetComponent`를 쓴다.
13. **CI**: `shared-ui.yml`이 `check_ui_contract.py --verify`를 돌린다. 어댑터 등록과 색 토큰이 빠지면 빨갛게 된다.
14. **성능**: 아이콘 48개마다 스텐실 `Mask`를 쓰면 배칭이 깨진다. UV가 있는 둥근 사각형 그래픽으로 자른다(5장 9번). 스크롤 내용을 움직이면 캔버스 전체가 재배치된다 — 스크롤 중에는 갱신을 미룬다.
15. **화면 갱신 키**: `visibleState`에 영웅 3명의 레벨·배분·언어가 들어가야 한다. 빠지면 다른 창에서 바꾼 값이 반영되지 않는다.

## 16. 사용자 확인이 필요한 항목 (화면)

기본값으로 진행할 수 있다. 사용자가 다르게 정하면 그 묶음 전에 바꾼다.

| # | 질문 | 기본값 |
|---|---|---|
| D6 | 글꼴(7.1) | 기존 `UiFonts`. Noto Serif KR·Cinzel 번들은 사용자가 용량과 일관성을 정해야 한다. |
| DU1 | 상세 패널 배경 그림 블러(A2)를 셰이더로 도입할지 | 도입한다(`GlobalHudFade.shader` 기반 한 개). 거절하면 흐림 없는 그림으로 간다. |
| DU2 | 마우스 올림 효과와 키보드·패드 이동 | 하지 않는다(시안에 없음). 필요하면 별도 작업으로 시안부터 만든다. |
| DU3 | 한국어 줄바꿈이 어절 중간에서 끊길 때 | `SkillTreeD4Wrap`으로 어절 단위 직접 줄바꿈한다(7.4). |
| DU4 | 클릭 거래 프레임의 GC 할당 한도(4.5) | 한도를 두지 않고 스모크가 크기를 적기만 한다. 첫 실측 뒤 한도가 필요한지 사용자와 정한다. |
