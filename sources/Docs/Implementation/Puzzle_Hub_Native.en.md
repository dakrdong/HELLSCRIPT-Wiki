# Native puzzle waiting room implementation

2026-10-07. Stage 6 implementation. Native visual/input/performance acceptance remains scheduled for the final integrated player at stage 9.

`PuzzleHubWindow` uses the generated `ContentWindowView` entry point, shared host, safe area, theme, fonts and buttons. Its one-screen composition adapts the shared body to status, character art, a level board and fixed actions. Existing inventory, Hunt Edict, skill tree and settings remain their actual owners.

`GameController.Tutorials` prepares the current owned level and connects the hub to fresh attempts. Persisted `RecordPuzzleAttempt` results return to the hub. Prior levels use detached `CreateReplay` state. Town access remains locked until all 15 levels clear.

Menu transitions move the hub and existing destination window for 0.62 seconds. Portrait uses the X axis, landscape the Y axis; perpendicular swipes are ignored. The status stays fixed during transitions. Closing a destination or selecting Waiting room returns to the same hub.

The first status row contains actual HP/resource and the second full-width row XP. Selection reads saved progress, locks future levels, and switches the final cleared level action to Town. `StoreViewBinding` compares hero, progress, XP, skills and hint display dependencies and defers repaint during input, transitions and child windows. Language and safe-area changes invalidate layout.

The existing window host/camera lifecycle hides the world behind the opaque hub. Effects modify existing transforms, colors and fills without rebuilding UI each frame. Any performance result requires three matching launches of each real before/after player binary at final validation.

Import only necessary prototype HTML/tools/art/prompts; preserve unknown model provenance. Native textures receive a dedicated importer and ResourceTextureBudget entries. Historical screenshot batches are not imported.

Final acceptance: three classes, KO/EN, portrait 440×956, landscape 956×440, desktop 16:9/16:10/21:9; real arrow, replay, graduation, menu, axis swipe and save/reentry interactions; shared UI/refresh/binding checks. Full EditMode and native smoke run once on final integrated code. Physical mobile remains separately unverified.

Effect cost will compare two actual binaries of the same new hub, with effects disabled and enabled. The previous version has no hub and cannot represent the same screen. Capture three launches per binary with matching source, input, resolution, FPS and quality.

Integration: Unity drives overlay Canvas root geometry, so animate the existing window frame and fade its existing backdrop CanvasGroup. `ContentWindowView.FixedOverlay` owns the fixed status layer. Replay inventory/edict menus use real `GameStore.Sandbox` transactions, validation and commit notifications while changing only a detached copy without disk writes. Replay transactions and guide discoveries leave the original account unchanged.

Focused evidence: `stage6-hub-save-focused-repair.xml` initially passed 91 of 95 checks. The missing Play/Town translations and replay checkpoint rejection were repaired. `stage6-replay-checkpoint-repair.xml` passes the four failed checks plus 11 title-screen checks: 106 unique checks now pass, zero skipped. Real owned Hunt Edict commits, transaction rollback, original-account preservation, save bindings and swipe axes are covered. `tools/check_ui_contract.py`, `tools/test_ui_contract.py` (11) and `tools/check_ui_refresh.py` pass. The unchanged prepared HTML prototype report has 382 passing checks; it is separate from native-player acceptance.

2026-10-07 final acceptance: [stage 9 integration](Puzzle_Tutorial_Final_Acceptance.en.md) verifies 30 class/locale/layout combinations, 12 safe-area simulations and 12 full journeys. Its record identifies reused completed spans, failed-only retries and the source/save/input evidence.
