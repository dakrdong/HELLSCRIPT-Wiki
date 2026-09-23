# Shared UI and new-content contract

Updated: 2026-09-23
Use the latest inventory as the equipment presentation baseline. Compose new content from shared owners rather than copying rendering or gameplay formulas. Keep specialized layouts and each domain's rules.

[Korean version](Shared_UI_Contract.md)

## Presentation owners

| Element | Owner and contract |
| --- | --- |
| Theme | `UiTheme`: dark olive backgrounds, brass selection/actions, ivory text. Preserve semantic danger/gain/loss colors. |
| Fonts | `UiFonts.Body`; `UiFonts.Display` only for title branding. Individual windows must not destroy borrowed fonts. |
| Type | Title 20, heading 15, body 12, caption 10 before reading scale. Existing HUD/legacy adapters convert their coordinate system. |
| Equipment slots | Portrait 52, landscape 54, gap 6, common `UiTheme.Scale`. Reduce columns instead of shrinking slots. Preserve persisted slot indices and capacity. |
| Windows | Header/close, navigation, scroll body, fixed actions. `ContentWindowHost` owns stacking, background input, back and nested pause leases. |
| Details | `ItemTooltip` and `ItemDetailView` share order, values, ranges, effects, sockets, upgrades and protection. Fixed outer bounds, scrolling body. |
| Comparison | `EquipmentComparisonView` and `ItemComparison` share ring/hand replacement and stat deltas. |
| Paperdoll | `CharacterEquipmentView` owns ten placements. Inventory/storage presets show actual items; slot growth shows translucent body-part emblems. |
| Slot states | `EquipmentSlotView` owns grade border, level, lock, worn and selected markers. Forge work uses `ForgeWorkingPulse`. |

Element/class/rune colors, HP/mana, danger telegraphs and map symbols retain their meaning. Forge keeps equal three-column landscape geometry, storage keeps two inventories, and rune boards keep hex cells.

## Domain ownership

Use `HeroStats`, `StatCatalog` and `ItemComparison` for numeric meaning; `EquipmentSlots` and `Storage` for equip positions and movement; existing bulk/salvage/sale policies and `ShopAutoSelect` for protection and selection. Different actions intentionally have different eligibility rules. Do not replace them with one permissive check.

`InventoryQuery`, `ShopAutoSettings` and per-window state preserve filters, selection and scrolling. Existing uGUI drag adapters share pointer lifecycle conventions while movement, hex occupancy and reorder validity remain domain-specific. Existing `GameStore` commands revalidate quotes, ownership and resources atomically.

`GameStore.Committed` fires only after a successful persisted adoption. Duplicate receipts do not publish again. `StoreViewBinding` defers repaint while a modal is being edited. Presentation callbacks cannot turn a completed purchase into a failed purchase. Existing edit models distinguish applied, draft and saved state.

`TownWalk` and `ContentUnlocks` own access. `UiTime` formats durations but never chooses a clock: UTC work and pausable battle time remain separate. `GlobalHudSnapshot`, `SkillIconView` and saved combat/training snapshots preserve consistent labels/icons without recomputing history from live gear. `UiSafeArea`, `Loc` and existing device display/text/audio preferences remain shared.

Pass `EquipmentViewSource.Owned`, `Draft`, `BattleSnapshot`, `Catalog` or `RewardSnapshot`. Detail/comparison views copy their input and never mutate the account. Catalog definitions have no acquired roll; never fabricate one for display. Craft results and reward history use `RewardSnapshot` to preserve committed values. Controllers supply domain commands and drafts.

## Hunt Edict

Keep all 25 groups and 130 global option IDs, values, ranges and combat behavior. Landscape uses category, summary list and selected editor with independent scrolling. Portrait slides between summary and detail and restores list position. Search names/help/IDs across categories; optionally show changed groups only.

Show current choices, numbers, sets and priorities compactly; open controls on demand. Disabled conditions remain explained inline. Skill rows show icon/name/rank/equipped state while commands belong to selected details. New skill design remains excluded. Keep dirty state, revert and save in a fixed footer. Navigation preserves drafts; close/preset replacement asks about unsaved edits.

## New content workflow

1. Start in a checkout containing the latest merged `main`, then run `python3 tools/new_content_ui.py FeatureName`. Keep unfinished changes in an older checkout intact and use a separate current checkout. Existing files are never overwritten.
2. Supply bilingual title, explicit source, reading scale and render callback. Keep the draft in the controller, outside repaint callbacks.
3. Compose common views under `Navigation`, `Body` and `Actions`; call established transactions for mutations.
4. Document any specialized layout adapter and its shared owners. Do not duplicate font creation, formulas, equipment cards or canvas ownership.
5. Run `python3 tools/check_ui_contract.py`, `python3 tools/test_ui_contract.py`, related Unity tests and native macOS acceptance. Record images plus actual before/after state and persistence.

