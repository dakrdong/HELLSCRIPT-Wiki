# 일일 퀘스트 캐주얼 스킨 — 최종 통합 인계, 소스 동결

2026-10-14 · [English](Casual_Daily_Quest_Skin_20261014.en.md)

## 2026-10-17 시안 ② 후속 보완

최신 38대지 인계에서 확인한 차이를 보완했다. 완료 버튼은 `UiButton.SetCompleted(q.claimed)`로 흰 면을 지정하면서 `interactable=false`·기존 role·라벨 alpha·체크·거래를 유지한다. 상태판은 `StorageSurface.SetSkin(...,fill:)` 한 개를 사용해 기존 흰/받음/안내 fill과 치수를 보존한다. 공통 API는 통합 담당 소유이며 전역 폰트·Jua 정책은 변경하지 않았다. `DailyQuestSkinTests`는 혼합 3상태의 메시 fill·face색·실제 비활성 submit 차단·수령 후 상태·repaint/계정 불변을 보강했다. 기존 기능·자정·시계역행·저장실패·상자·출석탭 경로는 보존하며, 이번 Unity 실행·실제 화면 검수는 통합 담당에게 인계한다. 정적 UI 계약 15개와 갱신 경계 검사는 통과했고, 관련 테스트 소스는 공통 API를 포함한 Roslyn 검사에서 오류·경고 0이었다. 테스트 실행은 하지 않았다. 아래 10/14의 미지원 API 설명은 당시 기록이며 이 후속 소스에서는 해소됐다.

## 최종 통합 인계 — 이전 개별 큐 계획을 대체

사용자 지시에 따라 모든 화면을 합친 단일 통합 검증으로 전환했다. 일일 퀘스트의 개별 Unity 실행·빌드 계획은 종료했다. 통합 소유자는 `01a13790-39bc-7062-a516-b83a354000a8`이며 `task-6/skill-skin`의 별도 통합 브랜치와 기존 Library를 그 자리에서 사용한다. 이 화면 담당은 본 인계 문서 저장 뒤 소스 편집을 멈추며, 통합 담당의 구체적인 결함 요청에만 재개한다. 브랜치·소스·원화·증거·복구 stash는 보존한다.

- **안전한 소스 상태:** `229df24acd9c42a51e9435f600a8ad02b7c6f66f`, clean. 이 최종 인계는 한영 문서만 바꾸며 구현/검사/원화 생성은 반복하지 않았다. 인계 문서 커밋의 전체 SHA는 task-20/evidence/implementation-manifest.json에 기록한다.
- **통합 순서:** 공통 `0ccba80eef0da4703cb7ffcd1fb8e22848002b3d` 또는 이를 포함한 통합 기준 → 화면 구현 `bb975a9aa7347f969a5cab72145293253fd03802` → 화면 보완 `229df24acd9c42a51e9435f600a8ad02b7c6f66f` → 본 문서 인계 커밋. 이미 포함된 커밋은 다시 적용하지 않는다. 로컬 공통 병합 `8b8c4cd4`는 두 이력을 보존하기 위한 것이므로 공통 기준이 있는 통합 브랜치에 별도로 cherry-pick할 필요가 없다.
- **원화 경로:** `Assets/HELLSCRIPT/Resources/DailyQuestCasual/abyssal-coin-casual-v01.png`와 `.meta`, 상위 `DailyQuestCasual.meta`는 첫 화면 커밋에 들어 있다. ID `abyssal-coin`, SHA `d2624826b5fc9a3cc987d2f7502ae7245e0450caf32aa7889a1e35daa57c9654`. 추가 복사·재생성·전역 재화 매핑 교체는 필요 없다.
- **남은 공통 연결:** 필수 호출 연결은 완료했다. 장비·차트·전투 API는 이 화면에 해당하지 않는다. 완료 버튼의 흰 면, 패널 fill override, Jua/Noto400 시안 차이는 아래에 기록한 시각 차이이며 현재 입력·게임 계약을 우회하지 않는다.
- **통합 EditMode 필터:** `Hellscript.Tests.DailyQuestSkinTests;Hellscript.Tests.DailyQuestTests;Hellscript.Tests.StoreViewBindingTests;Hellscript.Tests.UiButtonTests;Hellscript.Tests.CasualUiSkinTests`. 공통 검사를 통합 묶음에 한 번 포함하고 동일 입력으로 화면별 반복하지 않는다.
- **스모크 진입점:** `RuntimeDailyQuestSmoke.cs`의 개발 빌드 자동 진입 `-hellscriptDailyQuestSmoke -hellscriptSavePath <isolated-daily-save> -hellscriptScreenshots <daily-evidence>`. 재시작 확인은 같은 격리 세이브와 위 플래그를 유지하고 `-hellscriptDailyQuestResume`를 추가한다. 한 통합 빌드의 계획된 검증 묶음에서 실행하며 일일 퀘스트 전용 빌드를 만들지 않는다. 성공 근거는 `initial.txt`/`restart.txt`와 `HELLSCRIPT_DAILY_QUEST_RUNTIME_OK`/`HELLSCRIPT_DAILY_QUEST_RESTART_OK`다.
- **남은 실행 범위:** 140개 KO/EN·크기·안전영역·상태 조합, 입력·저장 실패/재시도·100주화·탭 왕복·재시작, 실제 게임 after 캡처/Library 저장. 아직 Unity 실행0이며 실제 iOS 검증0이다. 이 문서의 기존 정적 검사 근거를 실행 결과로 바꾸어 보고하지 않는다.

