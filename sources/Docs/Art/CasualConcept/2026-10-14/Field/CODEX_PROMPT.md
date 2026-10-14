# Codex 작업 프롬프트: 자동 생성형 던전(균열 필드)을 캐주얼 StyleD로 교체

## 목표
균열(자동 생성형 던전)의 6개 필드(F0 묘지, F1 성채, F2 사막, F3 동굴, F4 초원, F5 고원)의 화면을 어두운 고딕에서 밝고 아기자기한 캐주얼 StyleD로 바꾼다. 게임 규칙·방 배치·시드·저장 키·전투 결과는 바꾸지 않는다. 첫 단계는 F4 한 필드(대표)이고, 대표가 통과하면 나머지 5개로 넓힌다.

## 현재 상태 (확인한 사실)
- main에는 필드/던전의 캐주얼 적용이 없다. 캐주얼은 몬스터 초상화·도감, 타이틀, 일부 UI까지 들어갔다.
- 던전의 어두운 표현 소유자: `Assets/HELLSCRIPT/Runtime/Presentation/FieldLook.cs`(필드별 조명·안개·후처리·입자), `WorldView.Rift.cs`(바닥·벽·트림 타일, 벽 기둥·횃불), `WorldView.Props.cs`(장애물·장식 `Fields/F<n>/F<n>_<Kind>`), `Resources/World/Fields/F0..F5`(어두운 FBX와 타일 `_A/_N`), `WorldArt.cs`·`WorldArtImporter`(임포트 규칙).
- 1차 캐주얼 모델 키트(미연결, Docs 전용): 브랜치 `claude/casual-field-kit` 커밋 `63195b38`, 로컬 워크트리 `/Users/t8g-2410-pn-005/Documents/MyProject/HELLSCRIPT/HELLSCRIPT/.claude/worktrees/casual-field-kit`. 산출물은 `Docs/Art/CasualConcept/2026-10-14/Field/F0..F5/`(타일 3종 + 모델 9종씩, `manifest_fields_casual.json`)와 `preview_F*.png`. 생성기는 `tools/art3d/recipes_fields_casual.py`(`blender -b --factory-startup -P ... -- F4 --preview`).
- 키트 한계: 노멀맵 없음, 알파=0.1 고정(무광), 팔레트 아틀라스 64px, F5 눈 바닥 무늬 얼룩, 벽이 스튜디오 조명에서 어둡게 보임. 필요하면 생성기를 고쳐 다시 만든다.

## 참조 (먼저 열어서 본다)
- `Docs/Art/CasualConcept/2026-10-13/battle_encircled_v02_styleD.png`(밝은 초원 전투 장면, 필드 색감의 기준)
- `Docs/Art/CasualConcept/2026-10-13/Town/town_overview_gameview_v01.png`, `title_key_art_v05_styleD.png`
- `Docs/Art/Art_Transition_20261014.md` 2장(캐주얼 기준: 하늘색·풀 초록·따뜻한 노랑·코랄·크림, 굵은 외곽선·평면색·경계 뚜렷한 그림자 1단, 무광, 피·잔혹·음침한 연출 제외)

