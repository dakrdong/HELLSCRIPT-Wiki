# HELLSCRIPT 남은 개발 작업표

갱신일: 2026-09-13

**2026-09-13 균열 출구 선택 수정:** 현재 방의 유효 출구를 먼저 고르고, 성소의 실제 사용 거리 안에 들어온 뒤 사용하도록 수정했습니다. 균열 대조 360판에서 기존 규칙 169/180판, 핵심 기본값 173/180판이 클리어했고 성소 왕복 4판이 해결됐습니다. 룬 완료본을 포함한 최신 메인의 전체 검사 2,120개, 대표 대조 24판, 실제 앱 실행·재시작 3회와 화면 5장 검토도 마쳤습니다. 보스에게 얻은 룬 4개까지 재시작 후 중복 없이 유지됩니다. 자연 성장·파밍·남은 밸런스·모바일 실기기 검증은 후속 작업입니다. [개발 기록](../Implementation/Edict_Field_Expansion.md)

English: current-room exit ordering and shrine use range are corrected. The controlled 360-run comparison cleared legacy 169/180 and core defaults 173/180, resolving four shrine loops. The Rune-integrated main passed all 2,120 tests, 24 representative comparisons, three native launches/restarts and five screenshot reviews. Four earned boss runes also persist without duplication. Natural growth/farming, remaining balance and physical devices remain. [English record](../Implementation/Edict_Field_Expansion.en.md)

**2026-09-13 궁수 핵심 옵션:** 액티브 6종과 기본 사격의 핵심 옵션 30개를 독립 판단에 연결했습니다. 전체 검사 2,079개, 궁수 전용 98개와 실제 앱 실행·재시작 13회가 통과했고 화면 31장을 검토했습니다. 덫·투사체·후퇴 후 보행·표식·실제 적중으로 얻는 할인과 저장 복원을 확인했습니다. 생성 균열의 장기 생존·성장·파밍 효용과 모바일 실기기 검증은 남아 있습니다. [개발 기록](../Implementation/Edict_Ranger_Expansion.md)

English: all 30 Ranger active/BASIC core options now use independent policies. All 2,079 tests, 98 Ranger-specific cases and 13 native launches/restarts passed; 31 screenshots were reviewed. Actual traps, projectiles, follow-up walking, Mark, earned hit discounts and persistence were verified. Long-run generated-rift survival/growth/farming and mobile-device validation remain. [English record](../Implementation/Edict_Ranger_Expansion.en.md)

**2026-09-13 전사 방어·함성·기본 공격:** 철벽·전투 함성·근접 기본 공격의 나머지 13개 옵션을 연결했습니다. 전사 핵심 옵션 30개가 모두 독립 판단을 사용합니다. 전체 검사 1,981개와 최종 앱 실행·재시작 5회가 통과했고 화면 17장을 검토했습니다. 실제 보호막·자원 회복·3회 적중으로 얻은 절제 할인과 저장 복원을 확인했습니다. `main`만 유지하며, 궁수 정책과 생성 균열의 장기 밸런스·모바일 실기기 검증은 남아 있습니다. [개발 기록](../Implementation/Edict_Warrior_Support_Expansion.md)

English: all 13 remaining W05/W06/Warrior BASIC options are connected, completing 30 Warrior core options. All 1,981 tests and five final native processes passed; 17 screenshots were reviewed. Actual shields, resource recovery, earned three-hit LC02 discounts and persistence were verified. Only `main` remains. Ranger policies, long-run generated-rift balance and mobile-device validation remain. [English record](../Implementation/Edict_Warrior_Support_Expansion.en.md)

**2026-09-13 지면 강타와 브랜치 정리:** 지면 강타의 핵심 옵션 4개를 일반 전투와 지정 생존 대응에 연결했습니다. 실제 기절·보스 제압·시전 중단·저장 복원을 검증했고, 전체 검사 1,903개와 최종 앱의 실행·재시작 3회가 통과했습니다. 한국어·영어 화면 13장을 확인했습니다. 클로드의 통합 기록을 유지하면서 병합이 끝난 작업 브랜치를 로컬·원격에서 삭제했고, 현재 작업 위치는 `main`입니다. 철벽·함성·전사 기본 공격과 다른 직업의 남은 정책은 후속 작업입니다. [개발 기록](../Implementation/Edict_Slam_Expansion.md)

English: all four W04 options now independently drive ordinary combat and designated survival control. All 1,903 tests and three final native processes passed; 13 KO/EN screenshots were reviewed. The fully merged development branch was removed locally and remotely, Claude’s integration remains on main, and the checkout is now `main`. W05/W06, Warrior BASIC and remaining class policies still require work. [English record](../Implementation/Edict_Slam_Expansion.en.md)