## 현재 결과

부모가 지정한 `2e193c336c6176d80b9ddbdff7269246bf2590fb`에서 시작해 공통 담당의 `cdb9c140fbf8b778bd3c0bbc0fbcc68a5d9e3fa8`을 반영하고 화면 구현을 `bb975a9a`로 저장했다. 후속 공통 `0ccba80eef0da4703cb7ffcd1fb8e22848002b3d`은 기존 커밋을 보존한 병합 `8b8c4cd4`로 통합했다. **Unity import·EditMode·플레이어 검증 대기이며 완성 판정이 아니다.** 로컬 브랜치는 `codex/casual-daily-quest-skin`이고 최종 화면 커밋·파일 해시는 task-20/evidence/implementation-manifest.json에 기록한다. 기준 커밋 소비 외 공유 파일 수정은 없으며 출석 화면·Core·저장 코드는 그대로다. 부모 단일 검증 큐 배정 전 Unity는 실행하지 않았다.

후속 버전의 탭 Navy 대비와 공통 Noto 기호 보강은 기존 셸/LabelFont 호출로 반영된다. 화면 보조 글자는 UiSkin.Casual.Muted로 통일했다. 이 값은 크림에 맞춘 색이므로 파란 안내판 본문에는 기존 Body를 사용한다. 이전 화면의 D4Muted는 짙은 남색이었으므로 보조색 변경은 공통 역할 통일이며 가독성 향상이라고 주장하지 않는다. 장비·차트·전투 API의 호출 대상은 이 화면에 없고, 재화 provider·완료 전용 버튼·패널 fill override는 아직 없음을 확인했다. 장비359종을 화면별로 다시 복사하지 않았다.

## 시안 확인

시안: https://claude.ai/artifact/S8NPK47VHnhG6cFPEWhPvz

사용자 제시27개 중 실제 제목/본문/치수/버튼 상태를 확인한 것은27개, 확대 시각 확인은 서로 다른3개(PC16:9 KO 혼합, 세로 EN 혼합, 가로 EN 혼합)다. PC 수령 후 캡처는 같은 보드의 두 번째 상태다. 수령25→40, 출석 시연 탭 안내, 새로고침, 닫기/재열기를 확인했다. 빈/오류/2868×1320 보드는 시안에 없으며 실제 게임 검증은 별도다. 다음 브라우저 열람이 필요하면 사용자 지정 `dakrdong@gmail.com` Chrome 프로필을 먼저 확인한다.

| 비교 | 이전 게임 소스 | 이번 구현 소스 | 남은 확인 |
|---|---|---|---|
| 판 | 어두운 사각 면·가는 선 | 크림 본문, 흰 카드, 공통 Rounded Surface를 이용한2단위 선·반경14 | 실제 픽셀·반경과 시안 그림자/초록 tint 충실도 |
| 상태 | 금색/초록 강조 | 수령 가능 초록 테두리·게이지, 완료 회색 판·보조색, 파란 규칙판 | 활성/선택/비활성/오류의 실제 입력·시각 |
| 글꼴 | 기존 공통 본문 | Noto400 일반, 명시적 요약 Noto700, Jua 제목·숫자·탭·버튼 | KO/EN 잘림과 실기기 가독성 |
| 주화 | 전역 기존 주화 | 승인 abyssal-coin의 화면 전용 매핑 | 18단위 표시의 알파·테두리·작은 크기 가독성 |
| 셸·버튼 | 공통 어두운 UiWindowFrame/UiButtonFace | UiSkin.Casual 창·닫기·탭·새로고침, 초록 Primary 수령 버튼 | 실제 둥근 모서리·상태 전환·입력, 시안의 흰 완료 버튼과 차이 |

