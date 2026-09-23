using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    // Small line icons, using the same 24-unit geometry as the HTML reference.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class StorageGlyph : MaskableGraphic
    {
        public string symbol;
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            switch(symbol)
            {
                case "gear":
                    Path(mesh,9,2,15,2,16,5,19,5,22,10,20,12,22,14,19,19,16,19,15,22,9,22,8,19,5,19,2,14,4,12,2,10,5,5,8,5,9,2);
                    Path(mesh,10,8,14,8,16,10,16,14,14,16,10,16,8,14,8,10,10,8);break;
                case "shield":Path(mesh,3,3,12,1,21,3,20,14,17,19,12,23,7,19,4,14,3,3);Path(mesh,12,4,12,19);break;
                case "orb":Gem(mesh);Path(mesh,6,22,18,22);break;
                case "scroll":Path(mesh,4,3,18,3,21,6,19,9,17,7,17,20,4,20,2,17,4,15,7,17,7,3);Path(mesh,10,8,14,8);Path(mesh,10,12,14,12);break;
                case "arrows":Path(mesh,5,21,18,3,19,9);Path(mesh,18,3,12,5);Path(mesh,7,19,2,19,3,14);Path(mesh,10,21,9,16);break;
                case "wand":Path(mesh,4,22,17,5);Path(mesh,14,3,18,1,22,5,20,9,16,8,14,3);break;
                case "blade":Path(mesh,4,22,9,16,5,13,7,11,11,14,20,2,21,10,13,17,15,20,13,22,10,18);break;
                case "vault":
                    Path(mesh,3,9,6,4,18,4,21,9,21,20,3,20,3,9,21,9);
                    Path(mesh,10,9,10,14,14,14,14,9);break;
                case "bag":
                    Path(mesh,5,7,19,7,21,21,3,21,5,7);Path(mesh,8,7,8,4,10,2,14,2,16,4,16,7);break;
                case "transfer":
                    Path(mesh,3,7,20,7,16,3);Path(mesh,20,7,16,11);
                    Path(mesh,21,17,4,17,8,13);Path(mesh,4,17,8,21);break;
                case "arrow":Path(mesh,3,12,21,12,16,7);Path(mesh,21,12,16,17);break;
                case "gem-solid":Gem(mesh);break;
                case "coin":
                    Path(mesh,7,2,17,2,22,7,22,17,17,22,7,22,2,17,2,7,7,2);
                    Path(mesh,14,7,10,7,8,10,8,14,10,17,14,17);break;
                case "gem":
                    Path(mesh,5,4,19,4,23,10,12,23,1,10,5,4);Path(mesh,1,10,23,10);Path(mesh,8,4,6,10,12,23,18,10,16,4);break;
                case "chevron":Path(mesh,6,9,12,15,18,9);break;
                case "check":Path(mesh,5,12,10,17,19,6);break;
                case "lock":
                    Path(mesh,5,10,19,10,19,22,5,22,5,10);Path(mesh,8,10,8,5,10,2,14,2,16,5,16,10);Path(mesh,12,14,12,18);break;
                case "minus":Path(mesh,5,12,19,12);break;
                case "range":
                    Path(mesh,7,3,3,3,3,21,7,21);Path(mesh,17,3,21,3,21,21,17,21);
                    Path(mesh,6,12,18,12);Path(mesh,9,9,6,12,9,15);Path(mesh,15,9,18,12,15,15);break;
                case "info":
                    Path(mesh,6,3,18,3,22,8,22,16,18,21,6,21,2,16,2,8,6,3);Path(mesh,12,11,12,17);Path(mesh,12,6,12,7);break;
                case "history":
                    Path(mesh,5,2,19,2,19,22,5,22,5,2);Path(mesh,8,7,16,7);Path(mesh,8,12,16,12);Path(mesh,8,17,14,17);break;
            }
        }
        void Gem(VertexHelper mesh)
        {
            var points=new[]{new Vector2(12,1),new Vector2(21,8),new Vector2(19,17),new Vector2(12,23),new Vector2(5,17),new Vector2(3,8)};
            float[] lights={1.25f,.85f,.52f,.65f,.95f,1.45f};var r=rectTransform.rect;
            Vector2 Point(Vector2 p)=>new Vector2(r.xMin+p.x*r.width/24,r.yMax-p.y*r.height/24);
            for(int n=0;n<points.Length;n++)
            {
                int i=mesh.currentVertCount;Color c=new Color(Mathf.Min(1,color.r*lights[n]),Mathf.Min(1,color.g*lights[n]),Mathf.Min(1,color.b*lights[n]),color.a);
                mesh.AddVert(Point(new Vector2(12,10)),c,Vector2.zero);mesh.AddVert(Point(points[n]),c,Vector2.zero);mesh.AddVert(Point(points[(n+1)%points.Length]),c,Vector2.zero);mesh.AddTriangle(i,i+1,i+2);
            }
            Path(mesh,12,1,21,8,19,17,12,23,5,17,3,8,12,1);Path(mesh,3,8,12,10,21,8);Path(mesh,12,1,12,10,12,23);
        }
        void Path(VertexHelper mesh,params float[] points)
        {
            var r=rectTransform.rect;float sx=r.width/24,sy=r.height/24;
            for(int n=0;n<points.Length-2;n+=2)
            {
                var a=new Vector2(r.xMin+points[n]*sx,r.yMax-points[n+1]*sy);
                var b=new Vector2(r.xMin+points[n+2]*sx,r.yMax-points[n+3]*sy);
                var normal=new Vector2(-(b-a).y,(b-a).x).normalized*Mathf.Max(.6f,Mathf.Min(sx,sy)*.75f);
                int i=mesh.currentVertCount;
                mesh.AddVert(a-normal,color,Vector2.zero);mesh.AddVert(a+normal,color,Vector2.zero);
                mesh.AddVert(b+normal,color,Vector2.zero);mesh.AddVert(b-normal,color,Vector2.zero);
                mesh.AddTriangle(i,i+1,i+2);mesh.AddTriangle(i,i+2,i+3);
            }
        }
    }
}
