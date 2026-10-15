# 캐주얼 UI 단일 통합 검증 — 2026-10-14

[English](Casual_UI_Integration_20261014.en.md) · [공통 규칙](Casual_UI_Skin_Rules_20261014.md) · [공통 API](Casual_UI_Parallel_Handoff_20261014.md)

## 최종15화면 통합 입력 고정

캐릭터 PR2 `b87b581b` → 공통 `b0d4da6b` → HUD `4279440f` → preflight `f786f69e` 순서로 통합했다. 기존13개 UI·캐릭터 생성/선택·기본 HUD의 소스 인계는 완료됐으며, 아래 실행 결과와 화면 수용 상태는 별도로 구분한다.

HUD 동결 `4279440ffe823fc4e82a0a81fc4c6f94c6410d44`를 기존 task6 통합 브랜치에 로컬 fetch/fast-forward했다. 전달158파일의 SHA는 모두 일치하고 별도 저장소·Library·빌드 사본은 만들지 않았다. 전체 EditMode가 읽는 누락된 저장소 fixture15파일과 HUD 근거 폴더만 sparse checkout에 추가했다. 기존45필터 준비 목록은 최종 전체 회귀를 제한하지 않는다.

실행 전 리뷰에서 확인한 두 표시 결함을 수정했다. 모든 상태 효과의 스킬은 frame-status+사각을 쓰며, DPS의 궁극기 사용 표식은 frame-seal+원형을 쓴다. 후자는 `TrainingChartView.SetCasualHudAppearance()`가 풀 재사용과 재배치 때 실제 스킬 정의를 보고 적용하므로 매초 하위 아이콘을 검색해 덮는 경로를 제거했다. 실제 소비자 상태/일반/궁극기와 재사용 역할 회귀2개를 추가했다.

`RuntimeGlobalHudSmoke`는 폐기용 계정에 마법사를 실제 생성하고 튜토리얼 소유를 전환한다. GlobalHud/DamageGraph 스모크의 fixture만 legacyExempt로 준비해 필수 튜토리얼 대신 검증 대상 일반 균열로 진입한다. 게임의 캐릭터/튜토리얼 정책과 실제 세이브는 변경하지 않는다. 이 후속 커밋 `f786f69e`를 전체 EditMode의 실행 입력으로 고정했다. 실제 결과와 후속 재검증 상태는 아래에 기록한다.

저장 PR1 `e3809624`와 후속 `93219e3f`도 로컬 병합했다. 통합 소스는 D4 생성 탭·남녀 저장 소유·누름 중 영웅 전환·6슬롯 선택 guard·CharacterPlaza 예산을 연결한다. 초기 CLI 시도의 editor-path 형식 오류와 Unity 시작0 기록은 과거 이력이며, 이후 완료된 전체 EditMode 실행을 대신하는 현재 상태가 아니다.

13개 담당의 동결 소스를 `codex/casual-ui-validation-20261014`에 통합했다. 공통 기준은 `0ccba80e`이며 원본 담당 폴더와 main은 보존했다. 소스 통합과 실행 수용은 구분한다. 아래 소스 표는 원래13개 UI의 통합 이력이다. 캐릭터 화면의 승인된 영웅6명 저장(schema23)과 기본 HUD까지 합친 뒤 최종 전체 EditMode를 한 번 수행했다. 화면 검증은 두 화면비 중심을 유지한다. main 병합·웹 빌드·공개 배포·원격 push는 포함하지 않는다.

## 통합 입력

| 담당 | 화면 | 최종 소스 |
| --- | --- | --- |
| 8 | HUD·전투 설정 | `4842b667` + 동결 미커밋 패치 |
| 9 | 몬스터 도감 | `3dc7ec6b` |
| 10 | 균열 장비 추천 | `ca117bcd` |
| 11 | 물약·전선 이탈 | `89abe736` |
| 12 | 훈련장 | `d92c9cc6` |
| 13 | 상점·인벤토리 | `9b8ea779` |
| 14 | 대장간 | `efbf32ad` |
| 15 | 보석공 | `36418db4` |
| 16 | 위상석 | `e79e825d` |
| 17 | 룬 보드 | `7f8eb2fb` |
| 18 | 균열 입장 | `eeaa2111` |
| 19 | 균열 승리 | `53a34b3d` |
| 20 | 일일 퀘스트 | `93d32adf` |

담당 12/13은 공통 패치의 동일성을 확인하고 소유 파일 커밋만 순서대로 적용했다. 나머지는 merge했다. 담당 8의 동결 미커밋 8파일은 원본을 건드리지 않고 패치·SHA 영수증과 함께 `10ce5443`에 보존했다. 초상은 `57bba034`의 전체 tree에서 35 PNG와 meta를 인수했다. 기존 33종만으로는 N23/B06가 빠진다. 승인 장비 359 PNG는 공통 provider를 재사용하며 추가 복제하지 않는다.

## 통합 보완

- 훈련 HUD·일시정지·결과 차트·스킬 카드에 인스턴스 스킨을 전달한다. 기존 위치·크기·게임 규칙은 보존한다.
- 개발 플레이어의 `-hellscriptCasualUiFocused`는 큰 반복 목록을 440×956·2868×1320의 KO/EN으로 제한한다. 기본 검사 목록과 거래·보상·장착·저장/재시작 검사는 유지한다.
- 2868×1320에서 DPR3 가정 안전 영역(왼쪽/오른쪽 102, 아래54, 위0)을 기록한다. Mac 모의 화면이며 실제 iOS 검증이 아니다.
- 오래된 테스트 fixture를 현재 D4 학습·티어 계약에 맞춘다. 판정 기준이나 게임 계약을 완화하지 않는다.
- 원화 검사는 legacy fallback을 배제하고 승인 provider의 직접 경로 및 실제 리소스 identity를 비교한다. 결과문 조합 수는 실제 실행 수를 사용한다.
- `CasualImportedAssetTests` 7개는 장비359·초상35·입장43·위상1·훈련장 배경의 실제 Unity import를 확인한다. 원본 SHA 감사와 역할이 다르다.

