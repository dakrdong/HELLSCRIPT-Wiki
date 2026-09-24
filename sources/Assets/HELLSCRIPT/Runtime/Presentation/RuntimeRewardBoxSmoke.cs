using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Hellscript
{
    public sealed class RuntimeRewardBoxSmoke:MonoBehaviour
    {
        GameController game;string output;readonly List<string> checks=new List<string>();
        static void Require(bool value,string message){if(!value)throw new InvalidOperationException(message);}
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void Boot()
        {
            if(!Debug.isDebugBuild||!Environment.GetCommandLineArgs().Contains("-hellscriptRewardBoxSmoke"))return;
            Application.runInBackground=true;new GameObject("Native reward box acceptance").AddComponent<RuntimeRewardBoxSmoke>();
        }
        void OnEnable()=>Application.logMessageReceived+=Error;
        void OnDisable()=>Application.logMessageReceived-=Error;
        void Error(string message,string trace,LogType type)
        {if(type!=LogType.Exception)return;if(output!=null)File.WriteAllText(Path.Combine(output,"failure.txt"),message+"\n"+trace);Application.Quit(1);}
        Button Find(string name)=>game.UI.GetComponentsInChildren<Button>().FirstOrDefault(b=>b.name==name&&b.gameObject.activeInHierarchy);
        static Rect Bounds(RectTransform r){var v=new Vector3[4];r.GetWorldCorners(v);return Rect.MinMaxRect(v[0].x,v[0].y,v[2].x,v[2].y);}
        IEnumerator Tap(Button button)
        {
            Require(button!=null,"Missing pointer target");Require(button.interactable,"Disabled target: "+button.name);
            string targetName=button.name;var rect=(RectTransform)button.transform;var scroll=button.GetComponentInParent<ScrollRect>();
            if(scroll!=null)
            {
                Canvas.ForceUpdateCanvases();float delta=Bounds(scroll.viewport).center.y-Bounds(rect).center.y;
                var scale=button.GetComponentInParent<Canvas>().scaleFactor;var p=scroll.content.anchoredPosition;p.y=Mathf.Clamp(p.y+delta/scale,0,Mathf.Max(0,scroll.content.rect.height-scroll.viewport.rect.height));scroll.content.anchoredPosition=p;
                yield return null;yield return null;
                // Scrolling a pooled list can recycle the original button into another tier.
                button=Find(targetName);Require(button!=null,"Recycled target disappeared: "+targetName);rect=(RectTransform)button.transform;
            }
            var pointer=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,rect.TransformPoint(rect.rect.center)),button=PointerEventData.InputButton.Left};var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(pointer,hits);
            Require(hits.Count>0&&hits[0].gameObject.GetComponentInParent<Button>()==button,"Pointer blocked: "+button.name);
            ExecuteEvents.Execute(button.gameObject,pointer,ExecuteEvents.pointerDownHandler);ExecuteEvents.Execute(button.gameObject,pointer,ExecuteEvents.pointerUpHandler);ExecuteEvents.Execute(button.gameObject,pointer,ExecuteEvents.pointerClickHandler);
            yield return null;yield return null;
        }
        IEnumerator Tap(string name)=>Tap(Find(name));
        ContentWindowView Top()=>game.UI.GetComponentsInChildren<ContentWindowView>().OrderByDescending(v=>v.GetComponent<Canvas>().sortingOrder).First();
        IEnumerator ClosePopup()=>Tap(Top().GetComponentsInChildren<Button>().Single(b=>b.name=="content-close"));
        IEnumerator Capture(string name)
        {Canvas.ForceUpdateCanvases();yield return new WaitForEndOfFrame();var texture=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(output,name+".png"),texture.EncodeToPNG());Destroy(texture);}
        IEnumerator Resize(int w,int h,string language,int scale)
        {
            game.ApplyLanguage(language);game.InterfaceScale.Apply(scale);game.UI.ApplyInterfaceScale();Screen.SetResolution(w,h,FullScreenMode.Windowed);float until=Time.realtimeSinceStartup+8;
            while((Screen.width!=w||Screen.height!=h)&&Time.realtimeSinceStartup<until)yield return null;Require(Screen.width==w&&Screen.height==h,"Resolution mismatch");yield return new WaitForSecondsRealtime(.3f);
        }
        IEnumerator Start()
        {
            var args=Environment.GetCommandLineArgs();int save=Array.IndexOf(args,"-hellscriptSavePath");
            for(int i=0;i<args.Length-1;i++)if(args[i]=="-hellscriptEvidencePath")output=args[i+1];
            Require(output!=null&&save>=0,"Isolated save and evidence paths required");Directory.CreateDirectory(output);
            yield return null;game=FindAnyObjectByType<GameController>();Require(game?.Store!=null,"Missing runtime store");
            game.EnterPlaza(true);var a=game.Store.Data;
            foreach(int stage in new[]{1,10,30,100,1000})a.Hero.riftProgress.best.Add(new RiftBestTime{stage=stage,milliseconds=123000});
            a.Hero.highestClear=1000;Require(game.Store.Save(),game.Store.Error);game.SelectedStage=10;game.UI.ShowRiftKeeper();yield return null;yield return null;
            yield return Tap("rift-first-rewards");var list=FindAnyObjectByType<RiftRewardList>();Require(list!=null&&list.RowPoolCount<30,"First-clear list not bounded");
            yield return Tap("rift-reward-tier-10");Require(Find("reward-box-claim-10").interactable,"Normal clear not claimable");yield return Capture("first-clear-10");
            int gold=a.gold,materials=a.materials;yield return Tap("reward-box-claim-10");
            Require(a.rewardBoxes.claimedStages.Contains(10)&&a.rewardBoxes.owned.Sum(b=>b.count)==5,"R10 claim did not persist its five boxes");
            Require(a.rewardBoxes.owned.All(b=>!b.boxId.StartsWith("legendary-")&&!b.boxId.StartsWith("set-")),"R10 granted late-tier equipment");
            Require(a.gold==gold&&a.materials==materials,"Claim duplicated automatic currency");Require(!Find("reward-box-claim-10").interactable,"Duplicate claim still enabled");
            checks.Add("PASS real pointer: R10 normal-clear claim -> five boxes, no legendary/set reward, account receipt, no duplicated automatic currency.");
            yield return Tap("reward-box-inventory");yield return Tap("reward-box-filter-2");
            Require(!Find("reward-box-open-one").interactable,"Choice box opened without selection");
            yield return Tap("reward-box-choice-G04");yield return Tap("reward-box-open-one");Require(GemStacks.Count(a.gems,"G04",1)==10,"Choice box did not grant ten emeralds");
            checks.Add("PASS real pointer: choose emerald -> consume one box -> exactly ten T1 gems.");
            while(game.UI.GetComponentsInChildren<ContentWindowView>().Length>0)yield return ClosePopup();
            game.SelectedStage=30;game.UI.ShowRiftKeeper();yield return null;yield return Tap("rift-first-rewards");
            yield return Tap("rift-reward-tier-30");yield return Tap("reward-box-claim-30");
            Require(a.rewardBoxes.claimedStages.Contains(30)&&a.rewardBoxes.owned.Any(b=>b.boxId=="legendary-weapon"&&b.sourceStage==30&&b.count==1),"R30 legendary weapon box absent");
            yield return Capture("first-clear-30");yield return Tap("reward-box-inventory");
            yield return Tap("reward-box-filter-1");yield return Tap("reward-box-open-one");
            Require(Top().Source==EquipmentViewSource.RewardSnapshot&&Top().GetComponentInChildren<ItemDetailView>()!=null,"Equipment result bypasses shared detail");
            Require(a.Hero.inventory.Any(i=>i.rarity==3&&i.slot==0&&i.lootClass==a.Hero.heroClass),"Legendary weapon absent");yield return Capture("legendary-weapon-result");yield return ClosePopup();
            yield return Tap("reward-box-filter-3");
            var coin=a.rewardBoxes.owned.Single(b=>b.boxId=="premium-100");yield return Tap("reward-box-select-"+coin.id);int premium=a.premium;yield return Tap("reward-box-open-one");Require(a.premium==premium+100,"Coin box did not credit existing wallet");
            checks.Add("PASS real pointer: legendary weapon + shared RewardSnapshot detail; coin box credits account wallet.");
            while(game.UI.GetComponentsInChildren<ContentWindowView>().Length>0)yield return ClosePopup();
            game.EnterPlaza(true);game.UI.ShowPlayInventory();yield return null;yield return null;yield return Tap("inventory-reward-boxes");
            checks.Add("PASS inventory footer opens the reward box inventory through the common window host.");
            foreach(var size in new[]{new[]{440,956},new[]{956,440},new[]{1600,900},new[]{1600,1000},new[]{2100,900}})
            foreach(string language in new[]{"ko","en"})foreach(int scale in new[]{100,150})
            {
                yield return Resize(size[0],size[1],language,scale);var v=Top();Canvas.ForceUpdateCanvases();var frame=Bounds(v.Frame);
                foreach(string name in new[]{"reward-box-open-one","reward-box-open-batch","reward-box-filter-0","reward-box-filter-4"})
                {var b=Find(name);Require(b!=null,name);var r=Bounds((RectTransform)b.transform);Require(frame.Contains(r.min+Vector2.one)&&frame.Contains(r.max-Vector2.one),"Outside safe frame: "+name);}
                foreach(var t in v.GetComponentsInChildren<Text>().Where(t=>t.name=="Reward box text"||t.name=="Button text"))
                    Require(t.resizeTextForBestFit||t.preferredHeight<=t.rectTransform.rect.height+2,"Truncated reward text: "+t.text+" at "+size[0]+" "+language+" "+scale);
                foreach(var icon in v.GetComponentsInChildren<Image>().Where(i=>i.name.StartsWith("Reward box icon")))Require(icon.sprite!=null,"Missing sprite");
                checks.Add($"PASS layout {size[0]}x{size[1]} {language} {scale}%");
                if(language=="ko"&&scale==100&&size[0]==440||language=="en"&&scale==150&&size[0]==956)yield return Capture($"boxes-{size[0]}-{language}-{scale}");
            }
            yield return ClosePopup();game.UI.ClosePlayInventory();yield return Resize(440,956,"ko",100);game.SelectedStage=1000;game.UI.ShowRiftKeeper();yield return null;yield return Tap("rift-first-rewards");
            list=FindAnyObjectByType<RiftRewardList>();Require(list.LastVisible==1000&&list.RowPoolCount<30,"Cannot reach tier 1000");yield return Tap("rift-reward-tier-1000");yield return Capture("first-clear-1000");yield return Tap("reward-box-claim-1000");
            Require(a.rewardBoxes.owned.Any(b=>b.boxId=="gem-choice-t6"&&b.count==2),"T6 packs missing");Require(a.rewardBoxes.owned.Any(b=>b.boxId=="awakened-weapon"&&b.minimumQuality==9000),"Completion weapon missing");
            var loaded=new GameStore(args[save+1],game.catalog);Require(JsonUtility.ToJson(a.rewardBoxes)==JsonUtility.ToJson(loaded.Data.rewardBoxes),"Boxes did not survive reload");
            checks.Add("PASS tier-1000 view + claim, two ten-gem T6 boxes, 90% awakened weapon, save/reload preserves unopened contents.");
            File.WriteAllLines(Path.Combine(output,"runtime.txt"),checks);Debug.Log("HELLSCRIPT_REWARD_BOX_RUNTIME_OK");Application.Quit(0);
        }
    }
}
