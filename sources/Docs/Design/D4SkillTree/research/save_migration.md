# D4 skill tree: save, migration and level-cap impact map (read-only)

> **2026-10-28 A2 correction:** References to the large edict window, ≡ menu and their dedicated components or smokes are historical. Use the [current entry points](../../../../Assets/HELLSCRIPT/Runtime/Presentation/GameUI.HuntEdict.cs) and [replacement screens and source owners](../../../Implementation/Hunt_Edict_Window_Removal_20261028.en.md). Earlier captures and test counts describe the earlier implementation.

**Paths.** W = `/Users/t8g-2410-pn-005/Documents/MyProject/HELLSCRIPT/wt-d4-skill-plan` (branch `claude/d4-skill-tree-plan`; runtime code is identical to main a3fe67c5). Aliases:
- `C` = `W/Assets/HELLSCRIPT/Runtime/Core`
- `P` = `W/Assets/HELLSCRIPT/Runtime/Presentation`
- `T` = `W/Assets/HELLSCRIPT/Tests/Editor`
- `R` = `W/Assets/HELLSCRIPT/Resources`
- `D` = `W/Docs/Design/D4SkillTree`

I did not run Unity. Everything below comes from reading code, `git log -S` and the spec JSON. `D/golden/csharp/` is untracked in W.

## 0. Findings that change the plan

1. **Put the migration in `GameStore.Read()`, before `Normalize(a)` (`C/GameStore.cs:68-70`).** Do not put it in post-load `ActivateSkillTrees` (`C/GameStore.SkillTree.cs:26-47`).
   - A post-load change goes through `Write()`. `HuntEdictProgression.ValidateTransaction(edictBoundary,…)` (`GameStore.cs:399-401`, `C/HuntEdictProgression.cs:193-216`) treats it as a player edit.
   - If that throws ArgumentException, `Write` reverts the heroes to the boundary and returns false (`GameStore.cs:445-450`). Startup then throws (`P/GameController.cs:65-67`) and the app does not start.
   - `edictBoundary` is captured after Read (`GameStore.cs:53`), so a Read-time migration is invisible to it.
2. **Only `NotSupportedException` aborts safely inside Read (`GameStore.cs:103`).** Any other exception returns null (`:104`), and the constructor falls back to `.bak` (`:40-43`). A failed migration would silently load the older generation. Wrap failures as `:162-163` does.
3. **`Write()` stamps `schema=MaximumSchemaVersion` on every save (`:402`).** The 21→22 bump is the only old-binary guard (`:62`). A raw `level>40` (`:64`) returns null, so an old binary would fall back to `.bak`.
4. **Adding any serialized field to `ClassSkillLoadout` or `SkillInvestment` makes every old HED4/HED5 code undecodable.** `ClassSkillCodec.Decode` compares canonical JSON (`C/ClassSkillCodec.cs:27-49`).
5. **Keep `ClassSkillLoadout.Canonical` version-preserving; convert at account level.** Hundreds of tests use version-2 "validation" heroes (`classSkillValidation=true`, level 40). Examples: `T/ClassSkillRuntimeTests.cs:23-30`, 12 refs in 9 files.
6. **`Rank(id)` returns 1 for ids missing from `ranks` (`C/ClassSkills.cs:84`).** That is wrong for v5, where unlearned means 0.
7. **After refund-all, a level-40 hero cannot buy an ultimate.**
   - Points equal level (40), and the T5 gate needs 40 spent outside T5 (`D/golden/reference/rules.js` `before/apply`). The first ultimate point needs level 41.
   - Tutorial H08 and the "Lv.40" strings assume 40.
8. **Three separate level-cap spots.**
   - `LevelCap` is 30 for unmigrated heroes and 40 otherwise (`ClassSkills.cs:62`).
   - Read has a raw 40 (`GameStore.cs:64`).
   - `Validate` has a raw 40 (`ClassSkills.cs:137`).
9. **Persisted hero copies hold old loadouts.** `PuzzleLevelProgress.kitHero` (`C/PuzzleTutorial.cs:30`) is a full `HeroSave` per cleared puzzle level. `TrainingComparisonRecord.baselineHeroJson` is another.
10. **Balance1000 hashes the core files.** An edit to `GameStore.cs`, `ClassSkills.cs`, `ClassSkillTree.cs`, `Economy.cs` and others (`tools/balance_1000.py:222-231`) forces regeneration of the 12 MB `projection.json`.

## A. Current storage model

**Version constants**
- `GameStore.MaximumSchemaVersion=21` (`GameStore.cs:11`). `AccountSave.schema` defaults to 2 (`C/GameCatalog.cs:179`).
- `Normalize` raises schema to at least 7 when a loadout exists (`:162`). `CommitClassSkills` writes 7 into memory (`ClassSkillCodec.cs:76,78`), so in-memory `Data.schema` is unreliable between saves.
- `ClassSkillLoadout`: `format="CLASS_SKILLS_V1"` and `version` 1..4 (`ClassSkills.cs:75-76,99`).
  - 1 is ids with a shared WP18.
  - 2 is slot-based passives and is the field default.
  - 3 is the tree with free base rank 1.
  - 4 is the tree, zero-based.
