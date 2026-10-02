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
        [Test] public void GraphSplitsBoundaryTicksIsBoundedAndIsNotWrittenToSaves()
        {
            var f=new CombatFeedback();f.RecordActivity(CombatActivity.Farming,.1f,1.05f);
            Assert.That(f.activityHistory[0].seconds[3],Is.EqualTo(.05).Within(.00001));
            Assert.That(f.activityHistory[1].seconds[3],Is.EqualTo(.05).Within(.00001));
            f.RecordActivity(null,70,71.05f);Assert.AreEqual(CombatFeedback.ActivityHistoryLimit,f.activityHistory.Count);
            var restored=JsonUtility.FromJson<CombatFeedback>(JsonUtility.ToJson(f));
            Assert.AreEqual(f.lootSeconds,restored.lootSeconds);Assert.IsEmpty(restored.activityHistory);
            Assert.IsFalse(JsonUtility.ToJson(f).Contains("activityHistory"));
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
