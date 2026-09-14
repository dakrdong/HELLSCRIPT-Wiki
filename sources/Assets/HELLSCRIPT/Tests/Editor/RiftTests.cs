using System;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hellscript.Tests
{
    public sealed class RiftTests
    {
        static GameCatalog Catalog(){var catalog=ScriptableObject.CreateInstance<GameCatalog>();catalog.Populate();return catalog;}
        [Test]
        public void AllSixFallbackLayoutsPassWithDifferentRewardSeeds()
        {
            for(int theme=0;theme<2;theme++)for(int index=0;index<3;index++)for(uint seed=1;seed<=5;seed++)
            {var map=RiftGenerator.Fallback(seed,"fallback-test",seed%2==0?30:1,HeroClass.Warrior,theme,index);RiftGenerator.Validate(map);Assert.AreEqual(7,map.rooms.Count);Assert.IsNotEmpty(map.fallbackId);}
        }
        [Test]
        public void BossExclusionRemainsEvenAcrossAdjacentSeeds()
        {
            var transitions=new int[3,3];int previous=0;
            for(uint seed=1;seed<=100000;seed++){uint rng=RiftGenerator.Derive(seed,"boss-test");int boss=RiftGenerator.SelectBoss(ref rng,previous);Assert.AreNotEqual(previous,boss);transitions[previous,boss]++;previous=boss;}
            for(int a=0;a<3;a++){int first=transitions[a,(a+1)%3],second=transitions[a,(a+2)%3];Assert.Less(Math.Abs(first-second),6*Math.Sqrt(first+second)+2);}
        }
        [Test]
        public void EveryTemplateRotationAndVariantPreservesPortsAndAnchors()
        {
            foreach(var template in RiftTemplates.All)for(int rotation=0;rotation<4;rotation++)for(int variant=0;variant<3;variant++)
            {
                var map=new RiftLayout();var room=RiftTemplates.Instantiate(template,0,Vector2.zero,rotation,variant,map.obstacles);map.rooms.Add(room);
                foreach(var door in room.doors){door.corridor=map.corridors.Count;map.corridors.Add(new RiftCorridor{points=new System.Collections.Generic.List<Vector2>{door.position,door.position+door.direction*5}});}
                map.start=room.doors[0].position-room.doors[0].direction*2;var nav=new RiftNavigation(map);string label=$"{template.id} r={rotation} v={variant}";
                foreach(var door in room.doors){Assert.IsTrue(nav.Reachable(door.position),label);Assert.IsTrue(nav.TravelClear(door.position-door.direction*3,door.position+door.direction*3,1.2f),label);}
                foreach(var p in room.groupAnchors.Concat(room.chestAnchors))Assert.IsTrue(nav.Reachable(p),label);
                if(template.boss)for(int n=0;n<32;n++){float angle=n*Mathf.PI/16;Assert.IsTrue(nav.CanLand(new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*6,1.2f),label);}
            }
        }
        static RiftChest PrepareChest(CombatSimulation sim,bool withItem=true)
        {
            foreach(var e in sim.State.enemies)e.dead=true;
            var c=sim.State.layout.chests.First(c=>(c.reward!=null)==withItem);c.discovered=true;c.phase=ChestPhase.Opening;c.progress=0;
            sim.State.position=c.openingPosition;sim.State.meter=99;sim.State.exploration.chestId=c.id;sim.State.targetId=-1;
            return c;
        }
        [Test]
        public void OpeningFreezesOnPauseAndDamageResetsProgressWithoutReward()
        {
            var catalog=Catalog();try
            {
                var account=GameStore.NewAccount();var sim=new CombatSimulation(account,catalog,1,seed:111);var chest=PrepareChest(sim);
                sim.Tick(.05f);Assert.Greater(chest.progress,0);sim.State.paused=true;float progress=chest.progress;sim.Tick(.5f);Assert.AreEqual(progress,chest.progress);
                sim.State.paused=false;sim.State.portal=true;sim.Tick(.5f);Assert.AreEqual(progress,chest.progress);sim.State.portal=false;
                sim.State.effects.Add(new GroundEffect{id=5000,position=sim.State.position,radius=1,damage=1,duration=1,hostile=true});sim.Tick(.05f);
                Assert.AreEqual(ChestPhase.Available,chest.phase);Assert.AreEqual(0,chest.progress);Assert.AreEqual(0,account.gold);Assert.IsEmpty(account.transactions);
            }finally{UnityEngine.Object.DestroyImmediate(catalog);}
        }
        [Test]
        public void ChestCommitSurvivesReloadAndCannotDuplicateCurrencyOrItem()
        {
            string directory=Path.Combine(Path.GetTempPath(),"HellscriptChest-"+Guid.NewGuid());var catalog=Catalog();
            try
            {
                var store=new GameStore(directory);var sim=new CombatSimulation(store.Data,catalog,1,seed:112);store.Data.suspendedRun=sim.State;
                var chest=PrepareChest(sim);chest.progress=chest.Duration;Assert.IsTrue(store.CommitChest(sim.State,chest));
                Assert.AreEqual(chest.gold,store.Data.gold);Assert.AreEqual(1,sim.State.drops.Count);Assert.AreEqual(chest.reward.id,sim.State.drops[0].item.id);
                Assert.IsTrue(store.CommitChest(sim.State,chest));Assert.AreEqual(1,store.Data.transactions.Count);Assert.AreEqual(chest.gold,store.Data.gold);
                var loaded=new GameStore(directory);var restored=loaded.Data.suspendedRun.layout.chests.Single(c=>c.id==chest.id);
                Assert.AreEqual(ChestPhase.Opened,restored.phase);Assert.AreEqual(chest.dropId,restored.dropId);Assert.AreEqual(1,loaded.Data.suspendedRun.drops.Count);
                Assert.AreEqual(sim.State.layout.fingerprint,loaded.Data.suspendedRun.layout.fingerprint);
                Assert.IsTrue(store.Data.suspendedRun.layout.chests.Any(c=>c.reward==null));
            }finally{UnityEngine.Object.DestroyImmediate(catalog);if(Directory.Exists(directory))Directory.Delete(directory,true);}
        }
        [Test]
        public void FailedChestSavePreservesCurrencyDropAndOpeningState()
        {
            string directory=Path.Combine(Path.GetTempPath(),"HellscriptChestFail-"+Guid.NewGuid());var catalog=Catalog();
            try
            {
                var store=new GameStore(directory);var sim=new CombatSimulation(store.Data,catalog,1,seed:113);store.Data.suspendedRun=sim.State;
                var chest=PrepareChest(sim);chest.progress=chest.Duration;string before=JsonUtility.ToJson(store.Data);
                Directory.CreateDirectory(Path.Combine(directory,"hellscript-local-v1.json.tmp"));LogAssert.Expect(LogType.Error,new Regex("저장하지 못했습니다:"));
                Assert.IsFalse(store.CommitChest(sim.State,chest));Assert.AreEqual(before,JsonUtility.ToJson(store.Data));
                Directory.Delete(Path.Combine(directory,"hellscript-local-v1.json.tmp"));Assert.IsTrue(store.CommitChest(sim.State,chest));Assert.AreEqual(1,sim.State.drops.Count);
            }finally{UnityEngine.Object.DestroyImmediate(catalog);if(Directory.Exists(directory))Directory.Delete(directory,true);}
        }
        [Test]
        public void FullBagKeepsChestDropForPortalAndBossEndingClosesUnopenedChests()
        {
            var catalog=Catalog();try
            {
                var account=GameStore.NewAccount();account.Hero.capacity=0;var sim=new CombatSimulation(account,catalog,1,seed:114);var chest=PrepareChest(sim);chest.progress=chest.Duration;
                sim.Tick(.05f);Assert.AreEqual(ChestPhase.Opened,chest.phase);int gold=account.gold;
                for(int n=0;n<45;n++)sim.Tick(.05f);Assert.IsTrue(sim.State.portal);Assert.AreEqual(gold,account.gold);Assert.IsFalse(sim.State.drops[0].claimed);
                sim.State.portal=false;account.Hero.capacity=50;sim.State.portalCast=0;sim.Tick(.05f);Assert.IsTrue(sim.State.drops[0].claimed);Assert.AreEqual(1,account.Hero.inventory.Count(i=>i.id==chest.reward.id));
                sim.Abandon();Assert.IsTrue(sim.State.layout.chests.Where(c=>c!=chest).All(c=>c.phase==ChestPhase.Exhausted));
            }finally{UnityEngine.Object.DestroyImmediate(catalog);}
        }
        [Test]
        public void BossDeathAndCompletedOpeningInSameTickBothCommitOnce()
        {
            var catalog=Catalog();try
            {
                var account=GameStore.NewAccount();var sim=new CombatSimulation(account,catalog,1,seed:115);var chest=PrepareChest(sim);chest.progress=chest.Duration-.05f;
                sim.State.enemies.Add(new EnemyState{id=9000,boss=true,health=1,maxHealth=1,poison=2,poisonDamage=100,position=sim.State.position+Vector2.right*30});
                sim.State.build.chestsAfterBoss=true;sim.Tick(.05f);
                Assert.AreEqual(ChestPhase.Opened,chest.phase);Assert.IsTrue(sim.State.bossRewarded);
                // Boss gold is a physical resource drop and may be collected on a later tick.
                int TotalGold()=>account.gold+sim.State.resources.Where(r=>r.kind==RiftResourceKind.Gold&&!r.claimed&&!r.ignored).Sum(r=>r.amount);
                Assert.AreEqual(1950+chest.gold,TotalGold());Assert.AreEqual(1,account.transactions.Count);
                Assert.AreEqual(4,sim.State.drops.Count);sim.Tick(.05f);Assert.AreEqual(1950+chest.gold,TotalGold());
                foreach(var resource in sim.State.resources.Where(r=>r.kind==RiftResourceKind.Gold).ToList())RiftResources.Claim(account,sim.State,resource);
                Assert.AreEqual(1950+chest.gold,account.gold);sim.Tick(.05f);Assert.AreEqual(1950+chest.gold,TotalGold());
            }finally{UnityEngine.Object.DestroyImmediate(catalog);}
        }
        [Test]
        public void SixBuildsTraverseGeneratedMapsWithoutInvalidPositions()
        {
            var catalog=Catalog();try
            {
                for(int hero=0;hero<3;hero++)for(int build=0;build<2;build++)
                {
                    var account=GameStore.NewAccount();account.selectedHero=hero;account.Hero.level=30;account.Hero.build=GameCatalog.Preset((HeroClass)hero,build);
                    var sim=new CombatSimulation(account,catalog,1,seed:(uint)(710+hero*2+build));
                    for(int tick=0;tick<6001&&sim.State.phase!=RunPhase.Cleared&&sim.State.phase!=RunPhase.Failed;tick++)
                    {sim.Tick(.05f);Assert.IsTrue(sim.HeroTravelling?sim.Map.LineClear(sim.State.heroAction.origin,sim.State.position)&&sim.Map.CanLand(sim.State.heroAction.destination):sim.Map.Walkable(sim.State.position),$"hero={hero} build={build} tick={tick}");Assert.IsEmpty(sim.State.navigationError);if(sim.State.portal)sim.Abandon();}
                    Assert.Greater(sim.State.kills,10);Assert.Greater(sim.State.visited.Count,1);Assert.IsTrue(sim.State.phase==RunPhase.Cleared||sim.State.phase==RunPhase.Failed);
                    Debug.Log($"RIFT_BUILD class={hero} build={build} phase={sim.State.phase} visited={sim.State.visited.Count} kills={sim.State.kills} meter={sim.State.meter} time={sim.State.time} chests={sim.State.layout.chests.Count(c=>c.phase==ChestPhase.Opened)}");
                }
            }finally{UnityEngine.Object.DestroyImmediate(catalog);}
        }
        [Test]
        public void SeededMapsMeetGeometryEncounterAndRewardContracts()
        {
            string previous="";int boss=-1;
            for(uint seed=1;seed<=24;seed++)
            {
                var map=RiftGenerator.Generate(seed,"test",10,HeroClass.Mage,previous,boss,(int)(seed%2),6+(int)(seed%3));
                // Geometry and encounter placement are checked with the eventual gate passage open.
                // Closed-side reachability is independently enforced by GatePassageTests and Validate.
                RiftGenerator.Validate(map);var nav=new RiftNavigation(map,gatesOpen:true);
                Assert.AreNotEqual(previous,map.fingerprint);
                if(ContentUnlocks.BossCount(map.contentStage)>1)Assert.AreNotEqual(boss,map.bossKind);
                else Assert.AreEqual(0,map.bossKind);
                Assert.AreEqual(195,map.chests.Sum(c=>c.gold));Assert.AreEqual(1,map.chests.Sum(c=>c.materials));
                foreach(var room in map.rooms)
                {
                    Assert.LessOrEqual(map.spawns.Count(s=>s.room==room.index&&s.elite>=0),2);
                    foreach(var door in room.doors.Where(d=>d.corridor>=0))Assert.IsTrue(nav.Reachable(door.position));
                }
                foreach(var spawn in map.spawns){Assert.IsTrue(nav.Reachable(spawn.position));Assert.GreaterOrEqual(Vector2.Distance(spawn.position,map.start),10);}
                foreach(var chest in map.chests)
                {Assert.GreaterOrEqual(RiftGenerator.GraphDistances(map,0)[chest.room],2);foreach(var access in chest.accessPoints)Assert.IsTrue(nav.Reachable(access));Assert.GreaterOrEqual(nav.Length(map.start,chest.openingPosition),45);}
                previous=map.fingerprint;boss=map.bossKind;
                Debug.Log($"RIFT_SEED {seed} rooms={map.rooms.Count} candidate={map.candidate} fallback={map.fallbackId}");
            }
        }
        [Test]
        public void EveryLayoutCirculatesThroughAllRoomsWithoutDeadEnds()
        {
            for(uint seed=1;seed<=40;seed++)
            {
                var map=RiftGenerator.Generate(seed,"roles",10,HeroClass.Warrior,"",-1,(int)(seed%2),6+(int)(seed%3));
                Assert.AreEqual(1,map.rooms.Count(r=>r.role==RiftRoomRole.Entrance));
                Assert.AreEqual(0,map.rooms.Count(r=>r.role==RiftRoomRole.Antechamber||r.role==RiftRoomRole.BossArena||r.role==RiftRoomRole.Wing));
                Assert.AreEqual(RiftRoomRole.Entrance,map.rooms[0].role);
                Assert.AreEqual(-1,map.bossRoom);Assert.AreEqual(1,map.rooms.Count(r=>r.central));
                foreach(var room in map.rooms)Assert.GreaterOrEqual(room.doors.Count(d=>d.corridor>=0),2);
                RiftCirculation.Validate(map);Assert.AreEqual(map.rooms.Count+1,RiftCirculation.FindTour(map).Count);
            }
        }
        [Test]
        public void CombatGetsDenserTowardTheGateWithoutChangingTheBudget()
        {
            for(uint seed=1;seed<=16;seed++)
            {
                var map=RiftGenerator.Generate(seed,"beats",10,HeroClass.Ranger,"",-1,(int)(seed%2),6+(int)(seed%3),arenaBoss:true);
                Assert.AreEqual(110,map.spawns.Count(s=>s.elite<0));Assert.AreEqual(8,map.spawns.Count(s=>s.elite>=0));
                var ante=map.rooms.First(r=>r.role==RiftRoomRole.Antechamber);
                int anteNormal=map.spawns.Count(s=>s.room==ante.index&&s.elite<0);
                foreach(var room in map.rooms.Where(r=>r.index!=ante.index&&!r.boss&&r.index!=0))
                    Assert.GreaterOrEqual(anteNormal,map.spawns.Count(s=>s.room==room.index&&s.elite<0),"The antechamber holds the largest share.");
                Assert.GreaterOrEqual(map.spawns.Count(s=>s.room==ante.index&&s.elite>=0),1,"The antechamber always has an elite.");
                Assert.AreEqual(0,map.spawns.Count(s=>s.room==0&&s.elite>=0),"The entrance has no elite.");
                Assert.AreEqual(10,map.spawns.Count(s=>s.room==0&&s.elite<0));
                Assert.IsTrue(map.groups.Where(g=>g.room==ante.index).Any(g=>g.spawns.Count>=7),"Packs at the gate are larger than at the entrance.");
                Assert.IsTrue(map.groups.Where(g=>g.room==0).All(g=>g.spawns.Count<=6));
            }
        }
        [Test]
        public void SavedManifestAndIndependentStreamsReplayExactly()
        {
            var a=RiftGenerator.Generate(789,"same-run",17,HeroClass.Warrior);var b=RiftGenerator.Generate(789,"same-run",17,HeroClass.Warrior);
            Assert.AreEqual(JsonUtility.ToJson(a),JsonUtility.ToJson(b));
            var restored=JsonUtility.FromJson<RiftLayout>(JsonUtility.ToJson(a));
            Assert.AreEqual(a.fingerprint,restored.fingerprint);Assert.AreEqual(a.obstacles.Count,restored.obstacles.Count);
            Assert.AreEqual(a.rewardSeed,restored.rewardSeed);Assert.AreNotEqual(a.combatSeed,a.rewardSeed);
            var nav=new RiftNavigation(restored,gatesOpen:true);foreach(var point in restored.rooms.SelectMany(r=>r.groupAnchors))Assert.IsNotNull(nav.FindPath(restored.start,point));
        }
        [TestCase(.05f,false)]
        [TestCase(.13f,false)]
        [TestCase(.21f,false)]
        [TestCase(.4f,false)]
        [TestCase(.13f,true)]
        public void MeleeApproachAtGeneratedWallCornerMakesProgress(float step,bool restore)
        {
            // The case is a hero standing at a wall corner whose target is legal but not in a straight
            // line. Coordinates used to be pinned to one generated layout, which broke whenever the
            // composition rules changed, so the corner is now found by its defining property.
            var map=RiftGenerator.Generate(91367,"corner-regression",1,HeroClass.Warrior);var nav=new RiftNavigation(map);
            Vector2 p=Vector2.zero,target=Vector2.zero;float shortest=float.MaxValue;
            foreach(var o in map.obstacles.Where(o=>o.blocksWalk&&o.halfSize.x>0&&o.halfSize.y>0))
            {
                var probes=new[]{o.position+new Vector2(o.halfSize.x+.6f,0),o.position+new Vector2(0,o.halfSize.y+.6f),
                                 o.position-new Vector2(o.halfSize.x+.6f,0),o.position-new Vector2(0,o.halfSize.y+.6f)};
                for(int a=0;a<4;a++)for(int b=0;b<4;b++)
                {
                    if(a==b||a==(b+2)%4)continue;
                    Vector2 from=probes[a],to=probes[b];float gap=Vector2.Distance(from,to);
                    if(gap>=shortest||gap>4||!nav.Walkable(from)||!nav.Walkable(to)||nav.TravelClear(from,to))continue;
                    var candidate=nav.FindPath(from,to);if(candidate==null)continue;
                    shortest=gap;p=from;target=to;
                }
            }
            Assert.Less(shortest,4f,"The generated layout has no wall corner to test.");
            var path=nav.FindPath(p,target);Assert.IsNotNull(path,"A valid detour must exist.");
            Debug.Log("CORNER_PATH "+string.Join(" -> ",path.Select(point=>point.ToString("F5"))));
            for(int tick=0;tick<400&&Vector2.Distance(p,target)>.05f;tick++)
            {
                if(restore&&tick==2)nav=new RiftNavigation(JsonUtility.FromJson<RiftLayout>(JsonUtility.ToJson(map)));
                var next=nav.Move(p,target,step,0,tick*CombatSimulation.Step);
                Assert.IsTrue(nav.Walkable(next));Assert.IsTrue(nav.TravelClear(p,next));p=next;
            }
            Assert.Less(Vector2.Distance(p,target),.05f,"A reachable combat target cannot wait forever at a corner.");
        }
        [Test]
        public void PathFollowingCannotCrossAnObstacleOrVoid()
        {
            var map=RiftGenerator.Generate(144,"path-test",1,HeroClass.Warrior);var nav=new RiftNavigation(map);
            Vector2 p=map.start,target=map.rooms.Single(r=>r.central).groupAnchors[0];var path=nav.FindPath(p,target);Assert.IsNotNull(path);
            for(int tick=0;tick<6000&&Vector2.Distance(p,target)>.1f;tick++)
            {var next=nav.Move(p,target,.2f,0,tick*.05f);Assert.IsTrue(nav.TravelClear(p,next));p=next;}
            Assert.Less(Vector2.Distance(p,target),.11f);
        }
    }
}
