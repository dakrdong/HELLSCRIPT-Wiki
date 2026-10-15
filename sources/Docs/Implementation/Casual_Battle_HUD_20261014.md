# 캐주얼 기본 전투 HUD — 소스 인계 (2026-10-14)

[English](Casual_Battle_HUD_20261014.en.md) · [시안 정본](../Design/GlobalHUD/Casual/README.md) · [공통 인계](Casual_UI_Parallel_Handoff_20261014.md)

기본 HUD와 일반 균열의 네 그래프 창을 캐주얼 시안에 연결했다. 이 기록은 **소스·정적 검사 인계**이며 Unity 실행, EditMode 실행, 플레이어 빌드, 실제 화면 통과를 의미하지 않는다. 기존 13개 화면과 캐릭터 생성/선택을 합친 뒤 통합 담당이 최종 테스트 한 번을 수행한다.

## 기준과 소유

- 격리 경로: `task-22/battle-hud`, 브랜치 `codex/casual-battle-hud`. 원본 checkout 동시 쓰기 없음. 로컬 공유 Git 객체 + sparse source checkout이며 Library/플레이어 빌드 복제 없음.
- 통합 순서: 기존 13 UI `8d356b3c` → 문서 `529effd6`/`4148241a` → 캐릭터 6슬롯 `e3809624`/`93219e3f` → 생성·선택 `b87b581b249c5e682cd7832fa9190b24c0dd9c4e` → 공통 HUD API **`b0d4da6be02d9069bff04c4e81c4029caa677c13`** → 이 HUD 커밋. 이 커밋은 b0d4da6b의 직계 후속이며 이전 중간본을 별도 통합하지 않는다.
- 공통 통합 소유자: 복구 담당 `01a13a01-b651-7457-b0e9-aa4f6325a0e9`. 이전 담당 `01a13790-39bc-7062-a516-b83a354000a8`의 공통 미커밋 변경을 복구 담당이 커밋했다.
- 전용 추가: `CasualBattleHudLayout/Style`, `GameUI.CasualBattleHud/CasualDamageGraphs/CasualEdictMenu`, `GameController.BattleHud`, `CasualBattleHudTests`.
- 기본 HUD 소유 수정: `GlobalHudLayout/View/Snapshot`, `StatusStripView`, `Data/GlobalHudLayout.json`. 연결부: `GameUI.BattleLayout/DamageGraphs/Rift/RiftActivity/TrainingGround/EdictSheet/EdictQuick/Enemies`. Enemies는 새 보스 텍스트 배치 호출 1줄만 추가했다.
- 합의한 두 연결부도 이 커밋 소유: `GameController.Display.InitializeBattleHudPositions(directory)`와 `GameUI.IdleDashboard.Portrait(growth.heroClass,growth.gender)`.
- 공통 소스 `SkillIconView`, `PotionArt`, `TrainingDpsChart`, `RiftActivityPanel`, `ResourceTextureBudget`, `en.txt`는 통합 담당의 b0d4da6b를 그대로 소비한다.
- **보호 파일 미수정:** `GameUI.EdictPosition/Target/Order`, `CasualEdictSkinScope`, `GameUI.PotionQuick`. 생성·선택, HeroSlots, 세이브 이전, 재화·보상·전투 계산도 수정하지 않았다.

## 구현과 호환성

