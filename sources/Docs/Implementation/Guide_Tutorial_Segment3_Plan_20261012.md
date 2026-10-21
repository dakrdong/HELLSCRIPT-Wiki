# 안내 튜토리얼 3구간 구현 계획 (Codex 전달용)

갱신일: 2026-10-12

작성: Claude (계획만). 구현·검증·커밋은 Codex가 한다. **2구간 계획(`Guide_Tutorial_Segment2_Plan_20261012.md`)을 먼저 구현한 뒤 이어서 하거나 한 PR로 함께 한다.** 2구간 계획의 "2. 반드시 지킬 것", "4. HUD 수업 안내", "7. 알려진 함정"은 3구간에도 그대로 적용된다. 구현이 끝나면 이 문서를 결과 기록으로 고치고 영어판을 함께 만든다.

## 1. 목표

2구간(원거리 적 + 첫 교전·전투 중 위치) 다음 구역에서 여러 타입의 적과 싸운다. 그 과정에서 사냥 칙령의 세 가지를 HUD에서 직접 바꾼다.

1. **공격 대상 선택**: 몬스터 등급 순서, 몬스터 타입 순서. 몬스터 머리 위 순번 표시도 함께 알려 준다.
2. **보물 몬스터 등급**: 보물 고블린을 1순위로 올린다.
3. **공통 행동 우선순위**: "회수"를 1순위로 올려 전투 중에도 전리품을 줍게 한다.

보스를 처치하면 기존 마무리 대사와 마을 졸업으로 간다.

사용자가 준 화면 그림 3장:
- 첨부1(등급): 1 일반 몬스터 · 2 소환된 적 · 3 엘리트 몬스터 · 4 보스 몬스터. 5 보물 몬스터는 **지금은 표시하지 않는다**.
- 첨부2(타입): 1 원거리 물리 공격 타입 · 2 원거리 마법 공격 타입 · 나머지는 순서 무관.
- 첨부3(공통 행동 우선순위): 공격 · 회피 · 버프 유지 · 회수 · 목표 진행 · 재정비 · 탐색 순서의 시작 상태에서 회수를 1순위로.

## 2. main에서 이미 쓸 수 있는 것 (새로 만들지 않는다)

| 기능 | 위치 | 비고 |
| --- | --- | --- |
| 공격 대상 선택 패널 | `GameUI.EdictTarget.cs`(`BuildTargetPanel`, `AddTargetOrder`) | 탭 `edict-target-tab-0`(우선 순위), 판정 순서 칩 `edict-target-category-GRADE`/`-TYPE`/`-STATUS`, 카드 `edict-target-card-{code}`, 이동 버튼 `edict-sheet-up-{id}-{code}`/`edict-sheet-down-{id}-{code}`, 뒤로 `edict-target-back` |
| 몬스터 머리 위 순번 | `ShowTargetForecast` → `game.World.ShowEdictTargets` + `ShowEdictTargetLabels` | 공격 대상 패널이 열려 있는 동안 필드 몬스터 위에 순번을 띄운다. 순서를 바꾸면 갱신된다 |
| 공통 행동 우선순위 패널 | `GameUI.EdictOrder.cs`(`BuildOrderPanel`) | 카드 `edict-order-card-{code}`, 이동 `edict-sheet-up-common.order-{code}`, 뒤로 `edict-order-back`, 닫기 `edict-sheet-close` |
| 등급·타입 판정 | `EdictTargetPolicy.Grade/Type` | 보스=`BOSS`, 정예=`ELITE`, 고블린=`TREASURE`, 보스 소환물=`SUMMONED`(`add && summonerId>=0`), 나머지 `NORMAL`. 타입은 몬스터 역할에서: 근접 0=`MELEE`, 돌진 1=`CHARGER`, 원거리 2=`RANGED`, **장판 3=`CASTER`(원거리 마법)**, 지원 4=`SUPPORT`, 폭발 5=`EXPLODER` |
| 전투 중 회수 규칙 | `EdictRuleOrder.PickupDuringCombat(order)` | `PRIORITY_LOOT`가 `ATTACK`보다 위면 전투 중에 줍고, 아니면 전투가 끝난 뒤 줍는다. 시나리오의 "보스에게 돌아감 → 회수를 1순위로 → 줍기"가 이 규칙 그대로다 |
| 보물 고블린 | `CombatSimulation.Goblin.cs`(`SpawnGoldenGoblin`, `TickGoldenGoblin`, `RewardGoldenGoblin`), `GoldenGoblin` 상수(`EdictFieldPolicy.cs`) | 균열에서만 확률로 등장한다. 보상은 `State.training>=0`이면 **지급하지 않는다**(튜토리얼 런은 training=3) |
| 보스 소환 | `BossCombat.Summons/SummonKind/SummonCount`, `CombatSimulation.Bosses.cs` | 소환 패턴(`BossAttack.Summon` 등)이 있는 보스 패턴을 고른다 |

