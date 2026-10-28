namespace Hellscript
{
    public enum CharacterPlazaPhase { Browse, Selected, Creating, Equipment, Entering }
    // Previewing a hero never changes the account. Only the final start action commits it.
    public sealed class CharacterSelectionState
    {
        public int Selected {get;private set;}=-1;
        public int Revision {get;private set;}
        public CharacterPlazaPhase Phase {get;private set;}
        public HeroClass Focus {get;private set;}=HeroClass.Warrior;
        public float Age {get;private set;}
        public const float CreationSeconds=1.8f;
        public bool Busy=>Phase==CharacterPlazaPhase.Creating||Phase==CharacterPlazaPhase.Entering;
        public bool CreationReady=>Phase==CharacterPlazaPhase.Creating&&Age>=CreationSeconds;
        public void Reset(){Selected=-1;Phase=CharacterPlazaPhase.Browse;Age=0;Focus=HeroClass.Warrior;Revision++;}
        public bool Choose(AccountSave account,int index)
        {
            if(Busy||Phase==CharacterPlazaPhase.Equipment||!CanChoose(account,index))return false;
            if(Selected!=index){Selected=index;Phase=CharacterPlazaPhase.Selected;Age=0;Focus=account.heroes[index].heroClass;Revision++;}
            else if(Focus!=account.heroes[index].heroClass){Focus=account.heroes[index].heroClass;Revision++;}
            return true;
        }
        public bool FocusClass(HeroClass value)
        {if(Busy||Phase==CharacterPlazaPhase.Equipment)return false;if(Focus!=value){Focus=value;Revision++;}return true;}
        public bool BeginCreation(AccountSave account)
        {if(Busy||Phase!=CharacterPlazaPhase.Selected||!CanChoose(account,Selected)||account.heroes[Selected].created)return false;Phase=CharacterPlazaPhase.Creating;Age=0;Revision++;return true;}
        public void Tick(float seconds){if(Busy&&seconds>0)Age+=seconds;}
        public void FinishCreation(){if(Phase!=CharacterPlazaPhase.Creating)return;Phase=CharacterPlazaPhase.Selected;Age=0;Revision++;}
        public bool OpenEquipment(AccountSave account)
        {if(Busy||Phase!=CharacterPlazaPhase.Selected||!CanChoose(account,Selected)||!account.heroes[Selected].created)return false;Phase=CharacterPlazaPhase.Equipment;Revision++;return true;}
        public void CloseEquipment(){if(Phase!=CharacterPlazaPhase.Equipment)return;Phase=CharacterPlazaPhase.Selected;Revision++;}
        public bool BeginEntry(AccountSave account)
        {if(Busy||Phase!=CharacterPlazaPhase.Selected||!CanChoose(account,Selected)||!account.heroes[Selected].created)return false;Phase=CharacterPlazaPhase.Entering;Age=0;Revision++;return true;}
        public static string Line(HeroClass heroClass,HeroGender gender,bool entering)
        {
            switch(HeroSlots.Index(heroClass,gender))
            {
                case 0:return entering?"다 덤벼라! 앞장은 내가 선다!":"좋아, 한판 붙어 보자고!";
                case 3:return entering?"몸 다 풀렸어. 전부 날려 줄게!":"배도 채웠겠다, 덤벼!";
                case 1:return entering?"한 발이면 충분하지!":"흥정 끝! 이제 사냥 시간이야.";
                case 4:return entering?"숨 고르고… 놓치지 않아.":"조용히, 확실하게. 따라와.";
                case 2:return entering?"계산 끝. 불꽃 맛 좀 봐라!":"책에 다 나와 있었다고!";
                default:return entering?"자, 마법 쇼를 시작해 볼까?":"반짝반짝, 마법 시작이야!";
            }
        }
        public static bool CanChoose(AccountSave account,int index)
            =>account!=null&&index>=0&&index<account.heroes.Count;
    }
}
