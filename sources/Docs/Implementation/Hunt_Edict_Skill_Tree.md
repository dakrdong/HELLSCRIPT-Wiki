# 사냥 칙령 스킬 트리 통합

갱신일: 2026-09-29 · 최초 작성 2026-09-23 · [English](Hunt_Edict_Skill_Tree.en.md)

## 화면과 조작

사냥 칙령의 **스킬** 탭에서 현재 직업의 스킬 37개를 확인한다. 레벨 구간과 세 계열로 나누어 배치하고, 아이콘을 누르면 효과·해금 조건·등급을 보여 준다. 설명에 나오는 개발용 스킬 ID는 현재 언어의 스킬 이름으로 바꾼다.

일반 액티브 장착 칸 네 개와 별도 궁극기 칸 하나를 둔다. 궁극기는 동시에 하나만 장착한다. 패시브는 해금되고 포인트가 배분되어 있으면 적용하므로 장착 칸을 두지 않는다. 기존 버전 3 구성의 무료 기본 등급도 배분된 등급으로 인정한다. 전투 HUD도 같은 일반 액티브 네 칸·궁극기 한 칸과 실제 재사용 대기시간을 읽는다.

장착된 액티브를 누르면 **스킬 빼기**, **사냥 칙령 편집** 메뉴가 열린다. 편집을 선택하면 해당 스킬의 자동 사용과 사용 조건을 편집한다. 스킬 트리로 돌아가도 편집 내용은 유지된다. 별도의 ‘스킬 사냥 칙령’ 하위 탭은 제거했다. 기본 공격과 공격 순서는 ‘공통 공격 설정’에서 편집한다.

가로 화면은 왼쪽에 트리, 오른쪽 열 위에 스킬 설명, 그 아래에 장착 칸을 둔다. 세로 화면은 위에 트리, 맨 아래에 장착 칸을 고정하고, 스킬을 고르면 둘 사이에 설명이 열린다. 장착 칸·주요 분류·하단 저장 버튼은 고정하며, 트리와 설명·설정 본문은 각각 스크롤한다. 스킬별 칙령 편집 화면도 장착 칸을 아래에 둔다. 한국어·영어와 글자 확대를 지원한다.

## 전통 핵앤슬래시식 트리 화면 (2026-09-27 개편)

사용자는 기존 트리가 스킬마다 상자를 나열해 아이콘이 상자 위쪽에 붙어 어긋나 보이고 허술하다고 지적했다. 모바일 액션 RPG와 디아블로 3·4를 참고해 전통 핵앤슬래시의 스킬 트리처럼 다시 만들었다. 디아블로 4·3 화면은 내장 브라우저로 확인했으며, 다른 게임의 이미지나 화면은 가져오지 않았다.

- **디아블로 4 참고**: 해금한 길을 따라 빛나는 가지, 단계 관문의 마름모, 아이콘 아래의 등급 표시(`2/5`), 트리 옆의 설명, 남은 포인트 표시.
- **디아블로 3 참고**: 장식 테두리의 둥근 장착 칸, 장식선이 있는 단계 제목.
- **모바일 참고**: 세로 화면에서 아래에서 열리는 설명, 맨 아래에 고정한 장착 칸, 잠긴 스킬의 해금 레벨 표시, 큰 누름 영역.

세 계열을 세로 줄기로 그린다. 레벨만 필요한 스킬은 줄기에 달리고, 같은 단계의 선행 스킬이 있는 스킬은 그 바로 아래에 연결선으로 달린다. 영웅 레벨이 닿은 곳까지 줄기와 연결선이 황동색으로 빛나고, 아직 열리지 않은 단계는 어둡게 가리며 스킬마다 `Lv.26`처럼 해금 레벨을 보여 준다. 궁극기는 마지막 단계에서 줄기 끝에 놓인다. 스킬을 고르면 다른 단계나 계열에 있는 선행 스킬과, 그 스킬이 여는 스킬까지 밝은 선으로 이어 준다.

