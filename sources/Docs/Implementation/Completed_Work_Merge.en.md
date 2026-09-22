# Completed work and image-resource integration

2026-09-22 · [한국어](Completed_Work_Merge.md)

The integration combines the remaining skill design/runtime work, image-production results and Aspect Runestone HTML prototype with the existing `main` implementation of the runestone, blacksmith and shared UI. Original working folders and uncommitted artwork remain untouched.

## Included work

| Work | Source commit |
| --- | --- |
| Skill design, use policies and gated combat implementation | `0dea5e6` and its design ancestors |
| 108 skill icons | `4ad9a97` |
| 141 legendary equipment icons | `6ffaf4b` |
| 105 set pieces and 24 set emblems | `7417c9f` |
| 123 aspect emblems | `62f42e2` plus 195 outstanding working files |
| Aspect Runestone HTML prototype | `b7a830c` |
| Equivalent class-set design history | `267269d`; already integrated content retained |

The audit checks all 33 local `codex/` branch tips and remote work branches against the integrated history. Older dirty source and validation folders were compared with existing integration commits; current behavior was not replaced by older copies.

## Release state retained

Skill `playerEnabled=false` remains unchanged. New abilities, level-40 progression and additional equipment retain their existing development-validation gates. This merge does not activate their general release or add their UI integration.

All 501 existing images retain their original pixels and candidate status. Their generation model remains `unknown`; merging does not grant release approval or screen adoption. Of 141 planned aspect emblems, 123 exist and 122 passed visual review. `LM29` remains visually rejected and 18 new equipment-linked emblems remain ungenerated. The user confirmed there are no other completed files elsewhere. See the [production handoff](../Art/Skill_Resource_Delegation.en.md).

## Integration adjustments

- Keep save schema 9, aspect collection, equipment enhancement and slot growth while integrating class-skill persistence. Loading a class-skill account must not downgrade its schema to 7.
- Retain both aspect levels and new skill effects in combat snapshots, movement and damage calculations.
- Preserve design-time source fingerprints. Validation recognizes the already integrated effective-rank ceiling and appended equipment bases while continuing to reject changes to original numbers and definitions.
- Give the two README production briefs distinct wiki addresses while retaining existing addresses and history.
- Replace the link to an unproduced aspect contact sheet with an explicit pending entry, and preserve historical test reports.

## Verification

Verified with Unity 6000.6.0f1 on macOS 26.6.2. The full suite and subsequently added focused regression are separate runs; their counts are not combined.

| Check | Result and evidence |
| --- | --- |
| Full Unity Edit Mode regression | [3,568 passed, zero failed or skipped](CompletedMergeEvidence/completed-merge-full-editmode.xml) |
| Class-skill, aspect and slot-growth save compatibility | [One separate regression passed](CompletedMergeEvidence/completed-merge-save-editmode.xml). Two reload/save cycles retain schema 9, the selected ultimate, 16 aspect copies at level 5 and slot level 8. |
| Native macOS build | [Succeeded with zero errors](CompletedMergeEvidence/native-build.txt) |
| Combat checkpoint and restart | [Checkpoint creation](CompletedMergeEvidence/native-skills/native-checkpoint.txt) and [fresh-process resume](CompletedMergeEvidence/native-skills/native-restart.txt) passed. Seven cases compare damage, resources, cooldowns, RNG, events, rewards and policy progress with uninterrupted execution. |
| Aspect Runestone | [Transactions, collection, manual upgrades and equipment changes](CompletedMergeEvidence/native-aspects/initial.txt), plus [restart restoration](CompletedMergeEvidence/native-aspects/resume.txt), passed. |
| Blacksmith | [Enhancement, auto-reroll, slot growth, saving and layouts](CompletedMergeEvidence/native-forge/result.txt) passed. |
| Inventory | [Equipping, comparisons, salvage, drag, saving and layouts](CompletedMergeEvidence/native-inventory/result.txt) passed. |
| Static validation | Shared UI contract and nine related tests; 21 HTML prototype tests; skill design, runtime, policy and comparison-report checks passed. |
| Wiki | Build, links, ten Python tests and UI/database/read-only checks passed. |

The validation app and integration share the same [437 runtime and data file hashes](CompletedMergeEvidence/native-source-manifest.json). Combat comparison data was regenerated from the integration, and policy interpretations were corrected where observations changed from the isolated branch.

The Aspect Runestone checks cover 20 layouts: Korean/English, 100%/150% text, portrait 440×956, landscape 956×440, and PC 16:9, 16:10 and 21:9. Fourteen synthetic pointer interactions execute in the actual macOS app with before/after state assertions. Archived examples show [Korean portrait at 150%](CompletedMergeEvidence/native-aspects/layout-ko-150-440x956.png) and [English landscape at 150%](CompletedMergeEvidence/native-aspects/layout-en-150-956x440.png). This does not establish physical mobile touch or performance acceptance.

## Post-merge cleanup scope

Four folders with uncommitted changes were initially retained. The user then confirmed no work was in progress and requested complete cleanup. All 598 remaining file entries were rechecked: 555 matched main or its earlier history; the rest were corrected document links, unused legacy translations, an older wiki generator, validation-project settings and Unity-generated differences. No additional feature implementation remained to merge.

Those files, patches, validation artifacts and builds were backed up outside the repository before all temporary worktrees and work branches were removed. The primary project checkout now uses current `main`, and HELLSCRIPT retains only `main` locally and on GitHub. The independent public-wiki deployment checkout was also moved out of the temporary worktree directory to the adjacent `HELLSCRIPT-Wiki` folder.

This follow-up cleanup changes no game code or data. The full-suite and macOS results above were obtained with the same runtime and data; they are not counted as a second full regression run for cleanup.
