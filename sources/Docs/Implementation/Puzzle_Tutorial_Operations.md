# 운영·정비 퍼즐 구현 — 단계 7

작성 2026-10-07. L9~L13과 비전투 정비 P1·P2를 실제 전리품·장착·상자·판매·강화·반복 사냥 소유자에 연결했다. 전체 튜토리얼 완료 보고가 아니다. L4의 적 구성 결정과 단계 8~9는 남아 있다. 이 단계에는 새 이미지·패키지·씬 사본을 추가하지 않는다.

## 거래와 판정

- L9: 가까운 일반·마법 8개, 희귀 3개, 먼 전설 1개를 실제 드롭으로 만든다. 기존 가방 용량과 원래 장비를 바꾸지 않고 시험 장비만 3칸으로 제한한다. 20초에 희귀 3개를 회수하고 시작점 반경 8m 안전 구역에 있어야 한다. 먼 14.2m 독 장판은 7초 뒤 활성화하며 실제 이동·위험 예측에 영향을 준다. 모든 장비 선택은 필요한 희귀 장비를 놓치고, 넓은 회수 거리는 안전 구역 복귀를 놓친다. 사망으로 보정하지 않는다.
- L10: 실제 CH03 상자를 연다. HP 50%, 실제 물약 대기 20초에서 시작하며 여유 있을 때는 실제 HP 85%와 남은 120초를 확인한다. 12초 잔향 전투는 N01 넷, 체력 95/95/90, 첫 실제 직격, 이후 반복 300초의 시험 배치다. 정상 균열 CH03 적 구성은 바꾸지 않는다. 여유 설정은 실제 회복 후 전멸, 전투 후 설정은 조기 개방 후 진압 실패, 건너뛰기는 미개방으로 목표를 놓친다.
- L11: 실제 일반·마법·강화 장비 세 개와 전설 드롭으로 교체·판매·투자 보호를 조합한다. 기존 `Economy.AddItem`과 `EdictCleanupPolicy`를 사용하고 시험 ID만 교체·정리 대상으로 전달한다. 정상 가치 교체도 동일한 투자 보호 정책을 재사용한다. 정답은 마법 한 개 판매로 금화 80, 전설 회수, 투자 장비 보존, 빈칸 하나를 만든다. 귀환·무시·정리 안 함·투자 보호 끔은 각각 실제 결과로 실패한다. 귀환이 일반 전투의 portal 틱 차단에 묶여 멈추던 경로를 운영 퍼즐 결과 판정으로 연결했다.
- L12: 현재 주무기의 낮은 사본, 다른 직업 무기, 같은 계열 +6 강화 사본을 드롭한다. `EquipmentRecommendation`의 실제 자동 장착과 실제 처치가 필요하다. 3초 뒤 N07/E06 철갑 망령이 등장하며 체력은 전사 2400·궁수 2450·마법사 1900이다. 45초에 장착 끔 또는 무기 교체 끔은 살아 있는 적을 남긴다. 실패한 시험 장비는 제거하고 그 장비가 밀어낸 원래 착용품만 기존 장착/창고 거래로 복구한다. 성공한 상위 무기는 첫 클리어 장비 보상으로 남는다. 전체 영웅·XP·키트를 재설정하지 않는다.
- L13: 기존 `RepeatHunt.Start/Complete`·중단 조건과 `RiftEarnings.GrantGold`를 엄격히 이 시험의 실제 자식 전투에 연결한다. 각 판은 독립 ID·시드·실제 전투이며 이전 실패 판의 HP와 결과를 보존한다. 1단계 10, 2단계 100, 3단계 0을 실제 완료 거래로 지급한다. 15초 전투 제한에서 3단계를 이기지 못하면 `LOWER`가 2단계로 돌아간다. 90초 전체 목표에서 정답은 `1,2,3 실패,2`로 금화 210, 한 판 종료·같은 단계 반복·실패 단계 고정은 목표 미달이다. 현재 판 스냅샷과 세션은 저장 시 JSON으로 캡처하고 매 틱 전체 상태를 복사하지 않는다. 복원 검사는 실제 완료 횟수·금화·시간·판별 결과의 일치를 확인한다.

L9/L11 시험 장비와 L11/L13 시험 금화는 `RecordPuzzleAttempt`의 동일 결과 거래에서 정리한다. 원래 소유품과 원래 금화는 보존하고 첫 클리어 XP 220만 한 번 지급한다. 복습은 기존 독립 샌드박스 소유자를 사용한다. 운영 실패는 사망·시간 초과와 구분해 복기 카드에 표시한다. 일반 균열의 포탈·가방 용량·상자·반복 사냥의 훈련 거부는 유지하며, 실제 자식 반복 시험만 명시적으로 허용한다.

## 정비 P1·P2

