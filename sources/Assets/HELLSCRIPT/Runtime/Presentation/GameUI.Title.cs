using UnityEngine;

namespace Hellscript
{
    public sealed partial class GameUI
    {
        readonly TitleSession titleSession=new TitleSession();
        public TitleScreenView TitleScreen {get;private set;}
        TitleVista titleVista;GameObject titleFlight;
        // The login flight still playing above the page that replaced the title, if any.
        public TitleVista TitleFlight=>titleFlight!=null?titleFlight.GetComponentInChildren<TitleVista>():null;
        public void ShowTitle()
        {
            // Signing out of the tutorial leaves from the hub; the hub is a window above the page and would cover the title.
            ClosePuzzleHub();
            if(titleFlight!=null){Destroy(titleFlight);titleFlight=null;}
            game.Town?.Cancel();game.World.ClearDungeon();game.World.SuspendPresentation();pageRepaint=ShowTitle;
            Base("title","HELLSCRIPT","잿빛 숲 너머, 당신의 이야기가 시작됩니다",responsive:true);
            header.gameObject.SetActive(false);footer.gameObject.SetActive(false);
            headerApron.gameObject.SetActive(false);footerApron.gameObject.SetActive(false);
            content.parent.gameObject.SetActive(false);
            titleSession.CancelEntry();
            // The vista animates on its own canvas so the controls are not rebatched; pixel snapping would step the slow drift.
            var backdrop=Rect("Title vista",shell);Stretch(backdrop);var layer=backdrop.gameObject.AddComponent<Canvas>();layer.overridePixelPerfect=true;layer.pixelPerfect=false;
            titleVista=backdrop.gameObject.AddComponent<TitleVista>();titleVista.Initialize(titleSession);
            var page=Rect("Title screen",root);Stretch(page);
            TitleScreen=page.gameObject.AddComponent<TitleScreenView>();
            TitleScreen.Initialize(game,font,titleSession,titleVista,ShowScreenSettings);titleVista.KeepClear(TitleScreen.Home,TitleScreen.Identity);
            overlay.SetAsLastSibling();RefreshGlobalHud();
        }
        // Leaving the title flies its camera into the rift on a canvas above every page and window, below notices.
        void DepartTitle()
        {
            if(Page!="title"||!titleSession.MotionEnabled||titleVista==null||!titleVista.Ready)return;
            titleFlight=new GameObject("Title flight",typeof(RectTransform),typeof(Canvas));titleFlight.transform.SetParent(transform,false);
            var canvas=titleFlight.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=2500;
            game.Audio?.Play("flow.title_flight");titleVista.Impact=()=>game.Audio?.Play("flow.title_rift");
            titleVista.Depart(titleFlight.transform);
        }
    }
}
