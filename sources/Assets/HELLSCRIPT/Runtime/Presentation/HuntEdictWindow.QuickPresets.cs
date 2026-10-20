using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    public sealed partial class HuntEdictWindow
    {
        // View state only: selecting Custom never edits the saved values. The preset is inferred
        // from actual settings on reopening, including presets imported through a share code.
        readonly HashSet<string> customQuickScopes=new HashSet<string>();
        readonly Dictionary<string,string> chosenQuickPresets=new Dictionary<string,string>();
        public string QuickPresetSelection(string scope)
        {
            if(HuntEdictSkillPresets.Owns(scope))return HuntEdictSkillPresets.Active(Session.Draft,scope);
            if(customQuickScopes.Contains(scope))return HuntEdictQuickPresets.Custom;
            if(chosenQuickPresets.TryGetValue(scope,out var id)&&HuntEdictQuickPresets.Matches(Session.Draft,scope,id))return id;
            return HuntEdictQuickPresets.Match(Session.Draft,scope);
        }
        public void SelectQuickPreset(string scope,string id)
        {
            try
            {
                // Validate the scope even when selecting the presentation-only Custom entry.
                HuntEdictQuickPresets.For(scope);
                if(id==HuntEdictQuickPresets.Custom?!HasFeature(HuntEdictProgression.Details):!QuickDisclosed(scope,id))return;
                if(id==HuntEdictQuickPresets.Custom)
                {
                    // Custom from the overview means "show me the details": open the group where they live.
                    if(SelectedTab=="overview"&&scope.StartsWith("global/",StringComparison.Ordinal))
                    {DismissDialog();FocusGlobalOption(HuntEdictUiCatalog.Data.groups.Single(g=>HuntEdictQuickPresets.GlobalScope(g)==scope).ids[0]);return;}
                    customQuickScopes.Add(scope);
                    collapsed.Remove(scope.StartsWith("basic/",StringComparison.Ordinal)?"skill/BASIC":scope);
                }
                else
                {
                    Session.ApplyQuickPreset(scope,id);customQuickScopes.Remove(scope);chosenQuickPresets[scope]=id;
                }
                if(scope.StartsWith("global/",StringComparison.Ordinal))offsets[EditorKey]=0;
                else offsets[ScrollKey]=0;
                mainScroll=editorScroll=null;Repaint();
            }
            catch(Exception e){Error(e.Message);}
        }
        // The mockup's .stack: the small "Quick preset" label, the picker button, then the preset's own note, 8 px apart.
        bool DrawQuickPresetPicker(RectTransform list,float w,string scope)
        {
            string selected=QuickPresetSelection(scope);bool custom=selected==HuntEdictQuickPresets.Custom;
            bool expanded=customQuickScopes.Contains(scope);
            var preset=custom?null:HuntEdictQuickPresets.For(scope).Single(p=>p.id==selected);
            var stack=Row(list,"Quick preset stack",1);
            float labelLine=Look.SmallText*JuaLine;Line(stack,Loc.T("간편 프리셋"),0,0,w,Look.SmallText,labelLine,muted,true);
            float y=labelLine+8,titleSize=Look.CardTitle-1;
            var button=Button(stack,preset?.Name??Loc.T(expanded?"직접 설정":"현재 설정"),0,y,w,Look.Button,()=>QuickPresetDialog(scope),"input-well",titleSize);
            button.name="edict-quick-picker-"+scope;((UiButton)button).SetCasualAppearance(radius:14,borderWidth:2,shadow:3,edge:skin.Navy);
            var label=button.GetComponentInChildren<Text>();label.alignment=TextAnchor.MiddleLeft;
            float labelWidth=Mathf.Max(1,w-18-2-14-18-10);Place(label.rectTransform,18,0,labelWidth,1000);
            float h=Mathf.Max(Look.Button,label.preferredHeight+16);
            Place((RectTransform)button.transform,0,y,w,h);Place(label.rectTransform,18,0,labelWidth,h);
            SurfaceChoice(button,false,idleSkin:"input-well");HuntEdictDecor.Create(button.transform,"Picker chevron",w-2-14-18,(h-18)/2,18,18,"chevron",skin);
            y+=h+8;
            var note=Body(stack,preset?.Description??Loc.T(expanded?"현재 설정을 유지한 채 모든 세부 옵션을 직접 조정합니다.":"프리셋을 고르거나 직접 설정을 선택해 세부 옵션을 조정하세요."),0,y,w,Look.SmallText,1.5f,muted,out float noteHeight);note.name="Quick preset note";
            var layout=stack.GetComponent<LayoutElement>();layout.minHeight=layout.preferredHeight=y+noteHeight;
            return expanded&&HasFeature(HuntEdictProgression.Details);
        }
        void QuickPresetDialog(string scope)
        {
            Dialog("간편 프리셋",p=>
            {
                float h=Mathf.Min(height-16,470);
                float top=DialogTop;
                Scroll(p,"Quick preset choices",12,top,DialogWidth-24,Mathf.Max(1,h-top-Look.Gap),out var list);
                string selected=QuickPresetSelection(scope);float w=DialogWidth-38;
                foreach(var preset in HuntEdictQuickPresets.For(scope).Where(p=>QuickDisclosed(scope,p.id)))
                    QuickPresetChoice(list,w,scope,preset.id,preset.Name,preset.Description,selected==preset.id);
                // Keep this last even when the number of authored presets changes.
                if(HasFeature(HuntEdictProgression.Details))QuickPresetChoice(list,w,scope,HuntEdictQuickPresets.Custom,Loc.T("직접 설정"),Loc.T("현재 설정을 유지한 채 모든 세부 옵션을 직접 조정합니다."),selected==HuntEdictQuickPresets.Custom);
                EndList(list,4);
            },470);
        }
        void QuickPresetChoice(RectTransform list,float w,string scope,string id,string title,string description,bool selected)
        {
            float cp=Look.CardPadding,textWidth=Mathf.Max(1,w-cp*2-34);
            var row=Row(list,"Quick preset "+id,Look.Button);
            var button=Button(row,"",0,0,w,Look.Button,()=>SelectQuickPreset(scope,id),"button-idle");
            button.name="edict-quick-choice-"+id;SurfaceChoice(button,selected);
            var name=Text(button.transform,title,cp,cp,textWidth,1000,Look.BodyText+1,textColor,TextAnchor.UpperLeft);skin.ApplyLabel(name,true,textColor);
            float nameHeight=name.preferredHeight+4;Place(name.rectTransform,cp,cp,textWidth,nameHeight);
            var detail=Text(button.transform,description,cp,cp+nameHeight+4,w-cp*2,1000,Look.SmallText,muted,TextAnchor.UpperLeft);skin.ApplyLabel(detail,false,muted);detail.lineSpacing=1.5f;
            float detailHeight=detail.preferredHeight+4;Place(detail.rectTransform,cp,cp+nameHeight+4,w-cp*2,detailHeight);
            float h=Mathf.Max(Look.Button,nameHeight+detailHeight+cp*2+4);
            row.GetComponent<LayoutElement>().minHeight=row.GetComponent<LayoutElement>().preferredHeight=h+3;
            Place((RectTransform)button.transform,0,0,w,h);ChoiceCheck(button.transform,w,h,selected);
        }
        // The selector exists for groups whose settings come in authored presets (not auto equip). Quick() owns which pickers exist: new accounts
        // never get the potion one, legacy accounts keep every picker (the v2 survival lesson points at it). The v3 prologue lists direct options
        // only, even in the replay of a legacy account, because its HP lesson points at the option itself.
        bool QuickAllowed(HuntEdictUiGroup group)
        {
            if(ProgressivePrologue||group.tab=="autoEquip")return false;
            string scope=HuntEdictQuickPresets.GlobalScope(group);
            return HuntEdictQuickPresets.For(scope).Any(p=>QuickDisclosed(scope,p.id));
        }
        // The current answer shown under a group chip: the matching preset's name, or "Custom".
        string QuickSubtitle(HuntEdictUiGroup group)
        {
            if(group.ids.Any(id=>!Disclosed(id))||group.ids.Contains("bag.warehouseFull")||!QuickAllowed(group))return "";
            string scope=HuntEdictQuickPresets.GlobalScope(group),selected=QuickPresetSelection(scope);
            return selected==HuntEdictQuickPresets.Custom?Loc.T("직접 설정"):HuntEdictQuickPresets.For(scope).FirstOrDefault(p=>p.id==selected)?.Name??"";
        }
    }
}
