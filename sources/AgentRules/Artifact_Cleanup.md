# 빌드·백업·임시 산출물 정리 — 필수 (2026-10-04)

> `AGENTS.md`에서 옮긴 본문이다(2026-10-07 슬림화, 내용 변경 없음). 해당 작업을 시작하기 전에 읽는다. Moved verbatim from `AGENTS.md`; read it before starting the matching work.

사용자 요청에 따른 상시 규칙이다. 정상 검증뿐 아니라 산출물의 정리·보존 처리를 작업 완료 조건에 포함한다. Codex와 Claude 등 이 저장소를 작업하는 에이전트가 함께 따른다.

### 이번 누적에서 확인한 원인

- 과거 워크트리 전체 백업에 Git에서 제외된 테스트 앱·빌드 결과·Unity 캐시까지 함께 복사되어, 복구해야 할 소스와 재생성 가능한 산출물이 섞였다.
- `Builds/`의 반복 테스트 앱과 별도 검증 프로젝트에 소유 작업·보존기한·종료 후 정리 단계가 없어 사용이 끝난 결과가 계속 남았다.
- APFS 복사본이 디스크 블록을 공유하므로 폴더 표시 용량만으로 중복의 실제 점유나 삭제 후 회수량을 판단할 수 없었다. 실제 여유 공간 측정이 필요하다.

### 생성 전에 정할 것

- 대형 빌드·다운로드·검증 사본을 만들기 전에 데이터 볼륨의 여유 공간과 재사용 가능한 결과를 확인한다. 빌드와 복사·압축 중간본까지 필요한 최대 공간을 예상한다. 공간이 부족하면 범위를 줄이거나 필요한 정리 승인을 먼저 확보한다.
- 새 테스트 산출물은 기존 빌드 도구의 출력 계약을 지키는 작업 전용 경로 하나에 모은다. `Builds/`를 쓸 경우 작업별 하위 경로를 사용하고, 홈·여러 날짜 작업 폴더·다른 프로젝트에 사본을 흩뿌리지 않는다.
- 작업 폴더의 `artifact-lifecycle.json` 등에 소유 작업 ID, 정확한 경로, 용도, 소스 커밋과 생성 명령, 생성일, 실측 크기, 현재 의존 작업, 임시/납품 구분, 보존 이유와 기한, 종료 시 정리 대상·방법·승인 상태를 기록한다. 계획 용량은 생성 후 실측으로 갱신한다. 인증 정보와 대화 원문은 넣지 않는다.
- 기존 결과를 재사용하고 변경에 필요한 최소 검사를 선택한다. 전체 검사와 런타임 스모크는 기존 최종 검증 규칙대로 한 번 수행한다. 한 변경을 검증하려고 이전 빌드와 전체 프로젝트를 계속 복제하지 않는다.

### 백업과 보존

- Git 이력·필요한 stash/patch, 미커밋·미추적 소스, 원본 에셋과 `.meta`, 설정, 복구에 필요한 기록을 우선 보존한다. 작업 폴더를 제거하기 전 복구 가능성을 검증한다.
- 백업은 포함·제외 목록을 먼저 만든다. 해당 작업의 재생성 가능한 플레이어 빌드, `Library/Bee`, `Library/PlayerDataCache`, 빌드 중간 파일, 생성된 위키 배포 사본 등은 실제 용도를 확인해 제외한다. 전체 프로젝트를 복사한 뒤 나중에 줄이겠다는 방식은 금지한다. Git에서 제외되었다는 이유만으로 폐기하지 않는다.
- `Assets`, `Packages`, `ProjectSettings`, Git, 원본 이미지·첨부, 사용자 변경, 대화·세션·인증 데이터와 현재 납품본을 보존한다. `Builds/` 안에도 patch·로그·검사 결과가 있으므로 폴더째 삭제하지 않는다.
- `Library`와 `Temp` 전체를 일괄 삭제하지 않는다. `AI.Conversations`, `AI.CoreGraph`, `AI.Image` 같은 대화/작업 상태, 사용 중인 `PackageCache`와 데이터, `Temp/CSharpConsole` 등 도구 상태를 재생성 캐시로 간주하지 않는다.
- 새 테스트 앱·APK·Web 빌드·검증 프로젝트는 필요한 실행과 비교를 마친 뒤 종료 시 정리 목록에 넣는다. 계속 필요한 출시/납품본이나 실패 재현용 binary는 소유자·이유·다음 검토일을 기록한다.
- 검사 XML/JSON, 필요한 로그·스크린샷·CPU/GC 원자료, binary 해시, 커밋과 재빌드 명령을 보존한다. 실제 이전/이후 binary가 필요한 성능 비교·실패 재현이 끝나기 전에 삭제하지 않는다. 이 규칙은 기존 검증 근거 보존 의무를 줄이지 않는다.

