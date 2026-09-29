# 인벤토리 물약 슬롯과 보유 재화

갱신일: 2026-09-29

인벤토리의 무기 슬롯 옆 구석에 작은 물약 전용 슬롯 3개를 배치하고, 최상단 제목 바로 아래에 실제 보유 재화를 표시한다. 물약은 캐릭터별 재고, 재화는 계정 공용 잔액을 읽는다. HTML 시안이 아닌 Unity 플레이 씬의 기존 인벤토리에 연결한 기능이다.

English: [Inventory potion slots and account balances](Potion_Slots.en.md)

## 물약 배치와 소진 후 사용

- 물약 칸은 세로 32·가로 28로 장비 칸보다 작다. 별도 줄을 만들지 않고 무기 칸과 같은 높이의 오른쪽 구석에 나란히 놓는다. 세로 화면은 이전 물약 줄을 없애 캐릭터 영역을 50만큼 줄이고 소지품 영역을 넓힌다. 그림은 기존 원본을 보존하면서 가운데에 맞춘다.
- 세 물약 칸 옆에 공통 톱니바퀴 하나를 둔다. 누르면 ‘지정 물약을 모두 소진 했을 시’ 말풍선이 열리며 높은 등급·낮은 등급·최근 획득·오래된 획득 중 하나를 선택한다. 체크와 강조색으로 현재 선택을 표시하고, 같은 기준을 세 칸 모두에 적용한다.
- 물약 칸을 누르면 바로 보유 물약 선택 창이 열리고 ‘해제’로 빈칸을 만든다. 다른 슬롯에 지정한 물약과 미보유 물약은 선택할 수 없다. 장비를 물약 칸에 놓아도 장착되지 않는다.
- 지정 물약을 먼저 사용하고, 소진되면 **같은 효과**의 남은 물약만 해당 기준으로 고른다. 다른 슬롯의 지정품과 이미 선택한 대체품은 제외하며, 후보가 없으면 사용을 멈춘다.
- 배치는 기존 규칙대로 마을에서 변경한다. 소진 후 기준은 캐릭터별 공통 값 하나로 저장하고 전투 중에도 바꿀 수 있다. UI를 열거나 기준을 바꾸는 것만으로 물약을 소비하지 않는다. 과거 슬롯별 설정이 있으면 첫 번째로 물약이 지정된 칸의 기준을 한 번만 공통 값으로 옮긴다. 새 공통 값을 저장한 뒤에는 옛 슬롯별 값이 개입하지 않는다.

현재 Unity 카탈로그의 물약 8종은 모두 등급 0이며 효과별 상·하위 등급은 아직 없다. 등급·획득 순서 판정은 데이터에 연결했지만, 새 물약의 성능이나 가격을 임의로 추가하지 않았다. 같은 효과의 대체품이 없는 현재 재고에서는 소진 후 사용이 멈춘다. 보조 물약을 여러 칸에 지정해도 기존 공통 재사용 시간과 한 번에 하나의 활성 효과를 유지한다.

배치·기준은 `GameStore.SetPotionSlot`과 `SetPotionFallback` 거래로 저장하고, 저장 실패 시 메모리와 디스크 모두 이전 상태를 유지한다. `PotionLoadout`은 대체품 선택을, `PotionInventory`는 실제 수량과 획득 묶음을 관리한다. 과거 획득 기록이 없는 저장은 당시 저장 목록 순서를 한 번만 기준으로 삼으며, 실제 과거 시각을 복원했다고 간주하지 않는다. 자세한 조건은 [물약과 사냥 칙령](../Design/GlobalHUD/HELLSCRIPT_GlobalHUD_05_Potions.md)을 따른다.

## 재화 수량

가로·세로 모두 최상단 제목 바로 아래의 고정 영역에 이름과 수량을 표시한다. 재화 줄은 창 전체 너비를 사용하고, 캐릭터와 소지품은 그 아래에 배치한다. 0개도 숨기지 않고, 큰 수량은 천 단위 구분을 포함한 정확한 숫자로 표시한다. 소지품을 스크롤하거나 수량이 갱신되어도 재화 줄의 위치와 크기는 바뀌지 않는다.

| 표시 | 실제 저장 데이터 |
| --- | --- |
| 골드 | `AccountSave.gold` |
| 심연 주화 | 기존 유료재화 `AccountSave.premium` |
| 일반 재료 | `AccountSave.materials` |
| 강화석 | `AccountSave.enhancementStones` |
| 부위별 코어 8종 | `AccountSave.cores`: 무기·머리·몸통·손·발·벨트·목걸이·반지 |

