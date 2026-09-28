using System.Collections.Generic;
using UnityEngine;

namespace Hellscript
{
    // Owns the transient mesh so leaving/rebuilding a rift cannot leak native mesh allocations.
    // Submeshes: 0 the floor (UV = world xz / 4 m, the field floor tile's period), 1 the rock lip along the floor's open
    // edges (U along the edge, V down its profile, the 2 m trim period), 2 the walls (4 m wall period). A wall rises only
    // where the edge faces away from the camera, and only as high as keeps the floor behind it in sight, so it never hides
    // a room, a passage or the hero; camera-side edges keep the low lip. Everything stays within .7 m of the floor, inside
    // the fog's reveal pad, and corners turning up to about 90 degrees are mitred so neighbouring edges meet without slivers.
    public sealed class RiftFloorMesh : MonoBehaviour
    {
        // A point at the foot of a full-height wall's face: the view hangs columns and torches there.
        public readonly struct WallAnchor
        {
            public readonly Vector2 at,outside;public readonly float height,along;
            public WallAnchor(Vector2 at,Vector2 outside,float height,float along){this.at=at;this.outside=outside;this.height=height;this.along=along;}
        }
        public const float LipOffset=.35f,LipHeight=.45f,WallDepth=.2f,FullWall=3f;
        // The camera looks along (-12,-25,18): an edge whose outside points the same way is seen face-on across the room.
        static readonly Vector2 Far=new Vector2(-12,18).normalized;
        Mesh ownedMesh;
        void OnDestroy(){if(ownedMesh!=null)Destroy(ownedMesh);}

        // 0 on camera-side edges, 1 on edges facing away from the camera; walls ease between.
        public static float Facing(Vector2 outside)=>Mathf.SmoothStep(0,1,Mathf.InverseLerp(.1f,.55f,Vector2.Dot(outside,Far)));
        // Wall top above the floor at a point of the wall line: the lip where the edge faces the camera, otherwise 3.3-4.4 m
        // with a broken top hashed from the position (neighbours share it), but never taller than keeps the floor behind it
        // in sight: seen from the camera's 49-degree pitch, a wall h tall hides h/1.157 m of ground along the view.
        public static float WallHeight(Vector2 p,Vector2 outside,RiftSurface surface,uint seed)
        {
            float facing=Facing(outside);if(facing<=0)return LipHeight;
            float clear=5.2f;for(float d=.4f;d<5.2f;d+=.4f)if(surface.Contains(p+Far*d)){clear=d;break;}
            return Mathf.Max(LipHeight,Mathf.Min(Mathf.Lerp(LipHeight,3.3f+1.1f*Hash(p,seed),facing),(clear-.4f)*1.157f));
        }
        static float Hash(Vector2 p,uint seed)
        {
            unchecked
            {
                uint h=seed^(uint)Mathf.RoundToInt(p.x*8)*73856093u^(uint)Mathf.RoundToInt(p.y*8)*19349663u;
                h^=h>>13;h*=0x5bd1e995u;h^=h>>15;return (h&0xffff)/65535f;
            }
        }

