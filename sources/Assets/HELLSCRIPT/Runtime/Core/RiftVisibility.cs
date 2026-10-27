using System;
using System.Collections.Generic;
using UnityEngine;

namespace Hellscript
{
    [Serializable]
    public sealed class RiftDiscovery
    {
        public int version = 1;
        public string fingerprint;
        public List<int> cells = new List<int>();
    }

    // Renderers, cameras and HUD preferences never decide what a run has explored.
    public sealed class RiftVisibility
    {
        public const float CellSize = .35f, Radius = 12f;
        // The walled pit of the tutorial is lit end to end, so both fighters are in view from the first frame.
        float Sight => Run.tutorial ? 40f : Radius;
        public RiftNavigation Navigation { get; private set; }
        public readonly RunState Run;
        public readonly Vector2 Origin;
        public readonly int Width, Height;
        public readonly Color32[] Pixels;
        public readonly List<Vector4> Boundary = new List<Vector4>();
        public int Revision { get; private set; }
        public int ExploredCount => Run.discovery.cells.Count;
        readonly bool[] floor;
        readonly bool[] sightFloor;
        readonly List<int> visible = new List<int>();
        readonly List<RiftObstacle> nearbyOccluders = new List<RiftObstacle>();
        readonly List<RiftFloorPatch> originFloor = new List<RiftFloorPatch>();
        bool originOnFloor;
        Vector2 lastPosition;
        bool initialized, lastGateOpen;
        int lastObstacleCount;

