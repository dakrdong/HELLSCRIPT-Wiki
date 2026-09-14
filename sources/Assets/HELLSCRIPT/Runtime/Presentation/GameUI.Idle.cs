using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Hellscript
{
    public sealed partial class GameUI
    {
        bool idleIntroductionOpen;
        RectTransform idleCanvas, idleSafe, idleCard;
        Text idleSummary, idleAction, idleDetail, idleHint;
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
                ApplyScaler(obj.GetComponent<CanvasScaler>(), true);
                var backdrop = Box("현재 전투 잠깐 보기", idleCanvas, Color.black); Stretch(backdrop);
                var tap = backdrop.gameObject.AddComponent<Button>(); tap.transition = Selectable.Transition.None;
                tap.onClick.AddListener(game.RequestIdlePeek);
                idleSafe = Rect("Idle safe area", backdrop); Stretch(idleSafe);
                idleCard = Rect("Idle summary", idleSafe);
                idleSummary = Label(idleCard, "", 22, new Color(.44f,.44f,.44f), TextAnchor.MiddleCenter);
                idleAction = Label(idleCard, "", 20, new Color(.38f,.38f,.38f), TextAnchor.MiddleCenter);
                idleDetail = Label(idleCard, "", 18, new Color(.36f,.36f,.36f), TextAnchor.MiddleCenter);
                idleHint = Label(idleCard, "화면을 누르면 잠깐 봅니다.", 18, new Color(.3f,.3f,.3f), TextAnchor.MiddleCenter);
            }
            EventSystem.current?.SetSelectedGameObject(null);
            idleCanvas.gameObject.SetActive(true); root.parent.gameObject.SetActive(false);
            idleScreen = Vector2.zero; RefreshIdleSummary(true);
        }
        public void PrepareIdlePeek()
        { root.parent.gameObject.SetActive(true); ShowBattle(); }
        public void HideIdleOverlay()
        {
            if (idleCanvas != null) idleCanvas.gameObject.SetActive(false);
            if (root != null) root.parent.gameObject.SetActive(true);
        }
        void FitIdle(Text label, string text)
        {
            label.text = text;
            while (label.preferredHeight > label.rectTransform.rect.height + 1 && label.text.Length > 4)
                label.text = label.text.Substring(0, label.text.Length - (label.text.EndsWith("…") ? 2 : 1)) + "…";
        }
        public void RefreshIdleSummary(bool force = false)
        {
            if (!game.DisplayDimmed || idleCanvas == null || game.Combat == null) return;
            double now = Time.realtimeSinceStartupAsDouble;
            var screen = new Vector2(Screen.width, Screen.height); Rect safe = UiSafeArea.Current;
            if (!force && now < nextIdleSummary && screen == idleScreen && safe == idleSafeArea) return;
            nextIdleSummary = now + 1;
            if (screen != idleScreen || safe != idleSafeArea)
            {
                idleScreen = screen; idleSafeArea = safe; ApplyScaler(idleCanvas.GetComponent<CanvasScaler>(), true);
                idleSafe.anchorMin = new Vector2(safe.x / Screen.width, safe.y / Screen.height);
                idleSafe.anchorMax = new Vector2(safe.xMax / Screen.width, safe.yMax / Screen.height); idleSafe.offsetMin = idleSafe.offsetMax = Vector2.zero;
                Canvas.ForceUpdateCanvases();
            }
            float width = Mathf.Min(480, idleSafe.rect.width - 32), height = Mathf.Min(260, idleSafe.rect.height - 24);
            idleCard.anchorMin = idleCard.anchorMax = new Vector2(.5f,.5f); idleCard.pivot = new Vector2(.5f,.5f);
            idleCard.sizeDelta = new Vector2(width, height);
            int shift = (int)(now / 60) % 4;
            idleCard.anchoredPosition = new Vector2(shift < 2 ? -6 : 6, shift % 2 == 0 ? -6 : 6);
            Place(idleSummary.rectTransform, 0, 0, width, height * .23f);
            Place(idleAction.rectTransform, 0, height * .23f, width, height * .22f);
            Place(idleDetail.rectTransform, 0, height * .45f, width, height * .36f);
            Place(idleHint.rectTransform, 0, height * .83f, width, height * .17f);
            var run = game.Combat.State;
            FitIdle(idleSummary, Loc.F("균열 {0}단계 · {1}회 완료", run.stage, game.IdleCompleted));
            string action = !string.IsNullOrEmpty(game.ForegroundPauseReason) ? Loc.T(game.ForegroundPauseReason) :
                run.portal ? Loc.T("가방 정리가 필요해 사냥이 멈췄습니다.") :
                run.paused ? Loc.T("일시정지") : !string.IsNullOrEmpty(run.navigationError) ? Loc.T("이동 경로를 확인할 수 없어 사냥이 멈췄습니다.") :
                !game.Active ? Loc.T("이번 균열을 마쳤습니다.") : Loc.StoredText(run.action);
            FitIdle(idleAction, action);
            var session = game.RepeatSession;
            string detail;
            if (!game.Active || session != null && session.blocked != RepeatBlock.None) detail = RepeatStatusText();
            else if (session.cancelled) detail = Loc.T("이번 균열을 마친 뒤 반복을 중단합니다.");
            else
            {
                var item = game.Store.Data.Hero.inventory.Where(i => i.rarity >= 3 && i.acquiredOrder > 0).OrderByDescending(i => i.acquiredOrder).FirstOrDefault();
                detail = item != null ? Loc.F("최근 중요 획득 · {0}", Loc.StoredText(item.name)) :
                    Loc.T(session.policy.enabled ? "반복 설정에 따라 사냥을 계속합니다." : "이번 균열까지만 진행합니다.");
            }
            FitIdle(idleDetail, detail);
        }
    }
}
