using System;
using System.Collections.Generic;
using System.Linq;
using Hellscript.Runes;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
namespace Hellscript
{
    // Native adapter of the approved prototype, composed inside the shared content shell.
    public sealed partial class RuneBoardWindow:MonoBehaviour
    {
        public RuneBoardSession Session {get;private set;}
        ContentWindowView shell;GameStore store;Func<CombatSimulation> simulation;Action<int> service;Action closed;
        RectTransform stage,storagePanel,inspector,overlays,modal;RuneGemView board;RuneBoardPointer pointer;
        float width,height,U;bool landscape,refresh,closing;int focus,color=-1;bool overview,cameraTouched;
        readonly HashSet<int> sizes=new HashSet<int>();
        Vector2 camera;float cameraScale;string lastWeapon,notice="",modalKind="";float noticeUntil;
        Text hint,toast,zoomLabel;bool noticeError;float storageOffset;int observedRevision;
        public RuneGemView Board=>board;public RectTransform FrameRect=>shell.Frame;public RectTransform StorageRect=>storagePanel;
        public RectTransform InspectorRect=>inspector;public RectTransform DialogRect=>modal;public bool Landscape=>landscape;
        public bool Blocked=>modal!=null;public RuneBoardPointer Pointer=>pointer;
        public static RuneBoardWindow Open(Transform parent,GameStore store,Func<CombatSimulation> simulation,Action<int> service,Action closed,RuneBoardSession session=null)
        {
            RuneBoardWindow owner=null;
            ContentWindowView.Open(parent,"룬 보드",EquipmentViewSource.Draft,()=>1,view=>
            {
                if(owner==null){owner=view.gameObject.AddComponent<RuneBoardWindow>();owner.shell=view;owner.store=store;owner.simulation=simulation;owner.service=service;owner.closed=closed;owner.Session=session??new RuneBoardSession(store.Data.runes,RuneMasteryCatalog.EquippedWeapon(store.Data.Hero)??"sword",p=>RuneV13Art.StorageRotation(p.Shape.Id));owner.observedRevision=store.Data.runes.revision;StoreViewBinding.Attach(owner,store,owner.RefreshCommitted,()=>owner.modal==null&&owner.pointer?.Dragging!=true);}
                owner.Render();
            },()=>{if(owner!=null&&!owner.closing){owner.closing=true;owner.closed?.Invoke();}},back:()=>owner?.Escape(),skin:UiSkin.Casual);
            return owner;
        }
        public Button Find(string name)=>GetComponentsInChildren<Button>().FirstOrDefault(b=>b.name==name);
        static string T(string key,params object[] values)=>RuneBoardText.Get(key,values);
        float E(float value)=>value*U;int Z(float value)=>Mathf.Max(8,Mathf.RoundToInt(E(value)));
        void RefreshCommitted(){if(observedRevision==store.Data.runes.revision)return;observedRevision=store.Data.runes.revision;if(!Session.Dirty)Session.Reload(store.Data.runes);RequestRefresh();}
        public void RequestRefresh()=>refresh=true;
        void OnDisable(){pointer?.Cancel();}
        void LateUpdate()
        {
            if(closing)return;
            if(refresh&&pointer?.Dragging!=true){refresh=false;RememberCamera();shell.Repaint();}
            if(noticeUntil>0&&Time.unscaledTime>noticeUntil){noticeUntil=0;notice="";if(toast!=null)toast.transform.parent.gameObject.SetActive(false);}
            if(modal==null&&Keyboard.current!=null)
            {
                var k=Keyboard.current;bool command=k.ctrlKey.isPressed||k.leftMetaKey.isPressed||k.rightMetaKey.isPressed;
                if(command&&k.sKey.wasPressedThisFrame)Save();
                else if(command&&k.zKey.wasPressedThisFrame){pointer.Cancel();Session.Undo();RequestRefresh();}
                else if(k.rKey.wasPressedThisFrame)Rotate();
                else if(k.deleteKey.wasPressedThisFrame||k.backspaceKey.wasPressedThisFrame)Remove();
                else if((k.enterKey.wasPressedThisFrame||k.spaceKey.wasPressedThisFrame)&&Session.SelectedCell.HasValue)TapCell(Session.SelectedCell.Value);
            }
        }
        void Update(){if(board!=null)RememberCamera();}
        void RememberCamera(){if(board==null)return;camera=board.CameraCenter;cameraScale=board.Scale;}
        public void Escape()
        {if(modal!=null){var practiceInput=modal.GetComponentInChildren<RunePracticePointer>();if(practiceInput!=null&&practiceInput.Dragging)practiceInput.Cancel();else Dismiss();return;}if(pointer?.Dragging==true){pointer.Cancel();return;}if(Session.SelectedId!=null||Session.SelectedCell.HasValue){Session.ClearSelection();RequestRefresh();return;}if(Session.Dirty)OpenDialog("close");else Close();}
        public void Close(){if(closing||RequiredPractice)return;closing=true;pointer?.Cancel();shell.Close();closed?.Invoke();}
        void RequestClose(){if(Session.Dirty)OpenDialog("close");else Close();}
        void Render()
        {
            width=shell.Frame.rect.width;height=shell.Frame.rect.height;landscape=width>height;
            U=Mathf.Clamp(landscape?Mathf.Min(width/75.8f,height/35.7f):Mathf.Min(width/31.7f,height/64.5f),8,17);
            shell.Title.gameObject.SetActive(false);shell.Frame.Find("content-close")?.gameObject.SetActive(false);shell.Frame.GetComponent<UiWindowFrame>().enabled=false;shell.Scroll.gameObject.SetActive(false);
            var background=UiLayout.Rect("Rune surface",shell.Frame);UiLayout.Place(background,0,0,width,height);background.SetAsFirstSibling();var sky=background.gameObject.AddComponent<SkillTreeD4PaintGraphic>();sky.raycastTarget=false;sky.Apply("frame.root.pc",1);
            float header=E(landscape?3:3.4f),tabs=E(landscape?2.8f:5.1f),foot=E(landscape?3.9f:4),gap=E(.5f);
            DrawHeader(header);UiLayout.Place(shell.Navigation,0,header,width,tabs);DrawWeapons(tabs);
            var body=UiLayout.Rect("Rune body",shell.Frame);UiLayout.Place(body,gap,header+tabs+gap,width-2*gap,height-header-tabs-foot-2*gap);
            float w=body.rect.width,h=body.rect.height,mh=E(landscape?2.4f:2.5f);
            if(landscape)
            {
                float bw=(w-gap)*1.55f/2.55f,sw=w-bw-gap,dh=Mathf.Min(h*.44f,E(11.5f));DrawMastery(body,bw,mh);
                BuildStage(body,0,mh+gap,bw,h-mh-gap);storagePanel=Panel(body,"Rune storage panel",bw+gap,0,sw,h-dh-gap);inspector=Panel(body,"Rune inspector",bw+gap,h-dh,sw,dh);
            }
            else
            {
                float sh=E(20.3f),dh=E(7),bh=h-mh-dh-sh-gap*3;DrawMastery(body,w,mh);BuildStage(body,0,mh+gap,w,bh);
                inspector=Panel(body,"Rune inspector",0,mh+bh+gap*2,w,dh);storagePanel=Panel(body,"Rune storage panel",0,h-sh,w,sh);
            }
            DrawStorage();DrawInspector();UiLayout.Place(shell.Actions,0,height-foot,width,foot);DrawActions(foot);
            overlays=UiLayout.Rect("Rune overlays",shell.Frame);UiLayout.Stretch(overlays);modal=null;DrawToast();
            if(!string.IsNullOrEmpty(modalKind))RenderDialog();
            foreach(var b in GetComponentsInChildren<UiButton>())b.Configure(b.Role,b.Chosen,b.Locked);
        }
        RectTransform Panel(Transform parent,string name,float x,float y,float w,float h,Color? fill=null)
        {
            var r=UiLayout.Rect(name,parent);UiLayout.Place(r,x,y,w,h);var surface=r.gameObject.AddComponent<StorageSurface>();surface.raycastTarget=false;
            bool gauge=h<=E(.3f),well=fill.HasValue&&fill.Value==RuneBoardSkin.Inset;
            if(gauge||well)
            {var ink=ColorUtility.ToHtmlStringRGBA(fill??RuneBoardSkin.Panel);surface.Paint(ink,ink,gauge?"00000000":ColorUtility.ToHtmlStringRGBA(RuneBoardSkin.Border));surface.radius=Mathf.Min(E(.6f),h/2);}
            else surface.SetSkin(shell.Skin,radius:E(.6f),borderWidth:E(.13f),shadow:E(.18f));
            return r;
        }
        Text Txt(Transform parent,string value,float x,float y,float w,float h,float em=.9f,Color? color=null,TextAnchor anchor=TextAnchor.MiddleLeft,bool bold=false,bool display=false)
        {var t=UiLayout.Text(parent,"Text",Loc.T(value),x,y,w,h,Z(em),color??RuneBoardSkin.Text,UiFonts.CasualBody(bold));t.alignment=anchor;t.fontStyle=FontStyle.Normal;if(color.HasValue)t.color=shell.Skin.OnLight(color.Value);if(display)RuneBoardSkin.Display(t,bold);t.horizontalOverflow=HorizontalWrapMode.Wrap;return t;}
        Button Btn(Transform parent,string name,string label,float x,float y,float w,float h,Action action,bool primary=false)
        {var b=shell.Button(parent,label,x,y,w,h,action,primary);b.name=name;var t=b.GetComponentInChildren<Text>();t.fontSize=Z(.9f);RuneBoardSkin.Button((UiButton)b);return b;}
        Button IconButton(Transform parent,string name,string icon,float x,float y,float size,Action action)
        {bool shared=icon=="edit"||icon=="book"||icon=="close";var b=UiIconButton.Create(parent,name,icon,x,y,size,size,action,tint:name=="닫기"||name=="rune-dialog-close"?Color.white:RuneBoardSkin.Muted,folder:shared?null:"Art/RuneBoard/Icons/",uv:shared?(Rect?)null:RuneBoardArt.UV("Icons/icon-"+icon),artwork:shared?null:RuneBoardArt.Texture("Icons/icon-"+icon));RuneBoardSkin.Button(b,name=="닫기"||name=="rune-dialog-close");return b;}
        void Art(Transform p,string icon,float x,float y,float size,Color? color=null)
        {
            if(icon=="edit"||icon=="book"||icon=="close")
            {
                var r=UiLayout.Rect("Icon "+icon,p);UiLayout.Place(r,x+size*.12f,y+size*.12f,size*.76f,size*.76f);
                var accepted=AcceptedSemanticArt.ForKey("shared/"+icon);
                var raw=r.gameObject.AddComponent<RawImage>();raw.texture=accepted??Resources.Load<Texture2D>(UiIconButton.Folder+"icon-"+icon);
                if(accepted!=null)raw.uvRect=new Rect(0,0,1,1);
                var ink=color??RuneBoardSkin.Gold;raw.raycastTarget=false;raw.color=accepted!=null?new Color(1,1,1,ink.a):ink;
            }
            else RuneBoardArt.Image(p,"Icons/icon-"+icon,x,y,size,color??RuneBoardSkin.Gold);
        }
        Button ActionButton(Transform parent,string name,string label,string icon,float x,float y,float w,float h,Action action,bool primary=false)
        {var b=Btn(parent,name,label,x,y,w,h,action,primary);float size=Mathf.Min(E(1.45f),h*.65f);Art(b.transform,icon,E(.35f),(h-size)/2,size);var t=b.GetComponentInChildren<Text>();UiLayout.Place(t.rectTransform,size+E(.55f),0,w-size-E(.8f),h);return b;}
        ScrollRect Scroll(Transform parent,string name,float x,float y,float w,float h,out RectTransform content)
        {
            var r=UiLayout.Rect(name,parent);UiLayout.Place(r,x,y,w,h);var scroll=r.gameObject.AddComponent<ScrollRect>();scroll.horizontal=false;scroll.movementType=ScrollRect.MovementType.Clamped;
            var viewport=UiLayout.Rect("Viewport",r);UiLayout.Stretch(viewport);viewport.gameObject.AddComponent<Image>().color=Color.clear;viewport.gameObject.AddComponent<RectMask2D>();
            content=UiLayout.Rect("Content",viewport);content.anchorMin=new Vector2(0,1);content.anchorMax=Vector2.one;content.pivot=new Vector2(.5f,1);content.sizeDelta=Vector2.zero;scroll.viewport=viewport;scroll.content=content;return scroll;
        }
        public void Toast(string message,bool error=false){notice=message;noticeError=error;noticeUntil=Time.unscaledTime+3.3f;if(overlays!=null)DrawToast();}
        void DrawToast(){if(toast!=null)Destroy(toast.transform.parent.gameObject);if(notice==""||overlays==null)return;float h=E(2.8f),w=Mathf.Min(width-E(2),E(42));var box=Panel(overlays,"Rune toast",(width-w)/2,height-E(4.4f)-h,w,h,RuneBoardSkin.Raised);toast=Txt(box,notice,E(.6f),0,w-E(1.2f),h,.85f,noticeError?RuneBoardSkin.Danger:RuneBoardSkin.Text,TextAnchor.MiddleCenter);}
        public void Select(string id){bool wasView=Session.Viewing;Session.Select(id);RequestRefresh();if(wasView)Toast(T("toast.editOn"));}
        public void TapCell(HexCell c)
        {
            if(!Session.Editor.Board.TryGetCell(c,out var cell))return;
            var existing=Session.Editor.DraftPlacements.FirstOrDefault(p=>p.OccupiedCells.Contains(c));
            if(existing!=null&&!Session.Viewing){Session.Select(existing.InstanceId);RequestRefresh();return;}
            if(!Session.Viewing&&Session.Selected!=null&&!Session.SelectedPlaced&&Session.Editor.Board.IsOpen(cell,6)&&cell.Ability.Meaning!="root"&&existing==null){Place(Session.SelectedId,c,Session.Rotation);return;}
            Session.ClearSelection();Session.SelectedCell=c;RequestRefresh();if(!Session.Editor.Board.IsOpen(cell,6)){modalCell=c;OpenDialog("slot");}
        }
        public void Place(string id,HexCell c,int rotation)
        {bool moved=Session.Editor.IsInDraft(id);var p=Session.Preview(id,c,rotation);if(Session.Place(id,c,rotation,out var reason)){GameAudio.Fx("rune.place");PlayerPrefs.SetInt("rune-board-placed",1);Toast(T(moved?"toast.moved":"toast.placed",p.ActiveCount,p.Size));}else{GameAudio.Refuse();Toast(reason,true);}RequestRefresh();}
        public void Rotate(){if(pointer.Dragging){pointer.Rotate();return;}if(Session.Rotate(out var reason)){GameAudio.Fx("rune.rotate");RequestRefresh();}else if(reason!="")Toast(reason,true);}
        public void Remove(){if(Session.Remove(out var reason)){GameAudio.Fx("rune.remove");RequestRefresh();}else if(reason!="")Toast(reason,true);}
        public bool Save(){pointer.Cancel();if(Session.Save(store,simulation?.Invoke(),out var reason)){observedRevision=store.Data.runes.revision;Toast(T("toast.saved"));RequestRefresh();return true;}Toast(reason,true);return false;}
        void Unlock(HexCell c){if(Session.Unlock(c,out var reason)){GameAudio.Fx("rune.unlock");Dismiss();RequestRefresh();}else Toast(reason,true);}
        void SwitchWeapon(string weapon){pointer.Cancel();Session.SwitchWeapon(weapon);focus=0;overview=false;cameraTouched=false;cameraScale=0;RequestRefresh();}
        void SetMode(bool view){pointer.Cancel();Session.SetMode(view);RequestRefresh();}
        public void Preview(RuneCandidate candidate,string hidden=null){board.Board(Session,candidate,hidden,focus);if(hint!=null){hint.text=candidate==null?DefaultHint():candidate.Ok?T("note.canPlace")+" · "+T("note.active",candidate.ActiveCount,candidate.Size):candidate.Reason;hint.color=candidate!=null&&!candidate.Ok?RuneBoardSkin.Danger:RuneBoardSkin.Text;hint.transform.parent.gameObject.SetActive(hint.text!="");}}
        string DefaultHint()=>Session.Viewing?T("note.view"):Session.Selected!=null&&!Session.SelectedPlaced?T("note.pickSlot"):PlayerPrefs.GetInt("rune-board-placed",0)==0&&Session.Editor.DraftPlacements.Count==0?T("note.pickBlock"):"";
    }
}
