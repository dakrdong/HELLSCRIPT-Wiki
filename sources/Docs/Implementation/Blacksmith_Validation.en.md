# Blacksmith implementation validation

Updated: 2026-10-01 · Validation date: 2026-09-22

[한국어](Blacksmith_Validation.md) · [Implementation, resources and migration](Blacksmith_Unity_Integration.en.md)

## 2026-10-01 Button and layout corrections

Autosave emitted a successful-save notification every three seconds, and the forge recreated controls even when its displayed state was unchanged. New buttons faded from their initial enabled colour to the disabled colour, causing flashes; a control could also be replaced during a press. Only saves that change displayed state now redraw the forge, and new controls receive their final availability and lock colours before the first rendered frame.

The supplied screen's 45 G was below the 500 G cost of a Lv.1 reroll, so both buttons had been disabled without an explanation. Reroll now explains the shortage, and auto reroll opens settings with cost, owned gold and missing gold. Paid execution remains blocked when unaffordable. Slot actions show stone cost, owned stones and the shortage. The equipment stat comparison fits its text instead of stretching vertically, and removing the equipment-crafting guide leaves four service tabs.

| Check | Result |
| --- | --- |
| Focused Edit Mode | `BlacksmithTests`, `ForgePresentationAllocationTests`, `UiButtonTests`, `LocalizationTests`: **107 passed, 0 failed/skipped**, 9.29 s. [Completed job and individual cases](BlacksmithFix20261001Evidence/editmode.json) |
| Full forge runtime check | Passed once: quoted +10/max upgrades, actual spending and save, affix lock and auto pause/continue/stop, slot jobs/unlock/free completion/settlement, NPC button and E key, five shapes in KO/EN, and reload. [Result](BlacksmithFix20261001Evidence/runtime.txt) |
| Additional shortage check | After clearing a stale notice that overlapped auto settings, only the affected coverage was checked again: no spending with 45 G, settings open with paid start blocked, stone quote, settled initial disabled colour, and unchanged controls across autosave and a held press. No clipped shortage text or duplicate notice in five KO/EN shapes. [Result](BlacksmithFix20261001Evidence/feedback.txt) |
| Four-tab navigation | Passed once in the final player: four exact lock conditions, 2-second hints, keyboard/pointer/back handling and preserved selection, all four unlocked tabs, removed tab absent, five KO/EN shapes. [Result](BlacksmithFix20261001Evidence/navigation.txt) |
| Contract, compilation and build | UI contract passed; 11 contract tests passed. Final macOS development build succeeded with 0 errors and 176 warnings. [Build result](BlacksmithFix20261001Evidence/build.json) |

Default text size was checked at 440×956, 956×440, 1600×900 (16:9), 1440×900 (16:10), and 1680×720 (21:9). [Compact equipment comparison](BlacksmithFix20261001Evidence/gear-pc-ko.png) · [Portrait gear](BlacksmithFix20261001Evidence/gear-portrait-ko.png) · [Auto shortage](BlacksmithFix20261001Evidence/auto-pc-ko.png) · [English portrait](BlacksmithFix20261001Evidence/auto-portrait-en.png) · [Slot cost](BlacksmithFix20261001Evidence/slot-pc-ko.png) · [Landscape slot cost](BlacksmithFix20261001Evidence/slot-landscape-ko.png) · [Four tabs](BlacksmithFix20261001Evidence/four-tabs-ko.png). [Evidence and source hashes](BlacksmithFix20261001Evidence/manifest.json).

One incremental player for the additional check failed before UI initialization because URP shader resources were missing. A full asset refresh and clean build to a new output path recovered startup, and the shortage and navigation checks passed. Successful transaction/save and 107-test coverage was reused; the full game suite, other content smokes and enlarged-text checks were not rerun. Existing stripped post-processing shader logs and Editor AI subscription errors are outside this UI fix. Evidence uses an isolated save in a native macOS development player with synthetic Unity input; physical mobile devices were not tested.

## 2026-09-30: HTML prototype layout

The frame, tabs, locks and five service screens in the [integration record](Blacksmith_Unity_Integration.en.md) were operated in a macOS development player. The results below were produced once, after the last code change.

