# 2026-09-30 전체 최적화·대장간·일일 퀘스트 통합 검증

갱신일: 2026-10-01

게임 기능과 최적화를 실제 main에 통합했고 마지막 코드 변경 후 전체 EditMode·native·개발/출시 빌드 검증을 끝냈다. 새 회귀는 없으며 기존47개 실패는 아래에 명시한다. 서버 수집 사이트는 2026-10-01 별도 승인 후 공개 API 전환과 실제 QA 게임 업로드 검증을 완료했다.

## 보존과 변경 범위

최초 기준은 로컬 `4bd93ff39f545ad292b1cd21b99b1275586c064e`였다. 원본의 미커밋 상태를 기록하고 별도 최적화/통합 작업 폴더를 사용했다. 이후 계정·서버·Sites·대장간의 다른 작업 변경을 자기 변경으로 간주하지 않았으며, 사용자 확인으로 Claude 대장간 완료 후 통합했다. 최신 기존 main `94075e05`의 Sites/계정 변경은 ancestry로 식별해 한 번만 포함했다. reset/clean·강제 체크아웃/푸시·worktree/branch 삭제는 하지 않았다.

핵심 변경 커밋은 `69b37249`(월드/외곽선/스탯/구 룬 리소스), `a4b577a8`(대장간 할당), `946c3eaa`(일일 퀘스트)다. 완료된 Claude `c92a7022`와 Sites `3c33af8b`를 보존한다. 개발 전용 실행 fixture는 실제 런타임 입장·강화 단위·레이캐스트 렌더 시점에 맞춰 후속 수정했다. 마지막 게임 코드 커밋 `ebea961a`는 자정의 일일 페이지가 기존 두 출석 팝업을 이미 본 것으로 처리하지 않도록 수정하고, schema18 보존 검사의 기대 버전을 현재 schema20으로 갱신했다. 장비·균열·자동화 보존 단정문은 유지했다.

- `WorldView.Actions`, `WorldView`: live ID 집합과 재사용 제거 버퍼로 반복 정리한다. 지연/숨김 항목도 live set에 먼저 포함해 존재하는 객체를 제거하지 않는다. 중단·반복과 visibility 복원을 검사한다.
- `EnemyCombat.CopyOutline`, `WorldView.Enemies`: 스레드 전역 버퍼 대신 뷰별 49/17점 버퍼를 사용한다. 기존 배열 반환 API는 유지하고 점 순서/수학/크기를 비교했다.
- `Attributes`의 `StatCatalog.Combine`: 빈번한 2/10 인자 경로의 params 배열을 피하며 순서별 clamp와 NaN/Infinity 동작을 보존했다.
- 구 128px 무기 그림 6개와 `.meta`를 Resources 밖 `Art/Runes/V13/LegacyWeapons`로 이동했다. 소스 바이트/GUID를 보존하고 GUID·문자열/동적 Resources 경로·저장/프리셋·빌드/에디터 소비를 확인했다. 게임이 사용하는 512px Weapons 리소스는 그대로다. 삭제나 해상도 일괄 하향은 없다.
- 대장간 `BlacksmithWindow`: ticker 스냅샷 List 재사용, finally 정리로 콜백의 등록/해제 의미를 유지한다. `BlacksmithWindow.Slots`: 고정 마일스톤 목록을 공유한다. `ForgeMilestoneRing`: segment마다 Vector2 배열을 만들지 않고 같은 네 점을 직접 전달한다.
- 신규 [일일 퀘스트](Daily_Quests.md): 성공한 다섯 활동, 계정 저장·수령 거래, 하루 정확히 100 심연 주화, 출석과 같은 KST 자정, 공통 이벤트 UI와 KO/EN을 추가했다. 기존 밸런스/가격/해금과 지갑을 유지한다.

## 동일 조건 용량

아래는 **순수 핵심 최적화만** 적용한 전후 빌드다. 새 대장간/Sites/일일 콘텐츠가 추가된 최종 앱을 이 감소율로 설명하지 않는다. Unity6000.6.0f1, macOS Mono, 같은 장면과 빌드 함수/옵션, delivered 실제 파일 바이트 합계다. Library나 프로젝트 원본 크기가 아니다.

| 측정 | 전 | 후 | 감소 |
|---|---:|---:|---:|
| Development 앱 | 567,559,679B | 567,485,532B | 74,147B / 0.013064% |
| Release 앱 | 257,255,161B | 257,189,471B | 65,690B / 0.025535% |
| packed assets | | | 99,008B |
| build texture bytes | | | 104,352B |

이동한 여섯 소스 PNG의 합계 212,212B는 프로젝트에 보존돼 있다. packed/texture/delivered는 서로 다른 집계이므로 서로 합산하지 않는다. 소스/옵션/집계 요약은 [Development](OptimizationEvidence20260930/development-build-comparison.json), [Release](OptimizationEvidence20260930/release-build-comparison.json), [이동 manifest](OptimizationEvidence20260930/legacy-rune-moves.json)에 있다.

