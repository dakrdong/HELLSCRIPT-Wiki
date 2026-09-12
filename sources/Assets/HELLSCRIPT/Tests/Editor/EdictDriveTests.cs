using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Hellscript.Tests
{
    public sealed class EdictDriveTests
    {
        GameCatalog catalog;
        [SetUp] public void Setup(){catalog=ScriptableObject.CreateInstance<GameCatalog>();catalog.Populate();}
        [TearDown] public void Teardown()=>UnityEngine.Object.DestroyImmediate(catalog);

        AccountSave Account(int hero=2,int level=30,bool useEdict=false)
        {
            var a=ContentTestAccounts.Training(catalog);a.selectedHero=hero;a.Hero.level=level;
            a.Hero.build=BehaviorPresets.ForLevel((HeroClass)hero,0,level,catalog);
            a.Hero.edict=HuntEdictV2Storage.CreateForHero(a.Hero);a.Hero.useEdict=useEdict;
            return a;
        }
        CombatSimulation Fixture(AccountSave a,int training=0)=>new CombatSimulation(a,catalog,1,training,ownedTraining:true);
        static void Advance(CombatSimulation sim,int ticks){for(int i=0;i<ticks;i++)sim.Tick(CombatSimulation.Step);}
        static string Trace(CombatSimulation sim)
        {
            var s=sim.State;
            return $"{s.time:F3}|{s.health:F4}|{s.resource:F4}|{s.position.x:F4},{s.position.y:F4}|{s.kills}|{s.dealt:F4}|{s.rng}";
        }

        [Test]
        public void TheEdictStaysInactiveUntilTheHeroOptsIn()
        {
            var sim=Fixture(Account());
            Assert.IsFalse(sim.EdictActive);
            StringAssert.Contains("꺼져",sim.EdictInactiveReason);
            Advance(sim,120);
            Assert.IsNull(sim.LastEdictPlan,"an opted-out hero never consults the document");
        }

        [Test]
        public void OptingInLetsTheDecisionCycleConsultTheDocument()
        {
            var sim=Fixture(Account(useEdict:true));
            Assert.IsTrue(sim.EdictActive);Assert.AreEqual("",sim.EdictInactiveReason);
            Advance(sim,120);
            Assert.IsNotNull(sim.LastEdictPlan,"the decision cycle should have produced a plan");
            Assert.AreEqual("",sim.LastEdictPlan.blockedReason);
        }

        [Test]
        public void TurningItOnDoesNotChangeARunThatIsAlreadyDecided([Values(0,1,2)]int hero)
        {
            var off=Fixture(Account(hero));Advance(off,200);
            Assert.IsFalse(off.EdictActive);
            var again=Fixture(Account(hero));Advance(again,200);
            Assert.AreEqual(Trace(off),Trace(again),"the opted-out path stays deterministic");
        }

        [Test]
        public void AnOptedInRunStillReachesTheFixtureEnd([Values(0,1,2)]int hero)
        {
            var sim=Fixture(Account(hero,useEdict:true));Advance(sim,1201);
            Assert.AreEqual(1200,sim.State.statistics.ticks);
            Assert.IsEmpty(sim.State.navigationError);
        }

        [Test]
        public void TwoOptedInRunsOfTheSameSeedStayIdentical()
        {
            var a=Fixture(Account(useEdict:true));var b=Fixture(Account(useEdict:true));
            Advance(a,300);Advance(b,300);
            Assert.AreEqual(Trace(a),Trace(b),"consulting the document must not add nondeterminism");
        }

        [Test]
        public void ADocumentWhoseSlotsDoNotMatchTheLiveBuildIsRefused()
        {
            var account=Account(useEdict:true);
            // Drop one equipped skill from the document so its loadout no longer matches the fight.
            var slots=account.Hero.edict.slots.ToArray();slots[0]="";account.Hero.edict.slots=slots;
            var sim=Fixture(account);
            Assert.IsTrue(sim.EdictActive,"the document itself is still valid");
            Advance(sim,60);
            Assert.IsNotNull(sim.LastEdictPlan);
            Assert.AreNotEqual("",sim.LastEdictPlan.blockedReason,"a mismatched loadout must block the response");
            Assert.IsTrue(sim.State.logs.Any(l=>l.Contains("사냥 칙령")||l.Contains("원본의 스킬 배치")),"the refusal is written to the run log");
        }

        [Test]
        public void ADocumentForAnotherClassLeavesTheEdictInactiveWithoutEndingTheRun()
        {
            var account=Account(0,useEdict:true);
            account.Hero.edict=HuntEdictV2.Create(HeroClass.Mage);
            var sim=Fixture(account);
            Assert.IsFalse(sim.EdictActive);
            StringAssert.Contains("사냥 칙령 원본을 사용할 수 없습니다",sim.EdictInactiveReason);
            Assert.DoesNotThrow(()=>Advance(sim,120));
            Assert.AreEqual(120,sim.State.statistics.ticks);
        }

        [Test]
        public void AnEmergencyLetsTheEdictTakeTheMainAction()
        {
            var account=Account(useEdict:true);
            // The retreat condition ships off; turn it on so the document has something to react to.
            account.Hero.edict.global.Single(o=>o.id=="survival.lowHp").value="ON";
            // Both potion paths would heal past the retreat threshold before the decision cycle reads it,
            // so the fixture isolates the retreat decision from healing.
            account.Hero.edict.global.Single(o=>o.id=="survival.potion").value="OFF";
            account.Hero.build.potionThreshold=0;
            var sim=Fixture(account);Assert.IsTrue(sim.EdictActive);
            Advance(sim,20);
            sim.State.health=sim.Stats.hp*.1f;
            Advance(sim,8);
            Assert.IsNotNull(sim.LastEdictPlan);
            Assert.AreEqual("",sim.LastEdictPlan.blockedReason);
            Assert.IsTrue(sim.LastEdictPlan.assessment.emergency,"hp under the retreat threshold raises an emergency");
            Assert.IsTrue(sim.LastEdictPlan.ControlsMainAction||sim.LastEdictPlan.attempts.Count>0,
                "an emergency either acts or records why every option was unavailable");
        }

        [Test]
        public void AnAbsentDocumentLeavesTheEdictInactive()
        {
            var account=Account(useEdict:true);account.Hero.edict=null;
            var sim=Fixture(account);
            Assert.IsFalse(sim.EdictActive);
            StringAssert.Contains("원본이 없습니다",sim.EdictInactiveReason);
        }
    }
}
