# 몬스터 1일차 적용: 목록·데이터 전투·훈위안 모델

작성일: 2026-10-12 · [English](Monster_Day1_Integration_20261012.en.md)

[필드별 몬스터 로스터](../Design/HELLSCRIPT_Monster_Roster_20261011.md)를 매일 20종씩 게임에 넣기 위한 기반과 1일차 결과다. 매일 절차는 [매일 몬스터 20종 추가 절차](Monster_Daily_Pipeline.md), 모델 변환 규칙은 [훈위안 몬스터 READY 파이프라인](Hunyuan_Monster_Ready_Pipeline.md)에 있다.

## 들어간 것

- **몬스터 목록** `Resources/Monsters/MonsterRegistry.json`과 `MonsterRegistry`. 기존 25종(N01–N20, B01–B05)은 지금의 종류·패턴·수치를 그대로 설명하고, 새 몬스터는 목록의 데이터로 싸운다. 몬스터 도감은 이 목록을 읽는다(도감 구현 계획: [몬스터 도감 개발 계획](Monster_Codex_Plan_20261011.md)).
- **데이터 전투**: 공격 부품(근접·투사체·돌진·도약·잠행·장판·지원·소환)과 특성(오라, 정면 감소, 피격 반격, 매복, 보물 등), 부서지는 구조물(토템·깃발·등불·감옥·벽), 영웅 상태(기술 봉인, 저주, 독 중첩, 시야 감소). 로스터 138종 전체를 이 형식으로 적었고 보류 항목은 없다. 사냥 칙령 대상 분류도 목록의 역할로 정해진다.
- **출시**: N21·N22·N23·N33·B06이 목록에 들어가 균열에 나온다. 이 다섯은 아직 모델·초상화가 없어(`artPending`) 기본 도형 몸체로 보이며, 매일 루틴이 2일차 생성분으로 채운다.
- **훈위안 모델 20종**(1일차): N01 N03 N04 N05 N06 N07 N09 N10 N11 N14 N15 N16 N17 N18 N19 N20, B01 B02 B04 B05. 자동 경량화(LOD 6,000/3,000/1,500, 보스 12,000/6,000/3,000)와 자동 리깅, QA 시트 검토를 통과했다. N08은 튜닉이 팔에 딸려 올라가 제외했고 기존 모델을 쓴다(새 프롬프트로 다음 생성 때 다시 만든다).
- **Codex 초상화 19장**: 고른 모델 렌더를 디자인 참조로 그려 `Resources/Art/TrainingGround/Portraits/`의 같은 ID를 바꿨다. 기록은 `Docs/Art/Monsters/portrait-manifest.json`.
- **매일 루틴 도구** `tools/monster_pipeline/`과 루틴 프롬프트 `ROUTINE_PROMPT.md`.

## 고친 문제

- 공격 시작 거리가 공격 반경보다 길어 영원히 빗나가던 근접 공격 5종(N21·N35·N42·N73·N85)을 반경에 맞췄다. 목록 도구가 이런 데이터를 거절하고, `MonsterEngagementTests`가 로스터 전체의 데이터 몬스터·보스가 서 있는 영웅을 실제로 맞히는지 본다.
- 몬스터 스킬 문구 키 `자폭`이 사냥 칙령 대상 분류(`EXPLODER`)와 겹쳐 `자폭 돌진`으로 바꿨다.

## 검증

| 검사 | 결과 |
| --- | --- |
| 전체 EditMode (병합 후보, main 통합 뒤) | 5,949개 중 통과 5,945 · 실패 0 · 건너뜀 4 (54분) |
| 그 뒤 바꾼 부분의 관련 EditMode (목록·전투·소크·필드·서버 재현·READY·사냥 칙령 대상 + 새 `MonsterEngagementTests`) | 238개 중 236개 통과. 새 테스트 2건은 매복형 배치와 보스 기준이 잘못돼 실패했고, 테스트를 고친 뒤 `MonsterEngagementTests` 2/2 통과(나머지 236개는 테스트 파일만 바뀌어 다시 돌리지 않음) |
| `tools/test_monster_registry.py`, `monster_registry.py validate` | 10/10, OK |
| `check_ui_contract.py --verify` | 15 OK |
| macOS 개발 빌드 런타임 스모크 `-hellscriptMonsterModelsSmoke` (1600×900) | 통과. READY 모델 25/25가 모델로 보였고 데이터 몬스터 4종·보스 B06이 예고 뒤 적중 (근거: [boss-B01](MonsterModelsEvidence20261012/boss-B01.jpg) · [boss-B02](MonsterModelsEvidence20261012/boss-B02.jpg) · [boss-B03](MonsterModelsEvidence20261012/boss-B03.jpg) · [boss-B04](MonsterModelsEvidence20261012/boss-B04.jpg) · [boss-B05](MonsterModelsEvidence20261012/boss-B05.jpg) · [boss-B06](MonsterModelsEvidence20261012/boss-B06.jpg) · [fight-B06](MonsterModelsEvidence20261012/fight-B06.jpg) · [fight-N21](MonsterModelsEvidence20261012/fight-N21.jpg) · [fight-N22](MonsterModelsEvidence20261012/fight-N22.jpg) · [fight-N23](MonsterModelsEvidence20261012/fight-N23.jpg) · [fight-N33](MonsterModelsEvidence20261012/fight-N33.jpg) · [lineup-N01-N05](MonsterModelsEvidence20261012/lineup-N01-N05.jpg) · [lineup-N06-N10](MonsterModelsEvidence20261012/lineup-N06-N10.jpg) · [lineup-N11-N15](MonsterModelsEvidence20261012/lineup-N11-N15.jpg) · [lineup-N16-N20](MonsterModelsEvidence20261012/lineup-N16-N20.jpg) · [lineup-N21-N33](MonsterModelsEvidence20261012/lineup-N21-N33.jpg) · [runtime-monster-models-smoke](MonsterModelsEvidence20261012/runtime-monster-models-smoke.txt)) |

## 검증하지 않은 것

실기기(Android·iOS), APK·웹 빌드 크기(저장소의 HunyuanReady 원본 PNG는 197MB, 빌드 크기는 압축 뒤 값이라 따로 재야 한다), 프레임과 그리기 호출, 전체 런타임 스모크 묶음(이번 변경과 관련된 새 스모크만 실행), 훈위안 결과물의 공개 배포 승인(`productionApproved: false`).
