# 전투 구현 명세 (Codex 전달용)

작성일: 2026-10-12 · 대상: Codex 구현 작업 · [English](D4_Skill_Tree_Combat_Spec_20261012.en.md)

작성: Claude (명세와 조사). 구현·검증·커밋은 Codex가 한다.

이 문서는 계획 문서 [`D4_Skill_Tree_Plan_20261012.md`](D4_Skill_Tree_Plan_20261012.md) 8장(전투)을 구현할 수 있는 수준으로 풀어 쓴 것이다. 근거는 코드를 읽기만 한 조사 기록(`Docs/Design/D4SkillTree/research/combat_{warrior,ranger,mage}.md`, `save_migration.md`, `dependents.md`)이다. **줄 번호는 `main` `a3fe67c5` 기준**이라 코드가 바뀌면 어긋난다. 함수·클래스 이름으로 다시 찾는다. 수치의 원본은 `skill_tree_d4.json`과 `golden/rank_values.json`이고, 규칙·결정은 계획 문서가 우선한다.

## 0. 범위

- 들어간다: 액티브 30종(직업당 10, 궁극기 2 포함)의 랭크별 수치, **보조 스킬 84종**, 패시브 24종과 **핵심 패시브 6종**, 궁극기 3랭크, 흡수·삭제된 옛 스킬 정리, 자동 전투와의 만남.
- 들어가지 않는다: 스킬창 화면(UI 명세), 저장·이전(계획 7장, 2단계), 아이콘(3단계), 튜토리얼·칙령·빠른 편집(6단계). 이 문서는 그 단계들이 **전투 쪽에 요구하는 것**만 적는다.
- 전투 변경은 모두 `SkillTreeD4.Enabled` 뒤에서 켜지고, 7단계 전까지 출시 경로는 바뀌지 않는다.

**보조 스킬 84종 분류 결과** (분류 정의는 3장):

| 직업 | A (수치만) | A* (보조 값 접근자 필요) | B (기존 경로에 새 동작) | C (새 장치·상태) | 합계 |
|---|---|---|---|---|---|
| 전사 | 6 | 0 | 18 | 4 | 28 |
| 궁수 | 1 | 1 | 19 | 7 | 28 |
| 마법사 | 0 | 3 | 21 | 4 | 28 |
| **계** | **7** | **4** | **58** | **15** | **84** |

## 1. 새 체계가 전투에 요구하는 것 (요약)

1. 본 스킬은 **랭크 1–5**(궁극기 1–3), 랭크별 수치는 `from → to` 선형이다(`golden/rank_values.json`이 정답).
2. 스킬마다 보조 스킬 **하나**를 고르고(하나만 효과가 켜짐), 그 랭크는 `min(본 랭크, 최대)` 이하다. 보조 스킬 효과는 **본 스킬의 형태나 부가 효과**를 바꾼다.
3. 패시브는 영웅 레벨로만 해금되고(전투 중 별도 조건 없음), 핵심 패시브는 둘 중 하나만 점수를 가진다.
4. 장착한 4개 액티브 + 궁극기 1개만 시전된다. 배우지 않은 스킬(랭크 0)은 장착할 수 없다.
5. 영웅 레벨 상한 60, 포인트 = 레벨(계획 3장).

## 2. 공통 틀 (먼저 하는 일: 단계 5.0)

직업별 작업(5a·5b·5c)은 이 공통 틀이 끝난 뒤에 한다. 공통 틀은 하나의 PR이다.

### 2.1 두 엔진

| 엔진 | 스킬 | 실행 경로 | 수치 출처 |
|---|---|---|---|
| **레거시 영웅 행동** | 전사 W01–W06, 궁수 A01–A06, 마법사 M01–M04·M06(+패시브 01–06) | `TryClassSkillCore` → `StartHeroAction`(`Act:49-82`) → `ResolveSkill`(`Sim:291-353`) | `GameCatalog.cs`, `SkillEffects.cs`, **리터럴**. `ClassSkills.json`을 바꿔도 전투가 안 바뀐다 |
| **네이티브 시전** | 전사 W08·W09·W17·W18, 궁수 A07·A14·A17·A18, 마법사 M07·M08·M11·M17·M18, 흡수된 옛 스킬 | `ClassSkillCast` → `ResolveClassSkill`(`Fx:93-163`) → `ClassSkillEffect` 틱(`Fx:167-271`) | `ClassSkills.json`(`catalog.json` + `runtime_parameters.json`) |

새 30종 중 17종이 레거시, 13종이 네이티브다. 레거시 스킬의 **랭크 곡선은 코드 안에 있다**(`SE`, `Cost()`): 비용(예 W01 6→4, W03 25→21, M01·M03 25→21), 지속(블리자드 6→8), 기절(노바 1.5→1.9), 재사용(M04) 같은 값은 지금 랭크 항이 없다. 이 값들은 `GameCatalog.cs`·`SkillEffects.cs`가 sha256으로 고정된 파일(`tools/class_skills.py`)과 Balance1000 해시(`tools/balance_1000.py:222-231`)에 걸린다: 고치면 **지문을 같은 PR에서 갱신**하고 `python3 tools/balance_1000.py`를 다시 돌린다.

### 2.2 수치의 출처와 랭크

- **데이터 주도.** 스킬·보조 스킬·패시브의 랭크별 값은 `SkillTreeD4.json`(`value: {from, to, unit}`)에서 읽는다. 공식(`×(1+0.1(r−1))`)을 코드에 두지 않는다. 지금은 대부분 이 공식이 우연히 맞지만 맞지 않는 값이 있다(4장). `golden/rank_values.json`이 랭크마다의 값(선형, 개수 단위 `명 회 발 개`는 `floor`)을 정답으로 준다.
- **본 스킬 피해는 본 랭크**, **보조 스킬의 값은 보조 랭크**의 `from→to`다(계획 5.4). 보조 값이 본 랭크 배율을 다시 곱하지 않는다. 네이티브 `SkillHit`·`Dot`·`Placement`는 `a.rankScale`을 곱하니(`Fx:12-13,40,84`) 보조 값은 **곱하지 않는 경로**(별도 `source` 또는 우회 인자)로 넣는다.
- **궁극기(세 직업 모두)**: `RankScale`이 궁극기에서 1로 고정이다(`Cat:65`). `1 + 0.2(r−1)`(랭크 3 = 1.4)로 바꾼다. 영속 검증의 상한(`ClassSkillPersistence.cs:37`, rankScale ≤ 1.401)이 맞는다. 쿨다운 60/55/50은 두 궁극기가 **공유 타이머**(하한 45초, `Sk:163`, 검증 45–60 `Pers:26`) 안에서 정한다.
- 장비 +1 랭크(`HeroStats.Blacksmith.cs:17`)는 패시브·궁극기·보조 스킬에 **적용하지 않는다**(계획 D5).
- 레거시 `rank` 규칙: 라벨이 `"M0x"`인 적중은 기본 랭크(`runeSkillPower` +10%/랭크, `Sim:494`, `Dmg:66-67`)를 먹고 `"EXTRA:M0x"`는 먹지 않는다. 시안이 보조 랭크를 요구하는 곳(M04-1 등)은 라벨을 바꿔야 한다.

### 2.3 확장 지점: `ClassSkillCast`

옵션(`ClassSkillOptions`, 55묶음·166선택)은 **언제·누구에게·어디 서서**만 바꾸고 **무엇**은 바꾸지 않는다. 보조 스킬을 옵션에 실을 수 없다(랭크 필드가 없음, 55개 상한과 기본값 정리, 프리셋 복사 규칙). 대신:

1. **저장**: `SkillInvestment`에 `modifier`(−1..2, 궁극기 −1..1)와 `modifierRank`를 더한다(v5). `JsonUtility`는 없는 키에 필드 초기값을 유지하므로 `modifier = -1`이 옳은 기본이다(0은 "보조 1 선택"). `Rank(id)`는 v5에서 **없는 id에 0**을 돌려준다(지금은 1).
2. **시전 객체**: `ClassSkillCast`(`ClassSkillState.cs:28-38`, 저장됨)에 `mod`(1부터, 0 = 없음이라 옛 저장이 그대로 유효)와 `modRank`를 더하고, **한 번** `CaptureClassCast`(`Ps:20-46`)에서 채운다. 두 엔진이 모두 이 함수를 거친다(네이티브 `Sk:161`, 레거시 `Sk:223`의 `OriginalClassCast`). 검증은 `rankScale` 옆(`Pers:37`).
3. **읽는 쪽**: 효과 코드는 `Cast(root)`(`Sk:54`)로 시전 객체를 찾아 `a.mod`/`a.modRank`를 본다. **결정 쪽**(플래너, 비용, 예보, 조준, 자동 시전)은 시전 객체가 없으니 `Loadout.Modifier(id)`(`Rank()` 옆, `Sk:84`)를 읽는다. 시전 객체가 null이거나(30초 정리, `Sk:218`) 시전보다 오래 사는 효과(`Ps:124`)는 **만들 때 수치를 복사**한다(`Placement`가 선례, `Fx:84`).
4. **수치 덮어쓰기**: `N(id,key)`(`Sk:28`) 위에 보조 값을 덮는 `SkillTreeD4Values.Modifier(id, key, rank)`.
5. **시험 틀**: 비공개 메서드를 반사로 부르는 시험이 많다(위치 인자, `Single(name)`: `RT:17-22`, `PT:21`). **오버로드를 만들지 않고** 선택 인자를 뒤에 붙인다.

