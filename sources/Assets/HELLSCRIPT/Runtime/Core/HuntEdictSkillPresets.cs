using System;
using System.Collections.Generic;
using System.Linq;

namespace Hellscript
{
    [Serializable] public sealed class SkillPresetSelection { public string scope,preset; }

    // Records the explicitly activated tab, including Custom. Combat still reads the existing options.
    public static class HuntEdictSkillPresets
    {
        public static bool Owns(string scope)=>scope==HuntEdictQuickPresets.AttackOrder||scope?.StartsWith("skill/",StringComparison.Ordinal)==true||scope?.StartsWith("basic/",StringComparison.Ordinal)==true;
        static string Skill(string scope,int heroClass)
        {
            if(scope==HuntEdictQuickPresets.AttackOrder)return null;
            if(scope=="basic/"+HuntEdict.ClassId((HeroClass)heroClass))return "BASIC";
            string id=scope?.StartsWith("skill/",StringComparison.Ordinal)==true?scope.Substring(6):null;
            var definition=ClassSkills.Find(id);
            if(definition==null||definition.Passive||definition.heroClass!=heroClass)throw new ArgumentException("Invalid skill-preset scope.");
            return id;
        }
        public static void ValidateSelections(ClassSkillLoadout load)
        {
            load.presetSelections??=new List<SkillPresetSelection>();
            if(load.presetSelections.Count>20||load.presetSelections.Any(p=>p==null)||load.presetSelections.Select(p=>p.scope).Distinct().Count()!=load.presetSelections.Count)
                throw new ArgumentException("Invalid skill-preset selections.");
            foreach(var p in load.presetSelections)
            {
                Skill(p.scope,load.heroClass);
                if(p.preset!=HuntEdictQuickPresets.Custom&&!HuntEdictQuickPresets.For(p.scope).Any(q=>q.id==p.preset))throw new ArgumentException("Unknown skill preset.");
            }
            load.presetSelections=load.presetSelections.OrderBy(p=>p.scope,StringComparer.Ordinal).ToList();
        }
        public static string Active(HuntEdictLoadout load,string scope)
        {
            var selected=load.classSkills?.presetSelections?.FirstOrDefault(p=>p.scope==scope);
            if(selected?.preset==HuntEdictQuickPresets.Custom)return selected.preset;
            if(selected!=null&&HuntEdictQuickPresets.Matches(load,scope,selected.preset))return selected.preset;
            return HuntEdictQuickPresets.Match(load,scope);
        }
        public static HuntEdictLoadout Select(HuntEdictLoadout source,string scope,string preset)
        {
            if(!source.UsesTree)throw new ArgumentException("Skill presets require the skill tree.");
            Skill(scope,source.classSkills.heroClass);
            var next=preset==HuntEdictQuickPresets.Custom?HuntEdictLoadout.Canonical(source):HuntEdictQuickPresets.Apply(source,scope,preset);
            next.classSkills.presetSelections.RemoveAll(p=>p.scope==scope);
            next.classSkills.presetSelections.Add(new SkillPresetSelection{scope=scope,preset=preset});
            return HuntEdictLoadout.Canonical(next);
        }
        public static bool SamePolicy(HuntEdictLoadout source,HuntEdictLoadout owned,string scope)
        {
            owned=HuntEdictLoadout.Canonical(owned);
            var projected=CopyPolicy(source,owned,scope);
            // The explicit tab marker is presentation state; compare the executable policy separately.
            projected.classSkills.presetSelections=owned.classSkills.presetSelections;
            return UnityEngine.JsonUtility.ToJson(projected)==UnityEngine.JsonUtility.ToJson(owned);
        }
        // Copy only one policy into the owned build. Allocation, slots and unrelated tabs stay untouched,
        // even when the edited skill has only been equipped in the window's unsaved draft.
        public static HuntEdictLoadout CopyPolicy(HuntEdictLoadout source,HuntEdictLoadout destination,string scope)
        {
            var next=HuntEdictLoadout.Canonical(destination);source=HuntEdictLoadout.Canonical(source);
            if(!source.UsesTree||!next.UsesTree||source.classSkills.heroClass!=next.classSkills.heroClass)throw new ArgumentException("Different skill-preset class.");
            string skill=Skill(scope,next.classSkills.heroClass);
            if(skill==null)
            {
                // Uncommitted equipment may differ; never insert its actions into the saved attack order.
                var actions=next.classSkills.order;
                next.classSkills.order=source.classSkills.order.Where(actions.Contains).Concat(actions.Where(id=>!source.classSkills.order.Contains(id))).ToArray();
            }
            else
            {
                if(skill=="BASIC"||ClassSkills.Find(skill).legacy)
                    for(int i=1;i<=HuntEdictV2Options.ForSkill(skill,next.edict.heroClass).Count;i++)
                        next.edict=HuntEdictV2.WithOption(next.edict,skill,i,HuntEdictV2.Value(source.edict,skill,i));
                var options=ClassSkillOptions.For((HeroClass)next.classSkills.heroClass).Where(o=>o.skillId==skill).Select(o=>o.id).ToHashSet();
                next.classSkills.options.RemoveAll(o=>options.Contains(o.id));
                next.classSkills.options.AddRange(source.classSkills.options.Where(o=>options.Contains(o.id)).Select(o=>new ClassSkillOptionSelection{id=o.id,choice=o.choice}));
                if(next.classSkills.Equipped(skill))
                {
                    if(!source.classSkills.automatic.Contains(skill))next.classSkills.automatic=next.classSkills.automatic.Where(id=>id!=skill).ToArray();
                    else if(!next.classSkills.automatic.Contains(skill))next.classSkills.automatic=next.classSkills.automatic.Concat(new[]{skill}).ToArray();
                }
            }
            next.classSkills.presetSelections.RemoveAll(p=>p.scope==scope);
            next.classSkills.presetSelections.AddRange(source.classSkills.presetSelections.Where(p=>p.scope==scope).Select(p=>new SkillPresetSelection{scope=p.scope,preset=p.preset}));
            next.classSkills.legacyEdict=next.edict.Copy();
            return HuntEdictLoadout.Canonical(next);
        }
    }
}
