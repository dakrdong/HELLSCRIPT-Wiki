# CPU recurrence prevention and performance review rules

Updated: 2026-10-03 · [한국어](Performance_Review_Rules.md)

These development and review rules apply to menu/world presentation, frame updates, UI/layout and performance changes. Prevent the causes recorded in the [2026-10-02 CPU measurements](CPU_Optimization_20261002.en.md), while preserving the [shared UI contract](Shared_UI_Contract.en.md) and gameplay, save and reward behavior.

## Confirmed causes and result scope

| Confirmed cause | Implementation requirement |
| --- | --- |
| World cameras and terrain kept rendering behind opaque title and character-selection UI. | Suspend fully covered world presentation through the existing `World.SuspendPresentation()` and restore it through the existing `ResumeCamera()` when the world is needed again. Preserve menu UI and animation. |
| Controller and UI both called `RefreshPlaza()` in the same frame. | Keep one owner for continuous state in `GameUI.LateUpdate`, after movement. Preserve required action-triggered refreshes without adding the same periodic refresh to another frame callback. |
| Town buttons repeatedly changed false→true, causing activation and UI work. | Calculate the final state from portal/NPC/shop priority and apply it once. Unchanged state must not trigger activation transitions, body generation or layout rebuilding. |

Matched development-player measurements confirmed lower menu CPU/frame and town GC allocations. They did not establish consistent improvements in town/inventory/combat CPU, memory or frame time. Some menu/combat allocations increased. Do not extend these results to every scene, shipping foreground browsers, Safari/Firefox or physical-mobile CPU/battery use. The measurement record contains values and repeat ranges.

## Implementation and review checklist

- **Presentation lifecycle and restoration:** Confirm that the world is fully covered. Do not apply menu suspension to transparent/partial windows or a visible battle world. Reuse camera owners instead of introducing another global camera switch. Check required camera/UI/input/audio/frame-policy restoration across title↔character selection, menu exit/reentry, town↔battle portals, and power-saving entry/exit including peek/unlock.
- **Frame-update ownership:** Inspect existing callers before adding `Update`/`LateUpdate` work. Refresh continuously changing distance/proximity state once after movement. Apply unchanged text/colors/buttons/window layout only when their state changes. Removing periodic duplication must retain immediate action feedback and refreshes after successful saves.
- **Invalidation and shared components:** Any cache must list all display dependencies, including language, equipment/character, save completion, window opening/closing, resolution/layout and nearby targets where relevant. Measure the cost of rebuilding strings or lists just to compare them. Reuse existing display functions, shared UI and buffers before adding a cache layer without evidence.
- **Allocations and layout:** Investigate string formatting, LINQ, temporary collections, UI creation/destruction, activation transitions and layout updates on paths with measured repeated cost. Remove unnecessary work while preserving buffer ownership and initialization. Fewer lines do not establish performance improvement.
- **Domain correctness:** Preserve combat ticks/RNG/balance, prices/admission, save timing, transaction authorization/duplicate protection/failure/retry and reward validation. Do not replace final validation with cached UI values or reduce its frequency. Refresh through existing transactions after successful saving.
- **Ponytail lite:** Preserve requirements, reuse existing code, remove unnecessary repetition and keep changes small. The recorded measurements used installed 4.10.0 guidance manually; they do not claim an official CLI audit pass. Record the actual version and guidance in future use. An audit or readability review does not replace regression checks and CPU measurements.

## Before/after measurements and evidence

1. **Establish the current baseline and ownership.** Check checkout/commit/dirty state, running game/Editor/browser processes and disk space. Do not stop user games or other workers' processes or revert their work. Use the smallest necessary isolation without multiple large copies or parallel Unity builds.
2. **Fix comparison inputs.** Record the same machine, platform/build mode/backend, scene/save/seed/progress, resolution/DPR/quality, frame cap/VSync/background policy and visibility/focus. Compare menu, town idle, combat and an open UI separately. Distinguish the Editor, native development/release player, and web Renderer/GPU/browser processes.
3. **Verify actual pacing.** A configured 60FPS does not establish matching conditions. Check actual FPS, warmed frame intervals, and web visibility/focus, refresh-rate estimates and scheduler changes. Preserve excluded hidden-page or changed-condition samples with reasons and repeat under matching conditions. Report savings from lower FPS/resolution/quality separately from logic improvements. Do not transfer measurement-only scheduling into shipping policy.
4. **Freeze build evidence.** Use unique output paths per job with distinct before/after locations and record source commits, build settings and runtime arguments. Verify completed player and web HTML/loader/framework/data/WASM hashes before and after measurement. Apply instrumentation only to separate development outputs and freeze their hashes after patching. Do not mix replaced/overwritten executables into an earlier baseline. Preserve old evidence in separate paths.
5. **Record repeats and metrics together.** Run at least three before/after repeats per state with matching warm-up/sample duration and check ordering bias. Do not run another build or full suite during measurement. Preserve raw samples and repeat ranges for CPU ms/frame/usage, GC bytes/frame/collection count, managed heap/Unity allocations/process RSS/web JS heap, and mean/p95 frame intervals. State the CPU percentage's core convention and measurement window.
6. **Separate work from waits.** Compare actual process CPU time with Profiler regions; do not add `WaitForTargetFPS` or wall time as CPU work. Do not sum nested markers or infer savings solely from a smaller marker after moving work from `Update` to `LateUpdate`. Editor CPU reduction is not player optimization evidence.
7. **Report only the verified scope.** Examine repeat ranges and individual pairs as well as medians. Overlapping ranges or inconsistent differences remain unconfirmed improvements; disclose increased allocations/memory and slower samples too. Effects on other platforms/scenes, release mode or physical mobile remain unverified until measured separately.

## Completion criteria

- Review identifies changed presentation/update owners, removed repetition, restoration/invalidation conditions and preserved domain behavior.
- Related regressions and actual-player checks cover menu/portal/power-saving transitions, continuously moving town HUD, portal/NPC/shop priority, stable button activation and transaction/save failure/retry. UI changes retain shared-contract, default-text-size, KO/EN and existing resolution checks.
- Follow the existing full-suite procedure once after the last code change and last `main` merge. Compare failure names with a fresh baseline rather than treating historical failure counts as an allowlist. Never weaken checks to hide failures.
- Preserve matched raw measurements, hashes, repeat comparisons and unverified scope. If measurement is blocked, report the blocker and code-review findings without claiming improved performance.
- Documentation/instruction-only changes require link/history/static checks rather than rebuilding or running the game. Wiki generation/publication and commit scope follow root instructions and the authorization for that request.
