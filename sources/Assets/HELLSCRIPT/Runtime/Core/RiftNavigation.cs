using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    // Deterministic grid search shared by corridor construction and the authoritative simulation.
    // Rendering and NavMesh objects never own saved positions or movement outcomes.
    public static class RiftGridSearch
    {
        static readonly Vector2Int[] steps={Vector2Int.right,Vector2Int.up,Vector2Int.left,Vector2Int.down};
        struct Entry {public Vector2Int cell;public int score,cost;}
        sealed class Heap
        {
            readonly List<Entry> entries=new List<Entry>();
            public int Count=>entries.Count;
            static bool Before(Entry a,Entry b)=>a.score<b.score||a.score==b.score&&(a.cell.x<b.cell.x||a.cell.x==b.cell.x&&a.cell.y<b.cell.y);
            public void Push(Entry e)
            {
                entries.Add(e);int n=entries.Count-1;
                while(n>0){int p=(n-1)/2;if(!Before(e,entries[p]))break;entries[n]=entries[p];n=p;}entries[n]=e;
            }
            public Entry Pop()
            {
                var first=entries[0];var last=entries[entries.Count-1];entries.RemoveAt(entries.Count-1);if(entries.Count==0)return first;
                int n=0;while(n*2+1<entries.Count){int c=n*2+1;if(c+1<entries.Count&&Before(entries[c+1],entries[c]))c++;if(!Before(entries[c],last))break;entries[n]=entries[c];n=c;}entries[n]=last;return first;
            }
        }
        public static List<Vector2> Find(Vector2Int start,Vector2Int goal,Func<Vector2Int,bool> passable,int limit=50000,Func<Vector2Int,Vector2Int,bool> canStep=null)
        {
            if(!passable(start)||!passable(goal))return null;
            var open=new Heap();var costs=new Dictionary<Vector2Int,int>{{start,0}};var parents=new Dictionary<Vector2Int,Vector2Int>();
            int Heuristic(Vector2Int p)=>Mathf.Abs(p.x-goal.x)+Mathf.Abs(p.y-goal.y);
            open.Push(new Entry{cell=start,score=Heuristic(start)});int expanded=0;
            while(open.Count>0&&expanded++<limit)
            {
                var node=open.Pop();if(costs[node.cell]!=node.cost)continue;
                if(node.cell==goal)
                {
                    var result=new List<Vector2>{goal};var p=goal;while(p!=start){p=parents[p];result.Add(p);}result.Reverse();return Compress(result);
                }
                foreach(var step in steps)
                {
                    var next=node.cell+step;int cost=node.cost+1;
                    if(costs.TryGetValue(next,out int previous)&&previous<=cost)continue;
                    if(!passable(next)||canStep!=null&&!canStep(node.cell,next))continue;
                    costs[next]=cost;parents[next]=node.cell;open.Push(new Entry{cell=next,cost=cost,score=cost+Heuristic(next)});
                }
            }
            return null;
        }
        public static List<Vector2> Compress(List<Vector2> input)
        {
            var output=new List<Vector2>();foreach(var p in input)
            {
                if(output.Count>0&&Vector2.Distance(p,output[output.Count-1])<.001f)continue;
                if(output.Count>=2)
                {
                    var a=output[output.Count-1]-output[output.Count-2];var b=p-output[output.Count-1];
                    if(Mathf.Abs(a.x*b.y-a.y*b.x)<.001f&&Vector2.Dot(a,b)>0)output.RemoveAt(output.Count-1);
                }
                output.Add(p);
            }
            return output;
        }
    }

    public sealed class RiftNavigation
    {
        public readonly RiftLayout Layout;
        public Vector2[] Rooms {get;}
        readonly List<Rect> floors=new List<Rect>();
        public readonly RiftSurface Surface;
        readonly HashSet<Vector2Int> cells=new HashSet<Vector2Int>();
        readonly Dictionary<int,Route> routes=new Dictionary<int,Route>();
        readonly HashSet<Vector2Int> reachable=new HashSet<Vector2Int>();
        readonly bool forceGatesOpen;
        bool gridBuilt,gatesClosed;
        static readonly Vector2[] footprint={new Vector2(1,0),new Vector2(-1,0),new Vector2(0,1),new Vector2(0,-1),new Vector2(1,1),new Vector2(1,-1),new Vector2(-1,1),new Vector2(-1,-1)};
        sealed class Route {public Vector2 target;public List<Vector2> points;public int next;public float retryAt;}
        public bool HeroPathFailed {get{RefreshGates();return routes.TryGetValue(0,out var path)&&path.points==null;}}
        public int WalkableCellCount {get{RefreshGates();return cells.Count;}}
        public RiftNavigation(RiftLayout layout,bool gatesOpen=false)
        {
            Layout=layout??throw new ArgumentNullException(nameof(layout));Rooms=layout.rooms.Select(r=>r.position).ToArray();forceGatesOpen=gatesOpen;
            Surface=new RiftSurface(layout);foreach(var patch in Surface.patches)floors.Add(patch.bounds);
            RefreshGates();
        }
        public void RefreshGates()
        {
            if(Layout.legacy)return;
            bool closed=!forceGatesOpen&&!Layout.gateOpen&&(Layout.gates?.Count??0)>0;
            if(gridBuilt&&closed==gatesClosed)return;
            gridBuilt=true;gatesClosed=closed;cells.Clear();reachable.Clear();routes.Clear();
            var candidates=new HashSet<Vector2Int>();
            foreach(var floor in floors)for(int x=Mathf.CeilToInt(floor.xMin);x<=Mathf.FloorToInt(floor.xMax);x++)for(int y=Mathf.CeilToInt(floor.yMin);y<=Mathf.FloorToInt(floor.yMax);y++)candidates.Add(new Vector2Int(x,y));
            foreach(var p in candidates)if(Walkable(p))cells.Add(p);
            var start=NearestCell(Layout.start);if(start.HasValue)
            {
                var queue=new Queue<Vector2Int>();queue.Enqueue(start.Value);reachable.Add(start.Value);
                var directions=new[]{Vector2Int.up,Vector2Int.down,Vector2Int.left,Vector2Int.right};
                while(queue.Count>0){var p=queue.Dequeue();foreach(var d in directions){var next=p+d;if(cells.Contains(next)&&!reachable.Contains(next)&&TravelClear(p,next)&&reachable.Add(next))queue.Enqueue(next);}}
            }
        }
        bool Floor(Vector2 p)
        {return Surface.Contains(p);}
        public bool Walkable(Vector2 p,float radius=.45f)
        {
            if(Layout.legacy)return RiftMap.Walkable(p);
            RefreshGates();
            if(!Floor(p))return false;foreach(var offset in footprint)if(!Floor(p+offset*radius))return false;
            foreach(var obstacle in Layout.obstacles)if(obstacle.blocksWalk&&obstacle.Contains(p,radius))return false;
            if(gatesClosed)foreach(var gate in Layout.gates)if(gate.barrier.Contains(p,radius))return false;
            return true;
        }
        public bool CanLand(Vector2 p,float radius=.45f)=>Walkable(p,radius)&&!Layout.obstacles.Any(o=>o.blocksLanding&&o.Contains(p,radius));
        public bool LineClear(Vector2 a,Vector2 b)=>Clear(a,b,0);
        public bool ProjectileClear(Vector2 a,Vector2 b,float radius=0)=>Clear(a,b,1,radius);
        public Vector2 ProjectileEnd(Vector2 from,Vector2 end,float radius,out bool wall)
        {
            wall=!ProjectileClear(from,end,radius);if(!wall)return end;
            float low=0,high=1;for(int n=0;n<16;n++){float mid=(low+high)*.5f;if(ProjectileClear(from,Vector2.Lerp(from,end,mid),radius))low=mid;else high=mid;}
            return Vector2.Lerp(from,end,low);
        }
        public bool TravelClear(Vector2 a,Vector2 b,float radius=.45f)=>Clear(a,b,2,radius);
        bool Clear(Vector2 a,Vector2 b,int mode,float radius=0)
        {
            if(Layout.legacy)return RiftMap.LineClear(a,b);
            RefreshGates();
            if(!SegmentOnFloor(a,b))return false;
            if(mode!=0&&radius>0)foreach(var offset in footprint)if(!SegmentOnFloor(a+offset*radius,b+offset*radius))return false;
            int staticCount=Layout.obstacles.Count,count=staticCount+(gatesClosed?Layout.gates.Count:0);
            for(int index=0;index<count;index++)
            {
                var o=index<staticCount?Layout.obstacles[index]:Layout.gates[index-staticCount].barrier;
                if(!(mode==0?o.blocksSight:mode==1?o.blocksProjectile:o.blocksWalk))continue;
                float margin=mode==0?0:radius;
                if(o.radius>0)
                {Vector2 delta=b-a;float t=delta.sqrMagnitude<.000001f?0:Mathf.Clamp01(Vector2.Dot(o.position-a,delta)/delta.sqrMagnitude);if((a+t*delta-o.position).sqrMagnitude<(o.radius+margin)*(o.radius+margin))return false;}
                else
                {var half=o.halfSize+Vector2.one*margin-Vector2.one*.00001f;if(Clip(new Rect(o.position-half,half*2),a,b,out _,out _))return false;}
            }
            return true;
        }
        bool SegmentOnFloor(Vector2 a,Vector2 b)
        {return Surface.SegmentOnFloor(a,b);}
        static bool Clip(Rect r,Vector2 a,Vector2 b,out float begin,out float end)
        {
            begin=0;end=1;Vector2 delta=b-a;
            for(int axis=0;axis<2;axis++)
            {
                float p=axis==0?a.x:a.y,d=axis==0?delta.x:delta.y,min=axis==0?r.xMin:r.yMin,max=axis==0?r.xMax:r.yMax;
                if(Mathf.Abs(d)<.000001f){if(p<min||p>max)return false;continue;}
                float first=(min-p)/d,last=(max-p)/d;if(first>last)(first,last)=(last,first);begin=Mathf.Max(begin,first);end=Mathf.Min(end,last);if(begin>end)return false;
            }
            return true;
        }
        Vector2Int? NearestCell(Vector2 p,float radius=.45f)
        {
            var center=Vector2Int.RoundToInt(p);Vector2Int? best=null;float distance=float.MaxValue;
            for(int dx=-2;dx<=2;dx++)for(int dy=-2;dy<=2;dy++)
            {var next=center+new Vector2Int(dx,dy);float d=((Vector2)next-p).sqrMagnitude;if(d<distance&&cells.Contains(next)&&TravelClear(p,next,radius)){best=next;distance=d;}}
            return best;
        }
        public bool Reachable(Vector2 p)
        {if(Layout.legacy)return Walkable(p);RefreshGates();var near=NearestCell(p);return near.HasValue&&reachable.Contains(near.Value);}
        public List<Vector2> FindPath(Vector2 from,Vector2 to,float radius=.45f)
        {
            if(!Walkable(from,radius)||!Walkable(to,radius))return null;
            if(TravelClear(from,to,radius))return new List<Vector2>{from,to};
            if(Layout.legacy)
            {
                var legacy=new List<Vector2>{from};Vector2 p=from;for(int n=0;n<2000&&Vector2.Distance(p,to)>.1f;n++){p=RiftMap.Move(p,to,.5f);legacy.Add(p);}return legacy;
            }
            var a=NearestCell(from,radius);var b=NearestCell(to,radius);if(!a.HasValue||!b.HasValue)return null;
            if(reachable.Contains(a.Value)!=reachable.Contains(b.Value))return null;
            var path=RiftGridSearch.Find(a.Value,b.Value,p=>cells.Contains(p)&&(radius<=.45f||Walkable(p,radius)),canStep:(p,q)=>TravelClear(p,q,radius));if(path==null)return null;
            path.Insert(0,from);path.Add(to);var smooth=new List<Vector2>{from};int current=0;
            while(current<path.Count-1)
            {
                int next=current+1;for(int n=path.Count-1;n>current+1;n--)if(TravelClear(path[current],path[n],radius)){next=n;break;}
                if(!TravelClear(path[current],path[next],radius))return null;smooth.Add(path[next]);current=next;
            }
            return smooth;
        }
        public float Length(Vector2 from,Vector2 to)=>Length(FindPath(from,to));
        public static float Length(IReadOnlyList<Vector2> path)
        {if(path==null)return float.PositiveInfinity;float value=0;for(int n=1;n<path.Count;n++)value+=Vector2.Distance(path[n-1],path[n]);return value;}
        public Vector2 Move(Vector2 from,Vector2 to,float distance,int agent=0,float time=0,float radius=.45f)
        {
            if(Layout.legacy)return RiftMap.Move(from,to,distance);
            if(TravelClear(from,to,radius)){routes.Remove(agent);return MoveDirect(from,to,distance,radius);}
            if(!routes.TryGetValue(agent,out var route)||Vector2.Distance(route.target,to)>1.5f||route.points==null&&time>=route.retryAt||route.points!=null&&Vector2.Distance(from,route.target)<.2f)
            {route=new Route{target=to,points=FindPath(from,to,radius),retryAt=time+.5f,next=1};routes[agent]=route;}
            if(route.points==null||route.next>=route.points.Count)return from;
            // A nearby corner is still necessary when skipping it would cut through the wall.
            // Otherwise a point less than .2m away is skipped, rejected, and recreated forever.
            while(route.next<route.points.Count-1&&Vector2.Distance(from,route.points[route.next])<.2f&&TravelClear(from,route.points[route.next+1],radius))route.next++;
            if(!TravelClear(from,route.points[route.next],radius)){routes.Remove(agent);return from;}
            return MoveDirect(from,route.points[route.next],distance,radius);
        }
        public Vector2 MoveDirect(Vector2 from,Vector2 to,float distance,float radius=.45f)
        {
            var candidate=Vector2.MoveTowards(from,to,distance);if(Walkable(candidate,radius)&&TravelClear(from,candidate,radius))return candidate;
            float low=0,high=distance;for(int n=0;n<9;n++){float mid=(low+high)*.5f;var point=Vector2.MoveTowards(from,to,mid);if(Walkable(point,radius)&&TravelClear(from,point,radius))low=mid;else high=mid;}
            return Vector2.MoveTowards(from,to,low);
        }
        public void Repath(int agent=0)=>routes.Remove(agent);
        public int ClosestRoom(Vector2 p)
        {int result=0;float best=float.MaxValue;for(int n=0;n<Rooms.Length;n++){float d=(p-Rooms[n]).sqrMagnitude;if(d<best){best=d;result=n;}}return result;}
        public int RoomAt(Vector2 p)
        {foreach(var room in Layout.rooms)if(Surface.RoomContains(room.index,p))return room.index;return -1;}
        public Vector2 Escape(Vector2 p,Vector2 target,float range,List<EnemyState> enemies,List<GroundEffect> effects,Func<Vector2,bool> danger=null)
        {
            Vector2 best=p;float score=float.MaxValue;
            for(int ring=0;ring<3;ring++)for(int n=0;n<16;n++)
            {
                float angle=n*Mathf.PI/8;Vector2 q=p+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*Mathf.Lerp(2,range,ring/2f);
                if(!CanLand(q)||!LineClear(p,q))continue;
                float value=enemies.Count(e=>!e.dead&&Vector2.Distance(q,e.position)<3)*100+(danger!=null?(danger(q)?300:0):effects.Count(e=>e.hostile&&Vector2.Distance(q,e.position)<e.radius)*300)-Mathf.Min(8,Vector2.Distance(q,target));
                if(value<score){best=q;score=value;}
            }
            return best;
        }
    }
}
