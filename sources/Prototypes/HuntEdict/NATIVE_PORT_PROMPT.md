# 사냥 칙령 네이티브 이식 프롬프트

새 세션(Codex 또는 Claude)에 그대로 붙여 넣는 작업 지시서다. 이 문서만 읽고 시작할 수 있게 썼지만, 세부 표는 `Docs/Design/Hunt_Edict_UI_Overhaul.md`(한국어)와 `.en.md`가 기준이다. 코드 주석·커밋 메시지는 영어, 대화·개발 기록 문서는 한국어, 게임 화면 문구는 한국어와 영어를 함께 쓴다(`CLAUDE.md`).

## 0. 시작 전에 확인한다 (하나라도 아니면 멈추고 보고한다)

1. **튜토리얼 브랜치가 `main`에 병합되었는가.** `Assets/HELLSCRIPT/Resources/HuntEdictProgression.json`에 `E18B`와 `E18C` 규칙이 있고 낮은 HP(`survival.lowHp`)가 균열 3, 위치·거리(`position.engage`)가 균열 5인지 확인한다. 아니면 이식하지 않는다. 공개표가 달라 N1·티저·튜토리얼 대응이 어긋난다.
2. **작업 폴더가 최신 `origin/main`의 별도 워크트리인가.** 공유 체크아웃(다른 세션이 커밋하는 곳)에서 작업하지 않는다. 다른 사람의 미커밋 변경을 되돌리거나 커밋하지 않는다.
3. `AGENTS.md`의 필수 규칙을 읽었는가: 신규 콘텐츠와 공통 UI, 저장 후 UI 갱신과 버튼 깜빡임, CPU 비용·성능 리뷰, 커밋·푸시와 공개 위키 동기화, 빌드·백업·임시 산출물 정리, CoplayDev Unity MCP 활용 기준.
4. 대형 빌드·검증 사본을 만들기 전에 데이터 볼륨 여유 공간을 확인하고, 산출물은 작업 전용 경로 하나에 모아 `artifact-lifecycle.json`에 소유·경로·크기·보존기한을 적는다.

## 1. 목표와 범위

승인된 HTML 시안 v2(`Prototypes/HuntEdict/HELLSCRIPT-HuntEdict.html`)를 게임의 사냥 칙령 창(`HuntEdictWindow*.cs`)에 옮긴다. **새 기능이 아니라 표면과 정보 설계의 개편**이며, 도메인(공개 판정, 저장, 프리셋 적용, 전투)은 기존 것을 쓴다.

포함: 비레거시 계정의 창 틀·탭·그룹 화면·옵션 행·다이얼로그·개요·스킬 탭·정책 화면, 새 슬롯 N1~N6, 계약 스모크.
제외: 레거시 계정의 트리 화면, 구 편집기(`ShowLegacyEdictEditor`)와 그것을 쓰는 스모크 13개, 자동 착용 전용 화면(`DrawRecommendedEquipment`)과 스킬별 직접 설정 세부 편집의 표면 개편, 튜토리얼 코드(Phase 4는 별도).

## 2. 먼저 읽는다

- `Docs/Design/Hunt_Edict_UI_Overhaul.md` 전체(특히 6장 이름 매핑표, 8장 지정 영역, 9장 N1~N6, 11·12장 이식 지도와 스모크 목록).
- `Prototypes/HuntEdict/README.md`, 시안 소스 `ui.window.js`, `ui.skills.js`, `ui.dialogs.js`, `ui.tutorial.js`, `styles.css`(치수와 색은 여기가 정답이다), `engine.js`(공개 규칙 이식), `evidence/ui-names.json`(이름 계약), `evidence/*.png`(기준 이미지).
- 게임 쪽: `Assets/HELLSCRIPT/Runtime/Presentation/HuntEdictWindow*.cs`, `HuntEdictSurface.cs`, `UiTheme.cs`, `ContentWindowHost.cs`, `StoreViewBinding.cs`, `Core/HuntEdictProgression.cs`, `HuntEdictUiCatalog.cs`, `Docs/Implementation/Shared_UI_Contract.md`, `UI_Refresh_Stability_Rules.md`, `Hunt_Edict_Progression.md`.
- 튜토리얼 쪽(읽기만): `GameUI.TutorialStaging.cs`, `GameUI.TutorialComparison.cs`, `GameUI.Tutorials.cs`, `GameUI.EdictNotices.cs`, `PrologueGate.cs`, `TutorialAnchorRing.cs`.

## 3. 바꾸지 않는 것 (계약)

