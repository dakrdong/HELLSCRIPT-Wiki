# HELLSCRIPT 훈련장 개편 HTML 시안

갱신일: 2026-09-30 · 최초 작성 2026-09-28 · [English](#english)

[시안 열기](HELLSCRIPT-TrainingGround.html) · [브라우저 검증 기록](evidence/browser-validation.json) · [이미지 출처](../../Docs/Art/TrainingGround/art-manifest.json) · [원본 파일 해시](source-manifest.json)

같은 세팅에서 사냥 칙령만 바꿔 DPS와 클리어 시간이 어떻게 달라지는지 비교하는 훈련장의 조작 가능한 HTML 시안이다. 훈련장 세팅(로비), 전투 중 화면, 결과 화면(성공·실패)과 두 팝업(적 추가 목록, 스킬별 사냥 칙령 수정)을 가로·세로 비율로 만들었다. Unity 계정·저장·전투에는 연결하지 않는다. 적 HP와 공격력은 게임의 균열 공식을 따르고, 캐릭터·기록·영웅의 DPS와 생존·시간은 시연 데이터다.

## 화면 구성

**훈련장 세팅(로비)**

- 현재 장착한 일반 액티브 4개와 궁극기 1개를 보여 준다. 스킬마다 체크박스가 있으며, 체크한 스킬만 전투에서 사용한다. 각 행에는 현재 칙령 요약(간편 설정 이름 또는 직접 설정의 사용 방식)과 연필 아이콘 버튼(칙령 수정)이 있다. 같은 글씨가 줄마다 반복되지 않도록 글자 없이 테두리 있는 아이콘 버튼만 둔다.
- **사냥 칙령 설정** 버튼은 사용 스킬 열 아래에 넓게 고정한다(세로 화면에서는 제목 바로 아래). 게임에서 사냥 칙령 창의 스킬 탭을 열며, 스킬 교체·투자와 공통 공격 설정은 그 창에서 바꾼다. 가로 화면에서는 스킬 5개가 스크롤 없이 한 화면에 들어온다.
- **균열 단계**는 한 줄에서 −/+ 버튼과 슬라이더로 고르고 오른쪽에 단계를 보여 준다. 몬스터는 실제 균열의 같은 단계와 같은 HP·공격력을 가지며, 행마다 마리당 HP와 공격력을 보여 준다. 열린 단계(최고 돌파 단계 + 1)까지만 고를 수 있다.
- **적 추가**는 보스 5종과 일반 몬스터 20종의 목록 상자를 연다. 전체·보스·일반 몬스터 탭이 있고, 행마다 초상화, 역할, 처음 등장하는 지역을 표시한다. 보스와 일반 몬스터를 합쳐 5종까지 넣는다.
- 적 행은 낮게 만든다. 초상화 오른쪽 위 모서리에 테두리 없는 ×가 있고, 이름·역할·능력치가 두 줄에 들어가며, 일반 몬스터는 오른쪽에 **등급 목록 상자**(일반·매직·희귀·전설)와 **마릿수 목록 상자**(1~20)가 나란히 놓인다. 목록 상자의 등급 색은 게임의 몬스터 등급 효과 색(`WorldFx.TierColor`)과 같다.
- 보스는 한 종류만 1마리 넣는다. 보스 행에는 등급·마릿수 대신 `1마리 고정`이 표시된다. 보스가 이미 있으면 다른 보스 행에 **교체**가 표시되고, 누르면 같은 자리에서 바뀐다.
- 이 세팅의 최고 기록과 직전 기록을 보여 준다. 하단에는 균열 단계·적 종류·마릿수·총 HP 요약과 **전투 시작**을 고정한다.

**전투 중**

- 최근 3초 평균으로 계산한 실시간 DPS와, 직전 판의 같은 시점과 비교한 증감률을 보여 준다.
- 이번 판(실선)과 직전 판(점선)의 초당 피해 그래프를 같은 눈금에 그린다. 평균·최고 DPS와 총 피해를 함께 표시한다. 재사용 대기시간이 3초 이상인 스킬(궁극기는 항상)을 쓴 시점마다 선 위에 그 스킬 아이콘을 얹는다. 쿨타임이 없어 계속 쓰는 스킬(회오리·분쇄 일격)은 그래프를 가리므로 아이콘을 얹지 않는다.
- **그래프 접기** 버튼(화살표)으로 DPS 패널을 실시간 DPS 한 줄로 접을 수 있다. 캐릭터가 싸우는 배경을 보고 싶을 때 쓰며, 다시 누르면 펼쳐진다.
- 남은 적, 적 종류별 남은 마릿수, 보스 HP, 영웅 HP를 보여 준다. 체크를 끈 스킬과 자동 사용이 꺼진 스킬은 스킬 칸에 따로 표시한다.
- **일시정지**(또는 Esc)를 누르면 전투 시간이 멈추고 **계속하기**와 **훈련 중단**이 열린다. 멈춘 시간은 클리어 시간에 넣지 않으며, 중단한 판은 기록하지 않는다.
- 시안은 전투를 배속으로 재생하고 화면에 `시연 N배속`을 표시한다.

**결과**

- 성공하면 클리어 시간과 직전 판 대비 증감(초와 %)을 가장 크게 보여 준다. 첫 기록, 빨라짐, 느려짐, 같은 기록을 서로 다른 색과 기호로 구분하며 최고 기록 갱신도 표시한다.
- 영웅이 쓰러지면 **훈련 실패**로 끝난다. 클리어 시간은 `기록 없음`으로 표시하고, 쓰러진 시점과 남은 적을 알려 준다. 실패한 판은 기록하지 않으며 비교하지 않는다.
- 평균 DPS와 직전 대비 증감률, 최고 DPS, 총 피해, 두 판의 DPS 그래프를 보여 준다. 그래프에 포인터를 올리거나 터치하거나 방향키를 누르면 그 시점의 두 값과 함께 **그 초에 쓴 스킬 이름**을 읽을 수 있다(아이콘이 없는 회오리·분쇄 일격도 이름은 나온다).
- 스킬별 피해는 균열 결과의 스킬 카드와 같은 양식이다. 일반 스킬 4개는 2×2로, 궁극기는 전체 너비로 놓고, 카드마다 큰 아이콘, 스킬 이름, 사용 횟수, 일반 몬스터·보스 각각에 준 피해 비중 게이지와 연필 아이콘 버튼(칙령 수정)을 둔다. 기본 공격 행은 두지 않는다. 결과 화면에서 저장한 칙령은 다시 시작할 때 적용된다고 안내한다.
- 직전 판 이후 바뀐 사냥 칙령을 `회오리 · 간편 설정: 생존 우선 전투 → 빠른 중앙 침투`처럼 나열한다.
- 하단에는 **훈련장 세팅 변경**(로비로 돌아가기)과 **다시 시작하기**(지금 세팅 그대로)를 고정한다.

**칙령 수정 팝업**은 게임의 스킬별 칙령 편집과 같은 순서를 사용한다. 간편 설정 3개와 마지막의 직접 설정을 고르고, 직접 설정에서는 자동 사용, 사용 방식(이득·손해), 세부 옵션을 바꾼다. 저장하지 않은 변경 수를 표시한다. 저장하면 현재 사냥 칙령에 반영되어 훈련과 모든 사냥에 적용된다. 세로 화면에서는 두 팝업을 아래에서 올라오는 시트로 띄운다.

## 확정한 규칙

2026-09-28 사용자 확인 결과를 반영했다.

| 항목 | 규칙 |
| --- | --- |
| 같은 세팅의 기준 | 균열 단계, 적 종류·등급·마릿수와 체크한 스킬이 같으면 같은 세팅이다. 사냥 칙령은 시험하는 변수이므로 기준에 넣지 않는다. 게임에서는 영웅 레벨과 장비도 같아야 비교한다. |
| 배치와 시드 | 같은 세팅은 같은 배치와 시드로 시작한다. 같은 칙령으로 다시 하면 같은 결과가 나오고, 차이는 칙령에서만 생긴다. |
| 비교 대상 | 같은 세팅에서 직전에 성공한 판과 비교하고, 최고 기록을 함께 표시한다. |
| 보스 | 한 종류만 1마리다. 다른 보스를 고르면 교체한다. |
| 균열 단계 | 몬스터 능력치는 모든 적 생성이 거치는 `CombatSimulation.EnemyStats`의 공식을 그대로 쓴다. HP는 70 × 1.08^(단계−1) × 종류 계수, 공격력은 18 × 1.055^(단계−1) × 종류 계수다. 보스는 HP ×45·공격력 ×3에 보스별 보정을 곱하고, 1~5단계에는 입문 균열 보정을 적용한다. 빌드가 게임 코드에서 상수를 읽으며, 공식이 바뀌면 빌드가 실패한다. 시안은 라이브 운영 배율을 1로 보고, 게임의 훈련장은 균열과 같은 배율을 쓴다. |
| 실제 전투와 같음 | 훈련장의 모든 수치와 규칙은 실제 전투와 같다. 시안의 적 능력치와 정예 특성은 게임 코드에서 그대로 읽는다. 영웅의 DPS·생존·이동과 칙령 효과는 HTML이 계산할 수 없어 시연 값으로 두며, 게임 구현에서는 실제 전투 시뮬레이션이 모두 계산한다. |
| 몬스터 등급 | 일반 몬스터에만 적용한다. 희귀는 실제 균열의 정예와 같다: HP ×3, 공격력 ×1.5, 정예 특성 1개이며 20단계부터 50% 확률로 2개다. 특성은 `RiftGenerator`의 규칙을 따른다(추적 화염·얼음 고리·사격 방벽·분노 축적 중 하나, 4마리 이상이면 시체 폭발 추가, 추적 화염과 얼음 고리는 함께 붙지 않음). 정예 두 마리 행은 25% 확률로 생명 연결 쌍이 된다. 시안은 게임의 난수와 같은 방식(`TrainingGround.EliteTraits`)으로 뽑으므로 로비에 보이는 특성이 실제 훈련과 같다. 실제 전투의 매직·전설은 등급 효과만 있는 표시 전용(`MonsterTier`)이므로, 훈련장에서도 능력치는 일반과 같고 효과만 다르다. |
| 같은 적의 다른 등급 | 한 종류는 한 행만 둔다. |
| 체크와 자동 사용 | 체크박스는 훈련에만 적용된다. 칙령의 자동 사용이 꺼져 있으면 체크한 스킬도 쓰지 않으며, 행에 경고를 표시한다. |
| 저장 범위 | 팝업의 저장은 현재 사냥 칙령에 반영되어 훈련·균열·반복 사냥 모두에 적용된다. 기존 훈련의 복사본 격리 정책을 이 기능에서는 적용하지 않는다. |
| 종료와 기록 | 모든 적을 처치하면 성공이며 그 시간만 기록한다. 영웅이 쓰러지면 실패이며 기록하지 않는다. 일시정지한 시간은 넣지 않는다. |
| 기존 훈련 | 고정 훈련 3종과 A/B 비교 화면을 이 기능으로 대체한다. |

참고: 매직·전설을 실제로 더 강하게 만들려면 균열 전투에 등급 규칙을 먼저 추가해야 하며, 그러면 훈련장도 같은 규칙을 따른다.

## 데이터 출처

- 스킬 이름·설명, 스킬별 사용 방식과 이득·손해, 간편 설정, 세부 옵션은 빌드 때 `Assets/HELLSCRIPT/Resources/Data/ClassSkills.json`, `Assets/HELLSCRIPT/Resources/HuntEdictQuickPresets.json`, `Assets/HELLSCRIPT/Runtime/Core/HuntEdictV2.Options.cs`에서 읽는다. 영어는 `Assets/HELLSCRIPT/Resources/Localization/en.txt`를 사용하며, 번역이 없으면 빌드가 실패한다.
- 적 이름, 역할, 처음 등장하는 지역은 `GameCatalog.cs`와 `EnemyCombat.cs`에서 읽는다. 균열 단계 공식은 `CombatSimulation.cs`, `IntroductoryRift.cs`, `BossCombat.cs`에서, 정예 특성 규칙은 `RiftGenerator.cs`, `TrainingGround.cs`, `ContentUnlocks.json`에서 읽는다. 색은 `UiTheme.cs`, 몬스터 등급 색은 `WorldFx.cs`에서 가져온다.
- 스킬 아이콘과 스킬 테두리는 기존 게임 리소스를 축소해 넣었다. 시연 캐릭터는 균열 결과 시안과 같은 전사 아르덴(최고 돌파 24단계, 회오리·도약 내려찍기·분쇄 일격·철벽·선조의 전쟁)이다.
- 영웅의 DPS(420)·HP(1,800)·방어·회복과 칙령별 효과는 `engine.js`의 시연 값이다. 칙령 변경이 결과에 반영되는 모습을 보여 주기 위한 값이며 밸런스 데이터가 아니다.

## 이미지

사용자의 요청에 따라 Codex에 훈련장 배경 1장, 보스 5장, 일반 몬스터 20장을 요청했다. `codex exec`의 내장 `image_gen`으로 에셋마다 한 번씩 생성했고, 모델은 보고되지 않아 `candidate_model_unknown`으로 기록했다. 몬스터 외형은 3D 개편 기획의 외형 설명을 프롬프트에 사용했다. 원본 PNG는 게임 리소스 `Assets/HELLSCRIPT/Resources/Art/TrainingGround/`에 바이트 그대로 두고, HTML에는 macOS `sips`로 축소한 사본만 넣는다. 부푼 순례자(N06)는 첫 요청이 이미지 안전 필터에 막혀, 상처 묘사를 뺀 설명으로 다시 받았다. 프롬프트, 원본 경로, 해시, 차단 기록은 [이미지 출처](../../Docs/Art/TrainingGround/art-manifest.json)에 있다. 게임 구현을 위해 `TrainingGroundArtImporter`와 `ResourceTextureBudget`에 크기 상한(초상화 256, 배경 Android 1024·PC 2048)을 추가했다.

## 실행과 검증

```sh
node Prototypes/TrainingGround/build.cjs
node --test Prototypes/TrainingGround/engine.test.cjs
node Prototypes/TrainingGround/browser.test.cjs
python3 tools/check_ui_contract.py
python3 tools/test_ui_contract.py
```

`HELLSCRIPT-TrainingGround.html` 파일 하나로 실행하며 크기는 약 1.1 MiB다. 외부 폰트·라이브러리·이미지 서버가 필요 없다. 상단 도구에서 화면 크기(세로 440×956, 가로 956×440, PC 16:9·16:10·21:9), 화면 상태(세팅, 전투 중, 결과의 빨라짐·느려짐·첫 기록·실패), 안전 영역, 글자 크기 120%, 한국어·영어를 바꾼다. 캡처용으로 `?embed=1&device=portrait&scene=failed&lang=en&modal=edict:W03` 같은 주소 옵션도 받는다.

2026-09-28 결과(2026-09-30에 시안을 고친 뒤 다시 돌린 결과는 아래 항목에 덧붙였다):

- 규칙 검사 **7개 통과**: 5종·보스 한 종·일반 1~20마리 제한과 보스 교체, 균열 단계를 포함한 세팅 기준, 게임 공식과 일치하는 단계별 HP·공격력(1~5단계 입문 보정, 희귀 = 정예 배율, 매직·전설 = 일반 포함), 정예 특성 규칙과 게임과 같은 특성 추첨(게임 테스트와 같은 고정 사례 7개), 간편 설정 왕복, 같은 칙령의 같은 결과와 칙령에 따른 빨라짐, 영웅이 쓰러지는 실패와 철벽의 생존 효과.
- 헤드리스 Chrome에서 DevTools 프로토콜로 실제 마우스·키 입력을 넣어 **183개 검사 통과**(2026-09-28에는 191개; 등급·마릿수 버튼 검사가 목록 상자 검사로, 그래프 접기·사용 스킬 이름 툴팁 검사가 더해졌다). 스킬 체크, 적 추가·보스 교체·등급·마릿수·삭제, 균열 단계 버튼·슬라이더 키보드 조작, 칙령 수정·취소·저장, 전투의 일시정지·계속·Esc, 결과 비교, 결과에서 칙령 수정 후 다시 시작, 최고 단계에서 철벽을 끈 실패와 미기록을 이어서 확인했다. 다섯 화면 크기 × 두 언어 × 두 글자 크기 × 로비·성공 결과·실패 결과의 60개 조합에서 가로 넘침이 없고 하단 행동 버튼이 화면 안에 있으며, 영어 화면에 한국어가 남지 않았다. 콘솔 오류·경고는 없었다.
- 공통 UI 소유 검사가 통과했고, 공통 UI 검사 9개가 통과했다. HTML 시안의 결과이므로 Unity 화면이 공통 부품에 연결되었다는 뜻은 아니다.
- 내장 브라우저는 빌드로 생성한 로컬 파일을 열지 못해 Chrome 헤드리스로 확인했다. Unity 실행, 실제 계정·저장, 모바일 실기기와 터치 입력, 성능은 확인하지 않았다.

캡처: [가로 로비](evidence/lobby-landscape-ko.jpg) · [세로 로비](evidence/lobby-portrait-ko.jpg) · [가로 결과](evidence/result-landscape-ko.jpg) · [세로 결과](evidence/result-portrait-ko.jpg) · [가로 실패](evidence/result-failed-landscape-ko.jpg) · [세로 실패](evidence/result-failed-portrait-ko.jpg) · [가로 전투](evidence/battle-landscape-ko.jpg) · [세로 전투](evidence/battle-portrait-ko.jpg) · [일시정지](evidence/flow-paused-landscape-ko.jpg) · [적 추가 가로](evidence/picker-landscape-ko.jpg) · [적 추가 세로](evidence/picker-portrait-ko.jpg) · [칙령 직접 설정 세로](evidence/edict-custom-portrait-ko.jpg) · [칙령 간편 설정 가로](evidence/edict-preset-landscape-ko.jpg) · [저장 전 변경](evidence/flow-edict-unsaved-landscape-ko.jpg) · [실제 흐름의 전투](evidence/flow-battle-landscape-ko.jpg) · [영어 느려짐 결과](evidence/result-slower-landscape-en.jpg) · [영어 120% 세로](evidence/lobby-portrait-en-large.jpg) · [PC 로비](evidence/lobby-pc-ko.jpg) · [PC 영어 결과](evidence/result-pc-en.jpg)

![가로 로비](evidence/lobby-landscape-ko.jpg)

![가로 결과](evidence/result-landscape-ko.jpg)

## English

An interactive HTML mockup of a reworked Training Ground: keep the setup fixed, change only the Hunt Edict, and see how DPS and clear time move. It covers the setup lobby, the in-battle HUD, the success and failure results, and two dialogs (enemy list box and per-skill edict editor) in portrait and landscape. It has no Unity account, save or combat connection. Enemy HP and attack follow the game's rift formula; hero, records, hero DPS, survival and times are demo data.

**Lobby.** Shows the four equipped actives and the ultimate. Each has a checkbox (only checked skills fight), a summary of its current edict and a bordered pencil icon button (edit edict) with no repeated words. The wide **Hunt Edict settings** button is docked under the skill column (right under the heading in portrait) and opens the edict window's Skills tab in the game; on landscape screens all five skills fit without scrolling. The **Rift tier** sits on one line with −/+ buttons, a slider and the tier value, and sets monster HP and attack to those of a real rift at that tier, shown per monster; only open tiers (highest clear + 1) are allowed. **Add enemy** opens a list box of 5 bosses and 20 normal monsters with All/Bosses/Normal tabs, portraits, roles and first field. Up to 5 kinds. Enemy rows are low: a borderless × on the portrait's top-right corner, name, role and stats on two lines, and for a normal monster a **tier drop-down** (Normal/Magic/Rare/Legendary, coloured like the in-game tier effects) beside a **count drop-down** (1–20). A boss row says `Always 1` instead; only one boss kind is allowed and another boss offers **Swap**. Best and previous clear for the current setup are shown with a fixed **Start training** bar.

**Battle.** Live DPS over the last 3 seconds and its change against the previous run at the same second, a same-scale chart of this run (solid) and the previous run (dashed) with the skill's icon on the line at each cast of a skill on a cooldown of 3 seconds or more (ultimates always; cooldown-free rotation skills such as Whirlwind would bury the line, so they get no icon), average/peak DPS, total damage, enemies left, per-kind counts, boss HP and hero HP. An arrow button folds the DPS panel to one live-DPS line so the fight behind it can be watched; pressing it again unfolds it. **Pause** (or Esc) stops combat time and offers **Resume** and **Stop training**; paused time never counts, and a stopped run is not recorded. The mockup replays the fight at a labelled demo speed.

**Result.** A clear shows its time and difference from the previous run in seconds and percent (first clear, faster, slower and same are distinct), a new-best badge, average DPS change, the two-run DPS chart with a crosshair tooltip (pointer, touch or arrow keys) that also names every skill cast in that second (Whirlwind and Crushing Strike too, although they carry no icon), damage by skill in the same card as the rift result (four skills in two columns, the ultimate across; big icon, name, casts, and gauges for the share of damage done to normal monsters and to bosses, plus a pencil icon button that edits the edict; no basic-attack row), and the edict changes since the previous run. If the hero falls, the run ends as **Training failed** with no clear time, the moment of death and enemies left; it is neither recorded nor compared. **Change training setup** returns to the lobby and **Start again** reruns the same setup. Edicts saved on this screen apply from the next run.

**Edict editor** follows the in-game per-skill editor: three quick presets and Custom last; Custom shows automatic use, the use policy with its gain and trade-off, and the detailed options. Saving updates the current Hunt Edict used by training and every hunt. On portrait both dialogs open as bottom sheets.

**Rules confirmed on 2026-09-28.** A setup is the rift tier, enemies, tiers, counts and checked skills; the edict is the variable under test (the game should also require the same hero level and equipment). The same setup starts with the same placement and seed. Comparison is against the previous successful run of the same setup, with the best shown too. One boss kind only. Monster stats use `CombatSimulation.EnemyStats`, which every spawn goes through, exactly: HP 70 × 1.08^(tier−1) × kind factor, attack 18 × 1.055^(tier−1) × kind factor, bosses ×45 HP / ×3 attack with their profile, and the introductory reduction on tiers 1–5; the build reads the constants from the game code and fails if the formula changes; the mockup takes live-ops multipliers as 1, while the game's training ground uses the rift's own. Tiers apply to normal monsters. Rare equals a real rift elite: HP ×3, attack ×1.5 and one elite trait by `RiftGenerator`'s rule (homing flame, ice ring, volley barrier or rage buildup; corpse blast joins in packs of 4+; flame and ice never together), with a 50% chance of a second trait from tier 20, and a row of two elites becomes a life-linked pair a quarter of the time; the build reads these from the game code, and the mockup draws them with the game's own random stream (`TrainingGround.EliteTraits`), so the lobby shows exactly the traits the training spawns. Every number matches real combat: Magic and Legendary are presentation-only in real combat (`MonsterTier`), so they keep normal stats and differ only in their effect. The hero's DPS, survival and movement and the edict effects are demo values because HTML cannot compute them; the native build computes everything with the real combat simulation. One row per enemy kind. The checkbox is training-only; an edict with automatic use off still does not cast. Saving updates the live Hunt Edict for every hunt. Only successful clears are timed and recorded; a fallen hero is a failure; paused time is excluded. This replaces the three fixed trainings and the A/B screen. Making Magic or Legendary stronger would first need a tier rule in real rift combat, which the training ground would then follow.

**Sources.** Skill names, use policies, quick presets and legacy options are read at build time from `ClassSkills.json`, `HuntEdictQuickPresets.json` and `HuntEdictV2.Options.cs`, English from `Localization/en.txt` (a missing translation fails the build). Enemy names, roles and fields come from `GameCatalog.cs` and `EnemyCombat.cs`; the rift formula from `CombatSimulation.cs`, `IntroductoryRift.cs` and `BossCombat.cs`, and elite traits from `RiftGenerator.cs`, `TrainingGround.cs` and `ContentUnlocks.json`; colours from `UiTheme.cs` and `WorldFx.cs`. Skill icons and frames are downscaled game resources. Hero DPS (420), HP (1,800), armour, sustain and edict effects in `engine.js` are demo values, not balance data.

**Art.** At the user's request, Codex generated the training-yard key art, 5 boss portraits and 20 monster portraits with its built-in `image_gen` (one request per asset; model not reported, recorded as `candidate_model_unknown`). Monster looks follow the 3D overhaul brief. Originals live byte for byte in the game resources `Assets/HELLSCRIPT/Resources/Art/TrainingGround/`; the HTML embeds `sips`-downscaled copies. The first Bloated Pilgrim (N06) request was blocked by the image safety filter and was regenerated from a non-graphic description. Prompts, sources, hashes and the blocked attempt are in [the art manifest](../../Docs/Art/TrainingGround/art-manifest.json). `TrainingGroundArtImporter` and `ResourceTextureBudget` cap them for the game (portraits 256, backdrop 1024 on Android and 2048 on PC).

**Verification (2026-09-28, browser run repeated 2026-09-30).** 7 rule tests passed, including tier stats against the game formula, Rare as a real elite with its trait rule and the game's own trait draws (seven cases pinned in both the mockup and the Unity tests), Magic/Legendary at normal stats, one boss kind with swapping, and failure when the hero falls. 183 browser checks passed in headless Chrome (191 on 2026-09-28; the tier and count button checks became drop-down checks and checks for folding the live graph and the skill-naming tooltip were added) driven over the DevTools protocol with real mouse and key input: the whole flow (skill checkbox, enemy add/swap/tier/count/remove, rift tier buttons and keyboard slider, edict edit/cancel/save, pause/resume/Esc, comparison, edit from the result and restart, a failed top-tier run without Iron Wall that is not recorded) and 60 layouts (5 screen sizes × 2 languages × 2 text sizes × lobby/success/failure) with no sideways overflow, bottom actions on screen, no Korean left in English, and no console errors or warnings. The shared-UI ownership check passed and its 9 regression tests passed; for an HTML mockup this does not mean a Unity window is wired to the shared owners. The in-app browser could not open generated local files, so Chrome headless was used. Unity runtime, real saves, physical mobile devices, touch input and performance were not verified.
