# 룬 보드 균열 10단계 개방과 콘텐츠 열림 연출

2026-10-24 · [English](Rune_Unlock_Rift10_20261024.en.md) · 사용자 플레이 테스트 수정 U-J(항목 16-a)

## 무엇이 바뀌었나

- **개방 단계 15 → 10.** `ContentUnlocks.json`의 `UL11_RUNE`가 균열 10단계에서 열린다. `RuneEconomy.MinimumDropStage`가 같은 값을 읽으므로 룬 드롭도 10단계부터 시작한다(의도한 변화). 게임 안 용어는 '단계'이고 사용자는 '균열 레벨'이라고 부른다. 잠금 토스트만 사용자의 문장을 쓰고 나머지 화면은 '단계'를 유지한다.
- **잠긴 룬 보드 버튼.** 우측 상단 콘텐츠 독(마을·광장·전투 HUD, 프롤로그 제외)의 룬 보드 칸에 항상 버튼이 있다. 열리기 전에는 무채색 메달리온에 자물쇠가 겹친 `menu-rune-board-locked` 아이콘이고, 누르면 "균열 레벨 10 달성 시 오픈됩니다"가 나온다(숫자는 `UL11_RUNE.stage`에서 읽는다, 영어는 "Opens when you reach Rift Level 10"). 열린 뒤 안내가 시작되기 전에 누르면 "마을에서 룬 보드 안내를 마치면 열립니다."가 나온다. 칸의 위치와 순서는 열릴 때도 같다(`CasualBattleHudTests`의 독 앵커 유지).
- **열림 연출 시점.** 균열 10단계 이상 도달 → 결과 처리 → **마을 광장에 처음 도착**하면 한 번. 전투와 결과창에서는 아무것도 시작하지 않는다. 저장된 대기 상태는 앱을 껐다 켜도 유지된다.
- **열림 연출 순서.** 인젤 미르의 소개(룬이 무엇이고 어디서 얻는지 한 장 추가) → 선물(룬 블럭 세트) 수령 → 독이 펼쳐지고 **자물쇠가 깨지며** 빛이 번짐(새 기능 개방 알림, `flow.skill_unlock`) → 룬 인장이 날아와 **그 칸에 안착** → 기존 5단계 플레이어블 가이드. 동작 줄이기 설정은 짧은 연출을 쓴다. 연출은 `DockUnlockBurst`(보이는 효과만)이며 수업의 시계는 `GameUI`가 따로 가진다.
- **선물(룬 블럭 세트)은 수업이 직접 준다.** `ClaimRuneBoardTutorial`이 고정 ID `rune-lesson-starter` 상자를 만들어 같은 `Transact`에서 연다. 15단계 첫 클리어 약속에서 이 세트를 뺐고(`RewardBoxes.json` 마일스톤 제거) 서버 계약(`server/testdata/liveops-contract.json`, 사이트 복사본, 생성 스키마)을 같이 고쳤다. 이미 첫 클리어를 받은 계정과 결과창을 거치지 않는 경로에서도 같은 선물이 한 번 나가고, 결과창의 보상 버튼은 다른 결과와 같은 규칙을 따른다.

## 공통 '콘텐츠 열림 연출' 장치 (대장간 U-K가 재사용)

| 부품 | 역할 |
| --- | --- |
| `ContentUnlockState.introduced`, `introSeeded` | 안내를 마친 콘텐츠 목록과 첫 읽기 표식. 둘 다 없는 저장은 기본값으로 읽혀 버전 검사(`Version=2`)를 깨지 않는다. |
| `ContentUnlocks.IntroIds` | 열림 연출이 등록된 콘텐츠 id. 새 연출은 여기에 id를 더하고 `GameUI.StartContentIntro`에 시작 호출을 더한다. 등록되지 않은 콘텐츠는 열리는 순간 '안내 마침'으로 기록되어 나중에 등록해도 다시 나오지 않는다. |
| `IntroPending(a,id)`, `NextIntro(a)`, `MarkIntroduced(a,id)` | 대기 판정, 해방 단계순 다음 한 개, 완료 기록. `GrantAll`(가이드 졸업)은 모두 완료로 기록하고 `MergeVerified`는 목록을 합친다. |
| `GameUI.StartContentIntro` | 조용한 광장(다른 팝업·튜토리얼·결과창·중단된 균열·촌장 환영 없음)에서만, 한 번에 하나씩, 0.5초 간격으로 확인해 시작한다. 시작한 안내가 끝날 때까지 다음 안내는 기다린다. |
| `DockUnlockBurst` | 독 칸 위의 자물쇠 깨짐·빛 번짐 효과. 자물쇠 그림을 주지 않으면 빛만 낸다(착지용). |

보석(`UL05_GEM`)도 10단계에서 열려 같은 마을 첫 도착에 열린다. 이번에는 룬만 연출하고 보석은 등록하지 않았다(등록하면 같은 단계 안에서는 데이터 순서대로 보석이 룬보다 먼저 나온다).