### 종료 시 정리와 승인

- 정리 직전에 정확한 경로와 소유자, Unity Editor·빌드·테스트·앱·워크트리·의존 캐시의 사용 여부를 확인한다. 오래된 수정 시각이나 이름으로 작업 종료를 추정하지 않는다. 접근 거부나 소유자 불명은 보존·확인 사유이며 우회나 다른 작업의 프로세스 종료로 해결하지 않는다.
- 해당 작업이 소유하고 구체적 정리 동작이 승인된 산출물만 처리한다. 이 상시 지침은 기존 방치 파일·공유 캐시·다른 작업 산출물의 영구 삭제를 포괄 승인하지 않는다.
- 정상 복구가 가능한 보관/정리와 영구 삭제를 구분한다. 영구 삭제·휴지통 비우기 전에는 정확한 경로·실측 크기·용도·영향·재생성 방법과 복구 한계·보존 대상을 묶어 필요한 승인을 받는다. 같은 구체적 동작에 이미 받은 승인은 다시 묻지 않는다. 보존기한 경과만으로 자동 삭제하지 않는다.
- 승인된 종료 정리를 검증 후 같은 작업에서 끝낸다. 실패·중단된 시도, 더 이상 쓰지 않는 작업용 다운로드·자료 사본도 목록에 포함한다. 승인이 남으면 정확한 미처리 목록을 기록하고 최종 보고에서 정리 대기라고 명시한다.
- 공유 패키지·AI 모델·개발 도구 캐시는 별도 의존성 확인과 구체적 삭제 승인을 거친다. 디스크가 부족하다는 이유로 활성 빌드의 캐시나 설치된 런타임을 지우지 않는다.
- 관리형 워크트리는 앱의 복구 가능한 archive/restore 기능을 사용한다. 필요한 ignored 원본은 먼저 보존하되, 산출물을 담은 전체 프로젝트 사본을 추가로 남기지 않는다.
- 같은 데이터 볼륨에서 정리 직전/직후 여유 공간을 실측하고 삭제·보존 경로, 남은 보존 산출물과 승인 대기를 기록한다. 휴지통 이동·폴더 크기 감소를 실제 공간 회수로 보고하지 않는다. APFS 공유 블록과 병행 작업의 쓰기 영향을 구분한다.

### 저장소 밖 작업 사본과 반복 누적 (2026-10-06 전수 점검에서 확인)

2026-10-06 점검에서 프로젝트 폴더(38G)보다 저장소 밖의 작업 사본(약 280G `du`, 실제 회수 약 131G)이 훨씬 컸다. 위 규칙이 다루지 못한 부분을 다음으로 보완한다.

- **저장소 밖 작업 폴더도 정리 대상이다.** `~/Hellscript*Work`, `~/Hellscript*Backups`, 프로젝트 형제 폴더(`BuildWork`, `Builds`, `hunt-edict-*` 같은 추가 워크트리·Unity 클론 `proj*`)는 프로젝트 안의 `Builds/`·`Artifacts/`와 같은 기준으로 소유자·경로·크기·보존기한을 기록한다. 작업 시작 때 만든 폴더는 종료 목록에 넣는다. 병합·푸시된 작업의 Unity 클론은 그 브랜치를 병합한 같은 단계에서 정리 후보로 올린다.
- **Unity 클론(`cp -Rc`)은 1개가 약 7GB다.** 한 작업에 클론 여러 개를 두지 않고 재사용한다. 크기는 `du` 합계가 아닌 정리 전후 같은 볼륨의 여유 공간 차이로 보고한다(이번에 `du` 284G가 실제 131G였다).
- **워크트리 제거 전 확인:** 변경 0건, 브랜치가 `origin/main`에 병합됐거나 원격에 푸시됐는지(`git merge-base --is-ancestor`)를 확인한다. 병합되지 않은 브랜치와 열린 PR의 자료는 정리 대상에서 뺀다. 이동 뒤 `git worktree prune`을 실행하고, 스크래치패드(`/private/tmp`) 워크트리 등록이 끊긴 채 남지 않게 한다.
- **복구 가능한 보관 위치:** `~/.Trash`는 macOS 권한으로 막힐 수 있다. 이때는 같은 볼륨의 `~/HellscriptCleanup-<날짜>/`로 옮기고 `manifest.txt`에 경로·크기를 적는다. 이동은 공간 회수가 아니므로 영구 삭제 승인을 별도로 받는다.
- **웹 빌드는 최신 1건만 둔다.** `BuildWork/Web-*`와 `Builds/Web-*`는 게시된 최신 빌드 외에 반복 누적되지 않게 한다. `Builds/` 아래 0B 표식 폴더(`macOS-*` 등)는 정리 때 함께 제거한다.
- **`Wiki/site/public/`(약 2.9GB)는 배포용 사본이다.** 배포 저장소 체크아웃과 중복이며 `python3 tools/wiki.py build`로 재생성되므로 게시 확인 뒤 보관 이동 대상이다. `site/sources`·`site/media`·`site/data.*`는 로컬 미리보기와 `check`에 쓰이므로 바로 지우지 않는다.
- **`Artifacts/`는 참조 여부로 나눈다.** 최상위 폴더마다 `git ls-files`와 `Docs`·`Wiki/content`·`AGENTS.md`·`tools`의 `Artifacts/<이름>` 참조를 확인한다. 추적되지 않고 참조가 0건인 폴더만 보관 이동 후보이고, 참조된 폴더·`Validation/`·`Films/`의 납품 mp4와 원본은 제외한다. 큰 원자료(예: 영상 `capture/` 2.7GB)는 납품본과 따로 목록에 적어 별도 승인을 받는다.
- **새 검증 폴더는 만들 때 보존기한을 정한다.** 증거는 위키·문서가 참조하는 파일만 남기고 스크린샷·캡처 묶음은 병합 뒤 정리 목록에 올린다. 점검 때마다 새로 쌓이는 것을 막기 위해 작업 종료 보고에 새로 만든 폴더 목록과 크기를 포함한다.
- **정리 뒤 `python3 tools/wiki.py check`로 누락 증거 오류가 없는지 본다.** `Stale wiki source`는 정리와 무관한 데이터 갱신 필요 신호이므로 구분해 보고한다.

