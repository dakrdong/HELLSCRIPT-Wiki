# 캐주얼 UI 통합 인수 — 2026-10-17

**①·②·④·⑤의 범위별 네이티브 검증을 완료했다.** 같은 최종 앱으로 7개 프로세스가 모두 exit 0 및 입력·저장·출력 계약을 통과했고 155 PNG를 검수했다. 전체 캡처 기준 **149 PASS / 6 LIMITED / 확인된 제품 UI 실패 0**이며, 기록된 주요 UI는 155장 모두 통과했다. 아래에 출석 하단 스크롤 도달성과 활성 보스 등 검증 한계를 명시한다. 사용자 지시로 **③ 창고·⑥ 룬장인은 제외**했다.

## 통합 범위·소스

| 범위 | 반영 내용 |
| --- | --- |
| ① 광장 | 카드·제목·숨김 안내·조이스틱 장식과 반응형 치수 |
| ② 일일퀘스트·출석 | 화면 스킨, 선택·완료·수령 상태와 기존 입력·저장 경로 |
| ④ 공통 확인·알림 | 확인창·토스트 및 인스턴스별 버튼·프레임 표현 |
| ⑤ 사냥 칙령 | 필수 원문 치수·장식, 정책·스크롤·토글·2D 예시 색, 전용 탭 원화 4종 |

기존 도메인 거래·Back·공개 조건을 보존했다. ⑤ 후속은 19파일 중 17파일의 원본 바이트와 승인된 2파일 색상 scope를 통합했다. 전체 후 원화·소스·메타는 `a0745743`, QA 전용 후속은 `092d4b97`, 최종 실행 소스는 **`0fd79f62b99a40cfb8470b2f7521ff44db08f65d`**다. 마지막 후속은 검수 fixture·진단 정확성만 수정했으며 제품 UI·계정·밸런스 변경 0이다.

최종 source SHA256: `c0132640b03c3211cc2baf81085ff22f0eefef9a379d9afc79134a78bffc8f30`. 실제 Unity 컴파일 PASS, UI contract 15 / refresh 14 PASS, 소스 **1,191개 + 추가 21개 입력**의 검증 핀을 최종 실행 뒤 다시 확인했다. 이 검증 소스와 문서 게시 후 main SHA는 구분하며 게시 결과는 아래 별도 영수증을 참조한다.

## 전체 검사 원문과 집중 해소

| 실행 | 실제 결과 |
| --- | --- |
| 전체 EditMode **1회** | **5,817건: 5,802 통과 / 15 실패 / skip 0 / inconclusive 0**, 4,690.06초 |
| inventory 대조 | 목록 5,818 → 실행 대상 5,817. `WebGlOptimizationMeasurement.Capture` 1건은 **fixture 클래스 `[Explicit]`** 수동 출력 측정으로 미실행이며 skip 1로 세지 않는다. |
| 영향 범위 집중 8필터·26건 | **25 통과 / 1 실패**, skip 0 |
| HP 기대값만 보완한 정확 1건 | **1/1 통과**, skip 0. 원래 15실패의 미해결 NUnit 실패 **0건** |

15건은 shared reflection 초기화 11, Attendance Unity-null 1, unused localization 1, 도구 timeout 로그가 섞인 BossPhase 1, Field HP 스냅샷 1이었다. Field는 기존 레벨업 HP/MP 전회복 계약을 8 seed에서 확인한 뒤 HP 기대값만 갱신했다. 다른 48개 관찰값과 전투 런타임 코드는 유지했다. **26건과 후속 1건은 별도 실행이며 전체 무실패 또는 “집중 28건”으로 바꿔 세지 않는다. 전체 재실행은 0회다.**

원본 whole 결과 SHA256 `e79cba97c05fac67b24fbd80d709864f15465f310868f7c72428f43a691ef5f9`와 모든 실패 원문을 보존했다. `validation/integrated-validation-ledger.json` 및 `validation/unity/{whole-editmode-result.json,whole-inventory-reconciliation.json,post-whole-focused/summary.json}`, `validation/focused-fixes/field/verification-01/summary.json`이 근거다. Roslyn 타입 검사는 실제 NUnit 실행 결과와 구분했다.

## 원화 4종·출처

