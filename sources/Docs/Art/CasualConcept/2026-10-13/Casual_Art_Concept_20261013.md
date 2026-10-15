# 캐주얼 아트 컨셉 전환 시안 (2026-10-13)

작성일: 2026-10-13 · 수정일: 2026-10-14 · [English](Casual_Art_Concept_20261013.en.md)

## 결정

- 아트 컨셉을 다크 고딕에서 **밝고 아기자기한 캐주얼 중세 판타지(12세 이상)** 로 바꾼다. UI·전투·저장·화면 구성과 게임 배경 설정은 그대로 두고 아트만 바꾼다.
- 두 컨셉을 옵션으로 병행하지 않고 한 방향으로 전환한다. 기존 상태는 `main`의 태그·브랜치로 보존한다.
- 플레이어 캐릭터는 전사·마법사·궁수 각 남녀 **6명**이다.
- 화풍은 네 후보(청키 브롤러 / 앵귤러 / 코믹 잉크 셀 / 소년만화 치비) 중 **D "소년만화 치비"** 로 정했다. 토리야마(드래곤퀘스트)와 쿠키런 킹덤의 태도를 기준으로, 3.5~4등신, 둥근 몸에 뾰족한 머리·날카로운 눈·송곳니 같은 포인트를 얹는다.
- 렌더링은 2단 셀 셰이딩(평면 색 + 하드 섀도 1단), 균일한 외곽선, 무광 재질이다. 훈위안(Hunyuan3D) 결과물을 최소 가공으로 쓸 수 있도록 캐릭터당 약 6,000 삼각형·512px 텍스처로 만들 수 있는 큰 덩어리 형태만 쓴다.

## 파일

| 파일 | 내용 |
|---|---|
| [title_key_art_v05_styleD.png](title_key_art_v05_styleD.png) | 타이틀 키 아트. 상단 40%는 로고 자리로 비워 둠 |
| [battle_encircled_v02_styleD.png](battle_encircled_v02_styleD.png) | 균열 전장에서 몬스터에게 포위된 전투 준비 장면. 몬스터는 로스터 가족(해골·시체·가시뿔·거미·익사자·망령·광신자·지옥 하수인) 기준 |
| [Characters/sheet_1_warrior_m.png](Characters/sheet_1_warrior_m.png) | 남전사 설정 시트: 정면·3/4·측면·후면 A포즈, 표정 3종, 무기·방패 |
| [Characters/sheet_2_warrior_f.png](Characters/sheet_2_warrior_f.png) | 여전사 설정 시트 |
| [Characters/sheet_3_mage_m.png](Characters/sheet_3_mage_m.png) | 남마법사 설정 시트 |
| [Characters/sheet_4_mage_f.png](Characters/sheet_4_mage_f.png) | 여마법사 설정 시트 |
| [Characters/sheet_5_archer_m.png](Characters/sheet_5_archer_m.png) | 남궁수 설정 시트 |
| [Characters/sheet_6_archer_f.png](Characters/sheet_6_archer_f.png) | 여궁수 설정 시트 |

각 그림 옆의 `PROMPT_*.md`가 요청 프롬프트, `manifest_*.json`이 원본 경로·SHA-256·크기 기록이다. 이전 시안(v01~v04, 화풍 비교 A~D, 첫 라인업)은 사용자의 요청으로 삭제했다.

## 제작 방법

- Codex CLI 내장 `image_gen`(`codex exec -m gpt-6-astra`)으로 생성했고 PNG는 생성 원본을 바이트 그대로 복사했다. 도구가 모델명을 알려주지 않아 manifest의 model은 `unknown`이며, 다른 Codex 아트와 같이 출시 승인 전이다.
- 설정 시트는 영웅 6명을 병렬 실행 6개로 한 번에 받았다. 정체성 기준은 화풍 비교 때 뽑은 D 라인업이며 각 시트 프롬프트에 영웅별 외형을 글로 적었다.

## 훈위안 3D 테스트 (남전사)

