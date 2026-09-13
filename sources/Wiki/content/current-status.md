# 현재 개발 현황과 남은 과제

갱신일: 2026-09-13

**2026-09-13 전투 기록 보관:** 최근 10회에 스킬 집계·마지막 5초·당시 규칙과 칙령·종료 장비를 저장하고 다시 조회할 수 있습니다. 자동 반복 대기와 읽던 문단 유지, 다음 균열·캐릭터 변경·재시작을 검증했습니다. 전체 검사 2,156개와 실제 앱 검사 3회가 통과했습니다. [개발 기록](../../Docs/Implementation/Combat_History_Expansion.md)

English: the latest ten rifts now retain skill totals, the final five-second window, actual rule/edict context and final equipment. Countdown/reading-position behavior and history after another rift, character change and relaunch were verified. All 2,156 editor tests and three native processes passed. [English record](../../Docs/Implementation/Combat_History_Expansion.en.md)

**2026-09-13 균열 출구 선택 수정:** 현재 방의 유효 출구를 먼저 고르고, 성소의 실제 사용 거리 안에 들어온 뒤 사용하도록 수정했습니다. 균열 대조 360판에서 기존 규칙 169/180판, 핵심 기본값 173/180판이 클리어했고 성소 왕복 4판이 해결됐습니다. 룬 완료본을 포함한 최신 메인의 전체 검사 2,120개, 대표 대조 24판, 실제 앱 실행·재시작 3회와 화면 5장 검토도 마쳤습니다. 보스에게 얻은 룬 4개까지 재시작 후 중복 없이 유지됩니다. 자연 성장·파밍·남은 밸런스·모바일 실기기 검증은 후속 작업입니다. [개발 기록](../../Docs/Implementation/Edict_Field_Expansion.md)

English: current-room exit ordering and shrine use range are corrected. The controlled 360-run comparison cleared legacy 169/180 and core defaults 173/180, resolving four shrine loops. The Rune-integrated main passed all 2,120 tests, 24 representative comparisons, three native launches/restarts and five screenshot reviews. Four earned boss runes also persist without duplication. Natural growth/farming, remaining balance and physical devices remain. [English record](../../Docs/Implementation/Edict_Field_Expansion.en.md)

**2026-09-13 궁수 핵심 옵션:** 액티브 6종과 기본 사격의 핵심 옵션 30개를 독립 판단에 연결했습니다. 전체 검사 2,079개, 궁수 전용 98개와 실제 앱 실행·재시작 13회가 통과했고 화면 31장을 검토했습니다. 덫·투사체·후퇴 후 보행·표식·실제 적중으로 얻는 할인과 저장 복원을 확인했습니다. 생성 균열의 장기 생존·성장·파밍 효용과 모바일 실기기 검증은 남아 있습니다. [개발 기록](../../Docs/Implementation/Edict_Ranger_Expansion.md)

English: all 30 Ranger active/BASIC core options now use independent policies. All 2,079 tests, 98 Ranger-specific cases and 13 native launches/restarts passed; 31 screenshots were reviewed. Actual traps, projectiles, follow-up walking, Mark, earned hit discounts and persistence were verified. Long-run generated-rift survival/growth/farming and mobile-device validation remain. [English record](../../Docs/Implementation/Edict_Ranger_Expansion.en.md)

**2026-09-13 전사 방어·함성·기본 공격:** 철벽·전투 함성·근접 기본 공격의 나머지 13개 옵션을 연결했습니다. 전사 핵심 옵션 30개가 모두 독립 판단을 사용합니다. 전체 검사 1,981개와 최종 앱 실행·재시작 5회가 통과했고 화면 17장을 검토했습니다. 실제 보호막·자원 회복·3회 적중으로 얻은 절제 할인과 저장 복원을 확인했습니다. `main`만 유지하며, 궁수 정책과 생성 균열의 장기 밸런스·모바일 실기기 검증은 남아 있습니다. [개발 기록](../../Docs/Implementation/Edict_Warrior_Support_Expansion.md)

English: all 13 remaining W05/W06/Warrior BASIC options are connected, completing 30 Warrior core options. All 1,981 tests and five final native processes passed; 17 screenshots were reviewed. Actual shields, resource recovery, earned three-hit LC02 discounts and persistence were verified. Only `main` remains. Ranger policies, long-run generated-rift balance and mobile-device validation remain. [English record](../../Docs/Implementation/Edict_Warrior_Support_Expansion.en.md)

