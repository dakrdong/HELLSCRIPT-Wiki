using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    // The live HUD and completed battle use the same read-only activity presentation and chart.
    public sealed class RiftActivityPanel:MonoBehaviour
    {
        public UiSkin Skin {get;private set;}
        RectTransform bar;
        Text hint;
        readonly Text[] labels=new Text[4],percents=new Text[4];
        readonly RectTransform[] segments=new RectTransform[4];
        readonly TrainingDpsChart[] charts=new TrainingDpsChart[4];
        readonly List<float>[] series={new List<float>(),new List<float>(),new List<float>(),new List<float>()};
        readonly List<float> times=new List<float>();
        // The live HUD's approved look: a pill track, a colour swatch with the name beside it and the time line under it.
        RectTransform track;
        readonly RectTransform[] swatches=new RectTransform[4];
        readonly Text[] values=new Text[4];
        bool casual;
        static readonly string[] Names={"이동","공격","회피","파밍"};
        // Main.dc.html --c controls the share/area; --l is the separately authored sparkline hue.
        static readonly Color[] HudShares={new Color32(0x35,0xa7,0xff,0xff),new Color32(0xff,0x5a,0x5f,0xff),new Color32(0xff,0xd2,0x3f,0xff),new Color32(0x4c,0xbf,0x55,0xff)};
        static readonly Color[] HudLines={new Color32(0x2f,0x8f,0xd8,0xff),new Color32(0xe8,0x47,0x4c,0xff),new Color32(0xd9,0xa3,0x00,0xff),new Color32(0x36,0xa8,0x45,0xff)};
        static Color Ink(int i)=>i==0?UiTheme.Resource:i==1?UiTheme.Danger:i==2?UiTheme.PaidFatigueText:UiTheme.Success;
        public static RiftActivityPanel Create(Transform parent,UiSkin skin=null)
        {
            var root=UiLayout.Rect("Activity graphs",parent);var panel=root.gameObject.AddComponent<RiftActivityPanel>();panel.Skin=skin;
            panel.bar=UiLayout.Rect("Activity share 100%",root);
            var background=panel.bar.gameObject.AddComponent<Image>();background.color=skin?.Disabled??UiTheme.Inset;background.raycastTarget=false;
            panel.hint=UiLayout.Text(root,"Activity range","",0,0,1,18,10,skin?.Muted??UiTheme.Muted,skin?.BodyFont);
            for(int i=0;i<4;i++)
            {
                var segment=panel.segments[i]=UiLayout.Rect("Activity share "+i,panel.bar);
                var fill=segment.gameObject.AddComponent<Image>();fill.color=skin!=null?HudShares[i]:Ink(i);fill.raycastTarget=false;
                var percent=panel.percents[i]=UiLayout.Text(segment,"Activity percent "+i,"",0,0,1,18,10,skin?.Navy??UiTheme.Background,skin?.BodyFont);
                percent.alignment=TextAnchor.MiddleCenter;UiLayout.Stretch(percent.rectTransform);
                panel.labels[i]=UiLayout.Text(root,"Activity label "+i,"",0,0,1,32,12,skin?.OnLight(Ink(i))??Ink(i),skin?.BodyFont);
                panel.charts[i]=UiLayout.Rect("Activity chart "+i,root).gameObject.AddComponent<TrainingDpsChart>();
                panel.charts[i].raycastTarget=false;panel.charts[i].CurrentColor=Ink(i);panel.charts[i].SetSkin(skin);
                if(skin!=null)panel.charts[i].SetCasualActivityColors(HudLines[i],HudShares[i]);
            }
            return panel;
        }
        public void SetCasualHudAppearance()
        {
            var skin=Skin??UiSkin.Casual;
            if(!casual)BuildCasual(skin);
            for(int i=0;i<4;i++)
            {
                segments[i].GetComponent<Image>().color=HudShares[i];labels[i].color=skin.Navy;
                // The canvas draws the share text white with a one unit ink outline.
                percents[i].color=skin.White;percents[i].font=skin.LabelFont("0%");CasualBattleHudStyle.Outline(percents[i],1);charts[i].SetCasualActivityAppearance(HudLines[i],HudShares[i]);
                values[i].color=RuneCasual.Text2;hint.color=skin.Muted;
            }
        }
        void BuildCasual(UiSkin skin)
        {
            casual=true;var kit=new MockKit(transform,skin,()=>1);
            track=kit.Box(transform,"Activity share track",0,0,10,10,skin.Disabled,skin.Navy,11,2);track.SetAsFirstSibling();
            bar.GetComponent<Image>().color=Color.clear;
            for(int i=0;i<4;i++)
            {
                if(i<3)
                {
                    // The canvas closes every share but the last with a 2 unit ink rule.
                    var rule=UiLayout.Rect("Share divider",segments[i]);rule.anchorMin=new Vector2(1,0);rule.anchorMax=Vector2.one;rule.pivot=new Vector2(1,.5f);rule.sizeDelta=new Vector2(2,0);rule.anchoredPosition=Vector2.zero;
                    var line=rule.gameObject.AddComponent<Image>();line.color=skin.Navy;line.raycastTarget=false;rule.SetAsFirstSibling();
                }
                swatches[i]=kit.Box(transform,"Activity swatch "+i,0,0,12,12,HudShares[i],skin.Navy,4,1.5f);
                var value=values[i]=UiLayout.Text(transform,"Activity value "+i,"",0,0,1,16,13,RuneCasual.Text2,skin.LabelFont("0"));
                value.alignment=TextAnchor.MiddleLeft;value.horizontalOverflow=HorizontalWrapMode.Overflow;value.raycastTarget=false;
                labels[i].alignment=TextAnchor.MiddleLeft;labels[i].font=UiFonts.CasualBody(true);labels[i].horizontalOverflow=HorizontalWrapMode.Overflow;
            }
        }
        static int Size(float size,float k)=>Mathf.FloorToInt(size*k+.5f);
        // Positions in canvas units from the window's own top-left; kind is 0 PC, 1 landscape phone, 2 portrait phone.
        public void LayoutCasual(int kind,float k,float width)
        {
            if(!casual)return;
            float[] barX={15,11,11},barY={49,40,40},barH={22,18,18},hintY={71,62,62},hintH={16,13.5f,13.5f},hintFs={11,9.5f,9.5f},gap={14,10,10};
            float[] swatch={12,10,10},swatchDy={2.8f,2.5f,2.5f},nameH={17.5f,15,15},nameFs={12,10.5f,10.5f},nameDx={18,15,15},valueDx={18,15,0},valueDy={18.5f,16,16},valueH={13,11,11},valueFs={13,11,11};
            float[] sparkDy={36.1f,31.2f,31.2f},sparkH={38,20,14},percentFs={12,10,10};
            float[][] rows={new[]{93f,175.1f},new[]{79.5f,134.7f},new[]{79.5f,128.7f}};
            float bx=barX[kind],bw=width-2*bx,col=(bw-gap[kind])/2;
            UiLayout.Place(track,bx*k,barY[kind]*k,bw*k,barH[kind]*k);
            UiLayout.Place(bar,(bx+2)*k,(barY[kind]+2)*k,(bw-4)*k,(barH[kind]-4)*k);
            UiLayout.Place(hint.rectTransform,bx*k,hintY[kind]*k,bw*k,hintH[kind]*k);hint.fontSize=Size(hintFs[kind],k);hint.alignment=TextAnchor.MiddleLeft;hint.font=UiFonts.CasualBody(true);
            for(int i=0;i<4;i++)
            {
                float x=bx+(i%2)*(col+gap[kind]),y=rows[kind][i/2];percents[i].fontSize=Size(percentFs[kind],k);
                UiLayout.Place(swatches[i],x*k,(y+swatchDy[kind])*k,swatch[kind]*k,swatch[kind]*k);
                UiLayout.Place(labels[i].rectTransform,(x+nameDx[kind])*k,y*k,(col-nameDx[kind])*k,nameH[kind]*k);labels[i].fontSize=Size(nameFs[kind],k);
                UiLayout.Place(values[i].rectTransform,(x+valueDx[kind])*k,(y+valueDy[kind])*k,(col-valueDx[kind])*k,valueH[kind]*k);values[i].fontSize=Size(valueFs[kind],k);
                UiLayout.Place((RectTransform)charts[i].transform,x*k,(y+sparkDy[kind])*k,col*k,sparkH[kind]*k);
            }
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
                if(casual){labels[i].text=Loc.T(Names[i]);values[i].text=Loc.F("{0:0.0}초 · {1}%",durations[i],shares[i]);}
                else labels[i].text=Loc.F("{0}\n{1:0.0}초 · {2}%",Loc.T(Names[i]),durations[i],shares[i]);
                series[i].Clear();if(feedback?.activityHistory!=null)foreach(var sample in feedback.activityHistory)series[i].Add(sample.seconds[i]);
                charts[i].gameObject.SetActive(!final||times.Count>0);charts[i].Set(series[i],null,series[i].Count,times);
            }
            hint.text=times.Count>0?Loc.F("누적 시간 · {0:0.0}~{1:0.0}초",times[0],times[times.Count-1]):Loc.T(final?"시간별 활동 기록 없음 · 최종 합계만 표시":"활동 기록 대기 · 대기 시간 제외");
        }
    }
}
