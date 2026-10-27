# 균열 전투 HUD 정리 — 설정 버튼 테두리, 그래프 겹침 금지, 콘텐츠 독, 필드 상자 갯수 (2026-10-24)

[English](Rift_Battle_HUD_Cleanup_20261024.en.md) · 단위 U-G · 브랜치 `claude/battle-hud-rift-20261024-c02d25`

## 사용자 요청
2026-10-24 플레이 테스트(첨부 10, 세로 폰 균열 전투): ① 설정 버튼의 흰 테두리 제거, ② 전투 정보 그래프가 어떤 접힘·펼침에서도 겹치지 않게(위 그래프를 펴면 아래 그래프를 밀기) + 첫 균열 전투에서 활동 시간·실시간 DPS·몬스터 피해 그래프를 설명하는 튜토리얼, ③ 콘텐츠 버튼을 설정 버튼 바로 아래에 붙이고 기본 닫힘, ④ '필드 상자 갯수'는 별도 카드 없이 상단 텍스트로만.

## 확인한 원인과 변경
| 항목 | 원인(소스로 확인) | 변경 |
| --- | --- | --- |
| ① 테두리 | 설정 버튼은 `UiButton`이고, 크림색 둥근 판은 버튼의 `Face`("Button chrome")다. 마을(`ReflowPlazaGear`)은 `Face`를 끄지만 전투의 `CompactGear`는 끄지 않았고, 캐주얼 전투 재배치가 곧바로 `CombatHudButton(settings, UiSkin.Casual)`로 `Face`를 다시 칠했다. | `CompactGear`가 `Face`를 끈다(전투 레거시·캐주얼 공통). 캐주얼 재배치의 `CombatHudButton(settings…)` 호출 제거. |
| ② 겹침 | 네 창(활동·DPS·몬스터·보스 피해)은 레이아웃 기본 자리 뒤에 `DpsHudDrag.Restore()`가 각자 저장 위치를 따로 적용했고, 창끼리 부딪침을 푸는 단계가 없었다. | `BattleGraphStack.Resolve`(순수 로직): 위쪽 창이 자리를 지키고 부딪히는 창은 그 아래로 밀린다(연쇄). 표시 전용이며 저장 위치는 바꾸지 않아 접으면 아래 창이 돌아온다. 밀린 결과가 HUD 위쪽 한계를 넘으면 승인된 기본 자리로 돌아간다(잘리거나 숨지 않음). 재배치 끝(`ResolveCasualGraphs`)·창을 놓을 때(`DpsHudDrag.Dropped`)·보스 그래프 표시 전환 때 실행. |
| ② 튜토리얼 | 없음 | `GameUI.RiftGraphTutorial.cs`: 계정의 첫 균열 전투(가이드 스테이지·훈련장 제외)에서 벨리나가 세 번 눌러 가며 활동 시간 → 실시간 DPS → 몬스터 피해 창을 밝혀 설명한다. 설명 중 전투는 일시정지(`run.paused`, 끝나면 원래 값으로, 저장에 일시정지가 실리지 않게 저장 전에 복원). 마지막 단계를 저장(`AccountGuide.riftGraphLesson`, 없으면 미완료, `GameStore.CompleteRiftGraphLesson`)해야 끝나며 저장 실패 시 세 번째 카드가 돌아온다. |
| ③ 독 | 전투 독은 고정 칙령 자리(`layout.edict` 아래, 세로 기준 y≈162)에 걸려 설정 버튼과 떨어져 있었고, `contentDockOpen`이 마을에서 연 상태를 전투로 가져왔다. | 독 화살표를 설정 버튼 아래 한 칸(6 단위)에 건다(`CasualBattleHudLayout.GearTop/DockTop`). 새 전투는 항상 접힌 채 시작하고, 같은 전투 안의 재구성(언어·화면 비)은 사용자가 정한 상태를 유지한다. |
| ④ 상자 갯수 | `chestCountText`는 미니맵 카드의 자식이라 지도가 없어도 빈 카드에 떠 있었다. | 캐주얼 HUD에서는 헤더 세 번째 줄(XP 줄 오른쪽)의 글자로만 표시한다. 자리는 "XP 줄 끝 뒤, 단축 캡션 앞(캡션과 6 단위 간격)"이고 XP 줄이 양보한다(전체 → 다음 스킬 문구 제외 → XP만 → 말줄임). 킬 때마다 숫자만 바뀌는 XP 줄이 글자 수는 그대로여도 자리가 흔들리지 않게 숫자를 8로 친 폭으로 자리를 잡는다. 폰트 크기는 줄이지 않는다. 숫자나 언어가 바뀔 때만 글자를 바꾼다. 카드는 코너 지도나 길 찾기 오류 줄이 있을 때만 남는다. |

