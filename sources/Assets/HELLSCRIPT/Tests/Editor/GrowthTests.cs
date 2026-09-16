using System;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using static Hellscript.BehaviorRules;

namespace Hellscript.Tests
{
    public sealed class GrowthTests
    {
        GameCatalog catalog;
        [SetUp]public void Setup(){catalog=ScriptableObject.CreateInstance<GameCatalog>();catalog.Populate();}
        [TearDown]public void Teardown(){UnityEngine.Object.DestroyImmediate(catalog);}
        static void Flush(CombatSimulation sim)=>typeof(CombatSimulation).GetMethod("CommitExperience",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(sim,null);
        CombatSimulation Fixture(int heroClass=0,int level=1)
        {
            var account=ContentTestAccounts.Training();account.selectedHero=heroClass;account.Hero.level=level;
            account.Hero.build.rules.Clear();account.Hero.build.passives=Array.Empty<int>();account.Hero.build.potionThreshold=0;account.Hero.build.movement=MovementMode.Stand;
            var temporary=new CombatSimulation(account,catalog,1,0,ownedTraining:true);var run=temporary.State;run.training=-1;
            return new CombatSimulation(account,catalog,1,restore:run);
        }
        [Test]
        public void AllSixRecommendationsRespectEveryUnlockBoundary(
            [Values(0,1,2)]int heroClass,[Values(0,1)]int variant,[Values(1,3,6,10,15,20,30)]int level)
        {
            var hero=(HeroClass)heroClass;string original=JsonUtility.ToJson(GameCatalog.Preset(hero,variant));
            var recommended=BehaviorPresets.ForLevel(hero,variant,level,catalog);Assert.IsEmpty(Validate(recommended,hero));Assert.AreEqual(4,recommended.activeSkills.Count);Assert.AreEqual(ContentUnlocks.Rules.passiveLevels.Count(p=>level>=p),recommended.passives.Length);
            foreach(var rule in recommended.rules.Where(r=>r.action==RuleAction.Skill))Assert.AreEqual(catalog.skills[rule.skill].unlock<=level,rule.enabled);
            Assert.AreEqual(original,JsonUtility.ToJson(GameCatalog.Preset(hero,variant)));Assert.IsTrue(recommended.rules.Any(r=>r.action==RuleAction.Basic&&r.enabled));
        }
        [TestCase(1,false,false)][TestCase(2,false,true)][TestCase(1,true,true)]
        public void StarterMageFiresAtRealGroupsOrBossesWithoutAnUnlearnedBlizzard(int count,bool boss,bool expected)
        {
            var account=ContentTestAccounts.Training(catalog);account.selectedHero=2;var sim=new CombatSimulation(account,catalog,1,0,ownedTraining:true);
            sim.State.build.movement=MovementMode.Stand;var first=sim.State.enemies[0];first.position=sim.State.position+Vector2.up*2;first.boss=boss;
            if(count>1)sim.State.enemies.Add(new EnemyState{id=900,position=first.position+Vector2.right*.5f,health=100000,maxHealth=100000,speed=0,attack=0,cooldown=1000});
            for(int n=0;n<30;n++)sim.Tick(CombatSimulation.Step);
            Assert.AreEqual(expected,sim.State.actionEvents.Any(e=>e.kind=="ACTION_START"&&e.skill==12));
            Assert.IsFalse(sim.State.actionEvents.Any(e=>e.kind=="ACTION_START"&&e.skill==13));
            if(expected)Assert.IsTrue(sim.State.damageEvents.Any(d=>d.definitionId=="M01"&&d.hpLoss>0));
        }
        [Test]
        public void DefaultOwnedGroupTrainingSupportsStarterAreaAttack()
        {
            var account=ContentTestAccounts.Training(catalog);account.selectedHero=2;var sim=new CombatSimulation(account,catalog,1,1,ownedTraining:true);
            for(int n=0;n<600&&!sim.State.damageEvents.Any(d=>d.definitionId=="M01"&&d.hpLoss>0);n++)sim.Tick(CombatSimulation.Step);
            Assert.IsTrue(sim.State.damageEvents.Any(d=>d.definitionId=="M01"&&d.hpLoss>0),string.Join("; ",sim.State.decisions.Where(d=>d.skill==12).Select(d=>d.code+": "+d.detail+" ×"+d.count)));
        }
        [TestCase(0)][TestCase(1)][TestCase(2)]
        public void SeveralLevelsPreserveHealthRatioAndResourceWithOneGrowthRecord(int heroClass)
        {
            var sim=Fixture(heroClass);sim.Hero.xp=90;float old=sim.Stats.hp;sim.State.health=old*.4f;sim.State.resource=27;sim.State.pendingExperience=300;Flush(sim);
            Assert.AreEqual(3,sim.Hero.level);Assert.AreEqual(115,sim.Hero.xp);Assert.AreEqual(new HeroStats(sim.Hero).hp,sim.Stats.hp);
            Assert.AreEqual(sim.Stats.hp*.4f,sim.State.health,.0001f);Assert.AreEqual(27,sim.State.resource);Assert.AreEqual(0,sim.State.pendingExperience);
            var growth=sim.State.growthEvents.Single();Assert.AreEqual(old,growth.oldMaxHp);Assert.AreEqual(1,growth.fromLevel);Assert.AreEqual(3,growth.toLevel);CollectionAssert.AreEqual(new[]{heroClass*6+1},growth.unlockedSkills);
            Flush(sim);Assert.AreEqual(1,sim.State.growthEvents.Count);Assert.AreEqual(115,sim.Hero.xp);
        }
        [Test]
        public void SimultaneousEnemyDeathsAggregateExperienceBeforeStatRefresh()
        {
            var sim=Fixture();sim.Hero.xp=95;sim.State.health=sim.Stats.hp*.5f;var first=sim.State.enemies[0];first.health=0;first.dead=first.pendingDeath=true;
            sim.State.enemies.Add(new EnemyState{id=900,position=first.position,health=0,maxHealth=1,dead=true,pendingDeath=true,speed=0,attack=0,cooldown=1000});
            sim.Tick(CombatSimulation.Step);Assert.AreEqual(2,sim.Hero.level);Assert.AreEqual(15,sim.Hero.xp);Assert.AreEqual(20,sim.State.growthEvents.Single().experience);Assert.AreEqual(sim.Stats.hp*.5f,sim.State.health,.001f);
        }
        [TestCase(false)][TestCase(true)]
        public void BossExperienceRefreshesStatsAndCannotResurrectADeadHero(bool deadHero)
        {
            var sim=Fixture(2);sim.Hero.xp=90;sim.State.health=deadHero?0:sim.Stats.hp*.5f;sim.State.pendingExperience=10;
            var boss=sim.State.enemies[0];boss.boss=true;boss.health=0;boss.dead=boss.pendingDeath=true;sim.State.bossId=boss.id;sim.State.phase=RunPhase.Boss;
            sim.Tick(CombatSimulation.Step);Assert.IsTrue(sim.State.bossRewarded);Assert.AreEqual(3,sim.Hero.level);Assert.AreEqual(125,sim.Hero.xp);
            Assert.AreEqual(new HeroStats(sim.Hero).hp,sim.Stats.hp);Assert.AreEqual(deadHero?0:sim.Stats.hp*.5f,sim.State.health,.001f);Assert.AreEqual(310,sim.State.growthEvents.Single().experience);
            sim.Tick(CombatSimulation.Step);Assert.AreEqual(1,sim.State.growthEvents.Count);Assert.AreEqual(125,sim.Hero.xp);
        }
        [Test]
        public void GrowthLeavesReleasedEffectsAndInProgressActionsUntouched()
        {
            var sim=Fixture(2,2);sim.State.health=sim.Stats.hp*.4f;sim.State.resource=38;sim.State.cooldowns[13]=5;sim.State.potionCd=17;sim.State.channelTime=3;
            sim.State.heroAction=new HeroActionState{id=701,skill=12,phase=HeroActionPhase.Preparing,elapsed=.1f,prepare=.2f,cost=25};
            var snapshot=new DamageSnapshot{level=2,damage=31};sim.State.projectiles.Add(new CombatProjectile{id=800,actionId=700,definitionId="M01",snapshot=snapshot,remaining=8,speed=10,position=sim.State.position,direction=Vector2.up});
            sim.State.shields.Add(new ShieldEffect{id=801,definitionId="M05",amount=40,remaining=3,createdMaxHp=sim.Stats.hp});
            string action=JsonUtility.ToJson(sim.State.heroAction),projectile=JsonUtility.ToJson(sim.State.projectiles[0]),shield=JsonUtility.ToJson(sim.State.shields[0]);
            sim.State.pendingExperience=175;Flush(sim);Assert.AreEqual(3,sim.Hero.level);
            Assert.AreEqual(action,JsonUtility.ToJson(sim.State.heroAction));Assert.AreEqual(projectile,JsonUtility.ToJson(sim.State.projectiles[0]));Assert.AreEqual(shield,JsonUtility.ToJson(sim.State.shields[0]));
            Assert.AreEqual(38,sim.State.resource);Assert.AreEqual(5,sim.State.cooldowns[13]);Assert.AreEqual(17,sim.State.potionCd);Assert.AreEqual(3,sim.State.channelTime);
        }
        [Test]
        public void PendingExperienceSurvivesSerializationAndLootingResumeWithoutDoublePayment()
        {
            var sim=Fixture();sim.Hero.xp=90;sim.State.health=sim.Stats.hp*.3f;sim.State.pendingExperience=100;sim.State.phase=RunPhase.Looting;
            var account=ContentTestAccounts.Training();account.heroes[0]=JsonUtility.FromJson<HeroSave>(JsonUtility.ToJson(sim.Hero));var state=JsonUtility.FromJson<RunState>(JsonUtility.ToJson(sim.State));GameStore.NormalizeRun(state);
            var restored=new CombatSimulation(account,catalog,1,restore:state);restored.Tick(CombatSimulation.Step);
            Assert.AreEqual(2,account.Hero.level);Assert.AreEqual(90,account.Hero.xp);Assert.AreEqual(0,restored.State.pendingExperience);Assert.AreEqual(restored.Stats.hp*.3f,restored.State.health,.0001f);
            restored.Tick(CombatSimulation.Step);Assert.AreEqual(90,account.Hero.xp);Assert.AreEqual(1,restored.State.growthEvents.Count);
        }
        [Test]
        public void MaximumLevelConsumesPendingXpWithoutRepeatedGrowthOrHealing()
        {
            var sim=Fixture(0,29);sim.State.health=sim.Stats.hp*.2f;sim.Hero.xp=Economy.XpRequired(29)-1;sim.State.pendingExperience=100000;Flush(sim);
            Assert.AreEqual(30,sim.Hero.level);Assert.AreEqual(0,sim.Hero.xp);float hp=sim.State.health;sim.State.pendingExperience=1000;Flush(sim);
            Assert.AreEqual(hp,sim.State.health);Assert.AreEqual(1,sim.State.growthEvents.Count);Assert.AreEqual(0,sim.State.pendingExperience);
        }
        [Test]
        public void UnlockingDoesNotSilentlyEnableOrReplaceSavedUserRules()
        {
            var account=ContentTestAccounts.Training(catalog);account.selectedHero=2;var temp=new CombatSimulation(account,catalog,1,0,ownedTraining:true);temp.State.training=-1;
            var sim=new CombatSimulation(account,catalog,1,restore:temp.State);string original=JsonUtility.ToJson(sim.State.build);
            sim.State.pendingExperience=275;Flush(sim);Assert.AreEqual(3,sim.Hero.level);Assert.AreEqual(original,JsonUtility.ToJson(sim.State.build));Assert.AreEqual(original,JsonUtility.ToJson(account.Hero.build));
            Assert.IsFalse(sim.State.build.rules.Single(r=>r.skill==13).enabled);Assert.IsTrue(sim.State.growthEvents.Single().unlockedSkills.Contains(13));
            var next=BehaviorPresets.ForLevel(HeroClass.Mage,0,3,catalog);Assert.IsTrue(next.rules.Single(r=>r.skill==13).enabled);Assert.AreEqual(RuleTarget.OwnBlizzard,next.rules.Single(r=>r.skill==12).target);
        }
        [Test]
        public void CatalogAppliesStarterRecommendationsOnlyWhenCreatingAnAccount()
        {
            string directory=Path.Combine(Path.GetTempPath(),"hellscript-growth-"+Guid.NewGuid().ToString("N"));
            try
            {
                var store=new GameStore(directory,catalog);
                foreach(var hero in store.Data.heroes)
                {
                    // A new hero fights through the unified hunt edict: the recommended legacy build seeds the
                    // document's slots, and the stored build is that document's own projection.
                    var recommended=BehaviorPresets.ForLevel(hero.heroClass,0,1,catalog);
                    Assert.IsTrue(hero.useEdict);Assert.AreEqual(recommended.name,hero.build.name);
                    CollectionAssert.AreEqual(recommended.activeSkills.Where(i=>catalog.skills[i].unlock<=1),hero.build.activeSkills);
                    CollectionAssert.AreEqual(hero.build.activeSkills.Select(CombatTelemetry.SkillId),hero.edict.slots.Where(s=>s!=""));
                    var projected=HuntEdictLoadout.FromHero(hero).ToBuild(hero.build);
                    CollectionAssert.AreEqual(projected.rules.Select(r=>r.id),hero.build.rules.Select(r=>r.id));
                    Assert.AreEqual(JsonUtility.ToJson(projected.skillRanks),JsonUtility.ToJson(hero.build.skillRanks));
                }
                store.Data.selectedHero=2;var custom=store.Data.Hero.build;custom.name="직접 만든 조건";custom.version="owner-saved";custom.rules.Single(r=>r.skill==12).enabled=false;custom.distance=9;
                string expected=JsonUtility.ToJson(custom);Assert.IsTrue(store.Save());var restored=new GameStore(directory,catalog);
                Assert.AreEqual(expected,JsonUtility.ToJson(restored.Data.Hero.build));
            }
            finally{if(Directory.Exists(directory))Directory.Delete(directory,true);}
        }
        [Test]
        public void AbandonCommitsEarnedPendingExperienceEvenWhenPausedWithoutRevival()
        {
            var sim=Fixture();sim.State.paused=true;sim.State.health=0;sim.State.pendingExperience=100;sim.Abandon();
            Assert.AreEqual(2,sim.Hero.level);Assert.AreEqual(0,sim.State.health);Assert.AreEqual(0,sim.State.pendingExperience);Assert.AreEqual(RunPhase.Failed,sim.State.phase);
            sim.Abandon();Assert.AreEqual(1,sim.State.growthEvents.Count);
        }
    }
}
