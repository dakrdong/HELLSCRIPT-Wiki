# 운영 아이템과 보상 준비

갱신일: 2026-09-26 · [English](Operations_Rewards.en.md)

디아블로 이모탈·2·3·4의 **운영에 쓰이는 기능별 아이템 유형**을 HELLSCRIPT의 실제 저장·거래·소비처에 맞춰 정리했다. 모든 무기·전설 이름이나 각 시즌의 일회성 아이템을 그대로 복제한 목록은 아니다. 아래 공식 자료의 당시 사례를 참고한 설계이며, 2026년 모든 게임의 최신 시즌 수량·가격·드롭률을 검증한 자료도 아니다. HELLSCRIPT의 이름·아트·수치는 자체 정의를 사용한다.

## 구현 결과와 활성화 범위

| 구분 | 수량 | 현재 상태 |
| --- | ---: | --- |
| 기존 지급 상자 | 96종 | 기존 정의와 균열 최초 보상표를 유지한다. |
| 새 지급 상자 | 52종 | 기본 물약 8종, 보석 영약 36종, 희귀/전설 부위 선택 2종, 세트 머리/장갑/신발 3종, G1/G2/G3 룬 3종이다. |
| 전체 상자 | 148종 | `RewardBoxCatalog`와 기존 보상 보관함에서 사용한다. 52종은 기존 물약·장비·룬을 포장하는 지급품이며, 새 전투 효과 52개를 의미하지 않는다. |
| 운영 패키지 | 22개 | 명시적 지급 거래와 검토용 운영 설정 구성을 준비했다. 구성 수량은 초기 운영 템플릿이며 장기 경제 밸런스가 확정된 값은 아니다. |
| 예비 항목 | 30개 | 이름·목적·필요 소비처를 데이터에 등록했다. 지급·사용·소유 지갑은 구현하지 않았으며 지급 함수가 거절한다. |

신규·기존 계정에 자동으로 품목을 배포하지 않는다. 기본 출석·드롭·최초 클리어 일정도 변경하지 않는다. 준비된 패키지는 일일/주간이라는 이름만으로 날짜나 자격을 자동 판정하지 않는다. 실제 캠페인의 기간·대상·일일 한도는 호출하는 콘텐츠가 소유해야 한다.

## 네 게임에서 가져올 기능과 적용 판단

