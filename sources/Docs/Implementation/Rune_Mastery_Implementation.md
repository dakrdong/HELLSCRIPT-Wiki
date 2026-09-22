# 무기별 룬 성장 구현 기록

> 과거 구현·검증 기록입니다. 보드 규칙과 엘리트 수치는 2026-09-20의 [v13 기획](../Design/HELLSCRIPT_Rune_Mastery.md)으로 대체했습니다. 현재 구현·검증 근거는 [v13 적용 기록](Rune_V13_Implementation.md)에 있습니다. 아래의 이전 검사 결과는 당시 기록으로 보존합니다.

2026-09-13 · 구현 및 자동 검사·macOS 실행 검증을 완료했습니다.

기획은 [무기별 룬 성장](../Design/HELLSCRIPT_Rune_Mastery.md)과 [영어판](../Design/HELLSCRIPT_Rune_Mastery.en.md)을 따릅니다. 룬을 언제든 회수해 다른 무기 보드에 재사용할 수 있으며, 프리셋 5칸은 각각 모든 무기의 배치를 함께 저장합니다.

## 구현 범위

도검·대검·도끼·활·쇠뇌·지팡이 보드를 추가했습니다. 현재 장착한 무기 종류의 보드 하나만 공용 능력치와 무기 마스터리를 제공합니다. 무기 장비를 교체해도 같은 종류라면 기존 배치를 유지합니다.

각 61칸 영역은 기본 능력치 37칸, 고급 능력치 12칸, 특정 스킬 추가 효과 9칸, 스킬 레벨 증가 3칸으로 구성됩니다. 기본 능력치는 수치 등급 0~2, 치명타·공격속도 등의 무기 특화 능력은 3~4, 스킬 추가 효과는 5, 스킬 레벨 증가는 6입니다. 개방 등급·룬 색·능력 수치 등급은 별개이며 화면 상세와 DB에서 구분합니다.

PackBound의 육각 조각 34종, 7영역·427칸 배치, 60도 회전, 시작점 연결과 같은 색 간격 검사, 편집·소유 모델, 효과 활성화와 합산, 5페이지 연습 모델을 재사용했습니다. 원본 프로젝트를 수정하지 않았습니다. 출처와 해시는 [이식 기록](Rune_Growth_Import.json)에 보존했습니다.

보드 편집, 자유 회수, 전체 저장·되돌리기, 전역 프리셋 저장·불러오기·삭제, 합성·결과 수령, 연습 화면을 기존 UGUI에 연결했습니다. 보관함은 보드 오른쪽에서 독립적으로 스크롤하며 탭 선택과 드래그를 지원합니다. 가로 화면은 상단 간격을 줄이고 방향 전환 시 편집 중 배치를 유지합니다. 문구는 한국어와 영어를 함께 제공합니다.

신규·기존 계정에는 처음 한 번 G0 룬 16개를 지급합니다. 실제 보스 처치 시 룬 4개를 지급하며, 실행 ID로 중복을 막습니다. 같은 색·등급·크기 두 개의 합성은 수수료 없이 항상 성공합니다. 미수령 결과와 실제 룬 ID를 저장하므로 다시 실행해도 결과가 유지되고 프리셋이 룬을 복제하지 않습니다.

## 저장과 전투 경계

`RuneGrowth`가 계정 공유 소유·배치·프리셋·보상을 소유하고, `GameStore.Runes`가 복사본 검증과 파일 쓰기 성공 후 적용을 담당합니다. 저장 실패나 오래된 편집본은 기존 상태를 덮어쓰지 않습니다. 합성으로 사라진 룬을 요구하는 프리셋은 일부 적용 없이 전체 거절합니다.

`HeroStats`는 실제 장착 무기 보드만 계산합니다. 스킬 추가 피해, 자원 소모 감소와 레벨 증가는 기존 스킬 실행에 반영합니다. 레벨 강화는 스킬을 먼저 배우는 조건을 생략하지 않습니다. 기존 취약의 기본 보너스와 전투 상한을 유지합니다.

