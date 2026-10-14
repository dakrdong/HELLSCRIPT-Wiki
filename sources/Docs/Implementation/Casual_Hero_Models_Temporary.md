# 캐주얼 영웅 3D 모델 (임시 적용)

작성일: 2026-10-15 · [English](Casual_Hero_Models_Temporary.en.md)

새 아트 컨셉(밝은 캐주얼 StyleD 치비)의 영웅 3D 모델을 임시로 게임에 보이게 했다. 사용자 결정(2026-10-15): 지금 모델을 그대로 임시로 쓰고, 나중에 유료 생성 등으로 바꿀 수 있다.

| 직업 | 설치 키 | 원본 | 상태 |
| --- | --- | --- | --- |
| 전사 | `HunyuanReady/Characters/Hero_Warrior` | 남전사 HW02 (훈위안 이미지→3D, `--hand-capture` 리깅) | 팔·손이 몸에 붙어 움직임. 준비 자세에서 머리 위에 얇은 파란 조각이 보인다 |
| 궁수 | `HunyuanReady/Characters/Hero_Ranger` | 남궁수 HW17 (훈위안, 단순화본) | 비교적 깨끗함. 손·허벅지 옆에 작은 조각이 있다 |
| 마법사 | `HunyuanReady/Characters/Hero_Mage` | 남마법사 M42 (2026-10-15 텍스트→3D 재생성, 짧은 소매·맨팔 프롬프트, T자→A포즈 전처리, 손 캡처 없이 리깅) | 다리는 걷고 팔은 A포즈로 고정된다(팔 흔들림 없음). 조각 분리 없음. 처음 넣었던 HW82는 준비 자세에서 손이 떨어져 나가 교체했다 |
| 여전사 | `HunyuanReady/Characters/Hero_Warrior_F` | 여전사 HW52 (텍스트→3D T자→A포즈, `--hand-capture`) | 팔이 움직임. 준비 자세에 머리끝 작은 조각 |
| 여궁수 | `HunyuanReady/Characters/Hero_Ranger_F` | 여궁수 HW08 (이미지→3D, `--hand-capture`) | 팔을 들 때 긴 머리가 함께 들린다 |
| 여마법사 | `HunyuanReady/Characters/Hero_Mage_F` | 여마법사 F33 (2026-10-15 재생성, M42와 같은 방식) | 팔 고정형, 조각 분리 없음 |

- 코드: 기존 `HeroBodyArt` 키(`Characters/Hero_*`)는 그대로이며, `HunyuanReadyArt`가 같은 키의 READY 모델을 절차 모델보다 먼저 쓴다. READY 모델에는 무기 소켓이 없으므로 `WorldView.Actors.cs`가 소켓이 없을 때 손·등 뼈에 무기를 붙인다(`Pivot_Weapon`→`Pivot_Hand_R`, `Pivot_Shield`→`Pivot_Hand_L`, `Pivot_Quiver`→`Pivot_Chest`, 장착 당시의 정렬 회전 유지). 기존 절차 모델 파일은 그대로 두며, READY 폴더를 지우면 이전 모델로 돌아간다.
- 경량화: 모델마다 LOD 6000/3000/1500 삼각형, 512 px 텍스처. 폴더당 약 5.6–6.0 MB.
- 기록: 리깅 도구와 전처리(`tools/hunyuan_monster_ready.py --hand-capture`, `tools/art3d/tpose_to_apose.py`), 입력 그림과 시도 결과는 `claude/hero-rig-inputs` 브랜치의 `Docs/Art/CasualConcept/2026-10-14/Characters/HunyuanInput/README.md`에 있다.
- 여성 영웅 3종(`Hero_*_F`)은 사용자 요청(2026-10-15, 품질이 낮아도 임시로 넣기)에 따라 READY 모델로 설치했다. 게임에는 아직 성별 선택이 없어 화면에 나오지 않으며, 성별 선택을 붙일 때 이 키를 쓴다.
- 마법사 시도(2026-10-15): 훈위안 6회 생성 중 5회분(후보 20개)을 리깅했다. `--hand-capture`를 쓰면 손이 팔뚝에서 떨어져 날아가고(판정 실패 또는 눈 검토 탈락), 손 캡처 없이 리깅하면 팔이 몸통에 고정된 채 통과했다. 팔이 움직이는 마법사는 모자·소매를 분리한 입력이나 유료 생성이 필요하다. 원본은 `AssetDeliverables/Hunyuan/2026-10-15/heroes_text/`에 있다(저장소 밖).

## 검증

- 관련 EditMode(WorldArt·TownModel·HunyuanMonsterReady): 86개 통과, 실패 0 (Hero_Warrior/Ranger/Mage 포함).
- macOS 개발 빌드 런타임 스모크(`-hellscriptMonsterModelsSmoke`): 전사가 균열 전투 화면에 나타남([화면](CasualHeroModelsEvidence20261015/warrior-in-rift.jpg)). 궁수·마법사의 게임 화면은 이번에 캡처하지 못했다. 리깅 판정 시트: [전사](CasualHeroModelsEvidence20261015/warrior-rig-sheet.jpg), [궁수](CasualHeroModelsEvidence20261015/ranger-rig-sheet.jpg), [마법사 HW82(교체 전)](CasualHeroModelsEvidence20261015/mage-rig-sheet.jpg), [마법사 M42](CasualHeroModelsEvidence20261015/mage-m42-rig-sheet.jpg), [여전사](CasualHeroModelsEvidence20261015/warrior-f-rig-sheet.jpg), [여궁수](CasualHeroModelsEvidence20261015/ranger-f-rig-sheet.jpg), [여마법사](CasualHeroModelsEvidence20261015/mage-f-rig-sheet.jpg).
- 2026-10-15 추가 설치 뒤 HunyuanMonsterReadyTests 37개 통과(7개 영웅 폴더 포함). 여성 모델과 새 마법사의 게임 화면 스모크는 돌리지 않았다.
- 실기기·성능·빌드 크기는 검증하지 않았다.
