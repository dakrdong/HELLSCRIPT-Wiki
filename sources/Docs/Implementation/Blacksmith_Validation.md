# 대장간 구현 검증 기록

갱신일: 2026-10-01 · 작성일: 2026-09-22

[English](Blacksmith_Validation.en.md) · [구현·리소스·이전 명세](Blacksmith_Unity_Integration.md)

## 2026-10-01 버튼과 배치 수정

자동 저장은 3초마다 저장 성공 알림을 보냈고, 대장간은 표시 상태가 같아도 버튼을 다시 만들었다. 새 버튼이 활성 색에서 비활성 색으로 전환하면서 깜빡였으며, 누르는 중 버튼이 교체될 수도 있었다. 표시 상태가 바뀐 저장만 다시 그리도록 하고 새 버튼에는 첫 프레임부터 최종 활성·잠금 색을 적용했다.

첨부 화면의 45 G는 Lv.1 옵션 변경 비용 500 G보다 적었다. 골드 부족으로 두 버튼이 설명 없이 비활성화되어 있었다. 이제 옵션 변경은 부족 금액을 안내하고, 자동 변경은 설정을 열어 필요·보유·부족 금액을 표시한다. 부족할 때 유료 실행은 차단한다. 슬롯 강화 버튼에도 강화석 비용·보유·부족 수량을 표시했다. 장비 강화 비교 상자는 수치에 맞는 높이로 줄였고, 장비 제작 안내 탭을 삭제해 서비스는 네 개다.

| 검사 | 결과 |
| --- | --- |
| 집중 Edit Mode | `BlacksmithTests`, `ForgePresentationAllocationTests`, `UiButtonTests`, `LocalizationTests`: **107개 통과, 실패·건너뜀 0개**, 9.29초. [종료 결과와 개별 검사](BlacksmithFix20261001Evidence/editmode.json) |
| 전체 대장간 실행 검사 | 한 번 통과. +10·최대 강화의 실제 견적·차감·저장, 옵션 고정·자동 중단/계속/멈춤, 슬롯 작업·개방·무료 완료·정산, NPC 버튼·E키, KO/EN 다섯 비율, 재시작 복구. [결과](BlacksmithFix20261001Evidence/runtime.txt) |
| 부족 안내 추가 확인 | 이전 알림과 자동 설정 안내가 겹치는 부분을 정리한 뒤 해당 범위만 확인. 45 G 계정에서 무차감, 자동 설정 열림·유료 시작 차단, 강화석 견적, 초기 비활성 색, 자동 저장과 누름 중 같은 버튼 유지. KO/EN 다섯 비율에서 문구 잘림·중복 안내 없음. [결과](BlacksmithFix20261001Evidence/feedback.txt) |
| 네 탭 탐색 | 최종 실행본에서 한 번 통과. 네 잠금 조건·2초 안내·키보드/포인터·뒤로·선택 보존, 네 해금 탭 이동, 삭제 탭 부재, KO/EN 다섯 비율. [결과](BlacksmithFix20261001Evidence/navigation.txt) |
| 계약·컴파일·빌드 | UI 계약 통과, 계약 검사 11개 통과. 최종 macOS 개발 빌드 성공: 오류 0개, 경고 176개. [빌드 결과](BlacksmithFix20261001Evidence/build.json) |

기본 글자 크기에서 440×956, 956×440, 1600×900(16:9), 1440×900(16:10), 1680×720(21:9)을 확인했다. [축소한 장비 비교](BlacksmithFix20261001Evidence/gear-pc-ko.png) · [세로 장비 강화](BlacksmithFix20261001Evidence/gear-portrait-ko.png) · [자동 변경 부족 안내](BlacksmithFix20261001Evidence/auto-pc-ko.png) · [영어 세로 안내](BlacksmithFix20261001Evidence/auto-portrait-en.png) · [슬롯 비용](BlacksmithFix20261001Evidence/slot-pc-ko.png) · [가로 슬롯 비용](BlacksmithFix20261001Evidence/slot-landscape-ko.png) · [네 탭](BlacksmithFix20261001Evidence/four-tabs-ko.png). [근거·소스 해시](BlacksmithFix20261001Evidence/manifest.json).

