# Elemental Shield hunt edict implementation

Date: 2026-09-13 · [한국어](Edict_Shield_Expansion.md)

Status: **M05's four core policies, encounter-opening persistence and actual absorption rewards are connected. All 1,652 tests, three native macOS processes and 48 before/after diagnostic runs completed.**

## Problem and integration baseline

The ordinary M05 candidate still depended on legacy action rules. That bridge could cancel a preparing Chain Lightning without satisfying the new preventive policy. An opted-in Mage with M05 equipped now derives one independent candidate from the edict, even when its legacy row is missing, disabled or blocked. Duplicate rows collapse into one candidate. Stored legacy rules remain intact and resume when edict mode is disabled.

This phase builds on `7313034`, including Claude's attributes, affixes and balance handoff. Claude's `b1f3ea5` is already integrated into main. Combat numbers and affix weights were not retuned; shield creation and absorption use the integrated character sheet.

## Four core policies

| Option | Runtime behavior |
|---|---|
| Automatic use | ON admits the candidate; OFF excludes it. An already committed preparation survives option edits and restoration without cancellation or duplication. |
| When to defend | EMERGENCY uses global emergency conditions. PREVENT also accepts detected incoming damage under global dodge assessment. ENGAGE adds one review before the first attack in a new encounter. If unavailable, attacks continue. |
| Another shield | AFTER defers ordinary use while another active shield remains. TOGETHER permits use when actual additional capacity is positive. |
| Encounter | ALL, ELITE and BOSS restrict ordinary PREVENT/ENGAGE use to eligible perceived enemies in the encounter. Hidden or chase-excluded elites cannot qualify it. EMERGENCY stores this option but leaves it inactive. |

The global executor runs first. Only a **direct global emergency selection of M05** bypasses AFTER and encounter restrictions; an ordinary candidate cannot invent that exemption merely because emergency conditions hold. Both paths prohibit refreshing an active M05 and cap total shields at 100% of maximum HP.

Preventive damage requires a positive detected final-damage hit within the existing 1.5-second look-ahead. A zero-damage control request alone is insufficient. Targetless environmental damage remains eligible. Ordinary interruption respects global FINISH/CANCEL_ALLOWED and the current action phase; a selected global walk or survival response takes precedence.

## Opening and save boundaries

The opening is reviewed at the first ordinary decision with eligible perceived enemies and no busy main action. A ready ENGAGE shield runs before the first attack even when ordered later, while a higher-priority loot or exploration category remains respected. OFF, level lock, cooldown, existing shields or capacity limits consume the unavailable review without delaying attacks or retrying the opening when the shield later becomes ready.

Rearming requires at least three seconds without sensed enemies, the existing three-second out-of-combat gaps for attacks/outgoing damage/incoming damage, no busy action and no detected danger. Brief sight loss, target changes, option edits, disable/re-enable and app restart do not create another opening in the same encounter.

A version-1 optional `edictEngagement` record is added to the running encounter. Mages track actions even with edict mode disabled; Warriors and Rangers do not allocate this record. An older in-progress Mage save without it conservatively treats the opening as used until a real combat exit. Foreign hero/run records are not reused. An unsupported future version is rejected while preserving the file.

## Actual shield and legendary behavior

M05 retains unlock level **15**, **0.2-second** preparation, **14-second** cooldown and **zero resource cost**. It works at zero resource and preserves a live cost-discount charge from genuinely equipped LC02. Base creation is 35% of current maximum HP multiplied by actual Willpower/MP04 shield scaling, limited by remaining total capacity, lasting four seconds.

At release, an active M05 or exhausted capacity interrupts the action without generating a shield or refunding cooldown. No invulnerability, damage or queued follow-up attack is added.

With LM03 equipped, that M05 must actually absorb **15% of its creation-time maximum HP** to restore 20 resource, once per shield with a four-second internal cooldown. Absorption by another shield does not count. A later maximum-HP change does not change the threshold. Partial absorption survives restoration and can reach the threshold afterward; a paid shield cannot pay again. A capacity-limited shield that expires or depletes below the threshold pays nothing.

Combat effects reads actual absorption, creation threshold, remaining time and reward state. Its shorter title fits enlarged English portrait UI. Repainting after language or text-scale changes now preserves the entry running/manual-pause state when closing.

## Explicit survival comparison

Before (`7313034`) and after M05 each ran eight legacy-rule cases, eight fresh-default edict cases and eight explicitly configured survival-edict cases: **48 runs**. They use outcome-selected seed indices 0, 1, 2, 3, 4, 5, 9 and 14, level-30 Chain Control, stage 20 and eight legal equipped items. This is neither a random population sample nor an optimal-preset test.

Fresh edicts have global emergency/dodge responses disabled, M04 SURVIVAL and M05 PREVENT. A diagnostic-only document explicitly configures:

| Area | Diagnostic settings |
|---|---|
| Emergency | Low HP enabled at 30%, lethal danger enabled, return threshold 50%. |
| Actions | Defense M05, escape M04, DEFENSE→ESCAPE→WALK, OTHER_SKILL fallback M05. |
| Dodge | Ground, area and direct attacks use DAMAGE at 15% HP; interruption is CANCEL_ALLOWED. |
| Unchanged defaults | WALK_FIRST, potion enabled at 40%, skill/resource reserves OFF, no control selection, overlap OFF and all remaining global/skill options. M05 stays PREVENT/TOGETHER/ALL. |

