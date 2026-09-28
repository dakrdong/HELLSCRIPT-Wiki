# 사냥 칙령 요약 화면 구현

작성일: 2026-09-28 · [English](Hunt_Edict_Overview.en.md)

[사냥 칙령 UX 재설계](../Design/Hunt_Edict_UX_Redesign.md)의 요약 탭·전투 방식·지난 사냥 조언을 실제 게임 창에 구현한 기록입니다. 설계 배경과 규칙은 설계 문서를 따르고, 이 문서에는 소유 코드, 동작, 성능 개선, 검증 결과를 적습니다.

## 소유 구조

| 영역 | 소유 코드 | 책임 |
|---|---|---|
| 탭 목록 | `HuntEdictUiCatalog.Tabs`, `AutomationTabs` | 요약 탭을 첫 번째로 추가하고, 자동 관리로 묶을 5개 탭을 정합니다. |
| 전투 방식 | `Resources/HuntEdictQuickPresets.json`의 `styles`, `HuntEdictQuickPresets.ApplyStyle`·`MatchStyle`·`StyleScopes` | 기존 간편 프리셋의 조합으로 방식을 정의하고 적용·판정합니다. 새 옵션 값이나 저장 필드를 만들지 않습니다. |
| 지난 사냥 조언 | `HuntEdictCoach`, `EdictCoachTip` | 해당 영웅의 최근 `RunRecord`를 읽어 조언을 만듭니다. 전투를 다시 계산하거나 영웅을 바꾸지 않습니다. |
| 화면 | `HuntEdictWindow.Overview.cs` | 기존 창의 스크롤·버튼·테마·스킬 아이콘과 간편 프리셋 선택 창을 재사용합니다. 편집본은 `HuntEdictEditSession`, 저장은 기존 `GameStore.CommitHuntEdict`가 담당합니다. |
| 진입 경로 | `GameUI.RiftVictory.cs`, `GameUI.Tutorials.cs`, `GameUI.ShowDefeatAnalysis`, `HuntEdictWindow.SelectSkillPage`·`OpenSkillPolicy` | 결과 화면과 예전 기록의 사망 원인 분석은 요약 탭을 열고, 튜토리얼은 안내 대상 탭을 엽니다. 스킬 하위 화면을 여는 호출은 스킬 탭으로 전환합니다. |

새 창이나 캔버스, 글꼴, 저장 형식은 만들지 않았습니다. 요약 탭은 기존 `HuntEdictWindow`의 탭 하나이며, 공통 UI 계약서의 사냥 칙령 항목에 규칙을 추가했습니다.

## 동작

- 창을 열면 요약 탭이 먼저 나옵니다. 세로 화면의 탭 10개는 5열×2행, 가로 화면은 왼쪽 1열입니다.
- 전투 방식 카드를 누르면 7개 묶음의 간편 프리셋을 차례로 적용한 편집본이 만들어집니다. 저장 전에는 영웅과 계정이 바뀌지 않습니다. 되돌리기는 마지막 저장본으로 돌아갑니다.
- 현재 값이 세 방식 중 어느 것과도 다르면 그 사실을 알립니다. 회피 3종과 체력 위기 후퇴가 모두 꺼져 있으면 경고 문구를 표시합니다. 새 영웅은 이 상태로 시작합니다.
- 조언은 `review.heroId`가 해당 영웅인 가장 최근의 기록을 사용합니다. 사망·제한 시간 초과·스킬 대기 사유·여유 있는 정복을 판정하며, 저장된 칙령이 이미 따르는 조언은 제외합니다. 기준 수치는 `HuntEdictCoach`의 상수입니다.
- 조언을 적용하면 카드가 "적용함, 저장하면 반영됩니다" 상태로 바뀝니다. 저장하면 조언 목록에서 빠집니다.
- 판단 항목을 누르면 기존 간편 프리셋 선택 창이 열립니다. 그 창에서 직접 설정을 고르면 해당 탭의 묶음 상세로 이동하며, 값은 바뀌지 않습니다.

## 편집 성능 개선

첫 스모크 실행에서 요약 화면을 한 번 다시 그리는 데 평균 454ms가 걸렸습니다(macOS 개발 빌드, 1600×900). 원인은 요약 화면이 아니라 공통 정규화 경로에 있었습니다.

- `ClassSkillLoadout.ProjectEdict`와 `ClassSkillOptions.ProjectLegacy`는 옵션 하나를 쓸 때마다 `HuntEdictV2.WithOption`을 불러, 칙령 문서 전체를 매번 다시 정규화했습니다.
- `EdictOptions.Canonical`은 전역 옵션 151개를 서로 찾는 O(n²) 탐색을 했습니다.
- 간편 프리셋의 일치 판정은 프리셋을 실제로 적용한 복사본을 만든 뒤 비교했습니다.

