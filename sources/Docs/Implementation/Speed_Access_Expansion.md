# 배속 접근 제한 개발 기록

작성일: 2026-09-09 · 현재 제공 단계의 잠금 구현

## 적용 범위

[배속 잠금·구독·시간 사용권 정책](../Design/HELLSCRIPT_Speed_Access_Detail.md)의 현재 제공 정책을 구현한다. 현재 일반 균열·소유 캐릭터 훈련·A/B 비교는 1배속을 사용한다. 1.5배속·2배속에는 잠금 표시를 붙이며, 누르면 ‘현재는 사용할 수 없는 배속입니다’를 표시한다. 구매·구독·쿠폰 사용 화면으로 연결하지 않는다.

향후 월 구독 권한, 이벤트 쿠폰, 시간 충전·차감·만료, 서버의 중복 사용 방지는 이번 구현에 포함하지 않는다. 별도 절전 방치 화면도 아직 구현되지 않았다. 해당 화면을 연결할 때 같은 실행 배속 정책을 사용해야 한다.

## 구현 경계

`CombatSpeedAccess`가 현재 선택 가능 여부와 적용 배속을 관리한다. `GameController.EffectiveSpeed`를 전투 틱 누적, 전투 효과의 표시 시간, HUD와 전투 상태 안내에서 함께 사용한다. `AccountSave.speed`를 직접 고배속 값으로 바꾸더라도 실제 전투 진행은 1배속이다. 일반 화면에 개발용 해금 버튼이나 실행 인자를 추가하지 않았다.

`GameStore`는 기존 저장과 백업을 읽을 때 배속 값을 1로 정리하며, 쓰기 직전에도 1을 저장한다. 필드가 없거나 고배속이 저장되어 있어도 전투 시드·위치·HP·쿨타임·정지·획득물은 배속 변경 때문에 초기화하지 않는다. 계정 저장 형식과 버전을 추가하지 않았다.

저장 불러오기와 사용자의 **이어하기** 명령은 구분한다. 불러온 체크포인트의 정지 상태는 보존하고, 이어하기를 선택하면 기존 정책대로 실제 전투를 재개한다. 배속 잠금 때문에 이 흐름을 바꾸지 않았다.

잠긴 배속 입력은 안내만 표시하며 전투 로그·계정·저장 파일을 변경하지 않는다. 이미 1배속에서 같은 버튼을 눌러도 불필요한 저장과 배속 변경 로그를 만들지 않는다. 작은 화면에서는 숫자와 ‘잠김’을 두 줄로 표시하고 전투 상태 화면에서 전체 정책을 읽을 수 있다.

시뮬레이션의 고정 틱 계산은 그대로 유지한다. 세 배속의 동등성은 기존 내부 훈련 비교 검사로 계속 검증한다. 과거 네이티브 검사 코드에서 `SetSpeed(1.5f/2f)`를 호출하더라도 이제 사용자 정책에 따라 거절되므로, 이를 고배속 실행 증거로 해석하지 않는다. 향후 최대 부하 검사는 일반 사용자에게 노출되지 않는 별도 내부 검증 경로가 필요하다.

## 검증

전체 Edit Mode 검사 **990개**가 통과했다. 이 중 새 배속 정책 검사는 16개이며, 기존 내부 훈련 비교의 세 배속 계산 검사도 통과했다. 먼저 실행한 배속·배치·훈련 비교 대상 검사 96개 역시 통과했다. [전체 검사 결과](../../Artifacts/Validation/speed-access-editmode.xml)와 [대상 검사 결과](../../Artifacts/Validation/speed-access-focused.xml)를 보존했다.

최신 실행 파일은 [macOS 화면 대응 개발 빌드](../../Builds/macOS-ScreenLayout/HELLSCRIPT.app)다. 빌드 오류는 0건이며, 기존 실행 파일과 실제 사용자 저장을 교체하지 않고 별도 시험 저장 폴더를 사용했다.

