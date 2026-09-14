# HELLSCRIPT 유기적인 균열 지형과 주변 보스 소환

작성일: 2026-09-14 · 지도 버전: 6

[English](Organic_Rift_Expansion.en.md)

## 목표와 참고 범위

사각형 방과 직각 통로가 반복되는 균열을 비대칭 공간과 굽은 길이 이어지는 던전으로 바꾼다. 외곽을 한 바퀴 도는 탐색에서 벗어나도록 중앙 방과 X자 횡단로를 필수로 추가한다. 보스 전용 방을 없애고, 처치 게이지가 가득 차면 플레이어 주변에 보스를 소환한다.

블리자드의 [Diablo IV Quarterly Update—March 2022](https://news.blizzard.com/en-us/article/23788294/diablo-iv-quarterly-updatemarch-2022)는 제작한 타일 세트를 조합해 무작위 던전을 구성하고, 타일 세트 사이에 전환 구간을 넣는 방식을 설명한다. 이 공개 원리를 참고했으며, 디아블로 IV의 내부 생성 코드나 정확한 알고리즘을 확보하거나 복제한 것은 아니다. 첨부 지도는 지형과 연결 형태의 참고 자료로 사용했다. 이미지 안의 지명·보스·퀘스트 주석은 별도 게임 요구사항으로 옮기지 않았다.

## 필수 생성 규칙

| 영역 | 새 균열의 규칙 |
|---|---|
| 방의 윤곽 | 타원형 바탕에 완만한 굴곡을 더한다. 전투·상자·출입구 공간을 확보한 비대칭 윤곽을 저장한다. |
| 방의 구성 | 외곽 방 6~8개에 중앙 방 1개를 추가해 총 7~9개를 생성한다. 중앙 방에는 서로 다른 출입구를 두어 막다른 방이 되지 않게 한다. |
| 순환 보장 | 모든 방에 독립된 출입구를 2개 이상 연결한다. 통로 하나 또는 방 하나를 제외해도 나머지 구역이 연결되어야 하며, 모든 방을 한 번씩 거쳐 입구로 돌아오는 경로가 있어야 한다. 막다른 곁방과 외길로 연결된 두 고리는 금지한다. |
| 횡단 동선 | 서로 다른 외곽 방 4개를 잇는 두 횡단 동선을 만든다. 한 동선은 중앙 방을 거쳐 이어진다. 두 동선은 방 바깥의 X자 교차로에서 실제로 만나며, 네 방향 모두 통행할 수 있어야 한다. |
| 교차점의 형태 | 양쪽으로 각각 9m의 길이 확보되고 교차 각도가 60~120도인 지점을 확인한다. 끝점만 닿는 T자 연결이나 나란히 겹치는 길은 필수 교차로로 인정하지 않는다. 무작위 지도와 고정 대체 지도에 같은 검사를 적용한다. |
| 출입구 | 외곽 연결이 안쪽 출입구를 모두 차지하지 않도록 예약한다. 필요하면 장애물과 겹치지 않는 방향에 출입구를 보완한다. |
| 통로 | 방을 피하는 경로의 불필요한 꺾임을 줄이고 완만한 곡선을 만든다. 출입구는 4m 폭을 유지하며 안쪽은 최대 약 6.3m까지 서서히 넓어진다. |
| 보스 공간 | 전용 보스 방·전실·보스 문을 생성하지 않는다. 보스 템플릿 RM06·RM12는 기존 저장과 호환성 검사에만 남는다. |

중앙 방은 고리의 기하학적 중심에서 10m 이내에 놓이며, 일반 적과 정예가 배치되는 실제 전투 공간이다. 새 균열에는 막다른 곁방이 없고, 상자는 순환 동선의 방에 분산한다. 방 템플릿 DB의 크기는 배치용 외곽 범위이며 실제 바닥은 저장된 윤곽으로 결정한다.

외곽에서 연속된 네 방을 A·B·C·D로 선택하고 A–C, B–중앙 방–D를 연결한다. 기존 외곽 고리도 유지하므로 A→C→B→중앙 방→D로 탐색하고 나머지 외곽을 따라 입구로 돌아올 수 있다. [RiftCirculation.cs](../../Assets/HELLSCRIPT/Runtime/Core/RiftCirculation.cs)는 실제 방·통로 그래프에서 이 조건을 독립적으로 검사한다. 무작위 지도와 고정 대체 지도에 모두 적용한다. 플레이어의 선택이나 상자·전투 추적에 따른 재방문까지 금지하는 규칙은 아니며, 지형이 되돌아가기를 강제하지 않는다는 보장이다.

## 보스 소환과 진행 조건

새 균열은 일반 적 처치 1, 정예 처치 5로 게이지를 채운다. 게이지 100을 충족하면 보스 전투 상태로 한 번만 전환하고 플레이어 주변의 소환 위치를 찾는다. 이전 단계별 봉인·정수 운반자·제물 목표는 새 균열에 생성하지 않으며, 목표 완료가 게이지보다 먼저 보스를 소환하는 경로도 사용하지 않는다. 상자·성소·저주 상자 등 필드 콘텐츠는 유지한다.

소환 위치는 플레이어에게서 6~18m 떨어져 있고, 실제 이동 경로가 26m 이하인 곳이어야 한다. 보스의 1.2m 착지 여유, 연결된 바닥, 살아 있는 적과의 간격, 위험 장판과 공격 예고를 확인한다. 가까워 보여도 벽을 크게 돌아가야 하는 위치는 제외한다. 주변이 일시적으로 막혀 있으면 전투를 유지하면서 0.25초 간격으로 다시 찾는다. 공간이 잠시 부족하다는 이유로 균열을 실패 처리하지 않는다.

소환 후보의 순서는 지도 시드에서 따로 계산한다. 후보 검색이 전투나 보상 난수를 소비하지 않는다. 실제 소환 시에는 기존 적 생성·공격·보상 처리를 사용하며, 보스 식별자를 저장해 재접속 후 중복 소환을 막는다. 보스 표식과 자동 이동은 실제 보스 위치를 따라간다.

일반 적 110명, 정예 8명, 게이지 100, 제한시간과 보스 보상은 유지한다. 상자는 총 방 수에서 2를 뺀 5~7개로 분산되지만, 상자의 합계 골드·재료와 장비 보상 2개의 예산은 유지한다. 이번 작업은 밸런스 조정이 아니다.

## 이동과 화면이 공유하는 근거

[RiftGenerator.cs](../../Assets/HELLSCRIPT/Runtime/Core/RiftGenerator.cs)가 생성 순서와 배치를 관리한다. [RiftCrossRoutes.cs](../../Assets/HELLSCRIPT/Runtime/Core/RiftCrossRoutes.cs)는 중앙 방과 X자 횡단로를 만들고 검사한다. [RiftOrganicGeometry.cs](../../Assets/HELLSCRIPT/Runtime/Core/RiftOrganicGeometry.cs)는 방 윤곽과 통로의 점·폭을 만든다.

[RiftSurface.cs](../../Assets/HELLSCRIPT/Runtime/Core/RiftSurface.cs)의 다각형 바닥을 [RiftNavigation.cs](../../Assets/HELLSCRIPT/Runtime/Core/RiftNavigation.cs)의 보행·시야·투사체·착지 판정, [RiftFloorMesh.cs](../../Assets/HELLSCRIPT/Runtime/Presentation/RiftFloorMesh.cs)의 실제 바닥, [RiftMinimap.cs](../../Assets/HELLSCRIPT/Runtime/Presentation/RiftMinimap.cs)의 지도 표시가 함께 사용한다. 대각선 통로의 바깥 사각 영역을 바닥으로 취급하지 않는다. 길 찾기는 격자 지점 사이의 이동 가능 여부도 확인한다.

바닥 가장자리에는 낮은 암석 턱을 표시하며 연결부와 겹치는 내부 경계는 제외한다. 생성한 메시의 수명은 해당 게임 오브젝트가 소유한다. 미탐색 공간을 가리는 규칙은 유지한다. 주변 보스 위치의 선택은 [RiftBossPlacement.cs](../../Assets/HELLSCRIPT/Runtime/Core/RiftBossPlacement.cs), 소환과 복원은 [CombatSimulation.Rift.cs](../../Assets/HELLSCRIPT/Runtime/Core/CombatSimulation.Rift.cs)가 맡는다.

## 저장과 기존 플레이

새 지도는 버전 6과 `roamingBoss` 구분, 방의 `outline`과 `central`, 통로의 `widths`와 `crossing`을 저장한다. 재접속할 때 시드로 지형을 다시 만들지 않는다. 기존 버전 1~4는 원래 사각형 바닥 판정을, 버전 5는 저장된 곡선 바닥을 유지한다. 기존 저장의 보스 방·관문·목표 조건도 그대로 이어진다. 새 규칙은 새로 입장하는 일반 균열부터 적용한다.

명시적인 `forcedObjective` 또는 `arenaBoss` 개발용 호출은 버전 5의 보스 방·목표 구성을 만들 수 있다. 옛 저장 방식의 회귀 검사와 개발 도구를 위한 호환 경로이며, 일반 입장 화면에서 제공하는 선택 사항이 아니다.

## 검증

Unity 6000.6.0f1에서 최종 변경에 대한 Edit Mode 검사 21개가 모두 통과했다. 지형·순환 구조·주변 보스·고정 대체 지도·방 역할 검사 19개와, 24개 시드의 전투·보상 예산 검사 및 세 직업의 6개 빌드 이동 검사를 포함한다. 막다른 방, 외길로 연결된 두 고리, 한 방만 공유하는 두 고리, 모든 방을 한 번에 순환할 수 없는 구조를 각각 거부하는 회귀 검사를 추가했다. [한영 문구 검사 33개](OrganicRiftEvidence/localization-results.xml)도 모두 통과했다. 이전 오류 문구 1개는 키와 번역을 보존한 채 저장 이력용 구역으로 옮겼다.

- 시드 914001~914036에서 두 테마와 외곽 방 6·7·8개 구성을 검사했다. 모든 지도에 중앙 방과 통행 가능한 X자 교차로가 있었으며, 모든 방을 한 번씩 거쳐 입구로 돌아오는 경로가 있었다. 막다른 방과 보스 방은 없었다. 고정 대체 지도 사용은 0회, 방의 총면적 감소는 직사각형 범위 대비 16.45%였다.
- 통로 전체의 1.2m 이동 여유, 적·상자 접근과 저장·복원 후 지형 일치를 확인했다. 대각선 바닥 바깥의 보행·시야·투사체·착지 차단과 기존 버전 4 복원도 검사했다. 바닥 조각의 경계에서 짧은 이동만 막히던 정밀도 오류는 고정된 지형으로 재현해 검사한다.
- 방·출입구·교차로와 통로 표본에서 주변 소환 공간을 검사했다. 게이지 99에서 미소환, 마지막 처치로 100에 도달한 뒤 1회 소환, 위험 공간에서의 대기·재시도, 소환 전후 저장과 중복 방지를 확인했다.
- macOS 개발 빌드는 오류 0개로 성공했다. 별도 저장 폴더에서 실제 실행 종료와 재시작 후 지도·체력·시간·난수 상태를 확인했다. 1배속 자동 탐색·전투로 상자 5개를 개봉했고, 게이지 100에서 플레이어로부터 10.00m 떨어진 곳에 보스가 나타나 141.05초에 균열을 완료했다.
- 실행 조건은 1단계, 시드 91367, 레벨 30 전사, 아이템 레벨 30의 희귀 장비 8개, 상자 우회 거리 30m와 보스 이후 상자 정책이다. 기능 흐름을 검증한 결과이며 일반 계정의 난이도나 클리어율을 뜻하지 않는다.

[생성 지도 6개 비교](OrganicRiftEvidence/generated-maps.png), [실제 게임 화면](OrganicRiftEvidence/world.png), [게임 내 전체 지도](OrganicRiftEvidence/full-map.png), [주변 보스 소환 화면](OrganicRiftEvidence/roaming-boss.png), [균열 완료 화면](OrganicRiftEvidence/rift-result.png)을 보존한다. 전체 지도는 일시정지 중 검증용으로 탐색 영역을 잠시 공개한 뒤 원래 탐색 상태를 복원했다. 보스 소환 화면도 UI 갱신을 위해 잠시 일시정지한 화면이다.

근거: [Edit Mode 결과](OrganicRiftEvidence/editmode-results.xml), [최종 이동 검사](OrganicRiftEvidence/final-geometry-results.xml), [수정 후 이동·예산 검사](OrganicRiftEvidence/post-fix-flow-results.xml), [생성 요약](OrganicRiftEvidence/generation-summary.txt), [예시 지도 원본](OrganicRiftEvidence/sample-layouts.json), [짧은 이동 회귀 지형](../../Assets/HELLSCRIPT/Tests/Editor/Fixtures/organic-seam.json), [실행 결과](OrganicRiftEvidence/runtime-rift-smoke.txt), [소환 위치 기록](OrganicRiftEvidence/boss-spawn.json), [저장 지도](OrganicRiftEvidence/layout-seed-91367.json), [검증 조건과 소스 해시](OrganicRiftEvidence/validation.json), [지형 검사 코드](../../Assets/HELLSCRIPT/Tests/Editor/OrganicRiftTests.cs), [보스 소환 검사 코드](../../Assets/HELLSCRIPT/Tests/Editor/RoamingBossTests.cs).

## 적용 범위와 남은 확인

Unity 패키지, 씬, 프리팹과 기존 이미지 에셋은 변경하지 않는다. 현재 아트와 전투 시스템을 사용하는 지형·진행 규칙 개선이다. 모바일 실기기 성능, 전체 시드 공간과 장기 밸런스는 별도 검증 대상이다.

이 문서는 새 균열의 구조와 보스 진입 조건에 대해 [던전 구성 상세 기획](../Design/HELLSCRIPT_Dungeon_Composition_Detail.md), [자동 생성 균열 상세 기획](../Design/HELLSCRIPT_Rift_Exploration_Detail.md)의 이전 규칙을 대체한다. 옛 목표 방식의 근거와 당시 검증은 [목표 사슬 개발 기록](Objective_Chains_Expansion.md), [자동 생성 균열 개발 기록](Rift_Expansion.md)에 보존한다.
