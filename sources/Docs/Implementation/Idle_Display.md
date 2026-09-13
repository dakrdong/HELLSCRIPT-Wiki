# 절전 방치와 잠깐 보기

작성일: 2026-09-13 · [English](Idle_Display.en.md)

상태: 데스크톱 구현·검증 단계가 완료됐다. 전체 검사 2,471개, 최종 앱의 실행·재시작, 화면 13장과 같은 최종 빌드의 성능 측정 5회를 확인했다. Android 실기기와 플랫폼별 절전 정책은 아래의 남은 범위로 관리한다.

## 화면과 사냥의 분리

[절전 방치 상세 기획](../Design/HELLSCRIPT_Idle_Mode_Detail.md)과 [화면 배치 기준](../Design/HELLSCRIPT_Screen_Layout_Detail.md)을 따라 일반 화면·절전 화면·잠깐 보기를 분리했다. 절전 버튼은 실제 균열 전투에서 제공한다. 훈련·일시정지·포탈 정리·편집·확인창에서는 새로 진입하지 않는다. 첫 안내에는 현재 반복 정책을 표시하며, 확인 여부는 계정 진행도와 분리된 기기 설정 파일에 저장한다. 짧은 가로 화면에서도 시작·취소 버튼을 하단에 유지한다.

절전 중에도 같은 `CombatSimulation`과 `RunState`로 실제 전투를 계산한다. 전투 카메라와 월드 표시를 끄고, 일반 HUD·미니맵 갱신과 일회성 타격 효과 생성을 건너뛴다. 검은 화면의 요약은 기본 1초마다 바뀌며, 중단 사유를 우선 표시한다. 표시 영역은 60초 간격으로 조금씩 이동한다. 새 균열의 전투 데이터는 즉시 만들되 화면 객체는 확인 시점까지 만들지 않는다.

잠깐 보기는 현재 완료 틱의 영웅·적·카메라 위치로 장면을 맞추고, 살아 있는 투사체·장판·상자 상태를 표시한다. 끝난 타격 효과는 다시 재생하지 않는다. 표시 목표는 30 FPS이며 마지막 입력 후 10초가 지나면 절전 화면으로 돌아간다. 계속 보기나 메뉴 진입은 자동 복귀를 취소하고 기존 프레임·수직 동기화·화면 출력 간격·화면 자동 꺼짐 설정을 복원한다. 기기 전체 밝기는 변경하지 않는다.

## 시간·반복·중단 처리

단조 증가 시계의 실제 경과를 1/20초 단위로 누적해 기존 0.05초 전투 틱에 전달한다. 한 프레임의 전투 시간을 0.25초로 잘라 버리던 경로를 제거했다. 화면 상태 변경은 남은 부분 시간을 지우지 않는다. 한 갱신에서 최대 20틱을 처리하며, 처리하지 못한 시간이 실제 0.5초를 넘는 상태가 3초 지속되면 마지막 처리 틱에서 보존하고 명시적인 재개를 요구한다.

자동 반복은 결과 화면의 표시 여부와 분리했다. 기존 결과 대기시간·종료 조건·가방 정리·보상 저장을 유지한다. 다음 판부터 중단은 현재 판을 즉시 포기하지 않으며, 판 종료와 정산 뒤 반복을 멈춘다. 저장 실패나 가방 부족으로 막혀도 절전 화면에서 중단 이유를 확인할 수 있다.

앱 중단은 전투를 저장하고 정지하며, 복귀 시 일반 화면에서 재개를 요구한다. 중단된 시간을 전투 틱으로 몰아서 계산하지 않는다. 로컬 개발 빌드의 미실행 보상은 기존 계정 단위 골드·일반 재료·12시간 상한을 유지한다. 지급과 기준 시각을 함께 저장하고 실패한 정산 구간을 고정한다. 다른 저장이나 장비 거래도 미완료 정산을 먼저 재시도한다. 절전·잠깐 보기 전환 자체는 미실행 보상을 만들지 않는다. 온라인 계정·서버 정산은 구현 범위에 포함되지 않는다.

일반 사용자는 계속 1배속만 사용한다. 1.5배속·2배속은 내부 계산 검증에서만 사용하며 기존 잠금 정책을 유지한다.

## 현재 검증 근거와 남은 범위

