using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Hellscript.Tests
{
    // The Hunt Edict tab column opens beside the dock's Hunt Edict button, so the other dock shortcuts stay uncovered.
    public sealed class EdictMenuColumnTests
    {
        const BindingFlags Access=BindingFlags.Instance|BindingFlags.NonPublic;
        GameCatalog catalog;GameStore store;GameObject controller,host,quick,hudHost;GameUI ui;GlobalHudView hud;float aspect;

        [SetUp] public void Setup()
        {
            Loc.UseSource();aspect=UiSafeArea.AspectRatio;
            catalog=ScriptableObject.CreateInstance<GameCatalog>();catalog.Populate();
            var account=GameStore.NewAccount(catalog);GuideTutorial.OpenEverything(account);store=GameStore.Sandbox(account);store.Data.Hero.created=true;
            controller=new GameObject("Column controller");controller.SetActive(false);var game=controller.AddComponent<GameController>();
            typeof(GameController).GetProperty("Store").GetSetMethod(true).Invoke(game,new object[]{store});game.catalog=catalog;
            host=new GameObject("Column host",typeof(RectTransform));ui=host.AddComponent<GameUI>();Set("game",game);
            hudHost=new GameObject("Column hud",typeof(RectTransform));hud=hudHost.AddComponent<GlobalHudView>();hud.Initialize(null,null);Set("globalHud",hud);
            quick=new GameObject("Edict quick",typeof(RectTransform));quick.transform.SetParent(host.transform,false);
            // Both rects start at the world origin so a rect's anchored position is its world position, as on the full-screen quick canvas.
            foreach(var rect in new[]{(RectTransform)host.transform,(RectTransform)quick.transform}){rect.anchorMin=rect.anchorMax=rect.pivot=Vector2.zero;rect.sizeDelta=Vector2.zero;rect.anchoredPosition=Vector2.zero;}
            Set("quickRoot",(RectTransform)quick.transform);Set("quickScale",1f);Set("quickKind","edict");
        }
        [TearDown] public void Teardown()
        {
            if(ui!=null&&ui.Gate!=null)Call("OnDestroy");
            Object.DestroyImmediate(host);Object.DestroyImmediate(hudHost);Object.DestroyImmediate(controller);Object.DestroyImmediate(catalog);UiSafeArea.AspectRatio=aspect;Loc.UseSource();
        }
        void Set(string field,object value)=>typeof(GameUI).GetField(field,Access).SetValue(ui,value);
        object Call(string method,params object[] args)=>typeof(GameUI).GetMethod(method,Access).Invoke(ui,args);
        static void Quiet(System.Action action)
        {
            bool previous=UnityEngine.TestTools.LogAssert.ignoreFailingMessages;var errors=new System.Collections.Generic.List<string>();
            Application.LogCallback collect=(message,trace,type)=>{if((type==LogType.Error||type==LogType.Exception||type==LogType.Assert)&&!message.Contains("Destroy may not be called from edit mode!"))errors.Add(message);};
            Application.logMessageReceived+=collect;UnityEngine.TestTools.LogAssert.ignoreFailingMessages=true;
            try{action();}finally{Application.logMessageReceived-=collect;UnityEngine.TestTools.LogAssert.ignoreFailingMessages=previous;}
            Assert.IsEmpty(errors);
        }
        static Rect Bounds(Component c){var v=new Vector3[4];((RectTransform)c.transform).GetWorldCorners(v);return Rect.MinMaxRect(v[0].x,v[0].y,v[2].x,v[2].y);}
        // Destroy is deferred; same-name inactive columns can remain until the end of the frame.
        RectTransform Column()=>quick.GetComponentsInChildren<RectTransform>().Single(r=>r.name=="Casual edict tabs");
        Rect LayoutColumn(float ratio,bool low=false)
        {
            UiSafeArea.AspectRatio=ratio;var safe=UiSafeArea.Current;
            typeof(GlobalHudView).GetProperty("Layout").GetSetMethod(true).Invoke(hud,new object[]{new GlobalHudLayout(safe.width,safe.height)});
            var button=new Rect(safe.xMax-12-48,low?safe.yMin+30:safe.yMax-.3f*safe.height,48,48);
            float canvasScale=(float)typeof(GameUI).GetField("quickScale",Access).GetValue(ui);
            Set("quickAnchor",new Rect(button.position/canvasScale,button.size/canvasScale));Call("BuildEdictTabIcons");return button;
        }

        // The anchor stands in for the dock's Hunt Edict button: on the right edge, high on the screen or so low that the column cannot hang below it.
        [TestCase(440f/956,false)][TestCase(440f/956,true)][TestCase(956f/440,false)][TestCase(956f/440,true)][TestCase(16f/9,false)][TestCase(16f/9,true)]
        public void TheColumnSitsLeftOfTheButtonAtItsHeightAndStaysOnScreen(float ratio,bool low)
        {
            var button=LayoutColumn(ratio,low);var safe=UiSafeArea.Current;
            var column=Bounds(Column());
            Assert.LessOrEqual(column.xMax,button.xMin+.01f,"The column must clear the dock buttons.");
            Assert.GreaterOrEqual(column.xMin,safe.xMin-.01f);Assert.GreaterOrEqual(column.yMin,safe.yMin-.01f);Assert.LessOrEqual(column.yMax,safe.yMax+.01f);
            Assert.GreaterOrEqual(column.yMin,safe.yMin+hud.Layout.Pixels(hud.Layout.potionBounds).yMax+8*hud.Layout.presentationScale-.01f,"The raised column must clear the persistent skill and potion row.");
            if(!low)Assert.AreEqual(button.yMax,column.yMax,.01f,"The column starts at the top of the Hunt Edict button.");
            float k=hud.Layout.presentationScale,nominalRow=(hud.Layout.compact?46:hud.Layout.landscape?64:48)*k;
            bool fits=button.yMax-(HuntEdictHudRows.Tabs.Length*nominalRow+8*k)>=safe.yMin+hud.Layout.Pixels(hud.Layout.potionBounds).yMax+8*k;
            float rowHeight=Column().GetComponentsInChildren<Button>().First().GetComponent<RectTransform>().rect.height;
            foreach(string tab in HuntEdictHudRows.Tabs)
            {
                var tabBounds=Bounds(Column().Find("edict-sheet-tab-"+tab));
                Assert.IsTrue(column.Contains(tabBounds.min+Vector2.one*.01f)&&column.Contains(tabBounds.max-Vector2.one*.01f),tab);
                Assert.AreEqual(rowHeight,tabBounds.height,.01f,"All nine rows must shrink together.");
                if(!low&&fits)Assert.AreEqual(nominalRow,tabBounds.height,.01f,"Rows must keep their normal size while they fit below the anchor.");
            }
            // The entry list opens further left, clear of both the column and the button.
            Set("sheetTab","combat");Set("positionList",true);Call("BuildEdictSheet");
            var list=Bounds(quick.transform.Find("edict-sheet"));
            Assert.LessOrEqual(list.xMax,column.xMin+.01f);Assert.GreaterOrEqual(list.xMin,safe.xMin-.01f);
            Assert.GreaterOrEqual(list.yMin,safe.yMin-.01f);Assert.LessOrEqual(list.yMax,safe.yMax+.01f);
        }

        [TestCase("explore")][TestCase("loot")][TestCase("bag")]
        public void RebuiltPortraitSheetsKeepEveryTabHitTargetClear(string tab)
        {
            store.Data.Hero.level=40;store.Data.Hero.highestClear=20;ContentUnlocks.Reconcile(store.Data);
            LayoutColumn(440f/956);Set("sheetTab",tab);Set("positionList",false);
            Assert.IsNotEmpty(HuntEdictHudRows.Groups(tab,store.Data,store.Data.Hero),"The fixture must expose this sheet.");
            Call("RebuildEdictSheet");
            var column=Column();var sheet=quick.transform.Find("edict-sheet");
            Assert.LessOrEqual(Bounds(sheet).xMax,Bounds(column).xMin-.01f,"The portrait sheet must leave the complete tab rail uncovered.");
            Assert.GreaterOrEqual(Bounds(sheet).xMin,UiSafeArea.Current.xMin-.01f);
            foreach(var button in column.GetComponentsInChildren<Button>())Assert.IsFalse(Bounds(sheet).Overlaps(Bounds(button)),button.name);
            Assert.Greater(column.GetSiblingIndex(),sheet.GetSiblingIndex(),"The rebuilt card must not intercept taps on the rail.");
            Assert.AreEqual(HuntEdictHudRows.Tabs.Length,column.GetComponentsInChildren<Button>().Length);
        }

        [TestCase(true,"ko",440f/956)][TestCase(true,"en",440f/956)]
        [TestCase(true,"ko",956f/440)][TestCase(true,"en",956f/440)]
        [TestCase(false,"ko",440f/956)][TestCase(false,"en",440f/956)]
        public void BothMenusUseTheNineRequestedEntriesAndTheirOwnArtMapping(bool casual,string language,float ratio)
        {
            if(language=="en")Loc.Use("en",LocalizationTable.Parse(Resources.Load<TextAsset>("Localization/en").text).Entries);
            if(casual)LayoutColumn(ratio);else{Set("quickAnchor",new Rect(16,200,44,44));Call("BuildEdictTabIcons");}
            var buttons=quick.GetComponentsInChildren<Button>().Where(b=>b.name.StartsWith("edict-sheet-tab-")).ToArray();
            string[] tabs={"skills","combat","survival","explore","loot","bag","repeat","autoEquip","presets"};
            CollectionAssert.AreEqual(tabs.Select(t=>"edict-sheet-tab-"+t),buttons.Select(b=>b.name));
            if(casual)Assert.AreEqual(1,buttons.Select(b=>b.GetComponentInChildren<Text>().fontSize).Distinct().Count(),"All nine captions shrink together.");
            for(int i=0;i<tabs.Length;i++)
            {
                var icon=buttons[i].transform.Find("Icon").GetComponent<Image>();
                Assert.AreSame(GlobalHudView.LoadArt("menu-edict-"+(tabs[i]=="autoEquip"?"auto-equip":tabs[i])),icon.sprite);
                Assert.IsFalse(icon.raycastTarget);Assert.IsTrue(buttons[i].interactable);
                if(casual)
                {
                    Assert.AreEqual(1,buttons[i].GetComponentsInChildren<Text>().Length,"Each button owns one caption.");
                    var title=buttons[i].GetComponentInChildren<Text>();
                    string expected=tabs[i]=="autoEquip"?"추천 장비":HuntEdictUiCatalog.TabTitles[System.Array.IndexOf(HuntEdictUiCatalog.Tabs,tabs[i])];
                    Assert.AreEqual(Loc.T(expected),title.text);Assert.LessOrEqual(title.preferredHeight,title.rectTransform.rect.height+.1f);
                }
            }
        }

        [TestCase("repeat",false)][TestCase("autoEquip",false)][TestCase("skills",false)]
        [TestCase("repeat",true)][TestCase("autoEquip",true)][TestCase("skills",true)]
        public void DedicatedEntriesOpenTheirActualWindowAndRestoreTheMenuOnClose(string tab,bool battle)
        {
            string practice=tab=="skills"?"H07":tab=="repeat"?"H11":"H09";Set("tutorialFocus",practice);
            var stack=host.AddComponent<ContentWindowHost>();
            if(battle)
            {
                var simulation=new CombatSimulation(store.Data,catalog,1,seed:1741);store.Data.suspendedRun=simulation.State;
                typeof(GameController).GetField("combat",Access).SetValue(controller.GetComponent<GameController>(),simulation);
                stack.Initialize(()=>simulation.State);
            }
            ContentWindowHost.Attach((RectTransform)quick.transform,()=>{});
            LayoutColumn(440f/956);Quiet(()=>Call("OpenEdictSheet",tab));
            Assert.AreEqual(2,stack.Count);Assert.AreNotSame(quick.GetComponent<RectTransform>(),stack.TopOwner);
            Assert.IsFalse(quick.GetComponent<CanvasGroup>().interactable,"The child window owns input while open.");
            Assert.IsNull(quick.transform.Find("edict-sheet"),"Dedicated entries must not build the old inline sheet.");

            if(tab=="skills")
            {
                var tree=host.GetComponentInChildren<SkillTreeD4Window>(true);Assert.IsNotNull(tree);
                tree.Select(SkillTreeD4Selection.Active(0));Assert.IsTrue(tree.TacticsEntryVisible);tree.Close();
            }
            else
            {
                string field=tab=="repeat"?"repeatSettingsWindow":"gearRecommendationSettings";
                var popup=(ContentWindowView)typeof(GameUI).GetField(field,Access).GetValue(ui);Assert.IsNotNull(popup);
                if(tab=="repeat")Assert.AreEqual("repeat-settings-window",popup.Frame.name);
                else Assert.IsNotNull(popup.GetComponent<RiftGearSettingsProbe>());
                Quiet(popup.Close);Assert.IsNull(typeof(GameUI).GetField(field,Access).GetValue(ui));
            }
            Assert.IsTrue(quick.activeInHierarchy);Assert.IsTrue(Column().gameObject.activeInHierarchy);
            Assert.AreEqual(9,Column().GetComponentsInChildren<Button>().Length);
            Assert.IsTrue(Column().GetComponentsInChildren<Button>().All(b=>b.IsInteractable()),"The restored menu must accept the next tap.");
            Assert.AreSame(quick.GetComponent<RectTransform>(),stack.TopOwner);Assert.AreEqual(1,stack.Count);
            Assert.IsTrue(quick.GetComponent<CanvasGroup>().interactable);Assert.IsTrue(quick.GetComponent<CanvasGroup>().blocksRaycasts);
            Assert.AreEqual("",typeof(GameUI).GetField("tutorialFocus",Access).GetValue(ui));
            Assert.IsFalse(Tutorials.Record(store.Data,practice).practiced,"Closing without saving cannot complete a practice.");
            if(battle)Assert.IsTrue(controller.GetComponent<GameController>().Combat.State.paused,"Closing the child must retain the menu's pause lease.");
        }

        [TestCase("side")][TestCase("alias")][TestCase("tutorial")]
        public void RepeatEntrypointsShareTheSameUnlockAndNewPopup(string entry)
        {
            LayoutColumn(440f/956);store.Data.guide.everythingOpen=false;store.Data.guide.edictLegacyAccess=true;
            foreach(var hero in store.Data.heroes)hero.highestClear=14;
            void Open()
            {
                if(entry=="side")Call("OpenEdictSheet","repeat");
                else if(entry=="alias")ui.ShowEdictRepeat();
                else Call("StartTutorialAction",Tutorials.All.Single(t=>t.id=="H11"));
            }
            Assert.IsFalse(RepeatHunt.SettingsUnlocked(store.Data));string before=JsonUtility.ToJson(store.Data.Hero);
            Quiet(Open);Assert.IsNull(typeof(GameUI).GetField("repeatSettingsWindow",Access).GetValue(ui));
            Assert.AreEqual(before,JsonUtility.ToJson(store.Data.Hero));
            store.Data.Hero.highestClear=15;Assert.IsTrue(RepeatHunt.SettingsUnlocked(store.Data));Quiet(Open);
            var popup=(ContentWindowView)typeof(GameUI).GetField("repeatSettingsWindow",Access).GetValue(ui);Assert.IsNotNull(popup);
            Assert.AreEqual("repeat-settings-window",popup.Frame.name);
        }

        [TestCase("묘굴 구더기 떼","12345.6/초 · 100%")][TestCase("Burrow maggot swarm","12345.6/s · 100%")]
        public void NarrowMonsterRowsKeepNamesAndAmountsSeparateAndRemainScrollable(string name,string amount)
        {
            var view=UiLayout.Rect("Monster viewport",host.transform);UiLayout.Place(view,0,0,112,100);
            var content=UiLayout.Rect("Monster rows",view);Set("monsterRowsView",view);Set("monsterRowsContent",content);
            var scroll=view.gameObject.AddComponent<ScrollRect>();scroll.horizontal=false;scroll.viewport=view;scroll.content=content;
            var rows=(System.Array)typeof(GameUI).GetField("monsterRows",Access).GetValue(ui);
            for(int i=0;i<rows.Length;i++)
            {
                var row=Call("MakeCasualRow",content,false);rows.SetValue(row,i);
                ((Text)row.GetType().GetField("name").GetValue(row)).text=name;
                ((Text)row.GetType().GetField("amount").GetValue(row)).text=amount;
            }
            Call("LayoutNarrowMonsterRows",1f,112f);
            foreach(var row in rows)
            {
                var title=(Text)row.GetType().GetField("name").GetValue(row);var value=(Text)row.GetType().GetField("amount").GetValue(row);
                Assert.LessOrEqual(title.preferredHeight,title.rectTransform.rect.height+.1f);Assert.LessOrEqual(value.preferredHeight,value.rectTransform.rect.height+.1f);
                Assert.LessOrEqual(Bounds(value).yMax,Bounds(title).yMin-1.9f);
                foreach(var text in new[]{title,value}){Assert.AreEqual(HorizontalWrapMode.Wrap,text.horizontalOverflow);Assert.Greater(text.rectTransform.rect.width,0);}
            }
            Assert.Greater(content.rect.height,view.rect.height,"All five readable rows must remain reachable by scrolling.");
            scroll.verticalNormalizedPosition=0;
            var last=rows.GetValue(rows.Length-1);var bar=(RectTransform)last.GetType().GetField("track").GetValue(last);
            Assert.GreaterOrEqual(Bounds(bar).yMin,Bounds(view).yMin-.1f);Assert.LessOrEqual(Bounds(bar).yMax,Bounds(view).yMax+.1f);
        }

        [TestCase(440f/956,"combat","position.engage")][TestCase(440f/956,"combat","target.priorityGrade")][TestCase(440f/956,"combat","common.order")]
        [TestCase(440f/956,"survival","dodge.ground.policy")][TestCase(440f/956,"survival","dodge.control")]
        [TestCase(956f/440,"combat","position.engage")][TestCase(956f/440,"combat","target.priorityGrade")][TestCase(956f/440,"combat","common.order")]
        [TestCase(956f/440,"survival","dodge.ground.policy")][TestCase(956f/440,"survival","dodge.control")]
        [TestCase(16f/9,"combat","position.engage")][TestCase(16f/9,"combat","target.priorityGrade")][TestCase(16f/9,"combat","common.order")]
        [TestCase(16f/9,"survival","dodge.ground.policy")][TestCase(16f/9,"survival","dodge.control")]
        public void DetailedSheetsLeaveEveryTabHitTargetClear(float ratio,string tab,string option)
        {
            LayoutColumn(ratio);var safe=UiSafeArea.Current;
            var groups=HuntEdictHudRows.Groups(tab,store.Data,store.Data.Hero);
            int group=groups.ToList().FindIndex(g=>g.ids.Contains(option));Assert.GreaterOrEqual(group,0,"The fixture must expose the detailed editor.");
            Set("sheetTab",tab);Set("positionList",false);Set("sheetGroup",group);Call("BuildEdictSheet");
            var column=Column();var sheet=Bounds(quick.transform.Find("edict-sheet"));
            Assert.LessOrEqual(sheet.xMax,Bounds(column).xMin-.01f,"The detailed editor must end before the tab column, regardless of sibling order.");
            Assert.GreaterOrEqual(sheet.xMin,safe.xMin-.01f);Assert.LessOrEqual(sheet.xMax,safe.xMax+.01f);
            foreach(var button in column.GetComponentsInChildren<Button>())
            {
                Assert.IsTrue(button.interactable,button.name);
                Assert.IsFalse(sheet.Overlaps(Bounds(button)),button.name+" must remain clear of the raycastable sheet.");
            }
        }

        void OpenWorldPreviewEditor(string language,int width,int height,string option)
        {
            if(language=="en")Loc.Use("en",LocalizationTable.Parse(Resources.Load<TextAsset>("Localization/en").text).Entries);
            float ratio=width/(float)height;UiSafeArea.AspectRatio=ratio;var safe=UiSafeArea.Current;bool land=width>height;
            float scale=Mathf.Min(safe.width/(land?956:440),safe.height/(land?440:956));
            Set("quickScale",scale);quick.transform.localScale=Vector3.one*scale;LayoutColumn(ratio);
            var header=GlobalHudView.Rect("Battle header",host.transform);GlobalHudView.SetRect(header,new Rect(safe.xMin,safe.yMax-116*scale,safe.width,116*scale));Set("header",header);
            Set("quickLive",true);Set("sheetTab","combat");Set("positionList",false);
            Set("sheetGroup",HuntEdictHudRows.Groups("combat",store.Data,store.Data.Hero).ToList().FindIndex(g=>g.ids.Contains(option)));
            Call("BuildEdictSheet");Canvas.ForceUpdateCanvases();
        }
        [TestCase("ko")][TestCase("en")]
        public void PortraitSevenCardOrderReservesTheWorldPreviewBeforeGrowingThePopup(string language)
        {
            OpenWorldPreviewEditor(language,440,956,"common.order");var safe=UiSafeArea.Current;float scale=safe.width/440;
            var card=(RectTransform)typeof(GameUI).GetField("sheetCard",Access).GetValue(ui);
            var scroll=(ScrollRect)typeof(GameUI).GetField("sheetScroll",Access).GetValue(ui);
            Assert.AreEqual(7,card.GetComponentsInChildren<HuntEdictOrderCard>().Length,"All seven actions must remain available.");
            var viewport=(Rect)Call("EdictWorldViewport");Assert.Greater(viewport.width,0);Assert.GreaterOrEqual(viewport.height*Screen.height,64-.01f);
            float gap=Mathf.Max(8,14*scale),top=safe.yMax-116*scale-36*Mathf.Max(.75f,hud.Layout.scale)-64-gap;
            Assert.AreEqual(top,Bounds(card).yMax,.05f,"The overflowing frame must stop below the header and minimum world viewport.");
            Assert.AreEqual((top-Bounds(card).yMin)/scale,card.rect.height,.05f);
            Assert.Greater(scroll.content.rect.height,scroll.viewport.rect.height);Assert.IsTrue(scroll.vertical);
            Assert.IsNotNull(scroll.verticalScrollbar);Assert.IsTrue(scroll.verticalScrollbar.gameObject.activeSelf);
            Assert.AreEqual(scroll.viewport.rect.height/scroll.content.rect.height,scroll.verticalScrollbar.size,.01f);
        }
        static System.Collections.Generic.IEnumerable<TestCaseData> PreviewEditorSizes()
        {
            foreach(var size in new[]{new[]{440,956},new[]{956,440},new[]{1600,900},new[]{1600,1000},new[]{2100,900},new[]{900,1000}})
                foreach(string language in new[]{"ko","en"})foreach(string option in new[]{"position.engage","target.priorityGrade","common.order"})
                    yield return new TestCaseData(language,size[0],size[1],option);
        }
        [TestCaseSource(nameof(PreviewEditorSizes))]
        public void EveryPreviewEditorKeepsTheWorldOutsideThePopupAndInsideTheHeader(string language,int width,int height,string option)
        {
            OpenWorldPreviewEditor(language,width,height,option);
            var viewport=(Rect)Call("EdictWorldViewport");var world=new Rect(viewport.x*Screen.width,viewport.y*Screen.height,viewport.width*Screen.width,viewport.height*Screen.height);
            var card=(RectTransform)typeof(GameUI).GetField("sheetCard",Access).GetValue(ui);var safe=UiSafeArea.Current;
            Assert.GreaterOrEqual(world.width,64-.01f);Assert.GreaterOrEqual(world.height,64-.01f);Assert.IsFalse(world.Overlaps(Bounds(card)));
            float scale=(float)typeof(GameUI).GetField("quickScale",Access).GetValue(ui);
            Assert.LessOrEqual(world.yMax,safe.yMax-116*scale-36*Mathf.Max(.75f,hud.Layout.scale)+.01f);
            Assert.GreaterOrEqual(world.xMin,safe.xMin-.01f);Assert.LessOrEqual(world.xMax,safe.xMax+.01f);
            var scroll=(ScrollRect)typeof(GameUI).GetField("sheetScroll",Access).GetValue(ui);bool overflow=scroll.content.rect.height>scroll.viewport.rect.height+.01f;
            Assert.AreEqual(overflow,scroll.vertical);Assert.AreEqual(overflow,scroll.verticalScrollbar.gameObject.activeSelf);
        }

        [TestCase("강화 스킬·버프 물약이 끝나기 전에 다시 사용")]
        [TestCase("Refresh enhancement skills and buff potions before they expire")]
        public void NarrowOrderDescriptionsStayComplete(string text)
        {
            LayoutColumn(440f/956);var groups=HuntEdictHudRows.Groups("combat",store.Data,store.Data.Hero);
            Set("sheetTab","combat");Set("positionList",false);Set("sheetGroup",groups.ToList().FindIndex(g=>g.ids.Contains("common.order")));Call("BuildEdictSheet");
            var notes=(System.Collections.Generic.List<Text>)typeof(GameUI).GetField("orderDescriptions",Access).GetValue(ui);
            var note=notes[0];Call("SetOrderDescription",note,text);
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)typeof(GameUI).GetField("sheetContent",Access).GetValue(ui));
            var card=(RectTransform)note.transform.parent;var header=card.Find("Order header");
            Assert.AreEqual(text,note.text);Assert.AreEqual(HorizontalWrapMode.Wrap,note.horizontalOverflow);
            Assert.LessOrEqual(note.preferredHeight,note.rectTransform.rect.height+.1f);
            Assert.IsFalse(Bounds(note).Overlaps(Bounds(header)));
            Assert.IsTrue(Bounds(card).Contains(Bounds(note).min+Vector2.one*.01f)&&Bounds(card).Contains(Bounds(note).max-Vector2.one*.01f));
        }

        [TestCase("ko")][TestCase("en")]
        public void TheLootLessonKeepsItsCardAndFirstRankDestinationVisible(string language)
        {
            if(language=="en")Loc.Use("en",LocalizationTable.Parse(Resources.Load<TextAsset>("Localization/en").text).Entries);
            var edit=new HuntEdictEditSession(store.Data.Hero);
            edit.ReplaceDraft(HuntEdictQuickMenu.With(edit.Draft,"common.order","DODGE,ATTACK,BUFF,PRIORITY_LOOT,OBJECTIVE,REGROUP,EXPLORE"));
            Assert.IsTrue(store.CommitHuntEdict(edit,catalog),store.Error);
            UiSafeArea.AspectRatio=440f/956;Set("quickScale",UiSafeArea.Current.width/440);LayoutColumn(440f/956);
            Set("guideLesson","loot");Set("sheetTab","combat");Set("positionList",false);
            Set("sheetGroup",HuntEdictHudRows.Groups("combat",store.Data,store.Data.Hero).ToList().FindIndex(g=>g.ids.Contains("common.order")));
            Call("BuildEdictSheet");var scroll=(ScrollRect)typeof(GameUI).GetField("sheetScroll",Access).GetValue(ui);LayoutRebuilder.ForceRebuildLayoutImmediate(scroll.content);
            var all=quick.GetComponentsInChildren<HuntEdictOrderCard>();Assert.AreEqual(7,all.Length,"Keep every action category.");
            var cards=all.Where(c=>c.Id=="DODGE"||c.Id=="PRIORITY_LOOT").ToArray();Assert.AreEqual(2,cards.Length);
            var viewport=Bounds(scroll.viewport);
            foreach(var card in cards){var bounds=Bounds(card);Assert.GreaterOrEqual(bounds.yMin,viewport.yMin-.01f);Assert.LessOrEqual(bounds.yMax,viewport.yMax+.01f);}
        }

        [TestCase("ko")][TestCase("en")]
        public void TheTreasureLessonKeepsBothDragEndpointsInsideTheViewport(string language)
        {
            if(language=="en")Loc.Use("en",LocalizationTable.Parse(Resources.Load<TextAsset>("Localization/en").text).Entries);
            UiSafeArea.AspectRatio=440f/956;Set("quickScale",UiSafeArea.Current.width/440);LayoutColumn(440f/956);
            Set("guideLesson","treasure");Set("sheetTab","combat");Set("positionList",false);
            Set("sheetGroup",HuntEdictHudRows.Groups("combat",store.Data,store.Data.Hero).ToList().FindIndex(g=>g.ids.Contains("target.priorityGrade")));
            if(!store.Data.guide.edictUnlocks.Contains(GuideTutorial.TreasureGrade))store.Data.guide.edictUnlocks.Add(GuideTutorial.TreasureGrade);
            Call("BuildEdictSheet");var scroll=(ScrollRect)typeof(GameUI).GetField("sheetScroll",Access).GetValue(ui);LayoutRebuilder.ForceRebuildLayoutImmediate(scroll.content);
            var cards=quick.GetComponentsInChildren<HuntEdictOrderCard>().Where(c=>c.Id=="TREASURE"||c.Id=="NORMAL").ToArray();Assert.AreEqual(2,cards.Length);
            var viewport=Bounds(scroll.viewport);
            foreach(var card in cards){var bounds=Bounds(card);Assert.GreaterOrEqual(bounds.yMin,viewport.yMin-.01f);Assert.LessOrEqual(bounds.yMax,viewport.yMax+.01f);}
            Assert.IsNotEmpty(cards.Single(c=>c.Id=="TREASURE").GetComponentsInChildren<Text>(),"Keep the full card content visible.");
        }

        ScrollRect OpenOverflowingPriorityGuide(string language)
        {
            Assert.IsNull(ContentWindowHost.BackLocked,"The fixture must not replace another tutorial input owner.");
            if(language=="en")Loc.Use("en",LocalizationTable.Parse(Resources.Load<TextAsset>("Localization/en").text).Entries);
            store=GuideTutorialTests.FreshWarrior(catalog);var run=store.StartGuideTutorial(catalog);Assert.IsNotNull(run,store.Error);
            var sim=new CombatSimulation(store.Data,catalog,1,3,run,recordResume:false,tutorial:true);
            run.tutorialPhase=GuideTutorial.TargetTalk;run.paused=true;Assert.IsTrue(store.UnlockGuideEdict(run),store.Error);
            var game=controller.GetComponent<GameController>();typeof(GameController).GetProperty("Store").GetSetMethod(true).Invoke(game,new object[]{store});
            typeof(GameController).GetField("combat",Access).SetValue(game,sim);
            LayoutColumn(956f/440);Set("guideLesson","target-grade");Set("sheetTab","combat");Set("positionList",false);
            Set("sheetGroup",HuntEdictHudRows.Groups("combat",store.Data,store.Data.Hero).ToList().FindIndex(g=>g.ids.Contains("target.priorityGrade")));
            Call("BuildEdictSheet");TickPriorityGuide();
            var scroll=(ScrollRect)typeof(GameUI).GetField("sheetScroll",Access).GetValue(ui);
            Assert.IsTrue(game.TutorialActive);Assert.IsTrue(Tutorials.Mandatory(store.Data));Assert.IsTrue(scroll.vertical);
            Assert.Greater(scroll.content.rect.height,scroll.viewport.rect.height+.01f,"The regression requires an overflowing landscape list.");
            Assert.IsTrue(scroll.verticalScrollbar.gameObject.activeInHierarchy);Assert.AreEqual("edict-target-card-BOSS",ui.Gate.Target.name);
            return scroll;
        }
        void TickPriorityGuide()
        {
            Quiet(()=>Assert.IsTrue((bool)Call("TickGuideTargetLesson")));
            typeof(PrologueGate).GetMethod("LateUpdate",Access).Invoke(ui.Gate,null);
        }
        [TestCase("ko")][TestCase("en")]
        public void TheGuidedCenterDragReordersAnOverflowingPriorityListAndAdvancesTheLesson(string language)
        {
            OpenOverflowingPriorityGuide(language);
            for(int step=0;step<2;step++)
            {
                var from=ui.Gate.Target;var to=ui.GuideDragTo;Assert.IsNotNull(to);var order=from.GetComponentInParent<HuntEdictOrderList>();
                Assert.IsNotNull(from.GetComponent<HuntEdictOrderCard>());Assert.GreaterOrEqual(UiVisibility.Fraction(from,UiSafeArea.Current),.99f);
                var start=ui.Gate.Hole.center;Assert.Less(Vector2.Distance(start,UiVisibility.ScreenRect(from).center),.01f,"Use the center shown by the guide hand.");
                string before=HuntEdictSummary.Value(store.Data.Hero.edict,"target.priorityGrade"),sourceName=from.name;long revision=store.Revision;
                var pointer=new UnityEngine.EventSystems.PointerEventData(null){button=UnityEngine.EventSystems.PointerEventData.InputButton.Left,position=start,pressPosition=start,delta=Vector2.up,eligibleForClick=true};
                Quiet(()=>
                {
                    try
                    {
                        UnityEngine.EventSystems.ExecuteEvents.Execute(from.gameObject,pointer,UnityEngine.EventSystems.ExecuteEvents.beginDragHandler);
                        Assert.IsTrue(HuntEdictOrderCard.Dragging,"The guided card's center must reorder even when its list scrolls.");
                        pointer.position=UiVisibility.ScreenRect(to).center;
                        UnityEngine.EventSystems.ExecuteEvents.Execute(from.gameObject,pointer,UnityEngine.EventSystems.ExecuteEvents.dragHandler);
                        Assert.AreEqual(revision,store.Revision,"The drag preview must not save.");
                        UnityEngine.EventSystems.ExecuteEvents.Execute(from.gameObject,pointer,UnityEngine.EventSystems.ExecuteEvents.endDragHandler);
                    }
                    finally{order.Cancel();}
                });
                Assert.AreNotEqual(before,HuntEdictSummary.Value(store.Data.Hero.edict,"target.priorityGrade"),store.Error);Assert.Greater(store.Revision,revision);
                TickPriorityGuide();Assert.AreNotEqual(sourceName,ui.Gate.Target.name,"The completed guided move must advance to the next instruction.");
            }
            Assert.IsTrue(GuideTutorial.GradeDone(store.Data.Hero.edict));Assert.AreEqual("message",typeof(GameUI).GetField("guideLesson",Access).GetValue(ui));
            Assert.AreEqual("Voice narration",ui.Gate.Target.name);
            ui.Gate.Target.GetComponent<Button>().onClick.Invoke();TickPriorityGuide();
            Assert.AreEqual("target-type",typeof(GameUI).GetField("guideLesson",Access).GetValue(ui));Assert.AreEqual("edict-target-category-TYPE",ui.Gate.Target.name);
        }
        [TestCase("ko")][TestCase("en")]
        public void TheSamePriorityCardCenterScrollsWhenTheGuideIsHidden(string language)
        {
            var scroll=OpenOverflowingPriorityGuide(language);var from=ui.Gate.Target;ui.Gate.Hide();
            Canvas.ForceUpdateCanvases();scroll.Rebuild(CanvasUpdate.PostLayout);
            float maximum=scroll.content.rect.height-scroll.viewport.rect.height;scroll.content.anchoredPosition=new Vector2(0,maximum/2);
            float beforeOffset=scroll.content.anchoredPosition.y;string before=HuntEdictSummary.Value(store.Data.Hero.edict,"target.priorityGrade");long revision=store.Revision;
            var start=UiVisibility.ScreenRect(from).center;
            var pointer=new UnityEngine.EventSystems.PointerEventData(null){button=UnityEngine.EventSystems.PointerEventData.InputButton.Left,position=start,pressPosition=start,delta=Vector2.up,eligibleForClick=true};
            Quiet(()=>
            {
                UnityEngine.EventSystems.ExecuteEvents.Execute(from.gameObject,pointer,UnityEngine.EventSystems.ExecuteEvents.beginDragHandler);
                pointer.position+=Vector2.up*(maximum/4);
                UnityEngine.EventSystems.ExecuteEvents.Execute(from.gameObject,pointer,UnityEngine.EventSystems.ExecuteEvents.dragHandler);
                UnityEngine.EventSystems.ExecuteEvents.Execute(from.gameObject,pointer,UnityEngine.EventSystems.ExecuteEvents.endDragHandler);
            });
            Assert.Greater(scroll.content.anchoredPosition.y,beforeOffset);Assert.IsFalse(HuntEdictOrderCard.Dragging);Assert.IsFalse(pointer.eligibleForClick);
            Assert.AreEqual(before,HuntEdictSummary.Value(store.Data.Hero.edict,"target.priorityGrade"));Assert.AreEqual(revision,store.Revision);
        }

        [Test] public void BothEndsOfAnExpandedGripReorderAnOverflowingList()
        {
            var scroll=OpenOverflowingPriorityGuide("ko");var card=ui.Gate.Target.GetComponent<HuntEdictOrderCard>();ui.Gate.Hide();
            var grip=card.GetComponentsInChildren<Text>().Single(t=>t.text=="≡").rectTransform;
            grip.anchoredPosition=new Vector2(card.Rect.rect.xMax-65,grip.anchoredPosition.y);grip.sizeDelta=new Vector2(60,grip.rect.height);
            Canvas.ForceUpdateCanvases();Assert.IsTrue(scroll.vertical);var bounds=Bounds(grip);
            foreach(float x in new[]{bounds.xMin+2,bounds.xMax-2})
            {
                var point=new Vector2(x,bounds.center.y);var pointer=new UnityEngine.EventSystems.PointerEventData(null){button=UnityEngine.EventSystems.PointerEventData.InputButton.Left,position=point,pressPosition=point};
                Quiet(()=>{try{card.OnBeginDrag(pointer);Assert.IsTrue(HuntEdictOrderCard.Dragging,"The whole visible grip must reorder.");}finally{card.OnCancel(new UnityEngine.EventSystems.BaseEventData(null));}});
            }
        }

        [Test] public void ADescriptionCanGrowWhileTheDragInsertionMarkerExists()
        {
            LayoutColumn(440f/956);var groups=HuntEdictHudRows.Groups("combat",store.Data,store.Data.Hero);
            Set("sheetTab","combat");Set("positionList",false);Set("sheetGroup",groups.ToList().FindIndex(g=>g.ids.Contains("common.order")));Call("BuildEdictSheet");
            var notes=(System.Collections.Generic.List<Text>)typeof(GameUI).GetField("orderDescriptions",Access).GetValue(ui);var note=notes[0];
            var card=note.GetComponentInParent<HuntEdictOrderCard>();Assert.IsNotNull(card);var list=card.GetComponentInParent<HuntEdictOrderList>();
            float before=list.GetComponent<LayoutElement>().preferredHeight;long revision=store.Revision;
            var grip=new Vector2(Bounds(card).xMax-4,Bounds(card).center.y);
            var e=new UnityEngine.EventSystems.PointerEventData(null){position=grip,pressPosition=grip,button=UnityEngine.EventSystems.PointerEventData.InputButton.Left};
            bool ignored=UnityEngine.TestTools.LogAssert.ignoreFailingMessages;UnityEngine.TestTools.LogAssert.ignoreFailingMessages=true;
            try
            {
                card.OnBeginDrag(e);Assert.IsNotNull(list.transform.Find("Order insertion"));
                Assert.DoesNotThrow(()=>Call("SetOrderDescription",note,note.text+" · Refresh enhancement skills and buff potions before they expire"));
                list.Move(e);Assert.GreaterOrEqual(list.GetComponent<LayoutElement>().preferredHeight,before);
                Assert.AreEqual(revision,store.Revision,"A changing drag preview must not commit.");card.OnCancel(new UnityEngine.EventSystems.BaseEventData(null));
                Assert.IsFalse(HuntEdictOrderCard.Dragging);
            }
            finally{list.Cancel();UnityEngine.TestTools.LogAssert.ignoreFailingMessages=ignored;}
        }

        [TestCase(100f)][TestCase(140f)][TestCase(165f)]
        public void NarrowNumbersKeepPositiveSeparateControlsAndCommitTheDeclaredStep(float width)
        {
            Set("sheetInner",width);Set("positionUnit",1f);Set("sheetPosition",true);
            var card=UiLayout.Rect("Narrow number",host.transform);UiLayout.Place(card,0,0,width,160);
            var size=card.gameObject.AddComponent<LayoutElement>();size.preferredHeight=40;
            var definition=EdictOptions.Global.Single(d=>d.id=="position.engageDistance");
            double start=definition.min+2*definition.step;string latest=null;
            string Format(double value)=>value.ToString("0.###",System.Globalization.CultureInfo.InvariantCulture);
            Call("AddSheetNumber",card,new HuntEdictHudRows.Row{definition=definition,value=Format(start),editable=true},new System.Action<string,string>((id,value)=>latest=value));
            Call("AddSheetNumberBounds",card,width,1f,"0% Do not avoid","100% Always avoid");
            card.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,size.preferredHeight);
            var slider=card.GetComponentInChildren<Slider>();
            var controls=card.GetComponentsInChildren<Button>().Select(b=>(RectTransform)b.transform).Concat(new[]{(RectTransform)slider.transform,card.GetComponentsInChildren<Text>().Single(t=>t.name=="edict-sheet-value-"+definition.id).rectTransform}).ToArray();
            foreach(var control in controls)
            {
                Assert.Greater(control.rect.width,0,control.name);Assert.Greater(control.rect.height,0,control.name);
                Assert.IsTrue(Bounds(card).Contains(Bounds(control).min+Vector2.one*.01f)&&Bounds(card).Contains(Bounds(control).max-Vector2.one*.01f),control.name);
            }
            for(int i=0;i<controls.Length;i++)for(int j=i+1;j<controls.Length;j++)Assert.IsFalse(Bounds(controls[i]).Overlaps(Bounds(controls[j])),controls[i].name+" / "+controls[j].name);
            card.GetComponentsInChildren<Button>().Single(b=>b.name=="edict-sheet-plus-"+definition.id).onClick.Invoke();Assert.AreEqual(Format(start+definition.step),latest);
            latest=null;slider.value+=1;
            UnityEngine.EventSystems.ExecuteEvents.Execute(slider.gameObject,new UnityEngine.EventSystems.BaseEventData(null),UnityEngine.EventSystems.ExecuteEvents.submitHandler);
            Assert.AreEqual(Format(start+definition.step),latest);
        }

        [TestCase("ko",.5f)][TestCase("ko",1f)][TestCase("ko",2f)]
        [TestCase("en",.5f)][TestCase("en",1f)][TestCase("en",2f)]
        public void PortraitPositionModesFitTheirContentsAndExposeTheRestoredDirection(string language,float scale)
        {
            if(language=="en")Loc.Use("en",LocalizationTable.Parse(Resources.Load<TextAsset>("Localization/en").text).Entries);
            UiSafeArea.AspectRatio=440f/956;Set("quickScale",UiSafeArea.Current.width/440*scale);LayoutColumn(440f/956);
            Set("sheetTab","combat");Set("positionList",false);Set("sheetGroup",HuntEdictHudRows.Groups("combat",store.Data,store.Data.Hero).ToList().FindIndex(g=>g.ids.Contains("position.engage")));
            Call("BuildEdictSheet");
            Assert.IsTrue((bool)typeof(GameUI).GetField("sheetPosition",Access).GetValue(ui),"The fixture must open the dedicated position editor.");
            bool ignored=UnityEngine.TestTools.LogAssert.ignoreFailingMessages;UnityEngine.TestTools.LogAssert.ignoreFailingMessages=true;
            try
            {
                foreach(bool opening in new[]{true,false})foreach(string mode in opening?new[]{"APPROACH","STAND","DISTANCE","LURE"}:new[]{"CENTER","STAND","DISTANCE","EDGE"})
                {
                    var edit=new HuntEdictEditSession(store.Data.Hero);string key=opening?"position.engage":"position.mode";
                    edit.ReplaceDraft(HuntEdictQuickMenu.With(edit.Draft,new System.Collections.Generic.Dictionary<string,string>{{key,mode},{"position.engageDistanceSource","EXPLICIT"},{"position.distanceSource","EXPLICIT"}}));
                    Assert.IsTrue(store.CommitHuntEdict(edit,catalog),store.Error);Assert.AreEqual(mode,HuntEdictSummary.Value(HuntEdictLoadout.FromHero(store.Data.Hero).edict,key));
                    Set("positionOpening",opening);Call("RebuildEdictSheet");var scroll=(ScrollRect)typeof(GameUI).GetField("sheetScroll",Access).GetValue(ui);
                    // Deferred Destroy leaves inactive same-name cards in this synchronous fixture.
                    var card=(RectTransform)typeof(GameUI).GetField("sheetCard",Access).GetValue(ui);
                    Assert.IsTrue(card.gameObject.activeInHierarchy);Assert.IsTrue(scroll.transform.IsChildOf(card));float current=card.rect.height;
                    var fit=scroll.GetComponent<EdictPopupLayout>();Assert.IsNotNull(fit);
                    float chrome=(float)typeof(EdictPopupLayout).GetField("chrome",Access).GetValue(fit),maximum=(float)typeof(EdictPopupLayout).GetField("maximum",Access).GetValue(fit);
                    Assert.AreEqual(Mathf.Min(maximum,chrome+scroll.content.rect.height),current,.5f,"Mode changes must refit the contents up to the safe-area limit.");
                    string measure=language+" "+mode+" scale="+scale+" safe="+UiSafeArea.Current+" panel="+card.rect+" unit="+typeof(GameUI).GetField("positionUnit",Access).GetValue(ui)+" canvas="+typeof(GameUI).GetField("quickScale",Access).GetValue(ui)+" anchor="+typeof(GameUI).GetField("quickAnchor",Access).GetValue(ui)+" body="+scroll.content.rect+" viewport="+scroll.viewport.rect;
                    bool overflow=scroll.content.rect.height>scroll.viewport.rect.height+.01f;
                    Assert.AreEqual(overflow,scroll.vertical,measure);Assert.AreEqual(overflow,scroll.verticalScrollbar.gameObject.activeSelf,measure);
                    if(!opening)
                    {
                        Assert.IsTrue(quick.GetComponentsInChildren<Button>().Any(b=>b.name=="edict-sheet-choice-position.clockwise-ON"));
                        Assert.IsTrue(quick.GetComponentsInChildren<Button>().Any(b=>b.name=="edict-sheet-choice-position.clockwise-OFF"));
                    }
                }
            }
            finally{UnityEngine.TestTools.LogAssert.ignoreFailingMessages=ignored;}
        }

        [TestCase("ko")][TestCase("en")]
        public void PositionChoicesImmediatelyGrowAndShrinkTheSamePopupWithoutAFrameTick(string language)
        {
            if(language=="en")Loc.Use("en",LocalizationTable.Parse(Resources.Load<TextAsset>("Localization/en").text).Entries);
            var edit=new HuntEdictEditSession(store.Data.Hero);
            edit.ReplaceDraft(HuntEdictQuickMenu.With(edit.Draft,new System.Collections.Generic.Dictionary<string,string>{{"position.engage","STAND"},{"position.engageDistanceSource","EXPLICIT"}}));
            Assert.IsTrue(store.CommitHuntEdict(edit,catalog),store.Error);
            LayoutColumn(440f/956);Set("sheetTab","combat");Set("positionList",false);Set("positionOpening",true);
            Set("sheetGroup",HuntEdictHudRows.Groups("combat",store.Data,store.Data.Hero).ToList().FindIndex(g=>g.ids.Contains("position.engage")));Call("BuildEdictSheet");
            var card=(RectTransform)typeof(GameUI).GetField("sheetCard",Access).GetValue(ui);
            var scroll=(ScrollRect)typeof(GameUI).GetField("sheetScroll",Access).GetValue(ui);float shortHeight=card.rect.height;
            foreach(string mode in new[]{"DISTANCE","STAND","LURE","STAND","APPROACH","STAND"})
            {
                var button=card.GetComponentsInChildren<Button>().Single(b=>b.name=="edict-sheet-choice-position.engage-"+mode);
                if(mode=="STAND")scroll.content.anchoredPosition=new Vector2(0,50);
                Quiet(()=>button.onClick.Invoke());Assert.AreEqual(mode,HuntEdictSummary.Value(HuntEdictLoadout.FromHero(store.Data.Hero).edict,"position.engage"));
                Assert.AreSame(card,typeof(GameUI).GetField("sheetCard",Access).GetValue(ui));Assert.AreSame(scroll,typeof(GameUI).GetField("sheetScroll",Access).GetValue(ui));
                if(mode=="STAND")Assert.AreEqual(shortHeight,card.rect.height,.5f,"Removing the extra rows must shrink the current popup immediately.");
                else Assert.Greater(card.rect.height,shortHeight,"Adding option rows must grow the current popup immediately.");
                bool overflow=scroll.content.rect.height>scroll.viewport.rect.height+.01f;
                Assert.AreEqual(overflow,scroll.vertical);Assert.AreEqual(overflow,scroll.verticalScrollbar.gameObject.activeSelf);
                Assert.AreEqual(0,scroll.content.anchoredPosition.y,.01f,"Shrinking must clamp any obsolete scroll offset.");
            }
        }

        [TestCase(false,false)][TestCase(false,true)][TestCase(true,false)][TestCase(true,true)]
        public void PointerReorderPacksUnequalRowsAndCommitsOnlyOnDrop(bool downward,bool cancel)
        {
            var overlay=(RectTransform)quick.transform;overlay.sizeDelta=new Vector2(300,600);
            var listRect=UiLayout.Rect("Variable rows",overlay);UiLayout.Place(listRect,0,0,250,500);
            var layout=listRect.gameObject.AddComponent<VerticalLayoutGroup>();layout.spacing=5;layout.childControlHeight=layout.childControlWidth=true;layout.childForceExpandHeight=false;
            string moved=null;int destination=-1;var list=listRect.gameObject.AddComponent<HuntEdictOrderList>();
            list.Initialize(null,overlay,(id,to)=>{moved=id;destination=to;});
            var cards=new HuntEdictOrderCard[3];float[] heights={48,132,76};
            for(int i=0;i<cards.Length;i++)
            {
                var row=UiLayout.Rect("Row "+i,listRect);row.gameObject.AddComponent<Image>();row.gameObject.AddComponent<LayoutElement>().preferredHeight=heights[i];
                cards[i]=row.gameObject.AddComponent<HuntEdictOrderCard>();cards[i].Initialize(list,i.ToString(),i);list.Add(cards[i]);
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(listRect);var original=cards.Select(c=>c.Rect.anchoredPosition).ToArray();
            var source=cards[downward?0:2];var target=cards[downward?2:0];
            var e=new UnityEngine.EventSystems.PointerEventData(null){position=Bounds(source).center,button=UnityEngine.EventSystems.PointerEventData.InputButton.Left};
            bool ignored=UnityEngine.TestTools.LogAssert.ignoreFailingMessages;UnityEngine.TestTools.LogAssert.ignoreFailingMessages=true;
            try
            {
                source.OnBeginDrag(e);e.position=Bounds(target).center;source.OnDrag(e);
                Assert.IsNull(moved,"A preview must not commit.");
                var rows=cards.Select(Bounds).OrderByDescending(r=>r.yMax).ToArray();
                for(int i=1;i<rows.Length;i++)Assert.AreEqual(5,rows[i-1].yMin-rows[i].yMax,.01f,"Unequal rows must retain the layout gap.");
                Assert.AreEqual(Bounds(source).center,Bounds(listRect.Find("Order insertion")).center,"The insertion marker must follow the draft slot.");
                if(cancel)source.OnCancel(new UnityEngine.EventSystems.BaseEventData(null));else source.OnEndDrag(e);
                Assert.AreEqual(cancel?null:source.Id,moved);if(!cancel)Assert.AreEqual(target.Index,destination);
                for(int i=0;i<cards.Length;i++)Assert.AreEqual(original[i],cards[i].Rect.anchoredPosition,"Ending a draft must return positions to the layout.");
                Assert.IsTrue(layout.enabled);Assert.IsFalse(HuntEdictOrderCard.Dragging);
            }
            finally{UnityEngine.TestTools.LogAssert.ignoreFailingMessages=ignored;}
        }

        [TestCase("explore")][TestCase("combat")]
        public void OtherNarrowOrderDescriptionsStayComplete(string tab)
        {
            store.Data.Hero.level=40;store.Data.Hero.highestClear=20;ContentUnlocks.Reconcile(store.Data);LayoutColumn(440f/956);
            Set("sheetTab",tab);Set("positionList",false);
            if(tab=="combat")Set("sheetGroup",HuntEdictHudRows.Groups(tab,store.Data,store.Data.Hero).ToList().FindIndex(g=>g.ids.Contains("target.priorityGrade")));
            Call("BuildEdictSheet");LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)typeof(GameUI).GetField("sheetContent",Access).GetValue(ui));
            var notes=quick.GetComponentsInChildren<Text>().Where(t=>t.name=="Order description"&&!string.IsNullOrEmpty(t.text)).ToArray();Assert.IsNotEmpty(notes);
            foreach(var note in notes)
            {
                StringAssert.DoesNotContain("…",note.text);Assert.AreEqual(HorizontalWrapMode.Wrap,note.horizontalOverflow);Assert.LessOrEqual(note.preferredHeight,note.rectTransform.rect.height+.1f);
                var card=(RectTransform)note.transform.parent;Assert.IsFalse(Bounds(note).Overlaps(Bounds(card.Find("Order header"))));
                Assert.IsTrue(Bounds(card).Contains(Bounds(note).min+Vector2.one*.01f)&&Bounds(card).Contains(Bounds(note).max-Vector2.one*.01f));
            }
        }
        [Test] public void PresetEntryOpensTheStandalonePageWithoutChangingSavedSettings()
        {
            string before=JsonUtility.ToJson(store.Data.Hero);Quiet(()=>Call("OpenEdictSheet","presets"));
            Assert.IsNotNull(ui.PresetsWindow);Assert.AreEqual("edict-presets-window",ui.PresetsWindow.View.Frame.name);
            Assert.AreEqual(before,JsonUtility.ToJson(store.Data.Hero));
            Quiet(ui.PresetsWindow.RequestClose);
        }
        [TestCase("F05",true)][TestCase("E03",true)][TestCase("E05",true)][TestCase("E08",true)]
        [TestCase("H10",true)][TestCase("H11",true)][TestCase("E18B",true)]
        [TestCase("E02",true)][TestCase("E07",true)][TestCase("E09",true)]
        [TestCase("E12",true)][TestCase("E14",true)][TestCase("E18",true)]
        [TestCase("E18C",true)][TestCase("E12S",true)][TestCase("H09",false)]
        public void JournalOffersPracticeOnlyWhenTheNewUiHasThatSetting(string guide,bool available)
            =>Assert.AreEqual(available,Call("EdictPracticeAvailable",guide));

        [TestCase("combat","position.surrounded")][TestCase("combat","target.switch")]
        [TestCase("survival","survival.lowHp")][TestCase("survival","survival.potion")][TestCase("survival","survival.defenseSkill")]
        [TestCase("loot","loot.NORMAL")][TestCase("loot","loot.class")][TestCase("loot","loot.gold")][TestCase("loot","loot.distance")]
        [TestCase("bag","bag.trigger")][TestCase("bag","bag.cleanupAt")][TestCase("bag","bag.protectEnhanced")][TestCase("bag","bag.warehouseFull")]
        public void EachMovedCatalogGroupIsReachableAndItsControlsCommitTheExistingEdict(string tab,string member)
        {
            LayoutColumn(956f/440);Quiet(()=>Call("OpenEdictSheet",tab));
            var groups=HuntEdictHudRows.Groups(tab,store.Data,store.Data.Hero);int group=groups.ToList().FindIndex(g=>g.ids.Contains(member));
            Button Find(string name)=>quick.GetComponentsInChildren<Button>().Single(b=>b.name==name&&b.gameObject.activeInHierarchy);
            Quiet(()=>Find("edict-sheet-group-"+group).onClick.Invoke());
            var edict=HuntEdictLoadout.FromHero(store.Data.Hero).edict;
            var rows=HuntEdictHudRows.Rows(groups[group],edict,store.Data,store.Data.Hero);
            foreach(var row in rows)Assert.IsTrue(quick.GetComponentsInChildren<Transform>().Any(t=>t.name=="edict-sheet-row-"+row.definition.id),row.definition.id);
            var option=rows.First(r=>r.editable&&(r.definition.kind==EdictOptionKind.Toggle||r.definition.kind==EdictOptionKind.Choice));
            string next=HuntEdictHudRows.Choices(option.definition,edict).First(v=>v!=option.value);long revision=store.Revision;
            Quiet(()=>Find("edict-sheet-choice-"+option.definition.id+"-"+next).onClick.Invoke());
            Assert.AreEqual(revision+1,store.Revision);Assert.AreEqual(next,HuntEdictQuickMenu.Value(HuntEdictLoadout.FromHero(store.Data.Hero),option.definition.id));
            Quiet(()=>Find("edict-sheet-undo").onClick.Invoke());
            Assert.AreEqual(option.value,HuntEdictQuickMenu.Value(HuntEdictLoadout.FromHero(store.Data.Hero),option.definition.id));
        }
        [TestCase("ko")][TestCase("en")]
        public void RestoredPotionRowsUseBilingualNamesInsteadOfStorageIds(string language)
        {
            if(language=="en")Loc.Use("en",LocalizationTable.Parse(Resources.Load<TextAsset>("Localization/en").text).Entries);
            LayoutColumn(956f/440);Quiet(()=>Call("OpenEdictSheet","survival"));
            var group=HuntEdictHudRows.Groups("survival",store.Data,store.Data.Hero).ToList().FindIndex(g=>g.ids.Contains("potion.autoBuy"));
            Quiet(()=>quick.GetComponentsInChildren<Button>().Single(b=>b.name=="edict-sheet-group-"+group).onClick.Invoke());
            Assert.IsFalse(quick.GetComponentsInChildren<Transform>().Any(t=>t.name=="edict-sheet-row-potion.hpTarget"&&t.gameObject.activeInHierarchy),"Purchase quantities stay hidden while auto-buy is off.");
            long revision=store.Revision;
            Quiet(()=>quick.GetComponentsInChildren<Button>().Single(b=>b.name=="edict-sheet-choice-potion.autoBuy-ON"&&b.gameObject.activeInHierarchy).onClick.Invoke());
            Assert.AreEqual(revision+1,store.Revision);Assert.AreEqual("ON",HuntEdictSummary.Value(HuntEdictLoadout.FromHero(store.Data.Hero).edict,"potion.autoBuy"));
            foreach(var definition in PotionEdict.Options)
            {
                var row=quick.GetComponentsInChildren<Transform>().Single(t=>t.name=="edict-sheet-row-"+definition.id&&t.gameObject.activeInHierarchy);
                var label=row.GetComponentsInChildren<Text>().Single(t=>t.name=="Title");Assert.AreEqual(Loc.T(definition.label),label.text);
                Assert.IsFalse(label.text.StartsWith("potion.",System.StringComparison.Ordinal));
                if(language=="en")Assert.IsFalse(label.text.Any(c=>c>='가'&&c<='힣'),label.text);
            }
        }
        [Test] public void TheCombatStyleLessonOpensTheQuickPresetPageBeforeSlotUnlock()
        {
            store.Data.guide.edictLegacyAccess=false;store.Data.guide.edictUnlocks.Remove(HuntEdictProgression.Presets);store.Data.guide.edictUnlocks.Remove(HuntEdictProgression.Sharing);
            Quiet(()=>Call("StartTutorialAction",Tutorials.All.Single(d=>d.id=="E02")));
            Assert.IsNotNull(ui.PresetsWindow);
            Assert.IsTrue(ui.PresetsWindow.View.GetComponentsInChildren<Button>().Any(b=>b.name=="edict-quick-choice-balanced"));
            Assert.IsFalse(ui.PresetsWindow.View.GetComponentsInChildren<Button>().Single(b=>b.name=="edict-presets-tab-slots").interactable);
            Quiet(()=>ui.PresetsWindow.RequestClose());Assert.IsNull(ui.PresetsWindow);
            Assert.AreEqual("",typeof(GameUI).GetField("tutorialFocus",Access).GetValue(ui));
        }
        [Test] public void RestoredBossFightChoicesUseTheExistingEdictTransaction()
        {
            LayoutColumn(956f/440);Quiet(()=>Call("OpenEdictSheet","combat"));
            int group=HuntEdictHudRows.Groups("combat",store.Data,store.Data.Hero).ToList().FindIndex(g=>g.ids.Contains("target.bossFight"));
            Quiet(()=>quick.GetComponentsInChildren<Button>().Single(b=>b.name=="edict-sheet-group-"+group).onClick.Invoke());
            Quiet(()=>quick.GetComponentsInChildren<Button>().Single(b=>b.name=="edict-sheet-choice-target.bossFight-ADDS").onClick.Invoke());
            Assert.AreEqual("ADDS",HuntEdictQuickMenu.Value(HuntEdictLoadout.FromHero(store.Data.Hero),"target.bossFight"));
        }
    }
}