### 2.4 효과의 id와 정리 규칙 (가장 흔한 함정)

- 장착하지 않은 id의 효과·DoT·보호막은 **매 틱 지워진다**(`ResetClassSkillEquipment`, `Sk:237-239`). 흡수된 옛 스킬(W07·W10–W16, A08–A13·A15·A16, M05·M09·M10·M12·M14·M16)은 새 체계에서 **절대 장착되지 않는다**. 보조 스킬 효과는 새 id를 만들거나 **기본 스킬 id를 `source`로** 달아야 한다. 같은 이유로 `InspectClassSkill`의 `NOT_EQUIPPED`(`Sk:122`)와 `Reduce`(`Sk:72`), 진행 중 시전 취소(`HuntEdictChanges.cs:46`)와 이어하기 검증(`Pers:39-40`)의 "장착 여부" 검사도 보조 스킬 효과를 **부모 + 보조**로 판단하도록 바꾼다.
- 보조 스킬 효과가 하는 **2차 피해**(분기, 튕김, 메아리, 포탑)는 `DamageKind.Set`, id `EXTRA:<원본>`이다: 치명 없음, `Native`가 아님, 직접 적중이 아님(`Fx:12`, `Ps:101,121`). 기존 "다른 스킬이 아니다" 규칙을 지킨다.
- **하드코딩된 id**가 많다(`"W07"`, `"M12"`, `"A11"`, `"M05"` …). 흡수된 몸체를 재사용할 때 리터럴을 보조 스킬 판정으로 바꾼다. 각 행의 "함정" 칸에 위치가 있다.
- 장비·세트 후크 `DES_LW41–45`, `DES_LM41–46`은 흡수된 스킬에 달려 있다. **그 보조 스킬을 고른 경우에만** 적용한다(계획 12장).

### 2.5 자동 전투와 예보

- 자동 시전: `TryAutomaticClassSkill`(`Cast:179-201`)이 `Loadout.order`를 돈다. 레거시 스킬의 조건은 항상 참이고 실제 판단은 칙령 플래너(`PlanEdictWhirlwind/Leap/IronWall/Shout/Slam/…`, `Rules:159-262`)에 있다. 네이티브 스킬의 조건은 `ClassAutoDefaultCondition`(`Au:97-147`)이다.
- **예보와 해제가 같아야 한다**(`Identity:107-117` 시험이 강제). 모양을 바꾸는 보조 스킬(W03-1 원뿔 → 원, W04-2 원 → 직선, M01-1 메테오, M03-2 반사 …)은 **플래너와 예보도 함께 바꾼다**.
- **계획 8.5의 주의점**: 정지해서 겨냥하는 보조 스킬(A05-1), 설치물(A03-3 포탑, M11-2 구슬 3개), 따라다니는 지대(M02-1), 분신(A18-1)은 자동 이동과 부딪친다. 각 항목의 자동 전투 동작을 구현 전에 정하고 시험으로 고정한다(8장의 기본값).

### 2.6 저장과 이어하기

- 보조 스킬의 `mod`/`modRank`는 `ClassSkillCast`에 있어 **진행 중 시전과 효과가 이어하기 뒤에도 같은 모양**이다(`ClassSkillRuntimeState`가 저장). 영속 검증(`ClassSkillPersistence`)의 허용 목록(`Pers:42,47-50`: 의도 id는 W02/M13만, 지속 효과는 M13 채널·W15 대여만)을 새 지속 효과(A03-3 포탑, M11-2 구슬, W18-2 형태 버프 등)에 맞춘다. 없는 id는 이어하기에서 예외를 던져 **저장된 런이 읽히지 않게** 된다.
- 마이그레이션에서 진행 중인 런이 있으면 안전한 쪽으로 처리한다(저장 조사 F1, 계획 17장 새 결정).
- 통계·로그의 skill id 문자열(`statistics.skills`, `damageEvents`, `procHits` …)은 로드 때 검증하지 않는다. 보조 스킬 효과의 `definitionId`는 기본 스킬 id를 쓰고 보조 번호는 별도 필드로 둔다.

### 2.7 시험 틀

- 기본 틀: `ClassSkillRuntimeTests.Fixture(skill, passive, equipment)`(`RT:23-41`). 레벨 40 영웅(**레벨 60 영웅으로 바꾼다**), 장착 `[skill]`, 100000 HP 정지 적 하나(2m 앞), `Stats`는 피해 100·HP 1000·치명 0으로 덮어쓰고, `decisionTime = 100000`이라 **수동 시전만** 돈다, 시드 42. 도우미: `Advance`(`RT:42`), `CastSkill`(`RT:44`), `Use`(`PT:24`), `AddSkill`(`RT:49`), `ExtraEnemy`(`GT:12`), `Begin`/`Land`/`Status`(`PT:12-23`).
- 레거시 경로: `T/ActionTests.cs:12-24`, `SkillRankEffectTests.cs:17-33`(`ContentTestAccounts.Legacy()`). 투사체·덫 개수와 예보 = 해제 시험이 여기 있다.
- 결과 읽기: `State.damageEvents`(`definitionId`, `kind`, `critical`, `finalDamage`, `rootCastId`), `effectEvents`, `classSkills.{casts,effects,dots,cooldowns}`, `projectiles`, `traps`, `shields`, `ClassSkillChanged` 스트림.
- **결정성**: `Hit`은 항상 `State.rng`를 뽑는다(`Dmg:70`). 새 굴림은 이후 굴림을 밀므로 치명 0과 같은 시드로 비교한다.
- 마법사 시험은 `Stats.passives = new bool[6]`(RT:38)이 MP05·MP06을 끈다는 점을 알아야 한다(`sim.Stats.passives[4|5] = true`로 켜거나 레거시 마법사 틀 `PassiveTests.cs:21-33`).
- **보조 스킬 한 가지당 시험 한 가지(쌍 시험)**: 같은 틀에서 보조 없음(`modifier = -1`)과 있음을 돌려 차이를 단언한다.
```csharp
// helpers are real; r.modifier / r.modifierRank are the new v5 fields
int HitsOnSide(int modifier){
  var s=Fixture("W09");var r=s.State.build.classSkills.ranks.Single(x=>x.id=="W09");
  r.modifier=modifier;r.modifierRank=modifier<0?0:1;
  var side=ExtraEnemy(s,s.State.position+Vector2.right*2);Call(s,"Sense");
  Assert.IsTrue(CastSkill(s,"W09"));Advance(s,.6f);
  return s.State.damageEvents.Count(d=>d.definitionId=="W09"&&d.kind==DamageKind.Direct&&d.targetId==side.id);
}
[Test] public void SpinningRendReachesBesideTheCone(){Assert.AreEqual(0,HitsOnSide(-1));Assert.AreEqual(1,HitsOnSide(2));}
```
- 깨질 시험(다시 쓰기 대상): `RT:16,54-66`(개수 37/16/19), `PT:11,36-96`, `Identity:88-150`, `SkillPresetCombatPreviewTests.cs:105-115`, `ClassSkillNativeSmoke.cs:47-66`, 능력 측정 도구 `ClassSkillPassiveMeasurement`(옛 id 이름, 빈 패시브 칸 요구가 v3+에서 금지).

### 2.8 보조 스킬 하나의 수용 기준 (모든 행 공통)

