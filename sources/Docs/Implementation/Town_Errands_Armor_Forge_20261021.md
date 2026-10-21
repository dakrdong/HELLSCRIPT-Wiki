# 마을 퀘스트 확장(방어구 심부름)·대장간 정리·튜토리얼 수정 (2026-10-21)

사용자 요청 16항목의 처리 기록이다. 영문본은 [Town_Errands_Armor_Forge_20261021.en.md](Town_Errands_Armor_Forge_20261021.en.md).

## 1. 스킬창·범위 공격 (항목 1~3)

- **직업 버튼 제거:** 게임의 스킬창(`SkillTreeD4Window`)은 저장소(`store`)를 받아 열릴 때 상단 직업 버튼 줄(`d4-tab-0/1/2`)을 그리지 않는다(`showClassTabs=store==null`). 시안·진단용 호스트(저장소 없음)는 그대로 그린다. 각 영웅은 자기 직업의 스킬창만 본다. 테스트 `SkillTreeD4StoreHostTests`가 버튼이 없음을 확인한다.
- **찢어낸 상처·회전 공격의 범위:** 원인은 W09의 대상 수 상한이었다. 범위 안의 적을 id 순으로 5명까지만 자르던 `Take(5)`가 있어 적이 6명 이상이면 일부가 출혈을 받지 않았다. 상한을 없애 부채꼴(회전 변형은 360° 원)·반경 안의 모든 적이 출혈을 받는다. 단일 대상 변형(깊은 상처)은 그대로 한 명이다. 테스트 `RakingWoundHitsEveryEnemyInItsAreaAndNoOtherEnemy`(기본·회전 두 경우)가 범위 안 7명 모두와 뒤·바깥의 적 두 명이 받지 않음을 확인한다.

## 2. 튜토리얼 문구·위치 (항목 4~5)

- **물약:** "물약 사용도 자동이며, 설정한 HP 아래로 떨어졌을 때 물약이 남아 있으면 자동으로 마신다"는 두 줄로 바꿨다. 지금 설정이 1% 이하만 마시도록 되어 있어 위험하니 왼쪽 아래 물약 설정에서 30% 이상으로 저장하라는 안내가 이어진다(저장 승인 범위 30~100%는 그대로).
- **첫 교전·전투 중 위치:** 원거리 무리(`GuideTutorial.RangedPack`)를 기둥 주변에 흩뜨리지 않고 한곳(약 2m 안)에 모았다. 영웅이 무리 중심으로 가서 싸우는 것은 하드코딩이 아니라 `무리 중심`(`position.mode=CENTER`, `EdictPackCenterGoal`) 칙령이 실제로 동작한 결과이며, `GuideHazardYardTests`가 영웅이 무리 중심에 2.5m 안으로 다가가고 6m 넘게 물러서지 않음을 확인한다.

## 3. 표시·장비 카드 (항목 6, 8~11)

- **레벨업 글꼴(6):** 제목·레벨 숫자는 캐주얼 표시 글꼴(`UiSkin.Casual.LabelFont`), 부제·확인 문구는 캐주얼 본문 글꼴이다(`LevelUpCelebration`).
- **보물 고블린 뒤 대기(8):** 고블린을 잡은 뒤 회수 수업까지 2초 기다린다(`GuideTutorial.LootLessonSeconds=2`).
- **장비 추천 카드 크기(9):** 시안의 카드(92px 폭, 가로 화면은 0.75배)를 화면 크기에 맞춰 키운다. 카드·글자·아이콘 모두 배율 `k`로 처음부터 그려 글자가 흐려지지 않는다(`localScale` 미사용). 폰 세로 폭 440px 기준 1배에서 폭이 넓은 화면일수록 커진다.
- **장비 카드 튜토리얼 순서(10):** 아이템 획득 → 추천 카드가 나타남 → 카드에 강조 링이 2초 깜빡임 → 벨리나 대사 순서다(`GuideGearBeatWaits`, `GuideGearFlashSeconds=2`). 대사는 깜빡임이 끝나야 시작한다. 헬멧·희귀 검·낮은 검 세 수업이 같다.
- **퀘스트 화살표(11):** 노란 "Y"처럼 보이던 큐브 세 개를 평평한 화살표 한 장(몸통+삼각 머리)으로 바꾸고 남색 윤곽을 겹쳐 모래·풀 위에서 또렷하게 했다(`WorldView.TownGuide.cs`).