최종 새 콘텐츠 포함 앱은 Development **567,685,593B**, Release **257,269,998B**로 초기 기준보다 각각 +125,914B / +14,837B다. 새 대장간·Sites·일일 콘텐츠를 포함한 별도 결과이며 순수 최적화 감소율과 구분한다. 두 빌드 모두0errors다. [최종 빌드와 소스](OptimizationEvidence20260930/final-builds.json).

## 실제 native 성능

핵심 최적화 전후 각각 3회의 순차 native Development 실행을 비교했다. Apple M3 Pro/Mac15,7, Unity6000.6.0f1 Mono, PC 품질 1280×720, target60/vsync0, 동일 합성 stage30 Mage/seed93171/장비 RNG9132026/저장 SHA와 LiveOps/map hash, 3초 준비 후 20초 표본이다. 각 회의 평균 등 지표에서 세 회 중앙값을 비교했다. 이후 대장간·Sites·일일 퀘스트 추가까지 포함한 성능 개선 주장으로 확장하지 않는다.

| 지표 | 전 | 후 | 변화 |
|---|---:|---:|---:|
| World.Present 평균 | 465,678.254ns | 434,400.020ns | -6.7167% |
| Combat.Tick 평균 | 848,540.387ns | 752,190.292ns | -11.3548% |
| GC Allocated In Frame 평균 | 193,151.533B | 189,537.327B | -1.8711% |
| 프레임 p50 | 16.73306ms | 16.70258ms | -0.1821% |
| 프레임 p95 | 21.57783ms | 21.33321ms | -1.1337% |
| 최대 RSS 중앙값 | 621.15625MiB | 630.56250MiB | **+1.5143%** |

프레임은 60 cap의 영향을 받으므로 FPS 상승이나 무제한 프레임·배터리 개선을 주장하지 않는다. RSS는 프로세스 상주 메모리이며 Unity 할당 바이트와 다르다. RSS는 증가했고 전체 메모리 절감을 확인한 결과가 아니다. GPU/render-thread 및 draw/batch 카운터는 이 player에서 유효하게 얻지 못했다. 모바일 실기기·열·자연 진행 분포는 미측정이며 호스트 작업/스케줄링 잡음과 짧은 표본 한계가 있다. [요약과 원본 SHA](OptimizationEvidence20260930/native-profile-summary.json)를 참고한다.

## 할당 micro와 동등성

Unity Recorder에서 양수 할당을 만들고 카운터 동작을 확인한 **할당 이벤트 수**다. 바이트 수나 전체 프레임 결과로 해석하지 않는다.

| 경로 / 횟수 | 전 할당 이벤트 | 후 | 전 → 후 시간 |
|---|---:|---:|---:|
| 외곽선 / 60,000 | 370,000 | 0 | 244.0278 → 169.036ms |
| Combine / 200,000 | 200,000 | 0 | 26.8665 → 6.6243ms |
| 대장간 링 / 1,000 | 48,000 | 0 | 31.5021 → 32.7178ms |
| ticker / 10,000 | 10,000 | 0 | 3.2377 → 3.0395ms |
| milestones / 100,000 | 100,000 | 0 | 8.0396 → 0.6553ms |

링 CPU 시간은 조금 늘었으며 CPU 개선이라고 보고하지 않는다. 세 mesh 원본 바이트와 101개 마일스톤 값/활성 상태의 독립 원본 비교는 정확히 일치했다. 월드 100 projectile/25 trap 정리 모델은 5,375 equality 비교와 임시 keys 배열2개를 125 hash lookup+125 add와 임시 keys 배열0개로 바꿨다. 이 값은 모델의 탐색 횟수이며 player CPU 호출 수 실측이 아니다. [대장간 micro 근거](OptimizationEvidence20260930/forge-micro-comparison.json).

## 검사 상태