네 그림은 모두 **512×512 RGBA8 / 총 1,174,724 bytes**다. 1254×1254 raw 원본·개별 prompt·PNG/raw SHA·메타 GUID를 보존했고 실제 Unity import/consumer 8건 및 recursive budget을 통과했다. 기존 donor·registry 변경 0. 생성 모델명은 도구 미노출로 `unknown_not_exposed_by_tool`로 기록했다.

| PNG / 용도 | bytes | Library ID |
| --- | ---: | --- |
| `menu-edict-skills.png` / 스킬 | 259,120 | `libfile_91adae5de1d4819198d924de0b9819b9` |
| `menu-edict-loot.png` / 전리품 | 313,765 | `libfile_e8444cd5b7fc8191a84195a8bd5a0feb` |
| `menu-edict-bag.png` / 가방·정리 | 297,921 | `libfile_9b181715c80c8191af09359d5c072805` |
| `menu-edict-auto-equip.png` / 장비 추천 | 303,918 | `libfile_f1e0f479f6088191b30278d720fc8723` |

출처 정본은 `validation/painted-tab-art-intake-provenance.json`이다. 생산자 `/Users/t8g-2410-pn-005/Documents/Codex/2026-10-17/task-5/output/edict-icons/`에 manifest·`library-results.json`·prompts/raw/final과 delivery zip을 보존했다. Library 결과 SHA256 `6933c5221cbf3b74362f9babf0277eef9775006fe33c8ecfe0421a6e2c7ee0df`는 4/4 저장·픽셀 동일을 보고한다. 문서 작업에서 네트워크 재조회는 하지 않았다.

## 빌드 이력과 최종 동일 앱 인수

네이티브 빌드 요청·성공은 **3회**다. 새 프로세스로 같은 앱을 재사용한 실행은 추가 빌드로 세지 않는다.

| 앱 | 빌드 | 실제 결과·보존 이력 |
| --- | --- | --- |
| 최초, 소스 `6637e777` | `build_4414e0043b58` | 빌드 성공. Four·Attendance exit 1. Daily initial/resume·Edict initial/resume 계약 PASS. Daily 픽셀 17 PASS / 3 LIMITED(개발 콘솔 가림), Edict 26 scoped PASS. |
| Replacement, 소스 `092d4b97` | `build_b5de3eee516b` | 성공·오류 0/경고 227·16.852초·1,157,472,530B. Four 40·Daily 20·Edict 26 픽셀 PASS. 출석 월간 guard 실패와 명시적 동일 앱 재현의 inactive Boss rect 실패를 보존. |
| 최종, 소스 `0fd79f62` | **`build_734e0aeaa2ba`** | **성공·오류 0/경고 227·16.137초·1,157,474,782B. 같은 앱의 최종 7단계 계약 모두 PASS.** |

최종 bundle SHA256: **`d7be226610ab7fce002e1fbe6ce13aeb8130cf8e38ba9f220de02141578d641c`**. 세션 `20261017T073529Z-5947c13dfe74`의 `completion.json`과 단계별 영수증은 **Four 1 + Daily initial/resume 2 + Attendance initial/resume 2 + Edict initial/resume 2**, 총 7프로세스 exit 0 / 155 PNG를 확인한다. 전체 실행 248.506초다. Launcher의 `MANUAL_PIXELS_PENDING` 표시는 자동 실행 영수증의 범위이며, 실제 픽셀 인수는 아래 별도 수동 검수 기록으로 연결했다.

| 최종 앱 범위 | 실제 픽셀 인수 |
| --- | --- |
| Four | **40장 PASS**. KO/EN 광장·확인창·토스트·회전. |
| Daily | **20 PASS / 0 FAIL / 0 LIMITED**. 전체 7페이지·핵심 원본 8장; 이전 콘솔 가림 상태도 새 캡처로 확인. |
| Attendance | **69장: 63 PASS / 6 LIMITED / 0 FAIL**, 주요 UI 69/69 PASS. 아래 주간 하단 규칙의 도달성이 제한이다. initial/resume 거래·입력·save/restart 계약은 PASS. |
| Edict | **26장 scoped PASS**(overview 10 + 선택 탭 16). 네 전용 원화의 실제 게임 표시 포함. |

## 실패 해석·명시적 한계

