# 균열 추천 — 공통 후속 검토

2026-10-14. 공통/원본 저장소와 화면 소스를 읽기 전용으로 검토했다. Unity·Chrome·컴파일·회귀 테스트·import 스크립트 실행, 게임 코드 편집 없음. 기존 검토 기록을 보존했다.

검토한 화면 기준은 `rift-skin`의 clean HEAD **`35fd4986c0d4bf0779dd067dbb3c1850a9b20fac`**다. 공통은 `task-6/skill-skin`의 **`0ccba80eef0da4703cb7ffcd1fb8e22848002b3d`** git 객체와 최신 한국어 handoff/후속 영수증을 읽었다. 범위는 균열 추천에 필요한 공통 상세·폰트·탭·원화 공급·import 의존성이다. 새 chart/world API 전체를 독립 승인한 검토가 아니다.

## 결론

이전 네 가지 **상세 Text 배치 분기 차이는 소스상 복원됐다.** 제공된 회귀는 아직 실행 0이므로 실제 화면 배치 수용 완료로 바꾸어 보고하지 않는다. 현재 소비 3파일의 스킨 전달은 이미 연결되어 있어, 캐시한 `RiftGearSkin.Current`에 승인 공급자를 붙이는 한 곳이 필수 변경이다. 이 범위에서 추가 소스 병합을 막는 결함은 발견하지 않았다.

## 정확한 커밋 의존성

`0ccba80e` 한 커밋 자체는 전투/차트 스킨이다. 요청된 수정·원화는 아래 선행 커밋에 있으므로 tip **단독 cherry-pick**은 충분하지 않다. 부모가 누적 branch를 병합하거나 의존 커밋을 순서대로 포함해야 한다.

| 순서 | 전체 SHA | 균열 추천 관련 내용 |
|---|---|---|
| 기존 | `cdb9c140fbf8b778bd3c0bbc0fbcc68a5d9e3fa8` | opt-in UiSkin 및 장비 View 전달 |
| 후속 1 | `6cdf2df690941b7c9f751c39235ea36ce4ff9ed9` | 상세 geometry, 탭 대비, Noto 기호 |
| 후속 2 | `f25e67aab62d72f9792b5c0d49e6692bfbca5eed` | CasualEquipmentArt, 승인 359 PNG, importer/budget 경로 |
| 누적 tip | `0ccba80eef0da4703cb7ffcd1fb8e22848002b3d` | combat/chart API 및 최신 인계/검증 기록 |

## 네 배치 차이 재검토

아래 파일 경로 prefix는 `Assets/HELLSCRIPT/Runtime/Presentation/`이며, 모두 `ItemDetailView.Style.cs`다.

| 이전 caveat | 후속 소스 확인 | 판단 |
|---|---|---|
| 점수 plaque가 일반 Line으로 바뀌며 13/16pt→17pt, 높이52/64 손실 | `Head`가 Skin과 관계없이 기존 plaque 조건을 사용. 기존 `tight?13:Heading+1`, 높이 `tight?52:64`, y 증가와 `ScorePlaque` cap/폭 식을 유지. 그림만 StorageSurface로 교체 | 해당 명시 크기·배치 복원 |
| major divider의 art 높이 `h+9`→고정23 | divider art 존재/크기 판정은 유지하고 동일 `h`, `y+=4`, `y+=h+5`. Casual은 선만 그림 | 해당 간격 복원 |
| greater-affix inset16→12 | `star=bullet&&line.greater?ItemCardArt.Star:null`은 Skin과 무관, `inset=star!=null?16:...` 유지. Casual 표식은 diamond | 본문 inset 복원. 표식 픽셀 모양은 의도된 스킨 차이 |
| socket art24/inset28→mark16/inset22 | socket 리소스 존재를 기존처럼 판정. Casual mark도 `x-2,y-1,24,24`, gem 위치/크기 및 inset28 유지 | 리소스가 있는 기존 분기의 크기·배치 복원 |