1. **수치**: 랭크 1과 최대 랭크에서 시안의 `from → to`와 같다(`golden/rank_values.json`).
2. **동작 차이**: 쌍 시험이 보조 없음과 있음의 차이를 보인다.
3. **예보**: 예보가 있는 스킬은 예보와 해제가 같다.
4. **자동 전투**: 자동 시전이 멈추거나 공회전하지 않는다.
5. **이어하기**: 시전·효과를 저장했다 복원해도 같은 결과다.
6. **정리 규칙**: 흡수된 id가 정리되어도 효과가 사라지지 않는다(2.4).
7. **옛 동작 보존**: 보조 스킬을 고르지 않았을 때의 본 스킬 동작은 시안의 "보조 없음" 기본 모양이다(예: M07은 화염 속성에 추가 효과 없음, 계획 D8).

## 3. 보조 스킬 84종

**분류**: **A** = 수치만 바꾼다(키·리터럴을 값으로). **A\*** = 수치지만 값이 **보조 랭크**에서 나온다(2.3의 접근자 필요). **B** = 기존 코드 경로 위에 새 동작을 얹는다. **C** = 새 장치·상태가 필요하다(상태 필드, 새 효과 종류, 새 판정). 약칭: `Sk` = `CombatSimulation.ClassSkills.cs`, `Fx` = `…ClassSkillEffects.cs`, `Au` = `…ClassSkillAuto.cs`, `Ps` = `…ClassSkillPassives.cs`, `Po` = `…ClassSkillPolicies.cs`, `Id` = `…SkillIdentity.cs`, `Sim` = `CombatSimulation.cs`, `Act` = `…Actions.cs`, `Dmg` = `…Damage.cs`, `Eff` = `…Effects.cs`, `Cs` = `…ClassSets.cs`, `Proj` = `…Projectiles.cs`, `Fc` = `…Forecast.cs`, `Aim` = `…EdictAim.cs`, `Cast` = `…ClassSkills.cs`. 나머지 약칭과 정확한 위치는 조사 기록 각 파일의 머리에 있다. **보조 스킬 번호는 시안 순서(1–3)이고 구현 보조 번호 `modifier`는 0–2다.**

### 전사 보조 스킬 28종

| ID | 이름 | 효과(랭크 1 → 최대) | 분류 | 구현 지점 | 함정 |
|---|---|---|---|---|---|
| `W09-1` | 피의 회수 | 대상당 HP 회복 1.0% → 2.0% | **B** | 3번째 시전마다 `CashDot`+`Heal`(W10 몸체 재사용). 시전 횟수는 `Counter()` | W10 후크(Au:110, Po:39,89)는 죽은 코드가 된다 |
| `W09-2` | 깊은 상처 | 중첩당 출혈 +20% → +40% | **B** | 한 명만 베고 출혈을 `Dot(additiveBank,cap)` 항목으로 쌓는다(최대 3중첩) | 중첩당 +20%가 합인지 곱인지 불명(질문 W-1) |
| `W09-3` | 회전 베기 | 출혈 지속 2초 → 3초 | **A** | `width` 90→360(`arc>=360`이면 각도 검사 생략), `duration` | `Po:53`, `Au:85`가 같은 키를 읽는다. 짧은 지속은 DPS를 올린다(`bank/duration`) |
| `W08-1` | 돌진 갈고리 | 도착 시 추가 자원 +4 → +12 | **B** | W11 이동기(prepare+.25초, `Lerp`)를 재사용해 적 앞 멈춤 지점으로. W08을 착지 목록에 추가 | 이동 중 걷기가 막힌다(Sim:364). 도착 시 추가 자원 |
| `W08-2` | 관통 갈고리 | 관통 대상 3명 → 7명 | **B** | `Line(origin,aim,6,width,3..7)`로 여러 명에 `SkillHit`, 마지막만 `Pull` | 폭 키를 새로 둔다 |
| `W08-3` | 낙인 갈고리 | 낙인 대상에게 받는 피해 −10% → −18% | **B** | W07 표식(`ApplyStatus(Mark,"W07",6)`)을 재사용. 처치 자원은 `ClassHitFlags` 새 비트 | 리터럴 "W07"(`HasAmplifyingMark` Sk:90, Ps:179, Au:51), DES_LW41(Fx:103). 새 표식이 이전 표식을 지운다(Eff:106-107) |
| `W01-1` | 빠른 이동 | 사용 중 이동속도 ×2.0 → ×3.0 | **B** | 이동 속도 ×2~3을 `SpeedWithBonus` 상한(1.5배) **뒤에** 적용(EdictWhirlwindMovement.cs:40, Sim:437) | 적보다 빨라져 0.5초 빈 틱 종료에 걸릴 수 있다 |
| `W01-2` | 넓은 범위 | 타격 범위 ×1.6 → ×2.0 | **A** | `RuneSkillRadius` 안에서 반경 배율(모든 호출이 이 함수를 지난다) | 걷는 쪽은 2.3~2.5m에 붙는다(EdictWhirlwindMovement.cs:29) |
| `W01-3` | 빠른 회전 | 피해 간격 ×0.7 → ×0.5 | **A** | 틱 간격 리터럴(Act:80, Sim:458)을 값으로 | 틱마다 비용·당김·치명 굴림: 간격 ×0.5면 초당 비용 2배. 비용도 함께 조정(질문 W-3) |
| `W03-1` | 대지 분쇄 | 폭발 반경 2.0m → 3.0m | **B** | 원뿔 → 원: 호 100→360, 반경 2→3(Sim:313, Forecast.cs:98, EdictAim.cs:20). `Cast.area` 활용 | 시안은 기본을 '단일 타격'(SD4:581)이라 쓰지만 구현은 100° 다중 타격(질문 W-4) |
| `W03-2` | 처형 일격 | 처형 기준 HP 20% → 30% | **B** | HP ≤ 임계에서 계수 ×2(WP06과 같은 판정, Dmg:31) | 환급량 미정 |
| `W03-3` | 여진 | 여진 피해 D40% → D80% ×2 | **B** | `ScheduleExtra` 'burst' ×2, +1초(Cs:111-114, Fx:233) | `Kind.Set`이라 WP15·WP17·전설 크레딧이 없다 |
| `W05-1` | 무쇠 자세 | 방어 시 반격 D80% → D120% | **B** | W12 반격 몸체(`AfterClassHurt` Ps:164-165, 2회·1초 간격)를 `OwnIronWall` 조건으로 | W12의 +15% 막기(Ps:192)는 시안에 없다 |
| `W05-2` | 수호의 맹약 | 제어 면역 1.0초 → 2.0초 | **B** | W16: `ClearHeroControl` + `Buff("W16:immune")`(Fx:120-126) | W16 특수 처리(Sk:190, EdictOrder.cs:132, Au:116)를 새 대상으로 |
| `W05-3` | 가시 갑옷 | 흡수 피해 반사 30% → 50% | **B** | `ReactShield`(Id:77-90)에서 흡수량 × 비율을 `Nearby(..,3)`에 2차 피해로 | 가시 능력치(Attributes.cs:115)와 별개. `IronWallAmount` 사전 검사가 리터럴 .3(EdictWarriorSupport.cs:10) |
| `W06-1` | 결집의 함성 | 이동속도 +20% → +40% | **A** | `ClassMoveBonus`(Ps:168-174)에 `shoutTime>0` 추가 | 총합 +50% 상한(Economy.cs:45). '군중 제어 감소'는 시안 behaviourKo에만 있다 |
| `W06-2` | 광폭의 함성 | 공격속도 +20% → +40% | **B** | `ClassAttackSpeedBonus`(Ps:193) 적용, 자원 충전 생략(Sim:325, EdictWarriorSupport.cs:85) | 공격 속도 상한 1.5(Act:57) |
| `W06-3` | 투쟁의 빚 | 즉시 추가 자원 +40 → +60 | **B** | W15 대여(`BorrowResource/RepayResource` Id:22-38) | 시안은 '6초 동안 자원 회복 정지', 구현은 대여·상환(질문 W-8). 검증기가 "W15"에 묶임(ClassSkillPersistence.cs:47-50) |
| `W02-1` | 전선 돌파 | 돌진 거리 3m → 5m | **C** | 경로 위 밀치기: `Line`+`BreakFront`(Id:11-21, W11 Fx:109-112) + 밀치기 폭 키 | 착지 피해 유지 여부, `HeroAirHeight`(Act:28) 끄기, `EdictLeapLandingValid` 최소 2m, '공간' 정책 의미(Po:177) |
| `W02-2` | 지진 착지 | 착지 여진 D60% → D100% | **B** | 착지(`ClassLanding` Ps:57-69)에 +2초 'burst'와 기절(`ApplyStatus`). 'burst'에 기절 분기 추가 |  |
| `W02-3` | 연속 도약 | 충전 대기 8초 → 6초 | **C** | 충전 개념이 없다. `State.cooldowns[]`를 읽는 곳이 많다(EdictWarrior.cs:79, Dmg:156-157, Legendaries.cs:111,128, ClassSkillReadModel.cs:24-25) | 재충전 8→6초 vs 쿨다운 6.4초(질문 W-5) |
| `W04-1` | 전장의 고리 | 끌어오기 반경 3m → 5m | **B** | `Pull`(Fx:17-36)을 `AreaHit` 앞에 | 반경 5m는 영웅 쪽 당김이 필요(W13은 1.5m) |
| `W04-2` | 전율의 파동 | 파동 길이 5m → 9m | **B** | 원 → 직선: `Line()`(Sk:95-100) + 감속(W14 Fx:118) | Forecast.cs:87-98, EdictSlam.cs:35, EdictWarriorSupport.cs:67, DES_LW44(Ps:44, Sk:250) 갱신 |
| `W04-3` | 연속 강타 | 1회 피해 40% → 60% ×3 | **B** | 시전 시 발구르기 + 0.6/1.2초 'burst' 두 번, 마지막에 기절. `a.origin` 기준 | `Kind.Set`이라 WP17 없음 |
| `W17-1` | 선조의 군단 | 선조 3명 · 지속 6초 → 8초 | **A** | `summons`, `duration`, `ticks` 키 |  |
| `W17-2` | 선조의 기술 | 따라 쓰는 피해 50% → 70% | **C** | 마지막 스킬 기록 필드(`ClassSkillRuntimeState`)와 재생 | 복사본은 `CaptureClassCast`를 거치면 안 되고(WP15 기록 Ps:25-26) `EXTRA:` id를 쓴다 |
| `W18-1` | 대지의 심판 | 범위 6m → 8m | **A** | `prepare`를 읽는 접근자(시전 객체가 생기기 전 Sk:159) | 영웅이 1.8초 묶인다(Act:26) |
| `W18-2` | 거인화 | 거인화 지속 6초 → 8초 | **C** | 형태 버프: 범위 +50%(`Cast.area`, W01/W02는 무시)와 밀림 면역(`PushHero`) | 지속 효과는 ClassSkillPersistence.cs:47-48 허용 목록에 추가 |

