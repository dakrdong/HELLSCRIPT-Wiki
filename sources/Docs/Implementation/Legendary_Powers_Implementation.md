# 직업별 전설 효과 구현과 검증

작성일: 2026-09-22.

## 구현 범위

[직업별 전설 장비 목록](../Design/HELLSCRIPT_Legendary_Class_Expansion.md)의 전사·궁수·마법사 40종씩을 기존 장비 생성기에 등록했다. 기존 직업 전설 12종과 공용 3종을 유지하고 신규 108종을 추가했다. 세트 장비 24종은 이 수량에 포함하지 않는다.

신규 원본은 `Assets/HELLSCRIPT/Resources/Data/LegendaryPowers.json`이다. `LegendaryPowers`가 정의와 문구를 읽고 검증하며, `ItemCatalog.BuildUniques`가 기존 장비 풀에 추가한다. 기존 `ItemGenerator`, `Economy.Equip`, `HeroStats.specials`, `CombatSimulation`이 생성·장착·전투의 소유자다. 별도 아이템 인벤토리나 전투 관리자를 만들지 않았다. 공개 DB도 같은 JSON을 읽는다.

효과 실행은 기존 원 공격의 피해 처리, 시전 완료, 피격, 물약 사용에 연결했다. 예약 피해와 전설별 횟수·대기시간·강화는 기존 `CombatEffectState` 아래에서 저장한다. 전투 효과 저장 버전은 6이며 이전 저장은 빈 신규 목록으로 보완한다. 새 버전을 모르는 이전 클라이언트는 기존 버전 검사에 따라 안전하게 읽기를 중단한다.

## 참고 자료와 밸런스 범위

