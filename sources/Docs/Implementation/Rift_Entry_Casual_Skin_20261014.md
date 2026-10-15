# 균열 입장 캐주얼 스킨

2026-10-14 · [English](Rift_Entry_Casual_Skin_20261014.en.md)

상태: **소스 동결·통합 인계 문서 완료. 개별 Unity·빌드 계획 종료, 전체 화면 통합 입력에서 검증 예정**. 런타임 통과나 전체 시안 일치를 주장하지 않는다.


## 최종 통합 인계와 소스 동결

사용자의 전체 화면 통합 검증 지시에 따라 개별 실행 슬롯·빌드 요청을 종료했다. 통합 단일 소유자는 `01a13790-39bc-7062-a516-b83a354000a8`이며, `/Users/t8g-2410-pn-005/Documents/Codex/2026-10-14/task-6/skill-skin`의 별도 통합 브랜치와 기존 Library를 제자리에서 사용한다. task-18은 이 문서 커밋 이후 소스 편집을 멈추고 통합 담당의 구체적 결함 요청이 있을 때만 재개한다. 이후의 과거 개별 큐/pending 표기는 이 결정으로 대체한다. 소스·브랜치·원화 삭제는 하지 않았다.

**안전한 최종 소스:** `8ed46741b35071c23633f6bb43bf1d2e97799f5b`, 브랜치 `codex/casual-rift-entry-skin`, repo `/Users/t8g-2410-pn-005/Documents/Codex/2026-10-14/task-18/rift-entry-skin`. 동결 문서 작성 전 worktree clean이며 미커밋 소스0이다. 이 동결 인계를 담은 문서 커밋만 뒤따르며, 그 정확한 최종 HEAD는 task-18의 `evidence/validation-queue.json`과 부모 회신에 기록한다. 모델 용량 오류나 미완료 API 연결은 없고, 이미 존재하는 공통 API 소비는 끝났다.

**현재 통합 확인:** 인계 중 읽기 전용으로 관찰한 task-6 HEAD `873ea29062f19cfd769c3b57ffba7a385b2125b7`은 `118371c7`과 최종 소스 `8ed46741`을 이미 조상으로 포함한다. 따라서 **전용 소스를 다시 적용하지 않고 이번 동결 문서 커밋만 추가**하면 된다. 통합 담당이 계속 작업 중이므로 이 관찰 HEAD로 checkout/reset하지 않는다. 아래 순서는 순수 공통 `0ccba80eef0da4703cb7ffcd1fb8e22848002b3d`에서 복구할 때의 전용 non-merge 이력이다. 현재 통합 브랜치에 다시 cherry-pick하는 지시가 아니다. 일반적으로 최종 Rift 브랜치를 merge하면 공통/기존 전용 조상은 중복 적용되지 않는다. 공통 merge `2e03272a`/`70f76732`를 별도로 cherry-pick하지 않으며 task-18은 활성 통합 checkout을 수정하지 않는다.

1. `302876d1b07acae77d125075f78a5bbe90dee7ba` — 전용 화면 코드·43 PNG·매핑/초기 기록.
2. `ac6f49c08b7dd54a347d447d6da6c59835dd35d0` — 신규 meta 공백 정리.
3. `8c63d13c9638cf963a54066d6293bbe8dd8553d6` — 초기 검증 기록.
4. `118371c77908ddfa1cf57097efa7c6c0c9391b0a` — 공통 창/버튼/패널 API 연결, clip 좌표 보존.
5. `8ed46741b35071c23633f6bb43bf1d2e97799f5b` — 공통 Noto 기호 사용, 전용 마름모 우회 제거.

**소유 경로와 아트:** 코드 `Assets/HELLSCRIPT/Runtime/Presentation/RiftEntryWindow.cs`, `.Popups.cs`, `.CasualSkin.cs` 및 신규 `.CasualSkin.cs.meta`; 아트 `Assets/HELLSCRIPT/Resources/Art/RiftEntry/Casual/`와 폴더 meta; 기록 `Docs/Implementation/Rift_Entry_Casual_Skin_20261014{,.en}.md` 및 `RiftEntryCasualEvidence20261014/`. 아트는 승인42PNG+신규포털1PNG, 총33,952,354 bytes이며 매핑/원본 SHA는 아래 자산 절에 있다. 포털 `rift-entry-portal-casual-v01.png` SHA는 `788b21ce4e5516262a5267b26f9d2853c8585878563f4531ae3c7b9980cc760e`. 전역 구 자산49쌍은 보존한다. 공통 장비359PNG는 공통 커밋이 소유하며 이 전용 sparse 폴더를 Unity 실행 프로젝트로 쓰지 않는다.

