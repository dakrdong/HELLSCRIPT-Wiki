using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hellscript.Tests
{
    public sealed class HuntEdictSuspendedSaveTests
    {
        string directory,path;GameCatalog catalog;GameStore store;
        static string Json(object value)=>JsonUtility.ToJson(value);
        [SetUp]public void Setup()
        {
            Loc.UseSource();directory=Path.Combine(Path.GetTempPath(),"hellscript-suspended-edict-"+Guid.NewGuid().ToString("N"));
            path=Path.Combine(directory,"hellscript-local-v1.json");catalog=ScriptableObject.CreateInstance<GameCatalog>();catalog.Populate();
            store=new GameStore(directory,catalog);store.Data.guide.edictLegacyAccess=true;HuntEdictProgression.Reconcile(store.Data);ContentTestAccounts.CurrentSkillPolicies(store.Data,catalog);
            Assert.IsTrue(store.ActivateSkillTrees(catalog),store.Error);
        }
        [TearDown]public void Cleanup(){UnityEngine.Object.DestroyImmediate(catalog);if(Directory.Exists(directory))Directory.Delete(directory,true);Loc.UseSource();}
        void Suspend(int cls=0,bool portal=false)
        {
            store.Data.selectedHero=cls;var sim=new CombatSimulation(store.Data,catalog,1,seed:9451);
            var run=sim.State;run.time=17;run.paused=true;run.portal=portal;run.health=sim.Stats.hp*.37f;run.resource=19;
            run.cooldowns[cls*6]=3.5f;
            run.classSkills.cooldowns.Add(new ClassSkillTimer{id=run.build.classSkills.actives[0],remaining=3.5f,total=8});
            store.Data.suspendedRun=run;Assert.IsTrue(store.Save(),store.Error);
            // Reproduce a new town session with only the persisted checkpoint, no live simulation.
            store=new GameStore(directory,catalog);
        }
        bool Save(HuntEdictEditSession session)=>store.CommitHuntEdict(session,catalog);
        static void AssertProgress(RunState before,RunState after)
        {
            Assert.AreEqual(before.id,after.id);Assert.AreEqual(before.time,after.time);Assert.AreEqual(before.rng,after.rng);
            Assert.AreEqual(before.phase,after.phase);Assert.AreEqual(before.paused,after.paused);
            Assert.AreEqual(before.portal,after.portal);
            Assert.AreEqual(before.position,after.position);Assert.AreEqual(before.resource,after.resource);
            Assert.That(after.health,Is.EqualTo(before.health).Within(.001f));
            CollectionAssert.AreEqual(before.cooldowns,after.cooldowns);
            Assert.AreEqual(Json(before.classSkills),Json(after.classSkills));
            CollectionAssert.AreEqual(before.enemies.Select(Json),after.enemies.Select(Json));CollectionAssert.AreEqual(before.drops.Select(Json),after.drops.Select(Json));
            Assert.AreEqual(Json(before.layout),Json(after.layout));Assert.AreEqual(Json(before.potions),Json(after.potions));
            CollectionAssert.AreEqual(before.projectiles.Select(Json),after.projectiles.Select(Json));CollectionAssert.AreEqual(before.shields.Select(Json),after.shields.Select(Json));
            Assert.AreEqual(before.logs.Count+1,after.logs.Count,"Only the edit, not a battle resume, should be recorded");
            Assert.IsTrue(after.logs.Last().Contains("HUNT_EDICT_CHANGED"));
        }
        [TestCase(0,false)][TestCase(1,false)][TestCase(2,false)][TestCase(0,true)]
        public void TownPresetAutoSaveUpdatesCheckpointAndResumesWithoutResettingProgress(int cls,bool portal)
        {
            Suspend(cls,portal);var account=store.Data;var hero=account.Hero;var checkpoint=account.suspendedRun;string original=Json(checkpoint);
            var session=new HuntEdictEditSession(hero);string scope="skill/"+session.Draft.classSkills.actives[0];
            string preset=HuntEdictQuickPresets.For(scope)[1].id;
            Assert.IsTrue(session.SaveSkillPolicy(hero,scope,HuntEdictSkillPresets.Select(session.Draft,scope,preset),Save),store.Error);
            Assert.AreSame(account,store.Data);Assert.AreSame(hero,store.Data.Hero);Assert.IsFalse(session.Dirty);
            Assert.AreEqual(original,Json(checkpoint),"Preparation must leave the original checkpoint untouched");
            AssertProgress(checkpoint,store.Data.suspendedRun);
            Assert.AreEqual(preset,HuntEdictSkillPresets.Active(HuntEdictLoadout.FromHero(hero),scope));
            Assert.AreEqual(Json(hero.build.classSkills),Json(store.Data.suspendedRun.build.classSkills));
            var reopened=new GameStore(directory,catalog);Assert.AreEqual(Json(store.Data.suspendedRun),Json(reopened.Data.suspendedRun));
            var resumed=new CombatSimulation(reopened.Data,catalog,1,restore:reopened.Data.suspendedRun);
            Assert.IsTrue(resumed.EdictActive,resumed.EdictInactiveReason);
            Assert.AreEqual(Json(hero.build.classSkills),Json(resumed.State.build.classSkills));
            Assert.AreEqual(checkpoint.rng,resumed.State.rng);Assert.AreEqual(3.5f,resumed.State.classSkills.cooldowns.Single().remaining);
        }
        [Test]public void FailedCheckpointWritePreservesDiskOwnerDraftAndRunThenRetrySavesOnlySkillPolicy()
        {
            Suspend();var hero=store.Data.Hero;var checkpoint=store.Data.suspendedRun;var session=new HuntEdictEditSession(hero);
            int rank=hero.build.classSkills.Rank("W01");var pending=session.Draft.Copy();pending.classSkills=ClassSkillTree.ChangeRank(pending.classSkills,hero,"W01",1);
            pending=HuntEdictQuickPresets.Apply(pending,"global/survival.lowHp","careful");session.ReplaceDraft(pending);
            var next=HuntEdictSkillPresets.Select(session.Draft,"skill/W01","edge");
            string before=Json(store.Data),draft=Json(session.Draft),baseline=Json(session.Baseline),disk=File.ReadAllText(path),globals=Json(hero.edict);
            Directory.CreateDirectory(path+".tmp");LogAssert.Expect(LogType.Error,new Regex("저장하지 못했습니다:"));
            Assert.IsFalse(session.SaveSkillPolicy(hero,"skill/W01",next,Save));
            Assert.AreSame(checkpoint,store.Data.suspendedRun);Assert.AreEqual(before,Json(store.Data));Assert.AreEqual(disk,File.ReadAllText(path));
            Assert.AreEqual(draft,Json(session.Draft));Assert.AreEqual(baseline,Json(session.Baseline));
            Directory.Delete(path+".tmp");Assert.IsTrue(session.SaveSkillPolicy(hero,"skill/W01",next,Save),store.Error);
            Assert.AreEqual(rank,hero.build.classSkills.Rank("W01"));Assert.AreEqual(rank,store.Data.suspendedRun.build.classSkills.Rank("W01"));
            CollectionAssert.AreEqual(JsonUtility.FromJson<HuntEdictV2Document>(globals).global.Select(Json),hero.edict.global.Select(Json));
            Assert.IsTrue(session.Dirty);Assert.AreEqual(rank+1,session.Draft.classSkills.Rank("W01"));
            AssertProgress(checkpoint,store.Data.suspendedRun);
        }
        [TestCase("W02","space","natural",false)][TestCase("W02","space","natural",true)]
        [TestCase("W02","space","native",false)][TestCase("W02","space","native",true)]
        [TestCase("W02","space","space",false)][TestCase("W02","space","space",true)]
        public void PolicySaveKeepsOnlyStillValidPositioningIntent(string skill,string previous,string next,bool live)
        {
            store.Data.selectedHero=ClassSkills.Find(skill).heroClass;var hero=store.Data.Hero;
            hero.build.classSkills=ClassSkillTree.Equip(hero.build.classSkills,hero,skill,0);
            hero.build.classSkills=ClassSkillOptions.WithOption(hero.build.classSkills,skill,previous);
            var sim=new CombatSimulation(store.Data,catalog,1,seed:9451);var checkpoint=sim.State;
            checkpoint.time=17;checkpoint.paused=true;
            checkpoint.classSkills.intent=new ClassSkillIntent{skillId=skill,choice=previous,started=17,until=19,distance=5};
            store.Data.suspendedRun=checkpoint;Assert.IsTrue(store.Save(),store.Error);
            var session=new HuntEdictEditSession(hero);var draft=session.Draft.Copy();
            draft.classSkills=ClassSkillOptions.WithOption(draft.classSkills,skill,next);session.ReplaceDraft(draft);
            Assert.IsTrue(store.CommitHuntEdict(session,catalog,live?sim:null,true),store.Error);
            var saved=new GameStore(directory,catalog).Data.suspendedRun;
            Assert.AreEqual(next,ClassSkillOptions.Selected(saved.build.classSkills,skill).id);
            Assert.AreEqual(17,saved.time);Assert.IsTrue(saved.paused);
            if(previous==next)Assert.AreEqual(Json(checkpoint.classSkills.intent),Json(saved.classSkills.intent));
            else Assert.IsTrue(string.IsNullOrEmpty(saved.classSkills.intent?.skillId),"No old positioning plan may survive reload");
        }
        [TestCase(RunPhase.Cleared)][TestCase(RunPhase.Failed)]
        public void TerminalResultKeepsItsOriginalBuildWhileTownSettingsSave(RunPhase phase)
        {
            Suspend();var checkpoint=store.Data.suspendedRun;checkpoint.phase=phase;Assert.IsTrue(store.Save(),store.Error);
            string before=Json(checkpoint);var hero=store.Data.Hero;var session=new HuntEdictEditSession(hero);
            Assert.IsTrue(session.SaveSkillPolicy(hero,"skill/W01",HuntEdictSkillPresets.Select(session.Draft,"skill/W01","edge"),Save),store.Error);
            Assert.AreSame(checkpoint,store.Data.suspendedRun);Assert.AreEqual(before,Json(checkpoint));
            Assert.AreEqual(before,Json(new GameStore(directory,catalog).Data.suspendedRun));
        }
        [Test]public void TrainingCheckpointDoesNotBecomeAnEditableTownRun()
        {
            Suspend();store.Data.suspendedRun.training=1;
            var session=new HuntEdictEditSession(store.Data.Hero);session.ReplaceDraft(HuntEdictSkillPresets.Select(session.Draft,"skill/W01","edge"));
            string before=Json(store.Data),disk=File.ReadAllText(path);Assert.IsFalse(Save(session));
            Assert.AreEqual(before,Json(store.Data));Assert.AreEqual(disk,File.ReadAllText(path));Assert.IsTrue(session.Dirty);
        }
        [Test]public void AnotherHerosCheckpointIsNotRewritten()
        {
            Suspend();var checkpoint=store.Data.suspendedRun;string before=Json(checkpoint);store.Data.selectedHero=1;
            var hero=store.Data.Hero;var session=new HuntEdictEditSession(hero);string scope="skill/"+session.Draft.classSkills.actives[0];
            Assert.IsTrue(session.SaveSkillPolicy(hero,scope,HuntEdictSkillPresets.Select(session.Draft,scope,HuntEdictQuickPresets.For(scope)[1].id),Save),store.Error);
            Assert.AreSame(checkpoint,store.Data.suspendedRun);Assert.AreEqual(before,Json(checkpoint));
        }
    }
}
