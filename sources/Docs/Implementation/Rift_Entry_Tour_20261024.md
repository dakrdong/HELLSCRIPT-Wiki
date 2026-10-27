# 균열 입장 첫 안내 / First rift entry tour

## 변경 / Change

균열 로비를 처음 열면 벨리나가 도전 단계·재도전·최초 보상 → 피로도 → 물약 → 스킬·전투 기록 → 입장을 다섯 단계로 설명한다. 각 단계의 실제 화면 영역을 밝히고 기존 PrologueGate 내레이션 안에 다음/알겠어요와 건너뛰기 버튼을 둔다. 대사는 존댓말 두 문장 이내이며 한국어/영어를 지원한다.

On the first account visit, Velina explains stage selection/replays/first rewards, fatigue, potions, skills/combat records, and admission in five steps. The current live region stays bright. Explicit Next/Got it and Skip controls live inside the existing PrologueGate narration. Each page uses at most two polite sentences, localized in Korean and English.

- 계정 `guide.riftEntryTour`만 완료/건너뛰기 시 고정 요청 `rift-entry-tour`로 원자 저장한다. 기존 저장에 키가 없으면 false다. 영웅별 mapComplete·퀘스트·전투·재화·물약은 안내 때문에 바뀌지 않는다.
- Only the account flag is committed by the fixed, idempotent `rift-entry-tour` request on completion or skip. Missing legacy fields remain false. The tour does not advance hero map progress, quests, runs, wallet or potions.
- 저장 실패 시 플래그/디스크를 유지하고 안내 안에 오류와 재시도를 표시한다. 닫기·프로세스 종료 등 완료 전 중단은 확인으로 기록하지 않는다. 다시 열면 첫 단계부터 시작한다.
- Save failure keeps the prior flag/disk and retains the active tour with an error and retry actions. Closing/interruption never acknowledges unfinished work; reopening starts at step one.
- 최초 RiftOne 때 mapComplete가 false인 상태도 허용한다. 실제 진행 중인 룬/전투/대장간·방어구 gate, cinematic, 다른 foreground modal은 우선한다. 기존 F02 포커스는 지우지 않고 시각 링만 일시 제거하여 안내 후 다시 표시한다.
- The first RiftOne visit is eligible even while mapComplete is false. Actual rune/run/forge/armor guidance, cinematics and other foreground modals take precedence. Existing F02 context is preserved; its ring resumes after the tour.
- 로비 CanvasGroup의 interactable과 gate의 전체 입력 차단을 함께 사용한다. 밝은 영역도 실제 버튼을 누를 수 없고, 내레이션 배경 클릭은 넘어가지 않는다. 명시 버튼만 진행/건너뛰기를 실행한다.
- The lobby CanvasGroup and full gate raycast blocker prevent underlying pointer/submit actions, including actions in bright regions. Card/backdrop taps do not advance. Only explicit navigation acts.
- 기존 로비 크기·도형·기존 버튼 이름·스킬 창 반환 경로는 유지한다. 렌더의 실제 영역을 갱신하므로 측정용 숨은 복제/이전 프레임을 가리키지 않는다. 회전/재렌더 후 현재 영역을 다시 얻고 필요하면 UiVisibility.EnsureVisible을 사용한다.
- Existing lobby geometry, control names and skill-window return flow are preserved. Passive region references update with the actual final render, never measurement probes or deferred inactive copies. Rotation/repaint rebinds the current rectangle and uses EnsureVisible when needed.
- 강조 영역 밖의 위/아래/좌/우 공간에서 글을 측정해 배치한다. 글꼴 축소는 추가하지 않는다. 부족한 높이에서는 대사만 스크롤하고 최소 44 화면 픽셀의 내비게이션은 고정한다. 일반 Show/ShowMessage는 기존 동작을 유지하며 tour UI는 첫 사용 때만 만든다.
- Narration is measured in free space above/below/beside the highlight without adding font shrinking. Short space scrolls only the copy while navigation stays fixed at at least 44 screen pixels. Ordinary Show/ShowMessage behavior is unchanged; optional tour controls are created lazily.
- DailyMinutes, RecoveryMinutes, RecoveryLimit, SpeedCost, ResetHour, ResetUtcOffsetHours와 공통 재화 이름으로 문구를 구성한다. 현재 KST 자정 일일분 초기화, 유료 회복분 이월 계산은 동일하다.
- Copy derives from the current rule constants and shared currency label. Existing KST-midnight daily reset and paid-time carryover arithmetic is preserved.