- `HuntEdictLoadout.version` is 1 (legacy 12-slot) or 2 (tree) (`C/HuntEdictLoadout.cs:10,32`). `HuntEdictPreset.version=1` (`:78`).
- `ClassSkillRuntimeState.version=1`, `format="CLASS_SKILL_RUN_V1"` (`C/ClassSkillState.cs:41-42`, `C/ClassSkillPersistence.cs:22`).
- `HuntEdictV2` Schema 2 / Content 1 / Global 8 / Option 2 / Policy 1 (`C/HuntEdictV2.cs:28`).
- `ClassSkillTree.json`: `version 1`, `levelCap 40`, `ultimateFrom/To 30/39`. `ClassSkills.Data.version==1` (`ClassSkills.cs:52`).
- Share codes: HED4 (`ClassSkillCodec.cs:17`), HED5 with `TreePayload.schema=4` (`C/HuntEdictPresetCodec.cs:22,36,52`), HED3 with `Payload.schema=3` (`:17`).

**`SkillInvestment {id; rank=1}`** (`ClassSkills.cs:72`)
- There is one entry per catalog skill, 37 per class.
- It is validated only inside `Canonical` (`:122`): the count must equal the catalog size, ids must be distinct, and `MinimumRank<=rank<=maxRank`.
- It has no migration of its own.
- v5 adds `modifier` and `modifierRank`. JsonUtility keeps field initializers for absent keys (proof: `T/GemServiceTests.cs:76-80` strips `gemCapacity` and gets 50). So `modifier=-1` is the correct default; 0 would mean "modifier 1 chosen".

**`ClassSkillLoadout`** (`ClassSkills.cs:73-173`)
- Fields: `format, version, heroClass, actives[4], passives[3], order[], automatic[], ultimate, ranks, options{id,choice}, presetSelections{scope,preset}, legacyEdict`.
- `IsAbsent` (`:86`) means version ≤2 with everything empty, because JsonUtility materializes missing objects.
- `Canonical` (`:97-133`):
  - format/version whitelist `:99`
  - slots `:117-120`
  - v≥3 passives must be empty `:120`
  - v4 equipped skills need rank>0 `:123`
  - `order` must equal equipped actives plus ultimate plus BASIC exactly `:124-125`
  - `automatic` ⊆ that set `:126`
  - legacyEdict canonical and same class `:127-129`
  - `ClassSkillOptions.Validate` `:130`
  - `HuntEdictSkillPresets.ValidateSelections` `:131`
- `Validate(hero)` (`:134-149`): level 1..40 at `:137`; tree branch `:138-144` (equipped and ranked skills must be Unlocked, `Remaining>=0`); departing needs an ultimate at ≥40 at `:147`.
- Migrations:
  - v1→2 runs inside `Canonical` (`:101-115`, WP18→WP19, "preserve total spending").
  - v2→3 is `ClassSkillTree.Upgrade` (`C/ClassSkillTree.cs:62-72`): clears passives and refunds below-requirement ranks to 1.
  - v3→4 is never automatic. It happens by explicit `ResetAllocation` (`:31-37`) or `StartNewHeroesWithoutSkills` (`GameStore.SkillTree.cs:13-23`).
  - An absent loadout becomes one through `ClassSkillLoadout.FromHero` (`ClassSkills.cs:87-96`), which reads legacy `skillRanks/activeSkills/passives/edict`.

**`ClassSkillTree.cs`**
- `Uses` (`:25`) is v3||v4. `MinimumRank` `:26`. `Spent` `:27`. `Remaining` `:28` = `max(0,level-(v4?0:1))-Spent`. `UnlockLevel` `:30`.
- Fixed-point `Available` (`:40-58`) clamps level to 0..40 with cache key `heroClass*41+level` (`:42`).
- `Equip` `:79-98`. `ChangeRank` `:108-117`.
- `Display` regex `[WAM]P?\d{2}|BASIC` (`:119-120`):
  - `A06-2` renders as "name-2".
  - `WP20` matches, but `Name()` returns the raw id because new ids are not in the 111 catalog.

**`GameStore.SkillTree.cs`**
- `StartNewHeroesWithoutSkills` (`:13-23`) is `ResetAllocation(FromHero)` plus `ApplyHuntEdict` for brand-new accounts. This is exactly the all-refunded result wanted.
- `ActivateSkillTrees` (`:26-47`) stages a copy, skips heroes with `classSkillValidation`, a v3/v4 loadout, or a suspended run (`:30,34`), and keeps `edictLegacyBuild??=build.Copy()` (`:35`). It calls `ApplyHuntEdict` (`:37`), then `Write(staged)` (`:41`) and adopts (`:42-43`).

**Save classes** (`C/GameCatalog.cs`)
- `BuildConfig` `:54-79`: `classSkills` `:56`, legacy `activeSkills` `:59`, `passives` `:76`, `skillRanks[12]` `:77`.
- `HeroSave` `:122-155`: `level` `:135`, `build` `:138`, `presets` (5 BuildConfig) `:145`, `edictPassiveSlots` `:148`, `edictPresets` (5) `:149`, `edictLegacyBuild` `:151`, `trainingComparison` `:131`, `trainingGround` `:132`, `classUltimateRemaining/Total` `:127`.
- `AccountSave` `:157-199`: `transactions` `:194`, `suspendedRun` `:195`, `repeatHunt` `:196`.
- `RunState`: `classSkills` runtime state `:333`, `build` `:423`.

**Legacy 12-slot projection**
- `ProjectLegacy` (`ClassSkills.cs:162-172`) writes:
  - `activeSkills` only for ids with `LegacyIndex>=0`, i.e. W01-06/A01-06/M01-06, indices 0..17 (`:66-68`).
  - `passives` for legacy-flagged ids only.
  - `skillRanks[12]=Max(1,Rank(SkillProgression.Id(class,i)))` (`:168`): i<6 is xP01-06, i≥6 is x01-06 (`C/SkillProgression.cs:23-30`).
  - `rules` filtered to equipped bridge skills.
