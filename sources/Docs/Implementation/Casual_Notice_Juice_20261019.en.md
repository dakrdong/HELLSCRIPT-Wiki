# Common confirmation and toast polish — 2026-10-19

[한국어](Casual_Notice_Juice_20261019.md). The `.28s` unscaled rise and fade (`CasualNoticeEntrance`) from [the common notices note](Casual_Common_Notices_20261017.en.md) already covers the confirmation card and the toast. Only two missing pieces were added on top.

- **Confirmation scrim fade**: the Navy `.6` scrim now fades alpha 0→`.6` on the card's own clock (`SetBackdrop`). Final colour, alpha and input blocking are unchanged; no new colour literal, text or save key.
- **Stop updating when finished**: `CasualNoticeEntrance` sets `enabled=false` once complete and `Restart()` re-enables it, so no empty per-frame `Update` remains (`AgentRules/CPU_Performance.md`).

## Deliberately not done
- **Scale pop on card or toast**: the note says the entrance never changes size, and `RuntimeCasualUiFourSmoke` asserts "preserves scale rather than zooming".
- **Exit fade for toast or confirmation**: it would reorder the toast expiry (`GameUI.cs` Update) and the confirmation `Close` (immediate Destroy, then action), which is outside this scope.
- **Plaza, attendance and dock polish**: PRs #144 (`codex/town-shortcut-stack`) and #146 (`claude/dock-medallion-icons`) still edit the same files (`GameUI.Plaza/ContentDock/Attendance/SettingsFrame`); do it separately from the merged `main`.

## Verification
Only `CasualNoticeLayoutTests` (including the new `BackdropFadesOnTheEntranceClockAndAFinishedEntranceStopsUpdating`) ran in batch EditMode. `python3 tools/check_ui_contract.py --verify` and `python3 tools/check_ui_refresh.py` passed. The full EditMode suite, runtime smokes, a macOS build and a device were not run.
