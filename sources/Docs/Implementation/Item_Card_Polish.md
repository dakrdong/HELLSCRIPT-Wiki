# 아이템 정보창과 드래그 장착 안내

작성일: 2026-09-22 · [English](Item_Card_Polish.en.md)

## 변경 내용

공통 `ItemDetailView`에 등급색 모서리 장식, 어두운 카드 바탕, 구분선, 주요 수치 강조, 옵션 표식과 특수 효과 패널을 적용했다. 기존의 이름·수치·범위·소켓·성장·잠금·장착·교체 차이와 사라지는 속성을 유지한다. 상세와 비교의 바깥 크기는 고정하며 긴 내용은 내부에서 스크롤한다.

인벤토리에서 장비를 끌면 장착 가능한 캐릭터 슬롯의 테두리가 밝아진다. 양손 무기는 두 칸을 표시하고 어느 쪽에 놓아도 같은 주무기 인스턴스로 장착한다. 반지 두 칸, 전사 쌍수, 마법사·궁수와 보조무기 제약, 레벨·소지품 여유 조건은 `EquipmentSlots.PlanDrop`과 기존 거래 경로로 검증한다. 취소·완료·포커스 해제·화면 재배치 때 강조를 지운다.

자동 선택 설정에서 비활성 고유 항목을 제거했다. 현재 등급은 일반·마법·희귀·전설·세트이며, 전설 특수 효과를 뜻하는 `UniqueItemDefinition`과 별도 등급을 혼동하지 않는다. 기존 저장값의 예약 비트 4와 세트 비트 5는 유지한다. 가방 하단의 중복된 자동 선택 설정 버튼을 제거하고 일괄 분해 내부 진입점은 유지한다.

표시 계약과 참고 자료는 [장비 비교 공통 규칙](../Design/Equipment_Comparison_Rules.md)에 반영했다.

## 검증

- Unity 6000.6.0f1 Edit Mode: **108개 통과, 실패 0, 건너뜀 0**. 장착·드래그 후보 판정, 저장 호환성, 비교, 공통 UI, 번역 검사를 실행했다. [검사 원문](../../Artifacts/Validation/item-card-polish/editmode.xml)
- macOS 개발 빌드 성공. 격리된 저장 폴더에서 실제 uGUI 이벤트·레이캐스트·거래를 사용해 검증했다. 세로 440×956, 가로 956×440, PC 1440×810·1440×900·1680×720에서 한국어·영어, 글자 크기 100%·150%를 확인하고 41장을 캡처했다. [실행 결과](../../Artifacts/Validation/item-card-polish/runtime-result.txt)
- 상세·세 반지 비교·일괄 분해 정보·창고·상점·대장간에서 공통 카드와 내용 보존을 확인했다. 별도 Ctrl 검사에서는 인벤토리·창고·상점·획득 보상의 상세·비교, 왼쪽/오른쪽 Ctrl, 설정 저장·재로딩, 행 위치·스크롤 유지까지 통과했다. [범위 표시 결과](../../Artifacts/Validation/item-card-polish/ranges-result.txt)
- 공통 UI 소유 검사와 규칙 회귀 9개 통과. 위키 생성·검사, Python 검사 10개와 JavaScript 화면 경로 검사를 실행했다.
- 열린 사용자 에디터의 저장 데이터와 무관한 macOS 테스트다. 모바일 비율을 macOS 창으로 재현했으며 **모바일 실기기 터치·성능 검증은 수행하지 않았다**.

## 대표 화면

![세로 아이템 정보창](../../Artifacts/Validation/item-card-polish/detail-440x956-ko.png)

![가로 반지 비교](../../Artifacts/Validation/item-card-polish/comparison-956x440-ko.png)

![반지 드래그 하이라이트](../../Artifacts/Validation/item-card-polish/drag-ring-portrait.png)

![양손 무기 드래그 하이라이트](../../Artifacts/Validation/item-card-polish/drag-two-hand-portrait.png)

![다섯 등급 자동 선택 설정](../../Artifacts/Validation/item-card-polish/salvage-settings-five-grades.png)

![대장간 공통 정보창](../../Artifacts/Validation/item-card-polish/shared-forge.png)
