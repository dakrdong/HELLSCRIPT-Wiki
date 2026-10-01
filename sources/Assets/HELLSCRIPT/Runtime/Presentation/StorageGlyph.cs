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
                case "play":Path(mesh,7,3,21,12,7,21,7,3);break;
                case "pause":Path(mesh,7,3,7,21);Path(mesh,17,3,17,21);break;
                case "hint-up":
                    var tip=GetPixelAdjustedRect();
                    mesh.AddVert(new Vector3(tip.xMin,tip.yMin),color,Vector2.zero);
                    mesh.AddVert(new Vector3(tip.center.x,tip.yMax),color,Vector2.zero);
                    mesh.AddVert(new Vector3(tip.xMax,tip.yMin),color,Vector2.zero);
                    mesh.AddTriangle(0,1,2);break;
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
                // Daily quest activities, traced from the approved 24-unit HTML paths.
                case "daily-sell":Path(mesh,5,19,13,11);Path(mesh,4,16,8,20);Path(mesh,14,4,20,3,19,9,13,15,9,11,14,4);Path(mesh,3,21,6,18);Path(mesh,16,17,21,17);Path(mesh,18.5f,14.5f,18.5f,20);break;
                case "daily-rift":Path(mesh,5,21,4,12,8,4,12,2,16,4,20,12,19,21);Path(mesh,8,21,8,11,12,5,16,11,16,21);Path(mesh,3,21,21,21);Path(mesh,12,13,10,16,12,19,14,16,12,13);break;
                case "daily-enhance":Path(mesh,3,13,21,13,17,17,14,17,14,21,8,21,8,17,6,17,3,13);Path(mesh,13,3,17,7,14,10,10,6,13,3);Path(mesh,12,8,8,12);Path(mesh,18,2,18,5);Path(mesh,20,4,17,4);break;
                case "daily-shop":Path(mesh,4,10,4,21,20,21,20,10);Path(mesh,3,10,5,4,19,4,21,10);Path(mesh,3,10,3.6f,11.8f,5,12.2f,6.4f,11.8f,7,10,7.7f,11.8f,9.5f,12.2f,11.3f,11.8f,12,10,12.7f,11.8f,14.5f,12.2f,16.3f,11.8f,17,10,17.6f,11.8f,19,12.2f,20.4f,11.8f,21,10);Path(mesh,9,21,9,15,15,15,15,21);break;
                case "daily-potion":Path(mesh,9,3,15,3,15,6,9,6,9,3);Path(mesh,10,6,10,10,5,17,5.1f,19,6.2f,20.5f,8,21,16,21,17.8f,20.5f,18.9f,19,19,17,14,10,14,6);Path(mesh,7,16,17,16);Path(mesh,10,12,14,12);break;
                // Rift result icons, traced from the RiftVictory HTML reference's 24-unit paths.
                case "sword":Path(mesh,4,20,8,16);Path(mesh,4,13,11,20);Path(mesh,8,16,18,3,21,3,21,6,11,19);Path(mesh,8,16,11,19);break;
                case "right":Path(mesh,9,5,16,12,9,19);break;
                case "close":Path(mesh,6,6,18,18);Path(mesh,18,6,6,18);break;
                case "seal":Path(mesh,12,2,20,7,20,17,12,22,4,17,4,7,12,2);Path(mesh,8,12,11,15,16,9);break;
                case "bars":Path(mesh,4,20,20,20);Path(mesh,6,16,6,9);Path(mesh,12,16,12,4);Path(mesh,18,16,18,12);break;
                case "book":Path(mesh,12,5,10,3.8f,8.1f,3.2f,6.4f,3,4.9f,3.1f,3.7f,3.5f,3,4,3,19,4.5f,18.6f,6,18.4f,7.5f,18.4f,9,18.6f,10.5f,19.2f,12,20,13.5f,19.2f,15,18.6f,16.5f,18.4f,18,18.4f,19.5f,18.6f,21,19,21,4,20.3f,3.5f,19.1f,3.1f,17.6f,3,15.9f,3.2f,14,3.8f,12,5);Path(mesh,12,5,12,20);break;
                case "log":Path(mesh,4,4,20,4,20,21,4,21,4,4);Path(mesh,8,8,16,8);Path(mesh,8,12,16,12);Path(mesh,8,16,13,16);break;
                case "compass":Path(mesh,12,2,8.2f,2.8f,4.9f,4.9f,2.8f,8.2f,2,12,2.8f,15.8f,4.9f,19.1f,8.2f,21.2f,12,22,15.8f,21.2f,19.1f,19.1f,21.2f,15.8f,22,12,21.2f,8.2f,19.1f,4.9f,15.8f,2.8f,12,2);Path(mesh,17,7,14,14,7,17,10,10,17,7);break;
                case "compare":Path(mesh,4,7,19,7);Path(mesh,15,3,19,7,15,11);Path(mesh,20,17,5,17);Path(mesh,9,13,5,17,9,21);break;
                case "repeat":Path(mesh,20,8,18.4f,4.9f,15.6f,2.7f,12.2f,2,8.7f,2.8f,6,5,3,8);Path(mesh,3,3,3,8,8,8);Path(mesh,4,16,5.6f,19.1f,8.4f,21.3f,11.8f,22,15.3f,21.2f,18,19,21,16);Path(mesh,21,21,21,16,16,16);break;
                case "home":Path(mesh,3,11,12,2,21,11);Path(mesh,5,9,5,21,10,21,10,14,14,14,14,21,19,21,19,9);break;
                case "gift":Path(mesh,3,8,21,8,21,12,3,12,3,8);Path(mesh,5,12,5,21,19,21,19,12);Path(mesh,12,8,12,21);Path(mesh,12,8,7.7f,7,5.6f,5.3f,5.2f,3.9f,6.4f,3.3f,8.8f,4.5f,12,8);Path(mesh,12,8,16.3f,7,18.4f,5.3f,18.8f,3.9f,17.6f,3.3f,15.2f,4.5f,12,8);break;
                case "clock":Path(mesh,12,2,8.2f,2.8f,4.9f,4.9f,2.8f,8.2f,2,12,2.8f,15.8f,4.9f,19.1f,8.2f,21.2f,12,22,15.8f,21.2f,19.1f,19.1f,21.2f,15.8f,22,12,21.2f,8.2f,19.1f,4.9f,15.8f,2.8f,12,2);Path(mesh,12,6,12,12,16,14);break;
                case "crown":Path(mesh,3,6,7,11,12,4,17,11,21,6,19,19,5,19,3,6);Path(mesh,5,16,19,16);break;
                case "stop":Path(mesh,6,6,18,6,18,18,6,18,6,6);break;
                case "chest":Path(mesh,3,10,21,10,21,21,3,21,3,10);Path(mesh,3,10,3,6,4.3f,4.8f,7.7f,4,12,3.8f,16.3f,4,19.7f,4.8f,21,6,21,10);Path(mesh,3,10,21,10);Path(mesh,10,10,10,15,14,15,14,10);Path(mesh,6,4,6,21);Path(mesh,18,4,18,21);break;
                // Forge window icons, from the 24-unit paths of the forge HTML prototype.
                case "back":Path(mesh,14,5,7,12,14,19);Path(mesh,7,12,21,12);break;
                case "enhance":Path(mesh,12,3,19,11,14.5f,11,14.5f,21,9.5f,21,9.5f,11,5,11,12,3);break;
                case "craft":Path(mesh,3,8,21,8,18,13,13,13,13,17,17,17,17,20,6,20,6,17,9,17,9,13,6,13,3,8);Path(mesh,13,3,15,2,19,6,17,8,13,3);break;
                case "spark":Path(mesh,12,2,15,9,22,12,15,15,12,22,9,15,2,12,9,9,12,2);break;
                case "plus":Path(mesh,12,5,12,19);Path(mesh,5,12,19,12);break;
                case "auto":Path(mesh,5,4,13,12,5,20,5,4);Path(mesh,14,4,22,12,14,20,14,4);break;
                case "list":Path(mesh,8,6,21,6);Path(mesh,8,12,21,12);Path(mesh,8,18,21,18);Path(mesh,3,6,4,6);Path(mesh,3,12,4,12);Path(mesh,3,18,4,18);break;
                case "reroll":Path(mesh,19,7,17.5f,4.5f,15,3,12,3,8.5f,3.6f,6,6,3,9);Path(mesh,3,4,3,9,8,9);Path(mesh,5,17,6.5f,19.5f,9,21,12,21,15.5f,20.4f,18,18,21,15);Path(mesh,21,20,21,15,16,15);break;
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