## 실행 계획과 현황

기존 `skill-skin/Library` 한 개와 `Builds/Validation/CasualSkillSkin/HELLSCRIPT.app` 한 개를 재사용한다. 각 프로세스는 새 절대 save 경로와 음소거 오디오 설정을 사용하고 smoke 진입점은 하나만 켠다. 재시작 검사는 해당 검증용 save만 재사용한다.

전체 EditMode **1회 종료: 총5,890건, 통과5,869·실패18·건너뜀3**. 프로세스 경과 시간은 **7,319.615초**다. 실제 근거는 `Builds/Validation/CasualSkillSkin/integration-ready/final15/editmode.xml`과 `editmode-process.json`이다.

집중 검증 이력: `focused-1` 41건 중 통과38·실패3 (459.250초), `focused-2` 11건 중 통과11·실패0 (221.981초). `focused-1`은 실제 실패 메서드·누락된 CasualEdict10·import3만, 후속 실행은 남은 실패 범위만 검사했다. 전체 검사는 반복하지 않았다. 첫 통합 네이티브 빌드1회 성공 뒤29개 작업을 실행했다(통과9·실패20). 선행 실패에 종속된 재시작5개는 미실행이며 시각 수용은 진행 중이다. 수정 묶음 뒤 같은 앱 경로의 교정 빌드1회가 승인됐다. 수정 전후 결과는 분리한다.

기존 정적 UI 계약15/15, 갱신16바인딩/199파일, diff 통과와 4,764개 meta 중 중복0 GUID 감사는 당시 정적 근거다. 원래 스킬 창 파일럿 결과를 이번 통합 실행 결과로 보고하지 않는다.

과거 참고: 다른 프로젝트의 외부 전체 검사는 5,752건/통과5,748/실패1/건너뜀3, 약90분이었고 오래된 번역 키 1건이 실패했다. 외부 별도40건은08:24:29Z에 통과했고 baseline 전체 검사는08:34Z 당시 진행 중이었다. 이는 위 통합 결과와 별개인 과거 관찰이며 현재 실행·잠금·자원 상태를 뜻하지 않는다. 외부 프로세스·설정은 변경하지 않는다.

실행 근거는 `Builds/Validation/CasualSkillSkin/integration-ready/`의 소유자 인계·최소 검증 계획·자원 상태·결과 영수증에 기록한다. 기다림·import/compile·test·build·capture 시간은 각각 기록하며 실패 범위만 다시 검증한다.

## 남은 시안 수용 항목

실제 화면에서 모서리·KO/EN 줄바꿈·폰트 대비·입력·스크롤을 확인해야 한다. 담당이 남긴 하위 팝업/도움말/결과 상세·비장비 원화 연결 차이는 각 인계의 미완료 항목이며 소스 통합만으로 완료 처리하지 않는다. 위상석 5열 또는 글자 축소 예외는 승인되지 않았으며 적용하지 않는다.

산출물은 task-6 `artifact-lifecycle.json`에 경로·소유자·크기·보존 사유를 기록한다. 기존 담당 캐시/원화/미커밋 문서를 삭제하거나 덮지 않았으며 추가 프로젝트·캐시 사본은0개다.

## 6슬롯 공통 호환 후속

과거 소스 검증 기록: Runtime/Editor/Tests 정적 컴파일 오류0(경고89/28/43), UI 계약15/15(28.194초), refresh16/199, diff 통과. 포인터 검사의 미확인 공지로 인한 거짓 통과 가능성을 보완한 뒤 Tests만 다시 컴파일했다. 남녀 저장/실패 보존, 생성 전후 탭, 소유자 전환 중 누르기, 정상 효과, run ID 격리,6개 레벨 캐시 회귀도 이 단계에서 추가했다. 당시 정적 검증만 마친 상태였으며 이후 PR2의 아트41개 인수와 HUD 통합을 완료했다. 현재 Unity 판정은 위 전체 결과와 집중 재검증 기록을 따른다. 과거 정적 영수증: `Builds/Validation/CasualSkillSkin/dotnet/hero-slots-followup/receipt.json`.

## 전체 실행 후 확인한 수리

번들 Jua의 실제 cmap을 사용해 −·×·—·◆를 본문 글꼴로 보내며 TTF 원본은 보존했다. Casual HUD 재임포트가 512 상한과 패널 nine-slice 경계를 유지하도록 고쳤고 실제 재임포트2건이 이를 검사한다. Unity null 비교6곳과 Graphic의 CanvasRenderer 요구2곳을 보완했다. 보호막은 계산한 동일 경계를 재사용하며 숏컷 검사는 6칸과 실제 간격을 함께 검사한다. 가로 보스 도감의 영문 설명 공간을31→34, 카드 높이를82→85로 확보했다.