The helper explicitly assigns 16 fields; 13 values differ from fresh defaults. It does not modify player defaults, presets or saves. Schema 3 preserves each run's complete active edict. Paired documents, seed, map, encounters, objective, every equipment value, sheet, maximum HP and base damage match. The 16 baseline legacy/default summaries exactly reproduce archived M04 results. Final after-run summaries also reproduce the first after-run attempt. Cast starts, releases, hits, interruptions and 50ms occupancy time agree with actual statistics.

| Metric: before → after | Legacy, 8 runs | Default edict, 8 runs | Configured survival, 8 runs |
|---|---:|---:|---:|
| Clear / death / timeout | 0/4/4 → 0/4/4 | 0/7/1 → 0/7/1 | 0/0/8 → 0/0/8 |
| Runs reaching a boss | 4 → 4 | 5 → 2 | 6 → 5 |
| M03 starts / interruptions / first-target misses | 538/28/6 → 538/28/6 | 458/18/2 → 363/3/1 | 600/34/0 → 568/8/7 |
| M03 actual hits | 1,761 → 1,761 | 1,752 → 1,428 | 2,247 → 2,152 |
| M04 starts | 149 → 149 | 0 → 0 | 36 → 46 |
| M05 starts | 41 → 41 | 60 → 0 | 90 → 34 |
| Resource-blocked checks | 0 → 0 | 0 → 0 | 0 → 0 |

The default edict no longer has the 16 legacy-M05 interruptions. Its three remaining interruptions are one pull and two deaths. Zero M05 uses with PREVENT and response conditions disabled follows the document. Deaths remain seven, but boss arrival and hits decline and mean run time falls from 213.27 to 168.49 seconds; this is not an improvement claim.

Configured-survival interruptions fall from 34 to eight: three global emergency walks, three direct global M05 transitions and two pulls. Both versions avoid death in these cases but time out in all eight; boss arrival falls from six to five runs. The configuration is not presented as optimal or as improved clear performance. Last-decision global context is observational; actual action events determine later interruptions. No numeric, affix or default-setting changes were used to compensate for results.

Raw evidence is `Artifacts/Validation/EdictShield/Baseline24/` and `AfterFinal24/`, with paired checks in `comparison.json`. Collection started at 2026-09-12 17:24:51 UTC and 17:40:20 UTC, taking approximately 201.98 and 214.96 seconds. The first 24 after-runs remain as separate reproduction evidence.

## Validation and executable

Added 49 M05 tests and two diagnostic configuration/non-interference tests. The 268-test focused run and 125 tests covering final corrections passed. The final **1,652-test** Edit Mode suite passed without failures or skips at 2026-09-12 17:40:15–17:45:03 UTC, taking **287.28 seconds**.

Initial fixtures incorrectly treated unlock level 15 as the resource cost and omitted LC02 charge lifetime. They were corrected to the real zero cost and a valid charge with actual LC02 equipment. The first full suite exposed unnecessary Mage-record allocation on other classes and an old aiming test that assumed unconditional M05 use. Allocation is now Mage-only; the aiming fixture uses valid ENGAGE while retaining its assertion that a non-aimed shield produces no aiming telemetry. Earlier compilation/test failures remain archived.

| Native macOS stage | Verified behavior |
|---|---|
| Initial process | Actual UI callbacks edit moment, stacking and encounter policies. Draft, in-memory and freshly read file state agree. ENGAGE prepares despite a blocked legacy row at zero cost, then survives EMERGENCY/AFTER/BOSS/OFF edits and saving. |
| First restart | Original time, preparation, resource and opening state restore. Actual Willpower/MP04 scaling produces one shield. Real hazard damage absorbs 10% of creation maximum HP without reward, then saves partial absorption. |
| Second restart | Partial absorption and creation threshold restore. More actual damage reaches 15%, grants 20 resource and a four-second cooldown once. Further damage does not pay again; OFF/re-enable cannot repeat the encounter opening. |
| Screens | Text height, translations and footer safe areas pass at 1280×720, 720×1280 and 640×360 in Korean/English, including 140% text. Closing effects after language/scale changes retains running and manual-pause states. |

After fixing the first native attempt's clipped English title and pause-state issue, all three `Native2` processes passed at 2026-09-12 17:41:11–17:41:37 UTC. Twelve screenshots are retained; enlarged English portrait options, small landscape explanation, Korean partial absorption and English paid feedback were visually reviewed. Long content scrolls. These use controlled fixtures and automatic UI callbacks; mobile devices, physical touch and natural-play balance remain unverified.

Executable: `Builds/macOS-EdictShield/HELLSCRIPT.app`. Evidence: `Artifacts/Validation/EdictShield/`. The 251 C#/English-localization inputs, including 177 runtime C# files, match the final full suite, build and after-diagnostic sources. No C# meta files are missing and no asset GUIDs are duplicated. Baseline diagnostics use `7313034` with only the same two editor-only observer extensions.

`validated-source.tar.gz` archives 668 source, authored-setting, design-input and baseline-observer files. SHA-256: `fd6b38575f31cff4837d556e31e127618537cc97bcc8e273bb118af25445b655`. Unity-added material metadata in the build copy is recorded separately. Original scenes, prefabs, packages, settings and unrelated in-progress wiki-tool changes are preserved. Validation uses isolated projects and dedicated saves.

## Remaining work

Next is independent M06 Frost Nova logic for emergency control, crowd damage, chain-charge preparation, positioning and boss policies. Keep the specified survival configuration fixed while investigating pre-boss time, first-target misses and actual control/charge links. Growth/farming value, remaining legendary/set equipment boundaries and mobile/release quality follow. This completed phase does not complete the game or its balance validation.