- `ProjectEdict` (`:150-161`) feeds `HuntEdictV2Document`. Its `slots/skillOrder/skills` can hold only x01-x06+BASIC (`C/HuntEdict.cs:56-60`, `HuntEdictV2.cs:44-60`). Non-bridge ids (W07+, A07+, M07+) live only in the loadout.
- Of the bridge ids only M05 disappears. A06 is reused.
- The edict also stores skill ids in `survival.defenseSkill` (W04, W05, A03, M05, M06), `survival.escapeSkill` (W02, A04, M04) and `survival.fallbackSkill` (also M05) (`C/EdictOptions.cs:63`, validated `:26-27`).
- It also stores them in per-skill option values, e.g. A06 option 3 = BASIC|A01|A02 (`C/HuntEdictV2.Options.cs:93`).

**`ClassSkillCodec`**
- `Encode` (`:13-18`) is `HED4.`+base64(`ToJson(Canonical)`)+sha256, capped at 65536 bytes.
- `Decode` (`:19-52`) checks the hash, then requires `Encode(value)==code`. Otherwise it accepts only:
  - retired preset labels (`:34-35`)
  - older `legacyEdict.globalVersion` (`:36-41`)
  - omitted `"options":[]` / `"presetSelections":[]` (`:46-48`)
  - everything else throws "Noncanonical…".
- `Import` (`:53-64`) routes HED4 or the legacy `HuntEdictPresetImport` path.
- HED5 wraps a HED4 string inside `TreePayload`, and `JsonUtility.ToJson(tree)!=json` is checked strictly (`HuntEdictPresetCodec.cs:52-58`).
- `HuntEdictPreset.originalCode` stores the received code text in the save (`C/HuntEdictLoadout.cs:80`).

**`HuntEdictDefaults`** (`C/HuntEdictDefaults.cs:11-34`)
- `Skills()` runs `Canonical`, clears `options` and `presetSelections`, sets `order=automatic=equipped+BASIC`, and replaces `legacyEdict` with a style template. That template is built from `SkillProgression.BaseRanks()` (`:23`).
- `Resolve` is detached; it never writes the save.
- It is safe once v5 `Canonical` exists.

**Presets**
- `HuntEdictQuickPresets` (`C/HuntEdictQuickPresets.cs`):
  - The catalog `Resources/HuntEdictQuickPresets.json` has 211 presets, 54 `skill/<id>` scopes (24 on removed ids) and 47 `retired` pairs (`:65`, mapped to "custom").
  - The identity-order preset hard-codes `{"W15","W07","W11","W12","A18","A15","A05","M15","M12","M11"}` (`:112`).
  - `Apply` (`:97-142`) writes legacy edict options plus `classSkills.options/automatic`, not ranks.
- `HuntEdictSkillPresets` (`C/HuntEdictSkillPresets.cs`): `ValidateSelections` (`:22-35`) throws if the scope's skill is unknown; `CopyPolicy` (`:62-91`) copies policy only.
- Saved presets (`HuntEdictPreset`/`BuildConfig`) copy the whole loadout, allocation included.
  - `hero.presets[i].classSkills` is re-canonicalized at `GameStore.cs:182-188` and again in `Write` (`:408`).
  - `edictPresets[i].loadout` is canonicalized at `C/HuntEdictStorage.cs:68`.

## B. Save pipeline

**Paths**
- Saves are local only. `<dir>/hellscript-local-v1.json` plus `.bak` and `.tmp`.
- `<dir>` is `Application.persistentDataPath` or `-hellscriptSavePath` (`P/GameController.cs:45-46`). Per profile it is `accounts/guest-<32hex>` or `accounts/google-<64hex>`, indexed by `account-profiles-v1.json` (`C/AccountProfiles.cs:13-45,70-101`).
- Account switching opens another directory via `OpenAccountStore` (`P/GameController.Accounts.cs:21-25`): constructor + `StartNewHeroesWithoutSkills` + `ActivateSkillTrees` + `Save`. This is a second migration entry, and every old profile directory needs it.
- No server copy of `AccountSave` exists. Server stores sessions, live settings and combat summaries/payloads (`server/sites/README.md`; `C/ContentUnlocks.cs:131`). `server/combat_analytics.sql:21-22` reads `$.journal.initialBuild.classSkills` and `$.review.finalBuild.classSkills`.
- WebGL flushes via `FS.syncfs` (`Plugins/WebGL/PlayerAuthentication.jslib:36,40`).

**Order inside `Read(file)`** (`GameStore.cs:57-105`)
1. Parse `:61`.
2. Newer-schema throw `:62`.
3. Structural null (heroes==3, cores==8) `:63`.
4. `level<1||>40` null `:64`.
5. `level>LevelCap` or classSkills-present-but-not-Enabled throw `:65-66`.
6. `MigrateRetiredGemStacks` `:68`.
7. `HuntEdictProgression.Normalize(a,legacy:true)` `:69`.
8. `Normalize(a)` `:70` (`:119-169`). Per hero it runs `HuntEdictV2Storage.Normalize`, `NormalizePresetSlots`, `HuntEdictStorage.Normalize`, then `ClassSkillLoadout.Validate`+`ProjectLegacy` (`:159-164`); runs get `ValidateRun` (`:166`).
9. Archive `.before-content-unlocks-v2.json` `:71-75`.
10. Items and gems (schema≥2) `:76-95`, with archive `.gem-recovery-<guid>.json`.
11. `.schema{n}-before-hunt-edict.json` when schema<6 `:96-100`.

