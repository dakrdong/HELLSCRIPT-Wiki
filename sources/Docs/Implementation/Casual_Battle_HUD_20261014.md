# 캐주얼 기본 전투 HUD — 소스 인계 (2026-10-14)

> **2026-10-28 A2 정정:** 큰 칙령 창·≡ 메뉴·전용 부품·스모크 설명은 과거 기록이다. [현재 진입 코드](../../Assets/HELLSCRIPT/Runtime/Presentation/GameUI.HuntEdict.cs)와 [대체 화면·소유 코드](Hunt_Edict_Window_Removal_20261028.md)를 따른다. 이전 캡처와 검사 수치는 당시 구현의 근거다.

## 사냥칙령 진입·HUD 가독성 후속 (2026-10-18)

- 오른쪽 펼침 메뉴의 기존 전체 사냥칙령 버튼을 새 빠른 메뉴로 교체했다. 별도 상단 아이콘은 독이 있는 화면에서 숨기며, 캐릭터 인장은 기존 전체 편집창을 연다. 튜토리얼 전용 진입과 제한은 유지한다.
- `사냥칙령 → 전투 → 첫 교전 위치 / 전투 중 위치 → 해당 팝업` 순서다. 공개된 공격 대상·행동 우선순위와 튜토리얼 안내도 같은 목록에 연결했다.
- 바로가기·물약 설정·물약/스킬 프레임의 짙은 외곽선, 수량·궁극기 캡션의 남색/흰 외곽선을 적용했다. 세로 슬롯 기준선은 147→124 디자인 단위로 내리고 캡션을 슬롯 위로 옮겼다. 가로 슬롯 기준선은 유지한다.
- 실제 검사에서 DPS 드래그 핸들이 독 입력을 가렸다. `ContentWindowView.FixedOverlay` 공통 소유자로 HUD(105)와 빠른 편집(107) 사이인 106층에 독을 둔다. 저장·전투 계산·원본 아트는 바꾸지 않았다.
- 관련 Edit Mode 67/67, 공통 UI 후속 26/26, 최종 UI 계약 15/15, UI 갱신 14 bindings/198 sources 통과. macOS KO/EN 5비율 10/10와 마을 진입을 확인했다. 동일 레이어 코드를 공통 소유자로 옮긴 최종 상태는 세로 한국어 전투·마을 포인터 경로로 추가 확인했다. 전체 Edit Mode·모바일 실기기·성능 검사는 미실행이다.
- 근거는 `Builds/Validation/HudNavigation20261018/artifact-lifecycle.json`과 `evidence-layout-matrix/result.txt`, `evidence/result.txt`다. 빌드/네이티브 각 5회이며 접힌 독 검사 입력, URP 리소스, 그래프 입력 가림, 공통 소유자 보정의 실패·통과 로그를 보존한다. 처리·대기·재시도 시간은 별도 계측하지 않았다. 작업 브랜치 위키 생성·검사·게시는 0회다.
- 검사용 앱과 격리 세이브는 2026-10-25 검토 대상으로 보관하며 영구삭제 승인은 대기 중이다. 영구삭제나 공간 회수는 하지 않았다. 기존 소품 `.meta`와 `runInBackground` 변경은 보존하고 빌드 생성 설정만 복원했다.

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


## 2026-10-15 후속 — 세로 슬롯 그룹과 훈련 헤더

위 2026-10-14 본문은 당시 소스 인계 기록으로 보존한다. 아래는 사용자 요청에 따른 **HUD 선택 로컬 변경·한정 검증**이다. 현재 작업본에는 별도의 아트/provider 미커밋 통합 단위가 있으므로, 이 검증을 깨끗한 HEAD만의 통합 전수 통과로 해석하지 않는다.

### 변경 범위