- 세 방향 HUD: 오른쪽 위 칙령 버튼과 아래로 펼치는 탭, 오른쪽 아래 미니맵, 첫 물약 위 상태 효과. 세로는 상태 → 물약/스킬 → 인장/바 → XP 순서다. 44pt 기준 슬롯/버튼은 화면에 맞추는 전체 배율을 적용하므로 모든 기기에서 44 물리 픽셀이라는 주장은 하지 않는다.
- `HeroSave.heroClass/gender`를 읽어 남녀 6개 인장을 선택한다. 클래스·성별·created 규칙이나 슬롯 스키마를 별도로 만들지 않는다. 인장 저체력 링은 HP 30% 미만에 1.1초 주기로 코랄 표시, 사망은 회색이다.
- 스킬/궁극기/상태 프레임은 `SetCasualHudFrame`을 사용한다. 물약은 `PotionArt.Bottle(...,casualHud:true)`의 **512 전체 캔버스**를 슬롯의 66%에 놓는다. 알파 재crop은 하지 않는다. 제작 엘릭서의 별도 경로와 기존 스왑/홀드/설정 동작을 유지한다.
- 네 그래프는 독립 창이다. DPS/활동의 기존 파일을 유지하고 `hellscript-monster-damage-position-v1.json`, `hellscript-boss-damage-position-v1.json` 두 기기 위치 파일만 추가한다. 실패 시 기존 파일을 보존하는 `DpsHudPosition` 저장 경로를 재사용한다. 접기 상태는 기존처럼 세션 중 유지되며 계정 세이브에 넣지 않는다.
- DPS와 받은 피해·활동 집계, 20%/40% 경계, 50% 막대 포화, 보스 생존 조건, 캐스트 마크 상한 60개를 유지한다. 접힌 활동 창에도 원래 백분율을 쓰는 글자 없는 비율 막대를 갱신한다. 훈련/튜토리얼의 기존 그래프 배치·입력 경로는 유지한다.
- 상단 칙령과 기존 전용 편집 창 사이에는 `BuildCasualCompatibleEdictPanel` 어댑터만 둔다. 세로 편집 화면 계산 중 하단 기준 앵커를 전달하고 `finally`에서 실제 상단 앵커를 복원한다. 보호 파일의 화면·저장 구현을 복제하지 않는다.
- Jua 제목/숫자, Noto 본문, 기존 `SkillTreeD4TextShadow` 한 개의 재사용 메시 외곽선을 쓴다. uGUI Outline/Shadow를 여러 개 쌓지 않는다. 갱신은 기존 DPS 1초/활동 0.15초 경계를 유지하며 캐스트 프레임 탐색 목록을 재사용한다. CPU/GPU/GC 실측은 이번 범위에서 수행하지 않았다.
- 공통 b0d4da6b는 결과창에도 주황/하늘 및 활동 4색만 연결한다. HUD 전용 점선/끝점/선 굵기와 결과창 배치는 분리되어 있다.

## 아트와 보존

실제 열어 본 것은 로컬 참조 JPG 14장과 승인 그림의 픽셀 접촉시트다. 지정된 `dakrdong@gmail.com` Chrome 프로필을 확인해 온라인 artifact를 열었으나 제목 로드 뒤 시간 초과가 발생했다. **온라인 대지 자체는 확인하지 못했다.** 다른 Chrome 계정 창은 조작하지 않았다.

- 런타임 경로: `Assets/HELLSCRIPT/Resources/Art/GlobalHUD/Casual/`의 PNG 60개. 승인 인장 6개·물약 3개를 재사용하고 시안 벡터 51개를 포함한다. 이미 마련된 파일을 재사용했으며 후속 이미지 재생성·imagegen 호출은 없다.
- 소스/레시피: `Docs/Design/GlobalHUD/Casual/` 및 `Docs/Design/GlobalHUD/Resources/v2-casual/`. 현재 작업 완료를 위해 생성기를 다시 실행할 필요가 없다.
- [런타임 해시·GUID·크기·9-slice](CasualBattleHudEvidence/runtime-art.json), [승인 원본 해시](CasualBattleHudEvidence/approved-art.json), [감사 결과](CasualBattleHudEvidence/art-audit.json).
- PNG 합계 **2,846,922 bytes**. RGBA8 무압축 픽셀 상한 **20,702,464 bytes**이며 실제 GPU 사용량 측정치가 아니다. 공통 budget은 Casual 하위 최대 512, mip/readable 비활성이다.
- 기존 최상위 HUD PNG 91개와 메타 91개는 바이트 동일하다. 구 GUID·Resources fallback·공유 그림을 보존했고 삭제 파일 0개다. 전체 화면 검증 전 자산 정리는 수행하지 않는다.

## 검사와 남은 검증

