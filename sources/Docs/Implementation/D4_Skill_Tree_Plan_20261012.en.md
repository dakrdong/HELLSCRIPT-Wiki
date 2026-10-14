# Diablo 4-Style Skill Tree Development Plan (for Codex)

Updated: 2026-10-12 · For: Codex implementation work · [한국어](D4_Skill_Tree_Plan_20261012.md)

Author: Claude (plan and design). Codex implements, verifies and commits.

**Status: final (2026-10-12).** This document and the documents and material below are the reference for implementation. Reading order: this document → [step briefs](D4_Skill_Tree_Step_Briefs_20261012.en.md) (the starting point and prompt for each step) → for your area, the [screen spec](D4_Skill_Tree_UI_Spec_20261012.en.md) or the [combat spec](D4_Skill_Tree_Combat_Spec_20261012.en.md).

## 1. Goal

- Replace the current class skill tree with a Diablo 4-style skill tree. The current tree has 37 nodes per class, unlocks by level, joins nodes by lineage, and has a level cap of 40.
- Build the design's skill window in the game with **the same look and the same behaviour**. It includes:
  - class tabs
  - 5 tiers
  - active cards, each with 3 modifier sockets
  - silver passive medallions and gold hexagon key passives
  - link lines
  - invest and refund buttons in the detail panel
  - one-shot effects
- Bring the point, tier, modifier and passive rules and the combat effects into the actual game. The combat effects are 84 modifiers, 30 passives and ultimates with 3 ranks.
- `main` must keep working after every step. The new system stays behind a flag until the final switch-over step (section 14).

## 2. Reference material

**Design canvas:** https://claude.ai/artifact/4KCzKcxqrBBWrNiJZ5Esg8
- One row per class, in this order:
  - portrait Korean and English
  - portrait with a passive selected, Korean and English
  - landscape Korean and English
  - PC Korean and English
- Play mode lets you try tabs, selection, invest and refund directly.
- The link is private. To open it from a Codex account, the user must share it from the Share menu. If it cannot be opened, the repository copies below are enough.

**Repository copies (in the same change as this document):**

`Docs/Design/D4SkillTree/skill_tree_d4.json` is **the source of truth for the design data**. It contains:
- the rules
- per class: tiers, 10 actives (including 2 ultimates), 28 modifiers and 10 passives
- a one-line description per active (`descKo`/`descEn`)
- rank values (`rank1`/`rankMax`, and the parsed `value`)
- icon ids
- per modifier, a shape-change sentence (`shapeKo`/`shapeEn`, the player-facing wording) and the original design sentence (`behaviourKo`, Korean, for reference)
- the absorbed legacy skill (`absorbsLegacyId`)
- the legacy id map (`legacyRemap`)
- the screen text (`strings`)
- The `designSample` values are the example allocation used in the design (a level 24 hero with 24 points). They are not game data.

`Docs/Design/D4SkillTree/SkillWindow.dc.html` is **the design's source code**.
- Coordinates are computed in `renderVals()`: landscape and PC at lines 693–775, portrait at 776–848, header and plate at 866–908, the detail panel at 916–1013.
- The active skill descriptions (Korean and English) are in `DESC`, from line 362. The modifier shape sentences are the 5th and 6th values of each modifier entry in `DATA` (the first 4 are the names and the numeric effect).
- It also holds the colours, gradients, line state rules, the rules engine `apply()` (lines 537–567) and the effect `@keyframes` (lines 12–97).
- Port the screen layout from the numbers in this file.

`Docs/Design/D4SkillTree/extract.js` and `behaviour.json` are the export script that builds the spec JSON from the design source, and its input.
- Run: `node extract.js SkillWindow.dc.html skill_tree_d4.json behaviour.json`
- Run on the copy in this repository, it produces exactly the same result as `skill_tree_d4.json`.
- The legacy id map (`legacyRemap`) is built here as well.
- `behaviour.json` holds the modifiers' shape-change sentences (Korean).

`Docs/Design/D4SkillTree/reference/<Class>-<Layout>-<KO|EN>.jpg` are 24 renders at 2×.
- `Layout` is one of `Portrait`, `Portrait-Passive`, `Landscape`, `PC`.
- Sizes are 880×1912 portrait, 1912×880 landscape and 2880×1620 PC.

**Verification material and implementation specs (in the same change as this document):**
- `Docs/Design/D4SkillTree/golden/`: **golden files** obtained by actually running the design (rules, layout, wording, interaction, internal dimensions, look, effect signals), an easy-to-read **JavaScript reference implementation**, and a **C# port** (`golden/csharp`) that passes the same checks. `cd Docs/Design/D4SkillTree/golden/csharp && dotnet run` runs **55,803 checks in about 0.5 seconds** without Unity, and all of them match. The guide is `golden/README.md`.
- [`D4_Skill_Tree_UI_Spec_20261012.md`](D4_Skill_Tree_UI_Spec_20261012.en.md): the screen implementation spec (6 fidelity layers, units, hierarchy, drawing tools, looks, text, panel, effects, work packages UI-1 to UI-6, visual comparison).
- [`D4_Skill_Tree_Combat_Spec_20261012.md`](D4_Skill_Tree_Combat_Spec_20261012.en.md): the combat implementation spec (common frame, classification of the 84 modifiers, passives and key passives, ultimates, questions and defaults).
- [`D4_Skill_Tree_Step_Briefs_20261012.md`](D4_Skill_Tree_Step_Briefs_20261012.en.md): the step briefs and the prompts to paste into Codex.
- `Docs/Design/D4SkillTree/research/`: pre-implementation code investigation notes (read-only): screen infrastructure, per-class combat, save and migration, dependents.
- `Docs/Design/D4SkillTree/icons_manifest.json`: the original name, sha256 and import location of each of the 92 new icons (including the two that overwrite existing files).
- `Docs/Design/D4SkillTree/tools/legacy_id_inventory.py`: the list of places that point at ids that disappear, and `--check`.

**92 source icons:** `~/HellscriptOverhaulWork/work/d4skills/{Warrior,Ranger,Mage}/out/*.png`
- 1254×1254 RGBA, Codex `image_gen` output kept byte for byte.
- `manifest.json` in the same folder records the prompt, original path and sha256.
- The 3 Mage redraws (MK1, M18-1, M18-2) are already in `Mage/out`.
- This folder is outside the repository, so do not delete it before step 3 imports it.

## 3. Confirmed rules (approved by the user)

| Item | Rule |
|---|---|
| Actives | 8 per class, max Lv5. Lv1, the rank you learn first, also costs 1 point. |
| Modifiers | 3 per active, 2 per ultimate. Points can go into **only 1** modifier per skill. The base skill must be Lv1 or higher, and a modifier's level cannot exceed the base skill's level. To switch to another modifier, refund the current one down to 0. |
| Ultimates | 2 per class; the base and its modifiers all have max Lv3. You can learn both, but only 1 can be equipped. |
| Passives | 8 regular + 2 key per class, max Lv5. **Only hero level** is checked (no tier condition, no prerequisite). Points can go into only 1 of the 2 key passives. Invested passives need no equip slot and always apply. |
| Points | 1 point per level from hero level 1; at the level 60 cap that is 60 points. Maxing out one build (actives 4×5, modifiers 4×5, an ultimate and its modifier 3+3, passives 3×5 and a key passive 5) takes 66 points, so it was **deliberately set 6 short**. Passives have no equip slots, so all 8 regular passives and 1 key passive can be invested in, which makes the maximum for one class 137 points. With 60 points you have to choose what to raise. |
| Tiers | T1–T5. To invest in a tier's skill, the **points spent in lower tiers and in passives** must be at least T1 0, T2 2, T3 6, T4 11, T5 40. Passive points also count toward every tier condition. A refund that would break any tier condition is refused. |
| Loadout | As today: 4 active slots and 1 ultimate slot. Only Lv1 or higher can be equipped. Modifiers have no slots; they ride on their base skill. |
| Passive-to-skill lines | A line is drawn only when a passive names a specific skill directly (`linkedActives`). There are no lines between passives. The passives concerned: Warrior Battle Reserve → Crushing Blow and Ground Slam, Blood Frenzy → Raking Wound / Ranger Marked Opening → Hunter's Mark, Decoy Tactics → Retreat Leap / Mage Overload → Chain Lightning. |

