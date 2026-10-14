# Step Briefs and Codex Prompts (for Codex)

Updated: 2026-10-12 · For: Codex implementation work · [한국어](D4_Skill_Tree_Step_Briefs_20261012.md)

Author: Claude (briefs). Codex implements, verifies and commits.

This document expands section 14 (work order) of the plan into **a brief that can be started right away, one for each step**. Each step has: prerequisites, documents to read, what to create and change, tests, the done-when criteria, what not to do, and **a prompt to paste into Codex as is**.

## 0. Reference map

| Document | Contents |
|---|---|
| [`D4_Skill_Tree_Plan_20261012.en.md`](D4_Skill_Tree_Plan_20261012.en.md) | Rules, data, save, combat, screen, work order, decision list (section 17) |
| [`D4_Skill_Tree_UI_Spec_20261012.en.md`](D4_Skill_Tree_UI_Spec_20261012.en.md) | Screen implementation spec (6 fidelity layers, units, layering, drawing tools, look, text, panel, effects, work packages UI-1~6, visual comparison) |
| [`D4_Skill_Tree_Combat_Spec_20261012.en.md`](D4_Skill_Tree_Combat_Spec_20261012.en.md) | Combat implementation spec (common framework, classification of the 84 modifiers, passives, ultimates, questions and defaults) |
| `Docs/Design/D4SkillTree/skill_tree_d4.json` | Source of truth for the design data |
| `Docs/Design/D4SkillTree/golden/` | Golden files + JS reference implementation + **verified C# port** (`csharp/`). Start with `README.md` |
| `Docs/Design/D4SkillTree/research/` | Code research notes (read-only): screen infrastructure, per-class combat, save and migration, dependents |
| `Docs/Design/D4SkillTree/icons_manifest.json` | Original path, sha256 and destination of each of the 92 new icons |
| `Docs/Design/D4SkillTree/reference/*.jpg` | 24 reference renders |

## 1. Rules common to all steps

