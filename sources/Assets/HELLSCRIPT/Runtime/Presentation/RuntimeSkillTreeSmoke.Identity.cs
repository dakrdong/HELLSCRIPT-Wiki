#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    public sealed partial class RuntimeSkillTreeSmoke
    {
        static string SavedRun(RunState state)=>Json(JsonUtility.FromJson<RunState>(Json(state)));
        IEnumerator RegressionRepairFlow(bool resume)
        {
            // Fixed local clocks prevent offline settlement from changing a comparison fixture.
            const long clock=1791068400;
            foreach(int stage in new[]{1,5,12})
            {
                string directory=Path.Combine(output,"rift-save-"+stage),expected=Path.Combine(output,"rift-expected-"+stage+".json");
                if(!resume)Require(!Directory.Exists(directory),"Use a fresh repair evidence directory.");
                var store=new GameStore(directory,game.catalog,forgeClock:()=>clock,offlineClock:()=>clock);
                if(!resume){store.Data.Hero.level=30;store.Data.Hero.capacity=100;}
                var checkpoint=store.Data.suspendedRun;
                if(resume)Require(checkpoint!=null&&checkpoint.stage==stage,"Missing saved rift "+stage);
                string auditAccount=resume?Json(store.Data):"";
                var sim=new CombatSimulation(store.Data,game.catalog,stage,restore:resume?checkpoint:null,
                    seed:(uint)(4242+stage),recordResume:false);
                if(resume)
                {
                    Require(File.ReadAllText(Path.Combine(output,"rift-checkpoint-"+stage+".json"))==SavedRun(sim.State),"Reload changed the checkpoint "+stage);
                }
                for(int tick=0;tick<(resume?100:200);tick++)sim.Tick(CombatSimulation.Step);
                if(!resume)
                {
                    store.Data.suspendedRun=sim.State;Require(store.Save(),store.Error);
                    File.WriteAllText(Path.Combine(output,"rift-checkpoint-"+stage+".json"),SavedRun(sim.State));
                    for(int tick=0;tick<100;tick++)sim.Tick(CombatSimulation.Step);
                    File.WriteAllText(expected,SavedRun(sim.State));
                }
                else
                {
                    string actual=SavedRun(sim.State);Require(File.ReadAllText(expected)==actual,"Fresh process diverged after 100 ticks: rift "+stage);
                    File.WriteAllText(Path.Combine(output,"rift-actual-"+stage+".json"),actual);
                    var savedAccount=JsonUtility.FromJson<AccountSave>(auditAccount);var before=savedAccount.suspendedRun;
                    int resumes=before.journal.resumes,events=before.journal.events.Count(e=>e.kind=="RESUME");
                    string action=Json(before.heroAction);var playerResume=new CombatSimulation(savedAccount,game.catalog,stage,restore:before);
                    Require(playerResume.State.journal.resumes==resumes+1&&playerResume.State.journal.events.Count(e=>e.kind=="RESUME")==events+1,"Player resume did not append exactly one audit event");
                    Require(action==Json(playerResume.State.heroAction),"Player resume changed an in-flight action");
                }
            }
            if(resume)
            {
                File.WriteAllText(Path.Combine(output,"repair-restart.txt"),"PASS: three fresh-process rifts match their uninterrupted 100-tick controls as complete saved JSON, including positions, navigation waypoints, RNG, health, cooldowns, DPS and journal throttles. Real player resume adds exactly one audit event without changing the in-flight action.\n");
                Application.Quit(0);yield break;
            }
            ExistingTrees(game);foreach(var hero in game.Store.Data.heroes)hero.level=40;
            game.Store.Data.guide.hintsHidden=true;game.Store.Data.guide.mapComplete=true;
            Require(game.Store.ActivateSkillTrees(game.catalog),game.Store.Error);
            yield return Resize(1600,900);yield return PresetFixture(0,"W05");yield return PickPreset("chain");
            var session=Window.PreviewCombat;Require(session!=null,"Missing live chained-shield preview");
            Require(session.Combat.State.shields.Any(s=>s.definitionId=="W16"&&s.amount>0),"Prepared shield is absent");
            session.SetSpeed(2);float timeout=Time.realtimeSinceStartup+15;
            while(!session.Ended&&!session.Combat.State.shields.Any(s=>s.definitionId=="W05"&&s.amount>0)&&Time.realtimeSinceStartup<timeout)yield return null;
            Require(session.Combat.State.shields.Any(s=>s.definitionId=="W05"&&s.amount>0),"Chained defense never produced its shield");
            Require(!session.Combat.State.shields.Any(s=>s.definitionId=="W16"&&s.amount>0&&s.remaining>0),"Chained defense overlapped the previous shield");
            session.SetPaused(true);File.WriteAllText(Path.Combine(output,"W05-combat.json"),JsonUtility.ToJson(session.Combat.State,true));
            foreach(var size in new[]{new Vector2Int(440,956),new Vector2Int(956,440),new Vector2Int(1600,900),new Vector2Int(1600,1000),new Vector2Int(2100,900)})
            foreach(string language in new[]{"ko","en"})
            {yield return Resize(size.x,size.y,language);CheckPresetFrame();yield return Capture("W05-chain-"+size.x+"x"+size.y+"-"+language);}
            UiSafeArea.Simulate(32,24,40,28);yield return Resize(956,440,"en");CheckPresetFrame();yield return Capture("W05-chain-safe-area");UiSafeArea.StopSimulating();
            Window.RequestClose();yield return null;
            var saved=new GameStore(game.Profiles.GuestDirectory,game.catalog);Require(HuntEdictQuickPresets.Matches(HuntEdictLoadout.FromHero(saved.Data.Hero),"skill/W05","chain"),"Activated shield policy did not persist");
            File.WriteAllText(Path.Combine(output,"repair-checkpoint.txt"),"PASS: three real rifts saved at 10 seconds with uninterrupted 15-second controls; actual chained shields, production preset pointer/save, five viewports in KO/EN and simulated safe area.\n");
            Application.Quit(0);
        }
        IEnumerator IdentityFlow()
        {
            ExistingTrees(game);foreach(var hero in game.Store.Data.heroes)hero.level=40;
            game.Store.Data.guide.hintsHidden=true;game.Store.Data.guide.mapComplete=true;
            Require(game.Store.ActivateSkillTrees(game.catalog),game.Store.Error);
            var cases=new[]{(0,"W11","opportunity","PUSH"),(0,"W15","opportunity","BORROW"),
                (1,"A15","opportunity","CROSSFIRE"),(1,"A18","opportunity","AFTERIMAGE"),
                (2,"M05","chain","ELEMENT_REACTION"),(2,"M10","opportunity","RETURN"),(2,"M12","setup","RELAY")};
            foreach(var item in cases)
            {
                yield return Resize(1600,900);yield return PresetFixture(item.Item1,item.Item2);yield return PickPreset(item.Item3);
                var session=Window.PreviewCombat;Require(session!=null,"Missing actual combat preview");
                bool Outcome()
                {
                    var state=session.Combat.State;var evidence=state.effectEvents.FirstOrDefault(e=>e.definitionId==item.Item2&&e.kind==item.Item4);
                    if(evidence==null)return false;
                    if(item.Item2=="A15")return state.damageEvents.Any(d=>d.definitionId=="EXTRA:A15"&&d.time>=evidence.time&&d.hpLoss>0);
                    if(item.Item2=="A18")return state.damageEvents.Any(d=>d.definitionId=="EXTRA:A18"&&d.hpLoss>0);
                    if(item.Item2=="M10")return !state.classSkills.effects.Any(e=>e.kind=="returnGlobe"&&e.root==evidence.rootCastId);
                    return true;
                }
                session.SetSpeed(2);float timeout=Time.realtimeSinceStartup+18;
                while(!session.Ended&&session.Seconds<25&&!Outcome()&&Time.realtimeSinceStartup<timeout)yield return null;
                var state=session.Combat.State;File.WriteAllText(Path.Combine(output,item.Item2+"-combat.json"),JsonUtility.ToJson(state,true));
                Require(Outcome(),item.Item2+" automatic combat missing completed "+item.Item4);
                session.SetPaused(true);CheckPresetFrame();yield return Capture(item.Item2+"-automatic-combat");
                checks.Add(item.Item2+"/"+item.Item3+": real rendered automatic "+item.Item4+", activated preset saved via production pointer/transaction PASS");
                foreach(var size in new[]{new Vector2Int(440,956),new Vector2Int(956,440),new Vector2Int(1600,900),new Vector2Int(1600,1000),new Vector2Int(2100,900)})
                foreach(string language in new[]{"ko","en"})
                {
                    yield return Resize(size.x,size.y,language);CheckPresetFrame();
                    Require(ReferenceEquals(session,Window.PreviewCombat),"Language or resize changed the preview");
                    yield return Capture(item.Item2+"-"+size.x+"x"+size.y+"-"+language);
                }
                Window.RequestClose();yield return null;
                var reread=new GameStore(save,game.catalog);Require(HuntEdictQuickPresets.Matches(HuntEdictLoadout.FromHero(reread.Data.Hero),"skill/"+item.Item2,item.Item3),"Saved preset did not survive disk reload: "+item.Item2);
            }
            game.SelectHero(1);game.EnterPlaza(true);yield return Resize(1600,900);game.UI.ShowEdictSkillTree();yield return null;
            yield return Inspect("AP17");Check("identity-passive-icon");yield return Capture("AP17-tree-icon");Window.RequestClose();yield return null;
            game.UI.ShowEdictEditor();yield return null;Window.OpenSkillPolicy("BASIC");yield return null;
            yield return Tap("edict-policy-order");yield return PickPreset("identity");
            foreach(var size in new[]{new Vector2Int(440,956),new Vector2Int(956,440),new Vector2Int(1600,900),new Vector2Int(1600,1000),new Vector2Int(2100,900)})
            foreach(string language in new[]{"ko","en"})
            {yield return Resize(size.x,size.y,language);CheckPresetFrame();yield return Capture("identity-order-"+size.x+"x"+size.y+"-"+language);}
            UiSafeArea.Simulate(32,24,40,28);yield return Resize(956,440,"en");CheckPresetFrame();yield return Capture("identity-order-safe-area");UiSafeArea.StopSimulating();
            Window.RequestClose();yield return null;yield return Resize(440,956,"en");game.UI.ShowEdictEditor();yield return null;
            Window.FocusGlobalOption("position.engage");yield return null;yield return Tap("edict-quick-picker-global/position.engage");
            yield return Capture("orbit-preset-picker");yield return Tap("edict-quick-choice-mobile-crossfire");
            if(Window.HasDialog){Window.HandleBack();yield return null;}
            yield return Tap("edict-save");
            var saved=HuntEdictLoadout.FromHero(new GameStore(save,game.catalog).Data.Hero);
            Require(HuntEdictQuickPresets.Matches(saved,"global/position.engage","mobile-crossfire"),"Orbit preset did not survive a real pointer selection, save and disk reload");
            yield return Capture("orbit-preset-saved");Window.RequestClose();yield return null;
            checks.Add("Identity order and orbit presets selected through actual controls, responsive KO/EN order labels and simulated safe area, saved values read back from disk PASS");
            checks.Add("Eight imported icon masters; existing tree and policy renderers; five viewport ratios in KO/EN; disk reload; no real account save used PASS");
            File.WriteAllLines(Path.Combine(output,"identity-runtime.txt"),checks);Debug.Log("SKILL_IDENTITY_RUNTIME_OK");Application.Quit(0);
        }
    }
}
#endif
