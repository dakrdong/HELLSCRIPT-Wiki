# HELLSCRIPT Storage HTML Reference Implementation

Updated: 2026-09-20 · [한국어](Storage_Reference_Parity.md)

The storage screen has been rebuilt in the game's existing uGUI to follow the supplied HTML. Landscape places the warehouse, transfer rail and bag from left to right; portrait stacks them in the same order. Both inventories keep eight square columns. The screen uses real game items and existing storage transactions.

## Reference and scope

The references are `HELLSCRIPT-Storage-Reference.html` and `HELLSCRIPT-Storage-Development-Prompt.md`. Their CSS, DOM and state-specific presentation were inspected. Browser rendering of the local HTML was blocked by the tool's local-file policy, so no automated pixel comparison or exact pixel identity is claimed. Actual game captures were inspected in the macOS development build.

| Source | SHA-256 |
|---|---|
| HTML | `3c8e734198e77163c9ed541a77e71371e8f6de98b515cdd0572dfacfc48f3a49` |
| Development brief | `97b0bf000ec9b2f20169032aebba884f0eee11afc718831c5d5bde803b6984be` |

The HTML's sample items, sample images, local storage and demo controls were not adopted as game data. Fixtures are created only by an explicit development-build smoke argument in a separate temporary save directory. The user's save was not reset or given test currency.

## Visual implementation

| Area | Implementation |
|---|---|
| Surfaces | Charcoal/olive gradients and thin brass outlines drawn as native uGUI meshes by `StorageSurface` |
| Palette | Background `#1a1c16`, brass `#c0a777`, text `#d9d1be`, muted text `#9b9689`, primary action gradient `#702e28` to `#461b18` |
| Header/footer | Centered Warehouse title, HELLSCRIPT/account sharing on the left, close on the right; balances, history and guide in the footer |
| Warehouse tabs | I–V, editable names, lock/cost and selected underline; pencil opens the rename dialog |
| Categories/filter | Underlined categories; multi-select normal/magic/rare/legendary/set overlay; separate sort dropdown |
| Grid | Eight square columns, three logical units between cells, existing equipment atlas and muted rarity outlines; dashed expansion cell after the last slot |
| Transfer rail | Vertical in landscape and horizontal between the lists in portrait, with a directional emblem and bulk action |
| Detail/purchase | Larger real equipment image, name, stats and comparison, dark red actions; expansion shows before/after capacity, cost and balances |
| Responsive layout | Scale and logical dimensions derive from the actual safe area. Headers, tabs, filters and transfer controls stay fixed while each inventory scrolls independently |

Existing 6×4 `EquipmentAtlas` cells remain unchanged. `StorageGlyph` draws warehouse, bag, arrow, check and currency shapes at the current resolution. No raster asset, package, scene or prefab was added; existing assets and GUIDs are retained. Headings use an available dynamic serif font, so glyph appearance can vary by operating system. Development builds reserve footer space for their watermark.

## Game integration and saving

| Responsibility | Existing connection and change |
|---|---|
| Entry/pause | `GameUI.Storage` → `StorageWindow`; town, content dock and bag entry points retained; closing restores the previous pause state |
| Items | `ItemCatalog`, `ItemGenerator`, `Item.id`, `EquipmentAtlas`; cell, detail, drag and preset refer to the same instance |
| Gems/runes | `GemInventory`, `GemCatalog`, `RuneMasteryCatalog`, `RuneBoardGraphic`; actual types, effects and occupied shapes |
| Movement/purchase | Existing `GameStore.Storage` and `Storage` validation, atomic transactions, capacity and currency rules |
| Names | Additive `AccountSave.warehouseNames`, saved through `GameStore.RenameStorageTab`; only open tabs, 1–12 trimmed characters and no control characters |
| Legacy saves | Missing/short name arrays display defaults and are padded when renamed. Names survive unrelated transactions; existing equipment, positions and capacities remain intact |
| History | The latest 30 actual storage receipts from `AccountSave.transactions`, including rename, move, swap, purchase and presets |

`StorageWindow.cs` owns layout, `.Style.cs` drawing, `.Dialogs.cs` detail/purchase/presets, `.Tools.cs` sort/names/history/guide and `.Drag.cs` input. A continuous repaint after switching language was also fixed. Existing data, purchasing and preset rules are documented in [Storage UI implementation](Storage_UI.en.md).