**2026-09-13 지면 강타와 브랜치 정리:** 지면 강타의 핵심 옵션 4개를 일반 전투와 지정 생존 대응에 연결했습니다. 실제 기절·보스 제압·시전 중단·저장 복원을 검증했고, 전체 검사 1,903개와 최종 앱의 실행·재시작 3회가 통과했습니다. 한국어·영어 화면 13장을 확인했습니다. 클로드의 통합 기록을 유지하면서 병합이 끝난 작업 브랜치를 로컬·원격에서 삭제했고, 현재 작업 위치는 `main`입니다. 철벽·함성·전사 기본 공격과 다른 직업의 남은 정책은 후속 작업입니다. [개발 기록](../../Docs/Implementation/Edict_Slam_Expansion.md)

English: all four W04 options now independently drive ordinary combat and designated survival control. All 1,903 tests and three final native processes passed; 13 KO/EN screenshots were reviewed. The fully merged development branch was removed locally and remotely, Claude’s integration remains on main, and the checkout is now `main`. W05/W06, Warrior BASIC and remaining class policies still require work. [English record](../../Docs/Implementation/Edict_Slam_Expansion.en.md)

**2026-09-13 전사 공격 사냥 칙령:** 회오리·도약·분쇄의 13개 핵심 옵션을 독립 판단에 연결했습니다. 실제 틱 비용·보행 충전·이륙 예약·착지 후 한 번의 공격 선호와 저장 복원을 검증했습니다. 전체 검사 1,847개와 실제 앱의 11개 프로세스가 통과했습니다. 24판 대조에서 충전 연계가 발생했지만 클리어 수는 같고 기본값의 사망은 늘어, 밸런스 개선으로 판단하지 않습니다. 전사 나머지 스킬·기본 공격과 다른 직업의 후속 연결은 남아 있습니다. [전사 공격 개발 기록](../../Docs/Implementation/Edict_Warrior_Attacks_Expansion.md)을 확인하세요.

English: W01-W03 independently apply all 13 core options, actual tick costs, walking charges, takeoff reservation and a persistent one-attack landing preference. All 1,847 tests and 11 native processes passed. The 24-run comparison observed charge linking, unchanged clear counts and an additional default-mode death; no balance improvement is claimed. Remaining Warrior skills/BASIC and other class policies remain. See the [English record](../../Docs/Implementation/Edict_Warrior_Attacks_Expansion.en.md).

**2026-09-13 번개탄 사냥 칙령:** 마법사 기본 공격의 네 옵션을 독립 판단에 연결했습니다. 실제 비용에 따른 자원 보충, 자기 눈보라·절제 대상 우선, 관측 이동 조준과 저장 복원을 검증했습니다. 전체 검사 1,765개와 새 앱의 실행·재시작 17회가 통과했습니다. 전후 48판에서 생존 설정의 보스 도달은 3→5판이었지만 모두 시간 초과했으며, 새 기록은 M03→M06 전환 56회의 실제 선택·발사·적중을 구분합니다. 클로드의 능력치·접사 통합은 메인에 유지됩니다. [번개탄 개발 기록](../../Docs/Implementation/Edict_Mage_Basic_Expansion.md)을 확인하세요.

English: Mage BASIC now independently applies all four policies, actual-cost resource recovery, own-Blizzard/Restraint targeting, observed-motion aim and persistent committed shots. All 1,765 tests and 17 native processes passed. In 48 before/after runs, configured boss spawns increased from three to five but every configured run timed out; new telemetry distinguishes the actual decisions, releases and hits of 56 M03-to-M06 replacements. Claude's attribute/affix integration remains on main. See the [English record](../../Docs/Implementation/Edict_Mage_Basic_Expansion.en.md).

**2026-09-13 서리 폭발 사냥 칙령:** M06의 네 옵션을 독립 판단에 연결했습니다. 실제 자기 범위·보행 접근·보스 제한을 따르고, 실제 냉기 적중으로 만든 연쇄 충전과 할인 비용이 재시작 후 중복 적용되지 않습니다. 전투 도중 영어로 바꿀 때 이전 언어의 행동 문구가 남는 문제도 수정했습니다. 전체 검사 1,705개, 실제 macOS 앱의 세 프로세스 검사와 화면 13장 확인을 마쳤습니다. 전후 48판 중 생존 설정의 M03 중단은 8→68회, 보스 등장 판은 5→3개였고 8판 모두 시간 초과했습니다. 당시 다음으로 정한 기본 공격 연결과 전환 기록 보완은 위의 단계에서 진행했습니다. 실제 제어의 기회비용은 계속 검증합니다. [서리 폭발 개발 기록](../../Docs/Implementation/Edict_Nova_Expansion.md)을 확인하세요.

