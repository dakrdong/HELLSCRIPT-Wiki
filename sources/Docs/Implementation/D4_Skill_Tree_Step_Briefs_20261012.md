# 단계별 작업서와 Codex 프롬프트 (Codex 전달용)

> **2026-10-28 A2 정정:** 큰 칙령 창·≡ 메뉴·전용 부품·스모크 설명은 과거 기록이다. [현재 진입 코드](../../Assets/HELLSCRIPT/Runtime/Presentation/GameUI.HuntEdict.cs)와 [대체 화면·소유 코드](Hunt_Edict_Window_Removal_20261028.md)를 따른다. 이전 캡처와 검사 수치는 당시 구현의 근거다.

작성일: 2026-10-12 · 대상: Codex 구현 작업 · [English](D4_Skill_Tree_Step_Briefs_20261012.en.md)

작성: Claude (작업서). 구현·검증·커밋은 Codex가 한다.

이 문서는 계획 문서의 14장(작업 순서)을 **단계마다 바로 시작할 수 있는 작업서**로 풀어 쓴 것이다. 단계마다: 선행 조건, 읽을 문서, 만들고 바꿀 것, 시험, 끝났다는 기준, 하지 말 것, 그리고 **Codex에 그대로 붙여 넣을 프롬프트**가 있다.

## 0. 자료 지도

| 문서 | 내용 |
|---|---|
| [`D4_Skill_Tree_Plan_20261012.md`](D4_Skill_Tree_Plan_20261012.md) | 규칙, 데이터, 저장, 전투, 화면, 순서, 결정 목록(17장) |
| [`D4_Skill_Tree_UI_Spec_20261012.md`](D4_Skill_Tree_UI_Spec_20261012.md) | 화면 구현 명세(충실도 6층, 단위, 계층, 그리기 도구, 모양, 글자, 패널, 연출, 작업 묶음 UI-1~6, 눈으로 비교) |
| [`D4_Skill_Tree_Combat_Spec_20261012.md`](D4_Skill_Tree_Combat_Spec_20261012.md) | 전투 구현 명세(공통 틀, 보조 스킬 84종 분류, 패시브, 궁극기, 질문과 기본값) |
| `Docs/Design/D4SkillTree/skill_tree_d4.json` | 기획 데이터 원본 |
| `Docs/Design/D4SkillTree/golden/` | 정답 파일 + JS 참조 구현 + **검증된 C# 이식본**(`csharp/`). `README.md` 먼저 |
| `Docs/Design/D4SkillTree/research/` | 코드 조사 기록(읽기 전용): 화면 기반, 직업별 전투, 저장·이전, 의존처 |
| `Docs/Design/D4SkillTree/icons_manifest.json` | 새 아이콘 92장의 원본 경로, sha256, 가져올 위치 |
| `Docs/Design/D4SkillTree/reference/*.jpg` | 기준 렌더 24장 |

## 1. 모든 단계에 공통인 규칙

- 프로젝트 지침(`AGENTS.md`, `AgentRules/`, `CLAUDE.md`)을 먼저 따른다. 사용자와의 대화·개발 기록은 **한국어**, 코드·주석·커밋 메시지·로그는 **영어**, 결과물은 한국어·영어 함께.
- 한 단계 = 한 브랜치(`codex/d4-skill-tree-<단계>`) = PR 하나. 최신 `origin/main`에서 **별도 worktree**로 시작한다. 단계 0이 병합되기 전이면 `claude/d4-skill-tree-plan`에서 시작한다. 공유 체크아웃에서 브랜치를 바꾸지 않는다.
- 기본은 한 작업·한 에이전트이고 동시 작업은 최대 3개다. 아래 의존표를 지킨다.
- **작업 중에는 바꾼 부분의 검사만** 돈다. 전체 EditMode와 macOS 스모크는 **7단계의 마지막에 한 번**이다. 문서만 바꾸면 게임 실행 검사를 하지 않는다.
- **`Docs/Design/D4SkillTree/golden/`, `skill_tree_d4.json`, `SkillWindow.dc.html`은 고치지 않는다**(디자인 담당의 파일). 정답 파일이 틀렸다고 의심되면 실패한 검사 이름과 입력을 보고하고 멈춘다. 구현 쪽에서 수치를 먼저 바꿔야 하면 **게임 쪽 JSON을 먼저 고치고** 디자인 담당에게 알린다(계획 5.3).
- 위키는 작업 브랜치에서 생성·게시하지 않는다. 병합 때 한 번(`AgentRules/Wiki_Publish.md`).
- 색 리터럴은 `UiTheme` 토큰으로, 새 한국어 문구는 `en.txt` 항목과 함께(`LocalizationTests`), 새 창은 `ADAPTERS` 등록과 문서(UI 명세 3.2).
- 새 출력(빌드, 스크린샷, 비교 결과)은 소유 작업별 한 곳에 모으고 `artifact-lifecycle.json` 형식으로 기록하고, 끝날 때 보존·정리 상태를 적는다(`AgentRules/Artifact_Cleanup.md`).
- 결과 보고: 바꾼 것, **실제로 돌린 검사와 결과**, 돌리지 않은 범위, 사용자 결정이 필요한 것. macOS 결과를 실기기 결과로 보고하지 않는다.
- **PR 본문 틀**: ① 변경 요약 ② 검증(명령, 결과, 숫자) ③ 미검증 범위 ④ 사용한 기본 결정(D번호) ⑤ 다음 단계에 넘기는 것.

