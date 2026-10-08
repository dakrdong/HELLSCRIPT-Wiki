namespace Hellscript
{
    public sealed partial class GameController
    {
        // Keep the title flow separate from the in-game switch that deliberately abandons a run.
        public bool CommitEntryCharacter(int index)
        {
            if(Running||UI.Page!="characters"||!UI.EntrySession.SignedIn||!AccountStorageReady)return false;
            return SaveEntryCharacter(index);
        }
        bool SaveEntryCharacter(int index)
        {
            if(!CharacterSelectionState.CanChoose(Store.Data,index))return false;
            // The tutorial is each hero's own: choosing a hero continues their share of it.
            int previous=Store.Data.selectedHero;Tutorials.Switch(Store.Data,previous,index);
            if(!Store.Save()){Tutorials.Switch(Store.Data,index,previous);UI.ShowToast(Store.Error);return false;}
            CancelPotionDeparture();SelectedStage=Store.Data.Hero.highestClear+1;return true;
        }
    }
}
