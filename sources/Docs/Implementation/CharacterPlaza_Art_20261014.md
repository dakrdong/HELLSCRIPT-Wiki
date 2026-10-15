# 캐릭터 광장 원화·리소스 인계

2026-10-14 · [English](CharacterPlaza_Art_20261014.en.md)

시안 정본은 [CharacterPlaza README](../Design/CharacterPlaza/README.md), 원본은 [아트 README](../Art/CasualConcept/2026-10-13/CharacterPlaza/README.md)다. 이 기록은 원화 공급과 정적 검증만 다룬다. Unity 가져오기·화면 동작·메모리 측정은 단일 최종 통합 검증 대기다.

## 공급과 재현

- `Assets/HELLSCRIPT/Resources/Art/CharacterPlaza/`에 PNG 41개, 장면 JSON 2개와 `art.json`을 공급했다. 원본 장면 2장·포즈 12장은 바이트 그대로 복사했다. 장면 영웅층 12·가림 조각 12·인장 3은 정본 WebP의 디코딩 픽셀을 PNG로 저장했다. 전체 clean 배경 2장과 시안의 옛 장비 아틀라스는 Resources에 추가하지 않았다.
- [준비·검사 도구](../../tools/prepare_character_plaza_art.py): `python3 tools/prepare_character_plaza_art.py --from-webp`로 재현하고 `--verify`로 읽기 전용 검사한다. [영수증](../../tools/character_plaza_art_receipt.json)에 원본 17개와 공급 41개의 SHA256·경로·GUID·가공 방법을 기록했다.
- 먼저 원본 `lift.swift`를 성공적으로 컴파일하고 `prep_scene.py spec_landscape.json`을 실행했다. 첫 `warrior_m` 분리에서 macOS Vision Code 9, `Could not build inference plan / failed to load ANE model` 환경 오류가 발생했다. 세로 분리는 실행하지 않았다. 승인된 대안인 정본 WebP를 사용했으며 네이티브 분리가 다시 성공했다고 보고하지 않는다.
- WebP 자체의 손실 압축은 PNG 디코딩으로 복원되지 않는다. 장면 배경 원본과 영웅층의 미세한 색·경계 차이는 최종 실제 화면에서 확인한다. 새 imagegen 호출은 없었다.

## 데이터 계약

장면 원점은 왼쪽 위다. `scene_landscape`·`scene_portrait`은 `width`, `height`, `background`, `heroes[]`, `focus[]`를 제공한다. 영웅은 `artId`, `cut`/`patch`의 리소스·x/y/width/height, `headX/Y`, `feetX/Y`, `cheerHeight`다. `archer_*`는 게임의 `HeroClass.Ranger`다. 장면 12명의 모든 박스·기준점·세로 focus는 정본 `scene_data.json`과 정확히 같다.

`art.json`의 `poses[]`는 여섯 artId별 cheer/fight의 리소스·UV·잘린 폭/높이를 가진다. 원본 PNG 주변에는 매우 낮은 알파의 점이 있어 UV 기준은 alpha > 8이다. 원본 픽셀을 지우거나 다시 그리지 않았다. UV y는 Unity의 아래쪽 원점이다. `crests[]`는 `heroClass`·`resource`를 가진다.

cut 12개·pose 12개의 `hitMask`는 64×64, 위쪽 행부터, byte 내부 little-bit 순서의 Base64다. 런타임 원본 텍스처 CPU 읽기 없이 사용한다. 신규 `.meta`는 안정적인 UUIDv5 GUID, `isReadable:0`, 밉맵 끔, Clamp, Bilinear를 지정한다.

## 예산과 공통 담당 요청

PNG 파일 합계는 **31,524,964 bytes (30.07 MiB)**다. 원본 해상도 RGBA 환산은 95,228,440 bytes이고, 아래 cap·ASTC6×6을 적용한 이론 합계는 5,956,496 bytes (5.68 MiB)다. 실제 Unity 크기·상주 메모리 측정이 아니다.

공통 `ResourceTextureBudget.cs`는 직접 수정하지 않았다. 앞에서부터 매칭할 행은 다음과 같다. Android ASTC6×6, Standalone BC7, WebGL ETC2_RGBA8의 동일 cap을 신규 `.meta`에 넣었다.

| Resources 접두사 | 최대 크기 |
|---|---:|
| `Art/CharacterPlaza/landscape_bg` | 2048 |
| `Art/CharacterPlaza/portrait_bg` | 2048 |
| `Art/CharacterPlaza/cheer_` | 1024 |
| `Art/CharacterPlaza/fight_` | 1024 |
| `Art/CharacterPlaza/crest_` | 256 |
| `Art/CharacterPlaza/` | 512 |

