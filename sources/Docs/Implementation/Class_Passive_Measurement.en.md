# Passive skill measurement and tuning

Updated: 2026-10-05 · [한국어](Class_Passive_Measurement.md)

You asked for the passives to be measured and the weakest tuned first. All 57 passives of the three classes were removed one at a time from real combat to see what each contributes, and the weakest had their numbers and trigger conditions raised. This page records the method, the changes and what is still wrong afterwards. Every figure is a simulation of **a level 40, rank 1, item level 30 hero (no set or legendary gear) fighting 150 seconds of rift 10 and rift 15 under the default Hunt Edict.** None of it was measured on a real device or a real account.

## At a glance

- **Yardstick.** One more skill point on a normal active adds +1.7% damage per second on average (median +0.2%), and the best active in each build adds +6.4% on average. A passive weaker than +1.7% gives little reason to spend that point on it.
- **Before.** 35 of 57 were weaker than that average (10 on par, 7 good, 5 strong).
- **Changes.** 44 passives (12 first-row, 32 new) had their numbers raised and 8 of them also had their trigger condition relaxed. The weakest were tuned first, re-measured, and the ones still weak tuned once more.
- **After.** Weak passives went from 35 to **21**; good or strong ones from 12 to **26**.
- **The remaining 21.** Mostly structural: raising the number does not help (see “Remaining problems”). Fixing them changes what a passive does, so it is left as a decision.

| Verdict | Before | After |
| --- | --- | --- |
| weak | 35 | 21 |
| on par | 10 | 10 |
| good | 7 | 17 |
| strong | 5 | 9 |
| Total | 57 | 57 |

## Method

1. **Remove one at a time.** The same hero and the same random seed fight twice, once with the passive and once without only that passive. The difference is the passive's contribution. This hero wears plain rare gear without set or legendary effects, so a single passive's share can differ in a finished build.
2. **Fight.** 150 seconds of rift 10 and rift 15, with every cast decided by the default Hunt Edict. At rift 15 the hero's lowest HP averages 43–45% and 76–85% of the fights drop below half HP, yet the hero dies in only 2 of all 1,044 fights.
3. **Three setups.** (1) 30 design builds (the full builds as designed, 5–8 seeds); (2) 57 standard kits, one per passive (the actives it needs plus class staples, 4 seeds); (3) 23 assisted kits (the use option of the supporting skill switched on, 4 seeds; 6 of them have no option to switch on and equal (2)). Before/after comparisons use the mean of (2) and (3) only; (1) was measured before tuning only. After tuning used 3 seeds (first pass) and 5 seeds (second pass).
4. **Metric.** Damage, crit, cost, cooldown, movement and utility passives are scored by **the change in damage per second (%)**; defense and shield passives by **the larger of the reduction in damage taken and the protected share (%)**; healing passives by **healing as a share of damage taken (%)**. Defensive and healing metrics are in different units from offensive ones, so equal percentages are not equal value. Resource, protection, healing and trigger counts were also **counted directly per minute** from the combat log.
5. **Noise.** Re-running the same setup with other seeds moves the damage metric by ±1–3 percentage points, so **differences inside about ±3% are not treated as real.** Passives with a small damage effect rely on the direct counts.
6. **Verdicts.** Weaker than the average (+1.7%) is *weak*, up to +3% *on par*, up to the best single point on an active (+6.4%) *good*, above that *strong*. The yardstick was measured as one extra point on each active of 29 design builds: 116 pairs at rift 10, 2 seeds.
7. **How it was tuned.** If a passive triggers often but gives little, the **number** was raised; if its condition is so narrow that it almost never triggers, the **condition** was relaxed (basic hits 3→2, standing still 1→0.5 s, resource 80→60%, HP 50→70%, distance 7→6 m). After the first pass, the 9 passives that were still weak or varied too much between setups (WP07, WP09, WP14, WP17, AP01, AP12, MP07, MP09, MP17) were tuned once more. Passives that were already strong were left alone.

## Changes

Everything was measured at level 40, rank 1. Each further rank adds 10% to the value. “Direct counts” are per minute after tuning; a passive that only adds damage has nothing to count. The description text (Korean and English) and the skill description screens use the same values.

