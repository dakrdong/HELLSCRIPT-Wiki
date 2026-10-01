# Power-saving auto hunt

Updated: 2026-10-01
Korean: [자동 사냥 절전 모드](Idle_Display.md)

## Display and interaction

An upper-center `Power saving` button appears during an ordinary automatic rift hunt. It immediately opens a black dashboard. Training, tutorials, paused combat, portals and recovery blockers cannot enter this mode.

`Hunt elapsed time` shows monotonic wall time since this entry as `HH:MM:SS`, including inspection time and without wrapping after 24 hours. The status distinguishes paused inspection from active hunting.

The line below the timer shows current location and activity. Objective rifts read actual completed/total counts, such as `Rift Lv 15 : Progress 1 / 3`, and identify seals, essence carriers or offerings. Other rifts show the kill meter; boss and loot phases show their actual activity. The status also distinguishes return-portal inventory/storage blockers, potion replenishment, the next rift and entry countdown, inspection and save-failure pauses. Cleanup is an immediate portal/result transaction, so the display does not invent a walk through town. Completion is shown only with a committed cleanup report.

A fixed bottom row shows the current character's face seal, level and XP bar. It shares class portraits and seal construction with `GlobalHudView`, and growth rules with `GlobalHudSnapshot.ReadGrowth`. Owned level and XP refresh once per second, including level-ups and the actual level cap. Portrait places growth above the unlock area; landscape places it at the bottom left so each log can show at least one complete row. Growth, lock/gauge and the bottom-center exit control do not overlap.

- Warehouse: free slots / total capacity across every unlocked warehouse tab. Open storage launches the actual `StorageWindow`.
- Equipment changes: committed change time, equipment position and before → after slots. Shared `EquipmentSlotView` controls open immutable historical copies through `ItemDetailView`. Later disposal, movement or enhancement cannot alter the captured item. Open inventory launches the actual `InventoryWindow`.
- Equipment acquired: icon × count for magic, rare, legendary, set and unique. Pre-entry ownership and unclaimed floor drops are excluded; later disposal does not reduce acquired counts. Within the existing rarity-3 data, a definition with a set ID counts as set, another named unique as unique, and unnamed rarity-3 equipment as legendary. No drop probabilities or item rules change.
- Rift attempts: every current/subsequent attempt retains its level, local start timestamp, actual elapsed run time and running/cleared/failed outcome. An in-progress rift uses its existing journal start timestamp. Duration is the committed `run.realTime` and freezes at completion.

Equipment and attempt logs scroll independently. Portrait stacks sections; landscape uses equal-width columns with warehouse and equipment changes on the left, acquired equipment and attempts on the right. A centered vertical divider separates only the body, leaving the shared timer/status and growth/unlock footer clear. Every session entry is retained; only visible rows create UI objects. The bottom exit control remains outside the scroll areas.

Since 2026-10-01, `Hold to exit power saving` charges only while pressed. Filling an empty gauge takes three seconds. Releasing or leaving the button drains it at the same rate; pressing again continues from the remaining progress. The lock shakes while held, and both lock and gauge hide when empty. A short tap followed by waiting never unlocks. A second pointer cannot steal the hold. Opening details, rebuilding the layout or losing focus clears the hold and progress. `IdleUnlockHold` forwards pointer lifecycle events; `IdleDisplaySession` owns the monotonic gauge. The lock reuses `StorageGlyph`.

## Combat, persistence and pause ownership

The existing `ForegroundCombatClock` drives the same `CombatSimulation` at its 0.05-second fixed step. Reduced presentation frequency does not introduce another combat, RNG or reward calculation. Existing `GameStore` and `RepeatHunt` paths still own result commits, cleanup and the next attempt.

`IdleHuntJournal` captures an entry baseline and observes equipment, claimed drops and outcomes only after `GameStore.Committed`. Failed saves and repeated notifications do not create successful records. This is a foreground session journal, reset on re-entry; it has no separate permanent archive. Actual equipment and run checkpoints retain their existing persistence.

Storage, inventory and historical detail use `ContentWindowHost` nested pause leases. Combat, fatigue consumption, automated cleanup and next-rift delay stop during inspection. Closing the last detail restores the previous pause state without clearing unrelated manual/error blockers. Opening a detail clears the held pointer and unlock progress.

Application suspension preserves the checkpoint and exits power saving. Returning does not automatically resume combat or convert suspended time into combat catch-up. Existing offline-supply settlement remains independent.

## Power-saving behavior and shared UI

Density update, 2026-09-28: change time and equipment position share one line. Shared item slots use 40 units with a 6-unit gap, reducing 100% row height from 78 to 46. Both historical item-detail actions remain. See the [current UI rules](Responsive_Hud_20260928.en.md).

The mode disables world objects, the battle camera, transient effects and normal HUD updates, in addition to showing a black background. One empty display camera clears black so the Editor does not overlay `Display 1 / No cameras rendering`. It renders no world layers and disables shadows, post-processing, HDR, MSAA and depth/color texture requests. Exit disables it; re-entry reuses it. Repeated-rift presentation objects are deferred until reveal. Audio playback, queued effects and combat audio observation stop; exit seeds the observer at current events instead of replaying old effects.

At rest, the update target is 20 Hz and rendering interval is 20 frames, approximately one presentation per second. Summary text refreshes once per second. Scrolling, inspection and charging or draining the unlock gauge temporarily request 30 Hz presentation. Exit restores the previous frame target, v-sync, rendering interval and sleep policy. Screen sleep stays disabled to preserve real foreground combat. Global device brightness is unchanged.

