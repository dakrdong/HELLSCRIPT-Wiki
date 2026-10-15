# 대장간 캐주얼 스킨 — 전용 소스 인계

2026-10-14 · [English](Casual_Blacksmith_Skin_20261014.en.md)

상태: **공통 후속 공급, 승인 장비 provider·빈 슬롯9종의 화면 연결과 최종 정적 검사 완료. Unity import·실제 렌더 검증은 전체 화면 통합 담당에게 한 번 인계한다.** 초기 화면 기준은 `2e193c336c6176d80b9ddbdff7269246bf2590fb`이다. 첫 공통 API `cdb9c140fbf8b778bd3c0bbc0fbcc68a5d9e3fa8`는 `60a5c5d8d2d8fb23890b5f72059fc97a1bf05564`로 병합했고, 화면 연결·정적 검사는 `644e5782bfba943b75c1e957d2be67682de6273e`에 기록했다. 이후 공통 `0ccba80eef0da4703cb7ffcd1fb8e22848002b3d`를 `c0da48f4fe3c9db78b9347d29f2c111aaaef1f7b`로 병합했다. 이 문서는 위 후속 병합 이후의 화면 연결 변경을 포함한다. 부모가 승인한 14개 `BlacksmithWindow*.cs`와 새 `BlacksmithWindow.Casual.cs`, 전용 검사·문서만 직접 편집했다. **공통 파일은 직접 수정하지 않고 승인된 소유자 커밋을 merge로 받아왔다.** 원본 checkout/index, 게임 Core·데이터, 기존 PNG/.meta, 사용자 세이브를 직접 변경하지 않았으며 출석 파일럿 `6378320b`는 제외했다. 장비 원화 공급 장애가 해제된 것이며 전체 시각 완료는 아니다.

## 시안 확인과 기능 차이

시안: https://claude.ai/artifact/Qi7NdiGwLdRBwQh8ic7UAg

사용자 제시 32개, 실제 DOM 인벤토리 32개. 원본 크기로 시각 확인한 것은 **PC KO Main 1개**다. 32개를 전부 실기기·전체 화면 검증한 것이 아니다. 세부 목록은 [인벤토리](CasualBlacksmithSkinEvidence20261014/claude-artboard-inventory.json)에 있다. 옵션 변경·슬롯 강화·코어 탭·Craft placeholder의 실제 상호작용을 읽었다. 옵션 변경 비용 51,800 차감과 기록 증가, 슬롯 강화 재료 10 차감 및 작업 1/2, 코어 미선택 상태의 비활성 제어를 관찰했다. 이 값은 시안 샘플이며 게임에 복제하지 않았다.

시안의 다섯 번째 Craft는 placeholder다. 부모 결정에 따라 실제 게임의 **4개 서비스**를 유지했다. 시안의 일부 설명 본문 Noto 700은 공통 승인인 일반 본문 400으로 바꾸며 강조에만 700을 쓴다. 시안 크기로 게임 글자를 임의 확대하거나 축소하지 않았다.

핵심 시안 캡처: `forge-claude-pc-ko-initial.jpg`, Library `libfile_3f8e8b948f4081918d936a891d82df4c`, file `file_00000000eb0081f5a2d92bc97753a64f`, 버전 0. [실제 업로드 영수증](CasualBlacksmithSkinEvidence20261014/library-delivery.json). 사용자 브라우저 UI를 제외한 아트보드 crop이다. 이후 브라우저를 다시 사용할 때는 사용자 지정 `dakrdong@gmail.com` Chrome 프로필 확인이 선행되어야 한다.

## 변경 전후와 남은 차이

