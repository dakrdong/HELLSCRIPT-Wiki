# Rift Gear Offer Cards Implementation Plan

Updated: 2026-10-13

[한국어](Rift_Gear_Offer_Cards_Plan_20261013.md)

For the Codex implementation task. The approved mockup is "Rift gear recommendation UI" (Claude artifact `https://claude.ai/artifact/1fVkuNDqhdWyHN7ZFmFMmp`, private). If it cannot be opened, the dimension and wording tables below are authoritative. The mockup is HTML: translate colours and gradients to `UiTheme`, `UiButton`, `StorageSurface` and `EquipmentGradePalette`; add no new colour literals (`tools/check_ui_contract.py` checks them).

## 0. Mockup contents

The artifact has 16 boards: four screens × (portrait 440×956 Korean / portrait English / landscape 956×440 Korean / landscape English).

| Screen | Boards |
| --- | --- |
| 1 Battle offer cards | `Main`, `MainEn`, `MainLand`, `MainLandEn` |
| 2 Details (nothing worn) | `Detail`, `DetailEn`, `DetailLand`, `DetailLandEn` |
| 3 Details (compare with worn) | `Compare`, `CompareEn`, `CompareLand`, `CompareLandEn` |
| 4 Recommended gear settings | `Settings`, `SettingsEn`, `SettingsLand`, `SettingsLandEn` |

English boards use the game's `en.txt` wording; for new strings the table in §4.4 is authoritative. Landscape boards were derived from the portrait ones, so if a layout looks off, fix the mockup first and then implement.

## 1. Current state and scope

Branch `claude/rift-gear-offer-actions` (commit `c48c24d5`, pushed, not merged) holds the pre-mockup implementation. Continue on that branch.

| Area | Already there (c48c24d5) | To do |
| --- | --- | --- |
| Card | Name, icon, part, `score (+gain)`, Equip/Details buttons, gear, × (width 112) | **Remove Details, add a magnifier at the icon's top-right, centre a smaller Equip button, card width 92, apply mockup metrics and look** |
| Details | Card Details button → `RiftRecommendationWindow` (`compareFirst`) | The magnifier opens it; compare view when a worn item is replaced |
| Settings | `RiftGearSettingsWindow`, four sections, save | Match mockup wording, order and layout; add the items below |
| Rules | Settings gating, pickup-order queue, timer | Pin the lifecycle (town, end, re-entry) with tests |

## 2. Decisions (final)

1. Cards show **only on the battle screen**; leaving it hides them.
2. Re-entering the same run shows only items from that run that **match the conditions and are not yet equipped or closed**.
3. When the run ends (death, clear, give-up — anything that clears `suspendedRun`) all cards are gone.
4. **While in town via portal, no cards are shown.** They come back on return; the run has not ended, so nothing is deleted.
5. Eligibility is judged **once, at acquisition** (settings apply to gear acquired after saving). Cards already queued stay when settings change.
6. Rarities are Normal, Magic, Rare, Legendary, Set. **There is no Unique rarity in the game; do not add one.**
7. The queue is in **pickup order**, one card per slot; the next item of a slot appears after the previous is handled (equipped, closed, expired).
8. Time expiry is handled exactly like ×.

## 3. Open items (proceed with defaults, report them)

| Item | Default |
| --- | --- |
| Tapping the icon body | Opens details like the magnifier (larger touch target); the magnifier is the visible hint |
| Timer while a popup is open | Paused; restarts from zero on close (implemented) |
| Remaining-time indicator on cards | Not in the mockup; do not add |

## 4. Screen specification

### 4.1 Offer card (mockup board 1, reference width 440)

The strip sits at x=12, cards 4 apart, just above the bottom HUD (`BattleHudTopOnPage`), wrapping to more rows when needed. All values are px from the card's top-left.

