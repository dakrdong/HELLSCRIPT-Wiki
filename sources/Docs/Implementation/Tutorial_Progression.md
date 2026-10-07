# 튜토리얼 구현과 검증

## 2026-10-07 퍼즐 코어

신규 필수 진입은 v5 레벨 1·2로 바뀌었다. `AccountGuide.puzzle`이 소유 영웅, 현재 레벨, 시도·클리어·힌트, 고정 시작 키트를 저장하고 `GameStore.Puzzle.cs`가 첫 클리어 경험치와 재도전 소유권을 검증한다. 칙령 공개와 스킬 트랙은 일반 균열 진행보다 레벨 목록을 먼저 사용한다. 저장 복원은 `tutorial-v5`와 파 순번으로 중복 스폰을 막는다. 아래 과거 프롤로그 표와 검사 수치는 당시 결과다. [현재 코어 기록](Puzzle_Tutorial_Core.md).

갱신일: 2026-10-06

[기획](../Design/Tutorial_Flow_Design.md)의 38개 안내 그룹을 기존 첫 플레이 안내와 콘텐츠 개방 흐름에 연결한다. 최초 맵, 실제 인벤토리 장착, 마을 도착, 균열, 사냥칙령·스킬 변경과 콘텐츠 실습을 같은 저장 기록에서 추적한다.

> 2026-09-29: 필수 맵은 「칙령의 목소리」 프롤로그로 바뀌었다. 갑옷 지급과 장착 단계는 없어졌고, 현재 흐름은 [프롤로그 — 칙령의 목소리](Prologue_Edict_Voice.md)를 따른다.

## 2026-10-06 이동기 안내

궁수 A04·마법사 M04의 다음 성장 해금은 5레벨이고 전사 W04는 10레벨이다. F06은 첫 스킬 이후 아직 장착한 적 없는 액티브를 안내한다. 5레벨 후보의 마지막에 A04/M04가 추가되며 A02/M02 추천 순서는 유지한다. 퍼즐 튜토리얼의 이동기 학습은 별도 단계에서 안내할 예정이며 이 변경만으로 퍼즐 튜토리얼이 완성되지는 않는다. 아래 2026-09-29의 10레벨 검증은 당시 결과다. [구현 기록](Escape_Skill_Unlock_Level.md).

## 소유 구조

| 영역 | 소유 코드 | 책임 |
| --- | --- | --- |
| 안내 정의와 상태 | `Tutorials.cs`, `FirstPlayGuide.cs` | 계정·영웅 범위, 읽음·실제 수행·미루기·숨김, 구버전 이전, 개방 순서 |
| 실제 수행 판정 | `Tutorials.Progress.cs` | 실제 저장한 물약 HP 조건, 새로 장착한 성장 스킬, 동일 설정 훈련 후 새 균열 |
| 전용 맵 | `CombatSimulation.Tutorial.cs` | 고정 3개 방, 실제 적·보스 전투, 장착 대기, 사망 구간 재시작 |
| 저장과 일회성 지급 | `GameStore.Tutorials.cs` | 맵 시작·갑옷 지급·마을 도착을 검증하고 저장 |
| 실습 지원 | `TutorialPractice.cs` | 현재 비용·소유품으로 견적 작성, 기존 도메인 실행, 결과와 지원 사용 기록의 원자적 저장 |
| 게임 진행 연결 | `GameController.Tutorials.cs` | 신규 계정 분기, 재시작 복원, 다시보기 복사본, 필수 맵 이탈 방지 |
| 안내 화면 | `TutorialJournalWindow.cs`, `GameUI.Tutorials.cs` | 공통 창 템플릿, 목록·상세·다시보기, 미루기와 숨김, 전투 중 비차단 안내 |
| 연출과 대화 | `GameUI.TutorialStaging.cs`, `StoryDialogueWindow.cs`, `TutorialCinematic.cs` | 첫 맵의 단계별 장면·대사·카메라, 공통 게임 대화창, 주민의 콘텐츠 첫 이용 안내. [튜토리얼 연출](Tutorial_Staging.md) |
| 실습 선택 화면 | `GameUI.TutorialPractice.cs` | 실제 소유품 선택, 공통 장비 상세, 현재 지원 견적과 실행 결과 |
| 조작 대상 표시 | `TutorialAnchorRing.cs` | 실제 버튼과 소유 아이템 칸에 붙는 맥동 테두리와 화살표, 화면 좌표를 저장하지 않는 표시 |

