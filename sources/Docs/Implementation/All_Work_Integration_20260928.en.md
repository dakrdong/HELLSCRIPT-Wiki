# All Worktree and Branch Integration — 2026-09-28

Updated: 2026-09-28 · [한국어](All_Work_Integration_20260928.md)

The user requested merging every HELLSCRIPT worktree and branch into `main`, committing and pushing. This integration preserves completed features, ongoing art work and pending documentation. It extends the [September 27 integration](Feature_Integration_20260927.en.md) to include work excluded there.

## Scope and preservation

- The starting point is `origin/main` at `97ed8510`. The initial inventory contained 36 registered worktrees: 35 existing directories and one registration whose directory was already absent. There were 222 unique unmerged commits.
- Features include equipment recommendations and replacement storage, equipment variants, combat-log height, rift results with support contributions and incoming-damage rankings, ElevenLabs audio, NPC portraits/dialogue, power-saving hunts, attendance, the skill tree and map display preferences.
- Rendering, lighting, shaders, import/rig infrastructure, six fields, twenty enemy types, five bosses, procedural model recipes/assets and the effects library are also included. Existing asset provenance and approval states are preserved.
- Pending combat-journal edits and the Unity Pipeline `0.8.0-exp.1` update in the original checkout were committed separately. Pending documentation in another active checkout was preserved in a snapshot commit without changing that checkout's files or index.
- A verified Git bundle, dirty-file copies, binary patches and SHA-256 manifests are retained outside the repository. No branches or worktrees were deleted. See the [target inventory](AllWorkIntegration20260928Evidence/branch-inventory.json).

## Conflict resolution

All 62 initial unmerged references (33 distinct tips) are ancestors of the integrated revision. Later completed art/telegraph verification documents are included. An unfinished enemy attack-motion follow-up started after the snapshot remains in its own checkout and backup reference, outside this validation.

The latest rift statistics and existing equipment transaction improvements are both retained. Shared windows retain input restoration, gameplay-blocking and bottom-docked NPC dialogue options. Power-saving entry, minimap and boss layouts are combined. Power saving pauses every imported music voice, including when the audio lifecycle resumes, and clears stale camera shake.

Wiki validation now reflects sixty equipment bases, twenty ordinary enemies and five bosses together. Duplicate translation keys were resolved against their actual call sites. History merging preserves distinct bodies from both sides without duplicate bodies. Automatically expanded Unity metadata/settings were restored after confirming unchanged GUIDs.

## Validation

Text and controls shrink together. Potions, skills, seal and vitals retain one composition, with potions beside the skills. The live journal is at the bottom; expanding/collapsing translates the complete HUD by the height change. Power saving has equal columns, a center divider and compact one-line gear changes. Claimable attendance rewards have green light, and event access uses a calendar/chest icon. See [responsive HUD details](Responsive_Hud_20260928.en.md).

![Bottom journal in a small window](AllWorkIntegration20260928Evidence/journal-small-compact.png)

![Expanded journal moves the HUD upward](AllWorkIntegration20260928Evidence/journal-landscape-expanded.png)

![Attendance event shortcut](AllWorkIntegration20260928Evidence/attendance-event-icon.png)

Full Edit Mode: 4,430/4,479 passed, with 49 failures. Forty-seven match the baseline. Two new OperationsRewardTests failures hard-coded schema 18; use the current version constant and pass that fixture. Preserve the [comparison](AllWorkIntegration20260928Evidence/full-suite-summary.json); this is not an all-green full-suite claim.

Subsequent UI/attendance/reward/telegraph tests passed 162/163. The remaining attendance icon failure was a cubemap import; explicitly enforce 2D and pass all 7 attendance-art tests on rerun. Earlier HUD/power-saving tests passed 75/75 and shared UI contract regression passed 9/9. The journal matrix checks 48 combinations: 440×956, 956×440, 1600×900, 1600×1000, 2100×900, 512×288; Korean/English; 100%/140%; collapsed/expanded. HUD size and spacing remain constant while its position moves.

Final counts, source revisions and runtime results are recorded in the [validation manifest](AllWorkIntegration20260928Evidence/validation.json). No MCP Editor instance was connected, so verification uses the established Unity 6000.6.0f1 batch-test and native macOS development-build paths. All runtime saves are isolated.

The integrated build exercises power saving/restart, victory/utility/defeat and details, map display, attendance, skills, NPC dialogue, the art gallery, audio and equipment handling through their relevant smokes. Existing `main` failures are compared separately from integration regressions.

## Remaining scope and publication

Some procedural models have recipes and assets included while their production-view wiring remains future work. Merging a library or capturing its gallery does not establish complete runtime adoption or art approval. Previously unfinished features are not treated as completed solely because their branches were merged.

Synthetic macOS pointer, viewport and safe-area checks are distinct from physical-mobile touch, battery and thermal acceptance. No physical iOS/Android validation was performed.

The public wiki is built and checked from merged `main`, then synchronized as a complete read-only mirror. Source push, successful Pages deployment and logged-out in-app-browser verification are separate checks. Public site: [HELLSCRIPT Wiki](https://dakrdong.github.io/HELLSCRIPT-Wiki/#/tree).
