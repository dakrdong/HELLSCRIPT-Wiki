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
        public readonly RiftNavigation Navigation;
        public readonly RunState Run;
        public readonly Vector2 Origin;
        public readonly int Width, Height;
        public readonly Color32[] Pixels;
        public readonly List<Vector4> Boundary = new List<Vector4>();
        public int Revision { get; private set; }
        public int ExploredCount => Run.discovery.cells.Count;
        readonly bool[] floor;
        readonly List<int> visible = new List<int>();
        readonly List<RiftObstacle> nearbyOccluders = new List<RiftObstacle>();
        Vector2 lastPosition;
        bool initialized, lastGateOpen;
        int lastObstacleCount;

        public static RiftVisibility Get(RunState run, RiftNavigation navigation = null)
        {
            if (run?.layout == null || run.layout.legacy) return null;
            if (run.visibility == null || !ReferenceEquals(run.visibility.Navigation.Layout, run.layout))
                run.visibility = new RiftVisibility(run, navigation ?? new RiftNavigation(run.layout), run.discovery == null);
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
            floor = new bool[Width * Height]; Pixels = new Color32[floor.Length];
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
            => (p - Run.position).sqrMagnitude <= (Radius + extraRadius) * (Radius + extraRadius) && Navigation.LineClear(Run.position, p);
        void CollectOccluders()
        {
            nearbyOccluders.Clear();
            void Add(RiftObstacle obstacle)
            {
                if (!obstacle.blocksSight) return;
                var delta = Run.position - obstacle.position;
                if (obstacle.radius > 0)
                { if (delta.sqrMagnitude <= (Radius + obstacle.radius) * (Radius + obstacle.radius)) nearbyOccluders.Add(obstacle); }
                else
                { var closest = new Vector2(Mathf.Max(0, Mathf.Abs(delta.x) - obstacle.halfSize.x), Mathf.Max(0, Mathf.Abs(delta.y) - obstacle.halfSize.y)); if (closest.sqrMagnitude <= Radius * Radius) nearbyOccluders.Add(obstacle); }
            }
            foreach (var obstacle in Run.layout.obstacles) Add(obstacle);
            if (!Run.layout.gateOpen) foreach (var gate in Run.layout.gates) Add(gate.barrier);
        }
        bool SampleVisible(Vector2 p)
        {
            var a = Run.position; var delta = p - a;
            if (delta.sqrMagnitude > Radius * Radius || !Navigation.Surface.SegmentOnFloor(a, p)) return false;
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
        public bool Update(bool force = false)
        {
            if (!force && initialized && (Run.position - lastPosition).sqrMagnitude < .000001f && lastGateOpen == Run.layout.gateOpen && lastObstacleCount == Run.layout.obstacles.Count) return false;
            foreach (int i in visible) Pixels[i].g = 0; visible.Clear();
            CollectOccluders();
            lastPosition = Run.position; lastGateOpen = Run.layout.gateOpen; lastObstacleCount = Run.layout.obstacles.Count; initialized = true;
            int x0 = Mathf.Max(0, X(Run.position.x - Radius)), x1 = Mathf.Min(Width - 1, X(Run.position.x + Radius));
            int y0 = Mathf.Max(0, Y(Run.position.y - Radius)), y1 = Mathf.Min(Height - 1, Y(Run.position.y + Radius));
            for (int y = y0; y <= y1; y++) for (int x = x0; x <= x1; x++)
                if (floor[y * Width + x] && SampleVisible(Position(x, y))) Reveal(x, y, true);
            Revision++; return true;
        }
    }
}
