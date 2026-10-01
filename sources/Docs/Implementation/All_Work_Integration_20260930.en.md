# All worktree and branch integration and cleanup — 2026-09-30

Updated: 2026-10-01 · [한국어](All_Work_Integration_20260930.md)

The user asked to merge every worktree and branch into `main`, commit and push, and then delete the remaining worktrees and branches. The [2026-09-28 integration](All_Work_Integration_20260928.en.md) kept its worktrees and branches. This time they were all deleted after the merge.

## Scope

- The starting point was `origin/main` at `20169eaf`. There were 11 worktrees including the main checkout, 11 branches and 3 open pull requests (#34, #36, #39).
- No worktree other than the main checkout had uncommitted changes. The main checkout held 31 `.meta` files rewritten by Unity and an untracked `.DS_Store`. No HELLSCRIPT session was running.
- Six branches had commits that `main` did not have, and all six were merged. The other five (`claude/ecstatic-rhodes-26a7bd`, `claude/training-ground-mockup`, `claude/tutorial-prologue`, `codex/edict-resume-preset-fix`, `codex/storage-chest-art`) were already in `main`.

| Branch | Change | PR |
| --- | --- | --- |
| `claude/vibrant-curie-9bd918` | F06 "Equip a new skill" counts a new active after the first skill | two commits also in #39 |
| `claude/vigilant-hamilton-483e0d` | Tutorial design tables match the guide list in the code | #39 |
| `claude/musing-sammet-6561e3` | Bag fix for legacy rift checkpoints with empty slot levels | none |
| `claude/zen-ptolemy-d5d109` | Class-skill generator checks run in CI | #36 |
| `claude/elated-raman-3af2c9` | First-row actives at level 1, new accounts start without skills, the resource basic attack keeps attacking | none |
| `claude/ui-style-overhaul` | Shared UI restyled as Obsidian & Gilt, with a colour check | #34 |

## Conflicts and fixes during the merge

- `Wiki/history/tutorial-progression.json` and its English copy conflicted. The skill-start branch and the F06 branch had each added an unpublished revision 6. The merge kept `main`'s history and let the wiki build record one new revision for the merged record. The public wiki was published from `20169eaf`, so no published revision was lost.
- The tutorial record now holds both branches' paragraphs. The skill-start branch's paragraph describes the level-10 W04 fixture. A sentence now says that the rule changed later the same day, so the paragraph does not read as the current rule.
- After the merge, the shared UI contract check failed. The colour check from the UI overhaul caught six colours in prologue code that reached `main` after that branch last merged it: `f3d9a0` four times in `GameUI.TutorialStaging.cs`, and `262116` and `0f0d09` in `PrologueGate.cs`. They now use the nearest tokens, `GoldBright` (`f2d9a4`), `Raised` (`211e19`) and `Background` (`100f0d`). Each channel differs from the original colour by at most 5. The tutorial smoke captures after the merge show the prologue's light and narration plate drawn correctly; they were not compared side by side with earlier captures.
- The main checkout's 31 `.meta` files went in as a separate commit. The 26 training ground textures gained the Standalone/WebGL caps that `ResourceTextureBudget` sets (256 for portraits, 2048 for the yard) when they were imported. The 5 character model metas differ only in a trailing space after empty values. Every GUID is unchanged.

## Validation

- The shared UI contract check and its 9 tool tests passed. The four class-skill generator checks, the skill tree prototype rebuild (no diff) and the 39 engine tests also passed.
- The wiki build and check, the 11 wiki tests and the route checks passed. Compared with the public copy, there are 2 new pages (UI style, Korean and English), 23 changed pages and 3 changed databases (skills, class-abilities, resources). No page was removed. Every difference comes from the merged branches.
- On a macOS development build of the merged result, all 11 runtime smokes passed: Tutorial (two processes), SkillTree, SkillReset, SkillMenu, SkillPreset, EdictSections, QuickPreset, ClassSkill, Attendance, LegacyRunBag and UiStyle. In the tutorial, F06 completed at level 4 with W02 and no level fixture, and the Ranger and Mage started without skills.
- The full Edit Mode suite ran 4,742 tests: 4,695 passed and 47 failed. The 47 failures match the known-failure list recorded on 2026-09-27; none is new and none was fixed.
- Physical mobile devices were not tested. Web and APK were not rebuilt.

## Cleanup

- Before anything was deleted, every ref was saved to a bundle outside the repository: `~/HellscriptBackups/hellscript-all-refs-20260930.bundle`, SHA-256 `bb1973b591a5d89eae11d20b3950dfa06149ea3501cf94f694bb1097aee40c36`. `git bundle verify` passed. The ref and worktree lists are in the same folder.
- 9 worktrees were removed, including 2 Codex worktrees. 12 local branches, including the integration branch, and 10 remote branches were deleted. Each was checked to be in `main` right before it was deleted. Only the main checkout and `main` remain. Pull requests #34, #36 and #39 became merged with the push to `main`. The worktree of the session that wrote this record is removed last, after the public wiki is published.

[Integration inventory](AllWorkIntegration20260930Evidence/inventory.json) · [Smoke results](AllWorkIntegration20260930Evidence/smokes.json) · [Full Edit Mode comparison](AllWorkIntegration20260930Evidence/full-editmode.json) · [Cleanup result](AllWorkIntegration20260930Evidence/cleanup.json)

## Remaining branches and worktrees — 2026-10-01

The optimization, forge, Sites and daily-quest worktrees created after the September 30 integration were inventoried again. The fixed daily goals were in main game commit `a1edd320` at the start. Every one of the seven remaining work branches was an ancestor of that main commit, with no unmerged source changes.

Five older worktrees, six local branches and two actual remote branches were removed. Their `Artifacts`, `Builds`, `Logs` and `UserSettings` were moved or cloned into a local archive outside the removed worktrees. All commits remain reachable from main. The final daily-validation branch/worktree is removed last, after preserving its results and corrected documents and publishing the wiki. [Inventory and completed cleanup](DailyQuestFixedEvidence20261001/cleanup.json).

A new uncommitted forge edit began in the original checkout during cleanup, and a new `codex/edict-simple-skill-actions` worktree appeared. Those files and both new work branches are preserved. The [fixed daily-goal validation](Daily_Quests.en.md) uses the isolated, committed `a1edd320` snapshot; it does not claim to validate the concurrent changes.

The concurrent forge change was committed separately as `41f77e38`. These validation documents are integrated into main alongside it; the final full daily-quest run remains scoped to the documented `a1edd320` source. Uncommitted changes in the new skill-actions worktree remain preserved.
