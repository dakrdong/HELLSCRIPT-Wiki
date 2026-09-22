using UnityEngine;

namespace Hellscript
{
    public sealed partial class GameUI
    {
        readonly CharacterSelectionState characterSelection=new CharacterSelectionState();
        public TitleSession EntrySession=>titleSession;
        public CharacterSelectionView CharacterScreen {get;private set;}
        public void OpenCharacterSelection()
        {
            if(!titleSession.SignedIn){ShowTitle();return;}
            characterSelection.Reset();ShowCharacterSelection();
        }
        public void ShowCharacterSelection()
        {
            if(!titleSession.SignedIn){ShowTitle();return;}
            game.Town?.Cancel();game.World.ClearDungeon();titleSession.CancelEntry();
            pageRepaint=ShowCharacterSelection;Base("characters","HELLSCRIPT","캐릭터 선택하기",responsive:true);
            header.gameObject.SetActive(false);footer.gameObject.SetActive(false);
            headerApron.gameObject.SetActive(false);footerApron.gameObject.SetActive(false);content.parent.gameObject.SetActive(false);
            var backdrop=Rect("Character sanctuary",shell);Stretch(backdrop);
            var atmosphere=backdrop.gameObject.AddComponent<TitleAtmosphere>();atmosphere.Initialize(titleSession,"Art/CharacterSelection/SelectionCourtyard");
            var shade=Box("Selection scene shade",shell,new Color(.018f,.022f,.028f,.28f));Stretch(shade);shade.GetComponent<UnityEngine.UI.Image>().raycastTarget=false;
            var page=Rect("Character selection",root);Stretch(page);
            CharacterScreen=page.gameObject.AddComponent<CharacterSelectionView>();
            CharacterScreen.Initialize(game,font,characterSelection,titleSession,atmosphere);
            overlay.SetAsLastSibling();RefreshGlobalHud();
        }
    }
}
