# All-branch main integration — 2026-09-25

Updated: 2026-09-25 · [한국어](Main_Integration_20260925.md)

The starting inventory covered 27 local branches, 17 worktrees and remote branch tips. Of the 26 development branches excluding `main`, 24 were already ancestors of `main`. The remaining `codex/balance-1000-analysis` and its descendant `codex/idle-offline-rewards` were integrated with current `main`, retaining attendance artwork and Hunt Edict quick presets.

Starting commits were `16678cce` for main, `3b8f2726` for balance and `47809c65` for idle/offline rewards. No branch deletion or history rewriting is required. Every captured target tip is checked for ancestry. [Inventory and validation](MainIntegration20260925Evidence/validation.json) records the scope and results.

## Conflict resolution and preservation

- English localization retains both quick presets and Offline Supplies.
- Wiki generation includes both attendance artwork and reward-box artwork/databases.
- Korean and English shared-UI histories preserve every distinct body hash from both parents and remove duplicate bodies.
- The original main checkout's 408 uncommitted texture-import metadata files were recorded by content hash and patch. They do not overlap integration paths and are neither reverted nor included in feature commits.
- Import/project settings rewritten by isolated Unity validation and regenerated historical diagnostics are excluded from the integrated source. No user save is used.

## Integration verification

Hashes confirmed 1,523 matching code, data and image files between the test checkout and native build snapshot. See the [validation record](MainIntegration20260925Evidence/validation.json), [full Edit Mode report](MainIntegration20260925Evidence/editmode.xml), [first repair report](MainIntegration20260925Evidence/repair-regression.xml) and [native execution record](MainIntegration20260925Evidence/native-results.json) for exact results and scopes. Overlapping test runs are not added together.

The initial full Edit Mode run passed 3,851 of 3,873 tests with 22 failures. Failures were separated into real defects and fixtures that still assumed older content gates. This is not recorded as an entirely passing full run; repaired regression results are preserved separately. The first repaired batch passed 350 of 351 tests. After repairing the remaining fallback chest-space issue, all [18 map regression tests](MainIntegration20260925Evidence/map-repair.xml) passed, including 90 generation combinations (six fallback layouts × five reward seeds × R1/R5/R30) and passage checks for 12 archetype audit seeds. Every initial failure is covered by a subsequent passing check; the entire 3,873-test suite was not rerun after repair.

- A reconnect with no currency or fractional accrual saves normalization/recovery and the cursor without adding an empty financial receipt. Failed writes retain the exact pending interval; actual rewards and fractions still use the atomic transaction path.
- Introductory layouts reserve a guard pack at least two rooms from entry within the four-elite budget. Chest/shrine candidates must leave large-enemy passages clear, and final traversal validation remains enabled. When scaled authored positions are unavailable, in-room supplementary candidates pass the same safety checks.
- Attendance swipe setup uses explicit Unity object null checks. Duplicate and obsolete translations were removed, and spin movement choices were reconciled with the design catalog.
- Fixtures now establish the R60 gem-elixir and R15 rune gates. Gems acquired before R10 remain owned without opening the gem service early.

A new Warrior with starter gear completed R1→R2 at actual 1x speed in 235.90 seconds with zero failures, advancing from level 1 to 6 and earning 4,737 gold and 26 enhancement stones. An injected one-hour offline interval based on the final clear granted 1,763 gold and 19 stones; reopening the receipt and reloading the save did not grant twice. This does not claim a real one-hour absence or an OS clock change.

The old reward-box runtime check still expected the former R10 legendary reward. The game paid the correct current rewards; the check was updated. It now verifies five non-legendary boxes at R10 and actual receipt/opening of the legendary weapon box at R30. The [earlier report](RewardBoxEvidence/Reward_Box_Verification.md) remains historical evidence and is explicitly separated from current rules.

The attendance runtime check also retained an old scroll object after an autosave repainted the shared window. It now reads the current view and checks that an automatic town heartbeat preserves its page and scroll position. Both adjustments are limited to acceptance code; reward and presentation behavior are unchanged.

Native macOS flows cover idle/supplies, reward boxes, content gates, quick presets/restart and attendance/restart. Input uses uGUI raycasts and pointer events with persisted before/after assertions. Portrait, landscape and PC aspect ratios, Korean/English and 100%/150% text are included. This is not physical-mobile validation.

## Runtime and publication boundaries

Integration includes introductory R1–5 difficulty and map size, R25 unidentified equipment purchases, R30 legendary/set rift rewards, R35 option rerolling, reward boxes/first clears, real idle hunting and Offline Supplies. Long-range R1–1000 enemy/drop tables explicitly marked as proposals are not activated simply by merging them. The server verification module is not presented as deployed authentication, trusted time, database or network security.

The [public wiki](https://dakrdong.github.io/HELLSCRIPT-Wiki/#/tree) is generated from merged main and mirrors all documents, histories, databases, images and source evidence. Date-only presentation and read-only access remain required. Publication is verified through Pages completion and an unauthenticated in-app browser.

Evidence screenshots: [portrait supplies](MainIntegration20260925Evidence/supplies-440x956-ko-150.png), [landscape supplies](MainIntegration20260925Evidence/supplies-956x440-en-150.png), [R30 first-clear reward](MainIntegration20260925Evidence/first-clear-30.png).
