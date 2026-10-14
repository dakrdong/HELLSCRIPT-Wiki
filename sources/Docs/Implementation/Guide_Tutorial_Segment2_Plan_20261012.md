# 안내 튜토리얼 2구간 구현 계획 (Codex 전달용)

갱신일: 2026-10-12

작성: Claude (계획만). 구현·검증·커밋은 Codex가 한다. 구현이 끝나면 이 문서를 결과 기록으로 고치고 영어판(`.en.md`)을 함께 만든다.

## 1. 목표

안내 튜토리얼(흐름 6, `GuideTutorial`)의 1구간은 main에 있다. 영웅이 회오리로 포위한 무리를 쓸어낸 직후 마을로 졸업한다. 그 사이에 **2구간**을 넣는다. 2구간에서는 원거리 적을 만나 사냥 칙령의 **첫 교전 위치**와 **전투 중 위치**를 전투 HUD에서 직접 바꾼다. 2구간이 끝나야 지금의 마무리 대사와 마을 졸업으로 넘어간다.

현재 상태와 바탕 문서:
- 1구간 대본·구현 기록: `Docs/Implementation/Guide_Tutorial_Script_20261011.md`
- HUD 시트 기록: `Docs/Implementation/Edict_Hud_Sheet_20261011.md`
- 사용자가 준 화면 그림 3장(아래 4절의 UI 경로): 사냥칙령 버튼(1) → 전투 아이콘(2) → "전투 위치 거리"(3) → "첫 교전 위치" 탭 → "즉시 접근"(접근 방식 "걷기 이동"이 이미 선택됨), 그다음 "전투 중 위치" 탭 → "무리 중심".

## 2. 반드시 지킬 것 (사용자 요청)

- 벨리나는 **존댓말**로, 영웅이 아니라 **플레이어에게** 말한다. 영웅은 "영웅" 또는 "당신이 지켜보는 이 영웅"이다.
- 대사에서 설정을 설명하지 않는다. 눈 가림, 시간을 멈춘다·붙잡아 둔다 같은 말은 쓰지 않는다. 화면의 옅은 푸른 테두리(정지 표시)는 그대로 둔다.
- '즉시 접근'을 고르면 하위 옵션 **'걷기 이동'이 미리 선택된 상태**여야 한다. **'스킬 사용'은 대사·안내에서 언급하지 않는다**(다음 튜토리얼에서 가르친다). 패널에 라디오 두 개가 보이는 것은 괜찮다.
- 새 UI를 만들지 않는다. main에 이미 있는 HUD 사냥칙령 시트와 위치 패널(`GameUI.EdictSheet.cs`, `GameUI.EdictPosition.cs`)을 그대로 쓰고, 1구간 회오리 수업처럼 가림막(`PrologueGate`)으로 한 단계씩 안내한다.
- 게임 문구는 모두 `Assets/HELLSCRIPT/Resources/Localization/en.txt`에 한국어 키와 영어를 함께 넣는다.
- 작업 규칙은 `AGENTS.md`, `CLAUDE.md`, `AgentRules/Workflow.md`, `AgentRules/Wiki_Publish.md`를 따른다. 최신 `origin/main`에서 별도 worktree를 만들어 작업한다(공유 체크아웃에서 브랜치를 바꾸지 않는다).

## 3. 진행 순서와 대사

기존 단계 `Whirlwind` 뒤에 아래 단계를 넣고, 기존 `Cleared`(마무리 대사 → 마을 졸업)는 맨 끝으로 옮긴다.

