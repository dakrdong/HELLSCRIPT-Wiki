# HELLSCRIPT 프로젝트 지침

신규 콘텐츠 UI는 `AGENTS.md`의 **신규 콘텐츠와 공통 UI — 필수** 규칙과 `Docs/Implementation/Shared_UI_Contract.md`를 따른다. 공통 부품·신규 창 템플릿·자동 검사 경로를 새 화면에서도 사용한다.

New content UI must follow the shared UI contract and mandatory routing in `AGENTS.md`, including the starter, shared owners and validation checks.

Unity Editor 연동에는 `AGENTS.md`의 **CoplayDev Unity MCP 활용 기준**을 함께 따른다. 도구 선택, 대상 프로젝트 확인, 변경 후 검증과 연결 실패 시 대응 기준은 그 문서를 단일 기준으로 유지한다.

For Unity Editor integration, follow **CoplayDev Unity MCP 활용 기준** in `AGENTS.md` as the single source for tool selection, project targeting, verification, and connection recovery.

CPU 비용과 프레임 갱신 관련 변경은 `AGENTS.md`의 **CPU 비용 재발 방지와 성능 리뷰 — 필수** 및 [성능 리뷰 규칙](Docs/Implementation/Performance_Review_Rules.md)을 따른다. 카메라·HUD 갱신 소유권, 상태 변경 시 UI 반영, 동일 조건 실측과 산출물 보존 기준은 그 문서에서 유지한다.

For CPU and frame-update changes, follow the mandatory performance section in `AGENTS.md` and [the performance review rules](Docs/Implementation/Performance_Review_Rules.en.md), which own the camera/HUD lifecycle, UI invalidation, measurement and build-evidence requirements.

## 언어: 한국어와 영어를 함께 제공한다

이 프로젝트의 모든 개발 결과물은 한국어와 영어를 기본으로 갖춘다. 한쪽만 있는 상태를 완성으로 보지 않는다.

- **게임 화면의 모든 문구**는 한국어와 영어를 함께 가진다. 코드에 문자열을 직접 적지 않고 문구 표에 키로 넣은 뒤 두 언어의 값을 채운다. 새 화면이나 새 문구를 추가할 때 두 언어를 같이 추가한다.
- **언어 설정**을 두어 사용자가 언제든 바꿀 수 있게 한다. 선택한 언어는 기기에만 저장하며 계정 저장과 섞지 않는다. 기존 화면 방향 설정의 기기 저장 구조를 따른다.
- **기존 한국어 문구에도 영어를 채워 넣는다.** 이미 만들어진 화면의 문구가 한국어만 가진 상태로 남아 있지 않게 한다.
- **빠진 번역은 한국어로 대신 보여 주고 그 사실을 기록한다.** 빈 문자열이나 키 이름을 화면에 내보내지 않는다. 어떤 키가 비어 있는지 검사로 확인할 수 있게 한다.
- **개발 기록과 위키 문서**도 두 언어를 갖추는 것을 목표로 한다. 한국어 문서를 먼저 쓰고 영어판을 함께 유지한다.

적용 대상이 아닌 것도 분명히 해 둔다. 코드의 변수명과 주석, 커밋 메시지, 로그 문자열은 지금처럼 영어를 쓴다. 저장 파일의 키 이름도 바꾸지 않는다.

## 대화와 문서의 언어

사용자와의 대화는 한국어로 한다. 개발 기록 문서는 한국어로 쓴다. 커밋 메시지와 코드 주석은 영어로 쓴다.

## 작업 방식

한 단계씩 진행하고 결과를 알린다. 한 단계를 마칠 때마다 바꾼 부분과 직접 관련된 검사를 돌리고 기록을 갱신한 뒤 커밋과 푸시를 한다. 전체 Edit Mode 검사와 런타임 스모크 묶음은 작업의 마지막 코드 변경과 `main` 병합을 마친 뒤 한 번만 실행한다.

위키 내용이나 DB가 추가·변경·삭제되는 작업은 커밋·푸시와 함께 공개 위키에도 동일하게 게시해야 한다. 구체적인 필수 절차는 저장소 루트 `AGENTS.md`의 **커밋·푸시와 공개 위키 동기화**를 따른다. 공개 위키는 전체 문서·이력·DB·이미지·근거를 유지하고 날짜는 연·월·일까지만 표시하며 View 권한만 제공한다. 편집·삭제·저장 등 쓰기 기능은 공개하지 않는다. GitHub Pages 배포와 비로그인 접속 확인까지 마쳐야 작업을 완료한 것으로 본다. 이 상시 규칙에 따른 일반적인 공개 갱신은 매번 재승인을 요청하지 않는다.

검증은 Unity Edit Mode 검사와 macOS 개발 빌드의 런타임 스모크를 함께 쓴다. 화면을 바꾸면 해당 스모크를 마지막 검증에 포함한다. 배치모드 실행이 `ProjectSettings/ProjectSettings.asset`과 `ProjectSettings/UnityConnectSettings.asset`을 건드리므로 커밋 전에 되돌린다.

확인하지 않은 것을 확인했다고 적지 않는다. 실기기에서 보지 않았다면 그렇게 적는다.

## UI 검증 범위

2026-09-30 사용자 결정에 따라 글자 크기 비율 조정 기능을 제거했다. 이후 UI 검증은 기본 글자 크기에서 해상도·안전 영역·한국어·영어를 확인하며, 50~150% 등 글자 크기별 반복 검사와 전용 스모크를 실행하거나 다시 추가하지 않는다. 화면 크기에 맞춘 반응형 배치와 기본 크기에서의 잘림·조작 검사는 유지한다. 과거 검증 기록은 당시 결과로 보존한다.

The player text-size preference is retired. Future UI validation uses the default text size and retains viewport, safe-area, Korean/English, clipping and input checks. Do not add or run text-size matrices or enlarged-text-only smoke checks. Preserve historical evidence as historical.

## 저장 후 UI 갱신과 버튼 깜빡임

필수 규칙은 [UI 갱신 재발 방지](Docs/Implementation/UI_Refresh_Stability_Rules.md), [영문 규칙](Docs/Implementation/UI_Refresh_Stability_Rules.en.md), 루트 `AGENTS.md`의 **저장 후 UI 갱신과 버튼 깜빡임 — 필수**를 따른다. 저장 성공 알림을 표시 의존 키로 거르고 동일 버튼 상태의 전환을 재시작하지 않는다. 실제 값 변경은 반영하며 입력·중첩 창 동안 갱신을 합쳐 보류하고 focus·스크롤을 복원한다. `check_ui_refresh.py`와 `StoreViewBindingTests`, 실제 자동저장·입력·재진입 검사를 포함한다. 성능은 동일 조건의 실제 이전/이후 binary 각각 최소 3회 원자료로 확인하고 정적 조사·합성 비교·동기 구간과 전체 frame·웹·실기기를 구분한다.

English: The linked UI refresh rules and the root AGENTS section are mandatory for future save subscribers, repeated button configuration, input/modal refresh deferral and matched actual before/after validation.