시간·표시 상태·미실행 보상 관련 집중 검사 30개가 통과했다. 세 직업·두 빌드·세 배속에서 각각 1,000회 화면 상태를 전환한 뒤 고정 틱 기준 실행과 전투 상태 전체를 비교했다. 영웅·적·행동 단계·투사체·난수·보상 상태가 같았다. 일시적인 지연, 지속적인 처리 지연, 일시정지와 앱 중단, 정산 실패·재시도·재실행 및 기기 설정 파일 보호도 검사했다.

Apple M3 Pro(Mac15,7), Unity 6000.6.0f1, Mono 개발 빌드, PC 품질, 1280×720에서 고정 시드 93171·30단계 마법사 연쇄 제어와 준비된 합법 장비를 사용했다. 3초 준비 후 20초를 측정했으며, 잠깐 보기는 자동 복귀 전인 8초를 측정했다. 아래의 출력 대상은 Unity의 출력 예약 프레임 수이며 실제 GPU 제출 횟수나 패널 전력이 아니다. 월드·효과·HUD 0회는 일반 표시 경로의 값이며 최소 요약 UI의 비용은 제외한다. Main Thread 값에는 프레임 제한 대기가 섞여 있고 Draw Calls/Batches는 일반 화면에서도 0이므로 CPU 작업 시간이나 GPU 절감률의 근거로 쓰지 않았다.

| 측정 | 실제 시간(초) | 전투 틱 | 월드 / 효과 / 일반 HUD 갱신 | 출력 대상 프레임 | 원본 |
|---|---:|---:|---|---:|---|
| 변경 전 일반 1 | 20.015 | 401 | 1193 / 174 / 1193 | — | [JSON](../../Artifacts/Validation/IdleMode/Normal1/profile.json) |
| 변경 전 일반 2 | 20.011 | 401 | 1193 / 174 / 1193 | — | [JSON](../../Artifacts/Validation/IdleMode/Normal2/profile.json) |
| 변경 전 일반 3 | 20.015 | 401 | 1195 / 174 / 1195 | — | [JSON](../../Artifacts/Validation/IdleMode/Normal3/profile.json) |
| 최종 일반 | 20.015 | 400 | 1188 / 174 / 1188 | 1188 | [JSON](../../Artifacts/Validation/IdleMode/AfterNormal1/profile.json) |
| 최종 절전 1 | 20.006 | 400 | 0 / 0 / 0 | 21 | [JSON](../../Artifacts/Validation/IdleMode/Dim2/profile.json) |
| 최종 절전 2 | 20.042 | 401 | 0 / 0 / 0 | 21 | [JSON](../../Artifacts/Validation/IdleMode/Dim3/profile.json) |
| 최종 절전 3 | 20.011 | 401 | 0 / 0 / 0 | 21 | [JSON](../../Artifacts/Validation/IdleMode/Dim4/profile.json) |
| 최종 잠깐 보기 | 8.026 | 161 | 240 / 75 / 240 | 240 | [JSON](../../Artifacts/Validation/IdleMode/Peek1/profile.json) |

최종 일반·절전·잠깐 보기는 모두 Build10을 사용한다. 변경 전 시계와 최종 시계의 경계 처리 차이를 감추지 않고 원본을 보존했으며, 표시 작업의 직접 비교는 같은 최종 소스를 기준으로 한다.

최종 실행 검사는 실제 UI 이벤트로 첫 안내·10초 자동 복귀·계속 보기·메뉴 진입·다음 판 중단·저장 실패·앱 복귀·재실행을 확인했다. 반복 경계를 검사할 때는 실제 포기 경로로 종료 결과를 만들었고, 앱 중단 콜백과 저장 오류를 의도적으로 주입했다. 자연 승리나 물리적 터치·OS 잠금 검사로 해석하지 않는다. 재실행 시에는 마을의 기존 균열 이어하기 조작으로 보존한 상태를 재개한다.

전체 검사 소스와 최종 앱의 차이는 재실행 검사 도구 한 파일뿐이다. 기존 이어하기 버튼을 누른 뒤 다시 일시정지를 요구하던 잘못된 검사 조건을 고쳤고, 최종 실제 앱에서 통과했다. 게임·UI·시계·번역·Edit Mode 검사 소스는 전체 검사와 동일하다.

