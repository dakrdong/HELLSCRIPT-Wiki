using System.Collections.Generic;
using UnityEngine;

namespace Hellscript
{
    // Owns the transient mesh so leaving/rebuilding a rift cannot leak native mesh allocations.
    public sealed class RiftFloorMesh : MonoBehaviour
    {
        Mesh ownedMesh;
        void OnDestroy(){if(ownedMesh!=null)Destroy(ownedMesh);}
        public static GameObject Create(string label,Transform parent,IEnumerable<RiftFloorPatch> patches,RiftSurface surface,Material ground,Material edge)
        {
            var vertices=new List<Vector3>();var uv=new List<Vector2>();var floor=new List<int>();var walls=new List<int>();
            int Vertex(Vector2 p,float y){int n=vertices.Count;vertices.Add(new Vector3(p.x,y,p.y));uv.Add(p*.25f);return n;}
            foreach(var patch in patches)
            {
                int first=vertices.Count;foreach(var p in patch.points)Vertex(p,0);
                for(int n=1;n<patch.points.Length-1;n++){floor.Add(first);floor.Add(first+n+1);floor.Add(first+n);}
                for(int n=0;n<patch.points.Length;n++)
                {
                    var a=patch.points[n];var b=patch.points[(n+1)%patch.points.Length];var delta=b-a;
                    var outside=new Vector2(delta.y,-delta.x).normalized;int steps=Mathf.Max(1,Mathf.CeilToInt(delta.magnitude/.65f));
                    for(int k=0;k<steps;k++)
                    {
                        var from=Vector2.Lerp(a,b,k/(float)steps);var to=Vector2.Lerp(a,b,(k+1f)/steps);
                        if(surface.Contains((from+to)*.5f+outside*.025f))continue;
                        // A shallow rock lip sits outside the authoritative floor, never across
                        // a doorway or an overlapping bend. The inner edge stays at floor height.
                        int v=Vertex(from,0);Vertex(to,0);Vertex(to+outside*.35f,.45f);Vertex(from+outside*.35f,.45f);
                        walls.AddRange(new[]{v,v+1,v+2,v,v+2,v+3});
                        v=Vertex(from+outside*.35f,.45f);Vertex(to+outside*.35f,.45f);Vertex(to+outside*.35f,-.8f);Vertex(from+outside*.35f,-.8f);
                        walls.AddRange(new[]{v,v+1,v+2,v,v+2,v+3});
                    }
                }
            }
            var mesh=new Mesh{name=label};if(vertices.Count>65535)mesh.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.subMeshCount=2;mesh.SetTriangles(floor,0);mesh.SetTriangles(walls,1);mesh.RecalculateNormals();mesh.RecalculateBounds();
            var go=new GameObject(label);go.transform.SetParent(parent,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;
            go.AddComponent<MeshRenderer>().sharedMaterials=new[]{ground,edge};go.AddComponent<RiftFloorMesh>().ownedMesh=mesh;return go;
        }
    }
}
