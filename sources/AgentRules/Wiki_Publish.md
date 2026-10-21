# 커밋·푸시와 공개 위키 동기화 — 필수 (2026-10-18 개정: 검증 결과 재사용)

> `AGENTS.md`에서 분리한 본문이다. 위키 문서·DB·생성기가 바뀌는 작업을 시작하기 전에 읽는다.

사용자가 승인한 상시 규칙이다. 위키에 새 내용이 추가되거나 기존 내용이 변경되면 **PR 병합 때 한 번** 공개 위키에 게시한다. 일반적인 위키 갱신과 공개 게시에 대해 매번 다시 허락을 묻지 않는다.

## 개정 이유 (2026-10-07)

세션 기록 집계(33개 세션)에서 `wiki.py` 실행이 약 456회(`build` 216회, 한 세션 116회), 문서·위키·산출물만 바꾼 커밋이 최근 14일 858개 중 319개(37%)였다. 위키 빌드는 한 번에 3~6분이 걸리고 공개 데이터가 약 85MB다. 단계마다 빌드·검사·테스트를 돌릴 이유가 없으므로 작업 브랜치에서는 원본 문서만 갱신하고 위키 생성·검사·배포는 병합 시점에 한 번만 한다.

## 작업 브랜치에서 하는 일

- 원본 문서·DB 근거와 필요한 기록을 갱신하고 일반 커밋·푸시를 한다. 관계없는 사용자 변경을 임의로 커밋하거나 되돌리지 않는다.
- **`wiki.py build`/`check`, `test_wiki.py`, `test_wiki_ui.cjs`는 실행하지 않는다.** 생성된 `Wiki/history/`도 단계마다 커밋하지 않는다. 문서 한 줄을 고칠 때마다 위키를 다시 만들지 않는다.
- 기록·문서 갱신은 단계마다 따로 커밋하지 말고 그 단계의 코드 커밋과 묶거나 PR당 한두 개로 모은다. 같은 문서를 여러 번 고치는 커밋을 쌓지 않는다.
- 위키 생성기(`tools/wiki.py`)나 위키 화면을 바꾸는 작업은 예외로, 바꾼 부분에 직접 관련된 `test_wiki.py`·`test_wiki_ui.cjs`를 필요한 만큼 실행한다.
- 재사용 도구 수정은 `python3 tools/test_wiki_verify.py`의 독립 회귀 검사로 확인한다. 실제 전체 생성·위키 검사는 병합 후보에 남겨 둔다.

## 반복을 막는 기본 명령

