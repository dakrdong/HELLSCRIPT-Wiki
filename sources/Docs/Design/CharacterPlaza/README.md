# 캐릭터 생성·선택 화면 시안 (마을 장면)

작성일: 2026-10-14 · [English](README.en.md)

로그인 뒤 **캐릭터 생성과 선택·입장이 같은 화면**이다. 던전 앤 드래곤 아케이드처럼 모험을 떠나기 전 파티가 마을에 모여 있는 장면이고, 여섯 영웅(전사·궁수·마법사 각 남녀)이 시장 거리에서 각자 자연스럽게 시간을 보낸다.

- 디자인 캔버스: https://claude.ai/artifact/M43Cy6wg7BJUqpKxofu21M (28장, 비공개 — 사용자가 공유하기 전에는 Codex가 열 수 없다. 이 폴더의 사본이 정본이다)
- 아트 원본·프롬프트·분리 도구: [Docs/Art/CasualConcept/2026-10-13/CharacterPlaza](../../Art/CasualConcept/2026-10-13/CharacterPlaza/README.md)
- 게임 구현 지시: [Casual_Character_Plaza_Codex_Prompt_20261014.md](../../Implementation/Casual_Character_Plaza_Codex_Prompt_20261014.md)

## 폴더

| 경로 | 내용 |
|---|---|
| `source/` | 캔버스에 게시한 파일 그대로(`PlazaSelect.dc.html` 컴포넌트 하나 + 아트보드별 얇은 래퍼 27개 + `canvas.json`). 이미지는 `/_blob/<id>`로 참조하며 `art_map.json`이 로컬 파일에 연결한다 |
| `assets/` | 캔버스가 쓰는 이미지(webp). 장면 2, 영웅 잘라낸 층 12, 화이팅 조각 12, 화이팅 자세 6, 전투 준비 자세 6, 인장 3, 장비 아이콘 모음 1(시안용 예전 어두운 화풍) |
| `reference/` | 아트보드 28장의 렌더(JPEG). 게임 화면을 나란히 비교하는 기준 |
| `scene_data.json` | 두 장면의 그림 픽셀 좌표 자료(영웅별 잘라낸 박스, 머리·발 기준점, 화이팅 박스, 세로 카메라 틀). `tools/prep_scene.py`가 만든 것 |
| `tools/` | `smoke.js`(상태 흐름·배치 검사), `render_reference.js`(참조 렌더), `to_jpg.py` |

## 화면 구성

모든 상태와 크기를 한 컴포넌트(`source/PlazaSelect.dc.html`)가 그리며, props는 `locale`(ko·en), `orient`(portrait·landscape·pc), `hero`(none 또는 영웅 6), `start`(normal·cheer·created·gear·entering·rift), `focus`(세로 카메라 직업)이다. 레이아웃 숫자는 `layout()`, 상태 규칙은 `renderVals()`가 정본이다.

- **가로(956×440)·PC(1152×648을 1.25배)**: 장면 전체를 한 화면에 덮어 맞춘다(cover). 앞쪽 술집 테라스에 전사 남녀, 가운데 과일 가게에 궁수 남녀, 뒤쪽 분수 옆 마법 상점에 마법사 남녀가 있다.
- **세로(440×956)**: 세로로 다시 그린 장면을 쓰고, 오른쪽 인장 세 개(위부터 마법사·궁수·전사)를 누르면 카메라가 그 직업 두 명 쪽으로 이동하며 확대한다(0.9초, `cubic-bezier(.45,.05,.2,1)`). 인장은 65px, 화면 높이 가운데에 112px 간격이며 고른 직업은 직업 색 테두리와 빛이 켜진다. 보는 쌍 위쪽은 크림색 안개로 가려 두 명만 읽히게 한다.
- **상단**: 뒤로 가기(‹ 타이틀)와 제목만 둔다. **연출 켜짐·언어 버튼은 없다**(사용자 결정: 연출은 항상 켜짐, 언어는 설정에서 변경).
- **하단**: 선택 전에는 안내 문구 알약, 만든 캐릭터를 고르면 가운데에 초록 플레이 버튼.

## 상태와 전환

