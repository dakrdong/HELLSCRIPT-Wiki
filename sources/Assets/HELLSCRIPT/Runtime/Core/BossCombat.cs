using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    // Saved as ints in EnemyActionState.kind: append only, never renumber.
    public enum BossAttack
    {
        Basic=100, Slam, Hook, Summon, Beam, Charge, Blasts, Legacy,
        Roar=108, ReapingSweep=109, ExecutionLeap=110, ChainWhirl=111, GraveToll=112,
        DirgeRing=113, RequiemChoir=114, HymnOfSilence=115, LamentOrbs=116,
        MawSnap=117, RiftTear=118, ShardStorm=119, DevouringPull=120,
        TailLash=121, VenomSpray=122, BurrowStrike=123, QuicksandMaelstrom=124, ScarabSwarm=125, TripleEruption=126,
        IceLance=127, FrostNova=128, GlacialSpikes=129, BlizzardVeil=130, FrozenSentinels=131, ShatterFan=132,
    }
    [Serializable] public sealed class BossRefuge
    {
        public Vector2 position;
        public float radius=.65f;
        public List<Vector2> path=new List<Vector2>();
    }
    [Serializable] public sealed class BossPatternState
    {
        public bool initialized,enraged,enragePending;
        // Slot 0 is the basic attack; slot n is Kit[pattern][n-1].
        public float[] cooldowns=new float[BossCombat.Slots];
        public float planRetry;
        // Last non-basic slot used, so ready patterns take turns instead of the first one starving the rest.
        public int cursor;
        public List<BossRefuge> refuges=new List<BossRefuge>();
    }
    public readonly struct BossProfile
    {
        public readonly float health,attack,keepDistance,approach;
        public readonly int body;
        public BossProfile(float health,float attack,float keepDistance,float approach,int body){this.health=health;this.attack=attack;this.keepDistance=keepDistance;this.approach=approach;this.body=body;}
    }
    public readonly struct BossHit
    {
        public readonly float power,duration,interval,spacing,hold;
        public readonly int element;
        public readonly bool slow;
        public BossHit(float power,int element,float duration=0,float interval=0,float spacing=0,float hold=0,bool slow=false)
        {this.power=power;this.element=element;this.duration=duration;this.interval=interval;this.spacing=spacing;this.hold=hold;this.slow=slow;}
    }
    // How a moving boss attack travels: Dash, Burrow and Glide run pace metres per second along their legs, Leap and Blink
    // take pace seconds per leg. contact is the attack multiplier a dash deals once to a hero its body sweeps. hold is the
    // recovery after the last leg. carriesWhirl: the whirl hazard rides along with the boss for the whole travel.
    public enum BossLegStyle{Dash,Leap,Burrow,Glide,Blink}
    public readonly struct BossMotion
    {
        public readonly BossLegStyle style;public readonly float pace,contact,hold;public readonly bool carriesWhirl;
        public BossMotion(BossLegStyle style,float pace,float contact=0,float hold=.4f,bool carriesWhirl=false){this.style=style;this.pace=pace;this.contact=contact;this.hold=hold;this.carriesWhirl=carriesWhirl;}
        public bool Timed=>style==BossLegStyle.Leap||style==BossLegStyle.Blink;
        public float LegTime(Vector2 from,Vector2 to)=>Timed?pace:Vector2.Distance(from,to)/Mathf.Max(.01f,pace);
    }
    public readonly struct BossVolley
    {
        public readonly int count;
        public readonly float spread,speed,range,radius;
        public BossVolley(int count,float spread,float speed,float range,float radius){this.count=count;this.spread=spread;this.speed=speed;this.range=range;this.radius=radius;}
    }
    public static class BossCombat
    {
        public const int Slots=7;
        // Phase 1 uses entries 0-2 (slots 1-3), phase 2 adds entries 3-5 (slots 4-6).
        public static readonly BossAttack[][] Kit=
        {
            new[]{BossAttack.Slam,BossAttack.Hook,BossAttack.ReapingSweep,BossAttack.ExecutionLeap,BossAttack.ChainWhirl,BossAttack.GraveToll},
            new[]{BossAttack.Summon,BossAttack.Beam,BossAttack.DirgeRing,BossAttack.RequiemChoir,BossAttack.HymnOfSilence,BossAttack.LamentOrbs},
            new[]{BossAttack.Charge,BossAttack.Blasts,BossAttack.MawSnap,BossAttack.RiftTear,BossAttack.ShardStorm,BossAttack.DevouringPull},
            new[]{BossAttack.TailLash,BossAttack.VenomSpray,BossAttack.BurrowStrike,BossAttack.QuicksandMaelstrom,BossAttack.ScarabSwarm,BossAttack.TripleEruption},
            new[]{BossAttack.IceLance,BossAttack.FrostNova,BossAttack.GlacialSpikes,BossAttack.BlizzardVeil,BossAttack.FrozenSentinels,BossAttack.ShatterFan},
        };
        public static readonly BossProfile[] Profile=
        {
            new BossProfile(1,1,2.5f,1,6),new BossProfile(.85f,.9f,8,.35f,10),new BossProfile(1.1f,1.1f,2.5f,1,7),
            new BossProfile(1.15f,1.05f,2.5f,1,5),new BossProfile(.9f,1,7,.5f,8),
        };
        public static BossProfile ProfileOf(int pattern)=>Profile[Mathf.Clamp(pattern,0,Profile.Length-1)];
        public static BossAttack[] KitOf(int pattern)=>Kit[Mathf.Clamp(pattern,0,Kit.Length-1)];
        public static string Definition(int kind)
        {
            switch((BossAttack)kind)
            {
                case BossAttack.Basic:return "BOSS_BASIC";case BossAttack.Slam:return "BOSS01_SLAM";case BossAttack.Hook:return "BOSS01_HOOK";
                case BossAttack.Summon:return "BOSS02_SUMMON";case BossAttack.Beam:return "BOSS02_BEAM";case BossAttack.Charge:return "BOSS03_CHARGE";
                case BossAttack.Blasts:return "BOSS03_BLAST";case BossAttack.Roar:return "BOSS_ROAR";
                case BossAttack.ReapingSweep:return "BOSS01_SWEEP";case BossAttack.ExecutionLeap:return "BOSS01_LEAP";case BossAttack.ChainWhirl:return "BOSS01_WHIRL";case BossAttack.GraveToll:return "BOSS01_TOLL";
                case BossAttack.DirgeRing:return "BOSS02_DIRGE";case BossAttack.RequiemChoir:return "BOSS02_REQUIEM";case BossAttack.HymnOfSilence:return "BOSS02_HYMN";case BossAttack.LamentOrbs:return "BOSS02_ORBS";
                case BossAttack.MawSnap:return "BOSS03_MAW";case BossAttack.RiftTear:return "BOSS03_TEAR";case BossAttack.ShardStorm:return "BOSS03_SHARDS";case BossAttack.DevouringPull:return "BOSS03_PULL";
                case BossAttack.TailLash:return "BOSS04_TAIL";case BossAttack.VenomSpray:return "BOSS04_VENOM";case BossAttack.BurrowStrike:return "BOSS04_BURROW";
                case BossAttack.QuicksandMaelstrom:return "BOSS04_MAELSTROM";case BossAttack.ScarabSwarm:return "BOSS04_SWARM";case BossAttack.TripleEruption:return "BOSS04_ERUPTION";
                case BossAttack.IceLance:return "BOSS05_LANCE";case BossAttack.FrostNova:return "BOSS05_NOVA";case BossAttack.GlacialSpikes:return "BOSS05_SPIKES";
                case BossAttack.BlizzardVeil:return "BOSS05_BLIZZARD";case BossAttack.FrozenSentinels:return "BOSS05_SENTINELS";case BossAttack.ShatterFan:return "BOSS05_SHATTER";
                default:return "LEGACY_BOSS";
            }
        }
        public static string Name(int kind)
        {
            switch((BossAttack)kind)
            {
                case BossAttack.Basic:return "근접 공격";case BossAttack.Slam:return "내려찍기";case BossAttack.Hook:return "갈고리";case BossAttack.Summon:return "부하 소환";case BossAttack.Beam:return "집중 광선";case BossAttack.Charge:return "돌진";case BossAttack.Blasts:return "연속 폭발";
                case BossAttack.Roar:return "포효";
                case BossAttack.ReapingSweep:return "거두기 베기";case BossAttack.ExecutionLeap:return "처형 도약";case BossAttack.ChainWhirl:return "사슬 회오리";case BossAttack.GraveToll:return "무덤 종소리";
                case BossAttack.DirgeRing:return "만가의 고리";case BossAttack.RequiemChoir:return "진혼 합창";case BossAttack.HymnOfSilence:return "침묵의 찬가";case BossAttack.LamentOrbs:return "비탄의 구체";
                case BossAttack.MawSnap:return "아가리 물기";case BossAttack.RiftTear:return "균열 찢기";case BossAttack.ShardStorm:return "파편 폭풍";case BossAttack.DevouringPull:return "탐식의 끌어당김";
                case BossAttack.TailLash:return "꼬리 채찍";case BossAttack.VenomSpray:return "독액 분사";case BossAttack.BurrowStrike:return "잠행 강습";
                case BossAttack.QuicksandMaelstrom:return "유사 소용돌이";case BossAttack.ScarabSwarm:return "풍뎅이 떼";case BossAttack.TripleEruption:return "삼중 분출";
                case BossAttack.IceLance:return "얼음 창";case BossAttack.FrostNova:return "서리 폭발";case BossAttack.GlacialSpikes:return "빙하 가시";
                case BossAttack.BlizzardVeil:return "눈보라 장막";case BossAttack.FrozenSentinels:return "얼어붙은 파수꾼";case BossAttack.ShatterFan:return "파쇄 부채";
                default:return "이전 공격 마무리";
            }
        }
        public static EnemyAttackDefinition Attack(BossAttack attack)
        {
            switch(attack)
            {
                case BossAttack.Slam:return new EnemyAttackDefinition(4,1.2f,6);case BossAttack.Hook:return new EnemyAttackDefinition(10,1,9);
                case BossAttack.Summon:return new EnemyAttackDefinition(12,.8f,14);case BossAttack.Beam:return new EnemyAttackDefinition(12,1.5f,8);
                case BossAttack.Charge:return new EnemyAttackDefinition(8,1,7);case BossAttack.Blasts:return new EnemyAttackDefinition(12,1.3f,12);
                case BossAttack.Roar:return new EnemyAttackDefinition(99,1.5f,0);
                case BossAttack.ReapingSweep:return new EnemyAttackDefinition(7,1,8);case BossAttack.ExecutionLeap:return new EnemyAttackDefinition(9,1.2f,10);
                case BossAttack.ChainWhirl:return new EnemyAttackDefinition(6,1,11);case BossAttack.GraveToll:return new EnemyAttackDefinition(9,1.2f,12);
                case BossAttack.DirgeRing:return new EnemyAttackDefinition(9,1.2f,9);case BossAttack.RequiemChoir:return new EnemyAttackDefinition(12,1.6f,12);
                case BossAttack.HymnOfSilence:return new EnemyAttackDefinition(6,1.6f,14);case BossAttack.LamentOrbs:return new EnemyAttackDefinition(10,1.2f,9);
                case BossAttack.MawSnap:return new EnemyAttackDefinition(7,.7f,7);case BossAttack.RiftTear:return new EnemyAttackDefinition(12,1.2f,11);
                case BossAttack.ShardStorm:return new EnemyAttackDefinition(10,1,10);case BossAttack.DevouringPull:return new EnemyAttackDefinition(8,1,12);
                case BossAttack.TailLash:return new EnemyAttackDefinition(6,.9f,7);case BossAttack.VenomSpray:return new EnemyAttackDefinition(9,1,8);
                case BossAttack.BurrowStrike:return new EnemyAttackDefinition(10,1.4f,11);case BossAttack.QuicksandMaelstrom:return new EnemyAttackDefinition(5,1.3f,13);
                case BossAttack.ScarabSwarm:return new EnemyAttackDefinition(12,.9f,15);case BossAttack.TripleEruption:return new EnemyAttackDefinition(12,1.2f,11);
                case BossAttack.IceLance:return new EnemyAttackDefinition(11,1.1f,7);case BossAttack.FrostNova:return new EnemyAttackDefinition(6.5f,1.4f,10);
                case BossAttack.GlacialSpikes:return new EnemyAttackDefinition(10,1.2f,9);case BossAttack.BlizzardVeil:return new EnemyAttackDefinition(12,1.3f,13);
                case BossAttack.FrozenSentinels:return new EnemyAttackDefinition(12,.9f,15);case BossAttack.ShatterFan:return new EnemyAttackDefinition(10,1.1f,9);
                default:return new EnemyAttackDefinition(3,.6f,2);
            }
        }
        // Damage per hit (x attack) and timing of the attacks added with the phase kits. Hazards with a
        // duration tick every interval; hold keeps the boss in place. Travelling patterns take their timing from Moves.
        public static BossHit Hit(BossAttack kind)
        {
            switch(kind)
            {
                case BossAttack.Roar:return new BossHit(.8f,5);
                case BossAttack.ReapingSweep:return new BossHit(1.2f,0);case BossAttack.ExecutionLeap:return new BossHit(1.6f,0);
                case BossAttack.ChainWhirl:return new BossHit(.5f,0,interval:.5f);case BossAttack.GraveToll:return new BossHit(1,5);
                case BossAttack.DirgeRing:return new BossHit(1.2f,5);case BossAttack.RequiemChoir:return new BossHit(.3f,5,1.5f,.25f,hold:1.5f);
                case BossAttack.HymnOfSilence:return new BossHit(1.6f,5);case BossAttack.LamentOrbs:return new BossHit(1,5);
                case BossAttack.MawSnap:return new BossHit(1.4f,0);case BossAttack.RiftTear:return new BossHit(.35f,5,3,.5f);
                case BossAttack.ShardStorm:return new BossHit(1,0);case BossAttack.DevouringPull:return new BossHit(1.4f,0);
                case BossAttack.TailLash:return new BossHit(1.3f,0);case BossAttack.VenomSpray:return new BossHit(.9f,4);case BossAttack.BurrowStrike:return new BossHit(1.5f,0);
                case BossAttack.QuicksandMaelstrom:return new BossHit(.3f,0,3,.5f,slow:true);case BossAttack.TripleEruption:return new BossHit(1.2f,1);
                case BossAttack.IceLance:return new BossHit(1.3f,2,slow:true);case BossAttack.FrostNova:return new BossHit(1.4f,2,slow:true);case BossAttack.GlacialSpikes:return new BossHit(1.3f,2);
                case BossAttack.BlizzardVeil:return new BossHit(.3f,2,3,.5f,slow:true);case BossAttack.ShatterFan:return new BossHit(1,2);
                default:return new BossHit(0,0);
            }
        }
        // Attacks that travel before they strike. Waypoints are planned at announce: the multi-leg ones in points, the
        // others in aim (an older save without planned legs lands on its aim).
        public static bool Moves(BossAttack kind,out BossMotion motion)
        {
            switch(kind)
            {
                case BossAttack.ReapingSweep:motion=new BossMotion(BossLegStyle.Dash,14,.8f,.45f);return true;
                case BossAttack.ExecutionLeap:motion=new BossMotion(BossLegStyle.Leap,.55f,0,.5f);return true;
                case BossAttack.ChainWhirl:motion=new BossMotion(BossLegStyle.Dash,WhirlSpeed,0,.3f,true);return true;
                case BossAttack.GraveToll:motion=new BossMotion(BossLegStyle.Leap,.4f,0,.45f);return true;
                case BossAttack.DirgeRing:motion=new BossMotion(BossLegStyle.Blink,.35f,0,.3f);return true;
                case BossAttack.LamentOrbs:motion=new BossMotion(BossLegStyle.Blink,.3f,0,.25f);return true;
                case BossAttack.MawSnap:motion=new BossMotion(BossLegStyle.Dash,16,0,.35f);return true;
                case BossAttack.RiftTear:motion=new BossMotion(BossLegStyle.Dash,18,0,.4f);return true;
                case BossAttack.BurrowStrike:motion=new BossMotion(BossLegStyle.Burrow,9,0,.45f);return true;
                case BossAttack.TripleEruption:motion=new BossMotion(BossLegStyle.Burrow,7,0,.45f);return true;
                case BossAttack.FrostNova:motion=new BossMotion(BossLegStyle.Glide,10,0,.4f);return true;
                case BossAttack.ShatterFan:motion=new BossMotion(BossLegStyle.Blink,.3f,0,.25f);return true;
                default:motion=default;return false;
            }
        }
        public static bool Moves(int kind)=>Moves((BossAttack)kind,out _);
        public const float WhirlSpeed=3,WhirlRadius=3.2f;
        static bool MultiLeg(BossAttack kind)=>kind==BossAttack.GraveToll||kind==BossAttack.TripleEruption;
        public static int Legs(EnemyActionState a)=>MultiLeg((BossAttack)a.kind)&&a.points.Count>0?a.points.Count:1;
        public static Vector2 Waypoint(EnemyActionState a,int leg)=>MultiLeg((BossAttack)a.kind)&&a.points.Count>0?a.points[Mathf.Clamp(leg,0,a.points.Count-1)]:a.aim;
        public static Vector2 LegStart(EnemyActionState a,int leg)=>leg<=0?a.origin:Waypoint(a,leg-1);
        public static Vector2 LegDirection(EnemyActionState a,int leg)
        {
            // A blink faces the hero it read at announce; everything else faces where it travels.
            var d=a.kind==(int)BossAttack.DirgeRing||a.kind==(int)BossAttack.LamentOrbs||a.kind==(int)BossAttack.ShatterFan?a.direction:(Waypoint(a,leg)-LegStart(a,leg)).normalized;
            return d.sqrMagnitude<1e-6f?a.direction:d;
        }
        // Seconds from now until the boss finishes leg `leg`: the preparation still to run while preparing, then every leg up to it.
        public static float Arrival(EnemyActionState a,in BossMotion motion,int leg)
        {
            bool moving=a.phase==EnemyActionPhase.Charging;float t=moving?0:Mathf.Max(0,a.remaining);int first=moving?a.leg:0;
            for(int i=first;i<=leg;i++){float total=motion.LegTime(LegStart(a,i),Waypoint(a,i));t+=moving&&i==first?Mathf.Max(0,total-a.legElapsed):total;}
            return t;
        }
        // What lands where the boss arrives at the end of a leg.
        public static EnemyThreat LegStrike(EnemyActionState a,int leg,string key,float delay)
        {
            var kind=(BossAttack)a.kind;string d=Definition(a.kind);var at=Waypoint(a,leg);var dir=LegDirection(a,leg);
            EnemyThreat Circle(float radius)=>new EnemyThreat(key,d,AttackShape.Circle,at,at,dir,radius,delay:delay);
            switch(kind)
            {
                case BossAttack.ReapingSweep:return new EnemyThreat(key,d,AttackShape.Sector,at,at+dir,dir,3.5f,angle:220,delay:delay);
                case BossAttack.MawSnap:return new EnemyThreat(key,d,AttackShape.Sector,at,at+dir,dir,4,angle:70,delay:delay);
                case BossAttack.DirgeRing:return new EnemyThreat(key,d,AttackShape.Ring,at,at,dir,4.5f,2,delay:delay);
                case BossAttack.RiftTear:return new EnemyThreat(key,d,AttackShape.Line,a.origin,at,dir,.8f,delay:delay);
                case BossAttack.ExecutionLeap:return Circle(3);
                case BossAttack.BurrowStrike:return Circle(2.8f);
                case BossAttack.TripleEruption:return Circle(2.4f);
                case BossAttack.FrostNova:return Circle(4.5f);
                case BossAttack.GraveToll:return Circle(2);
                default:return Circle(2);
            }
        }
        public static bool Volley(BossAttack kind,out BossVolley volley)
        {
            switch(kind)
            {
                case BossAttack.LamentOrbs:volley=new BossVolley(6,60,8,10,.35f);return true;case BossAttack.ShardStorm:volley=new BossVolley(8,45,10,10,.3f);return true;
                case BossAttack.VenomSpray:volley=new BossVolley(5,12,11,9,.25f);return true;case BossAttack.ShatterFan:volley=new BossVolley(7,10,11,10,.25f);return true;
                default:volley=default;return false;
            }
        }
        // Attacks built from Shapes (everything from the Roar on); the original seven keep their own code.
        public static bool Shaped(int kind)=>kind>=(int)BossAttack.Roar&&!Summons(kind);
        public static bool Summons(int kind)=>kind==(int)BossAttack.Summon||kind==(int)BossAttack.ScarabSwarm||kind==(int)BossAttack.FrozenSentinels;
        public static int SummonCount(BossAttack kind)=>kind==BossAttack.Summon?4:3;
        public static int SummonKind(BossAttack kind)
        {
            // ponytail: falls back to N01 until the roster reaches N13/N19.
            int wanted=kind==BossAttack.ScarabSwarm?12:kind==BossAttack.FrozenSentinels?18:0;
            return GameCatalog.EnemyNames.Length>wanted?wanted:0;
        }
        // A plan that can fail (no room for adds, no safe blast layout, no landing) retries after planRetry.
        public static bool Planned(BossAttack kind)=>Summons((int)kind)||kind==BossAttack.Blasts||Moves(kind,out _);
        // Aimed shots need a clear projectile line of this radius to be chosen; -1 means no check.
        public static float ShotRadius(BossAttack kind)
        {
            switch(kind)
            {
                case BossAttack.Hook:case BossAttack.IceLance:case BossAttack.RiftTear:return .5f;case BossAttack.Beam:return .75f;case BossAttack.RequiemChoir:return .5f;
                case BossAttack.VenomSpray:case BossAttack.ShatterFan:return .25f;default:return -1;
            }
        }
        // Invulnerable from the step after the roar is announced until it releases, in whatever order damage
        // resolves within those steps. Hits resolving in the announcing step itself still land.
        public static bool Roaring(EnemyState enemy,float time)
        {var a=enemy.brain.action;return enemy.boss&&a.kind==(int)BossAttack.Roar&&a.phase==EnemyActionPhase.Preparing&&time>a.startedAt;}
        public static bool InRefuge(BossPatternState boss,Vector2 point)=>boss.refuges.Any(r=>Vector2.Distance(point,r.position)<=r.radius+.00001f);
        public static void EnsureSlots(BossPatternState boss)
        {
            if(boss.cooldowns==null||boss.cooldowns.Length==0){boss.cooldowns=new float[Slots];return;}
            if(boss.cooldowns.Length>=Slots)return;
            // Old saves had three slots. New slots wait as long as the slowest old one.
            int old=boss.cooldowns.Length;float wait=boss.cooldowns.Max();Array.Resize(ref boss.cooldowns,Slots);for(int n=old;n<Slots;n++)boss.cooldowns[n]=wait;
        }
        // Time until any selectable slot is ready: phase 1 cannot use the phase-2 slots.
        public static float ReadyIn(BossPatternState boss)
        {
            int count=Mathf.Min(boss.cooldowns.Length,boss.enraged?Slots:4);float ready=float.MaxValue;
            for(int n=0;n<count;n++)ready=Mathf.Min(ready,boss.cooldowns[n]);return ready;
        }
        // Candidate slots in order: phase 2 tries its own three first, phase-1 attacks stay available,
        // the basic attack is the fallback. Deterministic; never draws from the run's random stream.
        public static IEnumerable<int> Order(BossPatternState boss)
        {
            if(boss.enraged)foreach(int slot in Turns(4,boss.cursor))yield return slot;
            foreach(int slot in Turns(1,boss.cursor))yield return slot;
            yield return 0;
        }
        static IEnumerable<int> Turns(int first,int last)
        {int start=last>=first&&last<first+3?last-first+1:0;for(int n=0;n<3;n++)yield return first+(start+n)%3;}
        public static float[] FanAngles(int count,float spread)
        {
            var angles=new float[count];float start=spread*count>=360-.001f?0:-spread*(count-1)*.5f;
            for(int n=0;n<count;n++)angles[n]=start+n*spread;return angles;
        }
        public static bool OwnAdd(EnemyState add,EnemyState boss)=>add.add&&!add.boss&&string.IsNullOrEmpty(add.eventId)&&(add.summonerId==boss.id||add.summonerId<0);
        // Hit and warning geometry of the Shaped attacks; warnings, release and the forecast read the same list.
        public static List<EnemyThreat> Shapes(EnemyActionState a,string key,float delay)
        {
            var kind=(BossAttack)a.kind;string d=Definition(a.kind);var list=new List<EnemyThreat>();
            EnemyThreat Circle(Vector2 center,float radius,float after=0,int index=-1)=>new EnemyThreat(index<0?key:key+"-"+index,d,AttackShape.Circle,center,center,a.direction,radius,delay:delay+after);
            // skip starts the line that far from the boss, so a fan of lines does not stack at its feet.
            EnemyThreat Line(Vector2 end,float radius,int index=-1,float skip=0)
            {var from=a.origin+(end-a.origin).normalized*Mathf.Min(skip,Vector2.Distance(a.origin,end));return new EnemyThreat(index<0?key:key+"-"+index,d,AttackShape.Line,from,end,(end-a.origin).normalized,radius,delay:delay);}
            if(Moves(kind,out var motion))
            {
                // A travelling attack warns its path while the body still has to cover it, and each landing at the moment it lands.
                bool moving=a.phase==EnemyActionPhase.Charging;
                if(motion.carriesWhirl)
                {
                    // The whirl warns the whole lane it will sweep; once it runs, the remaining lane reads full.
                    var start=moving?Vector2.MoveTowards(a.origin,a.aim,a.legElapsed*motion.pace):a.origin;
                    list.Add(new EnemyThreat(key+"-path",d,AttackShape.Line,start,a.aim,a.direction,WhirlRadius,delay:moving?0:delay));
                    return list;
                }
                if(motion.contact>0&&(!moving||!a.hitHero))
                {var start=moving?Vector2.MoveTowards(LegStart(a,a.leg),Waypoint(a,a.leg),a.legElapsed*motion.pace):a.origin;list.Add(new EnemyThreat(key+"-path",d,AttackShape.Line,start,Waypoint(a,0),a.direction,1.2f,delay:moving?0:delay));}
                if(Volley(kind,out var shots))
                {
                    float at=delay-Mathf.Max(0,a.remaining)+Arrival(a,motion,0);
                    for(int i=0;i<a.points.Count;i++)list.Add(new EnemyThreat(key+"-"+i,d,AttackShape.Line,a.aim+(a.points[i]-a.aim).normalized*Mathf.Min(1.2f,Vector2.Distance(a.aim,a.points[i])),a.points[i],(a.points[i]-a.aim).normalized,shots.radius+.45f,delay:at));
                    return list;
                }
                for(int i=moving?a.leg:0;i<Legs(a);i++)list.Add(LegStrike(a,i,key+"-"+i,delay-Mathf.Max(0,a.remaining)+Arrival(a,motion,i)));
                return list;
            }
            switch(kind)
            {
                case BossAttack.Roar:list.Add(Circle(a.origin,4));break;
                case BossAttack.HymnOfSilence:list.Add(Circle(a.origin,5));break;
                case BossAttack.DevouringPull:list.Add(Circle(a.origin,3,a.remainingCharges>1?.8f:0));break;
                case BossAttack.QuicksandMaelstrom:list.Add(new EnemyThreat(key,d,AttackShape.Ring,a.origin,a.origin,a.direction,5,1.5f,delay:delay));break;
                case BossAttack.GlacialSpikes:for(int i=0;i<a.points.Count;i++)list.Add(Circle(a.points[i],2,0,i));break;
                case BossAttack.BlizzardVeil:for(int i=0;i<a.points.Count;i++)list.Add(Circle(a.points[i],1.8f,0,i));break;
                case BossAttack.RequiemChoir:for(int i=0;i<a.points.Count;i++)list.Add(Line(a.points[i],.5f,i,2));break;
                case BossAttack.TailLash:list.Add(Line(a.aim,1.1f));break;
                case BossAttack.IceLance:list.Add(Line(a.aim,.8f));break;
                default:
                    // Volleys warn along each shot's wall-clipped path.
                    if(Volley(kind,out var volley))for(int i=0;i<a.points.Count;i++)list.Add(Line(a.points[i],volley.radius+.45f,i));
                    break;
            }
            return list;
        }
        public static IEnumerable<EnemyThreat> Threats(RunState run,RiftNavigation navigation)
        {
            foreach(var enemy in run.enemies.Where(e=>e.boss&&!e.dead))
            {
                var a=enemy.brain.action;if(a.phase==EnemyActionPhase.Idle||a.phase==EnemyActionPhase.Recovering)continue;
                string key="boss-"+a.id,definition=Definition(a.kind);var kind=(BossAttack)a.kind;
                if(Summons(a.kind))continue;
                if(Shaped(a.kind)){foreach(var shape in Shapes(a,key,a.phase==EnemyActionPhase.Charging?0:a.remaining))yield return shape;continue;}
                if(kind==BossAttack.Blasts)
                {for(int i=0;i<a.points.Count;i++)yield return new EnemyThreat(key+"-"+i,definition,AttackShape.Circle,a.points[i],a.points[i],a.direction,2.5f,delay:a.remaining+i*.35f);}
                else if(kind==BossAttack.Hook||kind==BossAttack.Beam||kind==BossAttack.Charge)
                {
                    var from=a.phase==EnemyActionPhase.Charging?enemy.position:a.origin;float radius=kind==BossAttack.Beam?.75f:.95f;
                    yield return new EnemyThreat(key,definition,AttackShape.Line,from,a.aim,a.direction,radius,delay:a.phase==EnemyActionPhase.Charging?0:a.remaining);
                }
                else if(kind==BossAttack.Legacy)yield return new EnemyThreat(key,definition,AttackShape.Circle,a.aim,a.aim,a.direction,3,delay:a.remaining);
                else yield return new EnemyThreat(key,definition,AttackShape.Sector,a.origin,a.aim,a.direction,kind==BossAttack.Slam?4:3,delay:a.remaining);
            }
        }
        public static bool TryBlastPlan(RunState run,RiftNavigation map,EnemyState boss,float speed,out List<Vector2> centers,out List<BossRefuge> refuges)
        {
            centers=new List<Vector2>();refuges=new List<BossRefuge>();float budget=Mathf.Max(0,speed*1.1f);
            var threats=EnemyCombat.Threats(run,map).Where(t=>t.delay<=2&&Vector2.Distance(run.position,t.origin)<=14+t.radius&&map.LineClear(run.position,t.origin)).ToList();
            foreach(var f in run.effects.Where(f=>f.hostile&&f.delay<=2))threats.Add(new EnemyThreat("",f.definitionId,f.kind==6?AttackShape.Ring:AttackShape.Circle,f.position,f.position,Vector2.up,f.radius,f.kind==6?2:0));
            foreach(var p in run.projectiles.Where(p=>p.hostile))threats.Add(new EnemyThreat("",p.definitionId,AttackShape.Line,p.position,map.ProjectileEnd(p.position,p.position+p.direction*p.remaining,p.radius,out _),p.direction,p.radius+.45f));
            var candidates=new List<BossRefuge>();
            foreach(float radius in new[]{3.3f,4f})for(int n=0;n<16;n++)
            {
                if(radius>budget)continue;Vector2 point=run.position+EnemyCombat.Rotate(Vector2.up,n*22.5f)*radius;
                if(!map.CanLand(point,.65f)||!map.Reachable(point)||run.enemies.Any(e=>!e.dead&&Vector2.Distance(e.position,point)<(e.boss?1.2f:.4f)+.65f))continue;
                var path=map.FindPath(run.position,point);if(RiftNavigation.Length(path)>budget||path==null||Vector2.Distance(path[path.Count-1],point)>.01f)continue;
                bool safe=true;
                for(int i=1;i<path.Count&&safe;i++)for(float t=0;t<=1.0001f;t+=1f/Mathf.Max(1,Mathf.CeilToInt(Vector2.Distance(path[i-1],path[i])/.25f)))
                {var p=Vector2.Lerp(path[i-1],path[i],t);if(threats.Any(th=>EnemyCombat.Contains(th,p)&&map.LineClear(th.origin,p))||run.enemies.Any(e=>!e.dead&&Vector2.Distance(e.position,p)<(e.boss?1.2f:.4f)+.45f)){safe=false;break;}}
                if(safe)candidates.Add(new BossRefuge{position=point,path=path});
            }
            for(int n=0;n<8;n++)
            {
                var axis=EnemyCombat.Rotate(Vector2.up,n*22.5f);var points=new List<Vector2>{run.position,run.position+axis*3.5f,run.position-axis*3.5f};
                if(points.Any(p=>!map.CanLand(p,.1f)))continue;
                var valid=candidates.Where(r=>points.All(p=>Vector2.Distance(p,r.position)>2.5f+r.radius+.01f)).ToList();
                foreach(var first in valid)
                {
                    var second=valid.FirstOrDefault(r=>Vector2.Distance(first.position,r.position)>=1.5f);if(second==null)continue;
                    centers=points;refuges=new List<BossRefuge>{first,second};return true;
                }
            }
            return false;
        }
    }
}
