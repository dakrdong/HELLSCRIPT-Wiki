# 스킬별 사냥칙령 B1 — 개발 검증 창 (2026-10-24)

> **2026-10-28 A2 정정:** 큰 칙령 창·≡ 메뉴·전용 부품·스모크 설명은 과거 기록이다. [현재 진입 코드](../../Assets/HELLSCRIPT/Runtime/Presentation/GameUI.HuntEdict.cs)와 [대체 화면·소유 코드](Hunt_Edict_Window_Removal_20261028.md)를 따른다. 이전 캡처와 검사 수치는 당시 구현의 근거다.

[English](Skill_Tactics_Window_B1_20261024.en.md). **실제 플레이어 진입은 OFF이며 전투 연동은 구현하지 않았다.** 원문 U-E §9.4/9.5의 B1 UI·기존 B0/B2 저장 연결 범위다. 대표 외 부모 27종·보조 81종 데이터와 직업별 전투 반영은 기존 B3 후속 단위로 남는다.

## 기준과 진입

상위 담당이 지원 Claude UI에서 원래 시안의 Part A/B 포함 및 수정·승인 문맥을 확인했다고 전달했다. 사용자의 게임 직업 탭 제거, 스크롤을 만들지 않는 제목 오른쪽 아이콘 진입을 따른다. 새 미감 결정을 추가하지 않았고 재승인을 요청하지 않는다. 첨부 구 사양의 고정 하단 진입 행은 이후 승인된 **44px 제목 메달리온**으로 대체된 계약을 따른다. 기존 HTML 시안은 E 작업 폴더와 원래 브랜치에 보존하고 참조만 했다.

`SkillTreeD4Window.OpenDevelopmentTactics(parent, store, catalog, previewOrient, closed, initialSlot)`이 명시적 개발/검증 진입점이다. 일반 `SkillTreeD4Window.Open`에는 이 기능을 주입하지 않으며 게임 UI 호출부도 바꾸지 않았다. 새 화면은 `UNITY_EDITOR || DEVELOPMENT_BUILD`에서만 열 수 있고, 기존 `SkillTacticsCatalog.PlayerEntryEnabled=false`도 유지한다. 개발 API는 기존의 독립 검증용 `GameStore`를 전달해야 하며 보호된 사용자 테스트 프로젝트·세이브를 검증 입력으로 쓰지 않는다.

게임 직업 탭은 표시하지 않는다. 노드 선택은 기존 상세 확인이고, 포인트·보조 장착은 기존 트리가 소유한다. 일반 패시브·미작성 스킬·미작성 보조에는 빈 페이지나 버튼을 만들지 않는다. 미습득·미선택 보조·지원하지 않는 저장값은 기존 세션의 읽기 전용 이유를 표시한다. 자동 사용 Off여도 진입하고 다른 값을 편집할 수 있다. 진입 배지는 저장된 On/Off 또는 미리 보기 `i`이며 접근성 이름에 저장 요약을 포함한다.

## 공통 창과 데이터

`new_content_ui.py SkillTactics`로 공통 진입점을 만든 뒤 `ContentWindowView.Open(fit: ...)`와 `Adopt`로 연결했다. 공통 창이 캔버스·안전 영역·입력 순서·뒤로가기·중첩 일시정지를 소유한다. 하위 페이지의 외곽 좌표와 배율은 기존 D4 프레임과 일치시키며, 세로는 정보/본문 한 열, 가로·PC는 왼쪽 정보/오른쪽 본문이다. 헤더·뒤로·닫기·변경 표시와 저장/되돌리기·결과 영역은 고정한다.

색·폰트·버튼·패널은 `UiSkin.Casual`, `UiFonts`, `UiButton`, `StorageSurface`, `UiWindowFrame`을 사용한다. 스킬 그림은 `SkillIconView`를 재사용한다. 승인된 512 메달리온은 원본 바이트를 그대로 `Resources/Art/GlobalHUD/Casual/skill-edict-medallion.png`에 복사했으며 기존 512 단일 스프라이트 importer 설정과 새 GUID를 사용한다. 원본 그림과 `ResourceTextureBudget`는 수정하지 않았다.