| 운영 기능 | 참고 사례 | HELLSCRIPT 반영 |
| --- | --- | --- |
| 공통 화폐 | 네 게임의 골드 | 기존 `gold`와 골드 상자를 사용한다. |
| 행사 보상 화폐 | 이모탈의 Hilts·행사 교환 증표, D3 핏빛 파편 | 즉시 보상은 기존 심연 주화로 지급한다. 행사·공적 증표는 별도 소비처가 생길 때 사용한다. |
| 거래·구매 화폐 | 이모탈 백금·영원의 보주, D4 백금 | 거래소·구매 전용 재화는 예비 상태다. 현재 무료 심연 주화를 구매 잔액으로 해석하지 않는다. |
| 일반/희귀 제작 재료 | D3 제작/막별 재료, D4 분해 재료, 이모탈 고철·가루 | 고대의 요석(`materials`)·강화석을 재사용한다. 유사 재화를 여러 개 만들지 않는다. |
| 부위별 확정 제작 | D2 제작 조합, D3 큐브 제작 | 기존 부위별 코어와 코어 선택 상자를 활용한다. |
| 보석과 합성 | D2 보석·주얼, D3 보석 변환, D4 보석 | 기존 6종·6단계 보석과 보석 선택 상자를 사용한다. 무작위 주얼은 별도 전투 체계이므로 추가하지 않는다. |
| 룬과 성장 | D2 룬·룬워드, 이모탈 룬 | 기존 룬 보드에 맞춘 G1~G3 지급 상자를 보완한다. D2 룬워드 시스템을 이식하지 않는다. |
| 물약·영약 | D2 회복/재생 물약, D4 엘릭서 | 기존 물약 44종을 지급 상자로 만들었다. 효과는 기존 `PotionCatalog`와 `GemElixirs`가 소유한다. |
| 전설·세트·선택 보상 | D2 세트/고유, D3 보관함, 이모탈 행사 장비 | 부위 선택 상자와 세트 지급 부위를 보완한다. 부위는 선택하지만 옵션·전설 효과·세트 종류는 기존 생성 규칙을 따른다. |
| 위상·전설 효과 수집 | D3 전설 능력 추출, D4 위상 도감 | 기존 위상 소유자를 유지하며 별도의 위상 조각 지갑을 만들지 않는다. |
| 강화·재련·소켓 촉매 | D2 소켓 조합, D3 큐브, D4 담금질/명품화 | 기존 기능은 유지한다. 비용 대체권·촉매·비전서는 예비 정의와 소비처 조건을 남겼다. |
| 누적 교환과 확정 획득 | 이모탈 진주·행사 교환 재료 | 기존 코어와 역할이 겹치는 확정 제작 파편은 예비로 둔다. 획득량·천장을 임의로 확정하지 않는다. |
| 입장 열쇠·소환 재료 | D2 열쇠/장기, D3 대균열석/지옥불 장치, D4 소환 재료, 이모탈 열쇠/문장 | 도전 열쇠·토벌 봉인·조각·보물고 열쇠·변이 인장을 예비 등록했다. 입장 실패 환급과 보상 소유권이 선행 조건이다. |
| 시즌·패스 진행 | D4 Favor/Smoldering Ashes, 이모탈 패스 포인트 | 시즌 증표·여정 포인트·축복은 예비다. 시즌 분리·만료·이전·중복 수령 규칙이 필요하다. |
| 성장 보정·복귀 | 이모탈 복귀 보상과 성장 여정 | 복귀 보급 패키지를 준비했다. 레벨을 자동 상승시키거나 복귀 자격을 임의 판정하지 않는다. |
| 수리·귀환·감정·탄약 | D2 큐브 수리·스크롤·탄약 | 현재 게임에 비용·내구도·미감정·탄약 소비처가 없으므로 불필요한 소모품을 추가하지 않는다. |
| 초기화·시간·공간 이용권 | D4 초기화 두루마리, HELLSCRIPT 기존 피로도·대장간·보관함 | 무료 편집 기능은 유지한다. 시간·공간 이용권은 기존 거래에 비용 대체 규칙을 추가한 뒤 활성화한다. |
| 부적·전설 보석·동료 | D2 부적, 이모탈 전설 보석·동료 | 기존 룬·위상과 역할을 비교하도록 예비 정의했다. 전투 능력을 새로 추가한 상태는 아니다. |
| 외형·칭호·사회 보상 | 이모탈 패스 외형, D4 외형 보상 | 외형/칭호 교환권은 예비다. 길드·대전 증표는 HELLSCRIPT의 확장 제안이며 해당 콘텐츠의 구현을 뜻하지 않는다. |
| 점검·장애·기념일 보상 | 위 사례들의 보상 포장·선택·지급 패턴을 응용 | 점검·장애·참여·완주·기념일 패키지를 별도로 준비했다. 특정 원작에 같은 이름의 상자가 있다는 뜻은 아니다. |

## 사용과 소유권

원본은 [보상 상자](../../Assets/HELLSCRIPT/Resources/Data/RewardBoxes.json), [운영 패키지](../../Assets/HELLSCRIPT/Resources/Data/OperationsRewards.json)다. `python3 tools/operations_rewards.py list`로 전체 항목을 조회하고 `check`로 계약을 검사한다.

```sh
python3 tools/operations_rewards.py preview --package maintenance-v1 --stage 1
```

위 명령은 운영툴의 최초 보상 설정에 사용할 **검토용 행**만 출력한다. 서버 게시·플레이어 지급은 실행하지 않는다. 최초 보상은 원래부터 계정·단계당 1회이므로, 이미 수령한 플레이어의 점검 보상 발송 수단으로 사용해서는 안 된다. 갱신된 클라이언트와 서버 카탈로그가 배포된 뒤 신규 최초 보상 구성에 활용할 수 있다. 구버전 클라이언트에 새 상자를 먼저 게시하지 않는다.

콘텐츠 코드는 `GameStore.GrantOperationsPackage(deliveryId, packageId, sourceStage)`를 호출할 수 있다. 같은 계정의 같은 지급 ID를 재시도해도 상자를 다시 지급하지 않는다. 다른 구성·단계로 ID를 재사용하면 거절한다. 지급 ID는 요청마다 새로 만드는 난수가 아니라 **계정의 수령 자격을 식별하는 고정 ID**여야 한다. 예를 들어 `maintenance-20260926`처럼 캠페인과 자격을 결합한다.

