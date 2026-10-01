#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Hellscript
{
    // Synthetic, isolated native acceptance: actual domain transactions, not a human timing study.
    public sealed class RuntimeDailyQuestSmoke:MonoBehaviour
    {
        GameController game;string output,savePath;long now;int clicks,layouts,drags;uint rng=19007;
        ContentWindowView Panel=>game.UI.AttendancePanel;AccountSave A=>game.Store.Data;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void Boot()
        {
            if(!Debug.isDebugBuild||!Environment.GetCommandLineArgs().Contains("-hellscriptDailyQuestSmoke"))return;
            Application.runInBackground=true;Application.logMessageReceived+=(m,s,t)=>{if(t==LogType.Exception)Application.Quit(1);};
            new GameObject("Daily quest acceptance").AddComponent<RuntimeDailyQuestSmoke>();
        }
        static void Require(bool ok,string message){if(!ok)throw new InvalidOperationException(message);}
        static long Utc(string local)=>DateTimeOffset.Parse(local+"+09:00").ToUnixTimeMilliseconds();
        static Rect Bounds(RectTransform r){var c=new Vector3[4];r.GetWorldCorners(c);return Rect.MinMaxRect(c[0].x,c[0].y,c[2].x,c[2].y);}
        Button Find(string name)=>game.UI.GetComponentsInChildren<Button>().FirstOrDefault(b=>b.name==name);
        void Hide(){foreach(var kind in new[]{AttendanceKind.Weekly,AttendanceKind.Monthly})Require(game.AttendancePopups.SetHidden(kind,true,now),"Popup settings write failed");}
        IEnumerator Click(string name)
        {
            yield return new WaitForEndOfFrame();var b=Find(name);Require(b!=null&&b.IsInteractable(),"Missing/disabled "+name);Canvas.ForceUpdateCanvases();
            var data=new PointerEventData(EventSystem.current){position=Bounds((RectTransform)b.transform).center,button=PointerEventData.InputButton.Left};var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(data,hits);
            Require(hits.Count>0&&hits[0].gameObject.GetComponentInParent<Button>()==b,"Obscured "+name);ExecuteEvents.Execute(b.gameObject,data,ExecuteEvents.pointerClickHandler);clicks++;yield return new WaitForSecondsRealtime(.2f);
        }
        IEnumerator Resize(int w,int h)
        {Screen.SetResolution(w,h,FullScreenMode.Windowed);float until=Time.realtimeSinceStartup+8;while((Screen.width!=w||Screen.height!=h)&&Time.realtimeSinceStartup<until)yield return null;Require(Screen.width==w&&Screen.height==h,"Native resolution mismatch");yield return new WaitForSecondsRealtime(.3f);}
        IEnumerator Capture(string name)
        {yield return new WaitForEndOfFrame();var image=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(output,name+".png"),image.EncodeToPNG());Destroy(image);}
        static void Inside(Rect child,Rect parent,string message)
        {Require(child.xMin>=parent.xMin-1&&child.xMax<=parent.xMax+1&&child.yMin>=parent.yMin-1&&child.yMax<=parent.yMax+1,message);}
        void Hit(Button b)
        {
            var data=new PointerEventData(EventSystem.current){position=Bounds((RectTransform)b.transform).center};
            var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(data,hits);
            Require(hits.Count>0&&hits[0].gameObject.GetComponentInParent<Button>()==b,"Obscured daily button "+b.name);
        }
        void Geometry()
        {
            Canvas.ForceUpdateCanvases();var frame=Bounds(Panel.Frame);var viewport=Bounds(Panel.Scroll.viewport);
            Inside(frame,UiSafeArea.Current,"Daily frame outside safe area");
            Require(!Panel.Scroll.enabled&&!Panel.Body.GetComponent<VerticalLayoutGroup>().enabled&&!Panel.Body.GetComponent<ContentSizeFitter>().enabled,"Daily automatic scroll/layout is enabled");
            Require(Panel.Body.anchoredPosition==Vector2.zero,"Daily body moved");
            Require(!viewport.Overlaps(Bounds(Panel.Actions)),"Fixed body overlaps actions");
            foreach(var t in Panel.GetComponentsInChildren<Text>())
            {
                Require(t.preferredHeight<=t.rectTransform.rect.height+1,"Clipped daily text: "+t.text+" needs "+t.preferredHeight+" has "+t.rectTransform.rect.height);
                Inside(Bounds(t.rectTransform),frame,"Daily text outside frame: "+t.name);
                if(t.transform.IsChildOf(Panel.Body))Inside(Bounds(t.rectTransform),viewport,"Masked daily text: "+t.name);
                if(Loc.Language=="en")Require(!t.text.Any(c=>c>='가'&&c<='힣'),"Untranslated daily text: "+t.text);
            }
            foreach(var b in Panel.GetComponentsInChildren<Button>())
            {
                Inside(Bounds((RectTransform)b.transform),frame,"Daily action outside frame: "+b.name);
                if(b.transform.IsChildOf(Panel.Body))Inside(Bounds((RectTransform)b.transform),viewport,"Masked daily claim: "+b.name);
                Hit(b);
            }
            var cards=new List<Rect>();
            foreach(var q in A.dailyQuests.quests)
            {
                var b=Find("daily-quest-claim-"+q.id);Require(b!=null,"Missing daily claim "+q.id);
                Require(b.interactable==(!q.claimed&&q.progress>=q.target&&A.dailyQuests.day==DailyQuests.Day(now)),"Incorrect claim state "+q.id);
                var card=Bounds((RectTransform)b.transform.parent);Inside(card,viewport,"Masked daily card "+q.id);
                Require(!cards.Any(r=>r.Overlaps(card)),"Daily cards overlap");cards.Add(card);
                var labels=b.transform.parent.GetComponentsInChildren<Text>();
                for(int i=0;i<labels.Length;i++)for(int j=i+1;j<labels.Length;j++)
                    Require(!Bounds(labels[i].rectTransform).Overlaps(Bounds(labels[j].rectTransform)),"Daily labels overlap: "+labels[i].name+" / "+labels[j].name);
            }
            bool wide=Panel.Frame.rect.width>Panel.Frame.rect.height;
            if(cards.Count==5)
            {
                Require(wide?Mathf.Abs(cards[0].yMax-cards[2].yMax)<1&&cards[3].yMax<cards[0].yMin:cards.Zip(cards.Skip(1),(a,b)=>b.yMax<a.yMin).All(x=>x),"Daily direction layout mismatch");
                var rules=Panel.GetComponentsInChildren<RectTransform>().Single(r=>r.name=="Daily quest rules");
                Require(!cards.Any(r=>r.Overlaps(Bounds(rules))),"Rules overlap daily cards");Inside(Bounds(rules),viewport,"Masked rules");
            }
            var texts=Panel.GetComponentsInChildren<Text>();
            Require(texts.Single(t=>t.name=="daily-quest-claimed-coins").text==DailyQuests.ClaimedCoins(A.dailyQuests)+" / 100","Claimed summary does not use actual state");
            Require(texts.Single(t=>t.name=="daily-quest-goals").text==Loc.F("목표 달성 {0} / {1}",A.dailyQuests.quests.Count(q=>q.progress>=q.target),A.dailyQuests.quests.Count),"Goal summary does not use actual state");
            int ready=A.dailyQuests.day==DailyQuests.Day(now)?A.dailyQuests.quests.Where(q=>!q.claimed&&q.progress>=q.target).Sum(q=>q.reward):0;
            string status=DailyQuests.ClaimedCoins(A.dailyQuests)==100?Loc.T("오늘 보상 수령 완료"):ready>0?Loc.F("수령 가능 {0}개",ready):Loc.T("수령할 보상이 없습니다");
            Require(texts.Single(t=>t.name=="daily-quest-ready-coins").text==status,"Ready summary does not use actual state");
            layouts++;
        }
        IEnumerator Drag()
        {
            yield return new WaitForEndOfFrame();var before=Panel.Body.anchoredPosition;var actions=Bounds(Panel.Actions);
            var pos=Bounds(Panel.Scroll.viewport).center;var data=new PointerEventData(EventSystem.current){pointerId=2,position=pos,pressPosition=pos,button=PointerEventData.InputButton.Left};
            var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(data,hits);Require(hits.Count>0,"Daily body raycast missing");
            var target=ExecuteEvents.GetEventHandler<IDragHandler>(hits[0].gameObject);
            if(target!=null)
            {
                ExecuteEvents.Execute(target,data,ExecuteEvents.initializePotentialDrag);ExecuteEvents.Execute(target,data,ExecuteEvents.beginDragHandler);
                for(int i=0;i<8;i++){data.delta=Vector2.up*25;data.position+=data.delta;ExecuteEvents.Execute(target,data,ExecuteEvents.dragHandler);yield return null;}
                ExecuteEvents.Execute(target,data,ExecuteEvents.endDragHandler);
            }
            // Also challenge the disabled ScrollRect directly: it must reject the old scrolling route.
            Panel.Scroll.OnBeginDrag(data);data.position+=Vector2.up*200;Panel.Scroll.OnDrag(data);Panel.Scroll.OnEndDrag(data);
            yield return null;Require(Panel.Body.anchoredPosition==before&&Bounds(Panel.Actions)==actions,"Drag moved fixed daily body/actions");drags++;
        }
        void LayoutFixture(string state,string complete)
        {
            A.dailyQuests=state=="empty"?new DailyQuestState():JsonUtility.FromJson<DailyQuestState>(complete);
            for(int i=0;i<A.dailyQuests.quests.Count;i++)
            {
                var q=A.dailyQuests.quests[i];q.claimed=state=="claimed"||state=="mixed"&&(i==2||i==4);
                if(state=="start")q.progress=0;
                if(state=="mixed"&&(i==0||i==3))q.progress=q.target/2;
            }
            A.premium=DailyQuests.ClaimedCoins(A.dailyQuests);Panel.Repaint();
        }
        Item Gear(AccountSave a,string id)
        {var item=ItemGenerator.Create(a.Hero.heroClass,0,0,1,ref rng,id);a.Hero.inventory.Add(item);Storage.PlaceInBag(a.Hero,item);return item;}
        void CompleteActivities()
        {
            var store=game.Store;var sale=A.dailyQuests.quests.Single(q=>q.activity==DailyQuestActivity.SellEquipment);
            for(int i=0;i<sale.target;i++){int index=i;Require(store.Transact("daily-smoke-sale-"+i,"sale",a=>{var item=Gear(a,"daily-sale-"+index);return Economy.Sell(a,a.Hero,item);}),store.Error);}
            var item=Gear(A,"daily-enhancement");
            for(int i=0;i<2;i++){var owned=A.Hero.inventory.Single(x=>x.id==item.id);Require(store.EnhanceEquipment("daily-enhance-"+i,A.Hero.id,GearEnhancement.Quote(owned,A.gold,1)),store.Error);}
            for(int i=0;i<3;i++)Require(store.BuyTownEquipment("daily-smoke-buy-"+i,2),store.Error);
            A.Hero.potions.Activate();var sim=new CombatSimulation(A,game.catalog,1,seed:174);
            for(int i=0;i<10;i++){sim.State.resource=0;sim.State.potions.resourceCooldown=0;Require(sim.TryUseResourcePotion(),"Actual potion consumption failed");}
            int wins=A.dailyQuests.quests.Single(q=>q.activity==DailyQuestActivity.ClearRift).target;
            var finish=typeof(CombatSimulation).GetMethod("Finish",BindingFlags.NonPublic|BindingFlags.Instance);
            for(int i=0;i<wins;i++){var run=new CombatSimulation(A,game.catalog,1,seed:(uint)(300+i));finish.Invoke(run,new object[]{true,"균열 클리어",CombatFinish.Other});}
            Require(DailyQuests.Pending(A.dailyQuests)==5,"Actual domain events did not complete all five activities");Require(store.Save(),store.Error);Panel.Repaint();
        }
        IEnumerator Start()
        {
            var args=Environment.GetCommandLineArgs();for(int i=0;i<args.Length-1;i++){if(args[i]=="-hellscriptSavePath")savePath=args[i+1];if(args[i]=="-hellscriptScreenshots")output=args[i+1];}
            Require(!string.IsNullOrEmpty(output)&&!string.IsNullOrEmpty(savePath),"Explicit isolated paths required");Directory.CreateDirectory(output);yield return new WaitForSecondsRealtime(1);
            game=FindAnyObjectByType<GameController>();Require(game?.Store!=null,"Missing game store");game.enabled=false;
            now=Utc("2026-10-01T00:00:00");game.Store.AttendanceClock=()=>now;
            if(args.Contains("-hellscriptDailyQuestResume"))
            {
                Require(A.premium==100&&DailyQuests.ClaimedCoins(A.dailyQuests)==100&&A.dailyQuests.quests.All(q=>q.claimed),"Fresh player lost claimed quest rewards");
                foreach(var q in A.dailyQuests.quests)Require(game.Store.ClaimDailyQuest(q.id,A.dailyQuests.day),game.Store.Error);
                Require(A.premium==100,"Fresh player replay duplicated rewards");File.WriteAllText(Path.Combine(output,"restart.txt"),"PASS: fresh native player restored all five claims and exactly 100 coins; replay added zero coins. Synthetic isolated save.\n");Debug.Log("HELLSCRIPT_DAILY_QUEST_RESTART_OK");Application.Quit(0);yield break;
            }
            A.guide.legacyExempt=true;A.guide.hintsHidden=true;A.speed=1;A.gold=100000;A.Hero.highestClear=5;ContentUnlocks.Reconcile(A);
            now=Utc("2026-09-30T23:59:58");Hide();game.EnterPlaza(true);yield return new WaitForSecondsRealtime(.4f);game.UI.ShowAttendance(AttendanceKind.Weekly);yield return new WaitForSecondsRealtime(.2f);
            yield return Click("attendance-daily-quests");Require(Panel!=null&&Panel.Title.text==Loc.T("일일 퀘스트"),"Event tab did not open daily page");
            Require(Find("daily-quest-claim-sell-equipment")!=null&&!Find("daily-quest-claim-sell-equipment").interactable,"Incomplete quest can be claimed");Geometry();yield return Capture("initial-progress");
            long day=A.dailyQuests.day;now+=2000;Hide();yield return new WaitForSecondsRealtime(1.2f);Require(A.dailyQuests.day==day+1&&A.dailyQuests.quests.All(q=>q.progress==0),"Open daily page did not reset at attendance midnight");
            Require((int)typeof(GameUI).GetField("presented",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(game.UI)==0,"Daily page consumed an attendance popup at midnight");
            Require(A.dailyQuests.quests.Select(q=>q.target).SequenceEqual(new[]{30,20,2,500,10}),"Daily targets must be identical for every account and day");
            CompleteActivities();
            string complete=JsonUtility.ToJson(A.dailyQuests);
            string blocked=Path.Combine(savePath,"hellscript-local-v1.json.tmp");
            Directory.CreateDirectory(blocked);
            foreach(var size in new[]{(440,956),(956,440),(1440,810),(1440,900),(1680,720),(640,360)})foreach(string language in new[]{"ko","en"})
            {
                yield return Resize(size.Item1,size.Item2);game.ApplyLanguage(language);
                foreach(bool inset in new[]{false,true})
                {
                    if(inset)UiSafeArea.Simulate(size.Item1>size.Item2?32:18,20,18,24);else UiSafeArea.StopSimulating();
                    foreach(string state in new[]{"start","mixed","ready","claimed","empty"})
                    {
                        LayoutFixture(state,complete);yield return new WaitForEndOfFrame();
                        string unchanged=JsonUtility.ToJson(A.dailyQuests);int coins=A.premium;
                        Geometry();yield return Drag();
                        Require(JsonUtility.ToJson(A.dailyQuests)==unchanged&&A.premium==coins,"Layout/drag changed daily account state");
                        File.AppendAllText(Path.Combine(output,"layouts.txt"),$"PASS {size.Item1}x{size.Item2} {language} safe={inset} state={state} cards={A.dailyQuests.quests.Count} claimed={coins}\n");
                        if(!inset&&state=="mixed")yield return Capture("daily-"+size.Item1+"x"+size.Item2+"-"+language);
                    }
                }
            }
            Directory.Delete(blocked);UiSafeArea.StopSimulating();A.dailyQuests=JsonUtility.FromJson<DailyQuestState>(complete);A.premium=0;
            game.ApplyLanguage("ko");yield return Resize(440,956);Panel.Repaint();
            // Failed claims preserve progress, currency, ready summary and button availability.
            string failedBefore=JsonUtility.ToJson(A.dailyQuests);Directory.CreateDirectory(blocked);
            yield return Click("daily-quest-claim-sell-equipment");
            Require(JsonUtility.ToJson(A.dailyQuests)==failedBefore&&A.premium==0,"Failed claim changed progress/coins");
            Require(Find("daily-quest-claim-sell-equipment").interactable,"Failed claim lost availability");Geometry();yield return Capture("claim-save-failure");Directory.Delete(blocked);
            foreach(string id in A.dailyQuests.quests.Select(q=>q.id).ToArray())yield return Click("daily-quest-claim-"+id);
            Require(A.premium==100&&DailyQuests.ClaimedCoins(A.dailyQuests)==100,"Daily rewards are not exactly 100 coins");
            Require(A.dailyQuests.quests.All(q=>!Find("daily-quest-claim-"+q.id).interactable),"Claimed quest remains enabled");Geometry();yield return Capture("claimed-all");
            yield return Click("daily-attendance-1");Require(Panel.Frame.name=="Attendance Monthly","Daily-to-attendance navigation failed");yield return Click("attendance-daily-quests");
            yield return Click("daily-attendance-0");Require(Panel.Frame.name=="Attendance Weekly","Daily-to-weekly navigation failed");yield return Click("attendance-daily-quests");
            string before=JsonUtility.ToJson(A.dailyQuests);now-=86400000;yield return Click("daily-quest-refresh");Require(JsonUtility.ToJson(A.dailyQuests)==before&&A.premium==100,"Clock rollback changed progress or wallet");now+=86400000;Hide();
            // A failed preparation must present a recoverable empty page and preserve the saved claims.
            yield return Click("content-close");var savedState=A.dailyQuests;A.dailyQuests=new DailyQuestState();now+=86400000;
            Directory.CreateDirectory(blocked);game.UI.ShowDailyQuests();yield return null;
            Require(A.dailyQuests.day==-1&&A.dailyQuests.quests.Count==0,"Failed preparation mutated empty state");Geometry();yield return Capture("empty-save-failure");
            Directory.Delete(blocked);yield return Click("daily-quest-refresh");
            Require(A.dailyQuests.day==DailyQuests.Day(now)&&A.dailyQuests.quests.Count==5&&A.dailyQuests.quests.All(q=>q.progress==0)&&A.premium==100,"Preparation retry failed or granted coins");
            now-=86400000;A.dailyQuests=savedState;Panel.Repaint();
            var host=Panel.GetComponentInParent<ContentWindowHost>();Require(host!=null&&host.Back(),"Shared Back did not close daily window");yield return null;Require(Panel==null,"Daily page did not close");Require(game.Store.Save(),game.Store.Error);
            File.WriteAllText(Path.Combine(output,"initial.txt"),"PASS: event tab navigation; five successful domain activity hooks; incomplete/claimed button states; 100 manual coins; attendance midnight rollover; clock rollback; "+clicks+" raycast-verified clicks, "+layouts+" KO/EN native layouts across five states and safe insets; "+drags+" immobile body drags, all cards/actions visible and raycast-reachable, failed-claim rollback, fixed actions, no clipped text. Synthetic fixtures do not establish actual 30-minute human play time or physical mobile input.\n");
            Debug.Log("HELLSCRIPT_DAILY_QUEST_RUNTIME_OK");Application.Quit(0);
        }
    }
}
#endif