## 2. 단계와 의존

| 단계 | 브랜치 | 선행 | 병렬 가능 |
|---|---|---|---|
| 0 | `claude/d4-skill-tree-plan` | | 사용자가 PR로 병합(위키 게시 포함) |
| 1 데이터·규칙 | `codex/d4-skill-tree-data` | 0 | |
| 2 저장·이전·만렙 | `codex/d4-skill-tree-save` | 1 | 3과 병렬 |
| 3 아이콘 | `codex/d4-skill-tree-icons` | 0 | 1·2와 병렬 |
| 4 스킬창 UI-1~6 | `codex/d4-skill-tree-ui-<n>` | 1 (UI-3부터 3) | 5와 병렬 |
| 5.0 전투 공통 틀 | `codex/d4-skill-tree-combat-core` | 1, 2 | |
| 5a/5b/5c 직업별 전투 | `codex/d4-skill-tree-combat-<직업>` | 5.0 | 서로 병렬(최대 3) |
| 6 연결부 | `codex/d4-skill-tree-integration-<n>` | 4(UI-6), 5 | |
| 7 전환 | `codex/d4-skill-tree-switch` | 1~6 | |

## 3. 단계 0 — 문서 병합 (사용자)

- 사용자가 `claude/d4-skill-tree-plan`을 PR로 병합한다. 이 변경은 문서와 디자인·정답 자료뿐이다(게임 코드 없음).
- 병합 전 확인(이미 돌렸음): `node Docs/Design/D4SkillTree/golden/verify_golden.js`(정답 파일이 시안과 같음), `node Docs/Design/D4SkillTree/golden/reference/test_reference.js`, `cd Docs/Design/D4SkillTree/golden/csharp && dotnet run`(둘 다 55,803개 일치).
- 병합하면 `Docs/`가 늘어나므로 위키 게시 절차(`AgentRules/Wiki_Publish.md`)가 적용된다.

## 4. 단계 1 — 데이터·생성기·규칙 엔진

**선행**: 0. **브랜치**: `codex/d4-skill-tree-data`.

**읽을 것**: 계획 5·6장 → `golden/README.md` → `golden/csharp/SkillTreeD4.Model.cs`, `.Rules.cs` → `golden/reference/rules.js`.

**만들고 바꿀 것**
1. `tools/build_skill_tree_d4.py`(+ `check` 모드): `skill_tree_d4.json`에서 `Assets/HELLSCRIPT/Resources/Data/SkillTreeD4.json`을 만든다. 담을 것: 규칙(티어 문, 레벨 상한), 직업별 티어·액티브(능력치 `rank1/rankMax/value`, 보조 스킬 `ko/en/effect/shape/value`, 설명)·패시브(`linkedActiveIndices`, `value`)·`spriteCells`, 화면 문구 `strings`, `legacyRemap`(이전에 쓴다). **담지 않을 것**: `designSample`, `designSelection`, `behaviourKo`(시안 전용).
2. `Runtime/Core/SkillTreeD4/SkillTreeD4.Model.cs`, `.Rules.cs`를 `golden/csharp`에서 옮긴다(이름·폴더는 게임 규칙에 맞게 바꿔도 된다). `SkillTreeD4.Data.cs`: `JsonUtility` DTO로 `SkillTreeD4.json`을 읽어 같은 모델 클래스를 채운다. 정적 클래스 `SkillTreeD4`(`Enabled` 플래그, 기본 false, `-hellscriptSkillTreeD4`로 켬).
3. 랭크별 값: `SkillTreeD4Values`(스킬·보조 스킬·패시브의 `value`에서 랭크 r의 값을 돌려준다. 선형, 개수 단위 `명 회 발 개`는 `floor`). **`golden/rank_values.json`과 모두 일치**해야 한다.
4. 시험(`Tests/Editor/`): `GoldenJson.cs`를 `golden/csharp/Tests`에서 옮긴다. `SkillTreeD4RulesTests`(`rules_golden.json`의 32,208개), `SkillTreeD4DataTests`(생성 JSON이 시안 내보내기와 같음, 랭크 값 일치, 아이콘 id 셀 대응), `SkillTreeD4RulesTests`가 쓰는 모델은 **게임 데이터에서 읽은 것**이다.
5. 읽기 경로: 시험은 `Path.Combine(Application.dataPath, "..", "Docs", "Design", "D4SkillTree", "golden", …)`로 정답을 읽는다. 복사하지 않는다.

