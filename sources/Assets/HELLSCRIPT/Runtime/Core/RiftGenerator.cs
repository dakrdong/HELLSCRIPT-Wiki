using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Hellscript
{
    public static class RiftGenerator
    {
        // Structural alternatives certified against the same density and traversal limits.
        static readonly uint[,] CompactFallbackSeeds={{81002,81005,81008},{81101,81103,81105}};
        public static uint Derive(uint seed,string stream)
        {uint hash=2166136261;foreach(char c in stream){hash^=c;hash*=16777619;}hash^=seed;return RandomStream.Next(ref hash);}
        public static int SelectBoss(ref uint encounter,int previous)
        {
            // Diffuse adjacent development seeds before the history-dependent exclusion.
            // Consume exactly one encounter value; placement keeps its own deterministic sequence.
            uint choice=RandomStream.Next(ref encounter);choice^=choice>>16;choice*=0x7feb352du;choice^=choice>>15;choice*=0x846ca68bu;choice^=choice>>16;
            int roll=RandomStream.Range(ref choice,0,previous>=0?2:3);return previous>=0&&roll>=previous?roll+1:roll;
        }
        public static int SelectBossForStage(ref uint encounter,int previous,int stage)
        {
            int count=ContentUnlocks.BossCount(stage);if(count==3)return SelectBoss(ref encounter,previous);
            var candidates=Enumerable.Range(0,count).ToList();if(count>1)candidates.Remove(previous);
            return candidates[RandomStream.Range(ref encounter,0,candidates.Count)];
        }
        static void Shuffle<T>(List<T> list,ref uint rng)
        {for(int n=list.Count-1;n>0;n--){int p=RandomStream.Range(ref rng,0,n+1);(list[n],list[p])=(list[p],list[n]);}}
        static Rect Expand(Rect r,float amount)=>new Rect(r.min-Vector2.one*amount,r.size+Vector2.one*amount*2);
        // Normal entries use roaming bosses. Explicit objective/arena overrides are retained for
        // compatibility fixtures and development tools for the saved pre-v6 rule set.
        public static RiftLayout Generate(uint seed,string runId,int stage,HeroClass hero,string previous="",int lastBoss=-1,int forcedTheme=-1,int forcedCount=0,RiftObjectiveKind? forcedObjective=null,bool arenaBoss=false,LiveOpsRiftSettings tuning=null)
        {
            tuning??=new LiveOpsRiftSettings();LiveOpsConfig.Validate(tuning);
            arenaBoss|=forcedObjective.HasValue;
            uint choice=Derive(seed,"choice");int theme=forcedTheme>=0?forcedTheme:RandomStream.Range(ref choice,0,ContentUnlocks.ThemeCount(stage));
            int count=forcedCount>0?forcedCount:RandomStream.Range(ref choice,6,9);string lastError="";
            for(int candidate=0;candidate<24;candidate++)
            {
                try
                {
                    var map=Candidate(seed,runId,stage,hero,theme,count,candidate,lastBoss,forcedObjective:forcedObjective,arenaBoss:arenaBoss,tuning:tuning);
                    if(map.fingerprint==previous)continue;return map;
                }
                catch(InvalidOperationException e){lastError=e.Message;}
            }
            // Fixed structural seeds are checked by the same validator as normal layouts.
            // Rewards still use this run's private development reward stream, never the fallback seed.
            for(int n=0;n<3;n++)
            {
                try
                {
                    return Fallback(seed,runId,stage,hero,theme,n,lastBoss,forcedObjective,arenaBoss,tuning);
                }
                catch(InvalidOperationException e){lastError=e.Message;}
            }
            throw new InvalidOperationException(Loc.F("균열을 안전하게 생성하지 못했습니다. 입장 시간과 보상을 변경하지 않았습니다. {0}", lastError));
        }
        public static RiftLayout Fallback(uint seed,string runId,int stage,HeroClass hero,int theme,int index,int lastBoss=-1,RiftObjectiveKind? forcedObjective=null,bool arenaBoss=false,LiveOpsRiftSettings tuning=null)
        {
            arenaBoss|=forcedObjective.HasValue;
            if(theme<0||theme>1||index<0||index>2)throw new ArgumentOutOfRangeException();
            var map=Candidate(seed,runId,stage,hero,theme,6,0,lastBoss,arenaBoss?(uint)(81001+theme*100+index):CompactFallbackSeeds[theme,index],allowWings:false,forcedObjective:forcedObjective,arenaBoss:arenaBoss,tuning:tuning);
            map.fallbackId=$"F{theme+1}-{index+1}";map.candidate=24+index;return map;
        }
        static RiftLayout Candidate(uint seed,string runId,int stage,HeroClass hero,int theme,int count,int candidate,int lastBoss,uint structuralSeed=0,bool allowWings=true,RiftObjectiveKind? forcedObjective=null,bool arenaBoss=false,LiveOpsRiftSettings tuning=null)
        {
            tuning??=new LiveOpsRiftSettings();
            uint basis=structuralSeed==0?Derive(seed,"candidate:"+candidate):structuralSeed;
            var map=new RiftLayout{liveOpsMapScale=tuning.mapScale,liveOpsPackSpread=tuning.packSpread,liveOpsNormalDensity=tuning.normalDensity,version=arenaBoss?5:RiftLayout.CurrentVersion,roamingBoss=!arenaBoss,bossRoom=-1,contentStage=stage,theme=theme,mapSeed=seed,candidate=candidate,layoutSeed=Derive(basis,"layout"),decorationSeed=Derive(basis,"decoration"),encounterSeed=Derive(basis,"encounter"),combatSeed=Derive(seed,"combat"),rewardSeed=Derive(seed,"reward")};
            map.introductory=!arenaBoss&&IntroductoryRift.Applies(stage);
            // Keep established non-intro layouts/fingerprints at v7; v8 carries the smaller budget.
            map.version=map.introductory?RiftLayout.CurrentVersion:arenaBoss?5:7;
            map.objective=!arenaBoss||stage<ObjectiveFirstStage?RiftObjectiveKind.None:forcedObjective??RiftObjectives.Select(seed,stage);
            uint layout=map.layoutSeed,decor=map.decorationSeed;
            var pool=RiftTemplates.All.Skip(theme*6).Take(arenaBoss?6:5).ToList();var templates=new List<RoomTemplate>(pool);
            while(templates.Count<count){var t=pool[RandomStream.Range(ref layout,0,5)];if(templates.Count(x=>x.id==t.id)<2)templates.Add(t);}
            Shuffle(templates,ref layout);int start=templates.FindIndex(t=>!t.boss&&t.doors.Length==2);(templates[0],templates[start])=(templates[start],templates[0]);
            // Dead-end reward wings exist only in the legacy arena compatibility generator.
            int wings=!arenaBoss||!allowWings?0:count>=8?RandomStream.Range(ref layout,1,3):count==7?RandomStream.Range(ref layout,0,3):RandomStream.Range(ref layout,0,2);
            wings=Mathf.Min(wings,count-6);
            int spine=count-wings;
            // Legacy arenas remain on the spine; normal maps contain no arena template.
            int bossTemplate=templates.FindIndex(t=>t.boss);
            if(bossTemplate>=spine){int swap=RandomStream.Range(ref layout,1,spine);(templates[swap],templates[bossTemplate])=(templates[bossTemplate],templates[swap]);}
            float radius=arenaBoss?32+count*2:34+count*1.2f;float stretchX=1+RandomStream.Unit(ref layout)*.1f,stretchY=1+RandomStream.Unit(ref layout)*.1f;
            for(int n=0;n<spine;n++)
            {
                float angle=n*Mathf.PI*2/spine+(RandomStream.Unit(ref layout)-.5f)*(arenaBoss?.25f:.12f);
                float localRadius=radius+(RandomStream.Unit(ref layout)-.5f)*(arenaBoss?10:4);
                var p=new Vector2(Mathf.Round(Mathf.Cos(angle)*localRadius*stretchX),Mathf.Round(Mathf.Sin(angle)*localRadius*stretchY));
                int rotation=(Mathf.RoundToInt(angle/(Mathf.PI*.5f))+1)%4,variant=RandomStream.Range(ref decor,0,3);
                // RM01's axial-pillar variant blocks both possible side sockets. Perimeter
                // cross-route anchors need a clear inward exit, so use its corner-pillar variant.
                if(!arenaBoss&&templates[n].id=="RM01"&&variant==0)variant=1;
                var duplicate=map.rooms.Find(r=>r.templateId==templates[n].id);if(duplicate!=null&&duplicate.rotation==rotation&&duplicate.variant==variant)variant=!arenaBoss&&templates[n].id=="RM01"?3-variant:(variant+1)%3;
                map.rooms.Add(RiftTemplates.Instantiate(templates[n],n,p,rotation,variant,map.obstacles));if(templates[n].boss)map.bossRoom=n;
            }
            var hostPool=Enumerable.Range(1,spine-1).Where(c=>c!=map.bossRoom).ToList();Shuffle(hostPool,ref layout);
            if(hostPool.Count<wings)throw new InvalidOperationException("곁방 연결 방 부족");
            var hosts=new List<int>();
            for(int n=spine;n<count;n++)
            {
                int rotation=RandomStream.Range(ref layout,0,4),variant=RandomStream.Range(ref decor,0,3);
                var size=rotation%2==0?templates[n].size:new Vector2(templates[n].size.y,templates[n].size.x);
                int chosen=-1;Vector2 spot=Vector2.zero;
                // Placed radially outward so a wing never crosses the ring it hangs from. Hosts and
                // distances are tried in order; instantiating a trial would add its obstacles for good.
                foreach(int host in hostPool)
                {
                    if(hosts.Contains(host))continue;var anchor=map.rooms[host];
                    var outward=anchor.position.sqrMagnitude<1?Vector2.right:anchor.position.normalized;
                    for(int step=0;step<4&&chosen<0;step++)
                    {
                        float span=Mathf.Max(anchor.size.x,anchor.size.y)*.5f+Mathf.Max(size.x,size.y)*.5f+12+step*6;
                        var p=new Vector2(Mathf.Round(anchor.position.x+outward.x*span),Mathf.Round(anchor.position.y+outward.y*span));
                        if(map.rooms.Any(o=>Expand(o.Bounds,3).Overlaps(Expand(new Rect(p-size*.5f,size),3))))continue;
                        chosen=host;spot=p;
                    }
                    if(chosen>=0)break;
                }
                if(chosen<0)throw new InvalidOperationException("곁방 배치 공간 부족");
                hosts.Add(chosen);map.rooms.Add(RiftTemplates.Instantiate(templates[n],n,spot,rotation,variant,map.obstacles));
            }
            if(!arenaBoss)RiftCrossRoutes.AddCentralRoom(map,ref layout,ref decor);
            foreach(var a in map.rooms)foreach(var b in map.rooms)if(a.index<b.index&&Expand(a.Bounds,3).Overlaps(Expand(b.Bounds,3)))throw new InvalidOperationException("방 간격 부족");
            if(!arenaBoss)RiftCrossRoutes.PreparePerimeterSockets(map,spine);
            // Assign separate physical ports to the predecessor and successor of every spine room.
            var previousPorts=new int[spine];var nextPorts=new int[spine];
            for(int n=0;n<spine;n++)
            {
                var r=map.rooms[n];float best=float.MinValue;
                int inward=map.roamingBoss&&r.doors.Count>=3?r.doors.OrderByDescending(d=>Vector2.Dot(d.direction,-r.position.normalized)).First().index:-1;
                foreach(var a in r.doors)foreach(var b in r.doors)
                {
                    if(a.index==b.index||a.index==inward||b.index==inward)continue;
                    float score=Vector2.Dot(a.direction,(map.rooms[(n+spine-1)%spine].position-r.position).normalized)+Vector2.Dot(b.direction,(map.rooms[(n+1)%spine].position-r.position).normalized);
                    if(score>best){best=score;previousPorts[n]=a.index;nextPorts[n]=b.index;}
                }
            }
            if(!arenaBoss)RiftCrossRoutes.RingPorts(map,spine,previousPorts,nextPorts);
            for(int n=0;n<spine;n++)Connect(map,n,(n+1)%spine,nextPorts[n],previousPorts[(n+1)%spine],false);
            for(int n=spine;n<count;n++)
            {
                var wing=map.rooms[n];var anchor=map.rooms[hosts[n-spine]];
                var dh=anchor.doors.Where(d=>d.corridor<0).OrderByDescending(d=>Vector2.Dot(d.direction,wing.position-anchor.position)).FirstOrDefault();
                var dw=wing.doors.OrderByDescending(d=>Vector2.Dot(d.direction,anchor.position-wing.position)).FirstOrDefault();
                if(dh==null||dw==null)throw new InvalidOperationException("곁방 출입구 없음");
                Connect(map,anchor.index,wing.index,dh.index,dw.index,false);
            }
            if(!arenaBoss)RiftCrossRoutes.Connect(map,spine);
            else
            {
            var edges=new List<Vector2Int>();for(int a=0;a<spine;a++)for(int b=a+1;b<spine;b++)if(b!=a+1&&!(a==0&&b==spine-1)&&a!=0&&b!=0)edges.Add(new Vector2Int(a,b));
            Shuffle(edges,ref layout);int extras=RandomStream.Range(ref layout,0,3);
            foreach(var edge in edges)
            {
                if(extras<=0)break;var a=map.rooms[edge.x];var b=map.rooms[edge.y];
                var da=a.doors.Where(d=>d.corridor<0).OrderByDescending(d=>Vector2.Dot(d.direction,b.position-a.position)).FirstOrDefault();
                var db=b.doors.Where(d=>d.corridor<0).OrderByDescending(d=>Vector2.Dot(d.direction,a.position-b.position)).FirstOrDefault();
                if(da==null||db==null)continue;
                Connect(map,a.index,b.index,da.index,db.index,true);extras--;
            }
            }

            AddJunctions(map);AssignRoles(map,spine);
            var first=map.rooms[0];map.start=first.Transform(-RiftTemplates.Get(first.templateId).size*.5f+Vector2.one*2);
            RiftOrganicGeometry.ShapeRooms(map);
            IntroductoryRift.ScaleGeometry(map,(map.introductory?IntroductoryRift.GeometryScale:1)*tuning.mapScale);
            if(map.version>=7)RiftDensity.Validate(map);
            var nav=new RiftNavigation(map);if(!nav.Reachable(map.start))throw new InvalidOperationException("시작점 접근 불가");
            foreach(var corridor in map.corridors)for(int n=1;n<corridor.points.Count;n++)
                if(!nav.TravelClear(corridor.points[n-1],corridor.points[n],1.2f))throw new InvalidOperationException($"Curved passage clearance failed: {corridor.index}/{n} {corridor.points[n-1]} to {corridor.points[n]}");
            foreach(var r in map.rooms)
            {
                foreach(var d in r.doors.Where(d=>d.corridor>=0))if(!nav.Reachable(d.position)||!nav.TravelClear(d.position-d.direction*3,d.position+d.direction*3,1.2f))throw new InvalidOperationException("출입구 통행 불가");
                if(!r.groupAnchors.All(nav.Reachable))throw new InvalidOperationException("무리 앵커 접근 불가");
            }
            if(arenaBoss)
            {
            var boss=map.rooms[map.bossRoom];foreach(var offset in new[]{Vector2.zero,new Vector2(-6,0),new Vector2(6,0),new Vector2(0,-6),new Vector2(0,6)})
            {Vector2 p=boss.position+offset;if(nav.CanLand(p,1.2f)&&nav.Reachable(p))map.bossPoints.Add(p);}
            if(map.bossPoints.Count!=5)throw new InvalidOperationException("보스 공간 부족");
            }
            uint encounter=map.encounterSeed;map.bossKind=SelectBossForStage(ref encounter,lastBoss,stage);
            PlaceEncounters(map,nav,stage,ref encounter);PlaceChests(map,nav,runId,stage,hero,tuning);RiftFieldContent.AddBodiesAndValidate(map);PlaceSeals(map,new RiftNavigation(map),stage);
            RiftObjectives.Place(map,new RiftNavigation(map));
            var shape=new StringBuilder();shape.Append(map.version).Append(':').Append(map.layoutSeed).Append('|');
            if(map.introductory)shape.Append("intro|");
            if(tuning.mapScale!=1||tuning.packSpread!=1||tuning.normalDensity!=1)shape.Append(tuning.mapScale.ToString(System.Globalization.CultureInfo.InvariantCulture)).Append(':').Append(tuning.packSpread.ToString(System.Globalization.CultureInfo.InvariantCulture)).Append(':').Append(tuning.normalDensity.ToString(System.Globalization.CultureInfo.InvariantCulture)).Append('|');
            foreach(var r in map.rooms)shape.Append($"{r.templateId}.{r.rotation}.{r.variant}|");foreach(var c in map.corridors)shape.Append($"{c.roomA}:{c.doorA}-{c.roomB}:{c.doorB}|");
            map.fingerprint=Derive(0,shape.ToString()).ToString("x8");RiftGates.Place(map);Validate(map);return map;
        }
        internal static void Connect(RiftLayout map,int a,int b,int da,int db,bool extra,Vector2? crossing=null,Vector2? axis=null)
        {
            var first=map.rooms[a].doors[da];var last=map.rooms[b].doors[db];
            float clearance=map.version>=7?3.6f:4.3f;
            var forbidden=map.rooms.Select(r=>Expand(r.Bounds,clearance)).ToArray();
            float limit=map.rooms.Max(r=>Mathf.Max(Mathf.Abs(r.position.x),Mathf.Abs(r.position.y)))+45;
            bool Pass(Vector2Int p)=>Mathf.Abs(p.x)<=limit&&Mathf.Abs(p.y)<=limit&&!forbidden.Any(r=>r.Contains(p));
            uint shapeSeed=Derive(map.layoutSeed,"passage:"+map.corridors.Count);
            List<Vector2> Leg(RiftDoor from,RiftDoor to,float fromCollar=-1,float toCollar=-1)
            {
                if(fromCollar<0)fromCollar=map.version>=7?5.5f:9;if(toCollar<0)toCollar=map.version>=7?5.5f:9;
                var route=RiftGridSearch.Find(Vector2Int.RoundToInt(from.position+from.direction*fromCollar),Vector2Int.RoundToInt(to.position+to.direction*toCollar),Pass);
                if(route==null)throw new InvalidOperationException("Passage connection failed.");
                return RiftOrganicGeometry.Connect(from,to,route,forbidden,shapeSeed,Mathf.Min(3,fromCollar),Mathf.Min(3,toCollar));
            }
            List<Vector2> points;
            if(crossing.HasValue)
            {
                var center=crossing.Value;var direction=axis.Value.normalized;
                // Preserve four straight arms without forcing a large empty core around the X.
                float arm=map.version>=7?4:9,neck=map.version>=7?1.5f:3;
                var incoming=new RiftDoor{position=center-direction*arm,direction=-direction};
                var outgoing=new RiftDoor{position=center+direction*arm,direction=direction};
                points=Leg(first,incoming,toCollar:neck);
                for(int n=1;n<=6;n++)points.Add(Vector2.Lerp(incoming.position,outgoing.position,n/6f));
                points.AddRange(Leg(outgoing,last,fromCollar:neck).Skip(1));
            }
            else points=Leg(first,last);
            var c=new RiftCorridor{index=map.corridors.Count,roomA=a,roomB=b,doorA=da,doorB=db,extra=extra,crossing=crossing.HasValue,points=points};
            RiftOrganicGeometry.SetWidths(c,shapeSeed);
            first.corridor=c.index;last.corridor=c.index;map.corridors.Add(c);
        }
        // Geometry comes from the template; this decides what each room is for in the run.
        // Legacy arena fixtures from tier 5 carry an objective. Seals sit in the rooms with the most
        // fighting, so following the objective is not a way around the combat.
        public const int ObjectivePathMeter=60;
        public const int ObjectiveFirstStage=5;
        public const int ObjectiveSeals=2;
        static void PlaceSeals(RiftLayout map,RiftNavigation nav,int stage)
        {
            if(stage<ObjectiveFirstStage||map.objective!=RiftObjectiveKind.Seals)return;
            int Meter(int room)=>map.spawns.Count(s=>s.room==room&&s.elite<0)+map.spawns.Count(s=>s.room==room&&s.elite>=0)*5;
            var eligible=map.rooms.Where(r=>r.role==RiftRoomRole.Passage||r.role==RiftRoomRole.Crossroad||r.role==RiftRoomRole.Wing).ToList();
            if(eligible.Count<ObjectiveSeals)throw new InvalidOperationException("봉인 배치 방 부족");
            var distances=GraphDistances(map,0);
            var chosen=new List<RiftRoom>();
            RiftRoom wing=null;
            foreach(var room in eligible.Where(r=>r.role!=RiftRoomRole.Wing).OrderBy(r=>distances[r.index]).ThenBy(r=>r.index))
            {if(chosen.Count>=ObjectiveSeals)break;if(!chosen.Contains(room))chosen.Add(room);}
            int ante0=map.rooms.First(r=>r.role==RiftRoomRole.Antechamber).index;
            int Path(List<RiftRoom> set)=>set.Select(r=>r.index).Concat(new[]{ante0}).Distinct().Sum(Meter);
            foreach(var heavy in eligible.OrderByDescending(r=>Meter(r.index)).ToList())
            {
                if(Path(chosen)>=ObjectivePathMeter)break;
                var lightest=chosen.Where(r=>!ReferenceEquals(r,wing)).OrderBy(r=>Meter(r.index)).FirstOrDefault();
                if(lightest==null||chosen.Contains(heavy)||Meter(heavy.index)<=Meter(lightest.index))break;
                chosen.Remove(lightest);chosen.Add(heavy);
            }
            foreach(var room in chosen)
            {
                Vector2? spot=null;
                foreach(var candidate in room.chestAnchors.Concat(room.groupAnchors))
                {
                    if(map.chests.Any(c=>c.room==room.index&&Vector2.Distance(c.position,candidate)<3))continue;
                    if(room.doors.Any(d=>Vector2.Distance(d.position,candidate)<3)||!nav.Reachable(candidate))continue;
                    spot=candidate;break;
                }
                if(spot==null)throw new InvalidOperationException("봉인 자리 없음");
                var guard=map.groups.Where(g=>g.room==room.index).OrderBy(g=>Vector2.Distance(g.position,spot.Value)).FirstOrDefault();
                if(guard==null)throw new InvalidOperationException("봉인 경비 무리 없음");
                var seal=new RiftSeal{id="seal:"+map.seals.Count,room=room.index,guardGroup=guard.index,position=spot.Value};
                foreach(var offset in new[]{new Vector2(1.3f,0),new Vector2(-1.3f,0),new Vector2(0,1.3f),new Vector2(0,-1.3f)})
                {var p=spot.Value+offset;if(nav.Walkable(p,.4f)&&nav.Reachable(p))seal.accessPoints.Add(p);}
                if(seal.accessPoints.Count==0)throw new InvalidOperationException("봉인 접근 지점 없음");
                map.seals.Add(seal);
            }
            int ante=map.rooms.First(r=>r.role==RiftRoomRole.Antechamber).index;
            int path=map.seals.Select(s=>s.room).Concat(new[]{ante}).Distinct().Sum(Meter);
            if(path<ObjectivePathMeter)throw new InvalidOperationException("목표 경로 전투량 부족");
            map.objective=RiftObjectiveKind.Seals;
        }
        static void AssignRoles(RiftLayout map,int spine)
        {
            foreach(var r in map.rooms)r.role=r.index>=spine&&!r.central?RiftRoomRole.Wing:RiftRoomRole.Passage;
            map.rooms[0].role=RiftRoomRole.Entrance;
            if(map.roamingBoss)
            {
                foreach(var room in map.rooms)if(room.role==RiftRoomRole.Passage&&(room.central||room.doors.Count(d=>d.corridor>=0)>=3))room.role=RiftRoomRole.Crossroad;
                return;
            }
            map.rooms[map.bossRoom].role=RiftRoomRole.BossArena;
            int before=(map.bossRoom+spine-1)%spine,after=(map.bossRoom+1)%spine;
            int ante=before!=0?before:after;
            if(ante==0||ante==map.bossRoom)throw new InvalidOperationException("전실 없음");
            map.rooms[ante].role=RiftRoomRole.Antechamber;
            foreach(var r in map.rooms)if(r.role==RiftRoomRole.Passage&&r.doors.Count(d=>d.corridor>=0)>=3)r.role=RiftRoomRole.Crossroad;
        }
        // Beat shaping. The totals stay at 110 normal and 8 elite spawns; what changes is where they
        // sit and how large a single pack is, so the run reads denser as it approaches the gate.
        static (int normal,int elite,int group)[] CombatBudget(RiftLayout map)
        {
            var result=new (int normal,int elite,int group)[map.rooms.Count];
            if(map.roamingBoss)
            {
                var distance=GraphDistances(map,0);
                // With only four introductory elites, reserve the second pack for a room
                // eligible for the sealed chest instead of spending both packs beside entry.
                var rooms=map.rooms.Where(r=>r.index!=0).OrderByDescending(r=>r.central).ThenByDescending(r=>map.introductory&&distance[r.index]>=2).ThenByDescending(r=>r.doors.Count(d=>d.corridor>=0)).ThenBy(r=>r.index).ToArray();
                int entry=Mathf.RoundToInt((map.introductory?5:10)*Density(map));
                int normal=NormalBudget(map)-entry,remainingElites=map.introductory?IntroductoryRift.EliteCount:8;result[0]=(entry,0,Mathf.CeilToInt(5*Density(map)));
                for(int i=0;i<rooms.Length;i++)
                {
                    int amount=normal/(rooms.Length-i);normal-=amount;int elite=Mathf.Min(2,remainingElites);remainingElites-=elite;
                    result[rooms[i].index]=(amount,elite,Mathf.CeilToInt((rooms[i].central?7:5)*Density(map)));
                }
                return result;
            }
            var distances=GraphDistances(map,0);
            var ante=map.rooms.First(r=>r.role==RiftRoomRole.Antechamber);
            var rest=map.rooms.Where(r=>r.index!=0&&!r.boss&&r.index!=ante.index).ToList();
            var line=rest.Where(r=>r.role!=RiftRoomRole.Wing).OrderBy(r=>distances[r.index]).ThenBy(r=>r.index).ToList();
            var front=line.Take(line.Count/2).Select(r=>r.index).ToList();
            float total=1.45f;foreach(var r in rest)total+=front.Contains(r.index)?.85f:1f;
            int given=0;
            foreach(var r in rest)
            {
                bool early=front.Contains(r.index);
                int n=Mathf.FloorToInt(92*(early?.85f:1f)/total);given+=n;
                result[r.index]=(n,0,early?5:7);
            }
            result[ante.index]=(92-given,0,8);
            result[0]=(10,0,5);result[map.bossRoom]=(8,0,6);
            int elites=8;
            foreach(var room in new[]{ante}.Concat(rest))
            {
                if(elites<=0)break;int e=Mathf.Min(2,elites);elites-=e;
                result[room.index]=(result[room.index].normal,e,result[room.index].group);
            }
            if(elites>0)throw new InvalidOperationException("정예 배치 공간 부족");
            if(Density(map)!=1)
            {
                int normalTotal=0;for(int i=0;i<result.Length;i++){result[i]=(Mathf.RoundToInt(result[i].normal*Density(map)),result[i].elite,Mathf.CeilToInt(result[i].group*Density(map)));normalTotal+=result[i].normal;}
                result[ante.index]=(result[ante.index].normal+NormalBudget(map)-normalTotal,result[ante.index].elite,result[ante.index].group);
            }
            return result;
        }
        static void AddJunctions(RiftLayout map)
        {
            foreach(var a in map.corridors)foreach(var b in map.corridors)
            {
                if(a.index>=b.index)continue;
                for(int i=1;i<a.points.Count;i++)for(int j=1;j<b.points.Count;j++)
                {
                    Vector2 p=a.points[i-1],r=a.points[i]-p,q=b.points[j-1],s=b.points[j]-q;
                    float cross=r.x*s.y-r.y*s.x;if(Mathf.Abs(cross)<.001f)continue;
                    float t=((q.x-p.x)*s.y-(q.y-p.y)*s.x)/cross,u=((q.x-p.x)*r.y-(q.y-p.y)*r.x)/cross;
                    if(t<0||t>1||u<0||u>1)continue;Vector2 point=p+t*r;
                    var junction=map.junctions.Find(x=>Vector2.Distance(x.position,point)<.1f);
                    if(junction==null){junction=new RiftJunction{position=point};map.junctions.Add(junction);}
                    if(!junction.corridors.Contains(a.index))junction.corridors.Add(a.index);if(!junction.corridors.Contains(b.index))junction.corridors.Add(b.index);
                }
            }
        }
        static void PlaceEncounters(RiftLayout map,RiftNavigation nav,int stage,ref uint rng)
        {
            var budget=CombatBudget(map);
            foreach(var room in map.rooms)
            {
                int normal=budget[room.index].normal,elite=budget[room.index].elite;
                int groupCount=Mathf.CeilToInt(normal/(float)budget[room.index].group);var anchors=room.groupAnchors.ToList();
                if(room.index==0)anchors=anchors.Where(p=>Vector2.Distance(p,map.start)>=10).OrderByDescending(p=>Vector2.Distance(p,map.start)).ToList();else Shuffle(anchors,ref rng);
                if(anchors.Count<groupCount)throw new InvalidOperationException(Loc.F("무리 공간 부족 {0}", room.templateId));
                for(int g=0;g<groupCount;g++)
                {
                    int members=normal/(groupCount-g);normal-=members;int groupElite=g==0?elite:0;
                    int archetype=room.index==0?0:(map.groups.Count+RandomStream.Range(ref rng,0,5))%5;
                    var group=new RiftGroup{index=map.groups.Count,room=room.index,position=anchors[g],archetype=new[]{"melee_ranged","melee_support","melee_charge","melee_ground","melee_burst"}[archetype]};map.groups.Add(group);
                    for(int n=0;n<members+groupElite;n++)
                    {
                        bool isElite=n>=members;int type=stage<ContentUnlocks.Rules.expandedEnemiesStage?0:n==members-1?new[]{2,4,1,3,5}[archetype]:0;
                        var spawn=new RiftSpawn{index=map.spawns.Count,group=group.index,room=room.index,kind=map.theme*6+type};
                        bool placed=false;
                        for(int attempt=0;attempt<48;attempt++)
                        {
                            float angle=(n+attempt*.37f)*Mathf.PI*2/(members+groupElite);float radial=(1.15f+(attempt/16)*.25f)*(map.liveOpsPackSpread>0?map.liveOpsPackSpread:1);
                            Vector2 p=group.position+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*radial;
                            if(!nav.Walkable(p)||room.boss&&Vector2.Distance(p,room.position)<8||room.index==0&&Vector2.Distance(p,map.start)<10||room.doors.Any(d=>Vector2.Distance(d.position,p)<3)||map.spawns.Any(s=>(s.position-p).sqrMagnitude<.65f*.65f))continue;
                            if(!nav.Reachable(p))continue;
                            spawn.position=p;placed=true;break;
                        }
                        if(!placed)throw new InvalidOperationException(Loc.F("적 배치 공간 부족 {0}", room.templateId));
                        if(isElite)
                        {
                            var allowed=new List<int>{0,1,3,5};if(members>=4)allowed.Add(4);
                            spawn.elite=allowed[RandomStream.Range(ref rng,0,allowed.Count)];spawn.traits.Add(spawn.elite);
                            if(ContentUnlocks.EliteTraits(stage)>1&&RandomStream.Unit(ref rng)<.5f)
                            {allowed.RemoveAll(t=>t==spawn.elite||t==0&&spawn.elite==1||t==1&&spawn.elite==0);spawn.traits.Add(allowed[RandomStream.Range(ref rng,0,allowed.Count)]);}
                        }
                        map.spawns.Add(spawn);group.spawns.Add(spawn.index);
                    }
                    if(groupElite==2&&RandomStream.Unit(ref rng)<.25f)
                    {
                        var pair=group.spawns.Select(i=>map.spawns[i]).Where(s=>s.elite>=0).ToArray();
                        pair[0].elitePartner=pair[1].index;pair[1].elitePartner=pair[0].index;
                        foreach(var s in pair){s.elite=2;s.traits.Clear();s.traits.Add(2);}
                    }
                }
            }
        }
        public static int[] GraphDistances(RiftLayout map,int source)
        {
            var distance=Enumerable.Repeat(999,map.rooms.Count).ToArray();distance[source]=0;var queue=new Queue<int>();queue.Enqueue(source);
            while(queue.Count>0){int p=queue.Dequeue();foreach(var c in map.corridors.Where(c=>c.roomA==p||c.roomB==p)){int next=c.roomA==p?c.roomB:c.roomA;if(distance[next]<=distance[p]+1)continue;distance[next]=distance[p]+1;queue.Enqueue(next);}}
            return distance;
        }
        static void PlaceChests(RiftLayout map,RiftNavigation nav,string runId,int stage,HeroClass hero,LiveOpsRiftSettings tuning)
        {
            uint rng=map.rewardSeed;var distances=GraphDistances(map,0);int count=map.rooms.Count-2;
            var eligible=map.rooms.Where(r=>r.index!=0&&distances[r.index]>=2).ToList();Shuffle(eligible,ref rng);
            var guard=eligible.FirstOrDefault(r=>map.groups.Any(g=>g.room==r.index&&g.spawns.Any(i=>map.spawns[i].elite>=0)));
            if(guard==null)throw new InvalidOperationException("봉인 상자 경비 방 없음");eligible.Remove(guard);eligible.Insert(0,guard);
            // A wing must never be an empty detour, so it takes a chest before any through room does.
            foreach(var wing in eligible.Where(r=>r.role==RiftRoomRole.Wing).ToList()){eligible.Remove(wing);eligible.Insert(Mathf.Min(1,eligible.Count),wing);}
            for(int n=0;n<count;n++)
            {
                var room=eligible[n%eligible.Count];var used=map.chests.Where(c=>c.room==room.index).ToList();if(used.Count>=2)throw new InvalidOperationException("상자 분산 불가");
                var anchors=room.chestAnchors.ToList();Shuffle(anchors,ref rng);RiftChest chest=null;
                if(map.introductory)anchors.AddRange(RiftFieldContent.SupplementaryAnchors(room,nav));
                foreach(var anchor in anchors)
                {
                    if(used.Any(c=>Vector2.Distance(c.position,anchor)<3)||Vector2.Distance(map.start,anchor)<(map.version>=7?26:45)||!nav.Reachable(anchor))continue;
                    if(map.introductory&&(!RiftFieldContent.ClearOfSpawns(map,anchor,new Vector2(.6f,.425f))||!RiftFieldContent.ClearOfPassages(map,anchor,new Vector2(.6f,.425f))))continue;
                    var points=new List<Vector2>();foreach(var offset in new[]{Vector2.up,Vector2.down,Vector2.left,Vector2.right}){Vector2 p=anchor+offset*1.15f;if(nav.Reachable(p)&&nav.TravelClear(p,anchor))points.Add(p);}
                    if(points.Count<2)continue;string id="chest-"+n.ToString("00");
                    chest=new RiftChest{id=id,requestId=runId+":"+id+":open",definitionId=n==0?"CH02":"CH01",room=room.index,position=anchor,accessPoints=points,openingPosition=points[0]};break;
                }
                if(chest==null)throw new InvalidOperationException(Loc.F("상자 접근 공간 부족 {0}", room.templateId));
                if(n==0)chest.guardGroup=map.groups.First(g=>g.room==room.index&&g.spawns.Any(i=>map.spawns[i].elite>=0)).index;
                map.chests.Add(chest);
            }
            int cursed=RiftFieldContent.Place(map,nav,runId,stage);
            int gold=LiveOpsConfig.Scale(Mathf.FloorToInt(.15f*tuning.ClearGold(stage)),tuning.goldMultiplier);map.chests[0].gold=gold/2;map.chests[0].materials=tuning.ClearMaterials(stage)==0?0:Mathf.Max(1,Mathf.FloorToInt(.2f*tuning.ClearMaterials(stage)));
            int rest=gold-map.chests[0].gold,weight=count-1+(cursed>=0?1:0),distributed=0;
            for(int n=1;n<count;n++){map.chests[n].gold=rest*(n==cursed?2:1)/weight;distributed+=map.chests[n].gold;}
            for(int n=1;distributed<rest;n++,distributed++)map.chests[n].gold++;
            int other=cursed>=0?cursed:RandomStream.Range(ref rng,1,count);
            foreach(int n in new[]{0,other})
            {
                float roll=RandomStream.Unit(ref rng);int rarity=n==0||n==cursed?(roll<.6f?1:roll<.98f?2:3):(roll<.8f?1:roll<.99f?2:3);
                int slot=RandomStream.Range(ref rng,0,8),level=RandomStream.Range(ref rng,Mathf.Max(1,stage-2),stage+3);
                map.chests[n].reward=Economy.CreateRiftItem(hero,slot,rarity,level,stage,ref rng,runId+":"+map.chests[n].id+":item");
            }
            map.rewardSeed=rng;
        }
        static float Density(RiftLayout map)=>map.liveOpsNormalDensity>0?map.liveOpsNormalDensity:1;
        static int NormalBudget(RiftLayout map)=>Mathf.RoundToInt((map.introductory?IntroductoryRift.NormalCount:110)*Density(map));
        public static void Validate(RiftLayout map)
        {
            if(map.rooms.Count<(map.roamingBoss?7:6)||map.rooms.Count>(map.roamingBoss?9:8)||map.rooms.Count(r=>r.boss)!=(map.roamingBoss?0:1)||map.corridors.Count<map.rooms.Count)throw new InvalidOperationException("지도 구조 불일치");
            if(map.introductory&&(!map.roamingBoss||!IntroductoryRift.Applies(map.contentStage)))throw new InvalidOperationException("Invalid introductory rift.");
            if(map.spawns.Count(s=>s.elite<0)!=NormalBudget(map)||map.spawns.Count(s=>s.elite>=0)!=(map.introductory?IntroductoryRift.EliteCount:8))throw new InvalidOperationException("전투 예산 불일치");
            if(map.chests.Count!=map.rooms.Count-2||map.chests.Count(c=>c.reward!=null)!=2)throw new InvalidOperationException("상자 예산 불일치");
            if((map.contentStage==0||map.contentStage>=ContentUnlocks.Rules.expandedEnemiesStage)&&map.spawns.Select(s=>s.kind).Distinct().Count()<4||map.groups.Select(g=>g.archetype).Distinct().Count()<3)throw new InvalidOperationException("적 다양성 부족");
            if(map.rooms.Any(r=>r.doors.Count(d=>d.corridor>=0)<(r.role==RiftRoomRole.Wing?1:2))||GraphDistances(map,0).Any(d=>d==999))throw new InvalidOperationException("끊어진 연결");
            if(map.rooms.Count(r=>r.role==RiftRoomRole.Entrance)!=1||map.rooms.Count(r=>r.role==RiftRoomRole.Antechamber)!=(map.roamingBoss?0:1)||map.rooms.Count(r=>r.role==RiftRoomRole.BossArena)!=(map.roamingBoss?0:1))throw new InvalidOperationException("방 역할 불일치");
            if(map.rooms.Any(r=>r.role==RiftRoomRole.Wing&&!map.chests.Any(c=>c.room==r.index)))throw new InvalidOperationException("빈 곁방");
            if(map.objective==RiftObjectiveKind.Seals&&(map.seals.Count!=ObjectiveSeals||map.seals.Select(s=>s.room).Distinct().Count()!=ObjectiveSeals))throw new InvalidOperationException("봉인 구성 불일치");
            if(map.seals.Any(s=>map.rooms[s.room].role==RiftRoomRole.Entrance||map.rooms[s.room].role==RiftRoomRole.BossArena||map.rooms[s.room].role==RiftRoomRole.Antechamber))throw new InvalidOperationException("봉인 방 역할 오류");
            if(map.roamingBoss)
            {
                if(map.bossRoom!=-1||map.bossPoints.Count!=0||map.objective!=RiftObjectiveKind.None||map.gates.Count!=0)throw new InvalidOperationException("A roaming boss must not reserve an arena or gates.");
                RiftCrossRoutes.Validate(map);RiftCirculation.Validate(map);
                if(map.version>=7)RiftDensity.Validate(map);
            }
            RiftGates.Validate(map);
            RiftObjectives.Validate(map);
        }
    }
}
