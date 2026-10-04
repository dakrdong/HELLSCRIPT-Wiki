using System;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    public sealed partial class GameStore
    {
        public bool SetRecommendedEdict(string heroId,bool enabled,GameCatalog catalog)
        {
            try
            {
                if(Data.Hero.id!=heroId)throw new ArgumentException(Loc.T("설정을 편집한 캐릭터를 먼저 선택하세요."));
                var staged=JsonUtility.FromJson<AccountSave>(JsonUtility.ToJson(Data));
                staged.Hero.useEdict=true;staged.Hero.useRecommendedEdict=enabled;
                HuntEdictV2.ValidateReceiver(HuntEdictDefaults.Resolve(staged.Hero),staged.Hero,catalog);
                staged.lastSeenUtc=SeenNow();if(!Write(staged))return false;
                Data.Hero.useEdict=true;Data.Hero.useRecommendedEdict=enabled;Data.lastSeenUtc=staged.lastSeenUtc;
                Error="";NotifyCommitted("edict");return true;
            }
            catch(Exception e){Error=Loc.F("사냥 칙령을 저장하지 못했습니다: {0}",e.Message);return false;}
        }
        // Validate, write the file, then apply in memory. A save that fails must leave the hero
        // fighting exactly as before rather than with a document that never reached disk.
        public bool SaveHeroEdict(string heroId,HuntEdictV2Document source,GameCatalog catalog,bool? enabled=null)
        {
            try
            {
                var owner=Data.heroes.SingleOrDefault(h=>h.id==heroId);
                if(owner==null||owner!=Data.Hero)throw new ArgumentException("사냥 칙령을 편집한 캐릭터를 먼저 선택하세요.");
                if(catalog==null)throw new ArgumentException("현재 스킬 카탈로그를 확인하세요.");
                if(!ClassSkillLoadout.IsAbsent(owner.build.classSkills))throw new ArgumentException("Use the ID-based skill loadout.");
                var canonical=HuntEdictV2.Canonical(source);
                ValidateUtilityChange(owner,canonical);
                var gated=HuntEdictLoadout.FromHero(owner);gated.edict=canonical;HuntEdictProgression.ValidateChange(Data,owner,gated);
                HuntEdictV2.ValidateReceiver(canonical,owner,catalog);
                // Unlock levels are not re-checked here. The loadout itself keeps not-yet-unlocked
                // skills and only disables their rules, so requiring more of the document than of
                // BuildConfig would reject a hero's own settings. Manual slot edits go through
                // WithSlot and received codes through Import.Prepare; both enforce unlocks already.
                var staged=JsonUtility.FromJson<AccountSave>(JsonUtility.ToJson(Data));
                var hero=staged.heroes.Single(h=>h.id==heroId);
                hero.edict=canonical;if(enabled.HasValue)hero.useEdict=enabled.Value;
                staged.lastSeenUtc=SeenNow();
                if(!Write(staged))return false;
                owner.edict=canonical;if(enabled.HasValue)owner.useEdict=enabled.Value;
                Data.lastSeenUtc=staged.lastSeenUtc;Error="";NotifyCommitted("edict");return true;
            }
            catch(Exception e){Error=Loc.F("사냥 칙령을 저장하지 못했습니다: {0}", e.Message);return false;}
        }

        public bool SetHeroEdictEnabled(string heroId,bool enabled,GameCatalog catalog)
        {
            var owner=Data.heroes.SingleOrDefault(h=>h.id==heroId);
            if(owner==null){Error="사냥 칙령을 저장하지 못했습니다: 캐릭터를 찾을 수 없습니다.";return false;}
            if(HuntEdictV2Storage.IsAbsent(owner.edict)){Error="사냥 칙령을 저장하지 못했습니다: 저장된 원본이 없습니다.";return false;}
            if(ClassSkillTree.Uses(owner.build.classSkills))
            {
                try
                {
                    if(owner!=Data.Hero)throw new ArgumentException(Loc.T("설정을 편집한 캐릭터를 먼저 선택하세요."));
                    var staged=JsonUtility.FromJson<AccountSave>(JsonUtility.ToJson(Data));staged.Hero.useEdict=enabled;
                    staged.lastSeenUtc=SeenNow();if(!Write(staged))return false;
                    owner.useEdict=enabled;Data.lastSeenUtc=staged.lastSeenUtc;Error="";NotifyCommitted("edict");return true;
                }
                catch(Exception e){Error=Loc.F("사냥 칙령을 저장하지 못했습니다: {0}",e.Message);return false;}
            }
            return SaveHeroEdict(heroId,owner.edict,catalog,enabled);
        }
    }
}
