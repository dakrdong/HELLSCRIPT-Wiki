# HELLSCRIPT UI 갱신 안정성 조사

확인일: 2026-10-03

## 결과와 근거 수준

`main` **14735105**에서 조사했다. 자동 저장은 `GameController.Update`의 3초 주기 `Save` → `GameStore.Committed("save")` → `StoreViewBinding.LateUpdate`로 전달된다. 기존 11개 binding 중 대장장이·룬 보드 2개를 제외한 9개는 표시 데이터가 같아도 컨트롤을 재생성했다. 상점 4가지 진입과 출석 2개 트랙을 펼치면 수정 대상은 13개 창/탭이다. 이 경로는 웹과 네이티브가 공유한다.

설정의 `UpdateCommonPanel → RefreshScreenSettings → UiTheme.Choice → UiButton.Configure`는 매 프레임 동일 역할·선택 상태를 지정했다. 기존 `UiButtonFace.Show`는 동일 상태에서도 전환 진행률을 다시 설정했다. 새 컨트롤의 최초 가용 상태가 첫 메시 생성 전에 바뀔 때에도 이전 색에서 시작할 수 있었다. 이 공통 경로를 멱등화하고 최초 메시 전에는 최종 팔레트를 바로 적용했다. 실제 hover/press/선택/비활성 상태 변화의 전환은 유지한다.

이는 **코드 경로와 EditMode 오브젝트·전환 상태로 확인한 재생성 원인**이다. 기존 웹 영상이나 수정 전후 실제 player의 픽셀 깜빡임·전체 CPU/GC 비용을 여기서 확인했다고 주장하지 않는다. 아래 전용 runtime 검사는 수정 코드 및 실제 14735105 코드 양쪽에서 컴파일했으나 아직 실행하지 않았다.

## 수정

- `StoreViewBinding`은 표시 데이터 키가 마지막으로 그린 키와 같으면 저장 알림을 소비하고 기존 컨트롤을 유지한다. 일반 `Save`로 실제 표시 데이터가 바뀌면 계속 갱신한다. 모든 `save`를 무시하는 필터를 추가하지 않았다.
- 갱신이 필요한 경우에도 마우스/터치/버튼 누름, 활성 입력 필드, 열린 dropdown, 비활성 배경 CanvasGroup에서는 보류한다. 여러 알림은 한 번으로 합친다. 인벤토리/창고 드래그 및 기존 modal 준비 조건도 유지한다.
- 실제 데이터 변경으로 교체한 컨트롤은 같은 이름 경로가 존재하고 사용 가능할 때 이전 키보드/포인터 포커스를 복구한다. 다른 창이 얻은 포커스는 가져오지 않는다. 이름에 `/`가 있어도 경로를 문자열 조각으로 탐색한다.
- 명시적 거래·탭·레이아웃·언어 repaint가 이미 최신 상태를 표시하면 `Rendered`로 기준 키를 기록하여 다음 LateUpdate의 중복 repaint를 없앤다. 기존 스크롤 복원 코드는 유지했다.
- 거래·보상·저장 구현, receipt, 승인·보호 검사는 변경하지 않았다. 표시 키는 거래 허가나 게임 상태 캐시로 사용하지 않는다.

## 페이지·탭·팝업 점검표

아래는 현재 진입 경로와 저장소에 남은 호환 경로를 함께 조사한 목록이다. 모든 행의 **정적 경로 조사**는 완료했고, **실제 화면 runtime는 미실행**이다. 자동 추출한 153개 UI 소스 파일·222개 생성/탭/대화상자 후보·11개 binding의 위치와 갱신/활성화/파괴 지점은 [UI_Refresh_Audit.json](UI_Refresh_Audit.json)에 있다. 후보 수는 고유한 사용 가능한 페이지 수가 아니며, 함수 선언·동적 제목·호환 코드도 포함한다. 실제 클릭 가능한 화면의 전수 runtime 합격표로 해석하지 않는다.

