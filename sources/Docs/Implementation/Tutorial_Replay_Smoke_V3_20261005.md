# 튜토리얼 v3 다시보기 스모크 수정

갱신일: 2026-10-05 · [English](Tutorial_Replay_Smoke_V3_20261005.en.md)

> 역사 기록: 이 문서는 2026-10-05의 v3 튜토리얼 검사 결과입니다. 이후 v4 구덩이 시험과 15레벨 퍼즐 튜토리얼로 대체되었습니다. 현재 구현은 [퍼즐 튜토리얼](Puzzle_Tutorial_Final_Acceptance.md)을 참고하세요.

[이전 정지 조사](Tutorial_Smoke_Stall_20261005.md)의 마지막 다시보기 구간을 수정했다. 두 실행을 마친 `TutorialSmoke`가 `HELLSCRIPT_TUTORIAL_RUNTIME_SMOKE_OK`를 출력하고, 기존 `EconomySnapshot(a)==replayBefore` 검사까지 통과했다. 앱 버그 한 건과 전체 EditMode 시간 초과 한 건은 해결되지 않았다. 다시보기 안내만으로 끝까지 진행된 결과로 해석하면 안 된다.

범위는 개발용 스모크 두 파일이다. 앱의 튜토리얼·저장·칙령 UI·거래·테스트·패키지 코드는 변경하지 않았다. `codex/fix-tutorial-v3-replay-smoke`는 `claude/fix-tutorial-v2-survival-picker`의 `27c0d39e`에서 시작했다. 이 기준은 작업 시작 당시 `origin/main`의 `17dc75e9`를 이미 포함한다. 원본 체크아웃과 다른 세션의 Hunt Edict UI 변경은 보존했다.

## 원인과 공통 안내 따르기

스모크의 실제 계정은 v2 호환 검증을 위해 `edictLegacyAccess=true`다. 그러나 `GameController.BeginTutorial(true)`는 새 비호환 계정을 만들어 레벨 1 다시보기를 격리하므로 다시보기는 v3다. v2처럼 첫 대화 선택을 두 번 기다리거나 일반 포인터 누르기만 반복하면 A/B 관찰과 숫자 입력을 수행할 수 없다.

[RuntimeTutorialSmoke.Progression.cs](TutorialReplaySmokeEvidence20261005/RuntimeTutorialSmoke.Progression.cs.txt)의 기존 `ProgressiveAcceptance`에서 스킬·생존 포인터 루프를 `FollowProgressiveSkillLesson`과 `FollowProgressiveSurvivalLesson`으로 추출했다. 스킬 검사는 실행 영웅(`game.Combat.Hero`)을 읽는다. 기존 신규 계정 검증의 B 관찰 체크포인트 저장·종료와 별도 프로세스 재시작은 유지한다.

[RuntimeTutorialSmoke.cs](../../Assets/HELLSCRIPT/Runtime/Presentation/RuntimeTutorialSmoke.cs)의 다시보기는 첫 대화를 한 번 선택한 뒤 같은 헬퍼로 첫 스킬 배우기·첫 슬롯 장착·저장, 사용 방식 A/B의 실제 관찰, 일시정지·재실행, 최종 A 선택·저장을 수행한다. 보스 생존 실습에서는 실제 입력 칸에 60을 넣고 적용·저장·닫기를 거친다. 실행 영웅의 HP 기준이 60인지와 실제 자동 물약 사용(`tutorialLessonStep==5`)을 확인한 뒤 처치와 마을 복귀까지 기다린다. 기존 전투 대기 제한과 마지막 경제 동등성 검사는 늘리거나 약화하지 않았다.

스냅샷은 금화·재료·심연 주화·코어·룬·레벨·XP·최고 클리어·기록 수·인벤토리 항목을 비교하는 기존 검사다. 전체 계정 바이트 동등성을 검사하지는 않는다. [완료 기록](TutorialReplaySmokeEvidence20261005/runtime-tutorial-smoke.txt)과 [성공 마커](TutorialReplaySmokeEvidence20261005/tutorial-success-marker.txt)가 이 경로의 실행 근거다.

## 남아 있는 앱 버그와 스모크의 실제 조작

기존 계정의 v3 다시보기 생존 실습은 아직 잘못된 대상을 안내한다. [HuntEdictWindow.Summary.cs](../../Assets/HELLSCRIPT/Runtime/Presentation/HuntEdictWindow.Summary.cs)의 `DrawSelectedGroup`는 원래 `store.Data`의 legacy 상태로 간편 선택기를 만든다. [DrawQuickPresetPicker](../../Assets/HELLSCRIPT/Runtime/Presentation/HuntEdictWindow.QuickPresets.cs)는 직접 설정이 펼쳐지기 전에는 false를 반환해 숫자 옵션 행 생성을 막는다. 반면 [ProgressiveSurvivalLessonStep](../../Assets/HELLSCRIPT/Runtime/Presentation/GameUI.TutorialComparison.cs)은 격리된 v3 실행 영웅을 기준으로 `edict-option-survival.potionHpPercent`를 요구한다.

