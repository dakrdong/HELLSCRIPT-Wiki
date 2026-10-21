# Combat Implementation Spec (for Codex)

Updated: 2026-10-12 · For: Codex implementation work · [한국어](D4_Skill_Tree_Combat_Spec_20261012.md)

User correction, 2026-10-21: area damage and control affect every enemy inside the footprint. Preserve designed projectile pierce, chain, fork and scatter counts. This rule supersedes the earlier area target caps below.

Author: Claude (specification and research). Codex implements, verifies and commits.

This document expands section 8 (combat) of the plan document [`D4_Skill_Tree_Plan_20261012.en.md`](D4_Skill_Tree_Plan_20261012.en.md) to a level at which it can be implemented. Its basis is the research notes, written by reading the code only (`Docs/Design/D4SkillTree/research/combat_{warrior,ranger,mage}.md`, `save_migration.md`, `dependents.md`). **Line numbers are against `main` `a3fe67c5`**, so they drift when the code changes. Find the code again by function and class name. The source of the numeric values is `skill_tree_d4.json` and `golden/rank_values.json`; for rules and decisions, the plan document takes precedence.

## 0. Scope

- In scope: per-rank values for the 30 actives (10 per class, including 2 ultimates), the **84 modifiers**, the 24 passives and the **6 key passives**, the 3 ultimate ranks, cleanup of the absorbed and removed legacy skills, and where all of this meets auto-battle.
- Out of scope: the skill window screen (UI spec), save and migration (plan section 7, step 2), icons (step 3), and tutorial, edict and quick edit (step 6). This document lists only what those steps **require from the combat side**.
- All combat changes are switched on behind `SkillTreeD4.Enabled`, and the shipping path does not change until step 7.

**Classification result of the 84 modifiers** (the categories are defined in section 3):

| Class | A (values only) | A* (needs a modifier value accessor) | B (new behaviour on an existing path) | C (new mechanism or state) | Total |
|---|---|---|---|---|---|
| Warrior | 6 | 0 | 18 | 4 | 28 |
| Ranger | 1 | 1 | 19 | 7 | 28 |
| Mage | 0 | 3 | 21 | 4 | 28 |
| **Total** | **7** | **4** | **58** | **15** | **84** |

## 1. What the new system requires from combat (summary)

1. A base skill has **ranks 1–5** (ultimates 1–3). Per-rank values are linear `from → to` (`golden/rank_values.json` is the golden file).
2. For each skill you choose **one** modifier (only one takes effect), and its rank is at most `min(base rank, max)`. A modifier effect changes the **shape or the extra effect of the base skill**.
3. Passives unlock by hero level only (no separate condition in combat), and of the two key passives only one can hold points.
4. Only the 4 equipped actives plus 1 ultimate are cast. A skill that has not been learned (rank 0) cannot be equipped.
5. Hero level cap 60, points = level (plan section 3).

## 2. Common framework (the first job: step 5.0)

The per-class work (5a, 5b, 5c) starts after this common framework is finished. The common framework is one PR.

### 2.1 The two engines

| Engine | Skills | Execution path | Source of values |
|---|---|---|---|
| **Legacy hero action** | Warrior W01–W06, Ranger A01–A06, Mage M01–M04·M06 (+ passives 01–06) | `TryClassSkillCore` → `StartHeroAction` (`Act:49-82`) → `ResolveSkill` (`Sim:291-353`) | `GameCatalog.cs`, `SkillEffects.cs`, **literals**. Changing `ClassSkills.json` does not change combat |
| **Native cast** | Warrior W08·W09·W17·W18, Ranger A07·A14·A17·A18, Mage M07·M08·M11·M17·M18, absorbed legacy skills | `ClassSkillCast` → `ResolveClassSkill` (`Fx:93-163`) → `ClassSkillEffect` tick (`Fx:167-271`) | `ClassSkills.json` (`catalog.json` + `runtime_parameters.json`) |

17 of the 30 new skills are legacy and 13 are native. The **rank curve of a legacy skill lives in the code** (`SE`, `Cost()`): values such as cost (e.g. W01 6→4, W03 25→21, M01·M03 25→21), duration (Blizzard 6→8), stun (Nova 1.5→1.9) and reuse time (M04) have no rank term today. These values are covered by the file that pins `GameCatalog.cs` and `SkillEffects.cs` by sha256 (`tools/class_skills.py`) and by the Balance1000 hash (`tools/balance_1000.py:222-231`): when you change them, **update the fingerprints in the same PR** and rerun `python3 tools/balance_1000.py`.

### 2.2 Source of values and ranks

- **Data-driven.** Per-rank values of skills, modifiers and passives are read from `SkillTreeD4.json` (`value: {from, to, unit}`). Do not put the formula (`×(1+0.1(r−1))`) in code. Today this formula happens to fit most values, but some values do not fit (section 4). `golden/rank_values.json` gives the golden value for each rank (linear; for count units `명 회 발 개`, `floor`).
- **Base skill damage comes from the base rank**, and **a modifier's values come from the modifier rank's** `from→to` (plan 5.4). A modifier value does not multiply the base-rank scale again. Native `SkillHit`, `Dot` and `Placement` multiply by `a.rankScale` (`Fx:12-13,40,84`), so pass modifier values through a **path that does not multiply** (a separate `source` or a bypass argument).
- **Ultimates (all three classes)**: `RankScale` is fixed at 1 for ultimates (`Cat:65`). Change it to `1 + 0.2(r−1)` (rank 3 = 1.4). The persistence validation cap (`ClassSkillPersistence.cs:37`, rankScale ≤ 1.401) accommodates this. The cooldowns 60/55/50 are set inside the **shared timer** of the two ultimates (floor 45s, `Sk:163`, validation 45–60 `Pers:26`).
- The gear +1 rank (`HeroStats.Blacksmith.cs:17`) is **not applied** to passives, ultimates or modifiers (plan D5).
- Legacy `rank` rule: a hit labelled `"M0x"` gets the base rank (`runeSkillPower` +10% per rank, `Sim:494`, `Dmg:66-67`), and `"EXTRA:M0x"` does not. Where the design requires the modifier rank (M04-1 and so on), the label must be changed.

### 2.3 Extension point: `ClassSkillCast`

Options (`ClassSkillOptions`, 55 groups · 166 choices) change only **when, to whom and where the hero stands**, and never **what** the skill does. A modifier cannot be carried in the options (there is no rank field, entries are capped at 55 and defaults are cleaned up, and the preset copy rules apply). Instead:

1. **Save**: add `modifier` (−1..2, ultimates −1..1) and `modifierRank` to `SkillInvestment` (v5). `JsonUtility` keeps the field's initial value for a missing key, so `modifier = -1` is the right default (0 means "modifier 1 selected"). In v5, `Rank(id)` returns **0 for a missing id** (it returns 1 today).
2. **Cast object**: add `mod` (starting from 1; 0 = none, so old saves stay valid) and `modRank` to `ClassSkillCast` (`ClassSkillState.cs:28-38`, saved), and fill them **once** in `CaptureClassCast` (`Ps:20-46`). Both engines pass through this function (native `Sk:161`, legacy `OriginalClassCast` at `Sk:223`). Validate next to `rankScale` (`Pers:37`).
3. **Reading side**: effect code finds the cast object with `Cast(root)` (`Sk:54`) and looks at `a.mod`/`a.modRank`. The **decision side** (planner, cost, forecast, aiming, auto-cast) has no cast object, so it reads `Loadout.Modifier(id)` (next to `Rank()`, `Sk:84`). An effect whose cast object is null (30-second cleanup, `Sk:218`) or that outlives its cast (`Ps:124`) must **copy the values when it is created** (`Placement` is the precedent, `Fx:84`).
4. **Value override**: `SkillTreeD4Values.Modifier(id, key, rank)`, which overlays the modifier value on `N(id,key)` (`Sk:28`).
5. **Test fixture**: many tests call non-public methods by reflection (positional arguments, `Single(name)`: `RT:17-22`, `PT:21`). **Do not create overloads**; append optional arguments at the end.

