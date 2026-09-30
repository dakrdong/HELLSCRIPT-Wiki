# Work integration and repository cleanup on 2026-09-30

2026-09-30 · [한국어](Repository_Integration_2026_09_30.md)

The training-ground rework, rift-entry layout, blacksmith HTML prototype and town blacksmith access during an unfinished rift were merged into current `main`. Already integrated rift results, equipment art and live skill previews were also checked against branch history.

## Merge decisions

- Preserve removal of the player text-size preference and enlarged-text-only acceptance. Current viewport tests use the default text size; historical enlarged captures remain historical evidence.
- Add the training edit button to shared `RiftSkillShareView` while retaining rift-result contribution metrics. Keep both content texture budgets and both sides of shared UI history.
- Retain the newer rift-result repeat icons and section styling rather than reverting to the older result prototype carried by the entry branch.
- An unfinished saved rift no longer prevents opening the forge beside the town NPC. Actual combat, proximity, service unlock, ownership, cost and atomic-save guards remain.
- The blacksmith HTML page remains a prototype. Integration does not replace native service unlock tiers with prototype values.

## Preservation and cleanup

The inventory contained seven worktrees and eight local branches. Every feature tip was verified as an ancestor of integrated `main`. All Git refs, dirty/untracked files, and removable worktrees' builds, saves, evidence and settings were preserved outside the repository. The 15,787 preserved files were hash-verified against their originals. Regenerable Unity caches and public wiki exports were excluded; existing builds and saves in the primary checkout remain.

[Integration evidence](MainCleanup20260930Evidence/integration.json) records original branch tips and conflict resolutions.

## Final validation

| Check | Result and scope |
| --- | --- |
| Shared UI contract | Checker passed; all 11 contract tests passed |
| Full Edit Mode | One run: 4,878 tests, 4,830 passed, 48 failed, none skipped |
| Integration regression | Removed four identical duplicate English translation keys. Parsed translations are unchanged; the affected focused test passed |
| Baseline comparison | Only the remaining 47 failed cases were run on pre-integration `47091ff3`; all 47 also failed. No second full run |
| macOS development build | Succeeded with zero build errors; existing licensing and development-build diagnostics are preserved separately |
| macOS runtime | Eight checks passed: blacksmith, rift entry/restart, portal/restart, training and rift result/restart |
| Viewports and locales | Default text size, Korean/English, 440×956, 956×440, 1600×900, 1600×1000 and 2100×900 |

The full NUnit XML contains every test and the final end time. The CLI wrapper subsequently returned its 900-second timeout; this is not reported as a successful CLI exit. Both the [complete report](MainCleanup20260930Evidence/editmode.xml) and [baseline failure comparison](MainCleanup20260930Evidence/failure-classification.json) are retained. The full suite is not green because of these baseline failures.

The rift-entry restart helper also read the obsolete character chest field. It now checks the actual save owner, `account.rewardBoxes`. Only that failed independent-restart check was rerun and passed; successful runtime checks were reused. Translation cleanup removes identical duplicate values without changing the parsed runtime translations.

Blacksmith acceptance covers a fresh character, an unfinished saved rift, and NPC dialogue after an actual portal return: 32 access cases, 96 UI raycast clicks and 10 paid, persisted enhancements that preserve the saved rift. [Runtime results](MainCleanup20260930Evidence/native-results.json) and [blacksmith interaction evidence](MainCleanup20260930Evidence/blacksmith/blacksmith-access.txt) are retained. Physical mobile devices were not tested.

Six redundant local worktrees and seven feature branches were removed, leaving only the primary `main` checkout. Three existing stashes remain preserved and separately backed up. Remote feature branches are removed only after integrated `main` is confirmed pushed. The public wiki is generated, checked and published from merged `main`, including this record and its evidence.
