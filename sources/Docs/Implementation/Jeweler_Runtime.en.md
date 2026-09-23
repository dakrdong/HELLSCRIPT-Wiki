# Jeweler NPC content

Updated: 2026-09-23
Korean: [보석상 NPC 콘텐츠](Jeweler_Runtime.md)

Approach the town Gem Merchant and choose **Open jeweler**. The window provides gem conversion, elixir crafting, basic gem purchases and the existing socket-management route. Existing gem-content unlock rules apply, including the first acquired gem. Closing returns control at the same NPC.

## Transactions and ownership

- Gems use the existing account-shared storage: 999 per stack and the existing slot capacity.
- Upgrade **5 matching gems into 1 of the next tier**; downgrade **1 into 5 of the previous tier**, without gold. Existing storage and return-portal fusion also use 5:1.
- `1`, `5`, and `All` select operation counts. Five upgrades consume 25 gems and produce five. All computes the maximum allowed by input quantity and actual output capacity, including slots freed by consumed stacks. It converts exactly one tier and retains remainders.
- Invalid boundary tiers, insufficient inputs and full output storage cannot spend materials. Capacity is evaluated at freed-stack boundaries because a larger batch can fit when a smaller batch cannot.
- One gem crafts one elixir of the same color and tier. Elixirs belong to the current character's existing potion inventory, capped at 9,999 per ID. One/five/all crafting respects that limit.
- Crafted elixirs cannot be auto-purchased. Existing inventory slots and fallback policies handle their use; fallback grades remain within the same color.
- GameStore transactions commit costs and output together. Failed disk writes, duplicate requests, changed request payloads, in-run crafting and incorrect hero ownership are covered. Live NPC distance is checked when opening and when executing.
- Skull gems remain purchasable, storable and socketable; they do not produce elixirs.

## Balance

Tier order: **Chipped → Clouded → Clear → Tempered → Perfect → Crowned**. All elixirs last **30 seconds** and have a **30-second** base shared utility cooldown. Existing rune cooldown reduction applies.

| Gem | Effect 1 | Effect 2 | Effect 3 |
| --- | --- | --- | --- |
| Topaz | Magic chance +5/10/15/22/30/40 pp | Move speed +5/7/10/13/17/22% | Pickup radius +10/15/25/35/50/70% |
| Amethyst | Strength +10/20/35/55/80/120 | Maximum life +5/8/12/17/23/30% | Life steal +0.5/1/1.5/2/3/4 pp |
| Sapphire | Magic power +10/20/35/55/80/120 | Maximum mana +5/8/12/17/23/30% | Mana steal +0.5/1/1.5/2/3/4 pp |
| Emerald | Dexterity +10/20/35/55/80/120 | Dodge +2/3/5/7/10/14 pp | Attack speed +5/8/12/17/23/30% |
| Ruby | Armor +10/16/24/34/46/60% | Regenerate 0.5/0.8/1.2/1.8/2.5/3.5% maximum HP/s | Perfect block +2/3/5/7/10/14 pp |
| Diamond | All resistance +10/18/28/40/55/75 | CC duration -10/15/22/30/40/50% | Shield for 5/8/12/17/23/30% maximum HP |

GemElixirs owns these values. Presentation, combat and the wiki database read that table. The potion database now includes the original eight potions plus 36 gem elixirs.

## Combat semantics

Only one utility effect set is active. Automatic use waits until it expires. Inspect an equipped gem elixir in the HUD and choose **Use now** to replace the current effect after the shared cooldown ends.

Topaz shifts the indicated percentage points from common to magic rarity, bounded by the common pool. Rare/legendary probabilities, drop quantity and random-stream consumption stay unchanged. Attributes use existing derivation: magic power adds intelligence, and mana bonuses apply to each class's combat resource. Existing movement, attack-speed, dodge, pickup and CC-reduction caps remain.

Steal returns a fraction of actual enemy HP loss, capped at current maximum life/resource. Expiry or replacement clamps current life/resource to the restored maximum. Ruby perfect block nullifies direct damage, separately from ordinary block, and does not negate periodic damage. Diamond reduces hostile slow duration and instantaneous pull distance by the same percentage.

Diamond creates one shield at use, based on maximum HP without an additional barrier-generation multiplier and within the existing aggregate shield cap. Depletion, 30-second expiry or another utility potion removes it. Reuse replaces the remaining amount instead of stacking or retaining the larger old shield. Resume preserves remaining duration and absorption rather than regenerating the shield. Timers pause with combat/portals and continue during loot collection.

Active elixir life/resource bounds also survive live paused rune-layout commits and rune changes to a suspended run.

## Presentation and art

JewelerWindow starts from the new-content generator and uses ContentWindowView with EquipmentViewSource.Owned. UiTheme, UiFonts, safe area, fixed actions, ContentWindowHost and StoreViewBinding are shared. Landscape uses separate gem and potion scroll panels; portrait uses tabs. Selection and batch size survive orientation, language and reading-scale changes.

The gem matrix counts gem types/tiers rather than equipment positions, so its cells are a specialized adapter. Equipment socket details retain the existing common equipment path. PotionArt shares the same bottle assets across jeweler, inventory and HUD.

All 36 gem images and 36 bottle images were copied byte-for-byte from the previous design tasks. Existing source assets and GUIDs are untouched. Prototypes/Jeweler/runtime-art-manifest.json records hashes, alpha and source metadata. **Generation-model provenance remains unverified and the art remains development review material.** Low-alpha traces were preserved; no final art approval or verified generation-model claim is implied.

## Validation

- Unity 6000.6.0f1: **556 related Edit Mode tests passed, 0 failed or skipped**. [Original results](JewelerEvidence/editmode.xml).
- The macOS development player verifies NPC approach/range gates, one/five/all conversions in both directions, crafting, inventory assignment and combat use. Synthetic uGUI pointer events require a matching real UI raycast and observable state changes.
- **20 combinations** cover 440×956, 956×440, 1440×810, 1440×900 and 1680×720, with Korean/English and 100%/150% reading scale. Checks cover safe-area bounds, fixed actions, text height, tier-name wrapping and loaded art.
- A fresh process verifies saved costs, assignment, consumed stock, remaining effects and shield state. Advancing the native combat simulation verifies 30-second expiry.
- [Validation results and source hashes](JewelerEvidence/validation.json) record build and acceptance scope. Physical mobile devices were not tested. URP stripped postprocessing-shader warnings in the runtime log are separate from jeweler functional acceptance.

![Portrait gem storage](JewelerEvidence/gems-portrait.png)
![Enlarged English elixir list](JewelerEvidence/potions-portrait-en-150.png)
![Landscape jeweler](JewelerEvidence/jeweler-landscape.png)
![Diamond shield in combat](JewelerEvidence/active-shield.png)
