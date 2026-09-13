# Idle display and peek mode

Date: 2026-09-13 · [한국어](Idle_Display.md)

Status: the desktop implementation checkpoint passed all 2,471 tests, final native initial/restart checks, 13 screenshot reviews and five same-source profile captures. Android devices and platform-specific idle policies remain unfinished.

## Presentation and hunting

The [idle specification](../Design/HELLSCRIPT_Idle_Mode_Detail.md) and [screen layout rules](../Design/HELLSCRIPT_Screen_Layout_Detail.md) guide three display states: normal, dimmed and peek. The entry button is available during an actual rift, excluding training, manual pause, portal cleanup, editing and confirmation dialogs. The first explanation displays the real repeat policy. Acceptance is stored as a separate device preference, never as account progression. Start and cancel remain in the footer on short landscape screens.

The same `CombatSimulation` and `RunState` keep running. The dimmed state disables the battle camera and world presentation, skips normal HUD/minimap updates and suppresses transient impact creation. Its summary updates approximately once a second with priority for interruption reasons. The small summary shifts position every 60 seconds. Repeated rifts create their simulation data immediately but defer presentation objects until requested.

Peek aligns the hero, visible enemies and camera to the current completed tick. Live projectiles, ground effects and chest states reflect their current state; expired impacts are not replayed. Peek targets 30 FPS and dims ten seconds after the last input. Keep watching or opening a menu cancels that timer and restores the prior frame target, v-sync, rendering interval and sleep policy. Global device brightness is not modified.

## Time, repetition and suspension

A monotonic clock accumulates exact 1/20-second intervals for the existing 0.05-second simulation tick. Foreground elapsed time is no longer truncated to 0.25 seconds per frame, and display transitions retain the fractional tick. Each update has a 20-tick budget. More than 0.5 real seconds of pending work sustained for three seconds preserves the last completed tick and requires explicit resume.

Repeat hunting depends on committed results rather than the visibility of the result page. Existing delays, stop conditions, cleanup and reward commits remain authoritative. Stop after this rift finishes the current rift and its settlement before stopping repetition. Save failures or insufficient bag space remain visible on the dimmed summary.

Application suspension saves and pauses combat. Returning restores the normal display and requires Resume; suspended time is not converted into catch-up combat ticks. The local development adapter retains account-wide offline gold/materials and the twelve-hour cap. Rewards and their time cursor commit together. A failed interval remains fixed, and subsequent saves or equipment transactions retry it first. Display-mode transitions themselves create no offline income. Online account ownership and server settlement remain separate work.

Public gameplay stays at 1×. Internal checks exercise 1.5×/2× without unlocking those speeds for players.

## Current evidence and unfinished work

Thirty focused timing, display-state and settlement cases passed. Each of three classes, two builds and three speeds underwent 1,000 display-state transitions and was compared with a fixed-tick reference. Complete combat state matched, including actors, action phases, projectiles, RNG and rewards. Checks also cover temporary and persistent lag, pauses and suspension, failed settlement, retry/restart and independent preference-file preservation.

Captures used Apple M3 Pro (Mac15,7), Unity 6000.6.0f1, Mono Development Build, PC quality, 1280×720, seed 93171, stage-30 Mage chain control and seeded legal gear. A three-second warmup preceded twenty-second windows; peek used eight seconds before automatic dimming. Scheduled frames are Unity render scheduling observations, not measured GPU submissions or panel power. Zero world/effect/HUD counts exclude the minimal idle summary. Main Thread includes frame-cap waits, and Draw Calls/Batches are zero even in visible captures; neither supports CPU busy-time or GPU savings claims.

| Capture | Real seconds | Combat ticks | World / effects / normal HUD | Scheduled frames | Raw report |
|---|---:|---:|---|---:|---|
| Baseline normal 1 | 20.015 | 401 | 1193 / 174 / 1193 | — | [JSON](../../Artifacts/Validation/IdleMode/Normal1/profile.json) |
| Baseline normal 2 | 20.011 | 401 | 1193 / 174 / 1193 | — | [JSON](../../Artifacts/Validation/IdleMode/Normal2/profile.json) |
| Baseline normal 3 | 20.015 | 401 | 1195 / 174 / 1195 | — | [JSON](../../Artifacts/Validation/IdleMode/Normal3/profile.json) |
| Final normal | 20.015 | 400 | 1188 / 174 / 1188 | 1188 | [JSON](../../Artifacts/Validation/IdleMode/AfterNormal1/profile.json) |
| Final dimmed 1 | 20.006 | 400 | 0 / 0 / 0 | 21 | [JSON](../../Artifacts/Validation/IdleMode/Dim2/profile.json) |
| Final dimmed 2 | 20.042 | 401 | 0 / 0 / 0 | 21 | [JSON](../../Artifacts/Validation/IdleMode/Dim3/profile.json) |
| Final dimmed 3 | 20.011 | 401 | 0 / 0 / 0 | 21 | [JSON](../../Artifacts/Validation/IdleMode/Dim4/profile.json) |
| Final peek | 8.026 | 161 | 240 / 75 / 240 | 240 | [JSON](../../Artifacts/Validation/IdleMode/Peek1/profile.json) |