| 표현 | 연결 |
| --- | --- |
| 토글 | 부모 `auto` 한 곳만 편집. Off도 다른 값 유지·편집 |
| 짧은 선택 | 2~3개 버튼과 색 외 `✓` 표시 |
| 긴/다수 선택 | 현재 값 행 → 공통 선택 창, 긴 문구 높이 계산 |
| 수치 | 기존 decimal 정규화·간격·양 끝 비활성, 단위·범위·기본값 안내 |
| 스킬 참조 | 실제 아이콘/이름, 습득·착용·자동 사용·필요 보조의 선택 불가 이유 |
| 조건/교체/읽기 전용 | 현재 `Visible` 행과 보조만 표시, 미적용 이유, 숨긴 값 보존, 전역 제한 안내 |

선택지·수치·기본값을 새 코드에 복제하지 않았다. 기존 카탈로그 20개 정의가 같은 창에 연결된다.

| 대표 | 표시 행 |
| --- | --- |
| W02 도약 내려찍기 / W02-3 연속 도약 | 부모 5 / 보조 적용 7 |
| A03 맹독 덫 / A03-3 교차 쇠뇌 | 부모 4 / 덫 3행을 쇠뇌 2행으로 교체하여 3 |
| M04 순간이동 / M04-3 원소 보호막 | 부모 5 / 보조 적용 7 |

화면에 HET 번호나 개발용 상태 선택 메뉴를 넣지 않는다. 보조에서 진입하면 해당 보조 행으로 바로 스크롤하고 미선택 보조를 구분한다. 전역 이동은 [SkillTreeD4Window.Tactics](../../Assets/HELLSCRIPT/Runtime/Presentation/SkillTreeD4Window.Tactics.cs)가 스킬 창을 닫고 `GameUI.ShowEdictTab("combat")`으로 전투 시트를 연다. 기본 추천 세팅 잠금은 `SkillTacticsEditSession.RefreshEligibility`와 저장 직전 재검사가 소유한다.

## 저장·복귀·수명

`SkillTacticsDevelopmentHost`/`SkillTacticsEditSession`의 독립 초안을 그대로 사용한다. 저장은 기존 `CommitSkillTactics` 원자적 거래이며 성공 후에만 저장 요약을 바꾼다. 실패는 초안·마지막 저장본·디스크를 유지한다. 되돌리기는 마지막 저장본이다. 지원하지 않는 보존값은 원문을 바꾸지 않고 읽기 전용으로 조회한다.

뒤로/전체 닫기/전역 이동은 저장 후 이동·변경 버리고 이동·계속 편집의 공통 결정을 거친다. 실패나 계속 편집에서는 부모 닫기·슬롯 저장·closed 콜백을 실행하지 않는다. 전체 닫기 결정 후에만 기존 부모 `Close`가 한 번 실행하여 Part A 슬롯 초안을 별도로 저장한다. 부모가 직접 파괴되면 자신이 연 하위 창·선택 창·전역 창/그 트리만 강제 정리하고, 초안을 저장하거나 닫기 콜백을 실행하지 않는다.

현 D4 어댑터의 상세는 고정 배치이며 별도 상세 ScrollRect나 사용자 조절 splitter가 없다. 그 구조를 바꾸지 않는다. 복귀는 같은 직업·선택/상세 노드·실제 트리 스크롤과 기존 프레임 분할을 유지한다. `SkillTacticsReturnState`에는 실제 상세 스크롤 0과 현재 고정 분할 비율을 기록한다. 기존에 없는 분할 조작을 새로 만들지 않았다.

