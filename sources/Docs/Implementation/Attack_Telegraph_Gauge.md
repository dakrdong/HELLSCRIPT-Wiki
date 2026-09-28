# 적·보스 공격 예고 게이지

작성일: 2026-09-28 · [English](Attack_Telegraph_Gauge.en.md)

균열의 모든 적과 보스 공격은 공격 전에 바닥에 범위를 먼저 보여 주고, 그 안을 게이지처럼 채운다. 게이지가 가득 차는 틱에 피해가 들어가고, 그 순간 범위가 한 번 밝게 번쩍인다. 어디에 언제 공격이 떨어지는지를 한 화면에서 읽고 피할 수 있게 하려는 변경이다. 준비 시간·범위·피해 같은 전투 규칙은 바꾸지 않았다. [다크 고딕 개편](Dark_Gothic_Overhaul_Stage1.md) 2단계의 첫 항목이다.

## 보이는 방식

- 범위 전체를 옅게 먼저 보여 준다. 채워진 부분은 진하게, 채워지는 경계는 밝은 선으로 그린다.
- 원과 부채꼴은 중심에서 바깥으로, 고리는 안쪽 경계에서 바깥으로, 직선(돌진 경로·화살·광선)은 시작점에서 끝점으로 차오른다.
- 게이지가 70%를 넘으면 채움이 더 뜨거운 색으로 바뀌고 테두리 박동이 빨라진다.
- 가득 차는 틱에 피해가 들어가고, 범위 전체가 0.3초 동안 흰빛에 가깝게 번쩍인 뒤 사라진다. 투사체 예고(화살·석궁·갈고리·구체·파편·독액)는 번쩍이지 않고 날아가는 투사체로 이어진다.
- 보스 예고는 일반 적보다 밝고 굵다. 지속 장판과 돌진 중인 경로는 가득 찬 상태로 보인다. 피난 지점은 파란 원으로 따로 보인다.

## 시점 계산

- 게이지는 예고가 시작된 때부터 공격이 떨어지는 때(현재 시각 + 남은 지연)까지의 비율이다. 실제 준비 시간을 그대로 쓰므로 보이는 게이지와 피해 시점이 어긋나지 않는다.
- 보스가 발동 뒤 차례로 떨어뜨리는 원(무덤 종소리·삼중 분출·연속 폭발)은 보스가 예고를 시작한 때부터 각 원이 떨어질 때까지를 하나의 게이지로 계산한다. 발동 순간 원이 새 장판으로 바뀌어도 처음부터 다시 차지 않는다.
- 탐식의 끌어당김처럼 도중에 끌어당긴 뒤 물기가 떨어지는 공격은 물기까지 게이지가 끊기지 않는다.
- 내려찍기 추가타와 두 번째 돌진처럼 앞선 타격이 끝난 뒤 새로 예고하는 공격은 게이지를 새로 시작한다.
- 기절·빙결로 중단된 예고는 번쩍이지 않고 사라진다. 지속 장판이 끝날 때도 번쩍이지 않는다.
- 화면 표시만 계산하며 전투 상태를 바꾸거나 전투 난수를 쓰지 않는다.

## 적용 범위

| 구분 | 대상 |
| --- | --- |
| 일반 적 | N01–N12의 모든 공격(근접 부채꼴, 돌진 직선, 화살·석궁 직선, 장판 원). N13–N20은 고유 공격을 넣기 전까지 N01의 근접 부채꼴을 쓴다. |
| 정예 | E01 추적 화염, E02 얼음 고리, E05 시체 폭발 |
| 사망 | N06 사망 폭발, N12 파편 폭발 |
| 보스 | 보스 5종의 기본 공격, 패턴 30종, 2페이즈 포효 |

## 구현

- [`TelegraphGauge.cs`](../../Assets/HELLSCRIPT/Runtime/Presentation/TelegraphGauge.cs): 예고마다 시작 시각과 떨어질 시각으로 게이지를 계산하고, 이번 프레임에 완료된 예고(발동·활성화·추가타 전환)를 알려 준다. 공격 행동의 준비 시간을 기억해 발동 때 생기는 연속 원이 보스 예고를 이어받게 한다.
- [`WorldView.Enemies.cs`](../../Assets/HELLSCRIPT/Runtime/Presentation/WorldView.Enemies.cs): 예고마다 채움 장판을 빌려 게이지를 넘기고, 완료된 예고를 번쩍이게 한다. 기존 윤곽선과 스모크가 읽는 키는 그대로다.
- [`WorldFx.Telegraph.cs`](../../Assets/HELLSCRIPT/Runtime/Presentation/WorldFx.Telegraph.cs): 원·고리·부채꼴·직선 채움 메시와 발동 섬광. [`WorldTelegraph.shader`](../../Assets/HELLSCRIPT/Resources/WorldTelegraph.shader): 게이지와 섬광 표현.
- 이펙트 기반 작업을 함께 합쳤다. 풀링 이펙트 라이브러리 `WorldFx*.cs`와 절차적 이펙트 텍스처 34장이다. 출처는 [자산 출처](Asset_Provenance.md)에 적었다.

