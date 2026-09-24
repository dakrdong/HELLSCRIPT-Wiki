# Rift 1–10 natural progression: time, failures and rewards

2026-09-24 · One fresh Warrior account · `codex/balance-1000-analysis` · Production baseline `8d5725ec`

The continuous playthrough reached a normal Rift 10 clear after **13 attempts: 10 clears and 3 deaths**. Foreground rift time was **33:08.75**, including **7:47.30 spent on failed attempts**. The character finished at **level 15**. This is accelerated execution of the production `CombatSimulation` and `GameStore`, not an analytical projection or a manually controlled playthrough.

## Method

The account began at level 1, zero XP/currencies, and one normal starter weapon. The game's default 16 runes and starter potions were retained. Production admission, fatigue, AI navigation, attacks, XP, rewards, transactions, equipment rules, potion purchases, forge durations and save/reload were used. No levels, gear, clears, health or currency were injected. Encounters used a rule chosen before execution: `20260924 + 1337 * attempt`. Failed attempts were retained; no outcome-driven reseeding occurred.

Every 0.05-second combat step consumed 0.05 seconds of 1x foreground attendance. Rendering and town interaction were omitted. Harness wall time was **4:48.24**, excluding compilation, build and analysis. The **33:08.75 measurement includes combat, automatic movement and loot collection but excludes town/UI deliberation**. Assuming 30 seconds for each of the 12 visits between attempts would add 6 minutes, yielding about **39:09**; that extra time is an assumption, not a measurement.

Between attempts, the shared equipment comparison selected improvements greater than 0.5% in `basic-attack expectation * sqrt(mixed effective health)`. This heuristic does not fully value legendary procs or set combinations. Legendaries and future-level items were retained. Empty skill slots were filled in catalog order and equipped skills ranked evenly; points earned during combat were invested only after returning. Enhancements were capped at +1 before R5 and +2 thereafter, with a 700-gold reserve. Two free forge stations used production timers. No lower-stage farming, paid fatigue, sweeps or claimed offline income was needed. Rare crafting remained unused because no eligible empty slot required it. Gem boxes were opened after R10; sockets were not installed.

## Time and failures

Stage time includes failures. Cumulative time ends at the first normal clear. Attack is the value after subsequent town maintenance.

| Rift | Attempts | Failures | Stage time | Cumulative to clear | Level | Attack after town |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| 1 | 3 | 2 | 7:13 | 7:13 | 8 | 44.75 |
| 2 | 1 | 0 | 2:25 | 9:37 | 9 | 45.38 |
| 3 | 1 | 0 | 2:17 | 11:54 | 10 | 45.92 |
| 4 | 1 | 0 | 2:23 | 14:18 | 10 | 44.80 |
| 5 | 1 | 0 | 2:37 | 16:54 | 11 | 47.48 |
| 6 | 1 | 0 | 2:35 | 19:29 | 12 | 47.50 |
| 7 | 1 | 0 | 2:48 | 22:16 | 13 | 51.43 |
| 8 | 1 | 0 | 2:37 | 24:53 | 13 | 50.43 |
| 9 | 2 | 1 | 5:46 | 30:39 | 14 | 57.44 |
| 10 | 1 | 0 | 2:29 | 33:09 | 15 | 57.65 |

Deaths occurred at R1 attempt 1 (**139.10 s**), R1 attempt 2 (**155.55 s**) and R9 attempt 1 (**172.65 s**). There were no timeouts, abandonment or fatigue exhaustion. The observed 10/13 success fraction is **76.9%**, not an estimate of population win rate. **86:51.25** remained in the free daily 120-minute allowance.

## Acquisition and spending

All **135 generated equipment drops were collected**: 8 common, 56 magic, 63 rare and 8 legendary. First-clear boxes added one rare weapon and one legendary weapon, for **137 acquired items excluding the starter**.

| Source | Common | Magic | Rare | Legendary | Total |
| --- | ---: | ---: | ---: | ---: | ---: |
| 995 normal kills | 8 | 4 | 5 | 0 | 17 |
| 78 elite kills | 0 | 44 | 32 | 2 | 78 |
| 10 bosses | 0 | 0 | 24 | 6 | 30 |
| Field chests | 0 | 8 | 2 | 0 | 10 |
| First-clear boxes | 0 | 0 | 1 | 1 | 2 |

One golden goblin spawned and escaped, granting no kill or reward. The following includes failed runs and automatic per-character first-clear currency, but excludes town box opening, salvage and sales.

| Rift | Common / magic / rare / legendary | Gold | Materials | Stones | Gems | Runes |
| --- | --- | ---: | ---: | ---: | ---: | ---: |
| 1 | 2 / 13 / 13 / 1 | 3,994 | 16 | 12 | 4 | 11 |
| 2 | 1 / 4 / 6 / 1 | 2,931 | 15 | 14 | 4 | 5 |
| 3 | 0 / 4 / 6 / 0 | 3,142 | 15 | 16 | 2 | 9 |
| 4 | 0 / 4 / 6 / 1 | 3,430 | 16 | 18 | 4 | 7 |
| 5 | 2 / 6 / 5 / 0 | 3,692 | 17 | 20 | 4 | 6 |
| 6 | 1 / 2 / 5 / 2 | 3,937 | 17 | 22 | 3 | 5 |
| 7 | 0 / 4 / 6 / 0 | 4,067 | 17 | 24 | 2 | 9 |
| 8 | 0 / 5 / 4 / 1 | 4,414 | 17 | 26 | 5 | 10 |
| 9 | 1 / 8 / 8 / 2 | 6,208 | 18 | 28 | 9 | 8 |
| 10 | 1 / 6 / 4 / 0 | 4,915 | 19 | 30 | 2 | 5 |

