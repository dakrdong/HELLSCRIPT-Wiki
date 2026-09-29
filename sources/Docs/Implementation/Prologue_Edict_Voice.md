# 프롤로그 — 칙령의 목소리

갱신일: 2026-09-29

[English](Prologue_Edict_Voice.en.md)

첫 필수 맵 「성소로 가는 길」(갑옷 지급·장착)을 요청에 따라 「칙령의 목소리」 프롤로그로 바꿨다. 플레이어는 얼굴이 보이지 않고 후광만 빛나는 신의 목소리로 등장하며, 사냥 칙령 두루마리를 내려 캐릭터를 살린다. 두 번의 사냥 칙령 설정은 지정된 버튼만 누를 수 있는 강제 단계로 진행한다.

## 흐름

| 단계 | 전투 상태 (`ProloguePhase`) | 화면 |
| --- | --- | --- |
| 1 | `Surrounded` (정지) | 캐릭터가 마물 여섯에게 둘러싸여 무릎을 꿇은 자세(절망). 캐릭터의 독백 뒤 하늘에서 빛기둥이 쏟아지고 하늘의 목소리가 명한다. |
| 2 | 〃 | 두루마리가 내려오고 캐릭터가 두 팔을 하늘로 뻗어 받든다(들어 올리기 자세). 두루마리가 빛난 뒤 사냥 칙령 인장이 날아가 오른쪽 위 메뉴 칸에 자리 잡는다. |
| 3 | `EdictLesson` (정지) | 강제 단계: 메뉴의 사냥 칙령 → 스킬 탭 → 첫 스킬 → `+`(스킬 포인트 1) → 장착 → 저장 → 1번 칸 → 사냥 칙령 편집 → 프리셋 탭 네 개를 차례로 설명 → 지정 프리셋 선택 → 프리셋 활성화 → 닫기. |
| 4 | `Encircled` | 포위한 마물과 실제 전투. |
| 5 | `Gatekeeper` | 포위를 뚫은 뒤 대화, 보스 방으로 이동, 수문장 등장 연출. |
| 6 | `SurvivalLesson` (정지) | 캐릭터 HP가 50% 아래로 떨어지면 시간이 멈추고 목소리가 개입한다. 강제 단계: 사냥 칙령 → 생존 → 자동 물약 → 간편 프리셋 → 넉넉한 생존 보급 → 피해 유형별 회피 → 간편 프리셋 → 모든 예고 회피 → 저장 → 닫기. |
| 7 | `Showdown` | 바꾼 설정으로 수문장과 계속 싸운다. |
| 8 | `Cleared` | 승리 연출, 보상 **금화 1,000**. "보상을 받고 마을로"를 누르면 보상과 튜토리얼 완료가 한 거래로 저장된다. |
| 9 | 마을 | 도착 카드 뒤 안톤 진다크의 첫 대화(마을 이벤트 시작). 이방인을 환영하고, 신의 계시를 받고 마중 나왔다고 밝힌다. 균열에서 악마가 뛰쳐나와 위기가 왔고 "신의 계시를 받은 자가 균열의 악마를 처단하리라"는 예언이 있었다고 전한다. 계시를 받은 여러 전사가 함께 싸우며 모여 살아 집성촌이 되었고, 그래서 전사를 위한 상인들이 자리 잡았다고 소개한 뒤 이동 방법과 첫 균열을 안내한다. |

### 직업별 강제 스킬

1레벨에 찍을 수 있는 스킬은 직업마다 이미 세 개(W01~W03 등)이므로, 강제할 스킬 하나를 고정했다(`Tutorials.PrologueSkill`). 안내는 스킬 ID로 이 스킬만 가리킨다.

| 직업 | 스킬 | 시작 프리셋 | 선택하게 할 프리셋 |
| --- | --- | --- | --- |
| 전사 | W01 회오리 | 제자리 집중 회전 | 생존 우선 전투 |
| 궁수 | A01 관통 사격 | 정예 집중 관통 | 한 명도 빠르게 관통 |
| 마법사 | M01 화염구 | 정예 집중 화력 | 꾸준히 발사 |

## 소유와 저장

- **시작 상태**: 새 캐릭터는 모든 스킬이 무료 1레벨(버전 3)이고 1번 칸에 첫 스킬이 이미 있어, 요청한 "포인트 1을 찍고 장착"이 성립하지 않는다. 첫 입장 때 `GameStore.PreparePrologue`가 스킬 배분을 초기화(버전 4, 1레벨에 포인트 1, 빈 칸)하고 첫 스킬의 프리셋을 시작 프리셋으로 정한 뒤 저장한다.
- **강제 단계**: `GameUI.TickPrologueLesson`이 매 프레임 실제 저장 상태(소유 캐릭터의 스킬·프리셋·칙령 값)와 창 상태를 읽어 다음에 누를 버튼 하나를 정한다. 창을 닫거나 다른 탭으로 가도 다음 단계가 다시 계산된다.
- **가림막**: `PrologueGate`는 콘텐츠 창보다 위(정렬 2000)의 별도 캔버스로, 지정 버튼 영역만 입력을 통과시키고 나머지를 어둡게 덮는다. Esc 뒤로 가기도 막는다. 지정 버튼을 2초 동안 찾지 못하면 입력 차단을 풀어 진행이 막히지 않게 한다. 캔버스는 공통 규칙에 따라 `GameUI.cs`가 만든다.
- **칙령 저장**: 튜토리얼 전투는 체크포인트를 `guide.tutorialRun`에 두므로, `CommitHuntEdict`가 이 전투에도 설정 변경을 적용하도록 분기를 추가했다. 다시보기에서는 복사본 캐릭터에만 적용한다.
- **보상**: `CompleteTutorialRun`(거래 `tutorial-map-complete-v2`)이 금화 1,000과 완료 표시를 함께 저장한다. 다시보기는 보상이 없다. 마을에 도착하면 방금 고른 "넉넉한 생존 보급"이 물약을 자동 보충하므로 금화가 곧바로 줄어드는 것이 정상이다(스모크에서 175 사용).
- **이전 저장**: 옛 갑옷 흐름의 체크포인트(`tutorial-v1`)는 단계 의미가 달라 버리고 프롤로그를 처음부터 시작한다.
- 수문장은 개입 전에는 체력 35% 아래로 떨어지지 않는다. 쓰러지면 캐릭터만 회복하고 수문장이 입은 피해는 유지한다.

