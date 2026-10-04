using System;

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
