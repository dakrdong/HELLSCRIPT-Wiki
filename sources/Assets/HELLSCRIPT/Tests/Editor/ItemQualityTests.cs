using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hellscript.Tests
{
    public sealed class ItemQualityTests
    {
        GameCatalog catalog;AccountSave account;uint rng;string directory;
        static string Json(object value)=>JsonUtility.ToJson(value);
        [SetUp] public void Setup()
        {
            Loc.UseSource();catalog=ScriptableObject.CreateInstance<GameCatalog>();catalog.Populate();account=GameStore.NewAccount(catalog);
            account.Hero.level=30;account.Hero.highestClear=30;account.gold=10000000;account.materials=1000000;ContentUnlocks.Reconcile(account);
            rng=778911;directory=Path.Combine(Path.GetTempPath(),"hellscript-quality-"+Guid.NewGuid());
        }
        [TearDown] public void Teardown()
        {Loc.UseSource();UnityEngine.Object.DestroyImmediate(catalog);if(Directory.Exists(directory))Directory.Delete(directory,true);}
        Item Gear(int slot=0,int rarity=2,int level=30,bool awakened=false)
        {
            // These fixtures exercise a single equipped main stat; dependent offhands require a paired fixture.
            Item item;do{item=ItemGenerator.Create(account.Hero.heroClass,slot,rarity,level,ref rng,riftStage:awakened?100:0);}while(awakened&&!item.awakened||slot==0&&EquipmentSlots.Offhand(item));
            foreach(var old in account.Hero.inventory.Where(i=>i.slot==slot))old.equipped=false;
            item.equipped=true;account.Hero.inventory.Add(item);return item;
        }
        static void Enhanced(Item item){item.enhancement=5;item.investedMaterials=620;}
        GameStore Store()
        {var store=new GameStore(directory,catalog);JsonUtility.FromJsonOverwrite(Json(account),store.Data);Assert.IsTrue(store.Save());account=store.Data;return store;}
        static void Greater(Item item,AffixRoll roll)
        {roll.greater=true;roll.rollBasisPoints=10000;roll.tierId="T1";roll.value=ItemCatalog.Affix(roll.affixId).Value(item.level,10000);}
        static object Call(CombatSimulation sim,string name,params object[] arguments)
        {
            var method=typeof(CombatSimulation).GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic);
            var parameters=method.GetParameters();var values=new object[parameters.Length];
            for(int i=0;i<values.Length;i++)values[i]=i<arguments.Length?arguments[i]:parameters[i].DefaultValue;
            return method.Invoke(sim,values);
        }

        [TestCase(19,0)][TestCase(20,.02)][TestCase(25,.12)][TestCase(30,.22)][TestCase(40,.42)][TestCase(44,.5)][TestCase(100,.5)]
        public void AwakeningUsesTheAdoptedStageCurve(int stage,double expected)=>Assert.AreEqual(expected,ItemQuality.AwakeningChance(stage),.00001);
        [TestCase(20)][TestCase(25)][TestCase(30)][TestCase(40)][TestCase(44)][TestCase(100)]
        public void OneHundredThousandSeededQualityDrawsMatchTheStageProbability(int stage)
        {
            const int count=100000;uint random=(uint)(98711+stage);int successes=0;
            for(int n=0;n<count;n++)if(ItemQuality.RollAwakening(2,stage,ref random))successes++;
            double probability=Math.Min(.5,.02*(stage-19)),expected=count*probability,deviation=Math.Sqrt(count*probability*(1-probability));
            Assert.Less(Math.Abs(successes-expected),6*deviation+1);
            TestContext.WriteLine($"QUALITY_DISTRIBUTION stage={stage} count={count} awakened={successes} expected={expected}");
        }
        [Test]
        public void ExcludedRaritiesAndEarlyStagesNeverAwakenOrConsumeQualityRandomness()
        {
            foreach(int stage in new[]{1,19,20,100})foreach(int rarity in new[]{0,1,2,3})
                if(stage<20||rarity<2){uint before=rng;for(int n=0;n<100;n++)Assert.IsFalse(ItemQuality.RollAwakening(rarity,stage,ref rng));Assert.AreEqual(before,rng);}
            Assert.AreEqual(0,ItemQuality.AwakeningChance(int.MinValue));Assert.AreEqual(.5f,ItemQuality.AwakeningChance(int.MaxValue));
        }
        [Test,Timeout(600000)]
        public void CompleteRiftItemsRespectTheFloorAndIndependentGreaterAffixDraws()
        {
            const int count=100000;int awakened=0,greater=0,any=0,invalid=0;uint random=876431;
            for(int n=0;n<count;n++)
            {
                var item=Economy.CreateRiftItem(HeroClass.Warrior,0,3,30,30,ref random,"distribution-"+n);
                if(!item.awakened){if(item.rolls.Any(r=>r.greater))invalid++;continue;}
                awakened++;int found=0;
                foreach(var roll in item.rolls)
                {
                    if(roll.rollBasisPoints<4000)invalid++;
                    if(!roll.greater)continue;greater++;found++;
                    if(roll.rollBasisPoints!=10000||roll.tierId!="T1"||Math.Abs(ItemQuality.AffixValue(item,roll)-ItemCatalog.Affix(roll.affixId).Value(30,10000)*1.5f)>.001)invalid++;
                }
                if(found>0)any++;
            }
            Assert.Zero(invalid);Assert.That(awakened,Is.InRange(21000,23000));
            Assert.Less(Math.Abs(greater-awakened*4*.1),6*Math.Sqrt(awakened*4*.1*.9)+1);
            double chance=1-Math.Pow(.9,4);Assert.Less(Math.Abs(any-awakened*chance),6*Math.Sqrt(awakened*chance*(1-chance))+1);
            TestContext.WriteLine($"QUALITY_FULL_ITEMS count={count} awakened={awakened} greaterLines={greater} anyGreater={any}");
        }
        [Test]
        public void CompleteItemGenerationReplaysWithTheSameSeedAndCapsNewLevels()
        {
            uint left=887123,right=left;
            for(int n=0;n<120;n++)
            {
                var a=ItemGenerator.Create(HeroClass.Warrior,n%8,n%4,120,ref left,"replay-"+n,riftStage:100);
                var b=ItemGenerator.Create(HeroClass.Warrior,n%8,n%4,120,ref right,"replay-"+n,riftStage:100);
                Assert.AreEqual(60,a.level);Assert.AreEqual(30,a.RequiredLevel);Assert.AreEqual(Json(a),Json(b));
            }
            Assert.AreEqual(left,right);
        }
        [TestCase(0)][TestCase(1)][TestCase(2)]
        public void ShopAndBothCraftingServicesNeverAwakenHighStageItems(int mode)
        {
            account.Hero.highestClear=100;ContentUnlocks.Reconcile(account);
            for(int n=0;n<50;n++)
            {
                int slot=n%8;account.cores[slot]=100;
                Assert.IsTrue(ContentServices.Purchase(account,slot,mode,ref rng,out var item));
                Assert.AreEqual(60,item.level);Assert.IsFalse(item.awakened);Assert.Zero(ItemQuality.GreaterCount(item));
                account.Hero.inventory.Remove(item);
            }
        }
        [TestCase(0,12)][TestCase(30,12)][TestCase(60,102)][TestCase(90,192)][TestCase(93,200)][TestCase(int.MaxValue,200)]
        public void MasterworkCapsMatchAccountProgress(int stage,int expected)=>Assert.AreEqual(expected,ItemQuality.MasterworkCap(stage));
        [Test]
        public void MasterworkChargesEveryLevelAndOnlyBoostsAffixesAtFourEightAndTwelve()
        {
            account.Hero.highestClear=100;var item=Gear(1);Enhanced(item);
            int gold=account.gold,materials=account.materials;var rolls=item.rolls.Select(Json).ToArray();
            for(int level=1;level<=200;level++)
            {
                Assert.IsTrue(ItemQuality.Advance(account,item,ref rng));Assert.AreEqual(level,item.masterwork);
                Assert.AreEqual(Math.Min(3,level/4),item.masterworkLines.Count);ItemCatalog.Validate(item);
            }
            Assert.AreEqual(44200,materials-account.materials);Assert.AreEqual(4020000,gold-account.gold);
            Assert.AreEqual(44820,item.investedMaterials);Assert.AreEqual(44200,item.masterworkInvestedMaterials);
            CollectionAssert.AreEqual(rolls,item.rolls.Select(Json));
            Assert.AreEqual(Math.Pow(1.02,200),ItemQuality.MainMultiplier(item),.0005);
            string before=Json(account);Assert.IsFalse(ItemQuality.Advance(account,item,ref rng));Assert.AreEqual(before,Json(account));
        }
        [Test]
        public void RepeatedAffixBoostsAddBeforeMultiplyingTheGreaterAffixBonus()
        {
            var item=Gear(0,3,30,true);Enhanced(item);var roll=item.rolls[0];Greater(item,roll);
            item.masterwork=12;item.masterworkLines=new List<string>{roll.slotId,roll.slotId,roll.slotId};ItemCatalog.Validate(item);
            Assert.AreEqual(roll.value*1.5*1.75,ItemQuality.AffixValue(item,roll),.0001);
            var before=new HeroStats(account.Hero);item.masterworkLines.Clear();var after=new HeroStats(account.Hero);
            Assert.AreEqual(roll.value*1.5*.75,before.Bonus((StatId)ItemCatalog.Affix(roll.affixId).stat)-after.Bonus((StatId)ItemCatalog.Affix(roll.affixId).stat),.001);
        }
        [Test]
        public void AnItemWithoutAffixesStillReceivesMainStatMasterwork()
        {
            var item=Gear(2,0);Enhanced(item);float before=ItemCatalog.MainValue(item);
            for(int n=0;n<12;n++)Assert.IsTrue(ItemQuality.Advance(account,item,ref rng));
            Assert.Zero(item.masterworkLines.Count);float flat=GearEnhancement.Step(item)*item.enhancement;Assert.AreEqual((before-flat)*Math.Pow(1.02,12)+flat,ItemCatalog.MainValue(item),.001);
        }
        [Test]
        public void HigherAccountClearExtendsExistingGearAndLowerDamagedProgressDoesNotReduceIt()
        {
            var item=Gear();Enhanced(item);for(int n=0;n<12;n++)Assert.IsTrue(ItemQuality.Advance(account,item,ref rng));
            string full=Json(account);Assert.IsFalse(ItemQuality.Advance(account,item,ref rng));Assert.AreEqual(full,Json(account));
            account.heroes[1].highestClear=60;Assert.IsTrue(ItemQuality.Advance(account,item,ref rng));Assert.AreEqual(13,item.masterwork);
            account.Hero.highestClear=0;account.heroes[1].highestClear=0;float power=ItemCatalog.MainValue(item);
            var store=Store();var loaded=new GameStore(directory,catalog);var restored=loaded.Data.Hero.inventory.Single(i=>i.id==item.id);
            Assert.AreEqual(13,restored.masterwork);Assert.AreEqual(power,ItemCatalog.MainValue(restored));
            Assert.IsFalse(ItemQuality.Advance(loaded.Data,restored,ref rng));
        }
        [TestCase(0)][TestCase(1)][TestCase(2)][TestCase(3)][TestCase(4)][TestCase(5)][TestCase(6)][TestCase(7)]
        public void EverySlotAppliesQualityToItsMainStatAcrossAllThreeClasses(int slot)
        {
            for(int c=0;c<3;c++)
            {
                account.selectedHero=c;account.Hero.level=30;var item=Gear(slot,2,30,true);Enhanced(item);
                foreach(var r in item.rolls)r.greater=false;item.awakened=false;
                var before=new HeroStats(account.Hero);float main=ItemCatalog.MainValue(item);
                item.awakened=true;item.masterwork=12;var after=new HeroStats(account.Hero);double factor=1.25*Math.Pow(1.02,12);
                float flat=GearEnhancement.Step(item)*item.enhancement;double delta=(main-flat)*(factor-1);
                Assert.AreEqual((main-flat)*factor+flat,ItemCatalog.MainValue(item),.003);
                if(slot==0)Assert.AreEqual(delta*(1+.002*new[]{before.strength,before.dexterity,before.intelligence}[c]),after.damage-before.damage,.005);
                else if(slot<=2)Assert.AreEqual(delta,after.armor-before.armor,.005);
                else if(slot==3)Assert.AreEqual(delta,after.Bonus(StatId.AttackSpeed)-before.Bonus(StatId.AttackSpeed),.005);
                else if(slot==4)Assert.AreEqual(delta,after.Bonus(StatId.MovementSpeed)-before.Bonus(StatId.MovementSpeed),.005);
                else if(slot==5)Assert.AreEqual(delta*(1+before.bonuses[1]/100),after.hp-before.hp,.005);
                else if(slot==6)Assert.AreEqual(delta,after.Bonus(StatId.AllResistancePercent)-before.Bonus(StatId.AllResistancePercent),.005);
                else Assert.AreEqual(delta,after.Bonus(StatId.CriticalStrikeChance)-before.Bonus(StatId.CriticalStrikeChance),.005);
                Assert.AreEqual(before.resistance,after.resistance);
                if(slot!=3)Assert.AreEqual(before.attackSpeed,after.attackSpeed);if(slot!=4)Assert.AreEqual(before.speed,after.speed);
            }
        }
        [Test]
        public void RerollRemovesGreaterButPreservesTheAwakenedFloorAndMasterworkSlot()
        {
            var item=Gear(0,3,30,true);Enhanced(item);var previous=item.rolls[0];Greater(item,previous);
            item.masterwork=12;item.masterworkLines=new List<string>{previous.slotId,previous.slotId,previous.slotId};
            var other=item.rolls.Skip(1).Select(Json).ToArray();int gold=account.gold;long cost=Economy.RerollGold(item);
            Assert.IsTrue(Economy.Reroll(account,item,previous.slotId,ref rng));var result=item.rolls[0];
            Assert.IsFalse(result.greater);Assert.GreaterOrEqual(result.rollBasisPoints,4000);Assert.AreEqual(previous.slotId,result.slotId);
            Assert.AreEqual(3,ItemQuality.LineHits(item,result.slotId));Assert.AreEqual(result.value*1.75,ItemQuality.AffixValue(item,result),.001);
            Assert.AreEqual(cost,gold-account.gold);CollectionAssert.AreEqual(other,item.rolls.Skip(1).Select(Json));ItemCatalog.Validate(item);
        }
        [Test]
        public void ResetRefundsOnlyActualMasterworkInvestmentAndRetainsAllOtherEquipmentLayers()
        {
            var item=Gear(0,3,30,true);Enhanced(item);Greater(item,item.rolls[0]);
            item.sockets.Add(new SocketState{index=0,gemId="G06",tier=6});
            for(int n=0;n<12;n++)Assert.IsTrue(ItemQuality.Advance(account,item,ref rng));
            int materials=account.materials,gold=account.gold;var rolls=item.rolls.Select(Json).ToArray();
            Assert.IsTrue(ItemQuality.Reset(account,item));Assert.AreEqual(316,account.materials-materials);Assert.AreEqual(60000,gold-account.gold);
            Assert.AreEqual(5,item.enhancement);Assert.AreEqual(620,item.investedMaterials);Assert.Zero(item.masterwork);Assert.Zero(item.masterworkInvestedMaterials);Assert.IsEmpty(item.masterworkLines);
            Assert.IsTrue(item.awakened);Assert.IsTrue(item.rolls[0].greater);Assert.AreEqual("G06",item.sockets.Single().gemId);CollectionAssert.AreEqual(rolls,item.rolls.Select(Json));
            string before=Json(account);Assert.IsFalse(ItemQuality.Reset(account,item));Assert.AreEqual(before,Json(account));
            // A subsequent salvage receives the remaining enhancement investment once.
            item.sockets.Clear();item.equipped=false;materials=account.materials;Assert.IsTrue(Economy.Dismantle(account,account.Hero,item));Assert.AreEqual(496,account.materials-materials);
        }
        [Test]
        public void RejectedRequestsDoNotSpendMoneyMaterialsOrRandomness()
        {
            var item=Gear();string before=Json(account);uint previous=rng;
            Assert.IsFalse(ItemQuality.Advance(account,item,ref rng));Assert.AreEqual(before,Json(account));Assert.AreEqual(previous,rng);
            Enhanced(item);account.gold=199;before=Json(account);Assert.IsFalse(ItemQuality.Advance(account,item,ref rng));Assert.AreEqual(before,Json(account));
            account.gold=10000;account.materials=21;before=Json(account);Assert.IsFalse(ItemQuality.Advance(account,item,ref rng));Assert.AreEqual(before,Json(account));
            account.materials=100;account.Hero.inventory.Remove(item);account.warehouse.Add(item);before=Json(account);
            Assert.IsFalse(ItemQuality.Advance(account,item,ref rng));Assert.AreEqual(before,Json(account));
        }
        [Test]
        public void AnActiveRiftBlocksBothMasterworkAndResetEvenThroughTheCoreService()
        {
            var item=Gear();Enhanced(item);Assert.IsTrue(ItemQuality.Advance(account,item,ref rng));
            var sim=new CombatSimulation(account,catalog,1,seed:99171);account.suspendedRun=sim.State;
            string before=Json(account);Assert.IsFalse(ItemQuality.Advance(account,item,ref rng));Assert.IsFalse(ItemQuality.Reset(account,item));Assert.AreEqual(before,Json(account));
        }
        [Test]
        public void MaterialOverflowRejectsResetWithoutDestroyingTheInvestment()
        {
            var item=Gear();Enhanced(item);Assert.IsTrue(ItemQuality.Advance(account,item,ref rng));account.materials=int.MaxValue;
            string before=Json(account);Assert.IsFalse(ItemQuality.Reset(account,item));Assert.AreEqual(before,Json(account));
        }
        [Test]
        public void DiskFailureAndRepeatedRequestsPreserveTheWholeMasterworkTransaction()
        {
            var item=Gear(1);Enhanced(item);for(int n=0;n<3;n++)Assert.IsTrue(ItemQuality.Advance(account,item,ref rng));string id=item.id;
            var store=Store();string original=Json(account),path=Path.Combine(directory,"hellscript-local-v1.json");byte[] disk=File.ReadAllBytes(path);
            Func<AccountSave,bool> upgrade=a=>{uint random=77211;return ItemQuality.Advance(a,a.Hero.inventory.Single(i=>i.id==id),ref random);};
            Directory.CreateDirectory(path+".tmp");LogAssert.Expect(LogType.Error,new Regex("저장하지 못했습니다"));
            Assert.IsFalse(store.Transact("mw-four","masterwork:"+id+":3",upgrade));Assert.AreEqual(original,Json(account));CollectionAssert.AreEqual(disk,File.ReadAllBytes(path));
            Assert.That(store.Error,Does.Not.Contain(directory));
            Directory.Delete(path+".tmp");Assert.IsTrue(store.Transact("mw-four","masterwork:"+id+":3",upgrade));
            var saved=account.Hero.inventory.Single(i=>i.id==id);Assert.AreEqual(4,saved.masterwork);Assert.AreEqual(1,saved.masterworkLines.Count);
            int gold=account.gold,materials=account.materials;string snapshot=Json(saved);
            Assert.IsTrue(store.Transact("mw-four","masterwork:"+id+":3",_=>throw new Exception("A repeated request must not invoke its mutation.")));
            Assert.AreEqual(gold,account.gold);Assert.AreEqual(materials,account.materials);Assert.AreEqual(snapshot,Json(account.Hero.inventory.Single(i=>i.id==id)));
            var loaded=new GameStore(directory,catalog);Assert.IsTrue(loaded.Transact("mw-four","masterwork:"+id+":3",_=>false));
            Assert.AreEqual(snapshot,Json(loaded.Data.Hero.inventory.Single(i=>i.id==id)));Assert.IsFalse(loaded.Transact("mw-four","different",_=>true));
        }
        [Test]
        public void EquipmentAndItsIndependentCombatReviewRetainQualityAfterRestart()
        {
            var item=Gear(0,3,30,true);Enhanced(item);Greater(item,item.rolls[0]);for(int n=0;n<12;n++)Assert.IsTrue(ItemQuality.Advance(account,item,ref rng));
            var recorded=JsonUtility.FromJson<Item>(Json(item));account.records.Add(new RunRecord{review=new CombatReview{version=CombatHistory.Version,equipment=new List<Item>{recorded}}});
            string expected=Json(item);Store();var loaded=new GameStore(directory,catalog);
            Assert.AreEqual(expected,Json(loaded.Data.Hero.inventory.Single(i=>i.id==item.id)));Assert.AreEqual(expected,Json(loaded.Data.records.Last().review.equipment.Single()));
            var live=loaded.Data.Hero.inventory.Single(i=>i.id==item.id);Assert.IsTrue(ItemQuality.Reset(loaded.Data,live));
            Assert.AreEqual(expected,Json(loaded.Data.records.Last().review.equipment.Single()));
        }
        [Test]
        public void AHistoricalQualityItemStillMakesOlderPlayersRejectTheSaveAfterItsLiveCopyIsGone()
        {
            var item=Gear(1);Enhanced(item);Assert.IsTrue(ItemQuality.Advance(account,item,ref rng));
            account.records.Add(new RunRecord{review=new CombatReview{version=CombatHistory.Version,equipment=new List<Item>{JsonUtility.FromJson<Item>(Json(item))}}});
            account.Hero.inventory.Remove(item);foreach(var owned in account.heroes.SelectMany(h=>h.inventory))owned.contentVersion=3;
            var store=Store();Assert.AreEqual(GameStore.MaximumSchemaVersion,account.schema);Assert.IsTrue(account.heroes.SelectMany(h=>h.inventory).All(i=>i.contentVersion==3));
            var old=account.records.Last().review.equipment.Single();old.contentVersion=ItemCatalog.Version+1;string path=Path.Combine(directory,"hellscript-local-v1.json"),original=Json(account);File.WriteAllText(path,original);
            Assert.Throws<NotSupportedException>(()=>new GameStore(directory,catalog));Assert.AreEqual(original,File.ReadAllText(path));
        }
        [Test]
        public void QualityHistoryWithNoOwnedEquipmentRetainsItsSchemaBoundaryAndData()
        {
            var item=Gear(0,3,30,true);Enhanced(item);Assert.IsTrue(ItemQuality.Advance(account,item,ref rng));
            string expected=Json(item);account.records.Add(new RunRecord{review=new CombatReview{version=CombatHistory.Version,equipment=new List<Item>{JsonUtility.FromJson<Item>(expected)}}});
            foreach(var hero in account.heroes)hero.inventory.Clear();account.warehouse.Clear();
            Store();Assert.AreEqual(GameStore.MaximumSchemaVersion,account.schema);var loaded=new GameStore(directory,catalog);
            Assert.AreEqual(GameStore.MaximumSchemaVersion,loaded.Data.schema);Assert.AreEqual(expected,Json(loaded.Data.records.Last().review.equipment.Single()));
            loaded.Data.records.Clear();Assert.IsTrue(loaded.Save());Assert.AreEqual(GameStore.MaximumSchemaVersion,new GameStore(directory,catalog).Data.schema);
        }
        [Test]
        public void TheCurrentAccountSchemaAlsoRejectsAStillNewerSchemaWithoutFallback()
        {
            Store();string path=Path.Combine(directory,"hellscript-local-v1.json");account.schema=GameStore.MaximumSchemaVersion+1;
            string original=Json(account);File.WriteAllText(path,original);Assert.Throws<NotSupportedException>(()=>new GameStore(directory,catalog));Assert.AreEqual(original,File.ReadAllText(path));
        }
        [Test]
        public void InvalidQualityIsRepairedWithoutLosingEquipmentOrTheExactOriginalSave()
        {
            var item=Gear(1,3,30,true);Enhanced(item);item.masterwork=300;item.masterworkInvestedMaterials=396;item.investedMaterials+=396;
            item.masterworkLines.Add("missing-slot");var roll=item.rolls[0];roll.greater=true;roll.rollBasisPoints=6000;roll.tierId="T1";
            Directory.CreateDirectory(directory);string path=Path.Combine(directory,"hellscript-local-v1.json"),original=Json(account);File.WriteAllText(path,original);
            var store=new GameStore(directory,catalog);var restored=store.Data.Hero.inventory.Single(i=>i.id==item.id);
            Assert.Zero(restored.masterwork);Assert.IsEmpty(restored.masterworkLines);Assert.IsFalse(restored.rolls[0].greater);Assert.AreEqual("T2",restored.rolls[0].tierId);
            Assert.AreEqual(item.baseId,restored.baseId);Assert.AreEqual(1016,restored.investedMaterials);Assert.AreEqual(396,restored.masterworkInvestedMaterials);
            Assert.AreEqual(original,File.ReadAllText(store.QualityRecoveryArchive));Assert.IsNotEmpty(store.QualityRecoveryMessage);
            var again=new GameStore(directory,catalog);Assert.IsEmpty(again.QualityRecoveryMessage);Assert.AreEqual(1,Directory.GetFiles(directory,"*.quality-recovery-*.json").Length);
        }
        [Test]
        public void UnknownBoostSlotsAreRemovedIndividuallyWithoutReassigningThem()
        {
            var item=Gear(1);Enhanced(item);item.masterwork=12;string id=item.rolls[0].slotId;
            item.masterworkLines=new List<string>{id,"unknown",id};Assert.IsTrue(ItemQuality.RepairValues(item));
            CollectionAssert.AreEqual(new[]{id,id},item.masterworkLines);Assert.AreEqual(12,item.masterwork);ItemCatalog.Validate(item);
        }
        [Test]
        public void ExistingHighLevelItemsAreRetainedWhileNewItemsStopAtSixty()
        {
            var item=Gear();item.contentVersion=3;item.level=88;float main=ItemCatalog.MainValue(item);string id=item.id;Store();
            var loaded=new GameStore(directory,catalog);var restored=loaded.Data.Hero.inventory.Single(i=>i.id==id);
            Assert.AreEqual(88,restored.level);Assert.AreEqual(main,ItemCatalog.MainValue(restored));Assert.IsEmpty(loaded.QualityRecoveryMessage);
            Assert.AreEqual(60,Economy.CreateItem(HeroClass.Warrior,0,2,88,ref rng).level);
        }
        [Test]
        public void AwakenedFilterAndDescriptionsDistinguishQualityWithoutChangingRarity()
        {
            var normal=Gear(1);var awakened=Gear(2,2,30,true);Enhanced(awakened);awakened.masterwork=12;
            var query=new InventoryQuery{awakenedOnly=true};CollectionAssert.AreEqual(new[]{awakened.id},query.Apply(account.Hero.inventory).Select(i=>i.id));
            Assert.AreEqual(2,awakened.rarity);Assert.That(awakened.DisplayName,Does.StartWith("[각성]"));
            Loc.Use("en",LocalizationTable.Parse(Resources.Load<TextAsset>("Localization/en").text).Entries);
            Assert.That(awakened.DisplayName,Does.StartWith("[Awakened]"));Assert.That(ItemQuality.Summary(awakened),Does.Contain("Masterwork 12"));
            // The filter builds every unique/set option, including names composed by the catalog.
            foreach(var unique in ItemCatalog.Uniques)
                Assert.IsFalse(unique.Name.Any(c=>c>='가'&&c<='힣'),unique.id);
            Assert.AreEqual(0,Loc.MissingCount,string.Join("; ",Loc.Missing));Assert.IsFalse(normal.awakened);
        }
        [TestCase(19)][TestCase(100)]
        public void TheCombatDropOwnerUsesTheRiftStageAndCapsItems(int stage)
        {
            var sim=new CombatSimulation(account,catalog,stage,seed:799321);
            for(int n=0;n<240;n++)Call(sim,"Drop",Vector2.zero,n%4);
            var items=sim.State.drops.Select(d=>d.item).ToArray();Assert.AreEqual(240,items.Length);
            Assert.IsTrue(items.All(i=>i.level<=60));Assert.IsTrue(items.Where(i=>i.rarity<2).All(i=>!i.awakened));
            if(stage<20)Assert.IsTrue(items.All(i=>!i.awakened));else Assert.Greater(items.Count(i=>i.awakened),20);
            foreach(var item in items)ItemCatalog.Validate(item);
        }
        [TestCase(19)][TestCase(100)]
        public void GeneratedChestRewardsUseTheSameQualityRules(int stage)
        {
            var items=new List<Item>();
            for(uint seed=79001;seed<79061;seed++)
                items.AddRange(RiftGenerator.Generate(seed,"quality-chests-"+seed,stage,HeroClass.Warrior).chests.Where(c=>c.reward!=null).Select(c=>c.reward));
            Assert.AreEqual(120,items.Count);Assert.IsTrue(items.All(i=>i.level<=60));
            Assert.IsTrue(items.Where(i=>i.rarity<2).All(i=>!i.awakened));
            if(stage<20)Assert.IsTrue(items.All(i=>!i.awakened));else Assert.Greater(items.Count(i=>i.awakened),0);
            foreach(var item in items)ItemCatalog.Validate(item);
        }
        [TestCase(19)][TestCase(100)]
        public void SweepRewardsRespectQualityAndTheirRealReceiptGuard(int stage)
        {
            account.Hero.highestClear=stage;ContentUnlocks.Reconcile(account);int awake=0;
            for(int sample=0;sample<50;sample++)
            {
                // Each fixture represents a new daily allowance; the service still checks its receipt.
                account.sweepDay="2000-01-01";account.Hero.inventory.Clear();
                string request="quality-sweep-"+sample;Assert.IsTrue(Economy.Sweep(account,request,ref rng));
                Assert.AreEqual(3,account.Hero.inventory.Count);Assert.IsTrue(account.Hero.inventory.All(i=>i.level<=60));
                foreach(var item in account.Hero.inventory){ItemCatalog.Validate(item);if(item.awakened)awake++;if(item.rarity<2)Assert.IsFalse(item.awakened);}
                string before=Json(account);uint random=rng;Assert.IsFalse(Economy.Sweep(account,request,ref rng));Assert.AreEqual(before,Json(account));Assert.AreEqual(random,rng);
            }
            if(stage<20)Assert.Zero(awake);else Assert.Greater(awake,10);
        }
        [Test]
        public void QualityChangesInvalidateDisposalQuotesAndRespectEnhancedProtection()
        {
            var item=Gear(1);item.equipped=false;Enhanced(item);
            var plan=new InventoryBulkPlan(account,new[]{item.id},InventoryBulkOperation.Dismantle);
            Assert.IsTrue(ItemQuality.Advance(account,item,ref rng));string before=Json(account);
            Assert.IsFalse(plan.Apply(account));Assert.AreEqual(before,Json(account));
            var policy=new EdictCleanupPolicy{protectEnhanced=true,actions=new[]{EdictCleanupAction.Keep,EdictCleanupAction.Keep,EdictCleanupAction.Salvage,EdictCleanupAction.Keep,EdictCleanupAction.Keep}};
            var report=policy.Apply(account);Assert.AreEqual(1,report.protectedItems);Assert.Zero(report.salvaged);Assert.IsTrue(account.Hero.inventory.Contains(item));
        }
        [Test]
        public void WeaponQualityAndFlatBlacksmithBonusesComposeSeparatelyInActualDamage()
        {
            account.Hero.inventory.Clear();account.Hero.build.passives=Array.Empty<int>();var item=Gear(0,2,30,true);Enhanced(item);
            // Isolate section 5.2's stated +60% additive baseline. Affix boosts are a separate test.
            foreach(var roll in item.rolls)roll.greater=false;item.awakened=false;
            float unenhanced=GearEnhancement.Base(item),flat=GearEnhancement.Step(item)*item.enhancement;var before=new CombatSimulation(account,catalog,30,seed:61722);
            item.awakened=true;item.masterwork=12;item.sockets.Add(new SocketState{index=0,gemId="G06",tier=6});
            var after=new CombatSimulation(account,catalog,30,seed:61722);
            DamageEvent Hit(CombatSimulation sim)
            {
                sim.Stats.bonuses[(int)StatId.PhysicalDamage]=60+sim.Stats.gemBonuses[(int)StatId.PhysicalDamage];sim.Stats.overpower=0;
                var enemy=new EnemyState{id=9811,position=sim.State.position+Vector2.up*6,health=1000000,maxHealth=1000000,brain=new EnemyBrain{initialized=true}};
                sim.State.enemies.Add(enemy);return (DamageEvent)Call(sim,"Hit",enemy,1f,Element.Physical,false,0f,null,false);
            }
            var baseline=Hit(before);var quality=Hit(after);double fixedAttack=flat+before.Stats.slotAttack;double expected=(unenhanced*1.25*Math.Pow(1.02,12)+fixedAttack)/(unenhanced+fixedAttack)*1.83/1.6;
            Assert.AreEqual(.6,baseline.additive,.0001);Assert.AreEqual(.83,quality.additive,.0001);
            Assert.AreEqual(expected,quality.finalDamage/baseline.finalDamage,.0001);
            Assert.Greater(quality.finalDamage,baseline.finalDamage);
            TestContext.WriteLine($"QUALITY_CONTROLLED_DAMAGE baseline={baseline.finalDamage} quality={quality.finalDamage} ratio={quality.finalDamage/baseline.finalDamage} scope=isolated-section-5.2-hit");
        }
        [TestCase(30,12)][TestCase(60,102)][TestCase(90,192)]
        public void CurveFixturesKeepAllSixBuildsLegalAndTheirComparisonInputsMatched(int stage,int cap)
        {
            for(int c=0;c<3;c++)for(int variant=0;variant<2;variant++)
            {
                var baseline=Hellscript.Editor.BuildIntegrationValidation.QualityCurveAccount((HeroClass)c,variant,stage,77891,false);
                var quality=Hellscript.Editor.BuildIntegrationValidation.QualityCurveAccount((HeroClass)c,variant,stage,77891,true);
                Assert.AreEqual(Json(baseline.Hero.build),Json(quality.Hero.build));Assert.AreEqual(8,quality.Hero.inventory.Count);
                for(int slot=0;slot<8;slot++)
                {
                    var a=baseline.Hero.inventory[slot];var b=quality.Hero.inventory[slot];ItemCatalog.Validate(a);ItemCatalog.Validate(b);
                    Assert.AreEqual(a.baseId,b.baseId);Assert.AreEqual(a.special,b.special);Assert.AreEqual(Math.Min(60,stage),b.level);
                    Assert.AreEqual(5,a.enhancement);Assert.AreEqual(5,b.enhancement);Assert.IsTrue(a.equipped&&b.equipped);
                    Assert.IsFalse(a.awakened);Assert.Zero(a.masterwork);Assert.IsTrue(b.awakened);Assert.AreEqual(cap,b.masterwork);
                    Assert.AreEqual(3,b.masterworkLines.Count);Assert.IsEmpty(a.sockets);Assert.IsEmpty(b.sockets);
                    for(int line=0;line<a.rolls.Count;line++)
                    {Assert.AreEqual(a.rolls[line].affixId,b.rolls[line].affixId);Assert.AreEqual(a.rolls[line].value,b.rolls[line].value);Assert.AreEqual(a.rolls[line].rollBasisPoints,b.rolls[line].rollBasisPoints);}
                }
            }
        }
    }
}