| 항목 | 이전 소스 | 이번 전용 소스 | 아직 확인/연결할 것 |
|---|---|---|---|
| 본문/강조 | 기존 본문 + 합성 Bold | Noto 400/실제 700, 합성 굵기 제거 | KO/EN 실제 줄바꿈·잘림 |
| 제목·버튼·탭·기존 display 숫자 | 기존 display/body | 공통 LabelFont의 Jua/Noto fallback, 동적 최소 보장 표시의 기호 fallback 수정 | 실제 전환·폭 검증 |
| 바탕/본문 | 흑요석/아이보리 | 생성된 공통 토큰의 하늘·크림/남색 | 승인된 화면과 픽셀 비교 |
| 주요 PanelBox·열·dialog | 각진 면 | 공통 StorageSurface.SetSkin의 둥근 면·선·그림자 | 시안 3px 선/5px 그림자와 실제 배율 비교, 안쪽 점선 |
| 버튼·탭 면 | UiButton Gilt | UiSkin.Casual chrome 연결, 입력/역할/선택/상태 전환 유지 | Icon 역할의 CasualClose 원형 면 |
| 장비 슬롯·상세 | 기존 공통 표시 | 장비 API 13곳·상세 API 2곳에 승인 provider를 담은 DefaultSkin 전달 | Unity import·실제 표시 검증 |
| 검색/직업 필터 | 기존 색/폰트 | 화면 factory와 비활성 template 함께 적용 | 실제 검색·dropdown clone 입력 |
| 장비·빈 슬롯 원화 | 기존 리소스 | 공통 Art/Casual/Equipment 공급, 빈 슬롯9종 ForBase 연결 | Unity import·렌더, 재화/코어/영웅 별도 연결 |

실제 게임 before/after 캡처는 **아직 없다**. 기존 `forge-1440x810-ko.png`는 현재 기준 SHA의 캡처라는 근거가 없어 before로 사용하지 않았다. 위 표는 소스 변화이며 렌더 증거가 아니다. 폰트 포인트·고정 배치 호출·best-fit 하한은 그대로지만 `TextWidth/TextHeight` 결과는 새 폰트에 따라 달라진다. 실제 사각형·영문 잘림 동일성을 주장하지 않는다.

`ForgeTextInk` 임시 대비 shim과 화면 전용 display-font 선택 함수를 제거하고 공통 `UiSkin.OnLight`·`LabelFont`로 연결했다. 서비스 탭의 Gilt 글자색도 제거했다. 최소 보장 값처럼 빈 문자열로 만든 뒤 동적으로 기호가 들어오는 display text는 값 변경 뒤에도 fallback을 다시 적용하도록 수정했다. 투자 0→500→0에 따른 최소 보장 `—`와 숫자의 실제 폰트 전환은 통합 런타임 검증에서 확인한다. 본문·등급·원소 색을 전체 계층 후처리로 치환하지 않으며 새 매 프레임 갱신·거래·저장 경로는 없다.

## 공통 담당/부모에게 필요한 연결

1. **재화·코어·영웅의 별도 공통 연결.** 장비 공급은 아래의 검증된 `CasualEquipmentArt.Provider`로 해결했다. 대장간 `DefaultSkin=UiSkin.Casual.WithEquipmentArt(CasualEquipmentArt.Provider)`와 `Open(..., skin:null)`의 기본 연결을 소스로 확인했다. 명시적으로 받은 skin은 유지하며 전역 `EquipmentArt` fallback은 변경하지 않는다. 빈 슬롯9종도 public `ForBase`를 통해 승인 장비 원화를 읽고, 파일/provider가 없으면 기존 슬롯 원화로 돌아간다. 재화·코어·영웅은 이번 장비 provider의 범위가 아니므로 계속 단일 공통 담당의 연결이 필요하다.
2. **공통 담당의 남은 시각 보완.** 소유자 `01a13790-39bc-7062-a516-b83a354000a8`에게 `UiButtonFace`의 Icon 조기 반환 때문에 `CasualClose` 원형 코랄 면이 그려지지 않는 경로, 기존 UiTheme 색을 직접 사용하는 `CoreQualityGauge`·`UiLockGraphic`, Casual 패널의 안쪽 점선을 요청한다. 게이지의 `.color=gold`만으로는 스킨이 적용되지 않는다. 주요 패널은 현재 borderWidth 2/shadow 3으로 연결했으며 시안 3px/5px와의 실제 배율 비교는 미검증이다. `ForgeWorkingPulse`는 색상 없는 알파 애니메이션이므로 별도 스킨 API의 필수 장애로 묶지 않는다. 복제 렌더러나 상태 우회는 만들지 않았다.
3. **전체 화면의 단일 통합 검증 인계.** 개별 화면의 Unity 실행·빌드 계획은 종료했다. 단일 통합 소유자 `01a13790-39bc-7062-a516-b83a354000a8`가 `task-6/skill-skin`의 별도 통합 브랜치와 기존 `Library`를 재사용해 모든 화면을 함께 검증한다. 이 sparse 소스 폴더에서 Unity·빌드·개별 검사를 다시 실행하지 않는다. 아래 전용 테스트 필터·스모크 범위·기존 영수증을 한 번 전달하며, Roslyn 정적 컴파일은 통합 환경의 실행 검증을 대신하지 않는다. 최종 소스 정리 커밋 뒤 동결하고 통합 담당의 구체적인 결함 요청이 있을 때만 재개한다.

