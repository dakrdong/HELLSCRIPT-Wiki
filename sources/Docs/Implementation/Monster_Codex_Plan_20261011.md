# 몬스터 도감 개발 계획

작성일: 2026-10-11 · 대상: Codex 구현 작업 · [English](Monster_Codex_Plan_20261011.en.md)

## 1. 목표

처치한 적을 기록하는 몬스터 도감 창을 추가한다. 처치 기록이 있는 적은 초상화·이름·능력치·기술을 보여 주고, 아직 처치하지 않은 적은 `?`와 `???`로 가린다. 상세 화면의 균열 단계 슬라이더로 1단계부터 `최고 클리어 + 1` 단계까지 고르면, 그 단계 균열에 배치된 경우의 예상 능력치와 기술 피해량을 보여 준다.

## 2. 기준 시안

- 디자인 캔버스: https://claude.ai/artifact/MgykAGP9nxCmpwthfRN9P4
  - 1행: 가로 956×440 한국어 (목록 / 일반 적 상세 / 보스 상세)
  - 2행: 가로 956×440 영어
  - 3행: 세로 440×956 한국어 3장 + 영어 3장
  - 모든 화면은 실제로 조작할 수 있다. 목록의 카드를 누르면 상세로 가고, 슬라이더·`−`/`+`·일반/정예 전환이 수치를 바꾼다.
  - 시안 링크는 비공개 상태다. Codex 계정에서 열려면 사용자가 Share 메뉴로 공유해야 한다. 열 수 없으면 이 문서의 3–6장을 기준으로 한다.
- 시안의 수치(처치 수 예시, 발견 18/25, 최고 클리어 22)는 예시 데이터다. 실제 화면은 저장 데이터를 쓴다.
- 색·글꼴은 시안의 hex 값을 복제하지 말고 `UiTheme` 토큰과 `UiFonts`를 쓴다. 시안 색은 이미 `UiTheme`의 Obsidian & Gilt 값(Background `100f0d`, Panel `191713`, Border `3a3228`, Gold `d3af72`, GoldBright `f2d9a4`, Text `ece3cf`, Muted `a0978a`, Faint `6e675c`)에서 가져왔다.

## 3. 진입점: HUD 콘텐츠 독 아이콘

- 위치: 전투·광장 화면 오른쪽 위 설정 톱니 아래의 콘텐츠 독(`GameUI.ContentDock.cs`의 `AddContentDock`). 사용자가 지정한 칸은 **`사냥 칙령` 아이콘 바로 아래**다.
  - 구현: `contentDockEdict=Add("사냥 칙령",...)` 바로 다음 줄에 `Add("몬스터 도감","menu-monster-codex",ShowMonsterCodex)`를 넣는다. 그러면 기존 룬 보드·창고는 한 칸씩 아래로 내려간다.
  - 사용자가 위치를 표시한 스크린샷은 튜토리얼 스테이지(칙령의 목소리, 레벨 3) 전투 화면이었다. 그 화면에서도 보이도록 사냥 칙령과 같은 조건으로 보여 준다. 즉 `prologue` 분기 밖에 둔다. 이 판단이 맞는지 사용자에게 확인한다(9장).
- 아이콘 그림: Codex/GPT 이미지로 만든 `menu-monster-codex` 원형 메달 (`Docs/Art/MonsterCodex/`에 시안 A·B, 프롬프트, manifest). **사용자가 A안을 골랐다.** `Docs/Art/MonsterCodex/menu-monster-codex-A.png`를 그대로 `Assets/HELLSCRIPT/Resources/Art/GlobalHUD/menu-monster-codex.png`로 넣는다(1254×1254, 모서리 투명, 임포트 최대 512). 손으로 그리거나 코드로 도형을 그리지 않는다.
  - 텍스처 예산: `ResourceTextureBudget`의 `Art/GlobalHUD/menu-` 행(512, ASTC 6×6)이 이미 덮는다. 새 행은 필요 없고, 임포트 설정은 다른 `menu-*.png`의 `.meta`와 같게 맞춘다. Unity가 다시 쓴 `.meta`는 빌드 복제본에서 가져와 함께 커밋한다.
