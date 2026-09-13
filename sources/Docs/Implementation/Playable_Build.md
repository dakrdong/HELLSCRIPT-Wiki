# HELLSCRIPT 첫 플레이 빌드

**2026-09-13 직접 플레이 완료 범위:** 주요 기능 구현을 우선하고 세부 수치는 사용자가 플레이하며 조정한다. 이번 작업에서 밸런스 측정은 하지 않는다. 보석 획득·공용 보관·소켓 생성·장착·교체·분리·합성과 소탕 보상을 연결했으며, 재화 회수 칙령의 실제 추적·우선순위 누락을 수정했다. 이전 기록의 보석 보관 방식 확인과 서비스 미연결 표시는 이번 구현으로 대체한다. 50칸·묶음 999개는 조정 가능한 임시 기본값이다. [구현 기록](Playable_Completion.md) · [직접 플레이 안내](Local_Play_Guide.md)

English: prioritize major playable features; the owner tunes details and no balance measurements are performed in this task. Gems now connect acquisition, shared storage, sockets, installation/replacement/removal, fusion and sweep rewards. Resource-loot pursuit and priority gaps are fixed. These supersede earlier pending-storage/service notes; 50 slots and stacks of 999 are provisional defaults. [English record](Playable_Completion.en.md) · [Play guide](Local_Play_Guide.en.md)

**이번 작업의 종료 기준:** 영웅 선택 → 칙령 설정 → 생성 균열·전투·상자 → 보상 → 장비·보석·룬 성장 → 재도전·반복 → 저장·복원으로 직접 플레이할 수 있으면 완료한다. 온라인 서비스·결제·모바일 실기기·출시 리소스와 세부 밸런스는 이번 골의 완료를 지연시키지 않는다. 아래의 지난 검사·실험 기록은 당시 이력이며 재실행 지시가 아니다.

English: this goal ends once the local loop is playable through hero/edict selection, generated rifts, combat/chests, rewards, gear/gem/rune growth, retry/repetition and save recovery. Online services, payments, physical mobile work, release assets and balance are outside this completion target. Historical experiment records below do not instruct another run.

최신 실행본: `Builds/macOS-Playable-Build2/HELLSCRIPT.app`. 상세 조작과 제한은 위의 직접 플레이 안내를 따른다. English: current executable, same path.

**2026-09-13 구현 우선 방침·음향·스킬 연출:** 이후 개발은 미완성 플레이 기능을 먼저 채우고, 확인은 컴파일과 핵심 흐름에 집중합니다. 대규모 회귀 검사와 밸런스 반복 실험은 통합 단계로 미룹니다. 임시 효과음·성소/균열 배경음·기기별 음량 설정과 18개 스킬의 기본 도형·발사체·움직임을 추가했습니다. [개발 기록](Audio_Skill_Presentation.md)

English: prioritize missing gameplay features and limit immediate checks to compilation and critical flows; defer large regression and balance experiments until integration. Temporary audio, sanctuary/rift ambience, device volume settings and primitive-based presentation for all 18 skills are implemented. [English record](Audio_Skill_Presentation.en.md)

갱신일: 2026-09-13

최신 실행본은 `Builds/macOS-TrainingEquipment-Build8/HELLSCRIPT.app`입니다. **2026-09-13 소유 장비 훈련:** A/B 비교에서 가방·공유 창고 장비와 실제 사용 중인 칙령을 시험할 수 있습니다. 계정 룬 보드를 비교에서 빠뜨리던 문제와 B 프리셋에 A 장비를 저장하던 경로를 수정했습니다. 최종 전체 검사 2,515개, 실제 실행·재시작과 화면 13장 검토를 마쳤습니다. 실제 가방·창고·장착·진행도는 유지되며 결과와 프리셋은 직접 저장합니다. [개발 기록](Training_Equipment_Expansion.md)

English: A/B training now compares owned bag/warehouse equipment and the active hunt edict with frozen account rune boards. B presets reference B gear. All 2,515 final tests, native initial/restart checks and 13 screenshots passed; live property and progression remain unchanged. Physical mobile input and natural progression balance remain separate work. [English record](Training_Equipment_Expansion.en.md)

이전 실행본은 `Builds/macOS-IdleMode-Build10/HELLSCRIPT.app`입니다. **2026-09-13 절전 방치:** 절전 화면과 잠깐 보기를 실제 전투·자동 반복·저장 복구에 연결했습니다. 전체 검사 2,471개와 최종 앱의 실행·재시작 검사가 통과했으며 화면 13장을 검토했습니다. 같은 최종 빌드의 절전 측정 3회에서 전투는 계속되고 월드·타격 효과·일반 HUD 갱신은 0회였습니다. Android 전력·발열·실제 터치와 플랫폼별 중단 처리는 후속 작업입니다. [개발 기록](Idle_Display.md)

English: Idle display and peek now preserve live combat, repetition and save recovery. All 2,471 tests and final native initial/restart checks passed; 13 screenshots were reviewed. Three dimmed captures of the same final build continued combat with zero world, transient-effect or normal HUD updates. Android power/thermal/touch and platform interruption work remains. [English record](Idle_Display.en.md)

