# Escape skill unlocks at level 5

Updated: 2026-10-06 · [한국어](Escape_Skill_Unlock_Level.md)

This implements A1 of the puzzle tutorial plan. Ranger Retreat Leap (A04) and Mage Teleport (M04) remain locked at level 4 and open at level 5. Warrior Leap Slam (W02) remains available at level 1. This stage alone does not implement the fifteen-level puzzle tutorial.

## Owners and generated outputs

Update the unlock values in `Docs/Design/ClassSkills/catalog.json`, `GameCatalog.cs`, `legacy_definitions.json` and `tree-design.cjs` together. Existing generators produce class documents, `ClassSkills.json`, the HTML tree catalog and `ClassSkillTree.json`. Move both movement roots to the first band while retaining descendant levels and prerequisites. `ProjectBuilder.SyncSkillUnlocks` serializes the Unity catalog through the Editor, preserving metadata and asset references.

Saved point allocations, equipped skills and edicts are preserved. F06 appends the escape skill at level 5 while keeping A02/M02 first in the recommendation order. Tutorial equipment history can exhaust F06; later puzzle escape training therefore needs its own lesson. Historical level-10 verification remains evidence of its original state.

## Validation

Before implementation, the five added boundary checks produced 1 pass, 4 failures and 0 skips, confirming missing level-5 access. Unity 6000.6.0f1 focused Edit Mode coverage (`GrowthTests`, `PlayerTrainingTests`, `SkillTreeIntegrationTests`, `TutorialProgressionTests`) passed **187/187**, with no failures or skips. The HTML tree engine passed **42** checks and shared UI validator tests passed **11**. Skill data/catalog/options/reports generators and native tree checks passed.

Check the existing skill data, catalog, options and reports generators; rebuild the HTML and native skill trees and run the planner engine checks. Repeat generation to confirm no further source changes. Run the shared UI contract checks.

This stage does not run the full Edit Mode suite, a macOS player build, runtime smoke tests, the browser player or physical mobile devices. Focused unlock/data checks do not establish an exhaustive early-rift balance result.

## Delivery and artifacts

Deliver branch `codex/escape-skill-lv5` as a PR. This execution does not merge the PR or publish the public wiki; the merger handles deployment from integrated main.

Collect task evidence under `Artifacts/PuzzleTutorial/20261006/` and record exact paths, sizes, retention and cleanup in `artifact-lifecycle.json`. Materialize only the needed legacy wiki evidence as read-only files and preserve originals. Keep the checkout and reports for open PR review and review retention on 2026-10-13. No permanent deletion is authorized.

[Focused XML](../../Artifacts/PuzzleTutorial/20261006/stage1-focused.xml) · [Pre-change boundary XML](../../Artifacts/PuzzleTutorial/20261006/stage1-red.xml) · [Validation summary](../../Artifacts/PuzzleTutorial/20261006/stage1-validation.json)
