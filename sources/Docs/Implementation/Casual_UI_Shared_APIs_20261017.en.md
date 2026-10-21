# Casual shared presentation APIs — 2026-10-17

Base: `dbec4dbed084596471675f375c1d1ac517b4b86c`. These instance opt-ins support plaza, attendance/daily rewards and common notices without changing global styling, input, saves or transactions.

- `UiButton.SetCompleted(bool)` gives an unavailable completed Casual reward a white/cream face. Keep `interactable=false`; ordinary Disabled/Locked, label alpha `.55`, legacy/close/Google appearances retain their behavior.
- `SetCasualAppearance(float? radius=null,float? borderWidth=null,float? shadow=null,Color? edge=null)` scopes geometry and the available edge to one Casual button. Values are Canvas local units. An empty call restores defaults; Disabled/Locked colors and hit geometry retain priority.
- `StorageSurface.SetSkin` appends optional `fill`/`fillEnd`. No arguments retain cream defaults, one fill is solid, two colors form a gradient, and null skin restores legacy paint.
- `UiSkin.Casual` exposes AttendanceReadyFill `#e9f9e4`, AttendanceMilestoneEdge `#e0a400`, and OnLight PlazaSecondaryInk `#697190`/PlazaDetailInk `#4a5272`. Existing matching D4 tokens are reused.

Identical repeated state calls preserve transition progress and reuse the existing face.

Validation: independent Roslyn type compilation using actual Unity Bee references and all seven analyzers: Runtime 851 sources/296 references, Tests 268 sources/308 references; zero errors and zero changed-source warnings. Tests reference this candidate Runtime. Eight focused NUnit regressions are compiled, **not executed**. The 225 existing Runtime warnings are recorded separately. Input hashes remain unchanged. Unity, NUnit execution, game builds and primary writes: zero.

Receipts and input pins: integration workspace `ui-six-integration-20261017/validation/common-api/`. Actual runtime verification belongs to the authorized combined batch after all screens freeze.

## Full-original follow-up contract

These additions follow the original c/f/l audit after `559abd4f`; the initial compile receipt does not validate this later source. Final type checking and execution wait for the corrected input freeze.

- `UiWindowFrame.SetSkin(UiSkin,float? borderWidth=null,float? shadow=null)` retains scale-based defaults; explicit values are fixed local metrics for nested scaled cards.
- Explicit SetCasualAppearance geometry also supports CasualClose. Unspecified shape and danger colors remain unchanged.
- Immutable `WithInteractionStates()` supplies Action hover `#fff6c2`, Primary hover `#a2e891→#5cc865`,3-unit face/content press travel and zero pressed/disabled shadow. ApplyChrome passes the scope to the face; default Casual and legacy remain unchanged. Direct child content restores its resting position on release/disable.
- `WithButtonTops`/SelectedTop/DangerTop scope selection/danger upper colors; HuntEdict uses `#ffe26b`/`#ff8b8e`. WithEquipmentArt preserves both scopes.
- EdictCoinShade/EdictHover reuse existing D4 GoldF08A00A1000/GoldFFF6C2A1000. Central tokens add EdictChangedInk `#b86200`, EdictFactInk `#8a5600`, EdictSuccessFill `#e9f9e3`, EdictScrollTrack `#efe6cf`, EdictOffFill `#eef0f6`, EdictQuietFill `#f4f6fa`, EdictEmptyFill `#f7f8fb` and EdictSocketFill `#e9edf6`. Small text retains the caller's OnLight contrast handling.

한국어: [공통 표시 API](Casual_UI_Shared_APIs_20261017.md).
