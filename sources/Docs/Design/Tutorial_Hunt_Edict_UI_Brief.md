# 사냥 칙령 튜토리얼 요약 — UI 개선 입력 문서

갱신일: 2026-10-04

[English](Tutorial_Hunt_Edict_UI_Brief.en.md)

## 1. 이 문서의 용도

- 사냥 칙령 화면을 개선하는 작업에 넘기는 입력 문서다. 튜토리얼이 칙령 화면에서 **무엇을 가르치고, 어떤 컨트롤을 가리키고, 화면에 무엇을 요구하는지** 정리한다.
- UI가 바뀌면 튜토리얼도 맞춰 고쳐야 한다. UI 작업자는 4장의 계약과 5장의 요청을 반영하고, 끝낼 때 7장의 인계 대응표를 남긴다. 그 뒤 튜토리얼 작업(장 콘텐츠, 칙령 문장 연출)이 새 UI 위에서 이어진다.
- 상위 설계는 [튜토리얼 장 설계](Tutorial_Chapters.md), 단계 공개 데이터와 규칙은 [사냥 칙령 단계 공개](../Implementation/Hunt_Edict_Progression.md), 이전 UX 방향은 [사냥 칙령 UX 재설계](Hunt_Edict_UX_Redesign.md)를 따른다.
- 이 문서는 코드를 직접 읽고 쓴 것이다. 게임을 실행해 확인한 내용이 아니다.

## 2. 한눈에 보기

1. 신규 플레이어에게 칙령은 **한 번에 한 가지씩** 보인다. 전역 옵션 152개 중 처음에는 물약 HP 기준 1개만 보이고, 균열을 깰 때마다 한 문장씩 열린다. 한 번 열린 권한은 회수하지 않는다.
2. 튜토리얼이 칙령 화면에서 하는 일은 세 가지다.
   - **프롤로그 강제 레슨**: 지정한 컨트롤 하나만 누르게 가리고(`PrologueGate`) 금빛 테두리(`TutorialAnchorRing`)로 짚는다.
   - **단계 공개**: 공개 규칙에 따라 탭·그룹·옵션을 열고, 열리지 않은 것은 자리나 빈칸 없이 아예 그리지 않는다.
   - **마을 안내 카드 → 창 열기 → 항목 강조**: 균열 결과 뒤 마을에서 카드 한 장을 보여 주고, `설정해보기`를 누르면 해당 항목을 열어 강조한다.
3. 튜토리얼은 **컨트롤 이름(`edict-*`)과 창 상태**를 읽어 화면을 안내한다. 이름이 바뀌거나 사라지면 안내가 멈춘다. 4.1이 그 목록이다.
4. UI는 세 가지 밀도를 모두 감당해야 한다. 프롤로그(컨트롤 한 개) → 초반(탭 3~4개, 간편 프리셋만) → 중후반(직접 설정·프리셋·검색·공유). 3.1에 정리했다.
5. 칙령 공개 순서는 중요도 순 사다리(문장 1~15)로 정리했다. 이 브랜치(`claude/tutorial-chronicle`)에서 낮은 HP 대응을 균열 3으로, 위치·거리를 균열 5로 맞바꾸고 세밀 조건을 3묶음으로 나눴다. 8장에서 현재 상태를 구분한다.

## 3. 진행 흐름

### 3.1 화면 밀도 3단계

| 단계 | 시점 | 보이는 탭 | 편집 방식 |
| --- | --- | --- | --- |
| L0 프롤로그 | 필수 맵 | 스킬 → 생존, 한 번에 하나만 | 첫 스킬 노드 1개, 슬롯 1칸, 물약 HP 기준 1개 |
| L1 초반 | 마을 도착 ~ 균열 5 | 요약·스킬·생존, 균열 4부터 전투 | 간편 프리셋(공식 선택지)만. 직접 편집은 물약 HP 기준·자동 물약·방어/탈출 스킬만 |
| L2 중반 | 균열 6 ~ 15 | 프리셋(6), 전리품(7), 탐색(8), 가방·정리(9), 장비 추천 착용(12), 반복 설정(15 + 룬 안내 완료) | 공개된 그룹의 직접 설정, 스킬 상세, 공격 순서, 로컬 프리셋, 훈련 비교 |
| L3 후반 | 균열 18 ~ | 전 탭 | 세밀 조건, 행동 순서·세부 탐색, 설정 공유(20) |