## 해야 할 일 (단계)
1. **대표 F4**: 1차 키트의 F4 타일·모델을 `Resources/World/Fields/F4/`의 같은 이름(`Floor_A/_N`, `Wall_A/_N`, `Trim_A/_N`, `F4_Pillar` 등)으로 연결한다. 어두운 원본은 삭제하지 말고 Git 이력으로 보존한다. 노멀맵은 평평한 `_N`을 만들어 로더 규격(`_A` 알파=매끈함, `_N`)을 지킨다. `.meta`·GUID 규칙과 `WorldArtImporter` 설정을 따른다.
2. `FieldLook.Fields[4]`(grassland)를 밝은 프리셋으로: 낮은 채도 보정 제거, 밝은 하늘색 환경광, 옅은 안개, 따뜻한 햇빛 키라이트, 약한 비네트, 노을 입자 → 꽃가루·나비 같은 밝은 입자. 안개 시작/끝은 가독성을 우선하고, 미탐색 바닥이 검게 그려지는 규칙(카메라 배경 검정)은 유지하되 보이는 범위의 밝기를 해치지 않는다.
3. 장애물 크기: 장식이 아닌 장애물(Pillar, Rubble, Wall 등)은 기존 footprint·height 계약(`manifest`의 footprint/height, 게임 판정 반경)과 어긋나지 않게 맞춘다. 충돌·시야·AI 경로는 건드리지 않는다.
4. **가독성이 최우선**: 빨간 예고 장판·적 탄·영웅 90px 캐릭터가 바닥 위에서 또렷해야 한다. 바닥은 중간 밝기·저대비를 유지하고, 예고색(빨강)·피해 표식과 겹치는 색을 바닥에 쓰지 않는다.
5. 대표 F4가 통과하면 F0, F1, F2, F3, F5를 같은 방식으로 한 필드씩 추가한다. 필드마다 `FieldLook`·타일·모델을 함께 바꾸고, 필드별 분위기 이름(수정 동굴=보랏빛, 눈 고원=밝은 흰색 등)은 키트 팔레트(`PAL`)를 따른다.
6. 보스·훈련장·레거시 링(`FieldLook.Legacy`)·마을은 이번 범위가 아니다.
7. **튜토리얼 균열 (사용자 결정 2026-10-14)**: 튜토리얼 균열은 항상 F0이므로 F0을 밝게 바꾸면 튜토리얼도 함께 밝아진다. 이 변화를 그대로 받아들인다(F0만 어둡게 남기는 분기 금지). 대신 F0 단계에서 튜토리얼 대사·연출·가이드(여신) 문구·토스트·설명 이미지에 "어둡다·으스스하다·묘지·달빛·안개·무덤" 같은 이전 분위기를 전제한 표현이 있는지 점검한다. `Localization`의 한·영 키와 튜토리얼 코드(`Tutorial*`, 가이드 단계, 프롤로그)를 검색해 목록을 만들고, 밝은 화면과 어긋나는 문구만 한·영 키로 함께 고친다(키 이름·저장 키는 유지, 두 언어 모두 채움). 어긋나지 않는 문구는 건드리지 않는다. 점검한 파일과 고친 키, 고치지 않은 이유를 보고에 적는다. 튜토리얼 스모크(관련 `Tutorial*` 테스트와 가이드 스모크)를 F0 변경 뒤 한 번 돌려 흐름이 깨지지 않았는지 확인한다.

## 지켜야 할 것
- 게임 문구는 한·영 키로(새 문구가 생기면). 변수·주석·커밋 메시지·로그는 영어. 저장 키 변경 금지. 기본 글자 크기·UI 변경 없음.
- 새 Resources 텍스처는 `Editor/ResourceTextureBudget.cs`에 행을 추가한다(`hellscript-apk-size-budget`). 용량이 늘면 수치를 보고한다.
- 2D 그림은 직접 그리지 않는다(필요하면 Codex 이미지 생성). 이번 작업은 절차 제작 모델과 타일이므로 외부 생성기 없이 생성기 스크립트로 만든다.
- 몬스터 파이프라인(`monster_specs.json`, 일일 루틴)과 `claude/monsters-*` 브랜치는 건드리지 않는다.

## 검증
- 작업 중에는 관련 테스트만: `FieldTests`, `AuthoritativeRiftTests`, `TownGroundMaskTests`, `ResourceTextureBudget` 검사, 필드 스모크(`RuntimeFieldSmoke`). 필드마다 macOS 개발 빌드 또는 에디터 화면 캡처 2장(균열 진입 직후, 전투 중 예고 장판이 보이는 장면)을 `Docs/Implementation/CasualFieldEvidence20261014/`에 jpg로 두고 문서에서 파일마다 링크한다.
- 최종 코드 변경과 최신 main 병합 뒤 한 번만 전체 EditMode, 런타임 스모크, `python3 tools/check_ui_contract.py --verify`를 돌린다. 이미 main에서 실패하던 검사와 새 실패를 구분한다. 문서만 바꾸면 게임 실행 검사는 다시 하지 않는다.
- 성능 주장은 같은 조건의 이전·이후 바이너리 각각 3회 이상 원자료가 있을 때만 한다. 없으면 "미검증"으로 적는다. macOS 결과를 실기기 결과로 보고하지 않는다.
- 위키 원본이 바뀌면 `AgentRules/Wiki_Publish.md`대로 병합 후보에서 한 번 생성·검사하고 병합 뒤 공개 반영까지 확인한다.

## 보고 (한국어, 짧게)
바꾼 필드와 파일, 가독성 확인 장면, 테스트 수치(통과·실패·제외), 용량 변화, 미검증 범위(실기기·성능·빌드 크기), 남은 필드.
