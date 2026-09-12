using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Hellscript.Tests
{
    public sealed class EdictAimTests
    {
        GameCatalog catalog;
        [SetUp] public void Setup(){catalog=ScriptableObject.CreateInstance<GameCatalog>();catalog.Populate();}
        [TearDown] public void Teardown()=>UnityEngine.Object.DestroyImmediate(catalog);

        // One always-on rule for a single skill, so whether it fires depends only on the aiming stage.
        AccountSave Account(HeroClass hero,int skill,bool useEdict)
        {
            var a=ContentTestAccounts.Training(catalog);a.selectedHero=(int)hero;a.Hero.level=30;
            a.Hero.build=BehaviorPresets.ForLevel(hero,0,30,catalog);
            a.Hero.build.rules=new List<Rule>{BehaviorRules.Make(skill)};a.Hero.build.activeSkills=new List<int>{skill};
            a.Hero.edict=HuntEdictV2.WithOption(HuntEdictV2Storage.CreateForHero(a.Hero),"BASIC",1,"OFF");a.Hero.useEdict=useEdict;return a;
        }
        CombatSimulation Fixture(AccountSave a)=>new CombatSimulation(a,catalog,1,0,ownedTraining:true);
        static void Advance(CombatSimulation sim,int ticks){for(int i=0;i<ticks;i++)sim.Tick(CombatSimulation.Step);}
        static int Starts(CombatSimulation sim,string id)=>sim.State.statistics.skills.SingleOrDefault(s=>s.definitionId==id)?.starts??0;

        [Test]
        public void TheLegacyRuleFiresPierceAtASingleDummy()
        {
            var sim=Fixture(Account(HeroClass.Ranger,6,false));Advance(sim,400);
            Assert.Greater(Starts(sim,"A01"),0);
            Assert.IsNull(sim.LastEdictAim);Assert.AreEqual(0,sim.EdictAimedCasts,"an opted-out hero never consults the aiming stage");
        }

        [Test]
        public void TheDefaultPiercePurposeHoldsFireAgainstOneOrdinaryEnemy()
        {
            // A01 option 2 ships as ELITE_OR_GROUP: an elite, a boss, or two enemies. The dummy is one ordinary enemy.
            var sim=Fixture(Account(HeroClass.Ranger,6,true));Assert.IsTrue(sim.EdictActive);Advance(sim,400);
            Assert.AreEqual(0,Starts(sim,"A01"),"the purpose is not met, so the ready rule is passed over");
            Assert.AreEqual(0,sim.EdictAimedCasts);
            Assert.IsNotNull(sim.LastEdictAim);Assert.IsTrue(sim.LastEdictAim.supported);Assert.IsFalse(sim.LastEdictAim.available);
            Assert.IsTrue(sim.State.decisions.Any(d=>d.code=="EDICT_AIM"),"the reason is recorded where other decisions are");
        }

        [Test]
        public void ChoosingOneEnemyAsThePurposeLetsPierceFireThroughThePlan()
        {
            var a=Account(HeroClass.Ranger,6,true);a.Hero.edict=HuntEdictV2.WithOption(a.Hero.edict,"A01",2,"ONE");
            var sim=Fixture(a);Advance(sim,400);
            Assert.Greater(Starts(sim,"A01"),0);
            Assert.Greater(sim.EdictAimedCasts,0,"casts went through the aiming stage");
            int dummy=sim.State.enemies.Single().id;
            Assert.IsTrue(sim.State.actionEvents.Where(e=>e.skill==6&&e.kind=="ACTION_START").All(e=>e.targetId==dummy),"the plan's target is the one the action carries");
        }

        [Test]
        public void NonAimedShieldLeavesAimTelemetryUntouched()
        {
            var account=Account(HeroClass.Mage,16,true);account.Hero.edict=HuntEdictV2.WithOption(account.Hero.edict,"BASIC",1,"OFF");account.Hero.edict=HuntEdictV2.WithOption(account.Hero.edict,"M05",2,"ENGAGE");
            var sim=Fixture(account);Advance(sim,400); // M05 has a core policy, but no aiming stage.
            Assert.Greater(Starts(sim,"M05"),0);
            Assert.AreEqual(0,sim.EdictAimedCasts);Assert.IsNull(sim.LastEdictAim);
        }

        [Test]
        public void ChainLightningFiresThroughItsPlanAtASingleEnemyByDefault()
        {
            var sim=Fixture(Account(HeroClass.Mage,14,true));Advance(sim,400);   // M03 option 2 ships as SINGLE
            Assert.Greater(Starts(sim,"M03"),0);Assert.Greater(sim.EdictAimedCasts,0);
        }

        [Test]
        public void RequiringLinkedEnemiesHoldsChainLightningAgainstOne()
        {
            var a=Account(HeroClass.Mage,14,true);a.Hero.edict=HuntEdictV2.WithOption(a.Hero.edict,"M03",2,"LINKED");
            var sim=Fixture(a);Advance(sim,400);
            Assert.AreEqual(0,Starts(sim,"M03"));Assert.IsTrue(sim.State.decisions.Any(d=>d.code=="EDICT_AIM"));
        }

        [Test]
        public void TwoAimedRunsStayIdentical()
        {
            string Trace(CombatSimulation s)=>$"{s.State.time:F3}|{s.State.kills}|{s.State.dealt:F3}|{s.State.rng}|{s.EdictAimedCasts}";
            AccountSave Make(){var a=Account(HeroClass.Ranger,6,true);a.Hero.edict=HuntEdictV2.WithOption(a.Hero.edict,"A01",2,"ONE");return a;}
            var x=Fixture(Make());var y=Fixture(Make());Advance(x,300);Advance(y,300);
            Assert.Greater(x.EdictAimedCasts,0);Assert.AreEqual(Trace(x),Trace(y));
        }
    }
}