시안보다 글씨를 축소하거나 레이아웃 칸을 넓히지 않았다. 기존 프레임 최대1200×1600, 본문 고정 배치, 세로5카드·가로3×2, 모든 텍스트 font-size 인자, 게이지2단위 높이, 버튼/아이콘 사각형, 탭 콜백과 닫기를 유지했다. 시안 탭의 시연 토스트를 실제 게임 기능으로 복제하지 않았다. 공통 규칙에 따라 Jua 버튼/탭을 사용했으며, 시안의 Noto400 버튼/탭과 차이는 남은 공통 수용 항목이다.

## 변경 파일

- `Assets/HELLSCRIPT/Runtime/Presentation/DailyQuestWindow.cs`
- `Assets/HELLSCRIPT/Runtime/Presentation/RuntimeDailyQuestSmoke.cs`
- 신규 `Assets/HELLSCRIPT/Runtime/Presentation/DailyQuestCasualSkin.cs` 및 `.meta`
- 신규 `Assets/HELLSCRIPT/Tests/Editor/DailyQuestSkinTests.cs` 및 `.meta`
- 신규 `Assets/HELLSCRIPT/Resources/DailyQuestCasual.meta`
- 신규 `Assets/HELLSCRIPT/Resources/DailyQuestCasual/abyssal-coin-casual-v01.png` 및 `.meta`
- 본 한영 문서

최초 제안의 `Resources/UI/DailyQuestCasual` 대신 `Resources/DailyQuestCasual`을 사용했다. 기준에 존재하지 않는 공유 `UI` 상위 폴더/.meta를 새로 소유하지 않도록 하기 위함이다. 모든 새 GUID는 화면 전용이고 기존 주화 파일과 `.meta`는 그대로다. 새 어댑터는 기존 UiTheme.D4 토큰·UiSkin.Casual·StorageSurface를 읽어 사용하며 새 팔레트/430개 카탈로그 항목/메시/Canvas/게임 규칙을 추가하지 않는다.

공통 API 연결은 `ContentWindowView.Open(..., skin:UiSkin.Casual)`을 사용한다. 수령 가능 버튼만 Primary 역할을 주고 `SoundCue="ui.click"`으로 기존 클릭음을 유지했다. 초록 바탕 글자는 Navy, 금색·성공·오류 본문은 공통 OnLight/Success/Danger를 사용한다. `LabelFont`를 거쳐 Jua 미지원 문자의 Noto fallback을 보존한다. 재화는 장비 API 대상이 아니므로 `WithEquipmentArt`를 적용하지 않았다.

`StorageSurface.SetSkin`은 fill을 크림으로 고정하므로 흰/완료 회색/규칙 파란 판은 기존 공유 둥근 primitive 두 겹으로 유지했다. 완료와 진행 중 버튼은 실제 비활성이며 공통 회색 면/텍스트 alpha .55를 따른다. 시안의 흰 완료 버튼을 얻기 위해 Chosen이나 입력 가능 상태를 위조하지 않았다.

## 승인 원화·정리

ID `abyssal-coin`, 승인/실제 SHA256 `d2624826b5fc9a3cc987d2f7502ae7245e0450caf32aa7889a1e35daa57c9654`.
원본 `/Users/t8g-2410-pn-005/Documents/Codex/2026-10-13/task-6/output/hellscript-casual-items/currency/abyssal-coin-casual-v01.png`.
1,316,714 bytes,1254×1254, 알파 있음. 복사본의 바이트 해시가 원본과 같다. UI용 sprite import는 기존 주화 importer 설정을 참고했고 새 GUID를 발급했다. 실제 Unity import는 대기 중이다.

새 제작 필요 자산은 현재 발견하지 않았다. 주화를 전역 교체하거나 구원화를 삭제하지 않았다. 교체 검증·전체 참조·Resources 문자열·GUID/.meta·활성 작업·복구 경로 확인 전 정리하지 않는다. 파일 크기와 실제 여유 공간을 혼동하거나 확보하지 않은 디스크 회수량을 주장하지 않는다.

## 검증 근거와 큐 요청