## 4. What changes from the current code

| Area | Now | After |
|---|---|---|
| Skill count | 37 per class: 16 actives, 19 passives, 2 ultimates. `ClassSkills.Data` validates 111 (`ClassSkills.cs:52-56`). | Per class: 10 base + 28 modifiers + 10 passives = 48 slots, 144 for the 3 classes |
| Unlocking | Level and lineage (`level/all/oneOf/lineage` in `ClassSkillTree.json`); ultimates need a Lv30–39 node | Tier unlock conditions (points spent); passives by level only |
| Points | `ClassSkillTree.Remaining` = level − (v4 ? 0 : 1) − Spent | Hero level − points spent (same as the v4 rule) |
| Level cap | 40 (`ClassSkills.LevelCap`, `GameStore.cs:64`, `ClassSkillTree.cs:42`, `Tutorials.cs:214`, `RiftEntryWindow.cs:338`, `TrainingGroundWindow.Skills.cs:42`, `SkillTreeLayout.BandStarts`) | 60 |
| Ultimate ranks | 1 (`RankScale` is always 1) | 3 (+20% per rank, cooldown 60/55/50s) |
| Modifiers | None. `ClassSkillOptions` are use policies, not effect variants. | 1 chosen per skill, with ranks |
| Passive slots | Already none on the tree (`ClassSkills.cs:120`). | Still none |
| Screen | Tree, inspector and skill bar in `HuntEdictWindow.Skills.cs` | New skill window (section 9). The edict window's skills tab keeps only the loadout bar (section 17 D1). |
| Legacy 18-slot layer | Legacy actives 0..17, 6-slot passive arrays, `ProjectLegacy`/`LegacyIndex` bridge | Keep the bridge for surviving ids only; move removed ids through `legacyRemap`. |

## 5. Data and IDs

### 5.1 ID rules

- Surviving skills keep their existing ids (W01, WP13…). Their names stay the same too.
- **No new base skills are created.** The two skills that have a predecessor keep the old ids: Elemental Imbuement = **A06** (the old Shadow Arrow), Elemental Lance = **M07** (the old Ember Lance). The old behaviour of each becomes modifier number 1 (`A06-1` Shadow Imbuement, `M07-1` Ember Lance).
  - Why: the A06 code (about 570 references) and the M07 code (about 350) need no renaming to new ids, and the Ranger's A06 slot in the old 6-slot layer (`x01–x06`) does not become empty.
- Key passives are **WP20/WP21, AP20/AP21, MP20/MP21**.
- Modifier ids have the form `<base skill id>-<1|2|3>` (e.g. `W01-3` Rapid Whirl). In the save, a modifier is stored on its base skill as (selection number, level). The id is used only for icons, text and edict display.
- The source icon file names are the JSON `iconId` (`A_IMBUE`, `M_LANCE`, `WK1`…). When importing into the game, rename each file to its `id` and keep the contents byte for byte. Record the original name in the manifest. `A_IMBUE`→`A06.png` and `M_LANCE`→`M07.png` overwrite the old icon files of the same name (the old art stays in git history).
- Check that id regexes such as the one in `ClassSkillTree.Display` recognise the `A06-2` and `WP20` forms.

### 5.2 Legacy ID map (`legacyRemap`)

| Class | Kept | Absorbed as a modifier | Other | Passives removed |
|---|---|---|---|---|
| Warrior | W01–W06, W08, W09, W17, W18 / WP01, 04, 05, 06, 09, 13, 15, 17 | W07→W08-3, W10→W09-1, W11→W02-1, W12→W05-1, W13→W04-1, W14→W04-2, W15→W06-3, W16→W05-2 | — | WP02, 03, 07, 08, 10, 11, 12, 14, 16, 18, 19 |
| Ranger | A01–A05, A07, A14, A17, A18 / AP01, 05, 06, 07, 09, 10, 14, 15 | A08→A05-1, A09→A03-1, A10→A03-2, A11→A04-1, A12→A04-2, A13→A06-2, A15→A03-3, A16→A05-2 | A06 stays as the base skill and becomes Elemental Imbuement (its old dark bonus damage = A06-1) | AP02, 03, 04, 08, 11, 12, 13, 16, 17, 18, 19 |
| Mage | M01–M04, M06, M08, M11, M17, M18 / MP05, 06, 07, 09, 11, 14, 16, 17 | M05→M04-3, M10→M02-3, M14→M06-2, M16→M03-3, M09→M07-2, M12→M07-3 | M07 stays as the base skill and becomes Elemental Lance (the old Ember Lance behaviour = M07-1). M13 Mana Reclaim removed (it conflicts with auto-movement), M15 Elemental Compass removed | MP01, 02, 03, 04, 08, 10, 12, 13, 15, 18, 19 |

- The ultimate-only passives (WP18/19, AP18/19, MP18/19) are removed.
- Items, legendary powers, runes, sets, edict presets and puzzle kits that point at removed ids are moved with this table (section 12).

### 5.3 Data pipeline

- **Source:** `Docs/Design/D4SkillTree/skill_tree_d4.json`. It is exported from the design with `extract.js`, and once implementation starts this JSON is the game's reference.
  - When balance changes a number, edit this JSON. If the numbers disagree with the design, follow this JSON.
  - If you need to change the structure, rules or wording, tell the user so that the design is fixed as well.
  - If the design changes later, Claude re-exports it and merges only the differences without overwriting.
- **Generator:** create a new `tools/build_skill_tree_d4.py build|check`.
  - Output: `Assets/HELLSCRIPT/Resources/Data/SkillTreeD4.json`.
  - Output contents: per-class tiers, skill ids, tier, max rank, **per-rank value arrays** (5.4), modifier value arrays, passive unlock levels, key flags, link targets, Korean and English names and effect text, icon ids.
  - `check` compares the output with the source. It works the same way as the existing `class_skill_runtime.py` and `build_skill_tree.cjs`.
- **Runtime:** put the data loading and the rules engine (section 6) in `Runtime/Core/SkillTreeD4/`. The model and rules start as a copy of the **verified C# port** (`golden/csharp/SkillTreeD4.Model.cs`, `.Rules.cs`). The window, the edict quick edit and combat all use this one engine only.
- **What the generator leaves out** of `SkillTreeD4.json`: `designSample`, `designSelection`, `behaviourKo` (design only). **What it puts in** is extended with `legacyRemap` (used by the migration) and the screen text `strings`.

### 5.4 Per-rank values

- **Active stats:** interpolate linearly from `rank1` at Lv1 to `rankMax` at the max rank.
  - Damage % gives the same result as the existing `SkillEffects.Power` +10% per rank (D50% → D70% = ×1.4).
- **Integer counts** (targets, times, shots, etc.):
  - If the difference between the first and last value is 2, it steps +1 at Lv3 and Lv5. Scatter Shot 3→5, Ricochet 1→3, Chain Mark 1→3, Drawing Again 4→6, Overload Chain 6→8, Rebound 2→4.
  - If the difference is 4, it steps +1 per rank. Piercing Hook 3→7, Crossbow Turret 6→10.
- **Ultimates and ultimate modifiers:** linear over Lv1/2/3. Example: War of the Ancestors D65/78/91%, cooldown 60/55/50s.
- **Passives:** linear over Lv1→Lv5.
- **Modifiers:** the modifier level changes only the modifier's own effect values. The skill's damage multiplier follows the base skill rank.
- **`value` units:** `D%` relative to weapon damage, `x` multiplier, `%p` percentage points, `초` seconds, `m`, `m/s`, `명` targets, `회` times, `발` shots. A leading `−` means a decrease.
- **Some stats do not change.** Example: Iron Wall cooldown 14→14s. Keep the display format.

## 6. Rules engine

Port the design's `apply()` as is. **Verified code with the same content already exists in `golden/csharp/SkillTreeD4.Rules.cs`** (32,208 rule golden checks match), and what follows is that code written out in words. The test `SkillTreeD4RulesTests` reads `golden/rules_golden.json`.

- `tierPts[t]` is the sum of (base rank + modifier rank) over the actives of tier t.
- `spent` is the sum of tierPts plus the sum of passive ranks.
- `before(t)` is spent minus tierPts[t..4].
- `left` is hero level − spent.

