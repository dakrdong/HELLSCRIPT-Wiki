using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hellscript.Tests
{
    public sealed class ItemizationTests
    {
        string directory;
        [SetUp]public void Setup()=>directory=Path.Combine(Path.GetTempPath(),"HellscriptItems-"+Guid.NewGuid());
        [TearDown]public void Teardown(){if(Directory.Exists(directory))Directory.Delete(directory,true);}
        [Test]public void EveryClassSlotGradeProducesLegalCompleteAffixes()
        {
            uint rng=8411;
            for(int c=0;c<3;c++)for(int slot=0;slot<8;slot++)for(int grade=0;grade<4;grade++)for(int n=0;n<100;n++)
            {
                var item=ItemGenerator.Create((HeroClass)c,slot,grade,new[]{1,9,10,29,30,31,100}[n%7],ref rng);
                Assert.IsTrue(ItemCatalog.Base(item).Fits((HeroClass)c,slot));Assert.AreEqual(0,item.affixes.Count);
                Assert.That(item.rolls.Count,grade==0?Is.EqualTo(0):grade==1?Is.InRange(1,2):Is.EqualTo(grade==2?3:4));
                Assert.AreEqual(item.rolls.Count,item.rolls.Select(r=>ItemCatalog.Affix(r.affixId).group).Distinct().Count());
                Assert.IsTrue(item.rolls.All(r=>ItemCatalog.Affix(r.affixId).Allows(slot)&&r.value==ItemCatalog.Affix(r.affixId).Value(item.level,r.rollBasisPoints)));
                if(grade>=2){Assert.IsTrue(item.rolls.Any(r=>r.side==AffixSide.Prefix));Assert.IsTrue(item.rolls.Any(r=>r.side==AffixSide.Suffix));}
                if(grade==3)Assert.IsTrue(ItemCatalog.Unique(item.special).Fits((HeroClass)c,slot));
            }
        }
        [Test]public void EveryUniqueHasAValidBaseAndExactlyFourOptions()
        {
            Assert.AreEqual(24,ItemCatalog.Bases.Count);Assert.AreEqual(StatCatalog.Count-3,ItemCatalog.Affixes.Count,"One affix per stat that does something.");Assert.AreEqual(6,ItemCatalog.Sets.Count);Assert.AreEqual(39,ItemCatalog.Uniques.Count);
            uint rng=234;
            foreach(var d in ItemCatalog.Uniques)
            {
                var item=ItemGenerator.Create((HeroClass)Math.Max(0,d.heroClass),d.slot,3,30,ref rng,uniqueId:d.id);
                Assert.AreEqual(d.name,item.DisplayName);Assert.AreEqual(4,item.rolls.Count);
            }
        }
        // Boots used to hold only two prefix groups, so the three-prefix split could never be
        // filled and its weight was shared out among the rest. Taking on Diablo IV's sheet gave the
        // slot a third prefix group and a fourth suffix group, so every split is now reachable and
        // the written weights stand as they are: 60 / 20 / 20.
        [Test]public void FootAllocationAndLowLevelTiersFollowTheirWeights()
        {
            uint rng=7621;int prefixes2=0,t1=0,t2=0,t3=0,samples=20000;
            for(int i=0;i<samples;i++)
            {
                var item=ItemGenerator.Create(HeroClass.Warrior,4,3,1,ref rng);
                int p=item.rolls.Count(r=>r.side==AffixSide.Prefix);Assert.That(p,Is.InRange(1,3));if(p==2)prefixes2++;
                foreach(var r in item.rolls){if(r.tierId=="T1")t1++;else if(r.tierId=="T2")t2++;else t3++;}
            }
            Assert.That(prefixes2/(double)samples,Is.InRange(.58,.62));
            Assert.That(t1/(double)(samples*4),Is.InRange(.045,.055));Assert.That(t2/(double)(samples*4),Is.InRange(.24,.26));Assert.That(t3/(double)(samples*4),Is.InRange(.69,.71));
        }
        [Test]public void QualityBoundariesScaleOnlyFlatStats()
        {
            int[] q={0,4999,5000,8499,8500,10000};string[] tier={"T3","T3","T2","T2","T1","T1"};
            for(int n=0;n<q.Length;n++)Assert.AreEqual(tier[n],ItemGenerator.Tier(q[n]));
            Assert.AreEqual(7.5f,ItemCatalog.Affix("AF05").Value(30,9000));
            Assert.AreEqual(8,ItemCatalog.Affix("AF05").Value(100,10000));
            Assert.Greater(ItemCatalog.Affix("AF01").Value(100,10000),ItemCatalog.Affix("AF01").Value(30,10000));
            foreach(int level in new[]{1,9,10,29,30,31,100})foreach(var def in ItemCatalog.Affixes)foreach(int quality in q)
            {Assert.GreaterOrEqual(def.Value(level,quality),def.min*def.Scale(level)-.0001f);Assert.LessOrEqual(def.Value(level,quality),def.max*def.Scale(level)+.0001f);}
            uint rng=8;var item=ItemGenerator.Create(HeroClass.Warrior,0,0,100,ref rng);Assert.AreEqual(30,item.RequiredLevel);
        }
        [Test]public void RerollKeepsSelectedIdentitySideAndCreationClass()
        {
            var a=GameStore.NewAccount();a.gold=100000000;a.Hero.highestClear=8;uint rng=918;
            var item=ItemGenerator.Create(HeroClass.Mage,6,2,30,ref rng);a.Hero.inventory.Add(item);
            var selected=item.rolls.Last();var untouched=item.rolls.Take(item.rolls.Count-1).Select(JsonUtility.ToJson).ToArray();
            Assert.IsTrue(ItemGenerator.RerollPool(item,selected.slotId).Any(d=>d.id==selected.affixId));
            for(int n=0;n<12;n++)Assert.IsTrue(Economy.Reroll(a,item,selected.slotId,ref rng));
            Assert.AreEqual(HeroClass.Mage,item.lootClass);Assert.AreEqual(selected.slotId,item.rerollSlotId);Assert.AreEqual(selected.side,item.rolls.Last().side);
            CollectionAssert.AreEqual(untouched,item.rolls.Take(item.rolls.Count-1).Select(JsonUtility.ToJson).ToArray());
            string before=JsonUtility.ToJson(a);uint prior=rng;Assert.IsFalse(Economy.Reroll(a,item,item.rolls[0].slotId,ref rng));Assert.AreEqual(before,JsonUtility.ToJson(a));Assert.AreEqual(prior,rng);
            Assert.AreEqual(500L*30+50L*30*(30-1),Economy.RerollGold(item));
        }
        [Test]public void ForeignItemsAndClassRestrictedArmorCannotBeEquippedOrServiced()
        {
            var a=GameStore.NewAccount();uint rng=111;var foreign=ItemGenerator.Create(HeroClass.Warrior,1,2,1,ref rng);a.gold=a.materials=100000;
            Assert.IsFalse(Economy.Equip(a.Hero,foreign));Assert.IsFalse(Economy.Enhance(a,foreign));Assert.IsFalse(Economy.Reroll(a,foreign,0,ref rng));
            var mageBoots=ItemGenerator.Create(HeroClass.Mage,4,3,1,ref rng,uniqueId:"SM4");a.Hero.inventory.Add(mageBoots);Assert.IsFalse(Economy.Equip(a.Hero,mageBoots));
            a.suspendedRun=new RunState{heroId=a.Hero.id};Assert.IsFalse(Economy.Enhance(a,a.Hero.inventory[0]));
        }
        [Test]public void MixedSetsAndDuplicateSlotsNeverProduceFourPieceBonus()
        {
            var a=GameStore.NewAccount();uint rng=777;
            foreach(string id in new[]{"SW1","SW2","SWB3","SWB4","SW1"})
            {var d=ItemCatalog.Unique(id);var item=ItemGenerator.Create(HeroClass.Warrior,d.slot,3,1,ref rng,uniqueId:id);item.equipped=true;a.Hero.inventory.Add(item);}
            var stats=new HeroStats(a.Hero);Assert.AreEqual(2,stats.SetPieces("SW"));Assert.AreEqual(2,stats.SetPieces("SWB"));
        }
        [Test]public void PresetReferenceProtectsDestructionAndAutomaticReplacement()
        {
            var a=GameStore.NewAccount();uint rng=991;var item=ItemGenerator.Create(HeroClass.Warrior,1,2,1,ref rng);a.Hero.inventory.Add(item);a.Hero.capacity=1;
            var preset=GameCatalog.Preset(HeroClass.Warrior,0);preset.equipmentIds.Add(item.id);a.Hero.presets.Add(null);a.Hero.presets.Add(preset);
            Assert.IsFalse(Economy.Sell(a,a.Hero,item));Assert.IsFalse(Economy.Dismantle(a,a.Hero,item));Assert.IsFalse(Economy.AddItem(a.Hero,ItemGenerator.Create(HeroClass.Warrior,1,3,10,ref rng),BagPolicy.Replace));
        }
        [Test]public void LegacyMigrationPreservesValuesAndStableRerollSlotWithoutDrawing()
        {
            Directory.CreateDirectory(directory);var a=GameStore.NewAccount();a.schema=1;var item=a.Hero.inventory[0];item.contentVersion=0;item.rarity=2;
            item.affixes.AddRange(new[]{10,11,17});item.values.AddRange(new[]{9.3f,12.7f,4.1f});item.enhancement=5;item.locked=true;item.rerollIndex=1;item.rerolls=3;
            string path=Path.Combine(directory,"hellscript-local-v1.json");File.WriteAllText(path,JsonUtility.ToJson(a));
            var loaded=new GameStore(directory);var result=loaded.Data.Hero.inventory[0];
            Assert.AreEqual(2,loaded.Data.schema);Assert.AreEqual(item.id,result.id);Assert.AreEqual(9.3f,result.Value(10));Assert.AreEqual(12.7f,result.Value(11));Assert.AreEqual(4.1f,result.Value(17));
            Assert.AreEqual(620,result.investedMaterials);Assert.AreEqual(5,result.enhancement);Assert.IsTrue(result.locked);Assert.AreEqual(result.rolls[1].slotId,result.rerollSlotId);Assert.IsTrue(result.rolls.All(r=>r.legacyRoll));
            Assert.AreEqual(1,Directory.GetFiles(directory,"*.schema1-*.json").Length);
            string first=JsonUtility.ToJson(result);Assert.IsTrue(loaded.Save());Assert.AreEqual(first,JsonUtility.ToJson(new GameStore(directory).Data.Hero.inventory[0]));
        }
        [Test]public void TransactionCommitsOnceAndKeepsLiveHeroIdentity()
        {
            var store=new GameStore(directory);var hero=store.Data.Hero;store.Data.gold=5000;uint rng=44;var item=ItemGenerator.Create(HeroClass.Warrior,1,2,1,ref rng);hero.inventory.Add(item);store.Save();
            Assert.IsTrue(store.Transact("one","sell:"+item.id,a=>Economy.Sell(a,a.Hero,a.Hero.inventory.Find(i=>i.id==item.id))));
            int gold=store.Data.gold;Assert.AreSame(hero,store.Data.Hero);Assert.IsFalse(hero.inventory.Any(i=>i.id==item.id));
            Assert.IsTrue(store.Transact("one","sell:"+item.id,a=>throw new Exception("Must not run twice")));Assert.AreEqual(gold,store.Data.gold);
            Assert.IsFalse(store.Transact("one","different",a=>true));Assert.AreEqual(gold,new GameStore(directory).Data.gold);
        }
        [Test]public void InvalidTransactionAndUnknownContentVersionPreserveTheAccount()
        {
            var store=new GameStore(directory);string before=JsonUtility.ToJson(store.Data);
            Assert.IsFalse(store.Transact("invalid","negative",a=>{a.gold=-1;return true;}));Assert.AreEqual(before,JsonUtility.ToJson(store.Data));
            var future=GameStore.NewAccount();future.Hero.inventory[0].contentVersion=99;
            string file=Path.Combine(directory,"hellscript-local-v1.json"),json=JsonUtility.ToJson(future);File.WriteAllText(file,json);
            Assert.Throws<NotSupportedException>(()=>new GameStore(directory));Assert.AreEqual(json,File.ReadAllText(file));
        }
        [Test]public void FailedDiskCommitDoesNotConsumeGoldOrReplaceLiveItems()
        {
            var store=new GameStore(directory);store.Data.gold=1000;var hero=store.Data.Hero;string before=JsonUtility.ToJson(store.Data);
            Directory.CreateDirectory(Path.Combine(directory,"hellscript-local-v1.json.tmp"));LogAssert.Expect(LogType.Error,new Regex("저장하지 못했습니다:"));
            Assert.IsFalse(store.Transact("fail","spend",a=>{a.gold-=500;return true;}));Assert.AreEqual(before,JsonUtility.ToJson(store.Data));Assert.AreSame(hero,store.Data.Hero);
        }
        [Test]public void FutureSaveAndCorruptSaveAreNeverReplacedByNewProgress()
        {
            Directory.CreateDirectory(directory);string path=Path.Combine(directory,"hellscript-local-v1.json");var a=GameStore.NewAccount();a.schema=99;string future=JsonUtility.ToJson(a);File.WriteAllText(path,future);
            Assert.Throws<NotSupportedException>(()=>new GameStore(directory));Assert.AreEqual(future,File.ReadAllText(path));
            File.WriteAllText(path,"invalid data");Assert.Throws<InvalidDataException>(()=>new GameStore(directory));Assert.AreEqual("invalid data",File.ReadAllText(path));
        }
        [Test]public void InFlightSetChargesAndDelayedEchoSurviveSaveAndFreeze()
        {
            var a=GameStore.NewAccount();var catalog=ScriptableObject.CreateInstance<GameCatalog>();catalog.Populate();
            try
            {
                var sim=new CombatSimulation(a,catalog,1,1);sim.State.itemEffects.crushCharge=3.4f;sim.State.itemEffects.chainCharge=5;sim.State.itemEffects.chainCharges=2;
                sim.State.effects.Add(new GroundEffect{id=999,kind=40,position=sim.State.position,end=sim.State.position+Vector2.up*10,delay=.3f,duration=.1f,snapshot=new DamageSnapshot{damage=10,crit=0,critDamage=1.5f,level=30,elements=new float[6],passives=new bool[6]}});
                var restored=new CombatSimulation(a,catalog,1,1,JsonUtility.FromJson<RunState>(JsonUtility.ToJson(sim.State)));
                restored.State.paused=true;string before=JsonUtility.ToJson(restored.State);restored.Tick(.05f);Assert.AreEqual(before,JsonUtility.ToJson(restored.State));
                restored.State.paused=false;restored.Tick(.05f);Assert.AreEqual(3.35f,restored.State.itemEffects.crushCharge,.0001f);Assert.AreEqual(2,restored.State.itemEffects.chainCharges);Assert.AreEqual(.25f,restored.State.effects.Single(e=>e.id==999).delay,.0001f);
            }
            finally{UnityEngine.Object.DestroyImmediate(catalog);}
        }
        CombatSimulation SetSimulation(GameCatalog catalog,HeroClass c,string setId,params int[] skills)
        {
            var a=GameStore.NewAccount();a.selectedHero=(int)c;a.Hero.level=30;a.Hero.build.passives=Array.Empty<int>();a.Hero.build.movement=MovementMode.Stand;
            a.Hero.build.rules=skills.Select(i=>new Rule(i,ConditionKind.Always,0,i==9)).ToList();a.Hero.build.activeSkills=skills.Distinct().ToList();uint rng=991;
            for(int slot=1;slot<=4;slot++){var item=ItemGenerator.Create(c,slot,3,1,ref rng,uniqueId:setId+slot);item.equipped=true;a.Hero.inventory.Add(item);}
            var sim=new CombatSimulation(a,catalog,1,1);sim.State.position=RiftMap.Rooms[0];sim.State.enemies.Clear();sim.Stats.crit=0;sim.Stats.regen=0;
            return sim;
        }
        static EnemyState TargetAt(CombatSimulation sim,int id,Vector2 offset)
        {
            var e=new EnemyState{id=id,position=sim.State.position+offset,health=100000,maxHealth=100000,cooldown=1000,speed=0,attack=0};sim.State.enemies.Add(e);return e;
        }
        static void UntilSkill(CombatSimulation sim,int skill)
        {int releases=sim.State.actionEvents.Count(e=>e.skill==skill&&e.kind=="ACTION_RELEASE");for(int n=0;n<100&&sim.State.actionEvents.Count(e=>e.skill==skill&&e.kind=="ACTION_RELEASE")==releases;n++)sim.Tick(.05f);Assert.Greater(sim.State.actionEvents.Count(e=>e.skill==skill&&e.kind=="ACTION_RELEASE"),releases);}
        [Test]public void LeapSetGrantsAndConsumesCrushChargeWithCombinedCooldownCap()
        {
            var catalog=ScriptableObject.CreateInstance<GameCatalog>();catalog.Populate();
            try
            {
                var sim=SetSimulation(catalog,HeroClass.Warrior,"SWB",1,2);TargetAt(sim,900,Vector2.up*6);sim.Stats.cdr=.35f;
                sim.Tick(.05f);Assert.AreEqual(0,sim.State.itemEffects.crushCharge);Assert.AreEqual(4.8f,sim.State.cooldowns[1],.001f);sim.State.decisionTime=100;UntilSkill(sim,1);Assert.AreEqual(4,sim.State.itemEffects.crushCharge);
                var snapshot=JsonUtility.FromJson<RunState>(JsonUtility.ToJson(sim.State));snapshot.itemEffects.crushCharge=0;
                // Clone the equipped hero into an independent account.
                var account=GameStore.NewAccount();account.heroes[0]=JsonUtility.FromJson<HeroSave>(JsonUtility.ToJson(sim.Hero));
                var comparison=new CombatSimulation(account,catalog,1,1,snapshot);comparison.Stats.crit=0;comparison.Stats.regen=0;
                sim.State.decisionTime=0;comparison.State.decisionTime=0;UntilSkill(sim,2);UntilSkill(comparison,2);Assert.AreEqual(0,sim.State.itemEffects.crushCharge);
                Assert.Greater(sim.State.dealt,comparison.State.dealt+20);
            }
            finally{UnityEngine.Object.DestroyImmediate(catalog);}
        }
        [Test]public void RetreatEchoUsesSavedDamageAndCannotConsumeAnotherShadowCharge()
        {
            var catalog=ScriptableObject.CreateInstance<GameCatalog>();catalog.Populate();
            try
            {
                var sim=SetSimulation(catalog,HeroClass.Ranger,"SAB",9,6);TargetAt(sim,900,Vector2.up*2);
                sim.State.shadowCharges=3;sim.State.shadowTime=10;sim.Tick(.05f);Assert.AreEqual(0,sim.State.itemEffects.pierceCharge);UntilSkill(sim,9);Assert.AreEqual(6,sim.State.itemEffects.pierceCharge);
                UntilSkill(sim,6);Assert.AreEqual(0,sim.State.itemEffects.pierceCharge);Assert.AreEqual(2,sim.State.shadowCharges);
                var echo=sim.State.projectiles.Single(f=>f.extra);sim.State.decisionTime=100;
                for(int n=0;n<20&&sim.State.projectiles.Any(p=>!p.extra);n++)sim.Tick(.05f);
                float damageBefore=sim.State.dealt;sim.Stats.damage=999999;
                for(int n=0;n<8;n++)sim.Tick(.05f);
                Assert.IsFalse(sim.State.projectiles.Any(f=>f.extra));Assert.AreEqual(2,sim.State.shadowCharges);
                Assert.Greater(sim.State.dealt,damageBefore);Assert.Less(sim.State.dealt-damageBefore,echo.snapshot.damage*3);
            }
            finally{UnityEngine.Object.DestroyImmediate(catalog);}
        }
        [TestCase(false,6)]
        [TestCase(true,7)]
        public void NovaSetExtendsTwoChainsAndSingleBossStillCannotChainToItself(bool passive,int expectedHits)
        {
            var catalog=ScriptableObject.CreateInstance<GameCatalog>();catalog.Populate();
            try
            {
                var sim=SetSimulation(catalog,HeroClass.Mage,"SMB",17,14);sim.Stats.passives[2]=passive;
                for(int n=0;n<7;n++)TargetAt(sim,900+n,new Vector2(Mathf.Cos(n*Mathf.PI*2/7),Mathf.Sin(n*Mathf.PI*2/7))*2);
                sim.Tick(.05f);Assert.AreEqual(0,sim.State.itemEffects.chainCharges);UntilSkill(sim,17);Assert.AreEqual(2,sim.State.itemEffects.chainCharges);float[] hp=sim.State.enemies.Select(e=>e.health).ToArray();UntilSkill(sim,14);
                Assert.AreEqual(1,sim.State.itemEffects.chainCharges);Assert.AreEqual(expectedHits,sim.State.enemies.Where((e,n)=>e.health<hp[n]).Count());
                sim.State.enemies.RemoveRange(1,6);sim.State.enemies[0].boss=true;sim.Stats.specials.Add("LM04");sim.State.cooldowns[14]=0;sim.State.actionCd=0;sim.State.decisionTime=0;sim.State.activeSkill=-1;
                float before=sim.State.dealt;UntilSkill(sim,14);Assert.AreEqual(0,sim.State.itemEffects.chainCharges);
                Assert.Less(sim.State.dealt-before,sim.Stats.damage*2);
            }
            finally{UnityEngine.Object.DestroyImmediate(catalog);}
        }
    }
}
