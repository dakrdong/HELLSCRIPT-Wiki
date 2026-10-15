# 룬 보드 캐주얼 스킨 — 통합 전 인계 (2026-10-14)

상태: **단일 통합 인계 완료 후 소스 동결, 시각·플레이어 승인 미완료**. 화면 전용 구현과 후속 공통 API 연결은 완료됐고 공통 폰트의 누락6자는 정적 cmap 확인에서 해결됐다. 모든 화면을 합친 한 번의 통합 검증으로 전환했으므로 개별 Unity 실행·개별 빌드 계획은 종료한다. 원본 저장소는 수정하지 않았다. 부모가 승인한 공통 커밋을 병합했으며 공유 파일에 독자 변경은 없다. 원격 push, main 병합, 공개 공유, 웹 빌드를 수행하지 않았다.

## 기준과 소유권

- 격리 브랜치: `codex/casual-rune-board-skin`.
- 최초 기준: `2e193c336c6176d80b9ddbdff7269246bf2590fb`; 검증된 스킬 코드 `4139a77e2df495cee009c07c6094b2eea9b1aeaa` 이후 차이는 문서4개다. 최초 화면 구현 `5d0f91c450634d89a747da32463e117155f735a9`를 보존했다.
- 현재 공유 의존성: `0ccba80eef0da4703cb7ffcd1fb8e22848002b3d`(선행 `6cdf2df6` 폰트/geometry, `f25e67aa` 장비 공급 포함). 충돌 없이 merge commit `d5642cf3941bab7ad63e39227f1ac84125d23851`로 반영했다. 이전 공통 `cdb9c140` 연결은 `0380f408`에 보존했다. 부모 통합 시 최신 공통 커밋 → 최초 화면 구현 `5d0f91c4` → 연결 `0380f408` → 이번 후속 정리 순서다. merge 자체를 cherry-pick할 필요는 없다.
- 출석 파일럿 `6378320b` 제외. 원본 checkout·스테이징·미커밋 문서·승인 원화 보존.
- 허용: RuneBoardWindow*.cs, RuneGemView/Mesh, RuneBoardArt, 룬 전용 리소스/검사.
- 공유 담당 커밋과 동일: UiSkin/UiButton/UiButtonFace/StorageSurface/ContentWindow*/EquipmentArt 등 공통 API. 직접 수정 없음. UiTheme/UiFonts/UiIconButton/UiSafeArea/UiLayout, RuneBoardGraphic/RuneV13Art/RuneItemArt, Core/Runes 데이터와 거래, Packages/ProjectSettings/Scenes/asmdefs도 화면 작업에서 변경하지 않았다.
- 공통 화면 규칙·스킬 구현 기록·병렬 handoff를 읽고 적용했다. 일반 셸 U/Z·사각형·폰트 크기는 유지한다. 스킬 내부 텍스트 폭 확장 예외는 전파하지 않았다.

## 실제 시안 확인

원본: https://claude.ai/artifact/XiicccfvWaiUuAGVFDM44i

