# HELLSCRIPT 프로젝트 개요

정리 기준일: 2026-09-09 · 기존 기획과 현재 프로젝트 자료를 연결한 안내 문서입니다.

## 게임이 지향하는 경험

HELLSCRIPT는 어두운 판타지 세계에서 장비와 행동 규칙을 조합하는 3D 쿼터뷰 자동 전투 RPG입니다. 플레이어는 직접 공격하거나 이동하는 대신, 영웅이 어떤 적을 먼저 공격하고 언제 회피하며 무엇을 회수할지 설정합니다. 균열에서 결과를 관찰하고 장비와 규칙을 수정하는 과정이 핵심입니다.

**영웅·장비 준비 → 행동 설정 → 균열 탐험과 자동 전투 → 보스 또는 실패 결과 → 장비 비교·규칙 수정 → 재도전** 순서로 플레이합니다.

## 기획의 기본 범위

| 항목 | 기준 |
| --- | --- |
| 화면과 플랫폼 | 모바일 가로·세로를 함께 설계하고 설정에서 방향을 선택합니다. PC는 모바일 가로 구성을 창 크기에 맞게 확장합니다. Android 우선 검증과 교차 저장 목표를 유지하며, 새 화면 대응 기획과 실제 구현·검증 상태는 구분합니다. |
| 영웅 | 전사·궁수·마법사 3직업이며 직업마다 성장·장비·가방을 따로 관리합니다. |
| 스킬 구성 | 직업별 액티브 6개 중 4개, 패시브 6개 중 3개를 장착합니다. 기본 공격은 별도로 유지합니다. |
| 성장 | 최대 레벨은 30이며 무료 재설정을 제공합니다. |
| 능력치 | 디아블로4 캐릭터 시트와 같은 다섯 묶음의 57개 속성입니다. 힘·민첩성·지능·의지력 네 가지가 방어도·회피·저항·받는 치유를 만들고, 나머지는 장비가 올립니다. 군중 제어 지속시간 감소와 기력 두 줄은 정의만 있고 적용되는 곳이 없습니다. |
| 자동 행동 | 22개 조건, 목표 우선순위, 접근·선회·거리 유지, 회피와 획득 정책을 설정합니다. |
| 균열 | 탐색과 보스전을 합쳐 300전투 초입니다. 처치 게이지를 채워 보스를 등장시키며 실패 전 획득물은 유지합니다. |
| 배속 | 현재 1배속만 제공하고 1.5배속·2배속을 잠그는 정책이며, 현재 빌드에 잠금 표시와 저장값 정리가 구현되어 있습니다. 향후 1.5배속은 월 구독, 2배속은 이벤트의 누적 시간 사용권으로 해금할 계획이고 구독·쿠폰 기능은 아직 없습니다. |
| 장비 | 무기·머리·몸통·손·발·허리·목걸이·반지의 8부위입니다. 베이스·접사·전설·세트가 빌드와 연결됩니다. |
| 세계 | 묘지와 성채의 2테마, 방 템플릿 12종, 일반 적 12종, 정예 특성 6종, 보스 3종입니다. |

수치와 효과는 개발 시험값을 포함합니다. 콘텐츠가 등록되어 있다는 사실만으로 밸런스나 출시 품질이 검증된 것은 아닙니다.

## 기획 문서 읽는 순서

장비 세트 확장 후보는 [직업별 세트 24종 기획·레퍼런스](../../Docs/Design/HELLSCRIPT_Class_Set_Reference.md)에서 확인합니다. 직업마다 8종을 장비 3·4·5·6개 구성으로 나누었으며, 원작 출처·개별 장비 105개·효과 60단계와 [JSON 데이터](../../Docs/Design/ClassSetReference/catalog.json)를 제공합니다. 기존 실행용 세트 DB와 구분한 게임 미반영 검토안입니다.

English: see [24 class set concepts and references](../../Docs/Design/HELLSCRIPT_Class_Set_Reference.en.md) for eight candidates per class across 3/4/5/6-piece compositions, 105 named pieces, 60 bonus tiers, source links, and the [JSON catalog](../../Docs/Design/ClassSetReference/catalog.json). These design drafts are separate from the existing runtime set DB.

[절전 방치 모드](../../Docs/Design/HELLSCRIPT_Idle_Mode_Detail.md)는 실제 사냥의 연속성과 배터리 절감을, [배속 잠금·사용권 정책](../../Docs/Design/HELLSCRIPT_Speed_Access_Detail.md)은 현재 잠금과 향후 구독·이벤트 해금 방식을 정리합니다. 2배속 1h 쿠폰은 60분을 추가하며 남은 시간에 합산합니다.

