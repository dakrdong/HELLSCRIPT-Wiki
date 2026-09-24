using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hellscript.Tests
{
    public sealed class CoreCraftingTests
    {
        string directory;GameCatalog catalog;GameStore store;
        AccountSave A=>store.Data;HeroSave H=>A.Hero;
        string Recipe=>CoreCrafting.Recipes.First(r=>r.slot==0&&r.heroClass==0&&string.IsNullOrEmpty(r.setId)).id;
        [SetUp] public void Setup()
        {
            directory=Path.Combine(Path.GetTempPath(),"hellscript-core-craft-"+Guid.NewGuid().ToString("N"));
            catalog=ScriptableObject.CreateInstance<GameCatalog>();catalog.Populate();store=new GameStore(directory,catalog);
            H.highestClear=30;A.premium=12000;for(int n=0;n<8;n++)A.cores[n]=20;ContentUnlocks.Reconcile(A);
        }
        [TearDown] public void Cleanup(){UnityEngine.Object.DestroyImmediate(catalog);if(Directory.Exists(directory))Directory.Delete(directory,true);}
        CoreCraftQuote Quote(int investment=0)=>CoreCrafting.Quote(A,Recipe,investment);
        [Test] public void IntegratedSaveKeepsCraftingFatigueAndRetiredGemMigrationTogether()
        {
            Assert.IsTrue(store.CraftCoreEquipment("integrated-craft",Quote(8000),19),store.Error);
            string itemId=A.coreCraft.history.Single().item.id;
            A.riftFatigue=new RiftFatigue{version=1,day=RiftEntryRules.Day(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()),dailyMs=123456,paidMs=654321,recoveries=2};
            H.riftProgress.best.Add(new RiftBestTime{stage=24,milliseconds=32100});H.riftProgress.claimed.Add(24);
            A.gems.Add(new GemStack{gemId="G07",tier=4,count=7});A.schema=10;
            string path=Path.Combine(directory,"hellscript-local-v1.json"),legacy=JsonUtility.ToJson(A);
            File.WriteAllText(path,legacy);store=new GameStore(directory,catalog);
            Assert.AreEqual(legacy,File.ReadAllText(store.GemRecoveryArchive));
            Assert.AreEqual(7,GemStacks.Count(A.gems,"G06",4));Assert.AreEqual(0,GemStacks.Count(A.gems,"G07",4));
            Assert.AreEqual("integrated-craft",A.coreCraft.pendingId);Assert.AreEqual(itemId,A.coreCraft.history.Single().item.id);
            Assert.AreEqual(4000,A.premium);Assert.AreEqual(10,A.cores[0]);
            Assert.AreEqual(123456,A.riftFatigue.dailyMs);Assert.AreEqual(654321,A.riftFatigue.paidMs);Assert.AreEqual(2,A.riftFatigue.recoveries);
            Assert.AreEqual(32100,H.riftProgress.Best(24));Assert.AreEqual("claimed",H.riftProgress.Chest(24));
            Assert.IsTrue(store.AcknowledgeCoreCraft("integrated-craft"),store.Error);store=new GameStore(directory,catalog);
            Assert.AreEqual(GameStore.MaximumSchemaVersion,A.schema);Assert.IsEmpty(A.coreCraft.pendingId);Assert.AreEqual(1,A.coreCraft.history.Count);
            Assert.AreEqual(654321,A.riftFatigue.paidMs);Assert.AreEqual(32100,H.riftProgress.Best(24));Assert.AreEqual(4000,A.premium);
            Assert.IsEmpty(store.GemRecoveryArchive);
        }
        [Test] public void EveryLiveRecipeHasStableLegalRangesAndActualIdentity()
        {
            Assert.That(CoreCrafting.Recipes.Count,Is.GreaterThanOrEqualTo(147));
            foreach(var recipe in CoreCrafting.Recipes)foreach(int level in new[]{1,30,60})
            {
                var first=CoreCrafting.Definition(recipe.id,level);var second=CoreCrafting.Definition(recipe.id,level);
                ItemCatalog.Validate(first);Assert.AreEqual(recipe.id,first.special);Assert.AreEqual(recipe.slot,first.slot);Assert.AreEqual(JsonUtility.ToJson(first),JsonUtility.ToJson(second));
                Assert.AreEqual(4,first.rolls.Count);Assert.AreEqual(2,first.rolls.Count(r=>r.side==AffixSide.Prefix));
                Assert.AreEqual(4,first.rolls.Select(r=>ItemCatalog.Affix(r.affixId).group).Distinct().Count());
                var ranges=ItemTooltip.CatalogRanges(first).Where(l=>l.key.StartsWith("affix:")).ToArray();Assert.AreEqual(4,ranges.Length);Assert.IsTrue(ranges.All(r=>r.value.Contains(" ~ ")));
            }
        }
        [TestCase(0)][TestCase(1)][TestCase(2504)][TestCase(8000)] public void PaidFloorIsIndependentInclusiveAndPersisted(int investment)
        {
            int count=H.inventory.Count;var quote=Quote(investment);var expected=CoreCrafting.Definition(Recipe,30);
            Assert.IsTrue(store.CraftCoreEquipment("roll",quote,81231),store.Error);var result=A.coreCraft.history.Single();
            Assert.AreEqual(10,A.cores[0]);Assert.AreEqual(12000-investment,A.premium);Assert.AreEqual(count+1,H.inventory.Count);
            CollectionAssert.AreEqual(expected.rolls.Select(r=>r.affixId),result.item.rolls.Select(r=>r.affixId));
            Assert.AreEqual(4,result.item.rolls.Select(r=>r.rollBasisPoints).Distinct().Count());
            foreach(var roll in result.item.rolls){Assert.That(roll.rollBasisPoints,Is.InRange(investment,10000));Assert.AreEqual(ItemCatalog.Affix(roll.affixId).Value(30,roll.rollBasisPoints),roll.value);}
            store=new GameStore(directory,catalog);Assert.AreEqual("roll",A.coreCraft.pendingId);Assert.AreEqual(result.item.id,A.coreCraft.history.Single().item.id);Assert.AreEqual(12000-investment,A.premium);
        }
        [Test] public void UniformSamplingCanReachBothEndpoints()
        {
            uint seed=1717;int min=10000,max=0;
            for(int n=0;n<120000;n++){int q=ItemGenerator.UniformQuality(ref seed,8000);min=Math.Min(min,q);max=Math.Max(max,q);}
            Assert.AreEqual(8000,min);Assert.AreEqual(10000,max);Assert.AreEqual(10000,ItemGenerator.UniformQuality(ref seed,10000));
        }
        [Test] public void RepeatedRequestAndPendingResultCannotDoubleCharge()
        {
            var quote=Quote(8000);Assert.IsTrue(store.CraftCoreEquipment("same",quote,19));string before=JsonUtility.ToJson(A);
            Assert.IsTrue(store.CraftCoreEquipment("same",quote,20));Assert.AreEqual(before,JsonUtility.ToJson(A));
            Assert.IsFalse(store.CraftCoreEquipment("another",Quote(0),21));Assert.AreEqual(before,JsonUtility.ToJson(A));
            Assert.IsFalse(store.CraftCoreEquipment("same",Quote(1),21));Assert.AreEqual(before,JsonUtility.ToJson(A));
            Assert.IsTrue(store.AcknowledgeCoreCraft("same"));Assert.IsTrue(store.AcknowledgeCoreCraft("same"));Assert.IsEmpty(A.coreCraft.pendingId);
        }
        [TestCase(-1)][TestCase(8001)][TestCase(int.MaxValue)] public void InvalidInvestmentDoesNotMutate(int investment)
        {string before=JsonUtility.ToJson(A);Assert.IsFalse(store.CraftCoreEquipment("invalid",Quote(investment),18));Assert.AreEqual(before,JsonUtility.ToJson(A));}
        [Test] public void ShortagesAndFullBagLeaveTheAccountUntouched()
        {
            A.cores[0]=9;string before=JsonUtility.ToJson(A);Assert.IsFalse(store.CraftCoreEquipment("cores",Quote(),1));Assert.AreEqual(before,JsonUtility.ToJson(A));
            A.cores[0]=10;A.premium=49;before=JsonUtility.ToJson(A);Assert.IsFalse(store.CraftCoreEquipment("coins",Quote(50),1));Assert.AreEqual(before,JsonUtility.ToJson(A));
            uint random=88;while(Economy.FreeSlots(H)>0)Assert.IsTrue(Economy.AddItem(H,ItemGenerator.Create(H.heroClass,0,0,1,ref random),BagPolicy.Ignore,A));
            before=JsonUtility.ToJson(A);Assert.IsFalse(store.CraftCoreEquipment("bag",Quote(),1));Assert.AreEqual(before,JsonUtility.ToJson(A));
        }
        [Test] public void StaleLevelHeroAndRecipeQuotesCannotSpend()
        {
            var quote=Quote();H.highestClear++;string before=JsonUtility.ToJson(A);Assert.IsFalse(store.CraftCoreEquipment("level",quote,3));Assert.AreEqual(before,JsonUtility.ToJson(A));
            quote=Quote();A.selectedHero=1;before=JsonUtility.ToJson(A);Assert.IsFalse(store.CraftCoreEquipment("hero",quote,3));Assert.AreEqual(before,JsonUtility.ToJson(A));
            quote=Quote();quote.recipeId="not-a-recipe";before=JsonUtility.ToJson(A);Assert.IsFalse(store.CraftCoreEquipment("recipe",quote,3));Assert.AreEqual(before,JsonUtility.ToJson(A));
        }
        [Test] public void ExactTenSetCraftAcrossClassesUsesSharedBalance()
        {
            var recipe=CoreCrafting.Recipes.First(r=>!string.IsNullOrEmpty(r.setId)&&r.heroClass==2);A.cores[recipe.slot]=10;
            var q=CoreCrafting.Quote(A,recipe.id,0);Assert.IsTrue(store.CraftCoreEquipment("set",q,17),store.Error);
            var item=A.coreCraft.history.Single().item;Assert.AreEqual(HeroClass.Mage,item.lootClass);Assert.AreEqual(recipe.setId,ItemCatalog.Unique(item.special).setId);Assert.AreEqual(0,A.cores[recipe.slot]);Assert.IsTrue(H.inventory.Any(i=>i.id==item.id));
            Assert.IsTrue(store.AcknowledgeCoreCraft("set"));A.selectedHero=2;Assert.AreEqual(0,A.cores[recipe.slot]);
        }
        [Test] public void FailedCraftAndConfirmationSaveDoNotAdvance()
        {
            string temp=Path.Combine(directory,"hellscript-local-v1.json.tmp");Directory.CreateDirectory(temp);
            string before=JsonUtility.ToJson(A);LogAssert.Expect(LogType.Error,new Regex("저장하지 못했습니다"));
            Assert.IsFalse(store.CraftCoreEquipment("save",Quote(8000),19));Assert.AreEqual(before,JsonUtility.ToJson(A));Directory.Delete(temp);
            Assert.IsTrue(store.CraftCoreEquipment("save",Quote(8000),19));Directory.CreateDirectory(temp);before=JsonUtility.ToJson(A);
            LogAssert.Expect(LogType.Error,new Regex("저장하지 못했습니다"));Assert.IsFalse(store.AcknowledgeCoreCraft("save"));Assert.AreEqual(before,JsonUtility.ToJson(A));Directory.Delete(temp);
            store=new GameStore(directory,catalog);Assert.AreEqual("save",A.coreCraft.pendingId);Assert.AreEqual(4000,A.premium);
        }
        [Test] public void HistoryKeepsCraftTimeValuesAfterTheOwnedItemChanges()
        {
            Assert.IsTrue(store.CraftCoreEquipment("history",Quote(),72));var record=A.coreCraft.history.Single();string snapshot=JsonUtility.ToJson(record.item);
            H.inventory.Single(i=>i.id==record.item.id).enhancement=10;Assert.IsTrue(store.Save());store=new GameStore(directory,catalog);
            Assert.AreEqual(snapshot,JsonUtility.ToJson(A.coreCraft.history.Single().item));
        }
        [Test] public void LegacySavePreservesCoreWalletAndOtherGrowth()
        {
            A.coreCraft=null;A.schema=9;A.forge.stations=4;H.slotProgress.levels[0]=75;
            File.WriteAllText(Path.Combine(directory,"hellscript-local-v1.json"),JsonUtility.ToJson(A));store=new GameStore(directory,catalog);
            Assert.IsEmpty(A.coreCraft.history);Assert.AreEqual(12000,A.premium);Assert.AreEqual(20,A.cores[0]);Assert.AreEqual(4,A.forge.stations);Assert.AreEqual(75,H.slotProgress.levels[0]);
        }
        [TestCase(0)][TestCase(1)][TestCase(2)] public void SingleBulkAndAutoSalvageGiveOneMatchingCore(int route)
        {
            uint random=6;
            foreach(var recipe in CoreCrafting.Recipes.Where(r=>r.heroClass==0).GroupBy(r=>string.IsNullOrEmpty(r.setId)).Select(g=>g.First()))
            {
                var item=ItemGenerator.Create(H.heroClass,recipe.slot,3,30,ref random,uniqueId:recipe.id);Assert.IsTrue(Economy.AddItem(H,item,BagPolicy.Ignore,A));int before=A.cores[recipe.slot];
                if(route==0)Assert.IsTrue(store.Transact(Guid.NewGuid().ToString(),"salvage",a=>Economy.Dismantle(a,a.Hero,a.Hero.inventory.Single(i=>i.id==item.id))));
                else if(route==1)Assert.IsTrue(store.DismantleInventory(new InventorySalvagePlan(A,new[]{item.id})));
                else {var policy=new EdictCleanupPolicy{allowLegendaryDisposal=true,actions=Enumerable.Repeat(EdictCleanupAction.Salvage,5).ToArray()};Assert.IsTrue(store.Transact(Guid.NewGuid().ToString(),"auto-salvage",a=>policy.Apply(a).salvaged>0));}
                Assert.AreEqual(before+1,A.cores[recipe.slot]);
            }
        }
    }
}
