using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Hellscript
{
    public sealed partial class GameUI
    {
        RectTransform commonModal, commonSafe, commonCard, commonTabs, commonActions, screenPane, helpPane;
        RectTransform combatPane;
        Text screenActual, screenSelection, screenMessage;
        Button screenApply;
        GameObject commonPreviousSelection;
        Vector2 commonScreenSize;
        Rect commonSafeArea;
        float commonKeyboardTop;
        bool commonHelp, commonLaidOut;
        bool commonCombat;
        readonly System.Collections.Generic.List<(CanvasGroup group, bool interactable, bool raycasts)> commonInputGates = new System.Collections.Generic.List<(CanvasGroup, bool, bool)>();
        readonly System.Collections.Generic.List<(int percent, Button button)> scaleButtons = new System.Collections.Generic.List<(int, Button)>();
        Text scaleMessage;
        RectTransform scaleChoiceRow;
        readonly System.Collections.Generic.List<(string code, Button button)> languageButtons = new System.Collections.Generic.List<(string, Button)>();
        Text languageMessage;
        RectTransform languageChoiceRow;
        DeviceScreenDirection screenDraft;
        public bool CommonPanelOpen => commonModal != null || idleIntroductionOpen;
        public bool BlocksRepeat => runeSession || CommonPanelOpen || presetModal != null || root != null && root.Find("Confirm") != null;

        public void ShowScreenSettings() => ShowCommonPanel(false);
        public void ShowCombatOverview() { ShowCommonPanel(false); SelectCombatTab(); }
        void ShowHelp() => ShowCommonPanel(true);
        void ShowCommonPanel(bool help)
        {
            game.ExitIdle();
            if (commonModal != null) { SelectCommonTab(help); return; }
            commonPreviousSelection = EventSystem.current?.currentSelectedGameObject;
            EventSystem.current?.SetSelectedGameObject(null);
            commonInputGates.Clear();
            foreach (var existing in GetComponentsInChildren<Canvas>())
            {
                var group = existing.GetComponent<CanvasGroup>() ?? existing.gameObject.AddComponent<CanvasGroup>();
                commonInputGates.Add((group, group.interactable, group.blocksRaycasts)); group.interactable = group.blocksRaycasts = false;
            }
            screenDraft = game.DisplaySettings.Actual;
            commonModal = Rect("Screen and help modal", transform);
            var canvas = commonModal.gameObject.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 120; canvas.pixelPerfect = true;
            ApplyScaler(commonModal.gameObject.AddComponent<CanvasScaler>(), true);
            commonModal.gameObject.AddComponent<GraphicRaycaster>(); commonModal.gameObject.AddComponent<Image>().color = new Color(0, 0, 0, .9f);
            commonSafe = Rect("Common safe area", commonModal); Stretch(commonSafe);
            commonCard = Box("Common dialog", commonSafe, ink); commonCard.anchorMin = commonCard.anchorMax = commonCard.pivot = new Vector2(.5f, .5f);
            var heading = Label(commonCard, "설정 · 게임 안내", 26, gold); Place(heading.rectTransform, 18, 8, 300, 42);
            heading.rectTransform.anchorMax = Vector2.one; heading.rectTransform.sizeDelta = new Vector2(-36, 42);
            commonTabs = Rect("Common categories", commonCard);
            var settings = Button(commonTabs, "화면", () => SelectCommonTab(false));
            var guide = Button(commonTabs, "게임 안내", () => SelectCommonTab(true));
            AnchorButton(settings, 0, 2); AnchorButton(guide, 1, 2);
            screenPane = CommonScroll("화면 설정 본문", out var screenBody);
            screenActual = CommonNote(screenBody, "", 24, gold);
            CommonNote(screenBody, "언어", 24, gold);
            CommonNote(screenBody, "누르면 곧바로 적용하고 이 기기에 저장합니다. 보고 있던 화면은 선택한 언어로 다시 그리며, 번역이 없는 줄은 원문을 그대로 보여 줍니다.");
            var languageChoices = Rect("Language choices", screenBody); languageChoices.gameObject.AddComponent<LayoutElement>().preferredHeight = 58; languageChoiceRow = languageChoices;
            languageButtons.Clear();
            for (int i = 0; i < LanguageOptions.All.Length; i++)
            {
                var option = LanguageOptions.All[i];
                var choice = Button(languageChoices, option.NativeName, () => game.ApplyLanguage(option.Code));
                AnchorButton(choice, i, LanguageOptions.All.Length); languageButtons.Add((option.Code, choice));
            }
            languageMessage = CommonNote(screenBody, "", 20, gold);
            CommonNote(screenBody, "언어는 이 기기에만 저장합니다. 캐릭터와 장비, 사냥 칙령과 프리셋은 변경하지 않습니다. 이름을 입력하던 창은 화면을 다시 그리면서 닫힙니다.");
            if (!string.IsNullOrEmpty(game.LanguageLoadNotice)) CommonNote(screenBody, game.LanguageLoadNotice, 20, gold);
            if (game.DisplaySettings.Mobile)
            {
                CommonNote(screenBody, "선택한 방향에 맞춰 화면 구성이 바뀝니다. 적용을 눌러야 변경되며, 닫기만 하면 현재 방향을 유지합니다.");
                var choices = Rect("Direction choices", screenBody); choices.gameObject.AddComponent<LayoutElement>().preferredHeight = 58;
                AnchorButton(Button(choices, "세로", () => { screenDraft = DeviceScreenDirection.Portrait; RefreshScreenSettings(); }), 0, 2);
                AnchorButton(Button(choices, "가로", () => { screenDraft = DeviceScreenDirection.Landscape; RefreshScreenSettings(); }), 1, 2);
                screenSelection = CommonNote(screenBody, "", 21, gold);
                CommonNote(screenBody, "화면 방향은 이 기기에만 저장합니다. 캐릭터·장비·사냥 칙령과 프리셋은 변경하지 않습니다.");
            }
            else
            {
                CommonNote(screenBody, "창 크기에 따라 화면 설정과 안내의 분류·본문을 나란히 배치하거나 한 열로 접습니다. 창을 좁혀도 같은 내용을 읽을 수 있습니다.");
                CommonNote(screenBody, "PC에서는 창이나 모니터의 방향을 강제로 바꾸지 않습니다. 창 크기 변경 자체는 전투의 일시정지·재개 명령이 아닙니다.");
            }
            CommonNote(screenBody, "글자 크기", 24, gold);
            CommonNote(screenBody, "누르면 곧바로 적용하고 이 기기에 저장합니다. 글자만 키우면 정해진 칸을 넘치기 때문에 화면 구성 전체가 같은 비율로 커집니다. 한 화면에 보이는 항목은 줄어들고 나머지는 스크롤로 이어서 읽습니다.");
            var scaleChoices = Rect("Text size choices", screenBody); scaleChoices.gameObject.AddComponent<LayoutElement>().preferredHeight = 58; scaleChoiceRow = scaleChoices;
            scaleButtons.Clear();
            for (int i = 0; i < InterfaceScaleOptions.Steps.Length; i++)
            {
                int percent = InterfaceScaleOptions.Steps[i];
                var choice = Button(scaleChoices, InterfaceScaleOptions.Label(percent), () => { game.ApplyInterfaceScale(percent); RefreshScreenSettings(); DialogReadingAnchor.Show(scaleChoiceRow); });
                AnchorButton(choice, i, InterfaceScaleOptions.Steps.Length); scaleButtons.Add((percent, choice));
            }
            scaleMessage = CommonNote(screenBody, "", 20, gold);
            CommonNote(screenBody, "글자 크기는 이 기기에만 저장합니다. 캐릭터와 장비, 사냥 칙령과 프리셋은 변경하지 않습니다.");
            if (!string.IsNullOrEmpty(game.InterfaceScaleLoadNotice)) CommonNote(screenBody, game.InterfaceScaleLoadNotice, 20, gold);
            CommonNote(screenBody, "설정과 안내를 열어 둔 동안 전투·훈련과 다음 균열 대기가 멈춥니다. 닫으면 이전 상태에서 이어집니다. 이미 일시정지한 전투는 그대로 정지해 있습니다.");
            screenMessage = CommonNote(screenBody, "", 20, gold);
            if (!string.IsNullOrEmpty(game.DisplayLoadNotice)) CommonNote(screenBody, game.DisplayLoadNotice, 20, gold);
            helpPane = CommonScroll("게임 안내 본문", out var helpBody);
            CommonNote(helpBody, "명령을 설계하는 자동 전투 파밍 게임", 24, gold);
            CommonNote(helpBody, "1. 성소에서 직업과 행동 설정을 고릅니다. 직업별 추천 빌드의 스킬 조합과 행동 조건을 확인하세요.");
            CommonNote(helpBody, "2. 균열에 진입하면 탐색·전투·획득이 자동으로 진행됩니다. 처치 게이지 100을 채우면 보스가 등장합니다.");
            CommonNote(helpBody, "3. 실패해도 이미 얻은 장비·경험치·재화는 유지됩니다. 개봉하지 않은 상자의 보상은 지급하지 않습니다.");
            CommonNote(helpBody, "4. 행동 설계에서는 실행 조건을 수정할 수 있습니다. 편집 중에는 전투가 멈추며, 설정 적용을 눌러야 변경한 내용이 반영됩니다.");
            CommonNote(helpBody, "5. 현재는 1배속을 사용하며 1.5배속과 2배속은 잠겨 있습니다. 일시정지하면 전투 시간이 흐르지 않습니다.");
            CommonNote(helpBody, "6. 훈련에서는 현재 레벨과 소유 장비를 바탕으로 설정을 시험합니다. 훈련 결과에는 실제 계정 보상을 지급하지 않습니다.");
            CommonNote(helpBody, "첫 플레이 안내는 성소와 균열 결과의 ‘첫 플레이 안내’에서 다시 열 수 있습니다. 진행 단계와 다음에 할 일을 확인하세요.");
            CommonNote(helpBody, "현재 빌드는 로컬 개발용입니다. 실제 서버·계정·결제는 연결되지 않았으며, 보석과 일부 특수 효과는 추가 구현 대상입니다.", 20, gold);
            combatPane = null;
            if (game.Combat != null)
            {
                Button(commonTabs, "전투 상태", SelectCombatTab);
                combatPane = CommonScroll("전투 상태 본문", out var combatBody);
                var run = game.Combat.State;
                CommonNote(combatBody, run.training >= 0 ? "훈련 상태" : Loc.F("균열 {0}단계", run.stage), 24, gold);
                CommonNote(combatBody, Loc.F("HP {0:0} / {1:0} · 보호막 {2:0}\n자원 {3:0} / {4:0} · {5:0.#}배\n{6}", Mathf.Max(0,run.health), game.Combat.Stats.hp, run.shield, run.resource, game.Combat.Stats.maxResource, game.EffectiveSpeed, (run.paused ? "진입 전 일시정지 상태를 유지합니다." : "이 창을 닫으면 전투를 이어 갑니다.")));
                CommonNote(combatBody, Loc.F("현재 행동\n{0}", game.Combat.CurrentActionText), 22, gold);
                CommonNote(combatBody, "배속: 1배속을 사용합니다. 1.5배속과 2배속은 현재 잠겨 있습니다.");
                if (toastTime > 0 && toast != null && !string.IsNullOrEmpty(toast.text)) CommonNote(combatBody, Loc.F("알림\n{0}", toast.text), 20, gold);
                CommonNote(combatBody, run.training >= 0 ? Loc.F("표적 {0} / {1} · 실제 보상 없음", run.kills, run.enemies.Count) : Loc.F("처치 {0} · 균열 게이지 {1} / 100", run.kills, Mathf.Min(100,run.meter)));
                foreach (int id in run.build.activeSkills)
                {
                    var skill = game.catalog.skills[id];
                    CommonNote(combatBody, Loc.F("{0}\n{1}", skill.name, game.Combat.EffectiveLevel < skill.unlock ? Loc.F("Lv.{0}에 해금됩니다.", skill.unlock) : run.heroAction.skill == id && run.heroAction.phase != HeroActionPhase.Idle ? "현재 동작을 수행하고 있습니다." : run.cooldowns[id] > 0 ? Loc.F("재사용까지 {0:0.0}초", run.cooldowns[id]) : "사용 가능합니다."));
                }
                CommonNote(combatBody, Loc.F("개봉한 상자 {0} / {1}\n길잡이의 성소 {2:0.0}초 · 결의의 성소 {3:0.0}초", run.layout.chests.Count(c=>c.phase==ChestPhase.Opened), run.layout.chests.Count, run.guideShrineTime, run.resolveShrineTime));
                foreach (var evt in run.layout.events.Where(e=>e.phase==RiftEventPhase.Active)) CommonNote(combatBody, Loc.F("균열 잔향 {0}/4 · {1:0.0}초 남음", evt.enemyIds.Count(id=>run.enemies.Any(e=>e.id==id&&e.dead)), evt.remaining));
                if (!string.IsNullOrEmpty(run.navigationError)) CommonNote(combatBody, run.navigationError, 20, gold);
            }
            commonActions = Rect("Common actions", commonCard);
            var close = Button(commonActions, "닫기", CloseCommonPanel);
            screenApply = Button(commonActions, "적용", () =>
            {
                var display = game.DisplaySettings;
                if (display.CanRetrySave && screenDraft == display.Actual) display.RetrySave();
                else game.ApplyDisplayDirection(screenDraft);
                RefreshScreenSettings();
            }, gold * .55f);
            AnchorButton(close, 0, game.DisplaySettings.Mobile ? 2 : 1); AnchorButton(screenApply, 1, 2);
            commonLaidOut = false; SelectCommonTab(help); ReflowCommonPanel();
        }
        RectTransform CommonScroll(string name, out RectTransform body)
        {
            var frame = Box(name, commonCard, panel); var scroll = frame.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false; scroll.movementType = ScrollRect.MovementType.Clamped;
            var viewport = Rect("Viewport", frame); Stretch(viewport); viewport.gameObject.AddComponent<RectMask2D>(); scroll.viewport = viewport;
            body = Rect("Content", viewport); body.anchorMin = new Vector2(0, 1); body.anchorMax = Vector2.one; body.pivot = new Vector2(.5f, 1); body.sizeDelta = Vector2.zero;
            var layout = body.gameObject.AddComponent<VerticalLayoutGroup>(); layout.childControlWidth = layout.childControlHeight = layout.childForceExpandWidth = true; layout.childForceExpandHeight = false;
            layout.spacing = 18; layout.padding = new RectOffset(18, 18, 18, 18);
            body.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize; scroll.content = body; return frame;
        }
        Text CommonNote(Transform parent, string text, int size = 22, Color? color = null)
        { var label = Label(parent, text, size, color ?? pale); label.alignment = TextAnchor.UpperLeft; label.gameObject.AddComponent<LayoutElement>().minHeight = 28; return label; }
        void SelectCommonTab(bool help)
        {
            commonCombat = false; if (combatPane != null) combatPane.gameObject.SetActive(false);
            commonHelp = help; helpPane.gameObject.SetActive(help); screenPane.gameObject.SetActive(!help);
            foreach (Button button in commonTabs.GetComponentsInChildren<Button>()) button.GetComponent<Image>().color = button.name == (help ? "게임 안내" : "화면") ? gold * .4f : panel;
            screenApply.gameObject.SetActive(!help && game.DisplaySettings.Mobile);
            var close = commonActions.GetChild(0).GetComponent<Button>(); AnchorButton(close, 0, !help && game.DisplaySettings.Mobile ? 2 : 1);
            RefreshScreenSettings();
        }
        void SelectCombatTab()
        {
            if (combatPane == null) return;
            commonCombat = true; commonHelp = false; screenPane.gameObject.SetActive(false); helpPane.gameObject.SetActive(false); combatPane.gameObject.SetActive(true);
            screenApply.gameObject.SetActive(false); AnchorButton(commonActions.GetChild(0).GetComponent<Button>(), 0, 1);
            foreach (var button in commonTabs.GetComponentsInChildren<Button>()) button.GetComponent<Image>().color = button.name == "전투 상태" ? gold * .4f : panel;
        }
        void RefreshScreenSettings()
        {
            if (commonModal == null) return;
            var display = game.DisplaySettings; string Direction(DeviceScreenDirection value) => value == DeviceScreenDirection.Portrait ? "세로" : "가로";
            screenActual.text = Loc.T(display.Mobile ? Loc.F("현재 적용 · {0}", Direction(display.Actual)) : "가로 구성 · 창 크기에 맞춤");
            if (screenSelection != null) screenSelection.text = Loc.F("선택 · {0}{1}", Direction(screenDraft), (display.Pending != 0 ? "\n적용 결과를 확인하고 있습니다." : ""));
            screenMessage.text = Loc.T(display.Mobile ? display.Message : "이 화면을 닫으면 이전 화면으로 돌아갑니다.");
            screenApply.GetComponentInChildren<Text>().text = Loc.T(display.CanRetrySave && screenDraft == display.Actual ? "저장 재시도" : "적용");
            screenApply.interactable = display.Pending == 0;
            var language = game.Language;
            if (language != null && languageMessage != null)
            {
                foreach (var choice in languageButtons) choice.button.GetComponent<Image>().color = choice.code == language.Language ? gold * .4f : new Color(.14f, .17f, .2f);
                languageMessage.text = Loc.T(string.IsNullOrEmpty(language.Message)
                    ? Loc.F("현재 언어는 {0}입니다.", LanguageOptions.Find(language.Language).NativeName)
                    : language.Message + (language.CanRetrySave ? Loc.T("\n같은 언어를 다시 누르면 저장을 한 번 더 시도합니다.") : ""));
            }
            var scale = game.InterfaceScale;
            if (scale == null || scaleMessage == null) return;
            foreach (var choice in scaleButtons) choice.button.GetComponent<Image>().color = choice.percent == scale.Percent ? gold * .4f : new Color(.14f, .17f, .2f);
            scaleMessage.text = Loc.T(string.IsNullOrEmpty(scale.Message)
                ? Loc.F("현재 글자 크기는 {0}입니다.", InterfaceScaleOptions.Label(scale.Percent))
                : scale.Message + (scale.CanRetrySave ? Loc.T("\n같은 크기를 다시 누르면 저장을 한 번 더 시도합니다.") : ""));
        }
        void ReflowCommonPanel()
        {
            if (commonModal == null) return;
            var size = new Vector2(Screen.width, Screen.height); var safe = UiSafeArea.Current;
            var keyboard = TouchScreenKeyboard.visible ? TouchScreenKeyboard.area : new Rect();
            float bottom = keyboard.width > 0 && keyboard.xMin < safe.xMax && keyboard.xMax > safe.xMin ? Mathf.Clamp(keyboard.yMax, safe.yMin, safe.yMax) : safe.yMin;
            if (commonLaidOut && size == commonScreenSize && safe == commonSafeArea && Mathf.Abs(bottom - commonKeyboardTop) < 1) return;
            Canvas.ForceUpdateCanvases(); var scroll = (commonCombat ? combatPane : commonHelp ? helpPane : screenPane).GetComponent<ScrollRect>(); var reading = DialogReadingAnchor.Capture(scroll);
            commonScreenSize = size; commonSafeArea = safe; commonKeyboardTop = bottom;
            commonSafe.anchorMin = new Vector2(safe.xMin / Mathf.Max(1, size.x), bottom / Mathf.Max(1, size.y)); commonSafe.anchorMax = new Vector2(safe.xMax / Mathf.Max(1, size.x), safe.yMax / Mathf.Max(1, size.y)); commonSafe.offsetMin = commonSafe.offsetMax = Vector2.zero;
            Canvas.ForceUpdateCanvases(); var available = commonSafe.rect.size;
            float w = Mathf.Min(1100, Mathf.Max(1, available.x - 24)), h = Mathf.Min(800, Mathf.Max(1, available.y - 24));
            commonCard.sizeDelta = new Vector2(w, h); commonCard.anchoredPosition = Vector2.zero;
            bool wide = w >= 820; float top = wide ? 62 : 130, body = Mathf.Max(1, h - top - 78);
            Place(commonTabs, 12, 62, wide ? 200 : w - 24, wide ? 180 : 60);
            var tabs = commonTabs.GetComponentsInChildren<Button>();
            for (int i = 0; i < tabs.Length; i++)
            {
                var rect = (RectTransform)tabs[i].transform;
                if (wide) Place(rect, 0, i * 60, 200, 52); else AnchorButton(tabs[i], i, tabs.Length);
            }
            foreach (var pane in new[] { screenPane, helpPane }) Place(pane, wide ? 224 : 12, top, w - (wide ? 236 : 24), body);
            if (combatPane != null) Place(combatPane, wide ? 224 : 12, top, w - (wide ? 236 : 24), body);
            Place(commonActions, 12, h - 72, w - 24, 60);
            Canvas.ForceUpdateCanvases(); if (commonLaidOut) reading?.Restore(); commonLaidOut = true;
        }
        public void CloseCommonPanel()
        {
            if (commonModal == null) return;
            var old = commonModal; commonModal = null; old.gameObject.SetActive(false); Destroy(old.gameObject);
            foreach (var gate in commonInputGates) if (gate.group != null) { gate.group.interactable = gate.interactable; gate.group.blocksRaycasts = gate.raycasts; }
            commonInputGates.Clear();
            if (commonPreviousSelection != null && commonPreviousSelection.activeInHierarchy) EventSystem.current?.SetSelectedGameObject(commonPreviousSelection);
            commonPreviousSelection = null;
        }
        void UpdateCommonPanel()
        {
            if (root != null) ApplySafeArea();
            if (commonModal == null) return;
            ReflowCommonPanel(); RefreshScreenSettings();
            if (Keyboard.current?.escapeKey.wasPressedThisFrame == true) CloseCommonPanel();
        }
    }
}