### 2.4 Effect ids and the cleanup rule (the most common trap)

- Effects, DoTs and shields of ids that are not equipped are **cleared every tick** (`ResetClassSkillEquipment`, `Sk:237-239`). The absorbed legacy skills (W07·W10–W16, A08–A13·A15·A16, M05·M09·M10·M12·M14·M16) are **never equipped** in the new system. A modifier effect must create a new id or **attach the base skill id as its `source`**. For the same reason, the "is equipped" checks in `InspectClassSkill`'s `NOT_EQUIPPED` (`Sk:122`) and `Reduce` (`Sk:72`), in the cancellation of a cast in progress (`HuntEdictChanges.cs:46`) and in the resume validation (`Pers:39-40`) must also be changed to judge a modifier effect as **parent + modifier**.
- The **secondary damage** that a modifier effect deals (forks, bounces, echoes, turrets) is `DamageKind.Set` with id `EXTRA:<original>`: no crit, not `Native`, not a direct hit (`Fx:12`, `Ps:101,121`). Keep the existing "it is not another skill" rule.
- There are many **hardcoded ids** (`"W07"`, `"M12"`, `"A11"`, `"M05"` …). When reusing an absorbed body, replace the literals with a modifier check. The locations are in the "Trap" column of each row.
- The gear and set hooks `DES_LW41–45` and `DES_LM41–46` hang off absorbed skills. Apply them **only when that modifier is chosen** (plan section 12).

### 2.5 Auto-battle and forecast

- Auto-cast: `TryAutomaticClassSkill` (`Cast:179-201`) walks through `Loadout.order`. The condition of a legacy skill is always true, and the real decision is in the edict planners (`PlanEdictWhirlwind/Leap/IronWall/Shout/Slam/…`, `Rules:159-262`). The condition of a native skill is `ClassAutoDefaultCondition` (`Au:97-147`).
- **The forecast and the release must match** (enforced by the `Identity:107-117` test). A modifier that changes the shape (W03-1 cone → circle, W04-2 circle → line, M01-1 Meteor, M03-2 reflection …) must **change the planner and the forecast too**.
- **Cautions from plan 8.5**: a modifier that stops to aim (A05-1), placed objects (the A03-3 turret, the 3 orbs of M11-2), a zone that follows the hero (M02-1) and a clone (A18-1) collide with auto-movement. Decide each item's auto-battle behaviour before implementing it and pin it with a test (the defaults in section 8).

### 2.6 Save and resume

- A modifier's `mod`/`modRank` live in `ClassSkillCast`, so **a cast in progress and its effects look the same after a resume** (`ClassSkillRuntimeState` saves them). Adapt the allow list of the persistence validation (`ClassSkillPersistence`) (`Pers:42,47-50`: the only intent ids are W02/M13, and the only persistent effects are the M13 channel and the W15 loan) to the new persistent effects (A03-3 turret, M11-2 orbs, W18-2 form buff and so on). An id that is not on the list throws an exception on resume, so the **saved run can no longer be read**.
- If a run is in progress during migration, handle it on the safe side (save research F1, the new decision in plan section 17).
- Skill id strings in statistics and logs (`statistics.skills`, `damageEvents`, `procHits` …) are not validated on load. A modifier effect's `definitionId` uses the base skill id, and the modifier number goes in a separate field.

### 2.7 Test fixture

- Basic fixture: `ClassSkillRuntimeTests.Fixture(skill, passive, equipment)` (`RT:23-41`). A level 40 hero (**change it to a level 60 hero**), equipped `[skill]`, one stationary enemy with 100000 HP (2m ahead), `Stats` overwritten with damage 100, HP 1000 and crit 0, and `decisionTime = 100000`, so **only manual casts** run, seed 42. Helpers: `Advance` (`RT:42`), `CastSkill` (`RT:44`), `Use` (`PT:24`), `AddSkill` (`RT:49`), `ExtraEnemy` (`GT:12`), `Begin`/`Land`/`Status` (`PT:12-23`).
- Legacy path: `T/ActionTests.cs:12-24`, `SkillRankEffectTests.cs:17-33` (`ContentTestAccounts.Legacy()`). The projectile and trap count tests and the forecast = release tests are here.
- Reading results: `State.damageEvents` (`definitionId`, `kind`, `critical`, `finalDamage`, `rootCastId`), `effectEvents`, `classSkills.{casts,effects,dots,cooldowns}`, `projectiles`, `traps`, `shields` and the `ClassSkillChanged` stream.
- **Determinism**: `Hit` always draws from `State.rng` (`Dmg:70`). A new roll shifts all later rolls, so compare with crit 0 and the same seed.
- Mage tests must know that `Stats.passives = new bool[6]` (RT:38) turns off MP05 and MP06 (turn them on with `sim.Stats.passives[4|5] = true`, or use the legacy Mage fixture `PassiveTests.cs:21-33`).
- **One test per modifier (paired test)**: run the same fixture with no modifier (`modifier = -1`) and with the modifier, and assert the difference.
```csharp
// helpers are real; r.modifier / r.modifierRank are the new v5 fields
int HitsOnSide(int modifier){
  var s=Fixture("W09");var r=s.State.build.classSkills.ranks.Single(x=>x.id=="W09");
  r.modifier=modifier;r.modifierRank=modifier<0?0:1;
  var side=ExtraEnemy(s,s.State.position+Vector2.right*2);Call(s,"Sense");
  Assert.IsTrue(CastSkill(s,"W09"));Advance(s,.6f);
  return s.State.damageEvents.Count(d=>d.definitionId=="W09"&&d.kind==DamageKind.Direct&&d.targetId==side.id);
}
[Test] public void SpinningRendReachesBesideTheCone(){Assert.AreEqual(0,HitsOnSide(-1));Assert.AreEqual(1,HitsOnSide(2));}
```
- Tests that will break (to be rewritten): `RT:16,54-66` (counts 37/16/19), `PT:11,36-96`, `Identity:88-150`, `SkillPresetCombatPreviewTests.cs:105-115`, `ClassSkillNativeSmoke.cs:47-66`, and the ability measurement tool `ClassSkillPassiveMeasurement` (old id names; its requirement of empty passive slots is forbidden in v3+).

### 2.8 Acceptance criteria for one modifier (common to every row)

1. **Values**: at rank 1 and at the maximum rank they equal the design's `from → to` (`golden/rank_values.json`).
2. **Behavioural difference**: the paired test shows the difference between no modifier and the modifier.
3. **Forecast**: for a skill that has a forecast, the forecast and the release match.
4. **Auto-battle**: auto-cast does not stall or spin idle.
5. **Resume**: saving and restoring a cast or an effect gives the same result.
6. **Cleanup rule**: the effect does not disappear when absorbed ids are cleaned up (2.4).
7. **Old behaviour kept**: with no modifier chosen, the base skill behaves as the design's default "no modifier" form (e.g. M07 is fire element with no extra effect, plan D8).

## 3. The 84 modifiers

