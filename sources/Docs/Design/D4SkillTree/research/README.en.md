# Code research notes (read-only, before implementation)

Written: 2026-10-12 · [한국어](README.md)

Notes on how today's code meets each part of the plan, written before Codex implements anything. Nothing was changed; the code was only read. The base is `main` `a3fe67c5` (the base of this branch). **Line numbers are from that commit** and drift as the code changes; search by function and class name.

| File | Contents | Used by |
|---|---|---|
| `ui_infrastructure.md` | drawing tools (`UiMesh` ...) with their gaps, adapter window units, scrolling and input, text, animation, smoke framework, EditMode UI tests, icons and masks, file conventions, risks | UI spec |
| `combat_warrior.md` `combat_ranger.md` `combat_mage.md` | per-class combat hooks. Every skill classified A (numbers only), B (new behaviour on an existing path) or C (new mechanic); passives and key passives; test fixtures; open questions | combat spec |
| `save_migration.md` | storage model, migration order and algorithm, level cap 40 -> 60 impact, test patterns | plan chapter 7, step 2 brief |
| `dependents.md` | the old skill window and everything leaning on it (quick edit, tutorials, edict, tools, text, smokes, tests) plus 15 easy-to-forget places | plan chapter 12, step 6 and 7 briefs |

The **open questions** at the end of each file could not be settled from code. The important ones are collected in the plan's decision list (D1 ...) and in the combat spec appendix.

Why English: they are mostly code identifiers and `file:line` citations, which are harder to find when translated. The deliverables (plan, specs, briefs) are kept in both Korean and English.
