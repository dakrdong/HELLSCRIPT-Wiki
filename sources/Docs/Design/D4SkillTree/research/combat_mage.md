# D4 skill tree: Mage combat research (read-only)

**Paths.** W=/Users/t8g-2410-pn-005/Documents/MyProject/HELLSCRIPT/wt-d4-skill-plan; R=W/Assets/HELLSCRIPT/Runtime/Core; T=W/Assets/HELLSCRIPT/Tests/Editor; J=W/Assets/HELLSCRIPT/Resources/Data; D=W/Docs. Aliases Sk,Fx,Au,Ps,Po,Id,Sim,Act,Dmg,Eff,Cs,Cat,SE,GC,CS,RP,CAT,SD4,Plan as in the Warrior report; Proj,Fc,Aim,Pers,Stt,RT,PT,GT,SRT as in the Ranger report. New: Ar=CombatSimulation.Areas.cs, Atr=...Attributes.cs, Eco=Economy.cs, CE=CombatEffects.cs, EdF/EdB/EdC/EdN/EdT/EdS=...Edict{Fireball,Blizzard,Chain,Nova,Teleport,Shield}.cs, EdMB=...EdictMageBasic.cs, EdR=...EdictResponse.cs, Ord=HuntEdictV2.Order.cs, Identity=T/ClassSkillRuntimeTests.Identity.cs. Bare `Passives.cs`, `Outcomes.cs` are CombatSimulation.* in R/. SD4 cites are the `"id"` line. A/B/C as before; **A\*** = the number comes from the modifier rank (not base `rankScale`), so it needs the shared modifier accessor.

## A. Mage-specific framework facts

**Engines.**
- Legacy hero-action: M01-M06 and MP01-MP06 (`legacy:true`, CS:598-826,1072-1157; numbers GC:479-484 plus literals). Native: M07-M18 (CS:2850-3643), MP07-MP19.
- Spec actives: legacy M01,M02,M03,M04,M06; native M07,M08,M11,M17,M18. Absorbed: M05 legacy; M09,M10,M12,M14,M16 native. Dropped with no modifier: M13,M15.
- Legacy numbers outside CS: M01 coef `2.1f`, speed 14, radius `RuneSkillRadius(12,2.5f)` (Proj:35-36; R/RuneCombatBonuses.cs:45); Blizzard `.65f`, r 3, 6 s (Sim:334); Chain `1.1f*.8^i`, 4 hops (Sim:345, Fc:10); Nova freeze `1.5f` (Sim:323); shield 35% (SE:31); prepare by index (CombatActions.cs:96-102).
- Rank: legacy +10% damage via `runeSkillPower`, keyed by `LegacyIndex(definition)` (Sim:494; Dmg:66-67): a hit labelled "M0x" gets base rank, "EXTRA:M0x" does not. Native `SkillHit`, `Dot`, `Placement.value` multiply `a.rankScale` (Fx:12-13,40,84): modifier numbers need a distinct `source` or a bypass. M04 cooldown -5%/rank (SE:28-30; 9->7.2 = spec) also adds +5% range/rank. **No rank term exists for cost** (Sim:283-290; spec M01/M03 25->21), Blizzard duration (6->8) or Nova freeze (1.5->1.9).
- Mage-only wiring on the five legacy skills: compiled rules `edict:M01..M06` (Ord:51,75-100), planners EdF/EdB/EdC/EdN/EdT (Aim:50-52, Rules:228-240), option tables (HuntEdictV2.Options.cs:100-124), forecasts Fc:69-98. Forecast==release is test-enforced (Identity:107-117), so every shape change must change its planner too.
- Inside `ResolveSkill` the legacy cast exists (`Cast(action.id)`) but `cast.snapshot` is still null (set by `OriginalClassRelease`, Sk:225-228, after Act:122). `Placement`/`Arrow` copy it (Fx:84,89); `SkillHit` falls back to live `CaptureDamage()` (Fx:11).

**Mana.**
- `State.resource` (saved, GC:410); `Stats.maxResource`=100+gear (Eco:224); `Stats.regen`={8,10,12}[class]+gear, x(1+MP05) built once (Eco:182, not saved), ticked by `RegenerateClassResource` (Po:198-212, Sim:235).
- Gains: `Resource()` (Sk:76-80, the only helper that emits RESOURCE events), basic attack +5 (Act:154), mana steal/lucky hit (Atr:100,109), potions (R/CombatSimulation.Potions.cs:77), LM03 (Eff:42).
- Costs: legacy `Cost()` (Sim:283-290, reductions capped 50%), native `ClassCost` (Ps:77-81); planners and HUD call these (Sim:274, EdN:56, EdS:48, EdT:31).
- `resourceRatio` is read before payment (Sk:160; legacy Sk:223 runs at Act:66, before Act:67) and becomes `reserve` (>=60%) / `highResource` (>=80%) (Ps:22) for MP17 (Ps:112). The basic attack is the mana filler (`MageActiveShort`, EdMB:12-28).

