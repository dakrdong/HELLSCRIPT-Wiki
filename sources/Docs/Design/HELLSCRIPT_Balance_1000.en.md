# Rift 1–1000 progression and combat balance v0.4

Updated: 2026-09-24 · **Planning proposal · Not applied to gameplay · Natural progression and combat win rates unverified**

2026-09-24 update: [Rift content unlocks](HELLSCRIPT_Rift_Content_Unlocks.en.md) implements 14 service entitlements and first-clear popup guidance. Menu access timing is implemented; enemy, drop and growth curves remain proposals.

[한국어](HELLSCRIPT_Balance_1000.md) · [Drop and first-clear rewards](HELLSCRIPT_Rift_Rewards_1000.en.md) · [Interactive analysis](Balance1000/dashboard.html) · [All 1,000 stage calculations](Balance1000/projection.json) · [Editable inputs](Balance1000/parameters.json) · [Milestone tables](Balance1000/Balance_1000_Tables.en.md)

The current formulas cannot support a coherent journey to rift 1000. Enemy stats grow exponentially while most progression limits become accessible early. Set the playtime and growth budgets first, construct attainable reference characters, and derive fixed enemy targets from those characters.

The provisional baseline is one free-to-play character at 1x speed, 120 foreground rift minutes per day, reaching R1000 in **180 days / 360 rift hours**. The user has not specified the calendar horizon. Town management is additional time. Include one daily offline claim capped at 12 hours and three daily sweeps after unlocking. Exclude paid speed, fatigue recovery, premium crafting investment and development grants.

Current development-branch changes: [early rift balance](HELLSCRIPT_Early_Rift_Balance.en.md). R1–5 use smaller maps and reduced enemy stats; unidentified purchases open at R25, natural legendary/set drops and the first guarantee at R30, rerolls at R35. The complete 1000-tier enemy/reward curves remain proposals.

## Current implementation

Source commit: `08a68df48b5557685137958c7b26b01438d6ef71`. The generated dataset preserves source SHA-256 hashes. Unity 6000.6.0f1 evaluated 63 in-memory attribute conditions without changing user saves: 21 synthetic isolated probes, 33 item-validated equipment fixtures and nine production-equivalent skill-tree fixtures. These were not 63 combat runs.

