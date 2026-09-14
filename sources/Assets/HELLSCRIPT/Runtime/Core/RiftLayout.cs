using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    public enum ChestPhase { Locked, Available, Approaching, Opening, CommitPending, Opened, Exhausted }
    public enum ExplorationPolicy { LeftWall, RightWall, NearestUnexplored, FollowEnemies }
    public enum ShrinePhase { Available, Approaching, Invoking, Used, Exhausted }
    public enum RiftEventPhase { Dormant, Active, Succeeded, Failed, Cancelled }

    [Serializable]public sealed class RiftShrine
    {
        public string id,definitionId,requestId;
        public int room;
        public Vector2 position,openingPosition;
        public List<Vector2> accessPoints=new List<Vector2>();
        public ShrinePhase phase;
        public bool discovered;
        public float progress,retryAfter;
    }
    [Serializable]public sealed class RiftEvent
    {
        public string id,chestId,requestId;
        public RiftEventPhase phase;
        public float startedAt,remaining=12;
        public List<Vector2> spawnCandidates=new List<Vector2>();
        public List<int> enemyIds=new List<int>();
    }

    [Serializable]public sealed class RiftObstacle
    {
        public string id, kind;
        public Vector2 position, halfSize;
        public float radius, height;
        public bool blocksWalk=true, blocksSight=true, blocksProjectile=true, blocksLanding=true;
        public bool Contains(Vector2 p,float margin=0)
        {
            if(radius>0)return (p-position).sqrMagnitude<(radius+margin)*(radius+margin);
            Vector2 d=p-position;return Mathf.Abs(d.x)<halfSize.x+margin&&Mathf.Abs(d.y)<halfSize.y+margin;
        }
    }
    [Serializable]public sealed class RiftDoor
    {
        public int index, corridor=-1;
        public Vector2 position, direction;
        public float width=4;
    }
    // A room's part in the run. Geometry comes from the template; this says what the room is for.
    public enum RiftRoomRole { Entrance, Crossroad, Passage, Wing, Antechamber, BossArena }

    public enum RiftObjectiveKind { None, Seals, EssenceCarriers, Offerings }
    public enum SealPhase { Locked, Available, Approaching, Breaking, Broken }

    // The seal a run must break before the boss gate opens. Progress survives an interruption on
    // purpose: a hero driven off by a pack resumes where it stopped rather than starting over.
    [Serializable]public sealed class RiftSeal
    {
        public const float Duration=1.5f;
        public string id;
        public int room,guardGroup=-1;
        public Vector2 position,breakingPosition;
        public List<Vector2> accessPoints=new List<Vector2>();
        public SealPhase phase;
        public float progress,retryAfter;
        public bool discovered;
    }

    [Serializable]public sealed class RiftRoom
    {
        public int index, rotation, variant;
        public string templateId;
        public Vector2 position, size;
        public bool boss,central;
        public RiftRoomRole role;
        public List<RiftDoor> doors=new List<RiftDoor>();
        public List<Vector2> groupAnchors=new List<Vector2>(), chestAnchors=new List<Vector2>();
        // Version 5 saves the actual floor silhouette. Empty keeps older rectangular maps intact.
        public List<Vector2> outline=new List<Vector2>();
        public Vector2 Transform(Vector2 p)=>position+Rotate(p,rotation);
        public static Vector2 Rotate(Vector2 p,int quarter)=>quarter%4==0?p:quarter%4==1?new Vector2(-p.y,p.x):quarter%4==2?-p:new Vector2(p.y,-p.x);
        public Rect Bounds=>new Rect(position-size*.5f,size);
    }
    [Serializable]public sealed class RiftCorridor
    {
        public int index, roomA,roomB,doorA,doorB;
        public float width=4;
        public bool extra,crossing;
        public List<Vector2> points=new List<Vector2>();
        public List<float> widths=new List<float>();
    }
    [Serializable]public sealed class RiftJunction
    {
        public Vector2 position;
        public List<int> corridors=new List<int>();
    }
    [Serializable]public sealed class RiftSpawn
    {
        public int index,group,room,kind,elite=-1,elitePartner=-1;
        public Vector2 position;
        public List<int> traits=new List<int>();
    }
    [Serializable]public sealed class RiftGroup
    {
        public int index,room;
        public string archetype;
        public Vector2 position;
        public List<int> spawns=new List<int>();
    }
    [Serializable]public sealed class RiftChest
    {
        public string id,definitionId,requestId;
        public int room,guardGroup=-1,gold,materials,dropId=-1;
        public Vector2 position,openingPosition;
        public List<Vector2> accessPoints=new List<Vector2>();
        public Item reward;
        public bool discovered,abandoned;
        public ChestPhase phase;
        public float progress,retryAfter;
        public int Gate=>definitionId=="CH01"?30:70;
        public float Duration=>definitionId=="CH01"?1:1.5f;
    }
    [Serializable]public sealed class RiftLayout
    {
        public RiftObjectiveKind objective;
        public List<RiftSeal> seals=new List<RiftSeal>();
        public List<RiftGate> gates=new List<RiftGate>();
        public List<RiftEssenceCarrier> carriers=new List<RiftEssenceCarrier>();
        public List<RiftOffering> offerings=new List<RiftOffering>();
        public RiftOfferingAltar altar;
        public RiftObjectiveRecord objectiveRecord;
        public bool gateOpen,gateOpenedByMeter;
        public const int CurrentVersion=6;
        public int contentStage;
        public int version=CurrentVersion,theme,bossRoom,bossKind,candidate;
        public uint mapSeed,layoutSeed,decorationSeed,encounterSeed,combatSeed,rewardSeed;
        public string fingerprint, fallbackId="";
        public bool legacy,roamingBoss;
        public Vector2 start;
        public List<RiftRoom> rooms=new List<RiftRoom>();
        public List<RiftCorridor> corridors=new List<RiftCorridor>();
        public List<RiftObstacle> obstacles=new List<RiftObstacle>();
        public List<RiftJunction> junctions=new List<RiftJunction>();
        public List<RiftGroup> groups=new List<RiftGroup>();
        public List<RiftSpawn> spawns=new List<RiftSpawn>();
        public List<RiftChest> chests=new List<RiftChest>();
        public List<RiftShrine> shrines=new List<RiftShrine>();
        public List<RiftEvent> events=new List<RiftEvent>();
        public List<Vector2> bossPoints=new List<Vector2>();
        public static RiftLayout Legacy(int theme)
        {
            var layout=new RiftLayout{version=1,legacy=true,theme=theme,bossRoom=4,start=RiftMap.Rooms[0]+new Vector2(0,-4),fingerprint="legacy-ring-v1"};
            for(int n=0;n<8;n++)layout.rooms.Add(new RiftRoom{index=n,templateId="LEGACY",position=RiftMap.Rooms[n],size=new Vector2(15,15),boss=n==4});
            for(int n=0;n<8;n++)layout.corridors.Add(new RiftCorridor{index=n,roomA=n,roomB=(n+1)%8,width=5,points=new List<Vector2>{RiftMap.Rooms[n],RiftMap.Rooms[(n+1)%8]}});
            layout.bossPoints.Add(RiftMap.Rooms[4]);return layout;
        }
    }
    public sealed class RoomTemplate
    {
        public readonly string id,name;
        public readonly Vector2 size;
        public readonly Vector2[] doors,groups,chests;
        public readonly bool boss;
        public RoomTemplate(string id,string name,float width,float depth,bool boss,Vector2[] doors,Vector2[] groups,Vector2[] chests)
        {this.id=id;this.name=name;size=new Vector2(width,depth);this.boss=boss;this.doors=doors;this.groups=groups;this.chests=chests;}
    }
    public static class RiftTemplates
    {
        static Vector2[] Points(params float[] xy)=>Enumerable.Range(0,xy.Length/2).Select(i=>new Vector2(xy[i*2],xy[i*2+1])).ToArray();
        static Vector2[] Corners(float x,float y)=>Points(-x,-y,-x,y,x,-y,x,y);
        public static readonly IReadOnlyList<RoomTemplate> All=Array.AsReadOnly(new[]{
            new RoomTemplate("RM01","둥근 납골실",16,16,false,Points(-8,0,8,0),Corners(4.7f,4.7f),Points(0,-5.7f,0,5.7f)),
            new RoomTemplate("RM02","십자 회랑",20,20,false,Points(-10,0,10,0,0,-10,0,10),Corners(6.5f,6.5f),Points(-4,-6,4,-6,-4,6,4,6)),
            new RoomTemplate("RM03","이중 묘역",24,16,false,Points(-12,0,12,0),Points(-8,-4.7f,0,-4.7f,8,-4.7f,-8,4.7f,0,4.7f,8,4.7f),Points(-4,-5.8f,4,5.8f)),
            new RoomTemplate("RM04","곡선 장례길",24,12,false,Points(-12,-2,12,2),Corners(7,3.5f),Points(-2,-3.5f,2,3.5f)),
            new RoomTemplate("RM05","침수 제단",20,20,false,Points(-10,0,10,0,0,10),Points(-6.5f,-6.5f,-6.5f,6.5f,6.5f,-6.5f,6.5f,6.5f,-5.5f,0,5.5f,0),Points(-3,-7,3,-7,3,7)),
            new RoomTemplate("RM06","망자의 광장",24,24,true,Points(-12,0,12,0,0,-12,0,12),Corners(9,9),Points(-5,-9,5,9)),
            new RoomTemplate("RM07","무너진 병영",20,16,false,Points(-10,0,10,0),Corners(6.5f,4.7f),Points(0,-5.7f,0,5.7f)),
            new RoomTemplate("RM08","순환 성벽길",24,16,false,Points(-12,0,12,0),Points(-8,-4.7f,0,-4.7f,8,-4.7f,-8,4.7f,0,4.7f,8,4.7f),Points(-4,-5.8f,4,5.8f)),
            new RoomTemplate("RM09","쌍문 무기고",20,20,false,Points(-10,0,10,0,0,-10),Points(-6.5f,-6.5f,-6.5f,6.5f,6.5f,-6.5f,6.5f,6.5f,-5.5f,0,5.5f,0),Points(-3,7,3,7,3,-7)),
            new RoomTemplate("RM10","화로 뜰",20,20,false,Points(-10,0,10,0,0,10),Corners(6.5f,6.5f),Points(-3,-7,3,-7,3,7)),
            new RoomTemplate("RM11","갈라진 대홀",24,24,false,Points(-12,0,12,0,0,-12,0,12),Points(-8.5f,-8.5f,-8.5f,8.5f,8.5f,-8.5f,8.5f,8.5f,-6.5f,0,6.5f,0),Points(-4,-9,4,-9,-4,9,4,9)),
            new RoomTemplate("RM12","폐왕의 광장",26,26,true,Points(-13,0,13,0,0,-13,0,13),Corners(10,10),Points(-5,-10,5,10))
        });
        public static RoomTemplate Get(string id)=>All.Single(t=>t.id==id);
        public static RiftRoom Instantiate(RoomTemplate t,int index,Vector2 position,int rotation,int variant,List<RiftObstacle> obstacles)
        {
            var room=new RiftRoom{index=index,templateId=t.id,position=position,rotation=rotation,variant=variant,boss=t.boss,size=rotation%2==0?t.size:new Vector2(t.size.y,t.size.x)};
            for(int n=0;n<t.doors.Length;n++)
            {
                var p=t.doors[n];var normal=Mathf.Abs(p.x)>=t.size.x*.5f-.1f?new Vector2(Mathf.Sign(p.x),0):new Vector2(0,Mathf.Sign(p.y));
                room.doors.Add(new RiftDoor{index=n,position=room.Transform(p),direction=RiftRoom.Rotate(normal,rotation)});
            }
            room.groupAnchors=t.groups.Select(room.Transform).ToList();room.chestAnchors=t.chests.Select(room.Transform).ToList();
            void Box(string kind,float x,float z,float hx,float hz,float height,bool sight=true)
            {obstacles.Add(new RiftObstacle{id=index+":"+obstacles.Count,kind=kind,position=room.Transform(new Vector2(x,z)),halfSize=rotation%2==0?new Vector2(hx,hz):new Vector2(hz,hx),height=height,blocksSight=sight,blocksProjectile=sight});}
            switch(t.id)
            {
                case "RM01":
                    if(variant==0){Box("Pillar",0,-7,.6f,.6f,2.5f);Box("Pillar",0,7,.6f,.6f,2.5f);}
                    else if(variant==1){Box("Pillar",-6.8f,6.8f,.6f,.6f,2.5f);Box("Pillar",6.8f,-6.8f,.6f,.6f,2.5f);}
                    else {Box("Pillar",-6.8f,-6.8f,.6f,.6f,2.5f);Box("Pillar",6.8f,6.8f,.6f,.6f,2.5f);}break;
                case "RM02":foreach(var p in Corners(8.6f,8.6f))Box(variant==0?"Graves":variant==1?"Pillar":"Rubble",p.x,p.y,.6f,.6f,variant==2?.6f:2,variant!=2);break;
                case "RM03":
                    if(variant==1){Box("Graves",-4,6.8f,1.2f,.55f,1.4f);Box("Graves",4,-6.8f,1.2f,.55f,1.4f);}
                    else {Box("Graves",-4,0,1,variant==0?1.5f:1.8f,1.4f);Box("Graves",4,0,1,variant==0?1.5f:1.8f,1.4f);}break;
                case "RM04":Box(variant==1?"Rubble":"Pillar",-3,variant==2?4.8f:-4.8f,.8f,.55f,variant==1?.5f:1.8f,variant!=1);Box("Graves",3,variant==2?-4.8f:4.8f,.8f,.55f,1.4f);break;
                case "RM05":obstacles.Add(new RiftObstacle{id=index+":altar",kind="Altar",position=position,radius=2+variant*.5f,height=2});break;
                case "RM06":case "RM12":foreach(var p in Corners(t.size.x*.5f-.8f,t.size.y*.5f-.8f))Box(variant==0?"Sarcophagus":variant==1?"Pillar":"Statue",p.x,p.y,.6f,.6f,2+variant*.3f);break;
                case "RM07":if(variant<2)Box("Rubble",0,variant==0?3.2f:-3.2f,2,.6f,.8f,false);else {Box("Rubble",-3,-1,1,.6f,.8f,false);Box("Rubble",3,1,1,.6f,.8f,false);}break;
                case "RM08":Box(variant==2?"Rubble":"Wall",variant==0?-1.5f:0,0,variant==2?2:3,1,variant==2?.7f:2.5f,variant!=2);break;
                case "RM09":Box("Rack",variant==1?-8.5f:-3,variant==1?3:8.5f,.5f,.5f,2);Box("Rack",variant==1?8.5f:3,variant==1?-3:variant==2?-8.5f:8.5f,.5f,.5f,2);break;
                case "RM10":foreach(var p in Corners(variant==0?8.7f:variant==1?4:8.8f,8.7f))Box("Brazier",p.x,p.y,.55f,.55f,1.2f,false);break;
                case "RM11":if(variant==0)Box("Rubble",0,0,3,1,.6f,false);else if(variant==1){Box("Rubble",-3,-1,1,.8f,.6f,false);Box("Rubble",3,1,1,.8f,.6f,false);}else {Box("Rubble",-10,-4,.7f,1,.6f,false);Box("Rubble",10,4,.7f,1,.6f,false);}break;
            }
            return room;
        }
    }
}