### Warrior

| ID | Name | Type | Change | Measured before → after (%) | Direct counts (after) |
| --- | --- | --- | --- | --- | --- |
| WP01 | Into the Crowd | damage | Added damage +15% → +25% | −1.4 → −1.4 (weak) | not counted |
| WP02 | Endless Spin | cost | Cost reduction +20% → +35% | +0.4 → +1.2 (weak) | not counted |
| WP03 | Landing Stance | defense | Damage taken −15% → −30% | +1.8 → +3.7 (good) | 21 protected/min |
| WP05 | Hardened Will | shield | Shield strength +20 → +40 points | +0.1 → +0.1 (weak) | 0 protected/min |
| WP06 | Executioner's Eye | execute | Added damage +20% → +35% | +2.7 → +5.6 (good) | not counted |
| WP07 | Brand Pursuit | resource | Resource restored 2 → 8 | +3.8 → +4.6 (good) | 5.5 triggers/min · +44 resource/min |
| WP08 | Wound Tracking | damage | Added damage 10% → 20% | +0.6 → +2.8 (on par) | not counted |
| WP09 | Counter Rhythm | resource | Resource restored 4 → 18 | −0.8 → +7.3 (strong) | 2.2 triggers/min · +40 resource/min |
| WP12 | Combat Preparation | cost | 3 → 2 basic hits; cost reduction +15 → +30 points | +0.3 → −0.2 (weak) | 0.5 triggers/min |
| WP13 | Ragged Breathing | regen | HP below 50% → 70%; regeneration +20% → +40% | −0.1 → +1.2 (weak) | not counted |
| WP14 | Battlefield Pressure | cooldown | Cooldown cut 0.5 s → 2.5 s | +1.2 → +1.2 (weak) | 2.7 triggers/min |
| WP15 | Weapon Sequence | damage | Added damage +10% → +20% | +1.7 → +3.6 (good) | not counted |
| WP16 | Hold Formation | defense | Now also applies without a barrier (8%), 12% with one. It used to apply only with a barrier (8%) | +0.0 → +6.6 (strong) | 34 protected/min |
| WP17 | Battle Reserve | damage | Added damage +10% → +15%; resource 80% → 60% or more | +1.2 → +4.6 (good) | not counted |
| WP18 | Ancestral Legacy | resource | Resource per second 1 → 3; cap per cast 8 → 24 | +0.2 → +0.4 (weak) | 1.5 triggers/min · +4 resource/min |

### Ranger

| ID | Name | Type | Change | Measured before → after (%) | Direct counts (after) |
| --- | --- | --- | --- | --- | --- |
| AP01 | Long Reach | damage | Added damage +15% → +30%; distance 7 m → 6 m or more | +0.7 → +3.4 (good) | not counted |
| AP02 | Escaping Step | movement | Movement speed +20 → +35 points | +3.0 → +1.4 (weak) | not counted |
| AP04 | Trap Hunter | utility | Root time +25% → +50% | −0.7 → −5.8 (weak) | not counted |
| AP05 | Saved focus | cost | Cost reduction 25% → 40% | +0.8 → +3.4 (good) | not counted |
| AP06 | Finishing Shot | crit | Critical chance +10 → +20 points | +1.4 → +4.4 (good) | not counted |
| AP07 | Marked Opening | crit | Critical chance +5 → +15 points | +0.6 → +2.6 (on par) | not counted |
| AP08 | Quarry Tracks | movement | Movement speed 10% → 20% | −0.8 → −0.1 (weak) | not counted |
| AP09 | Drawing Again | resource | Resource restored 2 → 4 | +3.0 → +2.3 (on par) | 13.2 triggers/min · +53 resource/min |
| AP10 | Venom Cycle | resource | Resource restored 6 → 12 | +2.1 → +5.2 (good) | 6.1 triggers/min · +74 resource/min |
| AP12 | Prepared Escape | cooldown | Cooldown cut 1 s → 3 s | −1.2 → −1.8 (weak) | 5.7 triggers/min |
| AP13 | Focused Aim | damage | Standing still 0.8 s → 0.5 s; added damage +15% → +30% | +0.3 → +1.9 (on par) | not counted |
| AP14 | Decoy Tactics | damage | Added damage +10% → +20% | +1.4 → +4.1 (good) | not counted |
| AP15 | Venom and Frost | damage | Added damage +12% → +25% | +0.5 → +1.1 (weak) | not counted |
| AP16 | Seamless Reload | cost | Cost reduction +15 → +30 points | +0.8 → +2.3 (on par) | 3.6 triggers/min |
| AP17 | Piercing Crossfire | damage | Piercing shot damage share 50% → 80% | +0.3 → +2.2 (on par) | not counted |
| AP18 | Silent Execution | damage | Extra damage D100% → D200% | +0.7 → +0.3 (weak) | not counted |

