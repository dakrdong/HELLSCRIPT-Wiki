using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Hellscript.Tests
{
    public sealed class EdictPolicyBridgeTests
    {
        GameCatalog catalog;
        [SetUp] public void Setup(){catalog=ScriptableObject.CreateInstance<GameCatalog>();catalog.Populate();}
        [TearDown] public void Teardown()=>UnityEngine.Object.DestroyImmediate(catalog);

        AccountSave Account(int hero=2,bool useEdict=false)
        {
            var a=ContentTestAccounts.Training(catalog);a.selectedHero=hero;a.Hero.level=30;
            a.Hero.build=BehaviorPresets.ForLevel((HeroClass)hero,0,30,catalog);
            a.Hero.edict=HuntEdictV2Storage.CreateForHero(a.Hero);a.Hero.useEdict=useEdict;return a;
        }
        static HuntEdictV2Document With(HuntEdictV2Document d,params (string id,string value)[] edits)
        {foreach(var e in edits)d=HuntEdictV2Editing.WithGlobal(d,e.id,e.value);return d;}
        CombatSimulation Fixture(AccountSave a)=>new CombatSimulation(a,catalog,1,0,ownedTraining:true);
        static void Advance(CombatSimulation sim,int ticks){for(int i=0;i<ticks;i++)sim.Tick(CombatSimulation.Step);}

        [Test]
        public void WithTheEdictOffThePolicyIsTheLiveBuildItself()
        {
            var sim=Fixture(Account());
            Assert.AreSame(sim.State.build,sim.Policy);
        }

        [Test]
        public void TheBridgeIsACopyAndLeavesTheSourceAndItsRulesUntouched()
        {
            var a=Account();string before=JsonUtility.ToJson(a.Hero.build);
            var doc=With(a.Hero.edict,("explore.chests","OFF"),("bag.policy","REPLACE"));
            var policy=EdictPolicyBridge.Apply(a.Hero.build,doc,a.Hero.heroClass);
            Assert.AreNotSame(a.Hero.build,policy);
            Assert.AreEqual(before,JsonUtility.ToJson(a.Hero.build),"the live build is never rewritten");
            Assert.IsFalse(policy.openChests);Assert.AreEqual(BagPolicy.Replace,policy.bagPolicy);
            CollectionAssert.AreEqual(a.Hero.build.activeSkills,policy.activeSkills);
            Assert.AreEqual(a.Hero.build.rules.Count,policy.rules.Count,"rules ride along unchanged");
            Assert.AreEqual(a.Hero.build.passives,policy.passives);
        }

        [Test]
        public void EveryMappedGlobalOptionLandsOnItsLegacyField()
        {
            var a=Account(0);
            var doc=With(a.Hero.edict,
                ("target.default","LOW_HP"),("position.mode","STAND"),("position.distanceSource","EXPLICIT"),("position.distance","5"),("position.clockwise","OFF"),
                ("survival.potion","ON"),("survival.potionHpPercent","25"),
                ("explore.mode","LEFT_WALL"),("explore.normalChest","IGNORE"),("explore.sealedChest","PRIORITY"),("explore.chestDetour","7"),("explore.chestsAfterBoss","ON"),
                ("explore.cursedChest","CONDITIONAL"),("explore.cursedMinimumTime","120"),("explore.shrine","OFF"),("explore.guideShrine","OFF"),("explore.shrineDetour","3"),
                ("repeat.enabled","ON"),("repeat.success","NEXT"),("repeat.failures","5"));
            var p=EdictPolicyBridge.Apply(a.Hero.build,doc,HeroClass.Warrior);
            Assert.AreEqual(TargetMode.LowHealth,p.target);Assert.AreEqual(MovementMode.Stand,p.movement);Assert.AreEqual(5f,p.distance);Assert.IsFalse(p.clockwise);
            Assert.AreEqual(25f,p.potionThreshold);
            Assert.AreEqual(ExplorationPolicy.LeftWall,p.exploration);Assert.IsFalse(p.commonChests);Assert.IsTrue(p.sealedChests);Assert.AreEqual(7f,p.chestDetour);Assert.IsTrue(p.chestsAfterBoss);
            Assert.IsTrue(p.allowCursedChests);Assert.AreEqual(120f,p.cursedMinimumTime);Assert.IsFalse(p.useShrines);Assert.IsFalse(p.guideShrines);Assert.IsTrue(p.resolveShrines);Assert.AreEqual(3f,p.shrineDetour);
            Assert.IsTrue(p.autoRepeat);Assert.IsTrue(p.advanceOnWin);Assert.AreEqual(5,p.stopAfterFailures);
        }

        [Test]
        public void SpecialTargetsOverrideTheDefaultInTheDeclaredOrder()
        {
            var a=Account(1);
            var elite=EdictPolicyBridge.Apply(a.Hero.build,With(a.Hero.edict,("target.default","DENSE"),("target.specialEnabled","ELITE"),("target.specialOrder","BOSS,ELITE,SUPPORT,RANGED")),HeroClass.Ranger);
            Assert.AreEqual(TargetMode.Elite,elite.target);
            var support=EdictPolicyBridge.Apply(a.Hero.build,With(a.Hero.edict,("target.default","NEAREST"),("target.specialEnabled","SUPPORT,RANGED"),("target.specialOrder","RANGED,SUPPORT,BOSS,ELITE")),HeroClass.Ranger);
            Assert.AreEqual(TargetMode.Support,support.target,"RANGED has no legacy mode and yields to the next enabled special");
            var none=EdictPolicyBridge.Apply(a.Hero.build,With(a.Hero.edict,("target.default","HIGH_HP"),("target.specialEnabled","")),HeroClass.Ranger);
            Assert.AreEqual(TargetMode.Nearest,none.target,"HIGH_HP has no legacy mode and reads as nearest");
        }

        [Test]
        public void BasicRangeDistanceFollowsTheClass()
        {
            var w=Account(0);var m=Account(2);
            Assert.AreEqual(2f,EdictPolicyBridge.Apply(w.Hero.build,With(w.Hero.edict,("position.distanceSource","BASIC_RANGE")),HeroClass.Warrior).distance);
            Assert.AreEqual(10f,EdictPolicyBridge.Apply(m.Hero.build,With(m.Hero.edict,("position.distanceSource","BASIC_RANGE")),HeroClass.Mage).distance);
        }

        [Test]
        public void LootModesBecomeTheTwoLegacyThresholds()
        {
            var a=Account();
            var p=EdictPolicyBridge.Apply(a.Hero.build,With(a.Hero.edict,("loot.NORMAL","IGNORE"),("loot.MAGIC","PASSING"),("loot.RARE","AFTER_COMBAT"),("loot.LEGENDARY","PRIORITY")),HeroClass.Mage);
            Assert.AreEqual(1,p.minimumRarity,"magic is the first rarity not ignored");
            Assert.AreEqual(2,p.pursueRarity,"rare is the first rarity actually chased");
            var none=EdictPolicyBridge.Apply(a.Hero.build,With(a.Hero.edict,("loot.NORMAL","IGNORE"),("loot.MAGIC","IGNORE"),("loot.RARE","IGNORE"),("loot.LEGENDARY","IGNORE")),HeroClass.Mage);
            Assert.AreEqual(4,none.minimumRarity);Assert.AreEqual(4,none.pursueRarity);
            var passing=EdictPolicyBridge.Apply(a.Hero.build,With(a.Hero.edict,("loot.NORMAL","PASSING"),("loot.MAGIC","PASSING"),("loot.RARE","PASSING"),("loot.LEGENDARY","PASSING")),HeroClass.Mage);
            Assert.AreEqual(0,passing.minimumRarity);Assert.AreEqual(4,passing.pursueRarity,"passing pickup never chases");
        }

        [Test]
        public void StopAfterSuccessReadsAsNoAutomaticRepeat()
        {
            var a=Account();
            var p=EdictPolicyBridge.Apply(a.Hero.build,With(a.Hero.edict,("repeat.enabled","ON"),("repeat.success","STOP")),HeroClass.Mage);
            Assert.IsFalse(p.autoRepeat);Assert.IsFalse(p.advanceOnWin);
        }

        [Test]
        public void TheSimulationReadsThePotionThresholdFromTheEdict()
        {
            // Legacy preset: potion at 40%. Edict: potion off. The same low hp must drink in one case only.
            var off=Fixture(Account());Advance(off,2);off.State.health=off.Stats.hp*.1f;off.Tick(CombatSimulation.Step);
            Assert.AreEqual(20f,off.State.potionCd,.0001f,"the legacy threshold drinks");
            var a=Account(useEdict:true);a.Hero.edict=With(a.Hero.edict,("survival.potion","OFF"));
            var on=Fixture(a);Assert.IsTrue(on.EdictActive);Assert.AreEqual(0f,on.Policy.potionThreshold);
            Advance(on,2);on.State.health=on.Stats.hp*.1f;on.Tick(CombatSimulation.Step);
            Assert.AreEqual(0f,on.State.potionCd,.0001f,"with the edict's potion off nothing drinks");
            Assert.AreEqual(40f,on.State.build.potionThreshold,"the live build still carries the legacy value");
        }

        [Test]
        public void TheOverlayIsRebuiltWhenTheDocumentIsRefreshed()
        {
            var a=Account(useEdict:true);var sim=new CombatSimulation(a,catalog,1);
            Assert.IsTrue(sim.EdictActive);Assert.IsTrue(sim.Policy.openChests);
            a.Hero.edict=With(a.Hero.edict,("explore.chests","OFF"));
            Assert.IsTrue(sim.Policy.openChests,"nothing changes until the run is told");
            sim.RefreshEdict();
            Assert.IsFalse(sim.Policy.openChests);Assert.IsTrue(sim.State.build.openChests);
        }

        [Test]
        public void TwoRunsWithTheSamePolicyStayIdentical()
        {
            string Trace(CombatSimulation s)=>$"{s.State.time:F3}|{s.State.health:F4}|{s.State.position.x:F4},{s.State.position.y:F4}|{s.State.kills}|{s.State.rng}";
            AccountSave Make(){var a=Account(useEdict:true);a.Hero.edict=With(a.Hero.edict,("position.mode","STAND"),("survival.potion","OFF"));return a;}
            var x=Fixture(Make());var y=Fixture(Make());Advance(x,300);Advance(y,300);
            Assert.AreEqual(MovementMode.Stand,x.Policy.movement);Assert.AreEqual(Trace(x),Trace(y));
        }
    }
}