| System | Current quantitative behavior | Treatment |
|---|---|---|
| Character level | Production startup upgrades the skill tree to v3, enabling level 40 and 39 investment points. Primary attribute +2/level; HP +35/+28/+25 for warrior/ranger/mage. | The raw `NewAccount()` factory still exposes a legacy level-30 path before startup migration. |
| Skills, passives, ultimates | 37 per class, 111 total; tree-level and prerequisite rules. Common rank multiplier `1+0.1×(rank−1)`; ultimates have separate rules. | Respect point budgets, cooldowns, resource sustain and trigger uptime. |
| Equipment and affixes | Item level cap 60; base primary stat multiplier `1+0.08×(L−1)`. Common/magic/rare/legendary have 0/1–2/3/4 affixes. | Track level, rarity, useful rolls and roll quality separately. |
| Enhancement | +100 cap. Per rank: weapon attack +2; head armor +2; chest armor +3; gloves attack speed +0.08 pp; boots speed +0.05 pp; belt HP +8; necklace resistance +0.04 pp; ring crit +0.02 pp. | Flat gains have different relative value at different gear levels. |
| Slot growth | Ten persistent character slots, levels 1–100. Starting slots already grant attack 8.5, HP 30, armor 7.5 and all attributes 0.5. All slots at 100 grant attack 850. | Do not count the initial bonuses twice. |
| Reroll | One permanently selected affix line; fixed cost `50×L×(L+9)` per attempt, or 207,000 gold at L60. | An acquisition route for better affixes, not another damage multiplier. |
| Awakening and greater affixes | Starts R20, reaches 50% chance at R44. Primary stats ×1.25; each awakened affix has 10% chance to be greater, granting ×1.5 to that affix. | Drop chance is not equipped awakened-item fraction. |
| Masterwork | Primary stats ×`1.02^m`; one +25% line hit at each of ranks 4/8/12 only. Cap `min(200,12+3×max(0,highestClear−30))`. | Full cap at R93; maximum primary multiplier 52.48. Crit and attack-speed primary stats also grow. |
| Natural legendary and set pool | 123 legendary powers plus 24 pieces from six existing sets: 147 definitions. Some independent legendary damage stacking caps at ×2.5. | Additional hits, control and resource effects require separate conditions. |
| Expanded class equipment | 105 pieces across 24 sets plus 18 additional legendaries have definitions and combat code, but are absent from the current natural `ItemGenerator` pool. | Exclude them from current acquisition predictions; release requires an acquisition path and pool-dilution analysis. |
| Aspects | 1/2/4/8/16 salvaged copies allow aspect levels 1–5. Named effect magnitudes grow by 5% per level, up to 20%. Free imprinting promotes eligible rare gear to legendary. | +20% effect magnitude is not +20% final DPS. Duplicate imprints do not stack the same unique effect. |
| Core crafting | Ten matching-slot cores for selected crafting; optional premium investment 0–8,000. | Fills missing pieces/effects; direct standalone damage contribution is zero. |
| Runes and mastery | Six weapon boards, 259 cells each, mastery cap 41, six points/level. Only the equipped weapon's board contributes. Current G6 drops start at R30. | Require valid color, connectivity, shape, unlock and weapon. Never sum all six boards. |
| Rune fusion | Two same-grade/size pieces advance size; after size five, advance grade and reset to size one. G6 size five is terminal. | Supply route, not a second rune multiplier. |
| Gems and sockets | Six tiers, implemented synthesis **5:1**. Weapon elemental bonuses 3/5/8/12/17/23%. The two-hand reference has six socketable items. | The unlock description's 3:1 wording is stale. T3 is guaranteed from R30; T6 needs 125 matching T3 gems. |
| Potions and gem elixirs | Attack potion +15% for 8s, 30s cooldown; ideal average +4%. Thirty-six gem elixirs consume one matching gem and last 30s. | Two hours of attack-potion upkeep costs 240 charges / 3,600 gold. Do not require 240 T6 gems/day for baseline success. |
| Shops and crafting | Acquisition routes; unidentified-shop legendary chance 2%. Shop item level depends on hero level, actual clear record and the 60 cap. | Attribute the resulting gear once. |
| Sweep, offline, salvage | Three sweeps/day, three boss-grade equipment rolls and two gems per sweep. Offline cap 12h; hourly gold `200+40R`, material `5+floor(R/5)`. | Direct power zero; affects progression speed. |
| Hunt Edict, presets, training | Changes action uptime, targeting, movement and potion use. Training grants no actual progression. | Measure the same build before/after policy changes. |
| Storage, protection, speed | Asset retention, convenience and rewards per hour. | Direct power zero; paid 1.5x speed is excluded. |

Owners: [stats](../../Assets/HELLSCRIPT/Runtime/Core/Economy.cs), [blacksmith](../../Assets/HELLSCRIPT/Runtime/Core/Blacksmith.cs), [quality](../../Assets/HELLSCRIPT/Runtime/Core/ItemQuality.cs), [damage](../../Assets/HELLSCRIPT/Runtime/Core/CombatSimulation.Damage.cs), [production skill-tree activation](../../Assets/HELLSCRIPT/Runtime/Core/GameStore.SkillTree.cs), [unlocks](../../Assets/HELLSCRIPT/Resources/ContentUnlocks.json).

## Measured attribute calculations

Production-equivalent level-one characters all have attack 30.2385. HP is 330/270/240 for warrior/ranger/mage. Keeping starting equipment and slots at level 40 gives attack 34.6845 and HP 1,695/1,362/1,215. Character level primarily supports early survival and skill unlocks.

The following warrior fixture is level 30 with nine level-60 rare pieces, 70% affix quality and seed 902310. The standard attack proxy is `attack × attack speed × expected critical multiplier`; it excludes elemental damage, skills, legendary procs and enemy defense.

