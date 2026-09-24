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
                if(id==HuntEdictQuickPresets.Custom)
                {
                    customQuickScopes.Add(scope);
                    collapsed.Remove(scope.StartsWith("basic/",StringComparison.Ordinal)?"skill/BASIC":scope);
                }
                else
                {
                    var next=HuntEdictQuickPresets.Apply(Session.Draft,scope,id);
                    Session.ReplaceDraft(next);customQuickScopes.Remove(scope);chosenQuickPresets[scope]=id;
                }
                if(scope.StartsWith("global/",StringComparison.Ordinal))offsets[EditorKey]=0;
                else offsets[ScrollKey]=0;
                mainScroll=editorScroll=null;Repaint();
            }
            catch(Exception e){Error(e.Message);}
        }
        bool DrawQuickPresetPicker(RectTransform list,float w,string scope)
        {
            string selected=QuickPresetSelection(scope);bool custom=selected==HuntEdictQuickPresets.Custom;
            bool expanded=customQuickScopes.Contains(scope);
            var preset=custom?null:HuntEdictQuickPresets.For(scope).Single(p=>p.id==selected);
            Paragraph(list,"간편 프리셋",w,11,muted);
            var row=Row(list,"Quick preset selector",36);
            var button=Button(row,(preset?.Name??Loc.T(expanded?"직접 설정":"현재 설정"))+"  ›",0,0,w,36,()=>QuickPresetDialog(scope),"input-well",13);
            button.name="edict-quick-picker-"+scope;
            var label=button.GetComponentInChildren<Text>();label.alignment=TextAnchor.MiddleLeft;
            Place(label.rectTransform,8,0,w-16,1000);
            float h=Mathf.Max(36,label.preferredHeight+10);
            row.GetComponent<LayoutElement>().minHeight=row.GetComponent<LayoutElement>().preferredHeight=h;
            Place((RectTransform)button.transform,0,0,w,h);Place(label.rectTransform,8,0,w-16,h);
            Paragraph(list,preset?.Description??Loc.T(expanded?"현재 설정을 유지한 채 모든 세부 옵션을 직접 조정합니다.":"프리셋을 고르거나 직접 설정을 선택해 세부 옵션을 조정하세요."),w,11,muted);
            return expanded;
        }
        void QuickPresetDialog(string scope)
        {
            Dialog("간편 프리셋",p=>
            {
                float h=Mathf.Min(height-16,470);
                Scroll(p,"Quick preset choices",12,51,DialogWidth-24,h-61,out var list);
                string selected=QuickPresetSelection(scope);float w=DialogWidth-38;
                foreach(var preset in HuntEdictQuickPresets.For(scope))
                    QuickPresetChoice(list,w,scope,preset.id,preset.Name,preset.Description,selected==preset.id);
                // Keep this last even when the number of authored presets changes.
                QuickPresetChoice(list,w,scope,HuntEdictQuickPresets.Custom,Loc.T("직접 설정"),Loc.T("현재 설정을 유지한 채 모든 세부 옵션을 직접 조정합니다."),selected==HuntEdictQuickPresets.Custom);
                EndList(list,4);
            },470);
        }
        void QuickPresetChoice(RectTransform list,float w,string scope,string id,string title,string description,bool selected)
        {
            var row=Row(list,"Quick preset "+id,70);
            var button=Button(row,"",0,0,w,70,()=>SelectQuickPreset(scope,id),selected?"tab-current":"button-idle");
            button.name="edict-quick-choice-"+id;
            var name=Text(button.transform,(selected?"✓ ":"")+title,9,5,w-18,1000,13,selected?gold:textColor,TextAnchor.UpperLeft);
            float nameHeight=name.preferredHeight+4;Place(name.rectTransform,9,5,w-18,nameHeight);
            var detail=Text(button.transform,description,9,nameHeight+6,w-18,1000,11,muted,TextAnchor.UpperLeft);
            float detailHeight=detail.preferredHeight+4;Place(detail.rectTransform,9,nameHeight+6,w-18,detailHeight);
            float h=nameHeight+detailHeight+13;
            row.GetComponent<LayoutElement>().minHeight=row.GetComponent<LayoutElement>().preferredHeight=h+3;
            Place((RectTransform)button.transform,0,0,w,h);
        }
        string QuickGroupSummary(HuntEdictUiGroup group)
        {
            string scope=HuntEdictQuickPresets.GlobalScope(group),selected=QuickPresetSelection(scope);
            return selected==HuntEdictQuickPresets.Custom?Loc.T("직접 설정")+" · "+HuntEdictSummary.ForGroup(group,Session.Draft.edict,SkillName):HuntEdictQuickPresets.For(scope).Single(p=>p.id==selected).Name;
        }
    }
}
