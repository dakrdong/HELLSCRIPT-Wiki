using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    public sealed partial class GameUI
    {
        RectTransform casualEdictTabs;
        void BuildCasualEdictTabIcons()
        {
            if(casualEdictTabs!=null){casualEdictTabs.gameObject.SetActive(false);Destroy(casualEdictTabs.gameObject);}
            var hud=globalHud.Layout;float k=hud.presentationScale/quickScale;
            bool small=!hud.landscape||hud.compact;float pad=4*k;
            var bounds=CasualBattleHudLayout.EdictTabs(UiSafeArea.Current,hud,new Rect(quickAnchor.position*quickScale,quickAnchor.size*quickScale),LiveJournalHudInset);
            float width=bounds.width/quickScale,height=bounds.height/quickScale,row=(height-2*pad)/HuntEdictHudRows.Tabs.Length;
            casualEdictTabs=Rect("Casual edict tabs",quickRoot);CasualBattleHudStyle.Panel(casualEdictTabs);
            QuickRect(casualEdictTabs,bounds.x/quickScale,bounds.y/quickScale,width,height);
            var captions=new List<(Text label,RectTransform art)>();
            for(int i=0;i<HuntEdictHudRows.Tabs.Length;i++)
            {
                string tab=HuntEdictHudRows.Tabs[i];bool open=sheetTab==tab;
                var button=Button(casualEdictTabs,EdictTabTitle(tab),()=>OpenEdictSheet(tab),Color.clear);button.name="edict-sheet-tab-"+tab;
                QuickRect((RectTransform)button.transform,pad,height-pad-(i+1)*row,width-2*pad,row);
                var border=button.GetComponent<UIRectBorder>();if(border!=null)border.enabled=false;
                var art=Rect("Icon",button.transform);
                var image=art.gameObject.AddComponent<Image>();image.sprite=globalHud.Sprite(EdictTabArt(tab));image.preserveAspect=true;image.raycastTarget=false;
                bool available=EdictTabAvailable(tab);
                image.color=open?UiSkin.Casual.Gold:available?Color.white:UiSkin.Casual.Disabled;
                var label=button.GetComponentInChildren<Text>();label.fontSize=Mathf.RoundToInt((small?9:11)*k);label.color=UiSkin.Casual.Navy;
                label.font=UiFonts.CasualBody(true);label.resizeTextForBestFit=false;label.horizontalOverflow=HorizontalWrapMode.Wrap;
                captions.Add((label,art));
            }
            // A nine-row column uses one font size throughout, including long English captions.
            while(captions[0].label.fontSize>1&&captions.Any(c=>TextHeight(c.label,c.label.text,width-2*pad)+4*k>row))
                foreach(var caption in captions)caption.label.fontSize--;
            foreach(var caption in captions)
            {
                var label=caption.label;float icon=(small?30:44)*k;
                float captionWidth=width-2*pad,captionHeight=Mathf.Max(row-icon,Mathf.Ceil(TextHeight(label,label.text,captionWidth))+2*k);
                // Keep the fixed hit row and menu lane; complete captions own their measured height.
                icon=Mathf.Min(icon,Mathf.Max(0,row-captionHeight));
                QuickRect(caption.art,(captionWidth-icon)/2,row-icon,icon,icon);
                QuickRect(label.rectTransform,0,0,captionWidth,row-icon);
            }
            CasualEdictMenuChanged();
        }
        void BuildCombatEntryList(IReadOnlyList<HuntEdictUiGroup> groups)
        {
            var safe=UiSafeArea.Current;float k=(globalHud?.Layout?.presentationScale??1)/quickScale;
            float width=148*k,row=36*k,pad=4*k;
            int count=groups.Sum(g=>g.ids.Contains("position.engage")?2:1);
            float height=count*(row+pad)+pad;
            var anchor=quickRoot.GetComponentsInChildren<Button>().First(b=>b.name=="edict-sheet-tab-"+sheetTab);
            var corners=new Vector3[4];((RectTransform)anchor.transform).GetWorldCorners(corners);
            float x=Mathf.Max(safe.xMin/quickScale+pad,corners[0].x/quickScale-width-pad);
            float y=Mathf.Clamp(corners[2].y/quickScale-height,safe.yMin/quickScale+pad,safe.yMax/quickScale-height-pad);
            sheetCard=Box("edict-sheet",quickRoot,Color.clear);QuickRect(sheetCard,x,y,width,height);CasualBattleHudStyle.Panel(sheetCard);
            int item=0;
            void Add(string title,int group,bool opening,string name)
            {
                var button=Button(sheetCard,title,()=>{positionOpening=opening;SelectSheetGroup(group);});button.name=name;
                QuickRect((RectTransform)button.transform,pad,height-pad-(++item)*(row+pad)+pad,width-2*pad,row);
                EdictButtonSkin(button);var label=button.GetComponentInChildren<Text>();label.fontSize=Mathf.RoundToInt(12*k);label.color=UiSkin.Casual.Navy;
            }
            for(int i=0;i<groups.Count;i++)
            {
                if(groups[i].ids.Contains("position.engage"))
                {
                    if(HuntEdictProgression.Visible(game.Store.Data,game.Store.Data.Hero,"position.engage"))Add("첫 교전 위치",i,true,"edict-entry-opening");
                    if(HuntEdictProgression.Visible(game.Store.Data,game.Store.Data.Hero,"position.mode"))Add("전투 중 위치",i,false,"edict-entry-combat");
                }
                else Add(groups[i].title,i,true,"edict-sheet-group-"+i);
            }
            SheetEntrance(sheetCard,"edict-entry-list");
        }
        // The existing compact editors use a lower-screen anchor. Scope that layout input while
        // retaining the real upper-right button anchor for tabs, future reflows and input.
        void BuildCasualCompatibleEdictPanel(Action build)
        {
            Rect anchor=quickAnchor;var safe=UiSafeArea.Current;
            try
            {
                if(globalHud?.Layout!=null&&safe.height>=safe.width)
                {
                    float unit=Mathf.Min(safe.width/440,safe.height/956)/quickScale;
                    quickAnchor=new Rect(anchor.x,safe.yMin/quickScale+164*unit-anchor.height,anchor.width,anchor.height);
                }
                build();
            }
            finally{quickAnchor=anchor;}
        }
        void CasualEdictMenuChanged()
        {
            if(!CasualCombatHud)return;
            battleSize=Vector2.zero;ReflowBattleHud();
        }
    }
}
