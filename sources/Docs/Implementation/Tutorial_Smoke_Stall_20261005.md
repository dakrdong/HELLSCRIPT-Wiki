# 튜토리얼 호환 스모크 정지 조사

갱신일: 2026-10-05 · [English](Tutorial_Smoke_Stall_20261005.en.md)

`-hellscriptTutorialSmoke`의 두 번째 실행(`-hellscriptTutorialResume`)이 `origin/main`의 `ee5a949c`와 튜토리얼 연대기 브랜치에서 모두 약 170초 뒤 `Timed out following the prologue gate: survival lesson target=`으로 실패한다고 보고되었다. 이 스모크는 `edictLegacyAccess=true` 계정으로 단계 공개 이전의 v2 흐름을 확인하는 호환 검증이다. 원인은 하나가 아니었다. 단계 공개 구현(`93d59959`)과 그 뒤의 변경이 만든 동작과 스모크의 오래된 기대가 네 곳에서 겹쳤다. 넷 모두 고쳐서 스모크가 처음으로 끝까지 통과한다(최종 표지 `HELLSCRIPT_TUTORIAL_RUNTIME_SMOKE_OK`). 넷째를 고치는 중에 첫 수정(앱)이 만든 회귀도 하나 찾아 고쳤다. 조사 중 연대기 브랜치(PR #52), 패시브 조정(PR #55), Hunt Edict 네이티브 포트(PR #53)가 차례로 `main`에 병합되어, 최종 검증은 세 병합을 모두 포함한 트리에서 했다.

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

실행 5의 두 번째 실행은 생존 실습의 포인터 10단계(물약 선택기 → `넉넉한 생존 보급` → 회피 그룹 → 선택기 → `모든 예고 회피` → 저장 → 닫기), 처치·보상·도착 대화, 일반 첫 균열, F05, F06, F07 훈련·재도전, 늦은 해금 안내, 보조 강화, 다시보기를 끝까지 통과했다([통과 문장 전체](TutorialSmokeEvidence20261005/run5-runtime-tutorial-smoke.txt)). 오류 로그는 없다. 실행 1~4에서는 단계별 PASS 문장이 스모크가 끝날 때 한 번에 쓰이므로 중간 실패에 남지 않아, 진행 범위를 순서대로 기록된 증거 파일과 실패 지점으로 판단했다.

실행 6과 7의 두 번째 실행도 같은 통과 문장 17개를 모두 남겼고 오류 로그가 없다([실행 6의 통과 문장 전체](TutorialSmokeEvidence20261005/run6-runtime-tutorial-smoke.txt)). 실행 7은 포트가 창의 표면을 다시 만든 뒤의 트리다. 생존 실습의 물약 선택기, 다시보기의 v3 안내, 칙령 안내 카드와 출석 창 처리가 새 창에서도 그대로 통과했다.

관련 EditMode 5개 클래스(`HuntEdictProgressionTests` 37개, `HuntEdictQuickPresetTests` 11개, `TutorialProgressionTests` 33개, `TutorialChapterTests` 6개, 포트의 `HuntEdictSentenceTests` 6개)가 최종 트리에서 93개 모두 통과했다. 앞의 4개 클래스는 실행 5와 6의 코드에서도 87개 모두 통과했다. 추가한 검사는 `Quick`의 값을 고정할 뿐 원래 증상을 재현하지 않는다. 증상은 창의 조건이었으므로 스모크가 그 확인이다.

v3 첫 실행 검증 스모크(`-hellscriptEdictProgressionSmoke`)는 1절의 `!ProgressivePrologue` 조건이 신규 계정의 v3 프롤로그 창을 건드리므로 돌렸다. 포트 병합 전에는 수정한 코드와 수정 전 `main`의 같은 파일들로 만든 실행 파일 모두 첫 실행은 통과하고, 두 번째 실행이 95~97초 뒤 `A committed progression change did not reveal its tab.`에서 같은 지점에 같은 증거로 실패했다([수정 코드](TutorialSmokeEvidence20261005/v3-acceptance-failure.txt), [수정 전](TutorialSmokeEvidence20261005/v3-acceptance-failure-baseline.txt)). 연대기가 칙령 사다리를 바꾼 뒤의 낡은 `highestClear=3` 기대 때문이었고 PR #53이 그 기대를 고쳤다. 병합된 최종 트리에서는 그 지점을 지나 도착 뒤 일일 출석 창이 열린 마을에서 `Until`이 시간 초과한다([화면](TutorialSmokeEvidence20261005/v3-acceptance-attendance-window-after-port.png), [기록](TutorialSmokeEvidence20261005/v3-acceptance-failure-after-port.txt)). 이 스모크는 출석 창을 숨기지 않으며 2절과 같은 종류로 보이나, 어느 `Until`인지 이름을 붙인 진단은 하지 않았다. 어느 쪽이든 이번 변경이 건드리는 구간(비교 실습, HP 실습, 보스, 도착, 마을 칙령 레이아웃 10개)은 통과했고, 남은 실패는 이번 변경과 무관해 고치지 않았다.

근거는 [실행 기록](TutorialSmokeEvidence20261005/runs.json), 실행 1~3의 실패 화면([1](TutorialSmokeEvidence20261005/run1-timeout-attendance-popup.png)·[2](TutorialSmokeEvidence20261005/run2-timeout-edict-notice.png)·[3](TutorialSmokeEvidence20261005/run3-timeout-replay-v3.png)), [실행 4 실패 기록](TutorialSmokeEvidence20261005/run4-failure.txt), 회귀 화면, 생존 실습 [선택기 단계](TutorialSmokeEvidence20261005/survival-step-04-potion-picker.png)·[프리셋 선택](TutorialSmokeEvidence20261005/survival-step-05-careful-choice.png)·[저장 단계](TutorialSmokeEvidence20261005/survival-step-09-save.png), [균열 준비 화면](TutorialSmokeEvidence20261005/first-rift-preparation.png), [다시보기 완료 화면](TutorialSmokeEvidence20261005/run5-replay-complete.png), EditMode 결과 [병합 전](TutorialSmokeEvidence20261005/editmode-premerge.xml)·[최종](TutorialSmokeEvidence20261005/editmode-final.xml)이다. 실행 파일 해시와 빌드·실행 명령은 실행 기록에 있다. 빌드와 스모크 출력은 작업용 임시 경로에만 두고 저장소에는 위 근거만 남긴다.

## 검증하지 않은 것

- 수정 전 실행 파일은 이 트리에서 다시 실행하지 않았다. 보고된 실패, 코드 분석, 수정 뒤 같은 지점의 통과로 원인을 판단했다. v3 검증 스모크의 수정 전 실행만 위 비교를 위해 따로 했다.
- 전체 EditMode와 다른 스모크는 실행하지 않았다. v3 검증 스모크의 두 번째 실행은 출석 창을 숨기지 않는 낡은 기대로 끝까지 가지 못했다. 공유 UI 스모크(`-hellscriptSharedUiSmoke`)처럼 기존 계정으로 모든 그룹의 선택기를 도는 스모크도 돌리지 않았다. 포트가 바꾼 새 창에서 기존 계정의 물약 선택기는 v2 생존 실습으로만 확인했다.
- 신규 계정의 실제 화면은 이 변경 뒤 직접 열어 보지 않았다. v3 검증 스모크의 HP 실습 통과와 `Quick`이 같은 값을 돌려주는 것으로 확인했다.
- 스모크 픽스처는 새 영웅을 기존 계정 표지로 바꾼 혼합 계정이라, 선택기가 `현재 설정`으로 보이는데 안내 문구는 `균형 보급`이라 말한다. 새 영웅의 물약 자동 구매가 꺼져 있어 어떤 프리셋과도 일치하지 않기 때문으로 보이나 확인하지 않았고 고치지 않았다.
- macOS 합성 포인터 검증이며 모바일 실기기와 사용자 이해도 검증이 아니다.
