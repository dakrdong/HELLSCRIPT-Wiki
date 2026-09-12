# Teleport hunt-edict implementation record

Date: 2026-09-13 · [한국어](Edict_Teleport_Expansion.md)

Status: **All four M04 core options, the 1,601-test full suite, native macOS editing/save/restart and 32 before/after diagnostic runs are complete.**

## Problem and ownership

The global survival path already read M04 options, but ordinary candidates still came from legacy rules. An old danger condition could therefore cancel a preparing attack without a global emergency or dodge request. An enabled hunt edict now derives one independent M04 candidate from its four options, even when the old row is missing, disabled or blocked. Duplicate candidates collapse without changing saved legacy rows. Opting out restores those rows.

Ordinary distance adjustment waits until the current main action ends. Moving M04 earlier in the common skill order cannot use ordinary repositioning to interrupt a preparing Chain Lightning. Necessary survival responses remain with the global executor and its interruption/fallback policies. Its existing targetless environmental-hazard path is preserved.

## Connected options

| Option | Runtime behavior |
|---|---|
| Automatic use | ON permits new candidates; OFF excludes them. Editing options does not rewrite the destination of preparation already started. |
| Purpose | SURVIVAL runs only when global emergency/dodge requests M04. DISTANCE first filters ordinary candidates to those reducing desired-range error by at least 2 m, then applies arrival preference. A busy main action, active global response or emergency reservation prevents ordinary adjustment. |
| Arrival preference | SAFE minimizes forecast HP loss over the same 1.5-second horizon. SPARSE minimizes nearby visible enemies within 3 m among permitted candidates. DISTANCE minimizes error from global desired range. Without a valid perceived target, that preference falls back to SAFE; ordinary distance adjustment itself still requires the exact valid global target. |
| Landing criterion | GLOBAL uses candidates permitted by global dodge/lethal assessment. NO_DAMAGE additionally excludes any candidate with positive detected final damage within the same horizon. It does not promise safety from unobserved threats. |

Desired distance comes from either the explicit global value or basic attack range. The Mage's basic range is 10 m; it does not mix in the saved explicit value. Ordinary adjustment uses the actual global target and does not substitute a hidden or excluded enemy.

Survival use requires **assigning M04 as the global escape or fallback skill and enabling the intended response conditions**. New edict documents have no escape assignment and default emergency HP, lethal and dodge conditions to OFF. Merely enabling M04 automatic use does not configure survival. The Korean and English card note now states this dependency. Saved policies and global defaults are not silently enabled.

## Preparation and landing

Unlock level 10, 0.2-second preparation, 9-second cooldown and zero resource cost are unchanged. Landing requires an actual distance of 2–8 m, valid terrain, sight from the origin and no overlap with perceived enemy bodies. Zero-cost M04 does not consume an LC02 discount charge. Preparation grants no invulnerability and the skill deals no intrinsic damage.

Immediately before release, the committed destination is revalidated for distance, sight, terrain and enemy bodies. An invalid destination records `ACTION_INTERRUPTED`, does not move the hero and does not refund cooldown. Options cannot retarget that cast. Valid committed landings execute exactly once after OFF/purpose/preference edits and save restoration.

Physical validation also applies to legacy Teleport. When an old target-point rule requests the enemy's center, it chooses the closest currently legal candidate before starting. This preserves the requested positioning intent without permitting a body-overlapping landing. A small floating-point tolerance handles the exact 2 m boundary.

Global exploration or pursuit can resume in the same tick after landing. No Frost Nova/Fireball is queued, and no wall pass, invulnerability or cooldown reset is added. Completion is checked against the actual `ACTION_END` position rather than requiring the hero to remain there afterward.

## Chain Lightning interruption diagnostics

The read-only observer now retains actual interruption reason/time, HP percentage, replacement action and the previous global assessment. That assessment may precede a later event in the same tick; the actual action-event reason is authoritative. The observer non-interference check remains in the suite.

Baseline code is `e601046`, including development after the Claude attribute integration. Both baseline and changed code ran eight legacy-rule and eight default-edict cases: 32 runs total. Seed indices 0, 1, 2, 3, 4, 5, 9 and 14 were selected from earlier outcomes, not randomly sampled. Level 30, stage 20 and eight legal equipped items were fixed. Paired seeds, maps, encounters, objectives, full gear identities, stat sheets, maximum HP and base damage matched. Every baseline summary exactly reproduced the archived M03 summary. Observed starts, releases, hits, interruptions and time totals matched existing telemetry.

| Measure | Before legacy, 8 | Before default edict, 8 | After legacy, 8 | After default edict, 8 |
|---|---:|---:|---:|---:|
| Clear / death / timeout | 0 / 4 / 4 | 0 / 4 / 4 | 0 / 4 / 4 | 0 / 7 / 1 |
| Runs spawning a boss | 4 | 2 | 4 | 5 |
| M03 starts / interruptions / first-target misses | 535 / 29 / 6 | 551 / 78 / 13 | 538 / 28 / 6 | 458 / 18 / 2 |
| Actual M03 hits | 1,740 | 1,754 | 1,761 | 1,752 |
| M04 starts | 146 | 148 | 149 | 0 |
| Resource-blocked checks | 0 | 0 | 0 | 0 |

