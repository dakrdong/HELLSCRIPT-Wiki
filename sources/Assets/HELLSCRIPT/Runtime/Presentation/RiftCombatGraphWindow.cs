using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    // Generated with new_content_ui.py. Uses the training ground's complete DPS panel and chart inspection.
    public static class RiftCombatGraphWindow
    {
        public static ContentWindowView Open(Transform parent,RunState run,CombatReview review,Func<float> reading,Action closed=null)
        {
            var timeline=CombatJournal.Copy(review?.dps??run.dps);string id=run.id;float seconds=run.time;
            var feedback=CombatJournal.Copy(review?.totals?.feedback??run.statistics?.feedback);
            var activity=review?.activityHistory;
            if(activity==null||activity.Count==0)activity=run.statistics?.feedback?.activityHistory;
            if(feedback!=null)feedback.activityHistory=(activity??new List<CombatActivitySample>()).Select(CombatJournal.Copy).ToList();
            return ContentWindowView.Open(parent,"그래프 확인",EquipmentViewSource.BattleSnapshot,reading,v=>
            {
                float f=reading?.Invoke()??1,w=v.Width-24;
                UiLayout.Text(v.Navigation,"Graph battle time",Loc.F("균열 {0}단계 · {1}",run.stage,TrainingGroundResultWindow.Clock(seconds)),0,0,w,32,Mathf.RoundToInt(UiTheme.Caption*f),UiTheme.Muted);
                if(timeline==null)
                {
                    var t=UiLayout.Text(v.Body,"Graph unavailable",Loc.T("이 전투에는 DPS와 스킬 사용 시점 기록이 없습니다. 새 전투부터 기록합니다."),0,0,w,100,Mathf.RoundToInt(UiTheme.Body*f),UiTheme.Muted);
                    t.gameObject.AddComponent<LayoutElement>().preferredHeight=Mathf.Max(80,t.preferredHeight+12);
                }
                else
                {
                    var row=UiLayout.Rect("Rift DPS panel",v.Body);
                    row.gameObject.AddComponent<LayoutElement>().preferredHeight=TrainingDpsPanel.Draw(row,w,0,f,TrainingGround.CaptureDps(id,seconds,timeline.damage),null,timeline.casts);
                    var hint=UiLayout.Text(v.Body,"Graph inspection hint",Loc.T("그래프를 누르거나 마우스를 올려 DPS와 스킬 사용 시점을 확인하세요."),0,0,w,50,Mathf.RoundToInt(UiTheme.Caption*f),UiTheme.Muted);
                    hint.gameObject.AddComponent<LayoutElement>().preferredHeight=Mathf.Max(32,hint.preferredHeight+8);
                }
                var activityRow=UiLayout.Rect("Rift activity result",v.Body);float y=8;
                var face=activityRow.gameObject.AddComponent<StorageSurface>();face.Paint(UiTheme.PanelHex,UiTheme.InsetHex,UiTheme.BorderHex);face.raycastTarget=false;
                UiLayout.Text(activityRow,"Activity result title",Loc.T("전투 활동 시간"),10,y,w-20,26*f,Mathf.RoundToInt(UiTheme.Body*f),UiTheme.Gold);y+=30*f;
                if(feedback?.version==CombatFeedback.Version)
                {
                    var panel=RiftActivityPanel.Create(activityRow);float height=panel.Layout(w-20,f,64*f);
                    UiLayout.Place((RectTransform)panel.transform,10,y,w-20,height);panel.Show(feedback,final:true);y+=height;
                    var summary=UiLayout.Text(activityRow,"Activity final totals",Loc.F("활동 합계 {0:0.0}초 · 대기 {1:0.0}초 제외",feedback.ActivitySeconds,feedback.waitingSeconds),10,y,w-20,100,Mathf.RoundToInt(UiTheme.Caption*f),UiTheme.Muted);
                    float summaryHeight=Mathf.Max(28*f,summary.preferredHeight+4);summary.rectTransform.sizeDelta=new Vector2(w-20,summaryHeight);y+=summaryHeight;
                }
                else
                {
                    var unavailable=UiLayout.Text(activityRow,"Activity unavailable",Loc.T("이 전투에는 활동 시간 기록이 없습니다. 새 전투부터 기록합니다."),10,y,w-20,100,Mathf.RoundToInt(UiTheme.Body*f),UiTheme.Muted);
                    float height=Mathf.Max(48*f,unavailable.preferredHeight+8);unavailable.rectTransform.sizeDelta=new Vector2(w-20,height);y+=height;
                }
                activityRow.gameObject.AddComponent<LayoutElement>().preferredHeight=y+8;
                v.Button(v.Actions,"닫기",0,0,w,44,v.Close).name="rift-graph-close";
            },closed,maximumSize:new Vector2(760,760));
        }
    }
}
