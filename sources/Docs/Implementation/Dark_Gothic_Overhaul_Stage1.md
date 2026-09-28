# 다크 고딕 개편 1단계: 렌더링 기반·필드 6종·보스 5종

작성일: 2026-09-28 · [English](Dark_Gothic_Overhaul_Stage1.en.md)

어둡고 무게감 있는 액션 RPG 분위기로 게임 전체를 바꾸는 작업의 첫 병합이다. 이번 단계에는 월드 셰이더, 조명과 후처리, 절차적 3D 제작 도구와 모델 가져오기·리그 코드, 균열 필드 6종과 필드별 적 구성, 보스 5종의 2페이즈 패턴을 넣었다. 새로 만든 3D 모델은 아직 화면에 연결하지 않았다. 이펙트 라이브러리, 신규 일반 적 8종의 고유 공격, 신규 콘텐츠 효과음, 몬스터 등급(매직·엘리트·전설·고유) 이펙트도 다음 단계에서 이어 간다.

## 방향과 출처

목표는 디아블로 4가 주는 분위기다. 차가운 달빛과 따뜻한 불빛의 대비, 채도를 낮춘 흙·돌 색, 거칠고 낡은 재질, 무게감 있는 이펙트를 기준으로 삼았다. 분위기만 참고했을 뿐 **디아블로 4의 모델·텍스처·이펙트·이름·로고는 추출하거나 사용하지 않았다.** 셰이더와 후처리 설정은 이 저장소에서 직접 작성했고, 3D 자산은 저장소 안의 Blender 스크립트가 시드 고정 기하 연산과 Blender 기본 절차 노드로 만든다. 외부 메시·텍스처·스캔을 내려받지 않았으며 이미지·3D 생성 AI 모델을 쓰지 않았다. 출처 기록은 [자산 출처](Asset_Provenance.md)에 있다.

## 구성