GitHub Actions runs the ownership checker and its fault-injection tests. The checker catches new window/canvas bypasses, independent font creation, missing shared equipment connections and account access in reusable views. It does not redraw arbitrary UI or prove visual correctness. Repository rules guide subsequent development; they are not an operating-system restriction on external code generators.

## Acceptance

Check identical equipment data, two rings/two hands/offhand/presets, unchanged saved slot indices, fixed actions and scrolling at 440×956, 956×440, 16:9, 16:10 and 21:9. Check Korean, English, larger text, all edict options and preserved drafts across search/navigation/rotation/language. Test failed saves, duplicate/stale requests, insufficient resources and nested pause/input restoration. Distinguish synthetic macOS pointer acceptance from physical-mobile validation.

See [completed-work integration](Completed_Work_Integration.en.md) for the initial merge and excluded skill work, and [shared UI validation](Shared_UI_Validation.en.md) for implementation evidence.

## Forge core crafting

`BlacksmithWindow.Cores` extends the existing forge adapter with a fourth tab, reusing shared slots, rarity colors, details, fonts, scale and window/input ownership. Definitions use `ItemTooltip.CatalogRanges` and `ItemDetailView.AppendCatalog`; results pass saved items. The reasons for retaining portrait page navigation, three equal wide columns and the quality gauge inside the forge, along with validation, are documented in [core crafting](Core_Crafting.en.md).

## Aspect Runestone

`AspectStoneWindow` is generated from the new-content template and opens `ContentWindowView` with `EquipmentViewSource.Owned`. It composes `CharacterEquipmentView`, `EquipmentSlotView` and `ItemDetailView` inside the shared navigation, scrolling body and fixed action region. Landscape has equipment, library and detail columns; portrait uses three steps. The confirmation step contains an inline equipment picker so changing slots preserves the selected aspect. `AspectStoneSession` owns selection and filters; `GameStore` owns collection, level-up and imprint transactions. Rune glyphs and red upgrade dots are content-specific meaning, while fonts, slots, window scale and panel colors use the shared owners. There is no field placement tab.

## Buttons and tabs

Follow the [shared button interaction contract](Button_UX.en.md). Create new buttons with `ContentWindowView.Button` and supply persistent selection with `UiTheme.Choice`. Do not duplicate button palettes or pointer transitions per screen.


Shared UI does not require a rectangular frame for every control. Vaults use `UiButtonRole.Icon` and `StorageChestGraphic` for open, closed, purchasable and locked chests. Shared input state, theme and fonts remain authoritative; `StorageWindow` and `GameStore` own selection and transactions. Ambiguous filters retain text labels.

## Town and combat HUD placement

`GlobalHudLayout` owns the persistent vitals, skills and potions; `GameUI.Plaza` and `TownJoystick` own the town heading, shortcuts and movement input. This HUD is an adapter using its existing canvas and proportional screen layout, rather than a content window. Potion and title backings reuse `StorageSurface` and `UiTheme` colors without new raster artwork. Presentation does not save accounts or consume potions. See [town HUD validation](Town_Hud_Responsive.en.md) for the portrait bottom baseline, centered movement pad and enlarged-text layout.

## Native skill tree adapter

The Skills tab presents the approved 37-skill class tree, four normal active slots and a separate ultimate slot. Unlocked passives always apply; a slot menu opens that skill’s edict settings. The existing HuntEdictWindow retains draft ownership, shared theme/icons/window host, fixed controls and independent scrolling. See [integration](Hunt_Edict_Skill_Tree.en.md).

## Rift entry adapter

`RiftEntryWindow` uses the shared `ContentWindowView` owned-data entry point. Landscape fixes the sanctuary illustration and tier selection beside independently scrolling preparation, keeping the tier and entry conditions visible together. Pickers use the shared optional maximum size without a new canvas or lifecycle. It reuses `UiTheme` paid-fatigue colors, `PotionArt`, `SkillIconView` and existing `GameStore` transactions. First-clear rewards show empty icon slots in recycled rows without invented payouts. See [implementation and validation](Rift_Entry.en.md).
## Inventory potion placement

At the user's request, potion cells use smaller dimensions of 32 in portrait and 28 in landscape. Equipment retains its 52/54 baseline. The adapter reuses `EquipmentSlotView` and sits beside the weapon row owned by `CharacterEquipmentView`, without adding a row. Per-cell assignment is separate from the shared use-order setting, with one gear beside the group. [Potion slot validation](Potion_Slots.en.md) records geometry, input and persistence evidence.

## Jeweler

`JewelerWindow` uses `ContentWindowView`, `EquipmentViewSource.Owned`, `StoreViewBinding` and `GameStore` transactions. Its gem-type/tier count matrix is a specialized adapter rather than equipment-slot geometry. Landscape uses two independent scroll panels; portrait uses navigation tabs and the same fixed craft action. Socket details retain the common equipment owners. See [Jeweler runtime](Jeweler_Runtime.en.md).