**Element state (three separate mechanisms).**
1. `CS.lastElement` (Stt:42; saved; 0 = none; validated 0..3, Pers:25; never reset). Only writer: `PrepareReactiveShield` (Id:74, from `AfterClassCast` Ps:49) when a cast with element 1..3 is RELEASED. Cast element = static `ClassSkills.ElementOf` (Cat:69-70: Fire 1, Cold 2, Lightning 3; M04, M05, BASIC and the "TriElement" ultimates give 0). Only combat reader: the M05 reaction (Id:75). Deleting M05 deletes the only writer, yet M17-1 and M18-2 read it: keep Id:74.
2. `ItemEffects.lastElement/lastElementTime/elementHitTicks[6]/elementBuff` (GC:303-305, saved): written per damage event by `RecordElementHit` (Passives.cs:36-45, from `ApplyOutgoing` Dmg:50) for native definitions only (BASIC counts as lightning, `EXTRA:` does not) and only while MP06 is learned (`Stats.passives[5]`). A second element within 80 ticks sets `elementBuff`=4 s, read by `CaptureDamage` (Sim:494; buffs later snapshots, not the triggering hit), M17's auto-cast (Au:142-143) and option ELEMENT_BUFF (Po:102). Cleared when MP06 is dropped (Passives.cs:50-55).
3. Per-cast counters: `Cycle` (Ps:14-19; `Counter` effects with a `history` list, saved) for MP16 (Ps:40) and M18 (Ps:41); MP11 count (Ps:38-39); cast flags `lightningCharge`, `elementCharge` (Stt:33).
- Nothing is stored per enemy: no element mask, no damage-while-frozen (`StatusEffect` CE:11-18 has no accumulator; `TickStatuses` Eff:148-166 has no expiry hook; bosses never get a Freeze status, Eff:98-103).
- Static-element trap: M07's element will come from the modifier, but cast creation (Sk:160,223), Au:139,142,144, Po:101,104, Ps:87 and test helpers (PT:15,21) all call static `ElementOf`, and Fx:147 hard-codes element 1. Make it loadout-aware and use `a.element`.

## B. Per skill (CS = current numbers, SD4 = spec)

### M07 Elemental Lance (T1, native, SD4:2948; today Ember Lance, unlock Lv22)
- Now: CS:2850 cd 3, cost 18, range 10, coef 1.4, width .6, targets 3, prepare .5, burn bank .6 over 3 s (RP:321). Fx:147 = `Line` + `SkillHit` + `Dot("M07")`. Auto Au:131 (>=2 aligned, or a non-burning elite; literals 10/.6/3), Po:55,63. Burn flag Ps:97 (`HasDot("M07")||InFirewall`) feeds MP07 (Ps:129). DES_LM41 Fx:147.
- Base: spec D120->168 (CS 1.4); +6->10 mana on hit is new (B: `Resource(..,"M07",a.root)` as W08 Fx:101; per hit or per cast?); no cost/cooldown in spec; no-modifier default is fire without burn (Plan:536). As the generator its auto rule must become "cast while mana is not full".
- M07-1 Ember **A\***: current body; `Dot` multiplies `a.rankScale` (Fx:40): pass the modifier value (D60->100).
- M07-2 Glacier **B**: M09 body (Fx:149-150: slow .35/3 s, freeze an already-slowed enemy), element 2, freeze 0.6->1.0 s (M09: .8).
- M07-3 Grounding **B**: M12 body (Fx:153) + conduit. The relay keys on literal "M12": Id:136,142-144, Ps:148-152, Au:59-65,136, Po:65. The spear is planted at `a.aim` (Fx:83), not at the 12 m end; 4->6 s (M12: 6); relay reach 4 m literal (Id:136).
- Per-modifier range/width/targets/prepare (10/9/12, .6/.7/.8, 3/3/5, .5/.45/.45) are read at Sk:130,159 (prepare, before the cast exists), Au:37,41,50,57, Po:55, Fx:147: overlay `N()` (Sk:28). Element literals -> `a.element`.