The constructor (`:32-56`) tries main, then `.bak`, then throws `InvalidDataException` if either exists but is unreadable, else creates a new account (`:40-43`). It runs `Migrate` for schema 1 (`:44-48,106-118`), then `CaptureEdictBoundary` `:53`, then `SettleLocalIdle` `:286-312`, which is the first persist of any migration.

**Registry**
- There is none. Migrations are imperative blocks keyed by `schema<N` or module version stamps, ordered by position in `Read`/`Normalize`.
- Examples of version stamps: `Tutorials.cs:116` (`schema>=17` gate), `OfflineSupplies.cs:40`, and the `HuntEdictV2.GlobalVersion` waterfall `EquipmentRecommendation.Upgrade` (`C/EquipmentRecommendation.cs:35-83`).

**Unknown newer versions**
- Account schema >21: `NotSupportedException`, file untouched, no `.bak` fallback (`:62,103`).
- Loadout version >4: `Canonical` ArgumentException, wrapped as NotSupported (`:162-163`).
- Runs: `ClassSkillPersistence` wraps errors as NotSupported, "original save is preserved" (`:55-56`).
- `GameController` shows the message and disables itself (`P/GameController.cs:67`).
- `CombatJournalData.saveSchema=MaximumSchemaVersion` (`C/CombatJournal.cs:152`) is not validated by the server.

**Backup and rollback**
- `Write` writes `.tmp`, then `File.Replace(tmp,path,path+".bak")` (`:430-436`).
- `.bak` holds only the previous generation. After the second save the pre-migration bytes are gone.
- Only an explicit copy survives: `File.Copy(file, file+".schema"+n+"-before-…json", false)` guarded by `!File.Exists` (`:71-75,96-100`), or a Guid-suffixed variant (`:88-90`).
- There is no downgrade. An old binary refuses schema 22, and the archive is the manual rollback artifact.

**Receipt / refund**
- Refund is derived, not stored: `Remaining=level-Spent`. A v5 loadout (`version==5`) is itself the idempotence marker.
- One-time notice, option 1 (transient): a `GemRecoveryMessage`-style property set in Read and merged into `Notice` at `P/GameController.cs:69-70`. It shows once because the next save no longer migrates.
- One-time notice, option 2 (persistent, if it must wait for the first window open per plan ch.7): a per-hero struct, e.g. `{version,fromVersion,level,refunded,acknowledged}`.
  - JsonUtility materializes absent classes, so test absence by content, as `HuntEdictV2Storage.IsAbsent` does (`C/HuntEdictV2.Storage.cs:12`).
  - Acknowledge via `Transact("d4-skill-ack:"+heroId,"d4-skill-ack",…)` like `AcknowledgeOfflineSupplies` (`GameStore.cs:313-315`). `EconomyReceipt{requestId,operation,committedUtc}` is at `GameCatalog.cs:201-205`.
- In-save backup exists as precedent (`edictLegacyBuild`, `:151`). For D4 prefer the file archive only, because old loadouts would not validate under the new catalog.

**Pattern to copy**
- *Closest, retire-id migration with archive and notice* (git 5a33ecef, 93995a0f, 8a54d626, 2026-09-23):
  - `C/GameStore.Gems.cs:8-27`. It is static, load-only, and also rewrites `suspendedRun` and `repeatHunt.pendingResult`.
  - `GameStore.cs:68,83-94` and `P/GameController.cs:69-70`.
  - Tests `T/GemSocketTests.cs:266-298`.
- *Loadout version bump* (git 7dc11ff1, v4, 2026-09-28). It touched:
  - `ClassSkillTree.cs` `Uses/MinimumRank/Spent/Remaining/ResetAllocation`.
  - `ClassSkills.cs` `Enabled`, the `Canonical` whitelist (`:99`), passive rule (`:120`), min rank (`:122`), equip rule (`:123`), `Validate` (`:138`), `RankScale` (`:65`), `ProjectLegacy` (`:168`).
  - About 20 Edict/CombatSimulation files (`Unlocked`→`Learned`), `SkillTreeIntegrationTests`, docs.
- *Per-hero reset*: `StartNewHeroesWithoutSkills` (`GameStore.SkillTree.cs:13-23`).

## C. Hero level cap (40 → 60)

**Safe to raise, formula-driven (no new data)**
- `ClassSkills.cs:62` `LevelCap`. Callers: `C/Economy.cs:128,238`, `P/GlobalHudSnapshot.cs:50`, `HuntEdictV2.cs:91`, `C/HuntEdictImport.cs:29`, `C/CombatSimulation.cs:15`, `GameStore.cs:65`.
- Raw 40s: `GameStore.cs:64`, `ClassSkills.cs:137`, and the legacy-only `ClassSkills.cs:147` and `ClassSkillTree.cs:42`.
- Stat curves are linear in level, with no tables: `Economy.cs:162,168,180`.
- EXP is a closed-form formula (`Economy.cs:235-241`). No table is needed for 41-60.
  - X(40)=25,255 and X(59)=54,040.
  - Cumulative XP 1→40 is 333,645; 1→60 is 1,109,495 (3.33×); 40→60 is 775,850.
  - `LiveOpsConfig.experienceMultiplier` (`C/LiveOpsConfig.cs:19`) is the only remote XP knob.
