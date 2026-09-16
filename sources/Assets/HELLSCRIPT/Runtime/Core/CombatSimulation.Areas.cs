using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    public sealed partial class CombatSimulation
    {
        static bool GroundSkill(int skill)=>skill==8||skill==13;
        bool OwnBlizzard(GroundEffect fx)=>!fx.hostile&&fx.kind==13&&(string.IsNullOrEmpty(fx.casterId)||fx.casterId==State.heroId);
        float GroundRadius(int skill)=>skill==8&&Stats.SetPieces("SA")>=2?3.5f:3;
        Vector2 GroundAim(Rule rule,EnemyState target,EnemyState[] seen=null,Vector2? origin=null)
        {
            Vector2 start=origin??State.position;
            if(rule.areaAim==AreaAim.Self||target==null)return start;
            if(rule.areaAim==AreaAim.ObservedPath)
            {
                var observation=State.exploration.enemies.Find(e=>e.id==target.id);
                if(observation==null||!observation.motionSampled||State.time-observation.seenAt<-.00001f||
                    State.time-observation.seenAt>.4f||!Finite(observation.velocity)||Immobilized(target))return target.position;
                Vector2 offset=observation.velocity*.75f;
                return Map.MoveDirect(target.position,target.position+offset,offset.magnitude,.1f);
            }
            if(rule.areaAim!=AreaAim.Dense)return target.position;
            seen??=State.enemies.Where(Perceived).OrderBy(e=>e.id).ToArray();float radius=GroundRadius(rule.skill),range=catalog.skills[rule.skill].range;
            var points=seen.Select(e=>e.position).ToList();
            for(int i=0;i<seen.Length;i++)for(int j=i+1;j<seen.Length;j++)if(Vector2.Distance(seen[i].position,seen[j].position)<=radius*2)points.Add((seen[i].position+seen[j].position)*.5f);
            return points.Where(p=>Vector2.Distance(start,p)<=range+.0001f&&Map.CanLand(p,.1f)&&Map.LineClear(start,p))
                .OrderByDescending(p=>seen.Count(e=>Vector2.Distance(p,e.position)<=radius&&Map.LineClear(p,e.position)))
                .ThenBy(p=>(p-target.position).sqrMagnitude).ThenBy(p=>(p-start).sqrMagnitude).ThenBy(p=>p.x).ThenBy(p=>p.y).DefaultIfEmpty(target.position).First();
        }
        readonly struct AreaWindow
        {
            public readonly GroundEffect effect;
            public readonly float start,end;
            public AreaWindow(GroundEffect effect,float start,float end){this.effect=effect;this.start=start;this.end=end;}
        }
        AreaWindow BlizzardWindow(GroundEffect fx,float dt)
        {
            float start=Mathf.Max(0,fx.createdAt-(State.time-dt))+Mathf.Max(0,fx.delay);
            return new AreaWindow(fx,Mathf.Min(dt,start),Mathf.Max(0,Mathf.Min(dt,start+Mathf.Max(0,fx.duration))));
        }
        void ApplySetPoison(EnemyState enemy,CombatProjectile shot)
        {
            var snap=shot.snapshot;
            if(enemy.setPoisonTime<=.00001f)
            {
                enemy.setPoisonTick=1;enemy.setPoisonSnapshot=snap;enemy.setPoisonInstance=State.nextId++;enemy.setPoisonRoot=shot.actionId;enemy.setPoisonCasterId=State.heroId;
                EffectEvent("SA4","CREATED",enemy.setPoisonInstance,shot.actionId,enemy.id,3);
            }
            else
            {
                var old=enemy.setPoisonSnapshot;
                if(old==null||snap.damage*(1+snap.bonus+snap.elements[4])>old.damage*(1+old.bonus+old.elements[4]))
                {enemy.setPoisonSnapshot=snap;enemy.setPoisonRoot=shot.actionId;}
                EffectEvent("SA4","REFRESHED",enemy.setPoisonInstance,enemy.setPoisonRoot,enemy.id,3);
            }
            enemy.setPoisonTime=3;
        }
        void ClearSetPoison(EnemyState enemy,string reason)
        {
            if(enemy.setPoisonTime>0||enemy.setPoisonInstance!=0)EffectEvent("SA4",reason,enemy.setPoisonInstance,enemy.setPoisonRoot,enemy.id);
            enemy.setPoisonTime=enemy.setPoisonTick=0;enemy.setPoisonSnapshot=null;enemy.setPoisonInstance=enemy.setPoisonRoot=0;enemy.setPoisonCasterId=null;
        }
        void TickSetPoison(EnemyState enemy,float dt)
        {
            if(enemy.setPoisonTime<=0)return;
            float active=Mathf.Min(dt,enemy.setPoisonTime);enemy.setPoisonTime=Mathf.Max(0,enemy.setPoisonTime-dt);enemy.setPoisonTick-=active;
            while(enemy.setPoisonTick<=.00001f&&!enemy.dead)
            {
                enemy.setPoisonTick+=1;
                if(enemy.setPoisonSnapshot!=null)Hit(enemy,.8f,4,false,0,enemy.setPoisonSnapshot,false,definition:"SA4",root:enemy.setPoisonRoot,instance:enemy.setPoisonInstance,kind:DamageKind.Periodic);
            }
            if(enemy.setPoisonTime<=.00001f||enemy.dead)ClearSetPoison(enemy,enemy.dead?"REMOVED":"EXPIRED");
        }
        void ClearFrostMark(EnemyState enemy,string reason,int root=0)
        {
            if(enemy.frostMarkTime>0||enemy.frostMarkId!=0)EffectEvent("SM4",reason,enemy.frostMarkId,root,enemy.id);
            enemy.frostMarkTime=0;enemy.frostMarkId=0;enemy.frostCasterId=null;
        }
        void TickFrostExposure(EnemyState enemy,float dt)
        {
            float blocked=Mathf.Max(enemy.frostCooldown,enemy.frostMarkTime);
            enemy.frostCooldown=Mathf.Max(0,enemy.frostCooldown-dt);
            if(enemy.frostMarkTime>0)
            {enemy.frostMarkTime=Mathf.Max(0,enemy.frostMarkTime-dt);if(enemy.frostMarkTime<=.00001f)ClearFrostMark(enemy,"EXPIRED");}
            if(Stats.SetPieces("SM")<4||blocked>=dt){enemy.exposure=0;return;}
            if(blocked>0)enemy.exposure=0;
            float cursor=Mathf.Max(0,blocked);bool granted=false;
            var windows=State.effects.Where(f=>OwnBlizzard(f)&&Vector2.Distance(f.position,enemy.position)<=f.radius&&Map.LineClear(f.position,enemy.position))
                .Select(f=>BlizzardWindow(f,dt)).Where(w=>w.end>w.start&&w.end>cursor).OrderBy(w=>w.start).ThenBy(w=>w.end).ToArray();
            foreach(var window in windows)
            {
                float start=Mathf.Max(cursor,window.start),end=window.end;if(end<=start)continue;
                if(start>cursor+.00001f)enemy.exposure=0;
                float needed=Mathf.Max(0,2-enemy.exposure),elapsed=end-start;
                if(elapsed+.00001f>=needed)
                {
                    enemy.frostMarkTime=4-Mathf.Max(0,dt-(start+needed));enemy.frostMarkId=State.nextId++;enemy.frostCasterId=State.heroId;enemy.exposure=0;granted=true;
                    EffectEvent("SM4","EXPOSURE_READY",enemy.frostMarkId,target:enemy.id,value:enemy.frostMarkTime);break;
                }
                enemy.exposure+=elapsed;cursor=end;
            }
            if(!granted&&cursor<dt-.00001f)enemy.exposure=0;
        }
        void ConsumeFrostMark(EnemyState enemy,CombatProjectile shot)
        {
            if(Stats.SetPieces("SM")<4||enemy.frostMarkTime<=0||enemy.frostCasterId!=State.heroId)return;
            int mark=enemy.frostMarkId;ClearFrostMark(enemy,"CONSUMED",shot.actionId);enemy.exposure=0;enemy.frostCooldown=3;
            Hit(enemy,1,2,false,0,shot.snapshot,definition:"SM4",root:shot.actionId,instance:mark,kind:DamageKind.Set);
        }
        void TickBlizzards(float dt)
        {
            var windows=State.effects.Where(OwnBlizzard).Select(f=>BlizzardWindow(f,dt)).Where(w=>w.end>w.start).ToArray();
            foreach(var window in windows)
            {
                var fx=window.effect;float active=window.end-window.start;
                if(fx.followsTarget&&Target!=null&&Perceived(Target)&&fx.moved<4)
                {var p=Map.MoveDirect(fx.position,Target.position,Mathf.Min(active,4-fx.moved));fx.moved+=Vector2.Distance(p,fx.position);fx.position=p;}
            }
            foreach(var enemy in State.enemies.Where(e=>!e.dead).ToArray())
            {
                var inside=windows.Where(w=>Vector2.Distance(w.effect.position,enemy.position)<=w.effect.radius&&Map.LineClear(w.effect.position,enemy.position)).ToArray();
                float cursor=0;var points=inside.SelectMany(w=>new[]{w.start,w.end}).Append(dt).Distinct().OrderBy(t=>t);
                foreach(float end in points)
                {
                    if(end<=cursor)continue;
                    var active=inside.Where(w=>w.start<=cursor+.00001f&&w.end>cursor+.00001f).OrderByDescending(w=>{var s=GroundSnapshot(w.effect);return w.effect.damage*(1+s.bonus+s.elements[2]);}).ThenBy(w=>w.effect.id).ToArray();
                    if(active.Length==0){enemy.blizzardTick=.5f;cursor=end;continue;}
                    var fx=active[0].effect;var snap=GroundSnapshot(fx);enemy.blizzardTick-=end-cursor;
                    while(enemy.blizzardTick<=.00001f&&!enemy.dead)
                    {enemy.blizzardTick+=.5f;Hit(enemy,fx.damage*.5f/Mathf.Max(.0001f,snap.damage),2,false,0,snap,false,definition:"M02",root:fx.rootCastId,instance:fx.id,kind:DamageKind.Periodic);}
                    cursor=end;
                }
                // Slow strength is independent of which overlapping field supplied the stronger damage tick.
                var activeAreas=new List<int>();
                foreach(var window in inside)
                {
                    var fx=window.effect;float remaining=fx.duration-(window.end-window.start);if(window.end<dt-.00001f||remaining<=.00001f)continue;
                    activeAreas.Add(fx.id);var snap=GroundSnapshot(fx);ApplyStatus(enemy,StatusKind.Slow,"M02",remaining,fx.rootCastId,SlowStrength(snap),fx.id);
                }
                enemy.statuses.RemoveAll(s=>s.areaId>0&&s.casterId==State.heroId&&s.definitionId=="M02"&&!activeAreas.Contains(s.areaId));
            }
            foreach(var fx in State.effects.Where(f=>!f.hostile&&f.kind==13).ToArray())
            {
                var window=BlizzardWindow(fx,dt);float lived=Mathf.Clamp(State.time-fx.createdAt,0,dt);fx.delay=Mathf.Max(0,fx.delay-lived);fx.duration=Mathf.Max(0,fx.duration-(window.end-window.start));
                if(fx.duration<=.00001f)State.effects.Remove(fx);
            }
        }
        void ResetUnavailableAreaEffects()
        {
            foreach(var enemy in State.enemies)
            {
                if(Stats.SetPieces("SA")<4)ClearSetPoison(enemy,"REMOVED");
                if(Stats.SetPieces("SM")<4){ClearFrostMark(enemy,"REMOVED");enemy.exposure=enemy.frostCooldown=0;}
            }
        }
    }
}