- 기준 실패: 오래된 기록을 그대로 쓰지 않고 기준 소스의 해당47개를 다시 실행해 같은47개 실패를 확인했다. [정확한 이름](OptimizationEvidence20260930/known-failures-47.names.txt).
- 일일 추가 전 통합 전체 EditMode: **4,904 total / 4,857 passed / 기존47 failed / 0 skipped**, 2026-09-30 16:38–17:08UTC, 약29.8분. 기존 실패 이름과 정확히 같으며 새 회귀0. 이 결과를 일일 포함 최종 검사라고 표기하지 않는다.
- 핵심 집중158/158, 대장간 통합 집중200/200, 일일/출석/강화/판매/물약/번역 집중170/170, UI contract11/11 통과.
- 마지막 게임 코드/main 통합 이후 일일 포함 최종 전체 EditMode: **4,931 total / 4,884 passed / 기존47 failed / 0 skipped**, 2026-09-30 19:06:33–19:35:40UTC, 1,747.237초. 새 실패0이며 신선한 기준47개 재현과 이름이 정확히 같다. [최종 요약](OptimizationEvidence20260930/final-editmode-summary.json), [원본 XML](OptimizationEvidence20260930/optimization-20260930-final-editmode.xml).
- 최종 native **12/12 smokes / 16/16 launches passed**와 독립 근거검사 passed. 실제 레이캐스트/드래그, 기본 글자 크기 KO/EN 12화면 조합, 일일 초기28.7초/새 프로세스4.8초, 출석56.5초/새 프로세스6.5초를 포함한다. 자정 팝업 보존·중복/100개·clock rollback·빈 저장 실패·재시작, 대장간4smokes와 월드/효과/반복·중단을 검사했다. [실행 결과](OptimizationEvidence20260930/final-native-results.json), [근거 대조](OptimizationEvidence20260930/final-native-verification.json).
- 최종 자정/UI 및 스키마 보존 집중78/78 passed. 중간 전체의48번째 실패는 schema19를 기대한 구 테스트였고 current schema20으로 정정한 뒤 전체에서 통과했다. 수정 전48개 원본도 보존해 검증 DB에서 조회할 수 있다.
- 일일 이전 native: 9/10smokes,11/12launches 통과. BattleLayout는 fresh tutorial map의 fixture에서 벽을 찾지 못해 실패했고 기준 player에서도 같은 실패를 재현했다. 실제 정상 맵·비동기 입장에 맞춘 fixture는 이후 통과했다. 신규 Daily fixture도 실제 +1강화 및 렌더 후 포인터 타이밍으로 교정하며 검사 조건을 유지한다.
- 서버: 선언된 요구사항을 갖춘 독립 Python3.12 환경82/82, Sites worker10/10, 운영 웹15/15 통과. 시스템Python3.9의 최초 시도는 CacheControl 미설치 오류와 gunicorn skip이었으며 코드 회귀로 계산하지 않는다. 전역 환경/서버 요구사항을 바꾸지 않았다.

## Sites와 보류

최적화 검증 당시 `hellscript-player-logs.hoosung.chatgpt.site` v7/source `40439a3b`의 배포 성공과 소유자 전용 범위를 읽기 확인했다. 이후 2026-10-01 사용자 승인에 따라 별도 Sites 작업에서 공개 API로 전환하고, 사이트 접근 토큰 없이 API 10개 및 실제 macOS QA 게임 업로드·중복 처리·로컬 기록 보존을 검증했다. 이 검증은 위의 최적화 12개 스모크와 별도로 수행했으며 동일 게임 소스의 최종 개발 빌드를 재사용했다. Bearer/비공개 로그 저장소와 Railway 인증·운영·DB는 유지한다. [현재 Sites 상태와 근거](Sites_Player_Logs.md)를 따른다.

원본 main의 게임 코드 기준은 `ebea961a937c4c5ed59791f46c73683a7d33d62b`다. 여섯 로컬 브랜치와 세 실제 원격 브랜치의 변경은 모두 main의 조상이며 source175경로의 사전/사후 hash와 6PNG/meta/GUID를 확인했다. 원본과 독립 검사 worktree의 Assets/Packages/ProjectSettings5244개 파일이 일치하며 기능/최적화가 worktree에만 남은 것은 없다. [게임 적용 경로](OptimizationEvidence20260930/main-source-applied-paths.txt). 문서·위키 커밋은 이 게임 기준 위에 쌓이며 게임 소스를 바꾸지 않는다. 마지막 원격 main SHA/CI/Pages·비로그인 브라우저 반영 증거는 로컬 `evidence/FINAL-REPORT.md`에 기록한다. 서버tree는 기존 원격94075와 같아 서버 경로 필터 CI의 새 run을 만들어 통과했다고 세지 않는다. 미측정 대규모 텍스처 일괄 압축/해상도 하향, 대장간 JobSignature/CoreQualityGauge 재설계, 무제한 FPS·모바일 배터리 검증, 실제 Google로그인/실사용 로그 전송은 하지 않았다. 품질·밸런스·저장 호환성을 바꿀 근거가 부족하거나 공개 승인을 기다리는 범위다.

## 복구·재현

원본 프로젝트·독립 worktree와 커밋을 모두 보존한다. 구 룬은 위 manifest의 GUID/원본 바이트를 유지한 채 Unity 이동으로 Resources 원래 위치에 되돌릴 수 있다. 최적화 복구는 `69b37249`/`a4b577a8`의 검토된 역변경 커밋으로 처리하며 reset/clean을 쓰지 않는다. 일일 스키마20 저장은 보존하고 코드 롤백으로 스키마를 낮추거나 수령 상태를 버리지 않는다. 필요하면 기능 표시만 중지한 호환성 패치로 처리한다.

실행 경로: Unity `-runTests -testPlatform EditMode -testResults ... -logFile ...`; 동일 옵션 빌드는 `Hellscript.Editor.ProjectBuilder.BuildMac`/`BuildMacRelease`와 `-hellscriptBuildOutput`을 사용한다. native는 개발 전용 flag, 독립 `-hellscriptSavePath`와 증거 경로를 명시한다. 전체 로그/XML, 원본 측정, player와 소스 hash, GUID/meta 사후 검사와 파일별 복구 사본은 로컬 최종 보고의 경로에서 보존한다. 정상 런과 배포 서비스의 실제 저장에는 합성 데이터를 넣지 않는다.