All final normal/dimmed/peek captures use Build10. Baseline clock boundary handling differs, so direct presentation comparisons use the same final source. Raw captures retain that distinction.

Final native checks invoke actual UI events for the first explanation, ten-second dimming, keep watching, menu exit, repeat stop, save failure, foreground return and restart. Repeat boundaries use the real abandon/result path; suspension callbacks and save errors are explicitly injected. These fixtures do not prove natural victory, physical touch or OS locking. Restart uses the existing town Continue Rift control to resume the preserved checkpoint.

The full suite and final build differ only in the opt-in native restart harness. Its incorrect second pause/resume expectation was replaced with the existing explicit Continue Rift control and checkpoint assertions; final Native8 passes. Gameplay, UI, clock, localization and Edit Mode test sources are identical.

These are desktop presentation measurements, not mobile battery savings or completion of mobile idle support. Android power/thermal/long-session tests, physical touch/lock/minimize events, app-window brightness, low-battery/thermal interruption, and actual audio/haptics policies remain unfinished. Device-specific p95 reveal-latency targets are not yet established.

Owners: [combat clock](../../Assets/HELLSCRIPT/Runtime/Core/ForegroundCombatClock.cs), [display controller](../../Assets/HELLSCRIPT/Runtime/Presentation/GameController.Idle.cs), [idle UI](../../Assets/HELLSCRIPT/Runtime/Presentation/GameUI.Idle.cs), [world restoration](../../Assets/HELLSCRIPT/Runtime/Presentation/WorldView.Idle.cs), [offline settlement](../../Assets/HELLSCRIPT/Runtime/Core/GameStore.cs).

## Evidence and captures

[Validation summary](../../Artifacts/Validation/IdleMode/validation-summary.json) · [Full Edit Mode XML](../../Artifacts/Validation/idle-display-editmode.xml) · [Source hashes](../../Artifacts/Validation/IdleMode/validated-source.json) · [Harness diff](../../Artifacts/Validation/IdleMode/Full2-to-Build10.diff) · [Profile summary](../../Artifacts/Validation/IdleMode/profile-summary.json) · [Visual review](../../Artifacts/Validation/IdleMode/visual-review.json)

| Native8 | Capture |
|---|---|
| initial-01-battle-entry-ko | [PNG](../../Artifacts/Validation/IdleMode/Native8/initial-01-battle-entry-ko.png) |
| initial-02-introduction-ko | [PNG](../../Artifacts/Validation/IdleMode/Native8/initial-02-introduction-ko.png) |
| initial-03-introduction-en-140 | [PNG](../../Artifacts/Validation/IdleMode/Native8/initial-03-introduction-en-140.png) |
| initial-04-dimmed-en-140 | [PNG](../../Artifacts/Validation/IdleMode/Native8/initial-04-dimmed-en-140.png) |
| initial-05-peek-en-140 | [PNG](../../Artifacts/Validation/IdleMode/Native8/initial-05-peek-en-140.png) |
| initial-06-automatic-dim-en | [PNG](../../Artifacts/Validation/IdleMode/Native8/initial-06-automatic-dim-en.png) |
| initial-07-next-rift-deferred-en | [PNG](../../Artifacts/Validation/IdleMode/Native8/initial-07-next-rift-deferred-en.png) |
| initial-08-repeat-stopped-en | [PNG](../../Artifacts/Validation/IdleMode/Native8/initial-08-repeat-stopped-en.png) |
| initial-09-save-blocked-en | [PNG](../../Artifacts/Validation/IdleMode/Native8/initial-09-save-blocked-en.png) |
| initial-10-foreground-resume-required-en | [PNG](../../Artifacts/Validation/IdleMode/Native8/initial-10-foreground-resume-required-en.png) |
| restart-01-resume-choice-town-en | [PNG](../../Artifacts/Validation/IdleMode/Native8/restart-01-resume-choice-town-en.png) |
| restart-02-suspended-rift-restored-en | [PNG](../../Artifacts/Validation/IdleMode/Native8/restart-02-suspended-rift-restored-en.png) |
| restart-03-remembered-idle-en | [PNG](../../Artifacts/Validation/IdleMode/Native8/restart-03-remembered-idle-en.png) |

| Profile | Capture |
|---|---|
| Final normal | [PNG](../../Artifacts/Validation/IdleMode/AfterNormal1/profile.png) |
| Final dimmed 1 | [PNG](../../Artifacts/Validation/IdleMode/Dim2/profile.png) |
| Final dimmed 2 | [PNG](../../Artifacts/Validation/IdleMode/Dim3/profile.png) |
| Final dimmed 3 | [PNG](../../Artifacts/Validation/IdleMode/Dim4/profile.png) |
| Final peek | [PNG](../../Artifacts/Validation/IdleMode/Peek1/profile.png) |