사용자 제시 32개. **목록 32/32 실제 확인, 개별 상세 시각·상호작용 검토 1/32 (#2 16:9 KO)**. 다른31개는 상세 검증 완료로 세지 않는다. 처음 일부 loading 상태였고, 이후 전체 목록이 로드된 것을 확인했다. 제목·파일명은 task17/evidence/artboards.json에 있다.

PC 시안에서 검→대검 전환, 보관함 Attack Single 선택, 60° 회전과 저장 버튼 비활성 유지를 확인했다. 시안 저장은 실행하지 않았다. 선택 메타데이터700, 일반 설명 Noto Sans KR400(11.76px, line18.816px)을 computed style로 확인했다. 시안의 보관함155/160·무기 레벨·활성 노드는 데모 fixture이며 실제 계정 수치를 복제하지 않는다.

추가 열람은 보류다. 사용자 후속 지시는 `dakrdong@gmail.com` Chrome 프로필에서만 다시 열도록 했다. 지원 목록에 `kr.doubledown.com`과 `Hoosung`만 노출되어 이메일은 확인되지 않았다. 이후 새 탭은 열지 않았고 다른 사용자 탭도 변경하지 않았다. 지정 계정 확인 후 #1 portrait, #3 EN, dialog/empty/error artboards의 상세 비교가 남았다.

## 구현 전후 (소스 기준, 실제 캡처 승인 아님)

| 요소 | 이전 | 변경 |
| --- | --- | --- |
| 바탕 | 흑요석 단색 | 공통 `frame.root.pc` 하늘·크림 paint 읽기 재사용 |
| 패널 | 어두운 Image + 사각 UIRectBorder | Image 파생 StorageSurface.SetSkin, 공통 크림 면·남색 둥근 선·안쪽 범위 그림자 |
| 버튼 | 공통 Minimal의 기존 흑요석 mesh | UiButtonChrome.Casual/CasualClose; 기본 크림·Primary 초록·선택 노랑·비활성 회색, 기존 focus와 상태 fade 유지 |
| 닫기 | 일반 아이콘 버튼 | 같은 클릭 영역에서 둥근 산호색 면·흰 아이콘 |
| 본문 | 시스템 본문/가상 bold | 공통 Noto400/명시700, Jua 제목·탭·버튼/일부 숫자; Jua 미지원 기호 포함 문장은 일반 Noto400/명시 강조700 |
| 분류 원화 | 단색 emblem | 승인된 5개 원본 PNG를 흰 tint로 표시; UV만 투명 여백 조정 |
| XP | 무제한 비율·0에도1px | 0–1 clamp·0 숨김·공통 Clip으로 넘침 방지 |
| 외곽 연결선 | 선분별 분리 사각형 | 공유 꼭짓점으로 연결한 stroke mesh, 지역 밑선과 전경 순서 분리 |
| 보드 viewport | 사각 RectMask2D | 같은 입력 면·좌표에 공통 UiSkin.Clip 자식 (노드별 mask 생성 없음) |

저장·회전·배치·회수·무기전환·되돌리기 모델, pointer/drag, ScrollRect, StoreViewBinding guard를 유지했다. 주요 입력·저장 메서드13개가 기준과 byte 동일함을 source-contract.json으로 확인했다. 새 Canvas·Update 루프·재화 글로벌 매핑은 없다. 모서리/버튼 paint는 기존 사각형 내부에서만 그린다.

초기 화면 전용 BaseMeshEffect 두 종류는 공통 API 연결 후 제거했다(이 작업이 새로 만든 코드4파일만 삭제, Git 최초 구현으로 복구 가능). 이제 공통 UiButtonFace의85ms 보간을 그대로 사용한다. 공통 API는 Primary를 초록으로 정의하므로 초기 화면 전용 노랑 primary에서 초록으로 통일했다. 기존 Item 역할은 배경을 그리지 않아 같은 StorageSurface를 공통 SetSkin으로 그리고 알파1을 복구한다. hover/선택 때 공통 Item의 각진 링과 둥근 카드가 함께 보이는 차이는 실제 캡처 검증 항목으로 남는다.

창은 `ContentWindowView.Open(...,skin:UiSkin.Casual)`을 사용한다. 일반 패널은 `SetSkin`, 보드의 차가운 의미색과 XP는 기존 공개 `Paint`+`radius`를 사용한다(공통 SetSkin에 면색 override가 없음). Clip은 부모의 좌표/크기를 그대로 유지하고 입력은 원래 stage에 남긴다. 장비 상세·슬롯·비교 기능은 이 화면에 없고 승인 룬 category ID는 equipment ID가 아니므로 `WithEquipmentArt` 공급자를 만들지 않았다.

후속 API도 실제 호출부를 확인했다. 장비359종 provider, 차트/통계, ConfigureCombatSkin/ConfigureEdictSkin 호출 대상은 룬 화면에 없으므로 연결을 추가하지 않았다. 공통 Muted가 자체 대비 보정을 하므로 화면 alias의 중복 OnLight를 제거했다. 공통 Tab의 Navy 규칙은 기존 Btn+UiTheme.Choice 기반 룬 탭 경로를 재배치하지 않는다. 공통 장비 PNG는 Git 의존성에 포함되지만 source-only sparse checkout에 다시 복사하지 않았으며, 기존 룬 category5종은 화면 전용 매핑을 유지한다.

글자 크기·간격 축소를 추가하지 않았다. 기준부터 있던 미니맵 BestFit 최소4, 상세 능력명 Z(.75)–Z(.94) 범위는 그대로다. 이 기존 정책을 새 축소 예외 승인으로 해석하지 않는다. 새 폰트가 실제로 선택하는 크기와 KO/EN 잘림은 Unity 캡처로 확인해야 한다.

기존 육각 타일·능력 glyph·작은9×9 식별 emblem·무기 탭 원화는 유지했다. 승인 category 메달을 셀 전체에 대입하지 않았다.

## 승인 원화 매핑

5/5 실제 열람·SHA 검증. 총6,268,806 bytes. 감독 상태 `pass` 및 Art_Transition 인덱스 확인. 원본 PNG는 byte 단위 그대로 복사했다. 표시 UV는 alpha>=8 bbox+4px 여유이며 원본이미지 편집 없음. 기존 RuneBoard Icons importer 정책(128px/Bilinear/Clamp/no mip/sRGB/alpha)과 같은 meta로 새 GUID를 부여했다. Unity import 결과는 미검증이다.

| 승인 ID | 화면 전용 Resources 키 | SHA256 |
| --- | --- | --- |
| rune-attack | `Art/RuneBoard/Icons/Casual/rune-attack` | `f22cd4a945b6c1e01b0c7717d178941f0582c1ba0362e827490a9f062844153b` |
| rune-magic | `Art/RuneBoard/Icons/Casual/rune-magic` | `cd632f8bb0d53b85dcd8d3c2d22430e5dd0e4ca7291367ef34eba60f14251f61` |
| rune-support | `Art/RuneBoard/Icons/Casual/rune-support` | `486d18c9487fc5fb4de252dc27ee9c4d91b7f4a76e7c0421cd9fe8428daa6b87` |
| rune-critical | `Art/RuneBoard/Icons/Casual/rune-critical` | `8e317c60e6994de0c7edb2dcdc0b87de962ccdda0e9befc619594124e7e6ec99` |
| rune-skill | `Art/RuneBoard/Icons/Casual/rune-skill` | `c6c687908700ed79b1570ea56862465617837221e6cfc83c90057bd1bd9654c2` |

원본 경로·크기·해시는 task17/evidence/approved-rune-art-mapping.json에 있다. 새 원화 생성은 필요하지 않았다. 구 RuneBoard 리소스 삭제0개: RiftEntryWindow 도움말 Resources 문자열 참조 등 외부 의존성이 남아 있다. 공유 GUID37개에 대해 추적파일8,295개 기준 직접 외부GUID가 없었다는 사실만으로 삭제 안전성을 주장하지 않는다. 미추적/타작업 참조와 실제 교체 검증이 완료되지 않았다.

## 검사와 남은 승인 조건

실행:

- `git diff --check` 통과.
- `python3 tools/check_ui_contract.py --verify --receipt ../evidence/ui-contract-receipt.json`:15/15 통과.
- `python3 tools/check_ui_refresh.py`:16 guarded bindings/193 source inventory 통과.
- Roslyn C#9, 설치된 Unity6000.6/원본 프로젝트의 참조 어셈블리 **읽기 전용**: Runtime834파일 컴파일0오류, 변경파일 경고0. 전체 기존 경고90은 로그 보존. Editor·Player는 실행하지 않았다.
- 폰트 cmap 후속 확인 **통과**: Noto400/700/900 모두 `≠①②ⓘ□◆` 지원. 기존397 KO+397 EN 및 화면/카탈로그 감사의 Noto 누락 항목이 모두 해소됐다. 기존2604 codepoint의 수평 메트릭과 줄 메트릭·GUID 보존을 직접 확인했다. Jua는 그대로이며 미지원 기호는 기존 Noto400/명시700 fallback을 사용한다. 이전 실패 원자료는 `evidence/rune-font-coverage.json`, 후속 SHA·검사는 `evidence/rune-font-followup.json`에 각각 보존한다. Unity의 실제 font import/HasCharacter 검사는 아직 실행 전이다.
- RuneBoardSkinTests 6개 작성 및 C#9 소스 컴파일0오류·0경고. 공통 버튼 상태·chrome/CanvasGroup/Item 배경·Icon/Clip 경계·좌표/연결선 면적/원화/폰트 조건을 포함한다. Unity실행은 대기. 필수 글리프 테스트에 Ignore/Explicit/허용오차 완화는 없다.

단일 통합 담당이 전체 화면을 합친 검증 범위에 포함할 항목(룬 담당의 별도 실행 계획 아님):

1. `0ccba80e`와 새 Noto400/700/900 SHA를 검증 입력에 고정한 뒤 Unity import 및 필수6자 HasCharacter를 확인한다. 표시 문자열·숫자는 바꾸지 않는다.
2. RuneBoardSkinTests, RuneBoardNativeTests, RuneMasteryTests, StoreViewBindingTests의 관련 범위. 기존 저장 실패/undo/회전 parity를 제외하지 않는다.
3. KO/EN PC16:9·16:10·21:9, mobile440×956·956×440 및2868×1320. 비활성/선택/빈 보관함/오류/닫기 확인/연습5단계/무기6개·프리셋5개.
4. 클릭·터치 합성·스크롤·드래그·회전·취소·저장 실패/성공·중첩 focus와 재진입. 테스트 fixture/임시세이브만 사용, 무음. 사용자 세이브 쓰기 금지.
5. XP 모서리 넘침, 지역 연결선 교점, 아이콘 테두리, 큰패널 모서리, 영문 잘림을 실제 캡처로 확인. 글자 축소로 해결하지 않는다.
6. DPR3 left/right102,bottom54,top0의 Mac 안전영역 시뮬레이션은 가정이며 실제iOS검증이라고 부르지 않는다.

## 캡처/Library와 공통 제안

실제 게임 before/after 캡처는 아직 없다. Chrome PNG 내보내기는 앞선1차조사에서 다운로드 완료가 확인되지 않았고, 추가 브라우저 검사는 위 프로필 조건으로 보류다. **library_file_id: 없음 (미업로드)**. 캡처를 조작하거나 정적 mockup을 실제게임으로 표기하지 않는다.

공통 규칙 제안: (1) 스킬 문구뿐 아니라 연결될 화면들의 실제 Unicode 합집합으로 subset 검사, (2) 공통 API 소비 완료; Item의 둥근 선택 링과 의미색 패널 override는 실제 캡처 결과가 필요할 때 공통 담당에게 요청, (3) 승인 메달 원화는 category와 per-cell gameplay glyph의 용도를 나눠 매핑, (4) gauge zero/full과 둥근 mask 범위를 관련 상태 검사에 포함.

이 작업 공간은 source-only sparse clone(공유 Git objects)이며 Library/Builds를 복제하지 않았다. 브랜치·소스·검증 근거를 보존한다. 이후 수정은 통합 담당의 구체적인 결함 요청이 있을 때만 재개한다.

이전 큐 근거는 부모 전달07:50 UTC 외부 Unity PID30116이 `scratchpad/proj-final`에서 실행 중이라는 기록이다. 이 작업이 새로 조회하거나 실행한 결과로 표기하지 않는다. 이후 사용자 지시로 개별 화면 큐를 종료하고 아래 단일 통합으로 넘겼다. 외부 프로세스를 종료하거나 기존 캐시를 사용하지 않았다.

## 단일 통합 담당에게 넘기는 최종 입력

단일 소유자: `01a13790-39bc-7062-a516-b83a354000a8`. `/Users/t8g-2410-pn-005/Documents/Codex/2026-10-14/task-6/skill-skin`의 별도 통합 브랜치와 기존 Library를 그 자리에서 재사용한다. 이 룬 작업에서는 새 Editor·플레이어·빌드·캐시를 만들지 않으며 완료된 검사를 반복하지 않았다.

- 안전한 마지막 구현: `6ef4b12b87d1151b5e26108961f7816e75408577`. 인계 전 미커밋 소스0개. 이번 마지막 커밋은 이 문서만 갱신한다. 최종 HEAD와 clean 상태는 task17의 `evidence/IMPLEMENTATION_HANDOFF.json`에 기록한다.
- 필요한 순서: 공유 `0ccba80eef0da4703cb7ffcd1fb8e22848002b3d` → 룬 `5d0f91c450634d89a747da32463e117155f735a9` → `0380f4085b0c7bebc3c710fa2ffc28376afbaf86` → `6ef4b12b87d1151b5e26108961f7816e75408577`. 공유 커밋이 이미 있으면 재적용하지 않는다. merge 커밋은 별도 cherry-pick 대상이 아니다. 마지막 문서 커밋은 인계 기록용이다.
- 원화: `Assets/HELLSCRIPT/Resources/Art/RuneBoard/Icons/Casual/rune-{attack,magic,support,critical,skill}.png` 및 각각의 `.meta`, 상위 `Casual.meta`. 최초 룬 커밋에 모두 있다. 승인 ID·전체 SHA는 위 표와 `evidence/approved-rune-art-mapping.json`을 사용한다. 기존 GUID·리소스를 삭제하거나 장비359 PNG를 룬 폴더에 복제하지 않는다.
- 남은 공통 API 연결0개. 장비 상세/차트/전투/칙령의 소비 경로는 없다. `StorageSurface.Paint`로 남긴 보드 의미색/XP와 공통 Item 선택 링의 시각 차이는 실제 화면에서 판단할 항목이다.
- 이미 확보한 정적 근거: Runtime834파일·테스트6개 컴파일, UI 계약15개, refresh16 binding, 승인원화5 SHA, Noto3가중치의 필수6기호 확인. Unity·실제 입력·캡처를 수행한 근거로 바꾸어 표기하지 않는다.

### 통합 EditMode 집합에 포함할 필터

어셈블리 `Hellscript.Tests`; 아래는 소스상51 cases이며 실행 통과 수가 아니다.

| 전체 클래스명 | 소스상 cases |
| --- | ---: |
| Hellscript.Tests.RuneBoardSkinTests | 6 |
| Hellscript.Tests.RuneBoardNativeTests | 8 |
| Hellscript.Tests.RuneMasteryTests | 24 |
| Hellscript.Tests.StoreViewBindingTests | 13 |

통합 담당의 단일 검사 목록에 위 클래스들을 합친다. Test Framework `-testFilter`는 세미콜론으로 연결할 수 있다. 테스트 입력에는 `Assets/HELLSCRIPT/Tests/Fixtures/RuneNativeParity.json`, `Docs/Implementation/Rune_Mastery_Catalog.json`, 승인 category5 PNG와 공통 폰트가 필요하다. 필수 glyph assertion과 실패/저장 관련 검사를 제외하지 않는다.

### 동일 통합 개발 플레이어의 룬 스모크 진입점

`Runtime/Presentation/RuntimeRuneBoardSmoke.cs`의 `Install()` → `Start()`가 실행 경로다. `Debug.isDebugBuild`가 필요하다. 통합 개발 플레이어를 재사용하며 화면 전용 앱을 다시 빌드하지 않는다. 기존 개발 빌드 진입점은 `Hellscript.Editor.ProjectBuilder.BuildMac`/`-hellscriptBuildOutput <absolute-output.app>`다. 이 메서드의 `Setup()`이 설정을 갱신하므로 통합 담당만 실행한다. Release 빌드는 이 개발 스모크를 실행하지 않는다.

```text
-hellscriptRuneBoardSmoke
-hellscriptSavePath <fresh-disposable-save-directory>
-hellscriptRuneFixture <RuneGrowthState-fixture.json>
-hellscriptScreenshots <absolute-evidence-directory>
-hellscriptRuneSections layouts,dialogs,mouse,touch,save,practice,gallery
-logFile <absolute-player.log>
```

실제 사용 가능한 fixture는 원본 읽기 기준 저장소의 `Docs/Implementation/MainIntegration20260920Evidence/rune-fixture.json`이다. 160 owned/5 placements와 필수 `skill-pair-1`이 확인됐다. `Prototypes/RuneBoard/vendor/demo-state.json`은 다른 스키마이므로 직접 사용하지 않는다. 사용자 세이브 경로는 절대 지정하지 않는다.

스모크 전용 무음 CLI는 없다. 시작 전에 새 임시 세이브 루트의 `hellscript-audio-v1.json`에 아래 설정을 준비한다. `AudioSettings`/`GameAudio`가 같은 세이브 루트에서 읽는다.

```json
{"version":1,"master":0,"music":0,"effects":0,"muted":true}
```

`result.json`, `passed-sections.txt`, `failure.txt`, 실제 PNG와 종료 코드를 확인한다. 단순 버튼 helper 일부는 `onClick.Invoke`이며 mouse/touch 구간이 실제 InputSystem 경로다. 모든 캡처를 실제 클릭 검증으로 부르지 않는다. Preview는 한 번 캡처 후 자동 종료하지 않으므로 완료형 실행에 혼용하지 않는다.

현재 `layouts`의 자동 크기는440×956/956×440/1440×810/1440×900/1890×810 다섯 개다. **2868×1320은 포함되지 않으며 일반 RuneBoardSmoke에 `-hellscriptViewport`를 추가해도 적용되지 않는다.** 이 인자는 별도 RuneUnlockSmoke 경로만 사용한다. 통합 담당의 2868×1320 추가 캡처가 남았다. 원시 스모크 안전영역은 portrait `Simulate(0,24,0,40)`이며, 앞서 계획한 DPR3 가로 좌우102/하단54 가정과 별개다. 두 시뮬레이션을 실제 iOS 결과라고 부르지 않는다.

실제 시안 상세 검토1/32, 지정 Chrome 프로필 확인, 새 폰트의 실제 선택 크기/영문 잘림, XP·연결선·원형 원화·패널 모서리, 상태·저장·캡처·Library는 통합 수용에 남아 있다. `library_file_id`는 아직 없다. 모델 용량 오류로 중단된 연결 작업은 없다.
