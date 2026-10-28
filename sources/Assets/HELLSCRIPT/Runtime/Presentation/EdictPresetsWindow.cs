using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    // The existing preset/share page, hosted by the shared casual popup shell.
    public sealed partial class EdictPresetsWindow
    {
        GameStore store;GameCatalog catalog;HeroSave hero;
        CombatSimulation simulation;
        Func<HuntEdictEditSession,bool> commit;Func<string> saveError;Func<float> readingScale;
        readonly UiSkin skin=UiSkin.Casual;
        bool landscape=>UiSafeArea.Current.width>UiSafeArea.Current.height;
        EdictPresetsLayout Look=>new EdictPresetsLayout(landscape&&UiSafeArea.Current.width>=1100&&UiSafeArea.Current.height>=600,landscape);
        Color textColor=>skin.Navy;Color muted=>skin.Muted;Color gold=>skin.Navy;
        bool persistentPresets,quickPage;float width,height,bodyWidth,bodyHeight,lastScale=1;
        RectTransform window,body,modal;ScrollRect mainScroll;Action restoreDialog;
        readonly System.Collections.Generic.List<string> dialogValues=new System.Collections.Generic.List<string>();
        float presetOffset,quickOffset;
        public ContentWindowView View {get;private set;}
        public HuntEdictEditSession Session {get;private set;}
        public bool HasDialog=>modal!=null;
        bool HasFeature(string id)=>HuntEdictProgression.Has(store.Data,id);
        bool QuickDisclosed(string scope,string id)=>HuntEdictProgression.Quick(store.Data,scope,id,hero);
        public static bool Available(AccountSave account)=>HuntEdictProgression.Has(account,HuntEdictProgression.Presets)||HuntEdictProgression.Has(account,HuntEdictProgression.Styles)||HuntEdictProgression.Has(account,HuntEdictProgression.Dodge);
        public static EdictPresetsWindow Open(Transform parent,GameStore store,GameCatalog catalog,HeroSave hero,
            Func<float> reading,Func<HuntEdictEditSession,bool> commit,Func<string> error,Action closed,bool persistentPresets=true,CombatSimulation simulation=null)
        {
            var page=new EdictPresetsWindow{store=store,catalog=catalog,hero=hero,commit=commit,saveError=error,readingScale=reading,persistentPresets=persistentPresets,simulation=simulation,Session=new HuntEdictEditSession(hero)};
            page.quickPage=!page.HasFeature(HuntEdictProgression.Presets);
            page.Session.UseSkillTree(hero);
            page.View=ContentWindowView.Open(parent,"프리셋·공유",EquipmentViewSource.Draft,reading,page.Render,closed,
                maximumSize:new Vector2(860,680),back:page.HandleBack,skin:page.skin);
            StoreViewBinding.Attach(page.View,store,page.Repaint,()=>!page.HasDialog&&page.renamingPreset<0,
                ()=>string.Join("|",hero.id,hero.useRecommendedEdict,StoreViewBinding.Key(hero.build),StoreViewBinding.Key(hero.edict),StoreViewBinding.Key(hero.edictPresets),string.Join(",",store.Data.guide.edictUnlocks)));
            return page;
        }
        void HandleBack(){if(HasDialog)DismissDialog();else RequestClose();}
        public void RequestClose()=>RequestLeave(()=>View.Close());
        public void Repaint(){DismissDialog();View.Repaint();}
        public void ShowQuickPresets(){quickPage=true;Repaint();}
        void Render(ContentWindowView view)
        {
            var reopen=restoreDialog;var inputs=dialogValues.ToArray();
            modal=null;mainScroll=null;View=view;window=view.Frame;window.name="edict-presets-window";width=view.Width;height=window.rect.height;lastScale=readingScale?.Invoke()??1;
            var closer=view.Frame.Find("content-close").GetComponent<Button>();closer.onClick.RemoveAllListeners();closer.onClick.AddListener(RequestClose);
            float w=width-24,tab=(w-8)/2;
            float switchHeight=DrawEdictSwitch(view.Navigation,w);
            view.Navigation.sizeDelta+=Vector2.up*switchHeight;
            view.Scroll.viewport.anchoredPosition+=Vector2.down*switchHeight;view.Scroll.viewport.sizeDelta-=Vector2.up*switchHeight;
            var slots=view.Tab(view.Navigation,"프리셋·공유",0,switchHeight,tab,36,()=>{quickPage=false;Repaint();},!quickPage);slots.name="edict-presets-tab-slots";slots.interactable=HasFeature(HuntEdictProgression.Presets);
            view.Tab(view.Navigation,"간편 프리셋",tab+8,switchHeight,tab,36,ShowQuickPresets,quickPage).name="edict-presets-tab-quick";
            // The reused page owns an inner list; keep it inside the shared viewport without a competing layout group.
            var layout=view.Body.GetComponent<VerticalLayoutGroup>();layout.enabled=false;view.Body.GetComponent<ContentSizeFitter>().enabled=false;
            view.Scroll.vertical=false;UiLayout.Stretch(view.Body);body=view.Body;bodyWidth=w;bodyHeight=view.Scroll.viewport.rect.height;
            if(quickPage)DrawQuickPresets();else DrawPresets();
            if(mainScroll!=null)
            {
                var scroll=mainScroll;bool quick=quickPage;Canvas.ForceUpdateCanvases();
                scroll.content.anchoredPosition=new Vector2(0,Mathf.Clamp(quick?quickOffset:presetOffset,0,Mathf.Max(0,scroll.content.rect.height-scroll.viewport.rect.height)));
                scroll.onValueChanged.AddListener(_=>{if(quick)quickOffset=scroll.content.anchoredPosition.y;else presetOffset=scroll.content.anchoredPosition.y;});
            }
            float action=(w-8)/2;
            var revert=view.Button(view.Actions,"되돌리기",0,0,action,44,()=>{Session.Revert();Repaint();});revert.name="edict-presets-revert";revert.interactable=Session.Dirty;
            var save=view.Button(view.Actions,"저장",action+8,0,action,44,()=>Save(),true);save.name="edict-presets-save";save.interactable=Session.Dirty;
            if(reopen!=null){reopen();var fields=modal.GetComponentsInChildren<InputField>();for(int i=0;i<Math.Min(fields.Length,inputs.Length);i++){fields[i].SetTextWithoutNotify(inputs[i]);dialogValues[i]=inputs[i];}}
        }
        public bool Save(){if(!commit(Session)){Error(saveError());return false;}Repaint();return true;}
        void RequestLeave(Action next)
        {
            if(!Session.Dirty){next();return;}
            Dialog("변경된 내용이 있는데 저장하시겠어요?",p=>
            {
                Button(p,"이전 상태로 되돌리기",12,DialogTop,DialogWidth-24,Look.Button,()=>{Session.Revert();DismissDialog();next();});
                Button(p,"저장하고 나가기",12,DialogTop+Look.Button+Look.Gap,DialogWidth-24,Look.Button,()=>{if(Save()){DismissDialog();next();}},"button-primary");
            },2*Look.Button+4*Look.Gap+Look.Close);
        }
        public string QuickPresetSelection(string scope)=>HuntEdictSkillPresets.Owns(scope)?HuntEdictSkillPresets.Active(Session.Draft,scope):HuntEdictQuickPresets.Match(Session.Draft,scope);
        public void SelectQuickPreset(string scope,string id)
        {
            if(hero.useRecommendedEdict||id==HuntEdictQuickPresets.Custom)return;
            if(scope=="style"?!HasFeature(HuntEdictProgression.Styles):!QuickDisclosed(scope,id))return;
            try
            {
                if(scope=="style")Session.ApplyStyle(id,!HuntEdictProgression.Legacy(store.Data));
                else if(HuntEdictSkillPresets.Owns(scope))Session.ReplaceDraft(HuntEdictSkillPresets.Select(Session.Draft,scope,id));
                else Session.ApplyQuickPreset(scope,id);
                Repaint();
            }
            catch(Exception e){Error(e.Message);}
        }
        void DrawQuickPresets()
        {
            mainScroll=Scroll(body,"Quick presets",0,0,bodyWidth,bodyHeight,out var list);
            float w=bodyWidth-28;
            if(HasFeature(HuntEdictProgression.Styles))
            {
                Paragraph(list,"전투 방식",w,Look.CardTitle);
                foreach(var style in HuntEdictQuickPresets.Styles)
                    QuickPresetChoice(list,w,"style",style.id,style.Name,style.Description,HuntEdictQuickPresets.MatchStyle(Session.Draft)==style.id);
            }
            foreach(var group in HuntEdictUiCatalog.Data.groups.Where(g=>g.tab!="autoEquip"))
            {
                string scope=HuntEdictQuickPresets.GlobalScope(group);
                if(!HuntEdictQuickPresets.For(scope).Any(p=>QuickDisclosed(scope,p.id)))continue;
                Paragraph(list,group.title,w,Look.CardTitle);DrawQuickPresetPicker(list,w,scope);
            }
            if(Session.Draft.UsesTree)
            {
                Picker("기본 공격",HuntEdictQuickPresets.SkillScope("BASIC",Session.Draft.edict.heroClass));
                Picker("공통 공격 순서",HuntEdictQuickPresets.AttackOrder);
                foreach(string skill in Session.Draft.classSkills.actives.Concat(new[]{Session.Draft.classSkills.ultimate}).Where(id=>id!=""))
                    Picker(ClassSkillTree.Name(skill),HuntEdictQuickPresets.SkillScope(skill,Session.Draft.edict.heroClass));
            }
            void Picker(string title,string scope)
            {
                if(!HuntEdictQuickPresets.For(scope).Any(p=>QuickDisclosed(scope,p.id)))return;
                Paragraph(list,title,w,Look.CardTitle);DrawQuickPresetPicker(list,w,scope);
            }
            if(hero.useRecommendedEdict)
            {var locked=list.gameObject.AddComponent<CanvasGroup>();locked.interactable=false;locked.blocksRaycasts=false;locked.alpha=.45f;}
            EndList(list,4);
        }
        const float JuaLine=1.26f;
        Text Line(Transform parent,string value,float x,float y,float w,float size,float box,Color color,bool display,TextAnchor anchor=TextAnchor.MiddleLeft)
        {var t=Text(parent,value,x,y,w,box,size,color,anchor);skin.ApplyLabel(t,display,color);return t;}
        Text Body(Transform parent,string value,float x,float y,float w,float size,float css,Color color,out float h)
        {var t=Text(parent,value,x,y,w,1000,size,color,TextAnchor.UpperLeft);skin.ApplyLabel(t,false,color);t.lineSpacing=css;h=t.preferredHeight+4;Place(t.rectTransform,x,y,w,h);return t;}
        static Color Alpha(Color color,float alpha){color.a=alpha;return color;}
        void GroupGlyph(Transform parent,string id,float x,float y,float size,Color ink)=>Glyph(parent,id,x,y,size,size,ink);
        void SurfaceChoice(Button button,bool selected,string selectedSkin="chip-on",string idleSkin="button-idle")
        {button.GetComponent<HuntEdictSurface>().Paint(selected?selectedSkin:idleSkin,skin);UiTheme.Choice(button,selected,tab:false);}
        void ChoiceCheck(Transform parent,float w,float h,bool selected)
        {if(selected)HuntEdictDecor.Create(parent,"Choice check",Mathf.Max(0,w-40),(h-26)/2,26,26,"check",skin);}
        InputField Input(Transform parent,string initial,float x,float y,float w,float h,bool multiline=false)
        {
            var r=Panel(parent,"Text input","input-well");Place(r,x,y,w,h);var input=r.gameObject.AddComponent<InputField>();input.targetGraphic=r.GetComponent<Image>();
            var text=Text(r,"",7,3,Mathf.Max(1,w-14),Mathf.Max(1,h-6),Look.BodyText);text.alignment=TextAnchor.UpperLeft;text.supportRichText=false;skin.ApplyLabel(text,false,textColor);
            input.selectionColor=Alpha(skin.Gold,.5f);input.customCaretColor=true;input.caretColor=skin.Navy;
            input.textComponent=text;input.lineType=multiline?InputField.LineType.MultiLineNewline:InputField.LineType.SingleLine;
            input.characterLimit=HuntEdictPresetCodec.MaxCodeCharacters;input.SetTextWithoutNotify(initial);
            if(modal!=null&&r.IsChildOf(modal))
            {int index=dialogValues.Count;dialogValues.Add(initial);input.onValueChanged.AddListener(value=>dialogValues[index]=value);}
            return input;
        }
        // The mockup's .stack: the small "Quick preset" label, the picker button, then the preset's own note, 8 px apart.
        void DrawQuickPresetPicker(RectTransform list,float w,string scope)
        {
            string selected=QuickPresetSelection(scope);bool custom=selected==HuntEdictQuickPresets.Custom;
            var preset=custom?null:HuntEdictQuickPresets.For(scope).Single(p=>p.id==selected);
            var stack=Row(list,"Quick preset stack",1);
            float labelLine=Look.SmallText*JuaLine;Line(stack,Loc.T("간편 프리셋"),0,0,w,Look.SmallText,labelLine,muted,true);
            float y=labelLine+8,titleSize=Look.CardTitle-1;
            var button=Button(stack,preset?.Name??Loc.T("현재 설정"),0,y,w,Look.Button,()=>QuickPresetDialog(scope),"input-well",titleSize);
            button.name="edict-quick-picker-"+scope;((UiButton)button).SetCasualAppearance(radius:14,borderWidth:2,shadow:3,edge:skin.Navy);
            var label=button.GetComponentInChildren<Text>();label.alignment=TextAnchor.MiddleLeft;
            float labelWidth=Mathf.Max(1,w-18-2-14-18-10);Place(label.rectTransform,18,0,labelWidth,1000);
            float h=Mathf.Max(Look.Button,label.preferredHeight+16);
            Place((RectTransform)button.transform,0,y,w,h);Place(label.rectTransform,18,0,labelWidth,h);
            SurfaceChoice(button,false,idleSkin:"input-well");HuntEdictDecor.Create(button.transform,"Picker chevron",w-2-14-18,(h-18)/2,18,18,"chevron",skin);
            y+=h+8;
            var note=Body(stack,preset?.Description??Loc.T("현재 설정을 유지합니다. 프리셋을 골라 저장하세요."),0,y,w,Look.SmallText,1.5f,muted,out float noteHeight);note.name="Quick preset note";
            var layout=stack.GetComponent<LayoutElement>();layout.minHeight=layout.preferredHeight=y+noteHeight;
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
        float dialogBodyTop;
        float DialogTop=>dialogBodyTop;
        float DialogWidth=>Mathf.Min(Look.Dialog,width-2*Look.Padding);
        void Dialog(string title,Action<RectTransform> fill,float dialogHeight=200)
        {
            DismissDialog();restoreDialog=()=>Dialog(title,fill,dialogHeight);modal=Panel(window,"Modal blocker",null);Stretch(modal);modal.GetComponent<Image>().color=Color.clear;
            var dim=Panel(modal,"Dialog screen dim",null);Stretch(dim);FullScreen(dim);dim.GetComponent<Image>().color=Alpha(skin.Navy,.55f);dim.GetComponent<Image>().raycastTarget=false;
            DialogBackdrop("edict-dialog-dismiss");
            var panel=Panel(modal,"Dialog","dialog-panel");float h=Mathf.Min(height-2*Look.Padding,dialogHeight);Place(panel,(width-DialogWidth)/2,(height-h)/2,DialogWidth,h);
            HuntEdictDecor.Create(panel,"Dialog stitch",0,0,DialogWidth,h,"stitch",skin).GetComponent<HuntEdictDecor>().radius=22;
            float close=Look.Close-6;
            var heading=Text(panel,title,Look.CardPadding,12,DialogWidth-2*Look.CardPadding-close-Look.Gap,1000,Look.CardTitle,gold);
            skin.ApplyLabel(heading,true,gold);float titleHeight=Mathf.Max(close,heading.preferredHeight+2);
            Place(heading.rectTransform,Look.CardPadding,12,heading.rectTransform.sizeDelta.x,titleHeight);dialogBodyTop=12+titleHeight+Look.Gap;
            IconButton(panel,"닫기","close",DialogWidth-Look.CardPadding-close,12,close,close,DismissDialog).name="edict-dialog-close";
            fill(panel);
            // Wrapped titles and translated actions may need more height than the nominal recipe.
            float needed=h;
            foreach(Transform child in panel)
            {
                if(child==heading.transform||child.name=="Dialog stitch"||child.name=="edict-dialog-close")continue;
                var rect=child as RectTransform;if(rect!=null&&rect.anchorMin==rect.anchorMax)needed=Mathf.Max(needed,-rect.anchoredPosition.y+rect.sizeDelta.y+Look.CardPadding);
            }
            if(needed>h)
            {
                h=Mathf.Min(height-2*Look.Padding,needed);Place(panel,(width-DialogWidth)/2,(height-h)/2,DialogWidth,h);
                Place((RectTransform)panel.Find("Dialog stitch"),0,0,DialogWidth,h);
            }
        }
        void FullScreen(RectTransform rect)
        {
            var bounds=UiVisibility.ScreenRect(window);float scale=View.GetComponent<Canvas>().scaleFactor;
            rect.offsetMin=-bounds.min/scale;rect.offsetMax=(new Vector2(Screen.width,Screen.height)-bounds.max)/scale;
        }
        void DialogBackdrop(string name)
        {
            // A sibling behind the panel consumes the click. Putting a Button on the modal
            // parent would also dismiss when a non-interactive part inside the panel is tapped.
            var dismiss=Button(modal,"",0,0,width,height,DismissDialog,null);dismiss.name=name;
            var rect=(RectTransform)dismiss.transform;Stretch(rect);
            FullScreen(rect);
            UiTheme.Backdrop(dismiss);dismiss.targetGraphic.color=Color.clear;
        }
        void DismissDialog(){restoreDialog=null;dialogValues.Clear();if(modal==null)return;modal.gameObject.SetActive(false);if(Application.isPlaying)UnityEngine.Object.Destroy(modal.gameObject);else UnityEngine.Object.DestroyImmediate(modal.gameObject);modal=null;}
        void Error(string message)
        {
            GameAudio.Refuse();
            float errorHeight=Mathf.Min(height-2*Look.Padding,Look.Pc?300:landscape?220:360);
            Dialog("확인",p=>
            {
                var s=Scroll(p,"Message",Look.CardPadding,DialogTop,DialogWidth-2*Look.CardPadding,errorHeight-DialogTop-Look.Button-2*Look.Gap,out var list);Paragraph(list,message,DialogWidth-2*Look.CardPadding-16,Look.BodyText,textColor);EndList(list,Look.Gap);
                Button(p,"확인",DialogWidth-Look.CardPadding-Look.ActionMin,errorHeight-Look.CardPadding-Look.Button,Look.ActionMin,Look.Button,DismissDialog,"button-primary",Look.BodyText+3);
            },errorHeight);
        }
        void Confirm(string question,Action yes)=>Dialog(question,p=>
        {
            float w=(DialogWidth-2*Look.CardPadding-Look.Gap)/2;
            Button(p,"취소",Look.CardPadding,DialogTop,w,Look.Button,DismissDialog,size:Look.BodyText+3);
            Button(p,"확인",Look.CardPadding+w+Look.Gap,DialogTop,w,Look.Button,()=>{DismissDialog();yes();},"button-primary",Look.BodyText+3);
        },Look.CardTitle*3+Look.Button+3*Look.Gap+24);
        RectTransform Rect(string name,Transform parent)
        {var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);return r;}
        static void Stretch(RectTransform r){r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;}
        static void Place(RectTransform r,float x,float y,float w,float h)
        {r.anchorMin=r.anchorMax=new Vector2(0,1);r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);}
        RectTransform Panel(Transform parent,string name,string surface)
        {
            var r=Rect(name,parent);
            if(surface==null)r.gameObject.AddComponent<Image>();else
            {
                var face=r.gameObject.AddComponent<HuntEdictSurface>();face.Paint(surface,skin);
                // Dialog plates keep their shadow inside; preset rows retain their existing outside shadow.
                face.shadowOutside=surface!="dialog-panel";
            }
            return r;
        }
        Text Text(Transform parent,string value,float x,float y,float w,float h,float size=12,Color? color=null,TextAnchor align=TextAnchor.MiddleLeft)
        {
            var r=Rect(value.Length<50?value:"Text",parent);Place(r,x,y,w,h);var t=r.gameObject.AddComponent<Text>();
            t.text=ClassSkillTree.Display(Loc.T(value));t.fontSize=Mathf.RoundToInt(size*Mathf.Clamp(lastScale,.8f,1.5f));
            skin.ApplyLabel(t,size>=Look.ChoiceTitle||(value.Length>0&&value.All(c=>char.IsDigit(c)||c=='.'||c=='/'||c=='%')),skin.TextColor(color??textColor));
            t.alignment=align;t.supportRichText=false;t.lineSpacing=1;t.horizontalOverflow=HorizontalWrapMode.Wrap;t.verticalOverflow=VerticalWrapMode.Truncate;t.raycastTarget=false;return t;
        }
        Button Button(Transform parent,string label,float x,float y,float w,float h,Action action,string surface="button-idle",float size=12)
        {
            var r=Panel(parent,label,surface);Place(r,x,y,w,h);var b=r.gameObject.AddComponent<UiButton>();
            b.targetGraphic=r.GetComponent<Image>();
            UiTheme.Skin(b,surface);skin.ApplyChrome(b);
            bool tab=surface=="tab-current"||surface=="tab-idle";
            if(b.Face!=null)b.Face.ShadowOutside=true;
            b.SetCasualAppearance(radius:tab&&!Look.Pc&&landscape?9:surface=="input-well"?12:14,borderWidth:surface=="button-primary"||surface=="tab-current"?3:2,
                shadow:tab&&surface=="tab-idle"?Look.Pc||!landscape?2:1:3,edge:tab&&surface=="tab-idle"?skin.Edge:skin.Navy);
            if(surface=="beat-strip"){b.Configure(UiButtonRole.Icon);b.targetGraphic.canvasRenderer.SetAlpha(1);}
            var caption=Text(r,label,4,0,w-8,h,size,null,TextAnchor.MiddleCenter);skin.ApplyLabel(caption);
            if(surface=="button-primary")
            {
                caption.color=skin.White;var outline=caption.gameObject.AddComponent<Outline>();outline.effectColor=skin.Navy;outline.effectDistance=new Vector2(1,-1);
                var shadow=caption.gameObject.AddComponent<Shadow>();shadow.effectColor=skin.Navy;shadow.effectDistance=new Vector2(0,-2);
            }
            b.onClick.AddListener(()=>action());return b;
        }
        void Glyph(Transform parent,string id,float x,float y,float w,float h,Color? color=null)
        {
            var r=Rect(id,parent);Place(r,x,y,w,h);
            if(AcceptedSemanticArt.TryDraw(r,"nav/"+id,(color??gold).a))return;
            if(StorageGlyph.TryDrawControl(r,id,color??gold))return;var image=r.gameObject.AddComponent<RawImage>();image.texture=Resources.Load<Texture2D>("Art/GlobalHUD/Casual/status-unknown");image.color=new Color(1,1,1,(color??gold).a);image.raycastTarget=false;
        }
        Button IconButton(Transform parent,string label,string icon,float x,float y,float w,float h,Action action,string skin="button-idle")
        {
            var b=Button(parent,label,x,y,w,h,action,skin);b.GetComponentInChildren<Text>().enabled=false;
            if(icon=="close")
            {
                this.skin.ApplyChrome((UiButton)b,true);
                ((UiButton)b).SetCasualAppearance(borderWidth:3,shadow:3);
            }
            float size=Mathf.Min(18,Mathf.Min(w,h)-6);Glyph(b.transform,icon,(w-size)/2,(h-size)/2,size,size);return b;
        }
        ScrollRect Scroll(Transform parent,string name,float x,float y,float w,float h,out RectTransform list,bool mock=false)
        {
            // mock: the casual mockup's .scroll (see MockScroll); the viewport is 6 px wider than the area it serves.
            if(mock)w+=6;
            var viewport=Panel(parent,name,null);Place(viewport,x,y,w,h);viewport.GetComponent<Image>().color=Color.clear;viewport.gameObject.AddComponent<RectMask2D>();
            var scroll=viewport.gameObject.AddComponent<ScrollRect>();scroll.horizontal=false;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=24;
            list=Rect(name+" content",viewport);list.anchorMin=new Vector2(0,1);list.anchorMax=Vector2.one;list.pivot=new Vector2(.5f,1);list.sizeDelta=new Vector2(mock?0:-16,0);
            var layout=list.gameObject.AddComponent<VerticalLayoutGroup>();layout.spacing=mock?Look.Gap:3;if(mock)layout.padding=new RectOffset(2,8,2,10);layout.childControlWidth=true;layout.childForceExpandWidth=true;layout.childControlHeight=true;layout.childForceExpandHeight=false;
            list.gameObject.AddComponent<ContentSizeFitter>().verticalFit=ContentSizeFitter.FitMode.PreferredSize;scroll.viewport=viewport;scroll.content=list;
            // ScrollRect receives background and descendant drags. Interactive reorder cards intercept
            // their own drag stream, so moving a card never pans the list at the same time.
            var barRoot=Panel(viewport,"Scroll track","scroll-track");Place(barRoot,w-12,0,12,h);
            var thumb=Panel(barRoot,"Scroll thumb","scroll-thumb");Stretch(thumb);
            var bar=barRoot.gameObject.AddComponent<Scrollbar>();bar.handleRect=thumb;bar.targetGraphic=thumb.GetComponent<Image>();bar.direction=Scrollbar.Direction.BottomToTop;
            scroll.verticalScrollbar=bar;if(mock)scroll.verticalScrollbarVisibility=ScrollRect.ScrollbarVisibility.AutoHide;return scroll;
        }
        RectTransform Row(Transform parent,string name,float height,string skin=null)
        {
            var r=skin==null?Rect(name,parent):Panel(parent,name,skin);var le=r.gameObject.AddComponent<LayoutElement>();le.minHeight=height;le.preferredHeight=height;return r;
        }
        void Paragraph(Transform parent,string value,float w,float size=12,Color? color=null)
        {
            var row=Row(parent,"Paragraph",18);var t=Text(row,value,0,0,w,1000,size,color);
            bool display=size>Look.BodyText;skin.ApplyLabel(t,display,color??textColor);t.lineSpacing=display?1:size<=Look.SmallText?1.5f:1.55f;
            float h=Mathf.Max(size+4,t.preferredHeight+2);row.GetComponent<LayoutElement>().minHeight=row.GetComponent<LayoutElement>().preferredHeight=h;Place(t.rectTransform,0,0,w,h);
        }
        void EndList(Transform list,float padding)=>Row(list,"Bottom clearance",padding);
    }
}