## 4. 퀘스트 흐름 (항목 12, 15, 16)

저장 값 `AccountGuide.townQuest`: 0 없음, 1 안톤과 대화, 2 무기 강화, 3 완료, **4 방어구 받기, 5 균열 1레벨**. 1~3은 이미 저장된 값이라 바꾸지 않았고 순서는 `TownQuest.Next`가 정한다(0→1→2→4→5→3). 새 저장 필드는 `townEnhanced`(무기를 강화했음)와 `townArmorGiven`(선물 방어구를 받음)이며, 없는 저장은 거짓으로 읽힌다.

| 단계 | 목표 줄 | 안내 | 균열 |
| --- | --- | --- | --- |
| 2 무기 강화 | 대장간에서 무기를 강화하세요 | 대장장이 마르크 | 잠김(안톤: "어서 대장간을 다녀오게") |
| 4 방어구 | 방어구 구입 후 착용하기 | 무기 상인 제이크 보쿤 | 잠김(안톤: "무기만 손봐서는 아직 부족하네…") |
| 5 균열 1레벨 | 균열 1레벨 진행 | 균열지기 안톤 | 열림 |

- **대장간 강화 수업(12):** 강화 탭이 열리면 `+1레벨 강화` 단추만 남기고 화면 전체를 어둡게 덮는다. 장비 추천 수업과 같은 `PrologueGate`(캔버스 정렬 2000)를 써서 창 위에 그려지고, 단추가 2초 넘게 사라지면 가림이 풀려 갇히지 않는다(`GameUI.TownErrands.cs`의 `TownStep`). 예전의 링은 없앴다.
- **대장장이 배웅(15):** 강화가 성공하면 `townEnhanced`만 저장하고 퀘스트는 그대로 2다. 대장간 창을 닫으면 마르크가 "아직 균열 1레벨도 힘들다, 무기 상점에 가 보라, 내가 말해 두었으니 방어구 하나쯤은 공짜일 것, 하나 더 입고 가라"고 배웅하고, 선택 `무기 상점으로 간다`를 누르면 4로 넘어간다. 중간에 닫으면 다음에 다시 나온다(촌장 환영과 같은 방식).
- **제이크의 선물(16):** 4일 때 무기 상점(제이크)과 처음 이야기하면 대사 5줄(마르크가 말한 신입이로군 → 그 장비로 균열이라니 → 용사들이 오래 버텨야 가게도 산다 → 한 턱 쏜다 → 진열대 맨 위에 두었다)이 나오고, 선택 `진열대를 본다`로 장비 상점의 구매 탭이 열린다. 대사는 제이크의 말투(제육볶음 비유, 호탕한 반말)로 다시 썼다.
- **0골드 선물 방어구:** 4가 열리면 상점 목록 맨 위에 영웅별로 고정된 매직 철 갑옷(`GuideTutorial.ArmorKind`, 기본 B13, 옵션 AF03·AF01)이 가격 0으로 들어간다(`ShopOffer.gift`). 재입고가 와도 받기 전까지 맨 위에 남고 받은 뒤에는 다시 나오지 않는다(`EquipmentShop.EnsureGift`, `townArmorGiven`). 받기는 일반 구매와 달리 골드·구매 한도(`WithinBudget`)·구매 기록을 쓰지 않으며 영웅이 직접 입는다. 이 아이템은 `/town/` 아이디를 써서 가이드 스테이지 재시작이 가져가지 않는다.
- **구매·착용 단계:** 상점이 열리면 화면을 어둡게 덮고 `구매` 탭 → 선물 행 → `구매하기` → 상점 닫기 → 캐릭터 단추 → 가방의 방어구 → `장착` 순서로 한 곳씩 짚는다. 같은 방식의 `TownStep`이며 매 단계가 저장과 화면 상태를 읽는다(숫자 카운터 없음). 방어구가 착용되었거나 가방에서 사라지면(팔았음) 단계가 끝난 것으로 본다(`TownQuest.ArmorSettled`).
- **제이크 배웅:** 창을 모두 닫으면 "균열 1레벨쯤은 해볼 만하겠어, 살아서 다음에도 우리 상점을 이용해 달라"는 두 줄이 나오고 선택 `균열로 향한다`를 누르면 5가 된다. 목표 줄과 안내 화살표가 안톤을 가리킨다.
- **균열 입장·완료:** 5에서 균열 시설과 입장 경로는 막히지 않는다. 처음 균열(`BeginRun`, 이어 하기 제외)에 들어가면 퀘스트가 3(완료)이 된다.

