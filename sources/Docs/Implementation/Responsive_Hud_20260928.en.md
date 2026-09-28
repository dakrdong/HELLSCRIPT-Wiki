# Window shrinking and a unified bottom HUD

Date: 2026-09-28 · [한국어](Responsive_Hud_20260928.md)

Controls and text shrink together with the window. Combat, town, responsive pages, settings and presets now use the safe-area ratio from `UiTheme.Scale` instead of a fixed pixel scale. Legacy page coordinates are twice the shared content coordinates, so these pages use half that ratio. Window and safe-area changes refresh their canvases.

## One bottom HUD composition

`GlobalHudLayout` preserves the landscape reference composition across aspect ratios. Potions, ultimate and ordinary skills, character seal, level, XP, HP, MP and status effects use one scale. Portrait does not enlarge potions independently or shorten the XP bar. Center the group along the safe area's bottom edge without stretching its spacing or relative positions into unused space.

Ultimate and ordinary skills always share one row. The three potions sit to the left of the skills, centered on the same row. Fit the entire group instead of adding rows. Remove the fixed 0.4 minimum scale. Retain the reading preference, but fitting the complete bottom group takes precedence when that preference would overflow. Apart from integer font-pixel rounding, no part scales independently.

Anchor the live combat journal at the bottom of the safe area. Tapping its body or right-hand button expands or collapses it. The entire HUD sits above the journal and moves by exactly the journal height change, preserving its scale, size and internal spacing. Leaving combat removes this bottom reservation.

## Exactly three potion HUD slots

Remove the separate potion bottle/count tray and the three empty squares that appeared beside it. Those squares belonged to the legacy passive loadout HUD. The bottom row now creates exactly three square potion slots, each containing its equipped bottle, live stock and cooldown. Depleted stock dims the bottle; only unassigned slots show an empty caption. Clicking a slot retains the existing potion inspection and its permitted actions.

The single row is three potions, one ultimate, then four normal skills. No separate potion tray or fixed passive placeholders are created. Equipped legacy passives remain inspectable in the status-effect strip with their names, descriptions and remaining timers. New skill-tree passive rules, potion equipment, automatic consumption, stock and persistence are unchanged.

`GlobalHudView` reuses the bottle artwork and square frame, and `GlobalHudSnapshot` supplies the live data. `GlobalHudLayout.potionBounds` only reserves geometry against town input and shortcuts; it does not create another visual. Potions keep the common HUD scale and move together with the expanded combat journal.

## Power saving and attendance

Landscape power saving uses equal-width columns: warehouse and equipment changes on the left, acquired equipment and attempts on the right. A centered vertical divider separates only the body, leaving the shared timer/status and growth/unlock footer clear.

Equipment changes show time and position on one text line. Both item slots retain `EquipmentSlotView` and use the shared 40-unit `UiTheme.CompactEquipmentSlot`, with a 6-unit row gap. At 100%, row height falls from 78 to 46 units. Enlarged reading sizes preserve one line and both historical item-detail actions.

Every claimable, uncollected attendance reward has green light behind its item, a persistent green border and a green dot. Eligibility is independent of selection; claimed and locked rewards have no green highlight. `AttendanceClaimGlow` is a vector UI light resource with transparent radial falloff and soft rays. It follows the existing code-native UI graphics system, using no generated raster image or image model. It needs no per-frame animation or mesh rebuild. `UiTheme.Claimable` owns its color.

## Validation

Geometry tests cover common scale and relative positions, one skill row and non-overlap across sizes, orientations and reading preferences. Native macOS checks cover actual rendered scale after periodic refresh, the power-saving divider, attendance highlights before/after claiming, existing input, scrolling and persistence. Results are collected in the [integration record](All_Work_Integration_20260928.en.md). Physical mobile and battery measurements remain separate.

### Three-slot potion validation

Unity 6000.6.0f1 passed **147/147** focused Edit Mode tests and **9/9** shared-UI checks. The native macOS development player covered 440×956 portrait, 956×440 landscape, 1600×900 / 1600×1000 / 2100×900 desktop and a 512×288 small window in Korean/English at 100%/140% reading size. **48 screen combinations**, including collapsed/expanded logs, verified exactly three potion slots, live quantities/cooldowns, uniform scaling, log clearance and whole-HUD translation. Every potion was opened and closed with real raycasts and synthetic pointer events in every size/language/reading combination, preserving stock and the original pause state. Physical mobile was not tested.

- [Edit Mode summary](PotionHudEvidence/editmode-summary.json) · [Runtime result](PotionHudEvidence/runtime.txt) · [Per-screen measurements](PotionHudEvidence/live-layout.txt)
- [Landscape](PotionHudEvidence/landscape-ko.png) · [Small landscape, expanded log](PotionHudEvidence/landscape-small-expanded.png) · [English portrait](PotionHudEvidence/portrait-en.png)
