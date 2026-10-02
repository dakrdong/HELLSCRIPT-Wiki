# Rift result actions and battle graph

Updated: 2026-10-02 · [한국어](Rift_Result_Actions.md)

Rift result actions now connect reward claims, acquired equipment changes, graph inspection and retry to the actual saved state.

- **Claim Reward** commits `GameStore.ClaimRiftFirstRewards` immediately, then shows only the saved box grants over a black dim: icons, names, counts and **Tap anywhere to close**. No claim confirmation or follow-up popup appears. Already granted gold, XP and loot are not granted again; boxes are not automatically opened. Failed saves do not show success.
- **Empty skills** show only **Skill unassigned**, including the ultimate. The shared `RiftSkillShareView` creates no empty icon or frame in rift details and training results.
- **Acquired equipment** retains its acquisition snapshot while **Compare with equipped** and **Equip** resolve current ownership by ID. `EquipmentComparisonView` and `ItemComparison` provide the common ring/dual-wield target selector. Existing `EquipmentSlots` and `GameStore` transactions commit equipment changes. Automatically stored equipment is retrieved through `Storage.Move` and equipped in one transaction. Disposed or missing equipment is never recreated.
- **View Graph** replaces the previous-record text comparison with this battle's graph. It shares the training ground's existing `TrainingDpsPanel`, `TrainingChartView`, `TrainingDpsChart`, three-second DPS window and cast inspection. Damage uses actual HP loss, as in training; cooldown icons and all-cast tooltips keep the same rules. Rift checkpoints and completed reviews own their per-second samples and release times. Old saves without samples show an explicit unavailable-record message.
- **Repeat settings** appear dim when repetition for this result is off. A press shows a two-second **Repeat hunting has not been configured** bubble. The configured result's information page has no header settings shortcut.
- **Retry** keeps the result open and displays 5, 4, 3, 2, 1 with **Tap to cancel** on the button. Another press cancels entry. Manual retry takes over the automatic reservation for this result while preserving the hero's repeat configuration for the next battle. Nested windows and lost app focus pause the timer. Save failures or closing the result prevent entry.

## Live battle DPS, 2026-10-02

Rift battles use the existing training DPS HUD. Creation, layout and refresh in `GameUI.TrainingGround`, `TrainingChartView` and `TrainingDpsChart` are shared; no separate graph or damage collector is added.

- The same rules show the three-second DPS window, average/peak/total damage, time series and cooldown-skill/ultimate cast icons. The source is the current Rift's `RunState.dps`. Pausing combat stops samples and combat time.
- The comparison line and delta read the latest retained successful run with real samples for the same hero and tier. They exclude the current run, other characters/tiers, failures and legacy records without samples. No eligible record displays the existing no-previous-run message.
- The arrow folds to one live-DPS line and expands again. Training and Rifts share the session-only fold choice, including HUD rebuilds. Wide layouts place it beside the minimap; compact strips sit below the header/map. The default position stays clear of boss status; a deliberate user position takes precedence.
- Resumed legacy saves without samples show `—` and the unavailable-record message, without recalculating historical damage from current equipment. Town, results and mandatory tutorials retain no live panel. Training-specific pause/stop dialogs remain on the training path; Rifts retain their existing observation-menu pause.

Validation passed: 66/66 related Edit Mode tests, shared UI ownership and 11/11 contract tests, macOS development build and native runtime acceptance. Korean/English × 440×956 portrait, 956×440 landscape, and 1600×900, 1600×1000, 1680×720 PC × three minimap modes cover 30 combinations. Checks include safe bounds, boss/map/log separation, pointer folding/unfolding, retention after HUD rebuild and recorded-data agreement. The original training graph and pause/resume also passed. Mobile safe areas and pointer input were simulated on macOS, not physical iOS/Android. The first runtime attempt stopped because the harness did not await asynchronous Rift admission; after correcting the wait, only the failed runtime check was repeated with the required rebuild. The full game test suite was not repeated. Four URP post-processing shader warnings are retained in the evidence; there were no managed exceptions.

[Validation summary](RiftResultActionsEvidence/live-dps-validation.json) · [Edit Mode](RiftResultActionsEvidence/live-dps-editmode.xml) · [Shared UI](RiftResultActionsEvidence/live-dps-ui-contract.txt) · [Runtime record](RiftResultActionsEvidence/live-dps-runtime.txt)

