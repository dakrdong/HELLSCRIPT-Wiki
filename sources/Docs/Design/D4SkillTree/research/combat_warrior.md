# D4 Skill Tree: Warrior combat research (read-only)

**Paths.** W=/Users/t8g-2410-pn-005/Documents/MyProject/HELLSCRIPT/wt-d4-skill-plan; R=W/Assets/HELLSCRIPT/Runtime/Core; T=W/Assets/HELLSCRIPT/Tests/Editor; J=W/Assets/HELLSCRIPT/Resources/Data; D=W/Docs; X=W/tools. Aliases in R/: Sk=CombatSimulation.ClassSkills.cs, Fx=...ClassSkillEffects.cs, Au=...ClassSkillAuto.cs, Ps=...ClassSkillPassives.cs, Po=...ClassSkillPolicies.cs, Id=...SkillIdentity.cs (no plain SkillIdentity.cs exists), Sim=CombatSimulation.cs, Act=...Actions.cs, Dmg=...Damage.cs, Eff=...Effects.cs, Cs=...ClassSets.cs, Rules=...Rules.cs, Cat=ClassSkills.cs, Tree=ClassSkillTree.cs, SE=SkillEffects.cs, GC=GameCatalog.cs. Other bare names starting Edict, Forecast, Field, Monsters, MonsterTraits, Attributes, Legendaries, ClassSkillValidation are CombatSimulation.<name> in R/. Any other bare name is in R/ (test names in T/). Data: CS=J/ClassSkills.json, RP/CAT=D/Design/ClassSkills/runtime_parameters.json / catalog.json, SD4=D/Design/D4SkillTree/skill_tree_d4.json, Plan=D/Implementation/D4_Skill_Tree_Plan_20261012.md. Classes: A=parameter only, B=new behaviour on an existing path, C=new mechanic or state.

## A. Framework

**Two engines (key finding).** W01-W06 and WP01-WP06 are `legacy:true` (actives CS:78-340).
- They run on the hero-action state machine: `TryCastClassSkillCore` calls `StartHeroAction` (Sk:155-158, Act:49-82), `ResolveSkill` applies the effect (Sim:291-353), and W01 is a channel (`TickChannel`, Sim:446-461).
- Their numbers come from GC:467-472, SE and literals (W01 `.5f`/`.25f` Sim:458, W02 `1.8f` Sim:304, W04 stun `1.5f` Sim:323), not CS keys. Editing CS.json for them changes nothing in combat except W02's damagePerMeter (Ps:106); RP has rows only for W02, W05, W06 (RP:597,615,618).
- W07+ are native: `ClassSkillCast` plus the `ResolveClassSkill` switch (Fx:93-163), values via `N(id,key)` (Sk:28).
- Of the ten new Warrior actives, W01-W06 are legacy; W08, W09, W17, W18 are native.

**Definition.** `ClassSkillDefinition` (Cat:9-23). X/class_skill_runtime.py:8-14 merges CAT `parameters` (cooldownSeconds, resourceCost) with RP numbers (range, radius, coefficient, duration, period, targets, prepare, width, secondary, skill keys) into CS. `ClassSkills.Data` hard-requires 111 skills and unlock<=40 (Cat:52-56).

**Loadout/ranks.** `ClassSkillLoadout` (Cat:73-173) holds `actives[4]`, `ultimate`, and `ranks` = `SkillInvestment{id,rank}` (Cat:72,79). `Rank(id)` defaults to 1 (Cat:84). Tree versions 3/4 (v4 allows rank 0 = unlearned) use `Remaining=level-Spent` (Tree:25-28). Combat reads `Loadout` = `State.build.classSkills`, or a deep copy under the recommended edict (Sk:23, HuntEdictDefaults.cs:11-18). `ProjectLegacy` writes the 12-slot `build.skillRanks` that legacy code reads (Cat:168, Sim:277).

**Auto-cast.** Every 0.2 s (Sim:241-247) `DecideEdictActions` (EdictOrder.cs:191), or directly (Sim:246), calls `TryAutomaticClassSkill(category)` (EdictOrder.cs:224). That walks `Loadout.order` (Sk:179-201).
- Native ids: `InspectClassSkill` (Sk:116-140: equipped/unlocked/busy/cooldown/resource/range, id lists Sk:134-136, AUTO_OFF). Then `ClassAutoCondition` (Po:60-110) falls to the hard-coded `ClassAutoDefaultCondition` switch (Au:97-147) unless a non-NATIVE option applies.
- Legacy ids: the condition is always true (Au:104). Real gating is in the edict planners (`InspectRuleCore`, Rules:159-262: `PlanEdictWhirlwind/Leap/IronWall/Shout/Slam`, and `PlanEdictAim` for W03), then `TryClassLegacyAutomatic` to `StartHeroAction` (Au:10-26).
- Buff vs attack category: `EdictRuleOrder.BuffSkill` (HuntEdictV2.Order.cs:49).

