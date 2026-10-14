# 스킬창 정답 파일과 참조 구현

작성일: 2026-10-12 · 대상: Codex 구현 작업 · [English](README.en.md)

이 폴더는 "게임 안의 스킬창이 디자인 시안과 **같은 모양, 같은 동작**인가"를 사람이 눈으로만 보지 않고 **기계로 확인**하기 위한 자료다.

- 시안(`../SkillWindow.dc.html`)은 실행되는 명세다. 이 폴더의 정답 파일은 그 시안을 실제로 돌려서 뽑았다.
- 참조 구현(`reference/`)은 시안의 계산을 읽기 쉽게 다시 쓴 JavaScript다. 정답 파일과 **55,803개 검사가 모두 일치**하는 것을 확인했고(`node reference/test_reference.js`), 일부러 틀리게 고치면 검사가 실패하는 것도 확인했다.
- 게임(C#)은 참조 구현을 거의 줄마다 옮기고, **같은 정답 파일로 같은 검사**를 EditMode 테스트로 돌린다. 테스트가 통과하면 규칙, 배치, 문구, 상호작용이 시안과 같다는 뜻이다.

## 파일

| 위치 | 내용 | 게임에서 쓰는 곳 |
|---|---|---|
| `reference/` | 참조 구현 (`model` `rules` `layout` `viewmodel` `session`) + `test_reference.js` | C# 포팅의 원본. 각 파일 머리의 주석이 해당 장의 규칙이다. |
| `rules_golden.json` | 규칙 엔진 정답. 16개 영웅 레벨 × 직업 3 × 배분 6 = 288개 배분에서 **모든 노드의 투자·회수 결과**, 그리고 무작위 70걸음 재생 48개 | 6장 규칙 엔진 `SkillTreeD4RulesTests` |
| `layout/<직업>-<landscape\|portrait\|pc>.json` | 화면 9종 × 언어 2의 **모든 요소의 사각형, 상태, 글자**. 그리는 순서대로 나열 | 배치 함수 `SkillTreeD4LayoutTests`, 모양 이름 `SkillTreeD4LookTests` |
| `layout_scenarios/<직업>-<landscape\|portrait\|pc>.json` | 시안 예시 배분 밖의 **10가지 상태**(레벨 1~60, 투자 가능한 보조 스킬 칸, 다음 랭크 그림자 막대, 켜진 패시브 메달 …)에서 장면의 요소별 해시 | 배치 함수의 동적 상태 검사 `SkillTreeD4LayoutTests` |
| `viewmodel/<직업>-<ko\|en>-<landscape\|portrait>.json` | 노드 48개를 하나씩 골랐을 때 패널·탭·머리 수치의 글자와 켜짐 여부 | 화면 정보 구성 `SkillTreeD4ViewModelTests` |
| `interaction/<직업>-<ko\|en>.json` | 시안을 실제로 눌러서 기록한 **연속 조작 60건**(선택·투자·회수·탭). 걸음마다 화면 상태와 배분, 그리고 그 걸음이 시작한 **연출 신호**(`events`: 선택이 시작한 반짝임 선, 투자·회수가 시작한 노드 연출과 점화 선) | 상호작용 테스트와 스모크 |
| `styles.json` | 요소의 **모양**(CSS 선언) 428종. 상태 이름으로 찾는다. 배치 파일이 이 이름을 가리킨다. **모양은 이름 하나로 정해진다**(이름이 상태를 다 담지 못한 경우가 없음을 생성기가 확인한다). 막대 너비(`bars` `bar`)와 랭크 호의 켜진 칸 수(`arc`)는 요소마다 다른 값이라 모양 밖에 둔다 | 그리기 구현(9장)의 기준 |
| `panel_parts.json` | 상세 패널 **안쪽**(단추, 막힌 이유 줄, 보조 행, 패시브 블록)의 모양 이름. 노드 48개를 하나씩 골랐을 때 3직업 × 배치별로 적었다. 디자인 예시 상태(레벨 24)에는 투자 가능 상태가 없어서 **레벨 60 두 상태**(빈 배분, 아래 티어 가득)를 더했다(이 둘은 가로·세로만). 같은 부분 객체는 `parts[]`에 한 번만 두고 노드는 번호로 가리킨다 | 모양 이름 시험 `SkillTreeD4LookTests` |
| `anatomy/<배치>-<ko\|en>.json` | 브라우저에서 **실측한 위젯 내부 치수**: 상세 패널 6가지 상태, 카드·배지·패시브·탭·범례 등의 안쪽 사각형과 글자 크기 | 상세 패널과 위젯 내부 배치 |
| `rank_values.json` | 스킬·보조 스킬·패시브의 **랭크별 값**(규칙으로 계산) | 전투 수치와 생성기 테스트 |
| `fx.json` | 연출 키프레임 31종과 클래스별 길이·곡선 | 연출 구현(9.6) |
| `anatomy/OUTLINE.md` | 위 실측 치수를 들여쓰기 목록으로 풀어 쓴 것(생성 파일) | 읽기 |
| `anatomy/TEXT_STYLES.md` | 실측한 글자 스타일 129종(생성 파일) | 글자 구현 |
| `tools/` | `css_features.js`(디자인이 쓰는 CSS 기능·색 통계), `print_anatomy.js`(실측 치수·글자 스타일 목록), `measure_anatomy.mjs`(실측 스크립트, 디자인 담당용), `scene_dump.js`(시나리오 장면 출력), **`compare_screens.py`**(게임 스크린샷과 기준 렌더 비교, L5) | 계획·디버깅·검증 |
| `build_golden.js`, `verify_golden.js`, `lib/` | 정답 파일 생성기와 최신 여부 검사 | 시안이 바뀔 때 |

`anatomy/`만 사람이 브라우저로 잰 값(Chrome 헤드리스)이라 생성기가 만들지 않는다. 나머지는 모두 결정적이다.

## 약속(충실도 계약)

정답 파일과 **같아야 하는 것**:
- 규칙 엔진의 모든 판정(막힌 이유 포함)과 결과 배분. 해시까지 같다.
- 배치: 모든 사각형의 x, y, 너비, 높이(오차 0.01pt 이하), 요소 수, 그리는 순서, 상태 이름, 카드 글자 크기(`fitFontSize`).
- 문구: 패널, 탭, 머리 수치, 노드 이름과 접근성 문구, 막힌 이유. 한국어·영어 모두.
- 상호작용: 걸음마다 받아들였는지와 결과 상태.
- 모양 이름: 요소마다, 그리고 상세 패널의 단추·행·블록마다 어떤 이름(`styles.json`)을 쓰는지(`layout/*.json`의 `style`과 `panel.parts`, `panel_parts.json`). 참조 구현은 모양을 계산하지 않으므로 게임의 `SkillTreeD4LookTests`가 확인한다.

**달라도 되는 것**(엔진 차이):
- 글꼴 모양(17장 D6). 글자가 줄 바꿈되는 위치는 글꼴에 따라 조금 다를 수 있다. 줄 수 상한(설명 줄 2줄, PC 3줄)은 지킨다.
- 그라데이션, 번짐, 안쪽 그림자의 근사 방식. 색은 `styles.json`의 값을 쓴다.
- 연출은 `fx.json`의 길이와 곡선을 따르되 프레임 단위 일치는 요구하지 않는다.

## 쓰는 방법

```bash
node Docs/Design/D4SkillTree/golden/reference/test_reference.js   # 참조 구현이 정답과 같은지(1초 이내)
node Docs/Design/D4SkillTree/golden/verify_golden.js              # 정답 파일이 시안과 같은지
node Docs/Design/D4SkillTree/golden/build_golden.js               # 시안이 바뀐 뒤 정답 파일 다시 만들기(디자인 담당)
```

C# 테스트에서 읽는 위치: `Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Docs", "Design", "D4SkillTree", "golden", "<파일>"))`. 정답 파일을 복사해서 두지 않는다.

### 배분 문자열과 해시

배분은 한 줄 문자열이다. 액티브마다 `랭크,고른보조스킬,보조스킬랭크`를 `;`로 잇고, `|` 뒤에 패시브 랭크를 `,`로 잇는다. 고르지 않은 보조 스킬은 `-1`이다.

```
2,-1,0;0,-1,0;5,2,3;0,-1,0;3,1,1;2,-1,0;1,-1,0;0,-1,0;0,-1,0;0,-1,0|3,2,0,0,0,0,0,0,0,0
```

해시는 이 문자열의 UTF-8 바이트에 대한 FNV-1a 32비트이고, 소문자 16진수 8자리로 쓴다.

```csharp
static string Hash(string text){ uint h=2166136261; foreach(var b in System.Text.Encoding.UTF8.GetBytes(text)){ h^=b; h*=16777619; } return h.ToString("x8"); }
```

### 규칙 정답 읽기

`rules_golden.json`의 `heroes.<직업>.snapshots[]`는 한 배분과 그 배분에서 가능한 모든 조작이다.

- `alloc`: 배분 문자열, `level`: 영웅 레벨(점수 = 레벨 − 쓴 점수)
- `spent`, `left`, `tierPoints[5]`, `pointsBefore[5]`, `tierOpen[5]`: 파생 값
- `inv`, `ref`: 노드 키(`a3`=액티브 3, `a3m1`=액티브 3의 보조 스킬 1, `p6`=패시브 6)마다 결과
  - 투자: `ok:<새 배분 해시>` 또는 막힌 이유 `tier:3` `max` `pts` `base` `swap` `cap` `key` `lv:25`
  - 회수: `ok:<새 배분 해시>` 또는 `no`

`sequences[]`는 빈 배분에서 시작해 `op`(`inv`|`ref`)와 `node`를 차례로 적용한 기록이다. 걸음마다 `result`와 마지막 `finalHash`가 같아야 한다.

### 배치 정답 읽기

`layout/<직업>-<배치>.json`의 `locales.ko|en`은 한 화면이다.

- `frame`: 무대 크기와 배율(PC는 1152×648에 1.25배)
- `header`, `plate`, `panel`: 머리(제목·탭·수치·닫기), 트리 판(사각형, 안쪽 크기, PC 확대·이동), 상세 패널의 사각형
  - `panel.parts`: 패널 **안쪽**의 모양 이름. `buttons`(`layout` = `header` 정사각 단추 | `bottom` 막대 단추, `refund`, `invest`, `shine`), `reason`(세로 배치의 막힌 이유 줄, 없으면 `null`), `modifiers[]`(`focused`, `row`, `name`, `effect`, `effectTo`, `right`, `socket`), `passive`(`unlock`, `unlockBar`, `effect`, `effectTo`, `linkedHeading`, `links[]` = `chip`·`level`·`icon`). 액티브면 `passive`가 `null`이고 패시브면 `modifiers`가 `null`이다.
- `primitives[]`: 그리는 순서(뒤가 위). 종류: `band` `tray` `line` `arrowhead` `badge` `card` `modifier` `passive` `tag`
  - 모든 사각형은 `{x,y,w,h,minW}`이고 `null`은 "정해지지 않음"이다(글자 크기에 따라 정해지는 칩의 너비 등).
  - 카드 `id`, 보조 스킬 `id`, 패시브 `id`와 아이콘의 `iconId`(원본 파일 이름), `id`(게임 id)
  - `state`: 카드 `l`(투자함) `o`(투자 가능) `k`(잠김), 보조 스킬 `chosen` `available` `idle` `unlearned` `tier-locked`, 패시브 `l` `o` `k`, 배지 `open` `next-locked` `locked`, 선 `g` `o` `k` `s1` `s2` `s3`
  - `style`, `glow`, `boxStyle` 등은 `styles.json`의 이름이다.
- 언어에 따라 위치는 같고 글자와 카드 글자 크기만 다르다(생성기가 확인한다).
- `views.passives`(세로): 패시브 보기로 열 때의 선택과 처음 스크롤 위치.

### 참조 구현 쓰는 법

```js
const R = require("./reference");
const model = R.loadModel(require("../skill_tree_d4.json"));
const s = new R.Session(model, { hero: "Warrior", locale: "ko", orient: "landscape", level: 24 });
s.select({ kind: "modifier", i: 2, j: 2 }); s.invest(); s.tab(1);
s.scene();      // 화면의 모든 요소(배치 정답과 같은 모양)
s.viewModel();  // 패널·탭·수치의 글자와 켜짐 여부
s.snapshot();   // 상호작용 정답의 한 걸음과 같은 모양
```

C#으로 옮길 때의 대응: `model.js` → 데이터 읽기(`SkillTreeD4.json`), `rules.js` → 규칙 엔진, `layout.js` → 배치 순수 함수, `viewmodel.js` → 화면 정보 구성, `session.js` → 창이 들고 있는 상태.

## C# 이식본 (검증 끝남)

`csharp/`에 참조 구현의 C# 이식본이 있다. **JavaScript 참조 구현과 똑같은 55,803개 검사를 같은 정답 파일로 통과한다**(규칙 32,208 · 배치 2,975 · 배치 시나리오 15,034 · 화면 정보 600 · 상호작용 4,986). 일부러 상수와 조건을 틀리게 고쳐 보면(JavaScript·C# 각각 20곳 안팎) 검사가 모두 실패하는 것도 확인했다.

```bash
cd Docs/Design/D4SkillTree/golden/csharp && dotnet run     # Unity 없이 약 0.5초. 종료 코드 0 = 모두 일치
```

| 파일 | 내용 | 게임에서 |
|---|---|---|
| `SkillTreeD4.Model.cs` | 데이터 모델, 배분(Allocation) 문자열·해시, 선택 | `Runtime/Core`로 복사 |
| `SkillTreeD4.Rules.cs` | 규칙 엔진 `Apply` (막힌 이유 포함) | `Runtime/Core`로 복사 |
| `SkillTreeD4.Layout.cs` | 배치 순수 함수 `SkillTreeD4Layout.Build` → `SkillTreeD4Scene` | `Runtime/Presentation`(UnityEngine 없음) |
| `SkillTreeD4.ViewModel.cs` | 패널·탭·수치·접근성 문구 `SkillTreeD4ViewBuilder.Build` | 같은 곳 |
| `SkillTreeD4.Session.cs` | 선택·투자·회수·탭 상태기계 | 같은 곳 |
| `SkillTreeD4.Fx.cs` | 연출 신호 규칙(`FxOf`, `LineOverlays`): 어느 노드에 연출을 줄지, 최대 랭크, 새로 열린 티어, 점화·반짝임 선과 지연 | 같은 곳 |
| `Tests/GoldenJson.cs` `Tests/GoldenShapes.cs` `Tests/Program.cs` | 시험용 JSON 읽기, 정답 모양 변환, 검사 실행 | EditMode 테스트로 옮김 |
| `Tests/SpecLoader.cs` | 시안 내보내기 파일에서 모델 채우기(**시험 전용**) | 게임은 `SkillTreeD4.json`을 JsonUtility DTO로 읽어 같은 클래스를 채움 |

- C# 9, `System`만 사용. 좌표는 `double`(오차 0.01pt이므로 게임에서 `float`로 바꿔도 된다).
- 이식본은 **출발점**이다. 이름과 폴더는 게임 규칙에 맞게 바꿔도 되지만 동작은 이 검사를 계속 통과해야 한다.
- 정렬은 안정 정렬이어야 한다(`OrderBy`; `List.Sort`는 불안정). 반올림은 JavaScript처럼 반올림(`Math.Floor(x + 0.5)`)이다(`Math.Round`는 짝수 반올림).
- 게임 쪽에서 주의: 위 파일에는 한국어 문구가 없다. 문구는 모두 문구 표(`strings`)에서 오며, 이식본이 쓰는 `KeyPrefix`("핵심 · " / "Key · ") 키만 시안 내보내기에 없으므로 문구 표에 새로 넣는다.

## 시안이 바뀌면

1. 디자인 담당이 `node extract.js SkillWindow.dc.html skill_tree_d4.json behaviour.json`을 돌린다.
2. `node golden/build_golden.js`로 정답 파일을 다시 만들고 `node golden/reference/test_reference.js`가 실패하면 참조 구현을 고친다.
3. `verify_golden.js`가 통과하는 것을 확인하고 함께 커밋한다.
4. 게임 쪽 테스트가 실패하는 곳이 바뀐 곳이다. 구현이 시작된 뒤에는 **수치를 게임 쪽 JSON에서 먼저 고치고** 시안에 알린다(계획 5.3장).
