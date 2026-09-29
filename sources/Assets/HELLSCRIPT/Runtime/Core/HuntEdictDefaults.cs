using System;
using System.Linq;
using System.Collections.Generic;

namespace Hellscript
{
    // Recommended mode resolves a detached policy; never replace the player's stored settings.
    public static class HuntEdictDefaults
    {
        static readonly Dictionary<HeroClass,HuntEdictV2Document> templates=new Dictionary<HeroClass,HuntEdictV2Document>();
        public static ClassSkillLoadout Skills(ClassSkillLoadout source)
        {
            var d=ClassSkillLoadout.Canonical(source);
            d.options.Clear();d.presetSelections.Clear();
            d.order=d.actives.Where(s=>s!="").Concat(d.ultimate==""?Array.Empty<string>():new[]{d.ultimate}).Concat(new[]{"BASIC"}).ToArray();
            d.automatic=d.order.ToArray();d.legacyEdict=Document((HeroClass)d.heroClass);
            return d;
        }
        static HuntEdictV2Document Document(HeroClass heroClass)
        {
            if(!templates.TryGetValue(heroClass,out var value))
            {
                var load=new HuntEdictLoadout{edict=HuntEdictV2.Create(heroClass),ranks=SkillProgression.BaseRanks()};
                value=HuntEdictQuickPresets.ApplyStyle(load,HuntEdictQuickPresets.NewHeroStyle).edict;templates.Add(heroClass,value);
            }
            return value.Copy();
        }
        public static HuntEdictV2Document Resolve(HeroSave hero,BuildConfig build=null)
        {
            build??=hero.build;
            if(!hero.useRecommendedEdict)return hero.edict;
            if(!ClassSkillLoadout.IsAbsent(build.classSkills))return Skills(build.classSkills).ProjectEdict();
            var result=Document(hero.heroClass);result.slots=hero.edict.slots.ToArray();return HuntEdictV2.Canonical(result);
        }
    }
}
