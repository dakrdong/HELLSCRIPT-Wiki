# Ranger research for the D4 skill-tree migration (read-only)

All paths are relative to `/Users/t8g-2410-pn-005/Documents/MyProject/HELLSCRIPT/wt-d4-skill-plan`. Code is in `Assets/HELLSCRIPT/Runtime/Core/` unless noted.

Short names:
- `Skills`=ClassSkills.cs.
- `Cast`,`Fx`,`Auto`,`Pas`,`Pol`,`Idn`,`Proj`,`Dmg`,`Act`,`Chg`,`Fc`,`Aim` = CombatSimulation.ClassSkills / ClassSkillEffects / ClassSkillAuto / ClassSkillPassives / ClassSkillPolicies / SkillIdentity / Projectiles / Damage / Actions / Charges / Forecast / EdictAim .cs.
- `Sim`=CombatSimulation.cs.
- `RgrT/M/S`=CombatSimulation.EdictRangerTrap/Movement/Support.cs.
- `Opt`=ClassSkillOptions.cs; `SE`=SkillEffects.cs; `Pers`=ClassSkillPersistence.cs; `Stt`=ClassSkillState.cs.
- A bare `Enemies.cs`, `Outcomes.cs`, `Rules.cs`, `ClassSets.cs`, `Passives.cs`, `Attributes.cs`, `Legendaries.cs`, `Monsters.cs`, `Effects.cs`, `EdictOrder.cs`, `EdictField.cs`, `EdictMovement.cs`, `EdictRanger.cs`, `HuntEdictChanges.cs` means CombatSimulation.<name>.
- Tests (`Assets/HELLSCRIPT/Tests/Editor/`): `RT`=ClassSkillRuntimeTests.cs, `PT`=ClassSkillPassiveTests.cs, `GT`=ClassSkillGearTests.cs, `AT`=ActionTests.cs, `SRT`=SkillRankEffectTests.cs.
- JSON keys are non-zero only: `cd`=cooldownSeconds, `cost`=resourceCost, `coef`=coefficient. A01-A06 spell rangeMeters/radiusMeters/durationSeconds.

## A. Framework

A01-A06/AP01-AP06 are legacy-index skills (`legacy:true`, id<->index `Skills:66-68`). A07-A18 are class-cast skills.

1. **Definition.**
   - `ClassSkillDefinition` {id, category active|passive|ultimate, heroClass, order, unlock, maxRank, element, legacy, `parameters[]`} (`Skills:9-23`).
   - `catalog.json` + `runtime_parameters.json` -> `tools/class_skill_runtime.py:8-18` -> `Resources/Data/ClassSkills.json`. It is hard-checked: 111 skills, unlock<=40 (`Skills:52-55`).
   - A01-A06 numbers mirror `GameCatalog.cs:473-478` (sha-pinned).
   - Keys the code never reads: A14 `splitTargets` (literal 2/3, `Fx:265`), A08 `markedDamageBonus` (literal .25, `Pas:109`), A17 `radius`/`targets` (literals, `Fx:224`).
2. **Loadout/ranks.**
   - `ClassSkillLoadout` {actives[4], ultimate, `ranks` List<SkillInvestment{id,rank}>, order, automatic, options} (`Skills:72-86`).
   - `Rank()` defaults to 1. `Canonical` accepts versions 1-4 (:99).
   - `ProjectLegacy` writes legacy `activeSkills/passives/skillRanks[12]` (:162-172). Sim reads the loadout as `Loadout` (`Cast:23`).
3. **Auto-cast.**
   - `TryAutomaticClassSkill` (`Cast:179-201`; callers `EdictOrder.cs:224`, `Sim:246`) runs per id in `Loadout.order`: `automatic` gate, `ClassAutoTarget` (`Auto:28-54`), `ClassAutoAim` (:55-88).
   - Legacy ids: `ClassAutoCondition` + `TryClassLegacyAutomatic` (Hunt-Edict rows -> `StartHeroAction`, `Auto:10-26`).
   - Others: `InspectClassSkill` (`Cast:116-140`) -> `TryCastClassSkill`.
   - `ClassAutoCondition` (`Pol:60-110`) is an option-rule switch, else `ClassAutoDefaultCondition` (`Auto:97-147`, legacy -> true).