- 탭이 열리는 규칙은 코드의 `HuntEdictProgression.Tab`이다. 요약·스킬·생존은 항상 열리고, 프리셋은 균열 6 이후이며, 나머지는 그 탭 그룹의 옵션이 하나라도 공개되면 열린다.
- 영웅이 2레벨이 되기 전에는 첫 스킬 노드만 보인다. 슬롯은 장착한 스킬 수보다 한 칸 더 보이며(처음에는 1칸) 최대 4칸이다. 40레벨부터는 궁극기 칸이 더해진다.
- 보석·코어·영약·저주 상자 항목은 공개 단계와 별개로 실제 획득을 확인한 뒤에 나타난다.
- 추천 착용 잠금(`hero.useRecommendedEdict`) 중에는 요약·스킬·반복 탭만 열려 있고 나머지 탭은 비활성이다.

### 3.2 프롤로그 강제 레슨 (칙령 창)

필수 맵(`tutorialFlowVersion==3`)에서 칙령 창은 `tutorialLesson`(RunState)을 받아 열린다. 이때 창이 제한하는 것은 다음과 같다.

- 탭은 첫 단계에서 `스킬`, 보스전 개입 단계에서 `생존` 하나만 보인다.
- 스킬 트리에는 직업별 첫 스킬(전사 W01, 궁수 A01, 마법사 M01) 노드만 있고, 슬롯은 1칸이다.
- 옵션은 `survival.potionHpPercent` 하나와 그것이 든 그룹만 열린다.
- 사용 방식(프리셋) 선택지는 첫 스킬의 비교 두 가지만 열린다. 전사는 stand·edge, 궁수와 마법사는 steady·pack이다.
- `PrologueGate`가 지정 컨트롤 밖의 입력을 막고, 뒤로 가기(Esc)도 `ContentWindowHost.BackLocked`로 막는다. 지정 컨트롤을 2초 동안 찾지 못하면 가림막이 풀린다.

플레이어가 따르는 순서는 아래와 같다. 표의 컨트롤은 화면에 실제로 있어야(`interactable`이고 계층에서 활성) 안내가 그 컨트롤을 짚는다. 조건은 `GameUI.TutorialComparison.cs`의 `ProgressiveSkillLessonStep`과 `ProgressiveSurvivalLessonStep`에서 읽었다.

| 순서 | 플레이어 행동 | 지정 컨트롤 | 창 상태 조건 |
| --- | --- | --- | --- |
| 1 | 칙령 열기 | `menu-hunt-edict` (콘텐츠 독) | 창이 없음 |
| 2 | 스킬 탭 | `edict-tab-skills` | `SelectedTab != "skills"` |
| 3 | 사용 방식 화면이면 트리로 복귀 | `edict-policy-back` | `!ManagingSkills` |
| 4 | 첫 스킬 선택 | `edict-skill-<스킬ID>` | `SelectedSkill`이 첫 스킬이 아님 |
| 5 | 스킬 포인트 1 사용 | `edict-skill-rank-up` | 등급 0 |
| 6 | 1번 슬롯에 장착 | `edict-skill-equip` | 장착 안 됨 |
| 7 | 저장 | `edict-save` | 장착한 상태가 저장 전 |
| 8 | 장착한 스킬 슬롯 열기 | `edict-active-slot-0` → `edict-slot-policy` | 저장된 뒤, 대화상자 유무에 따라 |
| 9 | 방식 A 확인 | `edict-preset-tab-<A>` → `edict-preset-activate` | 레슨 단계 0 |
| 10 | A를 관찰하고 진행 | 영역 `Preset explanation`(전투 예시와 재생·일시정지·재실행) → `edict-starter-next` "동작을 확인했습니다" | 관찰 완료 시 활성, 단계 0→1 |
| 11 | 방식 B 활성화 후 저장 확인 | `edict-preset-tab-<B>` → `edict-preset-activate` → `edict-starter-next` "방식 B 저장 확인" | 단계 1→2 |
| 12 | B를 관찰하고 진행 | `Preset explanation` → `edict-starter-next` | 단계 2→3 |
| 13 | 원하는 방식 활성화 후 확정 | 영역 `Option area`(창 본문) → `edict-starter-next` "이 방식으로 사냥하기" | 단계 3→4 |
| 14 | 닫고 전투 재개 | `edict-close` | 단계 4 이상 |
| 15 | (보스전 개입) 칙령 다시 열기 | `menu-hunt-edict` | 물약 HP 기준이 60이 아님 |
| 16 | 생존 탭 | `edict-tab-survival` | `SelectedTab != "survival"` |
| 17 | 물약 HP 기준 40%를 눌러 변경 | `edict-option-survival.potionHpPercent` | 값이 60이 아님 |
| 18 | 숫자 입력 후 적용 | `edict-number-input`(60 입력) → `edict-number-apply` | `HasDialog` |
| 19 | 저장 | `edict-save` | 편집본 값이 60이고 저장 전 |
| 20 | 닫기 | `edict-close` | 저장된 값이 60 |

