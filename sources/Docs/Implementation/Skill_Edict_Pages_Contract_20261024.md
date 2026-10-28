# 스킬별 사냥칙령 하위 페이지 — B0 저장·계약 설계 메모 (2026-10-24)

> **2026-10-28 A2 정정:** 큰 칙령 창·≡ 메뉴·전용 부품·스모크 설명은 과거 기록이다. [현재 진입 코드](../../Assets/HELLSCRIPT/Runtime/Presentation/GameUI.HuntEdict.cs)와 [대체 화면·소유 코드](Hunt_Edict_Window_Removal_20261028.md)를 따른다. 이전 캡처와 검사 수치는 당시 구현의 근거다.

[English](Skill_Edict_Pages_Contract_20261024.en.md). 사용자 항목 17(Part B)의 B0 계약과 아래 구현 기록을 함께 둔다. **B0·B2의 화면 없는 데이터·편집·저장 기반을 구현했고, B1 화면은 직접 시안 승인 근거 확인 전까지 미작성이다.** 초기 제안은 아래 보존하며, 현행 계약은 마지막 구현 기록을 우선한다. 옵션의 의미·수치는 [`HELLSCRIPT_Hunt_Edict_Tactical_Options.md`](../Design/HELLSCRIPT_Hunt_Edict_Tactical_Options.md)가 기준이다. `HEC-*`는 옛 옵션, `HET-*`는 새 기획 행 번호이며 새 저장 키나 화면 문구로 쓰지 않는다.

## 한 줄 요약 (사용자 보고용)

스킬별 값은 `ClassSkillLoadout`(v5)에 **희소 목록**(`스킬 id + 보조 id + 행 키 + 값`, 기본값이면 생략)으로 싣는다. `HuntEdictV2Document`는 모르는 필드를 버리므로 쓰지 않는다. 편집본은 페이지가 따로 들고, [저장]만 기존 원자적 저장 한 번을 호출하며, [되돌리기]는 마지막 저장본 복원이다.

## 코드로 확인한 사실 (경로는 `Assets/HELLSCRIPT/Runtime/Core/`)

- `HuntEdictV2Document`(`HuntEdictV2.cs:14-25`)는 `Canonical`이 알 수 없는 키를 거부하고 통과해도 `heroClass, slots, global, skillOrder, skills`만 담은 새 문서를 만든다(`:58-69`) → 새 필드를 여기에 두면 조용히 사라진다. 스킬별 값(`EdictOption{id,value}` 문자열 쌍)은 레거시 6개 스킬 + BASIC의 `HEC-*` 행뿐이고 D4 액티브(W02, A03, M04 …)에는 행이 없다(`HuntEdictV2.Options.cs:33,131-135`).
- `ClassSkillLoadout`(`ClassSkills.cs:75-88`)은 `actives[4], ultimate, order[], automatic[], ranks(모든 보조 선택 포함), options, presetSelections, legacyEdict`를 가진다. D4 `Canonical`은 `options`/`presetSelections` ID를 `Split(':')[0]`으로 자르고 `KeptScope`(BASIC·액티브·패시브)만 허용한다(`SkillTreeD4/SkillTreeD4.Loadout.cs:39-42`) → 보조 ID(`W02-3` 꼴)는 지금 거부된다.
- 편집·저장: `HuntEdictEditSession`의 `Draft`는 `saved.Copy()`(JsonUtility 왕복), `Dirty`는 JSON 비교, `Revert`는 `Draft=saved.Copy()`(`HuntEdictLoadout.cs:93-147`). `CommitHuntEdictForHero`는 전체 검증 → 복제본(`staged`)에 적용 → `Write`(tmp 파일 + `File.Replace`) 성공 뒤에만 채택하고 실패하면 `Error`를 세팅하고 Draft를 유지한다(`GameStore.HuntEdict.cs:11-69`, `GameStore.cs:439-513`).
- **함정**: 스킬 창의 포인트 커밋(`CommitD4Allocation`)은 새 `HuntEdictEditSession`을 만들어 커밋하고(`GameStore.D4Allocation.cs:29,33`), 창은 커밋 뒤 `Session.AcceptSaved`를 부른다(`HuntEdictWindow.cs:175`). 칙령 편집본을 이 세션의 Draft에 두면 포인트를 투자하는 순간 사라진다 → 하위 페이지는 **자기 편집본**을 따로 든다.
- 5개 영속 프리셋은 `HeroSave.edictPresets[].loadout`(전체 `HuntEdictLoadout` → `ClassSkillLoadout`)이라 `ClassSkillLoadout`에 둔 새 목록은 복제·프리셋·HED6 공유 경로를 자동으로 탄다. 다만 `CopyPolicy`(스킬 scope별 복사, `HuntEdictSkillPresets.cs:62-91`), `SamePolicy`(`:58`, 점진 공개 게이트 `HuntEdictProgression.cs:166-183`), D4 `Canonical`의 가지치기를 함께 고쳐야 한다.
- 새로 장착한 스킬의 자동 사용 기본 켜기는 이미 `ClassSkillTree.Equip`에 있다(`ClassSkillTree.cs:54,59`: `if(!already) automatic=…Concat(id)`). 해제하면 `NormalizeActions`가 `automatic` 멤버십을 지워 다시 장착하면 켜짐으로 돌아간다(`:43-47,69`) — "해제는 규칙·자동 사용 상태를 삭제하지 않는다"(구현 계약 20행)와 다르다.
- 전역 옵션(대상 등급·추적·위험·HP·긴급 생존)은 `EdictOptions.Global`(`EdictOptions.cs:41`)이고 편집 화면은 옛 칙령 창의 `combat`/`survival` 탭이다. 링크 대상은 `HuntEdictWindow.SelectTab("combat"|"survival")`, `FocusGlobalOption(id)`(둘 다 public). `GameUI`에는 그 진입 래퍼가 없다.

