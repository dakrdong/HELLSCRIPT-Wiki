# D4 skill tree design and verification material

Updated: 2026-10-12 · [한국어](README.md)

This folder holds the design, the planning data, the golden files, the reference implementation and the research notes of the Diablo IV style skill tree. The implementation plan starts at `Docs/Implementation/D4_Skill_Tree_Plan_20261012.en.md`.

| Location | Contents | Owner |
|---|---|---|
| `SkillWindow.dc.html` | the design source (a design canvas component). Takes hero, language, layout and view as props | design owner |
| `skill_tree_d4.json` | the **planning data source**: rules, per-class tiers, actives, modifiers, passives, texts, `legacyRemap` | design owner (once implementation starts, the game-side JSON is the reference) |
| `extract.js`, `behaviour.json` | the exporter that builds the spec JSON from the design, and its input | design owner |
| `golden/` | **golden files** produced by really running the design, the **JavaScript reference implementation**, the **C# port**, and tools. Guide: `golden/README.en.md` | design owner |
| `reference/` | 24 reference renders (2x, Korean and English) | design owner |
| `research/` | read-only code research notes made before implementation. `research/README.en.md` | Claude |
| `icons_manifest.json` | the 92 new icons: source name, sha256, target location | Claude |
| `tools/legacy_id_inventory.py` | list of places that name ids that disappear, with `--check` | Claude |

The design canvas (private): https://claude.ai/artifact/4KCzKcxqrBBWrNiJZ5Esg8 — if it can not be opened, the copies in this folder are enough.

## Checking everything at once

```bash
node Docs/Design/D4SkillTree/golden/verify_golden.js                 # do the golden files match the design (about 2 minutes)
node Docs/Design/D4SkillTree/golden/reference/test_reference.js      # JavaScript reference: 55,803 checks match
(cd Docs/Design/D4SkillTree/golden/csharp && dotnet run)             # C# port: the same 55,803 checks match
python3 Docs/Design/D4SkillTree/golden/tools/compare_screens.py --selftest   # checks the pass limits of the visual comparison tool
python3 Docs/Design/D4SkillTree/tools/legacy_id_inventory.py          # list of old id references (--check passes only when implementation is done)
```

When the design changes, the design owner runs `node extract.js SkillWindow.dc.html skill_tree_d4.json behaviour.json` and `node golden/build_golden.js`, confirms that all the checks above pass, and commits them together. **Implementation work does not edit `golden/`, `skill_tree_d4.json` or `SkillWindow.dc.html` in this folder.**
