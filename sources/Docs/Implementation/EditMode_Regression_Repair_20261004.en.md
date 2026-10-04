# Repairing full-suite failures after the skill rework

[한국어](EditMode_Regression_Repair_20261004.md)

As of: 2026-10-04. This follow-up repairs the 128 failures retained by the earlier [skill identity rework](Skill_Identity_Rework.en.md). A pre-existing failure is still a completion failure. Historical reports remain available with their original results.

## Causes and resulting behavior

| Cause | Repair | Preserved contract |
| --- | --- | --- |
| Unity JSON materializes an absent DPS graph as an empty object | Real new graphs opt into version 1. Empty version-0 placeholders normalize to absence; existing samples migrate. | A real graph saved before its first sample survives. Old graphless records remain graphless. |
| Preview cloning creates absent combat journals | Shared copying and build/edict previews preserve absence; only valid journals accept events. | Preview and adopted builds produce the same combat state. Training does not acquire rift audit events. |
| Resume recalculates movement route caches | The existing navigation owner persists waypoints, next index, failed-route retry time and footprint radius; gate and terrain changes invalidate them together. | The same checkpoint and seed continue with the same hero/enemy positions, including failed paths. |
| Resume resets journal event throttles | Decision timestamps and the last movement key/time persist. | Pure restoration does not add duplicate decisions or movements. A real player resume separately appends exactly one RESUME audit event. |
| Tests start training and then flip only its rift flag | Restore a valid legacy rift checkpoint; use stage 1 for formerly stage-0 fixtures and a real LiveOps reward snapshot. | Existing action, projectile, channel, settings, failed-save and exception assertions remain. |
| Iron Wall's chained-shield example never reaches its intended response | A nearby melee warning triggers defense; the example explicitly allows attack-preparation interruption in KO/EN. | Guardian's Vow is actually prepared, then automatic Iron Wall creates its barrier after the earlier shield ends. |

Owners: [graph normalization](../../Assets/HELLSCRIPT/Runtime/Core/TrainingGround.cs), [save normalization](../../Assets/HELLSCRIPT/Runtime/Core/GameStore.cs), [navigation](../../Assets/HELLSCRIPT/Runtime/Core/RiftNavigation.cs), [combat journal](../../Assets/HELLSCRIPT/Runtime/Core/CombatJournal.cs), and [production scenario conditions](../../Assets/HELLSCRIPT/Resources/SkillPresetScenarios.json). No tests were disabled, deleted, ignored or given wider tolerances. Combat equivalence uses `recordResume:false`; actual player resume auditing is checked separately.

## Validation

All 5,122 final Edit Mode tests passed, with zero failed, skipped or inconclusive cases, in 1,923.9 seconds. The unfiltered full suite ran once against the final integrated game source. All 3,923 source inputs remained unchanged between build, native acceptance and the suite. The latest focused results cover 130 distinct passing cases; they do not substitute for the full suite. Two new graph regressions cover new empty graph restoration, historical absence and preservation of versionless damage/cast samples.

Unity 6000.6.0f1 produced a successful macOS development player with zero build errors. The 3,923 input source files have fingerprint `0d6e8748374383c73a0e8f0f4b8fd254b10544af5270ba635de57940011b3729`. Build-generated project settings were compared and restored; packages were unchanged.

Real rifts 1, 5 and 12 were saved locally after 200 ticks, then continued for 100 ticks in a fresh player process. Their entire saved JSON, including positions, waypoints, RNG, health, cooldowns, DPS and journal throttles, matches uninterrupted 300-tick controls byte for byte. Actual player resume adds one RESUME event and increments the resume count once while preserving the in-flight action.

| Rift | SHA-256 of the matching 300-tick result |
| --- | --- |
| 1 | `cb0cbca4b2937d15c6192419d6a79f7b09bec3d4fb4081dd0a4e5f5af72cd430` |
| 5 | `e0940b2a46a58433fdb76bc6abc281b0ca620214988fa5c3a4d48149cb793b96` |
| 12 | `325cf835548ce7259a6d5565f11a4b2efa1968a73e5a87ab2d6241e8e2cb2f9a` |

The production Chain shields preset was selected through actual uGUI raycasts and synthetic pointer down/up/click. Automatic combat created an Iron Wall barrier of 628 after the prior barrier ended, and policy persistence was verified by rereading the current guest profile directory. Ten KO/EN captures cover 440×956, 956×440, 1600×900, 1600×1000 and 2100×900; one more covers simulated safe area. Shared UI ownership, all 11 contract tests and the static refresh guard check pass. This is macOS acceptance, not physical-mobile or performance evidence.

The first fresh-process comparison failed because layout acceptance had persisted EN while the control journal was generated in KO. The full JSON assertion therefore compared different localized log strings. Matching the isolated fixture language to KO repaired the comparison; only the failed resume scope was repeated. Its first failure log remains available.

## Reproduction and retention

Use `-hellscriptRegressionRepairSmoke`, followed by `-hellscriptRegressionRepairResume`, from the [native acceptance owner](../../Assets/HELLSCRIPT/Runtime/Presentation/RuntimeSkillTreeSmoke.Identity.cs). Both launches require `-hellscriptOfflineQa`, a task-owned `-hellscriptSavePath` and the same `-hellscriptEvidence`; start with fresh evidence. Before each comparison launch, set the isolated save folder's `hellscript-language-v1.json` to `{"version":1,"language":"ko"}`. Real account, server and cloud saves are not used.

[Reports, checkpoints and screenshots](EditModeRepairEvidence20261004/EditMode_Repair_Evidence_20261004.md) are preserved in source and the public wiki. The binary and build intermediates stay in this task's `Artifacts/Validation/EditModeRepair20261004`. Review binary retention on 2026-10-11 and report/log/hash/recipe retention on 2026-11-04. A review date does not authorize deletion. Original source, images, player saves and shared caches were preserved. Public wiki staging uses macOS `/bin/cp -c` for independent APFS clone files instead of temporarily duplicating roughly 3GB. Other platforms retain normal copying; the previous public tree remains available until replacement generation finishes.