English: M06 now independently applies its four policies, physical self-area, walking approach and boss restrictions. Real cold-hit charges and discounts survive restart without duplication; switching language during preparation now updates the HUD text. All 1,705 tests and three native processes passed, retaining 13 screenshots. In the 48-run comparison, configured M03 interruptions rose from eight to 68, boss spawns fell from five to three and all eight runs timed out. The subsequent basic-attack and replacement-observation phase is recorded above; real control opportunity cost remains under investigation. See the [English record](../../Docs/Implementation/Edict_Nova_Expansion.en.md).

**2026-09-13 원소 보호막 사냥 칙령:** M05의 네 옵션을 이전 규칙과 분리하고, 교전 시작의 한 번 검토와 실제 보호막 생성·흡수·환류 보상을 연결했습니다. 일부 흡수한 상태를 재시작해도 기준과 지급 여부를 유지합니다. 전체 검사 1,652개, 실제 macOS 앱의 세 프로세스 검사와 화면 12장 확인을 마쳤습니다. 전후 48판 중 생존 설정 표본의 M03 중단은 34→8회로 줄었지만 전후 모두 8판 전부 시간 초과했습니다. 기본값 문서와 생존 설정을 구분해 기록했습니다. 당시 후속 대상으로 정한 M06은 위의 단계에서 연결했습니다. [원소 보호막 개발 기록](../../Docs/Implementation/Edict_Shield_Expansion.md)을 확인하세요.

English: M05 now has independent policies, one encounter-opening review and persistent actual shield absorption/reward state. All 1,652 tests and three native processes passed, retaining 12 screenshots. In 48 before/after runs, configured-survival M03 interruptions fell from 34 to eight, but all eight configured cases timed out in both versions. Default and configured documents remain separate; the subsequent M06 phase is recorded above. See the [English record](../../Docs/Implementation/Edict_Shield_Expansion.en.md).

**2026-09-13 순간이동 사냥 칙령:** M04의 네 옵션을 이전 규칙과 분리하고, 일반 거리 조정이 준비 중 공격을 취소하지 않도록 연결했습니다. 확정한 착지점을 실행 직전에 다시 검사하며 적 없는 환경 회피도 유지합니다. 전체 검사 1,601개와 새 macOS 앱의 저장·재시작이 통과했습니다. 전후 32판 진단에서 기본 칙령의 연쇄 번개 중단은 78→18회로 줄었지만 사망은 4→7회로 늘었습니다. 전역 생존·회피가 꺼진 기본값 문서의 결과이므로 성능 개선으로 해석하지 않습니다. 당시 후속 대상으로 정한 M05와 생존 설정 비교는 위의 단계에서 완료했습니다. [순간이동 개발 기록](../../Docs/Implementation/Edict_Teleport_Expansion.md)을 확인하세요.

English: M04 now has independent core policies and release-time physical landing validation. Ordinary distance adjustment cannot cancel a preparing attack; targetless global escape remains supported. All 1,601 tests and native save/restart passed. Across 32 before/after runs, default-edict M03 interruptions fell from 78 to 18, while deaths rose from four to seven with survival disabled. The subsequent M05 phase and explicit survival comparison are recorded above. See the [English record](../../Docs/Implementation/Edict_Teleport_Expansion.en.md).

**2026-09-13 연쇄 번개 사냥 칙령:** M03의 네 옵션을 이전 규칙 조건과 분리해 전투에 연결했습니다. 전체 검사 1,566개와 macOS 설정 저장·재시작이 통과했습니다. 준비 중 첫 대상을 잃으면 빗나가며, 저장 복원 후 예약한 충전과 원래 대상은 한 번만 실행합니다. 선택한 16판 진단에서는 자원 부족 차단은 없었고, 사냥 칙령 사용 시 시전 중단이 더 많았습니다. 후속 조사는 생존 대응과 준비 취소의 연결을 우선합니다. [연쇄 번개 개발 기록](../../Docs/Implementation/Edict_Chain_Expansion.md)을 확인하세요.

