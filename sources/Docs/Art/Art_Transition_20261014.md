# HELLSCRIPT 다크 판타지 제작 종료와 캐주얼 아트 전환 인계

기록일: 2026-10-14 한국 시간 · 아트 근거 확인: 2026-10-13 22:14 UTC · 최신 HEAD 확인: 22:20 UTC · [English](Art_Transition_20261014.en.md) · [자산 경로와 Library ID 색인](Art_Transition_20261014_Asset_Index.json)

**다크 판타지 컨셉의 신규 제작을 여기서 마감하고, 이후 신규 아트는 캐주얼 StyleD를 기준으로 이어간다.** 기존 그림과 게임 상태는 보존한다. 캐주얼 기본 납품은 **650종의 고유 마스터와 700PNG**이며 그림 검수, Library 저장, 부모 채팅의 기본 납품 첨부 접수까지 완료됐다.

현재 캐주얼 납품 그림은 게임에 적용되지 않았다. 사용자는 통합 담당의 수정·검증 병목과 아트 연결 구조 점검을 진행하고, 점검이 끝나면 **대표 화면부터 단계적으로 스킨을 교체하는 후속 작업을 승인했다.** 이 문서 작업의 범위는 인계와 색인 작성이다.

## 1. 다크 판타지 제작 마감과 보존 기준

마감은 제작 방향의 종료를 뜻한다. 기존 다크 판타지 자산의 모든 미결 작업이나 3D 리깅이 끝났다는 판정으로 확대하지 않는다. 새 그림이 필요한 작업에는 캐주얼 기준을 사용하며, 기존 런타임 그림의 교체는 후속 단계에서 적용 범위와 검증을 정해 진행한다.

다음 자료를 기존 위치와 Git 이력에 보존한다. 원본 이미지, `.meta`와 GUID, 생성 프롬프트·manifest, 과거 수정본, 현재 납품본과 검수 근거를 함께 유지한다.

| 보존 대상 | 기존 위치와 근거 |
| --- | --- |
| 현재 게임에서 사용하는 기존 아트 | [Assets/HELLSCRIPT/Resources/Art](../../Assets/HELLSCRIPT/Resources/Art). 캐릭터 선택·NPC·장비·상자·위상·HUD 등 기존 리소스와 `.meta` |
| 기존 제작 원화와 기록 | [Docs/Art](.)의 `HeroPortraits`, `NpcPortraits`, `ItemIcons`, `EquipmentVariants`, `ClassLegendaryIcons`, `ClassSetIcons`, `ClassAspectIcons`, `GeneratedGameArt`, `RewardBoxes`, `Monsters` 등. 실제 적용 범위는 [기존 그림 적용 기록](../Implementation/Generated_Game_Art.md)과 각 manifest 참조 |
| 캐주얼 정본 참조와 입력 원본 | [wt-casual-art](../../../wt-casual-art)의 `Docs/Art/CasualConcept/2026-10-13/`. 현재 참조 워크트리 HEAD는 `7196b0a9c93aa0009634c9be16cca351f75cdc6d`이며 이전 정리에서 보호된 작업 폴더 |
| main에 보존된 캐주얼 컨셉 자료 | [CasualConcept/2026-10-13](CasualConcept/2026-10-13). 정본 참조와 동일한 바이트의 이력 보존 자료. 원본 추가 65파일의 커밋 `7196b0a9`는 이력 보존 병합 `0d3b4d9e`를 거쳐 PR117에 포함됨 |
| 캐주얼 NPC 이전판 | [NPC v0 제작 폴더](/Users/t8g-2410-pn-005/Documents/Codex/2026-10-13/task-5/output/npc-casual-20261013). 현재 사용할 납품은 `revision-v02` |
| 3D 원본과 시험 결과 | [Hunyuan 영웅 원본](/Users/t8g-2410-pn-005/Documents/MyProject/HELLSCRIPT/AssetDeliverables/Hunyuan/2026-10-13/heroes), 컨셉 폴더의 `Characters/HunyuanInput`, `Characters/HunyuanTest`, `Town/HunyuanInput`. 완성 2D 납품 수에 합산하지 않음 |