### 궁수 보조 스킬 28종

| ID | 이름 | 효과(랭크 1 → 최대) | 분류 | 구현 지점 | 함정 |
|---|---|---|---|---|---|
| `A07-1` | 속사 | 5발 · 발당 D25% → D33% | **A*** | `arrows`/`coef`(Fx:95,133). 피해 기준 E1 |  |
| `A07-2` | 흩뿌림 | 서로 다른 대상 3명 → 5명 | **B** | `Nearby(a.origin,range,n)`로 화살마다 다른 대상(적이 적으면 반복) | 해제는 주 대상만 검사(Cast:170). 각 적중이 A18에 연결(Idn:64) |
| `A07-3` | 마무리 화살 | 3번째 화살 치명 피해 +30% → +70% | **B** | 관통은 기존(`Arrow(...,pierce)`). 치명 강제·치명 피해 인자는 새 배관(Dmg:70,73, `SkillHit` Fx:9-13) | A04-3·정밀과 같은 배관을 쓴다 |
| `A14-1` | 연쇄 분열 | 2차 분열 피해 30% → 50% | **B** | `secondary` 화살이면 한 세대 더. 시전별 방문 집합(`Once`) |  |
| `A14-2` | 폭발 화살 | 폭발 반경 1.5m → 2.5m | **B** | `Splash(a,pos,radius,power,0,secondary:true,source)` | 고유 source 필요(Dmg:56은 'EXTRA:A14'를 Split로 표기) |
| `A14-3` | 귀환 화살 | 돌아오는 화살 D30% → D50% | **C** | 화살에 귀환 구간이 없다. M10 부메랑(Idn:91-117: `count` 구간, `targets.Clear()`, 재조준)을 복사 |  |
| `A01-1` | 도탄 | 벽 튕김 1회 → 3회 | **C** | 튕김 코드와 벽 법선이 없다(RiftLayout.cs:32-43). 법선 탐사, 튕김 횟수 저장(`CombatProjectile`), `hitIds` 규칙 | 예보(Forecast)가 예측하지 못한다 |
| `A01-2` | 가속 관통 | 관통당 피해 +10% → +20% | **B** | LA01 성장 재사용(Proj:85: 추가 = k×`hitIds.Count`)과 동률 처리(Aim:74) | 둘 다 `Stats.specials`에 묶여 있다 |
| `A01-3` | 세 갈래 | 화살 피해 50% → 70% ×3 | **B** | ±30° 3발 `LaunchMulti` 식 + 그림자 충전 1개 소모 | Fc:52-56, Aim:44-77 갱신. SAB4 메아리 상호작용 |
| `A02-1` | 탄막 | 7발 · 발당 D50% → D70% | **B** | 7발·120° | `{-angle,0,angle}`가 Fc:60, Aim:22, FOCUS(Aim:66-77)에 하드코딩 |
| `A02-2` | 튕기는 화살 | 튕긴 화살 피해 40% → 80% | **B** | 적중 시 가장 가까운 미적중 적에게 SAB4 `extra` 패턴(Set, 효과 없음) + '튕김' 플래그 |  |
| `A02-3` | 밀집 사격 | 같은 대상 추가 화살 +10% → +30% | **B** | LA02 `narrow` 경로가 이미 같은 일(Proj:30). `Stats.specials.Contains("LA02")` 검사들을 보조 스킬로 바꾼다 | 같은 대상 보너스는 `victim.hits`(Proj:84) |
| `A04-1` | 미끼 투영 | 미끼 어그로 2.0초 → 3.0초 | **B** | A11 효과(Fx:136, 적 AI, 만료 폭발)를 LA03 훅에서 `a.origin`에 생성. 도발 2→3초는 `tauntSeconds` | AP14가 리터럴 `Fx("A11")`를 읽는다(Pas:110) |
| `A04-2` | 연막 엄폐 | 받는 피해 −12% → −20% | **B** | `ClassLanding` A04 블록에서 `Buff("A12",3,guard)` | A12의 +20% 속도(Pas:170)는 시안에 없다 |
| `A04-3` | 그림자 걸음 | 다음 공격 치명 피해 +20% → +60% | **C** | 대상 뒤 착지가 없다(`EdictEscapePoint`는 후퇴 전용, `RangerLandingValid`가 적 근처를 거부). 다음 타격 치명 버프는 A07-3 배관 공유 | `RememberRetreatLanding`이 영웅을 걷게 한다 |
| `A03-1` | 가시 덫 | 속박 1.0초 → 2.0초 | **B** | 레거시 덫이 이미 발동 시 속박한다. 폭발 한 방 + 속박 1→2초, 독 반복 생략 | A09 이식이 필요 없다 |
| `A03-2` | 서리 올가미 | 둔화 30% → 50% | **B** | 원소 4→2, `Poisoned`→`Slow` 30~50%(Proj:144-145) | `OwnTrapPoison`과 AP10/AP15/독사 독 출처가 사라진다. A10은 일회성 폭발인데 시안은 지대(질문 R-3) |
| `A03-3` | 교차 쇠뇌 | 쇠뇌 사격 6회 → 10회 | **B** | `Placement(a,"ballista")`+`FireBallista` 재사용(Fx:141-143, Idn:44-52). 배치는 C | A15 키를 읽으니 간접이 필요. 포탑은 `ClassSkillEffect`(상한 1)라 `LiveRangerTraps` 사용처가 무시. 자동 배치는 덫 군집 논리(RgrT), A15 사격점 탐색 아님 |
| `A06-1` | 그림자 주입 | 처치 폭발 D40% → D80% | **B** | 처치 시 폭발: `EliteCascade`(Dmg:121-129)가 선례 |  |
| `A06-2` | 맹독 주입 | 4초 독 D60% → D100% | **B** | A13의 `Dot`(Fx:37-55)이 `ClassSkillCast`를 필요로 한다(레거시는 `Cast(root)`가 있다). 새 dot id | 미장착 id는 매 틱 정리된다(Cast:239) |
| `A06-3` | 서리 주입 | 둔화 20% → 40% | **C** | 적별 중첩 카운터 + `Freeze` | 보스는 제어 게이지만 오른다(Effects.cs:100-104) |
| `A05-1` | 응시 사격 | 조준 저격 D200% → D280% | **C** | 겨냥 단계가 필요(조준 중 자동 이동 정지, 방해 시 취소: 계획 8장). A08은 class-cast이고 '정지'가 `CS.action.id=="A08"`로 하드코딩(Cast:26, 207, Sim:363, Po:27-31,200) |  |
| `A05-2` | 사냥 준비 | 처치 시 다음 액티브 비용 −10% → −30% | **B** | A16 효과 재사용, 표식 대상 처치로 발동: 피해 전에 표식 상태를 잡고(Dmg:40-52 패턴) `enemy.dead`에서 |  |
| `A05-3` | 연쇄 표식 | 표식 이동 1회 → 3회 | **C** | 죽음 훅 + 점프 카운터가 필요: `StatusEffect`에 없고 `Counter()`는 만료가 없다 | `ApplyStatus`는 이미 표식을 옮긴다 |
| `A17-1` | 이동 포화 | 따라오는 파동 6회 → 8회 | **B** | `f.position=State.position`을 `Nearby` 앞에. 8파는 `ticks` 8·지속 4초 | 루프가 `until`로 묶인다(Fx:203) |
| `A17-2` | 집중 포화 | 좁은 범위 피해 ×1.6 → ×2.0 | **A** | `radius`→`f.radius`(리터럴 4), 계수 배율, 주기 .25 | `ticks` 6이면 1.5초에 끝난다 |
| `A18-1` | 그림자 분신 | 분신 피해 40% → 60% | **C** | 따라다니는 효과가 없다. `kind="clone"`을 만들어 `IdentityDirectHit`에서 직접 적중을 따라 한다 | `Set` 2차 적중은 재발동하지 못한다. Idn:64의 id 목록 수정 |
| `A18-2` | 잔상 폭발 | 잔상 폭발 D60% → D100% | **B** | 만료 폭발(구슬·미끼처럼) | 소멸(Idn:68)·상한 초과 제거(Idn:57)와 '사라짐'의 정의 |

