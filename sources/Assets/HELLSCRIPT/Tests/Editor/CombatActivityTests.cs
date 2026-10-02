using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Hellscript.Tests
{
    public sealed class CombatActivityTests
    {
        [TestCase(CombatActivity.Movement)] [TestCase(CombatActivity.Attack)] [TestCase(CombatActivity.Evasion)] [TestCase(CombatActivity.Farming)]
        public void ActualTickUsesPurposeAndPauseOrPortalNeverAddsTime(CombatActivity activity)
        {
            var catalog=ScriptableObject.CreateInstance<GameCatalog>();catalog.Populate();
            try
            {
                var sim=new CombatSimulation(ContentTestAccounts.Legacy(),catalog,1,seed:111);
                foreach(var enemy in sim.State.enemies)enemy.dead=true;
                sim.State.decisionTime=10;sim.State.targetId=-1;
                if(activity==CombatActivity.Attack||activity==CombatActivity.Evasion)
                    sim.State.heroAction=new HeroActionState{skill=-1,phase=HeroActionPhase.Recovering,recovery=1,escape=activity==CombatActivity.Evasion};
                else if(activity==CombatActivity.Farming)
                {
                    var chest=sim.State.layout.chests.First();chest.phase=ChestPhase.Opening;chest.discovered=true;
                    sim.State.position=chest.openingPosition;sim.State.exploration.chestId=chest.id;
                }
                else
                {
                    sim.State.build.rules.Clear();sim.State.build.rules.Add(new Rule(-1){action=RuleAction.Explore});
                    sim.State.movementRule=0;sim.State.movementAction=RuleAction.Explore;
                }
                sim.Tick(CombatSimulation.Step);var feedback=sim.State.statistics.feedback;
                Assert.That(feedback.ActivityDurations()[(int)activity],Is.EqualTo(CombatSimulation.Step).Within(.00001));
                Assert.That(feedback.ActivitySeconds,Is.EqualTo(CombatSimulation.Step).Within(.00001));
                double before=feedback.ObservedSeconds;sim.State.paused=true;sim.Tick(1);sim.State.paused=false;sim.State.portal=true;sim.Tick(1);
                Assert.AreEqual(before,feedback.ObservedSeconds);
            }
            finally{Object.DestroyImmediate(catalog);}
        }
        [Test] public void PercentagesAreExclusiveAndAlwaysSumToOneHundred()
        {
            var f=new CombatFeedback();Assert.AreEqual(0,f.ActivityPercentages().Sum());
            f.RecordActivity(CombatActivity.Movement,18,18);f.RecordActivity(CombatActivity.Attack,24,42);
            f.RecordActivity(CombatActivity.Evasion,32,74);f.RecordActivity(CombatActivity.Farming,26,100);
            CollectionAssert.AreEqual(new[]{18,24,32,26},f.ActivityPercentages());
            f.RecordActivity(null,10,110);Assert.AreEqual(100,f.ActivitySeconds);Assert.AreEqual(110,f.ObservedSeconds);
            for(int i=0;i<500;i++)
            {f.RecordActivity((CombatActivity)(i%4),.05f,110+(i+1)*.05f);Assert.AreEqual(100,f.ActivityPercentages().Sum());}
            var equal=new CombatFeedback{movementSeconds=1,attackSeconds=1,evasionSeconds=1};
            CollectionAssert.AreEqual(new[]{34,33,33,0},equal.ActivityPercentages());
        }
        [Test] public void GraphIsCumulativeAcrossBoundariesBoundedAndIsNotWrittenToSaves()
        {
            var f=new CombatFeedback();f.RecordActivity(CombatActivity.Farming,.1f,1.05f);
            Assert.AreEqual(0,f.activityHistory[0].seconds[3]);
            Assert.That(f.activityHistory[1].seconds[3],Is.EqualTo(.05).Within(.00001));
            Assert.That(f.activityHistory[2].seconds[3],Is.EqualTo(.1).Within(.00001));
            f.RecordActivity(null,2000,2001.05f);Assert.LessOrEqual(f.activityHistory.Count,CombatFeedback.ActivityHistoryLimit);
            Assert.That(f.activityHistory[0].time,Is.EqualTo(.95).Within(.00001));
            Assert.That(f.activityHistory.Last().time,Is.EqualTo(2001.05f));
            Assert.That(f.activityHistory.Where(s=>s.time>=1.05f).All(s=>Mathf.Abs(s.seconds[3]-.1f)<.00001),Is.True);
            var restored=JsonUtility.FromJson<CombatFeedback>(JsonUtility.ToJson(f));
            Assert.AreEqual(f.lootSeconds,restored.lootSeconds);Assert.IsEmpty(restored.activityHistory);
            Assert.IsFalse(JsonUtility.ToJson(f).Contains("activityHistory"));
        }
        [Test] public void TwoShortFarmingIntervalsAccumulateWithAFlatGap()
        {
            var f=new CombatFeedback();f.RecordActivity(CombatActivity.Farming,.4f,.4f);
            f.RecordActivity(null,60,60.4f);f.RecordActivity(CombatActivity.Farming,.4f,60.8f);
            Assert.That(f.lootSeconds,Is.EqualTo(.8f));
            Assert.That(f.activityHistory.First(s=>s.time>=.4f).seconds[3],Is.EqualTo(.4f));
            Assert.That(f.activityHistory.Where(s=>s.time>=.4f&&s.time<=60.4f).All(s=>s.seconds[3]==.4f),Is.True);
            Assert.That(f.activityHistory.Last().seconds[3],Is.EqualTo(.8f));
            for(int i=1;i<f.activityHistory.Count;i++)
            {Assert.Greater(f.activityHistory[i].time,f.activityHistory[i-1].time);Assert.GreaterOrEqual(f.activityHistory[i].seconds[3],f.activityHistory[i-1].seconds[3]);}
        }
        [Test] public void ResumeStartsTheGraphAtSavedCumulativeTotals()
        {
            var f=JsonUtility.FromJson<CombatFeedback>(JsonUtility.ToJson(new CombatFeedback{lootSeconds=.4f}));
            f.RecordActivity(null,1,51);f.RecordActivity(CombatActivity.Farming,.4f,51.4f);
            Assert.AreEqual(50,f.activityHistory[0].time);Assert.AreEqual(.4f,f.activityHistory[0].seconds[3]);
            Assert.AreEqual(.8f,f.activityHistory.Last().seconds[3]);
        }
        [Test] public void LongRunCoarseningKeepsOriginExactTotalsAndMonotonicSamples()
        {
            var f=new CombatFeedback();float time=0;
            for(int i=0;i<20000;i++){time+=.05f;f.RecordActivity((CombatActivity)(i%4),.05f,time);}
            Assert.AreEqual(0,f.activityHistory[0].time);Assert.LessOrEqual(f.activityHistory.Count,CombatFeedback.ActivityHistoryLimit);
            CollectionAssert.AreEqual(f.ActivityDurations(),f.activityHistory.Last().seconds);Assert.AreEqual(time,f.activityHistory.Last().time);
            for(int i=1;i<f.activityHistory.Count;i++)
            {
                Assert.Greater(f.activityHistory[i].time,f.activityHistory[i-1].time);
                for(int j=0;j<4;j++)Assert.GreaterOrEqual(f.activityHistory[i].seconds[j],f.activityHistory[i-1].seconds[j]);
            }
        }
        [Test] public void OldTimingStartsANewObservationWithoutDiscardingSkillEvidence()
        {
            var run=new RunState{time=42,statistics=new CombatStatistics{version=CombatTelemetry.Version,feedback=new CombatFeedback{version=1,attackSeconds=10,movementSeconds=32}}};
            run.statistics.feedback.For("W01").channelTicks=12;CombatTelemetry.Normalize(run);
            Assert.AreEqual(CombatFeedback.Version,run.statistics.feedback.version);Assert.AreEqual(42,run.statistics.feedback.observedFrom);
            Assert.AreEqual(0,run.statistics.feedback.ActivitySeconds);Assert.AreEqual(12,run.statistics.feedback.For("W01").channelTicks);
        }
    }
}
