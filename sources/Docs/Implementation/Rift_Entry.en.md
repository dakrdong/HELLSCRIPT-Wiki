# Town rift entry and first-clear rewards

Updated: 2026-10-02 · [한국어](Rift_Entry.md)

The town rift interaction opens the approved preparation layout with actual hero state and domain transactions. The services shortcut opens the shared Hunt Edict Repeat Hunt tab and restores the entry window when closed.


## 2026-10-02 compact replacement popups

Potion replacement uses a **single-column list of 56-high rows**, replacing 150-high two-column cards. Shared potion art sits next to the name, owned count, effects, equipped slot and selection action. The shared body scrolls for long lists; short lists fit their content height. Selecting the same equipped potion removes it; selecting another slot's family swaps slots. Existing saved transactions and unchanged stock remain authoritative.

The remaining-fatigue help button and its dedicated guide popup were removed. Fatigue, restore count, daily/paid allowances, reset display and recovery transactions remain.

Active-skill and ultimate replacement put descriptions beside the candidate grid in landscape and directly below it in portrait. Candidate count determines grid height. The fixed 190-high list and nested scroll were removed. These selectors use shared content-height fitting and disable scrolling while keeping every learned candidate, rank/effect/activation descriptions and saved replacement actions. Shared `ContentWindowView`, `ContentWindowHost`, `UiButton`, `SkillIconView` and existing domain transactions retain ownership.

`-hellscriptRiftPickerSmoke` uses an isolated save and default text size to exercise KO/EN at five aspect ratios, selecting all 16 active skills and two ultimates for each of the three classes. It verifies compact rows, description bounds, unchanged scroll position, saved transactions and disk readback.

First-open descriptions were measured before `CanvasScaler` applied the screen scale on its next update. The shared window now applies that same scale to its `Canvas` before rendering content. This fixes the measurement order without extra padding or a per-screen repaint.

### Replacement popup verification

- Unity 6000.6.0f1 covered **139 relevant Edit Mode tests**. The first run passed 138 and failed one for two pre-existing unused translations. After removing those entries, both directly affected localization checks passed. Following the shared scale fix, all **23 shared UI tests** passed. The full-game suite was not run. [Initial result](RiftPickerEvidence/editmode.xml) · [Localization retry](RiftPickerEvidence/localization-retry.xml) · [Shared UI retry](RiftPickerEvidence/shared-ui-retry.xml) · [Baseline translation comparison](RiftPickerEvidence/baseline-localization.json).
- Shared UI ownership and **11/11** contract regression tests passed; the macOS Development build reported zero build errors. A protected project copy and isolated save avoided disturbing the original Editor.
- **Ten combinations** passed at default text size: KO/EN at 440×956, 956×440, 1600×900, 1600×1000 and 1680×720. **550 pointer selections** covered each class's 16 active skills, two ultimates and a single learned candidate. Bounds and wheel-input invariance passed. Potion swap/unequip/family replacement, unchanged stock, active/ultimate replacement and disk readback passed. [Runtime record](RiftPickerEvidence/picker-runtime.txt) · [Summary](RiftPickerEvidence/validation.json) · [Tested source hashes](RiftPickerEvidence/source-sha256.json).
- This is macOS validation with EventSystem pointer and wheel input. Physical mobile was not tested, and these results do not establish public Web player acceptance.

[PC potion list](RiftPickerEvidence/potions-1600x900-ko.png) · [Portrait potion list](RiftPickerEvidence/potions-440x956-en.png) · [Single skill, PC](RiftPickerEvidence/single-skill-1600x900-ko.png) · [All skills, portrait](RiftPickerEvidence/skills-440x956-ko.png) · [All skills, landscape](RiftPickerEvidence/skills-956x440-en.png) · [All skills, 21:9](RiftPickerEvidence/skills-1680x720-en.png).

## 2026-10-02 HTML composition parity

The reference is `Prototypes/RiftEntry/HELLSCRIPT-RiftEntry.html` and its `styles.css`/`ui.js` sources. These rules replace the layout exceptions documented on September 30.