액티브는 원형, 패시브는 사각, 궁극기는 네 갈래 문장 테두리를 쓴다. 테두리·트리 배경·포인트 보석은 사용자 요청에 따라 GPT로 생성했다. 테두리의 투명한 안쪽 폭을 측정해 아이콘을 그 안에 가운데 맞추므로 테두리와 아이콘이 어긋나지 않는다. 등급을 올린 스킬과 장착한 스킬은 은은한 빛, 고른 스킬은 밝은 빛으로 표시하고, 장착한 스킬에는 장착 칸 번호(궁극기는 마름모)를 붙인다. 선택을 테두리 선으로 그리면 다시 상자처럼 보여 빛만 사용한다. 제작 기록은 [스킬 트리 화면 이미지 제작 기록](../Art/SkillTreeUi/Skill_Tree_UI_Art.md)에 있다.

저장과 규칙은 바뀌지 않았다. 해금·등급·장착은 기존 `ClassSkillTree`와 편집본이 판정하고, 새 배치 코드(`SkillTreeLayout`)와 장식 메시(`SkillTreeGraphic`)는 화면에만 쓰인다. 기존 버튼 이름을 유지해 튜토리얼 안내와 다른 스모크가 같은 버튼을 찾는다.

## 성장과 전투 적용

`Prototypes/SkillTree/tree-design.cjs`의 연결과 레벨을 `tools/build_skill_tree.cjs`가 게임 리소스 `ClassSkillTree.json`으로 생성한다. HTML과 게임의 성장 조건을 별도로 수기 관리하지 않는다. 기존 버전 3 구성은 무료 기본 1등급과 강화 예산을 보존한다. 배분 초기화를 실행하면 버전 4로 전환하여 모든 스킬을 0등급으로 비우고 시작 포인트를 포함한 레벨 수만큼의 포인트를 돌려준다. 이후에는 0→1등급부터 등급마다 1포인트를 사용한다. 1레벨에서 1포인트, 40레벨에서 40포인트이며, 미투자 스킬은 장착하거나 효과를 적용할 수 없다. 기존 구성을 열거나 불러오는 것만으로 배분을 바꾸지 않는다.

직접 강화하는 스킬의 선행 관계와 궁극기의 ‘40레벨 이상 + 직전 30~39레벨 구간 스킬 중 하나 활성화’ 조건을 유지한다. 해금과 장착은 구분한다. 선행 스킬을 네 칸에 장착할 필요는 없다.

기존 여섯 패시브는 `HeroStats`, 확장 패시브는 기존 전투 효과 소유 코드에서 해금 여부와 배분 등급을 검사한다. 효과가 발생하는 고유 조건은 유지한다. ‘항상 적용’은 효과를 사용할 수 있다는 뜻이며, 적중·빙결 등 발동 조건 자체를 제거하지 않는다.

## 저장과 기존 데이터

`HuntEdictEditSession`의 편집본을 `GameStore.CommitHuntEdict`로 검증하고 한 번에 저장한다. 등급·액티브 위치·궁극기·자동 사용·스킬별 조건·공격 순서·전역 칙령을 함께 저장한다. 화면 재배치와 언어 변경은 저장을 실행하지 않는다.

기존 플레이어 구성은 `ClassSkillLoadout` 버전 3을 사용하며, 명시적으로 배분을 초기화한 구성은 버전 4를 사용한다. 시작 시 실제 소유 캐릭터를 원자적으로 전환하고, 원래 빌드는 보존한다. 새로운 해금 레벨에 미달하는 투자분은 기본 등급으로 되돌려 포인트를 돌려준다. 검증 전용 버전 1·2 구성은 기존 의미를 유지한다. 진행 중인 이전 균열은 저장된 구성을 유지하며, 해당 균열에서 스킬을 편집하면 기존 변경 거래로 전환한다.

프리셋 공유는 이름과 전체 스킬 구성을 담은 HED5를 사용한다. HED1~HED3 가져오기와 HED4 스킬 구성을 읽는 기존 경로를 유지한다. 불러온 과다 투자분은 보관할 수 있지만 실제 적용은 차단한다.

진행 중인 균열에서 구성 변경 시 체력 비율과 이미 소비한 쿨타임을 유지한다. 궁극기 교체로 공용 쿨타임을 초기화하지 않는다. 제거된 스킬의 진행 중 준비·집중과 이동 의도는 취소하며, 이미 방출된 효과의 저장 검증을 유지한다.

새 전설·세트의 일반 드롭 등록은 이 UI 통합에 포함하지 않는다.

## 검증

검증 결과와 캡처는 이 문서의 후속 기록에 연결한다. macOS 개발 플레이어의 uGUI 포인터 검증과 별도 프로세스 저장 복원은 모바일 실기기 검증과 구분한다.

