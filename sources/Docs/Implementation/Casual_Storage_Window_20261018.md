# 창고 창 캐주얼 스킨 — 2026-10-18

[English](Casual_Storage_Window_20261018.en.md)

기준: 캔버스 "HELLSCRIPT 창고 · 캐주얼"(BVaVczJFgajuJxeHfEw1R8)의 구성 명세 k절(게임 연결)과 f절(부품 레시피). 시작 `origin/main` `19072415`. 창고 이동·끌기·일괄 이동·탭 개방·칸 확장 거래와 이름(`storage-*`, `grade-*`, `order-*`, `category-*`, `bulk-*`, `preset-*`)은 바꾸지 않았다.

## 적용

- `StorageWindow.Style.cs`가 모든 면을 소유한다. `Surface(r, 이름)`을 캐주얼 `StorageSurface.SetSkin`으로 바꿨다. 판 `panel-base`(남색 3px·모서리 18/24·아래 그림자), 패널 `storage-panel`(흰색·카드 테두리), 레일 `transfer-rail`(크림), 우물 `input-well`, 빈 칸 `slot-empty`, 초과 안내 `notice-error`(코랄), 대화상자·팝오버·토스트 `dialog-panel`이다. 글자는 `UiSkin.LabelFont`(Jua, 없는 글자는 Noto)와 `BodyFont`, 색은 `UiSkin` 토큰(Navy·Muted·OrangeInk·GreenInk·WarningInk·SkyInk)이다.
- 창은 안전 영역을 꽉 채우지 않고 8~12 띄운 둥근 판으로 그린다. 뒤는 Navy α.45 베일이다. 머리줄은 브랜드·상자 젬 + 금빛 제목(`InventoryWindow`와 같은 제목 레시피)·코랄 둥근 닫기이며, 전투 중 열면 노란 일시정지 알약이 붙는다(세로 화면은 머리줄 아래 한 줄).
- 모든 `UiButton`은 `UiSkin.Casual.ApplyChrome`(확정은 초록 + 흰 글자 윤곽선, 닫기는 `CasualClose`)이다. 창고 탭은 흰 판·선택 노랑 + 아래 마름모 꼬리, 구매 가능은 초록 +, 잠김은 회색 자물쇠, 초과는 코랄 "!"이다(`StorageChestOverlay`).
- 아이템 칸은 `EquipmentSlotView.Paint(…, UiSkin.Casual.WithEquipmentArt(CasualEquipmentArt.Provider))`로 승인된 캐주얼 장비 그림과 등급 고리를 쓴다. 아이템 상세·장착 비교·프리셋 장착 부위도 같은 `skin`을 받는다.
- 토스트·탭 안내·끌기 안내·등급 팝오버·정렬 메뉴·이름 변경·프리셋·안내·기록·탭 개방·칸 확장 대화상자가 같은 면과 글자 규칙을 쓴다.

## 시안과 다른 점 (결정 필요)

- 레일은 점선(2px) 대신 실선 카드 테두리다. `StorageSurface`의 점선은 1px 전용이라 공통 부품을 바꾸지 않았다. 초과·확장 칸 점선도 같은 이유로 실선이다.
- 사용량 알약, 분류 줄 밑줄 색 띠, 탭 숫자 위치, 아이템 칸 위 레벨 윤곽 등 시안의 세부 장식 일부는 적용하지 않았다.
- 치수는 게임 단위를 유지하고 시안 비율만 반영했다(판 간격 8/12, 헤더 36/44, 레일 86/54, 모서리). 시안의 PC 1600×900 보드 픽셀을 그대로 옮기지 않았다.
- 등급 고리는 시안의 밝은 색(#8a93b5 #35a7ff #ffc21a #ff9f1c #4cbf55)을 이 창에 적용했다. 인벤토리의 `EquipmentGradePalette` 고리와 하나로 합칠지 결정이 필요하다.

## 검증

- 집중 EditMode 89개 통과: `StorageCasualSkinTests`(신규 6) · `UiButtonTests` · `StorageTests` · `EquipmentStorageTests`. 신규 테스트는 가로·세로에서 판·패널·레일·탭의 위치, 모든 글자·단추의 캐주얼 글꼴·색·크롬, 대화상자·토스트·팝오버·일괄·초과 상태를 확인한다.
- 정적 UI 계약 15개와 갱신 14개 바인딩 통과.
- Editor에서 실제 창을 PNG로 그려 PC·가로·세로, 일괄 이동, 등급 팝오버, 토스트, 안내, 탭 개방, 칸 확장, 아이템 상세, 보석 묶음을 눈으로 확인했다(`Assets/HELLSCRIPT/Editor/StorageCasualEvidence.cs`, 해상도 640×480).
- 미검증: 개발 플레이어 `RuntimeStorageSmoke`(끌기·자동 스크롤·거래 연동)와 macOS 빌드, 실기기·터치, 영어 긴 문구 잘림. 전체 EditMode는 실행하지 않았다.
