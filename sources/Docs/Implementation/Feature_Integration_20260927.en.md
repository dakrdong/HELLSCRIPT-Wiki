# Rift victory, equipment and audio integration

Updated: 2026-09-27 · [한국어](Feature_Integration_20260927.md)

The user requested merging and pushing completed work to main. Five branches were combined in an isolated integration checkout. This is a separate follow-up to the [earlier main integration](Main_Integration_20260927.en.md).

## Included work

| Feature | Source branch | Source commit |
| --- | --- | --- |
| Rift result and shared item detail popup | `codex/rift-victory-preview` | `7912623a` |
| Auto Equip, displaced equipment and warehouse handling | `codex/recommended-equipment` | `bc71ab3f` |
| Thirty equipment variants, drops and combat properties | `codex/equipment-variants` | `cb7a51bb` |
| Final sound effects and music, retired asset cleanup | `codex/elevenlabs-audio` | `23317191` |
| Combat log capacity based on available height | `codex/combat-log-height` | `43b11224` |

The rift result retains four normal skills plus one ultimate, scrollable loot grouped by source, salvage/discard indicators, combat records, repeat cancellation and reward claiming. Item clicks use the same `ItemDetailPopup` and `ItemDetailView` as inventory. Edit Hunt Edict opens the existing skills tab.

## Integration repairs

- Shared item details retain both gear score and material metadata. Duplicate translation keys were removed while preserving both features.
- Auto Equip retains a copy of the item as acquired. Subsequent equipment or enhancement changes do not rewrite the reward snapshot.
- Salvage, sale, warehouse storage and automatic sales that make warehouse space update rift disposition records. The existing `GameStore` transaction owns these actions and rolls back acquisition and disposition together if saving fails.
- `EquipmentScore` version 2 includes fixed armor, dodge, shield block and other new intrinsic properties using the existing affix reference for the same stat. Fixed properties and affixes each contribute once.
- Both branches' shared UI document histories remain intact; the merged body receives its own revision.
- Save schema 19 preserves existing schema 18 accounts and ensures older builds reject the new format instead of overwriting rift snapshots or warehouse ordering.
- Rift UI now uses the shared Unity object null-check rules. The eight-slot balance fixtures share equipment generation that supplies an attacking main hand instead of a standalone offhand. Production drop rules are unchanged.

## Exclusions and preservation

The 17 in-progress Claude branches under `claude/diablo-art-overhaul`, `claude/art-*` and `claude/overhaul-*` are excluded. They form the active 3D overhaul, including models, rigs, shaders, lighting, associated fields/bosses and production tools. Their branches and working copies are preserved.

The six uncommitted files in the primary checkout exactly matched the completed combat log branch. Patches, file copies and hashes were backed up under the local `Artifacts/MainIntegration20260927/` directory. Branches already contained in main and duplicate detached checkouts require no additional merge. This request does not include deleting branches or worktrees.

## Validation and publication

The initial full Edit Mode run had **4,214 tests: 4,164 passed, 50 failed, none skipped**. Forty-seven failures were reproduced on unchanged main `c751d84d`. The three additional failures concerned the edict option count, Unity object null checks and balance-fixture equipment. After repairs, **all 311 targeted tests passed**. This is not a claim that the entire suite was rerun clean.

Evidence includes the [full XML](FeatureIntegration20260927Evidence/editmode-full.xml), [current-main reproduction](FeatureIntegration20260927Evidence/baseline-current-main.xml), [failure comparison](FeatureIntegration20260927Evidence/full-baseline-comparison.json) and [311 passing targeted tests](FeatureIntegration20260927Evidence/editmode-final.xml). The full-run source hashes are in [source evidence](FeatureIntegration20260927Evidence/full-suite-source.json); final scopes are in the [validation summary](FeatureIntegration20260927Evidence/validation.json).

The macOS development build succeeded with zero build errors. Eleven execution/restart flows passed on isolated accounts: rift results, previous records, salvage/discard status and shared details; Auto Equip and warehouse transactions/reload; equipment variants; shared inventory/storage/shop/forge details; range preferences and temporary Ctrl display; audio routing/settings reload; and combat logs. Per-feature matrices cover 440×956 portrait, 956×440 landscape, PC 16:9/16:10/21:9, Korean/English and enlarged text. See the exact scopes in [runtime results](FeatureIntegration20260927Evidence/native-results.json).

Shared UI ownership, nine contract tests, seven audio-tool tests and the 30-variant equipment audit passed. The isolated UI fixtures unlock content and suppress attendance popups. They wait for guide discovery before comparing complete account data for read-only actions. The forge-detail fixture advances the existing town route to prepare the real NPC access gate, as the existing town/shop fixtures do.

No live Unity MCP instance was available, so validation used Unity batch tests and a standalone macOS development build. This does not establish physical-mobile touch/performance acceptance or human listening approval.

The full read-only wiki is generated and validated from merged main for deployment to the [HELLSCRIPT public wiki](https://dakrdong.github.io/HELLSCRIPT-Wiki/#/page/feature-integration-20260927.en). Deployment success, actual document/DB contents and logged-out read-only behavior are checked separately.

![Integrated rift result](FeatureIntegration20260927Evidence/rift-pc.png)

![Salvaged equipment uses the same shared detail popup](FeatureIntegration20260927Evidence/shared-item-detail.png)


## Final result-page refinements

All five skill cards are noninteractive and show two damage shares: normal monsters and bosses. Each category has its own all-source denominator. Zero damage shows 0%; older records without a complete target split show “No record”. Outgoing damage owns the target classification, and cumulative statistics survive event retention and save/restore.

Inventory and rift details share `ItemDetailPopup`, including footer layout and measurement. A kept item places source/status and Close in one row, reducing the logical footer height from 94 to 50. Disposal explanations add their measured height only. Failed rifts use the same result route with a dark red panel, large failure heading, failure mark and actual termination reason. Best times are preserved; the center action opens the existing failure analysis. No new raster artwork was introduced.

After these refinements, all **147 focused Edit Mode tests** passed. Five native paths were rerun: victory, persisted restart, failure, common item polish and affix ranges. This brings the distinct integration smoke flows to **11**, including earlier unchanged-feature evidence. Victory and failure each passed 20 native viewport/language/text-scale combinations; the HTML failure preview passed 20 layout combinations. Native checks use topmost uGUI raycasts and whole-account immutability checks, not physical-device input. See the [focused XML](FeatureIntegration20260927Evidence/editmode-result-refinements.xml), [native reruns](FeatureIntegration20260927Evidence/native-refinements.json) and [HTML layout evidence](FeatureIntegration20260927Evidence/html-refinements.json).

![Failed rift](FeatureIntegration20260927Evidence/rift-failure-pc.png)

![Compact shared footer](FeatureIntegration20260927Evidence/kept-item-detail.png)

## Shared focus restoration

`ContentWindowHost` preserves the original input modality when returning from a shared popup. Pointer-opened slots regain selection without a keyboard focus border; keyboard navigation keeps its visible focus. Rarity borders are unchanged. Nested-window restoration is covered by [42 passing focused Edit Mode tests](FeatureIntegration20260927Evidence/editmode-focus-restoration.xml), followed by [victory and restart reruns](FeatureIntegration20260927Evidence/native-focus-restoration.json). Native macOS pointer input opened the kept-item detail and clicked Close, confirming that no inner focus border remained. See the [pointer evidence](FeatureIntegration20260927Evidence/native-pointer-status.json).

![Result after closing the shared item detail with the pointer](FeatureIntegration20260927Evidence/native-pointer-result-return.png)