검증 완료: Unity Edit Mode 회귀 검사 417개 통과(실패·건너뜀 0), 후속 저장·전환·번역 검사 71개 통과(실패·건너뜀 0). 두 실행에는 중복되는 검사가 있으므로 합산한 고유 검사 수로 표기하지 않는다. macOS 개발 빌드는 성공했고, 실제 uGUI 레이캐스트와 포인터 누름·놓음·클릭으로 투자·장착·제거·칙령 설정·저장을 검증했다. 별도 플레이어 프로세스로 저장을 다시 읽어 동일 구성을 확인했다.

- [검증 요약](HuntEdictSkillTreeEvidence/validation.json)
- [장착 슬롯 메뉴](HuntEdictSkillTreeEvidence/slot-menu.png)
- [마법사 패시브와 스킬 이름](HuntEdictSkillTreeEvidence/mage-detail-ko.png)
- [세로 화면·영어·글자 150%](HuntEdictSkillTreeEvidence/portrait-large-en.png)
- [스킬별 칙령 편집](HuntEdictSkillTreeEvidence/policy-large-en.png)
- [조작 결과](HuntEdictSkillTreeEvidence/runtime.txt) · [재실행 결과](HuntEdictSkillTreeEvidence/restart.txt)

세로 440×956, 가로 956×440, PC 1600×900·1600×1000·2100×900을 확인했다. 현재 열린 Unity 편집기와 모바일 실기기에서 실행한 결과는 아니다.

### 2026-09-27 개편 검증

최신 `main`(`c751d84d`)에서 만든 별도 작업 폴더를 복제한 프로젝트로 빌드하고 검사했다. 사용자가 열어 둔 Unity 편집기와 실제 계정 저장은 사용하지 않았다.

- Unity Edit Mode 관련 검사 **131개 통과, 실패 0, 건너뜀 0**. 새 트리 배치 검사 3개(직업별)와 번역·공통 UI·버튼·사냥 칙령·빠른 설정·패시브 검사를 포함한다. 첫 실행에서는 번역 검사 1개가 실패했다. 원인은 이번 변경이 아니라 기존 커밋 `ce236394`의 첫 플레이 검증 문구 두 줄에 영어 항목이 없던 것이며, 항목을 추가한 뒤 다시 실행해 통과했다. [검사 XML](HuntEdictSkillTreeRedesignEvidence/editmode.xml)
- 공통 UI 소유 검사와 회귀 9개, 테두리 안쪽 폭 측정 검사(`Docs/Art/SkillTreeUi/measure.py`)가 통과했다.
- macOS 개발 빌드가 성공했다. 스킬 트리 스모크는 실제 uGUI 레이캐스트와 포인터 누름·놓음·클릭으로 등급 투자, 장착, 장착 칸 메뉴, 제거, 스킬별 칙령 편집, 화면 회전, 언어 전환, 저장을 확인했고, 별도 플레이어 프로세스에서 저장을 다시 읽어 같은 구성을 확인했다. [조작 결과](HuntEdictSkillTreeRedesignEvidence/runtime.txt) · [재실행 결과](HuntEdictSkillTreeRedesignEvidence/restart.txt)
- 이 스모크는 새 계정이 필수 튜토리얼부터 시작하도록 바뀐 뒤 칙령 창이 열리지 않아 실패하고 있었다. 튜토리얼을 마친 계정으로 시작하도록 고쳤다. 가로 화면의 영어·한국어 150% 글자와 레벨 18 영웅의 잠긴 단계 캡처도 추가했다.
- 확인한 화면: 세로 440×956, 가로 956×440, PC 1600×900(16:9)·1600×1000(16:10)·2100×900(21:9), 한국어·영어, 글자 150%(세로·가로).

