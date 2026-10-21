using System.Collections.Generic;
using System.Linq;
using Hellscript.Runes;
using UnityEngine;
using UnityEngine.UI;
namespace Hellscript
{
    // Batched native uGUI layer. Geometry changes rebuild the mesh; the parent handles camera transforms.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class RuneGemMesh:MaskableGraphic
    {
        public Texture texture;public override Texture mainTexture=>texture??RuneV13Art.Atlas;
        readonly List<UIVertex> vertices=new List<UIVertex>();readonly List<int> triangles=new List<int>();
        public void ResetMesh(){vertices.Clear();triangles.Clear();}
        int Vert(Vector2 point,Color shade,Vector2 uv){var v=UIVertex.simpleVert;v.position=point;v.color=shade;v.uv0=uv;vertices.Add(v);return vertices.Count-1;}
        void Tri(int a,int b,int c){triangles.Add(a);triangles.Add(b);triangles.Add(c);}
        public void Hex(Vector2 center,float radius,Color color,Rect? uv=null)
        {
            Rect box=uv??new Rect(RuneV13Art.White,Vector2.zero);int start=Vert(center,color,box.center);
            for(int i=0;i<6;i++){var d=RuneGemGeometry.Vertex(Vector2.zero,radius,i);Vert(center+d,color,new Vector2(box.x+box.width*(.5f+d.x/(radius*2)),box.y+box.height*(.5f+d.y/(radius*1.7320508f))));}
            for(int i=0;i<6;i++)Tri(start,start+1+i,start+1+(i+1)%6);
        }
        public void Circle(Vector2 center,float radius,Color color,Color? edge=null,int segments=24)
        {int s=Vert(center,color,RuneV13Art.White);for(int i=0;i<segments;i++){float a=i*Mathf.PI*2/segments;Vert(center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius,edge??color,RuneV13Art.White);}for(int i=0;i<segments;i++)Tri(s,s+i+1,s+(i+1)%segments+1);}
        public void Quad(Vector2 center,Vector2 size,Color color,Rect uv)
        {int s=vertices.Count;Vert(center-size/2,color,uv.min);Vert(center+new Vector2(size.x,-size.y)/2,color,new Vector2(uv.xMax,uv.yMin));Vert(center+size/2,color,uv.max);Vert(center+new Vector2(-size.x,size.y)/2,color,new Vector2(uv.xMin,uv.yMax));Tri(s,s+1,s+2);Tri(s,s+2,s+3);}
        public void Line(Vector2 a,Vector2 b,float width,Color color)
        {var n=new Vector2(-(b-a).y,(b-a).x).normalized*width/2;int s=vertices.Count;foreach(var p in new[]{a-n,b-n,b+n,a+n})Vert(p,color,RuneV13Art.White);Tri(s,s+1,s+2);Tri(s,s+2,s+3);}
        public void Outline(Vector2[] points,float width,Color color,bool dashed=false)
        {
            if(points.Length<3||width<=0)return;
            if(dashed)
            {for(int i=0;i<points.Length;i++){var a=points[i];var b=points[(i+1)%points.Length];float length=Vector2.Distance(a,b);for(float d=0;d<length;d+=7.3f)Line(Vector2.Lerp(a,b,d/length),Vector2.Lerp(a,b,Mathf.Min(length,d+4.5f)/length),width,color);}return;}
            // Joined edges share vertices: no open wedges at the hex/region corners.
            int first=vertices.Count;
            for(int i=0;i<points.Length;i++)
            {
                var a=(points[i]-points[(i+points.Length-1)%points.Length]).normalized;
                var b=(points[(i+1)%points.Length]-points[i]).normalized;
                var n=new Vector2(-a.y-b.y,a.x+b.x)*(width*.5f/Mathf.Max(.1f,1+Vector2.Dot(a,b)));
                n=Vector2.ClampMagnitude(n,width*2);
                Vert(points[i]-n,color,RuneV13Art.White);Vert(points[i]+n,color,RuneV13Art.White);
            }
            for(int i=0;i<points.Length;i++){int a=first+i*2,b=first+(i+1)%points.Length*2;Tri(a,b,b+1);Tri(a,b+1,a+1);}
        }
        public void Polygon(Vector2[] points,Color light,Color dark)
        {
            int s=vertices.Count;float lo=points.Min(p=>p.y),hi=points.Max(p=>p.y);foreach(var p in points)Vert(p,Color.Lerp(dark,light,Mathf.InverseLerp(lo,hi,p.y)),RuneV13Art.White);
            var indices=Enumerable.Range(0,points.Length).ToList();int guard=0;
            while(indices.Count>2&&guard++<points.Length*points.Length)
            {bool found=false;for(int i=0;i<indices.Count;i++){int a=indices[(i+indices.Count-1)%indices.Count],b=indices[i],c=indices[(i+1)%indices.Count];if(RuneGemGeometry.Cross(points[b]-points[a],points[c]-points[b])<=.00001f)continue;
                if(indices.Any(k=>k!=a&&k!=b&&k!=c&&RuneGemGeometry.Cross(points[b]-points[a],points[k]-points[a])>-.00001f&&RuneGemGeometry.Cross(points[c]-points[b],points[k]-points[b])>-.00001f&&RuneGemGeometry.Cross(points[a]-points[c],points[k]-points[c])>-.00001f))continue;
                Tri(s+a,s+b,s+c);indices.RemoveAt(i);found=true;break;}if(!found)break;}
        }
        public void Finish()=>SetVerticesDirty();
        protected override void OnPopulateMesh(VertexHelper vh){vh.Clear();vh.AddUIVertexStream(vertices,triangles);}
    }
    public static class RuneGemGeometry
    {
        public const float R=24;
        public static Vector2 Pixel(HexCell c)=>new Vector2(36*c.Q,-41.5692194f*(c.R+c.Q*.5f));
        public static Vector2 Vertex(Vector2 p,float r,int i)=>p+new Vector2(Mathf.Cos(i*Mathf.PI/3),Mathf.Sin(i*Mathf.PI/3))*r;
        public static Vector2[] Hex(Vector2 p,float r)=>Enumerable.Range(0,6).Select(i=>Vertex(p,r,i)).ToArray();
        public static float Cross(Vector2 a,Vector2 b)=>a.x*b.y-a.y*b.x;
        public static List<Vector2[]> Contours(IReadOnlyList<HexCell> cells)
        {
            var edges=new Dictionary<string,(Vector2 a,Vector2 b)>();string Key(Vector2 v)=>Mathf.RoundToInt(v.x*1000)+","+Mathf.RoundToInt(v.y*1000);
            foreach(var c in cells)for(int i=0;i<6;i++){var a=Vertex(Pixel(c),R,i);var b=Vertex(Pixel(c),R,(i+1)%6);string ak=Key(a),bk=Key(b),k=string.CompareOrdinal(ak,bk)<0?ak+"/"+bk:bk+"/"+ak;if(!edges.Remove(k))edges[k]=(a,b);}
            var remaining=edges.Values.ToDictionary(e=>Key(e.a),e=>e);var loops=new List<Vector2[]>();
            while(remaining.Count>0){var e=remaining.First().Value;string start=Key(e.a);var points=new List<Vector2>();for(int i=0;i<cells.Count*6+1;i++){points.Add(e.a);remaining.Remove(Key(e.a));if(Key(e.b)==start||!remaining.TryGetValue(Key(e.b),out e))break;}if(points.Count>=3)loops.Add(points.ToArray());}return loops;
        }
        public static Vector2[] Inset(Vector2[] p,float distance)
        {var result=new Vector2[p.Length];for(int i=0;i<p.Length;i++){var a=(p[i]-p[(i+p.Length-1)%p.Length]).normalized;var b=(p[(i+1)%p.Length]-p[i]).normalized;var n1=new Vector2(-a.y,a.x);var n2=new Vector2(-b.y,b.x);result[i]=p[i]+distance*(n1+n2)/(1+Vector2.Dot(n1,n2));}return result;}
    }
}