### 마법사 보조 스킬 28종

| ID | 이름 | 효과(랭크 1 → 최대) | 분류 | 구현 지점 | 함정 |
|---|---|---|---|---|---|
| `M07-1` | 잿불창 | 연소 D60% → D100% | **A*** | 현재 몸체. `Dot`이 `a.rankScale`을 곱한다(Fx:40): 보조 값(D60→100)을 직접 넘긴다 |  |
| `M07-2` | 빙하창 | 빙결 0.6초 → 1.0초 | **B** | M09 몸체(Fx:149-150: 감속 .35/3초, 이미 감속된 적 빙결). 원소 2, 빙결 0.6→1.0초 |  |
| `M07-3` | 접지창 | 꽂힌 창 유지 4초 → 6초 | **B** | M12 몸체(Fx:153) + 도체 | 중계가 리터럴 "M12"에 묶여 있다(Id:136,142-144, Ps:148-152, Au:59-65,136, Po:65). 창이 `a.aim`에 박힌다 |
| `M01-1` | 유성 | 피해 ×1.4 → ×1.8 | **B** | 1초 뒤 조준점 충돌: `delay`(Proj:56-58) + `remaining=0` 또는 `ScheduleExtra`. 반경 ×1.5 | 시안 충돌: ×1.4→1.8 vs 행동 '범위 ×1.5, 피해 ×1.6'(질문 M-1) |
| `M01-2` | 분열 화염구 | 작은 화염구 D40% → D60% | **B** | 충돌 지점에서 소형 3발: `Shot()`. `extra`는 'SAB4'/`Set`로 하드라벨(Proj:86) → 'EXTRA:M01' |  |
| `M01-3` | 관통 화염구 | 연소 자국 2초 → 4초 | **C** | 관통은 B이나 불꽃 궤적에 거처가 없다: 땅 종류 12는 일회성, `InFirewall`은 사각형 하나에 묶임 → 궤적 조각 효과 필요 | 예보는 원 대신 선(`ForecastLine`) |
| `M02-1` | 따라오는 눈보라 | 반경 2.5m → 3.5m | **B** | `fx.position=State.position` 분기(기존 `followsTarget`은 대상 추적) | 반경 2.5→3.5 vs 플래너 리터럴 3. KEEP/KEEP_TARGET이 고정 지대를 가정(EdB:53-55,67) |
| `M02-2` | 혹한 | 둔화 50% → 60% | **C** | 감속은 A*(`SlowStrength` 리터럴 .35)이나 '머무르면 빙결'은 적별 체류 타이머가 필요(`enemy.exposure`만 있다) |  |
| `M02-3` | 귀환 서리 | 왕복 피해 D70% → D110% | **B** | M10 몸체(Fx:151, `TickReturningGlobe` Id:91-117)를 레거시 눈보라 case에서 호출. M10 키 이름 변경 | `PlanEdictBlizzard`는 군집을 계획하지 직선이 아니다 |
| `M03-1` | 과부하 연쇄 | 연결 6명 → 8명 | **A*** | 홉 한도 리터럴 4 → 보조 6~8(절대값). 감쇠 리터럴 .8 | 예보와 같은 함수를 쓴다(Fc:10) |
| `M03-2` | 되튀김 | 왕복 2회 → 4회 | **B** | 혼자인 적을 2→4번 재타격: LM04 재방문 일반화 | `PlanEdictChain`이 고유 대상 2를 요구(EdC:28-30). 예보 `hits`가 id를 반복해야 한다 |
| `M03-3` | 자력 연쇄 | 끌어오기 1.0m → 2.0m | **B** | 루프 뒤 `Pull()`로 연결된 적을 첫 대상 쪽으로 1→2m | 루프 안 당김은 `NextChainTarget`을 바꿔 예보와 어긋난다. M16 소용돌이 DoT는 시안에 없다 |
| `M06-1` | 이중 파동 | 2차 파동 50% → 100% | **B** | +1초 'burst'/`ScheduleExtra`, `Set` 피해 50→100% | 2차 파도의 빙결 여부 미정 |
| `M06-2` | 균열 결계 | 받는 피해 −10% → −20% | **B** | 빙결 루프를 생략하고 M14 수치로 `Placement(..,"ward")`. `ClassGuard` 리터럴 .1을 보조 값으로(−10→−20%) | `Fx("M14")`는 종류를 안 본다. CONTROL 목적이 아직 '노바가 얼린다'고 믿는다(EdN:35-41) |
| `M06-3` | 얼음 갑옷 | 적 1명당 보호막 2% → 4% | **B** | `AreaHit`이 적중 수를 돌려준다 → `AddShield("M06",hp*min(n*pct,.2),dur,root)` | 지속 미정. 20% 상한은 behaviourKo에만 있다 |
| `M04-1` | 화염 잔불 | 출발 지점 폭발 D80% → D120% | **B** | `State.position=destination` 전에 `AreaHit(origin,r,coef,1)` | "M04" 라벨은 기본 랭크를 먹는다(Dmg:66). 시안은 보조 랭크 |
| `M04-2` | 번개 도약 | 도착 기절 0.3초 → 0.7초 | **B** | 착지 뒤 `Nearby(dest,r,3)`+`Hit`+`ApplyStatus(Stun)` | `EdictEscapePoint`가 한적한 곳을 고르므로 적이 거의 없다. 피해 수치가 시안에 없다 |
| `M04-3` | 원소 보호막 | 보호막 HP 25% → 35% | **B** | 도착 시 `AddShield`(25→35%, 4초). M05 리터럴을 모두 옮긴다(반응 Id:72-90, LM03 마나, REF_SM01 +30%, `OwnElementalShield`, `BuffSkill`, 생존 `defenseSkill`) | `lastElement` 기록(Id:74)은 유지(M17-1, M18-2가 읽는다) |
| `M08-1` | 화염 고리 | 고리 반경 2.5m → 3.5m | **B** | `InFirewall` → 영웅 주위 원(2.5→3.5), 위치 `a.origin`. `ClassAimContains`도 |  |
| `M08-2` | 전진 불길 | 전진 속도 1.5 → 2.5m/s | **B** | `f.position`을 `f.direction`으로 이동(1.5→2.5 m/s), `Map.MoveDirect`로 가린다 |  |
| `M08-3` | 십자 장벽 | 지속 2초 → 3초 | **B** | 수직 사각형도 검사. 지속 2→3초(기본 4→5보다 짧다) | `Placement`는 id+종류당 효과 하나(Fx:82) |
| `M11-1` | 구르는 구체 | 경로 피해 D40% → D60% | **B** | 틱마다 `f.position` 이동. '경로 피해' D40→60은 새 계수 |  |
| `M11-2` | 3분할 | 구체 지속 2초 → 3초 | **C** | id+종류당 효과 하나 가정을 깬다. 인스턴스 id, 상한, 만료 규칙 필요 | `Fx("M11")` 사용자 모두 구슬 하나를 가정(Ps:54, Id:144, Ps:151, Au:135). M03 충전은 어느 구슬? |
| `M11-3` | 과충전 | 만료 폭발 D200% → D300% | **A*** | `period` 0(틱 정지) + 만료 계수를 보조 값 200→300 |  |
| `M17-1` | 단일 원소 집중 | 피해 +20% → +30% | **B** | 모든 단계가 `CS.lastElement`를 쓴다, +20→30% | 0=없음일 때 대체 필요. 냉기 단계의 빙결 정체성이 사라진다 |
| `M17-2` | 추적 붕괴 | 재조준 범위 6m → 10m | **B** | 단계마다 `f.position`을 6→10m 안 최다 밀집점으로(Au:76-84 재사용) |  |
| `M18-1` | 원소 폭주 | 추가 시전 피해 40% → 60% | **C** | 다른 장착 핵심 스킬을 40→60%로 공짜 시전. 합성 `HeroActionState`가 필요, `CaptureClassCast`를 거치지 않기(MP11/MP16/순환/전설이 셈), 재귀 차단 | 현재 3원소 폭발(Ps:155, 40%)을 다듬는 것으로 근사하면 B |
| `M18-2` | 원소 화신 | 원소별 효과 20% → 30% | **B** | `CS.lastElement`로 분기: 불 피해, 냉기 보호(`ClassGuard`), 번개 쿨다운(시전 시 cdr 또는 감소 3곳) |  |


