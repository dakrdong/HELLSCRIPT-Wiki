# 퍼즐 튜토리얼 콘텐츠 — 단계 5

> 2026-10-08: 이 문서의 레벨 수치·예고·쿨타임 설명은 몬스터 패턴을 조정하던 이전 구현의 기록이다. 현재 구성은 [몬스터 고유 패턴 원칙](Puzzle_Tutorial_Native_Patterns.md)을 따른다.

작성 2026-10-07. 필수 튜토리얼 v5의 L3~L8, 공개 칙령 목록, 정찰·복기·힌트와 실제 영웅 복사본의 A/B 비교를 구현했다. **L4의 최종 구성과 검증 기준은 [9단계 기록](Puzzle_Tutorial_Final_Acceptance.md)에서 갱신한다.** 전체 튜토리얼 완료 기록이 아니다.

현재 레벨까지 공개된 옵션만 표시·저장한다. 스킬은 허용 트랙의 랭크 1만 학습·장착하며 스킬 정책 직접 편집은 L8, 공통 공격 순서는 L15에서 열린다. 교관도 실제 `HuntEdictEditSession`과 `CommitHuntEdict`로 저장한다. 실패는 실제 사망과 시간 초과를 구분하며 마지막 5초의 실제 HP 손실만 복기한다. 힌트 단계는 재시작 후 유지되고 4단에서 교관 설정을 적용할 수 있다.

L3는 0/11/22초의 실제 이중 공격과 20초 물약 재사용을 사용한다. L5는 실제 보행 후퇴와 물약 회복으로 귀환한다. L6는 사제 호위 셋과 궁병 쪽 시체 셋을 분리하고, 실제 지원 치유가 실행되는 상태에서 대상 우선순위가 결과를 바꾼다. W02/A01의 키트 조준만 전역 대상을 따르게 고정한다. L7은 지정 이동기 없이는 실제 E07 폭발로 죽고 정답은 최소 3주기 동안 폭발 피해 0이다. 사방 포위는 벽 밖에 적을 만드는 대신 유효한 착지 면에 반경 1.6m로 배치한다. L8은 W03/A01/M03의 무리·연결 조건과 단일/강적 조건을 실제 같은 적·장비·시드에서 비교한다. A/B는 계정·보상을 변경하지 않는다.

## L4 결정 이력

사용자는 9단계에서 N09 추가를 승인한 뒤, 권장 해법 외의 실제 클리어도 인정하도록 기준을 바꿨다. 현재 L4는 신중→공격적 전환을 가장 편하고 효율적인 공략으로 안내하며, 고정 성향으로 이겨도 같은 클리어와 XP를 인정한다. 아래 내용은 5단계 당시의 결정 대기 기록이다.

원래 N04×2+N10×2 다음 N01×12 구성과 성향 동작을 유지했다. 공격적 성향도 지속 장판을 전부 피하므로 신중→공격적만 승리하는 조건이 아직 성립하지 않는다. 예고를 0.2초로 줄인 대안도 오답 승리와 궁수 정답 시간 초과가 발생해 채택하지 않았다. 교관 정답 경로의 세 직업 승리는 확인했지만 L4 수락 기준 통과로 보고하지 않는다. N09 직격 적 추가를 제안한 소유자 결정은 아직 도착하지 않았다. 강제 사망·오답 선택 검사로 승패를 대신하지 않는다.

## 확인된 검사와 경계

L3/L5/L6/L7/L8 실제 저장 선택 61건, 힌트·복기 저장 3건, 실제 A/B 비교 3건이 통과했다. 공개·직접 편집·번역 검사 71건이 통과했다(70건 성공을 재사용하고 불필요한 숫자 형식 번역 항목을 제거한 뒤 실패한 1건만 재실행). 공통 UI/저장 갱신 정적 검사는 별도 보고한다. 전체 EditMode와 macOS 플레이어 스모크는 마지막 통합 단계 9에서 한 번 수행한다. 이 문서는 네이티브 화면·모바일 실기기 검증 완료를 주장하지 않는다.

상세 원자료: [승패 행렬](../../Artifacts/PuzzleTutorial/20261006/stage5-accepted-matrix.json). 실패 튜닝 XML/로그와 합성 저장은 같은 작업 폴더에 보존한다. `artifact-lifecycle.json`의 실측 크기와 2026-10-13 검토일을 따르며 영구 삭제 승인은 없다. 공개 위키는 게시하지 않고 PR 병합 담당자가 최신 main에서 게시한다.

