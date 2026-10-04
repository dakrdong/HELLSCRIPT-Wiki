# Shared equipment comparison rules

Updated: 2026-10-05 · First written: 2026-09-22 · Contract: `equipment-comparison-v1` · [한국어](Equipment_Comparison_Rules.md)

This is the display contract for **all equipment comparisons**, including inventory, storage, merchants, acquired rewards and training. New content must use `ItemComparison.Preview`, `ItemTooltip` and `EquipmentComparisonView`. Existing services retain ownership of generation, transactions and equipping.

## Evidence and scope

The user-provided Diablo IV [weapon](ItemComparisonReference/diablo-weapon-comparison.png) and [boots](ItemComparisonReference/diablo-boots-comparison.png) screenshots show equipped items on the left and candidates on the right, with candidate deltas and lost properties. This adopts the display behavior visible in those references, not every rule or numerical system in the latest Diablo IV release.

Do not introduce foreign item power, durability or class systems. HELLSCRIPT's `ItemCatalog`, `ItemQuality`, `StatCatalog`, `GemCatalog`, `EquipmentSlots` and `HeroStats` remain authoritative.

## Layout and state

| Element | Shared rule |
| --- | --- |
| Left | Current items labeled `Equipped · position`, with original values only. |
| Right | `Selected equipment`, with parenthesized differences after its values. |
| Order | Name; rarity/slot; gear score and before/after score; item/required level; hand usage; base value; implicits; affixes; unique/set description; sockets/upgrades; lost properties; resulting character attributes. Numeric gem contributions use separate Gem rows. |
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

## Shared item-card presentation

Every equipment detail uses `ItemDetailView`: a dark surface, rarity-tinted corner metalwork and a thin double border, identity followed by a prominent gear score, then level and the primary value. Engraved dividers separate base properties, additional options, special powers and sockets/progression. Affixes use small diamond markers; special powers use an inset panel. Comparisons use the shared compact presentation: small artwork beside identity, artwork omitted when a card is narrower than 160 units, and primary values/deltas in the same row. Duplicate outer frames, comparison titles and single-target selectors are omitted. Decorations never intercept input.

Use `EquipmentScore.Value` for intrinsic scores and `ItemComparison.Preview.scoreBefore` / `scoreAfter` for replacement scores. Weapons compare the whole hand configuration. Details, comparisons and automatic equipment share this calculation. Unrolled catalog ranges do not show a fabricated score. See [Gear score and automatic equipment](../Implementation/Recommended_Equipment.en.md) for the formula and scope.