코어별 수량은 ‘전체 재화’ 창에서 함께 확인한다. 외곽 크기는 고정하고 내부 목록을 스크롤한다. 저장이 성공하면 열린 재화 창의 숫자도 갱신하며 창 위치와 스크롤 위치를 유지한다. 이 화면은 재화를 지급·차감·이동하지 않는다. 심연 주화는 새 잔액을 만든 것이 아니라 사용자가 확인한 기존 `premium`의 표시 이름이다.

심연 주화 그림은 대장간 시안의 원본 PNG를 변경 없이 가져왔다. [출처 기록](PotionSlotsEvidence/abyssal-coin-source.json)과 [기존 생성 요청](PotionSlotsEvidence/abyssal-coin-generation-prompt.txt)을 보존했다. 원본 SHA-256은 `6f48ca86c7f47f886c7c009176200a30c0d942b5c6f610a2dc2631f8a7ee9097`이며 투명도와 1,254×1,254 크기를 유지한다. 생성 모델 출처가 확인되지 않은 개발용 후보라는 기존 상태는 바꾸지 않는다.

## 재화 그림과 소지품 공간

2026-09-29부터 재화 이름·아이콘·수량을 같은 줄에 붙여 표시한다. 가로 화면은 네 항목을 한 줄에, 세로 화면은 두 항목씩 두 줄에 배치해 큰 숫자와 영어 이름을 함께 읽을 수 있게 한다. 재화 영역 높이는 가로 24·세로 40으로 고정한다. 수량 변경으로 이름·아이콘 위치가 움직이지 않는다.

‘소지품’ 제목은 기본 글자 크기 16에서 12로 줄였다. 필터 아래의 상시 보호 안내, 하단의 드래그 안내, ‘새 장비 연속 비교’ 기능과 전용 탐색·창고 이동 경로를 제거했다. 개별 아이템의 상세·잠금·장착 비교는 유지한다. 오류와 장착 결과는 목록 위에 잠깐 겹쳐 표시하며, 분해 선택·취소·알림 때문에 목록 크기를 바꾸지 않는다. 내부 좌표 기준으로 목록 높이는 기존보다 가로 94·세로 88 늘었다. 슬롯 크기·저장 위치·수량·분해 보호 규칙은 바꾸지 않는다.

강화석은 은빛 광석과 금빛 균열로 구분한다. 코어 8종은 같은 검은 수정·금속 틀에 부위별 오라를 입혔다. `CurrencyIconView`가 인벤토리와 대장간 코어 목록의 그림을 공유한다. 전체 재화 창의 아이콘은 행 높이 42 안에서 36으로 표시하며 기존 창 크기와 스크롤을 유지한다.

| 부위 | 오라 색 |
| --- | --- |
| 무기 | 붉은색 |
| 머리 | 보라색 |
| 몸통 | 파란색 |
| 손 | 주황색 |
| 발 | 초록색 |
| 벨트 | 청록색 |
| 목걸이 | 옅은 금색 |
| 반지 | 진분홍색 |

내장 `image_gen`으로 각각 생성·편집했으며 원본 PNG 바이트와 네이티브 알파를 유지한다. 후처리로 배경을 지우거나 색을 바꾸지 않았다. 원본은 각각 1,254×1,254이며 게임에서는 플랫폼별 512 텍스처 예산을 적용한다. [요청·생성 출처](../Art/Currency/generation.json)와 [투명도 검사](../Art/Currency/alpha-validation.json)를 보존한다. 도구가 실제 모델명을 반환하지 않았으므로 `gpt-image-2` 사용을 검증할 수 없다. **모델 미확인 시안이며 출시 확정 자산이 아니다.** 강화석의 한 모서리에 남은 1/255 알파도 원본 그대로 보존했다.

## 공통 UI 연결과 검증

기존 `InventoryWindow`의 고정 배치 어댑터 안에서 `EquipmentSlotView`, `UiTheme`, `UiFonts`를 재사용한다. `PotionArt`는 HUD와 인벤토리의 병 그림을 공유한다. 재화 표시는 `InventoryWindow.Wallet`이 계정 상태를 읽고 `StoreViewBinding`의 저장 성공 알림에 맞춰 갱신한다. 물약 말풍선은 최초 크기를 유지하며 필터·선택 조작 때문에 가방 내용이 밀리지 않는다.