| 페이지/탭과 하위 팝업 | 실제 소유 코드 (`Runtime/Presentation/` 기준) | 주기/저장 갱신 판정 |
| --- | --- | --- |
| 타이틀; 로그인·Google 계정·진행 상황 선택·서버 선택 | `TitleScreenView`, `.Dialogs`, `GameUI.Title`, `.Accounts` | 크기 변화 reflow; 로그인 상태 polling; 이번 account 담당 수정과 별도 통합 필요 |
| 캐릭터 선택·복구 안내 | `GameUI.CharacterSelection`, `GameUI.cs` | 명시적 진입/상태 변경; 무조건 save repaint subscriber 없음 |
| 마을·마을 메뉴·NPC 대화·서비스 메뉴·콘텐츠 해금 | `GameUI.Plaza`, `.TownServices`, `.RiftKeeper`, `.ContentUnlocks`, `NpcDialogueWindow`, `StoryDialogueWindow` | HUD 0.15초/Revision, 서비스 활성화는 CPU 선행 작업 소유; 이번에는 수정하지 않음 |
| 전투·포탈/탈출 확인·균열 지도·적 행동·효과·HUD 상태 팝업 | `GameUI.cs`, `.BattleLayout`, `.BattleEscape`, `.Rift`, `.Enemies`, `.Effects`, `.GlobalHud`, `GlobalHudView` | HUD 수치·지도·전투 상태 갱신은 정상; 슬롯은 실제 데이터 존재 여부에 따라 active 변경 |
| 콘텐츠 dock·절전 안내·절전 요약·장비 상세 | `GameUI.ContentDock`, `.Idle`, `IdleEquipmentDetailWindow` | dock의 목표 변화 보간·절전 표시 정상; save 기반 전체 창 재생성 없음 |
| 인벤토리; 장비 상세·비교·능력치·일괄/선택 분해·물약 설정/선택·전체 재화·필터·범위 도움말 | `InventoryWindow*`, `GameUI.PlayInventory` | **수정**: 장비/빌드/보호 참조/슬롯 레벨·재화 키; 드래그/modal 보류, 재화 sheet 수치는 기존 방식으로 갱신 |
| 창고; 장비/보석/룬 보관, 5개 창고 탭·해금·확장·이름 변경·상세·분해/판매·프리셋·이용 안내·기록 | `StorageWindow*`, `GameUI.Storage` | **수정**: 보관/용량/이름/재화/장비·룬 상태 키; 드래그/modal 보류, 기존 각 panel 스크롤 복원 유지 |
| 장비 상점 구매·판매·되사기; 판매 확인·자동 선택 설정·보상 비교 | `EquipmentShopWindow*`, `GameUI.EquipmentShop` | **수정**: 장비/보호 참조·골드·상점 상태 키; 1초 stock 검사에서 실제 stock 교체는 유지 |
| 갬블 구매/판매/되사기·공개 연출·획득 비교 | 같은 상점 adapter의 `Gamble` 경로, `.Gamble` | **수정**: 동일 키; 정상 공개 연출 및 거래 후 repaint 유지 |
| 대장장이 옵션 변경·슬롯 강화·장비 강화·코어 제작; 자동 재련/완료·장비 선택·옵션 고정·안내·코어 결과 | `BlacksmithWindow*` | 기존 `StoreViewSignature`와 작업 ID 변화 guard 확인; 0.25초 timer 수치는 제자리 갱신; 정상 작업 완료/공지/재련 repaint 유지 |
| 보석상 보석 보관함·물약 제조·소켓 관리 진입 | `JewelerWindow`, `GameUI.Jeweler` | **수정**: 골드/보석/용량/물약/거래 가능·해금 키 |
| 위상 각인석 장비 선택·도감·확인·검색·필터·도움말·장비 picker | `AspectStoneWindow`, `GameUI.AspectStone` | **수정**: 장비/위상/해금·중단된 균열 유무 키; 검색 입력 보류 |
| 이벤트 7일·28일 출석·일일 퀘스트 진입; 페이지 swipe·하루 숨김 | `AttendanceWindow`, `AttendanceSwipe`, `GameUI.Attendance` | **수정**: 출석/날짜/수령 가능 퀘스트·캐릭터/가방 여유 키; 정상 claim glow와 실제 자정 갱신 유지 |
| 일일 퀘스트 5개·보상 수령·새로고침 | `DailyQuestWindow` | **수정**: 퀘스트 상태 키; 1초 countdown은 텍스트만 변경, 자정 시 하루 한 번 갱신 유지 |
| 보상 상자 전체/장비/보석/재화/룬/물약·페이지·선택; 획득 내역·장착 비교·최근 개봉 | `RewardBoxesWindow` | **수정**: 보유 상자/receipt·중단된 균열 유무만 키에 포함; 관계없는 장비/골드 변화는 제외 |
| 단계별 최초 보상·상자 수령·상자 보관함 진입 | `RewardBoxesWindow.ShowFirstClear` | **수정**: 실제 단계 상태/해금/보상 preview·중단된 균열 유무 키; 거래 시 기존 domain 재검사 유지 |
| 미접속 보급·보상 공개 연출 | `OfflineSuppliesWindow`, `RiftRewardRevealWindow` | 고정 보상 snapshot; 공개/등장 애니메이션 정상 |
| 균열 입장·단계 선택·1.5배 확인·피로 회복·물약/스킬/궁극기 교체·최초 보상 | `RiftEntryWindow*`, `GameUI.RiftKeeper` | 기존 DisplayKey 및 popup/CanvasGroup guard; 1초 날짜/피로 timer 제자리 갱신 |
| 균열 결과·획득 아이템·아이템 정보/비교·반복 설정·재도전 countdown | `RiftVictoryWindow*`, `GameUI.RiftVictory`, `.Repeat` | 기존 save 제외; rewardStatus/repeat 실제 변화 및 popup guard; 정상 countdown 유지 |
| 훈련장 캐릭터/스킬/적 목록·적 추가 picker·실전 스킬 편집 | `TrainingGroundWindow*`, `GameUI.TrainingGround` | 기존 save 제외; dropdown 변경의 다음 프레임 redraw 유지; 실제 non-save 변경 및 중첩 상호작용 runtime 미확인 |
| 훈련 결과·이전 결과/DPS·스킬/칙령 상세 | `TrainingGroundResultWindow*` | 기존 save 제외; snapshot 중심, 실제 non-save repaint runtime 미확인 |
| 사냥 칙령 요약·스킬·전투·생존·전리품·가방/정리·탐색·반복·추천 착용·프리셋/공유 | `HuntEdictWindow*`, `GameUI.HuntEdict` | draft 소유; 무조건 save subscription 없음; 크기/언어 reflow는 입력/스크롤 복원 |
| 칙령 옵션 숫자/집합/순서·스킬 행동/정책·프리셋 이름/가져오기·저장/되돌리기/닫기 확인 | `HuntEdictWindow.Summary`, `.Skills`, `.Presets`, `GameUI.Presets` | 명시적 사용자의 draft 변경/창 크기 변화; 정상 미리보기 애니메이션 유지 |
| 룬 보드·무기/효과/보유 필터·정보/도움말/범례/도감·프리셋/저장·영역/영역 정보/잠금/칸·실습·닫기/되돌리기 | `RuneBoardWindow*`, `GameUI.RuneBoard` | 기존 rune revision guard, Dirty draft reload 금지; 드래그 중 갱신 보류; 정상 카메라 overlay 갱신 |
| 룬 장인 변형·업그레이드·재료 선택·드래그·합성/업그레이드 결과 공개 | `RuneMasterWindow*`, `GameUI.RuneMaster` | 실제 rune revision만 repaint; 합성·공개 연출 정상 |
| 전투 기록·전투 로그·DPS/기여도 그래프·사망 분석·구간/틱/규칙/당시 칙령 | `CombatRecordsWindow`, `CombatLogWindow`, `RiftCombatGraphWindow`, `GameUI.History`, `.CombatJournal` | 고정 기록/snapshot; explicit 페이지/필터 변경, 무조건 save repaint 없음 |
| A/B 훈련 비교 조건·B 장비/원본·결과/기록·변경 조건·설정 활용·프리셋 저장 | `GameUI.Comparison`, `.TrainingEquipment`, `.Presets` | 비교 snapshot/draft; 화면 크기별 reflow guard; 거래 코드 변경 없음 |
| 성장·능력치·추천 행동·최초 실습 A/B·튜토리얼 일지/지원/완료 | `GameUI.Growth`, `.Attributes`, `.FirstPlay`, `.TutorialPractice`, `TutorialJournalWindow`, `RiftRecommendationWindow` | explicit action/snapshot/실습 진행; cue 연출 정상 |
| 설정 화면·사운드·전투·게임 안내; 비율/언어/캐릭터·음량·표시 옵션 | `GameUI.ScreenSettings`, `.Audio`, `.SettingsFrame`, `SettingsControls` | **공통 버튼 수정**: 같은 Configure/Show는 전환 재시작 안 함; 설정 수치 refresh와 safe-area reflow는 유지 |
| 퍼즐 허브·정비 P1/P2·정찰/복기/힌트 | `PuzzleHubWindow`, `PuzzleMaintenanceWindow`, `GameUI.PuzzleHub`, `.Puzzle` | 실제 영웅/진행/장비/예산 표시 키와 공통 입력 보류; 정찰·복기는 고정 run snapshot. [9단계](Puzzle_Tutorial_Final_Acceptance.md)에서 3직업 KO/EN·세로/가로 12여정, 실제 저장 실패·재시도·누름 중 저장·재진입 통과; 물리 기기 미검증 |
| 남은 호환 Base 페이지: 행동 설계·행동/조건 picker·판단 기록·장비 제작/도감·보석 메뉴/상세/소켓·legacy bag/warehouse·룬 합성/확률/연습·칙령 공유 | `GameUI.cs`, `.Rules`, `.Items`, `.Gems`, `.InventoryLayout`, `.Runes`, `.EdictEditor`, `.EdictShare` | 자동 생성 index에 유지; 현재 wrapper가 새 창으로 우회하는 경로와 구분. 이벤트 없는 명시적 repaint; 실제 reachability는 runtime에서 추가 확인 |