## 연출 자원

| 자원 | 위치 | 비고 |
| --- | --- | --- |
| 하늘의 목소리 초상 | `Resources/Art/NpcPortraits/prologue-divine-voice.png` | 얼굴 없이 후광만 빛나는 신. 코덱스 내장 이미지 생성, 모델 미확인 |
| 칙령 두루마리 | `Resources/World/Fx/edict_scroll.png` | 월드에 세워 보이는 카드. 빛기둥 뒤에 그려지면 하얗게 씻기므로 렌더 순서를 뒤로 뺐다 |
| 절망·들어 올리기 자세 | `ActorRig` `Despair`·`Offer` | 기존 절차적 리그에 자세 두 개 추가 |
| 빛기둥 | `WorldFx.DivineLight` | 기존 광선·모트·룬 원 재사용 |

요청 문서와 생성 기록: [프롬프트](../Art/Prologue/prologue-art-prompt.txt), [생성 기록](../Art/Prologue/prologue-art-manifest.json).

화자 이름표 옆 역할은 하늘의 목소리가 "칙령의 목소리", 캐릭터가 "계시를 받은 자"로 표시된다. 대화 중 캐릭터가 직접 말하는 줄에서는 오른쪽 듣는 사람 자리에 같은 캐릭터를 다시 그리지 않는다(`StoryDialogueWindow`). 개입 연출의 금빛 테두리는 대화가 끝나면 지우며, 테두리 모양은 세로 화면에서도 모서리까지 덮도록 바꿨다.

## 검증

- Edit Mode(배치 모드, 사본 프로젝트): `TutorialProgressionTests` 30/30. 세 직업 모두 준비 상태(포인트 1, 빈 칸, 시작 프리셋), 실제 전투로 포위 → 수문장 → HP 50% 미만 개입 → 수문장 처치, 금화 1,000 한 번 지급, 재시작 체크포인트, 옛 체크포인트 재시작, 쓰러짐 복구를 확인한다. 관련 검사 묶음(현지화·공통 UI·전투 기록·저장 문구·사냥 칙령·필드·대화·월드 FX·콘텐츠 개방) 314건 통과. 이 중 기존 실패 1건(3레벨 액티브 스킬이 없어진 데이터)을 함께 고쳤다.
- `tools/check_ui_contract.py`, `tools/test_ui_contract.py` 통과.
- macOS 개발 빌드 튜토리얼 스모크(두 프로세스, 440×956 등 20개 해상도·언어·글자 크기 조합): 인트로 대화 배치, 강제 스킬 단계 15회와 생존 단계 10회를 실제 포인터로 진행, 가림막이 다른 탭 입력을 막는지, 재시작 후 이어하기, 보스 개입, 보상, 마을 도착, 다시보기가 계정을 바꾸지 않는지 확인했다.
- 모바일 실기기에서는 보지 않았다.

## 화면 증거

- [포위와 절망](PrologueEvidence/prologue-02-despair.png) · [빛기둥](PrologueEvidence/prologue-03-light.png) · [목소리](PrologueEvidence/prologue-04-voice.png)
- [두루마리 강림](PrologueEvidence/prologue-05-scroll-descends.png) · [들어 올리기](PrologueEvidence/prologue-06-scroll-raised.png) · [인장 비행](PrologueEvidence/prologue-07-emblem-flight.png) · [강제 단계 시작](PrologueEvidence/prologue-08-edict-forced.png)
- [첫 스킬](PrologueEvidence/lesson-skill-03.png) · [포인트](PrologueEvidence/lesson-skill-04.png) · [1번 칸](PrologueEvidence/lesson-skill-07.png) · [프리셋 설명](PrologueEvidence/lesson-skill-09.png) · [프리셋 선택](PrologueEvidence/lesson-skill-13.png)
- [수문장 등장](PrologueEvidence/prologue-12-boss-entrance.png) · [개입](PrologueEvidence/prologue-13-intervention.png) · [자동 물약](PrologueEvidence/lesson-survival-05.png) · [모든 예고 회피](PrologueEvidence/lesson-survival-08.png)
- [승리](PrologueEvidence/prologue-14-victory.png) · [보상](PrologueEvidence/prologue-15-reward.png) · [마을 도착 대화](PrologueEvidence/prologue-17-arrival-dialogue.png)
- [스모크 결과](PrologueEvidence/runtime-tutorial-smoke.txt)