## 결정 (확정 제안)

**(a) 저장 위치와 모양.** `ClassSkillLoadout`에 `tactics`(선택 목록)를 추가한다. 한 항목 = `{ skill, scope, row, value }` — `skill`은 부모 액티브 id(`W02`), `scope`는 `""`(부모 행) 또는 실제 보조 id(`W02-3`), `row`는 데이터 행 키, `value`는 문자열(`ClassSkillOptionSelection`과 같은 방식). 값이 기본값이면 항목을 두지 않고(희소), 정렬은 Ordinal. 다른 보조의 값은 `scope`가 달라 그대로 남고 해당 보조가 선택되기 전에는 발동하지 않으며, 교체된 부모 행의 값도 그대로 보존된다(현재 형태에는 미적용). JsonUtility는 `Dictionary`를 직렬화하지 못하므로 목록으로 둔다.

**(b) 버전·하위 호환·원본 보존.** 새 필드는 비어 있을 때 직렬화에서 생략해 이미 발급된 HED6 코드가 계속 읽히게 한다(`ClassSkillCodec.Wire()`가 이미 빈 필드를 문자열 치환으로 빼는 선례가 있다, `ClassSkillCodec.cs:23`). 값이 있는 새 코드는 옛 빌드가 거부한다(HED6 정규형 엄격 검사) — 새 공유 자료에는 별도 스키마 버전이 필요하다는 기존 기준(`Skill_Option_Catalog` 474행)에 따라 구현 PR에서 계약 문서를 같이 개정한다. 새 저장 키에는 `HEC-*`도 `HET-*`도 쓰지 않는다.

**(c) 편집본·저장·되돌리기.** 페이지가 열릴 때 그 스킬의 `tactics`(+ 자동 사용)를 복제해 편집본으로 쓴다. 저장 전에는 실제 설정이 바뀌지 않는다. [저장]은 새 `GameStore` 메서드(예: `CommitSkillTactics(heroId, skillId, rows)`)가 `HuntEdictEditSession`으로 해당 스킬의 항목만 바꿔 `CommitHuntEdictForHero`에 넘긴다 — 원자적 저장 한 번, 실패하면 편집본·`Error` 유지. [되돌리기]는 편집본 = 마지막 저장본. 칙령 편집본 저장과 Part A의 '닫을 때 슬롯 저장'은 서로 독립이며 코드·문구에서 분명히 한다. 5개 프리셋·퀵 프리셋은 기존 흐름 그대로이고, 퀵 프리셋 적용이 숨은 값을 남기지 않는 기존 테스트(`HuntEdictQuickPresetTests.cs:62`)를 확인한다.

