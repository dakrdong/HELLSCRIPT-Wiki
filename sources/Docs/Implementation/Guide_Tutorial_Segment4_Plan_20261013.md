# 안내 튜토리얼 장비 추천 수업 구현 계획 (Codex 전달용)

갱신일: 2026-10-13

작성: Claude (계획만). 구현·검증·커밋은 Codex가 한다. 구현이 끝나면 이 문서를 결과 기록으로 고치고 영어판(`.en.md`)을 함께 만든다.

[English](Guide_Tutorial_Segment4_Plan_20261013.en.md)

> **상태 (2026-10-19): 구현됨.** 단계 번호·고정 보상·카드 흐름은 이 계획과 달라진 부분이 있으므로 실제 결과는 [실제 규칙 개편의 「장비 수업」](Guide_Tutorial_Real_Rules_20261019.md)을 따른다. 이 문서는 당시 계획으로 남긴다.

## 0. 전제와 기준

- 기준 코드는 **2·3구간이 들어간 `origin/main`**(PR #101, `a5596519` 이후)이다. 단계 상수는 `GuideTutorial.cs`의 `Entrance=0 … FinalBoss=24, Cleared=25, Last=Cleared`.
- **추천 장비 카드(시안 v2)가 먼저 `main`에 들어가 있어야 한다.** 시안은 Claude 아티팩트 「균열 장비 추천 UI」(`https://claude.ai/artifact/1fVkuNDqhdWyHN7ZFmFMmp`)이고, 구현 계획은 `Docs/Implementation/Rift_Gear_Offer_Cards_Plan_20261013.md`다. 구현은 브랜치 `claude/rift-gear-offer-actions` / `codex/rift-gear-offer-cards`에 있다. 그 작업이 머지된 `main`에서 시작한다. 아래 UI 이름은 그 브랜치 기준이다.
  - 카드 `rift-gear-card-{id}`, 아이콘 `rift-gear-icon-{id}`, 돋보기 `rift-gear-inspect-{id}`, 장착 `rift-gear-quick-equip-{id}`, 닫기 `rift-gear-dismiss-{id}`, 톱니바퀴 `rift-gear-settings-{id}`.
  - 설정 창의 추천 여부 `rift-gear-rule-all` / `rift-gear-rule-better`, 저장 `rift-gear-settings-save`, 닫기 `rift-gear-settings-close`.
  - 상세 창은 `RiftRecommendationWindow`(`compareFirst`)이고, 하단에 `장착`·`닫기`가 있다.
  - 이름이 바뀌었으면 실제 이름을 쓴다.
- 1~3구간 계획의 공통 규칙은 그대로 따른다.
  - 존댓말로 플레이어에게 말하고, 눈 가림·시간 정지 같은 설정은 설명하지 않는다.
  - 가림막(`PrologueGate`)으로 한 번에 컨트롤 하나만 가리킨다.
  - 진행 판단은 매 프레임 저장된 상태로 한다.
  - 문구는 `en.txt`에 한국어·영어를 함께 넣는다.
  - 작업은 최신 `origin/main` 기준 별도 worktree에서 한다.

## 1. 목표

3구간의 보물 고블린 장면과 보스 처치 뒤에 **추천 장비 카드** 사용법을 넣는다.

1. 고블린 보상: 금화(무작위 금액 5덩이) + 강화석(3·3·4개 3덩이) + **마법 등급 헬멧(고정)**. 헬멧을 주우면 추천 카드가 뜨고 → 아이콘을 눌러 상세 창을 연다 → 착용한다.
2. 보스 보상: **희귀 등급 검(고정) + 마법 등급 검(고정)**. 희귀 검 카드에서 `장착` → 이어서 점수가 더 낮은(−) 마법 검 카드가 뜬다 → 카드 설정에서 추천 여부를 `현재 착용중인 장비보다 점수가 높은 장비`로 바꾼다.
3. 그다음 기존 마무리 대사와 마을 졸업으로 간다.

## 2. 튜토리얼 전리품 (장비는 옵션까지 고정, 금화 금액만 무작위)

**장비는** 무작위 생성기(`ItemGenerator`/`DropFrom`의 무작위 굴림)에 맡기지 않는다(금화 덩이 금액만 예외). **필드를 명시해서 만든 고정 아이템**으로 떨어뜨린다. 기존 아이템 검증(`ValidateItems`, 부위·등급·옵션 범위)을 통과하는 값으로 정하고, 옵션 id는 게임에 있는 것만 쓴다. 수치는 아래 기본값에서 시작해 검증 범위에 맞게 조정한다.

| 출처 | 내용 | 기본값(제안) | 꼭 지킬 조건 |
| --- | --- | --- | --- |
| 보물 고블린 | 금화 **5덩이** | 덩이마다 **무작위 금액**(제안: 200~400, 합계 1,000~2,000) | 덩이마다 `RiftResources.Add(Gold)`로 따로 떨어뜨린다. 떨어지는 지점(영웅 뒤쪽 회수 우회 위치) 주변 반경 약 1~1.5m에 흩뿌리되, 모두 걸어서 닿는 위치(`Map.CanLand`)여야 한다. 금액은 런의 난수(`State.goblin.rng` 등 시드가 정해진 스트림)로 굴린다. 전리품 중 **금화만 무작위**이고 장비는 고정이다 |
| 보물 고블린 | 강화석 **3덩이** | **3개 · 3개 · 4개**(고정, 합계 10) | 덩이마다 `RiftResources.Add(EnhancementStone)`로 따로 떨어뜨린다. 금화 덩이와 같은 지점 주변에 흩뿌리고, 모두 걸어서 닿는 위치여야 한다 |
| 보물 고블린 | **헬멧 · 마법 등급(rarity 1)** | 영웅 레벨과 같은 아이템 레벨, 기본 방어 + 마법 옵션 2개(예: 최대 생명력 +, 방어력 %) | 튜토리얼 전사는 이 시점에 **헬멧을 착용하고 있지 않아야** 한다(대사가 "헬멧이 없었는데"). 시작 장비에 헬멧이 있으면 시작 때 빼 둔다 |
| 보스 | **검 · 희귀 등급(rarity 2)** | 옵션 3개, 공격 위주 | 장비 점수(`EquipmentScore.Value`)가 **지금 착용한 무기보다 분명히 높아야** 한다(카드에 `+차이`) |
| 보스 | **검 · 마법 등급(rarity 1)** | 옵션 2개 | 점수가 **희귀 검보다 낮아야** 한다(희귀 검 착용 뒤 카드 차이가 음수) |

- **정의 위치:** `GuideTutorial`(또는 전용 정적 클래스)에 고정 표로 두고 생성 함수 하나로 만든다. 무작위 시드·생성기 결과에 기대지 않는다.
- **아이템 id:** 계정 안에서 겹치지 않고 다시 만들어도 같은 값이어야 한다. 예: `{heroId}/guide/helmet`, `{heroId}/guide/rare-sword`, `{heroId}/guide/magic-sword`.
- **재시작 중복 방지:** 튜토리얼은 중단하면 처음부터 다시 시작한다(`StartGuideTutorial`). 같은 id의 아이템이 이미 영웅 인벤토리에 있으면 다시 떨어뜨리지 않는다(또는 기존 것을 그대로 쓴다). 이걸 막지 않으면 같은 아이템이 두 개 생긴다.
- **보상은 실제 계정에 지급한다.** 줍기는 균열과 같은 회수·저장 경로를 탄다. 튜토리얼 런에서 그 경로가 막혀 있으면 열어 준다. 졸업 뒤에도 인벤토리에 남는다.
- 이 표의 고정값을 **골든 테스트**로 묶는다(부위·등급·옵션 id·값·점수 관계). 생성 코드가 바뀌면 테스트가 잡게 한다.

## 3. 진행 순서와 대사

현재 3구간: `GoblinChase → ReturnToBoss → LootTalk(회수 수업) → Pickup → LootDoneTalk → FinalBoss → Cleared`. 여기에 끼워 넣는다(이름은 제안, 번호는 `Cleared`/`Last` 앞으로 이어 매기고 `Waiting()`에 대화 단계를 추가).

| 단계 | 상황 | 시간 | 내용 |
| --- | --- | --- | --- |
| `Pickup`(기존) | 회수 1순위로 고블린 보상을 줍는다 | 흐름 | **헬멧을 주워 그 카드가 처음 보이면** `HelmetTalk`(나머지 줍기는 수업 뒤 계속) |
| `HelmetTalk` | 헬멧 카드가 막 뜸 | 정지 | ① 보물 고블린에게서 새로운 장비가 떨어졌어요! 입을 수 있는 장비를 얻으면, 이렇게 바로 입을 수 있도록 추천 팝업이 나타난답니다. → [살펴본다] |
| (수업 H) | 카드 강조(링) → **아이콘** `rift-gear-icon-{helmet}` 터치 → 상세 창이 열림 | 정지 | 안내: 추천 장비의 아이콘을 눌러 자세히 살펴보세요. 상세 창이 열리면 ② 잘됐네요! 지금은 헬멧이 없었는데, 이 장비로 방어력을 더 보강할 수 있겠어요! 이어서 상세 창의 `장착`을 가리킨다(7절 결정 1): '장착'을 눌러 헬멧을 써 보세요. |
| (H 끝) | 헬멧 착용 | — | `ContinueTutorial()` → `Pickup`으로 돌아가 남은 보상(금화·강화석)을 줍는다 |
| `LootDoneTalk`(기존) | 고블린 보상을 다 주움 | 정지 | 기존 대사(다시 보스를 잡으러 가 볼까요?) |
| `FinalBoss`(기존) | 보스 처치 | 흐름 | **보스가 죽어도 `Finish`하지 않는다.** 희귀 검과 마법 검을 보스 자리에 떨어뜨리고 `BossLoot` |
| `BossLoot` | 영웅이 두 검을 줍는다(회수 1순위·전투 종료) | 흐름 | **희귀 검 카드가 보이면** `RareTalk`. 마법 검 카드는 같은 부위라 희귀 검 카드 뒤에 대기한다(시안 규칙: 부위당 카드 1장) |
| `RareTalk` | 희귀 검 카드 | 정지 | ③ 이것 보세요! 희귀 등급 장비를 얻었어요! ④ 이번에도 추천 장비로 빠르게 장비를 바꿔 볼까요? → [바꿔 본다] |
| (수업 R) | 카드의 `장착` `rift-gear-quick-equip-{rare}` | 정지 | 안내: '장착'을 눌러 주세요. 착용되면 대기하던 **마법 검 카드**가 나타난다 |
| `MagicTalk` | 마법 검 카드(점수 차이 음수) | 정지 | ⑤ 어? 이번엔 더 낮은 등급의 검이 표시됐네요. 이때 카드의 **점수 알약(음수 차이)**에 링을 씌워 강조한다. ⑥ 이렇게 기존 장비보다 낮은 등급의 아이템은 굳이 확인할 필요가 없죠. → [설정을 바꾼다] |
| (수업 S) | 카드 톱니바퀴 `rift-gear-settings-{magic}` → 설정 창 → 추천 여부 `rift-gear-rule-better` → `rift-gear-settings-save` | 정지 | 안내: 카드의 톱니바퀴를 눌러 주세요. / '현재 착용중인 장비보다 점수가 높은 장비'를 골라 주세요. / '저장'을 눌러 주세요. 저장되면 ⑦ 이렇게 하면 항상 기존보다 더 좋은 장비만 추천으로 나타나게 할 수 있어요. |
| (S 끝) | 남은 마법 검 카드 | — | 대기 중인 카드는 설정을 바꿔도 남는다(시안 규칙 5). 안내: 카드의 ×를 눌러 닫아 주세요. 닫히면 `Cleared`(7절 결정 2) |
| `Cleared`(기존) | 마무리 대사 → 졸업 | 정지 | 변경 없음. `Finish(true, …)`는 여기로 옮긴다 |

영어 초안(다듬어도 된다):
- "New gear dropped from the treasure goblin! When you get gear you can wear, a recommendation pops up like this so you can put it on right away."
- "Tap the recommended gear's icon to take a closer look."
- "Great! You had no helmet until now, so this will shore up your defense!"
- "Tap 'Equip' to put on the helmet."
- "Look at this! You got rare gear!"
- "Shall we swap gear quickly with the recommendation again?"
- "Huh? This time a lower-grade sword is showing."
- "There is no need to check items lower than your current gear like this."
- "Tap the gear icon on the card." / "Choose 'Gear with a higher score than what is worn'." / "Tap 'Save'."
- "This way, only gear better than what you have will be recommended."
- "Tap × to close the card."
- 선택지: 살펴본다 / Take a look · 바꿔 본다 / Swap it · 설정을 바꾼다 / Change the settings

## 4. 파일별 작업

### 추천 카드 쪽 (`RiftGearRecommendations.cs`, `GameUI.GearRecommendations.cs`, `GameStore.RecommendedEquipment.cs`)
- `RiftGearRecommendations.Visible`은 `run.training>=0||run.tutorial`이면 빈 목록을 돌려준다. **안내 튜토리얼 런(`GuideTutorial.Running(run)`, 리플레이 아님)은 허용한다.** 튜토리얼 런은 `training=3`이니 조건 순서를 조심한다.
- 카드 표시 조건(전투 화면, 포탈·마을 숨김)이 튜토리얼 전투 화면에서도 맞는지 확인한다.
- 줍기 때의 판정(`drop.gearRecommended = CanOffer(...)`, 획득 시점 1회)이 튜토리얼 줍기에서도 기록되는지 확인한다.
- 카드의 `장착`·`닫기`·설정 저장 거래가 튜토리얼 런에서 거절되지 않는지 확인한다(균열은 `suspendedRun` 기준일 가능성이 높다). 막히면 `CommitHuntEdict`의 "prologue" 판정(`Data.guide.tutorialRun==simulation.State`)처럼 안내 튜토리얼 런을 허용한다. 거래 뒤 `Data.guide.tutorialRun`이 살아 있는 런을 가리키는지 확인한다(1구간 함정).
- **점수 알약에 이름이 없으면 붙인다**(예: `rift-gear-score-{id}`). 수업 S 앞 강조에 쓴다.
- 상세 창 하단 `장착`에도 이름이 없으면 붙인다(예: `rift-gear-detail-equip`).

### 튜토리얼 쪽 (`GuideTutorial.cs`, `CombatSimulation.Guide.cs`, `CombatSimulation.Goblin.cs`, `GameStore.Guide.cs`)
- 단계 상수를 추가하고 `Cleared`/`Last`를 맨 끝으로 옮긴다. `Waiting()`에 `HelmetTalk`, `RareTalk`, `MagicTalk`을 넣는다. `Tutorials.NormalizeShare`가 `GuideTutorial.Last`로 범위를 검사하는지 확인한다(3구간에서 이미 바뀌었을 것).
- **시작 설정**: 튜토리얼 시작 때(`StartGuideTutorial`/`BarePowers` 근처) 영웅의 추천 장비 설정 `riftGearOffer.allWearable=true`(착용 가능한 모든 장비)로 둔다. 기본값 `false`(더 좋은 장비만)이면 마법 검이 획득 시점 판정에서 빠져 수업 S가 성립하지 않는다. 등급·부위 마스크는 모두 켠다(`gradeMask` 31, `rangeMask` 511). 이 값은 칙령이 아니라 `HeroSave.riftGearOffer`라 `GameStore.Transact`의 영웅 복사 목록에 이미 있는지 확인한다(카드 계획 5절).
- `RewardGoldenGoblin`의 `IsGuide` 분기: 지금은 금화 + 희귀 장비 1개(무작위)다. **금화 5덩이(무작위 금액)·강화석 3덩이(3·3·4개)·고정 헬멧**으로 바꾼다. 떨어뜨리는 위치 계산(영웅 뒤쪽, 회수 우회 필요)은 그대로 둔다.
- `FinalBoss`: 보스 사망 시 `Finish`를 부르지 않고, 보스 위치 근처의 회수 가능한 지점에 고정 검 두 개를 떨어뜨린 뒤 `BossLoot`. **희귀 검이 먼저 줍히도록** 희귀 검을 영웅 쪽에 둔다(획득순 대기열이라 희귀 검 카드가 먼저 뜬다).
- 단계 전환 판정:
  - `Pickup`: 헬멧 카드가 `Visible`에 처음 보이면 → `HelmetTalk`.
  - `HelmetTalk` 계속 조건: 헬멧 착용.
  - `BossLoot`: 희귀 검 카드가 보이면 → `RareTalk`.
  - `RareTalk` 계속 조건: 희귀 검 착용. 이어서 마법 검 카드가 보이면 같은 정지 안에서 `MagicTalk`.
  - `MagicTalk` 계속 조건: `riftGearOffer.allWearable==false` 저장 완료 + 7절 결정 2 처리.
  - 대화·수업은 시뮬레이션이 아니라 저장소·UI 상태(카드 목록, 착용, 설정)로 판단하는 것이 많다. 판단 함수는 `GuideTutorial`에 두고 UI·테스트가 함께 쓴다.
- 영웅 보호, 보스 정지(고블린~줍기 동안) 등 3구간 규칙은 유지한다. 보스가 죽은 뒤에는 남은 적이 없어야 한다(소환물 정리).

### UI (`GameUI.GuideTutorial.cs`)
- 수업 종류에 `helmet`, `rare`, `settings`를 추가하고 위 순서대로 가림막 대상을 고른다. 카드·상세 창·설정 창은 다른 캔버스이므로, 지금 쓰는 `Named(root,name)`로 각 루트(카드 묶음, `RiftRecommendationWindow`, `RiftGearSettingsWindow`)에서 찾는다.
- 수업 H에서 아이콘 대신 돋보기를 눌러도 같은 상세 창이 열리므로 둘 다 통과로 본다(시안: 아이콘 본체도 상세를 연다).
- 점수 강조(⑤·⑥)는 누를 대상이 없는 안내 단계다. 3구간에서 만든 "대상 없는 안내 카드 + 탭으로 다음"을 재사용하고, 점수 알약에 `TutorialAnchorRing`을 씌운다.
- 설정 창이 열려 있는 동안 카드 타이머가 멈추는지 확인한다(카드 계획: 구현됨). 튜토리얼은 처음에 팝업 시간 `취소 버튼을 누를 때까지`(기본)로 두어 수업 중 카드가 만료되지 않게 한다.
- 목표 문구(`GuideObjective`)를 새 단계마다 넣는다. 예: 목표 · 추천 장비 살펴보기 / 목표 · 희귀 장비로 교체 / 목표 · 추천 설정 바꾸기.

## 5. 테스트와 검증

작업 중에는 관련 검사, 마지막 코드 변경과 `main` 통합 뒤 전체 Edit Mode와 macOS 스모크를 한 번(`CLAUDE.md`).

- 골든 테스트(고정 장비): 세 아이템의 부위·등급·옵션 id·값이 정해진 값과 같다. 점수 관계는 희귀 검 > 착용 무기, 마법 검 < 희귀 검. 헬멧은 착용 헬멧이 없을 때 착용 가능하다. 같은 영웅으로 두 번 만들면 같은 id·같은 값이고, 이미 인벤토리에 있으면 다시 떨어뜨리지 않는다.
- `GuideTutorialTests` 확장:
  - 고블린 처치 → 금화 5덩이(각 금액이 정해진 범위 안, 서로 다른 지점)·강화석 3덩이(3·3·4개)·헬멧이 바닥에 생긴다. 금화·강화석 덩이를 모두 주워야 `LootDoneTalk`로 간다.
  - 회수 1순위 뒤 헬멧을 주우면 헬멧 카드가 보이고 `HelmetTalk`에서 멈춘다. 착용 전에는 계속이 막히고, 착용 뒤 남은 보상을 줍고 `LootDoneTalk`.
  - 보스 처치 → `Finish` 없이 두 검이 생긴다. 희귀 검 카드 → 장착 → 마법 검 카드가 보이고, 점수 차이(`RiftGearRecommendations.Offer(...).delta`)가 음수다.
  - 설정을 "더 좋은 장비만"으로 저장하기 전에는 계속이 막히고, 저장 뒤 `Cleared` → 졸업. 졸업 뒤 인벤토리에 세 장비가 있고 헬멧·희귀 검을 착용 중이며 금화·강화석이 늘었다.
- 추천 카드 테스트(`RecommendedEquipmentTests` 등): 튜토리얼 런에서 `Visible`이 카드를 돌려주고, 일반 훈련·리플레이 런에서는 여전히 빈 목록이다.
- `python3 tools/check_ui_contract.py --verify`, `python3 tools/check_ui_refresh.py`.
- `RuntimeGuideTutorialSmoke`를 이 단계까지 이어서 실제 포인터로 진행한다. 캡처해서 직접 볼 것: 헬멧 카드 강조, 상세 창, 희귀 검 카드 `+차이`, 마법 검 카드 `−차이` 강조, 설정 창 추천 여부 선택. 최소 440×956 한국어와 956×440 영어. 가로 화면은 카드가 75% 크기인 점을 확인한다.

## 6. 하지 말 것

- 무작위 굴림으로 튜토리얼 **장비**를 만들지 않는다(같은 아이템·같은 옵션이 항상 나와야 한다). 무작위는 금화 덩이 금액뿐이다.
- 추천 카드·상세·설정 창을 새로 만들지 않는다. 시안 v2 구현을 그대로 쓴다.
- 튜토리얼 밖의 추천 규칙(획득 시점 판정, 부위당 1장 대기열, 기본 "더 좋은 장비만")을 바꾸지 않는다. 튜토리얼 시작값만 바꾼다.
- 설정 설명(눈 가림·시간 정지)이나 '스킬 사용' 접근 방식을 대사에 넣지 않는다.

## 7. 확정된 결정 (2026-10-13 사용자 확인)

1. **헬멧은 수업 H에서 착용까지 시킨다.** 상세 창이 열리고 대사가 나온 뒤 상세 창의 `장착`을 가리킨다.
2. **설정 저장 뒤 남은 마법 검 카드는 플레이어가 직접 닫는다.** ⑦ 대사 뒤 카드의 ×(`rift-gear-dismiss-{magic}`)를 가리키고, 닫히면 `Cleared`로 간다.
3. **전리품 수치는 2절 표의 기본값으로 한다.** 금화는 무작위 금액 5덩이, 강화석은 3·3·4개 3덩이(둘 다 사용자 요청 2026-10-13), 옵션 값은 검증 범위 안에서 정하되, 점수 관계(희귀 검 > 착용 무기, 마법 검 < 희귀 검)는 반드시 지킨다.