### Mage

| ID | Name | Type | Change | Measured before → after (%) | Direct counts (after) |
| --- | --- | --- | --- | --- | --- |
| MP01 | Dense Burn | damage | Added damage +15% → +30% | +2.6 → +7.0 (strong) | not counted |
| MP04 | Stable Ward | shield | Shield strength +20 → +40 points | +0.0 → +0.0 (weak) | 0 protected/min |
| MP07 | Ignition Feedback | resource | Resource restored 3 → 12 | +0.8 → +3.4 (good) | 6.2 triggers/min · +74 resource/min |
| MP08 | Scorched Armor | defense | Damage taken −8% → −15% | +4.4 → +8.1 (strong) | 36 protected/min |
| MP09 | Opening in the Cold | resource | Resource restored 3 → 12 | +0.0 → +3.4 (good) | 5.2 triggers/min · +62 resource/min |
| MP11 | Overcharge | damage | Added damage +15% → +30% | +0.2 → +0.9 (weak) | not counted |
| MP12 | Reclaim Charge | resource | Resource restored 10 → 30 | +0.1 → +0.8 (weak) | 0.4 triggers/min · +12 resource/min |
| MP13 | Ward Breathing | regen | Resource regeneration +15% → +30% | +1.5 → +1.4 (weak) | not counted |
| MP14 | Mana Thrift | cost | Cost reduction +15 → +30 points | +2.4 → +2.9 (on par) | 5.6 triggers/min |
| MP15 | Steady Casting | defense | Standing still 1 s → 0.5 s; damage taken −8% → −15% | +1.8 → +6.0 (good) | 26 protected/min |
| MP17 | Overflowing Mana | damage | Added damage +10% → +20%; resource 80% → 60% or more | +0.4 → +0.5 (weak) | not counted |
| MP18 | Archmage's Testament | resource | Resource restored 15 → 50 | −0.6 → +1.9 (on par) | 0.7 triggers/min · +33 resource/min |
| MP19 | Sage's Ward | defense | Barrier bonus 10% → 20% of maximum HP | +0.0 → +0.0 (weak) | 0 protected/min |

## 13 passives left unchanged

| ID | Name | Type | Measured (%) | Reason |
| --- | --- | --- | --- | --- |
| WP04 | Blood Recovery | healing | +38.5 (strong) | Already among the strongest; lowering it is a decision for you. |
| WP10 | Lingering Shout | healing | +27.8 (strong) | Already among the strongest. |
| WP11 | Stone Landing | defense | +17.3 (strong) | Already strong. |
| WP19 | Titan's Bulwark | defense | +5.2 (good) | Above the yardstick. |
| AP03 | Piercing Gaze | damage | +0.7 (weak) | It raises a target count, so a number cannot fix it (see item 5 below). |
| AP11 | Alchemical Practice | defense | +5.2 (good) | Above the yardstick. |
| AP19 | Relentless March | utility | +0.7 (weak) | It lengthens an ultimate (see item 2 below). |
| MP02 | Deep Chill | utility | +7.4 (strong) | Above the yardstick. |
| MP03 | Chain of Lightning | damage | +2.7 (on par) | Close to the yardstick; it adds chain hits. |
| MP05 | Mana Circulation | regen | +5.2 (good) | Above the yardstick. |
| MP06 | Element crossover | damage | +5.4 (good) | Above the yardstick. |
| MP10 | Ice Reverberation | defense | +24.3 (strong) | Already among the strongest. |
| MP16 | Threefold Memory | defense | +2.2 (on par) | Close to the yardstick. |