| Check | Result |
| --- | --- |
| Full forge smoke `-hellscriptBlacksmithSmoke` | Pass: gear +10 and max, affix reroll (first-roll confirmation, auto pause/continue/stop), two slot jobs, confirmed workstation unlock, free 59-second finish, settlement and tooltip, NPC range, dialogue and E key, portrait 440×956 / landscape 956×440 / PC 16:9·16:10·21:9 in Korean and English, and saved state after a restart |
| Navigation smoke `-hellscriptForgeNavigationSmoke` | Pass: exact condition text for all five locked services, no page switch or transaction, the bubble gone after **2 seconds**, uniform button size/role, five window sizes × two languages |
| Core crafting smoke `-hellscriptCoreCraftingSmoke` | Pass: seven window shapes × two languages, filters, detail, +/- buttons, 80% cap, craft, confirmation, history and the pending result after a restart |
| Access smoke `-hellscriptBlacksmithAccessSmoke` | Pass: fresh character, use during a saved rift, and a real enhancement transaction on equipment chosen through the portrait sheet, persisted |
| Focused Edit Mode | Localization · Blacksmith · CoreCrafting · ContentUnlock · StoredLocalization: 165 of 165 passed |
| Full Edit Mode | 4,830 of 4,878 passed, 48 failed. 47 of the failures are exactly the pre-existing 2026-09-26 baseline list (unrelated to this change); the other one was a single `RiftContentUnlockTests` case still expecting core crafting at the old rift 120, so the test was updated to 50 and that class plus `ContentUnlockTests` then passed 62 of 62. The rest was not rerun |
| Text and contract checks | English missing/orphan entries 0 (the `l10n_check` emulation and the Unity tests); `check_ui_contract.py` passes; `test_ui_contract.py` 11 tests pass |

On `main` (4bd93ff3) the full forge smoke failed at once at the town fixture (tutorial map), and the NPC dialogue and rift unlock smokes fail the same way. The forge smoke's fixture (tutorial completion, hidden attendance popups, entry through the dialogue) was fixed and now passes end to end. The NPC dialogue (`-hellscriptNpcDialogueSmoke`) and rift unlock (`-hellscriptRiftUnlockSmoke`) smokes still fail at the same place as on `main` and are outside this change. The shared UI smoke (`-hellscriptSharedUiSmoke`) stopped at an earlier screen (`RuntimeSharedUiSmoke.cs` line 85) before reaching the forge, so its forge part (slot size, column width) was adapted to the new layout but not run.

Evidence screens: [landscape reroll](BlacksmithLayoutEvidence/landscape-ko-tab0.png) · [slots](BlacksmithLayoutEvidence/landscape-ko-tab1.png) · [gear](BlacksmithLayoutEvidence/landscape-ko-tab2.png) · [cores](BlacksmithLayoutEvidence/landscape-ko-tab3.png) · [portrait reroll](BlacksmithLayoutEvidence/portrait-ko-tab0.png) · [slots](BlacksmithLayoutEvidence/portrait-ko-tab1.png) · [gear](BlacksmithLayoutEvidence/portrait-ko-tab2.png) · [cores](BlacksmithLayoutEvidence/portrait-ko-tab3.png) · [PC reroll](BlacksmithLayoutEvidence/pc-ko-tab0.png) · [slots](BlacksmithLayoutEvidence/pc-ko-tab1.png) · [gear](BlacksmithLayoutEvidence/pc-ko-tab2.png) · [cores](BlacksmithLayoutEvidence/pc-ko-tab3.png) · [English landscape](BlacksmithLayoutEvidence/landscape-en-tab0.png) · [English portrait](BlacksmithLayoutEvidence/portrait-en-tab0.png). Locks: [landscape bubble](BlacksmithLayoutEvidence/locked-landscape-hint.png) · [portrait bubble](BlacksmithLayoutEvidence/locked-portrait-hint.png) · [all locked, PC](BlacksmithLayoutEvidence/locked-all-pc.png). Dialogs: [equipment sheet](BlacksmithLayoutEvidence/portrait-ko-sheet.png) · [lock confirmation](BlacksmithLayoutEvidence/landscape-ko-confirm.png) · [auto reroll](BlacksmithLayoutEvidence/landscape-ko-auto.png) · [workstation sheet](BlacksmithLayoutEvidence/portrait-ko-slots-jobs.png) · [growth plan](BlacksmithLayoutEvidence/landscape-ko-slots-plan.png) · [core quality](BlacksmithLayoutEvidence/portrait-ko-cores-chosen-coins.png) · [craft result](BlacksmithLayoutEvidence/landscape-ko-cores-result.png). Run records: [full smoke](BlacksmithLayoutEvidence/runtime-smoke.txt) · [navigation](BlacksmithLayoutEvidence/runtime-nav.txt) · [core crafting](BlacksmithLayoutEvidence/runtime-core.txt) · [access](BlacksmithLayoutEvidence/runtime-access.txt).