`AccountGuide`가 새 안내 기록을 소유한다. 기존 `HeroGuide.completed`는 이전 데이터와 기존 판정 코드의 호환성을 위해 유지하며, 사용자 진입점은 새 안내 일지로 통합한다. 기존의 9단계 전체 화면을 별도 사용자 안내로 운영하지 않는다.

## 저장과 이전

저장 스키마는 17이다. 운영 설정을 사용하던 정식 스키마 16도 기존 계정으로 안전하게 이전한다. 튜토리얼 정의 버전은 1이며, 지원 사용 기록과 갑옷 지급 영수증은 정의 문구가 바뀌어도 초기화하지 않는다. 신규 계정은 맵 미완료 상태로 시작한다. 구버전 계정은 `legacyExempt`로 면제하며 완료했다고 기록하지 않는다. 기존 실제 균열 입장·결과·재도전·소유 상태 훈련·실제 장비 비교 기록은 의미가 같은 새 안내로 이전한다. 장비가 없어 비교 설명만 확인한 기록은 실습 완료로 이전하지 않는다. 구 콘텐츠의 확인/건너뛰기 기록은 읽음으로만 이전한다.

`GameStore.Transact`가 계정 안내도 저장 성공 후 채택하도록 확장했다. 신규 계정 식별자 역시 최초 생성 시 확정해 저장 전후와 재시작 이후에 달라지지 않게 했다. 디스크 저장 실패 시 지원 재료, 소비, 결과, 완료 표시를 실제 계정에 채택하지 않는다.

필수 맵은 일반 균열의 `suspendedRun` 대신 `AccountGuide.tutorialRun`에 저장한다. 실행에는 명시적인 `tutorial` 표지가 있으며, 기존 무보상 실행 구분과 함께 경험치·재화·피로도·균열 기록·미접속 보급 기준을 차단한다. 스킬과 장비는 실제 레벨의 능력치를 사용한다. 튜토리얼 갑옷은 완료 전 잠금 해제·장착 해제·창고 이동을 막아 필수 장착 대상이 사라지지 않게 한다. 다시보기는 별도 레벨 1 계정 복사본을 사용한다.

## 최초 실습 지원

| 콘텐츠 | 지원하는 한 번의 실행 | 기존 실행 경로 |
| --- | --- | --- |
| 장비 강화 | 소유 +0 장비 → +1의 골드 | `GearEnhancement.Quote/Apply` |
| 희귀 제작 | 선택 부위 장비 1개의 골드·재료 | `ContentServices.Purchase` |
| 부위 강화 | 1 → 2의 강화석, 정상 5분 대기 | `BlacksmithCatalog.Start` |
| 보석 합성 | 선택 T1 보석 5개 → T2 1개 | `Jeweler.Convert` |
| 룬 구입 | G0 한 칸 룬 1개의 골드 | `TownTrade.BuyRune` |
| 미확인 장비 | 선택 부위 장비 1개의 현재 가격 | `GambleShop.Price/Buy` |
| 속성 재설정 | 소유 장비의 선택한 한 줄, 1회 골드 | `Economy.Reroll` |
| 영약 제조 | 선택 T1 보석 1개 → 영약 1개 | `Jeweler.Craft` |
| 명품화 | 실제 +5 이상 장비, 명품화 0 → 1의 골드·재료 | `ItemQuality.Advance` |
| 코어 제작 | 선택 제작법의 부위 코어 10개, 유료 투자 0 | `CoreCrafting.Quote/Apply` |

지원 선택 화면은 가상의 장비를 소유품처럼 표시하지 않는다. 장비는 실제 소유품을, 코어 제작법은 `EquipmentViewSource.Catalog`를 전달한다. 실행 시 NPC의 실제 거리, 개방 상태, 선택의 변경 여부, 소유권과 공간을 다시 확인한다. 견적 단계에서는 재료를 지급하지 않는다. 중복 요청은 기존 영수증으로 처리한다.

훈련, 미접속 보급, 위상 각인, 소탕은 기존 무료 기능과 정상 한도를 사용한다. 최초 안내라는 이유로 위상·전설·레벨·소탕 횟수를 만들지 않는다.

## 화면과 진행 정책