**2026-09-13 전사 공격 사냥 칙령:** 회오리·도약·분쇄의 13개 핵심 옵션을 독립 판단에 연결했습니다. 실제 틱 비용·보행 충전·이륙 예약·착지 후 한 번의 공격 선호와 저장 복원을 검증했습니다. 전체 검사 1,847개와 실제 앱의 11개 프로세스가 통과했습니다. 24판 대조에서 충전 연계가 발생했지만 클리어 수는 같고 기본값의 사망은 늘어, 밸런스 개선으로 판단하지 않습니다. 전사 나머지 스킬·기본 공격과 다른 직업의 후속 연결은 남아 있습니다. [전사 공격 개발 기록](../Implementation/Edict_Warrior_Attacks_Expansion.md)에 범위를 기록했습니다.

English: W01-W03 independently apply all 13 core options, actual tick costs, walking charges, takeoff reservation and a persistent one-attack landing preference. All 1,847 tests and 11 native processes passed. The 24-run comparison observed charge linking, unchanged clear counts and an additional default-mode death; no balance improvement is claimed. Remaining Warrior skills/BASIC and other class policies remain. See the [English record](../Implementation/Edict_Warrior_Attacks_Expansion.en.md).

갱신일: 2026-09-13 · 기준: D3 장비 통합·D4 성장·첫 안내·훈련 개발 중 · 능력치 시트 반영

이 문서는 지금부터 이어서 만들고 검증할 내용을 찾는 입구다. 전체 정책은 [후속 개발 기획](HELLSCRIPT_Development_Expansion_Plan.md), 수치·효과·경제 규칙은 각 담당 상세안을 따른다. 개발 기록에 있는 개별 통과 결과를 전체 게임의 완성으로 확대하지 않는다.

접두사·접미사 장비 생성, 6세트 정의, 자동 생성 균열, 상자·성소·저주 상자의 기반은 이미 구현되어 있다. 남은 핵심은 이 요소들이 **빌드 선택 → 탐험 → 장비 발견 → 비교·수정 → 재도전**으로 이어지는지 검증하고, 마을·성장·분석·온라인·기기 품질의 공백을 채우는 일이다.

**2026-09-09 화면 대응 추가 기획:** 앞으로 만드는 UI는 모바일 가로·세로를 함께 설계하고, PC는 모바일 가로 구성을 창 크기에 맞게 확장한다. 방향 선택·기기별 설정·편집 및 전투 상태 보존·환경별 완료 기준은 [화면 비율·방향 대응 상세안](HELLSCRIPT_Screen_Layout_Detail.md)을 따른다. 프리셋 대화창부터 배치 대응을 시작했으며, 공통 화면·모바일 방향 설정·전투 HUD와 실제 기기 검증은 남아 있다. 적용 범위는 [화면 배치 개발 기록](../Implementation/Screen_Layout_Expansion.md)을 따른다.

## 1. 전설·세트와 빌드 완성도

**2026-09-09 배속 정책 변경:** 현재 사용자 제공은 1배속이며, 1.5배속·2배속 잠금과 기존 고배속 저장값의 안전한 복원이 후속 구현 항목이다. 향후 월 구독의 1.5배속과 이벤트의 누적 시간 사용권은 [배속 잠금·사용권 정책](HELLSCRIPT_Speed_Access_Detail.md)을 따른다. 구독·쿠폰 기능 도입은 나중 BM 설계 범위이며, 아래 세 배속 검사는 내부 검증 권한으로 유지한다. [절전 방치 모드](HELLSCRIPT_Idle_Mode_Detail.md)에서도 같은 잠금·차감 기준을 적용한다.

담당 명세: [장비 상세안](HELLSCRIPT_Itemization_Detail.md), [능력치 체계 상세안](HELLSCRIPT_Attribute_System_Detail.md), [빌드 통합 검사 기준](HELLSCRIPT_Build_Integration_Detail.md). 다음 개발의 우선순위가 가장 높다.