### M01 Fireball (T2 core, legacy idx 12, SD4:3043)
- Now: GC:479 cost 25, coef 2.1, range 10, r 2.5 (CS:598). The projectile (Proj:35-36) explodes on the first enemy, range end or wall (Proj:79,95); `FireballImpact` (Proj:102-112) hits everyone in the circle as "M01" (+MP01 +30% at >=3, dropped; LM02 echo Proj:110; SM4 frost mark). D210->294 = the rank +10% already; cost 25->21 needs a rank term in `Cost()`.
- Planners: `PlanEdictFireball` EdF:70-102 (GROUP = >=3 in the circle), `ForecastEdictFireball` EdF:29-69 (speed 14, first collision), Fc:89-98, `ObservedProjectileAim` (speed 14, skill 12 only, Fc:103-123).
- M01-1 Meteor **B**: impact after 1 s at the aim, no flight. Reuse a projectile with `delay` (Proj:56-58) and `remaining=0` (impact Proj:95), or `ScheduleExtra` (Cs:111-114, but `Set` damage). Radius x1.5 at Proj:36, EdF:66, Fc:95; forecasts need the delay and no first-collision. Spec conflict: x1.4->1.8 vs behaviour "area x1.5, damage x1.6".
- M01-2 Splitting **B**: 3 small shots from the impact. `Shot()` takes an action and sets origin (Proj:11-16); `extra` shots are hard-labelled "SAB4"/`Set` (Proj:86): label "EXTRA:M01" (also avoids base-rank scaling, Dmg:66). D40->60 from the modifier.
- M01-3 Piercing **C**: pierce is B (`explosionRadius=0`, large `maxHits`, Proj:80-90), but the 2->4 s burn trail has no host: kind 12 ground is one-shot (Sim:595) and `InFirewall` is one rectangle bound to `Fx("M08")` (Fx:164). Needs a trail-segment effect; planners need a line forecast (`ForecastLine`, Fc:28) instead of the circle.

### M02 Blizzard (T2 core, legacy idx 13, SD4:3138)
- Now: GC:480 cd 6, cost 30, range 10, r 3, 6 s, D65/s. `AddGround` kind 13 (Sim:545-554; cap 2 own, Sim:548-549); tick .5 s `Hit(..,2,Periodic)` (Ar:146-150); slow `SlowStrength` .35 (+MP02, Sim:282; Ar:158). Planner EdB:45-77 (radius literals EdB:36,41,63; `GroundRadius` Ar:12), Fc:95. Duration 6->8 has no rank term (Sim:334).
- M02-1 Trailing **B**: a `fx.position=State.position` branch beside the `followsTarget` code (Ar:135-136; that one follows the TARGET up to 4 m, needs LM01 and option FOLLOW, HuntEdictV2.Availability.cs:70). Radius 2.5->3.5 vs planner literal 3; KEEP/KEEP_TARGET logic assumes fixed zones (EdB:53-55,67). Plan:262 asks for a movement-rule check.
- M02-2 Deep Freeze **A\*** slow, **C** freeze: `SlowStrength` literal .35 (MP02 = same idea, dropped) -> modifier 50->60 (behaviour text: 35->60); bosses read it (Eff:131-147). "Freeze after lingering 3 s" needs a per-enemy dwell timer; only `enemy.exposure` exists (SM-set gated, Ar:82-106).
- M02-3 Returning Frost **B** (absorbs M10): call the M10 body (Fx:151, `TickReturningGlobe` Id:91-117, walk-back Sim:368, Id:118-132) from the legacy Blizzard case instead of `AddGround`; pass `action.snapshot`; rename M10 keys (Id:121, Po:65, Au:66,134, Sk:136); `PlanEdictBlizzard` plans clusters, not a line. D70->110 vs M10 D90 per leg.

### M03 Chain Lightning (T2 core, legacy idx 14, SD4:3233)
- Now: GC:481 cd 2, cost 25, coef 1.1, range 10; Sim:335-350. Hop limit `ChainHitLimit` (Fc:10: 4 + SMB4 2 + rune targets; MP03 dropped) is shared by release and forecast (Fc:74-82); next hop <=4 m (Fc:13-16; LM04 revisit); conduit relay Id:133-145; hit memory Ps:147; orb charge Ps:54. Planner EdC:8-34. Cost 25->21: rank term.
- M03-1 Overload **A\***: limit literal 4 -> modifier 6..8 absolute; decay literal `.8f` (Sim:345) "relaxed" is unspecified; forecast follows (same function).
- M03-2 Rebound **B**: a lone enemy is re-hit 2->4 times (the hero is only a visual node). Generalise LM04's revisit (`count<2`, Fc:14; Sim:345). `PlanEdictChain` needs 2 unique targets unless SINGLE (default) or SAVE_CHARGE (EdC:28-30); forecast `hits` must repeat ids (`TotalHits`).
- M03-3 Magnetic **B** (absorbs M16): after the loop `Pull()` (Fx:17-36) the linked enemies toward the first target 1->2 m (bosses: .5 s stun). Pulling inside the loop changes `NextChainTarget` and desyncs the forecast. M16's vortex DoT (Fx:222) is not in the spec text.