이전 실행본은 `Builds/macOS-ItemQuality/HELLSCRIPT.app`입니다. **2026-09-13 장비 품질:** 각성·상위 접사·걸작을 균열·상자·소탕, 전투 수치와 대장장이 거래에 연결했습니다. 신규 장비 레벨은 60으로 제한하고 기존 고레벨 장비는 보존합니다. 최종 전체 검사 2,442개와 실제 앱 실행·재시작·복구 3회가 통과했으며 화면 15장을 검토했습니다. 다섯 성장 구간·여섯 빌드의 1,800회 전투 비교를 마쳤습니다. 기본 장비는 2/900판, 품질 장비는 108/900판을 클리어했으며, 90단계는 두 조건 모두 실패해 후속 검토가 필요합니다. [개발 기록](Item_Quality_Expansion.md)

English: awakening, greater affixes and masterworking now connect rift/chest/sweep rewards, combat stats and blacksmith transactions. New items cap at level 60 while legacy higher-level gear is preserved. All 2,442 final tests and three native processes passed; 15 screenshots were reviewed. The 1,800-run comparison completed: baseline gear cleared 2/900 runs and quality gear 108/900. Neither cleared stage 90; further balance review is required. [English record](Item_Quality_Expansion.en.md)

이전 실행본은 `Builds/macOS-GemSockets/HELLSCRIPT.app`입니다. **2026-09-13 보석·소켓 기반:** 보석 7종·6단계의 부위별 효과, 보석 장비의 판매·분해 보호, 저장 복구와 장비 비교를 연결했습니다. 전체 검사 2,380개와 실제 앱 실행·재시작·복구 3회가 통과했고 화면 9장을 검토했습니다. 보석 보관 방식은 사용자 확인 중이며, 획득·장착·분리·합성 서비스 연결은 남아 있습니다. [개발 기록](Gem_Socket_Foundation.md)

English: the latest player includes gem effects, socket protection, persistence/recovery and inspection UI. All 2,380 Editor tests and three native processes passed; nine screenshots were reviewed. Storage ownership awaits the user's choice; acquisition and socket/fusion services remain. [English record](Gem_Socket_Foundation.en.md)

2026-09-13 이전 실행본은 `Builds/macOS-RepeatHunt/HELLSCRIPT.app`입니다. **자동 반복 사냥:** 성공·실패 후 단계와 횟수·시간·획득 목표, 등급별 자동 정리, 가방·저장 오류 대기와 결과 복원을 연결했습니다. 전체 검사 2,198개와 실제 앱 실행·재시작 4회가 통과했으며, 화면 14장을 검토했습니다. [개발 기록](Repeat_Hunt_Expansion.md)

English: the previous player connects repeat outcome policies and attempt/time/acquisition targets, transactional grade-based cleanup, bag/save blockers and saved result recovery. All 2,198 Editor tests and four native launches/restarts passed; 14 screenshots were reviewed. [English record](Repeat_Hunt_Expansion.en.md)

2026-09-13 이전 실행본은 `Builds/macOS-CombatHistory/HELLSCRIPT.app`입니다. **전투 기록 보관:** 최근 10회에 스킬 집계·마지막 5초·당시 규칙과 칙령·종료 장비를 저장하고 다시 조회할 수 있습니다. 자동 반복 대기와 읽던 문단 유지, 다음 균열·캐릭터 변경·재시작을 검증했습니다. 전체 검사 2,156개와 실제 앱 검사 3회가 통과했습니다. [개발 기록](Combat_History_Expansion.md)

English: the latest ten rifts now retain skill totals, the final five-second window, actual rule/edict context and final equipment. Countdown/reading-position behavior and history after another rift, character change and relaunch were verified. All 2,156 editor tests and three native processes passed. [English record](Combat_History_Expansion.en.md)

2026-09-13 이전 실행본은 `Builds/macOS-EdictField/HELLSCRIPT.app`입니다. 룬 성장 완료본을 포함하고, 현재 방 출구 선택과 성소 앞 왕복 문제를 수정했습니다. 균열 대조 실험 360판을 보존했으며, 최신 메인의 전체 검사 2,120개·대표 대조 24판·실제 앱 실행과 재시작 3회·화면 5장을 검증했습니다. 장비·재화·초회 보상과 보스로 얻은 룬이 재시작 후 중복 없이 유지됩니다. [개발 기록](Edict_Field_Expansion.md) · [English](Edict_Field_Expansion.en.md)

English: the latest player includes completed Rune mastery and corrects exit selection and shrine approach loops. The 360-run controlled field comparison is preserved separately. The combined main passed all 2,120 tests, 24 representative comparisons, three native launches/restarts and five screenshot reviews, including persistence of actual boss-reward runes. See the record for outcomes and limits.

