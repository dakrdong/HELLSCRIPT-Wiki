using System;
using System.Collections.Generic;
using UnityEngine;

namespace Hellscript
{
    // What an enemy or boss attack looks like when it lands, by footprint and element: a swing leaves a slash, a circle a
    // shockwave with a burst and a ground mark, a ring a ring of bursts, a line a streak of bursts (a bolt for lightning,
    // cold and shadow), a charger's set-off a dust burst and a dust trail while it runs. Shots flash at the muzzle and burst
    // where they stop; lingering zones burn, bubble, frost or smoke while active. A landing near the hero shakes the camera,
    // harder for bosses. Footprints come from the telegraph gauge, so an effect plays on the step the damage applies.
    // Presentation only: reads RunState, never writes it, and spawns nothing the fog of war hides.
    public sealed partial class WorldView
    {
        static Dictionary<string,int> bossAttackKinds;
        // Element ints (Core/Attributes.cs): 0 physical, 1 fire, 2 cold, 3 lightning, 4 poison, 5 shadow.
        public static int AttackElement(string definition)
        {
            switch(definition)
            {
                case null:case "":return 0;
                case "N04":case "N16_DEATH":return 4;case "N10":case "E01":return 1;case "E02":case "N19":case "N20":return 2;case "E05":case "N12_DEATH":return 5;
            }
            if(bossAttackKinds==null)
            {bossAttackKinds=new Dictionary<string,int>(StringComparer.Ordinal);foreach(BossAttack k in Enum.GetValues(typeof(BossAttack)))bossAttackKinds[BossCombat.Definition((int)k)]=(int)k;}
            if(!bossAttackKinds.TryGetValue(definition,out int kind))return 0;
            var attack=(BossAttack)kind;return attack==BossAttack.Beam||attack==BossAttack.Blasts?5:BossCombat.Hit(attack).element;
        }
        public static int ActionElement(EnemyState enemy,int kind)=>AttackElement(enemy.boss?BossCombat.Definition(kind):EnemyCombat.Id(kind));
        // Gamma tints of the WorldFx convention.
        public static Color ElementColor(int element)
        {
            switch(element)
            {
                case 1:return new Color(1,.52f,.16f,1);case 2:return new Color(.55f,.85f,1,1);case 3:return new Color(.72f,.86f,1,1);
                case 4:return new Color(.5f,1,.36f,1);case 5:return new Color(.72f,.42f,1,1);default:return new Color(1,.84f,.62f,1);
            }
        }
        static readonly string[] LandBurst={"impact","explosion","frost","lightning","poison","shadow"};
        static readonly string[] LandMark={"crack","scorch","ice","scorch","poison","rune"};
        static bool Charger(string definition)=>definition=="N02"||definition=="N08"||definition==BossCombat.Definition((int)BossAttack.Charge);
        void LandAttack(WorldFx library,RunState run,in TelegraphGauge.Footprint f)
        {
            int element=AttackElement(f.definition);var tint=ElementColor(element);var at=Position(f.origin);float size=f.boss?1.35f:1;
            if(Charger(f.definition))
            {library.Burst("dust",at+Vector3.up*.2f,f.boss?1.6f:1);library.Shockwave(at,f.boss?2.2f:1.3f,tint,.35f);return;}
            switch(f.shape)
            {
                case AttackShape.Sector:
                {
                    var forward=Position(f.direction).normalized;var mid=at+forward*(f.radius*.55f);
                    library.Slash(at+Vector3.up*(f.boss?1.5f:.95f),forward,f.radius,Mathf.Min(f.angle,330),tint,f.boss?.3f:.22f);
                    library.Burst(LandBurst[element],mid+Vector3.up*.4f,Mathf.Max(.6f,f.radius*.4f)*size,tint);library.Decal(LandMark[element],mid,f.radius*.4f);
                    break;
                }
                case AttackShape.Ring:
                {
                    library.Shockwave(at,f.radius,tint,.5f);float middle=(f.radius+f.inner)*.5f;
                    for(int i=0;i<8;i++){var d=Quaternion.Euler(0,i*45+22.5f,0)*Vector3.forward;library.Burst(LandBurst[element],at+d*middle+Vector3.up*.3f,Mathf.Max(.5f,(f.radius-f.inner)*.45f),tint);}
                    break;
                }
                case AttackShape.Line:
                {
                    var from=at;var to=Position(f.end);float length=Vector3.Distance(from,to);int count=Mathf.Clamp(Mathf.CeilToInt(length/1.6f),2,7);
                    for(int i=0;i<count;i++)library.Burst(LandBurst[element],Vector3.Lerp(from,to,(i+.5f)/count)+Vector3.up*.35f,Mathf.Max(.5f,f.radius)*size,tint);
                    if(element==2||element==3||element==5)library.Beam(from+Vector3.up*1.1f,to+Vector3.up*1.1f,tint,Mathf.Max(.14f,f.radius*.5f),.24f);
                    break;
                }
                default:
                    library.Shockwave(at,f.radius,tint,f.boss?.55f:.42f);library.Burst(LandBurst[element],at+Vector3.up*.35f,Mathf.Max(.7f,f.radius*.5f)*size,tint);
                    library.Decal(LandMark[element],at,f.radius*.8f);if(f.boss)library.Burst("dust",at,f.radius*.5f);
                    break;
            }
            // The phase-change roar throws a second, wide wave and shakes the whole screen.
            if(f.definition=="BOSS_ROAR"){library.Shockwave(at,f.radius*2,ElementColor(5),.7f);library.Burst("shadow",at+Vector3.up*1.5f,2.5f,ElementColor(5));lighting.Shake(.75f,.5f);return;}
            // A landing the hero stood in or next to shakes the camera; boss impacts shake harder.
            float gap=f.shape==AttackShape.Line?DistanceToSegment(run.position,f.origin,f.end)-f.radius:Vector2.Distance(run.position,f.origin)-f.radius;
            if(gap<2.5f)lighting.Shake(f.boss?.55f:.2f,f.boss?.34f:.18f);
        }
        static float DistanceToSegment(Vector2 p,Vector2 a,Vector2 b)
        {var ab=b-a;float t=ab.sqrMagnitude<1e-6f?0:Mathf.Clamp01(Vector2.Dot(p-a,ab)/ab.sqrMagnitude);return Vector2.Distance(p,a+ab*t);}

