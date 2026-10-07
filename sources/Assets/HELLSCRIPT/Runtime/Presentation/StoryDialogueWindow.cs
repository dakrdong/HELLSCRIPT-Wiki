using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Hellscript
{
    // One spoken line. The speaker is an NpcProfiles id; an empty speaker is narration. The cue runs when the line
    // comes on screen (camera framing and the like) and must not touch the account.
    public sealed class StoryLine
    {
        public readonly string speaker,textKo;public readonly Action cue;
        public StoryLine(string speaker,string textKo,Action cue=null){this.speaker=speaker??"";this.textKo=textKo;this.cue=cue;}
    }
    public sealed class StoryChoice
    {
        public readonly string name,labelKo;public readonly Action action;public readonly bool primary,enabled;
        public StoryChoice(string name,string labelKo,Action action,bool primary=false,bool enabled=true){this.name=name;this.labelKo=labelKo;this.action=action;this.primary=primary;this.enabled=enabled;}
    }
    // Progression only. The window is redrawn from this after a language, scale or orientation change,
    // so the line being read and how much of it was written survive the redraw.
    public sealed class StoryDialogueState
    {
        readonly StoryLine[] lines;
        public int Index {get;private set;}
        public float Shown;
        public bool Complete;
        internal int presented=-1;
        public StoryDialogueState(IEnumerable<StoryLine> lines)
        {this.lines=lines?.ToArray()??Array.Empty<StoryLine>();if(this.lines.Length==0)throw new ArgumentException("A dialogue needs at least one line.");}
        public int Count=>lines.Length;
        public StoryLine Current=>lines[Index];
        public StoryLine Previous=>Index>0?lines[Index-1]:null;
        public bool Last=>Index==lines.Length-1;
        // The first press finishes the line being written; the next one moves on. True when the line changed.
        public bool Next()
        {
            if(!Complete){Complete=true;return false;}
            if(Last)return false;
            Index++;Shown=0;Complete=false;return true;
        }
        public bool Skip(){bool moved=!Last;Index=lines.Length-1;Complete=true;return moved;}
    }
    // Shared game dialogue in the manner of a story scene: large transparent character art standing out of a dark band
    // across the bottom, a cartouche name plate on the band's gold rule and a centred line written out a glyph at a
    // time. The speaker stands on the left; a silent listener may stand dimmed on the right, and each looks toward the other. It is a ContentWindowView,
    // so the host still owns input, back and pause leases, and the display never reads or writes an account. Every
    // text is passed as its Korean source (the heading too) so a language change redraws it translated.
    public static class StoryDialogueWindow
    {
        public const float GlyphsPerSecond=38;
        public static bool ReduceMotion=>PlayerPrefs.GetInt(WorldLighting.ReduceMotionKey,0)!=0||SystemReduceMotion;
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
        [System.Runtime.InteropServices.DllImport("/usr/lib/libobjc.A.dylib")]static extern IntPtr objc_getClass(string name);
        [System.Runtime.InteropServices.DllImport("/usr/lib/libobjc.A.dylib")]static extern IntPtr sel_registerName(string name);
        [System.Runtime.InteropServices.DllImport("/usr/lib/libobjc.A.dylib",EntryPoint="objc_msgSend")]static extern IntPtr SendObject(IntPtr receiver,IntPtr selector);
        [System.Runtime.InteropServices.DllImport("/usr/lib/libobjc.A.dylib",EntryPoint="objc_msgSend")]static extern byte SendBool(IntPtr receiver,IntPtr selector);
        static IntPtr workspace,motionSelector;static float motionChecked=-2;static bool systemMotion;
        public static bool SystemReduceMotion
        {
            get
            {
                if(Time.unscaledTime-motionChecked<1)return systemMotion;motionChecked=Time.unscaledTime;
                if(workspace==IntPtr.Zero){var type=objc_getClass("NSWorkspace");if(type==IntPtr.Zero)return false;workspace=SendObject(type,sel_registerName("sharedWorkspace"));motionSelector=sel_registerName("accessibilityDisplayShouldReduceMotion");}
                systemMotion=workspace!=IntPtr.Zero&&SendBool(workspace,motionSelector)!=0;return systemMotion;
            }
        }
#else
        public static bool SystemReduceMotion=>false;
#endif
        public static readonly Vector2 Size=new Vector2(4000,210);
        public static ContentWindowView Open(Transform parent,string localizedTitle,EquipmentViewSource source,Func<float> readingScale,
            StoryDialogueState state,Func<IReadOnlyList<StoryChoice>> choices,Action closed=null,string heading="",bool blocksGameplay=true,Action back=null,
            string listener="")
        {
            ContentWindowView window=null;
            // Battle staging passes blocksGameplay:false so the world keeps presenting camera cues; the host's pause lease still holds the run.
            window=ContentWindowView.Open(parent,localizedTitle,source,readingScale,Scene(state,readingScale,choices,heading,listener),
                closed,maximumSize:Size,back:back??(()=>Skip(window,state)),blocksGameplay:blocksGameplay,bottomDock:true);
            state.Current.cue?.Invoke();
            return window;
        }
        // The renderer for a ContentWindowView opened with bottomDock and Size; other windows open their own shell with it.
        public static Action<ContentWindowView> Scene(StoryDialogueState state,Func<float> readingScale,Func<IReadOnlyList<StoryChoice>> choices,string heading="",
            string listener="",bool choicesAtOnce=false,bool closable=false,string lineName="Dialogue line")
            =>view=>Render(view,readingScale?.Invoke()??1,state,choices,heading,listener,choicesAtOnce,closable,lineName,()=>Advance(view,state),()=>Skip(view,state));
        static void Advance(ContentWindowView view,StoryDialogueState state){if(view!=null&&state.Next()){state.Current.cue?.Invoke();view.Repaint();}}
        static void Skip(ContentWindowView view,StoryDialogueState state){if(view==null)return;if(state.Skip())state.Current.cue?.Invoke();view.Repaint();}
        static RawImage Figure(ContentWindowView view,string name,Texture2D texture,float x,float y,float height,Color tint)
        {
            float width=height*texture.width/texture.height;var art=UiLayout.Rect(name,view.Frame);UiLayout.Place(art,x,y,width,height);
            var image=art.gameObject.AddComponent<RawImage>();image.texture=texture;image.raycastTarget=false;image.color=tint;return image;
        }
        static void Render(ContentWindowView view,float reading,StoryDialogueState state,Func<IReadOnlyList<StoryChoice>> choices,string heading,string listenerPath,
            bool choicesAtOnce,bool closable,string lineName,Action advance,Action skip)
        {
            var line=state.Current;var profile=NpcProfiles.Find(line.speaker);
            var safe=UiSafeArea.Current.size/UiTheme.Scale(UiSafeArea.Current);float w=view.Width,h=view.Frame.rect.height;bool narrow=w<600;
            // A narrow screen has room for the speaker only; two figures side by side would hide the whole field.
            // The speaker on the left looks right and the listener on the right looks left, so the two face each other.
            var speakerArt=profile!=null?Resources.Load<Texture2D>(profile.PortraitFor(true)):null;// A line the listener speaks shows that figure once, as the speaker.
            var listenerProfile=NpcProfiles.FindByPortrait(listenerPath);
            var listenerArt=!narrow&&listenerPath!=""&&profile?.portrait!=listenerPath?Resources.Load<Texture2D>(listenerProfile!=null?listenerProfile.PortraitFor(false):listenerPath):null;
            view.Frame.GetComponent<Image>().color=Color.clear;view.Title.gameObject.SetActive(false);view.Navigation.gameObject.SetActive(false);
            var close=view.Frame.Find("content-close");
            // Landscape figures stand from the bottom of the safe area and rise far above the band, in front of it; on a narrow
            // screen they stand on the band, behind it, so its rule hides where each picture is cut.
            float figureH=narrow?Mathf.Min(safe.y*.44f,w*.5f*1.5f):Mathf.Min(safe.y*.86f,w*.3f*1.5f),top=13*reading+8,rest=Mathf.Min(16,top-2);
            float figureY=narrow?rest-figureH:h+8-figureH,left=24,right=w-24;
            var band=UiLayout.Rect("Dialogue band",view.Frame);UiLayout.Place(band,-8,-40*reading,w+16,h+40*reading+8);
            var bandArt=band.gameObject.AddComponent<DialogueBandGraphic>();bandArt.fade=40*reading;bandArt.raycastTarget=false;
            RawImage speaker=null,silent=null;
            if(speakerArt!=null)
            {
                speaker=Figure(view,"Dialogue portrait "+profile.id,speakerArt,narrow?0:4,figureY,figureH,Color.white);
                if(!narrow)left=4+speaker.rectTransform.rect.width+14;
                if(state.presented!=state.Index&&(state.Previous==null||state.Previous.speaker!=line.speaker)&&!ReduceMotion)speaker.gameObject.AddComponent<DialogueEntrance>();
            }
            if(listenerArt!=null)
            {
                // The one who listens stays in shadow, the way a story scene marks who is speaking.
                float listenerW=figureH*listenerArt.width/listenerArt.height;
                silent=Figure(view,"Dialogue listener",listenerArt,narrow?w-listenerW:w-4-listenerW,figureY,figureH,new Color(.4f,.4f,.44f,1));
                if(!narrow)right=w-4-listenerW-14;
            }
            // Order: narrow figures, band, then every control and line; landscape figures come after the band, in front of it.
            band.SetAsFirstSibling();
            if(narrow){if(silent!=null)silent.transform.SetAsFirstSibling();if(speaker!=null)speaker.transform.SetAsFirstSibling();}
            state.presented=state.Index;
            if(narrow){left=16;right=w-16;}
            float viewH=Mathf.Max(1,h-top-50);
            // The body's layout group sizes the line to the viewport, so place the viewport before measuring the line.
            UiLayout.Place(view.Scroll.viewport,left,top,right-left,viewH);
            var text=UiLayout.Text(view.Body,lineName,Loc.T(line.textKo),0,0,right-left,48,Mathf.RoundToInt(UiTheme.Heading*reading),profile!=null?StorageSurface.Hex(UiTheme.TextHex):UiTheme.Gold);
            text.alignment=TextAnchor.UpperCenter;text.lineSpacing=1.12f;text.supportRichText=false;
            Canvas.ForceUpdateCanvases();
            // A line that does not fit between the two figures takes the listener's place; the speaker always stays.
            if(silent!=null&&text.preferredHeight+text.fontSize*.4f+3>viewH)
            {
                silent.gameObject.SetActive(false);UnityEngine.Object.Destroy(silent.gameObject);silent=null;right=w-24;
                UiLayout.Place(view.Scroll.viewport,left,top,right-left,viewH);Canvas.ForceUpdateCanvases();
            }
            float centre=narrow?w/2:(left+right)/2;
            if(profile!=null)
            {
                int nameSize=Mathf.RoundToInt(UiTheme.Heading*reading),roleSize=Mathf.RoundToInt(UiTheme.Caption*reading);float ph=28*reading;
                var plate=UiLayout.Rect("Speaker plate",view.Frame);plate.gameObject.AddComponent<CartoucheGraphic>().raycastTarget=false;
                var name=UiLayout.Text(plate,"Speaker name",Loc.T(profile.name),0,0,400,ph,nameSize,StorageSurface.Hex(UiTheme.TextHex));name.alignment=TextAnchor.MiddleLeft;
                name.supportRichText=false;name.horizontalOverflow=HorizontalWrapMode.Overflow;float nameW=name.preferredWidth;
                var role=UiLayout.Text(plate,"Speaker role",Loc.T(heading!=""?heading:profile.Role),0,0,300,ph,roleSize,UiTheme.Gold);role.alignment=TextAnchor.MiddleLeft;
                role.supportRichText=false;role.horizontalOverflow=HorizontalWrapMode.Overflow;float roleW=role.preferredWidth;
                // A long name keeps the plate; the role gives way first so nothing spills past it.
                float pad=ph*.9f,full=pad*2+nameW+10*reading+roleW,pw=Mathf.Min(right-left+32,full);bool withRole=pw+.5f>=full;role.gameObject.SetActive(withRole);
                if(!withRole)pw=Mathf.Min(pw,pad*2+nameW);
                UiLayout.Place(plate,centre-pw/2,-ph/2,pw,ph);UiLayout.Place(name.rectTransform,pad,0,Mathf.Max(1,Mathf.Min(nameW,pw-pad*2)),ph);
                UiLayout.Place(role.rectTransform,pad+nameW+10*reading,0,Mathf.Max(1,roleW),ph);
            }
            UiLayout.Place(view.Actions,left,h-44,right-left,36);
            view.Scroll.scrollSensitivity=18*reading;
            // Large text can still outgrow the band: a thin rail beside the line says there is more to read.
            var rail=UiLayout.Rect("Dialogue scroll indicator",view.Frame);UiLayout.Place(rail,right+4,top,3,viewH);rail.gameObject.AddComponent<Image>().color=UiTheme.Panel;
            var thumb=UiLayout.Rect("Scroll thumb",rail);UiLayout.Stretch(thumb);thumb.gameObject.AddComponent<Image>().color=UiTheme.Gold;
            var bar=rail.gameObject.AddComponent<Scrollbar>();bar.handleRect=thumb;bar.targetGraphic=thumb.GetComponent<Image>();bar.direction=Scrollbar.Direction.BottomToTop;
            view.Scroll.verticalScrollbar=bar;view.Scroll.verticalScrollbarVisibility=ScrollRect.ScrollbarVisibility.AutoHide;
            var layout=text.gameObject.AddComponent<LayoutElement>();layout.preferredHeight=layout.minHeight=Mathf.Ceil(text.preferredHeight+text.fontSize*.4f)+2;
            var reveal=text.gameObject.AddComponent<DialogueReveal>();reveal.state=state;if(ReduceMotion)state.Complete=true;
            var driver=view.Frame.gameObject.AddComponent<StoryDialogueDriver>();driver.state=state;driver.advance=advance;driver.reveal=reveal;driver.choicesAtOnce=choicesAtOnce;
            driver.scroll=view.Scroll;view.Scroll.gameObject.AddComponent<StoryScrollWatch>().driver=driver;
            // A tap anywhere writes out the line or moves on; buttons keep their own clicks.
            var backdrop=view.transform.Find("Backdrop");if(backdrop!=null)backdrop.gameObject.AddComponent<StoryDialogueClick>().driver=driver;

            float corner=Mathf.Max(96,74*reading+30);
            var skipButton=view.Button(view.Frame,"건너뛰기 »",w-corner-8,-40*reading-30*reading,corner,26*reading,skip);skipButton.name="dialogue-skip";
            UiTheme.Skin(skipButton,"quiet");skipButton.GetComponentInChildren<Text>().fontSize=Mathf.RoundToInt(UiTheme.Caption*reading);
            if(close!=null)
            {
                close.gameObject.SetActive(closable);close.SetAsLastSibling();
                if(closable)UiLayout.Place((RectTransform)close,w-44,-40*reading-34,36,32);
            }
            var next=view.Button(view.Actions,"▶",right-left-46,0,46,36,advance);next.name="dialogue-next";UiTheme.Skin(next,"quiet");
            var cue=next.GetComponentInChildren<Text>();cue.color=UiTheme.Gold;
            driver.nextButton=next.gameObject;driver.skipButton=skipButton.gameObject;driver.cue=cue;
            var set=state.Last?choices?.Invoke()??Array.Empty<StoryChoice>():Array.Empty<StoryChoice>();
            if(set.Count>0)
            {
                var row=UiLayout.Rect("Dialogue choices",view.Actions);UiLayout.Place(row,0,0,right-left,36);driver.choices=row.gameObject;
                float cw=Mathf.Min(220*reading,(right-left-(set.Count-1)*UiTheme.Gap)/set.Count),start=(right-left-set.Count*cw-(set.Count-1)*UiTheme.Gap)/2;
                for(int i=0;i<set.Count;i++)
                {
                    var choice=set[i];
                    var b=view.Button(row,choice.labelKo,start+i*(cw+UiTheme.Gap),0,cw,36,()=>choice.action?.Invoke(),choice.primary);b.name=choice.name;b.interactable=choice.enabled;
                }
            }
            driver.Refresh();
        }
    }
    // Owns the moment-to-moment life of one drawn line: the continue cue, when the choices show, and keyboard advance.
    public sealed class StoryDialogueDriver:MonoBehaviour,IPointerClickHandler
    {
        internal StoryDialogueState state;internal Action advance;internal DialogueReveal reveal;internal bool choicesAtOnce,userScrolled;
        internal GameObject nextButton,skipButton,choices;internal Text cue;internal ScrollRect scroll;
        bool Done=>state.Last&&state.Complete;
        public void OnPointerClick(PointerEventData e){if(e.button==PointerEventData.InputButton.Left&&!Done)advance?.Invoke();}
        internal void Refresh()
        {
            // A conversation that offers its services at once shows them while the greeting is still being written.
            if(choices!=null)choices.SetActive(choicesAtOnce?state.Last:Done);
            if(nextButton!=null)nextButton.SetActive(!Done&&!(choicesAtOnce&&state.Last));
            if(skipButton!=null)skipButton.SetActive(!Done&&state.Count>1);
        }
        void Update()
        {
            Refresh();
            // A line longer than the band follows the writing down, until the reader scrolls it themselves.
            if(scroll!=null&&!userScrolled&&!state.Complete&&reveal!=null&&reveal.Glyphs>0&&scroll.content.rect.height>scroll.viewport.rect.height+1)
                scroll.verticalNormalizedPosition=1-Mathf.Clamp01(state.Shown/reveal.Glyphs);
            if(cue!=null)
            {
                float pulse=Mathf.Abs(Mathf.Sin(Time.unscaledTime*3.2f));var c=cue.color;
                c.a=state.Complete&&!state.Last&&!StoryDialogueWindow.ReduceMotion?.4f+.6f*pulse:1;cue.color=c;
            }
            var keys=Keyboard.current;
            // A focused button already answers Enter through the event system; do not advance twice.
            if(keys!=null&&EventSystem.current?.currentSelectedGameObject==null&&(keys.spaceKey.wasPressedThisFrame||keys.enterKey.wasPressedThisFrame||keys.numpadEnterKey.wasPressedThisFrame)&&!Done)advance?.Invoke();
        }
    }
    public sealed class StoryScrollWatch:MonoBehaviour,IScrollHandler,IBeginDragHandler
    {
        internal StoryDialogueDriver driver;
        public void OnScroll(PointerEventData e){if(driver!=null)driver.userScrolled=true;}
        public void OnBeginDrag(PointerEventData e){if(driver!=null)driver.userScrolled=true;}
    }
    public sealed class StoryDialogueClick:MonoBehaviour,IPointerClickHandler
    {
        internal StoryDialogueDriver driver;
        public void OnPointerClick(PointerEventData e){if(driver!=null)driver.OnPointerClick(e);}
    }
    // Writes the line out by fading glyph quads in order. Text.text stays the full line, so wrapping, preferred
    // height and every text check see the finished sentence from the first frame.
    public sealed class DialogueReveal:BaseMeshEffect
    {
        internal StoryDialogueState state;int glyphs=-1;bool drawnComplete;
        public int Glyphs=>glyphs;
        void Update()
        {
            if(state==null)return;
            if(state.Complete){if(!drawnComplete){drawnComplete=true;graphic.SetVerticesDirty();}return;}
            state.Shown+=Time.unscaledDeltaTime*StoryDialogueWindow.GlyphsPerSecond;
            if(glyphs>=0&&state.Shown>=glyphs)state.Complete=true;
            graphic.SetVerticesDirty();
        }
        public override void ModifyMesh(VertexHelper mesh)
        {
            if(!IsActive()||state==null)return;glyphs=mesh.currentVertCount/4;
            if(!state.Complete)Conceal(mesh,state.Shown);
        }
        // Text emits one four-vertex quad per visible glyph in reading order; hide what has not been written yet.
        public static void Conceal(VertexHelper mesh,float shown)
        {
            int whole=Mathf.Max(0,Mathf.FloorToInt(shown));float part=Mathf.Clamp01(shown-whole);var v=new UIVertex();
            for(int i=whole*4;i<mesh.currentVertCount;i++)
            {mesh.PopulateUIVertex(ref v,i);v.color.a=(byte)(i/4==whole?v.color.a*part:0);mesh.SetUIVertex(v,i);}
        }
    }
    // A new speaker fades in where they stand: the figure's bounds never change, so layout checks and scrolling see it fixed.
    public sealed class DialogueEntrance:MonoBehaviour
    {
        const float Duration=.3f;float t;RawImage image;Color tint;
        void Start(){image=GetComponent<RawImage>();if(image!=null)tint=image.color;Step();}
        void Update(){t+=Time.unscaledDeltaTime;Step();if(t>=Duration)Destroy(this);}
        void Step(){float k=Mathf.Clamp01(t/Duration);if(image!=null)image.color=new Color(tint.r,tint.g,tint.b,tint.a*(1-(1-k)*(1-k)*(1-k)));}
    }
    // The dialogue band: clear at the top, deepening into an opaque lacquer body, with a double brass rule whose ends fade
    // out. Opaque where text sits: under linear blending even a .96 dark overlay lets roughly a quarter of the HUD show.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class DialogueBandGraphic:TitleFrameGraphic
    {
        public float fade=40;
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();var r=rectTransform.rect;var ink=UiTheme.Tint(UiTheme.VoidHex);var gold=UiTheme.Gold;
            Color A(Color c,float a){c.a=a;return c;}
            float rule=r.yMax-fade;
            Quad(vh,new Rect(r.xMin,rule,r.width,fade),A(ink,.8f),A(ink,0),A(ink,.8f),A(ink,0));
            Quad(vh,new Rect(r.xMin,rule-16,r.width,16),A(ink,1),A(ink,.8f),A(ink,1),A(ink,.8f));
            Quad(vh,new Rect(r.xMin,r.yMin,r.width,rule-16-r.yMin),ink,ink,ink,ink);
            foreach(var (y,t,a) in new[]{(rule,1.3f,.95f),(rule-4,.7f,.4f)})
            {
                float x0=r.xMin+r.width*.03f,x1=r.xMax-r.width*.03f,ramp=r.width*.16f;
                Quad(vh,new Rect(x0,y-t/2,ramp,t),A(gold,0),A(gold,0),A(gold,a),A(gold,a));
                Quad(vh,new Rect(x0+ramp,y-t/2,x1-x0-ramp*2,t),A(gold,a),A(gold,a),A(gold,a),A(gold,a));
                Quad(vh,new Rect(x1-ramp,y-t/2,ramp,t),A(gold,a),A(gold,a),A(gold,0),A(gold,0));
            }
        }
        // Corners: bottom-left, top-left, bottom-right, top-right.
        static void Quad(VertexHelper vh,Rect r,Color bl,Color tl,Color br,Color tr)
        {
            if(r.width<=0||r.height<=0)return;int i=vh.currentVertCount;
            vh.AddVert(new Vector3(r.xMin,r.yMin),bl,Vector2.zero);vh.AddVert(new Vector3(r.xMin,r.yMax),tl,Vector2.zero);
            vh.AddVert(new Vector3(r.xMax,r.yMax),tr,Vector2.zero);vh.AddVert(new Vector3(r.xMax,r.yMin),br,Vector2.zero);vh.AddTriangle(i,i+1,i+2);vh.AddTriangle(i,i+2,i+3);
        }
    }
    // A pointed cartouche for the speaker's name: deep oxblood lacquer, a brass edge and an inner hairline.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class CartoucheGraphic:TitleFrameGraphic
    {
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();var r=rectTransform.rect;float tip=r.height*.5f;var gold=UiTheme.Gold;var faint=gold;faint.a=.45f;
            Vector2[] Shape(float inset)=>new[]{new Vector2(r.xMin+inset*1.6f,r.center.y),new Vector2(r.xMin+tip+inset,r.yMax-inset),new Vector2(r.xMax-tip-inset,r.yMax-inset),
                new Vector2(r.xMax-inset*1.6f,r.center.y),new Vector2(r.xMax-tip-inset,r.yMin+inset),new Vector2(r.xMin+tip+inset,r.yMin+inset)};
            var outer=Shape(0);var top=StorageSurface.Hex("4a2217");var bottom=StorageSurface.Hex("1b0c08");
            int c=vh.currentVertCount;vh.AddVert(r.center,Color.Lerp(top,bottom,.5f),Vector2.zero);
            foreach(var p in outer)vh.AddVert(p,Color.Lerp(bottom,top,Mathf.InverseLerp(r.yMin,r.yMax,p.y)),Vector2.zero);
            for(int i=0;i<6;i++)vh.AddTriangle(c,c+1+i,c+1+(i+1)%6);
            for(int i=0;i<6;i++)Line(vh,outer[i],outer[(i+1)%6],1.4f,gold);
            var inner=Shape(3);for(int i=0;i<6;i++)Line(vh,inner[i],inner[(i+1)%6],.7f,faint);
            foreach(var p in new[]{outer[0]+Vector2.left*7,outer[3]+Vector2.right*7})Diamond(vh,p,3.2f,gold);
        }
        static void Diamond(VertexHelper vh,Vector2 c,float s,Color tint)
        {
            int i=vh.currentVertCount;vh.AddVert(c+Vector2.up*s,tint,Vector2.zero);vh.AddVert(c+Vector2.right*s*.7f,tint,Vector2.zero);
            vh.AddVert(c+Vector2.down*s,tint,Vector2.zero);vh.AddVert(c+Vector2.left*s*.7f,tint,Vector2.zero);vh.AddTriangle(i,i+1,i+2);vh.AddTriangle(i,i+2,i+3);
        }
    }
}