| 상태 | 보이는 것 | 다음 |
|---|---|---|
| 둘러보기 | 여섯 모두 비활성(채도 .45, 밝기 .88). 하단 "캐릭터를 눌러 선택하세요" | 영웅을 누름 → 선택 |
| 선택(안 만든 영웅) | 흰 외곽선 + 직업 색 빛, 발밑 직업 이름표(직업명 + 성별 기호), 머리 위 초록 + 단추. 하단 "+를 눌러 새 모험가를 만들어요". 플레이 없음 | + 누름 → 화이팅 |
| 화이팅(생성) | 그림 속 영웅을 배경 조각으로 가리고 화이팅 자세를 세운다. 1.8초 동안 뛰며(뛰기 애니메이션 1.7초) 머리 위 말풍선에 영웅 성격 대사 | 1.8초 뒤 원래 자세로 돌아가며 생성 완료 → 레벨 Lv.1 |
| 선택(만든 영웅) | 머리 위 Lv. 알약 + 사람 아이콘, 하단 가운데 플레이. 안내 "선택한 캐릭터로 성소에 입장합니다." | 사람 아이콘 → 장비 팝업, 플레이 → 입장 |
| 장비 팝업 | 가운데 패널에 착용 장비 8칸(무기·머리·가슴·손·발·허리띠·목걸이·반지, 이름·슬롯·레벨, 등급 색). 오른쪽 위 닫기 | 닫기 |
| 입장 | 그림 속 영웅을 가리고 전투 준비 자세(착용 무기)를 세우며 말풍선 대사, 플레이 버튼은 "입장 중…"으로 비활성, 화면 전체에 옅은 크림 막. 안내 "성소로 향하는 중…" | 게임 입장 |
| 균열 진행 중 | 균열 캐릭터만 선택 가능(시안은 마법사 여 Lv.31). 나머지는 흑백 + 자물쇠. 안내 "진행 중인 균열의 캐릭터로 이어갑니다." | 플레이 |

생성·입장 중에는 다른 영웅과 인장을 눌러도 반응하지 않는다. 영웅을 고르면 세로 카메라가 그 직업으로 따라간다.

## 규격

- 색: 잉크 #2a3150, 크림 #fff6e3. 직업 색(테두리/글자/연한 바탕): 전사 #ff9f1c/#e08a00/#fff1d6, 궁수 #4cbf55/#2a8a36/#e5f7e0, 마법사 #35a7ff/#1f7fd1/#e3f2ff. 플레이 버튼 #8ee07a→#4cbf55, 잉크 3px 테두리와 4px 단단한 그림자, 글자는 흰색에 잉크 2px 외곽선. 제목은 노랑 #ffd23f에 잉크 외곽선. 글꼴은 Jua(제목·숫자·단추)와 Noto Sans KR(본문) — 게임에서는 `UiTheme`이 소유한 글꼴을 쓴다.
- 이름표 96×22(가로)·108×24(세로·PC), 머리 위 줄: + 단추 32(가로)·38, 레벨 알약 높이 26·30, 사람 단추 28·32, 말풍선 최대 폭 200·210 글자 13·15px. 화면 안에 남도록 가운데를 제한하고(가로 112~W−112, 세로·PC 118~), 위쪽은 제목 줄 아래(가로 60, 세로 160, PC 108)로 제한한다.
- 시안의 + 단추(32·38px)와 사람 단추(28·32px)는 게임의 44pt 터치 규칙보다 작다. 보이는 크기는 유지하고 눌리는 영역만 44pt 이상으로 넓힌다.
- 안정된 시간: 화이팅 1.8초(뛰기 1.7초), 전투 준비 튀김 0.55초, 말풍선 등장 0.5초, + 단추 위아래 1.2초 반복, 플레이 버튼 반짝임 5.6초 반복, 세로 카메라 이동 0.9초.
- 시안 예시 계정: 전사 남 Lv.24, 궁수 여 Lv.17, 마법사 여 Lv.31(균열 진행 중). 나머지 셋은 아직 만들지 않음. 장비 팝업의 장비 이름·레벨·아이콘은 예시이며 게임에서는 영웅이 실제로 착용한 장비를 공통 부품으로 보인다.

## 저장과의 관계

사용자가 2026-10-14에 **영웅 6명 저장**으로 정했다. `AccountSave.heroes`가 여섯 영웅(인덱스 0~2 남자 전사·궁수·마법사, 3~5 여자 전사·궁수·마법사)을 항상 가지고, 영웅마다 `gender`와 `created`(만들었는지)를 둔다. 이 화면의 "+"가 `created`를 켠다. 구현 지시와 단계는 [Casual_Character_Plaza_Codex_Prompt_20261014.md](../../Implementation/Casual_Character_Plaza_Codex_Prompt_20261014.md)의 "결정" 절에 있다.

## 문구

이미 있는 문구(재사용): `‹ 타이틀`, `선택한 캐릭터로 성소에 입장합니다.`, `진행 중인 균열의 캐릭터로 이어갑니다.`, `성소로 향하는 중…`, `닫기`, 직업명 `전사`·`궁수`·`마법사`, 장비 슬롯 이름. 새 문구는 아래이며 한국어 원문이 키다.