- **통과**: `python3 tools/check_ui_contract.py --verify --receipt ../evidence/followup-ui-contract-receipt.json` —15검사. 첫 API 연결의 이전 근거도 별도 보존했다.
- **통과**: `python3 tools/check_ui_refresh.py` —16 bindings guarded,192 UI source files inventoried.
- **통과**: `git diff --check`.
- **통과**: 설치된 Roslyn의 변경 런타임3파일+신규 NUnit 파일1개 독립 C# 형식 검사, 오류0·경고0. 후속 공통 assembly SHA `ceea23b5ee5c40f4a07666594541be1300c132336b8c739acce227cca6680e02`를 확인하고 읽기 참조했다. 연결된 런타임 소스 manifest833개를 `0ccba80e`와 대조해 차이0을 확인했다. Unity import/compile/EditMode/플레이어 성공을 뜻하지 않는다. 기준 assembly와 소스의 중복 이름 경고 CS0436만 명시적으로 억제했다. 근거: task-20/evidence/followup-typecheck/receipt.json.
- **통과**: 독립 읽기 전용 소스 리뷰에서 역할·클릭음·폰트·수령/저장 조건의 회귀를 발견하지 못했다. 신규 테스트는 실제 onClick 콜백을 호출하지만 물리 입력·오디오 재생은 실행 검증 대상이다.
- **대기**: 신규 DailyQuestSkinTests3개, 기존 DailyQuestTests33개, StoreViewBindingTests, 영향을 받는 UiButtonTests·CasualUiSkinTests·번역 검사. 새 테스트는 전역 주화 매핑/히트 영역, 둥근 판 경계/비레이캐스트, 재렌더·수령 콜백 후 스킨/역할/클릭음/위치·크기를 검사한다.
- **대기**: 무음 네이티브140조합(7크기×KO/EN×안전영역 유무×start/mixed/ready/claimed/empty). 기존120조합과 실패 조건을 보존했다.
- **추가 소스 검사**: 게이지 경계, 승인 주화 sprite/종횡비/비레이캐스트, 셸·닫기·버튼 스킨/역할/클릭음 유지 단언을 추가했다. 캡처 render/texture 픽셀 일치와 Canvas 배율을 기록하며 실제 해상도 불일치는 실패시킨다.
- **2868×1320 가정**: 논리956×440,DPR3; 안전영역 좌우102/하단54/상단0. Mac 시뮬레이션이며 physicalIOS=false를 출력한다. 원래6해상도의 안전영역 가정은 변경하지 않았다.
- **보존**: 실제 활동 후100개 수령, 저장 실패·재시도, 중복 지급 방지, 자정·시계 역행, 출석 탭 왕복, 뒤로/닫기·재시작. 무음은 테스트 플래그가 있는 개발 빌드의 BeforeSceneLoad에서 적용한다.

실행 프로젝트·큐 슬롯·출력 경로를 부모가 정한 뒤 격리 `-hellscriptSavePath`, `-hellscriptScreenshots`로 실행한다. 이 sparse 작업 폴더에는 Library/Temp/Obj/Builds를 만들지 않았다. 전체 회귀·웹 빌드·공개 배포·push/main 병합은 실행하지 않았다.

## Library 캡처

모두 Claude 시안 캡처이며 구현 후 게임 캡처가 아니다.

| 캡처 | 실제 library_file_id |
|---|---|
| PC KO 혼합 | `libfile_87a1673c890481918a9c6ff959404716` |
| PC KO 수령 후 | `libfile_ea502fe0399c8191a0eec70ddccab152` |
| 세로 EN 혼합 | `libfile_a5d002bdba5081918b9d00fdc5e4841e` |
| 가로 EN 혼합 | `libfile_b0c9068d418c81918ef05c381b51af06` |

시안 DOM·스타일·27보드 목록·검사 원자료·Library 결과·산출물 수명은 task-20/evidence와 task-20/artifact-lifecycle.json에 보존한다.

## 공통 규칙에 추가할 제안·남은 차이

1. 공통 UiButton에 입력 상태와 독립된 완료 표현이 필요하다. 현재 완료/진행 중은 모두 회색 비활성이며 시안의 흰 완료 면과 다르다. 실제 비활성·클릭 방지·alpha 정책을 유지하는 표현인지 공통 담당이 결정한다.
2. 공통 StorageSurface.SetSkin에 화면별 fill override가 생기면 흰/회색/파란 상태 판을 같은 API로 통일할 수 있다. 현재는 공통 primitive 두 겹이며 자체 mesh를 만들지 않았다. 그림자와 초록 tint는 실제 캡처 비교가 남았다.
3. 공통 Jua 버튼/탭과 시안 Noto400 차이를 일괄 결정한다. 글자 크기·칸 폭 변경으로 차이를 숨기지 않는다.
4. 공통 API 누락 대기는 해소됐다. **개별 큐는 종료했고 통합 소유자의 단일 통합 검증으로 이관했다.** 거절되거나 불확실하게 실행된 작업이 아니며 Unity를 시작한 적 없다. 통합 검증 후 핵심 게임 after 캡처를 Library에 저장해야 한다.
