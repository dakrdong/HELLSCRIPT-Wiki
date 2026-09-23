# Town rift entry and first-clear rewards

Date: 2026-09-23 · [한국어](Rift_Entry.md)

The town rift interaction opens the approved preparation layout with actual hero state and domain transactions. Existing sweep, training and repeat settings remain accessible through rift services.

## Shared UI and state ownership

`RiftEntryWindow` starts from the new-content generator and opens `ContentWindowView` with `EquipmentViewSource.Owned`. Shared header, navigation, scrolling body, fixed actions, safe area and window lifecycle are preserved. Portrait stacks the illustration above preparation; landscape fixes it on the left and independently scrolls preparation on the right. Pickers use the same shell's optional maximum size.

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

[Portrait](RiftEntryEvidence/entry-440-ko-100.png) · [Landscape, English, 150%](RiftEntryEvidence/entry-956-en-150.png) · [Tier 1000](RiftEntryEvidence/rewards-tier-1000.png) · [Warding](RiftEntryEvidence/diamond-potion.png) · [Skill detail](RiftEntryEvidence/skill-detail.png) · [Paid fatigue](RiftEntryEvidence/paid-fatigue.png) · [Open chest](RiftEntryEvidence/claimed-chest.png).

Checks use an isolated worktree and save, not the user's main Editor or account. Physical mobile validation was not performed. URP post-processing shader warnings in the player log are recorded separately from successful UI interaction.
