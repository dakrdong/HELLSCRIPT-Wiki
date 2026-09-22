# Shared UI integration validation

Updated: 2026-09-22

Built on completed-work integration `2a5012f`: shared equipment presentation, window management and theme, a summary/selected-editor Hunt Edict, and a starter plus repository/CI rules for future content. [Contract](Shared_UI_Contract.en.md) · [Korean version](Shared_UI_Validation.md)

## Scope

Inventory, storage, shop/gambling, forge, gems, crafting and training share `ItemDetailView`/`ItemTooltip` and existing comparison rules. Catalog definitions use the same body without fabricating rolled owned items. `EquipmentSlotView` and `CharacterEquipmentView` share slot states and ten equipment positions with explicit owned/draft/battle/catalog sources. Saved positions and schemas are preserved.

Theme, fonts, type metrics and button states are shared. Storage retains two inventories, forge equal landscape columns, and runes hex geometry. Legacy pages use coordinate adapters. The window host owns stacking, background input, back and nested pause restoration. Committed-state notifications refresh views only after a persisted adoption; modal edits defer refresh.

Hunt Edict retains all 25 groups, 130 global options and combat semantics while adding summaries, a selected editor, portrait slide/back, search, changed-group filtering, compact controls and fixed save actions. Unconfirmed numeric modal input survives rotation and language changes. HUD/history/maps/runes/settings retain established stat, icon, snapshot and settings providers and use shared styling/font/window ownership. Domain-specific equip/sale/salvage/hex validity remains with the relevant services.

Ongoing new skill design is excluded. Original worktree changes, resource GUIDs and socket/quality/investment history are preserved. See [the completed-work merge](Completed_Work_Integration.en.md).

## Verification

| Check | Evidence |
| --- | --- |
| Full Unity Edit Mode | **3,080 passed, 0 failed, 0 skipped**. [XML](SharedUiEvidence/full-editmode.xml) |
| Final focused Edit Mode | **275 passed, 0 failed, 0 skipped** after final presentation refinements: shared UI/HUD, localization, Editor null safety, settings, inventory/comparison, edict persistence and training. [XML](SharedUiEvidence/final-focused-editmode.xml) |
| UI ownership guard | Current-source check and **9 fault-injection/starter tests passed**, including nested new windows, canvas/font duplication, account access, dropped detail ownership, partial classes and overwrite prevention. |
| Native build | Unity **6000.6.0f1**, macOS development player, Metal, successful build. |
| Shared windows | Five ratios, fixed equipment slot metrics, equal forge columns and nested input/pause restoration. [Report](SharedUiEvidence/shared-runtime.txt) |
| Hunt Edict | All options, pointer choices and drag reorder, skills/presets/save/share/import, KO/EN and 150% text. [Runtime](SharedUiEvidence/edict-runtime.txt) · [Actions](SharedUiEvidence/edict-actions.txt) |
| Inventory/detail | Rings, two hands, offhand, protection/salvage, saved filters, pointer and held-touch drag, shared comparisons. [Report](SharedUiEvidence/inventory-runtime.txt) · [Ranges](SharedUiEvidence/ranges-runtime.txt) |
| Forge | NPC access, affix lock, enhancement batches, slot work, instant finish and persistence. [Report](SharedUiEvidence/blacksmith-runtime.txt) |
| Rune master | NPC, reshape/ascend, material protection, duplicate input, drag, rotation and persistence. [Report](SharedUiEvidence/rune-runtime.txt) |
| Settings | Aspect, text/view scale, language, character switch, world preview, pause and a separate restart. [Runtime](SharedUiEvidence/settings-runtime.txt) · [Restart](SharedUiEvidence/settings-restart.txt) |
| Training/restart | Actual A/B behavior differences, borrowed ownership unchanged, result/B preset/runes/edict restored in a separate process. [Runtime](SharedUiEvidence/training-runtime.txt) · [Restart](SharedUiEvidence/training-restart.txt) |

After the full run, refinements preserved modal input, restored settings background alpha, connected catalog bodies and centralized remaining palette references; native selectors were updated. Visual review also found HUD overlap with fixed training actions at enlarged text sizes. Reading pages now fit the HUD within their reserved area while battle layouts retain their existing geometry. All 275 final focused checks passed. Test reports overlap; do not add their counts. [Full-run source hashes](SharedUiEvidence/full-run-source.json) · [Final source hashes](SharedUiEvidence/final-source.json)

Legacy smoke assumptions about button/scroll names, pause ownership and title entry were updated. The training fixture's rune placement was corrected for the current v13 board. Production save validation was not weakened and failed attempts are not presented as passes.

