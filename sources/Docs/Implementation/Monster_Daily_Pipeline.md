# 매일 몬스터 20종 추가 절차

작성일: 2026-10-11 · [English](Monster_Daily_Pipeline.en.md)

[필드별 몬스터 로스터](../Design/HELLSCRIPT_Monster_Roster_20261011.md)의 138종을 하루 20종씩 게임에 넣는 매일 루틴의 운영 문서다. 루틴은 이 문서를 그대로 따른다. 사용자가 정한 원칙(2026-10-11): 훈위안 무료 생성 20회를 매일 쓴다, 경량화·Codex 일러스트·게임 적용·몬스터 도감 등록까지 한 번에 한다, 추가한 날부터 균열에 등장한다, 검사를 통과하면 main에 자동 병합하고 위키까지 게시한다.

**상시 규칙(2026-10-15 사용자 재확인):** 매일 작업이 끝나면 그날 통과한 몬스터를 게임에 적용하고 main에 병합한다. 하루 안에 다 못 끝낸 ID는 상태 파일에 남기고 다음 날 이어서 하며, 새 검사 실패가 있을 때만 병합을 멈추고 보고한다.

> **아트 기준 변경 (2026-10-14):** 다크 판타지 제작은 종료됐다. 몬스터 프롬프트(`monster_specs.json`)와 초상화 프롬프트·참조(`portrait_prompt.txt`, `run_portraits.py`)는 캐주얼 StyleD(치비 3.5~4등신, 밝은 평면색, 무광, 친근한 표정, 피·잔혹 없음) 기준이다. 같은 날 만든 어두운 15종(N08 N23 N26 N27 N29 N30 N32 N34 B06 B07 N36 N37 N38 N39 N40)은 보류했고 새 프롬프트로 다시 생성한다. 리깅 기준은 그대로다.

> **출시된 28종 재제작 (2026-10-14, 사용자 결정):** 이미 출시된 파이프라인 몬스터 28종(B01 B02 B04 B05 N01 N03 N04 N05 N06 N07 N09 N10 N11 N14 N15 N16 N17 N18 N19 N20 N21 N22 N24 N25 N28 N31 N33 N35)도 캐주얼 StyleD로 다시 만든다. 상태 파일에서 `_shipped_dark_20261014`로 옮겼으므로 `state.py next`가 보류한 15종과 함께 day_order 순서로 돌려준다(하루 약 15종, 총 43종 약 3일). 재제작은 같은 ID의 모델·초상화를 교체한다: `monster_registry.py add`는 다시 하지 않고(이미 있음), `hunyuan_monster_install.py`와 `install_portraits.py`로 덮어쓰며 `.meta`·GUID는 보존한다. 리깅·자세 검토는 새 몬스터와 같다. 교체한 날 ID별로 옛·새 비교 이미지를 PR에 남긴다. 화면은 같은 ID를 읽으므로 코드 변경은 없다. N02 N12 N13 B03은 파이프라인 밖의 절차 제작 모델이라 이번 범위에 넣지 않았다.

## 0. 준비물과 위치

| 무엇 | 위치 |
| --- | --- |
| 생성 순서 | `tools/monster_pipeline/day_order.json` (로스터 10.1장 순서, 134종) |
| 프롬프트·체형·키 | `tools/monster_pipeline/monster_specs.json` |
| 진행 상태 | `AssetDeliverables/Hunyuan/monster_state.json` (저장소 밖), `tools/monster_pipeline/state.py` |
| 날짜별 원본 | `AssetDeliverables/Hunyuan/<YYYY-MM-DD>/` (`urls.txt`, `previews/`, `raw/`, `selection.json`, `portraits/out/`) |
| 브라우저 주입 스크립트 | `tools/monster_pipeline/hy3d_runner.js` |
| 몬스터 목록 | `Assets/HELLSCRIPT/Resources/Monsters/MonsterRegistry.json`, 원본 `Docs/Design/MonsterRoster/roster.json`, 도구 `tools/monster_registry.py` |
| 리깅·설치 | `tools/hunyuan_monster_ready.py`, `tools/hunyuan_monster_install.py` |

