# Rift objectives and rewards integration

Date: September 12, 2026 · [한국어](Rift_Integration_Expansion.md)

## Scope

Validate [essence carriers and offerings](Objective_Chains_Expansion.en.md) together with the completed [stage-dependent rarity changes](Rift_Rarity_Expansion.en.md). Carrier deaths, offering collection, gate opening and objective history must work alongside normal, elite, boss and sweep rewards and fixed reroll costs.

Restrict the death-analysis action to a failed rift with a dead hero. Alive returns, live timeouts and training results do not offer it. Preserve the existing victory rule when the boss and hero die in the same tick.

Native validation also found that saved combat logs did not switch to English. Results and history now preserve each timestamp and event identifier while translating the message through the existing table and format templates. Keep stored numeric precision and original record bytes unchanged. Unknown text remains readable and is reported as missing. Reverse translation of older English-authored records into Korean is outside this correction.

## Ownership

While preparing validation, the separate task “콘텐츠 공개·해금 순서 구현” began changing the shared project. Its unfinished content-unlock code and data remain untouched and outside the completion claim. Claude's completed attribute/balance changes are tracked below and are not yet included in this application.

Build the validation project from the archived objective snapshot, apply the completed rarity commit `166106a`, then apply this result-screen correction. Check the patch before applying it and compare unchanged objective generation/persistence sources by hash. Do not revert other work in the shared project. Integration here means objectives, rewards and result navigation; it does not mean the entire mutable project, including the active unlock task, is complete.

## Validation status

| Check | Result |
|---|---|
| Final complete Editor suite | **1,325 passed**, zero failures or skips; September 12, 12:13:33–12:17:40 UTC |
| Included cases | 26 objective cases, 28 rarity cases, ten stored-text cases, plus existing gate, loot and pursuit coverage |
| Native launches/restarts in one app | **Seven passed**: five objective phases, one rarity scenario, one terminal-result scenario |
| Screenshots | 30; Korean/English at 1920×1080, 1280×720, 720×1280 and 640×360; not every combination |
| Terminal results | Alive return, live timeout, real hero death, training death and simultaneous boss/hero death victory; open and return from actual death analysis |
| Source audit | All 215 sources unchanged after the full suite; zero duplicate GUIDs or missing C# metas; all application files hashed |

Preserve the first native localization failure and earlier 1,315-case suite under `BeforeLogLocalization`. The final results above include the correction.

Preserve the [integrated validation macOS app](../../Builds/macOS-RiftIntegration/HELLSCRIPT.app), [final summary](../../Artifacts/Validation/RiftIntegration/validation-summary.json), [source/application manifest](../../Artifacts/Validation/RiftIntegration/final-source-manifest.json) and [archived Unity project](../../Artifacts/Validation/RiftIntegration/validated-project.tar.gz). The shared project also contains active unlock changes, so this is not a final build of the whole live checkout.

Objective map generation is unchanged from the previous 10,000-map test. That evidence does not validate the active unlock task's theme/enemy introduction rules. Physical mobile touch, sustained device performance and long-term economy/difficulty remain separate work.

## Claude handoff and main merge

The user confirmed completion of Claude's balance work and requested a merge into main. Repository evidence shows that `worktree-d4-stat-sheet` was **already merged into main on September 12, 2026 at 21:07:30 KST**. At that verification point, main ended at `b1f3ea5` and contained both attribute implementation `02d9d04` and balance report `39e2a3c`. Do not create a duplicate merge. Preserve [commit ancestry and reflog evidence](../../Artifacts/Validation/AttributeIntegration/branch-integration-status.json).

Include these items in the development scope:

- 57 attributes with saved indices 0–23 preserved; derived sheet values and damage, defense, recovery and resource integration.
- 54 affixes, slot-specific prefix/suffix allocation, the attribute screen and Korean/English text.
- Claude's reported six configurations × 30 seeds × four stages, and the remaining stage-20 affix-distribution investigation. Raw measurement artifacts are absent from this checkout; the report is not new validation evidence.
- Attribute/affix wiki changes and open items already present on main.

The shared checkout remains on `codex/claude-review-20260912`. Inclusion in main does not prove integration with that branch's ongoing changes. A separate review copy resolves overlapping reward code and translation entries. Movement-speed units and combat pause restoration after attribute-screen redraw are candidates to reproduce and correct next. See the [attribute review record](../../Artifacts/Validation/AttributeIntegration/review-plan.json).

## Subsequent integration

The later [attribute integration](Attribute_Integration_Expansion.en.md) incorporates Claude's changes and completed content unlocks, passing 1,391 tests and nine native launches/restarts. The 1,325-case suite and seven launches above remain evidence for this earlier objectives/rewards/results snapshot.