통합 순서: task6의 별도 통합 브랜치에 공통 `0ccba80e`가 포함됐는지 확인한 뒤, 로컬 `/Users/t8g-2410-pn-005/Documents/Codex/2026-10-14/task-14/forge-skin`의 `codex/casual-blacksmith-skin`에서 화면 non-merge 커밋 `77d6702b` → `644e5782` → **이 문서를 포함한 최종 연결 커밋** 순서로 받는다. `60a5c5d8`·`c0da48f4`는 공통 수신 기록이므로 화면 변경으로 별도 cherry-pick할 필요가 없다. 또는 검토한 최종 브랜치 전체를 merge한다. 최종 연결 SHA는 커밋 후 외부 `evidence/implementation-handoff.json`에 기록한다.

[승인 ID/SHA 매핑](CasualBlacksmithSkinEvidence20261014/approved-art-map.json)은 장비359, 영웅6, 재화3, 코어8 **376개 실제 파일 SHA 일치**, 누락/불일치0의 기존 확인 기록이다. 후속 [장비 공급 검증](CasualBlacksmithSkinEvidence20261014/approved-equipment-followup.json)은 `0ccba80e`의 [import 영수증](../../tools/casual_equipment_art_receipt.json), 승인 원장4개 hash, 장비359개 원본·donor PNG·고정 커밋 Git blob·기존 대장간 승인표의 SHA와 meta/GUID359개가 일치함을 확인했다. 공통 경로는 `Art/Casual/Equipment/{ID}`, item 키330개(실물329+alias), 세트 emblem30개다. B28은 B32 경로를 공유하며 별도 B28 PNG는 없다. 이 확인은 Unity import나 픽셀 가독성 검사가 아니다.

빈 슬롯 연결은 sword B01/shield B32/helm B11/armor B13/gloves B15/boots B17/belt B19/amulet B21/ring B23이며, 반지2위치는 같은 원화를 참조한다. `SlotEmblem()`은 `skin.EquipmentArt?.ForBase(id)`를 사용한다. 장비 계산·아이템 ID·카탈로그 해제 상태는 그대로다.

현재 실제 재화 소비 경로는 `CurrencyIconView`의 금화 `Art/Attendance/reward-gold`, 심연 주화 `Art/GlobalHUD/currency-abyssal-coin`, 강화석 `Art/GlobalHUD/currency-enhancement-stone`, 코어 `Art/GlobalHUD/currency-core-{weapon,head,chest,hands,feet,belt,amulet,ring}`이다. 빈 슬롯의 기존 `Art/Blacksmith/slot-{sword,shield,helm,armor,gloves,boots,belt,amulet,ring}` 경로는 위 승인 ForBase 조회가 실패할 때의 fallback이다. `CharacterEquipmentView`는 `CharacterFigure.Load(heroClass)`를 통해 `Art/CharacterSelection/{Warrior,Ranger,Mage}` Texture2D와 같은 경로의 `.atlas` TextAsset을 읽는다. 기존 atlas는 각각6프레임이며 새 승인 영웅 PNG는 단일 이미지이므로 덮어쓰지 않았다. 공통 캐릭터 표시 어댑터와 클래스별 원화 선택이 필요하며 HeroSave 성별·저장 계약을 추가하지 않는다. NPC는 현재 소비자가 없어 추가하지 않는다.

