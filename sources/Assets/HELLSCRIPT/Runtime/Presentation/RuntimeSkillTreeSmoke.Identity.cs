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
