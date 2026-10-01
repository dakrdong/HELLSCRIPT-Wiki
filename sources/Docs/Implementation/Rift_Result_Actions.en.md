# Rift result actions and battle graph

Updated: 2026-10-01 · [한국어](Rift_Result_Actions.md)

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

The shared UI contract and its 11 tests passed. Final integrated Unity checks, macOS input and save verification will be recorded here after validation. Physical mobile devices are outside this run's verified scope.