Real phone touch, rotation and performance, and Android/iOS builds were not checked. The results are a macOS player with synthetic input.

## 2026-09-29: Suspended runs without a slot snapshot

On 2026-09-29 a macOS development build (`main` `b1989282`) threw `IndexOutOfRangeException` in `HeroStats.ApplySlotGrowth` (`HeroStats.Blacksmith.cs:46`) when the town bag opened, through `GameUI.ShowPlayInventory` → `InventoryWindow.EquipmentStats` → `EquipmentPreviewHero`. The local guest save held a suspended run of hero 0 whose `slotLevels` was an empty array, not null.

A running battle keeps the slot enhancement levels it started with in `RunState.slotLevels`. A run saved before that field existed has no such field, and `JsonUtility` reads a missing array and a null array alike as an empty array. The old code treated only `null` as "no snapshot", so it copied the empty array, and `ApplySlotGrowth`, which reads all ten slots, threw. The same cause broke equipping and unequipping in town (`GameStore.ChangeEquipment`), resuming the run (the `CombatSimulation` constructor) and saving the hunt edict in town (`CombatSimulation.PrepareSuspendedHuntEdictChange`). Character switching and the server verifier's resume go through the same constructor.

- The rule now lives in one place, `BlacksmithCatalog.SlotLevels(run, hero)`. It uses the run's snapshot only when its length equals the number of blacksmith slots (10). A missing, empty or wrong-length array means the hero's current levels apply.
- All four places that read `slotLevels` and fall back to the hero go through it: the bag preview, town equipment changes, battle construction and resume, and the town edict edit. Everything else reads the `State.slotLevels` that the battle constructor settled, so a running battle still keeps the snapshot it started with.
- The save format and keys are unchanged, and older saves load as before. Resuming a run without a snapshot stores the hero's levels at that moment as its snapshot.
- Layout and text are unchanged.

### Missing-snapshot validation

- Reproduction: four new `BlacksmithTests` cases reload a run saved with an empty or a three-entry array, open the real bag and read its max HP, then equip in town, resume the run and save the edict in town. All four failed on the unfixed code, and the bag case stopped at the reported `ApplySlotGrowth` ← `HeroStats` ← `InventoryWindow.EquipmentStats` frames. [Before XML](LegacySlotSnapshotEvidence/editmode-before.xml)
- Per call site: with the helper kept, each of the four call sites was restored to the old code in turn. Every restored site made the case that passes through it fail. [Results](LegacySlotSnapshotEvidence/mutation.txt)
- 381 related Edit Mode tests (blacksmith, suspended edict saves, three inventory suites, rift entry, authoritative verifier, settings revisions, restore fidelity, current build saves, repeat hunts, combat journal, stored localization, growth, action continuity): 338 passed and 43 failed after the fix. The same 43 fail by name on the unfixed code, and all of them are in the baseline failures recorded on 2026-09-27. On the unfixed code the four new cases fail as well, 47 in total. [After](LegacySlotSnapshotEvidence/editmode-focused.xml) · [Before](LegacySlotSnapshotEvidence/editmode-focused-base.xml)
- Full Edit Mode: 4,686 of 4,734 passed and 48 failed, with none skipped (1,853 s). All four new cases passed. 47 of the 48 failures match the baseline failures recorded on 2026-09-27. The remaining `TutorialProgressionTests.EdictAndNewSkillRequireCommittedChangeThenMatchingTrainingAndFreshRift` fails with the same message when the runtime files are restored to the old code. [Summary](LegacySlotSnapshotEvidence/editmode-full-summary.json) · [Base check](LegacySlotSnapshotEvidence/editmode-tutorial-base.xml)
- macOS development build: zero build errors. The new `-hellscriptLegacyRunBagSmoke` uses the save's run when it has no slot snapshot, or builds that state on a fresh account. It opens the town bag at 440×956 Korean, 956×440 English and 1600×900 Korean, and checks that max HP equals the value from the hero's own levels and that every attribute is listed. It then equips one bag item, resumes the run and checks that the battle and the saved file use the hero's levels. [Build](LegacySlotSnapshotEvidence/build.txt)
  - Copy of the reported save: passed. Max HP 621 was shown, the equip was saved, and the resumed run continued at 38 kills and 273/621 HP. The original save file was not modified. [Runtime](LegacySlotSnapshotEvidence/runtime-user-save.txt)
  - Fresh account with the chest slot at level 40: passed. Max HP 2,749 reflects level 40, not the default level 1. [Runtime](LegacySlotSnapshotEvidence/runtime-fixture.txt)
  - A build with only the five runtime files restored to the old code exited with code 1 on the same save copy, with the reported exception as soon as the bag opened. [Runtime](LegacySlotSnapshotEvidence/runtime-before.txt)
