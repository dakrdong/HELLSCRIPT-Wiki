# CPU 실측과 보수적 최적화

검증일: 2026-10-02 · [English](CPU_Optimization_20261002.en.md)

최신 기준 `94e11019`에서 메뉴 뒤의 월드 렌더링과 마을 UI의 중복 갱신을 줄였다. 원본 checkout과 실행 중인 사용자 Unity는 보존하고, 단일 격리 checkout의 로컬 브랜치 `codex/cpu-ponytail-lite`에서 구현·검증했다. 개발 계측은 `3fc1033a`, 게임 코드 변경은 `91aceb5f`이다. 원격 push, main 병합, 공개 배포는 이번 요청에 따라 하지 않았다.

## 웹에서 먼저 확인한 원인

게임은 이미 `Application.targetFrameRate=60`과 백그라운드 실행을 설정한다. 그러나 Chrome의 실제 개발 플레이어는 같은 60/VSync0 설정에서도 약 60~120FPS로 실행됐다. 타이틀의 변경 전 세 표본은 102.2~120.2FPS, 변경 후는 약 60FPS였다. 이때 Renderer CPU 사용률 33.21→16.36%를 로직 최적화로 인정할 수 없다.

설치된 Unity 6000.6.0f1의 생성 JS와 엔진 상태를 확인했다. 한 진단 창 안에서 `screenAnimationFrameRate`가 60→120, Emscripten `timingMode=1`의 `timingValue`가 1→2로 바뀌었으며 실제 평균은 73.35FPS였다. 엔진의 rAF 주사율 추정과 프레임 건너뛰기 비율이 측정 중 변한 것이다. 설정값만 같게 두고 짧게 워밍업하면 웹의 실제 프레임 수를 통제하지 못한다. OS 표시 설정이나 사용자의 브라우저를 바꾸지 않았다. [진단 근거](CPUOptimization20261002Evidence/web-pacing-diagnosis.json)

최종 동일 조건 비교는 상태마다 전후 3회, **총 24개 표본**을 완료했다. 양쪽 모두 Chrome headless에서 실제 Apple M3 Pro ANGLE Metal GPU, GPU compositing/WebGL 켜짐, 소프트웨어 렌더러 금지를 확인했다. 가상 화면은 800×600, 실제 viewport/canvas 출력은 1280×720/DPR1이다. 선택한 독립 페이지는 BEGIN/END 모두 visible/focused였고 중간 노출·포커스 변화가 없었다. 엔진 timer mode0/16.6667ms도 유지됐다. 실제 FPS 범위는 아래에 공개하며, 이 개발 백엔드 비교를 실사용 foreground 출시 브라우저 성능 보장으로 확대하지 않는다.

| 상태 | 실제 FPS 범위 전→후 | Renderer ms/프레임 | Renderer CPU % | GPU 프로세스 CPU % |
| --- | --- | ---: | ---: | ---: |
| 메뉴 | 59.3~59.6 → 58.6~59.7 | 3.062 → 2.425 | 18.24 → 14.22 | 10.66 → 8.32 |
| 마을 idle | 59.3~59.5 → 58.0~59.1 | 4.194 → 3.950 | 24.85 → 22.91 | 13.99 → 13.30 |
| 인벤토리 | 58.7~59.3 → 58.9~59.2 | 3.874 → 3.823 | 22.94 → 22.51 | 12.91 → 12.93 |
| 전투 | 57.5~58.1 → 57.7~58.1 | 6.707 → 6.642 | 38.92 → 38.56 | 13.94 → 13.84 |

| 상태 | GC KB/프레임 | GC/20초 | 평균 간격 ms | p95 간격 ms |
| --- | ---: | ---: | ---: | ---: |
| 메뉴 | 11.4 → 11.5 | 7 → 7 | 16.84 → 17.05 | 18.00 → 18.00 |
| 마을 idle | 41.7 → 38.4 | 21 → 20 | 16.85 → 16.97 | 18.00 → 19.00 |
| 인벤토리 | 36.2 → 36.6 | 12 → 12 | 16.89 → 16.98 | 18.00 → 18.00 |
| 전투 | 122.8 → 122.6 | 32 → 31 | 17.23 → 17.22 | 21.00 → 21.00 |

