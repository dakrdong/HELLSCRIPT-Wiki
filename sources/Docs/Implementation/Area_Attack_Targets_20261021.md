# 범위 공격 대상 상한 점검·수정 (2026-10-21)

[English](Area_Attack_Targets_20261021.en.md)

## 작업 경계

`codex/aoe-all-targets-20261021`, `.claude/worktrees/aoe-all-targets-20261021`에서 작업했다. 최종 후보의 기준은 최신 원격 main `a4b40e4661333611ee4a28251a959873893f3756`이다. 최초 구현은 Claude 인계 `907c8bf82bffd970943321aafaeb25d5f779ee95` 위에서 했지만, 사용자 커밋·리뷰·병합 승인 후 광역 수정 커밋만 main 위로 옮겼다. 따라서 미병합 마을·대장간·UI 작업 55파일을 함께 들여오지 않는다. 광역 범위인 W09의 Take 상한 제거 한 줄만 별도로 포함했고, 새 범위 테스트가 기본·회수·회전·단일 대상 변형을 검증한다. 기존 `GuideTutorialTests`는 main 그대로이며 Claude 브랜치의 원본 수정·테스트도 손대지 않았다.

이번 광역 PR의 단일 담당은 Codex `01a15c22-3696-720e-b18b-9cc46004b021`이다. 작업판을 읽기 전용으로 확인했을 때 자원 예약·실행 중 명령·미완료 쓰기는 없었다. 기존 UI·튜토리얼 통합 담당의 역할과 기록은 유지한다. 공용 main의 UI 변경과 Claude 두 작업 폴더, 사용자 세이브·테스트본을 편집하지 않는다. 커밋·push·PR·공식 리뷰·조건 충족 후 merge commit 병합은 이번 추가 승인 범위이며, 실제 외부 결과는 이 작업의 `Builds/Validation/AreaTargets20261021/integration-status.json`과 PR에 남긴다.

추가 사용자 승인으로 기존 23파일에 `CasualNoticeInfo.cs`의 SVG 설명 주석 한 줄만 더했다. 좌표·길이·획 두께와 실행 코드는 그대로 두고 폐기 스킬 ID로 오인되던 경로 명령 표기를 풀어 썼다. 필수 `Class skill data`와 `Shared UI contract` 워크플로만 재활성화했으며 실행 정의·권한·인증·보안 설정은 바꾸지 않았다.

이후 사용자는 모든 CI를 Codex의 로컬 동등 검증으로 수행하고 GitHub 유료 기능을 피하도록 운영 규칙 변경을 요청했다. `AGENTS.md`, `AgentRules/Workflow.md/.en.md`, 한·영 공용 `AgentRules/Agent_Collaboration.md`에 이 기준과 실제 원격 보호 체크의 별도 유지 경계를 반영했다. 앞서 활성화한 두 자동 워크플로는 추가 유료 실행을 피하도록 범위·영향을 보고한 뒤 다시 비활성화했다. 검사 정의·보호·권한·결제·한도는 바꾸지 않았다.

## 분류와 변경