코드 값 참고: 등급 기본 `BOSS,NORMAL,ELITE,TREASURE,SUMMONED`, 타입 기본 `MELEE,CHARGER,RANGED,CASTER,SUPPORT,EXPLODER`, 판정 순서 기본 `GRADE,TYPE,STATUS`, 공통 행동 기본 `DODGE,BUFF,PRIORITY_LOOT,ATTACK,OBJECTIVE,REGROUP,EXPLORE`(`HuntEdictV2.Order.cs`). 옵션 id: `target.priorityGrade`, `target.priorityType`, `target.priorityStatus`, `target.priorityCategories`, `target.goblin`, `target.goblinChase`, `common.order`.

## 3. 진행 순서와 대사

2구간의 마지막 전투(`FinalFight`)가 끝나면 `Cleared`로 가지 않고 3구간으로 이어진다. `Cleared`(마무리 대사 → 졸업)는 계속 맨 끝이다. 대사는 존댓말로 플레이어에게 말하며, 설정 설명(눈 가림·시간 정지)은 하지 않는다. 사용자 원문을 다듬은 기본 문장이다.

| 단계(제안 이름) | 상황 | 시간 | 내용 |
| --- | --- | --- | --- |
| `Road3` | 회랑 오른쪽 문 → 꺾인 통로 → 광장(마지막 방)으로 걸어간다 | 흐름 | 광장에 보스와 무리가 있다 |
| `MixedFight` | 탱커·장판·(원거리)·보스가 싸움을 건다. 영웅은 시작 설정대로 **근접 탱커**를 친다. 장판 공격에 맞는다 | 흐름 | 영웅 HP가 50% 아래로 떨어지면 다음 단계 |
| `TargetTalk` | HP 50% 미만 | 정지 | ① 적들의 협동 공격이 시작됐어요! 이럴 때 체력이 많은 적부터 공격하면, 다른 적들에게 더 많은 공격 기회를 주게 돼요. ② 공격 우선순위를 바꿔서 적을 더 효율적으로 물리쳐 봐요. → [공격 대상을 정한다] |
| (수업 T1: 등급) | 사냥칙령 → 전투 → 공격 대상 선택 → 우선 순위 탭 → 판정 순서 '몬스터 등급' → 첨부1 순서로 이동 | 정지 | 완료 뒤 안내: ③ 지금은 보스보다 다른 몬스터들을 먼저 처치하는 게 좋겠어요. 그러면 보스를 공격할 때 다른 위협을 줄일 수 있답니다. |
| (수업 T2: 타입) | 판정 순서 '몬스터 타입' 칩 → 원거리 물리 1, 원거리 마법 2 | 정지 | 완료 뒤 안내: ④ 지금은 방어형 몬스터 뒤에서 공격하는 원거리 적을 먼저 처치하는 게 좋겠어요. ⑤ 우선순위를 다시 정할 때마다 몬스터의 머리 위에 공격 대상 순번이 나타나요. ⑥ 우선순위는 싸우는 도중에 바꿔 보면서, 내가 생각한 순번대로 정해지는지 확인하는 게 좋아요. |
| (수업 T 맺음) | 패널·펼침을 자동으로 닫는다 | — | `ContinueTutorial()` |
| `TargetFight` | 영웅이 장판·원거리 적부터, 그다음 일반·소환물을 처치한다 | 흐름 | 보스 외 모든 적이 쓰러지면 다음 단계 |
| `BossFight` | 보스만 남아 보스를 공격한다 | 흐름 | 보스 HP가 80% 아래면 다음 단계 |
| `GoblinAppear` | 보물 고블린이 화면 왼쪽에서 나타나 오른쪽으로 달린다 | 흐름 | 등장할 때 말풍선(멈추지 않음): 저기 보세요! 보물 고블린이 나타났어요! |
| `GoblinTalk` | 고블린이 화면 오른쪽 끝에 닿기 전 | 정지 | ① 보물 고블린은 처치하면 엄청난 보상을 떨어뜨려요! 하지만 시간이 지나면 도망가 버리니까, 빠르게 쫓아가는 게 중요해요. → [보물 고블린을 노린다] |
| (수업 G) | 사냥칙령 → 전투 → 공격 대상 선택 → 우선 순위 → 몬스터 등급. **'보물 몬스터' 카드가 나타나는 연출** 뒤 1순위로 올린다 | 정지 | 단계 안내: 몬스터 등급에서 '보물 몬스터'를 최우선 처치 순위로 올려 봐요. 1순위가 되면 필드의 보물 고블린 머리 위에 1이 나타난다(패널이 열린 동안의 기존 순번 표시). 완료 뒤 안내: ② 우선순위가 바뀌었어요! 이제 보물 고블린을 쫓아서 보상을 얻어 봐요! |
| `GoblinChase` | 영웅이 고블린을 쫓아가 처치한다. 보상이 바닥에 떨어진다 | 흐름 | 고블린이 죽으면 다음 단계 |
| `ReturnToBoss` | 공통 행동 1순위가 '공격'이라 영웅이 줍지 않고 보스에게 돌아간다 | 흐름 | 보상에서 몇 걸음 멀어지면 다음 단계(아래 6절) |
| `LootTalk` | 돌아가는 도중 | 정지 | ① 앗! 잠깐만요, 아직 바닥의 보상을 줍지 않았어요! ② 적을 처치하는 것도 중요하지만, 보상을 챙기는 건 그 이상으로 중요하다고요! → [행동 우선순위를 바꾼다] |
| (수업 L) | 사냥칙령 → 전투 → 행동 우선순위 → '회수'를 1순위로 → **닫기**(사용자 요청: 직접 창을 닫는다) | 정지 | 닫으면 `ContinueTutorial()` |
| `Pickup` | 영웅이 보상을 모두 줍는다 | 흐름 | 고블린 보상을 다 주우면 다음 단계 |
| `LootDoneTalk` | 다 주운 직후 | 정지 | ① 이제 떨어진 아이템을 모두 주웠으니, 다시 보스 몬스터를 잡으러 가 볼까요? → [보스에게 간다] |
| `FinalBoss` | 영웅이 보스에게 돌아가 처치한다 | 흐름 | 보스가 쓰러지면 `Cleared` |
| `Cleared`(기존) | 기존 마무리 대사 3줄 → 졸업 | 정지 | 변경 없음 |

