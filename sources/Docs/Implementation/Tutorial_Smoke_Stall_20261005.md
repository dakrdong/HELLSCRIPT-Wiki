# 튜토리얼 호환 스모크 정지 조사

갱신일: 2026-10-06 · [English](Tutorial_Smoke_Stall_20261005.en.md)

`-hellscriptTutorialSmoke`의 두 번째 실행(`-hellscriptTutorialResume`)이 `origin/main`의 `ee5a949c`와 튜토리얼 연대기 브랜치에서 모두 약 170초 뒤 `Timed out following the prologue gate: survival lesson target=`으로 실패한다고 보고되었다. 이 스모크는 `edictLegacyAccess=true` 계정으로 단계 공개 이전의 v2 흐름을 확인하는 호환 검증이다. 원인은 하나가 아니었다. 단계 공개 구현(`93d59959`)과 그 뒤의 변경이 만든 동작과 스모크의 오래된 기대가 네 곳에서 겹쳤다. 넷 모두 고쳐서 스모크가 처음으로 끝까지 통과한다(최종 표지 `HELLSCRIPT_TUTORIAL_RUNTIME_SMOKE_OK`). 넷째를 고치는 중에 첫 수정(앱)이 만든 회귀도 하나 찾아 고쳤다. 조사 중 연대기 브랜치(PR #52), 패시브 조정(PR #55), Hunt Edict 네이티브 포트(PR #53), 튜토리얼 연결(PR #56)이 차례로 `main`에 병합되어, 최종 검증은 네 병합을 모두 포함한 트리에서 했다. 마지막 병합 뒤에는 F05 단계에서 포인터가 가끔 빗나가는 간헐 실패가 하나 더 드러나 스모크의 `Tap`을 고쳤다(5절). PR #54가 그 수정보다 먼저 병합되어, 이 수정과 실행 8~13의 기록은 후속 PR로 올렸다. 후속 PR을 준비하는 동안 마을 모델 PR #57(`4fc52dba`)과 구덩이 튜토리얼 PR #58(`945fa46f`)이 병합되어, 실행 14·15는 #57까지의 트리에서, 실행 16~19는 #58까지의 트리에서 했다.

**PR #58 이후의 상태.** PR #58이 필수 튜토리얼(P00)을 구덩이 시험으로 바꾸었다(체크포인트 `tutorial-v4`, [구덩이 튜토리얼](Pit_Trial_Tutorial.md)). 스모크의 앞부분은 `RuntimeTutorialSmoke.Pit.cs`의 `PitAcceptance`로, 다시보기는 `PitReplay`로 바뀌었다. 이 기록이 다루는 `RuntimeTutorialSmoke.Progression.cs`(`ReplayAcceptance`, `FollowProgressiveGate`, `ProgressiveAcceptance`)와 `-hellscriptEdictProgressionSmoke` 진입점은 사라졌다. 그래서 1절의 v2 생존 실습과 4절의 v3 다시보기는 지금의 `main`에서 실행되지 않고, 두 절은 당시의 조사 기록으로 남는다. 도착 뒤 구간에 그대로 남은 것은 1절의 선택기 규칙(`QuickAllowed`, 구덩이 실습도 이제 `ProgressivePrologue`에 들어 같은 조건으로 선택기를 숨긴다), 2·3절의 스모크 처리(출석 창 숨기기, 첫 균열 뒤 안내 숨기기), 5절의 `Tap` 안정화다. 검증 표의 실행 1~15는 #58 이전 트리의 기록이다. #58이 병합된 트리에서는 전체 스모크가 마지막 다시보기에서 멈춰, 구덩이 시험의 다시보기에서 앱의 결함을 하나 더 찾아 고쳤다(6절).

## 1. 생존 실습의 물약 선택기 — 앱, 수정함

[HuntEdictWindow.DrawSelectedGroup](../../Assets/HELLSCRIPT/Runtime/Presentation/HuntEdictWindow.Summary.cs)이 `자동 물약` 그룹(`survival.potion`)의 간편 프리셋 선택기를 그룹 이름 검사로 모든 계정에서 제외했다. 신규 계정은 [HuntEdictProgression.Quick](../../Assets/HELLSCRIPT/Runtime/Core/HuntEdictProgression.cs)이 이미 이 범위를 거절하므로 같은 검사가 이중이었다. 기존(legacy) 계정에서는 `Quick`이 항상 참인데도 선택기가 사라졌다.

v2 생존 실습(`GameUI.SurvivalLessonStep`)은 기존 계정에 `edict-quick-picker-global/survival.potion`을 누르라고 안내한다. 버튼이 없어 `LessonTarget`이 null을 돌려주었고 게이트는 대상 없이 안내만 띄웠다. 게이트는 2초 뒤 입력을 풀어 플레이어를 가두지는 않는다. 그러나 안내가 존재하지 않는 버튼을 가리키며, 그 방법으로는 실습을 마칠 수 없다. 스모크의 `FollowGate`는 대상이 생기기를 120초 기다린 뒤 실패한다.

수정은 그룹 이름 검사를 지워 `Quick`이 단독으로 정하게 하는 한 줄이다. 기존 계정의 `자동 물약` 그룹은 다른 그룹처럼 간편 프리셋 선택기를 먼저 보여 준다(`93d59959` 이전 동작). `HuntEdictProgressionTests`에 `Quick`의 두 경우를 고정하는 검사 하나를 추가했다.

판단: 선택기를 되살리거나 v2 실습을 바꾸는 두 길이 있었다. 기존 계정에서 `Quick`이 모든 범위를 허용하고, 문서가 기존 계정의 접근 보존과 진행 중이던 v2 저장의 기존 흐름 마무리를 약속하며, 코치 조언 `death-potion`이 이 범위의 프리셋을 권하므로 앱의 선택기를 복원했다. 기존 계정에도 물약 직접 옵션만 보이게 하려는 의도였다면 이 수정 대신 v2 실습을 바꾸어야 한다.

병합 충돌: PR #53이 이 조건을 `HuntEdictWindow.QuickAllowed`로 옮기면서 물약 그룹 제외도 그대로 가져와 PR #54와 충돌했다. 그 설계 문서는 물약 그룹에 선택기가 없다는 것을 당시 코드의 상태로 적었을 뿐 기존 계정에 대한 결정으로 적지 않았다. 병합할 때 포트의 구조를 받아들이고, 이 수정의 두 조건(`Quick`이 정한다, v3 프롤로그에는 선택기가 없다)을 `QuickAllowed` 안으로 옮겼다. 새 칩 부제도 같은 조건을 따른다. 포트의 계약 스모크는 단계 공개 계정만 쓰고 다른 스모크는 선택기가 있어도 없어도 통과하도록 고쳐져 있어 영향이 없다. 다만 같은 제외가 두 갈래에서 따로 나왔으므로, 기존 계정에 물약 선택기를 두는 판단은 검토에서 한 번 더 확정해야 한다.

회귀와 후속 수정: 처음 푸시한 이 수정에는 회귀가 있었고, 다시보기 확인을 쓰는 중에 드러났다. 다시보기는 항상 v3 실습인데 그 창은 여전히 실제 계정을 읽는다. legacy 계정에서는 `Quick`이 참이라 v3 HP 실습 창에도 선택기가 그려졌고, 실습이 가리키는 `edict-option-survival.potionHpPercent`가 가려졌다([화면](TutorialSmokeEvidence20261005/replay-r2-picker-hides-hp-option.png)). 이 수정 전에는 물약 그룹에 선택기가 없어 이 경로가 동작했다. 같은 조건에 `!ProgressivePrologue`를 더해 v3 프롤로그가 계정과 무관하게 직접 옵션만 보이게 했다(`ed79f12f`). 신규 계정과 v2 실습은 그대로다. 이 조건은 창의 조건이라 EditMode로는 검사할 수 없고, 다시보기 확인이 그 검사다.

## 2. 출석 팝업 — 스모크, 수정함

선택기가 보이자 같은 실행이 마을 도착 뒤 일일 출석 창에 막혔다. 인사창이 닫히면 `GameUI.TickAttendance`가 설계대로 출석 창을 열고, `ShowAttendance`는 `Town.Cancel()`로 진행 중인 마을 이동을 취소한다. 균열지기 앞까지 걷기를 기다리던 단계가 시간 초과했다. 다른 여러 스모크가 쓰는 방식대로 두 출석 창의 "오늘 숨기기"를 저장하고 열린 창을 닫는다. 같은 스모크가 2026-09-30에는 이 처리 없이 통과한 이유는 확인하지 않았다.

## 3. 칙령 안내 카드 — 스모크, 수정함

이어서 F07 단계가 멈췄다. `TryEdictNotice`(`93d59959`)는 결과가 정리될 때마다 안내 카드 한 장을 마을에 띄운다. F06 단계 뒤 마을이 한가해지자 `수정한 설정으로 재도전` 카드가 열려 훈련장으로 가는 이동을 막았다. F05~F07 단계는 같은 안내를 손으로 진행하므로, 첫 균열이 끝나 마을로 돌아온 뒤 `hintsHidden`을 켠다. 첫 균열 중에 자동 팝업이 끼어들지 않는다는 기존 검사가 비어 버리지 않도록 첫 균열 뒤에 둔다.

## 4. 다시보기 — 스모크, 수정함

실행 3과 4는 마지막 다시보기 구간에서 멈췄다. `GameController.BeginTutorial(replay)`는 `GameStore.NewAccount`로 새 계정을 만들기 때문에 기존 계정의 다시보기도 항상 v3 흐름이다. v3의 첫 구간은 대화가 하나뿐인데 스모크는 v2처럼 `tutorial-continue`를 두 번 기다렸다. 두 번째 기다림이 15초 뒤 시간 초과했다.

`RuntimeTutorialSmoke.Progression.cs`의 `ReplayAcceptance`가 다시보기를 v3 흐름대로 따라간다. `FollowProgressiveGate`가 비교 관찰(A/B)의 영역 단계, 최종 선택, HP 기준 숫자 입력을 포인터로 진행한다. 최종 선택과 HP 기준이 다시보기 영웅 자신에게 저장되는지 확인하고, 끝까지 마친 다시보기가 실제 계정의 장비·경제·진행을 바꾸지 않는지 비교한다. `ProgressiveAcceptance`와 같은 안무를 첫 실행 확인 없이 복제했다. 둘을 반복문 하나로 합치지는 않았다. 합치려면 `ProgressiveAcceptance`(v3 첫 실행 확인)를 다시 검증해야 하기 때문이다. 이 도우미의 시간 초과는 `Until`과 같은 화면·창 목록 증거를 남긴다.

다시보기만 따로 도는 복제본 전용 지름길(저장소에 없음)로 세 번 돌려 고쳤다. 첫 실행은 HP 실습에서 대상이 없어 60초 뒤 멈췄다. 진단 증거를 더한 둘째 실행이 1절의 회귀를 보여 주었고, 고친 뒤 셋째 실행이 1분 43초에 통과했다.

## 5. 창 재구성과 포인터 — 스모크, 수정함

PR #56이 병합된 트리에서 두 번째 실행이 F05 단계에서 가끔 `Pointer blocked: edict-option-survival.potionHpPercent`로 멈췄다(실행 9, [실패 기록](TutorialSmokeEvidence20261005/run9-failure-pointer-blocked.txt)). 포인터가 닿은 곳에는 창 틀과 배경만 있었다. 같은 코드로 같은 단계를 통과한 실행(8, 10)도 있어 간헐 실패다. 원인은 코드로 추정한다. 실패한 실행에는 화면이 없었다. 가이드 폴링(0.25초마다)은 가이드의 제어를 처음 찾을 때 안내 띠를 창에 넣고, 이때 창이 다시 만들어져 제어가 바뀐다(`RefreshTutorialAnchor`, `SyncGuideCaption`). 스모크의 `Tap`은 스크롤 뒤 제어를 다시 가져오지만, 창이 다시 만들어진 직후에 쏜 포인터는 새 배치가 자리 잡기 전에 닿을 수 있다.

수정: `Tap`이 빗나가면 제어를 프레임마다 다시 가져오며 최대 0.6초 안에 포인터가 닿기를 기다린다. 계속 막혀 있으면 전과 같이 실패하고, 그때는 화면과 맞은 객체 목록을 남긴다(`pointer-blocked.png`, `pointer-blocked.txt`). 수정 뒤 같은 트리가 전체 스모크 3회를 모두 통과했다(실행 11~13). 수정 전에는 같은 트리에서 3번 중 1번이 이 단계에서 멈췄다. PR #54가 먼저 병합되어 이 수정은 후속 PR로 올렸고, `main`(`4fc52dba`, 마을 모델 PR #57) 위에 다시 얹어 전체 스모크를 2회 더 통과했다(실행 14·15). 구덩이 튜토리얼 PR #58이 병합된 트리에서도 F05 단계는 통과했다(실행 16·18·19, 실행 17은 그 앞에서 끝났다). 수정 뒤 F05 단계를 지난 실행이 8번(11~16, 18, 19)이고 이 단계에서 멈춘 실행은 없지만, 작은 표본이라 간헐 실패가 사라졌다고 단정하지 않는다.

자정: 출석 창 숨기기는 그날 자정(KST)까지만 유효하다. 실행이 00:00을 넘으면 출석 창이 다시 열려 다음 마을 단계가 멈춘다. 실행 8이 23:53에 시작해 이렇게 실패했고([화면](TutorialSmokeEvidence20261005/run8-timeout-attendance-after-midnight.png), 출석 일수 2/7), 자정 뒤 같은 실행 파일은 이 단계를 통과했다. 화면과 시각과 코드로 판단했고 따로 재현하지는 않았다. 스모크는 자정 직전에 시작하지 않는 것이 좋다.

## 6. 구덩이 시험의 다시보기 — 앱, 수정함

PR #58이 병합된 트리에서 전체 스모크를 돌리자 두 번째 실행이 마지막 다시보기(`PitReplay`)의 이동 방식 실습에서 `Timed out following the prologue gate: replay movement lesson target=`으로 멈췄다(실행 16, [실패 기록](TutorialSmokeEvidence20261005/run16-failure-replay-movement-lesson.txt)). PR #58은 스모크 끝의 다시보기를 확인하지 못했다고 적었다. 실패한 실행에는 화면이 없어서, 다시보기만 따로 도는 복제본 전용 지름길(저장소에 없음)로 재현하며 `FollowGate`가 시간 초과 때 화면·게이트 상태·열린 창 목록을 남기게 했다(`gate-timeout.png`, `gate-timeout.txt`). 시간 초과 때 런은 `Fallen` 단계에서 HP 0으로 일시정지해 있었고 대화창은 닫혀 있었으며 게이트에는 대상이 없었다([화면](TutorialSmokeEvidence20261005/replay-fallen-stuck.png)).

원인은 앱이다. 다시보기는 새 계정(`GameStore.NewAccount`)을 만들어 그 위에서 시험을 되풀이한다. 쓰러진 뒤 대화의 `이동 방식을 연다` 버튼은 `GameStore.UnlockPitMovement`를 부르는데, 이 함수는 먼저 런이 실제 계정의 체크포인트(`Data.guide.tutorialRun`)인지 검사한다. 다시보기의 런은 체크포인트가 아니라서 항상 거절되고, 버튼의 처리가 `ContinueTutorial()` 앞에서 조용히 돌아간다. 그래서 단계가 `MovementLesson`으로 넘어가지 못한다. 튜토리얼을 마친 모든 계정이 쓰는 다시보기가 이 자리에서 멈추므로 플레이어에게도 같은 결함으로 보이지만, 실제 메뉴에서의 진입은 직접 확인하지 않았다.

수정: `UnlockPitMovement`가 다시보기 런이면 계정에 아무것도 주지 않고 참을 돌려준다. 다시보기의 칙령 창은 구덩이 실습에서 이동 방식 항목을 자체 규칙(`HuntEdictWindow.Disclosed`)으로 보여 주므로 계정의 공개 상태가 필요 없다. `TutorialPitTests`에 이 경우를 고정하는 검사를 추가했다. 수정 전 코드에서는 이 검사만 실패하고(`The replay's lesson must be able to open the movement option.`, [결과](TutorialSmokeEvidence20261005/editmode-replay-fix-before.xml)), 수정 뒤에는 관련 EditMode 36개(`TutorialPitTests`, `TutorialProgressionTests`)가 모두 통과한다([결과](TutorialSmokeEvidence20261005/editmode-replay-fix-after.xml)). 이 수정은 PR #58의 앱 코드(`GameStore.Tutorials.cs`)를 건드린다. 따로 떼어 낼 수 있도록 커밋을 나누었다. 지름길로 돌린 다시보기는 수정 전 184.4초 뒤 같은 지점에서 멈췄고, 수정 뒤 73.4초에 끝까지 통과했다(보상 없이 마을로 돌아오고, 실제 계정의 장비·경제·진행이 그대로임을 확인).

## 검증

| 실행 | 코드 | 첫 실행 | 두 번째 실행 |
| --- | --- | --- | --- |
| 보고된 기준 | `ee5a949c`, 연대기 브랜치 | 통과 | 약 170초 뒤 실패: 생존 실습, 대상 없음 |
| 1 | 선택기 수정만 | 통과 48.6초 | 125.6초 뒤 실패: 도착 뒤 출석 창 |
| 2 | + 출석 창 숨김 | 통과 47.0초 | 450.5초 뒤 실패: F07 단계의 칙령 안내 카드 |
| 3 | + 첫 균열 뒤 안내 숨김 | 통과 55.3초 | 381.2초 뒤 실패: 다시보기 |
| 4 | 실행 3의 변경 + 병합된 `main`(`17dc75e9`, 연대기 PR #52) | 통과 47.9초 | 462.7초 뒤 실패: 다시보기(실행 3과 같은 지점) |
| 5 | 실행 4 + v3 프롤로그 선택기 제외 + 다시보기 안무 | 통과 47.7초 | 통과 533.4초 |
| 6 | 실행 5 + 병합된 `main`(`a94bb636`, 패시브 조정 PR #55) | 통과 45.6초 | 통과 492.2초 |
| 7 | 실행 6 + 병합된 `main`(`92407a06`, Hunt Edict 네이티브 포트 PR #53) | 통과 46.7초 | 통과 437.2초 |
| 8 | 실행 7 + 병합된 `main`(`b1970567`, 튜토리얼 연결 PR #56) | 통과 45.6초 | 실패 367.8초: 실행이 자정을 넘겨 출석 창이 다시 열림(F07 단계) |
| 9 | 실행 8과 같은 실행 파일, 자정 뒤 | 통과 41.8초 | 실패 402.8초: F05 단계에서 포인터가 빗나감 |
| 10 | 실행 9 + 빗나감 진단(동작 변화 없음) | 통과 45.0초 | 통과 507.1초 |
| 11 | 실행 10 + `Tap` 안정화 | 통과 44.8초 | 통과 446.8초 |
| 12 | 같은 실행 파일 | 통과 41.4초 | 통과 539.1초 |
| 13 | 같은 실행 파일 | 통과 41.2초 | 통과 538.5초 |
| 14 | `Tap` 안정화(같은 패치, 이 PR의 `b12fe73d`를 `4fc52dba` 위에 얹었던 리베이스 전 커밋) + 병합된 `main`(`4fc52dba`, 마을 모델 PR #57) | 통과 45.8초 | 통과 465.7초 |
| 15 | 같은 실행 파일 | 통과 40.3초 | 통과 444.3초 |
| 16 | `Tap` 안정화 + 병합된 `main`(`945fa46f`, 구덩이 튜토리얼 PR #58) | 통과 59.2초 | 실패 366.9초: 다시보기의 이동 방식 실습에서 게이트 대상이 없음 |
| 17 | 같은 실행 파일 | 통과 54.3초 | 342.4초에 첫 균열 도중 플레이어가 오류 없이 종료됨(종료 코드 0, 표지 없음, 원인 미상) |
| 18 | 실행 16 + 다시보기 수정 + `FollowGate` 증거 | 통과 61.4초 | 통과 263.2초 |
| 19 | 같은 실행 파일 | 통과 55.0초 | 통과 243.4초 |

실행 5의 두 번째 실행은 생존 실습의 포인터 10단계(물약 선택기 → `넉넉한 생존 보급` → 회피 그룹 → 선택기 → `모든 예고 회피` → 저장 → 닫기), 처치·보상·도착 대화, 일반 첫 균열, F05, F06, F07 훈련·재도전, 늦은 해금 안내, 보조 강화, 다시보기를 끝까지 통과했다([통과 문장 전체](TutorialSmokeEvidence20261005/run5-runtime-tutorial-smoke.txt)). 오류 로그는 없다. 실행 1~4에서는 단계별 PASS 문장이 스모크가 끝날 때 한 번에 쓰이므로 중간 실패에 남지 않아, 진행 범위를 순서대로 기록된 증거 파일과 실패 지점으로 판단했다.

실행 6과 7의 두 번째 실행도 같은 통과 문장 17개를 모두 남겼고 오류 로그가 없다([실행 6의 통과 문장 전체](TutorialSmokeEvidence20261005/run6-runtime-tutorial-smoke.txt)). 통과한 실행 10~15도 같은 17개를 남겼고([실행 13의 통과 문장 전체](TutorialSmokeEvidence20261005/run13-runtime-tutorial-smoke.txt), [실행 14](TutorialSmokeEvidence20261005/run14-runtime-tutorial-smoke.txt)), F06 문장의 영웅 레벨(3 또는 4)만 첫 균열의 자연 성장에 따라 다르다. 구덩이 튜토리얼 트리의 통과 실행 18·19는 앞부분이 구덩이 시험으로 바뀌어 문장 19개를 남기고, 둘은 글자까지 같다([실행 18의 통과 문장 전체](TutorialSmokeEvidence20261005/run18-runtime-tutorial-smoke.txt)). 실행 7은 포트가 창의 표면을 다시 만든 뒤의 트리다. 생존 실습의 물약 선택기, 다시보기의 v3 안내, 칙령 안내 카드와 출석 창 처리가 새 창에서도 그대로 통과했다.

관련 EditMode 7개 클래스(`HuntEdictProgressionTests` 37개, `HuntEdictQuickPresetTests` 11개, `TutorialProgressionTests` 33개, `TutorialChapterTests` 6개, 포트의 `HuntEdictSentenceTests` 6개, 튜토리얼 연결의 `EdictGuidanceTests` 9개와 `PrologueGateTests` 8개)가 최종 트리에서 110개 모두 통과했다. 앞의 4개 클래스는 실행 5와 6의 코드에서도 87개 모두 통과했다. 추가한 검사는 `Quick`의 값을 고정할 뿐 원래 증상을 재현하지 않는다. 증상은 창의 조건이었으므로 스모크가 그 확인이다.

v3 첫 실행 검증 스모크(`-hellscriptEdictProgressionSmoke`)는 1절의 `!ProgressivePrologue` 조건이 신규 계정의 v3 프롤로그 창을 건드리므로 돌렸다. 포트 병합 전에는 수정한 코드와 수정 전 `main`의 같은 파일들로 만든 실행 파일 모두 첫 실행은 통과하고, 두 번째 실행이 95~97초 뒤 `A committed progression change did not reveal its tab.`에서 같은 지점에 같은 증거로 실패했다([수정 코드](TutorialSmokeEvidence20261005/v3-acceptance-failure.txt), [수정 전](TutorialSmokeEvidence20261005/v3-acceptance-failure-baseline.txt)). 연대기가 칙령 사다리를 바꾼 뒤의 낡은 `highestClear=3` 기대 때문이었고 PR #53이 그 기대를 고쳤다. 포트 병합 직후의 트리에서는 그 지점을 지나 도착 뒤 일일 출석 창이 열린 마을에서 `Until`이 시간 초과했다([화면](TutorialSmokeEvidence20261005/v3-acceptance-attendance-window-after-port.png), [기록](TutorialSmokeEvidence20261005/v3-acceptance-failure-after-port.txt)). PR #56이 이 스모크에도 출석 창 숨기기를 더해, 튜토리얼 연결까지 병합한 최종 트리에서는 두 번째 실행까지 끝까지 통과한다(103.2초, [통과 문장](TutorialSmokeEvidence20261005/v3-acceptance-result.txt)). 후속 PR의 트리(`main` `4fc52dba` + `Tap` 안정화)에서도 같은 스모크가 끝까지 통과하고(25.6초 + 101.4초), 통과 문장 10개가 위 파일과 글자까지 같다. 이 실행에는 이번 변경이 건드리는 비교 실습, HP 실습, 보스, 도착 구간이 들어 있다. 어느 단계의 실패도 이번 변경이 만든 것이 아니다.

근거는 [실행 기록](TutorialSmokeEvidence20261005/runs.json), 실행 1~3의 실패 화면([1](TutorialSmokeEvidence20261005/run1-timeout-attendance-popup.png)·[2](TutorialSmokeEvidence20261005/run2-timeout-edict-notice.png)·[3](TutorialSmokeEvidence20261005/run3-timeout-replay-v3.png)), [실행 4 실패 기록](TutorialSmokeEvidence20261005/run4-failure.txt), 회귀 화면, 생존 실습 [선택기 단계](TutorialSmokeEvidence20261005/survival-step-04-potion-picker.png)·[프리셋 선택](TutorialSmokeEvidence20261005/survival-step-05-careful-choice.png)·[저장 단계](TutorialSmokeEvidence20261005/survival-step-09-save.png), [균열 준비 화면](TutorialSmokeEvidence20261005/first-rift-preparation.png), [다시보기 완료 화면](TutorialSmokeEvidence20261005/run5-replay-complete.png), EditMode 결과 [병합 전](TutorialSmokeEvidence20261005/editmode-premerge.xml)·[최종](TutorialSmokeEvidence20261005/editmode-final.xml)이다. 실행 파일 해시와 빌드·실행 명령은 실행 기록에 있다. 빌드와 스모크 출력은 작업용 임시 경로에만 두고 저장소에는 위 근거만 남긴다.

## 검증하지 않은 것

- 수정 전 실행 파일은 PR #58 이전 트리에서 다시 실행하지 않았다. 보고된 실패, 코드 분석, 수정 뒤 같은 지점의 통과로 원인을 판단했다. v3 검증 스모크의 수정 전 실행만 위 비교를 위해 따로 했다. PR #58 이후에는 v2 생존 실습과 그 v3 검증 스모크가 `main`에 없어 다시 확인할 수 없다.
- 후속 PR의 트리(`main` `945fa46f` + 이 PR)에서는 관련 EditMode 클래스(`TutorialPitTests`, `TutorialProgressionTests`) 36개만 돌렸다. 전체 EditMode와 위 7개 클래스의 나머지는 이 트리에서 돌리지 않았다(110개는 `d1d762fb` 트리의 결과다). 정적 UI 검사 3종(`check_ui_contract.py`, `test_ui_contract.py`, `check_ui_refresh.py`)과 전체 튜토리얼 스모크는 돌렸다. 다른 스모크도, 기존 계정으로 모든 그룹의 선택기를 도는 공유 UI 스모크(`-hellscriptSharedUiSmoke`)도 돌리지 않았다.
- 다시보기의 수정은 `edictLegacyAccess=true`로 표시한 스모크 계정에서만 확인했다. 칙령 항목의 공개가 계정 진행에 달린 비-레전시 계정(예: PR #58 이전에 튜토리얼을 마친 계정)에서 다시보기의 칙령 창이 이동 방식 항목을 보여 주고 저장하는지는 확인하지 못했다. 실제 메뉴에서 다시보기로 들어가는 길도 직접 열어 보지 않았다.
- 실행 17에서 플레이어가 첫 균열 도중 오류 없이 종료된 원인은 알지 못한다. 같은 구간을 지난 실행 16·18·19에서는 나타나지 않았고, 외부 종료로 의심하지만 확인하지 못했다.
- `Tap` 안정화의 원인은 코드로 추정한 것이고 실패한 순간의 화면이 없다. 수정 전 3번 중 1번 실패, 수정 뒤 F05 단계를 지난 실행 8번(트리 세 가지)이라는 작은 표본이며 간헐 실패가 사라졌다고 단정하지 않는다.
- 신규 계정의 실제 화면은 이 변경 뒤 직접 열어 보지 않았다. v3 검증 스모크의 HP 실습 통과와 `Quick`이 같은 값을 돌려주는 것으로 확인했다(당시 기록이며, PR #58 이후 v3 프롤로그는 쓰이지 않는다).
- 스모크 픽스처는 새 영웅을 기존 계정 표지로 바꾼 혼합 계정이라, 선택기가 `현재 설정`으로 보이는데 안내 문구는 `균형 보급`이라 말한다. 새 영웅의 물약 자동 구매가 꺼져 있어 어떤 프리셋과도 일치하지 않기 때문으로 보이나 확인하지 않았고 고치지 않았다.
- macOS 합성 포인터 검증이며 모바일 실기기와 사용자 이해도 검증이 아니다.
