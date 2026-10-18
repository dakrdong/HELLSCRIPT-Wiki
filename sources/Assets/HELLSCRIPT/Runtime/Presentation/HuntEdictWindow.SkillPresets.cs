using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    public sealed partial class HuntEdictWindow
    {
        readonly Dictionary<string,string> previewSkillPresets=new Dictionary<string,string>();
        bool previewAttackOrder;
        public string SkillPolicyScope=>policySkill=="BASIC"&&previewAttackOrder?HuntEdictQuickPresets.AttackOrder:HuntEdictQuickPresets.SkillScope(policySkill,Session.Draft.edict.heroClass);
        HuntEdictLoadout OwnedSkillPolicy{get{var owned=new HuntEdictEditSession(hero);owned.UseSkillTree(hero);return owned.Draft;}}
        public string ActiveSkillPreset=>HuntEdictSkillPresets.Active(OwnedSkillPolicy,SkillPolicyScope);
        public string VisibleSkillPreset
        {
            get
            {
                if(previewSkillPresets.TryGetValue(SkillPolicyScope,out var id)&&(id==HuntEdictQuickPresets.Custom||SkillChoiceDisclosed(SkillPolicyScope,id)))return id;
                string active=HuntEdictSkillPresets.Active(Session.Draft,SkillPolicyScope);
                // An explicitly activated Custom tab always reopens as Custom.
                return active==HuntEdictQuickPresets.Custom&&!Session.Draft.classSkills.presetSelections.Any(p=>p.scope==SkillPolicyScope&&p.preset==HuntEdictQuickPresets.Custom)
                    ?HuntEdictQuickPresets.For(SkillPolicyScope).First().id:active;
            }
        }
        public void PreviewSkillPreset(string id)
        {
            if(id==HuntEdictQuickPresets.Custom?ProgressivePrologue||!HasFeature(HuntEdictProgression.Details):!SkillChoiceDisclosed(SkillPolicyScope,id))return;
            if(id!=HuntEdictQuickPresets.Custom&&!HuntEdictQuickPresets.For(SkillPolicyScope).Any(p=>p.id==id))throw new ArgumentException("Unknown preview preset.");
            previewSkillPresets[SkillPolicyScope]=id;offsets[ScrollKey]=0;mainScroll=null;Repaint();
        }
        public void ActivateSkillPreset()
        {
            try{SaveSkillPolicy(SkillPolicyScope,HuntEdictSkillPresets.Select(Session.Draft,SkillPolicyScope,VisibleSkillPreset));}
            catch(Exception e){Error(e.Message);}
        }
        void SaveSkillPolicy(string scope,HuntEdictLoadout next)
        {
            if(!Session.SaveSkillPolicy(hero,scope,next,commit)){Error(saveError());return;}
            customQuickScopes.Remove(scope);chosenQuickPresets.Remove(scope);Repaint();
        }
        void ChangeSkillPolicy(string scope,Action<HuntEdictLoadout> change)
        {
            try
            {
                var next=Session.Draft.Copy();change(next);
                SaveSkillPolicy(scope,HuntEdictSkillPresets.Select(next,scope,HuntEdictQuickPresets.Custom));
            }
            catch(Exception e){Error(e.Message);}
        }
        void DrawSkillPresetEditor(RectTransform parent,float x,float y,float w,float h)
        {
            string scope=SkillPolicyScope,shown=VisibleSkillPreset,active=ActiveSkillPreset;
            bool custom=shown==HuntEdictQuickPresets.Custom,usingShown=shown==active&&HuntEdictSkillPresets.SamePolicy(Session.Draft,OwnedSkillPolicy,scope);
            float heading=Look.SmallButton,backWidth=Mathf.Min(180,w*.31f);
            Button(parent,"스킬 트리로",x,y,backWidth,heading,()=>SelectSkillPage(true),"button-idle",Look.SmallText).name="edict-policy-back";
            if(policySkill=="BASIC")
            {
                float rest=w-backWidth-Look.Gap;
                Button(parent,"기본 공격",x+backWidth+Look.Gap,y,rest*.47f,heading,()=>{previewAttackOrder=false;mainScroll=null;Repaint();},previewAttackOrder?"tab-idle":"tab-current",Look.SmallText).name="edict-policy-basic";
                Button(parent,"공통 공격 순서",x+backWidth+Look.Gap+rest*.48f,y,rest*.52f,heading,()=>{previewAttackOrder=true;mainScroll=null;Repaint();},previewAttackOrder?"tab-current":"tab-idle",Look.SmallText).name="edict-policy-order";
            }
            else Text(parent,SkillName(policySkill),x+backWidth+Look.Gap,y,w-backWidth-Look.Gap,heading,Look.ChoiceTitle,gold);
            var tabs=Rect("Skill preset tabs",parent);Place(tabs,x,y+heading+6,w,100);
            var presets=HuntEdictQuickPresets.For(scope).Where(p=>SkillChoiceDisclosed(scope,p.id)).ToArray();
            var ids=presets.Select(p=>p.id).Concat(!ProgressivePrologue&&HasFeature(HuntEdictProgression.Details)?new[]{HuntEdictQuickPresets.Custom}:Array.Empty<string>()).ToArray();
            int columns=landscape?ids.Length:2,rows=(ids.Length+columns-1)/columns;
            float cell=w/columns,tabHeight=Look.Chip;
            var buttons=new List<Button>();
            for(int i=0;i<ids.Length;i++)
            {
                string id=ids[i],name=id==HuntEdictQuickPresets.Custom?Loc.T("직접 설정"):presets.Single(p=>p.id==id).Name;
                var b=Button(tabs,name,(i%columns)*cell,(i/columns)*tabHeight,cell-3,tabHeight,()=>PreviewSkillPreset(id),shown==id?"tab-current":"tab-idle",Look.BodyText+1);
                b.name="edict-preset-tab-"+id;var t=b.GetComponentInChildren<Text>();Place(t.rectTransform,6,0,cell-46,1000);t.alignment=TextAnchor.MiddleLeft;
                tabHeight=Mathf.Max(tabHeight,t.preferredHeight+12);buttons.Add(b);
            }
            for(int i=0;i<buttons.Count;i++)
            {
                var b=buttons[i];Place((RectTransform)b.transform,(i%columns)*cell,(i/columns)*(tabHeight+2),cell-3,tabHeight);
                Place(b.GetComponentInChildren<Text>().rectTransform,6,0,cell-46,tabHeight);
                string id=ids[i];bool selected=id==active;
                float box=30;
                var radio=Button(b.transform,"",cell-box-8,(tabHeight-box)/2,box,box,()=>
                {previewSkillPresets[scope]=id;offsets[ScrollKey]=0;ActivateSkillPreset();},selected?"preset-radio-on":"preset-radio-off",14);
                radio.name=id==shown?"edict-preset-activate":"edict-preset-radio-"+id;
                ((UiButton)radio).Configure(UiButtonRole.Icon,selected);radio.targetGraphic.canvasRenderer.SetAlpha(1);
                if(selected)HuntEdictDecor.Create(radio.transform,"Active policy check",0,0,box,box,"white-tick",skin);
                radio.interactable=!(selected&&HuntEdictSkillPresets.SamePolicy(Session.Draft,OwnedSkillPolicy,scope));
                if(selected){radio.GetComponentInChildren<Text>().color=skin.GreenInk;radio.GetComponentInChildren<Text>().name="Active preset mark";}
            }
            float tabTotal=rows*(tabHeight+2);Place(tabs,x,y+heading+6,w,tabTotal);
            float plateY=y+heading+6+tabTotal,plateH=h-(plateY-y);
            var plate=Panel(parent,"Skill preset panel","skill-inspector");Place(plate,x,plateY,w,plateH);
            float actions=0;
            float cp=Look.CardPadding;
            mainScroll=Scroll(plate,"Skill preset content",cp,actions+cp,w-2*cp,Mathf.Max(24,plateH-actions-2*cp),out var list);float inner=w-2*cp-16;
            if(custom)
            {
                ReleaseCombatPreview();
                if(!usingShown)
                {
                    Paragraph(list,"직접 설정을 활성화하면 세부 옵션을 조정할 수 있습니다.",inner,Look.BodyText,muted);
                }
                DrawCustomSkillPolicy(list,inner,scope);
                if(!usingShown)
                {
                    var preview=list.gameObject.AddComponent<CanvasGroup>();preview.interactable=false;preview.blocksRaycasts=false;preview.alpha=.65f;
                }
            }
            else
            {
                var preset=presets.Single(p=>p.id==shown);
                bool sideBySide=landscape||inner>=430*Grow;
                float descriptionWidth=sideBySide?inner*.28f:inner;
                var row=Row(list,"Preset explanation",100);
                var description=Text(row,preset.Description,5,5,descriptionWidth-10,1000,Look.BodyText,textColor,TextAnchor.UpperLeft);
                float descriptionHeight=description.preferredHeight+12;
                Place(description.rectTransform,5,5,descriptionWidth-10,descriptionHeight);
                float artX=sideBySide?descriptionWidth+8:0,artY=sideBySide?0:descriptionHeight+4,artW=sideBySide?inner-descriptionWidth-8:inner;
                float artH=artW*.68f;
                bool live=scope.StartsWith("skill/",StringComparison.Ordinal);
                if(landscape&&!live)
                {
                    // Keep the whole tactical scene visible even in short landscape windows with
                    // enlarged text. Description/legend can still scroll when they need extra lines.
                    artH=Mathf.Min(artH,Mathf.Max(104,plateH-actions-Lines(10,2)-18));
                    float fitted=artH/.68f;artX+=(artW-fitted)/2;artW=fitted;
                }
                if(live)
                {
                    Destroy(description.gameObject);description.gameObject.SetActive(false);
                    var information=Rect("Preset information",row);Place(information,5,5,descriptionWidth-10,1);
                    var layout=information.gameObject.AddComponent<VerticalLayoutGroup>();layout.spacing=4;
                    layout.childControlWidth=true;layout.childForceExpandWidth=true;layout.childControlHeight=true;layout.childForceExpandHeight=false;
                    float badgeHeight=Lines(Look.SmallText,1)+8;
                    var badge=Row(information,"Policy state",badgeHeight,usingShown?"policy-active":"policy-preview");
                    var badgeText=Text(badge,usingShown?"변경 즉시 저장":"미리보기",8,0,descriptionWidth-26,badgeHeight,Look.SmallText,usingShown?skin.GreenInk:textColor);
                    badgeText.font=UiFonts.CasualWeight(700);
                    Paragraph(information,preset.Description,descriptionWidth-10,Look.BodyText,textColor);
                    if(!ProgressivePrologue)DrawPreviewConditions(information,SkillPresetScenario.Find(policySkill,shown),descriptionWidth-10);
                    else Paragraph(information,"실제 초반 영웅의 복사본 · 동일한 시작 조건 · 1배속 10초 · 연습 결과는 저장되지 않습니다.",descriptionWidth-10,10,muted);
                    artH=sideBySide?Mathf.Max(150,plateH-actions-2*cp):300;
                    DrawCombatPreview(row,information,descriptionWidth-10,policySkill,shown,artX,artY,artW,artH);
                    DrawStarterControls(information,descriptionWidth-10);
                    LayoutRebuilder.ForceRebuildLayoutImmediate(information);descriptionHeight=LayoutUtility.GetPreferredHeight(information)+10;
                    Place(information,5,5,descriptionWidth-10,descriptionHeight);
                    if(!sideBySide)
                    {
                        // Narrow screens read top to bottom: what to do and the progress button, then the example it refers to.
                        artY=descriptionHeight+4;Place(row.Find("Actual skill combat preview") as RectTransform,artX,artY,artW,artH);
                        Place(information,5,5,descriptionWidth-10,descriptionHeight);descriptionHeight+=artH+8;
                    }
                }
                else {ReleaseCombatPreview();SkillPresetExampleView.Create(row,scope,shown,Session.Draft,font,artX,artY,artW,artH,lastScale,skin);}
                float total=Mathf.Max(descriptionHeight,artY+artH);
                row.GetComponent<LayoutElement>().minHeight=row.GetComponent<LayoutElement>().preferredHeight=total;
                if(!live)Paragraph(list,"원은 내 캐릭터 · 마름모는 적 · 화살표는 이동과 공격 방향입니다.",inner,Look.SmallText,muted);
            }
            EndList(list,6);
        }
        void DrawCustomSkillPolicy(RectTransform list,float w,string scope)
        {
            Action<Action<HuntEdictLoadout>> apply=change=>ChangeSkillPolicy(scope,change);
            if(scope==HuntEdictQuickPresets.AttackOrder)
            {
                Paragraph(list,"위에 있는 공격부터 사용을 검토합니다.",w,Look.SmallText,muted);
                DrawOrder(list,w,Skills.order,SkillName,(id,target)=>apply(d=>d.classSkills.order=HuntEdictReorder.MoveVisible(d.classSkills.order,d.classSkills.order,id,target)));return;
            }
            if(policySkill=="BASIC"){DrawActiveOptions(list,w,"BASIC",apply);return;}
            foreach(var option in ClassSkillOptions.For(hero.heroClass).Where(o=>o.skillId==policySkill))
            {
                var selected=ClassSkillOptions.Selected(Skills,option.id);
                Paragraph(list,Loc.Language=="en"?option.nameEn:option.name,w,Look.ChoiceTitle,gold);
                foreach(var choice in option.choices)
                {
                    var c=choice;var choiceRow=Row(list,"Skill use choice "+choice.id,Look.Button+4);
                    var b=Button(choiceRow,Loc.Language=="en"?choice.nameEn:choice.name,0,0,w,Look.Button,()=>apply(d=>d.classSkills=ClassSkillOptions.WithOption(d.classSkills,option.id,c.id)),selected.id==choice.id?"chip-on":"button-idle",Look.BodyText+1);
                    b.name="edict-policy-choice-"+choice.id;
                    float high=Mathf.Max(Look.Button,b.GetComponentInChildren<Text>().preferredHeight+8);
                    Place((RectTransform)b.transform,0,0,w,high);Place(b.GetComponentInChildren<Text>().rectTransform,4,0,w-8,high);
                    choiceRow.GetComponent<LayoutElement>().minHeight=choiceRow.GetComponent<LayoutElement>().preferredHeight=high+3;
                    if(selected.id==choice.id){Paragraph(list,ClassSkillTree.Display(Loc.Language=="en"?choice.benefitEn:choice.benefit),w,Look.BodyText,textColor);Paragraph(list,ClassSkillTree.Display(Loc.Language=="en"?choice.costEn:choice.cost),w,Look.SmallText,muted);}
                }
            }
            if(ClassSkills.Find(policySkill).legacy)DrawActiveOptions(list,w,policySkill,apply);
        }
    }
}
