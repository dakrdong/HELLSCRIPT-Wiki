# Warehouse window casual skin — 2026-10-18

[한국어](Casual_Storage_Window_20261018.md)

Basis: the design canvas "HELLSCRIPT 창고 · 캐주얼" (BVaVczJFgajuJxeHfEw1R8), build spec section k (game mapping) and f (component recipes). Started from `origin/main` `19072415`. Transfer, drag, bulk-move, tab-purchase and expansion transactions and the control names (`storage-*`, `grade-*`, `order-*`, `category-*`, `bulk-*`, `preset-*`) are unchanged.

## What changed

- `StorageWindow.Style.cs` owns every surface. `Surface(r, name)` now maps to the casual `StorageSurface.SetSkin`: plate `panel-base` (navy 3px, radius 18/24, drop shadow), panel `storage-panel` (white, card edge), rail `transfer-rail` (cream), well `input-well`, empty slot `slot-empty`, overflow note `notice-error` (coral), and `dialog-panel` for dialogs, popovers and toasts. Text uses `UiSkin.LabelFont` (Jua, Noto when a glyph is missing) and `BodyFont`; colours are `UiSkin` tokens (Navy, Muted, OrangeInk, GreenInk, WarningInk, SkyInk).
- The window is a rounded plate floating 8–12 units inside the safe area over a navy α .45 veil instead of filling it. The header carries the brand, a chest gem with the golden title (the `InventoryWindow` title recipe) and a round coral close button; opened during battle it adds a yellow pause pill (own row under the header on portrait).
- Every `UiButton` uses `UiSkin.Casual.ApplyChrome` (confirm = green with a white outlined label, close = `CasualClose`). Vault tabs are white plates, yellow with a tail diamond when current; purchasable shows a green plus, locked a grey padlock, overflow a coral "!" (`StorageChestOverlay`).
- Item slots use `EquipmentSlotView.Paint(…, UiSkin.Casual.WithEquipmentArt(CasualEquipmentArt.Provider))` for the approved casual equipment art and grade rings. Item detail, equipment comparison and the preset slots receive the same `skin`.
- Toast, tab hint, drag hint, grade popover, order menu and the rename, presets, guide, history, tab-purchase and expansion dialogs use the same surfaces and text rules.

## Differences from the mockup (decisions needed)

- The transfer rail has a solid card edge instead of a 2px dashed one; `StorageSurface` dashes are 1px only and the shared part was not changed. Overflow and expansion slots are solid for the same reason.
- Some small ornaments are not applied: the usage pill, the colour band under the category row, the tab numeral placement and the level outline on item slots.
- Dimensions keep the game's units and only adopt the mockup's proportions (plate gap 8/12, header 36/44, rail 86/54, radii); the mockup's 1600×900 board pixels were not copied.
- Grade rings use the mockup's bright palette (#8a93b5 #35a7ff #ffc21a #ff9f1c #4cbf55) in this window. Whether to unify them with the inventory's `EquipmentGradePalette` rings is open.

## Verification

- 89 focused EditMode tests pass: `StorageCasualSkinTests` (6 new), `UiButtonTests`, `StorageTests`, `EquipmentStorageTests`. The new tests check plate, panel, rail and tab geometry in landscape and portrait, casual fonts, colours and chrome on every text and button, and the dialog, toast, popover, bulk and overflow states.
- Static UI contract (15) and refresh guard (14 bindings) pass.
- The real window was rendered to PNG in the Editor (PC, landscape, portrait, bulk move, grade popover, toast, guide, tab purchase, expansion, item detail, gem stacks) and checked by eye (`Assets/HELLSCRIPT/Editor/StorageCasualEvidence.cs`, 640×480).
- Not verified: the development-player `RuntimeStorageSmoke` (drag, auto-scroll, transaction wiring) and a macOS build, physical devices and touch, long English strings. The full EditMode suite was not run.
