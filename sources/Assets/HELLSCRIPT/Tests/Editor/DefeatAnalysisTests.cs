using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace Hellscript.Tests
{
    public sealed class DefeatAnalysisTests
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
        CombatSimulation Fixture(int hero=2,int training=0)=>new CombatSimulation(Account(hero),catalog,1,training,ownedTraining:true);
        static void Advance(CombatSimulation sim,int ticks){for(int i=0;i<ticks;i++)sim.Tick(CombatSimulation.Step);}

        [Test]
        public void PreDeathRecordingStaysOutOfTheSavedRunAndAccount()
        {
            var account=Account();string before=JsonUtility.ToJson(account);
            var sim=new CombatSimulation(account,catalog,1,0,ownedTraining:true);Advance(sim,40);
            Assert.Greater(sim.State.statistics.ringBuffer.Count,0,"ticks should record snapshots in memory");
            string run=JsonUtility.ToJson(sim.State);
            Assert.IsFalse(run.Contains("ringBuffer"),"the ring buffer must not enter the save format");
            Assert.IsFalse(run.Contains("damageSources"),"tick snapshots must not enter the save format");
            Assert.AreEqual(before,JsonUtility.ToJson(account),"recording must not touch the account");
            var reloaded=JsonUtility.FromJson<RunState>(run);GameStore.NormalizeRun(reloaded);
            Assert.IsNotNull(reloaded.statistics.ringBuffer);
            Assert.AreEqual(0,reloaded.statistics.ringBuffer.Count,"a reloaded run starts with an empty window");
        }

        [Test]
        public void EveryCombatTickRecordsOneSnapshotUntilTheCapacityIsReached()
        {
            var sim=Fixture();Advance(sim,120);
            Assert.AreEqual(sim.State.statistics.ticks,sim.State.statistics.ringBuffer.Count,"one snapshot per combat tick");
            var last=sim.State.statistics.ringBuffer[sim.State.statistics.ringBuffer.Count-1];
            Assert.AreEqual(sim.State.time,last.time,.0001f);Assert.AreEqual(Mathf.Max(0,sim.State.health),last.hp,.0001f);
            Advance(sim,CombatStatistics.RingBufferCapacity+50);
            Assert.AreEqual(CombatStatistics.RingBufferCapacity,sim.State.statistics.ringBuffer.Count,"oldest snapshots are dropped");
            Assert.Greater(sim.State.statistics.ringBuffer[0].time,0,"the retained window is the most recent one");
        }

        [Test]
        public void IncomingDamageIsAttributedToItsSourceInTheAnalysis()
        {
            var sim=Fixture();Advance(sim,2);
            Call(sim,"Hurt",12f,0,"ENEMY","ENEMY_ATTACK");
            sim.Tick(CombatSimulation.Step);
            var analysis=sim.State.statistics.BuildDefeatAnalysis(catalog,5f);
            Assert.Greater(analysis.snapshotCount,0);
            var source=analysis.fatalSources.FirstOrDefault(s=>s.sourceId=="ENEMY_ATTACK");
            Assert.IsNotNull(source,"the applied hit must appear as a damage source");
            Assert.AreEqual("적 일반 공격",source.sourceName);
            Assert.GreaterOrEqual(source.hitCount,1);Assert.Greater(source.totalDamage,0);
            Assert.Greater(analysis.burstDamage5s,0);
            Assert.IsTrue(analysis.fatalSources.SequenceEqual(analysis.fatalSources.OrderByDescending(s=>s.totalDamage)),"sources are ordered by damage");
        }

        [Test]
        public void OnlyLockoutCodesAreRecordedAsBlockedSkillAttempts()
        {
            var sim=Fixture();Advance(sim,2);
            var rule=new Rule(3){action=RuleAction.Skill};
            Call(sim,"RecordDecision",rule,0,"RESOURCE","필요 25 / 현재 4",null);
            Call(sim,"RecordDecision",rule,0,"COOLDOWN","남은 CD 1.20초",null);
            Call(sim,"RecordDecision",rule,0,"RANGE","스킬 사거리 3m 밖",null);
            Call(sim,"RecordDecision",rule,0,"NO_TARGET","감지한 대상이 없습니다.",null);
            sim.Tick(CombatSimulation.Step);
            var blocked=sim.State.statistics.BuildDefeatAnalysis(catalog,5f).blockedSummary;
            CollectionAssert.AreEquivalent(new[]{BlockReason.Resource,BlockReason.Cooldown},blocked.Select(b=>b.reason).ToArray());
            Assert.AreEqual("자원 부족",blocked.Single(b=>b.reason==BlockReason.Resource).reasonName);
            Assert.AreEqual("남은 CD 1.20초",blocked.Single(b=>b.reason==BlockReason.Cooldown).latestDetail);
            Assert.IsTrue(blocked.All(b=>b.skillId==CombatTelemetry.SkillId(3)));
        }

        [Test]
        public void TheAnalysisWindowIsCutByElapsedTimeNotBySnapshotCount()
        {
            var sim=Fixture();Advance(sim,200);
            var wide=sim.State.statistics.BuildDefeatAnalysis(catalog,5f);
            var narrow=sim.State.statistics.BuildDefeatAnalysis(catalog,1f);
            Assert.Less(narrow.snapshotCount,wide.snapshotCount);
            Assert.LessOrEqual(narrow.preDeathWindowSeconds,1.0001f);
            Assert.LessOrEqual(wide.preDeathWindowSeconds,5.0001f);
            Assert.AreEqual(Mathf.Max(0,sim.State.health),narrow.finalHp,.0001f);
        }

        [Test]
        public void TheTickThatEndsTheRunIsStillRecorded()
        {
            var sim=Fixture();Advance(sim,2);
            int before=sim.State.statistics.ringBuffer.Count;
            Call(sim,"Hurt",100000f,0,"ENEMY","ENEMY_ATTACK");
            sim.Tick(CombatSimulation.Step);
            Assert.AreEqual(0,sim.State.health,"the fixture hit should be lethal");
            Assert.Greater(sim.State.statistics.ringBuffer.Count,before,"the lethal tick is part of the window");
            var analysis=sim.State.statistics.BuildDefeatAnalysis(catalog,5f);
            Assert.AreEqual(0,analysis.finalHp,.0001f);
            Assert.IsTrue(analysis.fatalSources.Any(s=>s.sourceId=="ENEMY_ATTACK"));
        }
    }
}
