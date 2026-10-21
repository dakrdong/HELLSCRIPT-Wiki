# 사냥 칙령 Casual 화면 연결 · 2026-10-17

기준 `main`: `dbec4dbed084596471675f375c1d1ac517b4b86c`. 원본 Unity/작업 브랜치/미커밋 파일을 조작하지 않고 소스 전용 sparse clone에서 작업했다. 근거는 실제 Claude 30대지 조사 인계 `task/design-5-hunt-edict.md`와 통합 담당의 `shared-api-handoff.md`다. 조사 인계를 구현 계약으로 사용했으며 모든 대지를 게임에서 검수했다는 뜻은 아니다.

## 적용과 보존

- `GameUI.HuntEdict`의 기존 진입 → `HuntEdictWindow` → `HuntEdictEditSession` → 기존 저장 거래 경로를 유지한다. 초기 수정은 화면 전용 10개 C# 파일이었다. 후속 원문 보완은 2개 전용 부품과 승인된 2D 설명 그림을 포함한 16개 C# 파일이다. 공통 파일·도메인·번역표·D4 트리·스킬 아이콘·3D 미리보기·리소스·scene/prefab·package·설정 변경은 없다.
- `UiSkin.Casual`을 화면 인스턴스에 명시한다. 표면은 공통 둥근 mesh와 중앙 토큰, 버튼은 `ApplyChrome`, 닫기는 CasualClose, 글꼴은 `ApplyLabel/LabelFont`를 사용한다. 본문은 Noto Sans KR, 제목·버튼·탭·숫자는 Jua와 공통 미지원 문자 폴백을 사용한다. 초기 동결에서는 글자 크기와 배치를 유지했으며, 후속은 아래 원문 c/f/l의 반응형 치수로 대체한다. 안전 영역·고정 저장줄·그룹별 스크롤 동작은 유지한다.
- 입력·Dropdown의 비활성 템플릿·toggle·scroll·drag ghost·dialog·skill bubble까지 같은 색 역할을 적용한다. Dropdown 목록 복제는 public serialized 색·radius도 채워 인스턴스 skin 상태 소실을 대비한다. ghost는 스킨을 명시적으로 다시 전달한다. 초기 슬롯 프레임은 아래 후속의 명시 Casual 소켓 고리로 대체한다.
- 초안 저장과 기본 설정/정책 라디오의 즉시저장 예외, 미리보기와 활성화 구분, 실패 대화상자, Canvas 310, binding의 modal/drag 보류, Back 우선순위, 프리셋 5칸·이름 40자·행 링크, 공개/튜토리얼 게이트와 이름/그룹 순서를 보존한다. 직접 설정 미리보기 alpha .65는 이미 충족되어 유지했고 기본 설정 잠금 alpha .45와 구분한다.
- 승인된 기존 semantic donor 아이콘만 재사용한다. 네 전용 아이콘은 아직 최종 그림으로 확보하지 않았으며 socket3 mint는 후속에서 명시 요구대로 적용했다. 신규 문구/공통 API 요청은 없다.

## 원문 확보 후 필수 치수·장식 보완

초기 `d8c3044f`는 조사 요약의 m절 분류를 근거로 새 치수를 미확정으로 다뤘다. 이후 확보된 `task/design-5-spec-ko-original.txt`의 c/f/l은 CSS 변수와 부품 레시피를 제작 정본으로 명시한다. **m절 전체가 선택 사항이라는 이전 판단은 잘못됐으며 이 보완이 대체한다.** 추가 제안인 가로 탭 열 스크롤/2열 전환만 채택하지 않았다.

