# Per-skill hunt edict sub page — B0 storage and contract design memo (2026-10-24)

[한국어](Skill_Edict_Pages_Contract_20261024.md). B0 contract and implementation record for user item 17 (Part B). **The headless B0/B2 catalog, draft and persistence foundation is implemented; B1 screen code awaits direct visual approval evidence.** The initial proposal is retained below; the final implementation section supersedes it. Option meanings and numbers come from [`HELLSCRIPT_Hunt_Edict_Tactical_Options.en.md`](../Design/HELLSCRIPT_Hunt_Edict_Tactical_Options.en.md). `HEC-*` are old options and `HET-*` are planning row numbers, never new save keys or UI text.

## One-line summary (for the user report)

Per-skill values live in `ClassSkillLoadout` (v5) as a **sparse list** (`skill id + modifier id + row key + value`, omitted at the default). `HuntEdictV2Document` is not used because it drops unknown fields. The page keeps its own draft; only [Save] calls the existing atomic commit once; [Revert] restores the last saved state.

## Facts confirmed in code (paths under `Assets/HELLSCRIPT/Runtime/Core/`)

- `HuntEdictV2Document` (`HuntEdictV2.cs:14-25`): `Canonical` rejects unknown keys and returns a new document with only `heroClass, slots, global, skillOrder, skills` (`:58-69`), so a new field there would silently vanish. Per-skill values (`EdictOption{id,value}` string pairs) exist only as `HEC-*` rows for the six legacy skills + BASIC; D4 actives (W02, A03, M04 …) have none (`HuntEdictV2.Options.cs:33,131-135`).
- `ClassSkillLoadout` (`ClassSkills.cs:75-88`) holds `actives[4], ultimate, order[], automatic[], ranks (every modifier choice), options, presetSelections, legacyEdict`. D4 `Canonical` truncates `options`/`presetSelections` ids with `Split(':')[0]` and keeps only `KeptScope` (BASIC, actives, passives) (`SkillTreeD4/SkillTreeD4.Loadout.cs:39-42`), so modifier ids (`W02-3`) are rejected today.
- Editing/saving: `HuntEdictEditSession.Draft` is `saved.Copy()` (JsonUtility round trip), `Dirty` compares JSON, `Revert` is `Draft=saved.Copy()` (`HuntEdictLoadout.cs:93-147`). `CommitHuntEdictForHero` validates everything, applies to a cloned `staged`, `Write`s (tmp file + `File.Replace`) and adopts only after success; on failure it sets `Error` and keeps the draft (`GameStore.HuntEdict.cs:11-69`, `GameStore.cs:439-513`).
- **Trap**: a skill-window point commit (`CommitD4Allocation`) builds a fresh `HuntEdictEditSession` (`GameStore.D4Allocation.cs:29,33`) and the window then calls `Session.AcceptSaved` (`HuntEdictWindow.cs:175`). A hunt-edict draft kept in that session would vanish the moment a point is invested, so the sub page holds **its own draft**.
- The five persistent presets are `HeroSave.edictPresets[].loadout` (a full `HuntEdictLoadout` → `ClassSkillLoadout`), so a new list on `ClassSkillLoadout` rides along through copy, presets and HED6 sharing. `CopyPolicy` (per-skill-scope copy, `HuntEdictSkillPresets.cs:62-91`), `SamePolicy` (`:58`, progressive-disclosure gate `HuntEdictProgression.cs:166-183`) and the D4 `Canonical` pruning must change with it.
- A newly equipped skill already defaults to automatic use On in `ClassSkillTree.Equip` (`ClassSkillTree.cs:54,59`: `if(!already) automatic=…Concat(id)`). Unequipping makes `NormalizeActions` drop the `automatic` membership, so re-equipping returns to On (`:43-47,69`) — different from "unequipping never deletes rules or automatic-use state" (implementation contract, line 20).
- Global options (target grade, chase, danger, HP, emergency survival) are `EdictOptions.Global` (`EdictOptions.cs:41`), edited in the old edict window's `combat`/`survival` tabs. Link target: `HuntEdictWindow.SelectTab("combat"|"survival")`, `FocusGlobalOption(id)` (both public). `GameUI` has no entry wrapper for them.

## Decisions (proposed, firm)

