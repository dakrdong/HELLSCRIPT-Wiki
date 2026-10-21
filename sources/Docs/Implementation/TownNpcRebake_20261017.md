# 캐주얼 마을 NPC 7종·상자 더미 재제작 · 2026-10-17

[English](TownNpcRebake_20261017.en.md)

품질 보류 중이던 캐주얼 NPC 7명(자크 체이, 구젤 판, 달크 알뷔, 쟝 죠린, 차도르 사마프, 인젤 미르, 미슈 카루)과 상자 더미를 새로 경량화해 게임에 넣었다. 7명은 기존 `Npc_*` 파일을 교체하고 `.meta`·GUID를 유지했다. 상자 더미는 새 `Prop_CrateStack`이며, 두 상자 자리의 기존 F1 상자(2.5배)를 1배 크기로 바꿨다. 이미 적용된 마을 8종, 몬스터 N05·N14·N21·N22·N24, 가랜드, 남·여 마법사는 범위에서 제외했다.

## 원인과 바꾼 방법

10-16 후보는 훈위안 원본 UV를 그대로 둔 채 면만 줄이고 노멀만 다시 구웠다(`decimate_normal_only.py`). 원본 UV는 잘게 쪼개진 섬으로 되어 있어서, 면을 줄이면 한 삼각형이 섬 경계를 가로지르게 된다. 그러면 금장·문양이 톱니로 끊기고, 경계에 검은 삼각형이 생기며, 얼굴 색이 번진다.

이번에는 승인된 건물·소품과 같은 방식을 썼다(`blender_bake.py`). 면을 줄인 사본에 Smart UV로 UV를 새로 펴고, 색·ORM·노멀 세 장을 모두 고해상 원본에서 다시 굽는다. 섬 사이 여백은 0.004에서 0.002로 줄였고, 그 결과 UV 사용률이 17%에서 25~46%로 올랐다.

| 모델 | 면 | 텍스처 | UV 각도 | 8뷰 SSIM | 국소 결함(blob) | UV 겹침 |
|---|---|---|---|---|---|---|
| 자크 체이 | 6,000 | 1024 | 85° | 0.966 | 0 | 0.34% |
| 구젤 판 | 6,000 | 1024 | 66° | 0.969 | 0 | 0.09% |
| 달크 알뷔 | 6,000 | 1024 | 85° | 0.947 | 0 | 0.43% |
| 쟝 죠린 | 6,000 | 1024 | 85° | 0.970 | 0 | 0.74% |
| 차도르 사마프 | 6,000 | 1024 | 85° | 0.940 | 0 | 1.23% |
| 인젤 미르 | 6,000 | 1024 | 45° | 0.961 | 0 | 0.61% |
| 미슈 카루 | 6,000 | 1024 | 66° | 0.930 | 0.33% | 1.98% |
| 상자 더미 | 2,500 | 512 | 66° | 0.939 | 0 | 0 |

- **인젤:** 85°에서 망토 안쪽이 바깥 무늬를 받았다(UV 겹침 4.9%, blob 4.45%). 그래서 45°를 썼다.
- **미슈:** blob 0.33%는 정면 셔츠 가장자리의 밝기 차이다. Unity 근접 화면에서는 원본에 없던 세로선·삼각 자국이 보이지 않았다.
- **NPC 텍스처를 512에서 1024로 올린 이유:** 512에서는 새 UV로도 카드 마름모·코트 문양이 다시 흐려졌다(자크 SSIM 0.945 대 1024의 0.966). 마을 폴더의 임포트 상한은 원래 1024이고, `generate_world_art.py`의 npc 예산만 1024로 바꿨다. 안드로이드 ASTC 6x6 기준으로 7명 × 2장, 약 +6~7MB다.

**상자 더미:** 요청대로 훈위안 Text-to-3D로 새 상자 1건(후보 4개)을 생성했다. 하지만 네 후보 모두 위가 뚫린 상자였고, 금속 광택이 강해 컨셉(닫힌 뚜껑·평면색)에서 멀었다(같은 방식으로 구운 SSIM 0.861). 기존 원본(`f58965a5…glb`)을 새 방식으로 다시 구운 버전을 채택했다. 판재와 대각 지지대 접합부의 검은 얼룩은 Blender 8뷰와 Unity 근접 화면 모두에서 없어졌다.

