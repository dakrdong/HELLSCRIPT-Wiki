using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    public sealed partial class GameUI
    {
        Text casualDpsTitle,casualDpsHint,casualChartTop,casualChartZero;
        readonly Text[] casualDpsValues=new Text[3],casualDpsCaptions=new Text[3],casualChartTimes=new Text[3];
        readonly RectTransform[] casualDpsCells=new RectTransform[3];
        RectTransform casualLegend,activityRatio,casualDeltaDown,casualDeltaUp;
        Vector2? casualEventAt;
        bool CasualCombatHud=>globalHud!=null&&game.Combat!=null&&!game.TrainingGroundRun&&!game.Combat.State.tutorial&&game.Combat.State.training<0;
        void PrepareCasualBattleHud()=>ConfigureCombatSkin(UiSkin.Casual);
        Text CasualLabel(Transform parent,string name,string value,int size=12,bool display=false)
        {
            var t=Label(parent,value,size,UiSkin.Casual.Navy);t.name=name;t.font=display?UiFonts.CasualDisplay:UiFonts.CasualBody(true);return t;
        }
        // Restore and pointer dragging share this temporary bound; opening a menu never writes a saved position.
        float CasualGraphRightOnPage()
        {
            if(quickRoot==null||quickKind!="edict"||casualEdictTabs==null||!casualEdictTabs.gameObject.activeInHierarchy)return root.rect.width;
            var at=casualEdictTabs.TransformPoint(new Vector3(casualEdictTabs.rect.xMin,casualEdictTabs.rect.yMin,0));
            return Mathf.Clamp(root.InverseTransformPoint(at).x-root.rect.xMin-8*CasualWindowUnit,0,root.rect.width);
        }
        // The three boards the approved canvas draws: 0 PC, 1 landscape phone, 2 portrait phone.
        static int CasualKind(CasualBattleHudLayout b)=>b.portrait?2:b.compact?1:0;
        static int CasualSize(float size,float k)=>Mathf.FloorToInt(size*k+.5f);
        // Canvas units from the window's top-left; the face is the visible circle, the button keeps its 44 unit hit area around it.
        void PlaceCasualFold(Button button,float k,float x,float y,float side,float icon)
        {
            float c=side/2;Place((RectTransform)button.transform,(x+c-22)*k,(y+c-22)*k,44*k,44*k);
            var face=((UiButton)button).Face;var rect=face.rectTransform;rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(.5f,.5f);rect.anchoredPosition=Vector2.zero;rect.sizeDelta=Vector2.one*side*k;
            face.SetRound(true);((RectTransform)button.transform.Find("Icon")).sizeDelta=Vector2.one*icon*k;
        }
        // Best-fit text shrinks to a rect shorter than its line height, so such a box grows around the canvas's line box and keeps its centre.
        void PlaceCasualFitText(Text text,float k,float x,float y,float w,float h,float size,TextAnchor anchor)=>PlaceCasualText(text,k,x,y-h*.2f,w,h*1.4f,size,anchor);
        void PlaceCasualText(Text text,float k,float x,float y,float w,float h,float size,TextAnchor anchor)
        {
            Place(text.rectTransform,x*k,y*k,w*k,h*k);text.fontSize=CasualSize(size,k);text.alignment=anchor;text.horizontalOverflow=HorizontalWrapMode.Overflow;text.verticalOverflow=VerticalWrapMode.Overflow;
        }
        void BuildCasualTrainingChrome()
        {
            if(!CasualCombatHud)return;CasualBattleHudStyle.Panel(trainingHud);
            casualDpsTitle=CasualLabel(trainingHud,"Live DPS title","실시간 DPS",15,true);casualDpsTitle.raycastTarget=true;
            trainingDrag=casualDpsTitle.gameObject.AddComponent<DpsHudDrag>();trainingDrag.Bind(trainingHud,game.DpsPosition,GraphPositionFailed,()=>BattleHudTopOnPage-8,CasualGraphRightOnPage);trainingDrag.Dropped=RelayoutBattleGraphs;
            casualDpsHint=CasualLabel(trainingHud,"DPS window hint","최근 3초 평균",10);casualDpsHint.color=UiSkin.Casual.Muted;
            trainingDps.font=UiFonts.CasualDisplay;trainingDps.alignment=TextAnchor.MiddleRight;trainingDps.resizeTextForBestFit=true;
            trainingChart.SetCasualHudAppearance();
            casualLegend=Rect("DPS legend",trainingHud);
            for(int i=0;i<2;i++)
            {
                var key=Box("Series key",casualLegend,i==0?CasualBattleHudStyle.Orange:CasualBattleHudStyle.Sky);key.GetComponent<Image>().raycastTarget=false;Place(key,i*108,6,16,2);
                var text=CasualLabel(casualLegend,"Series name",i==0?"이번 판":"직전 판",11);text.color=UiSkin.Casual.Muted;Place(text.rectTransform,i*108+20,0,88,16);
            }
            var kit=new MockKit(trainingHud,UiSkin.Casual,()=>1);
            casualDeltaDown=kit.Gly(trainingHud,"downgrade",0,0,13,UiSkin.Casual.Danger);casualDeltaUp=kit.Gly(trainingHud,"upgrade",0,0,13,UiSkin.Casual.Success);
            casualDeltaDown.gameObject.SetActive(false);casualDeltaUp.gameObject.SetActive(false);
            casualChartTop=CasualLabel(trainingHud,"DPS axis top","",10);casualChartZero=CasualLabel(trainingHud,"DPS axis zero","0",10);
            casualChartTop.color=casualChartZero.color=UiSkin.Casual.Edge;
            for(int i=0;i<3;i++)
            {
                casualChartTimes[i]=CasualLabel(trainingHud,"DPS time "+i,"",10);casualChartTimes[i].color=UiSkin.Casual.Edge;
                var cell=casualDpsCells[i]=CombatHudPanel("DPS statistic "+i,trainingHud,UiSkin.Casual);
                casualDpsCaptions[i]=CasualLabel(cell,"DPS statistic caption",i==0?"평균":i==1?"최고":"총 피해",10);casualDpsCaptions[i].color=UiSkin.Casual.Muted;
                casualDpsValues[i]=CasualLabel(cell,"DPS statistic value","",16,true);
            }
        }
        // Every number below is read from the approved battlehud canvas (PC 1600x900, landscape 956x440, portrait 440x956), from the window's top-left.
        void PlaceCasualTrainingHud()
        {
            var bounds=CasualWindows;float k=CasualWindowUnit;PlaceCasualWindow(trainingHud,bounds.dps);int v=CasualKind(bounds);
            float w=trainingHud.rect.width/k;bool narrow=v==2&&w<180;
            PlaceCasualFold(trainingFold,k,new[]{15f,11,11}[v],new[]{13f,9,9}[v],new[]{30f,26,26}[v],16);
            float tx=new[]{53f,45,45}[v];
            PlaceCasualText(casualDpsTitle,k,tx,new[]{14f,9.6f,9.6f}[v],w-tx-100,new[]{15f,13,13}[v],new[]{15f,13,13}[v],TextAnchor.MiddleLeft);
            PlaceCasualText(casualDpsHint,k,tx,new[]{28.3f,22.5f,22.5f}[v],w-tx-100,new[]{16f,13.5f,13.5f}[v],new[]{11f,9.5f,9.5f}[v],TextAnchor.MiddleLeft);
            float pad=new[]{15f,11,11}[v];
            PlaceCasualFitText(trainingDps,k,w-pad-110,new[]{13f,11,12}[v],110,new[]{30f,22,20}[v],new[]{30f,22,20}[v],TextAnchor.MiddleRight);
            if(narrow)
            {
                // Keep the fold hit target and full-size labels on separate rows in the reserved lane.
                PlaceCasualText(casualDpsTitle,k,45,8,w-56,32,13,TextAnchor.UpperLeft);casualDpsTitle.horizontalOverflow=HorizontalWrapMode.Wrap;
                PlaceCasualText(trainingDps,k,pad,48,w-2*pad,20,20,TextAnchor.MiddleRight);
                PlaceCasualText(casualDpsHint,k,pad,74,w-2*pad,32,9.5f,TextAnchor.UpperLeft);casualDpsHint.horizontalOverflow=HorizontalWrapMode.Wrap;
            }
            trainingDps.resizeTextMinSize=Mathf.Max(8,Mathf.RoundToInt(10*k));trainingDps.resizeTextMaxSize=trainingDps.fontSize;
            trainingChart.gameObject.SetActive(!trainingFolded);trainingDelta.gameObject.SetActive(!trainingFolded);trainingStats.gameObject.SetActive(false);
            casualDpsHint.gameObject.SetActive(!trainingFolded);casualLegend.gameObject.SetActive(!trainingFolded&&v==0);
            bool stats=v<2&&!trainingFolded;
            for(int i=0;i<3;i++){casualDpsCells[i].gameObject.SetActive(stats);casualChartTimes[i].gameObject.SetActive(!trainingFolded);}
            casualChartTop.gameObject.SetActive(!trainingFolded);casualChartZero.gameObject.SetActive(!trainingFolded);
            if(!trainingFolded)
            {
                float ix=pad,iy=new[]{45.2f,38.1f,38.1f}[v];
                // Header rows and wrapped delta copy reserve separate height; the plot follows both.
                float dy=v==2?Mathf.Max(0,bounds.dps.height/globalHud.Layout.presentationScale-127.13f):0,headerExtra=narrow?NarrowDpsHeaderExtra:0;
                Place(casualDeltaDown,ix*k,(iy+headerExtra)*k,13*k,13*k);Place(casualDeltaUp,ix*k,(iy+headerExtra)*k,13*k,13*k);
                PlaceCasualText(trainingDelta,k,ix+17,new[]{43f,37,37}[v]+headerExtra,w-ix-17-pad,new[]{17.5f,15,15}[v]+Mathf.Max(0,dy-headerExtra),new[]{12f,10.5f,10.5f}[v],TextAnchor.MiddleLeft);
                trainingDelta.horizontalOverflow=HorizontalWrapMode.Wrap;
                float px=new[]{55f,43,43}[v],py=new[]{92.5f,57.1f,57.1f}[v]+dy,pw=new[]{250f,182,142}[v],ph=new[]{92f,40,42}[v];
                pw=Mathf.Min(pw,w-px-pad);
                Place((RectTransform)trainingChart.transform,px*k,py*k,pw*k,ph*k);trainingChart.SetCasualMarkSizes(new[]{18f,14,14}[v]*k,new[]{22f,17,17}[v]*k);
                Place(casualLegend,15*k,68.5f*k,w*k,16*k);
                for(int i=0;i<2;i++)
                {
                    float kx=i*66.5f;
                    Place((RectTransform)casualLegend.GetChild(i*2),kx*k,6.5f*k,16*k,3*k);
                    var label=casualLegend.GetChild(i*2+1).GetComponent<Text>();PlaceCasualText(label,k,kx+21,0,70,16,11,TextAnchor.MiddleLeft);
                }
                float axis=new[]{49f,37,37}[v],axisFs=new[]{10f,9,9}[v],axisH=new[]{14.5f,13,13}[v],topY=new[]{85.2f,50.6f,50.6f}[v]+dy,zeroY=new[]{177.2f,90.6f,92.6f}[v]+dy;
                PlaceCasualText(casualChartTop,k,axis-40,topY,40,axisH,axisFs,TextAnchor.MiddleRight);PlaceCasualText(casualChartZero,k,axis-40,zeroY,40,axisH,axisFs,TextAnchor.MiddleRight);
                float timeY=new[]{188.5f,101.1f,103.1f}[v]+dy,mid=px+pw/2-(v==0?3:2.7f);
                float tick=narrow?Mathf.Min(50,pw/3):50;if(narrow)mid=px+pw/2;
                PlaceCasualText(casualChartTimes[0],k,px,timeY,tick,axisH,axisFs,TextAnchor.MiddleLeft);
                PlaceCasualText(casualChartTimes[1],k,mid-tick/2,timeY,tick,axisH,axisFs,TextAnchor.MiddleCenter);
                PlaceCasualText(casualChartTimes[2],k,px+pw-tick,timeY,tick,axisH,axisFs,TextAnchor.MiddleRight);
                if(stats)
                {
                    float cx=pad,gap=v==0?6:4,cw=(w-2*cx-2*gap)/3,cy=new[]{203f,119.1f}[v],ch=new[]{44.5f,35.5f}[v],off=v==0?10:8;
                    for(int i=0;i<3;i++)
                    {
                        Place(casualDpsCells[i],(cx+i*(cw+gap))*k,cy*k,cw*k,ch*k);
                        PlaceCasualText(casualDpsCaptions[i],k,off,v==0?5:3,cw-2*off,v==0?14.5f:13,v==0?10:9,TextAnchor.MiddleLeft);
                        PlaceCasualFitText(casualDpsValues[i],k,off,v==0?21.5f:17.5f,cw-2*off,v==0?16:13,v==0?16:13,TextAnchor.MiddleLeft);
                        casualDpsValues[i].resizeTextForBestFit=true;casualDpsValues[i].resizeTextMinSize=Mathf.Max(8,Mathf.RoundToInt(9*k));casualDpsValues[i].resizeTextMaxSize=casualDpsValues[i].fontSize;
                    }
                }
            }
            casualDpsTitle.fontSize=CasualSize(new[]{15f,13,13}[v],k);
            trainingDrag.Restore();
        }
        void RefreshCasualTrainingValues(float live,double total,float peak,float seconds)
        {
            if(!CasualCombatHud)return;trainingDps.text=live.ToString("N0");
            casualDpsValues[0].text=(seconds>0?total/seconds:0).ToString("N0");casualDpsValues[1].text=peak.ToString("N0");casualDpsValues[2].text=total.ToString("N0");
            casualChartTop.text=trainingChart.Chart.Top.ToString("N0");
            for(int i=0;i<3;i++)casualChartTimes[i].text=Loc.F("{0:0}초",trainingChart.Seconds*i/2f);
            float before=trainingPreviousDamage==null?0:TrainingGround.WindowDps(trainingPreviousDamage,Mathf.FloorToInt(seconds));
            trainingDelta.color=before<=0?UiSkin.Casual.Muted:live>=before?UiSkin.Casual.Success:UiSkin.Casual.Danger;
            bool compared=before>0&&!trainingFolded;casualDeltaDown.gameObject.SetActive(compared&&live<before);casualDeltaUp.gameObject.SetActive(compared&&live>=before);
        }
        void BuildCasualActivityChrome()
        {if(!CasualCombatHud)return;activityDrag.Bind(activityHud,game.ActivityPosition,GraphPositionFailed,()=>BattleHudTopOnPage-8,CasualGraphRightOnPage);activityDrag.Dropped=RelayoutBattleGraphs;CasualBattleHudStyle.Panel(activityHud);activityRatio=CasualBattleHudStyle.RatioBar(activityHud);activityPanel.SetCasualHudAppearance();}
        void PlaceCasualActivityHud()
        {
            var bounds=CasualWindows;float k=CasualWindowUnit;PlaceCasualWindow(activityHud,bounds.activity);int v=CasualKind(bounds);
            float pw=activityHud.rect.width/k,ph=activityHud.rect.height/k;
            PlaceCasualFold(activityFold,k,v==0?13:10,v==0?11:8,v==0?30:26,16);activityTitle.font=UiFonts.CasualDisplay;
            float tx=v==0?51:42,ratio=(v==0?96:72);
            PlaceCasualText(activityTitle,k,tx,v==0?18:13.8f,pw-tx-(activityFolded?ratio+16:8),v==0?16:14,v==0?16:14,TextAnchor.MiddleLeft);
            if(activityFolded)activityHud.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,44*k);
            activityRatio.gameObject.SetActive(activityFolded);Place(activityRatio,(pw-ratio-10)*k,16*k,ratio*k,12*k);
            activityBody.gameObject.SetActive(!activityFolded);Place(activityBody,0,0,pw*k,ph*k);
            activityPanel.LayoutCasual(v,k,pw);
            activityDrag.Restore();
        }
        Rect PlaceCasualMinimap(RectTransform mini,out float map)
        {
            var bounds=CasualWindows;float k=CasualWindowUnit,c=BattleCanvasScale;var rect=bounds.minimap;
            float pw=rect.width/c,ph=rect.height/c;var run=game.Combat.State;string fault=string.IsNullOrEmpty(run.navigationError)?"":Loc.T(run.navigationError);
            float status=fault==""?0:28*k;ph+=status;
            // The card is the corner map and a navigation fault line; the chest count is header text, so with neither there is no card.
            map=game.MapDisplay.Mode==MapDisplayMode.Corner?Mathf.Max(0,ph-5*k-status):0;
            bool card=map>0||status>0;if(mini.gameObject.activeSelf!=card)mini.gameObject.SetActive(card);
            if(!card)return default;
            Place(mini,rect.x/c,rect.y/c-status,pw,ph);
            // The content dock unfolds over the map, as in the legacy narrow layout.
            int order=header.GetSiblingIndex();if(mini.GetSiblingIndex()>order)mini.SetSiblingIndex(order);
            var viewport=(RectTransform)mini.Find("Map viewport");viewport.gameObject.SetActive(map>0);Place(viewport,5*k,5*k,pw-10*k,map-5*k);
            fieldStatusText.font=UiFonts.CasualBody(true);fieldStatusText.fontSize=Mathf.RoundToInt(10*k);fieldStatusText.color=UiSkin.Casual.Danger;
            Place(fieldStatusText.rectTransform,4*k,ph-status,pw-8*k,status);fieldStatusText.gameObject.SetActive(status>0);fieldStatusText.text=fault;
            PlaceMinimapZoom(mini,map>0,map);
            return new Rect(rect.x/c,rect.y/c-status,pw,ph);
        }
        // The field chest count is plain header text on the XP row, right after the XP line. The XP line gives way to it (its full text, then
        // without the next-skill detail, then the XP alone, then cut with an ellipsis); the count never shrinks and stops 6 units short of the shortcut captions (which start 6 left of their buttons).
        void PlaceCasualChestCount(float k,float left,float xpCenter,float xpSize,float rightLimit)
        {
            if(chestCountText==null)return;
            var t=chestCountText;t.font=UiFonts.CasualBody(true);t.fontSize=CasualSize(xpSize,k);t.color=UiSkin.Casual.White;CasualBattleHudStyle.WorldOutline(t,k);
            t.alignment=TextAnchor.MiddleLeft;t.horizontalOverflow=HorizontalWrapMode.Overflow;
            float gap=8*k,width=Mathf.Ceil(TextWidth(t,t.text))+2,x=Mathf.Max(left+gap,Mathf.Min(left+TextWidth(headerSubtitle,xpTemplate)+gap,rightLimit-width));
            xpRoom=Mathf.Max(0,x-gap-left);
            Place(t.rectTransform,x,(xpCenter-7)*k,width,14*k);Place(headerSubtitle.rectTransform,left,(xpCenter-7)*k,xpRoom,14*k);
            FitXpLine();
        }
        string xpWithNext="",xpPlain="",xpOnly="",xpTemplate="",fittedWithNext="",fittedPlain="";float xpRoom=float.MaxValue,fittedRoom=-1;Text fittedLine;Font fittedFont;int fittedSize;
        // The count is placed by the XP line's width with every digit as wide as an 8, so a kill that only changes the digits never moves it;
        // a new digit count does, and the header is measured again then.
        void SetXpLine(string withNext,string plain,string only)
        {
            if(withNext!=xpWithNext||plain!=xpPlain)
            {
                xpWithNext=withNext;xpPlain=plain;xpOnly=only;
                var digits=withNext.ToCharArray();for(int i=0;i<digits.Length;i++)if(char.IsDigit(digits[i]))digits[i]='8';
                string template=new string(digits);if(template!=xpTemplate){xpTemplate=template;battleSize=Vector2.zero;}
            }
            FitXpLine();
        }
        void FitXpLine()
        {
            if(headerSubtitle==null||xpWithNext=="")return;
            if(!CasualCombatHud){headerSubtitle.text=xpWithNext;return;}
            if(fittedLine==headerSubtitle&&fittedFont==headerSubtitle.font&&fittedSize==headerSubtitle.fontSize&&fittedWithNext==xpWithNext&&fittedPlain==xpPlain&&fittedRoom==xpRoom)return;
            fittedLine=headerSubtitle;fittedFont=headerSubtitle.font;fittedSize=headerSubtitle.fontSize;fittedWithNext=xpWithNext;fittedPlain=xpPlain;fittedRoom=xpRoom;
            string shown=xpWithNext;
            // One unit of slack: the room is the measured width of this very text, so float rounding must not cut it.
            if(TextWidth(headerSubtitle,shown)>xpRoom+1)
            {
                shown=xpPlain;
                // The "· view growth" tail goes before any word is cut in half.
                if(TextWidth(headerSubtitle,shown)>xpRoom+1)shown=xpOnly;
                if(TextWidth(headerSubtitle,shown)>xpRoom+1)
                {int n=shown.Length;while(n>1&&TextWidth(headerSubtitle,shown.Substring(0,n)+"…")>xpRoom+1)n--;shown=shown.Substring(0,n)+"…";}
            }
            headerSubtitle.text=shown;
        }
        void ReflowCasualBossText()
        {
            float k=CasualWindowUnit,w=bossHud.rect.width;bool small=!globalHud.Layout.landscape||globalHud.Layout.compact;
            bossHud.GetComponent<Image>().color=Color.clear;bossTitle.font=UiFonts.CasualDisplay;bossActionLabel.font=UiFonts.CasualBody(true);
            bossTitle.fontSize=Mathf.RoundToInt((small?13:17)*k);bossActionLabel.fontSize=Mathf.RoundToInt((small?9:11)*k);
            bossTitle.color=bossActionLabel.color=UiSkin.Casual.White;bossTitle.alignment=bossActionLabel.alignment=TextAnchor.UpperCenter;
            CasualBattleHudStyle.WorldOutline(bossTitle,k);CasualBattleHudStyle.WorldOutline(bossActionLabel,k);bossHealthFill.color=UiSkin.Casual.Coral;
            bossTitle.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,w);bossActionLabel.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,w);
            float title=Mathf.Max(18*k,bossTitle.preferredHeight),action=Mathf.Max(18*k,bossActionLabel.preferredHeight);
            Place(bossTitle.rectTransform,0,0,w,title);Place(bossActionLabel.rectTransform,0,title+2*k,w,action);
            Place((RectTransform)bossHealthFill.transform.parent,0,title+action+5*k,w,3*k);bossHud.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,title+action+8*k);
        }
        void ReflowCasualBattleHud()
        {
            var layout=globalHud.Layout;float k=CasualWindowUnit,w=root.rect.width;int v=CasualKind(CasualWindows);
            bool portrait=!layout.landscape,compact=layout.compact;
            Place(header,0,0,w,110*k);headerTitle.text=BattleHeading(game.Combat.State);
            // Title, timer and kill line follow the approved canvas (PC / landscape phone / portrait phone); the XP link and the current action are game extras.
            float left=new[]{24f,70,18}[v]*k,titleCenter=new[]{31f,18.5f,18.5f}[v],timerCenter=new[]{58.9f,42.1f,42.1f}[v];
            headerTitle.font=UiFonts.CasualDisplay;headerTitle.color=UiSkin.Casual.Gold;CasualBattleHudStyle.WorldOutline(headerTitle,k);
            headerTitle.alignment=TextAnchor.MiddleLeft;headerTitle.horizontalOverflow=HorizontalWrapMode.Overflow;headerTitle.fontSize=CasualSize(new[]{22f,18,18}[v],k);
            float titleWidth=headerTitle.preferredWidth;Place(headerTitle.rectTransform,left,(titleCenter-13)*k,Mathf.Max(titleWidth,120*k),26*k);
            // The XP link sits under the timer row (PC, landscape) or in the gap above the activity window (portrait).
            float xpCenter=new[]{timerCenter+17,timerCenter+17,timerCenter+22}[v],xpSize=v==2?9:10;
            Place(headerSubtitle.rectTransform,left,(xpCenter-7)*k,260*k,14*k);headerSubtitle.font=UiFonts.CasualBody(true);headerSubtitle.fontSize=CasualSize(xpSize,k);headerSubtitle.color=UiSkin.Casual.White;CasualBattleHudStyle.WorldOutline(headerSubtitle,k);
            headerSubtitle.alignment=TextAnchor.MiddleLeft;headerSubtitle.horizontalOverflow=HorizontalWrapMode.Overflow;
            float timerSize=new[]{20f,17,17}[v],killSize=new[]{13f,12,12}[v];
            timerText.font=UiFonts.CasualDisplay;timerText.fontSize=CasualSize(timerSize,k);timerText.color=UiSkin.Casual.White;CasualBattleHudStyle.WorldOutline(timerText,k);
            timerText.alignment=TextAnchor.MiddleLeft;timerText.horizontalOverflow=HorizontalWrapMode.Overflow;
            float timerW=Mathf.Max(timerText.preferredWidth,new[]{53.4f,45.4f,45.4f}[v]*k);
            Place(timerText.rectTransform,left,timerCenter*k-12*k,timerW,24*k);
            meterText.font=UiFonts.CasualBody(true);meterText.fontSize=CasualSize(killSize,k);meterText.color=UiSkin.Casual.Navy;CasualBattleHudStyle.Halo(meterText,k);
            meterText.alignment=TextAnchor.MiddleLeft;meterText.horizontalOverflow=HorizontalWrapMode.Overflow;
            float killX=left+timerW+6*k;Place(meterText.rectTransform,killX,timerCenter*k-10*k,230*k,20*k);
            actionText.font=UiFonts.CasualBody(true);actionText.fontSize=CasualSize(12,k);actionText.color=UiSkin.Casual.White;CasualBattleHudStyle.WorldOutline(actionText,k);
            actionText.alignment=TextAnchor.MiddleLeft;actionText.horizontalOverflow=HorizontalWrapMode.Overflow;
            Place(actionText.rectTransform,killX+meterText.preferredWidth+10*k,timerCenter*k-10*k,120*k,20*k);
            actionText.gameObject.SetActive(!compact);
            float btnY=(v==0?18:8)*k;
            var settings=header.GetComponentsInChildren<Button>(true).Single(b=>b.name=="설정·안내");
            float right=(compact?66:portrait?12:24)*k,side=44*k;
            dockIcon=side;Place((RectTransform)settings.transform,w-right-side,btnY,side,side);CompactGear(settings,side);
            float pairX=compact?242*k:w-right-side-104*k;
            // The event shortcut keeps clear of the activity window: left of the shortcut pair, or beside the gear on the landscape phone.
            casualEventAt=new Vector2(compact?w-right-side-52*k:pairX-52*k,btnY);
            PlaceCasualChestCount(k,left,xpCenter,xpSize,pairX-12*k);
            var pair=new[]{(powerSavingButton,powerSavingCaption),(escapeButton,escapeCaption)};
            for(int i=0;i<pair.Length;i++)
            {
                var (button,label)=pair[i];if(button==null)continue;button.interactable=i!=0||game.CanEnterIdle;
                Place((RectTransform)button.transform,pairX+i*52*k,btnY,side,side);
                // The existing emblems and actions retain their click targets.
                label.font=UiFonts.CasualBody(true);label.fontSize=Mathf.RoundToInt((portrait||compact?9:11)*k);label.color=UiSkin.Casual.White;CasualBattleHudStyle.WorldOutline(label,k);
                float captionHeight=Mathf.Max(18*k,Mathf.Ceil(TextHeight(label,label.text,56*k))+2);
                Place(label.rectTransform,pairX+i*52*k-6*k,btnY+44*k,56*k,captionHeight);
            }
            // The content dock hangs directly under the gear; its arrow is the first slot and the shortcuts unfold below it.
            Place((RectTransform)contentDock.transform,w-right-side,CasualBattleHudLayout.DockTop(layout)/BattleCanvasScale,side,side);contentDock.Reflow(side,6*k);ReflowEventButton();
            if(riftMinimapPanel!=null)PlaceCasualMinimap(riftMinimapPanel,out _);
            float bossWidth=(portrait?280:compact?180:430)*k,bossX=portrait?left:compact?350*k:(w-bossWidth)/2,bossY=(portrait?100:compact?8:12)*k;
            Place(bossHud,bossX,bossY,bossWidth,(portrait?50:54)*k);ReflowBossText();
            PlaceCasualTrainingHud();PlaceCasualDamageGraphs();PlaceCasualActivityHud();ResolveCasualGraphs();UpdateBattleBrief();
        }
    }
}
