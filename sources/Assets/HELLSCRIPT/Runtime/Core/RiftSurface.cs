using System;
using System.Collections.Generic;
using UnityEngine;

namespace Hellscript
{
    // Convex floor pieces shared by simulation, world meshes and the minimap. No bounding box
    // becomes walkable floor: boxes are only the broad phase, including for diagonal passages.
    public sealed class RiftFloorPatch
    {
        public readonly Vector2[] points;
        public readonly Rect bounds;
        public readonly int room, corridor;
        public readonly Vector2 center;
        public RiftFloorPatch(Vector2[] points,int room=-1,int corridor=-1)
        {
            this.points=points;this.room=room;this.corridor=corridor;
            Vector2 min=points[0],max=min,sum=Vector2.zero;
            foreach(var p in points){min=Vector2.Min(min,p);max=Vector2.Max(max,p);sum+=p;}
            bounds=new Rect(min,max-min);center=sum/points.Length;
        }
        public static float Cross(Vector2 a,Vector2 b)=>a.x*b.y-a.y*b.x;
        public bool Contains(Vector2 p)
        {
            if(p.x<bounds.xMin-.0001f||p.x>bounds.xMax+.0001f||p.y<bounds.yMin-.0001f||p.y>bounds.yMax+.0001f)return false;
            for(int n=0;n<points.Length;n++)
                if(Cross(points[(n+1)%points.Length]-points[n],p-points[n])<-.0001f)return false;
            return true;
        }
        public bool Clip(Vector2 a,Vector2 b,out float begin,out float end)
        {
            begin=0;end=1;
            if(Mathf.Max(a.x,b.x)<bounds.xMin||Mathf.Min(a.x,b.x)>bounds.xMax||Mathf.Max(a.y,b.y)<bounds.yMin||Mathf.Min(a.y,b.y)>bounds.yMax)return false;
            // Opposite sides of a shared edge must produce the same intersection. Float cross
            // products introduced sub-micrometre gaps that blocked short steps on solid floor.
            double dx=(double)b.x-a.x,dy=(double)b.y-a.y,low=0,high=1;
            for(int n=0;n<points.Length;n++)
            {
                var p=points[n];var q=points[(n+1)%points.Length];
                double ex=(double)q.x-p.x,ey=(double)q.y-p.y;
                double offset=ex*((double)a.y-p.y)-ey*((double)a.x-p.x),slope=ex*dy-ey*dx;
                if(Math.Abs(slope)<1e-12){if(offset< -1e-10)return false;continue;}
                double t=-offset/slope;if(slope>0)low=Math.Max(low,t);else high=Math.Min(high,t);
                if(low>high+1e-12)return false;
            }
            begin=(float)low;end=(float)high;
            return true;
        }
    }