- 창을 여는 동작은 다른 독 버튼(사냥 칙령·창고)과 같다. 전투 중에 열면 기존 창들처럼 일시정지되고 닫으면 복원된다.

## 4. 데이터

### 4.0 선행 조건: 몬스터 레지스트리

2026-10-11 사용자가 몬스터 로스터 확장을 승인했다: 6개 필드 × 20종 + 보스 18종(N01–N120, B01–B18, 총 138). 매일 20종씩 추가하는 루틴이 몬스터를 게임과 도감에 함께 넣는다. 로스터 문서는 `Docs/Design/HELLSCRIPT_Monster_Roster_20261011(.en).md`이고, 지금은 `claude/monster-roster` 브랜치에 있다. 그래서 도감은 고정 25칸이 아니라 **레지스트리가 정하는 목록**을 그대로 따른다.

- 레지스트리 계약(브랜치 `claude/monster-registry`, 도감보다 먼저 `main`에 병합 예정):
  - 데이터: `Assets/HELLSCRIPT/Resources/Monsters/MonsterRegistry.json`
  - 런타임: `Runtime/Core`의 정적 클래스 `MonsterRegistry`
    - `All`: `IReadOnlyList<MonsterDefinition>`, 도감 순서(필드 순서, 그다음 ID)
    - `ById(string)`, `ForKind(int kind)`, `ForBoss(int pattern)`
    - `IdOf(EnemyState e)`: 보스는 pattern으로 B ID, 그 밖에는 kind로 N ID. 정예는 기본 kind로 세고, 고블린은 `null`.
  - `MonsterDefinition`:
    - 식별: `id`("N01".."N120", "B01".."B18"), `boss`, `kind`(N 번호 − 1, 보스는 −1), `pattern`(B 번호 − 1, 일반은 −1)
    - 표시: `name`(한국어 문구 키, 영어는 en.txt), `field`(`GameCatalog` 필드 인덱스, 보스도 거점 필드가 있다: B02 성채, B03 동굴), `role`, `family`, `unlockTier`, `portrait`(Resources 경로, 예: `Art/Monsters/Portraits/N21`)
    - 기술: `skills` 목록 `{이름 키, 설명 키, tags, 피해 배수 또는 0}`
  - 기존 25종은 지금의 kind/pattern 번호를 그대로 쓰고, 새 몬스터는 뒤에 붙는다.
  - 새 몬스터의 능력치도 `CombatSimulation.EnemyStats(kind 또는 pattern …)`로 얻는다. 종류별 배수(`KindHealth`/`KindAttack`/`KindSpeed`, `BossCombat.Profile`)를 늘리는 일은 레지스트리·로스터 작업이 맡는다. 도감은 공식을 따로 두지 않는다.
- **시작 조건**: `main`에 `MonsterRegistry`가 없으면 도감 작업을 시작하지 말고 사용자에게 알린다.

### 4.1 도감 항목 (레지스트리 기반)

- 항목: `MonsterRegistry.All` 전체, 그 순서대로. 하드코딩한 표나 개수를 두지 않는다. 지금은 기존 25종이고, 루틴이 하루 20종씩 늘린다(최종 138).
- 필드 탭: `definition.field`로 나눈다. 탭은 전체 · 6개 필드 · 보스이며, 보스 탭은 `boss == true`다. 탭의 `발견/전체` 숫자도 레지스트리에서 매번 계산한다.
- 이름: `definition.name` 문구 키(`Loc.T`). 필드 이름: `GameCatalog.FieldNames`. 보스의 필드 칩은 `definition.field`를 쓴다(B02 무너진 성채, B03 습한 동굴). 시안의 `균열 보스` 표기는 쓰지 않는다.
- 문구: 기존 몬스터·필드·보스 기술의 영어는 `Localization/en.txt`에 이미 있다(예: `쇠사슬 시체`→`Chained Corpse`, `묘지의 집행자`→`Graveyard Executioner`). 속성 이름도 있다: 물리=Physical, 화염=Fire, 냉기=Cold, 독=Poison, 비물리 저항=Non-Physical Resistance, 보스 제압=Boss staggered.
- 암흑 표기: `암흑`의 영어는 **Shadow로 통일한다**(사용자 결정, `암흑 저항`=`Shadow Resistance`에 맞춤). en.txt에서 아직 Dark를 쓰는 두 줄을 함께 고친다.
  - `암흑`=`Dark` → `Shadow`
  - `다음 사격 3회에 암흑 추가 피해.`=`Adds dark damage to the next 3 shots.` → `Adds shadow damage to the next 3 shots.`