**Invest (active base):**
1. If `before(tier) < gate`, block with `tier`, and report how many points are missing.
2. If `rank >= max`, block with `max`.
3. If `left == 0`, block with `pts`.
4. If it passes, raise the rank by 1.

**Invest (modifier m):**
1. The tier check is the same as for the base.
2. If base rank < 1, block with `base`.
3. If another modifier is already chosen, block with `swap`.
4. If the chosen modifier's rank has reached min(max, base rank), block with `max` or `cap`.
5. If `left == 0`, block with `pts`.
6. If it passes, choose this modifier and set it to Lv1 when nothing has been chosen yet. If it is the modifier already chosen, raise it by 1.

**Invest (passive):**
1. If hero level < unlock level, block with `lv` and report the required level.
2. If rank >= 5, block with `max`.
3. If it is a key passive and the other key passive holds points, block with `key`.
4. If `left == 0`, block with `pts`.
5. If it passes, raise it by 1.

**Refund:**
- Base: lower by 1 when rank > 0 and rank > the modifier rank.
- Modifier: lower by 1 when it is the chosen one and its rank > 0. When it reaches 0, the choice is cleared.
- Passive: lower by 1 when rank > 0.
- In all three cases, after the refund, if not every tier keeps `tierPts[t] > 0 → before(t) >= gate[t]`, the refund is rolled back.

**Tier shown as open:** `before(t) >= gate[t] || tierPts[t] > 0`.

**Blocked-reason text** (shown on the invest button: small text under the button in portrait; in landscape and PC the button label changes):

| Reason | Korean | English |
|---|---|---|
| max | 최대 레벨 | Max rank |
| tier | {n}pt 더 필요 | {n} more pt / {n} more pts |
| base | 스킬 Lv1 먼저 | Skill Lv1 first |
| swap | 마을에서 교체 | Swap in town |
| cap | 스킬 Lv 한도 | Skill Lv cap |
| key | 핵심 1종만 | One key only |
| lv | Lv{n} 필요 | Requires Lv{n} |

`pts` (no points left) shows no text. The header already shows the points left as 0. The invest button is then disabled and its label stays "Invest". The rules engine still returns `pts` (for disabling the button and for tests).
**Tests:** port the rule tests of the design's checker script to EditMode `SkillTreeD4RulesTests`:
- In the level 24, 24-point allocation: refund Whirlwind → Lv4, invest again → Lv5
- While Rapid Whirl is chosen, the other modifiers are blocked
- Fully refunding Rapid Whirl clears the choice, after which Swift Whirl can be chosen
- Blood Recovery (Lv15) can be invested in; Battle Reserve (Lv25) is blocked
- With only 2 points in T1, Iron Wall (T3) is blocked and Whirlwind (T2) is allowed
- A refund that lowers Raking Wound to 1 while it holds up Whirlwind is refused
- Only one key passive is allowed
- Switching tabs keeps each class's allocation

## 7. Save and migration

The basis is the read-only investigation `research/save_migration.md`. **The order and algorithm are in its section D, and the test method in its section E.** The brief for this work is section 5 of the step briefs.

- **Allocation save:** add version 5 to `ClassSkillLoadout`.
  - Contents: per class, 20 `ranks` = 10 actives (8 regular + 2 ultimates) + 10 passives (8 regular + 2 key). Every entry is a `SkillInvestment {id, rank, modifier(-1..2), modifierRank}`, and passive entries keep `modifier = -1` and `modifierRank = 0` and do not use them.
  - Do not rename existing field names (save keys); add new fields. `JsonUtility` keeps a field's initial value for a missing key, so `modifier = -1` is the right default (0 means "modifier 1 chosen").
  - `JsonUtility` never restores null. An empty array can read as an "old save", so tell them apart with the existing `IsAbsent` approach.
  - In v5, `Rank(id)` returns **0 for an id that is missing** (today it returns 1). `Canonical` **preserves the version** (hundreds of tests use v2 verification heroes). The conversion is done at account level.
  - **v5 load validation:** `ranks` has **10+10 = 20 entries** per class (in the order of `skill_tree_d4.json`: 10 actives = 8 regular + 2 ultimates, then 10 passives = 8 regular + 2 key). Every entry is the same `SkillInvestment`, and passive entries have `modifier == -1` and `modifierRank == 0`. Also: rank ≤ max (5, ultimate 3), `modifierRank == 0` only when `modifier == -1`, `modifierRank ≤ min(max, rank)`, when a passive rank > 0 its unlock level ≤ hero level and only one key passive, `Spent ≤ level`, tier gates by `SkillTreeD4Rules.Valid`, an equipped skill has rank > 0 and slot 4 is an ultimate, level ≤ 60.
- **Migration v3/v4/none → v5:**
  - The default is **refund everything** (D4). Empty the loadout slots, because skills that have not been learned cannot be equipped.
  - **The migration goes in `GameStore.Read()`, before `Normalize`.** A change made after loading (`ActivateSkillTrees`) is caught by the edict-boundary validation of `Write()` as a "player edit", and the start can fail.
  - A pure function `SkillTreeD4Migration.Apply(AccountSave)` (public static, no I/O, returns a report) is called by `Read`, `GameStore.Sandbox` and the tests. It must be **idempotent** and skips v5 heroes.
  - For each hero: ① read the old allocation ② refund = the whole hero level ③ an empty v5 allocation (20 `ranks` = 10 actives + 10 passives, all rank 0 and `modifier = -1`, loadout, ultimate and passive slots empty, `order = ["BASIC"]`) ④ remove the groups of removed ids from `options` and `presetSelections` (options 25, scopes 24) ⑤ clean up the M05 slot and the survival list in the edict document ⑥ re-project the legacy 12-slot layer ⑦ presets (`presets`, `edictPresets`) keep only the policy ⑧ clean up training records, comparisons, puzzle kit copies and guide records ⑨ keep the original ⑩ notice.
  - Before migrating, copy the original to `.schema{n}-before-d4-skill-tree.json` (`Read()` already has a precedent).
  - Wrap failures in `NotSupportedException`. `Read` swallows other exceptions as null, and **an older generation of `.bak` is silently read.**
  - Show a notice once, on first open: "스킬 체계가 바뀌어 포인트를 모두 돌려받았습니다." / "The skill system changed, so all points were refunded." Save it per hero and acknowledge it on the first skill window open (section 17 D19).
  - **Runs in progress** (interrupted runs, portals, tutorial runs): section 17 D13. **Old share codes**: D14. The lifetime of the old catalogue: D15.
- **Schema:** `GameStore.MaximumSchemaVersion` 21 → 22. Old binaries reject 22. How it relates to the flag is in section 17 D18 (only when it is on).
- **Level cap 60:**
  - Keep the `Economy.XpRequired` formula and only raise the cap (D9). The XP for levels 41–60 is a closed-form expression, so no table is needed.
  - Places with a level cap: `ClassSkills.LevelCap` (legacy 30, otherwise 40), the raw 40 at `GameStore.cs:64`, `ClassSkills.cs:137` and `:147`, `ClassSkillTree.cs:42` (cache key), and every hard-coded 40 in section 4.
  - Fixtures **fixed** at level 40 (`CombatSimulation.SkillPreview.cs:31`, `ClassSkillValidation.cs:33`, `HuntEdictWindow.CombatPreview.cs:90`, `FullSkillTraining`) are detached from `LevelCap` and given an explicit constant.
  - **A hero at level 40 or below cannot learn an ultimate.** The T5 gate is "40 points outside T5" and points = level, so the first ultimate point requires level 41 (D17). Change the "Lv.40" wording and gates (`RiftEntryWindow.cs:338,345,349,356`, `TrainingGroundWindow.Skills.cs:42`, `Tutorials.cs:56,214`, `en.txt:5758,5837`) to "T5 open or learned" and 41.
  - Review the balance of the 41–60 range separately with Balance1000. If you edit a fingerprinted file (`GameStore.cs`, `ClassSkills.cs`, `Economy.cs` …), run `python3 tools/balance_1000.py` again to refresh the outputs.
