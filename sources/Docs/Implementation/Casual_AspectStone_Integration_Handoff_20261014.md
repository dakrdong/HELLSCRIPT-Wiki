# 위상 각인석 — 단일 통합 인계

2026-10-14. 이 문서가 개별 Unity 실행·개별 플레이어 빌드 계획을 대체한다. 소스 담당은 문서 인계 후 편집을 중지하며 통합 담당의 구체적인 결함 요청이 있을 때만 재개한다. 기존 생성·검사는 반복하지 않았다.

## 소유자와 안전한 최종 상태

- 통합 소유자: `01a13790-39bc-7062-a516-b83a354000a8`.
- 통합 위치: `/Users/t8g-2410-pn-005/Documents/Codex/2026-10-14/task-6/skill-skin`의 별도 통합 브랜치. 해당 프로젝트의 기존 Library 재사용. task-16에서는 Unity·빌드를 실행하지 않는다.
- 원본 소스: `/Users/t8g-2410-pn-005/Documents/Codex/2026-10-14/task-16/aspect-skin`, `codex/casual-aspect-skin`.
- 최종 **코드** 커밋: `a00ecaff8b50a0319e80190b339bf79c246a873b`. 인계 작업 시작 시 미커밋 변경 0. 이후 수정은 이 인계문과 보고서·큐의 실행 계획뿐이며 C#·PNG·설정 변경 0.
- 최종 문서 커밋과 전체 파일 SHA는 task-16의 `validation-queue.json`에 기록한다. 소스 브랜치·기존 증거·원화·실패 로그는 보존한다.

## 통합 순서

1. 공통 선행 커밋 `0ccba80eef0da4703cb7ffcd1fb8e22848002b3d` 또는 이를 포함하는 공통 통합 브랜치를 사용한다. 해당 커밋에 Noto 기호 보강, 상세 geometry 복원, 장비359 공급자가 포함된다.
2. 화면 구현 `d0c10513911b6d4e098eb25462c0735aa6f18dbd`를 통합한다.
3. 화면 공급자 연결·검사 보강 `a00ecaff8b50a0319e80190b339bf79c246a873b`를 통합한다.
4. 이 문서의 최종 문서 커밋을 통합한다. 정확한 값은 `validation-queue.json.commit` 또는 원본 브랜치 HEAD에서 읽는다.

권장 방식은 공통 기준을 포함한 통합 브랜치에 2→3→4를 순서대로 cherry-pick하는 것이다. 이미 포함된 변경은 재적용하지 않는다. `15f5baf5c71a54a1cc12fcbc6f1d760409a34659`는 위상 브랜치가 공통0cc를 가져온 병합 기록이므로 별도로 cherry-pick할 필요가 없다. 위상 브랜치 전체를 병합하는 방식도 가능하지만 두 방식을 중복 적용하지 않는다. 원본 repo checkout·main·원격은 변경하지 않는다.

## 파일과 원화 의존성

화면 소유 코드는 `AspectStoneWindow.cs`, `AspectStoneSkin.cs`, `AspectStoneArt.cs`, `AspectRuneGraphic.cs`, `RuntimeAspectStoneSmoke.cs`, `AspectStoneSkinTests.cs`다. 새 파일의 meta와 아래 원화·폴더 meta를 함께 가져온다. 공통 파일 독자 수정 없음.

| 역할 | Resources 경로 | 승인 근거 및 처리 |
|---|---|---|
| 위상 LW02 문양 | `Art/CasualAspectStone/LW02` | 화면 d0 커밋에 PNG/meta 포함. 1,168,223 bytes, 1254×1254, SHA `7eaedc552682d30ece36195a7d7b6cf139aeb250bc577acbd0bbd607a78ff27e` |
| 장비359 | `Art/Casual/Equipment/{id}` | 공통0cc의 `CasualEquipmentArt.Provider`와 `tools/casual_equipment_art_receipt.json`. 330 item key/30 emblem key, 359 PNG SHA 확인 완료. 통합 프로젝트에서 Resources 실파일 필요 |
| 장비 LW02 | `Art/Casual/Equipment/LW02` | 512×512, SHA `e922415daabd88aeb3a684b81ed40d1488aa4869cb36f07e534d2d7337605a0f`. 위상 문양과 별개이므로 합치거나 제거하지 않음 |
| 글꼴 | `Fonts/CasualSkill/` | 공통0cc의 Jua/Noto400·700·900과 기존 meta 사용 |
| 나머지 위상·돌·캐릭터 | 기존 Resources | 기존 이미지·경로·GUID 보존. 전역 재화 교체 없음 |

task-16은 sparse 소스 checkout으로 공통 장비359 PNG를 별도 복제하지 않았다. 공통 Resources 증가량은 69,753,836 bytes이며 화면 자체 추가 PNG는 위상 LW02 1장이다. 승인 위상124개 매핑은 `CasualAspectStoneEvidence20261014/approved-art-manifest.json`에 있다.