시도했다가 버린 방법: 머리 쪽 면을 덜 줄이는 정점 가중 감축은 몸통 형태를 망가뜨렸다(SSIM 0.80). 면 수를 10,000으로 올려도 입 안의 어두운 선은 거의 그대로였다. 둘 다 쓰지 않았다.

## 근거

원본·Codex 후보·새 버전을 같은 카메라·조명으로 비교했다(Blender, 1024px 렌더에서 얼굴·몸통을 확대).

- [자크 체이](TownNpcRebakeEvidence20261017/compare-jacques-chei.jpg)
- [구젤 판](TownNpcRebakeEvidence20261017/compare-guzel-pan.jpg)
- [달크 알뷔](TownNpcRebakeEvidence20261017/compare-darc-alvi.jpg)
- [쟝 죠린](TownNpcRebakeEvidence20261017/compare-jean-jorin.jpg)
- [차도르 사마프](TownNpcRebakeEvidence20261017/compare-chador-samaf.jpg)
- [인젤 미르](TownNpcRebakeEvidence20261017/compare-injel-mir.jpg)
- [미슈 카루](TownNpcRebakeEvidence20261017/compare-mishu-karu.jpg)
- [상자 더미 8뷰 비교](TownNpcRebakeEvidence20261017/compare-casual-crate-stack.jpg)

macOS 개발 빌드의 `-hellscriptTownModelsSmoke`에 근접 촬영 단계를 추가했다. 실제 게임 재질·조명으로 정면·측면·후면·게임 시점을 찍고, 찍는 동안 다른 물체는 숨긴다.

- [NPC 1~4](TownNpcRebakeEvidence20261017/unity-closeups-npc-1.jpg)
- [NPC 5~7과 상자](TownNpcRebakeEvidence20261017/unity-closeups-npc-2-crate.jpg)
- [게임 화면 갬블 상인 앞](TownNpcRebakeEvidence20261017/game-near-gambler.jpg)

검사:
- 작업 중 관련 EditMode: TownModelTests와 TownEnvironmentTests 24/24 통과.
- 마을 모델 스모크 통과(`HELLSCRIPT_TOWN_MODELS_SMOKE_OK`).
- `generate_world_art.py --check`: 문제 수가 기존 2,419건과 같다(새 문제 없음).

재현 자료:
- 굽기 도구 사본, 최종 GLB 8개와 SHA256: `AssetDeliverables/Hunyuan/2026-10-17/town-rebake/` (저장소 밖)
- 새로 생성한 상자 후보: `AssetDeliverables/Hunyuan/2026-10-17/town-crate/`
- 원본 RAW: Downloads의 기존 파일을 읽기만 했다.

## 남은 점

- **입 경계:** 원본은 입이 움푹 들어간 형상이라, 6,000면에서는 윗니·입술 경계에 얇은 어두운 선이 남는다(달크·미슈). Codex 후보의 검은 삼각 조각보다는 작지만 완전히 원본과 같지는 않다.
- **미검증:** 실기기·안드로이드 ASTC 실물 화면·성능 비교·영어 화면 반복은 하지 않았다. 게임에서 NPC는 1080p 기준 약 70px 높이로 보인다.

## 최종 게임 연결 확인 · 2026-10-17

동일한 최종 macOS 개발 빌드에서 마을 모델 스모크를 통과했다. NPC·주민·7개 건물·포털의 모델 연결, 재제작 8종의 4방향 근접 촬영, 10개 시설 이동과 상점 가림 처리를 확인했다. [최종 결과](TownNpcRebakeEvidence20261017/runtime-town-models-smoke-final.txt). 관련 EditMode는 몬스터 마무리와 합친 156건에서 한 번 실행했고 마을 24건이 통과했다.

실행에는 격리 세이브와 `-hellscriptOfflineQa`를 사용했다. 첫 실행은 이 옵션 누락으로 캐릭터 선택 전에 실패했고, 옵션을 보완해 마을 검사만 재실행했다. 근거와 빌드는 `Builds/Validation/ModelIntegration20261017/`에 보존한다. 실기기·ASTC 화면·성능·영어 반복 및 위키 게시는 미검증이다.

최종 기준 보완: 마을 PR #141이 공유 체크아웃에 반영되어 최종 게임 빌드에도 포함됐다. 그 변경 후 마을 검사만 27/27 다시 통과했다. 앞선 마을 24건을 최신 마을 소스의 검증으로 재사용하지 않았다.
