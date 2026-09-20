# HELLSCRIPT Weapon and Equipment Shop Proposal

작성일: 2026-09-20 · Status: design review, not implemented · [한국어](HELLSCRIPT_Equipment_Shop_Proposal.md)

The shop should bridge small gaps between earned drops: buy a modest improvement, invest in the existing enhancement system, then return to rifts. Repeated shop purchases must not create an independent progression ladder. This document and its mockup are review artifacts. They do not read player saves or modify the game. All thresholds and price multipliers below are initial tuning hypotheses.

## Reference and visual direction

The in-app browser was used on September 20, 2026 to inspect the [weapon vendor screenshot and description](https://guides4gamers.com/diablo-4/pois/weapons/). Its image path dates to March 2023; this is not a claim of exact current-season parity. The [weapon vendor](https://diablo4.wiki.fextralife.com/Weapons+Vendor) and [armor vendor](https://diablo4.wiki.fextralife.com/Armor+Vendor) community pages, updated May 2, 2026, describe limited stock, gold purchases and sales, and a 60-minute stock refresh with a visible timer. Blizzard's internal generation algorithm was not available.

Retain the left vendor list, right equipment/bag, item details and comparison, buyback, and stock timer. Adapt them to HELLSCRIPT's existing layout. The 12-item persistent buyback buffer, six offers, recommendations, and power policy are our proposals, not verified D4 rules. Keep enhancement, dismantling and rerolling at the existing blacksmith. The unidentified equipment shop remains a separate gambling service.

The actual [storage](../Implementation/StorageReferenceEvidence/storage-reference-01-landscape-town.png), [rune board](../Implementation/RuneV13Evidence/02-reference-landscape-ko.png), and [Hunt Edict](../Implementation/HuntEdictGameUiEvidence/01-landscape-skills.png) screenshots were inspected. Use the recent storage/rune board's charcoal-olive surfaces, thin brass lines, serif headings, square inventory cells and fixed action areas. Storage tokens are background `#1a1c16`, heading `#c0a777`, text `#d9d1be`, and primary action `#702e28` to `#461b18`. Reuse the existing 6×4 equipment atlas. A small preview derivative does not modify the game asset. Recommendation marks must not resemble a higher rarity.

## Layout and transactions

Wide screens show offers on the left, selection and comparison in the middle, and equipped gear plus an eight-column bag on the right. The header holds the title, language and close action. Buyback remains near the offer area; gold remains in the footer.

On portrait and short landscape screens, show the offer list first and switch to details on selection, with a return control. In the future Unity implementation, keep navigation, currency and purchase action inside the safe area and scroll only the content. Do not squeeze three columns onto a phone. Support 320px minimum width, 390×844, 844×390 and 1600×900, then verify 125%/150% text and physical mobile safe areas. The inline mockup expands to its content height; it does not prove Unity scrolling or physical-device behavior.

The selection shows the exact generated instance, all stats, required hero level, item level and price. Default comparison uses currently equipped gear. If accessible bag/warehouse gear is better in the same role, show its location and allow comparison with it. Enhancement preview displays its separate cost; the purchased item is still +0. Purchase validates gold, space, offer generation and ID, then atomically inserts the exact instance into the bag and marks that offer sold. It does not equip automatically or immediately refill the slot.

Sell from the bag. Preserve existing equipped, locked, preset-reference and socketed-gem protections, including account-wide references. Bulk selling is outside version one. Buyback retains the last 12 complete instances, original IDs and sale prices across closing and normal restart. Refresh does not clear it. Warn before a thirteenth sale evicts an entry. Rebuy never rerolls affixes, enhancement, sockets, origin or acquisition order. Insufficient funds, full bag, expired stock and duplicate input must leave state unchanged or apply exactly once via a receipt.

## Three separate progression measures

Current code caps hero level H at 30 and item level I at 60. Required level uses integer `min(30, (I+1)/2)`. Highest cleared rift R is independent. The existing town vendor generates common equipment from R and does not guarantee useful improvements.

```text
I_cap = min(60, 2 * H, max(1, R) + 2)
Generate legal existing items in 1..I_cap, starting near the reference item level.
```

H12/R16 gives I18; H30/R20 gives I22. Do not raise the cap to beat exceptionally strong owned gear. Empty slots use the class's actual unarmed/starter behavior, not percentage division by zero.

## Recommendation policy

Separate purchase-value baseline B from progression anchor A.

**B** includes currently equipped gear, the active hero's bag, and the shared warehouse, filtered by class, slot and current equip eligibility. Gear worn by another hero is not immediately available. Prefer comparisons within the same weapon family because mastery and rune-board contributions can differ. Maintain a damage/survival frontier instead of a single score; gear with different roles can remain incomparable. Do not recommend an offer that is dominated by an already-owned item in the same role. Say when the warehouse already contains a better option.

**A** is a persisted high-water baseline based on actually earned non-shop gear and completed progression, keyed by hero, progression band, slot, weapon family and evaluator version. Selling, unequipping or transferring items cannot lower it. A purchased item, and its upgraded, rerolled or bought-back descendants, cannot raise it. Persist provenance across warehouse and hero transfers. Import origin-unknown legacy gear into a one-time baseline, then track new transactions explicitly. Only real progression and newly earned rift/chest/boss/sweep drops can advance A. Crafted items remain eligible for B but do not advance A, preventing shop materials from being dismantled and crafted into a new anchor. Refreshing a stock changes selection, not its absolute power allowance.

Evaluate a copy of the complete hero/build/edict/runes with only the candidate slot changed. Exclude temporary potions and buffs. Reuse `HeroStats` and combat logic. `TrainingEquipmentBaseline` currently accepts owned gear only: do not weaken that check to preview an unowned shop item; provide a dedicated immutable evaluation entry point.

Evaluate sustained useful damage D against normal packs, elites and bosses, including attack speed, resource starvation and skill cadence. Evaluate survival E against fixed physical and elemental patterns. Detect set breakage, essential legendary effects, mastery/rune changes and resource-cycle losses. Treat movement, pickup and gold utility separately. Use a cheap sheet filter followed by deterministic multi-condition combat evaluation of shortlisted candidates; retain seeds, conditions, repetitions and evaluator version. Check median and adverse results, not a lucky single seed.

| Rule | Initial proposal |
|---|---|
| Offensive recommendation | D +2–5%, E loss no worse than 1%. |
| Defensive recommendation | E +2–5%, D loss no worse than 1%. |
| After one enhancement | Principal benefit no greater than +10% versus the same original reference. |
| Combined recommended purchases | Evaluate every legal combination: D and E individually at most +8% at +0, +12% when all recommended purchases are +1. |
| Long-term enhancement | Evaluate +5 and currently unlocked masterworking against the existing drop/upgrade ceiling for this progression band. |
| Progression skipping | Compare next-tier and following-tier reference encounters using the existing clear condition. Reduce allowances if the shop unlocks several tiers by itself. |

Never average a large damage increase against a survival loss to pass the gate. Do not automatically recommend set/rune-breaking replacements. Uncertain evaluation gets a comparison-required state. If a player can immediately afford more than one enhancement, evaluate that attainable state against the tier ceiling too. Existing upgrade costs and progression remain unchanged. Final tolerances require real drop, encounter and economic calibration; the proposed percentages alone do not prove safety.

Generate at most 128 candidates per slot through the existing item generator, validate them, filter +0/+1/affordable/long-term outcomes, remove dominated and build-breaking recommendations, apply individual and combination caps, then persist the entire chosen offers and explanations. Start with common/magic/rare +0 non-awakened items. Exclude legendary, set and greater-affix stock in version one. Do not edit rolled values outside legal ranges to force a target. Candidate and simulation budgets need profiling.

Some early levels have an 8% base-stat jump from a single item level. If no legal 2–5% candidate exists, offer a basic replacement or another role without a recommendation. The same exception applies to players already near the loot ceiling. A useful upgrade is a conditional selection goal, not a universal promise.

## Stock, pricing and repeat-purchase controls

Start with six offers: two weapons, three armor pieces, one accessory, with at most two recommendations targeting gaps. Remaining items are role alternatives or basic replacements. Refresh every 60 minutes or on an actual hero-level/highest-clear change. Do not reroll on opening, reloading, swapping equipment or buying. All town vendors for a hero share a stock identity. A sold offer stays sold within its generation.

Retain A and its absolute cap across time refreshes. Include earlier purchases in combination checks. Current saves are local, so device-clock tampering cannot be prevented with server-grade guarantees. Persist last-seen time/generation and protect against backward-time/restart resets; independent power ceilings limit the effect of forward-time manipulation.

Use the current `TownTrade.EquipmentPrice` as a starting point for basic common replacements, always validating purchase price exceeds resale. Propose `8 * Item.Price` for magic/rare offers. Do not price-discriminate by wallet balance. Buyback uses original sale price. Target purchase plus planned enhancement at roughly two to four ordinary successful runs' net income and calibrate from actual economy measurements. The mockup's prices are not measured results.

## Concrete mockup example

The fixture is a level-12 warrior with highest clear 16; it is not the user's save. Names, slot mapping and artwork come from the real catalogue. Basic weapon power uses the existing `ItemCatalog.MainValue` formula.

| Value | Equipped B02 | Offered B02 | Offered B02 at +1 |
|---|---:|---:|---:|
| Item level | 15 | 16 | 16 |
| Weapon base power | 42.40 | 44.00 | 46.20 |
| Base-power gain | — | +3.77% | +8.96% |
| Cost | Resale 1,200 | Buy 10,240 | Extra 3,200 gold + 20 materials |

Affix values are illustrative values within legal ranges. These percentages describe weapon base power, not final DPS or win rate. Production recommendation labels require the full build evaluation described above.

## Ownership and implementation sequence

1. Review this proposal and portrait/landscape mockups.
2. Build immutable candidate evaluation by reusing current stats and simulation. Never inject unowned offers into the actual owned inventory.
3. Implement proposed policy, stock and `GameStore` transaction services. `EquipmentShopPolicy`, `EquipmentShopStock` and `GameStore.EquipmentShop` are proposed names, not existing implementations.
4. Connect `ShowTownMerchant` to a window reusing storage styling, real item art and Korean/English localization. The UI must not mutate currencies directly.
5. Validate saved-state compatibility, receipts, protections, progression, economy and real inputs.

Persist generation, exact offers, sold states, buyback instances, progression anchors, provenance and policy version. Inspect the existing meaning of `Item.origin` before reusing it; a dedicated backward-compatible field may be necessary.

## Acceptance cases and evidence boundaries

Verify unequipping weak gear, stronger warehouse items, heavily enhanced references, set/rune/family changes, 100 refresh/restart/transfer cycles, insufficient funds, full bags, duplicate clicks, expiry, exact buyback restoration, H1/H12/H30 and R0/R16/R60+, instantly affordable upgrades, and combinations with earlier purchases. No non-shop progress must mean no rising anchor/cap. Log per-condition combat growth, purchase rate, useful lifetime and price relative to net earnings. Required game checks include focused Edit Mode, macOS runtime and physical mobile input; none is claimed by the design prototype.

`EquipmentShop/verification.json` records browser mockup verification only. Source owners are `GameCatalog.cs`, `Economy.cs`, `Itemization.cs`, `ItemQuality.cs`, `TownTrade.cs`, `TrainingEquipment.cs`, `StorageWindow.Style.cs`, `GameUI.Items.cs` and `GameUI.TownServices.cs` under the existing HELLSCRIPT runtime folders.

## Review screens

- [Korean landscape](EquipmentShop/landscape-ko.jpg), [English landscape](EquipmentShop/landscape-en.jpg).
- [Portrait stock](EquipmentShop/portrait-list-ko.jpg), [portrait details and purchase](EquipmentShop/portrait-detail-ko.jpg).
- [320px English](EquipmentShop/portrait-320-en.jpg), [short landscape English](EquipmentShop/landscape-phone-en.jpg).
- [Buyback](EquipmentShop/buyback-ko.jpg), [stronger warehouse comparison](EquipmentShop/warehouse-comparison-ko.jpg).
- [Interactive mockup source](EquipmentShop/shop-mockup.html), [existing art preview](EquipmentShop/equipment-preview.png), [verification record](EquipmentShop/verification.json).

Captures show the visible viewport at the browser dimensions listed in the verification record. Content below the viewport remains accessible by scrolling the interactive preview. Narrow review screens can scroll vertically; this is not evidence of fixed Unity safe-area scrolling. The timer is a fixed design value and all inventory/currency/protection data is explicitly a fixture. The prototype demonstrates locked-item protection; equipped/preset/socket protections and persistent game saves remain implementation acceptance requirements.
