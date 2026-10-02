# HELLSCRIPT 웹 플레이어

갱신일: 2026-10-02 · [English](Web_Build.en.md)

웹 플레이어는 Unity 게임을 브라우저에서 실행하는 별도 배포본이다. 개인 계정이나 개발 프로젝트를 배포 폴더에 복사하지 않는다. 게임 소스는 기존 저장소에서 관리하고, 실행 파일만 `dakrdong/HELLSCRIPT-Web`에 게시한다. 기존 공개 위키는 별도 저장소를 유지한다.

공개 플레이 주소: [HELLSCRIPT Web](https://dakrdong.github.io/HELLSCRIPT-Web/). 배포 성공 여부는 아래 배포 실행 기록에서 확인한다.

## 플레이와 저장

- 웹 버전은 게스트로 시작한다. 기존 앱의 Google 로그인은 운영체제의 콜백을 사용하므로 웹 화면에서는 제공하지 않는다. Google 계정 연결이나 기기 간 저장 동기화를 제공한다고 표시하지 않는다.
- 게임 진행과 화면·언어 설정은 이 브라우저의 사이트 저장 공간에 보관한다. Unity의 `autoSyncPersistentDataPath`를 사용하므로 기존 파일 저장 거래와 백업 규칙을 그대로 거친다. 브라우저 데이터를 삭제하면 웹 저장도 삭제된다.
- 다른 탭이나 앱으로 이동할 때는 기존 전투 보존·재개 처리를 호출한다. 브라우저가 숨겨진 페이지를 멈춘 시간을 실시간 사냥으로 계산하지 않는다. 게임 안의 절전 화면을 켜고 현재 탭에 머무르는 흐름은 기존 구현을 사용한다.
- 웹 버전은 빌드에 포함된 게임 규칙을 사용한다. 앱 전용 인증과 동일 출처를 요구하는 운영 서버 API에는 연결하지 않는다. 운영 서버 보안을 변경하거나 임시 인증을 추가하지 않는다.
- Web Locks를 지원하는 브라우저에서는 같은 주소의 게임을 두 탭에서 동시에 열지 못하게 해 저장 충돌을 줄인다. 시작 화면은 저장 공간 오류와 로딩 실패를 한국어·영어로 안내한다.

## 빌드와 배포

Unity 6000.6.0f1과 설치된 Web Build Support를 사용한다.

```bash
Unity -batchmode -nographics -buildTarget WebGL -projectPath <checkout> \
  -executeMethod Hellscript.Editor.WebPlayerBuild.BuildGitHubPages \
  -hellscriptBuildOutput <output>/Web -logFile <output>/web-build.log -quit
python3 tools/package_web_build.py package <output>/Web <deployment-checkout> --revision <source-commit>
```

`WebPlayerBuild`는 출시 모드, WebAssembly DiskSizeLTO와 IL2CPP 크기 최적화, Low 코드 제거, WebGL 2, 단일 스레드와 gzip 압축을 사용한다. 정적 호스팅에서 압축 헤더를 설정할 필요가 없도록 압축 해제 코드를 포함한다. 웹 전용 텍스처 설정은 `ResourceTextureBudget`이 관리하며 기존 원본 이미지와 GUID를 유지한다. 다른 플랫폼의 설정을 대신 바꾸지 않는다. 출시 웹 빌드에서는 성능 검사 패키지가 자동 생성하는 기기 정보 파일도 제외하고, 완료된 빌드 보고서에서 다시 검사한다.

브라우저는 운영체제 글꼴에 접근할 수 없어 `UiFonts`가 웹 빌드에 미리 로드된 Nanum Gothic을 사용한다. Google Fonts의 나눔고딕 원본과 SIL Open Font License 1.1을 `Assets/HELLSCRIPT/ThirdParty/Fonts/`에 보관한다. 글꼴은 `Resources` 밖에 두어 기존 네이티브 빌드의 용량을 늘리지 않는다. 공개 배포에는 `ThirdPartyNotices.txt`를 함께 제공한다.

`Assets/WebGLTemplates/HELLSCRIPT/`가 반응형 로딩 화면과 브라우저 수명 주기를 담당한다. 게임 내부의 캔버스·안전 영역·전투 로직은 기존 소유자를 그대로 사용한다. 현재 UI는 기본 글자 크기를 사용한다. 창 비율 선택은 브라우저 창의 해상도를 강제 변경하지 않고 기존 안전 영역에 적용한다.

배포 체크아웃의 `assemble_web.py`는 `tools/package_web_build.py`와 같아야 한다. 워크플로는 `tools/web_pages_workflow.yml`을 `.github/workflows/pages.yml`로 복사한다. GitHub Pages의 배포 방식은 GitHub Actions다. 큰 실행 파일은 48 MiB 이하의 조각으로 Git에 저장하고, Actions에서 크기와 SHA-256을 확인한 뒤 원래 파일로 복원한다. 플레이어가 브라우저에서 실행할 때는 정상적인 Unity 빌드 파일을 받는다.

## 안드로이드 APK

웹 빌드를 검증한 뒤 같은 `main`의 기존 `ProjectBuilder.BuildAndroid`로 설치용 APK를 만든다. 별도 개발용 플래그 없이 릴리스 플레이어와 LZ4HC 압축을 사용한다. 현재 프로젝트의 앱 ID와 서명 설정은 유지한다.

```bash
Unity -batchmode -nographics -buildTarget Android -projectPath <checkout> \
  -executeMethod Hellscript.Editor.ProjectBuilder.BuildAndroid \
  -hellscriptBuildOutput <output>/HELLSCRIPT.apk -logFile <output>/android-build.log -quit
```

APK 생성 성공, 패키지·서명 검사와 실제 안드로이드 기기의 설치·플레이 검증은 구분한다.

## 통합 메인 공개 웹 배포 — 2026-10-02

병합된 `main`의 `fc2ac18157e96765199fcb7437b60e1c6af14631`을 빌드 시작 시 고정하고 별도 복사본에서 출시 WebGL을 생성했다. 기존 `WebPlayerBuild.BuildGitHubPages`와 패키징 도구를 재사용했으며 원본 체크아웃·저장·패키지 설정을 보존했다. [빌드 결과](WebBuildEvidence20261002/build.json)는 오류 0개·종료 코드 0·296.18초이다. 공개 파일 7개, 총 180,815,739바이트를 48 MiB 이하의 조각 10개에서 복원해 원본 SHA-256과 전부 일치함을 확인했다. [패키지 검증](WebBuildEvidence20261002/package-validation.json).

웹 패키지 검사 5개와 로더 검사 5개가 통과했다. 공개 배포 커밋 `04416f6be197dee63a207a59e22855d845d92528`의 [Pages 실행 36967964914](https://github.com/dakrdong/HELLSCRIPT-Web/actions/runs/36967964914)이 성공했다. 실제 공개 페이지가 이번 빌드의 로더 파일을 불러오는 것도 확인했다.

macOS 내장 브라우저에서 게스트·전사·마을 진입과 균열 입장 화면을 실제 조작했다. 한국어·영어 각각 세로 440×956, 가로 956×440, PC 1280×720·1440×900·1680×720에서 기본 글자 크기의 배치를 확인했다. 영어 저장 후 새로고침과 재진입에서 언어·전사·마을·물약·사용 스킬이 복원됐으며 검증 후 한국어로 되돌렸다. 실행 오류는 0개이다. 기존 URP FSR 미지원 셰이더 경고가 게임 시작마다 한 번씩, 두 번의 실행에서 총 2건 남아 있다. [검증 요약](WebBuildEvidence20261002/validation.json).

이번 검증 범위는 출시 웹 빌드·패키지·공개 실행·균열 입장 표시와 기존 저장 복원이다. 게임 코드 변경 없이 동일 소스를 빌드했으므로 전체 Edit Mode·네이티브 스모크를 반복하지 않았다. 전체 전투 거래·보상·성능과 Android·iOS 실기기는 이번에 검증하지 않았으며 APK도 생성하지 않았다.

![공개 웹 균열 입장 한국어 PC](WebBuildEvidence20261002/rift-entry-ko-pc.jpg)

![공개 웹 균열 입장 영어 세로](WebBuildEvidence20261002/rift-entry-en-portrait.jpg)

## 일일 퀘스트 공개 웹 배포 — 2026-10-01

스크롤 없는 일일 퀘스트를 빌드 시작 시 병합된 `main`의 `74a2477d6b454875020cc788982a218a9c7847fb`에서 출시 WebGL로 생성했다. 기존 `WebPlayerBuild.BuildGitHubPages`와 패키징 도구를 사용하고 원본 체크아웃과 저장을 보존했다. 빌드 중 별도 작업으로 추가된 이후 아트·위키 변경은 이 배포 소스에 포함하지 않는다.

[빌드 결과](DailyQuestWebEvidence20261001/build.json)는 오류 0개·종료 코드 0이다. 공개 파일 7개, 총 176,376,654바이트를 조각 패키지에서 복원하고 원본 SHA-256과 전부 일치함을 확인했다. [패키지 검증](DailyQuestWebEvidence20261001/package-validation.json). 공개 배포 커밋 `17035f7d14723b933b6cc43c776d151772358500`의 [Pages 실행 36858217920](https://github.com/dakrdong/HELLSCRIPT-Web/actions/runs/36858217920)이 성공했으며 실제 공개 루트 HTML도 빌드 파일과 SHA-256이 같았다. 내장 브라우저의 `build-info.json` 직접 열기는 차단되어 그 엔드포인트의 라이브 열람은 검증으로 집계하지 않았다.

공개 사이트에서 기존 게스트로 실제 게임에 진입해 일퀘 한국어·영어 12개 화면(세로 440×956, 가로 956×440, PC 16:9·16:10·21:9, 작은 가로 640×360)을 기본 글자 크기로 확인했다. 다섯 카드와 하단 행동이 스크롤 없이 보이고 드래그 뒤 위치가 유지됐다. 출석 왕복·닫기·뒤로가기·새로고침, 영어 설정 저장 후 페이지 재시작과 기존 마을·캐릭터·일퀘 상태 복원을 확인한 뒤 한국어를 다시 저장했다. [일퀘 상세와 캡처](Daily_Quests.md) · [검증 요약](DailyQuestWebEvidence20261001/validation.json).

브라우저 실행 오류는 0개이고 기존 URP FSR 셰이더 경고 2건은 남아 있다. 일퀘 소유 코드·거래·데이터가 네이티브 검증본과 같아 개별 수령 100개·저장 실패·자정·안전 영역·추가 상태와 전체 Edit Mode 결과를 재사용했다. 공개 게스트에서 달성 보상을 새로 수령하거나 전체 게임 검사·네이티브 스모크를 반복하지 않았다. Android 연결 기기가 없고 iOS 검사 도구도 없어 실기기 검증은 미실시이며, 새 APK는 생성하지 않았다. 이 기록은 macOS 내장 브라우저 조작 결과다.

## 이전 검증 기록 — 2026-09-28

2026-09-28, 통합된 `main`을 기준으로 Unity 6000.6.0f1에서 생성했다. [검증 요약](WebBuildEvidence/validation.json)에 결과와 APK 해시를 기록했다.

| 항목 | 결과 |
| --- | --- |
| Unity Edit Mode | 리소스·한글 폰트·타이틀·번역 검사 59개 통과 |
| 공통 UI·배포 파일·웹 로더 | 각각 9개·5개·5개 통과 |
| 웹 빌드 | 릴리스, 오류 0건, 전송 파일 합계 163,844,655바이트(약 164 MB) |
| 웹 실제 조작 | 게스트 시작, 전사 선택, 튜토리얼 전투·아이템 획득·장착, 마을 진입 확인 |
| 웹 저장 복원 | 갑옷 장착 후 새로고침·재진입해 같은 착용품과 방어도 92 복원 확인 |
| 화면·언어 | 세로 440×956, 가로 956×440, PC 1280×720·1440×900·1680×720 확인. 한국어·영어와 읽기 크기 140% 확인 |
| macOS 호환성 | 네이티브 플레이어 기동, 효과음 283개·클립 454개, 물약 잘라내기·월드 텍스처 검사 통과 |
| APK | 릴리스, 오류 0건, 142,832,282바이트(약 143 MB), ARM64, Android 8.0(API 26) 이상, 대상 API 36 |
| APK 서명 | 기존 Android Debug 인증서의 APK v2 서명 검증 통과. 직접 설치용이며 스토어 배포 서명은 별도 |

웹 콘솔의 실행 오류는 없었다. Unity URP의 FSR 업스케일링 셰이더 미지원 경고는 남아 있으며, 위 화면과 조작은 실제 렌더링으로 확인했다. 첫 웹 빌드는 Unity Bee의 내부 그래프 갱신 횟수 제한으로 종료됐고, 생성된 캐시를 사용한 재실행은 소스 수정 없이 성공했다.

공개 웹 배포 커밋과 성공 여부는 [배포 실행 기록](https://github.com/dakrdong/HELLSCRIPT-Web/actions) 및 공개 사이트의 `build-info.json`으로 확인한다. 브라우저 검증은 macOS 내장 브라우저 결과다. Android·iOS 실기기 설치·플레이·성능은 이번에 검증하지 않았다.

![웹 한국어 타이틀](WebBuildEvidence/web-title.png)

![웹 전투와 하단 HUD](WebBuildEvidence/web-combat.png)

![새로고침 후 복원된 장착 갑옷과 방어도 92](WebBuildEvidence/web-save-restored.png)
