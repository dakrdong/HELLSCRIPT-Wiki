# 사냥 칙령 UI 개편 설계 — 튜토리얼 호환형

갱신일: 2026-10-05 · [English](Hunt_Edict_UI_Overhaul.en.md) · [시안 열기](../../Prototypes/HuntEdict/HELLSCRIPT-HuntEdict.html) · [시안 README](../../Prototypes/HuntEdict/README.md) · [이식 프롬프트](../../Prototypes/HuntEdict/NATIVE_PORT_PROMPT.md)

상태: 설계 확정(HTML 시안 v2). **Unity 구현은 아직 없다.** 이 문서는 사냥 칙령 창을 새로 짜는 개발자와 튜토리얼 작업자가 같은 계약을 보게 하는 인계 문서다. 입력은 튜토리얼 브랜치(`claude/tutorial-chronicle`, 병합 전)의 `Docs/Design/Tutorial_Hunt_Edict_UI_Brief.md`와 코드 조사 6건(튜토리얼 계약, 현재 창 구조, 스모크·공개표 영향, 이식 지도 2건, 테스트 영향)이다. 아래 내용은 코드를 읽어 정리한 것이며 게임을 실행해 확인한 것이 아니다.

배경 문서: [사냥 칙령 UX 재설계(요약 화면)](Hunt_Edict_UX_Redesign.md), [사냥 칙령 단계 공개](../Implementation/Hunt_Edict_Progression.md), [공통 UI 계약](../Implementation/Shared_UI_Contract.md).

## 1. 요청과 결정

사용자는 사냥 칙령 화면이 시안 없이 기능만 있어 미감이 떨어진다고 보고, 지금의 기능을 모두 담으면서도 이해하기 쉽고 깔끔하며 **튜토리얼이 최소 기능부터 순차 공개하는 흐름에 대응**하는 디자인을 요청했다. 튜토리얼은 컨트롤 이름과 창 상태를 읽어 지정 버튼을 가리키므로, 화면을 예쁘게만 고치면 안내가 멈출 수 있다. 그래서 구조와 이름을 계약으로 고정하고 표면과 정보 설계만 바꾼다.

| 결정 | 내용 | 근거 |
| --- | --- | --- |
| D1 | 탭 10개의 id와 `edict-tab-<id>` 이름을 유지한다. 1차 시안의 5탭 통합은 철회했다 | 스모크 9곳이 탭 10개 동시 존재를 가정하고, 튜토리얼 코드가 탭 이름을 직접 가리킨다 |
| D2 | 개요는 2026-10-01 결정(전투 방식 카드 3개 + 기본 세팅 스위치)을 유지하고 **다음 문장 티저**만 더한다 | 같은 날 의도적으로 단순화했고 `CheckSimpleOverview`가 구조를 고정한다 |
| D3 | 간편 프리셋은 현재 다이얼로그 흐름(고르면 바로 적용)을 유지한다. 인라인 펼침은 튜토리얼이 안정된 뒤의 실험으로 보류한다 | 이어하기 v2 호환과 스모크 24곳 변경을 피한다 |
| D4 | 안내 문구(N5)는 떠 있는 말풍선이 아니라 `Option area` 맨 위의 **고정 캡션 줄**에 둔다 | 현재 A/B 비교 화면 캡처에서 말풍선이 프리셋 탭을 덮는다 |
| D5 | 3박자 띠(N4)는 기본 **한 줄**, 눌러서 펼친다 | 가로 영역 364 중 51을 늘 차지하면 옵션 행이 4줄 남짓만 보인다 |
| D6 | 기준 공개표는 튜토리얼 브랜치의 `HuntEdictProgression.json`이다. **Unity 이식 전에 그 브랜치를 `main`에 병합한다** | 낮은 HP 균열 3, 위치·거리 균열 5, E18 20/13/7 분리가 main과 다르다 |
| D7 | "새 문장" 표시(N1)는 저장 필드를 새로 만들지 않고 계산한다 | 저장 스키마 변경과 마이그레이션을 피한다 |

## 2. 한눈에

| 바뀌는 것 | 바뀌지 않는 것 |
| --- | --- |
| 탭: 아이콘 + 이름 + 부제 + 새 문장 점. 프롤로그는 아이콘 하나짜리 좁은 줄 | 탭 id 10개, `edict-tab-<id>` 이름, `Main tabs` 안에 탭 버튼 전부 |
| 그룹 칩: 아이콘 + 제목 + **현재 답**(간편 프리셋 이름) + 변경 점 + 새 문장 점 | `edict-group-<ids[0]>` 이름, 칩이 스크롤 밖에 있는 구조, `HuntEdictUi.json` 그룹·id 순서 |
| 옵션 행: 한 줄(라벨 + ? + 값 알약) | `edict-option-<id>`, `edict-help-<id>`, 편집 다이얼로그 종류 |
| 다이얼로그: 라디오 카드, 숫자 범위·빠른 값 칩 | `edict-quick-picker-*`/`edict-quick-choice-*`, `edict-number-input`/`edict-number-apply`, `HasDialog` |
| 개요: 전투 방식 카드에 실제 수치 칩 + 다음 문장 티저 | 요약 탭 구조, `edict-style-*` 정확히 3개, `edict-default-settings` |
| 정책 화면: 세로에서 진행 버튼을 설명 칸 위로, 관찰 진행 막대 | `Preset explanation` 영역, `edict-starter-next`가 그 안에 있음 |
| 새 슬롯 N1~N6 | 공개 판정, 권한, 저장 경로(`CommitHuntEdict`), 저장 후 갱신 규칙 |
| 안내 줄: 고정 캡션 | 레거시 계정의 트리 화면, 구 편집기 `ShowLegacyEdictEditor` |

## 3. 설계 원칙

- **P1 이름·ID 보존.** `edict-*` 이름, 탭 id 10개, 영역 이름, `ids[0]` 규칙을 바꾸지 않는다. 바뀌는 순간 6장 표에 유지/개명/이동/삭제/대체를 적는다.
- **P2 지정 컨트롤은 그 순간 화면 안에 있다.** 튜토리얼 게이트는 지정 컨트롤의 화면 사각형을 뷰포트나 스크롤 마스크로 자르지 않고 그대로 뚫는다. 스크롤 밖에 있는 컨트롤은 "있음"으로 판정되어 가림막이 풀리지 않는다. 프롤로그는 스크롤 없이 보이게 배치하고 그 밖은 `EnsureVisible`로 보장한다.
- **P3 미공개는 그리지 않는다.** 잠금 자리·빈칸 없이, 다음 한 가지만 개요 티저로 예고한다.
- **P4 새 표시는 `DisclosureDisplayKey` 안에서만.** 별도 Repaint, 타이머, 프레임 갱신으로 마커를 바꾸지 않는다(버튼 깜빡임 규칙).
- **P5 저장 경로는 하나.** 모든 변경은 편집본(`Session.Draft`)을 거쳐 `CommitHuntEdict`로 저장된다. 튜토리얼 완료는 저장된 값의 변화로 판정한다.
- **P6 `HasDialog`의 뜻 유지.** 모달이면 참이다. 스킬 행동 말풍선도 포함한다.
- **P7 N1~N5는 저장·탭·옵션을 가리거나 밀어내지 않는다.** 높이 예산을 정하고 사각형 비교로 검사한다.
- **P8 크기와 언어.** 세로 440×956, 가로 956×440, PC 16:9·16:10·21:9, 한국어·영어, 기본 글자 크기만 검증한다.
- **P9 신규 문구는 한국어·영어를 함께.** 한국어 원문이 `en.txt`의 키다.

## 4. 화면 밀도와 단계별 공개

화면 밀도는 4단계이며 탭 수와 편집 범위가 다르다. 탭 표시는 `HuntEdictProgression.Tab`이 결정하고 이번 개편은 이를 바꾸지 않는다.

| 단계 | 시점 | 보이는 탭 | 편집 범위 | 시안 근거 |
| --- | --- | --- | --- | --- |
| L0 프롤로그 | 필수 맵 | 스킬 → 생존(한 번에 하나) | 첫 스킬 노드 1개, 슬롯 1칸, 물약 HP 기준 1개 | `evidence/lesson-*`, `survival-*` |
| L1 초반 | 마을 도착 ~ 균열 5 | 요약·스킬·생존, 균열 4부터 전투 | 간편 프리셋만. 직접 편집은 물약 HP·자동 물약·방어·회피 지정 스킬 | `evidence/stage-t0-*`, `stage-t1-*`, `stage-r3-*` |
| L2 중반 | 균열 6 ~ 15 | 프리셋(6)·전리품(7)·탐색(8)·가방(9)·자동 착용(12)·반복(15 + 룬 안내) | 공개된 그룹의 직접 설정, 스킬 상세, 공격 순서, 로컬 프리셋, 훈련 비교 | `evidence/stage-r6-*`, `stage-r7-*`, `stage-r9-*`, `stage-r12-*` |
| L3 후반 | 균열 18~ | 모든 탭 | 세밀한 조건 3그룹, 행동 순서·설정 공유(20) | `evidence/stage-r18-*`, `stage-r20-*` |

문장이 열리는 시점과 위치는 튜토리얼 브랜치의 공개표를 따른다. 규칙 하나가 여러 그룹·탭에 걸칠 수 있다(E18B는 전투+전리품, E20은 전투+탐색+반복).

| 시점 | 안내 | 제목 | 탭 · 그룹 | 옵션 | 초점 옵션 |
| --- | --- | --- | --- | --- | --- |
| 필수 맵 | P00 | 물약 HP 기준 | 생존 · 자동 물약 | 1 | `survival.potionHpPercent` |
| 첫 일반 균열 종료 | F05 | 자동 물약·기본 회피 | 생존 · 자동 물약, 피해 유형별 회피(권한) | 1 + 권한 | `survival.potionHpPercent` |
| 균열 2 | E02 | 전투 성향 | 요약 · 전투 방식 | 권한 | `edict-style-balanced` |
| 균열 3 | E05 | 낮은 HP 대응 | 생존 · 긴급 대응·전투 복귀, 생존 수단·보존 | 5 | `survival.lowHp` |
| 균열 4 | E04 | 공격 대상과 전환 | 전투 · 공격 대상 선택, 대상 유지·추적 | 6 | `target.default` |
| 균열 5 | E03 | 전투 거리와 포위 대응 | 전투 · 전투 위치·거리, 포위 대응·몰이 | 11 | `position.engage` |
| 균열 6 | F07·H09·H10·H17 | 직접 설정·프리셋·훈련 비교 | 프리셋·공유, 공개된 모든 그룹의 직접 설정 | 권한 | — |
| 균열 7 | E07 | 전리품 회수 | 전리품 · 등급별 장비 회수, 금화·재료 회수, 회수 이동·종료 | 12 | `loot.NORMAL` |
| 균열 8 | E08 | 탐색과 특수 조우 | 탐색 · 탐색·균열 진행, 일반·봉인 상자, 성소(저주 상자는 실제 발견 뒤) | 9 | `explore.mode` |
| 균열 9 | E09 | 가방 공간과 보호 | 가방·정리 · 네 그룹 | 12 | `bag.trigger` |
| 균열 12 | E12 | 자동 장착 | 장비 추천 착용 · 공통 설정, 무기 | 3 | `autoEquip.enabled` |
| 균열 12 + 전설·세트 획득 | E12S | 전설과 세트 비교 | 장비 추천 착용 · 부위별 | 17 | `autoEquip.1.mode` |
| 균열 14 | E14 | 물약 보급 | 생존 · 자동 물약 | 11 | `potion.autoBuy` |
| 균열 15 + 룬 안내 | H11 | 반복 사냥 | 반복 설정 · 세 그룹 | 8 | `repeat.enabled` |
| 균열 18 | E18 / E18B / E18C | 세밀한 조건 3종 | 생존·전투·전리품·가방 | 20 / 13 / 7 | `dodge.interrupt` / `target.chaseDistance` / `bag.replacementRank` |
| 균열 20 | E20 | 행동 순서·설정 공유 | 전투·탐색·반복 + 공유 | 15 | `common.order` |

## 5. 구조 변경

**탭.** id 10개와 순서(`overview, skills, combat, survival, loot, bag, explore, repeat, autoEquip, presets`), 표시 조건, 제목 배열 위치(제목은 `HuntEdictUiCatalog.Tabs` 순서로 참조) 모두 그대로다. 달라지는 것은 표면이다.

| 항목 | 현재 | v2 |
| --- | --- | --- |
| 탭 버튼 | 글자(+가로 18px 아이콘) | 아이콘 + 이름 + 부제(탭이 6개 이하인 가로 화면) + 새 문장 점 |
| 프롤로그(L0) | 172폭 열에 탭 1개 | 가로 약 48폭 아이콘 열, 세로 얇은 한 줄. 버튼 오브젝트(`edict-tab-skills`/`edict-tab-survival`)는 유지 |
| 세로 | `min(5,n)`열 | 그대로 |
| 계열 구분선 | 없음 | 쓰지 않는다. 10개가 한 열에 들어가고 구분 없이도 읽힌다 |

