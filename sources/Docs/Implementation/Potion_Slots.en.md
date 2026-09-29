# Inventory potion slots and account balances

Updated: 2026-09-29

Three small potion-only slots sit in the corner beside the weapon slots, and actual owned balances appear at the top, directly below the inventory title. Potions belong to each character; currencies and crafting materials belong to the shared account. This is connected to the existing Unity play-scene inventory, not just an HTML prototype.

한국어: [인벤토리 물약 슬롯과 보유 재화](Potion_Slots.md)

## Assignment and exhaustion policy

- Potion cells measure 32 in portrait and 28 in landscape, smaller than equipment. They share the weapon row in its right corner instead of adding a row. Removing the old row shortens the portrait character panel by 50 and expands the bag. Existing bottle art remains centered without changing its source.
- One shared gear sits beside the three cells. Clicking it opens “When the assigned potion runs out.” Choose higher grade, lower grade, newest acquisition or oldest acquisition. The checked, highlighted choice applies to all three slots.
- Clicking a potion cell opens its owned-potion picker directly; “Unequip” clears it. Unowned potions and IDs assigned elsewhere are disabled. Dropping equipment on a potion slot does not equip it.
- Assigned stock takes priority. Once exhausted, only available potions with the **same effect** qualify. Other slots' assignments and already-resolved fallback IDs are excluded. With no candidate, use stops.
- Assignment changes retain the existing town restriction. A single shared policy persists per character and may change during battle. Opening the UI or changing a policy never consumes stock. Older per-slot saves migrate the first assigned slot's policy once; after saving the shared value, legacy slot values cannot override it.

All eight current Unity definitions have grade 0 and no same-effect tier alternatives. Grade and acquisition ordering are data-driven, but this change does not invent additional potion balance or prices. Exhausted slots with no same-effect alternatives stop using potions. Multiple utility assignments still share the existing cooldown and single active effect.

`GameStore.SetPotionSlot` and `SetPotionFallback` save through existing transactions. Failed writes retain previous memory and disk state. `PotionLoadout` resolves replacements; `PotionInventory` owns stock and acquisition batches. Saves without historical acquisition records use stored stack order once, rather than claiming to reconstruct timestamps. See [potions and hunting edicts](../Design/GlobalHUD/HELLSCRIPT_GlobalHUD_05_Potions.en.md) for the full rules.

## Owned resources

A fixed strip directly below the title spans the full window width in portrait and landscape, with equipment and the bag below it. It shows names and exact counts, including zero balances and thousands separators. Bag scrolling and balance updates do not move or resize the strip.

| Display | Existing save field |
| --- | --- |
| Gold | `AccountSave.gold` |
| Abyssal Coins | Existing premium currency, `AccountSave.premium` |
| Materials | `AccountSave.materials` |
| Enhancement stones | `AccountSave.enhancementStones` |
| Eight slot-specific cores | `AccountSave.cores`: weapon, head, chest, hands, feet, belt, amulet and ring |

“All resources” includes every core balance. The sheet has fixed outer bounds and an internal scroll area. Successful saves refresh the open sheet's values without moving it or resetting its scroll position. This view never grants, spends or transfers resources. Abyssal Coins rename the existing `premium` display, as confirmed by the user; no separate balance is introduced.

The Abyssal Coin PNG is copied unchanged from the blacksmith prototype. Its [provenance](PotionSlotsEvidence/abyssal-coin-source.json) and [original generation prompt](PotionSlotsEvidence/abyssal-coin-generation-prompt.txt) are preserved. SHA-256 is `6f48ca86c7f47f886c7c009176200a30c0d942b5c6f610a2dc2631f8a7ee9097`; native alpha and 1,254×1,254 dimensions remain intact. Its existing prototype-candidate status and unverified model provenance remain unchanged.

## Currency artwork and inventory space

Since 2026-09-29, each currency name, icon and exact amount share one line. Landscape uses four columns in a fixed 24-unit strip; portrait uses two columns over two rows in 40 units to retain readable English names and large balances. Changing the amount never moves the name or icon.

The Inventory heading decreases from 16 to 12. Permanent protection and drag guidance, continuous new-equipment review, and its dedicated navigation/storage flow are removed. Individual item details, locking and equipment comparison remain. Errors and equip feedback briefly overlay the grid without occupying a layout row. Selection, cancellation and feedback preserve its bounds. The item viewport gains 94 layout units in landscape and 88 in portrait. Slot size, capacity, positions, balances and salvage protection rules remain unchanged.

The enhancement stone is silver ore with golden cracks. Eight cores share a dark crystal and metal reliquary, with crimson weapon, violet head, cobalt chest, orange hands, emerald feet, cyan belt, champagne-gold amulet and rose-pink ring auras. The shared `CurrencyIconView` supplies the inventory wallet and blacksmith core list. Wallet artwork occupies 36 units within the unchanged 42-unit row and fixed scrolling modal.

