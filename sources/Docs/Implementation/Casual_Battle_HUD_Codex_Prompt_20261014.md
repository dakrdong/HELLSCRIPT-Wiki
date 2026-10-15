# 전투 HUD·실시간 그래프 캐주얼 스킨 — Codex 작업 지시

> 보존한 구현 인계: 이 코드 조사는 당시 공개 main `a3f96545` 기준이다. 완료된 로컬 UI/HUD와 검증은 [통합 기록](Casual_UI_Integration_20261014.md)과 [HUD 인계](Casual_Battle_HUD_20261014.md)를 따른다. 구 조사 내용을 현재 미구현 상태로 해석하거나 생성을 재개하지 않는다.

작성일: 2026-10-14 · [English](Casual_Battle_HUD_Codex_Prompt_20261014.en.md)

아래 코드 블록 전체를 Codex에 그대로 붙여 넣는다. 시안 자료는 `claude/casual-battle-hud` 브랜치(원격에 올라가 있음)에 있다.

| 전달할 것 | 위치 |
|---|---|
| 디자인 캔버스(대지 12장, 비공개) | https://claude.ai/artifact/PK5gEQdR1rrbW8Jzx3nDaA. Codex가 열려면 사용자가 공유해야 한다. 열 수 없어도 아래 저장소 사본으로 충분하다 |
| 시안 정본(소스·참조 화면 14장·배치 숫자·문구 표·인장/물약 그림·검사 도구) | `Docs/Design/GlobalHUD/Casual/` ([README](../Design/GlobalHUD/Casual/README.md)) |
| 이 지시 | 이 문서 |