- **Run state:**
  - Keep the chosen modifiers and their ranks in the run snapshot (`RunState.classSkills`) as well, so that the same effects appear after resuming.
  - Adjust the `ClassSkillPersistence` validation to the new fields and the new persistent effects (allowed id list: combat spec 2.6).
- **Share codes:** once the new fields exist, no old HED4/HED5 code can be decoded, because `ClassSkillCodec` compares canonical-form JSON. Issue v5 with a new prefix, and either import only the policy from old codes or reject them (D14).
- **Server:** `server/combat_analytics.sql` reads `classSkills`. It must read both the old shape (37 entries) and the new shape (20 entries).
- **Account vs device:** skill allocation is account save data (`hellscript-local-v1.json`). Do not mix it with device settings.

## 8. Combat

**The details of implementation are set by the [combat spec](D4_Skill_Tree_Combat_Spec_20261012.en.md).** It holds the common frame (two engines, the `ClassSkillCast` extension, cleanup rules, test frame), the classification of the 84 modifiers (A 7, A\* 4, B 58, C 15) and where each is implemented, passives and key passives, ultimates, and the questions that cannot be decided from code alone, with their defaults. This section is a summary.

### 8.1 Base skills

- Surviving ids reuse their existing effect code. Only the values in the stat table switch to per-rank values.
- **A06 Elemental Imbuement:** built directly on the existing A06 Shadow Arrow code. The base effect is "next 3 shots deal +25%→+35% bonus damage". The modifier decides the element and the extra effect (shadow burst / 4s poison / frost slow).
- **M07 Elemental Lance:** the shared projectile of the existing M07 Ember Lance, M09 Glacier Lance and M12 Grounding Lance. The modifier decides the element. Ember Lance is fire with a 3-target pierce plus burn, Glacier Lance is cold slow→freeze, and Grounding Lance is a 12m lightning pierce with a Chain Lightning relay point. The element when no modifier is chosen is in section 17 D8.

### 8.2 84 modifiers

- Implement each modifier from `shapeKo/shapeEn` (how the form changes) and `value` (per-rank values).
- `behaviourKo` is the original design sentence and its numbers may differ from `effectKo/value`. Where the numbers differ, follow `effectKo/value`.
- The 20 modifiers with `absorbsLegacyId` reuse the effect code of the absorbed legacy skill. Example: W10 Blood Reclaim → Raking Wound's Blood Reclaim.
- Give every modifier at least one deterministic combat-simulation EditMode test. With the same seed, check that the difference between no modifier and the modifier matches the text.

### 8.3 30 passives

- Surviving ids (8 per class) keep their existing mechanism, with numbers and conditions matched to the new `effectKo`.
- The 6 key passives are new.

| Class | Key passives |
|---|---|
| Warrior | Blood Frenzy: bleeding can crit; killing a bleeding target causes an explosion of D40%→D80% / Unending Resolve: alternating skill types gives +6%→+10% damage, 3 stacks |
| Ranger | Precision: every 3 crits, the next Core skill is a guaranteed crit, +30%→+50% / Viper's Blessing: poisoned enemies take +10%→+18% damage |
| Mage | Shatter: when freeze ends, an explosion of 30%→50% of the damage taken / Align the Elements: enemies hit by all three elements collapse for D60%→D100% |

### 8.4 Ultimates

- Rank 3; the effect is +20% per rank; the cooldown is 60/55/50s.
- Keep the shared ultimate cooldown and the 45s minimum rule (`CS.ultimateRemaining`).
- The 2 ultimate modifiers work the same way as in 8.2.

### 8.5 Auto-battle cautions

- **Swift Whirl, Trailing Blizzard:** their effect depends on the hero's movement path. Check them together with the movement rules of the hunt edict.
- **Gaze Shot:** aims for 0.9s. Stop auto-movement while aiming, and cancel if interrupted.
- **Decoy Projection, Crossbow Turret, Capacitor Orb (3-way split, Triple Orb):** decide the cap on the number of placed objects and when they expire.
- **Damage interval ×0.5 (Rapid Whirl):** the number of ticks increases. Check combat tick performance. A performance claim needs raw CPU and GC data from running the real binaries 3 times each before and after.

### 8.6 Rank bonuses

The scope of gear +1 rank (`MaximumEffectiveRank` 6) and rune skill levels is in section 17 D5.

## 9. Screen

**The details of implementation are set by the [screen spec](D4_Skill_Tree_UI_Spec_20261012.en.md).** It holds the 6 fidelity layers (L0 rules … L5 look), units and hierarchy, drawing tool extensions, the look catalogue, text, the detail panel, effects, work packages UI-1 to UI-6, and the procedure and pass limits for visual comparison. **If they conflict, the screen spec takes priority.** This section is a summary.

### 9.1 Window and entry points

- Add a new adapter window `SkillTreeD4Window` (`Runtime/Presentation/SkillTreeD4Window*.cs`).
  - It is an adapter instead of `ContentWindowView` because it is a special layout that uses the design's coordinates as they are. The precedent is the forge's HTML unit U (`BlacksmithWindow.cs:67`).
  - Register it in `ADAPTERS` in `tools/check_ui_contract.py` together with its reason. The required tokens are what this window really uses: `ContentWindowHost.Attach`, `SkillTreeD4Units`, `UiSafeArea.Current` (this window does not use `UiTheme.Scale`, so do not call it just to satisfy the token: screen spec 3.2).
  - Use `ContentWindowHost.Attach` for the window stack, Back and pause.
  - Give `StoreViewBinding.Attach` a `visibleState` key, as `tools/check_ui_refresh.py` requires.
- **Switch only the calls that open the skill tree page directly to the new window.** Today those calls are `GameUI.ShowEdictSkillTree()` and `HuntEdictWindow.SelectSkillPage(true)`.
  - Callers: `RiftEntryWindow.cs:355` (empty skill slot), `GameUI.FirstPlay.cs:22`, `HuntEdictWindow.SkillPresets.cs:59` (the "back to skill tree" button) and `HuntEdictWindow.cs:60` (Back handling).
  - Find the calls again with `grep` before implementing.
- The content dock, battle observation menu, puzzle hub, training ground, rift victory, tutorials and similar entries open the **whole edict window** through `GameUI.ShowEdictEditor()`. Leave these entry points as they are. The new skill window is reached from the "Skill tree" button on the edict window's "skills" tab.
- The edict window's "skills" tab keeps the loadout bar (4+1) and the attack order. A "Skill tree" button goes where the tree and the inspector were (section 17 D1).

### 9.2 Coordinate system

- The design has three frames.
  - portrait 440×956
  - landscape 956×440
  - PC: a 1152×648 stage shown at ×1.25 as 1440×810
- Portrait and landscape are iPhone 18 Pro Max points and include the safe area. Portrait has 62 at the top and 34 at the bottom; landscape has 62 on each side and 21 at the bottom.
- **Unit:** fit the design's safe-area-inner rectangle into the real `UiSafeArea.Current` at the same ratio (fit), and centre the leftover space. The formula and test vectors are in screen spec 3.1. **The canvas unit is half a design point**, so half-point font sizes become integers.
  - The design's safe-area-inner rectangle is (0,62)–(440,922) in portrait and (62,0)–(894,419) in landscape.
  - On an iPhone 18 Pro Max it is 1:1 with the design.
  - On PC (desktop, 16:9 / 16:10 / 21:9), fit the 1152×648 stage to the screen and centre it.
- **Area sizes (design values, pt):** portrait: tree plate (8,164)–(432,570), detail panel (8,578)–(432,922). Landscape: tree plate (62,62) 548×357, detail panel (618,62) 276×357. PC, on the 1152×648 stage: tree plate (24,64) 856×564, detail panel (888,64) 240×564.
- **Choosing the layout:**
  - On mobile, portrait when the safe area is taller than it is wide, otherwise landscape.
  - On desktop, PC.
  - Re-layout (`Reflow`) when the language or screen orientation changes.