계정 테스트는 미생성 영웅을 정상 API로 생성하고 schema22 fixture의 공개 상태를 snapshot 전에 정합화한다. 기존 원본 wire·영웅·guide 보존 단언은 유지한다. 무효 GUID로 빠졌던 CasualEdict10개를 복구했으며 닫기 테스트 인스턴스만 EditMode 콜백을 활성화한다. 보스 착지 검사는 같은48 arena·384후보·300초 제한·단언을 유지하면서 arena별 fixture를 재사용하고 구간별 시간을 기록한다.

개발 플레이어의 폐기용 fixture만 현행 생성·튜토리얼 흐름에 맞췄다. HUD 위치 저장 실패 안내가 전투 중에도 보이도록 하고, opt-in 수용 검사에 네 창의 독립 접기·드래그·저장 실패·재시작 복원 및6인장·부족◆·사망·저체력·전체 상태 스크롤을 연결했다. 초기 네이티브 판정은 아래에 보존하며, 교정 빌드의 수용 검사는 아직 미실행이다.

현재까지 관찰한 최신 결과를 사례 이름으로 합치면 **통과5,899·실패0·건너뜀3**이다. 건너뜀은 작성용 `PuzzleContentTests.Probe`, `PuzzleOperationTests.ProbeOperations`와 웹 측정 환경이 필요한 `WebGlOptimizationMeasurement.Capture`다. 보스 재검증은48/48 arena·384/384후보·실제공격336회를 완료했다(Generate11.213초/Fixture17.144초/Restore167.229초/Attack3.168초). 전체 실행의18실패와 집중 실행 이력은 삭제하지 않았다. 최신 정적 UI 계약15/15(25.112초), refresh16바인딩/202파일, source·전체 diff 검사는 통과했다.

## 첫 네이티브 검사와 교정 묶음

입력 `56df1d09b5c83082fa857fcd412f211630fb344e`의 macOS Development 빌드는68.319초/exit0/`HELLSCRIPT_BUILD Succeeded errors=0`, 보고 크기1,235,949,588B다. 검사 전후 bundle SHA256은 `5f7d14965c74eb2b6247d80b1e098eb75cb041033771da3a73f4530e06ca8724`로 같았다. `final15/build-process.json`, `native/build-freeze.json`, `native/final-build-verification.json`을 보존했다. 빌드 후 Crash Reporting 심볼 업로드 인증 경로의 빈 URI 경고1건은 게임 예외와 구분하며 숨기지 않는다.

초기29개 프로세스 경과 합계464.121초, 통과9·실패20·선행 조건 미충족 미실행5개다. 실패 작업: `aspect, battle-order, battle-position, battle-target, blacksmith, character, codex, hud-battle-layout, hud-dps-drag, hud-global, hud-rift-activity, hud-rift-dps, inventory, jeweler, recommended-controls, rift-result-actions, rift-victory, rune, shop-layout, shop-transactions`. 각 `native/<job>/receipt.json`은 당시 입력·로그·캡처·종료코드를 보존한다. 자동 통과를 시각 수용으로 간주하지 않는다.

수정 대상은 칙령 위치 본문 초과·대상 라벨 겹침·순서 저장 후 실행 배지, 전투 미니맵/마을 칙령의 메뉴 입력 가림, 긴 HUD 제목 높이다. 검사 fixture는 새 캐릭터 입장 시간, 시작 후 소모되는 CreatedAccount 대신 씬 시작 전 새 save 증명, clone의 RESUME 이벤트 제외와 기존 RestoreFidelity 저장형 비교, 장비 공통 분리 본문/반응형 탭/스킨 색, 해금 조건·합법D4배분·안내/출석 창 준비를 현재 계약에 맞춘다. 실제 거래·저장·전투·입력 단언은 유지한다. 초기 통과 화면의 실제 PNG에서 찾은 시각 결함도 교정 대상이며 최종 상태는 후속 기록을 따른다.

첫 빌드가 남긴 공백뿐인 meta465개와 검토된 IDE/Unity 설정4개는 정확한 patch·해시를 `post-build-source-cleanup.json`에 보존한 뒤 원복했다. 원본 아트/폰트/기존 앱/사용자 save는 변경하지 않았다. 원래 전체 EditMode는 반복하지 않는다. 같은 Library와 앱 경로에 수정 후 네이티브 빌드1회를 묶고 실패·영향 범위를 재검증한다. 수정 전 앱의 결과를 수정 후 통과로 바꾸지 않는다.

테스트 이름 기준5,899통과의 산식은5,890−3제외+누락10+신규2다. 신규2개는 `BuildResourceTests.CasualHudPanelsKeepAuthoredBordersAcrossReimport("panel-cream",26)`과 `("panel-white",40)`이며 같은 검사를 중복 집계한 값이 아니다.


## 교정 묶음 사전 검사와 퍼즐 제거 이력

초기 앱의 독립 화면/픽셀 검토 뒤 제품 배치·영문 초 표기와 검증 fixture를 한 묶음으로 수정했다. 정적 Runtime/Editor/Tests 컴파일 오류0, 변경 파일 경고0, UI 계약15/15(22.77초), refresh16바인딩, diff 검사 통과. focused-3은 변경과 연결된7 fixture만 실행해 **89/89 통과**, 프로세스62.418초/테스트14.198초다. 기존 전체 검사는 반복하지 않았다. 이89건은 기존 이름의 재검증이며 통과 건수에 중복 추가하지 않는다. 교정 플레이어 결과는 이후 별도 영수증으로 기록한다.

