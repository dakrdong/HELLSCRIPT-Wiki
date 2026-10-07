#if UNITY_EDITOR || DEVELOPMENT_BUILD
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
    // Development-build acceptance only. Always run against a disposable -hellscriptSavePath.
    public sealed partial class RuntimeTutorialSmoke:MonoBehaviour
    {
        GameController game;string output;readonly List<string> checks=new List<string>();
        static void Require(bool ok,string message){if(!ok)throw new InvalidOperationException(message);}
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Boot()
        {
            var args=Environment.GetCommandLineArgs();if(!Debug.isDebugBuild||!args.Contains("-hellscriptTutorialSmoke"))return;
            Require(args.Contains("-hellscriptSavePath"),"A disposable save path is required.");
            int server=Array.IndexOf(args,"-hellscriptTutorialLiveOps");
            if(server>=0)
            {
                Require(server+1<args.Length&&Uri.TryCreate(args[server+1],UriKind.Absolute,out _),"Loopback live operations endpoint required.");
                var endpoint=new Uri(args[server+1]);Require(endpoint.IsLoopback&&endpoint.Scheme=="http","Use an isolated loopback fixture.");
                var owner=FindAnyObjectByType<GameController>();Require(owner?.LiveOps!=null,"Live operations bootstrap missing.");owner.LiveOps.Configure(endpoint);
            }
            Application.runInBackground=true;new GameObject("Tutorial acceptance").AddComponent<RuntimeTutorialSmoke>();
        }
        void OnEnable()=>Application.logMessageReceived+=Error;
        void OnDisable()=>Application.logMessageReceived-=Error;
        void Error(string text,string trace,LogType type){if(type!=LogType.Exception)return;if(output!=null)File.WriteAllText(Path.Combine(output,"failure.txt"),text+"\n"+trace);Application.Quit(1);}
        Button Find(string name)=>game.UI.GetComponentsInChildren<Button>().FirstOrDefault(b=>b.name==name&&b.gameObject.activeInHierarchy);
        static Rect Bounds(RectTransform r){var v=new Vector3[4];r.GetWorldCorners(v);return Rect.MinMaxRect(v[0].x,v[0].y,v[2].x,v[2].y);}
        // A committed save can rebuild a list or a redraw can replace a control between frames: take it again before the pointer lands.
        // A guide's caption band rebuilds the window within a quarter second of it opening, so a pointer that misses gets a short time to land.
        IEnumerator Tap(Component b,Func<Component> again=null,Vector2? screenPoint=null,Action duringPress=null)
        {
            yield return null;yield return null;Canvas.ForceUpdateCanvases();if(b==null&&again!=null)b=again();
            Require(b!=null&&(!(b is Button button)||button.interactable),"Missing or disabled pointer target: "+b?.name);var rect=(RectTransform)b.transform;var scroll=b.GetComponentInParent<ScrollRect>();
            if(scroll!=null)
            {
                Canvas.ForceUpdateCanvases();float delta=Bounds(scroll.viewport).center.y-Bounds(rect).center.y;
                var p=scroll.content.anchoredPosition;p.y=Mathf.Clamp(p.y+delta/b.GetComponentInParent<Canvas>().scaleFactor,0,Mathf.Max(0,scroll.content.rect.height-scroll.viewport.rect.height));scroll.content.anchoredPosition=p;yield return null;yield return null;
                if(again!=null){b=again();Require(b!=null,"Pointer target vanished while scrolling");rect=(RectTransform)b.transform;}
            }
            PointerEventData e;var hits=new List<RaycastResult>();float settle=Time.realtimeSinceStartup+.6f;
            while(true)
            {
                e=new PointerEventData(EventSystem.current){position=screenPoint??RectTransformUtility.WorldToScreenPoint(null,rect.TransformPoint(rect.rect.center)),button=PointerEventData.InputButton.Left};hits.Clear();EventSystem.current.RaycastAll(e,hits);
                if(hits.Count>0&&ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject)==b.gameObject||Time.realtimeSinceStartup>=settle)break;
                yield return null;Canvas.ForceUpdateCanvases();if(b==null&&again!=null)b=again();
                Require(b!=null,"Pointer target vanished while its layout settled");if(again!=null){var fresh=again();if(fresh!=null)b=fresh;}rect=(RectTransform)b.transform;
            }
            if(!(hits.Count>0&&ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject)==b.gameObject))
            {
                // Leave what a screenshot and a hit list can show before the exception ends the run.
                var box=new Vector3[4];rect.GetWorldCorners(box);
                File.WriteAllText(Path.Combine(output,"pointer-blocked.txt"),b.name+" at "+e.position+" corners="+string.Join(" ",box.Select(c=>c.ToString("F0")))+" scroll="+(scroll!=null?scroll.name+" viewport="+Bounds(scroll.viewport)+" content="+scroll.content.anchoredPosition:"none")+"\nhits="+string.Join(" | ",hits.Take(10).Select(h=>h.gameObject.name+" order="+h.sortingOrder+" parent="+h.gameObject.transform.parent?.name))+"\nwindows="+string.Join(",",game.UI.GetComponentsInChildren<ContentWindowView>().Select(v=>v.name)));
                yield return Capture("pointer-blocked");
            }
            Require(hits.Count>0&&ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject)==b.gameObject,"Pointer blocked: "+b.name+" at "+e.position+" hits="+string.Join(",",hits.Take(6).Select(h=>h.gameObject.name+":"+h.sortingOrder)));
            ExecuteEvents.Execute(b.gameObject,e,ExecuteEvents.pointerDownHandler);
            if(duringPress!=null){duringPress();yield return null;yield return null;Require(b!=null&&b.gameObject.activeInHierarchy,"Save replaced a held pointer target.");}
            ExecuteEvents.Execute(b.gameObject,e,ExecuteEvents.pointerUpHandler);ExecuteEvents.Execute(b.gameObject,e,ExecuteEvents.pointerClickHandler);yield return null;yield return null;
        }
        IEnumerator Tap(string name){yield return null;var button=Find(name);Require(button!=null,"Missing button: "+name+" page="+game.UI.Page);yield return Tap(button,()=>Find(name));}
        string EconomySnapshot(AccountSave a)=>a.gold+"|"+a.materials+"|"+a.premium+"|"+string.Join(",",a.cores)+"|"+JsonUtility.ToJson(a.runes)+"|"+a.Hero.level+"|"+a.Hero.xp+"|"+a.Hero.highestClear+"|"+a.records.Count+"|"+string.Join(";",a.Hero.inventory.Select(JsonUtility.ToJson));
        IEnumerator Until(Func<bool> test,float seconds)
        {float end=Time.realtimeSinceStartup+seconds;while(!test()&&Time.realtimeSinceStartup<end)yield return null;var r=game.Combat?.State;
            if(!test()){yield return Capture("timeout");File.WriteAllText(Path.Combine(output,"timeout-windows.txt"),string.Join("\n",game.UI.GetComponentsInChildren<ContentWindowView>().Select(v=>v.name+":"+v.Title.text))+"\nhost="+game.UI.GetComponent<ContentWindowHost>()?.Count+" common="+game.UI.CommonPanelOpen+" inventory="+game.UI.PlayInventoryOpen+" staging="+game.UI.Staging.Busy+" dimmed="+game.DisplayDimmed+" enabled="+game.UI.isActiveAndEnabled+" prompt="+typeof(GameUI).GetField("tutorialPromptPhase",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic)?.GetValue(game.UI)+" journal="+game.UI.GetComponentsInChildren<Button>().Where(b=>b.gameObject.activeInHierarchy).Take(30).Select(b=>b.name).Aggregate("",(x,y)=>x+","+y));}
            Require(test(),"Timed out: phase="+r?.tutorialPhase+" page="+game.UI.Page+" action="+r?.action+" time="+r?.time+" run="+r?.phase+" paused="+r?.paused+" portal="+r?.portal+" nav="+r?.navigationError+" hp="+r?.health);}
        // A story dialogue writes its lines out; skip to the last line, then press the named choice.
        IEnumerator Choose(string name,float wait=15)
        {
            yield return Until(()=>DialogueReady(name),wait);
            if(Find(name)==null)yield return Tap(Find("dialogue-skip")!=null?"dialogue-skip":"dialogue-next");
            yield return Until(()=>Find(name)!=null,3);yield return Tap(name);
        }
        bool DialogueReady(string choice)=>Find(choice)!=null||Find("dialogue-skip")!=null||Find("dialogue-next")!=null;
        // Presses whatever the prologue gate leaves reachable until the voice speaks again. Every press is a real
        // pointer through the gate's opening; a control outside it must hit the gate instead.
        IEnumerator FollowGate(string label,string capturePrefix=null,Func<bool> done=null)
        {
            float end=Time.realtimeSinceStartup+120;int step=0;bool blockedChecked=false;
            while(done!=null?!done():Find("dialogue-skip")==null&&Find("tutorial-continue")==null)
            {
                if(Time.realtimeSinceStartup>=end)
                {
                    // A stall here used to leave only its label; leave the screen, the gate and the open windows too.
                    yield return Capture("gate-timeout");
                    File.WriteAllText(Path.Combine(output,"gate-timeout.txt"),label+" target="+game.UI.Gate?.Target?.name+" gateActive="+game.UI.Gate?.Active+" phase="+game.Combat?.State.tutorialPhase+" hp="+game.Combat?.State.health+"\n"+string.Join("\n",game.UI.GetComponentsInChildren<ContentWindowView>().Select(v=>v.name+":"+v.Title.text)));
                    Require(false,"Timed out following the prologue gate: "+label+" target="+game.UI.Gate?.Target?.name);
                }
                var gate=game.UI.Gate;
                if(gate==null||!gate.Active||gate.Target==null){yield return null;continue;}
                yield return new WaitForEndOfFrame();if(!gate.Active||gate.Target==null)continue;CheckGateTarget(label);
                if(!blockedChecked&&FindAnyObjectByType<HuntEdictWindow>()!=null)
                {
                    var other=FindAnyObjectByType<HuntEdictWindow>().GetComponentsInChildren<Button>().FirstOrDefault(b=>b.name.StartsWith("edict-")&&b.transform!=gate.Target&&b.interactable&&b.gameObject.activeInHierarchy&&!gate.Target.IsChildOf(b.transform)&&!b.transform.IsChildOf(gate.Target));
                    if(other!=null)
                    {
                        var r=(RectTransform)other.transform;var e=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,r.TransformPoint(r.rect.center))};var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(e,hits);
                        Require(hits.Count>0&&hits[0].gameObject.name=="Input blocker","Gate let a control outside the step through: "+other.name+" hit="+(hits.Count>0?hits[0].gameObject.name:"none"));
                        blockedChecked=true;checks.Add("PASS the prologue gate swallows "+other.name+" while "+gate.Target.name+" is the step.");
                    }
                }
                if(capturePrefix!=null)yield return Capture(capturePrefix+"-"+(++step).ToString("00")+"-"+gate.Target.name.Replace("/","_"));
                // The movement lesson points at the whole list of choices; the player picks one of them.
                if(gate.Target.name=="Choices")yield return Tap("edict-choice-CENTER");else yield return Tap(gate.Target);
                yield return new WaitForSecondsRealtime(.25f);
            }
            Require(blockedChecked||capturePrefix==null,"The gate's block was never exercised: "+label);
        }
        void CheckStory(string prefix)
        {
            Canvas.ForceUpdateCanvases();var v=game.UI.GetComponentsInChildren<ContentWindowView>().Last();var safe=UiSafeArea.Current;var frame=Bounds(v.Frame);
            Require(frame.height<=safe.height*.301f&&Mathf.Abs(frame.yMin-safe.yMin-8*UiTheme.Scale(safe))<1,"Story dialogue is not docked to the bottom third: "+prefix);
            foreach(var image in v.GetComponentsInChildren<RawImage>())
            {
                var r=Bounds(image.rectTransform);Require(safe.Contains(r.min+Vector2.one)&&safe.Contains(r.max-Vector2.one),"Portrait outside safe area: "+prefix);
                Require(Mathf.Abs(image.rectTransform.rect.width/image.rectTransform.rect.height-(float)image.texture.width/image.texture.height)<.001f,"Distorted portrait: "+prefix);
            }
            foreach(var b in v.GetComponentsInChildren<Button>())
            {var r=Bounds((RectTransform)b.transform);Require(safe.Contains(r.min+Vector2.one)&&safe.Contains(r.max-Vector2.one),"Dialogue control outside safe area: "+prefix+" "+b.name);}
            foreach(var text in v.GetComponentsInChildren<Text>().Where(t=>t.name=="Button text"||t.name=="Speaker name"||t.name=="Speaker role"))
                Require(text.preferredHeight<=text.rectTransform.rect.height+2,"Dialogue text clipped: "+prefix+" "+text.text);
            Require(v.GetComponentsInChildren<Text>().Count(t=>t.name=="Dialogue line")==1,"Story dialogue must show exactly one line: "+prefix);
            if(Loc.Language=="en")foreach(var t in v.GetComponentsInChildren<Text>())Require(!t.text.Any(c=>c>='가'&&c<='힣'),"Untranslated dialogue text: "+t.name+" "+t.text);
        }
        IEnumerator Capture(string name){Canvas.ForceUpdateCanvases();yield return new WaitForEndOfFrame();var t=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(output,name+".png"),t.EncodeToPNG());Destroy(t);}
        IEnumerator Resize(int w,int h,string language)
        {
            game.ApplyLanguage(language);game.UI.RefreshCanvasLayout();Screen.SetResolution(w,h,FullScreenMode.Windowed);
            yield return Until(()=>Screen.width==w&&Screen.height==h,8);yield return new WaitForSecondsRealtime(.35f);
        }
        // A gate target the player cannot see would soft-lock the cover: it must sit inside the safe area and inside any scroll
        // viewport above it, and the controls a lesson presses inside an area target must be visible too.
        void CheckGateTarget(string tag)
        {
            var gate=game.UI.Gate;if(gate?.Active!=true||gate.Target==null)return;
            CheckLessonCaption(tag);
            Canvas.ForceUpdateCanvases();var target=gate.Target;var box=Bounds(target);var safe=UiSafeArea.Current;bool area=target.name=="Option area"||target.name=="Preset explanation";
            Require(area?safe.Overlaps(box):safe.Contains(box.min+Vector2.one)&&safe.Contains(box.max-Vector2.one),"Gate target outside the safe area: "+tag+" "+target.name);
            var owner=target.GetComponentInParent<ScrollRect>();
            if(owner!=null&&!area){var view=Bounds(owner.viewport);Require(view.Contains(box.min+Vector2.one)&&view.Contains(box.max-Vector2.one),"Gate target scrolled out of view: "+tag+" "+target.name+" box="+box+" viewport="+view+" scroll="+owner.name);}
            if(target.name!="Preset explanation")return;
            foreach(string control in new[]{"edict-starter-next","edict-preview-pause","edict-preview-restart"})
            {
                var button=Find(control);if(button==null)continue;var r=Bounds((RectTransform)button.transform);var scroll=button.GetComponentInParent<ScrollRect>();
                Require(safe.Contains(r.min+Vector2.one)&&safe.Contains(r.max-Vector2.one),"Lesson control outside the safe area: "+tag+" "+control);
                if(scroll!=null){var view=Bounds(scroll.viewport);Require(view.Contains(r.min+Vector2.one)&&view.Contains(r.max-Vector2.one),"Lesson control scrolled out of view: "+tag+" "+control+" box="+r+" viewport="+view);}
            }
        }
        IEnumerator LayoutProfiles(string prefix,Action open=null)
        {
            foreach(var size in new[]{new[]{440,956},new[]{956,440},new[]{1600,900},new[]{1600,1000},new[]{2100,900}})
            foreach(string language in new[]{"ko","en"})
            { const int scale=100;
                yield return Resize(size[0],size[1],language);open?.Invoke();yield return null;yield return null;
                var v=game.UI.GetComponentsInChildren<ContentWindowView>().LastOrDefault();
                var edict=game.UI.GetComponentInChildren<HuntEdictWindow>();
                var frameRoot=v!=null?v.Frame:edict?.GetComponentsInChildren<RectTransform>().Single(r=>r.name=="Hunt Edict window");
                Require(frameRoot!=null,"Missing window owner: "+prefix);var frame=Bounds(frameRoot);
                yield return Capture(prefix+"-"+size[0]+"x"+size[1]+"-"+language+"-"+scale);
                Require(UiSafeArea.Current.Contains(frame.min+Vector2.one)&&UiSafeArea.Current.Contains(frame.max-Vector2.one),"Frame outside safe area: "+prefix);
                if(language=="en"&&game.UI.Gate?.Active==true)
                    foreach(var t in game.UI.Gate.GetComponentsInChildren<Text>().Where(t=>t.enabled))
                        Require(!t.text.Any(c=>c>='가'&&c<='힣'),"Untranslated active tutorial gate: "+t.text);
                CheckGateTarget(prefix+" "+size[0]+"x"+size[1]+"-"+language);
                foreach(var text in frameRoot.GetComponentsInChildren<Button>().SelectMany(b=>b.GetComponentsInChildren<Text>()).Where(t=>t.enabled&&t.gameObject.activeInHierarchy))
                    Require(text.preferredHeight<=text.rectTransform.rect.height+2,"Button text clipped: "+prefix+" "+text.text);
                var actions=v!=null?v.Actions:edict.GetComponentsInChildren<RectTransform>().FirstOrDefault(r=>r.name=="Fixed save controls");
                foreach(var b in actions!=null?actions.GetComponentsInChildren<Button>():Array.Empty<Button>())
                {var r=Bounds((RectTransform)b.transform);Require(frame.Contains(r.min+Vector2.one)&&frame.Contains(r.max-Vector2.one),"Action outside safe frame: "+prefix);}
            }
            checks.Add("PASS "+prefix+" layout in 10 resolution/language profiles at the default text size.");yield return Resize(440,956,"ko");
        }
        IEnumerator Start()
        {
            var args=Environment.GetCommandLineArgs();for(int i=0;i<args.Length-1;i++)if(args[i]=="-hellscriptEvidencePath")output=args[i+1];Require(output!=null,"Evidence path required");Directory.CreateDirectory(output);
            yield return null;game=FindAnyObjectByType<GameController>();Require(game?.Store!=null,"Runtime store missing");var a=game.Store.Data;
            bool resumed=args.Contains("-hellscriptTutorialResume");
            int classArg=Array.IndexOf(args,"-hellscriptEdictClass");if(classArg>=0)a.selectedHero=int.Parse(args[classArg+1]);
            if(a.guide.puzzle!=null||a.guide.tutorialRun==null){yield return PuzzleAcceptance(args,a,resumed);yield break;}
            yield return PitAcceptance(a,resumed);
            yield return Until(()=>Find("dialogue-skip")!=null,10);yield return new WaitForSecondsRealtime(1.2f);yield return Capture("pit-12-reward");
            Require(a.gold==0,"Reward paid before it was taken");yield return Choose("tutorial-continue");
            yield return Until(()=>game.UI.Page=="plaza",6);yield return new WaitForSecondsRealtime(.9f);yield return Capture("prologue-16-arrival-card");
            yield return Until(()=>Find("dialogue-skip")!=null,8);yield return Tap("dialogue-skip");yield return Capture("prologue-17-arrival-dialogue");
            yield return LayoutProfiles("arrival",()=>CheckStory("arrival"));yield return Tap("tutorial-arrival-close");
            // The sections below exercise the regular guides on an account with every Hunt Edict option open; fixture only.
            a.guide.edictLegacyAccess=true;HuntEdictProgression.Reconcile(a);
            // The greeting came first, so the daily attendance popup is next in line and would cancel the town walk below. Same suppression the other smokes use.
            foreach(var kind in new[]{AttendanceKind.Weekly,AttendanceKind.Monthly})Require(game.AttendancePopups.SetHidden(kind,true,game.Store.AttendanceClock()),"Popup settings write failed");
            game.UI.CloseAttendance();
            Require(game.UI.Page=="plaza"&&a.guide.mapComplete,"Tutorial did not arrive in town");// The chosen Extra survival supplies restock on arrival, so the reward is checked net of that visit's purchase.
            Require(a.transactions.Count(t=>t.operation.StartsWith("tutorial-map-complete-v2"))==1&&a.gold==Tutorials.PrologueGold-a.Hero.potions.spentThisVisit&&a.Hero.xp==0&&a.Hero.highestClear==0&&a.records.Count==0,"Prologue reward or rift economy wrong: gold="+a.gold+" restock="+a.Hero.potions.spentThisVisit);
            checks.Add("PASS real boss death, 1,000 gold reward once (arrival restock spent "+a.Hero.potions.spentThisVisit+"), town arrival event, no rift rewards or clear records.");
            Require(!Tutorials.Available(a,Tutorials.Definition("F06")),"F06 opened while the level-1 point holds the first skill");
            yield return LayoutProfiles("journal",()=>game.UI.ShowTutorialJournal());game.UI.CloseTutorialJournal();
            game.UI.ShowTutorialJournal(ContentUnlocks.Train);yield return Tap("tutorial-read");Require(Tutorials.Record(a,ContentUnlocks.Train).read&&!Tutorials.Record(a,ContentUnlocks.Train).practiced,"Reading fabricated practice");
            yield return Tap("tutorial-defer");Require(Tutorials.Record(a,ContentUnlocks.Train).deferred,"Defer did not persist");
            game.UI.ShowTutorialJournal(ContentUnlocks.Train);yield return Tap("tutorial-visibility");yield return Tap("tutorial-visibility");Require(Tutorials.Record(a,ContentUnlocks.Train).hidden,"Hide did not persist");
            yield return Tap("tutorial-visibility");Require(!Tutorials.Record(a,ContentUnlocks.Train).hidden&&!Tutorials.Record(a,ContentUnlocks.Train).deferred,"Restore did not clear hidden/deferred");game.UI.CloseTutorialJournal();
            checks.Add("PASS actual read/defer/hide/restore buttons remain separate from practice.");
            game.RequestStation(TownStation.RiftKeeper);yield return Until(()=>game.Town.Nearby==TownStation.RiftKeeper,30);game.InteractTown();yield return null;Require(Find("rift-enter-normal")!=null,"Town interaction failed");yield return Capture("first-rift-preparation");yield return Tap("rift-enter-normal");
            yield return Until(()=>game.Active&&!game.TutorialActive,20);Require(Tutorials.Record(a,"F01").practiced&&Tutorials.Record(a,"F02").practiced,"Real town and entry facts missing");
            if(args.Contains("-hellscriptTutorialLiveOps")){Require(game.Combat.State.liveOps?.source=="published","First rift did not use the published loopback release");checks.Add("PASS actual asynchronous admission pins the published loopback configuration; no cloud or telemetry claim.");}
            game.Combat.State.paused=true;game.UI.ShowTutorialJournal("F03");Require(game.UI.GetComponentsInChildren<ContentWindowView>().Length==0,"Regular combat guide opened a blocking window");game.Combat.State.paused=false;
            yield return new WaitForSecondsRealtime(6);Require(!game.Combat.State.paused&&game.UI.GetComponent<ContentWindowHost>().Count==0,"Automatic popup interrupted the first rift");
            yield return Until(()=>!game.Active&&game.UI.Page=="result",1500);game.ReturnTown();checks.Add("PASS actual first rift entry, nonblocking combat guidance, real result and return.");
            // Every result queues one automatic edict notice card. The F05-F07 steps below drive the same guides by hand, and the card covers the town and cancels their walks, so keep it out of the way from here on.
            a.guide.hintsHidden=true;
            game.UI.ShowTutorialJournal("F05");yield return Tap("tutorial-practice");yield return Until(()=>Find("edict-option-survival.potionHpPercent")!=null,5);yield return Tap("edict-option-survival.potionHpPercent");
            var input=game.UI.GetComponentsInChildren<InputField>().Single(f=>f.name=="edict-number-input");input.text=input.text=="55"?"60":"55";yield return Tap("edict-number-apply");yield return Tap("edict-save");
            Require(Tutorials.Record(a,"F05").practiced,"Real edict save did not complete F05");yield return Capture("edict-saved");yield return Tap("edict-close");
            // Level fixture only when the first rift did not reach the second point; never a natural-growth claim.
            bool skillFixture=a.Hero.level<2;if(skillFixture){a.Hero.level=2;game.Save();}
            Require(Tutorials.Available(a,Tutorials.Definition("F06")),"F06 did not open with the second point at level "+a.Hero.level);
            string skill=TutorialProgress.NewSkills(a.Hero).First();
            yield return LayoutProfiles("f06",()=>game.UI.ShowTutorialJournal("F06"));game.UI.CloseTutorialJournal();
            game.UI.ShowTutorialJournal("F06");yield return Tap("tutorial-practice");
            yield return Tap("edict-skill-"+skill);
            // F06 is free practice: close the tree's action bubble by real outside pointer input before learning.
            var skillWindow=FindAnyObjectByType<HuntEdictWindow>();
            if(skillWindow.HasDialog)
            {
                var rank=Find("edict-skill-rank-up");Require(rank!=null,"Missing F06 rank inspector");
                var rect=(RectTransform)rank.transform;var pointer=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,rect.TransformPoint(rect.rect.center)),button=PointerEventData.InputButton.Left};
                var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(pointer,hits);
                var dismiss=Find("edict-skill-menu-dismiss");Require(dismiss!=null&&hits.Count>0&&ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject)==dismiss.gameObject,"F06 bubble did not safely consume the outside click");
                ExecuteEvents.Execute(dismiss.gameObject,pointer,ExecuteEvents.pointerDownHandler);ExecuteEvents.Execute(dismiss.gameObject,pointer,ExecuteEvents.pointerUpHandler);ExecuteEvents.Execute(dismiss.gameObject,pointer,ExecuteEvents.pointerClickHandler);
                yield return null;yield return null;Require(!skillWindow.HasDialog,"F06 outside click did not dismiss the bubble");
            }
            yield return Tap("edict-skill-rank-up");yield return Tap("edict-skill-equip");yield return Tap("edict-save");
            Require(Tutorials.Record(a,"F06").practiced,"Real skill equip/save did not complete F06");yield return Capture("new-skill-equipped");yield return Tap("edict-close");
            checks.Add("PASS F06 opened at level "+a.Hero.level+" and completed by pointer learn/equip/save of "+skill+"; skill level fixture used="+skillFixture+".");
            game.UI.ShowTutorialJournal("F07");yield return Tap("tutorial-practice");yield return Until(()=>game.Town.Nearby==TownStation.Training,30);game.InteractTown();yield return null;
            // The training ground lobby replaced the fixed trainings: start its saved setup and run the fight to its end.
            yield return Tap("training-start");for(int i=0;i<8000&&game.Active;i++)game.Combat.Tick(CombatSimulation.Step);
            yield return Until(()=>!game.Active&&game.UI.Page=="result",90);game.ReturnTown();
            Require(a.Hero.guide.trainedSetup==TutorialProgress.Signature(a.Hero),"Training did not record the changed setup");
            game.UI.ShowTutorialJournal("F07");yield return Tap("tutorial-practice");yield return Until(()=>game.Town.Nearby==TownStation.RiftKeeper,30);game.InteractTown();yield return null;yield return Tap("rift-enter-normal");
            yield return Until(()=>game.Active&&!game.TutorialActive,20);
            Require(Tutorials.Record(a,"F07").practiced,"Matching new rift did not complete F07");game.ReturnTown();
            checks.Add("PASS F05 real edict save, F06 pointer skill equip/save, owned training and matching F07 fresh rift.");
            // Late-unlock UI fixtures only; this section makes no natural-progression claim.
            a.Hero.highestClear=120;ContentUnlocks.Reconcile(a);game.Save();game.EnterPlaza(true);game.UI.CloseTutorialJournal();
            game.UI.ShowTutorialBriefing(ContentUnlocks.Enhance);yield return Until(()=>Find("dialogue-skip")!=null,4);yield return Capture("staging-briefing-typing");
            yield return Tap("dialogue-skip");Require(Find("tutorial-practice")!=null&&Find("tutorial-read")!=null&&Find("tutorial-defer")!=null,"Resident briefing lost its choices");
            yield return LayoutProfiles("briefing",()=>CheckStory("briefing"));game.UI.CloseTutorialJournal();checks.Add("PASS the blacksmith briefs enhancement in the shared story dialogue with practice, read and later.");
            game.UI.ShowTutorialJournal(ContentUnlocks.Enhance);yield return Tap("tutorial-practice");yield return Until(()=>Find("tutorial-support-execute")!=null,35);
            yield return LayoutProfiles("support");yield return Capture("support-review");int gold=a.gold;yield return Tap("tutorial-support-execute");Require(a.guide.supportUsed.Contains(ContentUnlocks.Enhance)&&a.gold==gold,"Support transaction failed");checks.Add("PASS supported enhancement uses pointer-confirmed atomic domain transaction; late unlock uses explicit fixture.");
            game.UI.CloseTutorialJournal();yield return PitReplay(a);
            game.Save();File.WriteAllText(Path.Combine(output,"runtime-tutorial-smoke.txt"),string.Join("\n",checks)+"\nmacOS synthetic EventSystem pointers. No physical mobile or human comprehension claim.\nRenderer="+SystemInfo.graphicsDeviceType);
            Debug.Log("HELLSCRIPT_TUTORIAL_RUNTIME_SMOKE_OK");Application.Quit(0);
        }
    }
}

#endif