부제는 한국어·영어 모두 짧은 구절이다: 요약 "지금의 싸움 / How you fight", 스킬 "배우고 장착 / Learn and equip", 전투 "위치·대상 / Position, target", 생존 "물약·후퇴·회피 / Potion, retreat", 전리품 "무엇을 주울까 / What to pick up", 가방 "가득 차면 / When full", 탐색 "탐색·상자 / Explore, chests", 반복 "자동 반복 / Auto repeat", 자동 착용 "자동 착용 / Auto equip", 프리셋 "저장·비교·공유 / Save, compare, share".

**그룹.** `HuntEdictUi.json`은 바꾸지 않는다(35개 그룹, id 순서, 탭, 아이콘). `ids[0]`이 `edict-group-*` 이름, 퀵프리셋 범위(`global/<ids[0]>`), 스크롤 키를 정하므로 순서를 바꾸면 셋이 조용히 깨진다. 칩이 늘어나는 것은 표시뿐이다.

| 항목 | 현재 | v2 |
| --- | --- | --- |
| 칩 | 아이콘 + 제목 + 변경 점 | + 현재 답(퀵프리셋 이름 또는 "직접 설정") + 새 문장 점. 높이 42 → 44 |
| 열 수 | `clamp(floor(w/(116·확대)), 1, 그룹 수)` | 같은 공식 |
| 본문 | 설명 → 선택기 → 옵션 행 | 같은 순서. 옵션 행만 한 줄 |
| 기본 선택 | 이전 선택이 없으면 첫 그룹 | N2: 새 그룹 > 기억된 그룹 > 첫 그룹 |

**옵션 행.** 지금은 라벨 줄 아래에 값 우물이 있는 두 줄(최소 62)이다. v2는 라벨(+?)과 값 알약이 한 줄(최소 36)이며 값 알약이 `edict-option-<id>`다. 비활성 행은 이유 한 줄("세부 설정은 균열 6단계부터 공개됩니다." 또는 "{상위 옵션}: {값} 선택 시 적용")을 아래에 붙인다. 바꾼 행은 왼쪽에 금색 막대를 둔다.

**다이얼로그.** 틀(제목·닫기 `edict-dialog-close`·뒤 배경 `edict-dialog-dismiss`)은 그대로다.

| 다이얼로그 | v2 |
| --- | --- |
| 간편 프리셋 | 라디오 카드(이름 + 설명). 고르면 편집본에 적용하고 닫는다. 마지막 "직접 설정"은 균열 6부터 |
| 숫자 | −/＋, 범위·단위 줄, 퍼센트 옵션은 빠른 값 칩(입력창만 채우고 적용은 따로), 오류 줄 |
| 선택·복수·순서 | 선택은 고르면 적용, 복수·순서는 `edict-choice-apply`. **순서는 기존 드래그 카드를 유지**한다(시안의 ↑↓ 버튼은 단순화) |
| 도움말·확인 | 기존과 같다 |

**검색 줄.** 시안은 공개된 옵션이 8개 미만이면 `edict-search`와 `edict-changed-only`를 그리지 않았다. 스모크가 이 두 컨트롤을 낮은 공개 단계에서 쓸 수 있으므로 **이식의 기본은 프롤로그(`ProgressivePrologue`)에서만 숨기는 것**이다. 공개 단계가 낮은 계정에서도 숨길지는 열린 결정(15장)이다.

**개요.** 2026-10-01 구성(전투 방식 카드 3개 + 균열 20의 기본 세팅 스위치)에 다음을 더한다.

- 전투 방식 카드마다 실제 수치 칩 2개: "후퇴 HP {n}%"(해당 성향의 `survival.lowHp` 프리셋 값)와 회피 방식 이름. 지어낸 눈금은 쓰지 않는다.
- 다음 문장 티저 카드(N3). 버튼이 아니며 아이콘 대신 문장 번호 배지를 쓰므로 `CheckSimpleOverview`의 "스타일 버튼 정확히 3개, ScrollRect 1개, SkillIconView 없음"을 지킨다.

**기본 세팅 적용(잠금).** 균열 20부터 개요 상단의 스위치. 켜면 요약·스킬(관리)·반복만 열려 있고 나머지 탭과 푸터는 흐려진다(`DisableForDefaults`). 이때 `FocusGlobalOption`은 조용히 아무것도 하지 않으므로 안내 캡션이 `edict-default-settings`를 가리키는 문장으로 바뀐다(10장).

**스킬 탭(신규·비레거시 계정).** 트리가 아니라 "배울 수 있는 스킬 카드 목록"이다. 레벨 2 전에는 첫 스킬 1장만 보인다. 구성은 남은 포인트, 카드 목록, 인스펙터(−·＋·장착), 장착 스킬 줄이다. 카드를 누르면 행동 말풍선(모달)이 열려 `edict-slot-equip`/`edict-slot-remove`와 `edict-slot-policy`를 보인다. 장착 슬롯은 프롤로그에서 1칸, 이후 "장착 수 + 1"(최대 4), 레벨 40에서 궁극기 칸이 더해진다.

**정책 화면.** `edict-policy-back`, 프리셋 탭(탭마다 라디오), `Preset explanation`, 세로 레이아웃에서 장착 줄이 아래, 가로에서 오른쪽 세로 줄이다. 활성화는 즉시 저장되어 푸터가 없다(`edict-save` 없음).

**시안과 다르게 이식하는 것.** 시안은 HTML이라 단순화한 곳이 있다. 네이티브에서는 아래를 따른다.

- 변경 개수 칩("저장하지 않은 변경 n")은 옮기지 않는다. `Session.Dirty`는 불리언만 준다.
- 순서 다이얼로그는 기존 드래그 카드를 유지한다(시안의 ↑↓ 버튼은 쓰지 않는다).
- 닫기 확인은 기존 `RequestLeave`(되돌리거나 저장하고 나가기)를 유지한다. 시안의 "취소/확인"은 쓰지 않는다.
- 바꾼 옵션 행의 왼쪽 금색 막대는 네이티브에 대응물이 없다. 새 3폭 Panel이나 그룹 칩의 "•"와 같은 점으로 구현한다.
- `bag.warehouseFull` 그룹과 자동 착용 탭은 전용 네이티브 화면(`DrawWarehouseHandling`, `DrawRecommendedEquipment`)을 유지한다. 시안의 일반 옵션 행은 단순화다.
- 시안의 px를 상수로 옮기지 않는다. 시안은 게임과 같은 `UiTheme.Scale` 공식(가로 높이 450, 세로 폭 405 기준)으로 논리 크기를 잡지만, 기존 높이 계산이 곱하는 `lastScale`/`Grow`는 시안에 없다.

## 6. 이름 매핑표

상태 열: **유지** = 이름·의미·위치 개념이 그대로, **이동** = 같은 이름이 다른 위치에 있다, **신규** = 이번에 추가한다. "시안 확인"은 `Prototypes/HuntEdict/evidence/ui-names.json`(브라우저 검사가 방문한 이름)에 있는지를 뜻한다. 시안은 이름을 `data-ctl` 속성에 달았다.

### 6.1 창 틀·탭·푸터

| 이름 | 의미 | 상태 | 시안 확인 | 비고 |
| --- | --- | --- | --- | --- |
| `menu-hunt-edict` | 도크의 사냥 칙령 버튼 | 유지 | ✓ | 게임에서는 오브젝트 이름이 아니라 `contentDockEdict` 필드를 가리킨다(오브젝트 이름은 "사냥 칙령") |
| `Hunt Edict window` | 창 루트 | 유지 | ✓ | 스모크가 경로로 사용 |
| `Main tabs` | 탭 패널 | 유지 | ✓ | 10개 탭이 모두 안에 있어야 한다(스모크 9곳) |
| `edict-tab-<id>` | 탭 버튼 | 유지 | ✓ | id는 `overview skills combat survival loot bag explore repeat autoEquip presets` |
| `Option area` | 창 본문 루트(RectMask2D) | 유지 | ✓ | 위에 캡션 줄이 예약될 수 있다(8장) |
| `Fixed save controls` | 푸터 | 유지 | ✓ | 정책 화면에서는 높이 0 |
| `edict-save` / `edict-revert` | 저장 / 되돌리기 | 유지 | ✓ | 변경이 있을 때만 활성 |
| `edict-close` | 닫기 | 유지 | ✓ | |
| `edict-dialog-dismiss` / `edict-dialog-close` | 다이얼로그 뒤 배경 / 닫기 | 유지 | ✓ | |
| `Dialog` | 다이얼로그 패널 | 유지 | ✓ | |

### 6.2 요약(개요)

| 이름 | 의미 | 상태 | 시안 확인 | 비고 |
| --- | --- | --- | --- | --- |
| `Overview custom settings` | 개요 본문 루트 | 유지 | ✓ | `CheckSimpleOverview`가 경로로 사용 |
| `Overview combat styles` | 개요 스크롤 | 유지 | ✓ | |
| `edict-style-aggressive` / `balanced` / `careful` | 전투 방식 카드 | 유지 | ✓ | 버튼은 정확히 3개 |
| `edict-default-settings` / `edict-default-status` | 기본 세팅 적용 스위치 / 상태 문구 | 유지 | ✓ | 균열 20부터 |
| `edict-teaser` | 다음 문장 티저(N3) | **신규** | ✓ | 버튼이 아니다 |

### 6.3 그룹 화면

| 이름 | 의미 | 상태 | 시안 확인 | 비고 |
| --- | --- | --- | --- | --- |
| `edict-search` / `edict-changed-only` | 검색 입력 / 변경된 항목 | 유지 | ✓ | 표시 조건은 5장 |
| `edict-group-<ids[0]>` | 그룹 칩 | 유지 | ✓ | 스크롤 밖, 그룹 아이콘 필수 |
| `edict-search-result-<key>` | 검색 결과 카드 | 유지 | ✓ | |
| `Selected edict group` | 선택 그룹 스크롤 | 유지 | ✓ | |
| `edict-quick-picker-<scope>` | 간편 프리셋 선택기 | 유지 | ✓ | scope 예: `global/position.engage` |
| `edict-quick-choice-<preset id>` | 다이얼로그의 선택지(직접 설정은 `custom`) | 유지 | ✓ | |
| `edict-option-<id>` | 옵션 값 버튼 | 유지 | ✓ | 값 알약이 이 이름이다 |
| `edict-help-<id>` | 도움말 `?` | 유지 | ✓ | |
| `edict-choice-apply` | 복수 선택·순서 적용 | 유지 | ✓ | |
| `edict-number-input` / `edict-number-apply` | 숫자 입력 / 적용 | 유지 | ✓ | 입력창은 `InputField`(이름으로 찾는다) |
| `edict-beat-strip` | 3박자 띠(N4) | **신규** | ✓ | 칩과 스크롤 사이, 스크롤 밖 |

### 6.4 스킬·정책 화면

| 이름 | 의미 | 상태 | 시안 확인 | 비고 |
| --- | --- | --- | --- | --- |
| `Available skill tree` | 스킬 카드 목록 스크롤 | 유지 | ✓ | |
| `edict-skill-<skillId>` | 스킬 카드(레거시는 트리 노드) | 유지 | ✓ | `edict-skill-` 접두어를 `rank-up/rank-down/equip/reset`과 공유한다 |
| `edict-skill-rank-up` / `rank-down` / `equip` / `reset` | 인스펙터 버튼 | 유지 | ✓ (`reset`은 시안 범위 밖) | |
| `Skill action bubble` / `edict-skill-menu-dismiss` | 행동 말풍선(모달) / 바깥 클릭 소비 | 유지 | ✓ | `HasDialog`가 참 |
| `edict-slot-equip` / `edict-slot-remove` | 말풍선의 장착 / 해제 | 유지 | ✓ | |
| `edict-slot-policy` | 말풍선의 사냥 칙령 편집 | 유지 | ✓ | |
| `Equipped active skills` | 장착 스킬 줄 | 유지 | ✓ | |
| `edict-active-slot-<0..3>` / `edict-ultimate-slot` | 장착 슬롯 | 유지 | ✓ | |
| `edict-common-policy` | 공통 공격 설정 | 유지 | ✓ | 균열 6부터 |
| `edict-policy-back` | 스킬 트리로 | 유지 | ✓ | |
| `Skill preset panel` | 정책 화면 패널 | 유지 | ✓ | |
| `edict-preset-tab-<id>` | 사용 방식 탭 | 유지 | ✓ | 프롤로그는 두 방식뿐 |
| `edict-preset-activate` | 지금 보이는 탭의 라디오 | 유지 | ✓ | 활성화는 즉시 저장 |
| `edict-preset-radio-<id>` | 보이지 않는 탭의 라디오 | 유지 | ✓ | |
| `Preset explanation` | 설명·미리보기·진행 버튼 행 | 유지 | ✓ | 8장 |
| `edict-starter-next` | 비교 진행 버튼 | **이동** | ✓ | 같은 영역 안. 세로는 설명 칸 위쪽, 가로는 아래쪽 |
| `edict-preview-restart` / `pause` / `speed-<배속>` | 전투 예시 조작 | 유지 | ✓ | |
| `edict-policy-basic` / `edict-policy-order` / `edict-policy-choice-<id>` | 기본 공격·공격 순서·사용 방식 직접 선택 | 유지 | — | 시안 범위 밖 |

