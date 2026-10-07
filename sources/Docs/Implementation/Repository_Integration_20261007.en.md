# Worktree and branch integration, 2026-10-07

Updated: 2026-10-07 · [한국어](Repository_Integration_20261007.md)

The integration inventories 32 local branches and 10 worktrees and brings completed work into one main branch. Local main initially pointed to `05a7a568`; origin/main was `47f98fbb`. The user confirmed that the uncommitted Hunyuan/N13 work was complete and requested its inclusion.

## Integrated changes

- Includes the fifteen-level puzzle tutorial, two maintenance lessons, graduation, replay, reward persistence and Warrior survival changes. See [final tutorial acceptance](Puzzle_Tutorial_Final_Acceptance.en.md) for the existing feature and validation scope.
- Incorporates legacy v2 numeric potion guidance and related acceptance. Potion checks distinguish legacy presets from current-account direct controls; section-description checks exclude the retired equipment option. The newer pointer stabilization and current puzzle acceptance remain.
- Keeps the retired v3 acceptance file deleted. Its investigation and evidence are preserved as historical records; the deleted-source link points to its immutable commit.
- Preserves the original HTML mockup art, provenance and evidence while retaining the current fifteen-level prototype, growth curve and checks. Conflicting wiki history is deduplicated by body hash.
- Includes the delivered N13, N02, N12 and BOSS03 models, metadata, import tools and shared WorldView routing. The 241 untracked original files and tracked patch were separately preserved and hash-verified before committing.

## Validation

The [verification summary](RepositoryIntegrationEvidence20261007/summary.json) records source commits and actual results.

- Full Edit Mode: 5,614 passed, zero failed, two Explicit measurement tools excluded.
- macOS Development player: Warrior KO portrait and Mage EN landscape completed 30 level victories, two L7 process restarts, two graduations and two replay victories. Hub checks covered five viewport shapes, KO/EN and simulated safe areas.
- Quick presets, potions and section settings passed five viewport shapes, KO/EN, pointer, save and process-restart checks. The initial run exposed an outdated legacy/current-account expectation; only the affected native smokes were rerun after correcting the development harness. Production game logic was unchanged, so the full suite and tutorial results were reused.
- Shared UI contract: 15 checks passed; UI refresh static checks passed.

Player input uses synthetic EventSystem pointers and accelerated actual fixed combat ticks. Normal-rift acceptance covers five seconds at ordinary speed plus portal return. This does not establish physical-mobile behavior, human comprehension, complete ordinary-speed rift victories or performance improvement.

Wiki generation stopped once because the merged v3 document history referenced a deleted source file. The original file from its historical commit is now retained as a document attachment and old links resolve to it. Historical bodies, dates and hashes remain unchanged; a regression checks the frozen source hash and keeps unrelated missing links as errors.

Native Editor checks cover all four models through the real WorldView owner, LODs, shared skeletons, walking, attacks and default camera framing. BOSS03 extends beyond the viewport at the existing minimum 50% camera distance. Default framing passes; model scale and camera policy are preserved. This controlled presentation check is distinct from complete gameplay, performance and physical-mobile acceptance.

## Preservation and cleanup

The main checkout retains the Git recovery bundle, original patch and untracked sources, review Markdown, test reports, necessary captures and raw evidence, binary hashes and reproduction commands. All eight existing stashes remain. Logical directory size is reported separately from actual free-space recovery. Worktrees and branches are removed only after merge and activity checks. Global shared caches and the user's Unity and Claude processes are excluded.
