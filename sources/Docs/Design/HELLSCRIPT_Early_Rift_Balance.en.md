# Introductory rift balance and content unlocks

Updated: 2026-09-24

[한국어](HELLSCRIPT_Early_Rift_Balance.md) · [Previous Rift 1–10 playthrough](HELLSCRIPT_Rift_1_10_Playtest.en.md) · [Progression proposal](HELLSCRIPT_Balance_1000.en.md)

Rifts 1–5 provide a faster introduction before rare-item progression, unidentified purchases and legendary/set farming. All thresholds refer to **rift tiers**, not character levels.

| Rift | Behavior |
|---|---|
| 1–5 | About half-size introductory maps, 55 normal enemies and four elites, reduced HP and damage |
| 6 | Established map scale, 110 normals and eight elites |
| 10 | Gem services, two T1 gem-choice boxes and 100 coins |
| Clear 25 | Unidentified purchases: Magic 60%, Rare 38%, Legendary/Set combined 2% |
| 30 | Rift legendary/set drops begin; first normal clear grants a guaranteed legendary weapon box and a T2 gem-choice box |
| Clear 35 | Affix reroll service and a 20,400-gold first-clear box |

## Introductory combat and geometry

Scale coordinates and room dimensions by `sqrt(0.5)`, approximately halving the floor envelope. Characters retain their physical size, so passages stay at least 4m wide and room door/encounter clearance is rebuilt. Rendered floors and authoritative navigation use the same persisted geometry. Telegraphs, dodging, potions and death still operate normally.

| Tier | Normal/elite HP | Boss HP | All enemy attack |
|---|---:|---:|---:|
| 1 | 55% | 45% | 30% |
| 2 | 60% | 50% | 35% |
| 3 | 65% | 55% | 40% |
| 4 | 70% | 60% | 45% |
| 5 | 75% | 65% | 50% |

Multiply the existing stage-scaled values by these factors. Half the original enemy budget awards double kill-meter progress so the boss remains reachable. Keep three boss gear rolls, two equipment-bearing field chests and existing boss/completion currencies. Per-kill payouts remain unchanged; actual totals follow actual kills.

New introductory maps persist version 8 and an `introductory` flag. Existing suspended maps retain their geometry and enemy snapshots. R6+ geometry and fingerprints remain version 7. Training and legacy objective/arena fixtures keep their existing behavior.

## Legendary/set restrictions

New equipment from normal enemies, elites, bosses, treasure goblins and field chests cannot be legendary or set gear below actual Rift 30. A higher account record does not override a low-tier run. Sets are members of rarity 3 and share the same restriction.

For monster rarity rolls, set legendary probability to zero and redistribute across nonlegendary grades proportionally. Field chests retain their independent distribution and downgrade forbidden legendary rolls to rare, preserving item count. R30+ retains the existing stage curve and unique weights. Unidentified purchases use their own distribution from account clear 25.

Move the first guaranteed legendary from R10 to R30. Catalog validation rejects new first-clear legendary/set grants before the rift threshold. Existing owned items, guaranteed boxes, already rolled saved loot and previously unlocked services remain intact; old guaranteed boxes retain their promised grade.

## Ownership and presentation

The first-clear list/detail reads the same `ContentUnlocks` and `RewardBoxCatalog` definitions. It shows R25/R35 service unlocks and R30 legendary/set availability and guarantee; the rarity panel also explains the gate. Existing `GameStore` transactions validate availability, funds and persistence.

Regenerate the 1000-tier planning model with the revised gates and introductory encounter budget. This does not implement the full proposed enemy, drop-volume or upgrade-cost curves. Optional unidentified purchases remain outside the reference acquisition forecast.

## Validation record

Three fresh accounts used the production `CombatSimulation`, save and transaction paths. The warrior progressed continuously through R10; ranger and mage through R5. CPU execution accelerated the foreground clock at 0.05-second steps with 1x attendance. Only earned items and currency were used; no failed attempts or seeds were discarded. Town management time is excluded. These are single-route observations for each class, not population win-rate estimates.

| Class | R1–5 elapsed | Through R10 | Failed attempts | Final hero level |
|---|---:|---:|---:|---:|
| Warrior | 8m 1.35s | 21m 43.85s | 0 / 10 | 13 |
| Ranger | 8m 36.85s | Outside scope | 0 / 5 | 8 |
| Mage | 9m 5.50s | Outside scope | 0 / 5 | 8 |