4. **Cast, cooldown, resource.**
   - Legacy: `StartHeroAction` (`Act:49-82`; timing `CombatActions.cs:87-105`; cost :67; cooldown :73, reduced by `Stats.cdr` capped .4) -> `ResolveSkill` (`Sim:291-353`) -> projectile/trap/status. A mirror `ClassSkillCast` with rankScale 1 comes from `Cast:220-224`.
   - Class-cast: `TryClassSkillCore` (`Cast:146-166`) builds `ClassSkillCast` (:160), runs `CaptureClassCast` (`Pas:20-46`), pays cost (:161), and sets a timer in `CS.cooldowns` (:162-164; ultimates share one timer, >=45 s :163).
   - Then `ReleaseClassSkill` -> `ResolveClassSkill` switch (`Fx:93-163`) -> `ClassSkillEffect`s ticked by `Fx:167-271`.
   - Casts/effects are saved (`Stt:28-53`), pruned after 30 s (`Cast:218`) and validated (`Pers:37-40`). Resource gains go through `Resource()` (`Cast:76-80`).
5. **Passives.**
   - `Passive(id)`, `PV=N("value")*RankScale` (`Cast:30-31`); hooks in `Pas:20-191`.
   - AP01/05/06 are legacy: `Stats.passives[]` (`Economy.cs:129-131`) + `SE.Passive` (`SE:23`). Their ranks come from `build.skillRanks`, written at `Skills:168`.
6. **Rank scaling.**
   - `RankScale` = 0 (rank<=0) / 1 (ultimates) / 1+.1(r-1) (`Skills:65`). It is frozen in `cast.rankScale` (`Cast:160`) and applied in `SkillHit` (`Fx:12-13`).
   - Legacy skills: `runeSkillPower` +10/rank (`Sim:494`, `Dmg:66-67`), escape cooldown -5%/rank (`SE:28-30`), `MarkBonus`/`ShadowFraction` (`SE:34-35`). Cost never scales with rank.
