# 훈련장 폰트 커버리지 감사 / Training font coverage audit

검사 기준은 skill-skin **4139a77e2df495cee009c07c6094b2eea9b1aeaa**의 git 객체입니다. 현재 checkout 파일 대신 `git show <SHA>:<path>`로 폰트·소스·데이터를 읽었습니다. 공통 폰트·소스 변경, Unity 실행, 패키지 설치는 하지 않았습니다.

## 결과 / Result

Noto Sans KR 400/700/900은 아래 한정 문자 집합의 모든 KO 음절, EN 글자, 몬스터 이름과 수치 문자를 포함합니다. **재개 버튼 `▶`(U+25B6)만 세 폰트에서 모두 누락**됩니다. Jua에는 **`± · × – — ’ → − ▶` 9문자**가 없습니다. 이 결과는 파일의 실제 cmap 검사이며 Unity의 폰트 fallback·렌더링 성공을 뜻하지 않습니다.

| 실제 파일 | best cmap/Unicode union 문자 수 | 확장 집합 coverage | 누락 |
| --- | ---: | ---: | --- |
| NotoSansKR-Skill400.ttf | 2604 | 507/508 | ▶ `U+25B6` |
| NotoSansKR-Skill700.ttf | 2604 | 507/508 | ▶ `U+25B6` |
| NotoSansKR-Skill900.ttf | 2604 | 507/508 | ▶ `U+25B6` |
| Jua-SkillSubset.ttf | 2448 | 499/508 | ± `U+00B1` · `U+00B7` × `U+00D7` – `U+2013` — `U+2014` ’ `U+2019` → `U+2192` − `U+2212` ▶ `U+25B6` |

각 폰트의 best cmap과 Unicode cmap 전체 union은 동일합니다. `.notdef`로 매핑되는 항목은 coverage에서 제외했습니다. 따라서 다른 Unicode subtable에만 누락 글자가 숨어 있는 경우도 없습니다.

`GameUI.TrainingGround.cs:120`의 `run.paused?"▶":"||"`가 정확한 Noto 누락 사용처입니다. 이 파일은 훈련·균열 HUD 공유 소유 경계에 있으므로 훈련 전용 작업에서 바꾸지 않습니다. 기존 기준 구현은 `UiLayout.Text`의 OS `UiFonts.Body`와 일부 `UiFonts.Display`를 사용합니다. 이 감사는 해당 문구를 `CasualWeight`/`CasualDisplay`로 옮길 때의 coverage를 검증한 것이며 현재 OS 폰트에서 이미 깨진다고 주장하지 않습니다.

Jua 누락의 대표 사용처:

- `±`, `−`: `TrainingGroundResultWindow.Percent`의 0/감소 퍼센트; `−`는 단계 감소 버튼에도 사용.
- `·`, `×`, `→`: 요약 구분자, 정예 특성/지원 수치, 칙령 변경·공격 순서.
- `–`: EN picker의 `normal monsters 1–{2}`.
- `—`: 직전 기록·비교 값이 없는 상태.
- `’`: EN 결과 설명 `Share of each target group’s total damage`.
- `▶`: 공용 HUD 재개 버튼.

본문·수치·스킬명·요약은 Noto400/700/900 경로를 유지하고, Jua는 검사된 제목에 한정하는 편이 안전합니다. **기준 밖 추가 관측:** task-6의 현재 개발 중 dirty `ContentWindowView.Button`에는 `Skin.DisplayFont`를 버튼 글자에 적용하는 공통 API가 있으나, 이는 4139a77e의 코드가 아닙니다. 이 공통 변경의 SHA가 인계되기 전에는 가져오지 않습니다. 향후 이 API로 adapter를 치환할 때 `×` 닫기·`−` 버튼 등에 Jua가 자동 전파되지 않는지 재확인해야 합니다. 현재 훈련 전용 adapter는 4139a77e 기반 후보입니다. `▶`는 부모/공유 소유자에게 별도 icon 또는 공용 폰트 보완을 요청할 대상입니다. 폰트 확장이나 원문 기호 교체는 이번 감사에서 실행하지 않았습니다.

## 실제 검사 범위 / Exact corpus scope

`font-coverage-audit.json`에 모든 원문/번역, 경로/행 또는 JSON 경로, 입력 git blob·SHA-256, 필수 codepoint 목록, 누락별 원문 예시를 보관했습니다. 코퍼스 기록은 중복 사용처를 보존하고 문자 집합은 중복 제거했습니다.

- 직접 화면/이름 집합: **491 기록**, KO **408자**, EN **82자**, 합집합 **437자**.
- 공용 칙령 요약 카탈로그를 합친 집합: **897 기록**, KO **477자**, EN **84자**, 합집합 **508자**.

포함한 데이터는 훈련장 다섯 partial/결과 화면, 훈련 HUD·컨트롤러·검증/저장 오류, 공유 DPS/차트/스킬 카드 문구, 탈출 확인 제목/버튼/훈련 안내입니다. 동적 이름은 `GameCatalog` 직업·실행 시 적/보스·필드 배열, `EnemyCombat` 역할·특성, `MonsterRegistry.json`의 **35개 명칭**, D4 액티브/궁극기 **30개**, `ClassSkills.json` 액티브/궁극기 **30개**, 기본 공격명을 포함합니다. 패시브/장비명·별도의 B 훈련 비교 UI는 훈련장 화면 집합에서 제외했습니다.