### 6.5 프리셋·공유

| 이름 | 의미 | 상태 | 시안 확인 |
| --- | --- | --- | --- |
| `edict-preset-name-<slot>` / `store-<slot>` / `share-<slot>` / `import` | 슬롯 이름 / 저장 / 공유 / 가져오기 | 유지 | ✓ |
| `edict-preset-name-input` | 이름 바꾸기 입력 | 유지 | — |

### 6.6 창 밖·새 슬롯·시안 범위 밖

| 이름 | 의미 | 상태 | 시안 확인 |
| --- | --- | --- | --- |
| `edict-notice-configure` / `edict-notice-later` | 마을 안내 카드 버튼(창 밖) | 유지 | — |
| `Guide caption` (시안 이름 `edict-caption`) | 안내 캡션 줄(N5). `Option area`의 **형제** RectTransform | **신규** | ✓ |
| `edict-save-next` | 저장 직후 행동 버튼(N6) | **신규** | ✓ |
| `edict-auto-*`, `edict-storage-*` | 장비 추천 착용·입고 화면 | 유지(표면 개편은 후속) | — |

시안에만 있는 이름: `Skills page`(시안 래퍼). 게임 코드에는 없다.

## 7. 창 상태와 동작

튜토리얼이 창 밖에서 읽는 공개 멤버는 모두 그대로 둔다.

| 멤버 | 상태 |
| --- | --- |
| `SelectedTab`, `ManagingSkills`, `SelectedSkill`, `PolicySkill`, `SelectedGroup`, `VisibleSkillPreset`, `ActiveSkillPreset`, `QuickPresetSelection(scope)`, `ProgressivePrologue`, `PreviewCombat`, `Session.Draft`, `Session.Dirty` | 유지(의미 불변) |
| `HasDialog` | 유지. 모달이면 참(간편 프리셋·숫자·선택·복수·순서·도움말·확인·스킬 행동 말풍선) |
| `Open(…, tutorialLesson)`, `SelectTab(tabId)`, `SelectSkillPage`, `OpenSkillPolicy`, `HandleBack()` | 유지. 뒤로 가기 순서: 다이얼로그 → 정책 화면에서 스킬 목록 → 창 닫기. 프롤로그는 `ContentWindowHost.BackLocked` |
| `FocusGlobalOption(optionId)` | 유지하되 **옵션이 보이는 위치로 스크롤**한다(`EnsureVisible`). 기본 세팅 적용 중이면 호출자에게 거절을 알린다 |

추가한다(이름과 시그니처는 제안이다).

| 멤버 | 용도 |
| --- | --- |
| `EnsureVisible(string controlName)` | 컨트롤이 든 스크롤 영역을 움직여 사각형이 뷰포트 안에 들어오게 한다. `FocusGlobalOption`, 기본 그룹 선택, 튜토리얼 `LessonTarget`이 부른다 |
| `SetGuideCaption(EdictCaption)` / `ClearGuideCaption()` | 캡션 줄(N5) |
| `SetBeats(EdictBeats)` / `ClearBeats()` | 3박자 띠(N4) |
| `SetAfterSave(EdictAfterSave)` | 저장 직후 행동(N6) |
| `MarkGroupOpened(string groupKey)` | 이번 세션에 연 그룹을 기록(N1). 저장하지 않는다 |

## 8. 지정 영역

- **`Option area`** 는 유지한다. 창 본문 루트이며 RectMask2D를 가진다. 안내가 진행 중이면 높이 44(가로)/58(세로)를 예약한다. **구현은 `Option area` 자체를 그만큼 줄이고 그 위에 형제 `Guide caption`을 둔다.** 자식 래퍼를 넣으면 `Option area/Overview custom settings` 경로(스모크)와 이름이 유일해야 한다는 가정이 깨진다. 이렇게 하면 게이트의 `Option area` 구멍에 캡션 줄이 들어가지 않는다. 예약은 안내가 시작·종료될 때만 바뀌고 다이얼로그가 열려도 유지한다.
- **`Preset explanation`** 도 유지한다. 정책 화면의 한 행으로, 설명·전투 예시·진행 버튼을 담는다. 커스텀 탭에서는 그리지 않는다.
- **`edict-starter-next`** 는 `Preset explanation` **안**에 있어야 한다. 게이트는 지정 영역 안에서만 입력을 통과시키기 때문이다. 시안은 가로에서 설명 칸 맨 아래, 세로에서 설명 칸 맨 위(전투 예시 위)에 둔다. 두 크기 모두 스크롤 없이 보인다.
- **스크롤 없이 보이는 배치(P2).** 프롤로그 전 단계의 지정 컨트롤은 자동 스크롤을 끈 상태에서도 5개 크기·2개 언어 모두 화면 안에 있다(시안 검증).
- 튜토리얼의 `starter-selection-area`는 `Option area`, `starter-observation-area`는 `Preset explanation`에 대응하며 이 대응은 튜토리얼 쪽(`LessonTarget`)이 소유한다.

## 9. 새 슬롯 N1~N6

높이는 논리 단위이며 시안에서 균열 5 안내 상태로 측정했다. 시안은 게임의 `UiTheme.Scale`과 같은 공식으로 배율을 정하므로(가로 956×440 → 978×450, 세로 440×956 → 405×880) 가로 영역은 364, 세로 영역은 약 746이다.

| 슬롯 | 위치 | 높이(가로 / 세로) | 데이터 | 갱신 |
| --- | --- | --- | --- | --- |
| **N1** 새 문장 표시 | 탭 버튼·그룹 칩 모서리의 점 | 0(겹침) | 계산: `규칙 공개됨 ∧ 안내 미수행 ∧ 이번 세션에 그 그룹을 열지 않음` | `DisclosureDisplayKey`에 표시 목록을 넣는다 |
| **N2** 기본 펼침 | 탭 진입 시 선택 그룹 | 0 | 새 그룹 > 기억된 그룹 > 첫 그룹 | 기존 `SelectGroup` |
| **N3** 다음 문장 티저 | 개요, 전투 방식 카드 아래 | 76 / 76 | `EdictTeaser` | 개요는 이미 다시 그린다 |
| **N4** 3박자 띠 | 그룹 칩과 스크롤 사이(스크롤 밖) | 한 줄 28 · 펼침 51 / 한 줄 28 · 펼침 117 | `EdictBeats` | 튜토리얼이 설정·해제할 때만 |
| **N5** 안내 캡션 | `Option area` 맨 위 예약 줄 | 44 / 58 | `EdictCaption` | 튜토리얼이 설정·해제할 때만 |
| **N6** 저장 직후 행동 | 푸터 상태 줄 | 푸터 안 | `EdictAfterSave` | 저장 성공 알림 1회, 12초 또는 다음 변경까지 |

**N1 계산.** 규칙은 `HuntEdictProgression.json`의 항목이며 안내는 규칙의 `guide`(없으면 첫 일반 균열 규칙은 F05, 반복 규칙은 H11)다. 표시 대상은 공개된(`Disclosed`) 옵션이 하나라도 있는 규칙으로 한정한다. 간편 프리셋은 아직 공개되지 않은 규칙의 값까지 쓰므로(예: 낮은 HP 프리셋이 E18의 치명적 피해·부착 피해 값을 바꾼다), 공개 여부를 확인하지 않으면 안내가 일찍 "수행"으로 기록된다. 게임의 `TutorialProgress.EdictSaved`는 이미 `GuideVisible`을 확인한다. 마커는 `TutorialRecord.practiced`로 지워지고, 열어 본 그룹은 세션 동안 기록해 지운다. 새 저장 필드는 없다.

**데이터 모양**(제안, `Core`에 두고 `UnityEngine` 의존을 피한다).

```csharp
public struct EdictTeaser   // HuntEdictProgression.Next(...)의 구조화 결과 + 챕터 엔진
{ public int Sentence, Total, Stage, Now, PagesReceived, PagesTotal; public string Key, Title, Condition; }
public sealed class EdictBeats      // 튜토리얼이 안내별로 채운다
{ public string Guide, Focus, Problem, Fix, ResultBefore, ResultAfter; public bool Done; }
public struct EdictCaption          // 문구는 이미 번역된 상태
{ public string Text, Target; public bool Hard; }
public struct EdictAfterSave        // 라벨 + 실행
{ public string Label; public System.Action Run; }
```

- `EdictTeaser.Condition`은 "첫 일반 균열 종료 후", "균열 N단계 클리어 시", "룬 필수 안내 완료 후" 중 하나이고, `Now/Stage`로 진행 막대를 그린다. 쪽수는 챕터 엔진이 연결되기 전에는 숨긴다.
- `EdictBeats.Problem`은 첫 균열 종료 안내(F05)의 마지막 5초 HP·최대 피해원 문구를 재사용하고, 나머지 안내는 튜토리얼이 쓴다. 시안의 문구는 예시다.
- `EdictCaption.Hard`가 참이면 게이트 안내(프롤로그), 거짓이면 링 안내다. 프롤로그 문구는 게임의 v3 문구를 그대로 쓴다.
- N6 라벨은 훈련장이 열려 있으면 "훈련장에서 확인", 아니면 "다음 사냥에서 관찰"이다.

## 10. 튜토리얼 측 요청

UI 범위 밖이며 Phase 4에서 튜토리얼 작업자가 반영한다.

| # | 요청 | 이유 |
| --- | --- | --- |
| T1 | 게이트가 지정 컨트롤의 사각형이 뷰포트·마스크 밖이면 "없음"으로 취급해 2초 뒤 해제한다(`PrologueGate`, `LessonTarget`) | 지금은 스크롤 밖 컨트롤이 "있음"으로 판정되어 가림막이 풀리지 않는다(코드 읽기 기준) |
| T2 | 규칙 안내에서 편집본에 변경이 생기면 링을 `edict-save`로 넘긴다(`RefreshTutorialAnchor`) | 이름 목록의 첫 발견 컨트롤에만 붙어 옵션 행·선택기가 보이는 동안 링이 저장에 닿지 않는다 |
| T3 | 기본 세팅 적용 중에는 `FocusGlobalOption`이 거절되므로 캡션이 `edict-default-settings`를 가리킨다(`StartTutorialAction`) | 지금은 링이 뜨지 않아 안내가 막힌다 |
| T4 | 캡션을 자체 패널 대신 창이 노출하는 캡션 줄(`edict-caption`)에 그린다 | 자체 패널이 프리셋 탭 등 컨트롤을 덮는다 |
| T5 | N4·N6 내용을 안내별로 넘긴다(`GameUI.EdictNotices`) | UI는 자리와 모양만 제공한다 |
| T6 | `EdictSaved`의 "수행" 판정이 공개된 규칙만 대상인지 유지한다 | N1과 같은 규칙 |
| T7 | 안내 카드 선택 순서(`Tutorials.All`의 JSON 순서)와 사다리 순서의 차이를 검토한다 | 새 문장 점과 카드가 가리키는 문장이 다를 수 있다 |

## 11. 이식 지도

시안 수치는 5장과 8장의 환산 규칙(`UiTheme.Scale`, 세로 405×880, 기존 높이에 곱해지는 `lastScale`/`Grow`)으로 읽는다. 아래 "추정"은 그 공식으로 계산한 값이며 측정이 아니다.

기준일 2026-10-05. 창 UI 코드는 튜토리얼 브랜치와 `main`이 같고(`Resources/HuntEdictProgression.json`과 `Core/TutorialChapters.cs`만 다르다), 아래는 코드를 읽어 정리한 것이다(빌드·테스트 실행 없음). 줄 번호는 이 날짜의 소스 기준이라 코드가 바뀌면 어긋난다.

약어: `W` = `Presentation/HuntEdictWindow.cs`, `Su`·`Ov`·`Qp`·`Op`·`Pr`·`Pz`·`Sk`·`Eq` = `HuntEdictWindow.{Summary, Overview, QuickPresets, Options, Progression, Presets, Skills, Equipment}.cs`, `Prog` = `Core/HuntEdictProgression.cs`. 스모크는 `S:` + 이름(Ovw = RuntimeEdictOverviewSmoke, Sec = RuntimeSkillTreeSmoke.Sections, Qp = RuntimeEdictQuickPresetSmoke, Shared = RuntimeSharedUiSmoke, Save = RuntimeHuntEdictSaveLayoutSmoke, Tut = RuntimeTutorialSmoke).

### 11.1 창 틀

