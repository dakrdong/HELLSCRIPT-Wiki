using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

namespace Hellscript
{
    // Native player acceptance uses an isolated save and uGUI raycasts; it is not device-touch proof.
    public sealed class RuntimeCoreCraftingSmoke:MonoBehaviour
    {
        GameController game;string output,savePath;readonly List<string> checks=new List<string>();
        BlacksmithWindow View=>game.UI.BlacksmithPanel;AccountSave A=>game.Store.Data;
        string Recipe=>CoreCrafting.Recipes.First(r=>r.slot==0&&r.heroClass==0&&string.IsNullOrEmpty(r.setId)).id;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void Boot()
        {
            if(!Debug.isDebugBuild||!Environment.GetCommandLineArgs().Contains("-hellscriptCoreCraftingSmoke"))return;
            InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;Application.runInBackground=true;
            Application.logMessageReceived+=(m,s,t)=>{if(t==LogType.Exception)Application.Quit(1);};new GameObject("Core crafting acceptance").AddComponent<RuntimeCoreCraftingSmoke>();
        }
        static void Require(bool ok,string message){if(!ok)throw new InvalidOperationException(message);}
        static Rect Bounds(RectTransform r){var corners=new Vector3[4];r.GetWorldCorners(corners);return new Rect(corners[0],corners[2]-corners[0]);}
        IEnumerator Click(string name)
        {
            yield return new WaitForEndOfFrame();var b=View!=null?View.Find(name):game.UI.GetComponentsInChildren<Button>().FirstOrDefault(x=>x.name==name);Require(b!=null&&b.interactable,"Missing/disabled "+name);Canvas.ForceUpdateCanvases();
            var data=new PointerEventData(EventSystem.current){position=Bounds((RectTransform)b.transform).center,button=PointerEventData.InputButton.Left};
            var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(data,hits);
            Require(hits.Count>0&&hits[0].gameObject.GetComponentInParent<Button>()==b,"Obscured "+name+": "+string.Join(",",hits.Take(3).Select(x=>x.gameObject.name)));
            ExecuteEvents.Execute(b.gameObject,data,ExecuteEvents.pointerClickHandler);yield return new WaitForSecondsRealtime(.24f);
        }
        IEnumerator Outside()
        {
            Canvas.ForceUpdateCanvases();var frame=Bounds(View.FrameRect);var data=new PointerEventData(EventSystem.current){position=new Vector2(frame.xMin+4,frame.center.y),button=PointerEventData.InputButton.Left};
            var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(data,hits);var button=hits.FirstOrDefault().gameObject?.GetComponentInParent<Button>();
            Require(button!=null&&button.name=="core-dialog-outside","Popup backdrop not reachable");ExecuteEvents.Execute(button.gameObject,data,ExecuteEvents.pointerClickHandler);yield return null;
            Require(View.DialogRect==null,"Outside click did not close popup");
        }
        IEnumerator Escape()
        {
            var keyboard=Keyboard.current??InputSystem.AddDevice<Keyboard>();InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Escape));yield return new WaitForSecondsRealtime(.12f);
            InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return new WaitForSecondsRealtime(.24f);
        }
        IEnumerator Reach(string name)
        {
            var b=View.Find(name);Require(b!=null,"Missing "+name);var scroll=b.GetComponentInParent<ScrollRect>();
            if(scroll!=null){Canvas.ForceUpdateCanvases();float delta=Bounds((RectTransform)b.transform).center.y-Bounds(scroll.viewport).center.y;scroll.content.anchoredPosition-=Vector2.up*delta/View.GetComponent<Canvas>().scaleFactor;yield return null;}
            yield return Click(name);yield return new WaitForSecondsRealtime(.24f);
        }
        IEnumerator Resize(int width,int height)
        {
            Screen.SetResolution(width,height,FullScreenMode.Windowed);float until=Time.realtimeSinceStartup+8;
            while((Screen.width!=width||Screen.height!=height)&&Time.realtimeSinceStartup<until)yield return null;
            Require(Screen.width==width&&Screen.height==height,"Resolution mismatch");yield return new WaitForSecondsRealtime(.3f);
        }
        IEnumerator Capture(string name)
        {
            yield return new WaitForSecondsRealtime(.25f);Canvas.ForceUpdateCanvases();yield return new WaitForEndOfFrame();
            var image=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(output,name+".png"),image.EncodeToPNG());Destroy(image);
        }
        void Geometry()
        {
            Canvas.ForceUpdateCanvases();var frame=Bounds(View.FrameRect);
            Require(Math.Abs(frame.width-UiSafeArea.Current.width)<1&&Math.Abs(frame.height-UiSafeArea.Current.height)<1,"Safe area mismatch");
            if(View.Landscape){Require(View.Columns.Count==3,"Three columns required");Require(View.Columns.All(c=>Math.Abs(c.rect.width-View.Columns[0].rect.width)<.01f),"Unequal core columns");}
            foreach(var button in View.GetComponentsInChildren<Button>().Where(b=>b.name=="core-craft"||b.name=="core-dialog-confirm"||b.name=="forge-close"))
            {var r=Bounds((RectTransform)button.transform);Require(r.yMin>=frame.yMin&&r.yMax<=frame.yMax+1,"Action outside safe frame: "+button.name);}
        }
        IEnumerator Gauge(float ratio)
        {
            var gauge=View.GetComponentInChildren<CoreQualityGauge>();Require(gauge!=null,"Missing quality gauge");var scroll=gauge.GetComponentInParent<ScrollRect>();
            if(scroll!=null){Canvas.ForceUpdateCanvases();float delta=Bounds(gauge.rectTransform).center.y-Bounds(scroll.viewport).center.y;scroll.content.anchoredPosition-=Vector2.up*delta/View.GetComponent<Canvas>().scaleFactor;yield return null;}
            var r=Bounds(gauge.rectTransform);float inset=8*View.GetComponent<Canvas>().scaleFactor;
            var data=new PointerEventData(EventSystem.current){pointerId=13,button=PointerEventData.InputButton.Left,position=new Vector2(r.xMin+inset+(r.width-2*inset)*ratio,r.center.y)};
            var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(data,hits);Require(hits.Count>0&&hits[0].gameObject==gauge.gameObject,"Gauge is obscured");
            ExecuteEvents.Execute(gauge.gameObject,data,ExecuteEvents.pointerDownHandler);ExecuteEvents.Execute(gauge.gameObject,data,ExecuteEvents.beginDragHandler);ExecuteEvents.Execute(gauge.gameObject,data,ExecuteEvents.dragHandler);ExecuteEvents.Execute(gauge.gameObject,data,ExecuteEvents.endDragHandler);ExecuteEvents.Execute(gauge.gameObject,data,ExecuteEvents.pointerUpHandler);yield return null;
        }
        IEnumerator Start()
        {
            var args=Environment.GetCommandLineArgs();int shots=Array.IndexOf(args,"-hellscriptScreenshots"),save=Array.IndexOf(args,"-hellscriptSavePath");Require(shots>=0&&save>=0,"Isolated paths required");output=args[shots+1];savePath=args[save+1];Directory.CreateDirectory(output);
            yield return new WaitForSecondsRealtime(1);game=FindAnyObjectByType<GameController>();Require(game?.Store!=null,"Game not initialized");game.ApplyInterfaceScale(100);game.ApplyLanguage("ko");
            if(args.Contains("-hellscriptCoreCraftingVerify"))
            {
                Require(A.coreCraft.history.Count==2&&A.premium==4000&&A.cores.Sum()==96-20&&A.coreCraft.pendingId!="","Restart lost crafted items or costs");
                checks.Add("Fresh player process restored two results, pending result, cores and coins");
            }
            else
            {
                Require(game.Store.Transact("core-smoke-fixture","core-smoke-fixture",a=>
                {a.premium=12000;a.coreCraft=new CoreCraftState();a.cores=Enumerable.Repeat(12,8).ToArray();foreach(var hero in a.heroes){hero.highestClear=30;hero.capacity=50;}ContentUnlocks.Reconcile(a);return true;}),game.Store.Error);
            }
            game.EnterPlaza(true);game.UI.ShowBlacksmith(3);Require(View==null,"Forge opened outside NPC radius");game.RequestStation(TownStation.Blacksmith);
            float until=Time.realtimeSinceStartup+15;while(game.Town.Nearby!=TownStation.Blacksmith&&Time.realtimeSinceStartup<until)yield return null;
            Require(game.Town.Nearby==TownStation.Blacksmith,"NPC route failed");yield return Click("town-interact");Require(View!=null,"NPC did not open forge");yield return Click("forge-tab-3");yield return Resize(440,956);
            if(args.Contains("-hellscriptCoreCraftingVerify"))
            {
                Require(View.DialogRect!=null,"Pending result did not reopen");yield return Capture("restart-pending-result");yield return Click("core-dialog-confirm");Require(A.coreCraft.pendingId==""&&View.CorePage==0,"Confirmation failed after restart");
                File.WriteAllText(Path.Combine(output,"restart-result.txt"),"PASS\n"+string.Join("\n",checks));Debug.Log("HELLSCRIPT_CORE_CRAFTING_RESTART_OK");Application.Quit(0);yield break;
            }
            yield return Capture("portrait-cores");yield return Click("core-refine");Require(View.CorePage==1,"Refine did not enter catalogue");
            yield return Click("core-class-filter");yield return Click("core-class-1");Require(View.Find("core-recipe-"+Recipe)!=null,"Class filter hid matching item");
            var search=View.GetComponentsInChildren<InputField>().Single(i=>i.name=="core-search");search.text=ItemCatalog.Unique(Recipe).Name;yield return null;
            Require(View.GetComponentsInChildren<Button>().Count(b=>b.name.StartsWith("core-recipe-"))==1,"Name search did not narrow recipes");
            search.text="";yield return Click("core-class-filter");yield return Click("core-class-0");
            yield return Reach("core-inspect-"+Recipe);Require(View.DialogRect!=null,"Image detail did not open");yield return Capture("portrait-definition-popup");yield return Outside();
            yield return Reach("core-inspect-"+Recipe);yield return Escape();Require(View!=null&&View.DialogRect==null&&View.CorePage==1,"Esc did not close only details");
            yield return Reach("core-inspect-"+Recipe);yield return Click("core-dialog-confirm");Require(View.CorePage==1&&A.premium==12000,"Preview changed state");
            yield return Reach("core-recipe-"+Recipe);Require(View.CorePage==2,"Recipe did not enter forge");
            yield return Escape();Require(View.CorePage==1,"Esc did not return to catalogue");yield return Reach("core-recipe-"+Recipe);
            foreach(int step in new[]{50,100,500})yield return Reach("core-adjust-"+step);Require(View.CoreInvestment==650,"Increment buttons failed");
            foreach(int step in new[]{-50,-100,-500})yield return Reach("core-adjust-"+step);Require(View.CoreInvestment==0,"Decrement buttons failed");
            yield return Gauge(.99f);Require(View.CoreInvestment==8000,"Gauge exceeded 8,000 cap");yield return Capture("portrait-80-percent");
            yield return Click("core-craft");Require(A.premium==4000&&A.cores[0]==2&&A.coreCraft.history.Count==1,"Craft did not charge exactly once");
            Require(A.coreCraft.history[0].item.rolls.All(r=>r.rollBasisPoints>=8000),"80% guarantee violated");yield return Capture("portrait-crafted-result");yield return Click("core-dialog-confirm");Require(View.CorePage==0&&A.coreCraft.pendingId=="","Confirm did not return to cores");
            checks.Add("NPC access, class/name filters, read-only details, outside/Esc/close, +/- buttons, 80% cap, native craft and confirmation");
            yield return Click("core-slot-1");yield return Click("core-refine");yield return Click("core-grade-2");var set=CoreCrafting.Recipes.First(r=>r.slot==1&&!string.IsNullOrEmpty(r.setId));
            yield return Reach("core-recipe-"+set.id);yield return Gauge(.99f);Require(View.CoreInvestment==4000,"Gauge exceeded owned balance");
            foreach(var size in new[]{(440,956,"portrait"),(956,440,"landscape"),(1600,900,"pc-16x9"),(1440,900,"pc-16x10"),(1680,720,"pc-21x9")})
            {
                yield return Resize(size.Item1,size.Item2);
                foreach(string language in new[]{"ko","en"})
                {
                    game.ApplyLanguage(language);yield return new WaitForSecondsRealtime(.2f);Geometry();Require(View.CoreInvestment==4000&&View.CoreRecipeId==set.id,"Layout change lost draft");
                    yield return Capture(size.Item3+"-"+language+"-forge");yield return Reach("core-change-recipe");Require(View.CorePage==1&&View.DialogRect==null,"Catalogue must remain inline");yield return Capture(size.Item3+"-"+language+"-catalogue");
                    yield return Reach("core-inspect-"+set.id);Geometry();yield return Capture(size.Item3+"-"+language+"-detail");yield return Click("core-dialog-close");yield return Reach("core-recipe-"+set.id);
                }
            }
            checks.Add("Five aspect ratios, KO/EN, equal wide columns, inline catalogue, fixed scrolling popup and persistent draft");
            game.ApplyInterfaceScale(150);yield return new WaitForSecondsRealtime(.3f);Geometry();yield return Capture("large-text-pc");
            yield return Resize(956,440);Geometry();yield return Capture("large-text-landscape");yield return Resize(440,956);Geometry();yield return Capture("large-text-portrait");
            game.ApplyInterfaceScale(100);game.ApplyLanguage("ko");yield return new WaitForSecondsRealtime(.3f);
            View.SetCoreInvestment(0);yield return Click("core-craft");Require(A.premium==4000&&A.cores[1]==2&&A.coreCraft.history.Count==2,"Zero-coin set crafting mismatch");yield return Capture("set-result");
            var reloaded=new GameStore(savePath,game.catalog);Require(reloaded.Data.coreCraft.history.Count==2&&reloaded.Data.coreCraft.pendingId!=""&&reloaded.Data.Hero.inventory.Any(i=>i.id==A.coreCraft.history.Last().item.id),"Save reload lost result");
            checks.Add("Zero-coin set craft, wallet limit, large text and saved inventory/result state");
            File.WriteAllText(Path.Combine(output,"result.txt"),"PASS\n"+string.Join("\n",checks)+"\nNative macOS player / synthetic Unity input; no physical mobile validation.\n");Debug.Log("HELLSCRIPT_CORE_CRAFTING_SMOKE_OK");Application.Quit(0);
        }
    }
}