| Element | Position (x, y) | Size | Notes |
| --- | --- | --- | --- |
| Card | — | 92×150 (landscape: `slot+98` with a 54 icon) | 1px border, green family (`SuccessHex`) |
| Gear | (0, 0) | 24×24 | `StorageGlyph "gear"`, on every card, opens the same settings |
| × | top-right | 24×24 | Close as acknowledged |
| Item name | (3, 20) | 86×14, 10pt bold | Grade colour, one line, ellipsis |
| Icon | ((92−slot)/2, 36) | slot (52/54) | Existing `EquipmentSlotView` |
| Magnifier | inside the icon's top-right (x+33, y+1) | 18×18 | Named `rift-gear-inspect-{id}`, opens details |
| Part | (2, 91) | 88×13, 9pt | Gold |
| Score pill | (5, 105) | 82×17, 10pt bold, rounded | `500 (+400)`; gain only when something is replaced, negative in `DangerHex` |
| Equip | **centred** ((90−58)/2=16, 124) | **58×20**, 11pt | Primary gilt button; about 4 below, glow must not spill outside the card |

No Details button. Score format `0.#` (e.g. `149.6 (+28.6)`); it must not clip, also in English.

**On landscape screens (`root.width > root.height`: 956×440, 1600×900, …) the whole card is 75% of the portrait size.** Multiply every position, size and font size above by 0.75 (card about 69×112, icon slot also ×0.75). The strip's left start of 12 and the card gap stay, and the strip is aligned just above the HUD (about 8 above) for the reduced height. The mockup expresses it as `transform: scale(.75)`. Check that the shrunken text is readable at 956×440 in Korean and English; if not, do not change the ratio on your own — report it (consider 80–85%).

Rest of landscape: the bottom HUD is a single row (HP bar left, slots right) and the cards sit above it at the left.

### 4.2 Detail window (boards 2 and 3)

Keep `RiftRecommendationWindow` (shared `ItemDetailView` / `EquipmentComparisonView`). The magnifier opens with `compareFirst=true`: compare view when a worn item is replaced (worn left, candidate right, `score A → B` on the candidate, slot choice for rings and weapons), a single candidate otherwise. Footer `Equip` / `Close`; Close acknowledges (existing); a successful equip closes the window.

### 4.3 Recommended gear settings (board 4)

Inside the shared `ContentWindowView` (max 560×780), in this order. On landscape it is a centred 560×424 window whose body alone scrolls, with `Save`/`Close` (`Equip`/`Close` for the detail and compare windows) pinned at the bottom (the mockup shows it clipped); edit a draft, then `Save` (one transaction) or `Close`. Use localization keys with Korean and English filled.

1. **Rarities to recommend** — 3-column checkbox grid: Normal, Magic, Rare, Legendary, Set.
2. **Recommendation range** — Main hand, Off hand, Helmet, Chest armor, Gloves, Boots, Belt, Amulet, Ring; each row has an enable checkbox and `Details` (`Details enabled` when on).
   - Expanded panel: `Enable detailed filters` checkbox, a note (only gear meeting **every** selected option at or above its value is recommended; with none selected there is no condition), `Common options` rows (checkbox + name + number input + `or more`), enabled `Additional options`, `+ Add condition option` (the remaining options valid for that slot). One position expands at a time. Look and behaviour follow the bag auto-salvage detail settings (`EquipmentShopWindow.Settings`); data is separate.
   - Note under the list: non-matching or switched-off gear is not recommended; settings apply to gear acquired after saving.
3. **Recommendation rule** — radios: all equippable gear / gear with a higher score than what is worn (default).
4. **Popup duration** — radios: until I press cancel (default) / close after a set time. Only the latter enables the slider (1–60 s, default 10 s, value shown as `10s`) and the caption `1 s – 1 min (used only with a set time)`. An expiry/order note follows.

### 4.4 English wording (new items)