| Change | Attack | Standard attack proxy | Change from base | Mixed EHP |
|---|---:|---:|---:|---:|
| Rare gear | 152.12 | 190.78 | Base | 4,210 |
| All gear +20 | 201.63 | 257.47 | +35.0% | 4,741 |
| All gear +100 | 399.67 | 547.41 | +186.9% | 7,036 |
| All slots 25 | 412.46 | 519.20 | +172.2% | 6,162 |
| All slots 100 | 1,356.66 | 2,347.13 | +1,130.3% | 24,528 |
| All awakened | 187.52 | 238.81 | +25.2% | 4,661 |
| Masterwork 12, includes prerequisite +5 | 202.48 | 259.31 | +35.9% | 4,830 |
| Masterwork 50, includes prerequisite +5 | 404.02 | 563.21 | +195.2% | 7,810 |
| Masterwork 200, includes prerequisite +5 | 7,454.60 | 16,205.36 | +8,394.4% | 116,684 |
| Tier-six gems | 152.12 | 190.78 | Proxy unchanged | 5,694 |
| Active attack potion | 174.94 | 219.39 | +15.0% | 4,210 |

Gems still increase damage: the fixture's physical bonus rises from 6.5% to 29.5%, a `1.295/1.065 = 1.2160` multiplier. This is why attack alone cannot evaluate skills, legendaries or gems. Preserve [validated gear results](Balance1000/legal_build_audit.json), [reproducible C#](Balance1000/legal_build_probe.cs.txt) and [production startup results](Balance1000/production_skill_tree_audit.json).

Current normal-enemy HP is `70×1.08^(R−1)` and attack is `18×1.055^(R−1)`. At R1000 they reach approximately **1.72×10^35 HP** and **3.05×10^24 attack**. Do not inflate every player system to match that unrelated exponential curve.

## Attribution and model limits

The dataset provides attack power, benchmark boss DPS, mixed EHP and the planning index `sqrt(DPS×EHP)` for each stage. The index is not an existing in-game combat-power field. EHP uses 50% physical/50% elemental incoming damage and expected dodge, excluding healing, shield uptime, potions and player movement. It does not guarantee survival against burst sequences.

For nine growth groups, evaluate all **512 coalitions per stage** and allocate interactions with exact Shapley values. Baseline plus contributions sum to 100%. Individually removing each system and adding all losses would double-count multiplicative interactions.

Rerolls belong to equipment affixes, aspects to legendary effects, and rune fusion/mastery to active runes. Shops, crafting, sweep, offline rewards, training and storage have zero standalone direct-power contribution. Potions are optional margin, not required baseline power.

The endpoint uses legal four-line affix combinations on nine items: weapon primary/element/crit/crit damage; head primary/HP/CDR/HP%; chest HP/armor/HP%/healing; gloves primary/element/crit/attack speed; boots primary/armor/dodge/movement; belt HP/resist/resource regeneration/HP%; neck primary/resist/crit/CDR; first ring primary/element/crit/crit damage; second ring primary/element/crit damage/resource regeneration. Early equipment coverage and average quality scale this expected budget.

Actual rotations, resource costs, ultimate uptime, conditional passives, summons and secondary damage are not fully simulated. The model explicitly inputs target skill/passive and legendary effective-damage budgets. It must not be interpreted as an all-effects-active optimized build.

## Proposed unlocks and numeric changes

| Rift | Unlock / milestone |
|---|---|
| Start–3 | Training, Hunt Edict, equipment management; enhancement/offline at R1; targeted rare crafting at R3. |
| 5 / 10 / 15 | Slot tutorial; gem services; runes/mastery. |
| 25 / 35 / 40 | Unidentified shop; reroll; aspect-imprint tutorial. Preserve aspect collection from the first salvage. |
| 60 / 80 | Sweep and gem elixirs; masterwork. |
| 100 / 120 | Awakening and targeted set farming; selected core crafting. |
| 200 | Item level 60. |
| 300 / 500 / 650 | Rune G4 / G5 / G6. |
| 750 / 1000 | Gem T6 and masterwork 150; masterwork 200 and slot 100 target. |

Keep skills on the existing character-level tree: target hero level 30 at R50 and 40 at R100. Do not add duplicate rift gates to skills. Preserve existing-account unlocks and owned aspects/runes during migration.