다음과 같이 고쳤습니다. 결과 값은 바뀌지 않습니다.

- `HuntEdictV2.SetOption`을 추가했습니다. 두 투영 함수는 문서를 한 번만 정규화한 뒤 이 함수로 옵션을 씁니다. `WithOption`도 같은 함수를 사용합니다.
- `EdictOptions.Canonical`은 사전으로 값을 찾습니다. 누락·중복·알 수 없는 ID를 거부하는 조건은 같습니다.
- 전역 묶음의 일치 판정은 프리셋이 쓸 값을 적용 코드와 같은 함수(`GlobalValue`)로 계산해 현재 값과 비교합니다. 모든 호출자가 정규화된 편집본을 넘기므로 결과가 같으며, 이 동일성을 `GlobalMatchingAgreesWithApplyingTheRecipe`가 세 직업의 모든 전역 프리셋 조합에서 확인합니다.

| 측정(편집기 20회 평균, `MatchStyle`은 5회 평균) | 이전 | 이후 |
|---|---:|---:|
| 편집본 정규화 `HuntEdictLoadout.Canonical` | 12.8ms | 2.0ms |
| 전역 간편 프리셋 적용 | 26.6ms | 4.1ms |
| 스킬 간편 프리셋 적용 | 31.5ms | 5.4ms |
| 전투 방식 판정 `MatchStyle` | 188.6ms | 0.28ms |
| 요약 화면 다시 그리기(개발 빌드, 5회 평균) | 454.5ms | 40.1ms |

요약 화면 다시 그리기 시간은 다른 Unity 작업이 없을 때 측정한 값입니다. 최신 `main`을 병합한 최종 빌드에서도 같은 조건으로 42.7ms였습니다. 이 개선은 요약 화면뿐 아니라 기존 탭의 묶음 목록, 스킬 사용 설정, 저장 전 검증에도 적용됩니다. 모바일 기기에서의 시간은 측정하지 않았습니다.

## 기존 스모크 수정

- `RuntimeSkillTreeSmoke`: 창을 연 직후 스킬 트리를 검사하므로, 창을 연 뒤 스킬 탭을 선택하도록 고쳤습니다.
- `RuntimeRiftVictorySmoke`: 결과 화면의 **사냥 칙령 편집**이 요약 탭을 여는지 확인하도록 기대값을 바꿨습니다.
- `RuntimeEdictQuickPresetSmoke`: 새 계정이 필수 튜토리얼에 막히지 않도록 `mapComplete` 설정을 추가했습니다. 이 스모크가 처음으로 끝까지 진행되면서, 창고 공간 부족 묶음이 이전 작업(`bc71ab3f`)에서 간편 프리셋 선택기 대신 전용 조작으로 바뀐 사실이 드러났습니다. 이 묶음을 반복 검사 대상에서 제외했습니다. 해당 묶음의 프리셋 값은 편집 모드 검사가 계속 확인합니다.

## 검증

검증은 최신 `main`(`672e4a0c`)에서 만든 작업 폴더를 APFS로 복제한 Unity 프로젝트에서 수행했습니다. 작업 중에 `main`이 두 번 바뀌어(`40e601c4`, `b418de72`) 매번 병합한 뒤 다시 검증했습니다. 사용자가 열어 둔 Unity 편집기와 실제 계정 저장은 사용하지 않았습니다.

### 편집 모드 검사

- 새 검사 `HuntEdictOverviewTests` 17개가 통과했습니다. 전투 방식의 적용·판정·공유 코드 왕복·다른 옵션 보존, 설명문에 적힌 수치, 전역 판정과 실제 적용의 동일성, 조언 규칙(기록 없음·사망·시간 초과·사거리·자원·여유 있는 정복·저장 후 제외)을 확인합니다.
- 관련 검사 23개 클래스, 871개가 최신 병합 상태(`4c85f5e7`)에서 모두 통과했습니다. 칙령·간편 프리셋·번역·튜토리얼·첫 플레이·공통 UI·스킬 트리·스킬 사용 방식·전투 기록과 새로 병합된 적 공격·경고 게이지 검사를 포함하며, 투영 함수를 바꾼 영향은 스킬 런타임·사용 방식 검사가 확인합니다. [결과 XML](HuntEdictOverviewEvidence/editmode-related.xml)
- 전체 검사는 병합 전 4,567개 중 4,520개, `40e601c4`를 병합한 뒤 4,594개 중 4,547개가 통과했습니다. 두 번 모두 실패한 47개는 이 작업 전부터 실패하던 검사와 정확히 같으며, 새로 실패한 검사는 없습니다. 적 공격만 바꾼 `b418de72` 병합 뒤에는 전체 검사를 다시 돌리지 않고 관련 검사로 확인했습니다. [전체 검사 요약](HuntEdictOverviewEvidence/editmode-full.txt)

