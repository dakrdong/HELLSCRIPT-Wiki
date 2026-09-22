# Rune Master

Created: 2026-09-21 · [한국어](Rune_Master.md)

## NPC and layout

A dedicated Rune Master stands beside the rune merchant in the southeast of town. Approach and explicitly choose **Open rune workshop**. Arrival does not open the window. The overlay blocks town movement.

The window reuses the shop/storage olive surfaces, brass borders, fonts and buttons. Existing `RuneBoardGraphic` renders real rune colors and shapes, fitting even long blocks within each icon. Landscape places the workshop left and storage right; portrait places the workshop above storage. Storage always uses eight columns of square slots with vertical scrolling.

## Reshaping

Materials must be two distinct runes with the **same color, shape and grade**, each with at least three cells. Placed runes, preset references and unclaimed results are protected. Tap a stored rune or drag it into either material slot to automatically register its partner. Registration, clearing and closing are temporary and do not consume anything.

A missing partner shows the user's requested explanation. If matching color/shape runes only exist at another grade, the message explains the grade requirement. **Fuse** replaces the two materials with one different shape of the same color, cell count and grade. Rotation-equivalent shapes are excluded. No currency fee is charged.

After a successful save, both blocks converge with a short glow, then the result appears. Duplicate input is disabled during the reveal. Closing during animation cannot lose the saved output.

## Ownership and persistence

The feature uses actual instances in `AccountSave.runes.owned`. UI selection is transient. `GameStore.ReshapeRunes` revalidates revision, ownership, protection and material matching, stages the full account and publishes live changes only after persistence succeeds. A failed write preserves inputs and disk state and permits retrying the same request. Transaction receipts make retries idempotent.

The current authoritative persistence boundary is the local `GameStore`. This change does not introduce a remote account server or duplicate rune ownership store.

## Ascension

Grades are read from `RuneEconomy.Bands`, currently G0–G6. Each grade shows its stored count across all sizes and shapes. Storage groups eligible instances by grade, color and shape, with highlighted counts for two or more. Locked, placed, preset-linked, pending and maximum-stage runes are excluded from material candidates.

Tap a stack to register one actual instance in its cell-count group. Tap its small material silhouette to unregister it. Changing grades or tabs clears temporary selection without losing persisted pending results.

Every selected color within each size group must have an even count, and all materials must share one grade. Shapes may differ. Two 1–4-cell runes become the next size at the same grade; two 5-cell runes become one cell at the next grade. G6 five-cell runes are final. Ability color is independent of grade and remains unchanged. The output shape is selected from the real size catalog with a transaction-ID seed. The request limit is 100 pairs, with no fee or random failure.

**Fuse group** validates only its group. **Fuse all groups** is enabled only when every nonempty selected group is valid. Odd color counts and pending outputs explain disabled actions. All groups are validated before any materials are consumed and all results are saved atomically.

Each pair previews one question mark. Successful synthesis compresses and briefly brightens the cards before revealing real silhouettes. **Skip animation** is stored per device and applies to both workshop tabs. Duplicate synthesis/claim inputs are blocked during reveal. Clicking any one result claims all pending outputs for the currently selected source grade; other grades remain pending.

Existing `OwnedRune.pending/sourceGrade/sourceSize` fields restore results after tab changes, closing or restarting. `GameStore.AscendRunes` and `ClaimRuneAscensions` use revision checks and transaction receipts. The older rune-board entry uses the same `RuneAscension` rules. The additive `OwnedRune.locked` field defaults to false in existing saves; save versions and instance IDs are preserved.

## Reshaping validation

- All 63 focused Unity Edit Mode tests passed, covering reshaping, save failures, retries, reload, town routes and localization.
- The macOS development build completed with zero errors. Native uGUI raycast/pointer-event acceptance passed for NPC proximity and explicit interaction, both material drop targets, outside cancellation, error messages, eight columns, duplicate-input gating, actual consumption/grant, closing mid-reveal and disk reload.
- Captured PC windows at 1600×900 and 1920×1080, plus 440×956 portrait and 956×440 landscape examples matching the requested iPhone 17 Pro Max proportions, in Korean and English. This is desktop window/input evidence, not physical-mobile touch, notch or performance proof.
- [PC](RuneMasterEvidence/03-rune-reshape-ready-pc-ko.png), [portrait result](RuneMasterEvidence/07-rune-reshape-result-portrait-ko.png), [landscape](RuneMasterEvidence/05-rune-reshape-ready-landscape-ko.png), [runtime report](RuneMasterEvidence/runtime-rune-master-smoke.txt).

## Ascension and final integration validation

All **186 focused Edit Mode tests passed**, covering every grade/size/color recipe, atomic multi-group synthesis, odd and mismatched materials, duplicate instances, protection, final stage, pending-group rejection, the 100-pair limit, failed persistence, replay and reload. The suite includes existing rune, equipment shop, gambling shop, storage, town and localization regressions.

The macOS development build completed with zero errors. Native acceptance passed with 51 actual uGUI raycast/pointer-event clicks and five drags, including a simulated touch pointer in portrait. A destroyed-scroll reference found during repeated tab switching was fixed and the entire runtime acceptance rerun passed. Coverage includes group/all synthesis, question-mark previews, reveal input gating, skip preference, grade switching, closing mid-reveal, pending recovery, one-card claim-all and returning to equipment/storage windows.

- [Ascension on PC](RuneMasterEvidence/09-rune-ascend-materials-pc-ko.png)
- [Pending results in portrait](RuneMasterEvidence/11-rune-ascend-pending-portrait-ko.png)
- [Landscape](RuneMasterEvidence/12-rune-ascend-storage-landscape-ko.png)
- [English](RuneMasterEvidence/13-rune-ascend-storage-pc-en.png)
- [Final tests](RuneMasterEvidence/final-editmode.xml), [runtime report](RuneMasterEvidence/runtime-rune-master-smoke.txt), [scope](RuneMasterEvidence/validation.json)

Mobile-sized captures are macOS window evidence, not physical-iPhone touch, safe-area or performance validation. Public wiki deployment waits for merging into main, as required by repository policy.
