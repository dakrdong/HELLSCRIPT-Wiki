# Google 로그인과 기기별 계정 저장

갱신일: 2026-09-26

[English](Google_Login.en.md) · [서버 운영](Live_Operations.md)

타이틀의 체험용 계정·비밀번호 입력을 Google 로그인으로 교체한다. 게스트 입장은 유지한다. 서버가 Google 계정을 확인한 뒤 캐릭터 선택으로 이동하며, 신규 계정의 첫 게임은 기존 튜토리얼 규칙을 따른다.

**현재 범위는 실제 계정 인증과 기기 안의 저장 분리다.** 진행 상황은 현재 기기에 저장된다. 다른 기기의 진행 상황 내려받기, 클라우드 저장, 게스트 데이터의 서버 업로드는 포함하지 않는다. Google 로그인으로 운영툴 권한이나 QA 전투 수집 권한을 부여하지 않는다.

## 플레이어 동작

1. 타이틀에서 **Google로 계속**을 누른다. Google 계정과 비밀번호는 브라우저의 Google 화면에서 입력한다.
2. 이 기기에서 처음 사용하는 계정이면 **게스트 진행 상황 연결** 또는 **새 게임으로 시작**을 선택한다. 이미 연결한 Google 계정은 해당 저장을 바로 연다.
3. 캐릭터를 선택해 게임에 들어간다. 로그아웃하면 별도의 게스트 저장으로 돌아간다.

게임을 다시 실행하면 로그인 화면에서 시작한다. 게임 세션은 메모리에만 두며 최대 12시간 유지한다. 다른 로그인으로 서버 세션이 교체되어도 진행 중인 로컬 플레이를 원격으로 중단하지 않는다. 향후 서버 기능은 매 요청마다 세션 유효성을 확인해야 한다.

## 저장 소유권

`GameController.Accounts`가 `GameStore` 전환을 맡는다. 타이틀에서 전투·진입·공통 창이 진행 중이지 않을 때만 전환한다. 기존 저장을 먼저 저장하고 새 저장도 열기와 저장에 성공한 뒤 연결을 확정한다.

`AccountProfiles`의 `account-profiles-v1.json`은 서버 주소와 불투명한 계정 ID의 해시를 로컬 저장 폴더에 연결한다. 이메일과 인증 토큰은 이 파일에 기록하지 않는다.

| 선택 | 저장 처리 |
| --- | --- |
| 기존 게스트 | 색인 파일이 없으면 이전 저장 위치를 그대로 연다. |
| 게스트 연결 | 저장 폴더의 소유 연결만 원자적으로 이전한다. 저장·전투 기록을 복사하거나 합치거나 삭제하지 않는다. 이후 게스트에는 새 폴더를 배정한다. |
| 새 게임 | 해당 Google 계정의 별도 폴더에 저장한다. 게스트 저장은 유지한다. |
| 이미 연결한 계정 | 기존 연결을 열며 다른 게스트 진행으로 덮어쓰지 않는다. |
| 잘못된 색인·저장 실패 | 진입을 중단하고 원본을 보존한다. 계정 구분을 잃은 채 게스트로 대체하지 않는다. |

색인에는 중복 계정·폴더, 경로 이탈, 누락 필드와 잘못된 버전을 허용하지 않는다. 교체 시 파일 잠금과 임시 파일, 백업을 사용한다. 언어·글자 크기·소리·화면 설정은 기존처럼 기기에 남는다. 계정 전환 시 튜토리얼·출석·미접속 보급의 화면 캐시와 전투 업로드 대상을 갱신한다.

## 인증과 서버 설정