- Rift, enemy and item scaling do not read hero level. They are stage-based (`CombatSimulation.cs:543`). `EquipmentShop.LevelCap` is already 60 (`C/EquipmentShop.cs:48`).

**Needs a semantic change, not just 40→60**
- *Ultimate "Lv.40" gates:*
  - `P/RiftEntryWindow.cs:338,345,349,356`
  - `P/TrainingGroundWindow.Skills.cs:42`
  - `C/Tutorials.cs:56,214` (H08)
  - `R/Localization/en.txt:5758,5837`
  - The T5 gate makes the first ultimate possible at level 41.
- *Max-level UI uses a literal 30:* `P/GameUI.Growth.cs:20,36`.
- *Level bands:* `P/SkillTreeGraphic.cs:15` (`BandStarts={1,10,20,30,40}`), `P/HuntEdictWindow.Skills.cs:110-203,434`, `en.txt:5300`. They are replaced by tiers.
- *Level-based skill unlock lives in four catalogs:*
  - `GameCatalog.cs` constructors and `R/GameCatalog.asset` (legacy 18, unlock 1..30; pinned by `HuntEdictV2.cs:94`)
  - `R/Data/ClassSkills.json` (unlock 1..40)
  - `R/Data/ClassSkillTree.json` (node levels 1..40)
  - hard-coded `{1,3,6,10,15,20}` at `C/CombatSimulation.ClassSets.cs:25`
- *Gating code that compares unlock to level:* about 25 `SkillUnlockLevel>EffectiveLevel` sites via `ClassSkillTree.UnlockLevel` (`ClassSkillTree.cs:30`, `CombatSimulation.ClassSkills.cs:24`), plus `CombatSimulation.ClassSkillPolicies.cs:133` and `ClassSkills.cs:145-146`.
  - In D4 learned means rank>0, so a learned W09 (old unlock 26) would never fire unless these return "unlocked".
  - Also affected: `GlobalHudSnapshot.cs:80`, `GameUI.Growth.cs:19,40,41,67`, `GameUI.InventoryLayout.cs:191`, `CombatSimulation.Growth.cs:31`.

**Fixed-40 fixtures to decouple from `LevelCap`**
- `C/CombatSimulation.SkillPreview.cs:31`
- `C/CombatSimulation.ClassSkillValidation.cs:33` (report text `:24`)
- `P/HuntEdictWindow.CombatPreview.cs:90` (`en.txt:6356`)
- `FullSkillTraining` (`CombatSimulation.cs:14-15`, `Economy.cs:128`). Training copies silently become level 60 and change training DPS and record keys (`C/TrainingGround.cs:127`).
- 20 Runtime smoke files and 20 test files set `level=40`.

**Pins and hashes**
- `ClassSkills.Data` hard check: 111 skills and 123 gear (`ClassSkills.cs:52`), plus unlock 1..40 (`:55`).
- `tools/class_skill_runtime.py` asserts 111/123/60/55 and `playerEnabled=False`. `tools/class_skills.py:31,48,73,86-90` pin ultimate level 40 and one ultimate. `tools/class_skill_options.py` asserts 55/166.
- Balance1000:
  - `tools/balance_1000.py:222-231` hashes `Economy`, `GameStore`, `ClassSkills`, `ClassSkillTree`, `GameStore.SkillTree`, `CombatSimulation`, `ContentUnlocks`, `Presentation/GameController`, `ClassSkills.json`, `ClassSkillTree.json` and more into `projection.json.source_hashes`.
  - `test_balance_1000.py:93-94` asserts those hashes are current, and `--check` fails "Stale generated file". Any edit forces `python3 tools/balance_1000.py`, which rewrites `projection.json`, `dashboard.html`, tables and `validation.json`.
  - `Docs/Design/Balance1000/parameters.json` has `hero_level` reaching 40 at stage 100 and staying there to stage 1000.
  - `balance_1000.py:312-315` copies the XP formula.
- `production_skill_tree_audit.json` has 9 rows (3 heroes × level 1/30/40) with `skillTreeVersion:3` and `levelCap:40`. Only level-1 rows are read (`test_balance_1000.py:19-27`); the cap does not affect them.
- `tools/wiki.py:147` (`tree_levels[id]` KeyError if a node vanishes), `:302` (`expected skills:18, passives:18`), `tools/test_wiki.py:37-46,84,134-135`.
- Test pins: `T/SkillTreeIntegrationTests.cs:59` (37 nodes) and "ultimates require forty" (~`:56-66`); `T/ClassSkillRuntimeTests.cs:59` (37/16/19); `T/ClassSkillBoundaryTests.cs:48` (37 ranks) and `:69` (LevelCap==30); `T/ClassSkillPassiveTests.cs:124`; `T/GrowthTests.cs:88-92` (cap 30); `T/IdleHuntStatusTests.cs:85`.
- `ClassSkillTree.json levelCap` has no reader (only the DTO at `ClassSkillTree.cs:15`), but the file is hashed by Balance1000.

## D. Migration design check

**Facts.** The 111 legacy ids split as:
- 54 keep their id. This includes A06 and M07, which are reworked and keep their id.
- 22 are absorbed as modifiers.
- 2 actives are removed: M13 and M15.
- 33 passives are removed.

So 57 ids vanish as standalone. The 6 key passives (WP20/21, AP20/21, MP20/21) and 84 modifier ids do not exist in `ClassSkills.json`. The default decision (plan D4/D10) is to refund everything.

