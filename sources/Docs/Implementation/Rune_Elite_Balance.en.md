# HELLSCRIPT Rune Elite Balance Measurement

Date: 2026-09-17
작성일: 2026-09-17

## Why this was measured

[Elite Abilities at the Rune Region Centres](Rune_Elite_Centres.en.md) carried the mock-up's draft numbers in unchanged, and that record ends with "they have not been measured in real combat". This is that measurement.

## Method

The existing archetype balance runner (`RunMultiSeedBalanceSimulation`) gained one parameter. With `eliteRegion` 0 it is the sweep as it always ran; with 1–6 it covers that region's centre on the hero's own weapon board and nothing else. That one rune is the whole layout.

- 3 classes × 2 variants = 6 archetypes, level 30, item-level 30 gear, recommended builds.
- The baseline and all six elites use the **same seeds, the same gear and the same builds**. The only difference between two rows is the one elite the row names.
- Tier 10: 8 seeds → 48 runs per condition. Clears have room, so damage is readable.
- Tier 25: 4 seeds → 24 runs per condition. Deaths happen, so survival is readable.

A first attempt at tier 20 with 1 seed was unusable. Clear times of 286–300 s sat against the 300 s limit, so they were censored, and with one seed the trajectory difference was pure noise. An elite kills enemies sooner and changes everything that follows, so runs diverge chaotically; averaging over more seeds is the only way through.

## Tier 10 — damage

48 runs per condition, all cleared. DPS is total damage over total time across winning runs.

| Elite | DPS change | Clear time | SD across archetypes | Positive in |
|---|---|---|---|---|
| Combat Heat | **+2.93%** | −3.05% | 1.62 | 6 / 6 |
| Chain Collapse | **+2.12%** | −2.26% | 2.63 | 4 / 6 |
| Skill Inheritance | **+2.00%** | −2.09% | 0.72 | 6 / 6 |
| Exposed Weakness | +0.48% | −0.55% | 0.76 | 4 / 6 |
| Cycle Core | −0.38% | +0.46% | 0.76 | 2 / 6 |
| Unyielding Ward | 0.00% | 0.00% | 0.00 | — |

**The noise floor is about ±0.8%**: that is the archetype-to-archetype SD of the conditions that do nothing. Read against it,

- Combat Heat and Skill Inheritance are clear signal: positive in all six archetypes, and Inheritance is very even at SD 0.72.
- Chain Collapse has the higher mean but an SD of 2.63. It is +7.16% on Ranger Variant 2 and −0.17% on Warrior Variant 1. It depends heavily on the class.
- Exposed Weakness sits on the noise floor. **Its row in this table is the pre-buff value**; see the re-measurement below.
- Cycle Core and Unyielding Ward do not change damage at tier 10.

Unyielding Ward produced numbers **identical to the baseline**, meaning it never fired: nothing at tier 10 takes the hero below 35% life.

## Tier 25 — survival

24 runs per condition. This tier is far above the recommended builds — the baseline wins 0% — so the win rate is not a balance target. What is readable is **whether deaths went down**.

| Elite | Cleared | Hero deaths | Time limit | Related events per run |
|---|---|---|---|---|
| Baseline | 0 | **9** | 15 | 0 |
| Unyielding Ward | 0 | **2** | 22 | 10.9 |
| Cycle Core | 1 | **3** | 20 | 31.2 |
| Skill Inheritance | 2 | 6 | 16 | 0 |
| Exposed Weakness | 1 | 7 | 16 | 36.2 |
| Chain Collapse | 0 | 7 | 17 | 41.1 |
| Combat Heat | 2 | 8 | 14 | 41.4 |

**Two conclusions drawn from tier 10 alone were wrong.**

- Unyielding Ward is not dead content. Where there is a real threat it cuts deaths from 9 to 2. It was invisible at tier 10 only because nothing there crosses the threshold.
- Cycle Core does fire — 31 times a run, trimming 31 seconds of cooldown across a 300 second run. But it shows up as **survival, not damage**: deaths 9 → 3. "The equipped active with the longest remaining cooldown" is usually the escape or defensive skill, so that is what comes back sooner.

The six therefore split into three damage-leaning (Heat, Collapse, Inheritance), two survival-leaning (Ward, Cycle) and one that contributes to both (Inheritance). None of them is dead.

The event counts are an activity indicator, not an activation count: Heat counts both stacks gained and their expiry, and the Ward counts creation, absorption and expiry. Skill Inheritance only changes a number, so it raises no events.

## Compared with one board hex

Skill Inheritance gives +1 level to each equipped active, up to four, and one level is +10% on that skill's damage coefficient. It measured +2.00%, which puts **one skill-level hex at roughly +0.5%**. Skill-level hexes are the most expensive cells on the board.

On that conversion,

| Elite | Worth about |
|---|---|
| Combat Heat | 6 skill-level hexes |
| Chain Collapse | 4 hexes |
| Skill Inheritance | 4 hexes (by construction) |
| Exposed Weakness | about 1 hex before the buff, about 2 after |
| Cycle Core, Unyielding Ward | 0 for damage, large for survival |