| 소유 | 바꿀 것 | 함정 |
| --- | --- | --- |
| 머리줄 W:85-92 | 부제(:90-91, 직업 · 프리셋)를 직업 · Lv · 성향으로 | `CurrentStyle` Ov:10 재사용 |
| `Option area` W:112-113 | 안내가 있을 때만 높이를 예약한다. 가장 싼 방법은 이 `Place`를 줄이고 위에 형제 `Guide caption`을 두는 것이다(그리는 함수들이 모두 `bodyHeight`를 따른다) | 이름은 창의 유일한 직속 자식이어야 한다(S:Shared 69, `GameUI.TutorialStaging.cs:250`은 첫 "Option area"를 쓴다). 경로 `Option area/Overview custom settings`(S:Sec 20) 때문에 래퍼 자식을 넣을 수 없다. 줄이면 게이트 구멍에 캡션 줄이 빠지므로 `PrologueGate.cs:85-103`의 떠 있는 카드를 이 줄로 옮겨야 한다. 켜고 끄는 시점은 안내 활성 여부이며 `HasDialog`가 아니다(`Repaint`가 다이얼로그를 닫는다, W:138). `Update`(W:52-56)에서 처리 |
| 푸터 W:126-133 | `Save()`(:166-170) 뒤에 "저장했습니다"와 `edict-save-next`를 보인다. 라벨은 GameUI가 정한다(지금은 토스트, `GameUI.HuntEdict.cs:41-42`). 변경·되돌리기·이동 시 지우고 12초 타이머 | `DisableForDefaults(floating)`(:132) 앞에서 만든다. S:Sec 17-19는 잠금 중 활성 버튼을 4개로 고정한다. 상태 Text 폭은 `width-210`(:129). 변경 개수 API가 없어(`Session.Dirty` 불리언뿐, `HuntEdictLoadout.cs:102`) 개수 칩은 버린다. S:Save 45-56, S:Tut 134-136 |

### 11.2 탭 (W:94-111)

가로 172폭 한 열과 세로 `min(5,n)`열은 이미 시안과 같다(:96-97).

| 부분 | 바꿀 것 | 함정 |
| --- | --- | --- |
| 부제 | 가로이고 `visibleTabs.Length<=6`일 때만 둘째 `Text`. 새 문구는 `en.txt`에 | 탭 Text가 잘리면 안 된다(S:Ovw 72, S:Tut 132는 5개 크기 × 2개 언어). 시안처럼 `cellHeight`(:100)를 52로 제한한다. 지금은 늘어나서 탭 3개면 약 119 높이가 된다 |
| 세로 아이콘 | 세로에는 `Glyph`가 없다(:107). 라벨 위에 하나 더한다 | `Glyph` |
| 새 점 | 금색 원 자식, `raycastTarget=false`, 선택된 탭에서는 숨김 | 스킨 `unequip`(GoldHex) 또는 `HuntEdictSurface.cs:20-58`에 `new-dot` 추가 |
| L0 단독 탭 | `visibleTabs.Length==1`이면 `navWidth` 48(:97), 세로는 약 38 높이 한 줄, `cellHeight` ≤ 48. 라벨은 숨기고 아이콘을 가운데 | `IconButton` 패턴(W:258-262). 버튼 `edict-tab-skills`·`edict-tab-survival`은 유지. S:Tut.Progression 41은 보이는 탭 하나만 허용하고 :102는 마을에서 개요·스킬·생존만 요구한다. `Option area`의 x/y가 `navWidth`에서 나온다(:112) |

### 11.3 그룹 칩 (Su:44-59)

| 부분 | 바꿀 것 | 함정 |
| --- | --- | --- |
| 둘째 줄 | `QuickPresetSelection(scope)`(Qp:15-21)의 이름 또는 "직접 설정". 공개되지 않은 id가 있거나, 탭이 `autoEquip`이거나, `bag.warehouseFull`을 가진 그룹이거나, `survival.potion`이면 빈 문자열 | `QuickGroupSummary`(Qp:91-97, 지금은 안 쓰임)가 가깝다(그 안의 `VisibleSummary` 문구는 버린다). `HuntEdictQuickPresets.For(scope)`는 프리셋이 없는 범위(자동 착용 10개 그룹 전부)에서 예외를 던진다 |
| 높이 | `tabHeight` 측정(:46-50)에 둘째 줄을 포함 | S:Sec 67: 칩 Text 잘림 금지 |
| 변경 점 | "•"(:58) 유지. :54 람다 안의 `Session.Baseline`을 밖으로 뺀다 | `Baseline`은 접근할 때마다 깊은 복사를 한다(`HuntEdictLoadout.cs:99`) |
| 새 점 | 탭과 같은 점. 선택된 칩에서는 숨김 | |
| 유지 | 버튼 `edict-group-<ids[0]>`, `GroupGlyph`(`Glyphs.cs:8`) | S:Sec 64-69: `Option area` 안, ScrollRect 조상 없음(:65), `SkillTreeGraphic` 있음(:66), 겹침 없음 |
| N2 기본 그룹 | 새 그룹 > 기억된 그룹 > 첫 그룹 | 지금 Su:42는 탭과 무관한 `selectedGroup` 하나를 쓴다. 탭별 사전이 필요하다 |

### 11.4 그룹 본문

| 부분 | 소유 | 바꿀 것 | 재사용·함정 |
| --- | --- | --- | --- |
| 선택기 | Qp:47-63 | 이미 시안과 같다 | S:Qp 82: Text 잘림 금지 |
| 옵션 행 | Su:100-108 (두 줄, ≥62) | 한 줄: `Row(list,"Option "+id,max(36,34*lastScale),"option-card")` 안에 라벨, `?`, 오른쪽 값 우물 | Op:42-52가 한 줄 패턴이다(높이 35, "option-card" + "input-well"). `?`는 `Button(..,"tab-idle")` 이름 `edict-help-<id>`. `edict-option-<id>`는 직접 설정 모드에 모두 있어야 하고(S:Shared 92, S:Qp 136) 처음 방문·간편 모드에는 없어야 한다(S:Qp 134, 137). 비활성 행은 `interactable=false`로 남긴다. 변경 막대는 네이티브에 대응물이 없다 |
| 이유 줄 | Su:93-97 | 유지 | |
| 구역 제목 | Op:11-12 `SectionTitle`(24 높이, 잘림) | 12pt로 줄바꿈하는 `Heading`(Ov:43)으로 | Su:89-90에서 호출 |
| 간편 프리셋 다이얼로그 | Qp:64-90 | 그대로(고르면 적용, Qp:22-46) | "직접 설정"이 마지막이고 개수는 `For+1`(S:Qp 65) |
| 숫자 다이얼로그 | Su:146-155 | 범위 줄(:152)과 오류 Text(:149)는 이미 있다. 새 것은 칩뿐: 퍼센트 옵션(`d.id.ToLowerInvariant().Contains("percent")`, `HuntEdictSummary.cs:33`과 같은 판정)에 10~90을 최소·최대·단위로 거른 최대 6개 | 칩은 `input.text="60"`처럼 채우기만 한다(튜토리얼이 `=="60"`을 검사, `GameUI.TutorialComparison.cs:42`). `Repaint` 금지. 지금 y 칸(범위 98, 오류 125, 적용 178, 높이 230)을 아래로 민다. `edict-number-input`·`edict-number-apply` 유지 |
| 선택·복수·순서 | Su:121-145, Op:56-66 | 틀만 | 순서는 드래그 카드(Op:79-92, `HuntEdictOrderCard.Dragging`이 갱신을 막는다, W:50)이므로 시안의 ↑↓를 옮기지 않는다. 적용 버튼은 `edict-choice-apply` |

`bag.warehouseFull` 그룹과 `autoEquip` 탭은 `Eq:11,141`로 일찍 반환한다(Su:79-83). 네이티브 화면을 유지한다.

### 11.5 N4 띠

- **소유:** Su:60-61. **바꿀 것:** `top`에 스크롤 밖 고정 `Button(body,..,"chip-on")` 이름 `edict-beat-strip`을 넣고 `top += 띠 높이 + 4`. 높이는 접힘 28(가로·세로 같음), 펼침 51(가로)·117(세로)에 `Grow`를 곱한다. `stripOpen`은 창 필드이며 바꾸면 `Repaint`한다.
- **데이터:** 고칠 한 가지는 `Prog.Guide(id)`(Prog:95)의 규칙 본문. 겪은 문제·달라진 점은 GameUI가 가진다(`tutorialFocus`, `GameUI.Tutorials.cs:17`, 첫 균열 종료 안내의 위험 문구 `GameUI.EdictNotices.cs:24-29`).
- **함정:** ScrollRect 밖에 둔다. `Selected edict group` 뷰포트는 90 이상이어야 한다(S:Sec 70). 최악은 가로 자동 착용: 칩 2줄 + 캡션 줄 + 펼친 띠. GameUI가 소유한 상태는 명시적 setter와 `Repaint`가 필요하다(`StoreViewBinding`은 저장소 커밋에만 반응한다). 기본 세팅 잠금에서는 버튼을 잠근다(반복 탭은 W:121에서 감싼다).

### 11.6 개요

| 부분 | 소유 | 바꿀 것 | 함정 |
| --- | --- | --- | --- |
| 성향 카드 | Ov:30-65 | 수치 칩 2개를 **버튼이 아닌** Panel(`chip-off` + Text)로 더한다. 줄 높이 평탄화(:59-63) 전에 LayoutElement를 올린다. 수치는 `HuntEdictQuickPresets.Style(id).For(class)`의 선택 → 프리셋의 `survival.lowHpPercent`와 회피 프리셋 `.Name` | S:Ovw 83-100: `Option area` 아래 버튼은 성향 3 + `edict-default-settings`뿐, ScrollRect 1, 성향 Text 잘림 없음. `balanced` 프리셋에는 `lowHpPercent`가 없으므로 옵션의 `initial`로 대체한다. `GlobalValue`는 private |
| 기본 세팅 줄 | Pz:69-80 | 유지(Sharing이 없으면 이미 높이 0) | 이름 `edict-default-settings`·`edict-default-status`(S:Sec 141) |
| 티저 | Ov:27 | 같은 목록에 **버튼이 아닌** Panel `edict-teaser`. 막대는 Panel 둘 | `DisableForDefaults(custom)` 안. 스킨은 `skill-card` 또는 새 것 |
| `Next` | Prog:179-188 | 문자열을 돌려준다. 소비자는 Ov:27 하나뿐이고 테스트는 없다 | 낡은 `en.txt` 6564-6571행은 표식(3492행) 아래라 무해 |

`Next`의 구조화 대체안은 `readonly struct EdictNext { kind(first|rift|rune), guide, title, stage, now }`를 돌려주는 `Prog.Upcoming(a)`이다. 분기 순서는 `Next()`와 같다: 레거시는 null, `Dodge` 없음이면 first/F05, `Styles` 없음이면 단계 2, 최고 클리어 ≥ 5이고 `Details` 없음이면 단계 6, 그 밖에는 아직 부여되지 않은 id가 있는 가장 낮은 단계의 규칙(`special-equipment` 제외), 최고 ≥ 15이면 룬. `now`는 `ContentUnlocks.AccountClear`(`ContentUnlocks.cs:37`). 문장 번호 n/15의 15단 사다리는 네이티브에 없으므로(시안 `engine.js` 30-35) 더해야 한다. 쪽수는 튜토리얼 브랜치에서 `TutorialChapters.Pages(a)`를 `All.Length`(11)로 나눈 값이다.

### 11.7 다이얼로그

`HasDialog`는 `modal!=null`이다(W:37).

- **틀:** W:195-202의 `Dialog()`가 간편 프리셋·숫자·복수·순서·선택·도움말·`Error`·`Confirm`·`RequestLeave`를 모두 만든다. `modal` 설정(:197), `edict-dialog-dismiss`(:203-212), 450폭 `dialog-panel`, `edict-dialog-close`.
- **스킬 말풍선:** Sk:285-324가 `modal`을 설정(:300)하고 `edict-skill-menu-dismiss`, 패널 스킨 `skill-action-bubble`, 버튼 `edict-slot-remove|equip|policy`를 만든다. 튜토리얼은 `HasDialog`/`HandleBack`으로 닫는다(`GameUI.TutorialStaging.cs:237-238`).
- **푸터 영역 없음:** 적용 버튼은 `h-48`에 둔다. 그대로 유지한다.
- **닫기 확인은 다르다:** 시안의 "취소/확인"은 네이티브 닫기 흐름 `RequestLeave`(W:157-165, 되돌리거나 저장하고 나가기)와 다르다. `Confirm`(:223)은 스킬 초기화에만 쓴다.
- **함정:** `Reflow`가 `restoreDialog`로 다이얼로그를 복원한다(W:61-77). 탭 위에서도 바깥 클릭이 닫기에 닿아야 한다(S:RecommendedEquipment.Controls 67). 다이얼로그가 열려도 캡션 줄 예약은 유지한다.

### 11.8 `DisclosureDisplayKey` (Pr:62-63)

파생된 스칼라만 더한다.