### 3.1 흡수되는 옛 스킬과 보조 스킬의 짝

22종이 흡수된다: 20종은 데이터에 `absorbsLegacyId`가 있고, M09·M12는 원소창(M07)의 보조 2·3으로 합쳐진다(`legacyRemap`의 "merged"). 몸체를 재사용하되 "장착 여부"가 아니라 "고른 보조 스킬" 기준으로 켠다(2.4).

| 직업 | 옛 id → 보조 스킬 |
|---|---|
| 전사 | W07→W08-3, W10→W09-1, W11→W02-1, W12→W05-1, W13→W04-1, W14→W04-2, W15→W06-3, W16→W05-2 |
| 궁수 | A08→A05-1, A09→A03-1, A10→A03-2, A11→A04-1, A12→A04-2, A13→A06-2, A15→A03-3, A16→A05-2 |
| 마법사 | M05→M04-3, M09→M07-2, M10→M02-3, M12→M07-3, M14→M06-2, M16→M03-3 |
| 삭제(보조 없음) | 마법사 M13(마력 회수: 제자리에 서야 해서 자동 이동과 부딪침), M15 |

## 4. 패시브 24종과 핵심 패시브 6종

패시브는 영웅 레벨로만 해금된다(전투에서 별도 조건 없음). **랭크별 값은 데이터의 `from → to`**(`SE.Passive`, `PV`가 읽는다). 아래 "값"은 시안의 값이고 "맞음"은 현재 공식 `×(1+0.1(r−1))`이 그 값을 내는지다. ×는 **데이터 주도 `from→to`가 필요**하다.

| ID | 이름 | 해금 | 값(랭크 1→5) | 현재 적용 지점 | 맞음 | 메모 |
|---|---|---|---|---|---|---|
| WP13 | 거친 호흡 | 2 | 40→56% | `Ps:167` HP < 70% | ○ | 시안 "70% 이하" vs 구현 `<` (질문 W-13) |
| WP01 | 군중 속으로 | 4 | 25→35% | `Dmg:30-31`(3명 이상, 시전 때 고정) | ○ | 레거시 `SE.PassiveBase[0]` |
| WP06 | 처형자의 눈 | 6 | 35→49% | `Dmg:31`(대상별 HP 30% 이하) | ○ | |
| WP05 | 단단한 의지 | 8 | 40→56% | `Economy.cs:191` `shieldMultiplier` | ○ | |
| WP04 | 피의 회복 | 15 | 2→2.8% | `Passives.cs:28-35`(20틱마다) | ○ | |
| WP09 | 반격의 호흡 | 20 | 18→26 | `Ps:163` Proc(루트당 1초) | **×** | 18×1.4 = 25.2 |
| WP17 | 결전의 비축 | 25 | 15→21% | `Ps:22,27,103-105` | ○ | 비율은 비용 지불 전에 잡는다 |
| WP15 | 무기 연계 | 30 | 20→28% | `Ps:25-26`(4초 창, `Offensive`만) | ○ | |
| **WP20** | 피의 광란 (핵심) | 45 | 40→80 D% | 없음 | **×** | 아래 스케치 |
| **WP21** | 불굴의 의지 (핵심) | 45 | 6→10% | 없음 | **×** | 아래 스케치. 스킬 `role` 필드 필요 |
| AP01 | 긴 사거리 | 2 | 30→42% | `Dmg:32`(6m 이상, 가산) | ○ | |
| AP09 | 다시 당기는 시위 | 4 | 4→6 | `Ps:138`(BASIC 적중, 0.5초 Proc) | **×** | 4×1.4 = 5.6 |
| AP06 | 마무리 사격 | 6 | 20→28%p | `Dmg:69-70`(HP 30% 이하 치명, 상한 .75) | ○ | |
| AP07 | 표식의 빈틈 | 12 | 15→21%p | `Ps:93` `ClassCritical` | ○ | |
| AP15 | 독과 서리 | 15 | 25→35% | `Ps:111` | ○ | |
| AP10 | 독의 순환 | 20 | 12→20 | `Ps:135`(독 걸린 적 직접 처치, 2초) | **×** | 12×1.4 = 16.8 |
| AP05 | 절약된 집중 | 25 | −40→−56% | `Chg:18-45` + `Sim:285`(**레거시 시전만**) | ○ | `ClassCost`(`Ps:77-81`)가 무시해서 A14 등은 할인이 없다 |
| AP14 | 유인 전술 | 30 | 20→28% | `Ps:110` 리터럴 `Fx("A11")`(3m 이내) | ○ | A04-1의 미끼를 읽어야 한다 |
| **AP20** | 정밀 (핵심) | 45 | 30→50% | 없음 | **×** | 스케치. 핵심 스킬 분류가 코드에 없음 |
| **AP21** | 독사의 축복 (핵심) | 45 | 10→18% | 없음 | **×** | 스케치 |
| MP05 | 마나 순환 | 2 | 20→28% | `Eco:182`(재생 ×) | ○ | 대장간 +1 랭크는 적용하지 않는다(D5) |
| MP14 | 마력 절약 | 4 | −30→−42%p | `Ps:139-143`(기본 공격 2회째) | ○ | 총 비용 감소 상한 50% |
| MP17 | 초과 마력 | 6 | 20→28% | `Ps:112` | ○ | 시전의 첫 직접 적중만 |
| MP06 | 원소 교차 | 8 | 10→14% | `Passives.cs:36-45`, `Sim:494` | ○ | |
| MP07 | 점화의 되먹임 | 18 | 12→20 | `Ps:129` | **×** | 12×1.4 = 16.8. "불타는" = `HasDot("M07")‖InFirewall`뿐(M01-3 궤적, M04-1 폭발 추가) |
| MP09 | 냉기의 틈 | 18 | 12→20 | `Ps:130` | **×** | `Controlled()`가 감속도 센다(`Atr:27-29`) |
| MP11 | 과부하 | 18 | 30→42% | `Ps:29-30,38-39` | ○ | 번개 시전 아무거나 셈, M03·M12가 소비(시안: M03만) |
| MP16 | 삼색의 기억 | 30 | 8→12% | `Ps:40` | **×** | 8×1.4 = 11.2. "연속"이 아니라 6초 안 어떤 순서 |
| **MP20** | 산산조각 (핵심) | 45 | 30→50% | 없음 | **×** | 스케치. 적별 누적 피해 필드 |
| **MP21** | 원소 정렬 (핵심) | 45 | 60→100 D% | 없음 | **×** | 스케치. 적별 원소 비트마스크 |

공통 일: 마법사 패시브 인덱스 0–3(옛 MP01–MP04)의 분기는 죽은 코드가 되지만 `SE.PassiveBase`의 자리는 유지한다(`SE:14`). 핵심 패시브는 `ClassSkills.json`에 없다(`ClassSkills.Data`가 111개와 해금 ≤ 40을 하드 검사: `Cat:52-56`, `Cat:122`): 새 데이터 파일에서만 읽고 옛 검사는 새 체계 영웅에서 건너뛴다.

**핵심 패시브 스케치** (조사 기록의 것. 줄 번호는 같은 기준):

