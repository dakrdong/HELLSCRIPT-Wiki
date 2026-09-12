# Review of Claude changes and follow-up fixes

Follow-up: exact target priorities, pursuit and persistence are recorded in [targeting and pursuit](Edict_Targeting_Expansion.en.md). The validation and remaining-work list below describe the end of the review stage.

Date: 2026-09-12 · Baseline commit: `22ddf0f` · Branch: `codex/claude-review-20260912`

## Scope and ownership

Compared the last September 9 validation manifest with the subsequent commits, source and recorded tests. Of the previous 149 C# files, 73 changed and 38 were added, leaving 187 at the review baseline. The review covered the hunt edict storage/editor/sharing/runtime bridge, town walking, localization and reading size, touch layout, defeat telemetry, balance runner and dungeon seal objective. This does not claim a fresh runtime audit of every change.

Claude's latest full report, `diag7.xml`, records 1,193 passing tests. The attribute system remains under active development in `worktree-d4-stat-sheet`. The user explicitly asked to avoid that work. This branch does not modify that worktree, its processes or its balance measurements, and does not merge its changes.

## Confirmed issues and fixes

| Issue | Fix |
|---|---|
| Independent loot choices were compressed into two rarity thresholds. Ignored rare items could be collected and ordinary legendary items were conflated with sets. | Evaluate each item's own grade or set row. |
| After-combat items were collected while fighting; passing-only items could become detour targets through another grade's policy. | Wait until perceived opponents and current danger are gone. Passing collection never creates a movement goal. |
| Equipment filters, route limits and the leave-loot-at-completion option were not connected. | Apply class, slot and item-level filters, their explicit exceptions, path distance and completion policy. Exceptions never override Ignore. Waiting for danger checks the existing route. |
| The legacy preset's no-enemy condition blocked during-combat priority collection. | When the edict is enabled, it owns loot timing. Other action conditions remain in place. |
| Seals had simulation state and HUD counts but no world or minimap representation. | Add replaceable primitive geometry for the guarded monolith, progress arc and broken fragments, plus discovered-only hexagonal map markers. |
| Wide layouts lost the seal count during HUD refresh. | Keep the seal count and gate meter in every battle layout, instead of updating only the compact HUD. |
| Redrawing the map captured its already-paused state as the prior state. | Retain the original entry state across redraws, then restore it on close. |

Loot options are compiled at combat preparation or explicit refresh, rather than canonicalizing the whole document per frame. Turning the edict off or disabling it after an error restores the legacy route. Inventory capacity and protection still use `Economy.AddItem`.

New documents initially select all equipment slots. Existing explicitly empty selections remain empty and receive an explanation in the editor. Save versions, item identities and the attribute system's numeric identifiers are unchanged.

## Validation

Validated with Unity 6000.6.0f1. The latest player is `Builds/macOS-ClaudeReview/HELLSCRIPT.app`; evidence is under `Artifacts/Validation/ClaudeReview`.

| Check | Result and evidence |
|---|---|
| Before the loot fix | All 10 valid regression cases failed on the old implementation: `loot-regression.xml`. The first two attempts had fixture setup errors and are not used as the baseline reproduction. |
| Focused loot follow-up | 36 new and existing loot/policy/order cases passed: `loot-fixed-slots.xml`. One subsequent default-slot test brings the new loot cases to 17. |
| Full Edit Mode | 1,210/1,210 passed, no failures or skips. September 12, 08:31:18–08:34:39 UTC: `full-editmode.xml`. |
| Final HUD follow-up | Only the HUD display and native objective harness changed after the full run. All 115 selected loot/policy/order/seal/layout/localization checks passed: `final-focused.xml`. |
| Final macOS build | Success: `build-final.log`. |
| Native objective and restart | Two processes passed saved grade/set collection, discovered-only geometry, guarded/partial/broken states, 1920×1080, 844×390 and 720×1280 windows, and pause restoration after map redraw. A partial seal checkpoint survives restart and finishes with one response wave. The restored simulation matches an unrendered control: `objective-final.log`, `objective-final-resume.log`, 8 images in `ObjectiveFinalEvidence`. |
| Native edict and restart | Two processes passed editor/save/revert, clipboard sharing/import/error display, and persisted document/switch checks: `edict.log`, `edict-resume.log`, 12 images in `EdictEvidence`. |
| Native inventory and restart | Existing inventory checks and a separate-process restart passed: `inventory.log`, `inventory-resume.log`, 27 images in `InventoryEvidence`. |
| Native battle layout | Passed 16 window sizes, 5 boss layouts, compact overview, editor/settings return and 120 identical ticks against a control: `battle-layout.log`, 25 images in `BattleLayoutEvidence`. |

The final player passed 7 process runs, including 3 separate-process resumes. These use native macOS windows, Metal rendering and UI callbacks. They do not establish physical mobile input, rotation or performance. The wide and portrait objective views and map screenshots were also visually inspected.

The final manifest records 192 C# source hashes. All 287 protected baseline metadata, scene, serialized asset, settings, package and design files remain unchanged. Five new source metadata files exist with no duplicate asset GUIDs. Settings changed automatically by Unity during tests/build were restored. See `final-source-manifest.json` and `validation-summary.json`.

Screenshots: [partial break](../../Artifacts/Validation/ClaudeReview/ObjectiveFinalEvidence/03-breaking-1920x1080.png), [portrait](../../Artifacts/Validation/ClaudeReview/ObjectiveFinalEvidence/05-breaking-720x1280.png), [completed after restart](../../Artifacts/Validation/ClaudeReview/ObjectiveFinalEvidence/01-resumed-broken-wide.png).

## Remaining work

1. Review and integrate the separate attribute work after Claude finishes. This branch changes the loot section of `CombatSimulation.cs`, not attribute, damage or growth formulas. Both branches add localization entries; recheck duplicates and missing translations at integration.
2. Complete the remaining per-skill edict options and exact target policies. High-HP and ordered special targets are still approximated by the legacy single-policy bridge. An editor does not imply complete runtime behavior.
3. Connect individual non-equipment field drops, alternate safe routes, tolerated predicted damage, cleanup and full repeat policies. The current danger-wait option waits for the existing route to become safe. An independent loot scheduler for loadouts without an enabled loot rule is still outstanding.
4. Implement physical gate passage rules, wing exploration and reward access, and the remaining essence/offering objectives. This visual change does not alter passage rules, seal counts, meter thresholds or encounter scaling.
5. Expand responsive and long-English-text checks across result, training and other management pages, including the narrow condition-picker subtitle reported by the attribute work.
6. Continue online accounts and cross-save, gems/sockets/item quality, and physical mobile input, orientation and sustained performance validation under their dedicated specifications.

`Assets/Plans/next-development-plan.md` is an early Unity AI handoff draft. Its 1x/2x/4x controls and some archetype names disagree with the actual design. Use the owning specifications in `Docs/Design` and current source. See the [screen layout specification](../Design/HELLSCRIPT_Screen_Layout_Detail.md) and [speed access specification](../Design/HELLSCRIPT_Speed_Access_Detail.md).