| System | Proposed value |
|---|---|
| Item level | Interpolated input anchors from L1 at R1 to L60 at R200. |
| Enhancement | Keep +100 cap and flat per-slot gains; target +15/R100, +25/R200, +55/R500, +100/R1000. Costs become 80% of the current formula. |
| Slot attack | Total `8.5×S` through level 10, then `85+3×(S−10)`, reaching 355 at level 100. Other slot effects retain current definitions. |
| Slot time | 45% of current durations, two free stations, queued jobs, 85% utilization. Total workload implies about 143 days, versus 270.2 ideal days under current durations. |
| Masterwork | Primary multiplier `1+0.0125×m`, reaching 3.5 at 200. Preserve 4/8/12 line hits and awakening ×1.25. |
| Masterwork access | Open R80; target caps 8/30/50/100/150/200 at R100/200/300/500/750/1000. |
| Gems | Keep 5:1 conversion. Target T2/T3/T4/T5/T6 use at R30/100/300/500/750. Move direct T2/T3 drops to R100/300 and coordinate crafting/equip gates. |
| Runes | G0–G6 bands at R15/40/100/180/300/500/650. Mastery XP normal 1, elite 5, boss 30; level requirement 1000L. Remove stage-scaled XP inflation. |
| Stones | `10+2R` through R20, then `floor(50+0.075×(R−20))` across boss+completion or one sweep. Include salvage and first clears. See the reward proposal for allocation. |
| Legendary chance | Normal/elite/boss: R30 0.2/1.8/10%, R100 1/6/23%, R1000 5/18/40%; no natural legendary/set drops before R30. Other anchors are in the input file. |
| Consumables | Baseline success must not require permanent potion uptime. Attack potions provide optional theoretical average +4%. |

Queued crafting and these new gates/formulas are implementation dependencies. Aggregate duration checks are workload lower bounds, not an executed job-start/completion/claim schedule. The cost envelope includes assumed 35% enhancement replacement loss and 15% masterwork reinvestment; these rates have not been measured.

## Enemy targets and legendary forecasts

[Milestone tables](Balance1000/Balance_1000_Tables.en.md) summarize stages 1/10/30/50/100/200/300/500/750/1000; all 1,000 rows are in the dataset. Values between anchors use linear interpolation. Fractional progression levels represent cohort means, not fractional save-state levels.

Enemy stats are **fixed per-rift design targets**, not dynamic scaling against the current player. Normal HP equals reference DPS ×1.8–2.2 seconds, elite HP is three times normal, and boss HP equals reference DPS ×35–60 seconds. Incoming attack uses the lowest EHP across the three class references: 4.5% normal, 9% elite, 16% boss. Initially test telegraphed boss heavy attacks at 2–2.5 times baseline.

Assume 55 normals and four elites with introductory multipliers at R1–5, then 110 normals and eight elites, 3.5 effective AoE targets, 75 seconds travel/collection and 12 additional exploration seconds. An attempt takes about 100–114 seconds at R1–5 and 191–231 seconds afterward. A **target**, not measured, 90% success rate allocates about 5,470 successes across 180 days: 1,000 first clears and approximately 4,470 farms. Include chests, goblins and salvage; exclude failed-run partial rewards.

| Rift | Proposed equipable legendary P10 / P50 / P90, with R30/100/200 slot guarantees | Current-rate median on the same conditional route |
|---|---|---:|
| 10 | 0 / 0 / 0 | 0 |
| 20 | 0 / 0 / 0 | 0 |
| 30 | 1 / 2 / 3 | 3 |
| 50 | 7 / 9 / 9 | 9 |
| 100 | 9 / 9 / 9 | 9 |

These are acquisition predictions **conditional on the route being cleared**, using Poisson thinning and independent slot pools, not observed percentiles. Account for duplicate slots and two rings. The reference has at most nine items; dual-wield warrior ten-item setups require a separate model. Exclude imprinting, core crafting and shops from this count. Including those routes fills legendary rarity sooner.

Nine legendary items do not imply a finished build. Specific sets, distinct useful powers, awakening, greater affixes, rolls and aspect ranks remain. Natural ownership of the specific warrior Whirlwind four-piece set is approximately less than 0.01% at R30, 12.1% at R50 and 96.8% at R100 under this route. Prefer early functional builds followed by quality improvement over withholding essential effects until R1000.