**Algorithm.** `SkillTreeD4Migration.Apply(AccountSave)`: public static (the test assembly has no InternalsVisibleTo), pure, no I/O, and it returns a report. Call it from `Read()` before `Normalize`, and from `GameStore.Sandbox` and tests. `Sandbox` bypasses Read (`C/GameStore.Sandbox.cs:8`), and fixtures are loaded straight into `AccountSave` (`T/PuzzleContentTests.cs:93-94`).

Input: the account plus kept/removed sets derived from `legacyRemap`. Ship `legacyRemap` (or its sets) in `SkillTreeD4.json`; the plan's generator does not list it.

For each hero with `!classSkillValidation` and loadout version <5 (absent counts as 0):
1. `old = absent ? ClassSkillLoadout.FromHero(h) : Canonical(h.build.classSkills)`. This needs the legacy 111 catalog and `ClassSkillTree.json`, so keep them. Alternatively, read through a lenient DTO that needs only `heroClass, options, presetSelections, automatic, legacyEdict`.
2. Refunded points: for v3/v4, `ClassSkillTree.Spent(old)`. For v≤2 or absent, `SkillProgression.Spent(skillRanks)` (`SkillProgression.cs:20`). Points after migration equal `level`; formula `Remaining = level - (Σrank + ΣmodifierRank + Σpassive)`.
3. Build N, a v5 loadout:
   - `ranks` is always the full 20 entries in spec order, all zero with `modifier=-1`. An empty list reads as an old save (`ClassSkills.cs:86`).
   - `actives=["","","",""]`, `ultimate=""`, `passives=["","",""]`.
   - `order=["BASIC"]`. `Canonical` forbids unequipped ids (`:124-125`), so a custom order is lost. The relative order of kept x01-x06 survives in `legacyEdict.skillOrder` (`:156`).
   - `automatic = old.automatic ∩ {"BASIC"}`.
   - `options`: drop the 25 groups on removed skills (M05, W07, W10-W16, A08-A13, A15, A16, M09, M10, M12-M16, `M13:goal`). `ClassSkillOptions.Validate` (`C/ClassSkillOptions.cs:48-53`) throws on unknown groups.
   - `presetSelections`: drop the 24 `skill/<removed>` scopes. Keep `order`, `basic/<class>` and `skill/<kept>`.
4. Edict documents (`hero.edict`, `N.legacyEdict`, each `edictPresets[i].loadout.edict`, `presets[i].classSkills.legacyEdict`):
   - Only M05 can differ. Set `slots` M05→"" and `survival.defenseSkill/fallbackSkill=="M05"`→"".
   - Leave inert M05 per-skill options, because `Canonical` needs all 7 ids.
5. Re-project the legacy build via the `StartNewHeroesWithoutSkills` call (`ApplyHuntEdict`, `GameStore.HuntEdict.cs:63-67`; `HuntEdictLoadout.ToBuild`, `:58-73`).
   - Result: `activeSkills=[]`, `passives=[]`, `skillRanks=BaseRanks()`, `edictPassiveSlots=["","",""]`, rules reduced to BASIC+loot/chest/explore.
   - Preserve `useEdict`, as `GameStore.SkillTree.cs:36-38` does.
6. Presets: apply step 3 to `hero.presets[i].classSkills` and `hero.edictPresets[i].loadout`, so they become policy-only. Set `loadout.passives=["","",""]`, `ranks=BaseRanks()`. Keep `name`, `equipmentIds`, `originalCode`, `legacyBuild`, `needsReview`.
7. Other hero containers:
   - `trainingGround.setup.excluded` (`C/TrainingGround.cs:22`): filter to kept ids.
   - `trainingGround.records[].previous/best.loadout` (`:33`): null it (`HasLoadout` at `:37` uses `Uses`).
   - `trainingComparison`: null it (stale baseline hero plus two builds).
   - `guide.equippedSkillHistory` (`C/FirstPlayGuide.cs:42`, used by F06 at `Tutorials.Progress.cs:15-17`): prune ids not in D4 data.
   - `classUltimateRemaining/Total`: unchanged (still within 45..60, `ClassSkillPersistence.cs:26`).
8. Account containers:
   - `guide.puzzle.levels[].kitHero` and `tutorialSlots[].puzzle` (`PuzzleTutorial.cs:30`, replay at `:225-226`): run steps 1-5 on the HeroSave copy, or null them.
   - `guide.tutorialRun`, `suspendedRun`, `repeatHunt.pendingResult`: see F1.
9. History is left untouched. `records[].journal.initialBuild/builds/initialEdict`, `review.finalBuild/finalEdict`, `review.dps.casts[].id` and `statistics.skills[].definitionId` are never canonicalized on load (`GameStore.cs:144-147`; `C/CombatJournalArchive.cs:36,54`). UI falls back to the raw id (`P/GameUI.History.cs:16`, `C/CombatJournal.cs:86`). Reused A06/M07 rows will show the new names.
10. Make the archive in Read before any write (`.schema{n}-before-d4-skill-tree.json`, guarded by `!File.Exists`). Set the notice or receipt. Let `Write` stamp 22. Throw only `NotSupportedException`.

**Idempotence**
- v5 heroes are skipped, and `Apply(Apply(x))` must be byte-equal. Compare `ResetAllocation(empty)` at `T/SkillTreeIntegrationTests.cs:200`.
- A mixed account (one hero deferred for a run) is legal, so `Read` must not treat schema≥22 with a v<5 hero as corrupt.
- Crash before `File.Replace` leaves the file unchanged with a stale `.tmp`. Crash after it leaves a file that is already v5.