**남은 공통 개선:** 유료 버튼 accent API와 `RiftRewardList` skin 인자는 제공되지 않았다. 현재 한 개의 보라색 얼굴 및 세 이름 지정 라벨 어댑터로 동작을 보존했으므로 통합을 막는 미연결 호출은 아니다. 향후 공통 API가 추가되면 구체적 결함/연결 요청으로 재개한다. 장비·차트·전투/칙령 API는 입장 소유 파일에 소비 지점이 없다.

**통합 검증에 넘길 범위:** EditMode `Hellscript.Tests.RiftEntryTests`, `Hellscript.Tests.RiftAdmissionTests`, `Hellscript.Tests.PotionLoadoutTests`, `Hellscript.Tests.StoreViewBindingTests`; 실제 최초 보상 경로는 `Hellscript.Tests.RewardBoxTests`를 포함한다. 런타임 진입은 `RuntimeRiftEntrySmoke.cs`의 `-hellscriptRiftEntrySmoke`와 `RuntimeRiftEntrySmoke.Pickers.cs`의 `-hellscriptRiftPickerSmoke`. 준비/중단 사냥 관련 결함이 나오면 `.Preparation.cs`의 `-hellscriptRiftPreparationSmoke`/`-hellscriptRiftEntryParitySmoke`, `.Portal.cs`의 `-hellscriptRiftPortalSmoke`를 같은 통합 빌드에서 선택한다. 개발 플레이어와 작업 전용 `-hellscriptSavePath`, `-hellscriptEvidencePath`, 무음이 필요하다. 기본 입장 smoke 자체는 세이브 격리 인자를 강제하지 않고 자체 음소거도 없으므로 통합 실행자가 반드시 설정한다. PickerFlow의 마지막 크기는1680×720이다. 현재 harness는 첫 분기만 실행하므로 여러 smoke flag를 한 프로세스에 나열해 모두 실행됐다고 보고하지 않는다. 통합 담당이 하나의 통합 검증 계획에서 순서를 정한다.

기존 입장 smoke의 기본 행렬은440×956·956×440·1600×900·1600×1000·2100×900, KO/EN이며 **2868×1320은 포함되지 않는다**. 통합 검증에서 그 해상도와 실제 Canvas 배율, Mac DPR3 안전 여백 가정(좌우102/하단54/상단0), 새 ◆◇ 범례 폭·잠김 표식, 둥근 clip·게이지·영문 잘림, 클릭/터치/스크롤·교환/해제·즉시 저장/오류·입장 취소·최초 보상 상태를 추가 확인한다. 시안 DOM32/확대1, 포털의 풍부한 빛결, 세로 아트 스트립 유지, 다른 소유자의 SkillIconView/보상 상세 차이는 여전히 남는다. 실제 플레이어 after 캡처·실기기 검증은 없다.

기존 정적 근거(Runtime834, 오류0/경고47, UI계약15/15, refresh16/193)를 재사용하며 이번 동결 인계에서 생성·소스 검사·Unity·빌드를 반복하지 않았다. 통합 소스가 바뀐 뒤의 최종 검증은 통합 담당이 새 입력으로 기록한다. 작은 기록·원화·소스와 task-18의 기존 정적 출력은2026-10-28 검토까지 보존한다.

## 공통 후속 통합 — 0ccba80e (소스 최신 기록)

부모 지정 `0ccba80eef0da4703cb7ffcd1fb8e22848002b3d`를 merge `70f767327ddbfa7f825f5122bb326bf4ffae5186`로 가져왔다. 기존 커밋 `118371c7`과 원화·동작을 보존했으며, 아래 첫 API 연결 기록보다 이 상태가 우선한다.

