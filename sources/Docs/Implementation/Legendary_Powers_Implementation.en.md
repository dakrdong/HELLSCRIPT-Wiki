# Class legendary powers: implementation and verification

Updated on: 2026-09-22.

## Implementation

The [class legendary catalogue](../Design/HELLSCRIPT_Legendary_Class_Expansion.en.md) contains 40 Warrior, 40 Ranger and 40 Mage items. It retains the 12 existing class legendaries and 3 shared legendaries, and adds 108 items. The 24 set pieces are separate.

`Assets/HELLSCRIPT/Resources/Data/LegendaryPowers.json` owns new definitions and bilingual text. `LegendaryPowers` loads and validates it; `ItemCatalog.BuildUniques` adds the entries to the existing pool. `ItemGenerator`, `Economy.Equip`, `HeroStats.specials` and `CombatSimulation` retain generation, equipment and combat ownership. There is no parallel inventory or combat manager. The wiki DB reads the same JSON.

Effects attach to existing source damage, successful release, incoming-hit and potion hooks. Pending damage, counts, cooldowns and buffs are stored under `CombatEffectState`. Combat effect save version 6 initializes empty new lists for older saves; the existing version guard makes older clients reject newer effect saves instead of silently dropping them.

## Reference and balance scope

The [D4 Builds aspect database](https://d4builds.gg/database/legendary-aspects/) was inspected in the in-app browser on 2026-09-22. New items cite 83 distinct aspects, all matched against the rendered page in the [reference-name check](LegendaryExpansionEvidence/reference-validation.json). Names, descriptions, values and skill mappings were written for HELLSCRIPT, not imported as a current Diablo IV balance patch. There are 19 execution types and no fully duplicated executable rule within a class.

New item weight starts at 20. Items compete with existing legendaries and set pieces in class-and-slot draws, so relative named-item chances change. Multi-stage, multi-seed, optimized-build balance and physical-mobile performance remain separate validation work. Equipment uses existing base art; no new icons or models were created.

## Verification

All 305 tests in the final related regression run passed. Every one of the 108 new data rows is exercised through actual equipped combat state, checking damage, resources, HP, barriers, control, action speed or cooldown effects. Individual damage probes enter the actual damage function directly and are distinguished from automatic-combat evidence.

| Edit Mode scope | Passed | Failed | Skipped | Original result |
|---|---:|---:|---:|---|
| Initial focused run | 163 | 0 | 0 | [XML](LegendaryExpansionEvidence/legendary-class-focused-editmode.xml) |
| Full regression run | 2,940 | 4 | 0 | [XML](LegendaryExpansionEvidence/legendary-class-all-editmode.xml) |
| Related regression after corrections | 305 | 0 | 0 | [XML](LegendaryExpansionEvidence/legendary-class-regression-editmode.xml) |

Three of the four findings from the full run were corrected and retested. Localization now checks `UniqueItemDefinition.Name`, the same property used by the compendium. The composition test preserves assertions for the original 39 weights while accounting for 108 additions and the Warrior belt pool's total weight of 190. The integration test runs twelve complete rifts: a prior report took 178.17s and this full run took 185.02s, near or beyond the default 180s limit. Its outcome assertions are unchanged; its timeout is now 300s, and the rerun passed in 179.86s. This is not evidence of a performance improvement.

The remaining `EveryCustomGraphicBringsItsOwnCanvasRenderer` failure concerns five existing graphics classes. The test and owning source files are byte-identical to base commit `8852c6c`, as recorded in the [source comparison](LegendaryExpansionEvidence/existing-graphic-failure.json). Those graphics were not modified. The full 2,944-test suite was not rerun after the test-only corrections, so no full-suite pass is claimed. The [validation summary](LegendaryExpansionEvidence/validation-summary.json) separates run scopes and the outstanding failure.

Regression coverage includes non-recursive powers, kill attribution for extra damage, cancelled preparation, no healing after lethal incoming damage, boss stagger and the five-target area cap. Refreshing periodic damage preserves its next scheduled tick, and cooldown deadlines use the existing combat tick. Reopening is checked at simulation times 0, 128 and 256 seconds. LW28 reduces Crush resource cost while War Cry is active.

The [validated source hashes](LegendaryExpansionEvidence/validated-source.json) identify changed game code, data and test sources. Validation ran in an isolated checkout; it does not represent a full regression run in the user's open original Unity Editor.

### Native macOS player

The Unity 6000.6.0f1 macOS development build succeeded with zero build errors. In the Metal player, the actual `ItemGenerator` and `Economy.Equip` created and equipped one new weapon per class. The real `CombatSimulation` ran automatic combat for 12 seconds and saved through `GameStore` before process exit. A second player process reloaded each save and continued for 6 seconds. Effects, total damage and random state matched a control restored from the same save.

| Class | Weapon | Legendary damage events at 12s | Events after restart, at 18s |
|---|---|---:|---:|
| Warrior | LW07 Flesh-Cutting Wind | 11 | 17 |
| Ranger | LA05 Trailing Arrowhead | 6 | 10 |
| Mage | LM05 Lingering Ember | 10 | 15 |

[Native results and screenshot hashes](LegendaryExpansionEvidence/native-summary.json) are preserved. This is an automatic combat fixture inside the native player, not user-driven field combat or balance certification. Individual effects for all 108 new entries are covered by the focused suite above.

For all three classes, item details were captured in Korean at 540×960 and English at 960×540. Under both display conditions, the compendium checked names and descriptions for all 108 new entries, including preferred text height versus actual bounds. All 12 captures were visually reviewed. Physical mobile input and performance were not checked. Player logs contain URP warnings for stripped GaussianDepthOfField, BokehDepthOfField and PaniniProjection shaders; the legendary smoke produced no exceptions.

| Class | Korean detail | English detail | Korean compendium | English compendium |
|---|---|---|---|---|
| Warrior | [View](LegendaryExpansionEvidence/ko-portrait-LW07.png) | [View](LegendaryExpansionEvidence/en-landscape-LW07.png) | [View](LegendaryExpansionEvidence/ko-portrait-collection-LW07.png) | [View](LegendaryExpansionEvidence/en-landscape-collection-LW07.png) |
| Ranger | [View](LegendaryExpansionEvidence/ko-portrait-LA05.png) | [View](LegendaryExpansionEvidence/en-landscape-LA05.png) | [View](LegendaryExpansionEvidence/ko-portrait-collection-LA05.png) | [View](LegendaryExpansionEvidence/en-landscape-collection-LA05.png) |
| Mage | [View](LegendaryExpansionEvidence/ko-portrait-LM05.png) | [View](LegendaryExpansionEvidence/en-landscape-LM05.png) | [View](LegendaryExpansionEvidence/ko-portrait-collection-LM05.png) | [View](LegendaryExpansionEvidence/en-landscape-collection-LM05.png) |

### Wiki verification

Wiki generation, consistency checks, eight Python tests and UI checks passed. In the in-app browser, the local public mirror showed 40 items for each class filter and 120 catalogue rows in both Korean and English. Item parameters, references, date-only metadata and read-only behavior were checked. The [browser verification record](LegendaryExpansionEvidence/browser-validation.json) describes the local generated mirror, not a public deployment.

## Publication

Work is on `codex/legendary-class-expansion`. Pushing the source branch does not update the public wiki. Under repository rules, the main-merge owner reruns validation on merged `main` and publishes the complete public mirror. Public URL: [HELLSCRIPT Wiki](https://dakrdong.github.io/HELLSCRIPT-Wiki/#/tree).