## 1. 시작 점검

1. `python3 tools/monster_pipeline/state.py show`로 밀린 단계를 본다. 이전 날의 `generated`·`picked`·`rigged`·`portrait`가 남았으면 오늘 생성보다 먼저 이어서 처리한다.
2. 최신 `origin/main`에서 작업 브랜치 `claude/monsters-<날짜>`를 worktree로 만든다. 다른 세션이 같은 체크아웃을 쓰므로 main 체크아웃에서 직접 작업하지 않는다.
3. 내장 브라우저에서 `https://hy3d.tencent.ai/`를 연다. 로그인이 풀려 있으면 생성은 건너뛰고 사용자에게 알린다(비밀번호를 입력하지 않는다). 남은 횟수는 페이지가 부르는 `POST /api/3d/quotainfo` 응답(`remainQuota`)을 `read_network_requests`로 읽는다. 하루 횟수는 한국 시간 새벽 1시 무렵 초기화된다.

## 2. 훈위안 생성

1. 오늘 대상: `state.py next <remainQuota>`. 
2. AI Creation → Text-to-3D, 모델 `3DGeneration-V3.1`, 면 수 50k를 고른다(PBR은 기본으로 켜져 있다).
3. `hy3d_runner.js` 내용을 javascript 도구로 실행하고, `window.__queue=[[ID, monster_specs.json의 prompt], …]` 뒤 `window.__start()`를 부른다. 동시 3개, 4번째는 거절되며 횟수는 줄지 않는다. 제출한 ID는 `state.py set submitted …`.
4. javascript 도구는 45초에서 끊기므로 백그라운드 `sleep`(10–15분)으로 기다린다. 작업 하나가 5–10분 걸려 20개에 약 1.5–2시간이 든다.
5. 끝난 작업은 `await window.__compact(window.__ready())`의 결과를 `python3 tools/monster_pipeline/expand_compact.py <day_dir>`에 넣어 `urls.txt`로 바꾼다(서명 URL을 압축해 옮긴다. 브라우저는 로컬 주소로 보낼 수 없다). `state.py set generated …`.

## 3. 후보 선별

1. `cd <day_dir> && python3 <repo>/tools/monster_pipeline/fetch_previews.py <urls 일부>`로 ID마다 후보 4개의 비교 이미지(`previews/<ID>_sheet.jpg`)를 만들고 직접 본다.
2. 기준: 실루엣이 탑다운에서 읽힌다, 팔다리가 몸에서 떨어져 리깅이 쉽다, 바닥·받침이 없다(있으면 메모), 로스터의 무기·특징이 보인다, 상처·노출된 살 같은 잔혹 묘사가 없다.
3. `python3 <repo>/tools/monster_pipeline/pick.py urls.txt "ID:후보:이유" …`로 고른 GLB만 받는다(`raw/<ID>.glb`, `selection.json`에 해시·이유). `state.py set picked ID=후보`.
4. 네 후보가 모두 기준에 못 미치면 프롬프트를 고쳐 `monster_specs.json`에 반영하고 다음 날 예비 횟수로 다시 만든다.

## 4. 경량화·리깅

`python3 tools/hunyuan_monster_ready.py --glb raw/<ID>.glb --id <ID> --archetype <specs.archetype> --height <specs.height> [--boss] --out <작업 폴더>` 뒤 비교 시트를 보고 자동 판정 결과를 확인한다. 실패하면 다른 체형·키나 다른 후보로 다시 한다. 통과하면 `python3 tools/hunyuan_monster_install.py --id <ID> --from <작업 폴더>`로 `Resources/World/HunyuanReady/`에 넣는다. 판정을 통과해도 시트의 `front` 칸이 옆모습이면 몸이 옆을 향한 것이므로 `--yaw -90`(또는 `90`)으로 다시 돌린다. 연속 실행은 `tools/monster_pipeline/retry_rig.py`가 후보 순서대로 시도한다. 비교 시트에서 준비 자세(windup)와 걷기 칸에 떨어져 나간 조각, 손에서 빠진 무기, 위로 들린 옷이 있으면 판정과 무관하게 탈락시킨다. 네 후보가 모두 탈락하면 그 ID는 상태 파일의 `_deferred`와 `selection.json`에 사유를 남기고 상태를 지워 다음 날 `state.py next`가 먼저 돌려주게 하며, `monster_specs.json` 프롬프트를 고친다(예: 가는 긴 다리 → 짧고 굵은 다리, 바닥판 → 평평한 발). `state.py set rigged …`. 세부 규칙은 두 도구의 도움말과 `Editor/HunyuanReady/HunyuanReadyImporter.cs`의 규격을 따른다.