이 결과는 휴대폰 배터리 절감률이나 완성된 모바일 절전 기능을 뜻하지 않는다. Android의 기기별 전력·발열·장시간 동작, 실제 터치·잠금·최소화 이벤트, 앱 창 밝기, 저전력·열 상태에 따른 중단, 실제 오디오·진동 정책은 후속 검증·구현 범위다. 화면 복귀 지연의 기기별 p95 목표도 아직 입증하지 않았다.

구현 근거: [전투 시계](../../Assets/HELLSCRIPT/Runtime/Core/ForegroundCombatClock.cs), [표시 상태](../../Assets/HELLSCRIPT/Runtime/Presentation/GameController.Idle.cs), [절전 UI](../../Assets/HELLSCRIPT/Runtime/Presentation/GameUI.Idle.cs), [월드 복원](../../Assets/HELLSCRIPT/Runtime/Presentation/WorldView.Idle.cs), [미실행 정산](../../Assets/HELLSCRIPT/Runtime/Core/GameStore.cs).

## 검증 파일과 화면

[Validation summary](../../Artifacts/Validation/IdleMode/validation-summary.json) · [Full Edit Mode XML](../../Artifacts/Validation/idle-display-editmode.xml) · [Source hashes](../../Artifacts/Validation/IdleMode/validated-source.json) · [Harness diff](../../Artifacts/Validation/IdleMode/Full2-to-Build10.diff) · [Profile summary](../../Artifacts/Validation/IdleMode/profile-summary.json) · [Visual review](../../Artifacts/Validation/IdleMode/visual-review.json)

| Native8 | Capture |
|---|---|
| initial-01-battle-entry-ko | [PNG](../../Artifacts/Validation/IdleMode/Native8/initial-01-battle-entry-ko.png) |
| initial-02-introduction-ko | [PNG](../../Artifacts/Validation/IdleMode/Native8/initial-02-introduction-ko.png) |
| initial-03-introduction-en-140 | [PNG](../../Artifacts/Validation/IdleMode/Native8/initial-03-introduction-en-140.png) |
| initial-04-dimmed-en-140 | [PNG](../../Artifacts/Validation/IdleMode/Native8/initial-04-dimmed-en-140.png) |
| initial-05-peek-en-140 | [PNG](../../Artifacts/Validation/IdleMode/Native8/initial-05-peek-en-140.png) |
| initial-06-automatic-dim-en | [PNG](../../Artifacts/Validation/IdleMode/Native8/initial-06-automatic-dim-en.png) |
| initial-07-next-rift-deferred-en | [PNG](../../Artifacts/Validation/IdleMode/Native8/initial-07-next-rift-deferred-en.png) |
| initial-08-repeat-stopped-en | [PNG](../../Artifacts/Validation/IdleMode/Native8/initial-08-repeat-stopped-en.png) |
| initial-09-save-blocked-en | [PNG](../../Artifacts/Validation/IdleMode/Native8/initial-09-save-blocked-en.png) |
| initial-10-foreground-resume-required-en | [PNG](../../Artifacts/Validation/IdleMode/Native8/initial-10-foreground-resume-required-en.png) |
| restart-01-resume-choice-town-en | [PNG](../../Artifacts/Validation/IdleMode/Native8/restart-01-resume-choice-town-en.png) |
| restart-02-suspended-rift-restored-en | [PNG](../../Artifacts/Validation/IdleMode/Native8/restart-02-suspended-rift-restored-en.png) |
| restart-03-remembered-idle-en | [PNG](../../Artifacts/Validation/IdleMode/Native8/restart-03-remembered-idle-en.png) |

| Profile | Capture |
|---|---|
| 최종 일반 | [PNG](../../Artifacts/Validation/IdleMode/AfterNormal1/profile.png) |
| 최종 절전 1 | [PNG](../../Artifacts/Validation/IdleMode/Dim2/profile.png) |
| 최종 절전 2 | [PNG](../../Artifacts/Validation/IdleMode/Dim3/profile.png) |
| 최종 절전 3 | [PNG](../../Artifacts/Validation/IdleMode/Dim4/profile.png) |
| 최종 잠깐 보기 | [PNG](../../Artifacts/Validation/IdleMode/Peek1/profile.png) |
