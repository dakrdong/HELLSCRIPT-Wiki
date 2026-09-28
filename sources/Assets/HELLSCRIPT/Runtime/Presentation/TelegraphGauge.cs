using System;
using System.Collections.Generic;
using UnityEngine;

namespace Hellscript
{
    // Windup gauge of hostile ground warnings. Progress is 0 when a warning appears and reaches 1 on the step the attack lands;
    // active zones and running charges read 1. The view fills each footprint to that fraction, so a warning shows where and
    // when it hits. The gauge runs on run time from the start of the warning to the moment it lands (now + delay), so it
    // keeps filling smoothly when an attack changes phase without landing (the devouring pull's second stage) and restarts
    // only when a followup begins after the first hit landed (the slam's second hit, the second charge). Later parts of
    // one attack (the chained circles a boss creates as hazards at release) start where the boss's own warning started,
    // because the gauge remembers the preparation of every action it saw.
    // EndFrame reports warnings that completed since the last frame (released, activated or followed up), not interrupted
    // ones, so the view can flash them. Presentation only: it reads RunState and never writes it.
    public sealed class TelegraphGauge
    {
        public struct Footprint
        {
            public string key,definition;public AttackShape shape;public Vector2 origin,end,direction;public float radius,inner,angle;public bool boss;
            public static Footprint Of(in EnemyThreat t,bool boss)=>new Footprint{key=t.key,definition=t.definition,shape=t.shape,origin=t.origin,end=t.end,direction=t.direction,radius=t.radius,inner=t.innerRadius,angle=t.angle,boss=boss};
        }
        sealed class Track{public Footprint footprint;public float start,hit,progress,delay;public bool seen;}
        // A warning at least this full that disappears with a release is a completed attack.
        public const float CompleteAt=.8f;
        readonly Dictionary<int,float> preparations=new Dictionary<int,float>();
        readonly Dictionary<string,Track> tracks=new Dictionary<string,Track>(StringComparer.Ordinal);
        readonly Stack<Track> spare=new Stack<Track>();
        readonly HashSet<int> live=new HashSet<int>();
        readonly List<int> staleIds=new List<int>();
        readonly List<string> staleKeys=new List<string>();

        // Progress of one warning, recorded for EndFrame.
        public float Observe(RunState run,in EnemyThreat threat,bool boss)
        {
            string key=threat.key??"";float now=run.time,hit=now+Mathf.Max(0,threat.delay);
            if(!tracks.TryGetValue(key,out var track))
            {track=spare.Count>0?spare.Pop():new Track();track.start=Start(run,key,now);track.hit=hit;track.progress=0;track.delay=threat.delay;tracks[key]=track;}
            // The previous warning of this key was due: a followup starts a gauge of its own.
            else if(threat.delay>0&&track.hit<=now-1e-4f)track.start=Start(run,key,now);
            track.hit=hit;float progress=threat.delay<=0?1:Fraction(now-track.start,hit-track.start);
            track.footprint=Footprint.Of(threat,boss);track.seen=true;
            // A followup rewinds the same key (the slam's second hit, the second charge): the first warning completed.
            if(track.delay>0&&track.progress>=CompleteAt&&progress<track.progress-.5f)Complete(track);
            // A delayed zone that just started burning, a charger that just set off.
            else if(track.delay>0&&threat.delay<=0)Complete(track);
            track.progress=progress;track.delay=threat.delay;
            return progress;
        }
        // The same fraction without recording anything (reports, tests).
        public float Progress(RunState run,in EnemyThreat threat)
        {
            if(threat.delay<=0)return 1;
            string key=threat.key??"";float now=run.time,start=tracks.TryGetValue(key,out var track)?track.start:Start(run,key,now);
            return Fraction(now-start,now+threat.delay-start);
        }
        // Run time the warning started: the action's current windup began, or a hazard was created (less the preparation
        // of the attack that created it at release, so its gauge continues the boss's).
        float Start(RunState run,string key,float now)
        {
            if(Number(key,"action-",out int id)||Number(key,"boss-",out id))
            {
                foreach(var e in run.enemies)
                {
                    var a=e.brain.action;if(a.id!=id||a.phase!=EnemyActionPhase.Preparing)continue;
                    preparations[id]=a.preparation;return now-Mathf.Max(0,a.preparation-a.remaining);
                }
            }
            else if(Number(key,"hazard-",out id)||Number(key,"shard-",out id))
            {
                foreach(var h in run.enemyHazards)
                    if(h.id==id)return h.createdAt-(preparations.TryGetValue(h.actionId,out float lead)?lead:0);
            }
            return now;
        }
        static float Fraction(float elapsed,float total)=>total>1e-5f?Mathf.Clamp01(elapsed/total):1;