## 완료한 검증

- Unity **6000.6.0f1**의 격리 소형 프로젝트에서 `Hellscript.Tests.StoreViewBindingTests` **12/12 통과**, 실패/건너뜀 0, fixture 실행 0.804초. 원본 사용자 Editor·앱·worktree를 건드리지 않았다.
- 무변경 Save / 관계없는 transaction의 동일 Button 유지, 실제 gold 변경 일반 Save 반영 및 디스크 reload, 누름 중 보류 후 클릭 1회, ready=false 동안 여러 저장 합치기, child CanvasGroup 보류와 닫힘 후 갱신, 입력 중 unsaved text 보존, 수동 repaint 중복 방지, 키보드/포인터 focus 복원, 실패 저장·중복 receipt의 상태/보상 보존, equipment key의 리스트/보호 참조 민감도, 동일 버튼 구성의 progress 유지 검증.
- 전체 Runtime C#을 기존 Unity 6000.6 설치·package 참조로 Roslyn 컴파일: 오류 0. 기존 obsolete API 경고 존재. Editor import·IL2CPP·native/WebGL 빌드 성공과 동등한 검사는 아니다.
- `tools/check_ui_contract.py` 통과, `tools/test_ui_contract.py` 11/11 통과, `tools/check_ui_refresh.py` 11개 binding guard 통과, `git diff --check` 통과.
- [집중 결과 XML](UIRefreshEvidence20261003/focused-editmode.xml)과 [검증 범위·소스 hash](UIRefreshEvidence20261003/validation-scope.json)는 변경에 포함한다. 상세 로그는 전달 자료의 `evidence/`에 둔다. 이 문서의 검증은 공통 회귀 범위이며 실제 13개 창·전체 게임 화면을 실행했다고 해석하지 않는다.