구 퍼즐 튜토리얼은 현재 브랜치에서 **삭제 완료가 아니다**. 로컬 Git의 제거66c2e59b→은퇴 기록f0d39b1f→PR #132 병합bbe131de는 로컬 origin/main 계보에 있으나 통합 기준56df1d09의 조상이 아니다(공통 분기점53d6a3e1). 제거 계보가 아직 통합되지 않은 상태이며 제거 결정을 UI 작업에서 되돌렸다고 해석하지 않는다. 전체 XML의 퍼즐7클래스+TutorialPitTests는299건(통과297/작성용제외2), 실제 보스·3웨이브 졸업6건도 실행됐다. 초기 앱 크기 보고서에는 퍼즐 packed asset31개/29,223,340바이트가 남아 있다. 일반 보스 leap/burrow 회귀와 새 계정·일반 온보딩은 구 퍼즐 제거 대상과 구분한다. 현재 작업에서는 범위를 넓혀 삭제하거나 테스트만 제외하지 않는다. **부모의 머지 후 실제 main HEAD에서 제거 커밋 계보, 런타임 진입·복습·전용 검사·전용 리소스/빌드 등록의 제거를 재확인해야 완료**다. 최신 원격 main 및 머지 후 확인은 미실행이며 push/main 병합을 하지 않았다.

## 교정 앱 결과와 후속 동결 준비

입력 `75f2d5e5e79a3832a12f8a66351c8896dbe04a27`의 교정 macOS Development 빌드는67.999초/exit0으로 성공했다. 동일 앱의 bundle SHA256 `a37e91aebcb85624e505b20672e68c8d3095f26f900770d36d737febc0e7f8c4`를 전후 확인했다. 실행30개 중 통과15·실패15, 선행 실패로 재시작4개 미실행이며 프로세스 경과 합계669.818초다. `final15/corrective/build-process.json`과 `corrective/native/<job>/receipt.json`, `final-build-verification.json`이 정본이다. 이 결과를 후속 수정 소스의 통과로 바꾸지 않는다.

제품 수정은 회전 시 HUD 경계를 먼저 확정한 뒤 칙령 카메라를 배치하는 순서, 상단 칙령을 전체 하단 여백으로 계산하던 추천 카드, 작은 화면의 상태 시간·중첩 숫자 한 줄 높이, 미니맵 설명 아래 여백, 최종 폰트 적용 후 반복 바로가기 폭 측정, 입구 닫기 글리프와 같은 영웅 재선택 시 직업 focus 복원이다. 검증 fixture는 안내·출석·해금 조건과 실제 HoldPress 입력, 공통 장비 카드의 분리 본문에 맞췄다. 기존 거래·저장·전투·입력 단언과 원화/폰트/GUID는 유지한다. 캐릭터 마지막 클릭의 원인은 아직 확정하지 않았으며 실제 raycast 진단과 렌더 준비 후 클릭으로 재검증한다.

룬의 옛 빈 칸 검정 비율은 어두운 배경을 전제로 했다. 동일 보드·좌표·crop의 dark-control에 기존 판정을 모두 유지하고, 승인 Casual well의 점유 칸 RGB8 불변·기존 절대/밝은 글리프 기준·dim/empty 색 분리 비퇴행을 추가한다. 새 임계값이나 현재 실패 이미지를 golden으로 만들지 않았다. 대조 캡처 후 well은 finally에서 원복한다. 이 native 판정은 새 앱에서 실행하기 전까지 미검증이다.

`focused-4`는 변경에 연결된6 fixture만 실행해 **100/100 통과**, 프로세스54.796초/테스트10.002초다. 신규 `CharacterSelectionTests.ChoosingTheSelectedHeroRestoresItsClassAfterCrestBrowsingWithoutSaving` 1건을 포함한다. 정확한 테스트 이름으로 과거 관찰을 합치면 **통과5,900·실패0·건너뜀3**이며 마지막 소스의 전체 검사를 다시 실행한 뜻은 아니다. 집중 검사 이후 추가된 룬 smoke 전용 판정은 이6 fixture의 대상 밖이다. 전체 EditMode는 반복하지 않았다.

후속 native 범위는 이전 실패15·미실행 재시작4와 변경 영향을 받는 입구/재시작, HUD scale, 네 창 drag/재시작, 상점 반응형 배치6개로 총25개다. 직접 의존 변경이 없는 기존 통과9개는 당시 소스·앱 영수증으로 유지한다. 최종 소스 동결 뒤 기존 Library와 같은 앱 경로에서 필요한 빌드를 한 묶음으로 수행하며, 새 결과·시각 수용과 main의 UI/퍼즐 제거 확인은 별도로 남긴다.

최종 묶음 정적 Runtime/Editor/Tests 컴파일 오류0(기존 경고90/28/43), UI 계약15/15(22.29초), refresh16바인딩, diff 통과. 앞선 정적1차의 인벤토리 smoke 인자 누락1건은 수정 후 재검증했다. 영수증은 task23 `validation/native-followup-static-3/receipt.json`이다.


## 후속 앱 결과와 마지막 수용 수정 묶음

`412c54fbd7e4e193704965e4cb29becd8509a58d` 입력의 후속 빌드는 70.965초/exit0으로 성공했다. 동일 앱의 검사 전후 bundle SHA256은 `6d3289e7e8e9b064d57b062ecd9b7cc9485ef40bde70c08d5a97a664631740d3`다. 선택25개 중 24개 실행·16통과·8실패·선행 실패에 종속된 재시작1개 미실행, 프로세스 합계703.986초다. `final15/followup/native`의 원본 영수증과 PNG는 보존한다. 별도 정본 룬 fixture 검사는 10개 구간·쌍대 픽셀 판정 모두 통과했지만 영문 단위 `/초`가 남아 전체 실패했다. 이 결과를 통과로 합산하지 않는다.