따라서 화면에는 `edict-quick-picker-global/survival.potion`이 있고, 안내가 가리키는 숫자 버튼은 없다. [실패 화면](TutorialReplaySmokeEvidence20261005/replay-hp-timeout.png), [실패 기록](TutorialReplaySmokeEvidence20261005/replay-hp-failure.txt), [실제 활성 버튼 목록](TutorialReplaySmokeEvidence20261005/replay-hp-timeout-controls.txt)이 이를 보인다. 앱 코드는 이 작업에서 수정하지 않았다.

스모크는 이 조합에서만 [누락 화면](TutorialReplaySmokeEvidence20261005/replay-hp-hidden.png)과 [활성 버튼](TutorialReplaySmokeEvidence20261005/replay-hp-hidden-controls.txt)을 남기고, 게이트가 원래의 2초 유예 후 입력을 풀기를 기다린다. 실제 간편 선택기와 `edict-quick-choice-custom`을 포인터로 눌러 세부 옵션을 펼친다. 펼치기 전후 실행 영웅의 전체 `HuntEdictLoadout` JSON이 같은지도 검사한다. 이후 숫자 입력·적용·저장과 실제 물약 사용을 정상 안내로 이어 간다. 게이트 비활성화, 직접 저장 데이터 변경, 단계 강제 이동은 하지 않는다.

이는 완주한 다시보기의 경제 격리 검사를 살리는 스모크 조작이다. 안내 자체가 완전하다는 통과 근거는 아니다. 앱 수정 시에는 legacy 저장 계정과 v3 실행 영웅을 구분하는 소유 경계를 별도로 검증해야 한다.

## 같이 발견한 오래된 스모크 기대

공통 헬퍼를 쓰는 신규 계정 검증에서 세 곳을 실제 정책에 맞췄다.

- 전투 탭 공개 픽스처를 최고 클리어 3에서 4로 바꿨다. 현재 진행 데이터에서 3은 생존 옵션을 추가하고, 4가 전투 타깃 그룹을 처음 공개한다. 자연 진행의 증거로 사용하지 않는다.
- 생존 탭에서 자동 물약 행을 검사하기 전에 실제 `edict-group-survival.potion` 버튼으로 해당 그룹을 고른다.
- 마을 도착 후 기존 호환 스모크와 같은 기기별 출석 팝업 숨김을 적용하고 열린 출석 창을 닫는다. [출석 창에 가린 실패 화면](TutorialReplaySmokeEvidence20261005/progression-attendance-timeout.png)을 남겼다. 칙령의 선택 안내와 “나중에” 동작 검사는 유지한다.

개발용 `-hellscriptTutorialReplayOnly`도 추가했다. `-hellscriptTutorialSmoke`와 함께 쓰며, 튜토리얼을 완료한 작업용 저장에만 허용한다. 실패한 다시보기만 재현할 때 사용하는 경로이며 최종 통과는 이 지름길 없이 원래 두 실행으로 확인했다.

## 검증 결과와 한계

Unity 6000.6.0f1, macOS Metal 개발 빌드와 합성 EventSystem 포인터를 사용했다. [통합 결과](TutorialReplaySmokeEvidence20261005/validation-summary.json)에 시도별 실패, 명령, 바이너리 해시와 재사용 범위를 기록했다.

| 검사 | 결과 |
| --- | --- |
| 원래 TutorialSmoke 첫 실행 / 재시작 | 통과 49.4초 / 535.4초. 최종 마커와 경제 동등성 검사 도달 |
| 신규 EdictProgressionSmoke 첫 실행 | 통과 28.6초. 실제 B 체크포인트 저장 |
| 신규 EdictProgressionSmoke 실패한 재시작만 재검사 | 통과 108.5초. B 복원, 최종 A, HP 60, 자동 물약, 마을, 저장 focus, 공개 탭, 나중에, 수동 재보급 확인 |
| 전체 EditMode 1회 | 5,178개 중 통과 5,177, 실패 1, 건너뜀 0. XML 검사 시간 2,578.3초 |
| 실패한 EditMode 항목만 재검사 | 1개 실패, 같은 180초 제한 시간 초과 |
| 공통 UI 계약 | check 통과, test 11개 통과 |
| macOS 개발 빌드 | 최종 소스 빌드 성공. 통과한 튜토리얼 바이너리와 마지막 신규 계정 재검사 바이너리의 해시 보존 |

EditMode 실패는 `Hellscript.Tests.ObjectiveProgressTests.ExplorationWalksToEveryKnownOfferingAndDeliversWithoutTeleporting`이다. [전체 XML](TutorialReplaySmokeEvidence20261005/editmode.xml)과 [단독 재검사 XML](TutorialReplaySmokeEvidence20261005/failed-objective.xml)에서 같은 180,000ms 시간 초과를 확인했다. 원인은 확정하지 않았다. 이 작업은 해당 시뮬레이션이나 테스트, 제한 시간을 바꾸지 않았다. 전체 검사 통과로 보고하지 않는다.

