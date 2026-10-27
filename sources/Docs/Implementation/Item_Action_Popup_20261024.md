# 아이템 옆 기능 메뉴 — 사용자 항목 9 (2026-10-24)

[English](Item_Action_Popup_20261024.en.md). 아이템 칸을 한 번 누르면 큰 상세 창 대신 **칸 옆에 작은 메뉴**가 뜨고, 어느 창·어느 칸에서 눌렀는지, 아이템이 잠겼는지·착용 중인지에 따라 항목이 달라진다. 상세 정보와 비교는 기존 큰 창(`ItemDetailView`, `EquipmentComparisonView`)을 그대로 연다. 새 상세 창은 만들지 않는다.

시안: 디자인 캔버스 `HELLSCRIPT 창고 · 캐주얼`(https://claude.ai/artifact/BVaVczJFgajuJxeHfEw1R8)의 `menu` 속성(none · wh · bag · baglow · locked · full)과 `아이템 메뉴 · 항목 규칙` 대지. 사용자는 기존 H 시안을 사용하는 권장안에 동의했으며, 아래 범위의 구현을 진행한다(승인 근거는 문서 끝).

## 1. 범위

| 창 | 칸 | 이번 묶음 |
| --- | --- | --- |
| 창고 창(`StorageWindow`) | 창고 칸, 소지품 칸 | 메뉴 적용 |
| 캐릭터 화면(`InventoryWindow`) | 소지품 칸, 착용 중인 장비 | 메뉴 적용(튜토리얼 수업 중에는 기존 큰 창 직행) |
| 상점 판매 목록(`EquipmentShopWindow`) | 판매 칸 | **바꾸지 않음**: 누르면 지금처럼 판매 선택 토글. 메뉴는 다음 묶음에서 별도 결정(U-D가 상점 목록을 만짐) |
| 균열 보상·추천 장비·보상 상자·대장간·도감·캐릭터 선택·방치 대시보드 | — | 바꾸지 않음(각 화면이 자기 행동 버튼을 가진 큰 창 직행) |

## 2. 지금의 칸 누르기 동작 (코드 조사, `Assets/HELLSCRIPT/Runtime/` 기준)

더블탭·길게 누르기·오른쪽 클릭·Ctrl/Shift 클릭 단축은 어떤 아이템 칸에도 **없다**(전수 검색). 유지해야 할 단축 동작은 끌기뿐이다.

| 창 / 칸 | 누르면 | 결과 창의 행동 | 위치 |
| --- | --- | --- | --- |
| 창고·소지품 칸(일괄 아님) | `CellClicked` → `ShowDetail` | 상세 + 현재 위치 + 이동 목적지. 버튼: 꺼내기/창고에 넣기, 아이템 잠금/잠금 해제, 현재 장비와 비교/비교 닫기 | Presentation/StorageWindow.cs:239-251, StorageWindow.Dialogs.cs:23-65 |
| 창고 일괄 이동 모드 | 선택 토글(한쪽만) | 없음 | StorageWindow.cs:246-250 |
| 창고 끌기 | 누른 채 움직이면 바로 집힘 → 놓은 곳으로 이동·교환 | 고스트·힌트 | StorageWindow.Drag.cs:50-92 |
| 창고 묶음 칸(재료·보석·룬 블럭) | `ShowStack` | 읽기 전용 | StorageWindow.cs:243, Dialogs.cs:67-92 |
| 캐릭터 화면 소지품 칸·착용 칸 | `InventoryCell` → `SelectItem` → `ShowDetail` | 미착용은 비교 화면, 착용은 상세. 버튼: `inventory-equip`(장착/장착 해제/“{슬롯}에 장착”), `inventory-lock`, `inventory-detail-close` | InventoryWindow.Drag.cs:15, InventoryWindow.cs:190-198, Dialogs.cs:26-57 |
| 캐릭터 화면 분해 선택 모드 | 선택 토글(보호 아이템은 거부) | 없음 | InventoryWindow.cs:190-198 |
| 캐릭터 화면 끌기 | 마우스는 즉시, 터치는 0.18초 이상 눌러야 집힘 → 장착/해제 | 고스트 | InventoryWindow.Drag.cs:18-62 |
| 상점 판매 목록 칸 | `ItemClick`: 수동이면 선택 토글, 아니면 인라인 상세/`ShowMockItem` | 판매하기 | EquipmentShopWindow.cs:174-179 |
| 균열 보상(`victory-loot-*`), 추천 장비 팝업 등 | 큰 창 `ItemDetailPopup` | 비교·장착 | RiftVictoryWindow.Loot.cs, RiftRecommendationWindow.cs |

시간 상수: 창고 클릭 억제 0.35초(드롭 직후), 인벤토리 0.2초, 창고 탭 호버 1초, 토스트 3.2초.

## 3. 새 동작

### 항목 규칙 (한 곳: `Core/ItemActionMenu.cs`)

| 누른 곳 | 상세 정보 | 이동 | 아이템 잠금 | 착용 장비와 비교 | 장착·해제 |
| --- | --- | --- | --- | --- | --- |
| 창고 창 · 창고 칸 | ● | 소지품으로 이동 | 잠금/잠금 해제 | ● | – |
| 창고 창 · 소지품 칸 | ● | 창고로 이동 | 잠금/잠금 해제 | ● | – |
| 캐릭터 화면 · 소지품 칸 | ● | – | 잠금/잠금 해제 | ● | 장착 |
| 캐릭터 화면 · 착용 중 | ● | – | 잠금/잠금 해제 | – | 장착 해제 |

- 막힌 줄은 숨기지 않고 회색으로 두고 이유 한 줄을 보인다. 이유는 기존 사전 검사의 문구다: 이동 `Storage.MoveError`, 해제 `EquipmentSlots.UnequipError`와 필수 갑옷(`Tutorials.RequiredArmor`의 기존 갑옷 보호 문구). 장착은 `EquipmentSlots.Targets`의 놓을 곳 중 **하나라도** `Plan(hero,item,index)`을 통과하면 활성이고, 모두 막히면 첫 사유를 보인다(기본 위치 한 번만 검사하면 반지·두 손 자리를 대표하지 못한다). 필수 갑옷의 잠금 해제 금지는 거래 쪽(`SetInventoryLock`)이 일반 오류만 돌려주므로 메뉴가 `RequiredArmor`를 미리 검사해 같은 갑옷 보호 문구를 보이고, 거래 쪽 검사는 그대로 둔다.
- 잠금 줄은 창에 따라 기존 경로를 그대로 쓴다: 창고 창 `Transact("storage-lock:…")`, 캐릭터 화면 `GameStore.SetInventoryLock`. 잠긴 아이템도 이동할 수 있다(기존 규칙). 판매·분해 제외는 기존 `EquipmentShop.SaleExclusion`/`InventorySalvagePlan.Eligible`이 이미 잠금을 검사한다.
- 장착: 놓을 곳이 하나면 바로 `EquipInventoryItem`, 반지·두 손처럼 둘이면 기존 비교 화면을 열어 고르게 한다(기존 `choose` 흐름).
- 상세 정보 → 창고는 `ShowDetail(id)`, 캐릭터 화면은 `ShowDetail(id)`(미착용도 비교가 아닌 상세로 열 수 있게 매개변수 추가). 비교 → 창고는 `ShowDetail(id,true)`, 캐릭터 화면은 기존 비교 화면. 메뉴에서 연 비교는 닫으면 격자로 돌아온다.

### 열기·닫기

- 한 번 누르기 = 메뉴. 일괄 이동·분해 선택 모드는 지금처럼 선택·해제만(메뉴 없음). 끌기는 그대로(움직이면 바로 집힘).
- 닫힘: 메뉴 밖 누르기(그 누름은 소비) · 항목 선택 · 뒤로가기/Esc · 격자 스크롤 · 탭·분류·정렬 변경 · 창 닫기.
- 다시 그려질 때: 거래·저장 알림으로 격자가 다시 그려져도(`Repaint()`가 칸을 새로 만든다) 메뉴는 아이템 id로 칸을 다시 찾아 같은 아이템 옆에 다시 붙고, 아이템이 사라졌으면 닫힌다. 아이템 참조는 거래마다 새 인스턴스로 바뀌므로 id만 쥔다.
- 위치: `ItemMenuPlacement.Compute` — 칸 오른쪽 12px → 왼쪽 → 아래 → 위 순으로 들어가는 첫 곳, 판 안쪽 가장자리 8px 안으로 보정, 화살표는 칸 중심(모서리에서 22px 이상). 터치 행 높이 44px 이상(PC 50).

### 튜토리얼·안내 수업 호환

| 수업 | 의존 | 처리 |
| --- | --- | --- |
| 갑옷 심부름 3단계 | `GameUI.TownErrands.cs:68`이 캐릭터 화면의 `inventory-equip` 버튼을 찾아 가리킴. 안내 막(`PrologueGate`)은 대상이 보이는 동안 그 밖의 입력을 막고, 대상이 약 2초 안 보이면 풀림 | `GetArmor` 심부름이 끝나지 않은 동안 칸 누르기는 기존 큰 창으로 직행(`ItemActionMenu.LessonWaitsForDetail`) |
| 가이드 ‘장비’ 단계 | `ShowDetail`이 `ReviewInventoryItem(id,compare)`로 H01/H14 검토 기록과 `FirstPlayGuide.ReadComparison`을 남김 | 튜토리얼 대상(`Tutorials.Mandatory`)이고 비교 후보가 있으며 단계가 안 끝났을 때만 직행. 이 판정은 **클릭한 아이템별이 아니라 계정 상태별**이라 그동안은 캐릭터 화면의 모든 칸이 직행이다(단계 완료는 미착용·착용 가능 아이템을 읽을 때 기록됨). 끝나면 메뉴 |
| 창고 수업 H04 | 거래 이름(`storage-move`/`place`/`bulk`/`preset`/`register`)으로 진행 | 칸 누르기에 의존하지 않음. 메뉴의 이동도 같은 `MoveStorageItem` 거래 |
| `storage-lock`·`inventory-lock`·`inventory-unequip` | 수업에 매핑되지 않음 | 거래 이름 유지(`SoundBankTests`가 문자열을 확인) |

## 4. 구현 파일

- 새 파일: `Core/ItemActionMenu.cs`(규칙·라벨·수업 판정), `Presentation/ItemMenuPlacement.cs`(위치), `Tests/Editor/ItemActionMenuTests.cs`.
- 화면 코드(승인 뒤): 공용 메뉴 부품 1개(`ItemActionMenuView`, 막이 + 판 + 화살표 + 행, `UiButton`), `StorageWindow`·`InventoryWindow`에서 한 번 누르기 연결.
- 행 버튼 이름: 메뉴 행은 `storage-menu-<action>`·`inventory-menu-<action>`으로 두고, 상세창 푸터의 `inventory-equip`·`inventory-lock`은 그대로 둔다(심부름·스모크가 그 이름을 찾는다).
- 도움말: `StorageWindow.Tools.cs`의 “아이템을 눌러 상세 정보를 열고 …” 문장은 메뉴 설명으로 바꾸고, 영어 항목도 같은 줄을 제자리에서 바꾼다(고아 항목 시험 때문에 옛 키를 남길 수 없다). 이 줄은 파일 중간이라 다른 작업과 겹치면 병합 충돌 가능.
- 문구: 한국어 리터럴마다 `Resources/Localization/en.txt` 항목이 필요하다. 이 작업은 파일 끝에 4줄(상세 정보 · 소지품으로 이동 · 창고로 이동 · 착용 장비와 비교)을 이어 붙였다. 다른 작업과 파일 끝이 겹쳐 통합 충돌이 날 수 있다.

## 5. 검증 계획

- EditMode: `ItemActionMenuTests`(컨텍스트×상태 행 표, 막힌 줄, 라벨 영어 항목, 수업 판정, 위치 계산) + 영향받는 기존 시험(`StorageCasualSkinTests`, `NativeInventoryTests`, 지역화 시험).
- `python3 tools/check_ui_contract.py --verify`, `python3 tools/check_ui_refresh.py`.
- 런타임 스모크(영향 부분): 창고 스모크의 “일반 클릭이 상세를 연다” 단정은 메뉴를 거쳐 상세를 여는 순서로 바꾼다. 캐릭터 화면 스모크(`ItemClick` 뒤 `inventory-lock` 등)도 같은 방식으로 바꾼다. 무거운 Unity 실행은 협업 작업판 `run --resource unity`로 한 번에 하나.
- 정지 캡처(한국어 세로): 창고 칸, 소지품 칸, 화면 가장자리 칸 각 1장.

## 6. 확인한 사실 / 가설 / 결정 / 미실행

- 확인한 사실: 위 표(코드 읽기). 인계본은 로컬 렌더로 PC·세로·가로의 시안 흐름을 확인했다고 기록했다. 이어받은 세션은 공유 Claude UI를 조작하지 않았으며 실제 Unity 화면을 확인했다고 주장하지 않는다.
- 가설: 가이드 ‘장비’ 단계 판정(`Mandatory` + 비교 후보 존재)이 모든 오래된 저장에서 의도대로인지는 실제 저장 파일로 확인하지 않음.
- 확정: 기존 시안 사용, “창고로 이동/소지품으로 이동”, 캐릭터 화면 포함, 상점 판매 목록 제외, 수업 중 직행 유지, 막힌 줄과 사유 표시. 이 묶음에서 다시 승인받지 않는다.
- 미실행: Unity 컴파일·EditMode·스모크·캡처(아직 코드를 실행하지 않음), 실기기.

승인 근거: 2026-10-24 신뢰된 사용자 대화에서 기존 H 시안 사용을 포함한 권장안에 사용자가 **“다 동의하니 작업해줘”**라고 답했다. 상위 통합 담당이 이 정확한 H 범위로 인계했다. 이는 새로 받은 승인이나 이 세션의 Claude UI 확인으로 기록하지 않는다.

## 7. 이어받은 구현

최신 `origin/main 1f1887d1148d99e5b5f42f9a899d8945c6209259`의 독립 `codex/item-action-popup-20261024` 브랜치에서 작성했다. 중단된 `item-action-popup-20261024-0bc535`의 규칙·위치·모델 시험과 .meta, 번역 4개를 복사했으며 원본을 수정하지 않았다. E 작업 브랜치와 root 통합 작업 폴더도 바꾸지 않았다.

- `ItemActionMenuView`는 두 창의 기존 Canvas 위에 메뉴 하나를 그리는 어댑터다. 공통 `UiSkin/StorageSurface/UiButton`을 쓰고 상세·비교는 기존 `ItemDetailView/EquipmentComparisonView`를 연다. 새 상세 렌더러·Canvas·폰트·에셋은 없다. 화면별 규칙은 `ItemActionMenu.Rows` 한 곳에 있다.
- 메뉴는 ID만 보관한다. 행동 직전에 현재 아이템을 다시 얻고 기존 거래를 호출한다. 저장 실패는 현재 설정/디스크를 유지하고 기존 오류 토스트를 쓰며 메뉴는 행동 선택 시 닫힌다. 복수 장착 위치는 기존 비교의 위치 선택을 유지한다.
- 양손 장비처럼 같은 ID가 두 착용 칸에 표시되면 누른 물리적 착용 위치도 UI 상태로 보관한다. 다시 그린 뒤 같은 칸을 우선 찾아 메뉴와 강조가 다른 손으로 이동하지 않게 한다.
- 기존 저장 subscriber와 표시 키를 재사용한다. 인벤토리는 수업의 직행 여부만 추가 표시 의존성으로 갖는다. 의미 있는 저장이 격자를 다시 만들면 같은 메뉴/버튼을 유지한 채 새 칸에 붙인다. 아이템이 없어지거나 현재 탭·필터에 없거나 칸이 마스크에 잘렸으면 닫는다.
- 새 격자의 스크롤 복원 중에는 닫힘을 보류하고 복원된 실제 위치를 기준으로 기록한다. 실제 위치 변화 또는 휠 입력은 닫는다. 무변경 저장은 메뉴/칸을 다시 만들지 않는다. 메뉴에 새 Update나 매 프레임 조회는 없다.
- 세로·가로 기본 폭 176, PC 224이며 긴 비활성 설명이 높이에 맞지 않으면 폭을 늘려 맞춘다. 행은 실제 화면 기준 최소 44px(PC 기본 50), 글자는 줄이지 않는다. 칸은 안전 영역·창·모든 부모 마스크 안에 완전히 보일 때만 기준점이 된다. 위치 계산의 12/8/22px 규칙은 인계본을 유지했다.
- 한 번 누르면 메뉴를 연다. 바깥 누름은 투명 입력 막이에서 소비한다. Esc/Back, 실제 스크롤, 탭·분류·정렬 변경, 드래그 시작, 창 닫기와 아이템 소멸에서 닫힌다. 일괄 이동·분해 모드·드래그 임계값·드롭 후 클릭 억제 시간은 보존했다.
- 갑옷 심부름과 미완료 장비 수업은 계속 기존 상세로 직행한다. `inventory-equip`, `inventory-lock` 이름과 거래 이름을 보존했다. 상세 메뉴는 단일 상세, 비교 메뉴는 기존 비교이며 메뉴에서 연 비교는 닫으면 격자로 돌아간다. 기존 직접 `ShowDetail` 호출의 기본 비교 동작은 유지한다.

## 8. 검증 인계

`ItemActionMenuTests`는 인계된 규칙/배치/수업/번역 검사를 유지한다. `ItemActionMenuWindowTests`는 정상·가득 참·갑옷 보호·반지의 한쪽만 가능한 장착·저장 실패·현재 ID 재획득·동일 버튼 유지·복원/실제 스크롤 구분·아이템 소멸·바깥 클릭 소비·Esc·소유자 종료·선택/드래그·수업 직행을 검사한다. 5개 요구 비율 × 한국어/영어의 메뉴 경계와 글자 맞춤도 검사한다.

기존 `RuntimeInventorySmoke.ItemClick`는 메뉴에서 이전과 같은 상세/비교로 들어가며, `RuntimeStorageSmoke`는 일반 클릭에서 메뉴를 검증·캡처한 뒤 상세를 선택한다. 다른 콘텐츠의 클릭 경로는 바꾸지 않았다. 통합 담당이 관련 fixture, UI 계약/갱신, 실제 macOS 화면·조작·캡처를 최종 후보에서 확인한다.

**이 작업 폴더에서 Unity·Edit Mode·스모크·빌드·실기기·성능 측정을 실행하지 않았다.** Python 정적 검사 근거는 `Builds/Validation/ItemActionPopup20261024/`에 남긴다. 시안과 Unity의 실제 픽셀 정합·실제 조작·성능 개선은 아직 확인한 결과가 아니다.

최종 입력의 `check_ui_contract.py --verify`는 Python 회귀 15개를 통과했고, `check_ui_refresh.py`는 14개 바인딩 보호·240개 UI 소스 점검을 통과했다. `git diff --check`도 통과했다. 인계 원본 3개 코드와 .meta의 일치는 SHA-256으로 확인했으며 14개 모델 사례+25개 창 사례는 **작성 수이며 실행 결과가 아니다**. UI 계약 검증은 입력 수정에 맞춰 4회 실행했고 마지막 근거는 `ui-contract.json/.log`다.

## 통합 후보 집중 검사 갱신 (2026-10-24)

최신 main 1f1887d1을 포함한 후보 5936415e에서 E/H/F와 직접 호출자 23개 fixture를 실행했다. 최초 컴파일 오류는 새 E 테스트의 List.Length를 Count로 고쳤다. 이후 550개 중 521개 통과, H/F 29개는 EditMode EventSystem 가정에서 실패했다. 최종 H/F 40/40 통과와 바뀌지 않은 나머지 510개 근거를 합쳐 단위 범위 550/550을 확인했다. 런타임 코드는 이 테스트 보정으로 바뀌지 않았다. 근거: 로컬 takeover/e-h-f-unit-proof.json, e-h-f-retest/results.xml, h-f-final-unit/results.xml. 로컬 CI 9단계도 통과했다(takeover/integration-ci/report.json; 설치 Python3.9.6/Node26.8.2, 워크플로우3.12/22와 정확 버전 동등성은 미확인).

EditMode에서 새 EventSystem은 전역 current에 등록되지 않으므로 테스트 자체 인스턴스를 사용한다. 런타임 지연 삭제의 알려진 EditMode 엔진 진단만 정확한 메시지로 예상하고 다른 오류는 계속 실패한다. 저장 실패는 별도 특정 메시지를 검증한다. 단위 테스트는 실제 hit-area 크기·필터 판단·이벤트 전달·저장·초안·복귀를 검사한다.

**실제 렌더 프레임의 전역 Canvas 레이캐스트·네이티브 포인터·한/영 캡처는 미완료다.** Metal batch 대조에서도 EditMode 전역 hit가 비어 있던 5개 결과를 h-f-raycast-graphics에 보존했다. 그 실패를 네이티브 통과로 바꾸지 않았으며 실제 프레임 검사는 별도로 수행해야 한다. 임시 회사 식별자와 자체 Unity import 9파일은 해시와 함께 복구 가능 보관하고 HEAD 바이트로 복원했다. 실제 CLI 리뷰·원격 통합은 후속 근거가 필요하다.

## 상세 정보에서 다중 위치 장착 (2026-10-25)

실제 Sol/Max 통합 리뷰에서 단일 상세 카드의 장착이 반지/한손 무기의 위치 선택을 건너뛰는 문제를 발견했다. 상세 정보는 단일 카드를 유지하되, 여러 위치를 지원하는 미착용 장비의 장착 버튼은 기존 비교·위치 선택으로 이어진다. 최종 선택 전에는 착용 장비를 바꾸지 않고, 최종 장착은 기존 거래와 디스크 저장을 사용한다. 두 반지와 양손의 개별 무기에서 새 회귀 2건이 수정 전 실패해 재현됐으며, 수정 후 item-menu-review-fix/green2/results.xml에서 직접 영향 104/104가 통과했다. 여기에는 두 장착 위치 회귀와 실제 디스크 재확인이 포함된다.

개발용 RuntimeUserReportNativeAcceptance는 새 격리 save/evidence 및 disabled 서버를 강제하며 실제 최초 균열 투어와 메뉴 경로를 검사하도록 준비했다. 모든 합성 입력과 마지막 외부 포인터 단계를 구분한다. 현재 Unity 6000.6 컴파일도 통과했다. 새 도구의 폐기된 숫자 객체 ID 호출은 실제 컴파일 실패 기록을 보존하고 컴포넌트 참조 비교로 수정했다. 실제 플레이어 실행·렌더·포인터 검증은 Mac 잠금 해제 뒤 남는다.