- Unity 6000.6.0f1의 관련 Edit Mode 검사 **126개 통과, 실패·건너뜀 0개**: 소진 순서·중복 배치·잘못된 종류·저장 실패·재접속·전투·훈련 격리·기존 HUD·현지화를 포함한다. [결과 XML](PotionSlotsEvidence/compact-editmode.xml)
- 재화 표시를 최상단으로 옮긴 뒤 인벤토리·현지화 관련 Edit Mode 검사 **69개 통과, 실패·건너뜀 0개**를 확인했다. 아래 macOS 검증에는 재화 줄이 제목 바로 아래에 있고, 창 전체 너비를 사용하며, 장비·소지품과 겹치지 않는지도 포함한다. [상단 재화 검사 XML](PotionSlotsEvidence/header-editmode.xml)
- macOS 개발 빌드에서 세로 440×956·가로 956×440·PC 1440×810/1440×900/1680×720과 한국어·영어, 글자 100%·150%의 **20조합**을 통과했다. 실제 uGUI 이벤트·레이캐스트로 선택, 배치 변경, 장비 드롭 거부, 저장 재읽기, HUD 반영을 검사했다. [실행 결과](PotionSlotsEvidence/header-potion-slots-result.txt)
- 재화 4종과 코어 8종의 실제 저장값, 0개·2,147,483,647개의 정확한 표시, 원본 주화 아이콘 로드, 조회 시 무변경, 저장 성공 후 숫자 갱신과 스크롤 보존을 확인했다.
- 공통 UI 소유 검사와 검사기 9개, 물약 DB 검사 1개가 통과했다. 실제 휴대폰의 터치·성능 검증은 하지 않았다. macOS 자동 입력과 격리된 검사 저장 데이터를 사용했다.

대표 화면: [세로](PotionSlotsEvidence/header-slots-440x956-ko-100.png) · [가로](PotionSlotsEvidence/header-slots-956x440-ko-100.png) · [영문 확대 말풍선](PotionSlotsEvidence/header-policy-440x956-en-150.png) · [가로 확대 말풍선](PotionSlotsEvidence/header-policy-956x440-ko-150.png) · [재화·코어 스크롤](PotionSlotsEvidence/header-wallet-scrolled-440x956-en-150.png) · [PC 21:9](PotionSlotsEvidence/header-slots-1680x720-en-100.png)

## 2026-09-29 재화 그림·공간 검증

- `main`의 칙령·출석 아이콘 변경(`323dca88`)을 포함한 소스에서 Unity Edit Mode **72개 통과, 실패·건너뜀 0개**를 확인했다. 원본 PNG 투명도, 아홉 아이콘의 개별 로드와 512 텍스처 예산, 장비 거래·분해 보호, 현지화를 검사했다. [검사 XML](CurrencyInventoryEvidence/editmode.xml)
- macOS 개발 빌드의 세로 440×956·가로 956×440·PC 1440×810/1440×900/1680×720, 한국어·영어, 글자 100%·150%의 **20조합**에서 재화·물약·확장한 목록을 확인했다. 이름과 수량의 같은 줄 배치, 0개·최대 정수 수량, 8부위 그림, 분해 모드·알림의 위치 고정, 저장 후 재화 갱신과 스크롤 보존을 검사했다. [실행 결과](CurrencyInventoryEvidence/runtime.txt)
- 같은 빌드에서 코어 제작 해금 조건을 충족한 테스트 캐릭터가 대장장이에게 실제로 이동한 뒤, 코어 제작 목록에도 8종 그림이 연결되는 것을 확인했다. [대장간 화면](CurrencyInventoryEvidence/blacksmith.png)
- 인벤토리의 기존 가로·세로 조작 검사도 통과했다. 개별·반지 비교, 장착과 끌어놓기, 잠금, 선택·일괄 분해, 필터 저장, 전투 중 창 열기·닫기의 일시 정지 복원을 확인했다. [조작 결과](CurrencyInventoryEvidence/interactions.txt)
- 실제 입력은 macOS 플레이어의 uGUI 레이캐스트와 합성 포인터로 수행했다. 연결된 CoplayDev Editor 인스턴스가 없어 별도 작업 폴더의 Unity 배치 검사와 macOS 빌드를 사용했다. 실제 휴대폰의 터치·성능은 검증하지 않았다.

현재 화면: [가로](CurrencyInventoryEvidence/landscape.png) · [세로](CurrencyInventoryEvidence/portrait.png) · [재화 그림](CurrencyInventoryEvidence/wallet.png) · [모든 코어·영어 확대](CurrencyInventoryEvidence/cores-en-large.png)

## 주요 코드

- [물약 배치와 획득 순서](../../Assets/HELLSCRIPT/Runtime/Core/PotionLoadout.cs)
- [물약 저장 거래](../../Assets/HELLSCRIPT/Runtime/Core/GameStore.Potions.cs)
- [물약 슬롯과 말풍선](../../Assets/HELLSCRIPT/Runtime/Presentation/InventoryWindow.Potions.cs)
- [실제 계정 재화 표시](../../Assets/HELLSCRIPT/Runtime/Presentation/InventoryWindow.Wallet.cs)
- [소진·저장·전투 검사](../../Assets/HELLSCRIPT/Tests/Editor/PotionLoadoutTests.cs)
- [macOS 입력·화면 검증](../../Assets/HELLSCRIPT/Runtime/Presentation/RuntimeInventorySmoke.Potions.cs)