영어 초안(다듬어도 된다):
- "The enemies have started working together! If you attack the toughest enemy first, the others get more chances to strike."
- "Change your attack priorities to defeat the enemies more efficiently."
- "Right now it is better to defeat the other monsters before the boss. That way there are fewer threats while you fight the boss."
- "Right now it is better to defeat the ranged enemies attacking from behind the defensive monster first."
- "Each time you set the priorities again, the attack order appears above each monster's head."
- "When you change priorities, it is good to adjust them mid-fight and check that the order matches what you intended."
- "Look over there! A treasure goblin has appeared!"
- "A treasure goblin drops an enormous reward when defeated! But it runs away after a while, so it is important to chase it quickly."
- "Raise 'Treasure monster' to the top priority in the monster grades."
- "The priorities have changed! Now chase the treasure goblin and claim the reward!"
- "Oh! Wait a moment, you have not picked up the reward on the ground yet!"
- "Defeating enemies matters, but collecting rewards matters even more!"
- "Now that you have picked up every item, shall we go back and defeat the boss?"

## 4. 구역과 적 구성

- 구역: `GuideTutorial.Layout()`의 마지막 방(광장 `RM12`, 중심 `(84,38)`, 26×26)을 쓴다. 3구간 길(waypoint): 회랑 안 → 오른쪽 문 `(64,24)` → `(67,27)` → `(67,34)` → 광장 왼쪽 문 `(71,38)` → 광장 안 `(76,38)`.
- 적(시작값, 테스트로 맞춘다). 각 그룹은 `EnemyState.group`으로 구분하고, 소환은 처음부터 하되 `lastSeenTime=-100`으로 둔다(2구간과 같은 이유).
  - **근거리 탱커**: 역할 0(근접, 필드 0 슬롯 0 = kind 0). 앞줄 `(80,38)`. 체력을 높게 잡아 영웅이 먼저 때리는 동안 오래 버티게 한다.
  - **장거리 장판 공격**: 역할 3(장판, 필드 0 슬롯 3 = kind 3, 사거리 8). 탱커 뒤 `(86,35)`, `(86,41)` 근처 2마리. 타입이 `CASTER`라 타입 수업 뒤 우선 대상이 된다. 공격력은 영웅 HP를 10초 안팎에 50% 아래로 떨어뜨릴 정도로 잡는다.
  - (선택) **원거리 물리**: 역할 2(kind 2) 1마리. 첨부2의 1순위(원거리 물리)가 실제 필드에서 보이도록 넣는 것을 권한다.
  - **원거리 소환형 보스**: 광장 안쪽 `(90,40)`. 원거리 공격과 소환 패턴(`BossCombat.Summons`)을 가진 패턴을 고른다. 소환물은 `SUMMONED` 등급이다(현재 `SummonKind`는 로스터에 따라 N01로 대체될 수 있음).
