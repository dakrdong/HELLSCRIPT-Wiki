# Combat recording inclusion in the takeover candidate (2026-10-24)

[Korean](Combat_Recording_Integration_20261024.md)

The user requested inclusion of the existing recording-based combat change in main integration. Actual sources, Git ancestry, the official PR and supported task inventory were checked rather than inventing another implementation.

- Owner task: Codex **전투 진행 레코딩 방식 도입**, `01a16cab-1ba5-7dd2-b58c-c15b4b1896c9`, local worktree `/Users/t8g-2410-pn-005/.codex/worktrees/combat-recording/HELLSCRIPT`. It was idle and clean with no additional uncommitted change. Its separate push history does not authorize transmission of this takeover candidate.
- Branch `codex/combat-recording`, head `9bb20f75b0d5ad6acb69c96c4d019b97ff57968f`. [PR186](https://github.com/dakrdong/HELLSCRIPT/pull/186) merged at14:38:17UTC as main `1f1887d1148d99e5b5f42f9a899d8945c6209259`, with parents `ad3bbdff` and `9bb20f75`. The merge and recording head have identical trees.
- Existing takeover candidate `a33d448a15ad315e28e6a4a0384caed53f3a0752` already includes that main. No duplicate cherry-pick/merge or legacy combat replacement was performed. The [existing recording contract](CombatRecording20261024.md) remains authoritative.

## Combined impact

The overlapping changed files are localization, `CombatSimulation.ClassSkillAuto.cs`, `CombatSimulation.cs`, `GameUI.cs` and `RiftVisibilityTests.cs`. Combat differences retain the approved pack-center movement/target-origin work; the controller has not reverted to the old direct-tick path.

Seven critical sources match the recording head byte-for-byte: both snapshot files, recording simulation, frame settlement, controller, rune save and journal. The HUD preserves its preparation label while Playback is not Ready. Presentation reads played state; no new forecast reward/display/settlement path was added.

New E tactical fields and pack-walk timing/distance use the existing reflective field-capture fallback. Two sparse/replacement cases verify Off state, opaque Unicode/trailing-space values and both movement fields survive actual recorded-delta application. E player entry remains OFF and tactical combat behavior remains outside its current scope. Saves/rewards still settle through existing playback-time transactions; no future tape is copied into user saves.

## Actual validation

On the current candidate, directly affected recording/state restoration/E tactics/casual HUD/result popups/pack-center lesson cases passed **123/123, zero failures/skips**,19:50:29–19:52:41UTC, Unity6000.6.0f1. This includes preparation/playback parity, input-change replanning, checkpoints, rewards and failed-save retry. Two cases were added; no production runtime changed.

Local evidence: `Builds/Validation/UserReports20261024/takeover/recording-inclusion.json` and `recording-compatibility/results.xml`. Previous9-step local CI evidence is reused because its inputs did not change; all Unity assemblies recompiled during this run. Nine own validation-identity/import files were hash-preserved before restoration. The recording owner's worktree, user-test project and actual user saves were unchanged.

This is not fullEditMode, a new benchmark, rendered/native-pointer, mobile/IL2CPP or build validation. Existing benchmark numbers are not new measurements on this candidate. Actual CLI review and the subsequent pinned head are recorded separately in local orchestration/CODEX_CHECK_20261024.json. This request does not clear existing GitHub-transfer or J-import denials.