- `ItemTooltip` / `ItemComparison` retain text and numeric ownership. The separate primary `label` / `value` come from the same calculation.
- Mobile/desktop, Korean/English and enlarged text share the presentation. Outer bounds remain fixed while content scrolls. Range On/Off and Ctrl preserve row geometry and scroll.
- Inventory, salvage inspection, storage, shop, acquisition rewards, forge, gem equipment, aspect imprinting, training and the equipment catalogue use the same view. Existing transaction, draft and snapshot ownership remains unchanged.
- References: the rarity/requirements structure in [Diablo II basic item information](https://classic.battle.net/diablo2exp/items/basics.shtml), the rarity border description in [Diablo III's Ancient/Primal guide](https://news.blizzard.com/en-gb/article/22989464/legendary-an-ancient-primal-guide), and the hierarchy in the user-provided Diablo IV comparison images linked above. Use original vector ornaments and HELLSCRIPT equipment art, not copied game artwork.

## Drag guidance and filters

Dragging bag equipment illuminates eligible character-slot borders in gold, with a stronger border on hover. `EquipmentSlots.PlanDrop` uses the actual ownership, level, class, pairing and bag-capacity checks. Test both ring positions independently. Two-handed items indicate both slots and can land on either while storing one main-hand instance. Clear highlights on drop, cancellation, Esc, focus loss, window closing and reflow. Guidance never equips, saves or changes slot geometry.

Current rarities are Normal, Magic, Rare, Legendary and Set. `UniqueItemDefinition` defines legendary powers, not a separate Unique rarity. Remove the disabled Unique option without renumbering saved masks: bit 4 remains reserved and Set stays at bit 5. The separate inventory-footer auto-selection settings entry is removed; the bulk-salvage entry remains.

## Gear score in single-item details (2026-10-02)

Every actual equipment detail shows `Gear score equipped → candidate` without pressing Compare. An empty position starts at zero. Higher candidate scores are green, lower scores red, and equal scores neutral. Details for the currently equipped item show its own score without comparing it to itself.

`ItemTooltip.Score` reuses `EquipmentScore.Value` and `EquipmentSlots.ComparisonTarget`. Rings and dual wield default to the same first occupied position as the shared comparison view; an explicit comparison target takes precedence. The weapon score row compares individual items at that position, while the existing weapon-loadout score row retains the actual combined loadout comparison. Unmet equip conditions do not hide individual score comparisons.

Content controllers pass `hero` to `ItemDetailView.Create`/`Append`. The shared display copies the item and comparison hero without reading account storage. Training and battle details use their supplied draft or snapshot; reward and change-history details use the original hero's current equipped state. Undefined catalogue ranges do not receive invented scores.

## Diablo IV card rebuild (2026-10-04)

The information hierarchy of the user-provided Diablo IV screens was re-ported for both landscape and portrait. Calculation, transaction and save ownership are unchanged; only the presentation in `ItemDetailView`, `ItemDetailPopup` and `EquipmentComparisonView` changed.

- **Primary value on one line**: a large value, its name and the delta share a row, e.g. `1,435 damage per second (+236)`. When space runs out the name drops to the next line and the value shrinks rather than wrapping.
- **Implicit branch lines**: implicit properties such as base attack speed or fixed resistance hang under the primary value with vector `├`/`└` branches.
- **Greater-affix emphasis**: greater affixes use bright gold text and a larger diamond marker. The existing `◆` glyph is kept.
- **Socket row**: sockets show an empty or gem-filled ring mark with the socket text.
- **Requirement footer**: on wide cards item level / required level, hands, material and equipped/locked state are right-aligned at the bottom. Dense cards (< 300 wide) keep them under the title.
- **Two-column card (landscape)**: at 520+ width with affixes, powers, lost properties or after-replacement stats, the left column holds identity, primary value, sockets and requirements and the right column holds affixes, powers and comparison sections so the card fits one screen. Portrait and narrow cards stay single column.
- **Scroll-down cue**: while the card is taller than the window and not at the end, `Scroll down` appears at the bottom. It never intercepts input.
- **Landscape detail window**: inventory detail widens to at most 640 in landscape; footer actions use up to four per row at 480+ so the card keeps its height.
- **Portrait comparison (< 460 wide)**: instead of squeezing two cards into 200 units, `Selected gear / Equipped · position` tabs show one full-width card at a time, defaulting to the selected gear. Ring baseline tabs stay on top and tab switching never changes saved data. At 460+ the existing side-by-side comparison (equipped left, selected right) is kept.
- **Landscape default comparison**: in landscape, tapping an unequipped inventory item whose slot has equipped gear opens the detail window directly as a side-by-side comparison (`Equipped` left, `Selected gear` right). The window is full width and keeps Equip, Lock, Compare equipment and Close (`Compare equipment` opens the dedicated window with per-ring equip buttons). Equipped items, empty slots and portrait keep the single detail.

## Ornament art and motion (2026-10-05)

Following the user's request to make the UI design as polished and juicy as possible, the card's ornament and motion were raised one more step. Only presentation changed; calculation, transaction and save ownership are untouched. `ItemCardArt` loads the Codex images documented in [Item detail card ornament art](../Art/ItemCard/Item_Card_Art.en.md).

- **Rarity frame**: wide cards (300+) get four corner ornaments (one image mirrored) and a rarity-coloured top edge. Dense cards keep the vector corners.
- **Score plaque**: the gear score (`equipment-score`) sits on a plaque that stretches to the text. A green ▲ or red ▼ beside it marks a gain or loss. The text node and name are unchanged, so existing checks read the same values.
- **Major dividers**: ornamental dividers under the header and before `Additional options` / `Special power`; thin vector rules elsewhere. Cards shorter than 420 (such as landscape comparisons) use vector rules and a shorter plaque (52) to save height; below 300 the halo and plaque are dropped and the score is plain text.
- **Rarity halo**: a rarity-coloured halo sits behind the equipment art and the art frame border takes the rarity colour. Legendary and set gear glow brighter.
- **Greater-affix star**: the diamond before a greater affix becomes a four-point star with a rising flame, with bright gold text.
- **Socket ring**: an empty socket is a grey ring; a socketed gem's art sits inside a gold ring.
- **Comparison emphasis**: option rows that gain or lose value get a soft green/red wash and the primary delta gets a triangle arrow, so colour is never the only cue.
- **Special-power frame**: the inset panel border uses the rarity colour.
- **Scroll cue**: a down arrow above `Scroll down`, over an opaque strip so lines behind never show through the label.
- **Entrance**: the card fades in over 0.2 s while settling from 96.5% to 100%, and legendary/set headers catch one tilted streak of light. The component removes itself when done, so there is no per-frame cost afterwards. Native smokes disable it by default for stable captures; `-hellscriptUiMotion` captures slowed frames.
- **Window title flourish**: the item detail window's title rule carries a centred ornamental divider.