1. Landscape uses **0.95 : 1 : 1.05** columns for art/stage, fatigue/potions, and skills/entry. Fatigue sits at the top of the middle column and potions at its bottom. Skills are centred in the remaining right-column body.
2. The whole page follows the reference's `--b` calculation. Its `3.7b` masthead contains the small HELLSCRIPT brand, entry title, and vertically centred coin artwork/amount. Artwork crop, shading, spacing and the shared display font for numbers follow the reference.
3. Entry actions are **stacked at the lower right in landscape**, and **side by side across the bottom in portrait**. Readiness/errors/bag cleanup share that fixed area. Portal resume/restart and pending/cancel states use the same positions.
4. Portrait stacks the art strip, fatigue, three potions, four normal skills plus ultimate, auxiliary actions, and entry actions. Page scrolling is disabled; spare height is distributed between sections. Exhausted fatigue and locked ultimate states retain essential controls.
5. The entry opts into the shared `SkillIconView` square frame, preserving skill identity, artwork and passive semantics. Other callers retain their circular active seals. Empty learned slots still open the skill tree; owned potion equip/clear/swap still persists through existing transactions.
6. Existing services and battle history, absent from the mockup, remain as a compact auxiliary row below skills. Services open the **shared Hunt Edict Repeat Hunt tab**. Recommended mode exposes that tab and its existing explicit mode switch without silently changing the mode or applying locked custom policies.
7. History rows use measured text height plus 12 units of vertical padding, with 4-unit gaps. Selecting a record opens the **shared combat text log directly**. Closing returns through log → original filtered/scrolled record list → original selected Rift stage. The obsolete full-screen record summary and its dedicated routes were removed; archived combat data was retained.

This adapter keeps `ContentWindowView`/`ContentWindowHost` canvas, safe area, stacked input and Back handling, changing only this window's header/action geometry. It reuses `UiTheme`, `UiFonts`, Minimal `UiButton` chrome, `PotionArt` and `SkillIconView`. No new raster art, independent theme/font or save system was added. Current shared game artwork and actual account/unlock state replace the prototype's fixture data.

### Final verification, 2026-10-02

- Unity 6000.6.0f1: **105/105** tests passed across six directly affected Edit Mode suites. Shared UI ownership check and **11/11** contract tests passed. The full Edit Mode suite was not run.
- macOS development build succeeded. An isolated save exercised **10 combinations**: KO/EN at portrait 440×956, landscape 956×440, and PC 1600×900, 1600×1000 and 1680×720. Default text size only. No page-scroll, overlapping-control or text-overflow checks failed.
- EventSystem raycasts and pointer input verified repeat → entry and list → text log → list → entry, preserving the selected stage. Of 20 sample records, 5–6 complete rows fit in landscape and 13 in portrait. Controls survived seven idle seconds and two autosaves without being rebuilt. Empty-skill navigation, potion swap/clear/equip with unchanged inventory counts, exhausted fatigue and locked ultimate states also passed.
- CoplayDev reported no connected Editor, so validation used installed Unity batch and the macOS player. Physical mobile was not tested. The player logged four stripped/unsupported URP postprocess shader warnings and no managed exceptions.

| Screen | Capture |
| --- | --- |
| PC 16:9 | [Korean](RiftEntryParityEvidence/entry-1600x900-ko.png) · [English](RiftEntryParityEvidence/entry-1600x900-en.png) |
| PC 16:10 / 21:9 | [16:10](RiftEntryParityEvidence/entry-1600x1000-ko.png) · [21:9](RiftEntryParityEvidence/entry-1680x720-en.png) |
| Phone portrait / landscape | [Portrait](RiftEntryParityEvidence/entry-440x956-ko.png) · [Landscape](RiftEntryParityEvidence/entry-956x440-en.png) |
| Linked windows | [Repeat Hunt](RiftEntryParityEvidence/repeat-1600x900-ko.png) · [Records](RiftEntryParityEvidence/records-1600x900-ko.png) · [Text log](RiftEntryParityEvidence/log-956x440-en.png) |

