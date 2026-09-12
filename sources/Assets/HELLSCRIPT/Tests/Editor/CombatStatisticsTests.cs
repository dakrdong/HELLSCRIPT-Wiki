using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace Hellscript.Tests
{
    public sealed class CombatStatisticsTests
    {
        GameCatalog catalog;
        [SetUp] public void Setup(){catalog=ScriptableObject.CreateInstance<GameCatalog>();catalog.Populate();}
        [TearDown] public void Teardown()=>UnityEngine.Object.DestroyImmediate(catalog);
        static object Call(CombatSimulation sim,string name,params object[] args)
        {
            var method=typeof(CombatSimulation).GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic);var parameters=method.GetParameters();var values=new object[parameters.Length];
            for(int i=0;i<values.Length;i++)values[i]=i<args.Length?args[i]:parameters[i].DefaultValue;return method.Invoke(sim,values);
        }
        AccountSave Account(int hero=2,int level=30)
        {var a=ContentTestAccounts.Training(catalog);a.selectedHero=hero;a.Hero.level=level;a.Hero.build=BehaviorPresets.ForLevel((HeroClass)hero,0,level,catalog);return a;}
        CombatSimulation Quiet(int hero=2,int training=0)
        {
            var a=Account(hero);a.Hero.build.rules.Clear();a.Hero.build.passives=Array.Empty<int>();a.Hero.build.movement=MovementMode.Stand;a.Hero.build.potionThreshold=0;
            return new CombatSimulation(a,catalog,1,training,ownedTraining:true);
        }
        static void Advance(CombatSimulation sim,int ticks){for(int i=0;i<ticks;i++)sim.Tick(CombatSimulation.Step);}
        static void Until(CombatSimulation sim,Func<bool> ready,int ticks=200)
        {for(int i=0;i<ticks&&!ready();i++)sim.Tick(CombatSimulation.Step);Assert.IsTrue(ready());}
        CombatSimulation Restore(CombatSimulation source)
        {
            var a=Account((int)source.Hero.heroClass,source.Hero.level);a.heroes[a.selectedHero]=JsonUtility.FromJson<HeroSave>(JsonUtility.ToJson(source.Hero));
            var state=JsonUtility.FromJson<RunState>(JsonUtility.ToJson(source.State));GameStore.NormalizeRun(state);return new CombatSimulation(a,catalog,1,restore:state);
        }
        [Test]
        public void CompleteOwnedFixturesUseAllDamageSourcesAndDoNotChangeAccount([Values(0,1,2)]int hero,[Values(0,1,2)]int training)
        {
            var a=Account(hero);string before=JsonUtility.ToJson(a);var sim=new CombatSimulation(a,catalog,1,training,ownedTraining:true);Advance(sim,1201);
            var stats=sim.State.statistics;Assert.AreEqual(CombatFinish.Duration,stats.finish);Assert.AreEqual(1200,stats.ticks);Assert.IsTrue(stats.fullRun);Assert.IsFalse(stats.buildChanged);
            Assert.AreEqual(sim.State.dealt,stats.damage,Math.Max(.05,sim.State.dealt*.0001));Assert.AreEqual(stats.damage,stats.skills.Sum(s=>s.damage),.000001);
            Assert.AreEqual(stats.hpDamage,stats.skills.Sum(s=>s.hpDamage),.000001);Assert.LessOrEqual(stats.hpDamage,stats.damage+.000001);
            Assert.AreEqual(before,JsonUtility.ToJson(a));Assert.IsTrue(stats.skills.Any(s=>s.starts>0));
        }
        [Test]
        public void DamageTotalsSurviveTheTwoThousandEventRetentionLimit()
        {
            var sim=Quiet();sim.Stats.crit=0;var enemy=sim.State.enemies[0];double expected=0;
            for(int i=0;i<2501;i++){var hit=(DamageEvent)Call(sim,"Hit",enemy,.001f,0,false,0f,null,false,false,"BASIC",i+1,0,DamageKind.Basic);expected+=hit.finalDamage;}
            Assert.AreEqual(2000,sim.State.damageEvents.Count);Assert.AreEqual(2501,sim.State.statistics.skills.Single().hits);Assert.AreEqual(expected,sim.State.statistics.damage,.000001);
            Assert.Greater(sim.State.statistics.damage,sim.State.damageEvents.Sum(e=>(double)e.finalDamage));
        }
        [Test]
        public void ActionTotalsKeepCancelledPreparationsAfterOldEventsAreDiscarded()
        {
            var sim=Quiet();for(int i=0;i<401;i++){Call(sim,"StartHeroAction",12,sim.State.enemies[0],sim.State.position,0f,0,false);Call(sim,"InterruptHeroAction","test cancellation");}
            var row=sim.State.statistics.skills.Single();Assert.AreEqual(600,sim.State.actionEvents.Count);Assert.AreEqual(401,row.starts);Assert.AreEqual(401,row.interruptions);Assert.AreEqual(0,row.releases);
        }
        [Test]
        public void DamageSeparatesShieldAbsorptionHpLossAndOverkill()
        {
            var sim=Quiet();sim.Stats.hp=1000;sim.State.health=100;sim.Stats.armor=0;sim.Stats.shieldMultiplier=1;Call(sim,"AddShield","M05",300f,10f,1);Call(sim,"Hurt",200f);Call(sim,"Hurt",900f);
            var stats=sim.State.statistics;Assert.AreEqual(2,stats.incomingHits);Assert.AreEqual(1100,stats.incomingDamage,.001);Assert.AreEqual(300,stats.absorbed,.001);Assert.AreEqual(100,stats.hpLost,.001);
            Assert.AreEqual(0,stats.damage);Assert.AreEqual(0,sim.State.health);
        }
        [Test]
        public void CoalescedResourceDecisionsStillCountEachActualRuleEvaluation()
        {
            var sim=Quiet();var rule=BehaviorRules.Make(12);for(int i=0;i<501;i++)Call(sim,"RecordDecision",rule,0,"RESOURCE","not enough",sim.State.enemies[0]);
            Assert.AreEqual(1,sim.State.decisions.Count);Assert.AreEqual(501,sim.State.statistics.resourceChecks);Assert.AreEqual(501,sim.State.statistics.For("M01").resourceChecks);
        }
        [Test]
        public void StatisticsRestoreWithoutReplayOrLoss()
        {
            var a=Account();var sim=new CombatSimulation(a,catalog,1,1,ownedTraining:true);Advance(sim,431);var restored=Restore(sim);
            Assert.AreEqual(JsonUtility.ToJson(sim.State.statistics),JsonUtility.ToJson(restored.State.statistics));Advance(sim,400);Advance(restored,400);
            Assert.AreEqual(JsonUtility.ToJson(sim.State.statistics),JsonUtility.ToJson(restored.State.statistics));Assert.AreEqual(sim.State.rng,restored.State.rng);
        }
        [Test]
        public void LegacyStatisticsStartAtTheObservedTimeWithoutInventingEarlierTotals()
        {
            var sim=Quiet();Advance(sim,50);sim.State.statistics=null;sim.State.dealt=999;GameStore.NormalizeRun(sim.State);
            Assert.IsFalse(sim.State.statistics.fullRun);Assert.AreEqual(sim.State.time,sim.State.statistics.observedFrom);Assert.AreEqual(0,sim.State.statistics.damage);Assert.AreEqual(0,sim.State.statistics.ticks);
            sim.State.statistics.version=99;Assert.Throws<NotSupportedException>(()=>GameStore.NormalizeRun(sim.State));
        }
        [Test]
        public void PausedOrBlockedTicksDoNotAdvanceStatistics()
        {
            var sim=Quiet();Advance(sim,5);string before=JsonUtility.ToJson(sim.State.statistics);sim.State.paused=true;Advance(sim,10);Assert.AreEqual(before,JsonUtility.ToJson(sim.State.statistics));
            sim.State.paused=false;sim.State.portal=true;Advance(sim,10);Assert.AreEqual(before,JsonUtility.ToJson(sim.State.statistics));sim.State.portal=false;sim.State.navigationError="blocked";Advance(sim,10);Assert.AreEqual(before,JsonUtility.ToJson(sim.State.statistics));
        }
        [Test]
        public void MetadataEditsKeepComparabilityButARevertedCombatEditDoesNot()
        {
            var sim=Quiet();var original=sim.State.build.Copy();var edit=original.Copy();edit.name="another name";edit.version="another version";edit.autoRepeat=!edit.autoRepeat;
            Assert.IsTrue(sim.ApplyBuild(edit));Assert.IsFalse(sim.State.statistics.buildChanged);edit.distance+=1;Assert.IsTrue(sim.ApplyBuild(edit));Assert.IsTrue(sim.State.statistics.buildChanged);
            Assert.IsTrue(sim.ApplyBuild(original));Assert.IsTrue(sim.State.statistics.buildChanged);
        }
        [TestCase(0,CombatFinish.HeroDeath)][TestCase(1,CombatFinish.TargetsDefeated)][TestCase(2,CombatFinish.Abandoned)]
        public void TerminationReasonsDoNotTreatManualExitAsAFinishedTraining(int scenario,CombatFinish expected)
        {
            var sim=Quiet();if(scenario==0)sim.State.health=0;else if(scenario==1)foreach(var enemy in sim.State.enemies){enemy.health=0;enemy.dead=true;}else sim.Abandon();
            sim.Tick(.05f);Assert.AreEqual(expected,sim.State.statistics.finish);string frozen=JsonUtility.ToJson(sim.State.statistics);Advance(sim,10);Assert.AreEqual(frozen,JsonUtility.ToJson(sim.State.statistics));
        }
        [TestCase(1,0)][TestCase(9,1)][TestCase(15,2)]
        public void MovementCountsActualTravelAndCompletionWithoutCountingItAsWalking(int skill,int hero)
        {
            var sim=Quiet(hero);var start=sim.State.position;Call(sim,"StartHeroAction",skill,sim.State.enemies[0],start+Vector2.right*3,0f,0,true);Advance(sim,30);
            Assert.AreEqual(1,sim.State.statistics.movementStarts);Assert.AreEqual(1,sim.State.statistics.movementCompletions);Assert.AreEqual(0,sim.State.statistics.walkingDistance,.001);
            Assert.Greater(Vector2.Distance(start,sim.State.position),2.9f);
        }
        [TestCase(false)][TestCase(true)]
        public void RealDangerImpactDistinguishesLeavingTheAreaFromShieldedHits(bool leave)
        {
            var sim=Quiet(2,2);Until(sim,()=>sim.State.enemies[0].brain.action.phase==EnemyActionPhase.Preparing);var source=sim.State.enemies[0];
            // Both moves must be legal Teleports; the shorter one deliberately remains in the real warning.
            var destination=sim.State.position+Vector2.right*(leave?4:2);
            var warning=EnemyCombat.Threats(sim.State,sim.Map).Single(t=>t.key=="action-"+source.brain.action.id);
            Assert.IsTrue(EnemyCombat.Contains(warning,sim.State.position));Assert.AreEqual(!leave,EnemyCombat.Contains(warning,destination));
            Call(sim,"AddShield","M05",sim.Stats.hp,10f,1);Call(sim,"StartHeroAction",15,source,destination,0f,0,true);
            Until(sim,()=>sim.State.statistics.evasionSuccesses+sim.State.statistics.evasionHits>0);
            var stats=sim.State.statistics;Assert.AreEqual(1,stats.evasionAttempts);Assert.AreEqual(leave?1:0,stats.evasionSuccesses);Assert.AreEqual(leave?0:1,stats.evasionHits);
            Advance(sim,40);Assert.AreEqual(1,stats.evasionSuccesses+stats.evasionHits,"Repeated ground-effect ticks must not re-count the warning.");
        }
        [Test]
        public void CancelledWarningIsExcludedInsteadOfAwardingAnEvasion()
        {
            var sim=Quiet(2,2);Until(sim,()=>sim.State.enemies[0].brain.action.phase==EnemyActionPhase.Preparing);var enemy=sim.State.enemies[0];
            Call(sim,"StartHeroAction",15,enemy,sim.State.position+Vector2.right*4,0f,0,true);Until(sim,()=>sim.State.statistics.evasionAttempts>0,15);
            Call(sim,"InterruptEnemyAction",enemy,"test control");sim.Tick(.05f);Assert.AreEqual(1,sim.State.statistics.evasionExcluded);Assert.AreEqual(0,sim.State.statistics.evasionSuccesses);Assert.IsEmpty(sim.State.statistics.pendingEvasions);
        }
        [Test]
        public void PendingEvasionSurvivesASnapshotAndResolvesOnTheSameRealImpact()
        {
            var sim=Quiet(2,2);Until(sim,()=>sim.State.enemies[0].brain.action.phase==EnemyActionPhase.Preparing);Call(sim,"StartHeroAction",15,sim.State.enemies[0],sim.State.position+Vector2.right*4,0f,0,true);
            Until(sim,()=>sim.State.statistics.evasionAttempts>0,15);Assert.IsNotEmpty(sim.State.statistics.pendingEvasions);var restored=Restore(sim);Advance(sim,50);Advance(restored,50);
            Assert.AreEqual(1,sim.State.statistics.evasionSuccesses);Assert.AreEqual(JsonUtility.ToJson(sim.State.statistics),JsonUtility.ToJson(restored.State.statistics));
        }
    }
}