- 영웅이 쓰러지지 않는 기존 규칙(체력 0이면 다시 채움)은 유지한다. HP 50% 판정은 이 보호보다 먼저 걸린다. 자동 물약(`survival.potionHpPercent`, 기본 40%)이 50% 판정 전에 마시지 않는지 확인한다.
- 보스가 너무 빨리 죽거나 너무 오래 걸리지 않게 체력을 맞춘다. **고블린이 등장하는 순간부터 영웅이 돌아올 때까지(`GoblinAppear`~`Pickup`) 보스는 공격·소환을 멈추고 자리를 지키게** 한다(장면이 읽히게, 보상 줍기 중 영웅이 맞지 않게). 쓰는 방식(쿨다운 고정, 행동 대기 등)은 기존 보스 상태에 맞춰 고른다.

## 5. 시작 칙령 값 (튜토리얼 시작 때 `GuideTutorial.BarePowers`에서 미리 설정)

2구간 계획의 `position.engage=STAND`, `position.engageApproach=WALK`에 더해 아래 값을 정한다. 앞 구간 진행에는 영향이 없다(보스·고블린·전리품이 3구간에만 나온다).

| 옵션 | 시작값 | 이유 |
| --- | --- | --- |
| `target.priorityGrade` | `NORMAL,BOSS,ELITE,SUMMONED,TREASURE` | 사용자 지정 "등급 기본 = 일반 몬스터". 수업 T1에서 첨부1(`NORMAL,SUMMONED,ELITE,BOSS`)로 바꾼다. `TREASURE`는 맨 끝에 두고 화면에서는 숨긴다(7절) |
| `target.priorityType` | `MELEE,CHARGER,RANGED,CASTER,SUPPORT,EXPLODER`(기본값) | 사용자 지정 "타입 기본 = 근접 공격 타입". 그래서 처음에는 근접 탱커를 친다 |
| `target.priorityStatus` | 기본값 그대로 | "상태 타입 = 상관 없음": 판정 순서에서 마지막이라 결과에 거의 영향이 없다 |
| `target.priorityCategories` | `GRADE,TYPE,STATUS`(기본값) | 등급을 먼저, 다음 타입 |
| `target.goblin` | `OFF` | 기본은 켜짐("보물 고블린 최우선"). 켜져 있으면 등급 수업 없이도 고블린을 쫓아서 수업 G가 의미 없어진다. `target.goblinChase`(끝까지 추적)는 켜 둔다 |
| `common.order` | `ATTACK,DODGE,BUFF,PRIORITY_LOOT,OBJECTIVE,REGROUP,EXPLORE` | 첨부3 시작 상태. 공격이 회수보다 위라 고블린 보상을 두고 보스에게 돌아간다(`PickupDuringCombat` false) |

