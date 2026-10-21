# Plaza, attendance and dock motion polish — 2026-10-19

[한국어](Casual_Juice_Plaza_Attendance_Dock_20261019.md)

Basis: on the merged `main` (`8e57a374`), three restrained motions were added to the plaza, dock and attendance screens settled by #144 and #146. Colours, fonts, text, save keys, transactions and layout rules are unchanged. The confirmation card and toast are covered by [their own note](Casual_Notice_Juice_20261019.en.md).

## What changed

- **Plaza interaction card**: the card rises and fades in when the hero gets close to a resident or facility (`CasualNoticeEntrance`, .28 s) and once more when it moves on to another service. Reflow hands the position and scale over through `SetRestingPosition`/`SetRestingScale`, so the clock is never cut (the shared part's rule).
- **Attendance claim stamp**: the `Claimed` seal of a day you just collected stamps down large and settles (`UiPopIn`, .32 s, a small bounce). Collect-all staggers the days by .07 s (up to 8 steps). If the window repaints right after the press, the stamp continues from its clock instead of restarting; days claimed earlier and the other tab (weekly/monthly) never replay it.
- **Content dock**: the shortcuts decelerate into place when they unfold and the arrow overshoots slightly before it settles (`ContentDockView.Decelerate`, `UiPopIn.EaseOutBack`). The unfold order, its .22 s duration and the `Settled` check are unchanged.
- The new `UiPopIn` plays once and sets `enabled=false` when finished, so no per-frame cost remains (`AgentRules/CPU_Performance.md`). It uses unscaled time and, in the Editor, draws only the final state.

## Deliberately not done

- **Pop on the dock `!` badge**: `ReflowEventButton` re-places the badge and its pivot every frame, so a scale motion jumps. Changing that placement rule needs its own decision.
- **Count-up of reward amounts, plaza label pops and close animations**: each touches a different refresh path (post-save refresh, world label placement, `Close`) and goes beyond this scope.
- An opening motion for the whole attendance window would change the shared `ContentWindowView`, so it was left out.

## Verification

- 72 focused EditMode tests pass: `UiPopInTests` (7 new), `TownPlazaLayoutTests` (19), `AttendanceSkinTests`, `AttendanceTests`, `AttendanceArtTests`, `CasualNoticeLayoutTests` (15). The new tests check both ends and the overshoot of the bounce curve, the start size and transparency, landing and `enabled=false`, a delayed stamp, continuing in a repainted window, keeping the original scale on restart, no playback in the Editor, and the dock's deceleration curve.
- Static UI contract (15) and refresh guard (14 bindings) pass.
- Not verified: how the motion looks and feels while playing (the Editor has no frame clock), the development-player smokes (`RuntimePlazaSmoke`, `RuntimeContentDockSmoke`, `RuntimeAttendanceSmoke`), a macOS build, physical devices and touch. The full EditMode suite was not run.