- 보스전에서 실제로 물약이 60% 기준으로 쓰이면 레슨 단계가 5가 되고 수문장 처치로 완료한다.
- 구형 흐름(v2)은 강제 단계에서 `edict-group-<옵션ID>`, `edict-quick-picker-<범위>`, `edict-quick-choice-<선택지>`를 쓴다. 기존 v2 저장 이어하기와 구형 계정의 경로로 남아 있어, 아래 4.1의 이름에 포함했다.
- 지정 영역 두 개는 버튼이 아니라 이름이 붙은 사각형이다. `Option area`는 창 본문 루트, `Preset explanation`은 사용 방식 화면의 설명·전투 예시·진행 버튼을 담은 행이다. 가림막은 이 영역 안의 입력만 통과시키므로 **진행 버튼 `edict-starter-next`는 `Preset explanation` 안에 있어야 한다.**

### 3.3 마을 이후: 칙령 문장 사다리

번호는 [튜토리얼 장 설계](Tutorial_Chapters.md)의 문장 번호다. 중요도(생존 → 안정 → 효율 → 고급) 순이다. "열리는 곳"은 안내가 가리키는 탭과 그룹이며, "집중 옵션"은 `설정해보기` 때 `FocusGlobalOption`이 여는 옵션이다.

| 문장 | 기능 | 공개 | 안내 ID | 열리는 곳 (탭 · 그룹) | 집중 옵션 / 간편 프리셋 범위 |
| --- | --- | --- | --- | --- | --- |
| 1 | 물약 HP 기준 | 프롤로그 | P00 | 생존 · 자동 물약 | `survival.potionHpPercent` |
| 2 | 첫 스킬 방식 비교 | 프롤로그 | P00 | 스킬 · 첫 스킬의 사용 방식 | `skill/<첫 스킬>` (직업별 두 방식) |
| 3 | 자동 HP 물약, 기본 회피 | 첫 균열 종료(성공·실패 모두) | F05 | 생존 · 자동 물약, 피해 유형별 회피 | `survival.potionHpPercent`, `survival.potion`, `dodge.ground.policy`(balanced/all) |
| 4 | 전투 성향 | 균열 2 | E02 | 요약 · 전투 방식 카드 | `edict-style-aggressive/balanced/careful` |
| 5 | 낮은 HP 후퇴·복귀 | 균열 **3** (기존 5) | E05 | 생존 · 긴급 대응·전투 복귀 | `survival.lowHp` (생존 우선·균형 생존·위급할 때만 후퇴), 방어·탈출 스킬 `survival.defenseSkill` |
| 6 | 공격 대상 | 균열 4 | E04 | 전투 · 공격 대상 선택 | `target.default` (가까운 적부터·위험한 적 우선·밀집 무리 정리), `target.switch` |
| 7 | 위치·거리·포위 | 균열 **5** (기존 3) | E03 | 전투 · 전투 위치·거리, 포위 대응·몰이 | `position.engage` (4종), `position.surrounded` (3종) |
| 8 | 직접 설정·프리셋·훈련 비교 | 균열 6 | F07, H09, H10, H17 | 프리셋 탭, 모든 공개 그룹의 직접 설정, 스킬 상세 | 권한 `details`, `presets` |
| 9 | 전리품 회수 | 균열 7 | E07 | 전리품 · 등급별 장비 회수 | `loot.NORMAL` (모두·희귀 이상·전설/세트), `loot.class`, `loot.gold`, `loot.distance` |
| 10 | 탐색·저주 상자 | 균열 8 (저주 상자는 실제 발견 후) | E08, E08C | 탐색 · 탐색·균열 진행, 저주 상자 | `explore.mode`, `explore.chests`, `explore.cursedChest` |
| 11 | 가방 공간과 보호 | 균열 9 | E09 | 가방·정리 · 가방 공간 부족 | `bag.trigger`, `bag.cleanupAt`, `bag.protectEnhanced`, `bag.warehouseFull` |
| 12 | 자동 장착, 물약 보급 | 균열 12 / 14 (전설·세트 비교는 균열 12 이후 획득 시) | E12, E12S, E14 | 장비 추천 착용, 생존 · 자동 물약 | `autoEquip.enabled`(기본 OFF), `autoEquip.1.mode`, `potion.autoBuy`(기본 OFF) |
| 13 | 자동 반복 | 균열 15 + 룬 안내 완료 | H11 | 반복 설정 | `repeat.enabled`, `repeat.failures`, `repeat.freeSlots` |
| 14 | 세밀 조건 (3묶음, 마을에서 한 번에 한 묶음씩 안내) | 균열 18 | E18 / E18B / E18C | 생존 · 특수 위험·회피 방식 / 전투 · 대상 유지·추적 / 가방·정리 · 가방 공간 부족 | `dodge.interrupt` / `target.chaseDistance` / `bag.replacementRank` |
| 15 | 행동 순서·설정 공유 | 균열 20 | E20 | 전투 · 행동 우선순위 | `common.order`, 권한 `sharing` |