**(a) Where and what shape.** Add `tactics` (a list) to `ClassSkillLoadout`. One entry = `{ skill, scope, row, value }`: `skill` is the parent active id (`W02`), `scope` is `""` (parent row) or the actual modifier id (`W02-3`), `row` the data row key, `value` a string (same approach as `ClassSkillOptionSelection`). A default value has no entry (sparse), sorted Ordinal. Another modifier's values stay under their own `scope`, do not fire until that modifier is chosen, and replaced parent rows keep their values (not applied in the current form). JsonUtility cannot serialize `Dictionary`, hence a list.

**(b) Version, backward compatibility, original preservation.** Omit the new field when empty so already issued HED6 codes keep importing (`ClassSkillCodec.Wire()` already strips empty fields by string replacement, `ClassSkillCodec.cs:23`). A code with values is rejected by older builds (strict HED6 canonical check) — per the existing rule that new shared data needs a separate schema version (`Skill_Option_Catalog` line 474), the implementation PR amends the contract documents together. New save keys use neither `HEC-*` nor `HET-*`.

**(c) Draft, save, revert.** Opening the page copies that skill's `tactics` (+ automatic use) into a draft. Until saved the live settings do not change. [Save] calls a new `GameStore` method (e.g. `CommitSkillTactics(heroId, skillId, rows)`) that changes only that skill's entries through `HuntEdictEditSession` and hands off to `CommitHuntEdictForHero` — one atomic write; on failure the draft and `Error` stay. [Revert] = draft := last saved. Saving the hunt-edict draft and Part A's "save slots on close" are independent of each other, stated plainly in code and copy. The five presets and quick presets keep their flow; check the existing test that a quick preset leaves no hidden values (`HuntEdictQuickPresetTests.cs:62`).

**(d) Automatic use.** The toggle exists only on the sub page, and `automatic[]` stays the single owner. A newly equipped valid card defaults to On (already in `Equip`); import preserves the original Off (`HuntEdictPresetImport.Prepare` must not overwrite `automatic` — pin with a test at implementation time). The current loss of Off on unequip is best kept as a sparse `tactics` entry `row="auto", value="0"` that `Equip` reads; confirm with the user in the implementation PR.

**(e) Row definitions are data.** `Resources/Data/SkillTactics.json` (proposed name): row key, kind (toggle/choice/ref/num), options (with skill-reference index), min/max/step/default/unit, disabled-reason rules, owner (parent/modifier id), the parent rows a modifier replaces, summary rule. Numbers, choices and defaults are copied from the option document's tables; the cross-check test reports "skipped" when the document is absent.

**(f) Global settings link.** Today's target is the `combat`/`survival` tab of `HuntEdictWindow`. When the old edict window is removed, this link (including a new `GameUI` entry wrapper) is the point to update.

**(g) Row ceiling.** D4 cards allow at most seven editable rows including automatic use. Eight remains historical v0.2 scope. The initial concern about eight A02 rows is superseded by the current tactical document §3 and count table: equipment and modifiers reuse the same two rows without adding another pair, leaving six rows.

**(h) Entry button (user instruction 2026-10-24).** The entry is a 44 px icon button in the detail panel header, right of the skill name (not a fixed bottom row → the panel gets no scroll and the frame coordinates stay). The badge shows the saved automatic-use state (✓ on / – off), previews (unselected modifier, unlearned) show a blue `i`, and the full accessible name carries the current tactic summary. The summary line itself is in the sub page's top info card. The button art is a Codex-made medallion (`Docs/Art/SkillEdictIcon20261024/`, model unverified, not production-approved) to move into `Resources/Art/GlobalHUD/Casual/` at implementation. Long skill names (e.g. `War of the Ancestors`) narrow the name column, so implementation reuses the existing `fitFs`-style auto shrink.

## To confirm before implementing / open

- The option planning document (`Tactical_Options.md/.en.md`) must be committed for the test to cross-check ids — it is the user's file, so it was not committed here.
- The document does not say whether the other rows of the same card are cleared, folded or greyed while automatic use is Off. The mock-up assumes "rows stay editable + values kept + one note line".
- Making combat follow the options is outside this unit (values are saved but not yet applied in combat). A single switch showing the entry row only in development/mock hosts is the default.
- The Unity serialization depth limit was not found in the repository. A shallow field directly under `ClassSkillLoadout` does not extend today's deepest path (8 levels).
- Part B order: data + validation tests → save method + draft logic tests → sub page UI → the three representatives. Start after Part A merges (it changes the same detail area and close flow of the skill window).