시작값이 기존 저장 검증(`HuntEdictLoadout.ValidateForHero`, 순서 옵션 정규화)을 통과하는지 확인한다.

## 6. 시뮬레이션 쪽 작업 (`GuideTutorial.cs`, `CombatSimulation.Guide.cs`)

- 단계 상수를 2구간 뒤에 이어서 추가하고 `Cleared`와 `Last`를 맨 끝으로 옮긴다. `Waiting()`에 `TargetTalk`, `GoblinTalk`, `LootTalk`, `LootDoneTalk`을 더한다. `Tutorials.NormalizeShare`의 단계 범위 검사는 2구간 계획대로 `GuideTutorial.Last`를 쓴다.
- `GuideOutcome` 전환:
  - `FinalFight`(2구간) 끝 → `Road3`.
  - `Road3`: 3구간 적이 보이면(12.5m, `Map.LineClear`) → `MixedFight`(멈추지 않음). 적의 `lastSeen` 갱신은 이때부터.
  - `MixedFight`: `State.health < Stats.hp*0.5` → `TargetTalk` + 정지.
  - `TargetFight`: 보스·고블린을 뺀 적이 모두 쓰러지면 → `BossFight`. 보스가 그 사이 소환하면 소환물도 "보스 외 적"으로 센다.
  - `BossFight`: 보스 체력 80% 미만 → 고블린 소환 + `GoblinAppear`.
  - `GoblinAppear`: 고블린이 정해진 지점을 지나면 → `GoblinTalk` + 정지. "화면에서 사라지기 전"은 카메라 기준이므로, 세로(440×956)와 가로(956×440) 모두에서 고블린이 아직 화면 안에 있는 월드 지점을 스모크 스크린샷으로 정한다.
  - `GoblinChase`: 고블린이 죽으면 → `ReturnToBoss`.
  - `ReturnToBoss`: 고블린이 죽은 뒤 영웅이 보스 쪽으로 2초(`GuideTutorial.LootLessonSeconds`, 2026-10-20 변경, 21일 3초→2초; 이전에는 0.5~1.5초라 보상 바로 옆에서 시작했다) 더 나아간 다음 → `LootTalk` + 정지. 보상이 아직 남아 있어야 한다.
  - `Pickup`: 고블린 보상이 모두 회수되면 → `LootDoneTalk` + 정지.
  - `FinalBoss`: 보스가 죽으면 → `Cleared` + `Finish(true,...)` + 정지.
- `ContinueGuide`: 각 대화 단계는 해당 수업의 완료 조건이 참일 때만 다음으로 간다(`TargetTalk`→`TargetFight`: 등급·타입 둘 다 완료, `GoblinTalk`→`GoblinChase`: `TREASURE`가 등급 1순위, `LootTalk`→`Pickup`: `common.order` 첫 항목이 `PRIORITY_LOOT`, `LootDoneTalk`→`FinalBoss`: 조건 없음).
- **고블린 연출용 등장**: 기존 `SpawnGoldenGoblin`은 확률·방 선택이 들어 있으니 쓰지 않는다. 튜토리얼 전용 소환을 만든다. 같은 `EnemyState`(`goblin=true`, `State.goblin` 상태 포함)를 쓰되, 위치는 영웅 왼쪽 화면 밖 근처에 두고 `GoblinAppear` 동안은 오른쪽 출구 지점으로 직선 이동시킨다(기존 도주는 영웅 반대 방향이라 오른쪽으로 가지 않을 수 있음). 수업 G 뒤에는 기존 도주·탈출 규칙(20초 뒤 2초 탈출)을 쓰되, 영웅이 따라잡을 수 있게 속도·체력·남은 시간을 튜토리얼 상수로 맞춘다. 영웅이 놓치면 진행이 막히므로, 튜토리얼에서는 탈출을 끄거나 탈출 직전에 멈춰 기다리게 하는 안전장치를 둔다.
- **고블린 보상**: `RewardGoldenGoblin`은 `State.training>=0`이면 돌아가 버린다. 튜토리얼(`IsGuide`)에서는 보상을 바닥에 떨어뜨리게 바꾼다. 실제 계정 지급으로 할지, 회수만 보여 주고 지급하지 않을지는 아래 9절 결정. 떨어진 전리품이 `PRIORITY_LOOT`의 회수 대상(`FindEdictLoot(true)`가 찾는 우선 회수 전리품)에 해당하는지 반드시 확인한다. 해당하지 않으면 수업 L이 아무 효과가 없다. 필요하면 튜토리얼 드롭의 등급을 올리거나 회수 정책 시작값을 맞춘다.
- 영웅 보호, 길 걷기(`MoveGuideWalk`), 단계별 waypoint 처리는 1·2구간 방식을 이어서 쓴다.