- 초상화: `definition.portrait`. 기존 25종은 `Art/TrainingGround/Portraits/{N01..N20,B01..B05}`를 가리킨다.
- 기술 줄:
  - 기존 25종: 4.3의 코드 정의(`EnemyCombat.OwnHit`, `BossCombat.Hit` 등)에서 읽는다.
  - 새 몬스터: `definition.skills`에서 읽는다. 속성 칩은 tags의 속성(물리·화염·냉기·번개·독·암흑)을 쓰고, 피해 배수가 0이면 피해 칸을 비운다.
- 보스 기술 아이콘: 기존 보스는 `Resources/Art/Boss/skill-{BossCombat.Definition(attack)}.png`이다. `BossCombat.KitOf(pattern)` 순서 6개에 더해, 1페이즈와 2페이즈 사이에 `BOSS_ROAR`(포효)를 넣는다. 아이콘이 없는 기술(일반 적, 아이콘이 아직 없는 새 보스)은 글자만 표시한다.

### 4.2 처치 기록 (새 저장 항목)

- `AccountSave`에 ID 기준 처치 수를 추가한다:
  ```csharp
  [Serializable] class MonsterKill { public string id; public int count; }
  [Serializable] class MonsterCodexSave { public List<MonsterKill> kills = new(); }
  ```
  - JsonUtility는 없는 목록을 빈 목록으로 읽는다. 옛 저장은 빈 목록으로 시작하고, 기존 저장 키는 바꾸지 않는다.
- 기록 시점: `CombatSimulation.Deal`에서 적이 죽는 순간(`State.kills++` 옆)에 `MonsterRegistry.IdOf(e)`로 ID를 얻는다. 실행 중인 `RunState`의 ID별 카운터를 올린다. 계정 반영은 런 결산을 커밋하는 기존 `GameStore` 경로에서 함께 더한다. 새 저장 경로를 만들지 않는다.
  - `IdOf`가 `null`이거나 레지스트리에 없는 ID는 무시한다.
  - 정예와 부하의 처리는 `IdOf` 규칙을 따른다.
  - 셈 범위(기본값): 균열, 튜토리얼·가이드 스테이지, 퍼즐 등 실제 전투는 센다. **훈련장은 세지 않는다.** 훈련장은 원하는 적을 마음대로 배치할 수 있어서 세면 도감이 바로 채워진다. 9장에서 사용자에게 확인한다.
  - 중단 후 이어하기와 복원에서 두 번 세지 않도록, 런 스냅샷에 카운터를 저장하고 결산 때 한 번만 더한다.
- 발견 여부는 해당 ID의 `count > 0`이다. 진행 막대의 분모는 `MonsterRegistry.All.Count`이고 매일 늘어난다.

### 4.3 예상 능력치

- 공식을 새로 쓰지 말고 `CombatSimulation.EnemyStats(stage, kind, elite, boss, pattern, rift: true, introductory: IntroductoryRift.Applies(stage), tuning)`를 호출한다. `tuning`은 현재 라이브옵스 균열 설정이다.
  - 참고(설정 배수가 1일 때): 체력 = 70×1.08^(단계−1)×종류 배수(보스는 45×프로필)×정예 3, 공격 = 18×1.055^(단계−1)×종류 배수(보스는 3×프로필)×정예 1.5. 1–5단계는 입문 보정이 붙는다. 시안의 수치는 이 공식과 같고, 훈련장 10단계 보스 체력 6,297·공격력 87과도 맞는다.
