using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace Hellscript.Tests
{
    public sealed class GemSocketTests
    {
        GameCatalog catalog;AccountSave account;uint rng;string directory;
        [SetUp] public void Setup()
        {Loc.UseSource();catalog=ScriptableObject.CreateInstance<GameCatalog>();catalog.Populate();account=GameStore.NewAccount(catalog);rng=998871;directory=Path.Combine(Path.GetTempPath(),"hellscript-gems-"+Guid.NewGuid());}
        [TearDown] public void Teardown()
        {Loc.UseSource();UnityEngine.Object.DestroyImmediate(catalog);if(Directory.Exists(directory))Directory.Delete(directory,true);}
        static string Json(object value)=>JsonUtility.ToJson(value);
        Item Gear(int slot=1,int rarity=2,bool equipped=true)
        {
            foreach(var old in account.Hero.inventory.Where(i=>i.slot==slot))old.equipped=false;
            var item=ItemGenerator.Create(account.Hero.heroClass,slot,rarity,30,ref rng);item.equipped=equipped;account.Hero.inventory.Add(item);account.Hero.level=30;
            return item;
        }
        static void Install(Item item,string id,int tier=6)=>item.sockets=new List<SocketState>{new SocketState{index=0,gemId=id,tier=tier}};
        static object Call(CombatSimulation sim,string name,params object[] arguments)
        {
            var method=typeof(CombatSimulation).GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic);var p=method.GetParameters();var filled=new object[p.Length];
            for(int i=0;i<filled.Length;i++)filled[i]=i<arguments.Length?arguments[i]:p[i].DefaultValue;return method.Invoke(sim,filled);
        }
        CombatSimulation Simulation()
        {
            account.Hero.build.passives=Array.Empty<int>();account.Hero.build.potionThreshold=0;account.Hero.build.rules.Clear();account.Hero.build.activeSkills.Clear();
            var sim=new CombatSimulation(account,catalog,1,1,seed:993211);sim.State.position=RiftMap.Rooms[0];sim.State.enemies.Clear();sim.State.decisionTime=1000;
            sim.Stats.dodge=sim.Stats.blockChance=sim.Stats.overpower=0;
            sim.State.enemies.Add(new EnemyState{id=900,position=sim.State.position+Vector2.up*6,health=1000000,maxHealth=1000000,speed=0,attack=0,cooldown=1000,brain=new EnemyBrain{initialized=true}});
            return sim;
        }
        // Independent expected rows from the adopted design, exercised through the real stat owner.
        static IEnumerable Effects()
        {
            float[] attack={3,5,8,12,17,23},resist={8,14,22,32,45,60},utility={4,7,11,16,22,30};
            float[][] armor={new float[]{2,3.5f,5,7,9.5f,12},new float[]{1,1.5f,2.5f,3.5f,5,6.5f},new float[]{.4f,.7f,1.1f,1.6f,2.2f,3},new float[]{3,5,8,11,15,20},new float[]{1,1.5f,2.5f,3.5f,4.5f,6},new float[]{3,5,8,12,16,21},new float[]{3,5,8,12,16,21}};
            StatId[] weaponStats={StatId.FireDamage,StatId.ColdDamage,StatId.LightningDamage,StatId.PoisonDamage,StatId.ShadowDamage,StatId.PhysicalDamage,StatId.CriticalStrikeDamage};
            StatId[] armorStats={StatId.MaximumLifePercent,StatId.DamageReduction,StatId.ResourceGeneration,StatId.DamageReduction,StatId.MovementSpeed,StatId.BarrierGeneration,StatId.Armor};
            StatId[] jewelryStats={StatId.FireResistance,StatId.ColdResistance,StatId.LightningResistance,StatId.PoisonResistance,StatId.ShadowResistance,StatId.AllResistance,StatId.PotionHealing};
            GemEffectKind[] armorKinds={GemEffectKind.Stat,GemEffectKind.BuffReduction,GemEffectKind.Stat,GemEffectKind.PeriodicReduction,GemEffectKind.Stat,GemEffectKind.Stat,GemEffectKind.ArmorPercent};
            for(int gem=0;gem<7;gem++)for(int tier=1;tier<=6;tier++)
            {
                string id="G0"+(gem+1);
                yield return new TestCaseData(id,tier,0,GemEffectKind.Stat,weaponStats[gem],(gem==6?utility:attack)[tier-1]).SetName($"Gem_{id}_Tier{tier}_Weapon");
                yield return new TestCaseData(id,tier,1,armorKinds[gem],armorStats[gem],armor[gem][tier-1]).SetName($"Gem_{id}_Tier{tier}_Armor");
                yield return new TestCaseData(id,tier,6,GemEffectKind.Stat,jewelryStats[gem],(gem>=5?utility:resist)[tier-1]).SetName($"Gem_{id}_Tier{tier}_Jewelry");
            }
        }
        [TestCaseSource(nameof(Effects))]
        public void AllAdoptedEffectsReachTheCorrectStatBucket(string id,int tier,int slot,GemEffectKind kind,StatId stat,float expected)
        {
            var item=Gear(slot);var before=new HeroStats(account.Hero);Install(item,id,tier);ItemCatalog.Validate(item);
            Assert.IsTrue(GemCatalog.TryEffect(item,out var effect,out float value));Assert.AreEqual(kind,effect.kind);Assert.AreEqual(stat,effect.stat);Assert.AreEqual(expected,value,.00001f);
            var after=new HeroStats(account.Hero);
            if(kind==GemEffectKind.Stat){Assert.AreEqual(expected,after.Bonus(stat)-before.Bonus(stat),.0001f);Assert.AreEqual(expected,after.gemBonuses[(int)stat],.0001f);}
            if(kind==GemEffectKind.BuffReduction){Assert.AreEqual(expected/100,after.gemBuffReduction,.0001f);Assert.AreEqual(before.damageReduction,after.damageReduction);}
            if(kind==GemEffectKind.PeriodicReduction){Assert.AreEqual(expected/100,after.gemPeriodicReduction,.0001f);Assert.AreEqual(before.damageReduction,after.damageReduction);}
            if(kind==GemEffectKind.ArmorPercent)Assert.AreEqual(before.armor*(1+expected/100),after.armor,.001f);
            item.equipped=false;Assert.IsFalse(new HeroStats(account.Hero).gemBonuses.Any(v=>v!=0));
        }
        [Test]
        public void BothArmorAndJewelrySlotsUseTheSameRowsAndEachSlotHasItsOwnOneSocketLimit()
        {
            foreach(var gem in GemCatalog.Gems)
            {Assert.AreSame(gem.ForSlot(1),gem.ForSlot(2));Assert.AreSame(gem.ForSlot(6),gem.ForSlot(7));}
            for(int slot=0;slot<8;slot++)for(int rarity=0;rarity<4;rarity++)
            {
                var item=Gear(slot,rarity,false);Install(item,"G01",1);bool allowed=rarity>=2&&new[]{0,1,2,6,7}.Contains(slot);
                Assert.AreEqual(allowed,GemCatalog.AllowsSocket(item));
                if(allowed){Assert.DoesNotThrow(()=>ItemCatalog.Validate(item));item.sockets.Add(new SocketState{index=1});Assert.Throws<ArgumentException>(()=>ItemCatalog.Validate(item));}
                else Assert.Throws<ArgumentException>(()=>ItemCatalog.Validate(item));
            }
        }
        [Test]
        public void TwoSocketsAddBeforeTheSharedMovementBuffAndShieldCaps()
        {
            var head=Gear(1);var body=Gear(2);Install(head,"G05");Install(body,"G05");var stats=new HeroStats(account.Hero);
            Assert.AreEqual(6,stats.SpeedWithBonus(10),.001f);
            Install(head,"G02");Install(body,"G02");stats=new HeroStats(account.Hero);Assert.AreEqual(.13f,stats.gemBuffReduction,.00001f);Assert.AreEqual(.5f,stats.BuffReduction(.49f));
            Install(head,"G06");Install(body,"G06");var sim=Simulation();float multiplier=sim.Stats.shieldMultiplier;
            Call(sim,"AddShield","GEM_TEST",100f,5f,1);Assert.AreEqual(100*multiplier,sim.State.shield,.001f);
            Call(sim,"AddShield","GEM_TEST_CAP",100000f,5f,2);Assert.AreEqual(sim.Stats.hp,sim.State.shield,.001f);
        }
        [Test]
        public void MaximumLifeResourceAndPotionBonusesAffectFinishedValuesAndActualHealing()
        {
            var head=Gear(1);var before=new HeroStats(account.Hero);Install(head,"G01");var after=new HeroStats(account.Hero);
            Assert.AreEqual(before.hp/(1+before.Bonus(StatId.MaximumLifePercent)/100)*.12f,after.hp-before.hp,.001f);
            Install(head,"G03");after=new HeroStats(account.Hero);Assert.AreEqual(3,after.regen-before.regen,.001f);
            var ring=Gear(7);Install(ring,"G07");var sim=Simulation();sim.State.health=1;sim.State.potionCd=0;
            Assert.IsTrue((bool)Call(sim,"TryUsePotion",1f));Assert.AreEqual(Mathf.Min(sim.Stats.hp,1+sim.Stats.hp*.35f*sim.Stats.healing*sim.Stats.potionHealing),sim.State.health,.001f);
        }
        [TestCase("G01",Element.Fire)][TestCase("G02",Element.Cold)][TestCase("G03",Element.Lightning)][TestCase("G04",Element.Poison)][TestCase("G05",Element.Shadow)][TestCase("G06",Element.Physical)]
        public void WeaponGemsEnterActualAdditiveDamageWithoutAnIndependentMultiplier(string id,int element)
        {
            var weapon=Gear(0);var before=Simulation();Install(weapon,id);var after=Simulation();
            DamageEvent Hit(CombatSimulation sim)=>(DamageEvent)Call(sim,"Hit",sim.State.enemies[0],1f,element,false,0f,null,false);
            var empty=Hit(before);var gemmed=Hit(after);Assert.AreEqual(.23f,gemmed.additive-empty.additive,.0001f);Assert.AreEqual(empty.independent,gemmed.independent);
            Assert.AreEqual(empty.finalDamage/(1+empty.additive)*(1+gemmed.additive),gemmed.finalDamage,.001f);
        }
        [Test]
        public void ResistanceRatingsRetainTheExistingSeventyPercentCeiling()
        {
            var ring=Gear(7);Install(ring,"G01");var stats=new HeroStats(account.Hero);
            Assert.AreEqual(60,stats.gemBonuses[(int)StatId.FireResistance]);
            var numbers=DamageMath.Calculate(100,0,1,1,60,30,Element.Fire,0);Assert.AreEqual(60f/460,numbers.defenseReduction,.0001f);
            numbers=DamageMath.Calculate(100,0,1,1,100000,30,Element.Fire,0);Assert.AreEqual(.7f,numbers.defenseReduction);
        }
        [TestCase(false)][TestCase(true)]
        public void PeriodicGemReductionMatchesForecastAndLiveDamageButLeavesSingleHitsAlone(bool legacy)
        {
            var head=Gear(1);Install(head,"G04");var sim=Simulation();sim.Stats.armor=sim.Stats.resistance=sim.Stats.damageReduction=sim.Stats.physicalReduction=sim.Stats.lifeRegen=0;
            sim.State.enemyHazards.Add(new EnemyHazard{id=901,enemyId=900,definitionId="SINGLE",position=sim.State.position,shape=AttackShape.Circle,radius=2,createdAt=-1,damage=10,delay=.1f});
            if(legacy)sim.State.effects.Add(new GroundEffect{id=902,hostile=true,kind=4,position=sim.State.position,radius=2,createdAt=-1,damage=10,duration=.7f,tick=.1f});
            else sim.State.enemyHazards.Add(new EnemyHazard{id=902,enemyId=900,definitionId="PERIODIC",position=sim.State.position,shape=AttackShape.Circle,radius=2,createdAt=-1,damage=10,duration=.7f,interval=.25f,tick=.25f});
            string unchanged=Json(sim.State);var forecast=sim.ForecastIncoming(1);Assert.AreEqual(unchanged,Json(sim.State));
            for(int n=0;n<20;n++)sim.Tick(CombatSimulation.Step);
            var hits=sim.State.damageEvents.Where(e=>e.incoming).ToArray();Assert.AreEqual(forecast.HpLoss,hits.Sum(e=>e.hpLoss),.001f);
            Assert.AreEqual(10,hits.Single(e=>e.definitionId=="SINGLE").finalDamage,.0001f);
            foreach(var hit in hits.Where(e=>e.definitionId!="SINGLE")){Assert.AreEqual(DamageKind.Periodic,hit.kind);Assert.AreEqual(hit.baseAttack*.8f,hit.finalDamage,.0001f);}
        }
        [Test]
        public void ARepeatedGroundKeepsItsKindThroughItsLastTickAndARunReload()
        {
            var head=Gear(1);Install(head,"G04");var sim=Simulation();sim.Stats.armor=0;
            sim.State.effects.Add(new GroundEffect{id=902,hostile=true,periodic=true,kind=0,position=sim.State.position,radius=2,createdAt=-1,damage=10,duration=.05f,tick=0});
            var restored=JsonUtility.FromJson<RunState>(Json(sim.State));GameStore.NormalizeRun(restored);
            Assert.IsTrue(restored.effects.Single().PeriodicIncoming);var forecast=sim.ForecastIncoming(.1f);sim.Tick(CombatSimulation.Step);
            Assert.AreEqual(DamageKind.Periodic,sim.State.damageEvents.Single(e=>e.incoming).kind);Assert.AreEqual(forecast.HpLoss,sim.State.damageEvents.Single(e=>e.incoming).hpLoss,.001f);
        }
        [Test]
        public void BlueGemReductionRemainsAfterTimedBuffsExpireInForecastAndActualHits()
        {
            var head=Gear(1);var body=Gear(2);Install(head,"G02");Install(body,"G02");var sim=Simulation();
            sim.Stats.armor=sim.Stats.resistance=sim.Stats.damageReduction=sim.Stats.physicalReduction=sim.Stats.lifeRegen=0;sim.State.itemEffects.leapDefense=.4f;sim.State.resolveShrineTime=.5f;
            foreach(float delay in new[]{.1f,.75f})sim.State.enemyHazards.Add(new EnemyHazard{id=sim.State.nextId++,enemyId=900,definitionId="BLUE_TEST",position=sim.State.position,shape=AttackShape.Circle,radius=2,createdAt=-1,damage=10,delay=delay});
            Assert.AreEqual(.38f,sim.BuffDamageReduction,.0001f);var forecast=sim.ForecastIncoming(1);for(int n=0;n<20;n++)sim.Tick(CombatSimulation.Step);
            var hits=sim.State.damageEvents.Where(e=>e.incoming).ToArray();Assert.AreEqual(6.2f,hits[0].finalDamage,.0001f);Assert.AreEqual(8.7f,hits[1].finalDamage,.0001f);Assert.AreEqual(forecast.HpLoss,hits.Sum(e=>e.hpLoss),.001f);
        }
        static GemStack Stack(string id="G01",int tier=1,int count=1)=>new GemStack{gemId=id,tier=tier,count=count};
        [Test]
        public void FusionFromTierOneToTierSixConsumesExactly3125MaterialsAndNoGold()
        {
            var bag=new List<GemStack>();Assert.IsTrue(GemStacks.TryAdd(bag,50,Stack(count:3125)));int gold=100;
            for(int tier=1;tier<6;tier++)while(GemStacks.Count(bag,"G01",tier)>=5)Assert.IsTrue(GemStacks.TryFuse(bag,50,"G01",tier,ref gold));
            Assert.AreEqual(100,gold);Assert.AreEqual(1,bag.Count);Assert.AreEqual(6,bag[0].tier);Assert.AreEqual(1,bag[0].count);
            Assert.AreEqual(3125,GemCatalog.TierOneMaterials(6));Assert.AreEqual(0,GemCatalog.TotalFusionGold(6));
            CollectionAssert.AreEqual(new[]{0,0,0,0,0},Enumerable.Range(2,5).Select(GemCatalog.FusionGold));
        }
        [TestCase(2,900,50)][TestCase(3,899,50)][TestCase(6,900,1)]
        public void FailedFusionLeavesAllStacksAndGoldIntact(int count,int gold,int capacity)
        {
            var bag=new List<GemStack>{Stack(count:count)};int beforeGold=gold;string before=Json(bag[0]);
            Assert.IsFalse(GemStacks.TryFuse(bag,capacity,"G01",1,ref gold));Assert.AreEqual(beforeGold,gold);Assert.AreEqual(1,bag.Count);Assert.AreEqual(before,Json(bag[0]));
        }
        [Test]
        public void AConsumedStackCanSupplyTheOutputSlotAndTopTierCannotBeFused()
        {
            var bag=new List<GemStack>{Stack(count:5)};int gold=100000;
            Assert.IsTrue(GemStacks.TryFuse(bag,1,"G01",1,ref gold));Assert.AreEqual(100000,gold);Assert.AreEqual(2,bag.Single().tier);
            bag[0].tier=6;bag[0].count=3;Assert.IsFalse(GemStacks.TryFuse(bag,1,"G01",6,ref gold));Assert.AreEqual(100000,gold);Assert.AreEqual(3,bag[0].count);
        }
        [Test]
        public void StacksFillTo999AndOverflowOrExchangeIsAtomic()
        {
            var bag=new List<GemStack>{Stack(count:998)};Assert.IsTrue(GemStacks.TryAdd(bag,2,Stack(count:1000)));
            CollectionAssert.AreEqual(new[]{999,999},bag.Select(s=>s.count));var references=bag.ToArray();
            Assert.IsFalse(GemStacks.TryAdd(bag,2,Stack()));CollectionAssert.AreEqual(references,bag);Assert.AreEqual(1998,GemStacks.Count(bag,"G01",1));
            Assert.IsFalse(GemStacks.TryExchange(bag,2,new[]{Stack(count:3)},new[]{Stack("G02")}));CollectionAssert.AreEqual(references,bag);Assert.AreEqual(1998,GemStacks.Count(bag,"G01",1));
            Assert.IsTrue(GemStacks.TryExchange(bag,2,new[]{Stack(count:999)},new[]{Stack("G02")}));Assert.AreEqual(999,GemStacks.Count(bag,"G01",1));Assert.AreEqual(1,GemStacks.Count(bag,"G02",1));
        }
        [TestCase(5,true)][TestCase(6,false)]
        public void FusionConsumesTheSmallOverflowStackBeforeAFullStackToReuseItsSlot(int overflow,bool succeeds)
        {
            var bag=new List<GemStack>{Stack(count:999),Stack(count:overflow)};int gold=900;
            Assert.AreEqual(succeeds,GemStacks.TryFuse(bag,2,"G01",1,ref gold));
            Assert.AreEqual(900,gold);Assert.AreEqual(succeeds?999:999+overflow,GemStacks.Count(bag,"G01",1));Assert.AreEqual(succeeds?1:0,GemStacks.Count(bag,"G01",2));
        }
        [TestCase("INVALID",1,1)][TestCase("G01",0,1)][TestCase("G01",7,1)][TestCase("G01",1,0)][TestCase("G01",1,-1)][TestCase("G01",1,1000)]
        public void InvalidPersistedStacksAreRejectedWithoutRepairingTheirQuantities(string id,int tier,int count)
        {Assert.Throws<ArgumentException>(()=>GemStacks.Validate(new[]{Stack(id,tier,count)},50));}
        [TestCase(1,1,1)][TestCase(9,1,1)][TestCase(10,1,2)][TestCase(19,1,2)][TestCase(20,2,3)][TestCase(29,2,3)][TestCase(30,3,3)][TestCase(100,3,3)]
        public void RewardTierBoundariesAndSeedReplayNeverProduceFusionOnlyTiers(int stage,int minimum,int maximum)
        {
            uint a=81723,b=a;var counts=new int[7];int lower=0;
            for(int n=0;n<10000;n++)
            {
                var left=GemCatalog.Roll(stage,ref a);var right=GemCatalog.Roll(stage,ref b);Assert.AreEqual(Json(left),Json(right));Assert.That(left.tier,Is.InRange(minimum,maximum));Assert.AreEqual(1,left.count);
                counts[int.Parse(left.gemId.Substring(1))-1]++;if(left.tier==minimum)lower++;
            }
            foreach(int count in counts)Assert.That(count,Is.InRange(1200,1650));if(minimum!=maximum)Assert.That(lower,Is.InRange(6700,7300));
        }
        [Test]
        public void GemmedEquipmentCannotBeSoldSalvagedOrAutomaticallyReplaced()
        {
            var item=Gear(equipped:false);Install(item,"G01");account.Hero.capacity=account.Hero.inventory.Count(i=>!i.equipped);
            int gold=account.gold,materials=account.materials;Assert.IsFalse(Economy.Sell(account,account.Hero,item));Assert.IsFalse(Economy.Dismantle(account,account.Hero,item));
            var replacement=ItemGenerator.Create(account.Hero.heroClass,1,3,40,ref rng);Assert.IsFalse(Economy.AddItem(account.Hero,replacement,BagPolicy.Replace,account));
            Assert.AreEqual(gold,account.gold);Assert.AreEqual(materials,account.materials);Assert.Contains(item,account.Hero.inventory);
        }
        [TestCase(InventoryBulkOperation.Sell)][TestCase(InventoryBulkOperation.Dismantle)]
        public void BulkPreviewExcludesGemsAndCannotApplyAfterSocketChanges(InventoryBulkOperation operation)
        {
            var item=Gear(equipped:false);var plan=new InventoryBulkPlan(account,new[]{item.id},operation);Assert.AreEqual(1,plan.Count);
            Install(item,"G01");Assert.IsFalse(plan.Apply(account));plan=new InventoryBulkPlan(account,new[]{item.id},operation);Assert.AreEqual(0,plan.Count);Assert.That(plan.entries.Single().excludedReason,Does.Contain("보석"));
        }
        EdictCleanupPolicy Cleanup(string action,bool protect)
        {
            var d=HuntEdictV2Storage.CreateForHero(account.Hero);
            d=HuntEdictV2Editing.WithGlobal(d,"bag.cleanup.RARE",action);d=HuntEdictV2Editing.WithGlobal(d,"bag.protectSocketed",protect?"ON":"OFF");
            return RepeatHuntPolicy.Compile(account.Hero.build,d).cleanup;
        }
        [TestCase("SELL",false)][TestCase("SELL",true)][TestCase("SALVAGE",false)][TestCase("SALVAGE",true)][TestCase("WAREHOUSE",true)]
        public void AutoCleanupCannotDestroyGemsAndItsExtraProtectionAlsoStopsWarehouseMoves(string action,bool protect)
        {
            var item=Gear(equipped:false);Install(item,"G01");var policy=Cleanup(action,protect);Assert.AreEqual(protect,policy.protectGemmed);var report=policy.Apply(account);
            Assert.AreEqual(1,report.protectedItems);Assert.Contains(item,account.Hero.inventory);Assert.IsEmpty(account.warehouse);
        }
        [Test]
        public void WarehouseMoveKeepsSocketWhenExtraProtectionIsOff()
        {var item=Gear(equipped:false);Install(item,"G01");var report=Cleanup("WAREHOUSE",false).Apply(account);Assert.AreEqual(1,report.stored);Assert.AreSame(item,account.warehouse.Single());Assert.AreEqual(6,item.sockets.Single().tier);}
        [Test]
        public void ValidSocketsSurviveARealStoreRestartAndLegacyAbsenceStaysEmpty()
        {
            var store=new GameStore(directory,catalog);account=store.Data;var item=Gear();Install(item,"G07");Assert.IsTrue(store.Save());string id=item.id;
            var loaded=new GameStore(directory,catalog);Assert.AreEqual("G07",loaded.Data.Hero.inventory.Single(i=>i.id==id).sockets.Single().gemId);Assert.IsEmpty(loaded.GemRecoveryArchive);
            Assert.IsTrue(loaded.Data.heroes.Skip(1).SelectMany(h=>h.inventory).All(i=>i.sockets.Count==0));
        }
        [TestCase("G99",1)][TestCase("G01",0)][TestCase("G01",7)][TestCase("",5)]
        public void InvalidGemPayloadBecomesEmptyOnlyAfterArchivingTheExactOriginalBytes(string id,int tier)
        {
            var store=new GameStore(directory,catalog);account=store.Data;var item=Gear();Install(item,id,tier);string original=Json(account),path=Path.Combine(directory,"hellscript-local-v1.json");File.WriteAllText(path,original);byte[] bytes=File.ReadAllBytes(path);
            var loaded=new GameStore(directory,catalog);var recovered=loaded.Data.Hero.inventory.Single(i=>i.id==item.id);Assert.AreEqual("",recovered.sockets.Single().gemId);Assert.AreEqual(0,recovered.sockets.Single().tier);Assert.AreEqual(item.baseId,recovered.baseId);
            CollectionAssert.AreEqual(bytes,File.ReadAllBytes(loaded.GemRecoveryArchive));Assert.IsNotEmpty(loaded.GemRecoveryMessage);Assert.IsTrue(loaded.Save());
            var again=new GameStore(directory,catalog);Assert.IsEmpty(again.GemRecoveryArchive);Assert.AreEqual(1,Directory.GetFiles(directory,"*.gem-recovery-*.json").Length);
        }
        [Test]
        public void UnknownSocketStructureStopsLoadingWithoutFallingBackToAnOlderAccount()
        {
            var store=new GameStore(directory,catalog);account=store.Data;var item=Gear();Install(item,"G01");item.sockets[0].index=2;
            string path=Path.Combine(directory,"hellscript-local-v1.json"),original=Json(account);File.WriteAllText(path,original);
            Assert.Throws<NotSupportedException>(()=>new GameStore(directory,catalog));Assert.AreEqual(original,File.ReadAllText(path));
        }
        [Test]
        public void SocketSaveUsesAnItemVersionThatOlderPlayersReject()
        {
            var store=new GameStore(directory,catalog);account=store.Data;var item=Gear();item.contentVersion=2;Install(item,"G01");
            Assert.IsTrue(store.Save());Assert.AreEqual(GemCatalog.SocketItemVersion,item.contentVersion);Assert.Greater(item.contentVersion,2);
            var loaded=new GameStore(directory,catalog);Assert.AreEqual(GemCatalog.SocketItemVersion,loaded.Data.Hero.inventory.Single(i=>i.id==item.id).contentVersion);
        }
        [Test]
        public void EverySocketDescriptionHasCompleteEnglishIncludingFractionalArmorEffects()
        {
            Loc.Use("en",LocalizationTable.Parse(Resources.Load<TextAsset>("Localization/en").text).Entries);
            foreach(var gem in GemCatalog.Gems)for(int tier=1;tier<=6;tier++)foreach(int slot in new[]{0,1,6})
            {var item=Gear(slot);Install(item,gem.id,tier);Assert.IsNotEmpty(GemCatalog.SocketSummary(item));}
            Assert.AreEqual(0,Loc.MissingCount,string.Join("; ",Loc.Missing));
        }
    }
}