| 분야 | 내용 |
| --- | --- |
| 월드 셰이더 | [`RiftTerrain.shader`](../../Assets/HELLSCRIPT/Resources/RiftTerrain.shader)를 월드 공용 조명 셰이더로 바꿨다. 노멀맵, 알베도 알파의 매끄러움, 주광 그림자, 추가 광원(모바일 Forward·PC Forward+), 테두리광, 피격 섬광, 사망 소멸, 넓은 범위의 색 변화를 지원한다. ShadowCaster·DepthOnly·DepthNormals 패스를 더했고 SRP 배처와 호환된다. 시야(안개) 규칙은 그대로다. 이펙트용 [`WorldFx.shader`](../../Assets/HELLSCRIPT/Resources/WorldFx.shader), 예고 장판용 [`WorldTelegraph.shader`](../../Assets/HELLSCRIPT/Resources/WorldTelegraph.shader)를 추가했다. 마을 페이드 셰이더는 텍스처를 유지하고, 마을 바닥은 추가 광원을 받는다. |
| 조명과 후처리 | [`FieldLook.cs`](../../Assets/HELLSCRIPT/Runtime/Presentation/FieldLook.cs)에 필드 6종·마을·훈련장의 조명 프리셋을 두었다. [`WorldLighting.cs`](../../Assets/HELLSCRIPT/Runtime/Presentation/WorldLighting.cs)는 3색 환경광, 안개, 달빛 방향광, 영웅을 따라다니는 따뜻한 광원, 화로 같은 발광체에 가까운 순서로 배정하는 실광원 풀(PC 8개, 모바일 3개), 은은한 깜박임, 카메라 흔들림을 담당한다. 흔들림은 설정 화면·즉시 표시·동작 줄이기 상태에서 0이다. 후처리는 PC·모바일 프로필(ACES 톤매핑, 블룸, 색 보정, 비네트, 분할 톤, 필름 그레인은 PC만)을 쓰고, PC는 SMAA, 모바일은 FXAA를 쓴다. 위험 표시의 빨강이 톤매핑 뒤에도 선명하도록 기존 발광 배율을 ×1.8에서 ×0.8로 낮췄다. |
| 균열 필드 6종 | [균열 필드 6종](../Design/HELLSCRIPT_Rift_Fields.md). 필드는 별도 난수 흐름으로 정해 기존 시드의 방 배치·지문·전투 난수를 바꾸지 않는다. 필드마다 조명 프리셋과 바닥 색조를 적용하고, 전투 머리글에 필드 이름을 보여 준다. |
| 일반 적 20종 | N13–N20의 이름·필드·역할·능력치를 넣었다. 고유 공격은 개발 중이며 지금은 N01의 근접 타격을 임시로 쓴다. 역할 판정은 `EnemyCombat.Role`로 바꿨고 N01–N12의 결과는 이전과 같다. |
| 보스 5종 | [보스 전투 상세](../Design/HELLSCRIPT_Boss_Combat_Detail.md). 보스마다 1페이즈 3종·2페이즈 3종의 패턴을 가지며, 2페이즈는 1.5초 포효로 시작한다. 새 패턴 24종과 포효를 추가했고, 예측 피해와 실제 피해가 모든 패턴에서 같다. 신규 보스 두 종은 31·41단계부터 나온다. 새 공격의 소리는 가장 가까운 기존 소리를 임시로 쓴다. |
| 절차적 3D 제작 도구 | [`hs3d.py`](../../tools/art3d/hs3d.py)는 기하 도형, 절차 재질, 공용 아틀라스 굽기(색·거칠기·노멀 2배 슈퍼샘플링, 빈틈 채움, AO 거리 0.3m), 게임 카메라 방향 미리보기, 바이트 단위로 같은 FBX 내보내기를 제공한다. [`generate_world_art.py`](../../tools/generate_world_art.py)는 제작법 모듈을 찾아 Blender 프로세스별로 빌드하고, 매니페스트·결정적 GUID 메타를 쓰며, `--check`로 파일·해시·예산·코드 참조를 검사한다. |
| 모델 가져오기와 리그 | [`WorldArtImporter.cs`](../../Assets/HELLSCRIPT/Editor/WorldArtImporter.cs)는 `Pivot_*` 뼈대가 있는 모델의 부품을 스킨 메시 하나로 합쳐 캐릭터 하나를 드로우콜 한 번으로 그리게 한다. 발광 부품은 정점 색 알파로 표시한다. [`WorldArt.cs`](../../Assets/HELLSCRIPT/Runtime/Presentation/WorldArt.cs)는 모델·텍스처·매니페스트를 읽어 재질을 만들고, 자산이 없으면 null을 돌려 기존 기본 도형 표현이 그대로 쓰이게 한다. [`ActorRig.cs`](../../Assets/HELLSCRIPT/Runtime/Presentation/ActorRig.cs)는 이족·사족·부유·덩어리·벌레·거미·뱀형 7가지 골격의 걷기·대기·예비 동작·공격·회복·돌진·피격·사망을 절차적으로 움직인다. 프레임마다 메모리를 할당하지 않는다. |
| 확인용 스모크 | [`RuntimeArtGallerySmoke.cs`](../../Assets/HELLSCRIPT/Runtime/Presentation/RuntimeArtGallerySmoke.cs)(`-hellscriptArtGallerySmoke`)는 필드 6종의 방, 일반 적 전체, 보스, 정예, 이펙트, 마을을 한 번에 찍는다. 다음 단계에서 모델과 이펙트를 연결할 때 같은 목록으로 비교한다. |

Android 텍스처 예산에 `World/` 경로 규칙을 추가했다. 보스·필드·이펙트는 최대 1024, 나머지는 512이며 ASTC 6×6을 쓴다.

## 게임에서 달라지는 점

- 균열과 마을에 후처리와 새 조명이 적용된다. 영웅 주변에 따뜻한 광원이 따라다니고, 화로·등불은 실광원을 받는다.
- 균열마다 필드가 정해지며, 필드 이름과 필드별 조명·안개·바닥 색이 달라진다. 습한 동굴과 황혼 초원은 1단계부터, 작열 사막과 눈 덮인 고원은 11단계부터 나온다.
- 보스는 HP 50% 이하에서 포효한 뒤 2페이즈 패턴을 쓴다. 31단계부터 사구의 폭군, 41단계부터 혹한의 여사제가 나온다.
- 영웅·적·보스·방은 아직 기존 기본 도형으로 그린다.

