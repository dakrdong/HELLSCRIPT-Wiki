using UnityEngine;

namespace Hellscript
{
    public sealed partial class GameUI
    {
        readonly TitleSession titleSession=new TitleSession();
        public TitleScreenView TitleScreen {get;private set;}
        public void ShowTitle()
        {
            game.Town?.Cancel();game.World.ClearDungeon();pageRepaint=ShowTitle;
            Base("title","HELLSCRIPT","잿빛 숲 너머, 당신의 이야기가 시작됩니다",responsive:true);
            header.gameObject.SetActive(false);footer.gameObject.SetActive(false);
            headerApron.gameObject.SetActive(false);footerApron.gameObject.SetActive(false);
            content.parent.gameObject.SetActive(false);
            titleSession.CancelEntry();
            var backdrop=Rect("Living sanctuary",shell);Stretch(backdrop);
            var atmosphere=backdrop.gameObject.AddComponent<TitleAtmosphere>();atmosphere.Initialize(titleSession);
            var page=Rect("Title screen",root);Stretch(page);
            TitleScreen=page.gameObject.AddComponent<TitleScreenView>();
            TitleScreen.Initialize(game,font,titleSession,atmosphere,ShowScreenSettings);
            overlay.SetAsLastSibling();RefreshGlobalHud();
        }
    }
}