        public static RiftVisibility Get(RunState run, RiftNavigation navigation = null)
        {
            if (run?.layout == null || run.layout.legacy) return null;
            if (run.visibility == null || !ReferenceEquals(run.visibility.Navigation.Layout, run.layout))
                run.visibility = new RiftVisibility(run, navigation ?? new RiftNavigation(run.layout), run.discovery == null);
            else if (navigation != null) run.visibility.Navigation = navigation;
            return run.visibility;
        }
        public static void Initialize(RunState run, RiftNavigation navigation, bool restored)
        {
            if (run.layout.legacy) return;
            if (!restored) run.discovery = new RiftDiscovery { fingerprint = run.layout.fingerprint };
            run.visibility = new RiftVisibility(run, navigation, restored && run.discovery == null);
            run.visibility.Update();
        }
        public RiftVisibility(RunState run, RiftNavigation navigation, bool migrate = false)
        {
            Run = run; Navigation = navigation;
            var bounds = navigation.Surface.Bounds;
            Origin = new Vector2(Mathf.Floor(bounds.xMin / CellSize) * CellSize - 1, Mathf.Floor(bounds.yMin / CellSize) * CellSize - 1);
            Width = Mathf.CeilToInt((bounds.xMax + 1 - Origin.x) / CellSize);
            Height = Mathf.CeilToInt((bounds.yMax + 1 - Origin.y) / CellSize);
            floor = new bool[Width * Height]; sightFloor = new bool[floor.Length]; Pixels = new Color32[floor.Length];
            foreach (var patch in navigation.Surface.patches)
            {
                int x0 = X(patch.bounds.xMin), x1 = X(patch.bounds.xMax), y0 = Y(patch.bounds.yMin), y1 = Y(patch.bounds.yMax);
                for (int y = y0; y <= y1; y++) for (int x = x0; x <= x1; x++)
                    if (Inside(x, y) && patch.Contains(Position(x, y))) floor[y * Width + x] = true;
                for (int n = 0; n < patch.points.Length; n++)
                {
                    var a = patch.points[n]; var b = patch.points[(n + 1) % patch.points.Length];
                    var d = b - a; var outside = new Vector2(d.y, -d.x).normalized;
                    int steps = Mathf.Max(1, Mathf.CeilToInt(d.magnitude / .5f));
                    for (int k = 0; k < steps; k++)
                    {
                        var from = Vector2.Lerp(a, b, k / (float)steps); var to = Vector2.Lerp(a, b, (k + 1f) / steps);
                        if (!navigation.Surface.Contains((from + to) * .5f + outside * .025f)) Boundary.Add(new Vector4(from.x, from.y, to.x, to.y));
                    }
                }
            }
            // Sample positions and floor geometry stay fixed for this visibility instance.
            for(int y=0;y<Height;y++)for(int x=0;x<Width;x++)
                if(floor[y*Width+x])sightFloor[y*Width+x]=navigation.Surface.Contains(Position(x,y));
            run.discovery ??= new RiftDiscovery { fingerprint = run.layout.fingerprint };
            // Cell indices belong to the saved layout, never to a seed regenerated on resume.
            if (run.discovery.version != 1 || run.discovery.fingerprint != run.layout.fingerprint)
                run.discovery = new RiftDiscovery { fingerprint = run.layout.fingerprint };
            run.discovery.cells ??= new List<int>();
            var unique = new HashSet<int>();
            run.discovery.cells.RemoveAll(i => i < 0 || i >= Pixels.Length || !unique.Add(i));
            foreach (int i in run.discovery.cells) Pixels[i].r = 255;
            if (migrate)
            {
                // Previous versions had already disclosed entire visited rooms and completed passages.
                foreach (var patch in navigation.Surface.patches)
                {
                    bool known = patch.room >= 0 && run.visited.Contains(patch.room);
                    if (patch.corridor >= 0 && patch.corridor < run.layout.corridors.Count)
                    { var c = run.layout.corridors[patch.corridor]; known = run.visited.Contains(c.roomA) && run.visited.Contains(c.roomB); }
                    if (!known) continue;
                    for (int y = Y(patch.bounds.yMin); y <= Y(patch.bounds.yMax); y++)
                        for (int x = X(patch.bounds.xMin); x <= X(patch.bounds.xMax); x++)
                            if (Inside(x, y) && patch.Contains(Position(x, y))) Reveal(x, y, false);
                }
            }
        }
        int X(float x) => Mathf.FloorToInt((x - Origin.x) / CellSize);
        int Y(float y) => Mathf.FloorToInt((y - Origin.y) / CellSize);
        bool Inside(int x, int y) => x >= 0 && y >= 0 && x < Width && y < Height;
        public Vector2 Position(int x, int y) => Origin + new Vector2(x + .5f, y + .5f) * CellSize;
        public bool Explored(Vector2 p)
        { int x = X(p.x), y = Y(p.y); return Inside(x, y) && Pixels[y * Width + x].r != 0; }
        public bool Visible(Vector2 p, float extraRadius = 0)
            => (p - Run.position).sqrMagnitude <= (Sight + extraRadius) * (Sight + extraRadius) && Navigation.LineClear(Run.position, p);
        void CollectOccluders()
        {
            nearbyOccluders.Clear();
            void Add(RiftObstacle obstacle)
            {
                if (!obstacle.blocksSight) return;
                var delta = Run.position - obstacle.position;
                if (obstacle.radius > 0)
                { if (delta.sqrMagnitude <= (Sight + obstacle.radius) * (Sight + obstacle.radius)) nearbyOccluders.Add(obstacle); }
                else
                { var closest = new Vector2(Mathf.Max(0, Mathf.Abs(delta.x) - obstacle.halfSize.x), Mathf.Max(0, Mathf.Abs(delta.y) - obstacle.halfSize.y)); if (closest.sqrMagnitude <= Sight * Sight) nearbyOccluders.Add(obstacle); }
            }
            foreach (var obstacle in Run.layout.obstacles) Add(obstacle);
            if (!Run.layout.gateOpen) foreach (var gate in Run.layout.gates) Add(gate.barrier);
        }
        bool SampleVisible(Vector2 p)
        {
            var a = Run.position; var delta = p - a;
            if (!originOnFloor || delta.sqrMagnitude > Sight * Sight) return false;
            // These patches are in SegmentOnFloor's first bucket, so its identical full-cover test is sufficient.
            float tolerance=Mathf.Max(.000001f,.00001f/Mathf.Max(.01f,delta.magnitude));bool covered=false;
            foreach(var patch in originFloor)
                if(patch.Clip(a,p,out float first,out float last)&&first<=tolerance&&last>=1-tolerance){covered=true;break;}
            if(!covered&&!Navigation.Surface.SegmentOnFloorFromFloor(a,p))return false;
            // Same exact floor clipping and obstacle intersections as navigation. Only the broad
            // phase is cached per update, so distant props cannot multiply thousands of LOS tests.
            foreach (var obstacle in nearbyOccluders)
            {
                if (obstacle.radius > 0)
                {
                    float t = delta.sqrMagnitude < .000001f ? 0 : Mathf.Clamp01(Vector2.Dot(obstacle.position - a, delta) / delta.sqrMagnitude);
                    if ((a + delta * t - obstacle.position).sqrMagnitude < obstacle.radius * obstacle.radius) return false;
                }
                else
                {
                    var half = obstacle.halfSize - Vector2.one * .00001f;
                    float begin = 0, end = 1; bool intersects = true;
                    for (int axis = 0; axis < 2; axis++)
                    {
                        float origin = axis == 0 ? a.x : a.y, direction = axis == 0 ? delta.x : delta.y;
                        float min = (axis == 0 ? obstacle.position.x - half.x : obstacle.position.y - half.y), max = min + (axis == 0 ? half.x : half.y) * 2;
                        if (Mathf.Abs(direction) < .000001f) { if (origin < min || origin > max) { intersects = false; break; } continue; }
                        float first = (min - origin) / direction, last = (max - origin) / direction;
                        if (first > last) { float swap = first; first = last; last = swap; }
                        begin = Mathf.Max(begin, first); end = Mathf.Min(end, last); if (begin > end) { intersects = false; break; }
                    }
                    if (intersects) return false;
                }
            }
            return true;
        }
        void Mark(int index, bool current)
        {
            if (Pixels[index].r == 0) { Pixels[index].r = 255; Run.discovery.cells.Add(index); }
            if (current && Pixels[index].g == 0) { Pixels[index].g = 255; visible.Add(index); }
        }
        void Reveal(int x, int y, bool current)
        {
            Mark(y * Width + x, current);
            // Only the rock lip outside the floor gets padding; never reveal neighbouring floor.
            for (int dy = -2; dy <= 2; dy++) for (int dx = -2; dx <= 2; dx++)
            { int xx = x + dx, yy = y + dy; if (Inside(xx, yy) && !floor[yy * Width + xx]) Mark(yy * Width + xx, current); }
        }
        int[] recordedVisible;
        int recordedDiscovery;
        readonly List<int> recordingRanges=new List<int>();
        // Consecutive visible cells share a start/count pair instead of retaining one int per pixel.
        internal int[] RecordVisibleRanges(int[] previous)
        {
            recordingRanges.Clear();int first=Pixels.Length,last=-1;
            foreach(int cell in visible){first=Math.Min(first,cell);last=Math.Max(last,cell);}
            for(int i=first;i<=last;)
            {
                if(Pixels[i].g==0){i++;continue;}
                int start=i++;while(i<=last&&Pixels[i].g!=0)i++;
                recordingRanges.Add(start);recordingRanges.Add(i-start);
            }
            bool same=previous!=null&&previous.Length==recordingRanges.Count;
            for(int i=0;same&&i<previous.Length;i++)same=previous[i]==recordingRanges[i];
            return same?previous:recordingRanges.ToArray();
        }
        // Playback updates the fog texture from recorded cells without exploring or running line-of-sight rules.
        internal void PlayRecordedRanges(int[] cells)
        {
            if(ReferenceEquals(recordedVisible,cells)&&recordedDiscovery==Run.discovery.cells.Count)return;
            recordedVisible=cells;
            foreach(int i in visible)Pixels[i].g=0;visible.Clear();
            if(recordedDiscovery>Run.discovery.cells.Count){for(int i=0;i<Pixels.Length;i++)Pixels[i].r=0;recordedDiscovery=0;}
            for(int i=recordedDiscovery;i<Run.discovery.cells.Count;i++)Pixels[Run.discovery.cells[i]].r=255;
            recordedDiscovery=Run.discovery.cells.Count;
            for(int range=0;range<cells.Length;range+=2)
                for(int i=cells[range],end=i+cells[range+1];i<end;i++){Pixels[i].g=255;visible.Add(i);}
            lastPosition=Run.position;lastGateOpen=Run.layout.gateOpen;lastObstacleCount=Run.layout.obstacles.Count;initialized=true;Revision++;
        }
        public bool Update(bool force = false)
        {
            using var sample=PresentationMetrics.Visibility.Auto();PresentationMetrics.VisibilityCalls++;
            if (!force && initialized && (Run.position - lastPosition).sqrMagnitude < .000001f && lastGateOpen == Run.layout.gateOpen && lastObstacleCount == Run.layout.obstacles.Count) return false;
            foreach (int i in visible) Pixels[i].g = 0; visible.Clear();
            CollectOccluders();
            Navigation.Surface.OriginPatches(Run.position,originFloor);originOnFloor=originFloor.Count>0;
            lastPosition = Run.position; lastGateOpen = Run.layout.gateOpen; lastObstacleCount = Run.layout.obstacles.Count; initialized = true;
            int x0 = Mathf.Max(0, X(Run.position.x - Sight)), x1 = Mathf.Min(Width - 1, X(Run.position.x + Sight));
            int y0 = Mathf.Max(0, Y(Run.position.y - Sight)), y1 = Mathf.Min(Height - 1, Y(Run.position.y + Sight));
            for (int y = y0; y <= y1; y++) for (int x = x0; x <= x1; x++)
                if (sightFloor[y * Width + x] && SampleVisible(Position(x, y))) Reveal(x, y, true);
            Revision++; return true;
        }
    }
}
