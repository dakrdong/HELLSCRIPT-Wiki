using System;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    public sealed partial class GameStore
    {
        // Production entrypoint: atomically migrate owned heroes. A legacy suspended run keeps its
        // snapshot until it returns, or the player explicitly edits it through the unified transaction.
        public bool ActivateSkillTrees(GameCatalog catalog)
        {
            try
            {
                if(!Data.heroes.Any(h=>!h.classSkillValidation&&!ClassSkillTree.Uses(h.build.classSkills)&&Data.suspendedRun?.heroId!=h.id))return true;
                var staged=JsonUtility.FromJson<AccountSave>(JsonUtility.ToJson(Data));bool changed=false;
                foreach(var hero in staged.heroes)
                {
                    if(hero.classSkillValidation||ClassSkillTree.Uses(hero.build.classSkills)||staged.suspendedRun?.heroId==hero.id)continue;
                    hero.edictLegacyBuild??=hero.build.Copy();
                    bool edictEnabled=hero.useEdict;var skills=ClassSkillTree.FromHero(hero);
                    ApplyHuntEdict(hero,new HuntEdictLoadout{version=2,classSkills=skills,edict=skills.ProjectEdict()});
                    hero.useEdict=edictEnabled;ClassSkillLoadout.Validate(hero.build.classSkills,hero);changed=true;
                }
                if(!changed)return true;
                if(!Write(staged))return false;
                for(int i=0;i<Data.heroes.Count;i++)
                {AdoptHuntEdict(Data.heroes[i],staged.heroes[i]);Data.heroes[i].edictLegacyBuild=staged.heroes[i].edictLegacyBuild;}
                Error="";NotifyCommitted("hunt-edict");return true;
            }
            catch(Exception e){Error=Loc.F("사냥 칙령을 저장하지 못했습니다: {0}",e.Message);return false;}
        }
    }
}
