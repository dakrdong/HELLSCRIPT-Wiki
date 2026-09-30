# Sites 플레이 로그 수집

갱신일: 2026-10-01

[English](Sites_Player_Logs.en.md) · [전투 기록](Combat_Journal_Server.md) · [운영 설정](Live_Operations.md)

## 역할

수집 주소는 https://hellscript-player-logs.hoosung.chatgpt.site 이다. Sites Worker가 요청을 검사하고 전투 원문은 R2에, 검색용 요약과 충돌 감사 기록은 D1에 보관한다. Google 로그인과 운영 설정은 기존 Railway 서버를 사용한다. 계정 저장의 인증 서버 기준을 유지하므로 수집 주소 변경으로 다른 캐릭터 저장을 만들지 않는다.

기존 Railway의 전투·운영·계정 DB를 삭제하거나 이전하지 않는다. 이전 기록은 Railway에 남고 새 수집 주소에서 받은 기록은 Sites에 저장된다. 로그인·운영 설정까지 옮기는 전체 서버 이전과 구분한다.

## 게임 연결

Resources/Data/ServerConnection.json 버전 2는 **baseUrl**을 인증·운영 설정에, **telemetryBaseUrl**을 로그 수집에 사용한다. 두 주소 모두 자격 증명·경로·쿼리가 없는 HTTPS 원점이어야 한다. 버전 1은 기존 단일 서버 방식으로 읽는다.

Google 로그인이 완료되어 올바른 계정 저장을 연 뒤 업로더를 연결한다. 업로더는 당시 계정 ID에 고정하며 토큰은 같은 계정의 로그인 세션 메모리에서만 읽는다. 다른 계정의 새 세션이 저장 전환 전에 준비되어도 이전 계정의 대기 기록을 보내지 않는다. 만료·로그아웃에서도 새 전송을 멈춘다. 일반 게스트와 웹 게스트판은 로컬 기록을 유지하며 자동 서버 수집을 연결하지 않는다.

Editor·개발 빌드는 별도 QA 파일을 사용할 수 있다. 파일의 **baseUrl**은 Sites 수집 주소와 일치하고 **kind**는 hellscript-qa-telemetry여야 한다. 운영자 키, QA 키, Sites 소유자의 테스트용 접근 토큰을 게임 리소스·계정 저장·Git·공개 위키에 넣지 않는다.

## 인증과 수신 확인

Sites는 Bearer 토큰을 기존 서버의 **GET /v1/telemetry/session**에서 확인한다. 서버가 확인한 google: 또는 qa: 계정 ID를 소유자로 사용하며 클라이언트의 localAccountId와 구분한다. 이메일·표시 이름은 수집 계정 ID로 사용하지 않는다. 인증 서버 오류·시간 초과·리다이렉트에서는 저장을 진행하지 않는다.

**POST /v1/combat-runs**의 원문 SHA-256을 계산한다. 동일 계정·runId·원문은 같은 ACK를 반환한다. 다른 원문은 409로 거절하고 기존 기록을 보존한다. R2 원문 저장, D1 메타데이터 확정, R2 저장 확인 뒤에만 accepted·runId·payloadHash를 반환한다. 게임은 세 값이 일치한 뒤 업로드 대기 파일만 지운다. 최근 100회 플레이어 기록은 별도로 유지한다.

파일은 16 MiB, 이벤트와 각 드랍 목록은 20,000개, JSON 깊이는 64까지다. 정수는 JavaScript의 정확한 범위인 2^53−1까지 허용한다. 계정별 한 분에 120회·64 MiB를 초과하면 429를 반환한다. 재시도와 저장 실패에서도 수신 완료를 먼저 보내지 않는다.

공개 조회·보상 지급·계정 저장 변경 API는 없다. 사이트 공개 범위와 게임 토큰 검사는 별도다. 외부 게임의 직접 연결에는 인터넷에서 접근 가능한 사이트가 필요하지만, 업로드 토큰 검사와 로그 비공개 보관은 유지한다.

## 배포와 운영

