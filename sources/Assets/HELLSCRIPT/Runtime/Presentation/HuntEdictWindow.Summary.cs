using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    public sealed partial class HuntEdictWindow
    {
        string selectedGroup,searchQuery="";bool changedOnly,groupDetails;ScrollRect editorScroll;
        string EditorKey=>SelectedTab=="overview"?"overview/settings":"group/"+selectedGroup;
        public string SelectedGroup=>selectedGroup;
        public string SearchQuery=>searchQuery;
        public bool ChangedOnly=>changedOnly;
        public bool GroupDetails=>groupDetails;
        public void Search(string query,bool changesOnly=false)
        {searchQuery=query??"";changedOnly=changesOnly;groupDetails=false;Repaint();}
        // Opens the group holding the option and brings the option (or its preset picker) into view. False when nothing could be shown.
        public bool FocusGlobalOption(string id)
        {
            if(DefaultSettingsLocked||!Disclosed(id))return false;
            var group=HuntEdictUiCatalog.Data.groups.FirstOrDefault(g=>g.ids.Contains(id));if(group==null)return false;
            string scope=HuntEdictQuickPresets.GlobalScope(group);
            if(HasFeature(HuntEdictProgression.Details))customQuickScopes.Add(scope);SelectGroup(HuntEdictSummary.Key(group));
            return EnsureVisible("edict-option-"+id)||EnsureVisible("edict-quick-picker-"+scope);
        }
        public void SelectGroup(string key)
        {
            if(DefaultSettingsLocked)return;
            var group=HuntEdictUiCatalog.Data.groups.FirstOrDefault(g=>HuntEdictSummary.Key(g)==key);if(group==null||!GroupDisclosed(group))return;
            if(searchQuery.Length>0&&HasFeature(HuntEdictProgression.Details))customQuickScopes.Add(HuntEdictQuickPresets.GlobalScope(group));
            RememberScroll();addingEquipmentOption=false;selectedGroup=key;groupMemory[group.tab]=key;openedGroups.Add(OpenedKey(key));afterSave=null;groupDetails=true;SelectedTab=group.tab;searchQuery="";mainScroll=editorScroll=null;Repaint();
        }
        public void BackToGroups(){groupDetails=false;RequestClose();}
        void DrawSummaryEditor()
        {
            float w=bodyWidth,chipTop=0,gap=Look.Gap;
            // Search and the changed-only filter serve people with many settings; the prologue shows exactly one.
            if(!ProgressivePrologue)
            {
                float changedWidth=Mathf.Min(Look.ValueMin,w*.45f),queryWidth=Mathf.Max(1,w-changedWidth-gap);
                var query=Input(body,searchQuery,0,0,queryWidth,Look.Chip);query.gameObject.name="edict-search";
                // The mockup's .search: 2 px border, 14 px padding, an 18 px glyph, 8 px, then the field.
                var hint=Text(query.transform,"옵션 이름·설명 검색",42,0,Mathf.Max(1,queryWidth-56),Look.Chip,Look.BodyText,skin.Edge);query.placeholder=hint;hint.gameObject.SetActive(searchQuery.Length==0);
                query.textComponent.alignment=TextAnchor.MiddleLeft;Place(query.textComponent.rectTransform,42,0,Mathf.Max(1,queryWidth-56),Look.Chip);
                Glyph(query.transform,"search",16,(Look.Chip-18)/2,18,18,muted);
                query.onEndEdit.AddListener(value=>{if(value!=searchQuery){searchQuery=value;Repaint();}});
                Button(body,changedOnly?"✓ 변경된 항목":"변경된 항목",w-changedWidth,0,changedWidth,Look.Chip,()=>{changedOnly=!changedOnly;Repaint();},changedOnly?"tab-current":"tab-idle",Look.BodyText+1).name="edict-changed-only";
                chipTop=Look.Chip+gap;
            }
            var groups=HuntEdictUiCatalog.Data.groups.Where(g=>g.tab==SelectedTab&&GroupDisclosed(g)).ToArray();
            if(groups.Length==0){Text(body,"현재 공개된 설정이 없습니다.",0,chipTop+6,w,Look.Button,Look.BodyText,muted);return;}
            // Each tab keeps its own group; a tab nobody opened through SelectTab starts on its first group.
            selectedGroup=groupMemory.TryGetValue(SelectedTab,out var kept)&&groups.Any(g=>HuntEdictSummary.Key(g)==kept)?kept:HuntEdictSummary.Key(groups[0]);
            // The specification fixes the profile's maximum column count; only the selected content scrolls.
            int columns=Mathf.Min(groups.Length,Look.GroupColumns),rows=Mathf.CeilToInt(groups.Length/(float)columns);
            float chipGap=8,cell=Mathf.Max(1,(w-chipGap*(columns-1))/columns),gem=Look.Gem-4;
            bool tight=Look.Landscape&&!Look.Pc;float padX=tight?6:10,padY=tight?4:6;
            // The mockup draws the title through a span the .gtx rule shrinks, so both lines are the small size; the title line box keeps the larger strut.
            float lineSize=Look.SmallText-1,titlePitch=(Look.BodyText+1)*1.2f,subPitch=lineSize*1.3f;
            var subtitles=groups.Select(QuickSubtitle).ToArray();var baseline=Session.Baseline.edict;
            var rowHeights=new float[rows];var blocks=new float[groups.Length,2];
            for(int i=0;i<groups.Length;i++)
            {
                bool on=HuntEdictSummary.Key(groups[i])==selectedGroup;float textWidth=Mathf.Max(1,cell-2*(on?3:2)-2*padX-gem-padX);
                var host=Rect("Measure host",canvasRoot);Place(host,-3000,-3000,2000,400);
                Body(host,groups[i].title,0,0,textWidth,lineSize,titlePitch/lineSize,textColor,out blocks[i,0],true);
                if(subtitles[i]!="")Body(host,subtitles[i],0,0,textWidth,lineSize,1.3f,muted,out blocks[i,1]);
                host.gameObject.SetActive(false);if(Application.isPlaying)Destroy(host.gameObject);else DestroyImmediate(host.gameObject);
                float need=2*(on?3:2)+2*padY+Mathf.Max(gem,blocks[i,0]+blocks[i,1]);rowHeights[i/columns]=Mathf.Max(rowHeights[i/columns],Mathf.Max(Look.GroupChip,need));
            }
            float[] rowTops=new float[rows];float chipsHeight=0;for(int r=0;r<rows;r++){rowTops[r]=chipTop+chipsHeight;chipsHeight+=rowHeights[r]+(r<rows-1?chipGap:0);}
            for(int i=0;i<groups.Length;i++)
            {
                var group=groups[i];string key=HuntEdictSummary.Key(group);bool active=key==selectedGroup;int row=i/columns;float h=rowHeights[row],border=active?3:2;
                bool changed=group.ids.Where(Disclosed).Any(id=>HuntEdictSummary.Changed(Session.Draft.edict,baseline,id));
                var b=Button(body,"",(i%columns)*(cell+chipGap),rowTops[row],cell,h,()=>SelectGroup(key),active?"tab-current":"tab-idle",lineSize);
                b.name="edict-group-"+key;b.GetComponentInChildren<Text>().gameObject.SetActive(false);
                float tx=border+padX+gem+padX,textWidth=Mathf.Max(1,cell-tx-border-padX),top=(h-blocks[i,0]-blocks[i,1])/2;Color ink=active?skin.GuideInk:muted;
                Body(b.transform,group.title,tx,top,textWidth,lineSize,titlePitch/lineSize,ink,out _,true).name="Group title";
                if(subtitles[i]!="")Body(b.transform,subtitles[i],tx,top+blocks[i,0],textWidth,lineSize,1.3f,ink,out _).name="Group subtitle";
                HuntEdictDecor.Create(b.transform,"Group gem",border+padX,(h-gem)/2,gem,gem,"gem",skin);
                float glyph=gem*.62f;GroupGlyph(b.transform,group.icon,border+padX+(gem-glyph)/2,(h-glyph)/2,glyph,active?skin.Navy:skin.SkyInk);
                if(changed){var dot=Panel(b.transform,"Changed dot","changed-dot");dot.GetComponent<Image>().raycastTarget=false;Place(dot,cell-border-17,border+6,9,9);}
                if(!active&&newGroups.Contains(key))NewDot(b.transform,cell-border-(changed?32:32),border+6,10);
            }
            float top2=chipTop+chipsHeight+gap;top2+=DrawBeatStrip(0,top2,w);
            float cp=Look.CardPadding;
            editorScroll=MockScroll(body,"Selected edict group",0,top2,w,Mathf.Max(1,bodyHeight-top2),out var editor);mainScroll=null;
            // The mockup's .sec: a white card that is as tall as its content, laid out as a column with the window gap.
            var sec=Panel(editor,"Selected group card","skill-inspector");
            var secLayout=sec.gameObject.AddComponent<VerticalLayoutGroup>();int pad=Mathf.RoundToInt(2+cp);secLayout.padding=new RectOffset(pad,pad,pad,pad);secLayout.spacing=gap;
            secLayout.childControlWidth=secLayout.childControlHeight=true;secLayout.childForceExpandWidth=true;secLayout.childForceExpandHeight=false;
            float contentWidth=Mathf.Max(1,w-4-2*pad);
            if(searchQuery.Length>0)
            {
                var matches=HuntEdictSummary.Groups(Session.Draft.edict,Session.Baseline.edict,SelectedTab,searchQuery,changedOnly,Disclosed).ToArray();
                if(matches.Length==0)Paragraph(sec,"조건에 맞는 설정이 없습니다.",contentWidth,Look.BodyText,muted);
                foreach(var group in matches)Card(sec,contentWidth,"edict-search-result-"+HuntEdictSummary.Key(group),Loc.T(group.title),group.ids.Where(id=>id!="autoEquip.preserveEffects").All(Disclosed)?group.Description:Loc.T("현재 공개된 설정으로 사냥 규칙을 정하세요."),()=>SelectGroup(HuntEdictSummary.Key(group)),"tab-idle",textColor);
            }
            else
            {
                var selected=groups.Single(g=>HuntEdictSummary.Key(g)==selectedGroup);
                var descRow=Row(sec,"Group description",1);
                var description=Body(descRow,selected.ids.Where(id=>id!="autoEquip.preserveEffects").All(Disclosed)?selected.Description:"현재 공개된 설정으로 사냥 규칙을 정하세요.",0,0,contentWidth,Look.BodyText,1.55f,UiTheme.MockBody,out float descHeight);description.name="Group description text";
                descRow.GetComponent<LayoutElement>().minHeight=descRow.GetComponent<LayoutElement>().preferredHeight=descHeight;
                if(changedOnly&&!selected.ids.Where(Disclosed).Any(id=>HuntEdictSummary.Changed(Session.Draft.edict,Session.Baseline.edict,id)))Paragraph(sec,"조건에 맞는 설정이 없습니다.",contentWidth,Look.SmallText,muted);
                else DrawSelectedGroup(sec,contentWidth,selected);
            }
        }
        void DrawSelectedGroup(RectTransform list,float w,HuntEdictUiGroup group)
        {
            if(HasFeature(HuntEdictProgression.Details)&&group.ids.Where(id=>id!="autoEquip.preserveEffects").All(Disclosed))
            {
                if(group.ids.Contains("bag.warehouseFull")){DrawWarehouseHandling(list,w);return;}
                if(group.tab=="autoEquip"){DrawRecommendedEquipment(list,w,group);return;}
            }
            string scope=HuntEdictQuickPresets.GlobalScope(group);var baseline=Session.Baseline.edict;
            if(QuickAllowed(group)&&!DrawQuickPresetPicker(list,w,scope))return;
            // The mockup's .olist: option rows run edge to edge with no gap between them.
            var olist=Rect("Option list",list);var olayout=olist.gameObject.AddComponent<VerticalLayoutGroup>();olayout.spacing=0;olayout.childControlWidth=olayout.childControlHeight=true;olayout.childForceExpandWidth=true;olayout.childForceExpandHeight=false;
            foreach(string id in group.ids.Where(Disclosed))
            {
                if(id=="survival.potion")GroupSection(olist,"사용 조건",w);
                if(id=="potion.autoBuy")GroupSection(olist,"자동 구매",w);
                var d=EdictOptions.Global.Single(o=>o.id==id);bool available=HuntEdictUiCatalog.Visible(Session.Draft.edict,id)&&DirectDisclosed(id);
                DrawCompactOption(olist,w,d,HuntEdictSummary.Value(Session.Draft.edict,id),available,HuntEdictSummary.Changed(Session.Draft.edict,baseline,id));
                if(!available)
                {
                    string note;var rule=HuntEdictUiCatalog.Data.conditions.SingleOrDefault(c=>c.id==id);
                    if(rule==null)note=Loc.T("세부 설정은 균열 6단계부터 공개됩니다.");
                    else{var parent=EdictOptions.Global.Single(o=>o.id==rule.parent);note=Loc.F("{0}: {1} 선택 시 적용",HuntEdictUiCatalog.Label(parent),HuntEdictSummary.Display(parent,rule.value,SkillName));}
                    var cond=Row(olist,"Option condition",1);var line=Body(cond,note,14,4,w-28,Look.SmallText,1.5f,muted,out float noteHeight);line.name="Option condition text";
                    cond.GetComponent<LayoutElement>().minHeight=cond.GetComponent<LayoutElement>().preferredHeight=noteHeight+12;
                }
            }
        }
        // The mockup's .orow: 14 px lead, the label (and its "?"), the value pill at the right edge, a dashed rule under; a changed row gets a gold bar.
        void DrawCompactOption(RectTransform list,float w,EdictOptionDefinition d,string value,bool available,bool changed)
        {
            string title=HuntEdictUiCatalog.Label(d),help=HuntEdictUiCatalog.Help(d.id);float fs=Look.BodyText,pillFont=fs+1,minPill=Look.ValueMin;
            var row=Row(list,"Option "+d.id,1,"option-card");
            var pill=Button(row,HuntEdictSummary.Display(d,value,SkillName),0,0,minPill,Look.Button-14,()=>EditGlobal(d,value),available?(changed?"changed-value":"input-well"):"disabled-value",pillFont);pill.name="edict-option-"+d.id;pill.interactable=available;
            SurfaceChoice(pill,changed&&available,"changed-value",available?"input-well":"disabled-value");
            var shown=pill.GetComponentInChildren<Text>();shown.alignment=TextAnchor.MiddleLeft;shown.horizontalOverflow=HorizontalWrapMode.Overflow;
            float maxPill=Mathf.Max(1,w*.58f),pillWidth=Mathf.Min(maxPill,Mathf.Max(minPill,shown.preferredWidth+54));
            shown.horizontalOverflow=HorizontalWrapMode.Wrap;Place(shown.rectTransform,16,0,Mathf.Max(1,pillWidth-16-38),1000);
            float pillHeight=Mathf.Max(Look.Button-14,shown.preferredHeight+12);
            float helpWidth=help!=""?28+10:0,labelWidth=Mathf.Max(1,w-14-2-pillWidth-10-helpWidth);
            var caption=Body(row,title,14,0,labelWidth,fs,1.4f,available?textColor:skin.Edge,out float labelHeight);caption.name="Option label";
            float content=Mathf.Max(labelHeight,pillHeight),h=Mathf.Max(Look.Button-4,12+content+2),top=(h-2-content)/2;
            Place(caption.rectTransform,14,top+(content-labelHeight)/2+(fs*1.4f-NaturalLine(caption.font,caption.fontSize))/2,labelWidth,caption.preferredHeight);
            Place((RectTransform)pill.transform,w-2-pillWidth,top+(content-pillHeight)/2,pillWidth,pillHeight);Place(shown.rectTransform,16,0,Mathf.Max(1,pillWidth-16-38),pillHeight);
            HuntEdictDecor.Create(pill.transform,"Value chevron",pillWidth-2-10-18,(pillHeight-18)/2,18,18,"chevron",skin);
            if(help!="")Button(row,"?",14+labelWidth+10,top+(content-28)/2,28,28,()=>ShowOptionHelp(title,help),"help-round",15).name="edict-help-"+d.id;
            if(changed){var mark=Panel(row,"Changed mark","gold-bar");mark.GetComponent<Image>().raycastTarget=false;Place(mark,0,9,5,Mathf.Max(0,h-18));}
            var layout=row.GetComponent<LayoutElement>();layout.minHeight=layout.preferredHeight=h;
        }
        void EditGlobal(EdictOptionDefinition d,string value)
        {
            if(!DirectDisclosed(d.id))return;
            string title=HuntEdictUiCatalog.Label(d);void Set(string v)=>SetGlobal(d.id,v);
            if(d.kind==EdictOptionKind.Set){SetDialog(d,value,Set);return;}
            if(d.kind==EdictOptionKind.Order){OrderDialog(d,value,Set);return;}
            if(d.kind==EdictOptionKind.Number){NumericDialog(d,value,Set);return;}
            var choices=d.kind==EdictOptionKind.Toggle?new[]{"ON","OFF"}:d.kind==EdictOptionKind.SkillReference?new[]{""}.Concat(Session.Draft.edict.slots.Where(id=>id!=""&&(d.choices.Length==0||d.choices.Contains(id)))).ToArray():d.choices;
            // Standing still is what the lesson is about leaving behind.
            if(PitLesson&&d.id=="position.mode")choices=choices.Where(c=>c!="STAND").ToArray();
            ChooseValue(title,choices,value,v=>HuntEdictSummary.Display(d,v,SkillName),Set);
        }
        void ShowOptionHelp(string title,string help)
        {
            float top=Look.Close+Look.Gap,h=Mathf.Min(height-16,400);
            Dialog(title,p=>{top=DialogTop;Scroll(p,"Option help",12,top,DialogWidth-24,Mathf.Max(1,h-top-12),out var list);Paragraph(list,help,DialogWidth-38,Look.BodyText,textColor);EndList(list,Look.Gap);},h);
        }
        void SetDialog(EdictOptionDefinition d,string value,Action<string> set)
        {
            var selected=value.Split(',').Where(v=>v!="").ToHashSet();float h=Mathf.Min(height-16,420),top=Look.Close+Look.Gap;
            Dialog(HuntEdictUiCatalog.Label(d),p=>
            {
                top=DialogTop;
                Scroll(p,"Multiple choices",12,top,DialogWidth-24,Mathf.Max(1,h-top-Look.Button-Look.Gap*2),out var list);
                foreach(string id in d.choices)
                {
                    string choice=id;float w=DialogWidth-40;var row=Row(list,"Choice "+id,Look.Button);Button b=null;RectTransform check=null;
                    b=Button(row,Loc.T(EdictOptionLabels.Item(d,id)),0,0,w,Look.Button,()=>
                    {
                        if(!selected.Remove(choice))selected.Add(choice);
                        SurfaceChoice(b,selected.Contains(choice));check.gameObject.SetActive(selected.Contains(choice));
                    },"button-idle",Look.BodyText+1);
                    var label=b.GetComponentInChildren<Text>();label.alignment=TextAnchor.MiddleLeft;Place(label.rectTransform,14,0,w-54,1000);
                    float rowHeight=Mathf.Max(Look.Button,label.preferredHeight+12);row.GetComponent<LayoutElement>().minHeight=row.GetComponent<LayoutElement>().preferredHeight=rowHeight+2;
                    Place((RectTransform)b.transform,0,0,w,rowHeight);Place(label.rectTransform,14,0,w-54,rowHeight);SurfaceChoice(b,selected.Contains(id));
                    check=HuntEdictDecor.Create(b.transform,"Choice check",w-40,(rowHeight-26)/2,26,26,"check",skin);check.gameObject.SetActive(selected.Contains(id));
                }
                EndList(list,Look.Gap);Button(p,"적용",DialogWidth-12-Look.ActionMin,h-Look.Button-Look.Gap,Look.ActionMin,Look.Button,()=>{DismissDialog();set(string.Join(",",d.choices.Where(selected.Contains)));},"button-primary",Look.BodyText+3).name="edict-choice-apply";
            },h);
        }
        void OrderDialog(EdictOptionDefinition d,string value,Action<string> set)
        {
            string[] order=value.Split(',');float h=Mathf.Min(height-16,420),top=Look.Close+Look.Gap;
            Dialog(HuntEdictUiCatalog.Label(d),p=>
            {
                top=DialogTop;
                var scroll=Scroll(p,"Priority choices",12,top,DialogWidth-24,Mathf.Max(1,h-top-Look.Button-Look.Gap*2),out var list);
                DrawOrder(list,DialogWidth-40,order,id=>Loc.T(EdictOptionLabels.Item(d,id)),(id,target)=>OrderDialog(d,string.Join(",",HuntEdictReorder.MoveVisible(order,order,id,target)),set),scroll);
                EndList(list,Look.Gap);Button(p,"적용",DialogWidth-12-Look.ActionMin,h-Look.Button-Look.Gap,Look.ActionMin,Look.Button,()=>{DismissDialog();set(string.Join(",",order));},"button-primary",Look.BodyText+3).name="edict-choice-apply";
            },h);
        }
        // Percent options also offer a few common values. A chip only fills the field; Apply stays the single way to commit.
        static int[] QuickValues(EdictOptionDefinition d)
        {
            if(!d.id.ToLowerInvariant().Contains("percent"))return Array.Empty<int>();
            var values=Enumerable.Range(1,9).Select(i=>i*10).Where(v=>v>=d.min&&v<=d.max&&Mathf.Abs((v-d.min)/d.step-Mathf.Round((v-d.min)/d.step))<1e-4f).ToArray();
            return values.Length<=6?values:values.Where((v,i)=>i%2==0).ToArray();
        }
        void NumericDialog(EdictOptionDefinition d,string value,Action<string> set)
        {
            var quick=QuickValues(d);float top=Look.Close+Look.Gap,fieldHeight=Look.Button,gap=Look.Gap,rangeHeight=Look.SmallText+12;
            float chips=quick.Length>0?36+gap:0,errorY=top+fieldHeight+gap+rangeHeight+chips,errorHeight=Look.SmallText*2+8,applyY=errorY+errorHeight+gap;
            Dialog(HuntEdictUiCatalog.Label(d),p=>
            {
                float shift=DialogTop-top;top=DialogTop;errorY+=shift;applyY+=shift;
                float side=Look.Close,inputX=12+side+gap;
                var input=Input(p,value,inputX,top,Mathf.Max(1,DialogWidth-2*inputX),fieldHeight);input.contentType=InputField.ContentType.DecimalNumber;input.gameObject.name="edict-number-input";
                input.textComponent.fontSize=Mathf.RoundToInt(Look.CardTitle+4);input.textComponent.alignment=TextAnchor.MiddleCenter;skin.ApplyLabel(input.textComponent,true,textColor);
                var error=Text(p,"",12,errorY,DialogWidth-24,errorHeight,Look.SmallText,UiTheme.Danger);
                void Step(int direction){try{input.text=HuntEdictV2Editing.Stepped(d,d.CanonicalValue(input.text,Session.Draft.edict.heroClass),direction);error.text="";}catch(Exception ex){error.text=ex.Message;}}
                Button(p,"−",12,top+(fieldHeight-side)/2,side,side,()=>Step(-1),"help-round",Look.CardTitle);Button(p,"+",DialogWidth-12-side,top+(fieldHeight-side)/2,side,side,()=>Step(1),"help-round",Look.CardTitle);
                Text(p,Loc.F("범위 {0}~{1} · 단위 {2}",d.min,d.max,d.step),12,top+fieldHeight+gap,DialogWidth-24,rangeHeight,Look.SmallText,muted);
                float chipWidth=(DialogWidth-24-4*(quick.Length-1))/Mathf.Max(1,quick.Length);
                for(int i=0;i<quick.Length;i++)
                {int shown=quick[i];Button(p,shown+"%",12+i*(chipWidth+4),top+fieldHeight+gap+rangeHeight,chipWidth,36,()=>{input.text=shown.ToString();error.text="";},"chip-off",Look.BodyText+1).name="Quick value "+shown;}
                Button(p,"적용",DialogWidth-12-Look.ActionMin,applyY,Look.ActionMin,Look.Button,()=>
                {try{string canonical=d.CanonicalValue(input.text,Session.Draft.edict.heroClass);DismissDialog();set(canonical);}catch(Exception ex){error.text=ex.Message;}},"button-primary",Look.BodyText+3).name="edict-number-apply";
            },applyY+Look.Button+gap);
        }
    }
}
