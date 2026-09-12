using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Hellscript.Tests
{
    public sealed class IncomingForecastTests
    {
        GameCatalog catalog;
        [SetUp]public void Setup(){catalog=ScriptableObject.CreateInstance<GameCatalog>();catalog.Populate();}
        [TearDown]public void Teardown()=>UnityEngine.Object.DestroyImmediate(catalog);
        CombatSimulation Fixture(HeroClass hero=HeroClass.Mage,int level=30)
        {
            var account=ContentTestAccounts.Training();account.selectedHero=(int)hero;account.Hero.level=level;account.Hero.inventory.Clear();account.Hero.build.passives=Array.Empty<int>();account.Hero.build.rules.Clear();account.Hero.build.activeSkills.Clear();account.Hero.build.potionThreshold=0;
            var sim=new CombatSimulation(account,catalog,1,1,seed:211331,ownedTraining:true);sim.State.position=RiftMap.Rooms[0];sim.State.enemies.Clear();sim.Stats.armor=sim.Stats.resistance=sim.Stats.crit=sim.Stats.regen=0;sim.Stats.hp=sim.State.health=100;
            sim.State.enemies.Add(new EnemyState{id=900,position=sim.State.position+Vector2.up*6,health=1000000,maxHealth=1000000,speed=0,attack=0,cooldown=1000,brain=new EnemyBrain{initialized=true}});return sim;
        }
        static EnemyHazard Hazard(CombatSimulation sim,float damage,float delay=0,float duration=0,float interval=0,float tick=0)
        {
            var h=new EnemyHazard{id=sim.State.nextId++,actionId=sim.State.nextId++,enemyId=900,definitionId="TEST_HAZARD",position=sim.State.position,shape=AttackShape.Circle,radius=2,createdAt=-1,damage=damage,delay=delay,duration=duration,interval=interval,tick=tick};sim.State.enemyHazards.Add(h);return h;
        }
        static void Shield(CombatSimulation sim,float amount,float remaining,int id=1)
        {sim.State.shields.Add(new ShieldEffect{id=id,definitionId="TEST_SHIELD",amount=amount,remaining=remaining});sim.State.shield+=amount;sim.State.shieldTime=Mathf.Max(sim.State.shieldTime,remaining);}
        static void Advance(CombatSimulation sim,float seconds)
        {for(int n=0;n<Mathf.RoundToInt(seconds/CombatSimulation.Step);n++)sim.Tick(CombatSimulation.Step);}
        static HuntEdictV2Document Edict(CombatSimulation sim)=>HuntEdictV2.Create(sim.Hero.heroClass);
        static void Set(HuntEdictV2Document d,string id,string value)=>d.global.Single(o=>o.id==id).value=value;
        static float IncomingHp(CombatSimulation sim)=>sim.State.damageEvents.Where(e=>e.incoming).Sum(e=>e.hpLoss);
        static float Absorbed(CombatSimulation sim)=>sim.State.damageEvents.Where(e=>e.incoming).Sum(e=>e.absorbed);

        [Test]
        public void OverlappingAttacksSpendEachShieldOnlyOnceInTheActualAbsorptionOrder()
        {
            var sim=Fixture();Shield(sim,25,2,20);Shield(sim,15,1,10);Hazard(sim,30,.2f);Hazard(sim,30,.4f);
            string before=JsonUtility.ToJson(sim.State);var f=sim.ForecastIncoming(1);Assert.AreEqual(40,f.Absorbed,.0001f);Assert.AreEqual(20,f.HpLoss,.0001f);Assert.AreEqual(before,JsonUtility.ToJson(sim.State));
            Assert.AreEqual(0,f.hits[0].hpLoss,.0001f);Assert.AreEqual(20,f.hits[1].hpLoss,.0001f);Advance(sim,1);Assert.AreEqual(f.Absorbed,Absorbed(sim),.0001f);Assert.AreEqual(f.HpLoss,IncomingHp(sim),.0001f);
        }
        [TestCase(.45f,0)][TestCase(.5f,20)][TestCase(.55f,20)]
        public void ShieldExpiryIsCheckedAtTheDamageTick(float delay,float loss)
        {
            var sim=Fixture();Shield(sim,50,.5f);Hazard(sim,20,delay);var f=sim.ForecastIncoming(1);Assert.AreEqual(loss,f.HpLoss,.0001f);Advance(sim,1);Assert.AreEqual(f.HpLoss,IncomingHp(sim),.0001f);
        }
        [TestCase(0)][TestCase(1)][TestCase(2)][TestCase(5)]
        public void ForecastAndActualDamageShareArmorResistanceAndExpiringReductions(int element)
        {
            var sim=Fixture();sim.Stats.armor=80;sim.Stats.resistance=45;sim.State.itemEffects.leapDefense=.5f;sim.State.resolveShrineTime=.75f;
            foreach(float time in new[]{.25f,.5f,.75f,1f})Hazard(sim,10,time).element=element;
            var f=sim.ForecastIncoming(1);Advance(sim,1);Assert.AreEqual(f.HpLoss,IncomingHp(sim),.0002f);CollectionAssert.AreEqual(f.hits.Select(h=>h.element),sim.State.damageEvents.Where(e=>e.incoming).Select(e=>e.element));
            Assert.Less(f.hits[0].finalDamage,f.hits[1].finalDamage);Assert.Less(f.hits[1].finalDamage,f.hits[2].finalDamage);
        }
        [TestCase(1f)][TestCase(2f)][TestCase(2.5f)][TestCase(5f)]
        public void LongTimerBoundariesFollowTheSameRepeatedFixedStepsAsLiveDamage(float expiry)
        {
            var sim=Fixture();sim.State.itemEffects.leapDefense=expiry;sim.State.resolveShrineTime=expiry;Shield(sim,5,expiry);
            Hazard(sim,10,expiry);Hazard(sim,10,expiry+CombatSimulation.Step);var f=sim.ForecastIncoming(expiry+CombatSimulation.Step);
            Advance(sim,expiry+CombatSimulation.Step);var actual=sim.State.damageEvents.Where(e=>e.incoming).ToArray();
            Assert.AreEqual(2,actual.Length);Assert.AreEqual(actual.Sum(e=>e.hpLoss),f.HpLoss,.0001f);
            for(int n=0;n<2;n++){Assert.AreEqual(actual[n].finalDamage,f.hits[n].finalDamage,.0001f);Assert.AreEqual(actual[n].absorbed,f.hits[n].absorbed,.0001f);}
        }
        [TestCase(0f,.5f,2)][TestCase(.25f,.5f,1)][TestCase(.5f,.25f,2)]
        public void PeriodicHazardsUseRemainingDelayTickAndDuration(float delay,float interval,int hits)
        {
            var sim=Fixture();Hazard(sim,5,delay,1,interval,interval);var f=sim.ForecastIncoming(1);Assert.AreEqual(hits,f.hits.Count);Advance(sim,1);Assert.AreEqual(f.HpLoss,IncomingHp(sim),.0001f);
        }
        [TestCase(0f)][TestCase(.2f)][TestCase(.21f)]
        public void DelayedProjectileAlreadyOverlappingTheHeroWaitsForItsFirstMovementTick(float delay)
        {
            var sim=Fixture();Shield(sim,50,.225f);
            sim.State.projectiles.Add(new CombatProjectile{id=98,hostile=true,origin=sim.State.position,position=sim.State.position,direction=Vector2.up,radius=.2f,speed=10,remaining=9,delay=delay,damage=20,createdAt=-1});
            var f=sim.ForecastIncoming(.5f);Advance(sim,.5f);var actual=sim.State.damageEvents.Single(e=>e.incoming);
            Assert.AreEqual(actual.time,f.hits.Single().after,.0001f);Assert.AreEqual(actual.hpLoss,f.HpLoss,.0001f);
        }
        [TestCase(1)][TestCase(2)][TestCase(7)][TestCase(8)][TestCase((int)BossAttack.Charge)][TestCase((int)BossAttack.Hook)]
        public void NewlyReleasedMovementAttacksCannotDamageDuringTheirReleaseTick(int kind)
        {
            var sim=Fixture();Shield(sim,50,.225f);var e=sim.State.enemies[0];e.boss=kind>=100;e.kind=kind;e.position=sim.State.position+Vector2.up*.5f;e.attack=10;
            e.brain.boss.initialized=true;e.brain.boss.cooldowns=new[]{1000f,1000f,1000f};
            e.brain.action=new EnemyActionState{id=99,kind=kind,phase=EnemyActionPhase.Preparing,origin=e.position,aim=e.position+Vector2.down*8,direction=Vector2.down,preparation=.2f,remaining=.2f,remainingCharges=1};
            var f=sim.ForecastIncoming(.5f);Advance(sim,.5f);var actual=sim.State.damageEvents.Single(x=>x.incoming);
            Assert.AreEqual(.25f,actual.time,.0001f);Assert.AreEqual(actual.time,f.hits.Single().after,.0001f);Assert.AreEqual(actual.hpLoss,f.HpLoss,.0001f);
        }
        [Test]
        public void ShardBurstAlreadyOverlappingTheHeroCannotHitUntilAfterItSpawnsArrows()
        {
            var sim=Fixture();Shield(sim,50,.225f);var h=Hazard(sim,20,.2f);h.shardBurst=true;h.direction=Vector2.up;
            var f=sim.ForecastIncoming(.5f);Advance(sim,.5f);var actual=sim.State.damageEvents.Single(e=>e.incoming);
            Assert.AreEqual(.25f,actual.time,.0001f);Assert.AreEqual(actual.time,f.hits.Single().after,.0001f);Assert.AreEqual(actual.hpLoss,f.HpLoss,.0001f);
        }
        [Test]
        public void HostileFanCountsOneHitEvenWhenThreeProjectilesIntersect()
        {
            var sim=Fixture();sim.State.projectileGroups.Add(new ProjectileGroup{actionId=88,maxPerVictim=1});
            for(int i=-1;i<=1;i++)sim.State.projectiles.Add(new CombatProjectile{id=100+i,actionId=88,hostile=true,position=sim.State.position+Vector2.down*2,origin=sim.State.position+Vector2.down*2,direction=EnemyCombat.Rotate(Vector2.up,i*10),radius=.2f,speed=10,remaining=9,damage=20,createdAt=-1,definitionId="FAN"});
            var f=sim.ForecastIncoming(1);Assert.AreEqual(1,f.hits.Count);Advance(sim,1);Assert.AreEqual(1,sim.State.damageEvents.Count(e=>e.incoming));Assert.AreEqual(f.HpLoss,IncomingHp(sim),.0001f);
        }
        [Test]
        public void AlreadyHitFanGroupCannotThreatenAgainAndCasterDeathDoesNotRemoveLiveArrows()
        {
            var sim=Fixture();var p=new CombatProjectile{id=100,actionId=88,hostile=true,casterId="missing",position=sim.State.position+Vector2.down*2,origin=sim.State.position+Vector2.down*2,direction=Vector2.up,radius=.2f,speed=10,remaining=9,damage=20,createdAt=-1};sim.State.projectiles.Add(p);
            Assert.AreEqual(1,sim.ForecastIncoming(1).hits.Count);var group=new ProjectileGroup{actionId=88,maxPerVictim=1};group.victims.Add(new ProjectileVictim{id=-1,hits=1});sim.State.projectileGroups.Add(group);Assert.IsEmpty(sim.ForecastIncoming(1).hits);
        }
        [Test]
        public void ProjectileTerrainAndRingSafeInteriorRemainSafeInTheForecast()
        {
            var sim=Fixture();var h=Hazard(sim,50);h.shape=AttackShape.Ring;h.radius=4;h.innerRadius=2;
            sim.State.projectiles.Add(new CombatProjectile{id=80,hostile=true,position=sim.State.position+Vector2.down*6,origin=sim.State.position+Vector2.down*6,direction=Vector2.up,radius=.2f,speed=10,remaining=9,damage=20,createdAt=-1});sim.State.layout.legacy=false;
            sim.State.layout.obstacles.Add(new RiftObstacle{id="wall",position=sim.State.position+Vector2.down*3,halfSize=new Vector2(3,.1f),blocksSight=false,blocksProjectile=true,blocksWalk=false});
            Assert.IsEmpty(sim.ForecastIncoming(1).hits);
        }
        [Test]
        public void CandidatePositionCannotRevealSourcesOutsideCurrentObservation()
        {
            var sim=Fixture();var h=Hazard(sim,50);h.position+=Vector2.right*30;
            Assert.IsEmpty(sim.ForecastIncoming(1,h.position).sources);Assert.IsEmpty(sim.ForecastIncoming(1,h.position).hits);
        }
        [TestCase(BossAttack.Basic)][TestCase(BossAttack.Slam)][TestCase(BossAttack.Beam)][TestCase(BossAttack.Blasts)][TestCase(BossAttack.Legacy)]
        public void AnnouncedBossAttacksMatchDamageBeforeAnyNewDecision(BossAttack kind)
        {
            var sim=Fixture();var e=sim.State.enemies[0];e.boss=true;e.pattern=kind==BossAttack.Legacy?1:0;e.position=sim.State.position+Vector2.up*2;e.attack=10;e.brain.boss.initialized=true;e.brain.boss.cooldowns=new[]{1000f,1000f,1000f};
            var a=new EnemyActionState{id=99,kind=(int)kind,phase=EnemyActionPhase.Preparing,origin=e.position,aim=sim.State.position,direction=Vector2.down,preparation=.2f,remaining=.2f,remainingCharges=1};a.points.Add(sim.State.position);a.points.Add(sim.State.position+Vector2.right);a.points.Add(sim.State.position+Vector2.left);e.brain.action=a;
            if(kind==BossAttack.Beam)a.aim=e.position+Vector2.down*10;
            var f=sim.ForecastIncoming(1);Assert.Greater(f.hits.Count,0);Advance(sim,1);Assert.AreEqual(f.HpLoss,IncomingHp(sim),.001f,kind.ToString());
        }
        [TestCase(0)][TestCase(2)][TestCase(3)][TestCase(8)][TestCase(9)]
        public void AnnouncedNormalAttacksMatchActualIncomingDamage(int kind)
        {
            var sim=Fixture();var e=sim.State.enemies[0];e.kind=kind;e.position=sim.State.position+Vector2.up*2;e.attack=10;
            e.brain.action=new EnemyActionState{id=99,kind=kind,phase=EnemyActionPhase.Preparing,origin=e.position,aim=sim.State.position,direction=Vector2.down,preparation=.2f,remaining=.2f};
            var f=sim.ForecastIncoming(1);Assert.Greater(f.hits.Count,0);Advance(sim,1);Assert.AreEqual(f.HpLoss,IncomingHp(sim),.001f);
        }
        [Test]
        public void LegacyScalarPreparationIsObservedWithoutAllocatingAnActionOrChangingTheSave()
        {
            var sim=Fixture();var e=sim.State.enemies[0];e.position=sim.State.position+Vector2.up*2;e.attack=10;e.windup=.2f;e.aim=sim.State.position;
            string before=JsonUtility.ToJson(sim.State);var f=sim.ForecastIncoming(1);Assert.AreEqual(10,f.HpLoss,.0001f);Assert.AreEqual(before,JsonUtility.ToJson(sim.State));Advance(sim,1);Assert.AreEqual(f.HpLoss,IncomingHp(sim),.001f);
        }
        [Test]
        public void LegacyGroundRetainsItsSavedPulseTimingAndFinalDamageRule()
        {
            var sim=Fixture();sim.State.effects.Add(new GroundEffect{id=99,hostile=true,kind=4,position=sim.State.position,radius=2,delay=.2f,duration=.7f,tick=.1f,damage=10,createdAt=-1});
            var f=sim.ForecastIncoming(1);Advance(sim,1);Assert.AreEqual(f.HpLoss,IncomingHp(sim),.001f);Assert.AreEqual(f.hits.Count,sim.State.damageEvents.Count(e=>e.incoming));
        }
        [Test]
        public void OverlapOptionUsesItsOwnWindowAndDoesNotRequireOneHitToReachTheThreshold()
        {
            var sim=Fixture();Hazard(sim,10,.2f);Hazard(sim,10,.8f);var d=Edict(sim);Set(d,"dodge.area.policy","DAMAGE");Set(d,"dodge.area.hpPercent","15");Set(d,"dodge.overlap","ON");Set(d,"dodge.totalHpPercent","15");
            var a=sim.AssessEdictSurvival(d);Assert.IsTrue(a.DodgeRequested);Assert.AreEqual("OVERLAP",a.dodgeReasons.Single().code);
            Set(d,"dodge.window","0.5");Assert.IsFalse(sim.AssessEdictSurvival(d).DodgeRequested);
        }
        [Test]
        public void AlwaysDodgeAndControlAvoidanceRemainMeaningfulWithFullShieldAbsorption()
        {
            var sim=Fixture();Shield(sim,100,4);var h=Hazard(sim,10,.2f);var d=Edict(sim);Set(d,"dodge.area.policy","ALWAYS");
            var a=sim.AssessEdictSurvival(d);Assert.IsTrue(a.DodgeRequested);Assert.AreEqual(0,a.forecast.HpLoss,.0001f);sim.State.enemyHazards.Clear();Set(d,"dodge.area.policy","OFF");Set(d,"dodge.control","PULL");
            sim.State.projectiles.Add(new CombatProjectile{id=99,hostile=true,origin=sim.State.position+Vector2.down*2,position=sim.State.position+Vector2.down*2,direction=Vector2.up,radius=.5f,speed=12,remaining=10,damage=10,pullDistance=2,createdAt=-1});
            Assert.IsTrue(sim.AssessEdictSurvival(d).dodgeReasons.Any(r=>r.code=="CONTROL"));
        }
        [Test]
        public void ColdSlowAndPoisonGroundDoNotInventFreezeOrAttachedDamage()
        {
            var sim=Fixture();var h=Hazard(sim,5,0,2,.5f,.5f);h.element=4;h.heroSlow=.35f;var d=Edict(sim);Set(d,"dodge.control","FREEZE");Set(d,"survival.dot","ON");Set(d,"survival.dotHpPercent","1");
            var a=sim.AssessEdictSurvival(d);Assert.IsFalse(a.DodgeRequested);Assert.IsFalse(a.emergency);Assert.IsTrue(a.limitations.Any(x=>x.Contains("부착")));Assert.IsTrue(a.forecast.hits.All(x=>x.category==IncomingCategory.Ground));
        }
        [Test]
        public void GroundDamageThresholdMeasuresOneSecondInsteadOfOneTick()
        {
            var sim=Fixture();Hazard(sim,10,0,3,.5f,.5f);var d=Edict(sim);Set(d,"dodge.ground.policy","DAMAGE");Set(d,"dodge.ground.hpPercent","20");
            var a=sim.AssessEdictSurvival(d);Assert.IsTrue(a.DodgeRequested);Assert.AreEqual(20,a.dodgeReasons.Single().hpPercent,.0001f);
        }
        [Test]
        public void LethalPredictionAndRecoveryHysteresisBelongToTheCurrentRun()
        {
            var sim=Fixture();Shield(sim,50,4);Hazard(sim,80,.2f);Hazard(sim,80,.4f);var d=Edict(sim);Set(d,"survival.lethal","ON");
            var a=sim.AssessEdictSurvival(d);Assert.IsTrue(a.lethal);Assert.IsTrue(a.emergency);Assert.AreEqual(110,a.forecast.HpLoss,.001f);
            var memory=JsonUtility.FromJson<EdictSurvivalState>(JsonUtility.ToJson(a.NextState));sim.State.enemyHazards.Clear();sim.State.health=40;
            a=sim.AssessEdictSurvival(d,memory);Assert.IsTrue(a.emergency);Assert.IsTrue(a.heldForRecovery);sim.State.health=50;Assert.IsFalse(sim.AssessEdictSurvival(d,a.NextState).emergency);
            sim.State.health=40;memory.runId="another-run";Assert.IsFalse(sim.AssessEdictSurvival(d,memory).emergency);
        }
        [TestCase(30f,true)][TestCase(30.1f,false)]
        public void LowHealthUsesMaximumHealthAndTheSpecifiedBoundary(float health,bool expected)
        {
            var sim=Fixture();sim.State.health=health;Shield(sim,100,4);var d=Edict(sim);Set(d,"survival.lowHp","ON");Assert.AreEqual(expected,sim.AssessEdictSurvival(d).lowHp);
        }
        [Test]
        public void PotionIntentIsIndependentOfMainActionAndDoesNotApplyOrSaveAnything()
        {
            var sim=Fixture();sim.State.health=30;sim.State.paused=true;sim.State.heroAction.phase=HeroActionPhase.Preparing;var d=Edict(sim);string before=JsonUtility.ToJson(sim.State),code=HuntEdictV2Codec.Encode(d);
            var a=sim.AssessEdictSurvival(d);Assert.IsTrue(a.potionRequested);Assert.AreEqual(before,JsonUtility.ToJson(sim.State));Assert.AreEqual(code,HuntEdictV2Codec.Encode(d));
            sim.State.potionCd=.1f;Assert.IsFalse(sim.AssessEdictSurvival(d).potionRequested);sim.State.potionCd=0;Set(d,"survival.potion","OFF");Assert.IsFalse(sim.AssessEdictSurvival(d).potionRequested);
        }
        [Test]
        public void EmergencyIntentOrderAndFallbackPreserveExplicitPolicyAndDisabledReferences()
        {
            var sim=Fixture(HeroClass.Warrior);var d=Edict(sim);d.slots[0]="W02";d.slots[1]="W05";Set(d,"survival.defenseSkill","W05");Set(d,"survival.escapeSkill","W02");Set(d,"survival.order","WALK,ESCAPE,DEFENSE");Set(d,"survival.fallback","KEEP_FIGHTING");Set(d,"survival.reserveSkill","ON");Set(d,"survival.reserveResource","ON");
            var a=sim.AssessEdictSurvival(d);CollectionAssert.AreEqual(new[]{"WALK","ESCAPE","DEFENSE"},a.emergencyOrder.Select(x=>x.role));Assert.AreEqual("KEEP_FIGHTING",a.escapeFallback.role);Assert.IsTrue(a.preserveEscapeForEmergency);Assert.AreEqual(0,a.escapeResource,"The current three escape skills really cost zero.");
            d=HuntEdictV2.WithOption(d,"W02",1,"OFF");a=sim.AssessEdictSurvival(d);Assert.IsFalse(a.emergencyOrder[1].referenceAvailable);Assert.IsFalse(a.preserveEscapeForEmergency);Assert.AreEqual("W02",d.global.Single(x=>x.id=="survival.escapeSkill").value);
            d=HuntEdictV2.WithOption(d,"W02",1,"ON");d=HuntEdictV2.WithOption(d,"W02",2,"ATTACK");Assert.IsFalse(sim.AssessEdictSurvival(d).emergencyOrder[1].referenceAvailable);
        }
        [Test]
        public void LockedSurvivalReferenceStaysStoredAndCannotBeUsedAsAnEmergencyReference()
        {
            var sim=Fixture(HeroClass.Mage,1);var d=Edict(sim);d.slots[0]="M04";Set(d,"survival.escapeSkill","M04");var a=sim.AssessEdictSurvival(d);Assert.IsFalse(a.emergencyOrder.Single(x=>x.role=="ESCAPE").referenceAvailable);Assert.AreEqual("M04",d.slots[0]);
        }
        [Test]
        public void ForecastCollectionsAndRecoveryStateAreDetachedFromMutableInputs()
        {
            var sim=Fixture();Hazard(sim,10,.2f);var d=Edict(sim);sim.State.health=20;Set(d,"survival.lowHp","ON");string before=JsonUtility.ToJson(sim.State);var a=sim.AssessEdictSurvival(d);
            Assert.AreEqual(before,JsonUtility.ToJson(sim.State));a.NextState.active=false;Assert.IsTrue(a.NextState.active);
            Assert.Throws<NotSupportedException>(()=>((IList<ObservedIncomingHit>)a.forecast.hits).Clear());Assert.Throws<ArgumentException>(()=>sim.ForecastIncoming(float.NaN));Assert.Throws<ArgumentException>(()=>sim.ForecastIncoming(11));Assert.Throws<ArgumentException>(()=>sim.ForecastIncoming(1,new Vector2(float.NaN,0)));
        }
    }
}