| 단계(제안 이름) | 상황 | 시간 | 내용 |
| --- | --- | --- | --- |
| `Whirlwind`(기존) | 회오리로 포위 무리 처치 | 흐름 | 끝나면 `Cleared` 대신 `Road`로 간다. 이때 말풍선(Bark, 멈추지 않음): "보셨나요? 영웅이 당신이 가르친 그대로 움직였습니다." |
| `Road` | 영웅이 첫 투기장 위쪽 문 → 꺾인 통로 → 긴 회랑으로 걸어간다 | 흐름 | 회랑에 원거리 적 무리가 기다린다 |
| `OpeningTalk` | 영웅이 원거리 적을 처음 발견한 순간 | 정지 | ① 적이 나타났어요. 적과 처음 마주쳤을 때 어떻게 대응할지 정해 볼까요? ② 전사는 공격 거리가 짧기 때문에, 적에게 붙어서 싸우는 것이 중요합니다. → [첫 교전 위치를 정한다] |
| (수업 A) | 가림막 안내: 사냥칙령 버튼 → 전투 아이콘 → (목록이면) 전투 위치 거리 → 첫 교전 위치 탭 → **즉시 접근** | 정지 | 단계별 안내 문구는 4절 |
| `Approach` | 시간이 흐르고 영웅이 걸어서 원거리 적에게 붙는다 | 흐름 | 붙어서 **회오리를 한 번 쓴 직후** 다음 단계로 |
| `CombatTalk` | 회오리 첫 사용 직후 | 정지 | ① 적과 마주친 뒤에도, 전투 중에 어디에서 싸울지 정할 수 있습니다. → [전투 중 위치를 정한다] |
| (수업 B) | 가림막 안내: 사냥칙령 버튼 → 전투 아이콘 → (목록이면) 전투 위치 거리 → 전투 중 위치 탭 → **무리 중심** | 정지 | 4절 |
| (수업 B 맺음) | 무리 중심 저장 직후 | 정지 | ② 이렇게 하면 회오리가 적을 좀 더 효율적으로 공격할 수 있습니다. → [확인] |
| `FinalFight` | 시간이 흐르고 남은 원거리 적 처치 | 흐름 | 모두 쓰러지면 `Cleared` |
| `Cleared`(기존) | 기존 마무리 대사 3줄 → [마을로 향한다] → 졸업 | 정지 | 변경 없음 |

대사는 위 문장을 기본으로 쓰고, 다듬더라도 2절 규칙을 지킨다. 영어 초안:
- "Did you see? The hero moved exactly as you taught."
- "An enemy has appeared. Shall we decide how to respond when you first meet an enemy?"
- "A warrior's attack reach is short, so it matters to fight up close to the enemy."
- "Even after meeting the enemy, you can decide where to fight during combat."
- "This way, Whirlwind can strike the enemies more efficiently."
- 선택지: 첫 교전 위치를 정한다 / Set the first engagement · 전투 중 위치를 정한다 / Set the combat position · 확인 / OK

## 4. HUD 수업 안내 (가림막 단계)

1구간의 `TickGuideLesson`/`GuideLessonStep`(`GameUI.GuideTutorial.cs`)을 일반화해 수업 종류(Skill / Opening / Combat)마다 다음 목표 하나를 고른다. 매 프레임 **저장된 영웅의 칙령 값**으로 진행을 판단해서, 다시 그려지거나 창을 닫았다 열어도 이어지게 한다.

| 순서 | 조건 | 가림막 대상(이름) | 안내 문구(존댓말) |
| --- | --- | --- | --- |
| 1 | 칙령 펼침이 닫혀 있음(`quickRoot==null` 또는 `quickKind!="edict"`) | `globalHud.EdictButton` | 사냥 칙령 버튼을 눌러 주세요. |
| 2 | 펼침은 열렸고 시트가 없음 | 버튼 `edict-sheet-tab-combat` | 전투 아이콘을 눌러 주세요. |
| 3 | 시트가 목록(그룹 칩)으로 열림 | 위치 그룹 칩 `edict-sheet-group-{위치 그룹 index}` | '전투 위치 거리'를 눌러 주세요. |
| 4 | 위치 패널인데 탭이 다름 | `edict-position-tab-opening` / `edict-position-tab-combat` | '첫 교전 위치'(또는 '전투 중 위치')를 눌러 주세요. |
| 5 | 위치 패널, 맞는 탭 | `edict-sheet-choice-position.engage-APPROACH` / `edict-sheet-choice-position.mode-CENTER` | '즉시 접근'을 눌러 주세요. / '무리 중심'을 눌러 주세요. |
| 완료 | 저장값 `position.engage==APPROACH`(A) / `position.mode==CENTER`(B) | — | 가림막을 내리고 `CloseEdictQuick(false)`. A는 `game.ContinueTutorial()`, B는 맺음 대화를 연 뒤 그 선택지에서 `ContinueTutorial()` |

