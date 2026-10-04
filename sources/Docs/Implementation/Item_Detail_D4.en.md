# Shared item detail rebuilt after Diablo IV

Updated: 2026-10-04 · [한국어](Item_Detail_D4.md) · Rules: [Shared equipment comparison rules](../Design/Equipment_Comparison_Rules.en.md)

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