| Resource | Gross inflow | Spent | Final balance |
| --- | ---: | ---: | ---: |
| Gold | 40,910 | 38,042 | 2,868 |
| Materials | 631 | 0 | 631 |
| Enhancement stones | 593 | 80 | 513 |
| Abyss currency | 100 | 0 | 100 |
| Gems | 59 | 0 | 59 |
| Runes | 75 earned + 16 starter | 0 | 91 |

Gold spending was 36,612 on equipment upgrades and 1,430 on potions. Eight slot jobs cost 80 stones; six level-2 slots were complete and two jobs remained pending. Gross material/stone inflow includes 394/333 from salvage and 70/50 from boxes. Gems comprise 39 natural drops plus 20 box rewards: 58 tier-1 and one tier-2 gem. Both choice boxes were in one stack and selected **G01 diamonds (20 total)**. The original policy string's G01/G03 wording does not describe the actual choice as ten of each. Early gem/rune acquisition and access to their services currently have separate gates.

R10 boss settlement granted **three rare items, 1,300 gold, seven materials, 30 stones and two gems (one tier 1, one tier 2)**. The current code settles boss and repeat-clear rewards together. The character's automatic first-clear bonus was **2,000 gold and 12 materials**. The separately claimed account reward granted **two stones, 20 materials, a legendary weapon box, two tier-1 gem choice boxes of ten each, and 100 Abyss currency**. All ten account claims required normal-clear records and persisted after reload.

## Differences from the proposal

| Metric at R10 | Proposal | Observed path |
| --- | --- | --- |
| Cumulative rift time | 57:36 | 33:08.75 |
| Character level | 5 | 15 |
| Legendary introduction | First guarantee at R10; no earlier natural drops | First natural legendary on second R1 attempt |
| Legendary ownership/equipment | Median one equipped | Eight natural + one guaranteed owned; zero equipped by the stated heuristic |
| Item level | Reference level 4 | Equipped levels 1–12; guaranteed weapon level 4 |
| Attack | 52.36 | 57.65 |
| HP | 739.42 | 1,309.99 |
| Cumulative gems | About 25.9 | 59 |

The proposal assumes repeat farming, 90% success and 110 normal/eight elite kills per success. Applying its proposed probabilities to the **same observed normal/elite kills and ten bosses** yields **67.25 expected items**, versus **125 acquired** from those sources. This excludes field chests and first-clear boxes. Expected natural legendaries are approximately **0.10**, versus **eight observed**. These numbers demonstrate a difference between unapplied proposal rules and current code; one trajectory does not estimate true drop probabilities.

Current normal/elite equipment chances remain 2%/100% versus the proposed 1%/35%. Legendary probabilities still begin at R1 at 1%/5%/15% for normal/elite/boss sources. First-clear weapons already use the proposed lower item-level curve, so naturally acquired gear can outlevel the guarantee. Zero legendaries equipped is conditional on the sheet-based selection policy; it does not prove that their full proc-based combat performance is inferior.

## Recommended tuning order

1. Tune initial enemies and XP together. Two deaths before the first clear, while already reaching level 6, are the first onboarding problem. This path earned 20,121 XP. Level 5 spans 970–1,549 cumulative XP, or roughly 4.8–7.7% of the observed amount. This is a fixed-history calculation, not proof that an XP-only reduction would remain playable. Retune starting HP/attack and skill-investment guidance, then replay.
2. Apply normal/elite/boss rarity and equipment-level curves together. Re-evaluate guaranteed weapon usefulness with legendary effects after natural drops follow the same curve.
3. Decide whether gems/runes may accumulate before service unlocks; communicate stored stock and the required stage if they do.
4. Replace assumed kill counts and zero failed-run income with observed data, and model town time explicitly.
5. Repeat multiple seeds for all three classes before expanding to R30/60/100. This path does not validate progression to R1000.

## Evidence and scope

[Analysis and stage records](../Implementation/NaturalRift10Evidence/analysis.json) · [Raw attempts, actions, saves and executed harness](../Implementation/NaturalRift10Evidence/raw-evidence.zip) · [Verification status](../Implementation/NaturalRift10Evidence/verification.json)

The [audit runner](../../Assets/HELLSCRIPT/Editor/NaturalRiftProgressionAudit.cs) uses production owners. The [ledger validator](../../tools/report_natural_rift_progression.py) checks time totals, admissions, ten normal clears/claims, equipment conservation and all resource balances. The original observer counted the escaped goblin's `dead` flag and left hero-adjacent boss drops unattributed. Saved production state and the boss settlement contract correct these labels; raw data, outcomes, times and rewards remain unchanged.

Save/reload matches were verified. Native macOS readback and a real pointer click on the reward-box button passed. All 103 focused Edit Mode tests, the shared UI contract plus nine checks, ten wiki tests and JavaScript route checks passed. The [reloaded inventory](../Implementation/NaturalRift10Evidence/inventory.png) shows the final gear, currencies and character level. No physical-mobile play is claimed. Runtime balance was not changed; two older rarity test fixtures were updated to the current R60 sweep and R25 reroll gates. This is branch evidence; main integration and [public wiki](https://dakrdong.github.io/HELLSCRIPT-Wiki/#/tree) publication remain pending.
