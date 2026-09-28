using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Hellscript
{
    public sealed partial class GameUI
    {
        bool idleIntroductionOpen;
        RectTransform idleCanvas, idleSafe;
        Text idleSummary, idleAction;
        double nextIdleSummary;
        Vector2 idleScreen;
        Rect idleSafeArea;
        Vector2 idleIntroductionSize;

        public void ShowIdleIntroduction()
        {
            idleIntroductionOpen = true; pageRepaint = ShowIdleIntroduction;
            Base("idle-introduction", "절전 방치", "실제 사냥 유지", responsive: true);
            RepeatText("현재 균열 사냥을 계속하며 화면 표시를 줄입니다. 화면을 잠그거나 앱을 나가면 사냥을 보존합니다. 돌아온 뒤 재개를 눌러 계속할 수 있습니다.", pale);
            RepeatText("화면을 누르면 현재 전투를 잠깐 봅니다. 마지막 입력 후 10초가 지나면 다시 어두워집니다. 계속 보기를 누르면 일반 화면을 유지합니다.", pale);
            AddRepeatPreparation(game.Combat.RepeatPolicy);
            FooterButton(0, 2, "취소", () => { idleIntroductionOpen = false; ShowBattle(); });
            FooterButton(1, 2, "절전 방치 시작", () => { idleIntroductionOpen = false; ShowBattle(); game.EnterIdle(); }, true);
            idleIntroductionSize=Vector2.zero;ReflowIdleIntroduction();
        }
        void ReflowIdleIntroduction()
        {
            if(Page!="idle-introduction"||root==null)return;
            ApplySafeArea();var size=root.rect.size;if(size==idleIntroductionSize)return;
            Canvas.ForceUpdateCanvases();size=root.rect.size;idleIntroductionSize=size;
            header.sizeDelta=new Vector2(0,72);footer.sizeDelta=new Vector2(0,68);
            headerTitle.fontSize=size.x<600?22:24;Place(headerTitle.rectTransform,12,3,size.x-78,34);
            headerSubtitle.gameObject.SetActive(size.y>=400);Place(headerSubtitle.rectTransform,14,40,size.x-78,28);
            var settings=header.GetComponentsInChildren<Button>().Single(b=>b.name=="설정·안내");
            Right((RectTransform)settings.transform,12,8,48,48);settings.GetComponentInChildren<Text>().fontSize=18;
            foreach(var button in footer.GetComponentsInChildren<Button>())button.GetComponentInChildren<Text>().fontSize=size.x<600?17:21;
            var scroll=(RectTransform)root.Find("Scroll");scroll.offsetMin=new Vector2(16,80);scroll.offsetMax=new Vector2(-16,-84);
        }
        public void ShowIdleDimmed()
        {
            if (idleCanvas == null)
            {
                var obj = new GameObject("Idle display", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                obj.transform.SetParent(transform, false); idleCanvas = obj.GetComponent<RectTransform>();
                var canvas = obj.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 140;
                var backdrop = Box("Power saving backdrop", idleCanvas, Color.black); Stretch(backdrop);
                idleSafe = Rect("Idle safe area", backdrop); Stretch(idleSafe);
            }
            EventSystem.current?.SetSelectedGameObject(null);
            idleCanvas.gameObject.SetActive(true); root.parent.gameObject.SetActive(false);
            RefreshGlobalHud(); idleScreen = Vector2.zero; RefreshIdleSummary(true);
        }
        public void PrepareIdlePeek() { root.parent.gameObject.SetActive(true); ShowBattle(); }
        public void HideIdleOverlay()
        {
            if (idleCanvas != null) idleCanvas.gameObject.SetActive(false);
            if (root != null) root.parent.gameObject.SetActive(true);
            RefreshGlobalHud();
        }
        public void RefreshIdleSummary(bool force = false)
        {
            if (!game.DisplayDimmed || idleCanvas == null || game.Combat == null) return;
            double now = Time.realtimeSinceStartupAsDouble;
            var screen = new Vector2(Screen.width, Screen.height); Rect safe = UiSafeArea.Current;
            bool layout = screen != idleScreen || safe != idleSafeArea || idleLanguage != Loc.Language || idleReading != InterfaceFactor;
            if (layout)
            {
                idleScreen = screen; idleSafeArea = safe; idleLanguage = Loc.Language; idleReading = InterfaceFactor;
                float scale = UiTheme.Scale(safe);
                var scaler = idleCanvas.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize; scaler.scaleFactor = scale;
                idleSafe.anchorMin = idleSafe.anchorMax = idleSafe.pivot = Vector2.zero;
                idleSafe.anchoredPosition = safe.position / scale; idleSafe.sizeDelta = safe.size / scale;
                BuildIdleDashboard(); game.WakeIdleInteraction();
            }
            TickIdleUnlock();
            if (!force && !layout && now < nextIdleSummary) return;
            nextIdleSummary = now + 1;
            idleSummary.text = IdleHuntJournal.Duration(game.IdleJournal.Elapsed(now));
            var space = IdleHuntJournal.Warehouse(game.Store.Data);
            idleStorage.text = Loc.F("남은 칸 {0} / {1}", space.free, space.capacity);
            idleAction.text = game.CommonIdleStatus();
            RefreshIdleGrowth();
            for (int i = 0; i < idleLoot.Length; i++) idleLoot[i].text = "× " + game.IdleJournal.Acquired[i].ToString("N0");
            idleGear.Refresh(game.IdleJournal.Revision); idleAttempts.Refresh(game.IdleJournal.Revision);
        }
    }
}