- 세로 슬롯 최대 크기를 **50 디자인 단위**, 그룹 내부 간격을 **4**, 물약/스킬 그룹 사이를 **16**으로 한다. 남는 폭은 양 바깥으로 나누고 작은 화면에서는 슬롯과 간격을 함께 축소한다. 50 물리 픽셀을 보장하지 않는다. 실제 602×930 검수에서는 슬롯 **48.64px**, 내부 간격 **3.89px**, 그룹 간격 **15.56px**였다.
- 세로 콘텐츠 독은 기존 **44 기준**을 유지한다. 슬롯 확대가 독 버튼 크기까지 확대하지 않도록 `GameUI.ContentDock`의 세로 기준만 분리한다. 작은 가로/일반 가로 슬롯 배치는 이 슬롯 패치 범위에서 유지한다.
- 선택 슬롯 패치 SHA256은 `656eb19d5d432166d7d0c92225705a8cf541fe68f5880fe6b23f4f3e1a12de60`이다. `GlobalHudLayout` 변경, `GameUI.ContentDock`의 처음 두 레이아웃/주석 hunk, `GlobalHudTests` 기대값 한 줄만 포함한다. ContentDock의 `Emblem`→`GlobalHudView.LoadArt` 아트/provider hunk는 별도 통합 단위에 남긴다. HEAD `b5b86c51` 텍스트 3개에서 적용 검사만 통과했으며 이것은 컴파일·실행 증거가 아니다.
- 별도의 `GameUI.BattleLayout` 한 파일 수정은 **기존 훈련 화면의 noncompact 가로 헤더**에만 적용된다. 설정 버튼 크기를 칙령 위 여유에 맞춰 제한하고 독 상단을 칙령 아래로 옮긴다. 공유 `dockIcon` 값은 바꾸지 않아 독 아이콘·rail, field-events, 인장/미니맵 크기 입력을 유지한다. 일반 CasualCombatHud의 early return, 세로, compact 분기는 유지한다. 소스 SHA256은 `623060ef503b4bf14940aeb4a04f5fccd8fa64b2a80b9fd5bbb16bcd1e13664e` → `78dd091d775caeaff1a52858c9b40071c1ab8f8b91677b1c50db55672b639334`이며 메타는 보존했다.

### 실제로 확인한 슬롯 범위

- 한국어/영어 × 5 화면비(602×930, 390×844, 320×568, 956×440, 1600×900)의 **기하 10조건**을 확인했다. 각 조건의 보이는 대상 중심 EventSystem hit는 물리 입력 시험과 구분한다.
- 실제 callback 경로의 tap→상세 닫기 **16건**, 시간 경과 후 부분 hold→이탈 cancel **16건**, 비어 있지 않은 슬롯의 full hold→상세 닫기 **8건**을 확인했다. 나머지 빈칸/궁극기 슬롯의 full hold **8건은 NOT_RUN**이며 허가·장착을 추가해 시험하지 않았다.
- root가 기존 캡처의 하단 행 **9장**과 타이틀 비행이 자연 종료된 한국어 602×930 새 캡처 **1장**을 직접 확인해 슬롯 행을 수락했다. 이전 한국어 602×930의 비행 VFX 가림 프레임은 수락 대상에서 제외하고 원본을 보존했다. 기존 원화·물약 전체 캔버스의 66% 사용은 유지하며 다른 144개 원화를 전부 확인했다는 주장은 하지 않는다.
- 원 matrix 보고서와 한국어 단일 캡처 보고서의 **FAIL 상태는 수정하지 않았다.** 원인은 임시 GameView helper의 custom-size 삭제 인덱스 변환과 Unity 자동 잘림 뒤 전체 토큰 문자열 검사 두 가지였다. root는 원래 `Options review` **FixedResolution 2100×900**, 전체 preset **8개**, 선택 index **0**을 별도 증거로 복구했다. preference 파일 바이트 동일성을 별도 확인했으며 이 기록을 제품 슬롯 실패나 원 보고서 PASS로 바꾸지 않는다.

### 훈련 헤더·종료 처리·남은 검증

훈련 헤더는 한국어 1600×900에서 실제 `GetWorldCorners` 경계 3개와 EventSystem raycast, 버튼 간 비겹침을 통과했다. 콘텐츠 독을 실제로 열어 바로가기 **4개**의 안전 영역·raycast·하단 8슬롯 비겹침을 확인하고 닫았으며, 칙령 버튼→quick root 열기→배경 닫기도 확인했다. 설정 버튼은 raycast만 확인했고 옵션 창은 열지 않았다. root가 닫힘/rail 열림 **2장**을 직접 확인해 헤더 범위를 수락했다. 바로가기 최대 5개는 수학 검사 범위이고 실제 캡처는 4개다.

첫 헤더 시도는 15:00 UTC(한국 자정)에 기존 `hide_until`이 만료된 새날 출석 팝업 때문에 Training 진입 전 끝났고 PNG는 0장이었다. 그 FAIL 보고서는 보존했으며 제품 헤더 실패/통과로 집계하지 않는다. 후속 helper는 정확한 폐기용 fixture에서 공개 `SetHidden` 2호출로 출석 표시 설정만 처리하고, 설정 전후 계정 JSON 불변을 확인했다. 시계·출석 권리·보상은 변경하지 않았다. 후속 raw 결과는 `HEADER_SCOPED_PASS_VISUAL_REVIEW_PENDING` 그대로 보존하고 별도 root 화면 수락 근거를 기록했다.

소유 Editor **PID 53002는 graceful 종료·exit 0·PID 부재**를 확인했다. 원래 GameView preset **8개 / 선택 0 / Options review 2100×900**과 preference 파일 바이트 동일성을 확인했고, 폐기용 fixture **7파일을 정확히 복원**했다. 한정 검수용 helper 소스·메타 2개는 참조 0을 확인한 뒤 복구 가능한 증거 경로로 옮겼다. 실제 사용자 세이브 접근과 영구 삭제는 0이다.