**옛 장식 리소스는 아직 layout 의존성이다.** `ItemCardArt.Plaque`는 `Art/ItemCard/badge`의 sprite border/치수, Divider는 실제 texture 치수, Star/Socket은 존재 여부를 읽는다. PNG를 Casual에서 화면에 그리지 않는다는 이유로 badge/divider/star/socket을 정리하면 fallback 분기와 배치가 바뀐다. 복원은 동일 리소스 입력을 전제하며 새 geometry 상수로 분리한 것이 아니다.

추가된 `SkinKeepsDetailTextSizesAndLayoutSlots`는 `(420,700,false)`, `(420,380,false)`, `(420,270,false)`, `(220,500,false)`, `(220,500,true)`, `(640,700,false)` 여섯 조합에서 같은 explicit `UiFonts.Body`로 Text 개수/이름/fontSize/anchoredPosition/rect.size를 비교한다. geometry만 분리하는 유효한 회귀이지만 실행 전이며, Noto 교체에 따른 자연 줄바꿈·새 원화·비Text 표식·모든 화면비율을 검증하지는 않는다. 이전 내부 mount radius8/art inset1의 모서리 검증도 여전히 실제 PNG 렌더에서 확인해야 한다.

## 탭·보조 글자·폰트

- `ContentWindowView.Tab`은 선택 여부와 관계없이 Navy로 생성한다. 기존 `UiTheme.Choice`만 호출해도 Muted가 금색 선택 배경 위에 남지 않는다. 소비자의 추가 compare `ApplyLabel`은 이제 중복이지만 안전하며 제거가 필수는 아니다. 새 `ATabSelectedAfterCreationKeepsItsReadableLabelAndClickHandler`가 선택/해제와 click 1회를 검사하지만 아직 실행 전이다.
- `UiSkin.Muted` 자체가 `OnLight(rawMuted)`다. 소비자의 `OnLight(skin.Muted)`는 이미 목표 대비를 만족하면 그대로 반환하므로 반복해서 더 어둡게 만들지 않는다. 최소 변경에서는 유지 가능하며 정리한다면 skin.Muted만 써도 된다. 비활성 seconds/input도 공통 Muted 변경 영향을 받으므로 이전 대비 영수증 수치를 이 새 입력의 검증값으로 재사용하면 안 된다.
- Noto400/700/900 cmap를 git 객체에서 독립 확인했다. 요청 `≠①②ⓘ□◆＋×`는 모두 존재하고 15 codepoint 추가, 이전 codepoint 삭제 0. Jua와 네 폰트의 `.meta`는 byte 동일하다. [cmap 근거](common-followup-font-cmap.json)에 새 폰트 SHA/크기/추가 목록을 기록했다. 기존 glyph 윤곽·advance·hint·줄 메트릭 동일은 공통 소유자의 후속 영수증 결과를 참조하며 여기서 그 검사를 중복 실행하지 않았다.
- 현재 로컬 `＋`→`+`는 더 이상 전체 번들 미지원 문제의 필수 우회가 아니다. 유지하면 이전 소비 시각을 유지하고, 제거하면 원문 `＋`와 Noto400 fallback을 사용한다(Jua는 여전히 ＋ 없음). 어느 경우도 Loc 원본 키·fontSize를 바꾸지 않는다. 필수 provider 연결과 분리해 판단할 수 있다. 원문 복원을 택하면 라벨 전체 Jua→Noto400의 실제 폭을 확인한다.

## 최소 소비 변경과 원화 계약

`RiftRecommendationWindow.cs` 안의 기존 화면 adapter 한 곳을 다음과 같이 한 번 생성해 캐시하면 된다.

```csharp
public static UiSkin Current { get; } =
    UiSkin.Casual.WithEquipmentArt(CasualEquipmentArt.Provider);
```

