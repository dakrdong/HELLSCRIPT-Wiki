# 캐주얼 마을 전면 재구성 · 2026-10-17

[English](Town_Casual_Rebuild_20261017.en.md)

사용자 요청("기존 마을 구성은 잊고 시안처럼 아기자기한 캐주얼 마을로 새로 구상, 카메라·라이팅·셰이더도 필요하면 수정")에 따라 승인 시안 [town_overview_gameview_v01](../Art/CasualConcept/2026-10-13/Town/town_overview_gameview_v01.png)을 설계도로 삼아 마을을 다시 배치했다. 서비스 10종·NPC·대화·거래 동작과 저장 키는 그대로이고, 위치·크기·꾸밈·바닥·조명·셰이더만 바뀌었다.

![시안 대비](TownJuicyEvidence20261017/compare.jpg)

왼쪽 위 시안, 오른쪽 위 새 마을(시안과 같은 1536×1024 구도), 왼쪽 아래 이전 마을, 오른쪽 아래 실제 플레이 카메라.

## 배치

시안을 약 18px/m로 측정해 미터 좌표로 옮겼다. 울타리 안쪽은 75×53m(이전 118×87m), 걸을 수 있는 범위는 70×48m이고 큰길 횡단은 12.7초다.

| 위치 | 시설 |
| --- | --- |
| 북쪽 줄(서→동) | 창고, 무기 노점, 우물, 대장간, 훈련장(모래·3면 울타리·앞쪽 입구) |
| 서쪽 큰길 가운데 | 위상 각인석(둥근 돌판 광장, 사용자 지정 위치) |
| 십자로 남쪽 가장자리 | 균열 관문(둥근 돌판 광장) |
| 남쪽 줄(서→동) | 갬블·보석·룬·룬 공방, 앞마당과 상점 골목 |
| 문 | 서·동(큰길), 남(남북 길) |

건물 모델은 기존 Hunyuan 모델을 배율 1.0~1.35로 키웠고 충돌 사각형은 모델 크기×배율이다. 남쪽 줄은 지붕이 화면에서 큰길을 가리지 않도록 깊이·높이를 맞췄다. 마을 안 나무 12그루와 우물은 `TownLayout.Obstacles`로 길찾기가 돌아가며, 영웅이 뒤에 서면 건물처럼 반투명해진다.

## 그림과 셰이더

- 바닥: Codex가 시안 그림체로 그린 잔디·자갈길·광장 돌판·모래 타일([원본·프롬프트](../Art/CasualConcept/2026-10-17/TownGround/PROMPT_town_ground.md)). `tools/make_town_ground.py`가 이음새 보정·512px·노멀맵을 만들고 런타임 파일을 같은 바이트로 재현한다. 바닥 셰이더는 잔디 색 얼룩과 길 가장자리의 은은한 그늘을 더했고 진흙 테두리를 없앴다.
- 잔디 덤불·들꽃: Codex 스프라이트 2종을 한 장 아틀라스로 묶어 약 4천 개를 메시 하나로 깐다(`TownScatter.shader`, 바람 흔들림, 그림자 받기만).
- 툰 셰이더 `TownToon.shader`: `Town/` 모델만 적용. 한 단계 명암(그림자 면은 청보라 톤), 위쪽 면의 햇빛 강조, 하늘빛 가장자리, 1.4px 갈색 외곽선(역방향 껍질). 균열 화면은 기존 `RiftTerrain` 그대로다.
- 조명: 해를 화면 왼쪽 앞에서 비추게 돌려(yaw 40) 정면이 밝고 그림자가 오른쪽으로 보이게 했다. 주변광을 시원하게, 대비 +10, 따뜻한 필터. 마을 입자는 회색 먼지 대신 꽃가루·꽃잎·반짝임.
- 나무·덤불은 색 변형 7종, 울타리는 짙은 갈색, 깃발 줄은 가로등 사이에 더 크게 걸었다.

## 검증

- 관련 Edit Mode 13개 클래스 206건 통과(병합 뒤 재실행). 전체 Edit Mode는 `main`의 2026-10-17 규칙에 따라 돌리지 않았다. 자세한 내용은 [receipt](TownJuicyEvidence20261017/receipt.json).
- macOS 개발 빌드(`main` 병합 뒤 트리): 마을 모습 스모크(26장)와 모델 스모크 가로 1600×900·세로 900×1600 통과(모든 건물·NPC가 모델, 10개 서비스 도달, 노점 뒤 반투명).
- 미검증: 실기기·Android·iOS·WebGL 화면과 성능, 그리기 호출 수, 영어 화면 반복, 외곽선의 모바일 GPU 비용. 성능 개선은 주장하지 않는다(덤불 메시 1개·외곽선 패스가 추가됨).

![플레이 카메라](TownJuicyEvidence20261017/gameplay-crossroads.jpg)
![각인석](TownJuicyEvidence20261017/gameplay-aspect-stone.jpg)
![남쪽 상점](TownJuicyEvidence20261017/gameplay-south-row.jpg)
![HUD 가로](TownJuicyEvidence20261017/hud-landscape.jpg)
![HUD 세로](TownJuicyEvidence20261017/hud-portrait.jpg)


## 2026-10-17 merge validation / 병합 검증

Latest main f8b8e039 merged without conflicts. CI-equivalent UI contract (15), skill generators and Python tests (3 + 3 + 18) passed; generated tracked files unchanged. Existing town evidence (206 focused tests and macOS smokes) is reused because the town runtime/test inputs did not change; later main changes affect the guide/growth and rift monster rendering. No full Edit Mode or new player build ran. GitHub Actions jobs did not start due to account billing/spending restrictions.

최신 main을 충돌 없이 통합했고 CI와 동일한 로컬 검사를 통과했다. 마을 소스가 동일하여 기존 마을 검증 근거를 재사용한다. 위키 생성은 기존 문서 이력의 절대 경로 링크에서 중단되었다(`Nonportable local Markdown link: Docs/Implementation/Casual_UI_Skin_Rules_20261014.en.md`). 앞선 main에도 공개 원본 스냅샷 목록 변경으로 게시가 막힌 기록이 있다. 위키 검사·테스트·공개 배포는 완료하지 않았으며 부분 게시하지 않았다. / Wiki generation stopped on an existing nonportable absolute link in document history. Earlier main also records a changed public-original snapshot inventory. Wiki checks/tests and public delivery remain incomplete; no partial publication was made.

Small logs, generated history and materialized historical wiki inputs are retained in `Builds/Validation/TownMerge20261017/` and its lifecycle manifest, review 2026-10-24; exact permanent deletion is not authorized. / 검사 기록과 재생성 산출물은 보존하며 영구 삭제는 미승인이다.