Google OAuth 클라이언트는 **웹 애플리케이션**이며 승인된 리디렉션 URI는 `https://hellscript-production.up.railway.app/auth/google/callback`이다. 클라이언트 비밀키는 서버에만 둔다. 요청 권한은 `openid`와 `https://www.googleapis.com/auth/userinfo.email`이다. 이메일 권한의 정식 이름을 사용하며 `email`과 같은 범위다. Google Console의 게시 상태는 테스트 중이지만, 이 기본 인증 권한만 요청하는 앱은 테스트 사용자 목록·7일 승인 만료 제한의 예외다. [Google 공식 사용자 범위 안내](https://support.google.com/cloud/answer/15549945)를 따른다. 이름·로고의 브랜드 검증과 모바일 배포는 별도 절차다.

| 서버 환경 변수 | 값의 의미 |
| --- | --- |
| `HELLSCRIPT_GOOGLE_CLIENT_ID` | Google Cloud가 발급한 클라이언트 ID |
| `HELLSCRIPT_GOOGLE_CLIENT_SECRET` | 같은 클라이언트의 비밀키. Git·게임·문서에 기록하지 않는다. |
| `HELLSCRIPT_AUTH_ORIGIN` | `https://hellscript-production.up.railway.app` |

세 값이 모두 없으면 Google 로그인을 비활성화하고 게스트를 유지한다. 일부만 있거나 형식이 잘못되면 서버 시작을 거부한다. 계정 DB는 기존 영구 볼륨의 `accounts.sqlite`이며 운영 설정·전투 수집 DB와 분리한다. 계정 ID 연결은 이 DB를 복구해야 유지되므로 기존 서버 백업 절차의 대상에 포함해야 한다.

게임은 PKCE 검증값을 메모리에 보관하고 서버에서 브라우저 진입 주소를 받는다. 서버는 브라우저 쿠키와 상태값을 묶고 별도의 PKCE·nonce로 Google에 요청한다. Google 콜백에서는 공식 `google-auth`로 서명·발급자·대상 앱·만료를 검사하고 nonce를 확인한다. 이메일 대신 Google의 안정적인 `sub`를 계정 기준으로 사용한다.

브라우저가 게임에 전달하는 코드는 60초 동안 한 번만 사용할 수 있다. 게임의 원래 PKCE 검증값과 상태값이 있어야 세션으로 교환된다. Google의 액세스·갱신 토큰은 게임에 전달하거나 저장하지 않는다. 서버는 게임 세션 토큰의 해시만 저장하고 로그아웃 시 폐기한다. 인증 응답은 캐시하지 않으며 기존 서버의 접근 로그 비활성화를 유지한다.

PC는 임의 포트의 `127.0.0.1` 콜백을 사용한다. Android·iOS는 `hellscript://auth/google`로 돌아오며 `GoogleLoginBuild`가 기존 매니페스트 항목을 보존하면서 빌드에 경로를 한 번만 등록한다. 모바일 실기기와 Windows 동작은 별도 검증 대상이다.

## 화면 소유와 검증

기존 타이틀은 게임 안의 콘텐츠 창과 달리 로그인 전 화면 전체를 소유한다. 로그인 창도 이 타이틀 어댑터 안에 두고 `UiFonts`, `UiTheme`, `UiButton`과 기존 안전 영역을 재사용한다. 본문은 스크롤하며 닫기는 고정한다. Google 버튼은 [공식 표시 기준](https://developers.google.com/identity/branding-guidelines)에 따라 공식 로고와 밝은 바탕을 사용한다. 입력 상태는 공통 `UiButton`이 처리하고, 전용 표시 역할은 `UiTheme`과 `UiButtonFace`에 둔다. 영문 글꼴은 `UiFonts.GoogleSignIn`의 Roboto Medium, 한글은 기존 공용 글꼴을 사용한다. 에셋 출처와 글꼴 라이선스는 [인증 에셋 고지](../../Assets/HELLSCRIPT/Resources/Authentication/NOTICE.txt)에 기록했다. 인증은 `GoogleLoginClient`, 저장 연결은 `GameController.Accounts`와 `AccountProfiles`가 소유한다. 화면 부품이 Google 계정이나 저장 연결을 직접 만들지 않는다.

검증은 다음을 구분한다.

- 서버 단위 검사: 정상 인증 교환, 취소·만료·재사용·잘못된 PKCE·쿠키 차단, 공식 토큰 검증기의 서명·대상·발급자·만료·nonce 거부를 확인한다. Google 응답과 인증서는 테스트 자료이며 실제 계정 로그인의 증거가 아니다.
- Unity Edit Mode: 저장 소유권 이전과 재시작, 실제 `GameStore` 데이터 분리, 콜백 주소와 상태값, 타이틀 진입, 번역, 모바일 매니페스트 보존·중복 방지를 확인한다.
- `RuntimeTitleSmoke`: macOS 게임에서 세로 440×956·가로 956×440·PC 16:9·16:10·21:9, 한국어·영어, 글자 크기 100%·140%의 로그인 화면과 포인터 조작을 확인한다.
- `RuntimeGoogleLoginSmoke`: 실제 Google 로그인을 두 번 수행해 게스트 연결, 로그아웃, 새 게스트 분리, 원래 계정 복원과 실제 게임 진입을 확인한다. 인증 우회나 테스트 계정 주입 경로는 없다. `-hellscriptAuthBrowserHandoff`는 개발 빌드에서 브라우저 주소만 별도 파일로 전달한다.

[PR #12](https://github.com/dakrdong/HELLSCRIPT/pull/12)를 `main`에 병합했고, 2026-09-26에 Google 인증정보를 기존 Railway 서버에 등록했다. 이메일 권한 별칭을 Google 응답의 정식 이름과 일치시킨 `64a9a18a`를 배포했으며, 배포 `8b2ba0b3-0815-43a6-9da9-1ef3dc9bced7`의 성공을 확인했다. 실제 Google 계정으로 macOS 게임에 두 번 로그인해 게스트 연결, 로그아웃, 새 게스트 분리, 기존 진행 복원과 Mage의 게임 진입을 검증했다. [실제 인증 검증 기록](../../Artifacts/Validation/GoogleLogin/real-google/acceptance.json) · [게임 실행 결과](../../Artifacts/Validation/GoogleLogin/real-google/validation.txt)

## 이번 작업의 확인 결과

| 검사 | 결과 |
| --- | --- |
| 서버 | 80개 통과. 정식 이메일 권한 교환과 추가 권한 거부 검사를 포함하며 실제 Google 계정 검증은 아래 행과 구분한다. |
| 관련 Unity Edit Mode | 101개 통과, 실패·건너뜀 0개. 전체 프로젝트 검사 결과와는 구분한다. |
| macOS 개발 빌드 | 성공, 빌드 오류 0개. |
| 타이틀 실행 검사 | 31개 확인. 로그인 화면 20개 조합, 게스트·로그아웃·캐릭터 선택·실제 게임 진입을 포함한다. |
| 캐릭터 선택 실행 검사 | 11개 확인. 선택·저장·방향 변경·중단된 균열의 캐릭터 제한을 포함한다. |
| 실제 Google 로그인 | 공개 서버와 실제 계정으로 2회 성공했다. 게스트 연결·로그아웃·진행 복원·게임 진입을 확인했고 macOS 검사 종료 코드는 0이다. |
| Windows·Android·iOS 실기기 | 미검증이다. macOS 화면 크기 모의 검사는 모바일 실기기 증거가 아니다. |

[검증 요약](../../Artifacts/Validation/GoogleLogin/validation-summary.json) · [Edit Mode 결과](../../Artifacts/Validation/GoogleLogin/editmode-integrated.xml) · [타이틀 실행 기록](../../Artifacts/Validation/GoogleLogin/title-ui-final/validation.txt) · [캐릭터 선택 기록](../../Artifacts/Validation/GoogleLogin/character-ui/validation.txt)

![영어 140% Google 로그인 화면](../../Artifacts/Validation/GoogleLogin/title-ui-final/google-en-140-440x956.png)

![한국어 140% Google 로그인 화면](../../Artifacts/Validation/GoogleLogin/title-ui-final/google-ko-140-440x956.png)