- 실제 Noto400/700/900 cmap에서 `◆◇`를 확인했다. 전용 문자열 치환과 `RiftEntryCasualMarkerGraphic` **31줄을 제거**하고 원래 범례·잠김 문자열을 공통 폰트로 표시한다. 폰트 크기·좌표·기존 fitting 규칙은 바꾸지 않았다.
- 새 기호 윤곽은 이전 임시 벡터보다 넓다. KO/EN 피로도 범례의 폭/잘림과 잠긴 스킬 슬롯 표식은 Unity 화면에서 확인해야 한다. 임의 축소로 상쇄하지 않았다. 보조 본문은 이미 사용하는 `UiSkin.Casual.Muted`를 통해 공통 대비 보정을 받는다.
- 균열 입장에는 장비·차트·전투 HUD·칙령 표시 API 소비 지점이 없다. 장비 provider 및 359 PNG는 공통 커밋의 Git 이력에 포함하되, 소스 전용 sparse 작업 폴더에 장비 PNG를 추가 전개하거나 화면 전용 경로에 중복 복사하지 않았다. 부모의 공통 검증 프로젝트가 그 Resources를 제공한다. 코인·물약·상자·포털 매핑은 유지한다.
- 유료 버튼 accent와 `RiftRewardList` skin API는 이번 후속에도 없다. 보라색 버튼 하나와 세 중립 라벨 어댑터는 유지하며, 금색 제목·표면색까지 Navy로 바뀌는 `TextColor` 일괄 치환을 하지 않았다. 공통 소스·테스트·폰트 변경 28개는 지정 SHA와 byte 단위 일치한다.
- **최종 정적 결과:** Runtime 834개 소스, Roslyn 오류0·경고47(CS0108 1/CS0618 45/CS0414 1), 기존 reference296개 제자리 읽기. UI 계약15/15, refresh16 bindings/193 소스, diff 공백 검사 통과. [후속 검증 근거](RiftEntryCasualEvidence20261014/common-followup/summary.json)에 입력 SHA·폰트·원화·로그를 보존했다. 전용 소스 수정은 CasualSkin 파일 하나뿐이다.
- 승인 원화42/42 SHA 및 포털 해시 유지. 새 생성·Unity 실행·Library 재업로드0. 시안 확인 수32 DOM/1 확대는 그대로다. 07:50 외부 Unity PID30116 실행은 부모 전달 사실이며 프로세스를 조작하지 않았다. **첫 슬롯 후보지만 아직 pending**이고 부모의 슬롯·실행 프로젝트 지정 후 관련 검증을 수행한다.

## 공통 API 첫 연결 기록 (2026-10-14)

부모가 지정한 `cdb9c140fbf8b778bd3c0bbc0fbcc68a5d9e3fa8`과 선행 문서를 로컬 merge `2e03272a529bf24336042608f8740e30f40ea7f3`로 반영했다. 기존 균열 커밋 `8c63d13c`의 기능·원화는 보존했다. 아래 초기 구현 기록보다 이 후속 상태가 우선한다.