1단계는 신규 계정의 기본 지급 기준이다. 2단계 이상은 계정의 최고 정상 클리어 기록을 넘겨 지정할 수 없으며, 패키지별 최소 지급 단계도 검사한다. 전설은 30단계부터 지급하며 진행 중인 균열에서는 지급·개봉을 막는다. 거래가 디스크 저장에 성공한 뒤 보관함을 갱신한다. 지급 시 가방 공간을 요구하지 않고 상자로 보관하며, 개봉 시 물약 9,999개 상한·장비 가방·보석 보관함 공간을 검사한다. 실패하면 상자와 잔액을 그대로 보존한다. 소비한 상자가 없어져도 지급 거래 기록은 남는다.

보상 상자·골드·재료·보석·룬은 기존 계정 소유 구조를 쓴다. 장비와 물약은 **개봉하는 캐릭터**가 받는다. 물약은 지급만 하며 자동으로 사용하거나 장착하지 않는다. 장비는 기존 생성기·품질·공통 상세 표시를 사용한다. 원본 상자 ID와 패키지 ID는 지급 후 내용이 바뀌지 않도록 유지하고 변경 구성은 새 `-v2` ID로 추가해야 한다.

이 지급 경로는 기존 **로컬 계정 저장 거래**다. 운영자 인증, 서버 판정, 우편함, 쿠폰 발급, 전 계정 발송, 여러 기기 사이의 중복 지급 방지를 구현한 서버 API가 아니다. Google 로그인도 이 로컬 거래를 서버 권한으로 바꾸지 않는다. 실제 외부 운영 발송은 인증·대상·자격 판정과 서버 원장을 연결해야 한다.

## 준비된 운영 패키지

| ID | 용도 | 최소 지급 기준 단계 | 상자 구성 |
| --- | --- | ---: | --- |
| `welcome-v1` | 신규 모험가 보급 | 1 | `gold-10000` × 1; `potion-ph01` × 1; `potion-pm01` × 1 |
| `daily-supplies-v1` | 일일 보급 | 1 | `stones-30` × 1; `potion-ph01` × 1 |
| `weekly-supplies-v1` | 주간 보급 | 10 | `gold-10000` × 1; `stones-100` × 1; `gem-choice-t1` × 1 |
| `maintenance-v1` | 점검 보상 | 1 | `premium-100` × 1; `potion-ph01` × 1 |
| `incident-v1` | 장애 보상 | 1 | `premium-300` × 1; `gold-10000` × 1; `stones-100` × 1 |
| `returning-v1` | 복귀 모험가 보급 | 10 | `rare-slot-choice` × 1 (quality ≥ 50%); `gem-choice-t1` × 1; `potion-ph01` × 2; `potion-pm01` × 2 |
| `event-participation-v1` | 이벤트 참여 보상 | 1 | `premium-100` × 1; `gold-10000` × 1 |
| `event-completion-v1` | 이벤트 완주 보상 | 30 | `legendary-slot-choice` × 1 (quality ≥ 50%); `premium-300` × 1 |
| `anniversary-v1` | 기념일 선물 | 10 | `premium-500` × 1; `gem-choice-t2` × 1; `rare-slot-choice` × 1 (quality ≥ 60%) |
| `blacksmith-v1` | 대장간 보급 | 30 | `materials-200` × 1; `stones-100` × 1; `core-choice` × 1 |
| `gem-apprentice-v1` | 보석 입문 보급 | 10 | `gem-choice-t1` × 2 |
| `gem-adept-v1` | 보석 숙련 보급 | 100 | `gem-choice-t3` × 1 |
| `rune-apprentice-v1` | 룬 입문 보급 | 15 | `rune-starter` × 1; `rune-g1` × 1 |
| `rune-adept-v1` | 룬 숙련 보급 | 100 | `rune-g2` × 1; `rune-g3` × 1 |
| `legendary-hunt-v1` | 전설 사냥 보상 | 30 | `legendary-slot-choice` × 1 (quality ≥ 50%); `stones-100` × 1 |
| `class-set-v1` | 직업 세트 보급 | 200 | `set-helm` × 1 (quality ≥ 60%); `set-body` × 1 (quality ≥ 60%); `set-gloves` × 1 (quality ≥ 60%); `set-boots` × 1 (quality ≥ 60%) |
| `combat-potions-v1` | 전투 물약 보급 | 1 | `potion-ph01` × 2; `potion-pm01` × 2; `potion-pu04` × 1 |
| `utility-potions-v1` | 전술 물약 보급 | 1 | `potion-pu01` × 1; `potion-pu02` × 1; `potion-pu03` × 1; `potion-pu04` × 1; `potion-pu05` × 1; `potion-pu06` × 1 |
| `elixir-t1-v1` | 1단계 영약 종합 보급 | 10 | `elixir-g01-t1` × 1; `elixir-g02-t1` × 1; `elixir-g03-t1` × 1; `elixir-g04-t1` × 1; `elixir-g05-t1` × 1; `elixir-g06-t1` × 1 |
| `elixir-t3-v1` | 3단계 영약 종합 보급 | 100 | `elixir-g01-t3` × 1; `elixir-g02-t3` × 1; `elixir-g03-t3` × 1; `elixir-g04-t3` × 1; `elixir-g05-t3` × 1; `elixir-g06-t3` × 1 |
| `elixir-t6-v1` | 6단계 영약 종합 보급 | 750 | `elixir-g01-t6` × 1; `elixir-g02-t6` × 1; `elixir-g03-t6` × 1; `elixir-g04-t6` × 1; `elixir-g05-t6` × 1; `elixir-g06-t6` × 1 |
| `milestone-v1` | 성장 단계 달성 보상 | 30 | `legendary-slot-choice` × 1 (quality ≥ 50%); `gem-choice-t2` × 1; `rune-g1` × 1 |

