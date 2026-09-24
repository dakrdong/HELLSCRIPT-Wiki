using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    public static class RiftFieldContent
    {
        internal static bool ClearOfSpawns(RiftLayout map,Vector2 position,Vector2 halfSize)
            =>!map.spawns.Any(s=>Mathf.Abs(s.position.x-position.x)<halfSize.x+.45f&&Mathf.Abs(s.position.y-position.y)<halfSize.y+.45f);
        internal static bool ClearOfPassages(RiftLayout map,Vector2 position,Vector2 halfSize)
        {
            var body=new RiftObstacle{position=position,halfSize=halfSize};
            foreach(var corridor in map.corridors)for(int n=1;n<corridor.points.Count;n++)
                if(RiftNavigation.ObstacleBlocks(body,corridor.points[n-1],corridor.points[n],1.2f))return false;
            foreach(var room in map.rooms)foreach(var door in room.doors.Where(d=>d.corridor>=0))
                if(RiftNavigation.ObstacleBlocks(body,door.position-door.direction*3,door.position+door.direction*3,1.2f))return false;
            return true;
        }
        internal static IEnumerable<Vector2> SupplementaryAnchors(RiftRoom room,RiftNavigation nav)
        {
            // Scaled template corners may all be occupied. Try in-room positions only after
            // the authored anchors; the same spawn, passage, spacing and access checks apply.
            var bounds=room.Bounds;
            for(float y=bounds.yMin+2;y<=bounds.yMax-2;y+=1.5f)
            for(float x=bounds.xMin+2;x<=bounds.xMax-2;x+=1.5f)
            {
                var point=new Vector2(x,y);if(nav.Walkable(point,.8f))yield return point;
            }
        }
        static uint Mixed(uint x){x^=x>>16;x*=0x7feb352du;x^=x>>15;x*=0x846ca68bu;return x^(x>>16);}
        static void Shuffle<T>(List<T> list,ref uint rng)
        {for(int n=list.Count-1;n>0;n--){int p=RandomStream.Range(ref rng,0,n+1);(list[n],list[p])=(list[p],list[n]);}}
        public static int Place(RiftLayout map,RiftNavigation nav,string runId,int stage)
        {
            uint rng=Mixed(RiftGenerator.Derive(map.mapSeed,"field-content-v2"));int cursed=-1;
            if(stage>=5&&RandomStream.Unit(ref rng)<.25f)
            {
                var candidates=map.chests.Where(c=>c.definitionId=="CH01"&&!map.rooms[c.room].boss).ToList();Shuffle(candidates,ref rng);
                foreach(var chest in candidates)
                {
                    var positions=EchoCandidates(map,nav,chest);
                    if(positions.Count<4)continue;
                    chest.definitionId="CH03";cursed=map.chests.IndexOf(chest);
                    map.events.Add(new RiftEvent{id="echo-0",chestId=chest.id,requestId=runId+":echo-0:start",spawnCandidates=positions});break;
                }
                if(cursed<0)throw new InvalidOperationException("저주 상자 이벤트 공간 부족");
            }
            if(RandomStream.Unit(ref rng)<.5f)
            {
                var rooms=map.rooms.Where(r=>r.index!=0&&!r.boss).ToList();Shuffle(rooms,ref rng);RiftShrine shrine=null;
                foreach(var room in rooms)
                {
                    var anchors=room.chestAnchors.ToList();Shuffle(anchors,ref rng);
                    if(map.introductory)anchors.AddRange(SupplementaryAnchors(room,nav));
                    foreach(var anchor in anchors)
                    {
                        if(map.chests.Any(c=>Vector2.Distance(c.position,anchor)<3)||!nav.Reachable(anchor))continue;
                        if(map.introductory&&(!ClearOfSpawns(map,anchor,Vector2.one*.5f)||!ClearOfPassages(map,anchor,Vector2.one*.5f)))continue;
                        var access=new List<Vector2>();foreach(var d in new[]{Vector2.up,Vector2.down,Vector2.left,Vector2.right})
                        {Vector2 p=anchor+d*1.35f;if(nav.Reachable(p)&&nav.TravelClear(anchor,p))access.Add(p);}
                        if(access.Count<2)continue;
                        shrine=new RiftShrine{id="shrine-0",definitionId=RandomStream.Range(ref rng,0,2)==0?"SH01":"SH02",requestId=runId+":shrine-0:use",room=room.index,position=anchor,openingPosition=access[0],accessPoints=access};break;
                    }
                    if(shrine!=null)break;
                }
                if(shrine==null)throw new InvalidOperationException("성소 접근 공간 부족");map.shrines.Add(shrine);
            }
            return cursed;
        }
        static List<Vector2> EchoCandidates(RiftLayout map,RiftNavigation nav,RiftChest chest)
        {
            var result=new List<Vector2>();var room=map.rooms[chest.room];
            for(int ring=0;ring<3;ring++)for(int n=0;n<32;n++)
            {
                float angle=n*Mathf.PI/16;Vector2 p=chest.position+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*(4+ring*2);
                if(!room.Bounds.Contains(p)||!nav.Walkable(p,.4f)||!nav.Reachable(p)||room.doors.Any(d=>Vector2.Distance(p,d.position)<3)||map.spawns.Any(s=>Vector2.Distance(s.position,p)<.9f)||map.chests.Any(c=>Vector2.Distance(p,c.position)<1.5f)||result.Any(q=>Vector2.Distance(p,q)<1.8f))continue;
                result.Add(p);if(result.Count>=16)return result;
            }
            return result;
        }
        public static void AddBodiesAndValidate(RiftLayout map)
        {
            foreach(var chest in map.chests)map.obstacles.Add(new RiftObstacle{id="body:"+chest.id,kind="Chest",position=chest.position,halfSize=new Vector2(.6f,.425f),height=.8f,blocksSight=false,blocksProjectile=false});
            foreach(var shrine in map.shrines)map.obstacles.Add(new RiftObstacle{id="body:"+shrine.id,kind="Shrine",position=shrine.position,halfSize=Vector2.one*.5f,height=1.8f,blocksSight=false,blocksProjectile=false});
            var nav=new RiftNavigation(map);
            foreach(var chest in map.chests)
            {chest.accessPoints.RemoveAll(p=>!nav.Reachable(p));if(chest.accessPoints.Count<2)throw new InvalidOperationException("상자 몸체 우회 공간 부족");chest.openingPosition=chest.accessPoints[0];}
            foreach(var shrine in map.shrines)
            {shrine.accessPoints.RemoveAll(p=>!nav.Reachable(p));if(shrine.accessPoints.Count<2)throw new InvalidOperationException("성소 몸체 우회 공간 부족");shrine.openingPosition=shrine.accessPoints[0];}
            foreach(var evt in map.events)
            {evt.spawnCandidates.RemoveAll(p=>!nav.Reachable(p));if(evt.spawnCandidates.Count<4)throw new InvalidOperationException("잔향 생성점 부족");}
            if(map.spawns.Any(s=>!nav.Walkable(s.position,.4f)||!nav.Reachable(s.position)))throw new InvalidOperationException("상호작용 몸체와 적 배치 충돌");
            // Bodies are added after the initial corridor check. In the smaller layout, a
            // reachable chest can still obstruct a large enemy's passage; retry that candidate.
            if(map.introductory)foreach(var corridor in map.corridors)for(int n=1;n<corridor.points.Count;n++)
                if(!nav.TravelClear(corridor.points[n-1],corridor.points[n],1.2f))throw new InvalidOperationException("Introductory field body obstructs a corridor.");
        }
    }
}