- 먼저 `python3 tools/wiki.py verify --status`로 필요한 단계만 조회한다. 파일을 만들거나 검사를 실행하지 않으며, 재사용 가능하면 종료 0, 필요한 단계가 있으면 1, 도구 오류면 2다. 작업 브랜치에서 이 조회는 가능하지만 전체 실행은 병합 담당만 한다.
- 병합 후보 검증은 **`python3 tools/wiki.py verify` 한 번**으로 한다. 내부 순서는 기존 `build` → `check` → Python 회귀 → UI 회귀다. 따로 네 명령을 다시 실행하지 않는다. 설치된 Node가 PATH에 없으면 `--node /설치된/실행파일`을 준다.
- `Builds/Validation/Wiki/`의 단계별 성공 기록·전체 로그와 실제 생성 파일을 재사용한다. `--receipt <작업 경로>/receipt.json`으로 한곳에 모을 수 있다. 커밋 번호 대신 실제 입력·새 파일 목록·이력·도구·Python/의존 패키지/Node 환경·결과 파일·로그 해시를 확인한다. 미커밋 변경, 새 문서·에셋·보존 검사 보고서, 파일 삭제·손상이 있으면 관련 단계를 다시 수행한다. 타임스탬프만으로 이전 호출 결과를 재사용하지 않는다. 한 호출 안에서 변하지 않은 파일의 중복 해시 계산만 메모리에 재사용한다.
- 실패하면 즉시 멈춘다. 오류를 고친 뒤 같은 명령을 실행하면 여전히 유효한 성공 단계는 건너뛰고 필요한 단계부터 이어간다. 성공 결과를 무효화할 새 근거가 있을 때만 `--force`를 쓴다. 같은 실패를 입력 변경 없이 반복 호출하거나 검사 성공 뒤 관례적으로 재실행하지 않는다. 같은 체크아웃의 중복 실행은 잠금으로 차단한다.
- 생성이 필요해도 내용이 같고 원본과 독립적인 `site/sources` 파일은 다시 복사하거나 임시 파일을 만들지 않는다. 바뀐 파일은 기존 원자적 교체·내용 확인을 유지하며, 심볼릭 링크·원본과 같은 hard link는 재사용하지 않는다.
- 긴 데이터·문서·로그 전체를 대화에 출력하지 않는다. 기본 출력은 단계별 실행/재사용/성공과 실패 끝부분뿐이며, 전체 로그는 파일로 읽는다. `data.json` 전체를 모델에게 읽히지 말고 필요한 문서 ID·DB·입력 해시와 차이만 프로그램으로 추출한다. 판정 요약과 근거 경로를 기존 기록 한곳에 남긴다.
- 게시 저장소를 최신 원격 상태로 확인한 뒤 `--published-site <배포 저장소 경로>`를 함께 주면 **전체 공개 파일의 내용과 추가·삭제**를 비교해 `DEPLOY: unchanged; skip` 또는 `needed`를 출력한다. 루트의 `.git`, `README.md`, `.nojekyll`, `.wiki-generated`만 비교에서 제외하며 하위 경로의 동명 파일은 비교한다. 이 옵션은 네트워크·푸시·Pages·공개 브라우저 확인을 수행하지 않는다. 오래된 로컬 배포본만으로 공개 상태를 단정하지 않는다.
- 이 도구는 게시 권한·main 경계·필수 검사·한영 전체 내용·이력 보존을 바꾸지 않는다. 기존 CI를 건너뛰거나, 이전 실패/미완료 기록을 성공으로 취급하거나, 최신 공개 반영 확인을 생략하지 않는다. 관련 소스와 결과가 같으면 병합·빠른 전진·최종 보고를 이유로 같은 검사를 다시 하지 않는다.

## PR 병합 때 한 번 하는 일 (병합하는 사람)

1. 병합 후보(최신 `origin/main` 포함)의 변경 경로를 위키 수집 경로와 마지막 검증본의 `inputManifest`에 대조한다. 모든 변경이 위키 입력 밖임을 확인한 작업(예: `AGENTS.md`·`CLAUDE.md`·`AgentRules/`만 수정)은 생성·검사·배포를 생략하고 근거를 보고한다. 새 `Docs`·`Wiki/content` 문서, DB·에셋·연결 근거, 생성기·화면 변경은 기존 manifest에 없다는 이유로 제외하지 않는다. 영향이 있거나 불확실하면 `python3 tools/wiki.py verify`를 한 번 실행해 필요한 생성·검사·회귀만 수행한다.
2. 생성된 `Wiki/history/` 변경을 브랜치에 커밋·푸시한다(같은 본문으로 버전이 중복 생성되지 않게 한다). 이 한 번의 커밋만 위키 이력을 담는다.
3. PR을 병합한다. 병합된 트리가 검증한 트리와 같으면(`git rev-parse origin/main^{tree}` 비교) 같은 빌드를 다시 만들지 않는다. 다르면 바뀐 경로를 확인하고 병합된 `main`에서 1단계를 적용한다. 생성 이력 커밋이나 위키 입력 밖 변경만으로 유효한 검증 결과를 버리지 않는다.
4. 라이브 `data.json`과 빌드 결과를 쪽수·DB·`inputManifest` 단위로 비교한다. **내용 차이가 없으면 배포하지 않고** 그 사실을 보고한다. 루트 지침도 문서의 연결 근거로 수집될 수 있으므로 파일명만으로 위키 입력 밖이라고 판단하지 않는다.
5. 차이가 있으면 검증한 `Wiki/site/public/` 전체를 별도 배포 저장소(`dakrdong/HELLSCRIPT-Wiki`, `main` 브랜치) 체크아웃에 동기화해 커밋·푸시한다. 삭제된 생성 파일도 반영하고 `.git`·`README.md`·`.nojekyll`·`.wiki-generated`는 루트 기준(`/`)으로 보존한다. 원본 프로젝트 전체를 배포 저장소에 복사하지 않는다.
6. 배포 커밋에 대한 GitHub Pages 성공을 확인하고, 로그인하지 않은 상태에서 공개 주소(`https://dakrdong.github.io/HELLSCRIPT-Wiki/#/tree`)의 변경 문서와 관련 DB를 연다. UI가 바뀌면 날짜 표시와 쓰기 조작의 부재도 확인한다.
7. 최종 보고에 공개 주소와 반영 결과(또는 배포하지 않은 이유)를 적는다. 로컬 생성이나 Git 푸시만 끝난 상태를 공개 반영 완료로 보고하지 않는다. 인증·네트워크·배포 오류로 게시하지 못했다면 원본 푸시와 공개 배포의 상태를 구분하고 미반영 내용을 명시한다.