이번 정리에서 기존 파일을 삭제·이동하거나 그림을 재생성·재업로드하지 않았다. 새 ZIP이나 이미지 사본도 만들지 않았다. 다크 판타지 생성 루틴·자동화의 운영 상태는 이 문서 작업에서 변경하지 않았으므로, 후속 담당은 신규 제작을 시작하기 전에 전환 기준과 충돌하는 기존 작업이 있는지 확인한다.

### 아트 전환 전 코드 기준점

| 항목 | 확정 기준과 실제 확인 |
| --- | --- |
| 저장소 | `/Users/t8g-2410-pn-005/Documents/MyProject/HELLSCRIPT/HELLSCRIPT` |
| 통합 기준 | PR117, `main` **`083b03f01d25aebfef13549562be0b170dc9c3b2`**. 최초 22:14 UTC 확인 시 실제 HEAD와 로컬 `origin/main`이 일치 |
| 최신 실제 HEAD | **`53d6a3e13304a781f9823134ba0f497bafb78621`**. 문서 추가 중 지침 담당이 `AgentRules/Workflow.md`·`.en.md` 2파일을 커밋해 main이 진행됨. 로컬 `origin/main`도 이 SHA이며 게임 입력 트리는 PR117 기준점과 동일. 아래 CI 근거는 `083b03f0`에 대한 결과 |
| CI | 해당 main SHA의 `generated`·`ownership` **2개 success**. [병합 후 CI 로컬 기록](/Users/t8g-2410-pn-005/Documents/Codex/2026-10-13/task/merged-main-ci.json), [검사 1](https://github.com/dakrdong/HELLSCRIPT/actions/runs/38458710661), [검사 2](https://github.com/dakrdong/HELLSCRIPT/actions/runs/38458710625) |
| 전체 EditMode | **5,715개 중 5,712 통과·실패 0·기존 제외 3**. 새 제외 0. XML 최상위 `Skipped:Ignored`와 개별 결과를 구분해 보존 |
| 화면과 네이티브 | 최신 화면 **24/24** 비교 통과. macOS D4 실제 클릭·schema22 저장 6회·새 프로세스 재시작, 가이드와 장비 추천 검증 완료 |
| 검증 입력 | 검증 소스 `05973ea3`와 현재 main의 `Assets`·`Packages`·`ProjectSettings` Git 트리가 모두 일치. [최종 입력·결과 기록](../Implementation/D4SkillTreeEvidence20261012/integration-validation.json), [전체 XML](../Implementation/D4SkillTreeEvidence20261012/integration-full-editmode-retirement-final.xml) |
| 이전 정리 | 워크트리 **4개**, 로컬 브랜치 **20개**, 원격 브랜치 **18개** 정리 완료. 이 숫자는 정리한 개수이며 현재 잔여 개수가 아님. [기존 정리 결과](/Users/t8g-2410-pn-005/Documents/Codex/2026-10-13/task/cleanup-result.json) |
| 현재 남은 작업 폴더 | 원본 main과 보호된 `wt-casual-art` 2곳. 로컬 브랜치 `main`, `claude/casual-art-concept`; 원격 추적 브랜치도 두 개이며 `origin/HEAD`는 별도 심볼릭 참조 |

코드 검증은 위 코드 기준점의 macOS 근거다. 새 캐주얼 그림 적용, 모바일 실기기, 웹 빌드, 보통 속도의 사용성·프레임 성능 검증으로 확대하지 않는다. 코드 검사·빌드·원격 재조회는 이번 문서 작업에서 새로 실행하지 않았다.

이후 지침이나 문서 커밋으로 HEAD가 바뀌면 실제 HEAD를 별도로 기록하고 PR117 기준점을 유지한다. 다른 담당이 수정 중인 `AGENTS.md`·`CLAUDE.md`와 공통 지침은 이 작업의 소유 범위에 포함하지 않는다.

## 2. 이후 신규 제작의 캐주얼 기준

참조가 충돌하면 **사용자의 최신 수정 결정 → 공통 가이드 v05와 ID별 최신 승인 SHA → 정본 이미지 → 이전 시안·조사 기록** 순으로 따른다. 이전 다크 판타지 원화는 인물·아이템의 정체성과 의미를 찾는 근거로 보존한다.

| 우선순위 | 참조 | 적용 목적 |
| --- | --- | --- |
| 1 | [STYLE_GUIDE.md](/Users/t8g-2410-pn-005/Documents/Codex/2026-10-13/task-3/casual-art-brief/STYLE_GUIDE.md), [TARGETS.md](/Users/t8g-2410-pn-005/Documents/Codex/2026-10-13/task-3/casual-art-brief/TARGETS.md), `group_*.json/csv` | 최신 대상·수량·방향·공유 관계·출력 규격 |
| 2 필수 | [남전사 정본 시트](/Users/t8g-2410-pn-005/Documents/MyProject/HELLSCRIPT/wt-casual-art/Docs/Art/CasualConcept/2026-10-13/Characters/sheet_1_warrior_m.png) | 3.5~4등신, 형태·굵은 외곽선·셀 명암의 공통 기준 |
| 2 필수 | [StyleD 타이틀 키 아트](/Users/t8g-2410-pn-005/Documents/MyProject/HELLSCRIPT/wt-casual-art/Docs/Art/CasualConcept/2026-10-13/title_key_art_v05_styleD.png) | 전체 색감·표정·영웅 6명의 정체성 |
| 3 | 각 영웅의 `Characters/sheet_1~6_*.png`, 각 대상의 원본과 ID 목록 | 피부·헤어·나이·역할·소품·무기·문양 보존 |
| 4 | [마을 게임 시점 정본](/Users/t8g-2410-pn-005/Documents/MyProject/HELLSCRIPT/wt-casual-art/Docs/Art/CasualConcept/2026-10-13/Town/town_overview_gameview_v01.png) | 밝기·색감·무광 소재·기존 시설. 인물 정체성 참조를 대체하지 않음 |
| 5 | `reference_manifest.json`의 `casual-gear/project/Main.dc.html` | UI 색감·배경에서의 가독성 참고. 정적 시안 |

새 그림을 생성할 때에는 필수 두 이미지와 해당 대상 원본을 실제로 열어 보고 생성 입력에 첨부한다. 프롬프트에 경로를 적거나 축소 모음만 붙이는 것으로 대신하지 않는다.

- **화풍:** 밝고 아기자기한 중세 판타지, StyleD 소년만화 치비, 3.5~4등신. 둥근 큰 덩어리에 머리·칼라·장갑 끝의 선명한 포인트를 준다.
- **선·명암·소재:** 굵고 깨끗한 외곽선, 평면 기본색과 경계가 뚜렷한 그림자 1단, 밝은 무광 철·가죽·천·나무. 사진 같은 미세 질감, 때·부식·과한 광택과 블룸을 피한다.
- **색·표정:** 하늘색·풀 초록·따뜻한 노랑·코랄·크림. 캐릭터 고유 색과 나이·역할을 유지한다. 작은 송곳니는 기존 영웅의 포인트이며 모든 NPC에 강제하지 않는다.
- **의미 보존:** 해골·악마 등 원래 존재와 아이템 이름·종류·상징은 유지하되 친근한 형태로 표현한다. 피·잔혹·공포·음침한 연출은 신규 제작 기준에서 제외한다.
- **출력:** 주인공·NPC 독립 그림은 진짜 알파의 세로 PNG. 현재 주인공·NPC 납품은 1024×1536, 장비는 512×512다. 아이템은 각 파일의 실제 생성 치수를 명세대로 유지한다. 이미지 안 글자·라벨·번호·로고 없이 잘림과 축소 가독성을 확인한다.

`archer` 원본은 현재 코드의 `HeroClass.Ranger`에 대응한다. 새 Archer 직업이나 save ID를 만들지 않는다. B28→B32 공유, 포션 8 ID→3 병 실루엣, 상자 148 catalog ID→85파일, `account.premium`→심연주화 공유 관계를 유지한다. 희귀도·능력치·레벨만으로 새 비주얼을 늘리지 않는다.

## 3. 카테고리별 완성 수량과 납품 상태

| 그룹 | 고유 마스터 | 기본 납품 PNG | 현재 완성 상태 |
| --- | ---: | ---: | --- |
| 주인공 | 6 | 6 | 전사·궁수·마법사 각 남녀. 단독 전신 검수·Library 저장·부모 채팅 PNG와 미리보기 접수 완료 |
| NPC v02 | 15 | 30 | 마을 12명과 이야기 3명. 화면 오른쪽 기본 방향 15장과 정확한 RGBA 좌우반전 15장. 검수·동일 Library 정체성 3개 v1 갱신·미리보기와 그림 ZIP 4개 접수 완료 |
| 장비·세트 문양 | 359 | 359 | 기본 59 + 전설 141 + 세트 부위 129 + 세트 문양 30. 검수·Library 저장·미리보기와 분할 ZIP 6개 접수 완료 |
| 아이템·재화 | 270 | 305 | 신규 마스터 270 + 동일 원본 별칭 35. 검수·분할 ZIP 23개와 ID 명세·모음 Library 저장. 기본 그림의 부모 첨부 접수 완료 |
| **합계** | **650** | **700** | 그림 수정 대기 0. 보조 자료·설계 목록·3D는 합계에서 제외 |

NPC의 표냐·구젤·안톤·사령관 4명은 사용자 피드백에 따라 얼굴·나이를 선별 수정했다. 나머지는 승인 외형을 유지하고 기본 방향을 통일했다. v0는 역사 자료이고 현재판은 [revision-v02](/Users/t8g-2410-pn-005/Documents/Codex/2026-10-14/task-3/output/npc-casual-20261013/revision-v02)다. 주인공 대화용 `hero-warrior`·`hero-ranger`·`hero-mage`를 NPC 15명에 다시 세지 않는다.

아이템의 별칭 35파일은 보석 상자 7가족의 같은 원본 픽셀을 보존한 복사 납품이다. 신규 생성 35종으로 세지 않는다. 아이템의 23개 ZIP은 각각 독립적으로 풀 수 있고, 기존 납품 검증에서 305PNG가 정확히 한 번씩 포함되며 원본 SHA가 같음을 확인했다.

장비의 **현재 우선 212종 / 추가 설계 147종** 분류는 아트 조사 당시 `638a003d`의 카탈로그 스냅샷이다. 두 묶음 모두 그림은 완성됐다. 당시 `playerEnabled=false`를 현재 D4 비활성 상태로 읽지 않는다. PR117에서 D4 출시 기본값을 켠 코드 기준점과 구분한다.

### 현재 파일과 납품 기록의 대조

- [DIRECTOR_STATUS.json](/Users/t8g-2410-pn-005/Documents/Codex/2026-10-13/task-3/casual-art-brief/DIRECTOR_STATUS.json)의 최신 ID별 승인과 [FINAL_PRIMARY_EXPORTS_QA.json](/Users/t8g-2410-pn-005/Documents/Codex/2026-10-13/task-3/casual-art-brief/FINAL_PRIMARY_EXPORTS_QA.json)을 기준으로, **현재 700PNG의 SHA를 재대조해 누락·불일치 0**을 확인했다. 정본 참조 12파일도 기존 기록과 일치한다. 이 대조는 기존 픽셀 검수를 이어받는 파일 무결성 확인이다.
- [FINAL_SUPERVISOR_HANDOFF.md](/Users/t8g-2410-pn-005/Documents/Codex/2026-10-13/task-3/casual-art-brief/FINAL_SUPERVISOR_HANDOFF.md)와 [MORNING_DELIVERY_INDEX.json](/Users/t8g-2410-pn-005/Documents/Codex/2026-10-13/task-3/casual-art-brief/MORNING_DELIVERY_INDEX.json)의 20:36 UTC 기록에는 아이템 납품 확인이 남아 있다. 이후 **21:29 UTC의 [아이템 delivery-index.json](/Users/t8g-2410-pn-005/Documents/Codex/2026-10-13/task-6/output/hellscript-casual-items/delivery-index.json)**은 검증·Library 저장 25파일 완료를 기록하며, 부모의 최신 인계가 네 그룹의 기본 첨부 **accepted**를 확정한다. 오래된 대기 문구를 현재 미완료로 재사용하지 않는다.
- Library 저장과 채팅 첨부 접수는 서로 다른 상태다. `accepted`는 첨부 접수가 확인됐다는 뜻이며 사용자의 다운로드·압축 해제 확인으로 확대하지 않는다. 색인의 Library ID 51개는 기존 저장 기록에서 읽었으며 해당 로컬 파일의 존재를 확인했다. 완료 그림을 다시 생성하거나 업로드할 필요는 없다.

## 4. 완료되지 않은 범위

| 범위 | 현재 상태와 다음 경계 |
| --- | --- |
| 캐주얼 UI | HTML과 이미지 시안. 정적 렌더의 색감 확인이며 실제 게임 UI 구현·동작 검증 완료가 아님 |
| 새 캐주얼 그림의 게임 적용 | **현재 미적용.** 연결 구조·수정/검증 병목 점검 후 대표 화면부터 단계적 스킨 적용이 승인됨. 적용 완료를 표시하려면 실제 리소스 연결과 게임 화면 검증이 필요 |
| 주인공 대화·HUD 파생 | 연결 metadata가 있으나 이번 완성 수는 단독 전신 6PNG. 추가 초상·HUD 파생 납품이나 런타임 연결은 별도 범위 |
| 영웅 3D와 사람형 공통 리그 | 원본 GLB·입력 그림·간이 리그 시험은 보존됨. 간이 QA 통과 뒤 포즈에서 조각 이탈이 확인된 사례도 있으며, 공통 사람형 골격·장비 교체·게임 설치 완료와 구분. [기존 선택 기록](/Users/t8g-2410-pn-005/Documents/MyProject/HELLSCRIPT/AssetDeliverables/Hunyuan/2026-10-13/heroes/selection_status_20261013.md) 참조 |
| 마을·환경·몬스터 | 마을 전체와 전투 컨셉·3D 입력 시안이 있음. 네 그룹의 650종 납품에 환경·몬스터 전량 캐주얼 교체를 포함하지 않음 |
| 보충 UI 19개 | 슬롯 11개와 포션 효과 표식 8개. 별도 보류 목록이며 700PNG에 합산하지 않음 |
| 추가 DES 위상 18개 | 현재 대응 원본이 없는 설계 목록. 미제작이며 런타임·완성 수에 추가하지 않음 |
| 출시·플랫폼 범위 | 아트 검수와 Library 납품은 출시·출처 승인이나 모바일 실기기 성능 검증을 포함하지 않음. 웹 빌드·공개 Wiki·사이트 배포도 이번 작업 범위 밖 |

취소·보류된 생성과 리깅은 시범이라는 이유로 다시 시작하지 않는다. 이후 승인된 스킨 교체는 기존 게임 구조와 ID·공유 관계를 유지하면서 진행한다.

## 5. 다음 작업자가 이어갈 체크리스트

선행 점검과 적용 담당은 통합 담당 **`01a134e3-83aa-7508-8fa5-aad0a6d140ac`**다. 문서 담당은 게임 파일을 바꾸지 않는다.

1. **현재 작업 상태 확인:** 최신 실제 HEAD·미커밋 변경·기존 작업 소유자를 확인한다. PR117 코드 기준점과 차이가 있으면 별도로 적는다. `AGENTS.md`·`CLAUDE.md`·공통 지침의 다른 담당 변경을 덮거나 커밋에 섞지 않는다.
2. **선행 점검 결과 확인:** 수정·검증 병목과 아트 리소스 연결 구조를 정리한다. 기존 스프라이트·아틀라스·리소스 경로·공통 UI 소유자·임포터와 `.meta` 보존 방법을 확인하고 대표 화면의 적용 범위를 정한다.
3. **정본과 현재판 선택:** 공통 가이드, 남전사·StyleD 타이틀, 해당 인물/아이템 원본을 연다. NPC는 v02와 방향 쌍을 선택한다. 색인의 기존 로컬 파일을 사용하고 이동된 파일은 Library ID로 회수한다.
4. **ID 연결 확인:** `group_*.json/csv`, NPC 매핑 CSV, 장비 CSV, 아이템 ID 명세로 실제 대상·공유·별칭을 대조한다. `archer`→Ranger, B28→B32와 35별칭을 유지한다. 장비 212/147의 과거 우선순위는 현재 카탈로그에 다시 대조한다.
5. **대표 화면부터 적용:** 사용자 승인에 따라 선행 점검을 마친 뒤 대표 화면에서 스킨을 교체한다. 승인 그림의 원본 바이트와 과거 리소스·메타를 보존하고, UI 동작·게임 데이터·저장 계약을 변경하지 않는 적용 범위를 먼저 고정한다.
6. **대표 적용 검증 후 확대:** 관련 가독성·알파·방향·잘림·한국어/영어·필요 화면 크기·저장 재진입을 실제 게임에서 확인한다. 대표가 통과한 유형만 확대하며, 코드·에셋 변경에 맞는 검사를 기존 지침대로 진행한다. 문서만 바꾼 이번 작업의 결과를 새 적용 검증으로 재사용하지 않는다.
7. **별도 범위 유지:** UI 19개, DES 18개, 대화/HUD 파생, 환경·몬스터 전량 교체, 영웅 리깅은 별도 범위로 관리한다. 새로운 완료 수에 포함하려면 실제 제작·검수·납품 근거를 추가한다.
8. **인계 갱신:** 적용한 실제 파일·대상 ID·소스 SHA·검증 결과·남은 범위를 이 기록과 색인에 갱신한다. 그림 납품 완료와 게임 적용 완료를 계속 구분한다. 원격 push·공개 Wiki 동기화는 이번 문서 인계에서 수행하지 않는다.

## 6. 바로 찾는 자산 경로

| 그룹 | 현재 PNG 위치 | ID별 목록과 납품 근거 |
| --- | --- | --- |
| 주인공 | [delivery](/Users/t8g-2410-pn-005/Documents/Codex/2026-10-13/task-4/output/hellscript-main-characters-6/delivery) | 같은 제작 폴더의 `manifests/DELIVERY_MANIFEST.json`, `manifests/LIBRARY_IDS.json` |
| NPC v02 | [final](/Users/t8g-2410-pn-005/Documents/Codex/2026-10-14/task-3/output/npc-casual-20261013/revision-v02/final) | 같은 v02 폴더의 `delivery/npc-id-file-mapping.csv`, `metadata/LIBRARY_IDS.json`, `metadata/SPLIT_LIBRARY_IDS.json` |
| 장비 | [icons](/Users/t8g-2410-pn-005/Documents/Codex/2026-10-13/task-7/output/hellscript-casual-equipment/icons) | 같은 제작 폴더의 `delivery/HELLSCRIPT-equipment-359.csv`, `delivery/DELIVERY_INDEX.json`, `delivery/LIBRARY_DELIVERY.json` |
| 아이템·재화 | [제작 폴더](/Users/t8g-2410-pn-005/Documents/Codex/2026-10-13/task-6/output/hellscript-casual-items)의 각 가족 폴더 | `HELLSCRIPT-casual-items-305-id-spec.json`, `delivery-index.json`. 23개 분할 ZIP은 제작 폴더에 존재 |
| 공통 검수 | [casual-art-brief](/Users/t8g-2410-pn-005/Documents/Codex/2026-10-13/task-3/casual-art-brief) | `FINAL_PRIMARY_EXPORTS_QA.json`의 700개 레코드에서 정확한 경로·SHA·치수·종류를 조회 |

작은 JSON 색인은 그룹 경로, 정본 참조, 기존 Library 51파일의 정확한 ID·로컬 경로·저장 기록 출처를 담는다. 개별 700PNG 경로와 SHA는 이미 존재하는 검수 목록을 참조한다. 아래 ID 표는 Library에 저장된 이 문서만 열어도 납품 자료를 찾을 수 있도록 제공한다.

## 7. 기존 Library 납품 자료 색인

그림·명세·미리보기의 기존 저장 ID다. 원본 전체 ZIP의 Library 저장과 채팅용 분할 ZIP의 접수를 구분한다. 같은 파일의 로컬 경로·저장 기록 출처·알려진 버전은 JSON 색인에 있다. NPC 전체 ZIP·CSV·모음은 같은 ID의 v1이 현재판이다.

| 그룹 | 기존 저장 파일 | library_file_id |
| --- | --- | --- |
| 주인공 | `hero-warrior-m-v01.png` | `libfile_8c9b99437c74819185031cafc84f8587` |
| 주인공 | `hero-warrior-f-v01.png` | `libfile_29fecb2222e881918c9e5b525265def3` |
| 주인공 | `hero-mage-m-v01.png` | `libfile_0e5aa4dcf8908191a65d0f937d2bef1a` |
| 주인공 | `hero-mage-f-v01.png` | `libfile_04841dc7611081918791f7e8c5161bec` |
| 주인공 | `hero-ranger-m-v01.png` | `libfile_81ac5a6f5fdc81919ee4f47eaea4603f` |
| 주인공 | `hero-ranger-f-v01.png` | `libfile_fc53b20491948191958de0390ba784ad` |
| 주인공 | `main-characters-6-contact-light.png` | `libfile_42886e093a308191b2cfb3a20254e1f7` |
| 주인공 | `main-characters-6-contact-dark.png` | `libfile_6700c33ae38c81919f5c369868890ea3` |
| 주인공 | `main-characters-6-contact-alpha.png` | `libfile_31ed662b367c8191956f62342104cc9d` |
| NPC v02 | `HELLSCRIPT-NPC-Casual-20261013.zip` | `libfile_b65dfc6b30e481919f6a665900f557dc` |
| NPC v02 | `npc-id-file-mapping.csv` | `libfile_432a558522088191a38ba1ad64b88066` |
| NPC v02 | `HELLSCRIPT-NPC-Casual-15-contact-sheet.png` | `libfile_098c88839018819184bb99ab74618b2f` |
| NPC 분할 | `HELLSCRIPT-NPC-Casual-v02-part-01-of-04.zip` | `libfile_6e62438749fc819194d8423af0a62cba` |
| NPC 분할 | `HELLSCRIPT-NPC-Casual-v02-part-02-of-04.zip` | `libfile_fae174472e2c8191b7bc71161c6d79b4` |
| NPC 분할 | `HELLSCRIPT-NPC-Casual-v02-part-03-of-04.zip` | `libfile_ad933c4f808881918e1eb62744fe80d4` |
| NPC 분할 | `HELLSCRIPT-NPC-Casual-v02-part-04-of-04.zip` | `libfile_65338bda5d1c8191b2aaa23591af9e21` |
| 장비 | `HELLSCRIPT-casual-equipment-359.zip` | `libfile_d031514037c88191a139deb0d4e46f19` |
| 장비 | `HELLSCRIPT-equipment-359.csv` | `libfile_b81e9a531f90819184330416ad0a2440` |
| 장비 | `HELLSCRIPT-equipment-359-contact-sheets.pdf` | `libfile_bb2628e138a081918228c331043999b0` |
| 장비 분할 | `HELLSCRIPT-equipment-base-part-01-of-01.zip` | `libfile_a39a750471048191b8f4430838d08d34` |
| 장비 분할 | `HELLSCRIPT-equipment-legendary-part-01-of-02.zip` | `libfile_30840c56b1088191be8022a5a176c1bf` |
| 장비 분할 | `HELLSCRIPT-equipment-legendary-part-02-of-02.zip` | `libfile_919fc3f78208819194c9fd4cf064e859` |
| 장비 분할 | `HELLSCRIPT-equipment-set_piece-part-01-of-02.zip` | `libfile_ddb78417eb9c81919804c880fa0d7558` |
| 장비 분할 | `HELLSCRIPT-equipment-set_piece-part-02-of-02.zip` | `libfile_18c6a0d32fa4819186bfee470698d464` |
| 장비 분할 | `HELLSCRIPT-equipment-set_emblem-part-01-of-01.zip` | `libfile_e8ac54c3605c8191a713bf6169a41dc4` |
| 장비 미리보기 | `HELLSCRIPT-equipment-core-preview.png` | `libfile_d4444c4aad7881918a2cc0acc2f7fa2c` |
| 아이템·재화 | `HELLSCRIPT-casual-items-305-part-001.zip` | `libfile_af47d1718930819181ff1a8888fefaa4` |
| 아이템·재화 | `HELLSCRIPT-casual-items-305-part-002.zip` | `libfile_32c0bf7b69dc8191be0501f3afb1c5ad` |
| 아이템·재화 | `HELLSCRIPT-casual-items-305-part-003.zip` | `libfile_b60a4efb82d08191a1c2284840032f76` |
| 아이템·재화 | `HELLSCRIPT-casual-items-305-part-004.zip` | `libfile_b7d29f7918548191a333ec0d5c14df00` |
| 아이템·재화 | `HELLSCRIPT-casual-items-305-part-005.zip` | `libfile_5deba0eb1300819193a78e5c5fc0dc92` |
| 아이템·재화 | `HELLSCRIPT-casual-items-305-part-006.zip` | `libfile_385ffca31b4c8191b327490d439866d6` |
| 아이템·재화 | `HELLSCRIPT-casual-items-305-part-007.zip` | `libfile_b057e71bd5ac8191b1ec6b57cf542529` |
| 아이템·재화 | `HELLSCRIPT-casual-items-305-part-008.zip` | `libfile_dd970f9e40fc8191832cadf2777bc001` |
| 아이템·재화 | `HELLSCRIPT-casual-items-305-part-009.zip` | `libfile_d7cbbb0d0db081919c208797a98c7f88` |
| 아이템·재화 | `HELLSCRIPT-casual-items-305-part-010.zip` | `libfile_fb7bb1aff7a88191835561547315d205` |
| 아이템·재화 | `HELLSCRIPT-casual-items-305-part-011.zip` | `libfile_8b66fb1b26c88191819c143ef766e2b1` |
| 아이템·재화 | `HELLSCRIPT-casual-items-305-part-012.zip` | `libfile_7ef262aa65e08191ac6ecbcb6fc3a3ed` |
| 아이템·재화 | `HELLSCRIPT-casual-items-305-part-013.zip` | `libfile_48f2082440748191933d9ad75f3dc461` |
| 아이템·재화 | `HELLSCRIPT-casual-items-305-part-014.zip` | `libfile_5cd101edeea08191869a4ecea0092bb2` |
| 아이템·재화 | `HELLSCRIPT-casual-items-305-part-015.zip` | `libfile_a9b428745c9c8191bc0dd767b71f8c8d` |
| 아이템·재화 | `HELLSCRIPT-casual-items-305-part-016.zip` | `libfile_0fdcbc2caf748191be06f92bd7657f4a` |
| 아이템·재화 | `HELLSCRIPT-casual-items-305-part-017.zip` | `libfile_73076ce65b2c819187f1e64287381e92` |
| 아이템·재화 | `HELLSCRIPT-casual-items-305-part-018.zip` | `libfile_060c360a61348191b20ed09baffb0160` |
| 아이템·재화 | `HELLSCRIPT-casual-items-305-part-019.zip` | `libfile_2ca361cc4d988191bbfbd83796a5c46a` |
| 아이템·재화 | `HELLSCRIPT-casual-items-305-part-020.zip` | `libfile_388e6bebbac481919944c15cd20e9623` |
| 아이템·재화 | `HELLSCRIPT-casual-items-305-part-021.zip` | `libfile_5d781c768eec81919a0e29f1a3437327` |
| 아이템·재화 | `HELLSCRIPT-casual-items-305-part-022.zip` | `libfile_1b962f01cb988191980fec5a827eb32d` |
| 아이템·재화 | `HELLSCRIPT-casual-items-305-part-023.zip` | `libfile_f8bcf989b8608191b03989bf57435cd0` |
| 아이템·재화 | `HELLSCRIPT-casual-items-305-id-spec.json` | `libfile_69c27cc282f081919024f4566deedcc4` |
| 아이템·재화 | `HELLSCRIPT-casual-items-305-collection.png` | `libfile_165e3b9a31d081918e83d4f18a405ff6` |