### M06 Frost Nova (T3, legacy idx 17, SD4:3328)
- Now: GC:484 cd 10, cost 20, r 3; Sim:319-323 `AreaHit(3*ClassArea,coef 1,cold)` then Freeze 1.5 s literal (spec 1.5->1.9: rank term). Planner EdN: consts `NovaRadius` 3, `NovaPreparation` .35 (EdN:10); `NovaNewControl` assumes freeze (EdN:11-13,35-41); Fc:87,95; `MageActiveShort` index 17 (EdMB:15).
- M06-1 Double Nova **B**: 2nd wave at +1 s via "burst" (Fx:233) / `ScheduleExtra` (Cs:111-114; position = target ?? `a.aim`, needs the hero); `Set` damage 50->100% so no passives; freeze on wave 2 unspecified.
- M06-2 Rift Ward **B** (absorbs M14): skip the freeze loop (Sim:323), `Placement(..,"ward")` with M14 numbers; `InWard()` Fx:166 and `ClassGuard` literal `.1f` (Ps:185) -> modifier -10->-20%. `Fx("M14")` -> filter by kind (`Fx` ignores kind, Sk:34). The CONTROL purpose still believes the nova freezes (EdN:35-41).
- M06-3 Ice Armor **B**: `AreaHit` returns the hit count (Sim:320) -> `AddShield("M06",hp*min(n*pct,.2),dur,root)` (Eff:22-37: capped by max HP minus existing, x`shieldMultiplier`). Duration unspecified; the 20% cap is only in behaviourKo.

### M04 Teleport (T3 mobility, legacy idx 15, SD4:3423)
- Now: GC:482 cd 9, cost 0, range 8; Sim:298-311 moves, then `ClassLanding` (Ps:57-69, no M04 hook). Escape role `EdictSurvivalSkill` (EdR:146-152), `PlanEdictTeleport` EdT:27-52, `EdictEscapePoint` (R/CombatSimulation.EdictMovement.cs:60-81; scores SAFE/SPARSE/DISTANCE), `TeleportLandingValid` EdT:10-15. Cooldown 9->7.2 = spec.
- M04-1 Flame Afterburn **B**: `AreaHit(origin,r,coef,1)` before `State.position=destination` (Sim:299). A "M04" label takes base rank (Dmg:66), the spec says modifier rank. Radius unspecified.
- M04-2 Lightning Blink **B**: after landing `Nearby(dest,r,3)` + `Hit(..,3)` + `ApplyStatus(Stun)` (Eff:95-115; bosses get control meter only). Damage not in spec. Trap: `EdictEscapePoint` picks quiet landings, so the strike rarely finds targets.
- M04-3 Elemental Shield **B** (absorbs M05): `AddShield` on arrival, 25->35%, 4 s. M05 literals: reaction (Id:72-90), LM03 mana (Eff:40), REF_SM01 +30% (Eff:25), `OwnElementalShield`/`ElementalShieldAmount` .35 (EdS:9-10, R/CombatSimulation.EdictOrder.cs:25,34), `ObserveShieldOpeningAction` (EdS:33-44), `BuffSkill` (Ord:49), Sim:98, survival `defenseSkill` (EdR:158-174). Keep the `lastElement` write (A).

### M08 Firewall (T4 zone, native, SD4:3507)
- Now: CS:2912 cd 8, cost 25, range 8, coef .6, 4 s, targets 5, width 1, length 5, ticks 4, prepare .4 (RP:332). Fx:148 `Placement(..,"firewall")` at `a.aim`; shape = `InFirewall` (Fx:164: first `Fx("M08")`, `N("M08","length")`); tick Fx:220. Shape users: burn flag Ps:97, DES_LM41 Fx:147, `ClassAimContains` Au:89-96; auto Au:132 (target in own blizzard, or an enemy within 3 m). Duration 4->5: **A**.
- M08-1 Ring **B**: `InFirewall` -> circle around the hero (2.5->3.5; behaviour 3 m), position `a.origin`, same in `ClassAimContains`.
- M08-2 Advancing **B**: move `f.position` along `f.direction` in the tick (Fx:220), 1.5->2.5 m/s (behaviour 2), clip with `Map.MoveDirect`.
- M08-3 Cross **B**: also test the perpendicular rectangle; duration 2->3 s (shorter than the 4->5 base). `Placement` keeps one effect per id+kind (Fx:82).