| Korean | English |
| --- | --- |
| 추천 장비 설정 | Recommended Gear Settings |
| 추천 표시 등급 / 추천 범위 / 추천 여부 / 팝업 시간 | Rarities to recommend / Recommendation range / Recommendation rule / Popup duration |
| 착용 가능한 모든 장비 | All equippable gear |
| 현재 착용중인 장비보다 점수가 높은 장비 | Gear with a higher score than what is worn |
| 취소 버튼을 누를 때까지 / 지정 시간 후 닫힘 | Until I press cancel / Close after a set time |
| ＋ 조건 옵션 추가 | + Add condition option |
| 1초 ~ 1분 (지정 시간 선택 시에만 사용) | 1 s – 1 min (used only with a set time) |
| 상의 | Chest armor |

For the three long notes, the wording already in `Resources/Localization/en.txt` is authoritative.

Check these differences between the mockup and the current build: slider caption, radio wording, five grades, panel spacing.

## 5. Rules and data

- Storage: `HeroSave.riftGearOffer` (`RiftGearOfferSettings`: `gradeMask` 31, `rangeMask` 511, `allWearable`, `timed`, `seconds` 1–60, per-position `RiftGearRange{id, details, thresholds}`). Per hero, account save; not device-local like language or orientation.
- Eligibility: `RiftGearRecommendations.CanOffer` → `RiftGearOfferSettings.Accepts` (rarity, range, required options), then the rule (`allWearable`: any valid equip plan; otherwise a higher post-swap score).
- Queue: `DropState.gearOrder` (`max+1` at acquisition), one card per slot. `gearHandled` is saved through the existing `DismissRiftGear`.
- Timer: `GameUI.GearRecommendations.ExpireGearRecommendation`, counted only for visible cards; expiry runs the same transaction as ×; a failed save retries after the next interval (avoid repeating toasts).
- Every new `HeroSave` field must be added to the per-hero copy list in `GameStore.Transact` (`target.riftGearOffer=source.riftGearOffer`); otherwise saved settings vanish from memory. Done; re-check for any similar field.

## 6. Steps

1. **Card layout** — `GameUI.GearRecommendations.cs`: width 92 and height (everything ×0.75 on landscape), remove Details, magnifier (`rift-gear-inspect-{id}`), centred 58×20 Equip, 24 px ×, 24 px gear, score pill; keep the constants in one place.
2. **Magnifier/icon behaviour** — both call `OpenGearRecommendation(id, compareFirst: true)`. Fix the smoke's assumption of toggling `rift-gear-compare` (it now opens in compare).
3. **Settings window** — diff `RiftGearSettingsWindow.cs` against 4.3 (wording, order, caption, spacing); English in `Resources/Localization/en.txt`.
4. **Lifecycle** — prove §2 items 1–4 with tests and smoke (below).
5. **Docs** — update the Rift recommendation section of `Docs/Implementation/Hunt_Edict_Control_Refinement.md` and both languages of this plan. If wiki sources change follow `AgentRules/Wiki_Publish.md`.

## 7. Verification

During work run only related checks; run the full Edit Mode suite and the macOS runtime smoke once after the final change and integration into `main` (global rule).

- Edit Mode (`Hellscript.Tests.RecommendedEquipmentTests` and related): rarity/range/required-option/rule gating, settings save and reload (**live hero included**), pickup-order queue, next card after expiry, `Visible` empty after the run ends, no card creation or display during the portal town visit (`run.portal`, `Page != "battle"`), re-entry restores only unhandled cards.
- Smoke: `-hellscriptEdictControlsSmoke -hellscriptEdictGearSmoke -hellscriptSavePath <empty dir> -hellscriptEvidence <empty dir>` (pass arguments as an array; isolated paths are required). Sizes 440×956, 956×440, 1600×900, 1600×1000, 2100×900 in Korean and English: cards above the HUD and inside the safe area (landscape at 0.75 with readable text); card controls (Equip, magnifier, gear, score) inside the card with no clipped text; magnifier → details (compare); Equip button; settings open, save and time expiry; hidden during the town visit and shown again after return.
- `python3 tools/check_ui_contract.py --verify`.
- Report what was checked and what was not (no physical device or touch; never report a macOS result as a device result).

## 8. Traps

