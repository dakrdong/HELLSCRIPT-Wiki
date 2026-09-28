# APK 용량 최적화

2026-09-28 후속: 현재 Standalone·Android 예산과 보상 이미지 통합·검증 코드 제외 결과는 [빌드 최적화 기록](Build_Optimization_20260928.md)을 따른다. 아래 수치와 남은 선택지는 2026-09-27 당시 기록이다.

갱신일: 2026-09-27

2026-09-26에 만든 Android 개발 APK가 2.1GB(2,096,429,914바이트)였다. 게임에 들어간 사운드·모델은 거의 없지만, 이미지 설정과 쓰지 않는 자료 때문에 용량이 커졌다. 게임 동작과 화면은 그대로 두고 이 원인을 정리했다.

## 결과

같은 커밋(`a1bc4825`)을 기준으로 비교했다. 크기는 APK 파일 전체 바이트이며 1MB는 1,000,000바이트로 계산했다. 근거는 [APK 크기표](APKSizeOptimizationEvidence/apk-sizes.json)에 남겼다.

| 빌드 | 이전 | 이후 |
| --- | --- | --- |
| 개발 빌드 (`Development`, LZ4) | 2,097,620,190 B (2.1GB) | 88,376,002 B (88MB) |
| 배포 빌드 (Release, LZ4HC) | — | 60,072,342 B (60MB) |

효과음·운영 보상·첫 플레이 개선을 함께 넣은 2026-09-27 통합 배포 APK는 70,585,750바이트(70.6MB)다. 위 표는 최적화 전후의 동일 범위 비교로 보존하며, 추가 콘텐츠가 포함된 [통합 빌드 근거](MainIntegration20260927Evidence/android.json)와 구분한다.

| APK 안의 항목 (압축 후) | 이전 개발 | 이후 개발 | 이후 배포 |
| --- | --- | --- | --- |
| 에셋 묶음 `data.unity3d` | 2,028.7MB | 34.3MB | 31.9MB |
| `libil2cpp.so` | 34.0MB | 20.9MB | 11.2MB |
| `libunity.so` | 21.4MB | 21.0MB | 9.7MB |
| `global-metadata.dat` | 8.2MB | 6.8MB | 3.0MB |
| `classes.dex` | 3.0MB | 3.0MB | 2.4MB |

## 원인

- Unity 빌드 보고서에서 텍스처가 6.0GB였다. `Resources` 폴더의 이미지는 코드가 불러오지 않아도 모두 빌드에 들어간다.
- 위상 문양 123장(`ClassAspectIcons`)은 어떤 코드도 불러오지 않았다. 도구가 만든 최소 메타가 옛 형식이라 한 장이 약 32MB로 가져와져 3.9GB를 차지했다.
- 전설·스킬 아이콘 252장(141+111)은 1254px 원본이 압축 없이 최대 2048px로, 세트 아이콘 129장은 1024px 원본이 압축 없이 1024px로 들어갔다. 이 폴더에는 임포트 설정을 정하는 코드가 없었고, 아트 도구가 만든 메타가 `압축 없음`이었다. 보석·보상 상자·출석·대장간·HUD 폴더는 전용 임포터가 `압축 없음`을 강제했다. 문서에는 "초기 검수용으로 압축을 끄고 나중에 기기에서 비교한다"고 적혀 있었다.
- `com.unity.ai.inference`(Sentis)는 게임이 쓰지 않는데 직접 의존성이었다. 패키지의 `Resources` 셰이더 28.5MB가 빌드에 들어갔고, 이 패키지가 끌어온 App UI는 플레이어 시작 시 초기화 코드를 실행했다.
- 개발 빌드에서는 `com.unity.pipeline`이 link.xml로 95개 어셈블리를 통째로 보존한다. 그래서 개발 APK의 코드가 배포 빌드보다 크다. 이 도구는 에디터 연결 복구에 쓰므로 패키지는 유지하고, 배포 빌드 경로를 새로 두었다.

## 변경 내용

