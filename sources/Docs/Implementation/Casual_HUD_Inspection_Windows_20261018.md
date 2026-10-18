# 전투 HUD 정보 창 캐주얼 적용 — 2026-10-18

[English](Casual_HUD_Inspection_Windows_20261018.en.md)

기준: 캔버스 "HELLSCRIPT 전투 관찰 메뉴 · 캐주얼"(FYfemHtqRJFH7wdCg1rsT5)의 구성 명세. 시작 `origin/main` `3d3e0f4f`. 이 창들은 모두 `GameUI.GlobalHud.ShowHudPanel` 하나가 그리고, 그 안에 `skin != null`일 때 쓰는 캐주얼 분기가 이미 있었다(스킬 팝업만 사용). 이번에는 그 분기를 모든 창에 적용했다.

## 적용

- `ShowHudPanel`은 호출부가 `skin`을 넘기지 않아도 `UiSkin.Casual`을 쓴다. 관찰 메뉴(균열·훈련장·절전 엿보기), 상태 효과 정보, 슬롯 정보(스킬·보석 물약), 현재 상태 목록, 물약 팝업, 훈련장 일시정지 창이 같은 크림 카드(`UiWindowFrame.SetSkin`), 남색 글자, 흰/초록 단추를 쓴다.
- 닫기 단추는 `Item` 역할로 만들어져 `UiButtonFace`가 평소에는 면을 그리지 않았다. 흰 ×가 크림 판 위에서 보이지 않던 문제다. 캐주얼 분기에서 `Quiet` 역할로 바꿔 코랄 `CasualClose` 면이 보이게 했다(스킬 팝업에도 적용된다).
- 정보 창의 스킬 아이콘 줄은 `SkillIconView.SetCasualHudFrame`의 크림 판·금색 고리를 쓴다.
- 훈련장 일시정지 창이 따로 하던 캐주얼 면·단추 처리(`Training pause skin`)는 공통 분기와 겹쳐서 지웠다.

## 바꾸지 않은 것

- 창 열림·닫힘, 일시정지 연동, 각 단추의 동작, 이름(`hud-details-*`, `관찰 메뉴`), 거래는 그대로다. 배경 어둠막은 시안대로 기존 값이다.
- 시안이 코드를 읽고 지적한 별도 문제는 이번 범위가 아니다: `ShowHudEffects`(현재 상태 목록)를 부르는 곳이 없어 보인다, 1.5배·2배속 단추가 켜진 것처럼 보이지만 1배만 허용된다, 상태 창에 `MONSTER_POISON` 같은 내부 id가 그대로 보인다, 확인창이 HUD 아래 캔버스(100)에 있어 HUD(105)가 어두워지지 않는다. 게임 규칙·표시 문구를 바꿔야 하므로 따로 결정한다.
- 휴대전화 배율(시안 0.72)은 적용하지 않았다. 지금은 가로 0.489·세로 0.543이라 글자가 작다. 공통 `ApplyScaler`를 바꿔야 해서 결정이 필요하다.

## 검증

- 집중 EditMode 통과: `HudInspectionCasualTests`(신규 3) · `CasualHudSharedTests` · `CasualBattleHudTests` · `PotionQuickCasualSkinTests` · `CasualUiSkinTests`. 신규 테스트는 호출부가 스킨을 넘기든 말든 카드 프레임·닫기 단추 역할·모든 글자와 단추의 캐주얼 글꼴·색·크롬을 확인한다.
- 정적 UI 계약 15개와 갱신 14개 바인딩 통과.
- 미검증: 실제 전투 화면에서의 모양(Editor 캡처·macOS 빌드 미실행), 실기기·터치, 영어 긴 문구. 전체 EditMode는 실행하지 않았다.