- 구현은 server/sites/_worker.js다. Python 서버를 그대로 올리지 않고 Worker로 구현한다.
- .openai/hosting.json의 동일 project_id, D1 DB, R2 LOGS를 계속 사용한다. 환경 변수·비밀값을 호스팅 파일에 넣지 않는다.
- Sites 환경 변수 HELLSCRIPT_AUTH_ORIGIN은 기존 인증 서버의 정확한 HTTPS 원점이다. 변경 후 재배포해야 한다.
- server/sites/publish.py가 이 서비스만 Sites Git에 내보내고 푸시한 전체 SHA를 확인한 뒤 Worker를 dist/index.js로 패키징한다. Unity 프로젝트와 로컬 QA 파일은 배포 압축 파일에 포함하지 않는다.
- 버전 저장과 배포는 별도이며 모든 배포 주소는 운영 주소다. 프로젝트 ID·접근 범위를 보존하고 배포 성공 상태를 확인한다.
- /healthz는 인증 원점 설정·D1 스키마 조회·R2 접근을 검사한다. 실제 인증·업로드 성공은 별도 API 검사로 확인한다.
- 첫 화면은 /healthz의 실제 결과를 한국어·영어로 표시하고 다시 확인 버튼을 제공한다. 로그인 뒤에도 남아 있던 초기 안내 문구를 교체했다. 소유자 전용 접근과 일반 게임 연결의 공개 API 승인 대기를 각각 표시한다.

[공식 Sites 문서](https://learn.chatgpt.com/docs/sites)는 D1을 사이트당 10 GB로 안내하며 R2에는 고정 저장 한도를 두지 않는다. 계정 전체의 사용 제한은 별도로 적용된다.

## 검증

Node 검사는 실제 SQLite 쿼리로 중복·충돌·원문 보존·저장 실패·재시도·토큰 철회·정수/이벤트 경계·리다이렉트 거절을 확인한다. 기존 서버 검사는 QA·Google 세션 구분과 철회, 기존 QA·운영 권한의 분리를 확인한다.

verify_cloud.py는 별도 QA 파일로 합성 원문을 보내 실제 Sites ACK와 충돌·미인증·잘못된 기록·조회 경로 거절을 검사한다. 비공개 접근 토큰은 테스트 프로세스 메모리에서만 사용한다.

RuntimeTelemetrySitesSmoke는 격리 저장의 실제 균열을 완료해 기본 업로더로 보내고, 업로더 재시작 뒤 동일 원문을 다시 보낸다. 대기 파일 제거와 로컬 기록 보존을 확인한다. 개발 검증 코드이며 모바일 실기기 검증과 구분한다.

## 구현 근거와 현재 검증 상태

[수집 서버](../../server/sites/_worker.js) · [상태 화면 코드](../../server/sites/public/status.js) · [Node 검사](../../server/sites/test_worker.mjs) · [배포 도구](../../server/sites/publish.py) · [API 검사](../../server/sites/verify_cloud.py) · [게임 연결](../../Assets/HELLSCRIPT/Runtime/Presentation/GameServerConnection.cs) · [실제 업로더 검사](../../Assets/HELLSCRIPT/Runtime/Presentation/RuntimeTelemetrySitesSmoke.cs)

- Node 10/10 통과: 원문·중복·충돌·저장 실패·인증 철회·요청 한도·리다이렉트 거절. [결과](../../Artifacts/Validation/sites-player-logs-20260930/worker-tests.txt)
- 소유자 비공개 배포의 API 10개 확인: 최초/중복 ACK 일치, 변경된 원문 409, 잘못된 관측 422, 익명/잘못된 토큰 401, 목록 405, 개별 조회 404. D1에는 합성 전투 한 행과 일치하는 SHA-256이 남았다. [결과](../../Artifacts/Validation/sites-player-logs-20260930/private-api.json)
- 공개 범위 전환과 직접 게임 업로드 검증은 소유자의 공개 API 승인을 기다린다. 비공개 API 검증을 일반 플레이어 연결 완료로 해석하지 않는다.
- Unity Edit Mode 관련 검사 41/41 통과 뒤 계정 토큰 전환 보호를 보강했다. 변경된 서버 연결 검사 11/11을 다시 통과했고, 바뀌지 않은 계정 저장 12·전투 기록 18·서버별 운영 캐시 1의 성공 결과를 재사용한다. 변경된 코드로 macOS 개발 빌드도 다시 생성했으며 오류 0으로 완료했다. 전체 Edit Mode 실행은 범위를 좁히며 종료했으므로 전체 통과 결과로 보고하지 않는다. 직접 업로드 런타임 검사와 모바일 실기기 검사는 실행하지 않았다. [검증 요약](../../Artifacts/Validation/sites-player-logs-20260930/validation-summary.json)
- 배포된 상태 화면에서 한국어·영어 전환, 다시 확인 버튼과 실제 정상 응답을 확인했다. 화면의 정상 표시는 수집 서비스·저장소의 응답이며 일반 게임 업로드 완료를 뜻하지 않는다.
- 인증·운영 서버와 Sites 검사의 [CI](https://github.com/dakrdong/HELLSCRIPT/actions/runs/36729003860)가 main의 3c33af8b에서 통과했다. [D1 저장 근거](../../Artifacts/Validation/sites-player-logs-20260930/storage-proof.json)는 합성 runId·원문 해시만 제공하며 계정 ID·토큰은 제외한다.