- 묶음 크기: E18은 회피·생존 20개, E18B는 추적·전리품 13개, E18C는 가방 정리 7개로 모두 균열 18에서 함께 열린다.
- 스킬 관련 안내(칙령 화면의 스킬 탭이 목적지): F06 새 스킬 장착(2레벨부터, 아직 장착한 적 없는 액티브), H07 스킬 포인트와 패시브(2레벨부터), H08 궁극기(40레벨).
- 간편 프리셋은 그룹마다 선택 한 개로 묶인다. 균열 6 전에는 이 간편 프리셋만 쓰고, 균열 6부터 직접 설정이 열린다. 예외로 물약 HP 기준, 자동 물약, 방어·탈출 스킬은 처음부터 직접 편집한다.
- 자동 구매·처분·장착·반복은 공개돼도 처음에는 OFF이고 공개만으로 켜지지 않는다.

### 3.4 안내 카드부터 완료까지

1. 결과 화면(`ShowResult`)이 뜰 때마다 `EdictResultSettled()`가 호출되어 카드 한 장을 허용하는 상태가 된다. 전투가 진행 중(`game.Active`)인 동안에는 매번 허용 상태로 되돌려 두므로, 균열을 마치고 마을에 돌아오면 카드가 나올 수 있다. 카드를 한 장 띄우면 다음 결과 화면이 뜰 때까지 더 띄우지 않는다.
2. 마을(`plaza`)에서 다른 창·팝업이 없고, 안내를 숨기지 않았고, 첫 균열을 마친 상태이면 `TryEdictNotice()`가 카드를 **한 장** 띄운다.
   - 대상은 `E*`, F05, F06, F07, H07, H08, H10, H11 중 지금 열려 있고 아직 안내·미룸·숨김·수행하지 않은 첫 안내다. 순서는 `Tutorials.All` 순서다.
   - F05 카드에는 마지막 전투 5초의 HP 변화와 가장 큰 피격원이 덧붙는다.
   - 카드 버튼은 `edict-notice-configure`(설정해보기)와 `edict-notice-later`(나중에)다.
3. `설정해보기`는 `StartTutorialAction`이 칙령 창을 열고 안내별로 이동한다.
   - 규칙 안내(E*)와 E08C는 `FocusGlobalOption(<집중 옵션>)`으로 그룹 상세를 연다.
   - F05는 `survival.potionHpPercent`로 집중한다.
   - 그 밖의 안내는 탭을 고른다. 스킬 안내는 `skills`, H10은 `presets`, H11은 `repeat`, 나머지는 `overview`다.
4. 창이 열린 동안 `RefreshTutorialAnchor()`가 0.25초마다 이름 목록에서 **처음 발견한, 활성이고 `interactable`인 버튼**에 금빛 테두리를 붙인다. 안내가 수행 완료로 기록되면 테두리가 사라진다.
5. 완료 판정은 화면 조작이 아니라 **저장된 칙령의 변화**(`TutorialProgress.EdictSaved`)로 한다. 규칙 안내는 그 규칙의 옵션이 바뀌어 저장되면 수행으로 기록한다. F05는 물약 HP 기준·자동 물약·기본 회피 중 하나, E02는 전투 성향, H09는 칙령 서명이 바뀌어 저장될 때다.