| 상태 | 관리 힙 MiB | Unity 할당 MiB | Renderer RSS 범위 MiB | JS 힙 MiB |
| --- | ---: | ---: | --- | ---: |
| 메뉴 | 10.5 → 10.5 | 64.7 → 64.5 | 1282.3~1301.1 → 942.0~1299.2 | 8.8 → 8.5 |
| 마을 idle | 12.5 → 13.2 | 89.6 → 89.9 | 469.2~1464.4 → 1057.9~1451.5 | 9.2 → 9.6 |
| 인벤토리 | 17.6 → 17.6 | 91.0 → 91.4 | 1441.9~1469.9 → 1441.6~1458.3 | 8.7 → 9.1 |
| 전투 | 29.2 → 29.3 | 88.8 → 89.1 | 1516.4~1604.5 → 1016.8~1591.9 | 9.2 → 9.2 |

메뉴 Renderer CPU/프레임은 **20.8% 감소**했고 반복 범위가 3.047~3.110→2.365~2.499ms로 겹치지 않았다. 마을 CPU는 3.878~4.220→3.769~4.344ms로 겹치고 세 쌍 중 두 쌍이 증가했으므로 낮아진 중앙값을 CPU 개선으로 인정하지 않는다. 인벤토리·전투도 범위가 겹친다. 마을 할당은 이 환경에서 약 7.9% 감소했다. 메모리·프레임시간 개선은 인정하지 않는다. 120FPS→60FPS 정책 자체로 얻는 사용률 감소는 이 로직 결과에 포함하지 않는다.

[통제된 변경 전](CPUOptimization20261002Evidence/web-controlled-before-detailed.json) · [변경 후](CPUOptimization20261002Evidence/web-controlled-after-detailed.json) · [비교](CPUOptimization20261002Evidence/web-controlled-comparison.json) · [산출물·HTML 해시](CPUOptimization20261002Evidence/controlled-build-binaries.json)

기존 기준 산출물이 22:14(+0900)에 바뀌어 HTML 계측 인자·JS 함수가 사라지고 WASM/data 해시가 달라진 것을 확인했다. 근거 없이 특정 작업자를 원인으로 지목하지 않는다. 네이티브와 웹 이후 해시는 유지됐다. 정확한 3fc1033a 개발 기준을 새 전용 경로에 재빌드하고, 이후 산출물은 COW 복사로 고정했다. 최종 24회 뒤 HTML/loader/framework/data/WASM 해시가 모두 일치했다. 무보고 실패 시도는 CPU 근거에서 제외하고 이전 유효 표본과 분리했다. 최종 소스는 이미 전체 검사를 마친 91aceb5f와 동일하게 복원했다. 양쪽 로컬 페이지에 Analytics 코드·비콘은 없었다.

표시 조건을 맞춘 추가 비교는 생성된 개발 산출물의 측정용 JS에서만 timer mode 0 / 16.6667ms를 유지한다. 게임의 배포 코드에는 이 강제 스케줄링을 넣지 않았다. 따라서 이 비교에서 인정하는 것은 같은 페이스의 로직 비용 차이이며, 프레임 상한 정책으로 얻는 절감은 별도다. 페이지 표시·포커스 변화, 실제 FPS, 주사율 추정, 스케줄러 상태, 화면·viewport·DPR을 함께 기록한다.

## 변경 내용

- 타이틀·캐릭터 선택은 화면을 덮는 기존 UI 뒤의 월드 카메라를 기존 `World.SuspendPresentation()`으로 쉬게 한다. UI 애니메이션은 유지한다. 마을·던전 구축 시 기존 `ResumeCamera()`를 호출해 복원한다.
- 마을의 `RefreshPlaza()`를 Controller Update와 UI Update에서 중복 실행하던 것을 UI LateUpdate 한 번으로 모았다. 이동 후 상태를 같은 프레임에 반영하며 버튼을 누른 직후의 기존 갱신은 유지한다.
- 상호작용·구매·판매 버튼을 매 갱신마다 false→true로 토글하던 코드를 최종 상태를 한 번씩 설정하도록 정리했다. 포탈 우선순위, NPC 대화와 상점 행동은 유지한다.

전투 틱, 밸런스, 저장·보상 검증, 해상도, 품질과 게임의 FPS 정책은 변경하지 않았다. 공통 UI 소유자는 TitleAtmosphere/TitleScreen/CharacterSelection과 GameUI.Plaza이며 새 표시 체계를 만들지 않았다.

## 네이티브 개발 플레이어 결과

