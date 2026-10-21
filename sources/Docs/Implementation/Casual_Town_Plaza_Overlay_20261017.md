# 마을 광장 오버레이 — 캐주얼 시안 20대지

2026-10-17 · [English](Casual_Town_Plaza_Overlay_20261017.en.md)

## 근거와 범위

Claude `W1fPqL5NgY8L2cmofoC8i8`의 실제 구성명세·HTML section 주석을 추출한 `design-1-town-plaza.md`를 사용했다. PC 10개, 가로 4개, 세로 4개와 KO/EN 명세 2개를 위한 광장 오버레이다. 시작 main은 `dbec4dbed084596471675f375c1d1ac517b4b86c`이며, 공통 API `559abd4f`와 같은 패치를 작업본에 먼저 적용했다. 원본 프로젝트와 Unity는 조작하지 않았다.

`GameUI.Plaza.cs`가 제목판·설정·이름표·상호작용 카드·숨긴 시설안내를, `TownJoystick`이 기존 이동 입력과 패드를 소유한다. `TownPlazaLayout`과 `TownPlazaDecoration`만 화면 전용 helper로 추가했다. 마을 3D·모델·시설창·전역 HUD·콘텐츠독·번역·저장 데이터는 이 변경에 포함하지 않는다.

## 적용

- `UiSkin.Casual`을 명시해 크림 판, 남색 선, 금색 이름/knob, 초록 주요 행동을 연결했다. 카드에는 공통 `UiWindowFrame`, 제목/시설안내에는 `StorageSurface`, 버튼에는 공통 역할·입력·`SetCasualAppearance`를 사용한다.
- 제목은 Jua 지원 범위에 따라 공통 fallback을 사용하며 어두운 외곽선을 제거했다. NPC 이름은 공통 Noto 700, 설명은 Noto 400, 작은 글자는 공통 대비 보정 색을 사용한다. 긴 서비스 제목은 글자 크기를 줄이지 않고 한 줄 말줄임한다.
- 모바일 카드 `.88`, 이름표 `.68`, 제목 14/판 높이 28의 presentation ratio를 적용했다. 세로에서 카드 하단은 중앙 조이스틱 위 12 화면 픽셀을 비운다. 포탈의 기존 최소 44 화면 픽셀 행동 높이를 유지한다.
- 카드 바느질·젬과 조이스틱 ring/drop/shine은 공통 벡터 primitive를 재사용하며 장식은 raycast를 받지 않는다. 설정 원형 면은 hit 사각형의 68%, gear는 면의 56%다.
- `CreateTownBubble`의 새 스킨은 광장 생성 호출만 opt-in한다. 사냥 칙령이 함께 쓰는 기본 helper의 폰트·색·외곽선·배율은 유지했다. 광장 이름표 상단 가림은 커진 제목판 하단도 예약한다.

## 보존과 미적용 제안

첫 포인터 소유·deadzone·release/초점 이탈/창 열림 초기화, WASD/E·기존 탭 이동, 도착 후 카드만 표시, 대화/시설/구매/판매/포탈 실행과 첫 방문 힌트를 유지한다. 시설안내·상태·메뉴는 계속 기본 숨김이다. `CommonPanelOpen`과 안내 열림의 카드/이동 차단도 유지한다.

주민·포탈 168, 시설·각인석 232 높이를 보존하며 각인석 빈 줄 축소 제안은 적용하지 않았다. 안내 삭제·형제 순서 변경·선택 대화 아이콘은 적용하지 않았다. Jua manifest와 원본 글리프는 바꾸지 않아 제목의 `·`는 공통 Noto fallback 대상이다. 일반 대화 버튼은 시안 배율에서 약 38.7 화면 픽셀이며 모든 터치 목표가 44 이상이라고 주장하지 않는다.

이벤트 배지 GreenShade/남색 고리/흰 Jua는 `GameUI.Attendance.cs` 소유이므로 통합 담당에게 요청했다. 이 화면 커밋에서는 해당 파일을 수정하지 않는다.

## 검증과 통합 대기

