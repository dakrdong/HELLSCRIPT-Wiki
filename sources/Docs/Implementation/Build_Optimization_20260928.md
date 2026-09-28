# 빌드 리소스와 시작 메모리 최적화

검증일: 2026-09-28 · [English](Build_Optimization_20260928.en.md)

현재 게임 콘텐츠를 유지하면서 중복 이미지, 과도한 텍스처 크기, 출시 빌드의 검증 코드와 시작 시 불필요한 음원 로딩을 줄였다. 기준 소스는 `672e4a0ceb87359fb9489bf8a0c96ff27f4a23fd`이며, Unity 6000.6.0f1에서 같은 플랫폼·출시 옵션으로 전후 빌드를 만들었다. 이전의 [APK 최적화 기록](APK_Size_Optimization.md)은 당시 콘텐츠에 대한 별도 측정이다.

## 실제 결과

| 측정 대상 | 이전 | 이후 | 감소율 |
| --- | ---: | ---: | ---: |
| macOS arm64 출시 앱 | 801,359,644 B (764.2 MiB) | 232,688,023 B (221.9 MiB) | 71.0% |
| Android arm64 출시 APK | 176,155,058 B (168.0 MiB) | 138,223,218 B (131.8 MiB) | 21.5% |
| macOS 출시 Runtime DLL | 4,482,048 B | 2,831,872 B | 36.8% |
| 시작 화면 Unity 할당 메모리 | 145,622,355 B | 131,711,258 B | 9.6% |
| 시작 화면 효과음 로딩 | 454개 / 11,321,668 B | 0개 / 0 B | 필요할 때 로딩 |

앱 크기는 `.app` 내부 파일의 논리 크기를 합산했고 APK는 파일 전체 크기를 쟀다. 1 MiB는 1,048,576바이트다. 설치 후 디스크 점유량이나 ZIP 크기와는 다르다. 메모리는 Apple M3 Pro에서 같은 개발 빌드·956×440 시작 화면·3초 대기 조건으로 측정한 한 표본이며, 프로세스 전체 메모리나 모든 전투의 사용량을 뜻하지 않는다.

[빌드 크기와 주요 리소스 묶음](BuildOptimization20260928Evidence/build-sizes.json) · [시작 메모리](BuildOptimization20260928Evidence/startup-memory.json)

## 무엇을 줄였는가

- 보상 상자 PNG 148개를 픽셀이 같은 이미지끼리 묶어 85개로 줄였다. 중복 PNG와 메타 63쌍을 삭제했다. 보상 정의 148개, 보상량, 지급 조건, 저장 ID와 SVG 원본은 유지했다. 표시용 `icon`만 공유하며, 카탈로그 검증은 누락되거나 순환하는 이미지 참조를 거부한다. 삭제 대상의 직렬화된 GUID 참조는 없었다.
- `AndroidTextureBudget`을 `ResourceTextureBudget`으로 확장했다. Standalone에는 BC7 고품질 압축과 표시 크기에 맞는 상한을 적용했고, Android의 ASTC 예산도 NPC·추가 장비·물약·룬에 적용했다. 원본 PNG를 다시 압축하거나 덮어쓰지 않았다. 얇은 HUD 테두리·마스크는 기존 설정을 유지한다.
- 물약은 1254px 원본 좌표를 실제 임포트 크기에 맞춰 잘라 쓰도록 수정했다. 따라서 물약을 512px로 가져와도 병의 비율이나 잘리는 영역이 달라지지 않는다. 좌표로 사용하는 타이틀·캐릭터·룬 아틀라스는 원본 해상도를 유지한다.
- 월드 PNG 160개가 이전 메타 형식 때문에 Cubemap으로 들어오던 문제를 고쳤다. `WorldArtImporter`가 Texture2D를 명시하므로 실제 머티리얼이 알베도를 찾을 수 있다. 메시·피벗·읽기 권한과 원본 이미지는 유지했다.
- 시작 시 효과음 454개를 한꺼번에 읽던 코드를 없앴다. 음원 경로만 등록하고 실제 재생 요청이 음소거·절전·재생 제한 검사를 통과하면 필요한 클립을 읽어 캐시한다. 공유 효과음은 같은 원본을 재사용한다. 283개 큐와 454개 클립, 스트리밍 배경음 4개를 모두 유지했다.
- 런타임 검증용 코드 111개 파일은 `UNITY_EDITOR || DEVELOPMENT_BUILD`일 때만 컴파일한다. Android 출시 빌드의 프로젝트 초기화 진입점은 106개에서 실제 게임용 2개로 줄었다. 검증 도구를 소스에서 버리지 않아 개발 빌드에서는 계속 사용할 수 있다. 일부 작은 MonoScript 메타데이터가 빌드 보고서에 남는 것과 실행 코드의 포함 여부는 구분한다.
- macOS 출시 진입점 `ProjectBuilder.BuildMacRelease`를 추가했다. LZ4HC와 관리 코드 제거 수준 Low를 사용하며, 빌드 후 기존 설정으로 복원한다. 기존 `BuildMac`은 검증용 개발 빌드로 유지한다. 빌드마다 `.size.json`을 남겨 이후 용량 증가를 실제 산출물 기준으로 추적한다.

