# D4 스킬 트리 디자인·검증 자료

작성일: 2026-10-12 · [English](README.en.md)

디아블로4식 스킬 트리의 시안, 기획 데이터, 정답 파일, 참조 구현, 조사 기록이 모여 있는 폴더다. 구현 계획은 `Docs/Implementation/D4_Skill_Tree_Plan_20261012.md`에서 시작한다.

| 위치 | 내용 | 소유 |
|---|---|---|
| `SkillWindow.dc.html` | 시안 원본(디자인 캔버스의 컴포넌트). 직업·언어·배치·보기를 속성으로 받는다 | 디자인 담당 |
| `skill_tree_d4.json` | **기획 데이터 원본**: 규칙, 직업별 티어·액티브·보조 스킬·패시브, 문구, `legacyRemap` | 디자인 담당 (구현이 시작되면 게임 쪽 JSON이 기준) |
| `extract.js`, `behaviour.json` | 시안에서 명세 JSON을 만드는 내보내기와 그 입력 | 디자인 담당 |
| `golden/` | 시안을 실제로 돌려 얻은 **정답 파일**, **JavaScript 참조 구현**, **C# 이식본**, 도구. 설명서 `golden/README.md` | 디자인 담당 |
| `reference/` | 기준 렌더 24장(2배, 한국어·영어) | 디자인 담당 |
| `research/` | 구현 전 코드 조사 기록(읽기 전용). `research/README.md` | Claude |
| `icons_manifest.json` | 새 아이콘 92장: 원본 이름, sha256, 가져올 위치 | Claude |
| `tools/legacy_id_inventory.py` | 사라지는 id를 가리키는 곳의 목록과 `--check` | Claude |

시안 디자인 캔버스(비공개): https://claude.ai/artifact/4KCzKcxqrBBWrNiJZ5Esg8 — 열 수 없으면 이 폴더의 사본으로 충분하다.

## 한 번에 확인하기

```bash
node Docs/Design/D4SkillTree/golden/verify_golden.js                 # 정답 파일이 시안과 같은가 (약 2분)
node Docs/Design/D4SkillTree/golden/reference/test_reference.js      # JavaScript 참조 구현: 55,803개 일치
(cd Docs/Design/D4SkillTree/golden/csharp && dotnet run)             # C# 이식본: 같은 55,803개 일치
python3 Docs/Design/D4SkillTree/golden/tools/compare_screens.py --selftest   # 눈으로 비교 도구의 합격선 점검
python3 Docs/Design/D4SkillTree/tools/legacy_id_inventory.py          # 옛 id 참조 목록(--check는 구현이 끝나야 통과)
```

시안이 바뀌면 디자인 담당이 `node extract.js SkillWindow.dc.html skill_tree_d4.json behaviour.json`, `node golden/build_golden.js`를 돌리고 위 검사가 모두 통과하는 것을 확인한 뒤 함께 커밋한다. **구현 작업은 이 폴더의 `golden/`, `skill_tree_d4.json`, `SkillWindow.dc.html`을 고치지 않는다.**