[PC Korean](RiftResultActionsEvidence/live-dps-rift-dps-1600x900-ko.png) · [PC English](RiftResultActionsEvidence/live-dps-rift-dps-1600x900-en.png) · [Portrait Korean](RiftResultActionsEvidence/live-dps-rift-dps-440x956-ko.png) · [Portrait English](RiftResultActionsEvidence/live-dps-rift-dps-440x956-en.png) · [Landscape Korean](RiftResultActionsEvidence/live-dps-rift-dps-956x440-ko.png) · [Landscape English](RiftResultActionsEvidence/live-dps-rift-dps-956x440-en.png) · [Folded](RiftResultActionsEvidence/live-dps-rift-dps-folded-1600x900-ko.png) · [Shared training panel](RiftResultActionsEvidence/live-dps-training-shared-dps-1600x900-ko.png)

## DPS position and battle header, 2026-10-02

- Drag the live DPS title with a mouse or touch. The fold arrow remains a separate control.
- `DpsHudPosition` atomically saves the drop location in the device-only `hellscript-dps-position-v1.json`. Rifts and training share it and restore it after HUD rebuilds and game restarts. Account, character and battle saves remain separate. A failed save shows a message; another drag retries it.
- The top-left anchor is normalized to the safe area. Resizing, rotation and folding clamp the visible panel into safe bounds above the skill/potion/log controls without overwriting the stored anchor.
- Boss name, action and health stay at the top centre; narrow screens use the central row immediately below the header. Moving the graph cannot push boss status downward.
- Power Saving and Return to Portal sit side by side to the left of Settings, with the observation menu to their left. The minimap begins below the shortcut captions.

Validation passed **13/13** related Edit Mode tests and **11/11** shared UI contract tests. The first native attempt exposed a drag handle hidden behind the bottom HUD. After limiting the usable bottom bound, the affected drag regression passed **1/1** and the affected native acceptance was rerun. The final build had zero errors and the final player had no managed exceptions. KO/EN x five viewports x three map modes covered 30 drag/fold/rebuild/top-alignment cases, and a fresh process restored the position. Ten entry layouts verified the help popup's actual next unlock tier without changing selected tier 4. The fixture's next unlock was tier 5; real accounts pass their actual next tier, such as 10. No full game suite or physical-mobile run was performed.

English-table parsing and runtime translation coverage passed **2/2**. The orphan-key check failed on two entries already present at the baseline commit (`{0}단계 · {1}` and `기록 목록`); these are recorded separately from the change.

[Scope and source hashes](RiftResultActionsEvidence/dps-drag-validation.json) · [Runtime](RiftResultActionsEvidence/dps-drag-runtime.txt) · [Fresh process](RiftResultActionsEvidence/dps-drag-relaunch.txt) · [Localization baseline](RiftResultActionsEvidence/dps-drag-localization-baseline.json)

![Top-centred boss and right-side shortcuts](RiftResultActionsEvidence/dps-hud-default-1600x900-ko.png)

[Moved graph](RiftResultActionsEvidence/dps-drag-1600x900-en.png) · [Portrait](RiftResultActionsEvidence/dps-drag-440x956-en.png) · [Landscape](RiftResultActionsEvidence/dps-drag-956x440-ko.png) · [Restart](RiftResultActionsEvidence/dps-drag-relaunch-1600x900-ko.png)

## Shared UI adapters

`RiftRewardRevealWindow` and `RiftCombatGraphWindow` started with `tools/new_content_ui.py` and use `ContentWindowView` for safe areas, input and pause leases. The reward reveal intentionally hides the standard chrome and shows only the black dim and centered receipt. It has no separate canvas, equipment calculation or save owner. Its committed `RewardSnapshot` is presentation-only; tapping only dismisses it.

## Validation

### 2026-10-02: Editor result initialization failure after killing the boss

When the repeat-settings button had no `CanvasGroup`, `GetComponent<CanvasGroup>() ?? AddComponent<CanvasGroup>()` did not recognize Unity Editor's null object. Assigning alpha in `StyleRepeatSettings` first threw `MissingComponentException`, preventing `ContentWindowView.Open` from returning. `Update` line 47 subsequently repeated `NullReferenceException` because the result view had never been assigned.

The shared styling path now uses `TryGetComponent`, matching the existing window host. Both portrait and landscape add the component only when absent and reuse it otherwise. Dimming, the clickable unconfigured hint and configured-state styling remain unchanged.

