# Global HUD 05: three potion types

Date: 2026-09-14

Use a red angular HP vial, slim blue MP vial and round amber utility flask. In landscape, put all three above skills and scale only bottle artwork to 0.75 of baseline width and height. Count sits lower right; remaining cooldown is separate central text. Do not scale numbers down with bottles. Portrait keeps its approved sizes and row order.

| State | Display |
|---|---|
| Available | Bottle and actual inventory count. |
| Cooldown | Darkening and remaining time while retaining count. |
| Empty | Dim artwork and `0`, without starting a false cooldown. |
| Unequipped/locked | Empty or localized locked state. |
| Inventory unconnected | `—` and development-only unconnected state, never fictitious quantities. |

Current `CombatSimulation.Effects.TryUsePotion()` has an unlimited automatic HP potion and one `potionCd`. This preparation does not change its healing, threshold or cooldown. Before implementation, define `PotionState` with type/equipped item/count/remaining/effective total/unlock/unavailable reason. Inventory owns quantity; simulation atomically decides and consumes uses; the HUD only reads.

Content design must resolve MP recovery/threshold, the actual utility effect/trigger, supply/refill rules and legacy-save initial quantities or compensation. Independent cooldowns are proposed. If a shared lock is required, show the maximum of individual and shared remaining time and identify the reason. Do not invent economy or combat numbers during HUD wiring.

`potion-hp`, `potion-mp` and `potion-utility` are real-alpha PNGs. Preserve bottle alpha when darkening cooldowns, or use a separate compact mask. Acceptance covers 0/1/999 items, simultaneous automatic conditions, shared/individual locks and save/load without double consumption. No manual potion activation is attached.

한국어: [물약](HELLSCRIPT_GlobalHUD_05_Potions.md).