## 7. 공개·편집 권한과 '보물 몬스터' 숨김

- 새 계정은 이 옵션들이 숨겨져 있다. `TargetTalk`에 들어갈 때 `target.priorityGrade`, `target.priorityType`, `target.priorityStatus`, `target.priorityCategories`를, `LootTalk`에 들어갈 때 `common.order`를 2구간 계획의 `UnlockGuideEdict` 거래로 공개한다(거래 뒤 `Data.guide.tutorialRun`을 살아 있는 런으로 다시 묶는 것 포함).
- 편집 가능(`HuntEdictProgression.Direct`): 공격 대상 카드는 `TargetEditable`=`Direct`가 false면 이동 버튼이 꺼진다. 2구간 계획에서 `position.engage`, `position.engageApproach`를 Direct 목록에 넣기로 했다. **3구간까지 합치면 튜토리얼이 가르치는 옵션 목록을 한곳에 두는 편이 낫다.** 예: `GuideTutorial.TaughtOptions = {position.engage, position.engageApproach, target.priorityGrade, target.priorityType, target.priorityCategories, common.order}`를 두고, `Direct`는 이 목록의 옵션이 공개돼 있으면 true. 2구간의 Direct 목록 추가는 이것으로 대체해도 된다.
- 공격 대상 패널의 다른 탭(추적 거리·시간, 보물 고블린 추적)은 옵션이 숨겨져 있으면 빈 화면이다. 수업 중에는 가림막이 막으므로 그대로 두되, 빈 탭이 보기 싫으면 숨겨진 탭 버튼을 감춘다(선택).
- **'보물 몬스터' 카드 숨김(첨부1)**: 튜토리얼 중 수업 G 전까지 등급 목록에서 `TREASURE`를 표시하지 않는다.
  - 공개 키를 하나 정한다(예: 계정 `edictUnlocks`의 `"seen-treasure-grade"`). 기존 `seen-core` 같은 키와 같은 방식이다.
  - 숨김 조건: 계정이 아직 튜토리얼 중(`Tutorials.Mandatory`)이고 그 키가 없을 때. 졸업 뒤(모든 기능 개방)와 튜토리얼을 거치지 않은 계정에서는 항상 보인다.
  - `AddTargetOrder`에서 숨길 코드를 빼고 그린다. 순서 변경은 `HuntEdictReorder.MoveVisible(order, visible, item, to)`의 **보이는 목록 인자**로 숨긴 코드의 자리를 보존한다(지금은 `MoveVisible(order,order,…)`로 호출 중). 몬스터 머리 위 순번 미리보기(`PreviewTargetOrder`)도 같은 보이는 목록을 쓴다.
  - **등장 연출**: `GoblinTalk`에서 키를 공개한 뒤 등급 목록을 처음 그릴 때 `TREASURE` 카드만 서서히 나타나며(투명도·크기) 금색 테두리가 잠깐 반짝이게 한다(예: 기존 `ForgeWorkingPulse` 재사용). 한 번만 재생한다.