- Text-size changes, PC 16:10 and 21:9 and physical mobile devices were not checked in this work, because the layout did not change. No `mcpforunity://instances` resource was available in this session, so Unity MCP was not used. The checks ran in batch mode on a project clone and in the macOS player.

[Saved-game bag 1600×900](LegacySlotSnapshotEvidence/bag-save-1600x900-ko.png) · [Portrait 440×956](LegacySlotSnapshotEvidence/bag-save-440x956-ko.png) · [English 956×440](LegacySlotSnapshotEvidence/bag-save-956x440-en.png) · [Fresh-account bag](LegacySlotSnapshotEvidence/bag-fixture-1600x900-ko.png) · [Resumed rift](LegacySlotSnapshotEvidence/resumed-save.png)

## 2026-09-29: Locked services and consistent navigation

Reroll, slot enhancement, equipment enhancement, core crafting and equipment crafting now share `UiButtonRole.Tab` and the common theme. All five controls have the same dimensions in landscape. Portrait uses two rows of three and two equally sized controls. The separate small footer crafting button is removed. Once unlocked, equipment crafting still opens the existing crafting page.

- Access and explanations come directly from `ContentUnlocks.Has` and `ContentUnlocks.Condition`. Unlock thresholds and grandfathered account access remain unchanged. Equipment crafting follows `RareCraft`.
- Locked services have a dark face and an illustrated brass lock. Pointer clicks and keyboard submission remain available for explanations; navigation and transactions are blocked.
- A small bubble below the pressed button shows the real requirement for three seconds, stays inside the safe frame and does not intercept input. Repeated activation renews the timer; Esc dismisses the bubble first.
- Locked activation preserves the current equipment, affix and scroll state. Rotation, language and text-size changes re-anchor an open bubble. Unlocking removes the lock and restores the existing route.
- A new account with no available service sees a short introduction without selecting a locked tab. Otherwise the first available service is used for the initial page.

The artwork source is the vector mesh in `UiLockGraphic.cs`. Following `UiButtonFace` and `StorageChestGraphic`, it draws the brass face, iron backplate, shackle, rivets and keyhole directly with shared theme colors and no input interception. No raster-generation model, external art or texture importer was used. The existing blacksmith canvas, fonts, safe area, window host and equal-width three-column content remain the owners.

### Locked-navigation validation

- Final focused Edit Mode: **110 passed, zero failed/skipped**, covering buttons, graphics setup, account unlocks and localization. [XML](ForgeNavigationEvidence/editmode.xml)
- The extended suite including tutorials passed 139 of 140 tests. `EdictAndNewSkillRequireCommittedChangeThenMatchingTrainingAndFreshRift` cannot find a level-3 active skill. The same failure was reproduced on base `412be7dc` with every blacksmith source change removed. Skill rules and that test were left untouched. [Extended results](ForgeNavigationEvidence/extended-editmode.xml) · [Baseline comparison](ForgeNavigationEvidence/baseline-skill-failure.xml)
- macOS development build: zero build errors. Native uGUI raycasts/events verified all five locked services, exact conditions, three-second expiry, keyboard submission, back handling, unchanged currency, R1 access, selected equipment retention, every unlocked service and the existing crafting route. [Runtime](ForgeNavigationEvidence/runtime.txt) · [Build](ForgeNavigationEvidence/build.txt)
- Verified 440×956, 956×440, 1600×900, 1440×900 and 1680×720 × KO/EN × 100%/150% text: 20 combinations of matching controls, text bounds and re-anchored hints. Only tutorial/attendance presentation was bypassed in the isolated QA account; content progression started at R0. No MCP Editor was connected, so existing batch/native tools were used. Physical mobile devices were not tested. [Scope and screenshot hashes](ForgeNavigationEvidence/verification.json) · [Source hashes](ForgeNavigationEvidence/source-fingerprints.json)

[All locked on PC](ForgeNavigationEvidence/all-locked-pc.png) · [Landscape hint](ForgeNavigationEvidence/locked-956-ko-100.png) · [Portrait](ForgeNavigationEvidence/locked-440-ko-100.png) · [English at 150%](ForgeNavigationEvidence/locked-440-en-150.png) · [Unlocked](ForgeNavigationEvidence/all-unlocked-landscape.png)