### macOS 개발 빌드와 런타임 스모크

최신 병합 상태의 macOS 개발 빌드가 오류 없이 성공했습니다. 다른 Unity 작업 없이 실제 uGUI 레이캐스트와 포인터 누름·놓음·클릭으로 다음 스모크를 차례로 실행했습니다. [실행 요약](HuntEdictOverviewEvidence/smokes.txt)

| 스모크 | 결과 |
|---|---|
| `RuntimeEdictOverviewSmoke`(신규) | 통과. 새 영웅 경고, 전투 방식 적용·되돌리기·저장, 판단 프리셋, 직접 설정 이동, 스킬·트리·자동 관리 이동, 시간 초과 기록의 조언 3개 표시·적용·저장 후 제외를 확인했습니다. 5개 화면 크기×한국어·영어×글자 100%·150%의 20개 조합에서 탭 10개와 저장·되돌리기가 창 안에 있고 포인터가 닿으며, 글자가 잘리지 않는지 확인했습니다. 별도 프로세스에서 저장한 방식과 조언이 복원되는 것도 확인했습니다. [조작 결과](HuntEdictOverviewEvidence/runtime.txt) · [재실행 결과](HuntEdictOverviewEvidence/restart.txt) |
| `RuntimeSkillTreeSmoke` | 통과(첫 실행과 재실행). |
| `RuntimeEdictQuickPresetSmoke` | 통과(첫 실행과 재실행). |
| `RuntimeTutorialSmoke` | 통과(1단계와 재개). 튜토리얼의 칙령 저장(F05)과 새 스킬 장착(F06) 경로를 포함합니다. 앞선 병합 상태에서 전체 검사와 동시에 실행한 한 번은 재개 단계가 첫 균열 도중 결과 기록 없이 종료되었습니다. 예외나 실패 기록은 남지 않았으며, 단독 재실행과 최신 병합 상태의 실행에서는 모두 통과했습니다. |
| `RuntimeRiftVictorySmoke` | 통과. 결과 화면의 칙령 편집이 요약 탭을 엽니다. |

### 화면

- [새 영웅: 안전장치 경고와 세 방식](HuntEdictOverviewEvidence/overview-new-hero-1600x900-ko.png) · [균형 방식 선택](HuntEdictOverviewEvidence/overview-balanced-1600x900-ko.png) · [시간 초과 기록의 조언과 적용 상태](HuntEdictOverviewEvidence/overview-advice-1600x900-ko.png)
- 세로 440×956: [한국어 100%](HuntEdictOverviewEvidence/overview-440x956-ko-100.png) · [한국어 150%](HuntEdictOverviewEvidence/overview-440x956-ko-150.png) · [영어 150%](HuntEdictOverviewEvidence/overview-440x956-en-150.png)
- 가로 956×440: [한국어 100%](HuntEdictOverviewEvidence/overview-956x440-ko-100.png) · [한국어 150%](HuntEdictOverviewEvidence/overview-956x440-ko-150.png) · [영어 150%](HuntEdictOverviewEvidence/overview-956x440-en-150.png)
- PC: [16:9 1600×900](HuntEdictOverviewEvidence/overview-1600x900-ko-100.png) · [16:10 1600×1000](HuntEdictOverviewEvidence/overview-1600x1000-ko-100.png) · [21:9 2100×900](HuntEdictOverviewEvidence/overview-2100x900-ko-100.png)

![새 영웅의 요약 화면](HuntEdictOverviewEvidence/overview-new-hero-1600x900-ko.png)

![지난 사냥 조언을 적용한 상태](HuntEdictOverviewEvidence/overview-advice-1600x900-ko.png)

![세로 화면, 영어 150%](HuntEdictOverviewEvidence/overview-440x956-en-150.png)

### 검증하지 않은 것

- 휴대폰·태블릿 실기기에서 보지 않았습니다. 세로·가로 배치는 macOS 창 크기로만 확인했습니다. 모바일 기기의 다시 그리기 시간도 측정하지 않았습니다.
- Android·iOS 빌드는 만들지 않았습니다.
- 처음 시작한 플레이어가 실제로 더 빨리 이해하는지는 사람을 대상으로 검증하지 않았습니다.
- 조언의 기준 수치는 실제 플레이 기록으로 보정하지 않았습니다.
