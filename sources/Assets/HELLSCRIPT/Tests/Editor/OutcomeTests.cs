using System;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace Hellscript.Tests
{
    public sealed class OutcomeTests
    {
        GameCatalog catalog;
        [SetUp]public void Setup(){catalog=ScriptableObject.CreateInstance<GameCatalog>();catalog.Populate();}
        [TearDown]public void Teardown()=>UnityEngine.Object.DestroyImmediate(catalog);
        static object Call(CombatSimulation sim,string name,params object[] args)
        {
            var m=typeof(CombatSimulation).GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic);var p=m.GetParameters();
            return m.Invoke(sim,p.Select((x,i)=>i<args.Length?args[i]:x.DefaultValue).ToArray());
        }
        CombatSimulation Fixture(out AccountSave account,HeroClass hero=HeroClass.Ranger)
        {
            account=GameStore.NewAccount();account.selectedHero=(int)hero;account.Hero.level=30;account.Hero.build.passives=Array.Empty<int>();account.Hero.build.potionThreshold=0;
            var sim=new CombatSimulation(account,catalog,1,1,seed:917);sim.State.id="outcome-fixture";sim.State.enemies.Clear();sim.State.build.rules.Clear();sim.State.decisionTime=100;
            sim.State.position=RiftMap.Rooms[0];sim.State.destination=sim.State.position;sim.State.build.movement=MovementMode.Stand;
            sim.Stats.hp=sim.State.health=1000;sim.Stats.damage=100;sim.Stats.armor=sim.Stats.resistance=sim.Stats.regen=sim.Stats.crit=0;sim.Stats.healing=1;
            sim.State.enemies.Add(Enemy(sim,900,Vector2.up*2));return sim;
        }
        static EnemyState Enemy(CombatSimulation sim,int id,Vector2 offset)=>new EnemyState{id=id,position=sim.State.position+offset,health=10000,maxHealth=10000,cooldown=1000,speed=0};
        static DamageSnapshot Snapshot()=>new DamageSnapshot{damage=100,level=30,critDamage=1.5f,elements=new float[6],passives=new bool[6]};
        static void Shadow(CombatSimulation sim,EnemyState enemy,int root=71)
            =>Call(sim,"Hit",enemy,1f,0,true,0f,Snapshot(),false,true,"A01",root,80,DamageKind.Direct);
        static void Poison(CombatSimulation sim,EnemyState enemy)=>Call(sim,"ApplyStatus",enemy,StatusKind.Poisoned,"A03",3f,50,1f);
        static void Slow(CombatSimulation sim,EnemyState enemy,float duration,float strength,int root)
            =>Call(sim,"ApplyStatus",enemy,StatusKind.Slow,"M02",duration,root,strength);
        [Test]
        public void DistinctPoisonedOriginsCanSpreadToTheSameVictimOncePerOriginalAttack()
        {
            var sim=Fixture(out _);sim.Stats.specials.Add("LA04");var a=sim.State.enemies[0];var b=Enemy(sim,901,new Vector2(2,2));var victim=Enemy(sim,902,new Vector2(1,2));sim.State.enemies.AddRange(new[]{b,victim});
            Poison(sim,a);Poison(sim,b);Shadow(sim,a);Shadow(sim,b);Shadow(sim,a);Shadow(sim,b);
            var hits=sim.State.damageEvents.Where(d=>d.definitionId=="LA04"&&d.targetId==902).ToArray();
            Assert.AreEqual(2,hits.Length);CollectionAssert.AreEqual(new[]{900,901},hits.Select(d=>d.triggerTargetId));Assert.AreEqual(2,sim.State.procHits.Count);
            string serialized=JsonUtility.ToJson(sim.State);GameStore.NormalizeRun(sim.State);Assert.AreEqual(serialized,JsonUtility.ToJson(sim.State));Shadow(sim,b,72);Assert.AreEqual(3,sim.State.damageEvents.Count(d=>d.definitionId=="LA04"&&d.targetId==902));
        }
        [Test]
        public void EmptySpreadDoesNotGainNewVictimsWhenTheSameOriginIsHitAgain()
        {
            var sim=Fixture(out _);sim.Stats.specials.Add("LA04");var a=sim.State.enemies[0];Poison(sim,a);Shadow(sim,a);
            sim.State.enemies.Add(Enemy(sim,901,new Vector2(1,2)));Shadow(sim,a);Assert.IsFalse(sim.State.damageEvents.Any(d=>d.definitionId=="LA04"));
            Shadow(sim,a,72);Assert.AreEqual(1,sim.State.damageEvents.Count(d=>d.definitionId=="LA04"));
        }
        [Test]
        public void LegacySpreadReceiptBlocksOnlyItsOldRootAndSurvivesRepeatedRestore()
        {
            var sim=Fixture(out _);sim.Stats.specials.Add("LA04");var a=sim.State.enemies[0];Poison(sim,a);sim.State.enemies.Add(Enemy(sim,901,new Vector2(1,2)));
            sim.State.effectVersion=2;sim.State.procHits.Add(new ProcReceipt{rootCastId=71,targetId=901,definitionId="LA04"});
            GameStore.NormalizeRun(sim.State);GameStore.NormalizeRun(sim.State);Shadow(sim,a);Assert.IsFalse(sim.State.damageEvents.Any(d=>d.definitionId=="LA04"));Shadow(sim,a,72);Assert.AreEqual(1,sim.State.damageEvents.Count(d=>d.definitionId=="LA04"));
        }
        [TestCase(false)][TestCase(true)]
        public void BossSlowIntegratesTheStrongestRemainingRatioAcrossExpiryBoundaries(bool reverse)
        {
            var sim=Fixture(out _);var boss=sim.State.enemies[0];boss.boss=true;Slow(sim,boss,.02f,.8f,1);Slow(sim,boss,.05f,.2f,2);
            if(reverse)boss.statuses.Reverse();Call(sim,"TickStatuses",.05f);Assert.AreEqual(.22f,boss.bossControl.meter,.00001f);Assert.IsEmpty(boss.statuses);
        }
        [Test]
        public void SlowAfterPartialImmunityDoesNotCreditTheImmunePartOrExpiredStrongerSlow()
        {
            var sim=Fixture(out _);var boss=sim.State.enemies[0];boss.boss=true;boss.bossControl.immunity=.025f;
            Slow(sim,boss,.02f,.8f,1);Slow(sim,boss,.1f,.5f,2);Call(sim,"TickStatuses",.05f);Assert.AreEqual(.125f,boss.bossControl.meter,.00001f);
        }
        [Test]
        public void SlowIntegrationMatchesSplitStepsAndKeepsTheEightyPercentCap()
        {
            var sim=Fixture(out _);var boss=sim.State.enemies[0];boss.boss=true;Slow(sim,boss,.02f,4,1);Slow(sim,boss,.05f,.2f,2);
            Call(sim,"TickStatuses",.02f);Call(sim,"TickStatuses",.03f);Assert.AreEqual(.22f,boss.bossControl.meter,.00001f);
        }
        [TestCase(0)][TestCase(1)][TestCase(2)]
        public void OldBossScalarControlConvertsRemainingTimeOnceWithoutDirectImmobilization(int version)
        {
            var sim=Fixture(out _);var boss=sim.State.enemies[0];boss.boss=true;boss.stun=.6f;boss.root=1;boss.freeze=.4f;boss.slow=2;sim.State.effectVersion=version;
            GameStore.NormalizeRun(sim.State);Assert.AreEqual(20,boss.bossControl.meter,.0001f);Assert.IsFalse(CombatEffects.Has(boss,StatusKind.Stun));Assert.AreEqual(0,CombatEffects.Remaining(boss,StatusKind.Root));Assert.AreEqual(0,boss.slow);
            Assert.AreEqual(2,boss.statuses.Single().remaining);string converted=JsonUtility.ToJson(sim.State);GameStore.NormalizeRun(sim.State);Assert.AreEqual(converted,JsonUtility.ToJson(sim.State));
            Call(sim,"TickStatuses",.05f);Assert.AreEqual(20.175f,boss.bossControl.meter,.0001f);
        }
        [TestCase(false)][TestCase(true)]
        public void LegacyControlHonorsExistingImmunityOrCancelsTheWindupWhenItStaggers(bool immune)
        {
            var sim=Fixture(out _);var boss=sim.State.enemies[0];boss.boss=true;boss.stun=2;boss.bossControl.meter=90;boss.bossControl.immunity=immune?1:0;boss.windup=1;boss.cooldown=0;sim.State.effectVersion=2;
            GameStore.NormalizeRun(sim.State);Assert.AreEqual(immune?90:0,boss.bossControl.meter);Assert.AreEqual(immune?0:3,boss.bossControl.staggered);Assert.AreEqual(immune?1:0,boss.windup);Assert.AreEqual(immune?0:6,boss.cooldown);Assert.AreEqual(0,boss.stun);
        }
        [Test]
        public void LegacyConversionDoesNotChangeNonBossControlOrRecreditKnownCastStatuses()
        {
            var sim=Fixture(out _);var enemy=sim.State.enemies[0];enemy.stun=2;sim.State.effectVersion=2;GameStore.NormalizeRun(sim.State);Assert.AreEqual(2,enemy.stun);
            enemy.boss=true;enemy.stun=0;enemy.statuses.Add(new StatusEffect{kind=StatusKind.Root,definitionId="A03",rootCastId=77,remaining=1});enemy.bossControl.credited.Add("77:A03");sim.State.effectVersion=2;GameStore.NormalizeRun(sim.State);Assert.AreEqual(0,enemy.bossControl.meter);Assert.IsEmpty(enemy.statuses);
        }
        static void AddOpposingProjectiles(CombatSimulation sim,bool reverse)
        {
            var boss=sim.State.enemies[0];boss.boss=true;boss.health=1;sim.State.bossId=boss.id;sim.State.phase=RunPhase.Boss;sim.State.training=-1;sim.State.health=10;
            var shot=new CombatProjectile{id=3000,actionId=3001,skill=6,createdAt=-1,position=boss.position,direction=Vector2.up,speed=1,remaining=1,radius=.2f,coefficient=1,snapshot=Snapshot()};
            var incoming=new CombatProjectile{id=3002,actionId=3003,createdAt=-1,position=sim.State.position,direction=Vector2.up,speed=1,remaining=1,radius=.2f,hostile=true,damage=1000,casterId=boss.id.ToString(),definitionId="BOSS_TEST"};
            sim.State.projectiles.AddRange(reverse?new[]{incoming,shot}:new[]{shot,incoming});
        }
        [TestCase(false)][TestCase(true)]
        public void BothAlreadyFlyingAttacksResolveBeforeBossDeathWinsTheSameTick(bool reverse)
        {
            var sim=Fixture(out var account);int gold=account.gold,mats=account.materials;AddOpposingProjectiles(sim,reverse);sim.Tick(.05f);
            Assert.AreEqual(RunPhase.Looting,sim.State.phase);Assert.AreEqual(0,sim.State.health);Assert.AreEqual(2,sim.State.damageEvents.Count);Assert.AreEqual(2,sim.State.effectEvents.Count(e=>e.kind=="DEATH"));Assert.IsTrue(sim.State.heroDeathRecorded);
            CollectionAssert.AreEqual(new[]{"BOSS","HERO"},sim.State.effectEvents.Where(e=>e.kind=="DEATH").Select(e=>e.definitionId));Assert.AreEqual(gold+1950,account.gold);Assert.AreEqual(mats+15,account.materials);Assert.AreEqual(3,sim.State.drops.Count);
            sim.Tick(.05f);Assert.AreEqual(RunPhase.Cleared,sim.State.phase);Assert.AreEqual(2,sim.State.damageEvents.Count);Assert.AreEqual(gold+1950,account.gold);Assert.AreEqual(1,account.records.Count);
        }
        [Test]
        public void FatalGroundAfterTheBossProjectileStillRecordsDamageAndPreventsKillHealRevival()
        {
            var sim=Fixture(out _,HeroClass.Warrior);sim.Stats.passives[3]=true;AddOpposingProjectiles(sim,false);sim.State.projectiles.RemoveAll(p=>p.hostile);
            sim.State.effects.Add(new GroundEffect{id=4000,createdAt=-1,position=sim.State.position,radius=3,duration=.1f,damage=1000,hostile=true,definitionId="FATAL_GROUND"});
            sim.Tick(.05f);Assert.AreEqual(RunPhase.Looting,sim.State.phase);Assert.AreEqual(0,sim.State.health);Assert.AreEqual(-1000,sim.State.itemEffects.lastHeal);Assert.AreEqual("FATAL_GROUND",sim.State.damageEvents.Last().definitionId);
        }
        [TestCase(false)][TestCase(true)]
        public void BossKillWinsTheTimerBoundaryButHeroDeathAloneFails(bool killBoss)
        {
            var sim=Fixture(out var account);AddOpposingProjectiles(sim,false);if(!killBoss)sim.State.projectiles.RemoveAll(p=>!p.hostile);sim.State.time=299.99f;int gold=account.gold;sim.Tick(.05f);
            Assert.AreEqual(killBoss?RunPhase.Looting:RunPhase.Failed,sim.State.phase);Assert.AreEqual(killBoss?gold+1950:gold,account.gold);int damage=sim.State.damageEvents.Count;sim.Tick(1);Assert.AreEqual(damage,sim.State.damageEvents.Count);
        }
        [Test]
        public void LaterAttacksFreezeAfterLootingAndSavingCannotRepeatDeathOrFirstClearRewards()
        {
            var sim=Fixture(out var account);AddOpposingProjectiles(sim,false);sim.State.effects.Add(new GroundEffect{id=4000,createdAt=-1,position=sim.State.position,radius=3,delay=.2f,duration=2,damage=1000,hostile=true});sim.Tick(.05f);
            var save=JsonUtility.FromJson<AccountSave>(JsonUtility.ToJson(account));var run=JsonUtility.FromJson<RunState>(JsonUtility.ToJson(sim.State));var restored=new CombatSimulation(save,catalog,1,restore:run);int gold=save.gold;restored.Tick(.5f);restored.Tick(.5f);
            Assert.AreEqual(gold,save.gold);Assert.AreEqual(2,run.damageEvents.Count);Assert.AreEqual(2,run.effectEvents.Count(e=>e.kind=="DEATH"));Assert.AreEqual(1,save.Hero.firstClears.Count);Assert.AreEqual(1,save.records.Count);Assert.AreEqual(.15f,run.effects.Single().delay,.00001f);
        }
        [Test]
        public void PendingDeathRestoresWithoutRepeatingSettledOlderDeaths()
        {
            var sim=Fixture(out var account);var enemy=sim.State.enemies[0];enemy.boss=true;enemy.health=0;enemy.dead=enemy.pendingDeath=true;sim.State.training=-1;sim.State.phase=RunPhase.Boss;sim.State.health=0;
            sim.State.enemies.Add(new EnemyState{id=901,dead=true});var restored=new CombatSimulation(account,catalog,1,restore:JsonUtility.FromJson<RunState>(JsonUtility.ToJson(sim.State)));restored.Tick(.05f);
            Assert.AreEqual(RunPhase.Looting,restored.State.phase);Assert.AreEqual(2,restored.State.effectEvents.Count(e=>e.kind=="DEATH"));Assert.IsFalse(restored.State.enemies[0].pendingDeath);Assert.AreEqual(0,restored.State.time);
        }
        [Test]
        public void ReversingTheProjectileListKeepsDeathOrderAndRewardRollsIdentical()
        {
            string previous=null;
            foreach(bool reverse in new[]{false,true})
            {
                var sim=Fixture(out var account);AddOpposingProjectiles(sim,reverse);
                for(int i=0;i<2;i++)
                {
                    var e=Enemy(sim,950+i,new Vector2(4,i*3));e.health=1;sim.State.enemies.Add(e);
                    sim.State.projectiles.Add(new CombatProjectile{id=4000+i,actionId=4100+i,skill=6,createdAt=-1,position=e.position,direction=Vector2.right,speed=1,remaining=1,radius=.2f,coefficient=1,snapshot=Snapshot()});
                }
                if(reverse)sim.State.projectiles.Reverse();sim.Tick(.05f);
                string result=account.gold+":"+account.materials+":"+sim.State.rewardRng+":"+string.Join("|",sim.State.drops.Select(d=>JsonUtility.ToJson(d)))+":"+string.Join("|",sim.State.effectEvents.Where(e=>e.kind=="DEATH").Select(e=>JsonUtility.ToJson(e)));
                if(previous!=null)Assert.AreEqual(previous,result);previous=result;
            }
        }
        [Test]
        public void TrainingBossDeathsDoNotGrantRealClearRewards()
        {
            var sim=Fixture(out var account);var boss=sim.State.enemies[0];boss.boss=true;boss.health=0;boss.dead=boss.pendingDeath=true;int gold=account.gold,materials=account.materials;
            sim.Tick(.05f);Assert.AreEqual(RunPhase.Cleared,sim.State.phase);Assert.AreEqual(gold,account.gold);Assert.AreEqual(materials,account.materials);Assert.IsFalse(sim.State.bossRewarded);Assert.IsEmpty(account.Hero.firstClears);Assert.IsEmpty(sim.State.drops);
        }
        [Test]
        public void LocalIdleSettlementReportsItsOwnDeltaWithoutChangingClearOwnership()
        {
            string dir=Path.Combine(Path.GetTempPath(),"hellscript-idle-audit-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(dir);
            try
            {
                var account=GameStore.NewAccount();account.Hero.highestClear=1;account.Hero.firstClears.Add(1);account.gold=1950;account.materials=15;account.lastSeenUtc=DateTimeOffset.UtcNow.ToUnixTimeSeconds()-3600;ContentUnlocks.Reconcile(account);account.contentUnlocks.offlineActivatedUtc=account.lastSeenUtc;
                File.WriteAllText(Path.Combine(dir,"hellscript-local-v1.json"),JsonUtility.ToJson(account));var loaded=new GameStore(dir);
                Assert.GreaterOrEqual(loaded.LocalIdleGoldAwarded,240);Assert.AreEqual(loaded.LocalIdleGoldAwarded,loaded.Data.gold-account.gold);Assert.AreEqual(loaded.LocalIdleMaterialsAwarded,loaded.Data.materials-account.materials);
                CollectionAssert.AreEqual(account.Hero.firstClears,loaded.Data.Hero.firstClears);int gold=loaded.Data.gold;var reloaded=new GameStore(dir);Assert.AreEqual(reloaded.LocalIdleGoldAwarded,reloaded.Data.gold-gold);
            }
            finally{Directory.Delete(dir,true);}
        }
    }
}