- 슬라이더 범위: 1 ~ `account.Hero.highestClear + 1`. 균열 입장 규칙(`RiftEntry`: `run.stage <= highestClear + 1`)과 같다. 처음 값은 `highestClear`(0이면 1)이다.
- 표시 항목은 디아블로4 몬스터 구성을 따른다(4.4). 생명력, 공격력, 이동 속도, 피해 유형, 그리고 방어 구성(물리 저항, 비물리 저항, 보호막, 군중 제어, 보스는 제압).
  - 이동 속도는 단계와 무관하다(`단계와 무관`).
  - 보조 줄: `단계당 +8%` / `단계당 +5.5%`, 정예이면 `정예 ×3` / `정예 ×1.5`, 1–5단계면 `입문 보정`.
- 기술 피해량: 공격력 × 기술 배수. 기술마다 속성 칩(물리·화염·냉기·독·암흑)을 붙인다.
  - 배수와 속성은 코드 정의에서 읽고, 화면에 다시 적지 않는다: `EnemyCombat.OwnHit`, `CombatSimulation.Enemies`의 장판·사망 효과, `BossCombat.Hit`, `CombatSimulation.Bosses`. 속성 값은 `Element` 0 물리, 1 화염, 2 냉기, 4 독, 5 암흑이다.
  - 지속 피해는 초당 값으로 바꿔 보여 준다. 예: 0.25초마다 ×0.375이면 초당 ×1.5.
  - 배수가 없는 기술(소환·오라·회복)은 피해 칸을 비운다.
  - 시안의 배수는 코드에서 확인한 값이다. 예: 집행자 갈고리 ×1 물리, 무덤 종소리 ×1 암흑, 합창자 집중 광선 초당 ×1.5 암흑, 포식자 연속 폭발 각 ×1.5 암흑, 폭군 삼중 분출 각 ×1.2 화염, 포효 ×0.8 암흑.

### 4.4 디아블로4 몬스터 구성

사용자 지시: 몬스터 구성은 디아블로4의 실제 구성을 그대로 따른다. 조사 결과(2026-10, Maxroll 시즌 11 기준 정예 속성 문서 등), 디아블로4 몬스터는 다음으로 이루어진다. 몬스터에게 숫자로 된 방어도나 원소별 저항은 없고, 저항·보호막·면역은 정예 속성으로만 생긴다.

| 디아블로4 구성 | 디아블로4 내용 | 우리 게임 현재 | 도감 표시 |
|---|---|---|---|
| 몬스터 레벨 | 영웅과의 레벨 차이로 주고받는 피해가 보정된다 | 균열 단계 배율 | 균열 단계 슬라이더 |
| 등급 | 일반 / 정예(생명력·피해 증가, 정예 속성 2–4개) / 하수인(정예와 함께 나오며 생명력 증가) / 보스 | 일반 / 정예(생명력 ×3, 공격 ×1.5, 특성 1–2개) / 보스 | 등급 칩, 일반·정예 전환 |
| 생명력 · 피해 | 등급과 레벨로 정해짐 | `EnemyStats` | 생명력 · 공격력 |
| 피해 유형 | 물리 · 화염 · 냉기 · 번개 · 독 · 암흑 | 물리 · 화염 · 냉기 · 독 · 암흑 (번개를 쓰는 적 없음) | 피해 유형 칩, 기술별 속성 칩 |
| 물리 저항 / 비물리 저항 | 정예 속성 `Physical Resistance` / `Non-Physical Resistance`로만 생김 | 없음. 정예 특성 생명 연결(받는 피해 -20%)과 사격 방벽(전방 투사체 -50%)이 비슷한 역할 | 0%, 정예이면 "정예 속성으로만 생김" |
| 보호막 | 정예 속성 `Reprisal`이 가끔 최대 생명력 25% 보호막을 얻음 | 없음 | 없음 |
| 면역 · 저지 불가 | 정예 속성 `Shielded`(몇 초 면역), `Unstoppable` | 없음 | (구현 시 정예 속성 줄에 표시) |
| 보스 군중 제어 | 군중 제어가 걸리지 않고 와해(stagger) 게이지를 채움. 가득 차면 일정 시간 와해 | 속박·기절·빙결 면역(둔화는 적용), 제압 100 → 3초 무력화 → 10초 면역 | 군중 제어 "면역", 제압 칸 |
| 정예 속성 | 30개 이상 (화염·냉기·번개·독·암흑·일반 계열) | E01–E07 (아래) | 정예 전환 시 "정예 속성" 묶음 |

