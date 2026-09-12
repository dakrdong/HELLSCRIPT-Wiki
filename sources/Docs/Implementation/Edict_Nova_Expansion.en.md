# Frost Nova hunt edict implementation

Date / 작성일: 2026-09-13 · [한국어](Edict_Nova_Expansion.md)

Status: **M06's four core policies, physical area, walking approach and actual control/chain effects are connected. All 1,705 tests, three native processes and the 48-run before/after comparison are complete.**

## Integration and scope

Claude's attribute, affix and balance commit `b1f3ea5` is included in main through merge `92d5812`. This phase starts from `76cf5b0`, which also includes the subsequent shield work. After fetching the remote, Claude's commit was verified as an ancestor of `origin/main`. The 57 attributes, 54 affixes and existing saves remain in scope; damage values and affix weights were not tuned.

With the edict enabled and M06 equipped, the simulator now derives one independent candidate from its four options. Missing, disabled, blocked or duplicated legacy rows cannot override that candidate. Turning the edict off restores the saved legacy behavior. Mage active skills M01–M06 now have independent core policies; basic attacks and the remaining policies of other classes are separate work.

## Four connected options

| Option | Runtime behavior |
|---|---|
| Automatic use | ON admits the candidate; OFF excludes it. Edits and save restoration do not repay or replay an already committed preparation. |
| Purpose | CONTROL requires a new control opportunity plus a global emergency/dodge request, or a dangerous cast that can actually be interrupted in time. GROUP requires at least two victims in the real self-centered area. CHAIN_CHARGE requires actual SMB4, an equipped/unlocked/automatic M03, zero remaining charges or at most one second of lifetime, and at least one raw-damage victim. |
| Placement | HERE checks the current 3m area. APPROACH walks to a valid position within the currently observed engagement and starts casting only after entering the physical area. |
| Boss policy | PURPOSE counts bosses normally. STAGGER_ONLY excludes bosses already staggered or immune from the decision. HOLD_ALONE waits when all victims are bosses, then reevaluates the purpose when normal enemies are also present. |

CONTROL checks existing stun/freeze lifetime against the 0.35s preparation, remaining enemy cast time, actual threat geometry and interruptibility. Healing casts and boss summons also qualify when interruptible. Bosses must be neither staggered nor immune and must reach 100 control with this cast's contribution of 15 to count as a new interruption. They never receive normal Freeze.

CHAIN_CHARGE does not wait for M03 cooldown or resource. Missing, disabled or locked M03 falls back to CONTROL while preserving the source selection. Missing actual SMB4 blocks the reference. Direct global M06 defense still respects its purpose and boss policy, additionally requires new control at the current position, and never starts emergency approach movement. M05-specific override exceptions do not apply to Nova. Ordinary GROUP and CHAIN_CHARGE cannot cancel a busy attack; CONTROL follows FINISH/CANCEL_ALLOWED and the current action phase.

## Physical range and finite movement

Candidates come from currently observed, visible and pursuit-eligible enemies. A distant global priority target cannot prevent a valid cast against other nearby victims. Decision-only boss filters do not remove physical damage victims at release. Actual damage and control remain separate operations over the original self-centered area.

APPROACH examines positions near enemies and between close pairs, checking terrain, body spacing, global path danger, pursuit distance and remaining pursuit time. Conditions are reevaluated while walking. Movement does not consume resource or start cooldown, extend the radius, or teleport.

The additional-engagement restriction uses recent remembered observations outside the current encounter. It never reads the current coordinates of hidden enemies and cannot guarantee avoidance of undiscovered groups. Danger checks follow the configured global policies.

A selective `excludedReaches` record preserves M06's 3m rearm threshold after pursuit exhaustion. A target inside the Mage's 10m basic reach must not immediately restart an exhausted Nova chase. Existing position, target, pursuit budget and this exclusion survive restart. Older saves without the optional record retain their previous reach behavior.

## Equipped effects and restoration

The unchanged values are unlock level **20**, preparation **0.35s**, base resource cost **20**, cooldown **10s**, cold coefficient **1.0**, self radius **3m** and normal-enemy Freeze **1.5s**. Actual discounts are consumed once at preparation start. If every victim leaves before release, Nova records a real miss without refunding cost or cooldown.

A real raw-damage hit with SMB4 equipped grants two chain charges for six seconds. Each subsequent M03 preparation reserves one charge and adds two hops. MP06 requires actual cold followed by lightning damage to start its four-second buff. A live LC02 discount is consumed once by a paid cast. Restoration never duplicates payment or effects, and Nova does not force a later Teleport or Chain Lightning.

The native fixture equips generated SMB1–4 and LC02 items with MP06. It starts with resource 100, pays a live LC02-discounted Nova cost of 10, edits all four Nova options and M03 automatic use through actual UI, and saves during preparation. An independent restart releases one Nova and verifies cold hits, Freeze and two charges. The ordinary M03 decision then pays its actual SMB2 cost of 21.25 and reserves one charge. Another restart verifies real lightning and MP06. A separate approach fixture verifies free walking followed by one cost-20 Nova inside the true radius.

## Fixed-policy comparison

The preceding M05 final 24 runs are reused as the baseline. Its archived manifest matches 253 C#, English-localization and package inputs from `76cf5b0`. The preserved evidence is reused explicitly; it is not represented as a new baseline execution.