**(d) 자동 사용.** 토글은 하위 페이지 한 곳에서만 쓰며 `automatic[]`가 계속 유일한 소유자다. 신규 유효 장착 카드는 기본 켜기(`Equip`이 이미 구현), 가져오기는 원래 Off를 보존(`HuntEdictPresetImport.Prepare`가 `automatic`를 덮어쓰지 않는지 구현 때 테스트로 고정). 해제 때 Off 상태를 잃는 현재 동작은 `tactics`의 `row="auto", value="0"`(희소 항목)로 보존하고 `Equip`이 이를 읽도록 하는 안을 권장하되, 구현 PR에서 사용자에게 확인한다.

**(e) 행 정의는 데이터.** `Resources/Data/SkillTactics.json`(제안 이름): 행 키, 종류(toggle/choice/ref/num), 선택지(스킬 참조 인덱스 포함), 최소·최대·간격·기본값·단위, 비활성 사유 규칙, 소속(부모/보조 id), 교체하는 부모 행 목록, 요약 규칙. 수치·선택지·기본값은 옵션 문서의 표를 그대로 옮기며, 문서 대비 대조 테스트는 문서가 없으면 건너뜀으로 표시한다.

**(f) 전역 설정 링크.** 현재 연결은 [전술 창의 복귀 처리](../../Assets/HELLSCRIPT/Runtime/Presentation/SkillTreeD4Window.Tactics.cs) → `GameUI.ShowEdictTab("combat")` → 사이드 메뉴 전투 시트다. 생존은 같은 `ShowEdictTab("survival")` 경로를 사용한다. 삭제한 통합 창을 열지 않는다.

**(g) 편집 항목 상한.** D4 카드당 최대 7개(자동 사용 포함). 옛 `Hunt_Edict_Detail`의 8개 상한은 기존 v0.2의 이력이며 D4 새 카드는 7개를 따른다. 초기 메모의 A02 8행 우려는 현재 옵션 문서 §3·목록과 맞지 않는다. 문서는 장비와 보조가 같은 두 행을 재사용하고 추가 행을 만들지 않도록 정했으며 A02는 6행이다.

**(h) 진입 버튼(사용자 지시 2026-10-24).** 진입은 상세 패널 머리글의 스킬 이름 오른쪽 44px 아이콘 버튼이다(고정 하단 행이 아님 → 패널에 스크롤이 생기지 않고 프레임 좌표도 그대로). 버튼의 배지가 저장된 자동 사용 상태(✓ 켜짐 / – 꺼짐)를, 미리 보기(미선택 보조·미습득)는 파란 `i`를 보이며, 전체 접근성 이름에 현재 전술 요약을 담는다. 요약 줄은 하위 페이지 상단 정보 카드에 있다. 버튼 그림은 Codex가 만든 메달리온(`Docs/Art/SkillEdictIcon20261024/`, 모델 미확인·production 승인 전)이며 구현 때 `Resources/Art/GlobalHUD/Casual/`로 옮긴다. 긴 스킬 이름(예: `War of the Ancestors`)은 이름 칸이 좁아지므로 구현 때 기존 `fitFs` 식 자동 축소를 쓴다.

## 구현 전 확인할 것 / 미결

- 옵션 기획 문서(`Tactical_Options.md/.en.md`)를 저장소에 커밋해야 테스트가 ID를 대조한다 — 사용자 파일이라 임의로 커밋하지 않았다.
- 자동 사용 Off일 때 같은 카드의 다른 행을 지울지·접을지·회색 처리할지는 기획안에 없다. 시안은 "행은 그대로 편집 가능 + 값 보존 + 안내 한 줄"로 가정했다.
- 전투가 옵션을 따르게 하는 구현은 이 단위 범위 밖이다(저장만 되고 전투에는 반영되지 않음). 진입 행을 개발·시안 호스트에서만 보이게 하는 단일 스위치가 기본값이다.
- 유니티 직렬화 깊이 한도는 저장소에서 확인하지 못했다. `ClassSkillLoadout` 바로 아래에 얕게 두면 현재 가장 깊은 경로(8단계)를 늘리지 않는다.
- Part B 구현 순서: 데이터·검증 테스트 → 저장 메서드·편집본 로직 테스트 → 하위 페이지 UI → 대표 3종. Part A 병합 뒤에 시작한다(같은 스킬 창 상세 영역·닫기 흐름을 고침).

