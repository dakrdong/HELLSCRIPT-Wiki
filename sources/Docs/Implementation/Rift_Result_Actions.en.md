# Rift result actions and battle graph

Updated: 2026-10-02 · [한국어](Rift_Result_Actions.md)

Rift result actions now connect reward claims, acquired equipment changes, graph inspection and retry to the actual saved state.

- **Claim Reward** commits `GameStore.ClaimRiftFirstRewards` immediately, then shows only the saved box grants over a black dim: icons, names, counts and **Tap anywhere to close**. No claim confirmation or follow-up popup appears. Already granted gold, XP and loot are not granted again; boxes are not automatically opened. Failed saves do not show success.
- **Empty skills** show only **Skill unassigned**, including the ultimate. The shared `RiftSkillShareView` creates no empty icon or frame in rift details and training results.
- **Acquired equipment** retains its acquisition snapshot while **Compare with equipped** and **Equip** resolve current ownership by ID. `EquipmentComparisonView` and `ItemComparison` provide the common ring/dual-wield target selector. Existing `EquipmentSlots` and `GameStore` transactions commit equipment changes. Automatically stored equipment is retrieved through `Storage.Move` and equipped in one transaction. Disposed or missing equipment is never recreated.
- **View Graph** replaces the previous-record text comparison with this battle's graph. It shares the training ground's existing `TrainingDpsPanel`, `TrainingChartView`, `TrainingDpsChart`, three-second DPS window and cast inspection. Damage uses actual HP loss, as in training; cooldown icons and all-cast tooltips keep the same rules. Rift checkpoints and completed reviews own their per-second samples and release times. Old saves without samples show an explicit unavailable-record message.
- **Repeat settings** appear dim when repetition for this result is off. A press shows a two-second **Repeat hunting has not been configured** bubble. The configured result's information page has no header settings shortcut.
- **Retry** keeps the result open and displays 5, 4, 3, 2, 1 with **Tap to cancel** on the button. Another press cancels entry. Manual retry takes over the automatic reservation for this result while preserving the hero's repeat configuration for the next battle. Nested windows and lost app focus pause the timer. Save failures or closing the result prevent entry.

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