## 8. HUD 수업 안내 (가림막 단계)

2구간 계획 4절의 일반화된 안내기를 확장한다. 목표 패널까지의 길은 같은 규칙으로 찾는다.

1. 펼침이 닫혀 있으면 `globalHud.EdictButton` — "사냥 칙령 버튼을 눌러 주세요."
2. 시트가 없으면 `edict-sheet-tab-combat` — "전투 아이콘을 눌러 주세요."
3. 다른 패널이 열려 있으면 그 패널의 뒤로 버튼(`edict-position-back` / `edict-target-back` / `edict-order-back`) — "목록으로 돌아가 주세요." 지금은 전투 탭을 열면 첫 그룹(위치) 패널로 바로 들어가므로 이 단계가 자주 쓰인다.
4. 목록이면 목표 그룹 칩 `edict-sheet-group-{index}` — 그룹 index는 `HuntEdictHudRows.Groups("combat",…)`에서 `target.priorityGrade`(공격 대상 선택) 또는 `common.order`(행동 우선순위)를 가진 그룹으로 찾는다. 문구: "'공격 대상 선택'을 눌러 주세요." / "'행동 우선순위'를 눌러 주세요."
5. 공격 대상 패널에서 탭이 '우선 순위'가 아니면 `edict-target-tab-0`. 판정 순서 칩이 목표 범주가 아니면 `edict-target-category-GRADE`(또는 `-TYPE`) — "'몬스터 등급'을 눌러 주세요." / "'몬스터 타입'을 눌러 주세요."
6. 순서 맞추기: 저장된 순서와 목표 순서를 비교해 **다음 한 번의 이동**만 가리킨다. 끌기 대신 ▲/▼ 버튼(`edict-sheet-up-…`/`edict-sheet-down-…`)을 대상으로 한다(가림막 구멍 안에서 확실히 눌린다).
   - T1(등급, 보이는 순서 기준): 목표 `NORMAL,SUMMONED,ELITE,BOSS`. 시작 `NORMAL,BOSS,ELITE,SUMMONED`에서 보스 ▼ 두 번 → 소환된 적 ▲ 한 번. 문구: "'보스 몬스터'를 아래로 내려 주세요." / "'소환된 적'을 위로 올려 주세요."
   - T2(타입): 목표는 1·2순위가 `RANGED`, `CASTER`(나머지 무관). 원거리 물리 ▲를 1순위까지, 원거리 마법 ▲를 2순위까지. 문구: "'원거리 물리 공격 타입'을 맨 위로 올려 주세요." / "'원거리 마법 공격 타입'을 2순위로 올려 주세요."
   - G(보물): 연출이 끝난 뒤 `TREASURE` ▲를 1순위까지. 문구는 3절.
   - L(행동): `PRIORITY_LOOT`(회수) ▲를 1순위까지. 그다음 `edict-sheet-close`를 가리켜 직접 닫게 한다 — "창을 닫아 주세요." 닫히면 펼침도 닫고 `ContinueTutorial()`.
7. 완료 뒤 안내 문장(③~⑥ 등)은 **패널과 몬스터 머리 위 순번이 보이는 상태로** 읽혀야 한다. 대화창(`StoryDialogueWindow`)이 아래 1/3을 덮어 패널을 가리면, 가림막 상단 카드(`PrologueGate`의 화자 카드)에 한 줄씩 띄우고 탭하면 다음 줄로 넘어가게 한다. 이 "대상 없는 안내 단계"가 `PrologueGate`에 없으면 최소 기능으로 추가한다(카드 + 탭으로 다음). ⑤를 띄울 때 순번 라벨 하나에 링(`TutorialAnchorRing`)을 씌우면 "머리 위 숫자"가 무엇인지 분명해진다(권장).

## 9. 결정이 필요한 것 (구현 전 사용자에게 확인하거나 기본값으로 진행)

