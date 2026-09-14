using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    // HELLSCRIPT's own procedural tile silhouettes and transition paths. Room roles, rewards,
    // authored encounter anchors and gate sockets stay under their existing owners.
    public static class RiftOrganicGeometry
    {
        public static void ShapeRooms(RiftLayout map)
        {
            foreach(var room in map.rooms)
            {
                uint rng=RiftGenerator.Derive(map.layoutSeed,"outline:"+room.index);
                float phase=RandomStream.Unit(ref rng)*Mathf.PI*2;
                var half=room.size*.5f;
                var angles=new List<float>();for(int n=0;n<64;n++)angles.Add(n*Mathf.PI*2/64);
                var sockets=new List<Rect>();
                foreach(var door in room.doors.Where(d=>d.corridor>=0))
                {
                    var center=door.position-room.position-door.direction*3;
                    var size=door.direction.x!=0?new Vector2(6,door.width):new Vector2(door.width,6);
                    var rect=new Rect(center-size*.5f,size);sockets.Add(rect);
                    foreach(var corner in new[]{rect.min,rect.max,new Vector2(rect.xMin,rect.yMax),new Vector2(rect.xMax,rect.yMin)})
                    {float angle=Mathf.Atan2(corner.y,corner.x);angles.Add(angle<0?angle+Mathf.PI*2:angle);}
                }
                angles.Sort();room.outline.Clear();
                foreach(float angle in angles)
                {
                    var ray=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle));
                    // Low-frequency lobes vary the silhouette, never per-vertex white noise.
                    float radial=1/Mathf.Sqrt(ray.x*ray.x/(half.x*half.x)+ray.y*ray.y/(half.y*half.y));
                    radial*=.96f+.05f*Mathf.Sin(3*angle+phase)+.02f*Mathf.Sin(5*angle-phase);
                    // Reserve the full pack radius, diagonal movement footprint and contour chord margin.
                    foreach(var anchor in room.groupAnchors)radial=Mathf.Max(radial,RayCircle(ray,anchor-room.position,2.6f));
                    foreach(var anchor in room.chestAnchors)radial=Mathf.Max(radial,RayCircle(ray,anchor-room.position,2.1f));
                    if(room.index==0)radial=Mathf.Max(radial,RayCircle(ray,map.start-room.position,2.2f));
                    if(room.boss)radial=Mathf.Max(radial,8.2f);
                    foreach(var socket in sockets)radial=Mathf.Max(radial,RayRect(ray,socket));
                    float boundary=Mathf.Min(Mathf.Abs(ray.x)<.00001f?float.MaxValue:half.x/Mathf.Abs(ray.x),Mathf.Abs(ray.y)<.00001f?float.MaxValue:half.y/Mathf.Abs(ray.y));
                    var point=room.position+ray*Mathf.Min(radial,boundary);
                    if(room.outline.Count==0||Vector2.Distance(point,room.outline[room.outline.Count-1])>.001f)room.outline.Add(point);
                }
                if(Vector2.Distance(room.outline[0],room.outline[room.outline.Count-1])<.001f)room.outline.RemoveAt(room.outline.Count-1);
            }
        }
        static float RayCircle(Vector2 ray,Vector2 center,float radius)
        {float along=Vector2.Dot(ray,center),disc=radius*radius-center.sqrMagnitude+along*along;return disc>=0?Mathf.Max(0,along+Mathf.Sqrt(disc)):0;}
        static float RayRect(Vector2 ray,Rect r)
        {
            float first=0,last=float.MaxValue;
            for(int axis=0;axis<2;axis++)
            {
                float d=ray[axis],min=r.min[axis],max=r.max[axis];
                if(Mathf.Abs(d)<.00001f){if(min>0||max<0)return 0;continue;}
                float a=min/d,b=max/d;if(a>b)(a,b)=(b,a);first=Mathf.Max(first,a);last=Mathf.Min(last,b);
                if(first>last)return 0;
            }
            return last;
        }
        static bool Clear(Vector2 a,Vector2 b,Rect[] forbidden)
        {
            int steps=Mathf.Max(1,Mathf.CeilToInt(Vector2.Distance(a,b)*2));
            for(int n=0;n<=steps;n++)if(forbidden.Any(r=>r.Contains(Vector2.Lerp(a,b,n/(float)steps))))return false;
            return true;
        }
        public static List<Vector2> Connect(RiftDoor first,RiftDoor last,List<Vector2> route,Rect[] forbidden,uint seed,float firstNeck=3,float lastNeck=3)
        {
            var direct=new List<Vector2>{route[0]};int current=0;
            while(current<route.Count-1)
            {
                int next=current+1;for(int n=route.Count-1;n>current+1;n--)if(Clear(route[current],route[n],forbidden)){next=n;break;}
                direct.Add(route[next]);current=next;
            }
            var bends=new List<Vector2>{first.position,first.position+first.direction*firstNeck};
            for(int n=0;n<direct.Count;n++)
            {
                bends.Add(direct[n]);if(n==direct.Count-1)break;
                var delta=direct[n+1]-direct[n];if(delta.magnitude<14)continue;
                var side=new Vector2(-delta.y,delta.x).normalized;
                float shift=(RandomStream.Unit(ref seed)<.5f?-1:1)*Mathf.Min(5,delta.magnitude*.14f);
                var middle=(direct[n]+direct[n+1])*.5f+side*shift;
                if(Clear(direct[n],middle,forbidden)&&Clear(middle,direct[n+1],forbidden))bends.Add(middle);
            }
            bends.Add(last.position+last.direction*lastNeck);bends.Add(last.position);
            var smooth=new List<Vector2>{bends[0],bends[1]};
            for(int n=2;n<bends.Count-2;n++)
            {
                var p=bends[n];float trim=Mathf.Min(6,Mathf.Min(Vector2.Distance(p,bends[n-1]),Vector2.Distance(p,bends[n+1]))*.38f);
                var a=Vector2.MoveTowards(p,bends[n-1],trim);var b=Vector2.MoveTowards(p,bends[n+1],trim);
                var arc=new List<Vector2>{a};for(int k=1;k<=6;k++){float t=k/6f;arc.Add((1-t)*(1-t)*a+2*(1-t)*t*p+t*t*b);}
                // The two port approaches have a reserved collar inside their room's broad phase.
                bool safe=n==2||n==bends.Count-3||arc.All(v=>!forbidden.Any(r=>r.Contains(v)));
                if(safe)smooth.AddRange(arc);else smooth.Add(p);
            }
            smooth.Add(bends[bends.Count-2]);smooth.Add(bends[bends.Count-1]);
            var result=new List<Vector2>{smooth[0]};
            for(int n=1;n<smooth.Count;n++)
            {
                float distance=Vector2.Distance(smooth[n-1],smooth[n]);if(distance<.001f)continue;
                int pieces=Mathf.Max(1,Mathf.CeilToInt(distance/3));
                for(int i=1;i<=pieces;i++)result.Add(Vector2.Lerp(smooth[n-1],smooth[n],i/(float)pieces));
            }
            return result;
        }
        public static void SetWidths(RiftCorridor corridor,uint seed)
        {
            float length=RiftNavigation.Length(corridor.points),distance=0,phase=RandomStream.Unit(ref seed)*Mathf.PI*2;
            for(int n=0;n<corridor.points.Count;n++)
            {
                if(n>0)distance+=Vector2.Distance(corridor.points[n-1],corridor.points[n]);
                float collar=Mathf.SmoothStep(0,1,Mathf.Clamp01((Mathf.Min(distance,length-distance)-3)/6));
                corridor.widths.Add(Mathf.Lerp(4,5.6f+.7f*Mathf.Sin(distance*.12f+phase),collar));
            }
        }
    }
}
