# Aspect Runestone implementation

Updated: 2026-09-22
Korean: [위상 각인석 게임 적용](Aspect_Runestone_Implementation.md)
Design: [Aspect Runestone and all 123 progression tables](../Design/Aspect_Runestone.en.md)

## Scope

The runestone in the middle-left part of town operates on actual equipped items and the account collection. It uses the existing 123 legendary definitions. Legendary salvage activates the first copy at Lv.1 and accumulates later copies. At 2, 4, 8 and 16 copies, the player claims each additional level manually. Red dots remain while any attainable level is unclaimed.

Equipped Rare and Legendary items receive the collection's current level. Imprinting turns a Rare item Legendary while retaining its three affixes and other state. The new aspect replaces the previous power. Equipping duplicate aspects uses the highest level once. Collection upgrades do not rewrite existing equipment.

Imprinting consumes no additional resources. To prevent an imprint-and-salvage duplication loop, collection credit comes from the original power of a naturally acquired Legendary. An overwritten natural Legendary yields its original aspect; an imprinted Rare yields no aspect. Item details and salvage confirmation communicate the result.

## Ownership and persistence

| Responsibility | Owner |
| --- | --- |
| Canonical catalog, cumulative copies, manual levels, imprint transactions | `Runtime/Core/AspectStone.cs` |
| Progression values, descriptions, captured level lookup | `Runtime/Core/AspectGrowth.cs` |
| Salvage collection, commit, save failure recovery | `Economy.Dismantle`, `InventorySalvagePlan`, `GameStore.Transact` |
| Effective equipped power and highest duplicate level | `HeroStats`, `LegendaryPower.AtLevel`, existing `CombatSimulation` effect owners |
| Shared UI composition | `AspectStoneWindow`, `GameUI.AspectStone`, `AspectRuneGraphic` |
| Town footprint, navigation, interaction and monument | `TownLayout`, `WorldView.AspectStone`, existing town movement and interaction |

Account `aspects` records ID, cumulative count and claimed level. Items store `aspectId` and `aspectLevel`; original `special` remains as generation provenance. Save schema 9 migrates older accounts to an empty collection without retroactive grants. Future schemas and invalid aspect state preserve the original save and refuse loading.

An imprint confirmation freezes the hero, item fingerprint, equipment state and collection level, then checks them again at commit. Save failure leaves the account unchanged. Salvage retains existing protection checks and batch atomicity; replayed receipts cannot grant duplicate copies. Filters, item comparisons, automatic cleanup protection and shop recommendations use the effective power.

Lv.1 preserves existing combat behavior. Lv.2–5 increase the planned primary magnitude by 5% of the base per level, with effect-specific choices for control duration, pull distance or spread. Damage snapshots capture aspect IDs and levels so projectiles, delayed hits and damage over time do not read later equipment state. Older snapshots default to Lv.1.

## Shared UI and town prop

The entry was generated with `tools/new_content_ui.py`. `ContentWindowView` owns the frame, safe area, scrollable body and fixed actions. Equipment uses `EquipmentSlotView`, `CharacterEquipmentView` and `ItemDetailView`; theme and fonts use `UiTheme`/`UiFonts`. Ownership is explicitly `EquipmentViewSource.Owned`, and `StoreViewBinding` refreshes after committed changes.

Landscape uses independently scrollable equipment, collection and detail columns. Portrait uses three steps. The equipment swap button opens the equipped-item list inside confirmation and retains the selected aspect. Class, registration and upgrade filters accompany name/effect search. Unregistered aspects remain inspectable. Korean, English and reading size preferences are supported.

Rune graphics are 123 distinct vector patterns determined by aspect ID. The existing runtime world builder creates a freestanding rock 5m wide, 7m high and 2m deep. Its center is `(-34, 4)` with interaction at `(-34, 1)`, clear of the central road. Existing scene, prefab and image GUIDs are unchanged. There is no field-placement tab or cost copy in the game screen.

## Validation

