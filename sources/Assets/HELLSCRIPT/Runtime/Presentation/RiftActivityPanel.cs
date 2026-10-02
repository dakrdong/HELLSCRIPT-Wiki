using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    // The live HUD and completed battle use the same read-only activity presentation and chart.
    public sealed class RiftActivityPanel:MonoBehaviour
    {
        RectTransform bar;
        Text hint;
        readonly Text[] labels=new Text[4],percents=new Text[4];
        readonly RectTransform[] segments=new RectTransform[4];
        readonly TrainingDpsChart[] charts=new TrainingDpsChart[4];
        readonly List<float>[] series={new List<float>(),new List<float>(),new List<float>(),new List<float>()};
        readonly List<float> times=new List<float>();
        static readonly string[] Names={"이동","공격","회피","파밍"};
        static Color Ink(int i)=>i==0?UiTheme.Resource:i==1?UiTheme.Danger:i==2?UiTheme.PaidFatigueText:UiTheme.Success;
        public static RiftActivityPanel Create(Transform parent)
        {
            var root=UiLayout.Rect("Activity graphs",parent);var panel=root.gameObject.AddComponent<RiftActivityPanel>();
            panel.bar=UiLayout.Rect("Activity share 100%",root);
            var background=panel.bar.gameObject.AddComponent<Image>();background.color=UiTheme.Inset;background.raycastTarget=false;
            panel.hint=UiLayout.Text(root,"Activity range","",0,0,1,18,10,UiTheme.Muted);
            for(int i=0;i<4;i++)
            {
                var segment=panel.segments[i]=UiLayout.Rect("Activity share "+i,panel.bar);
                var fill=segment.gameObject.AddComponent<Image>();fill.color=Ink(i);fill.raycastTarget=false;
                var percent=panel.percents[i]=UiLayout.Text(segment,"Activity percent "+i,"",0,0,1,18,10,UiTheme.Background);
                percent.alignment=TextAnchor.MiddleCenter;UiLayout.Stretch(percent.rectTransform);
                panel.labels[i]=UiLayout.Text(root,"Activity label "+i,"",0,0,1,32,12,Ink(i));
                panel.charts[i]=UiLayout.Rect("Activity chart "+i,root).gameObject.AddComponent<TrainingDpsChart>();
                panel.charts[i].raycastTarget=false;panel.charts[i].CurrentColor=Ink(i);
            }
            return panel;
        }
        public float Layout(float width,float f=1,float chartHeight=24)
        {
            float cell=(width-10)/2,row=34*f+chartHeight+2;
            UiLayout.Place(bar,0,0,width,18*f);UiLayout.Place(hint.rectTransform,0,22*f,width,18*f);hint.fontSize=Mathf.RoundToInt(10*f);
            for(int i=0;i<4;i++)
            {
                float x=(i%2)*(cell+10),y=44*f+(i/2)*row;
                labels[i].fontSize=Mathf.RoundToInt(12*f);percents[i].fontSize=Mathf.RoundToInt(10*f);
                UiLayout.Place(labels[i].rectTransform,x,y,cell,32*f);
                UiLayout.Place((RectTransform)charts[i].transform,x,y+34*f,cell,chartHeight);
            }
            return 44*f+2*row;
        }
        public void Show(CombatFeedback feedback,bool final=false)
        {
            var durations=feedback?.ActivityDurations()??new float[4];var shares=feedback?.ActivityPercentages()??new int[4];
            times.Clear();if(feedback?.activityHistory!=null)foreach(var sample in feedback.activityHistory)times.Add(sample.time);
            float x=0;
            for(int i=0;i<4;i++)
            {
                var segment=segments[i];segment.gameObject.SetActive(shares[i]>0);
                segment.anchorMin=new Vector2(x/100,0);x+=shares[i];segment.anchorMax=new Vector2(x/100,1);segment.offsetMin=segment.offsetMax=Vector2.zero;
                percents[i].text=shares[i]>=12?shares[i]+"%":"";
                labels[i].text=Loc.F("{0}\n{1:0.0}초 · {2}%",Loc.T(Names[i]),durations[i],shares[i]);
                series[i].Clear();if(feedback?.activityHistory!=null)foreach(var sample in feedback.activityHistory)series[i].Add(sample.seconds[i]);
                charts[i].gameObject.SetActive(!final||times.Count>0);charts[i].Set(series[i],null,series[i].Count,times);
            }
            hint.text=times.Count>0?Loc.F("누적 시간 · {0:0.0}~{1:0.0}초",times[0],times[times.Count-1]):Loc.T(final?"시간별 활동 기록 없음 · 최종 합계만 표시":"활동 기록 대기 · 대기 시간 제외");
        }
    }
}