2026-09-13 이전 궁수 실행본은 `Builds/macOS-EdictRanger/HELLSCRIPT.app`입니다. 궁수 액티브·기본 사격의 핵심 옵션 30개를 독립 판단으로 연결했습니다. 전체 검사 2,079개와 실제 앱 실행·재시작 13회가 통과했고 화면 31장을 검토했습니다. [개발 기록](Edict_Ranger_Expansion.md) · [English](Edict_Ranger_Expansion.en.md)

English: the previous Ranger player independently applies all 30 Ranger active/BASIC core options. All 2,079 tests and 13 native launches/restarts passed, with 31 screenshots reviewed. See the record for controlled costs, earned charges, persistence, UI evidence and remaining work.

2026-09-13 이전 전사 지원 실행본은 `Builds/macOS-EdictWarriorSupport/HELLSCRIPT.app`입니다. 철벽·전투 함성·전사 기본 공격의 13개 옵션을 연결해 전사 핵심 옵션 30개를 모두 독립 판단으로 적용합니다. 전체 검사 1,981개와 최종 앱의 실행·재시작 5회가 통과했고 화면 17장을 검토했습니다. [개발 기록](Edict_Warrior_Support_Expansion.md) · [English](Edict_Warrior_Support_Expansion.en.md)

English: the previous Warrior support player connects all 13 W05/W06/Warrior BASIC options, completing independent policies for all 30 Warrior core options. All 1,981 tests and five final native processes passed, with 17 screenshots reviewed. Only `main` remains locally and remotely. See the record for the controlled verification scope and remaining work.

2026-09-13 이전 지면 강타 실행본은 `Builds/macOS-EdictSlam/HELLSCRIPT.app`입니다. 전사 지면 강타의 네 옵션, 실제 기절·보스 제압·생존 순서와 저장 복원을 연결했습니다. 전체 검사 1,903개와 최종 앱의 실행·재시작 3회가 통과했고 화면 13장을 검토했습니다. [지면 강타 개발 기록](Edict_Slam_Expansion.md) · [English](Edict_Slam_Expansion.en.md)

English: the previous Ground Slam player connects all four W04 options, actual stun/boss stagger, survival order and persistent preparation. All 1,903 tests and three final native processes passed, with 13 screenshots reviewed. The checkout and only remaining remote branch are `main`.

2026-09-13 이전 전사 공격 실행본은 `Builds/macOS-EdictWarriorAttacks/HELLSCRIPT.app`입니다. 전사 회오리·도약·분쇄의 핵심 옵션 13개, 실제 비용·충전·착지 후 공격과 저장 복원을 연결했습니다. 전체 검사 1,847개와 실제 실행·재시작 11회가 통과했습니다. [전사 공격 개발 기록](Edict_Warrior_Attacks_Expansion.md) · [English](Edict_Warrior_Attacks_Expansion.en.md)

English: the previous Warrior build connects all 13 W01-W03 core options, actual costs/charges, landing preference and persistence. All 1,847 tests and 11 native processes passed; 24 paired runs are functional observations, not a balance improvement claim.

2026-09-13 이전 실행본은 `Builds/macOS-EdictMageBasic/HELLSCRIPT.app`입니다. 클로드의 능력치·접사 통합 위에 번개탄의 네 옵션과 실제 적중·자원·절제·원소 연계, 두 단계의 저장 복원을 연결했습니다. 전체 검사 1,765개와 실제 앱의 실행·재시작 17회가 통과했습니다. 번개탄 화면 13장을 검토했고 기존 액티브의 자동 화면 검사 66장도 통과했습니다. 전후 48판은 클리어 0회였으므로 밸런스 개선으로 판단하지 않습니다. [번개탄 개발 기록](Edict_Mage_Basic_Expansion.md) · [English](Edict_Mage_Basic_Expansion.en.md)

English: the latest Mage BASIC player preserves Claude's integration and connects all four Lightning Bolt policies, actual-hit rewards and two independent restarts. All 1,765 tests and 17 native processes passed, with 13 visually reviewed basic screenshots and 66 automated active-regression screenshots. The 48-run comparison produced zero clears; no balance improvement is claimed.

2026-09-13 이전 서리 폭발 실행본은 `Builds/macOS-EdictNova/HELLSCRIPT.app`입니다. 클로드의 능력치·접사 통합을 유지하고 서리 폭발의 사용 목적·접근·보스 정책과 실제 충전 연계를 연결했습니다. 전체 검사 1,705개와 실제 앱의 세 프로세스 검사가 통과했고, 화면 13장을 보관했습니다. 전후 48판에서는 생존 설정의 M03 중단이 8→68회로 늘고 보스 도달이 줄었습니다. 이 결과를 밸런스 개선으로 판단하지 않으며 제어 전환과 공격 취소 비용을 이어서 조사합니다. [서리 폭발 개발 기록](Edict_Nova_Expansion.md) · [English](Edict_Nova_Expansion.en.md)