## 2026-10-24 구현 기록 — B0/B2 기반, B1 승인 대기

**결정이 필요한 경계:** Part A의 직접 승인 원문은 착용 스킬 줄 구현을 명시했다. 이 기록에서는 그것을 Part B 새 화면 승인으로 확대하지 않는다. `U-E-skill-window-flow.md` §5·§9.5는 직접 시안 승인 전 B1 화면 코드를 금지한다. 따라서 새 진입 버튼·하위 페이지·선택 목록·팝업은 작성하지 않았다. 기존 [시안 URL](https://claude.ai/artifact/JsCKnJUbKkzfBYyJDR3493)은 인계 기록에서 확인했으며, 이번 공개 읽기 도구로는 열리지 않아 현재 보드 내용이나 승인을 확인했다고 주장하지 않는다.

원문 §9.4·§9.5는 전투 시뮬레이션의 스킬별 행동 변경을 명시적으로 후속 단위로 뺀다. 이번 전투 미반영은 누락이 아닌 확정 범위다. `SkillTacticsCatalog.PlayerEntryEnabled=false`이며 실제 플레이어 호스트는 편집 세션을 열 수 없다. 개발/시안 호스트도 현재는 화면 없는 API다. 실제 전투 실행 코드는 변경하지 않았다.

시작 기준은 최신 `origin/main 1f1887d1148d99e5b5f42f9a899d8945c6209259`다. Part A `5f2ff71f719a5588fe1ae6b3f5129d930dae9c93`를 의존 커밋 `451bb130ea93f61b987865e98cb0f4898feb2a24`로 적용했고, 번역 파일의 추가 문구 충돌은 양쪽을 보존했다. 루트 원본·사용자 테스트본·세이브는 바꾸지 않았다. 사용자가 허용한 `Tactical_Options.md/.en.md` 두 원본은 내용 그대로 복사해 이 브랜치에 포함한다.

### 실제 저장·호환 계약

- `ClassSkillLoadout.tacticsVersion`의 0은 기존 저장, 1은 새 희소 목록이다. 항목은 `{skill, scope, row, value}`이며 부모와 실제 보조 ID로 구분한다. 기본값은 생략하고, 다른 보조·교체된 부모·알 수 없는 행은 보존한다. 1보다 새로운 버전은 보존하되 편집/공유 발급을 거절한다. 구형 독립 M04-3·HEC 옵션을 D4 보호막으로 자동 변환하지 않는다.
- `SkillTacticsEditSession`은 마지막 저장·편집본·요청한 이동을 따로 갖는다. `CommitSkillTactics`는 최신 저장본과 같은 카드 값/보조가 맞는지 검사한 뒤 보이는 행만 병합해 `CommitHuntEdictForHero`를 한 번 호출한다. 다른 스킬·전역·포인트·슬롯·미지원 행은 최신 저장본에서 유지한다. 저장 실패는 디스크·현재 설정·편집본을 그대로 유지하며 성공 알림만 기존 `Committed`를 사용한다.
- 되돌리기는 마지막 저장 성공본이다. 뒤로/전체 닫기/전역 이동은 저장 후 이동·버리고 이동·계속 편집을 같은 세션 규칙으로 처리한다. 반환 상태는 직업 탭·선택 노드·상세 노드·트리/상세 스크롤·분할 비율을 복제해 보관한다. 실제 uGUI 복원 연결은 B1에서 수행한다.
- `automatic[]`가 장착 스킬의 자동 사용 소유자다. 희소 부모 행 `auto=0`은 미장착 때 Off 선호를 보관한다. 새 카드 기본 On, 가져온 Off, 해제/재장착 Off 보존을 `NormalizeActions/Equip/SetSlots`에 연결했다. 다른 전술 행은 Off여도 편집·보관 가능하다.
- 기존 HED4/HED6의 새 필드가 비어 있으면 코드에서 생략해 기존 정규 바이트를 유지한다. 새 값이 있으면 직접 스킬 코드는 `HED7`, 프리셋 봉투는 `HED8`(내부 스키마 6)로 구분한다. 지원하지 않는 JSON 필드는 원문을 버리고 변환하지 않고 거절한다. 기존 5개 영속 프리셋의 전체 복사와 scope별 `CopyPolicy/SamePolicy`에 새 목록을 포함했다. 기존 간편 프리셋의 옛 실행 옵션을 새 전술로 추정 매핑하지 않는다.

### 원문·시안 대조와 표현 유형

대표 세 종의 선택지·범위·간격·기본값은 최신 원문과 첨부 시안 사양이 일치한다. 스킬 ID로 적힌 참조는 카탈로그에 안정 ID와 한·영 실제 이름을 따로 두며, HET 번호는 대조용 출처에만 둔다. 총 20행을 입력했고 최대 7행이다.

| 유형 | 데이터·화면 없는 계약 | B1에서 승인 후 연결 |
| --- | --- | --- |
| 토글 | auto 단일 소유, Off 값 보존 | 페이지 한 곳의 토글, 진입은 읽기 전용 배지 |
| 단일 선택 | 안정 선택 키·한/영 이름·기본값 | 짧은 2~3개 버튼, 긴/다수 선택 목록 |
| 수치 | decimal 범위·간격·단위, 클램프/반올림, 끝 버튼 불가 | 44px 이상 −/+와 범위/기본값 안내 |
| 스킬 참조 | 실제 해금·장착·자동 허용·필요 보조 사유 | 아이콘+이름과 선택 불가 이유 |
| 조건·행 교체 | whenRow/whenValues, 보조 replaces, 숨긴 값 보존 | 미적용 표시, 현재 보조만 표시 |
| 설명·전역 제한 | 카탈로그 한/영 notes·reason | 호버 없이 읽는 안내, 기존 전역 창 연결 |

### 승인 검토 대상 — 대표 상태

아래는 실제 카탈로그·세션에 연결할 구체적 검토 목록이며, 완성 화면이나 승인 기록이 아니다. 기존 시안의 프레임·탭·스크롤·분할을 재사용한다. Part A 계약의 진입 위치는 **상세 제목 오른쪽 44px 아이콘**이며 구 프롬프트의 하단 행으로 되돌리지 않는다. 각 대표에서 440×956, 956×440, PC 16:9·16:10·21:9와 한국어/영어 기본 글자를 확인해야 한다.

| 검토 ID | 대상과 기본 화면 | 변경·이동에서 확인할 상태 |
| --- | --- | --- |
| A | W02+W02-3: On / 둘 다 / 무리 가장자리 / 4m / 자유 / 한 충전 탈출용 / 1충전 (7행) | 2↔8m·0.5 간격, 0↔1 충전, Off에도 행 편집, 탈출에서는 거리 미적용 표시, W01/W03/W04 불가 사유 |
| B | A03: On / 접근 길 매복 / 기존 유지 / 새 적 담당 (4행), A03-3: On / 현재 교전 가장자리 / 주 대상 우선 (3행) | 덫 3행 제거·부모 값 보존, 미선택 보조 미리 보기 저장 비활성, 진지 변경 뒤 저장/복귀 요약 |
| C | M04+M04-3: On / 생존 탈출 / 없음 / 자유 / 전역 위험 기준 / 보호 받고 재배치 / 실제 마지막 원소 그대로 (7행) | 긴 한/영 선택 목록, 내 M02/M08/M11의 실제 이름·불가 이유, 글로벌 보호·긴급 시전 안내, 원소/보호막 무료 지급 없음 |
| 공통 | 변경 표시·되돌리기·저장 고정, 본문만 스크롤 | 세 가지 이탈 선택, 실패 뒤 초안 유지, 성공 요약 갱신, 트리/상세/분할 그대로 복귀, 슬롯 저장과 독립 |

### 검사와 미실행

`python3 -B tools/check_skill_tactics.py --report Builds/Validation/SkillTactics20261024/catalog-check.json`으로 한·영 원문 293행·대표 20행·선택지·기본값·수치·행 교체·요약 참조를 대조했다. 미입력 부모 **27종**, 보조 **81종**의 정확한 ID는 같은 결과와 `SkillTacticsCatalog.MissingNodes()`가 출력한다. 미입력 노드와 지원하지 않는 현재 보조에는 편집 세션을 만들지 않는다.

`SkillTacticsTests`는 카탈로그, 숫자 끝/소수, 참조 불가, Off, 교체/알 수 없는 행 보존, 미리 보기, 실패/재시도/되돌리기, 3지선다, 반환 상태, 슬롯 독립 저장, 오래된 초안 거절, HED6 호환·HED7/HED8 왕복·원문 보존·5개 프리셋을 검사한다. **이 브랜치에서는 Unity·Edit Mode·빌드·화면 캡처를 실행하지 않았다.** 상위 통합 담당이 실제 최종 후보에서 집중 실행하며 결과 전까지 컴파일/실행 통과를 주장하지 않는다. 새 씬·프리팹·패키지·설정·전투 코드는 바꾸지 않았다.

### B3 후속 분할

1. 전사 데이터: 나머지 부모 9종·보조 27종. 같은 카탈로그만 채우고 원문 대조·7행 검사를 통과시킨다.
2. 궁수 데이터: 나머지 부모 9종·보조 27종. A02 장비/보조의 동일 행 재사용을 포함한다.
3. 마법사 데이터: 나머지 부모 9종·보조 27종. 독립 구형 M04-3 원본과 D4 보조의 구분을 유지한다.
4. 전사 전투: 10종·보조 28종의 실제 착지·제어·충전·후속 사건과 전역 생존 제한을 연결한다.
5. 궁수 전투: 10종·보조 28종의 실제 덫/쇠뇌·주입 소비·화살/잔상 사건을 연결한다.
6. 마법사 전투: 10종·보조 28종의 실제 장판 소유·원소 기록·설치/만료·제어 사건을 연결한다.

각 전투 단위는 현재 실행 옵션의 원본을 보존하고 명시적 변환 초안·실제 사건·실패/재진입을 별도 증명한다. 전투 미연결 상태에서 플레이어 진입을 켜지 않는다. B1 화면 승인과 구현/검증은 이 데이터 확장의 공통 선행 조건이다.

## 통합 후보 집중 검사 갱신 (2026-10-24)

최신 main 1f1887d1을 포함한 후보 5936415e에서 E/H/F와 직접 호출자 23개 fixture를 실행했다. 최초 컴파일 오류는 새 E 테스트의 List.Length를 Count로 고쳤다. 이후 550개 중 521개 통과, H/F 29개는 EditMode EventSystem 가정에서 실패했다. 최종 H/F 40/40 통과와 바뀌지 않은 나머지 510개 근거를 합쳐 단위 범위 550/550을 확인했다. 런타임 코드는 이 테스트 보정으로 바뀌지 않았다. 근거: 로컬 takeover/e-h-f-unit-proof.json, e-h-f-retest/results.xml, h-f-final-unit/results.xml. 로컬 CI 9단계도 통과했다(takeover/integration-ci/report.json; 설치 Python3.9.6/Node26.8.2, 워크플로우3.12/22와 정확 버전 동등성은 미확인).

EditMode에서 새 EventSystem은 전역 current에 등록되지 않으므로 테스트 자체 인스턴스를 사용한다. 런타임 지연 삭제의 알려진 EditMode 엔진 진단만 정확한 메시지로 예상하고 다른 오류는 계속 실패한다. 저장 실패는 별도 특정 메시지를 검증한다. 단위 테스트는 실제 hit-area 크기·필터 판단·이벤트 전달·저장·초안·복귀를 검사한다.

**실제 렌더 프레임의 전역 Canvas 레이캐스트·네이티브 포인터·한/영 캡처는 미완료다.** Metal batch 대조에서도 EditMode 전역 hit가 비어 있던 5개 결과를 h-f-raycast-graphics에 보존했다. 그 실패를 네이티브 통과로 바꾸지 않았으며 실제 프레임 검사는 별도로 수행해야 한다. 임시 회사 식별자와 자체 Unity import 9파일은 해시와 함께 복구 가능 보관하고 HEAD 바이트로 복원했다. 실제 CLI 리뷰·원격 통합은 후속 근거가 필요하다.
