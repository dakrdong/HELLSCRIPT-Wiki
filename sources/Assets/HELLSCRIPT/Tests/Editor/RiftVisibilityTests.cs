using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace Hellscript.Tests
{
    public sealed class RiftVisibilityTests
    {
        [Test] public void RecordedRangesReplayEveryFogPixelAndReuseAnUnchangedView()
        {
            var run=Doorway();RiftVisibility.Initialize(run,new RiftNavigation(run.layout),false);
            var live=Doorway();RiftVisibility.Initialize(live,new RiftNavigation(live.layout),false);
            var flags=BindingFlags.Instance|BindingFlags.NonPublic;
            var record=typeof(RiftVisibility).GetMethod("RecordVisibleRanges",flags);
            var play=typeof(RiftVisibility).GetMethod("PlayRecordedRanges",flags);
            int[] ranges=null;
            foreach(var position in new[]{run.position,new Vector2(-13,4),new Vector2(13,4),new Vector2(500,500)})
            {
                run.position=position;run.visibility.Update(true);
                ranges=(int[])record.Invoke(run.visibility,new object[]{ranges});
                Assert.AreSame(ranges,record.Invoke(run.visibility,new object[]{ranges}));
                live.position=position;live.discovery.cells=new System.Collections.Generic.List<int>(run.discovery.cells);
                play.Invoke(live.visibility,new object[]{ranges});
                CollectionAssert.AreEqual(run.visibility.Pixels,live.visibility.Pixels);
                Assert.AreEqual(123u,run.rng);Assert.AreEqual(123u,live.rng);
                var pixels=(Color32[])live.visibility.Pixels.Clone();play.Invoke(live.visibility,new object[]{ranges});
                CollectionAssert.AreEqual(pixels,live.visibility.Pixels);
            }
            Assert.IsEmpty(ranges);
        }
        public static RunState Doorway()
        {
            var layout = new RiftLayout { fingerprint = "visibility-doorway", gateOpen = true };
            layout.rooms.Add(new RiftRoom { index = 0, position = new Vector2(-8, 0), size = new Vector2(12, 12) });
            layout.rooms.Add(new RiftRoom { index = 1, position = new Vector2(8, 0), size = new Vector2(12, 12) });
            layout.corridors.Add(new RiftCorridor { index = 0, roomA = 0, roomB = 1, width = 2, points = new System.Collections.Generic.List<Vector2> { new Vector2(-2, 0), new Vector2(2, 0) } });
            return new RunState { layout = layout, position = new Vector2(-2.5f, 0), rng = 123, rewardRng = 321 };
        }
        [Test] public void DoorwayRevealsOnlyTheVisiblePartBeforeEnteringTheRoom()
        {
            var run = Doorway(); run.visited.Add(0); var nav = new RiftNavigation(run.layout);
            RiftVisibility.Initialize(run, nav, false); var sight = run.visibility;
            Assert.IsTrue(sight.Visible(new Vector2(4, 0))); Assert.IsTrue(sight.Explored(new Vector2(4.25f, .25f)));
            Assert.IsFalse(sight.Visible(new Vector2(4, 4))); Assert.IsFalse(sight.Explored(new Vector2(4.25f, 4.25f)));
            CollectionAssert.AreEqual(new[] { 0 }, run.visited); Assert.AreEqual(123u, run.rng); Assert.AreEqual(321u, run.rewardRng);
        }
        [Test] public void TerrainIsRememberedButCurrentSightIsRecomputedOnMovementAndResume()
        {
            var run = Doorway(); RiftVisibility.Initialize(run, new RiftNavigation(run.layout), false);
            var seen = new Vector2(4.25f, .25f); Assert.IsTrue(run.visibility.Explored(seen));
            run.position = new Vector2(-13, 4); run.visibility.Update(); Assert.IsTrue(run.visibility.Explored(seen)); Assert.IsFalse(run.visibility.Visible(seen));
            string json = JsonUtility.ToJson(run); Assert.IsFalse(json.Contains("Navigation"));
            var restored = JsonUtility.FromJson<RunState>(json); RiftVisibility.Initialize(restored, new RiftNavigation(restored.layout), true);
            Assert.AreEqual(run.discovery.cells.Count, restored.discovery.cells.Count); Assert.IsTrue(restored.visibility.Explored(seen)); Assert.IsFalse(restored.visibility.Visible(seen));
        }
        [Test] public void FreshRunsDoNotRevealVisitedRoomsAndOldSavesOnlyMigrateKnownGeometry()
        {
            var fresh = Doorway(); fresh.visited.Add(0); var old = Doorway(); old.visited.Add(0);
            RiftVisibility.Initialize(fresh, new RiftNavigation(fresh.layout), false); RiftVisibility.Initialize(old, new RiftNavigation(old.layout), true);
            var corner = new Vector2(-13.75f, 5.75f);
            Assert.IsFalse(fresh.visibility.Explored(corner)); Assert.IsTrue(old.visibility.Explored(corner));
            Assert.IsFalse(old.visibility.Explored(new Vector2(13.25f, 5.25f)));
        }
        [Test] public void TeleportDoesNotRevealTheRouteAndStandingStillDoesNotRebuild()
        {
            var run = Doorway(); run.position = new Vector2(-13, 4); RiftVisibility.Initialize(run, new RiftNavigation(run.layout), false);
            int revision = run.visibility.Revision; Assert.IsFalse(run.visibility.Update()); Assert.AreEqual(revision, run.visibility.Revision);
            run.position = new Vector2(13, 4); run.visibility.Update(); Assert.IsFalse(run.visibility.Explored(Vector2.zero));
        }
        [Test] public void ClosedGateBlocksSightAndOpeningItRevealsWithoutMovement()
        {
            var run = Doorway(); run.layout.gateOpen = false;
            run.layout.gates.Add(new RiftGate { barrier = new RiftObstacle { position = Vector2.zero, halfSize = new Vector2(.4f, 1.2f), blocksSight = true } });
            RiftVisibility.Initialize(run, new RiftNavigation(run.layout), false);
            Assert.IsFalse(run.visibility.Explored(new Vector2(4.25f, .25f)));
            run.layout.gateOpen = true; Assert.IsTrue(run.visibility.Update()); Assert.IsTrue(run.visibility.Explored(new Vector2(4.25f, .25f)));
        }
        [Test] public void OrganicMapVisibilityUsesSavedSurfaceAndReportsItsUpdateCost()
        {
            var catalog = ScriptableObject.CreateInstance<GameCatalog>(); catalog.Populate();
            try
            {
                var sim = new CombatSimulation(GameStore.NewAccount(), catalog, 1, seed: 7421); var sight = sim.State.visibility;
                var watch = Stopwatch.StartNew();
                for (int i = 0; i < 60; i++) { sim.State.position = sim.State.layout.start + Vector2.right * (i % 5) * .02f; sight.Update(true); }
                watch.Stop(); UnityEngine.Debug.Log("RIFT_VISIBILITY_COST_MS " + watch.Elapsed.TotalMilliseconds / 60 + " cells=" + sight.Width * sight.Height);
                foreach (int i in sim.State.discovery.cells) Assert.Less(i, sight.Width * sight.Height);
            }
            finally { UnityEngine.Object.DestroyImmediate(catalog); }
        }
        [TestCase(7421u)][TestCase(18171u)][TestCase(91333u)]
        public void VisibilityRasterAgreesWithAuthoritativeSightAcrossOrganicRoomsAndBlockers(uint seed)
        {
            var catalog = ScriptableObject.CreateInstance<GameCatalog>(); catalog.Populate();
            try
            {
                var sim = new CombatSimulation(GameStore.NewAccount(), catalog, 1, seed: seed); var sight = sim.State.visibility;
                var positions=new System.Collections.Generic.List<Vector2>();uint rng=sim.State.rng;
                foreach (var room in sim.State.layout.rooms)
                {
                    positions.Add(room.position);
                    if(room.outline?.Count>0)
                    {
                        positions.Add(room.outline[0]);positions.Add(room.outline[0]+Vector2.one*.00005f);
                        positions.Add(room.outline[0]-Vector2.one*.00005f);
                    }
                }
                foreach(var position in positions)
                {
                    sim.State.position = position; sight.Update(true);
                    for (int y = 0; y < sight.Height; y++) for (int x = 0; x < sight.Width; x++)
                    {
                        var p = sight.Position(x, y); if ((p - sim.State.position).sqrMagnitude > RiftVisibility.Radius * RiftVisibility.Radius || !sim.Map.Surface.Contains(p)) continue;
                        Assert.AreEqual(sim.Map.LineClear(sim.State.position,p), sight.Pixels[y*sight.Width+x].g > 0, "LOS mismatch at " + p);
                    }
                }
                sim.State.position=new Vector2(5000,5000);sight.Update(true);
                foreach(var pixel in sight.Pixels)Assert.AreEqual(0,pixel.g);
                Assert.AreEqual(rng,sim.State.rng);
            }
            finally { UnityEngine.Object.DestroyImmediate(catalog); }
        }
    }
}