남전사 시트의 정면 컷([HunyuanInput/warrior_m_front.png](Characters/HunyuanInput/warrior_m_front.png))을 훈위안(hy3d.tencent.ai) Image-to-3D V3.1에 넣어 첫 모델을 만들었다. 업로드는 사용자가 내장 브라우저에서 직접 했고 면 수는 1.5M(의도는 50k였으나 결과에는 유리)이었다. 결과 GLB(80.3 MB)와 미리보기는 저장소 밖 `AssetDeliverables/Hunyuan/2026-10-13/heroes/`에 두고, `tools/hunyuan_monster_ready.py --id HW01 --archetype Biped --height 1.8`로 게임 티어(6,000/3,000/1,500 삼각형, 512px A·C·N·ORM) 베이크와 자동 리깅을 돌렸다. 모든 QA 게이트를 통과했다(91초).

| 파일 | 내용 |
|---|---|
| [warrior_m_preview.png](Characters/HunyuanTest/warrior_m_preview.png) | 훈위안 렌더 미리보기(512px) |
| [warrior_m_HW01_contact_sheet.png](Characters/HunyuanTest/warrior_m_HW01_contact_sheet.png) | 게임 티어 LOD0 비교 시트: 정면·측면·후면·49도 게임 시점·걷기·준비·공격 자세 |
| [warrior_m_hunyuan_test.json](Characters/HunyuanTest/warrior_m_hunyuan_test.json) | 생성 ID·면 수·원본 해시·QA 게이트 값 |

결론: 뾰족한 머리, 송곳니 웃음, 파란 스카프, 별 문양 방패 같은 D 화풍의 특징이 6,000 삼각형·512px에서도 읽힌다. 다만 몬스터용 피벗 리그라 오른다리 뼈가 템플릿으로 대체됐고 준비 자세에서 머리 위로 늘어난 조각이 보이므로, 영웅은 게임 적용 전에 사람형 리그를 따로 만들어야 한다. 실기기 성능과 라이선스(`productionApproved: false`)는 확인하지 않았다.

### 2차: 무기 없는 입력과 손 고정

1차 결과는 낮게 든 칼이 몸 옆에 떠 있는 조각으로 붙었고 이동 자세에서 칼·방패가 깨졌다. 게임은 무기·장비를 갈아끼우는 구조라 영웅 입력에 칼·방패를 넣지 않는다(장비별 외형과 기본 차림은 연출을 정할 때 다룬다. 지금은 갑옷 차림을 유지한다). 코덱스로 빈손 정면 A포즈([warrior_m_front_noweapon.png](Characters/HunyuanInput/warrior_m_front_noweapon.png))를 만들어 50k 면으로 다시 생성했다([미리보기](Characters/HunyuanTest/warrior_m_noweapon_preview.png)).

같은 도구로 구우면 모든 게이트는 통과하지만([HW02](Characters/HunyuanTest/warrior_m_noweapon_HW02_contact_sheet.png)) 걷기 자세에서 손가락이 떨어졌다. 원인: 허벅지 옆에 늘어진 손가락 정점을 열 가중치가 엉덩이·허벅지·가슴 뼈에 줬다(오른손 손목 아래 정점의 약 35%). `tools/hunyuan_monster_ready.py`에 `--hand-capture` 옵션을 더해, 손목에서 팔 방향으로 신장의 16% 안·축에서 8% 안의 정점(앞으로 굽은 손가락 포함)을 손 뼈에 붙였다. 다시 구운 [HW03](Characters/HunyuanTest/warrior_m_noweapon_HW03_handcapture_contact_sheet.png)은 걷기·준비·공격 자세에서 손이 팔을 따라간다. 옵션을 쓰지 않으면 기존 몬스터 결과는 바뀌지 않는다. 남은 것: 준비 자세에서 어깨 위로 늘어나는 스카프 조각. 장비 교체에는 모든 영웅이 공유하는 사람형 골격이 필요하므로 이 리그는 미리보기용이다.

## 마을 시안

같은 화풍으로 지금의 어두운 마을을 밝게 다시 그렸다. 배치와 시설은 그대로다(십자 도로, 교차로의 균열 관문, 북쪽 창고·무기 가판대·돌우물·대장간·허수아비 4개의 훈련장, 창고 옆 룬 제단, 남쪽 상점 4채, 가로등·울타리·나무).