## 예비 품목과 활성화 조건

아래 항목은 지급할 수 없다. 필요한 소비처를 구현하고 별도의 검증을 통과한 뒤 새 실행 정의로 등록한다.

| ID | 품목 | 필요한 작업 |
| --- | --- | --- |
| `event-token-reserved` | 행사 인장 | 기간별 교환 상점, 교환 한도, 종료 후 처리 규칙이 필요합니다. |
| `season-token-reserved` | 시즌 증표 | 시즌 저장 구분과 종료 시 이전·소멸 규칙이 필요합니다. |
| `honor-token-reserved` | 공적 훈장 | 공적 획득 조건과 반복 교환 한도가 필요합니다. |
| `guild-token-reserved` | 길드 공헌 증표 | 길드 소유권과 기여도 판정이 필요합니다. |
| `pvp-token-reserved` | 투기장 증표 | 대전 결과의 서버 판정과 교환 상점이 필요합니다. |
| `market-credit-reserved` | 거래소 재화 | 거래소·에스크로·수수료·회수 처리가 필요합니다. |
| `paid-credit-reserved` | 구매 전용 재화 | 구매 영수증과 환불·무료 지급분 구분이 필요합니다. |
| `rift-key-reserved` | 도전 균열 열쇠 | 입장권 전용 콘텐츠와 입장 실패 환급 거래가 필요합니다. |
| `boss-key-reserved` | 토벌 봉인석 | 보스 소환과 참여자 보상 소유권이 필요합니다. |
| `boss-fragment-reserved` | 토벌 봉인 조각 | 조각 합성과 봉인석 소비처가 필요합니다. |
| `treasure-key-reserved` | 보물고 열쇠 | 보물고 콘텐츠와 보상 확정 거래가 필요합니다. |
| `rift-modifier-reserved` | 균열 변이 인장 | 입장 스냅샷과 보상 배율의 별도 계약이 필요합니다. |
| `guarantee-fragment-reserved` | 확정 제작 파편 | 중복 재료를 늘리기 전에 기존 코어와 역할을 구분해야 합니다. |
| `reforge-voucher-reserved` | 옵션 재련권 | 기존 재련 거래의 비용 대체·재시도 계약이 필요합니다. |
| `socket-voucher-reserved` | 소켓 가공권 | 기존 소켓 가공 거래와 동일한 보호·비용 검사가 필요합니다. |
| `upgrade-catalyst-reserved` | 각성 촉매 | 각성 획득 구조와 확률·천장 설계가 필요합니다. |
| `crafting-manual-reserved` | 제작 비전서 | 계정 제작법 해금 소유자와 중복 획득 규칙이 필요합니다. |
| `charm-reserved` | 수호 부적 | 새 장착 구조보다 기존 룬·위상과 역할을 먼저 비교해야 합니다. |
| `legendary-gem-reserved` | 전설 보석 | 기존 보석 소켓과 별도의 전투·성장 설계가 필요합니다. |
| `respec-voucher-reserved` | 기술 재설정권 | 기존 무료 편집 기능에 비용을 추가하지 않습니다. 유료 초기화가 생길 때만 검토합니다. |
| `fatigue-voucher-reserved` | 탐험 시간 회복권 | 기존 회복 횟수·이월 규칙과의 관계를 확정해야 합니다. |
| `speed-voucher-reserved` | 집중 탐험권 | 1.5배속 입장 비용과 취소 환급에 연결해야 합니다. |
| `forge-voucher-reserved` | 대장간 시간 단축권 | 기존 작업 완료 거래와 중복 소비 방지가 필요합니다. |
| `stash-voucher-reserved` | 보관함 확장권 | 구매한 확장 단계와 충돌하지 않는 거래가 필요합니다. |
| `xp-boost-reserved` | 수련 축복 | 경험치 적용 시점·중첩·만료·전투 저장 규칙이 필요합니다. |
| `loot-boost-reserved` | 풍요의 축복 | 기존 영약·운영 배율과 중첩 관계를 확정해야 합니다. |
| `cosmetic-token-reserved` | 외형 교환권 | 외형 도감·장착·중복 교환 거래가 필요합니다. |
| `title-token-reserved` | 칭호 교환권 | 칭호 소유·표시·조건 검사가 필요합니다. |
| `pet-token-reserved` | 동료 계약서 | 동료 콘텐츠와 성장·전투 소유자가 필요합니다. |
| `battle-pass-reserved` | 여정 포인트 | 여정 보상 트랙·기간·중복 수령 방지가 필요합니다. |

