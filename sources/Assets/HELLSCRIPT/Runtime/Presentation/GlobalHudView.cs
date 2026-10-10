using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    public sealed class GlobalHudView:MonoBehaviour
    {
        public GlobalHudLayout Layout {get;private set;}
        public GlobalHudSnapshot Snapshot {get;private set;}
        public StatusStripView StatusStrip {get;private set;}
        public Action<HudEffectState> EffectSelected;
        public Action<HudSlotState> SlotSelected;
        public Action AllEffectsSelected;
        public Action EdictSelected,PotionSettingsSelected;
        public Action<int> SkillTapped,SkillHeld,PotionTapped,PotionHeld;
        readonly Dictionary<string,Sprite> sprites=new Dictionary<string,Sprite>();
        readonly Dictionary<int,Sprite> atlasSprites=new Dictionary<int,Sprite>();
        readonly List<Sprite> owned=new List<Sprite>();
        readonly List<Slot> slots=new List<Slot>();
        RectTransform safe,seal,level,hp,resource,xp,edict,potionSettings;
        Image edictEmblem;
        Image face,hpFill,resourceFill,xpFill,sealFrame,shieldLine;
        Text levelLabel,hpLabel,resourceLabel,shieldLabel,xpLabel;
        readonly List<RectTransform> ticks=new List<RectTransform>();
        Font font;Texture2D atlas;GlobalHudStyle style;
        Rect previousSafe;float factor=-1;bool ready,contentLayout;
        public GlobalHudStyle Style=>style;
        public float BottomInset {get;private set;}
        Color Gold=>style.gold;Color Pale=>style.pale;
        sealed class Slot
        {public RectTransform root;public Image icon,cover,frame,glyph;public SkillIconView skillView;public Text caption,number;public HudSlotState data;public bool potion;}
        public void Initialize(Font textFont,Texture2D skillAtlas)
        {
            font=textFont;atlas=skillAtlas;style=GlobalHudStyle.Load();
            var canvas=gameObject.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=105;
            gameObject.AddComponent<GraphicRaycaster>();
            safe=Rect("HUD safe area",transform);
            seal=CreateSeal(safe,"Class seal",out face,out sealFrame);
            edict=Rect("Hunt edict",safe);edictEmblem=Picture("Emblem",edict,"menu-hunt-edict");Stretch(edictEmblem.rectTransform);edictEmblem.preserveAspect=true;
            var edictHit=edict.gameObject.AddComponent<Image>();edictHit.color=Color.clear;var edictButton=edict.gameObject.AddComponent<Button>();edictButton.targetGraphic=edictHit;edictButton.transition=Selectable.Transition.None;
            edictButton.onClick.AddListener(()=>EdictSelected?.Invoke());edict.gameObject.SetActive(false);
            // A painted push-button (potion and settings cog), not a potion slot.
            potionSettings=Rect("Threshold potion button",safe);
            var settingsIcon=Picture("Icon",potionSettings,"menu-potion-settings");Stretch(settingsIcon.rectTransform);settingsIcon.preserveAspect=true;
            var settingsHit=potionSettings.gameObject.AddComponent<Image>();settingsHit.color=Color.clear;var settingsButton=potionSettings.gameObject.AddComponent<Button>();settingsButton.targetGraphic=settingsHit;settingsButton.transition=Selectable.Transition.None;
            settingsButton.onClick.AddListener(()=>PotionSettingsSelected?.Invoke());potionSettings.gameObject.SetActive(false);
            level=Rect("Level",safe);Stretch(Picture("Level badge",level,"badge-level").rectTransform);levelLabel=Text("Level text",level,20);Stretch(levelLabel.rectTransform);
            hp=Vital("HP",style.hpColor,out hpFill,out hpLabel);
            resource=Vital("Resource",style.resourceColor,out resourceFill,out resourceLabel);
            shieldLabel=Text("Shield",safe,12);shieldLabel.alignment=TextAnchor.MiddleRight;
            shieldLine=Picture("Shield line",safe,"fill-white");shieldLine.color=style.shieldColor;shieldLine.type=Image.Type.Filled;shieldLine.fillMethod=Image.FillMethod.Horizontal;
            xp=Rect("Experience",safe);var track=Picture("Track",xp,"fill-white");Stretch(track.rectTransform);track.color=style.xpTrack;
            xpFill=Picture("Progress",xp,"fill-white");Stretch(xpFill.rectTransform);xpFill.color=style.xpColor;xpFill.type=Image.Type.Filled;xpFill.fillMethod=Image.FillMethod.Horizontal;
            for(int i=1;i<10;i++){var t=Picture("XP "+i*10+"%",xp,i==5?"xp-tick-major":"xp-tick");ticks.Add(t.rectTransform);}
            xpLabel=Text("XP text",safe,16);xpLabel.alignment=TextAnchor.MiddleLeft;xpLabel.color=Gold;
            for(int i=0;i<3;i++)slots.Add(CreateSlot("Potion "+i,true,i));
            for(int i=0;i<4;i++)slots.Add(CreateSlot("Active "+i,false,i));
            slots.Add(CreateSlot("Ultimate",false,4));
            var strip=Rect("Status strip",safe);StatusStrip=strip.gameObject.AddComponent<StatusStripView>();
            StatusStrip.Initialize(this,font,e=>EffectSelected?.Invoke(e),()=>AllEffectsSelected?.Invoke());ready=true;
        }
        RectTransform Vital(string name,Color color,out Image fill,out Text label)
        {
            var r=Rect(name,safe);var plate=Picture("Track",r,"fill-vital");Stretch(plate.rectTransform);plate.color=style.vitalTrack;plate.type=Image.Type.Sliced;
            fill=Picture("Fill",r,"fill-vital");Stretch(fill.rectTransform);fill.color=color;fill.type=Image.Type.Filled;fill.fillMethod=Image.FillMethod.Horizontal;
            var border=Picture("Border",r,"frame-vital");Stretch(border.rectTransform);border.type=Image.Type.Sliced;
            label=Text(name+" value",r,16);Stretch(label.rectTransform);label.alignment=TextAnchor.MiddleLeft;return r;
        }
        Slot CreateSlot(string name,bool potion,int index=-1)
        {
            var slot=new Slot{root=Rect(name,safe),potion=potion};
            if(!potion)
            {
                slot.skillView=SkillIconView.Create(slot.root,false);Stretch(slot.skillView.Rect);
                slot.icon=slot.skillView.Icon;slot.cover=slot.skillView.Cover;slot.frame=slot.skillView.Frame;
                slot.skillView.Plate.color=style.slotPlate;slot.cover.color=style.cooldownTint;
            }
            else
            {
                var plate=Picture("Plate",slot.root,"mask-square");Stretch(plate.rectTransform);plate.color=style.slotPlate;
                // Image.preserveAspect aligns unused width to the RectTransform pivot.
                slot.icon=Picture("Icon",slot.root,"");slot.icon.rectTransform.pivot=Vector2.one*.5f;Stretch(slot.icon.rectTransform);slot.icon.preserveAspect=true;
                slot.cover=Picture("Cooldown",slot.root,"mask-square");slot.cover.rectTransform.pivot=Vector2.one*.5f;Stretch(slot.cover.rectTransform);slot.cover.color=style.cooldownTint;
                slot.cover.type=Image.Type.Filled;slot.cover.fillMethod=Image.FillMethod.Radial360;slot.cover.fillOrigin=2;slot.cover.fillClockwise=false;slot.cover.preserveAspect=true;
                slot.frame=Picture("Frame",slot.root,"frame-active");Stretch(slot.frame.rectTransform);
            }
            slot.glyph=Picture("Potion kind",slot.root,"");slot.glyph.enabled=potion;
            slot.number=Text("Cooldown time",slot.root,25);Stretch(slot.number.rectTransform);
            slot.caption=Text("Caption",slot.root,16);
            var target=slot.root.gameObject.AddComponent<Image>();target.color=Color.clear;
            // A slot is a tap (swap list) or a hold (details); the gauge rings the icon while the hold fills.
            var gauge=Picture("Hold gauge",slot.root,"mask-circle");Stretch(gauge.rectTransform);gauge.color=new Color(style.gold.r,style.gold.g,style.gold.b,.6f);
            gauge.type=Image.Type.Filled;gauge.fillMethod=Image.FillMethod.Radial360;gauge.fillOrigin=2;gauge.fillClockwise=true;gauge.fillAmount=0;gauge.enabled=false;
            var hold=slot.root.gameObject.AddComponent<HoldPress>();hold.gauge=gauge;
            hold.Tap=()=>{if(slot.data!=null)(potion?PotionTapped:SkillTapped)?.Invoke(index);};hold.Hold=()=>{if(slot.data!=null)(potion?PotionHeld:SkillHeld)?.Invoke(index);};
            return slot;
        }
        public void SetSnapshot(GlobalHudSnapshot data,float interfaceFactor,bool fitContent=false,float bottomInset=0)
        {
            if(!ready||data==null)return;
            if(Snapshot?.heroId!=data.heroId||Snapshot?.sessionId!=data.sessionId)StatusStrip.ResetPosition();
            Snapshot=data;
            Rect bounds=UiSafeArea.Current;
            // Translate the complete HUD above the live journal without changing its scale or composition.
            BottomInset=Mathf.Max(0,bottomInset);
            SetRect(safe,new Rect(bounds.x,bounds.y+BottomInset,bounds.width,bounds.height));
            if(Layout==null||bounds!=previousSafe||factor!=interfaceFactor||contentLayout!=fitContent)
            {factor=interfaceFactor;contentLayout=fitContent;previousSafe=bounds;float fitted=fitContent?GlobalHudLayout.ContentFactor(bounds.width,bounds.height,factor,style):factor;Layout=new GlobalHudLayout(bounds.width,bounds.height,fitted,style);Reflow();}
            face.sprite=Portrait(data.heroClass);
            levelLabel.text="Lv."+data.level;hpFill.fillAmount=Ratio(data.health,data.maxHealth);resourceFill.fillAmount=Ratio(data.resource,data.maxResource);
            hpLabel.text=data.maxHealth>0?Loc.F("  HP  {0:0} / {1:0}",Mathf.Max(0,data.health),data.maxHealth):"  HP  —";
            if(data.dead)hpLabel.text+=" · "+Loc.T("사망");
            if(!data.combat)hpLabel.text=Loc.F("  HP  — / {0:0} · 전투 전",data.maxHealth);
            resourceLabel.text=data.maxResource>0?Loc.F("  {0}  {1:0} / {2:0}",Loc.T(data.resourceName),data.resource,data.maxResource):Loc.T(data.resourceName)+"  —";
            if(!data.combat)resourceLabel.text=Loc.F("  {0}  — / {1:0}",Loc.T(data.resourceName),data.maxResource);
            shieldLabel.text=data.shield>0?Loc.F("보호막 +{0:0}",data.shield):"";
            shieldLine.fillAmount=Ratio(data.shield,data.maxHealth);shieldLine.enabled=data.shield>0;
            xpFill.fillAmount=data.ExperienceRatio;xpLabel.text=data.maximumLevel?Loc.T("최대 레벨"):Loc.F("EXP  {0:0}%",Mathf.FloorToInt(100*xpFill.fillAmount));
            sealFrame.color=data.dead?style.deadTint:Color.white;
            for(int i=0;i<slots.Count;i++)UpdateSlot(slots[i],i<3?data.potions[i]:i<7?data.actives[i-3]:data.ultimate);
            StatusStrip.SetEffects(data.ObservationEffects());
        }
        public RectTransform EdictButton=>edict;
        public void SetEdictButton(bool visible){if(edict.gameObject.activeSelf!=visible)edict.gameObject.SetActive(visible);}
        // Slot 0-3 are the four actives, 4 the ultimate.
        public RectTransform SkillSlot(int index)=>slots[3+index].root;
        public HudSlotState SkillState(int index)=>slots[3+index].data;
        public RectTransform PotionSlot(int index)=>slots[index].root;
        public HudSlotState PotionState(int index)=>slots[index].data;
        public RectTransform PotionButton=>potionSettings;
        public void SetPotionButton(bool visible){if(potionSettings.gameObject.activeSelf!=visible)potionSettings.gameObject.SetActive(visible);}
        // The button dims while its settings are fanned out, so it reads as the thing being edited.
        public void SetEdictOpen(bool open)=>edictEmblem.color=open?style.gold:Color.white;
        static float Ratio(float value,float maximum)=>maximum>0?Mathf.Clamp01(value/maximum):0;
        void UpdateSlot(Slot slot,HudSlotState data)
        {
            slot.data=data;if(data==null){slot.root.gameObject.SetActive(false);return;}slot.root.gameObject.SetActive(true);
            slot.icon.sprite=!slot.potion&&SkillIconAssets.TryResolve(data.id,out _,out var skillSprite)?skillSprite:!slot.potion&&data.atlas>=0?SkillIconAssets.Active(data.atlas):Sprite(data.icon);slot.icon.enabled=slot.icon.sprite!=null;
            slot.icon.color=data.locked||data.empty||slot.potion&&data.count==0?style.disabledTint:Color.white;
            slot.frame.enabled=true;slot.frame.color=data.active?style.activeTint:Color.white;
            slot.cover.fillAmount=data.total>0?Mathf.Clamp01(data.remaining/data.total):0;
            slot.number.text=data.locked?"—":GlobalHudSnapshot.TimeLabel(data.remaining);
            if(!slot.potion&&data.insufficient&&data.remaining<=0)slot.number.text="◇";
            slot.number.color=data.insufficient?style.resourceWarning:Pale;
            slot.caption.text=slot.potion?(data.empty?Loc.T("빈칸"):data.count<0?"∞":"×"+data.count):data.caption;
            if(slot.potion&&data.id.StartsWith("PU",StringComparison.Ordinal)){slot.glyph.sprite=Sprite(PotionCatalog.Get(data.id).icon);slot.glyph.enabled=true;}else slot.glyph.enabled=false;
        }
        void Reflow()
        {
            void Place(RectTransform t,Rect r)=>SetRect(t,Layout.Pixels(r));
            Place(seal,Layout.seal);Place(edict,Layout.edict);Place(potionSettings,Layout.potionSettings);Place(level,Layout.level);Place(hp,Layout.hp);Place(resource,Layout.resource);Place(xp,Layout.xp);Place(xpLabel.rectTransform,Layout.xpText);
            Place(shieldLabel.rectTransform,Layout.shield);Place(shieldLine.rectTransform,Layout.shieldLine);
            for(int i=0;i<slots.Count;i++)
            {
                var s=slots[i];var r=i<3?Layout.potions[i]:i<7?Layout.actives[i-3]:Layout.ultimate;Place(s.root,r);
                if(s.potion)
                {
                    var icon=s.icon.rectTransform;icon.anchorMin=icon.anchorMax=icon.pivot=Vector2.one*.5f;
                    icon.anchoredPosition=Vector2.zero;icon.sizeDelta=Vector2.one*(style.landscapePotion*Layout.scale);
                }
                SetRect(s.caption.rectTransform,new Rect(0,-style.captionHeight*Layout.scale,r.width*Layout.scale,style.captionHeight*Layout.scale));
                float gs=style.potionGlyph*Layout.scale;SetRect(s.glyph.rectTransform,new Rect(6*Layout.scale,6*Layout.scale,gs,gs));
                s.number.fontSize=FontSize(s.potion?style.potionCooldownFont:style.cooldownFont);s.caption.fontSize=FontSize(style.captionFont,(int)style.minimumFont);
            }
            hpLabel.fontSize=resourceLabel.fontSize=FontSize(style.valueFont,(int)style.minimumValueFont);levelLabel.fontSize=FontSize(style.levelFont,(int)style.minimumLevelFont);xpLabel.fontSize=FontSize(style.captionFont,(int)style.minimumFont);shieldLabel.fontSize=FontSize(style.shieldFont,(int)style.minimumFont);
            for(int i=0;i<ticks.Count;i++)
            {
                var t=ticks[i];float h=(i==4?style.xpMajorTickHeight:style.xpTickHeight)*Layout.scale;
                SetRect(t,new Rect((i+1)/10f*xp.rect.width-Layout.scale,(-h+xp.rect.height)*.5f,style.xpTickWidth*Layout.scale,h));
            }
            StatusStrip.Configure(Layout,style);
        }
        public int FontSize(float basis,int minimum=11)=>Layout!=null?Layout.FontSize(basis,minimum):Mathf.Max(minimum,Mathf.RoundToInt(basis));
        public Sprite Portrait(HeroClass heroClass)=>heroClass==HeroClass.Mage?Sprite("portrait-mage"):Atlas(18+(int)heroClass);
        public RectTransform CreateSeal(Transform parent,string name,out Image portrait,out Image frame)
        {
            var root=Rect(name,parent);var clipping=Picture("Circle",root,"mask-circle");Stretch(clipping.rectTransform);
            clipping.gameObject.AddComponent<Mask>().showMaskGraphic=false;
            portrait=Picture("Selected class",clipping.transform,"portrait-mage");Stretch(portrait.rectTransform);
            frame=Picture("Seal frame",root,"frame-seal");Stretch(frame.rectTransform);return root;
        }
        public Sprite Sprite(string id)
        {
            if(string.IsNullOrEmpty(id))return null;
            if(id=="potion-hp"||id=="potion-mp"||id=="potion-utility"||id.StartsWith("elixir-",StringComparison.Ordinal))return PotionArt.Bottle(id);
            if(!sprites.TryGetValue(id,out var value))
            {
                value=Resources.Load<Sprite>("Art/GlobalHUD/"+id);
                sprites[id]=value;
            }
            return value;
        }
        Sprite Atlas(int index)
        {
            if(atlas==null||index<0||index>23)return null;
            if(!atlasSprites.TryGetValue(index,out var sprite))
            {float w=atlas.width/6f,h=atlas.height/4f;sprite=UnityEngine.Sprite.Create(atlas,new Rect(index%6*w,atlas.height-(index/6+1)*h,w,h),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);atlasSprites[index]=sprite;owned.Add(sprite);}return sprite;
        }
        public Image Picture(string name,Transform parent,string id)
        {var r=Rect(name,parent);var image=r.gameObject.AddComponent<Image>();image.sprite=Sprite(id);image.raycastTarget=false;return image;}
        public Text Text(string name,Transform parent,int size)
        {
            var r=Rect(name,parent);var t=r.gameObject.AddComponent<Text>();t.font=font;t.fontSize=size;t.color=Pale;t.alignment=TextAnchor.MiddleCenter;
            t.horizontalOverflow=HorizontalWrapMode.Overflow;t.verticalOverflow=VerticalWrapMode.Overflow;t.raycastTarget=false;
            var shadow=r.gameObject.AddComponent<Shadow>();shadow.effectColor=style.textShadow;shadow.effectDistance=new Vector2(1,-1);return t;
        }
        public static RectTransform Rect(string name,Transform parent)
        {var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=r.anchorMax=r.pivot=Vector2.zero;return r;}
        public static void Stretch(RectTransform r)
        {r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;}
        public static void SetRect(RectTransform r,Rect area)
        {r.anchorMin=r.anchorMax=r.pivot=Vector2.zero;r.anchoredPosition=area.position;r.sizeDelta=area.size;}
        void OnDestroy(){foreach(var sprite in owned)if(sprite!=null)Destroy(sprite);}
    }
}