최종 수정 묶음은 인벤토리 검토 거래 후 새 아이템 참조를 상세/비교 카드에 연결하고, 룬 값의 단위를 기존 번역 키로 표시한다. 칙령 rail은 정해진 글꼴·hit row를 유지하면서 긴 caption의 줄 높이를 측정한다. HUD 접기 버튼은44pt hit을 유지하며30/26/22pt 원형 face를 중앙에 배치한다. 테스트는 실제 WorldView 체력바, 현재 갬블 해금 조건, 표시 준비가 끝난 캐릭터 시작 버튼, 현재 결과창 구분선 구조를 검사한다. 추천 설정은 기존 전체 종료 흐름을 보존하고 별도 live-run checkpoint를 만들어 실제 재시작 검사를 연결한다. 판정 완화나 실제 세이브 접근은 없다.

룬 clarity의 정본은 `Assets/HELLSCRIPT/Tests/Fixtures/RuneNativeClarity.json`(SHA256 `affacf7fb7f15257a45aa773bbf67f2718e7a6bf065ad5bd0b8e820774bd3477`)이다. 이전 통합 fixture와 clarity crop 좌표/해금 상태가 다르므로 둘을 혼용하지 않는다. 승인 Casual well과 같은 보드의 dark control 비교·기존 절대 픽셀 기준은 유지한다.

`focused-5`는 새 인벤토리2·룬 번역2·원형 버튼1 회귀와 버튼/Store binding의 관련 범위만 실행해 **40/40 통과**했다(48.726초 프로세스/7.913초 테스트). 기존 전체 실행과 집중 실행을 정확한 testcase 이름으로 합친 관찰은 **5,905통과·실패0·건너뜀3**이며 최종 SHA 전체 검사를 다시 한 숫자가 아니다. 전체 EditMode는 여전히1회다. 이후 caption guard의 빈 기본 라벨 선택만 바로잡아 정적 컴파일을 다시 확인한다.

마지막 수용은 기존 Library·동일 앱 경로를 재사용하는 공통 빌드와 실패/영향 범위의 실제 네이티브 입력·KO/EN·회전·재시작 검사를 실행한 뒤 별도 기록한다. 이 단락의 수정은 아직 새 앱 실행 통과를 뜻하지 않는다. 기존13화면·6슬롯 생성/선택·HUD의 통합 수용과 실제 main의 UI/구 퍼즐 은퇴 계보 확인은 별개이며, 후자의 부모 병합 후 확인은 계속 대기다.

## 수용 앱 결과와 글자 선명도 보완

`d2206cd3f60458620f0440a04cb5d77784c4be02`의 빌드는68.276초/exit0으로 성공했다. 동일 앱의 전후 bundle SHA256은 `5aaf2e0590dd269152816476cbfbd71d4e327521b183e64b3881ceaf633e6f0d`다. 선택19개 모두 실행해17통과·2실패, 미실행0이며 프로세스 합계662.097초다. 정본은 `final15/acceptance/native`다. 대상 변경 검사는 실제 스크롤을 생략해 가려진 버튼을 눌렀고, 상점 검사는 전용 원화가 없는 B03의 정상 공통 atlas를 거부했다. 실제 wheel 스크롤·전체 행 노출 뒤 원래 raycast 입력을 사용하고, atlas 경로에서도 구매 ID·원화/UV·거래·저장 단언을 유지하도록 검증 fixture를 고쳤다.

실제 PNG 검토에서 캐릭터 작은 글자는 낮은 부모 Canvas 배율로 생성한 뒤 확대되어 흐렸고, 룬 영문 미니맵 이름과 어두운 보드의 지역 이름은 가독성 보완이 필요했다. 캐릭터 페이지가 공통 Canvas 생성 helper를 통해 독립 배율을 소유하도록 하여 글꼴·글자크기·색·화면 좌표·44px 입력 경계를 보존한다. 일반 창은 기존 생성·스택·입력 동작을 유지한다. 룬 미니맵은 같은 글자크기로 줄바꿈하고 원래 입력 경계 안에1물리픽셀 안쪽 여백을 확보한다. 지역 이름에는 승인 시안의 밝은 비입력 label 배경을 연결한다. 원화/폰트/meta/GUID 추가·변경은 없다.

배율 진단 후보 focused-6은3실패로 기각했으며 해당 nested Canvas 방식은 제품에 적용하지 않았다. focused-7은 연결 중단 후 프로세스와 XML이 없어 `interrupted-incomplete`로 별도 보존한다. 로그만으로 통과를 선언하지 않았다. 정상 종료한 focused-8은48건 중44통과·4실패(45.679초): 캐릭터 배율3과 공통 UI/Store39는 통과했고 룬 글리프 경계4건만 실패했다. 룬 여백 보완 후 해당 메서드의 서로 다른5배율만 실행한 focused-9는5/5통과(35.007초/테스트0.429초)다. 전체 EditMode를 반복하지 않았다. 이 결과는 최종 SHA 전수 재검사를 뜻하지 않는다.

