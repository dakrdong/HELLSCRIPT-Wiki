# HELLSCRIPT 스킬 트리 HTML / Skill tree HTML

작성일 / Created: 2026-09-23

[전사 / Warrior](Warrior.html) · [궁수 / Ranger](Ranger.html) · [마법사 / Mage](Mage.html)

현재 프로젝트의 111개 스킬과 개별 PNG를 사용한 성장 설계 미리보기입니다. 자세한 규칙과 소유 경계는 [한국어 기획](../../Docs/Design/Class_Skill_Trees.md)과 [영어 기획](../../Docs/Design/Class_Skill_Trees.en.md)에 있습니다.

This progression proposal uses all 111 current project skills and individual PNGs. See the linked Korean and English design records for rules and ownership boundaries.

111개 스킬의 효과와 실제 코드를 대조했습니다. 궁극기마다 전용 패시브를 하나씩 연결했고, WP19·AP19·MP19와 새 아이콘을 추가했습니다. 35개는 실제 효과의 대상·발동 스킬을 요구하고, 나머지는 줄기의 앞 스킬이 열려 있어야 합니다(시작 스킬 12개 제외, 2026-10-05). 궁극기 6개의 공통 조건은 40레벨과 직전 30~39레벨 구간 스킬 하나의 활성화입니다. 직접 대상 선택 연결선은 관련 스킬을 선택할 때 표시합니다.

All 111 effects and runtime owners were audited. Each ultimate has a dedicated passive; WP19, AP19 and MP19 have new icons. 35 skills require actual effect targets/triggers, the rest need the parent skill of their branch to be open (except 12 starting skills, 2026-10-05), and six ultimates retain level 40 plus one preceding-stage skill. Direct alternative wires appear when tracing a related skill.

HTML을 직접 열거나 저장소 루트에서 로컬 서버를 실행합니다. 스킬 이미지는 `../../Assets/HELLSCRIPT/Resources/Art/ClassSkillIcons/`를 참조하므로 저장소 폴더 구조를 유지합니다. 인터넷이나 외부 글꼴은 필요하지 않습니다. 브라우저 배분은 게임 저장에 적용되지 않습니다.

Open an HTML file directly or serve the repository root locally. Preserve the folder layout: artwork is referenced from `../../Assets/HELLSCRIPT/Resources/Art/ClassSkillIcons/`. No internet or external fonts are required. Browser allocations do not affect game saves.

```sh
node Prototypes/SkillTree/build.cjs
node --test Prototypes/SkillTree/engine.test.cjs
python3 -m http.server 4192 --bind 127.0.0.1
```

브라우저 검사는 설치된 `playwright` 패키지를 사용합니다. 필요하면 `NODE_PATH`에 패키지 경로, `PLAYWRIGHT_EXECUTABLE`에 별도 검증용 Chromium 경로를 지정하고 `node Prototypes/SkillTree/browser.test.cjs`를 실행합니다. 기본 검증 URL은 로컬 4192 포트이며 `SKILL_TREE_URL`로 다른 로컬 포트를 지정할 수 있습니다. 사용자의 외부 브라우저에 연결하지 않습니다.

Browser checks use the installed `playwright` package. If necessary, provide its location through `NODE_PATH` and an isolated testing Chromium executable through `PLAYWRIGHT_EXECUTABLE`, then run `node Prototypes/SkillTree/browser.test.cjs`. The default test URL uses local port 4192; `SKILL_TREE_URL` can select another local port. Tests do not attach to the user's external browser.
