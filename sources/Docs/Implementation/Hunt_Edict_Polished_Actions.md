# 사냥 칙령 버튼과 두루마리 편집 그림

갱신일: 2026-10-01 · [English](Hunt_Edict_Polished_Actions.en.md)

## 화면 구성

사냥 칙령의 버튼은 짙은 무광 바탕, 얇은 청동·금색 선, 작게 둥근 모서리를 사용합니다. 두 겹의 테두리, 양옆 마름모와 강한 아래 빛은 표시하지 않습니다. 선택한 탭은 얇은 아래 선과 차분한 선택 바탕으로 구분하고, 주요 행동은 금색 외곽선으로 구분합니다. 기본·선택·마우스 올림·누름·키보드 포커스·비활성 상태는 공통 `UiButton`이 계속 관리합니다.

스킬 옆 말풍선은 하나의 작은 바탕 안에 −와 편집 그림을 나란히 놓고 얇은 구분선으로 나눕니다. 미장착 스킬에는 +만 나옵니다. 기본 크기에서 장착 말풍선은 88×48, 미장착 말풍선은 48×48 논리 단위입니다. 실제 누름 영역과 이동·장착·저장 동작은 기존 경로를 유지합니다.

요약에는 기존처럼 공격적·균형·신중의 제목과 설명만 표시합니다. 세 선택 카드는 같은 버튼 역할을 사용하며, 선택한 카드는 금색 선과 체크 표시로 구분합니다. 상단의 기본 세팅 On/Off, 하단의 기존 저장·되돌리기, 화면 크기에 맞춘 세로·가로 배치는 유지합니다.

## 생성 그림

편집 그림은 이미지 생성기의 **내장 생성 경로**로 만든 두루마리·붓 PNG입니다. 코드로 그리던 `edict-edit`는 제거했습니다. 원본 PNG를 가공하지 않고 그대로 복사했으며 원래 알파를 보존합니다.

- 런타임 그림: [`edict-edit-scroll-brush.png`](../../Assets/HELLSCRIPT/Resources/Art/HuntEdict/Actions/edict-edit-scroll-brush.png)
- 생성 문구·해시·출처: [`action-icon-provenance.json`](../../Assets/HELLSCRIPT/Art/HuntEdict/action-icon-provenance.json)
- 원본은 1254×1254 RGBA입니다. 투명 픽셀 860,568개, 네 모서리 알파 0을 확인했습니다. 주변 배경 제거·크로마 키·SVG 대체를 사용하지 않았습니다.
- Unity는 알파 입력, 전체 사각 스프라이트, Clamp, Bilinear, mipmap 없음으로 가져옵니다. 데스크톱·WebGL은 최대 256, Android는 최대 128입니다. 화면에서는 32×32 논리 단위로 표시하며 장식 그림이 입력을 가로채지 않습니다.

내장 호출 결과에 모델명이 없으므로 특정 모델로 제작했다는 표기나 최종 미술 승인을 만들지 않습니다. 출처 상태는 `candidate_model_unknown`으로 남깁니다. 이 기록은 실제 게임에 연결한 사실과 모델·미술 승인 상태를 구분합니다.

## 공통 부품과 소유권

`UiButtonChrome.Minimal`은 공통 버튼 표시의 선택 가능한 양식입니다. 사냥 칙령의 버튼 생성 진입점에서만 선택하며 다른 화면의 기본 양식은 바꾸지 않습니다. `UiTheme`의 기존 의미 색과 `UiFonts`를 공유하고, 화면별 버튼 색·입력 상태를 복제하지 않습니다. 아이콘의 이미지 리소스만 새로 연결하며 장착·정책 저장·기본 세팅·편집본의 소유권은 기존 모델과 거래에 남습니다.

## 검증

2026-10-01 통합 코드 `f9d028c6`에서 확인했습니다. 원본은 [검증 기록](HuntEdictPolishedActionsEvidence/validation.json), [Edit Mode 보고서](HuntEdictPolishedActionsEvidence/editmode.xml), [macOS 입력 기록](HuntEdictPolishedActionsEvidence/menu-runtime.txt), [새 프로세스 복원 기록](HuntEdictPolishedActionsEvidence/menu-restart.txt)에 보존합니다.

- 공통 UI 계약 검사와 관련 Python 검사 11개가 통과했습니다.
- `UiButtonTests`·`HuntEdictOverviewTests` 38개 통과, 실패·건너뜀 0개입니다. 무관한 전체 전투 검사를 실행하지 않았습니다.
- macOS 개발 빌드가 오류 0개로 성공했습니다. 최종 통합 스모크를 한 번 실행하고, 같은 저장 경로의 새 프로세스에서 장착 해제와 전투 방식 복원을 확인했습니다. PNG는 38장입니다.
- 440×956·956×440·1600×900·1600×1000·2100×900의 한국어·영어, 기본 글자 크기에서 요약·스킬 말풍선·인장 표시·프리셋 바로 이동을 확인했습니다. 전투·프리셋 공유의 새 버튼 양식도 세로와 PC에서 캡처했습니다.
- 실제 네이티브 Player에서 레이캐스트와 uGUI 포인터 이벤트를 사용했습니다. +·−·편집, 슬롯 교체, 궁극기 해제, 바깥 클릭·뒤로 가기, 안전 영역·회전, 되돌리기·저장, 기본 세팅 On/Off와 구경 중 편집본·저장 데이터 불변을 확인했습니다. 물리 모바일 기기 검사와 OS 마우스 조작 결과로 표시하지 않습니다.
- 실행 중인 원본 Unity Editor도 통합 코드를 새로고침하고 같은 GUID의 그림을 가져온 것을 조회했습니다. 새 컴파일 오류는 없습니다. 기존 Unity AI `NoSubscription` 예외 5개는 그대로 남아 있으며 이번 UI의 오류로 합산하지 않습니다.

실제 게임 화면입니다. [전체 38장 원본 캡처](HuntEdictPolishedActionsEvidence/native-captures.zip)는 별도로 보존합니다.

![PC 스킬 말풍선의 두루마리와 붓](HuntEdictPolishedActionsEvidence/tree-bubble-1600x900-ko.png)

![요약의 세 전투 방식 카드](HuntEdictPolishedActionsEvidence/overview-1600x900-ko.png)

추가 화면: [세로 스킬](HuntEdictPolishedActionsEvidence/tree-bubble-440x956-ko.png) · [가로 영어 프리셋](HuntEdictPolishedActionsEvidence/policy-buttons-956x440-en.png) · [전투](HuntEdictPolishedActionsEvidence/combat-buttons-1600x900.png) · [프리셋 공유](HuntEdictPolishedActionsEvidence/preset-buttons-1600x900.png) · [기본 세팅 On의 비활성 표시](HuntEdictPolishedActionsEvidence/overview-defaults-on.png).
