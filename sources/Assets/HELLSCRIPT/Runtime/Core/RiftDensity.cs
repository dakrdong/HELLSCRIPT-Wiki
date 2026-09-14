using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    // Geometric quality limits run before content placement and again in the final audit.
    // The two-metre sample grid is fixed in world space, so translating a map cannot
    // change its reported scale or hide an oversized hole by resizing a preview.
    public static class RiftDensity
    {
        public const float SampleStep=2,MinimumFloorRatio=.48f,MaximumEmptyRadius=14,MaximumUnbranchedLength=40;
        [Serializable]
        public struct Metrics
        {
            public float floorRatio,emptyRadius,hullArea,longestUnbranched;
            public int longestCorridor;
        }
        public static Metrics Measure(RiftLayout map)
        {
            var surface=new RiftSurface(map);
            var hull=Hull(surface.patches.SelectMany(p=>p.points));
            if(hull.Count<3)throw new InvalidOperationException("A dungeon needs a non-empty floor envelope.");
            var envelope=new RiftFloorPatch(hull.ToArray());
            var min=new Vector2(Mathf.Floor(envelope.bounds.xMin/SampleStep)*SampleStep,Mathf.Floor(envelope.bounds.yMin/SampleStep)*SampleStep);
            int width=Mathf.CeilToInt((envelope.bounds.xMax-min.x)/SampleStep)+1,height=Mathf.CeilToInt((envelope.bounds.yMax-min.y)/SampleStep)+1;
            var floor=new bool[width*height];var inside=new bool[floor.Length];int count=0,covered=0;
            for(int y=0;y<height;y++)for(int x=0;x<width;x++)
            {
                int i=y*width+x;var p=min+new Vector2(x,y)*SampleStep;
                floor[i]=surface.Contains(p);inside[i]=envelope.Contains(p);
                if(inside[i]){count++;if(floor[i])covered++;}
            }
            // Separable squared Euclidean distance transform; no per-sample physics calls.
            var horizontal=new float[floor.Length];
            for(int y=0;y<height;y++)for(int x=0;x<width;x++)
            {
                float best=float.PositiveInfinity;
                for(int k=0;k<width;k++)if(floor[y*width+k])best=Mathf.Min(best,(x-k)*(x-k));
                horizontal[y*width+x]=best;
            }
            float largest=0;
            for(int y=0;y<height;y++)for(int x=0;x<width;x++)
            {
                int i=y*width+x;if(!inside[i]||floor[i])continue;float best=float.PositiveInfinity;
                for(int k=0;k<height;k++)best=Mathf.Min(best,horizontal[k*width+x]+(y-k)*(y-k));
                largest=Mathf.Max(largest,best);
            }
            var result=new Metrics{floorRatio=count>0?covered/(float)count:0,emptyRadius=Mathf.Sqrt(largest)*SampleStep,longestCorridor=-1};
            for(int n=0;n<hull.Count;n++)result.hullArea+=RiftFloorPatch.Cross(hull[n],hull[(n+1)%hull.Count])*.5f;
            var choices=map.junctions.Where(j=>IsChoice(map,j)).ToArray();
            foreach(var route in map.corridors)
            {
                var cuts=new List<float>{0,RiftNavigation.Length(route.points)};
                foreach(var junction in choices)
                    if(junction.corridors.Contains(route.index)&&Project(route,junction.position,out float distance)<.15f)cuts.Add(distance);
                cuts.Sort();
                for(int i=1;i<cuts.Count;i++)if(cuts[i]-cuts[i-1]>result.longestUnbranched)
                {result.longestUnbranched=cuts[i]-cuts[i-1];result.longestCorridor=route.index;}
            }
            return result;
        }
        static List<Vector2> Hull(IEnumerable<Vector2> points)
        {
            var sorted=points.Distinct().OrderBy(p=>p.x).ThenBy(p=>p.y).ToArray();var hull=new List<Vector2>();
            foreach(var p in sorted)
            {
                while(hull.Count>=2&&RiftFloorPatch.Cross(hull[hull.Count-1]-hull[hull.Count-2],p-hull[hull.Count-1])<=0)hull.RemoveAt(hull.Count-1);
                hull.Add(p);
            }
            int lower=hull.Count;
            for(int n=sorted.Length-2;n>=0;n--)
            {
                var p=sorted[n];
                while(hull.Count>lower&&RiftFloorPatch.Cross(hull[hull.Count-1]-hull[hull.Count-2],p-hull[hull.Count-1])<=0)hull.RemoveAt(hull.Count-1);
                hull.Add(p);
            }
            if(hull.Count>1)hull.RemoveAt(hull.Count-1);return hull;
        }
        static float Project(RiftCorridor route,Vector2 point,out float distance)
        {
            distance=0;float error=float.PositiveInfinity,along=0;
            for(int i=1;i<route.points.Count;i++)
            {
                var a=route.points[i-1];var delta=route.points[i]-a;float length=delta.magnitude;if(length<.0001f)continue;
                float t=Mathf.Clamp01(Vector2.Dot(point-a,delta)/(length*length));float d=Vector2.Distance(a+delta*t,point);
                if(d<error){error=d;distance=along+t*length;}along+=length;
            }
            return error;
        }
        static Vector2 PointAt(RiftCorridor route,float distance)
        {
            for(int i=1;i<route.points.Count;i++)
            {
                float segment=Vector2.Distance(route.points[i-1],route.points[i]);
                if(distance<=segment)return Vector2.Lerp(route.points[i-1],route.points[i],segment>0?distance/segment:0);
                distance-=segment;
            }
            return route.points[route.points.Count-1];
        }
        public static bool IsChoice(RiftLayout map,RiftJunction junction)
        {
            var arms=new List<Vector2>();int routes=0;
            foreach(int index in junction.corridors.Distinct())
            {
                var route=map.corridors.FirstOrDefault(c=>c.index==index);
                if(route==null||Project(route,junction.position,out float distance)>.15f)continue;
                routes++;float length=RiftNavigation.Length(route.points);
                foreach(float d in new[]{distance-3,distance+3})
                {
                    if(d<0||d>length)continue;var arm=(PointAt(route,d)-junction.position).normalized;
                    // Parallel overlaps and tangent contacts are not new route choices.
                    if(arm.sqrMagnitude>.9f&&!arms.Any(a=>Vector2.Dot(a,arm)>.9f))arms.Add(arm);
                }
            }
            return routes>=2&&arms.Count>=3;
        }
        public static void Validate(RiftLayout map)
        {
            if(map.rooms.Any(r=>r.doors.Count(d=>d.corridor>=0)<3))throw new InvalidOperationException("Every compact room needs three independent exits.");
            var metrics=Measure(map);
            if(metrics.floorRatio<MinimumFloorRatio||metrics.emptyRadius>MaximumEmptyRadius)
                throw new InvalidOperationException($"Sparse dungeon interior: {metrics.floorRatio:F3}, empty radius {metrics.emptyRadius:F1} m.");
            if(metrics.longestUnbranched>MaximumUnbranchedLength)
                throw new InvalidOperationException($"Unbranched passage too long: {metrics.longestCorridor}, {metrics.longestUnbranched:F1} m.");
        }
    }
}