The final focused Edit Mode run passed **495 tests, with 0 failures and 0 skipped**. Coverage includes collection thresholds, receipt replay, save failure, imprint preservation, copy-loop prevention, all 123 actual Lv.5 combat effects, bilingual descriptions, captured levels, existing legendary effects, comparisons, shop, inventory, town and shared UI. [Test report](AspectRunestoneEvidence/final-focused.xml) · [Final source hashes](AspectRunestoneEvidence/final-source.json)

The earlier full regression ran **3,216 tests: 3,213 passed and 3 failed**. The failures concerned the original fireball aftershock attack basis, the new Graphic's CanvasRenderer declaration and localization registration. All were corrected and included in the passing 495-test run. The full suite was not repeated afterward; the overlapping reports must not be added together. [Earlier full report](AspectRunestoneEvidence/regression.xml)

The native macOS development build and both acceptance processes exited successfully. Actual salvage transactions collected 16 copies, followed by 14 synthetic uGUI pointer clicks covering town entry, manual single-level upgrades, notification lifetime, Rare imprinting, replacement, inline equipment selection, search, inactive aspects and close/reopen. A fresh process restored collection and equipment state. Layout acceptance retained selection, rejected Korean text in English mode, checked wrapped text height, fixed actions and safe-area bounds. [Initial run](AspectRunestoneEvidence/initial.txt) · [Fresh process](AspectRunestoneEvidence/resume.txt) · [Summary](AspectRunestoneEvidence/summary.json)

Shared UI ownership, its 9 regression tests, wiki generation/checking, 9 Python tests and Node UI checks over 252 pages, 24 databases and 2,329 records passed. Wiki generation uses this branch's design, source and evidence. Public deployment is performed from merged `main`.

### Native macOS captures

| Layout | Korean 100% | Korean 150% | English 100% | English 150% |
| --- | --- | --- | --- | --- |
| 440x956 | [↗](AspectRunestoneEvidence/layout-ko-100-440x956.png) | [↗](AspectRunestoneEvidence/layout-ko-150-440x956.png) | [↗](AspectRunestoneEvidence/layout-en-100-440x956.png) | [↗](AspectRunestoneEvidence/layout-en-150-440x956.png) |
| 956x440 | [↗](AspectRunestoneEvidence/layout-ko-100-956x440.png) | [↗](AspectRunestoneEvidence/layout-ko-150-956x440.png) | [↗](AspectRunestoneEvidence/layout-en-100-956x440.png) | [↗](AspectRunestoneEvidence/layout-en-150-956x440.png) |
| 1440x810 | [↗](AspectRunestoneEvidence/layout-ko-100-1440x810.png) | [↗](AspectRunestoneEvidence/layout-ko-150-1440x810.png) | [↗](AspectRunestoneEvidence/layout-en-100-1440x810.png) | [↗](AspectRunestoneEvidence/layout-en-150-1440x810.png) |
| 1440x900 | [↗](AspectRunestoneEvidence/layout-ko-100-1440x900.png) | [↗](AspectRunestoneEvidence/layout-ko-150-1440x900.png) | [↗](AspectRunestoneEvidence/layout-en-100-1440x900.png) | [↗](AspectRunestoneEvidence/layout-en-150-1440x900.png) |
| 1680x720 | [↗](AspectRunestoneEvidence/layout-ko-100-1680x720.png) | [↗](AspectRunestoneEvidence/layout-ko-150-1680x720.png) | [↗](AspectRunestoneEvidence/layout-en-100-1680x720.png) | [↗](AspectRunestoneEvidence/layout-en-150-1680x720.png) |

![Runestone on the left side of town](AspectRunestoneEvidence/01-town-stone.png)

[Collection upgrade notification](AspectRunestoneEvidence/02-pc-library.png) · [Inline equipment selection](AspectRunestoneEvidence/03-portrait-equipment-picker.png)

Validation uses an isolated checkout of current `main` and separate save paths. The original working directory and user account are not modified. Native macOS uGUI input evidence is distinct from physical mobile touch, safe-area and performance testing.
