# 완료 작업 통합 기록

작성일: 2026-09-28 · [English](Completed_Native_Work_Integration_2026_09_28.en.md)

분산된 작업에서 완료된 변경을 최신 `main`에 통합했다. 기존에 병합된 튜토리얼 연출과 영웅·적·보스 모델 연결을 유지하면서 아래 작업을 합쳤다.

- 일반 적 N08·N09·N10·N13·N14의 모델과 텍스처를 추가했다.
- [필드 환경](Field_Environments.md)과 [영웅 기술 효과](Hero_Skill_Effects.md)를 연결했다.
- [신규 적·보스 공격의 소리](New_Content_Sounds.md)를 기존 녹음에 연결했다.
- [전투 탈출·절전 모드 버튼](Battle_Escape.md)을 반영했다.
- [새 캐릭터의 사냥 칙령](Hunt_Edict_Overview.md)을 ‘균형’으로 시작하게 했다. 기존 캐릭터의 저장된 설정은 바꾸지 않는다.

훈련장에는 아직 완성되지 않은 화면 연결이 남아 있고, 웹 빌드는 사용자가 중단했다. 두 작업과 제작 중이던 추가 모델 도구는 전체 작업 폴더와 Git 이력을 별도로 백업했으며 게임에 추가하지 않았다.

## 통합 검증

- Unity 6000.6.0f1의 Edit Mode 대상 검사 **157/157개**가 통과했다. [검사별 결과](CompletedIntegrationEvidence/editmode-summary.txt)
- 공통 UI 계약과 해당 검사 **9/9개**, 아트 검사 **25/25개**가 통과했다. 설치된 월드 리소스 119개에서 누락·해시·용량 규칙 문제가 없었다. 아트 검사기는 동적으로 이어 붙이는 경로의 앞부분을 완성된 파일명으로 오인하지 않도록 고쳤고 회귀 검사를 추가했다.
- macOS 개발 빌드가 오류 없이 완료됐다. [빌드 결과](CompletedIntegrationEvidence/build-summary.txt)
- 전투 버튼은 세로·가로·PC 16:9·16:10·21:9, 한국어·영어·글자 크기 조합 **24개**에서 배치와 동작을 확인했다. [전투 버튼 결과](CompletedIntegrationEvidence/battle-escape.txt)
- 월드·적·기술 효과 갤러리의 **59개 장면**이 통과했다. [갤러리 결과](CompletedIntegrationEvidence/art-gallery.txt)
- 사냥 칙령 창은 **20개 화면 조합**에서 표시와 포인터 입력을 확인했고, 기본 설정·편집·저장과 별도 프로세스의 재실행을 확인했다. [화면 검사](CompletedIntegrationEvidence/edict-overview.txt) · [재실행 검사](CompletedIntegrationEvidence/edict-reload.txt)

첫 사냥 칙령 실행은 macOS 창이 요청한 크기의 두 배로 열려 해상도 조건에서 중단됐다. 시작 해상도를 명시한 새 프로세스에서 모든 화면·저장 검사를 통과했다. 게임 기능 실패로 감추거나 통과로 계산하지 않았다.

이번 통합에서는 전체 Edit Mode 검사를 다시 실행하지 않았다. 이전 작업의 전체 검사에는 기존 실패 47개가 기록되어 있으므로 전체 검사가 모두 통과했다고 해석하면 안 된다. 검증한 입력은 macOS의 합성 포인터이며 모바일 실기기 검증은 포함하지 않는다.
