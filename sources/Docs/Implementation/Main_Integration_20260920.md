# 완료된 작업의 main 통합 검증

작성일: 2026-09-20

[English](Main_Integration_20260920.en.md)

## 통합 범위

완료된 모든 브랜치와 워크트리를 점검해 미병합 변경을 `main`에 합쳤다. 게임 검증 대상 커밋은 `9b202bb0719279175369b1bf16e7c3cfe1a0550a`다. 이후 검증 기록과 위키 이력만 추가한다.

| 작업 | main 반영 내용 |
|---|---|
| 사냥 칙령 | 단순한 배경·버튼·카드, 선택·펼침 상태 구분, 가로형 왼쪽 탭 아이콘 확대와 가운데 정렬 |
| 룬 | v13 보드, 무기 그림·이름 가독성, 회전된 칸의 포인터 추적, 상세·보관함 드래그와 회수 |
| 창고 | 기준 시안과 같은 배치·조작, 즉시 집기와 포인터 추적, 빈칸 배치 힌트 정리 |
| 마을 HUD | 화면 크기에 따른 이동 패드, 누름·대기 투명도, 물약 위치, NPC 이름과 작은 상단 영역 |
| 장비 상점 | 기획 문서, 상호작용 시안, 한국어·영어와 가로·세로 캡처. 게임 런타임 구현을 완료했다는 뜻은 아니다. |
| Unity MCP | 이미 설치되어 있던 10.2.0의 패키지 명세와 잠금 파일을 커밋 |

룬·창고·마을 HUD는 점검 시점에 이미 `main`에 포함되어 있었다. 남은 사냥 칙령과 장비 상점 브랜치는 충돌 없이 병합했다. 로컬·원격 브랜치 참조 25개가 모두 통합 커밋의 조상임을 [병합 기록](MainIntegration20260920Evidence/branch-audit.json)으로 확인했다. 브랜치나 워크트리는 삭제하지 않았다.

관련 기록: [사냥 칙령](Hunt_Edict_Game_UI.md) · [룬](Rune_V13_Implementation.md) · [마을 HUD](Town_Hud_Responsive.md) · [장비 상점 기획](../Design/HELLSCRIPT_Equipment_Shop_Proposal.md)

## 통합 검증 결과

Unity 6000.6.0f1에서 `Hellscript.Tests` 어셈블리의 Edit Mode 검사 **2,755개를 모두 통과**했다. 실패·건너뛰기는 0개다. [전체 검사 결과](MainIntegration20260920Evidence/editmode.xml)를 보존한다.

같은 소스로 macOS 개발 빌드를 만들었고 오류 0개로 성공했다. [빌드 기록](MainIntegration20260920Evidence/build.txt)과 [소스 트리·검증 범위](MainIntegration20260920Evidence/validation.json)를 보존한다. 배치 실행이 바꾼 Unity Connect 설정은 기존 값으로 복구했다.

실행 검사는 각각 별도의 저장 폴더를 사용했다. 다섯 가지 검사가 모두 통과했고 총 66개 화면을 캡처했다. 대표 화면과 각 검사의 전체 결과를 아래에 연결한다.

| 실행 검사 | 확인한 내용 | 캡처 | 결과 |
|---|---|---:|---|
| 사냥 칙령 | 스킬 성장·장착·저장·재조회, 프리셋·공유, 순서 드래그, 8개 탭 입력 영역, 가로·세로·영어·큰 글자 | 24 | [통과](MainIntegration20260920Evidence/hunt-edict.txt) |
| 마을 HUD | 해상도·글자 크기 18개 조합, 안전 영역, 이동·정지·포인터 소유권, 패드 투명도, 창고 바로가기 | 8 | [통과](MainIntegration20260920Evidence/town-hud.txt) |
| 창고 | 실제 아이템·보석·룬, 즉시 집기·드롭·교환·취소·탭 이동·저장과 여러 화면 배치 | 24 | [통과](MainIntegration20260920Evidence/storage.txt) |
| 룬 화면·회전 | 목록·상세·집기 회전 일치, 회전된 칸 추적, 배치·회수·실행 취소·저장 후 재조회, 무기 이름 표시 | 7 | [통과](MainIntegration20260920Evidence/rune-clarity.txt) |
| 룬 드래그 | 보관함 위 표시, 즉시 집기, 포인터 추적, 회수·배치·취소·저장 후 재조회와 실제 UI 입력 모듈 경로 | 3 | [통과](MainIntegration20260920Evidence/rune-drag.txt) |

[실행 결과 모음](MainIntegration20260920Evidence/runtime-summary.json) · [룬 검사 데이터](MainIntegration20260920Evidence/rune-fixture.json)

룬 검사는 기존의 격리된 검사 저장에서 가져온 룬 160개·배치 5개의 데이터를 사용했다. 실제 사용자 계정은 변경하지 않았다. 입력 검증은 macOS 실행본의 자동 이벤트와 입력 시스템을 사용했다. 사람의 물리 입력, iOS·Android 실기기, 해당 플랫폼 빌드와 Unity Test Runner의 Play Mode 검사는 이번 범위에 포함하지 않았다.

## 대표 실행 화면

![통합된 사냥 칙령과 가운데 정렬한 탭 아이콘](MainIntegration20260920Evidence/hunt-edict.png)

![통합된 마을 HUD](MainIntegration20260920Evidence/town-hud.png)

![창고 세로형 드래그](MainIntegration20260920Evidence/storage.png)

![룬 세로형 화면](MainIntegration20260920Evidence/rune-clarity.png)

![룬 보관함 위 드래그](MainIntegration20260920Evidence/rune-drag.png)