English: the latest build preserves Claude's attribute/affix integration and connects M06 purpose, approach, boss policies and actual chain effects. All 1,705 tests and three native processes passed, retaining 13 screenshots. Across the 48-run comparison, configured-survival M03 interruptions increased from eight to 68 and boss spawns fell. Control replacements and cancelled preparation remain follow-up work; no balance improvement is claimed.

2026-09-13 이전 보호막 실행본은 `Builds/macOS-EdictShield/HELLSCRIPT.app`입니다. 클로드의 능력치 통합 위에 원소 보호막의 네 핵심 옵션과 교전 시작 기록을 연결했습니다. 전체 검사 1,652개와 실제 앱의 세 프로세스 검사가 통과했고, 화면 13장을 보관했습니다. 전후 48판에서는 생존 설정의 시전 중단이 줄었지만 모두 시간 초과했으므로 밸런스 개선으로 판단하지 않습니다. [원소 보호막 개발 기록](Edict_Shield_Expansion.md) · [English](Edict_Shield_Expansion.en.md)

English: the preceding shield build connects M05's four core policies and persistent encounter openings on top of Claude's attribute integration. All 1,652 tests and three native processes passed, with 12 screenshots. In 48 before/after runs, configured-survival interruptions fell but all eight configured cases still timed out; no balance improvement is claimed.

2026-09-13 이전 순간이동 실행본은 `Builds/macOS-EdictTeleport/HELLSCRIPT.app`입니다. 순간이동의 생존·거리 조정·착지 기준을 독립 판단에 연결했습니다. 전체 검사 1,601개와 실제 앱의 저장·재시작·적 없는 환경 회피가 통과했습니다. 전후 32판 진단에서 기본값 칙령의 연쇄 번개 중단은 줄었지만 사망은 늘었습니다. 생존 대응을 켜지 않은 기본 설정의 결과이며 밸런스 개선으로 판단하지 않습니다. [순간이동 개발 기록](Edict_Teleport_Expansion.md) · [English](Edict_Teleport_Expansion.en.md)

English: the preceding Teleport build connects M04 survival, distance adjustment and landing policies. All 1,601 tests and native save/restart/targetless escape checks passed. In 32 before/after runs, default-edict interruptions fell but deaths rose with survival responses disabled; this is not a balance improvement claim.

2026-09-13 연쇄 번개 실행본은 `Builds/macOS-EdictChain/HELLSCRIPT.app`입니다. 연쇄 번개 사냥 칙령의 네 옵션과 첫 대상의 발사 시점 검사를 연결했습니다. 전체 검사 1,566개와 실제 앱의 설정 저장·재시작이 통과했고, 한국어·영어와 140% 글자 화면 10장을 보관했습니다. 선택한 16판의 연쇄 제어 진단도 완료했습니다. [연쇄 번개 개발 기록](Edict_Chain_Expansion.md) · [English](Edict_Chain_Expansion.en.md). 아래 실행본과 수치는 이전 단계의 증거입니다.

English: the preceding Chain Lightning build connects all four options and revalidates its first target at release. The 1,566-test suite and native save/restart passed. Sixteen outcome-selected diagnostic runs are complete; see the linked record for scope and remaining balance work.

2026-09-12 화염구 연결 실행본은 `Builds/macOS-EdictFireball/HELLSCRIPT.app`입니다. 클로드의 능력치·접사 통합 위에 화염구의 사냥 칙령 네 옵션을 연결했습니다. 전체 검사 1,461개, 마지막 부제 수정의 번역 검사 43개와 새 앱 실행·재시작 2회가 통과했습니다. [화염구 개발 기록](Edict_Fireball_Expansion.md) · [English](Edict_Fireball_Expansion.en.md). 아래는 이전 단계별 증거입니다.

2026-09-12 능력치 통합 후 접사 생성 검사를 추가했습니다. 54종·24종 비교 사본에서 총 1,920만 개 생성과 분포 검사 12,864개가 통과했습니다. 대조 실험은 화염구 연결 이전의 능력치 통합 코드를 사용했고, 20단계 전투 비교 1,200판도 완료했습니다. 현재 54종은 188/600회, 비교용 24종은 212/600회 클리어했습니다. [완료 결과와 남은 검증](Affix_Audit_Expansion.md) · [English](Affix_Audit_Expansion.en.md)

2026-09-12 이전 능력치 통합 실행본은 `Builds/macOS-AttributeIntegration/HELLSCRIPT.app`이다. 클로드의 능력치 57개·접사 54종에 최근 균열 목표·보상·콘텐츠 해금 작업을 합쳤다. 이동 속도 단위, 최대 자원 게이지, 성장·능력치 화면에서 전투로 돌아갈 때의 정지 상태와 큰 영어 글자를 보완했다. 전체 검사 **1,391개**, 실제 앱 실행·재시작 **9회**가 통과했고 화면 **47장**을 보관했다. [능력치 통합 기록](Attribute_Integration_Expansion.md)과 [English](Attribute_Integration_Expansion.en.md)에서 검증 범위와 남은 항목을 확인한다. 아래 실행본은 이전 단계의 증거다.