- `string.Join(",", 새 안내 id)`.
- 티저용 `AccountClear`, `contentUnlocks.firstRunEnded`, `guide.runeBoard?.step`.
- 받은 쪽 수용 `TutorialChapters.Pages`.
- N4 문제 문구용 최근 기록 id 또는 `Count`.

`guide.tutorials` JSON은 넣지 않는다. `Tutorials.Discover`(`GameUI.HuntEdict.cs:13`)와 `Tutorials.Record`(프레임마다 호출, `GameUI.EdictNotices.cs:16`)가 그 데이터를 바꾸므로, 넣으면 저장이 바뀌지 않았는데도 창이 다시 그려져 S:Tut.Progression 104-105가 깨진다. 편집본 상태·시간·GameUI 포커스도 넣지 않는다(명시적 `Repaint`가 필요하다).

### 11.9 "새 문장" 계산(N1)이 읽을 곳

- **읽는 곳:** `store.Data.guide.tutorials`(`FirstPlayGuide.cs:20`)의 `TutorialRecord`(`Tutorials.cs:8-12`).
- **미수행:** `practiced`를 읽는다. `TutorialProgress.EdictSaved`가 `Tutorials.Performed`로 채우며 `GuideVisible`로 거른다(`Tutorials.Progress.cs:31-60`).
- **조회 방법:** `TutorialChapters.Finished`처럼 `tutorials.Find(x => x.id==g && x.hero==(Tutorials.Definition(g)?.perHero==true ? hero.id : "") && x.variant=="")`(:30-34). 기록을 덧붙이는 `Tutorials.Record`(:105-110)는 쓰지 않는다. `Finished`도 쓰지 않는다(E* 안내는 `read`만으로 끝난 것으로 센다).
- **공개 여부:** `Prog.GuideVisible(a, g)`(Prog:96-104).
- **보호:** `Legacy`, `guide.hintsHidden`, `Tutorials.Mandatory`일 때는 건너뛴다. 아니면 낡은 저장에서 점이 전부 켜진다. `legacyExempt`는 `Tutorials.Available`이 처리한다.
- **규칙 → 안내:** `GuideOf(rule)` 도우미가 없다. 시안의 것(`engine.js:45`)은 `repeat.enabled`를 H11, 첫 일반 균열 규칙을 F05로 매긴다. `Prog.Guide`(:95)는 둘 다 놓친다.
- **규칙 → 그룹:** 규칙의 id를 하나라도 가진 모든 그룹.

### 11.10 세션 동안 연 그룹

- **기존 패턴:** `GameUI.shownTutorialContexts`(`GameUI.Tutorials.cs:16`, 추가 :142, 읽기 :150)와 `edictNoticePresented`(`GameUI.EdictNotices.cs:7`). 창 수명의 집합(`collapsed` W:29, `customQuickScopes` Qp:13)은 `Destroy`(W:156)에서 사라진다.
- **안:** `opened` 집합을 GameUI에 두고 `HuntEdictWindow.Open`(`GameUI.HuntEdict.cs:44`)에 넘긴다. `SelectTab`(W:140-148)과 `SelectGroup`(Su:25-31)에서 더한다. 둘 다 이미 `Repaint`한다.

### 11.11 스킬 탭 (비레거시 경로)

스킬 탭 줄 번호는 `main` 체크아웃 기준이다. 튜토리얼 브랜치에서는 같은 코드가 `Skills.cs` 196행 뒤로 17줄 앞선다(레거시 트리 부분만 다르다). 아래 "추정"은 읽기 배율 1(고정, `GameUI.cs:71`)에서 공식으로 계산한 값이며 측정한 것이 아니다.

- **그리는 곳.**
  - 배치: `Skills.cs:34-64`(정책 레일 76 :42, 관리 가로 :52-55, 관리 세로 목록 → 인스펙터 → 바 :57-63).
  - 카드: `Skills.cs:70-75` → `Progression.cs:15-25` → `Card()`(`Overview.cs:30-41`).
  - 인스펙터: `Skills.cs:376-421`(−/＋/장착 :418-420). 바: `Skills.cs:333-371`.
  - 말풍선: `Skills.cs:285-324`(모달이므로 `HasDialog`, 앵커 옆에 배치 :316-323).
- **시안이 실제로 바꾸는 것.** 프롤로그 슬롯 1칸, 레벨 40의 4번 칸(`Progression.cs:13`), 세로 순서, −/＋/장착 세 버튼은 **이미 네이티브와 같다.** 실제 차이는 다음뿐이다.
  - 패시브 아이콘을 둥근 사각형으로(`Card`가 `Create(..,false)`를 하드코딩, `Overview.cs:33`).
  - 장착 중 부제를 초록으로(`Overview.cs:38`), 제목 색을 금색에서 본문색으로(`Progression.cs:21`).
  - 목록 최소 높이 120(`Skills.cs:60`, 시안 `styles.css:222`).
  - 슬롯 말풍선을 슬롯 **위**에(시안 `ui.dialogs.js:29-32`, 네이티브는 옆).
  - 인스펙터 메타에 원소 대신 가지 이름(`Skills.cs:398-399`, 시안 `ui.skills.js:36`).
- **유지.** `SkillBarHeight`(:336)가 바 높이 예산의 유일한 원천이다(:46, 52, 59-62). `BarCell`(:333)은 슬롯 히트 폭이기도 해서 슬롯 1칸의 튜토리얼 구멍이 바 폭 전체가 된다. `ActiveHole/PassiveHole/UltimateHole/BarGap`(:29)이 `FrameSize`(:236), 아이콘(:355), 인스펙터(:387-395)에 쓰인다. 시안의 CSS 고리는 대용이므로 `NodeFrame` 아트를 유지한다. `VisibleSlots`(`Progression.cs:13-14`)와 `Grow/Lines`(:30-32)도 유지한다.
- **함정.** 카드는 `skillNodeAnchors`(`Progression.cs:22`)에 등록되는 `edict-skill-<id>` Button이어야 하며 프롤로그에서 스모크는 최대 1개를 허용한다(`RuntimeTutorialSmoke.Progression.cs:42`). 네이티브의 −는 `rank > MinimumRank`(:418)이지 시안의 `r > 0`이 아니다.

### 11.12 정책 화면 (`SkillPresets.cs:53-156`)

지도: 뒤로 :58, 탭 :66-92, 판 :93-94, `mainScroll` :96(:95의 `actions=0`은 고정 띠 여유), `Preset explanation` 행 :115(정보 폭 .28 :114), 실시간 가지 :129-149(`DrawCombatPreview` :140 → `DrawStarterControls` :141, `Progression.cs:26-38`), 행 높이 :151.

- **(a) 진행 버튼 위치.**
  - 세로는 지금 전투 예시가 먼저(:144-148)라 버튼이 맨 끝에 온다. 그 블록을 `artY = descriptionHeight + 4; Place(art, artX, artY, artW, artH)`로 바꿔 정보가 위에 오게 한다(시안 `styles.css:256-257`).
  - 가로는 :141 앞에 `Row(information, "Starter spacer", 0)`(`flexibleHeight=1`)를 더하고 :143에서 정보 칸을 `Max(descriptionHeight, artH)`로 놓는다(시안 `styles.css:246`). 폭은 .28에서 약 .34로 넓힌다.
- **(b) 스크롤 없이 맞추기.**
  - 프롤로그 뷰포트는 약 h−79(추정)다: 가로 ≈329, 세로 ≈579, 캡션 줄을 빼기 전이다. 이 화면에는 푸터가 없다(`HuntEdictWindow.cs:93`).
  - 내용은 뷰포트−9 이하여야 한다(목록 간격 + `EndList` :155).
  - 지금 가로 정보 열은 ≈355(추정)이고 고정 행만 ≈250이다(`CombatPreview.cs:37-41`). 세로 행은 ≈590이다.
  - `ProgressivePrologue`에서는 통계를 1줄로 줄이고, 재사용 대기·효과 행을 없애고(`CombatPreview.cs:50-61`에 null 가드), 캡션을 2줄 이하로 한다.
- **(c) 관찰 진행 막대.**
  - 상태는 `Core/CombatPreviewSession.cs`에 있다: `Seconds` :29, `StarterObservationComplete` :21-22(Seconds ≥ 10, CAST_START, private `waited`/`moved` 플래그 :18), `Ended` :28(10초에 시계가 멈춘다). `PreviewCombat`(`CombatPreview.cs:11`)으로 닿는다.
  - 프레임마다 읽을 수 있다. `SkillCombatPreviewPresenter.cs:72-75`가 매 프레임 진행하고 보이는 동안 30Hz 이하로 `RefreshLabels`를 부른다. 그 클로저(`CombatPreview.cs:44-65`)가 이미 `Seconds`를 찍고(:49) `UpdateStarterControls`를 부른다(:47).
  - 편집: :141 앞에 채움 행을 더한다. 폭은 `Clamp01(Seconds/10)`이며 `Complete` 전에는 .99로 제한한다(아니면 가득 찬 막대에 버튼은 비활성이 된다). `CombatPreviewSession.cs:21, 28`의 `10-.001f` 리터럴은 상수로 뺀다.
- **함정.**
  - 오프셋 키는 `skills-edict`(`HuntEdictWindow.cs:39`)다. `mainScroll`만 기억되고(:186-193) `SkillPresets.cs:32, 86`에서 0이 된다. 둘째 `Scroll()`을 만들지 말고 `actions`를 쓴다.
  - `previewSkillPresets`는 범위별(:11)이다. `ActivateSkillPreset`은 `VisibleSkillPreset`을 저장하므로(:36) 라디오는 먼저 그것을 설정해야 한다(:86).
  - `UpdateStarterControls`(`Progression.cs:55-61`)는 `starterContinue==null`이면 아무것도 하지 않는다. 전투 예시 → 진행 버튼 순서와 이름을 유지한다. `LessonTarget`은 상호작용 가능한 버튼만 돌려준다(`GameUI.TutorialStaging.cs:253`).
  - 프리셋별 "관찰함" 기억이 없다(프리셋이나 서명이 바뀌면 세션을 다시 만든다, `SkillCombatPreviewPresenter.cs:27`). 시안의 "관찰 완료" 칩은 네이티브 원천이 없다. 다이얼로그가 열려 있으면 시계가 멈춘다(`CombatPreview.cs:42`).

### 11.13 튜토리얼 층

- **지금.**
  - `PrologueGate`가 "Voice narration"을 만든다(`PrologueGate.cs:29-32`). 58·s 얼굴 크롭이 시안 후광의 대용이고 이름은 `NpcProfiles.cs:27`에서 온다.
  - 캔버스 "Prologue gate"는 오버레이, 정렬 순서 2000, 픽셀 단위다(`GameUI.cs:143-147`, 생성 `TutorialStaging.cs:32-36`). 창 캔버스는 310이다.
  - 카드 높이는 최소 74·s다(:87-91). 배치(:93-103)는 구멍에서 먼 가장자리, 또는 구멍이 카드 2장 높이 이상이면 구멍 위·아래이므로 큰 구멍에서는 `Option area` 위에 뜬다.
  - 구멍(:70-84)은 `target`의 월드 코너(화면 px)이며 **마스크로 자르지 않는다.** `blocker.raycastTarget = missing < 2초`(:80).
- **캡션 줄에 안착시키기.**
  1. `HuntEdictWindow.cs:112-113`: `ProgressivePrologue`(또는 부드러운 안내 줄)이면 "Option area" 맨 위에 공개 RectTransform "Guide caption"을 둔다. 내용은 반복 탭이 쓰는 본문 교체 방식(:119-121, `bodyHeight -= band`)으로 자식 호스트에 옮긴다. 이름 "Option area"는 유지한다(`TutorialStaging.cs:250`). 지금 카드로는 띠가 74 이상이고, 시안의 44/58에는 더 작은 얼굴·여백·글꼴이 필요하다(34·s 얼굴이면 약 51). 11.1의 "형제로 두는" 안이 이 방식보다 간단하다.
  2. `PrologueGate.cs:85-103`: `public RectTransform Dock`을 더한다. 설정되어 있으면 내레이션 사각형은 그 월드 코너다. 룬 보드(`GameUI.RuneBoardTutorial.cs:41, 67, 91`)에는 null로 두고 이름 "Voice narration"(스모크 `RuntimeTutorialSmoke.cs:128`)을 유지한다.
  3. `TutorialStaging.cs:239`: `gate.Dock = huntEdictWindow?.GuideCaption`.
  - 띠는 열 때 예약하고 프레임마다 바꾸지 않는다. `Repaint()`가 다이얼로그를 닫기 때문이다(`HuntEdictWindow.cs:138`).