## Environment and evidence

Validation uses Unity 6000.6.0f1 Edit Mode tests and a macOS development player. Branch `codex/blacksmith-runtime` is an isolated checkout of the committed inventory foundation with `main` merged. The original running editor, uncommitted inventory work and existing saves are untouched.

The player uses a fixture account in a separate save directory. It raycasts actual UGUI button coordinates before dispatching pointer events, and also sends the E keyboard input. It checks real `GameStore` balances, upgrade levels, jobs and persisted state before and after interaction. Screenshots alone do not establish functional success.

The acceptance-only development flag temporarily keeps synthetic keyboard input enabled across macOS focus changes. Normal game launches retain their existing focus behavior.

## Results

- HTML behavior checks: **80 passed, 0 failed**.
- Focused Edit Mode suite covering blacksmith, resources, quality, attributes and localization: **178 passed, 0 failed/skipped**. Preserve the [XML](BlacksmithEvidence/blacksmith-focused-editmode.xml) and [summary/hash](BlacksmithEvidence/focused-summary.json). This overlaps the full suite and must not be added to its count.
- Integration Edit Mode suite after merging inventory `8a5c88d` and `main` `12e0a88`: **312 passed, 0 failed/skipped**, 99.25 seconds. Covers the forge, shared comparison/selection, inventory, shops, runes, graphics and town. [XML](BlacksmithEvidence/blacksmith-integration-editmode.xml) · [summary/hash](BlacksmithEvidence/integration-summary.json). Counts overlap earlier runs.
- macOS development build succeeded with 0 build errors. Native acceptance exited with `HELLSCRIPT_BLACKSMITH_SMOKE_OK`; see [results](BlacksmithEvidence/runtime-result.txt) and the [69-capture hash manifest](BlacksmithEvidence/runtime-manifest.json).
- Final native inventory integration acceptance also exited with `HELLSCRIPT_INVENTORY_SMOKE_OK`. It covers shared inventory/shop/storage comparisons, ring baselines, shared persisted salvage auto-selection and battle input gating in Korean/English portrait/landscape. [Results](BlacksmithEvidence/inventory-runtime-result.txt) · [retained capture hashes](BlacksmithEvidence/inventory-runtime-manifest.json) · [comparison](BlacksmithEvidence/inventory-20-shared-inventory-landscape-ko.png).
- All **30 transparent PNGs** passed alpha-channel, transparent-pixel, content and hash checks.

Full Edit Mode regression: **2,880 passed, 0 failed/skipped** in 1,518.18 seconds. Preserve the [full XML](BlacksmithEvidence/blacksmith-full-editmode.xml) and [summary](BlacksmithEvidence/full-summary.json). The full suite ran on the forge snapshot before the latest inventory merge. The subsequent boss-stage check and shared-comparison integration are covered by the latest 312-test run. Native macOS acceptance verifies language-switch retention and the final integrated UI.

The existing URP build configuration logs stripped Lens Flare/Panini post-processing shaders. Forge UI, input and persistence checks passed; this does not claim validation of those post-processing effects.

## Screen evidence

| Reference | Representative captures |
| --- | --- |
| 440×956 | [Affix list](BlacksmithEvidence/portrait-ko-tab0-list.png), [details](BlacksmithEvidence/portrait-ko-tab0.png), [slot growth](BlacksmithEvidence/portrait-ko-tab1.png), [equipment](BlacksmithEvidence/portrait-ko-tab2.png), [English slots](BlacksmithEvidence/portrait-en-tab1.png), [English equipment](BlacksmithEvidence/portrait-en-tab2.png) |
| 956×440 | [Enchanting](BlacksmithEvidence/landscape-ko-tab0.png), [slots](BlacksmithEvidence/landscape-ko-tab1.png), [equipment](BlacksmithEvidence/landscape-ko-tab2.png), [English roadmap](BlacksmithEvidence/landscape-en-growth.png) |
| PC 16:9 | [Slot growth](BlacksmithEvidence/pc-16x9-ko-tab1.png) |
| PC 16:10 | [English enchanting](BlacksmithEvidence/pc-16x10-en-tab0.png) |
| PC 21:9 | [Equipment enhancement](BlacksmithEvidence/pc-21x9-ko-tab2.png) |
| States | [Auto target match](BlacksmithEvidence/interaction-auto-match.png), [Lv.75 preview](BlacksmithEvidence/interaction-preview-75.png), [unlock tooltip](BlacksmithEvidence/interaction-unlock-tooltip.png), [pulse 1](BlacksmithEvidence/interaction-working-bright.png), [pulse 2](BlacksmithEvidence/interaction-working-pulse.png) |
| Fixed dialogs | [Affix candidates](BlacksmithEvidence/portrait-ko-candidates.png), [growth plan](BlacksmithEvidence/portrait-ko-growth.png) |

