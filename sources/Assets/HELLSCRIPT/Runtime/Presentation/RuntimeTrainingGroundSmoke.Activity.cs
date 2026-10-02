#if UNITY_EDITOR || DEVELOPMENT_BUILD
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
    public sealed partial class RuntimeTrainingGroundSmoke
    {
        RectTransform Activity=>game.UI.ActivityHud;
        RectTransform ActivityBody=>Activity.Find("Activity graphs") as RectTransform;
        void CheckActivity(string tag,bool defaultPosition)
        {
            Canvas.ForceUpdateCanvases();var bounds=Bounds(Activity);var safe=UiSafeArea.Current;
            Require(safe.Contains(bounds.min+Vector2.one)&&safe.Contains(bounds.max-Vector2.one),tag+" unsafe activity panel");
            if(defaultPosition)
                foreach(string path in new[]{"Training DPS panel","Rift minimap","Live combat journal","Header/Boss status"})
                {
                    var other=Activity.parent.Find(path) as RectTransform;
                    Require(other==null||!other.gameObject.activeInHierarchy||!bounds.Overlaps(Bounds(other)),tag+" activity overlaps "+path);
                }
            foreach(var text in Activity.GetComponentsInChildren<Text>().Where(t=>t.isActiveAndEnabled&&t.text.Length>0))
            {
                Require(text.preferredHeight<=text.rectTransform.rect.height+2,tag+" clipped activity text: "+text.text);
                if(Loc.Language=="en")Require(!text.text.Any(c=>c>=0xAC00&&c<=0xD7A3),tag+" untranslated: "+text.text);
            }
            if(!ActivityBody.gameObject.activeSelf)return;
            var f=game.Combat.State.statistics.feedback;var shares=f.ActivityPercentages();float end=0;
            Require(shares.Sum()==100,"Activity shares do not total 100");
            for(int i=0;i<4;i++)
            {
                var segment=(RectTransform)ActivityBody.Find("Activity share 100%/Activity share "+i);
                Require(Mathf.Abs(segment.anchorMin.x-end/100)<.00001f,"Wrong segment start");end+=shares[i];
                Require(Mathf.Abs(segment.anchorMax.x-end/100)<.00001f&&segment.gameObject.activeSelf==(shares[i]>0),"Wrong segment share");
                var chart=ActivityBody.Find("Activity chart "+i).GetComponent<TrainingDpsChart>();
                var series=(IReadOnlyList<float>)typeof(TrainingDpsChart).GetField("current",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(chart);
                var times=(IReadOnlyList<float>)typeof(TrainingDpsChart).GetField("times",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(chart);
                Require(chart.Top>=f.ActivityDurations()[i]&&series.Count==f.activityHistory.Count,"Cumulative activity graph has the wrong scale or samples");
                Require(series.Count>0&&Mathf.Abs(series[series.Count-1]-f.ActivityDurations()[i])<.00001f,"Graph endpoint differs from cumulative total");
                for(int j=0;j<series.Count;j++)
                {
                    Require(series[j]==f.activityHistory[j].seconds[i]&&times[j]==f.activityHistory[j].time,"Graph does not plot cumulative samples in elapsed-time order");
                    if(j>0)Require(series[j]>=series[j-1]&&times[j]>times[j-1],"Cumulative graph decreased or time order changed");
                }
                var label=ActivityBody.Find("Activity label "+i).GetComponent<Text>();
                Require(label.text.Contains(shares[i]+"%"),"Missing exact activity share label");
            }
            Require(ActivityBody.GetComponentsInChildren<Graphic>().All(g=>!g.raycastTarget),"Read-only graphs block gameplay input");
        }
        IEnumerator DragActivity(Vector2 destination)
        {
            var handle=Activity.Find("activity-drag-handle");var drag=handle.GetComponent<DpsHudDrag>();
            var parent=Bounds((RectTransform)Activity.parent);var start=Bounds((RectTransform)handle).center;
            var pointer=new PointerEventData(EventSystem.current){pointerId=11,button=PointerEventData.InputButton.Left,pressPosition=start,position=start};
            var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(pointer,hits);
            Require(hits.Count>0&&hits[0].gameObject.transform==handle,"Activity drag handle is blocked");
            var target=new Vector2(parent.xMin+parent.width*destination.x,parent.yMax-parent.height*destination.y);
            pointer.position=start+target-new Vector2(Bounds(Activity).xMin,Bounds(Activity).yMax);
            drag.OnBeginDrag(pointer);drag.OnDrag(pointer);drag.OnEndDrag(pointer);yield return null;yield return null;
            Require(game.ActivityPosition.Position==new DpsHudPosition(Path.GetDirectoryName(game.ActivityPosition.Path),"hellscript-activity-position-v1.json").Position,"Activity position did not persist");
        }
        IEnumerator RiftActivityFlow()
        {
            game.SelectedStage=1;game.Begin(seed:47021);float admission=Time.realtimeSinceStartup+40;
            while(game.RiftEntryPending&&Time.realtimeSinceStartup<admission)yield return null;
            yield return null;yield return null;Require(game.Active&&Activity!=null,"Activity rift did not start: "+game.Notice);
            game.enabled=false;var run=game.Combat.State;
            for(int i=0;i<600&&run.time<20&&game.Active;i++)game.Combat.Tick(CombatSimulation.Step);
            var f=run.statistics.feedback;Require(game.Active&&f.attackSeconds>0&&f.ActivitySeconds>0,"No real combat activity was recorded");
            Require(System.Math.Abs(f.ObservedSeconds-(run.time-f.observedFrom))<.01,"Activity intervals overlap or disappear");
            game.TogglePause();float paused=run.time;double total=f.ObservedSeconds;game.Combat.Tick(1);yield return new WaitForSecondsRealtime(.2f);
            Require(run.time==paused&&f.ObservedSeconds==total,"Pause adds activity time");
            game.TogglePause();game.enabled=true;yield return new WaitForSecondsRealtime(.5f);game.enabled=false;game.TogglePause();
            Require(run.time>paused&&f.ObservedSeconds>total,"Real-time play does not update activity");
            game.UI.RefreshHud();yield return null;CheckActivity("natural",true);yield return Capture("activity-natural-1600x900-ko");
            proof.Add("PASS natural seeded Rift: cumulative graph endpoints equal real totals, monotonic time/value samples, exclusive purpose intervals, live updates and pause/resume; no damage/result/time totals injected");
            string recorded=JsonUtility.ToJson(f);
            foreach(var size in new[]{new[]{440,956},new[]{956,440},new[]{1600,900},new[]{1600,1000},new[]{1680,720}})
            foreach(string language in new[]{"ko","en"})
            {
                UiSafeArea.StopSimulating();if(size[0]==440)UiSafeArea.Simulate(0,34,0,54);if(size[0]==956)UiSafeArea.Simulate(54,21,54,0);
                yield return Resize(size[0],size[1],language);string tag=$"{size[0]}x{size[1]}-{language}";
                foreach(var mode in new[]{MapDisplayMode.Corner,MapDisplayMode.Overlay,MapDisplayMode.Hidden})
                {
                    game.MapDisplay.Apply(mode);game.UI.ShowBattle();yield return null;yield return null;game.UI.RefreshHud();
                    CheckActivity(tag,false);var dpsPosition=LiveDps.anchoredPosition;var dpsSaved=game.DpsPosition.Position;
                    yield return DragActivity(new Vector2(.08f,.35f));var moved=Activity.anchoredPosition;
                    yield return Tap("rift-activity-fold");Require(!ActivityBody.gameObject.activeSelf&&Activity.anchoredPosition==moved,"Fold moved activity panel");
                    Require(LiveDps.anchoredPosition==dpsPosition&&game.DpsPosition.Position==dpsSaved&&LiveChart.gameObject.activeSelf,"Activity controls changed DPS");
                    game.UI.ShowBattle();yield return null;yield return null;
                    Require(!ActivityBody.gameObject.activeSelf&&Vector2.Distance(Activity.anchoredPosition,moved)<1,"HUD rebuild lost activity fold/position");
                    yield return Tap("rift-activity-fold");Require(ActivityBody.gameObject.activeSelf,"Unfold failed");game.UI.RefreshHud();CheckActivity(tag,false);
                    if(mode==MapDisplayMode.Corner)yield return Capture("activity-"+tag);
                }
                yield return DragActivity(new Vector2(2,2));CheckActivity(tag+" clamped",false);
                yield return DragActivity(new Vector2(-1,-1));CheckActivity(tag+" origin",false);
                yield return DragActivity(new Vector2(.08f,.35f));
                proof.Add("PASS "+tag+": safe area, localized 4 graphs, exact 100% bar, 3 map modes, independent pointer drag/fold, rebuild retention and bounds clamp");
            }
            Require(JsonUtility.ToJson(f)==recorded,"Reading/dragging/folding changed combat evidence");
            UiSafeArea.StopSimulating();game.ReturnTown();yield return null;yield return null;Require(Activity==null,"Activity panel leaked into town");
            yield return Resize(1600,900,"ko");game.BeginTrainingGround(TrainingGround.Default(game.Store.Data.Hero));yield return null;yield return null;
            Require(game.TrainingGroundRun&&Activity==null&&LiveDps!=null,"Training DPS adapter regressed");
            proof.Add("PASS cumulative evidence unchanged by UI; activity absent in town/training, existing training DPS retained. Synthetic macOS uGUI input; no physical mobile test.");
            File.WriteAllLines(Path.Combine(output,"runtime.txt"),proof);Debug.Log("HELLSCRIPT_RIFT_ACTIVITY_SMOKE_OK");Application.Quit(0);
        }
    }
}
#endif