- **Layout function:** the layout parts of `renderVals()` (landscape 693–775, portrait 776–848, header 866–908) have **already been ported** as a C# **pure function**: `golden/csharp/SkillTreeD4.Layout.cs` (plus the view model `.ViewModel.cs`, the state machine `.Session.cs` and the effect signals `.Fx.cs`). It matches all of the golden files' 18,009 layout checks (2,975 from the design sample + 15,034 from scenarios), 600 view model checks and 4,986 interaction checks. The inputs are data, allocation, selection and language, and the outputs are lists of rectangles, lines and text.
  - Check it in EditMode under the same conditions as the design's checks: nothing out of bounds, no node overlaps, no line passing through a node (passive shelf rails `pr` excepted).
  - The targets are 3 classes × 2 languages × 3 layouts, plus the states with every passive selected one at a time.

### 9.3 Components

| Element | Per the design |
|---|---|
| Title "스킬/Skills" | Serif 27/24/26pt, gold gradient text |
| Class tabs | 44pt high. The selected tab has an ember gradient, a gold border and a gold diamond below. Classes with points left show an ember dot at the top right. |
| Header stats | Level, Spent n/60, points-left orb. Above 0 the ember orb pulses on a 6.4s cycle; at 0 it is a dark orb. **The three values and the passive unlocks are the values of the hero on the tab.** The design is pinned to a single level, 24. |
| Close | 44pt circle, gold border |
| Tree plate | Obsidian plate, gold border, gold L-shaped corner ornaments (top in portrait, left in landscape). Legend strip at the bottom: Invested / Available / Locked / Passive link. |
| Tier badges | Roman numeral in a hexagon medallion, name and condition ("2+ pts spent"). The next locked tier shows a red "{n} more pts" and a progress bar below. Only portrait shows a lock icon. |
| Active cards | States l (invested, gold), o (available, brown), k (locked, dashed). Icon socket, 2pt rank bar along the bottom. When investable, a ghost bar for the next rank and an inner ember glow. Rank pill n/max (inside the card in portrait, top right of the card in landscape). |
| Modifier sockets | Chosen: gold border, ember fill, level chip at the bottom right. Investable: ember border. Locked: dashed. Under the base skill (landscape) or in a tray to the right (portrait). Hit area 44pt in portrait, 32pt in landscape. **The icons must stay recognisable even when the active is not learned yet (Lv0) or its tier is locked.** Lv0 active in an open tier: dashed border `#6a5a40`, icon opacity 0.78, saturation 80%, brightness 1.1×. Active in a locked tier: dashed border `#3f372c`, opacity 0.45, saturation 45%. |
| Passives | Silver coin medallions with a 5-segment rank arc ring (lit segments `#eaf6ff`). Key passives are gold hexagons. In portrait, capsule rows (name, "Unlocked Lv2 · 3 / 5") sit inside the tier that matches the unlock level. In landscape and PC, 3 per row on a silver shelf under the tier column, with a "Lv N" mark under each medallion and a points mark at the top right. |
| Lines | Gold = invested path, brown = available, dashed = locked, silver arrow = passive link. A soft glow line is laid under the gold and silver lines, and a black groove under the brown ones. |
| Detail panel | Blurred skill art backdrop, medallion, name (serif), "T2 Core · Active", "Lv 5 / 5 · values Lv1 → Lv5". **Actives have a description slot under that, above the stats divider line.** When the base skill is selected it shows the skill description (for example Whirlwind — "Spin with your weapons, striking every enemy around you."); when a modifier is selected (by tapping its socket in the tree or its row in the panel) it shows **modifier name (gold, bold) — shape-change sentence** (for example "Lunging Hook — Instead of pulling the enemy, you fly to it."). `#c8bfae` text, 12pt in portrait and 10.5pt in landscape, wrapped at word boundaries. So that the panel does not move when the selection changes, the height of 2 lines in portrait, 2 lines in landscape and 3 lines on PC is always left empty. Stats as "from → **to**". Modifier list: icon, name, effect, and on the right "n/5" or "Selected n/5". Passives: unlock level with a progress bar, rank, effect, linked-skill chips (a passive that has no linked skill has neither the chip area nor a note). |
| Invest / Refund | Portrait: square − / + buttons at the right of the panel header, with the blocked reason under them. Landscape/PC: "Refund" (iron) / "Invest" (gold) buttons at the bottom of the panel. When blocked, the button label becomes the reason (except when it is blocked only because there are no points, section 6). When investable, a shine passes over the button on a 7.8s cycle. |

**Colours:**
- Never write design hex values in screen code. The colour-literal check in `check_ui_contract` rejects them.
- Add tokens to `UiTheme`. For gold and obsidian, use the existing Obsidian & Gilt tokens.
- New tokens needed:
  - passive silver: `#b9c6d0`, `#6f8291`, `#3e4852`, text `#c9d5de` / `#9fb0bd` / `#66727c`, border gradient `STEEL_RIM`
  - line colours: `COL` g/o/k/s1/s2/s3
  - ember: the `#e07a32` family
  - blocked-reason red: `#df927b`

**Drawing:**
- Draw gradients, borders, arc rings and hexagons in code by extending the existing mesh owners (`UiMesh`, `SkillTreeGraphic`, `UiButtonFace`).
- Follow the generated-art rule that small pictures, frames, masks and selection effects are drawn in code (`Docs/Implementation/Generated_Game_Art.md`).
- Show icons through `SkillIconView`, clipped with the existing masks (`Art/GlobalHUD/mask-circle|mask-square`).

**Fonts:**
- Body text uses `UiFonts.Body`; the title and roman numerals use `UiFonts.Display`.
- They differ slightly in shape from the design's Noto Serif KR and Cinzel. New fonts: section 17 D6.

### 9.4 Layout differences

- **Portrait:**
  - The tree scrolls vertically.
  - Order: tier badge → active rows (card + modifier tray) → the passive capsules that belong to that tier.
  - A passive goes in "the tier already open at its unlock level". Tier i opens at Lv max(1, gate[i]).
  - Link lines run down along the rail on the right of the trays.
  - When opened in the "passive view" state, scroll so that the first linked passive is visible (`passTop − 200`).
- **Landscape:**
  - One column per tier. A tier with 3 or more actives (Mage T2) is split into two lines.
  - Passives sit 3 per row on a silver shelf under the column.
  - When the plate is narrow, the tree scrolls horizontally.
- **PC:**
  - The layout is the same as landscape.
  - The whole tree is scaled by min(1.5, 846/innerW, 530/innerH) and centred in the plate. It does not scroll.
  - The panel is 240×564 on the right.

### 9.5 Interaction

- Tapping a card, modifier socket, passive, or a modifier row in the panel selects it.
- Tapping a modifier socket or row turns the description slot into that modifier's shape-change sentence. Tapping the base skill card brings back the base skill description. Invest and refund apply to the target of the sentence that is shown now.
- Invest and refund apply to the selected target.
- Switching class tabs keeps each class's selection and allocation.
- Initial selection: the design uses Whirlwind / Piercing Shot / Fireball. In the actual game, it is the target chosen last, or else the first investable active.
- Invest and refund save immediately (a `GameStore` commit), and after saving follow the UI refresh rules (`AgentRules/UI_Refresh.md`).

### 9.6 Effects

Most are one-shot; only the two below repeat. If the `hellscript.reduce-motion` device setting (the counterpart of `prefers-reduced-motion`) or the OS setting is on, all of them are turned off.

| Trigger | Effect | Time |
|---|---|---|
| Successful invest | The target pops 0.94→1.06→1. A light ring spreads and star-shaped rays spin. "+1" floats up. | 0.52s / 0.56–0.6s / 0.75s |
| Max rank reached | The rays spin larger. "MAX" appears. | 0.8s |
| Successful refund | Saturation drains as it shakes left and right. An ash ring is sucked inward. "−1" drops. | 0.34s / 0.4s / 0.7s |
| Rank pill, level chip | Pops to 1.32× and brightens. | 0.38s |
| Header stats | Spent n/60 and points left pop to 1.12×. | 0.38s |
| New tier opens | Light spreads from the tier badge and a beam of light passes. | 0.8s |
| A line turns gold | White light burns along the path (ignite). | 0.65s |
| User selects | A glint passes along the path to the selected skill. Each part starts 90ms later. | 1.3s × 3 |
| Selection mark | The selection border glow brightens and dims as if breathing. | 2.4s × 3 |
| Repeat 1 | The points-left orb pulses (only when there are points). | 6.4s cycle |
| Repeat 2 | A shine passes over the invest button (only when investable). | 7.8s cycle |
| Class tab change | The tree appears, rising from 6pt below. | 0.26s |

