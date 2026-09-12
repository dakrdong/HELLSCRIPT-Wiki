using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hellscript.Tests
{
    public sealed class TrainingComparisonTests
    {
        GameCatalog catalog;
        [SetUp] public void Setup(){catalog=ScriptableObject.CreateInstance<GameCatalog>();catalog.Populate();}
        [TearDown] public void Teardown()=>UnityEngine.Object.DestroyImmediate(catalog);
        AccountSave Account(int hero=2)
        {var a=ContentTestAccounts.Training(catalog);a.selectedHero=hero;a.Hero.level=30;a.Hero.build=BehaviorPresets.ForLevel((HeroClass)hero,0,30,catalog);return a;}
        static void Complete(CombatSimulation sim,float speed=1)
        {
            float accumulator=0;
            for(int frame=0;frame<4000&&sim.State.phase!=RunPhase.Cleared&&sim.State.phase!=RunPhase.Failed;frame++)
            {sim.State.realTime+=1f/60;accumulator+=speed/60;while(accumulator>=CombatSimulation.Step){sim.Tick(CombatSimulation.Step);accumulator-=CombatSimulation.Step;}}
            Assert.IsTrue(sim.State.phase==RunPhase.Cleared||sim.State.phase==RunPhase.Failed);
        }
        static string WithoutReports(AccountSave original)
        {var a=JsonUtility.FromJson<AccountSave>(JsonUtility.ToJson(original));a.lastSeenUtc=0;foreach(var hero in a.heroes)hero.trainingComparison=null;return JsonUtility.ToJson(a);}
        TrainingComparisonRecord Pair(AccountSave account,int training=1)
        {
            var pair=new TrainingComparisonSession(account,catalog,training);var a=pair.StartA();Complete(a);Assert.IsTrue(pair.AcceptCompleted(a,out string error),error);
            var b=pair.StartB(pair.A.build);Complete(b);Assert.IsTrue(pair.AcceptCompleted(b,out error),error);return pair.Record();
        }
        [Test]
        public void SameBuildHasTheSameFullCombatAtEveryClassFixtureAndSpeed([Values(0,1,2)]int hero,[Values(0,1,2)]int training,[Values(1f,1.5f,2f)]float speed)
        {
            var account=Account(hero);string before=JsonUtility.ToJson(account);var pair=new TrainingComparisonSession(account,catalog,training);
            var a=pair.StartA();uint initialRng=a.State.rng;Complete(a,1);Assert.IsTrue(pair.AcceptCompleted(a,out string error),error);
            var b=pair.StartB(pair.A.build);Assert.AreEqual(initialRng,b.State.rng);Assert.AreEqual(0,b.State.time);Assert.AreEqual(b.Stats.hp,b.State.health);Assert.AreEqual(100,b.State.resource);
            Assert.IsTrue(b.State.cooldowns.All(c=>c==0));Assert.IsEmpty(b.State.projectiles);Assert.IsEmpty(b.State.shields);Assert.AreEqual(0,b.State.statistics.damage);
            Complete(b,speed);Assert.IsTrue(pair.AcceptCompleted(b,out error),error);var record=pair.Record();Assert.IsTrue(record.Complete);Assert.IsEmpty(record.changes);
            Assert.AreEqual(JsonUtility.ToJson(record.a.statistics),JsonUtility.ToJson(record.b.statistics));Assert.AreEqual(a.State.rng,b.State.rng);Assert.AreEqual(record.a.seconds,record.b.seconds);
            Assert.AreEqual(before,JsonUtility.ToJson(account));Assert.AreNotEqual(record.a.runId,record.b.runId);
        }
        [Test]
        public void ChangedRuleProducesAnActualBehaviorDifferenceAndACompleteChangeList()
        {
            var pair=new TrainingComparisonSession(Account(),catalog,1);var a=pair.StartA();Complete(a);Assert.IsTrue(pair.AcceptCompleted(a,out _));
            var edit=pair.A.build;foreach(var rule in edit.rules.Where(r=>r.skill==12))rule.enabled=false;edit.potionThreshold=25;edit.distance=5.5f;
            var b=pair.StartB(edit);Complete(b);Assert.IsTrue(pair.AcceptCompleted(b,out _));var record=pair.Record();
            Assert.Greater(record.a.statistics.For("M01").starts,0);Assert.AreEqual(0,record.b.statistics.For("M01").starts);
            Assert.IsTrue(record.changes.Any(x=>x.Contains("화염구")));Assert.IsTrue(record.changes.Any(x=>x.StartsWith("물약:")));Assert.IsTrue(record.changes.Any(x=>x.StartsWith("이동:")));
        }
        [Test]
        public void AllChangedRowsAndEmptyLoadoutPassiveNamesUseTheCorrectClass()
        {
            var before=Account().Hero.build;var after=before.Copy();after.rules[0].enabled=!after.rules[0].enabled;after.rules[1].enabled=!after.rules[1].enabled;
            var changes=BuildEditing.DescribeCombatChanges(before,after,catalog,HeroClass.Mage);Assert.IsTrue(changes.Any(x=>x.StartsWith("1번")));Assert.IsTrue(changes.Any(x=>x.StartsWith("2번")));
            before.activeSkills.Clear();after=before.Copy();before.passives=new[]{0};after.passives=new[]{1};changes=BuildEditing.DescribeCombatChanges(before,after,catalog,HeroClass.Mage);
            StringAssert.Contains(GameCatalog.Passives[12],changes.Single());StringAssert.Contains(GameCatalog.Passives[13],changes.Single());
        }
        [Test]
        public void AccountGrowthAndEquipmentChangesDoNotReplaceTheCapturedBaseline()
        {
            var account=Account();var pair=new TrainingComparisonSession(account,catalog,0);var original=pair.Hero;var a=pair.StartA();Complete(a);Assert.IsTrue(pair.AcceptCompleted(a,out _));
            account.Hero.level=1;account.Hero.inventory[0].enhancement=5;account.selectedHero=0;var b=pair.StartB(pair.A.build);
            Assert.AreEqual(original.id,b.Hero.id);Assert.AreEqual(original.level,b.EffectiveLevel);Assert.AreEqual(original.inventory[0].enhancement,b.Hero.inventory[0].enhancement);
            Complete(b);Assert.IsTrue(pair.AcceptCompleted(b,out _));Assert.AreEqual(original.id,pair.Record().heroId);
        }
        [TestCase(0)][TestCase(1)][TestCase(2)][TestCase(3)]
        public void EditedAbandonedIncompleteOrChangedEquipmentAttemptsAreRejected(int failure)
        {
            var pair=new TrainingComparisonSession(Account(),catalog,1);var sim=pair.StartA();sim.Tick(.05f);
            if(failure==0){var edit=sim.State.build.Copy();edit.distance=5.5f;Assert.IsTrue(sim.ApplyBuild(edit));Complete(sim);}
            if(failure==1)sim.Abandon();
            if(failure==3){sim.Hero.inventory[0].enhancement++;Complete(sim);}
            Assert.IsFalse(pair.AcceptCompleted(sim,out string error));Assert.IsNotEmpty(error);Assert.IsFalse(pair.HasA);Assert.IsNull(pair.Record());
        }
        [Test]
        public void WrongSessionAndStartingBTooSoonCannotSupplyAComparisonResult()
        {
            var account=Account();var pair=new TrainingComparisonSession(account,catalog,0);Assert.Throws<InvalidOperationException>(()=>pair.StartB(account.Hero.build));
            var a=pair.StartA();Assert.Throws<InvalidOperationException>(()=>pair.StartA());Assert.Throws<InvalidOperationException>(()=>pair.StartB(account.Hero.build));
            var foreign=new CombatSimulation(account,catalog,1,0,ownedTraining:true);Complete(foreign);Assert.IsFalse(pair.AcceptCompleted(foreign,out _));
            a.Abandon();Assert.IsFalse(pair.AcceptCompleted(a,out _));a=pair.StartA();Complete(a);Assert.IsTrue(pair.AcceptCompleted(a,out _));
        }
        [Test]
        public void LockedSkillCannotBeEnabledInBAndInvalidRequestKeepsA()
        {
            var account=ContentTestAccounts.Training(catalog);account.selectedHero=2;var pair=new TrainingComparisonSession(account,catalog,1);var a=pair.StartA();Complete(a);Assert.IsTrue(pair.AcceptCompleted(a,out _));
            var edit=pair.A.build;edit.rules.First(r=>r.skill==15).enabled=true;
            Assert.IsNotEmpty(pair.ValidateB(edit));Assert.Throws<InvalidOperationException>(()=>pair.StartB(edit));Assert.IsTrue(pair.HasA);Assert.IsFalse(pair.HasB);
        }
        [Test]
        public void MetadataAndRepeatOnlyChangesDoNotClaimCombatChanges()
        {
            var before=Account().Hero.build;var after=before.Copy();after.name="named B";after.version="new metadata";after.autoRepeat=!after.autoRepeat;foreach(var r in after.rules)r.id=Guid.NewGuid().ToString("N");
            Assert.AreEqual(BuildEditing.CombatSignature(before),BuildEditing.CombatSignature(after));Assert.IsEmpty(BuildEditing.DescribeCombatChanges(before,after,catalog,HeroClass.Mage));
        }
        [Test]
        public void CompletedReportsAreCopiesAndRepeatedAcceptanceIsIdempotent()
        {
            var pair=new TrainingComparisonSession(Account(),catalog,1);var a=pair.StartA();Complete(a);Assert.IsTrue(pair.AcceptCompleted(a,out _));var b=pair.StartB(pair.A.build);Complete(b);Assert.IsTrue(pair.AcceptCompleted(b,out _));
            var record=pair.Record();string frozen=JsonUtility.ToJson(record);record.b.statistics.damage=999;record.a.build.distance=9;
            Assert.AreEqual(frozen,JsonUtility.ToJson(pair.Record()));Assert.IsTrue(pair.AcceptCompleted(b,out _));Assert.AreEqual(frozen,JsonUtility.ToJson(pair.Record()));
            Assert.AreNotSame(pair.A,pair.A);Assert.AreNotSame(pair.Hero,pair.Hero);
        }
        [Test]
        public void DiskSaveRestoreKeepsOneOwnedReportAndLeavesProgressAndCurrentBuildAlone()
        {
            string directory=Path.Combine(Path.GetTempPath(),"hellscript-comparison-"+Guid.NewGuid().ToString("N"));
            try
            {
                var store=new GameStore(directory,catalog);ContentUnlocks.RecordRunEnd(store.Data);store.Save();var record=Pair(store.Data);string before=WithoutReports(store.Data);Assert.IsTrue(store.SaveTrainingComparison(record),store.Error);
                Assert.AreEqual(before,WithoutReports(store.Data));var loaded=new GameStore(directory,catalog);Assert.AreEqual(record.id,loaded.Data.Hero.trainingComparison.id);
                Assert.AreEqual(JsonUtility.ToJson(record),JsonUtility.ToJson(loaded.Data.Hero.trainingComparison));Assert.AreEqual(before,WithoutReports(loaded.Data));
                Assert.IsTrue(loaded.Data.heroes.Skip(1).All(h=>h.trainingComparison==null));
                Assert.IsTrue(loaded.Transact("after-comparison","test-report-preservation",a=>{a.gold++;return true;}));Assert.AreEqual(record.id,new GameStore(directory,catalog).Data.Hero.trainingComparison.id);
            }
            finally{if(Directory.Exists(directory))Directory.Delete(directory,true);}
        }
        [Test]
        public void SaveFailureRestoresThePreviousReportAndDoesNotChangeItsContents()
        {
            string directory=Path.Combine(Path.GetTempPath(),"hellscript-comparison-failure-"+Guid.NewGuid().ToString("N"));
            try
            {
                var store=new GameStore(directory,catalog);ContentUnlocks.RecordRunEnd(store.Data);store.Save();var first=Pair(store.Data);Assert.IsTrue(store.SaveTrainingComparison(first));var second=Pair(store.Data);string original=JsonUtility.ToJson(store.Data.Hero.trainingComparison);
                Directory.CreateDirectory(Path.Combine(directory,"hellscript-local-v1.json.tmp"));LogAssert.Expect(LogType.Error,new System.Text.RegularExpressions.Regex("저장하지 못했습니다:"));
                Assert.IsFalse(store.SaveTrainingComparison(second));Assert.AreEqual(original,JsonUtility.ToJson(store.Data.Hero.trainingComparison));Assert.IsNotEmpty(store.Error);
                Directory.Delete(Path.Combine(directory,"hellscript-local-v1.json.tmp"));Assert.AreEqual(first.id,new GameStore(directory,catalog).Data.Hero.trainingComparison.id);
            }
            finally{if(Directory.Exists(directory))Directory.Delete(directory,true);}
        }
        [Test]
        public void SaveRejectsAnotherHeroIncompleteReportAndPreservedRift()
        {
            string directory=Path.Combine(Path.GetTempPath(),"hellscript-comparison-owner-"+Guid.NewGuid().ToString("N"));
            try
            {
                var store=new GameStore(directory,catalog);ContentUnlocks.RecordRunEnd(store.Data);store.Save();var record=Pair(store.Data);store.Data.selectedHero=1;Assert.IsFalse(store.SaveTrainingComparison(record));store.Data.selectedHero=0;
                record.b.statistics.finish=CombatFinish.Abandoned;Assert.IsFalse(store.SaveTrainingComparison(record));record.b.statistics.finish=CombatFinish.Duration;
                store.Data.suspendedRun=new RunState{id="active",heroId=store.Data.Hero.id};Assert.IsFalse(store.SaveTrainingComparison(record));Assert.Throws<InvalidOperationException>(()=>new TrainingComparisonSession(store.Data,catalog,0));
            }
            finally{if(Directory.Exists(directory))Directory.Delete(directory,true);}
        }
        [Test]
        public void UnknownReportVersionIsPreservedAsUnsupportedInsteadOfRewrittenAsZeroes()
        {
            string directory=Path.Combine(Path.GetTempPath(),"hellscript-comparison-version-"+Guid.NewGuid().ToString("N"));
            try
            {
                var store=new GameStore(directory,catalog);ContentUnlocks.RecordRunEnd(store.Data);store.Save();var record=Pair(store.Data);record.version=99;store.Data.Hero.trainingComparison=record;Assert.IsTrue(store.Save());
                var loaded=new GameStore(directory,catalog);Assert.AreEqual(record.id,loaded.Data.Hero.trainingComparison.id);Assert.AreEqual(99,loaded.Data.Hero.trainingComparison.version);Assert.IsFalse(loaded.Data.Hero.trainingComparison.Supported);
            }
            finally{if(Directory.Exists(directory))Directory.Delete(directory,true);}
        }
    }
}