최종 정적 Runtime/Editor/Tests 오류0(기존 경고90/28/43), UI 계약15/15(22.59초), refresh16 및 diff 검사가 통과했다(`task23/validation/visual-repair-static-3/receipt.json`). 공통 Canvas 추출·소유권·기본 동작과 네이티브7개 선택은 독립 코드 검토를 마쳤다. 같은 Library·앱 경로에 한 번의 수정 빌드를 만들고 대상/상점/캐릭터/룬/인벤토리/승리/승리 재시작을 검증한다. 여기까지의 소스·정적·EditMode 기록은 새 플레이어 통과를 뜻하지 않는다. 실제 main에서 UI와 구 퍼즐 은퇴 계보를 확인하는 부모 후속은 계속 대기다.

## 선명도 수리 앱과 남은 네이티브 경계 보완

`06d105ffe6134e6f40846d8f816703e3452ef0c4` 빌드는62.584초/exit0 성공이며 정적 입력1,189개 해시가 전후 일치한다. bundle SHA256 `9313f1d6e55c848ed6df47eafcba6b145c0ba1908ff02a7dcd30ba16393579a8`도 검사 전후 동일하다. 선택7개는4통과·3실패·미실행0, 합계189.695초다. 캐릭터·상점 거래·승리·승리 재시작은 통과했고 대상·룬·인벤토리는 실패했다. `final15/visual-repair/native` 원본을 보존한다. 캐릭터26PNG 해시와 대표5장 직접 비교에서 작은 글자 흐림 해소를 확인했으며 장비 팝업의 본문 픽셀205,275개는 이전 앱과 동일했다. 정적 폰트·크기·색 및 실제 입력 단언과 시각 확인 범위는 task23의 별도 검토에 구분했다.

대상창의 `30 seconds`는 고정50u endpoint 폭이 영문을 담지 못했다. 통합 소유자가 `GameUI.EdictTarget.TargetNumber` 한 메서드의 두 endpoint만 기존 슬라이더 아래 여유 lane의 좌우 절반에 배치하는 최소 예외를 적용한다. 공통 EdictText 후처리로 다른 보호 화면까지 영향을 넓히지 않으며 원래 높이14u·글꼴·크기·정렬 끝점·문구·범위·저장 경로와 잘림 단언을 유지한다. 원래 보호 의도와 이번 수용 단계의 명시적 통합 수리 범위를 구분한다.

룬은 개별 글리프가 버튼 안에 맞아도 외곽 버튼 자체가 미니맵 패널 밖으로 나갔다. 종전 고정240 source-unit 여백을 실제 기존 반지름과1물리픽셀 여백으로 바꾸어7개 중심·버튼을 같은 패널에 맞춘다. hit 크기·글자크기·색·wrap 및 단언을 유지하며,5배율 검사를 KO/EN·양방향7컨트롤과 실제 패널 글리프 경계까지 보강한다.

인벤토리 cancel 콜백과 review plan은 직접 저장/소비하지 않지만, 검사 snapshot 사이 캡처 동안 활성 GameController가3초 자동저장을 수행하면 전체 계정의 lastSeenUtc/Revision이 바뀔 수 있다. 당시 before/after dump가 없어 과거 차이 필드를 확정하지는 않는다. 해당 selection/review/cancel 구간만 controller Update를 멈추고 finally 원래 상태를 복구한다. EventSystem·UI 입력과 원래 전체 JSON equality를 유지하며 Revision 불변 및 실패 시 양쪽 snapshot 저장도 추가한다. 제품 cancel 코드는 바꾸지 않는다.

새 수정4소스는 실제 Unity 참조 Runtime/Editor/Tests 오류0, UI 계약15/15(21.87초), refresh16·diff 통과다(`task23/validation/remaining-native-static-2/receipt.json`). 첫 정적검사의 테스트 내부 helper 직접참조1오류는 비공개 reflection 검사로 바로잡았다. 네이티브 통과는 다음 입력 빌드에서 별도 확인하며 전체 EditMode는 반복하지 않는다.

focused-10은 보강된 동일5개 이름을5/5통과했다(35.343초). 신규 testcase 증분은0이며 최신 이름별 관찰5,913통과·실패0·건너뜀3을 유지한다.

## 최종 통합 검증 인계

최종 플레이어 입력은 `49c08e20126179bc6d385e1bde2df653c29197ad`다. 기존 Library와 `Builds/Validation/CasualSkillSkin/HELLSCRIPT.app` 경로에서61.653초/exit0으로 빌드했다. 정적 입력1,189개가 빌드 전후 일치하며 bundle SHA256 `6ccb286b046cbc260330f2d0724e863aab5f13db39973767197c393f433e25d1`은3개 검사 전후 동일하다. 대상·룬·인벤토리3/3통과, 미실행0, 프로세스 합계124.346초다. 원본은 `final15/remaining-repair/native`의 각 receipt·PNG·`final-build-verification.json`이다. 이전 실패 영수증은 덮어쓰지 않았다.

35개 검증 항목의 최신 실제 관찰은35통과·실패0·선행 실패 미실행0이다. 이번 최종 앱에서 실행한3개와, 변하지 않은 소비자에 대해 이전 입력에서 통과한32개를 구분한다. 소스 SHA·번들 SHA·영수증 해시별 정본은 task23 `integration-acceptance-final.json`, `remaining-repair-native-selection.json`이다. 마지막 SHA에서35개를 전부 다시 실행했다는 뜻이 아니다. 캐릭터 선명도는 직전 앱의26PNG/대표5장, 최종 대상 숫자 라벨과 룬 영문은 최종 앱의 직접 픽셀 검토로 수용한다. 룬은10구간·paired clarity 및 실제 패널 경계 단언을 통과했고, 인벤토리 전체계정·Revision 취소 불변/입력/거래/저장 검사를 통과했다.