2026-09-12 목표·보상·결과 화면을 함께 검증한 실행본은 `Builds/macOS-RiftIntegration/HELLSCRIPT.app`이다. 정수·제물 목표에 단계별 장비 등급 확률과 고정 재설정 비용을 통합했다. 생존 귀환 결과의 사망 분석 버튼과 저장 로그의 영어 표시를 수정했다. 전체 검사 1,325개와 앱 실행·재시작 7회가 통과했으며, 화면 30장을 보관했다. [통합 구현과 검증 범위](Rift_Integration_Expansion.md)를 확인한다. 진행 중인 콘텐츠 해금 작업과 클로드의 능력치 변경은 이 실행본에 포함하지 않았다.

클로드의 능력치·밸런스 변경은 2026-09-12 21:07:30 KST에 `main`의 `b1f3ea5`까지 이미 병합되어 있다. 57개 능력치와 54종 접사, 능력치 화면, 밸런스 보고서와 미결 항목을 개발 범위에 포함했다. 현재 개발 브랜치와의 통합·수정 검증은 [클로드 완료본 인수 기록](Rift_Integration_Expansion.md)을 따른다.

직전 목표 기능 실행본은 `Builds/macOS-ObjectiveChains/HELLSCRIPT.app`이다. 새 균열의 정수 운반자 처치·제물 회수와 제단 전달을 자동 진행에 연결하고, 목표 진행도와 관문 개방 시간을 저장·결과 화면에 반영했다. 전체 검사 1,287개, 실제 지도 1만 개 생성, 앱 실행·재시작 5회가 통과했다. [목표 확장 구현과 검증 범위](Objective_Chains_Expansion.md)를 확인한다. 이 실행본에는 별도 작업의 단계별 보상 확률·재설정 비용 변경과 클로드의 능력치·밸런스 작업을 포함하지 않았다.

직전 관문 통행 실행본은 `Builds/macOS-GatePassage/HELLSCRIPT.app`이다. 5단계 이상 새 균열의 모든 보스 출입구에 실제 관문을 추가했다. 봉인 해제·게이지 충전으로 길이 열리고, 관문 모델·지도 표식·열림 알림과 저장 복원을 연결했다. 전체 검사 1,261개와 실제 실행·재시작 검사 3회가 통과했다. [관문 통행 구현과 검증 범위](Gate_Passage_Expansion.md)를 확인한다. 구형 진행 중 저장과 별도 클로드 작업 폴더의 능력치·밸런스 작업은 유지했다. 아래 내용은 각 단계의 기록이다.

직전 대상 선택 실행본은 `Builds/macOS-EdictTargets/HELLSCRIPT.app`이다. 사냥 칙령의 HP·특수 대상 우선순위, 시야 상실·추적 한도와 저장 복원을 연결하고, 영어 140% 글자 크기의 칙령 화면을 보완했다. [대상 선택·추적 구현과 검증 범위](Edict_Targeting_Expansion.md)를 확인한다. 전투·저장 코드는 전체 검사 1,243개가 통과했으며, 이후 화면 변경을 포함한 해당 실행본은 실행·재시작 검사 4회를 통과했다.

직전 검토 빌드는 `Builds/macOS-ClaudeReview/HELLSCRIPT.app`이다. 클로드가 반영한 변경을 기준으로 장비 회수 정책, 봉인석 표현과 지도 복귀를 보완했다. [검토 범위·검증·남은 작업](Claude_Review_20260912.md)을 확인한다.

작성일: 2026-09-08

2026-09-09 최신 화면 대응 빌드는 `Builds/macOS-ScreenLayout/HELLSCRIPT.app`이다. [전투 HUD·카메라 배치](Battle_Layout_Expansion.md)와 [현재 배속 잠금](Speed_Access_Expansion.md)을 포함한다. 기존 `Builds/macOS/HELLSCRIPT.app`은 교체하지 않았다. 아래 첫 빌드의 ‘세 배속 제공’은 과거 기록이며, 최신 빌드에서는 1배속만 사용할 수 있다. 전체 화면의 가로·세로 대응과 모바일 실기기 검증이 끝났다는 의미는 아니다.

이 문서는 첫 빌드 시점의 기록이다. 이후 추가한 기능과 검증은 [장비 확장 개발 기록](Itemization_Expansion.md), [자동 생성 균열·상자 개발 기록](Rift_Expansion.md), [성소·저주 상자 개발 기록](Field_Expansion.md), [전투 동작·투사체 개발 기록](Action_Expansion.md), [행동 규칙 편집·판단 개발 기록](Rule_Expansion.md)을 확인한다.

## 실행

Unity 6000.6.0f1에서 `Assets/HELLSCRIPT/Scenes/Hellscript.unity`를 열고 Play를 누른다. 메뉴 `HELLSCRIPT > 게임 시작`으로도 실행할 수 있다. `HELLSCRIPT > 프로젝트 구성`은 씬·카탈로그·이미지 가져오기 설정을 구성하며 같은 자산을 중복 생성하지 않는다.