**Categories**: **A** = only values change (keys and literals are turned into values). **A\*** = values only, but the value comes from the **modifier rank** (needs the accessor from 2.3). **B** = new behaviour on top of an existing code path. **C** = needs a new mechanism or state (a state field, a new effect kind, a new check). Abbreviations: `Sk` = `CombatSimulation.ClassSkills.cs`, `Fx` = `…ClassSkillEffects.cs`, `Au` = `…ClassSkillAuto.cs`, `Ps` = `…ClassSkillPassives.cs`, `Po` = `…ClassSkillPolicies.cs`, `Id` = `…SkillIdentity.cs`, `Sim` = `CombatSimulation.cs`, `Act` = `…Actions.cs`, `Dmg` = `…Damage.cs`, `Eff` = `…Effects.cs`, `Cs` = `…ClassSets.cs`, `Proj` = `…Projectiles.cs`, `Fc` = `…Forecast.cs`, `Aim` = `…EdictAim.cs`, `Cast` = `…ClassSkills.cs`. The other abbreviations and the exact locations are at the head of each file of the research notes. **The modifier number follows the design order (1–3), and the implementation's modifier number `modifier` is 0–2.**

### Warrior modifiers (28)

| ID | Name | Effect (rank 1 → max) | Category | Implementation point | Trap |
|---|---|---|---|---|---|
| `W09-1` | Blood Reclaim | HP per target 1.0% → 2.0% | **B** | On every 3rd cast, `CashDot`+`Heal` (reuse the W10 body). The cast count uses `Counter()` | The W10 hooks (Au:110, Po:39,89) become dead code |
| `W09-2` | Deep Wound | Bleed per stack +20% → +40% | **B** | Hit only one enemy and stack the bleed as `Dot(additiveBank,cap)` entries (up to 3 stacks) | Unclear whether +20% per stack is additive or multiplicative (question W-1) |
| `W09-3` | Spinning Rend | Bleed lasts 2s → 3s | **A** | `width` 90→360 (skip the angle check when `arc>=360`), `duration` | `Po:53` and `Au:85` read the same key. A shorter duration raises DPS (`bank/duration`) |
| `W08-1` | Lunging Hook | Extra resource +4 → +12 | **B** | Reuse the W11 movement skill (prepare+.25s, `Lerp`) to move to a stopping point in front of the enemy. Add W08 to the landing list | Walking is blocked while moving (Sim:364). Extra resource on arrival |
| `W08-2` | Piercing Hook | Pierces 3 → 7 targets | **B** | `SkillHit` several targets with `Line(origin,aim,6,width,3..7)`, `Pull` only the last one | Add a new width key |
| `W08-3` | Branding Hook | Damage from brand −10% → −18% | **B** | Reuse the W07 mark (`ApplyStatus(Mark,"W07",6)`). The kill resource uses a new `ClassHitFlags` bit | Literal "W07" (`HasAmplifyingMark` Sk:90, Ps:179, Au:51), DES_LW41 (Fx:103). A new mark erases the previous mark (Eff:106-107) |
| `W01-1` | Swift Whirl | Move speed ×2.0 → ×3.0 | **B** | Apply the move speed ×2~3 **after** the `SpeedWithBonus` cap (1.5x) (EdictWhirlwindMovement.cs:40, Sim:437) | Outrunning the enemies can trip the 0.5s empty-tick cutoff |
| `W01-2` | Wide Whirl | Hit area ×1.6 → ×2.0 | **A** | Radius multiplier inside `RuneSkillRadius` (every call passes through this function) | The walking logic stays pinned at 2.3~2.5m (EdictWhirlwindMovement.cs:29) |
| `W01-3` | Rapid Whirl | Tick interval ×0.7 → ×0.5 | **A** | Turn the tick interval literals (Act:80, Sim:458) into values | Cost, pull and crit roll on every tick: at interval ×0.5 the cost per second is 2x. Adjust the cost together (question W-3) |
| `W03-1` | Earth Shatter | Blast radius 2.0m → 3.0m | **B** | Cone → circle: arc 100→360, radius 2→3 (Sim:313, Forecast.cs:98, EdictAim.cs:20). Use `Cast.area` | The design calls the base a 'single strike' (SD4:581), but the implementation is a 100° multi-hit (question W-4) |
| `W03-2` | Execution Strike | Execute below 20% → 30% HP | **B** | Coefficient ×2 at HP ≤ threshold (same check as WP06, Dmg:31) | Refund amount undecided |
| `W03-3` | Aftershock | Aftershocks D40% → D80% ×2 | **B** | `ScheduleExtra` 'burst' ×2, +1s (Cs:111-114, Fx:233) | Being `Kind.Set`, it gets no WP15, WP17 or legendary credit |
| `W05-1` | Iron Stance | Counter on block D80% → D120% | **B** | Use the W12 counter body (`AfterClassHurt` Ps:164-165, 2 times at 1s intervals) under the `OwnIronWall` condition | W12's +15% block (Ps:192) is not in the design |
| `W05-2` | Guardian Oath | CC immunity 1.0s → 2.0s | **B** | W16: `ClearHeroControl` + `Buff("W16:immune")` (Fx:120-126) | Retarget the W16 special handling (Sk:190, EdictOrder.cs:132, Au:116) to the new target |
| `W05-3` | Thorn Armor | Reflect absorbed 30% → 50% | **B** | In `ReactShield` (Id:77-90), deal absorbed amount × ratio to `Nearby(..,3)` as secondary damage | Separate from the thorns attribute (Attributes.cs:115). The `IronWallAmount` pre-check has the literal .3 (EdictWarriorSupport.cs:10) |
| `W06-1` | Rallying Cry | Move speed +20% → +40% | **A** | Add `shoutTime>0` to `ClassMoveBonus` (Ps:168-174) | Total +50% cap (Economy.cs:45). 'Crowd control reduction' appears only in the design's behaviourKo |
| `W06-2` | Frenzy Cry | Attack speed +20% → +40% | **B** | Apply `ClassAttackSpeedBonus` (Ps:193), skip the resource refill (Sim:325, EdictWarriorSupport.cs:85) | Attack speed cap 1.5 (Act:57) |
| `W06-3` | Debt of Battle | Extra resource +40 → +60 | **B** | W15 loan (`BorrowResource/RepayResource` Id:22-38) | The design says 'resource regeneration stops for 6 seconds', but the implementation is borrow and repay (question W-8). The validator is tied to "W15" (ClassSkillPersistence.cs:47-50) |
| `W02-1` | Breach Charge | Charge distance 3m → 5m | **C** | Push along the path: `Line`+`BreakFront` (Id:11-21, W11 Fx:109-112) + a push width key | Whether to keep the landing damage; turning off `HeroAirHeight` (Act:28); the `EdictLeapLandingValid` minimum of 2m; the meaning of the 'space' policy (Po:177) |
| `W02-2` | Quake Landing | Landing quake D60% → D100% | **B** | Add a +2s 'burst' and a stun (`ApplyStatus`) to the landing (`ClassLanding` Ps:57-69). Add a stun branch to 'burst' |  |
| `W02-3` | Double Leap | Recharge 8s → 6s | **C** | There is no charge concept. Many places read `State.cooldowns[]` (EdictWarrior.cs:79, Dmg:156-157, Legendaries.cs:111,128, ClassSkillReadModel.cs:24-25) | Recharge 8→6s vs cooldown 6.4s (question W-5) |
| `W04-1` | Battle Ring | Pull radius 3m → 5m | **B** | `Pull` (Fx:17-36) before `AreaHit` | A 5m radius needs a pull toward the hero (W13 uses 1.5m) |
| `W04-2` | Tremor Wave | Wave length 5m → 9m | **B** | Circle → line: `Line()` (Sk:95-100) + slow (W14 Fx:118) | Update Forecast.cs:87-98, EdictSlam.cs:35, EdictWarriorSupport.cs:67, DES_LW44 (Ps:44, Sk:250) |
| `W04-3` | Repeated Stomp | Per stomp 40% → 60% ×3 | **B** | A stomp on cast + a 'burst' twice (at 0.6/1.2s), with a stun on the last one. Anchored to `a.origin` | Being `Kind.Set`, it gets no WP17 |
| `W17-1` | Ancestral Legion | 3 ancestors · 6s → 8s | **A** | `summons`, `duration` and `ticks` keys |  |
| `W17-2` | Ancestral Technique | Copied skill 50% → 70% | **C** | A last-skill record field (`ClassSkillRuntimeState`) and replay | The copy must not pass through `CaptureClassCast` (WP15 history Ps:25-26) and uses an `EXTRA:` id |
| `W18-1` | Judgment of the Earth | Area 6m → 8m | **A** | An accessor that reads `prepare` (before the cast object exists, Sk:159) | The hero is held for 1.8s (Act:26) |
| `W18-2` | Giant Form | Giant form 6s → 8s | **C** | A form buff: area +50% (`Cast.area`, ignored by W01/W02) and knockback immunity (`PushHero`) | Add the persistent effect to the allow list at ClassSkillPersistence.cs:47-48 |

