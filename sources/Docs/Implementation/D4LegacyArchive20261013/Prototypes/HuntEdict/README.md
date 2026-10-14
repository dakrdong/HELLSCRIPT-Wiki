# HELLSCRIPT 사냥 칙령 개편 HTML 시안 v2 (튜토리얼 호환형)

갱신일: 2026-10-05 · 상태: 시안 v2 (Unity 코드 변경 없음, 커밋 전) · [English](#english)

[시안 열기](HELLSCRIPT-HuntEdict.html) · [검증 기록](evidence/browser-validation.json) · [이름 계약 목록](evidence/ui-names.json) · [원본 파일 해시](source-manifest.json) · [폐기한 v1](v1-superseded/README.md)

사냥 칙령 창을 이해하기 쉽고 깔끔하게 다시 짜되, **튜토리얼이 컨트롤 이름과 창 상태를 읽어 동작하는 방식**을 그대로 지키도록 만든 조작 가능한 HTML 시안이다. 입력은 튜토리얼 브랜치(`claude/tutorial-chronicle`)의 `Docs/Design/Tutorial_Hunt_Edict_UI_Brief.md`와 코드 조사 3건이며, 방향은 계획서(탭 10개 유지·개요 최소화)를 따른다.

## 한눈에

- **구조는 그대로, 표면과 정보 설계를 바꿨다.** 탭 10개 id와 `edict-*` 이름, 영역 이름 6개, `ids[0]` 규칙을 모두 유지한다. 1차 시안의 5탭 통합은 철회했다.
- **프롤로그를 실제 v3 흐름 그대로 재생한다.** 탭 1개 → 첫 스킬 선택·＋·장착·저장 → 슬롯 말풍선 → 사용 방식 A/B 관찰·활성화·확인 → 생존 탭 → 숫자 입력(60) → 저장 → 닫기. 지정 컨트롤이 모든 단계에서 **스크롤 없이** 화면 안에 있고 게이트에 가려지지 않음을 크기 5종 × 언어 2종에서 실제 마우스 입력으로 확인했다.
- **튜토리얼이 요청한 새 슬롯 N1~N6**을 넣고 높이를 측정했다.
- 데이터는 손으로 옮기지 않았다. 공개 단계·옵션 152개·그룹 35개·간편 프리셋·영어 문구를 빌드가 게임 파일에서 읽는다.

## 기준 데이터

`node Prototypes/HuntEdict/build.cjs`가 아래 파일을 읽는다. 기준 체크아웃은 `EDICT_GAME_ROOT`, 없으면 튜토리얼 브랜치(`claude/tutorial-chronicle` 0ee6acf6)다. 해당 브랜치의 공개 단계가 main과 다르기 때문이다(낮은 HP 균열 3, 위치·거리 균열 5, 세밀한 조건 E18을 20/13/7로 분리).

| 읽는 것 | 출처 |
| --- | --- |
| 공개 단계 17개 규칙(152개 옵션) | `Resources/HuntEdictProgression.json` |
| 그룹 35개·설명·조건·도움말 | `Resources/HuntEdictUi.json` |
| 옵션 정의(종류·선택지·범위·초깃값) | `Runtime/Core/EdictOptions.cs`, `PotionEdict.cs` (C#을 변환해 평가) |
| 선택지 이름(한국어) | `Runtime/Core/HuntEdictV2.Editing.cs` (`EdictOptionLabels`) |
| 간편 프리셋 211개·전투 성향 3개 | `Resources/HuntEdictQuickPresets.json` |
| 그룹 아이콘 36개 | `Runtime/Presentation/HuntEdictWindow.Glyphs.cs` (선 그리기 명령을 SVG로) |
| 색 | `Runtime/Presentation/UiTheme.cs` |
| 영어 | `Resources/Localization/en.txt` (한국어 원문이 키) |
| 전사 스킬 37개·아이콘 | `Prototypes/SkillTree/catalog.json`, `Resources/Art/ClassSkillIcons/Warrior` |

검증 로직(`engine.js`)은 `HuntEdictProgression.cs`의 `Has/Visible/Direct/Quick/Tab/Next`와 `HuntEdictQuickPresets`의 `Apply/Match/ApplyStyle/MatchStyle`을 옮긴 것이며, 게임의 `HuntEdictProgressionTests` TestCase 표(균열 3·4·5·7·8·9·12·14·18·20)와 탭 노출을 `engine.test.cjs`가 대조한다.

## v1에서 바뀐 점

| v1 | v2 |
| --- | --- |
| 탭 5개 통합 | **탭 10개 id·이름 유지.** 스모크 9곳과 튜토리얼 코드가 탭 10개 동시 존재를 가정한다 |
| 신규 계정에도 트리 | **스킬 카드 목록 + 슬롯 줄**(레벨 2 전 1장). 트리는 레거시 계정 화면이다 |
| 개요에 조언·장착 스킬 | 2026-10-01 결정대로 **전투 방식 3 + 기본 세팅 스위치(균열 20) + 다음 문장 티저(N3)** |
| 공개표 손 입력(main) | 튜토리얼 브랜치 JSON을 빌드가 읽음 |
| 프롤로그 4단계·슬라이더 | 실제 v3(탭 1개, 슬롯 말풍선, A/B 정책 화면, **숫자 입력 다이얼로그**) |
| 잠금·16:10·21:9 없음 | 기본 세팅 적용 잠금, 크기 5종 |

## 화면 구성

창은 게임처럼 **논리 단위**로 배치한다. 배율은 게임의 `UiTheme.Scale`과 같은 공식(`min(폭/(가로 800 · 세로 405), 높이/(가로 450 · 세로 720))`)으로 정한다: 956×440 → 978×450, 440×956 → 405×880, PC 16:9 → 800×450, 16:10 → 800×500, 21:9 → 1050×450. 기존 코드가 높이에 곱하는 `lastScale`/`Grow`는 시안에 없다.

- **머리줄 / `Main tabs` / `Option area` / `Fixed save controls`**: 게임과 같은 네 덩어리. 정책 화면은 푸터가 없다(활성화가 즉시 저장).
- **탭**: 아이콘 + 이름 + 부제 + 새 문장 점(N1). 가로는 1열, 세로는 `min(5,n)`열. 탭이 6개 이하면 부제를 보이고, 프롤로그(L0)는 아이콘 하나짜리 좁은 줄로 줄이되 `edict-tab-skills` 오브젝트는 유지한다.
- **그룹 화면**(전투·생존·전리품·가방·탐색·반복·자동 착용): 검색·변경된 항목 → 그룹 칩 → (N4 띠) → 선택 그룹 본문. 칩은 아이콘 + 제목 + **현재 답**(간편 프리셋 이름) + 변경 점·새 문장 점을 보인다. 본문은 설명 → 간편 프리셋 선택기(눌러 다이얼로그) → **직접 설정을 고르면** 옵션 행(라벨 + ? + 값 알약). 미공개 값·조건 불충족 행은 이유 한 줄과 함께 비활성이다.
- **다이얼로그**: 간편 프리셋(고르면 바로 적용), 선택·복수 선택·순서, 숫자(−/＋, 범위, 빠른 값 칩 — 칩은 입력창만 채우고 적용은 따로), 도움말, 닫기 확인. 모두 `HasDialog`가 참이다.
- **스킬 탭**: 남은 포인트 + 카드 목록(탭하면 행동 말풍선 = 모달) + 인스펙터(−·＋·장착) + 장착 스킬 줄(슬롯 `edict-active-slot-N`, 레벨 40에 궁극기). 슬롯이 `edict-slot-policy`로 **정책 화면**을 연다: 프리셋 탭 + 라디오 + `Preset explanation`(설명·모의 전투 미리보기·재시작/일시정지/배속·관찰 진행·`edict-starter-next`).
- **프리셋·공유**(균열 6): 슬롯 5개, 공유 코드 버튼은 균열 20.
- **기본 세팅 적용**(균열 20): ON이면 요약·스킬(관리)·반복만 열려 있고 나머지는 흐려진다.

## 튜토리얼이 동작하게 만드는 장치

1. **이름 계약.** 시안의 모든 조작 요소에 게임 이름이 `data-ctl`로 달려 있다. 테스트가 방문한 이름 63개 패턴을 [`ui-names.json`](evidence/ui-names.json)에 기록한다(네이티브 계약 스모크의 입력). 새 이름은 `edict-teaser`(N3), `edict-beat-strip`(N4), `edict-caption`(N5), `edict-save-next`(N6) 넷뿐이다.
2. **프롤로그 재생.** 20단계 표의 정정본(탭 단계 2·16은 도달하지 않고, 방식 A는 미리 선택되어 있으며, 10·12·13단계는 **영역** 지정이고 `edict-starter-next`는 1단계에서만 이름 지정)을 `engine.js`가 옮겼고, 테스트가 단계마다 ① 게이트 대상 이름 ② 대상이 존재·상호작용 가능·화면 안 ③ 중앙을 눌렀을 때 게이트 가림막이 아니라 대상이 눌림을 확인한 뒤 **실제 마우스 입력**으로 진행한다. 관찰은 `__edict.skipObservation()`으로 10초 대기를 건너뛴다.
3. **스크롤 없이 닿는 배치.** 프롤로그 전 단계의 지정 컨트롤은 자동 스크롤을 끈 상태(`autoscroll=0`)에서도 5개 크기·2개 언어 모두 화면 안에 있다. 정책 화면은 세로에서 진행 버튼을 설명 칸 맨 위로 올렸다.
4. **EnsureVisible(제안).** 규칙 안내의 포커스 옵션이 스크롤 밖이면 가장 가까운 스크롤 영역을 움직인다. 끄면(`autoscroll=0`) 같은 상황이 `OUT OF VIEW`로 드러난다(도구줄 표시). 현재 게임의 게이트는 구멍을 화면에 맞춰 자르지 않으므로 이 안전장치가 없으면 소프트락 위험이 남는다(코드 읽기 기준).
5. **링→저장 넘김(제안).** 현재 안내 링은 이름 목록에서 처음 발견된 컨트롤에 붙어 옵션 행·선택기가 보이는 동안 `edict-save`에 닿지 않는다. 시안은 값이 바뀌면(저장 안 한 변경) 링을 저장으로 넘긴다.
6. **기본 세팅 적용 중 안내.** 게임의 `FocusGlobalOption`은 이때 조용히 아무것도 하지 않아 링이 뜨지 않는다. 시안은 대신 `edict-default-settings`를 가리키는 안내를 띄운다.

## 새 슬롯 N1~N6 — 구현 형태, 측정 높이, 튜토리얼이 넘길 데이터

높이는 논리 단위이며 `r5` 안내 상태에서 측정했다(가로 영역 364 / 세로 영역 약 746).

| 슬롯 | 구현 | 높이(가로 / 세로) | 튜토리얼이 넘길 것 |
| --- | --- | --- | --- |
| **N1** 새 문장 표시 | 탭·그룹 칩 모서리 점. `공개됨 ∧ 안내 미수행 ∧ 이번에 열지 않음`으로 계산 | 0 (겹침) | 없음. **저장 필드를 추가하지 않는다.** 마커는 공개된 규칙만 대상으로 한다(아래 7-② 참고) |
| **N2** 기본 펼침 | 탭 진입 시 새 그룹 > 기억된 그룹 > 첫 그룹이 열림 | 0 | 없음 |
| **N3** 다음 문장 티저 | 개요 카드: 문장 번호 n/15, 제목, 조건, 진행 막대(지금/필요), 받은 쪽 n/11 | 76 / 76 | `{sentence, total, nextTitle, condition, now, need, pagesReceived, pagesTotal}` — 쪽수는 챕터 엔진 연결 전이라 시안 값 |
| **N4** 3박자 띠 | 그룹 칩과 스크롤 사이, **스크롤 밖 고정**. 기본은 **한 줄**(현재 박자 + 점 3개), 탭하면 ①②③ | 한 줄 28 · 펼침 51 / 한 줄 28 · 펼침 117 | `{guide, focus, problem, fix, resultBefore, resultAfter}` — 문구는 시안 예시 |
| **N5** 안내 | **떠 있는 말풍선 대신 `Option area` 맨 위의 고정 캡션 줄**(아래 7-① 참고). 짧은 명령형 한 줄 | 44 / 58 | `{text, target}` — 프롤로그는 게임의 v3 문구 그대로 |
| **N6** 저장 직후 | 푸터 상태 줄에 "✓ 저장했습니다 + 버튼 1개"(12초 또는 다음 변경까지) | 푸터 안 | `{ko, en}` 라벨 — 훈련장이 열려 있으면 "훈련장에서 확인", 아니면 "다음 사냥에서 관찰" |

N1~N5가 서로, 그리고 탭·검색·그룹 칩·스크롤 영역·저장·되돌리기를 가리지 않음을 크기 5종 × 언어 2종에서 사각형 비교로 확인했다(띠 펼침 포함).

## 시안을 만들며 확인한 사실

모두 코드·캡처를 읽은 결과이며 게임 실행으로 확인한 것은 아니다.

1. **지금의 A/B 비교 화면은 안내 말풍선이 프리셋 탭을 덮는다**(`Docs/Implementation/EdictProgressionEvidence20261004/starter-comparison-*.png`). 또 L0에서 172폭 탭 열이 아이콘 하나만 담은 채 비어 있다. v2는 캡션 전용 줄을 예약하고 L0 탭 열을 좁혔다.
2. **간편 프리셋은 아직 공개되지 않은 규칙의 값도 바꾼다**(예: 낮은 HP 프리셋이 E18의 치명적 피해·부착 피해 값을 쓴다). 게임의 `TutorialProgress.EdictSaved`는 `GuideVisible`을 확인해 안전하지만, 시안 초안은 이를 빠뜨려 E18 안내가 일찍 "수행"으로 기록되는 오류를 냈다. N1과 완료 표시는 반드시 **공개된 규칙**만 대상으로 해야 한다.
3. **단계를 건너뛰면 새 문장 점이 한꺼번에 쌓인다.** 시안은 "정상 진행한 플레이어는 이전 안내를 수행했다"고 가정하고 가장 최근 규칙만 미수행으로 둔다.
4. `FocusGlobalOption`은 옵션으로 스크롤하지 않고 그룹의 기억된 오프셋만 복원한다. 그래서 위 "EnsureVisible(제안)"이 필요하다.
5. 튜토리얼 브랜치의 `Tutorials.All` 순서(F05, F06, F07, H07…)로 안내 카드를 고르므로 사다리 순서와 다를 수 있다. 시안의 "마을 안내 카드 → 설정해 보기"는 가장 최근에 열린 미수행 규칙을 고른다.

## 시연 값과 단순화

- **시안용 예시**: 전투 미리보기(모의 시뮬레이션이며 실제 전투가 아니다), 지난 사냥 문제 문구와 3박자 문구, 프리셋 슬롯 이름, 영웅 레벨 곡선, 받은 쪽 수.
- **단순화**: 자동 착용 탭은 일반 옵션 행으로 표시(게임은 전용 화면), 공통 공격 설정과 스킬별 직접 설정 세부 편집은 미구현, 코드 공유 대화상자는 토스트, 전사만, 레거시 계정(트리 화면)과 구 편집기 제외.
- 시안에서 영웅은 항상 새 영웅의 "균형" 방식으로 시작하고, 단계를 고르면 해당 단계까지 정상 진행한 상태를 가정한다.

## 검증과 한계

- `node Prototypes/HuntEdict/engine.test.cjs` — 32개: 공개 단계(게임 테스트 표), 탭·그룹·`Next`, 프리셋 적용/일치 왕복, 새 영웅 균형, 마커·완료, 스킬, 프롤로그 레슨 순서.
- `node Prototypes/HuntEdict/browser.test.cjs` — 655개(macOS 헤드리스 크롬, 실제 마우스·키보드): 프롤로그 레슨 크기 5종 × 언어 2종(자동 스크롤 끔), 단계별 탭·그룹, N1~N6와 겹침, 다이얼로그·검색·성향·잠금·스킬 동작, 가로 넘침 없음, 영어 누락 없음, 페이지 오류 없음.
- 미검증: 실기기·터치 입력, 사파리·파이어폭스, 스크린리더, 신규 사용자의 이해도, 게임 코드와의 동작 일치(HTML 시안이므로 Unity에서 다시 검증해야 한다).

## 사용법

```
node Prototypes/HuntEdict/build.cjs                      # 빌드 (튜토리얼 브랜치 체크아웃을 읽음)
node Prototypes/HuntEdict/engine.test.cjs                # 규칙 대조
node Prototypes/HuntEdict/browser.test.cjs --json out.json   # 크롬 검증 (QUICK=1이면 가로·한국어만)
node Prototypes/HuntEdict/capture.cjs [outDir]           # 증거 이미지
```

도구줄에서 화면(가로·세로·PC 16:9·16:10·21:9), 언어, **공개 단계**(프롤로그 ①②부터 균열 20), 튜토리얼 안내선, 자동 스크롤(제안), 링→저장 넘김(제안), "마을 안내 카드 → 설정해 보기"를 바꾼다. 주소 매개변수: `stage`, `device`, `lang`, `tab`, `group`, `custom=1`, `dialog=quick`, `skill`, `policy`, `focus=1`, `locked=1`, `coach=0`, `autoscroll=0`, `embed=1`(도구줄 숨김).

## 다음 단계

계획서 Phase 2(이름 매핑표·구조 변경·창 상태/동작·N1~N6 데이터 모양을 담은 인계 문서와 이식 프롬프트)와 Phase 3(Unity 이식)이 남았다. 이식 전에 튜토리얼 브랜치를 main에 병합해야 한다.

## English

A clickable HTML mockup of the Hunt Edict window (v2) built **around the tutorial's contract**: the tutorial finds controls by name and reads window state, so the 10 tab ids, every `edict-*` name, the named areas and the `ids[0]` rule are kept; only the surface and information design changed. The 5-tab merge of v1 was withdrawn.

- Data is read from the game files (tutorial-branch checkout): 152 options, 35 groups, 17 disclosure rules, 211 quick presets, English from `en.txt`.
- The forced prologue (v3) is replayed with real mouse input at five sizes × two languages with auto-scroll off: every designated control is on screen and never under the gate.
- New slots: N1 new-sentence dots (derived, no save field), N2 default group, N3 next-sentence teaser, N4 three-beat strip (one line by default), N5 caption band (a docked line instead of a floating bubble, which covers the preset tabs in the current game), N6 after-save action.
- 32 engine checks and 655 browser checks pass. Not verified: real devices, other browsers, screen readers, player comprehension, and the real game code (this is an HTML mockup).