## Remaining problems and decisions

21 passives are still weak after tuning. They are grouped by cause below; fixing them needs a different **kind of effect or a different default edict**, not a different number, so it was not done here.

| ID | Name | Measured (%) | Cause |
| --- | --- | --- | --- |
| AP04 | Trap Hunter | −5.8 | 4. Narrow condition or small share of damage |
| AP12 | Prepared Escape | −1.8 | 3. Cooldown cut |
| WP01 | Into the Crowd | −1.4 | 4. Narrow condition or small share of damage |
| WP12 | Combat Preparation | −0.2 | 4. Narrow condition or small share of damage |
| AP08 | Quarry Tracks | −0.1 | 4. Narrow condition or small share of damage |
| MP04 | Stable Ward | +0.0 | 1. Defensive skill the default edict never casts |
| MP19 | Sage's Ward | +0.0 | 2. Attached to an ultimate |
| WP05 | Hardened Will | +0.1 | 1. Defensive skill the default edict never casts |
| AP18 | Silent Execution | +0.3 | 2. Attached to an ultimate |
| WP18 | Ancestral Legacy | +0.4 | 2. Attached to an ultimate |
| MP17 | Overflowing Mana | +0.5 | 4. Narrow condition or small share of damage |
| AP19 | Relentless March | +0.7 | 2. Attached to an ultimate |
| AP03 | Piercing Gaze | +0.7 | 5. Adds a target count |
| MP12 | Reclaim Charge | +0.8 | 6. Resource restore |
| MP11 | Overcharge | +0.9 | 4. Narrow condition or small share of damage |
| AP15 | Venom and Frost | +1.1 | 4. Narrow condition or small share of damage |
| WP14 | Battlefield Pressure | +1.2 | 3. Cooldown cut |
| WP13 | Ragged Breathing | +1.2 | 6. Resource restore |
| WP02 | Endless Spin | +1.2 | 4. Narrow condition or small share of damage |
| MP13 | Ward Breathing | +1.4 | 1. Defensive skill the default edict never casts |
| AP02 | Escaping Step | +1.4 | 4. Narrow condition or small share of damage |