English: all four M03 policies are independent of legacy conditions. The 1,566-test suite and native save/restart passed, including first-target release validation and charge persistence. Sixteen outcome-selected runs found no resource-blocked checks and more interrupted casts with the complete edict. Next, investigate survival responses and preparation cancellation. See the [English record](../../Docs/Implementation/Edict_Chain_Expansion.en.md).

**2026-09-13 눈보라 사냥 칙령:** M02의 다섯 옵션을 전투에 연결했습니다. 전체 검사 1,514개와 macOS 실행·재시작이 통과했습니다. 장판 보존·교체, 설치 위치와 LM01 추적을 적용하며, 설정을 바꾸거나 앱을 재시작해도 지불한 시전은 원래 설정으로 한 번만 완료됩니다. [눈보라 개발 기록](../../Docs/Implementation/Edict_Blizzard_Expansion.md)에서 확인합니다.

English: all five M02 Blizzard options are connected. The 1,514-test suite and native launch/restart passed, including area preservation/replacement and paid cast continuity. See the [English record](../../Docs/Implementation/Edict_Blizzard_Expansion.en.md).

**2026-09-12 화염구 사냥 칙령:** M01의 네 옵션을 실제 전투에 연결했습니다. 전체 검사 1,461개, 최종 문구의 번역 검사 43개와 macOS 실행·재시작이 통과했습니다. 자동 사용을 꺼도 이미 발사한 화염구가 유지되며 저장 복원 뒤 한 번만 폭발합니다. 당시 앱과 검증 범위는 [화염구 개발 기록](../../Docs/Implementation/Edict_Fireball_Expansion.md)에서 확인합니다.

갱신일: 2026-09-13 · 최초 자료 정리 이후의 구현과 검증 결과를 함께 관리합니다. 각 단계의 자동 검사·실제 앱 확인·미검증 범위는 연결된 개발 기록에서 구분합니다.

**2026-09-12 능력치 통합 후 접사 검증:** 클로드 변경과 최근 균열·해금 개발을 합친 `92d5812`를 main에 반영했습니다. 현재 54종과 비교용 24종에서 장비를 각각 960만 개 생성했고, 접사·배분·티어 비교 12,864개가 통과했습니다. 기존 옵션의 출현율 감소를 확인해 기획 설명을 보완했습니다. 지도·적·장착 구성이 같은 20단계 대조 도구를 추가했고, 체계당 600판 확대 실행도 완료했습니다. 현재 54종의 승리·사망·시간 초과는 188·45·367회, 비교용 24종은 212·47·341회였습니다. [분포 검사와 전투 비교 결과](../../Docs/Implementation/Affix_Audit_Expansion.md)를 구분해 확인합니다.

English: reviewed integration `92d5812` is on main. Each affix profile generated 9.6 million items; all 12,864 distribution checks passed, confirming lower inclusion rates for existing affixes. Paired stage-20 tooling verifies maps, enemies and equipped item identities. Both 600-run combat profiles are complete: current54 cleared 188/600 runs and legacy24 cleared 212/600. See the [English audit record](../../Docs/Implementation/Affix_Audit_Expansion.en.md).

**2026-09-12 보상 기능 갱신:** [균열 단계별 등급 확률](../../Docs/Design/HELLSCRIPT_Rift_Rarity_Detail.md)을 일반·정예·보스·소탕과 한국어·영어 보상 안내에 연결했습니다. 일반 전투는 현재 단계, 소탕은 해당 영웅의 최고 실제 클리어 단계를 사용합니다. 첨부 GDD·Excel을 프로젝트 기준본으로 저장했고, 재설정은 아이템 레벨별 고정 비용을 적용합니다. [개발·검증 기록](../../Docs/Implementation/Rift_Rarity_Expansion.md)에 자동 검사·추첨 실험·macOS 화면 확인 결과와 미실행 범위를 기록했습니다. 상한과 증가 속도의 장기 밸런스 검증은 남아 있습니다.

English: rift rarity now uses the active encounter stage, while sweeps use the selected hero's highest actual clear. The GDD, workbook, shared reward code and Korean/English reward screen are aligned; reroll costs depend only on item level. See the [implementation and validation record](../../Docs/Implementation/Rift_Rarity_Expansion.en.md) for evidence and remaining limits.

## 2026-09-12 콘텐츠 공개·해금

