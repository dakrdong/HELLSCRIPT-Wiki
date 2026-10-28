using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Hellscript.Tests
{
    public sealed class EdictPresetsWindowTests
    {
        GameObject root;GameCatalog catalog;GameStore store;EdictPresetsWindow window;string directory;
        HeroSave Hero=>store.Data.Hero;
        [SetUp] public void Setup()
        {
            directory=null;Loc.UseSource();catalog=ScriptableObject.CreateInstance<GameCatalog>();catalog.Populate();
            var account=GameStore.NewAccount(catalog);GuideTutorial.OpenEverything(account);
            account.guide.mapComplete=true;account.guide.legacyExempt=true;
            account.Hero.created=true;account.Hero.level=60;account.Hero.useRecommendedEdict=false;
            store=GameStore.Sandbox(account);root=new GameObject("Preset popup fixture",typeof(RectTransform));
        }
        [TearDown] public void Cleanup()
        {UnityEngine.Object.DestroyImmediate(root);UnityEngine.Object.DestroyImmediate(catalog);if(directory!=null&&Directory.Exists(directory))Directory.Delete(directory,true);Loc.UseSource();}
        EdictPresetsWindow Open(Func<HuntEdictEditSession,bool> commit=null,CombatSimulation simulation=null)
            =>window=EdictPresetsWindow.Open(root.transform,store,catalog,Hero,()=>1,commit??(edit=>store.CommitHuntEdict(edit,catalog)),()=>"Save failed",null,simulation:simulation);
        Button Button(string name)=>window.View.GetComponentsInChildren<Button>().Single(b=>b.name==name);
        [Test] public void OpeningPreservesRecommendedModeAndTheSavedDocument()
        {
            Hero.useRecommendedEdict=true;string before=JsonUtility.ToJson(Hero);Open();
            Assert.AreEqual("edict-presets-window",window.View.Frame.name);
            Assert.AreEqual(before,JsonUtility.ToJson(Hero));Assert.IsFalse(window.Session.Dirty);
            Assert.IsTrue(Button("edict-default-settings").IsInteractable());
            Assert.IsTrue(Button("edict-default-settings").transform.IsChildOf(window.View.Navigation));
            Assert.IsNotNull(Button("edict-preset-store-0"));Assert.IsNotNull(Button("edict-preset-import"));
            window.ShowQuickPresets();Assert.IsTrue(Button("edict-default-settings").IsInteractable());
            Assert.IsFalse(window.View.GetComponentsInChildren<Button>().First(b=>b.name.StartsWith("edict-quick-choice-",StringComparison.Ordinal)).IsInteractable());
        }
        [TestCase(19,"ko")][TestCase(19,"en")][TestCase(20,"ko")][TestCase(20,"en")]
        public void RecommendedModeSwitchUsesTheSharingUnlockAndExplainsTheLock(int clear,string language)
        {
            Loc.Use(language,LocalizationTable.Parse(Resources.Load<TextAsset>("Localization/en").text).Entries);
            store.Data.guide.edictLegacyAccess=false;store.Data.guide.edictUnlocks.Remove(HuntEdictProgression.Sharing);
            foreach(var hero in store.Data.heroes)hero.highestClear=clear;
            HuntEdictProgression.Reconcile(store.Data);Open();
            Assert.AreEqual(clear>=20,window.View.GetComponentsInChildren<Button>().Any(b=>b.name=="edict-default-settings"));
            if(clear<20)
                Assert.AreEqual(Loc.T("추천 모드 전환은 균열 20단계부터 공개됩니다."),window.View.GetComponentsInChildren<Text>().Single(t=>t.name=="edict-default-locked").text);
            else Assert.IsTrue(Button("edict-default-settings").IsInteractable());
            Assert.IsFalse(window.Session.Dirty);
        }
        [TestCase(true)][TestCase(false)] public void FailedModeWriteKeepsTheFileOwnerAndListState(bool recommended)
        {
            directory=Path.Combine(Path.GetTempPath(),"edict-preset-mode-"+Guid.NewGuid().ToString("N"));store=new GameStore(directory,catalog);
            GuideTutorial.OpenEverything(store.Data);store.Data.guide.legacyExempt=true;store.Data.guide.edictLegacyAccess=true;HuntEdictProgression.Reconcile(store.Data);
            Assert.IsTrue(store.SetRecommendedEdict(Hero.id,recommended,catalog),store.Error);
            var simulation=new CombatSimulation(store.Data,catalog,1,seed:2164);store.Data.suspendedRun=simulation.State;Assert.IsTrue(store.Save(),store.Error);var policy=simulation.Policy;
            Open(simulation:simulation);window.ShowQuickPresets();
            string path=Path.Combine(directory,"hellscript-local-v1.json"),disk=File.ReadAllText(path),before=JsonUtility.ToJson(store.Data);long revision=store.Revision;
            Directory.CreateDirectory(path+".tmp");LogAssert.Expect(LogType.Error,new Regex("^저장하지 못했습니다: .*"+Regex.Escape(path+".tmp")));Button("edict-default-settings").onClick.Invoke();
            Assert.IsTrue(window.HasDialog);Assert.AreEqual(before,JsonUtility.ToJson(store.Data));Assert.AreEqual(disk,File.ReadAllText(path));Assert.AreEqual(revision,store.Revision);
            Assert.AreSame(policy,simulation.Policy);
            // Reload persists the offline-supplies cursor, so end the fault injection before constructing another store.
            Directory.Delete(path+".tmp");
            Assert.AreEqual(recommended,new GameStore(directory,catalog).Data.Hero.useRecommendedEdict);
            Button("edict-dialog-close").onClick.Invoke();Assert.IsTrue(Button("edict-default-settings").IsInteractable());
            Assert.AreEqual(!recommended,window.View.GetComponentsInChildren<Button>().First(b=>b.name.StartsWith("edict-quick-choice-",StringComparison.Ordinal)).IsInteractable());
            Assert.AreEqual(recommended?"ON":"OFF",window.View.GetComponentsInChildren<Text>().Single(t=>t.transform.parent.name=="Default settings state").text);
        }
        [Test] public void ADetachedHostCannotSwitchTheOwnedRecommendedMode()
        {
            window=EdictPresetsWindow.Open(root.transform,store,catalog,Hero,()=>1,_=>false,()=>store.Error,null,persistentPresets:false);
            string before=JsonUtility.ToJson(store.Data);long revision=store.Revision;
            Assert.IsFalse(Button("edict-default-settings").IsInteractable());Button("edict-default-settings").onClick.Invoke();
            Assert.AreEqual(before,JsonUtility.ToJson(store.Data));Assert.AreEqual(revision,store.Revision);
        }
        [Test] public void SlotStorageAndShareCodeKeepTheExistingPresetFormat()
        {
            Hero.edictPresets[0]=new HuntEdictPreset();Open();Button("edict-preset-store-0").onClick.Invoke();
            Assert.IsTrue(HuntEdictPreset.HasValue(Hero.edictPresets[0]),store.Error);
            Button("edict-preset-share-0").onClick.Invoke();
            var code=window.View.GetComponentsInChildren<InputField>().Single();Assert.IsTrue(code.readOnly);
            var decoded=HuntEdictPresetCodec.Decode(code.text);
            Assert.AreEqual(HuntEdictPresetCodec.Encode(Hero.edictPresets[0]),HuntEdictPresetCodec.Encode(decoded));
        }
        [Test] public void QuickPresetEditsStayDetachedUntilSaveAndRevertKeepsTheBaseline()
        {
            Open();const string scope="global/position.engage";
            var preset=HuntEdictQuickPresets.For(scope).First(p=>!HuntEdictQuickPresets.Matches(window.Session.Draft,scope,p.id));
            string before=JsonUtility.ToJson(Hero);window.SelectQuickPreset(scope,preset.id);
            Assert.IsTrue(window.Session.Dirty);Assert.AreEqual(before,JsonUtility.ToJson(Hero));
            Button("edict-presets-revert").onClick.Invoke();Assert.IsFalse(window.Session.Dirty);Assert.AreEqual(before,JsonUtility.ToJson(Hero));
            window.SelectQuickPreset(scope,preset.id);Assert.IsTrue(window.Save(),store.Error);
            Assert.IsTrue(HuntEdictQuickPresets.Matches(HuntEdictLoadout.FromHero(Hero),scope,preset.id));Assert.IsFalse(window.Session.Dirty);
        }
        [Test] public void LoadingASavedSlotWaitsForExplicitSave()
        {
            var saved=HuntEdictLoadout.FromHero(Hero);
            saved.edict=HuntEdictV2Editing.WithGlobal(saved.edict,"position.engage","STAND");
            Hero.edictPresets[0]=new HuntEdictPreset{emptySlot=false,name="Stored preset",loadout=saved};Hero.selectedEdictPreset=-1;
            Open();string before=JsonUtility.ToJson(Hero);Button("edict-preset-name-0").onClick.Invoke();
            Assert.IsTrue(window.Session.Dirty);Assert.AreEqual(0,window.Session.SelectedSlot);Assert.AreEqual(before,JsonUtility.ToJson(Hero));
            Assert.IsTrue(window.Save(),store.Error);Assert.AreEqual(0,Hero.selectedEdictPreset);
            Assert.AreEqual("STAND",HuntEdictSummary.Value(Hero.edict,"position.engage"));
        }
        [Test] public void ValidImportPreviewsAndStoresWithoutApplyingTheImportedLoadout()
        {
            Hero.edictPresets[0]=new HuntEdictPreset();Open();string before=JsonUtility.ToJson(Hero.edict);
            var imported=new HuntEdictPreset{emptySlot=false,name="Imported preset",loadout=window.Session.Draft.Copy()};
            imported.loadout.edict=HuntEdictV2Editing.WithGlobal(imported.loadout.edict,"position.engage","STAND");
            Button("edict-preset-import").onClick.Invoke();window.View.GetComponentsInChildren<InputField>().Single().text=HuntEdictPresetCodec.Encode(imported);
            window.View.GetComponentsInChildren<Button>().Single(b=>b.GetComponentInChildren<Text>()?.text==Loc.T("확인")).onClick.Invoke();
            Assert.IsFalse(HuntEdictPreset.HasValue(Hero.edictPresets[0]));Assert.AreEqual(before,JsonUtility.ToJson(Hero.edict));
            window.View.GetComponentsInChildren<Button>().Single(b=>b.GetComponentInChildren<Text>()?.text==Loc.T("저장 위치 선택")).onClick.Invoke();
            window.View.GetComponentsInChildren<Button>().Single(b=>b.GetComponentInChildren<Text>()?.text==Loc.F("{0}. {1}",1,Loc.T("비어 있음"))).onClick.Invoke();
            Assert.AreEqual("Imported preset",Hero.edictPresets[0].name);Assert.AreEqual(before,JsonUtility.ToJson(Hero.edict));
        }
        [Test] public void InvalidImportDoesNotChangeTheHeroOrPresetSlots()
        {
            Open();string before=JsonUtility.ToJson(Hero);Button("edict-preset-import").onClick.Invoke();
            window.View.GetComponentsInChildren<InputField>().Single().text="invalid-code";
            window.View.GetComponentsInChildren<Button>().Single(b=>b.GetComponentInChildren<Text>()?.text==Loc.T("확인")).onClick.Invoke();
            Assert.IsTrue(window.HasDialog);Assert.AreEqual(before,JsonUtility.ToJson(Hero));Assert.IsFalse(window.Session.Dirty);
        }
        [Test] public void TheFirstDodgePresetsStayAvailableBeforeCombatStylesAndSlotsUnlock()
        {
            store.Data.guide.edictLegacyAccess=false;store.Data.guide.edictUnlocks.Clear();store.Data.guide.edictUnlocks.AddRange(new[]{HuntEdictProgression.Dodge,"dodge.ground.policy"});
            Assert.IsTrue(EdictPresetsWindow.Available(store.Data));Open();
            Assert.IsNotNull(Button("edict-quick-picker-global/dodge.ground.policy"));Assert.IsFalse(Button("edict-presets-tab-slots").interactable);
        }
        [Test] public void CombatStylesStayAvailableBeforePersistentPresetSlotsUnlock()
        {
            store.Data.guide.edictLegacyAccess=false;store.Data.guide.edictUnlocks.Remove(HuntEdictProgression.Presets);store.Data.guide.edictUnlocks.Remove(HuntEdictProgression.Sharing);
            Assert.IsTrue(HuntEdictProgression.Has(store.Data,HuntEdictProgression.Styles));string before=JsonUtility.ToJson(Hero);
            Open();Assert.IsFalse(Button("edict-presets-tab-slots").interactable);
            foreach(var style in HuntEdictQuickPresets.Styles)Assert.IsNotNull(Button("edict-quick-choice-"+style.id));
            Assert.IsFalse(window.View.GetComponentsInChildren<Button>().Any(b=>b.name=="edict-preset-store-0"));
            Assert.AreEqual(before,JsonUtility.ToJson(Hero));
        }
        [Test] public void FailedCommitRetainsTheDraftAndCloseRequiresAChoice()
        {
            Open(_=>false);const string scope="global/position.engage";
            var preset=HuntEdictQuickPresets.For(scope).First(p=>!HuntEdictQuickPresets.Matches(window.Session.Draft,scope,p.id));
            window.SelectQuickPreset(scope,preset.id);string before=JsonUtility.ToJson(Hero);
            Assert.IsFalse(window.Save());Assert.IsTrue(window.HasDialog);Assert.IsTrue(window.Session.Dirty);
            Assert.AreEqual(before,JsonUtility.ToJson(Hero));Button("edict-dialog-close").onClick.Invoke();
            window.RequestClose();Assert.IsTrue(window.HasDialog);Assert.IsNotNull(window.View);
            Assert.AreEqual(before,JsonUtility.ToJson(Hero));
        }
    }
}