macOS 개발 실행 파일의 출력 위치는 `Builds/macOS/HELLSCRIPT.app`이다. 실제 빌드·실행 검증 결과는 `Artifacts/Validation`의 로그와 보고서를 따른다.

## 이번 구현의 범위

확정 기획서 v1.1을 기반으로, 플레이 흐름과 기술 구조를 검증하는 첫 구현이다. 공개 얼리 액세스 전체 범위를 완료한 빌드가 아니다.

| 영역 | 현재 구현 |
|---|---|
| UI | 한국어 세로 화면, 생성 배경·아이콘, 성소, 직업 선택, 단계 선택, 전투 HUD, 결과, 장비·상점·훈련·로그 화면 |
| 직업·성장 | 3직업, 레벨 1–30, 스킬 해금, 경험치, 직업별 영웅·가방, 계정 공용 재화·창고 |
| 행동 | 스킬별 순서·사용 여부·조건 종류·수치·공격/회피 위치, 목표 우선순위, 접근·선회·거리 유지·제자리, 물약, 추천 설정과 저장 슬롯 |
| 전투 | 기본 공격, 18개 액티브의 주요 행동, 이동·범위·지속 효과, 자원·재사용 대기시간·보호막·자동 회복 |
| 균열 | 연결된 8개 방의 고리 지도, 2개 테마 색상, 무작위 적 배치, 처치 게이지 100, 보스, 300초, 실패 후 보상 유지, 자동 반복 |
| 배속 | 1배·1.5배·2배, 같은 전투 시간 단위, 일시정지·편집·포탈 정지 |
| 장비·경제 | 베이스 24종, 접사 24종의 부위 필터·추첨, 전설·세트 후보 풀, 장착·잠금·강화·분해·판매·재설정·제작·미확인 상점 |
| 보상·저장 | 캐릭터별 초회, 소탕 공간 검사·요청 중복 방지, 로컬 저장·백업·균열 상태 복원, 개발용 미실행 보상 |
| 표현 | 자체 제작 기본 메시 방·기둥·화로, 골격 없는 단순 영웅·적 몸체, 회전·이동·흔들림, 타격·장판·예고 표시 |

## 공개 버전까지 남은 항목

후속 D3의 피해 계산·보호막·상태 출처·보스 제압 변경과 실제 실행 검증은 [전투 효과 통합 기록](Effect_Expansion.md)에 이어서 기록한다.

세트 충전·비용 감소의 소비 시점과 이후 실행 검증은 [충전·비용 효과 개발 기록](Charge_Expansion.md)을 따른다.

전염의 원 표적 판정, 보스 제어의 만료·구형 저장, 같은 순간에 발생한 사망의 정산은 [전염·사망 정산 개발 기록](Outcome_Expansion.md)에 정리했다.

눈보라의 설치·고정/추적 설정과 맹독·서리 세트의 시간 처리는 [장판·세트 개발 기록](Area_Expansion.md)을 확인한다. 해당 후속 빌드는 검사 256개와 실제 재시작 2회를 통과했다.

일반 몬스터 12종·정예 특성 6종의 개별 행동, 위험 형상과 상태 화면은 [적 콘텐츠 개발 기록](Enemy_Expansion.md)을 따른다. 해당 후속 빌드는 검사 303개와 돌진·사망 효과의 실제 재시작 2회를 통과했다. 정식 보스 패턴과 아래 첫 빌드 당시의 목록을 구분한다.

보스 3종의 전용 패턴·후반 단계 전환·피난 경로와 보스 HUD는 [보스 개발 기록](Boss_Expansion.md)에 이어서 정리했다. 해당 빌드는 검사 341개, 실제 재시작 4회와 전체 균열 클리어를 통과했다. 아래 첫 빌드 시점의 ‘보스 세부 패턴 미완료’ 기록은 이 후속 개발과 구분한다.

패시브 설명·버프 표시·시간과 HP 경계·원소 교차 저장의 후속 변경은 [패시브 개발 기록](Passive_Expansion.md)을 따른다. 이 단계부터 전투 효과 저장 버전은 5다.

실제 8부위 장비의 빌드 비교, 벽 모서리 이동과 서리 장판 추천 규칙 보완은 [빌드 통합 개발 기록](Build_Integration_Expansion.md)을 따른다. 현재 이어서 만들 기능과 완료 기준은 [남은 개발 작업표](../Design/HELLSCRIPT_Remaining_Development_Backlog.md)에 모았다.

전사 전설의 강제 이동·착지·단일 표적 기준과 전체 검사 416개·실제 재시작 1회의 후속 결과는 [전사 전설 통합 기록](Legendary_Expansion.md)을 따른다.

현재 레벨과 소유 장비를 사용하는 일반 훈련·개발용 전체 스킬 시험의 분리, 60초 종료와 설정 저장은 [실제 캐릭터 훈련 기록](Player_Training_Expansion.md)을 따른다. 해당 후속 빌드는 검사 456개와 실제 재시작 1회를 통과했다. 아래의 Lv.30 훈련 설명은 첫 빌드 당시 기록이다.

