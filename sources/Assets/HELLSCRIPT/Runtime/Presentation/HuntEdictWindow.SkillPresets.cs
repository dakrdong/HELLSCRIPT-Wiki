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
                if(previewSkillPresets.TryGetValue(SkillPolicyScope,out var id))return id;
                string active=HuntEdictSkillPresets.Active(Session.Draft,SkillPolicyScope);
                // Existing unmatched settings remain active, but do not open a wall of advanced options
                // on the first visit. An explicitly activated Custom tab always reopens as Custom.
                return active==HuntEdictQuickPresets.Custom&&!Session.Draft.classSkills.presetSelections.Any(p=>p.scope==SkillPolicyScope&&p.preset==HuntEdictQuickPresets.Custom)
                    ?HuntEdictQuickPresets.For(SkillPolicyScope)[0].id:active;
            }
        }
        public void PreviewSkillPreset(string id)
        {
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
            float heading=31*Grow;
            Button(parent,"스킬 트리로",x,y,101*Grow,heading,()=>SelectSkillPage(true),"button-idle",11).name="edict-policy-back";
            if(policySkill=="BASIC")
            {
                float rest=w-106*Grow;
                Button(parent,"기본 공격",x+106*Grow,y,rest*.47f,heading,()=>{previewAttackOrder=false;mainScroll=null;Repaint();},previewAttackOrder?"tab-idle":"tab-current",10).name="edict-policy-basic";
                Button(parent,"공통 공격 순서",x+106*Grow+rest*.48f,y,rest*.52f,heading,()=>{previewAttackOrder=true;mainScroll=null;Repaint();},previewAttackOrder?"tab-current":"tab-idle",10).name="edict-policy-order";
            }
            else Text(parent,SkillName(policySkill),x+110*Grow,y,w-110*Grow,heading,13,gold);
            var tabs=Rect("Skill preset tabs",parent);Place(tabs,x,y+heading+6,w,100);
            var presets=HuntEdictQuickPresets.For(scope);
            var ids=presets.Select(p=>p.id).Concat(new[]{HuntEdictQuickPresets.Custom}).ToArray();
            int columns=landscape?ids.Length:2,rows=(ids.Length+columns-1)/columns;
            float cell=w/columns,tabHeight=34*Grow;
            var buttons=new List<Button>();
            for(int i=0;i<ids.Length;i++)
            {
                string id=ids[i],name=id==HuntEdictQuickPresets.Custom?Loc.T("직접 설정"):presets.Single(p=>p.id==id).Name;
                var b=Button(tabs,name,(i%columns)*cell,(i/columns)*tabHeight,cell-3,tabHeight,()=>PreviewSkillPreset(id),shown==id?"tab-current":"tab-idle",11);
                b.name="edict-preset-tab-"+id;var t=b.GetComponentInChildren<Text>();Place(t.rectTransform,6,0,cell-28,1000);t.alignment=TextAnchor.MiddleLeft;
                tabHeight=Mathf.Max(tabHeight,t.preferredHeight+12);buttons.Add(b);
            }
            for(int i=0;i<buttons.Count;i++)
            {
                var b=buttons[i];Place((RectTransform)b.transform,(i%columns)*cell,(i/columns)*(tabHeight+2),cell-3,tabHeight);
                Place(b.GetComponentInChildren<Text>().rectTransform,6,0,cell-28,tabHeight);
                if(ids[i]==active){Glyph(b.transform,"confirm",cell-23,(tabHeight-13)/2,13,13,UiTheme.Success);b.transform.GetChild(b.transform.childCount-1).name="Active preset mark";}
            }
            float tabTotal=rows*(tabHeight+2);Place(tabs,x,y+heading+6,w,tabTotal);
            float plateY=y+heading+6+tabTotal,plateH=h-(plateY-y);
            var plate=Panel(parent,"Skill preset panel","skill-inspector");Place(plate,x,plateY,w,plateH);
            float actionWidth=Mathf.Min(w*.55f,150*Grow),actions=38*Grow;
            Text(plate,usingShown?"변경 즉시 저장":"미리보기",8,0,w-actionWidth-20,actions,10,usingShown?UiTheme.Success:muted);
            var use=Button(plate,usingShown?"활성 중":custom?"직접 설정 활성화":"프리셋 활성화",w-actionWidth-6,4,actionWidth,actions-8,ActivateSkillPreset,usingShown?"chip-on":"button-primary",11);
            use.name="edict-preset-activate";use.interactable=!usingShown;
            if(usingShown)use.GetComponentInChildren<Text>().color=UiTheme.Success;
            mainScroll=Scroll(plate,"Skill preset content",6,actions,w-12,Mathf.Max(24,plateH-actions-6),out var list);float inner=w-26;
            if(custom)
            {
                if(!usingShown)
                {
                    Paragraph(list,"직접 설정을 활성화하면 세부 옵션을 조정할 수 있습니다.",inner,12,muted);
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
                float descriptionWidth=sideBySide?inner*.36f:inner;
                var row=Row(list,"Preset explanation",100);
                var description=Text(row,preset.Description,5,5,descriptionWidth-10,1000,12,textColor,TextAnchor.UpperLeft);
                float descriptionHeight=description.preferredHeight+12;
                Place(description.rectTransform,5,5,descriptionWidth-10,descriptionHeight);
                float artX=sideBySide?descriptionWidth+8:0,artY=sideBySide?0:descriptionHeight+4,artW=sideBySide?inner-descriptionWidth-8:inner;
                float artH=artW*.68f;
                if(landscape)
                {
                    // Keep the whole tactical scene visible even in short landscape windows with
                    // enlarged text. Description/legend can still scroll when they need extra lines.
                    artH=Mathf.Min(artH,Mathf.Max(104,plateH-actions-Lines(10,2)-18));
                    float fitted=artH/.68f;artX+=(artW-fitted)/2;artW=fitted;
                }
                SkillPresetExampleView.Create(row,scope,shown,Session.Draft,font,artX,artY,artW,artH,lastScale);
                float total=Mathf.Max(descriptionHeight,artY+artH);
                row.GetComponent<LayoutElement>().minHeight=row.GetComponent<LayoutElement>().preferredHeight=total;
                Paragraph(list,"원은 내 캐릭터 · 마름모는 적 · 화살표는 이동과 공격 방향입니다.",inner,10,muted);
            }
            EndList(list,6);
        }
        void DrawCustomSkillPolicy(RectTransform list,float w,string scope)
        {
            Action<Action<HuntEdictLoadout>> apply=change=>ChangeSkillPolicy(scope,change);
            if(scope==HuntEdictQuickPresets.AttackOrder)
            {
                Paragraph(list,"위에 있는 공격부터 사용을 검토합니다.",w,11,muted);
                DrawOrder(list,w,Skills.order,SkillName,(id,target)=>apply(d=>d.classSkills.order=HuntEdictReorder.MoveVisible(d.classSkills.order,d.classSkills.order,id,target)));return;
            }
            if(policySkill=="BASIC"){DrawActiveOptions(list,w,"BASIC",apply);return;}
            bool automatic=Skills.automatic.Contains(policySkill);var row=Row(list,"Automatic use",36*Grow);
            Button(row,automatic?"자동 사용 켜짐":"자동 사용 꺼짐",0,0,w,32*Grow,()=>apply(d=>d.classSkills.automatic=automatic?d.classSkills.automatic.Where(id=>id!=policySkill).ToArray():d.classSkills.automatic.Concat(new[]{policySkill}).ToArray()),automatic?"chip-on":"button-idle",11).name="edict-skill-automatic";
            foreach(var option in ClassSkillOptions.For(hero.heroClass).Where(o=>o.skillId==policySkill))
            {
                var selected=ClassSkillOptions.Selected(Skills,option.id);
                Paragraph(list,Loc.Language=="en"?option.nameEn:option.name,w,12,gold);
                foreach(var choice in option.choices)
                {
                    var c=choice;var choiceRow=Row(list,"Skill use choice "+choice.id,36*Grow);
                    var b=Button(choiceRow,Loc.Language=="en"?choice.nameEn:choice.name,0,0,w,32*Grow,()=>apply(d=>d.classSkills=ClassSkillOptions.WithOption(d.classSkills,option.id,c.id)),selected.id==choice.id?"chip-on":"button-idle",11);
                    b.name="edict-policy-choice-"+choice.id;
                    float high=Mathf.Max(32*Grow,b.GetComponentInChildren<Text>().preferredHeight+8);
                    Place((RectTransform)b.transform,0,0,w,high);Place(b.GetComponentInChildren<Text>().rectTransform,4,0,w-8,high);
                    choiceRow.GetComponent<LayoutElement>().minHeight=choiceRow.GetComponent<LayoutElement>().preferredHeight=high+3;
                    if(selected.id==choice.id){Paragraph(list,ClassSkillTree.Display(Loc.Language=="en"?choice.benefitEn:choice.benefit),w,11,textColor);Paragraph(list,ClassSkillTree.Display(Loc.Language=="en"?choice.costEn:choice.cost),w,10,muted);}
                }
            }
            if(ClassSkills.Find(policySkill).legacy)DrawActiveOptions(list,w,policySkill,apply);
        }
    }
}
