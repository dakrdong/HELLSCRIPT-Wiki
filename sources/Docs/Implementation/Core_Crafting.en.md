# Forge core crafting

Updated: 2026-09-23 · [한국어](Core_Crafting.md)

## Rules

The fourth forge tab selects a core category and a specific legendary/set recipe. Existing town blacksmith access and content unlock rules apply. Rare crafting, enchanting, equipment enhancement and slot mastery remain available.

- Salvaging one legendary/set item grants one matching core through existing single, bulk and automatic salvage paths. Materials, enhancement stones and aspect rewards are preserved.
- `AccountSave.cores[8]` already stores account-wide balances. “Shared across bag and storage” reads that total once, without fictitious stacks or double counting. Both ring positions share ring cores.
- Crafting consumes 10 matching cores and 0–8,000 Abyssal Coins. Gold and ordinary materials are not charged. Results enter the current hero's bag; insufficient space rejects the transaction.
- Item level follows existing crafting: the current hero's highest actual clear, clamped to 1–60. Level 30 was an HTML demonstration value.
- Recipes come directly from active `ItemCatalog.Uniques`, with rarity, class and name filters. Other classes' equipment can be crafted. Disabled skill-design content is not activated.
- Recipe IDs deterministically select a base and four affix identities: two prefixes, two suffixes, legal slots, distinct groups and existing class weights. Browsing or language changes cannot reroll identities. Normal loot generation is unchanged.
- Investment `D` independently samples each affix's unbiased integer quality from `D` through `10,000`, inclusive. Investments of 0, 1 and 8,000 allow 0–100%, 0.01–100% and 80–100%. Values use `AffixDefinition.Value`. Investment does not change awakening, greater affixes, masterworking or special/set powers.

## Shared presentation

The existing `BlacksmithWindow` adapter gains a tab without a new canvas/window owner. Landscape/PC retains three equal columns: cores, equipment catalogue/details and quality. Portrait uses three pages with opposite slide directions.

The selected core row displays Refine with owned/10 below. There is no separate refine footer. Equipment and affix ranges appear side by side. A tap/drag gauge and −500/−100/−50/+50/+100/+500 buttons replace numeric input. Investment cannot exceed the wallet or 8,000. The track remains 0–100% and fills the possible interval from the selected minimum to 100%.

Shared owners are `EquipmentSlotView`, `EquipmentGradePalette`, `ItemDetailView`, `ItemTooltip` and `UiTheme`. Definition previews use `EquipmentViewSource.Catalog` with ranges; results use `EquipmentViewSource.RewardSnapshot` with the saved item. The existing inventory Abyssal Coin sprite is reused, with no new raster asset/resource GUID.

Catalogue art opens read-only details in a fixed-size popup with internal scrolling. Either close control, outside click or Escape closes it. Filters, selection, investment and list offsets survive orientation/language changes. `ContentWindowHost` owns input/window coordination; `StoreViewBinding` observes confirmed saves.

## Transactions and persistence

`CoreCrafting` owns definitions, quotes and quality. `GameStore.CraftCoreEquipment` executes through staged `Transact`; views never mutate saves. Execution rechecks hero, recipe, level, affix fingerprint, resources, capacity, unlocks and active-rift state.

Spending, the bag item, result snapshot and pending ID commit atomically. Save failures retain the previous account and emit no success event. Repeated request IDs cannot charge again. Pending results block further crafting and reopen when the core tab is entered after restart. Acknowledgement must persist before closing; failure keeps the result visible. Closing or exiting never rerolls.

Integrated schema 11 includes `AccountSave.coreCraft` history snapshots and a pending ID. Recorded values survive later enhancement/salvage. Earlier saves receive empty history while balances, items, growth, jobs and unrelated state are preserved. The existing future-schema guard prevents old clients silently dropping new records.

## Validation path

`CoreCraftingTests` and existing `BlacksmithTests` cover definitions, bounds, independent rolls, transactions, migration and salvage. A native development player runs with an isolated save:

```text
-hellscriptCoreCraftingSmoke -hellscriptSavePath <isolated-directory> -hellscriptScreenshots <evidence-directory>
```

Add `-hellscriptCoreCraftingVerify` in a separate process with the same save to check pending-result restoration/confirmation. After integrating inventory changes through `main` commit `ff25bae`, [130/130 integration tests](BlacksmithEvidence/core-crafting-main-editmode.xml) also passed. The counts below overlap and must not be summed.

Executed on 2026-09-23: **354/354** related Unity Edit Mode tests, **9/9** shared UI checks and **103/103** HTML tests passed. The macOS development build succeeded. Native acceptance used an isolated save and verified two legendary/set results, 20 cores spent and 8,000 Abyssal Coins spent. A second process restored and acknowledged the pending result.

Screens covered portrait 440×956, landscape 956×440 and PC 1600×900 / 1440×900 / 1680×720 in Korean/English, with 100% text and 150% text in portrait, landscape and PC. Synthetic pointer/keyboard input exercised class/name filters, outside/Escape/close controls, increment buttons/gauge, equal columns and retained drafts. Physical devices, notched safe areas and mobile performance were not tested. Existing URP post-processing shader warnings remain at player startup; core acceptance produced no runtime exceptions.

Evidence: [test XML](BlacksmithEvidence/core-crafting-editmode.xml), [summary](CoreCraftingEvidence/validation.json), [runtime](CoreCraftingEvidence/result.txt), [restart](CoreCraftingEvidence/restart-result.txt), [portrait forge](CoreCraftingEvidence/portrait-ko-forge.png), [English landscape](CoreCraftingEvidence/landscape-en-forge.png), [PC catalogue](CoreCraftingEvidence/pc-16x9-ko-catalogue.png), [large text](CoreCraftingEvidence/large-text-portrait.png).

Implementation: [domain](../../Assets/HELLSCRIPT/Runtime/Core/CoreCrafting.cs), [transaction](../../Assets/HELLSCRIPT/Runtime/Core/GameStore.CoreCrafting.cs), [tab](../../Assets/HELLSCRIPT/Runtime/Presentation/BlacksmithWindow.Cores.cs), [dialogs](../../Assets/HELLSCRIPT/Runtime/Presentation/BlacksmithWindow.Cores.Dialogs.cs), [tests](../../Assets/HELLSCRIPT/Tests/Editor/CoreCraftingTests.cs), [native acceptance](../../Assets/HELLSCRIPT/Runtime/Presentation/RuntimeCoreCraftingSmoke.cs).

The merge owner regenerates, verifies and publishes the wiki from merged `main`.
