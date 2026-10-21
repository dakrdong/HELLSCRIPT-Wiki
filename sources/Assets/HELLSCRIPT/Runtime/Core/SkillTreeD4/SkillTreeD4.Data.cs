using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Hellscript
{
    [Serializable] public sealed class SkillTreeD4ValueData { public double from,to; public string unit; public double[] ranks; }
    [Serializable] public sealed class SkillTreeD4StatData { public string ko,en,rank1,rankMax; public SkillTreeD4ValueData value; }
    [Serializable] public sealed class SkillTreeD4ModifierData { public string id,iconId,ko,en,effectKo,effectEn,shapeKo,shapeEn; public SkillTreeD4ValueData value; }
    [Serializable] public sealed class SkillTreeD4ActiveData
    {
        public string id,iconId,roleKo,ko,en,descKo,descEn;
        public int tier,maxRank; public bool ultimate;
        public SkillTreeD4StatData[] stats; public SkillTreeD4ModifierData[] modifiers;
    }
    [Serializable] public sealed class SkillTreeD4PassiveData
    {
        public string id,iconId,ko,en,effectKo,effectEn; public bool key; public int unlockLevel;
        public string[] linkedActives; public int[] linkedActiveIndices; public SkillTreeD4ValueData value;
    }
    [Serializable] public sealed class SkillTreeD4CellData { public string id,iconId; }
    [Serializable] public sealed class SkillTreeD4TierData { public string ko,en; public int gate; }
    [Serializable] public sealed class SkillTreeD4ClassData
    {
        public int heroClass; public string hero; public SkillTreeD4TierData[] tiers;
        public SkillTreeD4ActiveData[] actives; public SkillTreeD4PassiveData[] passives; public SkillTreeD4CellData[] spriteCells;
    }
    [Serializable] public sealed class SkillTreeD4RulesData
    {
        public int levelCap,pointsPerLevel,firstPointAtLevel,skillMaxRank,ultimateMaxRank,passiveMaxRank;
        public int[] tierGates; public string modifier,passive,tierGate;
    }
    [Serializable] public sealed class SkillTreeD4WhyData { public string max,tierOne,tierOther,@base,swap,cap,key; }
    [Serializable] public sealed class SkillTreeD4StringsData
    {
        public string locale,title,classNav,level,spent,left,close,start,gate,locked,mods,pick,selected,refund,invest,refundS,investS;
        public string active,tree,selRegion,passive,keyPassive,passHead,keyHead,unlock,rank,linked,req,open,now,lgInv,lgAvail,lgLock,lgLink,range,maxPop,keyPrefix;
        public string[] classes; public SkillTreeD4WhyData why;
    }
    // Load-only numeric wire identities never enter the playable catalog.
    [Serializable] public sealed class SkillTreeD4RemapData
    {
        public int heroClass,sourceSlot; public bool sourcePassive; public string to,modifier,note;
        public string from=>"WAM"[heroClass]+(sourcePassive?"P":"")+sourceSlot.ToString("00");
    }
    [Serializable] public sealed class SkillTreeD4Catalog
    {
        public int version; public SkillTreeD4RulesData rules; public SkillTreeD4ClassData[] classes;
        public SkillTreeD4StringsData[] strings; public SkillTreeD4RemapData[] legacyRemap;
    }

    public static class SkillTreeD4
    {
        static bool requested=true;
        static SkillTreeD4Catalog data; static SkillTreeD4Model model; static SkillTreeD4Rules rules;
        public static bool Enabled=>requested;
        public static SkillTreeD4Catalog Data
        {
            get
            {
                if(data!=null)return data;
                var asset=Resources.Load<TextAsset>("Data/SkillTreeD4");
                if(asset==null)throw new InvalidOperationException("Missing D4 skill catalog.");
                return data=Read(asset.text);
            }
        }
        static Dictionary<string,SkillTreeD4RemapData> legacy;
        public static SkillTreeD4RemapData Legacy(string id){legacy??=Data.legacyRemap.ToDictionary(r=>r.from,StringComparer.Ordinal);return id!=null&&legacy.TryGetValue(id,out var row)?row:null;}
        public static SkillTreeD4Model Model=>model??=ToModel(Data);
        public static SkillTreeD4Rules Rules=>rules??=new SkillTreeD4Rules(Model);
        public static SkillTreeD4ClassData For(HeroClass hero)=>Data.classes[(int)hero];
        public static SkillTreeD4Catalog Read(string json)
        {
            var value=JsonUtility.FromJson<SkillTreeD4Catalog>(json);
            if(value==null||value.version!=2||value.rules==null||value.rules.levelCap!=60||value.rules.pointsPerLevel!=1||value.rules.firstPointAtLevel!=1||value.rules.skillMaxRank!=5||value.rules.ultimateMaxRank!=3||value.rules.passiveMaxRank!=5||value.rules.tierGates?.Length!=5||value.classes?.Length!=3||value.strings?.Length!=2||value.legacyRemap?.Length!=111)
                throw new NotSupportedException("Unsupported D4 skill catalog.");
            for(int index=0;index<3;index++)
            {
                var c=value.classes[index];
                if(c==null||c.heroClass!=index||c.hero!=SkillTreeD4Model.Heroes[index]||c.tiers?.Length!=5||c.actives?.Length!=10||c.passives?.Length!=10||c.spriteCells?.Length!=48)
                    throw new InvalidOperationException("Invalid D4 class catalog.");
                var ids=c.actives.Select(a=>a.id).Concat(c.actives.SelectMany(a=>a.modifiers.Select(m=>m.id))).Concat(c.passives.Select(p=>p.id)).ToArray();
                if(ids.Length!=48||ids.Any(string.IsNullOrEmpty)||ids.Distinct().Count()!=48||!ids.OrderBy(s=>s).SequenceEqual(c.spriteCells.Select(s=>s.id).OrderBy(s=>s))||c.spriteCells.Any(s=>string.IsNullOrEmpty(s.iconId))||c.passives.Count(p=>p.key)!=2)
                    throw new InvalidOperationException("Invalid D4 skill identities.");
                for(int tier=0;tier<5;tier++)if(c.tiers[tier].gate!=value.rules.tierGates[tier])throw new InvalidOperationException("Inconsistent D4 tier gate.");
                foreach(var a in c.actives)
                {
                    if(a.tier<1||a.tier>5||a.maxRank!=(a.ultimate?3:5)||a.modifiers.Length!=(a.ultimate?2:3)||a.stats==null||a.stats.Length==0||string.IsNullOrEmpty(a.descKo))throw new InvalidOperationException("Invalid D4 active: "+a.id);
                    foreach(var s in a.stats)ValidateValue(s.value,a.maxRank);
                    for(int n=0;n<a.modifiers.Length;n++)
                    {
                        var m=a.modifiers[n];
                        if(m.id!=a.id+"-"+(n+1)||string.IsNullOrEmpty(m.shapeKo))throw new InvalidOperationException("Invalid D4 modifier: "+m.id);
                        ValidateValue(m.value,a.maxRank);
                    }
                }
                foreach(var p in c.passives)
                {
                    if(p.unlockLevel<1||p.unlockLevel>60||p.linkedActiveIndices==null||p.linkedActiveIndices.Any(n=>n<0||n>=10))throw new InvalidOperationException("Invalid D4 passive: "+p.id);
                    ValidateValue(p.value,5);
                }
            }
            if(!value.strings.Select(s=>s.locale).OrderBy(s=>s).SequenceEqual(new[]{"en","ko"})||value.strings.Any(s=>s.classes?.Length!=3||s.why==null||string.IsNullOrEmpty(s.title))||value.legacyRemap.Any(r=>r.heroClass<0||r.heroClass>2||r.sourceSlot<1||r.sourceSlot>(r.sourcePassive?19:18))||value.legacyRemap.Select(r=>r.from).Distinct().Count()!=111)
                throw new InvalidOperationException("Invalid D4 strings or legacy mappings.");
            return value;
        }
        static void ValidateValue(SkillTreeD4ValueData value,int maximum)
        {
            if(value==null||value.ranks?.Length!=maximum||double.IsNaN(value.from)||double.IsInfinity(value.from)||double.IsNaN(value.to)||double.IsInfinity(value.to)||value.ranks.Any(n=>double.IsNaN(n)||double.IsInfinity(n)))throw new InvalidOperationException("Invalid D4 rank values.");
        }
        static SkillTreeD4Text Pair(string ko,string en)=>new SkillTreeD4Text(ko,string.IsNullOrEmpty(en)?ko:en);
        public static SkillTreeD4Model ToModel(SkillTreeD4Catalog value)
        {
            var result=new SkillTreeD4Model{Gates=(int[])value.rules.tierGates.Clone(),MaxPoints=value.rules.levelCap};
            foreach(var s in value.strings)
                result.Strings[s.locale]=new SkillTreeD4Strings
                {
                    Title=s.title,Level=s.level,Spent=s.spent,Left=s.left,Start=s.start,Gate=s.gate,Locked=s.locked,Selected=s.selected,Invest=s.invest,InvestShort=s.investS,
                    Active=s.active,Passive=s.passive,KeyPassive=s.keyPassive,PassiveHeading=s.passHead,KeyHeading=s.keyHead,Req=s.req,Open=s.open,Now=s.now,
                    LegendInvested=s.lgInv,LegendAvailable=s.lgAvail,LegendLocked=s.lgLock,LegendLink=s.lgLink,Range=s.range,KeyPrefix=s.keyPrefix,Classes=s.classes,
                    WhyMax=s.why.max,WhyTierOne=s.why.tierOne,WhyTierOther=s.why.tierOther,WhyBase=s.why.@base,WhySwap=s.why.swap,WhyCap=s.why.cap,WhyKey=s.why.key
                };
            foreach(var c in value.classes)
            {
                int Cell(string id)=>Array.FindIndex(c.spriteCells,s=>s.id==id);
                result.Classes[c.hero]=new SkillTreeD4Class
                {
                    DesignSelection=-1,SpriteCells=c.spriteCells.Select(s=>new SkillTreeD4Cell{Id=s.id,IconId=s.iconId}).ToArray(),
                    Tiers=c.tiers.Select(t=>new SkillTreeD4Tier{Name=Pair(t.ko,t.en),Gate=t.gate}).ToArray(),
                    Actives=c.actives.Select(a=>new SkillTreeD4Active
                    {
                        Id=a.id,Tier=a.tier-1,Ultimate=a.ultimate,MaxRank=a.maxRank,Cell=Cell(a.id),Name=Pair(a.ko,a.en),Description=Pair(a.descKo,a.descEn),SampleModifier=-1,
                        Stats=a.stats.Select(s=>new SkillTreeD4Stat{Label=Pair(s.ko,s.en),From=s.rank1,To=s.rankMax}).ToArray(),
                        Modifiers=a.modifiers.Select(m=>new SkillTreeD4Modifier{Id=m.id,Name=Pair(m.ko,m.en),Effect=Pair(m.effectKo,m.effectEn),Shape=Pair(m.shapeKo,m.shapeEn)}).ToArray()
                    }).ToArray(),
                    Passives=c.passives.Select(p=>new SkillTreeD4Passive{Id=p.id,Key=p.key,UnlockLevel=p.unlockLevel,Cell=Cell(p.id),Name=Pair(p.ko,p.en),Effect=Pair(p.effectKo,p.effectEn),Links=p.linkedActiveIndices}).ToArray()
                };
            }
            return result;
        }
    }

    public static class SkillTreeD4Values
    {
        public static double At(SkillTreeD4ValueData value,int rank)
        {
            if(rank==0)return 0;
            if(rank<0||rank>value.ranks.Length)throw new ArgumentOutOfRangeException(nameof(rank));
            return value.ranks[rank-1];
        }
        public static double Active(string id,int stat,int rank)
        {
            var active=SkillTreeD4.Data.classes.SelectMany(c=>c.actives).Single(a=>a.id==id);var value=active.stats[stat].value;
            if(rank==6&&active.maxRank==5)
            {double extra=value.to+(value.to-value.from)/4;return value.unit=="명"||value.unit=="회"||value.unit=="발"||value.unit=="개"?Math.Floor(extra):extra;}
            return At(value,rank);
        }
        public static double Modifier(string id,int rank)=>At(SkillTreeD4.Data.classes.SelectMany(c=>c.actives).SelectMany(a=>a.modifiers).Single(m=>m.id==id).value,rank);
        public static double Passive(string id,int rank)=>At(SkillTreeD4.Data.classes.SelectMany(c=>c.passives).Single(p=>p.id==id).value,rank);
    }
}