**끝났다는 기준**: `python3 tools/build_skill_tree_d4.py check` 통과, EditMode `SkillTreeD4RulesTests`·`SkillTreeD4DataTests` 통과(32,208개 + 랭크 값). 이 PR은 게임 동작을 바꾸지 않는다(플래그 뒤).

**하지 말 것**: 규칙을 다른 곳에 따로 쓰지 않는다. `golden/`을 고치지 않는다. 옛 `ClassSkills.json`·`ClassSkillTree.json`을 건드리지 않는다.

**프롬프트**
```
HELLSCRIPT 저장소에서 작업한다. 먼저 AGENTS.md, AgentRules/Workflow.md와 CLAUDE.md를 읽고 따른다(대화·기록은 한국어, 코드·주석·커밋·로그는 영어).
브랜치 codex/d4-skill-tree-data를 최신 origin/main(단계 0 병합 전이면 claude/d4-skill-tree-plan)에서 새 worktree로 시작한다.
읽을 문서(순서): Docs/Implementation/D4_Skill_Tree_Step_Briefs_20261012.md 4장, D4_Skill_Tree_Plan_20261012.md 5·6장, Docs/Design/D4SkillTree/golden/README.md, golden/csharp/SkillTreeD4.Model.cs와 SkillTreeD4.Rules.cs.
작업: 단계 1(데이터·생성기·규칙 엔진). tools/build_skill_tree_d4.py(check 모드 포함)로 Resources/Data/SkillTreeD4.json을 만들고, golden/csharp의 Model·Rules를 Runtime/Core/SkillTreeD4로 옮기고, SkillTreeD4.Data.cs(JsonUtility DTO)와 SkillTreeD4Values(랭크별 값)를 만든다. SkillTreeD4.Enabled(기본 false)를 둔다.
검증: python3 tools/build_skill_tree_d4.py check, EditMode SkillTreeD4RulesTests(rules_golden.json 32,208개 일치)와 SkillTreeD4DataTests(golden/rank_values.json 일치). 전체 EditMode나 스모크는 돌리지 않는다.
하지 말 것: Docs/Design/D4SkillTree/golden, skill_tree_d4.json, SkillWindow.dc.html을 고치지 않는다. 규칙을 다른 곳에 따로 쓰지 않는다. 옛 ClassSkills.json·ClassSkillTree.json을 건드리지 않는다.
보고: 바꾼 것, 실제로 돌린 검사와 결과(숫자), 돌리지 않은 범위, 사용자 결정이 필요한 것. PR 본문은 브리프 1장의 틀을 따른다.
```

## 5. 단계 2 — 저장 v5, 이전, 만렙 60, 포인트 = 레벨, 장착 규칙

**선행**: 1. **브랜치**: `codex/d4-skill-tree-save`. **가장 위험한 단계다**: 모든 저장이 지나간다.

**읽을 것**: 계획 7장 → `research/save_migration.md` **전체**(0장 "계획을 바꾸는 발견" 10가지, D장 알고리즘, E장 시험 방법) → 계획 17장 D3·D4·D9·D10과 새 결정 D13~D20.

