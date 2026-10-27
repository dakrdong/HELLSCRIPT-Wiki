# Item action menu — user item 9 (2026-10-24)

[한국어](Item_Action_Popup_20261024.md). Tapping an item slot once opens a **small menu beside the slot** instead of the big detail window. Its rows depend on the window and slot that was tapped and on whether the item is locked or worn. Details and comparison open the existing big windows (`ItemDetailView`, `EquipmentComparisonView`); no new detail window is built.

Mockup: the design canvas `HELLSCRIPT 창고 · 캐주얼` (https://claude.ai/artifact/BVaVczJFgajuJxeHfEw1R8), the `menu` prop (none · wh · bag · baglow · locked · full) and `아이템 메뉴 · 항목 규칙` board. The user accepted the recommendation to use the existing H mockup; implementation proceeds within the scope below (approval evidence at the end).

## 1. Scope

| Window | Slots | This batch |
| --- | --- | --- |
| Warehouse (`StorageWindow`) | warehouse slot, bag slot | menu |
| Character screen (`InventoryWindow`) | bag slot, worn slot | menu (while a tutorial lesson waits for the old path, the big window opens directly) |
| Shop sell list (`EquipmentShopWindow`) | sell slot | **unchanged**: tapping still toggles the sale selection. A menu there is a separate decision for a later batch (U-D touches the shop list) |
| Rift rewards, recommended gear, reward boxes, forge, codex, character select, idle dashboard | — | unchanged (each opens a big window with its own action buttons) |

## 2. Today's slot taps (code survey, paths under `Assets/HELLSCRIPT/Runtime/`)

No double-tap, long-press, right-click or Ctrl/Shift-click shortcut exists on any item slot (searched everywhere). Dragging is the only shortcut to keep.

| Window / slot | Tap does | Resulting window | Where |
| --- | --- | --- | --- |
| Warehouse / bag slot (not bulk) | `CellClicked` → `ShowDetail` | detail + location + destination; buttons Withdraw/Store, Lock/Unlock, Compare/Close comparison | Presentation/StorageWindow.cs:239-251, StorageWindow.Dialogs.cs:23-65 |
| Warehouse bulk mode | toggles selection (one side only) | none | StorageWindow.cs:246-250 |
| Warehouse drag | any movement lifts the item at once → move/swap on drop | ghost + hint | StorageWindow.Drag.cs:50-92 |
| Warehouse stacks (materials, gems, rune blocks) | `ShowStack` | read-only | StorageWindow.cs:243, Dialogs.cs:67-92 |
| Character screen bag / worn slot | `InventoryCell` → `SelectItem` → `ShowDetail` | comparison view for unworn items, detail for worn; buttons `inventory-equip`, `inventory-lock`, `inventory-detail-close` | InventoryWindow.Drag.cs:15, InventoryWindow.cs:190-198, Dialogs.cs:26-57 |
| Character screen dismantle mode | toggles selection (protected items refused) | none | InventoryWindow.cs:190-198 |
| Character screen drag | mouse at once, touch after 0.18 s → equip/unequip | ghost | InventoryWindow.Drag.cs:18-62 |
| Shop sell slot | `ItemClick`: manual = toggle selection, else inline detail / `ShowMockItem` | sell | EquipmentShopWindow.cs:174-179 |
| Rift rewards (`victory-loot-*`), recommended gear popup, … | big `ItemDetailPopup` | compare / equip | RiftVictoryWindow.Loot.cs, RiftRecommendationWindow.cs |

Timing constants: warehouse click suppression after a drop 0.35 s, inventory 0.2 s, warehouse tab hover 1 s, toasts 3.2 s.

## 3. New behaviour

### Row rules (one place: `Core/ItemActionMenu.cs`)

| Tapped | Details | Move | Lock | Compare with worn gear | Equip / unequip |
| --- | --- | --- | --- | --- | --- |
| Warehouse window · warehouse slot | ● | Move to bag | Lock / Unlock | ● | – |
| Warehouse window · bag slot | ● | Move to warehouse | Lock / Unlock | ● | – |
| Character screen · bag slot | ● | – | Lock / Unlock | ● | Equip |
| Character screen · worn | ● | – | Lock / Unlock | – | Unequip |

- A blocked row is greyed, not hidden, with its reason on a second line. Reasons are the existing pre-check strings: moves `Storage.MoveError`, unequip `EquipmentSlots.UnequipError` and the required tutorial armor (`Tutorials.RequiredArmor`, its existing armor-protection line). Equip is enabled when **any** place in `EquipmentSlots.Targets` passes `Plan(hero,item,index)`; when all are blocked the first reason shows (checking only the default place cannot stand for rings or two-hand slots). `SetInventoryLock` returns only a generic error for unlocking the required armor, so the menu pre-checks `RequiredArmor` and shows the same armor line; the transaction's own check stays.
- Lock keeps each window's existing path: warehouse `Transact("storage-lock:…")`, character screen `GameStore.SetInventoryLock`. Locked items can still be moved (existing rule). Sale and salvage exclusion already read the lock (`EquipmentShop.SaleExclusion`, `InventorySalvagePlan.Eligible`).
- Equip: one target → `EquipInventoryItem` at once; two targets (rings, hands) → the existing comparison view opens so the player chooses (existing `choose` flow).
- Details → `ShowDetail(id)` (the character screen gains a parameter so an unworn item can open as details instead of a comparison). Compare → warehouse `ShowDetail(id,true)`, character screen the existing comparison. A comparison opened from the menu returns to the grid when closed.

### Opening and closing

- One tap = menu. Bulk-move and dismantle selection modes keep selecting only (no menu). Dragging is unchanged.
- Closes on: tap outside (that tap is consumed), choosing a row, back / Esc, grid scroll, tab / category / order change, closing the window.
- On repaint: transactions and save notifications rebuild the grid (`Repaint()` recreates slots), so the menu keeps only the item id, re-finds the slot, re-attaches beside the same item, and closes if the item is gone. Item references are replaced by every transaction.
- Placement: `ItemMenuPlacement.Compute` — right of the slot (12px gap), else left, else below, else above; kept 8px inside the plate; the arrow points at the slot centre (at least 22px from the corners). Touch rows are at least 44px tall (PC 50).

### Tutorial and guide compatibility

| Lesson | Dependency | Handling |
| --- | --- | --- |
| Armor errand step 3 | `GameUI.TownErrands.cs:68` points at the `inventory-equip` button of the character screen; the guide overlay (`PrologueGate`) blocks all other input while the target is visible and lets go after about 2 s without it | While the `GetArmor` errand is unfinished a slot tap opens the old big window directly (`ItemActionMenu.LessonWaitsForDetail`) |
| Guide "equipment" step | `ShowDetail` runs `ReviewInventoryItem(id,compare)`, recording H01/H14 and `FirstPlayGuide.ReadComparison` | Direct only for tutorial accounts (`Tutorials.Mandatory`) with a comparison candidate and the step unfinished. The check is **per account state, not per tapped item**, so every character-screen slot is direct meanwhile (the step completes when an unworn wearable item is read). The menu afterwards |
| Warehouse lesson H04 | progresses by transaction names (`storage-move`/`place`/`bulk`/`preset`/`register`) | independent of slot taps; the menu's move is the same `MoveStorageItem` transaction |
| `storage-lock`, `inventory-lock`, `inventory-unequip` | not mapped to lessons | operation names stay (`SoundBankTests` checks the strings) |

## 4. Implementation files

- New: `Core/ItemActionMenu.cs` (rows, labels, lesson check), `Presentation/ItemMenuPlacement.cs` (placement), `Tests/Editor/ItemActionMenuTests.cs`.
- Screen code (after approval): one shared menu part (`ItemActionMenuView`: blocker + plate + arrow + rows, `UiButton`), wired into `StorageWindow` and `InventoryWindow` slot taps.
- Row button names: menu rows are `storage-menu-<action>` / `inventory-menu-<action>`; the detail footer keeps `inventory-equip` and `inventory-lock` (the errand and smokes search for those names).
- Help text: the sentence "아이템을 눌러 상세 정보를 열고 …" in `StorageWindow.Tools.cs` becomes a description of the menu, and its English entry is changed in place (the orphan-entry test forbids keeping the old key). That line is mid-file, so another branch touching it can conflict on merge.
- Strings: every Korean literal needs an entry in `Resources/Localization/en.txt`. This work appended four lines at the end of the file (상세 정보 · 소지품으로 이동 · 창고로 이동 · 착용 장비와 비교); another branch appending at the end can conflict on integration.

## 5. Verification plan

- EditMode: `ItemActionMenuTests` (context × state row matrix, blocked rows, English entries, lesson check, placement) plus the affected existing tests (`StorageCasualSkinTests`, `NativeInventoryTests`, localization tests).
- `python3 tools/check_ui_contract.py --verify`, `python3 tools/check_ui_refresh.py`.
- Runtime smokes (affected part): the warehouse smoke assertion "a normal click opens item details" changes to menu → Details; the character-screen smokes (`ItemClick` then `inventory-lock` …) change the same way. Heavy Unity runs go through the board's `run --resource unity`, one at a time.
- Stills (Korean portrait): a warehouse slot, a bag slot and an edge slot, one each.

## 6. Facts / hypotheses / decisions / not run

- Facts: the tables above (code reading). The original handoff records local PC/portrait/landscape mock rendering. The takeover session did not operate the shared Claude UI or verify Unity screens.
- Hypothesis: the guide "equipment" check (`Mandatory` + candidate present) behaves as intended on every older save — not checked against real save files.
- Resolved: use the existing mock, Move to warehouse/Move to bag labels, include the character screen, exclude shop sale selection, preserve lesson direct entry and show blocked rows/reasons. No repeated approval is requested.
- Not run: Unity compile, EditMode, smokes, stills (no code has run yet), physical devices.

Approval evidence: in the trusted 2026-10-24 conversation, the user replied **“다 동의하니 작업해줘”** to recommendations including the existing H mock. The parent integration owner delegated this precise H scope. This is not recorded as new approval or as a Claude UI verification by this session.

## 7. Takeover implementation

Work starts from latest `origin/main 1f1887d1148d99e5b5f42f9a899d8945c6209259` on isolated `codex/item-action-popup-20261024`. The stopped owner's rules, placement, model tests, original metas and four translations were copied unchanged before integration. Neither the old workspace, E branch nor root integration worktree was edited.

- One `ItemActionMenuView` adapts the existing canvases using shared `UiSkin/StorageSurface/UiButton`. Details/comparison use `ItemDetailView/EquipmentComparisonView`; no second item renderer, canvas, font or art is introduced. `ItemActionMenu.Rows` owns contextual row rules.
- The owner stores only an item ID, reacquires the current instance before each action and uses the existing transaction. Failure retains owner/disk and the existing error toast; selecting an action closes the menu. Multiple equip targets keep the comparison's existing target chooser.
- When one two-handed item appears in both hand cells, UI state also retains the physical hand that was tapped. Repaint prefers that same cell so the anchor/highlight never jumps to the other hand.
- Existing save bindings and keys are reused; inventory adds only the lesson direct-entry predicate. Meaningful repaints reacquire cells while retaining the menu/buttons. Missing, filtered, off-tab or clipped anchors close it.
- Restoration suppresses scroll dismissal and records the restored actual offset. Actual motion or wheel input closes the menu; unchanged saves do not rebuild it. There is no new Update or per-frame query.
- Default width is 176 portrait/landscape and 224 PC, widening when long blocked descriptions need height. Touch rows are at least 44 screen pixels, PC defaults to 50; fonts are not shrunk. Anchors must fit the safe area, owner window and every parent mask. The handed-over 12/8/22px placement rules remain.
- Outside input is consumed. Back/Esc, real scrolling, tab/category/order changes, drag start, owner close and disappearance dismiss the menu. Bulk/salvage selection, drag thresholds and post-drop click suppression remain.
- Armor errands and unfinished equipment lessons keep direct detail entry and `inventory-equip`/`inventory-lock` targets. The Details action opens single detail; Compare opens the existing comparison and returns to the grid. Existing direct `ShowDetail` callers retain their default comparison behavior.

## 8. Validation handoff

`ItemActionMenuTests` retains the original model/placement/lesson/translation checks. `ItemActionMenuWindowTests` covers normal/full/protected states, one valid ring target, failed saves, ID reacquisition, retained controls, restored versus actual scroll, disappearance, consumed outside clicks, Back/Esc/owner close, selection/drag and lesson direct entry. It also checks menu/text bounds at the five required aspects in both languages.

`RuntimeInventorySmoke.ItemClick` selects the menu action leading to the prior detail/comparison target; `RuntimeStorageSmoke` verifies/captures the menu before selecting Details. Other content click paths are untouched. The parent coordinates affected fixtures, UI contracts/refresh, native interaction and captures on the final candidate.

**No Unity, Edit Mode, smoke, build, device or performance run was executed in this workspace.** Python static evidence lives under `Builds/Validation/ItemActionPopup20261024/`. Actual Unity-to-mock pixel matching, interaction and performance improvement remain unverified.

The final `check_ui_contract.py --verify` passed 15 Python regressions; `check_ui_refresh.py` passed 14 guarded bindings and inventoried 240 UI sources. `git diff --check` passed. SHA-256 confirms all three handed-over code/meta pairs remain identical to their originals. Fourteen model cases plus 25 window cases are **authored counts, not executed results**. The UI contract suite ran four times as inputs changed; `ui-contract.json/.log` identifies the final proof.

## Integrated focused validation update (2026-10-24)

Candidate5936415e includes latest main1f1887d1. The 23 directly affected E/H/F fixtures first stopped at a new test's List.Length compile error, corrected to Count. The subsequent run passed521/550;29 H/F cases failed an EditMode EventSystem assumption. The final corrected H/F run passed40/40. Together with the510 unchanged passing cases this establishes550/550 unit coverage. Runtime source did not change for these fixture corrections. Evidence: local takeover/e-h-f-unit-proof.json, e-h-f-retest/results.xml, h-f-final-unit/results.xml. All9 local CI steps passed (takeover/integration-ci/report.json; installedPython3.9.6/Node26.8.2, exact parity with workflow3.12/22 unverified).

New EventSystems do not register as global current in EditMode, so fixtures use their own instances. Only the known deferred-Destroy EditMode engine diagnostic is expected by its exact message; other errors still fail, and failed-save diagnostics are checked separately. Unit assertions retain real hit-area geometry, filter decisions, event dispatch, persistence, draft and navigation checks.

**Rendered global Canvas raycasts, native pointer interaction and Korean/English captures remain pending.** The five empty-hit failures persisted with Metal batch and are retained in h-f-raycast-graphics. They are not reported as native success; rendered-frame validation is a separate requirement. Nine own temporary/import files were preserved with hashes before restoring HEAD bytes. Actual local CLI review and remote integration need their own subsequent evidence.

## Multi-position equipment from Details (2026-10-25)

The actual Sol/Max integration review found that the single information card could equip rings/one-handed weapons without choosing the target position. Details remains a single card; Equip continues to the existing comparison/position chooser when the unequipped candidate supports multiple positions. No equipment changes until the final choice. The existing transaction and disk persistence remain authoritative. New two-ring and dual-hand regressions reproduced both failures before the fix; the directly affected104/104 cases passed in item-menu-review-fix/green2/results.xml, including both choice regressions and disk reload.

Development-only RuntimeUserReportNativeAcceptance requires a fresh isolated save/evidence pair and disabled server configuration for real first-entry tour and item-menu routes. Synthetic actions and the final external-pointer stage are labeled separately. The driver now also compiles under Unity6000.6 after replacing obsolete numeric object IDs with retained component references; the compiler failure is preserved. Native player, rendered and external-pointer acceptance remain pending Mac unlock.
