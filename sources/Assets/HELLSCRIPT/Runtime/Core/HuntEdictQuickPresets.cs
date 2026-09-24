using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    [Serializable] public sealed class EdictQuickPreset
    {
        public string scope,id,name,nameEn,description,descriptionEn;
        public EdictOption[] values=Array.Empty<EdictOption>();
        public ClassSkillOptionSelection[] choices=Array.Empty<ClassSkillOptionSelection>();
        public string Name=>Loc.Language=="en"?nameEn:name;
        public string Description=>ClassSkillTree.Display(Loc.Language=="en"?descriptionEn:description);
    }
    [Serializable] public sealed class EdictQuickPresetCatalog
    { public int version; public EdictQuickPreset[] presets; }

    // Quick presets are recipes for existing option values, never a second combat/save format.
    // Each recipe fully defines its own scope and is applied to a detached draft atomically.
    public static class HuntEdictQuickPresets
    {
        public const string Custom="custom",AttackOrder="order";
        static EdictQuickPresetCatalog catalog;
        public static string GlobalScope(HuntEdictUiGroup group)=>"global/"+HuntEdictSummary.Key(group);
        public static string SkillScope(string id,string heroClass)=>id=="BASIC"?"basic/"+heroClass:"skill/"+id;
        static EdictQuickPresetCatalog Catalog
        {
            get
            {
                if(catalog!=null)return catalog;
                var loaded=JsonUtility.FromJson<EdictQuickPresetCatalog>(Resources.Load<TextAsset>("HuntEdictQuickPresets").text);
                if(loaded.version!=1||loaded.presets==null||loaded.presets.Any(p=>p==null||string.IsNullOrWhiteSpace(p.scope)||string.IsNullOrWhiteSpace(p.id)||p.id==Custom||
                    string.IsNullOrWhiteSpace(p.name)||string.IsNullOrWhiteSpace(p.nameEn)||string.IsNullOrWhiteSpace(p.description)||string.IsNullOrWhiteSpace(p.descriptionEn))||
                    loaded.presets.Select(p=>p.scope+"/"+p.id).Distinct().Count()!=loaded.presets.Length)
                    throw new InvalidOperationException("Invalid quick-preset catalog.");
                catalog=loaded;return catalog;
            }
        }
        public static IReadOnlyList<EdictQuickPreset> For(string scope)
        {
            var authored=Catalog.presets.Where(p=>p.scope==scope).ToArray();
            if(authored.Length>0)return authored;
            if(!scope.StartsWith("skill/",StringComparison.Ordinal))throw new ArgumentException("Unknown quick-preset scope: "+scope);
            string id=scope.Substring(6);var definition=ClassSkillOptions.Find(id);
            if(definition==null)throw new ArgumentException("This skill has no configurable policy: "+id);
            // Native skills already have named, executable use policies with bilingual explanations.
            // Reuse that owner instead of maintaining duplicate combat conditions here.
            return definition.choices.Select(c=>new EdictQuickPreset
            {
                scope=scope,id=c.id,name=c.name,nameEn=c.nameEn,description=c.benefit+"\n"+c.cost,descriptionEn=c.benefitEn+"\n"+c.costEn,
                choices=new[]{new ClassSkillOptionSelection{id=definition.id,choice=c.id}}
            }).ToArray();
        }
        public static HuntEdictLoadout Apply(HuntEdictLoadout source,string scope,string presetId)
        {
            var preset=For(scope).SingleOrDefault(p=>p.id==presetId)??throw new ArgumentException("Unknown quick preset: "+presetId);
            var next=HuntEdictLoadout.Canonical(source);
            if(scope.StartsWith("global/",StringComparison.Ordinal))
            {
                var group=Group(scope);ValidateValues(preset,group.ids);
                foreach(string id in group.ids)
                {
                    var definition=EdictOptions.Global.Single(d=>d.id==id);
                    string value=preset.values.FirstOrDefault(v=>v.id==id)?.value??definition.initial;
                    if(value=="@equipped")value=next.edict.slots.FirstOrDefault(s=>s!=""&&definition.choices.Contains(s))??"";
                    next.edict.global.Single(o=>o.id==id).value=definition.CanonicalValue(value,next.edict.heroClass);
                }
            }
            else if(scope==AttackOrder)
            {
                if(!next.UsesTree)throw new ArgumentException("Attack-order presets require the current skill tree.");
                var skills=next.classSkills;var ordinary=skills.actives.Where(id=>id!="");var ultimate=skills.ultimate==""?Array.Empty<string>():new[]{skills.ultimate};
                skills.order=preset.id=="basic-first"?new[]{"BASIC"}.Concat(ordinary).Concat(ultimate).ToArray():
                    preset.id=="ultimate-first"?ultimate.Concat(ordinary).Concat(new[]{"BASIC"}).ToArray():ordinary.Concat(ultimate).Concat(new[]{"BASIC"}).ToArray();
            }
            else
            {
                string skill=Skill(scope,next);
                var definitions=skill=="BASIC"||ClassSkills.Find(skill).legacy?HuntEdictV2Options.ForSkill(skill,next.edict.heroClass):Array.Empty<EdictCoreOptionDefinition>();
                ValidateValues(preset,Enumerable.Range(1,definitions.Count).Select(i=>i.ToString()));
                // Reset every owned legacy field before overrides; previous custom values cannot leak.
                for(int i=0;i<definitions.Count;i++)
                    next.edict=HuntEdictV2.WithOption(next.edict,skill,i+1,preset.values.FirstOrDefault(v=>v.id==(i+1).ToString())?.value??definitions[i].initial);
                if(next.UsesTree)
                {
                    var options=ClassSkillOptions.For((HeroClass)next.classSkills.heroClass).Where(o=>o.skillId==skill).ToArray();
                    if(preset.choices.Select(c=>c.id).Distinct().Count()!=preset.choices.Length||preset.choices.Any(c=>!options.Any(o=>o.id==c.id&&o.choices.Any(v=>v.id==c.choice))))
                        throw new ArgumentException("A quick preset contains an unrelated skill policy.");
                    next.classSkills.options.RemoveAll(o=>options.Any(d=>d.id==o.id));
                    foreach(var option in options)next.classSkills.options.Add(new ClassSkillOptionSelection{id=option.id,choice=preset.choices.FirstOrDefault(c=>c.id==option.id)?.choice??option.initial});
                    if(!next.classSkills.automatic.Contains(skill))next.classSkills.automatic=next.classSkills.automatic.Concat(new[]{skill}).ToArray();
                }
            }
            next=HuntEdictLoadout.Canonical(next);
            // The projection may reorder legacy attacks. Keep both representations aligned before
            // entering the edit session so saving/reapplying cannot introduce a second change.
            if(next.UsesTree)next.classSkills.legacyEdict=next.edict.Copy();
            return next;
        }
        static void ValidateValues(EdictQuickPreset preset,IEnumerable<string> owned)
        {
            var ids=owned.ToHashSet();
            if(preset.values.Any(v=>v==null||!ids.Contains(v.id))||preset.values.Select(v=>v.id).Distinct().Count()!=preset.values.Length)
                throw new ArgumentException("A quick preset contains an unrelated or repeated option.");
        }
        static HuntEdictUiGroup Group(string scope)=>HuntEdictUiCatalog.Data.groups.Single(g=>GlobalScope(g)==scope);
        static string Skill(string scope,HuntEdictLoadout source)
        {
            if(scope=="basic/"+source.edict.heroClass)return "BASIC";
            if(!scope.StartsWith("skill/",StringComparison.Ordinal))throw new ArgumentException("Quick-preset class mismatch.");
            string id=scope.Substring(6);var skill=ClassSkills.Find(id);
            if(skill==null||skill.Passive||skill.heroClass!=HuntEdict.ClassIndex(source.edict.heroClass)||
                source.UsesTree&&!source.classSkills.Equipped(id)||!source.UsesTree&&!source.edict.slots.Contains(id))
                throw new ArgumentException("Quick presets require an equipped skill of this class.");
            return id;
        }
        public static string Match(HuntEdictLoadout source,string scope)
        {
            string current=Fingerprint(source,scope);
            foreach(var preset in For(scope))if(Fingerprint(Apply(source,scope,preset.id),scope)==current)return preset.id;
            return Custom;
        }
        public static bool Matches(HuntEdictLoadout source,string scope,string presetId)=>
            Fingerprint(source,scope)==Fingerprint(Apply(source,scope,presetId),scope);
        static string Fingerprint(HuntEdictLoadout source,string scope)
        {
            if(scope.StartsWith("global/",StringComparison.Ordinal))return string.Join("|",Group(scope).ids.Select(id=>id+"="+HuntEdictSummary.Value(source.edict,id)));
            if(scope==AttackOrder)return string.Join(",",source.classSkills.order);
            string skill=Skill(scope,source);var fields=new List<string>();
            if(skill=="BASIC"||ClassSkills.Find(skill).legacy)
                for(int i=1;i<=HuntEdictV2Options.ForSkill(skill,source.edict.heroClass).Count;i++)fields.Add(HuntEdictV2.Value(source.edict,skill,i));
            if(source.UsesTree)
            {
                fields.Add(source.classSkills.automatic.Contains(skill).ToString());
                foreach(var option in ClassSkillOptions.For((HeroClass)source.classSkills.heroClass).Where(o=>o.skillId==skill))fields.Add(option.id+"="+ClassSkillOptions.Selected(source.classSkills,option.id).id);
            }
            return string.Join("|",fields);
        }
    }
}