## 공통 연결 완료와 남은 차이

메인은 `UiSkin.Casual.WithEquipmentArt(CasualEquipmentArt.Provider)`를 사용하고 확인/완료·장비 슬롯·상세는 동일한 `view.Skin`을 받는다. 전역 Casual 공급자는 변경하지 않는다. 현재 필요한 API 연결 누락은 없다. 차트·전투·칙령 API는 이 화면에서 사용하지 않는다.

남은 시안 차이는 공개된 상태로 통합한다. 일반800×450 셸에서 기본16:9 도감은 1열이며 시안5열과 다르다. 1440×810 기준+패널300/742/362 제안은 물리 이름 글자를19.8px→11px로 줄여 승인되지 않았다. 글자·기준 크기·패널 폭을 임의 축소하지 않았다. 공통 Item 피드백은 각진 링으로 유지되어 카드 안쪽6단위로 넣는 보완도 유지했다. 위상은 LW02 대표만 신규 원화다. 필드/초기화/분해 탭과 시안4필터는 게임의 해당 화면 기능과 달라 추가하지 않았다.

## 통합 프로젝트에서 사용할 관련 검사

이 목록은 단일 통합 검증에 전달하는 선택지이며 task-16의 별도 실행 계획이 아니다. 기존 정적 검사를 다시 돌리지 않았다.

- EditMode 필터: `Hellscript.Tests.AspectStoneTests`; `Hellscript.Tests.AspectStoneSkinTests`; `Hellscript.Tests.StoreViewBindingTests`; `Hellscript.Tests.ItemArtCoverageTests.EveryReleasedAspectIdentityUsesNativeArtworkAboveItsStone`.
- 스모크 진입: `Hellscript.RuntimeAspectStoneSmoke.Boot`, `AfterSceneLoad`. `UNITY_EDITOR || DEVELOPMENT_BUILD`에서 컴파일되며 `Debug.isDebugBuild`와 `-hellscriptAspectStoneSmoke`를 요구한다.
- 최초 실행 인자: `-noaudio -hellscriptAspectStoneSmoke -hellscriptSavePath <통합담당이 지정한 새 전용 세이브 디렉터리> -hellscriptScreenshots <통합담당이 지정한 증거 디렉터리>`.
- 재실행 확인: 같은 전용 세이브·증거 경로와 `-hellscriptAspectStoneSmoke -hellscriptAspectStoneResume`를 함께 사용한다. Resume만 넣으면 Boot되지 않는다.
- Fixture는 전용 계정의 인벤토리·프리셋·위상 데이터를 바꾼다. 사용자 세이브를 연결하면 안 된다. 무음 실행과 통합 검증 프로세스 순서는 통합 담당이 관리한다. 스모크는 완료/예외 시 `Application.Quit`하므로 여러 스모크 플래그를 한 프로세스에 동시에 넣지 않는다.
- 최초 성공은 `initial.txt`·PNG·로그 `HELLSCRIPT_ASPECT_STONE_OK`를 남기고 재실행은 수집16개/Lv.5/장비 각인 복원을 확인해 `resume.txt`를 남긴다. 크기 변경과 캡처를 수행하므로 그래픽 Player가 필요하다.
- KO/EN, 선택·비활성·빈 검색·stale plan, 실제 uGUI synthetic 클릭/드래그/휠, 취소 무거래·확정 단일 receipt·수량 보존·저장/resume를 검사한다.
- 비율: 440×956, 956×440, 1440×810, 1440×900, 1680×720, 2868×1320. 마지막 안전영역 가정은 좌우102/아래54/위0px. Mac 시뮬레이션이며 iOS 실기기 증거가 아니다.

## 기존 증거와 미완료 상태

기존 UI 계약15개 통과, UI refresh16 binding/192 source 통과. 최신 정적 컴파일은 Runtime835/Editor53/Tests259 모두 오류0, 경고47/1/2, 공통 대비 추가 경고0. Editor는 새로 컴파일한 참조이며 입력 변경0. `CasualAspectStoneEvidence20261014/static-checks.json`에 원본 receipt·SHA가 있다. 이 인계에서 재검사하지 않았다.

Unity·네이티브 플레이어·실제 테스트 메서드 실행은 모두0. 렌더·영문 줄바꿈·원형/둥근 가장자리·저장/resume·게임 after 캡처는 단일 통합 검증에 이관된 **미완료 항목**이다. 모델 용량 오류로 남은 API 연결은 없다. 도구 거절이나 불확실한 변경 결과도 없다.

시안은 사용자 제시32개/iframe 이름32개, 개별 상호작용1개(Main PC KO), DOM 치수2개(Main PC KO·Imprint-Portrait-EN) 확인. 시안 캡처 Library ID `libfile_fd9fdb26ea908191a21eafb8af7b3cfd`. 게임 after 증거는 아직 없다. 기존 산출물 보존, 원격 push·공개 배포·main 병합·설치·권한 변경 없음.
