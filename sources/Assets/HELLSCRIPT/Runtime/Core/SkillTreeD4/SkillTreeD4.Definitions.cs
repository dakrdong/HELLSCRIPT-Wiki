using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    public static class SkillTreeD4Definitions
    {
        static Dictionary<string,ClassSkillDefinition> definitions;
        static void Ensure()
        {
            if(definitions!=null)return;definitions=new Dictionary<string,ClassSkillDefinition>(StringComparer.Ordinal);
            foreach(HeroClass hero in Enum.GetValues(typeof(HeroClass)))
            {
                var data=SkillTreeD4.For(hero);int order=0;
                foreach(var active in data.actives)
                {
                    var old=ClassSkills.Find(active.id);var d=old==null?new ClassSkillDefinition{parameters=Array.Empty<SkillNumber>()}:JsonUtility.FromJson<ClassSkillDefinition>(JsonUtility.ToJson(old));
                    d.id=active.id;d.category=active.ultimate?"ultimate":"active";d.heroClass=(int)hero;d.order=++order;d.unlock=active.ultimate?41:1;d.maxRank=active.maxRank;d.name=active.ko;d.nameEn=active.en;d.description=active.descKo;d.descriptionEn=active.descEn;definitions.Add(d.id,d);
                    foreach(var modifier in active.modifiers)definitions.Add(modifier.id,ClassSkills.Find(modifier.id)??throw new InvalidOperationException("Missing current modifier metadata: "+modifier.id));
                }
                foreach(var passive in data.passives)definitions.Add(passive.id,new ClassSkillDefinition{id=passive.id,heroClass=(int)hero,category="passive",order=++order,unlock=passive.unlockLevel,maxRank=5,name=passive.ko,nameEn=passive.en,description=passive.effectKo,descriptionEn=passive.effectEn,parameters=Array.Empty<SkillNumber>()});
            }
        }
        public static ClassSkillDefinition Find(string id){if(string.IsNullOrEmpty(id))return null;Ensure();return definitions.TryGetValue(id,out var d)?d:null;}
        public static IEnumerable<ClassSkillDefinition> For(HeroClass hero)
        {Ensure();var data=SkillTreeD4.For(hero);return data.actives.Select(a=>definitions[a.id]).Concat(data.passives.Select(p=>definitions[p.id]));}
        // The skill facts behind Description(): the picker draws them as chips and cards, Description() joins the same values as text.
        public sealed class StatRow{public string label,value,unit;}
        public sealed class ModifierChoice{public string id,name;public int rank;}   // rank: the chosen modifier's own rank, 0 for the others
        public sealed class SkillDetail
        {
            public int rank,bonus,maxRank,modifierRank;
            public List<StatRow> stats=new List<StatRow>();
            public List<ModifierChoice> modifiers=new List<ModifierChoice>();   // every modifier the skill can take, in catalog order
            public string passiveValue,liveCost,modifierName,modifierShape,modifierEffect,modifierValue,modifierUnit,description;
            public int modifierIndex=-1;   // the chosen modifier's place in modifiers, -1 when none is chosen
            public bool HasModifier=>modifierName!=null;
        }
        public static SkillDetail Detail(HeroClass hero,ClassSkillLoadout load,string id,int bonus=0,ClassSkillStatus live=null)
        {
            var data=SkillTreeD4.For(hero);var active=data.actives.FirstOrDefault(a=>a.id==id);var passive=data.passives.FirstOrDefault(p=>p.id==id);bool en=Loc.Language=="en";
            if(active==null&&passive==null)return null;int rank=load.Rank(id),effective=active==null||active.ultimate?rank:Math.Min(6,rank+Math.Max(0,bonus));
            var d=new SkillDetail{rank=rank,bonus=effective-rank,maxRank=active?.maxRank??5};
            if(rank>0&&active!=null)for(int i=0;i<active.stats.Length;i++){var row=active.stats[i];d.stats.Add(new StatRow{label=en?row.en:row.ko,value=Format(SkillTreeD4Values.Active(id,i,effective),row.value.unit),unit=row.value.unit});}
            if(rank>0&&passive!=null)d.passiveValue=Format(SkillTreeD4Values.At(passive.value,rank),passive.value.unit);
            if(live!=null&&active!=null)d.liveCost=Loc.F("현재 사용 비용 {0:0.##}",live.cost);
            if(active!=null){int chosen=load.Modifier(id)-1;d.modifiers.AddRange(active.modifiers.Select((m,i)=>new ModifierChoice{id=m.id,name=en?m.en:m.ko,rank=i==chosen?load.ModifierRank(id):0}));}
            if(active!=null&&load.Modifier(id)>0)
            {var modifier=active.modifiers[load.Modifier(id)-1];d.modifierIndex=load.Modifier(id)-1;d.modifierRank=load.ModifierRank(id);d.modifierName=en?modifier.en:modifier.ko;d.modifierShape=en?modifier.shapeEn:modifier.shapeKo;d.modifierEffect=en?modifier.effectEn:modifier.effectKo;d.modifierValue=Format(SkillTreeD4Values.At(modifier.value,d.modifierRank),modifier.value.unit);d.modifierUnit=modifier.value.unit;}
            d.description=active!=null?(en?active.descEn:active.descKo):(en?passive.effectEn:passive.effectKo);return d;
        }
        public static string Description(HeroClass hero,ClassSkillLoadout load,string id,int bonus=0,ClassSkillStatus live=null)
        {
            var d=Detail(hero,load,id,bonus,live);if(d==null)return "";
            var lines=new List<string>{Loc.F("현재 등급 {0} · 추가 등급 {1}",d.rank,d.bonus)};
            lines.AddRange(d.stats.Select(s=>s.label+": "+s.value));
            if(d.passiveValue!=null)lines.Add(d.passiveValue);
            if(d.liveCost!=null)lines.Add(d.liveCost);
            if(d.HasModifier){lines.Add(d.modifierName+" · "+d.modifierRank);lines.Add(d.modifierShape);lines.Add(d.modifierValue);}
            lines.Add("");lines.Add(d.description);return string.Join("\n",lines);
        }
        static string Format(double value,string unit)
        {string number=value.ToString("0.##",System.Globalization.CultureInfo.InvariantCulture);return unit=="D%"?"D"+number+"%":unit=="x"?"×"+number:number+(unit=="초"?Loc.T("초"):unit);}
    }
}