정적 UI 계약 회귀 15개와 refresh guard 14개를 확인했다. 실제 Unity 6000.6.0f1의 참조·분석기 7개를 유지한 Roslyn 타입 검사에서 Runtime 853소스/Tests 269소스 오류 0·변경 파일 경고 0이다. 기존 Runtime 경고 225개는 별도로 보존했다.

`TownPlazaLayoutTests`의 14개 focused case를 후보 DLL·실제 Unity 수학 어셈블리를 읽는 .NET harness에서 실행했다. 카드 28변형, PC 16:9/16:10/21:9·가로·세로, HUD/조이스틱 겹침·포탈 높이·Canvas 단위 변환을 확인했다. Unity Test Runner 실행·렌더·실제 포인터 검증을 대신하지 않는다.

KO/EN 실제 글자 잘림·모든 NPC/주민/상인/각인석/포탈 상태·독/설정/시설창/중첩 복원·실기기 터치는 미검증이다. 전체 Unity 검사·빌드·main 병합은 수행하지 않았으며 전원 동결 후 통합 담당의 최종 묶음에 남긴다. 외부 작업 폴더의 `validation/`에 응답 파일·로그·입력 해시·작은 후보 DLL을 보존하고 `artifact-lifecycle.json`에 용도를 기록했다. 위키 생성/게시는 병합 담당의 한 번 실행에 남긴다.

## 개발주석 전문 대조 후 보완

후속 확보된 `design-1-spec-ko-original.txt` 269행 전체를 위 요약과 `67552d4c` 구현에 대조했다. 명시적 선택인 대화 아이콘·각인석 축소·Jua 가운뎃점 외에 다음 필수 치수·레시피 누락을 화면 전용으로 보완했다.

- 유지하는 숨김 안내는 PC 300×546/행46/간격7/글자18, 가로 폭220/행30/간격5/글자13, 세로 폭220/행40/글자15로 재배치한다. 모바일 위치는 left10/top44이며 여는 단추는 계속 숨긴다.
- 카드 stitch는 inset4/r12/2px/alpha.13, 젬은 원형16/ring2.5/drop2와 기존 `GoldF08A00A1000` 토큰의 흰색·금색·주황 그라데이션으로 맞춘다. 카드 상단 radial 강조와 offset12/spread20/alpha.24 부드러운 그림자를 공통 벡터 primitive로 추가했다. CSS blur와의 픽셀 동일성은 렌더 미검증이다.
- 설명은 공통 폰트 측정 helper로 줄 높이21을 지정했다(카드 .88에서18.48). 제목은 줄바꿈을 막고 최대 폭을 화면−칸−36으로 제한한다. 손잡이 고리는 PC3/모바일2, 그림자는4/2, 광택 위치는 left22%/top18%다.
- 제목판·톱니의 선2/그림자3은 presentation/Canvas 단위 변환을 반영한다. 카드에는 통합 담당의 `UiWindowFrame.SetSkin(skin,borderWidth:3,shadow:4)`를 소비해 중첩 scale에서 선이 이중 보정되는 문제를 막는다. 톱니는 동일 hit 사각형을 유지하는 로컬 배율로 그린다.
- 통합 담당이 추가한 immutable `UiSkin.Casual.WithInteractionStates()`를 광장 버튼에만 적용한다. 노란 Action hover·초록 Primary hover·눌림3/그림자0·비활성 그림자0을 opt-in한다. 배지의 지름×.26/글자×.2 역시 전문 필수값으로 정정해 공통 담당이 적용한다.

이 후속은 전체 Runtime/Tests 타입 컴파일을 재실행하지 않았다. 변경 런타임4파일 Roslyn 문법 검사, 새 helper/집중 테스트만 컴파일한 .NET 17사례(기존 카드28변형+안내3방향)를 확인했다. 작은 harness에서 이전 Runtime metadata와 새 helper의 동일 타입 때문에 발생하는 CS0436 경고10개는 보존했으며 오류0이다. 공통 신규 API를 포함한 최종 의미 검사는 통합 담당의 한 번 실행에 남긴다. 앞 절의 Runtime853/Tests269 타입 검사 근거는 최초 동결 `67552d4c`에 해당한다.