## 4. UI가 지켜야 할 계약

### 4.1 튜토리얼이 쓰는 컨트롤 이름

튜토리얼은 이 이름으로 컨트롤을 찾는다. `Button`이면 `interactable`이고 `activeInHierarchy`일 때만 찾은 것으로 본다.

| 이름 | 의미 | 쓰는 곳 |
| --- | --- | --- |
| `menu-hunt-edict` | 콘텐츠 독의 사냥 칙령 버튼 | 프롤로그 1, 15 |
| `edict-close` | 창 닫기 | 프롤로그 14, 20 |
| `edict-save` | 저장 (변경이 있을 때만 활성) | 프롤로그, F05, F06, 규칙 안내, E02 |
| `edict-tab-<탭ID>` | 탭 버튼. 탭ID는 `overview`, `skills`, `combat`, `survival`, `loot`, `bag`, `explore`, `repeat`, `autoEquip`, `presets` | 프롤로그 2, 16, F05, F06 |
| `edict-skill-<스킬ID>` | 스킬 트리 노드 | 프롤로그 4 |
| `edict-skill-rank-up` / `edict-skill-equip` | 선택한 스킬의 포인트 사용 / 장착 | 프롤로그 5, 6, F06 |
| `edict-active-slot-0` / `edict-slot-policy` | 장착 슬롯 / 슬롯 메뉴의 사용 방식 편집 | 프롤로그 8 |
| `edict-policy-back` | 사용 방식 화면에서 스킬 트리로 돌아가기 | 프롤로그 3 |
| `edict-preset-tab-<프리셋ID>` / `edict-preset-activate` | 사용 방식 탭 / 이 방식 활성화 | 프롤로그 9, 11 |
| `edict-starter-next` | 비교 단계 진행 버튼(문구가 단계마다 바뀜) | 프롤로그 10~13 |
| `edict-option-<옵션ID>` | 전역 옵션 행. 옵션ID는 예: `survival.potionHpPercent` | 프롤로그 17, F05, 규칙 안내 |
| `edict-quick-picker-<범위>` | 그룹의 간편 프리셋 선택 버튼. 범위는 예: `global/position.engage` | 규칙 안내, 구형 프롤로그 |
| `edict-quick-choice-<프리셋ID>` | 간편 프리셋 선택 대화상자의 선택지 | 구형 프롤로그 |
| `edict-group-<그룹 첫 옵션ID>` | 그룹 진입 | 구형 프롤로그 |
| `edict-number-input` (InputField) / `edict-number-apply` | 숫자 입력 대화상자의 입력칸 / 적용 | 프롤로그 18, F05 |
| `edict-style-<성향ID>` | 요약 탭의 전투 성향 카드. 성향ID는 `aggressive`, `balanced`, `careful` | E02 (`edict-style-balanced`에 테두리) |
| `edict-notice-configure` / `edict-notice-later` | 마을 안내 카드 버튼(칙령 창 밖) | 안내 카드 |
| `Option area` (RectTransform) | 창 본문 루트 | 프롤로그 13의 지정 영역 |
| `Preset explanation` (RectTransform) | 사용 방식 설명·전투 예시·진행 버튼 행 | 프롤로그 10, 12의 지정 영역 |

테두리를 붙일 때의 우선순위 목록은 다음과 같다. 앞에서부터 처음 발견한 컨트롤에 붙는다.

- F05: `edict-number-apply`, `edict-option-survival.potionHpPercent`, `edict-save`, `edict-tab-survival`
- F06: `edict-skill-equip`, `edict-save`, `edict-tab-skills`
- 규칙 안내와 E08C: `edict-option-<집중 옵션>`, `edict-quick-picker-<그 그룹의 범위>`, `edict-save`
- E02: `edict-style-balanced`, `edict-save`

이 밖에 스모크와 테스트가 쓰는 이름도 많다(검색 `edict-search*`, 변경만 보기 `edict-changed-only`, 프리셋 `edict-preset-*`, 장비 `edict-auto-*`·`edict-storage-*`, 전투 예시 `edict-preview-*`, 공유 `edict-share`, 대화상자 `edict-dialog-*` 등). 화면을 바꾸면 이 이름이 쓰이는 곳도 6장에 따라 함께 확인한다.

### 4.2 튜토리얼이 읽는 창 상태와 호출하는 동작

`HuntEdictWindow`(통합 창)가 외부에 내놓는 것 중 튜토리얼이 쓰는 목록이다.