```csharp
// WP20 Blood Frenzy (Warrior): bleed ticks can crit; a bleeding target that dies explodes.
// bit 16 = HasDot("W09",enemy), captured BEFORE damage in ClassHitFlags (Ps:94-98); Deal sets enemy.dead (Sim:508)
if(Passive("WP20")&&(flags&16)!=0&&enemy.dead&&Once("WP20",root,enemy.id))
  Splash(a,enemy.position,r,N("WP20","value"),0,secondary:true,source:"WP20");   // raw value: SkillHit would multiply rankScale (Fx:12)
// bleed crit: TickClassDots calls ApplyOutgoing(..,critMultiplier 1,critical false) (Fx:73): roll RandomStream.Unit(ref State.rng) < Min(.75f, d.snapshot.crit)
```
```csharp
// WP21 Unending Resolve (Warrior): alternating skill roles within 4 s stack up to 3. Roles come from the spec's roleKo (needs a `role` field).
var c=Counter("WP21"); bool alt=c.until>State.time&&c.previous!=role;
c.count=alt?Mathf.Min(3,c.count+1):(c.until>State.time?c.count:0); c.previous=role; c.until=State.time+4; a.additive+=c.count*PV("WP21");
```
```csharp
// AP20 Precision (Ranger): every 3rd crit makes the next core-skill cast a forced crit (+value). Same shape as MP11 (Ps:29-30,38-39).
// AfterClassHit, inside if(direct) (Ps:133):
if(damage.critical&&Passive("AP20")){var c=Counter("AP20");if(++c.count>=3){c.count=0;Buff("AP20:charge",30,PV("AP20"),root,"charge");}}
// CaptureClassCast, Normal(a.id) block (Ps:23-30):
if(IsCore(a.id)&&Fx("AP20:charge") is ClassSkillEffect p&&!p.consumed){p.consumed=true;a.forceCrit=true;a.additive+=PV("AP20");}
// Hit (Dmg:70): draw the RNG first, then critical = canCrit && (roll<cap || Cast(root)?.forceCrit==true)
```
```csharp
// AP21 Viper's Blessing (Ranger): poisoned targets take more damage, including DoT ticks. ApplyOutgoing is the one choke point.
// beside DataDamageTaken (Dmg:44); the poison flag is captured before the hit (Dmg:41, Ps:97):
if((classFlags&2)!=0&&Passive("AP21"))independent*=1+PV("AP21");
```
```csharp
// MP20 Shatter (Mage): new EnemyState.shatter (float; old saves read 0). In ApplyOutgoing after Deal (Dmg:49):
if(Passive("MP20")&&CombatEffects.Has(enemy,StatusKind.Freeze)&&!definition.StartsWith("EXTRA:MP2"))enemy.shatter+=damage.hpLoss;
// TickStatuses, after the remaining-=dt loop, before RemoveAll (Eff:163-164):
if(enemy.shatter>0&&!CombatEffects.Has(enemy,StatusKind.Freeze)){float taken=enemy.shatter;enemy.shatter=0;
  if(Passive("MP20")&&!enemy.dead)foreach(var e in Nearby(enemy.position,2.5f,5))
    ApplyOutgoing(e,taken*Value("MP20"),0,1,1,false,2,CaptureDamage(),"EXTRA:MP20",0,0,DamageKind.Set,attackResolved:true);}
// bosses never get a Freeze status (Eff:98-103): use bossControl.staggered (Eff:155-158) if bosses should shatter. Clear in ResolveCombatDeaths and ResetUnavailablePassives.
```
```csharp
// MP21 Align the Elements (Mage): new EnemyState.elements (bit mask: 1 fire, 2 cold, 4 lightning). In ApplyOutgoing beside RecordElementHit (Dmg:50):
if(Passive("MP21")&&element>=1&&element<=3&&!enemy.dead){enemy.elements|=1<<(element-1);
  if(enemy.elements==7){enemy.elements=0;foreach(var e in Nearby(enemy.position,2.5f,5))
    ApplyOutgoing(e,snapshot.damage*Value("MP21"),0,1,1,false,0,snapshot,"EXTRA:MP21",root,0,DamageKind.Set,attackResolved:true);}}
// Value() is the from/to accessor, not PV (PV multiplies rankScale). EXTRA: ids are non-native: no recursion, no element recording.
```

## 5. 궁극기 6종 (랭크 3)

| ID | 이름 | 지금 | 새 체계에서 | 메모 |
|---|---|---|---|---|
| W17 | 선조의 전쟁 | 조상 2, 1초마다 가장 가까운 적에 2차 피해, 8초 | 랭크 +20%, 쿨다운 60/55/50 | W17-1 A(조상 수·지속·틱), W17-2 C |
| W18 | 거인의 심판 | 반경 4m 일격 D600%, 기절 2초, `prepare` .9 | 랭크 +20%, 쿨다운 | W18-1 A, W18-2 C |
| A17 | 일제 소탕 | 반경 4m 고정, 6펄스 D80%, 8명 | 랭크 +20%, 쿨다운 | A17-1 B, A17-2 A |
| A18 | 잔상 행군 | 잔상 마커, 직접 적중이 가장 오래된 잔상을 소비해 D60% 2차 화살 | 랭크 +20%, 쿨다운 | A18-1 C, A18-2 B |
| M17 | 삼원소 붕괴 | 3단계(불·냉기·번개), 반경 4m | D200%→280%(×1.4) | M17-1 B, M17-2 B |
| M18 | 현자의 화신 | 8초, 보호막 25%, 피해 +20%, 비용 −20%, 3원소 폭발 D40% | 보호막 25→35%, 피해 20→30 | 보호막이 `Fx:161`에서 랭크 배율을 받지 않는다. 피해 20→30은 ×1.4 = 28이라 from/to. 시안 기본 문구에 비용 −20%와 3원소 폭발이 없다(질문 M-9) |

- 궁극기 둘은 **공유 타이머**다(하한 45초). 랭크가 쿨다운을 60→55→50으로 줄여도 45 아래로 가지 않는다.
- **레벨 40 이하 영웅은 궁극기를 못 배운다.** T5 문은 "T5 밖에서 40포인트"이고 포인트 = 레벨이라 첫 궁극기 포인트는 레벨 41이 필요하다(저장 조사 0장 7). 튜토리얼(H08)과 "Lv.40" 문구가 이 가정을 쓴다(계획 17장 새 결정 D17).

## 6. 정리할 것

- **죽은 코드가 되는 것**: 사라지는 패시브 33개의 분기(`Ps:28,52-53,71,171`, `Fx:146,226,259`, `Proj:24,133`, `Passives.cs:25`, `Fc:55`, `RgrT:43`, 마법사 `Proj:107`, `Sim:282`, `Fc:10`, `Eco:191`). 코드에 남겨도 되지만 새 체계 영웅에게는 평가되지 않아야 한다.
- **하드코딩 id 목록**: 6단계에서 옛 id를 가리키는 곳을 고친다(`legacy_id_inventory.py --check`가 1~5군 참조가 0이 될 때까지 실패한다).
- **검증용 빌드**: `ClassSkills.json`의 `ClassSkillValidationBuild` 30개 가운데 29개가 옛 id를 쓴다(전사 10개 전부, 궁수 10개 전부, 마법사 9개). 새 규칙에서는 명시적인 **예외 경로**(킷)로만 만든다(포인트·티어 규칙 우회).
- **`ResetUnavailablePassives`**: 배우지 않은 패시브의 상태(적별 필드, 카운터)를 정리한다.

## 7. 작업 순서

계획 14장의 5단계를 넷으로 나눈다. 선행: **1단계(데이터·규칙)와 2단계(저장 v5, 만렙 60)** 완료. 새 시험은 `SkillTreeD4.Enabled`가 켜진 틀에서 돈다.

| 단계 | 내용 | 끝났다는 기준 |
|---|---|---|
| **5.0** 공통 틀 | 2장 전부: v5 필드, `Loadout.Modifier`, `ClassSkillCast.mod/modRank`, `SkillTreeD4Values`, 랭크 값 읽기(데이터 주도), 궁극기 `RankScale`·쿨다운, 정리 규칙(2.4), 영속 허용 목록, 지문·Balance1000 갱신, 레벨 60 틀 | 쌍 시험 틀이 보조 스킬 하나(W09-3 같은 A급)로 돈다. 기존 전투 시험이 `main`과 같은 결과 |
| **5a** 전사 | 본 스킬 수치 → A급 → B급 → C급 → 패시브 8 → 핵심 2 → 궁극기. 티어 순서로 묶음(T1, T2, T3, T4+궁극기) | 28 보조 + 10 패시브 + 두 궁극기의 시험(2.8) |
| **5b** 궁수 | 같은 순서. `A06`(원소 주입)은 옛 그림자 화살 코드를 바탕으로, 리터럴 `"A06"` 참조(`Sim:280`, `Dmg:57`, `EdictOrder.cs:31-33`, `HuntEdictV2.Options.cs:91-94`, `EdictValidation.cs`, `Itemization.cs:215`, 표시 코드 등 십여 곳)를 새 의미로 | 같음 |
| **5c** 마법사 | 같은 순서. `M07`(원소창)은 옛 M07 몸체 위에, 속성은 보조 스킬이 정한다 | 같음 |

