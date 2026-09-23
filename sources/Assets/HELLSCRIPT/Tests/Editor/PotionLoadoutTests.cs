using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hellscript.Tests
{
    public sealed class PotionLoadoutTests
    {
        GameStore store;string directory;GameCatalog catalog;
        HeroSave Hero=>store.Data.Hero;
        [SetUp] public void Setup()
        {
            directory=Path.Combine(Path.GetTempPath(),"potion-loadout-"+Guid.NewGuid().ToString("N"));store=new GameStore(directory);
            catalog=ScriptableObject.CreateInstance<GameCatalog>();catalog.Populate();Hero.potions.Activate();
            Hero.potions.Set("PU01",3);Hero.potions.Set("PU02",3);Hero.potions.Set("PU05",3);
        }
        [TearDown] public void Cleanup(){UnityEngine.Object.DestroyImmediate(catalog);Directory.Delete(directory,true);Loc.UseSource();}
        [Test] public void LegacySelectionIsReadOnlyAndExplicitSlotsReplaceOnlyTheirOwnPosition()
        {
            Hero.useEdict=true;Hero.edict=HuntEdictV2Editing.WithGlobal(Hero.edict,"potion.utility","PU02");
            string before=JsonUtility.ToJson(Hero.potions);Assert.AreEqual("PU02",PotionPolicy.Resolve(Hero).Slots[2].id);Assert.AreEqual(before,JsonUtility.ToJson(Hero.potions));
            Assert.IsTrue(store.SetPotionSlot("assign",Hero.id,1,"PU01"),store.Error);
            CollectionAssert.AreEqual(new[]{"PH01","PU01","PU02"},PotionPolicy.Resolve(Hero).Slots.Select(s=>s.id));
            Assert.IsTrue(store.SetPotionSlot("assign",Hero.id,1,"PU01"));
            Assert.IsFalse(store.SetPotionSlot("duplicate",Hero.id,0,"PU01"));
            Assert.IsFalse(store.SetPotionSlot("equipment",Hero.id,0,"B01"));
            Assert.IsFalse(store.SetPotionSlot("unowned",Hero.id,0,"PU06"));
            Assert.IsFalse(store.SetPotionSlot("bad-index",Hero.id,3,"PU01"));
            CollectionAssert.AreEqual(new[]{"PH01","PU01","PU02"},new GameStore(directory).Data.Hero.potions.slots.Select(s=>s.id));
        }
        [TestCase(PotionFallback.HigherGrade)][TestCase(PotionFallback.LowerGrade)][TestCase(PotionFallback.Newest)][TestCase(PotionFallback.Oldest)]
        public void SharedFallbackPersistsWithoutChangingSlotsConsumingOrSpending(PotionFallback choice)
        {
            string stock=string.Join(";",Hero.potions.stacks.Select(s=>s.id+":"+s.count));int gold=store.Data.gold;
            var assigned=PotionPolicy.Resolve(Hero).Slots.Select(s=>s.id).ToArray();
            Assert.IsTrue(store.SetPotionFallback("policy",Hero.id,choice),store.Error);
            var loaded=new GameStore(directory);Assert.AreEqual(choice,loaded.Data.Hero.potions.SharedFallback);
            CollectionAssert.AreEqual(assigned,PotionPolicy.Resolve(loaded.Data.Hero).Slots.Select(s=>s.id));Assert.AreEqual(gold,loaded.Data.gold);
            Assert.AreEqual(PotionFallback.HigherGrade,loaded.Data.heroes[1].potions.SharedFallback);
            Assert.AreEqual(stock,string.Join(";",loaded.Data.Hero.potions.stacks.Select(s=>s.id+":"+s.count)));
            Assert.IsFalse(store.SetPotionFallback("invalid",Hero.id,(PotionFallback)99));
        }
        [Test] public void FailedPolicySavePreservesDiskAndMemory()
        {
            Assert.IsTrue(store.Save());string before=JsonUtility.ToJson(Hero.potions);string path=Path.Combine(directory,"hellscript-local-v1.json"),disk=File.ReadAllText(path);
            Directory.CreateDirectory(path+".tmp");LogAssert.Expect(LogType.Error,new Regex("저장하지 못했습니다:"));
            Assert.IsFalse(store.SetPotionFallback("fail",Hero.id,PotionFallback.Oldest));
            Assert.AreEqual(before,JsonUtility.ToJson(Hero.potions));Assert.AreEqual(disk,File.ReadAllText(path));
        }
        PotionDefinition[] TierFixture()
        {
            // Catalog-shaped fixtures exercise future grades without adding unapproved live potion effects.
            return PotionCatalog.All.Select(p=>new PotionDefinition{id=p.id,effect=p.id=="PU01"||p.id=="PU02"||p.id=="PU05"?"hp":p.effect,grade=p.id=="PU01"?1:p.id=="PU02"?3:2}).ToArray();
        }
        [TestCase(PotionFallback.HigherGrade,"PU02")][TestCase(PotionFallback.LowerGrade,"PU01")]
        [TestCase(PotionFallback.Newest,"PU05")][TestCase(PotionFallback.Oldest,"PU01")]
        public void ExhaustionUsesSameEffectAndRequestedOrder(PotionFallback rule,string expected)
        {
            Assert.IsTrue(store.SetPotionFallback("fallback",Hero.id,rule));var slots=PotionLoadout.Defaults();Hero.potions.Set("PH01",1);
            Assert.AreEqual("PH01",PotionLoadout.Resolve(Hero.potions,slots,TierFixture())[0]);Hero.potions.Consume("PH01");
            string before=JsonUtility.ToJson(Hero.potions);Assert.AreEqual(expected,PotionLoadout.Resolve(Hero.potions,slots,TierFixture())[0]);Assert.AreEqual(before,JsonUtility.ToJson(Hero.potions));
            Assert.IsNull(PotionLoadout.Resolve(Hero.potions,slots)[0],"Live catalog has no same-effect replacement; never substitute a buff for healing.");
        }
        [TestCase(PotionFallback.HigherGrade)][TestCase(PotionFallback.LowerGrade)]
        [TestCase(PotionFallback.Newest)][TestCase(PotionFallback.Oldest)]
        public void OnePolicyDrivesEverySlotEvenWhenLegacyPerSlotRulesDiffer(PotionFallback choice)
        {
            Assert.IsTrue(store.SetPotionFallback("shared",Hero.id,choice));Hero.potions.Set("PH01",0);
            foreach(int index in Enumerable.Range(0,3))
            {
                var slots=new[]{new PotionSlot(),new PotionSlot(),new PotionSlot()};
                slots[index]=new PotionSlot{id="PH01",fallback=(PotionFallback)(((int)choice+1)%4)};
                string expected=choice==PotionFallback.HigherGrade?"PU02":choice==PotionFallback.Newest?"PU05":"PU01";
                Assert.AreEqual(expected,PotionLoadout.Resolve(Hero.potions,slots,TierFixture())[index]);
            }
        }
        [TestCase(false)][TestCase(true)]
        public void LegacyPerSlotPolicyMigratesOnceFromFirstAssignedSlot(bool emptyFirst)
        {
            var stock=JsonUtility.FromJson<PotionInventory>("{\"version\":1,\"slots\":[{\"id\":\"PH01\",\"fallback\":2},{\"id\":\"PM01\",\"fallback\":1},{\"id\":\"PU04\",\"fallback\":3}]}");
            if(emptyFirst)stock.slots[0].id="";
            var expected=emptyFirst?PotionFallback.LowerGrade:PotionFallback.Newest;string before=JsonUtility.ToJson(stock);
            Assert.AreEqual(expected,stock.SharedFallback);Assert.AreEqual(before,JsonUtility.ToJson(stock));
            stock.Validate();Assert.AreEqual(expected,stock.fallback);Assert.AreEqual(1,stock.fallbackVersion);
            stock.slots[emptyFirst?1:0].fallback=PotionFallback.Oldest;stock.Validate();Assert.AreEqual(expected,stock.SharedFallback);
            Assert.AreEqual(expected,JsonUtility.FromJson<PotionInventory>(JsonUtility.ToJson(stock)).SharedFallback);
        }
        [Test] public void ReservedAndAlreadyResolvedPotionsAreNotReusedByOtherSlots()
        {
            Hero.potions.Set("PH01",0);Hero.potions.Set("PM01",0);
            var defs=TierFixture();defs.Single(d=>d.id=="PM01").effect="hp";
            var slots=PotionLoadout.Defaults("PU02");var ids=PotionLoadout.Resolve(Hero.potions,slots,defs);
            Assert.AreEqual("PU05",ids[0]);Assert.AreEqual("PU01",ids[1]);Assert.AreEqual("PU02",ids[2]);
            Assert.AreEqual(3,ids.Distinct().Count());
        }
        [Test] public void AcquisitionBatchesTrackRemainingUnitsAndSurviveReload()
        {
            Hero.potions.Set("PU01",4);int latest=Hero.potions.Acquired("PU01",true),oldest=Hero.potions.Acquired("PU01",false);
            Assert.Greater(latest,oldest);Hero.potions.Consume("PU01",true);Assert.AreEqual(oldest,Hero.potions.Acquired("PU01",true));
            Hero.potions.Set("PU01",4);latest=Hero.potions.Acquired("PU01",true);
            for(int i=0;i<3;i++)Hero.potions.Consume("PU01");Assert.AreEqual(latest,Hero.potions.Acquired("PU01",false));
            Assert.IsTrue(store.Save());var stock=new GameStore(directory).Data.Hero.potions;
            Assert.AreEqual(1,stock.Count("PU01"));Assert.AreEqual(latest,stock.Acquired("PU01",false));stock.Validate();
        }
        [Test] public void OldInventoryMigratesOnceWithoutInventingHistoricalTimestamps()
        {
            var stock=JsonUtility.FromJson<PotionInventory>("{\"version\":1,\"stacks\":[{\"id\":\"PU02\",\"count\":2},{\"id\":\"PH01\",\"count\":3}]}");
            stock.Validate();Assert.AreEqual(2,stock.Count("PU02"));Assert.Less(stock.Acquired("PU02",false),stock.Acquired("PH01",false));
            string before=JsonUtility.ToJson(stock);stock.Validate();Assert.AreEqual(before,JsonUtility.ToJson(stock));
        }
        [Test] public void LoadoutDrivesRealConsumptionAndTrainingRemainsIsolated()
        {
            Hero.level=24;Hero.useEdict=false;
            Assert.IsTrue(store.SetPotionSlot("utility1",Hero.id,0,"PU01"));Assert.IsTrue(store.SetPotionSlot("utility2",Hero.id,1,"PU02"));
            var sim=new CombatSimulation(store.Data,catalog,1,seed:7421);sim.State.health=sim.Stats.hp*.1f;
            bool used=(bool)typeof(CombatSimulation).GetMethod("TryUsePotion",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(sim,new object[]{100f});Assert.IsFalse(used,"Unassigned healing potion must not be consumed.");
            int count=Hero.potions.Count("PU01");Assert.IsTrue(sim.TryUseEquippedUtility());Assert.AreEqual(count-1,Hero.potions.Count("PU01"));Assert.AreEqual("PU01",sim.State.potions.utilityId);
            Assert.IsFalse(sim.TryUseEquippedUtility(),"Slot changes must not bypass shared utility cooldown.");
            store.Data.suspendedRun=sim.State;Assert.IsFalse(store.SetPotionSlot("in-run",Hero.id,0,"PH01"));
            Assert.IsTrue(store.SetPotionFallback("in-run-policy",Hero.id,PotionFallback.Oldest));Assert.AreEqual(PotionFallback.Oldest,sim.Hero.potions.SharedFallback);
            store.Data.suspendedRun=null;
            var training=new CombatSimulation(store.Data,catalog,1,0,ownedTraining:true);count=Hero.potions.Count("PU01");Assert.IsTrue(training.TryUseEquippedUtility());Assert.AreEqual(count,Hero.potions.Count("PU01"));
        }
        [Test] public void RestockAndDepartureUseConfiguredEffects()
        {
            Assert.IsTrue(store.SetPotionSlot("nohp",Hero.id,0,"PU01"));Hero.potions.Set("PH01",0);
            var policy=PotionPolicy.Resolve(Hero);Assert.IsEmpty(policy.DepartureReason(Hero.potions));
            Hero.potions.Set("PU01",0);store.Data.gold=1000;policy.budget=45;policy.reserve=0;
            Assert.AreEqual(45,PotionRestock.Apply(store.Data,Hero,"configured",policy));Assert.AreEqual(3,Hero.potions.Count("PU01"));Assert.AreEqual(0,Hero.potions.Count("PH01"));
        }
    }
}
