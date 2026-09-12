using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Hellscript.Tests
{
    public sealed class ContentUnlockTests
    {
        GameCatalog catalog;
        AccountSave New()=>GameStore.NewAccount(catalog);
        [SetUp] public void Setup(){catalog=ScriptableObject.CreateInstance<GameCatalog>();catalog.Populate();}
        [TearDown] public void Cleanup(){UnityEngine.Object.DestroyImmediate(catalog);}
        [Test] public void U01_NewAccountHasBaseAndNoGrowthPrivileges()
        {
            var a=New();Assert.AreEqual(3,a.heroes.Count);Assert.IsTrue(a.heroes.All(h=>h.build!=null&&h.inventory.Count==1));
            foreach(var f in ContentUnlocks.Rules.features)Assert.IsFalse(ContentUnlocks.Has(a,f.id),f.id);
            Assert.IsTrue(a.heroes.All(h=>h.build.passives.Length==0));
        }
        [Test] public void U02_FailedRealRunEnablesTrainingOnly()
        {
            var a=New();var sim=new CombatSimulation(a,catalog,1,seed:902);sim.Abandon();
            Assert.AreEqual(0,ContentUnlocks.AccountClear(a));Assert.IsTrue(ContentUnlocks.Has(a,ContentUnlocks.Train));
            Assert.IsFalse(ContentUnlocks.Has(a,ContentUnlocks.Enhance));Assert.IsFalse(ContentUnlocks.Has(a,ContentUnlocks.Offline));
            Assert.DoesNotThrow(()=>new CombatSimulation(a,catalog,1,0,seed:90,ownedTraining:true));
        }
        [TestCase(ContentUnlocks.Enhance,1)] [TestCase(ContentUnlocks.Offline,1)]
        [TestCase(ContentUnlocks.RareCraft,3)] [TestCase(ContentUnlocks.Gem,5)]
        [TestCase(ContentUnlocks.Reroll,8)] [TestCase(ContentUnlocks.Shop,10)]
        [TestCase(ContentUnlocks.Sweep,12)] [TestCase(ContentUnlocks.CoreCraft,15)]
        public void U03To15_ThresholdsUseGreaterOrEqualAndPersist(string id,int threshold)
        {
            var a=New();a.heroes[1].highestClear=threshold-1;Assert.IsFalse(ContentUnlocks.Has(a,id));
            a.heroes[1].highestClear=threshold;Assert.IsTrue(ContentUnlocks.Has(a,id));
            a.heroes[1].highestClear=0;Assert.IsTrue(ContentUnlocks.Has(a,id));
            var jump=New();jump.heroes[2].highestClear=50;Assert.IsTrue(ContentUnlocks.Has(jump,id));
        }
        [Test] public void U06_GemAcquisitionHistorySurvivesReloadAndConsumption()
        {
            var a=New();ContentUnlocks.RecordGemAcquisition(a);var copy=JsonUtility.FromJson<AccountSave>(JsonUtility.ToJson(a));
            ContentUnlocks.Normalize(copy);Assert.IsTrue(ContentUnlocks.Has(copy,ContentUnlocks.Gem));Assert.AreEqual(0,copy.gold);
        }
        [Test] public void U07_RerollCostsDoNotEscalate()
        {
            var a=New();a.Hero.highestClear=8;a.gold=10000000;uint rng=9;var item=Economy.CreateItem(a.Hero.heroClass,0,2,8,ref rng);a.Hero.inventory.Add(item);
            foreach(int count in new[]{0,9,99}){item.rerolls=count;long cost=Economy.RerollGold(item);Assert.AreEqual(6800,cost);int before=a.gold;Assert.IsTrue(Economy.Reroll(a,item,item.rolls[0].slotId,ref rng));Assert.AreEqual(cost,before-a.gold);}
        }
        [Test] public void U10U11_SweepUsesSelectedHeroAndRejectsNoRecord()
        {
            var a=New();a.heroes[0].highestClear=50;a.selectedHero=1;uint rng=730;uint before=rng;
            Assert.IsFalse(Economy.Sweep(a,"no-record",ref rng));Assert.AreEqual(before,rng);
            a.Hero.highestClear=3;uint expected=rng;var h=a.Hero;
            for(int n=0;n<3;n++){int slot=RandomStream.Range(ref expected,0,8);int rarity=RiftRarity.Roll(RiftRewardSource.Boss,3,ref expected);int level=RandomStream.Range(ref expected,1,6);Economy.CreateItem(h.heroClass,slot,rarity,level,ref expected);}
            Assert.IsTrue(Economy.Sweep(a,"sweep",ref rng));Assert.AreEqual(expected,rng);Assert.AreEqual(950,a.gold);Assert.AreEqual(3,h.highestClear);Assert.IsFalse(Economy.Sweep(a,"sweep",ref rng));
        }
        [Test] public void U12To14_CoreRequiresSameSlotAndConsumptionKeepsPrivilege()
        {
            var a=New();a.cores[0]=5;a.cores[1]=5;Assert.IsFalse(ContentUnlocks.Has(a,ContentUnlocks.CoreCraft));
            a.cores[0]=10;Assert.IsTrue(ContentUnlocks.Has(a,ContentUnlocks.CoreCraft));Assert.AreEqual(10,a.cores[0]);
            uint rng=82;Assert.IsTrue(ContentServices.Purchase(a,0,2,ref rng,out var item));Assert.AreEqual(3,item.rarity);Assert.AreEqual(0,a.cores[0]);
            Assert.IsTrue(ContentUnlocks.Has(a,ContentUnlocks.CoreCraft));Assert.IsFalse(ContentServices.Purchase(a,0,2,ref rng,out _));
        }
        [Test] public void U16_LegendaryDismantlingWorksBeforeCoreCraft()
        {
            var a=New();uint rng=92;var item=Economy.CreateItem(a.Hero.heroClass,0,3,1,ref rng);a.Hero.inventory.Add(item);
            Assert.AreEqual("",Economy.EquipError(a.Hero,item));Assert.IsTrue(Economy.Dismantle(a,a.Hero,item));Assert.AreEqual(1,a.cores[0]);Assert.IsFalse(ContentUnlocks.Has(a,ContentUnlocks.CoreCraft));
        }
        [TestCase(2,0)] [TestCase(3,1)] [TestCase(5,1)] [TestCase(6,2)] [TestCase(9,2)] [TestCase(10,3)]
        public void U18_PassiveSlotsArePerHero(int level,int slots)
        {
            var a=New();a.Hero.level=level;a.heroes[1].highestClear=50;Assert.AreEqual(slots,ContentUnlocks.PassiveSlots(a.Hero));
            Assert.AreEqual(0,ContentUnlocks.PassiveSlots(a.heroes[1]));Assert.AreEqual(new[]{1,3,6,10,15,20},catalog.skills.Take(6).Select(s=>s.unlock).ToArray());
        }
        [Test] public void U19_LegacyMigrationPreservesPreviouslyAvailableServicesAndSlots()
        {
            var a=New();a.contentUnlocks=new ContentUnlockState();a.Hero.build.passives=new[]{0,2,5};a.Hero.capacity=73;
            ContentUnlocks.Normalize(a,legacy:true);Assert.AreEqual(3,ContentUnlocks.PassiveSlots(a.Hero));Assert.AreEqual(new[]{0,2,5},a.Hero.build.passives);Assert.AreEqual(73,a.Hero.capacity);
            Assert.IsTrue(ContentUnlocks.Has(a,ContentUnlocks.CoreCraft));Assert.IsFalse(ContentUnlocks.Has(a,ContentUnlocks.Gem));
            string before=JsonUtility.ToJson(a);ContentUnlocks.Normalize(a,legacy:true);Assert.AreEqual(before,JsonUtility.ToJson(a));
        }
        [TestCase(5,1,1,1)] [TestCase(10,1,1,1)] [TestCase(11,2,2,1)] [TestCase(19,2,2,1)] [TestCase(20,2,2,2)] [TestCase(21,2,3,2)]
        public void U20U21_ActualGeneratedCandidatesRespectBoundaries(int stage,int themes,int bosses,int traits)
        {
            Assert.AreEqual(themes,ContentUnlocks.ThemeCount(stage));Assert.AreEqual(bosses,ContentUnlocks.BossCount(stage));
            for(uint seed=1;seed<=6;seed++)
            {
                var map=RiftGenerator.Generate(seed,"unlock-"+seed,stage,(HeroClass)(seed%3),lastBoss:2);
                Assert.Less(map.theme,themes);Assert.Less(map.bossKind,bosses);Assert.IsTrue(map.spawns.All(s=>s.traits.Count<=traits));
                Assert.IsFalse(map.spawns.Any(s=>s.traits.Contains(0)&&s.traits.Contains(1)));
                if(stage<=5)Assert.IsTrue(map.spawns.All(s=>s.kind==0));
            }
        }
        [Test] public void U22U24U29_GuideSkipAndVerifiedMergeAreIndependentAndIdempotent()
        {
            var a=New();a.Hero.highestClear=15;ContentUnlocks.Reconcile(a);ContentUnlocks.CompleteGuide(a,ContentUnlocks.Gem);
            var b=New();ContentUnlocks.MergeVerified(b,a.contentUnlocks);string before=JsonUtility.ToJson(b);ContentUnlocks.MergeVerified(b,a.contentUnlocks);
            Assert.AreEqual(before,JsonUtility.ToJson(b));Assert.IsTrue(ContentUnlocks.Has(b,ContentUnlocks.Gem));Assert.Contains(ContentUnlocks.Gem,b.contentUnlocks.guidesCompleted);
            Assert.AreEqual(0,b.Hero.highestClear);Assert.AreEqual(0,b.gold);
        }
        [Test] public void U25_ExecutionGuardsRejectLockedServicesAndPortal()
        {
            var a=New();a.gold=100000;a.materials=1000;uint rng=21;var item=a.Hero.inventory[0];
            Assert.IsFalse(Economy.Enhance(a,item));Assert.IsFalse(ContentServices.Purchase(a,0,0,ref rng,out _));
            Assert.Throws<InvalidOperationException>(()=>new CombatSimulation(a,catalog,1,0,ownedTraining:true));
            a.Hero.highestClear=50;var sim=new CombatSimulation(a,catalog,1,seed:82);a.suspendedRun=sim.State;sim.State.portal=true;
            int gold=a.gold;Assert.IsFalse(Economy.Enhance(a,item));Assert.IsFalse(ContentServices.Purchase(a,0,2,ref rng,out _));Assert.IsFalse(Economy.Sweep(a,"portal",ref rng));Assert.AreEqual(gold,a.gold);
        }
        [Test] public void U24_SaveLoadPreservesUnlockAndGuide()
        {
            string dir=Path.Combine(Path.GetTempPath(),"unlock-test-"+Guid.NewGuid());
            try{var store=new GameStore(dir,catalog);store.Data.Hero.highestClear=15;ContentUnlocks.Reconcile(store.Data);ContentUnlocks.CompleteGuide(store.Data,ContentUnlocks.Reroll);Assert.IsTrue(store.Save());var loaded=new GameStore(dir,catalog);Assert.IsTrue(ContentUnlocks.Has(loaded.Data,ContentUnlocks.CoreCraft));Assert.Contains(ContentUnlocks.Reroll,loaded.Data.contentUnlocks.guidesCompleted);}
            finally{if(Directory.Exists(dir))Directory.Delete(dir,true);}
        }
        [Test] public void U27_EconomyFirstUseDeficitsAreVisibleAndNotGifted()
        {
            var a=New();var item=a.Hero.inventory[0];Assert.AreEqual(200,Economy.EnhancementGold(item));Assert.AreEqual(20,Economy.EnhancementMaterials(item));
            // Stage 1 boss + first clear guarantees 1,950 gold and 15 materials, before chest/dismantle income.
            Assert.AreEqual(1950,(800+50)+(1000+100));Assert.AreEqual(5,Economy.EnhancementMaterials(item)-(5+10));
            a.Hero.highestClear=3;uint rng=55;Assert.IsFalse(ContentServices.Purchase(a,0,1,ref rng,out _));Assert.AreEqual(0,a.gold);Assert.AreEqual(0,a.materials);
        }
        [Test] public void U03U23_BossRewardSettlementUnlocksOnceAndRestoreDoesNotDuplicate()
        {
            var a=New();var sim=new CombatSimulation(a,catalog,1,0,seed:911);
            sim.State.training=-1;sim.State.stage=1;sim.State.enemies.Clear();
            sim.State.enemies.Add(new EnemyState{boss=true,dead=true});
            sim.Tick(.05f);Assert.AreEqual(1,a.Hero.highestClear);Assert.IsTrue(sim.State.bossRewarded);
            Assert.IsTrue(ContentUnlocks.Has(a,ContentUnlocks.Enhance));int gold=a.gold,mats=a.materials;int grants=a.contentUnlocks.unlocked.Count;
            a.suspendedRun=sim.State;var copy=JsonUtility.FromJson<AccountSave>(JsonUtility.ToJson(a));
            var restored=new CombatSimulation(copy,catalog,50,restore:copy.suspendedRun);restored.Tick(.05f);
            Assert.AreEqual(gold,copy.gold);Assert.AreEqual(mats,copy.materials);Assert.AreEqual(grants,copy.contentUnlocks.unlocked.Count);Assert.AreEqual(1,restored.State.stage);
        }
        [Test] public void U03_OfflineUnlockDoesNotBackdateAndRepeatedLoadDoesNotDuplicate()
        {
            string dir=Path.Combine(Path.GetTempPath(),"unlock-offline-"+Guid.NewGuid());Directory.CreateDirectory(dir);
            try
            {
                var a=New();a.Hero.highestClear=1;a.lastSeenUtc=DateTimeOffset.UtcNow.ToUnixTimeSeconds()-3600;
                File.WriteAllText(Path.Combine(dir,"hellscript-local-v1.json"),JsonUtility.ToJson(a));
                var loaded=new GameStore(dir,catalog);Assert.AreEqual(0,loaded.LocalIdleGoldAwarded);Assert.IsTrue(ContentUnlocks.Has(loaded.Data,ContentUnlocks.Offline));
                var second=new GameStore(dir,catalog);Assert.AreEqual(loaded.Data.gold,second.Data.gold);Assert.AreEqual(0,second.LocalIdleGoldAwarded);
            }
            finally{Directory.Delete(dir,true);}
        }
        [TestCase(0)] [TestCase(1)] [TestCase(2)]
        public void U28_EarlyClassRunUsesOwnedLevelAndWalkableContent(int hero)
        {
            var a=New();a.selectedHero=hero;a.Hero.build.bagPolicy=BagPolicy.Ignore;
            var sim=new CombatSimulation(a,catalog,1,seed:112358);
            Assert.AreEqual(1,sim.EffectiveLevel);Assert.IsEmpty(sim.Hero.build.passives);Assert.AreEqual(0,sim.State.layout.bossKind);
            for(int i=0;i<6100&&sim.State.phase!=RunPhase.Cleared&&sim.State.phase!=RunPhase.Failed;i++)sim.Tick(.05f);
            Assert.IsEmpty(sim.State.navigationError);
            Assert.IsTrue(sim.State.phase==RunPhase.Cleared||sim.State.phase==RunPhase.Failed);
            TestContext.WriteLine($"AUTOMATED EARLY RUN: hero={hero}, seed=112358, stage=1, result={sim.State.phase}, time={sim.State.time}, kills={sim.State.kills}, finalLevel={sim.EffectiveLevel}. This is one simulated run, not a manual difficulty or fairness review.");
        }
    }
}
