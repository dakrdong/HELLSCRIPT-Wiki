# 공통 확인창·알림 — 2026-10-17

④ 실제 20대지 인계와 전문 `design-4-spec-ko-original.txt`(12,883자·FNV1a32 b6b0c45a)를 기준으로 표시를 구현했다. 소스 기준은 `dbec4dbe`, 초기 공통 API는 `559abd4f`다. 전문 c/f/l의 크기·폰트·배치·등장 효과가 정본이며 요약과 달랐던 수치를 보완했다.

## 구현과 기존 동작

- `GameUI.Confirm(string,Action)`은 `ExitIdle` 후 공통 `ContentWindowView.CreateCanvas`로 전체 화면 Navy `.6` scrim을 연다. ContentWindowHost의 400+ 정렬·pause lease·입력·Back/Esc를 사용한다. 배경은 버튼이 아니며 클릭해도 닫히지 않는다. 기존 `Confirm`/`Dialog`/`취소`/`확인` 이름을 유지한다. 취소는 닫기만, 확인은 `Detach → 비활성 → Destroy`를 요청한 뒤 기존 action을 호출한다. 페이지 변경 때 별도 Canvas도 명시 정리한다. HuntEdict 자체 Dialog/ConfirmEscape는 바꾸지 않는다.
- 카드 폭/최소 높이는 PC 600/300, 가로 520/210, 세로 370/290px이다. 실제 safe area에 맞춰 폭을 제한하고 Noto700 본문 21/15/17px·lineSpacing1.55를 고정한 뒤 preferredHeight로 높이를 늘린다. 원래 개행과 한/영 단어를 보존하고 한 단어가 폭보다 길 때만 grapheme 경계에서 나눈다. 스크롤이나 글자 축소로 대체하지 않는다. 본문 자체가 안전 화면보다 긴 임의 입력 전체를 표시한다고 보장하지 않으며 실제 호출 문구와 긴 시안은 실행 검수 대상이다.
- 공유 StorageSurface/UiButtonFace/Panel·DashedRing·Glow·Radial과 기존 pooled text-shadow를 재사용한다. 카드 선/단단한 그림자는 3/5px(가로2/4), 안쪽 여백은 PC30/28/26·가로18/22/16·세로26/20/20, 본문↔단추 간격16/12/12다. 점선·radial gloss·18px 원형 금 젬(가로 제외)·soft shadow를 적용했다. Action 흰/크림·Primary 초록, 버튼60/44/52px·Jua22/17/19px, Navy 선과 흰 Primary 외곽선/2px drop을 화면별로 적용한다. `WithInteractionStates`는 시안 hover·3px 눌림·그림자 규칙을 해당 버튼에만 적용한다.
- 토스트는 전체 화면 shell의 기존 Overlay3000/Notice다. Notice는 clear Image와 서로 다른 자식의 shadow/surface이며 모든 그림과 CanvasGroup이 입력을 받지 않는다. White→CreamShade, 기존 `Art/Options/Casual/notice-info`, compact pill을 사용한다. 새 이미지 생성은 없다.
- ShowToast의 Loc.T·덮어쓰기·seconds<=2 compact·절전 요약·전투 wide 숨김/튜토리얼/명시 오류 예외·unscaled 만료를 유지한다. wide 폭은 PC min(90%,760), 휴대기기90%, 최소 높이64/44/48px다. compact는 글자+32·최대90%, 높이56/44/44px다. PC/가로는 기존325/112 페이지단위 위치를 실제 배율·부모 safe 원점으로 보존하고 세로 전투/광장은 실제 HUD의 occupiedHeight×scale+BottomInset 위12px에 놓는다. HUD 예약을 두 번 더하지 않으며 짧은 가로의 상단 safe edge도 제한한다.
- `.28초` unscaled 등장은 아래10px에서 원래 위치로 이동하며 alpha0→1이다. 크기는 바꾸지 않는다. 회전은 본문/위치/기준 scale만 다시 계산하며 만료·등장 시계를 재시작하지 않는다. 같은 해상도 페이지 전환에서도 CanvasScaler를 먼저 확정한 후 토스트 배율을 보정한다.

## 통합 보완

광장 이벤트 배지는 전문의 칸×.26·글자×.2, Navy2 ring/GreenShade/흰 Jua+1px outline을 적용한다. 선 굵기는 실제 Canvas 배율로 변환하고 ready 계산·튜토리얼 숨김·클릭 경로는 유지한다. `UiWindowFrame.SetSkin`에 선택 borderWidth/shadow를 추가해 광장 축소 카드의 local3/4를 지키고 기존 기본값은 유지한다.

사냥 칙령 선택 상단 `#ffe26b`, 위험/닫기 상단 `#ff8b8e`는 `WithButtonTops` immutable scope로 해당 창에만 적용했다. 기본 Casual 색·다른 창·Disabled/Locked는 유지한다. `WithEquipmentArt`도 scope를 보존한다. 원문 c/f/l의 치수·장식은 필수로 정정되어 ⑤ 담당이 보완 중이다. 닫기의 명시 shape 인자도 지원하되 미지정 기본은 보존한다.

## 검증과 남은 일

집중 회귀 소스: `CasualNoticeLayoutTests` 14case, `CasualSharedPresentationTests` 11개. 세 화면, 원문 metrics, safe inset, 긴 문장, 실제 HUD 경계·방향별 폭, KO/EN 개행·grapheme, 같은 해상도 배율 전환, 등장 재배치, 완료 입력차단·scope 격리·실제 frame mesh를 다룬다. 초기 `validation/integrated-source/` Runtime858/Editor57/Tests271 타입 컴파일 PASS는 **전문 보완 전의 과거 근거**다. 최신 소스의 최종 타입 컴파일는 ①/⑤ 전문 후속 인수 후 한 번 수행하도록 대기 중이며 실제 NUnit/Unity 통과를 뜻하지 않는다.

Unity·NUnit 실행·네이티브 빌드·원본 main 병합은 미실행이다. 실제20대지의 KO/EN·PC16:9/16:10/21:9·440×956/956×440·긴 문구·배율·배경 입력차단·확인 action 순서·중첩 pause/Back·toast 예외를 부모의 전체 화면 완료 신호 후 단일 통합 묶음으로 검수한다. 사용자 Editor·세이브·모델 원본·미커밋4파일은 건드리지 않는다. ②는 최초38대지 요약은 확보됐으나 전문 대비 완전성 미확인, 최신 사용자 지시로 ③창고/⑥룬 장인은 이번 묶음에서 제외됐다. ①②④⑤만 통합하고 전역 검사를 한 번 수행한다.

English: [Common notices](Casual_Common_Notices_20261017.en.md).
