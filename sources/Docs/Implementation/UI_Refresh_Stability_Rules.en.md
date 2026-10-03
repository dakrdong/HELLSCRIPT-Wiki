# HELLSCRIPT UI refresh recurrence rules

Verified: 2026-10-03

These rules add only causes found in this audit to the Shared UI contract and CPU Performance Review Rules. Scope and verification gaps are in the [stability audit](UI_Refresh_Stability.en.md).

1. **Distinguish a successful save from a display change.** Periodic Save also publishes Committed. New subscribers compare displayed dependencies before repainting. Plain Save can contain meaningful changes, so do not discard every save. Audit reads such as inventory/build/presets/protection/unlocks/availability; exclude unrelated timestamps/content. Domain transactions retain independent live-state checks.
2. **Identical button state must not restart visual transitions.** Equal role/chosen/locked/chrome/availability/hover/pressed/focus preserves progress and mesh. Start the first mesh in its final state. Preserve real input, selection and availability changes.
3. **Do not repaint a commit already shown.** Explicit action/tab/locale/layout repaint records the rendered baseline. Coalesce notifications; defer background control replacement during press/touch/drag/InputField editing/dropdown/child modal. When replacement is necessary, preserve named-path input modality/focus and the owner's scroll restoration.

Run `python3 tools/check_ui_refresh.py` and `StoreViewBindingTests` during review. A guard-presence check does not prove dependency correctness or all-screen visual acceptance. Add new windows/tabs/popups to the source checklist and verify unchanged autosave, meaningful plain Save, save during press, child windows, repeat tabs/reentry, rapid clicks, KO/EN and failed-save/duplicate receipts in their actual runtime.

For performance claims, run at least three matched actual before/after binaries and preserve repaint/button creation-removal/CPU/GC raw data. Label a same-modified-binary key-disabled synthetic control separately. When no measurements exist, record cost changes as unmeasured.

Final native integration exposed another invalidation cause: advancing `AccountGuide.tutorialRun.realTime` changed the equipment key and replaced controls on autosave. Equipment dependencies must not serialize the whole guide or live combat record. Retain `Tutorials.Mandatory(account)` and `armorId`, which determine actual sell/store/unequip protection. `LiveTutorialTimeKeepsEquipmentControlsButArmorProtectionStillRefreshes` checks real tutorial saves with increasing time preserve buttons while armor protection changes still refresh them.