## Implementation record, 2026-10-24 — B0/B2 foundation; B1 approval pending

**Approval boundary:** The direct Part A approval explicitly names the equipped-skill row. This record does not broaden it into approval for new Part B screens. Prompt §5/§9.5 forbids B1 screen code before direct visual approval. No entry button, sub page, choice list or modal has been written. The [existing artifact URL](https://claude.ai/artifact/JsCKnJUbKkzfBYyJDR3493) comes from the handoff record; the public reader could not open it, so its current boards and approval were not verified here.

Prompt §9.4/§9.5 explicitly assigns skill-specific combat changes to subsequent work. Their absence is the agreed boundary. `SkillTacticsCatalog.PlayerEntryEnabled=false`; player hosts cannot open sessions. Development/mock hosts currently expose headless APIs only. No combat execution code changed.

The branch starts at latest `origin/main 1f1887d1148d99e5b5f42f9a899d8945c6209259`, with Part A `5f2ff71f719a5588fe1ae6b3f5129d930dae9c93` applied as dependency `451bb130ea93f61b987865e98cb0f4898feb2a24`. Both appended translation sets were retained when resolving the sole conflict. The root originals, user-test projects and saves remain untouched. The authorized KR/EN tactical documents are copied unchanged into this branch.

### Actual persistence and compatibility contract

- `ClassSkillLoadout.tacticsVersion=0` denotes existing storage; 1 denotes the sparse `{skill, scope, row, value}` list. Parent and actual modifier IDs are separate. Defaults may be omitted; inactive modifiers, replaced parent rows and unknown rows remain stored. Newer schemas are retained read-only and cannot be exported by this writer. No implicit conversion from legacy independent M04-3 or HEC options occurs.
- `SkillTacticsEditSession` owns the last successful save, detached draft and requested navigation. `CommitSkillTactics` checks the current owned values/modifier against that baseline, merges only visible rows and calls `CommitHuntEdictForHero` once. Other skills, globals, points, slots and unsupported rows come from the latest saved loadout. A failed write retains disk, owner and draft. Only the existing successful `Committed` event notifies subscribers.
- Undo restores the last successful save. Back, close-window and global navigation use save-and-leave, discard-and-leave or continue-editing. The detached return state preserves class tab, selected/detail nodes, tree/detail scroll and split ratio; actual uGUI wiring belongs to B1.
- `automatic[]` remains the equipped-action owner. Sparse parent `auto=0` remembers Off while unequipped. `NormalizeActions/Equip/SetSlots` preserve imported Off and removal/re-equip Off; a newly configured card defaults On. Other tactical values remain editable while Off.
- Empty new fields are omitted from old HED4/HED6 wires, preserving canonical bytes. New direct skill codes use `HED7`; preset envelopes use `HED8` with envelope schema 6. Unknown JSON fields are rejected, without replacing or approximating the original. Existing five persistent preset copies and scoped `CopyPolicy/SamePolicy` include tactics. Old quick recipes are not inferred to define new tactical meanings.

### Source comparison and representation types

All three representatives agree with the current source and attached mock specification on choices, ranges, steps and defaults. References store stable IDs separately from actual KR/EN display names. HET IDs are provenance only. Twenty authored rows produce cards of at most seven rows.

| Type | Headless contract | Connect after B1 approval |
| --- | --- | --- |
| Toggle | One automatic owner, retain Off values | One page toggle; read-only entry badge |
| Choice | Stable keys, KR/EN labels, default | 2–3 short buttons; longer choice list |
| Number | Decimal bounds, step, unit, clamp/snap, endpoints | 44px controls, range/default information |
| Skill reference | Learned/equipped/automatic/modifier reason | Icon, name and unavailable reason |
| Conditional/replacement | whenRow/whenValues and modifier replaces | Applicability indicator; selected modifier only |
| Explanation/global limit | Bilingual notes and reasons | Readable without hover; existing global destination |

### Concrete visual review targets

These are review states tied to real catalog/session values, not completed screens or an approval record. Reuse the existing outer frame, tabs, scroll and split. Part A's later contract places the entry in a **44px icon to the right of the detail title**, superseding the old footer-row instruction. Review 440×956, 956×440, PC 16:9/16:10/21:9 in both languages at the default font size.

| Review | Default state | Edit/navigation states |
| --- | --- | --- |
| A: W02+W02-3, seven rows | On; Both; Pack edge; 4m; Free; Keep one for escape; 1 charge | 2–8m in 0.5 steps; 0–1 charge; editable while Off; distance unused during Escape; W01/W03/W04 unavailable reasons |
| B: A03 four rows → A03-3 three rows | On/Ambush approach/Keep existing/Cover new victims → On/Current fight edge/Favor primary | Remove three trap rows and retain their values; unselected modifier preview cannot save; edited position appears in saved summary |
| C: M04+M04-3, seven rows | On; Survival escape; None; Free; Global danger rule; Protected reposition; Use actual last element | Long choices in both languages; actual M02/M08/M11 names and reasons; global protection and emergency-cast notes; no free shield or element |
| Shared | Fixed changed/undo/save controls; body scroll only | Three leave choices; failed-write draft; updated saved summary; unchanged tree/detail/split return; independent slot save |

### Validation and unexecuted work

`python3 -B tools/check_skill_tactics.py --report Builds/Validation/SkillTactics20261024/catalog-check.json` compares all 293 source rows and the 20 authored rows in both languages, including choices, defaults, numeric contracts, replacements and summary references. The report and `SkillTacticsCatalog.MissingNodes()` list the exact **27 missing parents and 81 missing modifiers**. Unsupported nodes/current modifiers cannot create an editing session.

`SkillTacticsTests` covers source matching, decimal/integer endpoints, unavailable references, Off, replacement/unknown-row retention, previews, failure/retry/undo, all leave choices, return state, independent slot saves, stale draft rejection, HED6 compatibility, HED7/HED8 round trips, original preservation and five presets. **No Unity, Edit Mode, build or screen capture ran on this branch.** The parent integration owner runs focused checks on the final candidate; compilation/runtime success is pending that result. No scene, prefab, package, project setting or combat code changed.

### B3 follow-up split

1. Warrior catalog: remaining nine parents and 27 modifiers, source comparison and seven-row validation.
2. Ranger catalog: remaining nine parents and 27 modifiers, including A02 equipment/modifier reuse of the same rows.
3. Mage catalog: remaining nine parents and 27 modifiers, preserving the legacy independent M04-3 distinction.
4. Warrior combat: ten parents/28 modifiers, actual landing/control/charge/follow-up events and global survival limits.
5. Ranger combat: ten parents/28 modifiers, actual trap/turret, infusion consumption, arrow and afterimage events.
6. Mage combat: ten parents/28 modifiers, actual owned fields, element history, placement/expiry and control events.

Each combat unit preserves original execution settings, uses explicit conversion drafts and proves actual events, failure and re-entry. Player entry stays disabled while integration is absent. Approved and validated B1 screens are a shared prerequisite to catalog expansion.

## Integrated focused validation update (2026-10-24)

Candidate5936415e includes latest main1f1887d1. The 23 directly affected E/H/F fixtures first stopped at a new test's List.Length compile error, corrected to Count. The subsequent run passed521/550;29 H/F cases failed an EditMode EventSystem assumption. The final corrected H/F run passed40/40. Together with the510 unchanged passing cases this establishes550/550 unit coverage. Runtime source did not change for these fixture corrections. Evidence: local takeover/e-h-f-unit-proof.json, e-h-f-retest/results.xml, h-f-final-unit/results.xml. All9 local CI steps passed (takeover/integration-ci/report.json; installedPython3.9.6/Node26.8.2, exact parity with workflow3.12/22 unverified).

New EventSystems do not register as global current in EditMode, so fixtures use their own instances. Only the known deferred-Destroy EditMode engine diagnostic is expected by its exact message; other errors still fail, and failed-save diagnostics are checked separately. Unit assertions retain real hit-area geometry, filter decisions, event dispatch, persistence, draft and navigation checks.

**Rendered global Canvas raycasts, native pointer interaction and Korean/English captures remain pending.** The five empty-hit failures persisted with Metal batch and are retained in h-f-raycast-graphics. They are not reported as native success; rendered-frame validation is a separate requirement. Nine own temporary/import files were preserved with hashes before restoring HEAD bytes. Actual local CLI review and remote integration need their own subsequent evidence.