| Level | Class | Choice | Result | HP | Seconds |
|---|---|---|---|---|---|
| L3 | Warrior | baseline | Failed | 0.00/365 | 23.25 |
| L3 | Warrior | potion25 | Failed | 0.00/365 | 23.25 |
| L3 | Warrior | potion40 | Failed | 0.00/365 | 23.25 |
| L3 | Warrior | answer | Cleared | 87.63/365 | 34.10 |
| L3 | Ranger | baseline | Failed | 0.00/298 | 23.50 |
| L3 | Ranger | potion25 | Failed | 0.00/298 | 23.50 |
| L3 | Ranger | potion40 | Failed | 0.00/298 | 23.50 |
| L3 | Ranger | answer | Cleared | 47.79/298 | 32.00 |
| L3 | Mage | baseline | Failed | 0.00/265 | 24.80 |
| L3 | Mage | potion25 | Failed | 0.00/265 | 24.80 |
| L3 | Mage | potion40 | Failed | 0.00/265 | 24.80 |
| L3 | Mage | answer | Cleared | 69.28/265 | 32.50 |
| L5 | Warrior | baseline | Failed | 0.00/435 | 3.70 |
| L5 | Warrior | emergency | Failed | 0.00/435 | 3.70 |
| L5 | Warrior | balanced | Failed | 0.00/435 | 3.70 |
| L5 | Warrior | answer | Cleared | 435.00/435 | 43.30 |
| L5 | Mage | baseline | Failed | 0.00/315 | 3.70 |
| L5 | Mage | emergency | Failed | 0.00/315 | 3.70 |
| L5 | Mage | balanced | Failed | 0.00/315 | 3.70 |
| L5 | Mage | answer | Cleared | 315.00/315 | 46.45 |
| L6 | Warrior | baseline | Failed | 430.36/435 | 90.00 |
| L6 | Warrior | nearest | Failed | 430.36/435 | 90.00 |
| L6 | Warrior | pack | Failed | 430.36/435 | 90.00 |
| L6 | Warrior | answer | Cleared | 435.00/435 | 66.30 |
| L6 | Mage | baseline | Failed | 315.00/315 | 90.00 |
| L6 | Mage | nearest | Failed | 315.00/315 | 90.00 |
| L6 | Mage | pack | Failed | 315.00/315 | 90.00 |
| L6 | Mage | answer | Cleared | 315.00/315 | 73.25 |
| L7 | Warrior | baseline | Failed | 0.00/470 | 9.50 |
| L7 | Warrior | unequipped | Failed | 0.00/470 | 9.50 |
| L7 | Warrior | attack-role | Failed | 0.00/470 | 9.50 |
| L7 | Warrior | answer | Cleared | 414.50/470 | 45.50 |
| L7 | Warrior | distance | Cleared | 388.00/470 | 63.45 |
| L7 | Warrior | four-sides | Cleared | 355.98/470 | 67.25 |
| L7 | Mage | baseline | Failed | 0.00/340 | 9.00 |
| L7 | Mage | unequipped | Failed | 0.00/340 | 9.00 |
| L7 | Mage | answer | Cleared | 304.40/340 | 35.75 |
| L7 | Mage | distance | Cleared | 313.24/340 | 36.90 |
| L7 | Mage | four-sides | Cleared | 297.59/340 | 35.35 |
| L8 | Warrior | baseline | Failed | 452.95/470 | 90.00 |
| L8 | Warrior | pack | Failed | 452.95/470 | 90.00 |
| L8 | Warrior | answer | Cleared | 434.98/470 | 59.15 |
| L8 | Mage | baseline | Failed | 287.99/340 | 90.00 |
| L8 | Mage | pack | Failed | 287.99/340 | 90.00 |
| L8 | Mage | answer | Cleared | 330.02/340 | 83.05 |
| L6 | Ranger | baseline | Failed | 354.00/354 | 90.00 |
| L6 | Ranger | nearest | Failed | 354.00/354 | 90.00 |
| L6 | Ranger | pack | Failed | 354.00/354 | 90.00 |
| L6 | Ranger | answer | Cleared | 354.00/354 | 84.80 |
| L5 | Ranger | baseline | Failed | 0.00/354 | 3.70 |
| L5 | Ranger | emergency | Failed | 0.00/354 | 3.70 |
| L5 | Ranger | balanced | Failed | 0.00/354 | 3.70 |
| L5 | Ranger | answer | Cleared | 354.00/354 | 46.05 |
| L7 | Ranger | baseline | Failed | 0.00/382 | 19.00 |
| L7 | Ranger | unequipped | Failed | 0.00/382 | 9.00 |
| L7 | Ranger | answer | Cleared | 382.00/382 | 35.10 |
| L7 | Ranger | distance | Cleared | 326.72/382 | 31.15 |
| L7 | Ranger | four-sides | Cleared | 382.00/382 | 31.85 |
| L8 | Ranger | baseline | Failed | 306.10/382 | 90.00 |
| L8 | Ranger | pack | Failed | 306.10/382 | 90.00 |
| L8 | Ranger | answer | Cleared | 326.06/382 | 77.55 |
