using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Hellscript
{
    // The title backdrop as painted depth layers: the far valley with the rift, floating rocks and the two hero cliffs.
    // GameUI gives it its own canvas, so idle motion never rebatches the title controls. Depart() moves it onto a top
    // canvas above the page that opens next and flies the camera into the rift: the heroes part to the sides, the
    // valley rushes toward the crack, the close-up takes over and its light fades into the new page.
    public sealed class TitleVista:MonoBehaviour
    {
        const string Folder="Art/Title/Vista/";
        // Calibration measured on the paintings: glowing core (UV, origin bottom-left) and halo radius (UV height).
        static readonly Vector3 WideCore=new Vector3(.5f,.514f,.07f),TallCore=new Vector3(.499f,.542f,.035f),CloseCore=new Vector3(.4994f,.506f,.16f);
        // The core sits here on screen (fraction of width and height from the bottom-left), between logo and buttons.
        static readonly Vector2 WideFocus=new Vector2(.5f,.53f),TallFocus=new Vector2(.5f,.44f);
        // Hero cliff squares; the heroes' feet stand 40 percent above the bottom of each painting. Beside a narrow
        // button column the cliffs run off the bottom corners. When the buttons span the screen (portrait, small
        // windows) the heroes stand just above them and the rock fades into the valley over its lowest 30 percent.
        const float HeroFeet=.4f;static readonly Vector4 LiftedFade=new Vector4(0,.3f,0,0);
        // Flight timeline: the keyframes in Layout are authored in story seconds and played Pace times slower, so the camera
        // glides forward instead of darting. The light peaks and the painting hides at Peak; the new page is clear at End.
        // The rift burst sound starts at ImpactAt and is loudest in its first 0.1 s, so it lands as the light blooms; the 2.25 s
        // rush sound starts at RushAt so its swell fills the camera's pass between the heroes and it fades out before the burst.
        const float Pace=1.6f,StoryPeak=2.15f,StoryEnd=2.75f;
        public const float Peak=StoryPeak*Pace,End=StoryEnd*Pace,ImpactAt=1.95f*Pace,RushAt=.7f;
        // The heroes finish their dolly at HeroDolly story seconds; their defocus peaks at HeroBlur of the screen height.
        const float FarZoom=2.6f,SkipGuard=.25f,HeroDolly=1.9f,HeroBlur=.03f;
        RectTransform root;CanvasGroup layers;
        RawImage far,mid,close,left,right;Image flash;Material farLight,closeLight,cliffs;
        Texture2D wide,tall;BaseInputModule blockedInput;float flight=-1;
        // Button panel top and width as fractions of the vista, measured while the title is up and kept for the flight.
        RectTransform controls,logo;float homeTop,homeWidth;Vector4 logoShade;static readonly Vector3[] Corners=new Vector3[4];
        const float ShadeStrength=.5f;
        static readonly int ClockId=Shader.PropertyToID("_SceneTime"),CoreId=Shader.PropertyToID("_Core"),GlowId=Shader.PropertyToID("_Glow"),RushId=Shader.PropertyToID("_Rush"),DefocusId=Shader.PropertyToID("_Defocus"),FadeId=Shader.PropertyToID("_Fade"),ShadeId=Shader.PropertyToID("_Shade"),ShadeStrengthId=Shader.PropertyToID("_ShadeStrength");
        public float Clock {get;private set;}
        public bool Departing=>flight>=0;
        // Raised once when the flight passes ImpactAt, also when a tap skips past it. Rush is raised at RushAt
        // only when the frame still lands before ImpactAt, so a skipped flight plays the burst alone.
        public System.Action Impact,Rush;
        public RectTransform LeftHeroes=>left.rectTransform;
        public RectTransform RightHeroes=>right.rectTransform;
        public bool Ready=>far!=null&&far.texture!=null&&close.texture!=null&&left.texture!=null&&right.texture!=null&&farLight!=null&&farLight.shader.isSupported;
        // Smokes step the flight to exact times for frame captures; the frame drawn next uses that time.
        public float FlightTime {get=>flight;set{if(flight<0)return;float before=flight;flight=Mathf.Clamp(value,0,End-.01f);Cue(before);Layout();}}
        public void Initialize(TitleSession state)
        {
            root=(RectTransform)transform;
            var group=Rect("Painted layers",root);layers=group.gameObject.AddComponent<CanvasGroup>();layers.blocksRaycasts=false;
            wide=Resources.Load<Texture2D>(Folder+"vista_far_landscape");tall=Resources.Load<Texture2D>(Folder+"vista_far_portrait");
            var shader=Resources.Load<Shader>("Art/Title/TitleVista");
            if(shader!=null&&shader.isSupported)
            {
                farLight=new Material(shader){name="Title vista far"};closeLight=new Material(shader){name="Title vista close"};
                cliffs=new Material(shader){name="Title vista cliffs"};cliffs.SetFloat(GlowId,0);
            }
            far=Layer("Far valley",group,null,farLight);
            mid=Layer("Floating rocks",group,"vista_mid_floaters",null);
            close=Layer("Rift close-up",group,"vista_rift_close",closeLight);
            left=Layer("Left heroes",group,"vista_heroes_left",cliffs);
            right=Layer("Right heroes",group,"vista_heroes_right",cliffs);right.rectTransform.anchorMin=right.rectTransform.anchorMax=Vector2.right;
            var light=Rect("Rift light",root);light.anchorMin=Vector2.zero;light.anchorMax=Vector2.one;light.offsetMin=light.offsetMax=Vector2.zero;
            flash=light.gameObject.AddComponent<Image>();flash.raycastTarget=false;
            Layout();
        }
        public void KeepClear(RectTransform buttons,RectTransform identity){controls=buttons;logo=identity;Layout();}
        // Moves onto the given top canvas, so tearing down the title page leaves the flight playing over the next one;
        // the vista destroys that canvas when the flight ends.
        public void Depart(Transform host)
        {
            if(flight>=0||!Ready)return;
            var frame=UiSafeArea.FrameNormalized;root.SetParent(host,false);
            root.anchorMin=frame.min;root.anchorMax=frame.max;root.offsetMin=root.offsetMax=Vector2.zero;
            // Real taps and keys skip the flight instead of reaching the hidden page; scripted UI events still work.
            blockedInput=EventSystem.current!=null?EventSystem.current.GetComponent<BaseInputModule>():null;
            if(blockedInput!=null&&blockedInput.enabled)blockedInput.enabled=false;else blockedInput=null;
            flight=0;Layout();
        }
        // Smallest size with the painting's aspect that covers the view while the painting's core sits on the focus.
        public static Vector2 Cover(Vector2 view,float aspect,Vector2 core,Vector2 focus)
        {
            float w=Mathf.Max(focus.x/Mathf.Max(core.x,.01f),(view.x-focus.x)/Mathf.Max(1-core.x,.01f));
            float h=Mathf.Max(focus.y/Mathf.Max(core.y,.01f),(view.y-focus.y)/Mathf.Max(1-core.y,.01f));
            w=Mathf.Max(w,h*aspect);return new Vector2(w,w/aspect);
        }
        void Update()
        {
            if(far==null)return;
            float dt=Mathf.Min(Time.unscaledDeltaTime,.1f);
            if(flight>=0)
            {
                float before=flight;
                if(flight>SkipGuard&&SkipPressed())flight=Mathf.Max(flight,Peak-.12f);
                flight+=dt;Clock+=dt;Cue(before);
                if(flight>=End){Destroy(root.parent.gameObject);return;}
            }
            else Clock+=dt; // Title motion stays on independently of legacy session flags.
            Layout();
        }
        void Cue(float before)
        {
            if(before<RushAt&&flight>=RushAt&&flight<ImpactAt)Rush?.Invoke();
            if(before<ImpactAt&&flight>=ImpactAt)Impact?.Invoke();
        }
        static bool SkipPressed()
        {
            var pointer=Pointer.current;if(pointer!=null&&pointer.press.wasPressedThisFrame)return true;
            var keys=Keyboard.current;return keys!=null&&keys.anyKey.wasPressedThisFrame;
        }
        void Measure(Vector2 size)
        {
            if(controls==null)return;controls.GetWorldCorners(Corners);var r=root.rect;
            Vector2 a=root.InverseTransformPoint(Corners[0]),b=root.InverseTransformPoint(Corners[2]);
            homeTop=(b.y-r.yMin)/size.y;homeWidth=(b.x-a.x)/size.x;
            // The overlay canvas works in screen pixels, which is the shader's screen UV once divided by the screen size.
            if(logo==null)return;logo.GetWorldCorners(Corners);float w=Mathf.Max(1,Screen.width),h=Mathf.Max(1,Screen.height);
            logoShade=new Vector4((Corners[0].x+Corners[2].x)*.5f/w,(Corners[0].y+Corners[2].y)*.5f/h,(Corners[2].x-Corners[0].x)*.75f/w,(Corners[2].y-Corners[0].y)*.9f/h);
        }
        static float Ramp(float from,float to,float x)=>Mathf.SmoothStep(0,1,Mathf.InverseLerp(from,to,x));
        void Layout()
        {
            // Idle title art uses the selected aspect frame, just like the controls. The flight
            // already owns a framed overlay after Depart(); do not change its reparented anchors.
            if(flight<0)
            {
                var frame=UiSafeArea.FrameNormalized;
                if(root.anchorMin!=frame.min||root.anchorMax!=frame.max||root.offsetMin!=Vector2.zero||root.offsetMax!=Vector2.zero)
                {root.anchorMin=frame.min;root.anchorMax=frame.max;root.offsetMin=root.offsetMax=Vector2.zero;}
            }
            var size=root.rect.size;if(size.x<1||size.y<1)return;Measure(size);
            bool portrait=size.y>size.x;var focus=Vector2.Scale(size,portrait?TallFocus:WideFocus);
            float t=Mathf.Max(0,flight)/Pace,travel=Mathf.Clamp01(t/2f);
            float zoom=Mathf.Pow(FarZoom,travel*travel*(3-2*travel))*(1+.012f*(.5f-.5f*Mathf.Cos(Clock*.15f)));
            float shake=flight>=0&&t<StoryPeak?size.y*.005f*Ramp(.9f,2f,t):0;
            var drift=new Vector2(Mathf.Sin(Clock*.21f)*size.x*.004f,Mathf.Sin(Clock*.17f)*size.y*.003f);
            var jolt=new Vector2(Mathf.Sin(t*53)*.6f+Mathf.Sin(t*31)*.4f,Mathf.Cos(t*47)*.6f+Mathf.Sin(t*29)*.4f)*shake;
            var tex=portrait&&tall!=null?tall:wide;var core=portrait&&tall!=null?TallCore:WideCore;
            if(far.texture!=tex)far.texture=tex;
            Paint(far,size,focus,core,zoom,drift*.3f+jolt,1.03f);
            // The floating rocks only crowd the logo at rest; they rush past the camera during the flight.
            float rocks=flight<0?0:Ramp(.1f,.45f,t)*(1-Ramp(1f,1.5f,t));mid.enabled=rocks>0&&mid.texture!=null;
            if(mid.enabled){Paint(mid,size,focus,portrait?TallFocus:WideFocus,Mathf.Pow(zoom,2.4f),drift*.8f+jolt*1.4f,1.04f);mid.color=new Color(1,1,1,rocks);}
            Paint(close,size,focus,CloseCore,1+.9f*Mathf.Pow(Mathf.Clamp01((t-1.3f)/.85f),2),jolt,1.02f);
            float handover=Ramp(1.25f,1.75f,t),light=flight<0?0:t<StoryPeak?Ramp(1.75f,StoryPeak,t):1-Ramp(StoryPeak,StoryEnd,t);
            // Invisible full-screen layers are switched off rather than drawn at zero alpha.
            close.enabled=handover>0;close.color=new Color(1,1,1,handover);flash.enabled=light>0;flash.color=new Color(1,.92f,1,light);
            bool lifted=homeWidth>.5f;float side=lifted?Mathf.Min(size.x*.64f,size.y*.5f):Mathf.Min(size.y*.86f,size.x*.47f);
            float overhang=lifted?.02f:.04f,bottom=lifted?(homeTop+.01f)*size.y-HeroFeet*side:-.025f*size.y;
            // The camera dollies toward the rift: the heroes grow about its vanishing point, gathering speed as the camera
            // passes them, while their focus slips away; they lean outward and down so the middle opens for the camera.
            float dolly=Ramp(0,HeroDolly,t),lens=1/(1-.8f*dolly);var lean=new Vector2(size.x*.14f,size.y*.08f);
            Cliff(left,side,overhang,bottom,lens,focus,lean,size.x,drift*1.2f+jolt*1.6f,-1);Cliff(right,side,overhang,bottom,lens,focus,lean,size.x,drift*1.2f+jolt*1.6f,1);
            if(cliffs!=null){cliffs.SetVector(FadeId,lifted?LiftedFade:Vector4.zero);cliffs.SetFloat(DefocusId,size.y*HeroBlur*Ramp(0,.7f,dolly));}
            Light(farLight,core,(float)tex.width/tex.height,1+2.5f*Ramp(.6f,1.9f,t),.12f*Ramp(.5f,1.7f,t));
            if(farLight!=null){farLight.SetVector(ShadeId,logoShade);farLight.SetFloat(ShadeStrengthId,logoShade.z>0?ShadeStrength*(1-Ramp(0,.4f,t)):0);}
            if(close.texture!=null)Light(closeLight,CloseCore,(float)close.texture.width/close.texture.height,1.5f+4*Ramp(1.5f,StoryPeak,t),.05f*Ramp(1.25f,1.6f,t)+.18f*Ramp(1.6f,StoryPeak,t));
            layers.alpha=t<StoryPeak?1:0;
        }
        static void Paint(RawImage image,Vector2 size,Vector2 focus,Vector2 core,float zoom,Vector2 offset,float overscan)
        {
            var tex=image.texture;if(tex==null)return;var r=image.rectTransform;
            r.pivot=core;r.sizeDelta=Cover(size,(float)tex.width/tex.height,core,focus)*overscan;
            r.anchoredPosition=focus+offset;r.localScale=new Vector3(zoom,zoom,1);
        }
        // Each cliff hangs past its bottom corner. During the flight it grows by lens about the vanishing point, as if the
        // camera were nearing it, and leans out by lean for every extra lens, leaving the screen diagonally down and outward.
        static void Cliff(RawImage image,float side,float overhang,float bottom,float lens,Vector2 focus,Vector2 lean,float width,Vector2 offset,int sign)
        {
            var tex=image.texture;if(tex==null)return;var r=image.rectTransform;
            r.pivot=new Vector2(sign<0?0:1,0);r.sizeDelta=new Vector2(side*tex.width/tex.height,side);
            var home=new Vector2(sign<0?-overhang*side:width+overhang*side,bottom);
            var at=focus+(home-focus)*lens+new Vector2(sign*lean.x,-lean.y)*(lens-1)+offset;
            r.anchoredPosition=new Vector2(sign<0?at.x:at.x-width,at.y);r.localScale=Vector3.one*lens;
        }
        void Light(Material material,Vector3 core,float aspect,float glow,float rush)
        {
            if(material==null)return;
            material.SetVector(CoreId,new Vector4(core.x,core.y,core.z,aspect));material.SetFloat(ClockId,Clock);
            material.SetFloat(GlowId,glow);material.SetFloat(RushId,rush);
        }
        static RectTransform Rect(string name,Transform parent)
        {
            var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);
            r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;return r;
        }
        static RawImage Layer(string name,Transform parent,string file,Material material)
        {
            var r=Rect(name,parent);r.anchorMin=r.anchorMax=Vector2.zero;
            var image=r.gameObject.AddComponent<RawImage>();image.raycastTarget=false;
            if(file!=null)image.texture=Resources.Load<Texture2D>(Folder+file);
            if(material!=null)image.material=material;
            return image;
        }
        void OnDestroy()
        {
            if(blockedInput!=null)blockedInput.enabled=true;
            if(farLight!=null)Destroy(farLight);if(closeLight!=null)Destroy(closeLight);if(cliffs!=null)Destroy(cliffs);
        }
    }
}