**Load-time validation for v5.** Needed in `Validate`, presets, runs and codes:
- Exactly 10+10 entries in spec order.
- `rank<=maxRank` (5; ultimates 3).
- `modifier ∈ -1..2` (ultimates -1..1), with `modifier==-1` iff `modifierRank==0`.
- `modifierRank<=min(maxRank,rank)`.
- Passive rank>0 requires `unlockLevel<=level`, and at most one key passive has points.
- `Spent<=level`.
- Tier gates via `SkillTreeD4Rules.Valid` (`D/golden/csharp/SkillTreeD4.Rules.cs:46-50`).
- Equipped skills have rank>0, and slot 4 holds an ultimate.
- `level<=60`.

**What can still reference a removed id after migration**

| Structure | Where | Action |
|---|---|---|
| Loadout `options/presetSelections/order/automatic/ranks` | `ClassSkills.cs:77-82` | Rewritten in step 3. |
| Edict document (`slots`, `skillOrder`, `skills[]`, `survival.*Skill`) in `hero.edict`, `legacyEdict`, `edictPresets`, `TrainingAttempt.edict` | `HuntEdictV2.cs:15-27`, `EdictOptions.cs:63` | M05 only. |
| Edict rules and conditions | `Rule.skill` index (`GameCatalog.cs:28-47`), `HuntEdict.cs:20-35` | Index 16 = M05 stays valid. `ProjectLegacy` filters to equipped (`ClassSkills.cs:169`). |
| Recommended edict | `HuntEdictDefaults.cs:11-34` | Safe once v5 `Canonical` exists. |
| Tutorials | `Tutorials.cs:98-101` (W01/A01/M01, all T2 in the spec, so not learnable at level 1); `GuideTutorial.cs:13,77` (W01); `ClassPracticeLesson.cs:9` (A02/M02); `Tutorials.Progress.cs:15-17`; `FirstPlayRecommendation.cs:18-40`; `HuntEdictProgression.cs:101,165`; `R/PuzzleTutorial.json` (13 refs to MP01; kits W01-04, A01-04, M01-04, WP01, AP01, MP01); `PuzzleTutorial.cs:90-175` | Plan D7. Kits are a rule exception. |
| Quick edit HUD | `C/HuntEdictQuickMenu.cs:57-61`, `P/GameUI.SkillQuick.cs:89,132` | Iterate the 111 catalog and use `Learned/Remaining/Unlocked`. Route through the D4 engine. |
| Skill presets | Quick-preset JSON (24 dead scopes), `R/SkillPresetScenarios.json` (122 scenarios), `HuntEdictQuickPresets.cs:112` | Add retired pairs or drop. Rewrite the identity list. |
| Auto-battle options | `ClassSkills.json options` (55 groups, 25 dead), `ClassSkillOptions.cs:43-56` | Pruned in step 3. |
| Saved runs | `ClassSkillState.cs:39-53`: `cooldowns/intervals/casts/action/effects/dots/policies/intent` | `ClassSkillPersistence.cs:14-53` throws on unknown ids. `intent.skillId` may only be W02/M13 (`:42`). Persistent effects may only be an M13 channel or a W15 loan (`:47-48`). |
| In-run id strings | `statistics.skills`, `shields`, `statuses`, `effects`, `traps`, `projectiles`, `damageEvents`, `effectEvents`, `procHits` (`C/CombatEffects.cs:10-40`, `C/CombatActions.cs:51-61`, `GameCatalog.cs:258`); `cooldowns[18]`/`cooldownTotals[18]` indexed by bridge slot (`GameStore.cs:199`) | Not validated at load. |
| Receipts, replays, leaderboards | There is no leaderboard in the repo. `ClassSkillNativeReceipt` is dev/editor only (`C/ClassSkillNativeSmoke.cs:1,9-32`) and covers W18, A07, M11, M13, W02, W09, W15, M10, M12, A18, M05 plus WP18/19 (`:47-50,89`). `ClassSkills.Data.validationBuilds` has 30 builds. `AuthoritativeRiftVerifier` (`#if UNITY_EDITOR\|\|UNITY_SERVER`) embeds `AccountSave` in `HuntAuthorityCheckpoint` (`:7-14`). Server R2/D1 payloads keep old shapes. | Rewrite the smoke and validation builds. The analytics SQL must read both `ranks` shapes. |
| Stored text | `Loc.StoredText` plus `SkillTerms` built from catalog names (`CombatJournal.cs:93-100`) | Names deleted from the catalog stop translating in stored logs. |

## E. Test patterns to copy

- **Old save, reopen, assert, archive, idempotent:** `T/GemSocketTests.cs:266-291`. It builds the old state, covers `suspendedRun` and `repeatHunt.pendingResult` variants, asserts exact values and archive bytes, then saves, reopens and checks that only one archive exists. The negative case is `:293-298` (NotSupported, bytes unchanged).
- **Legacy schema archived:** `T/HuntEdictFoundationTests.cs:176-185` (`account.schema=5`, archive name asserted).
- **Real fixture:** `T/PuzzleTutorialCoreTests.cs:24-33` loads `T/Fixtures/PuzzleAfterL8-Mage.json`.
  - That fixture is 748 KB, schema 21. The learner hero is level 5 with a v4 loadout; the other two heroes have an absent loadout. It also carries `guide.puzzle.levels[].kitHero` copies at v4.
  - The test asserts the migrated learner build is unchanged and the second reopen is idempotent.