2026-09-29: 튜토리얼·퀘스트·임무 안내의 상시 노출은 추후 구성하기로 하여 마을과 전투 화면 왼쪽 위의 ‘모험 안내’ 버튼을 제거했다. 이 버튼으로 유도하던 ‘새 안내’ 알림도 제거했다. 출석 아이콘의 크기와 위치는 유지한다. 이번 변경은 HUD 바로가기 정리이며, 기존 첫 맵 진행·저장 기록과 콘텐츠에서 직접 여는 안내 일지는 그대로 둔다.

이번 HUD 정리는 macOS 개발 빌드와 기존 출석 런타임 검사로 검증했다. 5개 화면 크기·한국어/영어·글자 100%/150%를 포함한 56개 배치 검사, 클릭 98회와 드래그 22회가 통과했다. [마을 화면](AdventureGuideRemovalEvidence/plaza-without-guide.png)과 [가로 전투 화면](AdventureGuideRemovalEvidence/battle-without-guide.png)에서 버튼 제거와 출석 아이콘 유지를 확인했다. 공통 UI 검사와 검사 도구 테스트 9개도 통과했다. 관련 Edit Mode 검사는 55개 통과·1개 실패이며, 실패한 기존 스킬/훈련 검사는 수정 전 코드에서도 같은 오류로 재현됐다. [검증 요약](AdventureGuideRemovalEvidence/validation.json)과 [수정 전후 비교](AdventureGuideRemovalEvidence/test-comparison.json)를 남겼다. 모바일 실기기는 확인하지 않았고 웹·APK는 다시 빌드하지 않았다.

2026-09-29: F06 「새 스킬 장착」의 기준을 **첫 스킬 다음에 장착하는 새 액티브**로 바꿨다. 직업별 첫 줄 액티브 세 종은 모두 트리 1레벨에 열려 있고, 레벨 성장으로 열리는 첫 액티브는 10레벨의 지면 강타·후퇴 도약·순간이동이다. 이전 코드는 이 구조와 맞지 않았다. 진행 가능 표시는 3레벨부터였지만, 완료하려면 트리 레벨이 1보다 높은 액티브를 장착해야 했다. 추천은 스킬 카탈로그의 해금 레벨로 대상을 골라 트리와도 어긋났다. 그 결과 3~9레벨에는 수행할 수 없는 실습이 안내 일지에 남았다. 코드 기준으로는 F06이 다음 안내로 남아 있는 동안 출석·미접속 보급의 자동 팝업도 미뤄질 수 있었다.

이제 F06은 첫 스킬 다음으로, 아직 슬롯에 넣은 적 없는 액티브를 처음 장착하고 저장할 때 완료된다. 1레벨 포인트는 첫 스킬(프롤로그의 W01·A01·M01)에 쓰이므로 두 번째 포인트를 받는 2레벨부터 진행 가능이 된다. 진행 가능 여부, 완료와 추천은 모두 `TutorialProgress.NewSkills`가 만든 같은 목록을 쓴다. 이 목록은 스킬 트리의 해금 상태와 영웅의 장착 기록으로 계산한다. 해금 레벨을 숫자로 고정하지 않으므로, 첫 줄 액티브를 이미 모두 장착해 본 기존 캐릭터는 트리의 다음 액티브가 열리는 레벨(현재 10)에 다시 진행 가능이 된다. 첫 스킬을 다른 슬롯으로 옮기는 것은 새 스킬로 세지 않는다. 추천은 이미 배운 새 스킬을 먼저 고른다. 배운 새 스킬이 없으면 남은 포인트로 배울 수 있는 새 스킬을 고른다. 이전에는 등급을 투자한 스킬만 추천했다. 안내 본문의 한국어와 영어, 영어 제목도 함께 바꿨다. 스킬 카탈로그의 해금 레벨을 1로 맞추는 작업과 신규 계정의 모든 영웅을 스킬 없이 시작하는 작업은 이 기록 시점에 `main`에 들어오지 않았다. 새 판정은 카탈로그를 읽지 않으므로 카탈로그 변경의 영향을 받지 않는다. 신규 계정의 영웅이 스킬 없이 시작하게 되면, 프롤로그를 진행하지 않은 영웅은 처음 장착한 액티브가 첫 스킬이 되고 그다음 새 액티브가 F06을 완료한다. 이 부분은 코드로 확인했으며 해당 브랜치에서 실행하지는 않았다.