## 5. 대장간 (항목 13, 14)

- **장비 제작 탭(13) — 삭제(2026-10-21 사용자 결정):** 다섯 번째 탭은 쓰지 않기로 해 대장간을 다시 네 탭(옵션 변경·슬롯 강화·장비 강화·코어 제작)으로 되돌렸다. 이어서 사용자가 '희귀 장비 제작' 기능 전체를 원하지 않는다고 해 탭 화면(`BlacksmithWindow.CraftScreen.cs`)과 전용 거래 `GameStore.CraftRareEquipment`뿐 아니라 상점 '장비 제작' 화면의 `희귀 제작` 단추, `ContentUnlocks.RareCraft`(UL04, `ContentUnlocks.json`의 정의 포함), '희귀 장비 제작' 튜토리얼과 실습, `ContentServices.Purchase`의 모드 1(번호는 그대로 두고 거절), 관련 문구·시험·문서를 모두 지웠다. 이미 제작한 장비는 일반 아이템으로 남고, 옛 저장의 `UL04_RARE_CRAFT` 값은 읽어도 무시된다(저장 구조·키 변경 없음). 남은 상점 '장비 제작' 화면은 갬블 상인·코어 제작 이동 단추와 도감 단추뿐이라 쓸모가 없어 마을 메뉴 버튼과 화면(`GameUI.ShowShop`·`RenderItemShop`)까지 지웠다. 갬블 상인은 마을 NPC(`TownStation.Gambler`), 코어 제작은 대장간 NPC·가방의 강화/재굴림 버튼, 전설·세트 도감(`ShowItemCollection`)은 콘텐츠 해금 화면과 가방에서 계속 열린다.
- **탭 잠금(14):** 졸업 때 모든 콘텐츠가 열려 있어 잠금이 보이지 않았다. 졸업 계정(`everythingOpen`)의 대장간은 서비스별 균열 단계를 다시 따른다(`ContentUnlocks.ForgeHas`): 장비 강화 1, 슬롯 강화 5, 옵션 변경 35, 코어 제작 50(`ContentUnlocks.json`의 값). 예외는 마을 퀘스트 2단계의 첫 강화 하나뿐이다. 잠긴 탭은 자물쇠로 보이고 누르면 해제 조건을 말풍선으로 알린다(기존 `ShowServiceHint`). 서비스가 모두 잠기면 기존 "잠긴 대장간" 화면이 나온다. 엔진(`Blacksmith.cs`·`Economy.cs`·`CoreCrafting.cs`·`ContentServices`·`GameStore.ForgeAccess`)도 같은 규칙을 적용해 화면 밖 호출로 우회할 수 없다. 졸업하지 않은 기존 계정은 받은 접근을 그대로 쓴다.

## 6. 보물 고블린 우선순위 수업 (항목 7)

코드를 읽고 안내 스모크로 확인했다. 한국어 세로(440×956)와 영어 가로(956×440) 모두 "사냥 칙령 버튼 → 전투 → 공격 대상 → 몬스터 등급 → '보물 몬스터' 카드 끌어올리기" 단계에서 카드 강조 링과 끌기 안내(드래그 힌트)가 나왔고 벨리나의 대사도 이어졌다. 즉 같은 흐름에서는 재현되지 않아 코드를 바꾸지 않았다. 사용자의 기기에서 안내가 사라진 정확한 조건(화면 크기·언어·직전에 한 조작)이 있으면 그 조건으로 다시 확인해야 한다.

## 7. 바뀐 파일

