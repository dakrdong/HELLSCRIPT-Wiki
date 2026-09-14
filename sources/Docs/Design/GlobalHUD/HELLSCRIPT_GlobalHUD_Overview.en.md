# Observation HUD: approved requirements and handoff

Date: 2026-09-14

Status: resources and implementation planning prepared. Unity integration has not started; await the user's next instruction.

The game is fully automatic. Keep the world visible to the bottom of the selected display area, with small floating information groups on the left and right. Skills and potions report state; they do not trigger combat actions. Only inspection controls accept input.

Landscape places skills on the lowest right row at 1.20 times the previous linear size, with potion artwork above at 0.75 times its baseline. Potion numbers retain independent sizing. The status viewport shows 4.5 slots collapsed and at most 8 expanded, and scrolls in both states. Hidden edges fade through actual alpha. XP runs almost across the entire bottom, with nine internal ticks at 10–90% and a slightly longer 50% tick. Portrait retains the previously approved arrangement; landscape-only resizing, row reordering and full-width XP do not automatically apply to portrait.

## References and deliverables

- [Final landscape concept](Resources/v1/references/landscape-soft-edge.png).
- [Expanded layout reference](Resources/v1/references/landscape-expanded.png): an older layout image; apply the final alpha-fade rule during implementation.
- [Approved portrait concept](Resources/v1/references/portrait.png).
- [Resource ZIP](Resources/HELLSCRIPT-GlobalHUD-v1.zip), [resource guide](HELLSCRIPT_GlobalHUD_Resources.en.md), [manifest](Resources/v1/resource-manifest.json), [layout proposal](Resources/v1/layout-profile.json), [pixel validation](Resources/v1/resource-qa.json).

Concepts are generated mockups, not runtime screenshots. Deliver separate artwork, frames, fills and live text; never paste a complete HUD screenshot into the game. Existing active artwork and Warrior/Ranger portraits are reused and may differ from the concept examples.

There are 68 resource PNG files: 4 newly generated native-alpha paintings, 63 original vector-authored UI/status/passive images, and one byte-identical existing atlas copy. That atlas defines 18 active skill cells and 3 legacy portrait cells. All 63 SVG sources are included. Baseline sizes were not numerically defined in the concept: 64 → 76.8 skills and 60 → 45 potions are proposed logical implementation values, while the 1.20/0.75 ratios are the approved requirements.

## Feature specifications

| Function | Specification |
|---|---|
| Class portrait | [01 Seal](HELLSCRIPT_GlobalHUD_01_Seal.en.md) |
| Health | [02 HP](HELLSCRIPT_GlobalHUD_02_HP.en.md) |
| Mana/class resource | [03 Resource](HELLSCRIPT_GlobalHUD_03_Resource.en.md) |
| Passive, four actives and cooldowns | [04 Skills](HELLSCRIPT_GlobalHUD_04_Skills.en.md) |
| Three potions, counts and cooldowns | [05 Potions](HELLSCRIPT_GlobalHUD_05_Potions.en.md) |
| Level | [06 Level](HELLSCRIPT_GlobalHUD_06_Level.en.md) |
| XP and ticks | [07 XP](HELLSCRIPT_GlobalHUD_07_XP.en.md) |
| Effects, scrolling and descriptions | [08 Status](HELLSCRIPT_GlobalHUD_08_Status.en.md) |

The [implementation plan](HELLSCRIPT_GlobalHUD_Implementation_Plan.en.md) identifies owner files, data prerequisites and acceptance gates. PNG dimensions, real alpha, sprite bounds, hashes and generated metadata fields were checked. Native alpha is preserved without background removal, chroma keying or recoloring. The rejected checkerboard potion and unused green variant are excluded. Unity import, runtime rendering, mobile readability and performance remain untested. This task changes no game code, Assets, scenes, prefabs, packages or saves.

한국어: [전체 기획](HELLSCRIPT_GlobalHUD_Overview.md).