**만들고 바꿀 것**
1. **v5 배분 저장**: `ClassSkillLoadout`에 version 5. `SkillInvestment`에 `modifier`(기본 −1)와 `modifierRank`를 더한다. `Rank(id)`는 v5에서 없는 id에 0. `Canonical`은 **버전을 보존**하고(v2 검증용 영웅이 수백 개 시험에서 쓰인다) 변환은 계정 수준에서 한다. v5 로드 검증: `ranks`는 직업마다 **10+10 = 20개 항목**이다(`skill_tree_d4.json` 순서로 액티브 10개 = 일반 8 + 궁극기 2, 그다음 패시브 10개 = 일반 8 + 핵심 2). 항목은 모두 같은 `SkillInvestment`이고 패시브 항목은 `modifier == -1`·`modifierRank == 0`이다. 그 밖에 랭크 ≤ 최대(5, 궁극기 3), `modifier == -1`일 때만 `modifierRank == 0`, `modifierRank ≤ min(최대, 랭크)`, 패시브 랭크 > 0이면 해금 레벨 ≤ 영웅 레벨이고 핵심은 하나만, `Spent ≤ level`, `SkillTreeD4Rules.Valid`, 장착한 스킬은 랭크 > 0이고 4번 칸은 궁극기, 레벨 ≤ 60.
2. **이전**: `SkillTreeD4Migration.Apply(AccountSave)` — public static, 순수(입출력 없음), 보고서를 돌려준다. **`GameStore.Read()`에서 `Normalize` 앞에 부른다**(발견 1: 로드 뒤 변경은 `Write` 검증에 플레이어 편집으로 잡혀 앱이 시작하지 못할 수 있다). `GameStore.Sandbox`와 시험 픽스처도 부른다. 알고리즘은 저장 조사 D장 1~10단계를 그대로 따른다(영웅별 `old` 읽기 → 환급 → v5 빈 배분 → 칙령 문서·프리셋·훈련 기록·퍼즐 킷 정리 → 원본 보관 → 안내). 실패는 `NotSupportedException`으로 감싼다(발견 2). 멱등이어야 한다.
3. **스키마**: `GameStore.MaximumSchemaVersion` 21 → 22. 옛 바이너리는 22를 거부한다. 이전 전에 원본을 `.schema{n}-before-d4-skill-tree.json`로 복사한다(`!File.Exists`).
4. **만렙 60**: `LevelCap`(`ClassSkills.cs:62`), `GameStore.cs:64`의 날 40, `ClassSkills.cs:137`·`:147`, `ClassSkillTree.cs:42`(캐시 키). 경험치 공식은 그대로이고 상한만 늘린다(D9). 레벨 40 고정 픽스처(`CombatSimulation.SkillPreview.cs:31`, `ClassSkillValidation.cs:33`, `HuntEdictWindow.CombatPreview.cs:90`, `FullSkillTraining`)는 `LevelCap`에서 떼어 명시 상수로 둔다.
5. **포인트와 장착**: `Remaining = level − Spent`. 랭크 0은 장착 불가를 v5에서 다시 적는다.
6. **공유 코드**: 새 접두사로 v5를 낸다. 옛 HED3/4/5는 정책(옵션·순서·자동)만 가져오거나 거부한다(D14 기본).
7. 진행 중인 런의 처리(D13 기본: 빈 배분으로 바꾸고 `ClassSkillRuntimeState`를 정리).
8. Balance1000: 지문 대상 파일을 고치면 `python3 tools/balance_1000.py`를 다시 돌려 산출물을 갱신한다.

**시험**: 옛 저장 **원본 바이트**를 단계 시작 전에 `Tests/Fixtures/D4Migration/`에 만든다(v4, v3, 없음/레거시, W15·M13 시전이 든 진행 중 런, 프리셋 5개, HED3/4/5 코드). 새 클래스로 다시 만들면 진짜 옛 모양이 아니다. "옛 저장 X가 정확히 Y로 이전된다" 시험(저장 조사 E장 레시피), 멱등(두 번째 열기), 스키마 가드(새 스키마 거부와 바이트 불변), 점진 공개 계정, `rules_golden.json`의 모든 배분이 저장→로드→`Encode`를 거쳐도 `Valid`.

**끝났다는 기준**: 이전·호환·만렙 시험 통과, 기존 `ClassSkillCompatibilityTests`·`GameStoreTests`와 영향받는 스위트 통과, `classSkillValidation` 영웅의 동작 불변.

**하지 말 것**: 저장 키 이름을 바꾸지 않는다. 자동 재배분을 하지 않는다(D4). 이전 때 원본 보관 없이 쓰지 않는다. **`SkillTreeD4.Enabled`가 꺼진 빌드에서는 이전·스키마 22·만렙 60을 일으키지 않는다**(D18 기본: 켜졌을 때만. 끈 빌드가 22를 읽으면 거부한다).

**프롬프트**
```
HELLSCRIPT 저장소에서 작업한다. 먼저 AGENTS.md, AgentRules/Workflow.md와 CLAUDE.md를 읽고 따른다(대화·기록 한국어, 코드·주석·커밋·로그 영어).
브랜치 codex/d4-skill-tree-save를 단계 1이 병합된 최신 origin/main에서 새 worktree로 시작한다.
읽을 문서(순서): Docs/Implementation/D4_Skill_Tree_Step_Briefs_20261012.md 5장, D4_Skill_Tree_Plan_20261012.md 7장과 17장(D3 D4 D9 D10 D13~D20), Docs/Design/D4SkillTree/research/save_migration.md 전체.
작업: 단계 2. (1) ClassSkillLoadout v5(SkillInvestment.modifier/modifierRank, Rank() 0 기본, 로드 검증) (2) SkillTreeD4Migration.Apply(AccountSave)를 GameStore.Read()의 Normalize 앞에서 호출, 원본 보관, 멱등 (3) MaximumSchemaVersion 21→22 (4) 레벨 상한 60과 40 고정 픽스처 분리 (5) 포인트 = 레벨, 랭크 0 장착 금지 (6) 공유 코드 v5 접두사 (7) 진행 중 런 처리.
시험: 옛 저장 원본 바이트를 먼저 Tests/Fixtures/D4Migration/에 만든 뒤 "옛 저장 X가 정확히 Y로 이전" 시험, 멱등, 스키마 가드, 점진 공개 계정, rules_golden 배분 라운드트립. 관련 스위트만 돌린다. 지문 대상 파일을 고치면 tools/balance_1000.py를 다시 돌린다.
하지 말 것: 저장 키 이름 변경, 자동 재배분, 원본 보관 없는 쓰기, Docs/Design/D4SkillTree/golden 수정.
보고: 바꾼 것, 돌린 검사와 결과, 돌리지 않은 범위, 사용자 결정이 필요한 것(D13~D20 가운데 기본값을 쓴 것).
```