캡처:
- [PC 16:9 · 패시브 선택](HuntEdictSkillTreeRedesignEvidence/mage-detail-ko.png) · [PC 16:10 · 궁극기 단계](HuntEdictSkillTreeRedesignEvidence/layout-1600x1000-ko.png) · [PC 21:9](HuntEdictSkillTreeRedesignEvidence/layout-2100x900-ko.png)
- [세로 440×956](HuntEdictSkillTreeRedesignEvidence/layout-440x956-ko.png) · [가로 956×440](HuntEdictSkillTreeRedesignEvidence/layout-956x440-ko.png)
- [세로 150% 한국어](HuntEdictSkillTreeRedesignEvidence/portrait-large-ko.png) · [세로 150% 영어](HuntEdictSkillTreeRedesignEvidence/portrait-large-en.png) · [가로 150% 한국어](HuntEdictSkillTreeRedesignEvidence/landscape-large-ko.png) · [가로 150% 영어](HuntEdictSkillTreeRedesignEvidence/landscape-large-en.png)
- [레벨 18 전사: 빛나는 가지와 잠긴 단계](HuntEdictSkillTreeRedesignEvidence/progress-lv18-440x956.png)
- [장착 칸 메뉴](HuntEdictSkillTreeRedesignEvidence/slot-menu.png) · [스킬별 칙령 편집](HuntEdictSkillTreeRedesignEvidence/selected-skill-policy.png) · [칙령 편집 150% 영어](HuntEdictSkillTreeRedesignEvidence/policy-large-en.png)
- 개편 전: [가로](HuntEdictSkillTreeEvidence/layout-1600x900-ko.png) · [세로](HuntEdictSkillTreeEvidence/layout-440x956-ko.png)

모바일 비율은 macOS 창에서 재현했다. 모바일 실기기의 터치·성능은 확인하지 않았다.

## 2026-09-28 초기 배분과 상세 화면 정리

- 남은 스킬 포인트 아래의 해금 개수를 제거했다. 스킬 설명에서는 사용 순서·자동 사용·칙령 요약과 사용 조건 확인 버튼을 제거했다. 장착 칸 메뉴에서 스킬별 칙령 편집에 계속 접근할 수 있다.
- 세 직업의 첫 줄 액티브 세 종을 모두 1레벨부터 선택할 수 있다. 전사는 회오리·도약 내려찍기·분쇄 일격, 궁수는 관통 사격·다중 사격·맹독 덫, 마법사는 화염구·눈보라·연쇄 번개다. 패시브와 이후 단계의 해금 레벨은 유지한다.
- 초기화는 편집본의 모든 등급과 장착 칸을 비운다. 저장하기 전에는 실제 캐릭터에 영향을 주지 않으며 되돌리기로 복구할 수 있다. 다시 포인트를 넣어야 해당 스킬을 장착할 수 있다. 마지막 1포인트를 빼면 해당 스킬의 장착과 공격 순서도 함께 정리한다.
- 0등급과 포인트 예산은 `ClassSkillTree`와 `ClassSkillLoadout`이 소유한다. 이전 저장·프리셋은 기존 규칙으로 읽고, 초기화한 구성을 저장한 뒤에는 다시 열거나 게임을 재실행해도 무료 1등급을 끼워 넣지 않는다.
- 전투의 기존 스킬 판정도 `ClassSkillTree.UnlockLevel`을 참조한다. 표시만 1레벨로 바뀌고 실제 자동 사용은 이전 3·6레벨에 막히는 불일치를 방지한다.

### 이번 변경의 검증

Unity 6000.6.0f1의 Edit Mode에서 초기화·거래·저장·공유·번역 관련 검사 112개와 전투 회귀 검사 502개가 각각 통과했다. 두 실행 모두 실패와 건너뜀은 0개다. HTML 트리 검사 39개, 공통 UI 소유 검사와 회귀 9개도 통과했다.

macOS 개발 빌드에서 세 직업의 1레벨 캐릭터로 초기화, 되돌리기, 첫 줄 두 번째·세 번째 액티브에 포인트 배분과 장착, 장착 칸의 칙령 편집, 저장·다시 열기, 마지막 포인트 반환을 uGUI 포인터로 확인했다. 전부 0등급인 구성을 저장한 뒤 별도 게임 프로세스로 다시 읽어 남은 1포인트와 빈 장착 칸이 유지되는 것을 확인했다. 기존 버전 3 구성의 등급·장착·칙령 편집과 재실행 복원 검사도 통과했다.

세로 440×956, 가로 956×440, PC 1600×900·1600×1000·2100×900 각각에서 한국어·영어와 글자 100%·150%를 조합한 20개 화면을 확인했다. 이 작업에서는 연결된 Unity 편집기가 없어 기존 배치 검사·빌드 경로를 사용했다. 실제 계정 저장은 건드리지 않았으며, 모바일 실기기 입력과 성능은 확인하지 않았다.