| 파일 | 내용 |
|---|---|
| [town_overview_gameview_v01.png](Town/town_overview_gameview_v01.png) | 게임 카메라(약 50° 내려다보는 시점)에서 본 마을 전체. 현재 배치를 그대로 따름 |
| [town_plaza_eyelevel_v01.png](Town/town_plaza_eyelevel_v01.png) | 눈높이에서 본 중앙 광장 키 아트 |

## 캐릭터 선택 화면 시안

직업(전사·궁수·마법사)마다 성별이 둘이 되어, 선택 화면에서 **직업을 고른 뒤 성별(남자·여자)을 골라** 게임에 들어가는 구성이다. 닉네임 입력은 없다(게임 진행 중 튜토리얼에서 만든다). 디자인 캔버스: https://claude.ai/artifact/K17toUvjigyghBaRN1A8D4 (25장: 세로·가로·PC × 한/영, 직업·성별 선택 상태, 준비 중, 입장 중, 균열 진행 중).

- 구성은 현재 선택 화면(`Docs/Implementation/Character_Selection.md`)을 따른다: 타이틀로 돌아가기·연출·언어, 제목, 세 영웅(고른 영웅이 가운데로 나오고 나머지는 물러남), 직업 버튼, 상세, 시작하기, 안내 문구. 새로 생긴 것은 **성별 버튼 두 개**와 영웅 그림 6장이다. 성별을 바꾸면 세 영웅이 모두 그 성별로 바뀐다.
- 상세에는 직업의 대표 스킬 3개(스킬 트리 데이터의 아이콘과 이름)를 보인다.
- 균열이 진행 중이면 기존 규칙대로 그 캐릭터만 고를 수 있다. 직업과 성별이 모두 고정되고 나머지는 잠김으로 보인다.
- 문구는 기존 것(전투 태세, 성소 입장, 직업 한 줄 소개)을 쓰고, 제목·부제·성별·입장 중은 새로 제안한 것이다.
- 그림: `CharacterSelect/hero_<직업>_<성별>.png` 6장(1024×1536, 투명 배경, 코덱스 생성)과 밝은 성소 마당 `selection_courtyard_casual.png`(1536×1024). 프롬프트·manifest를 함께 두었다. 모델명은 도구가 알려주지 않아 `unknown`이며 출시 승인 전이다. 윗머리가 위쪽 가장자리에 닿는 영웅(여전사)이 있다.
- 미정: 기존 저장 캐릭터(직업별 레벨)를 선택 화면에 어떻게 보일지, 한 계정이 직업·성별 조합을 여럿 가질 수 있는지.
- 이 시안은 아래 "캐릭터 생성·선택 화면: 마을 장면"으로 대체됐다.

## 캐릭터 생성·선택 화면: 마을 장면 (2026-10-14)

로그인 뒤 캐릭터 생성과 선택·입장을 같은 화면에서 한다. 던전 앤 드래곤 아케이드처럼 모험을 떠나기 전 파티가 마을에 모여 있는 장면을 오마주했다. 디자인 캔버스: https://claude.ai/artifact/M43Cy6wg7BJUqpKxofu21M (28장: 세로 11·가로 12·PC 5, 한/영).