The existing `GameUI.Idle` canvas owns this HUD adapter. Its background does not acquire a content-window pause lease because combat must continue. Shared theme, fonts, safe area and equipment slots remain authoritative. The new `IdleEquipmentDetailWindow` entry was generated with `tools/new_content_ui.py`, opens `ContentWindowView` and explicitly passes `RewardSnapshot` data.

## Hold-to-exit and black-background validation (2026-10-01)

**31/31 focused Edit Mode tests** passed (six new gauge tests and 25 existing combat-clock tests), along with the shared UI validator and **11/11 validator tests**. After integrating the code into `main`, the final runtime check ran once using the same source in a Unity 6000.6.0f1 macOS player. Ten combinations cover Korean/English at the default text size in 440×956, 956×440, 1600×900, 1600×1000 and 2100×900 viewports.

Live EventSystem raycasts and pointer events verified tap-and-wait, partial charge, release drain, resume from remaining charge, pointer exit, ignoring a second pointer, and cancellation on focus loss, layout rebuild or inspection. Only a three-second completed hold exited; previous frame/render policy, battle camera and audio were restored. Re-entry retained exactly one empty display camera with no world layers, shadows, post-processing, HDR, MSAA or depth/color texture requests. Over about 2.217 seconds in power saving, combat advanced 2.200 seconds with zero world/normal-HUD updates. These counters do not measure battery savings.

CoplayDev MCP verified the current project path, compilation after importing the new script, and the loaded `IdleUnlockHold` type. Console retained unrelated Unity AI generation-service `NoSubscription` errors; no compile errors from this change remained. The Editor Game View overlay was not directly recaptured; the native player verified the active empty display camera and black screen. Physical-mobile input/battery testing and new WebGL/APK builds were outside this validation scope.

- [Summary](IdleHoldUnlockEvidence/validation.json), [Edit Mode report](IdleHoldUnlockEvidence/editmode.xml), [runtime](IdleHoldUnlockEvidence/runtime.txt), [source hashes](IdleHoldUnlockEvidence/source-hashes.json).
- [Holding](IdleHoldUnlockEvidence/unlock-holding-ko.png), [released/draining](IdleHoldUnlockEvidence/unlock-released-ko.png), [portrait](IdleHoldUnlockEvidence/idle-440x956-ko.png), [English landscape](IdleHoldUnlockEvidence/idle-956x440-en.png).

The sections below retain historical validation of earlier implementations, including the former text-size matrix.

## Location and growth validation

**94/94 focused Edit Mode tests** and **9/9 UI contract tests** passed. Thirteen new status/growth tests cover actual objective counts, kill meter, boss/loot/portal phases, inventory/storage/potion/save blockers, repeat entry and level caps. Missing English entries for the existing cleared/failed outcome labels were also added.

The native macOS player passed twenty combinations of five viewports, Korean/English and 100%/150% text. A level-39 hero at 50% XP reached level 40 through combat growth; the level and XP display updated without forcing a refresh. Checks cover every status variant's text height, separate growth/unlock bounds and at least one fully visible row in each log. Visual review moved growth to the lower left in landscape to retain readable rows. Pointer open/close/scroll, repeated rifts, unlock, failed-save recovery and a fresh-process restart passed again. CoplayDev MCP reported no connected instances, so validation used the existing batch runner and native development player.

- [Validation summary](IdleDisplayStatusEvidence/validation.json), [Edit Mode report](IdleDisplayStatusEvidence/editmode.xml), [runtime](IdleDisplayStatusEvidence/runtime.txt), [restart](IdleDisplayStatusEvidence/restart.txt), [source hashes](IdleDisplayStatusEvidence/source-hashes.json).
- [Level and XP](IdleDisplayStatusEvidence/growth-live-ko.png), [level-up](IdleDisplayStatusEvidence/growth-level-up-ko.png), [English landscape](IdleDisplayStatusEvidence/idle-956x440-en.png), [unlock gauge](IdleDisplayStatusEvidence/unlock-countdown-ko.png).

## Original power-saving validation

Unity 6000.6.0f1 on an Apple M3 Pro passed **148/148 focused Edit Mode tests**, **9/9 UI contract tests**, native interaction and fresh-process restart checks. Twenty combinations cover five viewports, Korean/English and 100%/150% text. Checks include text height, safe control bounds, pointer open/close, scrolling, historical gear detail, saved failure/repeat transitions, the three-second gauge, cancellation by inspection, injected save failure and recovery.

The same build's normal two-second observation updated the world and HUD. About 2.2 seconds in power saving advanced combat by the same elapsed duration with zero world/normal-HUD updates, disabled camera and stopped audio. Exact counters are in the [runtime results](IdleDisplayEvidence/runtime.txt). These observations do not measure battery savings or CPU/GPU busy time.

- [Validation summary](IdleDisplayEvidence/validation.json), [Edit Mode results](IdleDisplayEvidence/editmode.xml), [restart results](IdleDisplayEvidence/restart.txt), [tested source hashes](IdleDisplayEvidence/source-hashes.json).
- [Portrait](IdleDisplayEvidence/idle-440x956-ko.png), [English landscape](IdleDisplayEvidence/idle-956x440-en.png), [unlock gauge](IdleDisplayEvidence/unlock-countdown-ko.png).

The initial shared-UI test still expected 25 groups, while prerequisite auto-equipment code already exposed 35. That stale expectation was corrected. The initial native harness was also corrected to await asynchronous rift admission before asserting that combat had begun.

The initial work was on `codex/power-saving-hunt` and is now included in the [September 28 integration](All_Work_Integration_20260928.en.md). Public deployment uses merged main. Physical-mobile battery, thermal, touch and OS-lock behavior remain unverified.
