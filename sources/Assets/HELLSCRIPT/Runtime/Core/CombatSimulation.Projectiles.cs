using System;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    public sealed partial class CombatSimulation
    {
        bool ConsumeShadow()
        {if(State.shadowTime<=0||State.shadowCharges<=0)return false;State.shadowCharges--;return true;}
        CombatProjectile Shot(HeroActionState action,Vector2 direction,float range,float speed,float radius,float coefficient,int element,int maxHits=1)
        {
            var p=new CombatProjectile{id=State.nextId++,actionId=action.id,skill=action.skill,origin=action.origin,position=action.origin,direction=direction.normalized,
                remaining=range,speed=speed,radius=radius,coefficient=coefficient,element=element,maxHits=maxHits,createdAt=State.time,snapshot=action.snapshot};
            State.projectiles.Add(p);return p;
        }
        void LaunchBasicProjectile(HeroActionState action)
        {
            var p=Shot(action,action.aim-action.origin,10,Hero.heroClass==HeroClass.Ranger?18:14,.2f,Hero.heroClass==HeroClass.Ranger?.9f:.8f,Hero.heroClass==HeroClass.Ranger?0:3);
            p.shadow=Hero.heroClass==HeroClass.Ranger&&ConsumeShadow();
        }
        void LaunchPierce(HeroActionState action)
        {
            var p=Shot(action,action.aim-action.origin,10,18,.3f,1.8f,0,Stats.passives[2]?7:5);p.shadow=ConsumeShadow();
            if(action.pierceBonus)
            {var echo=Shot(action,action.aim-action.origin,10,18,.3f,1.2f,0,5);echo.extra=true;echo.delay=.3f;}
        }
        void LaunchMulti(HeroActionState action)
        {
            bool narrow=Stats.specials.Contains("LA02"),shadow=ConsumeShadow();float angle=MultiShotHalfAngle(narrow);Vector2 direction=(action.aim-action.origin).normalized;
            State.projectileGroups.Add(new ProjectileGroup{actionId=action.id,maxPerVictim=narrow?3:1});
            foreach(float degrees in new[]{-angle,0,angle})
            {var p=Shot(action,RotateShot(direction,degrees),8,18,.2f,.8f,0);p.shadow=shadow;}
        }
        void LaunchFireball(HeroActionState action)
        {var p=Shot(action,action.aim-action.origin,Mathf.Min(10,Vector2.Distance(action.origin,action.aim)),14,.25f,2.1f,1);p.explosionRadius=2.5f;}
        void LaunchEnemyProjectile(EnemyState enemy)
        {
            State.projectiles.Add(new CombatProjectile{id=State.nextId++,actionId=State.nextId++,skill=-1,origin=enemy.position,position=enemy.position,direction=(enemy.aim-enemy.position).normalized,
                remaining=9,speed=10,radius=.2f,damage=enemy.attack,hostile=true,createdAt=State.time,casterId=enemy.id.ToString(),definitionId="ENEMY_ARROW"});
        }
        static float Entry(Vector2 a,Vector2 b,Vector2 point,float radius)
        {
            Vector2 delta=b-a,offset=a-point;float c=offset.sqrMagnitude-radius*radius;if(c<=0)return 0;
            float magnitude=delta.sqrMagnitude;if(magnitude<.0000001f)return float.PositiveInfinity;
            float dot=Vector2.Dot(offset,delta),discriminant=dot*dot-magnitude*c;if(discriminant<0)return float.PositiveInfinity;
            float t=(-dot-Mathf.Sqrt(discriminant))/magnitude;return t>=0&&t<=1?t:float.PositiveInfinity;
        }
        Vector2 ClipProjectile(Vector2 from,Vector2 end,float radius,out bool wall)
            =>Map.ProjectileEnd(from,end,radius,out wall);
        void TickProjectiles(float dt)
        {
            foreach(var p in State.projectiles.ToArray())
            {
                if(p.createdAt>=State.time-.00001f)continue;
                float movementTime=dt;
                if(p.delay>0){movementTime=Mathf.Max(0,dt-p.delay);p.delay=Mathf.Max(0,p.delay-dt);if(movementTime<.00001f)continue;}
                Vector2 from=p.position,to=ClipProjectile(from,from+p.direction*Mathf.Min(p.remaining,p.speed*movementTime),p.radius,out bool wall);
                bool remove=false;
                if(p.hostile)
                {
                    float t=Entry(from,to,State.position,p.radius+.45f);
                    if(!float.IsInfinity(t))
                    {
                        p.position=Vector2.Lerp(from,to,t);var group=State.projectileGroups.Find(g=>g.actionId==p.actionId);
                        if(group==null||!group.victims.Any(v=>v.id==-1))
                        {Hurt(p.damage,p.element,p.casterId,p.definitionId,p.actionId,p.id);if(p.pullDistance>0)PullHero(p);group?.victims.Add(new ProjectileVictim{id=-1,hits=1});}
                        remove=true;
                    }
                }
                else
                {
                    var targets=State.enemies.Where(e=>!e.dead&&!p.hitIds.Contains(e.id)).Select(e=>(enemy:e,t:Entry(from,to,e.position,p.radius+(e.boss?1.2f:.4f))))
                        .Where(x=>!float.IsInfinity(x.t)).OrderBy(x=>x.t).ThenBy(x=>x.enemy.id).ToArray();
                    foreach(var hit in targets)
                    {
                        if(hit.enemy.dead)continue;p.position=Vector2.Lerp(from,to,hit.t);
                        if(p.explosionRadius>0){FireballImpact(p);remove=true;break;}
                        var group=State.projectileGroups.Find(g=>g.actionId==p.actionId);var victim=group?.victims.Find(v=>v.id==hit.enemy.id);
                        bool allowed=group==null||victim==null||victim.hits<group.maxPerVictim;
                        if(allowed)
                        {
                            if(group!=null){if(victim==null){victim=new ProjectileVictim{id=hit.enemy.id};group.victims.Add(victim);}victim.hits++;}
                            bool poisoned=CombatEffects.OwnTrapPoison(hit.enemy,State.heroId);float extra=p.skill==6&&!p.extra&&Stats.specials.Contains("LA01")?Mathf.Min(.6f,p.hitIds.Count*.15f):0;
                            Hit(hit.enemy,p.coefficient,p.element,!p.extra,extra,p.snapshot,true,p.shadow,p.extra?"SAB4":SkillId(p.skill),p.actionId,p.id,p.extra?DamageKind.Set:p.skill<0?DamageKind.Basic:DamageKind.Direct,true,hit.enemy.position-p.direction);
                            if(p.skill<0)BasicResource(hit.enemy.id);
                            if(p.skill==6&&!p.extra&&poisoned&&Stats.SetPieces("SA")>=4&&!hit.enemy.dead)
                                ApplySetPoison(hit.enemy,p);
                        }
                        p.hitIds.Add(hit.enemy.id);if(p.hitIds.Count>=p.maxHits){remove=true;break;}
                        if(State.phase==RunPhase.Looting||State.phase==RunPhase.Cleared||State.phase==RunPhase.Failed)break;
                    }
                }
                if(!remove){p.remaining=Mathf.Max(0,p.remaining-Vector2.Distance(from,to));p.position=to;remove=wall||p.remaining<.0001f;if(remove&&p.explosionRadius>0)FireballImpact(p);}
                if(remove)State.projectiles.Remove(p);
                if(State.phase==RunPhase.Looting||State.phase==RunPhase.Cleared||State.phase==RunPhase.Failed)break;
            }
            State.projectileGroups.RemoveAll(g=>!State.projectiles.Any(p=>p.actionId==g.actionId));
        }
        void FireballImpact(CombatProjectile p)
        {
            var targets=AreaTargets(p.position,p.explosionRadius,default,360);
            foreach(var e in targets)
            {
                Hit(e,p.coefficient,1,true,p.snapshot.passives[0]&&targets.Length>=3?.15f:0,p.snapshot,definition:"M01",root:p.actionId,instance:p.id);
                ConsumeFrostMark(e,p);
            }
            if(Stats.specials.Contains("LM02"))AddGround(p.position,3.5f,.4f,.1f,Stats.damage*.7f,false,12,root:p.actionId);
            Visual?.Invoke(p.position,p.position,12,p.explosionRadius);
        }
        CombatTrap CreateTrap(HeroActionState action,Vector2 position,float duration)
        {
            if(!Map.CanLand(position,.1f))return null;
            var trap=new CombatTrap{id=State.nextId++,actionId=action.id,position=position,createdAt=State.time,duration=duration,radius=Stats.SetPieces("SA")>=2?3.5f:3,snapshot=action.snapshot,definitionId=action.skill==9?"LA03":"A03",casterId=State.heroId};State.traps.Add(trap);
            while(State.traps.Count(OwnRangerTrap)>2){var oldest=State.traps.Where(OwnRangerTrap).OrderBy(t=>t.id).First();State.traps.Remove(oldest);}
            return trap;
        }
        void TickTraps(float dt)
        {
            foreach(var trap in State.traps.ToArray())
            {
                if(trap.createdAt>=State.time-.00001f)continue;
                float elapsed=dt;
                if(trap.arm>0){elapsed=Mathf.Max(0,dt-trap.arm);trap.arm=Mathf.Max(0,trap.arm-dt);if(trap.arm>.00001f)continue;}
                if(!trap.triggered)
                {
                    var enemies=AreaTargets(trap.position,trap.radius,default,360);
                    if(enemies.Length>0)
                    {trap.triggered=true;trap.triggeredAt=State.time;trap.remaining=trap.duration;foreach(var e in enemies)ApplyStatus(e,StatusKind.Root,trap.definitionId,1.5f*(trap.snapshot.passives[3]?1.25f:1),trap.actionId);Log("TRAP_TRIGGERED","맹독 덫 발동");}
                    else {trap.wait-=elapsed;if(trap.wait<=.00001f)State.traps.Remove(trap);}continue;
                }
                trap.remaining=Mathf.Max(0,trap.remaining-dt);
            }
            foreach(var enemy in State.enemies.Where(e=>!e.dead).ToArray())
            {
                var strongest=State.traps.Where(t=>t.triggered&&Vector2.Distance(t.position,enemy.position)<=t.radius&&Map.LineClear(t.position,enemy.position))
                    .OrderByDescending(t=>t.snapshot.damage*(1+t.snapshot.bonus+t.snapshot.elements[4])).ThenBy(t=>t.id).FirstOrDefault();
                if(strongest==null){enemy.trapTick=.5f;continue;}
                ApplyStatus(enemy,StatusKind.Poisoned,strongest.definitionId,.55f,strongest.actionId);if(strongest.triggeredAt>=State.time-.00001f)continue;enemy.trapTick-=dt;
                if(enemy.trapTick<=.00001f){enemy.trapTick+=.5f;Hit(enemy,.3f,4,false,0,strongest.snapshot,false,definition:strongest.definitionId,root:strongest.actionId,instance:strongest.id,kind:DamageKind.Periodic);}
            }
            State.traps.RemoveAll(t=>t.triggered&&t.remaining<=.00001f);
        }
    }
}