Checks used an isolated Unity batch checkout and actual macOS players with synthetic uGUI pointer/drag events. The user's original live Editor and save were not repurposed for verification. **Physical mobile touch, safe areas and performance are not verified here.**


All **13 native launch/restart scenarios finished with exit code 0**. [Summary](SharedUiEvidence/summary.json) · [Edict restart](SharedUiEvidence/shared-restart.txt) · [Training equipment](SharedUiEvidence/training-equipment.png) · [Restored English result](SharedUiEvidence/training-restored.png).

After the HUD refinement, all **7 relevant launch/restart cases passed again**: shared windows and restart, settings and restart, battle HUD, training and restart. The remaining domain scenarios passed on preceding shared-UI builds. [Final native results](SharedUiEvidence/final-native-results.json) · [Battle HUD](SharedUiEvidence/hud-runtime.txt). Training result viewports remain scrollable and fixed actions are above the HUD at 150% text in all five reference sizes: [portrait](SharedUiEvidence/training-restored-result-440x956.png), [landscape](SharedUiEvidence/training-restored-result-956x440.png), [16:9](SharedUiEvidence/training-restored-result-1440x810.png), [16:10](SharedUiEvidence/training-restored-result-1440x900.png), [21:9](SharedUiEvidence/training-restored-result-1680x720.png).

## Reference screens

Each link is a native-player capture. Inventory retains its centered fixed frame; expanding windows fill the safe area. Equipment slots use the same scale and dimensions for equivalent conditions.

| Size | Inventory | Storage | Shop | Gear | Slot growth | Edict |
| --- | --- | --- | --- | --- | --- | --- |
| 440×956 | [↗](SharedUiEvidence/inventory-440x956.png) | [↗](SharedUiEvidence/storage-440x956.png) | [↗](SharedUiEvidence/shop-440x956.png) | [↗](SharedUiEvidence/forge-440x956.png) | [↗](SharedUiEvidence/slot-growth-440x956.png) | [↗](SharedUiEvidence/edict-440x956-ko.png) |
| 956×440 | [↗](SharedUiEvidence/inventory-956x440.png) | [↗](SharedUiEvidence/storage-956x440.png) | [↗](SharedUiEvidence/shop-956x440.png) | [↗](SharedUiEvidence/forge-956x440.png) | [↗](SharedUiEvidence/slot-growth-956x440.png) | [↗](SharedUiEvidence/edict-956x440-ko.png) |
| PC 16:9 | [↗](SharedUiEvidence/inventory-1440x810.png) | [↗](SharedUiEvidence/storage-1440x810.png) | [↗](SharedUiEvidence/shop-1440x810.png) | [↗](SharedUiEvidence/forge-1440x810.png) | [↗](SharedUiEvidence/slot-growth-1440x810.png) | [↗](SharedUiEvidence/edict-1440x810-ko.png) |
| PC 16:10 | [↗](SharedUiEvidence/inventory-1440x900.png) | [↗](SharedUiEvidence/storage-1440x900.png) | [↗](SharedUiEvidence/shop-1440x900.png) | [↗](SharedUiEvidence/forge-1440x900.png) | [↗](SharedUiEvidence/slot-growth-1440x900.png) | [↗](SharedUiEvidence/edict-1440x900-ko.png) |
| PC 21:9 | [↗](SharedUiEvidence/inventory-1680x720.png) | [↗](SharedUiEvidence/storage-1680x720.png) | [↗](SharedUiEvidence/shop-1680x720.png) | [↗](SharedUiEvidence/forge-1680x720.png) | [↗](SharedUiEvidence/slot-growth-1680x720.png) | [↗](SharedUiEvidence/edict-1680x720-ko.png) |

![Landscape edict editor](SharedUiEvidence/edict-956x440-ko.png)
![English edict at 150 percent](SharedUiEvidence/edict-portrait-en-150.png)

Shared detail: [inventory](SharedUiEvidence/detail-inventory.png), [storage](SharedUiEvidence/detail-storage.png), [shop](SharedUiEvidence/detail-shop.png). Extra states: [summary](SharedUiEvidence/edict-portrait-summary.png), [rotated numeric modal in English](SharedUiEvidence/edict-number-rotated-en.png), [portrait runes](SharedUiEvidence/rune-portrait.png), [landscape runes](SharedUiEvidence/rune-landscape.png).

## Future content

Use `tools/new_content_ui.py` to start from the common window. `AGENTS.md` and `CLAUDE.md` require this route; `tools/check_ui_contract.py` and GitHub Actions check shared ownership. Specialized layouts still preserve presentation/input/persistence boundaries and require visual acceptance. The guard does not redraw arbitrary UI or detect every visual defect.