편집 화면을 열면 전투를 정지하고 닫을 때 이전 상태로 돌아갑니다. 저장으로 최대 HP가 늘어도 회복하지 않으며, 줄어들면 새 최대치까지만 유지합니다. 행동·시간·자원·재사용 대기시간·난수 상태를 보존합니다. 이미 발사된 공격과 생성된 장판에는 생성 당시 룬 수치를 적용하고, 새 효과부터 변경한 값을 사용합니다.

## 검증

- 집중 검사에서 룬 배치·이동·전체 프리셋·합성·저장 실패·전투 유지·능력 등급과 번역을 확인했습니다.
- [전체 Edit Mode 보고서](../../Artifacts/Validation/rune-mastery-editmode-full.xml)는 **2,113개 통과, 실패 0개**입니다. 이후 취약 기본 보너스 보완과 새 검사에 대해 룬·능력치·효과·행동·성장·저장·번역 [회귀 검사](../../Artifacts/Validation/rune-mastery-editmode-regression.xml) **263개가 모두 통과**했습니다. 두 실행의 검사 수를 합산하지 않습니다.
- macOS 개발 빌드에서 실제 화면 버튼과 포인터 이벤트로 배치·회수·이동·프리셋·연습·합성을 확인했습니다. 세로 720×1280, 가로 1280×720, 영어 글자 크기 140%를 검사했습니다.
- 별도 계정으로 프로그램을 종료하고 다시 실행해 배치·프리셋 5칸·미수령 결과를 복원했습니다. 결과 수령과 일시정지 중 편집 후 체력·자원·쿨다운·행동·시간·난수 유지도 검사했습니다.
- Unity 실행 코드에서 생성한 [전체 카탈로그](Rune_Mastery_Catalog.json)를 공개 DB로 사용합니다. 코드와 내보낸 JSON의 일치 검사가 오래된 DB의 게시를 막습니다.

모바일 실기기, 장기 성장 경제, 모든 장비 조합의 밸런스 측정은 이번 검증에 포함되지 않습니다. 초기 지급·보상 주기와 능력 수치는 실제 플레이를 통한 조정 대상입니다.

## English

Six weapon-category boards now provide mastery only while their category is equipped. Runes can be removed and reused freely, but a physical rune can occupy only one board. Exactly five presets each capture all six layouts together. Presets store references, never extra rune ownership.

Each 61-cell region has 37 basic-stat cells at tiers 0–2, 12 advanced-stat cells at tiers 3–4, nine skill damage/cost cells at tier 5, and three skill-level cells at tier 6. Region unlock, rune color and ability tier are separate. Existing skills receive real damage, cost and level modifiers; unlearned skills remain locked.

PackBound supplied the 34 shapes, seven-region geometry, rotations, spacing/connectivity rules, draft/ownership model, activation/aggregation and five practice pages. HELLSCRIPT owns account persistence, weapon mapping, combat integration and UGUI. The import record preserves source hashes. The source project was not modified.

Staged writes apply only after successful persistence. Invalid presets, consumed references and stale drafts leave the current layouts intact. Fusion results persist before claiming. Editing pauses combat and preserves actions, clocks, cooldowns, resources and RNG; maximum-life increases do not heal. Released attacks retain their captured rune modifiers, and the existing base vulnerability bonus is preserved.

Native macOS UI checks cover Korean/English, portrait/landscape, 140% text, placement/recovery/dragging, five global presets, isolated practice, fusion, process restart and paused combat changes. The full Edit Mode run passed all 2,113 tests. After the final vulnerability fix, all 263 affected regression tests passed. These are separate runs, not additive coverage. The final native build and both initial/restart workflows passed. This is not physical mobile-device or long-term balance validation.


## 실행 근거와 공개 조회

macOS 개발 빌드 경로는 `Builds/macOS/RuneMastery/HELLSCRIPT.app`입니다. 빌드는 오류 0개로 성공했습니다. [최초 실행 결과](RuneMasteryEvidence/initial-result.txt)와 [재실행 결과](RuneMasteryEvidence/resume-result.txt)를 보존했습니다. 실제 포인터 이벤트와 화면 버튼을 사용한 검사이며, 아래 전체 영역 화면의 최고 클리어 30단계는 전용 시험 계정의 임시 설정입니다. 자연 플레이로 30단계를 달성했다는 근거가 아닙니다.

