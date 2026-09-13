# 무기별 룬 성장 구현 기록

2026-09-13 · 구현 및 자동 검사·macOS 실행 검증을 완료했습니다.

기획은 [무기별 룬 성장](../Design/HELLSCRIPT_Rune_Mastery.md)과 [영어판](../Design/HELLSCRIPT_Rune_Mastery.en.md)을 따릅니다. 룬을 언제든 회수해 다른 무기 보드에 재사용할 수 있으며, 프리셋 5칸은 각각 모든 무기의 배치를 함께 저장합니다.

## 구현 범위

도검·대검·도끼·활·쇠뇌·지팡이 보드를 추가했습니다. 현재 장착한 무기 종류의 보드 하나만 공용 능력치와 무기 마스터리를 제공합니다. 무기 장비를 교체해도 같은 종류라면 기존 배치를 유지합니다.

각 61칸 영역은 기본 능력치 37칸, 고급 능력치 12칸, 특정 스킬 추가 효과 9칸, 스킬 레벨 증가 3칸으로 구성됩니다. 기본 능력치는 수치 등급 0~2, 치명타·공격속도 등의 무기 특화 능력은 3~4, 스킬 추가 효과는 5, 스킬 레벨 증가는 6입니다. 개방 등급·룬 색·능력 수치 등급은 별개이며 화면 상세와 DB에서 구분합니다.

PackBound의 육각 조각 34종, 7영역·427칸 배치, 60도 회전, 시작점 연결과 같은 색 간격 검사, 편집·소유 모델, 효과 활성화와 합산, 5페이지 연습 모델을 재사용했습니다. 원본 프로젝트를 수정하지 않았습니다. 출처와 해시는 [이식 기록](Rune_Growth_Import.json)에 보존했습니다.

보드 편집, 자유 회수, 전체 저장·되돌리기, 전역 프리셋 저장·불러오기·삭제, 합성·결과 수령, 연습 화면을 기존 UGUI에 연결했습니다. 보관함은 보드 오른쪽에서 독립적으로 스크롤하며 탭 선택과 드래그를 지원합니다. 가로 화면은 상단 간격을 줄이고 방향 전환 시 편집 중 배치를 유지합니다. 문구는 한국어와 영어를 함께 제공합니다.

신규·기존 계정에는 처음 한 번 G0 룬 16개를 지급합니다. 실제 보스 처치 시 룬 4개를 지급하며, 실행 ID로 중복을 막습니다. 같은 등급·크기 두 개의 합성은 수수료 없이 항상 성공합니다. 미수령 결과와 실제 룬 ID를 저장하므로 다시 실행해도 결과가 유지되고 프리셋이 룬을 복제하지 않습니다.

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
