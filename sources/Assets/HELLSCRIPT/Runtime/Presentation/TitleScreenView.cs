using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Hellscript
{
    // The title owns its presentation only; entering and choosing a hero still use GameController.
    public sealed partial class TitleScreenView:MonoBehaviour
    {
        GameController game;Font font,displayFont;TitleAtmosphere atmosphere;Action settings;
        RectTransform root,home,logoGroup,toolbar,footline;CanvasGroup homeGroup;
        Button login,guest,server,enter,motion,language,options;
        Text logo,eyebrow,tagline,chapter,footnote,version,entryHint;
        Vector2 lastSize;float scale=1;bool wasSettingsOpen;
        public TitleSession Session {get;private set;}
        public TitleAtmosphere Atmosphere=>atmosphere;
        public bool DialogOpen=>dialog!=null;
        public string DialogKind {get;private set;}="";
        public RectTransform Home=>home;
        static readonly Color Bone=new Color(.91f,.865f,.75f),Gold=new Color(.65f,.49f,.3f),Muted=new Color(.67f,.69f,.69f),Slate=new Color(.047f,.049f,.053f,.93f),Red=new Color(.36f,.064f,.045f,.98f);

        public void Initialize(GameController owner,Font bodyFont,TitleSession state,TitleAtmosphere backdrop,Action openSettings)
        {
            game=owner;font=bodyFont;Session=state;atmosphere=backdrop;settings=openSettings;root=(RectTransform)transform;
            displayFont=UiFonts.Display;
            if(displayFont==null)displayFont=font;
            toolbar=Rect("Title tools",root);
            motion=ActionButton(toolbar,"title-motion","연출 켜짐",()=>{Session.MotionEnabled=!Session.MotionEnabled;Refresh();},false,13);
            language=ActionButton(toolbar,"title-language",LanguageOptions.Find(Loc.Language).NativeName,()=>game.ApplyLanguage(Loc.Language=="ko"?"en":"ko"),false,13);
            options=ActionButton(toolbar,"설정·안내","",()=>{CloseDialog();settings();},false,13);
            var gearRect=Rect("Settings gear",options.transform);Fill(gearRect);gearRect.offsetMin=Vector2.one*12;gearRect.offsetMax=-Vector2.one*12;
            var gear=gearRect.gameObject.AddComponent<SettingsGearGraphic>();gear.color=Bone;gear.raycastTarget=false;
            chapter=Caption(root,"chapter","잿빛 성소",12,Gold,TextAnchor.MiddleLeft);
            logoGroup=Rect("Title identity",root);
            eyebrow=Caption(logoGroup,"eyebrow","금지된 기록이 깨어난다",13,Bone);
            logo=Caption(logoGroup,"HELLSCRIPT","HELLSCRIPT",88,Bone);logo.font=displayFont;
            var shadow=logo.gameObject.AddComponent<Shadow>();shadow.effectColor=new Color(.07f,.01f,.007f,.95f);shadow.effectDistance=new Vector2(0,-3);
            var seal=Rect("Sanctuary sigil",logoGroup);var ornament=seal.gameObject.AddComponent<TitleOrnament>();ornament.color=Gold;ornament.raycastTarget=false;
            tagline=Caption(logoGroup,"tagline","재가 된 성소에서, 다시 깨어나라.",15,Bone);
            home=Rect("Title entry controls",root);homeGroup=home.gameObject.AddComponent<CanvasGroup>();
            login=GoogleButton(home,"title-login","Google로 계속",ShowLogin,14,1);
            guest=ActionButton(home,"title-guest","게스트로 계속",GuestAction,false,15);
            server=ActionButton(home,"title-server","서버 선택",ShowServers,false,16);
            enter=ActionButton(home,"title-enter","캐릭터 선택하기",Enter,true,22);
            entryHint=Caption(home,"entry-hint","로그인하거나 게스트로 시작하세요.",12,Muted);
            footline=Rect("Title footer",root);
            footnote=Caption(footline,"demo-note","진행 상황은 현재 기기에 저장됩니다.",11,Muted,TextAnchor.MiddleLeft);
            version=Caption(footline,"version","HELLSCRIPT  /  "+Application.version,11,Muted,TextAnchor.MiddleRight);
            Refresh();Reflow(true);
            if(game.GoogleLogin.Busy||game.GoogleLogin.Ready&&(!Session.SignedIn||Session.Guest))ShowLogin();
        }
        void Refresh()
        {
            SetCaption(login,Loc.T(Session.SignedIn&&!Session.Guest?"Google 계정":"Google로 계속"));
            SetCaption(guest,Session.SignedIn?Loc.T("로그아웃"):Loc.T("게스트로 계속"));
            guest.onClick.RemoveAllListeners();guest.onClick.AddListener(()=>{game.Audio?.Play(SoundCue.Select);GuestAction();});
            SetCaption(server,Loc.F("{0}   ·   {1}   ›",TitleSession.ServerNames[Session.Server],TitleSession.ServerStates[Session.Server]));
            SetCaption(motion,Loc.T(Session.MotionEnabled?"연출 켜짐":"연출 멈춤"));
            SetCaption(enter,Loc.T("캐릭터 선택하기"));
            entryHint.text=Loc.T("로그인 후 캐릭터를 선택합니다.");
        }
        public Button Find(string id)
        {
            foreach(var button in GetComponentsInChildren<Button>())if(button.name==id)return button;
            return null;
        }
        public InputField FindInput(string id)
        {
            foreach(var input in GetComponentsInChildren<InputField>())if(input.name==id)return input;
            return null;
        }
        void GuestAction()
        {
            if(Session.SignedIn){if(!game.SignOutAccount())game.UI.ShowToast(game.Notice);Refresh();return;}
            if(!game.EnterAsGuest())game.UI.ShowToast(game.Notice);
        }
        void Enter()
        {
            if(!Session.SignedIn){ShowLogin();return;}
            game.UI.OpenCharacterSelection();
        }
        void Update()
        {
            Reflow();
            RefreshGoogleLogin();
            if(dialog!=null&&lastKeyboardHeight!=(TouchScreenKeyboard.visible?TouchScreenKeyboard.area.height/root.GetComponentInParent<Canvas>().scaleFactor:0))ReflowDialog();
            if(game==null)return;
            bool blocked=game.UI.CommonPanelOpen;
            if(blocked!=wasSettingsOpen){wasSettingsOpen=blocked;homeGroup.interactable=!blocked&&!Session.Entering;toolbar.GetComponent<CanvasGroup>().interactable=!blocked&&!Session.Entering;}
            if(blocked||Session.Entering)return;
            var keys=Keyboard.current;
            if(keys==null)return;
            if(keys.escapeKey.wasPressedThisFrame){if(DialogOpen)CloseDialog();else settings();}
            // InputField and Button already handle submit/navigation through the EventSystem.
            if(keys.enterKey.wasPressedThisFrame&&!DialogOpen&&EventSystem.current?.currentSelectedGameObject==null)Enter();
        }
        void Reflow(bool force=false)
        {
            var size=root.rect.size;if(!force&&size==lastSize)return;lastSize=size;
            bool portrait=size.y>size.x;
            scale=Mathf.Clamp(Mathf.Min(size.x/(portrait?480:1180),size.y/(portrait?980:780)),1,1.65f);
            float w=size.x/scale,h=size.y/scale;
            // Use a bounded central column on PC; keep the same fixed controls in the lower portrait safe area.
            bool compact=!portrait&&h<570;
            float column=Mathf.Min(compact?660:460,w-40),homeY=h-(compact?216:252);
            Fit(home,(w-column)/2,homeY,column,compact?182:214);home.localScale=Vector3.one*scale;
            float half=(column-10)/2;
            Put((RectTransform)login.transform,0,0,half,46);Put((RectTransform)guest.transform,half+10,0,half,46);
            Put((RectTransform)server.transform,0,56,column,54);
            Put((RectTransform)enter.transform,0,120,column,62);Put(entryHint.rectTransform,0,192,column,22);
            entryHint.gameObject.SetActive(!compact);
            if(compact)
            {
                Put((RectTransform)server.transform,0,56,column,50);
                Put((RectTransform)enter.transform,(column-Mathf.Min(460,column))/2,118,Mathf.Min(460,column),62);
            }
            float logoW=Mathf.Min(740,w-32),logoY=portrait?Mathf.Min(h*.19f,homeY-205):Mathf.Min(94,homeY-162);
            logoY=Mathf.Max(compact?52:64,logoY);
            Fit(logoGroup,(w-logoW)/2,logoY,logoW,152);logoGroup.localScale=Vector3.one*scale;
            Put(eyebrow.rectTransform,0,0,logoW,24);Put(logo.rectTransform,0,20,logoW,98);
            logo.fontSize=Mathf.RoundToInt(Mathf.Min(90,logoW*.129f));
            Put((RectTransform)logoGroup.Find("Sanctuary sigil"),logoW*.18f,108,logoW*.64f,24);
            Put(tagline.rectTransform,0,135,logoW,24);
            if(compact)
            {
                Put(logo.rectTransform,0,15,logoW,65);logo.fontSize=60;
                Put((RectTransform)logoGroup.Find("Sanctuary sigil"),logoW*.28f,79,logoW*.44f,18);
                tagline.gameObject.SetActive(false);
            }
            else tagline.gameObject.SetActive(true);
            float toolsW=274;Fit(toolbar,w-toolsW-20,12,toolsW,46);toolbar.localScale=Vector3.one*scale;
            if(toolbar.GetComponent<CanvasGroup>()==null)toolbar.gameObject.AddComponent<CanvasGroup>();
            Put((RectTransform)motion.transform,0,0,106,46);Put((RectTransform)language.transform,116,0,102,46);Put((RectTransform)options.transform,228,0,46,46);
            chapter.gameObject.SetActive(w>700);Fit(chapter.rectTransform,28,17,210,38);chapter.rectTransform.localScale=Vector3.one*scale;
            Fit(footline,20,h-27,w-40,20);footline.localScale=Vector3.one*scale;
            Put(footnote.rectTransform,0,0,(w-40)*.65f,20);Put(version.rectTransform,(w-40)*.65f,0,(w-40)*.35f,20);
            ReflowDialog();
        }
        void Fit(RectTransform r,float x,float y,float w,float h)=>Put(r,x*scale,y*scale,w,h);
        static RectTransform Rect(string name,Transform parent)
        {var obj=new GameObject(name,typeof(RectTransform));obj.transform.SetParent(parent,false);return (RectTransform)obj.transform;}
        static void Fill(RectTransform r)
        {r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;}
        static void Put(RectTransform r,float x,float y,float w,float h)
        {r.anchorMin=r.anchorMax=new Vector2(0,1);r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);}
        RectTransform Plate(string name,Transform parent,Color color,bool border=true)
        {
            var r=Rect(name,parent);r.gameObject.AddComponent<Image>().color=color;
            if(border){var frame=Rect("Etched frame",r);Fill(frame);var g=frame.gameObject.AddComponent<TitleFrameGraphic>();g.color=Gold;g.raycastTarget=false;}
            return r;
        }
        Text Caption(Transform parent,string id,string value,int size,Color color,TextAnchor align=TextAnchor.MiddleCenter)
        {
            var r=Rect(id,parent);var t=r.gameObject.AddComponent<Text>();t.font=font;t.text=Loc.T(value);t.fontSize=size;t.color=color;t.supportRichText=false;
            t.alignment=align;t.raycastTarget=false;t.horizontalOverflow=HorizontalWrapMode.Wrap;t.verticalOverflow=VerticalWrapMode.Truncate;return t;
        }
        Button ActionButton(Transform parent,string id,string text,Action action,bool primary=false,int fontSize=16)
        {
            var r=Plate(id,parent,primary?Red:Slate);var button=r.gameObject.AddComponent<UiButton>();button.targetGraphic=r.GetComponent<Image>();

            var label=Caption(r,"Caption",text,fontSize,primary?Bone:Muted);Fill(label.rectTransform);label.rectTransform.offsetMin=new Vector2(12,3);label.rectTransform.offsetMax=new Vector2(-12,-3);
            UiTheme.Button(button,primary);button.onClick.AddListener(()=>{game.Audio?.Play(SoundCue.Select);action();});return button;
        }
        Button GoogleButton(Transform parent,string id,string text,Action action,int fontSize,float read)
        {
            var button=(UiButton)ActionButton(parent,id,text,action,false,fontSize);button.Configure(UiButtonRole.GoogleSignIn);
            button.transform.Find("Etched frame").gameObject.SetActive(false);
            var caption=button.GetComponentInChildren<Text>();caption.font=UiFonts.GoogleSignIn;caption.color=StorageSurface.Hex("1f1f1f");
            caption.rectTransform.offsetMin=new Vector2(40*read,3);caption.rectTransform.offsetMax=new Vector2(-12*read,-3);
            var icon=Rect("Official Google G",button.transform);icon.anchorMin=icon.anchorMax=new Vector2(0,.5f);icon.pivot=new Vector2(0,.5f);
            var texture=Resources.Load<Texture2D>("Authentication/GoogleG");
            icon.anchoredPosition=new Vector2(12*read,0);icon.sizeDelta=new Vector2(18*read,18*read*(texture!=null?(float)texture.height/texture.width:1));
            var graphic=icon.gameObject.AddComponent<RawImage>();graphic.texture=texture;graphic.raycastTarget=false;
            return button;
        }
        static void SetCaption(Button b,string text)=>b.GetComponentInChildren<Text>().text=text;

    }
}