English: Artifact cleanup/retention is a completion requirement. Full worktree backups included ignored builds and Unity caches; repeated test apps lacked owners/expiry, and APFS clone sizes did not equal exclusive disk usage. Check available/peak space and reuse valid results before generation. Keep new outputs in a dedicated task-owned path and record owner, exact paths, purpose, source commit/build recipe, creation time, measured size, active dependencies, deliverable/disposable status, retention reason/expiry, cleanup action and approval. Back up recovery-critical Git history, dirty/untracked source, original assets/meta and configuration; explicitly exclude verified regenerable outputs instead of copying whole projects. Preserve original conversations/authentication, user changes, current deliverables, validation reports and performance/reproduction evidence. Never blanket-delete Builds, Library, Temp, Assets, Artifacts or recovery folders; protect AI conversation/tool state and active caches. Verify owners and active editors/builds/tests/apps/worktrees/dependencies immediately before cleanup; denied access or old timestamps are not proof of inactivity. Use appropriate managed archive/restore for recoverable cleanup. Permanent deletion/emptying Trash requires approval for a concrete batch of paths, sizes, purpose, impact, regeneration/recovery limits and protected exclusions; reuse existing approval for the same action. This policy and expiry alone do not authorize deleting legacy/shared/other-task files. Finish approved cleanup, including failed attempts and unused task copies, before reporting completion; otherwise report the exact pending list. Measure free space before/after on the same volume and separate Trash movement, shared clone blocks and concurrent writes from actual reclamation. This is a workflow rule, not an installed background cleaner.

English (2026-10-06 audit additions): Out-of-repo work copies (`~/Hellscript*Work`, sibling `BuildWork`/`Builds`/extra worktrees, Unity clones `proj*`) were larger than the project and are in scope under the same owner/path/size/retention records. Unity clones are ~7 GB each: reuse them, and report reclaimed space from same-volume free-space deltas, not `du` totals (284 GB `du` was ~131 GB real). Before removing a worktree verify zero changes and that the branch is merged into `origin/main` or pushed, keep unmerged/open-PR material, then `git worktree prune`. If `~/.Trash` is blocked, move to `~/HellscriptCleanup-<date>/` with a manifest; a move is not reclamation and permanent deletion needs separate approval. Keep only the latest published web build; remove 0 B marker folders under `Builds/`. `Wiki/site/public/` is a regenerable deploy copy (archive after publishing); keep `site/sources|media|data.*` for preview and `check`. Split `Artifacts/` by reference: only untracked top-level folders with zero references in `Docs`/`Wiki/content`/`AGENTS.md`/`tools` are archive candidates; referenced folders, `Validation/` and delivered film files are excluded, and large raw captures need their own approval. Set retention when creating verification folders and list new folders and sizes in the completion report. After cleanup run `python3 tools/wiki.py check`; report `Stale wiki source` separately as unrelated.

현재 작업과 실패 재현에 필요한 산출물을 보호한 뒤 적용한다. 이 규칙 자체가 백그라운드 청소기나 예약 작업을 설치한 것은 아니다.