공통 Git에는 승인 장비359 PNG가 포함됐고 donor 작업 폴더에서 검증했다. **대장간 sparse 소스 폴더의 중복 물리 PNG 복사0**, 새 생성0, 기존 PNG 덮어쓰기0, 삭제0이다. `git cat-file --batch`로 고정 커밋 blob을 메모리에서 검증했으며 PNG checkout/export는 하지 않았다. 추가 Resources 원화는 opt-in과 관계없이 빌드 입력에 **69,753,836 logical bytes(약66.522 MiB)**를 더한다. 이는 최종 압축 플레이어 크기나 디스크 여유 공간 변화의 실측값이 아니다. Unity import·화면 검증·전체 참조 확인 전에는 구원화를 정리하지 않으며 공유 재화와 출석 리소스도 유지한다.

## 검증과 다음 수용 범위

`0ccba80e` 수신과 DefaultSkin/빈 슬롯 연결 이후 최종 UI 계약 `--verify`는 **15 tests/31.841초 통과**, 갱신 검사는 **16 guarded bindings/193 UI sources 통과**했다. [소스 불변 검사](CasualBlacksmithSkinEvidence20261014/check_source_invariants.py)는 기준14파일의 store 17호출, Rect 배치 124호출, 로컬라이제이션 182호출, click listener 5곳과 font-size/best-fit 대입이 유지됨을 확인했다. 팔레트 ID, 장비 API 13곳·ItemDetail API 2곳의 skin 전달 및 임시 대비 shim 제거도 통과했다. 이 불변 검사는 실행 결과가 아니다.

`check_source_invariants.py`의 ownership guard는 전용 브랜치를 `0ccba80e`와 비교한다. 다른 화면을 합친 통합 브랜치 전체 diff에는 그대로 적용할 수 없다. 동결된 전용 소스의 기존 영수증을 재사용하고, 통합 담당이 통합 범위에 맞춰 검증한다. 이 인계를 위해 검사 프로그램을 수정하거나 개별 검사를 재실행하지 않는다.

최종 Roslyn 정적 컴파일은 **Runtime 834 C# 소스: exit 0/오류0/경고47**, **Tests 259소스: exit 0/오류0/경고2**로 완료했다. 대장간 소스 경고는0이다. 영수증의 실제 소스 입력과 기록 시점을 확인할 링크: [컴파일 결과](CasualBlacksmithSkinEvidence20261014/compile-result.json), [정확한 입력](CasualBlacksmithSkinEvidence20261014/compile-inputs.json), [Runtime 로그](CasualBlacksmithSkinEvidence20261014/Runtime.log), [Tests 로그](CasualBlacksmithSkinEvidence20261014/Tests.log). 정적 출력은 task14 증거 폴더에만 두며 기존 donor 참조 DLL만 읽고 Unity 캐시·프로젝트를 복제하지 않았다. **Unity·EditMode 실행0**이며 정적 컴파일을 런타임 통과로 보고하지 않는다.

이전 `644e5782` 검사 이력은 보존한다: Runtime 831/Tests 258소스, 오류0·경고47/2, 대장간 경고0, 전용8개 case 컴파일만, UI 계약15개/28.586초, refresh16 bindings/192 sources 통과. 이는 이전 입력의 결과이며 위 최종 결과와 구분한다.

최신 `BlacksmithCasualSkinTests` **9개 case는 컴파일만 완료**했다. 검사 범위는 default provider/전역 fallback 보존, 본문·강조·display 사각형/크기, 기호 fallback, 숨은 dropdown template과 선택 콜백, 버튼 상태 변경 뒤 Casual chrome 유지, 주요 패널의 기존 사각형이다. 빈 슬롯9종의 연결은 이9개 case에 포함되지 않으며 별도 소스 검토와 [장비 공급 검증](CasualBlacksmithSkinEvidence20261014/approved-equipment-followup.json)으로 확인했다. 기존 텍스트 측정 경로의 지연 Destroy 때문에 투자 0→500→0 전체 흐름은 동기 EditMode fixture에 부적합하므로 최소 보장 `—` 폰트 전환 확인을 통합 런타임 검증에 인계한다. 통합 담당에게 넘길 최소 EditMode 필터: `BlacksmithCasualSkinTests`, `BlacksmithTests`, `CoreCraftingTests`, `PuzzleForgeTests`, `ForgePresentationAllocationTests`, `StoreViewBindingTests`. 기존 실패를 제외하지 않는다.

