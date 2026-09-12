# Affix distribution validation and paired stage-20 experiments

Date: September 12, 2026 · [한국어](Affix_Audit_Expansion.md)

Status: **Audit tools, distribution validation, and 600 combat runs per profile are complete. Tuning and natural-progression validation remain.**

## Implementation

Use the integrated Claude revision `92d5812` as the baseline for the affix-dilution and stage-20 backlog. This phase changes Editor diagnostics and documentation; runtime weights, combat values and save formats remain unchanged.

The former balance report grouped death and timeout under one cause and could call a portal interruption a timeout. It now distinguishes victory, hero death, time limit, abandonment, other failure, navigation blockage, portal interruption, pause and diagnostic tick limit. Nonterminal runs have a separate `incomplete` count. Aggregate clear time and DPS still cover winning runs only, stated in schema version 2.

The new per-run audit stores map identity, a hash of initial enemies and encounter RNG/boss identity, eight equipped items, 57 sheet values, cumulative skill records, boss arrival and remaining HP, gate state and recent logs. Failed equipment operations stop the audit. Slot-local gear streams preserve later base choices when a different affix pool consumes a different number of random draws. The report refuses mismatched maps, enemies, equipped bases or unique items.

The distribution runner calls production `ItemGenerator.Create`. Its independent calculation enumerates weighted group selection without replacement, using slot eligibility, class weights and feasible prefix/suffix allocations. It does not call the production allocation or selection helpers.

Owned source files are `BuildIntegrationValidation.cs`, new `BuildIntegrationValidation.BalanceAudit.cs`, `AffixDistributionValidation.cs`, two Editor test files and `tools/analyze_affix_audit.py`. New C# stays in the existing Editor/test assemblies. No scenes, prefabs or packages change.

## Distribution result

Each catalogue generated **3 classes × 8 slots × 4 rarities × 100,000 samples = 9,600,000 items**, at item level 30. Both profiles total **19,200,000 items**, including common items with zero affixes.

All **12,864 affix, allocation and tier checks passed**. Affix and allocation probabilities use items as the denominator; tiers use actual rolls. Checks use a two-sided Bernstein bound with per-check alpha `1e-8`, while impossible and certain events require exact counts. This is a diagnostic criterion for fixed pseudorandom samples, not a proof of RNG independence.

Examples below are for **Warrior legendary items**. Values describe whether one item contains the affix, not the chance to drop legendary rarity or the affix's rolled magnitude.

| Slot / affix | Legacy24 expected / observed | Current54 expected / observed |
|---|---:|---:|
| Weapon / Strength AF11 | 52.65% / 52.53% | 43.49% / 43.15% |
| Weapon / Critical Strike Chance AF16 | 50.11% / 50.36% | 23.04% / 23.19% |
| Chest / Maximum Life AF01 | 38.10% / 38.23% | 16.76% / 16.62% |
| Boots / Movement Speed AF22 | 75.00% / 74.97% | 53.26% / 53.09% |

Existing affix inclusion rates decrease. Individual new weights of 35–55 do not preserve old marginal probabilities when groups and candidates are added. Sampling matches the current definitions; no incorrect generator selection was found. Combat results and the value of new defensive, healing, conditional-damage, progression and farming affixes must inform any tuning.

Current boots have a third prefix group, making legendary P2S2/P3S1/P1S3 feasible at 60%/20%/20%. Legacy boots lacked P3S1 and normalized the other two to 75%/25%. The itemization and attribute specifications now explain these distinctions and remove the unsupported claim that lower individual weights prevent dilution.

## Paired combat protocol and completed results

Both isolated projects retain current 57-stat calculations, maps, unlocks and combat. Only the comparison copy removes the 30 AF25–AF54 definitions. This edit never enters main or the player app. It isolates the expanded affix pool rather than comparing the entire game before and after Claude's attribute implementation.

Fixtures use level-30 heroes, eight legal item-level-30 pieces, zero enhancement, existing recommended behavior and the recorded set configurations. Existing new-run initialization and natural death/timeout progression remain intact. No HP, damage, duration or position overrides force a win. Runs use fresh accounts and never write player saves.

The pilot used six builds × two seeds: 12 runs per profile. Maps, initial enemies and 96 equipped slots matched. Current54 produced four clears, one death and seven timeouts; legacy24 produced six clears, two deaths and four timeouts. Neither profile had incomplete or navigation-blocked runs. These small samples validate the tool, not game difficulty.

