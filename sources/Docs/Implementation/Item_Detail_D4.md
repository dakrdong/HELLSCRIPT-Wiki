# 공통 아이템 상세 Diablo IV 재구성

갱신일: 2026-10-05 · [English](Item_Detail_D4.en.md) · 규칙: [장비 비교 공통 규칙](../Design/Equipment_Comparison_Rules.md)

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

## 장식 아트와 연출 (2026-10-05)

"UI 디자인도 미감을 최대한 발휘해 더 쥬시하게, 필요하면 코덱스에게 이미지를 요청해도 좋다"는 요청에 따라 카드 장식을 한 번 더 키웠다. 규칙은 [장비 비교 공통 규칙](../Design/Equipment_Comparison_Rules.md)의 `2026-10-05 장식 아트와 연출`, 아트의 생성·가져오기 기록은 [아이템 상세 카드 장식 아트](../Art/ItemCard/Item_Card_Art.md)에 있다.

| 변경 | 위치 |
| --- | --- |
| Codex 이미지 5종(모서리·구분선·점수 명판·소켓 고리·상위 옵션 별)과 등급 후광·빛줄기·페이드 곡선, 등장 연출 `ItemCardIntro` | `ItemCardArt.cs`, `Resources/Art/ItemCard/` |
| 등급 색 틀, 점수 명판과 ▲▼, 큰 구분선, 후광, 별·소켓 고리, 비교 줄 바탕, 특수 효과 테두리, 스크롤 안내 띠 | `ItemDetailView.Style.cs`, `ItemDetailView.cs` |
| 상세 창 제목 줄 장식 | `ItemDetailPopup.cs` |
| 임포터·플랫폼 예산(`Art/ItemCard/`) | `ItemCardArtImporter.cs`, `ResourceTextureBudget.cs` |
| 높이에 따른 밀도 조절: 420 미만은 낮은 명판·벡터 선, 300 미만은 후광·명판 생략 | `ItemDetailView.Style.cs` |

### 검증 (2026-10-05)

- EditMode 집중 검사 110개 통과: 새 `ItemCardArtTests` 6개(아트 로드와 자르기 범위, 네 모서리·명판·후광, 조밀 카드의 벡터 유지, 상위 옵션 별, 등장 연출 단계·빛줄기, 비교 화살표·줄 바탕)와 `ItemComparisonTests`, `EquipmentArtTests`, `ItemArtCoverageTests`, `LocalizationTests`, `CurrencyArtTests`, `UiButtonTests`.
- `check_ui_contract.py`, `test_ui_contract.py`, `check_ui_refresh.py` 통과.
- macOS 개발 빌드 스모크 통과: `-hellscriptInventorySmoke`(가로·세로 한국어·영어; 세로 비교 탭 실제 클릭 추가), `-hellscriptItemPolishSmoke`(440×956, 956×440, 1440×810, 1440×900, 1680×720 × 한국어·영어, 창고·상점·대장간 상세, 드래그), `-hellscriptItemRangesSmoke`, `-hellscriptBlacksmithSmoke`, `-hellscriptItemDetailPopupSmoke`. 등장 연출은 `-hellscriptUiMotion`으로 1/5 속도 프레임을 찍어 확인했다.
- 스모크 정리: 세로 비교가 탭이 되면서 `-hellscriptInventorySmoke`가 두 카드가 동시에 있다고 가정해 실패했다. 이 스모크는 변경 전 `main`(53502b35)에서는 통과하므로 `ComparisonPanes`·`CheckEquippedCard`로 탭을 인식하게 고치고 실제 탭 클릭을 추가했다. 카드 레이아웃(조밀·넓은)에 따라 주요 수치 글자 구성이 달라지므로 도구 설명 서명은 줄 이름 기준으로 비교한다(글자 값은 `CheckComparison`이 줄마다 확인한다).
- 변경 전 `main`(53502b35)에서도 실패하는 스모크: `-hellscriptEquipmentShopSmoke`, `-hellscriptStorageSmoke`, `-hellscriptAspectStoneSmoke`. 이번 변경과 무관하다. `-hellscriptRiftVictorySmoke`, `RewardBox`, `RecommendedEquipment`, `GemSocket`은 증거 경로 인자가 필요해 돌리지 않았다.
- 화면: [세로 상세 한국어](ItemDetailD4Evidence/juicy-detail-portrait-ko.png) · [세로 상세 영어(전설)](ItemDetailD4Evidence/juicy-detail-portrait-en.png) · [가로 기본 비교](ItemDetailD4Evidence/juicy-default-compare-landscape-ko.png) · [PC 기본 비교(전설)](ItemDetailD4Evidence/juicy-default-compare-pc-en.png) · [세로 반지 비교](ItemDetailD4Evidence/juicy-ring-compare-portrait-ko.png) · [PC 반지 비교](ItemDetailD4Evidence/juicy-ring-compare-pc-ko.png) · [세로 장착 중 탭](ItemDetailD4Evidence/juicy-equipped-tab-portrait-en.png) · [상점](ItemDetailD4Evidence/juicy-shop-landscape-en.png) · [창고](ItemDetailD4Evidence/juicy-storage-landscape-en.png) · [대장간](ItemDetailD4Evidence/juicy-forge-landscape-en.png) · [등장 연출 5프레임](ItemDetailD4Evidence/juicy-motion-sheet.png) · [스모크 결과](ItemDetailD4Evidence/popup-result.txt)

### 확인하지 않은 것

실기기에서 보지 않았고 입력은 합성 macOS 입력이다. Android·WebGL 빌드와 텍스처 용량은 실측하지 않았다(예산 행만 추가). 등장 연출은 약 0.8초 동안만 프레임마다 도는 일회성이며 끝나면 스스로 제거되지만 프레임 비용은 실측하지 않았다. 마지막 코드 변경 뒤 전체 EditMode 검사 결과는 아래에 적는다.

### 전체 EditMode 검사 (2026-10-05)

마지막 코드 변경 뒤(커밋 `ea4b6317`, 이때 `main`과 같은 기준) 전체 EditMode 검사를 한 번 실행했다: 5,165개 중 5,165개 통과, 실패·건너뜀 0, 1,877초. 런타임 스모크는 위에 적은 항목만 실행했으며 전체 스모크 묶음은 실행하지 않았다.
