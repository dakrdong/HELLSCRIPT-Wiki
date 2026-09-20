# 마을 HUD 크기·투명도·표시 정리

작성일: 2026-09-20

[English](Town_Hud_Responsive.en.md)

## 적용한 동작

첨부 화면의 여섯 가지 요청을 기존 마을 UI와 전역 HUD에 적용했다.

| 항목 | 현재 동작 |
|---|---|
| 창고 바로가기 | 첨부한 상자 PNG로 표시하며 기존 계정 창고를 연다. 마을과 전투가 같은 아이콘을 쓴다. |
| 이동 패드 | 안전 영역의 짧은 변 길이의 20%를 지름으로 사용한다. 원형과 손잡이 비율을 유지하며 왼쪽 HUD 위에 둔다. |
| 패드 투명도 | 처음과 사용 종료 뒤에는 불투명도 30%, 누르는 동안에는 100%다. 두 번째 손가락이 소유권을 가져가지 못한다. 해상도 변경, 비활성화, 포커스 상실과 설정 진입 시에는 입력을 해제한다. |
| 물약 배치 | 가로·세로 화면 모두 가장 위에 있는 스킬 행보다 위에 둔다. 좁은 화면에서 스킬이 여러 줄로 나뉘어도 같은 기준을 쓴다. |
| 마을 안내 | 직업·레벨·횡단 시간 문구, 시설 안내와 메뉴 버튼을 숨긴다. |
| NPC 이름 | 검은 외곽선을 둔 이름만 표시한다. 말풍선 배경과 꼬리, 가까이 갔을 때 이름에 붙던 행동 문구는 표시하지 않는다. 실제 상호작용 카드는 유지한다. |
| 상단 영역 | 안내 문구·배경·구분선을 없애고 제목을 30에서 18로 줄였다. 영역 높이는 80에서 44, 톱니바퀴 그림은 20이다. 설정의 터치 영역은 44×44다. |

## 소유 구조와 이미지 출처

기존 `GameUI.Plaza`가 마을 화면을, `TownJoystick`이 포인터 소유권·입력·투명도를, `GlobalHudLayout`이 물약 위치를 담당한다. 화면 픽셀 크기를 구한 뒤 캔버스 좌표로 한 번만 변환한다. 게임 저장 형식·패키지·씬·프리팹은 바꾸지 않았다.

`menu-storage.png`는 사용자가 첨부한 1254×1254 RGBA PNG의 바이트를 그대로 복사했다. 새 이미지 생성, 리사이즈, 배경 제거를 하지 않았다. 실제 완전 투명 픽셀 611,509개와 원본 해시를 [출처 기록](TownHudEvidence/provenance.json)에 보존했다. 기존 `GlobalHudImporter`의 스프라이트·무압축·알파 보존 설정을 사용한다.

![창고 바로가기 원본](../../Assets/HELLSCRIPT/Resources/Art/GlobalHUD/menu-storage.png)

## 검증

Unity 6000.6.0f1의 관련 Edit Mode 검사 56개가 모두 통과했다. 범위는 `GlobalHudTests`, `GlobalHudResourceTests`, `TownWalkTests`, `InterfaceScaleTests`이며 프로젝트 전체 회귀 검사는 아니다. [검사 결과](TownHudEvidence/editmode-results.xml)를 보존한다.

macOS 개발 빌드는 오류 0개로 성공했다. 1600×900, 900×1600, 844×390, 390×844, 640×360, 360×640에서 글자 크기 50·100·150%를 조합한 18개 경우를 검증했다. 안전 영역과 한국어·영어, 실제 캐릭터 이동·정지, 누름·해제 시 투명도, 두 번째 포인터 간섭 방지, 창고 열기와 설정 진입도 통과했다. NPC 이름의 표시 범위도 현재 화면과 축소한 상단 영역에 맞췄다.

[실행 결과](TownHudEvidence/runtime.txt) · [실측 배치](TownHudEvidence/geometry.txt) · [검증 요약](TownHudEvidence/validation.json) · [빌드 결과](TownHudEvidence/build.txt)

입력 검증은 macOS 실행본에 전달한 EventSystem 포인터 이벤트이며 실제 iOS·Android 기기 터치 검증은 아니다. 실행 로그에 일부 URP 후처리 셰이더 제거 경고가 있었으며 이번 UI 작업에서 렌더 파이프라인은 변경하지 않았다.

## 실행 화면

![가로 화면과 대기 중 이동 패드](TownHudEvidence/1600x900-100.png)

![세로 화면의 물약 상단 배치](TownHudEvidence/900x1600-100.png)

![조작 중 불투명한 이동 패드](TownHudEvidence/landscape-held.png)

![첨부 아이콘을 사용하는 창고 바로가기](TownHudEvidence/storage-shortcut.png)

![작은 가로 화면의 NPC 이름](TownHudEvidence/844x390-100.png)
