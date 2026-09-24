# Rift 1–1000 equipment, materials and first-clear rewards v0.3

Updated: 2026-09-24 · **Drop/combat proposal · First-clear boxes implemented · Expected-value calculations**

[한국어](HELLSCRIPT_Rift_Rewards_1000.md) · [Combat and progression](HELLSCRIPT_Balance_1000.en.md) · [Reward tables](Balance1000/Rift_Reward_Tables.en.md) · [Interactive report](Balance1000/dashboard.html) · [All 1,000 stages](Balance1000/projection.json) · [Reward inputs](Balance1000/reward_parameters.json)

The two-hour, 180-day proposal now connects equipment rarity and quantity, gems, runes, materials, stones, cores and first-clear gifts. Reduce ordinary enemy gear volume while retaining three boss items. Introduce rift legendary/set gear at R30; forecast a median of two equippable legendaries at R30 and nine at R50. Later growth emphasizes sets, powers, awakening and affix quality. These are slot-coverage estimates allowing duplicates, not executed equipment decisions.

Implemented update: [R1–5 introductory balance, R25 unidentified purchases, R30 legendary/set rift rewards, R35 rerolls](HELLSCRIPT_Early_Rift_Balance.en.md). The first guaranteed legendary moved from R10 to R30. New drops use these gates; owned items, boxes and already unlocked services remain intact. Detailed future probabilities below are proposals, separate from the runtime curve.

## 1. Current implementation versus proposal

| Source | Current implementation | Proposal |
|---|---|---|
| 110 normal enemies | Gear 2%, gem 1.5%, rune 2% per enemy | Gear 1%, gem 0.5%, rune 0.5% |
| Eight elites | One guaranteed item, gem 8%, rune 20% | Gear 35%, gem 10%, rune 10% |
| Boss | Three gear, two gems, four runes, plus currencies | Keep three gear and two gems; reduce runes to two; apply unlock gates |
| Equipment-bearing chests | Two per map; M/R/L 60/38/2% or 80/19/1% | Stage-based rarity; assume 50% collection per chest, one item per run in expectation |
| Golden goblin | From R5, 10% spawn attempt; rare-or-better item with elite legendary probability; 8× elite gold | Assume 80% kill rate when spawned: 0.08 items/run. Placement failures need measurement |
| Salvage | C/M/R materials 1/2/5; legendary yields one same-slot core instead | Retain payout rules; assume 80% ordinary salvage and a 0–90% legendary salvage schedule |
| First clear | Per-character `firstClears` already pays gold/materials; account first-clear boxes now have catalog, claim and opening transactions | Extend basic rewards and add account-once milestone packages |

At R1–5, the current implementation uses 55 normals and four elites. Before R30, all current rift equipment sources exclude legendary/set results; chest legendary rolls become rare. At R30, the unchanged runtime growth curve gives 4.346% / 12.436% / 26.154% conditional legendary probabilities for normal / elite / boss gear. These differ from the proposal below.

Source evidence: [CombatSimulation](../../Assets/HELLSCRIPT/Runtime/Core/CombatSimulation.cs), [rarity](../../Assets/HELLSCRIPT/Runtime/Core/RiftRarityBalance.cs), [chest generation](../../Assets/HELLSCRIPT/Runtime/Core/RiftGenerator.cs), [chest delivery](../../Assets/HELLSCRIPT/Runtime/Core/RiftExploration.cs), [goblins](../../Assets/HELLSCRIPT/Runtime/Core/CombatSimulation.Goblin.cs), [salvage/sweeps](../../Assets/HELLSCRIPT/Runtime/Core/Economy.cs), [gems](../../Assets/HELLSCRIPT/Runtime/Core/GemCatalog.cs), and [runes](../../Assets/HELLSCRIPT/Runtime/Core/RuneEconomy.cs). Current payouts and proposed values are distinct.

## 2. Equipment occurrence and conditional rarity

Roll for an item before rolling its rarity. Legendary probabilities below are **conditional on an equipment roll**. At R100, one elite drops a legendary with probability `35% × 6% = 2.1%`. Summoned adds are excluded.

| Rift | Normal item L | Elite item L | Boss item L | Chest item L | Goblin item L |
|---|---:|---:|---:|---:|---:|
| 1–29 | 0% | 0% | 0% | 0% | 0% |
| 30 | 0.2% | 1.8% | 10% | 0.9% | 1.8% |
| 50 | 0.4% | 3% | 15% | 1.5% | 3% |
| 100 | 1% | 6% | 23% | 3% | 6% |
| 200 | 2% | 9% | 30% | 4.5% | 9% |
| 500 | 3.5% | 13% | 36% | 6.5% | 13% |
| 1000 | 5% | 18% | 40% | 9% | 18% |

