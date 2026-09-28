using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    public sealed partial class WorldView
    {
        // A model chest (Props/Chest_*) hinges its lid on Pivot_Lid at the back and opens the other way from the placeholder.
        sealed class ChestView {public GameObject root,seal;public Transform lid;public bool model;public Quaternion rest;}
        static readonly string[] ChestArt={"Props/Chest_Wood","Props/Chest_Iron","Props/Chest_Cursed"};
        readonly Dictionary<string,ChestView> chestViews=new Dictionary<string,ChestView>();
        readonly Dictionary<string,GameObject> shrineViews=new Dictionary<string,GameObject>();
        void BuildGeneratedGeometry(RunState run)
        {
            var layout=run.layout;int field=layout.Field;
            // The field's own floor art when it exists; otherwise the shared rift stone tinted per field.
            Material ground=FieldTile(field,"Floor");
            if(ground==null){ground=FieldGround(field);var paving=Resources.Load<Texture2D>("Art/RiftStone");if(paving!=null)ground.mainTexture=paving;}
            if(layout.rooms.Any(r=>r.outline!=null&&r.outline.Count>=3))BuildOrganicFloor(run,ground,FieldTile(field,"Trim")??darkStone,FieldTile(field,"Wall")??FieldWall(field));
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
                // The field's prop for the kind (WorldView.Props), observed per renderer; the placeholder shape otherwise.
                var prop=ObstacleProp(field,o,parent);
                if(prop!=null)
                {
                    if(o.kind=="Brazier")
                    {
                        var flame=new GameObject("Brazier light");flame.transform.SetParent(prop.transform,false);flame.transform.position=Position(o.position)+Vector3.up*(o.height+.2f);
                        lighting.RegisterEmitter(flame.transform,new Color(1,.55f,.25f),9,8);Fx?.Fire(flame.transform,1,new Color(1,.55f,.25f));
                    }
                    continue;
                }
                var obstacleView=Shape(o.kind,type,parent,Position(o.position)+Vector3.up*o.height*.5f,size,material);
                riftFog?.ObserveObstacle(obstacleView.GetComponent<Renderer>(),o);
                if(o.kind=="Brazier"){var glow=Shape("Brazier light",PrimitiveType.Sphere,parent,Position(o.position)+Vector3.up*(o.height+.25f),new Vector3(.45f,.5f,.45f),ember);riftFog?.ObserveObstacle(glow.GetComponent<Renderer>(),o);lighting.RegisterEmitter(glow.transform,new Color(1,.55f,.25f),9,8);}
            }
            foreach(var chest in layout.chests)
            {
                var root=new GameObject(chest.id);root.transform.SetParent(world.transform,false);root.transform.position=Position(chest.position);
                int grade=chest.definitionId=="CH03"?2:chest.definitionId=="CH02"?1:0;
                artMaterials??=new WorldArtMaterials();var chestModel=WorldArt.Spawn(ChestArt[grade],root.transform,artMaterials,RiftMaterial);
                var hinge=chestModel!=null?FindDeep(chestModel.transform,"Pivot_Lid"):null;
                if(hinge!=null)
                {
                    chestModel.name="Chest body";
                    // The glowing ring on the floor stands in for the seal until the chest opens.
                    var mark=Ring(root.transform,Vector3.up*.03f,.95f,grade==2?red:grade==1?purple:ember,.07f);mark.name="Chest seal";
                    chestViews[chest.id]=new ChestView{root=root,lid=hinge,seal=mark,model=true,rest=hinge.rotation};root.SetActive(false);continue;
                }
                if(chestModel!=null)Destroy(chestModel);
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
            if(decorSurface!=null){ScatterDecor(run,decorSurface);decorSurface=null;}
        }
        RiftSurface decorSurface;
        // Tints the shared rift stone per field until each field's own floor art is wired in.
        Material FieldGround(int field)
        {
            switch(field)
            {
                case 1:return Mat("Fortress Slate",new Color(.85f,.72f,.69f));case 2:return Mat("Desert Sandstone",new Color(1,.84f,.62f));
                case 3:return Mat("Cavern Rock",new Color(.52f,.62f,.64f));case 4:return Mat("Grassland Earth",new Color(.74f,.8f,.6f));
                case 5:return Mat("Highland Frost",new Color(.9f,.96f,1));default:return Mat("Grave Paving",new Color(.7f,.78f,.84f));
            }
        }
        // Fields/F<n>/<tile>_A/_N (Floor 4 m, Trim 2 m, Wall 4 m periods) as a world-art material, or null when the field has no
        // such tile yet. Tiles carry no glow mask and the floor mesh has no colour stream (read as alpha 1), so glow and the
        // characters' rim stay off; the floor gets a faint world-space macro tint against the 4 m repeat.
        Material FieldTile(int field,string tile)
        {
            string path="Fields/F"+field+"/"+tile;
            if(Resources.Load<Texture2D>(WorldArt.Root+path+"_A")==null)return null;
            artMaterials??=new WorldArtMaterials();var m=artMaterials.Get(path);if(m==null)return null;
            m.SetColor("_GlowColor",Color.black);m.SetColor("_RimColor",Color.black);
            if(tile=="Floor")m.SetColor("_DetailTint",new Color(.78f,.76f,.72f,.35f));
            return m;
        }
        // Walls of a field without wall art: its ground tint over the dark rift stone.
        Material FieldWall(int field)=>Mat("Field wall "+field,Color.Lerp(new Color(.07f,.1f,.14f),FieldGround(field).color*.2f,.5f));
        void BuildOrganicFloor(RunState run,Material ground,Material edge,Material wall)
        {
            var surface=new RiftSurface(run.layout);uint seed=run.layout.decorationSeed;int field=run.layout.Field;var anchors=new List<RiftFloorMesh.WallAnchor>();
            decorSurface=surface;
            foreach(var room in run.layout.rooms)
            {
                var root=RoomGeometry(room.index,run.visited.Contains(room.index));anchors.Clear();
                RiftFloorMesh.Create(room.templateId+" organic floor",root,surface.patches.Where(p=>p.room==room.index),surface,ground,edge,wall,seed,anchors);
                DressWalls(root,anchors,field);
                if(room.boss)Ring(root,Position(room.position)+Vector3.up*.04f,7,trim,.06f);
            }
            foreach(var corridor in run.layout.corridors)
            {
                // Keep the existing exploration culling granularity. Revealing one endpoint must
                // not reveal the entire winding passage or its remote room.
                foreach(var chunk in surface.patches.Where(p=>p.corridor==corridor.index).GroupBy(p=>new Vector2Int(Mathf.FloorToInt(p.center.x/4),Mathf.FloorToInt(p.center.y/4))))
                {
                    anchors.Clear();var piece=RiftFloorMesh.Create("Organic passage "+corridor.index,world.transform,chunk,surface,ground,edge,wall,seed,anchors);
                    DressWalls(piece.transform,anchors,field);
                    var center=chunk.Aggregate(Vector2.zero,(sum,p)=>sum+p.center)/chunk.Count();
                    passageGeometry.Add(new PassageView{go=piece,roomA=corridor.roomA,roomB=corridor.roomB,center=center});piece.SetActive(false);
                }
            }
        }
        // The field's wall columns about every 5 m along full-height walls, stretched to the wall's local height, and a torch
        // on every second one with a small fog-aware WorldFx flame and a warm WorldLighting emitter. Remembered like the walls.
        void DressWalls(Transform parent,List<RiftFloorMesh.WallAnchor> anchors,int field)
        {
            string column="Fields/F"+field+"/F"+field+"_WallColumn",torch="Fields/F"+field+"/F"+field+"_Torch";
            bool columns=WorldArt.Has(column),torches=WorldArt.Has(torch);if(!columns&&!torches)return;
            artMaterials??=new WorldArtMaterials();var columnInfo=WorldArt.Info(column);var torchInfo=WorldArt.Info(torch);
            float columnHeight=columnInfo.found&&columnInfo.height>0?columnInfo.height:3.9f;
            Vector2? last=null;int placed=0;
            foreach(var a in anchors)
            {
                if(last.HasValue&&Vector2.Distance(last.Value,a.at)<5)continue;last=a.at;
                var facing=Quaternion.LookRotation(new Vector3(a.outside.x,0,a.outside.y));
                if(columns)
                {
                    var go=WorldArt.Spawn(column,parent,artMaterials);go.name="Wall column";
                    go.transform.SetPositionAndRotation(new Vector3(a.at.x,0,a.at.y),facing);go.transform.localScale=new Vector3(1,(a.height+.15f)/columnHeight,1);
                    terrainProps.AddRange(go.GetComponentsInChildren<MeshRenderer>(true));
                }
                if(torches&&placed++%2==1)
                {
                    var go=WorldArt.Spawn(torch,parent,artMaterials);go.name="Wall torch";
                    go.transform.SetPositionAndRotation(new Vector3(a.at.x,Mathf.Min(2.1f,a.height-.6f),a.at.y)-facing*Vector3.forward*.02f,facing);
                    terrainProps.AddRange(go.GetComponentsInChildren<MeshRenderer>(true));
                    var flame=new GameObject("Torch flame");flame.transform.SetParent(go.transform,false);flame.transform.localPosition=new Vector3(0,.12f,-.42f);
                    var flameColor=torchInfo.found?torchInfo.glow:new Color(1,.62f,.3f);lighting.RegisterEmitter(flame.transform,flameColor,7,5);Fx?.Fire(flame.transform,.4f,flameColor);
                }
            }
        }
        void PresentChests(RunState run,float dt)
        {
            foreach(var c in run.layout.chests)
            {
                if(!chestViews.TryGetValue(c.id,out var v))continue;v.root.SetActive(c.discovered&&RiftVisibility.Get(run,game.Combat.Map).Visible(c.position));
                float angle=c.phase==ChestPhase.Opened?-105:c.phase==ChestPhase.Opening?-8*Mathf.Sin(c.progress*20):0;
                if(v.model)
                {
                    var open=Quaternion.AngleAxis(-angle,Vector3.right)*v.rest;
                    v.lid.rotation=snapPresentation?open:Quaternion.Slerp(v.lid.rotation,open,Mathf.Min(1,dt*14));
                }
                else v.lid.localRotation=snapPresentation?Quaternion.Euler(angle,0,0):Quaternion.Slerp(v.lid.localRotation,Quaternion.Euler(angle,0,0),Mathf.Min(1,dt*14));
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
