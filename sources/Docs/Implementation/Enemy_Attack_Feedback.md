# 적 공격의 예비 동작과 타격 반응

작성일: 2026-09-28 · [English](Enemy_Attack_Feedback.en.md)

적의 공격이 떨어지기 전부터 공격하는 느낌이 나고, 떨어졌을 때와 맞았을 때 반응이 분명하게 보이도록 화면 표현을 더했다. [예고 게이지](Attack_Telegraph_Gauge.md)가 "어디에, 언제"를 알려 준다면, 이번 항목은 "누가, 어떤 공격을" 하는지를 몸과 이펙트로 보여 준다. 전투 규칙은 바꾸지 않았다. [다크 고딕 개편](Dark_Gothic_Overhaul_Stage1.md) 2단계의 두 번째 항목이다.

## 공격 전

- 적은 공격 종류에 맞게 몸을 움직인다. 근접은 몸을 젖히고 무기를 머리 위로 치켜들며, 돌진은 낮게 웅크리고, 사격은 겨누며 살짝 물러서고, 시전은 몸을 띄우고 팔을 든다.
- 예고 게이지가 차오를수록 몸의 테두리가 붉게 달아오른다. 보스는 주황빛으로 달아오른다. 게이지가 80%를 넘으면 몸이 떨린다.
- 시전하는 적과 보스는 손끝에 속성 불꽃(불·서리·번개·독·암흑·불티)을 모은다. 모으는 간격이 끝으로 갈수록 빨라진다.

## 공격이 떨어질 때

- 근접은 몸을 앞으로 내밀며 무기를 내리치고, 부채꼴을 따라 베기 궤적이 지나간다. 돌진은 출발 지점에 흙먼지와 충격파를 남기고 달리는 동안 흙먼지 자국을 끈다. 사격은 반동으로 물러서며 총구 섬광을 낸다.
- 떨어진 범위는 모양과 속성에 맞는 이펙트를 낸다. 원은 충격파·폭발·바닥 자국, 고리는 고리를 따라 터지는 불꽃, 직선은 선을 따라 이어지는 폭발(냉기·번개·암흑은 번개 줄기)이다. 영웅 가까이 떨어지면 화면이 흔들리고, 보스는 더 크게 흔든다.
- 투사체는 날아가는 동안 속성 색을 띠고, 멈춘 곳에서 터진다.
- 지속 장판은 떨어진 뒤 빨간 게이지 대신 속성 색으로 바뀐다. 독은 초록, 불은 주황, 냉기는 파랑, 암흑은 보라다. 불은 불꽃이 타오르고 나머지는 연기와 거품이 올라온다. 떨어지기 전 예고는 계속 빨간 게이지로 남아, "곧 떨어질 공격"과 "이미 깔린 장판"을 색으로 구분할 수 있다.

## 맞았을 때

- 영웅이 맞으면 몸이 붉게 번쩍이고, 맞은 속성의 불꽃이나 피가 튄다. 화면은 맞은 피해가 최대 체력에서 차지하는 비율만큼 흔들린다. 흔들림은 기기의 동작 줄이기 설정(`hellscript.reduce-motion`)을 켜면 나오지 않는다. 회피하면 흙먼지가 인다.
- 적이 맞으면 몸이 희게 번쩍이고, 재질(살·뼈·금속·수정·얼음·체액·영혼)에 맞는 파편이 튄다. 치명타는 더 크게 번쩍인다.
- 쓰러진 적은 그 자리에서 불꽃 가장자리를 남기며 녹아 사라지고, 재질에 맞는 사망 폭발과 바닥 자국을 남긴다.

## 구현

- [`WorldView.ActorMotion.cs`](../../Assets/HELLSCRIPT/Runtime/Presentation/WorldView.ActorMotion.cs): 공격 자세, 테두리 발광, 피격 섬광, 사망 소멸, 영웅 피격 흔들림. 기본 도형 몸의 부위를 발 위치를 축으로 움직이므로 부위 계층과 이름·순서로 찾는 코드는 그대로다.
- [`WorldView.AttackFx.cs`](../../Assets/HELLSCRIPT/Runtime/Presentation/WorldView.AttackFx.cs): 떨어진 공격의 이펙트(게이지 완료 시점에 재생), 시전 불꽃과 돌진 자국, 투사체 섬광과 명중, 지속 장판 연출.
- [`WorldView.Enemies.cs`](../../Assets/HELLSCRIPT/Runtime/Presentation/WorldView.Enemies.cs): 떨어진 지속 장판을 속성 색으로 칠한다. 이전의 붉은 공 모양 피격 표시는 피해 기록을 읽는 새 반응으로 바꿨다.
- [`RuntimeEnemySmoke.cs`](../../Assets/HELLSCRIPT/Runtime/Presentation/RuntimeEnemySmoke.cs): 새 계정이 튜토리얼로 보내지고 장비 조합이 거부되어 첫 장면에 닿지 못하던 준비 코드를 고쳤다.

## 검증

2026-09-28, 복제 프로젝트와 macOS 개발 빌드(`180c9c36`)에서 확인했다.

- **Edit Mode**: 관련 검사 5종 251개 중 249개 통과. 실패 2개는 개편 전 기준선의 `EnemyTests.DeathPayloadRestoresIndependentlyAndNeverRepeatsItsReward`다.
- **`-hellscriptAttackFxSmoke` 통과**: 실제 게임 화면에서 다음을 확인했다. [결과](EnemyAttackFeedbackEvidence/runtime-attack-fx-smoke.txt)
  - 준비 중인 적마다 테두리가 달아올랐다.
  - 근접 공격이 떨어질 때 이펙트가 나오고 영웅이 맞았으며, 맞은 영웅이 번쩍였다.
  - 돌진 자국, 화염 장판, 날아가는 화살과 명중, 독 웅덩이, 정예 얼음 고리가 보였다.
  - 맞은 적이 번쩍이고, 쓰러진 적이 녹아 사라졌다.
  - 캡처: [준비 자세와 베기](EnemyAttackFeedbackEvidence/windup-and-strike.png), [돌진과 화염 장판](EnemyAttackFeedbackEvidence/charge-and-fire-patch.png), [투사체와 명중](EnemyAttackFeedbackEvidence/shots-and-impact.png), [독 웅덩이와 얼음 고리](EnemyAttackFeedbackEvidence/poison-pool-and-ice-ring.png), [적 피격과 사망](EnemyAttackFeedbackEvidence/enemy-hit-and-death.png)
- **예고 게이지·보스 패턴 스모크**: 같은 빌드에서 통과했다.
- **적 스모크**: 준비 코드를 고친 뒤 첫 실행(돌진 장면)이 처음으로 통과했다. 재시작 실행은 사망 폭발 장면(지연 폭발 세 개가 동시에 있기를 기다리는 단계)에서 멈춘다. 이번 항목의 화면 변경이 없는 빌드에서도 같은 단계에서 멈추므로, 원래 있던 장면 준비 문제로 기록한다. [비교](EnemyAttackFeedbackEvidence/enemy-smoke-comparison.txt)

## 확인하지 못한 것

- 실기기와 Android 빌드에서 보지 않았다. 모바일에서 이펙트 수는 라이브러리 규칙에 따라 절반으로 줄지만 성능은 재지 않았다.
- 몸은 아직 기본 도형이라 자세와 소멸이 작고 단순하게 보인다. 3D 모델을 연결하면 같은 입력으로 리그 애니메이션을 움직인다.
- 동작 줄이기 설정을 끄고 켜는 화면은 아직 없다.
- 사람의 눈으로 한 품질 평가가 아니다.