## 이전 저장의 처리

| 계정 | 결과 |
| --- | --- |
| 새 계정이 9 → 10 클리어 | 결과창은 평소와 같다. 마을 첫 도착에 연출·수업 1회. |
| 룬이 닫혀 있던 균열 10~14 계정 | 읽는 순간 보드가 열리고 마을 첫 도착에 1회. |
| 15 이상, 수업 완료·진행 중, 전부 개방(`everythingOpen`) | 안내를 마친 것으로 기록. 잠기지 않고 다시 보지 않는다. 진행 중이던 수업은 저장된 단계에서 마을 광장 기준으로 이어진다. |
| 15 이상, 옛 방식의 `Reward` 단계 | 저장된 15단계 약속의 세트를 소비하고 두 번째 세트를 주지 않는다. |
| 최고 클리어만 높고 정확한 클리어 기록이 없는 계정 | 안내 없이 열린 아이콘(기존과 같은 접근). |

## 검증과 한계

검증은 개발 worktree(U-G 브랜치 위)의 일회용 세이브에서만 했고 사용자 테스트본·세이브는 쓰지 않았다.

- **집중 Edit Mode** 14개 클래스 341개 중 336개 통과, 5개 실패였다. 실패는 모두 이번에 새로 쓴 테스트의 작성 오류(JSON 편집 정규식 4건, 숙련 XP가 레벨 경계에 딱 떨어지는 경우 1건)였고 고친 뒤 `ContentIntroTests`·`RiftContentUnlockTests` 30개가 모두 통과했다. 나머지 12개 클래스(`ContentUnlockTests`, `RuneEconomyTests`, `RewardBoxTests`, `LiveOpsTests`, `LocalizationTests`, `TutorialProgressionTests`, `CasualBattleHudTests`, `GlobalHudResourceTests`, `GuideTutorialTests`, `HuntEdictProgressionTests`, `HuntEdictSentenceTests`, `RepeatHuntTests`)는 첫 실행에서 통과했다. 전체 Edit Mode는 실행하지 않았다.
- 서버 계약: `server`의 `test_liveops` 22개 통과. `build.py --generate-only`로 바뀐 파일은 생성 스키마와 계약 사본뿐이다.
- `tools/check_ui_contract.py --verify` 15개 통과, `tools/check_ui_refresh.py` 바인딩 14개 통과.
- **실제 GameUI 정지 화면**(에디터 GUI, 한국어·영어, 440×956·956×440): 잠긴 칸이 같은 자리(여섯 번째)에 있고, 자물쇠 깨짐·착지 빛·열린 아이콘이 보인다. 누름 토스트 문구는 한국어 "균열 레벨 10 달성 시 오픈됩니다"·"마을에서 룬 보드 안내를 마치면 열립니다.", 영어 "Opens when you reach Rift Level 10"·"The Rune Board opens after its introduction in town."로 확인했다. [세로 한국어](RuneUnlockRift10Evidence20261024/portrait-ko-dock-stages.png) · [가로 한국어](RuneUnlockRift10Evidence20261024/landscape-ko-dock-stages.png) · [세로 영어](RuneUnlockRift10Evidence20261024/portrait-en-dock-stages.png) · [보고](RuneUnlockRift10Evidence20261024/runtime-report.txt).
- **실제 GameUI 틱 점검**: 대기 → 시작(`Waiting`→`Reward`, 안내 기록) → 이야기 창 → 수령(룬 +5, 첫 클리어 청구 없음, 수업 상자 소진) → 자물쇠 깨짐 → 비행 → 착지(`OpenBoard`, 아이콘 열림). 이 점검이 에디터에서만 터지는 `??` 컴포넌트 검사 오류를 찾아 고쳤다. [결과](RuneUnlockRift10Evidence20261024/flow-report.txt).
- **하지 않은 것**: 런타임 스모크(`RuntimeRuneBoardSmoke.Unlock`, `RuntimeNaturalRiftProof`, `RuntimeButtonUxSmoke`의 단언은 고쳤지만 macOS 플레이어에서 실행하지 않았다), 실제 포인터·터치 입력, 연출의 시간 경과(정지 프레임과 로직 단계만 확인), PC 16:9·16:10·21:9 화면, 모바일 실기기, 성능.

- 일부러 하지 않은 것: 전체 Edit Mode, 모바일 실기기, 성능 측정, 균형 모델(`tools/balance_rewards.py`의 룬 개방 15 가정)과 대시보드 문구 갱신, 운영 보상 묶음 `rune-apprentice-v1`(최소 15단계) 변경.
- `MergeVerified`는 콘텐츠 목록만 합치는 어댑터라 다른 기기의 튜토리얼·보상 소유를 증명하지 못한다.
- `en.txt` 끝에 이 작업의 항목만 이어 붙였으므로 다른 단위와 병합할 때 꼬리 겹침이 날 수 있다(내용 충돌 아님).
