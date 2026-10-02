using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    // A read-only HUD on the battle canvas. Folding and position are independent of the DPS panel.
    public sealed partial class GameUI
    {
        RectTransform activityHud,activityBody,activityBar;
        Text activityTitle,activityHint;
        Button activityFold;
        DpsHudDrag activityDrag;
        readonly Text[] activityLabels=new Text[4],activityPercents=new Text[4];
        readonly RectTransform[] activitySegments=new RectTransform[4];
        readonly TrainingDpsChart[] activityCharts=new TrainingDpsChart[4];
        readonly List<float>[] activitySeries={new List<float>(),new List<float>(),new List<float>(),new List<float>()};
        readonly List<float> activityTimes=new List<float>();
        readonly string[] activityNames={"이동","공격","회피","파밍"};
        bool activityFolded;
        float activityShownTime=-1;
        public RectTransform ActivityHud=>activityHud;
        static Color ActivityColor(int i)=>i==0?UiTheme.Resource:i==1?UiTheme.Danger:i==2?UiTheme.PaidFatigueText:UiTheme.Success;

        void BuildRiftActivityHud(RunState run)
        {
            if(run.training>=0||run.tutorial)return;
            activityHud=Box("Rift activity panel",root,UiTheme.Tint(UiTheme.PanelHex,.86f));activityHud.GetComponent<Image>().raycastTarget=false;
            activityTitle=Label(activityHud,"활동 시간",16,pale);activityTitle.name="activity-drag-handle";activityTitle.raycastTarget=true;
            activityDrag=activityTitle.gameObject.AddComponent<DpsHudDrag>();
            activityDrag.Bind(activityHud,game.ActivityPosition,()=>ShowToast(Loc.T("그래프 위치를 기기에 저장하지 못했습니다. 다시 끌어 놓아 저장을 재시도해 주세요.")),()=>BattleHudTopOnPage-8);
            activityFold=UiIconButton.CreateGlyph(activityHud,"rift-activity-fold","chevron",0,0,30,30,ToggleActivityFold,flip:!activityFolded);
            activityBody=Rect("Activity graphs",activityHud);
            activityBar=Box("Activity share 100%",activityBody,UiTheme.Inset);activityBar.GetComponent<Image>().raycastTarget=false;
            activityHint=Label(activityBody,"활동 기록 대기 · 대기 시간 제외",10,muted);
            for(int i=0;i<4;i++)
            {
                activitySegments[i]=Box("Activity share "+i,activityBar,ActivityColor(i));activitySegments[i].GetComponent<Image>().raycastTarget=false;
                activityPercents[i]=Label(activitySegments[i],"",10,UiTheme.Background,TextAnchor.MiddleCenter);UiLayout.Stretch(activityPercents[i].rectTransform);
                activityLabels[i]=Label(activityBody,"",12,ActivityColor(i));activityLabels[i].name="Activity label "+i;
                var plot=Rect("Activity chart "+i,activityBody);activityCharts[i]=plot.gameObject.AddComponent<TrainingDpsChart>();
                activityCharts[i].raycastTarget=false;activityCharts[i].CurrentColor=ActivityColor(i);
            }
            activityShownTime=-1;
        }
        void PlaceRiftActivityHud(float w)
        {
            if(activityHud==null)return;
            bool wide=w>=720;float pw=wide?Mathf.Min(320,w*.3f):w-24,ph=activityFolded?42:216,top=124;
            if(!wide&&trainingHud!=null)top=-trainingHud.anchoredPosition.y+trainingHud.rect.height+8;
            if(wide&&bossHud!=null&&bossHud.gameObject.activeSelf&&bossHud.anchoredPosition.x<pw+12)
                top=Mathf.Max(top,-bossHud.anchoredPosition.y+bossHud.rect.height+8);
            Place(activityHud,12,top,pw,ph);Place((RectTransform)activityFold.transform,8,6,30,30);
            Place(activityTitle.rectTransform,46,6,pw-54,30);
            activityBody.gameObject.SetActive(!activityFolded);
            Place(activityBody,10,42,pw-20,ph-48);float width=pw-20,cell=(width-10)/2;
            Place(activityBar,0,0,width,18);Place(activityHint.rectTransform,0,22,width,18);
            for(int i=0;i<4;i++)
            {
                float x=(i%2)*(cell+10),y=44+(i/2)*60;
                Place(activityLabels[i].rectTransform,x,y,cell,32);
                Place((RectTransform)activityCharts[i].transform,x,y+34,cell,24);
            }
            activityDrag.Restore();
        }
        void ToggleActivityFold()
        {
            activityFolded=!activityFolded;activityShownTime=-1;
            var mark=activityFold.transform.Find("Icon");if(mark!=null)mark.localRotation=Quaternion.Euler(0,0,activityFolded?0:180);
            battleSize=Vector2.zero;ReflowBattleHud();RefreshRiftActivityHud(game.Combat.State);
        }
        void RefreshRiftActivityHud(RunState run)
        {
            if(activityHud==null||activityFolded||activityShownTime>=0&&run.time-activityShownTime<.15f)return;
            activityShownTime=run.time;var feedback=run.statistics?.feedback;
            var durations=feedback?.ActivityDurations()??new float[4];var shares=feedback?.ActivityPercentages()??new int[4];
            activityTimes.Clear();
            if(feedback?.activityHistory!=null)foreach(var sample in feedback.activityHistory)activityTimes.Add(sample.time);
            float x=0;
            for(int i=0;i<4;i++)
            {
                var segment=activitySegments[i];segment.gameObject.SetActive(shares[i]>0);
                segment.anchorMin=new Vector2(x/100,0);x+=shares[i];segment.anchorMax=new Vector2(x/100,1);segment.offsetMin=segment.offsetMax=Vector2.zero;
                activityPercents[i].text=shares[i]>=12?shares[i]+"%":"";
                activityLabels[i].text=Loc.F("{0}\n{1:0.0}초 · {2}%",Loc.T(activityNames[i]),durations[i],shares[i]);
                var series=activitySeries[i];series.Clear();
                if(feedback?.activityHistory!=null)
                    foreach(var sample in feedback.activityHistory)series.Add(sample.seconds[i]);
                activityCharts[i].Set(series,null,series.Count,activityTimes);
            }
            activityHint.text=activityTimes.Count>0?Loc.F("누적 시간 · {0:0.0}~{1:0.0}초",activityTimes[0],activityTimes[activityTimes.Count-1]):Loc.T("활동 기록 대기 · 대기 시간 제외");
        }
    }
}
