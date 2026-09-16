# HELLSCRIPT Storage UI

Date: 2026-09-16 · [한국어](Storage_UI.md)

Status: implemented and verified on the desktop development build. Not checked on a physical phone or tablet. Verified and unverified items are separated under [Verification](#verification).

## Goal and scope

The account-wide warehouse and the current hero's bag share one screen. Both lists are grids of square slots, eight per row. Landscape reads `warehouse | transfer rail | bag`; portrait reads `warehouse → transfer rail → bag`. No phone resolution or fixed aspect ratio is forced: the window fits the real safe area the same way the Hunt Edict window does.

The attached `HELLSCRIPT-Storage-Reference.html` was used only as a reference for look and interaction. Item names, images, stats, affixes, grades, equipment slots, rune blocks and gems all come from the project's existing definitions (`ItemCatalog`, `GemCatalog`, `RuneMasteryCatalog`) and the instances the account actually owns. The HTML's sample arrays, `localStorage`, demo tools, character line and search box were not ported.

## Real connection points

| Role | Actual file, type, method | Connection |
|---|---|---|
| Item definitions and instances | `Core/GameCatalog.cs` `Item`, `Core/Itemization.cs` `ItemCatalog` | Instance id `Item.id`, base, unique and set definitions, `DisplayName`, `MainValue`, affix `rolls` |
| Icons | `Resources/Art/EquipmentAtlas.png` | The same 6×4 UV lookup as `GameUI.EquipmentIcon`, used in cells, the detail dialog and the drag ghost |
| Grade display | `GameCatalog.Rarities`, `UniqueItemDefinition.setId` | Set membership is not a rarity, so the display filter alone treats it as a fifth grade; the stored `rarity` is untouched |
| Stats and comparison | `HeroStats`, `Economy.Equip`, `Economy.EquipError` | The detail comparison equips a hero copy and computes the same sheet |
| Gems | `AccountSave.gems`, `GemCatalog`, `GemInventory.Name`, `GemStacks.Count` | Account stacks shown under the warehouse gem category, with real tiers and per-slot effects |
| Rune blocks | `AccountSave.runes.owned`, `RuneMasteryCatalog.ShapeById`, `RuneBoardGraphic(icon)` | Real hex shapes and grade colours in thumbnails and details, plus the board a rune is placed on |
| Materials | `AccountSave.materials` | Account-wide count |
| Ownership and position | `HeroSave.inventory` (with the `equipped` flag), `AccountSave.warehouse` | Both lists stay as they are; items gain position fields |
| Equipment presets | `HeroSave.presets[n].equipmentIds`, `GameStore.SaveBuildPreset`, `HuntEdictPreset.equipmentIds` | Registration is by instance id. The apply service is new; before this, the ids only protected items |
| Currency | `AccountSave.gold`, new field `AccountSave.premium` | Gold is the existing field. The project had no paid currency, so only a balance field was added; nothing grants it |
| Save and transactions | `GameStore.Transact`, `GameStore.Normalize`, `GameStore.ValidateItems`, `Write` | Every storage change is applied and validated on the staged account, then committed by file replacement |
| Pause | `RunState.paused`, the approach of `GameUI.ShowEdictEditor` | `paused=true` on open, previous value restored on close |
| Entry points | `GameUI.Plaza.OpenStation(TownStation.Warehouse)`, `GameUI.ContentDock`, the bag screen's `창고` footer button | The town warehouse NPC, the play-screen dock's storage button, and the equipment/bag screen |
| Automatic cleanup | Warehouse action in `EdictCleanupPolicy.Apply` | `Storage.Deposit` into the first open tab with room replaces the old 400-slot constant |
| Screen parts | `HuntEdictWindow` skins (`edict-ui-skins`), glyphs, checkboxes | The same charcoal and brass skins and controls; no new art concept |

## Data and saving

- `Item.storageTab`, `Item.storageSlot`: the real slot inside a container. The bag has one tab and uses only `storageSlot`; the warehouse uses tab 0–4 and slot. Worn items occupy no slot, so `storageSlot = -1`.
- `Item.origin`: where the item sat just before it was worn (`bag:0:12`, `warehouse:2:7`). A preset swap returns it there; without a value it returns to the first free bag slot of the current hero.
- `AccountSave.warehouseCapacity`: purchased capacity per tab, five values; 0 is a locked tab. A new account is `{50,0,0,0,0}`.
- `AccountSave.premium`: paid currency balance.
- Only `Storage` in `Core/Storage.cs` writes positions. `GameStore.Normalize` calls `Storage.Normalize` on load and on every transaction's staged copy to repair missing or duplicate slots, and `ValidateItems` rejects duplicates through `Storage.Validate`.
- Acquisition (`Economy.AddItem`, `TownTrade.BuyEquipment`) assigns the first free slot; manual equipping (`Economy.Equip`) hands the incoming item's slot to the item it replaces.

### Existing saves

- A save without position fields gets sequential first-free slots in list order on load. No item is deleted.
- A save holding more than 50 warehouse items under the old 400-slot constant keeps them in tab 1's temporary overflow slots. It reads `52 / 50`; withdrawing and buying an expansion work, only ordinary intake is blocked. No purchase right was granted or removed.
- A damaged `warehouseCapacity` is rounded to steps of 10 within 50–100, and a gap in the order opens the earlier tab at 50 rather than locking anything.
- The schema number is unchanged. New fields read as `JsonUtility` defaults, and an older build ignores fields it does not know.

## Purchase and expansion

| Tab | Initial | Unlock | One +10 expansion | Capacity |
|---|---|---:|---:|---|
| 1 | open | free | Gold 1,000 | 50→100 |
| 2 | locked | Gold 5,000 | Gold 5,000 | 50→100 |
| 3 | locked | Gold 100,000 | Gold 50,000 | 50→100 |
| 4 | locked | Premium 500 | Premium 100 | 50→100 |
| 5 | locked | Premium 2,000 | Premium 500 | 50→100 |

The values live only in `StorageRules` and both the UI and the logic read them. Tabs open in order 2→3→4→5; the previous tab need only be open, not expanded. Expansion costs the same fixed price per tab every time and each tab expands independently up to five times. The expansion cell sits once after the last storage slot including blanks (after any temporary overflow slots), is excluded from selection, drag start, drop, swap and counting, and disappears at 100. The confirmation shows cost, balance, balance after approval and capacity before and after; balance and current capacity are re-checked only at approval (`ExpandStorageTab` carries the capacity the dialog quoted so a stale confirmation cannot buy a second step). Cancel, insufficient currency and double clicks deduct nothing.

## Movement rules

- Single move: the detail dialog's `Withdraw` / `Store` goes to the first real free slot of the tab currently selected on the other side.
- Exact-slot drop: an empty slot takes the item; an occupied one swaps both items in one transaction, the displaced item taking the dragged item's previous slot. This holds inside one container, between warehouse and bag, and between warehouse tabs.
- Tab button and panel blank drops: the first free slot of that tab. Locked tabs, the expansion cell and off-window drops cancel.
- A normal full container (50/50) still swaps 1:1 without a temporary slot; it refuses empty-slot intake. An overfull tab refuses outside intake and swaps that return an item to it, and allows only withdrawals and in-tab rearrangement that adds no slot. Empty temporary slots are not drop targets.
- Ordinary moves never create a temporary slot; the only exception is the preset return.
- Bulk move: the rail button cycles `일괄 이동 → 일괄 이동 취소 (0 selected) → 옮기기 (1 or more)`. The first selection fixes the direction; tapping the other side shows exactly `이동은 한쪽으로만 가능합니다` and keeps the selection. If the destination cannot take everything, nothing moves. Select-all covers only the current tab's shown items and reports hidden selections. With a selection pending the source tab is locked and the destination tab may change.
- Sorting changes only the display. When an exact drop succeeds in a sorted full view (all categories, all grades), the shown order is written into real slots (`Storage.Reorder`) and the view returns to position order, so the dropped item stays put. A cancelled drag changes nothing. Under a filter, drops target the real slots of visible items and hidden items are never shown as free space.
- A lock prevents destruction, not movement. Lock, affixes, enhancement, gems and preset references stay on the same instance after a move.

## Preset application and equipment return

`GameStore.ApplyEquipmentPreset` applies a preset slot's `equipmentIds` in one transaction.

1. Resolve every id in the bag and the warehouse. Duplicate ids or two items for one slot are refused. A deleted item counts as `missing`; an item held by another character or failing the level or class check is `unusable`; either leaves its slot empty.
2. Pull every incoming item out of storage first, recording each one's departure point in `origin`. Items that stay worn are not touched.
3. Release worn items that are not kept, including slots with no registration.
4. Each released item returns to its own `origin`: that slot if free, else the first free slot of the same tab, else a temporary overflow slot of that tab. Example: sword A was worn from warehouse tab 2 and sword B from the bag replaces it; A goes back to warehouse tab 2, not into the bag.
5. Settle temporary slots in every container and save once at the end.

A preset with no registered equipment is refused rather than stripping the hero. Equipment changes stay town-only as before, so applying a preset during a rift is refused while warehouse↔bag moves remain available. `Register worn gear` writes the worn instance ids into that slot's `equipmentIds`, and into the matching Hunt Edict preset when one exists.

## Temporary overflow

Purchased capacity and actual holdings are separate. 51 items read `51 / 50`; the denominator never grows. Overflow applies to that tab only. An overfull tab may still buy an expansion, so `52/50` becomes `52/60` and the limit lifts. When usage drops to capacity or below, the marker and the limit disappear. Settling (`Storage.Settle`) moves items from temporary slots into free regular slots and renumbers the remaining temporary slots contiguously; it never compacts the player's regular placements.

## Screen

Split into `Presentation/StorageWindow.cs` (layout, grids, tabs, categories, rail), `StorageWindow.Drag.cs` (cell input and dragging) and `StorageWindow.Dialogs.cs` (detail, purchase, expansion, presets, grade menu). Like the Hunt Edict window it draws on its own canvas (sorting order 320), derives a scale from the safe area against a reference shape (landscape 844×390, portrait 430×840) and uses the whole safe area as logical size ÷ scale.

- Header: title, `account warehouse · {class} bag`, the `paused · time left` badge in a rift, close. No character line, no search box.
- Panel head: name, `used / capacity` (with `over` when overfull), the grade checkbox button, the sort cycle button, and a small `Presets` button on the bag.
- Warehouse tabs 1–5: a locked tab shows its price or `opens in order`; an overfull tab shows `!`.
- Seven categories: all, weapon, armour, jewellery, materials, rune blocks, gems. Weapon is slot 0, armour 1–5, jewellery 6–7.
- Grid: eight columns, cell = (width − 7 gaps) ÷ 8, independent vertical scroll per grid. Tabs, filters and the rail never scroll away.
- Cell: real equipment icon, grade-coloured outline, enhancement and level, lock glyph, a top-right checkbox in selection mode, an overflow tag.
- Rail (a column in landscape, a row in portrait): direction, count, free room, the three-state button, clear selection.
- Footer: gold and premium balances.
- The grade menu is a checkbox list floating under its button (select-all with a partial state, five grades with counts, shown count). Every change applies at once and the list stays open; tapping outside or Esc closes it and keeps the state. Warehouse and bag each keep their own.
- Detail dialog: real name, grade, class restriction, slot, item level, required level, main value, affixes with tiers, quality, sockets, unique or set text, protection flags, current position, destination; `Withdraw`/`Store`, lock toggle, whole-sheet comparison with the worn item.
- Materials, rune blocks and gems: account stock listed in the warehouse panel (the bag panel says they are account stock). Details read the real definitions.
- Tab, category, grades, sort and selection persist per hero in `GameUI.storageStates` across rotation and reopening, matching the existing inventory screen's session-only filter policy.

## Input

- Mouse: dragging past the uGUI threshold lifts the item. Touch: holding for 0.24 s and then dragging lifts it; moving earlier hands the gesture to the list scroll. Cells only decide how a gesture starts; while an item is held the window reads the pointer itself, so repainting a panel never loses the gesture.
- Resting on an open warehouse tab for one continuous second opens it (`TabHoverSeconds`); leaving or moving to another tab resets the wait. This only switches tabs, and the source container, tab and slot stay as recorded at pick-up.
- A ghost with the icon, destination and verdict follows the pointer; target slots and panels highlight as valid (green), swap (gold) or refused (ember). The preview layer never intercepts the pointer.
- Resting at a grid edge for 0.15 s auto-scrolls. Esc, closing the window, a screen-size change and an invalid drop cancel. A drop never opens the detail dialog afterwards.
- A drag moves one item or one stack. With a bulk selection pending only empty-slot drops to the other side are allowed; swaps and same-side rearrangement are refused.

## Pause

The window opens from the town, a rift or combat. In a rift it sets `RunState.paused = true`, which stops both combat and the dungeon clock (`GameController.Update` does not advance the simulation while paused), and closing restores the previous value so another overlay's pause reason is not overridden. The window's own timers, auto-scroll and toasts run on `Time.unscaledTime`. Opening the storage or swapping presets neither heals nor resets cooldowns, and time spent in the window is not settled as offline reward (`lastSeenUtc` only records save time).

## Verification

`Tests/Editor/StorageTests.cs` adds 22 Edit Mode tests: the fresh account, lossless migration of a save without positions, repair of a damaged capacity array, ordered purchases with fixed prices and no double charge on a repeated request, refusal on insufficient currency, independent expansion with the 100 cap, first-free moves, exact-slot moves and atomic swaps, swaps between full containers, overfull-tab intake and swap refusal with withdrawal and settling, expansion clearing overflow, all-or-nothing bulk moves, sorted-view freeze before a drop, filters and sorts changing no data, slot hand-off on manual equip, wearing from the warehouse and returning to the origin, overflow on a full return, empty slots for missing or unusable registrations, refusal during a rift and unchanged state on a failed write, registration, automatic deposit into the first open tab, duplicate detection, and id-set, count and attribute preservation across moves.

Existing tests that compare item JSON now ignore the position fields (`RiftRarityTests.ItemFingerprint`). The cleanup test's full-warehouse fixture uses the new capacity of 50.

The runtime smoke `RuntimeStorageSmoke` (`-hellscriptStorageSmoke`) prepares real `ItemGenerator` items, `GemInventory` stacks and rune pieces, opens the window and checks eight columns, square cells, the safe area, separate grids and the rail, then drives the cells' real pointer handlers and the window's pointer reading through an exact drop, a swap, a touch hold, a one-second tab hover followed by a drop, a cancel, a sorted-view drop, one-direction bulk selection with the exact message, a bulk move, a double-clicked expansion charged once, ordered tab purchases, preset apply and return, rotation keeping state, and rift pause and resume.

Results and captures are in the [evidence folder](StorageUiEvidence/).

| Item | Result |
|---|---|
| Focused Edit Mode run (storage, inventory, repeat hunt, gems, quality, training equipment, rift rarity, presets, current build, localization, graphics, editor null) | [result](StorageUiEvidence/editmode-focused.txt) |
| Full Edit Mode run | [result](StorageUiEvidence/editmode-full.txt) |
| macOS development build | [log](StorageUiEvidence/build.txt) |
| Runtime smoke `-hellscriptStorageSmoke` | [log](StorageUiEvidence/runtime.txt) |
| Existing inventory smoke `-hellscriptInventorySmoke` | Its warehouse segment was retargeted to the new window, but the smoke stops earlier at the enhancement button: since content unlocks were added it needs a stage-1 clear that its fresh account never has. This predates the storage work; the entry-point checks now run inside the storage smoke. [log](StorageUiEvidence/runtime-inventory.txt) |

Captures are real game screens saved by the smoke.

| Capture | Content |
|---|---|
| [01](StorageUiEvidence/01-landscape-town.png) | Landscape 1600×900 · opened from the town |
| [02](StorageUiEvidence/02-detail.png) | Set item detail |
| [03](StorageUiEvidence/03-detail-comparison.png) | Comparison with the worn item |
| [04](StorageUiEvidence/04-grade-menu.png) | Grade checkbox menu |
| [05](StorageUiEvidence/05-gems.png) | Gem category · account stacks |
| [06](StorageUiEvidence/06-runes.png) | Rune block category · real shapes |
| [07](StorageUiEvidence/07-rune-detail.png) | Rune detail |
| [08](StorageUiEvidence/08-drag-hover.png) | Drag preview over an empty slot |
| [09](StorageUiEvidence/09-drag-tab-opened.png) | Tab 2 opened by the one-second hover, before the drop |
| [10](StorageUiEvidence/10-bulk-selection.png) | Bulk selection |
| [11](StorageUiEvidence/11-expand-dialog.png) | Expansion confirmation |
| [12](StorageUiEvidence/12-buy-dialog.png) | Tab purchase confirmation |
| [13](StorageUiEvidence/13-presets.png) | Equipment presets |
| [14](StorageUiEvidence/14-portrait-town.png) | Portrait 900×1600 |
| [15](StorageUiEvidence/15-portrait-phone.png) | Portrait 390×844 |
| [16](StorageUiEvidence/16-landscape-phone.png) | Landscape 844×390 |
| [17](StorageUiEvidence/17-landscape-rift.png) | Opened during a rift · pause badge |

### Not done and constraints

- Physical touch on iOS or Android, real notches and home bars, and keyboard navigation were not checked. The safe area comes from `UiSafeArea` and was exercised only through desktop window sizes.
- Premium currency has no source or payment path, so tabs 4 and 5 cannot be opened in real play yet. Tests and the smoke set a balance directly to check the rules.
- Materials, gems and runes are account-wide stock in this project and never sit in a hero's bag, so they are not warehouse↔bag transfer targets; the warehouse categories show and detail them. Movement rules apply to equipment.
- Equipment changes remain town-only. Preset application is refused during a rift; storage moves are allowed.
- Grade and sort state stays in session memory like the existing inventory screen; it is not written to a device file.
- Two pre-existing failures are untouched: `LocalizationTests.EveryKoreanLiteralInTheRuntimeHasAnEntry` fails on untranslated lines in the in-progress Hunt Edict files (`CombatSimulation.HuntEdictChanges.cs`, `SkillProgressionInfo.cs`), and `RepeatHuntTests.FreeingBagSpaceRestoresPickupForNewDrops` fails on a baseline clone without these changes as well.

## Decisions to confirm

- A `premium` balance field was added. Its source, payment integration and icon still need a decision.
- Saves holding more than 50 warehouse items under the old rule migrate into tab 1's temporary overflow. If free tab unlocks are preferred instead, the migration branch in `Storage.Normalize` is the one place to change.
- A preset with no registered equipment is refused. If every slot should be emptied instead, remove the first check in `Storage.PlanPreset`.