## 제작과 재현

```sh
python3 tools/generate_world_art.py --only Sample --out Builds/WorldArt   # 샘플 두 개를 Blender로 빌드(설치하지 않음)
python3 tools/generate_world_art.py --check                               # 설치된 World/ 자산 검사(Blender 불필요)
python3 tools/test_world_art.py                                           # 러너 단위 검사
```

Unity 검사와 스모크는 에디터가 잠근 원본 대신 복제 프로젝트에서 실행한다.

```sh
Unity -batchmode -nographics -projectPath <복제> -runTests -testPlatform EditMode -testResults <결과.xml>
Unity -batchmode -projectPath <복제> -executeMethod Hellscript.Editor.ProjectBuilder.BuildMac -hellscriptBuildOutput <빌드>/HELLSCRIPT.app -quit
HELLSCRIPT -hellscriptArtGallerySmoke -hellscriptSavePath <저장> -hellscriptScreenshots <그림> -screen-fullscreen 0 -screen-width 1600 -screen-height 900
HELLSCRIPT -hellscriptBossSmoke -hellscriptBossResume 4 -hellscriptSavePath <저장> -hellscriptScreenshots <그림> -screen-fullscreen 0 -screen-width 1600 -screen-height 900
```

## 검증

2026-09-28, 통합 브랜치 `c6856548` 기준으로 복제 프로젝트에서 확인했다.

- **macOS 개발 빌드**: `BuildMac` 성공, 오류 0개. [빌드 결과](DarkGothicStage1Evidence/build-result.txt)
- **튜토리얼 스모크**: 첫 실행과 `-hellscriptTutorialResume` 재실행이 모두 종료 코드 0으로 끝났고, 실제 보스 처치·마을 복귀·20가지 해상도·언어·글자 크기 배치를 확인했다. [결과](DarkGothicStage1Evidence/runtime-tutorial-smoke.txt), [실제 보스 처치](DarkGothicStage1Evidence/tutorial-real-boss-cleared.png)
- **아트 갤러리 스모크**: 종료 코드 0, 캡처 39장. [결과](DarkGothicStage1Evidence/runtime-art-gallery-smoke.txt)
  - 필드 6종의 방: [잊힌 묘지](DarkGothicStage1Evidence/gallery-01-room-field0.png), [무너진 성채](DarkGothicStage1Evidence/gallery-02-room-field1.png), [작열 사막](DarkGothicStage1Evidence/gallery-03-room-field2.png), [습한 동굴](DarkGothicStage1Evidence/gallery-04-room-field3.png), [황혼 초원](DarkGothicStage1Evidence/gallery-05-room-field4.png), [눈 덮인 고원](DarkGothicStage1Evidence/gallery-06-room-field5.png), [한 장 비교](DarkGothicStage1Evidence/fields-sheet.png)
  - 휴대폰 비율: [가로 956×440](DarkGothicStage1Evidence/gallery-07-room-field0-956x440.png), [세로 440×956](DarkGothicStage1Evidence/gallery-08-room-field1-440x956.png)
  - 일반 적 20종: [N01–N06](DarkGothicStage1Evidence/gallery-09-enemies-00-05.png), [N07–N12](DarkGothicStage1Evidence/gallery-10-enemies-06-11.png), [N13–N18](DarkGothicStage1Evidence/gallery-11-enemies-12-17.png), [N19–N20](DarkGothicStage1Evidence/gallery-12-enemies-18-19.png)
  - 보스 5종: [집행자](DarkGothicStage1Evidence/gallery-13-boss-0.png), [합창자](DarkGothicStage1Evidence/gallery-14-boss-1.png), [포식자](DarkGothicStage1Evidence/gallery-15-boss-2.png), [사구의 폭군](DarkGothicStage1Evidence/gallery-16-boss-3.png), [혹한의 여사제](DarkGothicStage1Evidence/gallery-17-boss-4.png), [정예](DarkGothicStage1Evidence/gallery-18-elites.png), [마을](DarkGothicStage1Evidence/gallery-38-town-portal.png)