| 검사 | 확인한 결과 |
|---|---|
| 기존 저장·백업 | 1.5배속·2배속 값과 배속 필드가 없는 저장을 1배속으로 읽었다. 실제 균열 체크포인트와 영웅·재화를 보존하고, 쓰기에서도 1배속을 저장했다. |
| 잠금 입력 | 잠긴 버튼과 직접 전달한 고배속·잘못된 값에 잠금 안내를 표시했다. 전투·계정 JSON과 저장 파일이 바뀌지 않았다. 이미 선택한 1배속 버튼도 불필요하게 기록하지 않았다. |
| 실제 전투 속도 | 소유 캐릭터 훈련, A/B 비교의 A 실행, 자동 생성 균열에서 저장값을 2로 바꿔도 약 1.2초의 실제 진행에 약 1.2전투초가 진행됐다. 고정 틱과 프레임 경계를 포함해 검사했다. |
| 독립 프로세스 재시작 | 종료한 계정의 배속만 2로 바꾼 별도 시험 파일을 다시 읽었다. 마지막 접속 시각을 제외한 전체 계정·체크포인트가 예상값과 같고 배속은 1이었다. 이어하기 직전에 균열 ID·시간·HP·난수를 대조하고 실제 재개도 1배속인지 확인했다. |
| 잠금 표시와 창 크기 | 배속 잠금이 적용된 코드로 전투 16개 크기·보스 5개 크기를 재검사했다. 잠금 문구, 전체 한국어 텍스트 높이, 버튼 접근, 전투 영역과 HUD의 비중첩, 미관찰 정보 가림, 동일 훈련 120틱 비교를 통과했다. |
| 설정 회귀 | 설정·안내의 실제 macOS 창 11개 크기와 별도 프로세스 재시작을 통과했다. 미저장 초안·입력 문자열·정지 사유·반복 대기시간·확인창·기기 방향 설정을 보존했다. |

[배속 실행 기록](../../Artifacts/Validation/SpeedAccessNativeEvidence/runtime-speed-access-smoke.txt), [배속 재시작 기록](../../Artifacts/Validation/SpeedAccessNativeEvidence/runtime-speed-access-resume.txt), [실제 경과 시간](../../Artifacts/Validation/SpeedAccessNativeEvidence/speed-checks.txt), [잠금 적용 후 전투 배치 기록](../../Artifacts/Validation/ScreenLayoutLockedEvidence/runtime-battle-layout-smoke.txt), [설정 실행 기록](../../Artifacts/Validation/ScreenLayoutSettingsEvidence/runtime-screen-settings-smoke.txt), [설정 재시작 기록](../../Artifacts/Validation/ScreenLayoutSettingsEvidence/runtime-screen-settings-resume.txt)을 보존했다.

[넓은 전투 화면](../../Artifacts/Validation/ScreenLayoutLockedEvidence/02-battle-1280x720.png), [작은 세로 전투 화면](../../Artifacts/Validation/ScreenLayoutLockedEvidence/07-battle-360x640.png), [전투 상태와 배속 안내](../../Artifacts/Validation/SpeedAccessNativeEvidence/03-speed-policy-overview.png), [재시작 후 균열](../../Artifacts/Validation/SpeedAccessNativeEvidence/04-resumed-rift-lock.png)을 확인할 수 있다.

첫 재시작 검사는 저장 불러오기와 명시적 이어하기를 모두 정지 상태로 가정해 중단됐다. 기존 이어하기 명령은 재개하는 동작임을 확인하고 검사만 수정했다. 게임의 재개 정책은 변경하지 않았으며, 수정한 검사로 재시작을 통과했다. 첫 실패 로그도 보존했다.

전체 990개 검사 이후 변경한 C# 파일은 선택 실행되는 네이티브 검사 코드 `RuntimeSpeedAccessSmoke.cs`뿐이다. 마지막 빌드에 해당 검사를 포함하고 재시작을 통과했다. 소스 143개, 기존 보호 자산 98개의 해시와 문서 상태를 [최종 소스 기록](../../Artifacts/Validation/screen-layout-final-source-manifest.json) 및 [검증 요약](../../Artifacts/Validation/screen-layout-validation-summary.json)에 기록한다. 기존 씬·프리팹·패키지·프로젝트 설정 98개는 전투 배치 작업 시작 기준과 같다. 새 C# 메타 파일의 누락과 중복 GUID는 없다.

작업 도중 외부에서 수정된 설계 문서 6개와 새 배속·절전 기획 2개는 보존했다. 배속 정책을 읽은 시점 이후 설계 문서는 이 작업으로 변경하지 않았다.

## 남은 범위

장비·사냥 칙령·결과·훈련 비교 등 나머지 관리 화면의 전체 재배치, 별도 절전 방치 모드, 모바일 실기기의 방향 전환·터치·한글 조합 입력·키보드·노치·앱 복귀와 장시간 성능 검증이 남아 있다. 이번 네이티브 검사는 UI 콜백과 창 크기 API를 사용한 macOS 실행이며, 물리 입력이나 모바일 완료 증거가 아니다. 향후 구독·쿠폰·시간 잔액은 서버와 BM 상세 설계 단계에서 구현한다.