### Ranger modifiers (28)

| ID | Name | Effect (rank 1 → max) | Category | Implementation point | Trap |
|---|---|---|---|---|---|
| `A07-1` | Rapid Volley | 5 shots · D25% → D33% each | **A*** | `arrows`/`coef` (Fx:95,133). Damage basis E1 |  |
| `A07-2` | Scatter Shot | Targets 3 → 5 | **B** | A different target for each arrow with `Nearby(a.origin,range,n)` (repeat targets when enemies are few) | The release checks only the main target (Cast:170). Each hit connects to A18 (Idn:64) |
| `A07-3` | Finishing Arrow | 3rd arrow crit +30% → +70% | **B** | Pierce already exists (`Arrow(...,pierce)`). Forced crit and the crit damage argument are new plumbing (Dmg:70,73, `SkillHit` Fx:9-13) | Uses the same plumbing as A04-3 and Precision |
| `A14-1` | Chain Split | Second split 30% → 50% | **B** | If it is a `secondary` arrow, one more generation. A per-cast visited set (`Once`) |  |
| `A14-2` | Explosive Arrow | Blast radius 1.5m → 2.5m | **B** | `Splash(a,pos,radius,power,0,secondary:true,source)` | Needs its own source (Dmg:56 labels 'EXTRA:A14' as Split) |
| `A14-3` | Returning Arrow | Return arrows D30% → D50% | **C** | Arrows have no return leg. Copy the M10 boomerang (Idn:91-117: `count` leg, `targets.Clear()`, re-aim) |  |
| `A01-1` | Ricochet | Bounces 1 → 3 | **C** | There is no bounce code or wall normal (RiftLayout.cs:32-43). Normal probing, storing the bounce count (`CombatProjectile`), the `hitIds` rule | The forecast (Forecast) cannot predict it |
| `A01-2` | Accelerating Pierce | +10% → +20% per pierce | **B** | Reuse the LA01 growth (Proj:85: extra = k×`hitIds.Count`) and the tie handling (Aim:74) | Both are tied to `Stats.specials` |
| `A01-3` | Triple Pierce | Arrows 50% → 70% ×3 | **B** | 3 arrows at ±30° in the `LaunchMulti` style + consume 1 shadow charge | Update Fc:52-56, Aim:44-77. Interaction with the SAB4 echo |
| `A02-1` | Barrage | 7 arrows · D50% → D70% | **B** | 7 arrows · 120° | `{-angle,0,angle}` is hardcoded in Fc:60, Aim:22 and FOCUS (Aim:66-77) |
| `A02-2` | Bouncing Arrows | Bounce damage 40% → 80% | **B** | On hit, send the SAB4 `extra` pattern (Set, no effects) to the nearest enemy not yet hit + a 'bounce' flag |  |
| `A02-3` | Focused Spread | Same-target arrows +10% → +30% | **B** | The LA02 `narrow` path already does the same thing (Proj:30). Change the `Stats.specials.Contains("LA02")` checks to modifier checks | The same-target bonus uses `victim.hits` (Proj:84) |
| `A04-1` | Decoy Projection | Decoy taunt 2.0s → 3.0s | **B** | Create the A11 effect (Fx:136, enemy AI, expiry blast) at `a.origin` from the LA03 hook. The taunt of 2→3s is `tauntSeconds` | AP14 reads the literal `Fx("A11")` (Pas:110) |
| `A04-2` | Smoke Cover | Damage taken −12% → −20% | **B** | `Buff("A12",3,guard)` in the `ClassLanding` A04 block | A12's +20% speed (Pas:170) is not in the design |
| `A04-3` | Shadow Step | Next hit crit +20% → +60% | **C** | There is no landing behind the target (`EdictEscapePoint` is retreat-only, and `RangerLandingValid` rejects spots near enemies). The next-hit crit buff shares the A07-3 plumbing | `RememberRetreatLanding` makes the hero walk |
| `A03-1` | Spike Trap | Root 1.0s → 2.0s | **B** | The legacy trap already roots on trigger. One blast + root 1→2s, skip the poison repeats | No A09 port is needed |
| `A03-2` | Frost Snare | Slow 30% → 50% | **B** | Element 4→2, `Poisoned`→`Slow` 30~50% (Proj:144-145) | The `OwnTrapPoison` and AP10/AP15/Viper poison sources disappear. A10 is a one-shot blast, but the design says zone (question R-3) |
| `A03-3` | Crossbow Turret | Turret shots 6 → 10 | **B** | Reuse `Placement(a,"ballista")`+`FireBallista` (Fx:141-143, Idn:44-52). The placement is C | It reads the A15 keys, so an indirection is needed. The turret is a `ClassSkillEffect` (cap 1), so the `LiveRangerTraps` users ignore it. Auto-placement uses the trap-cluster logic (RgrT), not A15's firing-point search |
| `A06-1` | Shadow Imbuement | Kill blast D40% → D80% | **B** | Blast on kill: `EliteCascade` (Dmg:121-129) is the precedent |  |
| `A06-2` | Poison Imbuement | 4s poison D60% → D100% | **B** | A13's `Dot` (Fx:37-55) needs a `ClassSkillCast` (legacy has `Cast(root)`). A new dot id | An unequipped id is cleaned up every tick (Cast:239) |
| `A06-3` | Cold Imbuement | Slow 20% → 40% | **C** | A per-enemy stack counter + `Freeze` | Bosses only fill a control gauge (Effects.cs:100-104) |
| `A05-1` | Gaze Shot | Aimed snipe D200% → D280% | **C** | Needs an aiming step (auto-movement stops while aiming, cancel on interruption: plan section 8). A08 is a class-cast and 'stationary' is hardcoded as `CS.action.id=="A08"` (Cast:26, 207, Sim:363, Po:27-31,200) |  |
| `A05-2` | Hunter's Preparation | Next skill cost −10% → −30% | **B** | Reuse the A16 effect, triggered by killing a marked target: capture the mark state before damage (Dmg:40-52 pattern) and fire at `enemy.dead` |  |
| `A05-3` | Chain Mark | Mark jumps 1 → 3 | **C** | Needs a death hook + a jump counter: `StatusEffect` has none and `Counter()` has no expiry | `ApplyStatus` already moves the mark |
| `A17-1` | Moving Barrage | Waves 6 → 8 | **B** | `f.position=State.position` before `Nearby`. 8 waves = `ticks` 8 · duration 4s | The loop is bounded by `until` (Fx:203) |
| `A17-2` | Focused Barrage | Narrow area ×1.6 → ×2.0 | **A** | `radius`→`f.radius` (literal 4), coefficient multiplier, period .25 | With `ticks` 6 it ends in 1.5s |
| `A18-1` | Shadow Clone | Clone damage 40% → 60% | **C** | There is no follower effect. Create `kind="clone"` and mimic direct hits in `IdentityDirectHit` | A `Set` secondary hit cannot retrigger it. Fix the id list at Idn:64 |
| `A18-2` | Afterimage Burst | Burst D60% → D100% | **B** | Expiry blast (like the orbs and the decoy) | Vanishing (Idn:68), removal over the cap (Idn:57) and the definition of 'fading' |

