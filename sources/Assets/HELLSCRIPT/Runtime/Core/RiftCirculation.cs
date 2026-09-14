using System;
using System.Collections.Generic;
using System.Linq;

namespace Hellscript
{
    // Validate the authored room graph as well as the separate physical floor audit.
    // No saved route is exposed to exploration AI; this is a generation invariant.
    public static class RiftCirculation
    {
        static bool[,] Graph(RiftLayout map)
        {
            int count=map.rooms.Count;var graph=new bool[count,count];
            if(count<3||count>9||map.rooms.Where((r,i)=>r.index!=i).Any())
                throw new InvalidOperationException("Invalid circulation room graph.");
            foreach(var edge in map.corridors)
            {
                int a=edge.roomA,b=edge.roomB;
                if(a<0||a>=count||b<0||b>=count||a==b||graph[a,b])
                    throw new InvalidOperationException("Invalid circulation corridor.");
                graph[a,b]=graph[b,a]=true;
            }
            return graph;
        }
        static bool Connected(bool[,] graph,int omitRoom=-1,int omitA=-1,int omitB=-1)
        {
            int count=graph.GetLength(0),start=omitRoom==0?1:0;
            var seen=new bool[count];var pending=new Queue<int>();seen[start]=true;pending.Enqueue(start);
            while(pending.Count>0)
            {
                int a=pending.Dequeue();
                for(int b=0;b<count;b++)
                {
                    if(b==omitRoom||seen[b]||!graph[a,b]||a==omitA&&b==omitB||a==omitB&&b==omitA)continue;
                    seen[b]=true;pending.Enqueue(b);
                }
            }
            return Enumerable.Range(0,count).All(i=>i==omitRoom||seen[i]);
        }
        static bool Tour(bool[,] graph,List<int> path,bool[] used)
        {
            int last=path[path.Count-1];if(path.Count==used.Length)return graph[last,0];
            for(int next=1;next<used.Length;next++)
            {
                if(used[next]||!graph[last,next])continue;
                used[next]=true;path.Add(next);if(Tour(graph,path,used))return true;
                path.RemoveAt(path.Count-1);used[next]=false;
            }
            return false;
        }
        public static List<int> FindTour(RiftLayout map)
        {
            var graph=Graph(map);var path=new List<int>{0};var used=new bool[map.rooms.Count];used[0]=true;
            if(!Tour(graph,path,used))return new List<int>();path.Add(0);return path;
        }
        public static void Validate(RiftLayout map)
        {
            var graph=Graph(map);
            if(map.rooms.Any(r=>r.role==RiftRoomRole.Wing)||!Connected(graph))
                throw new InvalidOperationException("Every room must belong to a connected circulation route.");
            for(int room=0;room<map.rooms.Count;room++)
                if(!Connected(graph,room))throw new InvalidOperationException("A room must not be the only way between two regions.");
            foreach(var edge in map.corridors)
                if(!Connected(graph,omitA:edge.roomA,omitB:edge.roomB))
                    throw new InvalidOperationException("Every corridor must have an alternative return route.");
            if(FindTour(map).Count!=map.rooms.Count+1)
                throw new InvalidOperationException("All rooms must be visitable in one tour without retracing a corridor.");
        }
    }
}
