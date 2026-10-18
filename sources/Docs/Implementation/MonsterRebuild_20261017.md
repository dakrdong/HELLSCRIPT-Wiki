# 캐주얼 몬스터 재제작 9종 · 2026-10-17

[English](MonsterRebuild_20261017.en.md)

출시된 몬스터 8종(B01 N03 N07 N08 N09 N10 N19 N25)과 아트 대기였던 N23을 캐주얼 StyleD 모델·초상화로 교체했다. 같은 ID의 모델·초상화를 덮어썼고 `.meta`·GUID는 그대로다. N23은 `art-ready`로 해제했다. 코드·저장 키 변경은 없다.

| ID | 후보 | 비고 |
|---|---|---|
| N19 | 3 | |
| B01 | 2 | |
| N03 | 3 | |
| N07 | 2 | 후보 3은 몸에서 조각이 떨어져 탈락 |
| N08 | 0 | |
| N09 | 0 | |
| N10 | 1 | |
| N23 | 2 | |
| N25 | 0 | |

**보류(다음 날 다시 생성):** N18, N06, B02는 후보 4개가 모두 리깅 판정 또는 준비·걷기 자세 검토에서 떨어졌다(긴 머리·넓은 모자 챙·긴 옷단·촛대 고리가 찢기거나 튐). `monster_specs.json`의 프롬프트를 몸에 붙는 형태로 고쳤다.

**검사:** HunyuanMonsterReadyTests 39/39, 몬스터·도감·사냥 지침·균열 관련 EditMode 269/269, `check_ui_contract.py --verify` 15건 통과, `monster_registry.py validate` 통과. 전체 EditMode는 사용자 지시로 실행하지 않았다.

**모델 스모크:** `-hellscriptMonsterModelsSmoke`는 N28·N31·N33·N35 묶음에서 "모델 몸 3개, 4개 필요"로 실패한다. 오늘 바꾸지 않은 몬스터이고, 변경 전 main에서도 같은 이유로 실패하는 기존 실패다. 그 앞의 N01~N25 줄세우기 화면 5장은 찍혔다.

- [N01~N05](MonsterModelsEvidence20261017/lineup-N01-N05.jpg) · [N06~N10](MonsterModelsEvidence20261017/lineup-N06-N10.jpg) · [N11~N15](MonsterModelsEvidence20261017/lineup-N11-N15.jpg) · [N16~N20](MonsterModelsEvidence20261017/lineup-N16-N20.jpg) · [N21~N25](MonsterModelsEvidence20261017/lineup-N21-N25.jpg)
- [옛 초상화와 새 초상화 비교](MonsterModelsEvidence20261017/portraits-old-vs-new.jpg)

미검증: 실기기·성능·빌드 크기, 위키 게시.

## 게임 적용 마무리 · 2026-10-17

N28은 감지 거리 4m의 매복형인데, 모델 스모크는 5m 줄세우기에서 일시정지했다. 따라서 정상적으로 숨은 N28이 모델 누락처럼 판정됐다. 줄세우기 fixture만 `brain.revealed=true`로 설정하고 실제 활성화된 모델만 센다. 일반 게임과 공격 fixture의 매복 동작은 유지했다.

- 관련 EditMode 156/156 통과(HunyuanMonsterReadyTests, MonsterPrimitiveTests, TownModelTests, TownEnvironmentTests), UI 계약 15/15, registry validate 통과. 전체 EditMode 미실행.
- 최종 macOS 개발 빌드: READY 모델 34/34, N28·N31·N33·N35 4/4, 데이터 몬스터 9종과 B06 공격 적중 통과. B06은 아직 모델 미설치이므로 primitive가 정상이다. [최종 결과](MonsterModelsEvidence20261017/runtime-monster-models-smoke-final.txt).
- 첫 증분 빌드는 URP 셰이더 누락으로 실행 실패했다. 같은 소스를 Clean Build로 재생성한 뒤 위 게임 검증을 통과했다. 관련 EditMode는 반복하지 않았다.
- 교체 9종의 모델·초상화와 artPending 해제를 확인했다. 미수령 후보 다운로드·새 생성은 진행하지 않았다.
- 빌드·실행 로그·화면·리소스 연결 확인과 산출물 보존 기록: `Builds/Validation/ModelIntegration20261017/`. 위키 게시·기존 scratchpad/worktree 삭제는 이번 범위에서 진행하지 않았다.

공유 체크아웃에 검사 도중 마을 PR #141(`ce06385e`)이 반영됐다. 최종 플레이어는 이 변경을 포함하며, 달라진 마을 검사만 다시 실행해 27/27 통과했다. 기존 몬스터 검사 입력은 바뀌지 않아 재사용했다. 최종 유효 범위는 몬스터 132건과 마을 27건이다.