        public static GameObject Create(string label,Transform parent,IEnumerable<RiftFloorPatch> patches,RiftSurface surface,Material ground,Material edge,
            Material wall=null,uint seed=0,List<WallAnchor> anchors=null)
        {
            var vertices=new List<Vector3>();var uv=new List<Vector2>();var floor=new List<int>();var lips=new List<int>();var walls=new List<int>();
            int Vertex(Vector2 p,float y,Vector2 t){int n=vertices.Count;vertices.Add(new Vector3(p.x,y,p.y));uv.Add(t);return n;}
            void Quad(List<int> into,int v){into.AddRange(new[]{v,v+1,v+2,v,v+2,v+3});}
            foreach(var patch in patches)
            {
                var points=patch.points;int count=points.Length;
                int first=vertices.Count;foreach(var p in points)Vertex(p,0,p*.25f);
                for(int n=1;n<count-1;n++){floor.Add(first);floor.Add(first+n+1);floor.Add(first+n);}
                Vector2 Outside(int n){var d=points[(n+1)%count]-points[n];return new Vector2(d.y,-d.x).normalized;}
                // Mitred offset direction at polygon corner n (its length keeps the offset constant along both edges).
                Vector2 Miter(int n)
                {
                    Vector2 a=Outside((n+count-1)%count),b=Outside(n),m=(a+b).normalized;float c=Vector2.Dot(m,b);
                    return m.sqrMagnitude<.5f||c<.7f?b:m/c;
                }
                float along=0;
                for(int n=0;n<count;n++)
                {
                    var a=points[n];var b=points[(n+1)%count];var delta=b-a;var outside=Outside(n);
                    Vector2 startDir=Miter(n),endDir=Miter((n+1)%count);int steps=Mathf.Max(1,Mathf.CeilToInt(delta.magnitude/.65f));
                    for(int k=0;k<steps;k++)
                    {
                        var from=Vector2.Lerp(a,b,k/(float)steps);var to=Vector2.Lerp(a,b,(k+1f)/steps);float length=Vector2.Distance(from,to);
                        float u0=along,u1=along+length;along=u1;
                        if(surface.Contains((from+to)*.5f+outside*.025f))continue;
                        // A shallow rock lip sits outside the authoritative floor, never across a doorway or an overlapping
                        // bend. The inner edge stays at floor height.
                        Vector2 dirFrom=k==0?startDir:outside,dirTo=k==steps-1?endDir:outside;
                        Vector2 lipFrom=from+dirFrom*LipOffset,lipTo=to+dirTo*LipOffset;
                        const float slope=.57f,drop=1.25f;
                        int v=Vertex(from,0,new Vector2(u0*.5f,0));Vertex(to,0,new Vector2(u1*.5f,0));Vertex(lipTo,LipHeight,new Vector2(u1*.5f,slope*.5f));Vertex(lipFrom,LipHeight,new Vector2(u0*.5f,slope*.5f));
                        Quad(lips,v);
                        v=Vertex(lipFrom,LipHeight,new Vector2(u0*.5f,slope*.5f));Vertex(lipTo,LipHeight,new Vector2(u1*.5f,slope*.5f));Vertex(lipTo,-.8f,new Vector2(u1*.5f,(slope+drop)*.5f));Vertex(lipFrom,-.8f,new Vector2(u0*.5f,(slope+drop)*.5f));
                        Quad(lips,v);
                        if(wall==null)continue;
                        float hFrom=WallHeight(lipFrom,dirFrom.normalized,surface,seed),hTo=WallHeight(lipTo,dirTo.normalized,surface,seed);
                        if(hFrom<=LipHeight+.01f&&hTo<=LipHeight+.01f)continue;
                        // Face toward the room, then a cap .2 m deep so the broken top reads from above.
                        v=Vertex(lipFrom,LipHeight,new Vector2(u0*.25f,LipHeight*.25f));Vertex(lipTo,LipHeight,new Vector2(u1*.25f,LipHeight*.25f));Vertex(lipTo,hTo,new Vector2(u1*.25f,hTo*.25f));Vertex(lipFrom,hFrom,new Vector2(u0*.25f,hFrom*.25f));
                        Quad(walls,v);
                        Vector2 backFrom=lipFrom+dirFrom.normalized*WallDepth,backTo=lipTo+dirTo.normalized*WallDepth;
                        v=Vertex(lipFrom,hFrom,new Vector2(u0*.25f,hFrom*.25f));Vertex(lipTo,hTo,new Vector2(u1*.25f,hTo*.25f));Vertex(backTo,hTo,new Vector2(u1*.25f,(hTo+WallDepth)*.25f));Vertex(backFrom,hFrom,new Vector2(u0*.25f,(hFrom+WallDepth)*.25f));
                        Quad(walls,v);
                        if(anchors!=null&&hFrom>=FullWall)anchors.Add(new WallAnchor(lipFrom,dirFrom.normalized,hFrom,u0));
                    }
                }
            }
            var mesh=new Mesh{name=label};if(vertices.Count>65535)mesh.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.subMeshCount=wall!=null?3:2;mesh.SetTriangles(floor,0);mesh.SetTriangles(lips,1);if(wall!=null)mesh.SetTriangles(walls,2);
            mesh.RecalculateNormals();mesh.RecalculateTangents();mesh.RecalculateBounds();
            var go=new GameObject(label);go.transform.SetParent(parent,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;
            go.AddComponent<MeshRenderer>().sharedMaterials=wall!=null?new[]{ground,edge,wall}:new[]{ground,edge};go.AddComponent<RiftFloorMesh>().ownedMesh=mesh;return go;
        }
    }
}