- 탭 id 10개와 `edict-tab-<id>` 이름, `Main tabs` 안에 탭 버튼 전부(공개된 경우), `Option area`·`Preset explanation`·`Fixed save controls`·`Overview custom settings` 등 영역 이름.
- 컨트롤 이름 전부(설계 문서 6장). 새 이름은 `edict-teaser`, `edict-beat-strip`, `Guide caption`(시안의 `edict-caption`, `Option area`의 형제 RectTransform), `edict-save-next` 넷뿐이다.
- `HuntEdictUi.json`의 그룹·id 순서(`ids[0]`이 그룹 이름·퀵프리셋 범위·스크롤 키를 정한다). 공개 규칙·권한·`HuntEdictProgression` 판정.
- 저장 경로는 `CommitHuntEdict` 하나. 표시 부품이 저장 데이터를 직접 바꾸지 않는다. 튜토리얼 완료 판정은 저장된 값의 변화다.
- `HasDialog`는 모달이면 참(스킬 행동 말풍선 포함). 뒤로 가기 순서와 프롤로그의 `BackLocked`.
- 개요: `edict-style-*` 버튼은 정확히 3개, ScrollRect 1개, `SkillIconView` 없음. 그룹 칩은 스크롤 밖이고 그룹 아이콘이 있다.
- 새 저장 필드를 만들지 않는다(N1은 계산).

## 4. 마일스톤 (한 단계씩 진행하고 결과를 알린다)

각 단계가 끝나면 **바꾼 부분과 직접 관련된 검사만** 실행하고, 기록을 갱신한 뒤 커밋·푸시한다. 전체 Edit Mode 검사와 런타임 스모크 묶음은 마지막 코드 변경과 `main` 병합 뒤 **한 번만** 실행한다(마지막 전체 검사 뒤 작은 수정은 관련 검사만 다시 하고 다시 돌리지 않은 검사를 보고에 적는다). 배치모드가 건드리는 `ProjectSettings/ProjectSettings.asset`과 `UnityConnectSettings.asset`은 커밋 전에 되돌린다.

| 단계 | 내용 | 직접 관련 검사 |
| --- | --- | --- |
| **M0** | 기준 확인(0장), 계약 이름 목록을 코드(테스트 도우미)로 고정, 새 문구 목록(8장) 확인 | EditMode: 공개 단계 테스트(브랜치 표) |
| **M1** | 창 틀·탭·푸터 표면(아이콘+이름+부제+점 자리), L0 좁은 탭 열(`edict-tab-skills` 오브젝트 유지), 캡션 줄 예약 자리(비어 있으면 높이 0) | `RuntimeSharedUiSmoke`(EdictFrame), `RuntimeEdictOverviewSmoke`(10개 탭 프레임·크기), `RuntimeHuntEdictSaveLayoutSmoke`, `check_ui_contract.py` |
| **M2** | 그룹 칩 현재 답·변경 점, 옵션 행 한 줄·이유 줄, 간편 프리셋·숫자·선택·복수·순서 다이얼로그, 검색 줄 표시 조건(기본은 프롤로그에서만 숨김) | `RuntimeSkillTreeSmoke.Sections`(CheckSectionFrame), `RuntimeEdictQuickPresetSmoke`, 숫자 입력 흐름 |
| **M3** | 개요: 성향 카드 수치 칩, 티저(N3)와 구조화된 `Next` | `RuntimeEdictOverviewSmoke`(CheckSimpleOverview 갱신), `HuntEdictOverviewTests` |
| **M4** | N1·N2(공개된 규칙만 대상인 계산, 세션 열림 기록), `EnsureVisible`, `FocusGlobalOption` 스크롤, `DisclosureDisplayKey` 확장 | 새 EditMode 테스트(N1 계산), `StoreViewBindingTests`, `check_ui_refresh.py`, 실제 런타임 갱신 시나리오 |
| **M5** | N4 띠·N5 캡션 줄·N6 저장 직후 행동의 데이터 모델과 창 API(내용은 비어 있어도 동작), 빈 상태 높이 0 | 새 EditMode 테스트, 크기별 겹침 검사 |
| **M6** | 스킬 탭(카드·인스펙터·슬롯 줄·말풍선)과 정책 화면(L0 배치, 진행 버튼 위치, 관찰 진행 막대) | `RuntimeSkillTreeSmoke.Presets`, `RuntimeTutorialSmoke`(레슨 레이아웃 검사), 프롤로그 레슨 재생 |
| **M7** | 계약 스모크(`ui-names.json`의 이름이 공개 단계·크기·언어마다 존재·상호작용 가능·뷰포트 안), 영어 문구 완결, 한국어·영어 문서와 위키 기록, 5개 크기 × 2개 언어 캡처, 산출물 정리 | 전체 EditMode + 런타임 스모크 묶음(한 번), `tools/wiki.py build·check`, `test_wiki.py`, `test_wiki_ui.cjs` |