- **보스 패턴 스모크**: 보스 5종의 패턴 30종과 포효 5회가 모두 예고한 뒤 준비 시간에 맞춰 발동했다(예고에서 첫 발동까지 차이가 반 스텝 이내). [결과](DarkGothicStage1Evidence/runtime-boss-kit-smoke.txt)
  - 캡처: [집행자 포효](DarkGothicStage1Evidence/bosskit-k1-0-Roar-warning.png), [처형 도약](DarkGothicStage1Evidence/bosskit-k1-4-ExecutionLeap-warning.png), [진혼 합창](DarkGothicStage1Evidence/bosskit-k2-4-RequiemChoir-warning.png), [탐식의 끌어당김 상태](DarkGothicStage1Evidence/bosskit-k3-6-DevouringPull-status.png), [유사 소용돌이](DarkGothicStage1Evidence/bosskit-k4-4-QuicksandMaelstrom-warning.png), [눈보라 장막](DarkGothicStage1Evidence/bosskit-k5-4-BlizzardVeil-warning.png), [파쇄 부채](DarkGothicStage1Evidence/bosskit-k5-6-ShatterFan-warning.png)
- **Edit Mode 전체 검사**: 4,295개 중 4,248개 통과, 47개 실패. 실패 47개는 모두 개편 전 기준선에 들어 있던 것이며 새 실패는 없다. [요약](DarkGothicStage1Evidence/editmode-full-summary.txt)
- **UI 계약·월드 아트·위키 검사**: `check_ui_contract.py`, `test_ui_contract.py`(9개), `test_world_art.py`(24개), `generate_world_art.py --check`, `wiki.py build`·`check`를 통과했다.
- **화면 확인**: 필드 6종 캡처를 한 장으로 모아 보았다. 필드마다 조명 색과 바닥 색조가 구분되고, 방 윤곽과 영웅 주변은 모두 읽힌다. 습한 동굴과 무너진 성채가 가장 어둡다. 황혼 초원은 아직 공용 석재 바닥에 색조만 입힌 상태라 풀밭보다는 갈색 포장길에 가깝게 보인다. 필드 전용 바닥 모델을 연결할 때 다시 본다.

## 확인하지 못한 것

- 모바일 실기기와 Android 빌드에서 보지 않았다. 모바일 셰이더는 Vulkan·GLES3·iOS용으로 오프라인 컴파일만 확인했고, 모바일 후처리 경로(저품질 블룸, FXAA)와 기기 성능은 측정하지 않았다.
- 화면 흔들림·섬광을 끄는 설정 화면은 아직 없다. 지금은 기기 값 `hellscript.reduce-motion`만 읽는다.
- 튜토리얼 스모크는 조명 작업 빌드에서 5번 중 2번, 조명과 무관한 단계(갑옷 지급 한 프레임 경합, 훈련 결과 대기)에서 실패한 적이 있다. 원인은 확인하지 못했다.
- 새 조명에서 기억된 지형(탐색했지만 지금 보이지 않는 곳)은 이전보다 어둡게 보인다. 필드 바닥 모델을 연결할 때 다시 맞춘다.
- 사람의 눈으로 한 품질 평가가 아니다. 작업자는 캡처와 측정값으로 확인했으며, 최종 판단은 사용자의 몫이다.

## 이전 작업과 다음 단계

기존 [숲속 정착지](Forest_Settlement.md)와 균열 시야·기억 지형 작업의 표현 규칙을 유지한 채 조명과 셰이더만 바꿨다. 다음 단계는 새 브랜치에서 진행한다.

- 작업 브랜치에서 만들고 있는 3D 모델을 화면에 연결한다. 영웅 3종과 무기, 일반 적 20종, 보스 5종, 필드 6종의 바닥·벽·장애물·장식, 상자·제단·관문 같은 공용 소품, 마을 건물·포털·NPC가 대상이다.
- 이펙트 라이브러리를 만든다. 스킬, 적·보스 공격, 예고 장판 채움, 피격·사망, 전리품 빛기둥, 필드 분위기, 몬스터 등급 이펙트가 포함된다.
- N13–N20의 고유 공격과 신규 콘텐츠 효과음을 넣는다.