- 장면은 시장 거리다. 앞쪽 술집 테라스에 전사 남(나무 술잔을 들이킴)·전사 여(술통에 앉아 닭다리), 가운데 과일 가게 앞에 궁수 남(사과 값 흥정)·궁수 여(멜론과 바구니), 뒤쪽 분수 옆 마법 상점에 마법사 남(고서 읽기)·마법사 여(물약 살펴보기)가 있다. 영웅은 무기를 들지 않는다.
- 첫 시도(빈 광장에 소품을 든 영웅을 줄지어 세움)는 배치가 부자연스럽다는 피드백으로 버렸다. 각 영웅이 그럴듯한 장소에서 그럴듯한 행동을 하는 한 장의 장면을 기준으로 다시 만들었다.
- 가로·PC 화면은 장면 전체를 한 화면에 보인다. 세로 화면은 같은 장면을 세로로 다시 구성한 그림을 쓴다. 오른쪽 인장(위부터 마법사·궁수·전사)을 누르면 카메라가 그 직업 두 명 쪽으로 이동하며 확대된다. 먼 줄일수록 더 확대해 거리 안쪽으로 들어가는 느낌을 준다.
- 세로 인장은 다른 버튼과 같은 잉크 외곽선을 두른 65px 둥근 버튼이다. 화면 높이 가운데에 112px 간격으로 놓고, 고른 직업은 직업 색 테두리와 빛으로 표시한다. 인장과 캐릭터가 겹치지 않도록 직업별 카메라 위치를 왼쪽으로 맞췄다.
- 선택하지 않은 캐릭터는 채도를 낮춰 비활성으로 보인다. 누르면 색이 돌아오고 흰 외곽선과 직업 색 빛이 켜지며, 발밑에 직업 이름표가 붙는다.
- 한 번도 플레이하지 않은 캐릭터는 머리 위에 +가 있다. 누르면 그 자리에서 일어나 화이팅 자세로 성격에 맞는 한마디를 1.8초 외치고 원래 자세로 돌아간다. 그다음 머리 위에 Lv.1과 사람 아이콘이 생긴다. 사람 아이콘은 착용 장비 팝업(무기·머리·가슴·손·발·허리띠·목걸이·반지 8칸)을 연다.
- 만든 캐릭터를 고르면 하단 가운데에 플레이 버튼이 나온다. 누르면 캐릭터가 착용 무기(전사 검, 궁수 활, 마법사 수정 지팡이)를 든 전투 준비 자세로 바뀌어 한마디 하고 입장 중 상태가 된다. 균열이 진행 중이면 기존 규칙대로 그 캐릭터만 고를 수 있고 나머지는 잠김으로 보인다.

대사(새 문구 제안, 게임에 넣을 때 문구 키로 옮긴다):

| 캐릭터 | 생성(화이팅) | 입장(전투 준비) |
|---|---|---|
| 전사 남 | 좋아, 한판 붙어 보자고! / Alright, let's brawl! | 다 덤벼라! 앞장은 내가 선다! / Come at me! I'll lead the charge! |
| 전사 여 | 배도 채웠겠다, 덤벼! / Belly's full. Bring it on! | 몸 다 풀렸어. 전부 날려 줄게! / All warmed up. I'll send them flying! |
| 궁수 남 | 흥정 끝! 이제 사냥 시간이야. / Deal's done. Hunting time! | 한 발이면 충분하지! / One shot's all I need! |
| 궁수 여 | 조용히, 확실하게. 따라와. / Quiet and sure. Follow me. | 숨 고르고… 놓치지 않아. / Breathe… I won't miss. |
| 마법사 남 | 책에 다 나와 있었다고! / It was all in the book! | 계산 끝. 불꽃 맛 좀 봐라! / Calculations done. Taste the flames! |
| 마법사 여 | 반짝반짝, 마법 시작이야! / Sparkle sparkle, magic time! | 자, 마법 쇼를 시작해 볼까? / Shall we start the magic show? |
- 시안의 예시 계정: 전사 남 Lv.24, 궁수 여 Lv.17, 마법사 여 Lv.31(진행 중인 균열), 나머지 셋은 아직 만들지 않음.
- 이 화면에는 연출·언어 버튼을 두지 않는다. 게임에서 연출은 항상 켜져 있고, 언어는 설정에서 바꾼다(사용자 결정, 2026-10-14). 지금 선택 화면에 있는 두 버튼은 새 화면을 구현할 때 뺀다.

제작 방법:

