using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Hellscript.Tests
{
    public sealed class RoamingBossTests
    {
        GameCatalog catalog;
        [SetUp]public void Setup(){catalog=ScriptableObject.CreateInstance<GameCatalog>();catalog.Populate();}
        [TearDown]public void Teardown()=>Object.DestroyImmediate(catalog);
        CombatSimulation Fixture(uint seed=91367,int stage=10)
        {
            var account=GameStore.NewAccount(catalog);account.Hero.level=30;
            var sim=new CombatSimulation(account,catalog,stage,seed:seed);
            foreach(var enemy in sim.State.enemies){enemy.dead=true;enemy.pendingDeath=false;}
            sim.State.build.openChests=false;sim.State.build.useShrines=false;
            return sim;
        }
        [TestCase(1)] [TestCase(10)] [TestCase(30)]
        public void FinalMeterKillSpawnsOneNearbyBossWithoutAnArena(int stage)
        {
            var sim=Fixture(stage:stage);var run=sim.State;var origin=run.position;
            Assert.IsTrue(run.layout.roamingBoss);Assert.AreEqual(-1,run.layout.bossRoom);
            Assert.IsEmpty(run.layout.bossPoints);Assert.IsEmpty(run.layout.gates);Assert.IsFalse(sim.ObjectiveActive);
            run.meter=99;sim.Tick(.05f);Assert.AreEqual(-1,run.bossId);
            var last=run.enemies.First(e=>e.elite<0);last.pendingDeath=true;sim.Tick(.05f);
            var boss=run.enemies.Single(e=>e.boss);Assert.AreEqual(RunPhase.Boss,run.phase);Assert.AreEqual(boss.id,run.bossId);
            Assert.That(Vector2.Distance(origin,boss.position),Is.InRange(5.5f,18.5f));
            Assert.IsTrue(sim.Map.CanLand(boss.position,1.2f));Assert.LessOrEqual(sim.Map.Length(run.position,boss.position),RiftBossPlacement.MaximumPath+.5f);
            int id=boss.id;for(int n=0;n<10;n++)sim.Tick(.05f);
            Assert.AreEqual(id,run.enemies.Single(e=>e.boss).id);
        }
        [Test]
        public void TemporaryCrowdingRetriesWithoutFaultingOrSpawningInsideDanger()
        {
            var sim=Fixture();var run=sim.State;run.meter=100;
            run.effects.Add(new GroundEffect{id=990001,position=run.position,hostile=true,radius=1000,damage=0,duration=20});
            for(int n=0;n<65;n++)sim.Tick(.05f);
            Assert.AreEqual(RunPhase.Boss,run.phase);Assert.AreEqual(-1,run.bossId);Assert.IsEmpty(run.navigationError);
            run.effects.Clear();for(int n=0;n<10&&run.bossId<0;n++)sim.Tick(.05f);
            Assert.AreEqual(1,run.enemies.Count(e=>e.boss));Assert.IsEmpty(run.navigationError);
        }
        [TestCase(false)] [TestCase(true)]
        public void SaveBeforeOrAfterSummoningNeverDuplicatesOrRelocatesTheBoss(bool spawned)
        {
            var sim=Fixture();var run=sim.State;run.meter=100;run.phase=RunPhase.Boss;
            if(spawned)sim.Tick(.05f);
            string json=JsonUtility.ToJson(run);var copy=JsonUtility.FromJson<RunState>(json);
            var account=GameStore.NewAccount(catalog);account.Hero.level=30;
            var restored=new CombatSimulation(account,catalog,10,restore:copy);
            if(spawned)
            {
                Assert.AreEqual(run.bossId,copy.bossId);
                Assert.AreEqual(run.enemies.Single(e=>e.boss).position,copy.enemies.Single(e=>e.boss).position);
                Assert.AreEqual(run.rng,copy.rng);
            }
            for(int n=0;n<10;n++)restored.Tick(.05f);
            Assert.AreEqual(1,copy.enemies.Count(e=>e.boss));Assert.IsEmpty(copy.navigationError);
        }
        [Test]
        public void RoomsDoorsAndCrossingsAlwaysOfferNearbyConnectedSpawnSpace()
        {
            for(uint seed=1;seed<=12;seed++)
            {
                var layout=RiftGenerator.Generate(615000+seed,"boss-space",30,HeroClass.Warrior,forcedTheme:(int)(seed%2),forcedCount:6+(int)(seed%3));
                var nav=new RiftNavigation(layout);
                var positions=layout.rooms.SelectMany(r=>r.groupAnchors).Concat(layout.rooms.SelectMany(r=>r.doors.Where(d=>d.corridor>=0).Select(d=>d.position)))
                    .Concat(layout.corridors.SelectMany(c=>c.points.Where((p,i)=>i%12==0))).Append(RiftCrossRoutes.Crossing(layout).position);
                foreach(var player in positions)
                {
                    if(!nav.Walkable(player))continue;
                    var point=RiftBossPlacement.NearPlayer(layout,nav,player,p=>true);
                    Assert.IsTrue(point.HasValue,"No nearby summon space at "+seed+":"+player);
                    Assert.IsTrue(nav.CanLand(point.Value,1.2f));Assert.That(Vector2.Distance(player,point.Value),Is.InRange(5.99f,18.01f));
                    Assert.LessOrEqual(nav.Length(player,point.Value),RiftBossPlacement.MaximumPath);
                }
            }
        }
    }
}