| Rift | Warrior seconds | Level after clear | Rift gear acquired |
|---|---:|---:|---:|
| 1 | 104.15 | 4 | 5 |
| 2 | 95.40 | 6 | 7 |
| 3 | 99.80 | 7 | 8 |
| 4 | 87.90 | 8 | 6 |
| 5 | 94.10 | 8 | 7 |
| 6 | 150.40 | 9 | 11 |
| 7 | 179.80 | 10 | 13 |
| 8 | 158.85 | 11 | 10 |
| 9 | 167.10 | 12 | 10 |
| 10 | 166.35 | 13 | 10 |

The previous warrior route took 16m 54.10s through R5 including failures, and 33m 8.75s through R10. The observed reductions are approximately 52.5% and 34.4%. Fewer failed attempts also change which attempt-indexed seed is assigned to later stages, so this is not a controlled same-map statistical comparison.

The warrior acquired **87 rift items: six common, 31 magic and 50 rare**, plus one rare weapon from a first-clear box. Legendary/set count was zero. Rift sources were 11 normal, 40 elite, 30 boss and six chest items. Gems totaled 53: 33 natural plus 20 choice-box gems, comprising 52 T1 and one T2. Rune ownership rose from the starting 16 to 75. Early collection remains allowed; gem/rune services open at R10/R15. The two gem boxes formed one stack, so this run selected 20 diamonds.

Warrior attack grew from 30.24 to 56.22; maximum HP rose from 330 to 1,119.51. Gross income was 35,648 gold, 518 materials, 508 stones and 100 abyssal coins. Final balances were 1,481 gold, 518 materials, 448 stones and 100 coins. Acquisitions, equipment, sales, salvage and boxes reconcile with final ownership and currency. All three accounts passed save/reload checks.

With the same observed kills, the long-term proposal predicts approximately 49.84 normal/elite/boss items versus 81 acquired in the current runtime. Guaranteed elite gear versus the proposed 35% chance explains much of the difference. This comparison excludes six chest items and first-clear boxes. **The full drop-volume reduction is not applied.** Current gem/rune collection tiers also remain distinct from the long-term proposal.

### Findings during tuning and final verification

The initial compact-map trial timed out at 300 seconds in R1 because movement stalled at a floor seam. A fixed parametric tolerance became too strict for short movement steps. Regression checks now cover 0.20/0.22/0.30m steps and rejection of a real 1mm gap. Final placement also rechecks 1.2m large-enemy corridor clearance after adding chest/shrine bodies. Failed tuning trials, the intermediate successful route and the final rerun are retained separately in the raw archive.

- Unity 6000.6.0f1: **216 focused Edit Mode tests passed**, no failures or skips. This is not the full test suite. All **24 supplementary item/movement checks passed**, with no failures or skips.
- macOS Development build succeeded with zero build errors. **28 native checks passed**, including 20 display combinations, R25/R30/R35 notices, boundary transitions, save/reload and hero switching.
- Displays: 440×956, 956×440, 1600×900, 1600×1000, 2100×900; Korean/English, 100%/150% text. Automated actions used raycast-verified Unity EventSystem pointer events.
- The naturally earned warrior save was copied into a macOS Player and checked against hero state, full inventory, wallet, gems, runes, first-clear claims and all ten clear records. Native macOS input also opened the full attribute popup. Ten gold settled separately at native startup is excluded from playthrough income.
- The 22 planning-model tests and nine shared-UI tests passed. Physical mobile behavior, population success rates and natural progression through R1000 were not tested.

[Verification data](../Implementation/RiftOnboardingEvidence/verification.json) · [Warrior analysis](../Implementation/RiftOnboardingEvidence/warrior-analysis.json) · [Ranger analysis](../Implementation/RiftOnboardingEvidence/ranger-analysis.json) · [Mage analysis](../Implementation/RiftOnboardingEvidence/mage-analysis.json) · [Raw progression archive](../Implementation/RiftOnboardingEvidence/raw-evidence.zip) · [Unity results](../Implementation/RiftOnboardingEvidence/editmode.xml) · [Native checks](../Implementation/RiftOnboardingEvidence/runtime.txt)

[Native R1](../Implementation/RiftOnboardingEvidence/introductory-rift-native.png) · [R25 unlock](../Implementation/RiftOnboardingEvidence/boundary-before-25.png) · [R30 reward detail](../Implementation/RiftOnboardingEvidence/unlock-detail-440-ko-100.png) · [R35 English 150%](../Implementation/RiftOnboardingEvidence/unlock-detail-956-en-150.png) · [R29 rarity](../Implementation/RiftOnboardingEvidence/rarity-29.png) · [R30 rarity](../Implementation/RiftOnboardingEvidence/rarity-30.png) · [Earned inventory](../Implementation/RiftOnboardingEvidence/inventory.png)

Results apply to `codex/balance-1000-analysis`, isolated from the user's original checkout. Main integration and public wiki deployment have not been performed.