## 실제 화면과 비용 검증 인계

`RuntimeUiRefreshSmoke`는 development native player에서만 명시적 옵션으로 실행한다. 사용자 저장 대신 새 폴더를 쓰며 synthetic fixture 계정을 만든다. opt-in 검사 파일은 구 코드에서 없는 새 API를 반사로 선택적으로 호출하므로 **14735105에 이 파일만 추가한 실제 기준 player**에서도 `BaselineProbe`가 실행 가능하다.

```sh
PLAYER -hellscriptSavePath ISOLATED_SAVE -hellscriptScreenshots EVIDENCE -hellscriptUiRefreshBaselineProbe -hellscriptUiRefreshSource 14735105 -screen-width 440 -screen-height 956
PLAYER -hellscriptSavePath OTHER_ISOLATED_SAVE -hellscriptScreenshots AFTER_EVIDENCE -hellscriptUiRefreshSmoke -hellscriptUiRefreshSource INTEGRATED_COMMIT -screen-width 440 -screen-height 956
```

구 코드/CPU-only/수정 후 각각의 실제 source commit·build GUID·동일 해상도/언어·저장 fixture를 기록한다. 구 코드 경로는 안정성 합격을 주장하지 않고 실제 교체 횟수를 관측한다. 수정 경로는 13개 창/탭 × KO/EN에 대해 실제 controller 3초 자동저장 대기, 3회 × 무변경 저장 5회, 버튼 유지, 실제 표시 값 변경, 누름 중 보류/해제 후 반영, child 창 닫힘 후 반영, 닫기/재진입을 assert하고 screenshot 및 JSON을 남긴다.

