# 운영 보상 검증

갱신일: 2026-09-26 · [English](Operations_Rewards_Validation.en.md)

`codex/operations-reward-library`의 [운영 보상 준비](Operations_Rewards.md)를 Unity 6000.6.0f1에서 검증했다. 기준 커밋은 `24694040`이다. [검증 요약과 소스 해시](OperationsRewardEvidence/verification.json)에 환경과 적용 범위를 기록했다.

## 검사 결과

| 검사 | 결과 | 근거 |
| --- | --- | --- |
| 운영 보상 최종 Edit Mode | 12 통과, 실패·건너뜀 0 | [결과 XML](OperationsRewardEvidence/operations-final.xml) |
| 관련 확장 Edit Mode | 279개 중 275 통과, 4 실패, 건너뜀 0 | [결과 XML](OperationsRewardEvidence/editmode.xml) |
| 변경 전 main의 전투 저장 검사 | 14개 중 10 통과, 같은 4개 실패 | [기준 결과](OperationsRewardEvidence/baseline-save.xml), [동일 실패 비교](OperationsRewardEvidence/baseline-comparison.json) |
| macOS Development 빌드 | 성공, 빌드 오류 0 | [검증 요약](OperationsRewardEvidence/verification.json) |
| macOS 실행 | 23개 확인 항목 통과, 예외 0 | [실행 기록](OperationsRewardEvidence/runtime.txt) |
| 별도 프로세스 재실행 | 장비·영약 보존, 중복 지급 없음 | [재실행 기록](OperationsRewardEvidence/restart.txt) |
| 공통 UI 계약 | 계약 검사 통과, Python 9개 통과 | `check_ui_contract.py`, `test_ui_contract.py` |
| 운영 설정 도구 | Python 3개 통과 | `test_operations_rewards.py` |
| 카탈로그·아트 | 기존 96종 정의·PNG 바이트와 최초 보상표 유지, 148종 RGBA 검증 | [비교 결과](OperationsRewardEvidence/catalog-art.json) |

확장 검사는 출석, 기본 도메인, 현재 빌드 저장, 보석상, 운영 설정, 번역, 운영 보상, 물약, 보상 상자, 공통 UI, 튜토리얼 이전을 포함한다. 최종 운영 검사에는 물약 기능을 처음 열기 전에 상자를 개봉해도 입문 물약과 지급분이 함께 보존되는 사례를 추가했다. 두 실행의 통과 수를 더해 별도의 전체 검사 결과로 표시하지 않는다.

최초 검사에서 실패한 새 테스트 2개는 준비 데이터 문제였다. 빈 `RunState`는 기존 저장 정규화에서 진행 중인 전투로 취급하지 않으며, 장착 장비는 가방 빈칸을 소비하지 않는다. 유효한 전투 상태와 실제 공간 부족 조건으로 고친 뒤 통과했다.

남아 있는 4개는 `CurrentBuildSaveTests.RiftFailurePreservesInFlightStateThenDiskMatchesTheExactSuccessfulTransition`의 `(0,1)`, `(1,5)`, `(12,1)`, `(12,15)`다. 전투 기록의 `sequence`가 0에서 1로 달라지는 실패이며, 별도 작업 폴더의 변경 전 `24694040`에서도 이름과 실패 메시지가 정확히 같았다. 전체 회귀 검사가 모두 통과한 상태로 보고하지 않는다.

## 실제 조작과 화면

점검 패키지 지급 → 가방 하단의 보상 상자 → 물약 분류 → HP 물약 10개 추가 → 재화 분류 → 기존 심연 주화 100개 추가를 확인했다. 전설 상자는 부위를 고르기 전까지 개봉 버튼이 비활성화되며, 무기를 선택한 뒤 현재 캐릭터의 주무기와 공통 장비 상세를 확인했다. 영약 상자를 열어 해당 캐릭터에게 5개를 지급하고, 예약 품목 거절·지급 재시도·저장·별도 프로세스 재실행까지 검사했다.

- 화면 조합 20개: 440×956, 956×440, 1600×900, 1600×1000, 2100×900 × 한국어/영어 × 글자 크기 100%/150%.
- 창 내부 버튼 경계, 고정 탐색·행동 영역, 글자 높이와 아이콘 로드를 검사했다. 캡처 3장도 직접 열어 확인했다.
- [장비 부위 선택](OperationsRewardEvidence/equipment-choice.png) · [세로 한국어](OperationsRewardEvidence/operations-440-ko-100.png) · [가로 영어 150%](OperationsRewardEvidence/operations-956-en-150.png).

입력은 macOS Player의 Unity 이벤트 시스템에 합성 포인터를 보내고 실제 레이캐스트 대상과 저장 전후 상태를 비교했다. 물리 마우스·모바일 터치 실기기 검사와 구분한다. 별도 저장 경로의 QA 계정에 튜토리얼 면제와 정상 완료 기록을 준비했으며, 자연 플레이로 튜토리얼이나 750단계를 완료한 증거는 아니다.

## 에디터와 게시 범위

Coplay MCP에서 원본 프로젝트 경로와 준비 상태를 확인하고 컴파일 후 오류 0건을 조회했다. 이후 도메인 재로드로 연결이 끊겨, 별도 작업 폴더의 배치 검사와 개발 빌드를 최종 근거로 사용했다. MCP 패키지·전송 설정을 변경하지 않았다. 빌드 대상의 변경 코드·에셋 119개가 원본 작업 브랜치와 같은 바이트임을 확인했다. 빌드가 정규화한 별도 작업 폴더의 ProjectSettings는 작업 커밋에 포함하지 않았다.

위키 생성·검사와 Python 10개, Node 화면 검사를 통과했다. 로그인하지 않은 Codex 내장 브라우저에서 로컬 공개용 사본의 운영 DB 52개, `reserved` 필터 30개, 행사 인장 상세와 쓰기 조작의 부재를 확인했다. **로컬 미리보기 검증이며 공개 배포 확인은 아니다.**

현재 결과는 작업 브랜치 구현이다. `main` 병합, 공개 위키 게시, 실제 캠페인 활성화와 플레이어 발송은 별도다. [공개 위키](https://dakrdong.github.io/HELLSCRIPT-Wiki/#/tree)는 병합된 `main`에서 생성·검사·배포한 후 변경 문서와 DB를 다시 확인해야 한다. 예약 품목 30개는 소비처 미구현이며 지급할 수 없다. 장기 경제 수치와 모바일 실기기 검증도 이번 결과에 포함하지 않는다.
