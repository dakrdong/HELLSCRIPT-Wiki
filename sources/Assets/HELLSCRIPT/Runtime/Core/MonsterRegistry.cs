using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    // One skill as the codex shows it. name and description are Korean localization keys (English in en.txt);
    // tags hold the damage element (physical|fire|cold|lightning|poison|shadow) and free descriptors; damage is the
    // attack multiplier of one hit, 0 for skills that deal no direct damage. icon is an optional Resources path.
    [Serializable] public sealed class MonsterSkill
    {
        public string name="",description="",icon="";
        public string[] tags=Array.Empty<string>();
        public float damage;
    }
    // A breakable object an attack places (type "object"): totems, banners, lanterns, walls, cages, hearts, pylons, decoys.
    // Objects are stationary enemies with no attack or reward; they live duration seconds (0 = until broken). health is a
    // share of the owner's max HP. layout: ring (around the hero at place m), line (from the anchor along the aim, spacing
    // apart), around (around the owner at place m). blocks: the hero cannot walk through it (movement skills still pass).
    [Serializable] public sealed class MonsterObjectSpec
    {
        public string name="",layout="around";
        public int count=1;
        public float health=.1f,duration,place=3,spacing=1.6f,size=.6f;
        public bool blocks,ownerInvulnerable,mimic,link;
        public float auraRadius,auraAttack,auraSpeed,auraReduction,auraHeal,heroSlow;
        public float ownerReduction,stunOwner,burstPower,burstRadius,linkPower;
        public int element;
    }
    // One data-driven attack (MonsterBehavior). Times are seconds, distances metres, power is the attack multiplier.
    // type: strike | volley | charge | leap | blink | burrow | zone | heal | shield | summon | raise | object | attach | harvest
    // shape: circle | ring | sector | line;  at: self | hero | predicted | lastSkill (where the shape or zone is anchored, moved offset along the aim)
    [Serializable] public sealed class MonsterAttack
    {
        public string type="strike",shape="sector",at="self",pattern="";
        public int skill,element,hits=1,count=1,chill,chillLimit,summon=-1,cap=3;
        public float range=2,minRange,prep=.65f,cooldown=1.5f,power=1;
        public float radius=2.5f,inner,angle=120,length,spacing,spread;
        public float hitDelay=.35f,speed=10,projectile=.25f,travel;
        public bool returning,wallStun,consume,follow;
        public float slow,root,knockdown,push,pull;
        public float zoneDelay,duration,interval=.5f,moveSpeed,endRadius;
        public float trail,trailPower,heal,ward;
        // Slow strength (0 = the existing 35%, at most 60%), skill seal and curse (damage taken +curse for curseTime), poison stacks
        // (poisonPower x attack per stack per second, at most poisonCap stacks), life steal share, vision loss (seconds).
        public float slowStrength,seal,curse,curseTime,poisonPower,leech,blind;
        public int poison,poisonCap=5;
        // blinkTo: behind | side; retreat: metres sprung back after landing; growth: charge radius gained per metre.
        public string blinkTo="behind",trigger="";
        public float retreat,growth,rearPower,offset,reflect;
        // Projectile impact zone, trap zones (sprung by stepping in), spinning zones (degrees per second), ring zones whose inner
        // radius moves to endInner, zones with refuge spots (count, radius), boss invulnerability while the attack runs.
        public float impactZone,impactRadius,impactPower,spin,endInner,refugeRadius=1,invulnerable;
        public bool trap;
        public int refuges,summonAlt=-1;
        // Haste for allies within frenzyRadius for frenzyTime (attack cooldowns run 1+frenzy times as fast).
        public float frenzy,frenzyRadius,frenzyTime;
        public MonsterObjectSpec obj=new MonsterObjectSpec();
    }
    // Passive rules of a data-driven monster. All zero means none.
    [Serializable] public sealed class MonsterTraits
    {
        public float frontReduction,rearBonus;
        public int guardHits;
        public float enrageBelow,enrageAttack,enrageSpeed;
        public float auraRadius,auraAttack,auraSpeed,auraReduction,auraHeal;
        public bool stationary;
        public float ambushRadius,fleeSeconds,bonusGold;
        public int pack=1;
        public float packHealth=1;
        public float deathDelay,deathRadius,deathPower,deathDuration,deathSlow,deathRoot;
        public int deathElement;
        public int splitKind=-1,splitCount,splitDepth=1;
        public float splitHealth=.5f;
        // Damage reduction while not acting; frontal projectile immunity; frontal guard only before the boss's phase 2.
        public float idleReduction;
        public bool frontProjectiles,shieldPhase1,flying;
        // Retaliation: every retaliateHits hits taken start attacks[retaliateAttack] (an attack with trigger "hits").
        public int retaliateHits,retaliateAttack;
        // Haste stacks (cooldowns run faster): per nearby ally death, per consecutive hit landed; cap and duration.
        public float allyDeathHaste,hitHaste,hasteTime=5;
        public int hasteCap=3;
        // Share of damage taken healed to allies within 4 m; blink away when hit (metres, cooldown); move by blinks leaving embers.
        public float shareHeal,blinkOnHit,blinkCooldown=3,blinkStep,blinkTrail;
        // Petrify once below a health share (invulnerable, inactive); rise once after falling unless hit within reviveDelay.
        public float petrifyBelow,petrifySeconds,reviveHealth,reviveDelay;
        // Attack bonus within nearRadius of a living nearKind; heal a share of max HP by eating a corpse; shards fired on death.
        public int nearKind=-1,deathShards;
        public float nearRadius,nearAttack,corpseHeal,deathSlowStrength,auraHeroSlow;
    }
    // A registry entry. Ids: N01-N120 (kind = number-1, pattern -1) and B01-B18 (pattern = number-1, kind -1).
    // name/role/family are Korean localization keys. field is the GameCatalog field index (bosses: their home field).
    // slot is the pack role (EnemyCombat.Role): 0 melee, 1 charger, 2 ranged, 3 area, 4 support, 5 exploder.
    // legacy entries (the first 20 regular + 5 bosses) only describe hard-coded combat; the others fight from attacks/traits.
    // A boss's attacks are its kit: entries 0-2 are phase 1, 3-5 phase 2. hp/attack are multipliers, speed is metres per second.
    [Serializable] public sealed class MonsterDefinition
    {
        public string id="",name="",role="",family="",portrait="",art="";
        public bool boss,legacy,artPending;
        public int kind=-1,pattern=-1,field,slot,unlockTier=1,body;
        public float hp=1,attack=1,speed=2.7f,keepDistance=2.5f,approach=1;
        public MonsterSkill[] skills=Array.Empty<MonsterSkill>();
        public MonsterAttack[] attacks=Array.Empty<MonsterAttack>();
        public MonsterTraits traits=new MonsterTraits();
        public int Number=>boss?pattern+1:kind+1;
        // Data-driven combat (not one of the hard-coded 25).
        public bool Data=>!legacy;
    }
    [Serializable] sealed class MonsterRegistryFile {public MonsterDefinition[] monsters=Array.Empty<MonsterDefinition>();}

    // The single list of monsters the game knows: Resources/Monsters/MonsterRegistry.json, written by tools/monster_registry.py.
    public static class MonsterRegistry
    {
        public const string ResourcePath="Monsters/MonsterRegistry";
        static List<MonsterDefinition> all;
        static MonsterDefinition[] byKind=Array.Empty<MonsterDefinition>(),byPattern=Array.Empty<MonsterDefinition>();
        static Dictionary<string,MonsterDefinition> byId=new Dictionary<string,MonsterDefinition>(StringComparer.Ordinal);
        // Fired after Load/Reset so derived tables (names, rosters) rebuild.
        public static event Action Changed;
        // Codex order: field index, regular monsters before bosses, then number.
        public static IReadOnlyList<MonsterDefinition> All{get{Ensure();return all;}}
        public static MonsterDefinition ById(string id){Ensure();return id!=null&&byId.TryGetValue(id,out var d)?d:null;}
        public static MonsterDefinition ForKind(int kind){Ensure();return kind>=0&&kind<byKind.Length?byKind[kind]:null;}
        public static MonsterDefinition ForBoss(int pattern){Ensure();return pattern>=0&&pattern<byPattern.Length?byPattern[pattern]:null;}
        // Bosses map to their B id by pattern, everything else to its N id by kind; elites are their base kind; the goblin has none.
        public static string IdOf(EnemyState e)=>e==null||e.goblin||!string.IsNullOrEmpty(e.objectType)?null:e.boss?ForBoss(e.pattern)?.id:ForKind(e.kind)?.id;
        // One past the highest registered kind / pattern (gaps are kinds not shipped yet).
        public static int KindCount{get{Ensure();return byKind.Length;}}
        public static int PatternCount{get{Ensure();return byPattern.Length;}}
        // The data-driven definition of an enemy, or null for the hard-coded 25 and unknown kinds.
        public static MonsterDefinition Behavior(EnemyState e)
        {
            Ensure();if(e==null||e.goblin||!string.IsNullOrEmpty(e.objectType))return null;
            var d=e.boss?(e.pattern>=0&&e.pattern<byPattern.Length?byPattern[e.pattern]:null):e.kind>=0&&e.kind<byKind.Length?byKind[e.kind]:null;
            return d!=null&&d.Data?d:null;
        }
        public static MonsterDefinition DataKind(int kind){var d=ForKind(kind);return d!=null&&d.Data?d:null;}
        static void Ensure()
        {
            if(all!=null)return;
            var asset=Resources.Load<TextAsset>(ResourcePath);
            Build(asset!=null?asset.text:"{\"monsters\":[]}");
        }
        // Replaces the registry with this JSON (tests and tools). Throws on an invalid file.
        public static void Load(string json){Build(json);Changed?.Invoke();}
        // Back to the shipped Resources file.
        public static void Reset(){all=null;Ensure();Changed?.Invoke();}
        static readonly char[] IdLetters={'N','B'};
        public static bool ParseId(string id,out bool boss,out int number)
        {
            boss=false;number=0;
            if(string.IsNullOrEmpty(id)||id.Length<3||Array.IndexOf(IdLetters,id[0])<0)return false;
            boss=id[0]=='B';int end=1;while(end<id.Length&&char.IsDigit(id[end]))end++;
            return end-1>=2&&end==id.Length&&int.TryParse(id.Substring(1),out number)&&number>=1;
        }
        // Monster id from an attack or event definition: "N21", "N21_2", "N33_DEATH", "B06_4", "BOSS01_SLAM" -> "B01"; null otherwise.
        public static string IdFromDefinition(string definition)
        {
            if(string.IsNullOrEmpty(definition))return null;
            if(definition.StartsWith("BOSS",StringComparison.Ordinal)&&definition.Length>=6&&char.IsDigit(definition[4])&&char.IsDigit(definition[5]))return "B"+definition.Substring(4,2);
            int cut=definition.IndexOf('_');string head=cut<0?definition:definition.Substring(0,cut);
            return ParseId(head,out _,out _)?head:null;
        }
        static void Build(string json)
        {
            var file=JsonUtility.FromJson<MonsterRegistryFile>(json)??new MonsterRegistryFile();
            var list=new List<MonsterDefinition>();var ids=new Dictionary<string,MonsterDefinition>(StringComparer.Ordinal);
            foreach(var d in file.monsters??Array.Empty<MonsterDefinition>())
            {
                if(d==null||!ParseId(d.id,out bool boss,out int number))throw new InvalidOperationException("Invalid monster id: "+d?.id);
                if(boss!=d.boss||(boss?d.pattern!=number-1||d.kind!=-1:d.kind!=number-1||d.pattern!=-1))throw new InvalidOperationException("Monster id and kind/pattern disagree: "+d.id);
                if(ids.ContainsKey(d.id))throw new InvalidOperationException("Duplicate monster id: "+d.id);
                d.skills??=Array.Empty<MonsterSkill>();d.attacks??=Array.Empty<MonsterAttack>();d.traits??=new MonsterTraits();
                foreach(var s in d.skills)s.tags??=Array.Empty<string>();
                ids.Add(d.id,d);list.Add(d);
            }
            list=list.OrderBy(d=>d.field).ThenBy(d=>d.boss?1:0).ThenBy(d=>d.Number).ToList();
            int kinds=list.Where(d=>!d.boss).Select(d=>d.kind+1).DefaultIfEmpty(0).Max(),patterns=list.Where(d=>d.boss).Select(d=>d.pattern+1).DefaultIfEmpty(0).Max();
            var k=new MonsterDefinition[kinds];var p=new MonsterDefinition[patterns];
            foreach(var d in list){if(d.boss)p[d.pattern]=d;else k[d.kind]=d;}
            all=list;byId=ids;byKind=k;byPattern=p;
        }
    }
}