## 6. 단계 3 — 아이콘 92장

**선행**: 0(데이터는 1과 병렬 가능). **브랜치**: `codex/d4-skill-tree-icons`.

**읽을 것**: 계획 11장 → `Docs/Design/D4SkillTree/icons_manifest.json`.

**작업**
1. **입력 확인**: `~/HellscriptOverhaulWork/work/d4skills/<Warrior|Ranger|Mage>/out/`가 있고 `icons_manifest.json`의 sha256과 같은지 확인한다. 없거나 다르면 **중단하고 사용자에게 알린다**(이 폴더는 저장소 밖이다).
2. **대표 먼저**(`AgentRules/Workflow.md`): 보조 스킬 아이콘 한 장(예 `Warrior/W09-1`)을 `Assets/HELLSCRIPT/Resources/Art/ClassSkillIcons/<직업>/<id>.png`(파일 이름 `iconId` → `id`)로 넣고 임포트·예산·화면 표시를 확인한다: `.meta`는 이웃 아이콘과 같은 설정에 새 guid, `ResourceTextureBudget`의 `Art/ClassSkillIcons/` 행(안드로이드 512 ASTC 6×6, 데스크톱 BC7, 웹 ETC2)이 덮는다, `BuildResourceTests` 샘플, `SkillIconView`로 표시. 통과하면 나머지를 직업별 묶음으로 넣는다.
3. **덮어쓰기 둘**: `Ranger/A06.png`(그림자 화살 → 원소 주입), `Mage/M07.png`(잿불창 → 원소창)는 같은 이름의 기존 파일이 있다. **`.meta`의 guid가 그대로인지** 확인하고 함께 커밋한다.
4. **id 해석**: `SkillIconAssets.TryResolve`가 보조 스킬 id(`W09-1`)와 핵심 패시브(`WP20`)를 모른다(`ClassSkills.Find`가 돌려주는 id만 안다). 새 id를 `Art/ClassSkillIcons/<직업>/<id>`로 해석하게 넓히고 시험한다.
5. **출처 기록**: `Docs/Art/ClassSkillIcons/manifest.json`에 92행을 더한다(원래 경로, 새 이름, sha256, `productionApproved=false`, model unknown). 진짜 알파 확인은 `tools/adopt_generated_game_art.py`.
6. **크기**: 텍스처는 새 규칙의 총량이 안드로이드(ASTC 6×6) 약 11MB, **데스크톱(BC7)·웹(ETC2)은 각각 약 24MB**다(512² × 92, 밉맵 없음). 계획 17장 D11의 숫자를 이 값으로 고치고 웹 빌드 크기 영향을 보고한다.

**끝났다는 기준**: 92장이 들어오고 `BuildResourceTests`, 텍스처 예산 시험, `.meta` guid 확인, 대표 한 장의 화면 표시 증거가 있다.

**하지 말 것**: 아이콘을 리사이즈·재생성·배경 제거하지 않는다(계획 16장). 원본 폴더를 지우지 않는다.

**프롬프트**
```
HELLSCRIPT 저장소에서 작업한다. 먼저 AGENTS.md, AgentRules/Workflow.md, AgentRules/Artifact_Cleanup.md와 CLAUDE.md를 읽고 따른다.
브랜치 codex/d4-skill-tree-icons를 최신 origin/main에서 새 worktree로 시작한다.
읽을 문서: Docs/Implementation/D4_Skill_Tree_Step_Briefs_20261012.md 6장, D4_Skill_Tree_Plan_20261012.md 11장, Docs/Design/D4SkillTree/icons_manifest.json.
작업: 단계 3. 입력(~/HellscriptOverhaulWork/work/d4skills/*/out)의 sha256을 icons_manifest.json과 대조하고(다르면 중단 후 보고), 대표 한 장을 먼저 Assets/HELLSCRIPT/Resources/Art/ClassSkillIcons/<직업>/<id>.png로 넣어 임포트·예산·화면 표시를 확인한 다음 나머지 91장을 직업별로 넣는다. Ranger/A06.png와 Mage/M07.png는 기존 파일을 덮어쓰니 .meta guid가 그대로인지 확인한다. SkillIconAssets.TryResolve가 보조 스킬 id와 핵심 패시브 id를 해석하게 넓히고, Docs/Art/ClassSkillIcons/manifest.json에 92행(productionApproved=false)을 더한다.
검증: BuildResourceTests와 텍스처 예산 시험, .meta guid 확인, 대표 한 장의 스크린샷. 전체 EditMode나 스모크는 돌리지 않는다.
하지 말 것: 아이콘 리사이즈·재생성·배경 제거, 원본 폴더 삭제.
보고: 바꾼 것, 돌린 검사와 결과, 텍스처 크기 영향(안드로이드·데스크톱·웹), 돌리지 않은 범위.
```

