# 공통 아이템 상세 Diablo IV 재구성

갱신일: 2026-10-04 · [English](Item_Detail_D4.en.md) · 규칙: [장비 비교 공통 규칙](../Design/Equipment_Comparison_Rules.md)

`ItemDetailView`·`ItemDetailPopup`·`EquipmentComparisonView`의 표시를 Diablo IV 정보 위계로 다시 만들었다. 계산·거래·저장은 바꾸지 않았다.

| 변경 | 위치 |
| --- | --- |
| 주요 수치·이름·차이 한 줄, 가지선 고정 속성, 상위 옵션 강조, 소켓 고리, 하단 요구 조건, 두 열 카드, 아래로 스크롤 안내 | `ItemDetailView.Style.cs`, `ItemDetailView.cs` |
| 가로 상세 창 폭 640, 하단 버튼 한 줄 최대 4개 | `InventoryWindow.Dialogs.cs`, `ItemDetailPopup.cs` |
| 세로 비교 탭(`선택한 장비 / 장착 중`) | `EquipmentComparisonView.cs` |

## 검증 (2026-10-04)

- EditMode 집중 검사 82개 통과: `ItemComparisonTests`, `EquipmentArtTests`, `ItemArtCoverageTests`, `LocalizationTests`. 단일 장착 카드가 전체 폭에서 주요 수치 이름을 분리 표시하므로 조밀 카드 검사를 폭 300 미만 카드로 한정했다.
- `check_ui_contract.py`, `test_ui_contract.py` 통과.
- macOS 개발 빌드 `-hellscriptItemDetailPopupSmoke` 통과(`HELLSCRIPT_ITEM_POPUP_RUNTIME_OK`): 440×956, 956×440, 1440×810, 1440×900, 1680×720 × 한국어·영어, 기본 글자 크기. [결과](ItemDetailD4Evidence/popup-result.txt)
- 화면: [가로 상세](ItemDetailD4Evidence/detail-956x440-ko.png) · [세로 상세](ItemDetailD4Evidence/detail-440x956-ko.png) · [영어 가로](ItemDetailD4Evidence/detail-956x440-en.png) · [세로 비교 탭](ItemDetailD4Evidence/comparison-440x956-ko.png) · [가로 비교](ItemDetailD4Evidence/comparison-956x440-en.png) · [PC 비교](ItemDetailD4Evidence/comparison-1440x810-ko.png)

실기기에서는 확인하지 않았다. 합성 macOS 입력이다. 전체 EditMode·스모크 묶음은 이번 단계에서 다시 돌리지 않았다.

## 가로 기본 비교 (2026-10-04)

`InventoryWindow.ShowDetail`이 가로에서 같은 부위 장착 장비가 있는 미장착 아이템을 `EquipmentComparisonView`로 연다. 스모크 `-hellscriptItemDetailPopupSmoke` 5개 해상도 × 한국어·영어 통과(`score-*` 점수 검사는 후보 카드 기준으로 조정). [화면](ItemDetailD4Evidence/default-compare-956x440-ko.png)
