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
            if(!System.Environment.GetCommandLineArgs().Contains("-hellscriptResultActivityFocused"))
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
            yield return ResultActivityFlow();
            UiSafeArea.StopSimulating();game.ReturnTown();yield return null;yield return null;Require(Activity==null,"Activity panel leaked into town");
            yield return Resize(1600,900,"ko");game.BeginTrainingGround(TrainingGround.Default(game.Store.Data.Hero));yield return null;yield return null;
            Require(game.TrainingGroundRun&&Activity==null&&LiveDps!=null,"Training DPS adapter regressed");
            proof.Add("PASS cumulative evidence unchanged by UI; activity absent in town/training, existing training DPS retained. Synthetic macOS uGUI input; no physical mobile test.");
            File.WriteAllLines(Path.Combine(output,"runtime.txt"),proof);Debug.Log("HELLSCRIPT_RIFT_ACTIVITY_SMOKE_OK");Application.Quit(0);
        }
        IEnumerator ResultActivityFlow()
        {
            var run=game.Combat.State;run.paused=false;
            for(int i=0;i<14000&&game.Active;i++){game.Combat.Tick(CombatSimulation.Step);if(i%400==0)yield return null;}
            Require(!game.Active,"Natural Rift did not finish");
            typeof(GameController).GetMethod("CompleteHuntResult",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(game,null);
            Require(game.PrepareResultRetry(),game.Store.Error);game.UI.ShowResult();yield return null;yield return null;
            var result=game.UI.RiftVictory;Require(result!=null,"Natural Rift result did not open");
            var review=game.Store.Data.records.Single(r=>r.id==run.id).review;var feedback=review.totals.feedback;
            Require(review.activityHistory.Count>0&&review.activityHistory.Last().time==run.time,"Final activity sample missing");
            Require(game.Store.Save(),game.Store.Error);
            var restored=new GameStore(save,game.catalog).Data.records.Single(r=>r.id==run.id).review;
            Require(JsonUtility.ToJson(review)==JsonUtility.ToJson(restored),"Completed activity did not survive file reload");
            File.WriteAllText(Path.Combine(output,"final-feedback.json"),JsonUtility.ToJson(feedback,true));
            File.WriteAllText(Path.Combine(output,"final-activity-samples.json"),"["+string.Join(",",review.activityHistory.Select(s=>JsonUtility.ToJson(s)))+"]");
            foreach(var size in new[]{new[]{440,956},new[]{956,440},new[]{1600,900},new[]{1600,1000},new[]{2100,900}})
            foreach(string language in new[]{"ko","en"})
            {
                UiSafeArea.StopSimulating();if(size[0]==440)UiSafeArea.Simulate(0,34,0,54);if(size[0]==956)UiSafeArea.Simulate(54,21,54,0);
                yield return Resize(size[0],size[1],language);string tag=$"{size[0]}x{size[1]}-{language}";
                string before=JsonUtility.ToJson(game.Store.Data);yield return Tap("victory-compare");
                var graph=game.UI.GetComponentsInChildren<ContentWindowView>().Single(v=>v.Source==EquipmentViewSource.BattleSnapshot);
                Require(graph.GetComponentInChildren<TrainingChartView>()!=null,"Existing DPS inspection disappeared");
                var panel=graph.GetComponentInChildren<RiftActivityPanel>();Require(panel!=null,"Final activity panel missing");
                yield return Reveal((RectTransform)panel.transform);yield return null;
                foreach(var text in graph.GetComponentsInChildren<Text>().Where(t=>t.text.Length>0&&t.name.StartsWith("Activity")))
                {
                    Require(text.preferredHeight<=text.rectTransform.rect.height+2,"Clipped result activity: "+text.text);
                    if(language=="en")Require(!text.text.Any(c=>c>=0xAC00&&c<=0xD7A3),"Untranslated result activity: "+text.text);
                }
                var shares=feedback.ActivityPercentages();
                for(int i=0;i<4;i++)
                {
                    var chart=panel.transform.Find("Activity chart "+i).GetComponent<TrainingDpsChart>();
                    var values=(IReadOnlyList<float>)typeof(TrainingDpsChart).GetField("current",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(chart);
                    var times=(IReadOnlyList<float>)typeof(TrainingDpsChart).GetField("times",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(chart);
                    Require(values.Count==review.activityHistory.Count&&times.Count==values.Count,"Wrong final activity sample count");
                    for(int j=0;j<values.Count;j++)Require(values[j]==review.activityHistory[j].seconds[i]&&times[j]==review.activityHistory[j].time,"Result plots the wrong battle evidence");
                    Require(panel.transform.Find("Activity label "+i).GetComponent<Text>().text.Contains(shares[i]+"%"),"Final share missing");
                }
                yield return Capture("result-activity-"+tag);yield return Tap("rift-graph-close");
                Require(result==game.UI.RiftVictory&&before==JsonUtility.ToJson(game.Store.Data),"Result graph changed owned evidence");
                int battleStage=run.stage;run.stage=14;game.RepeatSession.policy.enabled=true;game.RepeatSession.cancelled=false;result.View.Repaint();yield return null;
                yield return Tap("victory-repeat-settings");
                Require(Find("victory-repeat-settings").GetComponent<CanvasGroup>().alpha<.5f&&Find("victory-repeat-configure")==null&&!game.UI.CommonPanelOpen,"Locked repeat exposed settings or configured guide");
                var locked=result.View.GetComponentsInChildren<Text>().Single(t=>t.name=="Repeat hint");
                Require(locked.text==Loc.T("반복 설정은 균열 15레벨 부터 가능합니다")&&locked.preferredHeight<=locked.rectTransform.rect.height+2,"Locked repeat hint missing or clipped");
                yield return Capture("repeat-locked-"+tag);yield return new WaitForSecondsRealtime(2.1f);
                Require(!result.View.GetComponentsInChildren<Text>().Any(t=>t.name=="Repeat hint"),"Locked repeat hint did not expire after two seconds");
                // Only the UI gate uses stage fixtures; the actual completed battle and activity record remain stage 1.
                foreach(int stage in new[]{15,16})
                {run.stage=stage;result.View.Repaint();yield return null;Require(Find("victory-repeat-settings").GetComponent<CanvasGroup>().alpha==1,"Unlocked configured repeat stayed dim");}
                run.stage=15;game.RepeatSession.policy.enabled=false;game.RepeatSession.cancelled=true;result.View.Repaint();yield return null;
                yield return Tap("victory-repeat-settings");var configure=Find("victory-repeat-configure");Require(configure!=null,"Repeat hint shortcut missing");
                var label=configure.GetComponentInChildren<Text>();var rect=(RectTransform)configure.transform;
                Require(rect.rect.width<=label.preferredWidth+18&&label.preferredHeight<=rect.rect.height+1,"Repeat shortcut is not compact or text clips");
                var safe=UiSafeArea.Current;var bubble=Bounds((RectTransform)configure.transform.parent);
                Require(safe.Contains(bubble.min+Vector2.one)&&safe.Contains(bubble.max-Vector2.one),"Repeat hint outside safe area");
                yield return Capture("repeat-shortcut-"+tag);yield return Tap("victory-repeat-configure");
                Require(Edict!=null&&Edict.SelectedTab=="repeat"&&game.UI.BlocksRepeat&&result==game.UI.RiftVictory,"Repeat shortcut lost result or selected wrong tab");
                yield return Capture("repeat-tab-"+tag);yield return Tap("edict-close");
                Require(Edict==null&&!game.UI.CommonPanelOpen&&result==game.UI.RiftVictory&&result.Run.id==run.id&&game.UI.Page=="result","Closing repeat settings did not restore the same result");
                run.stage=battleStage;result.View.Repaint();yield return null;
                Require(Loc.MissingCount==0,"Missing translations: "+string.Join(";",Loc.Missing));
                proof.Add("PASS final activity and repeat shortcut "+tag+": saved battle curves/totals/shares, shared DPS, read-only graph, stage-14 configured lock and two-second expiry, stage-15/16 configured availability, compact stage-15 unconfigured pointer action and same-result return");
            }
            proof.Add("PASS naturally completed Rift: final activity snapshot includes final tick and survives an actual saved-file reload; no combat totals/timestamps injected");
        }
    }
}
#endif
