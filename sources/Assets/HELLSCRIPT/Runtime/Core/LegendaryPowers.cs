using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    public enum LegendaryTrigger { Always, Hit, Critical, Kill, Cast, Hurt, Block, Potion }
    public enum LegendaryEffect { Damage, CriticalChance, Cost, Resource, Heal, Shield, ReduceCooldown, Empower, Economize, Guard, Move, Haste, Dot, Burst, Echo, Stun, Root, Freeze, Slow }
    public enum LegendaryCondition { Always, Channeling, ResourceHigh, ResourceLow, Crowd, Controlled, Injured, Marked, Shouting, Shielded, Distant, Close, Poisoned, Healthy, Slowed, Frozen, Blizzard }
    [Serializable] public sealed class LegendaryPowerRow
    {
        public string id,name,nameEn,description,descriptionEn,skill,trigger,effect,condition,affectedSkill,reference,referenceUrl;
        public int heroClass,slot,weight,element,count;
        public float amount,duration,cooldown,radius;
    }
    [Serializable] public sealed class LegendaryPowerData { public int version; public LegendaryPowerRow[] powers; }
    public sealed class LegendaryPower
    {
        public readonly UniqueItemDefinition Item;
        public readonly LegendaryTrigger Trigger;
        public readonly LegendaryEffect Effect;
        public readonly LegendaryCondition Condition;
        public readonly string Skill,AffectedSkill,Reference,ReferenceUrl;
        public readonly float Amount,Duration,Cooldown,Radius;
        public readonly int Element,Count;
        public string Id=>Item.id;
        public LegendaryPower(LegendaryPowerRow row)
        {
            if(row==null||!Enum.TryParse(row.trigger,out LegendaryTrigger trigger)||!Enum.IsDefined(typeof(LegendaryTrigger),trigger)
                ||!Enum.TryParse(row.effect,out LegendaryEffect effect)||!Enum.IsDefined(typeof(LegendaryEffect),effect)
                ||!Enum.TryParse(row.condition,out LegendaryCondition condition)||!Enum.IsDefined(typeof(LegendaryCondition),condition))
                throw new InvalidOperationException("Invalid legendary power vocabulary.");
            if(row.heroClass<0||row.heroClass>2||row.slot<0||row.slot>7||row.weight<=0||row.count<1||row.count>10
                ||row.element<0||row.element>5||!Finite(row.amount)||row.amount<=0||!Finite(row.duration)||row.duration<0
                ||!Finite(row.cooldown)||row.cooldown<0||!Finite(row.radius)||row.radius<0||row.radius>6
                ||string.IsNullOrWhiteSpace(row.id)||!row.id.StartsWith("L"+"WAM"[row.heroClass],StringComparison.Ordinal)
                ||string.IsNullOrWhiteSpace(row.name)||string.IsNullOrWhiteSpace(row.nameEn)||string.IsNullOrWhiteSpace(row.description)
                ||string.IsNullOrWhiteSpace(row.descriptionEn)||string.IsNullOrWhiteSpace(row.reference)||string.IsNullOrWhiteSpace(row.referenceUrl)
                ||!ValidSkill(row.skill,row.heroClass)||!ValidSkill(row.affectedSkill,row.heroClass))
                throw new InvalidOperationException("Invalid legendary power: "+row.id);
            bool passive=effect==LegendaryEffect.Damage||effect==LegendaryEffect.CriticalChance||effect==LegendaryEffect.Cost;
            if(passive&&trigger!=LegendaryTrigger.Always||trigger==LegendaryTrigger.Always&&!passive&&effect!=LegendaryEffect.Guard
                ||trigger!=LegendaryTrigger.Always&&row.cooldown<.5f
                ||effect==LegendaryEffect.Dot&&(row.duration<1||row.duration>10||row.duration!=Mathf.Floor(row.duration))
                ||effect==LegendaryEffect.ReduceCooldown&&Index(row.affectedSkill)<0)
                throw new InvalidOperationException("Unsupported legendary rule: "+row.id);
            Trigger=trigger;Effect=effect;Condition=condition;Skill=row.skill;AffectedSkill=row.affectedSkill;
            Amount=row.amount;Duration=row.duration;Cooldown=row.cooldown;Radius=row.radius;Element=row.element;Count=row.count;
            Reference=row.reference;ReferenceUrl=row.referenceUrl;
            string required=Index(Skill)>=0?Skill:Index(AffectedSkill)>=0?AffectedSkill:"";
            Item=new UniqueItemDefinition(row.id,row.name,row.heroClass,row.slot,row.weight,row.description,required,nameEn:row.nameEn,descriptionEn:row.descriptionEn);
        }
        readonly LegendaryPower[] levels=new LegendaryPower[5];
        readonly LegendaryPower root;
        LegendaryPower(LegendaryPower basis,int level)
        {
            root=basis.root??basis;Item=basis.Item;Trigger=basis.Trigger;Effect=basis.Effect;Condition=basis.Condition;Skill=basis.Skill;AffectedSkill=basis.AffectedSkill;
            Reference=basis.Reference;ReferenceUrl=basis.ReferenceUrl;Cooldown=basis.Cooldown;Radius=basis.Radius;Element=basis.Element;Count=basis.Count;
            Amount=AspectGrowth.DurationGrows(Effect)?basis.Amount:AspectGrowth.Value(basis.Amount,level);
            Duration=AspectGrowth.DurationGrows(Effect)?AspectGrowth.Value(basis.Duration,level):basis.Duration;
        }
        public LegendaryPower AtLevel(int level)
        {if(root!=null)return root.AtLevel(level);level=Mathf.Clamp(level,1,5);return level==1?this:levels[level-1]??(levels[level-1]=new LegendaryPower(this,level));}
        static bool Finite(float value)=>!float.IsNaN(value)&&!float.IsInfinity(value);
        static bool ValidSkill(string skill,int heroClass)=>skill=="*"||skill=="BASIC"||Index(skill)>=heroClass*6&&Index(skill)<heroClass*6+6;
        public static int Index(string skill)
        {
            if(string.IsNullOrEmpty(skill)||skill.Length!=3)return -1;
            int c="WAM".IndexOf(skill[0]);
            return c>=0&&int.TryParse(skill.Substring(1),out int n)&&n>=1&&n<=6?c*6+n-1:-1;
        }
        public bool Matches(string skill)=>Skill=="*"||Skill==skill;
        public bool Affects(string skill)=>AffectedSkill=="*"||AffectedSkill==skill;
    }
    public static class LegendaryPowers
    {
        static IReadOnlyList<LegendaryPower> all;
        static Dictionary<string,LegendaryPower> byId;
        public static IReadOnlyList<LegendaryPower> All
        {
            get
            {
                if(all!=null)return all;
                var asset=Resources.Load<TextAsset>("Data/LegendaryPowers");
                if(asset==null)throw new InvalidOperationException("Missing legendary power data.");
                var data=JsonUtility.FromJson<LegendaryPowerData>(asset.text);
                if(data?.version!=1||data.powers==null)throw new InvalidOperationException("Unsupported legendary catalogue version.");
                var powers=data.powers.Select(row=>new LegendaryPower(row)).ToArray();
                byId=powers.ToDictionary(p=>p.Id,StringComparer.Ordinal);all=Array.AsReadOnly(powers);return all;
            }
        }
        public static LegendaryPower Find(string id){_ = All;return id!=null&&byId.TryGetValue(id,out var p)?p:null;}
    }
    [Serializable] public sealed class LegendaryPowerState
    {
        public string id;
        public int count;
        public float lastCount=-1000,readyAt,activeUntil;
    }
    [Serializable] public sealed class LegendaryPulse
    {
        public string id;
        public int root,target,remaining;
        public float nextAt;
        public Vector2 position;
        public DamageSnapshot snapshot;
    }
}