- **Schema guards:** newer schema refused with bytes unchanged at `T/GemServiceTests.cs:76-84`; unknown id refused at `T/ClassSkillCompatibilityTests.cs:106-111`; forged run state at `:114-120`; `T/TutorialProgressionTests.cs:182-188` (schema 16).
- **Loadout migration unit test:** `T/ClassSkillPassiveTests.cs:116-135`. It builds the old loadout, runs `Canonical`, then asserts version, rank count, points conserved and source unchanged. It also checks that a hand-built legacy HED4 code decodes to the same JSON, that an unknown field throws, and that a truncated `ranks` list throws.
- **Old share codes:** `T/ClassSkillPolicyTests.cs:82-90`, `T/HuntEdictSkillPresetTests.cs:146-153`, `T/SkillPresetCombatPreviewTests.cs:128-144` (loops over the `retired` list), `T/RecommendedEquipmentTests.cs:209-242` (HED3/4/5 envelope helper), `T/ClassSkillBoundaryTests.cs:41-50`.
- **Accounts at a level:** `T/SkillTreeIntegrationTests.cs:14-18` `Store(heroClass,level,newPlayer)`; `T/ContentTestAccounts.cs:13-45`; `T/GrowthTests.cs:17` `Fixture(heroClass,level)`; `CreateClassSkillValidation` (`C/CombatSimulation.ClassSkillValidation.cs:29-60`).

**Recipe for "old save X migrates to exactly Y"**
1. Before changing any class, capture real old artifacts from the current build and commit them as literal bytes (with `.meta`) under `T/Fixtures/D4Migration/`. Needed: v4, v3 (free ranks), absent/legacy, an account with a suspended run carrying W15/M13 casts, 5 presets, and HED3/4/5 codes. Do not rebuild them with the new classes. After the change `ToJson` emits the new fields, so helpers like `OldCode` (`SkillPresetCombatPreviewTests.cs:128`) and `EncodeLegacy` no longer produce a true old wire.
2. Expected output Y: an inline or `.expected.json` loadout, plus the allocation string and FNV-1a hash from the golden C# port (`Encode()/Hash()`, `D/golden/csharp/SkillTreeD4.Model.cs`). The empty allocation is `0,-1,0;`×10`|0,0,0,0,0,0,0,0,0,0`.
3. Test body: write the fixture to a temp dir as `hellscript-local-v1.json`, then `new GameStore(dir,catalog)`. Assert:
   - the loadout JSON equals Y
   - `Save()` stamps schema 22
   - the archive bytes equal the fixture
   - the notice text
   - on reopen there is no second archive and the JSON is byte-equal
4. Also cover a progressive-disclosure account (`guide.edictLegacyAccess=false`, stage <6), to prove the Write gate does not reject the migration (finding 1).
5. Read goldens by path (`D/golden/README.en.md`): `Application.dataPath/../Docs/Design/D4SkillTree/golden/...`, and copy `D/golden/csharp/Tests/GoldenJson.cs` into `T/` (JsonUtility cannot parse them).
6. Property check: every `rules_golden.json` snapshot alloc round-trips through store, load and `Encode`, and passes `Valid` with `Spent<=level`.

## F. Open questions (not resolvable from code)

1. **Runs at migration.** Options for `suspendedRun`, `repeatHunt.pendingResult` and `guide.tutorialRun`:
   - (a) Defer the hero, as `ActivateSkillTrees` does today. This requires keeping v≤4 reader, catalog and combat alive.
   - (b) Convert the loadout to empty and sanitize `ClassSkillRuntimeState`, reusing the `ApplyHuntEdictChange` pattern (`C/CombatSimulation.HuntEdictChanges.cs:21-60`).
   - (c) End the run.
   - I recommend (b). Plan D3 allows investing mid-run.
2. **Old HED3/HED4/HED5 codes.** Import as policies only, or reject with a message? Use a new prefix (e.g. HED6, `TreePayload.schema=5`) so old clients fail at the prefix. The new decoder needs a legacy-wire branch.
3. **Legacy catalog lifetime.** Keep `ClassSkills.json` (111) and `ClassSkillTree.json` until the v1/v2 validation suites are rewritten? Plan step 7 cleanup conflicts with them.
4. **A06/M07 id reuse.** Old A06/M07 `options`, `presetSelections` and A06 edict options carry over silently. Keep or reset? The A06 buff mechanism is the same, so keeping looks plausible.
5. **Notice storage.** Transient (Read-time `Notice`) or persisted per hero with acknowledgement? Archive retention policy (about 0.75 MB per save, never deleted today)?
6. **`kitHero` copies and `trainingComparison`.** Migrate or drop?
7. **Level-40 players.** Refund-all means no ultimate until 41. Is that intended? H08 text and the "Lv.40" strings need the same decision.
8. **Fixed-level fixtures and the legacy cap.** Should training, preview and validation fixtures stay at 40 or use 60? Does the 30 regime for unmigrated heroes disappear?
9. **Balance.** Need a `hero_level` schedule to 60 in Balance1000. Is plan D9 (extend the XP formula as-is) final?
10. **Invalid v5 on load** (e.g. a later balance patch changes `tierGates`). Refuse (NotSupported) or repair by refunding top-down, as `Upgrade` does ("never destroy points")?
11. **`Rank()` default.** 0 for v5 only, or change it globally?
12. **Analytics.** `combat_analytics.sql` and any consumer of `classSkills` need both shapes (37-entry v≤4, 20-entry v5).