JSON 비용은 **동기 Save + binding flush + Canvas update 구간의 wall time, 현재 thread GC bytes, 전체 process CPU time**이다. screenshot·frame wait는 밖에 둔다. process CPU에는 다른 thread가 포함된다. 이것을 전체 frame CPU/GC나 사용자 브라우저 비용으로 확대하지 않는다. 선택 옵션 `-hellscriptUiRefreshLegacy`는 수정 binary에서 key만 끄는 합성 대조군이며 실제 이전 binary 비용을 대신하지 않는다. 원자료 없는 추정 개선율은 보고하지 않는다.

남은 검증: 440×956, 956×440, 1440×810, 1440×900, 1680×720의 KO/EN 실제 픽셀·hover/누름/포커스·스크롤 유지, 반복 탭 이동/빠른 클릭/닫기 재진입/외부 포커스 복귀, 실제 거래·보상·저장 실패, 설정·대장장이·룬·입장/결과/훈련/튜토리얼의 전체 흐름, **실제 웹 브라우저 대기 중 갱신과 native 비교**, matched 3회 이상 실제 player CPU/GC 측정, 실기기 터치. 자동 native 검사는 마우스 handler 호출을 포함하므로 실제 OS 입력/웹 DOM 클릭 검증도 별도로 필요하다. 최종 전체 EditMode·native·web·위키 생성은 통합 담당이 마지막 병합 후 순차 실행한다. 이 sparse 소스 체크아웃의 전체 위키 build는 기존 아트 첨부 `Docs/Art/ClassAspectIcons/manifest.json` 누락 링크에서 중단됐다. 새 문서 4개 렌더 및 이력은 보존했고, 전체 위키 check/test_wiki/test_wiki_ui는 미실행이다.

## 통합 기반과 경계

`codex/ui-refresh-stability`는 `14735105` 기반이다. CPU branch `codex/cpu-ponytail-lite`의 실제 구현 **91aceb5f**, 근거 **2873aa41**, 최신 지침 **ff300129**와 병합 검증해야 한다. 91aceb5f의 수정 파일과 이번 Runtime 수정 파일은 중첩하지 않는다. HUD 중복 갱신·servicebutton false→true·가려진 world rendering의 CPU 수정은 이번 변경에 복사하지 않았다.

14735105와 ff300129 전체 tree 비교에서는 `AttendanceWindow`, `ContentWindowView`, `EquipmentShopWindow`, `UiButton` 파일이 다르다. 이는 공통 조상 `94e11019` 이후 main에 반영된 다른 UI 작업도 포함한 차이다. CPU branch의 파일을 통째로 덮어써서 과거 레이아웃/출석/가용 클릭 정책으로 되돌리지 않는다. account 86e4dd87 및 활성 출석 등 다른 작업도 최종 main에서 합쳐 검사한다. 이 브랜치에서는 main 반영·push·공개 배포를 하지 않는다.

새 원인에 대한 지침은 [UI 갱신 재발 방지](UI_Refresh_Stability_Rules.md)에 있다. 기존 CPU 규칙을 중복 추가하지 않는다.


## 사냥 칙령 단계 공개, 2026-10-04

HuntEdictWindow는 영웅·레벨·공개 ID·물약 revision·칙령/스킬/프리셋·안내 단계를 표시 키로 사용하는 StoreViewBinding을 붙인다. 대화상자와 순서 드래그 동안 갱신을 보류하고 명시적 repaint 후 기준을 기록한다. 편집본·포커스·스크롤은 기존 소유자를 따른다. 선택형 공개 카드는 StoryDialogueWindow를 재사용한다. [구현과 해당 runtime 검증 범위](Hunt_Edict_Progression.md)를 참고한다. 이 행은 이번에 바뀐 편집기와 안내 경로의 범위이며 기존 전체 UI 후보의 시각 합격표가 아니다.