복제 프로젝트의 배치 모드 Edit Mode에서 19개 클래스의 429개 검사가 모두 통과했다. 대상은 튜토리얼, 첫 플레이, 현지화, 저장 문구, 전투 기록, 스킬 트리, 사냥 칙령과 성장 검사다. 전체 Edit Mode는 4,736개 중 4,689개가 통과하고 47개가 실패했다. 실패 47개는 이 작업 전인 2026-09-27에 기록한 기존 실패 목록과 같고, 새로 생긴 실패는 없다([전체 검사 비교](TutorialNewSkillRuleEvidence/full-suite-comparison.json)). 새 검사 `NewSkillIsAnActiveNeverEquippedAfterTheFirstSkill`은 세 직업 모두에서 다음을 확인한다.

- 프롤로그 준비 상태에서 첫 스킬을 배우고 장착해도 F06은 완료되지 않는다.
- 2레벨이 되면 진행 가능이 되고, 추천은 남은 포인트로 배울 수 있는 새 스킬을 가리킨다.
- 첫 스킬을 다른 슬롯으로 옮겨도 완료되지 않는다. 새 스킬을 장착하고 저장하면 완료된다.
- 열린 액티브를 모두 써 본 영웅은 트리의 다음 액티브가 열리는 레벨에 다시 진행 가능이 된다.

같은 검사를 이전 규칙으로 되돌린 코드에서 실행하면 4개가 실패한다. 새 검사 3개는 2레벨에서 진행 가능인지 확인하는 단언에서 실패하고, 기존 검사 1개는 W02를 장착한 뒤 완료되었는지 확인하는 단언에서 실패한다.

macOS 개발 빌드의 튜토리얼 스모크는 두 프로세스 모두 통과했다. 마을에 도착한 직후인 1레벨에는 F06이 진행 가능이 아니었다. 첫 균열을 마쳐 4레벨이 된 뒤에는 진행 가능이었다. 전사가 도약 내려찍기(W02)를 포인터로 배우고 2번 슬롯에 장착한 뒤 저장해 F06을 완료했다. 레벨 보정은 사용하지 않았다. 바뀐 F06 설명 창은 20개 조합에서 배치 검사를 통과했다. 조합은 5개 화면 크기, 한국어·영어, 글자 크기 100%·150%이다. 공통 UI 검사와 검사 도구 테스트 9개도 통과했다. 모바일 실기기에서는 확인하지 않았고, 웹과 APK는 다시 빌드하지 않았다. [검증 요약](TutorialNewSkillRuleEvidence/validation.json) · [Edit Mode 결과](TutorialNewSkillRuleEvidence/editmode.xml) · [이전 규칙 검사](TutorialNewSkillRuleEvidence/editmode-old-rule.xml) · [첫 프로세스](TutorialNewSkillRuleEvidence/phase-one.txt) · [복원 이후 결과](TutorialNewSkillRuleEvidence/runtime-tutorial-smoke.txt) · [새 스킬 장착 화면](TutorialNewSkillRuleEvidence/new-skill-equipped.png) · [세로 한국어 150%](TutorialNewSkillRuleEvidence/f06-440x956-ko-150.png) · [가로 영어 150%](TutorialNewSkillRuleEvidence/f06-956x440-en-150.png)

모든 새 문구는 기존 한국어 키와 `Resources/Localization/en.txt` 번역표를 사용한다. 38개 그룹뿐 아니라 전설·세트·각성과 여덟 가지 균열 목표·조우에도 별도 설명을 제공한다.

`tools/new_content_ui.py TutorialJournal`에서 생성한 공통 창 진입점을 사용한다. 장비 상세·비교·장착은 기존 소유 부품과 거래를 사용한다. 화면 회전·안전 영역·언어·글자 크기는 공통 창이 다시 배치하며, 버튼 테두리는 실제 조작 대상의 `RectTransform`에 붙는다.

