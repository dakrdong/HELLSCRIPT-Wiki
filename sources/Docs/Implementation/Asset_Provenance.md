# HELLSCRIPT 임시 리소스 출처

작성일: 2026-09-08

## 내장 이미지 생성

- 2026-10-01 사냥 칙령의 두루마리·붓 편집 그림은 내장 `image_gen`의 투명 PNG 생성으로 제작했다. 원본 RGBA와 알파를 변경 없이 보존했으며 코드로 그린 이전 그림을 교체했다. 호출에 모델명이 없어 `candidate_model_unknown`으로 기록한다. [실제 게임 연결과 검증](Hunt_Edict_Polished_Actions.md), [정확한 생성 문구·해시](../../Assets/HELLSCRIPT/Art/HuntEdict/action-icon-provenance.json).

- `Assets/HELLSCRIPT/Resources/Art/Sanctuary.png`: 성소·결과 화면 배경, 1024×1536.
- `Assets/HELLSCRIPT/Resources/Art/SkillAtlas.png`: 18개 액티브, 3개 직업, 3개 장비 표시용 아틀라스, 1536×1024. Unity의 UV 영역으로 사용하며 원본 PNG를 다시 편집하지 않았다.
- 생성 경로: Codex 내장 `image_gen`, 직접 API·CLI 미사용.
- 모델 확인: 도구가 `image_url`, `output_hint`만 반환해 모델명을 검증하지 못했다. `gpt-image-2` 또는 ‘2.0으로 생성했다’고 단정하지 않는다. 두 이미지는 개발용 임시 자산이며 출시 아트 승인과 모델 출처 검증을 마친 자산이 아니다.
- 생성 원본은 Codex의 `generated_images`에 보존하고 프로젝트 안에 복사했다. 파일 이름에 외부 게임의 상표나 원작 아트 식별자를 사용하지 않는다.

## 자체 제작

게임 UI·버튼·게이지·표식·공격 궤적은 Unity uGUI와 LineRenderer로 제작했다. 균열 방·성벽·기둥·화로는 기본 메시를 조합한다. 영웅과 적은 골격 없는 단순 몸체를 같은 루트 아래에 구성하고 이동·방향·흔들림으로 표현한다. 완성 모델·스켈레탈 애니메이션으로 교체할 수 있도록 전투 상태와 분리했다.

한국어 글꼴은 실행 기기의 운영체제 글꼴을 사용한다. 운영체제 글꼴 파일을 프로젝트에 복사하거나 재배포하지 않았다. Android 배포용 글꼴과 표시 검증은 별도 완료 조건이다.

## 생성 프롬프트

정확한 생성 프롬프트는 같은 폴더의 `Image_Prompts.md`에 기록한다.

## 후속 장비 아이콘

- `Assets/HELLSCRIPT/Resources/Art/EquipmentAtlas.png`: B01–B24, 1536×1024, 6열×4행의 RGBA 아이콘이며 네이티브 알파를 유지한다. 장비 목록과 상세 화면에 연결했다.
- 내장 이미지 생성 도구의 원본 PNG를 그대로 복사했다. 완성된 출시 자산으로 분류하지 않는다.
- 반환 PNG의 C2PA 생성 메타데이터에는 `softwareAgent.name = gpt-image`, `softwareAgent.version = 2.0`이 기록되어 있다. 이 확인은 해당 파일에만 적용하며, 위 두 이미지의 출처 확인 상태를 소급 변경하지 않는다.
- 프롬프트·경로·검토 내용: [장비 아이콘 생성 기록](Equipment_Atlas_Prompt.md).

## 후속 균열 바닥

- `Assets/HELLSCRIPT/Resources/Art/RiftStone.png`: 1254×1254 RGB 석재 바닥 원본이다. 내장 이미지 생성 도구로 만들고 Unity 재질에 적용했다.
- 이 PNG에서도 `softwareAgent`의 `gpt-image` / `2.0` 표기를 확인했다. 직접 API나 CLI로 생성하지 않았다.
- 프롬프트·출처·Unity 가져오기 설정은 [균열 바닥 이미지 생성 기록](Rift_Stone_Prompt.md)에 보존했다.

## 첫 캐릭터 3D 모델 (2026-09-10)

Unity 편집기의 AI 도구로 야만전사 참고 이미지 4장과 3D 모델 1개를 만들어 `Assets/HELLSCRIPT/Art/Characters/Barbarian`에 두었다. 골격 없는 임시 몸체를 대신할 첫 캐릭터 시도이며 출시 승인 자산이 아니다.