| 한국어(키) | English |
|---|---|
| 나만의 모험가를 골라요 | Choose your adventurer |
| 캐릭터를 눌러 선택하세요 | Tap a hero to choose |
| +를 눌러 새 모험가를 만들어요 | Tap + to create this adventurer |
| 새 모험가 만들기(접근성) | Create a new adventurer |
| 플레이 | PLAY |
| 입장 중… | Entering… |
| 착용 장비 | Equipped gear |
| 착용 장비 보기(접근성) | View equipped gear |
| 남자 / 여자 | Male / Female |
| 균열 진행 중에는 선택할 수 없어요(자물쇠 안내용, 시안에서는 표시하지 않음) | Locked while a rift is in progress |

영웅 대사(성격에 맞춘 새 문구):

| 영웅 | 생성(화이팅) | 입장(전투 준비) |
|---|---|---|
| 전사 남 | 좋아, 한판 붙어 보자고! / Alright, let's brawl! | 다 덤벼라! 앞장은 내가 선다! / Come at me! I'll lead the charge! |
| 전사 여 | 배도 채웠겠다, 덤벼! / Belly's full. Bring it on! | 몸 다 풀렸어. 전부 날려 줄게! / All warmed up. I'll send them flying! |
| 궁수 남 | 흥정 끝! 이제 사냥 시간이야. / Deal's done. Hunting time! | 한 발이면 충분하지! / One shot's all I need! |
| 궁수 여 | 조용히, 확실하게. 따라와. / Quiet and sure. Follow me. | 숨 고르고… 놓치지 않아. / Breathe… I won't miss. |
| 마법사 남 | 책에 다 나와 있었다고! / It was all in the book! | 계산 끝. 불꽃 맛 좀 봐라! / Calculations done. Taste the flames! |
| 마법사 여 | 반짝반짝, 마법 시작이야! / Sparkle sparkle, magic time! | 자, 마법 쇼를 시작해 볼까? / Shall we start the magic show? |

## 아트보드(참조 화면 28장)