저장·되돌리기·숫자·선택 변경은 이미 만든 컨트롤의 텍스트/상태만 갱신한다. 새 저장 subscriber는 추가하지 않아 무변경 자동 저장이 하위 페이지를 재구축하지 않는다. 언어·배치 재구축은 초안·본문 위치·가능한 기존 입력 focus를 유지하며, 선택/전역 child가 열려 있으면 배경 셸 재구축을 보류한다. 다른 창에서 같은 값을 바꾸면 기존 stale 검증으로 저장을 거절하고 초안을 유지한다. 자동 재기준화하지 않는다.

## 소유와 검증 상태

기존 파일 변경은 `SkillTreeD4Window.Panel.cs`의 제목 공간/아이콘 호출, 승인된 `SkillTreeD4Window.cs.Close()`의 첫 진입 한 줄, `en.txt` append뿐이다. 한국어는 공통 `Loc.T` 원문 키이며 영어 28개를 추가하고 기존 7개 키를 재사용했다. Core·카탈로그·전투·GameUI·Store host·J/K·기존 그림·패키지·프로젝트 설정은 변경하지 않았다.

`SkillTacticsWindowTests` 22사례를 작성했다. 대표 행/교체, 44px와 기존 패널/스크롤 유지, 일반 host 진입 OFF, Off/Undo/숫자/실제 참조 불가 이유, 미선택·미지원 미리 보기, 저장 실패/계속 편집, 독립 슬롯 저장과 한 번의 닫기, 부모 파괴 무저장, 전역 기존 창 연결, KO/EN의 440×956·956×440·1600×900·1280×800·1680×720 논리 배치·초안/스크롤 재구축을 검사한다.

이 에이전트는 **Unity·컴파일·Edit Mode/Play Mode·실제 CLI 리뷰·빌드를 실행하지 않았다.** 공통 UI Python 검사와 소스/지역화/메타/원본 해시 정적 확인만 수행한다. 실제 집중 실행·화면 렌더·긴 문구·안전 영역·입력 검증은 상위 단일 Unity 담당의 최종 후보에서 수행해야 한다. 성능 실측이나 최종 시각 통과를 주장하지 않는다.

기준 커밋: `e1c1434a843a552c2c0d72a1100234ac9045c998` (main `1f1887d1148d99e5b5f42f9a899d8945c6209259` 포함). 로컬 근거는 `Builds/Validation/SkillTacticsB120261024/`에 모으며, 부모의 집중 필터는 우선 `Hellscript.Tests.SkillTacticsWindowTests`, 연관 회귀는 `SkillTacticsTests`와 `SkillTreeD4SlotWindowTests`다.

## 통합 후보의 실제 검사 (2026-10-25)

원 인계 이후 단일 Unity 담당이 실제 컴파일과 집중 검사를 실행했다. Unity 6000.6의 TextGenerator 명시적 IDisposable 해제를 사용하고, 개발 미리 보기의 부모 파괴에서도 공통 창 등록과 소유한 하위 창을 정리하도록 수명을 보완했다. 테스트마다 닫기 횟수 fixture도 초기화했다. takeover/e-npc-combined/unit3/results.xml에서 B1 22/22가 통과했다. 같은 통합 상태의 기존 칙령·트리·입력·HUD·전투 연관 검사도 integrated-streaming-impact/run1의 일반 394개에 포함돼 통과했다. 그 실행에 잘못 선택된 별도 측정 2건은 환경변수 누락 실패로 보존하고, 별도 구성된 두 경로의 측정으로 분리했다.

개발용 RuntimeSkillTacticsB1Acceptance는 새 /tmp/hellscript-tactics-b1-* 아래의 비어 있는 save/evidence, 명시적 offline QA 및 disabled 서버 설정에서만 시작한다. 실제 창·Store·raycast를 사용한 합성 흐름과 마지막 외부 포인터 저장을 구분해 기록한다. 실제 렌더·포인터 단계는 아직 완료하지 않았으며 검사 도구의 컴파일을 시각 통과로 보지 않는다.