Evidence: [verification summary](RiftEntryParityEvidence/validation.json), [runtime checks](RiftEntryParityEvidence/runtime.txt), [Edit Mode XML](RiftEntryParityEvidence/editmode.xml), [source hashes](RiftEntryParityEvidence/source-sha256.json). The in-app browser rejected local-file navigation, so the current HTML was not rendered again in a browser. CSS/JS and available local images supplied the reference. Shared artwork/fonts/buttons and actual account state remain authoritative; pixel-identical HTML rendering is not claimed.

## 2026-09-30 New no-scroll layout

The window now follows the layout of the new [HTML prototype](../../Prototypes/RiftEntry/HELLSCRIPT-RiftEntry.html). In landscape and portrait the body never scrolls and all information and controls are visible on one screen.

| Orientation | Layout |
| --- | --- |
| Landscape | Three columns: (1) art, title, first-clear chest, tier selection and rule text; (2) fatigue and the three equipped potions, stacked; (3) four active skills, the ultimate bar, the sweep/training/repeat settings and combat records |
| Portrait | One column: art strip (title · tier selection · chest, rule text below) → fatigue → three potions → four skills plus the ultimate as a fifth cell → service buttons |

- **How it fits:** the shared window's body holds a single cell exactly as tall as the viewport, and the page is laid out inside it. If the content is taller, text, icons and gaps shrink together by one scale and the page is drawn again, so it never scrolls. Spare height becomes gaps between sections. When text is above 115% or the portrait width is narrow, the art strip puts the title and chest on one row and the tier card on the next. Single-line labels shrink to fit their cell.
- **Shared window kept:** the title bar, navigation area and fixed action area of `ContentWindowView` are used unchanged. Only three placements differ from the prototype:
  - The two entry buttons (normal and 1.5×) sit in the shared fixed action area across the bottom, not at the bottom of the right column, so the existing state buttons (checking settings, enter portal, restart rift) keep the same place. Each button has a name and a sub-line.
  - The readiness line (ready to enter / restore fatigue) and the bag-cleanup button are in the navigation area; the Abyssal Coin balance is in the title bar.
  - Sweep/training/repeat settings and combat records are existing features that the prototype does not show; they stay under the skills (one row below the skills in portrait). The saved-portal notice and the next-content goal stay with the rule text under the art.
- **Removed:** the “Prepare for the Rift” heading, the “Sanctuary › Rift Gate” breadcrumb, and the “normal entry 1× · fatigue also counts while paused” line (the help popup holds the same rule). The character and solo labels moved under the rift title.
- **Fatigue box:** remaining time, restores today, daily and paid gauge with legend, reset time and countdown, and the carry-over note appear in the prototype's order. Paid reserve is purple. At zero fatigue the restore button replaces the gauge.
- **Text:** 13 new lines were added to `en.txt` with their English, and 5 unused lines were removed.
- **Transactions and saves:** only the layout changed. Potion and skill changes, recovery, 1.5× entry and portal return/restart use the same `GameStore` transactions, and the button names (`rift-enter-normal`, `rift-enter-fast`, `rift-potion-*`, `rift-skill-*` and so on) are unchanged.

### Validation, 2026-09-30

