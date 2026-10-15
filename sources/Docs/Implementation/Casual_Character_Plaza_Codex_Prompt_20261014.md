# 캐릭터 생성·선택 화면(마을 장면) — Codex 작업 지시

작성일: 2026-10-14 · [English](Casual_Character_Plaza_Codex_Prompt_20261014.en.md)

아래 코드 블록 전체를 Codex에 그대로 붙여 넣는다. 시안·그림·도구는 `claude/casual-art-concept` 브랜치(원격에 올라가 있음, 이 저장소의 로컬 작업 폴더는 `/Users/t8g-2410-pn-005/Documents/MyProject/HELLSCRIPT/wt-casual-art`)에 있다.

| 전달할 것 | 위치 |
|---|---|
| 디자인 페이지(28장, 비공개) | https://claude.ai/artifact/M43Cy6wg7BJUqpKxofu21M — Codex가 열려면 사용자가 공유해야 한다. 열 수 없어도 아래 저장소 사본으로 충분하다 |
| 시안 정본(원본 소스·참조 화면 28장·규격·문구·검사 도구) | `Docs/Design/CharacterPlaza/` ([README](../Design/CharacterPlaza/README.md)) |
| 아트 원본·프롬프트·장면 분리 도구 | `Docs/Art/CasualConcept/2026-10-13/CharacterPlaza/` ([README](../Art/CasualConcept/2026-10-13/CharacterPlaza/README.md)) |
| 이 지시 | 이 문서 |

**확정된 결정:** 결정 1(저장 구조)은 사용자가 2026-10-14에 **옵션 A, 영웅 6명 저장**으로 정했다. 아래 "결정" 절의 나머지 기본값도 바꾸고 싶으면 Codex에 알려 주면 된다.