        // ------------------------------------------------------------ windup sparks, charge trails
        // Casters and bosses gather sparks of their element at the hand while the gauge fills; chargers kick up dust as they run.
        void AttackMotes(WorldFx library,GameObject actor,EnemyState enemy,RunState run)
        {
            var a=enemy.brain.action;if(library==null||a.phase==EnemyActionPhase.Idle||!CanDisplayEnemyMarker(run,enemy.position))return;
            if(a.phase==EnemyActionPhase.Charging)
            {
                // Trails of a travelling boss: sand thrown up over a burrow, frost behind a glide, dark sparks as a blink
                // fades, dust behind dashes and whirls; nothing while a leap is in the air.
                if(enemy.boss&&BossCombat.Moves((BossAttack)a.kind,out var travel))
                {
                    var at=actor.transform.position;
                    switch(travel.style)
                    {
                        case BossLegStyle.Leap:return;
                        case BossLegStyle.Burrow:if(Tick(actor,.07f))library.Burst("dust",at+Vector3.up*.1f,1.6f,new Color(1,.84f,.6f,1));return;
                        case BossLegStyle.Glide:if(Tick(actor,.08f))library.Burst("frost",at+Vector3.up*.3f,1.1f,ElementColor(2));return;
                        case BossLegStyle.Blink:if(Tick(actor,.1f))library.Burst("shadow",at+Vector3.up*1.6f,1.3f,ElementColor(5));return;
                        default:if(Tick(actor,travel.carriesWhirl?.12f:.07f))library.Burst(travel.carriesWhirl?"sparks":"dust",at+Vector3.up*(travel.carriesWhirl?1.2f:.15f),1.4f);return;
                    }
                }
                // The sandworm throws up sand as it tunnels; the ghoul is in the air.
                if(!enemy.boss&&EnemyCombat.Travels(a)){if(a.kind==13&&Tick(actor,.07f))library.Burst("dust",actor.transform.position+Vector3.up*.1f,1.1f,new Color(1,.84f,.6f,1));return;}
                if(Tick(actor,.1f))library.Burst("dust",actor.transform.position+Vector3.up*.15f,enemy.boss?1.3f:.8f);return;
            }
            if(a.phase!=EnemyActionPhase.Preparing)return;
            var motion=MotionOf(enemy,a.kind);if(motion!=AttackMotion.Cast&&!enemy.boss)return;
            int element=ActionElement(enemy,a.kind);float windup=Mathf.Clamp01(1-a.remaining/Mathf.Max(.01f,a.preparation));
            if(!Tick(actor,Mathf.Lerp(.26f,.1f,windup)))return;
            var hand=actor.transform.TransformPoint(new Vector3(.7f,enemy.boss?2.3f:2.2f,.4f));
            library.Burst(element==0?"sparks":LandBurst[element],hand,(enemy.boss?.7f:.4f)*(.6f+windup*.6f),ElementColor(element));
        }
        readonly Dictionary<GameObject,float> moteClocks=new Dictionary<GameObject,float>();
        bool Tick(GameObject actor,float every)
        {
            moteClocks.TryGetValue(actor,out float last);if(elapsed-last<every&&elapsed>=last)return false;
            moteClocks[actor]=elapsed;return true;
        }