성장 메뉴는 계정의 최고 실제 보상 확정 클리어 기록으로 열립니다. 첫 균열 시도 종료 후 훈련장, 1단 강화·오프라인 보상, 3단 기본 제작, 5단 보석 메뉴, 8단 옵션 재설정, 10단 미확인 장비 상점, 12단 소탕 메뉴, 15단 전설 코어 제작 순서입니다. 단계와 레벨은 HELLSCRIPT 테스트 초안입니다.

첫 보석 정상 획득과 같은 부위 코어 10개 달성은 각각 조기 해금 조건이며, 소비 후에도 권한을 유지합니다. 영웅별 스킬·패시브와 소탕 보상, 현재 균열 단계의 전투 콘텐츠는 계정 공통 메뉴 권한과 별도로 판정합니다. 기능 권한과 안내 완료 상태도 분리했습니다. 기존 저장에서 정당하게 열려 있던 기능·패시브와 기존 구매 권한, 단계별 등급 확률 및 레벨별 고정 재설정 비용을 보존합니다.

보석 실물·소켓·합성 레시피와 인증된 계정 서버·다른 기기 동기화는 아직 연결되지 않았습니다. 보석 메뉴의 해금이 보석 기능 전체의 구현 완료를 뜻하지 않습니다. 첫 강화에 필요한 재료와 신규 영웅의 첫 클리어 난도도 후속 검증이 필요합니다. 무료 재화는 추가하지 않았습니다.

구체적인 조건과 U01~U30 검증 구분은 [해금 상세 명세](../../Docs/Design/HELLSCRIPT_Content_Unlock_Spec_v0.1.md)와 [구현·검증 기록](../../Docs/Implementation/Content_Unlock_Expansion.md)을 확인하세요. 공통 원본을 읽는 ‘콘텐츠 해금’ DB와 Excel의 해금·검증 시트도 추가했습니다.

English: account-wide menu access follows rewarded actual clears, while hero skills, passive slots, sweep rewards and current-stage encounters retain separate rules. Early unlocks persist after spending gems or same-slot cores; legacy access and purchase rights are preserved. Gem operations and authenticated cross-device synchronization remain dependencies. See the [English implementation record](../../Docs/Implementation/Content_Unlock_Expansion.en.md).

## 현재 구현을 판단하는 기준

최초 빌드 문서는 당시 상태를 보존합니다. 최신 상태는 후속 개발 기록, 현재 소스, 해당 변경을 검사한 결과를 함께 확인합니다. 위키의 ‘현재 정의’는 코드나 에셋에 등록된 데이터를 뜻하며 모든 조합의 검증 완료를 뜻하지 않습니다.