- 화면 전용 `HuntEdictCasualLayout`으로 PC/가로/세로 보드 픽셀의 margin 12/8/8, padding 20/10/12, panel radius 26/18/24, header 56/36/44, 탭 60/27/70·간격 5/2/6, 글꼴 치수, 본문 gap/cp, 단추 56/36/50, dialog 560/440/396 등을 연결한다. 기존 shared scale의 800×450/405×720 기준을 각 보드 기준으로 보정하며 안전 영역/회전에서 프로필을 갱신한다.
- 바느질·위 금화 젬·금빛 제목·부제 알약·선택 꼬리·그룹 젬·체크 원·사실 칩·티저 금화·점선 옵션/저장 구분·그림 chevron·숫자 원·스크롤 12·84×40 스위치/30 손잡이·26 체크상자·프리셋 행 btn+6과 번호 원을 구현한다. 장식은 모두 raycastTarget=false다. 그룹/이름/프리셋/장비 판정은 기존 데이터를 읽는다.
- 스킬 아이콘은 기존 SkillIconView를 사용하며 이 화면 소켓 고리는 금/파랑/주황/민트와 궁극기 마름모로 그린다. D4 트리와 원화는 바꾸지 않는다. 좁고 짧은 정책 레일은 소켓/이름을 줄이지 않고 스크롤하며, 5개 프리셋 행도 지정 높이를 유지해 목록만 스크롤한다. 탭 열 스크롤/2열 제안과는 별개다.
- 정책 상태 알약·라디오 30·정보 28%·세로 예시 300, 예시 조작 38×32·효과 아이콘 30, 말풍선과 그림자, 다이얼로그 머리 실제 높이/본문 시작/자동 높이를 연결한다. 2D SkillPresetExampleView는 별도 소유 확장 승인 후 선택형 마지막 skin 인자로 Coral 적/Gold 영웅만 연결했고 null은 기존 색을 유지한다. Describe/Cue/순서/시나리오/좌표/효과 의미색/3D는 유지한다.
- 통합 담당의 `SetCasualAppearance`와 Edict* 색 속성 10개를 소비한다. Close는 radius를 생략하고 borderWidth=3/shadow=3만 명시한다. 공통 두 top 색은 통합 담당의 immutable WithButtonTops 후속 패치 소유이며 중복 구현하지 않는다. 네 전용 탭 최종 그림은 원문도 새 그림 필요/자리 그림으로 기록하므로 기존 승인 donor와 젬을 사용한다.

## 초기 동결 d8c3044f의 확인과 미검증

- `python3 tools/check_ui_contract.py --verify`: 15개 회귀 통과. `check_ui_refresh.py`: 14 binding, 195 소스 파일 통과. `git diff --check` 통과.
- 실제 Unity 참조 296개와 analyzer 7개를 읽은 독립 Roslyn Runtime 851소스 컴파일: 오류0, 경고225(변경파일0). 관련 HuntEdict·StoreViewBinding·Localization와 helper 18소스/308참조 컴파일: 오류0/경고0. 첫 test harness는 helper 2개 누락으로 실패했으며 helper를 포함해 재검증했다. 테스트 코드를 실제 실행한 결과는 아니다.
- 변경 전후 control-name 문자열 및 저장/Back 관련 12개 메서드 소스 불변을 확인했다. 파일별 before/after SHA와 정확한 compile inputs/log는 `task-3/evidence`에 보존한다.
- Unity import/Editor 컴파일/EditMode 실행/빌드/게임 조작/픽셀 검수는 0회. KO/EN·440×956·956×440·PC16:9/16:10/21:9, 실제 Jua 특수문자·터치·중첩 pause 복원·저장실패·드롭다운·드래그 인수는 통합 큐에서 남아 있다. 성능 개선은 측정하거나 주장하지 않는다.

## English

The source-only candidate opts HuntEdict into the existing Casual skin without changing shared owners, domain transactions, localization, resources or serialized project assets. It keeps draft saves, immediate defaults/policy activation, control identities, disclosure gates, drag/back ordering and presets. The follow-up replaces the initial geometry with the original specification profiles. Inputs, cloned dropdown templates, scrollbars, drag ghosts, dialogs and skill bubbles use shared palette/font/chrome APIs. Existing approved semantic icon art remains; the screen sockets now use the required gold/blue/orange/mint identities.

The original specification obtained after the first freeze makes sections c/f/l normative. The follow-up implements those profile dimensions and decorations; treating all of m as optional was incorrect. Only the optional landscape tab-column scrolling/two-column alternative remains unselected. Independent Roslyn Runtime (851 sources/296 references/7 analyzers) and focused test-source compilation (18 sources/308 references) passed with zero errors. Static ownership/refresh checks passed. No Unity, NUnit execution, player build or visual acceptance ran. Final language, viewport, input, save-failure and nested-window acceptance belongs to the integration owner after all sources are frozen.

## 보완 단계 검증 경계 / Follow-up validation boundary

보완 입력은 UI 계약 source/기존 Python 회귀, refresh guard, control-name 집합·구분자·프로필 멤버·공통 Edict API 존재와 2D Describe/Cue 불변만 검사한다. 새 C# 컴파일/Unity/import/EditMode/플레이어/브라우저 실행은 0회다. 초기 d8c3044f의 Roslyn 결과를 이 보완 소스의 typecheck나 실행 근거로 재사용하지 않는다. 통합 담당이 공통 후속을 함께 묶어 최종 typecheck와 KO/EN·화면비·입력·저장실패·pause 복원을 검수한다. 정확한 변경 파일/해시/commit은 task-3/handoff.json과 evidence/original-followup-source-proof.json에 보존한다.

The follow-up runs lightweight source/ownership/refresh guards only. No new C# compilation, Unity execution or visual acceptance runs. Earlier Roslyn receipts cover the initial freeze only; final typechecking and acceptance must include the coordinator's common API and button-top patches.