- **When something moves** (which node, max rank, newly opened tier, ignite and glint lines and their delays) is decided by `golden/csharp/SkillTreeD4.Fx.cs` and verified by `events` in the golden files. The window only plays back the result (screen spec section 11).
- There is no tween library. Animate by hand with `Time.unscaledTime`, and keep just one updater per window.
- Effect updates, frames with no change and mesh drawing do not allocate. The transaction frame of one click (save, layout, view model) may allocate, and does so only once (screen spec 4.5).
- Redrawing with the same state must not restart a transition.
- The curves (cubic-bezier) and keyframe values follow lines 12–97 of the source.

### 9.7 Performance and refresh

- Rebuild the node views only when (class, layout, language) changes.
- Invest, refund and select only change the state of the existing views. Do not `Clear` and redraw every time, as `HuntEdictWindow` does today.
- Group lines into meshes by state colour when drawing. Do not create a GameObject per segment.
- Redraw only when the `visibleState` key changes. Defer it while the user is pressing or scrolling, then restore the scroll position when it ends.

## 10. Text and localization

- **Screen text:** put `strings.ko/en` from `skill_tree_d4.json` into a new section `# D4 skill tree` of `Assets/HELLSCRIPT/Resources/Localization/en.txt`. The format is the Korean source, a tab, then English. Compose text with `Loc.F`.
- **Stored-text trap:**
  - A template that starts with a hole, such as `"{0}pt 더 필요"`, can swallow saved log sentences (`Loc.StoredText`).
  - If `StoredLocalizationTests` fails, fix it by adding fixed text before and after. Tell the user about the changed wording.
- **Skill names and effects:** the catalogue carries both Korean and English (the `ClassSkills` name/nameEn approach). They do not go in en.txt.
- **Skill descriptions and modifier shape sentences (new fields):** each active in `SkillTreeD4.json` gets `descKo`/`descEn` and each modifier gets `shapeKo`/`shapeEn` (the generator copies them from the spec JSON). Both appear only in the detail panel's description slot; for passives the effect sentence doubles as the description.
  - The existing `description/descriptionEn` in `ClassSkills.json` are long rule sentences holding numbers and decision rules. The wiki and other places use them, so **do not change them and do not show them in the new window.**
  - Rules for writing a shape sentence: say **how the form changes**. Do not write numbers that change with rank (the numeric effect line carries those); write only rank-independent values (3rd cast, 360°, a stack cap of 3). Korean uses the "~합니다" style; English is one complete sentence.
  - Length: the longest description in the design is 43 Korean and 70 English characters, and the longest shape sentence is 43 Korean and 64 English characters. Even with the modifier name and " — " added in front, they fit in 2 lines in portrait, 2 lines in landscape and 3 lines on PC. Anything longer is clipped at the bottom of the panel. The smoke test selects every active and every modifier in turn to check for overflow (section 13).
- **Missing text:** if English is missing, Korean is shown instead and the miss is recorded in `Loc.Missing`. Never put an empty string or a key name on screen.
- **Migration notice and refund-location text:** the migration notice (section 7) and "Refund in town" (section 17 D3) are new text. Add both languages.

## 11. Icons and art

- **92 new icons:** copy the originals byte for byte into `Assets/HELLSCRIPT/Resources/Art/ClassSkillIcons/<Class>/<id>.png`. Rename them from `iconId` to `id` (5.1).
  - Per class: Warrior 28 modifiers + 2 key passives, Ranger 28 modifiers + Elemental Imbuement + 2 key passives, Mage 28 modifiers + Elemental Lance + 2 key passives.
  - Match `.meta` to the existing icons.
  - The `Art/ClassSkillIcons/` row of `ResourceTextureBudget` (512, ASTC 6×6) already covers the texture budget.
  - Expected size: Android (ASTC 6×6) about 116KB × 92 ≈ 11MB. **Desktop (BC7) and web (ETC2) are each about 24MB** (512² × 92, no mipmaps). Check the effect on web build size (section 17 D11).
  - The original names, sha256 and import locations are in `Docs/Design/D4SkillTree/icons_manifest.json`. Only two overwrite an old file of the same name: `Ranger/A06.png` and `Mage/M07.png` (check the `.meta` guid).
- **Provenance:** add entries to `Docs/Art/ClassSkillIcons/manifest.json`. What to put in: original path, new name, sha256, `productionApproved=false`, model unknown. For adopting generated output, confirm real alpha with `tools/adopt_generated_game_art.py`.
- **Existing icons:** icons of surviving skills stay as they are. The design's base and passive icons are the current game icons.
- **Representative asset check:** import one icon first and check the import, budget and on-screen display before importing the rest (`AgentRules/Workflow.md`).

## 12. Places to change together

The table below is a partial list. **The full list of dependents and the "15 easy-to-forget places" are in `research/dependents.md`**, and the list of places that point at ids that disappear is counted by `python3 Docs/Design/D4SkillTree/tools/legacy_id_inventory.py` (`--check` fails until the references in groups 1~5 reach 0).

| Area | Files | Work |
|---|---|---|
| Legacy 18-slot layer | `GameCatalog.cs:466-484`, `SkillProgression.cs` (12 slots), `HuntEdict.cs:56` `SkillIds()`, `Economy.cs:160`, `CombatSimulation.ClassSets.cs:24-30`, `HeroStats.Blacksmith.cs:11` | Keep the bridge for the surviving 01–06. Handle the replacement of A06 and M05 as `legacyRemap` says. |
| Edicts | `HuntEdictV2.Options.cs`, `HuntEdictV2.Availability.cs:45-75`, `EdictOptions.cs:90-100`, `HuntEdictV2.Order.cs:49,69,85`, `HuntEdictQuickPresets.cs:112`, `Resources/HuntEdictQuickPresets.json`, `Resources/SkillPresetScenarios.json`, global `survival.escapeSkill/defenseSkill` | Move or drop removed ids. Adapt the use options of A06 and M07 to the new behaviour. Modifiers get no options of their own. |
| Quick edit | `GameUI.SkillQuick.cs`, `HuntEdictQuickMenu` | Use the same rules engine when changing ranks. Use the same wording for blocked reasons. |
| Items and runes | `Itemization.cs:208-219` (LW01–LM04 requiredSkill), `Resources/Data/LegendaryPowers.json` (skill/affectedSkill), `ClassSkills.json` gear/bonuses, `Docs/Design/ClassSetReference/catalog.json`, `Resources/Runes/V13/catalog.json` (01–03 only), `RuneV13Catalog.cs:40-44`, `RuneMasteryCatalog.cs:59`, `RuneCombatBonuses.cs:42` | Move through `legacyRemap`. Effects that pointed at an absorbed skill apply only when that modifier is chosen. |
| Tutorials | `Tutorials.cs:98-101` (W01/A01/M01 prologue), `Tutorials.Progress.cs:10-59` (F06/H07/H08), `GuideTutorial.cs:13` (W01), `GameUI.GuideTutorial.cs:36-102`, `ClassPracticeLesson.cs:9-54` (A02/M02) | Section 17 D7. Whirlwind (W01) is now T2 and cannot be learned at level 1. |
| Puzzles | `Resources/PuzzleTutorial.json` (kits), `PuzzleTutorial.cs:69,90-175` (line 129 `skill/W02` escape), `HuntEdictWindow.Skills.cs:23` `AllowedSkill` | Grant kits as an exception to the tree rules. MP01 is removed, so pick a replacement passive. |
| Presentation | `WorldView.SkillFx.cs`, `GameAudio.Combat.cs`, `GlobalHudSnapshot.cs:32`, `RiftEntryWindow.cs` | Hook up the effects and sounds of the new ids and modifier effects. Reuse the base skill's first. |
| Generators and docs | `tools/class_skills.py` (4/3/1 slot check, `sourceFingerprints`), `tools/class_skill_runtime.py` (111·123·60·55 checks), `tools/class_skill_options.py` (55/166), `tools/build_skill_tree.cjs`, `Docs/Design/Class_Skill_Trees(.en).md`, `HELLSCRIPT_Class_Skills(.en).md`, `HELLSCRIPT_{Warrior,Ranger,Mage}_Skills(.en).md` | Move them to the new system and regenerate the docs in both languages. If you edit a file that the fingerprint check covers (`GameCatalog.cs`, `SkillEffects.cs`, `SkillProgression.cs`, `Itemization.cs`), update the fingerprints with it. |
| Wiki sources | `tools/wiki.py:143-163` (skills·passives DB), `:302` `expected={'skills':18,'passives':18,…}`, `:690-714` `build_class_abilities`, `PAGE_META` | Switch the DB and the expected counts to the new system. On work branches, change only the sources. |
| Wiki tests | `tools/test_wiki.py:37-46` (01–03 at Lv1), `:134-135` (W01 Whirlwind range 2.5), `:84` (108) | Replace unlock-level assertions with tier assertions. |
| Balance | `tools/balance_1000.py:222-231` (fingerprints), `tools/test_balance_1000.py:19`, `Docs/Design/Balance1000/production_skill_tree_audit.json` | Re-baseline on the new system. Add the 41–60 range. |
| Audit tools | `ClassSkillPassiveMeasurement.cs`, `Editor/BuildIntegrationValidation*.cs`, `Editor/NaturalRiftProgressionAudit.cs`, `tools/class_skill_reports.py` | Reflect the new ids and modifiers. |
| Old screens | Tree and inspector in `HuntEdictWindow.Skills.cs`, `SkillTreeGraphic`/`SkillTreeLayout`, `Prototypes/SkillTree/*`, `RuntimeSkillTreeSmoke*.cs`, `ClassSkillNativeSmoke.cs` | Replace with the new window at the switch-over step. Move the smoke tests to the new window. |

