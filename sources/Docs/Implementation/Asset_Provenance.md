# HELLSCRIPT 임시 리소스 출처

작성일: 2026-09-08

## 내장 이미지 생성

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