- 상태: `SelectedTab`, `ManagingSkills`, `SelectedSkill`, `PolicySkill`, `SelectedGroup`, `VisibleSkillPreset`, `ActiveSkillPreset`, `HasDialog`, `Session.Draft`, `Session.Dirty`, `PreviewCombat`, `ProgressivePrologue`, `QuickPresetSelection(범위)`
- 동작: `Open(..., tutorialLesson)`, `SelectTab(탭ID)`, `FocusGlobalOption(옵션ID)`, `HandleBack()`
- 열기: `GameUI.ShowEdictEditor()`가 통합 창을 연다. 구형 편집기 `ShowLegacyEdictEditor`는 일부 스모크만 쓰며 튜토리얼 연결이 없다.

### 4.3 공개 규칙을 화면에 반영하는 방식

공개 판정은 코드가 가지고 있고(`HuntEdictProgression.Has/Visible/Direct/Quick/Tab`), 창은 이를 그대로 따른다. UI를 바꿔도 다음은 유지한다.

- 잠긴 탭·그룹·옵션은 **그리지 않는다**. 잠김 자리표시나 빈칸을 두지 않는다. 다음 기능은 요약 탭의 한 줄 예고(`HuntEdictProgression.Next`)로만 알린다.
- 직접 편집 가능 여부는 `Direct`, 간편 프리셋 선택지 노출은 `Quick`이 정한다. 화면이 이를 우회해 값을 바꿀 수 없다. 저장 단계(`ValidateChange`, `ValidateTransaction`)도 미공개 값 변경을 거절한다.
- 화면 갱신 키(`DisclosureDisplayKey`)에는 영웅·레벨·공개 목록·물약 revision·칙령·스킬·프리셋·레슨 단계가 들어 있다. 새 표시 상태를 추가하면 이 키에도 넣어야 갱신된다.
- 모든 문구는 한국어 원문을 `Loc.T`/`Loc.F`로 번역해 그린다. 새 문구는 `en.txt`에 영어를 함께 둔다.

### 4.4 깨지면 안 되는 동작

1. 컨트롤 이름이 창 안에서 안정적이고 유일해야 한다. 같은 이름의 컨트롤이 둘 이상이면 처음 발견한 것에 테두리가 붙는다.
2. 지정 컨트롤은 **그 순간 화면에 보여야** 한다. 가림막은 컨트롤의 화면 사각형만 뚫는다. 스크롤 안에 있는 항목이라면 `FocusGlobalOption`이 그 항목을 보이는 위치에 열어야 한다(현재는 그룹 상세를 열고 스크롤을 맨 위로 둔다).
3. 창을 다시 그리면 컨트롤이 파괴되어 테두리가 떨어진다. 프롤로그는 매 프레임, 안내 카드는 0.25초마다 이름으로 다시 붙인다. 저장 알림이나 매 프레임 갱신으로 창을 반복해서 다시 그리면 테두리가 깜빡인다. 현재는 `StoreViewBinding`이 누름·입력·드래그·대화상자 동안 갱신을 보류하고 편집본·포커스·스크롤을 복원한다. 이 보호를 유지한다.
4. 뒤로 가기 순서: 대화상자 → 사용 방식 화면에서 스킬 트리 → 창 닫기(`HandleBack`). 프롤로그에서는 뒤로 가기 자체가 잠긴다.
5. 저장은 기존 `CommitHuntEdict` 경로로 한다. 튜토리얼 완료와 진행 판정이 저장된 값 변화에 의존하므로 다른 경로로 저장하면 안내가 완료되지 않는다.
6. 숫자 입력과 간편 프리셋 선택은 대화상자(`HasDialog`)로 뜬다. 튜토리얼 단계 판정이 `HasDialog`를 읽으므로, 같은 일을 하는 새 화면에서도 "지금 입력 중인지"를 같은 방식으로 알려야 한다.
7. 해상도 440×956(세로), 956×440(가로), PC 16:9·16:10·21:9에서 지정 컨트롤과 안내 카드가 잘리지 않아야 한다. 검증은 기본 글자 크기에서 한국어·영어로 한다. 글자 크기 배율 기능은 없다.
8. 새 화면은 공통 UI 계약([Shared_UI_Contract](../Implementation/Shared_UI_Contract.md))과 `AGENTS.md`의 신규 콘텐츠 UI 규칙을 따른다.

## 5. 새로 필요한 UI (튜토리얼이 요청, 아직 없음)

