using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using static Hellscript.BehaviorRules;

namespace Hellscript.Tests
{
    public sealed class PlayerTrainingTests
    {
        GameCatalog catalog;
        [SetUp]public void Setup(){catalog=ScriptableObject.CreateInstance<GameCatalog>();catalog.Populate();}
        [TearDown]public void Teardown(){UnityEngine.Object.DestroyImmediate(catalog);}
        CombatSimulation Start(AccountSave account,int training=0)=>new CombatSimulation(account,catalog,20,training,ownedTraining:true);
        static void Advance(CombatSimulation sim,int ticks){for(int i=0;i<ticks;i++)sim.Tick(CombatSimulation.Step);}
        [TestCase(0)][TestCase(1)][TestCase(2)]
        public void OwnedHeroAndEquipmentAreDeepCopiesWithActualStats(int heroClass)
        {
            var account=ContentTestAccounts.Training();account.selectedHero=heroClass;account.Hero.level=6;account.Hero.xp=15;
            uint rng=62713;var item=ItemGenerator.Create((HeroClass)heroClass,0,2,6,ref rng);account.Hero.inventory.Add(item);Assert.IsTrue(Economy.Equip(account.Hero,item));item.enhancement=3;
            string before=JsonUtility.ToJson(account);var expected=new HeroStats(account.Hero);var sim=Start(account);
            Assert.AreNotSame(account.Hero,sim.Hero);Assert.AreEqual(before,JsonUtility.ToJson(account));
            Assert.AreEqual(6,sim.EffectiveLevel);Assert.AreEqual(expected.hp,sim.Stats.hp);Assert.AreEqual(expected.damage,sim.Stats.damage);Assert.AreEqual(expected.armor,sim.Stats.armor);
            Assert.AreEqual(JsonUtility.ToJson(account.Hero),JsonUtility.ToJson(sim.Hero));
            sim.Hero.inventory[0].enhancement++;sim.Hero.inventory.Clear();sim.Hero.firstClears.Add(99);sim.Hero.xp+=5;
            Assert.AreEqual(before,JsonUtility.ToJson(account),"No shared item, inventory, or progress references may escape the copy.");
        }
        [Test]
        public void EveryUnlockBoundaryUsesTheSameLevelForRulesAndCooldownConditions(
            [Values(0,1,2)]int heroClass,[Values(1,3,6,10,15,20,30)]int level)
        {
            for(int n=0;n<6;n++)
            {
                int index=heroClass*6+n;var account=ContentTestAccounts.Training();account.selectedHero=heroClass;account.Hero.level=level;
                var rule=Make(index);account.Hero.build.activeSkills=new[]{index}.ToList();account.Hero.build.rules=new(){rule};
                var sim=Start(account);sim.State.enemies[0].position=sim.State.position+Vector2.up;sim.Tick(CombatSimulation.Step);
                bool locked=catalog.skills[index].unlock>level;var decision=sim.State.decisions.Single(d=>d.ruleId==sim.State.build.rules[0].id);
                Assert.AreEqual(locked,decision.code=="LEVEL_LOCK",$"class={heroClass} level={level} skill={index}: {decision.code}");
                if(locked)Assert.IsFalse(sim.State.actionEvents.Any(e=>e.kind=="ACTION_START"&&e.skill==index));
                sim.State.cooldowns[index]=0;
                Assert.AreEqual(!locked,sim.EvaluateCondition(C("SC15",Comparison.AtMost,0,skill:index),rule,sim.State.enemies[0],out _));
            }
        }
        [TestCase(0)][TestCase(1)][TestCase(2)]
        public void FixedFixturesIgnoreRiftStageAndAccountRandomness(int training)
        {
            var account=ContentTestAccounts.Training();var first=Start(account,training);account.gold+=1000;var second=Start(account,training);
            Assert.AreEqual(1,first.State.stage);Assert.AreEqual(first.State.rng,second.State.rng);Assert.AreEqual(first.State.position,second.State.position);
            CollectionAssert.AreEqual(first.State.enemies.Select(e=>JsonUtility.ToJson(e)),second.State.enemies.Select(e=>JsonUtility.ToJson(e)));
            Assert.AreEqual(training==0?1:training==1?8:3,first.State.enemies.Count);
            Assert.IsTrue(first.State.enemies.All(e=>e.maxHealth==100000&&e.speed==0));
            if(training<2)Assert.IsTrue(first.State.enemies.All(e=>e.attack==0&&e.cooldown>=1000));
            else Assert.IsTrue(first.State.enemies.All(e=>e.kind==9&&e.attack>0));
        }
        [Test]
        public void OwnedTrainingFinishesAt1200TicksWithoutRealRewardsOrRecord()
        {
            var account=ContentTestAccounts.Training();string before=JsonUtility.ToJson(account);var sim=Start(account,1);
            Advance(sim,1199);Assert.AreNotEqual(RunPhase.Cleared,sim.State.phase);Assert.AreNotEqual(RunPhase.Failed,sim.State.phase);
            sim.Tick(CombatSimulation.Step);Assert.AreEqual(RunPhase.Cleared,sim.State.phase);Assert.AreEqual("60초 훈련 완료",sim.State.action);
            Assert.AreEqual(1200,Mathf.RoundToInt(sim.State.time/CombatSimulation.Step));Assert.Greater(sim.State.dealt,0);
            Assert.AreEqual(before,JsonUtility.ToJson(account));float damage=sim.State.dealt;Advance(sim,10);Assert.AreEqual(damage,sim.State.dealt);
        }
        [TestCase(false)][TestCase(true)]
        public void DeathAndTargetEliminationEndWithoutAccountMutation(bool heroDies)
        {
            var account=ContentTestAccounts.Training();string before=JsonUtility.ToJson(account);var sim=Start(account);
            if(heroDies)sim.State.health=0;else {var target=sim.State.enemies[0];target.health=0;target.dead=true;target.pendingDeath=true;}
            sim.Tick(CombatSimulation.Step);Assert.AreEqual(heroDies?RunPhase.Failed:RunPhase.Cleared,sim.State.phase);Assert.AreEqual(before,JsonUtility.ToJson(account));
        }
        [Test]
        public void EditingOnlyChangesTheCopyAndPreservesHealthResourceAndCooldowns()
        {
            var account=ContentTestAccounts.Training();string before=JsonUtility.ToJson(account);var sim=Start(account);
            sim.State.health=77;sim.State.resource=31;sim.State.cooldowns[1]=4;var edit=sim.State.build.Copy();edit.distance=5.5f;edit.name="훈련 시험";
            Assert.IsTrue(sim.ApplyBuild(edit));Assert.AreEqual(5.5f,sim.State.build.distance);Assert.AreEqual(77,sim.State.health);Assert.AreEqual(31,sim.State.resource);Assert.AreEqual(4,sim.State.cooldowns[1]);
            Assert.AreEqual(before,JsonUtility.ToJson(account));Assert.AreEqual(new HeroStats(account.Hero).hp,sim.Stats.hp);sim.Abandon();Assert.AreEqual(before,JsonUtility.ToJson(account));
        }
        [Test]
        public void DangerousFixtureUsesRealEnemyPreparationAndDamage()
        {
            var account=ContentTestAccounts.Training();account.Hero.build.movement=MovementMode.Stand;account.Hero.build.rules.Clear();account.Hero.build.potionThreshold=0;
            string before=JsonUtility.ToJson(account);var sim=Start(account,2);Advance(sim,200);
            Assert.IsTrue(sim.State.enemyEvents.Any(e=>e.kind=="PREPARE"));Assert.IsTrue(sim.State.enemyEvents.Any(e=>e.kind=="HAZARD_CREATED"));
            Assert.Less(sim.State.health,sim.Stats.hp);Assert.AreEqual(before,JsonUtility.ToJson(account));
        }
        [Test]
        public void TrainingClockConditionAndExplicitDevelopmentModeStayDistinct()
        {
            var account=ContentTestAccounts.Training();var sim=Start(account);sim.State.time=55;
            Assert.IsTrue(sim.EvaluateCondition(C("SC21",Comparison.AtMost,5),Make(0),null,out _));
            var development=new CombatSimulation(account,catalog,1,0,seed:42);Assert.IsFalse(development.OwnedTraining);Assert.AreEqual(30,development.EffectiveLevel);Assert.AreEqual(300,development.TimeLimit);
            Assert.AreEqual(1,account.Hero.level);Assert.AreEqual(new HeroStats(account.Hero,true).hp,development.Stats.hp);
        }
        [Test]
        public void SerializedOwnedModePreservesLevelAndDamageSnapshots()
        {
            var account=ContentTestAccounts.Training();account.selectedHero=2;account.Hero.level=3;var sim=Start(account);Advance(sim,80);
            Assert.IsTrue(sim.State.projectiles.Any()||sim.State.damageEvents.Any());
            var run=JsonUtility.FromJson<RunState>(JsonUtility.ToJson(sim.State));GameStore.NormalizeRun(run);
            var restored=new CombatSimulation(account,catalog,1,restore:run);Assert.IsTrue(restored.OwnedTraining);Assert.AreEqual(3,restored.EffectiveLevel);Assert.AreEqual(sim.Stats.hp,restored.Stats.hp);
            Advance(sim,100);Advance(restored,100);Assert.AreEqual(sim.State.dealt,restored.State.dealt);Assert.AreEqual(sim.State.rng,restored.State.rng);
        }
        [TestCase(1)][TestCase(2)]
        public void SavingALaterPresetKeepsEarlierSlotsEmptyAcrossDiskReload(int slot)
        {
            string directory=Path.Combine(Path.GetTempPath(),"hellscript-training-"+Guid.NewGuid().ToString("N"));
            try
            {
                var store=new GameStore(directory);var expected=store.Data.Hero.build.Copy();expected.distance=5.5f;
                while(store.Data.Hero.presets.Count<=slot)store.Data.Hero.presets.Add(null);store.Data.Hero.presets[slot]=expected;
                Assert.IsTrue(store.Save());var restored=new GameStore(directory);
                for(int n=0;n<slot;n++){Assert.IsTrue(restored.Data.Hero.presets[n].emptySlot);Assert.IsFalse(BuildEditing.HasPreset(restored.Data.Hero.presets[n]),"Empty slot "+n);}
                Assert.AreEqual(JsonUtility.ToJson(expected),JsonUtility.ToJson(restored.Data.Hero.presets[slot]));
                Assert.IsTrue(restored.Save());var again=new GameStore(directory);for(int n=0;n<slot;n++)Assert.IsFalse(BuildEditing.HasPreset(again.Data.Hero.presets[n]));
            }
            finally{if(Directory.Exists(directory))Directory.Delete(directory,true);}
        }
        [TestCase(false)][TestCase(true)]
        public void PresetNormalizationPreservesIntentionalModernEmptyAndLegacyRules(bool legacy)
        {
            string directory=Path.Combine(Path.GetTempPath(),"hellscript-training-"+Guid.NewGuid().ToString("N"));
            try
            {
                var store=new GameStore(directory);var build=new BuildConfig{ruleSchema=legacy?0:BehaviorRules.Version,name="명시한 설정"};
                if(legacy)build.rules.Add(new Rule(0));store.Data.Hero.presets[0]=build;Assert.IsTrue(store.Save());
                var loaded=new GameStore(directory).Data.Hero.presets[0];Assert.IsNotNull(loaded);Assert.AreEqual("명시한 설정",loaded.name);
                Assert.AreEqual(BehaviorRules.Version,loaded.ruleSchema);if(legacy)Assert.IsTrue(loaded.rules.Any(r=>r.skill==0));else Assert.IsEmpty(loaded.rules);
            }
            finally{if(Directory.Exists(directory))Directory.Delete(directory,true);}
        }
        [TestCase(false)][TestCase(true)]
        public void AnEmptySavedSlotCannotReplaceTrainingOrRiftRules(bool rift)
        {
            var sim=Start(ContentTestAccounts.Training());if(rift)sim.State.training=-1;string before=JsonUtility.ToJson(sim.State.build);
            var empty=new BuildConfig{emptySlot=true};Assert.IsNotEmpty(BehaviorRules.Validate(empty,sim.Hero.heroClass));
            Assert.IsFalse(sim.ApplyBuild(empty));Assert.AreEqual(before,JsonUtility.ToJson(sim.State.build));
        }
    }
}