우리 정예 특성과 가장 가까운 디아블로4 정예 속성(참고용, 게임 화면에는 디아블로4 이름을 쓰지 않는다):

| 우리 특성 | 비슷한 디아블로4 속성 |
|---|---|
| E01 추적 화염 | Mortar / Fire Enchanted |
| E02 얼음 고리 | Frozen |
| E03 생명 연결 | Life Link |
| E04 사격 방벽 | Suppressor |
| E05 시체 폭발 | (없음, 우리 고유) |
| E06 분노 축적 | Berserker / Soul Drinker |
| E07 인력 붕괴 | Tempest |

**도감 1차 구현 범위**: 위 표의 "도감 표시" 열만 구현한다. 값은 모두 지금 게임에 실제로 있는 값이다. 새 전투 규칙은 만들지 않는다.

**후속 작업 후보 (사용자 승인 후 별도 단계)**: 디아블로4 구성을 전투에도 그대로 옮기려면 아래가 필요하다. 모두 전투 밸런스를 바꾸므로 같은 시드의 이전·이후 균열 시뮬레이션 비교가 필요하다.
1. 정예 속성 수를 1–2개에서 2–4개로 늘림.
2. 하수인 등급: 정예와 함께 나오는 일반 적의 생명력 증가. 디아블로4 공개 자료에 배율이 없어 기획 값이 필요하다.
3. 새 정예 속성: 물리 저항, 비물리 저항, 보복(최대 생명력 25% 보호막), 보호막(몇 초 면역), 저지 불가, 그 밖의 디아블로4 속성.
   - 저항 비율과 면역 시간은 디아블로4 공개 자료에 없어 기획 값이 필요하다.
   - 적 보호막·면역 표시가 HUD에 새로 필요하다.
4. 번개 피해를 쓰는 적. 지금은 하나도 없다.
5. 보스 와해 시간: 디아블로4는 최소 약 12초, 우리는 3초 무력화다.
6. 레벨 차이 피해 보정: 영웅 레벨과 균열 단계 차이로 주고받는 피해를 보정.