- **(i) 화면 밖 컨트롤은 "없음".**
  - `LessonTarget`(`TutorialStaging.cs:246-255`)과 버튼 검색(`GameUI.Tutorials.cs:164`)을 고친다.
  - `GetWorldCorners`를 각 조상 `RectMask2D`와 교차시키는 도우미를 더한다. `parent`부터 시작한다(`Option area` 자체가 마스크를 가진다, :112).
  - 컨트롤은 완전히 포함되어야 하고 "Preset explanation"은 겹치기만 하면 된다. 실패하면 null을 돌려 게이트의 2초 유예를 쓴다.
  - EnsureVisible은 없다. 스크롤 코드는 `Skills.cs:289-298`에 인라인이며 `FocusGlobalOption`은 스크롤하지 않는다(`Summary.cs:30`).
- **(ii) 링 → 저장.** `GameUI.Tutorials.cs:155-164`에서 `edict-save`를 `names` 맨 앞으로 옮긴다. 상호작용 가능할 때만 찾히며 이는 `Session.Dirty`와 같다(`HuntEdictWindow.cs:131`). `HasDialog`이면 건너뛴다. 폴링은 0.25초다(:117).
- **(iii) 잠금 중 포커스.**
  - `HuntEdictWindow.Summary.cs:19-21`과 :27은 조용히 반환한다. `FocusGlobalOption`이 `bool`을 돌려주게 하고(현재 호출자는 모두 문장이다) private 잠금(`HuntEdictWindow.cs:38`)을 노출한다.
  - `StartTutorialAction`(`GameUI.Tutorials.cs:98-101`)에서 안내 줄을 정하고 `edict-default-settings`(`HuntEdictWindow.Presets.cs:75`, Sharing이 공개된 뒤에만 존재 :71)에 링을 건다.
  - 새 문구는 `en.txt`가 필요하다(영어 게이트 문구는 스모크가 검사한다, `RuntimeTutorialSmoke.cs:125`). `en.txt:6254`는 "Voice from Above"이고 시안은 VOICE OF THE EDICT다.

### 11.14 마을 안내 카드 경로

- **지금.** `GameUI.EdictNotices.cs:15-16`이 `Tutorials.All`의 첫 항목을 고른다(사다리 순서가 아니다). :21이 `StartTutorialAction`(`GameUI.Tutorials.cs:80`)을 부르고, 그것이 `tutorialFocus`(:82, GameUI 내부, 수행되면 지워진다 :137)를 정한 뒤 `ShowEdictEditor`(:96)와 `FocusGlobalOption(id)`(:98-101)를 부른다. 창은 옵션 id만 받는다.
- **더할 것.**
  - :96 뒤에 `window.ShowGuide(guideId, focusId, title, problem, fix)`를 부른다. 제목·고칠 한 가지는 `EdictDisclosureRule.title/body`(`Core/HuntEdictProgression.cs:8`) 또는 `TutorialDefinition.bodyKo`에서 가져온다.
  - 창이 사본을 갖고, 3박자의 셋째는 `Tutorials.Record(a, id).practiced`를 읽는다. 띠는 `Summary.cs:60-61`의 칩과 `editorScroll` 사이에 그린다(시안 `ui.tutorial.js:94-108`).
  - 문제 문구는 `EdictNotices.cs:24-30`을 카드와 띠가 함께 쓰는 도우미로 올린다(F05 문구는 `startingHp5s → finalHp`, `burstDamage5s`, 상위 `fatalSources`). E03·E07은 `HuntEdictCoach.Tips` 사유(`Core/HuntEdictCoach.cs:26-70`, 런타임 호출자 없음)를 재사용할 수 있다. E04·E05 문제 문구는 없다(시안 예시뿐).
- **N6.** `GameUI.HuntEdict.cs:41-42`의 `Save` 클로저가 이미 성공 여부와 `tutorialFocus != ""`를 안다(지금은 토스트). 거기서 `window.ShowAfterSave(label, action)`을 부른다. `Repaint()`(`HuntEdictWindow.cs:166-170`)보다 먼저 실행된다. 푸터(:129)에 그린다. 상태 폭이 width−210이라 405에서 약 195로 빠듯하다. 행동은 `game.RequestStation(TownStation.Training)` 또는 토스트다. 새 표시 상태는 `DisclosureDisplayKey`(`Progression.cs:62-63`)에 들어가야 한다. 아니면 `StoreViewBinding`이 갱신을 건너뛴다.

### 11.15 튜토리얼이 읽는 공개 멤버 확인

(소유 | 읽는 곳.) 순수 표면 개편은 배선이 유지되는 한 모두 그대로다. **굵은 글씨**는 조용히 바뀔 수 있는 것이다.

- `SelectedTab` `HuntEdictWindow.cs:35` | `TutorialComparison.cs:15`. :81, 83, 95에서 다시 쓰인다.
- **`ManagingSkills`** :36 | :16, 24. false가 정책 화면이며 푸터(:93), `ScrollKey`(:39), 미리보기 `Visible`(`CombatPreview.cs:42`), `HandleBack`(:58)도 좌우한다.
- `SelectedSkill` `Skills.cs:12` | :18. 카드를 누를 때만 `SelectSkill`(:22)이 설정한다.
- `PolicySkill` :13 | :25. 기본 "BASIC"이며 null이 아니다.
- **`VisibleSkillPreset`** `SkillPresets.cs:16` | :28. 범위별 `previewSkillPresets`가 유지된다. 시안은 열 때 초기화한다.
- `ActiveSkillPreset` :15 | :29. 저장된 영웅을 읽고 호출마다 세션을 할당한다(갱신당 4회, `Progression.cs:59-60`).
- **`HasDialog`** `HuntEdictWindow.cs:37` | `TutorialComparison.cs` :24, 39, `TutorialStaging.cs` :237. 스킬 말풍선은 모달이어야 한다(`Skills.cs:300`).
- `Session.Draft/Dirty` :31 | `TutorialComparison.cs` :17, 44, `TutorialStaging.cs` :268, 306. `Change`마다 Draft가 교체된다(:175).
- **`PreviewCombat`** `CombatPreview.cs:11` | `Progression.cs:35, 41, 60`. 이번 그리기에서 `DrawCombatPreview`가 돌지 않았으면 null이다(`HuntEdictWindow.cs:84`에서 해제).
- `ProgressivePrologue` `Progression.cs:11`은 창 내부 값이고 튜토리얼은 `RunState.tutorialFlowVersion`(`TutorialStaging.cs:258`)을 읽는다. `Legacy()`나 레벨 < 2를 검사하는 새 코드 경로는 탭 1개·카드 1장·슬롯 1칸·직접 설정 없음을 깨뜨릴 수 있다.
- `QuickPresetSelection` `QuickPresets.cs:15`와 `SelectedGroup` `Summary.cs:13`(키는 `ids[0]`, `Core/HuntEdictSummary.cs:9`)은 v2 흐름만 읽는다(`TutorialStaging.cs:267-316`). N2 기본 열림은 `Summary.cs:42`가 고르는 그룹을 바꾼다.
- GameUI도 부른다: `HandleBack`, `SelectTab`, `SelectSkillPage`, `FocusGlobalOption`, `SelectSkill`·`OpenSkillPolicy`(`GameUI.FirstPlay.cs:22`, `GameUI.TrainingGround.cs:41`), `Session.ReplaceDraft` + `Repaint`(`GameUI.Comparison.cs:213`).

### 11.16 공통 함정

1. `Repaint`는 전부 다시 만들고 다이얼로그를 닫는다. 튜토리얼 링은 0.25초마다 다시 붙는다(`GameUI.Tutorials.cs:117`). 프레임마다 `Repaint`하지 않는다.
2. `Button()`은 `.name`을 정하지 않으면 라벨로 오브젝트 이름을 짓는다. 스모크는 "확인"·"취소"를 라벨로 찾는다. 안내 대상은 `interactable && activeInHierarchy`여야 한다(`GameUI.TutorialStaging.cs:253`).
3. 새 Button은 모두 `DisableForDefaults` 영역 안에 둔다.
4. `Text()`는 입력에 `Loc.T`를 돌린다. 새 한국어 리터럴은 `en.txt` 항목이 필요하고 합성 문장은 `Loc.F`를 쓴다(`LocalizationTests.cs:239`). 시안의 `OWN_EN`(`ui.core.js`)이 출발점이다.
5. Text 사각형은 최소 `preferredHeight`여야 한다(S:Tut 132-133이 프로필 10종을 검사한다).
6. 옮기지 않을 시안의 단순화: 일반 자동 착용 행, 순서 ↑↓, 토스트뿐인 코드 공유, "취소/확인" 닫기 대화상자.

## 12. 영향받는 스모크·테스트와 새 검사

기준일 2026-10-05, 코드를 읽어 정리했고 실행하지 않았다. `P/` = `Assets/HELLSCRIPT/Runtime/Presentation/`, `T/` = `Assets/HELLSCRIPT/Tests/Editor/`. 변경 기호: (a) 옵션 행 한 줄(36) (b) 그룹 칩 둘째 줄 (c) L0 좁은 탭 열 (d) 캡션 줄 예약 (e) 개요 티저 (f) N4 띠 (g) 푸터 `edict-save-next` (h) 숫자·선택 다이얼로그의 칩과 범위 줄.

### 12.1 먼저 읽을 것

- **기준선이 이미 빨갈 수 있다.** 93d59959(2026-10-04)부터 신규 계정은 점진 공개 계정이다(`Core/GameStore.cs:265`). `RuntimeTutorialSmoke.cs:146`만 `edictLegacyAccess`를 설정한다. `highestClear=0` 픽스처는 개요·스킬·생존 탭만 보이므로, 아래의 "탭 10개/그룹 35개" 단언은 픽스처가 접근 권한을 주지 않으면 실패한다(`guide.edictLegacyAccess=true; HuntEdictProgression.Reconcile(a)`).
- **SharedUi는 또 다른 방식으로 낡았다.** `RuntimeSharedUiSmoke.cs:87-92`는 퀵 선택기가 없는 그룹(`survival.potion`, `bag.warehouseFull`, `autoEquip.*`)에서도 `edict-quick-picker-*`를 누른다. 표면 개편을 탓하기 전에 각 스모크를 손대지 않은 브랜치에서 한 번 돌려 기준선을 확인한다.
- **브랜치 차이.** 두 체크아웃의 스모크는 같다. 튜토리얼 브랜치는 Core·데이터·테스트만 다르다.

### 12.2 고치거나 확인할 스모크·테스트

