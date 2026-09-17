using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Hellscript.Tests
{
    // A suspended rift is stored as JSON, so what it can and cannot reproduce exactly is part of
    // the save contract. Everything the simulation reads back must be identical; the accumulated
    // damage telemetry is the one place where the format's precision shows, and it is pinned here
    // so a real divergence cannot hide behind it.
    public sealed class RestoreFidelityTests
    {
        GameCatalog catalog;
        [SetUp]public void Setup(){catalog=ScriptableObject.CreateInstance<GameCatalog>();catalog.Populate();}
        [TearDown]public void Teardown()=>UnityEngine.Object.DestroyImmediate(catalog);
        static string Json(object value)=>JsonUtility.ToJson(value);
        static string Saved(RunState run)=>Json(JsonUtility.FromJson<RunState>(Json(run)));
        static CombatSimulation Copy(AccountSave account,CombatSimulation source,GameCatalog catalog)
            =>new CombatSimulation(JsonUtility.FromJson<AccountSave>(Json(account)),catalog,source.State.stage,
                restore:JsonUtility.FromJson<RunState>(Json(source.State)));
        static void Advance(CombatSimulation sim,int ticks){for(int i=0;i<ticks;i++)sim.Tick(CombatSimulation.Step);}

        [TestCase(1)][TestCase(5)][TestCase(12)]
        public void ARestoredRiftKeepsFightingTheSameFight(int stage)
        {
            var account=GameStore.NewAccount(catalog);account.Hero.level=30;account.Hero.capacity=100;
            var sim=new CombatSimulation(account,catalog,stage,seed:(uint)(4242+stage));
            Advance(sim,200);
            var control=Copy(account,sim,catalog);
            Advance(sim,100);Advance(control,100);
            // Position, random streams, health, cooldowns, enemies, drops and the hero itself are exact.
            Assert.AreEqual(Json(sim.Hero),Json(control.Hero),"the hero diverged after restoring");
            Assert.AreEqual(sim.State.rng,control.State.rng);Assert.AreEqual(sim.State.rewardRng,control.State.rewardRng);
            Assert.AreEqual(sim.State.position,control.State.position);Assert.AreEqual(sim.State.health,control.State.health);
            Assert.AreEqual(Json(sim.State.enemies),Json(control.State.enemies));
            Assert.AreEqual(Json(sim.State.drops),Json(control.State.drops));
            // In its saved form the whole run matches, which is the form the game reloads from.
            Assert.AreEqual(Saved(sim.State),Saved(control.State),"a restored run must save back to the same file");
        }
        [Test]
        public void ReopeningARunDoesNotRepeatTheEdictBlockedLine()
        {
            // A hero whose document does not match the loadout it is fighting with reports the block once.
            var account=ContentTestAccounts.Training(catalog);account.selectedHero=2;var hero=account.Hero;hero.level=30;
            hero.build=BehaviorPresets.ForLevel(HeroClass.Mage,0,30,catalog);
            hero.edict=HuntEdictV2Storage.CreateForHero(hero);hero.edict.slots[0]="";hero.useEdict=true;
            var sim=new CombatSimulation(account,catalog,1,0,ownedTraining:true);
            Advance(sim,20);
            int blocked=sim.State.logs.Count(l=>l.Contains("EDICT_BLOCKED"));
            Assert.AreEqual(1,blocked,"the block is reported once while the run continues");
            var reopened=Copy(account,sim,catalog);
            Advance(reopened,20);
            Assert.AreEqual(blocked,reopened.State.logs.Count(l=>l.Contains("EDICT_BLOCKED")),"reopening repeated the line");
            Advance(sim,20);
            Assert.AreEqual(Json(sim.State.logs),Json(reopened.State.logs),"the reopened run wrote a different log");
        }
        [Test]
        public void OnlyTheAccumulatedDamageTelemetryLosesPrecisionInTheSaveFormat()
        {
            var account=GameStore.NewAccount(catalog);account.Hero.level=30;account.Hero.capacity=100;
            var sim=new CombatSimulation(account,catalog,5,seed:4247);
            Advance(sim,200);
            var reloaded=JsonUtility.FromJson<RunState>(Json(sim.State));
            var a=sim.State.statistics;var b=reloaded.statistics;
            // JsonUtility writes a double with too few digits to reproduce every value exactly.
            Assert.AreEqual(a.damage,b.damage,Math.Abs(a.damage)*1e-15,"damage moved by more than the format's rounding");
            Assert.AreEqual(a.hpDamage,b.hpDamage,Math.Abs(a.hpDamage)*1e-15,"hp damage moved by more than the format's rounding");
            // Nothing else in the run may change, including every other statistic.
            var trimmed=JsonUtility.FromJson<RunState>(Json(sim.State));
            trimmed.statistics=a;Assert.AreEqual(Json(sim.State),Json(trimmed),"a field other than the damage totals changed on reload");
        }
    }
}