**All six builds × 100 seeds completed: 600 runs per profile.** Current54 exited successfully at 14:40:02 UTC on September 12, 2026; legacy24 at 14:38:28 UTC. Both headers report `status=complete` and `completedRuns=600`. The final analyzer verified 600 matching map/encounter pairs and 4,800 matching equipment-slot pairs. Neither profile had incomplete runs, portal interruptions, or navigation blockage.

Each cell lists **clears / deaths / timeouts**. Every build/profile has 100 runs.

| Build | Current54 | Legacy24 | Clear-rate difference |
|---|---:|---:|---:|
| Warrior · Whirlwind Survival | 59 / 0 / 41 | 64 / 0 / 36 | -5 pp |
| Warrior · Leap and Crush | 47 / 0 / 53 | 51 / 0 / 49 | -4 pp |
| Ranger · Piercing at Distance | 25 / 3 / 72 | 31 / 2 / 67 | -6 pp |
| Ranger · Venom Ambush | 35 / 19 / 46 | 40 / 21 / 39 | -5 pp |
| Mage · Frost Field | 22 / 1 / 77 | 24 / 1 / 75 | -2 pp |
| Mage · Chain Control | 0 / 22 / 78 | 2 / 23 / 75 | -2 pp |
| Total, 600 runs | **188 / 45 / 367** | **212 / 47 / 341** | **-4.0 pp** |

Overall clear rates are 31.33% and 35.33%. Of matched pairs, current54 alone won 29 and legacy24 alone won 53. The net change primarily appears as more timeouts. Wilson 95% intervals are 27.75–35.15% and 31.61–39.24%. The exploratory paired McNemar two-sided p-value is 0.0106 overall; each individual build's p-value is at least 0.21. This is not a multiplicity-adjusted release criterion or a class ranking.

Mean initial maximum HP is 1,320.99 versus 1,419.27, and base damage 85.09 versus 87.09. Conditional damage, defense, healing, resources, and paths also contribute, so these two averages do not identify a single cause. Of current54's 367 timeouts, 308 occurred after the boss spawned. Chain Control spawned the boss in 45 runs in each profile and won zero versus two; investigate its actual skill uptime and time use separately from affix-wide tuning.

The experiment used validated `c0708b5` source, existing recommended rules, and Hunt Edict disabled. It is not a balance measurement of the subsequent [Fireball core policy](Edict_Fireball_Expansion.en.md). No affix weights or damage numbers were changed from this result alone.

Fixed level-30 fixtures cannot measure the progression benefit of experience affixes. One set configuration per build does not establish class rankings, rare-gear progression or all legendary combinations. The historical 12-to-6 stage-20 clear count is not reused as current integrated performance.

## Validation and reproduction

| Check | Result |
|---|---|
| Focused Editor tests | 32 passed, covering outcome classification, reproducible legal equipment, known group probabilities, allocation boundaries and statistical checks. |
| Full Editor regression | **1,422 passed**, zero failures or skips, September 12, 13:46:51–13:51:08 UTC. The whole suite was rerun after restoring a design-reference file omitted from the first validation copy. |
| Report analyzer | The real pilot comparison and five negative/mathematical boundary checks passed. Duplicate cases, running samples, mismatched enemies and duplicate equipment slots are rejected. |
| macOS player | Reran the attribute smoke on the existing AttributeIntegration app; passed with ten screenshots. Visually checked English at 140% text. This is not a newly built app. |
| Source and metadata | All 166 runtime C# files match the previous app's validated source. All 230 C# files in the audit copy match the working source; no missing C# metas or duplicate GUIDs. |

Evidence lives under `Artifacts/Validation/AffixBalance/`. Completed distribution evidence is in `distribution-summary.json` and `Full/*/cells.jsonl`. `Pilot/` and `pilot-comparison.json` contain the pilot. Expanded `runs.jsonl` files and headers contain the completed experiment. `comparison.json` is the final paired report; `completion.json` records successful exits, timestamps, and the report hash. `legacy24-only.patch` records the comparison-only change, and `setup.json` records source hashes, paths and baseline revision. `audited-source.tar.gz` preserves 631 source, settings, design and analyzer files, with individual and archive hashes verified. Initial compiler failures and the missing-design-file attempt remain preserved separately.

The Unity entry point is `Hellscript.Editor.BuildIntegrationValidation.RunAffixAuditSuite`. Supply `-hellscriptAuditOutput`, `-hellscriptAffixProfile`, `-hellscriptAuditStage`, `-hellscriptAuditSeeds` and `-hellscriptDistributionSamples`. The declared profile must match the compiled catalogue, and existing output files are never overwritten.

To reproduce the completed comparison, use `python3 tools/analyze_affix_audit.py <current54 output> <legacy24 output> <new report.json>`. Released balance, physical mobile performance and natural progression remain unverified.