각 직업 PR 안의 순서: ① 본 스킬 랭크 수치(데이터 주도), ② 수치만 바꾸는 보조 스킬(A·A\*), ③ 기존 경로 위 동작(B), ④ 새 장치(C: 설계를 이 문서에 한 단락씩 먼저 적고 구현), ⑤ 패시브(해당하는 것부터), ⑥ 핵심 패시브, ⑦ 궁극기. **C급은 질문(8장)의 답을 정한 뒤** 구현한다.

**하지 말 것**: 규칙(포인트·티어·보조 랭크)을 전투 코드에 다시 쓰지 않는다. 보조 스킬을 옵션 체계에 싣지 않는다. 시안에 없는 현행 덤 효과(W11 가드·자원, W12 +15% 막기, A12 +20% 속도 등)를 몰래 옮기지 않는다(질문 C-5).

## 8. 코드만으로 정할 수 없는 질문과 기본값

기본값으로 진행할 수 있다. 사용자가 다르게 정하면 그 직업 PR 전에 바꾼다. **C급 보조 스킬은 해당 질문이 답해지기 전에 구현하지 않는다.**

**공통**

| # | 질문 | 기본값 |
|---|---|---|
| C-1 | 보조 값이 본 랭크 배율을 곱하는가 | 곱하지 않는다. 본 피해는 본 랭크, 보조 값은 보조 랭크의 from→to(계획 5.4) |
| C-2 | `%`·`D%` 단위의 기준 | `D%` = 무기 피해 기준 계수, `%` = 그 줄 앞 단어가 가리키는 양. 모호한 줄(A14-1, A02-2, A01-3, A07-3)은 PR에서 한 줄씩 정해 표에 적는다 |
| C-3 | 시안에 없는 비용·쿨다운·지속 | 현행 유지 |
| C-4 | 강제 치명(A07-3, AP20, A04-3)이 치명 카운터에 세어지는가 | 센다 |
| C-5 | 시안에 없는 현행 덤 효과 | 뺀다(시안이 기준) |
| C-6 | 시안 문구와 구현이 다를 때 | 시안의 `effectKo`/`value`가 이긴다(계획 8.2) |
| C-7 | 레거시 랭크 표(비용·지속·기절)를 어디에 둘지 | 데이터 주도로 읽는다(`runtime_parameters.json` 확장 + `SkillTreeD4.json`). 지문 갱신은 PR에 포함 |
| C-8 | 설치물·소환물 상한(덫 2, 포탑 1, 구슬 3, 미끼, 분신) | 스킬마다 시전자 기준 상한을 두고 초과 시 가장 오래된 것을 제거(A03-3 포탑 1, M11-2 구슬 3, A18-1 분신 1) |

**전사**

| # | 질문 | 기본값 |
|---|---|---|
| W-1 | W09-2 "중첩당 +20%"는 합인가 곱인가 | 중첩당 출혈 피해에 가산 |
| W-2 | 시안의 자원 표기가 W09 "Resource on hit +8→12", W08 "Resource gain +10→18", W08-1 "Extra resource +4→12"로 제각각이다. 건당인가 시전당인가 | 모두 시전당 한 번(적중이 하나 이상일 때) |
| W-3 | W01-3으로 틱 간격이 줄면 비용도 | 초당 비용이 같도록 틱 비용을 간격에 비례해 줄인다 |
| W-4 | W03 기본이 단일 타격인가 100° 다중 타격인가 | 현행 100° 유지. 시안 문구(SD4:581)를 디자인 담당이 고친다 |
| W-5 | W02-3 "충전 2개, 재충전 8→6초"와 본 스킬 쿨다운 8→6.4초의 관계 | 충전 모델(전용 `charges` 상태, 저장됨)을 새로 만든다. 보조를 고른 동안 쿨다운을 **대체**하고 재충전 시간은 보조 랭크의 from→to(8→6초), 본 랭크의 쿨다운 감소는 적용하지 않는다 |
| W-8 | W06-3 "6초 동안 자원 회복 정지"와 구현(대여·상환) | 시안 문구대로 회복 정지. 대여 코드는 쓰지 않는다 |
| W-13 | WP13 "70% 이하"와 `<` | 시안(≤) |

**궁수**

| # | 질문 | 기본값 |
|---|---|---|
| R-1 | A06(원소 주입)의 기본 효과와 원소, 소비하는 발사 | 기본: 다음 3발 추가 피해 +25→35%(원소 없음). 소비하는 발사는 BASIC·A01·A02(옛 규칙). 분기·메아리·포탑·비는 소비하지 않는다 |
| R-2 | A01-1 튕김 규칙(`hitIds`, `maxHits`) | 한 대상은 한 번만 맞고, 튕김 횟수는 랭크 값(1·1·2·2·3회, `rank_values.json`)만큼 벽 법선으로 반사한다. 예보는 첫 구간만 |
| R-3 | A03-2 서리 덫: 지대인가 일회성인가(옛 A10은 일회성 폭발) | 시안대로 지대(덫이 있는 동안 감속 30→50%) |
| R-4 | A05-1 겨냥 중 이동·방해 | 겨냥 중 자동 이동 정지, 피해를 받으면 취소(계획 8장) |
| R-5 | A07 자원: 시안 "+8→12" vs 코드 비용 12와 25% 예비 | 시안(적중 시 획득)으로 바꾸고 비용 항목은 제거 |
| R-6 | A17-1 8파의 지속, A17-2 "Narrow area ×1.6→2.0"의 뜻 | A17-1은 `ticks` 8·지속 4초. A17-2는 반경 ×0.5, 파당 피해 ×1.6→2.0, 주기 .25, 지속 유지(`ticks` 10) |
| R-9 | A18-2 "사라짐"의 정의 | 만료·소비·상한 초과 모두에서 폭발 |

**마법사**

| # | 질문 | 기본값 |
|---|---|---|
| M-1 | 시안 문구와 `effectKo`가 다른 값(M01-1 ×1.4→1.8 vs "범위 ×1.5, 피해 ×1.6", M02-2, M03-1, M03-3, M04-2, M04-3, M06-3, M08-1·2·3, M11-2, M18-1) | `effectKo`/`value`가 이긴다(C-6). 문구는 디자인 담당이 고친다 |
| M-2 | M07: 지금은 비용 18·쿨다운 3인데 시안 기본 수치는 "Mana on hit +6→10"뿐인 T1 생성기다 | 쿨다운 유지, 비용 0, 마나 +6→10(시전당 한 번, 적중이 있을 때). 자동 시전 조건은 "마나가 가득 차지 않았을 때" |
| M-3 | M04-3이 M05의 마지막 원소 반응을 유지하는가 | 유지한다(`lastElement` 기록이 M17-1, M18-2에 필요) |
| M-4 | M08-1 원은 원판인가 고리인가, 영웅을 따라가는가 | 영웅을 따라가는 원판 |
| M-5 | M17-1의 `lastElement = 0` | 불로 대체 |
| M-6 | MP20 산산조각의 반경·상한·대상 | 반경 2.5m, 5명, 보스는 기절(stagger) 때 터진다 |
| M-7 | MP21 원소 정렬의 창과 초기화 | 한 적에 세 원소가 한 번씩 닿으면 터지고 초기화한다. 시간 창 없음. 지속 피해와 `EXTRA:` 적중은 세지 않는다 |
| M-9 | M18의 비용 −20%와 3원소 폭발 | 유지한다(코드에 있고 시안 기본 문구에 없다: 문구를 디자인 담당이 보완) |
| M-12 | MP07·MP09의 12→20, MP16의 8→12 | 시안 값(from→to) |

위 질문 가운데 **사용자 결정이 필요한 것**(기본값이 동작·밸런스를 크게 바꾸는 것: W-4, R-1, C-5, M-9)은 계획 17장에 D21~D24로 옮겨 두었다.