An actual Unity Editor regression covers first creation without the component, enabled settings, cancellation, reuse of one component and clickability while dimmed. **19/19** `RiftResultTests`, the shared UI contract and its 11 tests passed, with no new C# compilation errors. [Current Editor test evidence](RiftResultActionsEvidence/canvas-null-editmode.json).

The merged-main macOS development build completed with zero errors. One existing result-actions smoke used an isolated save to verify boss completion and the result, ten KO/EN viewport combinations, the unconfigured repeat hint, claims, equipment, graphs and manual retry. The player emitted `HELLSCRIPT_RIFT_RESULT_ACTIONS_OK`, exited zero, had none of the reported exceptions and entered a new battle after 5→4→3→2→1. The full game suite and physical-mobile checks were not repeated. [Current scope and source hashes](RiftResultActionsEvidence/canvas-null-validation.json) · [Build result](RiftResultActionsEvidence/canvas-null-build.json) · [Runtime actions](RiftResultActionsEvidence/canvas-null-runtime.txt) · [Retry input trace](RiftResultActionsEvidence/canvas-null-retry.txt) · [Landscape Korean result and repeat hint](RiftResultActionsEvidence/canvas-null-result-wide-ko.png) · [Portrait English result](RiftResultActionsEvidence/canvas-null-result-portrait-en.png).

The shared UI contract and its 11 tests passed. **64/64** tests from `RiftResultTests`, `TrainingGroundTests` and `TrainingGroundUiTests` passed. After latest main added an equipment recommendation hook, only the directly affected result equipment transactions were rechecked: **2/2** passed. Unchanged coverage was reused.

The native macOS development player verified default text size and KO/EN at **440×956, 956×440, 1600×900, 1600×1000 and 2100×900**, with mobile safe areas simulated on macOS. Actual UI raycast checks precede synthetic pointer input for graph inspection, comparison, equipping, claim, dismissal and cancellation. An isolated save readback confirmed equipment and one first-clear box. Result chart layout samples are a representative fixture; the separate training regression advances live combat and checks graphs, release timing, restart and edict changes. The focused native retry observed **5→4→3→2→1**, cancellation and a new battle after the existing online admission finished. The build had zero errors. No physical mobile device was verified.

The full Edit Mode suite ran once and completed 4,960 tests, but **did not pass overall**. All 25 capped failures returned by MCP match the pre-existing failure list. No final result object was returned, and domain reload discarded the detailed totals; full passed/failed/skipped counts are unavailable. The full suite was not repeated. The 64 directly related tests have complete XML results.

[Scope and source hashes](RiftResultActionsEvidence/validation.json), [related test XML](RiftResultActionsEvidence/related-editmode.xml), [integrated equipment checks](RiftResultActionsEvidence/integrated-equipment.xml), [layout progress](RiftResultActionsEvidence/layout-progress.txt), [retry input report](RiftResultActionsEvidence/runtime-actions.txt), [5-to-1 and entry trace](RiftResultActionsEvidence/retry-diagnostic.txt), [training regression](RiftResultActionsEvidence/training-runtime.txt), [save readback](RiftResultActionsEvidence/save-readback.json), [full suite job](RiftResultActionsEvidence/full-editmode-job.json), [baseline failures](RiftResultActionsEvidence/baseline-failures.json).

![Committed reward receipt over the black dim](RiftResultActionsEvidence/reward-wide-ko.png)

![Shared training DPS chart and cast inspection](RiftResultActionsEvidence/graph-wide-ko.png)

[Portrait result](RiftResultActionsEvidence/result-portrait-ko.png) · [English result](RiftResultActionsEvidence/result-wide-en.png) · [Portrait equipment detail](RiftResultActionsEvidence/loot-detail-portrait-en.png) · [Landscape comparison](RiftResultActionsEvidence/loot-comparison-landscape-en.png) · [Equipped receipt](RiftResultActionsEvidence/loot-equipped-ko.png) · [Portrait reward](RiftResultActionsEvidence/reward-portrait-en.png) · [Portrait retry](RiftResultActionsEvidence/retry-portrait-en.png) · [Four and cancel](RiftResultActionsEvidence/retry-four.png) · [Portrait graph](RiftResultActionsEvidence/graph-portrait-en.png).

During the new Rift 15 rune introduction, Injel Mir requires reward claiming and opens only the **Rune Block Set** in the same transaction. After saving, its emblem flies to the content menu and the existing five-step playable guide follows, with NPC explanations before every step. Other result rewards follow the rules above. See the [Rune Board unlock tutorial](Rune_Board_Unlock_Tutorial.en.md).