새 UI 작업에는 [화면 비율·방향 대응 상세안](../../Docs/Design/HELLSCRIPT_Screen_Layout_Detail.md)을 공통으로 적용합니다. 모바일 가로·세로 배치, 설정의 방향 선택, PC 리사이징과 전환 중 상태 보존을 정리한 기획이며 개발은 별도 작업에서 진행합니다.

화면 대응 구현은 [화면 배치 개발 기록](../../Docs/Implementation/Screen_Layout_Expansion.md)의 프리셋 대화창에서 시작해 [화면 설정·공통 안내](../../Docs/Implementation/Screen_Settings_Expansion.md), [전투 HUD·카메라 배치](../../Docs/Implementation/Battle_Layout_Expansion.md), [장비 목록·비교·일괄 정리](../../Docs/Implementation/Inventory_Management_Expansion.md) 순서로 이어졌습니다. [배속 접근 제한 개발 기록](../../Docs/Implementation/Speed_Access_Expansion.md)은 그 위에 현재 1배속만 제공하는 잠금을 적용했습니다. 나머지 화면의 재배치와 모바일 실기기 검증은 남아 있습니다.

사냥 자동화 설정은 **사냥 칙령(Hunt Edict)**으로 부릅니다. [저장·공유·프리셋 상세안](../../Docs/Design/HELLSCRIPT_Hunt_Edict_Detail.md), [전역 8개 분류](../../Docs/Design/HELLSCRIPT_Hunt_Edict_Global_Options.md), [스킬 옵션·화면 요약](../../Docs/Design/HELLSCRIPT_Hunt_Edict_Skill_Options.md), [스킬별 핵심 옵션 v0.2](../../Docs/Design/HELLSCRIPT_Hunt_Edict_Skill_Option_Catalog.md)에 새 요구를 정리했습니다. 최신 스킬 설정은 자동 사용을 포함해 각각 4~5개이며 추가 규칙·고급 조건 편집은 제거했습니다. 프리셋 5칸과 스킬 교체·설정 보존을 포함한 새 설계이며 기존 빌드의 구현 완료를 뜻하지 않습니다.

1. [상위 구현 기획서](../../Docs/Design/HELLSCRIPT_Unity_Implementation_Plan.md)에서 제품 방향과 확정 정책을 확인합니다.
2. [후속 개발 상세 기획](../../Docs/Design/HELLSCRIPT_Development_Expansion_Plan.md)에서 확장 범위와 의존성을 확인합니다.
3. [장비·빌드 상세안](../../Docs/Design/HELLSCRIPT_Itemization_Detail.md), [능력치 체계 상세안](../../Docs/Design/HELLSCRIPT_Attribute_System_Detail.md), [균열·탐험 상세안](../../Docs/Design/HELLSCRIPT_Rift_Exploration_Detail.md), 전투 상세 문서에서 담당 규칙을 읽습니다.
4. [현재 개발 현황](current-status.md)과 [남은 개발 작업표](../../Docs/Design/HELLSCRIPT_Remaining_Development_Backlog.md)에서 실제 구현과 후속 작업을 구분합니다.
5. [리소스 제작·관리 기준](resource-guide.md)에서 사용 중인 파일과 앞으로 필요한 제작물을 확인합니다.

## 데이터베이스에서 확인할 내용

| 분류 | 확인할 내용 |
| --- | --- |
| 영웅·스킬·패시브 | 직업, 해금, 기본 수치, 효과, 아이콘과 근거 문서를 확인합니다. |
| 행동 조건·추천 빌드 | 입력 범위, 주요 규칙, 장착 스킬, 이동 목표를 확인합니다. |
| 장비 베이스·접사 | 부위, 직업 제한, 기본 능력치, 접두·접미, 가중치와 허용 부위를 확인합니다. |
| 능력치 | 속성 번호, 분류, 단위와 상한, 그 줄을 뽑는 접사를 확인합니다. |
| 전설·세트·세트 장비 | 요구 스킬, 효과, 같은 부위의 경쟁, 2/4세트 구성을 확인합니다. |
| 일반 적·정예·보스·방 | 패턴과 대응 의도, 현재 등록값, 제작 표현을 확인합니다. |
| 리소스 | 이미지 원본, 아틀라스 영역, 코드로 만드는 표현과 미제작 항목을 확인합니다. |
| 검증 기록 | 보존된 Edit Mode 보고서의 단계, 통과·실패 수와 검사 시각을 확인합니다. 검사 수를 합산하지 않습니다. |

기획 DB는 개발 자료의 조회본입니다. 실제 보유 장비나 계정 저장 데이터를 포함하지 않습니다.