2026-09-28부터 필수 맵의 설명은 균열지기 안톤 진다크의 대사와 장면 연출로 진행하고, 콘텐츠를 처음 열 때는 해당 주민이 공통 대화창으로 설명한다. 순서와 규칙은 [튜토리얼 연출](Tutorial_Staging.md)을 따른다. 필수 맵의 설명과 장착 단계는 안전하게 멈춘다. 일반 균열에서는 긴 안내창을 열지 않고 짧은 안내만 표시한다. 콘텐츠를 처음 이용할 때 해당 안내를 제안하며, 사용자는 읽기·나중에 보기·숨기기를 선택할 수 있다. 활성 실습이 있으면 다른 안내가 중간에 끼어들지 않는다. 출석 기록과 미접속 정산은 유지하면서 필수 맵·첫 입장 준비·진행 중인 전투에서는 자동 팝업만 미룬다.

## 개발 순서와 검증

1. 기존 첫 플레이·개방·저장·공통 UI 소유자를 확인하고 최신 `main`의 별도 작업 폴더를 만들었다.
2. 버전이 있는 계정 기록, 구버전 이전, 실습 비용 거래를 구현했다.
3. 전용 맵과 실제 장착·보스·마을 복원을 연결했다.
4. 실제 균열, 칙령·스킬 저장, 훈련과 재도전 판정을 연결했다.
5. 14개 콘텐츠 안내와 16개 상황 안내, 세부 조우 기록을 연결했다.
6. 관련 Edit Mode 검사와 macOS 런타임 조작·화면 배치를 검증했다.
7. 위키 생성·검사를 통과한 작업을 커밋·푸시하고, 2026-09-25에 [PR #11](https://github.com/dakrdong/HELLSCRIPT/pull/11)을 `main`에 병합했다. 공개 위키는 병합된 `main`에서만 게시한다.

최종 검증 기준은 Unity 6000.6.0f1과 `main` 커밋 `32baf0fcb3d0d8872eb9188c699604b52e6a432f`이다. 초기 전체 회귀 감사는 `bdf659139c7705f59047eccd57111b07eccf1d66`에서 수행했다. 원래 작업 폴더의 408개 아트 메타데이터 변경은 그대로 두고 별도 작업 폴더에서 구현했다. macOS 빌드는 실행 중인 원본 에디터를 건드리지 않도록 소스가 일치하는 별도 검증 프로젝트에서 만들었다.

| 검사 | 결과 |
| --- | --- |
| 최종 관련 Edit Mode 회귀 | 최신 `main` 통합 후 538 / 538 통과, 건너뜀 0. 튜토리얼, 저장 이전, 거래 실패, 기존 콘텐츠, 운영 설정과 비동기 입장 검사를 포함한다. |
| 공통 UI 소유권 검사 | 검사 통과, 검사기 자체 테스트 9 / 9 통과 |
| macOS 개발 빌드 | 성공, 빌드 오류 0 |
| macOS 최종 런타임 | 별도 프로세스 복원부터 장착·보스·마을·첫 균열·칙령·새 스킬·훈련 후 재도전·강화 지원·무보상 다시보기까지 통과. 새 스킬은 실제 첫 균열의 성장으로 해금했으며 레벨 주입을 사용하지 않았다. |
| 화면 배치 | 도입·안내 일지·실습 지원 각각 20개 조합: 440×956, 956×440, 1600×900, 1600×1000, 2100×900 × 한국어·영어 × 글자 크기 100%·150% |
| Android/iOS 실기기 | 미검증. macOS의 합성 포인터 입력과 화면 크기 검사를 실기기 결과로 취급하지 않는다. |
| 사람의 4~6분 첫 플레이·이해도 | 미검증 |
| 원본 `main` 병합 | 2026-09-25, [PR #11](https://github.com/dakrdong/HELLSCRIPT/pull/11), 병합 커밋 `613e95689a8cb35c9a57503137d20606d5fe38fb`. 병합 결과의 게임 코드는 검증한 기능 커밋 `85206553191054b8bfe724fec009f076b21d1102`와 같다. |
| 공개 위키 배포 경로 | 병합된 `main`에서 문서·이력·DB·이미지·근거 전체를 읽기 전용으로 생성한다. [공개 위키](https://dakrdong.github.io/HELLSCRIPT-Wiki/#/page/tutorial-progression)는 Pages 성공과 비로그인 실제 화면 확인을 완료 조건으로 삼는다. |

### 전체 회귀 감사에서 구분한 실패

최신 운영 설정 통합 전 전체 검사는 3,923건 중 3,875건 통과, 48건 실패였다. 이 결과를 전체 통과로 보고하지 않는다.

- 튜토리얼에서 추가한 실패 4건을 수정했다. 잘못되거나 없는 사냥칙령은 연습 설정으로 인정하지 않되 기존 전투 시작을 막지 않는다. Unity 컴포넌트의 null 검사와 강조 테두리의 `CanvasRenderer` 선언도 수정했다. 해당 검사는 최종 538건에 포함된다.
- 전투 기록 JSON의 동일성 검사 43건은 초기 기준 코드와 최신 `main`의 독립된 복사본에서도 같은 테스트 이름으로 실패했다. `ActionContinuityTests` 36건, `CurrentBuildSaveTests` 4건, `RestoreFidelityTests` 3건이다. 이 작업에서 해당 전투 기록 코드는 수정하지 않았다.
- 던전 생성 검사 1건은 시드가 고정되지 않은 검사다. 기준 `main`의 6단계·시드 2에서 고블린 1마리가 추가되어 기대값 150 대신 151이 되는 것을 별도 재현했다. 고블린을 제외한 값은 150이다.

전체 감사와 기준 코드의 비교는 [회귀 비교](../../Artifacts/Validation/tutorial-progression/regression-comparison.json), [전체 검사 원본](../../Artifacts/Validation/tutorial-progression/editmode-full-audit.xml), [초기 기준 코드 검사](../../Artifacts/Validation/tutorial-progression/editmode-baseline.xml), [초기 고블린 재현](../../Artifacts/Validation/tutorial-progression/baseline-goblin.txt)에 남겼다. 최신 `main`에서도 같은 시드로 재현했다([결과](../../Artifacts/Validation/tutorial-progression/baseline-goblin-current.txt), [재현 코드](../../Artifacts/Validation/tutorial-progression/baseline-goblin-current-probe.cs.txt)). 최종 관련 검사 근거는 [통합 후 538건](../../Artifacts/Validation/tutorial-progression/editmode-integrated.xml)이다. 통합 전 [440건](../../Artifacts/Validation/tutorial-progression/editmode-focused.xml)과 [추가 66건](../../Artifacts/Validation/tutorial-progression/editmode-additional.xml)은 이전 검증 이력이며 최종 수에 더하지 않는다.

### 런타임 검증 경로

`RuntimeTutorialSmoke`는 개발 빌드에서 명시적인 `-hellscriptTutorialSmoke`, 임시 저장 경로, 증거 경로가 있을 때만 실행한다. UI 대상의 실제 레이캐스트를 확인한 후 포인터 입력으로 비교·장착·저장·실습 버튼을 누른다. 전투의 승리나 튜토리얼 완료 상태를 주입하지 않는다.

첫 프로세스는 신규 계정의 도입과 실제 일반 전투를 진행한 뒤 갑옷 지급 지점에서 종료한다. 두 번째 프로세스는 같은 임시 저장으로 `-hellscriptTutorialResume`을 추가해 시작한다. 갑옷 수량, 비교와 장착, 실제 보스 처치, 마을 도착, 첫 균열의 결과, 칙령·스킬 저장, 소유 상태 훈련, 동일 설정의 새 균열, 강화 지원과 다시보기를 확인한다. 고단계 콘텐츠의 화면 검사는 최고 클리어 120의 명시적인 검증용 상태를 사용하며 자연 성장 결과라고 보고하지 않는다.

2026-09-29: 커밋 `7dc11ff1`에서 직업별 첫 줄 액티브 세 종을 모두 1레벨에 열면서, 3레벨에는 패시브만 남았다. F06은 레벨 성장으로 열린 액티브, 즉 트리 레벨이 1보다 높은 액티브를 처음 장착하고 저장해야 완료된다. 3레벨 액티브를 찾던 Edit Mode 검사와 런타임 검사는 대상을 찾지 못해 실패했다. 이제 두 검사는 이 조건을 만족하는 액티브 가운데 가장 낮은 레벨의 스킬을 고르고, 캐릭터 레벨을 그 스킬의 레벨에 맞춘다. 현재 트리에서 선택되는 스킬은 10레벨의 지면 강타·후퇴 도약·순간이동이다. 처음 수정할 때는 새 캐릭터가 버전 3 구성이어서 등급을 배분하지 않고 장착했다. 같은 날 플레이어의 새 계정과 프롤로그 캐릭터가 버전 4의 0등급 상태로 시작하도록 바뀌었으므로, 런타임 검사는 장착하기 전에 포인트 1개를 배분한다. Edit Mode 검사의 캐릭터는 테스트가 직접 만든 계정이라 버전 3이며, 배분 없이 장착한다([스킬 트리 기록](Hunt_Edict_Skill_Tree.md)).

이번 실행에서 첫 균열은 1레벨 캐릭터를 4레벨까지 올렸다. 10레벨에 미달했으므로 레벨만 10으로 올리는 검증용 상태를 사용했으며, 이 결과를 자연 성장으로 보고하지 않는다. 수정 전 코드에서는 `TutorialProgressionTests` 30개 중 해당 검사 1개가 `Sequence contains no matching element`로 실패했고, 수정 후에는 30개가 모두 통과했다. macOS 개발 빌드에서는 첫 프로세스와 복원 이후의 전체 흐름이 통과했다. 전사가 지면 강타를 포인터로 장착하고 저장해 F06을 완료했다. 공통 UI 검사와 검사 도구 테스트 9개도 통과했다. 모바일 실기기에서는 확인하지 않았다. [검증 요약](TutorialNewSkillFixtureEvidence/validation.json) · [수정 후 검사](TutorialNewSkillFixtureEvidence/editmode.xml) · [수정 전 검사](TutorialNewSkillFixtureEvidence/editmode-baseline.xml) · [첫 프로세스](TutorialNewSkillFixtureEvidence/phase-one.txt) · [복원 이후 결과](TutorialNewSkillFixtureEvidence/runtime-tutorial-smoke.txt) · [새 스킬 장착 화면](TutorialNewSkillFixtureEvidence/new-skill-equipped.png)

이후 같은 날 F06의 기준이 첫 스킬 다음의 새 액티브로 바뀌었다(위의 「화면과 진행 정책」 참고). 지금 Edit Mode 검사와 런타임 검사는 10레벨 보정 대신 2레벨 이상에서 첫 줄의 다른 액티브를 장착한다.

[빌드 결과](../../Artifacts/Validation/tutorial-progression/build-result.txt)와 [빌드 소스 대조](../../Artifacts/Validation/tutorial-progression/source-parity.json)를 함께 보관한다. Unity가 자동 갱신한 아트 메타데이터, 머티리얼 버전, 에디터 설정과 기존 스킬 증거의 부동소수점 차이는 기능 변경에서 제외했다.

운영 설정 통합 시 저장 스키마 16과의 충돌을 피하도록 튜토리얼을 스키마 17로 분리했다. 스키마 16 계정의 진행 중인 균열과 입장 시 고정된 운영 설정을 보존한다. 다시보기를 시작하면 대기 중인 균열 입장을 취소한다. 일반 균열의 제한시간 안내는 고정 수치 대신 입장 당시 설정을 기준으로 설명한다. [최신 기준 코드 검사](../../Artifacts/Validation/tutorial-progression/editmode-baseline-current.xml)를 함께 보관한다.

최종 실행은 [정상 기본값의 로컬 게시 설정](../../Artifacts/Validation/tutorial-progression/loopback-liveops-release.json)을 HTTP로 받아 일반 균열 입장에 고정했다. 공개 서버나 전투 기록 업로드를 검증한 것은 아니다. [첫 프로세스 결과](../../Artifacts/Validation/tutorial-progression/phase-one.txt), [복원 이후 전체 결과](../../Artifacts/Validation/tutorial-progression/runtime-tutorial-smoke.txt), [저장 결과 요약](../../Artifacts/Validation/tutorial-progression/runtime-summary.json), [화면 파일 해시](../../Artifacts/Validation/tutorial-progression/screenshots-manifest.json)를 보관했다. 화면 60개 조합의 자동 배치 검사와 세로·가로·넓은 PC 화면의 대표 캡처 확인을 함께 수행했다.

![실제 소유 갑옷 비교와 장착](../../Artifacts/Validation/tutorial-progression/screenshots/actual-equipment-comparison.png)

![한국어 150% 세로 실습 지원](../../Artifacts/Validation/tutorial-progression/screenshots/support-440x956-ko-150.png)

![영어 150% 가로 실습 지원](../../Artifacts/Validation/tutorial-progression/screenshots/support-956x440-en-150.png)

### 화면 조합별 원본

| 화면 크기 | 언어 | 글자 크기 | 도입 | 안내 일지 | 실습 지원 |
| --- | --- | --- | --- | --- | --- |
| 440×956 | KO | 100% | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/intro-440x956-ko-100.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/journal-440x956-ko-100.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/support-440x956-ko-100.png) |
| 440×956 | KO | 150% | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/intro-440x956-ko-150.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/journal-440x956-ko-150.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/support-440x956-ko-150.png) |
| 440×956 | EN | 100% | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/intro-440x956-en-100.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/journal-440x956-en-100.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/support-440x956-en-100.png) |
| 440×956 | EN | 150% | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/intro-440x956-en-150.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/journal-440x956-en-150.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/support-440x956-en-150.png) |
| 956×440 | KO | 100% | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/intro-956x440-ko-100.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/journal-956x440-ko-100.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/support-956x440-ko-100.png) |
| 956×440 | KO | 150% | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/intro-956x440-ko-150.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/journal-956x440-ko-150.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/support-956x440-ko-150.png) |
| 956×440 | EN | 100% | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/intro-956x440-en-100.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/journal-956x440-en-100.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/support-956x440-en-100.png) |
| 956×440 | EN | 150% | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/intro-956x440-en-150.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/journal-956x440-en-150.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/support-956x440-en-150.png) |
| 1600×900 | KO | 100% | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/intro-1600x900-ko-100.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/journal-1600x900-ko-100.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/support-1600x900-ko-100.png) |
| 1600×900 | KO | 150% | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/intro-1600x900-ko-150.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/journal-1600x900-ko-150.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/support-1600x900-ko-150.png) |
| 1600×900 | EN | 100% | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/intro-1600x900-en-100.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/journal-1600x900-en-100.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/support-1600x900-en-100.png) |
| 1600×900 | EN | 150% | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/intro-1600x900-en-150.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/journal-1600x900-en-150.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/support-1600x900-en-150.png) |
| 1600×1000 | KO | 100% | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/intro-1600x1000-ko-100.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/journal-1600x1000-ko-100.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/support-1600x1000-ko-100.png) |
| 1600×1000 | KO | 150% | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/intro-1600x1000-ko-150.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/journal-1600x1000-ko-150.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/support-1600x1000-ko-150.png) |
| 1600×1000 | EN | 100% | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/intro-1600x1000-en-100.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/journal-1600x1000-en-100.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/support-1600x1000-en-100.png) |
| 1600×1000 | EN | 150% | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/intro-1600x1000-en-150.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/journal-1600x1000-en-150.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/support-1600x1000-en-150.png) |
| 2100×900 | KO | 100% | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/intro-2100x900-ko-100.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/journal-2100x900-ko-100.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/support-2100x900-ko-100.png) |
| 2100×900 | KO | 150% | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/intro-2100x900-ko-150.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/journal-2100x900-ko-150.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/support-2100x900-ko-150.png) |
| 2100×900 | EN | 100% | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/intro-2100x900-en-100.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/journal-2100x900-en-100.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/support-2100x900-en-100.png) |
| 2100×900 | EN | 150% | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/intro-2100x900-en-150.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/journal-2100x900-en-150.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/support-2100x900-en-150.png) |

추가 장면:

[갑옷 지급 지점](../../Artifacts/Validation/tutorial-progression/screenshots/armor-checkpoint.png) · [보스 처치](../../Artifacts/Validation/tutorial-progression/screenshots/real-boss-cleared.png) · [첫 균열 준비](../../Artifacts/Validation/tutorial-progression/screenshots/first-rift-preparation.png) · [칙령 저장](../../Artifacts/Validation/tutorial-progression/screenshots/edict-saved.png) · [새 스킬 장착](../../Artifacts/Validation/tutorial-progression/screenshots/new-skill-equipped.png) · [실습 지원 검토](../../Artifacts/Validation/tutorial-progression/screenshots/support-review.png) · [보상 없는 다시보기](../../Artifacts/Validation/tutorial-progression/screenshots/rewardless-replay.png)

2026-10-01: 룬 보드의 첫 해방은 일반적인 선택 안내와 별도로, 균열 15단계 결과창에서 인젤 미르가 진행하는 필수 보상 수령과 기존 5단계 플레이어블 가이드으로 연결했다. [룬 보드 해방 튜토리얼](Rune_Board_Unlock_Tutorial.md)을 따른다.