`TownQuest.cs`, `GameStore.TownQuest.cs`, `GameStore.Blacksmith.cs`, `GameStore.EquipmentShop.cs`, `EquipmentShop.cs`, `GuideTutorial.cs`, `FirstPlayGuide.cs`, `ContentUnlocks.cs`, `Economy.cs`, `Economy.ContentServices.cs`, `Blacksmith.cs`, `CoreCrafting.cs`, `CombatSimulation.D4Warrior.cs`, `GameController.cs`, `GameUI.TownErrands.cs`(신규), `GameUI.TownQuest.cs`, `GameUI.Plaza.cs`, `GameUI.NpcDialogue.cs`, `GameUI.Tutorials.cs`, `GameUI.TutorialStaging.cs`, `GameUI.GuideGearLesson.cs`, `GameUI.GearRecommendations.cs`, `GameUI.GuideTutorial.cs`, `BlacksmithWindow*.cs`, `EquipmentShopWindow.MockDetail.cs`, `LevelUpCelebration.cs`, `SkillTreeD4Window*.cs`, `WorldView.TownGuide.cs`, `en.txt`, 테스트(`TownQuestTests`·`GuideTutorialTests`·`GuideHazardYardTests`·`SkillTreeD4StoreHostTests`)와 안내 스모크(`RuntimeGuideTutorialSmoke`).

## 8. 검증

실제 실행한 것만 적는다.

- **Edit Mode(영향 영역만, 전체 실행 아님):** `TownQuestTests`(새 시험 포함: 선물 방어구 흐름·재입고·재방문·저장 복원·대장간 단계 잠금), `ContentUnlockTests`, `RiftContentUnlockTests`, `BlacksmithTests`, `BlacksmithCasualSkinTests`, `CoreCraftingTests`, `EquipmentShopTests`, `ForgePresentationAllocationTests`, `GuideGearTests`, `NpcDialogueTests`, `GuideTutorialTests`(찢어낸 상처·회전의 범위 시험 포함), `GuideHazardYardTests`(무리 중심 시험 포함), `SkillTreeD4StoreHostTests`, `LocalizationTests`. 모두 통과했고 실패는 `NoEntryIsLeftBehindByARewordedLine` 하나뿐이다. 이 실패는 `en.txt`에 있는 9개 항목(`고정 저항`, `보조무기에 장착`, `추천 후보` 등)의 원본 문장이 소스에 없어서 생기며 기준 커밋(a4b40e46)에도 같은 항목이 같은 상태로 있어 이번 변경과 무관하다. 번역 누락 시험(`EveryKoreanLiteralInTheRuntimeHasAnEntry`, `EveryEnglishEntryLeavesNoKoreanBehindAndKeepsItsPlaceholders`)은 이번 작업에서 두 번 걸렸고 고친 뒤 통과했다.
- **UI 계약:** `tools/check_ui_contract.py --verify` 15개 통과.
- **안내 스모크(개발용 macOS 플레이어, 버려도 되는 손님 저장):** 한국어 440×956과 영어 956×440에서 새 마을 흐름 전체를 지났다: 강화 수업의 화면 딤 → 강화 → 대장장이 배웅 → 방어구 목표 → 균열 잠금 문구 → 제이크의 선물 대사 → 상점 구매 탭·선물 행·구매하기 → 상점 닫기 → 메뉴 → 캐릭터 → 가방의 방어구 → 장착 → 제이크 배웅 → 목표 `균열 1레벨 진행`과 안톤을 가리키는 화살표. 장비 추천 카드·퀘스트 화살표·레벨업 글꼴·보물 수업의 캡처도 확인했다.
- **스모크의 마지막 부분은 통과하지 못했다:** 흐름 뒤의 "마을 HUD 단추" 검사(`CheckHudButtons("town")`)가 사냥 칙령 시트의 재생 단추를 찾는 부분에서 실패했다. 마을의 칙령 시트 목록 화면에는 재생 단추가 없어서(싸움이 없으면 볼 것이 없다) 이 검사가 그 화면을 예전 모양으로 가정한 것으로 보이며, 이번 변경과 무관하다고 판단해 단추가 없거나 꺼져 있으면 통과하도록 고쳤다. 고친 뒤에는 플레이어를 다시 만들지 않았으므로 그 뒤 검사(물약 패널 등)와 스모크의 최종 OK 줄은 확인하지 못했다.
- **하지 않은 것:** 전체 Edit Mode, 실기기, 성능 측정, 장비 카드 2초 깜빡임의 눈으로 보는 확인(캡처는 카드와 대사 순서만 보여 준다), 대장간 딤의 진하기에 대한 사용자 확인. 사용자 테스트 환경·세이브는 건드리지 않았다.


## 2026-10-21 Codex 최종 통합 검증