P1은 L10 전, P2는 L12 전 허브의 시작 행동에서 열린다. `tools/new_content_ui.py PuzzleMaintenance`로 만든 공통 창, `EquipmentComparisonView`·`ItemComparison.Preview`·`UiFonts`·`UiTheme`·`StoreViewBinding`을 사용한다. 허브를 닫고 정비 창을 열며 뒤로/완료는 같은 진행 허브로 돌아온다. 월드는 기존 허브 수명주기에서 숨긴다. 이 창의 계정은 실제 소유 상태이며 프리뷰 후보만 읽기 전용 사본이다.

P1은 카탈로그의 판금 B42/갑옷 접사 AF03과 천 로브 B39/저항 접사 AF04를 정상 최대 롤로 지급한다. 별도 피해 식 없이 `HeroStats.Resistance`와 공통 `StatCatalog.Mitigation`으로 영웅 레벨의 불·독 피해를 각각 10% 이상 낮춰야 한다. 실제 장착 거래를 거친 저항 장비가 통과하며 물리 방어구는 실패한다. P2는 시험 금화 110으로 실제 주무기 또는 착용 방어구의 +1 견적을 실행한다. 실제 `HeroStats.damage` 증가가 목표다. 아직 일반 강화 해금은 열지 않고, 준비된 이 시험의 두 지정 장비·한 번·시험 예산만 기존 강화 거래에 허용한다(대장간의 장비 강화는 L12 클리어 뒤에 열린다. [튜토리얼 대장간 정비](Tutorial_Forge_20261008.md)). 오답 재도전은 저장된 장비 지문이 같을 때 해당 강화 한 번과 그 지출만 되돌린다. 환불·지원금은 중복 지급되지 않고 되돌린 시도는 일일 강화 진행을 쌓지 않는다. 네 단계 힌트는 실제 오답 이후 표시한다.

## 검증과 남은 수락

집중 검사 338개가 통과했다. 공통 소유자·번역·저장 바인딩 269개 성공을 재사용했고, 새 전투의 초기화 전 검사와 저장 상태 검사를 구분한 뒤 실패했던 운영·정비 68개만 재실행해 모두 통과했다. 복습 반복 시험의 원래 계정 보존 검사 1개도 별도로 통과했다. 이전 실패 보고서는 보존한다. 가방 15선택, 자동 장착 9선택과 실제 장착 후 실패 복구, 반복 12선택과 진행 중 복원, 정비 12거래를 포함한다. 레벨 8 이후 합성 계정 fixture는 첫 여덟 레벨의 실제 클리어 결과이며 원래 사용자 세이브가 아니다. 경로·해시는 [승패와 fixture 근거](Evidence/Puzzle_Operations_Matrix.json)에 기록했다.

공통 UI 계약, UI 검사 11개, 갱신 검사 14 binding/165 source가 통과했다. 현재 UI 범위의 실제 macOS 입력·KO/EN·다섯 해상도·저장 실패·성능 측정은 마지막 단계 9에 남아 있다. 전체 EditMode·런타임 스모크는 아직 실행하지 않았다. 모바일 실기기는 미검증이다. 이 PR은 native 수락 전 Draft로 둔다. 공개 위키는 게시하지 않으며 main 병합 담당자가 게시한다. 원자료 XML/로그·튜닝 실패·임시 파일은 작업 전용 Artifacts 경로와 lifecycle manifest에 보존하며 검토일은 2026-10-13, 영구 삭제 승인은 없다.

