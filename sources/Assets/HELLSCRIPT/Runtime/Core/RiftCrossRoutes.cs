using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    // Two continuous transverse routes, one passing through an additional central combat room.
    // They retain real room/door endpoints so exploration, gates and saved routes share a graph.
    public static class RiftCrossRoutes
    {
        public static void AddCentralRoom(RiftLayout map,ref uint layout,ref uint decoration)
        {
            var template=RiftTemplates.Get(map.theme==0?"RM02":"RM10");
            var position=new Vector2(RandomStream.Range(ref layout,-4,5),RandomStream.Range(ref layout,-4,5));
            var room=RiftTemplates.Instantiate(template,map.rooms.Count,position,RandomStream.Range(ref layout,0,4),RandomStream.Range(ref decoration,0,3),map.obstacles);
            room.central=true;map.rooms.Add(room);
        }
        static Rect Expand(Rect bounds,float margin)=>new Rect(bounds.min-Vector2.one*margin,bounds.size+Vector2.one*margin*2);
        public static void PreparePerimeterSockets(RiftLayout map,int spine)
        {
            foreach(var room in map.rooms.Take(spine).Skip(1))
                foreach(var direction in new[]{Vector2.up,Vector2.down,Vector2.left,Vector2.right})
                {
                    if(room.doors.Any(d=>d.direction==direction))continue;
                    var position=room.position+Vector2.Scale(room.size*.5f,direction);
                    if(Enumerable.Range(-6,13).Any(step=>map.obstacles.Any(o=>o.blocksWalk&&o.Contains(position+direction*(step*.5f),1.2f))))continue;
                    room.doors.Add(new RiftDoor{index=room.doors.Count,position=position,direction=direction});
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
        public static void Connect(RiftLayout map,int spine)
        {
            var central=map.rooms.Single(r=>r.central);
            var outer=map.rooms.Take(spine).Where(r=>r.index!=0&&!r.boss).ToList();
            // Four consecutive ring rooms A/B/C/D support a complete tour through
            // A -> C -> B -> central -> D, in addition to the retained outer ring.
            // Intersect the A-C and B-central chords to place a genuine exterior X.
            uint rng=RiftGenerator.Derive(map.layoutSeed,"crossing");
            int choices=(outer.Count-3)*2,shift=RandomStream.Range(ref rng,0,choices);
            RiftRoom[] anchors=null;Vector2 crossing=Vector2.zero,axisA=Vector2.zero,axisB=Vector2.zero;
            var forbidden=map.rooms.Select(r=>Expand(r.Bounds,5f)).ToArray();
            for(int offset=0;offset<choices&&anchors==null;offset++)
            {
                int choice=(shift+offset)%choices,start=choice/2;
                var block=outer.Skip(start).Take(4).ToArray();if(choice%2!=0)Array.Reverse(block);
                var first=block[0];var middle=block[1];var last=block[2];
                var a=last.position-first.position;var b=central.position-middle.position;
                float denominator=RiftFloorPatch.Cross(a,b);if(Mathf.Abs(denominator)<.01f)continue;
                var delta=middle.position-first.position;
                float t=RiftFloorPatch.Cross(delta,b)/denominator,u=RiftFloorPatch.Cross(delta,a)/denominator;
                if(t<.15f||t>.85f||u<.15f||u>.85f||Mathf.Abs(Vector2.Dot(a.normalized,b.normalized))>.5f)continue;
                var point=first.position+a*t;bool clear=true;
                foreach(var direction in new[]{a.normalized,b.normalized})for(int step=-12;step<=12;step++)
                    if(forbidden.Any(r=>r.Contains(point+direction*step))){clear=false;break;}
                if(!clear)continue;
                anchors=block;crossing=point;axisA=a.normalized;axisB=b.normalized;
            }
            if(anchors==null)throw new InvalidOperationException("No clear four-arm crossing.");
            var a0=Port(map,anchors[0],crossing);var a2=Port(map,anchors[2],crossing);
            RiftGenerator.Connect(map,anchors[0].index,anchors[2].index,a0.index,a2.index,true,crossing,axisA);
            var a1=Port(map,anchors[1],crossing);var inner=Port(map,central,crossing);
            RiftGenerator.Connect(map,anchors[1].index,central.index,a1.index,inner.index,true,crossing,axisB);
            var exit=Port(map,central,anchors[3].position);var a3=Port(map,anchors[3],central.position);
            RiftGenerator.Connect(map,central.index,anchors[3].index,exit.index,a3.index,true);
        }
        public static RiftJunction Crossing(RiftLayout map)
        {
            var routes=map.corridors.Where(c=>c.crossing).ToArray();if(routes.Length!=2)return null;
            foreach(var junction in map.junctions.Where(j=>routes.All(c=>j.corridors.Contains(c.index))))
            {
                // A real interior intersection must have nine metres on both sides of each
                // route and a substantial angle. Endpoint touches and tangencies do not count.
                var axes=new List<Vector2>();
                foreach(var route in routes)
                {
                    int index=route.points.FindIndex(p=>Vector2.Distance(p,junction.position)<.01f);
                    if(index<3||index+3>=route.points.Count)break;
                    var a=route.points[index-3]-junction.position;var b=route.points[index+3]-junction.position;
                    if(a.magnitude<8.9f||b.magnitude<8.9f||Vector2.Dot(a.normalized,b.normalized)>-.99f)break;
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
            var crossing=Crossing(map);if(crossing==null||map.rooms.Any(r=>Expand(r.Bounds,3).Contains(crossing.position)))
                throw new InvalidOperationException("Two transverse routes must meet at an exterior X crossing.");
            var nav=new RiftNavigation(map,true);
            if(!nav.Reachable(crossing.position)||!nav.CanLand(crossing.position,1.2f))throw new InvalidOperationException("The X crossing is not traversable.");
        }
    }
}