## 검증

2026-09-28, 복제 프로젝트와 macOS 개발 빌드(`91821b37`)에서 확인했다.

- **Edit Mode `TelegraphGaugeTests` 73개 통과**: 일반 적 18종 공격, 사망 폭발 2종, 정예 3종, 보스 5종의 기본 공격·패턴 6종·포효 전부에 대해 확인한다.
  - 게이지가 비어서 시작해 줄지 않고 차오르며, 공격이 떨어지는 틱에 가득 차 완료된다.
  - 피해가 완료 틱에만 들어온다.
  - 보스 연속 원이 발동 뒤에도 이어서 찬다.
  - 중단된 예고와 끝나는 장판은 완료로 세지 않는다.
- **관련 검사 9종**: 395개 중 393개 통과. 실패 2개(`EnemyTests.DeathPayloadRestoresIndependentlyAndNeverRepeatsItsReward`)는 개편 전 기준선에 있던 실패다.
- **`-hellscriptTelegraphSmoke`**: 화면에 보인 예고 85개 모두 채움 장판의 `_Progress` 값이 게이지와 같았다. 게이지가 25·50·75%를 거쳐 발동했고, 떨어진 공격마다 섬광이 났으며, 보스 연속 원이 발동 뒤에도 이어서 찼다. [결과](AttackTelegraphGaugeEvidence/runtime-telegraph-smoke.txt)
  - 일반 적: [N01 근접](AttackTelegraphGaugeEvidence/gauge-n01-swing.png), [N02 돌진](AttackTelegraphGaugeEvidence/gauge-n02-charge.png), [N09 석궁 3연발](AttackTelegraphGaugeEvidence/gauge-n09-arrows.png), [N04 독 장판](AttackTelegraphGaugeEvidence/gauge-n04-pool.png), [여러 모양 동시](AttackTelegraphGaugeEvidence/every-shape-at-once.png)
  - 보스: [거두기 베기](AttackTelegraphGaugeEvidence/gauge-b01-sweep.png), [무덤 종소리 연속 원](AttackTelegraphGaugeEvidence/gauge-b01-grave-toll.png), [만가의 고리](AttackTelegraphGaugeEvidence/gauge-b02-dirge-ring.png), [침묵의 찬가와 피난 지점](AttackTelegraphGaugeEvidence/gauge-b02-hymn-refuges.png), [연속 폭발](AttackTelegraphGaugeEvidence/gauge-b03-blasts.png), [삼중 분출](AttackTelegraphGaugeEvidence/gauge-b04-triple-eruption.png), [서리 폭발](AttackTelegraphGaugeEvidence/gauge-b05-frost-nova.png), [빙하 가시](AttackTelegraphGaugeEvidence/gauge-b05-glacial-spikes.png). 각 그림은 25·50·75%·발동 순서다.
- **보스 패턴 스모크**(`-hellscriptBossResume 4`): 패턴 30종과 포효 5회가 준비 시간에 맞춰 발동했다. [결과](AttackTelegraphGaugeEvidence/runtime-boss-kit-smoke.txt)
- **원래부터 실패하는 스모크**: 적 스모크, 보스 스모크의 재시작 단계(1–3), 전투 배치 스모크는 이번 변경 전의 1단계 빌드에서도 같은 이유로 실패한다. 적 스모크는 새 계정이 튜토리얼로 보내지는 설정이 빠진 탓이고, 나머지는 장면 준비 단계에서 멈춘다. 적 스모크 준비 코드는 다음 항목에서 고친다.

## 확인하지 못한 것

- 실기기와 Android 빌드에서 보지 않았다.
- 화면 게이지는 표시 프레임마다 계산한다. 공격이 떨어지기 직전 마지막 프레임은 프레임 간격만큼 덜 찬 상태이고, 떨어지는 순간은 섬광으로 보인다.
- 여러 예고가 겹치면 채움이 겹쳐 더 진해진다. 겹침 표현은 따로 다듬지 않았다.
- 사람의 눈으로 한 품질 평가가 아니다. 캡처와 측정값으로 확인했다.

## 다음 항목

공격 전 예비 동작(자세·달아오르는 테두리)과 공격이 떨어질 때의 이펙트·피격 반응, 보스 패턴 재설계, N13–N20 고유 공격을 차례로 진행한다.
