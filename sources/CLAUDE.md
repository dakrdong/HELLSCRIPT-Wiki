# HELLSCRIPT 프로젝트 지침

`AGENTS.md`와 작업에 해당하는 `AgentRules/` 문서를 함께 따른다. 공통 UI·Unity MCP·성능·저장 후 UI 갱신·장비 비교·위키·산출물 규칙의 상세 기준은 그 문서가 소유하며 여기에 복제하지 않는다.

## 언어

- 사용자 대화와 개발 기록은 한국어로 한다. 개발 결과물과 위키 문서는 한국어·영어를 함께 유지한다.
- 게임 문구는 문구 표의 키를 사용하고 두 언어를 함께 채운다. 기존 한국어 문구도 영어를 보완한다. 누락 시 한국어로 대신 표시하고 누락 키를 기록하며 빈 문자열·키 이름을 화면에 내보내지 않는다.
- 언어 설정은 언제든 바꿀 수 있고 기존 화면 방향 설정처럼 기기에만 저장한다. 계정 저장과 섞지 않는다.
- 변수명·코드 주석·커밋 메시지·로그는 영어로 유지하고 저장 키는 바꾸지 않는다.

## 작업과 검증

한 단계씩 진행하고 관련 검사·필요한 기록을 함께 커밋·푸시한다. 승인된 목표를 끝까지 수행하며, 목표 변경이나 불필요한 문맥이 쌓였을 때 기존 기록·PR에 인계 내용을 남긴다. 기본은 한 작업·한 에이전트이며 동시 작업은 최대 3개다. 대표 에셋 검증, 실패 분류, 결과 재사용, 종료 기록은 [작업 절차](AgentRules/Workflow.md)를 따른다.

작업 중에는 관련 검사만 실행한다. 게임 코드·에셋 변경의 전체 Edit Mode와 macOS 개발 빌드 런타임 스모크는 최종 코드 변경과 `main` 통합 뒤 한 번 수행한다. 화면 변경은 관련 스모크를 포함한다. 문서·작업 지침만 바꾼 작업은 게임 실행 검사를 추가하지 않는다. 배치모드가 `ProjectSettings/ProjectSettings.asset`·`ProjectSettings/UnityConnectSettings.asset`을 바꾸면 이번 실행으로 생긴 변경만 확인해 복원하고 사용자 변경은 보존한다.

UI 계약 검사와 회귀 검사는 `python3 tools/check_ui_contract.py --verify`로 묶는다. 재사용에는 소스·미커밋 입력·설정·환경·검사 범위·결과의 일치가 필요하다. 실제 확인한 결과와 미검증 범위를 구분하며 macOS 결과를 실기기 결과로 보고하지 않는다.

위키 원본·DB가 바뀌면 [위키 게시 절차](AgentRules/Wiki_Publish.md)에 따라 병합 후보에서 생성·검사·테스트를 한 번 수행하고 통합된 `main`에서 공개 반영까지 확인한다. 작업 브랜치에서는 원본만 갱신하고, 생성기·화면 수정의 관련 검사만 예외로 실행한다. 한국어·영어, 전체 문서·이력·DB·이미지·근거, 날짜만 표시, View 권한은 유지한다. 사용자의 새로운 제한·승인 범위가 상시 규칙에 우선한다.

## UI와 성능

공통 UI·저장 후 UI 갱신·장비 비교는 `AGENTS.md`의 해당 계약과 기존 소유자를 사용한다. 새 화면의 문자열·색·글꼴·장비 표시를 복제하지 않는다. 기본 글자 크기에서 해상도·안전 영역·한국어·영어·잘림·입력을 확인한다. 제거한 글자 크기 조정 기능과 50~150% 검사·확대 글자 스모크는 다시 추가하거나 실행하지 않는다. 과거 근거는 당시 결과로 보존한다.

성능 변경은 같은 조건의 실제 이전·이후 바이너리 각각 최소 3회와 CPU·GC 원자료로 검증한다. 코드 줄 수·정적 검사·합성 비교를 실제 전체 프레임·웹·실기기 성능과 구분한다. 거래·저장·보상·전투 검증은 유지한다. 빌드·백업·임시 산출물은 [정리 규칙](AgentRules/Artifact_Cleanup.md)에 따라 원본과 활성 작업을 보호하고 종료 시 보존·정리 상태를 기록한다.

## English

Follow `AGENTS.md` and the relevant `AgentRules/` owner documents. Conversation and development records are Korean; deliverables and wiki documents retain Korean and English. Use localization keys for all game text, complete both languages, fall back to Korean and record missing keys, and keep language preferences device-local. Identifiers, comments, commit messages and logs stay English; save keys stay unchanged.

Finish the authorized goal, commit related checks and records together, and use concise handoffs at meaningful context boundaries. Default to one agent and cap independent tasks at three. Follow [the workflow](AgentRules/Workflow.en.md). Run focused checks during development and full Edit Mode plus macOS runtime smoke once after final game changes and integration; documentation-only changes do not need game execution. Restore only batch-mode settings changes caused by this run. UI verification is `python3 tools/check_ui_contract.py --verify`; reuse requires matching inputs, context and proof. State unverified surfaces and distinguish Mac from physical-device results.

Follow the wiki publishing owner at merge, retaining complete bilingual read-only content and date-only display; current user scope overrides standing authorization. Use shared UI, refresh and equipment owners. Verify default-size UI across required viewports and both languages, without restoring retired text-size tests. Performance claims require at least three matched real before/after binary runs and CPU/GC evidence, preserving transaction and simulation checks. Follow artifact retention and cleanup rules.
