# Item Rarity Probabilities by Rift Stage

Date: 2026-09-12. Status: draft balance values for HELLSCRIPT. [한국어](HELLSCRIPT_Rift_Rarity_Detail.md).

## 1. Purpose and baseline

Equipment rarity depends on both the acquisition source and the rift stage actually being played. Higher stages increase the legendary probability and the cumulative probability of rare or better equipment. This feature adds neither an equipment Magic Find affix nor a permanent stat accumulated through highest-clear records. Diablo IV is a reference only for the general direction of offering better reward opportunities in harder encounters. The values below are not that game's actual formulas or values.

The [GDD baseline](HELLSCRIPT_GDD_v0.1.md) and [content workbook](../Data/HELLSCRIPT_Content_v0.1.xlsx) supplied by the user on 2026-09-12 are saved in the project and updated. Their stage 1 baselines for normal enemies, elite enemies, and bosses matched the existing code. The caps and growth rate below are neither user-approved final values nor values validated through playtesting.

## 2. Shared formula and balance data

`R` is the stage of the current battle; values below 1 are treated as stage 1. Define `t=max(0,R−1)`, `K=49`, and `g=t/(t+K)`. Given the baseline legendary probability `p0` and legendary cap `c`, use `p=p0+(c−p0)×g`. Each non-legendary grade with baseline probability `b` is adjusted to `b×(1−p)/(1−p0)`.

| Source | Common baseline | Magic baseline | Rare baseline | Legendary baseline p0 | Legendary cap c |
|---|---:|---:|---:|---:|---:|
| Normal enemy | 55% | 25% | 19% | 1% | 10% |
| Elite enemy | 0% | 60% | 35% | 5% | 25% |
| Boss | 0% | 0% | 85% | 15% | 45% |

The increase in legendary probability comes proportionally from the existing non-legendary pool. The retained ratios are 55:25:19 for normal enemies and 60:35 for elites; bosses contribute only from rare equipment. The probability of rare equipment alone may decrease, but the cumulative probability of rare or better increases. Common equipment remains impossible for elites, and common and magic equipment remain impossible for bosses.

Stage 50 reaches half the increase from the baseline to the cap. Growth then slows as the probability approaches its cap. This supports unlimited stages without negative probabilities or probabilities above 100%, and allows the growth rate to be adjusted without multiple stage brackets. All three sources share `K`. Keeping both the legendary baselines and caps ordered guarantees boss ≥ elite ≥ normal legendary probabilities at every stage.

Following the existing `ItemCatalog` pattern of defining content data in code, `RiftRarityBalance.cs` owns `K` and the baselines and caps for each source; `RiftRarity.cs` performs calculation and selection. Data construction rejects invalid baseline totals, negative or non-finite values, forbidden grades, reversed legendary baseline or cap ordering, and 100% legendary caps. The workbook uses verification formulas that reference the same balance parameters. This feature does not add automatic runtime loading from Excel. Balance changes require updating and cross-checking both the code data and workbook parameters.

## 3. Representative stage probabilities

Each cell below lists **common / magic / rare / legendary**, in percent. Normal-enemy probabilities are conditional on an equipment drop having occurred. Display values are rounded to two decimal places, so their displayed sum may differ from 100.00% by 0.01 percentage points. Calculations use unrounded values.