2026-09-22에 내장 브라우저에서 확인한 [D4 Builds 위상 DB](https://d4builds.gg/database/legendary-aspects/)를 참고했다. 신규 항목에는 83개 참고 위상이 연결되어 있으며, [이름 대조 기록](LegendaryExpansionEvidence/reference-validation.json)에 페이지에서 확인한 결과를 보존했다. 이름·설명·수치와 스킬 대응은 HELLSCRIPT용으로 작성했다. 디아블로 IV의 현행 수치를 그대로 이식한 결과가 아니다. 신규 효과는 19가지 실행 유형을 사용하며 같은 직업 안에 실행 규칙 전체가 같은 중복 행은 없다.

아이템별 초기 상대 가중치는 20이다. 직업별 추가 효과와 기존 세트가 같은 부위 추첨에 참여하므로 개별 장비의 상대 출현 비율은 바뀐다. 다수 단계·시드·최적 빌드 간 밸런스와 모바일 실기기 성능은 별도 검증 범위다. 전용 아이콘·장비 외형은 만들지 않았으며 기존 베이스 아트를 공유한다.

## 검증 기록

최종 관련 회귀 검사 305개가 모두 통과했다. 신규 108개 데이터 행마다 실제 장착 상태에서 피해·자원·HP·보호막·제어·동작 속도·쿨타임 변화를 확인했다. 단위별 적중 검사는 실제 피해 함수에 직접 입력하며, 전체 자동 전투의 증거와 구분한다.

| Edit Mode 실행 범위 | 통과 | 실패 | 건너뜀 | 원본 결과 |
|---|---:|---:|---:|---|
| 최초 집중 검사 | 163 | 0 | 0 | [XML](LegendaryExpansionEvidence/legendary-class-focused-editmode.xml) |
| 전체 회귀 검사 | 2,940 | 4 | 0 | [XML](LegendaryExpansionEvidence/legendary-class-all-editmode.xml) |
| 수정 후 관련 회귀 검사 | 305 | 0 | 0 | [XML](LegendaryExpansionEvidence/legendary-class-regression-editmode.xml) |

전체 실행에서 발견한 네 항목 중 세 항목은 수정 후 재검사했다. 번역 검사는 도감이 실제로 사용하는 `UniqueItemDefinition.Name`을 확인하도록 바꿨다. 추첨 검사는 기존 39종의 가중치를 그대로 검증하면서 신규 108종과 전사 허리 부위의 가중치 합계 190을 반영했다. 12번의 균열 전투를 수행하는 통합 검사는 이전 기록 178.17초, 이번 전체 실행 185.02초로 기본 제한 180초에 근접하거나 이를 넘었다. 검증 조건은 유지하고 제한만 300초로 조정했으며, 재실행은 179.86초에 통과했다. 성능 개선 결과를 뜻하지 않는다.

남은 `EveryCustomGraphicBringsItsOwnCanvasRenderer` 실패는 기존 화면 컴포넌트 5개에 관한 것이다. 검사와 관련 소유 파일이 기준 커밋 `8852c6c`와 같은 바이트임을 [비교 기록](LegendaryExpansionEvidence/existing-graphic-failure.json)에 남겼다. 해당 화면 코드는 변경하지 않았다. 수정 후 전체 2,944개를 다시 실행하지는 않았으므로 전체 통과로 보고하지 않는다. [검사 결과 요약](LegendaryExpansionEvidence/validation-summary.json)에도 실행 범위와 남은 실패를 구분했다.

회귀 검사에는 효과의 재귀 발동 방지, 추가 피해의 처치 귀속, 시전 취소, 치명적 피격 뒤 회복 방지, 보스의 제어 누적과 광역 최대 5대상 조건을 포함한다. 지속 피해를 갱신해도 다음 피해 시각이 밀리지 않으며, 대기시간은 기존 전투 틱에 맞춰 판정한다. 전투 시간이 0·128·256초인 경우에도 같은 틱에서 다시 발동하는지 확인한다. LW28은 전투 함성이 지속되는 동안 분쇄 일격의 자원 소모를 줄인다.

[검증 대상 소스 해시](LegendaryExpansionEvidence/validated-source.json)에 변경한 게임 코드·데이터·검사 파일을 기록했다. 검증은 분리된 작업 사본에서 수행했으며, 사용자가 열어 둔 원본 Unity Editor의 전체 회귀 검사를 뜻하지 않는다.

### macOS 앱 실행

Unity 6000.6.0f1의 macOS 개발 빌드가 오류 0개로 성공했다. Metal 렌더러를 사용하는 앱에서 실제 `ItemGenerator`와 `Economy.Equip`으로 각 직업의 전설 무기 1종을 생성·장착했다. 앱 안에서 실제 `CombatSimulation` 자동 전투를 12초 진행하고 `GameStore`에 저장한 뒤 앱을 종료했다. 두 번째 앱 실행에서 저장을 읽고 6초 더 진행했으며, 동일 저장을 복원한 비교 전투와 효과 상태·총 피해·난수 상태가 일치했다.

| 직업 | 전설 무기 | 12초 시점 추가 피해 횟수 | 재시작 후 18초 시점 추가 피해 횟수 |
|---|---|---:|---:|
| 전사 | LW07 살점 가르는 바람 | 11 | 17 |
| 궁수 | LA05 뒤따르는 살촉 | 6 | 10 |
| 마법사 | LM05 오래 남는 잿불 | 10 | 15 |

[앱 실행 결과와 캡처 해시](LegendaryExpansionEvidence/native-summary.json)를 보존했다. 이 검사는 앱 안의 자동 전투 픽스처이며, 실제 필드에서 사용자가 클릭·터치한 플레이나 밸런스 인증은 아니다. 신규 108종 전체의 개별 효과는 위의 집중 검사에서 다뤘다.

세 직업의 상세 화면을 한국어 540×960, 영어 960×540으로 캡처했다. 같은 두 화면 조건에서 도감의 신규 108종 이름과 설명을 모두 조회하고 문구의 필요 높이가 실제 영역을 넘지 않는지 확인했다. 캡처 12장을 직접 검토했다. 모바일 실기기 입력과 성능은 확인하지 않았다. 앱 로그에는 URP의 GaussianDepthOfField·BokehDepthOfField·PaniniProjection 셰이더가 빌드에서 제외되었다는 경고가 있다. 본 전설 검사에서 예외는 발생하지 않았다.

| 직업 | 한국어 상세 | 영어 상세 | 한국어 도감 | 영어 도감 |
|---|---|---|---|---|
| 전사 | [보기](LegendaryExpansionEvidence/ko-portrait-LW07.png) | [보기](LegendaryExpansionEvidence/en-landscape-LW07.png) | [보기](LegendaryExpansionEvidence/ko-portrait-collection-LW07.png) | [보기](LegendaryExpansionEvidence/en-landscape-collection-LW07.png) |
| 궁수 | [보기](LegendaryExpansionEvidence/ko-portrait-LA05.png) | [보기](LegendaryExpansionEvidence/en-landscape-LA05.png) | [보기](LegendaryExpansionEvidence/ko-portrait-collection-LA05.png) | [보기](LegendaryExpansionEvidence/en-landscape-collection-LA05.png) |
| 마법사 | [보기](LegendaryExpansionEvidence/ko-portrait-LM05.png) | [보기](LegendaryExpansionEvidence/en-landscape-LM05.png) | [보기](LegendaryExpansionEvidence/ko-portrait-collection-LM05.png) | [보기](LegendaryExpansionEvidence/en-landscape-collection-LM05.png) |

### 위키 확인

위키 생성·일관성 검사, Python 검사 8개와 화면 검사를 통과했다. 내장 브라우저에서 로컬 공개본의 직업별 DB 필터가 각각 40종을 표시하고, 한국어·영어 문서에 각각 120개 항목이 있는지 확인했다. 상세 화면의 수치와 참고 자료, 날짜만 표시하는 등록·변경일, 읽기 전용 상태도 확인했다. [브라우저 확인 기록](LegendaryExpansionEvidence/browser-validation.json)은 로컬 생성본의 증거이며 공개 사이트 배포 결과가 아니다.

## 게시 상태

이 작업은 `codex/legendary-class-expansion`에서 진행한다. 원본 저장소의 작업 브랜치를 푸시한 뒤에도 공개 위키는 아직 바뀌지 않는다. 저장소 규칙에 따라 `main` 병합 담당자가 병합된 원본을 다시 검사하고 공개 위키 전체를 게시한다. 공개 주소는 [HELLSCRIPT 위키](https://dakrdong.github.io/HELLSCRIPT-Wiki/#/tree)다.
