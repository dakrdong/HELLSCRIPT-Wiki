namespace Hellscript
{
    public sealed partial class GameController
    {
        // Keep the title flow separate from the in-game switch that deliberately abandons a run.
        public bool CommitEntryCharacter(int index)
        {
            if(Running||UI.Page!="characters"||!UI.EntrySession.SignedIn)return false;
            return SaveEntryCharacter(index);
        }
        bool SaveEntryCharacter(int index)
        {
            if(!CharacterSelectionState.CanChoose(Store.Data,index))return false;
            int previous=Store.Data.selectedHero;Store.Data.selectedHero=index;
            if(!Store.Save()){Store.Data.selectedHero=previous;UI.ShowToast(Store.Error);return false;}
            CancelPotionDeparture();SelectedStage=Store.Data.Hero.highestClear+1;return true;
        }
    }
}