각 상태 3회, 20초 표본의 중앙값이다. CPU100%는 한 코어를 완전히 쓰는 값이다. GC KB는 1000바이트 단위다.

| 상태 | CPU ms/프레임 | CPU % | GC KB/프레임 | GC/20초 |
| --- | ---: | ---: | ---: | ---: |
| 메뉴 | 3.881 → 3.012 | 23.04 → 17.83 | 12.1 → 13.8 | 8 → 8 |
| 마을 idle | 5.330 → 5.288 | 31.62 → 31.50 | 61.8 → 55.4 | 53 → 47 |
| 인벤토리 | 5.194 → 5.180 | 30.79 → 30.86 | 48.1 → 48.5 | 29 → 30 |
| 전투 | 8.245 → 8.135 | 48.46 → 47.61 | 186.0 → 190.8 | 76 → 75 |

메뉴 CPU/프레임은 **22.4% 감소**했다. 세 표본 범위는 3.662~3.976→2.946~3.177ms로 겹치지 않았다. 메뉴 삼각형 카운터는 약 31,496→953으로 감소했고 아래 타이틀 화면은 유지됐다. 마을 할당은 **10.4% 감소**했지만 CPU 차이는 측정 노이즈 안이므로 CPU 개선으로 인정하지 않는다. 인벤토리·전투도 CPU 개선을 인정하지 않는다. 메뉴와 전투의 할당 중앙값은 오히려 증가했다.

| 상태 | 실제 FPS | 평균 간격 ms | p95 간격 ms |
| --- | ---: | ---: | ---: |
| 메뉴 | 59.89 → 59.83 | 16.69 → 16.71 | 16.93 → 16.91 |
| 마을 idle | 59.52 → 59.52 | 16.80 → 16.80 | 17.76 → 17.82 |
| 인벤토리 | 59.62 → 59.47 | 16.77 → 16.81 | 17.46 → 17.52 |
| 전투 | 58.65 → 58.66 | 17.04 → 17.04 | 21.59 → 21.61 |

프레임 간격은 실제 연속 프레임 timestamp의 차이이다. p95와 메모리에서 일관된 개선은 확인하지 못했다.

| 상태 | 관리 힙 MiB | Unity 할당 MiB | 플레이어 RSS MiB |
| --- | ---: | ---: | ---: |
| 메뉴 | 14.7 → 14.6 | 126.5 → 126.0 | 467.4 → 466.8 |
| 마을 idle | 15.5 → 15.5 | 153.5 → 153.6 | 534.7 → 536.5 |
| 인벤토리 | 19.6 → 19.5 | 155.1 → 155.4 | 550.2 → 549.4 |
| 전투 | 30.8 → 31.2 | 154.2 → 154.4 | 616.0 → 620.0 |

[변경 전 원본 수치](CPUOptimization20261002Evidence/native-before-detailed.json) · [변경 후](CPUOptimization20261002Evidence/native-after-detailed.json) · [비교](CPUOptimization20261002Evidence/native-comparison.json)

| 타이틀 | 변경 전 | 변경 후 |
| --- | --- | --- |
| 1280×720 | ![타이틀 전](CPUOptimization20261002Evidence/menu-before.png) | ![타이틀 후](CPUOptimization20261002Evidence/menu-after.png) |

## 남은 비용

웹 전투 baseline의 프레임당 marker 평균 중앙값은 BehaviourUpdate 1.293ms, World.Present 0.565ms, Combat.Tick 0.461ms, UGUI.Rendering.UpdateBatches 0.574ms였다. 구간은 중첩되므로 더해서 전체 CPU로 보지 않는다. WaitForTargetFPS 3.794ms와 Main Thread wall time은 실제 CPU 작업량과 구분한다. 기존 마을은 Mobile 품질에서도 약 339,423개의 삼각형이 처리된다. 타이틀처럼 화면 뒤의 월드를 쉬게 할 수 있는 경우와 실제 마을·전투의 렌더링 비용은 다르다.

갱신을 LateUpdate로 옮겨 마을의 BehaviourUpdate와 UI.Update marker 포함 범위가 달라졌다. 이 marker 전후 감소만으로 CPU 개선을 판정하지 않고 전체 Renderer/플레이어 CPU를 사용한다.

