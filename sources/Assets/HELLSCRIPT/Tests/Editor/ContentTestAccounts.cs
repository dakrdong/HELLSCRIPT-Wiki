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
        public static AccountSave Legacy(GameCatalog catalog=null)
        {
            var account=GameStore.NewAccount(catalog);
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
    }
}