통합 런타임의 입력 수용 범위(미실행): 네 탭/잠김 2초 안내/재진입, 선택·빈 상태·부족 재화·저장 실패, 옵션 변경 confirm/자동 목표 대기·계속·중단, 슬롯 시작/완료/시간경과, 코어 검색/직업·등급/투자 0·상한/제작·결과 확인·재접속, 스크롤 복원, dropdown open/close. 테스트 전용 세이브와 무음 실행을 사용한다.

기존 스모크 진입점은 소스로만 확인했으며 실행하지 않았다. 통합 담당이 한 통합 빌드의 순차 수용 범위로 선택한다.

| 기존 진입점 | 인수 | 성공 근거 |
|---|---|---|
| `RuntimeBlacksmithSmoke.cs` | `-hellscriptBlacksmithSmoke -hellscriptScreenshots <evidence> -hellscriptSavePath <disposable>` | `result.txt`의 PASS + `HELLSCRIPT_BLACKSMITH_SMOKE_OK` |
| `RuntimeForgeNavigationSmoke.cs` | `-hellscriptForgeNavigationSmoke -hellscriptEvidencePath <evidence> -hellscriptSavePath <disposable>` | `runtime.txt` + `HELLSCRIPT_FORGE_NAVIGATION_OK` |
| `RuntimeCoreCraftingSmoke.cs` | `-hellscriptCoreCraftingSmoke -hellscriptScreenshots <evidence> -hellscriptSavePath <disposable>` | `result.txt` + `HELLSCRIPT_CORE_CRAFTING_SMOKE_OK` |
| 코어 재접속 확인 | 위 코어 인수에 `-hellscriptCoreCraftingVerify` 추가 | `restart-result.txt` + `HELLSCRIPT_CORE_CRAFTING_RESTART_OK` |

세 스모크 모두 Debug build guard가 있으며 스스로 Quit하므로 서로 다른 스모크 플래그를 동시에 활성화하지 않는다. `CaptureOnly`/`SkipExercise`는 기능 합격 근거가 아니다. 기존 세 스모크의 해상도 입력에는 **2868×1320이 없으므로** 통합 수용에서 별도로 포함해야 한다. 이 source-only 작업에는 개별 실행·빌드 계획이 없다.

PC 16:9·16:10·21:9, 440×956/956×440, 2868×1320, KO/EN에서 글자·둥근 모서리·원형 가장자리·게이지 최소/최대·하단 버튼과 스크롤을 확인한다. 2868×1320은 Mac DPR3 가정, 좌우102/하단54/상단0 안전 영역 가정이다. 실제 iOS 검증이 아니다. 연보라 paid currency 숫자와 작은 주황 강조의 밝은 바탕 대비도 미검증이다. 성능 개선 주장 없음.

공통 규칙 제안: 완료된 body/font·button·equipment opt-in 연결에 더해 gauge·lock·Close Icon·점선까지 수용 범위를 명시한다. 동적 값 변경 뒤 glyph fallback과 숨은 dropdown template도 검사한다. 공유 PNG의 단일 저장소와 legacy alias·캐릭터 atlas 계약을 먼저 정한 후 화면별 소비자를 연결한다.

## 산출물과 보존

전용 clone은 Git objects를 원본에서 읽는 sparse source checkout으로 생성 직후 약25MB였다. 원본 index를 건드리지 않았고 Library/Builds/PNG를 복제하지 않았다. 증거와 최종 크기·통합 인계 상태는 task14의 `evidence/artifact-lifecycle.json` 및 `implementation-handoff.json`에 기록한다. 통합·검증 수용까지 보존한다. 최종 소스 커밋 후 동결하며 개별 검사·Unity·빌드를 재실행하지 않는다. 통합 담당의 구체 결함 요청이 있어야 작업을 재개한다. push/main 병합/웹 빌드/공개 배포/위키 실행은 0회이다.