## Calendar and economy

The 180-day envelope earns approximately 760.09 million gold against 613.09 million assigned to enhancement, masterwork, rerolls, sockets and basic potion budgets; 582,629 materials against 459,270; and 808,436 stones against 641,150. No stage has a cumulative deficit under these assumptions. This is not validation of real inventories, transactions, replacement behavior or natural saves. Optional shopping and crafting must fit inside the remaining gold budget.

| Horizon | Expected successful runs | Gold earned / required | Mastery | Assessment |
|---|---:|---:|---:|---|
| 90 days | ~2,750 | 393.5m / 612.4m | 31 | Deficit stages: 829 gold, 664 materials, 598 stones, 625 rune cells, 425 forge time. |
| 180 days | ~5,470 | 760.1m / 613.1m | 41 | Baseline proposal. |
| 365 days | ~11,062 | 1,513.7m / 614.4m | 41 | Same economy finishes growth too early. |

Socket spending covers six replacement items at R10/30/50/100/150/200/300/500/750/1000: opening costs 2,000 × item level gold and 30 materials per item; recovery costs 500 × the previous item level. The budget includes 5.895 million gold and 1,800 materials; recovered gems are reused. Mean supply assumes six equally likely gem families and 5:1 synthesis, and checks four diamonds for the physical weapon/jewelry plus two rubies for armor. Rune targets are checked against an upper bound on cells unlocked by mastery points. These are capacity checks, not proof of random acquisition, connected placement, or shape compatibility.

Keep the first seven onboarding days identical between calendar scenarios. A 90-day target requires approximately 1.56 times the gold capacity or equivalent cost reduction, plus separate stone, mastery and timer adjustments. A 365-day target requires a new completion cadence and quality choices, not just higher enemy HP.

The [reward proposal](HELLSCRIPT_Rift_Rewards_1000.en.md) defines rarity probabilities, counts, daily income, boss/completion splits and first-clear gifts. Changing reward inputs recalculates legendary coverage and the economy through the same generator. Choice gems are allocated once to specific reference families.

## Implementation and acceptance order

1. Confirm the calendar and time definition. Preserve existing account, character, equipment and transaction ownership.
2. Change the structurally incompatible curves together: enemy exponentials, masterwork, slots, rune bands, unlocks and timers. Apply changes through domain owners, not display-only substitutions.
3. Finish R1–30 first. Capture 30-minute, day-one and day-two saves, actual income/spending, levels, HP, starvation, first death, acquisitions and equipped items.
4. Extend the existing fixed-build runner into real new-save progression: drops, selection, salvage, enhancement, imprints, valid rune placement, re-entry, failures, offline rewards and queued jobs. Fixed Lv30 equipment results are not natural progression.
5. Compare two representative builds per class at ten milestone stages. Use 30 common seeds for screening and at least 200 per final candidate, with confidence intervals. Start with boss DPS within ±10% and crowd clearing within ±15%, while accounting for control and survival roles.
6. Use independent acquisition seeds to report useful slots, key powers, currency and arrival-day P10/P50/P90. Retain failures and interrupted runs. A few milestone clears do not prove R1000 completion.
7. Validate native macOS interaction, save/reload, Hunt Edict and potions, then separately validate physical-mobile timing, fatigue and performance. This analysis changes no game code or UI.

## Reproduction

Run `python3 tools/balance_1000.py` from the repository root to generate the stage model, calendar comparisons, attribution and interactive report. Run `python3 tools/balance_1000.py --check` to verify generated-file parity. [Validation output](Balance1000/validation.json) checks the analytical model's invariants and does not replace combat acceptance. Start future tuning from the explicit [input anchors](Balance1000/parameters.json) and documented formulas.

[Model generator](../../tools/balance_1000.py) · [Regression tests](../../tools/test_balance_1000.py) · [Executed verification record](Balance1000/verification_record.json) · [Isolated attribute probes](Balance1000/current_runtime.json)


2026-09-24 update: [96 consumable boxes](HELLSCRIPT_Reward_Boxes.en.md) are implemented; ten-gem packs and 5,600 account-first coins are included in the forecast. Proposed enemy stats, ordinary drops and growth costs remain unapplied.
