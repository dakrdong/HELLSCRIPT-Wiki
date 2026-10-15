# 캐릭터 광장 저장 영향 조사 — 2026-10-14

[English](Character_Plaza_Save_Audit_20261014.en.md)

사용자가 승인한 옵션 A를 구현한다. 격리 소스 폴더 `task-21/character-plaza`에서 저장·Core·관련 시험만 변경했다. 원본 체크아웃, 실제 저장, Unity Library/Build는 건드리지 않았다. UI·아트·공통 스킨은 다른 소유자가 이어서 통합한다.

## 단계와 저장 계약

- PR1: schema 23, 고정 여섯 슬롯, 이전 원본 보관, 생성 거래, 계정 집계 필터, 관련 시험.
- PR2: 캐릭터 광장 상태·UI·아트, 실제 새 계정 생성 경로에서 `NewAccount(catalog, onlyWarriorCreated:true)`. PR1의 기본 factory는 기존 시험·미리 보기용으로 남자 셋이 생성된 상태를 유지한다.
- 인덱스 0/1/2는 남자 Warrior/Ranger/Mage, 3/4/5는 여자 같은 직업이다. `HeroGender`만 추가하고 `HeroClass`, 기존 영웅 ID, 장비 ID는 바꾸지 않는다. `HeroSlots.Index(class,gender)`가 슬롯 매핑을 소유한다.
- 이전 3명은 남자·created=true로 명시한다. 여자 셋은 NewAccount와 같은 `NewHero` factory로 생성한 레벨 1 기본 데이터이며 created=false다. 최초 생성과 이전 모두 동일 catalog 경로를 사용한다.
- 변환 전에 읽기 원본의 정확한 바이트를 `<file>.schema<N>-before-six-heroes.json`에 보존한다. 백업 복구였다면 `.bak.schema<N>-before-six-heroes.json`이다. 보관 실패는 복구 fallback 없이 거부한다.
- 개발자 레거시 모드는 schema 21/3명으로 유지한다. schema 23 또는 여섯 슬롯 읽기·쓰기는 정규화·임시 파일 쓰기 전에 거부한다. 새 스키마 주 파일에서 옛 백업으로 몰래 돌아가지 않는다.
- `CreateCharacter(index)`는 기존 `Transact`의 분리 사본에서 created를 켜고 `Tutorials.Switch`로 새 튜토리얼 몫을 생성·주차한다. 현재 선택·현재 튜토리얼은 복원한다. 성공한 저장 뒤에만 현재 객체에 반영된다. 중복 생성·미생성 영웅으로 전환·진행 중 균열이 있을 때 생성은 거부한다.
- 실제 디스크가 없는 `Sandbox`의 옛 3명 사본도 같은 factory로 확장한다. 이 경로는 원본 저장을 쓰지 않는다.

## 실제 영향 분류

| 경계 | 처리 |
| --- | --- |
| GameStore 검증·경계 사본·거래 채택 | 3→6 읽기, gender/created 복사, 같은 직업 남녀를 ID 우선·성별 포함 fallback으로 구분 |
| Tutorials.Switch/Take/Park·GuideTutorial.WarriorIndex | ID 기반 몫 유지, 생성된 영웅만 이력·졸업 면제, 첫 시작은 남자 전사 고정 |
| PuzzleTutorial.ReplayAccount | 저장된 kitHero의 직업+성별 슬롯 사용 |
| ClassSkillValidation/SkillPreview·LegacyReplay | 성별 없는 직업 예시는 명시적으로 남자 슬롯 사용 |
| ContentUnlocks, RewardBoxes, FirstPlayRecommendation, OperationsRewards, RuneMasteryCatalog | 미생성 영웅을 계정 최고 단계·보상 자격·룬 등급에서 제외 |
| DailyQuests, PlayerObservations | 현재 영웅의 실제 이벤트를 사용, 미생성 선택의 이벤트 반영 방지. 전체 영웅별 추가 보상 루프 없음 |
| OfflineSupplies, RepeatHunt, MonsterCodex | 기록/진행의 영웅 ID를 유지하고 미생성 소유자 제외 |
| GameStore.SkillTree | 생성된 영웅만 운용 시점 초기화·활성화 |
| D4Allocation, CommitHuntEdictForHero, PreparePotions | 숨은 미생성 슬롯을 직접 편집/보급할 수 없도록 거부 |
| Blacksmith 대기 작업 | 미생성 영웅을 작업 시작·정산에서 제외하고 저장된 작업의 소유자 유효성 검증 |
| Storage, EquipmentShop, Blacksmith 기본 구조, PersistedItems, 장비 참조 보호 | 여섯 슬롯 전체 검증 유지. 미생성 슬롯의 기본 장비도 중복 ID·유효성·보호 검증 대상이며 보상/진행을 주지 않음 |
| Attendance seed | 기존 `heroes[0].id`를 유지하여 확정 보상 seed를 변경하지 않음 |
| classNames/skill arrays, `(int)hero.heroClass` | 직업 데이터 인덱스이므로 유지. 영웅 슬롯 인덱스로 사용되는 곳과 구분 |

## 공통 담당에게 필요한 정확한 Presentation 변경

이 파일들은 PR1에서 수정하지 않았다. 통합 소유자와 조율 후 적용한다.