7. **Use options.**
   - Counts: 55 groups / **166** choices (`tools/class_skill_options.py:10`; the brief's 164 is wrong). Ranger has 18 groups, 54 choices.
   - A choice changes only WHEN (rule switch `Pol:60-110`), WHO (`PolicyTargetEligible` `Pol:32-49`) or where the hero stands (`Pol:127-146`).
   - Elsewhere `UseChoice` only feeds reason text (`Cast:135,152`) and the M10 pickup walk (`Idn:121`). No effect code (`Fx:93-271`, `Proj`, `Sim:291-353`) reads it.
   - For legacy skills `patches` just set Hunt-Edict V2 fields (`Opt:62-73`; target/aim/trap-keep choices, `HuntEdictV2.Options.cs:70-94`). Even A02 "FOCUS" only picks the aim (`Aim:66-72`).
   - **It cannot carry a modifier as is.** Reasons:
     - `ClassSkillOptionSelection` has no rank (`Opt:21`).
     - There is a cap of 55 entries and default pruning (`Opt:46,55`).
     - Choices are freely swappable policy objects in presets (`Skills:130-131`).
     - The plan says options are not effect variants (`D4_Skill_Tree_Plan_20261012.md:90`) and stores `{id,rank,modifier,modifierRank}` per skill (:205-206).
   - Reuse only the `Selected(load,id)` pattern (`Opt:37-42`).
8. **Best extension point: `ClassSkillCast`**, fed by a keyed lookup `Loadout.Modifier(id)` beside `Rank()` (`Skills:84`). `SkillInvestment` (`Skills:72`) gains `modifier`,`modifierRank`.
   - Add `mod` (1-based, 0=none keeps old saves valid) and `modRank`/value to the cast, and fill them once in `CaptureClassCast` (`Pas:20`). Validate them next to `rankScale` (`Pers:37`).
   - Why:
     - Both cast creators funnel into it (`Cast:161`, `Cast:223`).
     - Every effect, projectile and trap holds a root id and finds its cast by `Cast(root)` (`Cast:54`, `Fx:184`, `CombatActions.cs:49-66`).
     - The cast is saved and already carries per-cast scalars (`additive`, `area`, `Pas:26-30`, `ClassSets.cs:42`). A later loadout edit therefore cannot change an effect in flight (cf. `SRT:99`).
     - `recommendedSkills` is a `Canonical` copy (`HuntEdictDefaults.cs:11-18`), so new serialised fields survive.
   - Use the live lookup only for pre-cast decisions. Effects that outlive the prune or run with a null cast (`Pas:124`) must copy numbers at creation, as `Placement` does (`Fx:84`).
   - **Call sites:**
     - Capture: `Cast:160-161,223`, `Pas:20-46`.
     - Cost/cooldown/readiness: `Pas:77-81`, `Sim:283-290`, `Act:73`, `Cast:116-140,162-164`.
     - Aim/forecast/auto: `Auto:55-147`, `Pol:50-58`, `Fc:52-67`, `Aim:21-22,44-77`, `RgrT:23-48`, `RgrM:10-32`, `RgrS:18-32`.
     - Effects: `Fx:93-271`, `Proj:22-34,51-148`, `Sim:291-353`, `Dmg:62-99`.
     - Triggers: `Pas:47-69,119-158`, `Idn:53-71`, `Outcomes.cs:8-25`.
     - Data: `Skills:97-133`.
   - Test safety: tests call private methods by reflection with positional args and `Single(name)` (`RT:17-22`, `PT:21`). Append optional parameters and add no overloads.

## B. Per skill (A = numbers only, B = new behaviour on an existing path, C = new mechanic/state)

**Cross-cutting risks**
- Absorbed ids (A08-A16) are never "equipped" in v5, yet code gates on it.
  - `ResetClassSkillEquipment` deletes effects/dots/shields of unequipped ids every tick (`Cast:237-239`).
  - `InspectClassSkill` returns NOT_EQUIPPED (:122), and `Reduce` also checks it (:72).
  - An active cast absent from the loadout is cancelled (`HuntEdictChanges.cs:46`) or throws on resume (`Pers:39-40`).
  - Modifier effects need new ids or an `Available()` map to parent+modifier.
- Secondary hits (`EXTRA:` ids, `SkillHit(secondary)`) are `DamageKind.Set`: no crit, not `Native`, so no passives/procs (`Fx:12`, `Pas:101,121`). This is the existing "not another skill" rule; keep it for forks, bounces, turret, echo.
- Design rules to preserve (`Docs/Design/ClassSkills/catalog.json`):
  - A07 is one cast, not three basic attacks (:3192).
  - A14 forks never return or re-split (:3601); A14-1 and A14-3 override this.
  - A15 shots trigger no procs (:3659).
  - A17 pulses are not A02 casts or A06 charged shots (:3774).
  - AP09 excludes splits, ballista and echoes (:5440). AP14 excludes the decoy explosion (:5740).
- Class-cast arrows call `Hit(...,procs:false)` (`Fx:13`), so A06 charges never touch A07/A14 today.
- Two arrow models:
  - Legacy `CombatProjectile` (`Proj:11-34`: speed 18, groups, deleted at walls :95).
  - `ClassSkillEffect kind="arrow"` (`Fx:86-92,241-271`: speed 20, radius .5, `limit` = pierce, ends at aim distance or wall).
  - Neither bounces. `ProjectileEnd` returns only an end point (`RiftNavigation.cs:140-145`).

**A07 Successive Shots** (class-cast). `Fx:133`; arrows `Fx:86-92`; flight `Fx:241-271`; MISS gate `Cast:170`; auto `Auto:119`.
- JSON (`runtime_parameters.json:164`): cost12 range10 coef.45 period.15 targets1 prepare.3 arrows3.
- Now: after .3 s, 3 arrows .15 s apart fly to the cast-time aim point (`Fx:89`). They hit the first enemy within .5 (`Fx:255`) as `Direct` "A07" x`rankScale`. The spec wants +8->12 resource per cast, but the code charges 12 and the auto rule keeps a 25% reserve (`Auto:119`).
- A07-1 Rapid Volley **[A]**: `arrows`/`coef` are read at `Fx:95,133`. Damage basis: E1.
- A07-2 Scatter Shot **[B]**: loop `Nearby(a.origin,range,n)` (`Cast:94`) with `Arrow(...,e.position)`, repeating targets if there are fewer. Release checks only the primary (`Cast:170`); each hit may feed A18 (`Idn:64`).
- A07-3 Finishing Arrow **[B]** + new Hit plumbing:
  - Pierce is the existing `Arrow(...,pierce)` (`Fx:86,255`).
  - Forced crit/crit-damage have no entry (`Dmg:70,73`; `SkillHit` has no such args `Fx:9-13`). This is shared with A04-3 and Precision.

**A14 Forking Arrow** (class-cast). `Fx:140`; fork `Fx:263-267`; `Auto:126`; `Pol:56`.
- JSON (`:254`): cd5 cost20 range10 radius3 coef1 targets3 prepare.4 secondary.5 splitTargets2 (unused).
- Now: the first arrow can crit. On a direct hit, 2 other enemies in radius get D50% "EXTRA:A14" arrows (3 with DES_LA44 on a poisoned victim). Forks have no crit or procs and never fork, because only `direct` arrows do (`Fx:258`).
- A14-1 Chain Split **[B]**: when the arrow is `secondary`, spawn generation two. Add a per-cast visited set (`Once`, `Cast:55-59`; `f.targets` is per arrow).
- A14-2 Explosive Arrow **[B]**: `Splash(a,pos,radius,power,0,secondary:true,source)` (`Fx:15-16`, as decoy expiry `Fx:178`). Use its own source id, because `Dmg:56` labels "EXTRA:A14" Split.
- A14-3 Returning Arrow **[C]**: arrows have no return leg. Copy the M10 boomerang (`Idn:91-117`: `count` leg, `targets.Clear()`, retarget).

**A01 Piercing Shot** (legacy). `GameCatalog.cs:473`; `Sim:326` -> `LaunchPierce` `Proj:22-27`; flight `Proj:51-101`; forecast `Fc:52-56`; auto-aim `Aim:44-77`.
- JSON: cost20 coef1.8 range10 radius.6 duration.65.
- Now: one projectile (speed 18, radius .3, maxHits 5 + AP03/rune, `Proj:24`) takes one shadow charge (`Proj:9-10`). SAB4 adds a delayed echo (:25-26). Walls delete it. Spec cost 20->16 needs a rank-scaled `Cost` (`Sim:283-290`).
- A01-1 Ricochet **[C]**:
  - No reflection code exists (only monster reflect, `Monsters.cs:83`), and there is no wall normal (obstacles are circles/boxes + floor polygons, `RiftLayout.cs:32-43`).
  - It needs normal probing, a saved bounce counter on `CombatProjectile` (`CombatActions.cs:49-58,110`) and a `hitIds` rule. Forecast cannot predict it.
- A01-2 Accelerating Pierce **[B]**: reuse LA01 growth (`Proj:85`: extra = k x `hitIds.Count` -> `Hit` extraBonus) and its tie-break `Aim:74`. Both gate on `Stats.specials` today.
- A01-3 Triple Pierce **[B]**: three `Shot`s at +-30 deg like `LaunchMulti` (`Proj:30-33`: group `maxPerVictim` 1, one shadow consume). Update `Fc:52-56` and `Aim:44-77`; mind SAB4 echo interplay.

**A02 Multishot** (legacy). `GameCatalog.cs:474`; `LaunchMulti` `Proj:28-34`; angle `Fc:20`; forecast `Fc:57-67`; `Aim:21-22`; stand point `RgrM:45-57`.
- JSON: cd3 cost30 coef.8 range8 radius8 duration.65.
- Now: 3 arrows at -30/0/+30 deg (speed 18, radius .2) share a `ProjectileGroup` (one hit per victim unless LA02, `Proj:31`). One shadow charge covers the volley (`Proj:30`; asserted `AT:84-93`).
- A02-1 Barrage **[B]**: 7 angles over 120 deg. `{-angle,0,angle}` is hard-coded in `Fc:60`, `Aim:22` and the FOCUS logic `Aim:66-77`.
- A02-2 Bouncing Arrows **[B]**: on hit, spawn a `Shot` to the nearest un-hit enemy using the SAB4 `extra` pattern (`Proj:26,86`: `Set`, no procs). Needs a "bounced" flag.
- A02-3 Focused Spread **[B]**: LA02's `narrow` path already does this (`Proj:30`: narrow half-angle, `maxPerVictim` 3). Re-key the `Stats.specials.Contains("LA02")` tests (`Proj:30`, `Fc:59`, `Aim:21,66`, `RgrM:47`) to the modifier. The same-target bonus comes from `victim.hits` (`Proj:84`).

**A03 Venom Trap** (legacy). `GameCatalog.cs:475`; `Sim:328` -> `CreateTrap` `Proj:113-119` (duration literal 5, radius 3/3.5, own-trap cap 2 :117); `TickTraps` `Proj:120-148`; planner `RgrT:23-48`.
- JSON: cd8 cost20 coef.6 range6 radius3 duration5.
- Now: arms .5 s; on contact roots 1.5 s (:133), then for the duration applies `Poisoned` and a .3 hit every .5 s (D60%/s, :144-145). The spec wants duration 5->7 (rank-scaled).
- A03-1 Spike Trap **[B]**: the legacy trap already roots on trigger. Add burst-hit + root 1->2 s and skip the poison loop (:133,139-146); no A09 port needed.
- A03-2 Frost Snare **[B]**:
  - Element 4->2 and `Poisoned`->`Slow` 30-50% (:144-145).
  - `OwnTrapPoison` (`CombatEffects.cs:74`) and the AP10/AP15/Viper poison source vanish.
  - A10 is a one-shot burst (`Fx:187-201`) but the spec says zone.
- A03-3 Crossbow Turret **[B]** effect, **[C]** placement:
  - Reuse `Placement(a,"ballista")` + `FireBallista` (`Fx:141-143`, `Idn:44-52`). They read A15 keys (ticks, radius, crossfire), so they need indirection.
  - The turret is a `ClassSkillEffect` (cap 1, `Fx:82`), not a `CombatTrap` (cap 2), so `LiveRangerTraps` users ignore it (`Pas:189`, `EdictField.cs:65`).
  - Auto placement uses trap-cluster logic (`RgrT`), not A15's firing-point search (`Auto:72-73`).

**A04 Retreat Leap** (legacy). `GameCatalog.cs:476`; travel `Act:95-102`; `BeginTravel` `Chg:75-86` (LA03 drops a trap at the origin :82-83); landing `Sim:298-311` -> `ClassLanding` (`Pas:57-69`, A04 block :63-68); planner `RgrM:10-32`; walk-back `RgrM:59-88`, `EdictRanger.cs:61`.
- JSON: cd8 range6 duration.5.
- Now: 0 s prepare and .5 s travel (`CombatActions.cs:99`) to a planner-chosen landing 2..`SkillRange(9)` m away with no enemy close (`RgrM:9`). Cooldown 8->6.4 already exists (`SE:28-30`); its +5% range/rank is not in the spec.
- A04-1 Decoy Projection **[B]**: A11 effect (`Fx:136`, enemy AI `Fx:272-279` via `Enemies.cs:189`, expiry splash `Fx:178`) spawned at `a.origin` from the LA03 hook. Taunt 2->3 s = `tauntSeconds` **[A]**. AP14 reads the literal `Fx("A11")` (`Pas:110`).
- A04-2 Smoke Cover **[B]**: `Buff("A12",3,guard)` (`Fx:137`) in the `ClassLanding` A04 block. `ClassGuard` lists A12 (`Pas:180`); its +20% speed (`Pas:170`) is not in the spec.
- A04-3 Shadow Step **[C]**:
  - There is no behind-target landing (`EdictEscapePoint` `EdictMovement.cs:60` is retreat-only; `RangerLandingValid` rejects points near enemies, `RgrM:9`).
  - `RememberRetreatLanding` would walk the hero away.
  - The next-hit crit buff shares A07-3 plumbing (charge-buff precedent `Pas:29-30`).

**A05 Hunter's Mark** (legacy). `GameCatalog.cs:477`; `Sim:329-331`; single own mark `Effects.cs:106-107`; value `Stats.vulnerable`=`MarkBonus` (`Economy.cs:192`, `Dmg:28`); planner `RgrS:18-32`.
- JSON: cd10 range10 duration10.
- Now: after .2 s prepare (`CombatActions.cs:97`) it applies a 10 s Mark to one target in range and sight. All hero damage to it gets +`vulnerable` (25% at rank 1), and recasting moves the mark.
- A05-1 Gaze Shot **[C]**:
  - Needs an aim phase after the legacy mark action. The plan says to stop auto-movement while aiming and cancel if interrupted (`D4_Skill_Tree_Plan_20261012.md:263`).
  - A08 is class-cast (`Fx:134`), and "stationary" is hard-coded to `CS.action.id=="A08"` (`Cast:26`; used `Cast:207`, `Sim:363`, `Pol:27-31,200`).
- A05-2 Hunter's Preparation **[B]**: the A16 effect (`Fx:144`) is reused, but a marked kill triggers it. Capture the mark before the hit (`Dmg:40-52` pattern; flag bit 1 `Pas:97`) and fire when `enemy.dead` (`Pas:119-158`; `Sim:508`).
- A05-3 Chain Mark **[C]**: needs a death hook + jump counter. `StatusEffect` has none (`CombatEffects.cs:11-18`) and `Counter()` effects never expire (`Fx:179`). `ApplyStatus` already moves the mark.

**A19 Elemental Imbuement** (A06 base, legacy). `GameCatalog.cs:478`; `Sim:332` (3 charges, 8 s), expiry `Sim:232`; consume `Proj:9-10` (basic :20, pierce :24, multi :30); apply `Dmg:82-97` (extra element-5 hit "A06", crit shared, LA04 contagion :86-95); state `GameCatalog.cs:411-414`.
- JSON: cd12 cost10 coef.25 duration8.
- Now: after .2 s prepare, 3 charges for 8 s. Each BASIC/A01/A02 shot spends one and adds an element-5 hit worth 25% (x rank) of that shot (`Dmg:85`).
- `LegacyId(11)`="A06" (`Skills:66`), so A19 needs a bridge. Literal "A06" sites: `Sim:280`, `Dmg:57`, `EdictOrder.cs:31-33`, `HuntEdictV2.Order.cs:49`, `HuntEdictV2.Options.cs:91-94`, `EdictOptions.cs:97`, `EdictValidation.cs:10,21`, `Rules.cs:21`, `ClassSets.cs:107,160`, `Itemization.cs:215`, HUD `GlobalHudSnapshot.cs:139`, `WorldView.Skills.cs:129-132`.
- A19-1 Shadow **[B]**: kill burst; precedent `EliteCascade` (`Dmg:121-129`).
- A19-2 Poison **[B]**: A13's `Dot` (`Fx:37-55`, sets `Poisoned` :53) needs a `ClassSkillCast` (`Cast(root)` exists for legacy via `Cast:223`) and a new dot id (unequipped ids are purged, `Cast:239`).
- A19-3 Cold **[C]**: per-enemy stack counter + `Freeze`. Bosses only gain control meter (`Effects.cs:100-104`).

**A17 Killing Rain** (ultimate). `Fx:145`; pulses `Fx:223-227` (`SkillPeriodic`, no crit, `Fx:239`).
- JSON (`:295`): cd60 range10 radius4 coef.8 duration2.5 period.5 targets8 prepare.35 ticks6.
- Now: it places a fixed 4 m rain at the aim (auto-aim picks the densest spot, `Auto:76-84`). Six pulses 0.5 s apart hit up to 8 enemies for D80%.
- `RankScale`=1 for ultimates (`Skills:65`). The plan needs 3 ranks, +20%/rank and 60/55/50 s (floor 45 `Cast:163`, `Pers:26`).
- A17-1 Moving Barrage **[B]**: `f.position=State.position` before `Nearby` (`Fx:224`). 8 waves need `ticks` 8 and duration 4 s (the loop is bounded by `until`, `Fx:203`).
- A17-2 Focused Barrage **[A]**: radius -> `f.radius` (literal 4, `Fx:224`), x coef, period .25. With `ticks` 6 the rain then ends at 1.5 s.

**A18 Afterimage March** (ultimate). `Fx:146`; `LeaveAfterimage` `Idn:53-60` (each tick `Cast:209`; anchor >=1.5 m, <=2 alive, `limit` 8); consumption `IdentityDirectHit` `Idn:61-71`; speed `Pas:170`.
- JSON (`:307`): cd60 coef.6 duration8 period1 prepare.2 speedBonus.2 procLimit8 trailMeters1.5.
- Now: afterimages are inert `ClassSkillEffect kind="afterimage"` markers. A direct hit by BASIC/A01/A02/A07/A08/A13/A14 consumes the oldest one with a clear <=10 m shot. That fires a D60% secondary shadow arrow (`Proc("A18",1,root)`).
- A18-1 Shadow Clone **[C]**:
  - No follower effect exists (nearest: decoy `Fx:136`, `returnGlobe`).
  - Add `kind="clone"` that follows the hero and mirrors direct hits in `IdentityDirectHit`. Secondary `Set` hits cannot re-trigger it (`Attributes.cs:77`).
  - Fix the id list at `Idn:64`.
- A18-2 Afterimage Burst **[B]**: expiry splash like orb/decoy (`Fx:174-178`). Echoes also vanish on consumption (`Idn:68`) and cap eviction (:57), so "fade" needs defining.

**Absorbed legacy skills** (all class-cast; effect code reusable)

| Legacy -> modifier | Impl | Params | Behaviour now |
|---|---|---|---|
| A08 -> A05-1 | `Fx:134`, `Pas:109`, `Auto:120` | cd5 cost30 range12 coef2.4 prepare.9 | .9 s still aim (move cancels), one D240% hit, +25% vs marked |
| A09 -> A03-1 | `Fx:135,187-201` | cd8 cost15 range6 radius2.5 coef.8 duration6 targets5 prepare.3 armSeconds.4 rootSeconds1.2 | one trap, arms .4 s, waits <=6 s, hits <=5 for D80% + root 1.2 s, vanishes |
| A10 -> A03-2 | same | cd10 cost15 coef.6 slowSeconds3 slowFraction.4 freezeSeconds.75 (+shared keys) | same trap, cold D60%, slow 40% 3 s, freeze .75 s if poisoned |
| A11 -> A04-1 | `Fx:136,178,272-279` | cd14 cost10 range7 radius2.5 coef.8 duration6 summonHealthFraction.1 tauntSeconds2.5 | decoy (10% max HP) pulls non-elites 2.5 s; expiry/destroy splash D80% |
| A12 -> A04-2 | `Fx:137-138`, `Pas:170,180` | cd14 duration3 prepare.2 guardFraction.2 speedBonus.2 | 3 s: -20% damage taken (guards cap .5, `Pas:190`), +20% speed |
| A13 -> A19-2 | `Fx:139,262` | cd4 cost15 range10 coef.8 duration4 period1 secondary1 prepare.4 | arrow D80% + 4 s poison bank D100% (larger bank kept) |
| A15 -> A03-3 | `Fx:141-143,218`, `Idn:39-52` | cd12 cost25 range6 radius9 coef.35 duration6 period1 ticks6 crossfireAngle60 crossfireCoefficient.45 shotWindow2 | 6 s ballista, 1 secondary shot/s at a perceived enemy <=9 m, +D45% on a crossfire angle >=60 deg |
| A16 -> A05-2 | `Fx:144`, `Pas:33-34,85-86` | cd18 duration6 prepare.2 resourceGain25 costReduction.2 | +25 resource; next paid active in 6 s -20% cost |

## C. Passives

| Id | Applied at | Param now -> spec fit |
|---|---|---|
| AP01 | `Dmg:32` (>=6 m, additive, snapshot rank) | `SE:14[6]`=30 x1.4=42, fits |
| AP05 | `Chg:18-45` + `Sim:285` | `SE:14[10]`=40 -> 56, fits. It reaches only legacy-index casts (`Act:67`); `ClassCost` ignores it (`Pas:77-81`), so A14 etc. get no discount |
| AP06 | `Dmg:69-70` (+crit vs HP<=30%, cap .75) | `SE:14[11]`=20 -> 28, fits |
| AP07 | `Pas:93` `ClassCritical` | .15 (`runtime_parameters.json:525`) -> .21, fits |
| AP09 | `Pas:138` (BASIC hit, 0.5 s `Proc`) | 4 (:531) -> code 5.6, spec 6 **no** |
| AP10 | `Pas:135` (direct kill of poisoned, 2 s) | 12 (:534) -> code 16.8, spec 20 **no** |
| AP14 | `Pas:110` literal `Fx("A11")` decoy <=3 m | .2 (:546) -> .28, fits; must read the A04-1 decoy |
| AP15 | `Pas:111` | .25 (:549) -> .35, fits |

- **What must change for spec numbers:**
  - Every rank formula is x(1+.1(r-1)) (`SE:23`, `Cast:31`).
  - AP09, AP10 and both key passives (AP20 30->50, AP21 10->18) therefore need a data-driven linear from->to in `PV` and `SE.Passive` (plan 5.4).
  - Stop the forge +1 rank on passives (`HeroStats.Blacksmith.cs:17`).
- **Other notes:**
  - Legacy passives are snapshotted (`Sim:494-495`) while `PV` reads live.
  - Dead code for removed passives remains (`Pas:28,52-53,71,171`, `Fx:146,226,259`, `Proj:24,133`, `Passives.cs:25`, `Fc:55`, `RgrT:43`).
  - Key passives' `unlockLevel` 45 trips `Skills:55`.

**Precision** copies the MP11 counter -> charge -> next-cast pattern (`Pas:29-30,38-39`):
```
// AfterClassHit, inside if(direct) (Pas:133)
if(damage.critical&&Passive("AP20")){var c=Counter("AP20");if(++c.count>=3){c.count=0;Buff("AP20:charge",30,PV("AP20"),root,"charge");}}
// CaptureClassCast, Normal(a.id) block (Pas:23-30)
if(IsCore(a.id)&&Fx("AP20:charge") is ClassSkillEffect p&&!p.consumed){p.consumed=true;a.forceCrit=true;a.additive+=PV("AP20");}
// Hit (Dmg:70): draw the RNG first, then critical = canCrit && (roll<cap || Cast(root)?.forceCrit==true)
```
**Viper's Blessing**
- `ApplyOutgoing` is the only choke point that sees hits, DoT ticks (`attackResolved`) and secondary hits.
- Add beside `DataDamageTaken` (`Dmg:44`): `if((classFlags&2)!=0&&Passive("AP21"))independent*=1+PV("AP21");`. The poison flag is captured before the hit (`Dmg:41`, `Pas:97`).
- `ClassDamageBonus` (`Pas:99-116`) is additive and normal-direct only, so it does not fit.

**Does the needed state already exist?**
- "Core" category: **no**. Categories are only active/passive/ultimate (`Skills:21-22`), the `Offensive()` list (`Cast:88`) differs, and tier exists only in the D4 JSON (Core = T2 = A01/A02).
- Crit counter: **no**. Only `DamageEvent.critical` and `LegendaryTrigger.Critical` exist (`Legendaries.cs:87-95`); storage helpers are `Counter` (`Cast:35-39`) and `Cycle` (`Pas:14-19`).
- Poisoned: **yes**. `StatusKind.Poisoned` (`CombatEffects.cs:8,66-73`); sources `Proj:144`, `Fx:53`, `Legendaries.cs:141`.

## D. Test patterns

- **Class-cast fixture** `RT:23-41`:
  - A validation-profile account (level 40), `ClassSkillLoadout.FromHero` + `ProjectLegacy`, and `new CombatSimulation(account,catalog,1,0,seed:42)`.
  - One 100 000-HP immobile enemy 2 m away, `Stats` neutralised (crit 0), and `decisionTime=100000` so only manual casts run.
  - Legacy ids cast through it too (`Cast:154-158`).
  - Helpers: `Advance` (`Tick(Step=.05)`, `RT:42`, `Sim:10`), `CastSkill` (`RT:44`), `Use` (`PT:24`), `AddSkill` (`RT:49`), `ExtraEnemy` (`GT:12`), `Begin`/`Land`/`Status` (`PT:12-23`, reflection into `CaptureClassCast`/`Hit`/`ApplyStatus`).
- **Legacy-path fixture** `AT:12-24`, `SRT:17-33` (`ContentTestAccounts.Legacy()`, fixed seed, `Advance` freezes decisions). It is used for projectile/trap counts and forecast vs release (`SRT:63-78`).
- **Reading results:** `State.damageEvents` (`definitionId`, `EXTRA:<src>`, `kind`, `critical`, `baseAttack`, `finalDamage`, `attackOrigin`, `rootCastId`; `Dmg:46-48`), `effectEvents`, `classSkills.{casts,effects,dots,cooldowns}`, `projectiles`, `traps`, `shields`, and the `ClassSkillChanged` stream (`Cast:10-19`).
- **Determinism:**
  - Fixed seed, crit 0, no decisions.
  - Twin fixtures with/without (`PT:91-96`; rank x1.4 `ClassSkillBoundaryTests.cs:51-60`).
  - Crit counting over 500 hits (`PT:58-61`, the pattern for Precision).
  - Resume equality via JSON round trip + `ClassSkillNativeReceipt.Verify` (`ClassSkillExperimentTests.cs:33-40`).
  - Tests that will break: counts 37/16/19 (`RT:54-65`), `NewActives`/`NewPassives` (`RT:16`, `PT:11`), AP cases (`PT:58-69`), `AT:84-93`.
- **Copyable body** (`GT:51-59`):
```
[Test] public void ForksLeaveTheOriginalVictimAndDoNotCopyVenomOrSplitAgain()
{
    var s=Fixture("A14",equipment:new[]{"DES_LA44"});var first=Target(s);
    for(int n=0;n<3;n++)ExtraEnemy(s,first.position+new Vector2((n-1)*1.5f,2));
    Use(s,"A13");Use(s,"A14",1);
    var forks=s.State.damageEvents.Where(d=>d.definitionId=="EXTRA:A14").ToArray();Assert.AreEqual(3,forks.Length);
    Assert.IsFalse(forks.Any(d=>d.targetId==first.id));Assert.AreEqual(1,s.State.classSkills.dots.Count(d=>d.id=="A13"));
}
```
A14-1 variant (sketch, not in the repo): helper `Forks(bool chain)` = same fixture, `SetModifier(s,"A14",chain?1:0)` (new helper writing the v5 row), four extra enemies, `Use(s,"A14",1.5f)`, return the `EXTRA:A14` count. Assert `Forks(true)>Forks(false)`.

## E. Open questions (not resolvable from code)

1. Does a modifier number (A07-1 "D25->33% per arrow") replace or multiply the base-rank scale (D45->63)? Plan 5.4 says base rank drives skill damage and modifier rank only modifier values.
2. Units "%" vs "D%": A14-1 second split 30->50% of what; A02-2 bounce 40->80%; A01-3 50->70% x3 (of weapon or of A01); is A07-3 "+30->70%" crit damage additive?
3. Resource model:
   - A07 +8->12 gain vs code cost 12 (and its 25% reserve rule).
   - A14 has no resource line (code cost 20).
   - Cooldown/cost of A03/A05/A19 are unlisted (assume unchanged).
   - A01/A02 cost scaling and A03 duration 5->7 need rank tables.
4. A19:
   - The id bridge to legacy index 11.
   - Element with no modifier (D8 covers only the Mage).
   - Whether A07/A14 arrows (`procs:false`) consume the 3 charges.
   - Whether all 7 Barrage arrows carry the extra (one charge per volley, `Proj:30-33`).
   - Which sources are excluded (forks, echo, turret, rain).
5. Precision: do forced crits (A07-3, Precision, Shadow Step) count; per hit or per cast; is Core exactly A01/A02? Viper: additive vs multiplicative; does it include poison ticks and secondary damage?
6. A03-2 frost zone vs A10 one-shot; trap/turret/decoy/clone caps (plan 8.5 undecided).
7. A05-1 cost (A08 30 vs A05 0), range 12 vs 10, cooldown. A05-2: keep +25 resource? A04-2: keep +20% speed? A04: keep +5% range/rank?
8. Geometry: is A02-3 "15 deg" a half or full angle? Pierce counts for A01-3 and A07-3; ricochet rules (`hitIds`, `maxHits`); A14-3 return target; A18-2 "fade"; A17-1 duration for 8 waves; A17-2 period vs 6 ticks.
9. Ultimate scale 1+.2(r-1) (rank 3 = 1.4, inside the `Pers:37` bound 1.401) and clamping 60/55/50 s to >=45 s.