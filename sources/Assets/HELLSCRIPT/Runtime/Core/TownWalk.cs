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
        public readonly string name,service,action,npcName;
        public readonly Vector2 position;
        public readonly Rect building;
        public Vector2 NpcPosition=>id==TownStation.AspectStone?building.center:id==TownStation.RiftKeeper?position+new Vector2(-2.2f,-1.6f):position;
        internal TownStationDefinition(TownStation id,string name,string service,string action,Vector2 position,Rect building=default,string npcName="")
        {this.id=id;this.name=name;this.service=service;this.action=action;this.position=position;this.building=building;this.npcName=npcName;}
    }
    public sealed class TownResidentDefinition
    {
        public readonly string id,name;
        public readonly Vector2 position;
        internal TownResidentDefinition(string id,string name,Vector2 position)
        {this.id=id;this.name=name;this.position=position;}
    }
    public static class TownLayout
    {
        public const float Speed=5.5f,ArriveRadius=1.4f,InteractionRadius=2.8f,HeroRadius=.55f,HalfWidth=55,HalfHeight=35;
        public static readonly Vector2 Spawn=new Vector2(0,6);
        // Map-right is screen-right at the fixed camera angle.
        public static readonly Vector2 Right=new Vector2(18,12).normalized,Back=new Vector2(-12,18).normalized;
        public static Vector2 ToWorld(Vector2 p)=>Right*p.x+Back*p.y;
        public static Vector2 FromWorld(Vector2 p)=>new Vector2(Vector2.Dot(p,Right),Vector2.Dot(p,Back));
        static Rect House(float x,float y,float w=12,float h=9)=>new Rect(x-w/2,y-h/2,w,h);
        public static readonly TownStationDefinition[] Stations=
        {
            new TownStationDefinition(TownStation.AspectStone,"위상 각인석","위상 수집 · 레벨업 · 각인","각인석 열기",new Vector2(-34,1),House(-34,4,5,2)),
            new TownStationDefinition(TownStation.Warehouse,"창고","계정 창고 · 장비 보관","창고 열기",new Vector2(-37,13),House(-37,21,14,10),"차도르 사마프"),
            new TownStationDefinition(TownStation.Merchant,"무기 상인","장비 구매 · 판매","상점 열기",new Vector2(-14,16),House(-14,24),"제이크 보쿤"),
            new TownStationDefinition(TownStation.Gambler,"갬블 상인","미확인 장비 구매 · 판매","갬블 상점 열기",new Vector2(-6,-24),House(-6,-16,11,9)),
            new TownStationDefinition(TownStation.Blacksmith,"대장간","장비 재련 · 강화 · 제작","대장간 이용",new Vector2(14,17),House(14,25,14,9),"마르크 쿠스"),
            new TownStationDefinition(TownStation.Training,"훈련 교관","자유 훈련 · 설정 비교","훈련하기",new Vector2(37,9)),
            new TownStationDefinition(TownStation.GemMerchant,"보석 상인","보석 구매 · 소켓 관리","보석 상점 열기",new Vector2(-32,-24),House(-32,-16,11,9)),
            new TownStationDefinition(TownStation.RuneMerchant,"룬 상인","룬 블록 구매 · 룬 배치","룬 상점 열기",new Vector2(21,-24),House(21,-16,12,9)),
            new TownStationDefinition(TownStation.RuneMaster,"룬 마스터","룬 재형성 · 룬 승급","룬 공방 열기",new Vector2(43,-24),House(43,-16,11,9),"인젤 미르"),
            new TownStationDefinition(TownStation.RiftKeeper,"균열","단계 선택 · 균열 입장","균열 열기",new Vector2(8,0),npcName:"안톤 진다크"),
        };
        // Residents have no service ID, so their labels cannot dispatch a shop or interaction.
        public static readonly TownResidentDefinition[] Residents=
        {
            new TownResidentDefinition("Pyonya","표냐 내르뭰",new Vector2(-13,-7)),
            new TownResidentDefinition("Jean","쟝 죠린",new Vector2(-11,7)),
            new TownResidentDefinition("Darc","달크 알뷔",new Vector2(23,4)),
        };
        public static TownStationDefinition Station(TownStation id)=>Stations.Single(s=>s.id==(id==TownStation.Reroller?TownStation.Blacksmith:id));
        public static Vector2 Clamp(Vector2 p)=>new Vector2(Mathf.Clamp(p.x,-HalfWidth,HalfWidth),Mathf.Clamp(p.y,-HalfHeight,HalfHeight));
        public static Rect Clearance(Rect r)=>new Rect(r.xMin-HeroRadius,r.yMin-HeroRadius,r.width+HeroRadius*2,r.height+HeroRadius*2);
        public static bool Walkable(Vector2 p)=>p==Clamp(p)&&!Stations.Any(s=>s.building.width>0&&Clearance(s.building).Contains(p));
        public static TownStation? Nearby(Vector2 p)
        {
            TownStation? result=null;float distance=InteractionRadius;
            foreach(var s in Stations){float d=Vector2.Distance(p,s.position);if(d<=distance){distance=d;result=s.id;}}return result;
        }
        public static bool ClearSegment(Vector2 a,Vector2 b)
        {
            foreach(var s in Stations)
            {
                if(s.building.width<=0)continue;Rect r=Clearance(s.building);Vector2 d=b-a;float low=0,high=1;
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
            foreach(var s in Stations.Where(s=>s.building.width>0))
            {
                Rect r=Clearance(s.building);const float e=.05f;
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