`=> UiSkin.Casual.WithEquipmentArt(...)`를 매 접근마다 호출할 필요는 없다. 기존 `GameUI.GearRecommendations.cs`의 slot 및 두 창의 Open→`v.Skin`→comparison/Append 전달이 모두 이 인스턴스를 받는다. 생성 후 RawImage 순회 교체, 전역 EquipmentArt 교체, 새 ID 조회, `ConfigureCombatSkin` 호출은 균열 추천 연결에 필요 없다. 타이머·저장·장착 callback 수정도 없다.

실제 제공 클래스는 `CasualEquipmentArt`이며 승인 receipt는 `casual-equipment-20261014-359:qa-sha256:619e9fb629ebb03ad0ea78f7bb6f172fb5b0a24c39b59a1798324e33e4d74f4b`다. runtime root는 **`Art/Casual/Equipment/`**, 확장자 없는 경로다. handoff/receipt는 item key330(별도 파일이 없는 B28→B32 alias 포함), emblem30, PNG359/69,753,836 bytes를 기록한다. receipt 구조와 총합만 읽었으며 이번 차수의 승인 SHA/GUID/원본↔목적지 전수 검증은 부모 소유다.

`EquipmentArtProvider` 우선순위는 새 special→기존 special→새 base이고 최종 공유 fallback이 기존 base/atlas를 보존한다. B28은 B32를 명시 매핑한다. 세트 emblem도 공급자→기존 경로이며 각인 원화 정체성과 카탈로그 잠금 상태를 바꾸지 않는다. `playerEnabled=false` 147개를 사용자 획득 가능으로 바꾸는 구현도 이 변경에 없다.

아트 공급 커밋에는 `EquipmentVariantArtImporter.cs`의 새 경로(기본 Texture2D, alpha, no mipmap,512)와 `ResourceTextureBudget.cs`의 새 경로(플랫폼512/ASTC, 기존 budget 체계)가 들어 있다. 이 공통 의존성을 PNG/클래스에서 분리해 놓치지 않아야 한다. Resources 자산은 화면 opt-in 여부와 무관하게 빌드 포함량을 늘리며 66.522MiB는 PNG 원본 합계이지 최종 빌드/런타임 메모리 실측값이 아니다. 실제 Unity import와 Texture2D 반환·대표 렌더는 아직 대기다.

## 남은 검증과 경계

공통 후속 영수증은 Runtime/Editor/Tests 정적 컴파일 오류0, UI계약15·refresh16 binding 통과를 기록한다. 이 검토가 새로 실행한 결과가 아니다. 신규 Unity21사례 중 geometry6·탭1·chart14는 **실행0**이다. 첫 API/이전 소비의 실행 결과를 새 공통 입력의 결과로 간주하지 않는다.

부모의 병합 및 전수 원화 검증 후 필요한 것은 동일 입력을 고정한 관련 컴파일/공통 geometry·탭 검사, KO/EN 화면과 실제 스크롤/선택/저장 동작·대표 새 원화 렌더다. 2868×1320 포함 비율, Noto fallback, 긴 영문, 비교 왼쪽/오른쪽, rounded mount, 빈/비활성 상태를 포함한다. 전체 회귀를 이 하위 검토에서 다시 시작하지 않는다.

Unity 실행은 부모 슬롯 **대기**다. handoff의 마지막 외부 관측은 07:50:38 UTC PID30116 `scratchpad/proj-final`이며 이 검토에서 새 프로세스 상태를 확인하거나 외부 작업을 종료하지 않았다. Chrome 실행도 금지 범위를 준수했다. 공통 파일·원본 repo·소비 게임 코드는 수정하지 않았다.

## English handoff

The four reported detail Text-layout regressions are restored in source by ancestor `6cdf2df6`; actual six-case geometry tests remain unexecuted. Tip `0ccba80e` alone does not contain the provider/font fixes as a standalone cherry-pick: include the ancestor chain, especially equipment commit `f25e67aa`.