세부 파일·함수 위치와 함정은 설계 문서 11장(이식 지도), 영향받는 스모크는 12장을 따른다.

## 5. 치수 (시안 → Unity, 논리 단위)

게임 창은 논리 단위로 배치된다. 시안은 게임의 `UiTheme.Scale`과 같은 공식(`min(폭/(가로 800 · 세로 405), 높이/(가로 450 · 세로 720))`)으로 논리 크기를 잡는다: 956×440 → 978×450, 440×956 → 405×880, PC 16:9 → 800×450, 16:10 → 800×500, 21:9 → 1050×450. 다만 기존 높이 계산은 `lastScale`/`Grow`를 곱하는데 시안에는 없다. 시안 px를 상수로 박지 말고 기존 공식에 맞춘다.

| 항목 | 값 | 비고 |
| --- | --- | --- |
| 머리줄 / 푸터 | 34 / 48 | 현재 값 유지(시안은 36을 썼으나 게임의 34를 따른다) |
| `Main tabs` 가로 / L0 | 172 / 약 48 | 세로는 `min(5,n)`열, 높이 `12 + 행 × max(38, 32·확대)` 유지 |
| 탭 부제 | 탭 6개 이하인 가로에서만 | 10개 탭이 한 열에 들어가는 높이를 해치지 않는다 |
| 그룹 칩 | 최소 44(현재 42) | 열 수 공식 유지 |
| 옵션 행 | 최소 36(현재 62) | 라벨(+?)과 값 알약 한 줄 |
| 캡션 줄 | 44 / 58 | 안내가 시작·종료될 때만 예약이 바뀐다 |
| N4 띠 | 한 줄 28, 펼침 51(세로 117) | 스크롤 밖 고정 |
| N3 티저 | 76 | 개요 스크롤 안 |
| 다이얼로그 | 너비 `min(450, 창−20)` | 숫자 다이얼로그는 범위 줄·칩·오류 줄 때문에 높이를 늘린다 |

색·글꼴은 `UiTheme`/`UiFonts`와 기존 `HuntEdictSurface` 스킨(`tab-current`, `tab-idle`, `button-idle`, `button-primary`, `input-well`, `chip-on` 등)을 쓴다. 따옴표로 된 `"rrggbb"` 색 리터럴을 새로 만들지 않는다(`check_ui_contract.py`). 그룹 아이콘은 `GroupGlyph`, 탭 아이콘은 `Art/HuntEdict/Glyphs/nav-*` 그대로다. **새 2D 이미지는 만들지 않는다.** 필요해지면 직접 그리지 말고 Codex에 요청한다.

## 6. 결정해 둔 것 (다시 묻지 않는다)

탭 10개 유지, 개요 최소 + 티저, 간편 프리셋은 다이얼로그 유지, 안내는 고정 캡션 줄(떠 있는 말풍선 금지), N4는 기본 한 줄, N1은 계산(저장 필드 없음), 공개표는 튜토리얼 브랜치 기준, 검색 줄은 프롤로그에서만 숨김(기본).

## 7. 사용자에게 물을 것

설계 문서 15장의 열린 결정을 이식 중 해당 단계에서 묻는다: 검색 줄 표시 조건 확장, 캡션 줄 이름(`Guide caption` 권장), 받은 쪽 수 연결 시점, 자동 착용·스킬별 직접 설정 화면의 후속 범위, 레거시 계정 표면 토큰, N6 행동 연결.

## 8. 새 문구 (한국어 원문이 `en.txt`의 키)

먼저 `Resources/Localization/en.txt`에서 같은 한국어 키를 찾아 재사용하고, 없는 것만 추가한다. 시안이 제안한 영어는 아래와 같다(설계 문서 9장의 튜토리얼 문구와 시연 전용 문구 제외). 보간 문자열은 `Loc.F`로 쓰고, 한국어 리터럴을 보간 문자열로 만들지 않는다. `LocalizationTests`가 미등록 한국어 리터럴·고아 항목을 잡는다.

