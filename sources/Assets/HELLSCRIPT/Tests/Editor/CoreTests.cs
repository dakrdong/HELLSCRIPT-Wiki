using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Hellscript.Tests
{
    public sealed class CoreTests
    {
        GameCatalog catalog;
        [SetUp]public void Setup(){catalog=ScriptableObject.CreateInstance<GameCatalog>();catalog.Populate();}
        [TearDown]public void Teardown(){UnityEngine.Object.DestroyImmediate(catalog);}
        [Test]public void CatalogHasEighteenDistinctSkillsAndFourEquippedPerPreset()
        {
            Assert.AreEqual(18,catalog.skills.Select(s=>s.id).Distinct().Count());
            for(int c=0;c<3;c++)for(int p=0;p<2;p++)Assert.AreEqual(4,GameCatalog.Preset((HeroClass)c,p).activeSkills.Count);
        }
        [Test]public void RingRoomsRemainWalkablyConnected()
        {for(int i=0;i<8;i++)Assert.IsTrue(RiftMap.LineClear(RiftMap.Rooms[i],RiftMap.Rooms[(i+1)%8]));Assert.IsFalse(RiftMap.Walkable(Vector2.zero));}
        [Test]public void MovementReachesDistantRoomWithoutOscillating()
        {var p=RiftMap.Rooms[0]+new Vector2(3,-3);for(int i=0;i<2000;i++)p=RiftMap.Move(p,RiftMap.Rooms[4],.2f);Assert.Less(Vector2.Distance(p,RiftMap.Rooms[4]),.3f);}
        [Test]public void GeneratedDungeonContainsEnoughPointsAndUniqueEnemyIds()
        {
            var sim=new CombatSimulation(GameStore.NewAccount(),catalog,1);
            Assert.GreaterOrEqual(sim.State.enemies.Sum(e=>e.elite>=0?5:1),150);
            Assert.AreEqual(sim.State.enemies.Count,sim.State.enemies.Select(e=>e.id).Distinct().Count());
            Assert.IsTrue(sim.State.enemies.All(e=>sim.Map.Walkable(e.position,.4f)));
        }
        [Test]public void PauseAndPortalFreezeAllSimulationTime()
        {
            var sim=new CombatSimulation(GameStore.NewAccount(),catalog,1);sim.State.paused=true;var before=JsonUtility.ToJson(sim.State);sim.Tick(.05f);Assert.AreEqual(before,JsonUtility.ToJson(sim.State));
            sim.State.paused=false;sim.State.portal=true;before=JsonUtility.ToJson(sim.State);sim.Tick(.05f);Assert.AreEqual(before,JsonUtility.ToJson(sim.State));
        }
        [Test]public void TrainingCannotChangeEconomyExperienceOrClearRecord()
        {
            var a=GameStore.NewAccount();string before=JsonUtility.ToJson(a);var sim=new CombatSimulation(a,catalog,1,1);
            for(int i=0;i<6000&&sim.State.phase!=RunPhase.Cleared&&sim.State.phase!=RunPhase.Failed;i++)sim.Tick(.05f);
            Assert.Greater(sim.State.kills,0);Assert.AreEqual(before,JsonUtility.ToJson(a));
        }
        [Test]public void FullBagSweepDoesNotSpendOrPayAndRequestCannotPayTwice()
        {
            var a=GameStore.NewAccount();a.Hero.highestClear=4;a.contentUnlocks.unlocked.Add(ContentUnlocks.Sweep);uint rng=123;a.Hero.capacity=2;
            Assert.IsFalse(Economy.Sweep(a,"one",ref rng));Assert.AreEqual(0,a.sweepCount);Assert.AreEqual(0,a.gold);
            a.Hero.capacity=50;Assert.IsTrue(Economy.Sweep(a,"one",ref rng));int gold=a.gold,count=a.Hero.inventory.Count;
            Assert.IsFalse(Economy.Sweep(a,"one",ref rng));Assert.AreEqual(gold,a.gold);Assert.AreEqual(count,a.Hero.inventory.Count);Assert.AreEqual(0,a.Hero.firstClears.Count);
        }
        [Test]public void ReplacementProtectsLocksEquippedAndLegendaries()
        {
            var a=GameStore.NewAccount();var h=a.Hero;h.capacity=1;uint rng=34;
            var locked=Economy.CreateItem(h.heroClass,2,1,1,ref rng);locked.locked=true;h.inventory.Add(locked);
            var newItem=Economy.CreateItem(h.heroClass,2,3,1,ref rng);
            Assert.IsFalse(Economy.AddItem(h,newItem,BagPolicy.Replace));Assert.IsTrue(h.inventory.Contains(locked));
            locked.locked=false;Assert.IsTrue(Economy.AddItem(h,newItem,BagPolicy.Replace));
            Assert.IsFalse(Economy.AddItem(h,Economy.CreateItem(h.heroClass,2,3,10,ref rng),BagPolicy.Replace));
        }
        [Test]public void LegendaryPoolAndAffixConstraintsHoldForEveryClassAndSlot()
        {
            uint rng=101;for(int c=0;c<3;c++)for(int slot=0;slot<8;slot++)for(int n=0;n<50;n++)
            {var i=Economy.CreateItem((HeroClass)c,slot,3,30,ref rng);Assert.IsNotEmpty(i.special);Assert.AreEqual(4,i.rolls.Select(r=>r.affixId).Distinct().Count());Assert.IsTrue(i.rolls.All(r=>ItemCatalog.Affix(r.affixId).Allows(slot)));}
        }
        [Test]public void EnhancementRefundCannotCreateMaterials()
        {
            var a=GameStore.NewAccount();a.gold=100000;a.materials=10000;uint rng=1;var item=Economy.CreateItem(HeroClass.Warrior,1,2,1,ref rng);a.Hero.inventory.Add(item);
            a.Hero.highestClear=1;
            for(int i=0;i<5;i++)Assert.IsTrue(Economy.Enhance(a,item));Assert.IsFalse(Economy.Enhance(a,item));
            Assert.IsTrue(Economy.Dismantle(a,a.Hero,item));Assert.AreEqual(10000-620+496+5,a.materials);
        }
        [Test]public void SaveRestoresRunIdentityHealthCooldownAndDropIds()
        {
            string directory=Path.Combine(Path.GetTempPath(),"HellscriptTest-"+Guid.NewGuid());
            try
            {var store=new GameStore(directory);var sim=new CombatSimulation(store.Data,catalog,2);sim.State.health=123;sim.State.cooldowns[1]=6;store.Data.suspendedRun=sim.State;Assert.IsTrue(store.Save());
             var loaded=new GameStore(directory);Assert.AreEqual(sim.State.id,loaded.Data.suspendedRun.id);Assert.AreEqual(123,loaded.Data.suspendedRun.health);Assert.AreEqual(6,loaded.Data.suspendedRun.cooldowns[1]);}
            finally{if(Directory.Exists(directory))Directory.Delete(directory,true);}
        }
        [Test]public void AllThreeClassesAdvanceInTraining()
        {
            for(int c=0;c<3;c++){var a=GameStore.NewAccount();a.selectedHero=c;var sim=new CombatSimulation(a,catalog,1,1);for(int t=0;t<6000&&sim.State.phase!=RunPhase.Failed&&sim.State.phase!=RunPhase.Cleared;t++)sim.Tick(.05f);Assert.Greater(sim.State.kills,0,"class "+c);Assert.IsFalse(float.IsNaN(sim.State.position.x));}
        }
        [Test]public void BossRewardPaysOnceAndFirstClearBelongsToEachHero()
        {
            var a=GameStore.NewAccount();
            for(int hero=0;hero<2;hero++)
            {
                a.selectedHero=hero;var sim=new CombatSimulation(a,catalog,1);sim.State.enemies.Clear();sim.State.phase=RunPhase.Boss;
                sim.State.enemies.Add(new EnemyState{id=900,kind=0,boss=true,health=.01f,maxHealth=1,position=sim.State.position+Vector2.up,cooldown=20});
                for(int i=0;i<50&&sim.State.phase!=RunPhase.Cleared;i++)sim.Tick(.05f);
                Assert.AreEqual(RunPhase.Cleared,sim.State.phase);Assert.AreEqual(1,a.Hero.highestClear);Assert.AreEqual(1,a.Hero.firstClears.Count);
                int gold=a.gold;sim.Tick(.05f);Assert.AreEqual(gold,a.gold);
            }
            Assert.AreEqual(3900,a.gold);Assert.AreEqual(0,a.heroes[2].firstClears.Count);
        }
    }
}