**Cast execution, cooldown, resource.**
- Native: `ClassSkillCast` is built at Sk:160, then `CaptureClassCast` (Ps:20-46). Cost and cooldown are set at Sk:161-164 (`CS.cooldowns`; the two ultimates share one timer, floor 45 s). Release comes after `prepare` (Sk:167-174, 214), then `AfterClassCast` (Ps:47-56). Lingering effects and dots live in `TickClassEffect` (Fx:182-238) and `Dot/TickClassDots` (Fx:37-78).
- Legacy: cooldown is `State.cooldowns[i]` (Act:73, ticked Sim:234) and cost is `Cost()` (Sim:283-290). A `ClassSkillCast` is still registered (`OriginalClassCast`, Sk:220-224, Act:66) with `rankScale` hard-coded to 1 (Sk:223).
- Resource gain is `Resource()` (Sk:76-80); regen is Po:198-212.
- Effects whose source skill is unequipped are purged every tick (Sk:239), so modifier effects must carry the base skill id as `source`. Persistent effects are whitelisted (ClassSkillPersistence.cs:47-48).

**Passives and rank scaling.**
- `Passive(id)` and `PV=value*RankScale` (Sk:30-31); passive effects are inline hooks in Ps.
- Legacy first-row passives read `SE.PassiveBase` (SE:14) in Dmg:30-31, CombatSimulation.Passives.cs:28-35, Economy.cs:129-131,191.
- Native `RankScale`=1+0.1(r-1), with ultimates fixed at 1 (Cat:65), applied by hand per effect (`a.rankScale` Fx:12-13,40,100-126).
- Legacy rank scaling comes from `SE.Power/ShieldFraction/ShoutResource` (SE:27-34) and `runeSkillPower` in `CaptureDamage` (Sim:494), used by `Hit` (Dmg:66-67). So `SkillHit(secondary)` on a legacy cast ignores base rank unless Sk:223 changes.
- Run validation caps rankScale at 1.401 (ClassSkillPersistence.cs:37).

**Direct hits.** There are four definitions:
- `DirectHit(kind)`=Direct|Basic (Attributes.cs:77: elite runes, set hooks, the `AfterClassHit` direct block Ps:133-153).
- "Normal direct"=`Normal(id)&&Kind.Direct` (Ps:103,122: additive bonuses such as WP15/WP17, proc passives).
- Legendaries fire for any non-passive class-skill id whatever the kind, bleed ticks included (Legendaries.cs:46,83-88).
- Secondary hits use `Kind.Set` and an `EXTRA:` id, which is non-native, so they count for none of the above (Fx:12, Ps:121).

