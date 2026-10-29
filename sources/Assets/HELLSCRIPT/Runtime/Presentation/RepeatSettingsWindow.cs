using System;
using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    public static class RepeatSettingsWindow
    {
        public static ContentWindowView Open(Transform parent,GameStore store,GameCatalog catalog,Func<float> readingScale,
            RepeatHuntPolicy current,string pendingRunId,Action applied,Action closed,CombatSimulation simulation=null)
        {
            var edit=new Editor(store,catalog,readingScale,current,pendingRunId,applied,simulation);
            return ContentWindowView.Open(parent,"반복 설정",EquipmentViewSource.Draft,readingScale,edit.Render,closed,
                maximumSize:new Vector2(960,860),skin:UiSkin.Casual);
        }
        sealed class Editor
        {
            readonly GameStore store;readonly RepeatSettings opened,draft;readonly string heroId,pendingRunId;readonly Action applied;
            readonly Func<float> readingScale;
            readonly EdictSettingsSection edict;
            ContentWindowView view;string notice="";float factor;
            UiSkin Skin=>UiSkin.Casual;
            int Maximum=>RepeatSettings.AvailableMinutes(store.Data.riftFatigue);
            public Editor(GameStore store,GameCatalog catalog,Func<float> readingScale,RepeatHuntPolicy current,string pendingRunId,Action applied,CombatSimulation simulation)
            {
                edict=new EdictSettingsSection(store,catalog,"repeat",simulation);
                this.store=store;this.readingScale=readingScale;this.pendingRunId=pendingRunId;this.applied=applied;heroId=store.Data.Hero.id;
                opened=store.Data.Hero.repeatSettings?.Copy();draft=RepeatSettings.Draft(store.Data.Hero,current);
                if(draft.goal==RepeatGoal.Minutes&&Maximum>0)draft.minutes=Math.Min(draft.minutes,Maximum);
            }
            Text Text(Transform parent,string name,string value,float x,float y,float width,int size=UiTheme.Body,Color? color=null)
            {
                var text=UiLayout.Text(parent,name,Loc.T(value),x,y,width,1,Mathf.RoundToInt(size*factor),color??Skin.Navy,Skin.BodyFont);
                text.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,Mathf.Ceil(text.preferredHeight)+2);return text;
            }
            RectTransform Card(Transform parent,string name,float x,float y,float width,string title,out float next)
            {
                var rect=UiLayout.Rect(name,parent);UiLayout.Place(rect,x,y,width,1);
                var surface=rect.gameObject.AddComponent<StorageSurface>();surface.raycastTarget=false;surface.SetSkin(Skin,Skin.Edge,12,1,2,Skin.White,Skin.Cream);
                var label=Text(rect,"Group title",title,12,10,width-24,UiTheme.Heading);next=10+label.rectTransform.rect.height+6;return rect;
            }
            float Radio(Transform parent,string name,string title,string description,float y,float width,bool selected,Action choose,bool available=true)
            {
                var button=view.Button(parent,"",8,y,width-16,54,choose);button.name=name;button.interactable=available;
                ((UiButton)button).Configure(UiButtonRole.Option,selected);Skin.ApplyChrome((UiButton)button);
                float glyphWidth=Mathf.Ceil(24*factor),textX=8+glyphWidth+4,textWidth=width-16-textX-10;
                var glyph=Text(button.transform,"Radio",selected?"●":"○",8,0,glyphWidth,UiTheme.Title);
                var label=Text(button.transform,"Choice title",title,textX,6,textWidth,UiTheme.Body,available?Skin.Navy:Skin.Muted);
                var detail=Text(button.transform,"Choice description",description,textX,8+label.rectTransform.rect.height,textWidth,UiTheme.Caption,Skin.Muted);
                float height=Mathf.Max(54,Mathf.Max(glyph.rectTransform.rect.height+12,detail.rectTransform.rect.height+label.rectTransform.rect.height+14));
                UiLayout.Place((RectTransform)button.transform,8,y,width-16,height);
                glyph.rectTransform.anchoredPosition=new Vector2(8,-(height-glyph.rectTransform.rect.height)/2);return height+4;
            }
            void SelectGoal(RepeatGoal goal)
            {draft.goal=goal;if(goal==RepeatGoal.Minutes&&Maximum>0)draft.minutes=Math.Clamp(draft.minutes,1,Maximum);view.Repaint();}
            float GoalSlider(Transform parent,float y,float width)
            {
                bool minutes=draft.goal==RepeatGoal.Minutes;int max=minutes?Maximum:100;
                int value=minutes?draft.minutes:draft.goal==RepeatGoal.Clears?draft.clears:draft.entries;
                string Label(int number)=>minutes?Loc.F("목표 {0}분",number):Loc.F("목표 {0}회",number);
                // Reserve the widest number before dragging; value changes never rebuild the active control.
                var label=Text(parent,"repeat-goal-value",Label(Math.Max(value,max)),12,y,width-24,UiTheme.Heading);label.text=Label(value);
                float sliderY=y+label.rectTransform.rect.height+8;
                var area=UiLayout.Rect("repeat-goal-slider",parent);UiLayout.Place(area,20,sliderY,width-40,28);area.gameObject.AddComponent<UnityEngine.UI.Image>().color=Color.clear;
                var slider=area.gameObject.AddComponent<SettingsStepSlider>();slider.ScrollOnVerticalDrag=true;slider.minValue=1;slider.maxValue=Math.Max(1,max);slider.wholeNumbers=true;slider.transition=UnityEngine.UI.Selectable.Transition.None;
                var track=UiLayout.Rect("Track",area);UiLayout.Place(track,0,10,width-40,8);var ink=track.gameObject.AddComponent<StorageSurface>();ink.raycastTarget=false;ink.SetSkin(Skin,Skin.Edge,4,1,0,Skin.Disabled,Skin.Disabled);
                var travel=UiLayout.Rect("Handle area",area);UiLayout.Stretch(travel);travel.offsetMin=new Vector2(8,0);travel.offsetMax=new Vector2(-8,0);
                var handle=UiLayout.Rect("Handle",travel);handle.sizeDelta=new Vector2(18,28);var face=handle.gameObject.AddComponent<StorageSurface>();face.SetSkin(Skin,Skin.Navy,8,1,2,Skin.Gold,Skin.GoldShade);
                slider.handleRect=handle;slider.targetGraphic=face;slider.SetValueWithoutNotify(Math.Clamp(value,1,Math.Max(1,max)));slider.interactable=max>1;
                slider.onValueChanged.AddListener(number=>
                {
                    int selected=Mathf.RoundToInt(number);if(minutes)draft.minutes=selected;else if(draft.goal==RepeatGoal.Clears)draft.clears=selected;else draft.entries=selected;
                    label.text=Label(selected);
                });
                var minimum=Text(parent,"Minimum","1",12,sliderY+32,30,UiTheme.Caption,Skin.Muted);
                var maximum=Text(parent,"Maximum",max.ToString(),width-74,sliderY+32,62,UiTheme.Caption,Skin.Muted);maximum.alignment=TextAnchor.MiddleRight;
                return sliderY+32+Mathf.Max(minimum.rectTransform.rect.height,maximum.rectTransform.rect.height)-y;
            }
            float Actions(float width)
            {
                float actionWidth=(width-16)/3;
                var cancel=view.Button(view.Actions,"취소",0,0,actionWidth,40,view.Close);cancel.name="repeat-settings-cancel";
                var stop=view.Button(view.Actions,"반복 중지",actionWidth+8,0,actionWidth,40,()=>Save(false));stop.name="repeat-settings-stop";
                var apply=view.Button(view.Actions,"저장하고 반복 시작",(actionWidth+8)*2,0,actionWidth,40,()=>Save(true),true);
                apply.name="repeat-settings-apply";apply.interactable=draft.goal!=RepeatGoal.Minutes||Maximum>=1;
                var buttons=new[]{cancel,stop,apply};float height=40;
                foreach(var button in buttons)height=Mathf.Max(height,Mathf.Ceil(button.GetComponentInChildren<Text>().preferredHeight)+12);
                for(int i=0;i<buttons.Length;i++)
                {
                    UiLayout.Place((RectTransform)buttons[i].transform,i*(actionWidth+8),0,actionWidth,height);
                    UiLayout.Place(buttons[i].GetComponentInChildren<Text>().rectTransform,5,0,actionWidth-10,height);
                }
                return height;
            }
            void FitShell(float navigationHeight,float actionHeight)
            {
                var close=(RectTransform)view.Frame.Find("content-close");var closeText=close.GetComponentInChildren<Text>();
                float closeHeight=Mathf.Max(32,Mathf.Ceil(closeText.preferredHeight)+4);
                float header=Mathf.Max(UiTheme.Header,Mathf.Max(Mathf.Ceil(view.Title.preferredHeight)+8,closeHeight+8));
                UiLayout.Place(view.Title.rectTransform,20,0,view.Width-76,header);
                UiLayout.Place(close,view.Width-46,(header-closeHeight)/2,34,closeHeight);UiLayout.Place(closeText.rectTransform,5,0,24,closeHeight);
                UiLayout.Place(view.Navigation,12,header,view.Width-24,navigationHeight);
                float height=view.Frame.rect.height,bodyY=header+navigationHeight+4;
                UiLayout.Place(view.Actions,12,height-actionHeight-8,view.Width-24,actionHeight);
                UiLayout.Place(view.Scroll.viewport,12,bodyY,view.Width-24,height-bodyY-actionHeight-16);
                var chrome=view.Frame.GetComponent<UiWindowFrame>();chrome.header=header;chrome.footer=actionHeight+16;
            }
            public void Render(ContentWindowView window)
            {
                view=window;factor=readingScale?.Invoke()??1;view.Frame.name="repeat-settings-window";
                EdictPopupLayout.AddScrollbar(view.Scroll);view.Body.sizeDelta=new Vector2(-20,0);
                float width=view.Width-44;bool wide=width>=760;const float gap=12;float column=wide?(width-gap*2)/3:width;
                var help=Text(view.Navigation,"Repeat help",Loc.F("남은 피로도 {0}분 · 현재 결과는 새 목표 횟수에 포함하지 않습니다.",Maximum),0,0,width,UiTheme.Caption,Skin.Muted);
                var groups=UiLayout.Rect("Repeat choices",view.Body);UiLayout.Place(groups,0,0,width,1);var groupLayout=groups.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();
                var goal=Card(groups,"repeat-goal-group",0,0,column,"반복 목표",out float y);
                y+=Radio(goal,"repeat-goal-clears","클리어 횟수","목표 횟수만큼 클리어하면 멈춥니다.",y,column,draft.goal==RepeatGoal.Clears,()=>SelectGoal(RepeatGoal.Clears));
                y+=Radio(goal,"repeat-goal-entries","입장 횟수","승패와 관계없이 입장 횟수를 셉니다.",y,column,draft.goal==RepeatGoal.Entries,()=>SelectGoal(RepeatGoal.Entries));
                y+=Radio(goal,"repeat-goal-minutes","시간 설정","현재 전투를 마친 뒤 다음 입장을 멈춥니다.",y,column,draft.goal==RepeatGoal.Minutes,()=>SelectGoal(RepeatGoal.Minutes),Maximum>=1);
                y+=GoalSlider(goal,y+4,column)+4;float goalHeight=y+12;goal.sizeDelta=new Vector2(column,goalHeight);
                var stop=Card(groups,"repeat-stop-group",wide?column+gap:0,wide?0:goalHeight+gap,column,"멈춤 조건",out y);
                y+=Radio(stop,"repeat-stop-defeat","패배","한 번 패배하면 자동 반복을 멈춥니다.",y,column,draft.stopCondition==RepeatStopCondition.Defeat,()=>{draft.stopCondition=RepeatStopCondition.Defeat;view.Repaint();});
                y+=Radio(stop,"repeat-stop-potions","물약 부족","착용한 물약 중 하나라도 소진되면 멈춥니다.",y,column,draft.stopCondition==RepeatStopCondition.Potions,()=>{draft.stopCondition=RepeatStopCondition.Potions;view.Repaint();});
                y+=Radio(stop,"repeat-stop-none","조건 없음","선택한 목표와 필수 입장 조건은 유지합니다.",y,column,draft.stopCondition==RepeatStopCondition.None,()=>{draft.stopCondition=RepeatStopCondition.None;view.Repaint();});
                float stopHeight=y+8;stop.sizeDelta=new Vector2(column,stopHeight);
                var progress=Card(groups,"repeat-progress-group",wide?(column+gap)*2:0,wide?0:goalHeight+stopHeight+gap*2,column,"진행 설정",out y);
                y+=Radio(progress,"repeat-progress-fixed","같은 레벨 반복","반복을 시작한 레벨을 유지합니다.",y,column,draft.progress==RepeatProgress.Fixed,()=>{draft.progress=RepeatProgress.Fixed;view.Repaint();});
                y+=Radio(progress,"repeat-progress-highest","상향 도전","현재 입장 가능한 최고 레벨에 도전합니다.",y,column,draft.progress==RepeatProgress.Highest,()=>{draft.progress=RepeatProgress.Highest;view.Repaint();});
                y+=Radio(progress,"repeat-progress-adaptive","타협 도전","성공하면 한 단계 위, 패배하면 한 단계 아래입니다.",y,column,draft.progress==RepeatProgress.Adaptive,()=>{draft.progress=RepeatProgress.Adaptive;view.Repaint();});
                float progressHeight=y+8;progress.sizeDelta=new Vector2(column,progressHeight);
                groupLayout.preferredHeight=wide?Mathf.Max(goalHeight,Mathf.Max(stopHeight,progressHeight)):goalHeight+stopHeight+progressHeight+gap*2;
                var edictHost=UiLayout.Rect("Repeat edict settings",view.Body);
                float edictHeight=edict.Draw(view,edictHost,width);
                edictHost.gameObject.AddComponent<LayoutElement>().preferredHeight=edictHeight;
                string message=notice!=""?notice:Maximum<1?Loc.T("남은 피로도가 1분 미만이므로 시간 목표를 선택할 수 없습니다."):Loc.T("전투 중 시간 목표가 끝나도 현재 판의 보상은 유지됩니다.");
                var note=Text(view.Body,"Repeat notice",message,0,0,width,UiTheme.Caption,Skin.Muted);note.gameObject.AddComponent<UnityEngine.UI.LayoutElement>().preferredHeight=Mathf.Max(44,note.rectTransform.rect.height);
                FitShell(Mathf.Max(36,help.rectTransform.rect.height),Actions(width));
                EdictPopupLayout.Fit(view.Frame,view.Scroll);
            }
            void Save(bool enabled)
            {
                var desired=draft.Copy();desired.enabled=enabled;
                if(!store.CommitRepeatSettings(heroId,opened,desired,pendingRunId)){notice=store.Error;view.Repaint();return;}
                view.Close();applied?.Invoke();
            }
        }
    }
}