| 파일 | 생성 모델 | 용도 |
|---|---|---|
| `Barbarian_Reference_v1.png` | Gemini 3.0 Pro (`gemini-3.0-pro`) | 정면 T자세 참고 이미지 |
| `Barbarian_Reference_Back.png` | Gemini 3.0 Pro | 후면 참고 이미지 |
| `Barbarian_Reference_Left.png` | Gemini 3.0 Pro | 좌측면 참고 이미지 |
| `Barbarian_Reference_Right.png` | Gemini 3.0 Pro | 우측면 참고 이미지 |
| `Barbarian_Assets/selected.fbx` 와 `Barbarian.prefab` | Tripo P1 Multi-View (`model3d-tripo-p1-multiview`) | 정면·후면·좌측면 이미지를 입력한 다중 시점 3D 생성 결과 |

정면 참고 이미지의 프롬프트 요지는 다음과 같다. 어두운 고딕 판타지의 근육질 야만전사를 머리부터 발끝까지 정면 T자세로 담고, 두 팔을 어깨 높이에서 수평으로 곧게 뻗게 한다. 나머지 세 장은 같은 인물의 정체성·비율·복장·색을 유지한 채 시점만 바꾸도록 요청했다. 3D 생성 프롬프트는 모피와 가죽 갑옷에 낡은 황동 장식과 은은한 호박빛을 넣은 저·중 폴리곤 게임용 캐릭터를 T자세로 요청했다.

각 이미지는 생성 후 배경 제거를 한 번 더 적용했다. 시드는 지정하지 않았다.

생성 원본과 프롬프트 기록은 프로젝트 루트의 `GeneratedAssets` 캐시에 자산 GUID별로 보존된다. 이 폴더는 가져온 결과와 같은 파일을 중복 보관하는 도구 캐시이므로 저장소에 포함하지 않는다. 확인이 필요한 프롬프트·모델명은 위 표와 이 문단에 옮겨 적었다.

`Assets/HELLSCRIPT/Scenes/Hellscript.unity`의 최상위에 `Barbarian_Preview` 프리팹 인스턴스를 배치했다. 원점에 놓고 세워 보이도록 회전만 적용한 확인용 배치이며, 기존 전투의 영웅 표현을 이 모델로 교체한 것은 아니다. 실행하면 시작 씬에 함께 보이므로 정식 연결 전까지는 확인용이라는 점을 구분한다.

## 이전 효과음 뱅크 (2026-09-26)

아래는 이전 DSP 합성음의 출처 기록이다. 현재 음원 교체는 [ElevenLabs 게임 음향](ElevenLabs_Game_Audio.md)을 따른다.

- `Assets/HELLSCRIPT/Resources/Audio/Sfx`: 효과음 283종, WAV 469개, 48 kHz·16-bit 모노. `tools/generate_sfx.py`와 `tools/sfx`의 시드 고정 DSP 제작법으로 합성한 원본이다.
- 외부 녹음·샘플·음성 합성 엔진·AI 음향 모델을 쓰지 않았다. 디아블로 4는 질감의 참고로만 삼았으며 그 게임의 음원은 추출하거나 사용하지 않았다.
- 목소리(함성·신음·죽음·괴물 소리)도 성문 펄스와 성도 공명기 모델로 합성한 비언어 발성이다. 실제 사람의 목소리를 녹음하거나 흉내 낸 것이 아니다.
- 출시 음향 승인과 청감 평가를 마친 자산이 아니다. 자세한 구성과 검증: [효과음 전면 교체](Sound_Effects_Bank.md).

## ElevenLabs 게임 음향 (2026-09-27)

- 효과음은 `eleven_text_to_sound_v2`, 배경음은 `eleven_music_v2`를 사용했다. 공식 MCP의 실제 생성 결과에서 모델 식별자를 확인했다.
- 선택한 원본 MP3와 생성 ID·전체 프롬프트·매개변수·원본 및 편집본 SHA-256을 `AudioSources/ElevenLabs`에 보존한다. 게임은 48 kHz PCM16 WAV를 Unity에서 압축해 사용한다. WAV 변환을 무손실 원본 생성으로 표시하지 않는다.
- 사람의 청감 검토와 출시 승인은 별도 상태로 남긴다. 전체 구성·편집·검증: [한국어](ElevenLabs_Game_Audio.md) · [English](ElevenLabs_Game_Audio.en.md).

English: effects use the verified `eleven_text_to_sound_v2` model and music uses `eleven_music_v2` through the official MCP. Selected MP3 sources, prompts, parameters, generation IDs and source/master SHA-256 hashes are retained in `AudioSources/ElevenLabs`. Runtime WAV conversion does not recover lost MP3 detail. Human listening review and release approval remain separate.

## 절차적 3D 제작 도구와 월드 셰이더 (2026-09-28)