### M11 Capacitor Orb (T4 zone, native, SD4:3602)
- Now: CS:3142 cd 10, cost 25, range 8, coef .3 per tick, 6 s, expiry .8 + .3/charge (max 3, +1 per M03 cast, Ps:54), r 3, 3 targets literal (Fx:221), expiry splash Fx:174-176, aim ring Au:67-74, cast needs an elite + M03 equipped (Au:135). Expiry D80->120 vs code x1.4 = 112: from/to.
- M11-1 Rolling **B**: move `f.position` per tick; "path damage" D40->60 is a new coefficient (the tick already hits r 3).
- M11-2 Triple **C**: one effect per id+kind (Fx:82) and every `Fx("M11")` user assumes one orb (Ps:54, Id:144, Ps:151, Au:135). Needs instance ids, a cap and an expiry rule (Plan:264 undecided); which orb takes M03 charges; blast per orb?
- M11-3 Overcharge **A\***: `period` 0 (ticks stop, Fx:202) + expiry coefficient from the modifier 200->300.

### M17 Triune Collapse (T5, native, SD4:3697)
- Now: CS:3573 cd 60, range 10, r 4, coef 2 (fire), secondary 1.5 (cold + freeze 1 s, then lightning), period .5, ticks 3, prepare .5. Fx:160; stages Fx:228-230 (`SkillHit(..,f.count)`: stage number = element; literal r 4 / 8 targets at the fixed `f.position`). Stage 3: MP18 +50 mana (dropped), DES_LM46. Auto Au:141-143 waits for `elementBuff` when MP06 is learned; aim Au:76-84.
- Ultimate rank: `RankScale` fixed 1 (Cat:65); spec D200->280 = x1.4 at rank 3; cooldown 60/55/50 (shared timer, floor 45).
- M17-1 Single Element Focus **B**: every stage uses `CS.lastElement`, +20->30%. 0 = none needs a fallback; the cold stage's freeze (Fx:229) loses its identity.
- M17-2 Tracking Collapse **B**: per stage set `f.position` to the densest point within 6->10 m (reuse Au:76-84 + `ClassAimContains`).

### M18 Sage Incarnate (T5, native, SD4:3775)
- Now: CS:3643 cd 60, 8 s, barrier .25 (+MP19, dropped), damageBonus .2, costReduction .2, cycleLimit 2, r 2.5, 5 targets. Fx:161; damage literal `.2f` (Ps:114, any damage kind of a normal skill), cost -.2 literal (Ps:89), 3-element burst D40% per element (Ps:41-42,154-155); auto Au:144 needs 3 elements equipped (static `ElementOf`); `BuffSkill` (Ord:49), upkeep (R/CombatSimulation.EdictOrder.cs:28-43).
- Spec shield 25->35% = x1.4 once `Barrier` applies rankScale (Fx:161 does not); damage 20->30 vs x1.4 = 28: from/to. The spec base lists neither the -20% cost nor the burst.
- M18-1 Elemental Surge **C** (B if approximated): a free follow-up cast of another equipped core skill at 40->60%. The 40% equals today's burst coefficient (Ps:155), so M18-1 may be that burst reworked (extend `Cycle`/`elementCharge`). A real copy of M01/M02/M03 needs a synthetic `HeroActionState` (`AreaHit` defaults definition and root to `State.heroAction`, Sim:491), no `CaptureClassCast` (else MP11/MP16/cycle/legendaries count it), and recursion blocked.
- M18-2 Elemental Avatar **B**: switch on `CS.lastElement`: fire damage (Ps:114), cold guard (`ClassGuard` Ps:175-190, cap .5), lightning cooldown: add to the cast-time cdr (Sk:162, Act:73, Cs:37) or speed the three decrement sites (Sk:205-206, Sim:234).

