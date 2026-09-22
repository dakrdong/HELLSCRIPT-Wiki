namespace Hellscript
{
    // Previewing a hero never changes the account. Only the final start action commits it.
    public sealed class CharacterSelectionState
    {
        public int Selected {get;private set;}=-1;
        public int Revision {get;private set;}
        public void Reset(){Selected=-1;Revision++;}
        public bool Choose(AccountSave account,int index)
        {
            if(!CanChoose(account,index))return false;
            if(Selected!=index){Selected=index;Revision++;}return true;
        }
        public static int LockedHero(AccountSave account)
        {
            if(account?.suspendedRun==null)return -1;
            int owner=account.heroes.FindIndex(h=>h.id==account.suspendedRun.heroId);
            return owner<0?-2:owner;
        }
        public static bool CanChoose(AccountSave account,int index)
        {
            if(account==null||index<0||index>=account.heroes.Count)return false;
            int locked=LockedHero(account);return locked==-1||locked==index;
        }
    }
}