- Codex가 인물이 들어간 장면을 한 장으로 그렸다. 가로는 후보 2장 중 영웅이 더 크고 깊이감이 있는 쪽을 골랐고, 세로는 가로 장면을 참조해 다시 구성했다. 인물만 지운 배경판(`clean_*.png`)도 Codex 편집으로 만들었다.
- 캐릭터 레이어는 macOS Vision 피사체 분리(`VNGenerateForegroundInstanceMaskRequest`, [tools/lift.swift](CharacterPlaza/tools/lift.swift))로 장면 픽셀을 그대로 잘라 냈다. 장면 위 같은 자리에 겹치므로 선택·비활성 효과가 그림과 정확히 맞는다. 화이팅 중에는 배경판 조각으로 원래 인물을 가리고, 무기 없는 화이팅 전신 그림(Codex)을 그 자리에 세운다. 자르는 범위와 기준점은 [tools/spec_*.json](CharacterPlaza/tools/spec_landscape.json), 처리 스크립트는 [tools/prep_scene.py](CharacterPlaza/tools/prep_scene.py)에 있다. 처음 결과에서 궁수 두건과 세로 여마법사 모자 끝이 잘려 외곽선이 끊겼다. 지금 스크립트는 잘라 낸 결과가 범위 가장자리에 닿으면 그쪽을 넓혀 다시 자르고, 기준점과 이어진 덩어리만 남기며, 두 캐릭터가 겹친 픽셀은 앞(더 가까운) 캐릭터에게 준다. 세로 여마법사 뒤에 붙은 행인은 제외 영역으로 뺐다.
- 분리와 조각은 시안용 가공이다. 게임 에셋 규칙(네이티브 알파만)과 다르므로 실제 게임에서는 3D 캐릭터와 애니메이션으로 다시 만든다.

| 파일 | 내용 |
|---|---|
| [scene_landscape.png](CharacterPlaza/scene_landscape.png) | 가로·PC용 마을 장면(1536×1024) |
| [scene_portrait.png](CharacterPlaza/scene_portrait.png) | 세로용 마을 장면(1024×1536) |
| [clean_landscape.png](CharacterPlaza/clean_landscape.png) · [clean_portrait.png](CharacterPlaza/clean_portrait.png) | 영웅만 지운 배경판(화이팅 연출용) |
| [cheer_warrior_m.png](CharacterPlaza/cheer_warrior_m.png) 외 5장 | 영웅별 화이팅 자세(투명 배경) |
| [fight_warrior_m.png](CharacterPlaza/fight_warrior_m.png) 외 5장 | 영웅별 전투 준비 자세(착용 무기, 투명 배경). 남궁수의 "활을 겨누는" 첫 요청은 Codex 안전 필터에 막혀, 활을 들고 엄지를 세운 자세로 다시 그렸다 |
| [crests.png](CharacterPlaza/crests.png) | 직업 인장 3종(전사·궁수·마법사) |

각 그림 옆에 프롬프트(`PROMPT_*.md`)와 manifest가 있다. `PROMPT_cheer_*.md`에는 함께 요청한 소품 포즈도 적혀 있지만, 그 그림은 첫 시도와 함께 버렸다. 모델명은 도구가 알려주지 않아 `unknown`이며 출시 승인 전이다.

미결:

- 전사 남의 술잔은 그대로 둔다(사용자 결정, 2026-10-14).
- 착용 장비 팝업의 아이콘은 아직 예전 어두운 화풍이다.
- 세로 마법사 구역은 1.2배 확대라 조금 흐리다.
- 장면 속 NPC(과일 장수, 마법 상점 주인, 행인 1명)는 선택 대상이 아니다.
- 브라우저 화면은 로컬 Chrome 정적 렌더로만 확인했다. 캔버스 안의 실제 동작과 실기기 화면은 확인하지 않았다.

## 미결 사항

- 나머지 영웅 5명을 같은 방법으로 만든다. 내장 브라우저에는 파일 업로드 도구가 없어서 이미지 전달 방법(직접 업로드, 사용자 크롬의 업로드 도구, base64 주입)을 먼저 정해야 한다.
- 영웅용 사람형 리그와 게임 적용(플레이어 모델 교체)은 별도 작업이다.
- 진행 중인 다크 고딕 3D 오버홀과 일일 몬스터 생성 루틴은 새 컨셉과 충돌한다. 계속·중단·일부 유지 결정이 필요하다.
- 여전사 복장(배꼽 노출)은 12세 기준 검토가 필요하다.
- 전투 장면 앞쪽 임프에 약한 피사계 심도 블러가 남아 있다. 두 번 재생성해도 같았다.
