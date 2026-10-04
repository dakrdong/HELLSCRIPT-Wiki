# Shared item detail rebuilt after Diablo IV

Updated: 2026-10-05 · [한국어](Item_Detail_D4.md) · Rules: [Shared equipment comparison rules](../Design/Equipment_Comparison_Rules.en.md)

`ItemDetailView`, `ItemDetailPopup` and `EquipmentComparisonView` now present items with the Diablo IV information hierarchy. Calculation, transactions and saves are unchanged.

| Change | Owner |
| --- | --- |
| Primary value/name/delta on one line, branch-line implicits, greater-affix emphasis, socket ring, requirement footer, two-column card, scroll-down cue | `ItemDetailView.Style.cs`, `ItemDetailView.cs` |
| Landscape detail window up to 640 wide, up to four footer buttons per row | `InventoryWindow.Dialogs.cs`, `ItemDetailPopup.cs` |
| Portrait comparison tabs (`Selected gear / Equipped`) | `EquipmentComparisonView.cs` |

## Validation (2026-10-04)

- 82 focused EditMode tests passed: `ItemComparisonTests`, `EquipmentArtTests`, `ItemArtCoverageTests`, `LocalizationTests`. A lone equipped card at full width shows the primary-value name separately, so the dense-card assertion now applies to cards narrower than 300.
- `check_ui_contract.py` and `test_ui_contract.py` passed.
- macOS development build `-hellscriptItemDetailPopupSmoke` passed (`HELLSCRIPT_ITEM_POPUP_RUNTIME_OK`): 440×956, 956×440, 1440×810, 1440×900, 1680×720 × Korean/English at the default text size. [Result](ItemDetailD4Evidence/popup-result.txt)
- Screens: [landscape detail](ItemDetailD4Evidence/detail-956x440-ko.png) · [portrait detail](ItemDetailD4Evidence/detail-440x956-ko.png) · [English landscape](ItemDetailD4Evidence/detail-956x440-en.png) · [portrait comparison tabs](ItemDetailD4Evidence/comparison-440x956-ko.png) · [landscape comparison](ItemDetailD4Evidence/comparison-956x440-en.png) · [PC comparison](ItemDetailD4Evidence/comparison-1440x810-ko.png)

Not checked on a physical device; input was synthetic macOS input. The full EditMode and smoke suites were not rerun in this step.

## Landscape default comparison (2026-10-04)

`InventoryWindow.ShowDetail` opens an unequipped item through `EquipmentComparisonView` in landscape when the slot has equipped gear. `-hellscriptItemDetailPopupSmoke` passed for five sizes × Korean/English (the `score-*` check now reads the candidate card). [Screen](ItemDetailD4Evidence/default-compare-956x440-ko.png)

## Ornament art and motion (2026-10-05)

On the request to use full design taste and make the UI juicier, with Codex images allowed where needed, the card ornament was raised another step. Rules are under `Ornament art and motion (2026-10-05)` in the [Shared equipment comparison rules](../Design/Equipment_Comparison_Rules.en.md); the art's generation and import record is [Item detail card ornament art](../Art/ItemCard/Item_Card_Art.en.md).

| Change | Owner |
| --- | --- |
| Five Codex images (corners, divider, score plaque, socket ring, greater-affix star), rarity halo / light streak / fade curves, entrance `ItemCardIntro` | `ItemCardArt.cs`, `Resources/Art/ItemCard/` |
| Rarity frame, score plaque with ▲▼, major dividers, halo, star and socket ring, comparison row wash, special-power border, scroll-cue strip | `ItemDetailView.Style.cs`, `ItemDetailView.cs` |
| Detail window title flourish | `ItemDetailPopup.cs` |
| Importer and platform budget (`Art/ItemCard/`) | `ItemCardArtImporter.cs`, `ResourceTextureBudget.cs` |
| Density by height: below 420 a shorter plaque and vector rules, below 300 no halo or plaque | `ItemDetailView.Style.cs` |

### Validation (2026-10-05)

- 110 focused EditMode tests passed: six new `ItemCardArtTests` (art loading and crop bounds, four corners / plaque / halo, vector trim kept on dense cards, greater-affix star, entrance steps and light streak, comparison arrows and row wash) plus `ItemComparisonTests`, `EquipmentArtTests`, `ItemArtCoverageTests`, `LocalizationTests`, `CurrencyArtTests` and `UiButtonTests`.
- `check_ui_contract.py`, `test_ui_contract.py` and `check_ui_refresh.py` passed.
- macOS development-build smokes passed: `-hellscriptInventorySmoke` (landscape and portrait, Korean/English; real clicks on the portrait comparison tabs added), `-hellscriptItemPolishSmoke` (440×956, 956×440, 1440×810, 1440×900, 1680×720 × Korean/English, storage / shop / forge details, drag), `-hellscriptItemRangesSmoke`, `-hellscriptBlacksmithSmoke` and `-hellscriptItemDetailPopupSmoke`. The entrance was checked with `-hellscriptUiMotion` frames at one-fifth speed.
- Smoke maintenance: once portrait comparison became tabs, `-hellscriptInventorySmoke` failed because it assumed both cards at once. It passes on `main` before this work (53502b35), so it now understands tabs through `ComparisonPanes` / `CheckEquippedCard` and clicks the tabs for real. Because the primary-value text is composed differently on dense and wide cards, the tooltip signature compares line names (`CheckComparison` still verifies every line's text).
- Smokes that also fail on `main` before this work (53502b35): `-hellscriptEquipmentShopSmoke`, `-hellscriptStorageSmoke`, `-hellscriptAspectStoneSmoke`; unrelated to this change. `-hellscriptRiftVictorySmoke`, `RewardBox`, `RecommendedEquipment` and `GemSocket` need evidence-path arguments and were not run.
- Screens: [portrait detail Korean](ItemDetailD4Evidence/juicy-detail-portrait-ko.png) · [portrait detail English (legendary)](ItemDetailD4Evidence/juicy-detail-portrait-en.png) · [landscape default comparison](ItemDetailD4Evidence/juicy-default-compare-landscape-ko.png) · [PC default comparison (legendary)](ItemDetailD4Evidence/juicy-default-compare-pc-en.png) · [portrait ring comparison](ItemDetailD4Evidence/juicy-ring-compare-portrait-ko.png) · [PC ring comparison](ItemDetailD4Evidence/juicy-ring-compare-pc-ko.png) · [portrait equipped tab](ItemDetailD4Evidence/juicy-equipped-tab-portrait-en.png) · [shop](ItemDetailD4Evidence/juicy-shop-landscape-en.png) · [storage](ItemDetailD4Evidence/juicy-storage-landscape-en.png) · [forge](ItemDetailD4Evidence/juicy-forge-landscape-en.png) · [five entrance frames](ItemDetailD4Evidence/juicy-motion-sheet.png) · [smoke result](ItemDetailD4Evidence/popup-result.txt)

### Not verified

Not checked on a physical device; input was synthetic macOS input. Android and WebGL builds and texture sizes were not measured (only the budget rows were added). The entrance runs per frame for about 0.8 s and removes itself, but its frame cost was not measured. The full EditMode result after the last code change is recorded below.

### Full EditMode run (2026-10-05)

After the last code change (commit `ea4b6317`, on the same base as `main`) the full EditMode suite ran once: 5,165 of 5,165 passed, none failed or skipped, 1,877 s. Only the runtime smokes listed above were run; the full smoke bundle was not.
