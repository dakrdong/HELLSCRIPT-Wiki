using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    [Serializable]public sealed class DeferredRiftGoal {public string id;public float until;}
    [Serializable]public sealed class EnemyObservation
    {public int id,kind,elite;public Vector2 position,velocity;public float seenAt;public bool investigated,motionSampled;}
    [Serializable]public sealed class RiftExplorationState
    {
        public int room=-1,enteredFrom=-1,failures,repaths;
        public string goalId="",chestId="",shrineId="",sealId="";
        public Vector2 goal,probePosition,entryDirection=Vector2.up;
        public float holdUntil,probeTime;
        public List<string> observed=new List<string>();
        public List<DeferredRiftGoal> deferred=new List<DeferredRiftGoal>();
        public List<EnemyObservation> enemies=new List<EnemyObservation>();
    }
    public static class RiftExploration
    {
        public static void Discover(RunState run,RiftNavigation nav)
        {
            if(run.layout.legacy)return;var state=run.exploration;int current=nav.RoomAt(run.position);
            if(current>=0&&state.room!=current)
            {
                state.enteredFrom=state.room;state.room=current;
                var port=run.layout.rooms[current].doors.Where(d=>d.corridor>=0).OrderBy(d=>(d.position-run.position).sqrMagnitude).FirstOrDefault();
                if(port!=null)state.entryDirection=-port.direction;
                if(!run.visited.Contains(current))run.visited.Add(current);
            }
            if(current>=0)
            {
                var room=run.layout.rooms[current];for(int n=0;n<room.groupAnchors.Count;n++)
                {string id=current+":observe:"+n;Vector2 p=room.groupAnchors[n];if(Vector2.Distance(run.position,p)<4&&nav.LineClear(run.position,p)&&!state.observed.Contains(id))state.observed.Add(id);}
            }
            foreach(var chest in run.layout.chests)if(!chest.discovered&&Vector2.Distance(run.position,chest.position)<=12&&nav.LineClear(run.position,chest.position))chest.discovered=true;
            foreach(var shrine in run.layout.shrines)if(!shrine.discovered&&Vector2.Distance(run.position,shrine.position)<=12&&nav.LineClear(run.position,shrine.position))shrine.discovered=true;
            foreach(var gate in run.layout.gates)if(!gate.discovered&&Vector2.Distance(run.position,gate.Outside)<=12&&nav.LineClear(run.position,gate.Outside))gate.discovered=true;
            foreach(var point in run.layout.offerings)if(!point.discovered&&Vector2.Distance(run.position,point.position)<=12&&nav.LineClear(run.position,point.position))point.discovered=true;
            var altar=run.layout.altar;if(altar!=null&&!altar.discovered&&Vector2.Distance(run.position,altar.position)<=12&&nav.LineClear(run.position,altar.position))altar.discovered=true;
            foreach(var drop in run.drops)if(!drop.discovered&&Vector2.Distance(run.position,drop.position)<=12&&nav.LineClear(run.position,drop.position))drop.discovered=true;
            foreach(var seen in state.enemies)if(run.time-seen.seenAt>.3f&&Vector2.Distance(run.position,seen.position)<2)seen.investigated=true;
        }
        public static void ObserveEnemies(RunState run,IEnumerable<EnemyState> visible)
        {
            var observations=run.exploration.enemies;
            observations.RemoveAll(o=>run.enemies.Any(e=>e.id==o.id&&e.dead));
            foreach(var e in visible)
            {
                var memory=observations.Find(o=>o.id==e.id);if(memory==null){memory=new EnemyObservation{id=e.id,kind=e.kind,elite=e.elite};observations.Add(memory);}
                float elapsed=run.time-memory.seenAt;Vector2 delta=e.position-memory.position;
                memory.velocity=memory.motionSampled&&elapsed>.00001f&&elapsed<=.4f&&delta.magnitude<=e.speed*1.5f*elapsed+.05f?delta/elapsed:Vector2.zero;
                var carrier=run.layout.carriers.Find(c=>c.enemyId==e.id);
                if(carrier!=null&&!carrier.completed){carrier.discovered=true;carrier.position=e.position;}
                memory.motionSampled=true;memory.position=e.position;memory.seenAt=run.time;memory.investigated=false;
            }
        }
        public static Vector2? RememberedTarget(RunState run,RiftNavigation nav)
        {
            float best=float.PositiveInfinity;Vector2? result=null;
            foreach(var memory in run.exploration.enemies.Where(o=>!o.investigated&&run.time-o.seenAt<=30))
            {
                if(!run.enemies.Any(e=>e.id==memory.id&&!e.dead))continue;
                float length=nav.Length(run.position,memory.position);if(length>=best)continue;best=length;result=memory.position;
            }
            return result;
        }
        // Closed gates exclude the arena from scouting as well as blocking every physical entrance.
        // Old maps without gate geometry retain their original saved passage behavior.
        public static bool GateClosed(RunState run)=>run.layout.objective!=RiftObjectiveKind.None&&!run.layout.gateOpen;
        public static Vector2 Goal(RunState run,RiftNavigation nav,ExplorationPolicy? exploration=null)
        {
            var policy=exploration??run.build.exploration;
            var s=run.exploration;
            bool valid=s.goalId.Contains(":observe:")&&!s.observed.Contains(s.goalId);
            if(s.goalId.StartsWith("exit:")&&int.TryParse(s.goalId.Substring(5),out int exit)&&exit<run.layout.corridors.Count)
            {var c=run.layout.corridors[exit];valid=!run.visited.Contains(c.roomA)||!run.visited.Contains(c.roomB);}
            if(s.goalId.StartsWith("audit:")&&int.TryParse(s.goalId.Substring(6),out int group))valid=run.enemies.Any(e=>!e.dead&&e.group==group);
            // 0.6 seconds is a minimum commitment, not a deadline for replacing a valid route.
            // Crossing an already visited room must not reverse an in-progress exit choice.
            if(valid&&Vector2.Distance(run.position,s.goal)>1&&!s.deferred.Any(d=>d.id==s.goalId&&d.until>run.time))return s.goal;
            s.deferred.RemoveAll(d=>d.until<=run.time);
            var candidates=new List<(string id,Vector2 p,float score)>();
            foreach(int index in run.visited)
            {
                if(GateClosed(run)&&index==run.layout.bossRoom)continue;
                var room=run.layout.rooms[index];for(int n=0;n<room.groupAnchors.Count;n++)
                {
                    string id=index+":observe:"+n;if(s.observed.Contains(id)||s.deferred.Any(d=>d.id==id))continue;
                    var p=room.groupAnchors[n];float length=nav.Length(run.position,p);if(float.IsInfinity(length))continue;
                    candidates.Add((id,p,length+(index==s.room?0:30)));
                }
            }
            if(candidates.Count==0)
            {
                var localExits=new List<(string id,Vector2 p,float turn,float length)>();
                foreach(var corridor in run.layout.corridors)
                {
                    bool a=run.visited.Contains(corridor.roomA),b=run.visited.Contains(corridor.roomB);if(a==b)continue;
                    int known=a?corridor.roomA:corridor.roomB,unknown=a?corridor.roomB:corridor.roomA;
                    if(GateClosed(run)&&unknown==run.layout.bossRoom)continue;
                    var port=run.layout.rooms[unknown].doors[a?corridor.doorB:corridor.doorA];var p=port.position-port.direction*2;
                    string id="exit:"+corridor.index;if(s.deferred.Any(d=>d.id==id))continue;
                    float length=nav.Length(run.position,p);if(float.IsInfinity(length))continue;
                    if(known==s.room&&(policy==ExplorationPolicy.LeftWall||policy==ExplorationPolicy.RightWall))
                    {
                        var direction=run.layout.rooms[known].doors[a?corridor.doorA:corridor.doorB].direction;
                        float angle=Vector2.SignedAngle(s.entryDirection,direction)*(policy==ExplorationPolicy.LeftWall?1:-1);
                        localExits.Add((id,p,180-angle,length));
                    }
                    candidates.Add((id,p,length));
                }
                // A wall-following turn is an order at the current junction, not a distance
                // penalty. Mixing degrees with metres made remote visited branches win.
                // Only backtrack to the nearest frontier when no local exit is eligible.
                if(localExits.Count>0)
                {
                    var next=localExits.OrderBy(c=>c.turn).ThenBy(c=>c.length).ThenBy(c=>c.id,StringComparer.Ordinal).First();
                    candidates.Clear();candidates.Add((next.id,next.p,next.length));
                }
            }
            if(candidates.Count==0)
            {
                // Recover missed observations from the saved spawn anchors, not hidden current positions.
                foreach(var missed in run.layout.groups.Where(g=>run.enemies.Any(e=>!e.dead&&!e.add&&e.group==g.index)))
                {string id="audit:"+missed.index;if(s.deferred.Any(d=>d.id==id))continue;float length=nav.Length(run.position,missed.position);if(!float.IsInfinity(length))candidates.Add((id,missed.position,length));}
            }
            if(candidates.Count==0)return run.position;
            var selected=candidates.OrderBy(c=>c.score).ThenBy(c=>c.id,StringComparer.Ordinal).First();
            if(s.goalId!=selected.id){s.failures=0;s.probeTime=run.time;s.probePosition=run.position;}
            s.goalId=selected.id;s.goal=selected.p;s.holdUntil=run.time+.6f;return s.goal;
        }
        public static bool TrackMovement(RunState run,RiftNavigation nav,Vector2 target,bool deliberateStop)
        {
            var s=run.exploration;
            if(deliberateStop||Vector2.Distance(run.position,target)<.75f){s.probeTime=run.time;s.probePosition=run.position;return true;}
            if(run.time-s.probeTime<2)return true;
            if(Vector2.Distance(run.position,s.probePosition)>=.5f){s.failures=0;s.probeTime=run.time;s.probePosition=run.position;return true;}
            nav.Repath();s.repaths++;s.failures++;s.probeTime=run.time;s.probePosition=run.position;
            if(s.failures<3)return true;
            if(s.goalId!="")s.deferred.Add(new DeferredRiftGoal{id=s.goalId,until=run.time+5});s.goalId="";s.holdUntil=0;s.failures=0;
            return false;
        }
    }
    public static class ChestRewards
    {
        public static bool Apply(AccountSave account,RunState run,string id,bool receipt=true)
        {
            var chest=run.layout.chests.SingleOrDefault(c=>c.id==id);if(chest==null)return false;
            if(chest.phase==ChestPhase.Opened)return true;
            if(chest.abandoned||chest.phase!=ChestPhase.Opening||chest.progress+.0001f<chest.Duration||run.meter<chest.Gate||Vector2.Distance(run.position,chest.position)>1.5f)return false;
            if(chest.guardGroup>=0&&run.enemies.Any(e=>e.group==chest.guardGroup&&!e.dead))return false;
            if(chest.definitionId=="CH03"&&!run.layout.events.Any(e=>e.chestId==chest.id&&e.phase==RiftEventPhase.Succeeded))return false;
            if(account.transactions.Any(r=>r.requestId==chest.requestId))return false;
            RiftEarnings.GrantGold(account,run,chest.gold);RiftEarnings.GrantMaterials(account,run,chest.materials);
            if(chest.reward!=null&&!string.IsNullOrEmpty(chest.reward.id))
            {chest.dropId=run.nextId++;run.drops.Add(new DropState{id=chest.dropId,position=chest.position,item=chest.reward});}
            chest.phase=ChestPhase.Opened;chest.progress=chest.Duration;
            if(receipt)account.transactions.Add(new EconomyReceipt{requestId=chest.requestId,operation="chest:"+run.id+":"+chest.id,committedUtc=DateTimeOffset.UtcNow.ToUnixTimeSeconds()});
            return true;
        }
    }
}
