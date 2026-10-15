# 균열 추천 공통 API 소비 검토

2026-10-14. 읽기 전용 소스 검토. 게임 코드 수정·Unity 실행·컴파일·전체 정적 회귀 재실행 없음.

- 공통 입력: `cdb9c140fbf8b778bd3c0bbc0fbcc68a5d9e3fa8` (22개 파일).
- 소비 코드 관측: `rift-skin` HEAD `f02d6f4c4e2fce6d923a13891888b32fc9e6eacf`, 공통 merge 후 clean, 소비 호출 변경 전. 이전 화면 커밋 `12880d71`.
- 정확한 소스 prefix: `Assets/HELLSCRIPT/Runtime/Presentation/`.
- 검토 종료 시 부모의 소비 코드 변경이 3파일에 진행 중임을 확인했다. 이 문서는 위 clean 스냅샷과 공통 커밋의 연결 계획 검토이며, 진행 중 dirty diff의 최종 승인 결과가 아니다.
- 공통 handoff의 선택형 API/원화 공급 계약을 읽음. 아트 runtime Resources 경로·import receipt 미제공 상태이므로 생성하지 않음.

## 필요한 최소 소비 변경

1. `RiftRecommendationWindow.cs`: `ContentWindowView.Open(..., skin: UiSkin.Casual)`; render 내부 비교 `EquipmentComparisonView.Create(..., font:null, atlas:기존 atlas, ..., skin:v.Skin)`와 `ItemDetailView.Append(..., font:null, ..., skin:v.Skin)`에 명시 전달. `sideBySide:true`, 비교 target, source, scale, 버튼 callback과 maximumSize는 유지한다. 현재 CasualBody 명시 인자도 동작하지만 공통 기본을 소비하려면 null이 더 단순하다.
2. `RiftGearSettingsWindow.cs`: Open에 skin 전달. `Choice`와 `more`의 `SetChrome(Minimal)` 제거 또는 Casual로 변경. 이후 `UiTheme.Choice`/interactable 변경은 새 chrome을 보존한다. 일반 섹션 패널은 `SetSkin` 소비 가능. threshold 본문 Noto400, seconds Noto700과 모든 크기/배치/입력 callback은 유지한다.
3. `GameUI.GearRecommendations.cs`: 카드 `EquipmentSlotView.Create(..., skin:UiSkin.Casual)` 및 단독 장착 버튼 Casual + 라벨 적용. Item hit 역할은 유지하고 focus 색을 맞추려면 hit에도 Casual. inspect는 UiIconButton에 skin 인자가 없으므로 `tint:UiSkin.Casual.Navy`와 `SetChrome(Casual)` 명시. bare dismiss/settings는 Icon 역할을 유지한다.
4. 로컬 `Display`는 Jua를 강제하므로 `LabelFont(text.text)`/`ApplyLabel` 사용 또는 공통 생성 버튼의 wrapper 제거. 단, `ApplyLabel`은 fontStyle을 리셋하지 않으므로 필요한 `FontStyle.Normal`은 유지한다. KO `＋`→`+` 표시 보정은 Loc 키 유지 후, font 선택 **전에** 적용하거나 이후 라벨 폰트를 다시 선택한다. 폰트 cmap에 U+FF0B가 없다는 앞선 검사 결과는 그대로 유효하다.

## 우선 주의점

- **Surface 일괄 교체 금지:** `StorageSurface.SetSkin`은 top/bottom 필드를 사용하지 않고 Cream/CreamShade를 그린다. 로컬 helper 전체를 바꾸면 green score pill, 선택 gold check, disabled sky input의 의미 fill이 사라진다. 카드/섹션 등 일반 패널만 선택 적용하고 나머지는 기존 로컬 fill을 유지하거나 명시적으로 새 배경 결정을 기록한다. `Paint`도 Skin을 해제하지 않는다.
- **점수 대비:** 공통 `OnLight`는 CreamShade 위 4.5:1을 목표로 한다. 기존 green pill에 단순 적용하면 양수 3.85794, 음수 3.85751이다. 기존 `OnPaper`는 각각 5.49035/5.57193. green pill 유지 시 기존 보정 유지가 최소 변경이다. 원본 의미색에 변환을 한 번만 적용하고 이미 OnPaper한 값을 다시 OnLight하지 않는다. [계산 근거](common-api-contrast-caveat.json)는 source RGB 끝점 산출이며 화면/기기 검증이 아니다.
- **close 역할:** `UiButtonFace`는 Icon/Item을 CasualClose보다 먼저 처리한다. bare Icon에 CasualClose만 설정해도 coral 원형 face가 나오지 않는다. 현재 역할/히트 영역 보존이 우선이며 공통 창 closer는 기존 Quiet 역할이라 정상 소비 경로다.
- **재렌더:** Open의 Skin은 Repaint에 유지되고 comparison/detail의 Skin도 재생성 자식으로 전달된다. 생성 후 Text/RawImage 전체 순회 교체는 필요 없으며 재렌더 때 사라질 수 있다. 추후 provider를 연결할 때 HUD 슬롯과 상세/비교에 동일한 immutable skin을 전달한다. 새 원화는 공급자 없는 현재 Casual에서는 기존 원화다.

