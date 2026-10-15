# 스킬창 캐주얼 스킨 교체 — Codex 작업 지시

작성일: 2026-10-14

아래 전체를 Codex에 그대로 붙여 넣는다. 같은 내용이 `Docs/Design/D4SkillTree/casual/README.md`(시안 설명)와 함께 `claude/casual-art-concept` 브랜치에 있다.

```text
HELLSCRIPT 스킬창(SkillTreeD4Window)의 스킨을 새 캐주얼 아트 컨셉으로 교체해 줘. 규칙·배치·문구·상호작용은 그대로 두고 모양(색, 글꼴, 테두리, 그림자, 장식)만 바꾸는 작업이야.

## 배경
- 게임 아트가 어두운 고딕("Obsidian & Gilt")에서 밝고 아기자기한 캐주얼 중세 판타지(스타일 D, 12세 이상)로 바뀐다. 컨셉 기록: Docs/Art/CasualConcept/2026-10-13/Casual_Art_Concept_20261013(.en).md
- 스킬창 새 시안은 사용자가 확인했다(디자인 캔버스 https://claude.ai/artifact/JsCKnJUbKkzfBYyJDR3493). 시안 파일은 claude/casual-art-concept 브랜치의 Docs/Design/D4SkillTree/casual/ 에 있다. 먼저 README.md를 읽어라.
- 게임에 옮길 기준은 SkillWindow.casual.locked.dc.html 이다. 캔버스 시안에서 배치에 영향을 주는 글자 크기와 순위 배지 크기를 원본으로 되돌려, 모든 사각형·글자 크기가 원본과 같다. 캔버스 시안 그대로의 큰 글자(SkillWindow.casual.dc.html)를 게임에 넣을지는 사용자가 따로 정한다. 이번 작업에서는 하지 않는다.

## 시작
1. 규칙: AGENTS.md, AgentRules/(Shared_UI, UI_Refresh, Workflow, CPU_Performance, Unity_MCP, Wiki_Publish), CLAUDE.md. 한 작업·한 에이전트, 대화·개발 기록은 한국어, 코드 주석·커밋 메시지·로그는 영어.
2. 최신 origin/main에서 새 브랜치 codex/casual-skill-skin 을 worktree로 만든다(다른 세션이 같은 체크아웃을 쓴다). 시안만 가져온다:
   git fetch origin claude/casual-art-concept && git checkout origin/claude/casual-art-concept -- Docs/Design/D4SkillTree/casual
3. 스킬창 구현 위치: Assets/HELLSCRIPT/Runtime/Presentation/SkillTreeD4Window*.cs, Presentation/SkillTreeD4/*.cs, 모양 데이터 Resources/Data/SkillTreeD4Looks.json(생성물), Resources/Data/SkillTreeD4Fx.json, Resources/SkillTreeD4Tone.shader. 정답 파일: Docs/Design/D4SkillTree/golden/ (README.md의 충실도 계약 포함).

## 범위
할 것: 시안의 모양을 게임에 반영한다.
- 창 바탕(하늘 그라데이션), 판(크림색, 남색 외곽선, 아래쪽 단단한 그림자, 점선 안쪽 테두리, 위쪽 둥근 젬), 번진 아트 배경 제거
- 탭, 티어 배지, 스킬 카드, 보조 스킬 칸, 패시브 메달, 순위 호, 연결선(3px, 켜진 선 밑 남색 밑줄), 범례, 상세 패널(활성/패시브), 단추(투자 초록, 회수 흰색, 닫기 코랄)
- 글꼴(제목·탭·숫자·단추·소제목 Jua, 본문 Noto Sans KR 700/900)과 글자 색, 제목의 남색 외곽선
- 연출 색(Fx): 시안의 FXC(금 255,194,26 / 파랑 53,167,255 / 회 150,156,180)에 맞춘다.
하지 않을 것: 규칙 엔진, 배치 사각형·글자 크기, 문구, 상호작용, 전투, 저장, 스킬 아이콘 그림(기존 그대로), 다른 창(레거시 HuntEdictWindow의 node-frame-*.png, tree-backdrop.png 포함), 전역 UiTheme 램프(다른 창이 쓴다. 스킬창 전용 색은 SkillTreeD4Looks 팔레트로 간다).

## 순서
1. SkillWindow.casual.locked.dc.html 을 Docs/Design/D4SkillTree/SkillWindow.dc.html 로 교체한다. 어두운 원본은 Docs/Design/D4SkillTree/dark/ 에 보관한다(롤백용).
2. 정답 파일을 다시 만든다(golden/build_golden.js, verify_golden.js). 합격 기준: golden/layout, layout_scenarios, viewmodel, interaction, rules_golden, rank_values 의 diff가 비어 있어야 한다. 바뀌어도 되는 것은 styles.json 뿐이다. 다르면 시안이 아니라 원인을 찾아 보고한다. node reference/test_reference.js 도 통과해야 한다.
3. 모양 컴파일러(tools/build_skill_tree_d4_looks.py)를 시안에 맞춘다. 지금 컴파일러는 text-shadow(글자 외곽선), 점선 등 시안이 쓰는 일부를 다루지 않을 수 있다. SkillTreeD4Looks.json 을 다시 만들고 build/check 를 통과시킨다.
4. 그리기 구현을 시안에 맞춘다(SkillTreeD4Looks.cs, PanelLooks, Graphics, PaintGraphic, Fx...). 글자 외곽선은 모바일 비용이 크지 않은 방식으로 고르고(uGUI Outline 8개를 모든 글자에 붙이지 않는다), 근거를 남긴다.
5. 글꼴: Jua(SIL OFL)와 Noto Sans KR 을 UiTheme 소유자를 통해 쓴다. 파일 크기와 라이선스를 확인하고, 크면 한글 2,350자와 라틴 부분집합을 쓴다. WebGL 경로(UiTheme.WebBody)도 확인한다. 라이선스·용량이 문제면 중단하고 사용자에게 묻는다.
6. 새 색을 C# 상수로 흩지 않는다(tools/check_ui_contract.py 가 chrome 계열 hex 리터럴을 거른다). 색은 데이터(Looks/Fx JSON 팔레트)에서 온다.

## 검증(작업 중에는 관련 검사만)
- tools/build_skill_tree_d4_looks.py check, node Docs/Design/D4SkillTree/golden/verify_golden.js, node reference/test_reference.js, Docs/Design/D4SkillTree/golden/csharp 의 dotnet run
- EditMode: Tests/Editor/D4 의 SkillTreeD4LookTests, GraphicsTests, PanelTests, ScreenCoreTests, AnimationTests
- python3 tools/check_ui_contract.py --verify
- 마지막 코드 변경과 main 통합 뒤 한 번만: 전체 EditMode 와 macOS 개발 빌드 런타임 스모크(-hellscriptSkillTreeD4Smoke, 가로 1600x900·세로 900x1600, 한국어·영어). 스크린샷은 시안과 나란히 눈으로 비교한다.
- 시각 기준: golden/reference 의 24장 렌더는 어두운 시안 기준이다. 캐주얼 기준 렌더가 필요하면 Docs/Design/D4SkillTree/README.md 의 QA 절차대로 잠금판에서 다시 뽑는다. 못 하면 못 했다고 적고 사용자에게 요청한다.
- golden/anatomy/ 는 어두운 시안을 브라우저로 잰 값이다. 크기·사각형은 잠금판과 같아 유효하고, 색·글꼴 필드는 시안 코드(renderVals 글자 style)를 근거로 삼는다.
- 성능은 같은 조건의 실제 이전·이후 바이너리 각각 3회 이상 CPU·GC 원자료로만 말한다. 측정하지 않았으면 "미측정"이라고 적는다.

## 산출물
- PR(병합은 하지 않는다. 사용자 승인 뒤에 한다). 관련 검사와 기록을 같은 커밋에 넣는다.
- 한/영 기록: Docs/Implementation/Casual_Skill_Window_Skin_20261014(.en).md — 바꾼 것, 정답 파일 diff 결과, 글꼴 파일·라이선스·용량, 검사 결과, 확인하지 못한 것(실기기, 성능, 캐주얼 기준 렌더)
- 위키 원본이 바뀌면 AgentRules/Wiki_Publish.md 절차를 따른다.

## 보고(한국어)
실제로 확인한 것과 미검증 범위를 구분하고, macOS 결과를 실기기 결과로 말하지 마. 규칙 엔진·배치·문구·상호작용이 시안과 같다는 증거(golden diff 0)를 먼저 보여 줘.
```