[한국어 보드 화면](RuneMasteryEvidence/board-ko.png) · [영어 가로 화면](RuneMasteryEvidence/board-en-landscape.png) · [전체 무기 프리셋 5칸](RuneMasteryEvidence/global-presets-ko.png) · [전체 영역 시험 화면](RuneMasteryEvidence/all-regions-fixture-ko.png)

위키 생성·정합성 검사, Python 검사, JavaScript 화면·경로 검사와 룬 DB 대조 검사를 수행합니다. 공개본은 [전체 위키](https://dakrdong.github.io/HELLSCRIPT-Wiki/#/tree), [룬 성장 기획](https://dakrdong.github.io/HELLSCRIPT-Wiki/#/page/rune-mastery), [무기별 보드 DB](https://dakrdong.github.io/HELLSCRIPT-Wiki/#/db/rune-boards), [룬 조각 DB](https://dakrdong.github.io/HELLSCRIPT-Wiki/#/db/rune-shapes)에서 조회합니다. 원본 문서·이력·DB·첨부 자료를 함께 게시하며 공개본은 읽기 전용입니다.

The native build succeeded with zero errors. Initial and process-restart result files and representative screenshots are linked above. The all-region screenshot uses a temporary stage-30 unlock on an isolated test account, not a natural-play achievement. The public wiki mirrors the complete documentation, history, exported database and evidence as read-only content.


## 몬스터 드롭과 합성 확장

2026-09-13 후속 변경입니다. 기존의 보스 전용·1~5칸 균등 지급을 단계별 분포로 교체하고 일반 몬스터 2%, 정예 몬스터 20% 드롭을 추가했습니다. 보스 4개 확정 보상은 유지합니다. 1~4단계 G0은 1칸만 나오고, 30단계 이상 G6은 3칸 20%·4칸 40%·5칸 40%로 나옵니다. 단계별 전체 표는 [획득과 합성 기획](../Design/HELLSCRIPT_Rune_Mastery.md)에 있습니다.

`RuneEconomy`의 같은 정의를 실제 지급, 게임 내 ‘룬 드롭 확률’ 화면, 합성 결과 미리보기, 카탈로그 내보내기와 공개 DB에서 사용합니다. 룬은 몬스터 처치 보상 처리 중 전용 보관함으로 자동 수령됩니다. 장비 줍기 필터·가방 용량에 영향을 받지 않고 결과 화면은 몬스터와 보스의 지급 개수를 합산합니다. 훈련·소환 몬스터에서는 드롭하지 않습니다.

`EnemyState.runeRewardRolled`는 성공과 실패 모두의 판정 완료를 기록합니다. 결과는 실행 ID와 몬스터 ID로 고정하고 별도 난수를 사용하므로 처치 순서·전투 난수·장비 보상 난수에 영향을 주지 않습니다. 이미 죽음 처리를 마친 기존 저장의 몬스터는 재처리하거나 소급 지급하지 않습니다. 보스 지급 기록과 기존 룬·프리셋·미수령 합성 결과는 유지합니다.

합성은 같은 색·등급·크기 두 개에서 한 개로 진행합니다. 1~4칸은 같은 등급에서 한 칸 커지고, 5칸 두 개는 다음 등급 1칸이 됩니다. G6 5칸은 최종 단계입니다. 화면에서 각 행의 결과 등급·크기와 성공률 100%를 미리 확인할 수 있습니다. 결과 모양은 해당 크기의 모양 중 균등하게 선택합니다.

[드롭 DB](https://dakrdong.github.io/HELLSCRIPT-Wiki/#/db/rune-drops)는 몬스터 분류 3종 × 단계 구간 7종의 21개 기록을, [합성 DB](https://dakrdong.github.io/HELLSCRIPT-Wiki/#/db/rune-fusion)는 최종 단계까지 포함한 35개 규칙을 제공합니다. 드롭 성공 확률과 성공 후 크기 조건부 확률을 구분해 표시합니다.

### Drop and fusion expansion — English

The follow-up replaces uniform boss-only sizes with stage-based distributions and adds normal (2%) and elite (20%) monster drops. Bosses still guarantee four runes. G0 tiers 1–4 drop only one-hex runes; G6 tier 30+ drops three/four/five hexes at 20%/40%/40%. RuneEconomy is the shared source for rewards, in-game rates, fusion previews and exported wiki data. Runes are automatically received into dedicated storage and do not depend on equipment bag space or pickup filters. The result screen totals monster and boss rewards. Training and summoned enemies are excluded.

Every eligible corpse records its attempted roll, including misses. Results use run and enemy IDs independently of combat/gear RNG and death order. Old processed corpses are never replayed or rewarded retroactively. Existing ownership, global presets, pending fusion outcomes and boss receipts remain intact. Fusion previews show the exact output grade/size and guaranteed success; shapes are uniform within the output size. The public databases contain 21 drop profiles and 35 fusion rules, with conditional size rates explicitly distinguished from per-kill drop rates.


### 드롭·합성 확장 검증

관련 Edit Mode 검사 **138개가 모두 통과**했습니다. [검사 보고서](../../Artifacts/Validation/rune-loot-editmode.xml)는 크기 분포의 모든 백분위 경계, 단계 전환, 금지 크기, 일반·정예·보스 확률, 35개 합성 규칙, 실제 사망 처리 연결, 저장·복원과 룬 소비 이후 중복 방지를 포함합니다. 전투·장비 난수와 처치 순서 독립성, 기존 룬·전투 종료·장비 희귀도·저장·번역 회귀 검사도 통과했습니다.

`Builds/macOS/RuneLoot/HELLSCRIPT.app` 개발 빌드는 오류 0개로 성공했습니다. [최초 실행 검사](RuneLootEvidence/initial-result.txt), [몬스터 드롭 검사](RuneLootEvidence/monster-drop-result.txt), [프로세스 재실행 검사](RuneLootEvidence/resume-result.txt)가 모두 통과했습니다. 실제 화면 버튼으로 확률표와 합성·수령을 열고 한국어 세로 화면, 영어 가로 화면과 글자 크기 140%를 확인했습니다. 재실행 후에도 미수령 결과가 유지되고, 결과 수령과 일시정지 중 보드 편집이 정상 작동했습니다.

[한국어 드롭 확률](RuneLootEvidence/drop-rates-ko.png) · [영어 확률 안내·글자 140%](RuneLootEvidence/drop-rates-en-large.png) · [합성 결과 미리보기와 수령](RuneLootEvidence/fusion-result-ko.png)

몬스터 실행 검사는 결과가 재현되는 별도 시험 계정을 사용했습니다. 실측 드롭 통계나 자연 플레이의 성장 속도를 측정한 결과는 아닙니다. 모바일 실기기와 장기 성장 경제는 후속 플레이 검증이 필요합니다.

All **138 affected Edit Mode tests passed**, covering percentile boundaries, tier transitions, forbidden sizes, source rates, all 35 fusion rules, native death integration, persistence, consumed-item duplicate prevention, RNG/order independence and existing mastery/outcome/rarity/save/localization regression. The macOS development build succeeded with zero errors. Initial UI, native monster rewards and process-restart checks all passed. The linked screenshots cover Korean portrait, English landscape at 140% text and fusion output previews. Monster verification uses an isolated deterministic fixture; it is not empirical drop-rate, natural progression or physical mobile-device validation.

## 2026-09-21 룬 공방 연결

[룬 마스터](Rune_Master.md)에 재형성과 승급을 연결했다. 승급은 같은 색끼리만 합성하며 잠금·보드·프리셋에 사용 중인 룬을 보호한다. NPC와 기존 룬 보드 화면이 같은 검증 규칙을 사용한다.

The [Rune Master](Rune_Master.en.md) now provides reshaping and ascension. Ascension requires matching colors and protects locked, placed and preset-linked runes. The NPC and existing rune-board entry share one validation rule.
