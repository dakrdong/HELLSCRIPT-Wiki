# 몬스터 도감 최종 통합 인계 — 소스 동결

2026-10-14. 사용자 요청에 따라 화면별 Unity/빌드 계획을 종료하고 전체 화면을 합친 단일 통합 검증으로 넘긴다. **이후 소스 수정은 통합 담당의 구체적인 결함 요청이 있을 때만 재개한다.** 이 인계 과정에서 생성·컴파일·테스트를 반복하지 않았다. 소스·브랜치·원화를 삭제하지 않는다.

## 고정 입력과 소유권

- 담당 화면/스레드: 몬스터 도감 / `01a13915-e84f-7660-8044-31fef29120f8`.
- 단일 통합 소유자: `01a13790-39bc-7062-a516-b83a354000a8`.
- 통합 작업공간: `/Users/t8g-2410-pn-005/Documents/Codex/2026-10-14/task-6/skill-skin`, 담당자가 관리하는 별도 통합 브랜치와 그 자리의 기존 Library 재사용.
- 소스 작업공간: `/Users/t8g-2410-pn-005/Documents/Codex/2026-10-14/task-9/monster-codex-skin`.
- 보존 브랜치: `codex/casual-monster-codex-skin`.
- 최종 실행 소스 커밋: `d11008811c4a20778dd1324f1861b75d4aaff487`. 인계 시작 시 미커밋 변경 0. 이후 커밋은 이 인계와 상태 문서만 포함한다.
- 반영된 공통 커밋: `0ccba80eef0da4703cb7ffcd1fb8e22848002b3d`(cdb9c140, Noto 보강6cdf2df6, 장비 공급f25e67aa 포함).
- 소유 파일: `MonsterCodexWindow.cs`, `.Detail.cs`, `.Skin.cs`/`.meta`, `RuntimeMonsterCodexSmoke.cs`, `MonsterCodexWindowTests.cs` 및 도감 전용 문서. 공통 파일은 소유자의 커밋 그대로 수입했다.

## 통합 순서

공통0ccba80e를 가진 통합 브랜치에서 도감 전용 커밋만 순서대로 인수할 수 있다. 파일에 이미 같은 변경이 있는지는 통합 담당이 확인한다.

1. 기존 전체 UV/aspect-fit, 준비된 초상 fallback, 배지 위치 보정이 없을 때만 `27d41bc1aec3b8202685b1deac9e644b476cdfd7`을 먼저 적용한다. 이미 원본의 도감 원화 소스 보정을 인수했다면 중복 적용하지 않는다.
2. `330d30ffc12dec8cda632ea31e87a4892aec4a4c` — 도감 스킨, 검사 소스, 보고서.
3. `549939eea089d1b532c68d55a3319162b2d7ff14` — 전용 버튼 어댑터 제거, 공통 shell/chrome 연결.
4. `d11008811c4a20778dd1324f1861b75d4aaff487` — 공통 TextColor 연결과 후속 상태 문서.

공통 병합 이력1dbf3458/4a3c414a는 위 cherry-pick 방식에서 다시 적용할 필요가 없다. task-9 clone의 커밋은 원본 repo에 아직 없을 수 있으므로 소스 경로에서 로컬 fetch로 가져온다. 원격 push나 main 병합은 수행하지 않았다. 출석 파일럿6378320b는 인수 목록이 아니다.

## 원화와 데이터 의존성

- [고정 ID/PNG/meta SHA256 목록](CasualMonsterCodexEvidence20261014/approved-portrait-map.json): 일반29·보스6, 총35 PNG, 63,338,362bytes. `Assets/HELLSCRIPT/Resources/Art/TrainingGround/Portraits/{ID}.png` 및 각 `.meta`.
- 현재 도감 브랜치의 registry는35종이지만 Git 초상 tree는기존33종이며 N23/B06 PNG가 없다. 따라서 sparse 파일을 펼치는 것만으로는 원화 의존성이 충족되지 않는다.
- 정확한 자산 판은 **57bba034dad1659e8a36dc9645b1291d2554e497의 전체 tree**다. 이 커밋은 merge이며 첫 부모 대비 변경은 보스6장 중심이다. 이를 단순 cherry-pick하여35종이 모두 들어온다고 판단하지 않는다. 통합 tree에35종이 없으면 전체 tree에서 위 매핑의35 PNG/meta만 인수하여 SHA를 확인한다. 기존33 GUID를 유지하고 N23/B06를 빠뜨리지 않는다.
- `pixel-review-accepted`와 `productionApproved:false`를 구분한다. 모델 또는 출시 승인을 추정하지 않는다. 도감/훈련장 공용 경로이며 새 복제본·새 원화·전역 재화 치환은 없다.
- 장비 B01과 몬스터 B01은 별도 ID 도메인이다. 도감에 CasualEquipmentArt.Provider를 연결하지 않는다.
- 공통 Noto400/700/900/Jua와 locale/monster registry, 보스 스킬 아이콘, GlobalHUD 초상 placeholder/도감 메뉴 아이콘은 통합 프로젝트의 기존 Resources를 사용한다. 이 sparse 소스 작업공간에는 이미지/폰트를 복제하지 않았다.

## 남은 API 연결과 시안 차이

