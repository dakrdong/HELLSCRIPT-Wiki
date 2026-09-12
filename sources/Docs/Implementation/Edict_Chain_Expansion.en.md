# Chain Lightning hunt-edict implementation record

Date: 2026-09-13 · [한국어](Edict_Chain_Expansion.md)

Status: **all four M03 core options are connected; the full suite, a new macOS launch/restart and 16 selected diagnostic runs are complete.**

## Problem and resulting behavior

Chain Lightning's saved options affected aiming, but executable candidates still came from legacy behavior rules. An opted-in hero with M03 equipped now derives an independent policy from all four saved options. Missing, disabled or condition-blocked legacy rows cannot override the new policy. Duplicate rows yield one candidate without changing the saved rule list. Opting out restores legacy behavior.

M01, M02 and M03 follow the common attack order and display their own names in decision history. A valid first target outside casting range permits an approach; OFF cancels that approach. Unlock level, actual cost, cooldown, action locks and global survival responses retain their execution gates.

## Four connected options

| Option | Runtime behavior |
|---|---|
| Automatic use | OFF excludes new casts. Paid preparation retains its first target and reserved set bonus. |
| Purpose | SINGLE allows one valid first target. LINKED requires at least two distinct targets under the real linking rules. SAVE_CHARGE normally requires two, but permits one when an actual SMB four-piece charge exists and expires within one second. Without the set it falls back to LINKED while preserving the saved choice. No special wait-for-Nova state is introduced. |
| First target | ALL compares all valid candidates. ELITE prefers valid elite/boss candidates and SUPPORT prefers support enemies. If that category is absent or entirely outside first-cast range, other valid candidates are used. An insufficient chain within the preferred category cannot borrow another group's target count. |
| Chain criterion | UNIQUE maximizes distinct targets; TOTAL_HITS includes LM04 revisits. Candidates are ranked within the preferred first-target category, then by distance to the exact global target and by enemy ID. Without LM04, TOTAL_HITS falls back to unique targets while preserving the original choice. |

First targets must be alive, perceived and not excluded by global tracking. They pass the 10 m initial range and line-of-sight checks. Later links use currently perceived enemies under the actual chain rules; first-target exclusion does not grant immunity to subsequent links. Forecasts describe the current formation, so movement during preparation can change the real result.

## Linking, damage and charge boundaries

The next target must be alive, within 4 m of the previous enemy and visible from it. Unvisited enemies take priority, with distance and ID breaking ties. The base hit limit is four, MP03 adds one and an SMB charge reserved at cast start adds two, for a maximum of seven. Forecasts and releases share the limit calculation and next-target selector.

LM04 permits at most two visits per enemy but prohibits consecutive hits on the same enemy. A→B→A is valid. Each hop applies the existing 0.8 decay, and revisits additionally multiply damage by 0.5. LM04 does not increase the total hit cap. A lone boss receives only one hit even with MP03 and a charge.

An actual Frost Nova hit grants two SMB four-piece charges for six seconds. Chain Lightning spends one at preparation start and stores the two bonus hits in the pending action. Forecasting, held conditions, insufficient resources, cooldowns and level locks do not spend charges. SMB two-piece and LC02 discounts retain the 50% total cost-reduction cap.

Release now rechecks the first target's life, perception, initial range and sight. If it dies or moves beyond range or behind a wall, the cast records `ACTION_MISS` and deals no damage. It does not transfer to another enemy or refund the paid resource/charge. This correction also applies to legacy-rule casts. Subsequent links are reevaluated using the release-time formation.

## UI and persistence

The card explains the scope of the four settings and the charge-consumption boundary in Korean and English. `edict:M03` identifies the derived decision. Validation follows the [screen layout specification](../Design/HELLSCRIPT_Screen_Layout_Detail.md): scrollable content, accessible footer actions, enlarged text and compact landscape layouts.

The save schema, legacy rules, scenes, prefabs and packages remain intact. Changing the first-target preference or saving OFF during preparation does not rewrite a paid cast's target or reserved bonus. Validation uses isolated projects and dedicated saves.

## Scope of Chain Control diagnostics

The completed [affix comparison](Affix_Audit_Expansion.en.md) found poor Chain Control clear rates with both profiles. A new observer leaves combat stats unchanged and records time in preparation, cooldown, other actions, no perceived enemy, outside initial range and remaining states, plus actual casts, links and boss damage. Time buckets sample state after each 50 ms step; they are approximate occupancy, not a causal attribution of delay. The remaining-state bucket does not guarantee that every execution gate passes. A test verifies that observing a generated rift preserves its outcome and existing combat aggregates.

The stage-20 sample selects two previous cases from each category: death before boss spawn, timeout before spawn, death after spawn and timeout after spawn. Seed indices 0, 1, 2, 3, 4, 5, 9 and 14 are outcome-selected, not a random sample. Level 30 and eight legal equipped items are fixed; legacy rules and the current hunt edict use matched maps, enemies and gear. The edict treatment includes global policies, so differences cannot be attributed solely to M03. Current runtime changes also mean this is not a replay of the archived affix experiment's executable.

### Results from the 16 selected runs

