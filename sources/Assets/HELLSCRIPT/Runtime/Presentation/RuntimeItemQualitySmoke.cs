using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Hellscript
{
    // Explicit development-only fixtures. Progress, funds and equipment are seeded on an isolated save.
    public sealed class RuntimeItemQualitySmoke:MonoBehaviour
    {
        GameController game;string directory,saveDirectory,stage,itemId;int capture;uint rng=981327;
        [Serializable] sealed class Equipment {public List<Item> items;}
        static string Json(object value)=>JsonUtility.ToJson(value,true);
        static void Require(bool value,string message){if(!value)throw new InvalidOperationException(message);}
        Item Item=>game.Store.Data.Hero.inventory.Single(i=>i.id==itemId);
        string Texts=>string.Join("\n",game.UI.GetComponentsInChildren<Text>().Select(t=>t.text));
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void Install()
        {
            if(!Debug.isDebugBuild||!Environment.GetCommandLineArgs().Contains("-hellscriptItemQualitySmoke"))return;
            Application.runInBackground=true;Application.logMessageReceived+=(m,s,t)=>{if(t==LogType.Exception)Application.Quit(1);};
            new GameObject("Item quality verification").AddComponent<RuntimeItemQualitySmoke>();
        }
        Button Button(string name)=>game.UI.GetComponentsInChildren<Button>().Single(b=>b.name==name);
        void Click(string name){var b=Button(name);Require(b.IsInteractable(),"Disabled quality command: "+name);b.onClick.Invoke();}
        void OpenMasterwork(){game.UI.ShowItemDetail(itemId);Click("걸작 · 진행과 초기화");}
        void QuoteNext()
        {int next=Item.masterwork+1;Click(Loc.F("걸작 {0}단계 · 재료 {1:N0} / 골드 {2:N0}",next,ItemQuality.Materials(next),ItemQuality.Gold(next)));}
        IEnumerator Capture(string name,int width,int height,string focus=null)
        {
            Debug.developerConsoleVisible=false;
            Screen.SetResolution(width,height,FullScreenMode.Windowed);float deadline=Time.realtimeSinceStartup+6;
            while((Screen.width!=width||Screen.height!=height)&&Time.realtimeSinceStartup<deadline)yield return null;
            Require(Screen.width==width&&Screen.height==height,"Quality fixture resize failed");yield return new WaitForSecondsRealtime(.3f);
            if(focus!=null)
            {
                var label=game.UI.GetComponentsInChildren<Text>().First(t=>t.text.Contains(focus));
                DialogReadingAnchor.Show(label.rectTransform);yield return new WaitForSecondsRealtime(.15f);
            }
            Canvas.ForceUpdateCanvases();
            foreach(var label in game.UI.GetComponentsInChildren<Text>())Require(label.preferredHeight<=label.rectTransform.rect.height+2,"Clipped quality text: "+label.text);
            Require(Loc.MissingCount==0,"Missing quality translations: "+string.Join("; ",Loc.Missing));
            string prefix=stage+"-"+(++capture).ToString("00")+"-"+name;
            File.WriteAllText(Path.Combine(directory,prefix+".txt"),Texts);
            ScreenCapture.CaptureScreenshot(Path.Combine(directory,prefix+".png"));yield return new WaitForSecondsRealtime(.2f);
        }
        IEnumerator Start()
        {
            var args=Environment.GetCommandLineArgs();for(int i=0;i<args.Length-1;i++)
            {if(args[i]=="-hellscriptSavePath")saveDirectory=args[i+1];if(args[i]=="-hellscriptScreenshots")directory=args[i+1];if(args[i]=="-hellscriptQualityStage")stage=args[i+1];}
            Require(!string.IsNullOrEmpty(saveDirectory)&&!string.IsNullOrEmpty(directory)&&!string.IsNullOrEmpty(stage),"Use explicit isolated quality paths and stage");
            Directory.CreateDirectory(directory);yield return new WaitForSecondsRealtime(1);
            game=FindAnyObjectByType<GameController>();Require(game?.Store!=null,"Quality fixture account did not load");game.enabled=false;
            var account=game.Store.Data;var hero=account.Hero;
            if(stage=="initial")
            {
                hero.level=30;hero.highestClear=30;hero.inventory.Clear();hero.build=GameCatalog.Preset(HeroClass.Warrior,0);hero.build.autoRepeat=false;hero.useEdict=false;
                account.guide.hintsHidden=true;account.gold=2000000;account.materials=100000;ContentUnlocks.Reconcile(account);
                Item item;do{item=ItemGenerator.Create(HeroClass.Warrior,0,3,30,ref rng,riftStage:30);}while(!item.awakened);
                item.equipped=true;hero.inventory.Add(item);itemId=item.id;
                for(int n=0;n<5;n++)Require(Economy.Enhance(account,item),"Fixture enhancement failed");
                var greater=item.rolls[0];greater.greater=true;greater.rollBasisPoints=10000;greater.tierId="T1";greater.value=ItemCatalog.Affix(greater.affixId).Value(item.level,10000);
                item.sockets.Add(new SocketState{index=0,gemId="G06",tier=6});
                for(int n=0;n<3;n++)Require(ItemQuality.Advance(account,item,ref rng),"Fixture masterwork failed");
                var candidate=JsonUtility.FromJson<Item>(Json(item));candidate.id=Guid.NewGuid().ToString("N");candidate.equipped=false;candidate.awakened=false;
                candidate.masterwork=0;candidate.masterworkLines.Clear();candidate.investedMaterials-=candidate.masterworkInvestedMaterials;candidate.masterworkInvestedMaterials=0;
                candidate.sockets.Clear();foreach(var roll in candidate.rolls)roll.greater=false;hero.inventory.Add(candidate);game.Save();
                File.WriteAllText(Path.Combine(directory,"item-id.txt"),itemId);
                game.ApplyLanguage("ko");game.UI.ShowItemDetail(itemId);yield return Capture("quality-detail-ko",1920,1080,"각성");
                OpenMasterwork();yield return Capture("masterwork-ko",1280,720);
                game.ApplyLanguage("en");game.ApplyInterfaceScale(140);OpenMasterwork();yield return Capture("masterwork-en-portrait",720,1280,"Blacksmith");
                yield return Capture("masterwork-en-short-landscape",640,360,"Masterwork 4");
                QuoteNext();yield return Capture("milestone-confirm-en",720,1280);
                var commit=Button("확인").onClick;int gold=account.gold,materials=account.materials;
                commit.Invoke();commit.Invoke();yield return null;
                Require(Item.masterwork==4&&Item.masterworkLines.Count==1&&account.gold==gold-800&&account.materials==materials-28,"Milestone double click charged twice or lost its affix boost");

                OpenMasterwork();QuoteNext();commit=Button("확인").onClick;
                string path=Path.Combine(saveDirectory,"hellscript-local-v1.json"),disk=File.ReadAllText(path),before=Json(Item);gold=account.gold;materials=account.materials;
                Directory.CreateDirectory(path+".tmp");commit.Invoke();yield return null;
                Require(Json(Item)==before&&account.gold==gold&&account.materials==materials&&File.ReadAllText(path)==disk,"Failed quality save changed equipment or funds");
                Require(!Texts.Contains(saveDirectory),"The player-facing failure message exposed an internal path");
                yield return Capture("save-failure-en",1280,720);
                Directory.Delete(path+".tmp");commit.Invoke();yield return null;Require(Item.masterwork==5&&account.gold==gold-1000&&account.materials==materials-30,"Quality retry failed");

                OpenMasterwork();QuoteNext();commit=Button("확인").onClick;
                Require(game.Store.Transact("quality-external-six","fixture-masterwork-six",a=>
                {uint random=7711;return ItemQuality.Advance(a,a.Hero.inventory.Single(i=>i.id==itemId),ref random);}),"Concurrent fixture change failed");
                before=Json(Item);gold=account.gold;materials=account.materials;commit.Invoke();yield return null;
                Require(Json(Item)==before&&account.gold==gold&&account.materials==materials,"A stale quality confirmation changed the newer item");
                for(int n=7;n<=12;n++){OpenMasterwork();QuoteNext();Click("확인");yield return null;Require(Item.masterwork==n,"Quality UI missed a step");}
                Require(Item.masterworkLines.Count==3,"Quality UI did not retain three milestones");
                OpenMasterwork();Require(!game.UI.GetComponentsInChildren<Button>().Any(b=>b.name.StartsWith("Masterwork 13",StringComparison.Ordinal)),"The account cap offered another step");
                yield return Capture("cap-and-affix-boosts-en",720,1280);
                Click(Loc.F("걸작 초기화 · 골드 {0:N0}",ItemQuality.ResetGold(Item)));yield return Capture("reset-confirm-en",1280,720);
                gold=account.gold;materials=account.materials;Click("확인");yield return null;
                Require(Item.masterwork==0&&Item.masterworkLines.Count==0&&Item.enhancement==5&&Item.awakened&&Item.rolls[0].greater&&Item.sockets[0].gemId=="G06","Reset destroyed a retained equipment layer");
                Require(account.gold==gold-60000&&account.materials==materials+316&&Item.investedMaterials==620,"Reset mixed enhancement and masterwork materials");
                for(int n=1;n<=4;n++){OpenMasterwork();QuoteNext();Click("확인");yield return null;}
                game.UI.ShowItemDetail(itemId);Click("한 줄 재설정");Require(Texts.Contains("Greater"),"Greater reroll warning missing before row selection");
                var reroll=game.UI.GetComponentsInChildren<Button>().First(b=>b.name.StartsWith(Loc.T("◆ 상위 ·")+" ",StringComparison.Ordinal));reroll.onClick.Invoke();
                yield return Capture("greater-reroll-warning-en",720,1280);gold=account.gold;
                Require(Texts.Contains("removes"),"The greater loss warning is missing from confirmation");Click("확인");yield return null;
                Require(!Item.rolls[0].greater&&Item.rolls[0].rollBasisPoints>=4000&&Item.masterwork==4&&account.gold==gold-Economy.RerollGold(Item),"Reroll lost the awakened floor or masterwork");
                game.ApplyInterfaceScale(100);game.UI.ShowItemDetail(candidate.id);
                yield return new WaitForSecondsRealtime(4.1f);
                yield return Capture("quality-comparison-en",1920,1080,"Gem effect removed");
                Click("필터");Click("각성 장비만 · 꺼짐");Click("목록 보기");Click("목록");yield return null;
                Require(!game.UI.GetComponentsInChildren<Button>().Any(b=>b.name=="Inventory item "+candidate.id),"Awakened filter retained the plain candidate");
                Require(game.UI.GetComponentsInChildren<Button>().Any(b=>b.name=="Inventory item "+itemId),"Awakened filter hid its matching equipment");
                yield return Capture("awakened-filter-en",720,1280);
                int previousClear=hero.highestClear;hero.highestClear=100;game.UI.ShowShop();yield return null;
                Require(Texts.Contains(Loc.F("골드 {0:N0} · 재료 {1:N0}\n생성 아이템 레벨 {2} · 최고 실클리어 기준",account.gold,account.materials,60)),"Shop advertised a level above the generation cap");
                yield return Capture("shop-level-cap-en",1280,720);hero.highestClear=previousClear;game.ApplyInterfaceScale(140);game.Save();
                File.WriteAllText(Path.Combine(directory,"expected-items.json"),Json(new Equipment{items=hero.inventory}));
            }
            else
            {
                itemId=File.ReadAllText(Path.Combine(directory,"item-id.txt"));
                if(stage=="restart")
                {
                    Require(Json(new Equipment{items=hero.inventory})==File.ReadAllText(Path.Combine(directory,"expected-items.json")),"Restart changed equipment quality");
                    Require(game.Store.QualityRecoveryMessage=="","Valid quality data triggered recovery");OpenMasterwork();yield return Capture("restored-masterwork-en",1280,720);
                }
                else if(stage=="recover")
                {
                    Require(Item.masterwork==0&&Item.masterworkLines.Count==0&&!Item.rolls[0].greater,"Invalid quality was not repaired");
                    Require(File.ReadAllText(game.Store.QualityRecoveryArchive)==File.ReadAllText(Path.Combine(directory,"corrupt-original.json")),"Original quality save was not archived exactly");
                    Require(game.Notice.Contains(game.Store.QualityRecoveryMessage)&&game.Store.QualityRecoveryMessage!="","Quality recovery notice missing");
                    game.UI.ShowTown();yield return Capture("quality-recovery-notice-en",1280,720);OpenMasterwork();yield return Capture("recovered-investment-en",720,1280);
                }
                else throw new InvalidOperationException("Unknown quality fixture stage");
            }
            File.WriteAllText(Path.Combine(directory,stage+"-passed.txt"),"PASS: native quality UI, service transactions and persistence fixture "+stage+". Seeded fixtures do not measure natural progression.\n");Application.Quit(0);
        }
    }
}