## 공통 상세의 geometry 차이—보존 완료로 보고하면 안 됨

소비 3파일의 기존 치수를 유지해도 새 공통 스킨은 `ItemDetailView.Style.cs`의 장식 분기를 바꾼다. 다음은 소스에서 확정되는 조건부 차이이며 런타임 실제 발생 수는 미측정이다.

| 조건 | 기존 → Casual |
|---|---|
| 비compact·비narrow·비veryTight 점수 + plaque 리소스 존재 | plaque 높이 52/64 및 폰트 13/16 → 일반 Line, 폰트 17 (`Heading+2`), 행 높이는 preferredHeight 기반 |
| major divider, 비compact·비tight + art 존재 | art 높이 기반 y 증가 `h+9` → 기본 divider y 증가 23 |
| greater affix + star art 존재 | star/본문 inset 16 → diamond/본문 inset 12 |
| socket + ring art 존재 | art 24 및 inset 28 → vector mark 16 및 inset 22, filled면 gem 10 추가 |

따라서 shell maximumSize·히트 영역·소비자 직접 fontSize 식 보존과 **상세 내부 크기/행 위치 보존**은 별개다. 공통 소유자에게 이 차이의 승인 범위를 확인하거나 후속 공통 수정으로 배치를 보존해야 한다. 소비자에서 fontSize를 줄여 맞추면 안 된다. font fallback으로 Jua→Noto400이 되면 크기 값은 같아도 실측 폭이 달라진다.

공통 detail의 바깥 rounded stencil clip은 재생성에 포함되므로 추가 전체 mask는 중복하지 않는다. 단, 내부 Relic mount는 radius8/inset1 art이며 그 mount 자체에는 둥근 mask가 없다. 새 PNG의 모서리가 불투명하면 backing 모서리를 가릴 수 있어 실제 원화 연결 후 확대 캡처로 확인해야 한다. 현재는 아트 경로 대기라 발생 여부 미확정이다.

## 호환성·남은 검증

API들은 trailing optional skin 인자로 기존 호출과 소스 호환된다. `EquipmentComparisonView.Create`의 font/atlas는 필수 인자여서 호출에서 둘 다 생략하면 안 된다. `UiFonts.CasualBody()`는 메서드, `UiSkin.Casual.BodyFont`는 속성이다. `SetSkin`/`ApplyChrome`은 입력·저장 callback을 바꾸지 않는다. pending provider는 catalog/base ID→확장자 없는 Resources 경로이며 inventory instance ID를 키로 사용하지 않는다.

최소 후속 검증은 소비자 연결 후 관련 소스 audit와 공통 테스트 소유자의 기존 receipt 확인, 부모가 배정한 단일 Unity 슬롯에서 KO/EN·비율·resize/readingScale repaint·선택/비활성·옵션 추가·숫자 입력·저장 실패·비교 target·스크롤/클리핑 확인이다. 외부 PID30116 때문에 Unity 실행은 **부모 큐 배정 대기**이며 이 검토에서 실행하거나 종료하지 않았다. 새 아트 mapping도 **공급 대기**다.

## English handoff

The common API is source-compatible and persists skin through shell/comparison/detail redraws. Pass skin explicitly to both windows and every nested equipment view; remove subsequent Minimal overrides. Prefer LabelFont instead of unconditional Jua. Keep localization keys and normalize only the rendered fullwidth plus before choosing its font.

Do not replace every local Surface helper with SetSkin: it always renders cream and would erase semantic fills. OnLight gives only ~3.858:1 against the existing green score pill; keep the current score contrast adjustment unless its background changes. CasualClose does not override Icon or Item drawing branches.

Shared detail styling changes conditional score font/row height, divider height, greater-affix inset and socket dimensions. Consumer geometry preservation does not prove preservation of these descendants. These require common-owner review and runtime captures; no local font shrinking is recommended. New art paths/receipt and the Unity slot remain pending. No game code was edited and no tests were re-run in this review.

## Latest snapshot review — 소비 연결 및 라벨 보정 후