| 항목 | 변경 | 게임 영향 |
| --- | --- | --- |
| 위상 문양 | `Resources/Art/ClassAspectIcons` 사본 123장과 메타 삭제. 원본은 `Docs/Art/ClassAspectIcons/native/`에 바이트까지 같게 남음. `prepare_assets.py`와 [manifest](../Art/ClassAspectIcons/manifest.json)도 `native/`를 가리킴 | 없음(불러오는 코드 없음) |
| Android 텍스처 예산 | `Assets/HELLSCRIPT/Editor/AndroidTextureBudget.cs`를 추가. 폴더별로 Android에만 최대 크기와 ASTC 압축을 지정한다. 기본(Default) 설정은 각 기능 임포터가 계속 관리하므로 Standalone 대상의 에디터·EditMode 검사·macOS 스모크는 이전과 같은 텍스처를 본다. 에디터를 Android 대상으로 바꾸면 예산이 적용된 이미지가 보인다 | Android 화면 이미지가 표시 크기에 맞게 줄어듦 |
| 사냥 칙령 스킨 | 실행 중에 불러오지 않는 스킨 아틀라스·조작 부품·`resource-manifest.json`을 `Assets/HELLSCRIPT/Art/HuntEdict/`로 이동(GUID 유지). 게임이 쓰는 문양은 `Resources`에 남김 | 없음 |
| 룬 문양 아틀라스 | `RuneV13ArtImporter`가 NPOT 확대를 끔. 1536×2016을 2048×2048로 늘이던 것을 원본 크기로 둔다. 좌표가 정규화 UV라 표시 위치는 같다 | 없음 |
| 패키지 | `com.unity.ai.inference`(App UI 포함), `com.unity.timeline`, `com.unity.ai.navigation`, `com.unity.collab-proxy`와 참조가 없는 엔진 모듈 12개(cloth, vehicles, wind, umbra, vectorgraphics, tetgen, timelinefoundation, adaptiveperformance, terrainphysics, unitywebrequesttexture/assetbundle/www) 제거 | 없음 |
| 유지한 모듈 | ParticleSystem(MCP 도구), Video·Terrain·오디오 요청(AI Assistant), Director·Accessibility(URP 에디터), XR·Analytics(테스트 프레임워크 등) | — |
| 템플릿 잔여물 | `Assets/Scenes/SampleScene.unity`, `Assets/TutorialInfo`, `Assets/Readme.asset` 삭제. 빌드 장면 목록과 App UI 설정 참조 정리 | 없음 |
| Android 플레이어 설정 | 관리 코드 제거 수준 Low, IL2CPP 코드 생성 "Faster (smaller) builds" | 제네릭 코드가 약간 느려질 수 있음(기기 미측정) |
| 빌드 진입점 | `ProjectBuilder.BuildAndroid` 추가. 기본은 배포 빌드(LZ4HC), `-hellscriptDevelopment`를 주면 스모크용 개발 빌드(LZ4) | — |

폴더별 Android 예산은 다음과 같다. 크기는 화면 배율 150%에서 가장 크게 보이는 경우를 덮도록 정했다. 전설 아이콘만 예외로, 도박 공개 장면(569px)에서는 512px 텍스처를 약간 확대한다.

| 폴더 | Android 최대 크기 | 형식 | 가장 큰 표시 크기 |
| --- | --- | --- | --- |
| 전설·스킬·세트 아이콘 | 512 | ASTC 6×6 | 569px(전설 도박 공개), 384px, 396px |
| 보석·물약(`Jeweler`) | 256 | ASTC 6×6 | 185px |
| 출석 보상 | 256 | ASTC 4×4 | 224px |
| 보상 상자 | 256 | ASTC 4×4 | 242px |
| 대장간 | 512(원본 유지) | ASTC 4×4 | 114px |
| HUD 물약·심연 주화 | 원본 유지 | ASTC 6×6 | `PotionArt`가 1254px 좌표로 잘라 쓰고 스모크가 주화 폭을 확인하므로 줄이지 않음 |
| HUD 초상·메뉴 | 512 | ASTC 6×6 | 301px |

HUD의 테두리·마스크·상태 아이콘 같은 작은 UI 부품은 9분할 테두리와 마스크 품질을 위해 압축하지 않았다. 용량 영향은 작다.

## 검증