## 7. 단계 4 — 스킬창 (UI-1 ~ UI-6)

**선행**: 1(UI-3부터 3). **브랜치**: `codex/d4-skill-tree-ui-<n>`. 단계 4는 **여섯 개의 PR**이다. 내용·파일·끝났다는 기준은 **UI 명세 13장**에 있다. 이 장에는 공통 읽을 것과 프롬프트 틀만 둔다.

**읽을 것(UI-1에서 모두, 이후 해당 장)**: UI 명세 0~3장 → `golden/README.md` → 해당 묶음의 장(UI-2: 5장, UI-3: 4·6장, UI-4: 7·8·9장, UI-5: 10·11장, UI-6: 14장) → `research/ui_infrastructure.md`.

**UI-1** `golden/csharp`의 Layout·ViewModel·Session·Fx와 시험 도우미를 옮겨 EditMode 시험 4종(+ 단위)이 정답을 통과하게 한다.
**UI-2** 그리기 도구(`UiMesh` 확장, `PolylineGraphic`, `RankArcGraphic`, `TextGradient`, 셰이더, 색 토큰).
**UI-3** 창 껍데기와 정적 장면(24장 상태 스크린샷, `compare_screens.py` 요소별 점수).
**UI-4** 상세 패널과 머리, 넘침 스모크.
**UI-5** 입력·커밋·연출, 클릭 재생 스모크(60건).
**UI-6** 진입점 연결, `visibleState`, 부모 창 갱신, 최종 눈으로 비교표.

**프롬프트 틀** (`<n>`과 `<장>`을 바꿔 쓴다)
```
HELLSCRIPT 저장소에서 작업한다. 먼저 AGENTS.md, AgentRules/Workflow.md, AgentRules/Shared_UI.md, AgentRules/UI_Refresh.md와 CLAUDE.md를 읽고 따른다(대화·기록 한국어, 코드·주석·커밋·로그 영어).
브랜치 codex/d4-skill-tree-ui-<n>을 최신 origin/main(필요한 이전 PR이 병합된 상태)에서 새 worktree로 시작한다.
읽을 문서(순서): Docs/Implementation/D4_Skill_Tree_UI_Spec_20261012.md 0~3장과 13장의 UI-<n>, 그 묶음의 장(<장>), Docs/Design/D4SkillTree/golden/README.md, research/ui_infrastructure.md.
작업: UI 명세 13장의 UI-<n> 묶음만. 다른 묶음의 일은 하지 않는다.
검증: UI 명세 13장의 UI-<n> "끝났다는 기준"에 적힌 시험과 스모크, python3 tools/check_ui_contract.py --verify, python3 tools/check_ui_refresh.py. 눈으로 비교(14장)는 UI-3·UI-4·UI-6에서 24장 표를 PR에 붙인다. 전체 EditMode와 전체 스모크 묶음은 돌리지 않는다.
하지 말 것: Docs/Design/D4SkillTree/golden, skill_tree_d4.json, SkillWindow.dc.html 수정. 규칙·장면 사각형을 창 코드에서 다시 계산. PC 확대를 localScale로 하기. 색 hex를 코드에 쓰기. 아이콘마다 스텐실 Mask. 허용 근사(A1~A8) 밖의 차이를 남기기.
보고: 바꾼 것, 돌린 검사와 결과(숫자와 비교표), 돌리지 않은 범위, 허용 근사로 남긴 차이와 이유, 사용자 결정이 필요한 것.
```

## 8. 단계 5 — 전투 (5.0, 5a, 5b, 5c)

**선행**: 1, 2(5.0), 이후 5.0(5a~5c). 내용은 **전투 명세 7장**에 있다. **읽을 것**: 계획 8장 → 전투 명세 **전체** → `research/combat_<직업>.md`(해당 직업) → 5.0은 세 직업 모두의 A장(프레임워크).

**5.0 공통 틀** — 전투 명세 2장 전부: v5 `modifier` 필드와 `Loadout.Modifier`, `ClassSkillCast.mod/modRank`, `SkillTreeD4Values` 연결, 랭크 값 읽기, 궁극기 `RankScale`·쿨다운, 정리 규칙(2.4), 영속 허용 목록, 지문·Balance1000 갱신. 끝났다는 기준: 쌍 시험 틀이 A급 보조 스킬 하나로 돌고 기존 전투 시험 결과가 `main`과 같다.