- Replacement의 `Missing/disabled attendance-monthly`는 null과 effective interactability를 합친 오류여서 원인이 확정되지 않았다. 동일 앱 명시적 재현과 최종 앱은 그 구간을 통과했지만 **인과적으로 고쳤다고 주장하지 않는다**. 실패 당시 로그·앱·67장 재현 캡처를 보존했다.
- 후반 출석 실패는 보스·gravity가 없는 정상 Rift01에서 숨은 Boss rect까지 검사한 QA 정확성 문제였다. 최종 guard는 **가시 boss일 때 같은 겹침 assertion을 유지**한다. 두 방향 로그는 모두 `active=False / ACTIVE_BOSS_NOT_COVERED`다. 활성 보스 배치 적합성은 이번 실행으로 인수하지 않는다.
- 출석 세로 주간 화면 **6장**의 하단 규칙과 일부 stash는 viewport fold 아래다. 자체 텍스트 영역 높이 검사는 통과했다. 정상적인 스크롤 경계이며 기능 실패나 글자 잘림으로 단정하지 않는다. **그 하단까지 실제 스크롤 도달했는지는 명시적으로 검증하지 않아 LIMITED**로 남긴다.
- 초기 진단 후보의 `GetInstanceID()`가 CS0619를 낸 기록도 보존했다. 선택적인 ID 출력 조각만 제거해 호환성을 보완한 뒤 실제 컴파일을 통과했다. 최종 Attendance SHA256 `141270204bf94d291133471528d92d494e9778b653f890b7fe068e9dd0517ef1`. 추가 wait·retry·강제 enable 0, 제품/GameUI 변경 0이다.
- 고정 desktop player 화면의 인수다. 물리 모바일·터치·성능·Web 및 기록되지 않은 모든 탭/스크롤 위치를 검증했다고 주장하지 않는다. 픽셀 검수는 입력·거래의 별도 runtime 영수증을 대체하지 않는다.

## 보호·정리·게시 결과

최종 `validation/final-execution/final/final-preservation-receipt.json`은 소스 1,191 + 추가 21핀, 사용자 dirty 4파일(Town CrateStack FBX/A/N 메타 3 + `ProjectSettings.asset`)의 정확 SHA와 stash `e64a330e339e0309a07ce090cb50f0a3cfa4507f`를 확인했다. 문서 5개·미완성 `hero-manual-rig`·RAW는 보존했다. 실제 사용자 save 접근 0; 폐기용 offline·무음 fixture만 사용했다. 빌드 자동 설정 2개도 생성 바이트 보존 후 복구했다. 소유 native 7 PID 종료, 사용자 Editor PID 64285는 열린 채 stopped·scene clean·컴파일/검사 없음·audio original false 상태다.

완료된 모델 worktree/merged local branch 각 3개를 정리했고 ignored **1,953 entry / 21,788,126,934B(약 22GB)**는 같은 볼륨 `retained-ignored/`로 보존했다. 고유·미분류 입력과 Git object 영구삭제 0. 이 rename을 용량 회수로 집계하지 않는다. UI history bundle 3개·총 85,282B와 clone 밖 evidence도 보존했다.

**후속 main `[skip ci]` 게시·원격 SHA 대조·UI clone/root worktree 정리 결과는 별도 정본 `validation/publication-cleanup-receipt.json`에 실행 후 기록한다.** 이 네이티브 인수 문서가 게시·정리의 성공을 대신하지 않는다. 복구·명령 근거는 `validation/FINAL_CLEANUP_COMMANDS.md`와 retained manifests에 있다. GC/prune 재시도·force·Git object 영구삭제·추가 전체 검사·Web 빌드/게시·유료 CI·추가결제는 수행하지 않았다.

근거 root: `/Users/t8g-2410-pn-005/Documents/Codex/2026-10-14/task-23/ui-six-integration-20261017/`. 최종 4 family 픽셀 기록은 `validation/final-execution/final/{four,daily,attendance,edict}-pixel-review.json`, 자동 실행은 `validation/final-execution/sessions/20261017T073529Z-5947c13dfe74/completion.json`이다. 통합 소유자 정본은 `validation/scoped-acceptance-receipt.json`의 `PASSED_SCOPED_ACCEPTANCE_WITH_DOCUMENTED_LIMITS`다. 출석 픽셀 영수증 SHA256은 `c68f631a85d946daf6b696a950c59654a7c8d3ed4918d1402e02b5a436747a10`이다. 이전 실패·원화 출처·집중 ledger·정리 영수증을 함께 보존한다.
