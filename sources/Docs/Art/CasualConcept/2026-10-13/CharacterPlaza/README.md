# 캐릭터 생성·선택 화면 마을 장면 — 아트 원본

작성일: 2026-10-14 · [English](README.en.md)

화면 시안과 규칙은 [Docs/Design/CharacterPlaza](../../../../Design/CharacterPlaza/README.md)에 있다. 이 폴더는 Codex가 만든 원본 그림, 프롬프트, manifest와 영웅을 장면에서 분리하는 도구다. 컨셉 기록은 [Casual_Art_Concept_20261013.md](../Casual_Art_Concept_20261013.md)의 "캐릭터 생성·선택 화면: 마을 장면" 절이다.

## 파일

| 파일 | 내용 |
|---|---|
| `scene_landscape.png` · `scene_portrait.png` | 영웅 여섯이 들어간 마을 장면(1536×1024, 1024×1536). 각자 장소에서 활동: 술집 테라스(전사 남녀), 과일 가게(궁수 남녀), 마법 상점(마법사 남녀) |
| `clean_landscape.png` · `clean_portrait.png` | 영웅만 지운 같은 장면(화이팅·전투 준비 연출에서 그림 속 영웅을 가리는 용도) |
| `cheer_<영웅>.png` ×6 | 화이팅 자세(무기 없음, 투명 배경, 1024×1536) |
| `fight_<영웅>.png` ×6 | 전투 준비 자세(착용 무기: 전사 검, 궁수 활, 마법사 수정 지팡이, 투명 배경, 1024×1536). 남궁수는 활을 겨누는 자세가 Codex 안전 필터에 막혀 활을 들고 엄지를 세운 자세로 그렸다 |
| `crests.png` | 직업 인장 세 개(전사·궁수·마법사)를 한 장에 그린 것. 시안은 이 그림을 세 개로 잘라 쓴다 |
| `PROMPT_*.md` · `manifest_*.json` | 요청 프롬프트와 원본 경로·SHA-256·크기 기록. 도구가 모델명을 알려주지 않아 model은 `unknown` |
| `tools/` | `lift.swift`(macOS Vision 피사체 분리), `prep_scene.py`(층 만들기), `spec_landscape.json` · `spec_portrait.json`(영웅별 자를 범위·기준점) |

`PROMPT_cheer_*.md`에는 첫 시도(빈 광장에 소품과 함께 선 그림)에서 같은 실행으로 요청한 소품 자세도 적혀 있지만, 그 그림은 버렸다. `PROMPT_fight_rangers.md`는 남궁수 요청이 막혀 여궁수만 나온 실행이고, 남궁수는 `PROMPT_fight_archer_m.md`로 다시 요청했다.

## 층 다시 만들기

```bash
swiftc -O tools/lift.swift -o tools/lift
python3 tools/prep_scene.py tools/spec_landscape.json --out /tmp/plaza --format png
python3 tools/prep_scene.py tools/spec_portrait.json  --out /tmp/plaza --format png
```

각 그림마다 `<pic>_bg`(장면), `<pic>_cut_<영웅>`(장면에서 분리한 영웅), `<pic>_patch_<영웅>`(영웅 자리의 배경판 조각), `scn_<pic>.json`(좌표)이 나온다. 이미 올린 캔버스 자료와 같은 박스·기준점을 만든다는 것을 확인했다(영웅 12명 모두 일치).

- 화면 자료: `scn_*.json`의 `cut`은 분리한 영웅의 장면 속 위치, `head`·`feet`는 이름표·말풍선·자세 그림을 세울 기준점, `cheer`는 자세 그림을 세울 박스(발 x, 발 y, 높이), `patch`는 가림 조각 위치다. 세로 카메라 틀은 `focus`(직업별 x, y, 확대, 화면 x).
- 자세 그림(`cheer_*`·`fight_*`)은 투명 여백이 있는 원본이다. 알파 경계로 잘라 발 중앙을 `feet`에 맞추고 높이를 `cheer[2]`에 맞춰 세운다.

## 가공과 규칙

- 영웅 층은 macOS Vision 피사체 분리로 장면에서 오려 낸 것이라 **네이티브 알파가 아니다**. 저장소의 아트 규칙(크로마키·배경 제거 없이 원래 알파)과 달라, 게임에 넣을 때는 교체 가능한 개발용 아트로 등록하고(`productionApproved: false`) 이 가공 과정을 출처 기록에 적는다. 출시 전에는 3D 영웅과 애니메이션 또는 네이티브 알파 그림으로 바꾼다.
- 장면과 자세 그림은 Codex가 생성한 원본 픽셀 그대로다(후처리 없음). 모델명은 알 수 없다(`unknown`)이며 C2PA에는 생성 도구가 기록되어 있다.
- 여전사 복장(배꼽 노출)은 12세 등급 기준 검토가 필요하다. 전사 남의 술잔은 사용자가 그대로 두기로 했다.
