# 산출물 보존·정리 — 현행 소유 규칙

빌드·다운로드·백업·검증 사본·정리에만 읽는다. 소스 복구와 재생성 출력은 구분하고 보존·승인된 정리까지 완료한다. 과거 점검 수치·완료된 정리 경로는 Git 이력에 보존하며 현재 삭제 허가로 사용하지 않는다.

## 생성과 복구
- 큰 생성 전에 같은 데이터 볼륨의 여유 공간, 재사용 결과, 출력+staging 최대 크기와 작업 여유를 확인한다. 부족하면 범위를 줄이거나 정확한 정리 승인을 먼저 받는다. 필요한 부분 검사를 재사용하고 하나의 변경을 위해 전체 Unity 프로젝트·기존 빌드를 복제하지 않는다.
- 출력은 기존 도구의 계약 안에서 작업 소유 경로 한곳에 둔다. `artifact-lifecycle.json` 등에 소유자/작업 ID·정확한 경로·용도·소스 커밋/명령·생성일·예상→실측 크기·활성 의존성·임시/납품·보존 이유/검토일·정리 방법/승인 상태를 남긴다. 비밀과 대화 원문은 넣지 않는다.
- 복구에 필요한 Git refs·stash/patch·미커밋/미추적 소스·원본 에셋/`.meta`·설정과 작은 근거를 먼저 보존하고 복원 가능함을 확인한다. 포함/제외 목록으로 검증된 재생성 출력만 제외한다. `.gitignore`만으로 폐기 가능하다고 판단하거나 전체 사본부터 만들지 않는다.
- 대화·세션·첨부·인증·사용자 문서/변경·현재 납품·다른 작업은 보호한다. `Assets`, `Packages`, `ProjectSettings`, Git, `Library`, `Temp`, `Builds`, `Artifacts`, 복구 폴더를 통째로 삭제하지 않는다. 특히 AI.Conversations/CoreGraph/Image, 사용 중 PackageCache, Temp/CSharpConsole 등 상태를 일반 캐시로 취급하지 않는다.
- 테스트 앱/APK/Web 출력은 필요한 검증·비교까지 유지한 뒤 정리 검토에 넣는다. 납품·실패 재현·성능 비교에 필요한 바이너리, XML/JSON·관련 로그·측정 원자료·해시·재생성 명령은 필요한 동안 보존하고 소유자·이유·다음 검토일을 기록한다. 같은 증거 폴더를 기본적으로 여러 곳에 복제하지 않는다.

## 정리·승인·공간 보고
- 직전에 정확한 경로·소유자와 Editor·앱·빌드·검사·워크트리·의존 job/cache를 확인한다. 이름/수정일만으로 비활성을 판단하지 않는다. 접근 거부·소유자 불명은 보존하고 확인하며 우회하거나 다른 작업을 중단하지 않는다.
- 자기 작업이 소유하고 **정확한 정리 동작이 승인된 것만** 처리한다. 복구 가능한 archive/이동과 영구 삭제를 구분한다. 영구 삭제·휴지통 비우기 전에 경로·실측 크기·용도·영향·재생성/복구 한계·보호 대상을 한 묶음으로 제시해 승인받는다. 같은 구체적 승인은 재사용하고 기한 만료만으로 삭제하지 않는다.
- 공유 패키지·모델·도구 캐시는 별도 소유/의존성 확인과 정확한 승인이 필요하다. 공간 확보를 위해 활성 Unity import/build cache나 설치 런타임을 지우지 않는다. 실패·중단한 시도도 종료 목록에 넣고 승인된 정리는 같은 작업에서 끝낸다. 남은 경로는 정리 대기로 보고한다.
- managed worktree는 앱의 archive/restore를 사용한다. 제거 전 dirty/untracked/필요 ignored 입력을 확인·보존하고 브랜치의 병합/원격 복구 가능성을 검증한다. 미병합·열린 PR의 활성 자료는 보존하며 Git 이력 없이 작업 폴더를 버리지 않는다. 필요한 prune은 정상 lifecycle 뒤에만 한다. 별도 전체 백업을 추가로 남기지 않는다.
- 같은 볼륨의 정리 직전/직후 여유, 제거/보존 경로·남은 출력·대기 승인을 기록한다. `du` 표시·휴지통 이동·폴더 크기 감소는 실제 회수 공간이 아니다. APFS 공유 블록과 병행 쓰기의 영향을 구분한다. 이 절차는 백그라운드 청소기·예약 작업 설치가 아니다.

## HELLSCRIPT 경로별 판단
- 저장소 밖 `~/Hellscript*Work`, `~/Hellscript*Backups`, 형제 BuildWork/Builds, 추가 worktree/Unity clone, `/private/tmp`도 같은 소유·보존 기준이다. 큰 Unity clone은 기존 것을 재사용하고 병합 후 정리 후보로 기록한다. Trash 접근이 안 되면 승인된 recoverable 이동만 하며 이동을 공간 회수로 보고하지 않는다.
- Web 빌드는 게시된 최신본과 아직 필요한 비교/납품본만 보존 대상으로 둔다. 오래된 출력·0B 표식도 현재 소유/활동과 정확한 승인 확인 후 처리한다. 오래됐다는 이유로 자동 삭제하지 않는다.
- `Wiki/site/public/`은 재생성 가능한 게시 사본이다. 게시 확인 뒤 보관 검토하되 `site/sources`, `site/media`, `site/data.*`는 미리보기/check 의존성을 확인한다. 문서의 보존 근거를 지워 위키를 맞추지 않는다.
- `Artifacts/` 하위는 추적 여부와 `Docs`, `Wiki/content`, `AGENTS.md`, `tools`의 참조를 대조한다. 미추적·참조 0개만 보관 이동 후보이며 참조된 경로·Validation·납품 영상/원본은 보호한다. 큰 원자료는 별도 목록/승인으로 처리한다.
- 위키가 참조하는 실제 파일을 정리하면 [위키 절차](Wiki_Publish.md)에 따라 관련 링크/해시를 확인한다. `Stale wiki source`는 자료 삭제와 구분한다. 같은 결과가 유효한데 정리 보고·폴더 이동만으로 전체 위키/게임 검사를 다시 하지 않는다.

English: Preserve originals, active work, current deliverables and necessary reproducible proof. Preflight free/peak space; reuse outputs and keep one task-owned lifecycle record. Verify owners/activity and exact approval before cleanup; expiry, ignore rules and old timestamps are not permission. Permanent deletion requires a concrete path/size/impact/recovery batch. Use managed archive/restore after preserving needed inputs, keep shared caches/runtimes, and distinguish APFS apparent sizes or moves from reclaimed space. Apply reference checks to wiki evidence and keep authorized checks scoped; historical cleanup events are not current instructions.