실제 플레이어 CPU와 원본 Editor CPU도 분리했다. 사용자 Editor PID55085는 측정 내내 남겨 두었고 표본별 CPU 범위를 JSON에 기록했다. 네이티브 baseline에서 약 140~189%, 이후 약 116~185%였다. Editor 수치를 게임 최적화 증거로 사용하지 않는다. 웹의 별도 GPU·브라우저 프로세스 CPU는 Renderer에 합치지 않는다.

## 방법과 Ponytail

macOS 26.6.2, Mac15,7 / Apple M3 Pro, 12코어, 36GiB RAM, Unity 6000.6.0f1. macOS 개발 플레이어는 Mono/PC 품질, WebGL 개발 플레이어는 IL2CPP/Mobile 품질이다. 서로 다른 플랫폼끼리 CPU를 직접 비교하지 않는다. Chrome 154.0.8037.93, 실제 Metal GPU, canvas 1280×720/DPR1. 기본 표본은 3초 워밍업 뒤 20초, 상태마다 3회이다.

독립 Mage Lv30 세이브, rare +5 장비 8개, 고정 UTC1790899200, 장비 시드9132026, 전투 시드93171, 30단계, autoRepeat/edict 꺼짐을 사용했다. fixture SHA256는 `81dd0abc25c1d46416523dae026233e3beba1aedd2e977d95dc46dc5983dfad5`, workload SHA256는 `5381209ced8858655d03efff3a77444db0d3fed9097947d8ec258ede007239f5`, 전투 map fingerprint는 `cb352198`이다. 진입과 운영 설정 갱신은 기존 경로로 처리한다. 계측은 개발 빌드에서 명시적 플래그와 빈 독립 세이브 경로가 있을 때만 켜진다.

네이티브 CPU는 실제 앱 `Process.TotalProcessorTime`의 창 내 차이이다. 웹은 BEGIN/END `console.timeStamp` 사이 CDP `Performance.ProcessTime`을 사용하고 해당 Renderer의 `SystemInfo.getProcessInfo` CPU 차이와 25ms 이내로 일치하는지 검사한다. GC/frame은 `GC Allocated In Frame`, 횟수는 `GC.CollectionCount(0)`이다. 관리 힙, Unity allocated, 프로세스 RSS와 JS heap을 분리한다. 웹의 Draw Calls/Batches가 0이거나 GC.Collect marker가 0인 것은 실제 부하가 없다는 증거로 사용하지 않는다.