| Rift stage | Normal enemy | Elite enemy | One boss draw |
|---:|---|---|---|
| 1 | 55.00 / 25.00 / 19.00 / 1.00 | 0.00 / 60.00 / 35.00 / 5.00 | 0.00 / 0.00 / 85.00 / 15.00 |
| 10 | 54.22 / 24.65 / 18.73 / 2.40 | 0.00 / 58.04 / 33.86 / 8.10 | 0.00 / 0.00 / 80.34 / 19.66 |
| 20 | 53.60 / 24.36 / 18.52 / 3.51 | 0.00 / 56.47 / 32.94 / 10.59 | 0.00 / 0.00 / 76.62 / 23.38 |
| 30 | 53.14 / 24.16 / 18.36 / 4.35 | 0.00 / 55.30 / 32.26 / 12.44 | 0.00 / 0.00 / 73.85 / 26.15 |
| 50 | 52.50 / 23.86 / 18.14 / 5.50 | 0.00 / 53.68 / 31.32 / 15.00 | 0.00 / 0.00 / 70.00 / 30.00 |
| 100 | 51.66 / 23.48 / 17.84 / 7.02 | 0.00 / 51.55 / 30.07 / 18.38 | 0.00 / 0.00 / 64.93 / 35.07 |
| 300 | 50.70 / 23.05 / 17.52 / 8.73 | 0.00 / 49.15 / 28.67 / 22.18 | 0.00 / 0.00 / 59.22 / 40.78 |
| 1000 | 50.23 / 22.83 / 17.35 / 9.58 | 0.00 / 47.96 / 27.98 / 24.06 | 0.00 / 0.00 / 56.40 / 43.60 |

## 4. Applicable sources and reward finalization

- Actual battles use the saved `RunState.stage`. A hero who has cleared stage 50 still receives stage 10 probabilities when repeating stage 10.
- The equipment-drop chance for normal enemies remains 2%. Elites retain one equipment draw, and bosses retain three. The probability of obtaining at least one legendary from a boss is `1−(1−p)^3`; `3p` is the expected number of legendaries.
- A sweep grants one boss reward bundle for the selected hero's `highestClear`. Each of its three equipment draws uses the boss probability for that stage. It adds no separate probability bonus, record update, repeated first-clear reward, or experience reward.
- Offline rewards remain limited to gold and common enhancement materials. The unidentified equipment shop, core crafting, basic crafting, rift chests, and events are excluded from this stage adjustment.
- After rarity is selected, the existing item generator selects legendary and set members that fit the equipment slot and hero class, using their existing individual weights. Sets remain part of the legendary grade, with no separate preference for extremely rare members.
- Normal and elite rewards are rolled once when their kill rewards are generated. Boss rewards are rolled once at finalization, protected by `bossRewarded`; sweeps are rolled once during reward processing. A complete item is stored in each ground drop's `DropState.item`. Collection, reconnection, and highest-clear updates do not reroll stored items. Existing sweep receipts and save structures are preserved.

## 5. Preserved rules and existing differences between documents and code

Item levels retain the existing code's uniform integer draw from `max(1,R−2)` through `R+2`, inclusive. Affix counts and value ranges, individual legendary weights, enhancement and dismantling costs, gold and material amounts, skills, passives, and monster stats also remain unchanged. Equipment rerolls use the latest rule in the supplied baseline: `L=max(1,item level)`, with a cost of `500×L+50×L×(L−1)` gold. Every reroll at the same item level costs the same amount; 500 and 50 are test coefficients. The old reroll-count multiplier still present in the project code was corrected to match this latest baseline.

The project's existing detailed item design specifies an item-level cap of 60, but the current item-generation code does not implement that cap. Likewise, legendary candidates' `requiredSkill` data is not used as an unlock filter during generation. These are pre-existing differences between design and implementation, not changes introduced by this feature. This task preserves the existing generation behavior and member weights without expanding its scope to resolve those differences. Differences between the supplied initial content and later project content expansions are also outside the scope of this feature.

## 6. Interface and validation

Open **Rarity by rift stage** from the stage-selection area in the Sanctuary or Rift keeper. The interface supports Korean and English, displays all three sources for the selected stage, and shows the sweep's reference stage separately. It distinguishes the 2% normal-enemy equipment-drop chance from conditional rarity probabilities, and explains that the boss probability applies to each of three draws, alongside the chance of at least one legendary.

Executed checks, sample sizes, seeds, tolerances, and checks not performed are recorded in the [implementation and validation log](../Implementation/Rift_Rarity_Expansion.md). Mathematical probabilities, experiments using fixed random seeds, automated combat-processing checks, inspection of the actual player interface, and extended playtesting are reported separately.