The 78 baseline edict interruptions comprise 56 old M04 actions, 18 old M05 actions, two pulls and two deaths. Afterward, 18 comprise 16 old M05 actions and two deaths. The recorded last global emergency/dodge request was inactive while the legacy rules could still cancel preparation.

**Interruptions fell, but deaths increased from four to seven; this is not a demonstrated performance improvement.** `CreateForHero` copies equipped slots into a document with fresh global defaults. It does not translate old recommended HP/danger conditions. This diagnostic edict has global survival/dodge disabled and M04 set to SURVIVAL. Zero M04 starts after the fix follow that configuration. A subsequent comparison must evaluate an explicitly configured survival document as a separate group. Legacy outcomes also need not be identical because physical landing validation changed there too.

No damage or affix weights were tuned, and default survival policies were not enabled to improve these results. Deaths and time before boss arrival remain joint investigation targets. The specified single Chain Lightning hit on an isolated boss is preserved.

Raw evidence is in `Artifacts/Validation/EdictTeleport/Baseline16/` and `After16/`; matched analysis is `comparison.json`. Collection completed on 2026-09-12 at 16:30:14–16:32:09 UTC and 16:32:36–16:34:21 UTC, respectively. The existing `RunChainDiagnostics` arguments are retained; output schema is now 2.

## Validation and build

There are 35 new M04 cases. The focused policy/response/order/action-continuity/localization selection passed 268 tests. After the later fixture and card-note fixes, 141 related tests passed. The final full Edit Mode suite passed **1,601 tests** with no failures or skips on 2026-09-12, 16:44:24–16:49:19 UTC, in approximately 295.44 seconds.

The first full suite passed 1,600 of 1,601 tests. An existing evasion-statistics fixture attempted a 0.5 m Teleport, violating the specified 2 m minimum. It now makes a legal 2 m move remaining inside the real N10 warning and compares it with a 4 m escape. A fully shielded actual hit still counts as a hit, preserving the test's purpose. Earlier focused failures involving an array API, synthetic map, visibility, landing boundary and post-landing walking are retained with their corrections in the evidence directory.

| Native macOS scenario | Verified result |
|---|---|
| Editing and saving | Actual UI callbacks select DISTANCE, arrival preference and NO_DAMAGE. Drafts leave the original unchanged; saved memory matches a fresh file read. The edict enable switch uses the actual button. |
| Independent restart | During preparation, options change to SURVIVAL/SAFE/GLOBAL/OFF. A separate process restores time, resource and committed destination, releases once, retains zero cost and neither queues an attack nor starts another OFF Teleport. |
| Targetless hazard | M04 and the global response are explicitly enabled. With enemies moved outside perception, a visible environmental hazard starts global survival Teleport with target id -1 and completes its landing. |
| Layout and pause | At 1280×720, 720×1280 and 640×360, Korean/English and 140% text pass height, missing-translation and footer safe-area checks. Language changes and closing the decision screen preserve both running and manually paused entry states. |

The first native attempt incorrectly treated valid post-landing pursuit as a failed landing. Only the fixture assertion was changed to inspect the real completion event. Final `Native2` checkpoint and restart processes both passed on 2026-09-12 at 16:44:17–16:44:37 UTC. Ten screenshots were retained; the English 140% portrait options, small landscape note, Korean resumed decisions and targetless escape battle were visually inspected. Long content remains scrollable. This uses controlled combat fixtures and UI callbacks, not mobile-device or natural-play balance validation.

The build is `Builds/macOS-EdictTeleport/HELLSCRIPT.app`; evidence is `Artifacts/Validation/EdictTeleport/`. All 247 C#/English-table inputs, including 174 Runtime C# files, match the full-test and final build projects. No C# metadata is missing and no asset GUIDs are duplicated. Diagnostic combat code matches final code; the earlier card note, translation, native fixture and evasion-statistics fixture are preserved as four source variants.

`validated-source.tar.gz` contains 662 source, validated design-input, authored-setting and diagnostic-variant files. Its SHA-256 is `9579f66d65c799ad084a9db79d1782b87d3f9a64cbfa9a142198344e2cb75130`. Unity-added URP Editor material metadata is recorded separately; the authored asset is preserved. Save schemas, scenes, prefabs and packages are unchanged. Validation uses isolated projects and dedicated save directories.

## Remaining work

Next are independent M05 Elemental Shield and M06 Frost Nova core policies. Start with remaining legacy M05 eligibility and attack interruptions, then compare explicitly configured survival policies separately from fresh default documents. Existing player settings must not be replaced automatically. Multi-stage growth/farming value, remaining legendary/set equipment boundaries, mobile execution and release quality remain open.
