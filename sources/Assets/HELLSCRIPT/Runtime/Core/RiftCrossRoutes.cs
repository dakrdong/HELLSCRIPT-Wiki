using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    // Compact interior routes give every room an inward choice and retain a true X crossing.
    // They retain real room/door endpoints so exploration, gates and saved routes share a graph.
    public static class RiftCrossRoutes
    {
        public static void AddCentralRoom(RiftLayout map,ref uint layout,ref uint decoration)
        {
            var template=RiftTemplates.Get(map.theme==0?"RM02":"RM10");
            var position=new Vector2(RandomStream.Range(ref layout,-2,3),RandomStream.Range(ref layout,-2,3));
            var room=RiftTemplates.Instantiate(template,map.rooms.Count,position,RandomStream.Range(ref layout,0,4),RandomStream.Range(ref decoration,0,3),map.obstacles);
            room.central=true;map.rooms.Add(room);
        }
        static Rect Expand(Rect bounds,float margin)=>new Rect(bounds.min-Vector2.one*margin,bounds.size+Vector2.one*margin*2);
        public static void PreparePerimeterSockets(RiftLayout map,int spine)
        {
            foreach(var room in map.rooms.Take(spine))
                foreach(var direction in new[]{Vector2.up,Vector2.down,Vector2.left,Vector2.right})
                {
                    if(room.doors.Any(d=>d.direction==direction))continue;
                    var side=new Vector2(-direction.y,direction.x);
                    foreach(float offset in new[]{0f,-4,4,-6,6})
                    {
                        if(Mathf.Abs(offset)>Vector2.Dot(room.size,new Vector2(Mathf.Abs(side.x),Mathf.Abs(side.y)))*.5f-3)continue;
                        var position=room.position+Vector2.Scale(room.size*.5f,direction)+side*offset;
                        if(Enumerable.Range(-6,13).Any(step=>map.obstacles.Any(o=>o.blocksWalk&&o.Contains(position+direction*(step*.5f),1.2f))))continue;
                        room.doors.Add(new RiftDoor{index=room.doors.Count,position=position,direction=direction});break;
                    }
                }
        }
        static RiftDoor Port(RiftLayout map,RiftRoom room,Vector2 target)
        {
            var toward=(target-room.position).normalized;var candidates=new List<RiftDoor>(room.doors.Where(d=>d.corridor<0));
            foreach(var direction in new[]{Vector2.up,Vector2.down,Vector2.left,Vector2.right})
                if(!room.doors.Any(d=>d.direction==direction))candidates.Add(new RiftDoor{index=-1,direction=direction,position=room.position+Vector2.Scale(room.size*.5f,direction)});
            foreach(var door in candidates.OrderByDescending(d=>Vector2.Dot(d.direction,toward)))
            {
                bool clear=true;
                for(int step=-6;step<=6;step++)
                    if(map.obstacles.Any(o=>o.blocksWalk&&o.Contains(door.position+door.direction*(step*.5f),1.2f))){clear=false;break;}
                if(!clear)continue;
                // Procedural side sockets supplement authored sockets without reusing an occupied
                // exit. ShapeRooms carves the neck and the final navigation audit checks it.
                if(door.index<0){door.index=room.doors.Count;room.doors.Add(door);}
                return door;
            }
            throw new InvalidOperationException($"No independent crossing socket: {room.index} {room.templateId}.");
        }
        public static void RingPorts(RiftLayout map,int spine,int[] previous,int[] next)
        {
            var states=new List<Vector2Int>[spine];var costs=new float[spine][,];
            for(int i=0;i<spine;i++)
            {
                var room=map.rooms[i];int inward=room.doors.OrderByDescending(d=>Vector2.Dot(d.direction,-room.position.normalized)).First().index;
                states[i]=new List<Vector2Int>();
                foreach(var a in room.doors)foreach(var b in room.doors)
                    if(a.index!=b.index&&a.index!=inward&&b.index!=inward)states[i].Add(new Vector2Int(a.index,b.index));
                if(states[i].Count==0)throw new InvalidOperationException("A perimeter room has no independent inward exit.");
            }
            var forbidden=map.rooms.Select(r=>Expand(r.Bounds,3.6f)).ToArray();
            for(int i=0;i<spine;i++)
            {
                int j=(i+1)%spine;costs[i]=new float[map.rooms[i].doors.Count,map.rooms[j].doors.Count];
                foreach(var a in map.rooms[i].doors)foreach(var b in map.rooms[j].doors)
                {
                    var start=a.position+a.direction*5.5f;var end=b.position+b.direction*5.5f;
                    float cost=Vector2.Distance(start,end)+11;int samples=Mathf.CeilToInt(Vector2.Distance(start,end));
                    foreach(var bounds in forbidden)
                    {
                        bool blocked=false;for(int n=0;n<=samples;n++)if(bounds.Contains(Vector2.Lerp(start,end,n/(float)Mathf.Max(1,samples)))){blocked=true;break;}
                        if(blocked)cost+=30;
                    }
                    costs[i][a.index,b.index]=cost;
                }
            }
            float best=float.PositiveInfinity;int[] selected=null;
            for(int initial=0;initial<states[0].Count;initial++)
            {
                var distances=new float[spine][];var parents=new int[spine][];
                for(int i=0;i<spine;i++){distances[i]=Enumerable.Repeat(float.PositiveInfinity,states[i].Count).ToArray();parents[i]=new int[states[i].Count];}
                distances[0][initial]=0;
                for(int i=1;i<spine;i++)for(int b=0;b<states[i].Count;b++)for(int a=0;a<states[i-1].Count;a++)
                {
                    float cost=distances[i-1][a]+costs[i-1][states[i-1][a].y,states[i][b].x];
                    if(cost<distances[i][b]){distances[i][b]=cost;parents[i][b]=a;}
                }
                for(int final=0;final<states[spine-1].Count;final++)
                {
                    float cost=distances[spine-1][final]+costs[spine-1][states[spine-1][final].y,states[0][initial].x];
                    if(cost>=best)continue;best=cost;selected=new int[spine];selected[spine-1]=final;
                    for(int i=spine-1;i>0;i--)selected[i-1]=parents[i][selected[i]];
                }
            }
            for(int i=0;i<spine;i++){previous[i]=states[i][selected[i]].x;next[i]=states[i][selected[i]].y;}
        }
        public static void Connect(RiftLayout map,int spine)
        {
            var central=map.rooms.Single(r=>r.central);
            // Every perimeter room gets an inward choice. A subset reaches the central
            // room; the others pair across the interior and cross those radial routes.
            int[] spokeIds=spine==6?new[]{0,2,3,5}:spine==7?new[]{0,2,4}:new[]{0,2,4,6};
            uint rng=RiftGenerator.Derive(map.layoutSeed,"crossing");int shift=RandomStream.Range(ref rng,0,spine);
            spokeIds=spokeIds.Select(i=>(i+shift)%spine).ToArray();
            var remaining=Enumerable.Range(0,spine).Where(i=>!spokeIds.Contains(i)).ToArray();
            // Pair opposite remaining rooms, avoiding another perimeter connection.
            var pairs=new List<Vector2Int>();
            for(int i=0;i<remaining.Length/2;i++)pairs.Add(new Vector2Int(remaining[i],remaining[i+remaining.Length/2]));
            var forbidden=map.rooms.Select(r=>Expand(r.Bounds,3.6f)).ToArray();
            float bestCrossing=float.PositiveInfinity;
            int chosenSpoke=-1,chosenPair=-1;Vector2 crossing=Vector2.zero,radial=Vector2.zero,transverse=Vector2.zero;
            foreach(int spoke in spokeIds)
            {
                var outer=map.rooms[spoke];var axis=(central.position-outer.position).normalized;
                var side=new Vector2(-axis.y,axis.x);
                for(int pair=0;pair<pairs.Count;pair++)
                {
                    var a=map.rooms[pairs[pair].x];var b=map.rooms[pairs[pair].y];
                    foreach(float t in new[]{.5f,.45f,.55f,.4f,.6f})
                    {
                        var point=Vector2.Lerp(outer.position,central.position,t);
                        if(Vector2.Dot(a.position-point,side)*Vector2.Dot(b.position-point,side)>=0)continue;
                        bool clear=true;
                        foreach(var direction in new[]{axis,side})for(int n=-12;n<=12;n++)
                            if(forbidden.Any(r=>r.Contains(point+direction*(n*.5f)))){clear=false;break;}
                        if(!clear)continue;
                        float da=Vector2.Distance(a.position,point),db=Vector2.Distance(b.position,point);
                        float score=Mathf.Max(da,db)+(da+db)*.25f;
                        if(score>=bestCrossing)continue;bestCrossing=score;
                        chosenSpoke=spoke;chosenPair=pair;crossing=point;radial=axis;
                        transverse=Vector2.Dot(b.position-a.position,side)>0?side:-side;
                    }
                }
            }
            if(chosenSpoke<0)throw new InvalidOperationException("No compact four-arm crossing.");
            // Assign central ports together so an earlier diagonal cannot consume the
            // only useful cardinal socket for a later spoke.
            var centralPorts=new Dictionary<int,RiftDoor>();
            foreach(int spoke in spokeIds.OrderByDescending(i=>Mathf.Max(Mathf.Abs((map.rooms[i].position-central.position).normalized.x),Mathf.Abs((map.rooms[i].position-central.position).normalized.y))))
            {
                var door=Port(map,central,map.rooms[spoke].position);centralPorts.Add(spoke,door);door.corridor=int.MaxValue;
            }
            foreach(int spoke in spokeIds)
            {
                var room=map.rooms[spoke];var port=Port(map,room,central.position);var inner=centralPorts[spoke];inner.corridor=-1;
                RiftGenerator.Connect(map,spoke,central.index,port.index,inner.index,true,spoke==chosenSpoke?(Vector2?)crossing:null,spoke==chosenSpoke?(Vector2?)radial:null);
            }
            for(int i=0;i<pairs.Count;i++)
            {
                var a=map.rooms[pairs[i].x];var b=map.rooms[pairs[i].y];
                var pa=Port(map,a,central.position);var pb=Port(map,b,central.position);
                RiftGenerator.Connect(map,a.index,b.index,pa.index,pb.index,true,i==chosenPair?(Vector2?)crossing:null,i==chosenPair?(Vector2?)transverse:null);
            }
        }
        public static RiftJunction Crossing(RiftLayout map)
        {
            var routes=map.corridors.Where(c=>c.crossing).ToArray();if(routes.Length!=2)return null;
            foreach(var junction in map.junctions.Where(j=>routes.All(c=>j.corridors.Contains(c.index))))
            {
                // Saved v6 maps retain nine-metre arms. Compact maps require four metres,
                // a substantial angle and four actual traversable branches.
                var axes=new List<Vector2>();
                foreach(var route in routes)
                {
                    int index=route.points.FindIndex(p=>Vector2.Distance(p,junction.position)<.01f);
                    if(index<3||index+3>=route.points.Count)break;
                    var a=route.points[index-3]-junction.position;var b=route.points[index+3]-junction.position;
                    float arm=map.version>=7?3.9f:8.9f;
                    if(map.introductory)arm*=IntroductoryRift.GeometryScale;
                    if(a.magnitude<arm||b.magnitude<arm||Vector2.Dot(a.normalized,b.normalized)>-.99f)break;
                    axes.Add(a.normalized);
                }
                if(axes.Count==2&&Mathf.Abs(Vector2.Dot(axes[0],axes[1]))<.5f)return junction;
            }
            return null;
        }
        public static void Validate(RiftLayout map)
        {
            var centers=map.rooms.Where(r=>r.central).ToArray();
            if(centers.Length<1||centers.Any(r=>r.boss||r.role==RiftRoomRole.Wing||r.position.magnitude>10||r.doors.Count(d=>d.corridor>=0)<2))
                throw new InvalidOperationException("An interior room with independent exits is required.");
            var crossing=Crossing(map);if(crossing==null||map.rooms.Any(r=>Expand(r.Bounds,map.introductory?3*IntroductoryRift.GeometryScale:3).Contains(crossing.position)))
                throw new InvalidOperationException("Two transverse routes must meet at an exterior X crossing.");
            var nav=new RiftNavigation(map,true);
            if(!nav.Reachable(crossing.position)||!nav.CanLand(crossing.position,1.2f))throw new InvalidOperationException("The X crossing is not traversable.");
        }
    }
}