| 영역 | 확인한 구현 | 남은 범위 |
| --- | --- | --- |
| 장비·아이템화 | 베이스 24종, 접사 54종, 개별 전설 15종, 세트 6종과 세트 장비 24종이 현재 코드에 정의되어 있습니다. 장비 목록의 필터·정렬, 획득 순번과 일괄 판매·분해 미리보기를 연결했습니다. 2026-09-12에 목걸이 초희귀 전설 셋의 가중치를 1에서 10으로 올렸습니다. 보석·소켓과 장비 품질은 기획을 마쳤고 구현은 시작하지 않았습니다. | 모든 합법적 장착 조합, 장비 교체·편집·복원 경로와 여러 시드의 빌드 역할을 이어서 검사합니다. |
| 능력치 | 2026-09-12에 클로드가 추가한 속성 57개와 접사 54종을 인수했습니다. 네 핵심 능력치에서 최종 수치를 계산하며, 성장 화면에서 능력치를 확인합니다. 기존 접사 번호 24개의 의미를 보존했습니다. 이동 속도는 실제 m/s로, 장비의 이동 속도 보너스는 %로 표시합니다. 최대 자원 증가도 전투 게이지에 반영합니다. 접사 체계별 960만 개 생성 검사에서 기존 옵션의 출현율 감소를 확인했고, 실제 추첨은 예상 분포와 맞았습니다. | 군중 제어 지속시간 감소와 기력 두 줄은 현재 적용 대상이 없어 장비에서 제외합니다. 과거 20단계 승리 12→6회는 현재 통합본의 성능으로 재사용하지 않습니다. 54종·24종의 각 600판 대조를 완료했습니다. 후속 전후 32판 진단에서 옛 M04·M05 규칙의 공격 중단을 확인했습니다. M04 연결 후 중단은 줄었지만 전역 생존이 꺼진 기본 문서의 사망은 늘었습니다. M06 전후 48판에서도 생존 설정은 모두 시간 초과했고 M03 중단은 8→68회로 늘었습니다. M06 제어 전환·공격 취소 비용과 보스 도달·성장·파밍 효용을 계속 검증합니다. |
| 행동 편집 | 22개 조건을 공용 정의하고 규칙별 목표·이동과 행동 차단 이유를 연결했습니다. | 첫 플레이 안내, 규칙 이해도와 실제 터치 사용성을 검증합니다. |
| 전투 | 준비·발사·도약·착지·채널, 투사체·덫, 상태 효과·보호막·비용 충전·세트 노출을 구현했습니다. | 전체 전설·세트 조합과 장시간·배속·중단 복원을 확대 검증합니다. |
| 균열과 탐험 | 방의 역할과 곁방이 있는 자동 생성 지도, 탐색·시야·미니맵, 상자·성소·저주 상자와 실제 보스 관문을 연결했습니다. 5단계부터 봉인석 2개·정수 운반자 3명·제물 3개 중 하나를 선택합니다. 목표 완료 또는 처치 게이지 충전으로 관문을 열고 진행 상황을 저장합니다. | 단계별 콘텐츠 공개 규칙과 합친 지도·전투는 이번 통합 검사에 포함합니다. 이전 1만 개 지도 검사는 콘텐츠 해금 이전 생성본의 증거로 구분하며, 여러 시드의 자동 진행률과 장기 난이도는 추가 측정합니다. |
| 적과 보스 | 일반 적 12종·정예 6특성·보스 3종의 개별 행동과 위험 예고를 구현했습니다. | 최종 모델·애니메이션·음향을 연결하고 작은 화면의 가독성을 검증합니다. |
| 플레이어 훈련 | 실제 캐릭터 복사본, 해금 제한, 고정 배치와 60초 종료의 상세안·소스·검사 결과가 있습니다. | A/B 비교 기록, 적용 미리보기와 기기 입력 검증은 별도 과제입니다. |
| 화면·기기 대응 | 공통 설정·안내 창과 기기별 방향 설정, 화면 크기로 계산하는 전투 HUD 배치와 카메라 표시 영역, 장비 목록·상세·비교의 영역 전환을 구현했습니다. 전투 배치는 macOS 창 16개 크기에서 확인했습니다. 2026-09-11에 글자 크기 100~140% 선택을 추가했고, 20:9 화면에서 밀려나 있던 조작 20곳을 고쳤으며, 주요 조작을 80단위로 키우고 배경이 노치까지 덮도록 바꿨습니다. | 행동 설계·사냥 칙령 편집·전투 HUD의 촘촘한 위젯 크기와 모바일 실기기의 회전·터치·복귀 검증이 남아 있습니다. |
| 배속 잠금 | 일반 균열·훈련·A/B 비교는 1배속을 사용하고 1.5배속·2배속에는 잠금 표시를 붙였습니다. 기존 고배속 저장값은 1배속으로 읽고 저장합니다. | 월 구독·이벤트 쿠폰·시간 충전과 차감, 서버의 중복 사용 방지, 절전 방치 화면은 구현되지 않았습니다. |
| 사냥 칙령 v0.2 | 영웅별 v0.2 원본·편집·공유, 전역 대상·추적·전리품·생존 대응과 세 직업의 액티브·기본 공격을 연결했습니다. 궁수의 옵션 30개까지 옛 규칙과 독립해 판단하며 실제 비용·적중·저장 복원을 검사했습니다. | 생성 균열 여러 시드에서 생존·제어 전환·공격 취소·시간 초과와 장비 파밍 효용을 계속 검증합니다. 설정별 승률 개선이나 모바일 실기기 사용성은 이번 기능 검사로 입증하지 않았습니다. |
| 저장·서비스 | 로컬 저장과 개발용 정산을 제공합니다. | 계정 서버·서버 시각·교차 저장·구매 검증을 연결해야 합니다. |
| 아트·출시 품질 | 생성 이미지 4개와 코드로 만든 임시 3D·UI 표현을 사용합니다. 2026-09-10에 첫 캐릭터 3D 모델(야만전사)을 생성해 시작 씬에 확인용으로 배치했습니다. 2026-09-11에 성소 보행 광장과 정거장 5곳을 임시 표현으로 만들어 도착 시 서비스가 열리고, 균열 관리자는 전용 화면을 엽니다. | 마을·NPC·정식 모델·애니메이션·음향·Android 글꼴·성능을 완성해야 합니다. |