| 파일:줄 | 단언 | 깨지나 | 수정 |
| --- | --- | --- | --- |
| `P/RuntimeEdictOverviewSmoke.cs:88-91` | `Option area`의 버튼은 `edict-style-*` 3개 + `edict-default-settings`뿐, ScrollRect 1, `SkillIconView` 없음 | 티저·성향 칩·캡션이 Button이거나 둘째 ScrollRect·아이콘일 때만(e) | 그대로. 티저·칩은 기존 목록 안의 Image + Text |
| `:92-98` | 성향 카드가 뷰포트 안, Text 잘림 없음 | 아니오(카드·티저·44/58 캡션이 5개 크기에 들어간다, 계산상). 칩 Text는 `preferredHeight`로 사각형을 준다 | 그대로 |
| `:59-74` CheckFrame | 탭 10개와 저장/되돌리기가 프레임 안, 탭 중앙 레이캐스트, 탭·스크롤 Text 잘림 없음 | (c)로는 아니오. 부제 Text를 켠 채 줄이면 :72가 깨진다 | 그대로. 부제는 `SetActive(false)`로 숨긴다 |
| `P/RuntimeSharedUiSmoke.cs:66-73` | `Main tabs`, `Fixed save controls`, `Option area`가 `Hunt Edict window`의 자식, 본문이 푸터 위, 탭 10개가 nav 안 | 이름과 부모가 유지되고 캡션이 형제이면 아니오 | 그대로 |
| `:97`, `:115-119` | 선택지를 `GetComponentInChildren<Text>().text==Loc.T(..)`로 찾음, `Single(InputField "edict-number-input")` | 한 줄 값 알약이 " ›"를 빼고 같은 글자를 보이면 :97(a). 칩이 InputField면 :119(h) | 버튼 이름으로 찾는다. 칩은 Button |
| `P/RuntimeHuntEdictSaveLayoutSmoke.cs:25-56,72-77` | `Node()`는 이름의 첫 항목, 저장·되돌리기는 `Single`, 저장이 되돌리기 오른쪽 한 줄 | 아니오. `edict-save-next`는 `Single`에 걸리지 않는다(g) | 그대로. 이름은 유일하게 |
| `P/RuntimeSkillTreeSmoke.Sections.cs:14-49` CheckDefaultLock | 비활성 4개를 뺀 모든 Button(:19), `Option area/Overview custom settings` + CanvasGroup(:20), ScrollRect 잠김(:31-38) | `edict-save-next`가 흐려지는 푸터 밖에 있거나 캡션·티저가 Button이면(g) | 그대로. 경로 유지 |
| `:57-73` CheckSectionFrame | 칩이 `Option area` 안(:64), ScrollRect 밖(:65), `SkillTreeGraphic`(:66), Text 잘림 없음(:67), 뷰포트 ≥ 90(:70), Text 하나가 `Description`과 같음(:72) | 둘째 줄(프리셋 이름)을 재지 않으면 :67(b). 설명이 잘리거나 옮겨지면 :72. :70은 유지된다(최악 자동 착용 10그룹 + 캡션 + N4에서 약 120+ 논리 px 남음) | 부제를 측정 루프에 넣는다(`P/HuntEdictWindow.Summary.cs:47-50`). 스크롤 안에 정확한 설명 Text를 유지 |
| `:50-56`, `:74-91` | 모든 ScrollRect 세로, 프리셋 페이지는 ScrollRect 0 | 프리셋 페이지에서 캡션이 `Option area`를 줄일 때만(d) | 그대로(픽스처가 `hintsHidden`) |
| `P/RuntimeSkillTreeSmoke.Presets.cs:24,33` | 탭 10개 + 활성화·뒤로·공통 정책이 프레임 안, 잘림 없음, `Skill preset content` 뷰포트 > 40 | 아니오 | 그대로 |
| `P/RuntimeSkillTreeSmoke.cs:351` | `edict-tab-combat` 중앙 클릭이 `edict-skill-menu-dismiss`에 닿음 | 아니오(배경이 창 전체, `P/HuntEdictWindow.Skills.cs:302`) | 그대로 |
| `P/RuntimeEdictQuickPresetSmoke.cs:77,82,134-137` | 탭·저장/되돌리기 프레임 안, 선택기 Text 잘림 없음, 직접 설정 전에 `edict-option-*` 없음 · 후에 모두 있음 | 아니오. (a)는 비활성 옵션 행도 만들어야 한다. 검색은 프롤로그에서만 숨길 수 있다 | 그대로 |
| `P/RuntimeRecommendedEquipmentSmoke.cs:57-63`, `.Controls.cs:58,62,67` | 같은 프레임 검사 + `CheckText`, `edict-tab-overview` 중앙의 바깥 클릭이 다이얼로그를 닫음, `Dialog` 3px 모서리가 비어 있음 | 아니오: 다이얼로그 ≤ 450폭, 가로 탭 열 x < 172, 세로 탭 줄이 위쪽. (h)의 칩이 그 모서리에 가면 안 된다 | 그대로 |
| `P/RuntimeTutorialSmoke.cs:111-139` | :119 `Single("Hunt Edict window")`, :125 영어 게이트 Text에 한글 없음, :128 `Single("Voice narration")` 대 일시정지/재시작, :132 버튼 Text 잘림 없음, :134 푸터 버튼 프레임 안 | :128은 캡션이 게이트 카드를 대체하면 깨진다(카드가 비활성이라 `Single`이 예외). :125는 검사 범위가 줄어든다. :132는 48px 탭 라벨이 켜져 있으면 깨진다(c) | :128은 활성 카드를 `FirstOrDefault`로, 없으면 `Guide caption`. 캡션 Text에 한글 검사를 추가. 좁은 탭의 라벨은 비활성화 |
| `:74-83` FollowGate | 하나뿐인 탭의 중앙이 "Input blocker"에 닿음 | 아니오(구멍은 정확히 대상 사각형, `P/PrologueGate.cs:76`) | 그대로 |
| `P/RuntimeTutorialSmoke.Progression.cs:41,102,105` | 레슨 중 `edict-tab-skills`만, 마을 탭은 개요·스킬·생존, 무변경 저장은 `edict-save` 인스턴스 유지 | 숨은 탭이 비활성이고 N1이 `DisclosureDisplayKey` 입력만 읽으면 아니오 | 그대로 |
| `:108-111` | `highestClear=3` 뒤 `edict-tab-combat`이 존재 | 튜토리얼 브랜치에서는 깨진다(전투 탭이 균열 4에 열린다, main은 3) | 병합 뒤 4로 |
| `P/RuntimeButtonUxSmoke.cs:184-185` | 모든 `UiButton`에 비레이캐스트 Face와 Chosen/Available 일치, `Single(edict-save)` | 새 버튼이 `Button()`으로 만들어지면 아니오 | 그대로 |
| `P/RuntimeRiftEntrySmoke.Repeat.cs:58-71` | `edict-tab-repeat` 잠금·자기 중앙 레이에 맞음, :63 `Loc.MissingCount==0` | 탭은 아니오. :63은 `en.txt` 키 없이 스킬 페이지에 그린 새 한국어 문자열(미리 만든 숨은 부제 포함)이 있으면 깨진다 | 키를 추가한다. 부제는 보일 때만 만든다 |
| `P/RuntimeLanguageSmoke.cs:183-187`, `P/RuntimeUiStyleSmoke.cs:69-70` | `ShowLegacyEdictEditor`의 레거시 페이지, 캡처만 | 아니오 | 그대로. 스크린샷 갱신 |
| `T/HuntEdictOverviewTests.cs:24-166`, `HuntEdictDefaultsTests.cs:70-74`, `SharedUiTests.cs:91-104`, `StoreViewBindingTests.cs` | 탭·성향 id, 코치 팁, 그룹 필드, 그룹 35개, 일반 하네스 | 아니오 | 그대로. `DisplayKey`(`P/HuntEdictWindow.Progression.cs:62`)를 시험하는 테스트가 없으므로 새 테스트 (2)를 더한다 |
| `T/HuntEdictProgressionTests.cs:65-74` | 152개 id, `Single(stage==18)`이 40개, TestCase | 표면 개편은 아니오. 튜토리얼 브랜치는 :69와 :73-74에서 깨진다 | 브랜치 파일을 채택한다(`Where(stage==18).Sum==40`, `Count==3`, 낮은 HP@3, 위치@5, 18의 `target.chaseDistance`·`bag.replacementRank`). 15단 사다리는 `HuntEdictProgression.json`에 넣지 않는다. 규칙 getter가 옵션 id를 정확히 한 번씩만 허용하고 18단계 개수를 단언한다 |
| `T/TutorialProgressionTests.cs:20`, `TutorialChapterTests` | 정의 52개(브랜치 54), 챕터 | 아니오 | 없음. 티저 쪽수는 병합 뒤 `TutorialChapters.Pages(a)`를 `All.Length`(11)로 나눈다 |

이름만 찾아 누르는 그 밖의 스모크(`RuntimeRiftEntrySmoke.Portal`·`Preparation`, `RuntimeTrainingEquipmentSmoke:121-158`, `RuntimeTrainingGroundSmoke:176-246`, `RuntimeRiftVictorySmoke:232`, `RuntimeFirstPlayAcceptance:200`)는 그대로다. `RuntimeSkillTreeSmoke.CombatPreview.cs:51`은 미리보기 조작 Text의 잘림을 검사한다.

### 12.3 `LocalizationTests`와 `en.txt` (`T/LocalizationTests.cs:177-256`)

- `Runtime/**/*.cs`의 보간되지 않은 한국어 리터럴은 모두 `en.txt`에 정확히 같은 키가 있어야 한다. 이름에 "Smoke"가 들어간 파일과 `Localization.cs`는 제외한다. `$"…한글…"`은 거부되므로 `Loc.F`를 쓴다.
- `en.txt` 형식: 한 줄에 탭 하나, 중복 키 없음, 키에는 한글이 있고 값에는 없음, `{n}` 집합과 `|` 개수가 같음.
- `# --- composed at runtime`(`en.txt:3492`) 위의 키는 여전히 소스 리터럴이어야 한다. 문구를 고치거나 지울 때는 키를 지우거나 표식 아래로 옮긴다. 합성 문장이나 JSON 문구는 표식 아래에 둔다.
- 시안의 명시적 한·영 쌍 49개 중 38개는 아직 `en.txt`에 없다(예: 다음에 열릴 문장, 받은 쪽, 칙령을 저장했습니다, 겪은 문제, 고칠 한 가지, 달라진 점). 튜토리얼 브랜치가 같은 `en.txt` 영역을 고친다.

### 12.4 저장소 검사

- **`tools/check_ui_contract.py`**: `HuntEdictWindow*.cs`를 이은 것에 `ContentWindowHost.Attach`, `UiTheme.Scale`, `HuntEdictSummary`, `Session.Draft`가 있어야 한다(`ADAPTERS` :15). `Font.CreateDynamic` 금지(:55). 따옴표 6·8자리 16진 색 리터럴 금지, `UiTheme` 토큰 사용(:71). `class …Window`는 이름이 `HuntEdictWindow.*`인 파일에서만(:57). `typeof(Canvas)`는 소유자가 `ADAPTERS`에 있어 허용(:62). `Runtime*Smoke*.cs`는 제외(:46). **새 partial은 반드시 `HuntEdictWindow.<X>.cs`여야 하며**, 다른 이름의 파일에 `partial class HuntEdictWindow`를 두거나 새 `…Window` 클래스를 만들면 실패한다.
- **`tools/check_ui_refresh.py`**: 최상위 `Presentation/*.cs`만 훑고 `Runtime*`은 건너뛴다(:50). 모든 `StoreViewBinding.Attach(`는 인자 5개 이상이거나 `visibleState:`가 있어야 한다(:56). `HuntEdictWindow.cs:50`은 5개다. N6용 두 번째 `Attach`가 필요하면 키를 줘야 한다. 새 partial은 목록에만 오른다.
- **`tools/test_ui_contract.py`**: `test_current_source`가 `Presentation/`을 복사해 `check()==[]`를 요구하므로 같은 규칙이 적용된다. 새 파일마다 `.meta`를 추가한다.

### 12.5 새 테스트

1. **계약 스모크.** `P/RuntimeEdictContractSmoke.cs`, 플래그 `-hellscriptEdictContractSmoke -hellscriptUiNames <ui-names.json>`. `Runtime…Smoke` 이름이라 두 검사 도구에 걸리지 않는다.
   - **파싱.** 최상위 키를 정규식으로 읽는다. `JsonUtility`는 사전을 못 읽고 Newtonsoft는 설치되어 있지 않다.
   - **패턴 펼치기.** `<tab>`은 `HuntEdictUiCatalog.Tabs`를 `HuntEdictProgression.Tab`으로 거른 것. `<ids[0]>`은 `GroupDisclosed`를 통과한 그룹의 `HuntEdictSummary.Key`. `<option id>`는 `FocusGlobalOption` 뒤 각 그룹의 `Visible` 통과 id(도움말은 `HuntEdictUiCatalog.Help(id)!=""`인 것). `<scope>`는 선택기가 있는 그룹의 `HuntEdictQuickPresets.GlobalScope(g)`. `<preset id>`는 `For(scope)`와 `custom`(프롤로그에는 없음). `<slot>`은 0..4, `<n>`은 0..3(+ `edict-ultimate-slot`), `<skill id>`는 `ClassSkills.For(hero.heroClass)`. `<speed>`는 프롤로그에서 1뿐(`P/HuntEdictWindow.CombatPreview.cs:35`). `<key>`는 `Window.Search` 결과.
   - **따로 처리할 이름.** `Skills page`는 시안 래퍼라 Unity에 없다. `edict-caption`은 `Guide caption`과 같은 요소이며 최종 이름은 열린 결정(15장)이다. `menu-hunt-edict`는 도크 버튼이며 `game.UI`에서 찾는다(6.1장: 오브젝트 이름은 "사냥 칙령"). JSON에는 컴포넌트 종류가 없으므로 이름 → 종류 표를 따로 둔다.
   - **단계.** 프롤로그 스킬·생존(새 계정, `game.EnterPlaza(true)`, `Until(game.TutorialActive)`, `Progression.cs:19`처럼), 마을 3탭, 그리고 `hero.highestClear=N; HuntEdictProgression.Reconcile(a)`로 균열 3·4·6·9·12·15+룬·20. 각 단계를 5개 크기 × 한국어·영어로.
   - **보이는 이름마다.** 찾아지고 활성이며 `IsInteractable()`이 기대와 같고 `Face.Available`과 일치한다. `UiSafeArea.Current` 안에 있고, 첫 화면 컨트롤은 가운데로 스크롤하기 전에 `scroll.viewport` 안에 있다. 중앙 레이가 자기에게 닿고, 같은 컨테이너의 Button끼리 겹치지 않고, Text가 잘리지 않는다. 없어야 할 이름은 없다.
   - **불변식.** 칩은 ScrollRect 밖. `Guide caption`은 `Option area` 위에 있고 높이가 44/58 × `CanvasScaler.scaleFactor`이며 `Option area`가 정확히 그만큼 짧다. `edict-beat-strip`은 칩과 `Selected edict group` 사이. `edict-save-next`는 `Fixed save controls` 안에 있고 저장·되돌리기와 겹치지 않으며 다음 변경에 사라지고, 12초 만료에도 `edict-save`/`edict-revert` 인스턴스는 유지된다. 둘러보기는 `Session.Draft` JSON과 `Store.Revision`을 바꾸지 않는다.
   - **재사용할 도우미.** `P/RuntimeEdictOverviewSmoke.cs`: `Boot`·`Error` :19-28, `Find(name)` :29(QuickPreset·RecommendedEquipment 스모크에도 같은 것, `game.UI` 전체 탐색판은 `RuntimeTutorialSmoke:34`·`RuntimeRiftEntrySmoke:26`), `Bounds` :31, `Trail` :30, `Tap`(레이캐스트 증명) :33-47(먼저 가운데로 스크롤하므로 "뷰포트 안" 검사는 그 전에), `TabsHit`·`Settle` :51-57, `Resize(w,h,lang)` :75-80, `Capture` :81, 크기 배열 :125. 그 밖에 `Within(a,b)`(±1px) `RuntimeSharedUiSmoke:26`, `Click`의 뷰포트 검사 `RuntimeSharedUiSmoke:32`, 쌍별 겹침·글자 넘침 `RuntimeRiftEntrySmoke:49-57`, `UiSafeArea.Simulate` `RuntimeSkillTreeSmoke.Sections:146`, 픽스처 `RuntimeSkillTreeSmoke.ExistingTrees`(:22)·`ContentUnlocks.RecordRunEnd`(`Progression:115`)·`runeBoard.step=RuneBoardLesson.Complete`(`ButtonUx:122`). 창 상태 API는 `SelectTab`, `SelectGroup`, `FocusGlobalOption`, `OpenSkillPolicy`, `SelectSkillPage`, `Search`, `HasDialog`, `PreviewCombat`.
