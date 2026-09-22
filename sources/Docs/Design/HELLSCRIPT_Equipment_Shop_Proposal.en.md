# HELLSCRIPT Weapon and Equipment Shop Proposal

Created: 2026-09-20 · Updated: 2026-09-21 · Revision 8: separate automatic selection from settings and manual mode · Status: design review · [한국어](HELLSCRIPT_Equipment_Shop_Proposal.md)

The shop bridges small gaps between earned drops: buy a modest improvement over currently equipped gear and return to rifts. Repeated purchases must not create an independent progression ladder. The HTML mockup uses an example account. The approved screen's connection to the actual equipment merchant, inventory, saves and currency is documented in [Equipment merchant integration](../Implementation/Equipment_Shop.en.md), including the implemented sheet metrics and remaining combat-simulation evaluation. Numerical thresholds and prices remain initial tuning values.

Auto-select settings contain **rarity filters, equipment filters and optional detailed rules for individual types**. Rarity and type alone can select unwanted gear. Enable detailed exclusions only for equipment worth inspecting. Save the settings, then explicitly run Auto-select to replace the selection; selling remains separate.

Revision 8 makes Auto-select only check items that match saved conditions. Auto-select settings opens the editor, and Multi-select controls manual checking. Missing settings produce an inline notice without opening a dialog or changing modes.

The comparison design remains: **Selecting an item opens selected-item and equipped-item popups. Each stat and affix ends with its parenthetical difference; a lower section lists properties lost on equip. Keep the comparison area empty before selection.** Buy hides inventory. Sell uses the existing eight-column inventory with multi-selection and bulk sale. Buyback shows one sold item per row; selecting a row displays the complete item as sold.

**Enhancement has not been designed. Remove +1 previews, costs and post-enhancement assumptions. Compare only with currently equipped gear; remove best-owned and warehouse comparison.** Upgrade information displays existing item records only. The fixtures have no upgrade records, so display “No record” without inventing levels, costs or probabilities. Existing enhancement-related source code does not establish an approved design.

## Reference and visual identity