- Unity Edit Mode, the eight suites the change touches: **169 tests passed** (`RiftEntryTests` 20, `LocalizationTests` 33, `StoredLocalizationTests` 19, `CombatJournalTests` 18, `SharedUiTests` 23, `UiButtonTests` 17, `RiftContentUnlockTests` 20, `RewardBoxTests` 19). The full Edit Mode suite was not re-run. `python3 tools/check_ui_contract.py` and `python3 tools/test_ui_contract.py` (11 tests) also pass.
- On a macOS development build (zero compile errors), **20 combinations** of 440×956, 956×440, 1600×900, 1600×1000 and 2100×900, Korean/English and 100%/150% text were checked. Each combination verified that the body needs no scrolling, every button lies inside the body without overlapping another, and no text spills its box, except cells whose text shrinks automatically (all passed). [Interactions](RiftEntryLayoutEvidence/runtime.txt) · [Portal](RiftEntryLayoutEvidence/portal-runtime.txt) · [Independent-process reload](RiftEntryLayoutEvidence/portal-restart.txt) · [Summary](RiftEntryLayoutEvidence/validation.json) · [Edit Mode result](RiftEntryLayoutEvidence/editmode.xml).
- The same build passed real interactions for the 1000-tier list, potion and skill changes, cancel/paid entry/fatigue recovery, and going to or restarting a saved portal (including button visibility in the 20 combinations).
- Screens: [portrait, Korean 100%](RiftEntryLayoutEvidence/entry-440x956-ko-100.png) · [portrait, English 150%](RiftEntryLayoutEvidence/entry-440x956-en-150.png) · [landscape, Korean 100%](RiftEntryLayoutEvidence/entry-956x440-ko-100.png) · [landscape, English 150%](RiftEntryLayoutEvidence/entry-956x440-en-150.png) · [1600×900](RiftEntryLayoutEvidence/entry-1600x900-ko-100.png) · [2100×900](RiftEntryLayoutEvidence/entry-2100x900-ko-100.png) · [portal, portrait](RiftEntryLayoutEvidence/portal-440-ko-100.png) · [portal, landscape English 150%](RiftEntryLayoutEvidence/portal-956-en-150.png).
- **Not done or only partly done:** no physical mobile device was used. Walking through town to the rift keeper passed, but the town interaction button was not active in this smoke fixture, so the window was opened through the same entry call instead of a real pointer click. Activating a skill preset while a portal exists was skipped because the preset tab was not reachable. Stale smoke assumptions (tutorial completion flag, skill trees, asynchronous admission wait, 1000-tier scroll distance) were fixed in this change. The scroll-retention check became a “fits without scrolling” check because the page no longer scrolls. The full Edit Mode suite and the runtime smoke batch run once after the last merge.

## 2026-09-29 portal return and re-entry

The battle HUD and observation menu use **Return through portal**. Confirmation saves the unfinished rift and returns to the sanctuary without finalizing it as a failure. Tutorial and training exit behavior is preserved.

When a saved rift exists, the entry window and existing sanctuary/rift-service menus show two actions.

| Action | Result |
| --- | --- |
| Enter portal | Continues the same rift ID and progress, preserving entry speed without charging admission again. |
| Restart rift | Starts a new rift at the selected stage at normal speed. The previous portal is replaced only after the new admission is saved. |

Closing or cancelling entry retains the portal. Admission checks, preparation or save failures retain the prior rift. Restart authorization is bound to the exact owned rift ID so stale requests cannot discard a newer portal. Rift time and participation fatigue do not advance while the hero is in the sanctuary.

`GameStore.SuspendRift` owns return persistence; re-entry reuses `CommitRiftEntry`. Potion preparation receives the same authorized replacement ID. [Skill-preset edits](Hunt_Edict_Quick_Presets.en.md) made while a portal remains also update the policy used when continuing that rift.

## 2026-10-02 return portal in town

Returning through a portal during a rift battle creates a smaller blue **Return portal**, 7m to the right of the town's golden rift. Selecting it makes the hero walk there; arrival alone does not resume combat. Within 2.8m, the existing town interaction card shows **Resume rift**. The button continues the saved battle directly, without opening the admission window.

The same run ID, simulation time, health, resource, position, enemies, cooldowns and entry speed are retained, with no second admission charge. Combat and fatigue usage stop in town. Only the selected hero's unfinished rift provides a portal; training, tutorials and terminal runs do not. Activation rechecks distance, town state and open windows. A failed admission save preserves the original checkpoint and portal for retry. Relaunching the game recreates the portal for the saved battle.

`TownLayout` owns position and reach, `GameController` and existing `CommitRiftEntry` own resumption, `WorldView.Town` owns the visuals, and existing `GameUI.Plaza` owns text and input. The implementation reuses the golden rift's meshes, shaders and animation, plus the shared theme, fonts, buttons and safe area. It adds no window, canvas or save schema.


The return card keeps its button at least 44 screen pixels tall on small screens and its world label at least 14 screen pixels high. This size floor stays within the town HUD adapter while retaining other residents' layouts.

### Town return portal verification, 2026-10-02