## 사용자 결정이 필요한 것 (기본값으로 진행)
1. 튜토리얼 시간 동안 피로도: 지도·메뉴 같은 기존 일시정지와 같이 **흐른다**(게임 규칙 'F02: 일시정지에도 피로도는 흐릅니다'). 멈추려면 `GameController`의 피로도 정산을 바꿔야 해서 이번 단위에서는 하지 않았다.
2. 계정에 새 필드가 없으면 미완료로 읽으므로, **이미 균열을 해 본 계정도 다음 첫 균열 전투에서 한 번** 튜토리얼을 본다.
3. 건너뛰기 버튼은 넣지 않았다(세 번 누르면 끝). 필요하면 추가.
4. 독을 닫힘으로 둔 결과 전투 중 사냥 칙령 빠른 편집은 독을 열어야 눌린다(요청한 동작).
5. 문구는 기존 '상자 N / M'을 그대로 썼다('필드 상자'로 바꾸려면 en 항목 하나 추가).

## 검증 (2026-10-24, 개발 환경 worktree `battle-hud-rift-20261024-c02d25`, 사용자 테스트본·세이브 미사용)
- 집중 EditMode(배치 모드) 14개 클래스 **265개 통과, 실패 0, 건너뜀 0**, 컴파일 오류 0: `BattleGraphStackTests`(새), `RiftGraphLessonSaveTests`(새), `CasualBattleHudTests`(독 기준점 갱신), `CasualHudSharedTests`, `CurrentBuildSaveTests`, `DpsHudTests`, `EdictMenuColumnTests`, `FirstPlayGuideTests`, `GlobalHudTests`, `TownPlazaLayoutTests`, `TutorialProgressionTests`, `UiButtonTests`, `UiPopInTests`, `LocalizationTests`. 전체 Edit Mode는 실행하지 않았다.
- `BattleGraphStackTests`: 5개 판(440×956, 956×440, 1600×900, 1440×900, 2560×1080) × 보스 유무 × 접힘 16가지 × 무작위 저장 위치 20회에서 겹침 0, HUD 위쪽 한계 밖으로 잘림 없음(못 담으면 승인된 기본 자리), 위 창을 펴면 아래 창이 연쇄로 밀리고 다시 접으면 돌아옴, 기본 자리는 움직이지 않음.
- `python3 tools/check_ui_contract.py --verify` 통과(회귀 15개), `python3 tools/check_ui_refresh.py` 통과(14 bindings).
- 실제 `GameUI`로 균열 전투 HUD를 만들어 정지 화면 **24장면**(세로 440×956·가로 956×440·PC 1920×1080 × 한국어·영어 × 기본/저장 위치 겹침+보스/독 열림/튜토리얼)을 찍고 값을 읽었다. 네 창 겹침 0, 설정 버튼 `Face` 꺼짐, 튜토리얼 세 단계 동안 전투 일시정지 → 끝나면 원래 값 복원, 상자 갯수·XP 줄·캡션 겹침 0(최소 간격 6 px). 대표 10장과 수치: [RiftBattleHudEvidence20261024](RiftBattleHudEvidence20261024/runtime-report.txt). 임시 Editor 캡처 스크립트는 커밋하지 않았다. 16:10(1440×900)·21:9(2560×1080)·16:9(1600×900)는 앞선 같은 코드 계열의 기본 장면에서 겹침 0·헤더 겹침 0을 확인했고, 마지막 코드(캡션 간격·XP 폴백 추가) 이후의 24장면은 위 세 판이다.
- 정지 화면은 배치·대비 근거이며 동작 근거는 아니다. 드래그 놓기·접기 버튼 누르기·실제 터치는 이 단위에서 실행하지 않았다(순수 로직 테스트와 같은 재배치 경로의 장면으로만 확인).

## 미실행·남은 위험
- 런타임 스모크 `RuntimeContentDockSmoke`, `RuntimeDamageGraphSmoke`, `RuntimeMapDisplaySmoke`는 새 규칙(새 전투는 독 접힘·창 겹침 금지·상자 갯수가 헤더 글자)에 맞춰 **단언만 고쳤고 실행하지 않았다**(macOS 개발 플레이어 빌드 필요). `RuntimeTrainingGroundSmoke.RiftDps/Activity`는 '놓은 위치가 화면 위치와 정확히 같다'를 단언하므로, 놓은 창이 다른 창과 겹치면 화면에서 밀려 단언이 어긋날 수 있다(저장 파일은 그대로) — 통합 때 재기준이 필요할 수 있다.
- 포트레이트에서 독을 연 동안에는 독이 오른쪽 레인의 DPS 창 가장자리를 덮는다(이전에도 같았고 독은 겹쳐 그리는 창). 독까지 장애물로 보려면 승인된 레인 이동이 필요해 사용자 결정으로 남긴다.
- `Assets/HELLSCRIPT/Resources/Localization/en.txt` 끝에 세 줄을 이어 붙였다. 다른 단위(U-A·U-C~U-K)도 끝에 붙이므로 병합 때 꼬리 겹침 충돌이 날 수 있다(내용 충돌 아님). `LocalizationTests` 전체는 통과했다.
- `GameUI.ContentDock.cs`는 이 단위가 먼저 바꾼다(U-J의 잠긴 룬 버튼이 이어받음): `CompactGear`가 `Face`를 끄고, 독 자리는 `CasualBattleHudLayout.DockTop`, 전투 시작 때 `contentDockOpen=false`(`battleDockRun`).