### Absorbed and dropped legacy skills
- M05 (to M04-3): Sim:324; CS:767 cd 14, 4 s, 35% x rank (SE:31); reaction r 3, burn .6, root 1, +8 mana (RP:630).
- M09 (to M07-2): Fx:149-150; CS:2982 cd 4, cost 18, range 9, coef 1.3, slow .35/3 s, freeze .8 (RP:345).
- M10 (to M02-3): Fx:151, Id:91-132; CS:3052 cd 6, cost 25, range 8, coef .9 per leg, speed 5, flight 3.5, freeze .75, pickup .7, +12 mana (RP:358).
- M12 (to M07-3): Fx:153, Id:133-145, Ps:148-152; CS:3216 cd 5, cost 22, range 12, coef .9, 6 s, +4 mana (RP:390); one relay and one refund per chain (CAT:4187).
- M14 (to M06-2): Fx:155,166, Ps:185; CS:3353 cd 16, cost 10, r 3, 6 s, barrier .1/4 s, guard .1 (RP:417).
- M16 (to M03-3): Fx:157-159,222; CS:3503 cd 12, cost 20, r 3, coef .3/s, 4 s, pull 1.5 (RP:447).
- Dropped: M13 (channel: `CSStationary` Sk:26, Po:201-220, intent whitelist Pers:42,47-48, option group `M13:goal`) and M15 (Ps:35-36,85-88, Au:139).
- Absorbed natives lose effects every tick unless equipped (Sk:239; M05 is legacy, not purged, Sk:240): modifier effects must carry the base id. Gear on absorbed ids applies only with the modifier (Plan:430): DES_LM41-46 at Fx:147,158,212,230,252; Id:106,144; Ps:151,155; Po:217.
- 9 of 10 Mage validation builds use absorbed or dropped ids (all but BUILD_REF_SM02; CS validationBuilds 16-23,28,29).

## C. Passives and the two key passives

| Id | Applied today | x(1+.1(r-1)) vs SD4 | State |
|---|---|---|---|
| MP05 | Eco:182 (regen x(1+`PassiveBase[16]`xMagnitude), SE:14,23); on via `Stats.passives[4]` (Eco:129-130) | 20->28 = spec; forge +1 rank (R/HeroStats.Blacksmith.cs:17) would give 30 | none |
| MP14 | Ps:139-143 (2nd BASIC direct hit on any target, 4 s charge, 3 s pause), reduce Ps:85-86, consume Ps:33-34 | .3->.42 = spec (total cost cut capped 50%) | `Counter("MP14:count")` + charge effect exist |
| MP17 | Ps:112, `reserve` Ps:22 | .2->.28 = spec; first direct hit of the cast only (`firstHit` Ps:157) | exists |
| MP06 | Passives.cs:36-45, Sim:494 | 10->14 = spec | `elementHitTicks`, `elementBuff` exist (A) |
| MP07 | Ps:129 | 12->16.8, spec 20: from/to | "burning" = `HasDot("M07")`/`InFirewall` only (Ps:97): add M01-3 trail, M04-1 blast |
| MP09 | Ps:130 | 12->16.8, spec 20: from/to | `Controlled()` counts Slow (Atr:27-29) |
| MP11 | Ps:29-30,38-39 | .3->.42 = spec | counts any lightning cast (M03, M11, M07-3); consumed by M03 and M12 (spec: M03 only) |
| MP16 | Ps:40 | .08->.112, spec 12: from/to | `Cycle` = any order within 6 s, not "in a row" |

- Data-driven from/to needed for MP07, MP09, MP16 and both key passives (+5 and +10 per rank); the rest equal x1.4 by coincidence. Readers to change: `PV` (Sk:31), `SE.Passive` (SE:23). SD4 unlock levels differ from CS (MP05 2, MP14 4, MP17 6, MP06 8, MP07/09/11 18, MP16 30, keys 45).
- Dropped MP01-MP04 branches (Proj:107, Sim:282, Fc:10, Eco:191) turn dead, but `PassiveBase` indices 0-3 must stay (SE:14). Key passives are not in data: `ClassSkills.Data` hard-checks 111 skills and unlock <=40 (Cat:52-56).

**MP20 Shatter / 산산조각** (SD4:4019, value 30->50 "%", unlock 45): effectKo `빙결이 풀릴 때 그동안 받은 피해의 30% → 50%로 폭발`; effectEn `Frozen enemies shatter for 30% → 50% of damage taken`.
**MP21 Align the Elements / 원소 정렬** (SD4:4039, value 60->100 "D%", unlock 45): effectKo `세 원소를 모두 맞은 적에게 D60% → D100% 붕괴 폭발`; effectEn `Enemies hit by all three elements burst for D60% → D100%`.