- Unity 6000.6.0f1: **88 of 89** directly affected Edit Mode tests passed. All town walking, Rift entry, portal recovery and NPC dialogue tests passed. One localization check failed because two unused translation lines already existed at baseline `e427a5be`. [Baseline comparison](RiftTownPortalEvidence/baseline-localization.json) and [test XML](RiftTownPortalEvidence/editmode.xml) preserve this result. The full Edit Mode suite was not run.
- Shared UI ownership and **11/11** contract tests passed; the macOS development build had zero errors. An isolated save exercised **10 combinations**: KO/EN at 440×956, 956×440, 1600×900, 1600×1000 and 1680×720, at default text size. Button height of at least 44 pixels, safe bounds and text clipping were checked, and captures inspected.
- Real frame-based movement and EventSystem raycasts/pointer input verified battle → portal return → town walking → resume button → the same battle → another return. Town waiting preserved time/fatigue; actions disappeared outside range; inventory blocked activation; other heroes, training, tutorials and terminal runs had no usable portal.
- Injected admission-save failure preserved the original checkpoint, disk and portal; retry succeeded. A separate process loaded and resumed the same battle with no second admission charge and the original 1.5× speed. [Initial process](RiftTownPortalEvidence/town-portal-runtime.txt) · [Reload](RiftTownPortalEvidence/town-portal-reload.txt) · [Summary](RiftTownPortalEvidence/validation.json) · [Source hashes](RiftTownPortalEvidence/source-sha256.json).
- Validation used a separate worktree's Unity batch process and macOS player to avoid disturbing other work in the original Editor. Physical mobile was not tested. Four existing URP postprocess shader warnings remain.

[PC Korean](RiftTownPortalEvidence/town-portal-1600x900-ko.png) · [Portrait Korean](RiftTownPortalEvidence/town-portal-440x956-ko.png) · [Landscape English](RiftTownPortalEvidence/town-portal-956x440-en.png) · [Resumed battle](RiftTownPortalEvidence/town-portal-resumed-battle.png).

## Portal and preset-save verification, 2026-09-29

**140 focused Unity Edit Mode tests**, shared UI ownership, **9 UI regression tests**, and a macOS Development build with zero errors passed. See the [validation summary](EdictPortalSaveEvidence/validation.json) and [test results](EdictPortalSaveEvidence/editmode.xml).

The macOS game used isolated saves and uGUI raycasts with synthetic pointer down/up/click events. Checks covered progress/cooldown preservation on portal return, actual preset activation and auto-save in town, closing entry without deleting the portal, and resuming the same rift in a fresh process. An injected admission write failure preserved the previous portal and disk; a successful retry left only the new rift. [Runtime checks](EdictPortalSaveEvidence/portal-runtime.txt) · [Independent-process resume/restart](EdictPortalSaveEvidence/portal-restart.txt).

**20 layout combinations** covered 440×956, 956×440, 1600×900, 1600×1000 and 2100×900, Korean/English and 100%/150% text. Both actions remained visible, enabled and within the frame. [Portrait](EdictPortalSaveEvidence/portal-440-ko-100.png) · [Landscape English 150%](EdictPortalSaveEvidence/portal-956-en-150.png) · [Saved preset](EdictPortalSaveEvidence/portal-preset-saved.png) · [Fresh rift](EdictPortalSaveEvidence/portal-restarted.png).

This is not a full-game suite pass. Four failures in the separately run legacy `CurrentBuildSaveTests` reproduced on unmodified main `b1989282`; see the [baseline evidence](EdictPortalSaveEvidence/baseline-legacy.json). Physical mobile was not tested. The injected save error and existing URP shader warnings remain in the original logs.

## Shared UI and state ownership