| Level | Class | Choice | Result | Actual record |
|---|---|---|---|---|
| L12 | Warrior | answer | Cleared | hp=505.0 time=38.05 remaining=0.0 stored=0 |
| L12 | Warrior | off | Failed | hp=505.0 time=45.05 remaining=392.7 stored=0 |
| L12 | Warrior | weapon-off | Failed | hp=505.0 time=45.05 remaining=392.7 stored=0 |
| L12 | Ranger | answer | Cleared | hp=410.0 time=37.20 remaining=0.0 stored=0 |
| L12 | Ranger | off | Failed | hp=410.0 time=45.05 remaining=282.3 stored=0 |
| L12 | Ranger | weapon-off | Failed | hp=410.0 time=45.05 remaining=282.3 stored=0 |
| L12 | Mage | answer | Cleared | hp=365.0 time=41.70 remaining=0.0 stored=0 |
| L12 | Mage | off | Failed | hp=365.0 time=45.05 remaining=399.8 stored=0 |
| L12 | Mage | weapon-off | Failed | hp=365.0 time=45.05 remaining=399.8 stored=0 |
| L13 | Warrior | climb | Cleared | elapsed=47.85 gold=210 rounds=4 stages=1,2,3,2 |
| L13 | Warrior | same | Failed | elapsed=90.00 gold=80 rounds=8 stages=1,1,1,1,1,1,1,1 |
| L13 | Warrior | once | Failed | elapsed=10.90 gold=10 rounds=1 stages=1 |
| L13 | Warrior | hold | Failed | elapsed=67.00 gold=110 rounds=5 stages=1,2,3,3,3 |
| L13 | Ranger | climb | Cleared | elapsed=55.90 gold=210 rounds=4 stages=1,2,3,2 |
| L13 | Ranger | same | Failed | elapsed=90.00 gold=60 rounds=6 stages=1,1,1,1,1,1 |
| L13 | Ranger | once | Failed | elapsed=13.95 gold=10 rounds=1 stages=1 |
| L13 | Ranger | hold | Failed | elapsed=72.55 gold=110 rounds=5 stages=1,2,3,3,3 |
| L13 | Mage | climb | Cleared | elapsed=54.45 gold=210 rounds=4 stages=1,2,3,2 |
| L13 | Mage | same | Failed | elapsed=90.00 gold=60 rounds=6 stages=1,1,1,1,1,1 |
| L13 | Mage | once | Failed | elapsed=13.10 gold=10 rounds=1 stages=1 |
| L13 | Mage | hold | Failed | elapsed=71.40 gold=110 rounds=5 stages=1,2,3,3,3 |
| L11 | Warrior | answer | Cleared | time=8.00 sale=80 slots=1 invested=True |
| L11 | Warrior | return | Failed | time=4.05 sale=0 slots=0 invested=True |
| L11 | Warrior | leave | Failed | time=8.00 sale=120 slots=2 invested=True |
| L11 | Warrior | keep | Failed | time=8.00 sale=0 slots=0 invested=True |
| L11 | Warrior | unprotected | Failed | time=8.00 sale=120 slots=2 invested=False |
| L11 | Ranger | answer | Cleared | time=8.00 sale=80 slots=1 invested=True |
| L11 | Ranger | return | Failed | time=4.05 sale=0 slots=0 invested=True |
| L11 | Ranger | leave | Failed | time=8.00 sale=120 slots=2 invested=True |
| L11 | Ranger | keep | Failed | time=8.00 sale=0 slots=0 invested=True |
| L11 | Ranger | unprotected | Failed | time=8.00 sale=120 slots=2 invested=False |
| L11 | Mage | answer | Cleared | time=8.00 sale=80 slots=1 invested=True |
| L11 | Mage | return | Failed | time=4.05 sale=0 slots=0 invested=True |
| L11 | Mage | leave | Failed | time=8.00 sale=120 slots=2 invested=True |
| L11 | Mage | keep | Failed | time=8.00 sale=0 slots=0 invested=True |
| L11 | Mage | unprotected | Failed | time=8.00 sale=120 slots=2 invested=False |
| L10 | Warrior | safe | Cleared | hp=275.9/505.0 time=25.35 event=Succeeded started=20.30 |
| L10 | Warrior | skip | Failed | hp=432.0/505.0 time=160.01 event=Cancelled started=0.00 |
| L10 | Warrior | after | Failed | hp=174.4/505.0 time=12.30 event=Failed started=0.30 |
| L10 | Ranger | safe | Cleared | hp=212.8/410.0 time=29.20 event=Succeeded started=20.25 |
| L10 | Ranger | skip | Failed | hp=350.7/410.0 time=160.01 event=Cancelled started=0.00 |
| L10 | Ranger | after | Failed | hp=136.0/410.0 time=12.25 event=Failed started=0.25 |
| L10 | Mage | safe | Cleared | hp=194.4/365.0 time=24.90 event=Succeeded started=20.30 |
| L10 | Mage | skip | Failed | hp=312.2/365.0 time=160.01 event=Cancelled started=0.00 |
| L10 | Mage | after | Failed | hp=64.6/365.0 time=12.30 event=Failed started=0.30 |
| L9 | Warrior | answer | Cleared | hp=470.0 time=20.05 loot=3 position=(4.50, 0.88) |
| L9 | Warrior | all | Failed | hp=470.0 time=20.05 loot=3 position=(4.49, 0.85) |
| L9 | Warrior | far | Failed | hp=470.0 time=20.05 loot=3 position=(11.13, 0.28) |
| L9 | Ranger | answer | Cleared | hp=382.0 time=20.05 loot=3 position=(4.52, 0.89) |
| L9 | Ranger | all | Failed | hp=382.0 time=20.05 loot=3 position=(4.50, 0.85) |
| L9 | Ranger | far | Failed | hp=382.0 time=20.05 loot=3 position=(11.13, 0.28) |
| L9 | Mage | answer | Cleared | hp=340.0 time=20.05 loot=3 position=(4.50, 0.88) |
| L9 | Mage | all | Failed | hp=340.0 time=20.05 loot=3 position=(4.49, 0.85) |
| L9 | Mage | far | Failed | hp=340.0 time=20.05 loot=3 position=(11.13, 0.28) |
