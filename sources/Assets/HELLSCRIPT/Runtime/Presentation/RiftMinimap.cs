using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    public sealed class RiftMinimap : MaskableGraphic
    {
        public RunState run;
        public bool expanded;
        Vector2 center;float scale;
        Vector2 Point(Vector2 p)=>(p-center)*scale;
        void Rect(VertexHelper v,Vector2 p,Vector2 size,Color color)
        {
            int n=v.currentVertCount;v.AddVert(p-size*.5f,color,Vector2.zero);v.AddVert(p+new Vector2(-size.x,size.y)*.5f,color,Vector2.zero);v.AddVert(p+size*.5f,color,Vector2.zero);v.AddVert(p+new Vector2(size.x,-size.y)*.5f,color,Vector2.zero);v.AddTriangle(n,n+1,n+2);v.AddTriangle(n,n+2,n+3);
        }
        void Line(VertexHelper v,Vector2 a,Vector2 b,float width,Color c)
        {
            Vector2 side=new Vector2(-(b-a).y,(b-a).x).normalized*width*.5f;int n=v.currentVertCount;
            v.AddVert(a-side,c,Vector2.zero);v.AddVert(a+side,c,Vector2.zero);v.AddVert(b+side,c,Vector2.zero);v.AddVert(b-side,c,Vector2.zero);v.AddTriangle(n,n+1,n+2);v.AddTriangle(n,n+2,n+3);
        }
        protected override void OnPopulateMesh(VertexHelper v)
        {
            v.Clear();if(run?.layout==null)return;var r=rectTransform.rect;center=expanded?Vector2.zero:run.position;
            scale=Mathf.Min(r.width,r.height)/(expanded?155:55);
            Color floor=new Color(.25f,.29f,.34f),gold=new Color(.96f,.67f,.3f),muted=new Color(.4f,.45f,.5f);
            foreach(var c in run.layout.corridors)
            {
                bool a=run.visited.Contains(c.roomA),b=run.visited.Contains(c.roomB);if(!a&&!b)continue;
                if(a&&b||run.phase==RunPhase.Boss&&expanded)
                {for(int n=1;n<c.points.Count;n++)Line(v,Point(c.points[n-1]),Point(c.points[n]),Mathf.Max(2,c.width*scale),floor);}
                else
                {
                    var points=c.points;Vector2 p=a?points[0]:points[points.Count-1],next=a?points[1]:points[points.Count-2];
                    Line(v,Point(p),Point(Vector2.MoveTowards(p,next,5)),Mathf.Max(2,c.width*scale),floor);
                }
            }
            foreach(var room in run.layout.rooms)if(run.visited.Contains(room.index))
            {
                Rect(v,Point(room.position),room.size*scale,floor);
                foreach(var o in run.layout.obstacles.Where(o=>room.Bounds.Contains(o.position)))Rect(v,Point(o.position),(o.radius>0?Vector2.one*o.radius*2:o.halfSize*2)*scale,new Color(.08f,.1f,.13f));
            }
            foreach(var c in run.layout.chests.Where(c=>c.discovered))
            {
                Vector2 p=Point(c.position);float size=expanded?7:8;
                Rect(v,p,Vector2.one*size,c.phase==ChestPhase.Opened||c.abandoned?muted:c.phase==ChestPhase.Locked?new Color(.62f,.42f,.86f):gold);
                if(c.phase==ChestPhase.Locked)Rect(v,p+Vector2.up*size*.6f,new Vector2(size*.6f,size*.5f),muted);
                if(c.phase==ChestPhase.Opened)Line(v,p-new Vector2(size*.4f,0),p+new Vector2(size*.4f,size*.4f),2,Color.white);
                if(c.definitionId=="CH03")Line(v,p-Vector2.one*size*.5f,p+Vector2.one*size*.5f,2,new Color(1,.25f,.2f));
            }
            foreach(var seen in run.exploration.enemies.Where(e=>e.elite>=0&&!e.investigated))
            {
                Vector2 p=Point(seen.position);Color color=run.time-seen.seenAt>.3f?muted:new Color(.8f,.48f,1);float size=6;
                Line(v,p+Vector2.up*size,p+Vector2.right*size,2,color);Line(v,p+Vector2.right*size,p+Vector2.down*size,2,color);
                Line(v,p+Vector2.down*size,p+Vector2.left*size,2,color);Line(v,p+Vector2.left*size,p+Vector2.up*size,2,color);
            }
            foreach(var shrine in run.layout.shrines.Where(s=>s.discovered))
            {
                Vector2 p=Point(shrine.position);Color color=shrine.phase==ShrinePhase.Used||shrine.phase==ShrinePhase.Exhausted?muted:shrine.definitionId=="SH01"?new Color(.3f,.8f,1):gold;
                Line(v,p-Vector2.up*6,p+Vector2.up*6,3,color);Line(v,p-Vector2.right*5,p+Vector2.right*5,3,color);
            }
            foreach(var seal in run.layout.seals.Where(s=>s.discovered&&run.visited.Contains(s.room)))
            {
                Vector2 p=Point(seal.position);float size=expanded?8:7;
                Color color=seal.phase==SealPhase.Broken?new Color(.3f,.9f,.55f):seal.phase==SealPhase.Locked?new Color(.62f,.42f,.86f):gold;
                for(int i=0;i<6;i++)
                {
                    float a=i*Mathf.PI/3,b=(i+1)*Mathf.PI/3;
                    Line(v,p+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*size,p+new Vector2(Mathf.Cos(b),Mathf.Sin(b))*size,2,color);
                }
                if(seal.phase==SealPhase.Locked)Line(v,p-Vector2.up*4,p+Vector2.up*4,2,color);
                if(seal.phase==SealPhase.Broken)
                {Line(v,p+new Vector2(-4,0),p+new Vector2(-1,-3),2,color);Line(v,p+new Vector2(-1,-3),p+new Vector2(4,3),2,color);}
            }
            foreach(var carrier in run.layout.carriers.Where(c=>c.discovered))
            {
                Vector2 p=Point(carrier.position);Color color=carrier.completed?new Color(.3f,.9f,.55f):new Color(.8f,.48f,1);
                foreach(float size in new[]{4f,8f})
                {Line(v,p+Vector2.up*size,p+Vector2.right*size,2,color);Line(v,p+Vector2.right*size,p+Vector2.down*size,2,color);Line(v,p+Vector2.down*size,p+Vector2.left*size,2,color);Line(v,p+Vector2.left*size,p+Vector2.up*size,2,color);}
            }
            foreach(var offering in run.layout.offerings.Where(o=>o.discovered))
            {
                Vector2 p=Point(offering.position);Color color=offering.collected?new Color(.3f,.9f,.55f):offering.available?gold:new Color(.62f,.42f,.86f);
                Line(v,p+Vector2.up*7,p+new Vector2(-6,-5),2,color);Line(v,p+new Vector2(-6,-5),p+new Vector2(6,-5),2,color);Line(v,p+new Vector2(6,-5),p+Vector2.up*7,2,color);
            }
            var altar=run.layout.altar;if(altar!=null&&altar.discovered)
            {
                Vector2 p=Point(altar.position);Color color=altar.phase==OfferingAltarPhase.Offered?new Color(.3f,.9f,.55f):gold;
                Line(v,p+new Vector2(-8,4),p+new Vector2(8,4),3,color);Line(v,p+new Vector2(-5,-5),p+new Vector2(-5,4),3,color);Line(v,p+new Vector2(5,-5),p+new Vector2(5,4),3,color);
            }
            foreach(var gate in run.layout.gates.Where(g=>g.discovered))
            {
                Vector2 p=Point(gate.barrier.position),side=new Vector2(-gate.outward.y,gate.outward.x);
                Color color=run.layout.gateOpen?new Color(.3f,.9f,.55f):new Color(.62f,.42f,.86f);
                Line(v,p-side*7,p-side*4,4,color);Line(v,p+side*4,p+side*7,4,color);
                if(!run.layout.gateOpen)Line(v,p-side*4,p+side*4,3,color);
            }
            if(run.phase==RunPhase.Boss)
            {
                var boss=run.enemies.Find(e=>e.id==run.bossId);Vector2 p=Point(boss!=null?boss.position:run.layout.rooms[run.layout.bossRoom].position);
                if(!expanded)p=Vector2.ClampMagnitude(p,Mathf.Min(r.width,r.height)*.42f);
                Line(v,p-new Vector2(6,6),p+new Vector2(6,6),3,new Color(1,.2f,.2f));Line(v,p+new Vector2(-6,6),p+new Vector2(6,-6),3,new Color(1,.2f,.2f));
            }
            Vector2 hero=Point(run.position);int index=v.currentVertCount;v.AddVert(hero+new Vector2(0,7),Color.white,Vector2.zero);v.AddVert(hero+new Vector2(-5,-4),Color.white,Vector2.zero);v.AddVert(hero+new Vector2(5,-4),Color.white,Vector2.zero);v.AddTriangle(index,index+1,index+2);
        }
    }
}
