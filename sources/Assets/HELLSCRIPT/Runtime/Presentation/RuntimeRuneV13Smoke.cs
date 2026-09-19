using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hellscript.Runes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Hellscript
{
    // Explicit opt-in development harness; requires an isolated save and user-supplied fixture paths.
    public sealed class RuntimeRuneV13Smoke:MonoBehaviour
    {
        GameController game;string output;int captures;readonly HashSet<string> missing=new HashSet<string>();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void Install()
        {if(Debug.isDebugBuild&&Environment.GetCommandLineArgs().Contains("-hellscriptRuneV13Smoke")){Application.runInBackground=true;new GameObject("Rune v13 verification").AddComponent<RuntimeRuneV13Smoke>();}}
        static string Arg(string name){var a=Environment.GetCommandLineArgs();int i=Array.IndexOf(a,name);if(i<0||i+1>=a.Length)throw new ArgumentException(name);return a[i+1];}
        static void Require(bool check,string message){if(!check)throw new InvalidOperationException(message);}
        Button Find(string name)=>game.UI.GetComponentsInChildren<Button>().Single(b=>b.name==name);
        void Click(string name){var button=Find(name);Require(button.IsInteractable(),"Disabled "+name);button.onClick.Invoke();}
        RuneBoardGraphic Board=>game.UI.GetComponentsInChildren<RuneBoardGraphic>().Single(b=>b.name=="rune-board");
        void Tap(HexCell cell)=>Board.OnPointerClick(new PointerEventData(EventSystem.current){position=Board.ScreenCell(cell)});
        IEnumerator Resize(int width,int height)
        {
            Screen.SetResolution(width,height,FullScreenMode.Windowed);float until=Time.realtimeSinceStartup+8;while((Screen.width!=width||Screen.height!=height)&&Time.realtimeSinceStartup<until)yield return null;
            Require(Screen.width==width&&Screen.height==height,"Resolution failed");yield return null;Canvas.ForceUpdateCanvases();yield return new WaitForSecondsRealtime(.25f);
        }
        IEnumerator Capture(string name,int width,int height)
        {
            yield return Resize(width,height);
            Require(Board.editor.Board.Cells.Count==259,"Wrong board geometry");foreach(var key in Loc.Missing)missing.Add(key);
            ScreenCapture.CaptureScreenshot(Path.Combine(output,(++captures).ToString("00")+"-"+name+".png"));yield return new WaitForSecondsRealtime(.2f);
        }
        IEnumerator Start()
        {
            output=Arg("-hellscriptScreenshots");Arg("-hellscriptSavePath");Directory.CreateDirectory(output);
            Application.logMessageReceived+=(m,s,t)=>{if(t==LogType.Exception){File.WriteAllText(Path.Combine(output,"failure.txt"),m+"\n"+s);Application.Quit(1);}};
            yield return new WaitForSecondsRealtime(1);game=FindAnyObjectByType<GameController>();Require(game?.Store!=null,"Store unavailable");game.enabled=false;game.ApplyLanguage("ko");game.ApplyInterfaceScale(100);
            game.UI.ShowRunes();yield return Capture("fresh-portrait-ko",720,1280);
            Click("rune-weapon-sword");yield return null;
            string id=game.Store.Data.runes.owned[0].id;Click("rune-card-"+id);Tap(new HexCell(1,0));Require(Board.editor.DraftPlacements.Count==1,"Tap placement failed");Click("rune-save");yield return null;
            Require(game.Store.Data.runes.placements.Single().runeId==id,"Save failed");
            Tap(new HexCell(1,0));Click("rune-remove");Click("rune-weapon-bow");yield return null;
            Click("rune-card-"+id);Tap(new HexCell(1,0));Click("rune-save");Require(game.Store.Data.runes.placements.Single().weapon=="bow","Cross-weapon transfer failed");
            Click("닫기");yield return null;
            RuneMasteryProgress.AddExperience(RuneMasteryProgress.Get(game.Store.Data.runes,"sword"),100);game.Store.Save();game.UI.ShowRunes();Click("rune-weapon-sword");
            Tap(new HexCell(3,0));Click("rune-unlock");Require(RuneMasteryProgress.Get(game.Store.Data.runes,"sword").unlocked.Count==19,"Slot draft leaked before Save");
            Click("rune-save");Require(RuneMasteryProgress.Get(game.Store.Data.runes,"sword").unlocked.Count==20,"Slot unlock failed");Click("닫기");yield return null;
            game.Store.Data.runes=JsonUtility.FromJson<RuneGrowthState>(File.ReadAllText(Arg("-hellscriptRuneFixture")));RuneGrowth.Normalize(game.Store.Data);game.Store.Save();
            game.UI.ShowRunes();Click("rune-weapon-sword");yield return Capture("reference-landscape-ko",1280,720);
            Click("rune-presets");
            for(int i=0;i<5;i++){Click("rune-preset-save-"+i);yield return null;Click("rune-preset-confirm");yield return null;}
            Require(game.Store.Data.runes.presets.All(p=>!p.occupied),"Preset draft leaked before Save");Click("rune-dialog-close");Click("rune-save");yield return null;
            var reopened=new GameStore(Arg("-hellscriptSavePath"),game.catalog);Require(reopened.Data.runes.presets.All(p=>p.occupied&&p.placements.Count==5),"Global presets failed disk reload");
            Tap(new HexCell(1,0));Click("rune-remove");Require(Board.editor.DraftPlacements.Count==4,"Retrieve sample failed");Click("rune-undo");Require(Board.editor.DraftPlacements.Count==5,"Undo did not restore sample");
            Tap(new HexCell(1,0));Click("rune-remove");Click("rune-presets");Click("rune-preset-load-0");Require(Board.editor.DraftPlacements.Count==5,"Preset load did not restore all placements");
            Click("rune-presets");yield return Capture("global-presets-ko",1280,720);Click("rune-dialog-close");
            yield return Capture("reference-portrait-ko",720,1280);
            Click("rune-weapon-sword");Click("rune-region-1");yield return Capture("region-preview-ko",1280,720);
            Click("rune-region-lock-1");yield return Capture("region-dialog-ko",1280,720);Click("rune-dialog-close");
            Click("rune-region-0");Click("rune-overview");yield return Capture("full-map-ko",1280,720);
            Click("rune-fit");game.ApplyLanguage("en");yield return Capture("reference-landscape-en",1280,720);yield return Capture("reference-portrait-en",720,1280);
            game.ApplyLanguage("ko");game.ApplyInterfaceScale(140);yield return Capture("large-type-portrait-ko",720,1280);game.ApplyInterfaceScale(100);
            yield return Resize(1280,720);Click("rune-effects-type-0");yield return Capture("attack-effects-ko",1280,720);Click("rune-dialog-close");
            Click("rune-codex");Click("rune-codex-weapons");Click("지팡이");
            var search=game.UI.GetComponentsInChildren<InputField>().Single();search.text="연쇄";yield return Capture("codex-search-ko",1280,720);
            Click("rune-codex-regions");Click("기교");yield return Capture("codex-region-ko",1280,720);Click("rune-dialog-close");
            yield return Resize(720,1280);Click("rune-codex");yield return Capture("codex-portrait-ko",720,1280);Click("rune-dialog-close");
            File.WriteAllText(Path.Combine(output,"result.txt"),"PASS: exact 259-cell boards; tap placement, account save, cross-weapon recovery, mastery slot unlock draft/save, five global preset drafts/save/disk reload/load, undo; supplied v13 sample; landscape/portrait, map, region preview, per-color effects, codex weapon/region selection/search and English.\nMissing translations: "+string.Join(" | ",missing));
            Require(missing.Count==0,"Missing translations: "+string.Join(" | ",missing));Application.Quit(0);
        }
    }
}