## 5. Codex 일러스트

1. `python3 tools/monster_pipeline/run_portraits.py <스테이지> <day_dir> ID …` (한 번에 5–7종, 동시 3개 실행까지). 외형은 `monster_specs.json`, 강조색은 필드에서 정하고, 고른 후보 렌더를 디자인 참조로, 기존 초상화 두 장을 화풍 참조로 넘긴다. 1장에 약 1–5분.
2. `python3 tools/monster_pipeline/review_sheet.py <스테이지> <시트.jpg> ID …`로 모델과 초상화를 나란히 보고, 디자인이 다르거나 글자·테두리·잔혹 묘사가 있으면 그 ID만 다시 만든다.
3. `collect_portraits.py <스테이지> <day_dir>`로 보존한 뒤, 작업 브랜치에서 `install_portraits.py <repo> <day_dir>/portraits ID …`로 `Resources/Art/TrainingGround/Portraits/<ID>.png`에 넣고 `Docs/Art/Monsters/portrait-manifest.json`을 갱신한다. `state.py set portrait …`.

## 6. 게임 적용과 도감

1. 새 몬스터: `python3 tools/monster_registry.py add <ID…>`가 로스터 항목을 `MonsterRegistry.json`에 옮기고 한·영 문구를 `en.txt`에 넣는다. 기존 25종은 모델·초상화만 바뀌고 목록 항목은 그대로다.
2. `python3 tools/monster_registry.py validate`가 통과해야 한다(문구 누락, 초당 피해 예산, 행동 불가·둔화 상한, 리소스 존재).
3. 몬스터 도감은 `MonsterRegistry.All`을 읽으므로 목록에 들어간 몬스터가 도감에 바로 나타난다(도감 구현이 main에 들어온 뒤). 추가 작업이 필요 없는지 도감 화면 스모크로 확인한다.
4. 관련 EditMode 테스트(목록·전투 부품·HunyuanReady·도감)만 먼저 돌린다.

## 7. 검사·병합·게시

1. 최신 `origin/main`을 작업 브랜치에 합친다.
2. 마지막 코드 변경과 main 통합 뒤 한 번: 전체 EditMode, macOS 개발 빌드 런타임 스모크(균열에서 새 몬스터가 보이는 장면 포함), `python3 tools/check_ui_contract.py --verify`. main에서 이미 실패하던 검사와 새 실패를 구분한다.
3. 모두 통과하면 PR을 만들고 병합한다. 새 실패가 있으면 병합하지 않고 PR과 원인을 사용자에게 알린다.
4. 병합 뒤 위키 게시 절차(`AgentRules/Wiki_Publish.md`)대로 생성·검사·게시하고 공개 주소에서 확인한다. `state.py set shipped …`.
5. 빌드 복제본·스모크 산출물은 정리 규칙(`AgentRules/Artifact_Cleanup.md`)대로 정리한다. 원본 GLB·미리보기·초상화 원본은 보존한다.

## 8. 보고

오늘 추가한 ID·이름, 고른 후보와 이유, 실패·재시도, 검사 결과(통과·실패 수), PR·병합·위키 상태, 남은 횟수와 다음 날 대상, 검증하지 않은 것(실기기, 성능)을 한국어로 짧게 보고한다. 훈위안 결과물의 공개 배포 승인은 아직 열려 있다(`productionApproved: false`).
