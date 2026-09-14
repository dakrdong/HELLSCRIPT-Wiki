using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    public sealed partial class WorldView
    {
        sealed class ChestView {public GameObject root,seal;public Transform lid;}
        readonly Dictionary<string,ChestView> chestViews=new Dictionary<string,ChestView>();
        readonly Dictionary<string,GameObject> shrineViews=new Dictionary<string,GameObject>();
        void BuildGeneratedGeometry(RunState run)
        {
            var layout=run.layout;Material ground=run.theme==0?Mat("Grave Paving",new Color(.7f,.78f,.84f)):Mat("Fortress Slate",new Color(.85f,.72f,.69f));
            var paving=Resources.Load<Texture2D>("Art/RiftStone");if(paving!=null)ground.mainTexture=paving;
            if(layout.rooms.Any(r=>r.outline!=null&&r.outline.Count>=3))BuildOrganicFloor(run,ground);
            else
            {
            foreach(var corridor in layout.corridors)for(int n=1;n<corridor.points.Count;n++)
            {
                Vector2 a=corridor.points[n-1],b=corridor.points[n];int pieces=Mathf.Max(1,Mathf.CeilToInt(Vector2.Distance(a,b)/4));
                for(int part=0;part<pieces;part++)
                {
                    Vector2 from=Vector2.Lerp(a,b,part/(float)pieces),to=Vector2.Lerp(a,b,(part+1f)/pieces),middle=(from+to)*.5f;
                    Vector2 size=new Vector2(Mathf.Abs(from.x-to.x),Mathf.Abs(from.y-to.y))+Vector2.one*corridor.width;
                    var piece=Shape("Passage "+corridor.index+" part "+part,PrimitiveType.Cube,world.transform,Position(middle)-Vector3.up*.25f,new Vector3(size.x,.5f,size.y),darkStone);
                    passageGeometry.Add(new PassageView{go=piece,roomA=corridor.roomA,roomB=corridor.roomB,center=middle});piece.SetActive(false);
                }
            }
            foreach(var room in layout.rooms)
            {
                var roomRoot=RoomGeometry(room.index,run.visited.Contains(room.index));
                Shape(room.templateId+" floor",PrimitiveType.Cube,roomRoot,Position(room.position)-Vector3.up*.3f,new Vector3(room.size.x,.6f,room.size.y),ground);
                for(float x=room.Bounds.xMin+2;x<room.Bounds.xMax;x+=4)for(float y=room.Bounds.yMin+2;y<room.Bounds.yMax;y+=4)
                {Shape("Stone inlay",PrimitiveType.Cube,roomRoot,new Vector3(x,.01f,y),new Vector3(3.99f,.025f,3.99f),ground);}
                foreach(var normal in new[]{Vector2.left,Vector2.right,Vector2.up,Vector2.down})
                {
                    bool vertical=normal.x!=0;float half=(vertical?room.size.y:room.size.x)*.5f;
                    var gaps=room.doors.Where(d=>d.corridor>=0&&d.direction==normal).Select(d=>new Vector2((vertical?d.position.y-room.position.y:d.position.x-room.position.x)-d.width*.5f,(vertical?d.position.y-room.position.y:d.position.x-room.position.x)+d.width*.5f)).OrderBy(g=>g.x).ToList();
                    gaps.Add(new Vector2(half,half));float start=-half;
                    foreach(var gap in gaps)
                    {
                        float end=Mathf.Clamp(gap.x,-half,half);if(end-start>.01f)
                        {
                            float middle=(start+end)*.5f;Vector2 p=room.position+normal*((vertical?room.size.x:room.size.y)*.5f+.18f)+(vertical?Vector2.up:Vector2.right)*middle;
                            Shape("Room boundary",PrimitiveType.Cube,roomRoot,Position(p)+Vector3.up*.5f,vertical?new Vector3(.35f,1,end-start):new Vector3(end-start,1,.35f),darkStone);
                        }
                        start=gap.y;
                    }
                }
                foreach(var door in room.doors.Where(d=>d.corridor>=0))
                {
                    var along=new Vector2(-door.direction.y,door.direction.x);
                    foreach(int sign in new[]{-1,1})
                    {Vector2 p=door.position+along*sign*(door.width*.5f+.55f);Shape("Door marker",PrimitiveType.Cube,roomRoot,Position(p)+Vector3.up*.3f,new Vector3(.5f,.6f,.5f),trim);}
                }
                if(room.boss)Ring(roomRoot,Position(room.position)+Vector3.up*.04f,7,trim,.06f);
            }
            }
            foreach(var o in layout.obstacles)
            {
                if(o.kind=="Chest"||o.kind=="Shrine"||o.kind=="OfferingAltar")continue;
                Material material=o.kind=="Brazier"?trim:o.kind=="Altar"?Mat("Drowned altar",new Color(.13f,.24f,.27f)):darkStone;
                var type=o.radius>0?PrimitiveType.Cylinder:PrimitiveType.Cube;
                Vector3 size=o.radius>0?new Vector3(o.radius*2,o.height*.5f,o.radius*2):new Vector3(o.halfSize.x*2,o.height,o.halfSize.y*2);
                var owner=layout.rooms.FirstOrDefault(room=>room.Bounds.Contains(o.position));
                var parent=owner!=null&&roomGeometry.TryGetValue(owner.index,out var group)?group.transform:world.transform;
                var obstacleView=Shape(o.kind,type,parent,Position(o.position)+Vector3.up*o.height*.5f,size,material);
                riftFog?.ObserveObstacle(obstacleView.GetComponent<Renderer>(),o);
                if(o.kind=="Brazier"){var glow=Shape("Brazier light",PrimitiveType.Sphere,parent,Position(o.position)+Vector3.up*(o.height+.25f),new Vector3(.45f,.5f,.45f),ember);riftFog?.ObserveObstacle(glow.GetComponent<Renderer>(),o);}
            }
            foreach(var chest in layout.chests)
            {
                var root=new GameObject(chest.id);root.transform.SetParent(world.transform,false);root.transform.position=Position(chest.position);
                Shape("Chest body",PrimitiveType.Cube,root.transform,Vector3.up*.4f,new Vector3(1.2f,.8f,.85f),chest.definitionId=="CH02"?darkStone:Mat("Chest wood",new Color(.24f,.12f,.075f)));
                var pivot=new GameObject("Lid pivot");pivot.transform.SetParent(root.transform,false);pivot.transform.localPosition=new Vector3(0,.8f,-.425f);
                Shape("Chest lid",PrimitiveType.Cube,pivot.transform,new Vector3(0,.09f,.425f),new Vector3(1.3f,.18f,.95f),trim);
                var seal=Shape("Chest seal",PrimitiveType.Cube,root.transform,new Vector3(0,.55f,.46f),new Vector3(.25f,.4f,.08f),chest.definitionId=="CH03"?red:chest.definitionId=="CH02"?purple:ember);
                if(chest.definitionId=="CH03")Ring(root.transform,Vector3.up*.03f,1,red,.08f);
                chestViews[chest.id]=new ChestView{root=root,lid=pivot.transform,seal=seal};root.SetActive(false);
            }
            foreach(var shrine in layout.shrines)
            {
                var root=new GameObject(shrine.id);root.transform.SetParent(world.transform,false);root.transform.position=Position(shrine.position);
                Shape("Shrine plinth",PrimitiveType.Cube,root.transform,Vector3.up*.2f,new Vector3(1,.4f,1),darkStone);
                Shape("Shrine monolith",PrimitiveType.Cube,root.transform,Vector3.up, new Vector3(.55f,1.6f,.55f),trim);
                var light=Shape("Blessing light",PrimitiveType.Sphere,root.transform,Vector3.up*1.65f,Vector3.one*.45f,shrine.definitionId=="SH01"?blue:ember);
                Ring(root.transform,Vector3.up*.04f,.85f,shrine.definitionId=="SH01"?blue:ember,.07f);
                shrineViews[shrine.id]=root;root.SetActive(false);
            }
        }
        void BuildOrganicFloor(RunState run,Material ground)
        {
            var surface=new RiftSurface(run.layout);
            foreach(var room in run.layout.rooms)
            {
                var root=RoomGeometry(room.index,run.visited.Contains(room.index));
                RiftFloorMesh.Create(room.templateId+" organic floor",root,surface.patches.Where(p=>p.room==room.index),surface,ground,darkStone);
                if(room.boss)Ring(root,Position(room.position)+Vector3.up*.04f,7,trim,.06f);
            }
            foreach(var corridor in run.layout.corridors)
            {
                // Keep the existing exploration culling granularity. Revealing one endpoint must
                // not reveal the entire winding passage or its remote room.
                foreach(var chunk in surface.patches.Where(p=>p.corridor==corridor.index).GroupBy(p=>new Vector2Int(Mathf.FloorToInt(p.center.x/4),Mathf.FloorToInt(p.center.y/4))))
                {
                    var piece=RiftFloorMesh.Create("Organic passage "+corridor.index,world.transform,chunk,surface,ground,darkStone);
                    var center=chunk.Aggregate(Vector2.zero,(sum,p)=>sum+p.center)/chunk.Count();
                    passageGeometry.Add(new PassageView{go=piece,roomA=corridor.roomA,roomB=corridor.roomB,center=center});piece.SetActive(false);
                }
            }
        }
        void PresentChests(RunState run,float dt)
        {
            foreach(var c in run.layout.chests)
            {
                if(!chestViews.TryGetValue(c.id,out var v))continue;v.root.SetActive(c.discovered&&RiftVisibility.Get(run,game.Combat.Map).Visible(c.position));
                float angle=c.phase==ChestPhase.Opened?-105:c.phase==ChestPhase.Opening?-8*Mathf.Sin(c.progress*20):0;
                v.lid.localRotation=snapPresentation?Quaternion.Euler(angle,0,0):Quaternion.Slerp(v.lid.localRotation,Quaternion.Euler(angle,0,0),Mathf.Min(1,dt*14));
                v.seal.SetActive(c.phase!=ChestPhase.Opened&&c.phase!=ChestPhase.Exhausted);
            }
            foreach(var s in run.layout.shrines)
            {
                if(!shrineViews.TryGetValue(s.id,out var root))continue;root.SetActive(s.discovered&&RiftVisibility.Get(run,game.Combat.Map).Visible(s.position));
                root.transform.Find("Blessing light").gameObject.SetActive(s.phase!=ShrinePhase.Used&&s.phase!=ShrinePhase.Exhausted);
            }
        }
    }
}
