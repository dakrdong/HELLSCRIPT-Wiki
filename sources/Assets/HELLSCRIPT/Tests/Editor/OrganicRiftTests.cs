using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Hellscript.Tests
{
    public sealed class OrganicRiftTests
    {
        [Test]
        public void DiagonalFloorDoesNotFillItsBoundingBox()
        {
            var map=new RiftLayout{start=Vector2.zero};
            map.corridors.Add(new RiftCorridor{points=new List<Vector2>{Vector2.zero,new Vector2(12,12)},widths=new List<float>{4,4}});
            var nav=new RiftNavigation(map);
            Assert.IsTrue(nav.TravelClear(Vector2.zero,new Vector2(12,12),1.2f));
            Assert.IsFalse(nav.Walkable(new Vector2(0,10)));
            Assert.IsFalse(nav.LineClear(Vector2.zero,new Vector2(0,10)));
            Assert.IsFalse(nav.ProjectileClear(Vector2.zero,new Vector2(0,10),.3f));
            Assert.IsFalse(nav.CanLand(new Vector2(0,10)));
        }
        [Test]
        public void SegmentCoverageRejectsThinGapsAndAcceptsTouchingPatches()
        {
            var map=new RiftLayout();
            map.rooms.Add(new RiftRoom{index=0,position=Vector2.zero,size=new Vector2(2,4)});
            map.rooms.Add(new RiftRoom{index=1,position=new Vector2(2.01f,0),size=new Vector2(2,4)});
            Assert.IsFalse(new RiftSurface(map).SegmentOnFloor(Vector2.zero,new Vector2(2,0)));
            map.rooms[1].position=new Vector2(2,0);
            Assert.IsTrue(new RiftSurface(map).SegmentOnFloor(Vector2.zero,new Vector2(2,0)));
        }
        [Test]
        public void OldSavedRectanglesKeepTheirOriginalFloor()
        {
            const string old="{\"version\":4,\"start\":{\"x\":0,\"y\":0},\"rooms\":[{\"index\":0,\"position\":{\"x\":0,\"y\":0},\"size\":{\"x\":10,\"y\":10}}],\"corridors\":[{\"index\":0,\"width\":4,\"points\":[{\"x\":5,\"y\":0},{\"x\":10,\"y\":5}]}]}";
            var restored=JsonUtility.FromJson<RiftLayout>(old);var surface=new RiftSurface(restored);
            Assert.AreEqual(4,restored.version);Assert.IsTrue(surface.Contains(new Vector2(-4.9f,-4.9f)));
            Assert.IsTrue(surface.Contains(new Vector2(5,5)),"Old diagonal AABB floors are preserved on resume.");
            Assert.IsFalse(surface.Contains(new Vector2(-6,0)));
            Assert.IsTrue(new RiftSurface(JsonUtility.FromJson<RiftLayout>(JsonUtility.ToJson(restored))).Contains(new Vector2(5,5)));
        }
        [Test]
        public void ShortStepsDoNotHitInvisibleSeamsInsideAConnectedRoom()
        {
            // Reproduces the Ranger's seed-713 loot route, without a 300-second combat fixture.
            var map=JsonUtility.FromJson<RiftLayout>(File.ReadAllText("Assets/HELLSCRIPT/Tests/Editor/Fixtures/organic-seam.json"));var nav=new RiftNavigation(map);
            var from=new Vector2(42.237030029296875f,-.07635553181171417f);
            var goal=new Vector2(43.27467727661133f,3.1358015537261963f);
            Assert.IsTrue(nav.TravelClear(from,goal));
            foreach(float step in new[]{.001f,.01f,.05f,.1f,.2f,.5f,1f})
            {
                var next=Vector2.MoveTowards(from,goal,step);
                Assert.IsTrue(nav.TravelClear(from,next),"Prefix of a clear route: "+step);
                Assert.Greater(Vector2.Distance(from,nav.MoveDirect(from,goal,step)),step*.99f);
            }
        }
        [Test]
        public void GeneratedContoursRoundTripAndAllContentRemainsReachable()
        {
            string directory="Artifacts/Validation/OrganicRift/maps";Directory.CreateDirectory(directory);
            var densityLines=new List<string>{"seed,candidate,theme,rooms,floorRatio,emptyRadius,hullArea,longestUnbranched"};
            int fallback=0;var shapes=new HashSet<string>();float removed=0,rectArea=0;
            for(uint seed=1;seed<=36;seed++)
            {
                var map=RiftGenerator.Generate(914000+seed,"organic-audit",seed<=6?1:10,HeroClass.Warrior,forcedTheme:(int)(seed%2),forcedCount:6+(int)(seed%3));
                var density=RiftDensity.Measure(map);densityLines.Add(FormattableString.Invariant($"{map.mapSeed},{map.candidate},{map.theme},{map.rooms.Count},{density.floorRatio:F4},{density.emptyRadius:F2},{density.hullArea:F2},{density.longestUnbranched:F2}"));
                RiftDensity.Validate(map);
                RiftGenerator.Validate(map);Assert.IsNotNull(RiftCrossRoutes.Crossing(map));Assert.AreEqual(1,map.rooms.Count(r=>r.central));Assert.AreEqual(-1,map.bossRoom);var nav=new RiftNavigation(map,true);
                Assert.IsEmpty(map.spawns.Where(s=>!nav.Reachable(s.position)));
                Assert.IsEmpty(map.chests.SelectMany(c=>c.accessPoints).Where(p=>!nav.Reachable(p)));
                foreach(var room in map.rooms)
                {
                    Assert.GreaterOrEqual(room.outline.Count,32);float area=0;
                    for(int n=0;n<room.outline.Count;n++)area+=RiftFloorPatch.Cross(room.outline[n]-room.position,room.outline[(n+1)%room.outline.Count]-room.position)*.5f;
                    Assert.Greater(area,room.size.x*room.size.y*.55f);rectArea+=room.size.x*room.size.y;removed+=room.size.x*room.size.y-area;
                }
                foreach(var c in map.corridors)
                {
                    Assert.AreEqual(c.points.Count,c.widths.Count);Assert.AreEqual(4,c.widths[0]);Assert.AreEqual(4,c.widths[c.widths.Count-1]);
                    Assert.Greater(c.widths.Max(),4.5f);Assert.Greater(c.points.Count,5);
                    for(int n=1;n<c.points.Count;n++)Assert.IsTrue(nav.TravelClear(c.points[n-1],c.points[n],1.2f),"Corridor clearance "+seed+":"+c.index+":"+n);
                }
                string json=JsonUtility.ToJson(map);var loaded=JsonUtility.FromJson<RiftLayout>(json);
                Assert.AreEqual(json,JsonUtility.ToJson(loaded));var restored=new RiftNavigation(loaded,true);
                foreach(var spawn in map.spawns)Assert.IsTrue(restored.Reachable(spawn.position));
                if(map.fallbackId!="")fallback++;Assert.IsTrue(shapes.Add(map.fingerprint));
                if(seed<=6)File.WriteAllText(Path.Combine(directory,seed+".json"),json);
            }
            Directory.CreateDirectory("Artifacts/Validation/CompactRift");File.WriteAllLines("Artifacts/Validation/CompactRift/density-audit.csv",densityLines);
            Assert.Less(fallback,4,"Frequent fixed fallbacks hide a broken generator.");
            Assert.Greater(removed/rectArea,.07f,"The new geometry must visibly change the square footprint.");
            File.WriteAllText("Artifacts/Validation/OrganicRift/generation-summary.txt",$"PASS: 36 seeds, both themes, 6/7/8 perimeter rooms + central room, X crossings, all rooms on one complete tour, no bridges/articulation/dead ends, no boss arena or gate, 1.2 m passage clearance, save round trip.\nfallback={fallback}\nroomAreaRemoved={removed/rectArea:P2}\n");
        }
    }
}
