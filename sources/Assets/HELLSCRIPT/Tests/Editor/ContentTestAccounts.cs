using System;
using System.Linq;

namespace Hellscript.Tests
{
    // A new account fights through the unified hunt edict. Tests of the legacy rule engine ask
    // for the pre-edict hero explicitly: the recommended legacy build, a document seeded from
    // it, and the edict switched off. Combat tests also assume access to training, so that
    // prerequisite is established before snapshots; keep fresh-account and unlock-boundary
    // tests on GameStore.NewAccount.
    static class ContentTestAccounts
    {
        // Domain-contract fixtures with existing full access retain their pre-disclosure automation defaults.
        // Fresh progression and permission tests deliberately do not call this helper.
        public static void ReleasedAccess(AccountSave account)
        {
            account.guide.edictLegacyAccess=true;HuntEdictProgression.Reconcile(account);
            foreach(var hero in account.heroes)
            {
                var defaults=HuntEdictV2.Create(hero.heroClass);
                foreach(string id in new[]{"potion.autoBuy","autoEquip.enabled","repeat.enabled","bag.cleanupAt"})
                    hero.edict=HuntEdictV2Editing.WithGlobal(hero.edict,id,HuntEdictSummary.Value(defaults,id));
                if(!ClassSkillLoadout.IsAbsent(hero.build.classSkills))hero.build.classSkills.legacyEdict=hero.edict.Copy();
            }
        }
        public static void CurrentSkillPolicies(AccountSave account,GameCatalog catalog)
        {
            account.guide.legacyExempt=true;account.guide.mapComplete=true;account.guide.edictLegacyAccess=true;HuntEdictProgression.Reconcile(account);
            foreach(var hero in account.heroes)
            {
                hero.level=60;var load=SkillTreeD4Loadout.Empty(hero.heroClass);var data=SkillTreeD4.For(hero.heroClass);
                foreach(var active in data.actives)load.ranks.Single(r=>r.id==active.id).rank=1;
                foreach(var passive in data.passives.Where(p=>!p.key))load.ranks.Single(r=>r.id==passive.id).rank=5;
                load.actives[0]=new[]{"W01","A01","M01"}[(int)hero.heroClass];load.order=new[]{load.actives[0],"BASIC"};load.automatic=load.order.ToArray();load.legacyEdict=hero.edict.Copy();
                load.ProjectLegacy(hero.build,catalog);hero.edict=load.ProjectEdict();ClassSkillLoadout.Validate(load,hero);
            }
        }
        // Feature fixtures buy their prerequisites through the current tree rules. No
        // free ranks, authored kit allowance, or production gate override is used here.
        public static ClassSkillLoadout OwnSkills(HeroSave hero,GameCatalog catalog,params string[] ids)
        {
            var load=hero.build.classSkills?.version==5?hero.build.classSkills.Copy():SkillTreeD4Loadout.Empty(hero.heroClass);
            var data=SkillTreeD4.For(hero.heroClass);load.legacyEdict=hero.edict.Copy();
            foreach(string id in ids)
            {
                var target=data.actives.Single(a=>a.id==id);
                while(!ClassSkillTree.Unlocked(load,hero.level,id))
                {
                    var prerequisite=data.actives.FirstOrDefault(a=>a.tier<target.tier&&load.Rank(a.id)<a.maxRank&&ClassSkillTree.Unlocked(load,hero.level,a.id));
                    if(prerequisite==null)throw new ArgumentException("Fixture cannot buy this skill's prerequisites: "+id);
                    load=ClassSkillTree.ChangeRank(load,hero,prerequisite.id,1);
                }
                if(load.Rank(id)==0)load=ClassSkillTree.ChangeRank(load,hero,id,1);
            }
            load.ProjectLegacy(hero.build,catalog);hero.edict=load.ProjectEdict();ClassSkillLoadout.Validate(load,hero);return load;
        }
        public static ClassSkillLoadout HistoricalSkills(HeroClass hero,GameCatalog catalog)
        {
            var load=ClassSkillLoadout.FromHero(Legacy(catalog).heroes[(int)hero]);load.version=3;
            return ClassSkillLoadout.Canonical(load);
        }
        public static AccountSave Legacy(GameCatalog catalog=null)
        {
            var account=GameStore.NewAccount(catalog);
            account.guide.edictLegacyAccess=true;HuntEdictProgression.Reconcile(account);
            foreach(var hero in account.heroes)
            {
                hero.build=catalog!=null?BehaviorPresets.ForLevel(hero.heroClass,0,hero.level,catalog):GameCatalog.Preset(hero.heroClass,0);
                hero.build.passives=Array.Empty<int>();hero.edict=HuntEdictV2Storage.CreateForHero(hero);hero.useEdict=false;
            }
            return account;
        }
        public static AccountSave Training(GameCatalog catalog=null)
        {
            var account=Legacy(catalog);
            ContentUnlocks.RecordRunEnd(account);
            return account;
        }
        // A reward-bearing legacy arena is still a real rift checkpoint. Changing a
        // training flag after construction skips its balance/journal/save initialization.
        public static CombatSimulation LegacyRift(AccountSave account,GameCatalog catalog,uint seed)
        {
            var state=new RunState{id=Guid.NewGuid().ToString("N"),heroId=account.Hero.id,stage=1,training=-1,
                rng=seed,layout=RiftLayout.Legacy(0),position=RiftMap.Rooms[0],build=account.Hero.build.Copy()};
            var sim=new CombatSimulation(account,catalog,1,restore:state,recordResume:false);
            state.health=sim.Stats.hp;state.visited.Add(0);return sim;
        }
    }
}
