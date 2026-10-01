# 사냥 칙령 장착 칸별 인장 색

갱신일: 2026-10-01 · [English](Hunt_Edict_Slot_Seals.en.md)

## 네 가지 장착 색

일반 액티브 네 칸에 고정된 인장 색을 사용합니다. 스킬트리, 장착 칸, 스킬 설명 머리글, 세부 칙령의 장착 레일은 같은 칸의 같은 그림을 표시합니다. 스킬의 등급·원소·프리셋 활성 상태를 나타내는 색이 아닙니다.

| 장착 칸 | 인장 색 | 이미지 |
| --- | --- | --- |
| 첫째 | 초록 | [기존 초록 인장](../../Assets/HELLSCRIPT/Resources/Art/SkillTree/node-frame-active-equipped.png) |
| 둘째 | 파랑 | [파란 인장](../../Assets/HELLSCRIPT/Resources/Art/SkillTree/node-frame-active-equipped-blue.png) |
| 셋째 | 호박색 | [호박색 인장](../../Assets/HELLSCRIPT/Resources/Art/SkillTree/node-frame-active-equipped-amber.png) |
| 넷째 | 보라 | [보라 인장](../../Assets/HELLSCRIPT/Resources/Art/SkillTree/node-frame-active-equipped-violet.png) |

빈칸은 기존 청동 인장이며, 앞 칸이 비어도 뒤 칸의 색을 당기지 않습니다. 스킬을 교체하면 새 스킬이 해당 칸의 색을 받습니다. 다른 칸에 다시 장착하면 그 칸의 색을 받습니다. 궁극기는 기존 별도 문장의 초록 장착 표시를 유지합니다. 패시브는 등급 배분으로 상시 적용되는 정사각형 인장입니다. 장착 번호 배지는 붙이지 않습니다.

## 그림과 소유 경계

세 색상은 내장 이미지 생성기에 기존 초록 인장을 참조 이미지로 제공한 색상별 PNG입니다. 그림을 코드로 다시 그리거나 색을 입히지 않고 원본 알파를 그대로 복사했습니다. 새 그림도 같은 1254×1254 캔버스와 중앙 정렬·안쪽 폭을 사용합니다. PNG 측정 결과의 안쪽 폭은 0.649~0.651이며 기존 배치 상수 0.66과의 차이는 0.015 이하입니다.

[생성 문구·해시·투명도·출처](../Art/SkillTreeUi/slot-frame-provenance.json)에 전체 프롬프트를 보존합니다. 내장 호출이 정확한 모델명을 반환하지 않아 `candidate_model_unknown`으로 기록했습니다. 특정 모델이나 최종 미술 승인을 주장하지 않습니다.

색은 `HuntEdictWindow.Skills`가 실제 편집본 `classSkills.actives`의 위치에서 읽습니다. 계정에 별도의 색 설정을 저장하지 않습니다. 동일한 `NodeFrame`을 사용하므로 장착 상태를 변경해도 아이콘·인장의 크기나 위치, 입력 영역을 바꾸지 않습니다. 실제 장착·해제·되돌리기·저장은 기존 `ClassSkillTree`·`HuntEdictEditSession`·`GameStore` 소유 경로를 유지합니다.

## 검증

2026-10-01 통합된 `743d6c87`에서 마지막 검증을 한 번 수행했습니다. 공통 UI 계약 검사와 11건의 계약 테스트, 다섯 인장 리소스의 Edit Mode 검사가 통과했습니다. Unity 6000.6.0f1 macOS 개발 빌드는 오류 0건으로 성공했습니다. 실제 메인 에디터의 가져오기·준비 상태도 확인했습니다. Console의 Unity AI 구독 관련 `NoSubscription` 5건은 C# 컴파일 실패와 구분해 보존했습니다.

기존 네이티브 메뉴 조작 묶음과 새 프로세스 저장 복원을 통과했습니다. 440×956·956×440·1600×900·1600×1000·2100×900, 한국어·영어, 안전 영역에서 네 색의 트리·칸·머리글·세부 칙령 레일 일치와 중앙 정렬을 확인했습니다. 빈칸, 교체, 파란 칸의 스킬을 초록 칸으로 재장착, 되돌리기·저장·다시 열기도 확인했습니다. 40개의 원본 캡처 중 아래 대표 화면을 기록합니다.

![PC 실제 화면](HuntEdictSlotSealsEvidence/tree-bubble-1600x900-ko.png)
![다른 칸에 재장착한 도약 내려찍기](HuntEdictSlotSealsEvidence/slot-color-moved.png)

[세로 화면](HuntEdictSlotSealsEvidence/tree-bubble-440x956-ko.png) · [가로 영어 화면](HuntEdictSlotSealsEvidence/tree-bubble-956x440-en.png) · [세부 칙령 레일](HuntEdictSlotSealsEvidence/policy-buttons-1600x900-ko.png) · [새 프로세스 복원](HuntEdictSlotSealsEvidence/menu-restart.png) · [결과와 범위](HuntEdictSlotSealsEvidence/validation.json) · [Edit Mode](HuntEdictSlotSealsEvidence/editmode.xml) · [조작 기록](HuntEdictSlotSealsEvidence/menu-runtime.txt)

조작 증거는 macOS 실제 플레이어에서의 uGUI 레이캐스트·포인터 이벤트입니다. OS 마우스나 모바일 실기기 검증으로 표시하지 않습니다. 기본 글자 크기만 사용했으며 전체 전투 검사와 글자 크기별 반복 검사는 실행하지 않았습니다.