도감의 필요한 연결은 완료됐다: `ContentWindowView.Open(..., skin: UiSkin.Casual)`, shared Casual/CasualClose chrome, `LabelFont`, `TextColor(Color)`. 장비·차트·전투/칙령 API는 도감 경로에서 사용하지 않는다. 모델 용량 오류로 미완료된 연결은 없다.

시안12개 DOM 확인, 개별 확대 확인5개, 유효 캡처6상태. 실제 게임35종과 시안25종 fixture가 다르다. 상세 헤더/방어 판·초상 배치 및 상세명15 vs 시안22, 소제목10 또는12 vs16, 기술명12 vs13 차이를 유지했다. 기존 배치·글자 크기를 보존하라는 공통 규칙을 따랐고 검사 통과를 위한 축소는 없다. 둥근 카드와 shared Item chamfer 초점선, 초상 모서리, 게이지 한계, KO/EN 줄바꿈은 실제 렌더 수용 대기다. [전체 보고서](Casual_Monster_Codex_Skin_20261014.md).

## 통합 검증 입력

아래는 통합 담당이 합산할 필터이며 task-9 개별 실행 계획이 아니다.

- `Hellscript.Tests.MonsterCodexWindowTests` — 21 사례(6해상도×KO/EN, 상태/폰트/입력 보존·원화/배지·locale 포함).
- `Hellscript.Tests.MonsterCodexTests`.
- `Hellscript.Tests.MonsterCodexPreviewTests`.
- `Hellscript.Tests.StoreViewBindingTests.CommitDuringPressWaitsForReleaseWithoutCancellingTheClick`.
- `Hellscript.Tests.StoreViewBindingTests.MeaningfulRefreshRestoresTheSameNamedFocusAndModality`.
- `Hellscript.Tests.StoreViewBindingTests.RepeatingButtonConfigurationDoesNotRestartAnExistingTransition`.

스모크 진입점은 `Hellscript.RuntimeMonsterCodexSmoke.Boot`의 `-hellscriptMonsterCodexSmoke`. UNITY_EDITOR 또는 DEVELOPMENT_BUILD에 컴파일되며 실제 Boot는 Debug.isDebugBuild가 필요하다. 필수 인자는 `-hellscriptOfflineQa -hellscriptServerBase invalid -hellscriptSavePath <통합 담당의 폐기용 경로> -hellscriptScreenshots <통합 출력 경로>`다. 의도한 실행은 폐기용 세이브만 사용하며 오디오는 mute/pause된다. **현재 검사는 인자 토큰의 존재만 확인하므로 실행자가 저장·출력 인자 바로 뒤에 유효한 새 절대경로 값이 있는지 반드시 검증해야 한다.** 저장 경로 값이 없으면 GameController가 사용자 기본 저장 경로를 사용할 수 있다. 이 인계에서 실행 코드는 변경하지 않았다.

이 기존 스모크는 자체 계정 fixture/해상도 전환과 `Application.Quit(0/1)`을 소유한다. **다른 화면 smoke 플래그와 한 프로세스에서 동시에 켜면 안 된다.** 통합 담당이 한 통합 Development Player/통합 검증 배치에서 순차 모드로 실행하거나 별도 통합 runner로 조율한다. 이 담당이 새 빌드나 Unity를 실행하지 않는다. 기존 과거 스킬 앱은 새 통합 소스의 검증 증거가 아니다.

검증 행렬:440×956,956×440,1280×720,1280×800,1680×720,2868×1320 × KO/EN. prologue/dock, 발견/미발견 목록, 일반·정예·보스·월드 모델 대기 초상, tier drag/±, pause/back, 실제 처치/snapshot/복귀/디스크 재로드 계약. 정상 출력은73 PNG 예상과 `runtime.txt`, 로그 마커 `HELLSCRIPT_MONSTER_CODEX_SMOKE_OK`, 종료0이다. 실패는 예외/종료1 및 조건부 blocked 캡처다. 전체 watchdog은 없으므로 통합 실행자가 시간·로그 진척을 감시하며, 종료 코드만으로 성공 판정하지 말고 runtime.txt·성공 마커·캡처73장도 함께 확인한다. 안전 영역은 UiSafeArea/macOS Screen.safeArea 가정이며 실제 iOS/터치 실기기 인증이 아니다.

## 재실행하지 않은 기존 근거와 남은 작업

공통 UI 소유권/refresh16 binding·193 source 검사 통과. Roslyn 런타임 오류0/경고91(도감 파일0), 테스트 소스 오류0/경고0. 공통 폰트 source/license 검사 및4개 Git blob SHA 일치. source hash 근거는 task-9 `evidence/static-compile/common-followup-receipt.json`; 폰트는 `evidence/common-followup-fonts.json`. Unity/EditMode 실제 실행0, native after 캡처0. 과거 성공이나 정적 컴파일을 실제 화면 검증으로 세지 않는다.

시안 Library 시트: `libfile_38a63ef1ddfc819184f30d8517c23195`(시안 근거이며 게임 after 아님). 통합 담당에게 남은 것은 통합 tree/원화 확인, 하나의 통합 검사 배치·native 검증 및 after 증거 저장이다. task-9의 개별 Unity/빌드 대기는 종료됐으며 단일 통합 담당에게 이전됐다. 거절/불명확한 실행 결과 없음.
