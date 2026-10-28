# 사냥 칙령 창 제거 후 진입 경로와 소스 소유

갱신일: 2026-10-28 · [English](Hunt_Edict_Window_Removal_20261028.en.md)

A2에서 큰 사냥 칙령 창과 균열의 ≡ 관찰 메뉴를 제거했다. HUD 얼굴 인장은 창을 열지 않는다. 사냥 칙령 버튼의 사이드 메뉴는 **스킬 → 전투 → 생존 → 탐색 → 전리품 → 가방·정리 → 반복 설정 → 추천 장비 → 프리셋·공유** 순서다. [메뉴 진입과 라우팅](../../Assets/HELLSCRIPT/Runtime/Presentation/GameUI.EdictSheet.cs), [9행 배치](../../Assets/HELLSCRIPT/Runtime/Presentation/GameUI.CasualEdictMenu.cs)가 현재 소유 코드다.

## 현재 화면과 저장

| 이전 탭·기능 | 현재 위치와 소유 코드 |
| --- | --- |
| 스킬 트리·장착 | 사이드 메뉴 **스킬**이 [독립 스킬 창](../../Assets/HELLSCRIPT/Runtime/Presentation/SkillTreeD4Window.cs)을 직접 연다. [진입·닫힘 처리](../../Assets/HELLSCRIPT/Runtime/Presentation/GameUI.HuntEdict.cs)는 안내 게이트도 해제한다. |
| 스킬별 직접 설정·사용 방식·호환 필드 | 스킬 창의 [스킬별 전술 진입](../../Assets/HELLSCRIPT/Runtime/Presentation/SkillTreeD4Window.Tactics.cs) → [전술 화면](../../Assets/HELLSCRIPT/Runtime/Presentation/SkillTacticsWindow.cs)과 [호환 설정 행](../../Assets/HELLSCRIPT/Runtime/Presentation/SkillTacticsWindow.Legacy.cs). 기본 추천 세팅 중에는 읽기 전용이다. |
| 전투·생존·탐색·전리품·가방 정리 | 같은 사이드 메뉴의 [설정 시트](../../Assets/HELLSCRIPT/Runtime/Presentation/GameUI.EdictSheet.cs)와 [카탈로그 행](../../Assets/HELLSCRIPT/Runtime/Presentation/GameUI.EdictCatalog.cs). [기존 그룹 정의](../../Assets/HELLSCRIPT/Resources/HuntEdictUi.json)를 읽고 편집할 때마다 저장한다. |
| 반복 조건·다음 단계·다음 판 준비 | **반복 설정**의 [독립 설정 창](../../Assets/HELLSCRIPT/Runtime/Presentation/RepeatSettingsWindow.cs). 기존 반복 설정 초안과 추가된 칙령 즉시 적용 구역의 저장 범위는 구분된다. |
| 자동 착용 공통·슬롯별 설정 | **추천 장비**의 [장비 설정 창](../../Assets/HELLSCRIPT/Runtime/Presentation/RiftGearSettingsWindow.cs). 기존 추천 팝업 설정과 추가된 칙령 즉시 적용 구역을 제공한다. |
| 프리셋 5칸·저장·불러오기·이름·공유 코드·가져오기 | **프리셋·공유**의 [독립 캐주얼 팝업](../../Assets/HELLSCRIPT/Runtime/Presentation/EdictPresetsWindow.cs)과 [기존 보관함 페이지](../../Assets/HELLSCRIPT/Runtime/Presentation/EdictPresetsWindow.Presets.cs). [기존 레이아웃](../../Assets/HELLSCRIPT/Runtime/Presentation/EdictPresetsLayout.cs)을 재사용한다. |
| 기본 세팅 적용·사용자 설정 전환 | 같은 팝업 상단의 [기존 모드 칩](../../Assets/HELLSCRIPT/Runtime/Presentation/EdictPresetsWindow.Presets.cs). Sharing 공개(균열 20단계 또는 기존 계정 접근 권한) 후 사용할 수 있고, 그전에는 해금 이유를 표시한다. 잠긴 목록 밖에서 `SetRecommendedEdict`로 즉시 저장한다. |
| 전투 방식·전역·스킬·기본 공격·공통 순서 간편 프리셋 | 같은 팝업의 **간편 프리셋** 페이지. 선택은 초안만 바꾸며 **저장**으로 확정한다. [기존 프리셋 정의와 적용](../../Assets/HELLSCRIPT/Runtime/Core/HuntEdictQuickPresets.cs), [스킬 정책 복사](../../Assets/HELLSCRIPT/Runtime/Core/HuntEdictSkillPresets.cs)를 유지한다. |
| 튜토리얼·안내 | [일반 안내](../../Assets/HELLSCRIPT/Runtime/Presentation/GameUI.Tutorials.cs), [프롤로그](../../Assets/HELLSCRIPT/Runtime/Presentation/GameUI.TutorialStaging.cs), [가이드](../../Assets/HELLSCRIPT/Runtime/Presentation/GameUI.GuideTutorial.cs), [보조 스킬 레슨](../../Assets/HELLSCRIPT/Runtime/Presentation/GameUI.GuideModifierLesson.cs)이 현재 화면을 가리킨다. 삭제한 비교 페이지의 구형 체크포인트는 [튜토리얼 이관](../../Assets/HELLSCRIPT/Runtime/Core/Tutorials.cs)이 처리한다. |