- Roslyn으로 실제 Unity 참조를 사용한 Runtime **861**, Editor **53**, Tests **271** 소스 컴파일: 오류 0. 기존 경고는 90/28/43개, 변경 파일 경고 0. Tests 어셈블리 **컴파일**이며 EditMode 테스트 실행은 아니다.
- 실제 `GlobalHudLayout`와 `CasualBattleHudLayout` C#을 값 형식/수학 shim으로 실행: 440×956, 956×440, 1600×900, 1280×720, 1720×720, 보스 유무·칙령 메뉴·16개 접기 조합 **320개 / 38,480검사 통과**. 기본 창·최대 상태줄·슬롯·메뉴의 경계/겹침을 검사하며 폰트/입력/렌더링 증거는 아니다.
- UI 계약 15/15, UI refresh 16 bindings, 시안 소스 smoke 38 variants, diff 공백 검사 통과. [컴파일 결과](CasualBattleHudEvidence/compile-result.json), [배치 결과](CasualBattleHudEvidence/geometry-result.json), [최종 근거](CasualBattleHudEvidence/receipt.json).
- 통합 진입점: `GlobalHudTests`, `GlobalHudResourceTests`, `CasualBattleHudTests`, `CasualHudSharedTests`, `CasualChartSkinTests`, `BuildResourceTests`, 기존 DamageGraph/DpsHud/CombatActivity/HuntEdict/Potion 검사.
- 단일 통합 플레이어의 기존 opt-in: `-hellscriptGlobalHudSmoke`, `-hellscriptHudScaleSmoke`, `-hellscriptBattleLayoutSmoke`, `-hellscriptDamageGraphSmoke`, `-hellscriptRiftDpsSmoke`, `-hellscriptDpsDragSmoke`, `-hellscriptRiftActivitySmoke`, `-hellscriptBattleQuickEditSmoke`. 해당 스모크의 기존 폐기용 save/evidence 인자를 사용한다. 실제 사용자 세이브를 사용하지 않는다.
- 최종 확인: 한국어/영어, 긴 보스 문구와 큰 DPS 숫자, 6인장, 자원 부족 ◆·잠김/빈칸/사망/저체력, 최대 상태 스크롤, 네 창 독립 접기·드래그·재실행·저장 실패, 칙령/물약 설정·회전·미니맵 3모드·관찰 메뉴·설정·절전/복귀, 기존 13 UI 회귀. 물리 마우스/모바일 터치/스크린 리더/성능 수치는 미검증이다.

## 시안 대비 명시적 조정

- 세로 슬롯 면은 42→44 디자인 pt, 캡션 공간을 포함한 실제 면 하단은 세로147/작은 가로37/PC54다. 작은 가로 상태줄과 열린 메뉴가 슬롯 면에 닿지 않도록 조정했다.
- 작은 가로에서는 보스 피해 창을 가운데 `x306/y96`에 둔다. 오른쪽 두 창 아래에 5행을 쌓으면 하단 슬롯을 침범한다. 보스 상태와 헤더 버튼을 분리했고 긴 현재 행동은 기존 관찰 메뉴에서 읽는다.
- 칙령 메뉴가 열리면 콘텐츠 독을 잠시 숨기고 닫으면 복원한다. 세로 오른쪽 두 그래프는 메뉴 여유 폭을 확보하고, PC/작은 가로 미니맵은 열린 메뉴를 피한다. 사용자 저장 위치를 다시 쓰지는 않는다.
- 새 프레임은 별도 Casual 경로에 두어 기존 GUID를 교체하지 않았다. 실제 화면 및 전체 참조 검증 전 구 아트 영구 삭제는 보류한다.
- DPR/CSS와 Unity 폰트 메트릭의 픽셀 일치, 보스/스킬 원화의 기존 리소스 분위기 차이, 실제 렌더링 가독성은 최종 통합 리뷰 항목이다.

동결 이후 추가 변경은 통합 담당과 새 소유 범위를 합의한다. 원격 push, main 병합, 웹 빌드, Unity 실행, 별도 플레이어 빌드, 실제 세이브 접근은 수행하지 않았다. 임시 소스/검사 산출물은 task-22의 `artifact-lifecycle.json`에 따라 통합 리뷰까지 보존하며 영구 삭제하지 않는다.

기록 계측: 중단·도구 재연결을 포함한 전체 작업/대기/재시도 시간과 사용 토큰은 미측정이다. 최종 정적 UI 계약 실행 15개는 로그상 43.491초다. Unity 전체 테스트·플레이어 빌드·위키 실행은 0회다.