| 항목 | 기본값 제안 |
| --- | --- |
| 고블린 보상을 실제 계정에 지급할지 | **지급한다**. 기존 전리품 회수·저장 경로를 쓰고, 튜토리얼 런에서 그 경로가 막혀 있으면 열어 준다. 안 되면 회수 연출만 하고 지급은 하지 않는다(그 사실을 기록) |
| 3구간 적 종류 | 위 4절(필드 0 kind 0/3/2와 소환 패턴 보스). 몬스터 로스터 파이프라인으로 새 모델이 생기면 나중에 교체 |
| 패널을 닫을지 | T 수업은 자동으로 닫고(사용자: "창이 닫히고 전투 시작"), L 수업은 플레이어가 직접 닫는다(사용자: "창 닫기") |

## 10. 테스트와 검증

작업 중에는 관련 검사, 마지막 코드 변경과 `main` 통합 뒤 전체 Edit Mode와 macOS 스모크를 한 번(`CLAUDE.md`).

- `GuideTutorialTests` 확장(시뮬레이션 + 저장소):
  - 시작값(5절)이 적용되고 저장 검증을 통과한다.
  - `MixedFight`에서 처음 대상이 근접 탱커이고, HP 50% 미만에서 `TargetTalk`로 멈춘다.
  - T1·T2 완료 전에는 계속이 막히고, 완료 뒤 첫 대상이 장판(`CASTER`) 또는 원거리 적으로 바뀐다(`EdictTargetPolicy`로 확인).
  - 보스만 남으면 `BossFight`, 80% 미만에서 고블린이 왼쪽에 생기고 오른쪽으로 이동하다 `GoblinTalk`로 멈춘다.
  - `TREASURE`가 1순위가 되기 전에는 계속이 막히고, 1순위가 되면 영웅이 고블린을 쫓아 처치한다. 고블린이 도망쳐 진행이 막히지 않는다.
  - 고블린 보상이 바닥에 남은 채 영웅이 보스 쪽으로 가다 `LootTalk`로 멈춘다(`PickupDuringCombat` false).
  - `PRIORITY_LOOT`를 1순위로 저장하면 보상을 모두 줍고 `LootDoneTalk`, 그 뒤 보스를 처치하고 `Cleared`, 졸업까지 간다. 기존 졸업 단언 유지.
  - `TREASURE` 숨김: 튜토리얼 중 키 없음 → 보이는 등급 목록에 없음, 키 공개 뒤 있음, 졸업 뒤 항상 있음. 숨긴 상태에서 순서를 바꿔도 `TREASURE` 자리가 보존된다.
  - 체크포인트 정규화가 새 마지막 단계까지 통과한다.
- 관련 묶음: `HuntEdictProgressionTests`, `HuntEdictHudRowsTests`, 공격 대상·행동 우선순위 관련 테스트(`EdictTargetPolicy`, `HuntEdictReorder`, 행동 순서 테스트), `LocalizationTests`, `StoredLocalizationTests`.
- `python3 tools/check_ui_contract.py --verify`, `python3 tools/check_ui_refresh.py`.
- `RuntimeGuideTutorialSmoke`를 3구간까지 이어서 실제 포인터로 가림막을 따라간다. 단계별 캡처에서 특히 다음을 직접 본다.
  - 수업 T 동안 몬스터 머리 위 순번이 바뀌는 장면.
  - '보물 몬스터' 카드 등장 연출.
  - 고블린 머리 위 1.
  - 고블린이 화면 안에 있는 채로 멈추는지(세로·가로).
  - 회수 1순위 뒤 줍기.

  최소 440×956 한국어, 956×440 영어. 영어 화면에 한글이 남지 않는지 확인한다.

## 11. 하지 말 것

- 공격 대상·행동 우선순위 패널을 새로 만들거나 다시 디자인하지 않는다.
- `edictLegacyAccess`나 `Details`를 튜토리얼 중에 주지 않는다. 수업에 필요한 옵션만 공개한다.
- '보물 몬스터'는 수업 G 전에는 보이지 않게 한다(숨김은 튜토리얼 중에만).
- '스킬 사용' 접근 방식, 눈 가림·시간 정지 같은 설정을 대사에서 말하지 않는다.
- 1·2구간 대사와 흐름은 바꾸지 않는다(졸업 시점만 뒤로 밀린다).
