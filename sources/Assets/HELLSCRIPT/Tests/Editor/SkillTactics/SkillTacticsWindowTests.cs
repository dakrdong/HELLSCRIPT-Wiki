using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Hellscript.Tests
{
    public sealed class SkillTacticsWindowTests
    {
        GameObject root;GameCatalog catalog;GameStore store;string directory;int closed;
        HeroSave Hero=>store.Data.Hero;
        [SetUp] public void Setup()
        {
            closed=0;Loc.UseSource();root=new GameObject("Tactics window fixture",typeof(RectTransform),typeof(ContentWindowHost));
            catalog=ScriptableObject.CreateInstance<GameCatalog>();catalog.Populate();
            directory=Path.Combine(Path.GetTempPath(),"skill-tactics-window-"+Guid.NewGuid().ToString("N"));
            store=new GameStore(directory,catalog);ContentTestAccounts.ReleasedAccess(store.Data);Configure("W02",true);Assert.IsTrue(store.Save(),store.Error);
        }
        [TearDown] public void Cleanup()
        {
            UnityEngine.Object.DestroyImmediate(root);UnityEngine.Object.DestroyImmediate(catalog);
            if(Directory.Exists(directory))Directory.Delete(directory,true);Loc.UseSource();
        }
        void Configure(string parent,bool modifier)
        {
            int cls="WAM".IndexOf(parent[0]);store.Data.selectedHero=store.Data.heroes.FindIndex(h=>(int)h.heroClass==cls&&h.gender==HeroGender.Male);Hero.level=60;
            string[] skills=cls==0?new[]{"W09","W01","W05","W06","W02","W03","W04"}:cls==1?new[]{"A07","A01","A02","A03"}:new[]{"M07","M01","M02","M03","M04","M06","M08","M11"};
            var empty=SkillTreeD4Loadout.Empty(Hero.heroClass);empty.legacyEdict=Hero.edict.Copy();empty.ProjectLegacy(Hero.build,catalog);Hero.edict=empty.ProjectEdict();
            var load=ContentTestAccounts.OwnSkills(Hero,catalog,skills);if(modifier)load=ClassSkillTree.ChangeRank(load,Hero,parent+"-3",1);
            var others=skills.Where(id=>id!=parent).Take(3).ToArray();load=ClassSkillTree.SetSlots(load,Hero,new[]{parent,others[0],others[1],others[2],""});
            load.ProjectLegacy(Hero.build,catalog);Hero.edict=load.ProjectEdict();Hero.useEdict=true;
        }
        SkillTreeD4Window Tree(string parent="W02",string orient="portrait",string modifier=null,CombatSimulation simulation=null)
        {
            var tree=SkillTreeD4Window.Open(root.transform,new StoreSkillTreeD4Host(store,catalog,simulation),hero:Hero.heroClass.ToString(),store:store,previewOrient:orient,closed:()=>closed++);
            Select(tree,parent,modifier);return tree;
        }
        static void Select(SkillTreeD4Window tree,string parent,string modifier=null)
        {
            var actives=SkillTreeD4.Model.Classes[tree.Session.Hero].Actives;int index=Array.FindIndex(actives,a=>a.Id==parent);
            tree.Select(modifier==null?SkillTreeD4Selection.Active(index):SkillTreeD4Selection.ModifierOf(index,Array.FindIndex(actives[index].Modifiers,m=>m.Id==modifier)));
        }
        static Button Button(Component owner,string name)=>owner.GetComponentsInChildren<Button>(true).Single(b=>b.name==name);
        static RectTransform Named(Component owner,string name)=>owner.GetComponentsInChildren<RectTransform>(true).Single(r=>r.name==name);
        static string Texts(Component owner)=>string.Join("\n",owner.GetComponentsInChildren<Text>(true).Select(t=>t.text));
        static string[] Rows(SkillTacticsWindow window)=>window.View.GetComponentsInChildren<RectTransform>(true).Where(r=>r.name.StartsWith("tactics-row-",StringComparison.Ordinal)).Select(r=>r.name.Substring(12)).ToArray();
        static Rect Bounds(RectTransform rect){var points=new Vector3[4];rect.GetWorldCorners(points);return Rect.MinMaxRect(points[0].x,points[0].y,points[2].x,points[2].y);}
        [Test] public void PlayerEntryAddsNoScrollOrPanelHeightAndPassivesHaveNoTacticsButton()
        {
            var normal=SkillTreeD4Window.Open(root.transform,new StoreSkillTreeD4Host(store,catalog,canEdit:()=>false),store:store,previewOrient:"portrait");Select(normal,"W02");
            Assert.IsFalse(normal.TacticsEntryVisible);Assert.IsFalse(normal.OpenTactics());
            Vector2 panelSize=normal.Panel.rect.size;float header=Named(normal,"PanelHeader").rect.height;
            var tree=Tree();Assert.IsTrue(tree.TacticsEntryVisible);Assert.AreEqual(panelSize,tree.Panel.rect.size);Assert.AreEqual(header,Named(tree,"PanelHeader").rect.height);
            Assert.AreEqual(1,tree.GetComponentsInChildren<ScrollRect>(true).Length);
            var entry=Named(tree,"d4-tactics-entry");Assert.AreEqual(new Vector2(44,44),entry.rect.size);
            Assert.IsNotNull(entry.GetComponentsInChildren<Image>().FirstOrDefault(i=>i.sprite!=null&&i.sprite.name.StartsWith("skill-edict-medallion",StringComparison.Ordinal)));
            Assert.IsFalse(tree.GetComponentsInChildren<RectTransform>(true).Any(r=>r.name.StartsWith("d4-tab-",StringComparison.Ordinal)&&r.gameObject.activeSelf));
            tree.Select(SkillTreeD4Selection.PassiveAt(0));Assert.IsFalse(tree.TacticsEntryVisible);Assert.IsFalse(tree.OpenTactics());
        }
        [TestCase("W02")][TestCase("A03")][TestCase("M04")]
        public void EveryActiveAndModifierOpensFromTheOrdinaryPlayerTree(string representative)
        {
            Configure(representative,true);var tree=Tree(representative);
            foreach(var active in SkillTreeD4.Model.Classes[tree.Session.Hero].Actives)
                foreach(string node in new[]{active.Id}.Concat(active.Modifiers.Select(m=>m.Id)))
                {
                    Select(tree,active.Id,node==active.Id?null:node);
                    Assert.IsTrue(tree.TacticsEntryVisible,node);Assert.IsTrue(tree.OpenTactics(),node);
                    Assert.AreEqual(node,tree.TacticsWindow.Session.NodeId);Assert.AreEqual(SkillTacticsHostKind.Player,tree.TacticsWindow.Session.Host);
                    tree.TacticsWindow.RequestLeave(SkillTacticsLeaveTarget.SkillTree);Assert.IsNull(tree.TacticsWindow,node);
                }
            foreach(var passive in SkillTreeD4.Model.Classes[tree.Session.Hero].Passives.Select((_,i)=>i))
            {tree.Select(SkillTreeD4Selection.PassiveAt(passive));Assert.IsFalse(tree.TacticsEntryVisible);Assert.IsFalse(tree.OpenTactics());}
        }
        [Test] public void AReadOnlyTreeCannotBypassItsHostThroughTactics()
        {
            var tree=SkillTreeD4Window.Open(root.transform,new StoreSkillTreeD4Host(store,catalog,canEdit:()=>false),store:store,previewOrient:"portrait");
            Select(tree,"W02");Assert.IsFalse(tree.TacticsEntryVisible);Assert.IsFalse(tree.OpenTactics());
        }
        [TestCase("W02",true,"portrait","ko",7)]
        [TestCase("A03",false,"landscape","ko",4)]
        [TestCase("A03",true,"landscape","en",3)]
        [TestCase("M04",true,"pc","en",7)]
        public void RepresentativesUseTheSameDataRowsAndReplacementWithoutChangingTheOwner(string parent,bool modifier,string orient,string locale,int count)
        {
            Configure(parent,modifier);Loc.Use(locale,LocalizationTable.Parse(Resources.Load<TextAsset>("Localization/en").text).Entries);
            var tree=Tree(parent,orient);
            // Reuse the physical preview plate even when its owner is offset from the default screen fit.
            tree.Frame.anchoredPosition+=new Vector2(17,-23);Canvas.ForceUpdateCanvases();var parentBounds=Bounds(tree.Frame);
            string before=JsonUtility.ToJson(Hero);Assert.IsTrue(tree.OpenTactics());var window=tree.TacticsWindow;
            Assert.AreEqual(count,Rows(window).Length);Assert.AreEqual(before,JsonUtility.ToJson(Hero));StringAssert.DoesNotContain("HET-",Texts(window.View));
            if(parent=="A03"&&modifier)CollectionAssert.AreEqual(new[]{"auto","position","fireAssignment"},Rows(window));
            Assert.AreEqual(EquipmentViewSource.Draft,window.View.Source);Assert.Greater(window.Scroll.viewport.rect.height,44);
            Assert.AreEqual(parentBounds,Bounds(tree.Frame));var childBounds=Bounds(window.View.Frame);
            Assert.AreEqual(parentBounds.x,childBounds.x,.5);Assert.AreEqual(parentBounds.y,childBounds.y,.5);Assert.AreEqual(parentBounds.size,childBounds.size);
            foreach(var control in window.View.GetComponentsInChildren<Button>())Assert.GreaterOrEqual(control.GetComponent<RectTransform>().rect.height,44);
        }
        [Test] public void ACombatPolicySaveAppliesToTheOwnedLiveRunWithoutReplacingItOrItsCooldowns()
        {
            var simulation=new CombatSimulation(store.Data,catalog,1,seed:713);var run=simulation.State;store.Data.suspendedRun=run;run.paused=true;run.cooldowns[1]=3.5f;
            root.GetComponent<ContentWindowHost>().Initialize(()=>simulation.State);
            var tree=SkillTreeD4Window.Open(root.transform,new StoreSkillTreeD4Host(store,catalog,simulation),hero:Hero.heroClass.ToString(),store:store,previewOrient:"portrait");Select(tree,"W02");
            Assert.IsTrue(tree.OpenTactics());var window=tree.TacticsWindow;Assert.IsTrue(window.SetLegacy("HEC-W02-04","7"));Assert.IsTrue(window.Save(),store.Error);
            Assert.AreSame(run,simulation.State);Assert.AreSame(run,store.Data.suspendedRun);Assert.AreEqual(3.5f,run.cooldowns[1]);Assert.IsTrue(run.paused);
            Assert.AreEqual("7",HuntEdictV2.Value(run.build.classSkills.ProjectEdict(),"W02",4));
            var reloaded=new GameStore(directory,catalog);Assert.AreEqual("7",HuntEdictV2.Value(reloaded.Data.suspendedRun.build.classSkills.ProjectEdict(),"W02",4));
        }
        [TestCase(false)][TestCase(true)] public void ResettingAllSkillsLeavesBasicAttackControlsOnThePreviewPage(bool recommended)
        {
            var empty=SkillTreeD4Loadout.Empty(Hero.heroClass);empty.legacyEdict=Hero.edict.Copy();empty.ProjectLegacy(Hero.build,catalog);Hero.edict=empty.ProjectEdict();
            Hero.useRecommendedEdict=recommended;
            var tree=Tree();Assert.IsTrue(tree.OpenTactics());var window=tree.TacticsWindow;
            Assert.IsTrue(window.Session.Preview);Assert.IsFalse(Button(window.View,"edict-sheet-choice-W02-native").interactable);
            Assert.AreEqual(!recommended,Button(window.View,"edict-sheet-choice-HEC-WARRIOR-02-RESOURCE").interactable);
            if(recommended)
            {
                string before=JsonUtility.ToJson(Hero);Assert.IsFalse(window.Session.CanEditBasic);Assert.IsFalse(window.SetLegacy("HEC-WARRIOR-02","RESOURCE"));Assert.IsFalse(window.Save());
                Assert.AreEqual(before,JsonUtility.ToJson(Hero));StringAssert.Contains(SkillTacticsCatalog.Reason("recommendedDefaults"),Texts(window.View));return;
            }
            Assert.IsTrue(window.SetLegacy("HEC-WARRIOR-02","RESOURCE"));Assert.IsTrue(window.Save(),store.Error);
            Assert.AreEqual("RESOURCE",HuntEdictV2.Value(Hero.edict,"BASIC",2));Assert.IsTrue(Hero.build.classSkills.ranks.All(r=>r.rank==0));
        }
        [Test] public void ExistingCombatPoliciesHaveControlsAndTheSaveGuardIncludesThem()
        {
            var tree=Tree();Assert.IsTrue(tree.OpenTactics());var window=tree.TacticsWindow;
            foreach(var option in window.Session.UsageOptions)
                foreach(var choice in option.choices)Assert.IsNotNull(Button(window.View,"edict-sheet-choice-"+option.id+"-"+choice.id));
            foreach(var option in window.Session.LegacyOptions)Assert.IsNotNull(Named(window.View,"edict-sheet-row-"+option.id));
            Assert.IsTrue(window.SetLegacy("HEC-W02-04","7"));Assert.IsTrue(Button(window.View,"tactics-save").interactable);
            Assert.IsTrue(window.Save(),store.Error);Assert.AreEqual("7",HuntEdictV2.Value(Hero.build.classSkills.ProjectEdict(),"W02",4));
        }
        [TestCase(false,"ko")][TestCase(true,"ko")][TestCase(true,"en")]
        public void DefaultsLockOnlyTacticsAndLeavePointAndSlotEditingAvailable(bool recommended,string language)
        {
            Loc.Use(language,LocalizationTable.Parse(Resources.Load<TextAsset>("Localization/en").text).Entries);
            store.Data.guide.legacyExempt=true;Assert.IsTrue(store.SetRecommendedEdict(Hero.id,recommended,catalog),store.Error);
            var tree=Tree();var active=SkillTreeD4.For(Hero.heroClass).actives.First(a=>Hero.build.classSkills.Rank(a.id)<a.maxRank&&ClassSkillTree.Unlocked(Hero.build.classSkills,Hero.level,a.id));
            Select(tree,active.id);int rank=Hero.build.classSkills.Rank(active.id);Assert.IsTrue(tree.Invest());
            Assert.AreEqual(rank+1,Hero.build.classSkills.Rank(active.id));Assert.IsTrue(Tutorials.Record(store.Data,"H07").practiced);Select(tree,"W02");
            Assert.IsTrue(tree.TacticsEntryVisible);Assert.IsTrue(tree.OpenTactics());var window=tree.TacticsWindow;
            Assert.AreEqual(recommended,window.Session.Preview);Assert.AreEqual(!recommended,Button(window.View,"tactics-toggle-auto").interactable);
            Assert.AreEqual(!recommended,Button(window.View,"edict-sheet-choice-W02-native").interactable);
            Assert.AreEqual(!recommended,Button(window.View,"edict-sheet-choice-HEC-WARRIOR-02-RESOURCE").interactable);
            string before=JsonUtility.ToJson(Hero);
            Assert.AreEqual(!recommended,window.SetValue("auto","0"));Assert.AreEqual(!recommended,window.Save());
            if(recommended)
            {
                Assert.AreEqual(before,JsonUtility.ToJson(Hero));Assert.IsFalse(Button(window.View,"tactics-save").interactable);
                StringAssert.Contains(SkillTacticsCatalog.Reason("recommendedDefaults"),Texts(window.View));
                Assert.AreEqual("i",Named(tree,"Saved automatic state").GetComponent<Text>().text);
            }
            window.RequestLeave(SkillTacticsLeaveTarget.SkillTree);
            Named(tree,"d4-slot-0").GetComponentInChildren<Button>().onClick.Invoke();Named(tree,"d4-card-W03").GetComponentInChildren<Button>().onClick.Invoke();
            Assert.IsTrue(tree.Slots.Dirty);tree.Close();Assert.IsTrue(Hero.build.classSkills.Equipped("W03"));Assert.AreEqual(recommended,Hero.useRecommendedEdict);
        }
        [Test] public void AnOpenTacticsPageTracksModeChangesAndKeepsItsDraft()
        {
            var tree=Tree();tree.OpenTactics();var window=tree.TacticsWindow;window.SetValue("minimumDistance","6");
            var binding=window.View.GetComponent<StoreViewBinding>();Assert.IsNotNull(binding);
            var tick=typeof(StoreViewBinding).GetMethod("LateUpdate",BindingFlags.Instance|BindingFlags.NonPublic);
            Assert.IsTrue(store.SetRecommendedEdict(Hero.id,true,catalog),store.Error);tick.Invoke(binding,null);
            Assert.IsTrue(window.Session.Preview);Assert.IsFalse(Button(window.View,"tactics-save").interactable);
            StringAssert.Contains(SkillTacticsCatalog.Reason("recommendedDefaults"),Texts(window.View));
            Assert.AreEqual("6",window.Session.Value("minimumDistance"));Assert.IsTrue(window.Session.Dirty);
            window.RequestLeave(SkillTacticsLeaveTarget.SkillTree);Assert.IsTrue(window.HasDialog);
            Assert.IsFalse(Button(root.GetComponent<RectTransform>(),"tactics-leave-Save").interactable);
            Assert.IsFalse(window.ResolveLeave(SkillTacticsLeaveChoice.ContinueEditing));
            Assert.IsTrue(store.SetRecommendedEdict(Hero.id,false,catalog),store.Error);tick.Invoke(binding,null);
            Assert.IsFalse(window.Session.Preview);Assert.IsTrue(Button(window.View,"tactics-save").interactable);Assert.IsTrue(window.Save(),store.Error);
        }
        [TestCase("ko")][TestCase("en")]
        public void PresetModeChipRoundTripsAndRefreshesTheListTacticsAndLivePolicy(string language)
        {
            Loc.Use(language,LocalizationTable.Parse(Resources.Load<TextAsset>("Localization/en").text).Entries);
            store.Data.guide.edictLegacyAccess=true;HuntEdictProgression.Reconcile(store.Data);
            var load=Hero.build.classSkills.Copy();load.legacyEdict=HuntEdictV2Editing.WithGlobal(load.legacyEdict,"survival.potionHpPercent","91");
            load.ProjectLegacy(Hero.build,catalog);Hero.edict=load.ProjectEdict();Assert.IsTrue(store.SetRecommendedEdict(Hero.id,true,catalog),store.Error);
            var simulation=new CombatSimulation(store.Data,catalog,1,seed:2163);var run=simulation.State;store.Data.suspendedRun=run;
            run.paused=true;run.cooldowns[1]=3.5f;Assert.IsTrue(store.Save(),store.Error);float defaultThreshold=simulation.Policy.potionThreshold;
            root.GetComponent<ContentWindowHost>().Initialize(()=>run);var tree=Tree(simulation:simulation);Assert.IsTrue(tree.OpenTactics());var tactics=tree.TacticsWindow;
            string edict=JsonUtility.ToJson(Hero.edict),skills=JsonUtility.ToJson(Hero.build.classSkills),presets=StoreViewBinding.Key(Hero.edictPresets);int selectedPreset=Hero.selectedEdictPreset;
            foreach(bool recommended in new[]{false,true})
            {
                var popup=EdictPresetsWindow.Open(root.transform,store,catalog,Hero,()=>1,edit=>store.CommitHuntEdict(edit,catalog,simulation,true),()=>store.Error,null,simulation:simulation);
                popup.ShowQuickPresets();var toggle=Button(popup.View,"edict-default-settings");Assert.IsTrue(toggle.IsInteractable());long revision=store.Revision;
                toggle.onClick.Invoke();Assert.AreEqual(revision+1,store.Revision);Assert.AreEqual(recommended,Hero.useRecommendedEdict);
                Assert.IsTrue(Button(popup.View,"edict-default-settings").IsInteractable());
                Assert.AreEqual(!recommended,popup.View.GetComponentsInChildren<Button>().First(b=>b.name.StartsWith("edict-quick-choice-",StringComparison.Ordinal)).IsInteractable());
                Assert.AreEqual(recommended,new GameStore(directory,catalog).Data.Hero.useRecommendedEdict);
                Assert.AreEqual(recommended?defaultThreshold:91,simulation.Policy.potionThreshold);
                Assert.AreSame(run,simulation.State);Assert.AreSame(run,store.Data.suspendedRun);Assert.IsTrue(run.paused);Assert.AreEqual(3.5f,run.cooldowns[1]);
                ContentWindowHost.Detach(popup.View);UnityEngine.Object.DestroyImmediate(popup.View.gameObject);
                typeof(StoreViewBinding).GetMethod("LateUpdate",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(tactics.View.GetComponent<StoreViewBinding>(),null);
                Assert.AreEqual(recommended,tactics.Session.Preview);Assert.AreEqual(!recommended,Button(tactics.View,"tactics-toggle-auto").IsInteractable());
                if(recommended)StringAssert.Contains(SkillTacticsCatalog.Reason("recommendedDefaults"),Texts(tactics.View));
            }
            Assert.AreEqual(edict,JsonUtility.ToJson(Hero.edict));Assert.AreEqual(skills,JsonUtility.ToJson(Hero.build.classSkills));
            Assert.AreEqual(presets,StoreViewBinding.Key(Hero.edictPresets));Assert.AreEqual(selectedPreset,Hero.selectedEdictPreset);
        }
        [Test] public void AutomaticOffKeepsValuesAndAStoredSummaryUpdatesOnlyAfterSaveWithoutRebuildingControls()
        {
            var tree=Tree();Assert.IsTrue(tree.OpenTactics());var window=tree.TacticsWindow;
            var save=Button(window.View,"tactics-save");int builds=window.LayoutBuilds;string before=JsonUtility.ToJson(Hero.build.classSkills);
            Assert.IsTrue(window.SetValue("auto","0"));Assert.IsTrue(window.SetValue("minimumDistance","6"));Assert.IsTrue(window.SetValue("mission","attack"));
            Assert.AreEqual(before,JsonUtility.ToJson(Hero.build.classSkills));Assert.IsTrue(window.Save(),store.Error);Assert.IsFalse(Hero.build.classSkills.automatic.Contains("W02"));
            Assert.AreSame(save,Button(window.View,"tactics-save"));Assert.AreEqual(builds,window.LayoutBuilds);
            Assert.IsTrue(store.Save());tree.Tick(Time.unscaledTime+1);Assert.AreSame(save,Button(window.View,"tactics-save"));Assert.AreEqual(builds,window.LayoutBuilds);
            window.RequestLeave(SkillTacticsLeaveTarget.SkillTree);Assert.IsNull(tree.TacticsWindow);
            StringAssert.Contains("공격 진입",Named(tree,"d4-tactics-entry").GetComponentInChildren<Button>().name);
            Assert.AreEqual("−",Named(tree,"Saved automatic state").GetComponent<Text>().text);Assert.IsTrue(tree.OpenTactics());
            Assert.AreEqual("0",tree.TacticsWindow.Session.Value("auto"));Assert.AreEqual("6",tree.TacticsWindow.Session.Value("minimumDistance"));
        }
        [TestCase(false)][TestCase(true)]
        public void LiveTacticsSaveUpdatesAutomaticPolicyWithoutReplacingTheOwnedRun(bool failFirstWrite)
        {
            Hero.useRecommendedEdict=false;var sim=new CombatSimulation(store.Data,catalog,1,seed:2161);var run=sim.State;
            store.Data.suspendedRun=run;Assert.IsTrue(store.Save(),store.Error);Assert.Contains("W09",run.build.classSkills.automatic);
            root.GetComponent<ContentWindowHost>().Initialize(()=>run);
            var tree=Tree("W09",simulation:sim);Assert.IsTrue(tree.OpenTactics());var window=tree.TacticsWindow;
            Assert.IsTrue(window.SetValue("auto","0"));string path=Path.Combine(directory,"hellscript-local-v1.json");
            if(failFirstWrite)
            {
                string before=JsonUtility.ToJson(store.Data),disk=File.ReadAllText(path);Directory.CreateDirectory(path+".tmp");
                LogAssert.Expect(LogType.Error,new Regex(".*"));Assert.IsFalse(window.Save());
                Assert.AreEqual(before,JsonUtility.ToJson(store.Data));Assert.AreEqual(disk,File.ReadAllText(path));
                Assert.AreSame(run,store.Data.suspendedRun);Assert.IsTrue(window.Session.Dirty);Directory.Delete(path+".tmp");
            }
            Assert.IsTrue(window.Save(),store.Error);Assert.AreSame(run,sim.State);Assert.AreSame(run,store.Data.suspendedRun);
            CollectionAssert.DoesNotContain(Hero.build.classSkills.automatic,"W09");CollectionAssert.DoesNotContain(run.build.classSkills.automatic,"W09");
            Assert.IsTrue(run.paused);Assert.IsFalse(window.Session.Dirty);
            var loaded=new GameStore(directory,catalog);CollectionAssert.DoesNotContain(loaded.Data.Hero.build.classSkills.automatic,"W09");
            CollectionAssert.DoesNotContain(loaded.Data.suspendedRun.build.classSkills.automatic,"W09");
            window.RequestLeave(SkillTacticsLeaveTarget.SkillTree);tree.Close();Assert.IsFalse(run.paused);
            Assert.AreEqual("AUTO_OFF",sim.InspectClassSkill("W09",aim:run.position,automatic:true).reason);
            Assert.IsTrue(store.CommitRunMutation(run,Guid.NewGuid().ToString("N"),"tactics-owned-run-check",snapshot=>{snapshot.paused=true;return true;}),store.Error);
            Assert.AreSame(run,store.Data.suspendedRun);Assert.AreSame(run,sim.State);
        }
        [TestCase(false)][TestCase(true)]
        public void RelocatedGlobalEditsRefreshTheLivePolicyOnlyAfterTheWriteSucceeds(bool recommended)
        {
            var load=Hero.build.classSkills.Copy();
            load.legacyEdict=HuntEdictV2Editing.WithGlobal(load.legacyEdict,"survival.potionHpPercent","11");
            load.ProjectLegacy(Hero.build,catalog);Hero.edict=load.ProjectEdict();Hero.useRecommendedEdict=recommended;
            var sim=new CombatSimulation(store.Data,catalog,1,seed:2162);var run=sim.State;
            store.Data.suspendedRun=run;run.paused=true;run.cooldowns[1]=3.5f;Assert.IsTrue(store.Save(),store.Error);
            var policy=sim.Policy;var section=new EdictSettingsSection(store,catalog,"survival",sim);
            string path=Path.Combine(directory,"hellscript-local-v1.json"),disk=File.ReadAllText(path),before=JsonUtility.ToJson(store.Data);
            Directory.CreateDirectory(path+".tmp");LogAssert.Expect(LogType.Error,new Regex(".*"));Assert.IsFalse(section.Apply("survival.potionHpPercent","91"));
            Assert.AreEqual(before,JsonUtility.ToJson(store.Data));Assert.AreEqual(disk,File.ReadAllText(path));Assert.AreSame(policy,sim.Policy);
            Directory.Delete(path+".tmp");Assert.IsTrue(section.Apply("survival.potionHpPercent","91"),store.Error);
            Assert.AreEqual(recommended?policy.potionThreshold:91,sim.Policy.potionThreshold);
            Assert.AreSame(run,sim.State);Assert.AreSame(run,store.Data.suspendedRun);Assert.IsTrue(run.paused);Assert.AreEqual(3.5f,run.cooldowns[1]);
            var reloaded=new GameStore(directory,catalog);Assert.AreEqual(recommended,reloaded.Data.Hero.useRecommendedEdict);
            Assert.AreEqual("91",HuntEdictSummary.Value(reloaded.Data.Hero.edict,"survival.potionHpPercent"));
            Assert.AreEqual("91",HuntEdictSummary.Value(reloaded.Data.suspendedRun.build.classSkills.ProjectEdict(),"survival.potionHpPercent"));
        }
        [Test] public void NumericEndpointsUndoAndActualReferenceReasonsUseTheSharedSession()
        {
            var tree=Tree();tree.OpenTactics();var window=tree.TacticsWindow;
            window.SetValue("minimumDistance","2");Assert.IsFalse(Button(window.View,"tactics-minus-minimumDistance").interactable);Assert.IsFalse(window.Step("minimumDistance",-1));
            window.SetValue("minimumDistance","8");Assert.IsFalse(Button(window.View,"tactics-plus-minimumDistance").interactable);window.Revert();Assert.AreEqual("4",window.Session.Value("minimumDistance"));
            window.SetValue("minimumDistance","6");Assert.IsTrue(window.Save(),store.Error);window.SetValue("minimumDistance","7");window.Revert();Assert.AreEqual("6",window.Session.Value("minimumDistance"));
            window.RequestLeave(SkillTacticsLeaveTarget.SkillTree);tree.Close();Configure("M04",true);
            var load=ClassSkillTree.Equip(Hero.build.classSkills,Hero,"M11",1);SkillTacticsStorage.SetAutomatic(load,"M11",false);load.ProjectLegacy(Hero.build,catalog);Hero.edict=load.ProjectEdict();
            tree=Tree("M04","pc");tree.OpenTactics();window=tree.TacticsWindow;Button(window.View,"tactics-picker-anchor").onClick.Invoke();
            Assert.IsTrue(window.HasDialog);Assert.IsTrue(Button(root.GetComponent<ContentWindowHost>(),"tactics-pick-anchor-M02").interactable);
            Assert.IsFalse(Button(root.GetComponent<ContentWindowHost>(),"tactics-pick-anchor-M08").interactable);Assert.IsFalse(Button(root.GetComponent<ContentWindowHost>(),"tactics-pick-anchor-M11").interactable);
            StringAssert.Contains(SkillTacticsCatalog.Reason("automaticOff"),Texts(root.GetComponent<ContentWindowHost>()));
            Assert.IsTrue(root.GetComponent<ContentWindowHost>().Back());Assert.IsFalse(window.HasDialog);
        }
        [Test] public void UnselectedModifierAndUnsupportedAutomaticValuesStayReadableAndCannotSaveOrMutate()
        {
            Configure("W02",false);var tree=Tree(modifier:"W02-3");string before=JsonUtility.ToJson(Hero);tree.OpenTactics();var window=tree.TacticsWindow;
            StringAssert.Contains("미리 보는 보조 옵션",Texts(window.View));StringAssert.Contains("현재 보조: 없음",Texts(window.View));
            Assert.IsTrue(window.Session.Preview);Assert.IsFalse(Button(window.View,"tactics-save").interactable);Assert.IsFalse(window.SetValue("reservedCharges","0"));Assert.IsFalse(window.Save());Assert.AreEqual(before,JsonUtility.ToJson(Hero));
            window.RequestLeave(SkillTacticsLeaveTarget.SkillTree);tree.Close();Configure("W02",true);
            var load=Hero.build.classSkills.Copy();SkillTacticsStorage.Set(load,"W02","","auto","futureAuto","");load=ClassSkillTree.Remove(load,"W02");load.ProjectLegacy(Hero.build,catalog);Hero.edict=load.ProjectEdict();
            before=JsonUtility.ToJson(Hero);tree=Tree();Assert.DoesNotThrow(()=>tree.OpenTactics());window=tree.TacticsWindow;
            Assert.IsTrue(window.Session.Preview);Assert.IsFalse(window.Session.Dirty);StringAssert.DoesNotContain("futureAuto",Texts(window.View));
            Assert.IsFalse(window.SetValue("auto","1"));Assert.IsFalse(window.Save());Assert.AreEqual(before,JsonUtility.ToJson(Hero));
        }
        [Test] public void FailedSaveAndContinueKeepTheWindowDraftDiskAndParentCloseCallbackUntouched()
        {
            var tree=Tree();tree.OpenTactics();var window=tree.TacticsWindow;window.SetValue("minimumDistance","6");
            string path=Path.Combine(directory,"hellscript-local-v1.json"),disk=File.ReadAllText(path),before=JsonUtility.ToJson(Hero);
            tree.Close();Assert.IsTrue(window.HasDialog);Assert.AreEqual(0,closed);
            Directory.CreateDirectory(path+".tmp");LogAssert.Expect(LogType.Error,new Regex(".*"));Assert.IsFalse(window.ResolveLeave(SkillTacticsLeaveChoice.Save));
            Assert.IsTrue(window.HasDialog);Assert.IsTrue(window.IsOpen);Assert.IsTrue(window.Session.Dirty);Assert.AreEqual("6",window.Session.Value("minimumDistance"));Assert.AreEqual(disk,File.ReadAllText(path));Assert.AreEqual(before,JsonUtility.ToJson(Hero));Assert.AreEqual(0,closed);
            Assert.IsFalse(window.ResolveLeave(SkillTacticsLeaveChoice.ContinueEditing));Assert.IsFalse(window.HasDialog);Assert.IsTrue(window.IsOpen);
            Directory.Delete(path+".tmp");tree.Close();Assert.IsTrue(window.ResolveLeave(SkillTacticsLeaveChoice.Discard));Assert.AreEqual(1,closed);Assert.IsFalse(window.IsOpen);
        }
        [Test] public void FullCloseSavesTacticsAndTheIndependentSlotDraftOnceEach()
        {
            int commits=0;store.Committed+=_=>commits++;var tree=Tree();
            Named(tree,"d4-slot-0").GetComponentInChildren<Button>().onClick.Invoke();Named(tree,"d4-card-W03").GetComponentInChildren<Button>().onClick.Invoke();
            Assert.IsTrue(tree.Slots.Dirty);Select(tree,"W02");tree.OpenTactics();var window=tree.TacticsWindow;window.SetValue("minimumDistance","6");
            int slots=0;tree.Committed+=()=>slots++;tree.Close();Assert.IsTrue(window.HasDialog);Assert.IsTrue(window.ResolveLeave(SkillTacticsLeaveChoice.Save));
            Assert.AreEqual(1,closed,"Owner close callback");Assert.AreEqual(1,slots,"Slot commit callback");Assert.AreEqual(2,commits,"Tactics and slots transactions");Assert.IsTrue(Hero.build.classSkills.Equipped("W03"));
            Assert.IsTrue(Hero.build.classSkills.tactics.Any(v=>v.skill=="W02"&&v.row=="minimumDistance"&&v.value=="6"));
        }
        [Test] public void BackRestoresSelectionScrollAndFrameWhileAParentDestructionCancelsWithoutSaving()
        {
            var tree=Tree();tree.Content.anchoredPosition=new Vector2(0,80);var selected=tree.Session.Selection;var frame=tree.Frame.rect;
            tree.OpenTactics();var window=tree.TacticsWindow;window.RequestLeave(SkillTacticsLeaveTarget.SkillTree);
            Assert.AreEqual(selected,tree.Session.Selection);Assert.AreEqual(new Vector2(0,80),tree.Content.anchoredPosition);Assert.AreEqual(frame,tree.Frame.rect);
            tree.OpenTactics();window=tree.TacticsWindow;window.SetValue("minimumDistance","6");tree.Close();Assert.IsTrue(window.HasDialog);
            string before=JsonUtility.ToJson(Hero),path=Path.Combine(directory,"hellscript-local-v1.json"),disk=File.ReadAllText(path);int commits=0;store.Committed+=_=>commits++;
            UnityEngine.Object.DestroyImmediate(tree.gameObject);Assert.IsFalse(window.IsOpen);Assert.AreEqual(0,closed);Assert.AreEqual(0,commits);Assert.AreEqual(before,JsonUtility.ToJson(Hero));Assert.AreEqual(disk,File.ReadAllText(path));Assert.AreEqual(0,root.GetComponent<ContentWindowHost>().Count);
        }
        [TestCase(440,956,"ko")][TestCase(956,440,"ko")][TestCase(1600,900,"ko")][TestCase(1280,800,"ko")][TestCase(1680,720,"ko")]
        [TestCase(440,956,"en")][TestCase(956,440,"en")][TestCase(1600,900,"en")][TestCase(1280,800,"en")][TestCase(1680,720,"en")]
        public void LogicalBoardSizesKeepSevenRowsFixedActionsAndDraftScrollAcrossRepaint(int width,int height,string language)
        {
            Configure("M04",true);Loc.Use(language,LocalizationTable.Parse(Resources.Load<TextAsset>("Localization/en").text).Entries);
            var window=SkillTacticsWindow.OpenDevelopment(root.transform,store,catalog,"M04",_=> (1,new Rect(0,0,width,height)),_=>{});
            Assert.AreEqual(7,Rows(window).Length);var viewport=Bounds(window.Scroll.viewport);var save=Bounds(Button(window.View,"tactics-save").GetComponent<RectTransform>());
            Assert.Greater(viewport.yMin,save.yMax);Assert.IsTrue(window.SetValue("mission","leaveField"));
            window.Scroll.content.anchoredPosition=new Vector2(0,40);window.Scroll.onValueChanged.Invoke(Vector2.zero);window.View.Repaint();
            Assert.AreEqual("leaveField",window.Session.Value("mission"));Assert.IsTrue(window.Session.Dirty);Assert.AreEqual(40,window.Scroll.content.anchoredPosition.y,.5);
            if(language=="en")StringAssert.Contains("hunt edict",Named(window.View,"Tactics title").GetComponent<Text>().text);
            var frame=UiVisibility.ScreenRect(window.View.Frame);
            Assert.AreEqual(width,frame.width,.5f);Assert.AreEqual(height,frame.height,.5f);
            var visible=UiVisibility.Clip(frame,UiSafeArea.Current);
            var content=UiVisibility.ScreenRect(Named(window.View,"Tactics visible content"));
            Assert.AreEqual(visible.xMin,content.xMin,.5f);Assert.AreEqual(visible.yMin,content.yMin,.5f);
            Assert.AreEqual(visible.width,content.width,.5f);Assert.AreEqual(visible.height,content.height,.5f);
            foreach(string name in new[]{"tactics-back","tactics-close","tactics-revert","tactics-save"})
            {
                var rect=UiVisibility.ScreenRect(Button(window.View,name).GetComponent<RectTransform>());
                Assert.GreaterOrEqual(rect.width,43.99f);Assert.GreaterOrEqual(rect.height,43.99f);
                Assert.GreaterOrEqual(rect.xMin,visible.xMin-.5f);Assert.LessOrEqual(rect.xMax,visible.xMax+.5f);
                Assert.GreaterOrEqual(rect.yMin,visible.yMin-.5f);Assert.LessOrEqual(rect.yMax,visible.yMax+.5f);
            }
            var title=Named(window.View,"Tactics title").GetComponent<Text>();
            Assert.AreEqual(UiTheme.Title,title.fontSize);Assert.IsFalse(title.resizeTextForBestFit);
            Assert.LessOrEqual(title.preferredHeight,title.rectTransform.rect.height+.5f);
        }
        [TestCase(1f,"ko")][TestCase(1.6f,"ko")][TestCase(2f,"ko")]
        [TestCase(1f,"en")][TestCase(1.6f,"en")][TestCase(2f,"en")]
        public void InformationCardFitsTheFinalTextAtNativeCanvasDensities(float scale,string language)
        {
            Loc.Use(language,LocalizationTable.Parse(Resources.Load<TextAsset>("Localization/en").text).Entries);
            var window=SkillTacticsWindow.OpenDevelopment(root.transform,store,catalog,"W02",_=> (1,new Rect(0,0,1600,900)),_=>{});
            var canvas=window.View.GetComponent<Canvas>();window.View.GetComponent<CanvasScaler>().scaleFactor=canvas.scaleFactor=scale;
            // 1600x900 uses scale 2 and a 224-unit information card; force these independently of the Editor's Game view.
            var host=UiLayout.Rect("Native density information fixture",window.View.transform);UiLayout.Place(host,0,0,224,500);
            typeof(SkillTacticsWindow).GetMethod("Info",BindingFlags.Instance|BindingFlags.NonPublic)
                .Invoke(window,new object[]{host,0f,0f,224f,false,true});
            Canvas.ForceUpdateCanvases();
            var availability=Named(host,"Skill availability").GetComponent<Text>();
            Assert.AreEqual(scale,availability.pixelsPerUnit,.001f);Assert.AreEqual(12,availability.fontSize);Assert.IsFalse(availability.resizeTextForBestFit);
            if(language=="en")Assert.AreEqual("Equipped · Development validation only; combat is not connected.",availability.text);
            foreach(var label in host.GetComponentsInChildren<Text>())
                Assert.LessOrEqual(label.preferredHeight,label.rectTransform.rect.height+.5f,label.name+" must fit its final Text settings");
            var summary=Named(host,"Tactics summary");var card=Named(host,"Tactics skill information");
            Assert.GreaterOrEqual(Bounds(availability.rectTransform).yMin,Bounds(summary).yMax);
            Assert.LessOrEqual(-summary.anchoredPosition.y+summary.rect.height,card.rect.height-13.5f);
            window.OwnerDestroyed();
        }
        [Test] public void GlobalNavigationLeavesThroughTheOwnerWithoutAnImplicitSave()
        {
            var target=SkillTacticsLeaveTarget.SkillTree;var window=SkillTacticsWindow.OpenDevelopment(root.transform,store,catalog,"W02",_=> (1,new Rect(0,0,1600,900)),next=>target=next);
            string before=JsonUtility.ToJson(Hero);long revision=store.Revision;
            window.RequestLeave(SkillTacticsLeaveTarget.GlobalSettings);
            Assert.IsFalse(window.IsOpen);Assert.AreEqual(SkillTacticsLeaveTarget.GlobalSettings,target);
            Assert.AreEqual(before,JsonUtility.ToJson(Hero));Assert.AreEqual(revision,store.Revision);
        }
    }
}