**Use options (55 groups, 166 choices; the brief's 164 is wrong, X/class_skill_options.py:10).**
- An option is `{id,choice}` in `Loadout.options` (ClassSkillOptions.cs:21,34-61). Every consumer is a decision site: `UseChoice` in `PolicyTargetEligible`, `ClassAutoCondition`, positioning intents (Po:10-219), journal (Sk:152), read model (ClassSkillReadModel.cs:34).
- `LEGACY` choices only patch HuntEdictV2 option values (ClassSkillOptions.cs:62-73) that planners read (HuntEdictV2.Options.cs:40-75).
- `ResolveClassSkill` and `ResolveSkill` never read them. Choices change WHEN, WHERE and AT WHOM, never WHAT.
- The `{id,choice}` shape could hold a modifier id but should not:
  - `HuntEdictDefaults.Skills` clears `options` (HuntEdictDefaults.cs:14).
  - `Validate` caps entries at 55 and strips defaults (ClassSkillOptions.cs:46,55).
  - There is no rank field.
  - Quick-preset code treats every group of a skill as a use policy (HuntEdictQuickPresets.cs:129,190, HuntEdictSkillPresets.cs:78).
- Plan 7 already stores `{id,rank,modifier,modifierRank}` on `SkillInvestment`.

**Best extension point.** Add `modifier`+`modifierRank` to `SkillInvestment` (Cat:72) and resolve once per cast into `ClassSkillCast` (ClassSkillState.cs:28-38, serialized in `CS.casts`) inside `CaptureClassCast` (Ps:20). Both engines call it (Sk:161, 223).
- The cast object already carries per-cast tweaks for legacy skills: `ClassArea(root)` (Sim:313,320, Cs:42) and `ClassTiming(root)` (Act:66, Cs:43).
- Effects read `Cast(root)` (Sk:54). Decision-side code (planners, cost, forecast, aim) has no cast yet and uses a loadout accessor beside `RankScale` (Sk:29).
- Native parameter-only modifiers can overlay `N()` (Sk:28).
- Fall back to the loadout when `Cast(root)` is null. Legacy casts are purged after 30 s without effects (Sk:218), and a W01 channel is not `CS.action`.

Call sites:
- Capture: Sk:159-161, 223, Ps:20-46.
- Native effects: Fx:93-163,182-238, 37-78, Ps:99-193, Id:77-90.
- Legacy effects: Sim:291-353,446-461, 283, Act:57,73,97-101, SE:29-30.
- Decisions: Sk:116-140, Au:55-147, Po:50-59, Rules:159-262, EdictWarrior.cs:24-98, EdictAim.cs:35-80, EdictSlam.cs:18-56, EdictWarriorSupport.cs:11-91, Forecast.cs:87-98, EdictWhirlwindMovement.cs:10-45.
- Read models: ClassSkillReadModel.cs:17-28, SkillProgressionInfo.cs:16-41.
- Codec: ClassSkillCodec.cs:42-49. New fields break its canonical-JSON comparison for old HED4 codes.

## B. Per skill (CS = current values, SD4 = spec)

### W09 Raking Wound (T1 generator, native, SD4:259-345)
- Where: `ResolveClassSkill` case Fx:105; ledger `Dot/CashDot/TickClassDots` Fx:37-78; auto Au:109, aim Au:85.
- CS:1316: cooldown 4, cost 15, range/radius 3, coefficient .5, width 90, duration 4, period 1, targets 5, prepare .35, secondary 1.2.
- Now: up to 5 enemies (lowest ids, Sim:487-488) in a 90 degree cone take D50% plus a D120% bleed bank ticking each second (Fx:52,72). A recast keeps the larger remainder (Fx:48-51). The spec swaps cost 15 for "+8 to 12 resource on hit", and Au:109 (unbled elite or 3+ near) suits a spender, not a generator.
- W09-1 B: cast counter via `Counter()` (Sk:35-39); every 3rd cast runs `CashDot`+`Heal` (reuse W10, Fx:106-108). W10 hooks Au:110, Po:39,89 become dead.
- W09-2 B: hit `Enemy(a.target)`. Stacks are extra ledger entries via `Dot(additiveBank,cap)` (Fx:43-47); stack count = entry count.
- W09-3 A: `width` 90 to 360 (arc>=360 skips the angle test, Sim:488) and `duration`. Po:53, Au:85 read the same keys. A shorter duration raises DPS (`bank/duration`, Fx:52).

### W08 Impaling Hook (T1, native, SD4:354)
- Where: Fx:101-104; MISS check Sk:170; target-required lists Sk:134, Au:51,107-108.
- CS:1246: cooldown 7, range 6, coefficient .65, targets 1, prepare .3, resourceGain 10, pullMeters 2.
- Now: one target takes D65% and is pulled up to 2 m (bosses get a 0.5 s stun instead, Fx:17-36). Resource is +10 times rankScale (14 at rank 5; the spec's 10 to 18 needs explicit from/to). Au:108 (target beyond 3 m) fits a pull, not a generator.
- W08-1 B: reuse W11's mover (prepare+.25 s, `Lerp` in `TickClassSkills`, Sk:159,213; walking is blocked while `CS.action` runs, Sim:364) toward a stop-short point. Add W08 to the landing list (Sk:136). Extra resource on arrival.
- W08-2 B: `Line(origin,aim,6,width,3..7)` (Sk:95-100; add a width key), `SkillHit` each, `Pull` only the last.
- W08-3 B (absorbs W07): `ApplyStatus(Mark,"W07",6)` (Fx:98-100; Eff:106-107 clears other marks). Guard is literal `.1f` in `ClassGuard` (Ps:179, cap .5 Ps:190). Kill resource needs a new bit in `ClassHitFlags` (Ps:94-98) read in `AfterClassHit`. Traps: hard-coded "W07" in `HasAmplifyingMark` (Sk:90; otherwise the brand amplifies damage like a Ranger mark), Ps:179, Au:51; DES_LW41 (Fx:103).

### W01 Whirlwind (T2 core, legacy, SD4:449)
- Where: `StartHeroAction` Act:61,80; `TickChannel` Sim:446-461; `MoveEdictWhirlwind` EdictWhirlwindMovement.cs:10-45; `PlanEdictWhirlwindCandidate` EdictWarrior.cs:25-50.
- GC:467/CS:78: cost 6, coefficient .5, range/radius 2.5, cooldown 0; literals `.25f`, `.5f`, `RuneSkillRadius(0,2.5f)` (Sim:451,453,458).
- Now: every 0.25 s it pays 6 and hits all enemies within 2.5 m for D50%. It ends after 0.5 s with no enemy, no resource, or an edict rule (Sim:451-457, EdictWarrior.cs:125-140). The hero may walk meanwhile (Sim:364). Rank D50 to 70 already matches +10%/rank (Sim:494, Dmg:66-67); cost 6 to 4 has no rank term (Sim:283-290).
- W01-1 B: x2-3 must be applied after the `SpeedWithBonus` cap of 1.5x (Economy.cs:45, Field.cs:14-19), in EdictWhirlwindMovement.cs:40 and Sim:437. It may outrun targets and trip the 0.5 s empty cutoff. `HeroPace` slows still multiply (MonsterTraits.cs:13-20).
- W01-2 A: scale inside `RuneSkillRadius` (RuneCombatBonuses.cs:45). Every site funnels through it (Sim:451,458, EdictWarrior.cs:29, EdictWarriorSupport.cs:67, Forecast.cs:95-96), but the walker still hugs at 2.3-2.5 m (EdictWhirlwindMovement.cs:29, Sim:393).
- W01-3 A: interval literal at Act:80, Sim:458. Each tick also pays cost, pulls (Sim:460) and rolls crit, so cost per second doubles at x0.5 unless scaled.

### W03 Crushing Blow (T2, legacy, SD4:544)
- Where: `ResolveSkill` Crush case Sim:312-318; `PlanEdictAim` EdictAim.cs:35-80; `ForecastAttack` Forecast.cs:97-98; timing CombatActions.cs:95.
- GC:469: cost 25, coefficient 2.2, range/radius 3, prepare .65/attack speed.
- Now: 100 degree cone, radius `RuneSkillRadius(2,3)*ClassArea` (Sim:313); every enemy takes D220% (LW03 single bonus and SWB4 extra hit, Sim:315-317). The spec text calls the base a "single strike": mismatch.
- W03-1 B (cone to circle): arc 100 to 360 and radius 2 to 3 at Sim:313, Forecast.cs:98 (50 degree test), EdictAim.cs:20, Cs:136. `Cast.area` can carry the radius (Cs:42).
- W03-2 B: coefficient x2 when `health<=maxHealth*threshold` (same test as WP06, Dmg:31). Refund amount is unspecified.
- W03-3 B: `ScheduleExtra` "burst" x2 at +1 s (Cs:111-114, Fx:233). It is `Kind.Set`, so no WP15/WP17/legendary credit.

### W05 Iron Wall (T3, legacy, SD4:639)
- Where: `ResolveSkill` Shield case Sim:324 (4 s literal) into `AddShield` Eff:22; `PlanEdictIronWall` EdictWarriorSupport.cs:11-37; guard Act:117.
- GC:471: cooldown 14, 30% max HP (SE:31), prepare .2. Now: one shield (replaces its own, capped by max HP, Eff:24-33) times `shieldMultiplier` (WP05).
- W05-1 B (W12): the counter is `AfterClassHurt` (Ps:164-165; 2 counters, 1 s apart). Gate on `OwnIronWall` (EdictWarriorSupport.cs:9). W12's +15% block (Ps:192) is not in the spec.
- W05-2 B (W16): `ClearHeroControl` + `Buff("W16:immune")` (Fx:120-126, MonsterTraits.cs:25; `HeroControlImmune` keys on that id, Monsters.cs:88). W16 special cases (Sk:190, EdictOrder.cs:132, Au:116) must retarget.
- W05-3 B: `ReactShield` already runs per absorbing shield (Id:77-90, Eff:75). Reflect absorbed x fraction to `Nearby(..,3)` as secondary damage (not the thorns stat, Attributes.cs:115).
- Risk: the `IronWallAmount` precheck uses literal .3 (EdictWarriorSupport.cs:10).

### W06 Battle Shout (T3, legacy, SD4:734)
- Where: `ResolveSkill` Shout case Sim:325 (`shoutTime=6`), bonus in `CaptureDamage` (Sim:494), `PlanEdictShout` EdictWarriorSupport.cs:71-91. GC:472: cooldown 16; 40 resource and +20% (SE:32-33) equal the spec at rank 5.
- W06-1 A: add `shoutTime>0` to `ClassMoveBonus` (Ps:168-174); total is capped at +50% (Economy.cs:45). CC reduction exists only in behaviourKo.
- W06-2 B: `ClassAttackSpeedBonus` (Ps:193) feeds a hard 1.5 cap (Act:57). Skip the refill in Sim:325 and in `PlanEdictShout` (EdictWarriorSupport.cs:85).
- W06-3 B (W15): loan `BorrowResource/RepayResource` (Id:22-38; repaid by regen Po:203, close hits Id:63 and blocks Ps:162) differs from the spec's "regen pauses 6 s". The loan validator is keyed to "W15" (ClassSkillPersistence.cs:47-50).

### W02 Leap Slam (T4, legacy, SD4:829)
- Where: `TickHeroAction` lerp Act:97-101 (timing .15+.45, CombatActions.cs:94); `ResolveSkill` Leap case Sim:298-311 (radius 2.5, literal 1.8f :304); `ClassLanding` Ps:57-69; `PlanEdictLeapCandidate` EdictWarrior.cs:64-98 (2-8 m).
- GC:468, RP:597: cooldown 8, coefficient 1.8, range 8, radius 2.5, damagePerMeter .15 (Ps:106).
- Now: a lerp jump (no body collision), then D180% in 2.5 m. Cooldown 8 to 6.4 equals the escape curve (SkillProgression.cs:57), but Leap is not `Escape` (SE:28-29).
- W02-1 C: shove along the path via `Line`+`BreakFront` (Id:11-21; W11 Fx:109-112) with new push/width keys.
  - Decide whether landing damage stays.
  - The presentation arc `HeroAirHeight` (Act:28) must be off.
  - `EdictLeapLandingValid` has a 2 m minimum (EdictWarrior.cs:51).
  - The "space" policy (Po:177) changes meaning.
- W02-2 B: schedule a "burst" +2 s plus stun at `ClassLanding` (`ApplyStatus`, Eff:95-115; "burst" needs a stun branch).
- W02-3 C: there is no charge concept. `State.cooldowns[]` is read by planners (EdictWarrior.cs:79), LW04 (Dmg:156-157), ReduceCooldown (Legendaries.cs:111,128) and the read model (ClassSkillReadModel.cs:24-25).

### W04 Ground Slam (T4, legacy, SD4:924)
- Where: `ResolveSkill` Slam case Sim:319-323 (circle radius 3*`ClassArea`, stun literal 1.5 s); `PlanEdictSlam` EdictSlam.cs:18-56 (uses W03's rune index 2 at :35; literal 15 boss credit :50). GC:470: cooldown 10, coefficient 1.2, prepare .35.
- Now: D120% and a 1.5 s stun in 3 m; bosses gain control meter (Eff:98-103). The spec's stun 1.5 to 1.9 is new.
- W04-1 B: `Pull` (Fx:17-36) before `AreaHit`. A 5 m radius needs pull-to-hero (W13 pulled 1.5 m).
- W04-2 B (circle to line): `Line()` (Sk:95-100) + Slow (W14, Fx:118) in the Slam case. Update Forecast.cs:87-98, EdictSlam.cs:35, EdictWarriorSupport.cs:67; DES_LW44 (Ps:44, Sk:250).
- W04-3 B: stomp at release, two scheduled bursts at +0.6/+1.2 s, stun on the last; anchor to `a.origin`. Follow-ups are `Kind.Set`, so no WP17.

### W17 War of the Ancestors (T5, native, SD4:1019)
- Where: `ResolveClassSkill` Fx:127-129; `TickClassEffect` ancestor case Fx:213-217. CS:1868: cooldown 60, range 6, coefficient .65, duration 8, period 1, summons 2, ticks 8, guard .15.
- Now: 2 ancestors hit the hero's nearest enemy each second as secondary damage. `RankScale` is 1 for ultimates (Cat:65), so the spec needs +20%/rank and cooldown 60/55/50. W17 and W18 share one timer, floored at 45 s (Sk:163) and validated 45-60 (ClassSkillPersistence.cs:26).
- W17-1 A: `summons`, `duration`, `ticks`.
- W17-2 C: there is no last-skill record. Add a field to `ClassSkillRuntimeState` (ClassSkillState.cs:39-53) and a replay. Copies must skip `CaptureClassCast` (WP15 history, Ps:25-26) and use `EXTRA:` ids.

### W18 Titan's Judgment (T5, native, SD4:1097)
- Where: Fx:130-132; W03/W04 +25% for 6 s (Ps:108). CS:1942: cooldown 60, radius 4, coefficient 6, duration 6, targets 8, prepare .9, stun 2.
- W18-1 A: `prepare` is read before the cast exists (Sk:159), so the accessor goes there. The hero is busy 1.8 s (Act:26).
- W18-2 C: a form buff replacing the strike: area +50% (via `Cast.area`, Cs:44-45, which W01/W02 ignore) and knockback immunity (`PushHero`, Monsters.cs:96). A persistent effect needs a ClassSkillPersistence.cs:47-48 whitelist entry.

### Absorbed legacy skills (all native; reuse as noted)
- W07 (to W08-3): Fx:98-100, CS:1174: cd 10, range 8, 6 s, gain 10, guard .1. Brands one enemy (clears older brands), +10 resource, branded attacker deals 10% less (Ps:179). Reuse status+guard; rename "W07" literals.
- W10 (to W09-1): Fx:106-108, CS:1378: cd 8, radius 4, coef .8, heal .01, cap 5. D80% in 4 m, cashes only the W09 ledger, heals 1% per enemy (max 5). Reuse `CashDot`+`Heal`.
- W11 (to W02-1): Fx:109-112, Sk:213, Id:11-21, CS:1449: cd 9, coef .9, push 1.8, guard .15, gain 15. Dashes 4 m, shoves enemies sideways, grants guard and resource (not in spec).
- W12 (to W05-1): Fx:113-115, Ps:164-165,192, CS:1523: cd 12, cost 10, block .15, 2 counters, coef .8. 4 s stance, +15% block, counters.
- W13 (to W04-1): Fx:116-117, CS:1594: cd 10, cost 15, radius 4, coef .7, pull 1.5. Hits then pulls 1.5 m (spec: pull first).
- W14 (to W04-2): Fx:118, CS:1660: cd 6, cost 20, range 7, width 1, coef 1.5, slow .3 for 3 s. Line hit plus slow.
- W15 (to W06-3): Fx:119, Id:9-38, CS:1726: cd 18, gain 60, repay 4. Instant 60 resource, debt repaid by regen/hits.
- W16 (to W05-2): Fx:120-126, CS:1797: cd 16, barrier .12, immune 2, duration 5. Barrier, clears control, 2 s immunity.
- Cleanup:
  - One option group per non-passive skill is enforced (ClassSkillPolicyTests.cs:58-59).
  - DES_LW41-45 hooks (Fx:103,105,107,115,117,122-125, Ps:44,61) apply only with the modifier (Plan 12).
  - The `BuffSkill` list (HuntEdictV2.Order.cs:49) needs updating.
  - All 10 Warrior validation builds use removed ids.

## C. Passives

| Passive | Applied at | Rank 1 to 5 today vs SD4 |
|---|---|---|
| WP01 | Dmg:30-31 (3+ enemies within 3 m, frozen at cast, Sim:496); `SE.PassiveBase[0]`=25 | 25 to 35 = spec (x(1+.1(r-1)), SE:23, SkillProgression.cs:56; SD4:1197) |
| WP06 | Dmg:31, per target at or below 30% HP; base 35 | 35 to 49 = spec |
| WP05 | Economy.cs:191 adds to `shieldMultiplier`; base 40 | 40 to 56 = spec |
| WP04 | CombatSimulation.Passives.cs:28-35, once per 20 ticks; base 2 | 2 to 2.8 = spec |
| WP13 | Ps:167, `health<.7*hp` (strict) | .4 to .56 = spec |
| WP15 | Ps:25-26 (4 s window, `Offensive` skills only) | .2 to .28 = spec |
| WP17 | Ps:22,27,103-105; ratio captured before cost (Sk:160, Act:66-67); W03/W04 direct hits | .15 to .21 = spec |
| WP09 | Ps:163 (`Proc`, 1 s per root) | 18 x1.4 = 25.2, SD4 says 26 (SD4:1277): needs from/to |

The four legacy passives need no change. WP20 (40 to 80) and WP21 (6 to 10) are not x1.4, so rank tables must be data (Plan 5.3). Key passives are absent from data: `ClassSkills.Data` counts 111 (Cat:52), `Canonical` needs one rank per definition (Cat:122), and unlock Lv45 exceeds the 40 caps (Cat:55,62,137, Tree:42). Ablation tooling assumes one `value` key (ClassSkillPassiveMeasurement.cs:45-46).

**WP20 Blood Frenzy.**
- Bleed crit: `TickClassDots` calls `ApplyOutgoing(..,critMultiplier 1,critical false)` (Fx:73). Under `Passive("WP20")&&d.id=="W09"`, roll `RandomStream.Unit(ref State.rng)<Mathf.Min(.75f,d.snapshot.crit)` and use `snapshot.critDamage`, as `Hit` does (Dmg:69-73); `CashDot` (Fx:62) likewise. The roll shifts the shared rng that restore receipts compare (ClassSkillNativeSmoke.cs:29).
- Explosion: add bit 16 `HasDot("W09",enemy)` to `ClassHitFlags` (Ps:94-98; evaluated before damage, Dmg:41). `Deal` already sets `enemy.dead` (Sim:508), which AP10 relies on (Ps:135). So in `AfterClassHit` (Ps:119-158):
  `if(Passive("WP20")&&(flags&16)!=0&&enemy.dead&&Once("WP20",root,enemy.id))Splash(a,enemy.position,r,N("WP20","value"),0,secondary:true,source:"WP20")`
- Pass the raw value: `SkillHit` already multiplies passive sources by `RankScale` (Fx:12), which is wrong for rank tables. `EXTRA:` ids are non-native: no recursion, not direct hits.

**WP21 Unending Resolve.**
- No category notion exists. There is only buff/attack (HuntEdictV2.Order.cs:49-50), `Normal/Offensive/Native` (Sk:87-89) and result metrics (CombatStatistics.cs:10).
- The design has `roleKo`: W09/W08 generator (SD4:262,357), W01/W03 core (452,547), W04 control (927), W02 mobility (832), W05/W06 defense (642,737). The generator plan omits it, so add a `role` field.
- Trigger beside WP15 in `CaptureClassCast` (Ps:25-26):
  `var c=Counter("WP21"); bool alt=c.until>State.time&&c.previous!=role; c.count=alt?Mathf.Min(3,c.count+1):(c.until>State.time?c.count:0); c.previous=role; c.until=State.time+4; a.additive+=c.count*PV("WP21");`
- It reaches only direct hits of the casting skill (Ps:103-105).

## D. Test patterns
- **Fixture.** `ClassSkillRuntimeTests.Fixture(skill,passive,equipment)` (T/ClassSkillRuntimeTests.cs:23-41):
  - Builds `GameStore.NewAccount`, level 40, loadout `[skill]`, `new CombatSimulation(account,catalog,1,0,seed:42)`.
  - One 100000-HP stationary enemy 2 m up, `Stats` overwritten (damage 100, hp 1000, crit 0), resource 50/100, `decisionTime=100000` (no AI).
  - Helpers: `Advance` (:42; `Tick(Step=.05)`, Sim:10), `Target`, `CastSkill` (:44-48), `AddSkill` (:49-53), reflection `Call` (:17-22), `Begin/Land/Status/Use/HurtAmount` (ClassSkillPassiveTests.cs:12-35), `ExtraEnemy` (ClassSkillGearTests.cs:12-15), `Policy/GlobalTarget` (ClassSkillPolicyTests.cs:13-23).
  - Legacy-path variant: SkillRankEffectTests.cs:17-36 (seed 5150, hunt rules, `FirstHit`).
- **Reading results.** `State.damageEvents` (CombatEffects.cs:36-44), `State.classSkills.{dots,effects,casts,cooldowns}` (ClassSkillState.cs:39-53), `shields`, `effectEvents`, `enemy.statuses`, `ClassSkillChanged` (Sk:10).
  - Patterns to copy: with/without pairs (`PassiveScenario` ClassSkillPassiveTests.cs:36-96, `GearScenario` ClassSkillGearTests.cs:16-50), rank ratios (SkillRankEffectTests.cs:38-50), restore equality (`ClassSkillNativeReceipt`, ClassSkillNativeSmoke.cs:17-31; ClassSkillExperimentTests.cs:35-39).
  - Contributions tests (ClassSkillRuntimeTests.Contributions.cs:25-77) assert heal/protect/resource ledgers per `source` string (Eff:10-12), so modifier effects need a stable source id.
  - `RunClassSkillValidation` (ClassSkillValidation.cs:59-91) reports damage, casts and movement meters for 60 s of automatic play. Use it for the auto-movement checks of W01-1, W02-1, W08-1.

```csharp
// helpers are real; r.modifier/modifierRank are the new v5 fields
int HitsOnSide(int modifier){
  var s=Fixture("W09");var r=s.State.build.classSkills.ranks.Single(x=>x.id=="W09");
  r.modifier=modifier;r.modifierRank=modifier<0?0:1;
  var side=ExtraEnemy(s,s.State.position+Vector2.right*2);Call(s,"Sense");
  Assert.IsTrue(CastSkill(s,"W09"));Advance(s,.6f);
  return s.State.damageEvents.Count(d=>d.definitionId=="W09"&&d.kind==DamageKind.Direct&&d.targetId==side.id);
}
[Test] public void SpinningRendReachesBesideTheCone(){Assert.AreEqual(0,HitsOnSide(-1));Assert.AreEqual(1,HitsOnSide(2));}
```

- **Determinism.** `Hit` always draws `State.rng` (Dmg:70). New rolls shift later ones, so keep crit 0 and compare equal seeds.
- **Ablation tool.** ClassSkillPassiveMeasurement is `Explicit`:
  - `Kits` (:62-76) and `Assist` (:80-87) name removed ids.
  - `CreateAccount` removes a passive by blanking a passive slot (:101), which v3+ forbids (Cat:120), so zero its rank instead. The "+W03" yardstick edits `rank` (:103) and still works.
  - `ClassSkillValidationBuild` (ClassSkillValidation.cs:8-9) has no modifier field, and all 10 Warrior builds use removed ids.
  - Dependent tests: ClassSkillCompatibilityTests.cs:14-31,39-61, ClassSkillExperimentTests.cs:35,55, ClassSkillNativeSmoke.cs:47-50, ClassSkillPassiveTests.cs:11 (WP07-WP19).
- **Balance1000.**
  - X/balance_1000.py:222-231 hashes `ClassSkills.cs`, `ClassSkillTree.cs`, `GameStore.SkillTree.cs`, `CombatSimulation.cs`, `CombatSimulation.Damage.cs`, `CombatEffects.cs`, `Economy.cs`, `HeroStats.Blacksmith.cs` and both skill JSONs. Any edit stales `--check`.
  - It hard-codes +10%/rank (:148) and level 40 (parameters.json).
  - X/test_balance_1000.py:19 reads production_skill_tree_audit.json (skillTreeVersion 3, levelCap 40).
  - X/class_skills.py:11-22,97-113 fingerprints `SkillEffects.cs`, `SkillProgression.cs` and the 18 `SkillDefinition` constructor arguments, so legacy rank tables need a fingerprint update.

## E. Open questions (unresolved from code)
1. W09: do cost 15 and cooldown 4 stay, or is it free? Is "+8 to 12 on hit" per target or per cast? For W09-2 "+20% per stack", is it a multiplier per application or on the total? W09-3: bleed is 2-3 s (effectKo) versus 4 to 2 s (behaviourKo); is the D120% total kept? W09-1: does the 3rd cast add W10's D80% hit?
2. W08-1/3: arrival point; extra resource on top of 10 to 18; does a kill by any source grant +20?
3. W01-3: should tick cost scale? W01-1: baseline for x2-3 given the 1.5x speed cap.
4. W03: single strike (SD4:581) or the implemented 100 degree multi-hit cone? Execute refund amount; aftershock position; WP17 on aftershocks.
5. W02-1: keep the D180 landing, the 8 s cooldown, W11's guard/resource? W02-3: recharge 8 to 6 s versus cooldown 6.4 s.
6. W04: does the stun survive wave/pull? Stomp anchor; WP17 on stomps.
7. W05-1: keep +15% block and the 2-counter limit? W05-3 target cap.
8. W06-1: CC reduction in or out? W06-2 under the 1.5 attack-speed cap, and its damage bonus. W06-3: loan versus 6 s pause, and extra 40-60 above max resource 100.
9. W17-1: base (2 ancestors, 8 s) versus 3 ancestors 6-8 s. W17-2: copyable skills, and whether copies count for WP15/WP21/legendaries.
10. W18-1: base 4 m versus 6-8 m. W18-2: scope of "+50% range" and "stagger immunity".
11. WP21: do W02, W05, W06 and ultimates reset, pause or ignore the chain? Stack window (WP15 uses 4 s); does a repeat of the same type reset?
12. WP20: crit per tick or per bleed; separate rng stream; does the W09-1 cash-out crit; explosion radius and target cap; chain explosions; do set bleeds count?
13. WP09: 26 versus 25.2. WP13: "70% or below" versus the strict test.
14. Where do the legacy rank tables live (W01 cost 6 to 4, W03 25 to 21, W02 cooldown, W04 stun), given the fingerprint checks?

**Key files (absolute).**
- /Users/t8g-2410-pn-005/Documents/MyProject/HELLSCRIPT/wt-d4-skill-plan/Assets/HELLSCRIPT/Runtime/Core/CombatSimulation.ClassSkills.cs
- /Users/t8g-2410-pn-005/Documents/MyProject/HELLSCRIPT/wt-d4-skill-plan/Assets/HELLSCRIPT/Runtime/Core/CombatSimulation.ClassSkillPassives.cs
- /Users/t8g-2410-pn-005/Documents/MyProject/HELLSCRIPT/wt-d4-skill-plan/Assets/HELLSCRIPT/Runtime/Core/ClassSkillState.cs
- /Users/t8g-2410-pn-005/Documents/MyProject/HELLSCRIPT/wt-d4-skill-plan/Assets/HELLSCRIPT/Tests/Editor/ClassSkillRuntimeTests.cs