- 메인·팝업은 `ContentWindowView.Open(..., skin:UiSkin.Casual)`을 사용한다. 일반/선택 버튼과 도움말은 `UiButtonChrome.Casual`, 닫기는 `CasualClose`로 연결했다. 전용 frame과 일반 버튼 렌더러, 전체 자식 Text 일괄 변경, 버튼 목록 순회 갱신은 제거했다.
- 피로도 판·빈 스킬 칸·슬롯 번호 판은 `StorageSurface.SetSkin`을 사용한다. 게이지/투명 레이어에는 적용하지 않아 의미색과 마스크를 보존한다. 메인 Scroll/Actions는 공통 둥근 clip 아래에서 frame과 같은 pivot·전체 크기를 유지한다. clip inset은 frame의 border+shadow에 해당하는 `7*frameLine`으로 가장자리 장식의 덮임을 방지한다. 원점 차이와 하단 그림자 덮임은 소스 리뷰에서 발견해 수정했으며 실행 픽셀 수용은 남아 있다.
- 표시 폰트 fallback은 공통 `LabelFont`를 사용한다(Jua에 없는 기호는 Noto400). 명시적 강조 Noto700과 ◆/◇ 벡터 표식은 유지한다. Success/Danger/유료 본문색은 공통 의미색·OnLight 경로로 연결했다. 공통 `RiftRewardList`에는 아직 skin 인자가 없으므로 그 목록의 세 중립 라벨만 화면 쪽에서 명시적으로 맞춘다.
- **남은 공통 요청:** `ApplyChrome(button,bool close)`에는 유료 행동 팔레트 인자가 없다. `rift-enter-fast` 하나만 보라색 면 전용 `RiftEntryPaidButtonGraphic`을 유지한다. 원래 UiButton의 역할·입력·비활성·포커스는 그대로다. 일반 버튼까지 되돌리거나 역할을 속이지 않았다. 공통 담당에게 optional paid/semantic accent API와 `RiftRewardList` skin 입력을 요청한다.
- `WithEquipmentArt`는 장비 공급 계약이다. 이 화면의 코인·물약·상자·포털을 그 계약에 억지로 넣지 않았으며 기존 검증 매핑을 유지했다. 새로운 공통 원화 경로·전투/차트 API는 추측하지 않았다.
- 승인된 공통 파일은 가져온 커밋과 byte 단위로 동일하며 직접 편집 0. 전용 변경은 기존 세 RiftEntryWindow 파일뿐이다. 병합 후 sparse 설정이 전용 원화 경로를 제외한 것을 검사에서 발견해 작업 폴더에 다시 포함했다. 승인 PNG 42/42와 포털 해시는 보존됐다. 최종 checkout 파일 논리 크기는 약 104MB(디스크 할당 약 111MiB)이며 Unity 캐시가 아니라 소스·Git 객체·원화·문서다.
- 이번 컴파일은 기존 Unity reference 296개를 제자리에서 읽는 **Roslyn 정적 컴파일**이다. 831개 Runtime 소스 대상이며 Editor/Player를 실행하지 않는다. 바이너리·rsp는 task-18/evidence/static-common-api에만 생성한다. 최종 결과는 **오류 0, 경고 47**(CS0108 1·CS0618 45·CS0414 1)이며 공통 기준과 같은 경고 분포다. 현재 831개 소스 해시가 컴파일 영수증과 일치한다. UI 계약 **15/15**, refresh 감사 **16 guarded bindings / 192 UI 소스**, `git diff --check`도 통과했다. [검증 요약](RiftEntryCasualEvidence20261014/common-api/summary.json)과 같은 폴더에 최종 로그·입력 해시 영수증을 보존했다. Unity 또는 EditMode 실행 결과가 아니다.
- 검증 큐는 여전히 **pending**이다. 부모가 외부 Unity PID30116의 실행을 알렸고 슬롯을 배정하지 않았다. 그 프로세스를 조작하지 않았고 이 작업의 Unity 시작은 0회다. 기존 보고서의 before/after 및 32/1 시안 확인 수·Library ID는 변하지 않는다.

## 입력과 소유 경계

- Claude 시안: https://claude.ai/artifact/Tzz2Knnx2nSZbgR1C5v4rp
- 사용자 제시 32개, DOM으로 읽은 아트보드 32개, 실제 확대한 화면 **1개(PC 16:9 KO)**. 단계 24→25, 다음 단계 잠김, 1.5x 확인/취소, 물약 선택창 열기/닫기를 확인했다. 나머지 31개는 목록·상태 조사이며 픽셀 확인이 아니다.
- 기준 `2e193c336c6176d80b9ddbdff7269246bf2590fb`; 검증된 게임 코드 `4139a77e2df495cee009c07c6094b2eea9b1aeaa`. 초기 구현 당시 공통 문서 HEAD `df2e366c18ba01eaf8a2ceaa145dfb8c940e5f7a`까지 Presentation 소스 차이는 0이었다. 현재 적용 API 기준은 위 `cdb9c140`이다.
- 브랜치 `codex/casual-rift-entry-skin`, 작업 경로 `/Users/t8g-2410-pn-005/Documents/Codex/2026-10-14/task-18/rift-entry-skin`.
- 변경 소유 파일: `RiftEntryWindow.cs`, `RiftEntryWindow.Popups.cs`, 신규 `RiftEntryWindow.CasualSkin.cs`와 meta. 추가 산출물은 화면 전용 Art/RiftEntry/Casual과 이 보고서·근거다.
- 공통 테마·렌더링 직접 편집 0(부모 승인 API 커밋은 가져옴). 도메인·로컬라이징·프로젝트 설정 수정 0. 원본 checkout/staging 수정 0. 출석 파일럿 제외. Library/Temp/Obj/Builds 캐시 복제·Unity 실행 0. 초기 sparse 문서 선택이 포함했던 추적 증거 사본은 sparse 설정으로 제외했고 초기 소스 작업 공간은 약 63MB였고, 공통 API 병합 후 현재 파일 논리 크기는 약 104MB다.
- 시안 재열기 필요 시 사용자가 지정한 Chrome 프로필을 먼저 확인한다. 이번 구현 구간에는 브라우저를 다시 열지 않았다.