전체 EditMode는1회(원래5,890:5,869통과·18실패·3건너뜀)이며 필요한 집중검사로 실패를 해소했다. 정확한 이름별 최신 관찰은5,913통과·실패0·건너뜀3이다. 최종 입력 전수 재실행을 뜻하지 않으며 focused-6 기각 진단/7 연결 중단과 과거 실패는 별도 보존한다. 정적 Runtime/Editor/Tests 오류0, UI 계약15/15, refresh16, diff 통과다. 네이티브는 초기1회와 승인된 필요 교정5회, 총6번의 공통 빌드로 동일 앱 경로·Library를 재사용했다(프로세스 합계399.796초). 개별 화면·웹 빌드나 프로젝트/Library 복제는0이다.

빌드가 남긴 공백 meta465개와 검토된 설정4개는 원본 patch·해시 보존 뒤 되돌렸고 소스 clean 상태를 확인했다. Editor UDS 해제 진단과 빈 symbol-upload URI는 숨기지 않고 원본 로그 및 task23 분류 JSON에 남겼으며 제품 플레이어의 예외/폰트/단언 판정과 구분한다. 실제 사용자 save 접근·원본 원화/폰트/GUID 변경·영구 삭제·추가 결제/자동충전·push/main 병합은0이다. 무음·offline 폐기용 fixture와 macOS Development의 합성 입력 검증이며 물리 모바일 검증을 주장하지 않는다.

이 통합 UI 수용 작업의 실행 장애는 해소됐고 알려진 실패는 남아 있지 않다. **부모가 실제 main에 병합한 뒤 UI 및 구 퍼즐 은퇴 계보·진입/복습·전용 검사·리소스/빌드 등록을 확인하는 후속은 계속 대기**이며 이번 스레드가 중복 수행하거나 완료로 바꾸지 않았다. 온라인 시안 로드는 이전 인계에서 timeout으로 미확인이고 로컬 승인 참조/실제 PNG 검토를 사용했다. 실제 산출물 크기와 보존 사유는 task6 `artifact-lifecycle.json`에 남긴다.

## 로컬 main 병합과 퍼즐 은퇴 확인 완료

사용자의 ‘웹 빌드 취소·main 병합까지만’ 승인과 병합 후 퍼즐 제거 확인 요청에 따라 로컬 main에 통합했다. 초기 로컬 main `a30605bc3aad6227f93ad92124cb4777a758db20`은 뒤처져 있었고, 읽기 전용 원격 조회에서 최신 `1f56f76e6767d9f5c42ff83e39debe83296a246e`를 확인했다. 로컬 main을 그 기준으로 fast-forward한 뒤 UI `cdc9f31331a16084d7f669d4387d5f82f8512851`을 합친 **병합 커밋은 `1fe680cc5461fe36050fabf96ac757467427c43c`**다. main 체크아웃은 기존 task6/skill-skin을 재사용했다. 별도 던전 담당은 완료·idle 상태였고, 기본 프로젝트의 다른 pilot 브랜치·미커밋 문서2개의 해시·미추적 에셋/문서 상태는 전후 동일하게 보존했다.

충돌12곳 중 은퇴한 퍼즐 코드/테스트4개는 main의 삭제를 유지했다. 나머지는 새 UI·6슬롯과 최신 도감 원화/제목 비행/밝은 필드를 함께 보존했다. GameStore schema23·정확 원문 archive·created/gender와 main의 Guide run 소유권 정합화를 합쳤으며, 공통 창은 스킨·Canvas helper를 유지하면서 은퇴한 퍼즐 허브의 fullArea 인자는 제거했다. LiveIncoming을 되살리지 않고 훈련 HUD 공통 오류 처리/스킨은 유지한다. 임포터 버전12는 합친 예산 규칙을 반영한다. 실제 main에서 기존13UI·6인 생성/선택·기본HUD 연결과 일반 Guide/Onboarding을 독립 소스 검토했다.

퍼즐 은퇴 원본 `66c2e59be021b069e5932006797073851eed348a`, 은퇴 기록 `f0d39b1fca5fa4c5ca0b43e4bb62053a02e6e99d`, PR132 병합 `bbe131dede9e8fe3b56691732bc1c04c218a02ad`, 이후 필드 통합 `239742c1c25fb7535658f5ba6e6c28a3c1a6e554`와 저장 전환 수리 `cbe52141b95f4a3912b66a54a26d66f2a777fc5a`를 실제 main의 조상으로 보존했다. **은퇴 삭제 대상94경로는 main Git tree와 실제 파일시스템 모두 부재**, 구 퍼즐 실행/진입/복습·전용 테스트·Resources/빌드 등록 및 삭제 GUID 참조 잔재0이다. 일반 Guide flow6(phase0–25)·FirstPlayGuide·Onboarding·옛 저장의 무해화/kit 환급·역사 문서는 보존했다. `RetiredPuzzleSavesStillLoad`3직업은 과거 저장을 정상 계정으로 이전하는 호환 검사이며 구 퍼즐을 실행하지 않는다.

