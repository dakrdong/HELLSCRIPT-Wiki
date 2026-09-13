using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    // Opt-in development fixtures on isolated saves. Gems are seeded explicitly because
    // acquisition and the owning storage/service are not connected in this phase.
    public sealed class RuntimeGemSocketSmoke:MonoBehaviour
    {
        GameController game;string directory,saveDirectory,stage;int capture;uint rng=819322;
        [Serializable] sealed class Items {public List<Item> items;}
        [Serializable] sealed class Sample {public int seed,stage,kills;public bool gemmed;public string result;public float seconds,maximumHp,armor;public double damageDealt,damageReceived;}
        [Serializable] sealed class Samples {public string method;public Sample[] rows;}
        static string Json(object value)=>JsonUtility.ToJson(value,true);
        static void Require(bool value,string message){if(!value)throw new InvalidOperationException(message);}
        static T Field<T>(object owner,string name)=>(T)owner.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public).GetValue(owner);
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void Install()
        {
            if(!Debug.isDebugBuild||!Environment.GetCommandLineArgs().Contains("-hellscriptGemSocketSmoke"))return;
            Application.runInBackground=true;Application.logMessageReceived+=(m,s,t)=>{if(t==LogType.Exception)Application.Quit(1);};
            new GameObject("Gem socket verification").AddComponent<RuntimeGemSocketSmoke>();
        }
        void Click(string name)
        {var b=game.UI.GetComponentsInChildren<Button>().Single(b=>b.name==name);Require(b.IsInteractable(),"Disabled gem fixture command: "+name);b.onClick.Invoke();}
        Item Add(int slot,string gem="")
        {
            var item=ItemGenerator.Create(HeroClass.Warrior,slot,2,30,ref rng);item.sockets.Add(new SocketState{index=0,gemId=gem,tier=gem==""?0:6});
            Require(Economy.AddItem(game.Store.Data.Hero,item,BagPolicy.Ignore,game.Store.Data),"Fixture inventory full");return item;
        }
        IEnumerator Capture(string name,int width,int height,string focus=null,bool comparison=false)
        {
            Screen.SetResolution(width,height,FullScreenMode.Windowed);float end=Time.realtimeSinceStartup+6;
            while((Screen.width!=width||Screen.height!=height)&&Time.realtimeSinceStartup<end)yield return null;
            Require(Screen.width==width&&Screen.height==height,"Gem fixture resize failed");yield return new WaitForSecondsRealtime(.3f);
            if(focus!=null)
            {
                Transform root=game.UI.transform;
                if(comparison)
                {var pane=Field<ScrollRect>(game.UI,"inventoryComparison");root=pane.gameObject.activeInHierarchy?pane.content:Field<ScrollRect>(game.UI,"inventoryDetail").content;}
                var text=root.GetComponentsInChildren<Text>().First(t=>t.text.Contains(focus));DialogReadingAnchor.Show(text.rectTransform);yield return new WaitForSecondsRealtime(.15f);
            }
            Canvas.ForceUpdateCanvases();
            foreach(var text in game.UI.GetComponentsInChildren<Text>())Require(text.preferredHeight<=text.rectTransform.rect.height+2,"Clipped gem text: "+text.text);
            Require(Loc.MissingCount==0,"Missing gem translations: "+string.Join("; ",Loc.Missing));
            ScreenCapture.CaptureScreenshot(Path.Combine(directory,stage+"-"+(++capture).ToString("00")+"-"+name+".png"));yield return new WaitForSecondsRealtime(.2f);
        }
        void CompareRuns()
        {
            var samples=new List<Sample>();
            foreach(bool gemmed in new[]{false,true})
            {
                var account=JsonUtility.FromJson<AccountSave>(Json(game.Store.Data));account.Hero.build.autoRepeat=false;account.Hero.useEdict=false;account.Hero.build.bagPolicy=BagPolicy.Ignore;
                if(!gemmed)foreach(var item in account.Hero.inventory)foreach(var socket in item.sockets){socket.gemId="";socket.tier=0;}
                var sim=new CombatSimulation(account,game.catalog,10,seed:73551);
                // Full fixed-step run with a seeded legal loadout; no outcome, health, enemy,
                // layout or reward injection. This is a comparison fixture, not a farming claim.
                for(int tick=0;tick<10000&&sim.State.phase!=RunPhase.Cleared&&sim.State.phase!=RunPhase.Failed;tick++)
                {sim.Tick(CombatSimulation.Step);Require(!sim.State.portal,"Comparison unexpectedly requested inventory cleanup");}
                Require(sim.State.phase==RunPhase.Cleared||sim.State.phase==RunPhase.Failed,"Comparison did not finish");
                samples.Add(new Sample{seed=73551,stage=10,gemmed=gemmed,result=sim.State.phase.ToString(),kills=sim.State.kills,seconds=sim.State.time,damageDealt=sim.State.statistics.damage,damageReceived=sim.State.statistics.incomingDamage,maximumHp=sim.Stats.hp,armor=sim.Stats.armor});
                File.WriteAllText(Path.Combine(directory,gemmed?"gemmed-run.json":"empty-run.json"),Json(sim.State));
            }
            File.WriteAllText(Path.Combine(directory,"combat-comparison.json"),Json(new Samples{method="Seeded level-30 legal rare gear, tier-six gems on five eligible slots, stage 10, seed 73551, ignore new loot if the equipment bag is full. Fixed-step native simulation. Gem acquisition was not exercised. One paired sample; not a balance guarantee. Damage totals use the run-wide telemetry.",rows=samples.ToArray()}));
        }
        IEnumerator Start()
        {
            var args=Environment.GetCommandLineArgs();for(int i=0;i<args.Length-1;i++)
            {if(args[i]=="-hellscriptSavePath")saveDirectory=args[i+1];if(args[i]=="-hellscriptScreenshots")directory=args[i+1];if(args[i]=="-hellscriptGemStage")stage=args[i+1];}
            Require(!string.IsNullOrEmpty(saveDirectory)&&!string.IsNullOrEmpty(directory)&&!string.IsNullOrEmpty(stage),"Use explicit isolated gem fixture paths and stage");
            Directory.CreateDirectory(directory);yield return new WaitForSecondsRealtime(1);game=FindAnyObjectByType<GameController>();Require(game?.Store!=null,"Gem fixture account did not load");game.enabled=false;
            var account=game.Store.Data;var hero=account.Hero;
            if(stage=="initial")
            {
                hero.level=30;hero.highestClear=10;hero.inventory.Clear();hero.build=BehaviorPresets.ForLevel(HeroClass.Warrior,0,30,game.catalog);hero.build.autoRepeat=false;hero.useEdict=false;account.guide.hintsHidden=true;
                var weapon=Add(0,"G06");var head=Add(1,"G02");var body=Add(2,"G04");var neck=Add(6,"G01");var ring=Add(7,"G07");
                foreach(var item in hero.inventory)item.equipped=true;
                var candidate=Add(1);game.Save();File.WriteAllText(Path.Combine(directory,"candidate-id.txt"),candidate.id);File.WriteAllText(Path.Combine(directory,"gemmed-id.txt"),head.id);
                game.ApplyLanguage("ko");game.UI.ShowItemDetail(head.id);yield return Capture("socket-ko",1920,1080,"소켓 ·");
                game.ApplyLanguage("en");game.ApplyInterfaceScale(140);game.UI.ShowItemDetail(head.id);yield return Capture("socket-en-portrait",720,1280,"Socket ·");
                yield return Capture("socket-en-short-landscape",640,360,"Socket ·");
                game.UI.ShowItemDetail(candidate.id);yield return Capture("comparison-en",1920,1080,"Gem effect removed",true);
                // The equipment UI performs the real swap. The removed gem stays in its item.
                Click("장착");yield return null;head=hero.inventory.Single(i=>i.id==head.id);Require(!head.equipped&&head.sockets.Single().gemId=="G02","Equipment swap changed the old gem");
                game.UI.ShowItemDetail(head.id);yield return null;
                Require(game.UI.GetComponentsInChildren<Button>().Where(b=>b.name.StartsWith("판매 ·",StringComparison.Ordinal)||b.name.StartsWith("분해 ·",StringComparison.Ordinal)).All(b=>!b.IsInteractable()),"Gemmed item can be disposed");
                Click("공유 창고로 이동");yield return null;Require(account.warehouse.Single().sockets.Single().gemId=="G02","Warehouse lost gem");
                Click("창고");yield return null;Click("Inventory item "+head.id);yield return null;Click("현재 캐릭터의 가방으로 이동");yield return null;game.UI.ShowItemDetail(head.id);Click("장착");yield return null;
                game.ApplyInterfaceScale(100);game.ApplyLanguage("ko");game.UI.ShowAttributes();yield return Capture("attributes-ko",1280,720);
                game.ApplyLanguage("en");game.ApplyInterfaceScale(140);game.UI.ShowAttributes();yield return Capture("attributes-en",720,1280);
                CompareRuns();game.Save();File.WriteAllText(Path.Combine(directory,"expected-items.json"),Json(new Items{items=hero.inventory}));
            }
            else if(stage=="restart")
            {
                Require(Json(new Items{items=hero.inventory})==File.ReadAllText(Path.Combine(directory,"expected-items.json")),"Restart changed item or socket data");
                Require(game.Store.GemRecoveryArchive=="","Valid sockets unexpectedly triggered recovery");
                game.UI.ShowItemDetail(File.ReadAllText(Path.Combine(directory,"gemmed-id.txt")));yield return Capture("restored-socket-en",1280,720,"Socket ·");
            }
            else if(stage=="recover")
            {
                string id=File.ReadAllText(Path.Combine(directory,"gemmed-id.txt"));var item=hero.inventory.Single(i=>i.id==id);
                Require(item.sockets.Single().gemId==""&&item.sockets.Single().tier==0,"Damaged gem was not restored to empty");
                Require(File.Exists(game.Store.GemRecoveryArchive)&&File.ReadAllText(game.Store.GemRecoveryArchive)==File.ReadAllText(Path.Combine(directory,"corrupt-original.json")),"Recovery did not preserve original save bytes");
                Require(game.Notice.Contains(game.Store.GemRecoveryMessage)&&game.Store.GemRecoveryMessage!="","Recovery notice missing");
                game.UI.ShowTown();yield return Capture("recovery-notice-en",1280,720);game.UI.ShowItemDetail(id);yield return Capture("recovered-empty-socket",720,1280,"Socket ·");
            }
            else throw new InvalidOperationException("Unknown gem fixture stage");
            File.WriteAllText(Path.Combine(directory,stage+"-passed.txt"),"PASS: gem effect, socket, protection and save fixture "+stage+". Acquisition and service UI are not claimed.\n");Application.Quit(0);
        }
    }
}