### Mage modifiers (28)

| ID | Name | Effect (rank 1 → max) | Category | Implementation point | Trap |
|---|---|---|---|---|---|
| `M07-1` | Ember Lance | Burn D60% → D100% | **A*** | The current body. `Dot` multiplies by `a.rankScale` (Fx:40): pass the modifier value (D60→100) directly |  |
| `M07-2` | Glacier Lance | Freeze 0.6s → 1.0s | **B** | The M09 body (Fx:149-150: slow .35 for 3s, freeze an already slowed enemy). Element 2, freeze 0.6→1.0s |  |
| `M07-3` | Grounding Lance | Lance stays 4s → 6s | **B** | The M12 body (Fx:153) + conductor | The relay is tied to the literal "M12" (Id:136,142-144, Ps:148-152, Au:59-65,136, Po:65). The lance sticks into `a.aim` |
| `M01-1` | Meteor | Damage ×1.4 → ×1.8 | **B** | Impact at the aim point after 1s: `delay` (Proj:56-58) + `remaining=0` or `ScheduleExtra`. Radius ×1.5 | Design conflict: ×1.4→1.8 vs the behaviour 'area ×1.5, damage ×1.6' (question M-1) |
| `M01-2` | Splitting Fireball | Splits D40% → D60% | **B** | 3 small shots at the impact point: `Shot()`. `extra` is hard-labelled 'SAB4'/`Set` (Proj:86) → 'EXTRA:M01' |  |
| `M01-3` | Piercing Fireball | Burn trail 2s → 4s | **C** | The pierce is B, but the flame trail has no host: ground kind 12 is one-shot and `InFirewall` is tied to a single rectangle → a trail-segment effect is needed | The forecast is a line (`ForecastLine`) instead of a circle |
| `M02-1` | Trailing Blizzard | Radius 2.5m → 3.5m | **B** | A `fx.position=State.position` branch (the existing `followsTarget` tracks the target) | Radius 2.5→3.5 vs the planner literal 3. KEEP/KEEP_TARGET assume a fixed zone (EdB:53-55,67) |
| `M02-2` | Deep Freeze Storm | Slow 50% → 60% | **C** | The slow is A* (`SlowStrength` literal .35), but 'freeze if they linger' needs a per-enemy dwell timer (only `enemy.exposure` exists) |  |
| `M02-3` | Returning Frost | Round trip D70% → D110% | **B** | Call the M10 body (Fx:151, `TickReturningGlobe` Id:91-117) from the legacy Blizzard case. Rename the M10 keys | `PlanEdictBlizzard` plans clusters, not a line |
| `M03-1` | Overload Chain | Chains 6 → 8 | **A*** | Hop limit literal 4 → modifier 6~8 (absolute value). Decay literal .8 | Uses the same function as the forecast (Fc:10) |
| `M03-2` | Rebound | Rebounds 2 → 4 | **B** | Re-hit a lone enemy 2→4 times: generalise the LM04 revisit | `PlanEdictChain` requires 2 unique targets (EdC:28-30). The forecast `hits` must repeat the id |
| `M03-3` | Magnetic Chain | Pull 1.0m → 2.0m | **B** | After the loop, `Pull()` the linked enemies toward the first target by 1→2m | A pull inside the loop changes `NextChainTarget` and falls out of step with the forecast. The M16 vortex DoT is not in the design |
| `M06-1` | Double Nova | Second wave 50% → 100% | **B** | +1s 'burst'/`ScheduleExtra`, `Set` damage 50→100% | Whether the second wave freezes is undecided |
| `M06-2` | Rift Ward | Damage taken −10% → −20% | **B** | Skip the freeze loop and use `Placement(..,"ward")` with the M14 values. Turn the `ClassGuard` literal .1 into the modifier value (−10→−20%) | `Fx("M14")` does not look at the kind. The CONTROL purpose still believes 'Nova freezes' (EdN:35-41) |
| `M06-3` | Ice Armor | Shield per enemy 2% → 4% | **B** | `AreaHit` returns the hit count → `AddShield("M06",hp*min(n*pct,.2),dur,root)` | Duration undecided. The 20% cap is only in behaviourKo |
| `M04-1` | Flame Afterburn | Departure blast D80% → D120% | **B** | `AreaHit(origin,r,coef,1)` before `State.position=destination` | The "M04" label takes the base rank (Dmg:66). The design uses the modifier rank |
| `M04-2` | Lightning Blink | Arrival stun 0.3s → 0.7s | **B** | After landing, `Nearby(dest,r)`+`ApplyStatus(Stun)` controls every enemy in the area | `EdictEscapePoint` picks quiet spots, so there are almost no enemies. The damage value is not in the design |
| `M04-3` | Elemental Shield | Shield HP 25% → 35% | **B** | `AddShield` on arrival (25→35%, 4s). Move every M05 literal (reaction Id:72-90, LM03 mana, REF_SM01 +30%, `OwnElementalShield`, `BuffSkill`, survival `defenseSkill`) | Keep the `lastElement` record (Id:74) (M17-1 and M18-2 read it) |
| `M08-1` | Ring of Fire | Ring radius 2.5m → 3.5m | **B** | `InFirewall` → a circle around the hero (2.5→3.5), position `a.origin`. Also `ClassAimContains` |  |
| `M08-2` | Advancing Flames | Advance 1.5 → 2.5 m/s | **B** | Move `f.position` along `f.direction` (1.5→2.5 m/s), clipped with `Map.MoveDirect` |  |
| `M08-3` | Cross Wall | Duration 2s → 3s | **B** | Also test the perpendicular rectangle. Duration 2→3s (shorter than the base 4→5) | `Placement` allows one effect per id+kind (Fx:82) |
| `M11-1` | Rolling Orb | Path damage D40% → D60% | **B** | Move `f.position` on every tick. The 'path damage' D40→60 is a new coefficient |  |
| `M11-2` | Triple Orb | Orbs last 2s → 3s | **C** | Breaks the one-effect-per-id+kind assumption. Needs instance ids, a cap and an expiry rule | Every caller of `Fx("M11")` assumes a single orb (Ps:54, Id:144, Ps:151, Au:135). Which orb gets the M03 charge? |
| `M11-3` | Overcharge | Expiry blast D200% → D300% | **A*** | `period` 0 (ticking stops) + the expiry coefficient from the modifier value 200→300 |  |
| `M17-1` | Single Element Focus | Damage +20% → +30% | **B** | Every step uses `CS.lastElement`, +20→30% | A substitute is needed when 0 = none. The freeze identity of the cold step disappears |
| `M17-2` | Tracking Collapse | Retarget 6m → 10m | **B** | On every step, set `f.position` to the densest point within 6→10m (reuse Au:76-84) |  |
| `M18-1` | Elemental Surge | Extra casts 40% → 60% | **C** | Cast another equipped Core skill for free at 40→60%. Needs a synthetic `HeroActionState`, must not pass through `CaptureClassCast` (MP11/MP16/cycle/legendaries count it), block recursion | If approximated by refining the current 3-element blast (Ps:155, 40%), it is B |
| `M18-2` | Elemental Avatar | Element effect 20% → 30% | **B** | Branch on `CS.lastElement`: fire damage, cold protection (`ClassGuard`), lightning cooldown (cdr on cast, or the reduction in 3 places) |  |