"칙령 한 문장 = 문제 체험 → 처방 한 가지 → 결과 확인" 3박자를 보여 주려면 칙령 화면에 다음 자리가 필요하다. 표시할 내용과 판정은 튜토리얼 쪽이 제공하고, UI는 **자리와 모양**을 마련한다.

| 번호 | 요청 | 설명 | 중요도 |
| --- | --- | --- | --- |
| N1 | 새 문장 표시 | 마지막으로 본 뒤 새로 열린 탭·그룹·옵션에만 "새 문장" 표시를 붙이고, 열어 보거나 바꾸면 지운다. 열린 규칙 목록의 변화를 입력으로 받는다. | 필수 |
| N2 | 접힘 기본값 | 새로 열린 항목이나 집중 항목만 펼치고, 이미 열려 있던 그룹은 접어 둔다. | 필수 |
| N3 | 요약 탭의 다음 문장 예고 | 지금 몇 번째 문장인지, 다음 문장의 이름과 해금 조건, 두루마리 쪽 진행(받은 쪽 수/전체 쪽 수, 현재 장 11개)을 보여 준다. 현재는 한 줄 문구 하나다. | 필수 |
| N4 | 3박자 안내 띠 | 그룹 상세 화면 위에 ① 직전 사냥에서 겪은 문제 요약 ② 처방 한 가지(간편 프리셋 선택 또는 옵션 하나) ③ 저장 뒤 달라진 점을 순서대로 보여 주는 칸. 내용은 튜토리얼이 채운다. | 필수 |
| N5 | 안내자 말풍선 자리 | 강조한 항목 근처에 안내 NPC의 짧은 대사(1~2줄)를 두는 자리. 지금은 마을 카드 한 장으로만 안내한다. | 선택 |
| N6 | 저장 직후 결과 확인 진입점 | 저장 뒤 "훈련장에서 확인" 또는 "다음 사냥에서 관찰" 같은 다음 행동을 한 개 누르게 하는 버튼 자리. | 선택 |

- N1~N4는 모든 해상도에서 칙령의 주 조작(저장·탭·옵션)을 가리거나 밀어내지 않아야 한다.
- 새 표시가 화면 갱신을 늘려 버튼 깜빡임을 만들지 않도록 4.4의 3번을 지킨다.

## 6. UI를 바꾸면 같이 고쳐야 하는 곳

튜토리얼 쪽(UI 작업 뒤 이어서 맞춘다):

- `GameUI.TutorialStaging.cs`: `LessonTarget`, `SkillLessonStep`, `SurvivalLessonStep`, `TickPrologueLesson`
- `GameUI.TutorialComparison.cs`: `ProgressiveSkillLessonStep`, `ProgressiveSurvivalLessonStep`
- `GameUI.Tutorials.cs`: `RefreshTutorialAnchor`, `StartTutorialAction`
- `GameUI.EdictNotices.cs`: 안내 카드와 대상 목록
- `HuntEdictWindow.Progression.cs`, `HuntEdictProgression.cs`, `HuntEdictProgression.json`: 공개 판정과 단계

스모크와 테스트(`edict-*` 이름 사용 횟수가 많은 순):

| 파일 | 이름 사용 횟수 |
| --- | --- |
| `RuntimeSkillTreeSmoke` (본체·Sections·Presets·CombatPreview·Identity) | 93 / 50 / 30 / 14 / 4 |
| `RuntimeTutorialSmoke.Progression`, `RuntimeTutorialSmoke` | 26, 17 |
| `RuntimeRecommendedEquipmentSmoke` (본체·Controls·Storage) | 25 / 23 / 10 |
| `RuntimeEdictQuickPresetSmoke` | 25 |
| `RuntimeSharedUiSmoke` | 24 |
| `RuntimeEdictOverviewSmoke` | 15 |
| `RuntimeRiftEntrySmoke` (Preparation·Repeat·Portal) | 9 / 5 / 3 |
| `RuntimeTrainingGroundSmoke`, `RuntimeLanguageSmoke`, `RuntimeUiStyleSmoke`, `RuntimeHuntEdictSaveLayoutSmoke`, `RuntimeButtonUxSmoke`, `RuntimeTrainingEquipmentSmoke` | 각 1~6 |
| Edit Mode: `HuntEdictProgressionTests`, `TutorialProgressionTests`, `TutorialChapterTests`, `RepeatHuntTests`, `HuntEdictDefaultsTests` | 공개 단계·안내 ID 검증 |