Allocate the remaining probability to common, magic and rare. At R1/10/30/50/100/200/500/1000, normal nonlegendary common/magic weights are 70/25, 45/40, 20/40, 10/35, 2/23, 0/10, 0/2 and 0/0 percent; the remainder is rare. Elite nonlegendary magic weights are 65/50/30/15/5/0/0/0%; chest weights are 80/65/45/30/10/0/0/0%. Boss and goblin nonlegendaries are always rare. Interpolate between anchors and multiply these weights by `1−legendary probability`.

Mean gear per successful run is `110×0.01 + 8×0.35 + 3 + 2×0.5 + 0.1×0.8 = 7.98`. R1–4 instead use 55 normals and four elites, giving 5.95 items; R5 adds goblins for 6.03. Excluding both optional sources gives 4.95 at R1–5 and 6.90 afterward. Assume zero magic find. Current magic find promotes common rarity and must not multiply legendary odds again.

Sets are a subset of legendary rarity. Weighting the current 147 natural unique definitions by class and slot yields approximately 31.88% sets among warrior legendary drops. Awakening is an overlay on rare/legendary items: 2/20/35/45/50/50% at R100/200/300/500/750/1000. Neither is an extra rarity bucket. Completing one specific class set differs from receiving any set piece.

Round the progression target item level to obtain the drop center. Offsets −2/−1/0/+1/+2 have probabilities 10/15/50/15/10%, clamped to levels 1–60. A center of 60 therefore yields L58/59/60 at 10/15/75%. The center reaches L60 at R200; quality and enhancement drive subsequent growth.

## 3. Boss loot and repeat completion bonus

Let `R` be rift stage. Floor integer currency as specified. Boss kill and normal completion are separate payout events that **split one total**, not duplicate it.

| Resource | Boss kill | Normal completion bonus |
|---|---|---|
| Equipment | Three boss-rarity rolls | None |
| Gems | Two from R10 | None |
| Runes | Two from R15 | None |
| Gold | `floor((800+50R)×0.6)` | `800+50R−boss gold` |
| Materials | `4+floor(R/18)` | `2+floor(R/50)` |
| Stones | `floor(S×0.6)` | `floor(S)−boss stones` |

Stone total `S` is `10+2R` through R20, then `50+0.075×(R−20)`. R100 pays 33 boss + 23 completion = 56; R1000 pays 74 + 49 = 123. Salvage is additional.

Retain normal gold `5+R` and elite gold `25+5R`. Retain total chest gold `floor(0.15×(800+50R))` and guarded-chest materials `max(1,floor(0.2×(5+floor(R/5))))`, with 50% expected collection. Goblin gold is `8×(25+5R)`, with an 8% combined spawn/kill expectation.

From R60, three daily sweeps each substitute one boss-plus-completion bundle. Include gear, gems and currencies; exclude runes, mastery XP and first-clear rewards. Offline rewards remain capped at 12h, with gold `200+40R` per hour; reduce hourly materials to **`1+floor(R/50)`**. Keeping the current `5+floor(R/5)` would overfund masterwork materials.

## 4. Gems, runes and salvage

From R10, gems average **3.35/run**: normal 0.5%, elite 10%, boss two. Six equally likely families average 0.5583 each. Natural tiers are T1 at R10–99, T2 at R100–299, T3 at R300–1000; T4–T6 never drop naturally. Preserve matching-family 5:1 synthesis: one T6 needs 125 T3 gems. Target equipped tiers T1/2/3/4/5/6 at R10/30/100/300/500/750, combining accumulated lower-tier gems and first-clear gifts.

The reference allocates R10's two ten-gem choice boxes to ten diamonds and ten rubies; later choice gems go to the scarce diamond family. A choice gem is never credited to all colors. Recover and reuse gems when replacing sockets. Implement higher-tier synthesis/use gates alongside the target schedule. Continuous gem-elixir consumption is not a baseline requirement.

From R15, runes also average **3.35/run**, or 0.67 of each of five types. G0/1/2/3/4/5/6 start at R15/40/100/180/300/500/650. Retain size 1–5 weights: G0 `100/0/0/0/0`, G1 `80/20/0/0/0`, G2 `45/40/15/0/0`, G3 `15/35/35/15/0`, G4 `5/15/40/30/10`, G5 `0/5/30/40/25`, G6 `0/0/20/40/40%`. Select uniformly among currently allowed shapes of a chosen size. Quantity does not establish connected placement, rotation or color efficiency.