| 한국어 | 영어(제안) |
| --- | --- |
| 후퇴 HP {0}% | Retreat at {0}% HP |
| 처음이라면 | New players |
| 어느 성향과도 다릅니다 | Matches no style |
| 다음에 열릴 문장 | Next sentence |
| 받은 쪽 | pages |
| 첫 일반 균열 종료 후 | After the first rift ends |
| 균열 {0}단계 클리어 시 | At rift {0} clear |
| 룬 필수 안내 완료 후 | After the rune guide |
| 지금 {0}/{1} | Now {0}/{1} |
| 직접 설정과 훈련 비교 | Custom values and training comparison |
| 현재 공개된 설정이 없습니다. | Nothing is open here yet. |
| {0}/10초 관찰 | Observed {0}/10 s |
| 관찰 완료 | Observed |
| 1배속으로 10초 관찰 | Watch 10 s at 1× |
| 1배속에서만 관찰 시간이 쌓입니다 | Only 1× counts toward observation |
| 장착 해제 | Unequip |
| 사냥 칙령 편집 | Edit Hunt Edict |
| 접기 | Collapse |
| 겪은 문제 / 고칠 한 가지 / 달라진 점 | Problem / One fix / What changed |
| 저장하면 여기에 표시됩니다. | Shown here after you save. |
| 이 항목을 눌러 값을 바꾸세요. | Tap this row to change its value. |
| 간편 프리셋을 눌러 하나 고르세요. | Tap Quick Preset and pick one. |
| 저장하면 다음 사냥부터 적용됩니다. | Saving applies it from the next hunt. |
| 기본 세팅 적용 중에는 설정을 바꿀 수 없습니다. 먼저 기본 세팅 적용을 끄세요. | Settings are locked while Default settings are applied. Turn them off first. |
| 저장했습니다 | Saved |
| 훈련장에서 확인 / 다음 사냥에서 관찰 | Check in training / Watch next hunt |
| 안내 | Guide |

탭 부제 10개는 설계 문서 5장에 있다.

## 9. 검증과 보고

- 크기 5종(440×956, 956×440, PC 16:9·16:10·21:9) × 한국어·영어를 기본 글자 크기로 확인한다. 글자 크기별 검사나 확대 글자 스모크는 만들지 않는다.
- 설계 문서 13장의 캡처 16장(+PC 3종)을 `Docs/Implementation/HuntEdictOverhaulEvidence/`처럼 한 경로에 모으고 경로·해시를 기록한다.
- 갱신 시나리오(무변경 자동 저장, 값 변경 저장, 누르는 중 저장, 자식 창 닫기, 탭 반복·재진입, 빠른 클릭, 저장 실패)는 실제 런타임에서 확인한다. 정적 검사와 가드 존재만으로 통과했다고 하지 않는다.
- 성능을 주장하지 않는다. 필요하면 `Docs/Implementation/Performance_Review_Rules.md`의 실측 기준(동일 조건 이전/이후 binary 각 3회)을 따른다. 프레임마다 UI를 다시 만들거나 마커·띠·캡션을 타이머로 갱신하지 않는다.
- 확인하지 않은 것을 확인했다고 적지 않는다. 실기기·사람 이해도·다시 돌리지 않은 검사를 보고에 적는다.

## 10. 완료 조건

1. 설계 문서 6장 이름 매핑표의 모든 "유지" 항목이 계약 스모크로 확인되고, 달라진 항목은 표에 기록되었다.
2. 영향받는 스모크·테스트(12장)를 고쳤거나 고치지 않은 이유를 적었다. 구 편집기를 쓰는 스모크는 건드리지 않았다.
3. 한국어·영어 문구가 모두 있고 `LocalizationTests`가 통과한다.
4. 한국어·영어 개발 기록(`Docs/Implementation/Hunt_Edict_UI_Overhaul_Native.md`·`.en.md` — 위키 문서 id는 파일 이름에서 정해지므로 설계 문서와 같은 이름을 쓰지 않는다)과 증거를 남겼다. 설계 문서의 이식 지도·스모크 목록을 실제와 맞췄다.
5. `tools/wiki.py build·check`와 위키 검사를 통과하고 `Wiki/history`를 커밋에 포함했다. **공개 위키 게시는 병합된 `main`에서만** 한다.
6. 승인된 산출물 정리를 끝냈고(영구 삭제는 구체 승인 필요), 남은 보존 산출물과 승인 대기를 기록했다.

## 11. 하지 않는다

- 탭을 합치거나 id·이름을 바꾸지 않는다. `HuntEdictUi.json` id 순서를 바꾸지 않는다.
- 떠 있는 말풍선, 잠금 자리 표시, 새 저장 필드, 새 창 시스템을 만들지 않는다.
- 시안의 변경 개수 칩, 순서 다이얼로그의 ↑↓, 닫기 확인의 "취소/확인"을 옮기지 않는다(기존 `Session.Dirty`, 드래그 카드, `RequestLeave`를 쓴다).
- 레거시 편집기·레거시 스모크를 고치지 않는다. 검사를 통과시키려고 소유자 목록을 임의로 늘리지 않는다.
- 다른 세션의 변경을 되돌리거나 커밋하지 않는다. 튜토리얼 코드(Phase 4)는 이 작업에서 바꾸지 않는다(요청 목록은 설계 문서 10장).