The [D4 weapon vendor screenshot](https://guides4gamers.com/diablo-4/pois/weapons/) informed the offer list, comparison, gold transactions and buyback. The user replaced simultaneous shop/inventory display with transaction-specific tabs. Its image path is from March 2023, not proof of current-season parity. The [weapon](https://diablo4.wiki.fextralife.com/Weapons+Vendor) and [armor](https://diablo4.wiki.fextralife.com/Armor+Vendor) community references, updated May 2, 2026, describe finite stock and a 60-minute refresh. Blizzard's internal generation formula is unknown.

The two D4 comparison screenshots attached by the user directly inform this revision: paired popups, parenthetical option deltas and a lower lost-properties section. Preserve HELLSCRIPT surfaces, equipment art and borders. Eight offers, recommendation policy and 12 persistent buyback entries are HELLSCRIPT proposals. Increase the fixture from six to eight offers so the portrait list can show 6.5 rows with remaining stock below. The existing unidentified-item shop retains its separate role. Do not establish enhancement, dismantling or reroll design in this proposal.

The primary visual authority is the [user's actual storage/inventory screenshot](EquipmentShop/inventory-reference.png). The current project and isolated worktree have identical `StorageWindow.cs`, `StorageWindow.Style.cs` and `EquipmentAtlas.png` files. Preserve:

- Eight columns of square inventory slots, 50 slots, vacant-slot numbers, bottom-right item levels and lock markings.
- The existing 6×4 equipment atlas with alpha. The thumbnail is a derivative; the original asset is unchanged.
- Rarity edges: common `#5b594c`, magic `#586b78`, rare `#978554`, legendary `#a47643`, set `#607557`.
- Charcoal/olive surfaces, thin brass rules, serif headings, text `#d9d1be`, headings `#c0a777` and the existing dark-red purchase button.
- Rarity filters, position/recent/rarity/level sorting, presets and all/weapons/armor/jewelry/materials/runes/gems categories.
- The exact 24-unit geometry from `StorageGlyph.cs` for storage, bag, transfer, coin and gem symbols.

The shop uses the same surfaces, slot art and controls. Show only the active tab's list and its detail area. Buy and Buyback use one item per row; Sell retains the eight-column square inventory. Multi-selection adds an overlay without replacing slot art. Narrow the list to give details more room in landscape, or stack them in portrait. Replace the wide decorative middle area with a thin divider. Slot art, rarity edges, details and dialogs retain the storage style.

## Platforms and layout

[Apple's official specification](https://support.apple.com/ko-kr/125091) lists 2868×1320 pixels and Dynamic Island for iPhone 17 Pro Max. Review portrait at **440×956 design units**, a 3:1 reduction of 1320×2868, and landscape at 956×440. This is a design coordinate conversion, not physical-device Unity evidence.

| Target | Layout | Scrolling |
|---|---|---|
| PC 1600×900 and 1920×1080 | Active transaction list left, paired comparison popups right | Buy/Buyback use rows; Sell uses eight inventory columns. |
| iPhone 17 Pro Max 956×440 landscape | Active transaction list left, paired comparison popups right | Keep eight inventory columns and fixed transaction controls. |
| iPhone 17 Pro Max 440×956 portrait | About 6.5 Buy rows above; remaining space below for comparison | Selected and equipped cards sit side by side. List and comparison interiors scroll independently. |

Keep title, transaction tabs, category/filter controls, currencies and transaction actions fixed. Popups appear inside the reserved comparison area without covering the list. Initial entry and tab changes do not auto-select an item. Selecting the same item again, Close or Escape clears both comparison and transaction controls. Filtering a singly selected item out also clears the comparison. Bulk selection continues to persist by item ID across filters. During multi-selection, each row's Details button opens the same complete comparison in a safe-area dialog.

Illustrative safe margins are 62 top / 34 bottom in portrait and 62 left/right / 21 bottom in landscape. These are review assumptions, not verified device safe-area constants. Production must read `UiSafeArea.Current`; actual touch, cutouts, text scaling and performance require device testing.

### Information density and comparison rules

Retain Revision 4's compact sizes: mobile offer icons 32×32, rows 40, row gaps 4, title 16, offer names and comparison body 11 design units. PC icons remain 44 and list width is capped at 560.

The portrait Buy pane is 340 units tall including its heading and categories. It shows six complete items and approximately half of the seventh. The remaining 430-unit comparison pane includes transaction controls and keeps the same boundary before and after selection. Landscape keeps the list left and comparison right. Long comparison content scrolls internally while the transaction price and action remain fixed.

- Each base stat and affix on the selected card ends with `selected value − equipped value`: for example, Attack `44 (+1.6)`, Strength `+15.9 (+1.9)` and Physical damage `+5.8% (−0.7%)`.
- Treat a new affix's equipped value as zero and show its full value as the gain. Show unchanged values as `(0)`. Percentage differences are percentage-point differences, not relative growth. Match base stats and affixes separately by property, unit and effect condition.
- Positive differences are green, negative red and zero neutral, with explicit signs. All fixture stats benefit from higher values. Future lower-is-better stats such as cooldown must use the canonical stat definition's benefit direction.
- The lower Properties lost when equipped section lists equipped affixes absent from the selected item, in red. A reduced but retained affix belongs in the negative delta above and must not also appear as lost. Show None when nothing disappears.
- Preserve name, rarity, slot, item level, applicable base stats, weapon speed, every affix, existing upgrade records, required level, sale value and transaction price. Do not add inapplicable attack/defense rows. The equipped card shows its own complete information. Do not add enhancement forecasts.

Sell retains eight square columns. Portrait uses approximately 43-unit slots to show all 50 positions; landscape uses approximately 41-unit slots to show 40 positions before interior scrolling. Embedded phone previews scale the entire device uniformly so a narrower conversation surface does not reflow the device layout. Physical touch usability is not yet verified.

## Purchase, sale and buyback

Buy displays offers and selected-item comparison only, with no inventory. Selecting an offer opens the paired popups described above, preserving full information, per-option differences and lost properties. There is no comparison-target selector or enhancement action.

Purchase validates gold, free space, offer identity and generation, then atomically transfers the exact instance into the first free bag slot. Mark the offer sold; do not auto-equip or immediately refill it. Production uses receipts against duplicate transactions.

Sell alone displays the inventory: eight square columns and 50 capacity positions in default slot order. Selecting an item shows its information and Sell button. Multi-select displays selected items, total proceeds and Sell selected. Auto-select evaluates the entire inventory using saved rarity, equipment-type and enabled detailed rules, independently of the inventory view's category and rarity filters, and replaces the existing selection. Filters and sorting preserve selection by item ID; disclose the hidden selection count. Clear and leaving multi-select remove all selections. Each selected row can open its complete information.

Preserve equipped, locked, preset-reference and socketed-item protections; the fixture demonstrates locked/preset examples and excludes equipped gear from the bag. Bulk sale must revalidate every ID, ownership, protection and price before applying the whole transaction. A failure must not partially sell the selection.

Auto-select preserves whether manual Multi-select is on or off. Automatic checks, their count, total, Clear and Sell selected remain available in normal mode. Clicking an inventory slot then opens its information dialog without altering the checks. Entering manual Multi-select retains those checks so the user can adjust them; finishing manual selection clears them. If rarities or equipment types are unconfigured, preserve existing selections and show a notice below the toolbar. If configured rules match nothing, clear outdated checks and show a no-match notice. Both notices remain visible on mobile.

Buyback uses **one sold item per row**, with art, name, rarity, level and original price. Do not show unsold gear or empty inventory slots. Selecting a row shows all item information listed above, including the original sale price and buyback price. Never truncate long information; scroll within details. Production should reuse the canonical item description for sockets, gems and additional model fields. Preserve exact original IDs, affixes, sockets, upgrade records, provenance and prices without rerolling. Displaying upgrade records does not define a future enhancement system.

Retain the latest 12 items. Before a single or bulk sale exceeds the limit, show how many entries will be lost and require confirmation; canceling changes neither gold nor items. Process selected items in inventory slot order and retain the latest 12. Normal-restart persistence remains a production requirement. Insufficient funds, full inventory, protected gear and sold stock disable the corresponding action.

The preset dialog displays the example hero's current equipped set; it does not edit real presets. History shows transactions made in the mockup. Game persistence is not connected.

### Auto-select settings

Place Auto-select settings next to Auto-select. Rarities and equipment types initially start unchecked. Each section has Select all, including a mixed state when partially selected. Keep the live candidate count, Cancel and Save settings fixed at the bottom. Saving alone neither selects nor sells gear.

#### 1. Rarity filter

Individually check Common, Magic, Rare, Legendary, Unique and Set. Unchecked rarities are excluded; no checked rarity means no candidates. The Korean Rare label is now 희귀 consistently throughout the prototype.

Unique is a requested independent filter with zero sample items. The current game uses `UniqueItemDefinition` through a Legendary item's `special` reference and additional set identity. The six display categories must not be copied into save-file rarity numbers. Define the production mapping for Legendary, Unique and Set before connecting these controls to real items.

#### 2. Equipment filter

Individually check the types below. **Both rarity and equipment type must match.** Checking a type does not open or enable detailed conditions. For example, Common + Magic and all equipment types select eight sample items with all detailed rules off. This is the quick path for unwanted gear.

| Group | Individually selectable types | Initially visible condition examples |
|---|---|---|
| Weapons | One-handed sword, two-handed sword, axe, bow, crossbow, staff | Attack, critical chance/damage, attack speed bonus, Strength |
| Armor | Helmets/hoods, chest armor, gloves, pants, boots, belts | Defense and slot-appropriate life, resistance, critical chance, movement speed or damage reduction |
| Jewelry | Amulets, rings | Slot-appropriate life, resistance, critical chance, cooldown reduction and other affixes |

#### 3. Optional detailed settings

Each equipment row has a Details button. For a checked type, this opens the settings beneath the row. The first checkbox, Use detailed rules, controls whether the existing stat conditions are enabled and evaluated. When off, the fields are disabled and their values are preserved, but only rarity and equipment type affect selection.

When detailed rules are in use, highlight that type's button with the existing brass treatment and Details · On text. Merely opening the panel or checking the type does not turn on the highlight. Unchecking the type disables its button while preserving its configuration. Mobile landscape arranges detailed conditions into two columns; portrait stacks them. Validation stays above the fixed actions. The following threshold rules apply only when Use detailed rules is enabled.

Each condition has a checkbox and a numeric “or higher: exclude” threshold. Only checked conditions count. Add exclusion exposes the remaining compatible options without duplicates. Names, units and slot masks follow the 54 current `StatCatalog`/`ItemCatalog.Affixes` definitions plus base weapon attack, yielding 55 possible fields before filtering by slot. Evaluate the item's own base value plus affixes of the same property and unit. Do not use hero totals, equipped-item deltas, buffs or hypothetical enhancement values. An absent affix does not satisfy even a zero threshold.

For each type, set “Exclude when N or more conditions match.” N is an integer from zero through the number of enabled conditions. Equality satisfies a condition. For N ≥ 1, leave items unselected when at least N conditions match. **Zero is proposed to disable stat exclusions**, while protection rules remain in force; the dialog explicitly labels that meaning. Enabling the first condition initializes N to one, and the user can set it back to zero. Removing conditions clamps N to the remaining count.

For example, an axe with Attack 48.76 and Critical chance 3% matches one of the thresholds Attack ≥ 50 and Critical chance ≥ 3%. It is kept at N=1 and selected for sale at N=2. N=0 ignores those stat exclusions. Negative/empty thresholds and out-of-range or fractional counts block saving only for enabled detailed rules. Unfinished fields in disabled detailed rules do not block the coarse-filter workflow. Cancel, Close and Escape discard draft edits. Saved rules reopen for editing; disabling and re-enabling a type preserves its conditions. Repeated Auto-select replaces the selection without toggles or duplicates. Locked and preset protection is applied both when selecting and when selling; equipped gear remains outside the sale bag.

The 24 existing base items map to 13 kinds. **Pants** is an additional requested UI choice with zero sample items because the current catalogue has no pants slot/base. Its provisional option list uses the chest slot mask until the actual slot and affix rules are designed. Sword/greatsword families provide the one-/two-handed UI categories; this does not implement a new combat taxonomy.

Settings persist only in conversation-preview state, not game account storage. A fresh standalone page resets them. Production must store rarity categories, kind IDs, the per-type detailed-rule toggle, stat IDs, units, thresholds, match counts and rule versions as account settings, reuse all existing equipped/locked/preset/socketed protections, and revalidate inventory and protection immediately before sale.

## Progression and recommendation policy

Current source caps hero level H at 30 and item level I at 60. Required level is integer `min(30, (I+1)/2)`. Highest cleared rift R is independent. The current common vendor uses R without promising a modest improvement.

```text
I_cap = min(60, 2 * H, max(1, R) + 2)
Search legal existing-generator candidates in 1..I_cap near the equipped item.
```

H12/R16 permits I18; H30/R20 permits I22. Do not raise the cap to beat exceptional equipped gear. Empty slots use actual unarmed/starter behavior, avoiding division by zero.

**B is currently equipped gear.** Evaluate a candidate swapped into the same slot, including attack speed, weapon mastery and rune effects when families differ. Do not select a best-owned or warehouse item as the comparison target.

**A is a persisted progression anchor** built from earned non-shop loot and actual rift progress, keyed by hero, band, slot, weapon family and evaluator version. Selling, unequipping and transfers cannot lower it. A is not another visible comparison mode. Purchased items, buyback and provenance-linked descendants do not raise it. Real rift/chest/boss/normal-sweep drops and progress can raise it. Exclude crafting from anchor updates to avoid laundering shop-derived materials. Import legacy unknown-origin gear once; preserve provenance afterward. Restocking changes selection, not the absolute allowance.

Evaluate an immutable copy of the hero, edict, skills, runes and other equipment with one slot replaced; exclude temporary potions/buffs. Reuse `HeroStats` and combat logic without weakening `TrainingEquipmentBaseline`'s owned-item-only restriction. Unowned candidates need a dedicated evaluation entry point, not insertion into the player's real bag.

| Gate | Initial hypothesis |
|---|---|
| Offensive recommendation | Sustained useful damage D +2–5%, survival E loss at most 1%. |
| Defensive recommendation | E +2–5%, D loss at most 1%. |
| Combined purchases | Include earlier purchases and every legal combination against the same anchor; initially cap D and E separately at +8%. |
| Build preservation | Detect set, essential-effect, mastery, rune and resource-cycle losses. |
| Tier skipping | Compare next-tier and following-tier encounters using existing rift-clear conditions; reduce allowance if shop purchases unlock several tiers alone. |

Use cheap stat filtering before deterministic multi-condition combat evaluation of shortlisted items. Include normal packs, elites, bosses, physical/elemental patterns, resource starvation, skill cadence and speed. Check median and adverse outcomes. Keep movement/pickup/gold utility separate. Do not average large damage gains against survival loss to pass a gate.

Start with a budget of 128 candidates per slot through the existing generator. Propose common/magic/rare stock and reserve legendary/set/greater-affix rewards for farming. Never alter rolled values beyond legal ranges to force a percentage. If no legal small upgrade exists, show a basic replacement or alternative without a recommendation. A player near the ceiling is not guaranteed an improvement.

When enhancement is eventually designed, the evaluator may need to incorporate further progression after purchase. **No enhancement levels, costs, probabilities or power caps are defined here.** Purchase-time gates do not establish the safety of a future enhancement system.

## Stock and prices

Propose eight offers: two weapons, four armor pieces and two accessories, with at most two recommendations. About 6.5 rows are visible in portrait; the rest scroll within the list. Refresh every 60 minutes or upon an actual hero-level/highest-clear change. Opening, restarting, swapping gear and purchases do not reroll. A hero's vendors share stock identity; sold slots remain sold for that generation. Include earlier purchases in combined-power checks.

Local saves cannot guarantee server-grade clock protection. Persist last observed time/generation and independently enforce progression caps. Use existing `TownTrade.EquipmentPrice` as a common-item starting point and `8 * Item.Price` as the magic/rare hypothesis. Validate buy price exceeds resale; do not price by wallet size. Compare purchase price alone with two to four ordinary runs' net income. Do not add an assumed enhancement budget. Economy calibration remains outstanding.

## Example values

The fixture is a level-12 warrior at rift 16 with 14 inventory items arranged to resemble the reference. Levels and rarities are example data; names, slots and art use the existing catalogue.

| Value | Equipped B02 | Offered B02 |
|---|---:|---:|
| Item level | 15 | 16 |
| Weapon base power | 42.40 | 44.00 |
| Base-power gain | — | +3.77% |
| Purchase price | — | 10,240 gold |

This uses `ItemCatalog.MainValue` and describes base weapon power, not final DPS or success probability. The popup shows the absolute difference `(+1.6)` instead of this percentage. Equipped sword fixtures have Strength 14, Physical damage 6.5% and Movement speed 3%. The offered sword consequently shows Strength +1.9, Physical damage −0.7%, Critical chance +3%, and Movement speed 3% as lost. These affix values illustrate comparison behavior; recommendation badges still require real build evaluation in production.

## Ownership, acceptance and evidence

Review this proposal and all three platform layouts first. Then build immutable candidate evaluation, proposed `EquipmentShopPolicy`/`EquipmentShopStock`/`GameStore.EquipmentShop` services, and a `ShowTownMerchant` entry point reusing inventory styling, localization and safe areas. These service names are proposals, not existing implementations. Persist generation, offers, sold state, buyback instances, anchors, provenance and policy version. Inspect `Item.origin` before extending it.

Required checks include weak unequipped references, exceptional gear, empty slots, set/rune/family changes, 100 refresh/restart/hero-transfer cycles, earlier-purchase combinations, insufficient money/space, duplicate input, expiry, protection and exact buyback restoration. Also verify selection across filters/sorting/tabs, clearing, protection exclusions, all-or-nothing bulk sale, 12-entry overflow warnings and complete item information preservation. Cover H1/H12/H30 and R0/R16/R60+. Measure growth, purchase rate, useful lifetime and price relative to net earnings. Validate save compatibility, receipts, combat/economy, macOS runtime and physical mobile input before shipping.

[Revision 8 verification](EquipmentShop/verification.json) covers automatic selection, unchanged manual mode and missing-settings notices. [Focused policy, action and state checks](EquipmentShop/auto-selection-policy-checks.json) cover thresholds and selection behavior. Preserve the [30 Revision 7 filter checks](EquipmentShop/verification-v7.json), [44 Revision 6 interaction checks](EquipmentShop/verification-v6.json), [36 Revision 5 comparison checks](EquipmentShop/verification-v5.json), [Revision 4 density checks](EquipmentShop/verification-v4.json) and [43 Revision 3 functional checks](EquipmentShop/verification-v3.json) separately; do not claim historical checks were all rerun. These reports do not establish Unity integration, real saves, full regression, touch or device performance. Source owners include `StorageWindow.cs`, `StorageWindow.Style.cs`, `StorageGlyph.cs`, `EquipmentAtlas.png`, `GameCatalog.cs`, `Economy.cs`, `Itemization.cs`, `ItemQuality.cs`, `TownTrade.cs` and `TrainingEquipment.cs`. Automatic-selection names, units, slot masks and weapon kinds also reference `Attributes.cs`, `Itemization.cs` and `RuneMasteryCatalog.cs`. Enhancement code in those files is not adopted as design authority.

## Review screens

- Revision 8 automatic-selection results: [PC](EquipmentShop/auto-select-v8-pc-result-ko.jpg), [iPhone portrait](EquipmentShop/auto-select-v8-portrait-result-ko.jpg), [iPhone landscape in English](EquipmentShop/auto-select-v8-landscape-result-en.jpg).

The settings hierarchy retains the Revision 7 layout below.

- [PC filters](EquipmentShop/auto-select-v7-pc-filters-ko.jpg), [PC detailed settings](EquipmentShop/auto-select-v7-pc-details-ko.jpg), [PC English](EquipmentShop/auto-select-v7-pc-filters-en.jpg).
- [Portrait filters](EquipmentShop/auto-select-v7-portrait-filters-ko.jpg), [portrait detailed settings](EquipmentShop/auto-select-v7-portrait-details-ko.jpg), [portrait English](EquipmentShop/auto-select-v7-portrait-filters-en.jpg).
- [Landscape filters](EquipmentShop/auto-select-v7-landscape-filters-ko.jpg), [landscape detailed settings](EquipmentShop/auto-select-v7-landscape-details-ko.jpg), [landscape English](EquipmentShop/auto-select-v7-landscape-filters-en.jpg).

The following Revision 5 images document the retained comparison layout. Revision 7 documents the settings hierarchy; Revision 8 documents automatic-selection behavior.

- [PC 1600×900 Korean](EquipmentShop/comparison-v5-pc-ko.jpg), [PC 1920×1080 English](EquipmentShop/comparison-v5-pc-en.jpg).
- [iPhone landscape Korean](EquipmentShop/comparison-v5-landscape-ko.jpg), [landscape English](EquipmentShop/comparison-v5-landscape-en.jpg).
- [Empty portrait comparison](EquipmentShop/comparison-v5-portrait-empty-ko.jpg), [portrait comparison](EquipmentShop/comparison-v5-portrait-ko.jpg), [portrait English](EquipmentShop/comparison-v5-portrait-en.jpg).
- [Landscape Sell](EquipmentShop/comparison-v5-landscape-sell-ko.jpg), [portrait Sell](EquipmentShop/comparison-v5-portrait-sell-ko.jpg), [portrait multi-selection](EquipmentShop/comparison-v5-portrait-bulk-ko.jpg).
- [Landscape buyback](EquipmentShop/comparison-v5-landscape-buyback-ko.jpg), [portrait buyback](EquipmentShop/comparison-v5-portrait-buyback-ko.jpg).
- [Interactive shop](EquipmentShop/shop-mockup.html), [editable fragment](EquipmentShop/shop-fragment.html), [existing-art preview](EquipmentShop/equipment-preview.png).

Captures show the full fixed viewport, with only panel interiors scrolling. The timer is a fixed example. Earlier revision images remain solely as document-history evidence and are not the current design for approval.