| 경로 | 처리 |
| --- | --- |
| W18 기본·확장 일격 | 원 안 모든 생존 적에게 피해·기절 |
| W02-2·W03-3·W04-3 여진, W04-2 직선 파동 | 범위 대상 5명 제한 제거. W02-1 돌진 경로의 임의 100명 검색 상한도 제거 |
| W05 자세 반격·가시 반사, W17 복사 공격·복사 반격/반사 | 원/부채꼴/복사 형상의 5명 제한 제거. 일반 선조의 단일 대상 선택은 유지 |
| A04-1 미끼·A14-2 폭발 화살·A18-2 잔상·A06-1 처치 폭발 | 폭발 안 모두 적중. 일반 A14의 분기 개수는 유지 |
| A17 기본·이동·집중 포화 | 매 파동 8명 제한 제거. 파동 수·집중 반경은 유지 |
| M01 화염구·유성·분열 화염구의 최초 폭발과 관통 화염구의 연소 궤적 | 원/경로 장판 5명 제한 제거. 분열 투사체 수·관통 제한은 유지 |
| M04 출발 폭발·도착 기절·보호막 반응, M06-1 후속 노바 | 범위 피해·기절·연소·속박 상한 제거. 도착 기절에 새 피해는 추가하지 않음 |
| M08 장벽·고리·전진·십자 | 틱당 5명 제한 제거. 기존 사각형·원·십자와 시야 판정 유지 |
| M11 구체 기본·이동·3분할의 틱과 모든 만료 폭발, M17 원소 붕괴 | 3/5/8명 제한 제거. 구체 인스턴스 수·파동/원소 순서 유지 |
| WP20·MP20·MP21, M18 삼원소 추가 폭발, DES_LM42 | 광역 패시브·연계 폭발의 5명 제한 제거 |
| REF_SW06_B5·REF_SM05_B5·REF_SM08_B6·REF_SA08_B6 | 공통 Splash를 통해 폭발 안 모두 적중 |
| REF_SA06_B5 | 공통 Nearby의 숨은 기본 5명 제한을 없애 범위 피해·기절·대상 기록에 전원 포함 |
| 전설 범위 피해·제어 18종 | 반경이 있는 LegendaryTargets의 5명 제한 제거. 시야·거리·동률 ID 순서와 반경 0의 단일 대상은 유지 |

`Nearby`와 `Line`은 개수를 생략하면 전원을 반환한다. 개수를 명시한 관통·선택 호출은 그 개수대로 처리한다. `Splash`의 max 인자는 제거했다. 범위 밖·사망·벽 뒤 대상은 기존 필터가 제외한다.

## 유지한 설계 제한과 예보

- W09-2 깊은 상처는 한 대상이다. W08-2 관통 갈고리는 3→7명, A07-2 흩뿌림은 3→5명이다. 근거는 전투 명세의 해당 보조 스킬 행이다.
- A01은 화살별 5명(+룬 추가 대상), 세 갈래는 독립된 세 발이다. A02의 산탄/탄막 발수, A14의 일반 분기 2명(장비 조건 시 3명), M03의 연쇄/되튀김 횟수, M07의 3명 관통, 귀환 서리의 구간별 5명, 작은 분열 화염구의 관통 5명, LA04 전이 2명과 룬의 projectile.maxHits는 유지했다.
- 관통 화염구(M01-3)는 기존 예보의 총 5명과 달리 실제 코드가 틱마다 5명씩 다시 선택했다. 비행 누적 5명으로 맞추고 예보와 같은 선분 충돌 폭·순서를 재사용한다. 뒤에 남는 연소 장판은 인원 상한이 없다.
- D4 A01 예보가 레거시 패시브 때문에 7명을 예측하던 경우를 실제 D4의 화살별 5명과 일치시켰다. 레거시 경로는 기존 패시브 한도를 유지한다. M07 정책 대상 수도 실제 3명 관통과 일치시켰다. W04 파동은 예보와 실제가 공유하는 형상 함수에서 상한을 제거했다.

## 데이터·문구

원본 `current_runtime_parameters.json`과 생성된 `ClassSkills.json`에서 W09, W09-1, W05-1, W02-1, W04-1/2, W18, A04-1, A03-1/2, M08, M11, M17, M18의 사용하지 않는 targets 상한을 제거했다. M03-3은 현재 연쇄 뒤 당김이며 옛 지대의 targets=5가 쓰이지 않아 이 값도 제거했다. 단일 대상과 관통·분기 데이터는 유지했다. 역사적 `LegacySkillWire.json`은 바꾸지 않았다.

W09 연계 장갑, 냉기 연계 장신구, 후퇴 잔영 세트와 전설 18종의 한국어/영어 설명에서 광역 5명 제한을 제거했다. 전설 목록과 전투 명세도 두 언어로 갱신했다. 전설 목록 생성기가 동적 wiki 모듈을 sys.modules에 등록하지 않아 실패하던 한 줄도 고쳐 이 목록을 재생성·검사했다. 별도 위키 검증·게시의 실제 상태는 PR과 작업 소유 검증 기록에서 확인한다.

## 미확정·보존