| 작업 | 플레이어에게 필요한 동작 | 개발·완료 기준 |
|---|---|---|
| 전설 15종의 조합 | 설명에 적힌 발동 조건을 만족하면 효과가 발생하고, 작동하지 않으면 이유를 확인한다. | LW01의 간격·몸체·벽·실제 재시작과 LW02·LW03의 장착 경계 검사는 [전사 전설 기록](../Implementation/Legendary_Expansion.md)에 정리했다. 남은 LW04·LC03·LM03과 공용 전설의 실제 장착·피해·물약·복원 연결을 이어서 검사한다. |
| 세트 6종의 선택 | 2세트+부위 전설과 4세트 구성을 비교한다. | 4세트와 같은 손·발 전설을 동시에 장착하는 잘못된 구성은 허용하지 않는다. 교체 시 사라지는 충전·노출·추가타를 설명한다. |
| 6빌드의 역할 | 같은 적을 상대해도 회오리 유지, 도약 진입, 관통 사선, 덫 유도, 장판 연계, 연쇄 제어가 행동으로 구별된다. | 사용·중단·피해·이동·자원 부족을 비교한다. 단독 보스에게 연계가 아예 시작되지 않는 규칙 오류와 의도한 단일 화력 약점을 구분한다. |
| 균열 비교 표본 | 특정 지형 하나만 잘 도는 빌드가 되지 않게 한다. | 기존 기획의 빌드당 30시드 시작 표본을 사용한다. 단계·레벨·접사·강화·상자 정책을 고정해 기록하고, 사망·시간 초과·길 막힘을 따로 집계한다. |
| 20단계 성능 재검증 | 장비를 갖춘 캐릭터의 승리·사망·시간 초과 원인을 구분한다. | 과거 180판 중 승리 12→6회는 당시 보고서의 기록이며 현재 지도·해금 규칙과 다르다. 현재 전투 규칙을 고정하고 접사 54종과 비교용 24종의 지도·적·장착 베이스·세트가 같은지 검사하는 도구를 추가했다. 각 600판 확대 실행을 완료했다. 현재 54종은 승리 188·사망 45·시간 초과 367회, 비교용 24종은 212·47·341회였다. 미완료·길 막힘은 없었다. 두 체계에서 모두 낮은 연쇄 제어를 선택한 16판으로 후속 진단했다. 자원 부족 차단은 없었고 긴 보스 도달 시간·다른 행동·시전 중단을 확인했다. 전후 32판 후속 진단에서 옛 M04·M05 규칙의 공격 중단을 확인했다. M04 연결 후 기본 칙령의 중단은 78→18회로 줄었지만 사망은 4→7회로 늘었으며, 해당 문서의 전역 생존은 꺼져 있었다. M05 연결과 생존 설정을 구분한 전후 48판 비교도 완료했다. 생존 설정 표본의 중단은 34→8회로 줄었지만 전후 모두 8판이 시간 초과했고 보스 등장 판도 6→5개로 줄었다. M06 연결 후 같은 생존 설정의 전후 48판에서 중단은 8→68회로 늘고 보스 등장 판은 5→3개로 줄었다. M06의 실제 제어 전환과 공격 준비 취소 비용, 보스 도달 과정·성장·파밍 효용을 이어서 조사한다. 최종 성능 판단은 [접사 검증 기록](../Implementation/Affix_Audit_Expansion.md)의 완료 결과를 따른다. |
| 접사 희석 확인 | 장비를 모으는 동안 빌드에 필요한 옵션을 얻을 수 있다. | 54종·24종 각각 96개 생성 조합에 10만 개씩, 총 1,920만 개를 검사했다. 기존 옵션의 출현율 감소를 확인했고 추첨 자체는 예상 분포와 맞았다. 전투 비교와 성장·파밍 효용을 함께 검토한 뒤 가중치·허용 부위 조정을 결정한다. 새 접사를 일괄 하향하지 않았다. |
| 배속과 편집·복원 | 1배·1.5배·2배에서 같은 전투 규칙을 유지하고, 편집·재시작이 효과를 중복 지급하지 않는다. | 같은 틱 기준 비교와 실제 앱 재시작을 구분해 증거를 남긴다. 기존 사용자 규칙을 새 추천안으로 자동 교체하지 않는다. |

능력치 시트에서 아직 정하지 않은 것이 셋 있다. **최대 기력과 기력 회복**은 디아블로4 본편 캐릭터 시트에 없는 줄이고 이 게임에 소모할 곳도 없으므로, 남길지 뺄지 정해야 한다. **군중 제어 지속시간 감소**는 영웅이 군중 제어를 당하지 않는 한 계속 빈 줄로 남는다. **적에게 같은 시트를 줄지**도 정하지 않았다. 지금 적의 방어력은 단계에서 나오는 한 값이다.

희귀 장비군과 세트 장비군은 접사도 다르므로, 둘의 차이를 세트 효과 하나의 배율로 해석하지 않는다. 희소 전설 하나의 획득 여부가 빌드 기본 작동을 잠그는지도 별도로 검사한다.

## 2. 성장과 첫 플레이 안내

