using System.Collections.Generic;
using UnityEngine;

namespace Hellscript
{
    public sealed partial class WorldView
    {
        readonly Dictionary<int, GameObject> roomGeometry = new Dictionary<int, GameObject>();
        sealed class PassageView { public GameObject go; public int roomA, roomB; public Vector2 center; }
        readonly List<PassageView> passageGeometry = new List<PassageView>();
        bool CanDisplayEnemyMarker(RunState run, Vector2 position, float radius = 0)
            => Vector2.Distance(run.position, position) <= 14 + radius && game.Combat.Map.LineClear(run.position, position);
        Transform RoomGeometry(int index, bool visited)
        {
            var room = new GameObject("Discovered room " + index); room.transform.SetParent(world.transform, false);
            roomGeometry[index] = room; room.SetActive(visited); return room.transform;
        }
        void PresentExploredGeometry(RunState run)
        {
            if (run.layout.legacy) return;
            foreach (var room in roomGeometry) room.Value.SetActive(run.visited.Contains(room.Key));
            foreach (var passage in passageGeometry)
            {
                // Knowledge comes from exploration or the same fixed observation radius on every display.
                // A wider camera cannot reveal additional routes or change exploration state.
                bool known = run.visited.Contains(passage.roomA) && run.visited.Contains(passage.roomB);
                bool observed = Vector2.Distance(run.position, passage.center) <= 12 && game.Combat.Map.LineClear(run.position, passage.center);
                passage.go.SetActive(known || observed);
            }
        }
    }
}