병합 실제 소스1,172개 입력은 정적 검증과 일치한다. Runtime/Editor/Tests 컴파일 오류0(기존 경고88/28/35), UI 계약15/15(21.26초), refresh16·diff 통과다. 원문과 byte동일한 폰트 라이선스2개·과거 XML1개의 trailing whitespace는 보존·별도 기록했다. main `1fe680cc`에서28개 명시 메서드의 **56사례 전부 통과**, 실패/건너뜀0이다. 프로세스268.781초·테스트47.305초이며 나머지221.476초는 기존 Library 임포트/컴파일·시작/종료 등을 포함한 차이다. 3직업 구 저장에6슬롯/기존3소유자·성별/선택 ID·원문 archive를 함께 단언했고, 신규 남전사1인·생성 실패 보존·여성 소유권·일반 Guide 완주/저장 실패·재시도·공통 창·도감 KO/EN/필드 연결을 검사했다. 정본은 task23 `validation/main-merge/focused-1.xml`, `focused-result.json`, 두 최종 소스 감사 JSON이다.

이 main에서 추가 전체 EditMode·네이티브 실행/빌드·웹 빌드·위키 생성/게시·원격 push/Actions는0이다. 이전35개 네이티브와5,913개 누적 관찰은 각각 이전 소스/앱의 기록이며 main에서 전부 실행한 결과로 바꾸지 않는다. 보존 앱은 여전히 병합 전 `49c08e20` 입력이다. 앱 파일/실행파일 동일성을 확인했지만 **새 main 플레이어를 빌드하지 않았으므로 그 옛 바이너리 안의 퍼즐 packed asset 제거를 주장하지 않는다.** 이번 확인은 요청된 실제 main의 소스·진입·등록 제거와 집중 검증 범위다.

검사 임포트가 만든 공백 meta465개와 IDE 설정1개는 정확한 patch·해시 보존 뒤 원복했다. 원본 아트/폰트/GUID·실사용 save·다른 작업 변경을 보존했고, 브랜치·검증 근거·파일을 임의 영구 삭제하지 않았다. 이전의 ‘main 병합 후 확인 대기’ 항목은 이 범위에서 완료됐으며 남은 차단은 없다. 공개 원격 main/위키 반영과 새 main의 플랫폼 바이너리 검증은 이번 제한에 따라 미실행이다.

## 원본 main 전환과 캐주얼 리소스 공통 전환 (2026-10-15)

사용자의 원본 HELLSCRIPT 교체 지시와 안전 확인 후 전환 승인에 따라 원본 체크아웃을 최신 main 기준6d1c36a4로 전환했다. 기존 task6 통합 체크아웃은 같은 입력의 detached 상태로 보존했다. 고유 문서5개는 정확한 byte 검증을 거쳐 stash/소스 복구 기록에 보존했으며 진행 중 리깅·몬스터 작업과 원본 Git 객체·원화는 유지했다. 위키 프론트23파일은 인수된31af3b91을 로컬 fast-forward로 통합했고, 공통 리소스 미커밋843경로의 내용은 전후 동일하다.

공통 장비 조회는 승인된359원화 공급자를 기본으로 사용한다. 부분 provider의 고유 장비 우선순위와 B28→B32 공유를 유지하며 저장·카탈로그·해금은 바꾸지 않았다. 보석/제조 영약은 승인된 JewelerWindow 그림을 공통 사용한다. HUD 물약의512전체캔버스와66% 배치 계약은 유지한다. 새 대체본 ID·PNG/meta SHA·GUID를 고정한 tools/casual_resource_retirement_20261015.json에 따라 구 장비335·보석36·영약36의407 PNG와407 meta를 복구 가능한 휴지통으로 분리했다. 논리 크기는429,308,815B이며 영구 삭제나 실제 공간 회수로 보고하지 않는다. 기존 import receipt는 byte동일하며 portable verifier는 정확한407쌍의 부재와 새 대응407개만 허용한다.

현재 Runtime848/Editor55/Tests265 소스의 실제 Unity 참조 Roslyn 컴파일은 오류0, 경고88/28/35로 신규 경고0이다. retirement Python9검사와 삭제 후 importer 검증(removed407/replacements407)이 통과했다. 이 단계의 Unity/EditMode/네이티브 실행은0이며 과거 수용 결과를 현재 입력의 실행 통과로 바꾸지 않는다.

위키는 고정 구 리소스 목록 대신 현재 Resources·승인 장비·Hunyuan READY37·밝은 필드6을 수집한다. 실제 적용된 수집기의 집중10검사 및 직접 DB 수집은1515행/1337미리보기, 제거한407구경로의 현재 참조0으로 통과했다. 과거 문서가 필요로 하는141첨부164,301,114B는 exact6d Git객체/hash allowlist로 내보내며 Docs/Assets에 원화 사본을 추가하지 않는다. 프론트의 image.file 및 일반 필드 계약을 유지한다. 최종 전체 위키 생성·검사·이력 갱신은 후속 아트 입력 확정 뒤 한 번 수행한다. 공개 게시와push는 별도 직접 승인 범위 확인 전 실행하지 않는다.

사용자는 이후 새 대체본이 없는 그림까지 제작해 이전 컨셉을 전부 교체하도록 확정했고, 종류별 독립 채팅에서 병렬 진행을 요청했다. 스킬/공통UI/인물·아이템 인수의 세 제작방은 게임 소스를 쓰지 않고 대표와 명세를 납품한다. 승인된 기존700원화를 먼저 재사용하며 실제 부족한 스킬/공통그림만 내장image_gen으로 제작한다. 각 유형 대표의 게임 검증 뒤 확대하고 최종 코드·아트 통합 뒤 전체 EditMode와 한 개 네이티브 개발 빌드로 검증한다. 현재 신규 W01 대표는 픽셀 검토 중이며 게임 통과나 전량 완료가 아니다. 근거와 경로별 보존 상태는 task23 validation/cleanup-20261015/main-canonical-resources의 영수증과 new-art/production-threads.json에 있다.
