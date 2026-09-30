# Town rift entry and first-clear rewards

Date: 2026-09-23 · [한국어](Rift_Entry.md)

The town rift interaction opens the approved preparation layout with actual hero state and domain transactions. Existing sweep, training and repeat settings remain accessible through rift services.

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

Each potion slot opens six family tabs and six grades per family, with original icons, effects and owned quantities. An owned grade equips immediately without consuming stock. Families used by another slot disable the entire tab at every grade, and the domain transaction enforces the restriction. The inventory picker follows the same family rule. Older saves with different grades of the same family keep the first slot and clear later duplicates without losing owned stock.

Active skill candidates are actually unlocked skills, excluding other equipped actives. Selecting a candidate shows details; **Replace** commits through Hunt Edict. A separate ultimate slot only offers ultimate candidates. Unlocked passives apply automatically and are omitted. Changes are blocked while a saved rift is in progress.

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