Resources의 문자열 로딩, 데이터 카탈로그와 공유 경로까지 확인했다. 효과음처럼 실제로 쓰는 리소스는 삭제하지 않았으며, 과거 최적화에서 이미 제거한 패키지·템플릿을 이번 삭제량에 다시 세지 않았다.

## 검증

- 관련 Unity Edit Mode 검사 **107/107 통과**, 실패·건너뜀 0개. 이미지 공유, 보상·저장 규칙, 음원 지연 로딩, 물약 경계, 월드 텍스처와 임포트 반복 안정성을 검사했다. [검사별 결과](BuildOptimization20260928Evidence/editmode.json)
- 네이티브 개발 빌드에서 **283개 큐 / 454개 클립**의 실제 Resources 로딩, 세 물약의 경계, 월드 Texture2D와 알베도 연결을 확인했다.
- 같은 독립 테스트 계정으로 전후 각각 약 20초 전투를 실행했다. 두 빌드 모두 **400틱**을 진행하고 검사에 통과했다. 첫 안내가 측정을 멈추지 않도록 두 복사본에 동일한 안내 완료 상태를 사용했다. 프레임 수를 FPS 개선 근거로 해석하지 않는다. [전투 기록](BuildOptimization20260928Evidence/combat.json)
- 출시 빌드에서 실제 클릭으로 게스트 로그인 → 전사 생성 → 전투 3킬 → 인벤토리 → 장비 상세 → 장착을 확인했다. 방어도가 68에서 92로 바뀌었고 저장 파일의 갑옷 장착 상태를 확인했다. 다른 실행 중인 게임과 구분하기 위해 검증 복사본의 앱 이름·번들 ID·서명만 바꿨으며 플레이어 Data 파일은 측정한 출시 앱과 해시가 같았다. [실행 근거](BuildOptimization20260928Evidence/native-release.json)
- GPU로 그린 이미지 13종의 전후 표본을 비교했다. 가장 큰 평균 차이는 창고 아이콘의 2.86/255였으며, 축소 전의 잔무늬가 부드러워지는 차이가 있다. 원본 PNG와 투명도는 유지했다. [이미지 수치](BuildOptimization20260928Evidence/image-comparison.json)
- 스킬 아트 111개와 세트 아트 129개 검사, 공통 UI 계약 검사와 Python 계약 테스트 9개를 통과했다. 스킬 아트의 기존 휴리스틱 경고는 별도로 남아 있으며 새 검사 오류는 없다.
- 운영 서버의 보상 카탈로그·기본 지급 규칙 검사를 함께 실행했다. 이미지 공유는 서버의 보상 ID와 지급 의미를 바꾸지 않는다.

| 전후 이미지 | 이전 | 이후 |
| --- | --- | --- |
| 창고 | ![이전 창고](BuildOptimization20260928Evidence/baseline-Art_GlobalHUD_menu-storage.png) | ![이후 창고](BuildOptimization20260928Evidence/optimized-Art_GlobalHUD_menu-storage.png) |
| 물약 | ![이전 물약](BuildOptimization20260928Evidence/baseline-Art_GlobalHUD_potion-hp.png) | ![이후 물약](BuildOptimization20260928Evidence/optimized-Art_GlobalHUD_potion-hp.png) |

[출시 빌드 전투](BuildOptimization20260928Evidence/release-ui.png) · [장비 상세](BuildOptimization20260928Evidence/release-detail.png) · [장착 후 인벤토리](BuildOptimization20260928Evidence/release-equipped.png)

## 검증 범위와 재실행

이 작업에서는 전체 회귀 검사를 다시 실행하지 않았다. 이전 APK 최적화 기록의 전체 검사 결과와 이번 107개 집중 검사는 별개다. 연결된 Unity MCP 인스턴스가 없어 설치된 Unity 배치 모드와 독립 macOS 플레이어로 검증했다. Android는 출시 APK 빌드 성공까지 확인했으며 실기기 설치·화질·프레임·배터리는 확인하지 않았다. macOS 결과를 모바일 성능으로 일반화하지 않는다.

```bash
Unity -batchmode -nographics -projectPath <checkout> -executeMethod Hellscript.Editor.ProjectBuilder.BuildMacRelease -hellscriptBuildOutput <output>/HELLSCRIPT.app -quit
Unity -batchmode -nographics -buildTarget Android -projectPath <checkout> -executeMethod Hellscript.Editor.ProjectBuilder.BuildAndroid -hellscriptBuildOutput <output>/HELLSCRIPT.apk -quit
python3 tools/compare_build_size.py <before>.size.json <after>.size.json
```

크기 비교 도구는 플랫폼과 빌드 옵션이 다르면 거부한다. 검증용 macOS 앱은 `BuildMac`, 개발 APK는 `BuildAndroid -hellscriptDevelopment`로 만든다. 원본과 비교 산출물을 함께 보관해야 파일 크기를 직접 비교할 수 있다. 계정·저장·로그와 전체 빌드 보고서는 로컬 `Artifacts/BuildOptimization/`에 두고, 공개 근거에는 개인 식별자가 없는 측정 결과와 화면만 담았다.
