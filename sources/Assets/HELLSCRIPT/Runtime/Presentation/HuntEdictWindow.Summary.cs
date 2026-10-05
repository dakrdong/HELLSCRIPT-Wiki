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
            if(HuntEdictProgression.Has(store.Data,HuntEdictProgression.Details))customQuickScopes.Add(scope);SelectGroup(HuntEdictSummary.Key(group));
            return EnsureVisible("edict-option-"+id)||EnsureVisible("edict-quick-picker-"+scope);
        }
        public void SelectGroup(string key)
        {
            if(DefaultSettingsLocked)return;
            var group=HuntEdictUiCatalog.Data.groups.FirstOrDefault(g=>HuntEdictSummary.Key(g)==key);if(group==null||!GroupDisclosed(group))return;
            if(searchQuery.Length>0&&HuntEdictProgression.Has(store.Data,HuntEdictProgression.Details))customQuickScopes.Add(HuntEdictQuickPresets.GlobalScope(group));
            RememberScroll();addingEquipmentOption=false;selectedGroup=key;groupMemory[group.tab]=key;openedGroups.Add(OpenedKey(key));afterSave=null;groupDetails=true;SelectedTab=group.tab;searchQuery="";mainScroll=editorScroll=null;Repaint();
        }
        public void BackToGroups(){groupDetails=false;RequestClose();}
        void DrawSummaryEditor()
        {
            float w=bodyWidth-16,chipTop=6;
            // Search and the changed-only filter serve people with many settings; the prologue shows exactly one.
            if(!ProgressivePrologue)
            {
                var query=Input(body,searchQuery,8,6,w-126,30);query.gameObject.name="edict-search";
                var hint=Text(query.transform,"옵션 이름·설명 검색",7,0,w-142,30,11,muted);query.placeholder=hint;hint.gameObject.SetActive(searchQuery.Length==0);
                query.onEndEdit.AddListener(value=>{if(value!=searchQuery){searchQuery=value;Repaint();}});
                Button(body,changedOnly?"✓ 변경된 항목":"변경된 항목",bodyWidth-126,6,118,30,()=>{changedOnly=!changedOnly;Repaint();},changedOnly?"tab-current":"tab-idle",10).name="edict-changed-only";
                chipTop=42;
            }
            var groups=HuntEdictUiCatalog.Data.groups.Where(g=>g.tab==SelectedTab&&GroupDisclosed(g)).ToArray();
            if(groups.Length==0){Text(body,"현재 공개된 설정이 없습니다.",12,chipTop+6,w,30,12,muted);return;}
            // Each tab keeps its own group; a tab nobody opened through SelectTab starts on its first group.
            selectedGroup=groupMemory.TryGetValue(SelectedTab,out var kept)&&groups.Any(g=>HuntEdictSummary.Key(g)==kept)?kept:HuntEdictSummary.Key(groups[0]);
            // Tabs remain fixed and wrap as a whole row. Only the selected content scrolls.
            int columns=Mathf.Clamp(Mathf.FloorToInt(w/(116*Mathf.Clamp(lastScale,1,1.5f))),1,groups.Length);
            int rows=Mathf.CeilToInt(groups.Length/(float)columns);float cell=(w-4*(columns-1))/columns;
            // Each chip shows the group, its current answer (the matching preset) and a dot while it differs from the saved edict.
            float tabHeight=44;var subtitles=groups.Select(QuickSubtitle).ToArray();var baseline=Session.Baseline.edict;
            for(int i=0;i<groups.Length;i++)
            {
                var measure=Text(body,groups[i].title,0,0,cell-34,1000,11);float need=measure.preferredHeight+2;measure.gameObject.SetActive(false);Destroy(measure.gameObject);
                if(subtitles[i]!=""){var line=Text(body,subtitles[i],0,0,cell-34,1000,9);need+=line.preferredHeight+2;line.gameObject.SetActive(false);Destroy(line.gameObject);}
                tabHeight=Mathf.Max(tabHeight,need+10);
            }
            for(int i=0;i<groups.Length;i++)
            {
                var group=groups[i];string key=HuntEdictSummary.Key(group);bool active=key==selectedGroup;
                bool changed=group.ids.Where(Disclosed).Any(id=>HuntEdictSummary.Changed(Session.Draft.edict,baseline,id));
                var b=Button(body,group.title,8+(i%columns)*(cell+4),chipTop+(i/columns)*(tabHeight+4),cell,tabHeight,()=>SelectGroup(key),active?"tab-current":"tab-idle",11);
                b.name="edict-group-"+key;var label=b.GetComponentInChildren<Text>();Place(label.rectTransform,29,0,cell-34,1000);label.alignment=TextAnchor.UpperLeft;
                float titleHeight=label.preferredHeight+2,subHeight=0;Text sub=null;
                if(subtitles[i]!=""){sub=Text(b.transform,subtitles[i],29,0,cell-34,1000,9,active?UiTheme.Tint(UiTheme.GoldDeepHex):UiTheme.Faint);subHeight=sub.preferredHeight+2;}
                float inset=(tabHeight-titleHeight-subHeight)/2;Place(label.rectTransform,29,inset,cell-34,titleHeight);
                if(sub!=null)Place(sub.rectTransform,29,inset+titleHeight,cell-34,subHeight);
                GroupGlyph(b.transform,group.icon,5,(tabHeight-19)/2,19,active?gold:muted);
                if(changed)Text(b.transform,"•",cell-9,0,8,14,10,gold);
                if(!active&&newGroups.Contains(key))NewDot(b.transform,cell-(changed?19:12),5,7);
            }
            float top=chipTop+rows*(tabHeight+4)+4;top+=DrawBeatStrip(8,top,w);
            editorScroll=Scroll(body,"Selected edict group",8,top,w,bodyHeight-top-6,out var editor);mainScroll=null;
            if(searchQuery.Length>0)
            {
                var matches=HuntEdictSummary.Groups(Session.Draft.edict,Session.Baseline.edict,SelectedTab,searchQuery,changedOnly,Disclosed).ToArray();
                if(matches.Length==0)Paragraph(editor,"조건에 맞는 설정이 없습니다.",w-14,12,muted);
                foreach(var group in matches)Card(editor,w-14,"edict-search-result-"+HuntEdictSummary.Key(group),Loc.T(group.title),group.ids.Where(id=>id!="autoEquip.preserveEffects").All(Disclosed)?group.Description:Loc.T("현재 공개된 설정으로 사냥 규칙을 정하세요."),()=>SelectGroup(HuntEdictSummary.Key(group)),"tab-idle",textColor);
            }
            else
            {
                var selected=groups.Single(g=>HuntEdictSummary.Key(g)==selectedGroup);
                Paragraph(editor,selected.ids.Where(id=>id!="autoEquip.preserveEffects").All(Disclosed)?selected.Description:"현재 공개된 설정으로 사냥 규칙을 정하세요.",w-14,12,muted);
                if(changedOnly&&!selected.ids.Where(Disclosed).Any(id=>HuntEdictSummary.Changed(Session.Draft.edict,Session.Baseline.edict,id)))Paragraph(editor,"조건에 맞는 설정이 없습니다.",w-14,12,muted);
                else DrawSelectedGroup(editor,w-14,selected);
            }
            EndList(editor,6);
        }
        void DrawSelectedGroup(RectTransform list,float w,HuntEdictUiGroup group)
        {
            if(HuntEdictProgression.Has(store.Data,HuntEdictProgression.Details)&&group.ids.Where(id=>id!="autoEquip.preserveEffects").All(Disclosed))
            {
                if(group.ids.Contains("bag.warehouseFull")){DrawWarehouseHandling(list,w);return;}
                if(group.tab=="autoEquip"){DrawRecommendedEquipment(list,w,group);return;}
            }
            string scope=HuntEdictQuickPresets.GlobalScope(group);var baseline=Session.Baseline.edict;
            if(QuickAllowed(group)&&!DrawQuickPresetPicker(list,w,scope))return;
            foreach(string id in group.ids.Where(Disclosed))
            {
                if(id=="survival.potion")GroupSection(list,"사용 조건",w);
                if(id=="potion.autoBuy")GroupSection(list,"자동 구매",w);
                var d=EdictOptions.Global.Single(o=>o.id==id);bool available=HuntEdictUiCatalog.Visible(Session.Draft.edict,id)&&HuntEdictProgression.Direct(store.Data,hero,id);
                DrawCompactOption(list,w,d,HuntEdictSummary.Value(Session.Draft.edict,id),available,HuntEdictSummary.Changed(Session.Draft.edict,baseline,id));
                if(!available)
                {
                    var rule=HuntEdictUiCatalog.Data.conditions.SingleOrDefault(c=>c.id==id);if(rule==null){Paragraph(list,"세부 설정은 균열 6단계부터 공개됩니다.",w,10,muted);continue;}var parent=EdictOptions.Global.Single(o=>o.id==rule.parent);
                    Paragraph(list,Loc.F("{0}: {1} 선택 시 적용",HuntEdictUiCatalog.Label(parent),HuntEdictSummary.Display(parent,rule.value,SkillName)),w,10,muted);
                }
            }
        }
        // One line per option: the label (and its "?") on the left, the value pill on the right. A changed row gets a gold bar.
        void DrawCompactOption(RectTransform list,float w,EdictOptionDefinition d,string value,bool available,bool changed)
        {
            string title=HuntEdictUiCatalog.Label(d),help=HuntEdictUiCatalog.Help(d.id);var row=Row(list,"Option "+d.id,36,"option-card");
            var pill=Button(row,HuntEdictSummary.Display(d,value,SkillName)+"  ›",0,0,128,32,()=>EditGlobal(d,value),"input-well",11);pill.name="edict-option-"+d.id;pill.interactable=available;
            var shown=pill.GetComponentInChildren<Text>();shown.alignment=TextAnchor.MiddleLeft;
            // The pill takes what its text needs, between 128 and 58% of the row; a long value wraps inside it.
            float pillWidth=Mathf.Clamp(shown.preferredWidth+26,128,w*.58f);Place(shown.rectTransform,8,0,pillWidth-14,1000);float pillHeight=Mathf.Max(32,shown.preferredHeight+10);
            float lead=changed?8:0,helpWidth=help!=""?30:0,labelWidth=w-pillWidth-8-helpWidth-lead;
            var caption=Text(row,title,lead,0,labelWidth,1000,12,available?textColor:muted);
            float h=Mathf.Max(Mathf.Max(36,34*lastScale),caption.preferredHeight+12,pillHeight+6);
            Place(caption.rectTransform,lead,0,labelWidth,h);Place((RectTransform)pill.transform,w-pillWidth,(h-pillHeight)/2,pillWidth,pillHeight);Place(shown.rectTransform,8,0,pillWidth-14,pillHeight);
            if(help!="")Button(row,"?",lead+labelWidth+4,(h-24)/2,24,24,()=>ShowOptionHelp(title,help),"chip-off",11).name="edict-help-"+d.id;
            if(changed){var mark=Panel(row,"Changed mark","gold-bar");mark.GetComponent<Image>().raycastTarget=false;Place(mark,0,6,3,h-12);}
            var layout=row.GetComponent<LayoutElement>();layout.minHeight=layout.preferredHeight=h;
        }
        void EditGlobal(EdictOptionDefinition d,string value)
        {
            if(!HuntEdictProgression.Direct(store.Data,hero,d.id))return;
            string title=HuntEdictUiCatalog.Label(d);void Set(string v)=>SetGlobal(d.id,v);
            if(d.kind==EdictOptionKind.Set){SetDialog(d,value,Set);return;}
            if(d.kind==EdictOptionKind.Order){OrderDialog(d,value,Set);return;}
            if(d.kind==EdictOptionKind.Number){NumericDialog(d,value,Set);return;}
            var choices=d.kind==EdictOptionKind.Toggle?new[]{"ON","OFF"}:d.kind==EdictOptionKind.SkillReference?new[]{""}.Concat(Session.Draft.edict.slots.Where(id=>id!=""&&(d.choices.Length==0||d.choices.Contains(id)))).ToArray():d.choices;
            ChooseValue(title,choices,value,v=>HuntEdictSummary.Display(d,v,SkillName),Set);
        }
        void ShowOptionHelp(string title,string help)=>Dialog(title,p=>
        {var scroll=Scroll(p,"Option help",12,50,DialogWidth-24,Mathf.Min(height-80,330),out var list);Paragraph(list,help,DialogWidth-38,12,textColor);EndList(list,8);},Mathf.Min(height-16,400));
        void SetDialog(EdictOptionDefinition d,string value,Action<string> set)
        {
            var selected=value.Split(',').Where(v=>v!="").ToHashSet();float h=Mathf.Min(height-16,420);
            Dialog(HuntEdictUiCatalog.Label(d),p=>
            {
                Scroll(p,"Multiple choices",12,50,DialogWidth-24,h-108,out var list);
                foreach(string id in d.choices)
                {
                    string choice=id;var row=Row(list,"Choice "+id,38);Button b=null;
                    b=Button(row,(selected.Contains(id)?"✓ ":"   ")+Loc.T(EdictOptionLabels.Item(d,id)),0,0,DialogWidth-40,36,()=>
                    {if(!selected.Remove(choice))selected.Add(choice);b.GetComponent<HuntEdictSurface>().Paint(selected.Contains(choice)?"chip-on":"tab-idle");b.GetComponentInChildren<Text>().text=(selected.Contains(choice)?"✓ ":"   ")+Loc.T(EdictOptionLabels.Item(d,choice));},selected.Contains(id)?"chip-on":"tab-idle",12);
                }
                EndList(list,4);Button(p,"적용",DialogWidth-104,h-48,92,36,()=>{DismissDialog();set(string.Join(",",d.choices.Where(selected.Contains)));},"button-primary").name="edict-choice-apply";
            },h);
        }
        void OrderDialog(EdictOptionDefinition d,string value,Action<string> set)
        {
            string[] order=value.Split(',');float h=Mathf.Min(height-16,420);
            Dialog(HuntEdictUiCatalog.Label(d),p=>
            {
                var scroll=Scroll(p,"Priority choices",12,50,DialogWidth-24,h-108,out var list);
                DrawOrder(list,DialogWidth-40,order,id=>Loc.T(EdictOptionLabels.Item(d,id)),(id,target)=>OrderDialog(d,string.Join(",",HuntEdictReorder.MoveVisible(order,order,id,target)),set),scroll);
                EndList(list,4);Button(p,"적용",DialogWidth-104,h-48,92,36,()=>{DismissDialog();set(string.Join(",",order));},"button-primary").name="edict-choice-apply";
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
            var quick=QuickValues(d);float chips=quick.Length>0?34:0;
            Dialog(HuntEdictUiCatalog.Label(d),p=>
            {
                var input=Input(p,value,58,53,DialogWidth-116,38);input.contentType=InputField.ContentType.DecimalNumber;input.gameObject.name="edict-number-input";
                var error=Text(p,"",12,125+chips,DialogWidth-24,42,11,UiTheme.Danger);
                void Step(int direction){try{input.text=HuntEdictV2Editing.Stepped(d,d.CanonicalValue(input.text,Session.Draft.edict.heroClass),direction);error.text="";}catch(Exception ex){error.text=ex.Message;}}
                Button(p,"−",12,53,38,38,()=>Step(-1));Button(p,"+",DialogWidth-50,53,38,38,()=>Step(1));
                Text(p,Loc.F("범위 {0}~{1} · 단위 {2}",d.min,d.max,d.step),12,98,DialogWidth-24,25,11,muted);
                float chipWidth=(DialogWidth-24-4*(quick.Length-1))/Mathf.Max(1,quick.Length);
                for(int i=0;i<quick.Length;i++)
                {int shown=quick[i];Button(p,shown+"%",12+i*(chipWidth+4),126,chipWidth,28,()=>{input.text=shown.ToString();error.text="";},"chip-off",11).name="Quick value "+shown;}
                Button(p,"적용",DialogWidth-104,178+chips,92,36,()=>
                {try{string canonical=d.CanonicalValue(input.text,Session.Draft.edict.heroClass);DismissDialog();set(canonical);}catch(Exception ex){error.text=ex.Message;}},"button-primary").name="edict-number-apply";
            },230+chips);
        }
    }
}