첫 최종 검증 묶음이 실패한 뒤에는 실패 항목과 직접 영향 범위만 재실행했다. 전체 EditMode 이후 달라진 코드는 스모크 두 파일뿐이고, 앱·테스트·패키지 소스는 같다. [전체 검사 당시 소스](TutorialReplaySmokeEvidence20261005/tested-source-manifest.json)와 [최종 소스](TutorialReplaySmokeEvidence20261005/final-source-manifest.json)를 보존했다. 전체 검사를 다시 실행하지 않았다.

TutorialSmoke 통과 이후 마지막 변경은 `ProgressiveAcceptance`의 출석 팝업 처리 3줄뿐이다. 튜토리얼 진입점과 공유 헬퍼는 동일하다. [재사용 근거](TutorialReplaySmokeEvidence20261005/tutorial-result-reuse.json)에 해시와 정확한 차이를 기록해 이미 통과한 전체 튜토리얼을 반복하지 않았다. 신규 계정 초기 실행도 보존한 B 체크포인트로 재사용하고 실패한 재시작만 다시 확인했다.

레이아웃 검사는 기본 글자 크기에서 440×956, 956×440, 1600×900, 1600×1000, 2100×900의 KO/EN 10개 프로필을 실행했다. [A 관찰](TutorialReplaySmokeEvidence20261005/comparison-a.png), [B 관찰](TutorialReplaySmokeEvidence20261005/comparison-b.png), [영문 비교 화면](TutorialReplaySmokeEvidence20261005/starter-comparison-440x956-en.png), [재연습 마지막 장면](TutorialReplaySmokeEvidence20261005/rewardless-replay.png), [신규 계정 완료 기록](TutorialReplaySmokeEvidence20261005/progression-result.txt)과 [수동 재보급 화면](TutorialReplaySmokeEvidence20261005/entry-manual-restock-440x956-ko.png)을 보존했다. 전체 화면의 사람에 의한 시각 승인, 실제 손 입력, 모바일 실기기, WebGL, 다른 클래스·런타임 스모크는 검증하지 않았다.

로컬 위키의 build·check·Python/Node 검사는 아래 scratch 프로젝트에서 수행하며, 정확한 명령·결과·로그는 작업 경로의 `evidence/wiki-validation-results.json`에 기록한다. `main` 병합과 공개 위키 게시·브라우저 검증은 사용자 지정에 따라 수행하지 않는다. 이후 병합 담당자가 공개한다.

## 산출물과 보존

작업 전용 경로는 `/private/tmp/hellscript-tutorial-replay-v3-20261005-6hihv708`다. 작업 브랜치의 Assets·Packages·ProjectSettings와 원본 체크아웃의 warm Library를 APFS clone으로 복사했다. 첫 복사본의 전체 테스트가 프로젝트 잠금을 오래 유지해 플레이어 빌드용 복사본을 하나 더 만들었다. 원본 Temp, 기존 플레이어 출력, 다른 작업 폴더나 전체 프로젝트 백업은 복사하지 않았다. 위키에 필요한 과거 Artifacts/Validation·Builds 파일만 별도 목록으로 materialize했다.

위키 생성 전 측정한 디렉터리 표시 크기는 다음과 같다. APFS 공유 블록 때문에 합계가 이 작업의 독점 물리 점유량을 뜻하지 않는다.

| 작업 경로의 하위 경로 | 표시 크기(KiB) | 용도 |
| --- | ---: | --- |
| project | 10,628,320 | 전체 테스트, 실패 항목 재현, 로컬 위키 |
| player-project | 8,309,776 | 독립 플레이어 빌드 |
| player | 2,476,180 | 초기·진단·튜토리얼 통과·마지막 신규 계정 재검사의 앱 4개 |
| evidence | 224,772 | 로그·XML·화면·명령·작업용 저장·체크포인트 |

작업 경로의 `artifact-lifecycle.json`에 정확한 경로, 생성 명령, 소스 해시, 최종 실측 크기, 보존 사유와 승인 상태를 기록한다. 근거와 실행 파일은 2026-10-12에 보존 필요성을 재검토하며, 그 날짜가 삭제 승인은 아니다. 임시 출력은 명시적 승인 전까지 보존하고 정리를 실행하지 않았다. 모든 Unity·플레이어 실행에는 작업용 `-hellscriptSavePath`를 지정했다. 일반 플레이어 저장 경로를 사용하거나 다른 세션의 프로세스를 종료하지 않았다.

저장소에는 [근거 파일 목록과 해시](TutorialReplaySmokeEvidence20261005/evidence-manifest.json)의 작은 재현 자료만 추가한다. 기본 main 체크아웃의 untracked runner와 plan은 수정하지 않았으며, 실행 명령은 [첫 실행](TutorialReplaySmokeEvidence20261005/tutorial-phase-one-command.json)·[재시작](TutorialReplaySmokeEvidence20261005/tutorial-resume-command.json)·[신규 계정 재검사](TutorialReplaySmokeEvidence20261005/progression-resume-command.json)에 배열로 남겼다. runner 종료 코드만으로 통과를 판단하지 않고 개별 실행 결과와 마커를 확인했다.