참고:
- 지금 main에서는 전투 탭을 열면 첫 그룹(전투 위치 거리)이면 위치 패널로 바로 들어간다(`BuildEdictSheet`의 `sheetPosition`). 3단계는 목록이 보일 때만 쓰이게 이름으로 찾는다.
- 대상 찾기는 1구간의 `Named(root,name)`(활성·interactable 버튼만)을 재사용한다. 펼침 아이콘은 `quickRoot` 아래, 시트·패널 버튼은 `sheetCard` 아래에 있다.
- 가림막 Canvas는 2000이라 HUD(105)·빠른 편집(107)·패널(108) 위에서 구멍만 통과시킨다. 1구간 회오리 수업이 같은 방식으로 이미 동작한다.
- 튜토리얼 중 칙령 편집은 `QuickLiveRun`(PR #91)으로 진행 중인 튜토리얼 전투에 바로 들어간다(`CommitHuntEdict(..., game.Combat, ...)`). 따로 저장 경로를 만들지 않는다.
- 수업 중에는 다른 칙령 탭이나 옵션을 가림막이 막는다. 수업이 끝난 뒤 플레이어가 다른 값을 바꾸는 것은 막지 않는다.

## 5. 파일별 작업

### `Assets/HELLSCRIPT/Runtime/Core/GuideTutorial.cs`
- 단계 상수를 다시 매긴다. 예: `Entrance0 Intro1 Walk2 FirstTalk3 FirstFight4 Surrounded5 Whirlwind6 Road7 OpeningTalk8 Approach9 CombatTalk10 FinalFight11 Cleared12`. `Waiting()`에 `OpeningTalk`, `CombatTalk`을 더한다. `public const int Last=Cleared;`을 둔다.
- 2구간 길(`Road2` 같은 waypoint 배열): 첫 투기장 안쪽 → 위쪽 문 `(30,14)` → 통로 꺾임 `(31,19)`, `(37,23)` → 회랑 왼쪽 문 `(40,24)` → 회랑 안 `(44,24)`. 지금 레이아웃(`Layout()`)의 방·통로 좌표를 그대로 쓴다.
- 원거리 적 무리: 필드 0 명단의 슬롯 2(`EnemyCombat.RosterKind(0,2)` = kind 2, 사거리 9 투사체)를 3마리, 회랑 `(54,21)`, `(56,24)`, `(54,27)` 근처. 체력·공격은 상수로 둔다. 시작값은 체력 140·공격 2를 제안한다. 첫 회오리 한 번에 다 죽지 않아야 하고(그래야 `CombatTalk`이 의미 있음), 영웅은 쓰러지지 않아야 한다. 테스트로 맞춘다.
- `BarePowers`: 스킬을 비우는 것에 더해 칙령 값 두 개를 미리 정한다. `position.engage="STAND"`(제자리에서 맞이: 원거리 적에게 맞으며 서 있게 해서 '즉시 접근'을 고를 이유를 만든다), `position.engageApproach="WALK"`(걷기 이동 미리 선택). `HuntEdictV2Editing.WithGlobal(d.edict,id,value)`로 바꾸고 `legacyEdict` 복사는 그 뒤에 한다.
  - 주의: 첫 교전이 `STAND`가 되면 1구간 첫 전투에서 영웅이 적을 감지한(12m) 자리에 서서 맞이한다. 첫 적은 근접이라 다가오므로 흐름은 유지되지만, `GuideTutorialTests`의 "첫 타격 때 x>21" 같은 위치 단언은 다시 맞춰야 할 수 있다.
- 수업 완료 판단 함수: `OpeningDone(edict)=>engage=="APPROACH"`, `CombatDone(edict)=>mode=="CENTER"`. `HuntEdictSummary.Value(HuntEdictDefaults.Resolve(hero), id)` 또는 시뮬레이션 쪽 값을 쓴다.

### `Assets/HELLSCRIPT/Runtime/Core/CombatSimulation.Guide.cs`
- 원거리 무리는 처음부터 소환해 두되(`SpawnGuide`), 1구간의 첫 적처럼 `lastSeenTime=-100`으로 시작한다. 0이면 "방금 봤음"으로 처리돼 맵 원점으로 걸어간다(1구간에서 실제로 겪은 함정). `EnemyState.group`으로 구분한다(첫 적 0, 포위 1, 원거리 2 등). 1구간 단계의 승리 조건이 원거리 무리를 세지 않게, `Whirlwind` 완료 판정은 포위 그룹만 본다.
- `GuideOutcome`:
  - `Whirlwind`: 포위 그룹이 모두 쓰러지면 `Road`(멈추지 않음). `Finish`를 부르지 않는다.
  - `Road`: 원거리 적 하나라도 영웅 기준 12.5m 안에 있고 `Map.LineClear`이면 `OpeningTalk` + `paused=true`. 영웅 감지 거리(12m)와 첫 교전 STAND 때문에 영웅이 12m에서 멈출 수 있으니 판정 거리를 12m보다 약간 크게 잡는다.
  - `Approach`: 이 단계에서 회오리(`W01`)가 처음 시작·피해를 준 직후(예: 영웅이 준 피해 이벤트 중 정의가 `W01`이고 시간이 단계 시작 이후) `CombatTalk` + `paused=true`. 레거시 `lastSkillStarts`는 카탈로그 스킬 기준이라 클래스 스킬 판정에 쓰지 않는다. 기존 피해 기록(`State.damageEvents`, `definitionId`)이나 영웅 행동 상태에서 실제로 확인되는 값을 고른다.
  - `FinalFight`: 남은 적이 모두 쓰러지면 기존처럼 `Cleared` + `Finish(true, ...)` + `paused=true`.
  - 영웅 체력 0 보호(다시 채움)와 전투 단계의 `lastSeen` 갱신은 1구간 방식을 유지하되, 갱신은 해당 단계의 그룹에만 한다(`Road`에서는 하지 않는다. 미리 달려오면 안 됨).
- `MoveGuideWalk`: `Road` 단계에서도 대상이 없을 때 2구간 길을 걷는다. waypoint 인덱스는 `State.guideWaypoint`를 재사용하되 2구간 시작 때 0으로 되돌리거나 별도 필드를 둔다.
- `ContinueGuide`: `OpeningTalk`는 `OpeningDone`일 때만 `Approach`, `CombatTalk`는 `CombatDone`일 때만 `FinalFight`. 조건이 안 되면 false(1구간 회오리 수업과 같은 방식).

### `Assets/HELLSCRIPT/Runtime/Core/Tutorials.cs`
- `NormalizeShare`의 체크포인트 검사에서 `r.tutorialPhase>ProloguePhase.Last`가 흐름 6에도 적용된다. `ProloguePhase.Last`는 11이라 `Cleared=12`가 되면 저장 때마다 "Invalid tutorial checkpoint" 예외가 난다. 흐름 6이면 `GuideTutorial.Last`로 검사한다. `Normalize`는 저장할 때마다 돌기 때문에 진행 중인 런을 지우거나 막는 규칙을 넣으면 안 된다(1구간에서 실제로 겪은 함정).

### `Assets/HELLSCRIPT/Runtime/Core/HuntEdictProgression.cs`
- 새 계정은 위치 옵션이 균열 5단계 규칙(`E03`)에 묶여 숨겨져 있다. `OpeningTalk`에 들어갈 때 `position.engage`, `position.engageApproach`, `position.mode`만 계정에 공개한다(아래 GameStore 거래).
- `Direct(...)`의 직접 편집 허용 목록에 `position.engage`, `position.engageApproach`를 더한다. `position.mode`는 이미 있다. 이게 없으면 위치 패널 선택 버튼이 `interactable=false`가 된다(`GameUI.EdictPosition.cs`).
- `ValidateChange`가 두 옵션 변경을 막지 않는지 확인한다.

### `Assets/HELLSCRIPT/Runtime/Core/GameStore.Guide.cs`
- `UnlockGuideEdict(RunState run, params string[] ids)` 거래를 추가한다. 기존 `UnlockPitMovement`(`GameStore.Tutorials.cs`)를 본뜬다.
  - 조건: `run.tutorialFlowVersion==GuideTutorial.Version`, 리플레이 아님, `run==Data.guide.tutorialRun`, `Tutorials.Mandatory(Data)`.
  - `a.guide.edictUnlocks`에 id를 추가하고, 거래 뒤 반드시 `Data.guide.tutorialRun=run;`으로 되돌린다. `Transact`가 `Data.guide`를 스테이징 복사본으로 바꿔서, 그대로 두면 `CommitHuntEdict`의 "prologue" 판정(`Data.guide.tutorialRun==simulation.State`)이 깨진다.
- 졸업(`CompleteGuideTutorial`)은 `Cleared`가 맨 끝으로 옮겨진 것 외에는 바꾸지 않는다.

### `Assets/HELLSCRIPT/Runtime/Presentation/GameUI.GuideTutorial.cs`
- `ShowGuideBeat`에 `OpeningTalk`, `CombatTalk` 대사와 선택지를 추가한다. `OpeningTalk`를 열 때 `game.Store.UnlockGuideEdict(run, ...)`를 먼저 부른다(이미 공개돼 있으면 그냥 통과).
- 수업 상태를 `guideLesson`(bool) 대신 종류로 들고 있게 바꾼다. 예: `"skill" | "opening" | "combat" | ""`. `GuideLessonActive`는 회오리 수업에만 쓰이므로(HUD 스킬 목록 허용 조건) 의미를 유지한다.
- 수업 B 완료 뒤 맺음 대화(한 줄 + [확인])를 연다. [확인]에서 `ContinueTutorial()`을 부르고, 같은 단계에서 대화가 다시 열리지 않게 `tutorialPromptPhase`를 다룬다.
- `Whirlwind` 종료 말풍선은 1구간 회오리 시작 말풍선처럼 `cinematic.Bark(GuideGoddess, ...)`를 쓴다.
- `GuideObjective`에 새 단계 목표를 넣는다. 예: 목표 · 길을 따라 전진 / 목표 · 첫 교전 위치 정하기 / 목표 · 원거리 적에게 접근 / 목표 · 전투 중 위치 정하기 / 목표 · 남은 적 처치.

### 그 외
- `en.txt`: 새 문구 전부(대사, 선택지, 수업 안내, 목표, 말풍선).
- 문서: 이 계획서를 결과 기록으로 고치거나 `Guide_Tutorial_Script_20261011(.en).md`에 2구간 절을 더한다. 한국어·영어 둘 다. 날짜 표기는 `갱신일:` / `Updated:`로 시작한다(위키가 그 줄로 날짜를 읽는다).

## 6. 테스트와 검증

작업 중에는 관련 검사만, 마지막 코드 변경과 `main` 통합 뒤에 전체 Edit Mode와 macOS 스모크를 한 번 돌린다(`CLAUDE.md`).

- Edit Mode `GuideTutorialTests`(`Assets/HELLSCRIPT/Tests/Editor/GuideTutorialTests.cs`) 확장:
  - 회오리 뒤 졸업하지 않고 `Road`로 가는지, 원거리 적을 보면 `OpeningTalk`에서 멈추는지.
  - `OpeningTalk`에서 `engage`를 바꾸기 전에는 `ContinueTutorial()`이 false인지. `engageApproach`가 처음부터 `WALK`인지.
  - 공개 거래 뒤 `HuntEdictProgression.Visible/Direct`가 세 옵션에서 true인지, 그리고 `CommitHuntEdict(session, catalog, sim, true)`로 `APPROACH`를 저장할 수 있는지.
  - 영웅이 걸어서(이동 스킬 없이) 원거리 적에게 붙고, 회오리 첫 사용 뒤 `CombatTalk`에서 멈추는지.
  - `CENTER` 저장 전에는 계속이 막히고, 저장 뒤 `FinalFight` → `Cleared` → 졸업까지 가는지. 기존 졸업 단언(모든 기능 개방 등)도 유지.
  - 체크포인트 정규화: 단계 12가 들어간 저장이 `Tutorials.Normalize`를 통과하는지.
- 관련 묶음: `HuntEdictProgressionTests`, `HuntEdictHudRowsTests`, `HuntEdictQuickMenuTests`, `LocalizationTests`, `StoredLocalizationTests`, `TutorialHeroSlotsTests` 등.
- `python3 tools/check_ui_contract.py --verify`, `python3 tools/check_ui_refresh.py`.
- 런타임 스모크 `RuntimeGuideTutorialSmoke`(`Assets/HELLSCRIPT/Runtime/Presentation/RuntimeGuideTutorialSmoke.cs`, 플래그 `-hellscriptGuideTutorialSmoke -hellscriptOfflineQa`): 1구간 뒤에 2구간을 이어서 실제 포인터로 가림막을 따라간다(1구간 수업 루프와 같은 방식). 단계별 캡처, 벨리나 화자·초상 확인, 영어에서 한글 없음 확인. 최소 440×956 한국어와 956×440 영어. 스크린샷은 직접 확인한다.
- 빌드·스모크 방법은 기존 기록대로 Unity 복제본에서 배치 빌드한다. 인자는 배열로 넘긴다.

## 7. 알려진 함정 (1구간에서 실제로 겪은 것)

- 적의 `lastSeenTime` 기본값 0은 "방금 봤음"으로 처리돼 맵 원점으로 이동한다. 소환할 때 -100으로 둔다.
- `Tutorials.Normalize`는 저장할 때마다 돈다. 체크포인트를 지우거나 범위를 벗어나면 진행 중인 런이 사라지거나 저장이 실패한다.
- `Transact` 뒤 `Data.guide`가 바뀌므로 `Data.guide.tutorialRun`을 살아 있는 런으로 다시 묶는다.
- 튜토리얼 런은 `training=3`이라 `QuickBattleEditable`은 false다. 튜토리얼 편집 허용은 `QuickLiveRun`이 담당한다.
- 칙령 펼침이 열려 있으면 반투명 배경이 사냥칙령 버튼을 덮는다. 닫기는 배경 탭(`edict-quick-backdrop`)이다.
- 이미 졸업한 계정에서는 튜토리얼이 나오지 않는다. Unity에서 확인하려면 메뉴 HELLSCRIPT → 테스트 → 튜토리얼 계정 초기화.

## 8. 하지 말 것

- '스킬 사용' 접근 방식을 대사나 안내에서 언급하지 않는다.
- 위치 패널·칙령 시트를 새로 만들거나 다시 디자인하지 않는다.
- 숨김 규칙을 통째로 열지 않는다(`edictLegacyAccess`나 `Details` 부여 금지). 수업에 필요한 세 옵션만 공개한다.
- 1구간 대사·흐름을 바꾸지 않는다(회오리 뒤 졸업이 2구간으로 미뤄지는 것만 바뀐다).
