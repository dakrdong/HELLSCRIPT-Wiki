# Inventory potion slots and account balances

Updated: 2026-09-23

Three small potion-only slots sit in the corner beside the weapon slots, and actual owned balances appear at the bottom of the inventory. Potions belong to each character; currencies and crafting materials belong to the shared account. This is connected to the existing Unity play-scene inventory, not just an HTML prototype.

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

A fixed bottom strip shows names and exact counts, including zero balances and thousands separators.

| Display | Existing save field |
| --- | --- |
| Gold | `AccountSave.gold` |
| Abyssal Coins | Existing premium currency, `AccountSave.premium` |
| Materials | `AccountSave.materials` |
| Enhancement stones | `AccountSave.enhancementStones` |
| Eight slot-specific cores | `AccountSave.cores`: weapon, head, chest, hands, feet, belt, amulet and ring |

“All resources” includes every core balance. The sheet has fixed outer bounds and an internal scroll area. Successful saves refresh the open sheet's values without moving it or resetting its scroll position. This view never grants, spends or transfers resources. Abyssal Coins rename the existing `premium` display, as confirmed by the user; no separate balance is introduced.

The Abyssal Coin PNG is copied unchanged from the blacksmith prototype. Its [provenance](PotionSlotsEvidence/abyssal-coin-source.json) and [original generation prompt](PotionSlotsEvidence/abyssal-coin-generation-prompt.txt) are preserved. SHA-256 is `6f48ca86c7f47f886c7c009176200a30c0d942b5c6f610a2dc2631f8a7ee9097`; native alpha and 1,254×1,254 dimensions remain intact. Its existing prototype-candidate status and unverified model provenance remain unchanged.

## Shared UI and validation

The existing fixed `InventoryWindow` adapter reuses `EquipmentSlotView`, `UiTheme` and `UiFonts`. `PotionArt` shares bottle rendering between inventory and HUD. `InventoryWindow.Wallet` reads account state and refreshes through `StoreViewBinding` after successful saves. Potion bubbles keep their initial dimensions; selection changes never shift the bag.

- Unity 6000.6.0f1: **126 focused Edit Mode tests passed, zero failed or skipped**, covering fallback ordering, duplicate/type rejection, save failures, reload, combat, training isolation, existing HUD and localization. [XML results](PotionSlotsEvidence/compact-editmode.xml)
- Native macOS development build: **20 combinations** of 440×956, 956×440, 1440×810, 1440×900 and 1680×720; Korean/English; 100%/150% text. Actual uGUI events and raycasts verify policy choice, assignment, rejected equipment drops, reload and HUD propagation. [Runtime result](PotionSlotsEvidence/compact-potion-slots-result.txt)
- All four balances and eight core counts match account state. Exact zero and 2,147,483,647 values, original coin loading, read-only inspection, successful-save refresh and retained scroll position pass.
- Shared UI ownership checks, nine checker tests and the potion DB test pass. Physical-phone input and performance have not been tested; native acceptance uses synthetic macOS input and isolated fixture saves.

Representative captures: [portrait](PotionSlotsEvidence/compact-slots-440x956-ko-100.png), [landscape](PotionSlotsEvidence/compact-slots-956x440-ko-100.png), [large English bubble](PotionSlotsEvidence/compact-policy-440x956-en-150.png), [large landscape bubble](PotionSlotsEvidence/compact-policy-956x440-ko-150.png), [resource/core scrolling](PotionSlotsEvidence/compact-wallet-scrolled-440x956-en-150.png), [21:9 PC](PotionSlotsEvidence/compact-slots-1680x720-en-100.png).

## Source owners

- [Assignments and acquisition batches](../../Assets/HELLSCRIPT/Runtime/Core/PotionLoadout.cs)
- [Potion transactions](../../Assets/HELLSCRIPT/Runtime/Core/GameStore.Potions.cs)
- [Potion slots and speech bubble](../../Assets/HELLSCRIPT/Runtime/Presentation/InventoryWindow.Potions.cs)
- [Account balance display](../../Assets/HELLSCRIPT/Runtime/Presentation/InventoryWindow.Wallet.cs)
- [Exhaustion, persistence and combat tests](../../Assets/HELLSCRIPT/Tests/Editor/PotionLoadoutTests.cs)
- [Native macOS input and layout acceptance](../../Assets/HELLSCRIPT/Runtime/Presentation/RuntimeInventorySmoke.Potions.cs)