**5a/5b/5c 직업별** — 본 스킬 수치 → 보조 스킬 A·A\* → B → C(전투 명세 8장의 질문 답을 정한 뒤) → 패시브 → 핵심 패시브 → 궁극기. 끝났다는 기준: 보조 스킬 28 + 패시브 10 + 궁극기 둘의 시험(수용 기준 2.8).

**프롬프트 틀** (`<단계>`, `<직업>`)
```
HELLSCRIPT 저장소에서 작업한다. 먼저 AGENTS.md, AgentRules/Workflow.md와 CLAUDE.md를 읽고 따른다(대화·기록 한국어, 코드·주석·커밋·로그 영어).
브랜치 codex/d4-skill-tree-combat-<단계>를 선행 단계가 병합된 최신 origin/main에서 새 worktree로 시작한다.
읽을 문서(순서): Docs/Implementation/D4_Skill_Tree_Step_Briefs_20261012.md 8장, D4_Skill_Tree_Plan_20261012.md 8장, D4_Skill_Tree_Combat_Spec_20261012.md 전체, Docs/Design/D4SkillTree/research/combat_<직업>.md (5.0은 세 직업 파일의 A장).
작업: 전투 명세 7장의 <단계>만. 보조 스킬 하나마다 쌍 시험(보조 없음 vs 있음)을 만들고 수용 기준 2.8을 만족시킨다. C급은 명세 8장의 질문에 대한 기본값 또는 사용자 답을 PR에 적은 뒤 구현한다.
검증: 바꾼 스킬·패시브의 시뮬레이션 시험, 영향받는 기존 전투 스위트, 예보 = 해제 시험. 지문 대상 파일을 고치면 tools/class_skills.py check와 tools/balance_1000.py를 다시 돌린다. 전체 EditMode와 스모크 묶음은 돌리지 않는다.
하지 말 것: 포인트·티어·보조 랭크 규칙을 전투 코드에 다시 쓰기, 보조 스킬을 사용 옵션 체계에 싣기, 시안에 없는 현행 덤 효과를 옮기기, 성능을 실제 이전·이후 바이너리 3회와 CPU·GC 원자료 없이 주장하기, golden/ 수정.
보고: 바꾼 것(스킬별 분류 A/A*/B/C와 시험 이름), 돌린 검사와 결과, 돌리지 않은 범위, 기본값을 쓴 질문 번호, 사용자 결정이 필요한 것.
```

## 9. 단계 6 — 연결부

**선행**: 4(UI-6), 5. **브랜치**: `codex/d4-skill-tree-integration-<n>`. 한 PR에 다 넣지 말고 영역별로 나눈다(최대 3개 병렬). **읽을 것**: 계획 12장 → `research/dependents.md` **전체**(특히 G장 "잊기 쉬운 15곳").

| 묶음 | 내용 | 주요 지점 |
|---|---|---|
| 6a 빠른 편집·HUD | `SkillQuickAvailable`이 `ClassSkillTree.Uses`(v3·v4)에 묶여 v5에서 꺼진다 → 새 `Uses`. `HudSlotState`·`ClassSkillStatus`에 랭크·보조 스킬. 이름·설명에 보조 스킬(`ClassSkillTree.Display`의 `A06-2` 정규식 오류 먼저). 랭크 변경은 같은 규칙 엔진, 막힌 이유도 같은 문구. 궁극기 "Lv.40" → "T5 열림 또는 배움" | `GameUI.SkillQuick.cs`, `HuntEdictQuickMenu`, `GlobalHudSnapshot.cs`, `ClassSkillReadModel.cs`, `ClassSkillDisplay.cs` |
| 6b 튜토리얼·퍼즐 | D7: 가이드 첫 스킬 W09, 2포인트 뒤 W01. 프롤로그 W01/A01/M01 → 직업별 T1(W09·W08, A07·A14, M07). 퍼즐 킷은 예외 지급 API. F06/H07/H08을 "포인트 = 레벨, 티어" 기준으로 다시. 가리키는 요소 이름(`edict-…` → `d4-…`) | `Tutorials*.cs`, `GuideTutorial.cs`, `PuzzleTutorial.cs`, `GameUI.TutorialStaging.cs`, `Resources/PuzzleTutorial.json`, 시험 픽스처 `PuzzleAfterL8-*.json` |
| 6c 칙령·자동 전투 | 사용 옵션 25묶음 정리, 프리셋 211개(옛 id 50개)와 `SkillPresetScenarios.json`(122개, **프리셋마다 시나리오가 정확히 하나** 불변식), `survival.*Skill` 목록의 M05, 추천 칙령, 검증용 빌드 30개의 예외 경로 | `HuntEdictV2.*`, `EdictOptions.cs`, `HuntEdictQuickPresets.*`, `ClassSkillOptions.cs`, `SkillPresetScenario.cs` |
| 6d 도구·문서·문구 | CI `class-skills.yml` 7단계와 지문, `balance_1000.py`, `wiki.py` 기대 개수·`test_wiki.py`, 스킬 문서(한·영), `en.txt`의 옛 키(5286–5336 등), 소리 은행(`bank.json` 30건), 아이템·룬 `requiredSkill`·`legendaryPowers` → `legacyRemap` | `tools/*.py`, `.github/workflows/class-skills.yml`, `Docs/Design/HELLSCRIPT_*_Skills*.md` |

