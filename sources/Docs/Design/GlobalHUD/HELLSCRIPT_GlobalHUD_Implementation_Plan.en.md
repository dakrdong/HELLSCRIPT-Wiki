# Global HUD, potions and hunt-edict implementation contract v2

Date: 2026-09-14

## Approved changes

Keep the existing three-passive equipment rules and show all three entries, four active skills and three potion slots. The combat camera covers the entire selected aspect frame; only individual HUD graphics overlay it. Display graphics never trigger combat actions.

At 1600×900, landscape skill bounds are 76.8 and potion artwork is 45: 120% of 64 and 75% of 60. Three passives precede four active skills in the lowest right row; potions sit above. At 900×1600, portrait has separate passive, active and potion rows. Narrow views and enlarged reading sizes wrap whole groups.

Landscape status geometry is 44 icons, 8 gaps, 230 collapsed width (4.5 entries), at most 408 expanded width (8 entries), 0.18 seconds expansion. Edge fade is at most 22 and shrinks to the actual hidden distance. Artwork, text and badges use the same real alpha, while controls remain outside the mask. Test both edges and 0/4/5/8/12/40 entries. Portrait keeps four entries and an additional-count entry leading to full inspection.

Landscape XP uses 32 side insets, 16 bottom inset and a 3-unit track. Nine internal ticks represent 10–90%; the 50% tick is taller. Portrait keeps the short left track.

## Potion contract

HP restores 35% maximum health with existing healing modifiers. Resource restores 35% maximum resource. Both have 20-second cooldowns. Six utility kinds are Sprint (+20 percentage points movement bonus), Assault (+15% attack), Resistance (+20+3×hero level all-resistance rating), Iron (+30% armour), Haste (+15 percentage points attack speed), Focus (+10 percentage points critical chance). Each lasts eight seconds with a 30-second cooldown, respects existing caps, and cannot be consumed again while its effect remains.

Sanctuary restocking buys only the selected utility type, in HP/resource/utility order. Targets are 20/20/10, unit prices 5/5/15 gold, per-visit budget 500 and protected balance 200. Each hero receives initial supplies once. Repainting a screen is not a sanctuary purchase event.

The hunt edict owns use switches/thresholds, utility type/condition, automatic purchasing/targets/budget/reserve and shortage departure policy. Choose wait when HP is empty, wait until targets are stocked, or continue despite shortages. New defaults wait for HP; the saved user policy is authoritative. Running combat is not forcibly ended by depleted stock.

Preserve old HP thresholds, passives, unlocks and presets. A suspended legacy run finishes under its old potion contract, then converts in the sanctuary. Training and comparison clone quantities and gold. Import and stored edicts gain new default options while retaining existing choices.

## Implementation and evidence order

1. Freeze geometry/resources and share production components with the preview.
2. Verify layout, real alpha on artwork/text, and effect navigation.
3. Connect simulation modifiers, inventory, edicts, purchases and persistence.
4. Check orientations, small screens, 50–150% reading sizes, safe areas, Korean/English, pause/speed and restore.
5. Provide Unity Edit Mode and native macOS development-player evidence, bilingual records and public wiki synchronization. Physical mobile touch verification remains a separate release gate.

See the [bilingual implementation record](../../Implementation/Global_HUD_Potions.en.md) for results and evidence. This document preserves the requirements applied to implementation and acceptance.

한국어: [구현 기준](HELLSCRIPT_GlobalHUD_Implementation_Plan.md).