## 초기 문서와 달라진 부분

| 초기 기록 | 현재 확인할 내용 | 근거 |
| --- | --- | --- |
| 세트 3종·세트 장비 12종 | 후속 개발 가정에 따라 세트 6종·24부위가 정의되었습니다. 개별 전설 15종을 합친 특수 장비 결과는 39종입니다. | [장비 확장 기록](../../Docs/Implementation/Itemization_Expansion.md) |
| 접사 24종 | 디아블로4 캐릭터 시트를 옮기며 54종이 되었고, 표시 이름도 그 표기를 따릅니다. 저장 파일이 적어 둔 번호 24개의 뜻은 그대로입니다. | [능력치 체계 개발 기록](../../Docs/Implementation/Attribute_System_Expansion.md) |
| 주요 행동 조건 10종 | 현재 조건 정의는 SC01–SC22입니다. | [행동 규칙 개발 기록](../../Docs/Implementation/Rule_Expansion.md) |
| 고정 고리 지도 | 현재는 방 템플릿 기반 자동 생성 지도이며, 구형 고리는 저장 호환 경로로 남아 있습니다. | [균열 개발 기록](../../Docs/Implementation/Rift_Expansion.md) |
| 적·보스의 공통 행동 | 일반 적과 보스의 전용 패턴이 추가되었습니다. | [적 개발 기록](../../Docs/Implementation/Enemy_Expansion.md), [보스 개발 기록](../../Docs/Implementation/Boss_Expansion.md) |
| 레벨 30 개발 훈련 | 플레이어용 실제 캐릭터 훈련을 분리하는 후속 작업이 진행되었습니다. | [플레이어 훈련 상세안](../../Docs/Design/HELLSCRIPT_Player_Training_Detail.md) |
| 세 배속 제공 | 최신 빌드는 1배속만 제공하며 1.5배속·2배속은 잠금 표시입니다. | [배속 접근 제한 개발 기록](../../Docs/Implementation/Speed_Access_Expansion.md) |
| 고정 좌표의 전투 HUD | 화면 크기로 계산하는 배치와 카메라 표시 영역으로 바뀌었고, 미탐색 정보는 방·통로 단위로 가립니다. | [전투 HUD·카메라 배치 개발 기록](../../Docs/Implementation/Battle_Layout_Expansion.md) |

## 검증 근거가 말해 주는 범위

[전사 전설 통합 기록](../../Docs/Implementation/Legendary_Expansion.md)은 전체 검사 416개, 집중 검사 23개, 실제 macOS 앱 재시작 1회의 결과를 보존합니다. 이는 LW01–LW03의 장착·강제 이동·착지·단일 표적 경계에 관한 근거이며, 전설 전체의 출시 검증으로 확대하지 않습니다.

정리 시점에 존재하는 `training-editmode-final.xml`은 전체 450개 통과를 기록합니다. 해당 파일은 플레이어 훈련의 후속 검사이며 기존 416개 기록을 지우거나 그 시점의 결과를 바꾸지 않습니다. 최신 검사 파일의 수량과 시간은 위키의 검증 기록 DB에서 별도로 확인합니다. 자동 검사 통과와 실제 앱·Android·물리 입력의 검증은 구분합니다.

이번 정리 시점에 가장 최근의 전체 검사 보고서는 `speed-access-editmode.xml`(990개 통과, 2026-09-09 07:55 UTC)과 `inventory-final-editmode.xml`(1,027개 통과, 2026-09-09 08:43 UTC)입니다. [장비 목록 개발 기록](../../Docs/Implementation/Inventory_Management_Expansion.md)은 첫 전체 검사 1,026개 통과 이후의 최종 검사를 진행 중으로 기록하므로, 보존된 최종 보고서의 수치는 문서 갱신 전의 결과로 읽습니다. 위키의 검증 기록 DB는 이제 보존된 Edit Mode 보고서 전체를 단계별로 표시하며, 실패를 수정하기 전의 보고서도 그대로 남깁니다. 2026-09-09 최신 화면 대응 빌드는 [첫 플레이 빌드 문서](../../Docs/Implementation/Playable_Build.md)에 기록된 `Builds/macOS-ScreenLayout/HELLSCRIPT.app`이며 저장소에는 포함하지 않습니다.