- **Android 빌드**: 배포·개발 APK 모두 오류 없이 빌드됐다. 컴파일 오류는 없었다.
- **EditMode**: 변경 전후 모두 3,983개 중 47개가 실패했고 실패 목록이 같다. 47개는 모두 변경 전부터 실패하던 검사다([EditMode 비교](APKSizeOptimizationEvidence/editmode-comparison.json)).
- **Android 이미지 비교**: Android로 가져온 텍스처를 게임처럼 표시 크기로 그려 변경 전후를 비교했다. Apple M3 Pro GPU가 ASTC를 직접 읽으므로 실제 압축 결과를 그렸다. 평균 밝기 차이는 모든 폴더에서 0.5/255 미만이다. 줄이지 않고 압축만 한 이미지는 대부분 PSNR 38~56dB로 차이를 알아보기 어렵다. 예외로 HUD 메뉴 아이콘 5장(512px 유지, 264px로 표시)은 평균 35.7dB, 최저 26.7dB이다. 크기를 줄인 아이콘은 PSNR 26~33dB이다. 이전에는 1254px 원본을 밉맵 없이 그려 계단 현상이 있었고, 줄인 이미지는 그 잡음이 사라져 조금 부드럽다. 전설 아이콘이 569px로 커지는 도박 공개 장면에서는 512px 원본을 약간 확대한다([수치](APKSizeOptimizationEvidence/texture-comparison.json)).
  - [전설 아이콘 전후](APKSizeOptimizationEvidence/legendary-before-after.png)
  - [전사 스킬 아이콘 전후](APKSizeOptimizationEvidence/skill-warrior-before-after.png)
  - [출석 보상 전후](APKSizeOptimizationEvidence/attendance-before-after.png)
  - [2배 확대 비교](APKSizeOptimizationEvidence/zoom-2x-before-after.png)
  - [룬 문양 아틀라스 전후(UV 기준)](APKSizeOptimizationEvidence/glyph-atlas-before-after.png)
- **아트 검증 도구**: `ClassSkillIcons`와 `ClassSetIcons`의 `validate_assets.py`가 새 Android 블록을 제외하고 검사하도록 고쳤고 둘 다 통과한다.
- **macOS 런타임 스모크**: 변경 전(`a1bc4825`)과 변경 후 macOS 개발 빌드에서 같은 스모크 102개를 돌렸다. 두 빌드 모두 6개 통과, 94개 실패, 2개 건너뜀이며 스모크마다 결과와 실패 사유가 같다. 통과한 것은 캐릭터 선택·클래스 스킬·장비 아트·프리셋·타이틀·튜토리얼이다. 94개 실패는 변경 전부터 있던 것으로, 튜토리얼 선행·운영 설정 대기·콘텐츠 개방 조건이 추가된 뒤 스모크 준비 코드가 갱신되지 않은 탓이다. Google 로그인은 실제 계정 로그인이 필요해, 사냥 칙령은 스킬 트리 스모크와 같은 검사라 건너뛰었다([스모크 비교](APKSizeOptimizationEvidence/smoke-comparison.json)).

## 확인하지 못한 것

- APK를 Android 실기기나 에뮬레이터에 설치하거나 실행하지 않았다. 연결된 기기가 없었다. 화면 품질은 macOS GPU에서 그린 비교로만 확인했다.
- IL2CPP "Faster (smaller) builds"의 실행 속도 영향은 측정하지 않았다.
- 배포 빌드는 `Debug.isDebugBuild`로 막힌 스모크·QA 세션·개발자 도구가 꺼진다. 스모크와 QA에는 개발 빌드를 쓴다.

## 남은 선택지

- 전설 아이콘을 1024로 올리면 도박 공개 장면도 원본 이상으로 보인다. APK가 약 50MB 늘어난다.
- 보상 상자 148장 중 76장은 바이트가 같은 13묶음이다(중복 63장). 한 장으로 합치려면 `icon==id` 규칙·생성기·서버 스키마를 함께 바꿔야 하고, 압축 후 절감은 약 4MB라 이번에는 두었다.
- R8 코드 축소, Unity 시작 로고 끄기, 스모크 파일을 배포 빌드에서 제외하기는 각각 수 MB 이하이며 기기 확인이 필요해 이번에는 하지 않았다.
- macOS 개발 빌드는 기본 설정을 그대로 두어 2.4GB이다. 필요하면 같은 예산을 Standalone에도 적용할 수 있다.
- `applicationIdentifier`가 아직 템플릿 값(`com.UnityTechnologies.com.unity.template.urpblank`)이다. 스토어 출시 전에 바꿔야 하지만, 바꾸면 기존 설치본의 로컬 저장 위치가 달라지므로 이번 작업에서는 건드리지 않았다.

## 다시 빌드하는 방법

Unity 에디터가 프로젝트를 열고 있으면 APFS 복제본에서 빌드한다. 배치 모드가 바꾸는 `ProjectSettings.asset`·`UnityConnectSettings.asset`·URP 설정은 커밋하지 않는다.

```bash
Unity -batchmode -nographics -buildTarget Android -projectPath <복제본> -executeMethod Hellscript.Editor.ProjectBuilder.BuildAndroid -hellscriptBuildOutput Builds/Android/HELLSCRIPT.apk -quit
```

스모크용 개발 APK는 같은 명령에 `-hellscriptDevelopment`를 붙인다.
