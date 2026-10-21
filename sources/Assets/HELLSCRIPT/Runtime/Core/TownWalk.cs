using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    // Preserve old station numbers. Reroller shares the blacksmith's service.
    public enum TownStation { Blacksmith, Reroller, Merchant, RiftKeeper, Warehouse, Training, GemMerchant, RuneMerchant, Gambler, RuneMaster, AspectStone }
    public sealed class TownStationDefinition
    {
        public readonly TownStation id;
        public readonly string name,service,action;
        public string npcName=>NpcProfiles.ForStation(id)?.name??"";
        public readonly Vector2 position;
        public readonly Rect building;
        // The building model is shown at this uniform scale; building is its scaled footprint.
        public readonly float scale;
        public Vector2 NpcPosition=>id==TownStation.AspectStone?building.center:id==TownStation.RiftKeeper?position+new Vector2(-2.2f,-1.6f):position;
        internal TownStationDefinition(TownStation id,string name,string service,string action,Vector2 position,Rect building=default,float scale=1)
        {this.id=id;this.name=name;this.service=service;this.action=action;this.position=position;this.building=building;this.scale=scale;}
    }
    public sealed class TownResidentDefinition
    {
        public readonly string id;
        public string name=>NpcProfiles.ForResident(id).name;
        public readonly Vector2 position;
        internal TownResidentDefinition(string id,Vector2 position)
        {this.id=id;this.position=position;}
    }
    public static class TownLayout
    {
        public const float Speed=5.5f,ArriveRadius=1.4f,InteractionRadius=2.8f,HeroRadius=.55f,HalfWidth=35,HalfHeight=24;
        public static readonly Vector2 Spawn=new Vector2(-2,1);
        // Map-right is screen-right at the fixed camera angle.
        public static readonly Vector2 Right=new Vector2(18,12).normalized,Back=new Vector2(-12,18).normalized;
        public static Vector2 ToWorld(Vector2 p)=>Right*p.x+Back*p.y;
        public static Vector2 FromWorld(Vector2 p)=>new Vector2(Vector2.Dot(p,Right),Vector2.Dot(p,Back));
        static Rect House(float x,float y,float w=12,float h=9)=>new Rect(x-w/2,y-h/2,w,h);
        // A building model's footprint (manifest size at scale 1) shown at scale, its camera-facing front edge on y = front.
        static Rect Model(float x,float front,float w,float d,float scale)=>new Rect(x-w*scale/2,front,w*scale,d*scale);
        // The layout follows the approved casual overview (Docs/Art/CasualConcept/2026-10-13/Town): a compact fenced village around a
        // cross of cobbled streets. North row, west to east: warehouse, weapon stall, well, forge, training yard. The aspect stone stands
        // on a round court in the middle of the west main street (the user's placement); the rift gateway at the south edge of the crossroads. South row: gamble, gem, rune and rune-workshop houses facing the
        // camera. Every rectangle is its World/Town model footprint times the scale; a missing model falls back to a primitive house of it.
        // South-row roofs stay below the main street on screen (back edge + height / tan 49 deg < -2.6), so they never hide it.
        public static readonly TownStationDefinition[] Stations=
        {
            new TownStationDefinition(TownStation.AspectStone,"위상 각인석","위상 수집 · 레벨업 · 각인","각인석 열기",new Vector2(-19,-2.2f),House(-19,.3f,3f,1.4f)),
            new TownStationDefinition(TownStation.Warehouse,"창고","계정 창고 · 장비 보관","창고 열기",new Vector2(-26.5f,10.2f),Model(-26.5f,12,10.645f,9.577f,1),1),
            new TownStationDefinition(TownStation.Merchant,"무기 상인","장비 구매 · 판매","상점 열기",new Vector2(-11,11.2f),Model(-11,13,5.035f,5.28f,1.2f),1.2f),
            new TownStationDefinition(TownStation.Gambler,"갬블 상인","미확인 장비 구매 · 판매","갬블 상점 열기",new Vector2(-24.5f,-22),Model(-24.5f,-20,5.76f,6.447f,1.25f),1.25f),
            new TownStationDefinition(TownStation.Blacksmith,"대장간","장비 재련 · 강화 · 제작","대장간 이용",new Vector2(10,9.7f),Model(10,11.5f,6.653f,9.835f,1.35f),1.35f),
            new TownStationDefinition(TownStation.Training,"훈련 교관","자유 훈련 · 설정 비교","훈련하기",new Vector2(28,11.2f)),
            new TownStationDefinition(TownStation.GemMerchant,"보석 상인","보석 변환 · 물약 제조 · 소켓 관리","보석상 열기",new Vector2(-8.5f,-22),Model(-8.5f,-20,7.723f,5f,1.15f),1.15f),
            new TownStationDefinition(TownStation.RuneMerchant,"룬 상인","룬 블록 구매 · 룬 배치","룬 상점 열기",new Vector2(14,-22),Model(14,-20,4.375f,6.922f,1.15f),1.15f),
            new TownStationDefinition(TownStation.RuneMaster,"룬 마스터","룬 재형성 · 룬 승급","룬 공방 열기",new Vector2(29.5f,-22),Model(29.5f,-20,6.579f,6.621f,1.25f),1.25f),
            new TownStationDefinition(TownStation.RiftKeeper,"균열","단계 선택 · 균열 입장","균열 열기",new Vector2(5.5f,-3.8f)),
        };
        // Residents can converse, but never own a service ID or dispatch a shop.
        public static readonly TownResidentDefinition[] Residents=
        {
            new TownResidentDefinition("Pyonya",new Vector2(-9.5f,-.6f)),
            new TownResidentDefinition("Jean",new Vector2(-32,.4f)),
            new TownResidentDefinition("Darc",new Vector2(21,-.6f)),
        };
        // Solid scenery the hero walks around like a building: the well and the trunks of the trees that stand inside the fence
        // (WorldView.TownEnvironment plants a tree on the centre of each). Small props (lanterns, barrels, planters) stay walk-through.
        public static readonly Rect[] Obstacles=
        {
            House(-6.5f,6.2f,3.2f,3f),
            House(-18,21.5f,2.6f,2.6f),House(-17.3f,16.2f,1.8f,1.8f),House(-5.6f,20.8f,2.6f,2.6f),House(17.4f,17.6f,1.8f,1.8f),House(-33.5f,18,1.8f,1.8f),
            House(-33.5f,-11,2.6f,2.6f),House(-33.8f,-18.5f,1.8f,1.8f),House(-16.6f,-14.5f,1.8f,1.8f),House(21.8f,-12.5f,2.6f,2.6f),
            House(-18,-8.2f,1.8f,1.8f),House(5.4f,-14.2f,2.6f,2.6f),House(9.2f,-9.8f,1.6f,1.6f),
        };
        static readonly Rect[] Blocks=Stations.Where(s=>s.building.width>0).Select(s=>s.building).Concat(Obstacles).ToArray();
        public static TownStationDefinition Station(TownStation id)=>Stations.Single(s=>s.id==(id==TownStation.Reroller?TownStation.Blacksmith:id));
        public static Vector2 ReturnPortalPosition=>Station(TownStation.RiftKeeper).position+new Vector2(7,0);
        public static Vector2 Clamp(Vector2 p)=>new Vector2(Mathf.Clamp(p.x,-HalfWidth,HalfWidth),Mathf.Clamp(p.y,-HalfHeight,HalfHeight));
        public static Rect Clearance(Rect r)=>new Rect(r.xMin-HeroRadius,r.yMin-HeroRadius,r.width+HeroRadius*2,r.height+HeroRadius*2);
        public static bool Walkable(Vector2 p)=>p==Clamp(p)&&!Blocks.Any(b=>Clearance(b).Contains(p));
        public static TownStation? Nearby(Vector2 p)
        {
            TownStation? result=null;float distance=InteractionRadius;
            foreach(var s in Stations){float d=Vector2.Distance(p,s.position);if(s.id==TownStation.RiftKeeper)d=Mathf.Min(d,Vector2.Distance(p,s.NpcPosition));if(d<=distance){distance=d;result=s.id;}}return result;
        }
        public static bool ClearSegment(Vector2 a,Vector2 b)
        {
            foreach(var block in Blocks)
            {
                Rect r=Clearance(block);Vector2 d=b-a;float low=0,high=1;
                bool Intersects(float origin,float delta,float min,float max)
                {
                    if(Mathf.Abs(delta)<.00001f)return origin>min&&origin<max;
                    float t1=(min-origin)/delta,t2=(max-origin)/delta;
                    low=Mathf.Max(low,Mathf.Min(t1,t2));high=Mathf.Min(high,Mathf.Max(t1,t2));return high>=low;
                }
                if(Intersects(a.x,d.x,r.xMin,r.xMax)&&Intersects(a.y,d.y,r.yMin,r.yMax))return false;
            }return true;
        }
        // Small visibility graph: the same footprints govern destination taps and manual movement.
        public static List<Vector2> Route(Vector2 from,Vector2 to)
        {
            to=Clamp(to);if(!Walkable(to))return new List<Vector2>();if(ClearSegment(from,to))return new List<Vector2>{to};
            var nodes=new List<Vector2>{from,to};
            foreach(var block in Blocks)
            {
                Rect r=Clearance(block);const float e=.05f;
                nodes.Add(new Vector2(r.xMin-e,r.yMin-e));nodes.Add(new Vector2(r.xMax+e,r.yMin-e));
                nodes.Add(new Vector2(r.xMax+e,r.yMax+e));nodes.Add(new Vector2(r.xMin-e,r.yMax+e));
            }
            var cost=Enumerable.Repeat(float.PositiveInfinity,nodes.Count).ToArray();var previous=Enumerable.Repeat(-1,nodes.Count).ToArray();var done=new bool[nodes.Count];cost[0]=0;
            for(int step=0;step<nodes.Count;step++)
            {
                int next=-1;for(int n=0;n<nodes.Count;n++)if(!done[n]&&(next<0||cost[n]<cost[next]))next=n;
                if(next<0||float.IsInfinity(cost[next])||next==1)break;done[next]=true;
                for(int n=1;n<nodes.Count;n++)
                {
                    float candidate=cost[next]+Vector2.Distance(nodes[next],nodes[n]);
                    if(done[n]||candidate>=cost[n]||!ClearSegment(nodes[next],nodes[n]))continue;cost[n]=candidate;previous[n]=next;
                }
            }
            var route=new List<Vector2>();if(previous[1]<0)return route;
            for(int n=1;n!=0;n=previous[n])route.Add(nodes[n]);route.Reverse();return route;
        }
    }
    public static class TownPick
    {
        public static bool Nearest(IReadOnlyList<(TownStation id,Vector2 screen)> projected,Vector2 point,float radius,out TownStation station)
        {
            station=default;float best=float.PositiveInfinity;bool found=false;
            foreach(var item in projected){float d=Vector2.Distance(item.screen,point);if(d<=radius&&d<best){best=d;station=item.id;found=true;}}return found;
        }
    }
    // Session-only movement. Arrival never opens UI or mutates the account.
    public sealed class TownWalk
    {
        public Vector2 Position {get;private set;}=TownLayout.Spawn;
        public Vector2 Facing {get;private set;}=Vector2.up;
        public TownStation? Destination {get;private set;}
        public TownStation? Nearby=>TownLayout.Nearby(Position);
        public float Elapsed {get;private set;}
        public bool Walking=>route.Count>0||manualMoving;
        public float Remaining=>route.Count==0?0:Vector2.Distance(Position,route[0])+route.Zip(route.Skip(1),(a,b)=>Vector2.Distance(a,b)).Sum();
        readonly List<Vector2> route=new List<Vector2>();TownStation? arrival;bool manualMoving;
        public void Request(TownStation station)
        {
            Cancel();var s=TownLayout.Station(station);
            if(Vector2.Distance(Position,s.position)<=TownLayout.ArriveRadius){arrival=s.id;return;}
            route.AddRange(TownLayout.Route(Position,s.position));if(route.Count>0)Destination=s.id;
        }
        public void RequestPoint(Vector2 point){Cancel();route.AddRange(TownLayout.Route(Position,point));}
        public void Cancel(){route.Clear();Destination=null;arrival=null;manualMoving=false;}
        public bool Move(Vector2 input,float dt)
        {
            if(dt<=0||input.sqrMagnitude<.01f){manualMoving=false;return false;}
            Cancel();Elapsed+=dt;input=Vector2.ClampMagnitude(input,1);Facing=input.normalized;Vector2 before=Position;
            Vector2 delta=input*(TownLayout.Speed*dt);int steps=Mathf.Max(1,Mathf.CeilToInt(delta.magnitude/.2f));delta/=steps;
            for(int n=0;n<steps;n++)
            {
                Vector2 next=TownLayout.Clamp(Position+delta);
                if(TownLayout.Walkable(next)&&TownLayout.ClearSegment(Position,next)){Position=next;continue;}
                next=TownLayout.Clamp(Position+new Vector2(delta.x,0));if(TownLayout.Walkable(next)&&TownLayout.ClearSegment(Position,next))Position=next;
                next=TownLayout.Clamp(Position+new Vector2(0,delta.y));if(TownLayout.Walkable(next)&&TownLayout.ClearSegment(Position,next))Position=next;
            }
            manualMoving=Vector2.Distance(before,Position)>.0001f;return manualMoving;
        }
        public bool Tick(float dt)
        {
            manualMoving=false;if(dt<=0)return false;Elapsed+=dt;if(route.Count==0)return false;Vector2 before=Position;float budget=TownLayout.Speed*dt;
            while(route.Count>0&&budget>0)
            {
                var delta=route[0]-Position;float distance=delta.magnitude;float stop=route.Count==1&&Destination.HasValue?TownLayout.ArriveRadius:0;
                float travel=Mathf.Min(budget,Mathf.Max(0,distance-stop));if(distance>.0001f)Facing=delta/distance;
                Position+=Facing*travel;budget-=travel;
                if(distance-travel>stop+.001f)break;route.RemoveAt(0);
            }
            if(route.Count==0){arrival=Destination;Destination=null;}return before!=Position;
        }
        public bool TakeArrival(out TownStation station)
        {if(arrival.HasValue){station=arrival.Value;arrival=null;return true;}station=default;return false;}
    }
}