**끝났다는 기준**: `python3 Docs/Design/D4SkillTree/tools/legacy_id_inventory.py --check`의 1~5군 참조가 0이 된다(6군 문서는 해당 문서를 새 체계로 다시 쓴 뒤). 관련 스위트와 생성기 `check`가 통과한다.

**프롬프트 틀**
```
HELLSCRIPT 저장소에서 작업한다. 먼저 AGENTS.md, AgentRules/Workflow.md와 CLAUDE.md를 읽고 따른다.
브랜치 codex/d4-skill-tree-integration-<n>을 선행 단계가 병합된 최신 origin/main에서 새 worktree로 시작한다.
읽을 문서: Docs/Implementation/D4_Skill_Tree_Step_Briefs_20261012.md 9장의 <묶음>, D4_Skill_Tree_Plan_20261012.md 12장, Docs/Design/D4SkillTree/research/dependents.md 전체.
작업: 9장의 <묶음>만. 옛 id를 가리키는 곳은 legacyRemap대로 옮기거나 지운다. 구현 전에 grep과 legacy_id_inventory.py로 대상을 다시 센다.
검증: 관련 스위트, 생성기 check, python3 Docs/Design/D4SkillTree/tools/legacy_id_inventory.py(남은 참조 보고). 전체 EditMode와 스모크 묶음은 돌리지 않는다.
하지 말 것: 규칙을 따로 쓰기, 저장 키 이름 변경, 검사 목록을 통과용으로 넓히기, 옛 화면 삭제(7단계 일).
보고: 바꾼 것, 돌린 검사와 결과, 남은 옛 id 참조 수, 돌리지 않은 범위, 사용자 결정이 필요한 것.
```

## 10. 단계 7 — 전환

**선행**: 1~6. **브랜치**: `codex/d4-skill-tree-switch`.

**작업**: `SkillTreeD4.Enabled` 기본값을 켠다. 옛 트리 화면·데이터 경로를 정리한다(`HuntEdictWindow.Skills.cs`의 트리·인스펙터, `SkillTreeGraphic`의 옛 배치, `SkillTreeLayout`, `Prototypes/SkillTree/*`, `RuntimeSkillTreeSmoke*.cs`, 옛 `ClassSkillTree.json`·아트, 호출 없는 `GameStore.CommitClassSkills` 등: `research/dependents.md` A장 "삭제"). 그 뒤 **`main`을 통합**하고, **전체 EditMode 1회 + macOS 개발 빌드 스모크 1회**(새 `-hellscriptSkillTreeD4Smoke`와 이번 변경에 걸리는 기존 스모크: SkillTree, ClassSkill, BattleQuickEdit, Tutorial, GuideTutorial, 퍼즐)를 돌린다. 배치모드가 `ProjectSettings.asset`·`UnityConnectSettings.asset`을 바꾸면 이번 실행으로 생긴 변경만 되돌린다. 병합한 뒤 위키 게시(`AgentRules/Wiki_Publish.md`).

**끝났다는 기준**: 계획 13장 전부. 확인하지 못한 화면과 실기기 미검증 범위를 따로 적는다.

**프롬프트**
```
HELLSCRIPT 저장소에서 작업한다. 먼저 AGENTS.md, AgentRules/Workflow.md, AgentRules/Wiki_Publish.md, AgentRules/Artifact_Cleanup.md와 CLAUDE.md를 읽고 따른다.
브랜치 codex/d4-skill-tree-switch를 단계 1~6이 병합된 최신 origin/main에서 새 worktree로 시작한다.
읽을 문서: Docs/Implementation/D4_Skill_Tree_Step_Briefs_20261012.md 10장, D4_Skill_Tree_Plan_20261012.md 13·14장, research/dependents.md A장의 "삭제" 목록.
작업: 단계 7. SkillTreeD4.Enabled 기본값 켜기, 옛 트리 화면·데이터 경로 정리, main 통합, 전체 EditMode 1회 + macOS 개발 빌드 스모크 1회(-hellscriptSkillTreeD4Smoke 포함), 병합 뒤 위키 게시.
검증: 계획 13장 전부. 같은 조건의 main 결과와 비교해 새 실패만 새 실패로 보고한다.
하지 말 것: 마지막 코드 변경 전의 전체 검사 결과를 재사용하기, macOS 결과를 실기기 결과로 보고하기.
보고: 결과 표, 확인하지 못한 화면, 실기기 미검증 범위, 산출물 보존·정리 상태.
```
