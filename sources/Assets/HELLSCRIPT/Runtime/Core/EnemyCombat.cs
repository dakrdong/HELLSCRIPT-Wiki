using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    public enum EnemyActionPhase { Idle, Preparing, Charging, Recovering }
    public enum AttackShape { Circle, Ring, Sector, Line }
    [Serializable] public sealed class MotionObservation {public float time;public Vector2 position;}
    [Serializable] public sealed class EnemyActionState
    {
        public int id,kind,remainingCharges;
        public EnemyActionPhase phase;
        public Vector2 origin,aim,direction;
        public float remaining,preparation,damage,moved;
        public bool hitHero,empowered,released;
        public List<Vector2> points=new List<Vector2>();
    }
    [Serializable] public sealed class EnemyBrain
    {
        public bool initialized;
        public Vector2 facing=Vector2.up;
        public float bornAt,rearWindow;
        public int auraSource=-1,rageStacks;
        public string state="대기";
        public float[] traitCooldowns=new float[6];
        public EnemyActionState action=new EnemyActionState();
        public BossPatternState boss=new BossPatternState();
        public List<MotionObservation> observations=new List<MotionObservation>();
    }
    [Serializable] public sealed class EnemyCombatEvent
    {
        public int enemyId,actionId;
        public float time,value;
        public string definitionId,kind;
        public Vector2 position,aim;
    }
    [Serializable] public sealed class EnemyCorpse
    {
        public int id,enemyId,consumedBy=-1;
        public float createdAt;
        public Vector2 position;
    }
    [Serializable] public sealed class EnemyHazard
    {
        public int id,actionId,enemyId,element;
        public string definitionId;
        public AttackShape shape;
        public Vector2 position,end,direction;
        public float createdAt,delay,duration,interval,tick,damage,radius,innerRadius,angle,heroSlow;
        public bool shardBurst;
    }
    public readonly struct EnemyAttackDefinition
    {
        public readonly float range,preparation,cooldown;
        public EnemyAttackDefinition(float range,float preparation,float cooldown){this.range=range;this.preparation=preparation;this.cooldown=cooldown;}
    }
    public readonly struct EnemyThreat
    {
        public readonly string key,definition;
        public readonly Vector2 origin,end,direction;
        public readonly AttackShape shape;
        public readonly float radius,innerRadius,angle,delay;
        public EnemyThreat(string key,string definition,AttackShape shape,Vector2 origin,Vector2 end,Vector2 direction,float radius,float innerRadius=0,float angle=120,float delay=0)
        {this.key=key;this.definition=definition;this.shape=shape;this.origin=origin;this.end=end;this.direction=direction;this.radius=radius;this.innerRadius=innerRadius;this.angle=angle;this.delay=delay;}
    }
    public static class EnemyCombat
    {
        public static readonly string[] TraitNames={"추적 화염","얼음 고리","생명 연결","사격 방벽","시체 폭발","분노 축적"};
        public static bool Trait(EnemyState enemy,int id)=>!enemy.boss&&(enemy.elite==id||enemy.eliteTraits!=null&&enemy.eliteTraits.Contains(id));
        public static string Id(int kind)=>kind>=100?BossCombat.Definition(kind):"N"+(kind+1).ToString("00");
        public static EnemyAttackDefinition Attack(int kind)
        {
            switch(kind)
            {
                case 1:return new EnemyAttackDefinition(8,.8f,5);case 2:return new EnemyAttackDefinition(9,1,2);
                case 3:return new EnemyAttackDefinition(8,1.2f,6);case 4:return new EnemyAttackDefinition(6,1,6);
                case 7:return new EnemyAttackDefinition(8,.9f,7);case 8:return new EnemyAttackDefinition(9,1.2f,3);
                case 9:return new EnemyAttackDefinition(8,1.5f,6);case 10:return new EnemyAttackDefinition(6,0,0);
                default:return new EnemyAttackDefinition(2,.65f,1.5f);
            }
        }
        public static Vector2 Rotate(Vector2 direction,float angle)
        {float a=angle*Mathf.Deg2Rad;return new Vector2(direction.x*Mathf.Cos(a)-direction.y*Mathf.Sin(a),direction.x*Mathf.Sin(a)+direction.y*Mathf.Cos(a));}
        public static bool Contains(EnemyThreat threat,Vector2 point)
        {
            Vector2 v=point-threat.origin;float d=v.magnitude;
            if(threat.shape==AttackShape.Line)
            {Vector2 segment=threat.end-threat.origin;float t=segment.sqrMagnitude<.00001f?0:Mathf.Clamp01(Vector2.Dot(v,segment)/segment.sqrMagnitude);return Vector2.Distance(point,threat.origin+segment*t)<=threat.radius+.00001f;}
            if(d>threat.radius+.00001f)return false;
            if(threat.shape==AttackShape.Ring)return d>=threat.innerRadius-.00001f;
            return threat.shape!=AttackShape.Sector||d<.00001f||Vector2.Angle(threat.direction,v)<=threat.angle*.5f+.0001f;
        }
        public static IEnumerable<Vector2[]> Outlines(EnemyThreat t)
        {
            if(t.shape==AttackShape.Line)
            {
                var d=(t.end-t.origin).normalized;if(d==Vector2.zero)d=Vector2.up;var points=new List<Vector2>();
                for(int i=0;i<=16;i++)points.Add(t.end+Rotate(d,-90+i*180f/16)*t.radius);
                for(int i=0;i<=16;i++)points.Add(t.origin+Rotate(d,90+i*180f/16)*t.radius);points.Add(points[0]);yield return points.ToArray();yield break;
            }
            if(t.shape==AttackShape.Sector)
            {var points=new List<Vector2>{t.origin};for(int i=0;i<=24;i++)points.Add(t.origin+Rotate(t.direction,-t.angle*.5f+i*t.angle/24)*t.radius);points.Add(t.origin);yield return points.ToArray();yield break;}
            foreach(float radius in t.shape==AttackShape.Ring?new[]{t.radius,t.innerRadius}:new[]{t.radius})
            {var points=new Vector2[49];for(int i=0;i<49;i++)points[i]=t.origin+Rotate(Vector2.up,i*360f/48)*radius;yield return points;}
        }
        public static EnemyThreat HazardThreat(EnemyHazard h)=>new EnemyThreat("hazard-"+h.id,h.definitionId,h.shape,h.position,h.end,h.direction,h.radius,h.innerRadius,h.angle,h.delay);
        static Vector2 ArrowEnd(RiftNavigation navigation,Vector2 origin,Vector2 direction,float range)
            =>navigation==null?origin+direction*range:navigation.ProjectileEnd(origin,origin+direction*range,.2f,out _);
        public static IEnumerable<EnemyThreat> Threats(RunState run,RiftNavigation navigation=null)
        {
            foreach(var threat in BossCombat.Threats(run,navigation))yield return threat;
            foreach(var h in run.enemyHazards)
            {
                if(h.shardBurst)
                {foreach(float angle in new[]{-30f,0,30}){var d=Rotate(h.direction,angle);yield return new EnemyThreat("shard-"+h.id+"-"+angle,h.definitionId,AttackShape.Line,h.position,ArrowEnd(navigation,h.position,d,8),d,.65f,delay:h.delay);}}
                else yield return HazardThreat(h);
            }
            foreach(var e in run.enemies.Where(e=>!e.dead&&!e.boss&&e.brain.action.phase!=EnemyActionPhase.Idle))
            {
                var a=e.brain.action;int kind=a.kind;string key="action-"+a.id;
                if(kind==4||kind==10)continue;
                if(kind==1||kind==7)
                    yield return new EnemyThreat(key,Id(kind),AttackShape.Line,a.phase==EnemyActionPhase.Charging?e.position:a.origin,a.aim,a.direction,1.05f,delay:a.phase==EnemyActionPhase.Charging?0:a.remaining);
                else if(kind==2||kind==8)
                {foreach(float angle in kind==8?new[]{-10f,0,10}:new[]{0f}){var d=Rotate(a.direction,angle);yield return new EnemyThreat(key+"-"+angle,Id(kind),AttackShape.Line,a.origin,ArrowEnd(navigation,a.origin,d,9),d,.65f,delay:a.remaining);}}
                else if(kind==3||kind==9)yield return new EnemyThreat(key,Id(kind),AttackShape.Circle,a.aim,a.aim,a.direction,2.5f,delay:a.remaining);
                else yield return new EnemyThreat(key,Id(kind),AttackShape.Sector,a.origin,a.aim,a.direction,2.5f,angle:120,delay:a.remaining);
            }
        }
        public static void Normalize(RunState run)
        {
            run.enemyHazards??=new List<EnemyHazard>();run.enemyCorpses??=new List<EnemyCorpse>();run.enemyEvents??=new List<EnemyCombatEvent>();
            run.enemies??=new List<EnemyState>();foreach(var e in run.enemies)
            {
                e.brain??=new EnemyBrain();e.brain.action??=new EnemyActionState();e.brain.action.points??=new List<Vector2>();e.brain.observations??=new List<MotionObservation>();
                if(e.brain.traitCooldowns==null||e.brain.traitCooldowns.Length!=6)e.brain.traitCooldowns=new float[6];
                e.brain.boss??=new BossPatternState();e.brain.boss.refuges??=new List<BossRefuge>();if(e.brain.boss.cooldowns==null||e.brain.boss.cooldowns.Length!=3)e.brain.boss.cooldowns=new float[3];
            }
        }
    }
}