## Before → after (소스 기준)

| 항목 | 이전 | 이번 구현 |
|---|---|---|
| 배경·본문 | 어두운 성역 배경, 기존 폰트 | 크림 판, 하늘색 아트 칸, Noto400 본문·700 강조, Jua 제목/숫자 |
| 버튼 | 공통 어두운 Minimal 얼굴 | 같은 UiButton 위 공통 밝은 둥근 얼굴, 녹색 입장·코랄 닫기·상태 표현; 보라색 1.5x 면만 전용 어댑터 유지 |
| 원화 | 글로벌 코인·기본 물약·상자 | 승인 원화 42개를 전용 경로에서 매핑, 신규 보라 포털 1개 |
| 피로도 | 사각 채움 | 동일 용량/폭 계산, 둥근 마스크 안에 일일·유료 채움 제한 |
| 글자 기호 | 기존 시스템 폰트 기호 | Jua 누락 기호는 공통 Noto400 fallback, 후속 Noto에 보강된 ◆/◇는 원래 문자열 그대로 표시 |

외곽 창, 히트 사각형, 열/행, 가격, 지급량, 세이브 명령은 유지했다. 폰트 적용은 preferredHeight 측정 전에 한다. 기존 S fit·best-fit 설정을 추가로 낮추거나 폭을 확대하지 않았다. 새 폰트가 기존 fit 결과에 미치는 실제 영향은 런타임에서 S와 글자 크기를 비교해야 한다. 위 표는 실행 화면 before/after 캡처가 아니다.

시안 물약 선택은 일부 장착 항목을 비활성화하지만 실제 게임의 **즉시 저장·같은 물약 해제·다른 슬롯 교환**을 보존했다. 시안의 최초 보상 placeholder 대신 실제 RewardBoxCatalog/FirstClearRewards·수령 연결을 유지했다. 시안에 없는 보충, 반복 설정, 전투 기록, 가방 안내, 입장 대기/취소, 중단된 사냥 포탈/재시작, 다음 콘텐츠 안내도 유지했다. 전투·결과·상세 보상 창의 스킨은 다른 담당 경계다.

## 자산과 재사용

- 승인 매핑: [applied-art-mapping.json](RiftEntryCasualEvidence20261014/applied-art-mapping.json). 42/42 SHA 일치: 코인1, 기본 물약3, 엘릭서36, 상자2.
- 총 43 PNG **33,952,354 bytes**, 신규 import GUID 43개 모두 고유. 기존 meta/GUID는 손대지 않았다.
- 포털 소유 ID `rift-entry-portal-casual-v01`, `Assets/HELLSCRIPT/Resources/Art/RiftEntry/Casual/rift-entry-portal-casual-v01.png`.
- 포털 SHA256 `788b21ce4e5516262a5267b26f9d2853c8585878563f4531ae3c7b9980cc760e`, 1,097,923 bytes, 실제 1254×1254 RGBA, alpha 0–254. built-in image_gen 제작. 승인 목록에 대체본이 없어 배정받은 제작 범위로 만들었으며 **부모 시각 검토는 남아 있다**. 전체 프롬프트는 근거 폴더에 보존했다.
- 코인 파일명은 기존 smoke의 sprite 이름 계약 `currency-abyssal-coin`을 유지한다. 포털은 다른 담당이 위 파일/해시로 재사용할 수 있다.
- 구 리소스 49 PNG/meta는 동적 Resources 참조가 타 화면에 남아 모두 보존했다. 전역 코인·물약·상자 교체나 삭제 0. [참조 감사](RiftEntryCasualEvidence20261014/legacy-art-reference-audit.json).