The only required screen consumer change is a cached `RiftGearSkin.Current = UiSkin.Casual.WithEquipmentArt(CasualEquipmentArt.Provider)`. Existing slot/window/detail/comparison propagation already covers the screen. Keep the legacy ItemCardArt badge/divider/star/socket resources because their dimensions/presence still determine layout. Noto symbol coverage is independently confirmed from cmap; Jua and font metadata remain unchanged. Existing plus normalization is now optional and changing it changes the label's fallback font. Provider source/hash verification belongs to the parent, while Unity import/render acceptance remains queued. No blocking source issue was found in the reviewed scope; no game code, Unity, Chrome or shared files were changed.

## 병합 후 최종 소비 diff 추가 검토

부모 병합 HEAD **`b22a82c220884e33c88aeacc6ad649b3b3af3107`**의 3파일 dirty diff를 추가로 읽었다.

- `Current {get;}=UiSkin.Casual.WithEquipmentArt(CasualEquipmentArt.Provider)`로 한 번 생성하는 연결이 들어 있다. 기존 전달경로를 사용하며 callback/ID를 바꾸지 않는다.
- compare `ApplyLabel` 제거는 새 공통 Tab의 항상 Navy와 호환된다. active part/Note/Unit의 중복 `OnLight(Muted)` 제거도 새 Muted 구현과 호환된다.
- `＋` 치환과 중복 ApplyLabel을 제거하여 Loc 결과를 원문대로 표시한다. 새 Noto400에 해당 기호가 있으며 공통 LabelFont는 이를 fallback으로 사용한다. KO 해당 버튼이 Jua에서 Noto400으로 바뀌므로 실제 글자 폭은 여전히 렌더 확인 대상이다. 직접 fontSize/RectTransform/callback 식은 이 추가 diff에서 변경되지 않았다.
- 해당 소비 diff에서 추가 소스 결함은 발견하지 않았다. 공유 font 입력이 달라졌으므로 이전 전체 폰트/대비 수치를 새 입력에서 직접 실행한 것처럼 재사용하지 않는다.

추가 검토 SHA256:

| 파일 | SHA256 |
|---|---|
| GameUI.GearRecommendations.cs | `f4feec49401fa98c910d7d5663f36f23e8a6395adc7f0c3769104f165d8338f5` |
| RiftRecommendationWindow.cs | `8216d58cc70795a7519a0517e59a75f8a29450b6deb3d0e43be0f4f0b9613490` |
| RiftGearSettingsWindow.cs | `90cd5ee6ea534a6a4502fe0fe6a72a5d2b22a92b5d3390ad53e5a658639ac18a` |

## 기존 Roslyn 재사용 명령안 — 실행하지 않음

기존 template은 `/Users/t8g-2410-pn-005/Documents/Codex/2026-10-14/task-6/skill-skin/Builds/Validation/CasualSkillSkin/dotnet/common-followup/Runtime.rsp`이며 SHA256은 `b06c1dd8202f8000891f55bbdea1f8840e7304e668fa1d73168d588534e5ab07`다. [읽기 전용 사전 확인](common-followup-compile-preflight.json)에서 reference **296개 모두 존재**, 화면 작업 폴더의 Runtime **833개 source와 RSP source 목록 정확히 일치**, 누락·추가 0을 확인했다. `langversion:9.0`, 기존 UNITY defines와 reference 구성을 그대로 재사용할 수 있다.

실제 설치된 compiler 후보는 `/usr/local/share/dotnet/dotnet` + `/usr/local/share/dotnet/sdk/10.0.401/Roslyn/bincore/csc.dll` 또는 Unity의 `Contents/Resources/Scripting/DotNetSdk/dotnet` + `.../sdk/8.0.318/Roslyn/bincore/csc.dll`이다. 파일 존재만 확인했고 version/compile 명령은 실행하지 않았다. 기존 receipt는 성공 로그/RSP를 보관하지만 정확한 과거 shell 명령은 그 파일에 없으므로 아래는 **재사용 제안**이다.