원본 town `907c8bf82bffd970943321aafaeb25d5f779ee95`와 UI `8c104054d46de8c5587710b0bfd10428798c7118`의 이력을 보존해 함께 통합했다. 시작 main은 `25faf86714ab7097f5ccd2245ce91333baf272ab`이며 이후 main `f276442aaa9cdbc3524580c979174509a557953f`와 실행기 최종 커밋 `ac93266fadb31529bffe112eca9e9138de7b314f`도 포함했다. 영어 표 끝의 독립 추가 문구는 양쪽 모두 유지했다. 기존 W09 광역 상한 제거와 전역 광역 회귀를 보존했다.

- 실제 Unity 6000.6.0f1에서 명시한 35개 영향 클래스와 광역 회귀 55개 메서드를 검사했다. 최초 `178177bf3db6049377ae12826c78dea4917b8e2c`의 730개 중 729개 통과, 1개 실패, 건너뜀 0개였다. 실패 원인인 옛 장비 번역 9개가 기준 main과 후보의 런타임·데이터에서 참조되지 않음을 확인하고 원문을 복구 자료에 보존한 뒤 제거했다. 검사나 조합 문구 예외를 바꾸지 않았다. 지역화 34/34 재검사가 통과했다.
- 공식 리뷰에서 소지품이 가득 찼을 때 선물 구매 안내가 판매·닫기까지 막는 문제와 영어 세로 DPS 비교 문구의 줄바꿈 누락을 확인했다. 빈 칸이 없으면 안내 게이트를 풀고, DPS 비교 문구에 줄바꿈과 확보된 두 번째 줄 높이를 적용했다. 공간을 비우면 안내가 재개되는 회귀를 추가했다. `b63d66013f560a79578b9975ef24be8521336dc5`에서 직접 영향받는 6개 클래스 69/69가 통과했다. 기존 68개 재검사와 새 회귀 1개이며, 나머지 662개 성공 사례는 변경 없는 입력의 근거를 재사용한다. 전체 EditMode를 한 번에 실행한 결과가 아니다.
- 새 룬 대화상자의 직접 색상 8곳은 값이 같은 공통 토큰으로 바꿨다. UI 계약 15개와 생성 데이터·레거시 목록·UI 갱신 검사가 통과했다. 최종 실행기 커밋을 합친 뒤 PR 이벤트 단위 검사 17/17도 통과했다. 최종 SHA의 CI·리뷰 상태는 아래 PR 기록을 따른다.
- 실제 macOS Metal 개발 플레이어에서 한국어 440×956와 영어 956×440 마을 흐름이 각각 마지막 HUD·위치·물약 확인까지 통과했다(각 169장). 합성 EventSystem 포인터와 가속한 실제 전투 틱을 썼다. 일반 속도나 실기기·사용자 이해도 검증으로 해석하지 않는다.
- 캐릭터 화면의 한·영·PC 16:9/16:10/울트라와 세로·가로 창 검증이 통과했다(26장). 공통 HUD 재검사도 통과했으며, 실제 직전 판 비교를 넣은 영어 세로 DPS 문구의 두 줄·전체 글자·패널 내부 렌더링 회귀를 포함했다(20장).
- 룬은 한·영 6개 화면 크기에서 선택 상태·설명 창 7종·5단계 연습과 안전 영역 검증이 통과했다(73장, 번역 누락 0). macOS InputSystem 마우스와 스크롤 보조를 사용했다. 현재 시안의 형제 라벨·기준선 행 상자와 PC의 내용별 카드 높이에 맞춰 개발용 검증기를 갱신했다. 휴대폰 높이 고정·PC 패널 겹침·글자 경계 검사는 유지했고 가로 폰은 모바일 배치 분기를 사용했다. 최초 불완전 시험 데이터와 과거 검증기 가정으로 실패한 기록도 보존했다.

검증·최종 리뷰·병합 상태는 [통합 PR #179](https://github.com/dakrdong/HELLSCRIPT/pull/179)와 로컬 `Builds/Validation/FinalIntegration20261021` 근거를 따른다. 일회용 오프라인 시험 데이터만 사용했고 실제 사용자 세이브·테스트본은 변경하지 않았다. 전체 EditMode·실기기·성능 검증은 하지 않았다. 보물 고블린 재현 조건은 여전히 미해결이다. 이후 사용자 요청으로 완료된 `a8a3918c261d7b5f7b4f0d7d4709166e0408cfc1` 인계를 이 후보에 포함해 희귀 제작 기능·다섯 번째 탭·옛 마을 제작 허브를 제거했다. 검증과 최종 병합 상태는 PR 기록을 따른다.