생성 균열의 한 시드에서 6추천 빌드 × 희귀/세트의 12구성을 비교한 결과, 10구성은 클리어했고 연쇄 제어의 두 구성은 시간 초과했습니다. 이 결과는 여러 단계의 승률이나 전체 밸런스를 뜻하지 않습니다. 전사 전설 수정 이후 회오리 세트의 클리어 시간도 달라졌으므로 [최신 전사 전설 기록](../../Docs/Implementation/Legendary_Expansion.md#3-생성-균열-재검사)을 따릅니다.

## 다음 개발의 우선순위

1. 전설·세트의 실제 장착·편집·복원과 6빌드의 여러 시드 비교를 마칩니다.
2. 실제 성장·첫 플레이 안내·장비 정리·빌드 슬롯·훈련과 결과 분석을 연결합니다.
3. 성소 광장의 담당자 4명과 광장의 정식 모델을 제작합니다. 보행, 정거장 탭, 도착 시 서비스 열기, 균열 관리자 화면은 구현되었습니다.
4. 서버 계정·교차 저장·정산·구매와 보석의 검증된 적용표를 완성합니다.
5. 행동 설계·사냥 칙령 편집·전투 HUD의 촘촘한 위젯을 터치 최소 크기에 맞춰 다시 배치하고, Android 빌드·실기기 입력·글꼴·성능·아트·음향·접근성을 검증합니다. 글자 크기 조절, 주요 조작의 터치 크기, 노치·홈 바 여백은 구현되었습니다.

## 2026-09-10에 추가된 진행 중 작업

[사망 원인 분석 개발 기록](../../Docs/Implementation/Defeat_Analysis_Expansion.md)은 사망 직전 구간의 피격원과 스킬 불발을 읽는 화면을 만들었습니다. 자료 구조와 화면을 먼저 추가한 뒤 전투 루프와 규칙 판단에 기록을 연결했습니다. 기록은 메모리에만 남고 저장 파일에는 들어가지 않으므로, 저장을 불러온 전투는 빈 창에서 다시 쌓기 시작합니다.

[다중 시드 밸런스 러너 개발 기록](../../Docs/Implementation/Balance_Runner_Expansion.md)은 6개 추천 빌드를 여러 시드에서 자동으로 돌리는 러너와 보고서 저장, 단계 훑기를 추가했습니다. 구성당 2시드로 1·10단계를 실행해 진입점이 보고서를 만드는 것까지 확인했습니다. 계획의 빌드당 30시드 × 4단계는 720판이라 아직 실행하지 않았으므로, 단계별 클리어율은 측정되지 않은 상태입니다.

두 작업의 전체 Edit Mode 검사 1,037개가 통과했습니다. 다만 실제 macOS 앱에서 죽어 분석 화면을 열어 본 기록은 없습니다. 자동 검사 통과를 실제 사용 확인으로 해석하지 않습니다.

6개 추천 빌드의 30시드 × 1·10·20·30단계 밸런스 실행(720판)을 마쳤습니다. 1단계는 모두 86.7~96.7% 클리어, 10단계는 다섯 구성이 70% 이상이고 마법사 변형 2(연쇄 제어)만 3.3%, 20단계는 6.7~30%, 30단계는 전부 0%였습니다. 레벨 30 고정 장비와 기존 규칙 설정의 측정이므로 밸런스 결론이 아니라 조사 출발점입니다. 자세한 표는 [밸런스 러너 기록](../../Docs/Implementation/Balance_Runner_Expansion.md)에 있습니다.

사냥 칙령 v0.2는 저장·전투·공유·편집의 네 단계를 마쳤고, 편집·공유 화면은 macOS 개발 빌드의 런타임 스모크로 실제 버튼을 눌러 확인했습니다. 첫 실행과 별도 프로세스 재시작이 모두 통과했으며 스크린샷 11장을 보존했습니다. 이는 당시 편집 화면의 검증 기록입니다. 이후 전역 대상·추적·전리품·공통 순서·기존 조준과 M01·M02·M03·M04·M05·M06의 핵심 판단을 추가로 연결했습니다.

각 항목의 구체적인 완료 조건은 [남은 개발 작업표](../../Docs/Design/HELLSCRIPT_Remaining_Development_Backlog.md)와 [게임 완성 체크리스트](../../Docs/Design/HELLSCRIPT_Gameplay_Completion_Checklist.md)를 따릅니다. 근거 없는 전체 완성률은 표시하지 않습니다.