Each image was generated/edited with the built-in `image_gen` tool. Original 1,254×1,254 PNG bytes and native alpha are retained without recoloring, matting or background removal; imported player textures use the 512 platform budget. [Prompts and provenance](../Art/Currency/generation.json) and [alpha validation](../Art/Currency/alpha-validation.json) are preserved. The tool did not return a model name, so use of `gpt-image-2` cannot be verified. **These are candidates with unverified model provenance, not release-approved art.** One enhancement-stone corner retains its original 1/255 alpha.

## Shared UI and validation

The existing fixed `InventoryWindow` adapter reuses `EquipmentSlotView`, `UiTheme` and `UiFonts`. `PotionArt` shares bottle rendering between inventory and HUD. `InventoryWindow.Wallet` reads account state and refreshes through `StoreViewBinding` after successful saves. Potion bubbles keep their initial dimensions; selection changes never shift the bag.

- Unity 6000.6.0f1: **126 focused Edit Mode tests passed, zero failed or skipped**, covering fallback ordering, duplicate/type rejection, save failures, reload, combat, training isolation, existing HUD and localization. [XML results](PotionSlotsEvidence/compact-editmode.xml)
- After moving balances to the top, **69 inventory/localization Edit Mode tests passed, zero failed or skipped**. The native checks below also require the strip to sit directly below the title, span the full window width and avoid overlapping equipment or the bag. [Header-balance XML](PotionSlotsEvidence/header-editmode.xml)
- Native macOS development build: **20 combinations** of 440×956, 956×440, 1440×810, 1440×900 and 1680×720; Korean/English; 100%/150% text. Actual uGUI events and raycasts verify policy choice, assignment, rejected equipment drops, reload and HUD propagation. [Runtime result](PotionSlotsEvidence/header-potion-slots-result.txt)
- All four balances and eight core counts match account state. Exact zero and 2,147,483,647 values, original coin loading, read-only inspection, successful-save refresh and retained scroll position pass.
- Shared UI ownership checks, nine checker tests and the potion DB test pass. Physical-phone input and performance have not been tested; native acceptance uses synthetic macOS input and isolated fixture saves.

Representative captures: [portrait](PotionSlotsEvidence/header-slots-440x956-ko-100.png), [landscape](PotionSlotsEvidence/header-slots-956x440-ko-100.png), [large English bubble](PotionSlotsEvidence/header-policy-440x956-en-150.png), [large landscape bubble](PotionSlotsEvidence/header-policy-956x440-ko-150.png), [resource/core scrolling](PotionSlotsEvidence/header-wallet-scrolled-440x956-en-150.png), [21:9 PC](PotionSlotsEvidence/header-slots-1680x720-en-100.png).

## Source owners

- [Assignments and acquisition batches](../../Assets/HELLSCRIPT/Runtime/Core/PotionLoadout.cs)
- [Potion transactions](../../Assets/HELLSCRIPT/Runtime/Core/GameStore.Potions.cs)
- [Potion slots and speech bubble](../../Assets/HELLSCRIPT/Runtime/Presentation/InventoryWindow.Potions.cs)
- [Account balance display](../../Assets/HELLSCRIPT/Runtime/Presentation/InventoryWindow.Wallet.cs)
- [Exhaustion, persistence and combat tests](../../Assets/HELLSCRIPT/Tests/Editor/PotionLoadoutTests.cs)
- [Native macOS input and layout acceptance](../../Assets/HELLSCRIPT/Runtime/Presentation/RuntimeInventorySmoke.Potions.cs)

## 2026-09-29 artwork and compact-layout validation

Unity Edit Mode passed **72 tests with zero failures or skips**, on source including main's Hunt Edict and attendance-icon changes (`323dca88`). Coverage includes native PNG alpha, nine distinct sprite loads, the 512 texture budget, equipment transactions, salvage protection and localization. [XML](CurrencyInventoryEvidence/editmode.xml)

The macOS development player passed **20 combinations**: 440×956, 956×440, 1440×810, 1440×900 and 1680×720; Korean/English; 100%/150% text. It checks inline labels and exact zero/int.MaxValue balances, eight slot-specific core images, an expanded fixed bag viewport, selection/toast stability, committed balance refresh, retained wallet scrolling and potion interactions. [Runtime result](CurrencyInventoryEvidence/runtime.txt)

In the same build, a test character meeting the core-crafting unlock requirement walked to the blacksmith's actual interaction radius. All eight core images were verified in the crafting list. [Blacksmith](CurrencyInventoryEvidence/blacksmith.png)

The existing landscape/portrait inventory acceptance also passed: individual/ring comparisons, equip and drag, locks, selected/bulk salvage, saved filters and restoration of the battle pause when opening/closing inventory. [Interactions](CurrencyInventoryEvidence/interactions.txt)

Input uses native macOS uGUI raycasts and synthetic pointers with isolated saves. No CoplayDev Editor instance was connected, so validation used Unity batch tests and a standalone macOS build. Physical-phone input and performance remain untested.

Screens: [landscape](CurrencyInventoryEvidence/landscape.png), [portrait](CurrencyInventoryEvidence/portrait.png), [wallet](CurrencyInventoryEvidence/wallet.png), [all cores at enlarged English text](CurrencyInventoryEvidence/cores-en-large.png).
