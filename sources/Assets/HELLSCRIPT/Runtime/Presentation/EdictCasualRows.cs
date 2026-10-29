using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    // Shared controls for the compact edict sheets and edict sections inside the existing settings windows.
    public static class EdictCasualRows
    {
        public static string NumberSuffix(string id)
        {
            if(id.EndsWith("Percent",StringComparison.Ordinal)||id.EndsWith(".level",StringComparison.Ordinal))return "%";
            if(id.EndsWith("Seconds",StringComparison.Ordinal)||id=="position.gatherTime"||id=="target.chaseTime"||id=="dodge.window"||id=="explore.cursedMinimumTime")return Loc.T("초");
            if(id=="repeat.minutes")return Loc.T("분");
            if(id.StartsWith("HEC-",StringComparison.Ordinal)||id=="position.distance"||id=="position.engageDistance"||id=="target.chaseDistance"||id=="loot.distance"||id=="explore.chestDetour"||id=="explore.shrineDetour")return "m";
            return "";
        }
        public static string ChoiceText(EdictOptionDefinition d,string code)
            =>d.kind==EdictOptionKind.Toggle?Loc.T(code=="ON"?"켜짐":"꺼짐"):d.kind==EdictOptionKind.SkillReference?(code==""?Loc.T("지정 안 함"):ClassSkillTree.Name(code)):Loc.T(EdictOptionLabels.Item(d,code));
        public static void Choices(RectTransform card,float width,float unit,HuntEdictHudRows.Row row,HuntEdictV2Document edict,
            Action<string,string> apply,Func<Transform,string,Action,Button> create,Action<Button,bool> style,Func<string,string> text=null,float minimumHeight=36)
        {
            var d=row.definition;var choices=HuntEdictHudRows.Choices(d,edict);bool multi=d.kind==EdictOptionKind.Set;
            var selected=(multi?row.value.Split(',').Where(c=>c!=""):new[]{row.value}).ToHashSet();
            if(choices.Count==0)return;
            int cols=d.kind==EdictOptionKind.Toggle?2:d.kind==EdictOptionKind.Set?Math.Min(choices.Count,5):choices.Count<=3?choices.Count:2;
            float gap=6*unit,cell=(width-gap*(cols-1))/cols;
            var grid=UiLayout.Rect("Choices",card);var layout=grid.gameObject.AddComponent<GridLayoutGroup>();var labels=new List<Text>();
            foreach(string code in choices)
            {
                string c=code;bool on=selected.Contains(c);
                var chip=create(grid,text?.Invoke(c)??ChoiceText(d,c),()=>
                {
                    if(!multi){if(!on)apply(d.id,c);return;}
                    apply(d.id,HuntEdictV2Editing.ToggledInSet(d,row.value,c));
                });
                chip.name="edict-sheet-choice-"+d.id+"-"+(c==""?"none":c);chip.interactable=row.editable;style(chip,on);
                var label=chip.GetComponentInChildren<Text>();UiLayout.Stretch(label.rectTransform);label.rectTransform.offsetMin=new Vector2(6*unit,2*unit);label.rectTransform.offsetMax=new Vector2(-6*unit,-2*unit);labels.Add(label);
            }
            float height=Mathf.Max(minimumHeight*unit,labels.Max(t=>
            {
                var settings=t.GetGenerationSettings(new Vector2(Mathf.Max(1,cell-12*unit),0));
                return Mathf.Ceil(t.cachedTextGeneratorForLayout.GetPreferredHeight(t.text,settings)/t.pixelsPerUnit)+14*unit;
            }));
            layout.cellSize=new Vector2(cell,height);layout.spacing=new Vector2(gap,gap);layout.constraint=GridLayoutGroup.Constraint.FixedColumnCount;layout.constraintCount=cols;
            grid.gameObject.AddComponent<LayoutElement>().preferredHeight=Mathf.CeilToInt(choices.Count/(float)cols)*(height+gap)-gap;
        }
        public static float Row(ContentWindowView view,Transform parent,float width,HuntEdictHudRows.Row row,HuntEdictV2Document edict,Action<string,string> apply,Func<string,string> choiceText=null)
        {
            var skin=UiSkin.Casual;var d=row.definition;var card=UiLayout.Rect("edict-sheet-row-"+d.id,parent);UiLayout.Place(card,0,0,width,1);
            var surface=card.gameObject.AddComponent<StorageSurface>();surface.raycastTarget=false;surface.SetSkin(skin,border:skin.CardEdge,radius:12,shadow:2,fill:skin.White,fillEnd:skin.Cream);
            var stack=card.gameObject.AddComponent<VerticalLayoutGroup>();stack.padding=new RectOffset(10,10,8,10);stack.spacing=6;stack.childControlWidth=stack.childControlHeight=true;stack.childForceExpandWidth=true;stack.childForceExpandHeight=false;
            var title=UiLayout.Text(card,"Title",HuntEdictUiCatalog.Label(d),0,0,width-20,1,15,row.editable?skin.Navy:skin.Muted,skin.BodyFont);
            title.gameObject.AddComponent<LayoutElement>().preferredHeight=Mathf.Max(22,Mathf.Ceil(title.preferredHeight));
            if(d.kind==EdictOptionKind.Number)Number(view,card,width-20,row,apply);
            else Choices(card,width-20,1,row,edict,apply,(host,label,click)=>view.Button(host,label,0,0,1,1,click),(button,on)=>{UiTheme.Choice(button,on,tab:false);button.GetComponentInChildren<Text>().font=skin.BodyFont;},choiceText,44);
            LayoutRebuilder.ForceRebuildLayoutImmediate(card);float height=LayoutUtility.GetPreferredHeight(card);card.sizeDelta=new Vector2(width,height);return height;
        }
        static void Number(ContentWindowView view,RectTransform parent,float width,HuntEdictHudRows.Row row,Action<string,string> apply)
        {
            var d=row.definition;var skin=UiSkin.Casual;
            var line=UiLayout.Rect("Number row",parent);line.gameObject.AddComponent<LayoutElement>().preferredHeight=110;
            string suffix=NumberSuffix(d.id);
            var value=UiLayout.Text(line,"Number value",row.value+suffix,48,0,width-96,44,18,skin.Navy,skin.BodyFont);value.alignment=TextAnchor.MiddleCenter;
            void Step(int direction)=>apply(d.id,HuntEdictV2Editing.Stepped(d,row.value,direction));
            var minus=view.Button(line,"−",0,0,44,44,()=>Step(-1));minus.name="edict-sheet-minus-"+d.id;
            var plus=view.Button(line,"+",width-44,0,44,44,()=>Step(1));plus.name="edict-sheet-plus-"+d.id;
            foreach(var button in new[]{minus,plus})button.GetComponentInChildren<Text>().font=skin.BodyFont;
            minus.interactable=row.editable&&float.Parse(row.value,CultureInfo.InvariantCulture)>d.min;plus.interactable=row.editable&&float.Parse(row.value,CultureInfo.InvariantCulture)<d.max;
            var area=UiLayout.Rect("edict-sheet-number-"+d.id,line);UiLayout.Place(area,10,50,width-20,28);area.gameObject.AddComponent<Image>().color=Color.clear;
            var slider=area.gameObject.AddComponent<SettingsStepSlider>();slider.ScrollOnVerticalDrag=true;slider.minValue=0;slider.maxValue=(d.max-d.min)/d.step;slider.wholeNumbers=true;slider.transition=Selectable.Transition.None;
            var track=UiLayout.Rect("Track",area);UiLayout.Place(track,0,10,width-20,8);track.gameObject.AddComponent<StorageSurface>().SetSkin(skin,radius:4,shadow:0,fill:skin.Disabled,fillEnd:skin.Disabled);
            var travel=UiLayout.Rect("Handle area",area);UiLayout.Stretch(travel);travel.offsetMin=new Vector2(8,0);travel.offsetMax=new Vector2(-8,0);
            var handle=UiLayout.Rect("Handle",travel);handle.sizeDelta=new Vector2(18,28);var face=handle.gameObject.AddComponent<StorageSurface>();face.SetSkin(skin,radius:8,shadow:2,fill:skin.Gold,fillEnd:skin.GoldShade);
            slider.handleRect=handle;slider.targetGraphic=face;slider.SetValueWithoutNotify((float.Parse(row.value,CultureInfo.InvariantCulture)-d.min)/d.step);slider.interactable=row.editable;
            string Selected()=>((decimal)d.min+(decimal)slider.value*(decimal)d.step).ToString("0.###",CultureInfo.InvariantCulture);
            void Save(){string next=Selected();if(next!=row.value)apply(d.id,next);}
            slider.onValueChanged.AddListener(_=>{value.text=Selected()+suffix;if(!slider.Dragging)Save();});slider.Released=Save;
            UiLayout.Text(line,"Number range",d.min.ToString("0.###",CultureInfo.InvariantCulture)+suffix+" — "+d.max.ToString("0.###",CultureInfo.InvariantCulture)+suffix,0,84,width,22,12,skin.Muted,skin.BodyFont);
        }
    }
}