### 3.1 Absorbed legacy skills and their modifier pairs

22 skills are absorbed: 20 of them have `absorbsLegacyId` in the data, and M09 and M12 are merged into modifiers 2 and 3 of Elemental Lance (M07) (the "merged" entry in `legacyRemap`). Reuse the bodies, but switch them on by the "chosen modifier", not by "is equipped" (2.4).

| Class | Legacy id → modifier |
|---|---|
| Warrior | W07→W08-3, W10→W09-1, W11→W02-1, W12→W05-1, W13→W04-1, W14→W04-2, W15→W06-3, W16→W05-2 |
| Ranger | A08→A05-1, A09→A03-1, A10→A03-2, A11→A04-1, A12→A04-2, A13→A06-2, A15→A03-3, A16→A05-2 |
| Mage | M05→M04-3, M09→M07-2, M10→M02-3, M12→M07-3, M14→M06-2, M16→M03-3 |
| Removed (no modifier) | Mage M13 (Mana Reclaim: it needs the hero to stand still, which collides with auto-movement), M15 |

## 4. 24 passives and 6 key passives

Passives unlock by hero level only (no separate condition in combat). **Per-rank values are the data's `from → to`** (read by `SE.Passive` and `PV`). In the table below, "Value" is the design's value and "Fits" says whether the current formula `×(1+0.1(r−1))` produces that value. × means **a data-driven `from→to` is needed**.

| ID | Name | Unlock | Value (rank 1→5) | Current application point | Fits | Note |
|---|---|---|---|---|---|---|
| WP13 | Ragged Breathing | 2 | 40→56% | `Ps:167` HP < 70% | ○ | Design "70% or below" vs implementation `<` (question W-13) |
| WP01 | Into the Crowd | 4 | 25→35% | `Dmg:30-31` (3 or more, fixed at cast time) | ○ | Legacy `SE.PassiveBase[0]` |
| WP06 | Executioner's Eye | 6 | 35→49% | `Dmg:31` (per target, HP 30% or below) | ○ | |
| WP05 | Hardened Will | 8 | 40→56% | `Economy.cs:191` `shieldMultiplier` | ○ | |
| WP04 | Blood Recovery | 15 | 2→2.8% | `Passives.cs:28-35` (every 20 ticks) | ○ | |
| WP09 | Counter Rhythm | 20 | 18→26 | `Ps:163` Proc (1s per root) | **×** | 18×1.4 = 25.2 |
| WP17 | Battle Reserve | 25 | 15→21% | `Ps:22,27,103-105` | ○ | The ratio is captured before the cost is paid |
| WP15 | Weapon Sequence | 30 | 20→28% | `Ps:25-26` (4s window, `Offensive` only) | ○ | |
| **WP20** | Blood Frenzy (key) | 45 | 40→80 D% | None | **×** | Sketch below |
| **WP21** | Unending Resolve (key) | 45 | 6→10% | None | **×** | Sketch below. Needs a skill `role` field |
| AP01 | Long Reach | 2 | 30→42% | `Dmg:32` (6m or more, additive) | ○ | |
| AP09 | Drawing Again | 4 | 4→6 | `Ps:138` (BASIC hit, 0.5s Proc) | **×** | 4×1.4 = 5.6 |
| AP06 | Finishing Shot | 6 | 20→28%p | `Dmg:69-70` (crit at HP 30% or below, cap .75) | ○ | |
| AP07 | Marked Opening | 12 | 15→21%p | `Ps:93` `ClassCritical` | ○ | |
| AP15 | Venom and Frost | 15 | 25→35% | `Ps:111` | ○ | |
| AP10 | Venom Cycle | 20 | 12→20 | `Ps:135` (direct kill of a poisoned enemy, 2s) | **×** | 12×1.4 = 16.8 |
| AP05 | Saved Focus | 25 | −40→−56% | `Chg:18-45` + `Sim:285` (**legacy casts only**) | ○ | `ClassCost` (`Ps:77-81`) ignores it, so A14 and others get no discount |
| AP14 | Decoy Tactics | 30 | 20→28% | `Ps:110` literal `Fx("A11")` (within 3m) | ○ | Must read the A04-1 decoy |
| **AP20** | Precision (key) | 45 | 30→50% | None | **×** | Sketch. The code has no Core skill classification |
| **AP21** | Viper's Blessing (key) | 45 | 10→18% | None | **×** | Sketch |
| MP05 | Mana Circulation | 2 | 20→28% | `Eco:182` (regen ×) | ○ | The Blacksmith +1 rank is not applied (D5) |
| MP14 | Mana Thrift | 4 | −30→−42%p | `Ps:139-143` (2nd basic attack) | ○ | Total cost reduction cap 50% |
| MP17 | Overflowing Mana | 6 | 20→28% | `Ps:112` | ○ | Only the first direct hit of a cast |
| MP06 | Element Crossover | 8 | 10→14% | `Passives.cs:36-45`, `Sim:494` | ○ | |
| MP07 | Ignition Feedback | 18 | 12→20 | `Ps:129` | **×** | 12×1.4 = 16.8. "Burning" is only `HasDot("M07")‖InFirewall` (add the M01-3 trail and the M04-1 blast) |
| MP09 | Opening in the Cold | 18 | 12→20 | `Ps:130` | **×** | `Controlled()` also counts a slow (`Atr:27-29`) |
| MP11 | Overload | 18 | 30→42% | `Ps:29-30,38-39` | ○ | Counts any lightning cast, consumed by M03 and M12 (design: M03 only) |
| MP16 | Threefold Memory | 30 | 8→12% | `Ps:40` | **×** | 8×1.4 = 11.2. It is any order within 6s, not "in a row" |
| **MP20** | Shatter (key) | 45 | 30→50% | None | **×** | Sketch. A per-enemy accumulated damage field |
| **MP21** | Align the Elements (key) | 45 | 60→100 D% | None | **×** | Sketch. A per-enemy element bit mask |

Common work: the branches of Mage passive indices 0–3 (the old MP01–MP04) become dead code, but keep their slots in `SE.PassiveBase` (`SE:14`). Key passives are not in `ClassSkills.json` (`ClassSkills.Data` hard-checks for 111 entries and unlock ≤ 40: `Cat:52-56`, `Cat:122`): read them only from the new data file and skip the old checks for heroes in the new system.

**Key passive sketches** (taken from the research notes. Line numbers use the same basis):

