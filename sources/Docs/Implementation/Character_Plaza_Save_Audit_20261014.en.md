# Character plaza save audit — 2026-10-14

> **2026-10-28 A2 correction:** References to the large edict window, ≡ menu and their dedicated components or smokes are historical. Use the [current entry points](../../Assets/HELLSCRIPT/Runtime/Presentation/GameUI.HuntEdict.cs) and [replacement screens and source owners](Hunt_Edict_Window_Removal_20261028.en.md). Earlier captures and test counts describe the earlier implementation.

[Korean and exact integration edits](Character_Plaza_Save_Audit_20261014.md)

PR1 implements the approved six-hero contract in the isolated source workspace. Schema 23 keeps male Warrior/Ranger/Mage at indices 0–2 and female counterparts at 3–5. Hero IDs and class IDs remain stable. `HeroSlots.Index`, `Created`, and `IsCreated` own mapping and eligibility. The default `NewAccount` factory keeps three created male heroes; PR2 opts the production constructor into `onlyWarriorCreated:true`.

Three-hero input is archived byte-for-byte as `.schema<N>-before-six-heroes.json` before mutation. Archive failure stops loading. Existing heroes become explicitly male and created; female defaults use the same catalog-aware factory as new accounts. Legacy developer mode remains schema 21 and rejects newer/six-slot reads and writes before normalization or temporary-file writes, without falling back to an older backup. Detached sandbox fixtures expand without disk access.

`CreateCharacter(index)` uses the existing staged transaction, starts and parks the hero's tutorial share through `Tutorials.Switch`, restores current selection/share, and adopts only after a successful write. Duplicate creation, switching to uncreated slots, and creation during a suspended rift are refused. Class-equality fallbacks now include gender; replay kits use their real gender slot.

Created-only aggregation covers unlocks, first-clear rewards, recommendations, operations rewards, rune grades, tutorial history/exemption and skill activation. Daily quests and telemetry reject uncreated selected heroes; codex, offline supplies and repeat state preserve ID ownership and reject uncreated owners. D4 allocation, Hunt Edict editing and potion preparation reject hidden uncreated targets. Queued forge jobs reject uncreated owners and do not settle for them. All six slots still undergo item/ownership/reference integrity checks. The attendance seed still uses the original first hero ID.

The shared UI owner must update settings/legacy character lists, the controller’s hardcoded index clamp, the Hunt Edict preview fallback, and D4 host class lookup. `Single(class)` is invalid with six heroes; select the active matching hero ID or a created same-class fallback and disable unavailable class tabs. The Korean record provides exact locations and proposed substitutions. Character selection state/UI tests belong to PR2.

Validation: `git diff --check` passed. Thirteen focused `HeroSlotsTests` were authored for migration, exact backup preservation, archive failure, creation rollback/retry, female ownership, lock rules, aggregate/queued forge exclusion and legacy rejection. Existing schema/count/refund/tutorial assertions were adapted. **No Unity compilation, EditMode execution, runtime smoke, physical-device checks or performance measurement was performed.** Integrate PR1, shared UI dependencies and PR2 before the single final validation. No original checkout, real saves, Library or build outputs were touched.


## PR2 follow-through

The production constructor now opts into a single created male Warrior when D4 is enabled. Fresh legacy developer accounts retain three created heroes, since legacy mode has no six-slot creation command. `GameStore.NewAccount` keeps its three-created-male default for explicit existing-account fixtures. Existing multi-hero tests now call `ContentTestAccounts.ExistingMaleRoster` at the point that prerequisite is needed; first-start, creation and migration expectations remain separate. Two cross-hero unlock fixtures were also corrected after independent review. Two additional `HeroSlotsTests` cases cover the production/legacy first-account split and a different existing migration archive (15 total). These tests compile but await Unity execution.

PR2 supplies the settings and legacy town roster (created heroes only, with gender), plus the Hunt Edict preview gender fallback. Item integrity still traverses all six owned bags. Integration owns `GameController.SelectHero`, D4 class host/tabs and the texture budget.