## 13. Verification

**Verification layers** (screen spec section 1): L0 rules, L1 layout, L2 wording and L3 interaction run in EditMode against the golden files (55,803 checks; they already pass once the port is moved in). L4 internal dimensions are checked by measuring the real rectangles of the detail panel and widgets and comparing them. L5 look compares smoke test screenshots with the reference renders (`golden/tools/compare_screens.py`).

**During work, run only the checks for what changed.**
- rules: `SkillTreeD4RulesTests`
- layout, view model, interaction and internal dimensions: `SkillTreeD4LayoutTests`, `SkillTreeD4ViewModelTests`, `SkillTreeD4InteractionTests`, `SkillTreeD4AnatomyTests` (screen spec section 13)
- save: `ClassSkillCompatibilityTests` and the new migration tests
- combat: per-modifier and per-passive simulation tests
- edicts, tutorials and puzzles: the related suites
- text: `LocalizationTests`, `StoredLocalizationTests`
- resources: `BuildResourceTests`
- generators: `build_skill_tree_d4.py check`, `class_skill_runtime.py check`
- UI: `python3 tools/check_ui_contract.py --verify`, `python3 tools/check_ui_refresh.py`, `StoreViewBindingTests`

**Once at the end:** run after the final code change and the `main` integration.
- full EditMode, 1 run
- macOS development build runtime smoke test, 1 run
  - the new `-hellscriptSkillTreeD4Smoke`
  - the existing smoke tests this change touches: SkillTree, ClassSkill, BattleQuickEdit, Tutorial, GuideTutorial, puzzles
- If batch mode changes `ProjectSettings/ProjectSettings.asset` or `UnityConnectSettings.asset`, restore only the changes made by this run.

**New smoke test `-hellscriptSkillTreeD4Smoke`:**
- Run the same sequence as the design's click test in Korean and English.
  1. select Whirlwind
  2. refund → Spent 23/60, Lv4
  3. select Blood Recovery
  4. invest → Lv1/5
  5. Ranger tab → Piercing Shot
  6. Warrior tab → allocation kept
- Replay the **60 recorded cases** of `golden/interaction/*.json` in the same order with **real clicks**, and check at every step that the screen state and the effect signals equal the golden ones (Korean and English).
- Relaunch, as with `-hellscriptSkillTreeResume`, and check that the save persists.
- Screen sizes: 440×956, 956×440, 1440×810, 1440×900, 1680×720 (21:9). Simulate notches with `UiSafeArea.Simulate`.
- Select every active (10 per class, 30 in all) and every modifier (84) in turn, in Korean and English at every screen size, and check that the detail panel content does not overflow the panel and that the description slot does not exceed two lines (three on PC). In the design, all 684 selection states on the 18 screens passed.
- Attach to the PR a **table** (screen spec section 14) comparing the 24 screenshots with the 24 reference renders using `golden/tools/compare_screens.py`. For anything outside the pass limit, write the number and reason of an allowed approximation (A1~A8), or write it as something to fix.

**Physical devices:** never report macOS results as physical-device results. List the screens that were not checked separately.

**Wiki:** do not run `wiki.py build/check` on work branches. At merge, generate, check and publish once as `AgentRules/Wiki_Publish.md` says.

## 14. Work order (PR units)

Branches are `codex/d4-skill-tree-<step>`, each started in its own worktree from the latest `origin/main`. One PR per step, with only the related checks run.

Step 0 (this document and the specs) must be merged into `main` before step 1 starts. If it is not merged yet, start from the branch that holds this document, `claude/d4-skill-tree-plan`.

The new system sits behind `SkillTreeD4.Enabled` (default false). Development builds and smoke tests turn it on with the `-hellscriptSkillTreeD4` argument. **The save migration, schema 22 and level cap 60 also happen only when it is on** (section 17 D18). The shipping path does not change before step 7.

The starting point, prerequisites, whether steps can run in parallel, and the prompt for each step are in the [step briefs](D4_Skill_Tree_Step_Briefs_20261012.en.md).

| Step | Content | Done when |
|---|---|---|
| 0 | (the same change as this document) plan, spec JSON, design source, reference renders | merged into `main` |
| 1 | Data, generator and rules engine (sections 5–6) | `build_skill_tree_d4.py check` and `SkillTreeD4RulesTests` pass |
| 2 | Save v5, migration, level cap 60, points = level, loadout rules (section 7) | migration and compatibility tests, XP tests for levels 41–60 |
| 3 | Import 92 icons (section 11) | 1 representative icon → all, `BuildResourceTests`, manifest |
| 4 | Skill window (sections 9–10): six packages UI-1 to UI-6 (screen spec section 13), opened through the development flag | each package's done criteria, 55,803 golden checks, UI contract, comparison table against the 24 reference renders |
| 5.0 | Combat common frame (combat spec section 2): v5 fields, `ClassSkillCast` extension, reading rank values, ultimate ranks, cleanup rules | the paired-test frame runs with one A-class modifier |
| 5a/5b/5c | Combat: Warrior / Ranger / Mage (base skill values, modifiers, passives, key passives, ultimates with 3 ranks) | per-class simulation tests, acceptance criteria (combat spec 2.8) |
| 6 | Connections (section 12): 6a quick edit and HUD, 6b tutorials and puzzles, 6c edicts and auto-battle, 6d tools, docs, text, items, runes, wiki sources, Balance1000 | related suites, generator check, group 1~5 references of `legacy_id_inventory.py --check` at 0 |
| 7 | Switch-over: turn the flag on by default, clean up the old tree screen and data paths → `main` integration → full EditMode 1 run + macOS smoke test 1 run → merge → wiki publish | all of section 13 |

## 15. Known traps

