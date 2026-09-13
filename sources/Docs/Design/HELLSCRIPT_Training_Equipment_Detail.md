# HELLSCRIPT 소유 장비 훈련과 비교 기준

작성일: 2026-09-13 · 상태: 데스크톱 구현·검증 완료 · 모바일 검증 별도

이 문서는 [플레이어 훈련](HELLSCRIPT_Player_Training_Detail.md)과 [A/B 비교](HELLSCRIPT_Training_Comparison_Detail.md)에 남아 있던 장비 교체를 구체화한다. 실제 계정과 분리된 기존 훈련 시뮬레이션을 사용한다. 배치와 조작 크기는 [화면 배치 기준](HELLSCRIPT_Screen_Layout_Detail.md)을 따른다.

## 1. 시험할 수 있는 장비

비교 시작 시 해당 캐릭터의 가방과 계정 공유 창고를 복사한다. 다른 캐릭터의 개인 가방·장착품, 비교 시작 후 얻은 장비, 소유하지 않은 가상 장비는 이번 비교에 넣지 않는다. 새 장비를 가져오려면 새 비교를 시작한다.

직업과 요구 레벨은 실제 장착 규칙으로 검사한다. 부위마다 한 개만 선택하며 빈 부위도 허용한다. 잠금·프리셋 참조·장착 보석은 그대로 보존한다. 공유 창고에서 고른 장비는 훈련 복사본만 사용하므로 실제 가방 용량을 소비하거나 창고에서 이동하지 않는다. 강화·접사·각성·상위 접사·걸작·보석 값을 시험 중 임의로 만들거나 바꾸지 않는다.

## 2. 고정하는 기준과 바꾸는 조건

| 항목 | A/B의 처리 |
|---|---|
| 영웅·환경 | A 시작 전 영웅 ID·직업·레벨·XP, 훈련 종류·시드·배치·초기 적과 RNG를 고정한다. |
| 장비·룬 | 장비의 모든 수치와 계정 룬 보드를 함께 복사한다. B에서는 소유 장비의 장착 조합만 바꾼다. 룬 보드 자체는 A/B에서 동일하며, 바꾼 무기에 대응하는 실제 보드 효과를 계산한다. |
| 행동 | 영웅이 실제 사용 중인 행동 체계를 유지한다. 기본 행동 사용자는 기존 행동·스킬·패시브를, 사냥 칙령 사용자는 현재 칙령 원본과 패시브를 B의 편집 대상으로 삼는다. 비활성 행동 설정을 바꾸고 실제 행동이 달라졌다고 표시하지 않는다. |
| 새 시도 | A와 B는 각각 새 HP·자원·CD·효과에서 시작한다. A 종료 상태를 B에 넘기지 않는다. |
| 실행 중 변경 | 실행 시작 후 장비 수치·장착·레벨·칙령 원본을 바꾼 시도는 고정 조건 비교로 인정하지 않는다. 다시 시작해야 한다. |

기존 A/B 시작 경로가 계정 룬 보드를 빠뜨리던 문제를 이 범위에서 수정했다. 수정 전 결과에는 당시 기록이 없던 룬 효과를 소급해서 더하지 않는다.

## 3. 편집과 결과

A 결과에서 B 편집으로 이동한다. 부위를 고르면 당시 가방·창고 후보와 출처·장착 조건을 보여 준다. 후보의 전체 옵션, A 장비와 현재 B 장비, 교체 후 능력치·세트·보석의 차이를 읽을 수 있어야 한다. 선택과 상세 열람만으로 실제 장비의 확인 여부나 장착 상태를 바꾸지 않는다.

좁은 화면은 목록·상세를 전환하고 돌아왔을 때 선택과 읽던 위치를 유지한다. 넓은 화면은 목록과 상세를 나란히 표시한다. 시작·취소·편집 복귀 조작은 본문 스크롤과 분리한다. 글자 140%, 짧은 가로 화면, 세로 화면과 언어 변경에서도 편집안을 유지한다.

결과는 같은 기준 장비인지 장비를 바꾼 비교인지 명시한다. 행동 변경과 장비 변경을 구분하고, 슬롯별 A/B 장비 원본과 당시 룬 보드를 보존한다. 총 피해·생존·사용 횟수는 실제 두 훈련 결과를 보여 주며 한 쌍으로 승률이나 우수 빌드를 판정하지 않는다.

## 4. 저장과 활용

새 비교 기록은 버전 2로 장착 장비·활성 칙령·룬 보드를 포함한다. 버전 1은 기존 의미로 조회하며 없는 룬 기록을 현재 계정의 값으로 채우지 않는다. 알 수 없는 버전이나 불완전한 새 기록은 원본을 보존하고 지원하지 않는 상태로 표시한다.

결과 저장과 프리셋 저장은 명시적인 선택으로만 진행한다. A/B에서 고른 실제 장비 ID를 해당 프리셋의 참조로 저장한다. 나중에 장비가 사라졌거나 다른 캐릭터에게 옮겨졌다면 기존 불러오기 미리보기에서 누락·소유권을 검사한다. 자동 구매·제작·장착은 하지 않는다.

기본 행동과 사냥 칙령의 저장 범위를 구분한다. 기존 빌드 슬롯은 장비 참조·스킬·패시브·기본 행동을 저장한다. 활성 사냥 칙령 원본은 선택한 기록에서 실제 칙령 편집안으로 불러와 차이를 확인한 뒤 직접 적용한다. 슬롯 저장만으로 칙령 전체를 보존했다고 표시하지 않는다.

## 5. 완료 근거

세 직업·세 훈련·내부 세 배속에서 같은 장비의 재현성과 실제 장비 변경의 전투 차이를 검사한다. 가방·창고 복사본, 요구 레벨·직업·중복 부위, 시작 뒤 실제 장비·룬 변경, 실행 중 변조, 전설·세트·보석·걸작, 활성 칙령 편집, 저장 실패·구형 기록·실제 재시작을 확인한다. 최종 소스의 전체 검사와 실제 앱 화면 검증이 끝나야 구현 완료로 분류한다. Android 물리 터치와 자연 성장 밸런스는 별도 검증이다.

## English contract

Capture the selected hero's bag, shared warehouse, active behavior document and account rune boards when the comparison begins. B may equip only those frozen owned items, one per slot, respecting class and required level. Selection borrows copies; it cannot transfer, review, enhance, reroll or mutate live property. Rune boards remain fixed, while the chosen weapon uses its actual board contributions.

Each attempt starts fresh in the same seeded training environment. Edit the behavior system actually used by the hero: legacy behavior or the active hunt edict, plus passives. Label equipment changes separately from behavior changes. Persist version-2 equipment/edict/rune evidence; preserve version-1 records without inventing missing rune values. Explicit build-slot saves retain the chosen real item IDs. The active hunt edict has a separate preview-and-apply path; existing build slots must not be presented as full edict archives.

Use responsive list/detail views, preserved drafts and scroll positions, accessible footer actions, Korean/English strings and 140% text. Completion requires focused combat/ownership tests, the full suite and native save/restart/UI verification. Physical Android input and natural progression remain distinct evidence.

실제 완료 근거와 남은 범위: [한국어 개발 기록](../Implementation/Training_Equipment_Expansion.md) · [English](../Implementation/Training_Equipment_Expansion.en.md).