```
// MP20: new EnemyState.shatter (float; old saves read 0). In ApplyOutgoing after Deal (Dmg:49):
if(Passive("MP20")&&CombatEffects.Has(enemy,StatusKind.Freeze)&&!definition.StartsWith("EXTRA:MP2"))enemy.shatter+=damage.hpLoss;
// TickStatuses, after the remaining-=dt loop, before RemoveAll (Eff:163-164):
if(enemy.shatter>0&&!CombatEffects.Has(enemy,StatusKind.Freeze)){float taken=enemy.shatter;enemy.shatter=0;
  if(Passive("MP20")&&!enemy.dead)foreach(var e in Nearby(enemy.position,2.5f,5))
    ApplyOutgoing(e,taken*Value("MP20"),0,1,1,false,2,CaptureDamage(),"EXTRA:MP20",0,0,DamageKind.Set,attackResolved:true);}
// clear in ResolveCombatDeaths (Outcomes.cs:15) and ResetUnavailablePassives (Passives.cs:46); Value() = from/to accessor, not PV
// MP21: new EnemyState.elements (int mask 1 fire, 2 cold, 4 lightning). In ApplyOutgoing beside RecordElementHit (Dmg:50), same gate:
if(Passive("MP21")&&element>=1&&element<=3&&!enemy.dead){enemy.elements|=1<<(element-1);
  if(enemy.elements==7){enemy.elements=0;foreach(var e in Nearby(enemy.position,2.5f,5))
    ApplyOutgoing(e,snapshot.damage*Value("MP21"),0,1,1,false,0,snapshot,"EXTRA:MP21",root,0,DamageKind.Set,attackResolved:true);}}
```
- `EXTRA:` ids are non-native (Sk:89): no recursion, no element recording. DoT ticks (Fx:73) and Blizzard ticks do feed the mask. Bosses never get a Freeze status (Eff:98-103): Shatter needs `bossControl.staggered` (Eff:155-158) if bosses should shatter. By the same gate, modifier secondary hits (`EXTRA:`) never count for MP06/MP21 (E11).

## D. Test patterns

- Fixture `Fixture(skill,passive,equipment)` (RT:23-41): Mage account for M** ids, level 40, ultimate M17 (RT:28), one 100000-HP immobile enemy 2 m ahead, `decisionTime=100000` (manual casts only), regen 0, mana 50/100 (mana deltas come only from skills), seed 42. **`Stats.passives=new bool[6]` (RT:38) switches legacy MP05/MP06 off**: set `sim.Stats.passives[4|5]=true`, or use the legacy Mage fixture T/PassiveTests.cs:21-33 (`Hit` helper :35; asserts `itemEffects.elementBuff/elementHitTicks`, :160-191).
- Helpers: `Advance` RT:42, `Target` :43, `CastSkill` :44 (sets mana 100), `AddSkill` :49, `Use` PT:24 (works for legacy ids: `Use(s,"M03",.8f)` runs Sim:335-350), `ExtraEnemy` GT:12, `Status` PT:23, `Begin`/`Land` PT:12-22 (they read the static `ElementOf`: a modifier test needs a renamed helper; `Call` does `Single(name)`, RT:19, so never add overloads of private methods).
- Assertions: `State.damageEvents` (`definitionId`, `kind`, `element`, `finalDamage`; `EXTRA:M11` = secondary/Set); `classSkills.lastElement=n` set directly (Identity:91); statuses `Target(s).statuses.Any(x=>x.kind==StatusKind.Freeze&&x.definitionId=="M06")` (Identity:95) or `CombatEffects.Has`; `classSkills.effects`/`.dots`; legacy ground `State.effects` kind 13 (SRT:101); `State.shields` (hp 1000, `shieldMultiplier` 1); `State.resource` and RESOURCE `effectEvents`; forecast parity `ForecastAttack(new Rule(14),first).hits` vs the M03 events (Identity:111-114).
- Determinism: `Hit` always draws `State.rng` (Dmg:70), REF_SM07 rolls at capture (Cs:55): keep crit 0, compare equal seeds.
- Will break or need rewriting: RT:16,54-66 (id lists, counts), RT:110 (M13), Identity:88-150 (M05/M10/M12 and presets), SkillPresetCombatPreviewTests.cs:105-115 (M05 + `lastElement` 3), PT:11,36-96 (MP08/10/12/13/15/18/19 cases), GT:61 (MP12), R/ClassSkillNativeSmoke.cs:47-66 (M05/M10/M12/M13 checkpoints).

