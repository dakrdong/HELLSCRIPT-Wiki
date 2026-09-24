using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    public sealed class RuntimeContentUnlockSmoke : MonoBehaviour
    {
        GameController game;string directory;int capture;
        static void Require(bool value,string message){if(!value)throw new InvalidOperationException(message);}
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void Install()
        {
            if(!Debug.isDebugBuild||!Environment.GetCommandLineArgs().Contains("-hellscriptUnlockSmoke"))return;
            Application.runInBackground=true;Application.logMessageReceived+=(m,s,t)=>{if(t==LogType.Exception)Application.Quit(1);};
            new GameObject("Content unlock validation").AddComponent<RuntimeContentUnlockSmoke>();
        }
        IEnumerator Capture(string label,int width,int height,float scroll=1)
        {
            Screen.SetResolution(width,height,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(.8f);
            foreach(var view in game.UI.GetComponentsInChildren<ScrollRect>())view.verticalNormalizedPosition=scroll;
            Canvas.ForceUpdateCanvases();yield return new WaitForSecondsRealtime(.2f);
            foreach(var text in game.UI.GetComponentsInChildren<Text>())Require(text.preferredHeight<=text.rectTransform.rect.height+2,"Clipped unlock text: "+text.text);
            Require(Loc.MissingCount==0,"Missing translations: "+string.Join(";",Loc.Missing));
            ScreenCapture.CaptureScreenshot(Path.Combine(directory,$"{++capture:00}-{label}.png"));yield return new WaitForSecondsRealtime(.2f);
        }
        IEnumerator Start()
        {
            var args=Environment.GetCommandLineArgs();Require(args.Contains("-hellscriptSavePath"),"Isolated save required.");
            for(int i=0;i<args.Length-1;i++)if(args[i]=="-hellscriptScreenshots")directory=args[i+1];
            Require(!string.IsNullOrEmpty(directory),"Evidence directory required.");Directory.CreateDirectory(directory);
            yield return new WaitForSecondsRealtime(1);game=FindAnyObjectByType<GameController>();game.enabled=false;
            game.Store.Data.guide.hintsHidden=true;
            Require(ContentUnlocks.Has(game.Store.Data,ContentUnlocks.Train),"Fresh account must have training.");
            game.ApplyLanguage("ko");game.UI.ShowContentUnlocks();yield return Capture("new-ko",720,1280);
            game.ApplyLanguage("en");game.UI.ShowContentUnlocks();yield return Capture("new-en",1280,720);
            game.UI.ShowShop();yield return Capture("shop-locked-en",720,1280);
            Require(game.UI.GetComponentsInChildren<Button>().Count(b=>!b.interactable)>=3,"Individual shop functions not locked.");
            game.Begin(seed:849);game.Combat.Abandon();game.ReturnTown();
            Require(ContentUnlocks.Has(game.Store.Data,ContentUnlocks.Train),"Failed first attempt did not unlock training.");
            Require(!ContentUnlocks.Has(game.Store.Data,ContentUnlocks.Enhance),"Failure unlocked enhancement.");
            game.Store.Data.Hero.highestClear=ContentUnlocks.Rules.features.Max(f=>f.stage);ContentUnlocks.Reconcile(game.Store.Data);game.Save();
            game.UI.ShowContentUnlocks();yield return Capture("unlocked-en-bottom",720,1280,0);
            game.UI.ShowContentGuide(ContentUnlocks.Reroll);yield return Capture("replay-guide-en",720,1280);
            var skip=game.UI.GetComponentsInChildren<Button>().First(b=>b.GetComponentInChildren<Text>()?.text==Loc.T("안내 확인 / 건너뛰기"));skip.onClick.Invoke();
            Require(game.Store.Data.contentUnlocks.guidesCompleted.Count==1,"Guide not saved.");
            game.SelectHero(1);Require(ContentUnlocks.Has(game.Store.Data,ContentUnlocks.CoreCraft),"Account access lost on hero switch.");
            uint rng=42;Require(!Economy.Sweep(game.Store.Data,"no-personal-record",ref rng),"Hero without a clear swept.");
            game.ApplyLanguage("ko");game.UI.ShowContentUnlocks();yield return Capture("other-hero-ko",1280,720);
            game.UI.ShowGemMenu();yield return Capture("gem-dependency-ko",720,1280);
            File.WriteAllText(Path.Combine(directory,"runtime-unlock-smoke.txt"),"PASS: native development player; Korean/English and both orientations; locked shop actions; first failed run; account-wide unlocks; guide skip persisted; personal sweep restriction; no clipped text or missing translations. Automated UI/flow fixture, not manual balance playtesting.");
            Debug.Log("HELLSCRIPT_UNLOCK_RUNTIME_OK");Application.Quit(0);
        }
    }
}