```text
HELLSCRIPT 전투 화면의 메인 HUD와 전투 그래프 창을 캐주얼 StyleD 시안대로 다시 만들어 줘. 시안 정본은 Docs/Design/GlobalHUD/Casual/ 이다(README, 참조 화면 14장, 시안 소스, 인장·물약 그림, 검사 도구). 전투·저장·그래프 계산은 그대로 두고, 모양·배치·그래프 창 분리를 바꾸는 작업이다.

## 배경
- 게임 아트가 어두운 고딕에서 밝은 캐주얼(StyleD, 12세 이상)로 바뀐다. 대표 화면부터 단계적으로 스킨을 바꾸는 것이 승인되어 있고, 이 작업은 전투 HUD 차례다.
- 다른 캐주얼 시안(스킬 트리, 훈련장, 전투 위치 거리 HUD 등)과 같은 양식을 쓴다: 잉크 외곽선 #2a3150, 크림 패널, 노랑·주황 강조, 하늘 자원, 코랄 위험, Jua 숫자·제목과 Noto Sans KR 본문, 굵은 아래 그림자.
- 사용자가 시안에서 직접 정한 것(README "시안 메모" 1번):
  - 사냥 칙령 단추는 세 방향 모두 오른쪽 위(메뉴·설정 아래)에 두고, 탭은 단추 아래로 펼친다.
  - 미니맵은 오른쪽 아래에 둔다.
  - 상태 효과는 첫 물약의 왼쪽 끝 위에서 시작한다. 세로는 물약·스킬 줄 위 왼쪽이다.
  - 세로 화면의 그래프는 작은 창으로 하고, 세로 HUD는 쌓아서 배치한다.
  - 실시간 DPS·몬스터 피해·보스 피해·활동 시간을 각각 따로 옮기고 접는다.
  - 접은 활동 시간 창은 제목 옆에 글자 없는 색 비율 막대를 보인다.

## 먼저 할 것
1. 규칙: AGENTS.md, CLAUDE.md, AgentRules/(Shared_UI, UI_Refresh, Workflow, CPU_Performance, Unity_MCP, Wiki_Publish, Artifact_Cleanup). 한 작업·한 에이전트로 한다. 대화·개발 기록은 한국어, 코드 주석·커밋 메시지·로그는 영어로 쓴다.
2. 최신 origin/main 에서 새 worktree 와 브랜치 codex/casual-battle-hud 를 만든다(다른 세션이 같은 체크아웃을 쓴다). 시안은 다음처럼 가져온다:
   git fetch origin claude/casual-battle-hud && git checkout origin/claude/casual-battle-hud -- Docs/Design/GlobalHUD/Casual Docs/Implementation/Casual_Battle_HUD_Codex_Prompt_20261014.md Docs/Implementation/Casual_Battle_HUD_Codex_Prompt_20261014.en.md
3. Docs/Design/GlobalHUD/Casual/README.md 를 끝까지 읽고 reference/ 의 14장을 열어 본다. 배치 숫자·부품 상태·그래프 규칙·문구는 그 문서와 canvas/Main.dc.html 의 CSS 가 정본이고, 아래는 요약이다. node Docs/Design/GlobalHUD/Casual/tools/smoke.js 는 시안 규칙 검사이며, 게임 시험으로 옮길 규칙의 원본이다.

## 확인한 현재 상태 (main a3f96545 기준, 직접 읽은 코드)
- 메인 HUD:
  - Presentation/GlobalHudView.cs 가 런타임에 HUD 를 만든다(미리 보기와 게임이 공유).
  - GlobalHudLayout.cs 의 GlobalHudStyle 이 숫자와 색을 가지며, Resources/Data/GlobalHudLayout.json 에서 읽는다. 좌표는 왼쪽 아래 기준이고 기준 화면은 1600×900 이다. 세로 화면도 가로 구성을 한 줄로 폭에 맞추므로, 휴대전화 세로에서 슬롯이 약 27pt 가 된다.
  - GlobalHudSnapshot.cs 는 슬롯 상태(준비, 재사용 대기, 자원 부족 ◇, 사용 중, 잠김, 빈칸)와 효과 목록을 만든다.
  - GameUI.GlobalHud.cs 는 슬롯·효과 설명과 관찰 메뉴를 맡는다.
- HUD 그림: Resources/Art/GlobalHUD/*.png (frame-seal, frame-vital, fill-vital, frame-active, frame-status, badge-*, status-*, mask-*, xp-*, control-*, menu-*, potion-hp/mp/utility).
  - 직접 만든 벡터 그림 63개는 Docs/Design/GlobalHUD/Resources/v1/build-vectors.cjs(sharp 사용)로 다시 만들 수 있다.
  - 인장 초상은 GlobalHudView.Portrait(heroClass)가 고른다. 마법사는 portrait-mage, 나머지는 SkillAtlas 18+직업이다.
  - 물약 병은 PotionArt.IconId → Art/GlobalHUD/potion-* 이다. 병 잘라내기 사각형이 PotionArt 에 숫자로 들어 있다. 제작 엘릭서는 Art/Jeweler/Potions 에 있다.
  - 스킬 칸은 SkillIconView(frame-active, frame-seal)가 그린다.
- 사냥 칙령 단추:
  - GlobalHudLayout.edict 가 인장 위 자리이고, GlobalHudView 의 "Hunt edict"(menu-hunt-edict)가 단추다.
  - 누르면 GameUI.EdictQuick.ToggleEdictQuick 가 quickRoot(정렬 107)를 연다. GameUI.EdictSheet.BuildEdictTabIcons 가 HuntEdictHudRows.Tabs(combat, survival, explore, repeat, presets)의 menu-edict-* 아이콘을 단추 오른쪽으로 단추의 80% 크기로 늘어놓는다. 탭을 누르면 OpenEdictSheet 가 열린다.
  - GameUI.EdictPosition.cs 도 같은 단추를 앵커로 쓴다.
- 미니맵: GameUI.Rift.ReflowRiftMinimap(riftMinimapPanel)이 그린다. 오른쪽 콘텐츠 독 열의 왼쪽, 위쪽(top 88/122/158)에 놓이고 MapDisplay.Corner 모드를 따른다. 배치는 GameUI.BattleLayout.ReflowBattleHud 에서 한다.
- 실시간 DPS 창:
  - GameUI.TrainingGround.BuildTrainingHud/PlaceTrainingHud 가 trainingHud 를 만든다. 기본 위치는 맵 왼쪽 top 124, 폭 min(320, 30%)이고, 접기가 있고, DpsHudDrag 와 game.DpsPosition 으로 위치를 기기에 저장한다.
  - 차트는 TrainingChartView + TrainingDpsChart 다. 색은 Current #b98230, Previous #4a87bd 이고, 결과 창 TrainingDpsPanel·RiftCombatGraphWindow 와 공유한다.
- 받은 피해 그래프: GameUI.DamageGraphs.cs 의 몬스터·보스 그래프는 trainingHud 의 자식이라 DPS 창 아래에 붙고 DPS 접기와 함께 접힌다. 50%에서 가득 차고, 20%·40%에서 색이 바뀐다. 데이터는 DamageGraph.Monsters/Boss, 보스 인장은 Art/Boss/seal-*, 공격 아이콘은 Art/Boss/skill-* 이다.
- 활동 시간:
  - GameUI.RiftActivity.cs 가 activityHud 를 만든다. 접기가 있고 DpsHudDrag 와 game.ActivityPosition 으로 위치를 저장한다.
  - RiftActivityPanel.cs 는 결과 창과 공유한다. 색은 UiTheme.Resource/Danger/PaidFatigueText/Success 이다.
  - 위치 저장 형식은 Core/DpsHudPosition.cs(directory, file)이며 지금 두 개(DPS, 활동)가 있다.
- 공통 소유자: 색·글꼴·크기는 UiTheme·UiFonts, 패널 칠은 StorageSurface 가 맡는다. main 에는 캐주얼 색 토큰과 Jua·Noto Sans KR 글꼴이 아직 없다. 2026-10-14 에 확인했을 때 다른 캐주얼 작업 브랜치에도 UiTheme·UiFonts 변경이 없었다.
- 관련 기록: Docs/Implementation/Global_HUD_Potions, Damage_Graphs_20261010, Edict_Hud_Sheet_20261011, Rift_Activity_Time(각 .md/.en.md), Docs/Design/GlobalHUD/*.
- 관련 시험: GlobalHudTests, GlobalHudResourceTests, BattleHudLayoutTests, DamageGraphTests, DpsHudTests, CombatActivityTests, HuntEdictHudRowsTests, PotionTests, PotionLoadoutTests.
- 관련 스모크: RuntimeGlobalHudSmoke, RuntimeBattleLayoutSmoke, RuntimeDamageGraphSmoke, RuntimeHudScaleSmoke, RuntimeTrainingGroundSmoke(.RiftDps, .Activity), RuntimeBattleQuickEditSmoke.

## 할 일 (요약, 숫자는 README)
1. 양식 소유자: 캐주얼 색(잉크·크림·노랑·주황·금색·하늘·코랄·민트·초록·빈 칸·보조 글자)을 기존 소유자(UiTheme, 또는 HUD 전용이면 GlobalHudStyle/GlobalHudLayout.json)에 이름 붙은 값으로 추가한다. HUD·그래프 코드는 그 값만 쓰고 색 숫자를 코드에 흩어 쓰지 않는다.
   - 공유 색(UiTheme 의 기존 값)을 바꾸면 다른 화면에 영향이 가므로, 바꿔야 하면 범위를 보고한다.
   - 패널 레시피(크림 + 잉크 테두리 + 안쪽 점선 + 아래 그림자)는 StorageSurface 같은 공통 칠하기의 캐주얼 변형이나 9-slice 스프라이트 하나로 만든다.
   - 글자 외곽선은 한 가지 방식을 정해 근거를 남긴다. uGUI Outline/Shadow 를 글자마다 여러 개 붙이지 않는다.
2. HUD 그림 다시 만들기:
   - build-vectors.cjs 를 복사한 캐주얼판 생성기(예: Docs/Design/GlobalHUD/Resources/v2-casual/)로 frame-seal, frame-vital, fill-vital, frame-active, frame-status, badge-*, status-*, control-*, xp-* 를 README 레시피로 다시 만든다. 파일 이름, 크기, 9-slice 테두리, .meta GUID 를 유지한다.
   - menu-hunt-edict 와 menu-edict-* 는 캐주얼 둥근 단추(흰 바탕 + 잉크 + 아래 그림자)에 잉크 단색 아이콘으로 만든다.
   - 손으로 그린 래스터 그림은 금지한다. 벡터 생성기 또는 Codex 이미지 생성만 쓴다.
3. 인장·물약 그림:
   - 인장: Docs/Design/GlobalHUD/Casual/assets/seal-{warrior,ranger,mage}-m.png 를 Resources 로 가져와 Portrait(heroClass)가 직업별 새 인장을 쓰게 한다. seal-*-f 세 장도 같은 폴더에 함께 넣되 연결하지 않는다. 성별 저장은 캐릭터 생성·선택 작업의 범위이므로 이번에 만들지 않는다.
   - 물약: potion-hp/mp/utility.png 는 Art/GlobalHUD 의 같은 이름 파일을 GUID 를 유지하며 교체하고, PotionArt 의 잘라내기 사각형을 새 그림에 맞게 다시 잰다.
   - SkillAtlas 18+직업 초상을 다른 화면이 쓰면 그대로 둔다.
   - 새 텍스처는 Editor/ResourceTextureBudget.cs 에 행을 추가하고 용량 변화를 보고한다. 원본은 Git 이력으로 보존된다.
4. 배치(GlobalHudLayout, GameUI.BattleLayout, ReflowRiftMinimap):
   - 사냥 칙령 단추를 세 방향 모두 오른쪽 위 메뉴·설정 단추 아래로 옮긴다. BuildEdictTabIcons 는 탭을 단추 아래로 세로 메뉴(크림 틀, 아이콘 아래 탭 이름)로 펼치게 바꾼다. 게임의 콘텐츠 독 열과 겹치면 독 열 맨 위(설정 아래)에 넣고, 시안과 다른 점을 보고한다.
   - 인장 위 칙령 자리는 비운다.
   - 미니맵을 오른쪽 아래(스킬 줄 위)로 옮긴다.
   - 상태 효과: PC·가로는 첫 물약 왼쪽 끝 위에서 시작하고, 세로는 물약·스킬 줄 위 왼쪽에 둔다. 펼친 8칸이 미니맵·그래프 창과 겹치지 않아야 한다.
   - 세로 HUD 는 한 줄 맞춤 대신 위에서부터 상태 효과 → 물약·스킬 줄(양끝 정렬) → 인장·바·물약 설정 → 경험치로 쌓는다. 가로·PC 의 아래 구성은 지금과 같다.
5. 그래프 창:
   - 몬스터·보스 그래프를 trainingHud 에서 떼어 각자 창으로 만든다. 각 창이 이동·접기·기기 위치 저장을 갖는다(DpsHudPosition 파일 2개 추가, 저장 실패 토스트는 기존 문구).
   - 기본 위치는 DPS 아래 차례대로다. 세로 보스전에서는 보스 창이 왼쪽 활동 시간 창 아래에 온다.
   - DPS 접기는 DPS 창만 접는다.
   - 접힌 활동 시간 창은 제목 옆에 4색 비율 막대를 글자 없이 보인다.
   - 세로는 작은 창(폭 196·180, DPS 범례·세 칸 숨김)이고, 가로는 DPS 범례를 숨긴다. 창 모양은 캐주얼 패널이다.
   - 차트 색: 이번 판은 주황 #ff9f1c 실선 + 노랑 30% 영역, 직전 판은 하늘 #35a7ff 점선, 끝점과 원형 스킬 표시를 그린다. TrainingDpsChart 는 결과 창과 공유하므로 결과 창 차트도 같은 색이 된다(캐주얼 결과 시안과 같은 색). 이 점을 보고한다.
   - 활동 4색: 이동 #35a7ff, 공격 #ff5a5f, 회피 #ffd23f(선 #d9a300), 파밍 #4cbf55. RiftActivityPanel 을 공유하는 결과 창도 같은 색이 된다.
6. 상태 표시:
   - HP 30% 미만이면 인장 코랄 고리와 HP 바 고리를 보인다. 게임에 같은 기준이 이미 있으면 그 기준을 쓴다. 깜박임은 1.1초 주기로 약하게 한다.
   - 보호막은 HP 바 위쪽 민트 띠와 바 위 글자로 보인다(기존 shieldLine·shield 를 다시 칠한다).
   - 슬롯 상태 모양은 README "부품과 상태"를 따른다.
7. 문구: README 문구 표대로 기존 키를 재사용하고, 새 키는 한국어·영어를 함께 넣는다. 누락 키는 한국어로 대신 표시하고 기록하며, 빈 문자열이나 키 이름을 화면에 내지 않는다.

## 결정 (아래 기본값으로 진행하고 보고)
- 결정 1 — 글꼴: 이번 작업에서 새 글꼴을 넣지 않는다. UiFonts 의 기존 글꼴로 만들고, 스킬창 캐주얼 스킨 작업이 Jua·Noto Sans KR 을 main 에 넣었다면 그것을 쓴다.
- 결정 2 — HUD 크기: 시안은 휴대전화에서 읽기 좋게 그렸다(가로 슬롯 48, 세로 42 pt). 게임은 GlobalHudLayout 의 배율 규칙(기준 1600×900, 하한 0.4)을 지키되, 세로 쌓기에서 슬롯이 시안 크기에 가깝게 나오게 맞추고 실제 크기를 보고한다.
- 결정 3 — 콘텐츠 독과 칙령 단추: 위 4번 기본값을 따른다.
- 결정 4 — 결과 창: 공유 부품(TrainingDpsChart, RiftActivityPanel)의 색 변화만 허용하고 결과 창 배치는 바꾸지 않는다.

## 하지 않을 것
전투·보상·저장 규칙과 그래프 계산(DamageGraph, CombatFeedback) 변경, 영웅 성별·6명 저장(캐릭터 생성·선택 작업), 새 직업, 스킬 아이콘·보스 그림·전장·몬스터 아트 재제작, 제거된 글자 크기 조정 기능과 50~150% 검사·확대 글자 스모크, 다른 화면의 색·배치 변경, 손으로 그린 그림.

## 검증 (작업 중에는 관련 검사만)
- 관련 EditMode:
  - 위 관련 시험을 실행한다.
  - 레이아웃 시험을 더한다: 세 방향(세로 440×956, 가로 956×440, PC 1600×900)에서 상태 효과(접힘·펼침), 슬롯 줄, 미니맵, 칙령 메뉴(펼침), 그래프 창 기본 위치가 서로 겹치지 않고 안전 영역 안에 있는지 확인한다.
  - DpsHudPosition 새 파일 두 개의 저장·불러오기, 저장 실패, 화면 밖 복원을 시험한다.
  - 접힘 상태와 LocalizationTests(새 키 한·영), ResourceTextureBudget 검사를 실행한다.
  - tools/smoke.js 의 규칙(50%에서 가득, 20%·40% 색, 활동 비율 합 100, 탭 순서, 보스 생존 중에만 보스 창)을 EditMode 로 옮긴다.
- 스모크:
  - RuntimeGlobalHudSmoke, RuntimeBattleLayoutSmoke, RuntimeDamageGraphSmoke, RuntimeHudScaleSmoke, RuntimeTrainingGroundSmoke(RiftDps, Activity), RuntimeBattleQuickEditSmoke 를 새 배치와 창 분리에 맞게 고친다.
  - 칙령 메뉴는 아래로 펼쳐지고 탭이 열려야 한다.
  - 창 이동은 별도 Mac 실행 창에서 실제 마우스로 끌어 옮기고, 저장된 위치가 다시 실행했을 때 복원되는 근거를 남긴다.
- 화면:
  - 세로 440×956, 가로 956×440, PC 1600×900·1280×720·1720×720 에서 한국어·영어로 찍는다.
  - 상태는 기본, 보스전, 위험, 접힘 + 칙령 메뉴 + 상태 효과 펼침이다. 찍은 화면을 reference/ 의 같은 상태와 나란히 놓고 비교한다.
  - 안전 영역, 터치 44pt, 겹침, 문구 잘림을 확인한다. 휴대전화 비율은 Mac 창과 모의 여백 결과이므로 실기기 결과라고 하지 마라.
- python3 tools/check_ui_contract.py --verify 를 통과하고, 저장 후 UI 갱신 규칙(AgentRules/UI_Refresh.md)을 지킨다.
- 전체 EditMode 와 macOS 개발 빌드 런타임 스모크는 마지막 코드 변경과 최신 main 병합 뒤 한 번만 실행한다. 이미 실패하던 검사와 새 실패를 구분한다. 문서만 바꾸면 게임 실행 검사를 다시 하지 않는다.
- 성능:
  - 그래프 갱신은 지금 주기(DPS·피해 1초, 활동 0.15초)를 지키고 매 프레임 다시 배치하지 않는다.
  - 성능을 주장하려면 같은 조건의 이전·이후 실제 바이너리 각각 3회 이상 CPU·GC 원자료가 있어야 한다. 없으면 "미측정"으로 적는다.
  - 텍스처 메모리 변화는 용량 숫자로 보고한다.

## 산출물
- PR 1개(codex/casual-battle-hud)를 만든다. 병합은 사용자 승인 뒤에 하므로 하지 않는다. 시안 패키지(Docs/Design/GlobalHUD/Casual)를 포함하고, 관련 검사와 기록을 같은 커밋에 넣는다.
- 한/영 기록 Docs/Implementation/Casual_Battle_HUD_20261014(.en).md 를 쓴다(바꾼 것, 결정 결과, 시안과 다르게 한 곳, 검사 결과, 확인하지 못한 것). Global_HUD_Potions, Damage_Graphs, Rift_Activity_Time 기록도 갱신한다. 증거 스크린샷은 Docs/Implementation/CasualBattleHudEvidence/ 에 jpg 로 두고 파일마다 링크한다.
- 위키 원본이 바뀌면 AgentRules/Wiki_Publish.md 절차를 따르고, 산출물 정리는 AgentRules/Artifact_Cleanup.md 를 따른다.

## 보고(한국어, 짧게)
실제 변경, 결정 1~4 결과, 시안과 다르게 한 곳과 이유, 실제로 확인한 것과 미검증 범위(실기기, 성능), 용량 변화, 남은 일. macOS 결과를 실기기 결과로 말하지 마라.
```