## 검증 인계 / Validation handoff

이 에이전트는 root의 지시에 따라 Unity·테스트·빌드·실제 CLI 리뷰를 실행하지 않았다. 아래는 **작성한 검사와 실행 요청**이며 통과 결과가 아니다. 원본 에셋·기존 meta·실제 플레이어 저장은 수정하지 않았다. B 음성 생성/계정 UI는 계속 보류했다.

Per root coordination, this agent ran no Unity, tests, builds or actual CLI review. The following are **authored checks and requested execution**, not passing evidence. Original assets, existing metas and real player saves were untouched. B audio generation/account UI remains paused.

작성 경계는 `0968f014b54bf2dcf2611f244342367451098ccb` 기반 `codex/rift-entry-tour-20261024`이다. 이 경계는 승인된 stacked integration이며 origin/main `1f1887d1148d99e5b5f42f9a899d8945c6209259`를 포함한다. U-J는 가져오지 않았다. `git diff --check`의 공백 검사는 통과했다.

The branch is `codex/rift-entry-tour-20261024`, based on the approved stacked integration `0968f014b54bf2dcf2611f244342367451098ccb`, which contains that origin/main revision. U-J was not imported. The whitespace-only `git diff --check` passed.

집중 EditMode 필터 / Focused EditMode filter:

`Hellscript.Tests.RiftEntryTourTests;Hellscript.Tests.PrologueTourTests;Hellscript.Tests.PrologueGateTests;Hellscript.Tests.RiftEntryCasualWindowTests;Hellscript.Tests.RiftEntryTests;Hellscript.Tests.TutorialHeroSlotsTests`

- 새 fixture는 legacy 기본값, account/hero 경계, 재시작 유지, 5단계 확인·중간 건너뛰기·중단, 저장 실패/재시도, underlying submit 차단/복원, 다른 modal 우선, F02 재개, 실제 영역/내레이션 비겹침·44px·레이캐스트, 회전/재렌더, KST 경계/paid carryover를 검사한다.
- New fixtures cover legacy defaults, account/hero ownership, persistence, five acknowledgements/skip/interruption, failed-save retry, input blocking/restoration, foreground modal priority, F02 resumption, live-region separation/44px/raycasting, repaint/rotation, and reset/carryover boundaries.
- 화면 비율 fixture는 KO 440:956, EN 956:440, EN 1920:1080, KO 768:1024를 사용한다. 실제 GameView/기기 픽셀 캡처로 주장하지 않는다. root가 KO 440×956 및 EN 956×440 실제 스모크/캡처를 이어서 확인해야 한다.
- Aspect fixtures use those four boards but are not physical GameView/device captures. Root should verify actual KO 440×956 and EN 956×440 smoke/captures.
- `tools/check_ui_contract.py`와 `tools/check_ui_refresh.py`의 해당 변경 경로 집중 확인 및 실제 리뷰/local CI는 root 소유다. 전체 EditMode는 요청하지 않는다.
- Root owns affected-path UI-contract/UI-refresh checks, actual review and local CI. No full EditMode run is requested.

후속 수동 확인 / Follow-up manual check: 미완료 계정의 첫 RiftOne 로비 열기 → 다섯 단계에서 실제 입장·회복·빠른 입장·물약·스킬 버튼 미동작 → 마지막 확인 → 기존 입장/F02가 돌아옴 → 재입장/계정 재로드 시 재등장 안 함. 중간 닫기/앱 종료는 재등장, 저장 실패는 완료되지 않고 오류와 재시도 유지. 일반 prologue/graph tap 메시지와 U-E PartA 스킬 창 닫기 반환도 확인한다.