전체 통합 **NUnit 0회 / native build 0회**다. 일반 quick-list/장착, 궁극기 지급·해금, 물약 소비·자동구매, 다른 자산 갤러리, 물리 마우스·터치와 전체 화면 회귀는 이 한정 검증에서 미실행이다. 슬롯 행 callback 결과를 이 기능의 통과로 확대하지 않는다. 최종 승인 범위 동결 뒤 전체 테스트 한 번과 재사용할 네이티브 빌드 한 개의 통합 검증은 남아 있다.

저장소의 한정 증거는 [receipt](CasualBattleHudEvidence/SlotGroups20261015/receipt.json), [한국어 세로 슬롯](CasualBattleHudEvidence/SlotGroups20261015/portrait-ko.png), [훈련 헤더](CasualBattleHudEvidence/SlotGroups20261015/training-header-ko.png)다. 원 raw 보고서·root 수락·종료/복원 근거는 receipt의 SHA와 Task 증거 경로로 추적한다. 현재 소스 UI 계약 **15/15**, refresh **14 bindings / 195 UI sources**가 통과했으며, 전체 NUnit/native 실행을 대신하지 않는다. 옵션 PR 138은 사용자 승인 전 인수·병합하지 않는다.


## 2026-10-15 최종 통합 후속 — 검사 범위와 세로 훈련 헤더

앞 절들은 당시 실행 범위의 기록으로 보존한다. 최종 통합에서 실행한 원본 `hud-battle-layout` 네이티브 검사는 훈련 640×360의 설정·콘텐츠·이벤트 크기가 다르다는 이유로 **FAIL**이며, 그 receipt와 24개 선행 PNG는 변경하지 않는다. 당시 구현은 위 후속 인계대로 기존 noncompact 가로 훈련에서 설정만 줄이고 콘텐츠·이벤트 크기는 유지했으므로, 무조건 같은 크기를 요구한 검사 범위가 잘못됐다.

`RuntimeBattleLayoutSmoke.CheckBattle`의 빌드 이후 QA 보정은 그 legacy noncompact 가로 분기에 정사각·양의 크기, 설정의 44 페이지 단위 하한 및 콘텐츠 크기 상한, 콘텐츠·이벤트 동일 크기를 검사한다. 나머지 분기의 동일 크기와 Casual 44 기준, 카메라·텍스트·안전 영역·가시성 검사는 유지한다. 이 QA 보정 자체는 제품 배치를 바꾸지 않으며, 원본 네이티브 FAIL을 PASS로 바꾸지 않는다.

그 뒤 한정 Editor 시도는 훈련 720×1280에서 40틱 비교와 실제 `CheckBattle` 1회를 통과한 후 콘텐츠 버튼과 고정 칙령 버튼의 터치 사각형 겹침으로 **FAIL**했다. 실제 화면 좌표는 콘텐츠 하단 `1132.05762`, 칙령 상단 `1132.71961`로 약 **0.662px** 겹쳤다. 서로 다른 두 overlay Canvas를 화면 픽셀로 변환한 결과이며, 두 중심 raycast가 각각 통과해도 가장자리 겹침은 남는다. 이 시도와 helper의 앞선 복구 실패 기록도 보존한다.

제품 보정은 `GameUI.BattleLayout`의 **nonCasual·noncompact** 경로에서 콘텐츠 독 상단을 기존 칙령 하단보다 **6 페이지 단위** 아래에 두는 기존 가로 규칙을 세로에도 적용한다. 설정 크기 제한은 가로에만 유지한다. 독·이벤트·설정 크기, 슬롯 그룹과 원화, `ContentDockView`의 펼침·접힘·간격·회전, 전투·저장 계산은 변경하지 않는다. 같은 legacy 세로 경로를 쓰는 훈련장·튜토리얼도 배치 영향 범위이며, 일반 Casual early return과 compact 경로는 유지한다.

제품 보정 후 집중 Editor 검증은 이 준비 기록에서 **NOT_RUN**이다. 계획은 폐기용 세이브의 훈련 2에서 720×1280·1280×720·640×360, 한국어/영어의 실제 버튼 경계·최상위 EventSystem raycast·독/칙령 열기·닫기 및 렌더한 실제 전투와 제어 복사본의 **120 동일 틱** 비교다. 특히 세로에서 펼친 독의 모든 실제 바로가기와 하단 물약·스킬 8슬롯 비겹침을 확인해야 한다. 이 실제 경로가 회귀 검증이며, 같은 수식만 복제한 단위 검사는 추가하지 않는다. 완료 여부·실행 조건·화면 수락은 `task-23/validation/cleanup-20261015/main-canonical-resources/new-art/final-two-native-followup/`의 후속 실제 receipt로 별도 확정한다.