All 69 captures are included under `Artifacts/Blacksmith/RuntimeEvidence/` in the resource package. Representative captures above are also versioned in Git.

## Validation matrix

| Area | Checks | Evidence |
| --- | --- | --- |
| Quotes | Prices at levels 1–100; +10 sum; affordable maximum and remainder; +97→100; insufficient/stale quotes | `BlacksmithTests` |
| Transactions | Repeated request IDs, failed writes, currency overflow, unchanged rejected state | `BlacksmithTests` |
| Quality/migration | Legacy +5, constant gains after awakening/masterwork, affix lock and investment records, masterwork at +5 or higher | Blacksmith and quality tests |
| Enchanting | First paid slot lock, constant repeated price, actual candidates/ranges, auto match/pause/continue/stop/close | Core and player checks |
| Workstations | Per-character levels, two account-shared stations, sequential 10/50/250-diamond unlocks, duplicate rejection | `BlacksmithTests` |
| Time | Destination-level costs/durations, exactly-once offline settlement, 60 seconds costs 1; 59/1 seconds free | Core and player checks |
| Combat | Frozen run growth, hero levels for older runs without a snapshot, next-run application, 70% elemental resistance cap, effective learned-active/equipped-passive levels | Core, skill and combat regression tests, `BlacksmithTests`, macOS bag smoke |
| Salvage | Ordinary legendary 10, unique-effect 15, set 10 in single/bulk/automatic paths; cores and persistence retained | Nine route/reward combinations |
| Rift/sweep | Stage-based stones, one-time pickup, duplicate sweep rejection | Blacksmith and resource tests |
| Access/layout | NPC range guard, interaction button and E key, three equal columns, portrait navigation, full-frame dialogs | Player checks |
| Details | Live preview values, three-second unlock tooltip, working pulse | Player state checks and captures |
| Locale/ratios | KO/EN, 440×956, 956×440, PC 16:9/16:10/21:9, shared inventory grade colors | Localization tests and captures |

## Reproduction

HTML: `node --test Prototypes/Blacksmith/*.test.cjs`.

Unity: use `-batchmode -nographics -runTests -testPlatform EditMode -testResults <xml>`. Add `-testFilter Hellscript.Tests.BlacksmithTests` for the focused suite.

Build the macOS development player through `Hellscript.Editor.ProjectBuilder.BuildMac` with `-hellscriptBuildOutput <app path>`. Run it with `-hellscriptBlacksmithSmoke -hellscriptSavePath <new fixture directory> -hellscriptScreenshots <evidence directory>`. It exits after acceptance; success requires `HELLSCRIPT_BLACKSMITH_SMOKE_OK` and `result.txt`. Omit these arguments for normal gameplay.

The bag check for runs without a slot snapshot runs with `-hellscriptLegacyRunBagSmoke -hellscriptSavePath <save directory> -hellscriptScreenshots <evidence directory>`. A copy of a `hellscript-local-v1.json` that holds such a run is used as is; an empty directory gets the same state on a fresh account. Success requires `HELLSCRIPT_LEGACY_RUN_BAG_SMOKE_OK` and `runtime-legacy-run-bag.txt` in the evidence directory.

`Artifacts/Blacksmith/Play-Blacksmith.command` in the current worktree launches manual play using the separate fixture account, without sharing the original account's save directory.

Resources: run `python3 tools/blacksmith/package.py` to create `Artifacts/Blacksmith/HELLSCRIPT-Blacksmith-Resources.zip`, including per-file SHA-256 hashes. This package contains resources and integration references. Integrate the complete Git branch for the runnable game.

## Scope and publication

Mobile proportions are macOS windows. Physical iPhone/Android touch, keyboards, rotation, cutouts and performance require device validation. The project retains its local save adapter; no server-authoritative time or economy implementation is claimed.

Do not publish this feature branch. The `main` merger regenerates and validates the wiki from merged `main`, publishes the mirror and verifies GitHub Pages. Public destination: [HELLSCRIPT Wiki](https://dakrdong.github.io/HELLSCRIPT-Wiki/#/tree).