출처:
- [Maxroll: Elite and Affixes Overview](https://maxroll.gg/d4/resources/elites-affixes) (2025-12-09 갱신, 시즌 11): 정예 2–4 속성, 하수인 생명력 증가, 물리/비물리 저항·보복 보호막·면역·저지 불가 속성.
- [Fextralife: Stats and Attributes](https://diablo4.wiki.fextralife.com/Stats_and_Attributes): 몬스터 레벨 차이 피해 보정.
- [PureDiablo: Stagger Meter](https://www.purediablo.com/diablo4/Stagger_Meter), [WePC: How to Stagger Bosses](https://www.wepc.com/gaming/how-to-stagger-bosses-in-diablo-4/): 보스 군중 제어 면역과 와해 게이지.
- 일반/정예 전환은 일반 적에만 있다. 보스는 `보스 등급` 칩만 표시한다.

## 5. 화면

공통: `python3 tools/new_content_ui.py MonsterCodex`로 `MonsterCodexWindow`(`ContentWindowView`)를 만들고, 제목·뒤로·스크롤·안전 영역은 공통 구현을 쓴다. 목록과 상세는 한 창 안에서 전환하고(시안과 같음), 뒤로 버튼은 상세에서는 목록으로, 목록에서는 창을 닫는다.

### 5.1 목록

- 머리줄: 뒤로, (가로만) `HELLSCRIPT` 워드마크, 제목 `몬스터 도감`, 오른쪽에 `발견한 적` 진행 막대와 `18 / 25`. 세로에서는 워드마크를 숨기고 막대를 짧게 한다.
- 필드 탭: 전체 · 6개 필드 · 보스. 탭마다 `발견/전체` 숫자를 붙이고, 넘치면 가로로 스크롤한다.
- 격자: 가로 8열, 세로 4열. 항목 수가 매일 늘어 138개까지 가므로 세로로 스크롤하고, 카드 수에 맞춰 늘어나는 목록을 쓴다(스크롤 목록 재사용 또는 화면 밖 카드 생략 등, 기존 공통 스크롤 부품을 따른다). 정사각 초상화, 오른쪽 아래 도감 ID, 보스는 왼쪽 위 `보스` 띠와 금색 테두리. 아래에 이름과 `처치 N`을 쓴다.
- 미발견 카드: 비활성 버튼. 초상화 대신 `?`, 이름 `???`, `미발견`. 눌러도 열리지 않는다.
- 사용자 지시로 "한 번이라도 처치한 적이 기록됩니다" 안내 문구는 넣지 않는다.

### 5.2 상세

- 가로: 왼쪽 230px 초상화 패널(등급 칩 일반/정예/보스, 처치 수 칩, ID, 이름, 필드·행동 유형 칩). 오른쪽에는 능력치 4칸 한 줄(생명력·공격력·이동 속도·피해 유형 칩), 방어 구성 한 줄(물리 저항·비물리 저항·보호막·군중 제어, 보스는 제압 칸 추가), 사용 기술 목록(스크롤)이 온다. 정예로 바꾸면 목록 맨 위에 `정예 속성 · 1–2개 무작위` 묶음(E01–E06)이 붙는다. 아래 고정 줄에 `균열 단계` 라벨과 `최고 클리어 N단계`, `−`, 슬라이더(최고 클리어 위치 눈금), `+`, 큰 단계 숫자와 상태 문구(`클리어한 단계` / `다음 도전 단계` / `입문 균열 보정 적용`), 일반/정예 전환.
- 세로: 초상화 패널은 위쪽 전체 폭(높이 약 260), 능력치와 방어 구성은 각각 2×2(보스 방어 구성은 3줄), 기술 목록은 남은 높이에서 스크롤한다. 아래 줄은 두 단으로 나눈다: 1단은 라벨·단계 숫자·등급 전환, 2단은 `−` 슬라이더 `+`.
- 기술 줄: (보스만) 44px 아이콘(1페이즈 금색 테두리, 2페이즈·포효 붉은 테두리), 이름, 태그 칩(1페이즈/2페이즈/페이즈 전환/사망 시/약점/회복/오라/독/화염/냉기/둔화/2회), 설명, 오른쪽에 피해량(`피해`, `초당 피해`, `각 피해`, `0.5초마다 피해`)과 `공격력 ×배수`.
- 터치 영역은 44px 이상이다. 슬라이더는 드래그와 `−`/`+`를 모두 지원한다.

## 6. 문구와 다국어

- 모든 문구는 문구 표 키(`Loc.T`/`Loc.F`)로 넣고 `en.txt`에 영어를 채운다. 시안의 영어를 기준으로 한다.
  - 새 키 예: 몬스터 도감=Monster Codex, 발견한 적=Discovered, 미발견=Undiscovered, 처치 {0}=Killed {0}, 생명력=Life, 피해 유형=Damage type, 방어 구성=Defenses, 물리 저항=Physical Resistance, 보호막 없음/흡수하는 피해 없음=None/Nothing absorbs damage, 군중 제어=Crowd control, 모두 적용=All apply, 면역=Immune, 제압=Stagger, 정예 속성=Elite traits, 피해 감소 없음=No damage reduction, 단계와 무관=Same at every tier, 사용 기술=Skills, 최고 클리어 {0}단계=Best clear: tier {0}, 다음 도전 단계=Next challenge, 클리어한 단계=Cleared, 입문 균열 보정 적용=Intro rift reduction, 보스 등급=Boss grade, 1페이즈=Phase 1, 2페이즈=Phase 2, 페이즈 전환=Phase shift, 사망 시=On death, 근접 추적=Melee chaser, 사망 폭발=Death burst.
  - 이미 있는 키: 균열 단계=Rift tier, 정예=Elite, 일반=Normal, 보스=Boss, 공격력=Attack power, 이동 속도=Movement Speed, 원거리=Ranged, 장판=Zone, 지원=Support, 몬스터·필드·보스 기술 이름 전부.
  - 일반 적 기술 이름·설명과 보스 기술 설명은 새 문구다. 시안의 한국어·영어를 그대로 쓴다.
- 키 템플릿 함정: `{0}/{1}` 형태 키가 저장된 전투 문구를 삼키는 문제가 있었다. 새 키는 기존 키와 겹치지 않게 짓는다.

## 7. 검증

- EditMode:
  - 종류별 처치 기록. 정예는 기본 kind, 보스는 pattern, 부하는 자기 kind로 센다.
  - 훈련장 제외, 중단·복원 시 중복 없음.
  - 옛 저장은 빈 처치 목록으로 시작한다.
  - 상세 수치가 `EnemyStats`와 같다(1, 5, 6, 최고+1 단계, 정예).
  - 기술별 속성·배수가 코드 정의(`EnemyCombat.OwnHit`, `BossCombat.Hit`, 장판·사망 효과)와 같다.
  - 보스 방어 구성이 실제 규칙과 같다: 속박·기절·빙결 면역, 둔화 적용, 제압 100 → 3초 → 10초 면역.
  - 슬라이더 범위 1~highestClear+1, 미발견 항목은 열리지 않음.
  - 레지스트리의 모든 항목을 돌며 확인한다(개수를 하드코딩하지 않는다): 한국어·영어 이름, 초상화, `EnemyStats` 능력치가 모두 있다. 기존 보스는 기술 아이콘도 로드된다.
  - 레지스트리에 없는 ID나 `IdOf`가 `null`인 적의 처치는 저장되지 않는다.
  - 레지스트리에 항목을 하나 더한 테스트용 목록에서도 목록·탭 숫자·진행 막대 분모가 자동으로 따라간다.
  - 한국어·영어 문구가 빈 문자열이나 키 이름으로 나오지 않는다.
- UI 계약: `python3 tools/check_ui_contract.py --verify`.
- 네이티브 확인(macOS 개발 빌드 스모크): 독 아이콘 위치(사냥 칙령 아래), 목록, 일반 상세, 보스 상세를 세로 440×956, 가로 956×440, PC 16:9/16:10/21:9에서 한국어·영어로 캡처한다. 잘림·겹침·터치 영역을 확인한다. 글자 크기 조정 검사는 하지 않는다.
- 전체 EditMode와 런타임 스모크는 마지막 코드 변경과 `main` 통합 뒤 한 번만 실행한다. 병합 때 위키 게시 절차를 따른다.

## 8. 작업 순서 (권장 단계)

0. `main`에 `MonsterRegistry`가 있는지 확인한다(4.0). 없으면 멈추고 사용자에게 알린다.
1. 저장 항목 + 처치 기록 + 테스트.
2. `MonsterCodexWindow` 목록·상세(가로) + 문구 키.
3. 세로 배치, 보스 기술 아이콘, 포효 줄.
4. 독 아이콘 리소스 등록 + `ContentDock` 진입점.
5. 네이티브 캡처, UI 계약, 문서·위키.

## 9. 사용자 확인이 필요한 항목

1. 4.4의 후속 작업 후보(디아블로4 구성을 전투 규칙에도 옮기기)를 진행할지, 진행한다면 어느 항목부터 할지. 도감 1차 구현은 이 결정과 무관하게 진행한다.
2. 훈련장 처치를 도감에 셀지 (기본: 세지 않음).
3. 프롤로그·튜토리얼 화면에서도 도감 아이콘을 보일지 (기본: 사냥 칙령과 같이 보임, 스크린샷 위치 기준).

결정된 항목: 독 아이콘은 A안, `암흑`의 영어는 Shadow, 도감 목록은 `MonsterRegistry` 기반(로스터 세션 요청, 2026-10-11).
