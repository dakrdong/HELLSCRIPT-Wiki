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
    [Serializable] public sealed class EdictStylePick { public string scope,preset,classes=""; }
    // A combat style is a named bundle of existing group presets; it owns no option values of its own.
    [Serializable] public sealed class EdictStyle
    {
        public string id,name,nameEn,description,descriptionEn;
        public EdictStylePick[] picks=Array.Empty<EdictStylePick>();
        public string Name=>Loc.Language=="en"?nameEn:name;
        public string Description=>Loc.Language=="en"?descriptionEn:description;
        public IEnumerable<EdictStylePick> For(string heroClass)=>picks.Where(p=>string.IsNullOrEmpty(p.classes)||p.classes.Split(',').Contains(heroClass));
    }
    [Serializable] public sealed class EdictQuickPresetCatalog
    { public int version; public EdictQuickPreset[] presets; public EdictStyle[] styles; public SkillPresetSelection[] retired=Array.Empty<SkillPresetSelection>(); }

    // Quick presets are recipes for existing option values, never a second combat/save format.
    // Each recipe fully defines its own scope and is applied to a detached draft atomically.
    public static class HuntEdictQuickPresets
    {
        public const string Custom="custom",AttackOrder="order",NewHeroStyle="balanced";
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
                ValidateStyles(loaded);
                catalog=loaded;return catalog;
            }
        }
        // Every style must name authored global presets and cover the same groups once per class,
        // so choosing another style always replaces the whole bundle.
        static void ValidateStyles(EdictQuickPresetCatalog loaded)
        {
            var styles=loaded.styles??Array.Empty<EdictStyle>();
            bool Authored(EdictStylePick p)=>p!=null&&p.scope.StartsWith("global/",StringComparison.Ordinal)&&loaded.presets.Any(q=>q.scope==p.scope&&q.id==p.preset);
            string Scopes(EdictStyle s,string cls)=>string.Join("|",s.For(cls).Select(p=>p.scope).OrderBy(x=>x,StringComparer.Ordinal));
            if(styles.Length==0||styles.Select(s=>s?.id).Distinct().Count()!=styles.Length||styles.Any(s=>s==null||string.IsNullOrWhiteSpace(s.id)||
                new[]{s.name,s.nameEn,s.description,s.descriptionEn}.Any(string.IsNullOrWhiteSpace)||s.picks==null||!s.picks.All(Authored)||
                HuntEdict.ClassIds.Any(c=>s.For(c).Select(p=>p.scope).Distinct().Count()!=s.For(c).Count()||Scopes(s,c)!=Scopes(styles[0],c))))
                throw new InvalidOperationException("Invalid combat-style catalog.");
        }
        public static IReadOnlyList<EdictStyle> Styles=>Catalog.styles;
        // Only published, explicitly retired IDs migrate. Never accept arbitrary unknown metadata.
        public static bool Retired(string scope,string id)=>Catalog.retired.Any(p=>p.scope==scope&&p.preset==id);
        public static EdictStyle Style(string id)=>Catalog.styles.SingleOrDefault(s=>s.id==id)??throw new ArgumentException("Unknown combat style: "+id);
        // The groups a style decides for this class, in catalog order.
        public static string[] StyleScopes(string heroClass)=>Catalog.styles[0].For(heroClass).Select(p=>p.scope).ToArray();
        public static HuntEdictLoadout ApplyStyle(HuntEdictLoadout source,string id)
        {
            var next=source;
            foreach(var pick in Style(id).For(source.edict.heroClass))next=Apply(next,pick.scope,pick.preset);
            return next;
        }
        // Null when the current values differ from every style in at least one of its groups.
        public static string MatchStyle(HuntEdictLoadout source)=>
            Catalog.styles.FirstOrDefault(s=>s.For(source.edict.heroClass).All(p=>Matches(source,p.scope,p.preset)))?.id;
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
            =>Apply(source,scope,presetId,true);
        // Read-only recipe comparison also covers stored policies for unequipped skills. Reuse
        // the same recipe writer on a detached copy without changing equipment or enabling it.
        static HuntEdictLoadout Apply(HuntEdictLoadout source,string scope,string presetId,bool requireEquipped)
        {
            var preset=For(scope).SingleOrDefault(p=>p.id==presetId)??throw new ArgumentException("Unknown quick preset: "+presetId);
            var next=HuntEdictLoadout.Canonical(source);
            if(scope.StartsWith("global/",StringComparison.Ordinal))
            {
                var group=Group(scope);ValidateValues(preset,group.ids);
                foreach(string id in group.ids)next.edict.global.Single(o=>o.id==id).value=GlobalValue(next,preset,id);
            }
            else if(scope==AttackOrder)
            {
                if(!next.UsesTree)throw new ArgumentException("Attack-order presets require the current skill tree.");
                var skills=next.classSkills;var ordinary=skills.actives.Where(id=>id!="");var ultimate=skills.ultimate==""?Array.Empty<string>():new[]{skills.ultimate};
                if(preset.id=="identity")
                {
                    var setup=new[]{"W15","W07","W11","W12","A18","A15","A05","M15","M12","M11"};
                    var equipped=ordinary.Concat(ultimate).ToArray();
                    skills.order=setup.Where(equipped.Contains).Concat(equipped.Where(id=>!setup.Contains(id))).Concat(new[]{"BASIC"}).ToArray();
                }
                else skills.order=preset.id=="basic-first"?new[]{"BASIC"}.Concat(ordinary).Concat(ultimate).ToArray():
                    preset.id=="ultimate-first"?ultimate.Concat(ordinary).Concat(new[]{"BASIC"}).ToArray():ordinary.Concat(ultimate).Concat(new[]{"BASIC"}).ToArray();
            }
            else
            {
                string skill=Skill(scope,next,requireEquipped);
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
                    if(next.classSkills.Equipped(skill)&&!next.classSkills.automatic.Contains(skill))next.classSkills.automatic=next.classSkills.automatic.Concat(new[]{skill}).ToArray();
                }
            }
            next=HuntEdictLoadout.Canonical(next);
            // The projection may reorder legacy attacks. Keep both representations aligned before
            // entering the edit session so saving/reapplying cannot introduce a second change.
            if(next.UsesTree)next.classSkills.legacyEdict=next.edict.Copy();
            return next;
        }
        // The value a global recipe writes for one option of its group.
        static string GlobalValue(HuntEdictLoadout source,EdictQuickPreset preset,string id)
        {
            var definition=EdictOptions.Global.Single(d=>d.id==id);
            string value=preset.values.FirstOrDefault(v=>v.id==id)?.value??definition.initial;
            if(value=="@equipped")value=source.edict.slots.FirstOrDefault(s=>s!=""&&definition.choices.Contains(s))??"";
            return definition.CanonicalValue(value,source.edict.heroClass);
        }
        static void ValidateValues(EdictQuickPreset preset,IEnumerable<string> owned)
        {
            var ids=owned.ToHashSet();
            if(preset.values.Any(v=>v==null||!ids.Contains(v.id))||preset.values.Select(v=>v.id).Distinct().Count()!=preset.values.Length)
                throw new ArgumentException("A quick preset contains an unrelated or repeated option.");
        }
        static HuntEdictUiGroup Group(string scope)=>HuntEdictUiCatalog.Data.groups.Single(g=>GlobalScope(g)==scope);
        static string Skill(string scope,HuntEdictLoadout source,bool requireEquipped)
        {
            if(scope=="basic/"+source.edict.heroClass)return "BASIC";
            if(!scope.StartsWith("skill/",StringComparison.Ordinal))throw new ArgumentException("Quick-preset class mismatch.");
            string id=scope.Substring(6);var skill=ClassSkills.Find(id);
            if(skill==null||skill.Passive||skill.heroClass!=HuntEdict.ClassIndex(source.edict.heroClass)||
                requireEquipped&&(source.UsesTree?!source.classSkills.Equipped(id):!source.edict.slots.Contains(id)))
                throw new ArgumentException("Quick presets require an equipped skill of this class.");
            return id;
        }
        public static string Match(HuntEdictLoadout source,string scope)=>For(scope).FirstOrDefault(p=>Matches(source,scope,p.id))?.id??Custom;
        public static bool Matches(HuntEdictLoadout source,string scope,string presetId)
        {
            // A global recipe only writes its own group's values, so compare those directly instead of
            // copying and re-projecting the whole loadout. Callers hold canonical drafts.
            if(scope.StartsWith("global/",StringComparison.Ordinal))
            {
                var preset=For(scope).SingleOrDefault(p=>p.id==presetId)??throw new ArgumentException("Unknown quick preset: "+presetId);
                return Group(scope).ids.All(id=>HuntEdictSummary.Value(source.edict,id)==GlobalValue(source,preset,id));
            }
            return Fingerprint(source,scope)==Fingerprint(Apply(source,scope,presetId,false),scope);
        }
        static string Fingerprint(HuntEdictLoadout source,string scope)
        {
            if(scope.StartsWith("global/",StringComparison.Ordinal))return string.Join("|",Group(scope).ids.Select(id=>id+"="+HuntEdictSummary.Value(source.edict,id)));
            if(scope==AttackOrder)return string.Join(",",source.classSkills.order);
            string skill=Skill(scope,source,false);var fields=new List<string>();
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