        // ------------------------------------------------------------ hostile shots
        readonly Dictionary<int,(Vector3 at,int element)> hostileShots=new Dictionary<int,(Vector3,int)>();
        readonly List<int> goneShots=new List<int>();
        readonly HashSet<int> liveShots=new HashSet<int>();
        void PresentShots(WorldFx library,RunState run)
        {
            liveShots.Clear();
            foreach(var p in run.projectiles)
            {
                if(!p.hostile||p.delay>0)continue;liveShots.Add(p.id);var at=Position(p.position)+Vector3.up;
                int element=p.element!=0?p.element:AttackElement(p.definitionId);
                if(!hostileShots.ContainsKey(p.id)&&library!=null&&CanDisplayEnemyMarker(run,p.origin))library.Flash(Position(p.origin)+Vector3.up*1.2f,.7f,ElementColor(element));
                hostileShots[p.id]=(at,element);
            }
            goneShots.Clear();foreach(var pair in hostileShots)if(!liveShots.Contains(pair.Key))goneShots.Add(pair.Key);
            foreach(int id in goneShots)
            {
                var (at,element)=hostileShots[id];hostileShots.Remove(id);
                if(library!=null&&CanDisplayEnemyMarker(run,new Vector2(at.x,at.z)))library.Burst(LandBurst[element],at,.6f,ElementColor(element));
            }
        }

        // ------------------------------------------------------------ lingering zones
        sealed class Zone{public GameObject anchor;public float clock;public int element;}
        readonly Dictionary<int,Zone> zones=new Dictionary<int,Zone>();
        readonly List<int> goneZones=new List<int>();
        readonly HashSet<int> liveZones=new HashSet<int>();
        void PresentZones(WorldFx library,RunState run)
        {
            liveZones.Clear();
            foreach(var h in run.enemyHazards)
            {
                if(h.delay>0||h.duration<=0||h.shardBurst)continue;
                bool shown=CanDisplayEnemyMarker(run,h.position,h.radius);liveZones.Add(h.id);
                if(!zones.TryGetValue(h.id,out var zone))
                {
                    zone=new Zone{element=h.element!=0?h.element:AttackElement(h.definitionId)};
                    zone.anchor=new GameObject("Hostile zone "+h.id);zone.anchor.transform.SetParent(world.transform,false);zone.anchor.transform.position=Position(h.position);
                    if(library!=null)
                    {
                        float scale=h.shape==AttackShape.Line?Mathf.Max(.6f,h.radius):Mathf.Max(.6f,h.radius*.45f);var tint=ElementColor(zone.element);
                        if(zone.element==1)library.Fire(zone.anchor.transform,scale,tint);else library.Smoke(zone.anchor.transform,scale,tint);
                    }
                    zones[h.id]=zone;
                }
                zone.anchor.SetActive(shown);if(!shown||library==null)continue;
                zone.clock-=motionStep;if(zone.clock>0)continue;zone.clock=h.shape==AttackShape.Line?.14f:.3f;
                // A spot inside the footprint for the next puff: a hash of the zone and time, never the run's random stream.
                float u=WorldFx.Hash(new Vector3(h.id,elapsed*7.3f,0)),v=WorldFx.Hash(new Vector3(elapsed*3.1f,h.id,1));var tintZone=ElementColor(zone.element);
                if(h.shape==AttackShape.Line)
                {
                    var from=Position(h.position);var to=Position(h.end);
                    if(zone.element==5||zone.element==3||zone.element==2)library.Beam(from+Vector3.up*1.1f,to+Vector3.up*1.1f,tintZone,Mathf.Max(.2f,h.radius*.6f),.16f);
                    library.Burst(LandBurst[zone.element],Vector3.Lerp(from,to,u)+Vector3.up*.3f,Mathf.Max(.4f,h.radius*.8f),tintZone);
                }
                else
                {
                    float r=Mathf.Lerp(h.shape==AttackShape.Ring?h.innerRadius:0,h.radius,Mathf.Sqrt(v));var d=Quaternion.Euler(0,u*360,0)*Vector3.forward;
                    library.Burst(LandBurst[zone.element],Position(h.position)+d*r+Vector3.up*.25f,.55f,tintZone);
                }
            }
            goneZones.Clear();foreach(var pair in zones)if(!liveZones.Contains(pair.Key))goneZones.Add(pair.Key);
            foreach(int id in goneZones){if(zones[id].anchor!=null)Destroy(zones[id].anchor);zones.Remove(id);}
        }
        void ClearAttackFx(){hostileShots.Clear();zones.Clear();moteClocks.Clear();}
    }
}