## Verification and captures

An isolated checkout containing only this storage change passed **99 of 99** tests across `StorageTests`, `PresetStorageTests`, `InventoryLayoutTests` and `LocalizationTests`. New tests cover name persistence, legacy arrays, preservation through other transactions and unchanged state for invalid names or locked tabs. The full Edit Mode regression suite was not rerun for this change.

The first run of the same scope in the shared working directory passed 98 of 99. Its one failure reported nine missing translations in concurrently developed rune files. That separate work was neither changed nor included in this commit. The isolated 99-pass result is a distinct run.

The macOS development build and storage runtime checks are recorded in the [verification summary](StorageReferenceEvidence/verification.txt), [Edit Mode result](StorageReferenceEvidence/editmode.xml) and [runtime result](StorageReferenceEvidence/runtime.txt). Synthetic input through the real pointer handlers exercised exact-slot moves, atomic swaps, touch-hold pickup, one-second tab hover, cancellation, sorted-view drops, bulk transfers, duplicate-purchase protection, preset returns, state retention on rotation and combat pause/resume. Sort, rename, history, guide and Korean/English layouts were also checked.

| Game capture | Coverage |
|---|---|
| [Landscape](StorageReferenceEvidence/storage-reference-01-landscape-town.png) | 1600×900, opened from town |
| [Sort](StorageReferenceEvidence/storage-reference-02-sort-menu.png) | Dropdown |
| [Guide](StorageReferenceEvidence/storage-reference-03-guide.png), [history](StorageReferenceEvidence/storage-reference-04-history.png) | Functional auxiliary screens |
| [Detail](StorageReferenceEvidence/storage-reference-05-detail.png), [comparison](StorageReferenceEvidence/storage-reference-06-detail-comparison.png) | Real equipment and stats |
| [Grades](StorageReferenceEvidence/storage-reference-07-grade-menu.png) | Multi-select filter |
| [Gems](StorageReferenceEvidence/storage-reference-08-gems.png), [runes](StorageReferenceEvidence/storage-reference-09-runes.png), [rune detail](StorageReferenceEvidence/storage-reference-10-rune-detail.png) | Actual account stock |
| [Drag](StorageReferenceEvidence/storage-reference-11-drag-hover.png), [tab hover](StorageReferenceEvidence/storage-reference-12-drag-tab-opened.png) | Input and drop previews |
| [Bulk selection](StorageReferenceEvidence/storage-reference-13-bulk-selection.png) | One-direction selection |
| [Expansion](StorageReferenceEvidence/storage-reference-14-expand-dialog.png), [tab purchase](StorageReferenceEvidence/storage-reference-15-buy-dialog.png), [presets](StorageReferenceEvidence/storage-reference-16-presets.png) | Purchases and existing eight equipment slots |
| [Portrait](StorageReferenceEvidence/storage-reference-17-portrait-town.png) | 900×1600 |
| [Small portrait](StorageReferenceEvidence/storage-reference-18-portrait-phone.png), [small landscape](StorageReferenceEvidence/storage-reference-19-landscape-phone.png) | 390×844 and 844×390 desktop windows |
| [English landscape](StorageReferenceEvidence/storage-reference-20-english-landscape.png), [English portrait](StorageReferenceEvidence/storage-reference-21-english-portrait.png), [English at 125%](StorageReferenceEvidence/storage-reference-22-english-large-text.png) | Language and reading size |
| [Rift](StorageReferenceEvidence/storage-reference-23-landscape-rift.png) | Storage during combat and pause |

Open storage from the town warehouse, the play-screen content dock or the bag footer. The build output is `Builds/StorageReference/HELLSCRIPT.app`. Automated acceptance uses `-hellscriptStorageSmoke` with explicit separate `-hellscriptSavePath` and `-hellscriptScreenshots` paths.

## Unverified scope and existing constraints

Physical iOS/Android touch, notches, home indicators and performance were not verified. Phone-sized captures and touch scenarios above are macOS windows and synthetic input, not physical-device evidence. Neither full-game regression nor HTML pixel comparison is claimed.

Materials, gems and runes remain account-wide stock, displayed in warehouse categories without transfer into a hero bag. Equipment presets remain town-only. The existing premium balance is reused without adding a grant or payment path; test balances exist only in temporary fixtures.