    public sealed class RiftSurface
    {
        public readonly List<RiftFloorPatch> patches=new List<RiftFloorPatch>();
        public Rect Bounds {get;private set;}
        const float BucketSize=8;
        readonly Dictionary<Vector2Int,List<int>> buckets=new Dictionary<Vector2Int,List<int>>();
        readonly List<Vector2> intervals=new List<Vector2>();
        int[] stamps;int stamp;
        static Vector2Int Bucket(Vector2 p)=>new Vector2Int(Mathf.FloorToInt(p.x/BucketSize),Mathf.FloorToInt(p.y/BucketSize));
        public RiftSurface(RiftLayout map)
        {
            foreach(var room in map.rooms)
            {
                if(room.outline!=null&&room.outline.Count>=3)
                {
                    // Merge adjacent fan triangles while the sector stays convex. This preserves
                    // the exact contour but avoids testing dozens of narrow triangles per step.
                    int start=0;
                    while(start<room.outline.Count)
                    {
                        var sector=new List<Vector2>{room.position,room.outline[start],room.outline[(start+1)%room.outline.Count]};
                        int next=start+2;
                        while(next<=room.outline.Count)
                        {
                            sector.Add(room.outline[next%room.outline.Count]);
                            if(!Convex(sector)){sector.RemoveAt(sector.Count-1);break;}next++;
                        }
                        Add(new RiftFloorPatch(sector.ToArray(),room:room.index));start=next-1;
                    }
                }
                else Add(Rectangle(room.Bounds,room.index,-1));
            }
            foreach(var c in map.corridors)
            {
                bool organic=c.widths!=null&&c.widths.Count==c.points.Count;
                for(int n=1;n<c.points.Count;n++)
                {
                    var a=c.points[n-1];var b=c.points[n];
                    if(!organic)
                    {var min=Vector2.Min(a,b)-Vector2.one*c.width*.5f;var max=Vector2.Max(a,b)+Vector2.one*c.width*.5f;Add(Rectangle(new Rect(min,max-min),-1,c.index));continue;}
                    var side=new Vector2(-(b-a).y,(b-a).x).normalized;
                    Add(new RiftFloorPatch(new[]{a-side*c.widths[n-1]*.5f,b-side*c.widths[n]*.5f,b+side*c.widths[n]*.5f,a+side*c.widths[n-1]*.5f},corridor:c.index));
                }
                // Small convex discs close the outside of bends; endpoint discs remain within
                // the authored four-metre gate neck. Old manifests keep their square end caps.
                if(organic)for(int n=0;n<c.points.Count;n++)
                {
                    var points=new Vector2[12];for(int i=0;i<points.Length;i++)
                    {float angle=i*Mathf.PI*2/points.Length;points[i]=c.points[n]+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*c.widths[n]*.5f;}
                    Add(new RiftFloorPatch(points,corridor:c.index));
                }
            }
            stamps=new int[patches.Count];
        }
        static bool Convex(List<Vector2> polygon)
        {
            for(int n=0;n<polygon.Count;n++)
                if(RiftFloorPatch.Cross(polygon[(n+1)%polygon.Count]-polygon[n],polygon[(n+2)%polygon.Count]-polygon[(n+1)%polygon.Count])<-.00001f)return false;
            return true;
        }
        static RiftFloorPatch Rectangle(Rect r,int room,int corridor)=>new RiftFloorPatch(new[]{r.min,new Vector2(r.xMax,r.yMin),r.max,new Vector2(r.xMin,r.yMax)},room,corridor);
        void Add(RiftFloorPatch patch)
        {
            int index=patches.Count;patches.Add(patch);
            Bounds=index==0?patch.bounds:Rect.MinMaxRect(Mathf.Min(Bounds.xMin,patch.bounds.xMin),Mathf.Min(Bounds.yMin,patch.bounds.yMin),Mathf.Max(Bounds.xMax,patch.bounds.xMax),Mathf.Max(Bounds.yMax,patch.bounds.yMax));
            Vector2Int min=Bucket(patch.bounds.min),max=Bucket(patch.bounds.max);
            for(int x=min.x;x<=max.x;x++)for(int y=min.y;y<=max.y;y++)
            {var key=new Vector2Int(x,y);if(!buckets.TryGetValue(key,out var list)){list=new List<int>();buckets.Add(key,list);}list.Add(index);}
        }
        public bool Contains(Vector2 p)
        {if(buckets.TryGetValue(Bucket(p),out var list))foreach(int i in list)if(patches[i].Contains(p))return true;return false;}
        public bool RoomContains(int room,Vector2 p)
        {if(buckets.TryGetValue(Bucket(p),out var list))foreach(int i in list)if(patches[i].room==room&&patches[i].Contains(p))return true;return false;}
        public bool SegmentOnFloor(Vector2 a,Vector2 b)
        {
            if(!Contains(a)||!Contains(b))return false;
            // Movement uses short steps. A fixed parametric tolerance shrank to sub-micrometre
            // world gaps and could reject a 0.2 m prefix of an accepted 0.3 m path at a shared seam.
            // Keep a 0.01 mm world tolerance; this does not bridge meaningful floor gaps.
            float tolerance=Mathf.Max(.000001f,.00001f/Mathf.Max(.01f,Vector2.Distance(a,b)));
            intervals.Clear();if(++stamp==int.MaxValue){Array.Clear(stamps,0,stamps.Length);stamp=1;}
            var cell=Bucket(a);var goal=Bucket(b);var delta=b-a;
            int stepX=delta.x>=0?1:-1,stepY=delta.y>=0?1:-1;
            float strideX=Mathf.Abs(delta.x)<.000001f?float.PositiveInfinity:BucketSize/Mathf.Abs(delta.x);
            float strideY=Mathf.Abs(delta.y)<.000001f?float.PositiveInfinity:BucketSize/Mathf.Abs(delta.y);
            float nextX=float.IsPositiveInfinity(strideX)?strideX:((cell.x+(stepX>0?1:0))*BucketSize-a.x)/delta.x;
            float nextY=float.IsPositiveInfinity(strideY)?strideY:((cell.y+(stepY>0?1:0))*BucketSize-a.y)/delta.y;
            int remaining=Mathf.Abs(goal.x-cell.x)+Mathf.Abs(goal.y-cell.y)+1;
            // Visit only buckets crossed by the segment. A diagonal sight ray must not inspect
            // every polygon in its bounding rectangle, especially across an empty central void.
            while(remaining-->0)
            {
                if(!buckets.TryGetValue(cell,out var list))return false;
                foreach(int i in list)
                {
                    if(stamps[i]==stamp)continue;stamps[i]=stamp;
                    if(!patches[i].Clip(a,b,out float first,out float last))continue;
                    if(first<=tolerance&&last>=1-tolerance)return true;
                    intervals.Add(new Vector2(first,last));
                }
                if(cell==goal)break;
                if(nextX<nextY){cell.x+=stepX;nextX+=strideX;}else{cell.y+=stepY;nextY+=strideY;}
            }
            intervals.Sort((x,y)=>x.x.CompareTo(y.x));float covered=0;
            foreach(var interval in intervals){if(interval.x>covered+tolerance)return false;covered=Mathf.Max(covered,interval.y);if(covered>=1-tolerance)return true;}
            return false;
        }
    }
}
