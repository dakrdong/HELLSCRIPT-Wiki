using UnityEngine;

namespace Hellscript
{
    public sealed partial class GameUI
    {
        readonly CharacterSelectionState characterSelection=new CharacterSelectionState();
        public TitleSession EntrySession=>titleSession;
        public CharacterSelectionView CharacterScreen {get;private set;}
        RectTransform characterCanvas;
        void CloseCharacterSelectionPage()
        {
            if(characterCanvas==null)return;
            var old=characterCanvas;characterCanvas=null;CharacterScreen=null;
            old.gameObject.SetActive(false);Destroy(old.gameObject);
        }
        public void OpenCharacterSelection()
        {
            if(!titleSession.SignedIn){ShowTitle();return;}
            // Leaving the title flies its camera into the rift above whatever opens next; entry itself is unchanged.
            DepartTitle();
            if(GuideTutorial.FirstStart(game.Store.Data)&&game.EnterFirstStart())return;
            titleSession.CancelEntry();characterSelection.Reset();ShowCharacterSelection();
        }
        public void ShowCharacterSelection()
        {
            if(!titleSession.SignedIn){ShowTitle();return;}
            game.Town?.Cancel();game.World.ClearDungeon();game.World.SuspendPresentation();
            pageRepaint=ShowCharacterSelection;Base("characters","HELLSCRIPT","캐릭터 선택하기",responsive:true);
            header.gameObject.SetActive(false);footer.gameObject.SetActive(false);
            headerApron.gameObject.SetActive(false);footerApron.gameObject.SetActive(false);content.parent.gameObject.SetActive(false);
            // This page uses its final pixel density rather than enlarging a small font atlas.
            // Keep the main page canvas and its notices at their existing scale.
            characterCanvas=ContentWindowView.CreateCanvas(transform,"Character selection display",109,CharacterSelectionView.PixelScale(UiSafeArea.Current),pixelPerfect:true);
            var page=Rect("Character selection",characterCanvas);Stretch(page);
            CharacterScreen=page.gameObject.AddComponent<CharacterSelectionView>();
            CharacterScreen.Initialize(game,characterSelection,titleSession);
            overlay.SetAsLastSibling();RefreshGlobalHud();
        }
    }
}
