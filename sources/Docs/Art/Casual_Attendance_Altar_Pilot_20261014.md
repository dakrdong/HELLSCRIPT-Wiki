# 출석 제단 캐주얼 스킨 파일럿 — 2026-10-14

[English](Casual_Attendance_Altar_Pilot_20261014.en.md) · [검증 영수증](Casual_Attendance_Altar_Pilot_20261014.receipt.json)

기준 `53d6a3e1`에서 `Assets/HELLSCRIPT/Resources/Art/Attendance/popup-altar.png` 한 장을 StyleD 원화로 교체했다. 게임 코드·규칙·보상 ID·세이브·Importer·UI 위치는 변경하지 않았다. 기존 700PNG 승인 목록에 이 배경이 없어 내장 image_gen으로 신규 원화 1회와 검수 수정 3회를 만들었다. 대리석 무늬를 단순화하고 실제 팝업 글자가 놓이는 앞면·바닥·받침을 청회색 음영으로 맞췄다. 선택 원화의 바이트를 후처리 없이 적용했다.

- 새 SHA-256: `1d8ec19907d2e66acefae53a1b051b304f80328bc1fd5eca8874305776e2d18f`
- 기존 SHA-256: `05d2b3df863412c16ca55b75dc6f9b4b37c4bd01f891ea569e8456aa409a3f44`
- GUID: `688eef0212b544a0f9f9f673441bc601`; `.meta` SHA-256 `fd891bb4ae492ab2b472262ea9a40b8bd57b41f2afc6fb69d94563dc2b093dc3`, 바이트 동일.
- 1254×1254 불투명 RGB, 제단 윗면 약 46% 구도. 실제 가져오기는 1024×1024, mipmap 없음; Standalone BC7, Android ASTC 6×6, WebGL ETC2 RGBA8의 기존 1024 상한을 유지한다.

[Before](../Implementation/CasualAttendanceAltarPilotEvidence/before/weekly-ko.png) · [After](../Implementation/CasualAttendanceAltarPilotEvidence/after/editor-selected-1893x1637-ko.png) · [세로 수령 가능](../Implementation/CasualAttendanceAltarPilotEvidence/native-screens/weekly-ready-440x956.png) · [영어 가로 월간](../Implementation/CasualAttendanceAltarPilotEvidence/native-screens/monthly-956x440-en-100.png) · [세로 수령 완료](../Implementation/CasualAttendanceAltarPilotEvidence/native-screens/weekly-claimed-440x956-ko-100.png)

## 검증과 제한

최종 원화 `AttendanceArtTests` 7/7(실패·건너뜀 0), 공통 UI 계약과 회귀 검사 15/15 통과. 원화 검사는 대비 수정 전후 두 입력에 각각 한 번 실행했고, 최종 UI 계약 호출은 동일 입력·로그·환경의 성공 영수증을 재사용했다.

macOS 개발 빌드는 **한 개**, 34.661초·오류 0·종료 0이다. 초기 실행은 25장을 캡처한 뒤 `Collect button obscured: Weekly day 4`(빈 raycast, `RuntimeAttendanceSmoke.Geometry:127`, 호출 `Start:234`)로 종료 1이었다. 이 실패를 제외하거나 통과로 바꾸지 않았다. 같은 바이너리와 격리 저장의 기존 `CollectOnly` 진입점으로 이어 실행한 결과, 같은 수령 버튼 단언과 나머지 검증이 종료 0으로 통과했다. 새 프로세스 `Resume`도 종료 0이다. **초기부터 끝까지 한 번에 깨끗하게 통과한 실행은 주장하지 않는다.** 최초 실패 원인은 확정하지 못했으며 repaint/입력 시간 경계는 추정이다. 확장 전 별도 하네스 후속 점검 위험으로 남긴다.

이어하기 구간은 78개 기본 크기 화면·언어·안전 영역 조합, 1,066개 수령 버튼의 표시·raycast, 1,596개 수령 가능/도장/일차 노출 단언, 104회 raycast 확인 클릭을 통과했다. 440×956·956×440·1440×810·1440×900·1680×720와 기존 추가 640×360, KO/EN을 확인했고 실제 캡처 69장을 보존했다. 개별·모두 수령, 저장 실패와 재시도, 저장 후 UI 갱신, 스와이프·숨김·수동 재열기·전투 일시정지 경로를 두 실행 구간에서 확인했다. 재시작은 월간 28일 전체 수령, 주화 990·골드 110,000·보석 110·코어 55·보관 상자 5개·개봉 장비 1개를 복원했다. 마지막 날짜에는 주간이 새 기간의 1일차로 정상 초기화돼 있다.

모든 플레이어 실행은 `-noaudio`와 `/Users/t8g-2410-pn-005/Documents/Codex/2026-10-14/task-5/evidence/native-save`를 사용했다. 실제 사용자 세이브를 사용하지 않았다. 저장 실패 장면의 개발 콘솔 메시지는 고의로 막은 임시 저장 경로의 증거다. 물리 모바일 기기·전체 5715 검사·웹 빌드·공개 배포는 이번 파일럿에서 수행하지 않았다. 전체 검사·통합 스모크는 최종 통합 소유자의 기존 완료 기준으로 남는다.

## 보존과 다음 범위

다른 담당의 `Art_Transition_20261014.md`, `.en.md`, `_Asset_Index.json`은 해시가 동일하며 이 커밋에 포함하지 않았다. Unity 빌드가 자동으로 바꾼 설정 3파일은 사전 바이트로 복원하고 새 상태 증거를 보존했다. 원본은 `git show 53d6a3e1:Assets/HELLSCRIPT/Resources/Art/Attendance/popup-altar.png`와 `/Users/t8g-2410-pn-005/Documents/Codex/2026-10-14/task-5/evidence/original/popup-altar.png`로 복구할 수 있다. 원본·meta·미승인 그림·검사 실패 증거를 영구 삭제하지 않았다.

다음 후보는 승인된 `random-core-casual-v01.png`를 이용한 `reward-random-core.png` **네이티브 알파 Sprite 대표 한 장**이다. 이 유형은 별도로 알파 가장자리·축소 가독성·녹색 수령 빛·GUID·512 상한을 확인한 뒤, 승인된 전설 상자 Sprite로 소량 확대할 수 있다. 현재 출석 UI와 다른 보상 그림은 기존 테마이므로 전체 캐주얼 적용 완료로 읽지 않는다. 공유 UiTheme(164개 표시 파일), 고정 크롭/아틀라스, 영웅 3D·리깅은 별도 영향 범위다. 확대는 부모의 조율을 기다린다.

원시 프롬프트·선택본·검사 JSON·로그·캡처·산출물 수명은 `/Users/t8g-2410-pn-005/Documents/Codex/2026-10-14/task-5`에 보존했다. 원본 앱의 생성 경로·해시와 종료 보관 경로는 `artifact-lifecycle.json`에 기록한다.
