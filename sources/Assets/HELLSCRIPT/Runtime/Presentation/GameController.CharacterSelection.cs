namespace Hellscript
{
    public sealed partial class GameController
    {
        // Previewing is read-only; entry commits recovery and hero ownership together.
        public bool CommitEntryCharacter(int index)
        {
            if(Running||UI.Page!="characters"||!UI.EntrySession.SignedIn||!AccountStorageReady)return false;
            return SaveEntryCharacter(index);
        }
        // The first start has no class choice: the tutorial stage is the warrior's, so the warrior enters at once.
        public bool EnterFirstStart()
        {
            if(Running||!UI.EntrySession.SignedIn||!AccountStorageReady)return false;
            int warrior=GuideTutorial.WarriorIndex(Store.Data);
            if(warrior<0||!SaveEntryCharacter(warrior)||!UI.EntrySession.BeginEntry())return false;
            Audio?.Play("flow.game_start");EnterPlaza(true);return true;
        }
        bool SaveEntryCharacter(int index)
        {
            if(!CharacterSelectionState.CanChoose(Store.Data,index)||!HeroSlots.IsCreated(Store.Data,index))return false;
            if(Store.Data.guide.heroSelectionPending)
            {
                if(!Store.CompleteTutorialHeroSelection(index)){UI.ShowToast(Store.Error);return false;}
                CancelPotionDeparture();SelectedStage=Store.Data.Hero.highestClear+1;return true;
            }
            if(!Store.EnterCharacter(index,catalog)){UI.ShowToast(Store.Error);return false;}
            CancelPotionDeparture();SelectedStage=Store.Data.Hero.highestClear+1;return true;
        }
        public void EnterSelectedCharacter()
        {
            if(RiftInterruption.Applies(Store.Data.suspendedRun)){Begin(resume:true);return;}
            if(RepeatSession?.pendingResult!=null){ResumeRepeatResult();return;}
            EnterPlaza(true);
        }
        bool RecoverInterruptedRift()
        {
            var saved=Store.Data.suspendedRun;
            if(!Store.ResumeRiftSession(catalog)){Notify(Store.Error);return false;}
            if(saved==null||Store.Data.suspendedRun!=null)return true;
            UI.CloseCommonPanel();RestoreForegroundClock();
            if(Combat!=null){Combat.Visual-=World.Effect;Combat=null;}
            EnterSelectedCharacter();return false;
        }
    }
}
