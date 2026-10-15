using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Hellscript
{
    public sealed partial class CharacterSelectionView:MonoBehaviour
    {
        GameController game;TitleSession session;CharacterSelectionState state;CharacterPlazaSlots provider;CharacterPlazaSlot[] slots;
        RectTransform root,design,worldArea,badge,nameplate,crestRail;Text title,hint,heroName,level,bubble;Button back,start,create,gear;
        Button[] crests;Text[] locks;StorageSurface topFog,bottomFog;Image entryFade;Canvas parentCanvas;
        Rect lastArea;Vector2 lastSize,logical,lastDisplay;float units,minimumHit,lastPixels;bool portrait,compact,entered;int revision=-1;ContentWindowView gearWindow;
        int anchorRevision=-1;bool anchorsDirty=true;StorageSurface[] crestDiscs;
        static readonly int[] KeyboardOrder={0,3,1,4,2,5};
        readonly UiSkin skin=UiSkin.Casual.WithEquipmentArt(CasualEquipmentArt.Provider);
        public CharacterPlazaStage Stage {get;private set;}
        public CharacterSelectionState Selection=>state;
        public Button StartButton=>start;
        public bool BackdropReady=>Stage!=null&&Stage.ArtReady;
        public static Color ClassColor(HeroClass type)=>type==HeroClass.Warrior?UiTheme.D4Token("GoldFF9F1CA1000"):type==HeroClass.Mage?UiTheme.D4Token("Steel35A7FFA1000"):UiSkin.Casual.GreenShade;
        public void Initialize(GameController owner,CharacterSelectionState selection,TitleSession login)
        {
            game=owner;state=selection;session=login;root=(RectTransform)transform;provider=new CharacterPlazaSlots(game.Store);slots=provider.Read();
            parentCanvas=root.GetComponentInParent<Canvas>();root.gameObject.AddComponent<RectMask2D>();
            design=UiLayout.Rect("Plaza design units",root);worldArea=UiLayout.Rect("Hero stage",design);
            Stage=worldArea.gameObject.AddComponent<CharacterPlazaStage>();Stage.Initialize(state,slots);Stage.Choose=Choose;
            topFog=Fog("Upper cream mist",true);bottomFog=Fog("Lower cream mist",false);
            title=Caption(design,"selection-heading","나만의 모험가를 골라요",30,skin.Gold,true);Outline(title,2.5f);
            back=Button(design,"characters-back","‹ 타이틀",Back,14);
            hint=Caption(Panel(design,"Hint pill",13),"selection-hint","캐릭터를 눌러 선택하세요",13,skin.Muted);
            start=Button(design,"character-start","플레이",StartGame,26,true);Outline(start.GetComponentInChildren<Text>(),2);start.GetComponentInChildren<Text>().color=skin.White;
            crestRail=Panel(design,"Class crest rail",3);
            crests=new Button[3];crestDiscs=new StorageSurface[3];var classes=new[]{HeroClass.Mage,HeroClass.Ranger,HeroClass.Warrior};
            for(int i=0;i<crests.Length;i++)
            {
                var type=classes[i];crests[i]=Button(design,"character-focus-"+type,"",()=>{if(!game.UI.CommonPanelOpen&&state.FocusClass(type))Refresh();},14);
                ((UiButton)crests[i]).Face.gameObject.SetActive(false);
                var disc=Panel(crests[i].transform,"Circular crest chrome",100);UiLayout.Stretch(disc);crestDiscs[i]=disc.GetComponent<StorageSurface>();
                var image=UiLayout.Rect("Class crest",crests[i].transform).gameObject.AddComponent<RawImage>();image.texture=Stage.Crest(type);image.raycastTarget=false;UiLayout.Stretch(image.rectTransform);image.rectTransform.offsetMin=new Vector2(4,6);image.rectTransform.offsetMax=new Vector2(-4,-4);
            }
            nameplate=Panel(design,"Selected hero nameplate",12);heroName=Caption(nameplate,"selected-hero-name","",14,skin.Navy,true);
            badge=UiLayout.Rect("Selected hero badge",design);level=Caption(Panel(badge,"Level pill",15),"selected-hero-level","",16,skin.Navy,true);
            create=Button(badge,"character-create","+",Create,26,true);gear=Button(badge,"character-equipment","",OpenEquipment,16);
            var person=UiLayout.Rect("Equipment person glyph",gear.transform).gameObject.AddComponent<CharacterPlazaPerson>();person.raycastTarget=false;person.color=skin.Navy;UiLayout.Stretch(person.rectTransform);person.rectTransform.offsetMin=new Vector2(12,10);person.rectTransform.offsetMax=new Vector2(-12,-10);
            bubble=Caption(Panel(badge,"Hero speech",16),"hero-speech","",15,skin.Navy,true);
            locks=new Text[slots.Length];for(int i=0;i<locks.Length;i++){var panel=Panel(design,"Locked hero "+i,12);locks[i]=Caption(panel,"Rift lock","",12,skin.Muted);EquipmentSlotView.Glyph(panel,"Rift lock symbol","lock",5,4,14,skin.Muted);}
            entryFade=UiLayout.Rect("Entry cream fade",design).gameObject.AddComponent<Image>();entryFade.color=Color.clear;entryFade.raycastTarget=false;
            Reflow();Refresh();
        }
        public Button Find(string id){foreach(var b in GetComponentsInChildren<Button>())if(b.name==id)return b;return null;}
        public void Choose(int index)
        {
            if(session.Entering||game.UI.CommonPanelOpen||!provider.Choose(state,index))return;
            game.Audio?.Play("char.select."+GameAudio.ClassKey(slots[index].Class));Refresh();
        }
        void Back(){if(state.Busy||session.Entering)return;if(gearWindow!=null){gearWindow.Close();return;}game.UI.ShowTitle();}
        void Create()
        {if(session.Entering||game.UI.CommonPanelOpen||!provider.BeginCreation(state))return;Refresh();}
        void StartGame()
        {
            if(state.Busy||state.Phase!=CharacterPlazaPhase.Selected||game.UI.CommonPanelOpen||!session.SignedIn||!slots[state.Selected].Created)return;
            if(!game.CommitEntryCharacter(state.Selected))return;
            if(!provider.BeginEntry(state)||!session.BeginEntry())return;
            game.Audio?.Play("flow.game_start");Refresh();
        }
        void Refresh()
        {
            anchorsDirty=true;
            revision=state.Revision;slots=provider.Read();Stage.SetSlots(slots);bool selected=state.Selected>=0;
            var slot=selected?slots[state.Selected]:null;bool busy=state.Busy,equipment=state.Phase==CharacterPlazaPhase.Equipment;
            nameplate.gameObject.SetActive(selected&&!equipment);badge.gameObject.SetActive(selected&&!equipment);
            start.gameObject.SetActive(selected&&slot.Created&&state.Phase!=CharacterPlazaPhase.Creating);
            start.interactable=!busy;start.GetComponentInChildren<Text>().text=Loc.T(state.Phase==CharacterPlazaPhase.Entering?"입장 중…":"플레이");
            back.interactable=!busy;foreach(var crest in crests)crest.interactable=!busy&&!equipment;
            create.gameObject.SetActive(selected&&!slot.Created&&!busy);gear.gameObject.SetActive(selected&&slot.Created&&!busy);level.transform.parent.gameObject.SetActive(selected&&slot.Created&&!busy);
            bubble.transform.parent.gameObject.SetActive(busy);
            if(selected)
            {
                heroName.text=Loc.T(game.catalog.classNames[(int)slot.Class])+" "+(slot.Gender==HeroGender.Female?"♀":"♂");heroName.font=skin.LabelFont(heroName.text);heroName.color=skin.OnLight(ClassColor(slot.Class));
                level.text="Lv."+slot.Level;bubble.text=Loc.T(CharacterSelectionState.Line(slot.Class,slot.Gender,state.Phase==CharacterPlazaPhase.Entering));bubble.color=skin.OnLight(ClassColor(slot.Class));
            }
            bool locked=Array.Exists(slots,s=>s.Locked);
            hint.text=Loc.T(state.Phase==CharacterPlazaPhase.Entering?"성소로 향하는 중…":locked?"진행 중인 균열의 캐릭터로 이어갑니다.":!selected?"캐릭터를 눌러 선택하세요":!slot.Created?"+를 눌러 새 모험가를 만들어요":"선택한 캐릭터로 성소에 입장합니다.");
            hint.transform.parent.gameObject.SetActive(!equipment&&state.Phase!=CharacterPlazaPhase.Creating&&(!start.gameObject.activeSelf||!portrait&&!compact));
            for(int i=0;i<locks.Length;i++)locks[i].transform.parent.gameObject.SetActive(slots[i].Locked);
            LayoutCrests();
            LayoutAnchors();
        }
        void Update()
        {
            Reflow();float dt=Mathf.Min(Time.unscaledDeltaTime,.1f);state.Tick(dt);
            if(state.CreationReady)
            {
                bool saved=provider.Create(state.Selected);state.FinishCreation();if(!saved)game.UI.ShowToast(game.Store.Error);Refresh();
            }
            if(revision!=state.Revision)Refresh();Stage.Tick(dt);LayoutAnchors();
            if(state.Phase==CharacterPlazaPhase.Entering)
            {
                var tint=skin.Cream;tint.a=.32f+.68f*Mathf.Clamp01((state.Age-1.8f)/.65f);entryFade.color=tint;
                if(!entered&&state.Age>=2.45f){entered=true;game.EnterPlaza(true);}return;
            }
            if(state.Busy||game.UI.CommonPanelOpen)return;
            var keyboard=Keyboard.current;if(keyboard==null)return;
            if(keyboard.escapeKey.wasPressedThisFrame&&!ContentWindowHost.EscapeHandledThisFrame){Back();return;}
            int direction=keyboard.rightArrowKey.wasPressedThisFrame?1:keyboard.leftArrowKey.wasPressedThisFrame?-1:0;
            if(direction!=0)
            {
                int at=Array.IndexOf(KeyboardOrder,state.Selected);if(at<0)at=direction>0?-1:0;
                for(int n=1;n<=KeyboardOrder.Length;n++){int index=KeyboardOrder[(at+direction*n+KeyboardOrder.Length*2)%KeyboardOrder.Length];if(index<slots.Length&&!slots[index].Locked){Choose(index);break;}}
            }
            if(keyboard.enterKey.wasPressedThisFrame&&EventSystem.current?.currentSelectedGameObject==null&&state.Selected>=0)StartGame();
        }
        internal static float PixelScale(Rect area)
        {
            bool vertical=area.height>area.width;
            return Mathf.Max(.01f,vertical?area.width/440:area.height<650?area.height/440:Mathf.Min(area.width/1152,area.height/648));
        }
        void Reflow()
        {
            var area=UiSafeArea.Current;var display=new Vector2(Screen.width,Screen.height);float pixels=PixelScale(area);var size=area.size/pixels;
            if(size==lastSize&&pixels==lastPixels&&area==lastArea&&display==lastDisplay)return;
            lastArea=area;lastSize=size;lastPixels=pixels;lastDisplay=display;portrait=area.height>area.width;compact=!portrait&&area.height<650;
            parentCanvas.GetComponent<CanvasScaler>().scaleFactor=pixels;parentCanvas.scaleFactor=pixels;
            root.anchorMin=new Vector2(area.xMin/Screen.width,area.yMin/Screen.height);root.anchorMax=new Vector2(area.xMax/Screen.width,area.yMax/Screen.height);
            root.offsetMin=root.offsetMax=Vector2.zero;
            units=1;logical=size;UiLayout.Place(design,0,0,logical.x,logical.y);design.localScale=Vector3.one;
            minimumHit=44/Mathf.Max(.01f,units*pixels);
            UiLayout.Place(worldArea,0,0,logical.x,logical.y);Stage.Layout(logical,portrait);
            float w=logical.x,h=logical.y;
            Place(back,portrait?16:compact?16:24,portrait?Mathf.Min(56,h*.06f):12,100,44);
            UiLayout.Place(title.rectTransform,portrait?16:compact?120:0,portrait?Mathf.Min(108,h*.113f):compact?8:56,portrait?w-32:compact?w-240:w,portrait?44:46);title.fontSize=portrait?30:compact?26:38;
            Place(start,(w-(portrait?220:compact?200:180))/2,h-(portrait?88:compact?52:78),portrait?220:compact?200:180,portrait?50:46);
            var hintRect=(RectTransform)hint.transform.parent;UiLayout.Place(hintRect,(w-Mathf.Min(w-40,400))/2,h-(portrait?76:compact?28:27),Mathf.Min(w-40,400),26);Fill(hint);
            LayoutCrests();
            UiLayout.Place(bottomFog.rectTransform,0,h-110,w,110);bottomFog.gameObject.SetActive(portrait);
            UiLayout.Place(entryFade.rectTransform,0,0,w,h);
            // Preserve 44 screen points even when fitting a short safe area.
            ExpandHit(back,minimumHit);ExpandHit(start,minimumHit);
            if(revision>=0)Refresh();
        }
        void LayoutAnchors()
        {
            if(Stage==null||logical.x<=0)return;
            bool bouncing=state.Selected>=0&&!slots[state.Selected].Created&&!state.Busy;
            if(!anchorsDirty&&anchorRevision==Stage.LayoutRevision&&!bouncing)return;
            anchorsDirty=false;anchorRevision=Stage.LayoutRevision;
            UiLayout.Place(topFog.rectTransform,0,0,logical.x,Stage.TopFog);
            for(int i=0;i<locks.Length;i++){var p=Stage.Head(i);UiLayout.Place((RectTransform)locks[i].transform.parent,p.x-12,p.y-30,24,24);}
            if(state.Selected<0)return;
            var head=Stage.Head(state.Selected);var feet=Stage.Feet(state.Selected);float bh=state.Busy?(compact?52:60):44;
            float max=portrait?logical.x-188:logical.x-118,min=portrait?118:compact?112:118;
            float cx=Mathf.Clamp(head.x,Mathf.Min(min,max),Mathf.Max(min,max));float y=Mathf.Clamp(head.y-bh-8,portrait?160:compact?60:108,logical.y-bh-104);
            UiLayout.Place(badge,cx-130,y,260,bh);
            float tagHeight=compact?22:24;
            UiLayout.Place(nameplate,Mathf.Clamp(feet.x-(compact?48:54),4,logical.x-(compact?100:112)),Mathf.Clamp(feet.y+4,0,logical.y-tagHeight-4),compact?96:108,tagHeight);Fill(heroName);
            float hit=Mathf.Max(44,minimumHit);
            Place(create,130-hit/2,(44-hit)/2,hit,hit);float visible=compact?32:38;var face=((UiButton)create).Face.rectTransform;UiLayout.Place(face,(hit-visible)/2,(hit-visible)/2,visible,visible);
            float bounce=!state.Busy&&!slots[state.Selected].Created?Mathf.Sin(Stage.Clock*Mathf.PI*2/1.2f)*3:0;face.anchoredPosition+=Vector2.up*bounce;
            UiLayout.Place((RectTransform)level.transform.parent,78,7,70,compact?26:30);Fill(level);Place(gear,176-hit/2,(44-hit)/2,hit,hit);
            var gearFace=((UiButton)gear).Face.rectTransform;visible=compact?28:32;UiLayout.Place(gearFace,(hit-visible)/2,(hit-visible)/2,visible,visible);
            UiLayout.Place((RectTransform)bubble.transform.parent,(260-(compact?200:210))/2,0,compact?200:210,bh);Fill(bubble,10,4);
        }
        void LayoutCrests()
        {
            crestRail.gameObject.SetActive(portrait);UiLayout.Place(crestRail,logical.x-49.5f,logical.y*.5f-111.5f,6,224);
            for(int i=0;i<crests.Length;i++)
            {
                bool chosen=(int)state.Focus==2-i;float size=chosen?73:65;
                crests[i].gameObject.SetActive(portrait);Place(crests[i],logical.x-79-(chosen?4:0),logical.y*.5f-144+112*i-(chosen?4:0),size,size);
                ((UiButton)crests[i]).Configure(UiButtonRole.Icon,chosen);
                crestDiscs[i].SetSkin(skin,chosen?ClassColor((HeroClass)(2-i)):skin.Navy,radius:100,borderWidth:chosen?4:3,shadow:3);
            }
        }
        static void Place(Button b,float x,float y,float w,float h)=>UiLayout.Place((RectTransform)b.transform,x,y,w,h);
        static void Fill(Text t,float x=4,float y=0){UiLayout.Stretch(t.rectTransform);t.rectTransform.offsetMin=new Vector2(x,y);t.rectTransform.offsetMax=new Vector2(-x,-y);}
        static void ExpandHit(Button b,float minimum){var r=(RectTransform)b.transform;r.sizeDelta=new Vector2(Mathf.Max(r.sizeDelta.x,minimum),Mathf.Max(r.sizeDelta.y,minimum));}
        RectTransform Panel(Transform parent,string name,float radius)
        {var r=UiLayout.Rect(name,parent);var surface=r.gameObject.AddComponent<StorageSurface>();surface.SetSkin(skin,radius:radius,borderWidth:2.5f,shadow:3);surface.raycastTarget=false;return r;}
        Text Caption(Transform parent,string name,string value,int size,Color color,bool display=false)
        {var t=UiLayout.Text(parent,name,Loc.T(value),0,0,1,1,size,color,display?skin.LabelFont(Loc.T(value)):skin.BodyFont);t.alignment=TextAnchor.MiddleCenter;t.supportRichText=false;return t;}
        Button Button(Transform parent,string name,string label,Action action,int size,bool primary=false)
        {
            var r=UiLayout.Rect(name,parent);var hit=r.gameObject.AddComponent<Image>();hit.color=Color.clear;var b=r.gameObject.AddComponent<UiButton>();b.targetGraphic=hit;UiTheme.Button(b,primary);skin.ApplyChrome(b);
            var text=Caption(r,"Caption",label,size,skin.Navy,true);Fill(text,5,2);b.onClick.AddListener(()=>action());return b;
        }
        StorageSurface Fog(string name,bool top)
        {var r=UiLayout.Rect(name,design);var f=r.gameObject.AddComponent<StorageSurface>();var clear=skin.Cream;clear.a=0;var solid=skin.Cream;solid.a=.94f;f.top=top?solid:clear;f.bottom=top?clear:solid;f.raycastTarget=false;return f;}
        static void Outline(Text text,float width)
        {
            var values=new D4Shadow[8];int i=0;foreach(var p in new[]{new Vector2(1,0),new Vector2(-1,0),new Vector2(0,1),new Vector2(0,-1),new Vector2(.75f,.75f),new Vector2(-.75f,.75f),new Vector2(.75f,-.75f),new Vector2(-.75f,-.75f)})values[i++]=new D4Shadow{x=p.x*width,y=p.y*width,color="Steel2A3150A1000"};
            text.gameObject.AddComponent<SkillTreeD4TextShadow>().Apply(values,1);
        }
        void OnDestroy(){if(gearWindow!=null)gearWindow.Close();}
    }
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class CharacterPlazaPerson:MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper vh)
        {vh.Clear();var r=rectTransform.rect;UiMesh.RoundedBox(vh,new Rect(r.x+r.width*.25f,r.y+r.height*.53f,r.width*.5f,r.height*.43f),r.width*.25f,color,color);UiMesh.RoundedBox(vh,new Rect(r.x,r.y,r.width,r.height*.47f),r.width*.25f,color,color);}
    }
}