## 실제 픽셀 조사

정본 참조 JPEG 28장 전체의 픽셀을 네 contact 이미지로 열어 가로·세로·PC, KO/EN, 둘러보기·선택·새 영웅·생성·장비·입장·균열 상태를 확인했다. 장면 PNG 2장과 자세 12장도 열어 확인했다. contact는 검사 보조이며 원본 참조를 대체하지 않는다.

- 가로·PC에서는 앞쪽 술집의 전사, 오른쪽 과일 가게의 궁수, 뒤쪽 마법 상점의 마법사가 같은 한 장면에 남는다. 선택은 흰 실루엣 외곽선과 직업색으로 구분한다.
- 세로에서는 Mage/Ranger/Warrior 인장 순서와 세 카메라 구역, 위쪽 크림 안개, 생성 시 자세 교체를 확인했다. 세로 마법사 1.2배 확대의 원화 선명도 한계는 원본에 이미 있다.
- 화이팅 6장은 빈손이며 입장 6장은 클래스별 무기가 그림에 고정돼 있다. 실제 저장 장비를 바꿀 때 해당 무기 외형이 동적으로 바뀌는 원화는 아니다. 여전사 입장은 원래 모델 시트의 도끼 대신 검이다.
- 생성/입장 포즈 12개 모두 alpha 최대값은 254다. 포즈의 낮은 알파 점과 일부 원본의 가장자리 접촉은 보존했다. 본문 가독성·잘림의 최종 판단은 실제 Unity 화면에서 한다.
- 장면에서 분리한 영웅층은 Vision 가공이며 네이티브 알파가 아니다. 모든 공급품의 `productionApproved:false`를 유지했다. 여전사 복장은 원본 지시에 따라 12세 등급 검토 대상으로 남긴다.

## 기존 승인 6명과 구 리소스 보존

독립 정면 원화는 `/Users/t8g-2410-pn-005/Documents/Codex/2026-10-13/task-4/output/hellscript-main-characters-6/delivery/`의 `hero-<class>-<gender>-v01.png`다. 감독 원장 `task-3/casual-art-brief/DIRECTOR_STATUS.json`·`FINAL_MAIN_CHARACTERS_QA.json`의 exact-hash pass와 실제 파일 6/6이 일치했고, 밝은 배경 contact의 실제 픽셀을 확인했다. 이 여섯 정면 원화로 마을의 자연스러운 행동 자세를 덮어쓰지 않았다. 별도 `CharacterSelect/` 옛 후보와도 다르다.

| ID | 승인된 원본 SHA256 |
|---|---|
| hero-warrior-m | `501832b1fac8f3546040e065b2e249c2193c5463a113b476f9c4db82451b7116` |
| hero-warrior-f | `decb611d0b52abcbc7385a05b69d7d120fe8287aa9245dd46e8ece59d3ed3d08` |
| hero-mage-m | `3729d8767b2231cd78458409341abf2620eddafa3a98830900fe6574037ebbd2` |
| hero-mage-f | `a4ba90f23479a7637db371bbdb5153039c34ab9f1cb2cdcaf45c00fbd411f8bd` |
| hero-ranger-m | `899b16d3f8d5e378e14f411ab5c51a5b58b298015f98aa94e129507139620ba1` |
| hero-ranger-f | `c7b3c0df1a505754062bbce36ded7ce884187cf0a6b7f3eb887b868a75b8e80b` |

구 `Art/CharacterSelection/{Warrior,Mage,Ranger}` 아틀라스는 `CharacterEquipmentView` → `CharacterFigure.Load`와 대장간 등 공유 경로가 사용하므로 보존한다. `SelectionCourtyard`도 새 실제 화면 전환과 전체 참조 검증 전에는 보존한다. 원본·.meta·이력·3D 영웅 자산을 이동/삭제하지 않았다.

## 검증 상태와 산출물

정적 검사: 원본 17 SHA, 런타임 41 SHA·GUID, 장면 12명 좌표, 24개 클릭 마스크, 6포즈 쌍·3인장, GPU-only/밉맵 설정, 전체 clean 배경 미포함을 통과했다. 실제 Unity 검사 0회다. 전체 빌드와 세이브/입력 검증은 통합 담당이 수행한다.

임시 Swift binary·모듈 캐시·실패 재현 입력·contact·검사 결과는 작업 폴더 `validation/character-plaza-art/` 하나에 보존했고 `artifact-lifecycle.json`에 기록했다. 실행한 Unity/게임/웹 빌드는 없고 커밋·push·병합·영구 삭제도 하지 않았다.
