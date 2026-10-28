using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    // Adapter for the approved fixed-coordinate skill design. Store access stays in the host.
    public sealed partial class SkillTreeD4Window:MonoBehaviour
    {
        ISkillTreeD4Host host;SkillTreeD4Model model;RectTransform root,frame,plate,viewport,content,panel,header,legend;
        UnityEngine.UI.CanvasScaler scaler;UnityEngine.UI.ScrollRect scroll;UnityEngine.Canvas uiCanvas;float paintedTextScale;Rect safe;string orient,language,visible;
        float TextScale=>uiCanvas.scaleFactor;
        SkillTreeD4CornerGraphic plateCorners,panelCorners;
        SkillTreeD4FadeGraphic rightFade;
        bool ready;Action closed;string forcedOrient,forcedLocale;SkillTreeD4UnitFit fit;
        public SkillTreeD4Session Session {get;private set;}
        public SkillTreeD4Scene Scene {get;private set;}
        public SkillTreeD4View View {get;private set;}
        public RectTransform Frame=>frame;
        public RectTransform Content=>content;
        public RectTransform Panel=>panel;
        public int SceneBuilds {get;private set;}
        bool showClassTabs=true;ISkillTreeD4SlotHost slotHost;SkillTreeD4SlotDraft slotDraft;
        // The equipped-skill row's edit state (null when the host has no slots or the window is the class-tab design host) and what the last tapped node did to it.
        public SkillTreeD4SlotDraft Slots=>slotDraft;
        public SkillTreeD4SlotResult LastSlot {get;private set;}
        // Why the layout could not be saved on close ("" when it saved or nothing changed); the opener shows it, since the window is gone by then.
        public string CloseError {get;private set;}="";
        public event Action Committed;
        // A successful input action, including reselecting the same card; programmatic Select/Repaint never emits it.
        public event Action<SkillTreeD4Selection> SelectionAccepted;
        public static SkillTreeD4Window Open(Transform parent,ISkillTreeD4Host host,SkillTreeD4Model model=null,string hero="Warrior",string previewOrient=null,string previewLocale=null,GameStore store=null,Action closed=null,int initialSlot=-1,bool previewEquipped=false)
        {
            if(host==null)throw new ArgumentNullException(nameof(host));
            hero=AvailableHero(host,hero);
            var go=new GameObject("d4-window",typeof(RectTransform),typeof(UnityEngine.Canvas),typeof(UnityEngine.UI.CanvasScaler),typeof(UnityEngine.UI.GraphicRaycaster));go.transform.SetParent(parent,false);
            var canvas=go.GetComponent<UnityEngine.Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=340;
            // Keep dark design colours precise; built-in UI shaders convert on the GPU.
            canvas.vertexColorAlwaysGammaSpace=true;
            var window=go.AddComponent<SkillTreeD4Window>();window.uiCanvas=canvas;window.host=host;window.model=model??SkillTreeD4.Model;window.closed=closed;
            window.tacticsStore=store;
            if(host is StoreSkillTreeD4Host owned){window.tacticsCatalog=owned.Catalog;window.tacticsSimulation=owned.Simulation;}
            window.root=(RectTransform)go.transform;window.scaler=go.GetComponent<UnityEngine.UI.CanvasScaler>();window.scaler.uiScaleMode=UnityEngine.UI.CanvasScaler.ScaleMode.ConstantPixelSize;
            // The class strip is a design-preview control; the game shows only the hero's own skill window.
            window.showClassTabs=store==null&&!previewEquipped;
            window.forcedOrient=previewOrient;window.forcedLocale=previewLocale;window.ready=true;
            var levels=new Dictionary<string,int>();var allocations=new Dictionary<string,SkillTreeD4Alloc>();
            foreach(var name in SkillTreeD4Model.Heroes){levels.Add(name,host.Level(name));allocations.Add(name,host.Alloc(name));}
            window.orient=previewOrient??window.CurrentOrient();window.language=previewLocale??Loc.Language;
            window.Session=new SkillTreeD4Session(window.model,hero,window.language,window.orient,levels,allocations);
            foreach(var name in SkillTreeD4Model.Heroes)if(window.model.Classes[name].DesignSelection<0)window.Session.Sel[name]=SkillTreeD4Selection.Active(0);
            // A game window edits the equipped row of its own hero; a slot that opened pressed starts waiting for a skill.
            if((store!=null||previewEquipped)&&host is ISkillTreeD4SlotHost slots)
            {window.slotHost=slots;window.slotDraft=new SkillTreeD4SlotDraft(slots.Slots(hero),host.Level(hero),slots.SlotsBlocked(hero)==""?initialSlot:-1);}
            window.BuildShell();window.Reflow();window.Repaint();ContentWindowHost.Attach(window,window.Close);
            if(store!=null)SkillTacticsViewLifetime.Watch(window,()=>window.TacticsWindow?.OwnerDestroyed());
            if(store!=null)StoreViewBinding.Attach(window,store,window.Resync,()=>!window.InputBusy,visibleState:host.VisibleState);
            window.PresentHostNotice();return window;
        }
        string CurrentOrient()
        {
            var area=UiSafeArea.Current;return SkillTreeD4Units.Orient(area.width,area.height,Application.isMobilePlatform);
        }
        int pressedCount;
        bool InputBusy=>pressedCount>0||scroll!=null&&scroll.velocity.sqrMagnitude>.01f||TryGetComponent<CanvasGroup>(out var gate)&&!gate.interactable;
        void Update()=>Tick(Time.unscaledTime);
        public void Tick(float now)
        {
            if(!ready)return;
            string next=forcedOrient??CurrentOrient(),nextLanguage=forcedLocale??Loc.Language;
            if(next!=orient||nextLanguage!=language)
            {orient=next;language=nextLanguage;Session.Orient=next;Session.Locale=nextLanguage;Reflow();Repaint();}
            else if(safe!=UiSafeArea.Current)Reflow();
            if(paintedTextScale!=TextScale)Repaint();
            if(visible!=host.VisibleState()&&!InputBusy)Resync();
            TickEquip(now);TickFx(now);
        }
        void Resync()=>Repaint();
        static bool HasHero(ISkillTreeD4Host source,string hero)=>!(source is ISkillTreeD4HeroAvailability available)||available.HasHero(hero);
        static string AvailableHero(ISkillTreeD4Host source,string requested)
        {
            if(Array.IndexOf(SkillTreeD4Model.Heroes,requested)>=0&&HasHero(source,requested))return requested;
            foreach(var hero in SkillTreeD4Model.Heroes)if(HasHero(source,hero))return hero;
            throw new InvalidOperationException("The skill tree requires a created hero.");
        }
        void Reflow()
        {
            safe=UiSafeArea.Current;var f=SkillTreeD4Frame.For(orient);
            fit=forcedOrient!=null?new SkillTreeD4UnitFit(orient,1,0,0):SkillTreeD4Units.Fit(safe.x,Screen.height-safe.yMax,safe.width,safe.height,Application.isMobilePlatform);
            scaler.scaleFactor=(float)fit.CanvasScale;
            root.Find("D4Backdrop").GetComponent<SkillTreeD4PaintGraphic>().Apply("frame.root."+orient);
            frame.anchorMin=frame.anchorMax=new Vector2(0,1);frame.pivot=new Vector2(0,1);
            frame.anchoredPosition=new Vector2((float)(fit.FrameX/fit.CanvasScale),(float)(-fit.FrameY/fit.CanvasScale));frame.sizeDelta=new Vector2((float)f.W*2,(float)f.H*2);
        }
        public void Repaint()
        {
            if(!ready)return;
            string nextVisible=host.VisibleState();
            if(visible!=null&&visible!=nextVisible)
            {
                StopMaterialFx();Session.Fx=null;Session.GlintOn=false;Session.Hero=AvailableHero(host,Session.Hero);
                foreach(var name in SkillTreeD4Model.Heroes){Session.Levels[name]=host.Level(name);Session.Alloc[name]=host.Alloc(name);}
            }
            paintedTextScale=TextScale;Vector2 previous=content.anchoredPosition;Scene=Session.Scene();View=Session.View();SceneBuilds++;visible=nextVisible;
            ApplyContextButtons();RenderHeader();RenderEquipped();RenderScene();RenderEquippedTree();RenderPanel();
            RepeatFx();
            Canvas.ForceUpdateCanvases();
            if(orient!="pc")content.anchoredPosition=new Vector2(orient=="landscape"?Mathf.Clamp(previous.x,Mathf.Min(0,viewport.rect.width-content.rect.width),0):0,
                orient=="portrait"?Mathf.Clamp(previous.y,0,Mathf.Max(0,content.rect.height-viewport.rect.height)):0);
        }
        public RectTransform GuideTarget(SkillTreeD4Selection selection,bool? invest=null)
        {
            var action=invest.HasValue?(invest.Value?D4ButtonAction.Invest:D4ButtonAction.Refund):D4ButtonAction.Select;
            foreach(var node in nodes.Values)
                if(node.action==action&&(invest.HasValue||node.selection.Equals(selection))&&node.button!=null&&node.button.interactable&&node.rect.gameObject.activeInHierarchy)return node.rect;
            return null;
        }
        public RectTransform GuideCloseTarget()=>nodes.TryGetValue("header:close",out var node)?node.rect:null;
        public RectTransform GuideModifierRowTarget(SkillTreeD4Selection selection)
        {
            if(!ready||selection.Passive||selection.Modifier<0||!nodes.TryGetValue("panel:mod:"+selection.Modifier,out var node))return null;
            return node.selection.Equals(selection)&&node.button!=null&&node.button.interactable&&node.rect.gameObject.activeInHierarchy?node.rect:null;
        }
        public RectTransform GuideDescriptionTarget(SkillTreeD4Selection selection)
            =>ready&&Session.Selection.Equals(selection)&&nodes.TryGetValue("panel:desc",out var node)&&node.rect.gameObject.activeInHierarchy?node.rect:null;
        public bool Select(SkillTreeD4Selection target)
        {
            if(!ready)return false;StopMaterialFx();Session.Select(target);Repaint();SelectionFx();
            return true;
        }
        public bool Tab(int index)
        {
            if(!ready||index<0||index>=SkillTreeD4Model.Heroes.Length||!HasHero(host,SkillTreeD4Model.Heroes[index]))return false;
            if(visible!=host.VisibleState())Resync();
            if(!Session.Tab(index))return false;StopMaterialFx();Repaint();TabFx();PresentHostNotice();return true;
        }
        public void Close()
        {
            if(RequestTacticsOwnerClose())return;
            if(!ready)return;CommitSlots();ready=false;ContentWindowHost.Detach(this);closed?.Invoke();if(Application.isPlaying)Destroy(gameObject);else DestroyImmediate(gameObject);
        }
        // The equipped row saves once, here: one commit for every edit made while the window was open.
        void CommitSlots()
        {
            CloseError="";if(slotDraft==null||!slotDraft.Dirty)return;
            if(!slotHost.CommitSlots(Session.Hero,slotDraft.Opened(),slotDraft.Slots())){CloseError=slotHost.SlotError;if(string.IsNullOrEmpty(CloseError))CloseError=Loc.T("스킬을 저장하지 못했습니다.");}
            else Committed?.Invoke();
        }
        // A skill-point commit of this window already dropped skills that lost their last rank from the saved layout; follow it and keep the other edits.
        // Only this window's own commit rebases: a layout changed elsewhere must still fail the stale check when the window saves.
        void RebaseSlots()
        {
            if(slotDraft==null)return;var c=model.Classes[Session.Hero];var alloc=Session.Alloc[Session.Hero];
            slotDraft.Rebase(slotHost.Slots(Session.Hero),id=>{int i=Array.FindIndex(c.Actives,a=>a.Id==id);return i>=0&&alloc.Actives[i].Rank>0;});
        }
        void OnDestroy()
        {
            foreach(var material in toneMaterials.Values)if(material!=null){if(Application.isPlaying)Destroy(material);else DestroyImmediate(material);}
            foreach(var material in fxMaterials)if(material!=null){if(Application.isPlaying)Destroy(material);else DestroyImmediate(material);}
        }
        void BuildShell()
        {
            var backdrop=UiLayout.Rect("D4Backdrop",root);UiLayout.Stretch(backdrop);backdrop.gameObject.AddComponent<SkillTreeD4PaintGraphic>().Apply("frame.root."+orient);
            frame=UiLayout.Rect("Frame",root);header=UiLayout.Rect("Header",frame);UiLayout.Stretch(header);
            plate=UiLayout.Rect("d4-plate",frame);plate.gameObject.AddComponent<SkillTreeD4PaintGraphic>();
            viewport=UiLayout.Rect("Viewport",plate);viewport.gameObject.AddComponent<UnityEngine.UI.RectMask2D>();
            var hit=viewport.gameObject.AddComponent<UnityEngine.UI.Image>();hit.color=UnityEngine.Color.clear;
            var clip=UiLayout.Rect("Rounded viewport clip",viewport);UiLayout.Stretch(clip);
            var maskShape=clip.gameObject.AddComponent<SkillTreeD4ShapeGraphic>();maskShape.width=0;maskShape.top=maskShape.bottom=Color.white;maskShape.raycastTarget=false;
            clip.gameObject.AddComponent<Mask>().showMaskGraphic=false;
            content=UiLayout.Rect("d4-content",clip);scroll=plate.gameObject.AddComponent<UnityEngine.UI.ScrollRect>();scroll.viewport=viewport;scroll.content=content;scroll.movementType=UnityEngine.UI.ScrollRect.MovementType.Clamped;
            legend=UiLayout.Rect("d4-legend",plate);legend.gameObject.AddComponent<SkillTreeD4PaintGraphic>().Apply("plate.legend");
            panel=UiLayout.Rect("d4-panel",frame);panel.gameObject.AddComponent<SkillTreeD4PaintGraphic>();
            var edge=UiLayout.Rect("Right edge fade",clip);rightFade=edge.gameObject.AddComponent<SkillTreeD4FadeGraphic>();rightFade.raycastTarget=false;
            var plateCaps=UiLayout.Rect("Plate corner caps",plate);UiLayout.Stretch(plateCaps);plateCorners=plateCaps.gameObject.AddComponent<SkillTreeD4CornerGraphic>();plateCorners.raycastTarget=false;
            var panelCaps=UiLayout.Rect("Panel corner caps",panel);UiLayout.Stretch(panelCaps);panelCorners=panelCaps.gameObject.AddComponent<SkillTreeD4CornerGraphic>();panelCorners.panel=true;panelCorners.raycastTarget=false;
        }
    }
}
