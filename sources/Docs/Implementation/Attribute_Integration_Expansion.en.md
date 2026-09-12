# Integrating Claude's attribute changes

Date: September 12, 2026 · [한국어](Attribute_Integration_Expansion.md)

## Merge and scope

Claude's `worktree-d4-stat-sheet` was already merged into `main` at 21:07:30 KST on September 12. At that point main ended at `b1f3ea5`, including attribute implementation `02d9d04`, balance report `39e2a3c`, wiki changes and the remaining backlog. That history was preserved and integrated into `codex/claude-review-20260912`.

The reviewed integration is commit `92d5812`, pushed to both main and the development branch, with local main fast-forwarded as well. Follow-up distribution checks and paired experiments on current rifts are recorded in the [affix audit](Affix_Audit_Expansion.en.md).

On September 13, local and remote main were verified at `f6f80ab`, including Claude commit `b1f3ea5`. At the user’s request, the fully merged `codex/claude-review-20260912` branch was deleted locally and remotely, and the working checkout switched to `main`. All 29 existing and in-progress uncommitted files retained identical status and content hashes. Only `main` remains on the remote.

The combined development version includes:

- Claude's 57 attributes, 54 affixes, character sheet and combat/progression calculations, preserving saved indices 0–23.
- [Rift objectives, rewards and result presentation](Rift_Integration_Expansion.en.md), physical gate passage, and hunt-edict targeting, pursuit and collection policies.
- Completed content-unlock commit `046354a`, separating account access, current encounter stage and each hero's clear record.
- Stage-dependent rarity commit `166106a` and fixed, item-level-dependent reroll costs.

Resolve boss-settlement overlap while retaining gold bonuses, per-character first-clear rewards, current-stage rarity draws and content reconciliation after rewards. The wiki reads 57 attributes, 54 affixes and nine unlock conditions together. Preserve historical status-page bodies from both branches.

## Corrections

| Issue | Result |
|---|---|
| The sheet displayed 4m/s as 4%. | Show actual metres per second on the sheet; retain percentage bonuses for equipment and affix comparisons. Cover all three classes, bonuses and the movement cap. |
| Folding rows or changing language could leave combat paused after returning. | Capture pause state on entry to growth/attributes and retain it through redraws, navigation and recommendation cancellation. Preserve a deliberate manual pause. |
| HUD and combat overview still assumed a resource maximum of 100. | Use the actual maximum for both text and fill; 70/140 produces a half-full bar. |
| The English attribute subtitle clipped at 140% text size. | Shorten the English description within the existing layout and validate enlarged text. |

Follow the existing header, scrolling body and footer structure from the [screen layout specification](../Design/HELLSCRIPT_Screen_Layout_Detail.md). Open **Growth and Skills → Attributes** from town or combat. Do not replace scenes, prefabs or packages. Exclude incidental material serialization from Claude's commit while retaining the existing material and GUID.

## Validation

| Check | Result |
|---|---|
| Full Unity Editor suite | **1,391 passed**, zero failures or skips; September 12, 12:46:54–12:51:10 UTC |
| Included coverage | 29 attribute, 37 unlock, 28 rarity, 26 objective and ten stored-text cases, plus existing combat, equipment and persistence coverage |
| macOS app | **Nine launches/restarts passed**: one attribute, five objective, one rarity, one result and one unlock scenario |
| Screens | **47 screenshots**; Korean/English at 720×1280, 640×360, 1280×720 and 1920×1080, plus 140% text on the attribute page. Not every language/size combination. |
| Source/assets | All **227 C# files** unchanged after the full suite; authored assets compared with the shared project; no missing C# metas or duplicate GUIDs. |

Reproduce movement units, resource capacity, pause restoration and enlarged-text clipping in the pre-fix app. Remove one unnecessary translation-table entry after the first combined suite, then pass the full final suite. Preserve failed evidence under `BeforeFix` and `FirstCombinedTests`.

Preserve the [final summary](../../Artifacts/Validation/AttributeIntegration/validation-summary.json), [source/application manifest](../../Artifacts/Validation/AttributeIntegration/final-source-manifest.json), [archived Unity project](../../Artifacts/Validation/AttributeIntegration/validated-project.tar.gz) and [integrated macOS app](../../Builds/macOS-AttributeIntegration/HELLSCRIPT.app). Capture Unity's generated material-version metadata separately and retain the authored file after checking that material properties and its GUID are identical. Run Unity against an isolated copy and preserve pre-existing project-setting differences and the separate wiki task's uncommitted work.

## Balance evidence and remaining work

Adopt [Claude's original report](Attribute_System_Expansion.md), covering six configurations × 30 seeds × four stages. Its stage-20 result of 12→6 wins and affix-dilution hypothesis remain investigation items. The original measurement artifacts are absent here, and subsequent map/unlock changes alter the comparison. Do not treat those historical figures as measurements of this combined build. These checks establish functional behavior and regressions, not final difficulty.

Baseline dodge and the 3% direct-hit overpower roll exist without equipment. Correct the earlier description that all new stats are neutral to acknowledge these exceptions. Do not retune combat values or affix weights without evidence.

Keep crowd-control duration reduction and both stamina indices defined but excluded from equipment rolls. Actual gems/sockets, long-term economy/difficulty, physical mobile touch/performance and server synchronization remain separate work.