Each version uses eight legacy, eight fresh-default edict and eight configured-survival runs: 48 comparative runs. Seed indices 0, 1, 2, 3, 4, 5, 9 and 14 were selected from earlier outcomes, using level-30 Chain Control, stage 20 and eight legal equipped items. Paired maps, encounters, objectives, full equipment values, stat sheets, HP, base damage and complete edict documents match. M03 starts, releases, hits, interruptions and 50ms observation totals reconcile with actual records.

Fresh defaults disable global emergency and dodge responses. The diagnostic-only survival document enables HP 30%, lethal danger and return HP 50%; defense M05, escape M04 and defense/escape/walk order; OTHER_SKILL fallback M05; ground/area/direct DAMAGE thresholds of 15%; and CANCEL_ALLOWED. Sixteen explicit assignments produce 13 actual differences from defaults. WALK_FIRST, HP 40% potion, preservation OFF and other options remain unchanged. M06 remains CONTROL/HERE/PURPOSE in both edict documents; legacy mode has no edict. Neither purpose nor order was changed to improve observed outcomes.

| Before → after | Legacy, eight runs | Default edict, eight runs | Configured survival, eight runs |
|---|---:|---:|---:|
| Clear / death / timeout | 0/4/4 → 0/4/4 | 0/7/1 → 0/7/1 | 0/0/8 → 0/0/8 |
| Runs spawning the boss | 4 → 4 | 2 → 1 | 5 → 3 |
| M03 starts / interruptions / first-target misses | 538/28/6 → 538/28/6 | 363/3/1 → 321/2/2 | 568/8/7 → 565/68/7 |
| M03 hits | 1,761 → 1,761 | 1,428 → 1,317 | 2,152 → 1,815 |
| M06 starts / releases / hits | 69/62/226 → 69/62/226 | 62/61/212 → 59/58/213 | 95/95/384 → 110/109/382 |
| M04 / M05 starts | 149/41 → 149/41 | 0/0 → 0/0 | 46/34 → 62/44 |
| Mean pre-boss phase time | 227.39 → 227.39s | 163.45 → 151.27s | 253.01 → 271.21s |
| Resource-blocked checks | 0 → 0 | 0 → 0 | 0 → 0 |

This table uses the completed final-source `AfterFinal24` runs, which also reproduced all first-after result summaries. Default edicts have the same death count but fewer boss spawns and chain hits. CONTROL still permits interruptible dangerous casts when global survival is disabled, so zero global requests do not imply zero Nova casts.

Of 68 configured M03 interruptions, 52 are actual `edict:M06` replacements. The remainder are seven global M05 replacements, six global walks, one pull and two time-limit events. Both versions avoid death but time out in all eight cases, while boss spawns fall from five to three. This is not a performance improvement or an optimized preset. Follow-up work must separate the cost of cancelling preparation for real control from other pre-boss delays.

Mean pre-boss time includes cases ending in death or timeout before reaching a boss. Shorter default runs therefore do not establish better travel. Last-global-decision context is observational; actual action events establish subsequent interruption causes. Placement is fixed to HERE, so these runs do not measure APPROACH balance.

## Validation evidence

The 51 new M06 cases cover independent candidates, real range, purposes, bosses, interruption gates, pursuit and observations, actual equipment, paid-action restoration and planner noninterference. The existing M03 integration fixture now explicitly selects GROUP for a valid Nova while M03 automatic use is off.

The focused 280 tests and first full suite of 1,703 passed. The first focused failure checked MP06 before actual lightning release and was corrected. The first native restart harness omitted the normal resume call; that was restored. The second native run found a product issue: changing language during preparation left the previously composed action text in Korean. HUD rendering now translates the stored action at read time; two tests verify language switching without changing the saved run. Failed evidence is retained.

The final **1,705 Edit Mode tests** passed without failures or skips from 2026-09-12 18:34:31 to 18:39:19 UTC, taking **288.47 seconds**. The final macOS build succeeded. All three `Native3` processes passed from 18:38:29 to 18:38:55 UTC.

The run retains **13 screenshots**. Following the screen-layout specification, it uses 1280×720, 720×1280 and 640×360, Korean/English and 140% text, checking draft isolation, text height and footer safe areas. Large portrait options, small landscape guidance, actual charges, translated preparation, elemental crossover and before/after approach screens were visually inspected. Long content scrolls. Controlled enemies and automated UI callbacks are not mobile-device, physical-touch or natural-play balance validation.

Final diagnostic collection started at 2026-09-12 18:34:29 UTC and completed 24 runs in **235.92 seconds**. Raw evidence is in `EdictShield/AfterFinal24/` and `EdictNova/AfterFinal24/`; paired results are in `EdictNova/comparison.json`. The first after-run and failed initial native attempts remain separately available.

The executable is `Builds/macOS-EdictNova/HELLSCRIPT.app`; evidence is under `Artifacts/Validation/EdictNova/`. Isolated Unity projects and dedicated saves preserve source scenes, prefabs, packages, settings and the user's unrelated wiki-tool changes.

All **254 C#/English-localization inputs**, including **179 Runtime C# files**, match the final test, native-build and diagnostic projects. There are no missing C# meta files or duplicate asset GUIDs. `validated-source.tar.gz` retains **673 files**, including validated inputs, authored settings, sources and baseline provenance. Its SHA-256 is `a44ed370b52e9c2d6d6f9d051b599e14aafc7caf052cb013f37e883c42914018`. Unity-added material metadata in validation copies is recorded separately.

## Remaining work

Next, investigate actual M06 control replacements and M03 preparation cost under the fixed survival document, then connect the Mage basic attack's remaining fixed policies independently. Other-class policies, growth/farming utility, legendary/set equipment boundaries and mobile execution quality remain open. This phase does not complete the game or its balance validation.