훈련 결과의 `TrainingGround.DescribeChanges`가 `EdictLine` 및 `HuntEdictSummary.ForGroup`을 부르므로 skill/basic 프리셋 이름과 choice fallback 이름도 포함합니다. 공용 요약 확장 집합은 `HuntEdictUi` group.title/label.text, `HuntEdictSummary`, `EdictOptionLabels`, `EdictOptions`, `EdictTargetLabels.Title`, 장비 위치/자동 착용 모드 이름의 한글 문자열을 보존한 보수적 카탈로그입니다. **모든 전역 옵션 문구가 훈련의 모든 상태에서 도달한다고 주장하지 않습니다.** 전체 칙령 편집기 도움말/설명, 전체 도감, 장비·아이템 이름은 넣지 않았습니다.

최소 문자 집합의 정의는 위 선택된 실제 소스/데이터의 KO와 실제 EN 출력 문자열에서 필요한 Unicode scalar의 합집합입니다. C# 주석과 char literal은 제외하고 ordinary/verbatim 문자열의 escape를 해제했습니다. `.NET` format placeholder, rich-text tag와 개행/제어 문자는 glyph 요구에서 제외했습니다. 숫자 0–9, 시간/퍼센트·수치 구분자, 직접 표시되는 ASCII와 기호를 명시적으로 추가했습니다. 한글 Unicode 블록이나 폰트 subset 범위를 추정해서 채우지 않았습니다.

## Localization 구조 / Localization structure

`Resources/Localization/en.txt`는 CSV/JSON이 아닌 **탭 구분 한국어 키 → 영어 값**입니다. `LocalizationTable.Parse`처럼 첫 탭에서 나누고 `#` 주석/빈 줄을 제외했으며 `\n`, `\r`, `\t`, `\\` escape를 풀었습니다. 중복 키가 있으면 처음 값을 유지하는 방식입니다. 기준 파일은 **7,368개 키, 중복 0개**입니다. 이 코퍼스의 exact KO lookup은 **누락 0개**였고, 스킬/프리셋 JSON의 `name/nameEn` 또는 `ko/en`은 테이블이 아닌 직접 언어 선택 경로로 비교했습니다. 문자열 조각/조합식은 확장 집합의 보수적 범위이며 번역 UI 전체의 정확성을 증명하지는 않습니다.

## 재현 근거와 한계 / Reproducibility and limits

- `NotoSansKR-Skill400.ttf`: 515,376 bytes, SHA-256 `00bad3cfb48563a45c51b13089ce83356bb290ab39acb993c2f46c0cb556528c` (git blob `65baf5495d9a58064ea075928ff5cf24602c27fd`).
- `NotoSansKR-Skill700.ttf`: 515,908 bytes, SHA-256 `56c404ca5dd5bdf99c634ad5675ee3b5860123ea1f5a82472cfc0ba812b7e72b` (git blob `b7e88202e4111e760e7cf51ca8ce891696098db8`).
- `NotoSansKR-Skill900.ttf`: 515,760 bytes, SHA-256 `a574ab95fca157c65692644dc7ebd0f564689701af88c5101cd9930045a9db79` (git blob `00922c65fd9eddc485b1be2880c6ab0972c52806`).
- `Jua-SkillSubset.ttf`: 2,035,040 bytes, SHA-256 `32743ae07a2af66d7f6ad066fc9ac91c713156174780ac126fcd0d5b83e5986e` (git blob `bc6c7b136db514849a745f4fad15cdd2ee9c9238`).

검사 도구: Python `3.9.6`, fontTools `4.60.2`. 35개 몬스터 명칭은 각 언어의 직접 name 레코드에서 별도 검사했고 네 폰트 모두 누락 0자입니다. 모든 KO 음절도 네 파일에서 누락 0자입니다. JSON의 `corpus`, `required_sets`, `fonts[].checks`로 집합과 각 누락을 재계산할 수 있습니다.

범위 밖: 사용자 입력, 임의 exception/OS 진단 문자열, 유효하지 않은 ID, 미래 콘텐츠, 내포된 전체 칙령/도감 UI. 저장 실패의 작성된 안내문은 포함했지만 `GameStore.Error=e.Message`의 무한한 외부 문자열은 유한 집합으로 보장할 수 없습니다. Unity glyph rasterization/fallback, 글자 폭·잘림, 2868×1320/모바일 레이아웃은 아직 검증하지 않았습니다.

## English handoff

At exact baseline 4139a77e, all three bundled Noto weights cover the bounded authored KO/EN training corpus except **U+25B6 BLACK RIGHT-POINTING TRIANGLE** used by the shared HUD resume button. Jua lacks nine punctuation/symbol codepoints listed above; it has no missing Hangul syllables in this corpus. The actual cmaps contain 2,604 / 2,448 Unicode entries, respectively. The direct training/name set has 437 distinct codepoints; a separately identified conservative shared-edict-summary closure brings it to 508. No whole-Hangul-block assumption was used. All 35 registry monster names are covered by all four fonts in both languages. Use Noto for text/numbers and limit Jua to validated display labels. Common font/HUD ownership remains with the shared owner. This was static, read-only analysis; Unity rendering and layout verification remain pending.