전체 NUnit과 38개 네이티브 검사는 다시 실행하지 않고 추가 네이티브 빌드도 만들지 않는다. 기존 한 개 앱과 그 해시는 **이 제품 보정 이전 입력**의 증거로 보존한다. 따라서 이 후속 소스의 제품 배치가 같은 네이티브 앱에 포함됐다고 주장할 수 없으며, 제품 보정의 확인 범위는 후속 Editor 검증에 한정된다. 실제 사용자 세이브·원격 작업·물리 마우스/터치·모바일 기기·성능 검증으로 확대하지 않는다.


### 최종 통합 후속의 실제 집중 Editor 결과

위 **NOT_RUN** 문단은 제품 보정을 준비하던 시점의 기록이다. 후속 `training-evidence-4/result.json`은 `accepted=true`, 오류 없음, **120틱 / 실제 CheckBattle 5회 / 헤더 최상위 raycast 20개 / pointer 입력 8회 / PNG 9장**으로 완료됐다. 실제 조건은 **720×1280 한국어·영어, 1280×720 한국어, 640×360 한국어·영어**의 5개이며, 3개 화면에서 각각 40틱을 렌더한 실제 전투와 제어 복사본의 전체 RunState·Hero JSON으로 비교했다. 1280×720 영어까지 포함한 6조건 전수 통과로 확대하지 않는다.

720×1280의 한국어·영어와 640×360 영어에서 독을 실제 EventSystem 입력으로 열고 닫았다. 해당 fixture에서 실제 바로가기는 **4개**이며 모두 안전 영역·최상위 raycast·서로 비겹침 및 **활성 하단 슬롯** 비겹침을 통과했다. 640×360 영어에서는 칙령 버튼→quick root→가려지지 않은 backdrop의 실제 입력으로 닫기도 확인했다. 이 fixture의 legacy build는 궁극기 슬롯이 비활성이므로, 이 결과는 비활성 궁극기의 입력 시험이나 실제 5개 바로가기 시험을 포함하지 않는다. 세로 보정 후 헤더 4개 대상의 양의 터치 사각형 겹침은 5개 실제 조건에서 없었다.

임시 helper가 추가한 3개 프리셋은 제거됐고, 시작 당시 **9개 프리셋·선택 8·전체 label/identity**와 언어가 복구됐다. 이 9개에는 부모가 별도로 소유한 1440×810 프리셋이 포함된다. 원래 8개 프리셋의 최종 복구, Editor 정상 종료, 폐기용 디스크 fixture 복구와 최종 화면 수락은 부모의 별도 종료 근거로 확인해야 하며 이 coroutine 결과만으로 주장하지 않는다.

실제 helper SHA256은 `9567c6db4955892be0b2b631c8680d37c6f44c19652eca5266a01379a62e2471`, 결과 SHA256은 `61625ea065779db24dbb885969db91889490f85e784fc13dd806ab3f467e5510`이다. `training-evidence-4/`의 실제 PNG 9개는 결과의 파일 목록·규격·SHA와 일치한다. 원본 네이티브 FAIL 및 보정 전 시도들의 FAIL은 보존한다. 후속 실제 결과의 상태는 `FOCUSED_EDITOR_TRAINING_PASS_VISUAL_REVIEW_PENDING_ORIGINAL_NATIVE_FAIL_PRESERVED`이며, 전체 NUnit·38개 네이티브 재실행이나 추가 빌드의 통과가 아니다. 기존 앱은 여전히 이 제품 보정 이전 입력의 산출물이다.

부모의 최종 수락은 `final-two-native-followup/acceptance.json`(SHA256 `e321f6717ad600036ce41b0a2342b65ab80f76cd6b0538133e1b3b09e034fe65`)에 기록했다. 실제 대표 PNG 5장을 직접 검수했고 세로 간격은 기존 약 0.662px 겹침에서 약 5.333px 여유로 바뀌었다. 가로 두 해상도의 버튼 화면 사각형은 수정 전과 0.001px 미만 차이로 일치했다. `restoration.json`은 원래 프리셋 8개·선택 0·장면, ProjectSettings 28개 SHA 및 폐기용 기기 설정 3개 복구와 소유 Editor PID 33106의 정상 종료 exit 0을 확인한다. 진단 helper의 잘못된 프리셋 인덱스 오류 3개는 원본 콘솔에 보존했고 정확한 소유 프리셋 복구로 해결했다. 제품 수정 이후 콘솔 오류는 없었다. 실제 사용자 세이브·영구삭제·원격 작업은 0이며, 이 후속은 성능이나 실제 모바일 입력 검증을 포함하지 않는다. 앞 절의 옵션 인수 보류는 당시 기록이며 옵션 PR 138은 사용자 승인 후 로컬 main에 통합됐다.