담당 명세: [게임 완성 체크리스트 2장](HELLSCRIPT_Gameplay_Completion_Checklist.md#2-첫-15분과-여섯-빌드의-성장), [성장 상세안](HELLSCRIPT_Growth_Detail.md). 레벨업·저레벨 추천안의 구현과 검증은 [성장 개발 기록](../Implementation/Growth_Expansion.md)을 따른다.

단계별 안내와 기록의 구체적인 경계는 [첫 플레이 상세안](HELLSCRIPT_First_Play_Detail.md), 구현·검증은 [첫 플레이 개발 기록](../Implementation/First_Play_Expansion.md)을 따른다. 안내 구현은 검사 542개와 최종 macOS 앱의 자연 전투·소유 장비 비교·직접 수정·훈련·재도전·실제 재시작으로 확인했다. 신규 사용자의 첫 15분 관찰과 성장 구간별 비교는 남아 있다.

| 작업 | 구체적인 흐름 | 개발·완료 기준 |
|---|---|---|
| 새 계정의 첫 균열 | 직업을 선택하고, 자동 전투와 현재 행동의 이유를 확인한 뒤 첫 결과로 돌아온다. | 실제 레벨·해금 상태로 진행하며 레벨 30 시험 영웅을 일반 플레이에 사용하지 않는다. |
| 첫 장비 비교 | 새 장비의 부위·요구 레벨·접사·효과를 현재 장비와 비교한다. | 무조건 장착을 요구하지 않는다. 드롭이 없을 때는 보상을 위조하지 않는 비교 안내로 이어 간다. |
| 첫 규칙 수정 | 사거리·HP 조건 등 이해하기 쉬운 조건 하나를 바꾸고 훈련·재도전에서 차이를 확인한다. | 안내를 건너뛰거나 다시 열 수 있으며, 설명 때문에 저장된 규칙을 몰래 수정하지 않는다. |
| 전투 중 레벨업 | 획득 XP와 레벨업 뒤 능력치·해금 표시가 실제 전투 상태와 맞는다. | XP 정산, HP 비율 유지, 자원·진행 중 효과 보존과 성장 화면을 연결했다. 실제 생성 균열 처치와 재시작 복원을 검사했다. 성장 구간별 장기 비교는 남아 있다. |
| 빌드별 성장 안내 | 아직 배우지 않은 핵심 스킬 대신 현재 사용할 수 있는 행동을 안내한다. | 현재 레벨 추천안·미리보기·명시적 적용과 PM01의 저레벨 공백을 보완했다. 6빌드 × 7개 해금 경계를 검사했다. 여러 시드에서의 실제 성장·첫 이용 안내는 이어서 검증한다. |

## 3. 장비 정리와 빌드 저장

담당 명세: [장비 상세안](HELLSCRIPT_Itemization_Detail.md), 체크리스트 CHK-U03~U06.

**2026-09-12 후속 구현:** 화염구 M01의 자동 사용·용도·대상·관측 이동 조준을 이전 규칙 조건과 분리해 전투에 연결했다. 전체 검사 1,461개와 macOS 실행·재시작이 통과했다. 세부 범위는 [화염구 개발 기록](../Implementation/Edict_Fireball_Expansion.md)을 따른다.

**2026-09-13 후속 구현:** M02의 자동 사용·설치 목적·위치·기존 장판 보존/교체·LM01 추적 선택을 전투에 연결했다. 전체 검사 1,514개와 macOS 실행·재시작이 통과했다. 당시 다음 대상으로 정한 M03은 아래 후속 단계에서 연결했다. 나머지 조준 스킬도 각자 기존 조건 의존 여부를 확인한다. [눈보라 개발 기록](../Implementation/Edict_Blizzard_Expansion.md) · [English](../Implementation/Edict_Blizzard_Expansion.en.md)

**2026-09-13 연쇄 번개 후속 구현:** M03의 네 옵션을 독립 판단으로 연결하고 준비 완료 시 첫 대상의 사거리·시야·생존을 다시 검사한다. 충전 소비와 예약 보너스·첫 대상의 저장 복원, 전체 검사 1,566개와 macOS 실행·재시작을 검증했다. 선택한 16판 진단은 양쪽 모두 클리어 0·사망 4·시간 초과 4회였다. 자원 부족 차단은 없었으며, 사냥 칙령의 시전 중단이 29→78회로 늘어 생존 대응·준비 취소를 후속 조사한다. 무작위 승률 표본은 아니다. 당시 다음 대상으로 정한 M04와 중단 사유는 아래 후속 단계에서 연결했다. [연쇄 번개 개발 기록](../Implementation/Edict_Chain_Expansion.md) · [English](../Implementation/Edict_Chain_Expansion.en.md)

**2026-09-13 순간이동 후속 구현:** M04의 네 옵션과 준비 완료 시 실제 착지 검사를 연결했다. 일반 거리 조정은 준비 중 공격을 취소하지 않으며, 전역 지정·대응 조건을 켠 생존 경로와 적 없는 환경 위험을 별도로 처리한다. 전체 검사 1,601개와 macOS 설정 저장·재시작이 통과했다. 전후 32판에서 기본 칙령의 M03 중단은 78→18회로 줄었지만 사망은 4→7회로 늘었다. 새 기본 문서는 이전 규칙의 생존 조건을 옮기지 않아 M04 사용이 0회였으므로 성능 개선으로 판단하지 않는다. 당시 후속 대상으로 정한 M05와 생존 설정 비교는 아래 단계에서 완료했다. 기존 사용자 설정은 자동 교체하지 않는다. [순간이동 개발 기록](../Implementation/Edict_Teleport_Expansion.md) · [English](../Implementation/Edict_Teleport_Expansion.en.md)

**2026-09-13 원소 보호막 후속 구현:** M05의 네 옵션을 독립 판단으로 연결하고, 교전 시작을 한 번 검토한 기록과 실제 흡수·LM03 지급의 저장 복원을 검증했다. 비용 0·쿨타임 14초·전체 상한과 생성 당시 HP 기준을 유지한다. 전체 검사 1,652개와 실제 앱의 세 프로세스 검사가 통과했다. 전후 48판에서 생존 설정 문서는 사망 없이 모두 시간 초과했으며 성능 개선으로 판단하지 않는다. 당시 후속 대상으로 정한 M06의 목적·위치·보스 정책은 아래 단계에서 연결했으며, 고정한 생존 설정의 보스 도달 전 시간과 불발도 비교했다. [원소 보호막 개발 기록](../Implementation/Edict_Shield_Expansion.md) · [English](../Implementation/Edict_Shield_Expansion.en.md)

English: M05 policies, persistent encounter openings and actual LM03 absorption/reward restoration passed all 1,652 tests and three native processes. The 48-run comparison separated default and explicit survival settings; all configured cases timed out despite avoiding death. The subsequent M06 phase below connects its purpose, positioning and boss policies and compares pre-boss time and misses under the fixed survival configuration.

**2026-09-13 서리 폭발 후속 구현:** M06의 네 옵션과 실제 자기 중심 3m, 추적 한도를 지키는 보행 접근, 일반 빙결·보스 제압·SMB 충전을 연결했다. 준비 중 설정 변경과 두 차례 독립 재시작에서 할인·연쇄 예약·MP06이 중복 적용되지 않았다. 전체 검사 1,705개와 실제 앱의 세 프로세스 검사가 통과했다. 전후 48판에서 생존 설정의 M03 중단은 8→68회로 늘었고, 그중 M06 전환은 52회였다. 보스 등장 판은 5→3개였으며 모두 시간 초과했다. 당시 후속 대상으로 정한 기본 공격과 전환 기록은 아래 단계에서 연결했다. 실제 제어의 기회비용 검증은 계속한다. 수치나 사용자 설정을 결과에 맞추어 바꾸지 않는다. [서리 폭발 개발 기록](../Implementation/Edict_Nova_Expansion.md) · [English](../Implementation/Edict_Nova_Expansion.en.md)

English: M06's four policies, physical 3m area, finite approach and actual control/SMB charges passed 1,705 tests and three native processes. Two restarts retained discounts, chain reservations and MP06 without duplicate effects. In the 48-run comparison, configured M03 interruptions increased from eight to 68, including 52 M06 replacements; boss spawns fell from five to three and all cases timed out. The subsequent phase below connects Mage BASIC and exact replacement records. Real control opportunity cost remains under investigation, without tuning values or player settings to these outcomes.

**2026-09-13 번개탄 후속 구현:** BASIC의 네 옵션을 이전 규칙과 분리해 마법사 전체 공격의 핵심 판단 연결을 마쳤다. 실제 비용에 따른 자원 보충, 절제·자기 눈보라 대상, 관측 이동 조준과 전역 추적 한도를 적용하며 준비·비행 중 재시작에도 실제 보상을 한 번만 처리한다. 전체 검사 1,765개와 앱 실행·재시작 17회가 통과했다. 전후 48판에서 생존 설정의 보스 도달은 3→5판, M03 중단은 68→64회였지만 모두 시간 초과했다. 새 기록의 M06 전환 56회는 실제 위험 시전 판단과 발사·적중을 연결하며, 제어 적용 수를 막은 적 시전 수로 해석하지 않는다. 다음은 다른 직업의 남은 판단과 제어의 실제 처치·중단 결과, 성장·파밍 효용을 검증하는 작업이다. [번개탄 개발 기록](../Implementation/Edict_Mage_Basic_Expansion.md) · [English](../Implementation/Edict_Mage_Basic_Expansion.en.md)

English: Mage BASIC now independently applies all four policies, completing the Mage attack family. Actual-cost recovery, target preferences, observed aim and pursuit limits survive preparation/projectile restarts without duplicate rewards. All 1,765 tests and 17 native processes passed. In 48 runs, configured boss spawns rose from three to five and M03 interruptions fell from 68 to 64, but every configured run timed out. The 56 new M06 replacement records connect actual dangerous-cast decisions to release/hit outcomes; applied statuses are not prevented enemy casts. Other classes, control kills/interruptions, growth and farming utility remain.

**2026-09-09 추가 요구:** 사냥 행동의 저장·공유와 스킬 교체는 [사냥 칙령 상세안](HELLSCRIPT_Hunt_Edict_Detail.md), [전역 옵션](HELLSCRIPT_Hunt_Edict_Global_Options.md), [스킬별 옵션](HELLSCRIPT_Hunt_Edict_Skill_Options.md)을 따릅니다. 명시적 저장·되돌리기·미저장 이탈 확인, 공유 코드·5개 슬롯, 모든 스킬 설정 보존과 현재 장착 스킬만 표시하는 흐름이 후속 구현 대상입니다. 아래의 기존 빌드 슬롯 설명 중 사냥 칙령과 스킬 교체 범위는 새 요구를 우선하며 장비 교체 제한은 그대로 구분합니다. 이번에는 기획과 옵션 설계까지만 작성했습니다.

| 작업 | 구체적인 동작 | 개발·완료 기준 |
|---|---|---|
| 비교 화면 보완 | 현재 장비와 후보의 기본값·접두/접미·값 범위·티어·전설·세트 활성 수를 나란히 보여 준다. | 공격력 하나만으로 우열을 확정하지 않는다. 긴 설명과 여러 접사도 마지막 줄까지 읽힌다. |
| 필터·정렬·새 장비 | 부위·등급·직업·세트·효과로 좁히고 획득순·레벨순으로 정리한다. | 화면을 바꾼 뒤에도 선택 상태가 일관되며, 실제 소유 아이템 ID에 표시를 연결한다. |
| 일괄 판매·분해 | 실행 전에 처리 대상, 보호되어 제외된 장비, 예상 골드·재료를 보여 준다. | 장착·잠금·프리셋 참조 장비를 모든 처리 경로에서 보호한다. 실제 강화 재료 환급 규칙을 그대로 사용한다. |
| 빌드 슬롯 불러오기 | 장비·스킬·패시브·행동의 변경점을 먼저 보여 주고 마을에서 적용한다. | 미소유·창고 보관·부위 충돌·미해금 항목을 표시한다. 없는 장비를 자동 구매하지 않으며 전투 중에는 허용된 행동 설정만 바꾼다. |
| 포탈 정리 | 진행 중인 균열을 보존한 채 가방·판매·분해·창고를 사용한다. | 장착·강화·회복 등의 금지 작업이 메뉴·NPC·프리셋으로 우회되지 않는다. 돌아온 위치·HP·적·쿨타임이 같다. |

## 4. 걸어 다니는 성소와 네 NPC

담당 명세: 체크리스트 CHK-U01~U02. 작은 마을 한 곳을 먼저 만들며, 움직이는 캐릭터와 NPC에는 교체 가능한 통자 임시 모델을 사용한다.

| 대상 | 연결할 서비스 | 개발·완료 기준 |
|---|---|---|
| 대장장이 | 강화·일반 제작·분해를 연다. | 기존 경제 처리 함수를 공유한다. NPC 접근만으로 비용을 쓰거나 장비를 변경하지 않는다. |
| 재설정 담당 | 접사 재설정과 검증 완료 후 보석 서비스를 연다. | 보석 자료가 준비되지 않은 상태를 설명하고, 임의의 효과를 넣어 완성된 기능처럼 표시하지 않는다. |
| 수수께끼 상인 | 부위를 선택해 미확인 장비를 구매한다. | 메뉴와 같은 후보 풀·비용·확정 영수증을 사용한다. 재진입으로 결과를 다시 추첨하지 않는다. |
| 균열 관리자 | 단계 선택·소탕·훈련을 연다. | 입장·소탕 자격과 실제 성장 상태를 공통 경계에서 확인한다. |
| 공유 창고 | 계정 보관함을 연다. | 이동해도 잠금·프리셋 참조와 소유권을 유지한다. |

목적지를 누르면 영웅이 자동으로 이동하고 도착 후 해당 서비스를 연다. 새 목적지를 선택하면 이전 이동 요청을 교체한다. 이동 실패·앱 복귀·서비스 닫힘을 처리하고, 메뉴 바로가기는 같은 서비스를 즉시 열 수 있게 한다. 마을 이동을 서비스 사용의 강제 대기시간으로 만들지 않는다. 직접 공격 조작이나 조이스틱을 추가하지 않는다.

## 5. 훈련과 실패 원인 분석

담당 명세: 체크리스트 CHK-F01~F04, [실제 캐릭터 훈련 상세안](HELLSCRIPT_Player_Training_Detail.md). 현재 레벨·장비 복사와 60초 종료의 구현·검사는 [훈련 개발 기록](../Implementation/Player_Training_Expansion.md)에 이어서 정리한다.

A/B의 고정 기준·변경 목록·피해 출처별 누적 집계·회피 판정·결과 저장은 [훈련 비교 상세안](HELLSCRIPT_Training_Comparison_Detail.md)에 따라 구현했다. Core 검사 69개·전체 회귀 611개와 이후 UI 변경의 실제 실행 증거는 [훈련 비교 개발 기록](../Implementation/Training_Comparison_Expansion.md)에서 구분한다. 후속 단계에서 기존 로컬 설정의 5개 슬롯·이름 저장을 연결했다. [사냥 칙령 개발 기록](../Implementation/Hunt_Edict_Expansion.md)에 범위를 구분하며, 핵심 옵션 v0.2·공유 요구까지 완료한 것으로 보지 않는다.

| 작업 | 구체적인 동작 | 개발·완료 기준 |
|---|---|---|
| 플레이어용 훈련 | 실제 영웅·소유 장비·해금 상태의 복사본으로 3개 훈련을 실행한다. | 복사본·해금·60초 종료·명시적 슬롯 저장과 실제 앱 재시작을 검증했다. XP·재화·장비·실클리어는 유지된다. 장비 교체를 포함한 훈련 편집은 남아 있다. |
| A/B 비교 | 같은 시드·배치·영웅 상태에서 행동·스킬·패시브를 바꿔 정상 종료까지 비교한다. | 변경 목록·전체 누적 지표·최근 비교 1개 저장·선택한 설정의 명시적 활용을 구현했다. 60초·사망·표적 전멸을 구분하고 다른 장비나 레벨을 같은 조건으로 표시하지 않는다. N10 외 회피 분석과 5개 슬롯·공유 이관은 별도다. |
| 최근 10회 | 결과 요약에서 원인 집계와 사망 직전 5전투초, 해당 행동 규칙으로 이동한다. | 당시 목표·상태·규칙 버전을 남긴다. 사거리 부족, 자원 부족, 다른 행동 진행 중, 제어에 의한 중단을 구분한다. |
| 시간과 파밍 효율 | 전투·이동·회수·포탈·편집·로딩 시간을 구분해 보여 준다. | 전투 초당 피해와 실제 시간당 보상을 혼용하지 않는다. 배속에 별도 보상 배율을 곱하지 않는다. |

최근 10회 상세 기록의 저장·조회 구현과 검증은 [전투 기록 개발 문서](../Implementation/Combat_History_Expansion.md)에 정리한다. 종료 시 설정과 불발 당시 설정을 구분하며, 재시작 전 관측하지 못한 구간을 추정하지 않는다. [English](../Implementation/Combat_History_Expansion.en.md)

## 6. 탐험 보상과 반복 플레이의 마무리

담당 명세: [균열 상세안](HELLSCRIPT_Rift_Exploration_Detail.md), 체크리스트 CHK-F05~F06.

상자 종류·수량·열기 시간·보상 예산과 성소·저주 상자의 수치는 담당 명세를 유지한다. 새 상자만 더 늘리기 전에 다음 연결을 검증한다.

| 상황 | 필요한 처리 | 완료 기준 |
|---|---|---|
| 상자를 발견했다. | 상자 정책·우회 거리·위험·잔여 시간에 따라 방문한다. | 발견하지 않은 보상을 미리 알고 최단 파밍 경로를 고르지 않는다. |
| 여는 중 피격·위험·앱 종료가 발생했다. | 열기 상태와 중단 사유, 확정 전·후 보상을 구분한다. | 재개해도 같은 상자에서 재화를 두 번 얻거나 장비를 다시 추첨하지 않는다. |
| 가방이 부족하다. | 바닥 아이템과 상자 보상을 유지한 채 허용된 정리 흐름으로 이동한다. | 자동 장착·임의 삭제·무한 포탈 반복이 없다. |
| 보스를 잡거나 제한시간이 끝났다. | 유지한 획득물과 놓친 바닥 장비·미개봉 상자를 구분한다. | 발생하지 않은 보상을 사후 지급하지 않고, 확정된 소유 장비는 잃지 않는다. |
| 자동 반복을 켰다. | 실패 시 같은 단계·1단계 하향·중단 정책을 따른다. | 결과 대기 중 취소와 상세 열람을 처리한다. 저장·연결·가방 오류가 남아 있으면 다음 판을 시작하지 않는다. |

2026-09-13 구현: 성공·실패 후 단계, 횟수·시간·금화·장비·단계 목표, 결과 열람 중 대기 정지, 실제 설정 소유권에 따른 중지와 앱 재실행 후 명시적 이어가기를 연결했다. 등급별 판매·분해·창고 이동은 보호 대상과 저장 거래를 지키며, 가방·저장 오류가 남으면 다음 판을 시작하지 않는다. 검증 결과와 로컬 구현의 범위는 [반복 사냥 개발 기록](../Implementation/Repeat_Hunt_Expansion.md)을 따른다. 온라인 연결·서버 소유 세션과 추가 정리 트리거는 후속 범위다.

English: repeat outcome/target policies, paused result review, owner-correct cancellation and explicit recovery after relaunch are implemented. Grade-based cleanup respects protected equipment and save transactions; bag/save blockers prevent the next run. See the [English record](../Implementation/Repeat_Hunt_Expansion.en.md) for validation and local-only scope. Online session ownership and additional cleanup triggers remain follow-up work.

## 7. 공개 전에 필요한 계정·보상·보석

담당 명세: 체크리스트 CHK-S01~S08, 상위 기획서의 보석 기준.

1. 서비스 공급자와 로그인·복구 방식을 정한 뒤 계정당 활성 전투 세션 하나와 저장 revision을 연결한다.
2. 보스·상자·소탕·미실행·제작·구매를 확정 영수증으로 조회한다. 응답이 유실되면 기존 요청 결과를 확인한다.
3. 서버 시각으로 KST 09:00 경계·계정 소탕 3회·캐릭터별 초회·12시간 미실행 상한을 검증한다.
4. 유료 가방·창고 탭의 구매·복원·환불·초과칸 꺼내기 전용 상태를 구현한다. 환불 때문에 보관 장비를 삭제하지 않는다.
5. 지정된 보석 자료 버전을 검증한 뒤 소켓·장착·해제·비용·효과를 연결한다. 여러 버전의 수치를 섞지 않는다.

온라인 공급자·로그인, 운영 규모와 예산, 스토어·지원 지역·상품 정보는 이 단계의 소유자 선택 사항이다. 후보와 비용·지원 범위를 먼저 정리한 뒤 확인한다. 현재 로컬 저장 검사를 온라인 중복 보상 방지의 완료 증거로 사용하지 않는다.

## 8. 아트·음향·Android 품질

담당 명세: 체크리스트 CHK-P01~P08.

| 작업 | 제작·검사 범위 | 완료 기준 |
|---|---|---|
| 화면 비율·방향 대응 | 모바일 가로·세로 UI와 설정의 방향 선택, PC의 가로 공통 구성 리사이징을 연결한다. | [화면 상세안](HELLSCRIPT_Screen_Layout_Detail.md)의 SCR-01~SCR-08에 따라 가독성·안전 영역·편집 및 전투 상태 보존·설정 복원·실기기 입력을 검증한다. 프리셋 대화창부터 배치 대응을 시작했으며 전체 화면과 방향 설정은 개발 중이다. |
| 정적 이미지 | 장비·세트·상자·서비스 아이콘을 정의 ID와 연결한다. 내장 이미지 생성기를 활용한다. | 원본·프롬프트·사용처·검토 상태를 남긴다. 확인되지 않은 생성 모델 버전을 사용했다고 표기하지 않는다. |
| 임시 3D 표현 | 영웅·적·보스의 형태와 준비·공격·피격·사망, 상자 개폐를 표현한다. | 표현 시점과 실제 판정이 일치하고, 교체 후에도 저장 ID·판정이 유지된다. |
| 전투 가독성 | 적 예고와 아군 장판, 안전 영역, 보스 후속 공격을 구분한다. | 색뿐 아니라 형상·테두리·시간 변화로 읽히며 효과 축소로 판정 정보를 잃지 않는다. |
| 소리와 설정 | UI·공격·피격·위험·상자·전설 획득·환경음을 연결한다. | 권리를 확인한 음원을 사용하고 겹침 상한을 둔다. 무음에서도 플레이할 수 있으며 음악·UI 음높이가 배속에 따라 변하지 않는다. |
| Android 실행 | 빌드 모듈·대표 기기 2종을 준비하고 터치·안전 영역·글자 확대·백그라운드 복귀를 검사한다. | 실제 기기의 결과를 남긴다. macOS 검사를 Android 검사로 대신하지 않는다. |
| 장시간 품질 | 세 배속, 보스+부하+장판, 30분 반복에서 프레임·메모리·발열·배터리를 측정한다. | 기존 30fps·95백분위 33.3ms 시험 목표와 대조한다. 측정 전에 최적화 완료나 발열 안정성을 보장하지 않는다. |

## 9. 착수 순서와 다시 물을 내용

전체 순서는 **전설·세트 통합 → 6빌드 비교 → 실제 성장·훈련 → 장비 정리·마을 → 결과·반복 흐름**이다. 2026-09-12에 능력치 시트가 들어오면서 20단계 난이도와 접사 분포가 먼저 확인할 항목이 되었다. 실제 캐릭터 훈련과 성장에 이어 첫 안내와 행동 A/B를 연결했다. 추가된 사냥 칙령의 저장·공유 요구와 장비 정리·성장 구간별 비교를 진행하며 신규 사용자 관찰과 전설의 남은 실제 조합 검사도 유지한다. 온라인·보석·기기 준비는 필요한 자료를 함께 확보하되 공개 필수 항목으로 남긴다.

기존의 배속 3종, 캐릭터별 초회 보상, 직업·장비 슬롯, 온라인 요구를 다시 확인할 필요는 없다. 새 전투 수치와 추천 규칙은 개발 시험안으로 기록하고 검증한다. 서비스 공급자·로그인·예산·스토어 정보는 구체적인 구성을 제시할 때 확인하며, 실기기 검사는 실제 기기 확보 후 진행한다.

도망가는 보물 적, 깨지는 장식물, 랜덤 NPC, 다층 던전, 추가 바이옴은 기본 탐험·경제 검증 뒤 검토할 별도 제안이다. 거래·PvP·시즌·펫·용병·추가 일일 숙제를 이번 남은 필수 작업에 넣지 않는다.