Collection completed from 2026-09-12 15:57:47 to 15:59:41 UTC. Seeds, maps, initial enemies, objectives, complete equipment values, maximum HP, base damage and stat sheets matched within every pair and against the archived sample. Starts, releases and hits matched the existing combat aggregates; sampled durations agreed with elapsed simulation time within tolerance in every run.

| Measurement | Eight legacy runs | Eight current-edict runs |
|---|---:|---:|
| Clears / deaths / timeouts | 0 / 4 / 4 | 0 / 4 / 4 |
| Runs reaching boss spawn | 4 | 2 |
| M03 starts / interruptions / first-target misses | 535 / 29 / 6 | 551 / 78 / 13 |
| Actual M03 hits | 1,740 | 1,754 |
| Hits per release, including misses | 3.44 | 3.71 |
| Starts reserving a charge | 116 | 137 |
| Resource-blocked checks | 0 | 0 |
| Pre-boss time with no perceived enemy | 39.7% | 41.2% |
| Pre-boss time performing another action | 41.2% | 40.7% |

Connecting the options and consuming charges did not resolve poor outcomes. This sample supports investigating exploration, other actions and interruptions before resource starvation. No perceived enemy includes normal exploration, and another action is not necessarily an error. The four legacy runs reaching the boss spawned it at an average of 232.63 seconds, consuming much of the 300-second limit. The specified single hit against a lone boss remains intact.

Next, record interruption reasons alongside active threats and survival responses, and connect remaining core policies such as M04 while checking for unnecessary repeated preparation cancellation. Separate time spent reaching the boss from single-boss damage. The selected sample is not a population win-rate estimate; affix weights and damage multipliers were not retuned.

Raw data is `Artifacts/Validation/EdictChain/Diagnostic1/runs.jsonl`, with matched summaries in `diagnostic-summary.json`. The isolated-project entry point is `Hellscript.Editor.BuildIntegrationValidation.RunChainDiagnostics`, accepting `-hellscriptAuditStage`, `-hellscriptChainSeeds` and `-hellscriptAuditOutput`. Existing output directories are not overwritten.

## Validation and executable

The final **292-test focused run** and **1,566-test full Edit Mode suite** passed. The full suite ran from 2026-09-12 15:56:55 to 16:01:59 UTC in 304.19 seconds, with zero failures or skips. New coverage includes 51 M03 cases and one observer noninterference case. Integration includes an actual Frost Nova granting two charges, followed by two Chain Lightning casts consuming one each and producing seven hits apiece.

The first focused run passed 289 of 290 tests. Its single failure came from incompatible corridor data retained in a synthetic sight-blocking fixture. The fixture was corrected; actual Nova integration and observer coverage were added before the passing final run. The initial failed report is preserved.

| Native macOS check | Result |
|---|---|
| Editing and persistence | Actual buttons selected SAVE_CHARGE, SUPPORT and TOTAL_HITS. The saved source stayed unchanged while editing; memory and a fresh file read matched after applying. An actual button enabled the edict. |
| Charge hold and start | One uncharged target held the cast. Supplying a short-lived fixture charge and linked enemies selected a support first target and consumed one charge at preparation start. Actual Frost Nova charge generation was separately verified in Edit Mode. |
| Independent process restart | ELITE/OFF was saved during preparation before exiting. Restart restored the original first target, resource, time and reserved bonus, producing one seven-hit release. Each enemy received at most two nonconsecutive hits. No duplicate payment, extra charge consumption or new OFF-state cast occurred. |
| Layout and pause | Korean/English and 140% text were checked at 1280×720, 720×1280 and 640×360. Text-height, missing-translation and footer safe-area checks passed. Closing translated decision screens preserved running and manually paused entry states. |

The first native attempt exited because its new command-line flag collided with an existing rift-objective smoke flag. The new smoke now uses `-hellscriptChainEdictSmoke` and `-hellscriptChainEdictResume`. The final build and both `Native2` processes passed from 2026-09-12 15:58:14 to 15:58:34 UTC. Those two smoke flag names are the only difference between the full-suite/diagnostic source and the final app source; the variant is preserved explicitly. Combat and UI behavior did not change after the full suite began.

Ten screenshots were retained. Enlarged English options, compact landscape options, Korean restored decisions and the post-restart combat screen were visually inspected. Inputs use actual UI callbacks with controlled combat placement and time. This is not mobile-device touch, rotation or performance evidence.

The app is `Builds/macOS-EdictChain/HELLSCRIPT.app`, with evidence under `Artifacts/Validation/EdictChain/`. All 242 C#/English-table inputs, including 172 Runtime C# files, match the final build source. No C# metadata is missing and no asset GUID is duplicated. `validated-source.tar.gz` preserves 653 source/design-input/authored-setting files, including the old smoke-flag variant. SHA-256: `55cec1caeb98aa741755e80e16714679c1e000e213b22e13368e21b56090cbd2`. Unity-added URP Editor version metadata in the validation copy's material is recorded separately; the authored asset is preserved.

## Remaining work

Next: connect remaining fixed policies such as M04 and record the reasons for interrupted preparation. Investigate Chain Control's boss-arrival time alongside survival responses and interrupted attacks. Multi-stage progression and farming value, remaining legendary/set equipment boundaries, mobile execution and overall release quality remain open.