- [검증 요약](HuntEdictSkillResetEvidence/validation.json) · [초기화·저장 검사](HuntEdictSkillResetEvidence/editmode.xml) · [전투 회귀 검사](HuntEdictSkillResetEvidence/combat-editmode.xml) · [빌드 결과](HuntEdictSkillResetEvidence/build.txt)
- [포인터 조작](HuntEdictSkillResetEvidence/runtime.txt) · [0등급 재실행 복원](HuntEdictSkillResetEvidence/restart.txt) · [기존 구성 조작](HuntEdictSkillResetEvidence/legacy-runtime.txt) · [기존 구성 재실행 복원](HuntEdictSkillResetEvidence/legacy-restart.txt)
- [PC 한국어](HuntEdictSkillResetEvidence/desktop-ko.png) · [세로 영어 150%](HuntEdictSkillResetEvidence/portrait-en-150.png) · [가로 한국어](HuntEdictSkillResetEvidence/landscape-ko.png) · [가로 영어 150%](HuntEdictSkillResetEvidence/landscape-en-150.png) · [초기화 후 빈 장착 칸과 반환 포인트](HuntEdictSkillResetEvidence/reset-empty-ko.png)

## 2026-09-29 장착 스킬 옆의 작은 메뉴

장착한 일반 액티브나 궁극기를 누르면 해당 아이콘 바로 위에 작은 메뉴가 열린다. 위쪽 공간이 부족하면 아래에 놓고, 좌우 끝에서는 안전 영역 안으로 옮긴다. 전체 화면에 어두운 배경을 덮지 않으며, 스킬 이름과 닫기, **스킬 빼기**, **사냥 칙령 편집**만 보여 준다.

바깥을 누르거나 닫기·뒤로가기를 실행하면 메뉴만 닫힌다. 바깥을 누른 입력이 아래의 다른 탭이나 버튼으로 전달되지는 않는다. 화면 방향·안전 영역·언어·글자 크기가 바뀌면 새로 그린 같은 장착 칸을 기준으로 메뉴 위치를 다시 계산한다. 메뉴를 열고 닫는 동작은 편집본과 저장에 영향을 주지 않으며, 제거·편집은 기존 거래 경로를 사용한다.

### 작은 메뉴 검증

Unity Edit Mode 관련 검사 99개와 공통 UI 회귀 검사 9개가 통과했고, macOS 개발 빌드는 오류 없이 완료됐다. 세로 440×956, 가로 956×440, PC 1600×900·1600×1000·2100×900에 한국어·영어, 글자 100%·150%를 조합한 20개 화면에서 장착 칸 다섯 개를 각각 눌렀다. 총 100개 메뉴의 위치·크기·문자 잘림·바깥 클릭 처리를 확인했다.

메뉴를 연 상태의 화면 회전·언어·글자 크기 변경과 모의 안전 영역, 칙령 편집으로 이동, 일반 액티브·궁극기 제거, 되돌리기와 저장을 확인했다. 별도 게임 프로세스로 저장을 다시 읽어 제거한 칸은 비어 있고 다른 스킬은 유지되는 것도 확인했다. 검증은 분리된 저장을 사용하는 macOS 개발 플레이어의 uGUI 포인터 입력으로 수행했다. 연결된 Unity 편집기와 모바일 실기기는 사용하지 않았다.

- [검증 요약](HuntEdictSkillMenuEvidence/validation.json) · [Edit Mode 검사](HuntEdictSkillMenuEvidence/editmode.xml) · [빌드 결과](HuntEdictSkillMenuEvidence/build.txt)
- [포인터 조작](HuntEdictSkillMenuEvidence/runtime.txt) · [재실행 복원](HuntEdictSkillMenuEvidence/restart.txt)
- [PC 한국어](HuntEdictSkillMenuEvidence/desktop-ko.png) · [세로 영어 150%](HuntEdictSkillMenuEvidence/portrait-en-150.png) · [가로 한국어](HuntEdictSkillMenuEvidence/landscape-ko.png) · [오른쪽 끝 궁극기·영어 150%](HuntEdictSkillMenuEvidence/ultimate-edge-en.png) · [모의 안전 영역](HuntEdictSkillMenuEvidence/safe-area.png)