`RiftEntryWindow` starts from the new-content generator and opens `ContentWindowView` with `EquipmentViewSource.Owned`. Shared header, navigation, scrolling body, fixed actions, safe area and window lifecycle are preserved. The layout follows the [2026-09-30 new layout](#2026-09-30-new-no-scroll-layout), and the body does not scroll. Pickers use the same shell's optional maximum size.

`UiTheme`, `UiFonts`, `SkillIconView` and `PotionArt` remain the presentation owners. Sanctuary and Abyssal Coin images are existing assets. The 36 potion PNGs share the original assets registered by the jeweler; [provenance and hashes](RiftEntryEvidence/potion-provenance.json) preserve their review status. No new raster art was generated. The chest is drawn in code with the shared palette.

Account fatigue and restoration count belong to `RiftFatigue`. The saved run owns entry speed and attendance. Each hero owns `RiftEntryProgress`, potion inventory and loadout. Skill replacement reuses `ClassSkillTree`, `HuntEdictEditSession` and `CommitHuntEdict`. UI selection and scrolling are transient. Display refresh follows `GameStore.Committed`. The integrated save schema is 11. The existing local development adapter remains; production server time and payment enforcement are separate work.

## Fatigue and entry rules

- Following the prototype interpretation, daily fatigue resets to 120 minutes at midnight KST. Unused daily time does not accumulate; all heroes share the account allowance.
- Actual foreground rift attendance is charged in milliseconds independently of simulation speed. In-rift pauses and content windows count; town, training and background time do not. Return and death do not refund attendance.
- Daily time is consumed before the purple reserve. Restored time survives midnight in addition to the new daily grant. Attendance crossing midnight is split at the boundary.
- At zero total fatigue, entry is blocked and restoration is offered: 500 Abyssal Coins for 20 minutes, at most three times a day. Conditions and balance are revalidated at commit.
- Confirmed 1.5× entry saves the new run and one 200-coin debit together. Cancellation, insufficient balance and save failure do not debit. Saved-run resume does not charge again. Subsequent repeat entries use normal speed.
- Only faster successful completions replace each tier's best real-time record. Return, death and exhaustion cannot create a record. Records survive reset and restart; historical cleared tiers without a measurement do not receive invented times.

## In-place loadout changes

Changed 2026-10-01: each potion slot opens a scrolling list of owned legacy and crafted potions with icons, effects and quantities. Equipped potions remain selectable and show a check plus **Equipped · Slot number**. Selecting the current slot's potion clears it; selecting another slot's potion swaps the positions; an unequipped potion fills the chosen slot. A different grade of a family already assigned elsewhere exchanges that slot too, preserving family uniqueness. Selection spends neither stock nor gold.

`PotionLoadout.Select` and `GameStore.SelectPotionSlot` atomically persist the hero's actual three-slot loadout. A failed write preserves memory, disk and stock and shows an error in the popup. Existing explicit `SetPotionSlot` assignment semantics remain unchanged. A saved portal no longer blocks preparation in the sanctuary: the transaction verifies the expected portal ID and suspended state. Relaunch and portal resume use the saved slots while retaining run progress and potion cooldowns.

Equipped skills retain their existing replacement popup. Empty regular and unlocked ultimate slots show **+**, opening the shared Hunt Edict **skill tree**. The entry window stays in the common window stack, so Close/Back returns to the same stage and displays saved skill changes. Recommended mode allows point allocation and skill equipment without disabling recommended mode or unlocking custom combat policies. Existing `CommitHuntEdict` also updates the saved portal's skill loadout.

The three-second autosave notification no longer rebuilds an unchanged entry screen. Only changed presentation state triggers a repaint, deferred until child windows close. Fatigue/countdown labels update in place, keeping idle button and focus instances stable.

## Shared jeweler inventory and effects

Rift preparation reads `GemElixirs` and the actual `PE-G01-1` potion IDs used by manufacturing. Names, tiers, effects, stock and icons are not duplicated. The eight legacy potions and 36 crafted gem elixirs retain their owners. A potion manufactured at the jeweler can be equipped immediately in the rift screen without consuming stock; combat use consumes it.

Stats, cooldowns, shields and automatic use follow `HeroStats.Elixirs`, `CombatSimulation.Potions` and `PotionLoadout`. Diamond tiers use shields of 5, 8, 12, 17, 23 and 30 percent of maximum HP without stacking the same barrier. The inventory's shared fallback policy remains active. See the owning [jeweler implementation](Jeweler_Runtime.en.md).

## First-clear chest and 1–1000 list

The chest opens a continuous list with only each tier and an empty reward-icon slot. It starts near the selected tier and reaches 1000. A bounded row pool renders the visible portion instead of instantiating 1000 UI objects.

The chest is closed before a clear, periodically shakes after a clear while unclaimed, and remains open for a claimed receipt. Historical `firstClears` also establish clear eligibility. **The new reward catalog and grant transaction are intentionally empty.** There is no invented claim button or payout. Once rewards are defined, a `GameStore` transaction must validate eligibility and duplicate claims, grant rewards and persist `claimed` atomically. Existing automatic first-clear gold/material rewards are unchanged and separate. Claimed appearance and persistence were tested with an explicit isolated fixture.

## Validation

Unity Edit Mode passed 232 integration tests and 147 follow-up save-compatibility tests. These suites overlap and are not summed. The 9 shared-UI tests, macOS build (zero errors), 29 native pointer/layout checks and independent-process restart also passed.

See the [validation report](RiftEntryEvidence/validation.json), [native interactions](RiftEntryEvidence/runtime.txt) and [independent-process reload](RiftEntryEvidence/restart.txt). Native macOS acceptance uses real town movement and uGUI raycasts with pointer down/up/click. Twenty layouts cover 440×956, 956×440, 1600×900, 1600×1000 and 2100×900, Korean/English and 100/150-percent text.

[Portrait](RiftEntryLayoutEvidence/entry-440x956-ko-100.png) · [Landscape, English, 150%](RiftEntryLayoutEvidence/entry-956x440-en-150.png) · [Tier 1000](RiftEntryEvidence/rewards-tier-1000.png) · [Warding](RiftEntryEvidence/diamond-potion.png) · [Skill detail](RiftEntryEvidence/skill-detail.png) · [Paid fatigue](RiftEntryEvidence/paid-fatigue.png) · [Open chest](RiftEntryEvidence/claimed-chest.png).

Checks use an isolated worktree and save, not the user's main Editor or account. Physical mobile validation was not performed. URP post-processing shader warnings in the player log are recorded separately from successful UI interaction.

## Entry-control verification, 2026-10-01

**138/138 targeted Edit Mode tests**, shared UI ownership plus 11 validator tests, and a macOS development build with zero build errors passed. Native raycast-checked synthetic pointer input covered Korean/English at 440×956, 956×440 and PC 16:9/16:10/21:9, empty-slot navigation/return and the owned/equipped potion list. Button instances stayed stable across two autosaves. Potion selection/clear/swap disk state, recommended-mode skill saving, an independent process restart and actual portal resume were verified. Physical mobile devices were not tested.

After macOS adjusted the 21:9 English window, the remaining case continued at 1680×720. The recommended-mode skill footer gate was corrected and only the affected save/return flow was repeated. Unchanged successful coverage was reused. The [validation record](RiftEntryControlsEvidence/validation.json), [138-test results](RiftEntryControlsEvidence/editmode.xml) and [restart result](RiftEntryControlsEvidence/restart.txt) retain exact scope and intermediate failures.

[PC entry](RiftEntryControlsEvidence/entry-pc-ko.png) · [Portrait potion list](RiftEntryControlsEvidence/potions-portrait-ko.png) · [English landscape list](RiftEntryControlsEvidence/potions-landscape-en.png) · [Return after saving skills](RiftEntryControlsEvidence/saved-entry-portrait-ko.png)

## Next content hint, 2026-10-02

The art panel's long instructions and portal note are replaced by **Next Content / Rift tier / feature name**. `ContentUnlocks.NextLocked` supplies the actual account progression. The circular question button reuses `UiIconButton` and the existing Rune Board help artwork.

Pressing it opens the existing **First-clear Rewards** popup and passes that unlock tier to `RiftRewardList`, which scrolls to and highlights the tier. It does not change the selected challenge tier or saved data; closing returns to the entry screen. Once every feature is unlocked, only the completion hint is shown.

KO/EN across five viewports verified display, clipping, actual pointer input and full visibility of the target tier. Closing preserved the selected challenge tier. [Combined validation](Rift_Result_Actions.en.md) · [PC](RiftResultActionsEvidence/dps-next-content-1600x900-ko.png) · [Portrait English](RiftResultActionsEvidence/dps-next-content-440x956-en.png) · [Scrolled reward list](RiftResultActionsEvidence/dps-next-content-rewards-ko.png).