추가 확인용 증분 빌드 한 번은 URP 셰이더 리소스 누락으로 UI 초기화 전에 실패했다. 전체 에셋 새로고침 후 새 출력 경로에 클린 빌드하여 부족 안내와 탐색 검사를 통과시켰다. 통과한 거래·저장 검사와 107개 검사는 그대로 활용했으며 전체 게임 회귀 검사, 다른 콘텐츠의 스모크, 확대 글자 검사는 다시 실행하지 않았다. 기존 후처리 셰이더 제외 로그와 Editor AI 구독 오류는 이번 UI 수정의 검증 범위가 아니다. 위 결과는 격리 저장 경로의 macOS 개발 플레이어와 합성 Unity 입력이며 모바일 실기기 검증은 하지 않았다.

## 2026-09-30: HTML 시안 배치 적용

[적용 기록](Blacksmith_Unity_Integration.md#2026-09-30-html-시안-배치로-재구성)의 창틀·탭·잠금·다섯 서비스 화면을 macOS 개발 플레이어에서 조작해 확인했다. 아래는 마지막 코드 변경 뒤 한 번 실행한 결과다.

| 검사 | 결과 |
| --- | --- |
| 전체 대장간 스모크 `-hellscriptBlacksmithSmoke` | 통과. 장비 강화 +10·최대, 옵션 변경(첫 변경 확인 창, 자동 변경 일시정지·계속·멈춤), 슬롯 강화 두 작업·확인 창을 거친 작업 칸 개방·59초 무료 완료·정산·툴팁, NPC 반경·대화·E키, 세로 440×956 / 가로 956×440 / PC 16:9·16:10·21:9의 한국어·영어, 재시작 뒤 저장 상태 |
| 탐색 스모크 `-hellscriptForgeNavigationSmoke` | 통과. 잠긴 5개 서비스의 정확한 조건 문구, 화면 전환·거래 없음, **2초** 뒤 말풍선 소멸, 같은 버튼 크기·역할, 5개 화면 크기 × 두 언어 |
| 코어 제작 스모크 `-hellscriptCoreCraftingSmoke` | 통과. 7개 창 모양 × 두 언어, 필터·상세·+/- 버튼·80% 한도·제작·확인·기록, 재시작 뒤 보류 결과 |
| 접근 스모크 `-hellscriptBlacksmithAccessSmoke` | 통과. 신규 캐릭터, 저장된 균열 중 이용, 세로 장비 시트로 고른 장비의 실제 강화 거래와 저장 |
| 집중 Edit Mode | Localization · Blacksmith · CoreCrafting · ContentUnlock · StoredLocalization 165개 중 165개 통과 |
| 전체 Edit Mode | 4,878개 중 4,830개 통과, 48개 실패. 실패 47개는 2026-09-26 기준선 목록과 정확히 같은 기존 실패(이번 변경과 무관)이고, 나머지 1개는 코어 제작 개방을 50단계로 바꾸며 기대값이 옛 120이던 `RiftContentUnlockTests` 한 건이라 테스트를 50 기준으로 고친 뒤 그 클래스와 `ContentUnlockTests` 62개 중 62개가 통과했다. 나머지는 다시 돌리지 않았다 |
| 문구·계약 검사 | 영어 누락·고아 항목 0(`l10n_check` 재현과 Unity 검사), `check_ui_contract.py` 통과, `test_ui_contract.py` 11개 통과 |

`main`(4bd93ff3)에서 같은 스모크를 돌리면 대장간 전체 스모크는 마을 준비 단계에서 바로 실패했고(튜토리얼 지도), NPC 대화·균열 개방 스모크도 같은 이유로 실패한다. 이번에 전체 대장간 스모크의 고정 조건(튜토리얼 완료, 출석 팝업 숨김, 대화창 경유 진입)을 고쳐 끝까지 통과시켰다. NPC 대화(`-hellscriptNpcDialogueSmoke`)와 균열 개방(`-hellscriptRiftUnlockSmoke`) 스모크는 `main`과 같은 위치에서 여전히 실패하며 이번 변경 범위가 아니다. 공통 UI 스모크(`-hellscriptSharedUiSmoke`)는 대장간에 이르기 전 초반 화면(`RuntimeSharedUiSmoke.cs` 85행)에서 멈춰 대장간 검사를 실행하지 못했다. 이 스모크의 대장간 부분(칸 크기·열 너비)은 새 배치에 맞게 고쳤지만 실행으로 확인하지 못했다.

근거 화면: [가로 옵션 변경](BlacksmithLayoutEvidence/landscape-ko-tab0.png) · [슬롯 강화](BlacksmithLayoutEvidence/landscape-ko-tab1.png) · [장비 강화](BlacksmithLayoutEvidence/landscape-ko-tab2.png) · [코어 제작](BlacksmithLayoutEvidence/landscape-ko-tab3.png) · [세로 옵션 변경](BlacksmithLayoutEvidence/portrait-ko-tab0.png) · [세로 슬롯 강화](BlacksmithLayoutEvidence/portrait-ko-tab1.png) · [세로 장비 강화](BlacksmithLayoutEvidence/portrait-ko-tab2.png) · [세로 코어 제작](BlacksmithLayoutEvidence/portrait-ko-tab3.png) · [PC 옵션 변경](BlacksmithLayoutEvidence/pc-ko-tab0.png) · [PC 슬롯 강화](BlacksmithLayoutEvidence/pc-ko-tab1.png) · [PC 장비 강화](BlacksmithLayoutEvidence/pc-ko-tab2.png) · [PC 코어 제작](BlacksmithLayoutEvidence/pc-ko-tab3.png) · [영어 가로](BlacksmithLayoutEvidence/landscape-en-tab0.png) · [영어 세로](BlacksmithLayoutEvidence/portrait-en-tab0.png). 잠금: [가로 말풍선](BlacksmithLayoutEvidence/locked-landscape-hint.png) · [세로 말풍선](BlacksmithLayoutEvidence/locked-portrait-hint.png) · [전부 잠긴 PC](BlacksmithLayoutEvidence/locked-all-pc.png). 팝업: [장비 시트](BlacksmithLayoutEvidence/portrait-ko-sheet.png) · [위치 고정 확인](BlacksmithLayoutEvidence/landscape-ko-confirm.png) · [자동 변경](BlacksmithLayoutEvidence/landscape-ko-auto.png) · [작업 칸 시트](BlacksmithLayoutEvidence/portrait-ko-slots-jobs.png) · [성장 계획](BlacksmithLayoutEvidence/landscape-ko-slots-plan.png) · [코어 품질 설정](BlacksmithLayoutEvidence/portrait-ko-cores-chosen-coins.png) · [제작 결과](BlacksmithLayoutEvidence/landscape-ko-cores-result.png). 실행 기록: [전체 스모크](BlacksmithLayoutEvidence/runtime-smoke.txt) · [탐색](BlacksmithLayoutEvidence/runtime-nav.txt) · [코어 제작](BlacksmithLayoutEvidence/runtime-core.txt) · [접근](BlacksmithLayoutEvidence/runtime-access.txt).

모바일 실기기의 터치·회전·성능, Android/iOS 빌드는 확인하지 못했다. 위 결과는 macOS 플레이어와 합성 입력이다.

## 2026-09-29: 슬롯 스냅샷이 없는 이전 균열

2026-09-29에 macOS 개발 빌드(`main` `b1989282`)에서 마을 가방을 열자 `HeroStats.ApplySlotGrowth`(`HeroStats.Blacksmith.cs:46`)가 `IndexOutOfRangeException`을 던졌다. 호출 경로는 `GameUI.ShowPlayInventory` → `InventoryWindow.EquipmentStats` → `EquipmentPreviewHero`였다. 해당 로컬 게스트 저장본에는 영웅 0의 중단된 균열이 있었고, 이 균열의 `slotLevels`는 `null`이 아니라 빈 배열이었다.

진행 중인 균열은 시작할 때의 장착 슬롯 강화 단계를 `RunState.slotLevels`에 스냅샷으로 저장한다. 이 필드가 생기기 전에 저장된 균열에는 필드 자체가 없는데, `JsonUtility`는 없는 배열과 `null` 배열을 모두 빈 배열로 읽는다. 기존 코드는 `null`만 스냅샷이 없는 상태로 처리했다. 그래서 빈 배열을 그대로 복사했고, 10개 부위를 모두 읽는 `ApplySlotGrowth`에서 예외가 발생했다. 같은 원인으로 마을에서 장비를 장착·해제하는 거래(`GameStore.ChangeEquipment`), 균열 재개(`CombatSimulation` 생성자), 마을에서 사냥 칙령을 저장하는 작업(`CombatSimulation.PrepareSuspendedHuntEdictChange`)도 실패했다. 캐릭터 전환과 서버 판정기의 전투 재개도 같은 생성자를 거친다.

- 판정 기준을 `BlacksmithCatalog.SlotLevels(run, hero)` 한 곳에 두었다. 균열에 저장된 배열의 길이가 대장간 부위 수(10)와 같을 때만 그 스냅샷을 사용한다. 배열이 없거나 비었거나 길이가 다르면 캐릭터가 현재 가진 강화 단계를 사용한다.
- `slotLevels`를 읽고 없으면 캐릭터의 값으로 대신하던 네 곳이 모두 이 함수를 거친다. 가방 미리보기, 마을 장비 교체, 전투 생성·재개, 마을 칙령 편집이다. 나머지 코드는 전투 생성자가 확정한 `State.slotLevels`만 읽으므로, 진행 중인 전투가 시작 시점의 스냅샷을 유지하는 기존 규칙은 그대로다.
- 저장 형식과 키는 바꾸지 않았고, 이전 저장본도 그대로 불러온다. 스냅샷이 없는 균열을 재개하면 그 시점에 캐릭터가 가진 강화 단계를 스냅샷으로 저장한다.
- 화면 배치와 문구는 바꾸지 않았다.

### 스냅샷 누락 검증 결과

- 재현: `BlacksmithTests`에 검사 4개를 추가했다. 빈 배열과 길이 3 배열로 저장한 균열을 다시 불러온 뒤 가방을 실제로 열어 최대 HP를 확인하고, 마을 장비 장착·균열 재개·마을 칙령 저장을 확인한다. 수정 전 코드에서는 4개가 모두 실패했고, 가방 검사는 보고와 같은 `ApplySlotGrowth` ← `HeroStats` ← `InventoryWindow.EquipmentStats` 경로에서 멈췄다. [수정 전 XML](LegacySlotSnapshotEvidence/editmode-before.xml)
- 호출 지점별 보호: 판정 함수는 남기고 네 호출 지점을 하나씩 수정 전 코드로 되돌렸다. 어느 지점을 되돌려도 그 지점을 지나는 검사가 실패했다. [결과](LegacySlotSnapshotEvidence/mutation.txt)
- 관련 Edit Mode 검사 381개(대장간, 중단된 균열의 칙령 저장, 인벤토리 3종, 균열 입장, 서버 판정, 설정 개정, 복원, 현재 빌드 저장, 반복 사냥, 전투 기록, 저장 문구 번역, 성장, 행동 연속성): 수정 후 338개가 통과하고 43개가 실패했다. 실패한 43개는 수정 전 코드에서도 같은 이름으로 실패하며, 2026-09-27에 기록된 기준 실패 목록에도 모두 들어 있다. 수정 전 코드에서는 여기에 새 검사 4개가 더해져 47개가 실패했다. [수정 후](LegacySlotSnapshotEvidence/editmode-focused.xml) · [수정 전](LegacySlotSnapshotEvidence/editmode-focused-base.xml)
- 전체 Edit Mode: 4,734개 중 4,686개가 통과하고 48개가 실패했으며, 건너뛴 검사는 없었다(1,853초). 새 검사 4개는 모두 통과했다. 실패한 48개 중 47개는 2026-09-27에 기록된 기준 실패 목록과 같다. 나머지 1개인 `TutorialProgressionTests.EdictAndNewSkillRequireCommittedChangeThenMatchingTrainingAndFreshRift`는 런타임 파일을 수정 전으로 되돌린 상태에서도 같은 메시지로 실패한다. [요약](LegacySlotSnapshotEvidence/editmode-full-summary.json) · [기준 코드 검사](LegacySlotSnapshotEvidence/editmode-tutorial-base.xml)
- macOS 개발 빌드 오류 0개. 새 스모크 `-hellscriptLegacyRunBagSmoke`는 저장본에 스냅샷이 없는 균열이 있으면 그 균열을 그대로 쓰고, 없으면 새 계정에 같은 상태를 만든다. 마을 가방을 440×956 한국어, 956×440 영어, 1600×900 한국어로 열고, 최대 HP가 캐릭터 자신의 강화 단계로 계산한 값과 같은지와 전체 능력치가 모두 표시되는지 확인한다. 이어서 가방에서 장비 하나를 장착하고 균열을 재개한 뒤, 전투와 저장 파일이 캐릭터의 강화 단계를 사용하는지 확인한다. [빌드](LegacySlotSnapshotEvidence/build.txt)
  - 보고된 저장본의 복사본: 통과했다. 최대 HP 621을 표시했고 장착을 저장했으며, 재개한 균열은 처치 38·HP 273/621 상태로 이어졌다. 원본 저장 파일은 수정하지 않았다. [실행 결과](LegacySlotSnapshotEvidence/runtime-user-save.txt)
  - 가슴 부위를 40단계로 만든 새 계정: 통과했다. 최대 HP 2,749는 기본값인 1단계가 아니라 40단계를 반영한 값이다. [실행 결과](LegacySlotSnapshotEvidence/runtime-fixture.txt)
  - 런타임 파일 5개만 수정 전으로 되돌린 빌드에 같은 복사본을 넣으면, 가방을 여는 순간 보고와 같은 예외가 발생해 종료 코드 1로 끝났다. [실행 결과](LegacySlotSnapshotEvidence/runtime-before.txt)
- 화면 배치를 바꾸지 않았기 때문에 글자 크기 변경, PC 16:10·21:9와 모바일 실기기는 이번 작업에서 확인하지 않았다. 이 세션에서는 `mcpforunity://instances` 리소스가 보이지 않아 Unity MCP를 사용하지 못했다. 검사는 프로젝트 복제본의 배치 모드와 macOS 실행본에서 수행했다.

[저장본 가방 1600×900](LegacySlotSnapshotEvidence/bag-save-1600x900-ko.png) · [세로 440×956](LegacySlotSnapshotEvidence/bag-save-440x956-ko.png) · [영어 956×440](LegacySlotSnapshotEvidence/bag-save-956x440-en.png) · [새 계정 가방](LegacySlotSnapshotEvidence/bag-fixture-1600x900-ko.png) · [재개한 균열](LegacySlotSnapshotEvidence/resumed-save.png)

## 2026-09-29: 잠긴 기능 안내와 탐색 버튼 통일

대장간의 옵션 변경·장착 슬롯 강화·장비 강화·코어 제작·장비 제작은 같은 `UiButtonRole.Tab`과 공통 테마를 사용한다. 가로에서는 다섯 버튼의 너비와 높이가 같으며, 세로에서는 같은 크기의 버튼을 3개·2개씩 두 줄에 배치한다. 기존 세로 하단의 작은 제작 버튼은 제거했다. 장비 제작은 해금 후 기존 제작 화면으로 이동한다.

- `ContentUnlocks.Has`와 `ContentUnlocks.Condition`을 그대로 사용한다. 해금 단계나 이전 계정의 개방 기록은 바꾸지 않는다. 장비 제작 버튼은 `RareCraft`의 해금 조건을 따른다.
- 잠긴 버튼은 어두운 면과 황동 자물쇠로 표시한다. 안내를 볼 수 있도록 클릭·키보드 제출은 유지하고, 화면 전환과 거래만 차단한다.
- 누른 버튼 아래에 실제 해제 조건을 3초간 표시한다. 말풍선은 안전 영역 안으로 이동하고 입력을 가로채지 않는다. 다시 누르면 표시 시간을 갱신하며, Esc는 말풍선을 먼저 닫는다.
- 현재 장비·옵션 선택과 스크롤은 잠긴 버튼을 눌러도 유지한다. 화면 방향·언어·글자 크기가 바뀌면 말풍선을 새 버튼 위치에 맞춘다. 해금 뒤에는 잠금 표시가 사라지고 기존 기능으로 진입한다.
- 처음 방문해 모든 기능이 잠겨 있으면 선택된 탭 없이 짧은 안내를 표시한다. 이용 가능한 기능이 있으면 그 기능을 기본 화면으로 사용한다.

자물쇠 원본은 `UiLockGraphic.cs`의 벡터 메시다. 기존 `UiButtonFace`·`StorageChestGraphic`과 같은 방식으로 황동 앞면, 철제 뒷판, 경첩, 리벳과 열쇠구멍을 직접 그렸다. 공통 테마 색을 사용하고 입력을 받지 않는다. 래스터 생성 모델, 외부 원화와 텍스처 임포트는 사용하지 않았다. 대장간 어댑터의 기존 캔버스·폰트·안전 영역·창 관리자와 1:1:1 본문 배치를 유지한다.

### 잠금 탐색 검증 결과

- 최종 관련 Edit Mode **110개 통과, 실패·건너뜀 0개**. 버튼, 그래픽 구성, 계정 해금과 번역을 검사했다. [검사 XML](ForgeNavigationEvidence/editmode.xml)
- 튜토리얼까지 넓힌 검사는 140개 중 139개 통과·1개 실패다. 실패한 `EdictAndNewSkillRequireCommittedChangeThenMatchingTrainingAndFreshRift`는 레벨 3 액티브 스킬을 찾지 못한다. 대장간 변경을 모두 제외한 기반 `412be7dc`에서도 같은 실패를 재현했다. 스킬 규칙과 이 검사는 수정하지 않았다. [확장 검사](ForgeNavigationEvidence/extended-editmode.xml) · [기반 비교](ForgeNavigationEvidence/baseline-skill-failure.xml)
- macOS 개발 빌드 오류 0개. 다섯 기능의 잠금·정확한 조건·3초 만료·키보드 제출·뒤로가기·재화 무변경, R1 해금과 기존 장비 선택 유지, 전체 기능 해금 후 탐색·기존 제작 화면 이동을 실제 uGUI 레이캐스트와 이벤트로 확인했다. [실행 결과](ForgeNavigationEvidence/runtime.txt) · [빌드](ForgeNavigationEvidence/build.txt)
- 440×956, 956×440, 1600×900, 1440×900, 1680×720 × 한국어/영어 × 100%/150%의 20조합에서 버튼 크기, 글자 잘림, 말풍선 위치와 갱신을 검사했다. 새 계정의 튜토리얼·출석 안내만 검증 계정에서 생략하고, 콘텐츠 개방 기록은 R0에서 시작했다. 연결된 MCP 에디터가 없어 기존 배치 검사·macOS 실행 도구를 사용했다. 모바일 실기기는 검증하지 않았다. [범위와 캡처 해시](ForgeNavigationEvidence/verification.json) · [소스 해시](ForgeNavigationEvidence/source-fingerprints.json)

[모두 잠긴 PC 화면](ForgeNavigationEvidence/all-locked-pc.png) · [가로 말풍선](ForgeNavigationEvidence/locked-956-ko-100.png) · [세로 화면](ForgeNavigationEvidence/locked-440-ko-100.png) · [영어 150%](ForgeNavigationEvidence/locked-440-en-150.png) · [해금 후](ForgeNavigationEvidence/all-unlocked-landscape.png)

## 실행 환경과 근거

Unity 6000.6.0f1의 Edit Mode 검사와 macOS 개발 빌드를 사용한다. 구현 브랜치는 `codex/blacksmith-runtime`이며, 커밋된 인벤토리 기반에 `main`을 병합한 별도 작업 디렉터리에서 검증한다. 원래 체크아웃의 실행 중인 에디터, 미커밋 인벤토리 변경과 기존 저장 파일은 수정하지 않는다.

macOS 실행 검사는 별도 저장 경로에 만든 검증용 계정을 사용한다. UGUI의 실제 화면 좌표에서 레이캐스트로 버튼의 노출을 확인한 뒤 포인터 이벤트를 보내고, 키보드 E 입력도 전달한다. 클릭 전후의 실제 `GameStore` 잔액·강화 단계·작업·저장 복구 결과를 확인한다. 사진만 보고 기능 성공으로 판정하지 않는다.

자동 검증 실행에서는 macOS 창의 초점 이동으로 합성 키보드가 비활성화되지 않도록 입력 설정을 일시적으로 조정한다. 이 설정은 검증 실행 인자가 있는 개발 빌드에만 적용한다.

## 검사 결과

- HTML 동작 검사: **80개 통과, 실패 0개**.
- 대장간·자원·품질·능력치·번역 집중 Edit Mode 검사: **178개 통과, 실패·건너뜀 0개**. [원본 XML](BlacksmithEvidence/blacksmith-focused-editmode.xml)과 [요약·해시](BlacksmithEvidence/focused-summary.json)를 보존한다. 이 수는 전체 회귀 검사와 중복되므로 더하지 않는다.
- 최신 인벤토리 `8a5c88d`와 `main` `12e0a88`을 합친 뒤 실행한 통합 Edit Mode 검사: **312개 통과, 실패·건너뜀 0개**, 99.25초. 대장간·공용 비교·분해 설정·인벤토리·상점·룬·그래픽·마을을 포함한다. [원본 XML](BlacksmithEvidence/blacksmith-integration-editmode.xml) · [요약·해시](BlacksmithEvidence/integration-summary.json). 검사 수는 다른 실행과 중복된다.
- macOS 개발 빌드: 성공, 빌드 오류 0개. 실제 실행 검사는 `HELLSCRIPT_BLACKSMITH_SMOKE_OK`로 종료했다. [실행 결과](BlacksmithEvidence/runtime-result.txt)와 [69개 캡처의 해시 목록](BlacksmithEvidence/runtime-manifest.json)을 보존한다.
- 최종 macOS 인벤토리 통합 검증도 `HELLSCRIPT_INVENTORY_SMOKE_OK`로 종료했다. 한·영 가로·세로에서 인벤토리·상점·창고의 공용 비교, 두 반지의 비교 기준, 분해 자동 선택 설정의 공유·저장과 전투 입력 차단을 확인했다. [실행 결과](BlacksmithEvidence/inventory-runtime-result.txt) · [대표 화면 해시](BlacksmithEvidence/inventory-runtime-manifest.json) · [비교 화면](BlacksmithEvidence/inventory-20-shared-inventory-landscape-ko.png).
- 리소스: 투명 PNG **30개**의 알파 채널·투명 픽셀·내용과 해시를 확인했다.

전체 Edit Mode 회귀 검사: **2,880개 통과, 실패·건너뜀 0개**, 1,518.18초. [전체 XML](BlacksmithEvidence/blacksmith-full-editmode.xml)과 [요약](BlacksmithEvidence/full-summary.json)을 보존한다. 이 전체 검사는 최신 인벤토리 병합 전의 대장간 구현 스냅샷에서 실행했다. 이후 추가한 보스 보상 단계 검사와 공용 비교 통합은 최신 312개 검사에 포함한다. 언어 변경 시 화면 유지와 최종 통합 화면은 macOS 실행 검사로 확인한다.

실행본에는 기존 URP 빌드 설정의 Lens Flare·Panini 후처리 셰이더 제외 로그가 남는다. 대장간 UI·입력·저장 검증은 통과했으며 해당 후처리 효과까지 검증한 것으로 보고하지 않는다.

## 화면 기록

| 기준 | 대표 캡처 |
| --- | --- |
| 440×956 | [옵션 목록](BlacksmithEvidence/portrait-ko-tab0-list.png) · [옵션 상세](BlacksmithEvidence/portrait-ko-tab0.png) · [슬롯 성장](BlacksmithEvidence/portrait-ko-tab1.png) · [장비 강화](BlacksmithEvidence/portrait-ko-tab2.png) · [영어 슬롯](BlacksmithEvidence/portrait-en-tab1.png) · [영어 강화](BlacksmithEvidence/portrait-en-tab2.png) |
| 956×440 | [옵션 변경](BlacksmithEvidence/landscape-ko-tab0.png) · [슬롯 성장](BlacksmithEvidence/landscape-ko-tab1.png) · [장비 강화](BlacksmithEvidence/landscape-ko-tab2.png) · [영어 성장 상세](BlacksmithEvidence/landscape-en-growth.png) |
| PC 16:9 | [슬롯 성장](BlacksmithEvidence/pc-16x9-ko-tab1.png) |
| PC 16:10 | [영어 옵션 변경](BlacksmithEvidence/pc-16x10-en-tab0.png) |
| PC 21:9 | [장비 강화](BlacksmithEvidence/pc-21x9-ko-tab2.png) |
| 주요 상태 | [자동 목표 발견](BlacksmithEvidence/interaction-auto-match.png) · [Lv.75 미리보기](BlacksmithEvidence/interaction-preview-75.png) · [해금 안내](BlacksmithEvidence/interaction-unlock-tooltip.png) · [작업 표시 1](BlacksmithEvidence/interaction-working-bright.png) · [작업 표시 2](BlacksmithEvidence/interaction-working-pulse.png) |
| 고정 팝업 | [가능 옵션](BlacksmithEvidence/portrait-ko-candidates.png) · [성장 상세](BlacksmithEvidence/portrait-ko-growth.png) |

전체 69개 화면은 리소스 묶음의 `Artifacts/Blacksmith/RuntimeEvidence/`에 포함한다. 위 대표 화면은 Git에도 보존한다.

## 검증표

| 대상 | 확인 항목 | 근거 |
| --- | --- | --- |
| 강화 견적 | 1~100단계 가격, +10 합계, 최대치와 잔액, +97→100, 부족·변경된 견적의 무차감 | `BlacksmithTests` |
| 거래 보호 | 같은 요청 재실행, 저장 실패, 재화 넘침, 상태 보존 | `BlacksmithTests` |
| 품질·이전 | 이전 +5 보존, 각성·걸작 뒤 고정 증가량, 접사 고정·투자 기록 보존, 걸작 +5 이상 | `BlacksmithTests`, 품질 검사 |
| 옵션 변경 | 최초 결제 후 위치 고정, 반복 비용 유지, 실제 후보·범위, 자동 목표 대기·계속·중단·창 닫기 | 코어 검사와 macOS 실행 검사 |
| 작업 칸 | 캐릭터별 성장, 계정 공용 두 칸, 10·50·250다이아 순차 개방, 중복 작업 차단 | `BlacksmithTests` |
| 시간 | 도달 레벨별 비용·시간, 종료 후 한 번 정산, 60초 1다이아·59초/1초 무료 | `BlacksmithTests`, macOS 실행 검사 |
| 전투 | 진행 중 슬롯 스냅샷 유지, 스냅샷이 없는 이전 균열의 캐릭터 강화 단계 사용, 다음 전투 반영, 원소 저항 70% 상한, 배운 액티브·장착 패시브의 유효 레벨 | 코어·스킬·전투 회귀 검사, `BlacksmithTests`, macOS 가방 스모크 |
| 분해 | 전설 10·고유 효과 15·세트 10을 단건/일괄/자동 경로 각각 검사, 코어와 저장 보존 | 9개 경로 조합 검사 |
| 균열·소탕 | 보상 단계별 강화석, 한 번만 획득, 소탕 반복 요청 차단 | `BlacksmithTests`와 자원 검사 |
| 접근·화면 | NPC 범위 밖 차단, 대장간 이용·E키, 같은 폭의 세 열, 목록 왕복, 전체 화면 팝업 | macOS 실행 검사 |
| 상세 연출 | 성장 미리보기 값 갱신, 3초 해금 안내, 작업 중 점멸 | macOS 상태 검사와 캡처 |
| 언어·비율 | 한국어·영어, 440×956·956×440·PC 16:9/16:10/21:9, 인벤토리 공용 등급 색상 | 번역 검사와 macOS 캡처 |

## 재현 방법

HTML: `node --test Prototypes/Blacksmith/*.test.cjs`.

Unity: `-batchmode -nographics -runTests -testPlatform EditMode -testResults <xml>`로 검사한다. 대장간 범위만 실행할 때는 `-testFilter Hellscript.Tests.BlacksmithTests`를 추가한다.

macOS 개발 빌드: 기존 `Hellscript.Editor.ProjectBuilder.BuildMac`을 실행하고 `-hellscriptBuildOutput <app 경로>`를 전달한다. 실행본에 `-hellscriptBlacksmithSmoke -hellscriptSavePath <새 검증 저장 폴더> -hellscriptScreenshots <증거 폴더>`를 전달하면 자동 검증 후 종료한다. 성공 표식은 `HELLSCRIPT_BLACKSMITH_SMOKE_OK`와 증거 폴더의 `result.txt`다. 일반 게임 실행에는 해당 인자를 넣지 않는다.

스냅샷이 없는 균열의 가방 검사는 `-hellscriptLegacyRunBagSmoke -hellscriptSavePath <저장 폴더> -hellscriptScreenshots <증거 폴더>`로 실행한다. 저장 폴더에 그런 균열이 든 `hellscript-local-v1.json`의 복사본을 넣으면 그 저장본을 사용하고, 빈 폴더를 주면 같은 상태를 새 계정에 만든다. 성공 표식은 `HELLSCRIPT_LEGACY_RUN_BAG_SMOKE_OK`와 증거 폴더의 `runtime-legacy-run-bag.txt`다.

현재 작업 폴더의 `Artifacts/Blacksmith/Play-Blacksmith.command`는 별도 검증 계정으로 수동 플레이를 시작한다. 원래 계정과 저장 경로를 공유하지 않는다.

리소스 묶음: `python3 tools/blacksmith/package.py`. 결과는 `Artifacts/Blacksmith/HELLSCRIPT-Blacksmith-Resources.zip`이며 파일별 SHA-256 목록을 포함한다. 이 묶음은 리소스와 적용 근거를 전달한다. 실행 가능한 전체 변경은 Git 브랜치로 통합한다.

## 확인 범위와 공개 상태

모바일 해상도는 macOS 창에서 재현했다. 실제 아이폰·안드로이드의 터치, 키보드, 회전, 노치와 성능은 별도 기기 검증이 필요하다. 로컬 저장 어댑터를 사용하며 서버 시각·서버 재화 거래를 구현했다고 주장하지 않는다.

이 작업 브랜치에서는 공개 위키를 배포하지 않는다. `main` 병합 담당자가 최신 `main`에서 위키를 다시 생성·검사하고 공개 저장소와 GitHub Pages에 게시한 뒤 확인한다. 공개 주소는 [HELLSCRIPT Wiki](https://dakrdong.github.io/HELLSCRIPT-Wiki/#/tree)다.