- A resize rebuilds the battle page and closes popups without acknowledgement. A same-size language change can repaint and keep the detail window open. The smoke explicitly rebuilds the page before opening the next detail view. Settings drafts lost on page rebuild remain an accepted limitation.
- `JsonUtility` reads a missing list as `[]`: call `Normalize()` on read and never trust list lengths.
- The default smoke entry (`-hellscriptEdictControlsSmoke` alone) fails at the settings step regardless of this work; add `-hellscriptEdictGearSmoke` to run only the gear flow.
- Do not re-add the removed text-size adjustment or its 50–150% checks.
- Clean builds and temporary artifacts per `AgentRules/Artifact_Cleanup.md` and record the final state.

## 9. Implementation record · 2026-10-13

- Inspected all 16 board structures/strings and enlarged card/settings views in the verified in-app browser. Continued the existing `c48c24d5` implementation and plan on an isolated checkout containing latest `main` (`a5596519`); preserved both unrelated tutorial drafts in the original checkout.
- Translated the card metrics, 75% landscape scale, magnifier/icon hit area, centred Equip, score pill, ellipsis, settings sections/filters/wording and timer caption through shared owners. The magnifier opens comparison immediately. A content-specific `EquipmentComparisonView` choice keeps portrait offer cards side by side; other callers retain default portrait panes.
- Added acquisition-order and acquisition-once checks, portal/terminal boundaries and live-hero/reloaded-hero settings coverage. Normalize missing settings lists on load and suppress repeated identical expiry-save toasts.
- Defaults used: icon body opens details; popup duration pauses and restarts from zero on close; no remaining-time indicator.
- Final related checks and gear-only macOS acceptance results are consolidated here. Full Edit Mode, the full runtime batch and public wiki follow the once-at-main-merge validation/publishing rule.
- Artifact ownership/size/disposition is recorded in `Builds/RiftGearOfferCards20261013/artifact-lifecycle.json`. No new raster assets, dependencies or full project copies.

### Final verification

- [Validation record](RiftGearOfferCardsEvidence20261013/validation.json): 185 related Edit Mode tests passed, zero failed/skipped. Shared UI ownership, 15 validator regressions and UI refresh checks passed. Unchanged core/shared owners and their tests reused successful results.
- Native macOS checks covered five viewports in Korean and English: screen-pixel metrics, 75% scale, safe bounds, controls/labels and fixed actions. Cards reuse the HUD pixel canvas and sit above the floating Hunt Edict hit area. Visibility toggles only when its state changes.
- Verified magnifier/icon comparison, detail/quick equip/close, red negative gain, settings draft/additional conditions/save/cancel, live/reloaded hero equality, actual popup-time pause/restart/expiry, empty-slot details in ten combinations, inventory hiding, portal town/return, unhandled-only re-entry, run end and ended-run reload.
- After the initial gear flow, only failed/changed coverage was rechecked four times. Fixed the HUD input blocker, screen-pixel/glyph scaling, a test assumption about language-only detail repaint, modal timer resets and deferred inventory-page visibility. Unchanged settings/comparison results were reused.
- About 71 minutes elapsed after the artifact record; validation tools ran about 6 minutes, with zero approval/connection wait. One related Edit Mode run, one validator regression run, five native builds, one initial gear flow and four affected rechecks.
- Full Edit Mode/full runtime and wiki generation/public publishing await main merge. No WebGL or physical mobile/touch/notch checks. The 75% text was visually checked at 956×440 on macOS; the scale was not increased unilaterally.

![Native portrait cards and HUD crop](RiftGearOfferCardsEvidence20261013/cards-portrait-ko-crop.png)

![Landscape cards in English](RiftGearOfferCardsEvidence20261013/cards-landscape-en.png)

![Shared side-by-side comparison](RiftGearOfferCardsEvidence20261013/comparison-portrait-ko.png)

![Detailed filters in English](RiftGearOfferCardsEvidence20261013/settings-filters-portrait-en.png)
