using System.Collections.Generic;
using UnityEngine;

namespace Hellscript
{
    public sealed partial class WorldView
    {
        readonly Dictionary<int, GameObject> roomGeometry = new Dictionary<int, GameObject>();
        sealed class PassageView { public GameObject go; public int roomA, roomB; public Vector2 center; }
        readonly List<PassageView> passageGeometry = new List<PassageView>();
        RiftFogView riftFog;
        Color? riftBackground;
        public RiftFogView RiftFog => riftFog;
        Material RiftMaterial(Material source) => riftFog != null ? riftFog.Resolve(source) : source;
        bool CanDisplayEnemyMarker(RunState run, Vector2 position, float radius = 0)
            => Vector2.Distance(run.position, position) <= 14 + radius && game.Combat.Map.LineClear(run.position, position);
        Transform RoomGeometry(int index, bool visited)
        {
            var room = new GameObject("Discovered room " + index); room.transform.SetParent(world.transform, false);
            roomGeometry[index] = room; return room.transform;
        }
        void InitializeRiftVisibility(RunState run)
        {
            if (run.layout.legacy) return;
            riftFog = world.AddComponent<RiftFogView>(); riftFog.Initialize(RiftVisibility.Get(run, game.Combat.Map));
            riftBackground = viewCamera.backgroundColor; viewCamera.backgroundColor = Color.black;
        }
        void BindRiftTerrain()
        {
            if (riftFog == null) return;
            foreach (var room in roomGeometry.Values)
                foreach (var renderer in room.GetComponentsInChildren<MeshRenderer>(true))
                    if (renderer.GetComponent<RiftFloorMesh>() != null || renderer.name.EndsWith(" floor") || renderer.name == "Stone inlay" || renderer.name == "Room boundary") riftFog.Bind(renderer, true);
            foreach (var passage in passageGeometry)
            { passage.go.SetActive(true); foreach (var renderer in passage.go.GetComponentsInChildren<MeshRenderer>(true)) riftFog.Bind(renderer, true); }
        }
        void PresentExploredGeometry(RunState run)
        {
            if (run.layout.legacy) return;
            riftFog?.Present();
        }
    }
}
