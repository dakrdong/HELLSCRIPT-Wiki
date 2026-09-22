# 완료 개발분 통합 검증

2026-09-22 기준으로 완료된 개발분을 별도 작업 폴더에서 합쳤습니다. 원본 작업 폴더의 미커밋 변경은 보존했으며, 공통 UI 전환은 이 통합본을 기준으로 진행합니다.

## 포함·제외 범위

| 범위 | 기준 |
| --- | --- |
| 기존 main | `96e8094`의 타이틀·캐릭터 선택·마을·HUD·룬·사냥 칙령·세트 기획을 유지합니다. |
| 상점·갬블·창고 | `665448b`까지 포함합니다. |
| 인벤토리·옵션 범위 표시 | `68e7373`까지 포함합니다. |
| 대장간 HTML·Unity | `0389d6d`, `1b56ef0`까지 포함합니다. |
| 직업별 전설 장비 120종 | 완료 커밋 `5b083ab`까지 포함합니다. 공용 전설 3종과 세트 24종도 유지합니다. |
| 제외 | 진행 중인 `codex/class-skill-design`의 신규 스킬 기획·위키·생성물은 포함하지 않습니다. |
| 개발 도구 | 원본 폴더의 Unity CLI 스킬을 보존합니다. 이는 제외 대상인 게임 스킬 기획과 별개입니다. |

장비 기반 30종, 속성 58종, 접사 54종, 전설 123종과 세트 장비 24종이 함께 남아 있습니다. 기존 장착 위치·범위 표시·강화석·슬롯 성장·거래 저장 경로를 함께 유지했습니다.

## 호환 수정

- 인벤토리의 옵션 범위 높이를 확인할 때 Unity 객체의 유효성을 확인하도록 수정했습니다.
- 전설 장비 검사에서 주무기 없이 보조 장비만 무작위로 선택하던 초기 설정을 수정했습니다. 실제 게임의 장착 조건은 유지합니다.
- 전설 효과의 체력 80% 경계를 저장된 실수 정밀도로 비교하도록 수정했습니다.
- 위키 생성 시 로컬 앱 빌드 폴더를 다운로드 첨부 자료로 처리하지 않도록 수정했습니다.

## 검증 범위

| 검사 | 결과 |
| --- | --- |
| Unity Edit Mode 전체 | 3,062개 중 3,030개 통과, 32개 실패, 건너뜀 0개입니다. |
| 위 실패를 수정한 후 관련 검사 | 128개 모두 통과했습니다. 초기 실패 32개를 모두 포함합니다. 두 보고서의 검사 수는 합산하지 않습니다. |
| macOS 개발 빌드 | Unity 6000.6.0f1에서 성공했습니다. |
| 인벤토리 | 장착·양손 무기·반지·분해·설정·저장 흐름을 실제 앱에서 확인했습니다. |
| 옵션 범위 | 인벤토리·창고·상점·획득 상세의 공통 설정과 Ctrl 임시 표시, 저장값·스크롤 유지 조건을 확인했습니다. |
| 대장간 | 강화 묶음 비용, 옵션 고정과 자동 변경, 슬롯 작업·즉시 완료·NPC 접근, 저장 후 재실행을 확인했습니다. |
| 전설 장비 | 세 직업의 생성·장착·실제 추가 피해와 별도 프로세스 재시작 후 상태·난수·피해의 일치를 확인했습니다. |
| 화면 | 한국어·영어, 세로·가로와 대장간의 16:9·16:10·21:9 화면을 확인했습니다. |

이 기록은 macOS 앱의 자동 입력과 상태 확인 결과입니다. 모바일 실기기 터치나 성능 검증 결과는 아닙니다. 이 단계는 완료 개발분 통합이며, 새 공통 UI가 모든 화면에 적용됐다는 뜻은 아닙니다.

## 근거

- [검증 요약](IntegrationEvidence/summary.json)
- [초기 전체 검사](IntegrationEvidence/baseline-editmode.xml), [수정 후 관련 검사](IntegrationEvidence/compatibility-editmode.xml)
- [인벤토리](IntegrationEvidence/inventory-result.txt), [옵션 범위](IntegrationEvidence/ranges-baseline-result.txt), [대장간](IntegrationEvidence/blacksmith-result.txt)
- [전설 효과 최초 실행](IntegrationEvidence/legendary-initial-result.txt), [별도 프로세스 재시작](IntegrationEvidence/legendary-resume-result.txt)

![대장간 세로 화면](IntegrationEvidence/forge-portrait-ko.png)
![대장간 가로 성장 화면](IntegrationEvidence/forge-landscape-en.png)
![인벤토리 세로 화면](IntegrationEvidence/inventory-portrait-ko.png)
![전설 장비 영어 상세](IntegrationEvidence/legendary-landscape-en.png)
