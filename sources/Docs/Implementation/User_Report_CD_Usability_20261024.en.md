# Gear guidance and usability fixes (2026-10-24)

[한국어](User_Report_CD_Usability_20261024.md)

Implements U-C (reports 4,5) and U-D (3,6,7,10) without changing saved keys or shared equipment comparison calculations.

- Explain and highlight the shared equipment score before equipping the recommended helmet.
- Require an explicit better-score choice and successful Save in the lesson, even when that preference was already stored. Only the detached lesson draft starts on the all-gear choice. Cancel and failed saves do not advance the lesson. Explain afterwards that lower-score gear still enters the bag.
- Draw map boundaries with a dark outer stroke and light center under the same exploration mask and existing invalidation conditions. No per-frame search or new timer.
- Remove a purchased shop row immediately after a successful transaction; retain the owned item, purchase record and buyback rules.
- Keep daily quest cards at least 104px tall, scrolling only overflowing portrait bodies. Existing eligibility conditions remain unchanged.
- Put an equipped-weapon badge in a 16px row above the rune tabs, independently of the board being viewed. Existing tab and text sizes stay unchanged; the portrait stage gives up 16px while the window height stays fixed. An equipment commit refreshes the badge without reloading a dirty rune draft. Unrelated saves do not redraw it.

## Evidence

Daily quest baseline: both short (688px) Korean/English cases failed while both 876px cases passed. The fix passed 53 related tests, 15 UI-contract tests and the UI-refresh guard.

The C/D focused run initially passed 162 of 167 tests. Five new-test failures concerned Edit Mode reflection/deferred object disposal and the intentional rune stage-height expectation. Corrected tests plus the gear-window fixture passed all 11 retests. Unchanged passing inputs reuse their original proof.

Checks cover stored preferences and manual selection/save, cancel and failure draft preservation, actual shop purchase action and retained inventory, Korean/English badge placement and selection independence, equipment-save refresh with rune draft/camera preservation, and map contrast/fog coordinates without simulation mutation.

Local evidence: `Builds/Validation/UserReports20261024/takeover/{repro,daily-fix,c-d-focused,c-d-retest}/`. Unity 6000.6.0f1; explicit fixture filters only. No full Edit Mode run. GameStore fixtures use disposable directories. The first framework run also wrote a side XML under the default company path; that fact is recorded separately. Later runs use a temporary worktree-only company identity, restored before commit. No actual player save was read or cleaned.

These Edit Mode checks do not establish native pointer acceptance or device performance. Final combined review, local equivalent CI and runtime acceptance need their own completion evidence.

Final combined checks with main PR186 (1f1887d1) and an explicit rune visibleState key passed all 223 tests. Evidence: takeover/cd-latest-main/results.xml (2026-10-24 15:02:46–15:07:17 UTC).
