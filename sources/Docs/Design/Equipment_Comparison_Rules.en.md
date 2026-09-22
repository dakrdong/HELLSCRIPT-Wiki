# Shared equipment comparison rules

Date: 2026-09-22 · Contract: `equipment-comparison-v1` · [한국어](Equipment_Comparison_Rules.md)

This is the display contract for **all equipment comparisons**, including inventory, storage, merchants, acquired rewards and training. New content must use `ItemComparison.Preview`, `ItemTooltip` and `EquipmentComparisonView`. Existing services retain ownership of generation, transactions and equipping.

## Evidence and scope

The user-provided Diablo IV [weapon](ItemComparisonReference/diablo-weapon-comparison.png) and [boots](ItemComparisonReference/diablo-boots-comparison.png) screenshots show equipped items on the left and candidates on the right, with candidate deltas and lost properties. This adopts the display behavior visible in those references, not every rule or numerical system in the latest Diablo IV release.

Do not introduce foreign item power, durability or class systems. HELLSCRIPT's `ItemCatalog`, `ItemQuality`, `StatCatalog`, `GemCatalog`, `EquipmentSlots` and `HeroStats` remain authoritative.

## Layout and state

| Element | Shared rule |
| --- | --- |
| Left | Current items labeled `Equipped · position`, with original values only. |
| Right | `Selected equipment`, with parenthesized differences after its values. |
| Order | Name; rarity/slot; item/required level; hand usage; base value; implicits; affixes; unique/set description; sockets/upgrades; lost properties; resulting character attributes. Numeric gem contributions use separate Gem rows. |
| Same item | An equipped item shows details without a self-comparison or comparison button. |
| Empty position | Show an empty equipped placeholder. Missing properties have a baseline of zero. |
| Space | Window/card bounds stay fixed. Long text wraps and scrolls inside each card. Changing the comparison position never changes window height. |
| Actions | Equip, purchase and confirmation actions stay outside the scrolling text. Reading or switching the baseline does not equip or transact. |

## Values and ranges

The **range icon beside the class/level label in the inventory footer** toggles range visibility. Gold highlighting indicates On. Its adjacent `?` button opens a short upward help bubble: `Shows the minimum and maximum option values.` Help does not change the preference and closes when clicking outside or pressing Esc. The default is Off, saved on the device. Every equipment detail and comparison in inventory, storage, shops, rewards, training and blacksmith services reads this same preference across character and content changes. Account progress and item values remain untouched.

On desktop, holding either Ctrl key temporarily shows ranges. Releasing it or losing application focus restores the saved choice. Ctrl never inverts an enabled preference or writes the settings file. Mouse hover shows **Hold Ctrl to show ranges temporarily**; touch input does not show this hint. Mobile controls and desktop input share the same display policy.

Reserve space for the expanded text from the start. Visibility changes update text without resetting card bounds, row positions, scroll or ring targets. Reuse `ItemComparison.Affix` / `ItemComparison.Properties` for ranges, `ItemRangeDisplay` for the preference and `ItemRangeText` for live text updates.

1. Difference means **candidate minus outgoing equipment**. Positive changes are green `(+value)`, negative changes red `(−value)`. Signs accompany color. Omit differences that round to zero at the display precision.
2. Percentage attributes use percentage-point differences: 18% to 30% is `(+12%)`, not a 66.7% relative increase. 24.8% to 17.1% is `(−7.7%)`.
3. Match stable identities rather than translated labels. Separate `main`, `implicit`, `affix` and `gem`; affixes use `StatId`, while gems use effect kind and stat ID. Conditional effects remain distinct. Never subtract a socket bonus from an affix's baseline.
4. New properties compare against zero. A reduced property present on both items gets an inline negative delta, not a duplicate lost-property entry.
5. `[minimum–maximum]` comes from the item's actual level, awakened range, greater affix and masterwork multiplier. Fixed maximum rolls have equal endpoints. Legacy values with unknown ranges omit the range. Future upgrades are not included.
6. Base attack speed uses the game's multiplier unit `×`. A base-item difference is distinct from total character attack power or DPS.

## Properties lost on equip

The candidate ends with a red **Properties lost on equip** list when applicable. An empty list omits its heading without resizing the window.

- Show outgoing base/implicit/affix/gem contributions absent from the candidate.
- Do not numerically equate different unique effects. An effect still supplied by retained equipment is not lost.
- Set losses use actual `HeroStats` 2/4-piece activation changes, not simply different item names.
- Gems remain in their original equipment. Comparison never extracts or transfers them.

## Rings and paired weapons

Show both occupied rings to the left of the candidate; with only one equipped ring, omit the second empty ring card. Fixed tabs choose the left or right replacement baseline. Deltas and resulting attributes follow that position without moving or resizing cards. Selecting an empty position retains the other ring.

`EquipmentSlots.Plan` determines outgoing weapons, including two-handed displacement and incompatible offhand removal. Highlight the outgoing equipment and name all items removed together. Count a two-handed instance once.

For multiple outgoing items, sum additive affixes, resistance and gem contributions. Weapon base values and base speeds follow the dual-wield averaging used by `HeroStats`; offensive offhand base values are added separately. Never subtract shield armor from weapon attack. The separate resulting-character section includes runes, sets and caps.

If equip conditions fail, show the condition in red and omit the resulting-character prediction. Warehouse, merchant and training previews equip copies only. Existing inventory read/guide progress tracking remains; comparison calculation itself never writes saves.

## Implementation and acceptance

- Calculation: `Runtime/Core/ItemComparison.cs`.
- Shared body/cards: `Runtime/Presentation/EquipmentComparisonView.cs`.
- Consumers: `InventoryWindow`, `StorageWindow`, `EquipmentShopWindow`, legacy `GameUI` details and training candidates. Acquisition reveals show a single detail card; Compare opens the shared paired view.
- The same item, equipment and language must yield the same comparison text across consumers.
- Cover unchanged/new/lost properties, units, zero deltas, quality ranges, both ring positions, two-handed changes, equip restrictions, immutable source data and fixed geometry.
- Record executed checks and their limits in [implementation evidence](../Implementation/Equipment_Comparison.en.md).
