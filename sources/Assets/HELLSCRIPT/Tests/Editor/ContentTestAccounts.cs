namespace Hellscript.Tests
{
    // Combat tests assume access to training. Establish that prerequisite before snapshots;
    // keep fresh-account and unlock-boundary tests on GameStore.NewAccount.
    static class ContentTestAccounts
    {
        public static AccountSave Training(GameCatalog catalog=null)
        {
            var account=GameStore.NewAccount(catalog);
            ContentUnlocks.RecordRunEnd(account);
            return account;
        }
    }
}