전투 중 레벨업의 HP 비율 유지·성장 기록, 현재 레벨 추천안의 미리보기·적용은 [성장 개발 기록](Growth_Expansion.md)을 따른다. 전체 검사 514개와 실제 앱의 균열 처치·재시작·해금 스킬 연결을 확인했다. 첫 15분 안내와 장기 성장 밸런스까지 완료했다는 의미는 아니다.

첫 안내의 아홉 단계, 숨기기·재열람, 실제 소유 장비 비교와 설정 수정·훈련·재도전 연결은 [첫 플레이 개발 기록](First_Play_Expansion.md)을 따른다. 전체 검사 542개 이후 안내 문구와 스크롤 검증을 다듬고, 해당 macOS 빌드의 독립 실행·실제 재시작과 화면 12개를 확인했다. 사람의 첫 15분 이해도 관찰은 남아 있다.

이후 추가한 같은 조건의 행동 A/B, 스킬별 전체 집계와 명시적 결과·설정 저장은 [훈련 비교 개발 기록](Training_Comparison_Expansion.md)을 따른다. Core 검사 611개 이후의 UI 변경은 별도 빌드 `Builds/macOS-Comparison/HELLSCRIPT.app`에서 확인한다. 성소의 **고정 훈련장 → 같은 조건으로 A/B 비교**로 진입하며, 장비 교체 훈련과 사냥 칙령의 5개 슬롯·공유 코드는 후속 범위다.

기존 로컬 프리셋을 5칸으로 늘린 뒤 이름 저장·변경·실패 후 재시도를 연결했다. [사냥 칙령 개발 기록](Hunt_Edict_Expansion.md)과 [화면 배치 개발 기록](Screen_Layout_Expansion.md)에 새 실행본과 검증 범위를 정리한다. 핵심 옵션 v0.2와 공유 기능, 모바일 방향 설정은 아직 개발 중이다.

사냥 설정의 적용을 파일 저장 성공 뒤로 옮기고, 실패 시 편집 내용과 실제 전투 상태를 유지하도록 보완했다. 전체 검사 696개 이후의 macOS 화면·재시작 검증은 [현재 설정 저장 개발 기록](Current_Build_Save_Expansion.md)을 따른다.

설정이 실제로 달라진 저장에서도 시작한 공격의 원래 설정을 유지하도록 보완했다. 회오리 종료 요청의 저장·복원과 후속 검증은 [전투 동작 보존 기록](Action_Continuity_Expansion.md)을 따른다.

핵심 옵션 89개와 HED2 원본·공유 형식·부분 가져오기·구형 간소화 미리보기를 추가했다. 전체 검사 780개와 연결 전 범위는 [사냥 칙령 핵심 옵션 v0.2 기록](Hunt_Edict_V2_Expansion.md)을 따른다. 이 단계에서는 새 화면이나 실행 빌드를 추가하지 않았다.

기존 전투의 명중 예측과 실제 연쇄의 대상 선택 기준을 맞추고, 새 핵심 옵션 중 네 스킬의 조준 후보 계산을 추가했다. 실제 연결 범위와 후속 검증은 [조준·명중 예측 기록](Edict_Aim_Expansion.md)을 따른다.

예고된 공격·투사체·장판의 피해와 보호막 만료를 반영하는 예상 HP 손실, 새 전역 회피·긴급 대응 판단을 추가했다. 전체 검사 871개와 실제 실행 연결 전 범위는 [회피·긴급 생존 기록](Edict_Survival_Expansion.md)을 따른다. 이 단계는 실행 중인 앱에 새 설정 화면을 추가하지 않는다.

생존 요청의 실제 시전·보행·물약·동작 중단과 저장 상태를 연결한 후속 변경은 [생존 행동 실행 기록](Edict_Response_Expansion.md)을 따른다. 전투 동작 저장 버전은 5이며, 실행 중인 기존 앱에 새 설정을 공개한 단계는 아니다.

상단 **설정·안내**와 공통 안내의 가로·세로 배치, 기기별 방향 설정·실패 처리·전투와 편집 상태 보존은 [화면 설정·공통 안내 기록](Screen_Settings_Expansion.md)에 정리했다. 새 macOS 실행본은 `Builds/macOS-ScreenSettings/HELLSCRIPT.app`이며, 기존 실행 중인 앱을 교체하지 않았다. 다른 주요 화면의 전체 재배치와 모바일 실기기 검증은 남아 있다.

후속 개발의 상세 규칙과 우선순위는 [후속 개발 상세 기획 v1.2](../Design/HELLSCRIPT_Development_Expansion_Plan.md)에 정리했다. 접두사·접미사와 빌드 장비, 자동 생성 균열·상자, 전투·성장·화면의 완료 기준을 포함한다. 그 문서의 기능을 이번 첫 빌드에 구현했다고 해석하지 않는다. 아래 목록은 실제 구현 상태의 기록으로 유지한다.