- **Two skill systems:** the legacy 18-slot arrays and the id-based tree are bridged by `ProjectLegacy`. Removing one id can silently shift the array indexes.
- **Fingerprints:** `tools/class_skills.py` pins the sha256 of `GameCatalog.cs`, `SkillEffects.cs`, `SkillProgression.cs` and `Itemization.cs`. Editing these files fails check.
- **Wiki constants:** the `expected` counts in `wiki.py` and the level and name assertions in `test_wiki.py` fail as soon as the counts change.
- **Balance1000:** its outputs go stale when the tree files change (`--check`).
- **Stored-text template trap:** templates that start or end with a hole swallow saved logs (section 10).
- **JsonUtility:** it never restores null. You must tell an empty array from "field missing".
- **Edict window:** it opens on the Overview tab. Smoke tests call `SelectTab("skills")` first. There is a trap where `Repaint` closes an open dialog.
- **`.meta`:** when Unity rewrites `.png.meta`, check the guid and commit it together.
- **Smoke builds:** build in batch mode from a `cp -Rc` clone and run with the smoke flag. Pass arguments as arrays.
- **Baseline:** full EditMode and the smoke tests may already have failures. Before calling a failure new, compare with the `main` result under the same conditions.
- **The golden files belong to the design owner:** do not edit `golden/`, `skill_tree_d4.json` or `SkillWindow.dc.html` in implementation work. If you suspect a problem, report the name and input of the failing check. When the design changes, the design owner regenerates them with `build_golden.js` and confirms that the reference implementation and the port pass the same checks (`golden/README.md`).
- **git ignores `*.csproj`** (Unity template): the port's test project (`golden/csharp/GoldenTests.csproj`) was added with `git add -f`. The output is sent by `Directory.Build.props` to `Temp/`, which is ignored.
- **The 15 screen traps are in screen spec section 15, the combat traps (cleanup rules, hard-coded ids, forecast = release) are in combat spec 2.4 and 2.5, and the save traps are in section 0 of `research/save_migration.md`.**
- **Concurrent work:** when this was written (2026-10-12), Codex was working on the guide tutorial in `codex/guide-tutorial-segment2`. That plan uses the first W01 hit as a trigger. W01 is moving to T2, so align it in step 6. Do not switch branches in the shared checkout.

## 16. Do not

- Do not put rules in screen code. The skill window, quick edit, edicts and combat all use the same engine.
- Do not write design hex values in Presentation code. Use `UiTheme` tokens.
- Do not add new font files or create fonts separately. That holds until D6 is decided.
- Do not resize icons, regenerate them or remove their backgrounds.
- Do not bring back text-size adjustment checks or large-text smoke tests.
- Do not run full EditMode or the smoke suite at every step.
- Do not rename existing save keys.
- Do not generate or publish the wiki from a work branch.
- Do not widen `ADAPTERS` or the check lists just to pass. Register only the one new window, with its reason.
- Do not edit `Docs/Design/D4SkillTree/golden/`, `skill_tree_d4.json` or `SkillWindow.dc.html` in implementation work (design owner).
- Do not load modifiers onto the use-option system. Do not quietly carry over current bonus effects that are not in the design (D23).
- Do not trigger the save migration, schema 22 or level cap 60 in a build where `SkillTreeD4.Enabled` is off (D18).

## 17. Items that need user confirmation

Work can proceed on the defaults. If the user decides differently, change it before that step.

| # | Question | Default |
|---|---|---|
| D1 | Where the new skill window lives | A standalone window. The edict window's skills tab keeps only the loadout bar (4+1), the attack order and a "Skill tree" button. The design panel has no equip button, so equipping stays in the edict window and the quick edit, as today. |
| D2 | What the class tabs mean | An account creates 1 hero per class (Warrior, Ranger, Mage; `GameStore.NewAccount`). The tabs map 1:1 to these three heroes. Switching tabs does not change the selected hero (`selectedHero`) or the run in progress. Each tab edits that hero's own `build.classSkills` directly (a new store method that takes a hero index). Level, points and passive unlocks are also that hero's. While a run is in progress, only that run's hero can be invested in and the other tabs are view-only. |
| D3 | Where refunds and modifier swaps happen | Only in town (outside a run). During a run, only investing works. The reason shown when a refund is blocked is "마을에서 회수" / "Refund in town" (new text). |
| D4 | Existing saves | Refund everything. Empty the loadout and show a notice once. No automatic re-allocation. |
| D5 | Gear +1 rank and rune skill levels | Apply to active base skills only (max 6). Do not apply to ultimates, modifiers or passives. The skill window does not show them, as in the design. |
| D6 | Fonts | Use the existing `UiFonts`. Using the design's Noto Serif KR and Cinzel as they are would need adding font files and deciding a size budget separately. |
| D7 | Tutorials | Change the guide tutorial's first skill to the T1 generator Raking Wound (W09), then teach Whirlwind after 2 points. The prologue and puzzles keep granting kits as today (an exception to the tree rules). |
| D8 | Elemental Lance (M07) when no modifier is chosen | Fire element, no extra effect. |
| D9 | Level cap 60 | Extend the XP formula unchanged. Review rift and content balance for 41–60 separately in Balance1000. |
| D10 | Removed passives (11 per class) and ultimate-only passives | Delete them. Their points are refunded during migration. |
| D11 | Icon texture size | Follow the existing 512 ASTC 6×6 row. The APK grows by about 11MB. **Desktop (BC7) and web (ETC2) are each about 24MB.** Lowering only the 84 modifier icons to 256 makes it about 3.5MB for the APK and about 8MB each for desktop and web. |
| D12 | Where modifier shape sentences are shown | Only in the detail panel's description slot (right under the header, above the stats divider line) (the user's specification). Tapping a modifier socket or a modifier row in the panel turns that slot into "modifier name — shape-change sentence", and tapping the base skill card brings back the base skill description. Modifier rows carry only the name and the numeric effect. To let the three sentences be compared at a glance, there is an option of putting the sentence in each row, but it makes the portrait panel about 50pt taller and the landscape screen needs scrolling inside the panel. If requested, try it in the design first. |

### Decisions from the investigation (D13 ~ D24)

Work can proceed on the defaults. **The ones worth having the user decide are D6 (fonts), D17 (heroes at level 40 or below and ultimates) and D18 (flag scope)**: their defaults change behaviour or size significantly. For the rest, use the default while implementing and note it in the PR.

| # | Question | Default |
|---|---|---|
| D13 | When migrating a hero who has a run in progress (an interrupted run, resume, portal, tutorial run) | Replace the hero's allocation with an empty v5 and clean up the run's `ClassSkillRuntimeState` (the `ApplyHuntEdictChange` way). Alternatives: postpone the migration until the run ends (the old read path would have to be kept alive until step 7), or end the run. |
| D14 | Old share codes (HED3·4·5) and saved presets | Issue v5 with a new prefix (HED6). From old codes, import only the policy (options, order, automatic) and discard the allocation. Show a notice. |
| D15 | Lifetime of the old catalogue (`ClassSkills.json`, 111 entries) and `ClassSkillTree.json` | Keep them until step 7 (the old read path and the verification tests). Move the tests to the new system in step 6, then delete them in step 7. |
| D16 | How to show modifiers in the quick edit and HUD | HUD slots show the base skill icon only (no new art). A long-press popup shows the modifier name, shape sentence and rank. In the popup during a run only investing works, and refunds only in town (D3). |
| D17 | Heroes at level 40 or below cannot learn an ultimate (T5 gate 40, points = level) | Leave the rule as is (from level 41). Change the "Lv.40" wording and tutorial H08 to 41. Alternative: lower the T5 gate (a rule change, so the user decides). |
| D18 | Scope of the `SkillTreeD4.Enabled` flag | The save migration, schema 22, level cap 60, the new screen and the new combat happen only when it is on. A build with it off rejects a schema 22 save when reading it (this only happens in developer builds). The default is turned on in step 7. |
| D19 | Saving the migration notice | Save it per hero and acknowledge it on the first skill window open (`Transact`). Do not delete the archive files (about 0.75MB per save). |
| D20 | Training records, comparison baselines, puzzle kit copies (`kitHero`) and guide equip records | Empty the training records and comparison baselines (they point at the old loadout). Rebuild the kit copies from the new kits. Filter the guide records through the new id set. |
| D21 | Default shape of Warrior Crushing Blow (W03): the design says "single hit", the implementation is a 100° multi-hit | Keep the current 100°. The design owner fixes the design's wording. |
| D22 | Base effect and consumption rule of Ranger Elemental Imbuement (A06) | Next 3 shots deal +25→35% bonus damage (no element). BASIC, A01 and A02 shots consume it. Branches, echoes, turrets and rain do not consume it. |
| D23 | Current bonus effects that are not in the design (W11 guard and resource, W12 +15% block, A12 +20% speed …) | Remove them. The design is the reference. |
| D24 | Mage Sage Incarnate (M18)'s −20% cost and 3-element burst (not in the design's default text) | Keep them. The design owner completes the wording. |