## 검사와 Library

- `python3 tools/check_ui_contract.py --verify --receipt ../evidence/ui-contract-receipt.json`: 최종 15/15 통과. 입력 해시 영수증 보존.
- `python3 tools/check_ui_refresh.py --inventory ../evidence/ui-refresh-inventory.json`: 16 guarded bindings, 192 UI 소스 통과.
- `git diff --check`: 최종 통과. 첫 staged 검사에서 신규 meta의 후행 공백을 발견했고 46개 신규 meta를 정규화했다. 이 실패는 수정 후 기준 SHA 대비 전체 diff 검사로 해소했다. 실제 TTF cmap 검사에서 기호 누락을 찾고 해결했다. 독립 소스 검토에서 추가 확정 결함 없음. 이 초기 검사만으로 C# 컴파일을 주장하지 않았으며, 공통 API 후속에서 별도 Roslyn 컴파일을 통과했다.
- 시안 핵심 캡처 1325×746: Library `libfile_f1f61ec6c4a48191a6f5659566bc34da`, current version 1. 초기 빈 캡처는 올바른 네이티브 캡처로 교체했다.
- 새 포털: Library `libfile_c60dd4966560819186473c6a951e9fcb`, version 0.
- Unity 실행 화면 after 캡처·Library ID는 아직 없다. Mac 시뮬레이션을 iOS 실기기 검증으로 부르지 않는다.

## 부모 검증 큐 요청과 남은 차이

상태 **pending**: 부모가 단일 Unity 슬롯과 실행 프로젝트를 지정해야 컴파일/플레이어를 실행한다. 자동 승인 거절이나 불확실한 실행 결과는 아니다. 세이브는 일회용, 오디오는 무음으로 실행한다.

관련 EditMode 우선 필터: `RiftEntryTests|RiftAdmissionTests|PotionLoadoutTests|StoreViewBindingTests` (실제 보상 경로 확인 시 `RewardBoxTests`). Runtime 우선 `-hellscriptRiftEntrySmoke`, `-hellscriptRiftPickerSmoke`; 준비/중단 사냥 회귀는 관련 .Preparation/.Portal 시나리오를 사용한다. 테스트 소유 파일을 변경하지 않았다.

PC 16:9/16:10/21:9와 모바일 세로/가로, KO/EN, **2868×1320**을 포함해야 한다. Mac DPR3 가정의 좌우102·하단54·상단0 안전 여백은 가정이며, 실제 Canvas scale과 S를 기록한다. 비활성/선택/빈 항목/저장 오류, 클릭/터치/스크롤/뒤로 복귀, 물약 교환·해제, 지불 확인 취소·입장 대기, 최초 보상 상태를 검증한다. 둥근 게이지 넘침·패널 모서리·아이콘 끝·영문 잘림·기호 글리프 정렬을 우선 본다.

생성 포털은 시안의 단순 타원보다 빛의 결이 풍부하다. 세로에서는 기존 아트 스트립 배치를 유지하려고 포털을 낮은 불투명도로 배경에 둔다. 공유 SkillIconView의 프레임과 다른 담당 보상 상세 스킨은 이번 변경 밖이다. 실제 31개 미확대 아트보드, 픽셀 일치·실기기·성능·접근성 수용은 미검증이다.

공통 규칙 제안: 공통 기호 범위와 보강 전후의 실제 가독성을 기록하고, 제공된 캐주얼 API에 보라색 유료 행동 accent와 RiftRewardList skin 인자를 추가하면 남은 전용 어댑터를 제거할 수 있다. 현재 고정 430 look·글꼴 파일은 변경하지 않았다.

통합 전 소스·원화·캡처와 작은 JSON/log만 유지하며 2026-10-28 부모 검토 대상으로 기록했다. 원격 push·공개 공유·main 병합·웹 빌드는 실행하지 않았다.