```csharp
// WP20 Blood Frenzy (Warrior): bleed ticks can crit; a bleeding target that dies explodes.
// bit 16 = HasDot("W09",enemy), captured BEFORE damage in ClassHitFlags (Ps:94-98); Deal sets enemy.dead (Sim:508)
if(Passive("WP20")&&(flags&16)!=0&&enemy.dead&&Once("WP20",root,enemy.id))
  Splash(a,enemy.position,r,N("WP20","value"),0,secondary:true,source:"WP20");   // raw value: SkillHit would multiply rankScale (Fx:12)
// bleed crit: TickClassDots calls ApplyOutgoing(..,critMultiplier 1,critical false) (Fx:73): roll RandomStream.Unit(ref State.rng) < Min(.75f, d.snapshot.crit)
```
```csharp
// WP21 Unending Resolve (Warrior): alternating skill roles within 4 s stack up to 3. Roles come from the spec's roleKo (needs a `role` field).
var c=Counter("WP21"); bool alt=c.until>State.time&&c.previous!=role;
c.count=alt?Mathf.Min(3,c.count+1):(c.until>State.time?c.count:0); c.previous=role; c.until=State.time+4; a.additive+=c.count*PV("WP21");
```
```csharp
// AP20 Precision (Ranger): every 3rd crit makes the next core-skill cast a forced crit (+value). Same shape as MP11 (Ps:29-30,38-39).
// AfterClassHit, inside if(direct) (Ps:133):
if(damage.critical&&Passive("AP20")){var c=Counter("AP20");if(++c.count>=3){c.count=0;Buff("AP20:charge",30,PV("AP20"),root,"charge");}}
// CaptureClassCast, Normal(a.id) block (Ps:23-30):
if(IsCore(a.id)&&Fx("AP20:charge") is ClassSkillEffect p&&!p.consumed){p.consumed=true;a.forceCrit=true;a.additive+=PV("AP20");}
// Hit (Dmg:70): draw the RNG first, then critical = canCrit && (roll<cap || Cast(root)?.forceCrit==true)
```
```csharp
// AP21 Viper's Blessing (Ranger): poisoned targets take more damage, including DoT ticks. ApplyOutgoing is the one choke point.
// beside DataDamageTaken (Dmg:44); the poison flag is captured before the hit (Dmg:41, Ps:97):
if((classFlags&2)!=0&&Passive("AP21"))independent*=1+PV("AP21");
```
```csharp
// MP20 Shatter (Mage): new EnemyState.shatter (float; old saves read 0). In ApplyOutgoing after Deal (Dmg:49):
if(Passive("MP20")&&CombatEffects.Has(enemy,StatusKind.Freeze)&&!definition.StartsWith("EXTRA:MP2"))enemy.shatter+=damage.hpLoss;
// TickStatuses, after the remaining-=dt loop, before RemoveAll (Eff:163-164):
if(enemy.shatter>0&&!CombatEffects.Has(enemy,StatusKind.Freeze)){float taken=enemy.shatter;enemy.shatter=0;
  if(Passive("MP20")&&!enemy.dead)foreach(var e in Nearby(enemy.position,2.5f,5))
    ApplyOutgoing(e,taken*Value("MP20"),0,1,1,false,2,CaptureDamage(),"EXTRA:MP20",0,0,DamageKind.Set,attackResolved:true);}
// bosses never get a Freeze status (Eff:98-103): use bossControl.staggered (Eff:155-158) if bosses should shatter. Clear in ResolveCombatDeaths and ResetUnavailablePassives.
```
```csharp
// MP21 Align the Elements (Mage): new EnemyState.elements (bit mask: 1 fire, 2 cold, 4 lightning). In ApplyOutgoing beside RecordElementHit (Dmg:50):
if(Passive("MP21")&&element>=1&&element<=3&&!enemy.dead){enemy.elements|=1<<(element-1);
  if(enemy.elements==7){enemy.elements=0;foreach(var e in Nearby(enemy.position,2.5f,5))
    ApplyOutgoing(e,snapshot.damage*Value("MP21"),0,1,1,false,0,snapshot,"EXTRA:MP21",root,0,DamageKind.Set,attackResolved:true);}}
// Value() is the from/to accessor, not PV (PV multiplies rankScale). EXTRA: ids are non-native: no recursion, no element recording.
```

## 5. The 6 ultimates (rank 3)

| ID | Name | Now | In the new system | Note |
|---|---|---|---|---|
| W17 | War of the Ancestors | 2 ancestors, secondary damage to the nearest enemy every 1s, 8s | Rank +20%, cooldown 60/55/50 | W17-1 A (ancestor count, duration, ticks), W17-2 C |
| W18 | Titan's Judgment | 4m radius strike D600%, stun 2s, `prepare` .9 | Rank +20%, cooldown | W18-1 A, W18-2 C |
| A17 | Killing Rain | Fixed 4m radius, 6 pulses D80%, every enemy in the area | Rank +20%, cooldown | A17-1 B, A17-2 A |
| A18 | Afterimage March | Afterimage markers; a direct hit consumes the oldest afterimage to fire a D60% secondary arrow | Rank +20%, cooldown | A18-1 C, A18-2 B |
| M17 | Triune Collapse | 3 steps (fire, cold, lightning), radius 4m | D200%→280% (×1.4) | M17-1 B, M17-2 B |
| M18 | Sage Incarnate | 8s, shield 25%, damage +20%, cost −20%, 3-element blast D40% | Shield 25→35%, damage 20→30 | The shield does not receive the rank scale in `Fx:161`. Damage 20→30: ×1.4 gives 28, so use from/to. The design's base text has neither the cost −20% nor the 3-element blast (question M-9) |

- The two ultimates use a **shared timer** (floor 45s). Even when the ranks reduce the cooldown 60→55→50, it never goes below 45.
- **A hero at level 40 or below cannot learn an ultimate.** The T5 gate is "40 points outside T5" and points = level, so the first ultimate point needs level 41 (save research, section 0 item 7). The tutorial (H08) and the "Lv.40" text rely on this assumption (the new decision D17 in plan section 17).

## 6. Cleanup

- **What becomes dead code**: the branches of the 33 passives that disappear (`Ps:28,52-53,71,171`, `Fx:146,226,259`, `Proj:24,133`, `Passives.cs:25`, `Fc:55`, `RgrT:43`, and for the Mage `Proj:107`, `Sim:282`, `Fc:10`, `Eco:191`). They may stay in the code, but they must not be evaluated for heroes in the new system.
- **Hardcoded id lists**: in step 6, fix every place that points at a legacy id (`legacy_id_inventory.py --check` fails until the references of groups 1~5 reach 0).
- **Validation builds**: 29 of the 30 `ClassSkillValidationBuild` entries in `ClassSkills.json` use legacy ids (all 10 Warrior ones, all 10 Ranger ones and 9 Mage ones). Under the new rules, create them only through an explicit **exception path** (a kit) that bypasses the point and tier rules.
- **`ResetUnavailablePassives`**: clean up the state of passives that have not been learned (per-enemy fields, counters).

## 7. Work order

Split step 5 of plan section 14 into four. Prerequisite: **step 1 (data and rules) and step 2 (save v5, level cap 60)** are done. New tests run in a fixture with `SkillTreeD4.Enabled` turned on.

