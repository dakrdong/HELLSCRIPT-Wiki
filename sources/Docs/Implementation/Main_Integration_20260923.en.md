# Development branch integration — 2026-09-23

Updated: 2026-09-23 · [한국어](Main_Integration_20260923.md)

The initial inventory covered 19 local feature branches and 17 worktrees, starting from main `31caa29b`. Previously merged work is retained. The integration includes all commits and supporting evidence from the remaining branches below.

## Included work

| Feature | Branches and initial heads |
| --- | --- |
| Rift preparation, fatigue, equipment changes and first-clear placeholders | `codex/rift-entry-runtime` · `9497599d`; `codex/rift-entry-html` · `11d705b2` |
| Hunt edict skill trees | `codex/class-skill-tree` · `a1f5913d` |
| Blacksmith core crafting | `codex/blacksmith-cores-runtime` · `3138a5d3`; `codex/blacksmith-core-prototype` · `9e41f415` |
| Shared buttons and warehouse chest tabs | `codex/button-ux` · `a884a983` |
| Six gems and retired Skull migration | `codex/six-gems` · `5a33ecef` |
| Jeweler potion art, provenance and comparison preview | `codex/jeweler-potion-art` · `f797d119` |

The other 11 branches covering equipment art, inventory potions/currency, item details, jeweler, rune layouts and town HUD were already ancestors of the initial main. [Integration evidence](MainIntegration20260923Evidence/validation.json) records every branch head and inclusion status. Candidate art retains its original adoption and provenance boundaries.

## Reconciliation

- Preserve the fourth core-crafting tab and the new skill tree while applying shared selection, pressed and focus feedback. Jeweler and rift preparation lists also pass explicit selection state.
- Keep six gem types and six tiers with the current jeweler's **5:1 upgrade, 1:5 downgrade and no gold cost**. Update stale 3:1 descriptions and native fixture expectations. Remove the retired Skull purchase action and calculate the portal-cleanup fusion shortage against five gems.
- Use integrated save schema **11**. Rift and core crafting independently used schema 10; the new boundary prevents either older client from dropping the other feature's records. Existing balances, history and progression are preserved.
- Add a regression covering crafting history/pending result, both fatigue pools, restore count, rift best time/claim state and retired-gem migration in one save. Confirming the crafting result and restarting must not charge again.
- Normalize new heroes' potion metadata at creation so a restart does not mutate other heroes' saved state. Preserve the existing stock grant and activation timing. Use Unity-aware object checks in inventory, item details and storage, and declare the required `CanvasRenderer` on both custom graphics.
- Merge localization by key. Preserve document history in chronological order with duplicate body hashes removed. Re-export the HTML core catalog from the integrated equipment definitions.

## Original work

The main worktree had 408 pre-existing image metadata changes with no overlap with integration paths. Their hashes and patch were captured for comparison after updating main. No other worktree contained uncommitted code or documents. Importer/project-setting churn created by Unity in validation worktrees is excluded from feature commits.

## Validation

[Integration evidence](MainIntegration20260923Evidence/validation.json) records the final results and scope separately for the full Edit Mode suite, focused integration checks, macOS development build, native interaction and fresh-process restoration using isolated saves. uGUI raycasts and synthetic pointer/keyboard input do not constitute physical-mobile proof.

- Full suite on initial integration `8a54d626`: 3,770 tests, 3,766 passed and 4 failed. Failures involved required graphic components, Unity object checks and fresh potion metadata initialization.
- After correcting those issues, **243 related tests passed with zero failures or skips**, including all four previously failing tests. This is not reported as a full 3,770-test rerun on the corrected source.
- Earlier focused integration and UI reconciliation runs passed 346 and 139 tests respectively. Overlapping runs are not summed.
- All 231 HTML prototype tests and 9 shared UI contract tests passed. Wiki generation/parity checks, 13 Python tests and document/database route checks are performed separately.

The final corrected macOS development build succeeded with zero errors. **All 13 native interaction/restart processes passed**, covering rift, core crafting, buttons, skill trees, jeweler, shared UI, potions and six gems. Layout coverage includes 440×956, 956×440, 1600×900, 1600×1000 and 2100×900, Korean/English and 100%/150% text. Individual features have different scope; each report records its actual actions and layout combinations. Existing Unity API, some shader and shutdown memory warnings remain.

- [Portrait rift entry](MainIntegration20260923Evidence/rift-entry-440-ko-100.png) · [Landscape English at 150% text](MainIntegration20260923Evidence/rift-entry-956-en-150.png)
- [Tier 1,000 first-clear placeholders](MainIntegration20260923Evidence/rift-rewards-tier-1000.png) · [Core crafting result](MainIntegration20260923Evidence/core-portrait-crafted-result.png)

Historical feature evidence remains in [rift preparation](Rift_Entry.en.md), [core crafting](Core_Crafting.en.md), [buttons](Button_UX.en.md), [skill trees](Hunt_Edict_Skill_Tree.en.md), [jeweler](Jeweler_Runtime.en.md) and [six gems](Six_Gem_Catalog.en.md). Integration results are recorded independently; overlapping historical counts are not summed.

## Public wiki

After integration into main, build and check the wiki and deploy its complete read-only mirror, including documents, history, databases, images and source evidence. Retain date-only display and the absence of write actions.
