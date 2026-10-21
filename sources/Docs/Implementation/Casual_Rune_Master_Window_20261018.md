# 룬 마스터 창 캐주얼 스킨 — 2026-10-18

[English](Casual_Rune_Master_Window_20261018.en.md)

기준: 캔버스 "HELLSCRIPT 룬 장인 · 캐주얼"(AefHsWjXq1UDFzdsNzmdSW)의 구성 명세 k절(게임 연결)과 f절(부품 레시피). 시작 `origin/main` `3d3e0f4f`. 룬 재형성·승급 규칙과 거래(`RuneReshape`, `RuneAscension`, `GameStore.ReshapeRunes/AscendRunes/ClaimRuneAscensions`), 끌기 판정, 연출 시간, 이름(`rune-master-*`), `PlayerPrefs` 키는 바꾸지 않았다.

## 적용

- `RuneMasterWindow.Controls.cs`가 모든 면·글자·단추를 소유한다. `Panel(…, 이름)`이 캐주얼 `StorageSurface.SetSkin`을 쓴다: 판 `plate`(남색 3px·모서리 18/24·그림자), 카드 `base`(흰색·카드 테두리), 선택 칸 `active`(남색 3px), 보관 칸 `slot`, 빈 칸 `empty`, 스크롤바 `track`/`handle`, 뒤 막 `veil`(Navy α.45). 글자는 `UiSkin.LabelFont`(Jua)·`BodyFont`, 색은 `RuneBoardSkin.Shared`(= `UiSkin.Casual`) 토큰이다.
- 창은 안전 영역을 꽉 채우지 않고 8~12 띄운 둥근 판이다. 머리줄은 브랜드 + 룬 보드 젬 + 금빛 제목 `룬 마스터`(`InventoryWindow`와 같은 제목 레시피) + 코랄 둥근 닫기, 아래 2px 구분선이다.
- 모든 `UiButton`은 `RuneBoardSkin.Button`/`ApplyChrome`이다. 합성·한번에 조합하기는 초록 + 흰 글자 윤곽선, 비활성은 회색 + 흐린 글자(`Enable`)이다. 탭은 선택 노랑 + 아래 마름모 꼬리, 등급 단추는 선택 노랑이다. 연출 제거는 켜면 노랑 칩이다.
- 재료 칸은 선택 시 남색 링 + 노랑 그림자 배지 G칩, 끌어 올린 대상은 연노랑(`SlotLook`)이다. + 는 노란 원, 결과는 초록 테두리 카드, 오류는 코랄 안내 상자, 완료는 노랑 안내 상자(`NoticeBox`)이다. 보관 칸의 선택 ✓는 노란 원, NEW는 초록 알약, 보호는 잠금 아이콘이다.

## 시안과 다른 점 (결정 필요)

- 룬 그림은 지금 게임의 `RuneBoardGraphic` 아이콘 + 타입 엠블럼을 그대로 쓴다. 시안은 칸마다 casual 보석 타일을 깔았다. 룬 보드 창과 하나로 통일할지 결정이 필요하다.
- 빈 재료 칸 점선은 실선이다(`StorageSurface` 점선은 1px 전용이라 공통 부품을 바꾸지 않았다).
- 승급 공개 연출(카드 뒤집기)의 면 색 번쩍임은 카드 면이 `UiButton` 얼굴이라 보이지 않는다. 뒤집기 움직임 자체(scaleX)는 그대로다.
- 치수는 게임 단위를 유지하고 시안 비율만 반영했다(판 간격 8/12, 헤더 36/44, 모서리). 시안의 PC 1600×900 픽셀을 그대로 옮기지 않았다.
- 새 문구가 없다. 시안이 짚은 누락 키("✓ 연출 제거", "NEW")는 그대로다.

## 검증

- 집중 EditMode 60개 통과: `RuneMasterCasualSkinTests`(신규 8) · `RuneAscensionTests` · `RuneEconomyTests` · `UiButtonTests`. 신규 테스트는 가로·세로·두 탭에서 판·카드·탭 위치, 모든 글자·단추의 캐주얼 글꼴·색·크롬, 짝 선택·오류·승급 선택 상태를 확인한다.
- 정적 UI 계약 15개와 갱신 14개 바인딩 통과.
- Editor에서 실제 창을 PNG로 그려 PC·가로·세로의 재형성, 짝 선택, 오류, 승급(빈·선택)을 눈으로 확인했다(`Assets/HELLSCRIPT/Editor/RuneMasterCasualEvidence.cs`, 해상도 640×480).
- 미검증: 개발 플레이어 `RuntimeRuneMasterSmoke`(끌기·합성 연출·거래)와 macOS 빌드, 실기기·터치, 영어 긴 문구 잘림. 합성·승급 연출 모습은 정지 화면만 확인했다. 전체 EditMode는 실행하지 않았다.