| Step | Content | Done when |
|---|---|---|
| **5.0** Common framework | All of section 2: the v5 fields, `Loadout.Modifier`, `ClassSkillCast.mod/modRank`, `SkillTreeD4Values`, rank value reading (data-driven), ultimate `RankScale` and cooldown, the cleanup rule (2.4), the persistence allow list, fingerprint and Balance1000 updates, the level 60 fixture | The paired-test fixture runs with one modifier (a category A one such as W09-3). The existing combat tests give the same results as `main` |
| **5a** Warrior | Base skill values → category A → category B → category C → 8 passives → 2 key passives → ultimates. Grouped in tier order (T1, T2, T3, T4+ultimates) | Tests (2.8) for the 28 modifiers + 10 passives + the two ultimates |
| **5b** Ranger | The same order. `A06` (Elemental Imbuement) is built on the old Shadow Arrow code, and the literal `"A06"` references (`Sim:280`, `Dmg:57`, `EdictOrder.cs:31-33`, `HuntEdictV2.Options.cs:91-94`, `EdictValidation.cs`, `Itemization.cs:215`, display code and others, a dozen or so places) take the new meaning | Same |
| **5c** Mage | The same order. `M07` (Elemental Lance) is built on the old M07 body, and the modifier decides the element | Same |

Order inside each class PR: ① base skill rank values (data-driven), ② modifiers that only change values (A, A\*), ③ behaviour on an existing path (B), ④ new mechanisms (C: write the design in this document first, one paragraph each, then implement), ⑤ passives (the applicable ones first), ⑥ key passives, ⑦ ultimates. **Implement category C only after the answer to its question (section 8) is decided.**

**Do not**: re-write the rules (points, tiers, modifier rank) in the combat code. Do not carry modifiers in the option system. Do not quietly carry over the current bonus effects that are not in the design (W11 guard and resource, W12 +15% block, A12 +20% speed and so on) (question C-5).

## 8. Questions and defaults that cannot be settled from code alone

You can proceed on the defaults. If the user decides differently, change the default before that class's PR. **Do not implement a category C modifier before its question is answered.**

**Common**

| # | Question | Default |
|---|---|---|
| C-1 | Does a modifier value multiply the base-rank scale | No. Base damage uses the base rank, and modifier values use the modifier rank's from→to (plan 5.4) |
| C-2 | The basis of the `%` and `D%` units | `D%` = a coefficient based on weapon damage, `%` = the quantity that the word before it on that line points to. Decide ambiguous lines (A14-1, A02-2, A01-3, A07-3) one by one in the PR and write them in the table |
| C-3 | Costs, cooldowns and durations not in the design | Keep the current ones |
| C-4 | Does a forced crit (A07-3, AP20, A04-3) count toward the crit counter | Yes, it counts |
| C-5 | Current bonus effects that are not in the design | Remove them (the design is the basis) |
| C-6 | When the design text and the implementation differ | The design's `effectKo`/`value` wins (plan 8.2) |
| C-7 | Where to put the legacy rank tables (cost, duration, stun) | Read them data-driven (extend `runtime_parameters.json` + `SkillTreeD4.json`). The fingerprint update goes in the PR |
| C-8 | Caps on placed and summoned objects (traps 2, turret 1, orbs 3, decoy, clone) | Give each skill a per-caster cap and remove the oldest one when it is exceeded (A03-3 turret 1, M11-2 orbs 3, A18-1 clone 1) |

**Warrior**

| # | Question | Default |
|---|---|---|
| W-1 | Is the W09-2 "+20% per stack" additive or multiplicative | Additive to the bleed damage per stack |
| W-2 | The design's resource notation is inconsistent: W09 "Resource on hit +8→12", W08 "Resource gain +10→18", W08-1 "Extra resource +4→12". Per hit or per cast | All once per cast (when there is at least one hit) |
| W-3 | If W01-3 shortens the tick interval, does the cost change too | Reduce the tick cost in proportion to the interval so that the cost per second stays the same |
| W-4 | Is the W03 base a single strike or a 100° multi-hit | Keep the current 100°. The design owner fixes the design text (SD4:581) |
| W-5 | The relation between W02-3 "2 charges, recharge 8→6s" and the base skill's cooldown 8→6.4s | Build a new charge model (a dedicated `charges` state, saved). While the modifier is chosen it **replaces** the cooldown, the recharge time is the modifier rank's from→to (8→6s), and the base rank's cooldown reduction does not apply |
| W-8 | W06-3 "resource regeneration stops for 6 seconds" versus the implementation (borrow and repay) | Stop regeneration as the design text says. Do not use the loan code |
| W-13 | WP13 "70% or below" versus `<` | The design (≤) |

**Ranger**

| # | Question | Default |
|---|---|---|
| R-1 | The base effect and element of A06 (Elemental Imbuement), and which shots consume it | Base: the next 3 shots deal +25→35% extra damage (no element). The shots that consume it are BASIC, A01 and A02 (the old rule). Forks, echoes, turrets and rain do not consume it |
| R-2 | The A01-1 bounce rules (`hitIds`, `maxHits`) | A target is hit only once, and the number of bounces is the rank value (1, 1, 2, 2, 3 from `rank_values.json`), each reflecting off the wall normal. The forecast shows only the first segment |
| R-3 | A03-2 Frost Snare: a zone or one-shot (the old A10 is a one-shot blast) | A zone, as in the design (slow 30→50% while the trap exists) |
| R-4 | Movement and interruption during A05-1 aiming | Stop auto-movement while aiming, and cancel when damage is taken (plan section 8) |
| R-5 | A07 resource: the design "+8→12" vs the code cost 12 and a 25% reserve | Change to the design (gained on hit) and remove the cost entry |
| R-6 | The duration of the A17-1 8 waves, and the meaning of A17-2 "Narrow area ×1.6→2.0" | A17-1 uses `ticks` 8 and a duration of 4s. A17-2 uses radius ×0.5, damage per wave ×1.6→2.0, period .25, and keeps the duration (`ticks` 10) |
| R-9 | The definition of A18-2 "fading" | Burst on expiry, consumption and removal over the cap |

**Mage**

| # | Question | Default |
|---|---|---|
| M-1 | Values where the design text and `effectKo` differ (M01-1 ×1.4→1.8 vs "area ×1.5, damage ×1.6", M02-2, M03-1, M03-3, M04-2, M04-3, M06-3, M08-1·2·3, M11-2, M18-1) | `effectKo`/`value` wins (C-6). The design owner fixes the text |
| M-2 | M07: today it costs 18 with cooldown 3, but the design's only base value is "Mana on hit +6→10", a T1 generator | Keep the cooldown, cost 0, mana +6→10 (once per cast, when there is a hit). The auto-cast condition is "while mana is not full" |
| M-3 | Does M04-3 keep M05's last-element reaction | Keep it (the `lastElement` record is needed by M17-1 and M18-2) |
| M-4 | Is the M08-1 circle a disc or a ring, and does it follow the hero | A disc that follows the hero |
| M-5 | `lastElement = 0` in M17-1 | Substitute fire |
| M-6 | The radius, cap and targets of MP20 Shatter | Every enemy within 2.5m; bosses burst on stun (stagger) |
| M-7 | The window and reset of MP21 Align the Elements | When all three elements have touched one enemy once each, it bursts and resets. No time window. Damage over time and `EXTRA:` hits do not count |
| M-9 | M18's cost −20% and 3-element blast | Keep them (they are in the code but not in the design's base text: the design owner fills in the text) |
| M-12 | MP07 and MP09 12→20, MP16 8→12 | The design values (from→to) |

Among the questions above, **those that need a user decision** (those whose default changes behaviour or balance significantly: W-4, R-1, C-5, M-9) have been carried over to plan section 17 as D21~D24.
