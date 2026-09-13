using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    public sealed partial class GameUI
    {
        RectTransform battleStatus, battleActions, battleSkills, battleWorld, battleMatte;
        readonly List<RectTransform> battleIcons = new List<RectTransform>();
        readonly List<Button> battleSpeeds = new List<Button>();
        RectTransform[] battleCurtains;
        Button battleEffects, battleReason, battleGrowth, battleIdle;
        BattleHudLayout battleLayout;
        Vector2 battleSize;
        Rect battleSafe;
        bool battleBoss;
        string fullBattleAction = "";
        public Rect BattleViewport { get; private set; } = new Rect(0, 0, 1, 1);
        public float BattleViewHeight => Page == "battle" && battleWorld != null ? battleWorld.rect.height : Screen.height;

        void BuildBattleHud(RunState run)
        {
            battleIcons.Clear(); battleSpeeds.Clear(); battleSize = Vector2.zero;
            battleMatte = Rect("Combat viewport borders", root.parent); Stretch(battleMatte); battleMatte.SetAsFirstSibling();
            battleCurtains = Enumerable.Range(0, 4).Select(i => Box("Border " + i, battleMatte, new Color(.02f, .03f, .045f, 1))).ToArray();
            foreach (var border in battleCurtains) border.GetComponent<Image>().raycastTarget = false;
            battleWorld = Rect("Combat viewport", root);
            battleStatus = Box("Combat HUD", root, panel);
            hpFill = Bar(battleStatus, Vector2.zero, Vector2.one, new Color(.7f, .16f, .16f));
            hpText = Label(battleStatus, "", 18, pale, TextAnchor.MiddleCenter);
            battleEffects = Button(battleStatus, "보호막·피해 상세", ShowCombatEffects, Color.clear); battleEffects.GetComponentInChildren<Text>().text = "";
            resourceFill = Bar(battleStatus, Vector2.zero, Vector2.one, new Color(.15f, .45f, .7f)); resourceText = Label(battleStatus, "", 17, pale, TextAnchor.MiddleCenter);
            meterFill = Bar(battleStatus, Vector2.zero, Vector2.one, gold); meterText = Label(battleStatus, "", 18, gold);
            timerText = Label(battleStatus, "", 27, gold, TextAnchor.MiddleCenter);
            foreach (float speed in new[] { 1f, 1.5f, 2f })
            {
                bool available = CombatSpeedAccess.CanSelect(speed);
                var button = Button(battleStatus, Loc.F("{0}배속{1}", speed.ToString("0.#"), (available ? "" : " 잠김")), () => game.SetSpeed(speed), available ? new Color(.36f,.25f,.12f) : new Color(.12f,.15f,.19f));
                var label = button.GetComponentInChildren<Text>();
                label.text = Loc.F("{0}×{1}", speed.ToString("0.#"), (available ? "" : "\n잠김"));
                label.color = available ? gold : pale;
                Inset(label.rectTransform, 2, 2, 0, 0);
                battleSpeeds.Add(button);
            }
            AddBossHud(battleStatus);
            battleActions = Box("Actions", root, panel);
            battleReason = Button(battleActions, "현재 행동", ShowRuleDecisions, Color.clear); battleReason.GetComponentInChildren<Text>().text = "";
            actionText = Label(battleReason.transform, "", 18, gold); Inset(actionText.rectTransform, 8, 8, 4, 4);
            battleSkills = Rect("Equipped skills", battleActions);
            foreach (int index in run.build.activeSkills.Take(4))
            {
                var skill = Rect("Battle skill " + index, battleSkills); Icon(skill, index, 0, 0, 48); battleIcons.Add(skill.GetChild(0) as RectTransform);
                var label = Label(skill, "", 17, pale); label.gameObject.name = "Skill " + index; skillLabels.Add(label);
            }
            if(game.DisplayMode==IdleDisplayMode.Peek)
            {
                FooterButton(0,3,"계속 보기",game.KeepWatching);
                FooterButton(1,3,"절전으로 돌아가기",game.DimIdle);
                FooterButton(2,3,"다음 판부터 중단",game.StopAutoRepeat);
            }
            else
            {
            FooterButton(0, 5, "일시정지", () => game.TogglePause());
            FooterButton(1, 5, game.ComparisonRun ? "비교 조건" : "행동 수정", () => game.EditBuild());
            FooterButton(2, 5, "지도", ShowRiftMap);
            FooterButton(3, 5, "전투 상태", ShowCombatOverview);
            FooterButton(4, 5, "귀환", () => Confirm(game.ComparisonRun ? "진행 중인 비교를 끝내고 성소로 돌아갑니다. 저장하지 않은 비교 결과는 남지 않습니다." : "현재까지 얻은 전리품을 보존하고 마을로 돌아갑니다.", () => game.ReturnTown()));
            }
            battleIdle=run.training<0&&game.DisplayMode==IdleDisplayMode.Normal?Button(root,"절전 방치",game.RequestIdle,panel):null;
            AddRiftMinimap(run);
            battleGrowth = Button(header, "성장과 스킬", ShowGrowth, Color.clear); battleGrowth.GetComponentInChildren<Text>().text = "";
            RefreshHud(); ReflowBattleHud();
        }
        void PlaceBattle(RectTransform target, Rect rect) => Place(target, rect.x, rect.y, rect.width, rect.height);
        void PlaceBar(Image fill, Rect rect) => PlaceBattle((RectTransform)fill.transform.parent, rect);
        void ReflowBattleHud()
        {
            if (Page != "battle" || battleStatus == null || game.Combat == null) return;
            Vector2 size = root.rect.size; Rect safe = UiSafeArea.Current;
            bool boss = bossHud != null && bossHud.gameObject.activeSelf;
            if (size == battleSize && safe == battleSafe && boss == battleBoss) return;
            Canvas.ForceUpdateCanvases(); size = root.rect.size;
            battleSize = size; battleSafe = safe; battleBoss = boss;
            var plan = new BattleHudLayout(size.x, size.y, boss); battleLayout = plan;
            PlaceBattle(header, plan.header); PlaceBattle(footer, plan.footer); PlaceBattle(battleStatus, plan.status); PlaceBattle(battleActions, plan.actions); PlaceBattle(battleWorld, plan.world);
            if(battleIdle!=null)Place((RectTransform)battleIdle.transform,plan.world.xMax-128,plan.world.y+8,120,48);
            headerTitle.fontSize = plan.compact ? 22 : 26;
            Place(headerTitle.rectTransform, 12, 5, size.x - 154, plan.compact ? 38 : 40);
            headerSubtitle.gameObject.SetActive(!plan.compact);
            Place(headerSubtitle.rectTransform, 14, plan.wide ? 43 : 48, size.x - 28, plan.wide ? 24 : 40);
            Place((RectTransform)battleGrowth.transform, 10, plan.compact ? 2 : 44, size.x - (plan.compact ? 160 : 20), plan.compact ? 44 : plan.wide ? 26 : 44);
            var settings = header.GetComponentsInChildren<Button>().Single(b => b.name == "설정·안내");
            Place((RectTransform)settings.transform, size.x - 136, plan.compact ? 2 : 5, 124, 44);
            float w = plan.status.width;
            if (plan.compact)
            {
                float left = w * .5f - 12;
                PlaceBar(hpFill, new Rect(8, 4, left, 25)); Place(hpText.rectTransform, 8, 3, left, 28);
                PlaceBar(resourceFill, new Rect(8, 34, left, 20)); Place(resourceText.rectTransform, 8, 31, left, 25);
                PlaceBar(meterFill, new Rect(left + 20, 71, w - left - 28, 5));
                Place(meterText.rectTransform, left + 20, 46, w - left - 28, 24); meterText.fontSize = 16;
                Place(timerText.rectTransform, left + 20, 0, 84, 44); timerText.fontSize = 23;
                float cell = (w - left - 114) / 3;
                for (int i = 0; i < 3; i++) Place((RectTransform)battleSpeeds[i].transform, left + 108 + i * cell, 0, cell - 4, 44);
            }
            else if (plan.wide)
            {
                float left = w - 230;
                PlaceBar(hpFill, new Rect(10, 8, left, 27)); Place(hpText.rectTransform, 10, 6, left, 30);
                PlaceBar(resourceFill, new Rect(10, 44, left, 19)); Place(resourceText.rectTransform, 10, 39, left, 28);
                PlaceBar(meterFill, new Rect(10, 75, left, 6)); Place(meterText.rectTransform, 10, 82, w - 20, 28); meterText.fontSize = 18;
                Place(timerText.rectTransform, w - 210, 5, 200, 36); timerText.fontSize = 28;
                for (int i = 0; i < 3; i++) Place((RectTransform)battleSpeeds[i].transform, w - 210 + i * 68, 49, 60, 44);
            }
            else
            {
                PlaceBar(hpFill, new Rect(8, 4, w - 16, 28)); Place(hpText.rectTransform, 8, 3, w - 16, 30);
                PlaceBar(resourceFill, new Rect(8, 40, w - 16, 20)); Place(resourceText.rectTransform, 8, 36, w - 16, 28);
                PlaceBar(meterFill, new Rect(8, 70, w - 16, 6)); Place(meterText.rectTransform, 8, 78, w - 16, 32); meterText.fontSize = 17;
                Place(timerText.rectTransform, 8, 112, 86, 38); timerText.fontSize = 26;
                float cell = (w - 110) / 3;
                for (int i = 0; i < 3; i++) Place((RectTransform)battleSpeeds[i].transform, 102 + i * cell, 113, cell - 6, 38);
            }
            PlaceBattle((RectTransform)battleEffects.transform, new Rect(hpText.rectTransform.anchoredPosition.x, -hpText.rectTransform.anchoredPosition.y, hpText.rectTransform.rect.width, hpText.rectTransform.rect.height));
            if (boss)
            {
                float height = plan.compact ? 48 : plan.wide ? 82 : 76;
                Place(bossHud, 6, plan.status.height - height, w - 12, height);
                bossTitle.fontSize = plan.compact ? 17 : 19; bossActionLabel.fontSize = plan.compact ? 16 : 17;
                Place(bossTitle.rectTransform, 6, 0, w - 24, plan.compact ? 22 : plan.wide ? 30 : 42);
                Place(bossActionLabel.rectTransform, 6, plan.compact ? 21 : plan.wide ? 31 : 42, w - 24, plan.compact ? 22 : plan.wide ? 36 : 28);
                PlaceBar(bossHealthFill, new Rect(6, height - 5, w - 24, 4));
            }
            battleSkills.gameObject.SetActive(!plan.collapsedSkills);
            float reasonHeight = plan.collapsedSkills ? 44 : plan.wide ? 62 : 46;
            Place((RectTransform)battleReason.transform, 0, 0, plan.actions.width, reasonHeight);
            Place(battleSkills, 6, reasonHeight, plan.actions.width - 12, Mathf.Max(1, plan.actions.height - reasonHeight - 4));
            int columns = plan.wide ? 1 : size.x >= 600 ? 4 : 2; float cellWidth = battleSkills.rect.width / columns;
            for (int i = 0; i < skillLabels.Count; i++)
            {
                var parent = (RectTransform)skillLabels[i].transform.parent;
                Place(parent, (i % columns) * cellWidth, (i / columns) * 55, cellWidth - 4, 52);
                float icon = plan.wide || columns == 2 ? 40 : 32;
                Place(battleIcons[i], 0, 5, icon, icon); Place(skillLabels[i].rectTransform, icon + 6, 0, cellWidth - icon - 12, 52);
            }
            var mini = root.Find("Rift minimap") as RectTransform;
            if (mini != null)
            {
                mini.gameObject.SetActive(plan.wide);
                if (plan.wide)
                {
                    PlaceBattle(mini, plan.minimap); float mapSize = Mathf.Min(plan.minimap.height - 76, plan.minimap.width - 16);
                    Place(mini.Find("Map viewport") as RectTransform, (plan.minimap.width - mapSize) * .5f, 6, mapSize, mapSize);
                    Place(chestCountText.rectTransform, 8, mapSize + 9, plan.minimap.width - 16, 27);
                    Place(fieldStatusText.rectTransform, 8, mapSize + 38, plan.minimap.width - 16, 35);
                }
            }
            foreach (var button in footer.GetComponentsInChildren<Button>())
            {
                var rect = (RectTransform)button.transform; rect.offsetMin = new Vector2(4, 12); rect.offsetMax = new Vector2(-4, -4); button.GetComponentInChildren<Text>().fontSize = plan.compact || size.x < 600 ? 17 : 21;
            }
            Canvas.ForceUpdateCanvases(); var corners = new Vector3[4]; battleWorld.GetWorldCorners(corners);
            BattleViewport = UnityEngine.Rect.MinMaxRect(corners[0].x / Screen.width, corners[0].y / Screen.height, corners[2].x / Screen.width, corners[2].y / Screen.height);
            var borders = new[] { UnityEngine.Rect.MinMaxRect(0,0,BattleViewport.xMin,1), UnityEngine.Rect.MinMaxRect(BattleViewport.xMax,0,1,1), UnityEngine.Rect.MinMaxRect(BattleViewport.xMin,0,BattleViewport.xMax,BattleViewport.yMin), UnityEngine.Rect.MinMaxRect(BattleViewport.xMin,BattleViewport.yMax,BattleViewport.xMax,1) };
            for (int i = 0; i < 4; i++) { battleCurtains[i].anchorMin = borders[i].min; battleCurtains[i].anchorMax = borders[i].max; battleCurtains[i].offsetMin = battleCurtains[i].offsetMax = Vector2.zero; }
            UpdateBattleBrief();
        }
        void UpdateBattleBrief()
        {
            if (actionText == null || battleLayout == null) return;
            if (battleLayout.compact)
            {
                var run = game.Combat.State;
                if (run.training >= 0) meterText.text = Loc.F("표적 {0} / {1}", run.kills, run.enemies.Count);
                else if (run.phase != RunPhase.Boss && game.Combat.ObjectiveActive)
                    meterText.text = run.layout.objective==RiftObjectiveKind.Seals?Loc.F("봉인 {0} / {1} · 게이지 {2} / {3}", game.Combat.SealsBroken, run.layout.seals.Count, Mathf.Min(CombatSimulation.GateMeter,run.meter), CombatSimulation.GateMeter):Loc.F("{0} · 게이지 {1} / {2}",ObjectiveProgress(run.layout),Mathf.Min(CombatSimulation.GateMeter,run.meter),CombatSimulation.GateMeter);
                else if (run.phase != RunPhase.Boss) meterText.text = Loc.F("처치 {0} · 게이지 {1} / 100", run.kills, Mathf.Min(100,run.meter));
            }
            actionText.text = Loc.T(toastTime > 0 && toast != null && !string.IsNullOrEmpty(toast.text) ? toast.text : fullBattleAction);
            var pause = footer.GetComponentsInChildren<Button>().FirstOrDefault(b => b.name == "일시정지");
            if (pause != null) pause.GetComponentInChildren<Text>().text = Loc.T(game.Combat.State.paused||game.ForegroundResumeRequired ? "재개" : "일시정지");
            if(battleIdle!=null)battleIdle.interactable=game.CanEnterIdle;
            // The full action remains available in the combat overview and decision log.
            while (actionText.preferredHeight > actionText.rectTransform.rect.height + 1 && actionText.text.Length > 8)
                actionText.text = actionText.text.Substring(0, actionText.text.Length - (actionText.text.EndsWith("…") ? 2 : 1)) + "…";
            for (int i = 0; i < battleSpeeds.Count; i++) battleSpeeds[i].GetComponentInChildren<Text>().fontSize = i > 0 || battleLayout.compact ? 16 : 20;
        }
        void ClearBattleLayout()
        {
            if (battleMatte != null) { battleMatte.gameObject.SetActive(false); Destroy(battleMatte.gameObject); }
            battleMatte = null; battleLayout = null; battleSize = Vector2.zero; BattleViewport = new Rect(0,0,1,1);
        }
    }
}
