# 커밋·푸시와 공개 위키 동기화 — 필수 (2026-10-07 개정: PR 병합 시 한 번)

> `AGENTS.md`에서 분리한 본문이다. 위키 문서·DB·생성기가 바뀌는 작업을 시작하기 전에 읽는다.

사용자가 승인한 상시 규칙이다. 위키에 새 내용이 추가되거나 기존 내용이 변경되면 **PR 병합 때 한 번** 공개 위키에 게시한다. 일반적인 위키 갱신과 공개 게시에 대해 매번 다시 허락을 묻지 않는다.

## 개정 이유 (2026-10-07)

세션 기록 집계(33개 세션)에서 `wiki.py` 실행이 약 456회(`build` 216회, 한 세션 116회), 문서·위키·산출물만 바꾼 커밋이 최근 14일 858개 중 319개(37%)였다. 위키 빌드는 한 번에 3~6분이 걸리고 공개 데이터가 약 85MB다. 단계마다 빌드·검사·테스트를 돌릴 이유가 없으므로 작업 브랜치에서는 원본 문서만 갱신하고 위키 생성·검사·배포는 병합 시점에 한 번만 한다.

## 작업 브랜치에서 하는 일

- 원본 문서·DB 근거와 필요한 기록을 갱신하고 일반 커밋·푸시를 한다. 관계없는 사용자 변경을 임의로 커밋하거나 되돌리지 않는다.
- **`wiki.py build`/`check`, `test_wiki.py`, `test_wiki_ui.cjs`는 실행하지 않는다.** 생성된 `Wiki/history/`도 단계마다 커밋하지 않는다. 문서 한 줄을 고칠 때마다 위키를 다시 만들지 않는다.
- 기록·문서 갱신은 단계마다 따로 커밋하지 말고 그 단계의 코드 커밋과 묶거나 PR당 한두 개로 모은다. 같은 문서를 여러 번 고치는 커밋을 쌓지 않는다.
- 위키 생성기(`tools/wiki.py`)나 위키 화면을 바꾸는 작업은 예외로, 바꾼 부분에 직접 관련된 `test_wiki.py`·`test_wiki_ui.cjs`를 필요한 만큼 실행한다.

## PR 병합 때 한 번 하는 일 (병합하는 사람)

1. 병합 후보(최신 `origin/main` 포함)의 변경 경로를 위키 수집 경로와 마지막 검증본의 `inputManifest`에 대조한다. 모든 변경이 위키 입력 밖임을 확인한 작업(예: `AGENTS.md`·`CLAUDE.md`·`AgentRules/`만 수정)은 생성·검사·배포를 생략하고 근거를 보고한다. 새 `Docs`·`Wiki/content` 문서, DB·에셋·연결 근거, 생성기·화면 변경은 기존 manifest에 없다는 이유로 제외하지 않는다. 영향이 있거나 불확실하면 `python3 tools/wiki.py build`와 `python3 tools/wiki.py check`, `python3 tools/test_wiki.py`와 `node tools/test_wiki_ui.cjs`를 한 번 실행한다. Node는 설치된 실행 경로를 사용한다.
2. 생성된 `Wiki/history/` 변경을 브랜치에 커밋·푸시한다(같은 본문으로 버전이 중복 생성되지 않게 한다). 이 한 번의 커밋만 위키 이력을 담는다.
3. PR을 병합한다. 병합된 트리가 검증한 트리와 같으면(`git rev-parse origin/main^{tree}` 비교) 같은 빌드를 다시 만들지 않고, 다르면 병합된 `main`에서 1단계를 다시 실행한다.
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

English: Wiki publishing happens once per merge. Work branches update source records, with focused tests for generator/UI changes. At merge, compare changed paths against collection rules and the last verified input manifest. Skip generation, tests and deployment only when all changes are demonstrably outside wiki inputs, recording the reason. Newly collected documents/assets/evidence and generator/UI changes must not be excluded merely because they are absent from an old manifest. For affected or uncertain inputs, build/check/test the merge candidate once, commit history once, merge, and rebuild only if the merged tree differs. Compare live pages, databases and input manifests before deployment. Publish only merged `main`, preserve the complete bilingual read-only copy and date-only display, honor current user scope, verify Pages and logged-out content, and report the actual result.