| 참조 화면 | 크기 | orient | locale | hero | start | focus | 보이는 것 |
|---|---|---|---|---|---|---|---|
| [Main](reference/Main.jpg) | 1440×810 | pc | ko | none | normal | auto | 둘러보기, 선택 없음 |
| [Portrait-Browse-Warrior-KO](reference/Portrait-Browse-Warrior-KO.jpg) | 440×956 | portrait | ko | none | normal | Warrior | 둘러보기, 선택 없음 · 카메라: 전사 구역 |
| [Portrait-Browse-Ranger-EN](reference/Portrait-Browse-Ranger-EN.jpg) | 440×956 | portrait | en | none | normal | Ranger | 둘러보기, 선택 없음 · 카메라: 궁수 구역 |
| [Portrait-Browse-Mage-KO](reference/Portrait-Browse-Mage-KO.jpg) | 440×956 | portrait | ko | none | normal | Mage | 둘러보기, 선택 없음 · 카메라: 마법사 구역 |
| [Portrait-Played-KO](reference/Portrait-Played-KO.jpg) | 440×956 | portrait | ko | warrior_m | normal | auto | 만든 캐릭터 선택: 레벨·사람 아이콘·플레이 · 전사 남 |
| [Portrait-New-KO](reference/Portrait-New-KO.jpg) | 440×956 | portrait | ko | archer_m | normal | auto | 아직 안 만든 캐릭터 선택: 머리 위 + · 궁수 남 |
| [Portrait-Cheer-KO](reference/Portrait-Cheer-KO.jpg) | 440×956 | portrait | ko | archer_m | cheer | auto | 생성: 화이팅 자세와 대사 · 궁수 남 |
| [Portrait-Created-EN](reference/Portrait-Created-EN.jpg) | 440×956 | portrait | en | archer_m | created | auto | 생성 직후 Lv.1 · 궁수 남 |
| [Portrait-Gear-KO](reference/Portrait-Gear-KO.jpg) | 440×956 | portrait | ko | warrior_m | gear | auto | 착용 장비 팝업 · 전사 남 |
| [Portrait-Gear-Mage-EN](reference/Portrait-Gear-Mage-EN.jpg) | 440×956 | portrait | en | mage_f | gear | auto | 착용 장비 팝업 · 마법사 여 |
| [Portrait-Rift-KO](reference/Portrait-Rift-KO.jpg) | 440×956 | portrait | ko | none | rift | auto | 진행 중인 균열: 균열 캐릭터(마법사 여)만 선택, 나머지 잠금 |
| [Portrait-Entering-EN](reference/Portrait-Entering-EN.jpg) | 440×956 | portrait | en | warrior_m | entering | auto | 플레이를 누름: 전투 준비 자세와 대사, 입장 중 · 전사 남 |
| [PlazaSelect](reference/PlazaSelect.jpg) | 956×440 | landscape | ko | none | normal | auto | 둘러보기, 선택 없음 |
| [Landscape-Browse-EN](reference/Landscape-Browse-EN.jpg) | 956×440 | landscape | en | none | normal | auto | 둘러보기, 선택 없음 |
| [Landscape-Played-KO](reference/Landscape-Played-KO.jpg) | 956×440 | landscape | ko | warrior_m | normal | auto | 만든 캐릭터 선택: 레벨·사람 아이콘·플레이 · 전사 남 |
| [Landscape-Played-Mage-EN](reference/Landscape-Played-Mage-EN.jpg) | 956×440 | landscape | en | mage_f | normal | auto | 만든 캐릭터 선택: 레벨·사람 아이콘·플레이 · 마법사 여 |
| [Landscape-New-KO](reference/Landscape-New-KO.jpg) | 956×440 | landscape | ko | mage_m | normal | auto | 아직 안 만든 캐릭터 선택: 머리 위 + · 마법사 남 |
| [Landscape-New-Warrior-EN](reference/Landscape-New-Warrior-EN.jpg) | 956×440 | landscape | en | warrior_f | normal | auto | 아직 안 만든 캐릭터 선택: 머리 위 + · 전사 여 |
| [Landscape-Cheer-KO](reference/Landscape-Cheer-KO.jpg) | 956×440 | landscape | ko | archer_m | cheer | auto | 생성: 화이팅 자세와 대사 · 궁수 남 |
| [Landscape-Created-EN](reference/Landscape-Created-EN.jpg) | 956×440 | landscape | en | archer_m | created | auto | 생성 직후 Lv.1 · 궁수 남 |
| [Landscape-Gear-KO](reference/Landscape-Gear-KO.jpg) | 956×440 | landscape | ko | warrior_m | gear | auto | 착용 장비 팝업 · 전사 남 |
| [Landscape-Gear-Ranger-EN](reference/Landscape-Gear-Ranger-EN.jpg) | 956×440 | landscape | en | archer_f | gear | auto | 착용 장비 팝업 · 궁수 여 |
| [Landscape-Rift-KO](reference/Landscape-Rift-KO.jpg) | 956×440 | landscape | ko | none | rift | auto | 진행 중인 균열: 균열 캐릭터(마법사 여)만 선택, 나머지 잠금 |
| [Landscape-Entering-EN](reference/Landscape-Entering-EN.jpg) | 956×440 | landscape | en | mage_f | entering | auto | 플레이를 누름: 전투 준비 자세와 대사, 입장 중 · 마법사 여 |
| [PC-Played-EN](reference/PC-Played-EN.jpg) | 1440×810 | pc | en | warrior_m | normal | auto | 만든 캐릭터 선택: 레벨·사람 아이콘·플레이 · 전사 남 |
| [PC-New-KO](reference/PC-New-KO.jpg) | 1440×810 | pc | ko | mage_m | normal | auto | 아직 안 만든 캐릭터 선택: 머리 위 + · 마법사 남 |
| [PC-Gear-KO](reference/PC-Gear-KO.jpg) | 1440×810 | pc | ko | archer_f | gear | auto | 착용 장비 팝업 · 궁수 여 |
| [PC-Rift-EN](reference/PC-Rift-EN.jpg) | 1440×810 | pc | en | none | rift | auto | 진행 중인 균열: 균열 캐릭터(마법사 여)만 선택, 나머지 잠금 |

## 다시 만들기와 검사

```bash
node Docs/Design/CharacterPlaza/tools/smoke.js            # 상태 흐름·UI 배치(겹침·화면 밖)·인장 카메라·말풍선 검사
node Docs/Design/CharacterPlaza/tools/render_reference.js /tmp/plaza-ref [아트보드 이름 ...]   # 로컬 Chrome으로 참조 렌더(PNG)
python3 Docs/Design/CharacterPlaza/tools/to_jpg.py /tmp/plaza-ref Docs/Design/CharacterPlaza/reference
```

게임 쪽 이미지 층(PNG)은 `Docs/Art/CasualConcept/2026-10-13/CharacterPlaza/tools/prep_scene.py`로 원본 장면에서 다시 뽑는다(방법은 그 폴더의 README).

## 한계

- 이 시안은 로컬 Chrome 정적 렌더와 상태 검사로만 확인했다. 캔버스 안의 실제 동작과 실기기 화면은 확인하지 않았다.
- 영웅 층은 장면에서 분리한 개발용 가공이다(네이티브 알파 아님). 게임 에셋 규칙과 달라 교체 가능한 개발용 아트로 등록하고, 출시 전에 3D 영웅과 애니메이션 또는 네이티브 알파 그림으로 바꾼다.
- 세로 마법사 구역은 1.2배 확대라 조금 흐리다. 장비 아이콘은 예전 어두운 화풍이다.
- 장면 속 NPC(과일 장수·마법 상점 주인·행인)는 선택 대상이 아니다.