- 숫자는 소스에서 `"edict-…"` 문자열이 나온 횟수이며 개수 추정용이다. 실제로 영향받는 스모크는 이름을 바꾸는 범위에 따라 달라진다.
- 참고: 일부 스킬 스모크는 구형 편집기를 쓴다(통합 창이 아님). 개선 대상이 통합 `HuntEdictWindow`인지 구형인지 먼저 정한다.
- 칙령 단계를 바꾸면 안내 문구, `ValidateChange`의 오류 메시지, `HuntEdictProgressionTests`의 `TestCase` 숫자도 같이 바뀐다.

## 7. UI 작업자에게 요청하는 인계 사항

UI 작업을 마칠 때 다음을 남기면 튜토리얼 작업이 새 화면에 바로 이어진다.

1. **이름 대응표**: 4.1의 모든 이름이 새 UI에서 무엇이 되었는지 (유지, 이름 변경, 위치 이동, 삭제, 대체) 한 줄씩 적는다.
2. **구조 변경**: 탭·그룹 구성이 바뀌었다면 `HuntEdictUi.json`의 그룹 배치와 3.3 표의 "열리는 곳"이 어떻게 달라졌는지 적는다.
3. **창 상태와 동작**: 4.2에서 없어지거나 이름이 바뀐 항목, 새로 생긴 항목.
4. **지정 영역**: `Option area`와 `Preset explanation`을 유지하는지, 대체한다면 무엇인지, 진행 버튼(`edict-starter-next`)의 위치.
5. **5장의 N1~N4** 구현 여부와 입력받는 데이터 형태(튜토리얼이 무엇을 넘겨야 하는지).
6. 주요 화면 캡처: 요약 탭, 그룹 상세(간편 프리셋 선택 포함), 숫자 입력 대화상자, 스킬 탭과 사용 방식 화면. 440×956과 956×440, 한국어와 영어.
7. 고친 스모크와 테스트 목록, 실행한 검사와 결과, 실행하지 못한 검사.

## 8. 현재 구현 상태

| 내용 | `main` | `claude/tutorial-chronicle` (병합 전) |
| --- | --- | --- |
| 단계 공개(균열 2~20, 첫 결과) | 있음 | 있음 |
| 낮은 HP 대응 균열 3, 위치·거리 균열 5 | 반대(낮은 HP 5, 위치 3) | 맞바꿈 |
| 세밀 조건 | E18 한 묶음 40개 | E18 / E18B / E18C 세 묶음(20·13·7) |
| 장(章) 엔진(쪽 보상, 현재 장 추천) | 없음 | 있음(화면 없음) |
| 새 문장 표시, 접힘, 다음 문장 예고, 3박자 띠 | 없음 | 없음 |
| 장 대화·두루마리 창, 안내 NPC | 없음 | 없음 |

- 이 브랜치의 변경은 관련 Edit Mode 테스트까지 확인했다. 전체 검사와 런타임 스모크는 마지막에 한 번 실행할 예정이며 아직 하지 않았다.
- 이 문서가 설명하는 화면 동작은 코드를 읽은 결과이며, 이번 작업에서 게임을 실행해 다시 확인하지 않았다.

## 9. 근거 파일

- 프롤로그 강제 레슨: `Assets/HELLSCRIPT/Runtime/Presentation/GameUI.TutorialStaging.cs`, `GameUI.TutorialComparison.cs`
- 안내 연결·테두리: `GameUI.Tutorials.cs`, `GameUI.EdictNotices.cs`, `TutorialAnchorRing.cs`, `PrologueGate.cs`
- 칙령 창: `HuntEdictWindow.cs`, `HuntEdictWindow.Progression.cs`, `HuntEdictWindow.Overview.cs`, `HuntEdictWindow.Summary.cs`, `HuntEdictWindow.QuickPresets.cs`, `HuntEdictWindow.SkillPresets.cs`, `HuntEdictWindow.Skills.cs`
- 공개 규칙·데이터: `Assets/HELLSCRIPT/Runtime/Core/HuntEdictProgression.cs`, `Assets/HELLSCRIPT/Resources/HuntEdictProgression.json`, `HuntEdictUi.json`, `HuntEdictQuickPresets.json`
- 안내 정의와 판정: `Assets/HELLSCRIPT/Runtime/Core/Tutorials.cs`, `Tutorials.Progress.cs`