[공통 설정 행](../../Assets/HELLSCRIPT/Runtime/Presentation/EdictCasualRows.cs), [추가 설정 구역](../../Assets/HELLSCRIPT/Runtime/Presentation/EdictSettingsSection.cs), [칙령 편집 거래](../../Assets/HELLSCRIPT/Runtime/Core/GameStore.HuntEdict.cs)를 재사용한다. 전투 중 저장은 현재 시뮬레이션을 전달하며 저장 실패 시 실제 영웅·진행 균열·전투 상태를 보존한다. 저장 키와 기본·추천 값은 바꾸지 않았다. **기본 세팅 적용** 토글은 프리셋·공유 상단에서 기존 `useRecommendedEdict` 저장값을 바꾼다. 성공한 저장 알림이 팝업·전술·HUD를 갱신하며, 전투 중에는 main의 기존 처리처럼 현재 시뮬레이션의 정책을 갱신한다. 실패하면 모드·사용자 초안·프리셋·전투 정책을 유지한다.

## 제거된 화면과 과거 기록

- 큰 통합 창의 요약·검색·배지·변경 목록·안내 띠·저장 후 행동은 제거했다. 전투 방식 선택은 간편 프리셋 페이지에서 제공한다. 삭제된 창의 숫자 행·선택기·자동 착용·프리셋 partial 파일은 위 표의 현재 소유 코드로 대체됐다.
- 창 안의 실시간 전투 예시·기본 공격 도해와 그 표시 부품은 제거했다. [스킬 전술 화면](../../Assets/HELLSCRIPT/Runtime/Presentation/SkillTacticsWindow.cs)은 직접 설정을 소유하며, 예전 실시간 미리보기를 제공하지 않는다. 과거 예시 캡처·검사 수치는 당시 구현의 증거다.
- ≡ 버튼과 관찰 팝업은 제거했다. 관찰 메뉴 전용 일시정지·배속·행동 수정·절전 엿보기 바로가기는 추가하지 않았다. [지도 패널](../../Assets/HELLSCRIPT/Runtime/Presentation/GameUI.Rift.cs)로 확대 지도를 열 수 있고, [포탈 귀환](../../Assets/HELLSCRIPT/Runtime/Presentation/GameUI.CasualBattleHud.cs)과 [훈련 일시정지](../../Assets/HELLSCRIPT/Runtime/Presentation/GameUI.TrainingGround.cs)는 남아 있다. [나머지 HUD 조회 팝업](../../Assets/HELLSCRIPT/Runtime/Presentation/GameUI.GlobalHud.cs)은 별개다.
- 옛 창 전용 runtime smoke와 Editor 캡처 도구도 제거했다. 해당 실행 플래그·오브젝트 이름·줄 번호는 현재 검사 지시가 아니다. 진입·설정·저장 검사는 [메뉴 검사](../../Assets/HELLSCRIPT/Tests/Editor/EdictMenuColumnTests.cs), [프리셋 검사](../../Assets/HELLSCRIPT/Tests/Editor/EdictPresetsWindowTests.cs), [전술 검사](../../Assets/HELLSCRIPT/Tests/Editor/SkillTactics/SkillTacticsWindowTests.cs), [중단 저장 검사](../../Assets/HELLSCRIPT/Tests/Editor/HuntEdictSuspendedSaveTests.cs), [HUD 스모크](../../Assets/HELLSCRIPT/Runtime/Presentation/RuntimeGlobalHudSmoke.cs)가 맡는다. 이 문서 수정으로 Unity나 runtime 검사를 새로 실행하지 않았다.

이전 문서와 위키 개정본의 삭제된 소스 링크는 현재 소유 파일로 교정했다. 그 문서에 남은 옛 심볼·줄 번호·캡처·검증 결과는 과거 구현 기록이며, 현재 동작과 소유 경로는 이 표를 따른다. 증거 manifest의 원본 경로·해시는 당시 입력을 증명하므로 변경하지 않는다.