1. `GameUI.ScreenSettings.cs`의 `BuildCharacterPane`: `foreach(int index in new[]{0,2,1})`를 `Enumerable.Range(0,game.Store.Data.heroes.Count).Where(i=>HeroSlots.IsCreated(game.Store.Data,i))`로 바꾼다. 표시 이름에는 `hero.gender==HeroGender.Female?" ♀":" ♂"`를 붙여 같은 직업 두 영웅을 구분한다.
2. `GameUI.cs`의 캐릭터 목록(기존 227행): 고정 3개 반복을 위와 같은 생성 슬롯 배열 반복으로 변경하고 `classNames[(int)hero.heroClass]`를 사용한다. `Across`의 위치/개수는 배열 위치/길이를 사용한다. 화면 폭에 여섯 명을 한 줄로 강제하지 않도록 공통 레이아웃 담당이 배치한다.
3. `GameController.cs`의 `SelectHero`(기존 87행): `if(Running||!HeroSlots.IsCreated(Store.Data,index))return;`으로 거르고 `Mathf.Clamp(index,0,2)`를 원래 `index`로 바꾼다. 여성 슬롯이 남자 마법사로 바뀌는 경로를 막는다.
4. `HuntEdictWindow.Progression.cs`의 copy fallback(기존 73행): `index=(int)hero.heroClass` → `index=HeroSlots.Index(hero.heroClass,hero.gender)`.
5. `SkillTreeD4/StoreSkillTreeD4Host.cs`: `Single(h=>h.heroClass==...)`는 여섯 슬롯에서 반드시 실패한다. 현재 선택 영웅이 해당 직업이면 그 ID, 그렇지 않으면 생성된 동일 직업 영웅을 선택한다. 생성된 영웅이 없는 직업 탭은 편집 불가여야 한다. 레벨/표시 key는 ID·gender·created를 포함한다. D4 공통 소유자 담당.
6. 기존 `RuntimeGuideTutorialSmoke`의 졸업 면제 검사는 `HeroSlots.Created(a)`만 대상으로 한다. 새 계정이 남자 전사만 생성된 PR2 정책에서는 다른 직업 전환 검사가 명시적으로 해당 테스트 영웅을 생성해야 한다.
7. `CharacterSelectionTests`의 index 3 invalid 가정·순회 3개는 PR2 소유자가 6개로 변경한다. 이전 atlas 검사는 자원 보존 여부에 맞춰 유지 가능하다.

`GameUI.Items` 등 장비 참조 보호 루프는 캐릭터 목록이 아니다. 미생성 슬롯에도 데이터 무결성 검사를 유지하므로 필터를 넣지 않았다.

## 검증과 인계

- `git diff --check`: 통과.
- 소스 검색으로 모든 Runtime `.heroes` 사용과 선택 인덱스/직업 캐스트를 검토했다. 위 표의 UI 의존성을 통합 담당에 전달했다.
- `HeroSlotsTests`: 13개 집중 시험 추가. 슬롯 독립성, schema22 원본·ID·튜토리얼 보존, 백업 복구, 원본 보관 실패, 생성 성공/실패/재시도, 여성 소유권, 생성/전환 잠금, 미생성 계정 집계·대기 대장간 작업 제외, 레거시 읽기/쓰기 거부를 다룬다.
- 관련 기존 시험의 schema22/3명 고정 기대와 D4 환급·튜토리얼 면제 대상은 새 계약에 맞췄다.
- **EditMode 시험·Unity 컴파일·실제 화면·런타임 스모크는 실행하지 않았다.** 최종 UI들과 합친 한 번의 통합 검증에 포함해야 한다. PR1 단독을 출시하지 말고 위 공통 UI 의존성과 PR2를 함께 검증한다.
- 실기기·CPU/GC 성능은 미측정. 새 아트/바이너리 생성 없음. 실제 세이브 접근 없음. 저장 테스트는 실행 시 임시 디렉터리만 사용한다.


## PR2 후속 적용

생산 생성자는 D4 활성 상태에서 남자 전사 한 명만 생성한다. 6슬롯 생성 명령이 없는 레거시 개발자 모드는 기존대로 남자 세 명을 생성한다. 명시적 기존 계정 fixture용 `GameStore.NewAccount` 기본은 남자 세 명 생성으로 유지한다. 기존 다중 영웅 시험에는 해당 전제가 필요한 위치에만 `ContentTestAccounts.ExistingMaleRoster`를 넣었다. 첫 시작·새 생성·이전 시험에는 적용하지 않는다. 독립 리뷰에서 다른 영웅의 진행도로 해제를 기대하는 fixture 두 곳도 보완했다. 생산/레거시 첫 계정 차이와 기존 보관본 바이트 불일치를 검사하는 시험을 추가했다(`HeroSlotsTests` 총15개). C# 정적 컴파일만 수행하며 Unity 실행은 통합에서 한다.

PR2가 설정 및 옛 성소 캐릭터 목록(생성된 영웅만, 성별 표시)과 사냥 칙령 미리보기 성별 fallback을 공급한다. 장비 무결성은 소유된 여섯 가방 모두 검사한다. `GameController.SelectHero`, D4 직업 host/탭 및 텍스처 예산은 통합 담당 소유다.
