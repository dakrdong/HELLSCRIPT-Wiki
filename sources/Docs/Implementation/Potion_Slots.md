# 인벤토리 물약 슬롯과 보유 재화

갱신일: 2026-09-23

인벤토리의 무기 슬롯 옆 구석에 작은 물약 전용 슬롯 3개를 배치하고, 하단에 실제 보유 재화를 표시한다. 물약은 캐릭터별 재고, 재화는 계정 공용 잔액을 읽는다. HTML 시안이 아닌 Unity 플레이 씬의 기존 인벤토리에 연결한 기능이다.

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

하단의 고정 영역에 이름과 수량을 표시한다. 0개도 숨기지 않고, 큰 수량은 천 단위 구분을 포함한 정확한 숫자로 표시한다.

| 표시 | 실제 저장 데이터 |
| --- | --- |
| 골드 | `AccountSave.gold` |
| 심연 주화 | 기존 유료재화 `AccountSave.premium` |
| 일반 재료 | `AccountSave.materials` |
| 강화석 | `AccountSave.enhancementStones` |
| 부위별 코어 8종 | `AccountSave.cores`: 무기·머리·몸통·손·발·벨트·목걸이·반지 |

코어별 수량은 ‘전체 재화’ 창에서 함께 확인한다. 외곽 크기는 고정하고 내부 목록을 스크롤한다. 저장이 성공하면 열린 재화 창의 숫자도 갱신하며 창 위치와 스크롤 위치를 유지한다. 이 화면은 재화를 지급·차감·이동하지 않는다. 심연 주화는 새 잔액을 만든 것이 아니라 사용자가 확인한 기존 `premium`의 표시 이름이다.

심연 주화 그림은 대장간 시안의 원본 PNG를 변경 없이 가져왔다. [출처 기록](PotionSlotsEvidence/abyssal-coin-source.json)과 [기존 생성 요청](PotionSlotsEvidence/abyssal-coin-generation-prompt.txt)을 보존했다. 원본 SHA-256은 `6f48ca86c7f47f886c7c009176200a30c0d942b5c6f610a2dc2631f8a7ee9097`이며 투명도와 1,254×1,254 크기를 유지한다. 생성 모델 출처가 확인되지 않은 개발용 후보라는 기존 상태는 바꾸지 않는다.

## 공통 UI 연결과 검증

기존 `InventoryWindow`의 고정 배치 어댑터 안에서 `EquipmentSlotView`, `UiTheme`, `UiFonts`를 재사용한다. `PotionArt`는 HUD와 인벤토리의 병 그림을 공유한다. 재화 표시는 `InventoryWindow.Wallet`이 계정 상태를 읽고 `StoreViewBinding`의 저장 성공 알림에 맞춰 갱신한다. 물약 말풍선은 최초 크기를 유지하며 필터·선택 조작 때문에 가방 내용이 밀리지 않는다.

- Unity 6000.6.0f1의 관련 Edit Mode 검사 **126개 통과, 실패·건너뜀 0개**: 소진 순서·중복 배치·잘못된 종류·저장 실패·재접속·전투·훈련 격리·기존 HUD·현지화를 포함한다. [결과 XML](PotionSlotsEvidence/compact-editmode.xml)
- macOS 개발 빌드에서 세로 440×956·가로 956×440·PC 1440×810/1440×900/1680×720과 한국어·영어, 글자 100%·150%의 **20조합**을 통과했다. 실제 uGUI 이벤트·레이캐스트로 선택, 배치 변경, 장비 드롭 거부, 저장 재읽기, HUD 반영을 검사했다. [실행 결과](PotionSlotsEvidence/compact-potion-slots-result.txt)
- 재화 4종과 코어 8종의 실제 저장값, 0개·2,147,483,647개의 정확한 표시, 원본 주화 아이콘 로드, 조회 시 무변경, 저장 성공 후 숫자 갱신과 스크롤 보존을 확인했다.
- 공통 UI 소유 검사와 검사기 9개, 물약 DB 검사 1개가 통과했다. 실제 휴대폰의 터치·성능 검증은 하지 않았다. macOS 자동 입력과 격리된 검사 저장 데이터를 사용했다.

대표 화면: [세로](PotionSlotsEvidence/compact-slots-440x956-ko-100.png) · [가로](PotionSlotsEvidence/compact-slots-956x440-ko-100.png) · [영문 확대 말풍선](PotionSlotsEvidence/compact-policy-440x956-en-150.png) · [가로 확대 말풍선](PotionSlotsEvidence/compact-policy-956x440-ko-150.png) · [재화·코어 스크롤](PotionSlotsEvidence/compact-wallet-scrolled-440x956-en-150.png) · [PC 21:9](PotionSlotsEvidence/compact-slots-1680x720-en-100.png)

## 주요 코드

- [물약 배치와 획득 순서](../../Assets/HELLSCRIPT/Runtime/Core/PotionLoadout.cs)
- [물약 저장 거래](../../Assets/HELLSCRIPT/Runtime/Core/GameStore.Potions.cs)
- [물약 슬롯과 말풍선](../../Assets/HELLSCRIPT/Runtime/Presentation/InventoryWindow.Potions.cs)
- [실제 계정 재화 표시](../../Assets/HELLSCRIPT/Runtime/Presentation/InventoryWindow.Wallet.cs)
- [소진·저장·전투 검사](../../Assets/HELLSCRIPT/Tests/Editor/PotionLoadoutTests.cs)
- [macOS 입력·화면 검증](../../Assets/HELLSCRIPT/Runtime/Presentation/RuntimeInventorySmoke.Potions.cs)