2. **Edit Mode `T/HuntEdictSentenceTests.cs`.** N1과 티저 로직이 `UnityEngine` 없이 Core에 있어야 한다(`EdictTeaser`, 9장). `Prototypes/HuntEdict/engine.test.cjs`의 네 사례(이전 안내는 수행으로 센다, 변경 + 저장이 안내를 수행으로 표시한다, 새 문장 표시, 티저가 일정을 따라 걷는다)를 옮긴다.
   - **미수행 안내.** `NewRules`는 "공개됨 ∧ `GuideVisible` ∧ `practiced` 아님"이며 이전 안내는 `Tutorials.Performed`로 설정한다. 표: 균열 1 종료 → F05, main에서 r3 → E03·r5 → E05, 튜토리얼 브랜치에서 r3 → E05·r5 → E03.
   - **미공개 규칙.** 브랜치의 균열 5에서 `HuntEdictQuickPresets.Apply(d,"global/survival.lowHp","careful")` 뒤 `TutorialProgress.EdictSaved`는 E05를 수행하고 E18은 수행하지 않는다.
   - **그룹·탭 표시.** 연 그룹은 자기 점만 지운다. 탭 표시는 그룹 표시의 합집합. 미공개 규칙은 표시하지 않는다. `AccountGuide` 필드를 더하지 않는다.
   - **표시 키.** `Performed`나 `edictUnlocks`가 바뀌면 키가 바뀌고, 관계없는 골드 저장에는 바뀌지 않는다.
   - **티저 걷기.** 첫 결과 전: First, 키 F05, 단계 1. 최고 클리어 1→2(E02), 2→3 … 5→6(`details`), 19→20. 룬 없이 20: Rune H11, 단계 15. 룬이 있거나 레거시면 없음. `Now==min(best,Stage)`, `Total==15`, `Sentence`는 줄지 않는다. `Next()` 문자열 다섯 개와 둘이 공존하는 동안 일치한다.
   - **쪽수.** main에서는 `PagesTotal==0`이라 쪽수를 숨긴다. 병합 뒤 `PagesReceived==TutorialChapters.Pages(a)`(11 중), `C00`을 수령하면 1.
3. **레슨 재생 확장 (`P/RuntimeTutorialSmoke*.cs`).**
   - **훅.** `LayoutProfiles`(:111)에 `Action each` 인자를 더한다. `Progression:37`, `:81`, `cs:72`의 반복에서 서로 다른 `gate.Target.name`마다 한 번씩 `yield return LayoutProfiles("gate-"+name, null, () => AssertInView(game.UI.Gate.Target))`를 실행한다. 마지막은 `Resize(440,956,"ko")`(:138).
   - **프로필마다 단언.** 대상 사각형이 `UiSafeArea.Current` 안에 있고, ScrollRect 아래라면 `Tap`이 가운데로 스크롤하기 전에 `Within(Bounds(scroll.viewport),…)`이다. 영역 대상(`Option area`, `Preset explanation`)은 하위 컨트롤(`edict-starter-next`, 일시정지·재시작, `edict-preset-activate`)이 화면 안에 있다. 중앙 레이가 "Input blocker"가 아니라 대상이나 그 자식에 닿는다. 캡션이 대상과 겹치지 않고, v3 캡션 문구(`P/GameUI.TutorialComparison.cs:8-46`) 전부가 한국어·영어에서 44/58에 들어가며 영어에는 한글이 없다.
   - **지정 목록.** `menu-hunt-edict`, `edict-skill-<첫 스킬>`, `edict-skill-rank-up`, `edict-skill-equip`, `edict-save`, `edict-active-slot-0`, `edict-slot-policy`, `edict-preset-tab-<B>`, `edict-preset-activate`, `edict-starter-next`, `starter-observation-area`, `starter-selection-area`, `edict-close`, `edict-option-survival.potionHpPercent`, `edict-number-input`, `edict-number-apply`.

### 12.6 영향받는 것만 돌리는 방법

`AGENTS.md`에는 스모크 실행 레시피가 없다(MCP `run_tests`만 안내한다). 이 컴퓨터에서 쓴 절차는 다음과 같다.

- **Edit Mode.** 프로젝트를 복제한다(`cp -Rc Assets Packages ProjectSettings Library <작업폴더>/proj; rm -f proj/Temp/UnityLockfile`). `Unity -batchmode -projectPath <proj> -runTests -testPlatform EditMode -testFilter "Hellscript.Tests.X;…"`로 실행한다. 필터는 bash 배열로 만든다. 필터가 깨지면 조용히 0개를 돌린다.
- **개발 빌드.** `-executeMethod Hellscript.Editor.ProjectBuilder.BuildMac -hellscriptBuildOutput <x>.app -quit`(약 2분).
- **스모크 실행.** `<x>.app/Contents/MacOS/HELLSCRIPT <플래그> -hellscriptSavePath <빈 폴더> -hellscriptEvidencePath <폴더> -screen-fullscreen 0 -screen-width W -screen-height H`를 bash 스크립트에서 `ARGS=(…)` 배열로 실행한다(zsh는 단어를 나누지 않는다). 로컬 러너(`Artifacts/Validation/FinalMain20260926/Runner/run_smokes.py`, 저장소에 없을 수 있다)는 이어 실행과 `HELLSCRIPT_*_OK` 표식을 처리하며 계획은 ButtonUx, QuickPreset, SaveLayout, SharedUi, SkillTree, Language, RiftEntry, Tutorial만 다룬다.
- **플래그.** `-hellscriptEdictOverviewSmoke`, `-hellscriptSharedUiSmoke`(각각 `…Resume` 변형), `-hellscriptEdictSaveLayoutSmoke`, `-hellscriptEdictSectionsSmoke`·`-hellscriptSkillPresetSmoke`·`-hellscriptSkillMenuSmoke`(이어 실행 `-hellscriptSkillTreeResume`), `-hellscriptQuickPresetSmoke`(+`…Resume`), `-hellscriptEdictControlsSmoke`, `-hellscriptRecommendedEquipmentSmoke`, `-hellscriptEdictProgressionSmoke -hellscriptEdictClass 0|1|2`(+`-hellscriptTutorialResume`, v2 흐름은 `-hellscriptTutorialSmoke`, 둘 다 `-hellscriptEvidencePath`), `-hellscriptButtonUxSmoke`, `-hellscriptUiStyleSmoke`, `-hellscriptRepeatSettingsSmoke`, `-hellscriptLanguageSmoke`.

## 13. 검증 계획과 캡처 목록

작업 중에는 바꾼 부분과 직접 관련된 검사만 실행하고, 전체 Edit Mode와 런타임 스모크 묶음은 마지막 코드 변경과 `main` 병합 뒤 한 번만 실행한다(전역 규칙).

| 항목 | 내용 |
| --- | --- |
| 정적 검사 | `python3 tools/check_ui_contract.py`, `python3 tools/test_ui_contract.py`, `python3 tools/check_ui_refresh.py` |
| Edit Mode | 12장의 갱신·신규 테스트, `LocalizationTests`, `StoreViewBindingTests` |
| 런타임 스모크 | 12장의 갱신·신규 스모크(개발 빌드, 한 번에 한 세트) |
| 갱신 시나리오 | 무변경 자동 저장, 실제 값 변경 저장, 누르는 중 저장, 자식 창 닫기, 탭 반복·재진입·빠른 클릭, 한국어·영어, 저장 실패를 실제 런타임에서 확인 |
| 크기 | 440×956, 956×440, PC 16:9·16:10·21:9 × 한국어·영어(기본 글자 크기만) |
| 문서·위키 | 한국어·영어 문서와 `Wiki/history`, `tools/wiki.py build·check`, 위키 게시는 병합된 `main`에서만 |
| 산출물 | 소유 작업·경로·크기·보존기한을 `artifact-lifecycle.json`에 기록하고 승인된 정리를 완료 조건에 포함 |

**캡처 목록(브리프 7장 6).** 아래 네 화면을 세로 440×956·가로 956×440, 한국어·영어로 남긴다(합 16장). PC 3종은 개요와 안내 상태 한 장씩.

| 화면 | 시안 기준 이미지(`Prototypes/HuntEdict/evidence`) |
| --- | --- |
| 요약(개요) | `stage-r3-overview-N1-N3-*`, `stage-r20-overview-*` |
| 그룹 상세 + 간편 프리셋 선택 | `focus-r5-before-*`, `focus-r5-dialog-*`, `stage-r9-bag-custom-*` |
| 숫자 입력 다이얼로그 | `survival-02-input-*` |
| 스킬 탭 + 사용 방식 화면 | `skills-r12-*`, `lesson-07-observe-A-*`, `lesson-11-select-*` |

미실행 검사는 보고에 "실행하지 않음"으로 적는다. 실기기·사람 이해도는 별도로 기록한다.

## 14. 시안 검증 결과와 한계

- `node Prototypes/HuntEdict/engine.test.cjs` 32개 통과: 게임의 `HuntEdictProgressionTests` 공개 단계 표, 탭·그룹·`Next`, 프리셋 적용·일치 왕복, 새 영웅 균형, 마커·완료, 스킬, 프롤로그 레슨.
- `node Prototypes/HuntEdict/browser.test.cjs` 655개 통과(헤드리스 크롬, 실제 마우스·키보드): 프롤로그 레슨(크기 5종 × 언어 2종, 자동 스크롤 끔), 단계별 탭·그룹, N1~N6와 겹침, 다이얼로그·검색·성향·잠금·스킬, 가로 넘침 없음, 영어 누락 없음.
- 미검증: 실기기·터치, 다른 브라우저, 스크린리더, 신규 사용자의 이해도, 게임 코드와의 동작 일치(HTML이므로 Unity에서 다시 검증).
- 시안용 예시: 전투 미리보기, 지난 사냥 문제·3박자 문구, 프리셋 슬롯 이름, 영웅 레벨 곡선, 받은 쪽 수. 단순화: 자동 착용 탭, 스킬별 직접 설정 세부 편집, 코드 공유 대화상자, 닫기 확인 다이얼로그, 순서 다이얼로그의 ↑↓, 전사만, 레거시 계정·구 편집기 제외(5장 "시안과 다르게 이식하는 것").

## 15. 열린 결정

1. **검색 줄 표시 조건.** 기본은 프롤로그에서만 숨긴다. 공개 옵션이 적은 초반에도 숨길지(시안 방식)는 스모크 영향을 본 뒤 정한다.
2. **캡션 줄의 이름과 소유.** 창이 `Guide caption`(RectTransform, `Option area`의 형제)을 노출하고 튜토리얼이 그 안에 그린다는 안을 권한다. 시안의 `edict-caption`과 같은 요소다. 이름 확정은 튜토리얼 작업자와 맞춘다.
3. **받은 쪽 수.** 챕터 엔진(`TutorialChapters`)이 `main`에 들어온 뒤 티저에 연결한다. 그 전에는 쪽수를 숨긴다.
4. **자동 착용·스킬별 직접 설정 화면.** 시안에서 다루지 않았다. 같은 토큰으로 후속 개편할지 범위를 정한다.
5. **레거시 계정의 트리 화면.** 이번 범위 밖이다. 표면 토큰만 맞출지 정한다.
6. **N6 행동 연결.** "훈련장에서 확인"을 실제 훈련장 열기로 연결할지, 안내 토스트로 둘지 정한다.

## 16. 변경 기록

- 2026-10-05: 시안 v2 기준으로 작성. 이름 매핑표, 구조 변경, 창 상태·동작, 지정 영역, N1~N6 데이터 모양, 튜토리얼 측 요청, 이식 지도, 영향받는 스모크·테스트, 검증·캡처 계획을 정리했다.
