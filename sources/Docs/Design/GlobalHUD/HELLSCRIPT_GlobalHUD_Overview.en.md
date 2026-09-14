# Observation HUD: approved requirements and handoff

Date: 2026-09-14

Status: implemented in Unity 6000.6 with uGUI. See the [implementation record](../../Implementation/Global_HUD_Potions.en.md) for runtime evidence. Physical mobile verification remains a separate release gate.

The game is fully automatic. Keep the world visible to the bottom of the selected display area, with small floating information groups on the left and right. Skills and potions report state; they do not trigger combat actions. Only inspection controls accept input.

Landscape places skills on the lowest right row at 1.20 times the previous linear size, with potion artwork above at 0.75 times its baseline. Potion numbers retain independent sizing. The status viewport shows 4.5 slots collapsed and at most 8 expanded, and scrolls in both states. Hidden edges fade through actual alpha. XP runs almost across the entire bottom, with nine internal ticks at 10–90% and a slightly longer 50% tick. Portrait retains the previously approved arrangement; landscape-only resizing, row reordering and full-width XP do not automatically apply to portrait.

## References and deliverables

- [Final landscape concept](Resources/v1/references/landscape-soft-edge.png).
- [Expanded layout reference](Resources/v1/references/landscape-expanded.png): an older layout image; apply the final alpha-fade rule during implementation.
- [Approved portrait concept](Resources/v1/references/portrait.png).
- [Resource ZIP](Resources/HELLSCRIPT-GlobalHUD-v1.zip), [resource guide](HELLSCRIPT_GlobalHUD_Resources.en.md), [manifest](Resources/v1/resource-manifest.json), [layout proposal](Resources/v1/layout-profile.json), [pixel validation](Resources/v1/resource-qa.json).

Concepts are generated mockups, not runtime screenshots. Deliver separate artwork, frames, fills and live text; never paste a complete HUD screenshot into the game. Existing active artwork and Warrior/Ranger portraits are reused and may differ from the concept examples.

The archived v1 handoff contains 68 PNGs: 4 native-alpha paintings, 63 vector-authored UI/status/passive images and one existing atlas copy. The game imports 67 PNGs and reuses its existing atlas for 18 active skills and legacy portraits. All 63 SVG sources remain available. The [v2 runtime profile](Resources/v2/layout-profile.json) fixes 1600×900 and 900×1600 references, landscape skill size 76.8, bottle height 45 and a 24-unit gap between three passives and four actives.

## Feature specifications

| Function | Specification |
|---|---|
| Class portrait | [01 Seal](HELLSCRIPT_GlobalHUD_01_Seal.en.md) |
| Health | [02 HP](HELLSCRIPT_GlobalHUD_02_HP.en.md) |
| Mana/class resource | [03 Resource](HELLSCRIPT_GlobalHUD_03_Resource.en.md) |
| Three passives, four actives and cooldowns | [04 Skills](HELLSCRIPT_GlobalHUD_04_Skills.en.md) |
| Three potions, counts and cooldowns | [05 Potions](HELLSCRIPT_GlobalHUD_05_Potions.en.md) |
| Level | [06 Level](HELLSCRIPT_GlobalHUD_06_Level.en.md) |
| XP and ticks | [07 XP](HELLSCRIPT_GlobalHUD_07_XP.en.md) |
| Effects, scrolling and descriptions | [08 Status](HELLSCRIPT_GlobalHUD_08_Status.en.md) |

The [implementation plan](HELLSCRIPT_GlobalHUD_Implementation_Plan.en.md) identifies owner files and acceptance gates. PNG dimensions, native alpha, sprite bounds, hashes and generated metadata are recorded. Original PNG bytes remain unchanged.

The production HUD and preview share `GlobalHudView` on persistent Canvas 105, above pages/maps and below inspection/settings dialogs. The camera fills the selected aspect frame. No new UI package, scene rewrite or prefab rewrite is required. Narrow layouts wrap complete groups. A minimum HUD scale of 0.4 preserves legibility in small windows even with the 50% preference; the 50–150% setting otherwise remains intact. Status timer numbers are seconds, with the unit included in details.

한국어: [전체 기획](HELLSCRIPT_GlobalHUD_Overview.md).