GameUI가 여러 partial에 걸쳐 private 멤버를 사용하므로 세 파일만 별도 assembly로 컴파일하는 것보다 Runtime assembly 한 번이 최소의 신뢰 가능한 정적 컴파일 범위다. Tests/Editor compile·Unity·전체 테스트를 실행할 필요가 있다는 뜻은 아니다. reference는 기존 cache에서 읽기만 하고 복사하지 않는다.

부모가 실행을 결정하면 먼저 전용 RSP를 생성한다. 다음 명령안은 공통 source/RSP/output을 수정하지 않는다.

```bash
python3 - <<'PY'
from pathlib import Path
shared = Path('/Users/t8g-2410-pn-005/Documents/Codex/2026-10-14/task-6/skill-skin')
local = Path('/Users/t8g-2410-pn-005/Documents/Codex/2026-10-14/task-10/rift-skin')
output = local.parent / 'evidence/roslyn-current-runtime'
output.mkdir(parents=True, exist_ok=True)
template = shared / 'Builds/Validation/CasualSkillSkin/dotnet/common-followup/Runtime.rsp'
lines = []
for line in template.read_text().splitlines():
    if line.startswith('-r:'):
        ref = Path(line[3:].strip('"'))
        line = '-r:"' + str(ref if ref.is_absolute() else shared / ref) + '"'
    elif line.startswith('"Assets/') and line.endswith('.cs"'):
        line = '"' + str(local / line.strip('"')) + '"'
    elif line.startswith('-out:'):
        line = '-out:"' + str(output / 'Hellscript.Runtime.dll') + '"'
    elif line.startswith('-refout:'):
        line = '-refout:"' + str(output / 'Hellscript.Runtime.ref.dll') + '"'
    lines.append(line)
(output / 'Runtime.rsp').write_text('\n'.join(lines) + '\n')
PY
```

이후 별도의 정적 compiler 명령은 아래 한 번이다. 로그/종료 코드를 확인해야 하며 compiler 성공이 실제 Unity import/플레이 수용을 뜻하지 않는다.

```bash
/usr/local/share/dotnet/dotnet \
  /usr/local/share/dotnet/sdk/10.0.401/Roslyn/bincore/csc.dll \
  -noconfig \
  @/Users/t8g-2410-pn-005/Documents/Codex/2026-10-14/task-10/evidence/roslyn-current-runtime/Runtime.rsp \
  > /Users/t8g-2410-pn-005/Documents/Codex/2026-10-14/task-10/evidence/roslyn-current-runtime/runtime-compile.txt 2>&1
```

위 RSP 생성·컴파일 두 명령 모두 이 하위 검토에서는 실행하지 않았다. 실행 전에 부모가 기록하는 최종 source hash가 이 검토 이후 변하지 않았는지 확인해야 한다. source와 ref 경로를 변환하지 않은 공통 RSP 원본을 로컬 CWD에서 그대로 실행하면 잘못된 입력 또는 출력 경로를 사용할 수 있다.

English addendum: After merge `b22a82c2`, the cached provider and removal of now-redundant label/color workarounds are compatible; no new consumer-source issue was found. Removing plus normalization restores the original label and selects Noto400, so native width remains to be checked. The existing Runtime response file has 296 available references and exactly matches all 833 local runtime sources. A proposed isolated response-file/compiler command is provided above; neither preparation nor compilation was executed. No Unity, browser or shared output was touched.

## Final integration direction

The later parent instruction supersedes the standalone compile/Unity proposals above: all screens will be tested together by the common owner in task-6/skill-skin on a separate integration branch using the existing Library. No local RSP was created and no compiler or Unity was run. This review is complete; further source edits require a specific integration defect request.