- Follow the project instructions (`AGENTS.md`, `AgentRules/`, `CLAUDE.md`) first. Conversation with the user and development records are in **Korean**; code, comments, commit messages and logs are in **English**; deliverables are in Korean and English together.
- One step = one branch (`codex/d4-skill-tree-<step>`) = one PR. Start from the latest `origin/main` in a **separate worktree**. If step 0 is not merged yet, start from `claude/d4-skill-tree-plan`. Do not switch branches in the shared checkout.
- The default is one task and one agent, and at most 3 tasks run concurrently. Follow the dependency table below.
- **During work, run only the checks for the part you changed.** Full EditMode and the macOS smoke test run **once, at the end of step 7**. If you change only documents, do not run game execution checks.
- **Do not edit `Docs/Design/D4SkillTree/golden/`, `skill_tree_d4.json` or `SkillWindow.dc.html`** (they are the design owner's files). If you suspect that a golden file is wrong, report the name and input of the failing check and stop. If the implementation side has to change a number first, **fix the game-side JSON first** and tell the design owner (plan section 5.3).
- Do not build or publish the wiki from a work branch. Do it once, at merge (`AgentRules/Wiki_Publish.md`).
- Colour literals become `UiTheme` tokens, new Korean strings come with `en.txt` entries (`LocalizationTests`), and a new window comes with an `ADAPTERS` registration and documentation (UI spec section 3.2).
- Collect new outputs (builds, screenshots, comparison results) in one place per owning task, record them in the `artifact-lifecycle.json` format, and write down their retention and cleanup state when you finish (`AgentRules/Artifact_Cleanup.md`).
- Result report: what you changed, **the checks you actually ran and their results**, the scope you did not run, and anything that needs a user decision. Do not report macOS results as physical-device results.
- **PR description template**: ① summary of changes ② verification (commands, results, numbers) ③ unverified scope ④ default decisions used (D numbers) ⑤ what is handed to the next step.

## 2. Steps and dependencies

| Step | Branch | Prerequisite | Can run in parallel |
|---|---|---|---|
| 0 | `claude/d4-skill-tree-plan` | | The user merges it by PR (including the wiki publish) |
| 1 Data and rules | `codex/d4-skill-tree-data` | 0 | |
| 2 Save, migration, level cap | `codex/d4-skill-tree-save` | 1 | In parallel with 3 |
| 3 Icons | `codex/d4-skill-tree-icons` | 0 | In parallel with 1 and 2 |
| 4 Skill window UI-1~6 | `codex/d4-skill-tree-ui-<n>` | 1 (and 3 from UI-3 on) | In parallel with 5 |
| 5.0 Common combat framework | `codex/d4-skill-tree-combat-core` | 1, 2 | |
| 5a/5b/5c Per-class combat | `codex/d4-skill-tree-combat-<class>` | 5.0 | In parallel with each other (up to 3) |
| 6 Integration | `codex/d4-skill-tree-integration-<n>` | 4 (UI-6), 5 | |
| 7 Switch-over | `codex/d4-skill-tree-switch` | 1~6 | |

## 3. Step 0 — Merge the documents (user)

- The user merges `claude/d4-skill-tree-plan` by PR. This change holds only documents and design and golden material (no game code).
- Checks before the merge (already run): `node Docs/Design/D4SkillTree/golden/verify_golden.js` (the golden files match the design), `node Docs/Design/D4SkillTree/golden/reference/test_reference.js`, `cd Docs/Design/D4SkillTree/golden/csharp && dotnet run` (both match all 55,803 checks).
- Merging adds to `Docs/`, so the wiki publish procedure (`AgentRules/Wiki_Publish.md`) applies.

## 4. Step 1 — Data, generator and rules engine

**Prerequisite**: 0. **Branch**: `codex/d4-skill-tree-data`.

**What to read**: plan sections 5 and 6 → `golden/README.md` → `golden/csharp/SkillTreeD4.Model.cs`, `.Rules.cs` → `golden/reference/rules.js`.

**What to create and change**
1. `tools/build_skill_tree_d4.py` (+ a `check` mode): builds `Assets/HELLSCRIPT/Resources/Data/SkillTreeD4.json` from `skill_tree_d4.json`. What to include: the rules (tier gates, level cap), per-class tiers, actives (stats `rank1/rankMax/value`, modifiers `ko/en/effect/shape/value`, descriptions), passives (`linkedActiveIndices`, `value`), `spriteCells`, the screen strings `strings`, and `legacyRemap` (used by the migration). **What not to include**: `designSample`, `designSelection`, `behaviourKo` (design-only).
2. Move `Runtime/Core/SkillTreeD4/SkillTreeD4.Model.cs` and `.Rules.cs` over from `golden/csharp` (you may change the names and folders to fit the game project's conventions). `SkillTreeD4.Data.cs`: reads `SkillTreeD4.json` through `JsonUtility` DTOs and fills the same model classes. A static class `SkillTreeD4` (an `Enabled` flag, default false, turned on with `-hellscriptSkillTreeD4`).
3. Per-rank values: `SkillTreeD4Values` (returns the value at rank r from the `value` of a skill, modifier or passive. Linear; count units `명 회 발 개` (targets, times, shots, pieces) use `floor`). It must **match `golden/rank_values.json` in every entry**.
4. Tests (`Tests/Editor/`): move `GoldenJson.cs` over from `golden/csharp/Tests`. `SkillTreeD4RulesTests` (the 32,208 cases of `rules_golden.json`), `SkillTreeD4DataTests` (the generated JSON equals the design export, rank values match, icon ids correspond to cells), and the model that `SkillTreeD4RulesTests` uses is **the one read from the game data**.
5. Read path: the tests read the golden files with `Path.Combine(Application.dataPath, "..", "Docs", "Design", "D4SkillTree", "golden", …)`. Do not copy them.

**Done when**: `python3 tools/build_skill_tree_d4.py check` passes, and the EditMode `SkillTreeD4RulesTests` and `SkillTreeD4DataTests` pass (32,208 cases + rank values). This PR does not change game behaviour (it is behind the flag).

**What not to do**: Do not write the rules anywhere else. Do not edit `golden/`. Do not touch the old `ClassSkills.json` and `ClassSkillTree.json`.

**Prompt**
```
Work in the HELLSCRIPT repository. First read and follow AGENTS.md, AgentRules/Workflow.md and CLAUDE.md (conversation and records in Korean; code, comments, commit messages and logs in English).
Start branch codex/d4-skill-tree-data in a new worktree from the latest origin/main (or from claude/d4-skill-tree-plan if step 0 is not merged yet).
Documents to read (in order): Docs/Implementation/D4_Skill_Tree_Step_Briefs_20261012.md section 4, D4_Skill_Tree_Plan_20261012.md sections 5 and 6, Docs/Design/D4SkillTree/golden/README.md, golden/csharp/SkillTreeD4.Model.cs and SkillTreeD4.Rules.cs.
Task: step 1 (data, generator and rules engine). Build Resources/Data/SkillTreeD4.json with tools/build_skill_tree_d4.py (including a check mode), move Model and Rules from golden/csharp to Runtime/Core/SkillTreeD4, and create SkillTreeD4.Data.cs (JsonUtility DTOs) and SkillTreeD4Values (per-rank values). Add SkillTreeD4.Enabled (default false).
Verification: python3 tools/build_skill_tree_d4.py check, EditMode SkillTreeD4RulesTests (all 32,208 cases in rules_golden.json match) and SkillTreeD4DataTests (golden/rank_values.json matches). Do not run full EditMode or the smoke tests.
Do not: edit Docs/Design/D4SkillTree/golden, skill_tree_d4.json or SkillWindow.dc.html. Do not write the rules anywhere else. Do not touch the old ClassSkills.json and ClassSkillTree.json.
Report: what you changed, the checks you actually ran and their results (numbers), the scope you did not run, and anything that needs a user decision. Follow the template in section 1 of the brief for the PR description.
```

## 5. Step 2 — Save v5, migration, level cap 60, points = level, loadout rules

**Prerequisite**: 1. **Branch**: `codex/d4-skill-tree-save`. **This is the riskiest step**: every save passes through it.

**What to read**: plan section 7 → `research/save_migration.md` **in full** (section 0, "Findings that change the plan", 10 of them; section D, the algorithm; section E, the test method) → plan section 17, D3, D4, D9, D10, and the new decisions D13~D20.

**What to create and change**
1. **v5 allocation save**: version 5 in `ClassSkillLoadout`. Add `modifier` (default −1) and `modifierRank` to `SkillInvestment`. `Rank(id)` returns 0 for a missing id in v5. `Canonical` **preserves the version** (the v2 validation heroes are used by hundreds of tests), and the conversion is done at the account level. v5 load validation: `ranks` has **10+10 = 20 entries** per class (in the order of `skill_tree_d4.json`: 10 actives = 8 regular + 2 ultimates, then 10 passives = 8 regular + 2 key). All entries are the same `SkillInvestment`, and a passive entry has `modifier == -1` and `modifierRank == 0`. In addition: rank ≤ max (5; 3 for ultimates), `modifierRank == 0` only when `modifier == -1`, `modifierRank ≤ min(max, rank)`, if a passive's rank is > 0 then its unlock level ≤ hero level, and only one key passive holds points, `Spent ≤ level`, `SkillTreeD4Rules.Valid`, an equipped skill has rank > 0 and slot 4 is an ultimate, level ≤ 60.
2. **Migration**: `SkillTreeD4Migration.Apply(AccountSave)` — public static, pure (no I/O), returns a report. **Call it in `GameStore.Read()` before `Normalize`** (finding 1: a change made after loading is caught by the `Write` validation as a player edit, so the app may fail to start). `GameStore.Sandbox` and the test fixtures call it too. Follow the algorithm in save research section D, steps 1~10, exactly (per hero: read `old` → refund → empty v5 allocation → clean up edict documents, presets, training records and puzzle kits → keep the original → notice). Wrap failures in `NotSupportedException` (finding 2). It must be idempotent.
3. **Schema**: `GameStore.MaximumSchemaVersion` 21 → 22. Old binaries reject 22. Before migrating, copy the original to `.schema{n}-before-d4-skill-tree.json` (guarded by `!File.Exists`).
4. **Level cap 60**: `LevelCap` (`ClassSkills.cs:62`), the bare 40 at `GameStore.cs:64`, `ClassSkills.cs:137` and `:147`, `ClassSkillTree.cs:42` (cache key). The XP formula stays as it is and only the cap is raised (D9). Fixtures pinned to level 40 (`CombatSimulation.SkillPreview.cs:31`, `ClassSkillValidation.cs:33`, `HuntEdictWindow.CombatPreview.cs:90`, `FullSkillTraining`) are separated from `LevelCap` and get an explicit constant.
5. **Points and loadout**: `Remaining = level − Spent`. Restate in v5 the rule that rank 0 cannot be equipped.
6. **Share codes**: emit v5 under a new prefix. Old HED3/4/5 codes either import only the policy (options, order, automatic) or are rejected (D14 default).
7. Handling of a run in progress (D13 default: switch to an empty allocation and clean up `ClassSkillRuntimeState`).
8. Balance1000: if you edit a fingerprinted file, run `python3 tools/balance_1000.py` again to refresh the outputs.

**Tests**: before the step starts, create the **original bytes** of old saves in `Tests/Fixtures/D4Migration/` (v4, v3, none/legacy, a run in progress holding W15 and M13 casts, 5 presets, HED3/4/5 codes). Rebuilding them with the new classes does not give the true old shape. Then write these tests: "old save X migrates to exactly Y" (recipe in save research section E), idempotence (opening a second time), the schema guard (rejecting a newer schema, with the bytes unchanged), a progressive-disclosure account, and `Valid` for every allocation in `rules_golden.json` after save → load → `Encode`.

**Done when**: the migration, compatibility and level cap tests pass, the existing `ClassSkillCompatibilityTests`, `GameStoreTests` and the affected suites pass, and the behaviour of `classSkillValidation` heroes is unchanged.

**What not to do**: Do not rename save keys. Do not re-allocate automatically (D4). Do not write during migration without keeping the original. **In a build where `SkillTreeD4.Enabled` is off, do not trigger the migration, schema 22 or level cap 60** (D18 default: only when it is on. A build with it off rejects 22 if it reads it).

**Prompt**
```
Work in the HELLSCRIPT repository. First read and follow AGENTS.md, AgentRules/Workflow.md and CLAUDE.md (conversation and records in Korean; code, comments, commit messages and logs in English).
Start branch codex/d4-skill-tree-save in a new worktree from the latest origin/main, with step 1 merged.
Documents to read (in order): Docs/Implementation/D4_Skill_Tree_Step_Briefs_20261012.md section 5, D4_Skill_Tree_Plan_20261012.md section 7 and section 17 (D3 D4 D9 D10 D13~D20), Docs/Design/D4SkillTree/research/save_migration.md in full.
Task: step 2. (1) ClassSkillLoadout v5 (SkillInvestment.modifier/modifierRank, Rank() returns 0 by default, load validation) (2) call SkillTreeD4Migration.Apply(AccountSave) in GameStore.Read() before Normalize, keep the original, idempotent (3) MaximumSchemaVersion 21→22 (4) level cap 60, and separate the fixtures pinned to level 40 (5) points = level, no equipping at rank 0 (6) share code v5 prefix (7) handling of a run in progress.
Tests: first create the original bytes of old saves in Tests/Fixtures/D4Migration/, then write the "old save X migrates to exactly Y" tests, idempotence, schema guard, progressive-disclosure account and the rules_golden allocation round trip. Run only the related suites. If you edit a fingerprinted file, run tools/balance_1000.py again.
Do not: rename save keys, re-allocate automatically, write without keeping the original, edit Docs/Design/D4SkillTree/golden.
Report: what you changed, the checks you ran and their results, the scope you did not run, and anything that needs a user decision (which of D13~D20 you used the default for).
```

## 6. Step 3 — 92 icons

**Prerequisite**: 0 (can run in parallel with the data step, 1). **Branch**: `codex/d4-skill-tree-icons`.

**What to read**: plan section 11 → `Docs/Design/D4SkillTree/icons_manifest.json`.

**Work**
1. **Check the input**: confirm that `~/HellscriptOverhaulWork/work/d4skills/<Warrior|Ranger|Mage>/out/` exists and matches the sha256 in `icons_manifest.json`. If it is missing or different, **stop and tell the user** (this folder is outside the repository).
2. **Representative first** (`AgentRules/Workflow.md`): import one modifier icon (e.g. `Warrior/W09-1`) as `Assets/HELLSCRIPT/Resources/Art/ClassSkillIcons/<Class>/<id>.png` (file name `iconId` → `id`) and check the import, the budget and the on-screen display: the `.meta` uses the same settings as the neighbouring icons with a new guid, the `Art/ClassSkillIcons/` row of `ResourceTextureBudget` (Android 512 ASTC 6×6, desktop BC7, web ETC2) covers it, a `BuildResourceTests` sample, and display through `SkillIconView`. If it passes, import the rest in one batch per class.
3. **Two overwrites**: `Ranger/A06.png` (Shadow Arrow → Elemental Imbuement) and `Mage/M07.png` (Ember Lance → Elemental Lance) already exist under the same names. Check that **the `.meta` guid is unchanged** and commit it together with them.
4. **Id resolution**: `SkillIconAssets.TryResolve` does not know modifier ids (`W09-1`) or key passives (`WP20`); it knows only the ids that `ClassSkills.Find` returns. Extend it to resolve the new ids to `Art/ClassSkillIcons/<Class>/<id>` and test it.
5. **Provenance record**: add 92 rows to `Docs/Art/ClassSkillIcons/manifest.json` (original path, new name, sha256, `productionApproved=false`, model unknown). Check for native alpha with `tools/adopt_generated_game_art.py`.
6. **Size**: under the new rule the total texture size is about 11MB on Android (ASTC 6×6) and **about 24MB each on desktop (BC7) and web (ETC2)** (512² × 92, no mipmaps). Correct the numbers in D11 of plan section 17 to these values and report the effect on the web build size.

**Done when**: all 92 icons are in, and you have `BuildResourceTests`, the texture budget test, the `.meta` guid check, and on-screen display evidence for the one representative icon.

**What not to do**: Do not resize, regenerate or remove the backgrounds of the icons (plan section 16). Do not delete the source folder.

**Prompt**
```
Work in the HELLSCRIPT repository. First read and follow AGENTS.md, AgentRules/Workflow.md, AgentRules/Artifact_Cleanup.md and CLAUDE.md.
Start branch codex/d4-skill-tree-icons in a new worktree from the latest origin/main.
Documents to read: Docs/Implementation/D4_Skill_Tree_Step_Briefs_20261012.md section 6, D4_Skill_Tree_Plan_20261012.md section 11, Docs/Design/D4SkillTree/icons_manifest.json.
Task: step 3. Compare the sha256 of the inputs (~/HellscriptOverhaulWork/work/d4skills/*/out) with icons_manifest.json (if they differ, stop and report). First import one representative icon as Assets/HELLSCRIPT/Resources/Art/ClassSkillIcons/<Class>/<id>.png and check the import, the budget and the on-screen display, then import the remaining 91 per class. Ranger/A06.png and Mage/M07.png overwrite existing files, so check that the .meta guid is unchanged. Extend SkillIconAssets.TryResolve to resolve modifier ids and key passive ids, and add 92 rows (productionApproved=false) to Docs/Art/ClassSkillIcons/manifest.json.
Verification: BuildResourceTests and the texture budget test, the .meta guid check, and a screenshot of the representative icon. Do not run full EditMode or the smoke tests.
Do not: resize, regenerate or remove the background of icons, or delete the source folder.
Report: what you changed, the checks you ran and their results, the effect on texture size (Android, desktop, web), the scope you did not run.
```

## 7. Step 4 — Skill window (UI-1 ~ UI-6)

**Prerequisite**: 1 (and 3 from UI-3 on). **Branch**: `codex/d4-skill-tree-ui-<n>`. Step 4 is **six PRs**. The content, files and done-when criteria are in **UI spec section 13**. This section holds only the shared reading list and the prompt template.

**What to read (all of it in UI-1, then the relevant sections)**: UI spec sections 0~3 → `golden/README.md` → the sections of that package (UI-2: section 5, UI-3: sections 4 and 6, UI-4: sections 7, 8 and 9, UI-5: sections 10 and 11, UI-6: section 14) → `research/ui_infrastructure.md`.

**UI-1** Move Layout, ViewModel, Session, Fx and the test helpers from `golden/csharp` so that the 4 EditMode test kinds (+ units) pass against the golden files.
**UI-2** Drawing tools (`UiMesh` extension, `PolylineGraphic`, `RankArcGraphic`, `TextGradient`, shaders, colour tokens).
**UI-3** Window shell and static scene (24 state screenshots, per-element scores from `compare_screens.py`).
**UI-4** Detail panel and header, overflow smoke test.
**UI-5** Input, commit and effects, click-replay smoke test (60 cases).
**UI-6** Entry point wiring, `visibleState`, parent window refresh, the final visual comparison table.

**Prompt template** (substitute `<n>` and `<section>`)
```
Work in the HELLSCRIPT repository. First read and follow AGENTS.md, AgentRules/Workflow.md, AgentRules/Shared_UI.md, AgentRules/UI_Refresh.md and CLAUDE.md (conversation and records in Korean; code, comments, commit messages and logs in English).
Start branch codex/d4-skill-tree-ui-<n> in a new worktree from the latest origin/main (with the required earlier PRs merged).
Documents to read (in order): Docs/Implementation/D4_Skill_Tree_UI_Spec_20261012.md sections 0~3 and UI-<n> in section 13, the sections of that package (<section>), Docs/Design/D4SkillTree/golden/README.md, research/ui_infrastructure.md.
Task: only the UI-<n> package of UI spec section 13. Do not do the work of other packages.
Verification: the tests and smoke tests listed under "Done when" for UI-<n> in UI spec section 13, python3 tools/check_ui_contract.py --verify, python3 tools/check_ui_refresh.py. For the visual comparison (section 14), attach the 24-image comparison table to the PR in UI-3, UI-4 and UI-6. Do not run full EditMode or the full smoke test suite.
Do not: edit Docs/Design/D4SkillTree/golden, skill_tree_d4.json or SkillWindow.dc.html. Recompute rules or scene rectangles in window code. Scale up for PC with localScale. Write colour hex values in code. Use a stencil Mask per icon. Leave differences outside the allowed approximations (A1~A8).
Report: what you changed, the checks you ran and their results (numbers and the comparison table), the scope you did not run, the differences you left within the allowed approximations and why, and anything that needs a user decision.
```

## 8. Step 5 — Combat (5.0, 5a, 5b, 5c)

**Prerequisite**: 1 and 2 (for 5.0), then 5.0 (for 5a~5c). The content is in **combat spec section 7**. **What to read**: plan section 8 → the combat spec **in full** → `research/combat_<class>.md` (the relevant class) → for 5.0, section A (the framework) of all three classes.

**5.0 Common framework** — all of combat spec section 2: the v5 `modifier` field and `Loadout.Modifier`, `ClassSkillCast.mod/modRank`, connecting `SkillTreeD4Values`, reading rank values, the ultimate's `RankScale` and cooldown, the cleanup rules (2.4), the persistence allow list, and updating fingerprints and Balance1000. Done when: the paired-test framework runs with one grade A modifier, and the existing combat test results are the same as on `main`.

**5a/5b/5c per class** — base skill numbers → modifiers A and A\* → B → C (after the answers to the questions in combat spec section 8 are decided) → passives → key passives → ultimates. Done when: tests for 28 modifiers + 10 passives + the two ultimates (acceptance criterion 2.8).

**Prompt template** (`<step>`, `<class>`)
```
Work in the HELLSCRIPT repository. First read and follow AGENTS.md, AgentRules/Workflow.md and CLAUDE.md (conversation and records in Korean; code, comments, commit messages and logs in English).
Start branch codex/d4-skill-tree-combat-<step> in a new worktree from the latest origin/main, with the prerequisite steps merged.
Documents to read (in order): Docs/Implementation/D4_Skill_Tree_Step_Briefs_20261012.md section 8, D4_Skill_Tree_Plan_20261012.md section 8, D4_Skill_Tree_Combat_Spec_20261012.md in full, Docs/Design/D4SkillTree/research/combat_<class>.md (for 5.0, section A of the three class files).
Task: only <step> of combat spec section 7. For every modifier, write a paired test (without vs with the modifier) and satisfy acceptance criterion 2.8. Implement grade C only after writing into the PR the default, or the user's answer, to the questions in spec section 8.
Verification: simulation tests for the skills and passives you changed, the existing combat suites that are affected, and the forecast = release test. If you edit a fingerprinted file, run tools/class_skills.py check and tools/balance_1000.py again. Do not run full EditMode or the smoke test suite.
Do not: rewrite the point, tier or modifier rank rules in combat code, put modifiers into the use-option system, carry over current bonus effects that are not in the design, claim performance without 3 real before/after binary runs and raw CPU/GC data, edit golden/.
Report: what you changed (the A/A*/B/C classification per skill and the test names), the checks you ran and their results, the scope you did not run, the numbers of the questions where you used the default, and anything that needs a user decision.
```

## 9. Step 6 — Integration

**Prerequisite**: 4 (UI-6), 5. **Branch**: `codex/d4-skill-tree-integration-<n>`. Do not put everything in one PR; split it by area (up to 3 in parallel). **What to read**: plan section 12 → `research/dependents.md` **in full** (especially section G, "15 places most likely to be forgotten").

| Package | Content | Main locations |
|---|---|---|
| 6a Quick edit · HUD | `SkillQuickAvailable` is tied to `ClassSkillTree.Uses` (v3·v4), so it turns off in v5 → a new `Uses`. Rank and modifier in `HudSlotState` and `ClassSkillStatus`. Modifier in names and descriptions (first fix the `A06-2` regex error in `ClassSkillTree.Display`). Rank changes use the same rules engine, and blocked reasons use the same wording. Ultimate "Lv.40" → "T5 open or learned" | `GameUI.SkillQuick.cs`, `HuntEdictQuickMenu`, `GlobalHudSnapshot.cs`, `ClassSkillReadModel.cs`, `ClassSkillDisplay.cs` |
| 6b Tutorials · puzzles | D7: the guide tutorial's first skill is W09, then W01 after 2 points. Prologue W01/A01/M01 → per-class T1 (W09·W08, A07·A14, M07). Puzzle kits are granted through an exception API. Redo F06/H07/H08 on the "points = level, tiers" basis. Update the names of the elements the tutorials point at (`edict-…` → `d4-…`) | `Tutorials*.cs`, `GuideTutorial.cs`, `PuzzleTutorial.cs`, `GameUI.TutorialStaging.cs`, `Resources/PuzzleTutorial.json`, test fixtures `PuzzleAfterL8-*.json` |
| 6c Edicts · auto-battle | Clean up the 25 groups of use options; 211 presets (50 old ids) and `SkillPresetScenarios.json` (122 entries; the invariant is **exactly one scenario per preset**); M05 in the `survival.*Skill` lists; recommended edicts; the exception path for the 30 validation builds | `HuntEdictV2.*`, `EdictOptions.cs`, `HuntEdictQuickPresets.*`, `ClassSkillOptions.cs`, `SkillPresetScenario.cs` |
| 6d Tools · docs · text | The 7 steps of CI `class-skills.yml` and the fingerprints, `balance_1000.py`, `wiki.py` expected counts and `test_wiki.py`, skill documents (Korean and English), old keys in `en.txt` (5286–5336 and others), the sound bank (30 entries in `bank.json`), item and rune `requiredSkill` and `legendaryPowers` → `legacyRemap` | `tools/*.py`, `.github/workflows/class-skills.yml`, `Docs/Design/HELLSCRIPT_*_Skills*.md` |

**Done when**: the group 1~5 references reported by `python3 Docs/Design/D4SkillTree/tools/legacy_id_inventory.py --check` drop to 0 (group 6, the documents, once those documents are rewritten for the new system). The related suites and the generator `check` pass.

**Prompt template**
```
Work in the HELLSCRIPT repository. First read and follow AGENTS.md, AgentRules/Workflow.md and CLAUDE.md.
Start branch codex/d4-skill-tree-integration-<n> in a new worktree from the latest origin/main, with the prerequisite steps merged.
Documents to read: <package> in section 9 of Docs/Implementation/D4_Skill_Tree_Step_Briefs_20261012.md, D4_Skill_Tree_Plan_20261012.md section 12, Docs/Design/D4SkillTree/research/dependents.md in full.
Task: only the <package> in section 9. Move or delete the places that point at old ids as legacyRemap says. Before implementing, recount the targets with grep and legacy_id_inventory.py.
Verification: the related suites, the generator check, python3 Docs/Design/D4SkillTree/tools/legacy_id_inventory.py (report the remaining references). Do not run full EditMode or the smoke test suite.
Do not: write the rules separately, rename save keys, widen check lists just to pass, delete the old screens (that is step 7's work).
Report: what you changed, the checks you ran and their results, the number of remaining old id references, the scope you did not run, and anything that needs a user decision.
```

## 10. Step 7 — Switch-over

**Prerequisite**: 1~6. **Branch**: `codex/d4-skill-tree-switch`.

**Work**: switch the default of `SkillTreeD4.Enabled` on. Clean up the old tree screen and data paths (the tree and inspector in `HuntEdictWindow.Skills.cs`, the old layout in `SkillTreeGraphic`, `SkillTreeLayout`, `Prototypes/SkillTree/*`, `RuntimeSkillTreeSmoke*.cs`, the old `ClassSkillTree.json` and art, `GameStore.CommitClassSkills` which has no callers, and so on: `research/dependents.md` section A, "Delete"). After that, **integrate `main`**, then run **1 full EditMode run + 1 macOS development build smoke test run** (the new `-hellscriptSkillTreeD4Smoke` and the existing smoke tests this change touches: SkillTree, ClassSkill, BattleQuickEdit, Tutorial, GuideTutorial, puzzles). If batch mode changes `ProjectSettings.asset` or `UnityConnectSettings.asset`, restore only the changes made by this run. After the merge, publish the wiki (`AgentRules/Wiki_Publish.md`).

**Done when**: all of plan section 13. List separately the screens you could not check and the scope not verified on physical devices.

**Prompt**
```
Work in the HELLSCRIPT repository. First read and follow AGENTS.md, AgentRules/Workflow.md, AgentRules/Wiki_Publish.md, AgentRules/Artifact_Cleanup.md and CLAUDE.md.
Start branch codex/d4-skill-tree-switch in a new worktree from the latest origin/main, with steps 1~6 merged.
Documents to read: Docs/Implementation/D4_Skill_Tree_Step_Briefs_20261012.md section 10, D4_Skill_Tree_Plan_20261012.md sections 13 and 14, the "Delete" list in section A of research/dependents.md.
Task: step 7. Switch SkillTreeD4.Enabled on by default, clean up the old tree screen and data paths, integrate main, run 1 full EditMode run + 1 macOS development build smoke test run (including -hellscriptSkillTreeD4Smoke), and publish the wiki after the merge.
Verification: all of plan section 13. Compare with the main results under the same conditions and report only new failures as new failures.
Do not: reuse the full-check results from before the last code change, or report macOS results as physical-device results.
Report: a results table, the screens you could not check, the scope not verified on physical devices, and the retention and cleanup state of the outputs.
```
