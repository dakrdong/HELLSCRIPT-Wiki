# Awakened items, greater affixes and masterworking

Date: 2026-09-13 · [한국어](Item_Quality_Expansion.md)

Status: feature, persistence and native macOS UI checks passed. The 1,800-run combat comparison remains in progress; this record does not establish overall balance or completion of the game.

## Generation and combat values

The three layers in the [item quality specification](../Design/HELLSCRIPT_Item_Quality_Detail.md) extend the existing normal, magic, rare and legendary rarities. Rare-or-better rift items can awaken from stage 20 with probability `min(0.5, max(0, 0.02 × (R − 19)))`. Awakening multiplies the main base stat by 1.25 and raises the affix quality floor to 4000. Each awakened affix independently has a 10% greater-affix roll: quality 10000, tier T1, and 1.5 times the ordinary maximum value.

The shared monster/boss drop owner, generated chest equipment and sweep rewards receive their actual rift stage. Shop purchases and both crafting services do not roll awakening. Every new item-generation route caps item level at 60. Existing saved items above level 60 retain their level and values.

The main stat applies enhancement, awakening and masterwork multipliers once each. This covers weapon damage, armor-piece armor, necklace HP and ring resistance. Fixed secondary resistance and weapon attack speed do not receive the quality multiplier. Greater affixes and masterwork affix boosts are derived once from the unchanged stored raw roll.

## Blacksmith and equipment UI

Only enhancement +5 equipment can advance one masterwork level per transaction. The cap is `min(200, 12 + 3 × max(0, H − 30))`, where `H` is the account's highest actual clear. Every level multiplies the main stat by 1.02. Levels 4, 8 and 12 each choose one random affix for a 25% boost. Repeated selections add as `1 + 0.25 × hits`. Levels 13 and above grant no additional affix boosts.

The next level `k` costs `20 + 2k` materials and `200k` gold. Level 12 costs 396 materials and 15,600 gold cumulatively; level 200 costs 44,200 materials and 4,020,000 gold. Service transactions check ownership, an active rift, stale equipment quotes and durable save success. Retrying a failed masterwork save uses the same random choice for that request.

A reset costs `2,000 × max(1,I)` gold and returns the floored 80% of **actual masterwork material investment only**. It clears masterwork levels and affix boosts while retaining ordinary enhancement, awakening, greater affixes and gems. Later dismantling cannot refund the same masterwork materials again.

Equipment lists and details display an awakened name marker, a separate outline, greater-affix count and masterwork level. An awakened-only filter, quality-aware replacement stats, separate main-stat multipliers and actual affix ranges are connected. The blacksmith displays the current cap, next cost, distance to the next affix milestone and the already-boosted lines. Greater-affix rerolling warns about losing the greater bonus both during line selection and in the confirmation.

The UI reuses the existing portrait/landscape inventory layout and fixed confirmation footer from the [screen layout specification](../Design/HELLSCRIPT_Screen_Layout_Detail.md), with Korean and English text. This feature changes no raster image, model, scene, prefab or package.

## Persistence and recovery

Item data version 4 stores `awakened`, `masterwork`, `masterworkLines` and `AffixRoll.greater`. `investedMaterials` remains the combined enhancement/masterwork investment ledger. `masterworkInvestedMaterials` separately records actual masterwork investment so reset and dismantle refunds remain distinct.

Accounts containing quality data or version-4 historical equipment use account schema 3. The account-level guard protects historical quality even when every inventory and the warehouse are empty. Removing all equipment and history does not lower the schema. Valid legacy schema-1 and schema-2 saves remain readable.

Masterwork outside 0–200 or incompatible with the enhancement requirement is repaired to zero. Valid levels above the current progress cap remain intact, with further advancement blocked. Unknown boost slot IDs and excess boost entries are removed individually. Inconsistent greater-affix flags lose their greater effect while preserving the raw roll; invalid awakening conditions lose the awakening effect. Equipment itself is retained.

Before saving repaired data, the exact original is archived as `*.quality-recovery-*.json`. Concurrent gem repairs share a single archive using the existing `*.gem-recovery-*.json` name. Failure to archive stops loading. The same recovery covers owned gear, warehouse items, suspended drops/chests, pending repeat-hunt results and historical equipment copies.

## Completed validation and ongoing checks

All **2,442 tests** in the final-source [Unity Edit Mode suite](../../Artifacts/Validation/item-quality-editmode.xml) passed. The 62 quality-specific cases cover production drops/chests/sweeps, shop/craft exclusion, costs through level 200, all eight slots, reset refunds, failed/duplicate transactions, restart and legacy-save protection. Schema 3 also protects accounts whose only remaining quality gear is historical, with every inventory and the warehouse empty.

The same-source `Builds/macOS-ItemQuality/HELLSCRIPT.app` passed **three processes**: initial execution, restart and corruption recovery. Fixtures explicitly seeded funds and equipment in an isolated save, then invoked real UI button events for masterworking, duplicate clicks, failed-save retry, stale-quote rejection, caps, resetting and greater-affix rerolling. The recovery archive matched the exact original bytes. The user's existing save was not used.