## 공식 근거

2026-09-26에 로그인하지 않은 Codex 내장 브라우저로 열람했다. 위 비교의 빈 칸을 추측으로 채우지 않으며, 원작의 사례와 HELLSCRIPT에 제안한 확장 기능을 구분한다.

- [D2 아이템 분류 / Item families](https://classic.battle.net/diablo2exp/items/): 보석·주얼·룬·부적·제작·세트 등 원형 / gems, jewels, runes, charms, crafting and sets.
- [D2 큐브 조합 / Horadric Cube](https://classic.battle.net/diablo2exp/items/cube.shtml): 합성·재생 물약·소켓·수리 / conversion, rejuvenation, sockets and repair.
- [D2 기본 아이템 / Basic items](https://classic.battle.net/DIABLO2EXP/ITEMS/basics.shtml): 열쇠·장기·정수·면죄의 징표의 존재 / keys, organs, essences and Token of Absolution.
- [D3 패치 2.3.0 / Patch 2.3.0](https://news.blizzard.com/en-us/article/19859662/patch-2-3-0-now-live), 2015-08-25: 보관함·핏빛 파편·막별 재료·대균열석·지옥불 장치 / caches, shards, act materials, Greater Rift keys and Infernal Machines.
- [D4 전리품의 재탄생 / Loot Reborn](https://news.blizzard.com/en-us/article/24077223/galvanize-your-legend-in-season-4-loot-reborn), 2024-05-01: 위상·제작법·담금질·명품화·소환 재료·시즌 보상 / aspects, manuals, tempering, masterworking, summoning and seasonal rewards.
- [이모탈 Crucible of Justice](https://news.blizzard.com/en-gb/article/24135095/wield-untold-power-in-crucible-of-justice), 2024-09-09: 열쇠·문장·진주·행사 교환·복귀·재료 / keys, crests, pearls, event exchange, returners and materials.
- [이모탈 Forgotten Nightmares](https://news.blizzard.com/en-us/article/23827589/explore-a-new-piece-of-sanctuary-in-forgotten-nightmares), 2022-09-21: 백금·영원의 보주·무료/유료 패스·외형 / platinum, orbs, free/paid passes and cosmetics.

## 검증 상태

검증 결과는 [운영 보상 검증](Operations_Rewards_Validation.md)에 기록한다. 작업 브랜치 구현, main 통합, 공개 위키와 플레이어 배포를 구분한다.