        List<Footprint> completed;
        void Complete(Track track){completed?.Add(track.footprint);}
        // Call once per frame before the Observe calls; completed receives the warnings that finished this frame.
        public void BeginFrame(List<Footprint> completedThisFrame)
        {
            completed=completedThisFrame;completed?.Clear();
            foreach(var track in tracks.Values)track.seen=false;
        }
        // Call once per frame after the Observe calls.
        public void EndFrame(RunState run)
        {
            staleKeys.Clear();
            foreach(var pair in tracks)
            {
                var track=pair.Value;if(track.seen)continue;
                if(track.delay>0&&track.progress>=CompleteAt&&Landed(run,pair.Key))Complete(track);
                staleKeys.Add(pair.Key);
            }
            foreach(var key in staleKeys){spare.Push(tracks[key]);tracks.Remove(key);}
            live.Clear();
            foreach(var e in run.enemies)if(e.brain.action.phase!=EnemyActionPhase.Idle)live.Add(e.brain.action.id);
            foreach(var h in run.enemyHazards)live.Add(h.actionId);
            staleIds.Clear();foreach(int id in preparations.Keys)if(!live.Contains(id))staleIds.Add(id);
            foreach(int id in staleIds)preparations.Remove(id);
            completed=null;
        }
        // A warning that left: an action's released (not interrupted, not merely out of sight), a hazard's applied.
        static bool Landed(RunState run,string key)
        {
            if(Number(key,"action-",out int id)||Number(key,"boss-",out id))
            {
                for(int i=run.enemyEvents.Count-1;i>=0;i--)
                {
                    var ev=run.enemyEvents[i];if(ev.time<run.time-1)break;
                    if(ev.actionId==id&&ev.kind=="RELEASE")return true;
                }
                return false;
            }
            if(Number(key,"hazard-",out id)||Number(key,"shard-",out id))
            {foreach(var h in run.enemyHazards)if(h.id==id)return false;return true;}
            return false;
        }
        public void Clear(){tracks.Clear();preparations.Clear();completed=null;}
        static HashSet<string> shots;
        // Warnings of projectiles: when full, the shot leaves and flies, so the footprint does not flash as if it hit at once.
        public static bool Shot(string definition)
        {
            if(shots==null)
            {
                shots=new HashSet<string>(StringComparer.Ordinal){EnemyCombat.Id(2),EnemyCombat.Id(8),"N12_DEATH",BossCombat.Definition((int)BossAttack.Hook)};
                foreach(BossAttack kind in Enum.GetValues(typeof(BossAttack)))if(BossCombat.Volley(kind,out _))shots.Add(BossCombat.Definition((int)kind));
            }
            return definition!=null&&shots.Contains(definition);
        }
        public int Tracked=>tracks.Count;
        static bool Number(string key,string prefix,out int id)
        {
            id=0;if(!key.StartsWith(prefix,StringComparison.Ordinal))return false;
            int i=prefix.Length;while(i<key.Length&&key[i]>='0'&&key[i]<='9'){id=id*10+(key[i]-'0');i++;}
            return i>prefix.Length;
        }
    }
}