All 15 Korean/English screenshots were directly reviewed, including portrait and short landscape at 140% text scale, costs/refunds, save errors, replacement stats, the awakened filter and the shop's level-60 display. Scroll-view edges intentionally clip offscreen content; the short landscape view was scrolled to its service button. These are not OS-input or physical touch checks. Evidence includes the [validation summary](../../Artifacts/Validation/ItemQuality/validation-summary.json), [source/build provenance](../../Artifacts/Validation/ItemQuality/validated-source.json) and [visual review](../../Artifacts/Validation/ItemQuality/visual-review.json).

![Korean masterwork progression and costs](../../Artifacts/Validation/ItemQuality/Native3/initial-02-masterwork-ko.png)

![English replacement stats](../../Artifacts/Validation/ItemQuality/Native3/initial-10-quality-comparison-en.png)

Among 100,000 actual generated stage-30 items, 21,944 awakened, 8,809 affix lines were greater, and 7,550 awakened items had at least one greater affix. Separate fixed-seed samples of 100,000 draws at stages 20, 25, 30, 40, 44 and 100 were also checked against the designed probabilities. These are distribution checks, not promises for individual loot rolls.

An actual damage calculation isolating specification section 5.2 produced 153.5032 baseline damage and 278.3303 quality damage, a **1.813189 ratio**. The fixture used item level 30, enhancement +5, awakening, masterwork 12 and a tier-6 weapon diamond, explicitly matching the stated 60% existing additive-damage baseline. This is not an average damage increase across builds. Greater affixes and random masterwork affix boosts are checked separately.

A **1,800-run comparison is in progress**: five stages (30/45/60/75/90), six builds, 30 seeds and paired quality/baseline profiles. Quality gear uses actual awakened generation and its greater-affix rolls, then the enhancement and masterwork services up to the cap. Baselines retain identical bases, unique effects and raw affix rolls, with quality layers removed. Each pair must have identical layouts and initial encounters. Gems and rune boards are absent. Hero level, account progression and investment currency are seeded fixture conditions, not natural-growth or farming-time measurements.

The combat comparison uses the frozen `Focus5-inputs.json` source. Subsequent edits strengthen schema protection, error messages, UI, translations and tests; the generation, masterwork and combat calculations used by the comparison are unchanged. The final build and full suite were confirmed to use identical `Build3-inputs.json` and `Full4-inputs.json` sources. Ongoing comparison files are not presented as completed final-player results.

Remaining work includes completion/analysis of that comparison and natural growth/farming-cost validation. Physical mobile input and sustained performance, the pending gem-storage decision, and gem acquisition/service integration remain separate unfinished work. Main and the public wiki receive the verified feature checkpoint first; the combat comparison receives a separate final record after it completes.

## Screen evidence

| Checked view | Original |
|---|---|
| Initial quality details | [PNG](../../Artifacts/Validation/ItemQuality/Native3/initial-01-quality-detail-ko.png) |
| Korean masterwork | [PNG](../../Artifacts/Validation/ItemQuality/Native3/initial-02-masterwork-ko.png) |
| Portrait / large text | [PNG](../../Artifacts/Validation/ItemQuality/Native3/initial-03-masterwork-en-portrait.png) |
| Short landscape | [PNG](../../Artifacts/Validation/ItemQuality/Native3/initial-04-masterwork-en-short-landscape.png) |
| Milestone confirmation | [PNG](../../Artifacts/Validation/ItemQuality/Native3/initial-05-milestone-confirm-en.png) |
| Save failure | [PNG](../../Artifacts/Validation/ItemQuality/Native3/initial-06-save-failure-en.png) |
| Cap / affix boosts | [PNG](../../Artifacts/Validation/ItemQuality/Native3/initial-07-cap-and-affix-boosts-en.png) |
| Reset refund | [PNG](../../Artifacts/Validation/ItemQuality/Native3/initial-08-reset-confirm-en.png) |
| Greater-affix warning | [PNG](../../Artifacts/Validation/ItemQuality/Native3/initial-09-greater-reroll-warning-en.png) |
| Replacement stats | [PNG](../../Artifacts/Validation/ItemQuality/Native3/initial-10-quality-comparison-en.png) |
| Awakened filter | [PNG](../../Artifacts/Validation/ItemQuality/Native3/initial-11-awakened-filter-en.png) |
| Shop level 60 | [PNG](../../Artifacts/Validation/ItemQuality/Native3/initial-12-shop-level-cap-en.png) |
| Restart persistence | [PNG](../../Artifacts/Validation/ItemQuality/Native3/restart-01-restored-masterwork-en.png) |
| Recovery notice | [PNG](../../Artifacts/Validation/ItemQuality/Native3/recover-01-quality-recovery-notice-en.png) |
| Investment retained | [PNG](../../Artifacts/Validation/ItemQuality/Native3/recover-02-recovered-investment-en.png) |

Implementation evidence: [quality calculations/services](../../Assets/HELLSCRIPT/Runtime/Core/ItemQuality.cs), [equipment UI](../../Assets/HELLSCRIPT/Runtime/Presentation/GameUI.ItemQuality.cs), [quality tests](../../Assets/HELLSCRIPT/Tests/Editor/ItemQualityTests.cs), [combat comparison runner](../../Assets/HELLSCRIPT/Editor/BuildIntegrationValidation.ItemQuality.cs), [native fixture](../../Assets/HELLSCRIPT/Runtime/Presentation/RuntimeItemQualitySmoke.cs).