1. **Defensive skills the default edict never casts.** Iron Wall (W05), Elemental Shield (M05), Teleport (M04), Firewall (M08) and Forking Arrow (A14) were never cast in 150-second fights of the design builds, and Patient Shot (A08) was cast 0.08 times per minute. The standard kit that uses Iron Wall (Hardened Will (WP05)) and the one that uses Elemental Shield (Stable Ward (MP04)) were also run at rift 20, 25 and 30. At rift 20 the hero's lowest HP was 13~38%, and at 25 and 30 heroes died, yet there were 0 shield casts and removing the shield passive changed neither damage nor losses. The cause is not confirmed, but the edict's *designated defense skill* defaults to Ground Slam (W04) and the *dodge forecast policies* default to off. If so, Hardened Will (WP05), Stable Ward (MP04) and Ward Breathing (MP13) do nothing until a player changes the edict. → **Please decide whether the default edict should use the shield skills.**
2. **Passives attached to an ultimate.** Ultimates are cast only 0.3–1 times a minute (War of the Ancestors 0.3, Titan's Judgment 0.5, Afterimage March 0.7, Sage Incarnate 0.8, Killing Rain 0.8, Triune Collapse 1.0), so a passive gets one or two chances per 150-second fight. Removing the extra barrier of Sage's Ward (MP19) left the result at rift 10 and 15 exactly the same (the barrier ends before it is used up), and at rift 20 and 25 the protection differed by only 1–3%. → **Please decide whether to keep ultimate passives or to strengthen the ultimates themselves.**
3. **Cooldown cuts.** Battlefield Pressure (WP14) and Prepared Escape (AP12) trigger often (2.5 and 5.9 per minute), but the skills they speed up are almost never waiting on cooldown. Even cutting 10 seconds moved damage per second by only +1.6% (WP14) and +0.1% (AP12). → The kind of effect has to change, not the number.
4. **Narrow conditions or a small share of damage.** Combat Preparation (WP12) triggers only 0.5 times a minute even with the condition relaxed (the Warrior's basic attacks run 1.7 times a minute in this kit). Overflowing Mana (MP17) affects only the first direct hit and needs high resource; raising its bonus from +20% to +100% still moved damage per second by only +2.3% (±4.4). Overcharge (MP11), Venom and Frost (AP15), Trap Hunter (AP04), Into the Crowd (WP01) and Endless Spin (WP02) reach a small share of damage or sit inside seed noise, and the movement passives (Escaping Step (AP02), Quarry Tracks (AP08)) seem to have a weak path to damage.
5. **A passive that adds a target count.** Piercing Gaze (AP03) lets Pierce hit up to 7 instead of 5 enemies but showed almost no effect. Six or seven enemies rarely stand on one line, which is probably why, but that was not checked, and there is no number left to raise.
6. **Resource restore.** The value of these passives is roughly proportional to the resource restored per minute. Passives that restore about 40–70 per minute in the standard kits (Brand Pursuit (WP07), Counter Rhythm (WP09), Ignition Feedback (MP07), Venom Cycle (AP10)) are worth about +5%. Reclaim Charge (MP12) restores only when an orb expires naturally, 0.4 times a minute, so 30 each time is 12 per minute; Ancestral Legacy (WP18) hangs on an ultimate and restores 4.5 per minute. Ragged Breathing (WP13) and Ward Breathing (MP13) only speed up regeneration while their condition holds, so they restore little.
7. **Passives that are too strong.** Blood Recovery (WP04) +38.5%, Lingering Shout (WP10) +27.8%, Ice Reverberation (MP10) +24.3% and Stone Landing (WP11) +17.3% far exceed the best single point on an active (+6.4%). They are defensive or healing metrics in different units, but the gap is large, so this is only reported. → **Please decide whether to lower them.**
8. **Limits.** One hero (level 40, rank 1, item level 30) and rift 10 and 15 only. Defensive passives may behave differently at stages where the hero is in more danger, and offensive ones in a build with set and legendary gear. In particular cost reduction (cap 50%), damage-taken reduction (combined cap 50%) and critical chance (cap 75%) share their caps with gear and sets, so in a hero with set and legendary gear the passives that raise them (WP02, WP03, WP12, WP16, AP05, AP06, AP07, AP16, MP08, MP14, MP15) add less than measured. With 3–8 seeds, differences inside ±3 points cannot be told apart. Only 23 setups were measured with changed use options. The numbers used are one balancing pass, not a final balance.

## Verification

The checks ran in a copy of the working folder; the open Unity Editor and account saves were not used. The same checks ran once on the branch before merging and once on the merged tree.

- **Data and generated files.** The `check` of `class_skill_runtime`, `class_skills`, `class_skill_options` and `class_skill_reports`, `node tools/build_skill_tree.cjs --check`, `node Prototypes/SkillTree/build.cjs` (all 111 skills reachable by level), `engine.test.cjs` and the shared UI checks (`check_ui_contract.py`, `test_ui_contract.py`, `check_ui_refresh.py`) pass. They were repeated on the merged tree and the generated files did not change.
- **Unity Edit Mode, whole suite, branch before merging.** All 5,162 tests except the explicit-only measurement tool ran once each, split into several class batches. Nine failed on the first run, all of them checks with old passive numbers written into the code (for example a root time of 1.875 s, a shield multiplier of 1.2, a damage reduction of 0.25). After they were changed to read `SkillEffects.PassiveBase` and the skill data, the affected classes were run again and passed. The run log and the list of failures and fixes are in [editmode-runs.json](ClassPassiveMeasurementEvidence20261005/editmode-runs.json); the report of the 666 skill and passive tests is [passive-tuning-editmode.xml](ClassSkillEvidence/passive-tuning-editmode.xml).
- **Unity Edit Mode, whole suite, merged tree.** On `origin/main` (17dc75e9, which already contains the tutorial chronicle and the item card ornaments) plus the skill-tree branch and this branch, 5,236 tests ran in six batches and 5,235 passed. The last one (`ObjectiveProgressTests.ExplorationWalksToEveryKnownOfferingAndDeliversWithoutTeleporting`) exceeded the default 180 s limit. It fails the same way on `origin/main` before the merge (190.0 s there, 189.9 s on the merged tree) and passed the branch before merging at 176.4 s, so it is a borderline timeout unrelated to this work.
- **Smokes on a macOS development build.** The merged tree and `origin/main` were built the same way and the same eight smokes ran on both with identical outcomes. The skill tree (37 nodes, four slots, fixed control positions), skill reset, skill preset and class skill smokes passed on both launches (start and restart). The skill menu and edict sections smokes (the repeat-hunt tab is locked), the quick preset smoke (the new potion group opens its advanced options at once) and the tutorial smoke (after the restart it waits for the survival lesson target until it times out) fail on `origin/main` for the same reasons, so they are existing problems. Every tree-family smoke had been broken since staged disclosure (93d59959) stopped treating its account as a legacy account and hid slots, styles and presets, so two lines in `RuntimeSkillTreeSmoke.ExistingTrees` now switch legacy access on. The record is [merge-validation.json](ClassPassiveMeasurementEvidence20261005/merge-validation.json).
- **Recorded results refreshed.** The [90 build comparisons](ClassSkillEvidence/Build_Comparison.en.md) and [18 use-policy comparisons](ClassSkillEvidence/Policy_Comparison.en.md) that `ClassSkillRuntimeTests` writes were regenerated with the new numbers. 51 of the 90 build fights changed, damage by +2.1% on average (−4.4 to +14.8%), and the hero survived all 90.
- **Measurement evidence.** [passive-measurement.json](ClassPassiveMeasurementEvidence20261005/passive-measurement.json) (before/after for all 57) and [probes.json](ClassPassiveMeasurementEvidence20261005/probes.json) (deeper rifts, oversized values, cast rates under the default edict).
- **Not done.** The changed description text was not looked at on a device or in a macOS development build. No further whole-suite run follows the merge; the record above is the tree just before it.

## How to measure again

The tool is `ClassSkillPassiveMeasurement` (an Edit Mode test marked `Explicit`, so it is not part of the normal suite). An open Unity Editor locks the project, so run it in a **copy of the working folder** (`cp -Rc` on macOS is fast). Every environment variable is listed in the comment at the top of the file.

```bash
# One passive in its standard kit, rift 10 and 15, 4 seeds, 150 s
HELLSCRIPT_MEASURE_OUT=/tmp/passive-kit HELLSCRIPT_MEASURE_KITS=1 HELLSCRIPT_MEASURE_BUILDS=WP07,AP12 \
HELLSCRIPT_MEASURE_SEEDS=4 HELLSCRIPT_MEASURE_SECONDS=150 HELLSCRIPT_MEASURE_SCENARIOS=rift10,rift15 \
/Applications/Unity/Hub/Editor/<version>/Unity.app/Contents/MacOS/Unity -batchmode -nographics -projectPath <copy> \
  -runTests -testPlatform EditMode -testFilter Hellscript.Tests.ClassSkillPassiveMeasurement
```

- `HELLSCRIPT_MEASURE_VALUES="WP07=8;MP07=12"` tries candidate numbers without regenerating data (`value` for the new passives, the percent in `SkillEffects.PassiveBase` for the first-row passives xP01–xP06).
- `HELLSCRIPT_MEASURE_ASSIST=1` measures the assisted setup and `HELLSCRIPT_MEASURE_YARDSTICK=only` the yardstick (one point on an active). The script that merges the resulting `passive-measurement.json` files is not kept in the repository; the merged results are in the [measurement evidence](ClassPassiveMeasurementEvidence20261005/passive-measurement.json).
- When changing a number, edit `Docs/Design/ClassSkills/runtime_parameters.json` (new passives) or `SkillEffects.PassiveBase` (first-row passives) together with the `catalog.json` description, then run `python3 tools/class_skill_runtime.py build`, `python3 tools/class_skills.py build` and `node Prototypes/SkillTree/build.cjs`. First-row descriptions also live in `GameCatalog.PassiveDescriptions` and `Localization/en.txt`; keep them in step.
