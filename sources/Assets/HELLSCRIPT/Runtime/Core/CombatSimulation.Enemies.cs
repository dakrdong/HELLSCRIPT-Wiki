using System;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    public sealed partial class CombatSimulation
    {
        void EnemyEvent(EnemyState e,string kind,string definition=null,int action=0,float value=0,Vector2? aim=null)
        {
            State.enemyEvents.Add(new EnemyCombatEvent{enemyId=e.id,actionId=action,definitionId=definition??(e.boss?"BOSS0"+(e.pattern+1):EnemyCombat.Id(e.kind)),kind=kind,time=State.time,value=value,position=e.position,aim=aim??e.aim});
            if(State.enemyEvents.Count>400)State.enemyEvents.RemoveAt(0);
        }
        void EnemyMode(EnemyState e,string state)
        {if(e.brain.state==state)return;e.brain.state=state;EnemyEvent(e,"STATE:"+state);}
        static void InitializeEnemyBrain(EnemyState e,RunState run)
        {
            if(e.brain.initialized)return;var b=e.brain;b.initialized=true;b.bornAt=run.time;
            var facing=run.position-e.position;if(facing.sqrMagnitude>.00001f)b.facing=facing.normalized;
            for(int n=0;n<6;n++)b.traitCooldowns[n]=Mathf.Max(e.cooldown,n==0?5:n==1?8:6);
        }
        void TickEnemyTimers(float dt)
        {
            State.enemySlowTime=Mathf.Max(0,State.enemySlowTime-dt);
            foreach(var e in State.enemies.Where(e=>!e.dead))
            {
                InitializeEnemyBrain(e,State);e.brain.rearWindow=Mathf.Max(0,e.brain.rearWindow-dt);
                if(e.boss)TickBossTimers(e,dt);else e.cooldown=Mathf.Max(0,e.cooldown-dt);
                for(int n=0;n<6;n++)e.brain.traitCooldowns[n]=Mathf.Max(0,e.brain.traitCooldowns[n]-dt);
                int rage=EnemyCombat.Trait(e,5)?Mathf.Min(5,Mathf.FloorToInt((State.time-e.brain.bornAt+.00001f)/10)):0;
                if(rage!=e.brain.rageStacks){e.brain.rageStacks=rage;EnemyEvent(e,"RAGE","E06",value:rage);}
            }
            State.enemyCorpses.RemoveAll(c=>State.time-c.createdAt>10);
        }
        EnemyState AuraSource(EnemyState target)=>State.enemies.Where(e=>e!=target&&!e.dead&&!e.boss&&e.kind==10&&Vector2.Distance(e.position,target.position)<=4&&Map.LineClear(e.position,target.position)).OrderBy(e=>e.id).FirstOrDefault();
        float EnemyAttackValue(EnemyState enemy)=>enemy.attack*(1+.1f*enemy.brain.rageStacks+(AuraSource(enemy)!=null?.2f:0));
        void RefreshEnemyAuras()
        {
            foreach(var e in State.enemies.Where(e=>!e.dead))
            {int source=AuraSource(e)?.id??-1;if(e.brain.auraSource==source)continue;e.brain.auraSource=source;EnemyEvent(e,source<0?"AURA_REMOVED":"AURA_APPLIED","N11",value:source);}
        }
        Vector2 EnemyPredictedAim(EnemyState enemy)
        {
            var samples=enemy.brain.observations;var first=samples.FirstOrDefault(s=>s.time>=State.time-.5f-.00001f);
            if(first==null||State.time-first.time<=.00001f)return State.position;
            Vector2 offset=Vector2.ClampMagnitude((State.position-first.position)/(State.time-first.time),4);
            return Map.MoveDirect(State.position,State.position+offset,offset.magnitude,.1f);
        }
        void BeginEnemyAction(EnemyState enemy,float? remaining=null)
        {
            var b=enemy.brain;var def=EnemyCombat.Attack(enemy.kind);var a=new EnemyActionState{id=State.nextId++,kind=enemy.kind,phase=EnemyActionPhase.Preparing,origin=enemy.position,aim=remaining.HasValue?enemy.aim:State.position,
                preparation=remaining??def.preparation,remaining=remaining??def.preparation,remainingCharges=enemy.kind==7?2:enemy.kind==1?1:0};
            if(enemy.kind==9&&!remaining.HasValue)a.aim=EnemyPredictedAim(enemy);
            a.direction=(a.aim-a.origin).normalized;if(a.direction.sqrMagnitude<.00001f)a.direction=b.facing;
            if(enemy.kind==1||enemy.kind==7)a.aim=Map.MoveDirect(a.origin,a.origin+a.direction*8,8,.4f);
            b.action=a;b.facing=a.direction;enemy.aim=a.aim;enemy.windup=a.remaining;enemy.cooldown=Mathf.Max(enemy.cooldown,def.cooldown);
            EnemyMode(enemy,"공격 준비");EnemyEvent(enemy,"PREPARE",action:a.id,value:a.remaining,aim:a.aim);
        }
        void InterruptEnemyAction(EnemyState enemy,string reason)
        {
            var a=enemy.brain.action;if(a.phase==EnemyActionPhase.Idle)return;
            EnemyEvent(enemy,"INTERRUPTED:"+reason,EnemyCombat.Id(a.kind),a.id);enemy.brain.action=new EnemyActionState();enemy.windup=0;EnemyMode(enemy,"제어로 중단");
        }
        void FinishEnemyAction(EnemyState enemy)
        {var a=enemy.brain.action;EnemyEvent(enemy,"ACTION_END",EnemyCombat.Id(a.kind),a.id);enemy.brain.action=new EnemyActionState();enemy.windup=0;EnemyMode(enemy,"재사용 대기");}
        void EnemyProjectiles(EnemyState e,int action,Vector2 origin,Vector2 direction,float damage,string definition,bool fan=false,float spread=10,float distance=9,float speed=10)
        {
            if(fan)State.projectileGroups.Add(new ProjectileGroup{actionId=action,maxPerVictim=1});
            foreach(float angle in fan?new[]{-spread,0,spread}:new[]{0f})
                State.projectiles.Add(new CombatProjectile{id=State.nextId++,actionId=action,hostile=true,casterId=e.id.ToString(),definitionId=definition,origin=origin,position=origin,direction=EnemyCombat.Rotate(direction,angle),
                    remaining=distance,speed=speed,radius=.2f,damage=damage,createdAt=State.time});
        }
        void CreateEnemyHazard(EnemyState enemy,string definition,Vector2 position,float delay,float duration,float damage,int element,float radius=2.5f,float inner=0,int action=0,float slow=0,bool shards=false)
        {
            State.enemyHazards.Add(new EnemyHazard{id=State.nextId++,actionId=action==0?State.nextId++:action,enemyId=enemy.id,definitionId=definition,position=position,end=position,direction=enemy.brain.facing,
                shape=inner>0?AttackShape.Ring:AttackShape.Circle,createdAt=State.time,delay=delay,duration=duration,interval=duration>0?.5f:0,tick=.5f,damage=damage,element=element,radius=radius,innerRadius=inner,heroSlow=slow,shardBurst=shards});
            EnemyEvent(enemy,"HAZARD_CREATED",definition,State.enemyHazards.Last().actionId,delay,position);
        }
        void ReleaseEnemyAction(EnemyState e)
        {
            var a=e.brain.action;float attack=EnemyAttackValue(e);EnemyEvent(e,"RELEASE",EnemyCombat.Id(a.kind),a.id);
            if(a.kind==1||a.kind==7){a.phase=EnemyActionPhase.Charging;a.damage=attack*1.5f;a.moved=0;a.hitHero=false;e.windup=0;EnemyMode(e,"돌진");return;}
            if(a.kind==2||a.kind==8)EnemyProjectiles(e,a.id,a.origin,a.direction,attack,EnemyCombat.Id(a.kind),a.kind==8);
            else if(a.kind==3||a.kind==9)CreateEnemyHazard(e,EnemyCombat.Id(a.kind),a.aim,0,a.kind==3?4:3,attack*(a.kind==3?.25f:.3f),a.kind==3?4:1,action:a.id);
            else if(a.kind==4)
            {
                foreach(var ally in State.enemies.Where(x=>!x.dead&&Vector2.Distance(e.position,x.position)<=3&&Map.LineClear(e.position,x.position)))
                {float healed=Mathf.Min(ally.maxHealth-ally.health,ally.maxHealth*.15f);ally.health+=healed;if(healed>0)EnemyEvent(e,"HEAL",action:a.id,value:healed,aim:ally.position);}
            }
            else
            {
                var shape=new EnemyThreat("",EnemyCombat.Id(a.kind),AttackShape.Sector,a.origin,a.aim,a.direction,2.5f);
                if(EnemyCombat.Contains(shape,State.position)&&Map.LineClear(a.origin,State.position))Hurt(attack,0,e.id.ToString(),EnemyCombat.Id(a.kind),a.id,a.id);
                if(a.kind==6)e.brain.rearWindow=.8f;
            }
            FinishEnemyAction(e);
        }
        void TickEnemyAction(EnemyState e,float dt)
        {
            var a=e.brain.action;
            if(a.phase==EnemyActionPhase.Preparing)
            {a.remaining=Mathf.Max(0,a.remaining-dt);e.windup=a.remaining;if(a.remaining<=.00001f)ReleaseEnemyAction(e);return;}
            if(a.phase!=EnemyActionPhase.Charging)return;
            Vector2 from=e.position,to=Map.MoveDirect(from,a.aim,Mathf.Min(12*dt,Mathf.Max(0,8-a.moved)),.4f);float moved=Vector2.Distance(from,to);a.moved+=moved;e.position=to;
            if(!a.hitHero&&!float.IsInfinity(Entry(from,to,State.position,1.05f))&&Map.LineClear(from,State.position))
            {a.hitHero=true;Hurt(a.damage,0,e.id.ToString(),EnemyCombat.Id(a.kind),a.id,a.id);}
            if(moved>.00001f&&Vector2.Distance(to,a.aim)>.01f&&a.moved<8-.0001f)return;
            if(--a.remainingCharges>0)
            {
                a.origin=e.position;a.direction=(e.lastSeenPosition-e.position).normalized;if(a.direction==Vector2.zero)a.direction=e.brain.facing;
                a.aim=Map.MoveDirect(a.origin,a.origin+a.direction*8,8,.4f);a.phase=EnemyActionPhase.Preparing;a.preparation=a.remaining=.6f;e.aim=a.aim;e.windup=.6f;e.brain.facing=a.direction;
                EnemyMode(e,"두 번째 돌진 준비");EnemyEvent(e,"FOLLOWUP_PREPARE",EnemyCombat.Id(a.kind),a.id,.6f,a.aim);
            }
            else FinishEnemyAction(e);
        }
        void TickEnemyTraits(EnemyState e,bool visible)
        {
            if(!visible)return;
            var b=e.brain;
            if(EnemyCombat.Trait(e,0)&&b.traitCooldowns[0]<=.00001f)
            {CreateEnemyHazard(e,"E01",State.position,1,3,EnemyAttackValue(e)*.25f,1,2);b.traitCooldowns[0]=5;}
            if(EnemyCombat.Trait(e,1)&&b.traitCooldowns[1]<=.00001f)
            {CreateEnemyHazard(e,"E02",State.position,1.5f,0,EnemyAttackValue(e)*1.5f,2,4,2,slow:.35f);b.traitCooldowns[1]=8;}
            if(EnemyCombat.Trait(e,4)&&b.traitCooldowns[4]<=.00001f)
            {
                var corpse=State.enemyCorpses.Where(c=>c.consumedBy<0&&Vector2.Distance(e.position,c.position)<=6&&Map.LineClear(e.position,c.position)).OrderBy(c=>(c.position-e.position).sqrMagnitude).ThenBy(c=>c.id).FirstOrDefault();
                if(corpse!=null){corpse.consumedBy=e.id;CreateEnemyHazard(e,"E05",corpse.position,1.2f,0,EnemyAttackValue(e)*1.5f,5);EnemyEvent(e,"CORPSE_CONSUMED","E05",value:corpse.id);b.traitCooldowns[4]=6;}
            }
        }
        void TickEnemies(float dt)
        {
            RefreshEnemyAuras();
            foreach(var e in State.enemies.ToArray())
            {
                if(e.dead)continue;e.mark-=dt;e.slow=Mathf.Max(0,e.slow-dt);e.root=Mathf.Max(0,e.root-dt);
                if(e.poison>0){e.poison-=dt;if(e.poisonDamage>0)Hit(e,e.poisonDamage*dt/Mathf.Max(.0001f,Stats.damage),4,false,canCrit:false,definition:"LEGACY_POISON",kind:DamageKind.Periodic);if(e.dead)continue;}
                bool hard=CombatEffects.Has(e,StatusKind.Stun)||CombatEffects.Has(e,StatusKind.Freeze)||e.bossControl.staggered>0;
                if(hard){if(e.boss)InterruptBossAction(e,"보스 제압");else InterruptEnemyAction(e,"기절·빙결");e.stun=Mathf.Max(0,e.stun-dt);e.freeze=Mathf.Max(0,e.freeze-dt);continue;}
                if(e.boss){TickBoss(e,dt);continue;}
                var b=e.brain;float distance=Vector2.Distance(e.position,State.position);bool visible=distance<=14&&Map.LineClear(e.position,State.position);
                if(visible)
                {
                    e.lastSeenTime=State.time;e.lastSeenPosition=State.position;b.observations.Add(new MotionObservation{time=State.time,position=State.position});
                    b.observations.RemoveAll(s=>s.time<State.time-.5f-.00001f);
                }
                else b.observations.Clear();
                TickEnemyTraits(e,visible);
                // Old saves can contain a scalar preparation but no explicit enemy action.
                if(b.action.phase==EnemyActionPhase.Idle&&e.windup>0)BeginEnemyAction(e,e.windup);
                if(b.action.phase!=EnemyActionPhase.Idle)
                {
                    if(b.action.phase==EnemyActionPhase.Charging&&CombatEffects.Has(e,StatusKind.Root)){InterruptEnemyAction(e,"속박");continue;}
                    TickEnemyAction(e,dt);continue;
                }
                if(!visible&&State.time-e.lastSeenTime>4){EnemyMode(e,"목표 상실");continue;}
                var def=EnemyCombat.Attack(e.kind);
                bool line=visible&&((e.kind!=2&&e.kind!=8)||Map.ProjectileClear(e.position,State.position,.2f));
                if((distance>def.range||!line)&&!Immobilized(e))
                {
                    Vector2 target=e.lastSeenPosition;
                    if(visible&&!line)
                    {
                        var candidates=new[]{e.position+Vector2.right*2,e.position+Vector2.left*2,e.position+Vector2.up*2,e.position+Vector2.down*2};
                        target=candidates.Where(p=>Map.CanLand(p,.4f)&&Map.ProjectileClear(p,e.lastSeenPosition,.2f)).OrderBy(p=>(p-e.position).sqrMagnitude).FirstOrDefault();if(target==Vector2.zero)target=e.lastSeenPosition;
                    }
                    var before=e.position;e.position=Map.Move(e.position,target,e.speed*(1-SlowRatio(e))*dt,e.id+1,State.time,.4f);if(e.position!=before&&b.rearWindow<=0)b.facing=(e.position-before).normalized;EnemyMode(e,"추적");
                }
                else if(b.rearWindow<=0&&visible&&b.action.phase==EnemyActionPhase.Idle){var d=State.position-e.position;if(d.sqrMagnitude>.00001f)b.facing=d.normalized;}
                if(e.kind==10){EnemyMode(e,"공격 강화 오라");continue;}
                if(e.kind==4&&!State.enemies.Any(x=>!x.dead&&x.health<x.maxHealth&&Vector2.Distance(e.position,x.position)<=3&&Map.LineClear(e.position,x.position)))continue;
                if(e.cooldown<=.00001f&&distance<=def.range&&line)BeginEnemyAction(e);
            }
        }
        void RegisterEnemyDeath(EnemyState e)
        {
            if(e.boss)InterruptBossAction(e,"사망");else InterruptEnemyAction(e,"사망");
            if(!e.boss&&!e.add)State.enemyCorpses.Add(new EnemyCorpse{id=State.nextId++,enemyId=e.id,position=e.position,createdAt=State.time});
            if(!e.boss&&e.kind==5)CreateEnemyHazard(e,"N06_DEATH",e.position,1,0,EnemyAttackValue(e)*2,0);
            if(!e.boss&&e.kind==11)CreateEnemyHazard(e,"N12_DEATH",e.position,.9f,0,EnemyAttackValue(e)*1.2f,0,shards:true);
        }
        void TickEnemyHazards(float dt)
        {
            foreach(var h in State.enemyHazards.ToArray())
            {
                if(h.createdAt>=State.time-.00001f)continue;
                float active=Mathf.Max(0,dt-h.delay);h.delay=Mathf.Max(0,h.delay-dt);if(h.delay>.00001f)continue;
                if(h.shardBurst)
                {
                    var source=State.enemies.Find(e=>e.id==h.enemyId)??new EnemyState{id=h.enemyId};EnemyProjectiles(source,h.actionId,h.position,h.direction,h.damage,h.definitionId,true,30,8,12);
                    State.enemyHazards.Remove(h);continue;
                }
                if(h.interval<=0)
                {ApplyEnemyHazard(h);State.enemyHazards.Remove(h);continue;}
                float applied=Mathf.Min(active,h.duration);h.duration=Mathf.Max(0,h.duration-applied);h.tick-=applied;
                while(h.tick<=.00001f){h.tick+=h.interval;ApplyEnemyHazard(h);}
                if(h.duration<=.00001f)State.enemyHazards.Remove(h);
            }
            ClearFinishedBossRefuges();
        }
        void ApplyEnemyHazard(EnemyHazard h)
        {
            bool inside=EnemyCombat.Contains(EnemyCombat.HazardThreat(h),State.position),visible=Map.LineClear(h.position,State.position);
            ObserveHazardImpact(h,inside,visible);
            if(!inside||!visible)return;
            Hurt(h.damage,h.element,h.enemyId.ToString(),h.definitionId,h.actionId,h.id,h.interval>0?DamageKind.Periodic:DamageKind.Direct);
            if(h.heroSlow>0)State.enemySlowTime=Mathf.Max(State.enemySlowTime,2);
        }
        bool EnemyThreatAt(Vector2 point,float warning,bool activeOnly=false)
            =>EnemyCombat.Threats(State,Map).Any(t=>t.delay<=(activeOnly?0:warning)+.00001f&&Vector2.Distance(State.position,t.origin)<=14+t.radius&&Map.LineClear(State.position,t.origin)&&EnemyCombat.Contains(t,point)&&Map.LineClear(t.origin,point));
    }
}