설치된 [Ponytail 원본](https://github.com/DietrichGebert/ponytail)의 **4.10.0 SKILL.md**를 lite 지침으로 수동 적용했다. 정확한 SHA256는 `1316a2f3f95741d2300b116fe0c2d81ce4a9568656ed0a62643f54aaf09957f2`이다. 요구사항과 안전 검사를 보존하고 기존 함수 재사용·반복 제거·작은 변경을 검토했다. 별도 캐시 계층이나 UI 재작성보다 기존 카메라 제어와 갱신 함수를 재사용하는 쪽을 선택했다. 설치 스크립트·Node hooks·전역 지침은 실행·변경하지 않았고 공식 CLI audit 통과로 표현하지 않는다. 이 검토는 실측·회귀 검사를 대체하지 않는다.

## 검증과 한계

마지막 게임 코드 변경 뒤 전체 EditMode 검사를 완료했다. baseline과 이후 모두 **5,041개 / 4,913 통과 / 128 실패 / 건너뜀 0**이며 실패 이름은 동일했다. **새 실패 0, 기존 실패 해결 0**이다. 실패를 숨기거나 검사를 약화시키지 않았다. 공통 UI 계약 검사와 Python 계약 검사 11개도 통과했다. [전체 전후 비교](CPUOptimization20261002Evidence/editmode-comparison.json) · [기존 실패 상세](CPUOptimization20261002Evidence/editmode-baseline-failures.json) · [빌드 완료 근거](CPUOptimization20261002Evidence/build-outcomes.json)

네이티브·WebGL 전후 개발 빌드가 각각 성공했다. 실제 네이티브 절전 실행·독립 재시작, 마을 포탈 실행·독립 재시작 4개가 통과했다. 포탈 검증은 KO/EN, 440×956/956×440/PC 16:9·16:10·21:9의 실제 raycast 입력, 저장 실패와 재시도, 같은 run/time/HP/resource/position/RNG/cooldown/enemies 복원, 신규 진입 비용 없음, 원래 속도와 재개 후 실제 시뮬레이션까지 검사했다. 절전 검증은 카메라·오디오·프레임 정책 복원과 장비·언어·저장 보존을 확인했다. 기본 글자 크기만 사용했다.

일반 스모크의 `Town build button missing`, 캐릭터 스모크의 `characters-back` 44px 미달, NPC 스모크의 `Wrong service turk-garbi page=plaza`는 변경 전에서도 같은 오류로 재현됐다. 새 카메라 휴식 검사와 첫 NPC 버튼 활성 안정성 검사는 기존 실패 지점 전에 통과했으며 전체 스모크를 통과했다고 보고하지 않는다. 포탈 runner의 누락된 evidence 플래그·checkpoint 복사는 실제 설정을 고친 뒤 재실행했고 초기 실패도 보존했다. [실행 결과 구분](CPUOptimization20261002Evidence/runtime-comparison.json) · [포탈](CPUOptimization20261002Evidence/town-portal-runtime.txt) · [포탈 재시작](CPUOptimization20261002Evidence/town-portal-reload.txt) · [절전](CPUOptimization20261002Evidence/idle-runtime.txt)

이전 30935f37/출석 팝업이 덮인 계측, 8191바이트로 잘린 Unity JSON, 종료한 페이지 재사용 실패, 전체 검사와 겹친 스케줄러 진단은 성능 비교에서 제외했다. 완전 JSON은 개발 플레이어 virtual FS의 기존 보고서를 동일한 측정용 JS로 회수했다. 연결된 물리 모바일 장치가 없으므로 모바일 CPU·배터리는 확인하지 못했다. 출시 빌드와 Safari/Firefox도 이번 CPU 비교 범위에 없다. UnityCLI 시작 대기는 동일 설치 Editor의 직접 배치 실행으로 진행했으며, Bee 빌드 오류와 키체인 승인 사이의 인과관계는 확정하지 않았다. [조건·제외 목록](CPUOptimization20261002Evidence/method.json)

## 재실행

baseline 소스는 계측을 포함한 `3fc1033a`, 이후 소스는 `91aceb5f`이다. 두 개발 빌드에 같은 계측을 사용하고 CPU 측정 중 별도 빌드·검사를 실행하지 않는다. `Unity`는 설치된 6000.6.0f1 Editor 바이너리를 뜻한다.

```bash
Unity -batchmode -nographics -quit -projectPath "$CHECKOUT" -buildTarget StandaloneOSX -executeMethod Hellscript.Editor.ProjectBuilder.BuildMac -hellscriptBuildOutput "$OUTPUT/HELLSCRIPT.app" -logFile "$OUTPUT/build.log"
python3 tools/profile_native_cpu.py "$OUTPUT/HELLSCRIPT.app/Contents/MacOS/HELLSCRIPT" "$CAPTURES" --editor-pid "$USER_EDITOR_PID"
Unity -batchmode -nographics -quit -projectPath "$CHECKOUT" -buildTarget WebGL -executeMethod Hellscript.Editor.WebPlayerBuild.BuildGitHubPages -hellscriptDevelopment -hellscriptBuildOutput "$WEB_OUTPUT" -logFile "$WEB_OUTPUT/build.log"
python3 tools/prepare_web_cpu_profile.py "$WEB_BEFORE" before
python3 tools/prepare_web_cpu_profile.py "$WEB_AFTER" after
PROFILE_HEADLESS=1 PROFILE_FIXED_FPS=60 PROFILE_EDITOR_PID="$USER_EDITOR_PID" python3 tools/profile_web_cpu_batch.py "$WEB_BEFORE" before "$WEB_CAPTURES" --after-build "$WEB_AFTER"
python3 tools/summarize_cpu_profiles.py "$WEB_CAPTURES/before" "$RESULTS/before.json"
python3 tools/summarize_cpu_profiles.py "$WEB_CAPTURES/after" "$RESULTS/after.json"
```

측정 JS는 새로운 개발 산출물에 한 번만 적용하며 배포용 출력을 수정하지 않는다. Chromium은 표본마다 독립 프로필로 실행하고 runner가 만든 자식만 종료한다. 반복 2는 전후 순서를 뒤집으며 병렬 Unity·브라우저를 실행하지 않는다. 전체 원본 JSON·XML·로그·표본 이미지는 작업 디렉터리의 `evidence/`에 보존했고, 저장소에는 위 링크의 간결한 근거만 포함한다.