Open first RiftOne with an unfinished account; verify that admission/recovery/paid entry/potion/skill actions cannot execute during all five pages; acknowledge, then verify ordinary admission/F02 resumption and no repeat after reopening/reloading. Closing/interruption should repeat; failed save should retain the error/retry. Recheck normal prologue/graph tap messages and U-E PartA skill-window return.

## 통합 후보 집중 검사 갱신 (2026-10-24)

최신 main 1f1887d1을 포함한 후보 5936415e에서 E/H/F와 직접 호출자 23개 fixture를 실행했다. 최초 컴파일 오류는 새 E 테스트의 List.Length를 Count로 고쳤다. 이후 550개 중 521개 통과, H/F 29개는 EditMode EventSystem 가정에서 실패했다. 최종 H/F 40/40 통과와 바뀌지 않은 나머지 510개 근거를 합쳐 단위 범위 550/550을 확인했다. 런타임 코드는 이 테스트 보정으로 바뀌지 않았다. 근거: 로컬 takeover/e-h-f-unit-proof.json, e-h-f-retest/results.xml, h-f-final-unit/results.xml. 로컬 CI 9단계도 통과했다(takeover/integration-ci/report.json; 설치 Python3.9.6/Node26.8.2, 워크플로우3.12/22와 정확 버전 동등성은 미확인).

EditMode에서 새 EventSystem은 전역 current에 등록되지 않으므로 테스트 자체 인스턴스를 사용한다. 런타임 지연 삭제의 알려진 EditMode 엔진 진단만 정확한 메시지로 예상하고 다른 오류는 계속 실패한다. 저장 실패는 별도 특정 메시지를 검증한다. 단위 테스트는 실제 hit-area 크기·필터 판단·이벤트 전달·저장·초안·복귀를 검사한다.

**실제 렌더 프레임의 전역 Canvas 레이캐스트·네이티브 포인터·한/영 캡처는 미완료다.** Metal batch 대조에서도 EditMode 전역 hit가 비어 있던 5개 결과를 h-f-raycast-graphics에 보존했다. 그 실패를 네이티브 통과로 바꾸지 않았으며 실제 프레임 검사는 별도로 수행해야 한다. 임시 회사 식별자와 자체 Unity import 9파일은 해시와 함께 복구 가능 보관하고 HEAD 바이트로 복원했다. 실제 CLI 리뷰·원격 통합은 후속 근거가 필요하다.

## Integrated focused validation update (2026-10-24)

Candidate5936415e includes latest main1f1887d1. The 23 directly affected E/H/F fixtures first stopped at a new test's List.Length compile error, corrected to Count. The subsequent run passed521/550;29 H/F cases failed an EditMode EventSystem assumption. The final corrected H/F run passed40/40. Together with the510 unchanged passing cases this establishes550/550 unit coverage. Runtime source did not change for these fixture corrections. Evidence: local takeover/e-h-f-unit-proof.json, e-h-f-retest/results.xml, h-f-final-unit/results.xml. All9 local CI steps passed (takeover/integration-ci/report.json; installedPython3.9.6/Node26.8.2, exact parity with workflow3.12/22 unverified).

New EventSystems do not register as global current in EditMode, so fixtures use their own instances. Only the known deferred-Destroy EditMode engine diagnostic is expected by its exact message; other errors still fail, and failed-save diagnostics are checked separately. Unit assertions retain real hit-area geometry, filter decisions, event dispatch, persistence, draft and navigation checks.

**Rendered global Canvas raycasts, native pointer interaction and Korean/English captures remain pending.** The five empty-hit failures persisted with Metal batch and are retained in h-f-raycast-graphics. They are not reported as native success; rendered-frame validation is a separate requirement. Nine own temporary/import files were preserved with hashes before restoring HEAD bytes. Actual local CLI review and remote integration need their own subsequent evidence.