- `tools/art3d/hs3d.py`, `tools/generate_world_art.py`, `tools/art3d/build_one.py`: Blender 5.2를 명령줄로 실행해 모델을 만드는 저장소 내부 도구다. 형태는 시드 고정 기하 연산으로, 재질은 Blender 기본 절차 노드로 만들고, 알베도·거칠기·노멀을 2배 해상도로 구운 뒤 줄인다. 외부 메시·텍스처·스캔·폰트를 내려받지 않았고, 이미지·3D 생성 AI 모델을 쓰지 않았다.
- `Assets/HELLSCRIPT/Tests/Editor/Fixtures/WorldArt/`: 가져오기 규칙을 검사하려고 `tools/art3d/fixture_probe.py`로 만든 시험용 모델과 텍스처다. `Resources` 밖에 있어 게임 빌드에 들어가지 않는다.
- `Assets/HELLSCRIPT/Resources/RiftTerrain.shader`, `WorldFx.shader`, `WorldTelegraph.shader`와 `Resources/Rendering/WorldPost_*.asset`: 이 저장소에서 직접 작성한 셰이더와 후처리 설정이다.
- 디아블로 4는 어둡고 무게감 있는 분위기의 참고로만 삼았다. 그 게임의 모델·텍스처·이펙트·이름·로고는 추출하거나 옮기지 않았다.
- 이 도구로 만든 영웅·일반 적·보스·필드·소품·마을 모델의 소스와 자산은 2026-09-28 전체 브랜치 통합에 포함했다. 일부 모델의 실제 게임 화면 연결은 후속 작업으로 남아 있다. 모든 결과물은 개발용 후보 자산이며 출시 승인을 뜻하지 않는다. 자세한 내용: [다크 고딕 개편 1단계](Dark_Gothic_Overhaul_Stage1.md).

## 출석 이벤트 진입 아이콘 — 2026-09-28

`Art/Attendance/event-attendance.png`는 달력·체크·보상 상자가 있는 원형 황동 아이콘이다. 네이티브 RGBA 원본을 변경 없이 복사했다. 내장 도구의 `transparent_background=true`를 사용했고 실제 투명 픽셀과 가장자리 알파를 검증했다. 모델 선택 인자와 반환 모델 정보가 없어 `gpt-image-2` 제작으로 단정하지 않는다. 상태는 `candidate_model_unknown`, 개발용이며 출시 아트 승인은 별도다. [제작 기록](../Art/Attendance/event-icon-manifest.json)과 [알파 검사](../Art/Attendance/event-icon-validation.json)를 참조한다.

## 월드 이펙트 텍스처 (2026-09-28)

- `Assets/HELLSCRIPT/Resources/World/Fx/*.png` 34장과 `manifest_fx.json`: `tools/generate_fx_textures.py`가 이름마다 고정한 시드(`hellscript-fx-v1|<이름>`의 sha256)로 numpy·PIL 연산만 써서 만든다. 값 노이즈, 메타볼, 무작위 걸음 균열, 기하학 문양으로 구성했고 사진·스캔·내려받은 이미지·이미지 생성 AI·다른 게임의 이펙트를 쓰지 않았다. `--check`로 설치된 파일을 다시 만들어 비교할 수 있다.
- Android 텍스처 예산의 `World/Fx/` 규칙(최대 1024, ASTC 6×6)이 적용된다. 개발용 후보 자산이며 출시 승인을 뜻하지 않는다. 사용처: [적·보스 공격 예고 게이지](Attack_Telegraph_Gauge.md).

## 훈위안 생성 마을 3D 모델 (2026-10-06)

- `Assets/HELLSCRIPT/Resources/World/Town/`의 FBX 20개, PNG 40장과 `manifest_town_hunyuan.json`: 텐센트 훈위안3D 웹(`hy3d.tencent.ai`, 텍스트→3D, PBR 켬, 모델 HY3D-V3.1, 목표 5만 면, 유료 기능 미사용)이 2026-10-06에 만든 GLB 20개(NPC 12 + 마을 오브젝트 8)를 Blender 5.2로 줄이고 다시 구운 것이다. 원본 GLB, 생성 설정, 모델별 프롬프트와 SHA-256은 저장소 밖 납품 폴더 `AssetDeliverables/Hunyuan/2026-10-06/`의 `artifact-lifecycle.json`에 있고, 경량화·검사 기록은 같은 폴더의 `optimized/`에 있다. 매니페스트는 모델마다 경량화한 GLB(`bakedGlb`)의 이름과 SHA-256을 적는다.
- 변환 도구는 `tools/hunyuan_town_export.py`, `tools/import_hunyuan_town.py`다. 도구는 면 수·UV·텍스처 해상도만 바꾸며 형태와 색은 생성물 그대로다. 생성물이 기존 저작물과 닮았는지는 확인하지 않았다.
- 이용 조건(라이선스)을 검토하지 않았다. 매니페스트는 `productionApproved: false`이며 개발용 후보다. 출시 전에 상업 이용·저작권 귀속·표시 의무를 확인해야 한다. 구성과 검증: [마을 3D 모델 적용](Town_Hunyuan_Models.md).