Assume 80% salvage of common/magic/rare and legendary salvage fractions 0/40/75/85/90% at R10/30/100/200/1000, linearly interpolated. Actual transactions must respect equipped, locked, preset and socket protections. This is a post-selection disposal assumption, not an instruction to automatically salvage protected items.

| Salvaged rarity | Materials | Stones | Same-slot cores |
|---|---:|---:|---:|
| Common | 1 | 0 | 0 |
| Magic | 2 | 1 | 0 |
| Rare | 5 | 5 | 0 |
| Non-set legendary | 0 | 15 | 1 |
| Set legendary | 0 | 10 | 1 |

Retain automatic aspect collection on eligible legendary salvage; do not count it as another item drop. Exclude invested-material refunds, item sales and recycling crafted outputs. A targeted legendary costs ten same-slot cores; rare crafting costs 50 materials plus `500×highest clear` gold. These are optional expenses. Natural salvage earns approximately 7,810 cores through R1000, or 976 per slot in expectation. This is gross income before spending; crafted items are not added to the natural legendary forecast.

## 5. First-clear rewards

Each first normal clear pays gold `1000+100R`, materials `10+floor(R/10)`, stones `2+floor(R/50)`. Every tenth stage adds `20+floor(R/20)` materials; every fiftieth adds `50+floor(R/2)` stones. Across R1–1000, basic totals are **51,050,000 gold · 64,100 materials · 17,770 stones**.

First-clear grants are now consumable boxes. The [implementation document](HELLSCRIPT_Reward_Boxes.en.md) and [runtime catalog](../../Assets/HELLSCRIPT/Resources/Data/RewardBoxes.json) define the current milestone schedule. Each gem box grants ten gems of one selected color: two at R10, one each at R30/100/300/500/750, and two at R1000. Account-once coins total 5,600 and are not spent in the two-hour baseline.

Automatic gold/materials remain character-once; new boxes and interval extras are account-once. The basic material formula above is a reduction proposal, distinct from the existing runtime `10+floor(R/5)`. Exact normal-clear evidence, account claim receipts, box RNG, choices and remaining counts persist. Full gear/gem storage leaves boxes unopened. Schema-12 box transactions are implemented separately from the proposed repeat drops, unlocks and enemies.

## 6. Economy and validation limits

Add 12 seconds per attempt for optional exploration, giving approximately 100–114 seconds at R1–5 and 191–231 seconds afterward. At 120 minutes/day and a target 90% success rate, expect 33.88 successful R10 runs/day or 28.02 R1000 runs/day. The 180-day route contains approximately **5,470 successes: 1,000 first clears and 4,470 repeats**. Exclude failed-run partial rewards. Town management is additional time.

| Resource | Cumulative income | Assigned growth cost | Reserve | Income with no chests/goblins |
|---|---:|---:|---:|---:|
| Gold | 760,092,334 | 613,091,044 | 147,001,290 | 738,411,312 |
| Materials | 582,629 | 459,270 | 123,359 | 498,308 |
| Stones | 808,436 | 641,150 | 167,286 | 781,879 |

Check cumulative affordability at every stage. Reserves support optional crafting, rerolls, build changes and collection variance. Material surplus is not an invitation to spend everything on 50-material crafts: their gold cost also applies. The 90-day alternative lacks gold, materials, stones, mastery and forge time; the original combat report's scenarios have been recalculated.

At R1000, two hours of farming plus three sweeps, 12h offline and salvage yield about **232.63 items** including 55.82 legendary/set pieces, 99.88 T3 gems, 93.88 G6 runes, 6.48 million gold, 4,047 materials and 5,197 stones per day. This is a daily stationary farming view, not a claim that the whole 180-day route farms R1000. The cumulative ledger allocates time separately to every stage.

Automated checks cover probability mass, source conservation, duplicate allocation, synthesis equivalents, first-clear totals, per-stage deficits and the fixed two-hour budget. Per-run count quantiles use exact finite Bernoulli convolution; long-term slot coverage uses a separate Poisson approximation. **Real win rates, inventory transactions, natural-save progression, gem-color tails, rune placement and power-specific build completion remain unvalidated.**

Implement in order: reward receipts and schema → unlock/drop tables → boss/completion split → first-clear choices and overflow → real new-account acquisition/spending simulation → class combat tests. Changing enemy HP alone or adding a payout chest first would disconnect rewards from combat growth.

[Reward model](../../tools/balance_rewards.py) · [Reward tests](../../tools/test_balance_rewards.py) · [Full generator](../../tools/balance_1000.py) · [Verification record](Balance1000/verification_record.json)