`TickClassEffect`의 잔존 kind="shape"는 A01이면 관통 5명, 그 외에는 60도 형상 5명이다. 현재 소스에서 이를 새로 만드는 경로를 찾지 못했으며 A02 산탄 메아리인지 순수 부채꼴인지 활성 설계 근거가 부족하다. 이 분기는 그대로 보존했다. 호환 효과로 다시 쓰려면 설계 소유자가 의미를 확정해야 한다. 실제로 현재 생성되는 REF_SA08_B6는 이 분기가 아니라 위치 고정 burst이며 인원 상한을 제거했다.

M01-3의 숫자 5는 기존 예보/타격 코드에서 보존한 수치다. 명세에는 관통 형태만 있고 이 숫자의 랭크 설계는 명시되지 않아 별도 조정 의도가 있다면 확인해야 한다. 임의로 무제한 관통으로 바꾸지 않았다.

## 검증 결과와 한계

- 새 범위/경계/기본·변형/유지 제한 회귀: **79 통과, 0 실패**. 주로 범위 안 10명+경계 1명, 바로 밖 대상, 사망 대상과 벽을 검사한다. 전설 18종은 하나의 테스트 안에서 모두 순회한다.
- 기존 직접 영향 스킬 회귀: **44 통과, 0 실패**. W09 출혈/회수, 관통 갈고리, 반격/반사, 화염구·눈보라·연쇄·노바·순간이동·장벽·구체·붕괴·현자, 흩뿌림·사격 예보·미끼·포화·잔상 쌍을 검사했다.
- Python ClassSkills 회귀 **5/5**, D4 데이터 회귀 **3/3**. ClassSkills·D4 생성 데이터 및 전설 목록 check 모두 통과했다. 보존 수치 시험은 광역 targets 제거만 예외로 인정하고 나머지 원본 수치·역사적 wire 도메인을 계속 비교한다.
- 위 C# 79+44개와 Python 5+3개는 마을 선행 변경을 제외한 main 기준에서 다시 통과했다. 필수 CI의 legacy-ID 검사 회귀 18개도 통과했다. 실제 inventory check의 기존 main SVG 주석 오탐은 사용자 승인된 주석 한 줄 수정 후 **통과**했다(raw=421, actual-use=0, verification-contract=22, retirement-record=399). 검사 기준과 UI 실행 코드는 바꾸지 않았다. 후속 변경은 주석·기록뿐이므로 기존 Core 검증 근거를 재사용하며, 새 최종 SHA의 공식 리뷰와 필수 원격 CI 결과는 PR에서 확인한다.
- C#은 **실제 Core 소스**, .NET SDK 10.0.401·C# 9·설치된 Unity NUnit assertion으로 실행했다. Unity 수학·Resources·JsonUtility·프로파일 표식을 테스트용 대체 구현으로 연결했다. 이 결과는 Unity Editor 컴파일/Console, Unity Edit Mode나 플레이어 실행 근거가 아니다.
- 실제 Unity 6000.6.0f1 EditMode에서 명시 필터의 55개 메서드만 실행해 **123 통과, 0 실패, 0 건너뜀**을 확인했다(신규 79+기존 44, 테스트 133.36초). 설치된 Unity CLI, 전용 개발 worktree와 작업 소유 저장 경로를 사용했다. 사용자 확인으로 다른 Unity 테스트·빌드가 없음을 확인한 뒤 한 개만 실행했으며, 앞서 권한 거부된 `ps` 조회는 재시도·우회하지 않았다. C# 컴파일 오류는 없었지만 첫 임포트에서 Unity Data Store의 `Failed to free block` 오류 한 건이 기록되어 한계로 보존한다. 이 실행의 자동 생성 파일 5개만 보관 후 원복했고 ProjectSettings 두 파일은 변경되지 않았다. 전체 Edit Mode·Play Mode·실기기·성능 검증은 하지 않았으며 사용자 세이브와 테스트본은 읽거나 쓰지 않았다.

## 로컬 CI 대응

| 기존 워크플로 | 로컬 동등 검사 | 근거 |
| --- | --- | --- |
| Class skill data | D4/ClassSkills 생성 check, Python 회귀 3+5개, legacy-ID 회귀 18개와 실검사, `git diff --exit-code` | 단계별 명령·종료 코드·로그·대상 SHA를 작업 소유 CI 기록에 보존 |
| Shared UI contract | `tools/check_ui_contract.py --verify --receipt ...`의 15개 검사 | 작업 소유 receipt와 실행 로그 |