## 유지되는 원칙

**게시는 병합된 `main`에서만 한다.** 작업 브랜치에서 게시하지 않는다(2026-09-18 결정). 그 전에는 각자 작업 브랜치에서 게시해 하루 사이에 공개본이 세 번 덮였고, 문서가 링크하는 `sources/`가 어느 커밋에도 없는 조합으로 되돌아갔다. 쓰는 쪽을 `main` 하나로 두면 게시가 쌓이며 문서와 근거가 같은 커밋을 가리킨다. 사용자가 병합 전 게시를 직접 요청한 경우만 예외이며, 이때도 최신 `origin/main`을 먼저 합친다.

- 적용 범위는 문서 추가·수정·삭제, 문서 이력, 태그, DB 내용, 이미지·첨부 자료, 위키 화면과 생성기 변경이다. 게임 코드·에셋 변경으로 위키 DB가 달라지는 경우도 포함한다.
- 공개 대상은 `dakrdong/HELLSCRIPT-Wiki` 저장소의 `main` 브랜치이며 GitHub Pages는 브랜치 루트를 게시한다.
- 공개 위키는 개발 위키 전체의 읽기 전용 사본이다. 문서·이력·태그·DB·이미지·연결된 원본 근거를 동일하게 제공하며 별도 소개 페이지나 일부 문서로 축소하지 않는다.
- 공개 화면의 페이지 등록일·변경일과 버전 날짜는 한국 시간 기준 **연·월·일까지만** 표시한다. 정렬을 위한 원본 시각과 인용문·본문·로그의 원문은 보존한다.
- 공개 위키는 **View 권한만** 제공한다. 검색·필터·정렬·상세 조회·이력 비교·다운로드는 유지하되 생성·편집·삭제·저장·업로드·게임 반영 기능 및 쓰기 API는 제공하지 않는다. 화면에서 버튼만 숨기는 것으로 끝내지 않고 실제 쓰기 경로를 제외한다.
- 이 규칙은 작업 절차에 대한 의무이며 자동 훅이 설치되어 있다는 뜻이 아니다. 병합하는 에이전트가 공개 배포와 확인까지 완료해야 한다.

English: Publish once per merge. Use `python3 tools/wiki.py verify --status` for a read-only plan (exit 0 reusable, 1 needed, 2 error), and `python3 tools/wiki.py verify` once on the merge candidate instead of repeating build/check/Python/UI commands. Per-stage receipts in `Builds/Validation/Wiki/` pin actual source bytes, newly discovered optional files, history, tooling, runtime/package versions, generated artifacts and full logs. Unchanged successful stages resume across failures; missing/corrupt proof cannot count as passed. Hash memoization lasts only within one invocation, and a checkout lock rejects overlapping runs. Use `--receipt` for task-owned evidence, `--node` for an installed runtime and `--force` only with a new reason to invalidate success. Keep verbose output on disk and extract narrow programmatic differences rather than feeding entire datasets or logs into chat. `--published-site` compares every public payload byte and file addition/deletion against a separately refreshed local deployment checkout, excluding only protected root files; it neither contacts the network nor proves Pages deployment. Test reuse logic with the independent `tools/test_wiki_verify.py` fixtures. Compare changed paths and previous manifests before generation; new collected inputs are not excluded by absence from an old manifest. Retain mandatory checks, complete bilingual read-only content, immutable history, date-only display, publication from merged `main`, Pages success and anonymous browser verification. Identical relevant inputs and output proof survive history-only commits and unrelated merge changes.