```csharp
// sketch, not run; modifier index 0-based, -1 = none; r.modifier/r.modifierRank are the new v5 fields
(int ticks,float blast) Orb(int modifier){
  var s=Fixture("M11");var r=s.State.build.classSkills.ranks.Single(x=>x.id=="M11");r.modifier=modifier;r.modifierRank=modifier<0?0:1;
  Assert.IsTrue(CastSkill(s,"M11"));Advance(s,6.6f);var d=s.State.damageEvents;
  return (d.Count(x=>x.definitionId=="M11"&&x.kind==DamageKind.Periodic),d.Where(x=>x.definitionId=="EXTRA:M11").Sum(x=>x.finalDamage));
}
[Test] public void OverchargeDropsTheTicksAndPaysOnExpiry(){var plain=Orb(-1);var over=Orb(2);Assert.Greater(plain.ticks,0);Assert.AreEqual(0,over.ticks);Assert.Greater(over.blast,plain.blast*2);}
```
Legacy-path variant (M03-1, untested): `Fixture("M03")`, modifier 0, eight `ExtraEnemy` 1.5 m apart, `Use(s,"M03",.8f)`: 4 `M03` events without, 6+ with, equal to the forecast hit count.

## E. Open questions (not resolvable from code)

1. Spec text conflicts (Plan:239: effectKo/value win): M01-1 x1.4->1.8 vs "area x1.5, damage x1.6"; M02-2 slow 50->60 vs 35->60; M03-1 6->8 vs 4->8; M03-3 pull 1->2 m vs 1.5; M04-2 stun .3->.7 vs .5 s; M04-3 25->35% vs 35%; M06-3 2->4% per enemy (20% cap only in behaviour); M08-1 radius 2.5->3.5 vs 3 m; M08-2 1.5->2.5 vs 2 m/s; M08-3 2->3 s vs base 4->5; M11-2 2->3 s vs "half"; M18-1 40->60 vs 60%.
2. M07: cost 18 / cd 3 / unlock 22 today vs a T1 generator whose only number is +6->10 mana on hit (per hit or per cast; cost kept?); per-modifier range/width/targets (10/9/12, 3/3/5); where the spear is planted (aim or 12 m end).
3. "D%" units on modifiers: M07-1 burn D60->100 over how long; M01-2 per small fireball, reach, radius; M02-3 round trip total or per leg; M04-1/-2 radius, lightning damage (M04 has no damage line); M06-1 second wave basis, freeze?; M11-1 per tick or per pass.
4. M01-1 does the meteor follow the target; M01-3 trail shape and tick rate; M03-1 decay value; M03-2 decay x.8 per re-hit, can it hit the hero.
5. M06-2 does the nova still deal damage, ward duration (M14: 6 s), M14's 10% barrier kept; M06-3 shield duration; M06-1 centre (hero now or at +1 s).
6. M04-3 keep M05's last-element reaction (then the `lastElement` writer stays)? Which skill is the survival defense role now? Keep M04's +5% range per rank?
7. M08-1 disc or ring, follows the hero?; M08-3 centre; M11-2 orbs per cast, blast per orb, which takes M03 charges; M11-3 keeps +D30% per charge?
8. M17-1 fallback when `lastElement`=0, does stage 2 still freeze, +20->30% additive or multiplicative; M17-2 range from the hero or the previous stage; MP18 (+50 mana) is gone.
9. M18: the -20% cost and the 3-element burst exist in code, not in the spec (keep?). M18-1: "core skill" = M01/M02/M03 only? cost and cooldown of the free cast, does it count for MP11/MP16/legendaries, cap per duration. M18-2: stacking with the base +20->30%, cooldown as speed or reduction.
10. MP20: radius, target cap, burst element, damage basis (HP lost or incl. overkill), death while frozen, bosses (stagger), may bursts feed other frozen enemies. MP21: window or reset, once per enemy or per completion, radius, element, do DoT, basic-attack (lightning) and `EXTRA:` hits count.
11. Element of a modifier hit (M04-1 fire from a physical cast): MP06/MP21 read the hit element, MP11/MP16/M18 and `lastElement` the cast element. MP07 "burning" and MP09 "controlled" definitions (Slow counts today, Atr:27).
12. MP07/MP09 12->20 and MP16 8->12 are not x1.4 (confirm from/to); MP16 "in a row" vs any order.
13. Legacy rank tables (cost 25->21, Blizzard 6->8, Nova 1.5->1.9, M04 range) live in GC/SE/Sim, which are fingerprinted (X/class_skills.py:11-22) and hashed by Balance1000 (X/balance_1000.py:222-231); ultimate numbers (M17 60/55/50, M18 shield 25/30/35, damage 20/25/30) need tables (Plan:137).

**Key file.** /Users/t8g-2410-pn-005/Documents/MyProject/HELLSCRIPT/wt-d4-skill-plan/Assets/HELLSCRIPT/Runtime/Core/CombatSimulation.ClassSkillEffects.cs