초기 로컬 8단계는 macOS/Python 3.9.6/Node 26.8.2에서 모두 통과했다. 정의된 버전에 맞춘 검증에는 기존 Python 3.12.13과 작업 전용 Node 22.23.3을 사용한다. Node는 공식 배포본의 SHA256을 확인했으며 전역 설치·설정은 바꾸지 않았다. macOS에서 실행하므로 Ubuntu runner 환경 자체의 검증으로 보고하지 않는다. 최종 SHA, 실제 실행·재사용 결과와 로그는 `Builds/Validation/AreaTargets20261021/`의 CI 기록과 PR에 남긴다. 결제·지출 한도로 시작되지 못한 GitHub 실행은 성공으로 바꾸지 않으며 실제 원격 보호 게이트가 있으면 별도로 보고한다.

검증의 입력 SHA256·결과·미실행 목록은 작업 폴더 `Builds/Validation/AreaTargets20261021/verification.json`, main 기준 재검증의 C# 세부 결과는 그 아래 `headless/isolated-area.json`, `headless/isolated-existing.json`에 있다. 최초 Claude 기준의 `area-final.json`, `existing-regressions.json`도 이력으로 보존한다. 최종 후보의 실제 영향 영역 Unity 검사·공식 리뷰·필수 CI가 모두 충족되어야 병합하며, 전체 Edit Mode는 별도 명시 허가가 필요하다.

## 변경 파일

- `Assets/HELLSCRIPT/Resources/Data/ClassSkills.json`
- `Assets/HELLSCRIPT/Resources/Data/LegendaryPowers.json`
- `Assets/HELLSCRIPT/Runtime/Core/CombatSimulation.ClassSkillEffects.cs`
- `Assets/HELLSCRIPT/Runtime/Core/CombatSimulation.ClassSkillPassives.cs`
- `Assets/HELLSCRIPT/Runtime/Core/CombatSimulation.ClassSkillPolicies.cs`
- `Assets/HELLSCRIPT/Runtime/Core/CombatSimulation.ClassSkills.cs`
- `Assets/HELLSCRIPT/Runtime/Core/CombatSimulation.Forecast.cs`
- `Assets/HELLSCRIPT/Runtime/Core/CombatSimulation.Legendaries.cs`
- `Assets/HELLSCRIPT/Runtime/Core/SkillTreeD4/CombatSimulation.D4Mage.cs`
- `Assets/HELLSCRIPT/Runtime/Core/SkillTreeD4/CombatSimulation.D4Ranger.cs`
- `Assets/HELLSCRIPT/Runtime/Core/SkillTreeD4/CombatSimulation.D4Warrior.cs`
- `Assets/HELLSCRIPT/Runtime/Presentation/CasualNoticeInfo.cs` (승인된 주석 한 줄)
- `AGENTS.md`
- `AgentRules/Workflow.md`
- `AgentRules/Workflow.en.md`
- `AgentRules/Agent_Collaboration.md`
- `Docs/Design/ClassSkills/current_runtime_parameters.json`
- `Docs/Design/D4SkillTree/skill_tree_d4.json`
- `Docs/Design/HELLSCRIPT_Legendary_Class_Expansion.en.md`
- `Docs/Design/HELLSCRIPT_Legendary_Class_Expansion.md`
- `Docs/Implementation/D4_Skill_Tree_Combat_Spec_20261012.en.md`
- `Docs/Implementation/D4_Skill_Tree_Combat_Spec_20261012.md`
- `tools/legendary_catalog.py`
- `tools/test_class_skill_runtime.py`
- `Assets/HELLSCRIPT/Tests/Editor/D4/SkillTreeD4CombatCoreTests.AreaTargets.cs`
- `Assets/HELLSCRIPT/Tests/Editor/D4/SkillTreeD4CombatCoreTests.AreaTargets.cs.meta`

이 기록과 영문본도 이번 작업에 추가했다. 패키지·씬·프리팹·프로젝트 설정은 바꾸지 않았다.