- 현재 행동 편집기는 10개 주요 조건을 제공한다. 전체 22조건, OR 2묶음 × AND 3조건, 스킬별 3규칙의 완전한 편집·검증은 확장 대상이다.
- 훈련은 개발 편의를 위해 레벨 30 능력치와 해금 스킬을 시험한다. 실제 영웅의 레벨·보유 장비·재화에는 반영하지 않는다. 정식 기획의 미해금 제한을 따르는 플레이어용 훈련과 개발용 전체 스킬 시험은 이후 분리해야 한다.
- 12개 방 템플릿의 서로 다른 장애물·연결구·배치, 12종 적의 모든 개별 패턴, 6개 정예 효과·보스 세부 패턴은 현재 공통 동작을 바탕으로 추가 구현해야 한다. 현재 지도는 통로가 연결된 고리 구조다.
- 전설·세트의 추첨과 일부 효과는 작동하지만, 모든 효과의 기준 시점·내부 대기시간·연쇄 제약을 최종 명세와 대조하지 않았다. 특히 LM04 재방문, 일부 패시브, 보스의 정식 제압 게이지, 덫의 무장·소모 규칙은 미완료다.
- 도약·순간이동은 전투 위치를 즉시 바꾸고 표현 위치를 보간한다. 정식 도약 준비·체공·착지와 행동 중단 정책은 추가 구현해야 한다.
- 재설정은 첫 접사 위치를 사용한다. 첫 재설정 때 플레이어가 원하는 접사 위치를 선택하는 UI는 추가해야 한다.
- 실제 계정 로그인, 온라인 세션 권한, 서버 시각, 기기 간 교차 저장, 영수증 검증·결제·환불은 연결되지 않았다. 현재 로컬 구현을 실제 온라인 서비스로 간주하지 않는다.
- 로컬 미실행·일일 소탕 정산은 기기 UTC 시각을 사용한다. 서버 구현 전에는 시계 변조 방지와 여러 기기의 중복 정산을 보장하지 않는다. 미실행 재화의 소수 이월도 추가 대상이다.
- 진행 중 균열의 주요 상태는 저장하지만 일부 전설 내부 타이머 등은 아직 실행 객체에만 있다. 모든 효과의 완전한 중단 복원은 별도 검증이 필요하다.
- 보석은 2.0.1 공식 표 대조가 완료되지 않아 이번 빌드에 넣지 않았다. 임의의 혼합 버전 데이터로 대체하지 않는다.
- 성소는 현재 생성 배경을 사용하는 메뉴다. 걸어 다니는 마을과 4개 NPC는 후속 장면 제작 대상이다.
- 가방은 기본 탭의 50→100칸 골드 확장과 기본 공유 창고 400칸까지 지원한다. 유료 추가 탭과 프리셋 참조 장비의 자동 보호는 추가 구현해야 한다.
- Android 모듈이 설치되지 않은 환경에서 작업했다. Android 실행 파일·실기기 프레임률·발열·터치·한국어 글꼴을 검증한 상태가 아니다. macOS 실행 검증과 구분한다.

## 구조와 소유자

- `Runtime/Core/GameCatalog.cs`: 직업·스킬·저장 자료 구조와 기본 콘텐츠 정의. Unity 에셋은 `Resources/GameCatalog.asset`이다.
- `CombatSimulation.cs`: 상태를 전투 시간으로 진행한다. `RiftMap.cs`가 이동 가능한 영역과 연결을 판단한다.
- `Economy.cs`: 아이템·강화·분해·경험치·소탕 규칙을 처리한다.
- `GameStore.cs`: 로컬 개발용 저장 어댑터다. 서버 계정 구현의 권위 저장소와는 구분한다.
- `GameController.cs`: 실행 수명·배속·저장 시점·화면 전환을 조정한다.
- `WorldView.cs`: 임시 3D 몸체·방·이펙트를 표현한다. 전투 결과를 판정하지 않는다.
- `GameUI.cs`: 화면과 입력을 구성한다. 전투 중 장착 변경을 막고 설정 편집은 일시정지한다.
- `Editor/ProjectBuilder.cs`: Unity API로 에셋·씬을 만들고 개발 빌드를 생성한다.

## 검증 산출물

- `Artifacts/Validation/setup.log`: Unity 컴파일과 에셋·씬 생성.
- `Artifacts/Validation/editmode.xml`: 핵심 규칙 자동 검사 결과.
- `Artifacts/Validation/build-macos.log`: macOS 개발 빌드 결과.
- `Artifacts/Validation/runtime-smoke.txt`와 화면 PNG: 실제 플레이어 실행 점검 결과. 해당 파일이 만들어졌을 때만 통과한 것으로 취급한다.

기존 사용자 변경인 Editor 파일 삭제와 패키지·품질·설정 변경을 되돌리지 않았다. URP manifest의 17.7.0 지정은 설치된 Unity에 포함된 17.6.0과 일치시켰다. 새 Unity 패키지는 추가하지 않았다.
