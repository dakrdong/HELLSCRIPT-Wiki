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
        // Run time at which a boss announced this action (regular enemies leave it 0).
        public float startedAt;
        public bool hitHero,empowered,released;
        public List<Vector2> points=new List<Vector2>();
        // Moving boss attacks (BossCombat.Moves): the leg being travelled and the time spent on it.
        public int leg;
        public float legElapsed;
        // A regular enemy's second attack (1 = the Cave Ghoul's leap); 0 is its usual attack.
        public int variant;
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
        // Follows its caster (enemyId) every tick: a boss's whirl that chases the hero.
        public bool followsCaster;
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
        // Pack role per kind: 0 melee, 1 charger, 2 ranged, 3 area, 4 support, 5 exploder.
        // Kinds 0-11 keep their old kind%6 meaning; 12-19 are the field-specific additions.
        static readonly int[] Roles={0,1,2,3,4,5,0,1,2,3,4,5,0,1,0,5,0,2,0,3};
        public static int Role(int kind)=>kind>=0&&kind<Roles.Length?Roles[kind]:0;
        public static readonly string[] RoleNames={"근접","돌진","원거리","장판","지원","폭발"};
        public static bool IsSupport(int kind)=>Role(kind)==4;
        // Hunt edict "RANGED" also covers ground casters, as it always has (kinds 2, 3, 8, 9).
        public static bool IsRangedTarget(int kind)=>Role(kind)==2||Role(kind)==3;
        // Fields (biomes): 0 Graveyard, 1 Fortress, 2 Desert, 3 Cavern, 4 Grassland, 5 Highland.
        // A field reuses the room templates of its set; the roster slot is the pack role.
        public const int FieldCount=6;
        static readonly int[] TemplateSets={0,1,1,0,0,1};
        static readonly int[][] Rosters={new[]{0,1,2,3,4,5},new[]{6,7,8,9,10,11},new[]{12,13,2,9,10,5},new[]{14,1,2,3,4,15},new[]{16,1,17,9,4,11},new[]{18,7,8,19,10,11}};
        public static int FieldTemplateSet(int field)=>TemplateSets[field];
        public static int RosterKind(int field,int slot)=>Rosters[field][slot];
        // The first field whose roster holds the kind; kinds shared by several fields name the earliest.
        public static int FieldOf(int kind)=>Array.FindIndex(Rosters,r=>Array.IndexOf(r,kind)>=0);
        public static EnemyAttackDefinition Attack(int kind)
        {
            switch(kind)
            {
                case 1:return new EnemyAttackDefinition(8,.8f,5);case 2:return new EnemyAttackDefinition(9,1,2);
                case 3:return new EnemyAttackDefinition(8,1.2f,6);case 4:return new EnemyAttackDefinition(6,1,6);
                case 7:return new EnemyAttackDefinition(8,.9f,7);case 8:return new EnemyAttackDefinition(9,1.2f,3);
                case 9:return new EnemyAttackDefinition(8,1.5f,6);case 10:return new EnemyAttackDefinition(6,0,0);
                // N13-N20. The melee slots keep N01's damage per second: 2 x 0.6 per 1.8 s, 1.1 per 1.65 s, 1.0 per 1.6 s.
                case 12:return new EnemyAttackDefinition(2.3f,.65f,1.8f);case 13:return new EnemyAttackDefinition(8,.9f,6);
                case 16:return new EnemyAttackDefinition(2.1f,.9f,1.65f);case 17:return new EnemyAttackDefinition(9,.8f,3);
                case 18:return new EnemyAttackDefinition(2.4f,.65f,1.6f);case 19:return new EnemyAttackDefinition(8,1.4f,6);
                default:return new EnemyAttackDefinition(2,.65f,1.5f);
            }
        }
        // The Cave Ghoul swings like N01 up close and leaps onto a hero 2.5-4.5 m away.
        public static readonly EnemyAttackDefinition GhoulLeap=new EnemyAttackDefinition(4.5f,.7f,5);
        public const float GhoulLeapMin=2.5f,LeapTime=.45f,BurrowSpeed=12;
        // Hit geometry of N13-N20 (kinds 12-19), shared by the release, the warnings and the forecast; null = N01's swing.
        public static EnemyThreat? OwnShape(EnemyActionState a,string key,float delay)
        {
            string d=Id(a.kind);
            switch(a.kind)
            {
                case 12:return new EnemyThreat(key,d,AttackShape.Sector,a.origin,a.aim,a.direction,2.3f,angle:110,delay:delay);
                case 13:return new EnemyThreat(key,d,AttackShape.Circle,a.aim,a.aim,a.direction,2.2f,delay:delay);
                case 14:return a.variant==1?new EnemyThreat(key,d,AttackShape.Circle,a.aim,a.aim,a.direction,1.6f,delay:delay):(EnemyThreat?)null;
                case 16:return new EnemyThreat(key,d,AttackShape.Circle,a.origin,a.origin,a.direction,2.1f,delay:delay);
                case 17:return new EnemyThreat(key,d,AttackShape.Line,a.origin,a.aim,a.direction,.35f,delay:delay);
                case 18:return new EnemyThreat(key,d,AttackShape.Sector,a.origin,a.aim,a.direction,2.4f,angle:120,delay:delay);
                case 19:return new EnemyThreat(key,d,AttackShape.Ring,a.aim,a.aim,a.direction,3.6f,1.6f,delay:delay);
                default:return null;
            }
        }
        // (attack multiplier, element, hero slow seconds) of those shapes.
        public static (float power,int element,float slow) OwnHit(int kind)
        {
            switch(kind)
            {
                case 12:return (.6f,0,0);case 13:return (1.4f,0,0);case 14:return (.9f,0,0);case 16:return (1.1f,0,0);
                case 17:return (.85f,0,1.5f);case 18:return (1,2,1.5f);case 19:return (1.3f,2,2);default:return (1,0,0);
            }
        }
        // Seconds between the release and the landing of a travelling attack (the sandworm's tunnel, the ghoul's leap).
        public static float Travel(EnemyActionState a)=>a.kind==13?Vector2.Distance(a.origin,a.aim)/BurrowSpeed:a.kind==14&&a.variant==1?LeapTime:0;
        public static bool Travels(EnemyActionState a)=>a.kind==13||a.kind==14&&a.variant==1;
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
            int count=t.shape==AttackShape.Line?35:t.shape==AttackShape.Sector?27:49;
            for(int index=0;index<(t.shape==AttackShape.Ring?2:1);index++)
            {var points=new Vector2[count];CopyOutline(t,index,points);yield return points;}
        }
        // Presentation reuses its own scratch buffer. The public iterator still returns
        // independent arrays of the original lengths to callers that retain the geometry.
        public static int CopyOutline(EnemyThreat t,int index,Vector2[] points)
        {
            if(index<0||index>=(t.shape==AttackShape.Ring?2:1))throw new ArgumentOutOfRangeException(nameof(index));
            int count=t.shape==AttackShape.Line?35:t.shape==AttackShape.Sector?27:49;
            if(points==null)throw new ArgumentNullException(nameof(points));
            if(points.Length<count)throw new ArgumentException("The outline buffer is too small.",nameof(points));
            if(t.shape==AttackShape.Line)
            {
                var d=(t.end-t.origin).normalized;if(d==Vector2.zero)d=Vector2.up;
                for(int i=0;i<=16;i++)points[i]=t.end+Rotate(d,-90+i*180f/16)*t.radius;
                for(int i=0;i<=16;i++)points[17+i]=t.origin+Rotate(d,90+i*180f/16)*t.radius;points[34]=points[0];
            }
            else if(t.shape==AttackShape.Sector)
            {points[0]=t.origin;for(int i=0;i<=24;i++)points[i+1]=t.origin+Rotate(t.direction,-t.angle*.5f+i*t.angle/24)*t.radius;points[26]=t.origin;}
            else
            {float radius=index==0?t.radius:t.innerRadius;for(int i=0;i<49;i++)points[i]=t.origin+Rotate(Vector2.up,i*360f/48)*radius;}
            return count;
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
                var own=OwnShape(a,key,0);
                if(own.HasValue)
                {
                    // A travelling attack lands after its travel; while it travels, what remains of it.
                    float delay=a.phase==EnemyActionPhase.Charging?Mathf.Max(0,Travel(a)-a.moved):a.remaining+Travel(a);
                    var o=own.Value;yield return new EnemyThreat(key,o.definition,o.shape,o.origin,o.end,o.direction,o.radius,o.innerRadius,o.angle,delay);continue;
                }
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
                e.brain.boss??=new BossPatternState();e.brain.boss.refuges??=new List<BossRefuge>();BossCombat.EnsureSlots(e.brain.boss);
            }
        }
    }
}