```text
HELLSCRIPT의 캐릭터 선택 화면(페이지 "characters")을 새 "캐릭터 생성·선택 화면"으로 바꿔 줘. 로그인(또는 게스트 입장) 뒤 한 화면에서 아직 안 만든 캐릭터를 만들고, 만든 캐릭터를 골라 입장한다. 그림은 이미 있다. 이번 작업은 화면·상태 규칙·문구·검증을 게임에 넣는 것이다. 저장 구조는 사용자가 확정한 "결정 1"(영웅 6명 저장)을 따른다.

## 배경
- 게임 아트가 어두운 고딕에서 밝은 캐주얼(스타일 D, 12세 이상)로 바뀐다. 직업마다 성별이 둘이 되어 영웅이 여섯이다(전사·궁수·마법사 각 남녀). 닉네임 입력은 없다(닉네임은 나중에 게임 진행 중 튜토리얼에서 만든다).
- 던전 앤 드래곤 아케이드의 캐릭터 선택에 대한 오마주다. 모험을 떠나기 전 파티가 마을에 모여 있고, 각자 그럴듯한 장소에서 그럴듯한 행동을 한다(전사는 술집 테라스에서 술잔과 닭다리, 궁수는 과일 가게에서 흥정, 마법사는 마법 상점에서 책과 물약). 사용자가 한 번 "서 있는 자리가 억지스럽다"고 되돌렸으므로 시안의 장면을 그대로 따른다.
- 컨셉 기록: Docs/Art/CasualConcept/2026-10-13/Casual_Art_Concept_20261013(.en).md (이 브랜치의 "캐릭터 생성·선택 화면: 마을 장면" 절).

## 먼저 할 것
1. 규칙: AGENTS.md, CLAUDE.md, AgentRules/(Shared_UI, UI_Refresh, Workflow, CPU_Performance, Unity_MCP, Wiki_Publish, Equipment_Comparison). 한 작업·한 에이전트, 대화·개발 기록은 한국어, 코드 주석·커밋 메시지·로그는 영어.
2. 최신 origin/main에서 새 브랜치를 worktree로 만든다(다른 세션이 같은 체크아웃을 쓴다). PR 1(저장 구조)은 codex/casual-hero-slots, PR 2(새 화면)는 codex/casual-character-plaza 로 하고, PR 2 는 PR 1 위에서 시작하거나 PR 1 병합 뒤 main 에서 시작한다. 시안과 아트는 PR 2 브랜치에 가져온다:
   git fetch origin claude/casual-art-concept && git checkout origin/claude/casual-art-concept -- Docs/Design/CharacterPlaza Docs/Art/CasualConcept/2026-10-13/CharacterPlaza Docs/Art/CasualConcept/2026-10-13/Casual_Art_Concept_20261013.md Docs/Art/CasualConcept/2026-10-13/Casual_Art_Concept_20261013.en.md
   (main은 위 두 컨셉 문서를 건드리지 않았다. 그 브랜치는 main에 깔끔히 병합된다.)
3. Docs/Design/CharacterPlaza/README.md 를 끝까지 읽고 reference/ 의 참조 화면 28장을 열어 본다. 화면 규칙·규격·문구·상태 전환은 그 문서가 정본이고, 아래는 요약이다. node Docs/Design/CharacterPlaza/tools/smoke.js 가 시안의 상태 흐름 검사다(게임 테스트로 옮길 흐름의 원본).

## 확인한 현재 상태 (main 305744d3 기준, 직접 읽은 코드)
- 화면: Presentation/GameUI.CharacterSelection.cs(ShowCharacterSelection, 페이지 id "characters", 배경 TitleAtmosphere + Art/CharacterSelection/SelectionCourtyard), CharacterSelectionView.cs, CharacterSelectionStage.cs(세 영웅의 이동·자세·대기), CharacterFigure.cs(투명 이미지 클릭 마스크), 상태 Core/CharacterSelectionState.cs(미리 보기, 균열 소유 잠금 LockedHero), 확정 Presentation/GameController.CharacterSelection.cs(CommitEntryCharacter, EnterFirstStart, SaveEntryCharacter). 아트 Resources/Art/CharacterSelection/(Warrior·Mage·Ranger 아틀라스 + SelectionCourtyard.png), 검사 tools/inspect_character_atlases.py, 근거 Docs/Implementation/CharacterSelectionEvidence/, 스모크 -hellscriptCharacterSelectionSmoke(11 checks), 테스트 Tests/Editor/CharacterSelectionTests.cs, 설명 Docs/Implementation/Character_Selection(.en).md.
- 첫 시작은 이 화면을 건너뛴다: GameUI.OpenCharacterSelection 이 GuideTutorial.FirstStart 이면 EnterFirstStart 로 전사를 바로 입장시킨다.
- 저장: AccountSave.heroes 는 정확히 3명이다(GameStore.cs:66 의 검증이 아니면 불러오기를 거부). HeroClass { Warrior, Ranger, Mage } 이며 영웅 인덱스를 직업 인덱스처럼 쓰는 곳이 많다(selectedHero 245곳·146파일, heroes[ ]/heroes.Count 150곳·92파일, 직업 인덱스 캐스트 218곳·124파일). 영웅 id(HeroSave.id)는 튜토리얼 슬롯(Tutorials.Switch/Take/Park)과 진행 중 균열(suspendedRun.heroId)을 구분한다. HeroSave 에는 성별도 "만들었는지" 표시도 없다. 아트 이름의 archer 는 코드의 HeroClass.Ranger 이다. 새 직업이나 저장 ID 를 만들지 마라.
- 장비: 영웅별 HeroSave.inventory 의 equipped 아이템. 공통 부품 CharacterEquipmentView.Create(parent, HeroSave hero, ...), EquipmentSlotView, ItemDetailView. 주의: CharacterEquipmentView 의 영웅 그림은 예전 어두운 직업 아틀라스(CharacterFigure.Load(heroClass))라서 이 팝업에 그대로 넣으면 새 화풍과 어긋난다.
- 터치 대상은 44pt 를 지킨다(커밋 9697bd9e). 시안의 + 단추(32·38px)와 사람 단추(28·32px)는 그보다 작으므로 보이는 크기는 두고 눌리는 영역만 44pt 이상으로 넓힌다.
- 문구는 한국어 원문이 키이고 Resources/Localization/en.txt 가 탭으로 구분한 영어다. 누락 키는 한국어로 대신 표시하고 기록하며 빈 문자열이나 키 이름을 화면에 내지 않는다.

## 화면 요구 (시안 요약, 자세한 숫자는 README)
- 장면: 영웅 여섯이 한 장의 마을 장면 속 자기 자리에 있다. 앞쪽 술집 테라스에 전사 남·여, 가운데 과일 가게에 궁수 남·여, 뒤쪽 분수 옆 마법 상점에 마법사 남·여. 가로(956×440)·PC(1152×648)는 장면 전체를 한 화면에 덮어 맞춘다. 세로(440×956)는 세로로 다시 그린 장면을 쓰고, 오른쪽 인장 세 개(위부터 마법사·궁수·전사, 65px, 화면 높이 가운데, 112px 간격)를 누르면 카메라가 그 직업 두 명 쪽으로 이동하며 확대한다(0.9초). 영웅을 고르면 카메라가 그 직업으로 따라간다.
- 상단에는 "‹ 타이틀"과 제목 "나만의 모험가를 골라요"만 있다. 연출 켜짐·언어 버튼은 이 화면에 두지 않는다(사용자 결정: 게임에서 연출은 항상 켜짐, 언어는 설정에서 변경). 이 화면은 TitleSession.MotionEnabled 를 읽지 않고 항상 재생한다. 다른 화면의 두 버튼은 이번 범위가 아니다.
- 선택하지 않은 영웅은 비활성(채도 .45·밝기 .88)으로 보인다. 누르면 색이 돌아오고 흰 외곽선과 직업 색 빛이 켜지며, 발밑에 직업 이름표(직업명 + 성별 기호)가 붙는다.
- 아직 안 만든 영웅을 고르면 머리 위에 초록 + 단추. 누르면 그림 속 영웅을 배경 조각으로 가리고 화이팅 자세(cheer_*)를 세워 1.8초 동안 영웅 성격 대사를 말풍선으로 보인다. 끝나면 원래 자세로 돌아가며 생성이 끝나고 머리 위에 Lv.1 알약과 사람 아이콘이 생긴다. 사람 아이콘은 착용 장비 팝업(무기·머리·가슴·손·발·허리띠·목걸이·반지 8칸)을 연다. 팝업의 장비는 그 영웅이 실제로 착용한 것이며 공통 부품으로 그린다.
- 만든 영웅을 고르면 하단 가운데에 초록 "플레이". 누르면 영웅이 전투 준비 자세(fight_*, 착용 무기)로 바뀌어 성격 대사를 말하고, 플레이 버튼은 "입장 중…"으로 비활성이 되며 기존 입장(CommitEntryCharacter → EnterPlaza(true))으로 이어진다.
- 생성·입장 중에는 다른 영웅과 인장을 눌러도 반응하지 않는다.
- 진행 중인 균열이 있으면 그 균열을 가진 영웅만 선택할 수 있고 나머지는 흑백 + 자물쇠로 보인다(기존 LockedHero 규칙).
- 미리 보는 동안에는 저장하지 않는다. 저장은 "+"로 만들 때와 플레이로 입장을 확정할 때만 한다. 저장 실패 시 이전 상태로 복구하고 토스트를 보인다(기존 SaveEntryCharacter 방식).
- PC 키 동작(좌우 방향키 선택, Esc 타이틀, 선택 단추가 없을 때 Enter 시작)은 유지하고 여섯 영웅(앞줄 전사 남·여, 가운데 궁수 남·여, 뒷줄 마법사 남·여)을 순회하도록 넓힌다.
- 영웅 대사(생성 / 입장): 전사 남 "좋아, 한판 붙어 보자고!" / "다 덤벼라! 앞장은 내가 선다!", 전사 여 "배도 채웠겠다, 덤벼!" / "몸 다 풀렸어. 전부 날려 줄게!", 궁수 남 "흥정 끝! 이제 사냥 시간이야." / "한 발이면 충분하지!", 궁수 여 "조용히, 확실하게. 따라와." / "숨 고르고… 놓치지 않아.", 마법사 남 "책에 다 나와 있었다고!" / "계산 끝. 불꽃 맛 좀 봐라!", 마법사 여 "반짝반짝, 마법 시작이야!" / "자, 마법 쇼를 시작해 볼까?" (영어와 새 문구 전체는 README 의 문구 표).

## 결정
결정 1 — 확정(사용자, 2026-10-14): 영웅 6명 저장(옵션 A, 시안과 같음).
- 구조: AccountSave.heroes 는 여섯 영웅을 항상 모두 가진다. 인덱스 0~2는 지금과 같은 남자 전사·궁수·마법사, 3~5는 여자 전사·궁수·마법사다. 그러면 heroes[(int)heroClass] 같은 기존 가정이 남자 영웅에서는 그대로 맞고, 영웅 id(HeroSave.id)와 직업(HeroClass)도 그대로다. HeroSave 에 gender(기본 남자)와 created(만들었는지)를 더하고, (직업, 성별)로 영웅 인덱스를 찾는 한 곳을 둔다. 만들지 않은 영웅도 저장에는 기본 상태의 영웅으로 있고, "+"는 created 를 켜면서 그 영웅의 튜토리얼 몫을 기존 방식으로 시작한다.
- 이전: 저장 schema 를 올린다(GameStore.MaximumSchemaVersion, 지금은 D4 켜짐 22·레거시 21). 불러올 때 3명 저장을 여섯으로 늘리고(여자 3명은 NewAccount 와 같은 방식으로 만든 기본 영웅), 기존 3명은 남자 + 만든 상태로 표시한다. 원본은 기존 이전 방식처럼 .schema<N>-before-six-heroes.json 으로 보관한다. Read 의 heroes.Count!=3 검증은 3(옛 저장, 이전 대상) 또는 6만 받게 바꾼다. D4 레거시 개발자 모드(schema 21)는 새 저장을 읽거나 쓰지 않고 원본을 보존한 채 거부한다(기존 D4 거부 방식).
- 점검: "영웅 인덱스 = 직업 인덱스"를 가정하는 곳(selectedHero 245곳, heroes[ ] 150곳, 직업 인덱스 캐스트 218곳 중 실제 영웅 인덱스로 쓰는 것)을 분류해 바꾼다. 내가 찾은 곳: GameStore.cs 33·66·422 줄, GameStore.CharacterSwitch.cs, Tutorials.cs 의 Switch/Take/Park 와 GuideTutorial.WarriorIndex, PuzzleTutorial.cs 229·240, CombatSimulation.ClassSkillValidation.cs 32, CombatSimulation.SkillPreview.cs 30, CharacterSelectionState.LockedHero. 계정 전체 순회(일일 퀘스트·오프라인 보상·텔레메트리·도감 등)가 만들지 않은 영웅을 건드리지 않아야 한다. 영웅 목록을 보이는 기존 화면(인게임 캐릭터 변경, 직업 버튼 등)은 만든 영웅만 보인다. 영웅이 3명이라는 시험 가정은 고친다.
- 단계(권장): PR 1 = 저장 구조·이전·시험. 옛 선택 화면은 만든 영웅만 보여 동작이 그대로이고, 새 계정은 지금처럼 남자 셋을 모두 만든 상태로 시작한다. PR 2 = 새 화면과 새 계정 기본(결정 2). 영향 조사 결과와 단계 계획을 PR 설명에 적고 바로 진행한다(사용자 답을 기다리지 않는다). 영향이 예상보다 크면 멈추고 보고한다.
- 화면은 저장 구조를 직접 읽지 말고 "슬롯 여섯(직업·성별·만들었는지·레벨·균열 잠금·착용 장비)" 하나를 통해 읽게 만든다. PR 1 이 끝나기 전에는 슬롯 공급자를 시안의 예시 계정(전사 남 Lv.24, 궁수 여 Lv.17, 마법사 여 Lv.31 균열 진행 중, 나머지 미생성)으로 채운 가짜로 두고 화면·상태 규칙·시험을 먼저 만들 수 있다.
결정 2 — 새 계정의 첫 시작(기본): 지금처럼 EnterFirstStart 가 남자 전사를 바로 입장시킨다. PR 2 부터 새 계정은 남자 전사만 만든 상태로 시작하고 나머지 다섯은 +로 보인다(남자 궁수·마법사도 처음부터 만든 상태로 두려면 사용자 확인이 필요하다). 이 화면은 두 번째 접속부터 보인다.
결정 3 — 기존 계정의 세 영웅은 모두 남자 + 만든 상태로 옮긴다(기본). 레벨 1에서 진행이 없는 영웅을 +로 되돌리지 않는다.
결정 4 — 아트 지위: 장면·자세 그림은 Codex 원본, 영웅 층은 macOS Vision 으로 장면에서 오린 가공이라 네이티브 알파가 아니다. 게임에서는 교체 가능한 개발용 아트로 등록하고(productionApproved false) 가공 과정을 출처 기록에 적는다. 여전사 복장(배꼽 노출)은 12세 등급 검토가 필요하다고 사용자에게 알린다.
결정 5 — 글꼴: 이번 작업에서 새 글꼴을 넣지 않는다. UiTheme 이 소유한 글꼴을 쓰고, 스킬창 캐주얼 스킨 작업이 정한 Jua·Noto Sans KR 이 main 에 아직 없으면 지금 글꼴로 만든다.
결정 2~5는 위 기본으로 진행하고 보고에 적는다.

## 순서
0. 영향 조사와 단계 계획(코드 변경 없음): 결정 1 의 점검 목록, 위험, PR 1·2 계획을 PR 설명에 적는다. 이어서 PR 1(저장 구조)을 하고, 슬롯 공급자를 가짜로 둔 채 1~4번(그림·상태 규칙·화면·문구)을 병행해도 된다.
1. 이미지 층을 만든다. swiftc -O Docs/Art/CasualConcept/2026-10-13/CharacterPlaza/tools/lift.swift -o <임시>/lift 로 도구를 빌드하고, tools/prep_scene.py <spec> --out <임시> --format png --lift <임시>/lift 로 가로·세로 두 장면의 PNG 층과 scn_*.json 을 뽑는다(영웅 12명의 박스·기준점이 시안의 scene_data.json 과 같은지 확인). 가림 조각은 영웅 자리 조각만 싣고 전체 배경판 두 장은 화면 배경 외에 더 싣지 않는다. Resources 아래에 가져오고(투명 알파, 밉맵 끔, 모바일 압축), 새 텍스처마다 Editor/ResourceTextureBudget.cs 에 행을 더하고 용량 변화를 보고한다. 클릭 영역은 기존처럼 알파에서 미리 구운 마스크로 하고 런타임에 CPU 로 원본 텍스처를 읽지 않는다. provenance·art-qc 기록을 갱신한다.
2. 순수 C# 상태 규칙(Core): 슬롯 여섯, 선택, 단계(둘러보기·선택·생성 연출·만든 뒤·장비 팝업·입장·균열 잠금), 1.8초 생성 연출, 대사 고르기. 시안 tools/smoke.js 의 흐름(선택 → + → 화이팅 → Lv.1 → 장비 팝업 → 플레이 → 입장, 균열 잠금, 생성·입장 중 입력 무시)을 EditMode 시험으로 옮긴다.
3. 화면(Presentation): 장면과 영웅 층을 scene 좌표(scn_*.json)로 배치한다. 가로·PC 는 cover 맞춤, 세로는 직업별 카메라 틀(focus: x, y, 확대, 화면 x)로 이동·확대한다. 영웅 층은 그림 속 같은 자리에 정확히 겹친다(같은 변환). 비활성·활성(외곽선)은 모바일 비용이 작은 방식으로 고르고(작은 UI 셰이더 하나 또는 미리 구운 외곽선 층. uGUI Outline 여러 개를 영웅마다 붙이지 않는다) 근거를 남긴다. 이름표·머리 위 단추 줄·말풍선은 화면 안에 남도록 시안의 제한을 따른다. 장비 팝업은 공통 부품(EquipmentSlotView, ItemDetailView 계열)으로 8칸을 시안의 격자로 그린다. 영웅 그림이 필요하면 새 화풍 그림을 쓰고 예전 어두운 아틀라스를 이 팝업에 넣지 않는다. UiSafeArea, 새 문구·색·글꼴을 복제하지 않는다.
4. 문구: README 의 새 문구와 영웅 대사 12개를 한국어 키와 en.txt 로 함께 넣는다(이미 있는 문구는 재사용).
5. 저장·입장 연결(PR 1 의 구조 위에서): "+"는 영웅 만들기(만든 표시, 레벨 1, 그 영웅의 튜토리얼 몫은 기존 방식으로 시작), 플레이는 기존 CommitEntryCharacter. 미리 보기 중 저장 불변, 저장 실패 복구, 뒤로 가기·로그아웃·언어 전환·회전 뒤 선택 유지, 균열 소유 영웅 잠금.
6. 이전 화면 정리: CharacterSelectionStage, 세 아틀라스, SelectionCourtyard 사용을 새 화면으로 대체한다. 삭제하지 말고 쓰지 않게 만든 뒤 어떤 리소스를 지울 수 있는지와 용량을 사용자에게 보고한다(원본·.meta·Git 이력 보존).
7. 검증·문서.

## 하지 않을 것
닉네임 입력, 3D 영웅·애니메이션 교체(이 화면은 2D 그림 층), 새 BGM·효과음 제작(기존 char.select.* 를 재사용), 타이틀 화면과 다른 화면의 연출·언어 버튼, 전투·보상 규칙 변경과 결정 1 의 영웅 6명 저장 범위 밖의 저장 규칙 변경, 새 직업 ID(archer 는 Ranger), 직접 그린 그림(필요하면 Codex 이미지 생성), 스킬창 스킨 작업 영역, 시안에 없는 영웅 추가.

## 검증 (작업 중에는 관련 검사만)
- 관련 EditMode: 새 상태 규칙 시험, CharacterSelectionTests, LocalizationTests(새 키 한·영), ResourceTextureBudget 검사, 저장 이전·불러오기 시험(PR 1: 3명 저장이 여섯 영웅으로 올라가고 기존 3명이 만든 상태인지, 원본 보관, 새 schema 를 거부하는 레거시 모드, 만들지 않은 영웅이 계정 순회에서 제외되는지).
- 스모크 -hellscriptCharacterSelectionSmoke 를 새 흐름(여섯 영웅 클릭, 비활성·외곽선, +, 화이팅 대사, 레벨·사람 아이콘, 장비 팝업, 플레이·전투 준비·입장, 균열 잠금, 미리 보기 중 저장 불변)으로 고친다. 가능하면 별도 Mac 실행 창에서 실제 마우스·키보드 입력 근거(native-input.json 방식)를 남긴다.
- 화면: 세로 440×956, 가로 956×440, PC 1280×720·1440×900·1720×720, 한국어·영어에서 상태별(둘러보기, 만든 영웅 선택, +, 화이팅, 장비 팝업, 입장 중, 균열 잠금) 스크린샷을 찍어 Docs/Design/CharacterPlaza/reference/ 의 같은 상태와 나란히 눈으로 비교한다. 안전 영역, 터치 44pt, 겹침, 문구 잘림, 말풍선이 화면 안에 있는지 확인한다. 휴대전화 비율은 Mac 창과 모의 여백 결과이므로 실기기 결과라고 하지 마라.
- python3 tools/check_ui_contract.py --verify. 저장 후 UI 갱신 규칙(AgentRules/UI_Refresh.md)을 지킨다.
- 마지막 코드 변경과 최신 main 병합 뒤 한 번만: 전체 EditMode 와 macOS 개발 빌드 런타임 스모크. 이미 실패하던 검사와 새 실패를 구분한다. 문서만 바꾸면 게임 실행 검사를 다시 하지 않는다.
- 성능은 같은 조건의 이전·이후 실제 바이너리 각각 3회 이상의 CPU·GC 원자료가 있을 때만 말하고, 없으면 "미측정"이라고 쓴다. 텍스처 메모리 변화는 용량 숫자로 보고한다.

## 산출물
- PR 1(저장 구조)과 PR 2(새 화면). 둘 다 병합하지 않는다(사용자 승인 뒤에 한다). 관련 검사와 기록을 같은 커밋에 넣는다. PR 2 에 Docs/Design/CharacterPlaza 와 Docs/Art/.../CharacterPlaza 도 포함된다.
- 한/영 기록: Docs/Implementation/Character_Plaza_Select_20261014(.en).md(바꾼 것, 결정 1~5 결과, 가공 출처, 검사 결과, 확인하지 못한 것)와 Character_Selection(.en).md 갱신. 증거 스크린샷은 Docs/Implementation/CharacterPlazaEvidence/ 에 jpg 로 두고 문서에서 파일마다 링크한다.
- 위키 원본이 바뀌면 AgentRules/Wiki_Publish.md 절차를 따른다. 산출물 정리는 AgentRules/Artifact_Cleanup.md.

## 보고(한국어, 짧게)
결정 1 의 조사 결과(점검 목록·단계 계획)와 실제 변경, 실제로 확인한 것과 미검증 범위(실기기, 성능, 캔버스 안 동작), 시안과 다르게 한 곳과 이유, 용량 변화, 남은 일. macOS 결과를 실기기 결과로 말하지 마라.
```