동일 HEAD `f02d6f4c4e2fce6d923a13891888b32fc9e6eacf`의 3파일 dirty diff를 재검토했다. 공통 구현은 이 절에서 수정하거나 승인하지 않았다. 부모가 추가 공통 수정을 적용하면 그 입력은 별도 검토 대상이다.

| 소비 파일 | 검토한 SHA256 |
|---|---|
| GameUI.GearRecommendations.cs | `7b1cc5c6ffc0593313fea5281a77be1e1f1c246d761c635f5edbfce442d4d7ee` |
| RiftRecommendationWindow.cs | `fbffbca2c319591f46ce6bbdfff66e83e67440858f20188c5ec35894ccb15214` |
| RiftGearSettingsWindow.cs | `fb7a8979a36afedc38ed16a7b2226e392ad152db3e3ae5662e516a3be288e072` |

**최종 소비 소스에서 새로 도입된 기능·직접 치수 회귀는 발견하지 않았다.** 관련 [소스 audit](final-consumer-source-audit.json)는 원래 기준 `2e193c33`과 직전 HEAD 모두에 대해 Place 11개, Text geometry/fontSize 8개, Button geometry/callback 8개, Tab 1개, Icon geometry/callback 3개, Slot geometry 1개, 직접 geometry/fontSize 대입 15개가 일치함을 기록한다. 숫자 입력·slider callback도 동일하며 GameUI timer/equip/open/dismiss/clear 구간은 byte 동일하다. 검사 도중 3파일이 바뀌지 않았음을 확인했다. 본 스크립트는 관련 소스 비교이며 Unity 테스트/컴파일을 대체하지 않는다.

소비 연결은 두 Open, nested comparison/Append, HUD slot 모두 명시 skin을 사용한다. Minimal override와 무조건 Jua 지정이 제거되었고 폰트 fallback을 공통 소유자가 유지한다. KO Loc key와 `＋`→`+` 표시 보정은 남아 있으며 보정 뒤 ApplyLabel이 실행된다. 일반 panel은 SetSkin, 체크/숫자 입력의 의미 fill은 로컬 Surface여서 새 common setter에 지워지지 않는다. 점수 pill은 **green→cream 배경·green border**로 의도적으로 바뀌었다. 이 변화는 승인된 스킨 차이로 보고해야 하며 원래 green 배경을 보존했다고 하면 안 된다.

처음 검토에서 발견한 비교 탭 라벨 문제는 해결됐다. comparing=false에서 열어 선택으로 바꾸면 Muted가 gold 배경 위에 남던 상태를, 생성 직후 `v.Skin.ApplyLabel(...)`로 모든 상태 Navy를 유지하도록 고쳤다. 기존 toggle callback은 그대로다. 선택 Gold/GoldShade 끝점 대비는 **8.794/7.852**(수정 전 **3.233/2.886**). part/Note/Unit의 활성 보조 텍스트도 OnLight(Muted)로 바뀌었다.

[최신 대비 산출](final-consumer-contrast-audit.json): 크림 점수 positive/negative·제목·활성 보조 텍스트·5개 등급 이름은 CreamShade 끝점 **4.5000–4.5017**, Cream 끝점은 더 높다. 단독 primary 장착 버튼은 Navy 대 GreenShade **5.377**, pressed endpoint **4.509**다. 이는 공통 소스 RGB 식을 재현한 산술이며 anti-alias·투명도·실제 표시 배율·기기 접근성 인증은 아니다. 비활성 seconds/input의 Muted는 의도적으로 남아 있으므로 모든 텍스트가 4.5 이상이라는 주장도 하지 않는다.

위 공통 상세의 geometry/폰트 분기 차이, 새 PNG 내부 mount 모서리, 원화 provider 경로, Unity 런타임 시각 검증은 여전히 **공통/통합 대기**다. 소비자 직접 치수 검사 통과와 구분한다. 기존 preliminary JSON은 `final-consumer-source-audit.pre-label-fix.json`으로 보존했다. 그 첫 검사에서 Slot의 여섯 번째 font 인자를 geometry로 잘못 포함한 판정은 최종 parser에서 parent/item/x/y/size 다섯 인자만 비교하도록 바로잡았다. 제품 코드 실패가 아니며 그 오류를 숨긴 pass 집계가 아니다.

English: The final consumer snapshot preserves domain callbacks, timer/transaction code, direct layout and font-size expressions against both the original base and pre-integration HEAD. The comparison-tab contrast issue is fixed by keeping its label Navy; active muted labels use OnLight. The score pill intentionally changes to cream, making the shared contrast mapping applicable. No new consumer-source defect remains identified. Shared detail geometry changes, approved-art provider mapping and runtime visual acceptance are still pending and are not covered by this static result. No game code, Unity process or browser was changed by the reviewer.