Four to six of the most expensive hexes, as the reward for opening a whole region, is not excessive. The conversion is an estimate and moves with how much of a build's damage comes from skills.

## Buffing Exposed Weakness, and re-measuring

The first measurement made Exposed Weakness the weakest of the six (+0.48%, the noise floor), and the user asked for it to be raised.

The measurement said which knob to turn. Combat Heat carries the same +10% additive at five stacks and measured +2.93%, so the ceiling for "hold +10% additive" is about +2.9% — and Exposed Weakness was collecting **17%** of it. The limit was coverage, not size, so raising the number alone would not have helped.

Both limits were therefore raised together: **4 s → 8 s and +10% → +20%.** The critical trigger and the single target were left alone; they are the ability's identity and the mock-up's specification says "the target hit". The two values sit in `CombatSimulation.Damage.cs` as `ExposedSeconds` and `ExposedBonus` so the next tuning pass can find them.

Only the baseline and Exposed Weakness were re-run, on the same seeds and gear. The baseline's DPS came back as 101.52, identical to the first measurement, which confirms both runs share one fixture.

| | Before | After |
|---|---|---|
| DPS (tier 10, 48 runs) | +0.48% | **+0.96%** |
| Clear time | −0.55% | −0.93% |
| Archetype mean / SD | +0.50 / 0.76 | +1.05 / 0.85 |
| Positive in | 4 / 6 | 5 / 6 |
| Deaths (tier 25, 24 runs) | 9 → 7 | 9 → 7 |

**On paper that is 4×; measured it is 2×.** The single target is why. At tier 10 the exposure lands 19.5 times a run, about once every 9.6 s, so an 8 s duration already covers roughly 83% of the time on that target. More duration buys almost nothing, and in a fight with several enemies most of the hero's damage lands on enemies that are not the exposed one.

Magnitude does scale close to linearly, but reaching the +2% band would need +40–50%, and a single-target +50% does not sit well beside Combat Heat's +10% against everything.

**The remaining knob is target coverage.** Letting a critical expose nearby enemies as well would likely reach the band, but that changes the ability's shape rather than its numbers and departs from the specification's "the target hit". That needs a decision, so this is where it stopped.

Even after the change Exposed Weakness is the lowest of the six on damage. It is, however, now above the ±0.8% noise floor and positive in five archetypes of six.

## Judgement

No balance number other than Exposed Weakness was changed. What follows is what the measurement suggests.

1. **Exposed Weakness was raised and is still the lowest.** See the section above. The next step is a decision about target coverage, not a number.
2. **Chain Collapse depends far too much on the class**: +7% on Ranger, 0% on Warrior. Part of the cause is that the burst goes out with `additive=0`, so it receives no elemental, conditional or vulnerable bonus at all. That is the literal reading of the specification's "30% of attack basis D", and whether that sentence should stand is a decision.
3. **Cycle Core's name and behaviour disagree.** It reads as a resource ability and measures as a survival one, because of the rule that picks the longest remaining cooldown. If damage was the intended role, the target rule has to change.
4. **The other three — Heat, Inheritance and the Ward — earn their place at the current numbers,** and none of them is excessive.

## Scope and limits

- The tier 25 death counts come from 24 runs per condition. 9 → 2 is a large move; 9 → 6 could be chance. These are directions, not conclusions.
- A 0–8% win rate at tier 25 means the tier is beyond these builds. It is a stress fixture, not a balance target.
- This is not a claim about natural growth, real player input, mobile hardware or released balance. It is a macOS batch simulation.
- The raw reports are kept as [tier 10, 8 seeds](RuneEliteBalanceEvidence/tier10-8seeds.json) and [tier 25, 4 seeds](RuneEliteBalanceEvidence/tier25-4seeds.json).
- The re-measurement after the Exposed Weakness change is kept separately as [tier 10](RuneEliteBalanceEvidence/weakness-buffed-tier10-8seeds.json) and [tier 25](RuneEliteBalanceEvidence/weakness-buffed-tier25-4seeds.json). The other five elites do not appear in those two reports.

## Repeating it

```
Unity -batchmode -nographics -projectPath <copy> \
  -executeMethod Hellscript.Editor.BuildIntegrationValidation.RunEliteBalanceSweep \
  -hellscriptEliteOutput <folder> -hellscriptEliteSeeds 8 -hellscriptEliteTiers 10 -quit
```

The editor menu `HELLSCRIPT/룬 엘리트 밸런스 시뮬레이션 실행` does the same. Running all six took about 70 minutes at tier 10 with 8 seeds, and about 60 at tier 25 with 4.

`-hellscriptEliteRegions 4` narrows the run to one region; the baseline is always measured, so the comparison is never missing. One elite at tier 10 with 8 seeds takes about 20 minutes. There is no reason to run all six while tuning one.
