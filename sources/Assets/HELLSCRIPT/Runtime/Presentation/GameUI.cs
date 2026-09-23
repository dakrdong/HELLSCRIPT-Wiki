using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Hellscript
{
    public sealed partial class GameUI : MonoBehaviour
    {
        public string Page {get;private set;}="town";
        GameController game;
        RectTransform root,shell,headerApron,footerApron,content,header,footer,overlay,toastFrame;
        Font font;
        Texture2D background,atlas,equipmentAtlas;
        Text status,hpText,resourceText,timerText,actionText,meterText,toast,headerTitle,headerSubtitle;
        Image hpFill,resourceFill,meterFill;
        List<Text> skillLabels=new List<Text>();
        float hudClock,toastTime;
        BuildConfig editing;
        bool buildWasPaused;
        // The page that is on screen redraws itself after a language change; the pages that need an
        // argument to be drawn keep it in the delegate instead of in another field.
        Action pageRepaint;
        bool RiftLoadoutLocked=>game.Active&&game.Combat.State.training<0;
        int selectedSlot;
        bool portalBag;
        const float TouchHeight=80;
        readonly Color ink=UiTheme.Background,panel=UiTheme.Panel,gold=UiTheme.Gold,pale=UiTheme.Text,muted=UiTheme.Muted;
        ContentWindowHost windowHost;long shownStoreRevision;
        // Shared with the tests that measure real line heights against the rectangles the layout hands out.
        public static Font CreateFont()=>UiFonts.Body;
        public void Initialize(GameController controller)
        {
            game=controller;
            windowHost=gameObject.AddComponent<ContentWindowHost>();windowHost.Initialize(()=>game.Active?game.Combat.State:null);
            font=CreateFont();
            background=Resources.Load<Texture2D>("Art/Sanctuary");atlas=Resources.Load<Texture2D>("Art/SkillAtlas");
            equipmentAtlas=Resources.Load<Texture2D>("Art/EquipmentAtlas");
            var canvasObject=new GameObject("HELLSCRIPT UI",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform,false);var canvas=canvasObject.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=100;canvas.pixelPerfect=true;
            ApplyScaler(canvasObject.GetComponent<CanvasScaler>(),false);
            shell=Rect("Display",canvasObject.transform);Stretch(shell);
            root=Rect("Safe area",canvasObject.transform);Stretch(root);ApplySafeArea();
            if(FindAnyObjectByType<EventSystem>()==null){var es=new GameObject("UI Input",typeof(EventSystem),typeof(InputSystemUIInputModule));es.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();}
            InitializeGlobalHud();
            if(game.Store==null)
            {Base("recovery","저장 복구가 필요합니다","진행 기록을 덮어쓰지 않고 게임을 멈췄습니다");Note(content,game.Notice,22,360,pale);FooterButton(0,1,"게임 종료",()=>Application.Quit());}
            else ShowTitle();
        }
        // Controls stay inside the usable area while the backdrop keeps covering the display, and the
        // header and footer colours continue into the notch and home bar bands instead of leaving them black.
        void ApplySafeArea()
        {
            UpdateAspectMask();
            Rect s=UiSafeArea.Current;
            if(ReserveGlobalHudSpace&&globalHud?.Layout!=null)
            {float inset=globalHud.Layout.occupiedHeight*globalHud.Layout.scale+12;s.yMin+=inset;}
            float w=Mathf.Max(1,Screen.width),h=Mathf.Max(1,Screen.height);
            root.anchorMin=new Vector2(s.x/w,s.y/h);root.anchorMax=new Vector2(s.xMax/w,s.yMax/h);root.offsetMin=root.offsetMax=Vector2.zero;
            if(headerApron!=null){headerApron.anchorMin=new Vector2(0,s.yMax/h);headerApron.anchorMax=Vector2.one;headerApron.offsetMin=headerApron.offsetMax=Vector2.zero;}
            if(footerApron!=null){footerApron.anchorMin=Vector2.zero;footerApron.anchorMax=new Vector2(1,s.y/h);footerApron.offsetMin=footerApron.offsetMax=Vector2.zero;}
        }
        public float InterfaceFactor=>game!=null&&game.InterfaceScale!=null?game.InterfaceScale.Factor:1f;
        static float DevicePixelScale()=>Application.isMobilePlatform?Mathf.Clamp(Screen.dpi>0?Screen.dpi/160f:Mathf.Min(Screen.width,Screen.height)/720f,1,4):1;
        // Every canvas takes the same reading size. Enlarging the reference layout keeps each fixed
        // box in proportion with its text, so a larger choice never truncates a label.
        void ApplyScaler(CanvasScaler scaler,bool constantPixels)
        {
            float factor=InterfaceFactor;
            if(constantPixels){scaler.uiScaleMode=CanvasScaler.ScaleMode.ConstantPixelSize;scaler.scaleFactor=DevicePixelScale()*factor;}
            else{scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(720,1280)/factor;scaler.matchWidthOrHeight=.5f;}
        }
        // Clearing the cached screen size rather than the laid-out flag keeps the reading anchor, so the
        // paragraph in front of the reader stays in place while every box changes size.
        public void ApplyInterfaceScale()
        {
            if(root==null)return;
            var pageScaler=root.parent.GetComponent<CanvasScaler>();
            ApplyScaler(pageScaler,pageScaler.uiScaleMode==CanvasScaler.ScaleMode.ConstantPixelSize);
            if(commonModal!=null&&!(scaleSlider?.Dragging??false)){ApplyScaler(commonModal.GetComponent<CanvasScaler>(),true);commonScreenSize=Vector2.zero;ReflowCommonPanel();}
            if(presetModal!=null){ApplyScaler(presetModal.GetComponent<CanvasScaler>(),true);presetScreenSize=Vector2.zero;ReflowPresetDialog();}
            Canvas.ForceUpdateCanvases();
        }
        // Every line is written when its screen is drawn, so a new language reaches the player by
        // drawing the page that is in front of them again. The settings window is rebuilt on the same
        // tab and scrolled back to the language row, so the choice that was just made stays in view.
        public void ApplyLanguage()
        {
            if(root==null)return;
            bool panelOpen=commonModal!=null;var tab=commonTab;
            if(panelOpen)CloseCommonPanel();
            if(jewelerWindow!=null)jewelerWindow.Repaint();else if(aspectStoneWindow!=null)aspectStoneWindow.Repaint();else if(blacksmith!=null)blacksmith.Repaint();else pageRepaint?.Invoke();
            if(!panelOpen)return;
            ShowCommonPanel(false);SelectSettingsTab(tab);
            Canvas.ForceUpdateCanvases();
        }
        RectTransform Rect(string name,Transform parent)
        {var go=new GameObject(name,typeof(RectTransform));go.transform.SetParent(parent,false);return go.GetComponent<RectTransform>();}
        static void Stretch(RectTransform r){r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;}
        RectTransform Box(string name,Transform parent,Color color)
        {var r=Rect(name,parent);var i=r.gameObject.AddComponent<Image>();i.color=color;return r;}
        void Place(RectTransform r,float x,float y,float w,float h)
        {r.anchorMin=r.anchorMax=new Vector2(0,1);r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);}
        // A canvas is narrower than the 720 unit reference on a tall phone, so anything that has to
        // stay reachable is measured from the right edge or stretched between both edges instead of
        // being placed at a fixed distance from the left.
        static void Right(RectTransform r,float right,float y,float w,float h)
        {r.anchorMin=r.anchorMax=new Vector2(1,1);r.pivot=new Vector2(1,1);r.anchoredPosition=new Vector2(-right,-y);r.sizeDelta=new Vector2(w,h);}
        static void Span(RectTransform r,float left,float y,float right,float h)
        {r.anchorMin=new Vector2(0,1);r.anchorMax=new Vector2(1,1);r.pivot=new Vector2(.5f,1);r.sizeDelta=new Vector2(-(left+right),h);r.anchoredPosition=new Vector2((left-right)/2f,-y);}
        static void Across(Button b,int index,int count,float y,float h,float gap=10)
        {var r=(RectTransform)b.transform;r.anchorMin=new Vector2((float)index/count,1);r.anchorMax=new Vector2((index+1f)/count,1);r.pivot=new Vector2(.5f,1);r.sizeDelta=new Vector2(-gap,h);r.anchoredPosition=new Vector2(0,-y);}
        // Every line of text on every screen is created here, so this is where the chosen language is
        // read. The object name keeps the source text: lookups by name and the runtime smoke screens
        // then behave the same in every language.
        Text Label(Transform parent,string value,int size,Color color,TextAnchor align=TextAnchor.MiddleLeft)
        {
            var r=Rect(value.Length>20?"Label":value,parent);Stretch(r);var t=r.gameObject.AddComponent<Text>();t.font=font;t.supportRichText=false;t.text=Loc.T(value);t.fontSize=size;t.color=color;t.alignment=align;t.raycastTarget=false;t.horizontalOverflow=HorizontalWrapMode.Wrap;t.verticalOverflow=VerticalWrapMode.Truncate;return t;
        }
        RectTransform Row(Transform parent,float height=70)
        {var r=Box("Card",parent,panel);var le=r.gameObject.AddComponent<LayoutElement>();le.minHeight=height;le.preferredHeight=height;return r;}
        void Inset(RectTransform r,float left=18,float right=18,float top=8,float bottom=8){Stretch(r);r.offsetMin=new Vector2(left,bottom);r.offsetMax=new Vector2(-right,-top);}
        Text Note(Transform parent,string text,int size=21,float height=60,Color? color=null)
        {var r=Rect("Text row",parent);var le=r.gameObject.AddComponent<LayoutElement>();le.preferredHeight=height;le.minHeight=height;var label=Label(r,text,size,color??muted);Inset(label.rectTransform,4,4,0,0);return label;}
        Button Button(Transform parent,string text,Action action,Color? color=null)
        {
            var r=Box(text,parent,color??UiTheme.Panel);var b=r.gameObject.AddComponent<Button>();
            r.gameObject.AddComponent<UIRectBorder>();UiTheme.Button(b);
            var colors=b.colors;colors.highlightedColor=new Color(1.15f,1.15f,1.15f);colors.pressedColor=new Color(.75f,.75f,.75f);b.colors=colors;
            var label=Label(r,text,22,pale,TextAnchor.MiddleCenter);Inset(label.rectTransform,5,5,2,2);b.onClick.AddListener(()=>{game.Audio?.Play(SoundCue.Select);action();});return b;
        }
        Button BigButton(Transform parent,string text,Action action,bool primary=false)
        {var b=Button(parent,text,action,primary?UiTheme.Primary:panel);var le=b.gameObject.AddComponent<LayoutElement>();le.minHeight=TouchHeight;le.preferredHeight=TouchHeight;return b;}
        void Icon(Transform parent,int index,float x,float y,float size)
        {
            if(index>=0&&index<18)
            {var skill=SkillIconView.Create(parent,false);Place(skill.Rect,x,y,size,size);skill.SetSprite(SkillIconAssets.Active(index));return;}
            var r=Rect("Generated art",parent);Place(r,x,y,size,size);var raw=r.gameObject.AddComponent<RawImage>();raw.texture=atlas;
            int col=index%6,row=index/6;raw.uvRect=new Rect(col/6f,1-(row+1)/4f,1/6f,1/4f);raw.raycastTarget=false;
        }
        void Base(string page,string title,string subtitle,bool art=false,bool battle=false,bool responsive=false)
        {
            CloseBlacksmith();CloseEquipmentShop();CloseRuneMaster();CloseAspectStone();CloseJeweler();
            if(PlayInventoryOpen&&page!="bag"&&page!="warehouse")ReleasePlayInventory();
            if(page!="battle")game.ExitIdle();
            CloseHudPanel();ClosePresetDialog();ClearBattleLayout();ClearInventoryLayout();ClearComparisonEquipmentLayout();
            ApplyScaler(root.parent.GetComponent<CanvasScaler>(),battle||responsive);
            if(Page=="build"&&content!=null)buildScrollOffset=content.anchoredPosition.y;
            if(Page=="edict"&&content!=null)edictScrollOffset=content.anchoredPosition.y;
            Page=page;foreach(Transform child in root){child.gameObject.SetActive(false);Destroy(child.gameObject);}skillLabels.Clear();status=hpText=null;
            foreach(Transform child in shell){child.gameObject.SetActive(false);Destroy(child.gameObject);}headerApron=footerApron=null;
            if(!battle)
            {
                var back=Box("Backdrop",shell,ink);Stretch(back);
                if(art&&background!=null){var image=Rect("Generated Sanctuary",shell);Stretch(image);var raw=image.gameObject.AddComponent<RawImage>();raw.texture=background;raw.color=new Color(1,1,1,.8f);raw.raycastTarget=false;}
            }
            header=Box("Header",root,UiTheme.Background);header.anchorMin=new Vector2(0,1);header.anchorMax=Vector2.one;header.pivot=new Vector2(.5f,1);header.sizeDelta=new Vector2(0,80);
            var top=Label(header,title,30,gold);Place(top.rectTransform,20,6,650,38);headerTitle=top;
            top.rectTransform.anchorMax=Vector2.one;top.rectTransform.sizeDelta=new Vector2(-100,38);
            var settings=Button(header,"설정·안내",ShowScreenSettings);var settingsRect=(RectTransform)settings.transform;
            settingsRect.anchorMin=settingsRect.anchorMax=new Vector2(1,1);settingsRect.pivot=new Vector2(1,1);settingsRect.anchoredPosition=new Vector2(-12,-12);settingsRect.sizeDelta=new Vector2(52,52);MakeSettingsIcon(settings);
            var sub=Label(header,subtitle,16,muted);Span(sub.rectTransform,22,46,82,26);headerSubtitle=sub;
            var line=Box("Gold divider",header,UiTheme.Gold);line.anchorMin=new Vector2(0,0);line.anchorMax=new Vector2(1,0);line.sizeDelta=new Vector2(0,2);
            footer=Box("Footer",root,UiTheme.Background);footer.anchorMin=Vector2.zero;footer.anchorMax=new Vector2(1,0);footer.pivot=new Vector2(.5f,0);footer.sizeDelta=new Vector2(0,108);footer.anchoredPosition=Vector2.zero;
            headerApron=Box("Header apron",shell,UiTheme.Background);
            footerApron=Box("Footer apron",shell,UiTheme.Background);
            headerApron.GetComponent<Image>().raycastTarget=footerApron.GetComponent<Image>().raycastTarget=false;
            RefreshGlobalHud();ApplySafeArea();
            overlay=Rect("Overlay",root);Stretch(overlay);
            toastFrame=Box("Notice",overlay,ink);toastFrame.GetComponent<Image>().raycastTarget=false;
            toastFrame.anchorMin=new Vector2(.05f,0);toastFrame.anchorMax=new Vector2(.95f,0);toastFrame.pivot=new Vector2(.5f,0);toastFrame.anchoredPosition=new Vector2(0,battle?325:112);toastFrame.sizeDelta=new Vector2(0,battle?80:62);
            toast=Label(toastFrame,"",20,pale,TextAnchor.MiddleCenter);Inset(toast.rectTransform,16,16,8,8);toastTime=0;toastFrame.gameObject.SetActive(false);
            if(!battle)
            {
                var scroll=Rect("Scroll",root);scroll.SetSiblingIndex(root.childCount-2);scroll.anchorMin=new Vector2(0,0);scroll.anchorMax=Vector2.one;scroll.offsetMin=new Vector2(24,124);scroll.offsetMax=new Vector2(-24,-94);
                scroll.gameObject.AddComponent<Image>().color=Color.clear;scroll.gameObject.AddComponent<RectMask2D>();var sr=scroll.gameObject.AddComponent<ScrollRect>();sr.horizontal=false;sr.movementType=ScrollRect.MovementType.Clamped;
                content=Rect("Content",scroll);content.anchorMin=new Vector2(0,1);content.anchorMax=new Vector2(1,1);content.pivot=new Vector2(.5f,1);content.sizeDelta=Vector2.zero;
                var layout=content.gameObject.AddComponent<VerticalLayoutGroup>();layout.spacing=10;layout.childControlHeight=true;layout.childForceExpandHeight=false;layout.childControlWidth=true;layout.childForceExpandWidth=true;
                content.gameObject.AddComponent<ContentSizeFitter>().verticalFit=ContentSizeFitter.FitMode.PreferredSize;sr.content=content;sr.viewport=scroll;
            }
        }
        void FooterButton(int index,int count,string title,Action action,bool primary=false)
        {
            var b=Button(footer,title,action,primary?UiTheme.Primary:UiTheme.Panel);var r=(RectTransform)b.transform;
            r.anchorMin=new Vector2((float)index/count,0);r.anchorMax=new Vector2((float)(index+1)/count,1);r.offsetMin=new Vector2(8,10);r.offsetMax=new Vector2(-8,-10);
        }
        public void ShowTown()=>game.EnterPlaza();
        public void ShowTownMenu()
        {
            var a=game.Store.Data;var h=a.Hero;
            pageRepaint=()=>ShowTownMenu();Base("town","HELLSCRIPT","행동을 설계하고, 균열을 지배하라",true);
            Note(content,"SANCTUARY  /  잿빛 성소",18,45,gold);
            var heroCard=Row(content,158);Icon(heroCard,18+(int)h.heroClass,15,15,125);
            var info=Label(heroCard,Loc.F("{0}   Lv.{1}\n최고 실클리어 {2}단계\n{3}", game.catalog.classNames[(int)h.heroClass], h.level, h.highestClear, h.build.name),25,pale);Place(info.rectTransform,160,10,470,135);
            var classes=Row(content,96);for(int i=0;i<3;i++){int id=i;var b=Button(classes,game.catalog.classNames[i],()=>game.SelectHero(id),i==a.selectedHero?new Color(.35f,.25f,.14f):panel);Across(b,i,3,8,TouchHeight);b.interactable=a.suspendedRun==null;}
            AddTownGuide();
            Note(content,Loc.F("골드 {0:N0}    일반 재료 {1:N0}    가방 {2}/{3}", a.gold, a.materials, h.inventory.Count(x=>!x.equipped), h.capacity),21,50,pale);
            var stage=Row(content,100);var minus=Button(stage,"−",()=>{game.SelectedStage=Mathf.Max(1,game.SelectedStage-1);ShowTownMenu();});Place((RectTransform)minus.transform,12,10,TouchHeight,TouchHeight);
            var st=Label(stage,Loc.F("균열 {0:00}단계", game.SelectedStage),28,gold,TextAnchor.MiddleCenter);Span(st.rectTransform,102,19,102,62);
            var plus=Button(stage,"+",()=>{game.SelectedStage=Mathf.Min(h.highestClear+1,game.SelectedStage+1);ShowTownMenu();});Right((RectTransform)plus.transform,12,10,TouchHeight,TouchHeight);
            BigButton(content,"단계별 등급 확률",ShowRiftRewards);
            AddRepeatPreparation();
            if(!game.Running&&a.repeatHunt?.pendingResult!=null)BigButton(content,"저장된 반복 결과 확인",game.ResumeRepeatResult,true);
            if(a.suspendedRun!=null)BigButton(content,"진행 중인 균열 이어하기",()=>game.Begin(resume:true),true);
            else BigButton(content,"균열에 진입",()=>game.Begin(),true);
            BigButton(content,"성소 거닐기 · NPC 방문",()=>game.EnterPlaza());
            BigButton(content,"균열 관리자 · 단계·소탕·훈련",ShowRiftKeeper);
            BigButton(content,"자동 행동 설계",()=>ShowBuild());
            BigButton(content,"사냥 칙령 v0.2 편집",ShowEdictEditor);
            BigButton(content,"사냥 칙령 공유 · v0.2 사용",()=>{ClearEdictShare();ShowEdictShare();});
            BigButton(content,"성장과 스킬",ShowGrowth);
            BigButton(content,"룬 성장",ShowRunes);
            BigButton(content,"장비 · 대장간 · 창고",()=>ShowBag());
            ContentButton(ContentUnlocks.Train,"고정 훈련장",ShowTraining);
            BigButton(content,"콘텐츠 해금 · 안내",ShowContentUnlocks);
            ContentButton(ContentUnlocks.Gem,"보석 장착·교체·합성",ShowGemMenu);
            BigButton(content,"장비 제작",ShowShop);
            ContentButton(ContentUnlocks.Sweep,"최고 단계 소탕",SweepAction(ShowTown));
            Note(content,"개발용 로컬 플레이 · 계정 연동과 결제는 연결 전입니다.",17,54);
            FooterButton(0,2,"전투 기록",ShowRecords);FooterButton(1,2,"게임 안내",ShowHelp);
            if(!string.IsNullOrEmpty(game.Notice))ShowToast(game.Notice);
        }
        string BattleHeading(RunState run)=>run.training>=0?(game.ComparisonRun?"TRAINING / "+(game.Comparison.IsB?"B":"A"):"TRAINING"):"RIFT / "+run.stage.ToString("00");
        public void ShowBattle()
        {
            if(game.Combat==null){ShowTown();return;}
            var run=game.Combat.State;pageRepaint=()=>ShowBattle();Base("battle",BattleHeading(run),run.training>=0?(game.Combat.OwnedTraining?Loc.F("현재 캐릭터 Lv.{0} · 60초 훈련 · 보상 없음", game.Combat.EffectiveLevel):"개발용 Lv.30 시험 · 보상 없음"):run.theme==0?"잊힌 묘지 · 처치 게이지를 채워 보스를 소환하세요":"무너진 성채 · 처치 게이지를 채워 보스를 소환하세요",battle:true);
            BuildBattleHud(run);
        }
        Image Bar(Transform parent,Vector2 pos,Vector2 size,Color color)
        {
            var back=Box("Bar track",parent,new Color(.1f,.12f,.15f));Place(back,pos.x,pos.y,size.x,size.y);var fill=Box("Bar fill",back,color);Stretch(fill);fill.pivot=new Vector2(0,.5f);return fill.GetComponent<Image>();
        }
        static void Fill(Image image,float ratio){var r=image.rectTransform;r.anchorMax=new Vector2(Mathf.Clamp01(ratio),1);r.offsetMin=r.offsetMax=Vector2.zero;}
        public void RefreshHud()
        {
            RefreshGlobalHud();
            if(game.DisplayDimmed||Page!="battle"||game.Combat==null||timerText==null)return;
            var run=game.Combat.State;RefreshGrowthHud();
            int remaining=Mathf.CeilToInt(Mathf.Max(0,game.Combat.TimeLimit-run.time));timerText.text=$"{remaining/60:00}:{remaining%60:00}";
            meterText.text=run.training>=0?Loc.F("표적 {0} / {1}",run.kills,run.enemies.Count):Loc.F("처치 {0} · 균열 {1}/100",run.kills,Mathf.Min(100,run.meter));
            if(run.training<0&&game.Combat.ObjectiveActive)meterText.text=ObjectiveProgress(run.layout);
            fullBattleAction=run.paused?Loc.T("일시정지"):Loc.F("{0} · {1:0.#}×",game.Combat.CurrentActionText,game.EffectiveSpeed);
            RefreshBossHud(run.enemies.Find(e=>e.boss&&!e.dead&&e.id==run.bossId));ReflowBattleHud();UpdateBattleBrief();
            if(riftMinimap!=null)riftMinimap.SetVerticesDirty();
            if(chestCountText!=null)chestCountText.text=Loc.F("상자 {0} / {1}",run.layout.chests.Count(c=>c.phase==ChestPhase.Opened),run.layout.chests.Count);
            if(fieldStatusText!=null)fieldStatusText.text=run.navigationError;
        }
        public void ShowTraining()
        {
            if(!RequireContent(ContentUnlocks.Train))return;
            pageRepaint=()=>ShowTraining();Base("training","훈련장","현재 캐릭터로 60전투초 동안 시험합니다");
            var hero=game.Store.Data.Hero;var stats=new HeroStats(hero,false,game.Store.Data.runes);
            Note(content,Loc.F("{0} Lv.{1} · 현재 장비와 해금한 스킬\nHP {2:0} · 공격 기준 {3:0.0}", game.catalog.classNames[(int)hero.heroClass], hero.level, stats.hp, stats.damage),22,92,pale);
            Note(content,"현재 상태의 복사본으로 실행합니다. XP·재화·장비는 바뀌지 않으며, 훈련 설정은 슬롯 저장을 선택할 때만 남습니다.",20,114);
            BigButton(content,"01  단일 적 · 스킬 순환",()=>game.Begin(0),true);
            BigButton(content,"02  다수 적 · 밀집과 선회",()=>game.Begin(1));
            BigButton(content,"03  위험 지대 · 생존과 회피",()=>game.Begin(2));
            BigButton(content,"같은 조건으로 A/B 비교",ShowComparisonPicker,true);
            Note(content,"단일·다수 표적은 공격하지 않습니다. 위험 훈련은 실제 예고와 피해가 발생하며 사망할 수 있습니다.\n종류마다 배치와 시드가 고정됩니다. 앱을 종료하면 훈련을 새로 시작합니다.",20,150,pale);
            FooterButton(0,1,"성소로 돌아가기",ShowTown);
        }
        public void ShowBuild()
        {
            if(game.ComparisonRun){if(game.Active)ShowComparisonConditions();else ShowComparisonEditor();return;}
            comparisonEditing=false;
            buildWasPaused=game.Active&&game.Combat.State.paused;if(game.Active)game.Combat.State.paused=true;
            editing=(game.Active?game.Combat.State.build:game.Store.Data.Hero.build).Copy();BehaviorRules.Normalize(editing);expandedRule=0;buildScrollOffset=0;RenderBuild();
        }
        void LoadEditing(BuildConfig proposed)
        {
            editing=RiftLoadoutLocked?BuildEditing.DuringRift(game.Combat.State.build,proposed,true):proposed.Copy();RenderBuild();
            if(RiftLoadoutLocked)ShowToast("장착은 유지하고 현재 스킬의 행동 설정을 불러왔습니다.");
        }
        public void CancelBuild()
        {if(comparisonEditing){comparisonEditing=false;ShowComparisonResult();return;}if(game.Active){game.Combat.State.paused=buildWasPaused;ShowBattle();}else ShowTown();}
        void RenderBuild()
        {
            pageRepaint=()=>RenderBuild();Base("build",comparisonEditing?"B 행동 설정":"행동 설계",comparisonEditing?"B 훈련에만 사용합니다 · 실제 설정은 유지됩니다":"위에서부터 검사합니다 · 실행할 수 있는 행동 하나를 선택합니다");
            if(comparisonEditing&&comparisonHero.useEdict){RenderEdictComparisonBuild();return;}
            var presets=Row(content,96);for(int i=0;i<2;i++){int v=i;var b=Button(presets,GameCatalog.Preset(EditingHero.heroClass,i).name,()=>ShowRecommendation(v));Across(b,i,2,8,TouchHeight);}
            if(comparisonEditing)AddComparisonDifference();else AddGuideBuildDifference();
            if(RiftLoadoutLocked)Note(content,"균열에서는 장착을 유지합니다. 불러온 설정에 없는 장착 스킬은 기존 행동을 유지합니다.",20,92,pale);
            Note(content,comparisonEditing?"행동과 소유 장비의 조합을 B에서 시험합니다. 실제 캐릭터에는 적용하지 않습니다.":game.Active&&game.Combat.State.training>=0?"훈련 복사본에 적용합니다. 실제 설정에 남기려면 슬롯에 저장하세요.":"전투 중에는 조건만 편집합니다. 스킬 교체는 성소에서 합니다.",18,70);
            if(comparisonEditing)BigButton(content,"B 장비 선택",RenderComparisonEquipment,true);
            Cycle(content,"목표",new[]{"가까운 적","정예·보스 우선","지원형 우선","낮은 HP","밀집 중심"},(int)editing.target,i=>{editing.target=(TargetMode)i;RenderBuild();});
            Cycle(content,"이동",BehaviorRules.Movements,(int)editing.movement,i=>{editing.movement=(MovementMode)i;RenderBuild();});
            SliderRow(content,"목표 거리",editing.distance,.5f,10,v=>editing.distance=Mathf.Round(v*2)/2,"m");
            SliderRow(content,"물약 HP",editing.potionThreshold,5,95,v=>editing.potionThreshold=Mathf.Round(v/5)*5,"%");
            RenderRules();
            Note(content,"장착 스킬 · 서로 다른 액티브 최대 4개",22,48,gold);
            int start=(int)EditingHero.heroClass*6;
            for(int i=start;i<start+6;i++)
            {
                int id=i;bool equipped=editing.activeSkills.Contains(id);var s=game.catalog.skills[i];
                var b=BigButton(content,Loc.F("{0}{1}   Lv.{2}",(equipped?"✓ ":"+ "),s.name,s.unlock),()=>
                {
                    if(RiftLoadoutLocked){ShowToast("스킬 장착은 성소와 훈련장에서 바꿀 수 있습니다.");return;}
                    if(equipped){editing.activeSkills.Remove(id);editing.rules.RemoveAll(r=>r.action==RuleAction.Skill&&r.skill==id);}
                    else if(editing.activeSkills.Count<4){editing.activeSkills.Add(id);AddRule(BehaviorRules.Make(id));return;}
                    else {ShowToast("먼저 다른 스킬 하나를 해제해 주세요.");return;}RenderBuild();
                });
            }
            Note(content,Loc.F("패시브 · 현재 {0}개 슬롯 (영웅 Lv.{1})",EditingPassiveSlots,string.Join("/",ContentUnlocks.Rules.passiveLevels)),22,70,gold);
            for(int i=0;i<6;i++)
            {
                int id=i;bool selected=editing.passives.Contains(id);BigButton(content,Loc.F("{0}{1}",(selected?"✓ ":"+ "),GameCatalog.Passives[start+i]),()=>{if(RiftLoadoutLocked){ShowToast("패시브는 성소와 훈련장에서 바꿀 수 있습니다.");return;}var list=editing.passives.ToList();if(selected)list.Remove(id);else if(list.Count<EditingPassiveSlots)list.Add(id);editing.passives=list.ToArray();RenderBuild();});
                Note(content,GameCatalog.PassiveDescriptions[start+i],18,80,pale);
            }
            Cycle(content,"가방 부족",new[]{"포탈 정리","낮은 가치 교체","획득 무시"},(int)editing.bagPolicy,i=>{editing.bagPolicy=(BagPolicy)i;RenderBuild();});
            Cycle(content,"획득 허용",GameCatalog.Rarities,editing.minimumRarity,i=>{editing.minimumRarity=i;RenderBuild();});
            Cycle(content,"추적할 최소 등급",GameCatalog.Rarities,editing.pursueRarity,i=>{editing.pursueRarity=i;RenderBuild();});
            Cycle(content,"균열 탐색",new[]{"왼쪽 벽 따라가기","오른쪽 벽 따라가기","가까운 미탐색 지점","감지한 적 추적"},(int)editing.exploration,i=>{editing.exploration=(ExplorationPolicy)i;RenderBuild();});
            BigButton(content,editing.openChests?"✓ 안전할 때 상자 개봉":"상자 개봉 꺼짐",()=>{editing.openChests=!editing.openChests;RenderBuild();});
            BigButton(content,editing.commonChests?"✓ 일반 보물 상자 회수":"일반 보물 상자 제외",()=>{editing.commonChests=!editing.commonChests;RenderBuild();});
            BigButton(content,editing.sealedChests?"✓ 봉인 상자 회수":"봉인 상자 제외",()=>{editing.sealedChests=!editing.sealedChests;RenderBuild();});
            SliderRow(content,"상자까지 허용 이동 거리",editing.chestDetour,0,30,v=>editing.chestDetour=Mathf.Round(v),"m");
            BigButton(content,editing.chestsAfterBoss?"✓ 보스 등장 후에도 발견한 상자 회수":"보스 등장 후 보스 우선",()=>{editing.chestsAfterBoss=!editing.chestsAfterBoss;RenderBuild();});
            BigButton(content,editing.allowCursedChests?"✓ 저주 상자 참여 · 12초 진압전":"저주 상자 참여 안 함",()=>{editing.allowCursedChests=!editing.allowCursedChests;RenderBuild();});
            SliderRow(content,"저주 상자 시작 시 남은 시간",editing.cursedMinimumTime,90,180,v=>editing.cursedMinimumTime=Mathf.Round(v),"초 이상");
            Note(content,"저주 상자는 5단계부터 등장할 수 있습니다. 참여하지 않거나 진압에 실패하면 해당 상자의 보상은 지급되지 않습니다.",19,102,muted);
            BigButton(content,editing.useShrines?"✓ 안전할 때 성소 사용":"성소 사용 꺼짐",()=>{editing.useShrines=!editing.useShrines;RenderBuild();});
            BigButton(content,editing.guideShrines?"✓ 길잡이의 성소 · 비전투 이동":"길잡이의 성소 제외",()=>{editing.guideShrines=!editing.guideShrines;RenderBuild();});
            BigButton(content,editing.resolveShrines?"✓ 결의의 성소 · 피해 감소":"결의의 성소 제외",()=>{editing.resolveShrines=!editing.resolveShrines;RenderBuild();});
            SliderRow(content,"성소까지 허용 이동 거리",editing.shrineDetour,0,30,v=>editing.shrineDetour=Mathf.Round(v),"m");
            BigButton(content,editing.autoRepeat?"✓ 자동 반복 활성":"자동 반복 꺼짐",()=>{editing.autoRepeat=!editing.autoRepeat;RenderBuild();});
            BigButton(content,editing.advanceOnWin?"✓ 성공 시 다음 단계":"성공 시 같은 단계",()=>{editing.advanceOnWin=!editing.advanceOnWin;RenderBuild();});
            RenderPresetRows();
            if(game.Active)Note(content,Loc.F("행동 {0}행 변경 · 적용 후 {1}", BuildEditing.ChangedRows(game.Combat.State.build,editing), (buildWasPaused?"일시정지 유지":"전투 재개")),20,62,pale);
            FooterButton(0,2,"취소",CancelBuild);
            FooterButton(1,2,comparisonEditing?"B 훈련 시작":"설정 적용",()=>{if(!ValidateEditing())return;editing.version=DateTime.UtcNow.Ticks.ToString();if(comparisonEditing)StartComparisonDraft();else game.CommitBuild(editing,buildWasPaused);},true);
            RestoreBuildScroll();
        }
        void Cycle(Transform parent,string title,string[] choices,int index,Action<int> next)
        {BigButton(parent,Loc.F("{0}  ·  {1}",title,choices[index]),()=>next((index+1)%choices.Length));}
        void SliderRow(Transform parent,string title,float value,float min,float max,Action<float> changed,string unit="")
        {var row=Row(parent,78);SliderAt(row,title,value,min,max,changed,new Vector2(18,8),0,unit);}
        void SliderAt(Transform parent,string title,float value,float min,float max,Action<float> changed,Vector2 pos,float width,string unit="")
        {
            var label=Label(parent,Loc.F("{0}  {1:0.#}{2}",title,value,unit),20,muted);
            if(width>0)Place(label.rectTransform,pos.x,pos.y,width,30);else Span(label.rectTransform,pos.x,pos.y,pos.x,30);
            var r=Rect("Slider",parent);
            if(width>0)Place(r,pos.x,pos.y+38,width,22);else Span(r,pos.x,pos.y+38,pos.x,22);
            var slider=r.gameObject.AddComponent<Slider>();slider.minValue=min;slider.maxValue=max;slider.value=value;
            var track=Box("Track",r,new Color(.15f,.2f,.25f));Stretch(track);track.offsetMin=new Vector2(0,7);track.offsetMax=new Vector2(0,-7);
            var fill=Box("Fill",track,gold);Stretch(fill);slider.fillRect=fill;
            var handle=Box("Handle",r,gold);handle.sizeDelta=new Vector2(22,22);slider.handleRect=handle;slider.targetGraphic=handle.GetComponent<Image>();
            slider.onValueChanged.AddListener(v=>{changed(v);label.text=Loc.F("{0}  {1:0.#}{2}",title,v,unit);RefreshGuideBuildDifference();RefreshPresetSaveButtons();});
        }
        public void ShowBag(bool portal=false)=>RenderEquipment(portal);
        Color RarityColor(int r)=>StorageSurface.Hex(EquipmentGradePalette.Hex[Mathf.Clamp(r,0,4)]);
        void ShowWarehouse(bool portal)=>RenderWarehouse(portal);
        public void ShowShop()=>RenderItemShop();
        public void ShowRecords()
        {
            pageRepaint=()=>ShowRecords();Base("records","전투 기록","최근 균열 10회 · 행동 이유와 실패 원인을 확인합니다");
            if(game.Store.Data.records.Count==0)Note(content,"아직 완료한 균열이 없습니다.",24,80);
            foreach(var record in game.Store.Data.records){var r=record;BigButton(content,Loc.F("{0} · {1}단계 · {2}", r.hero, r.stage, r.result),()=>ShowLog(r));}
            FooterButton(0,1,"성소로",ShowTown);
        }
        void ShowLog(RunRecord record)
        {ShowRunReview(record);}
        public void ShowResult()
        {
            if(game.ComparisonRun){ShowComparisonResult();return;}
            if(game.Combat==null)return;var r=game.Combat.State;bool won=r.phase==RunPhase.Cleared;ReviewBase("result",r.training>=0?"훈련 결과":won?"균열 정복":"다시 설계할 시간",r.action,ShowResult);
            game.RecordGuide(()=>FirstPlayGuide.ReadResult(game.Store.Data,r));
            Note(content,r.training>=0?"TRAINING":won?"VICTORY":"RECALIBRATE",42,98,gold);
            if(r.training<0)AddRepeatResult();
            string summary=r.training>=0?Loc.F("{0} Lv.{1} · 고정 훈련 {2}\n\n표적 처치 {3} / {4} · 남은 HP {5:0}\n전투 시간 {6:0.0}초 / 실제 {7:0.0}초\n총 피해 {8:N0} · 전투 초당 {9:0.0}", game.catalog.classNames[(int)game.Combat.Hero.heroClass], game.Combat.EffectiveLevel, r.training+1, r.kills, r.enemies.Count, Mathf.Max(0,r.health), r.time, r.realTime, r.dealt, r.dealt/Mathf.Max(CombatSimulation.Step,r.time)):Loc.F("{0} · {1}단계\n\n처치 {2}   /   획득 {3}개\n전투 시간 {4:0.0}초   /   실제 {5:0.0}초\n총 피해 {6:N0}", game.catalog.classNames[(int)game.Store.Data.Hero.heroClass], r.stage, r.kills, r.lootCount, r.time, r.realTime, r.dealt);
            var card=Row(content,250);var t=Label(card,summary,27,pale);Inset(t.rectTransform,24,24,18,18);
            if(r.training>=0)Note(content,"훈련 결과입니다. 실제 계정 보상은 지급하지 않습니다.",21,70,gold);
            else Note(content,Loc.F("상자 개봉 {0} / {1}개\n미개봉 상자 보상은 다음 판으로 이월되지 않습니다.", r.layout.chests.Count(c=>c.phase==ChestPhase.Opened), r.layout.chests.Count),21,92,gold);
            if(r.runesAwarded>0)Note(content,Loc.F("룬 {0}개 획득 · 룬 성장에서 배치할 수 있습니다.",r.runesAwarded),21,70,gold);
            if(r.gemsCollected>0)BigButton(content,Loc.F("보석 {0}개 획득 · 공용 보관함 열기",r.gemsCollected),ShowGemMenu);
            if(r.training<0)ObjectiveResult(RiftObjectives.Capture(r));
            if(r.training<0&&r.phase==RunPhase.Failed&&r.health<=0)BigButton(content,"사망 원인 분석 (최근 5초 기록)",ShowDefeatAnalysis,true);
            var completed=game.Store.Data.records.FirstOrDefault(record=>record.id==r.id);
            if(r.training<0&&HasReview(completed))BigButton(content,"전투 상세 기록",()=>ShowRunReview(completed,true));
            if(game.Combat.OwnedTraining)BigButton(content,"시험한 설정을 슬롯에 저장",ShowTrainingPresetSave);
            if(r.training<0)Note(content,"성공·실패와 관계없이 이미 얻은 XP·장비·재화는 유지됩니다. 다음 해금과 변경할 행동을 확인한 뒤 다시 도전하세요.",20,104,pale);
            BigButton(content,"첫 플레이 안내 · 다음 할 일",ShowOnboarding);
            foreach(string log in r.logs.Skip(Math.Max(0,r.logs.Count-8)))Note(content,Loc.LogLine(log),17,45);
            FooterButton(0,2,"성소로",()=>game.ReturnTown());FooterButton(1,2,"다시 도전",()=>{int training=r.training;game.ReturnTown();game.Begin(training);},true);
        }
        public void ShowDefeatAnalysis()
        {
            if(game.Combat==null)return;var r=game.Combat.State;
            var completed=game.Store.Data.records.FirstOrDefault(record=>record.id==r.id);
            if(HasReview(completed)){ShowReviewWindow(completed,true,true);return;}
            pageRepaint=()=>ShowDefeatAnalysis();Base("defeat-analysis","사망 원인 분석","사망 직전 5초간의 피해 집중 및 스킬 불발 내역");
            var analysis=r.statistics?.BuildDefeatAnalysis(game.catalog,5f);
            if(analysis==null||analysis.snapshotCount==0)
            {
                Note(content,"기록된 5초간의 상세 피격 데이터가 없습니다.",22,80);
            }
            else
            {
                float dps=analysis.burstDamage5s/Mathf.Max(0.1f,analysis.preDeathWindowSeconds);
                Note(content,Loc.F("5초간 총 피격 피해: {0:N0} (초당 {1:0.0} / HP 손실 {2:N0})", analysis.burstDamage5s, dps, analysis.hpLost5s),24,60,gold);
                Note(content,"주요 사망 원인 피격원:",21,45,pale);
                foreach(var src in analysis.fatalSources)
                {
                    Note(content,Loc.F("• {0}: {1:N0} ({2:0.0}%)",src.sourceName,src.totalDamage,src.percentage),20,40,pale);
                }
                if(analysis.blockedSummary.Count>0)
                {
                    Note(content,"스킬 불발 원인 (생존/반응 실패 요인):",21,45,gold);
                    foreach(var blk in analysis.blockedSummary)
                    {
                        Note(content,Loc.F("• {0}: {1} ×{2}회", blk.skillName, blk.reasonName, blk.count),20,40,new Color(1,.5f,.4f));
                    }
                }
                else
                {
                    Note(content,"5초 동안 자원/쿨다운으로 인한 스킬 불발은 감지되지 않았습니다.",19,50,muted);
                }
            }
            BigButton(content,"사냥 칙령 수정하러 가기",ShowBuild,true);
            FooterButton(0,1,"결과 화면으로",ShowResult);
        }
        public void ShowTrainingPresetSave()
        {
            if(game.Combat==null||!game.Combat.OwnedTraining)return;
            pageRepaint=()=>ShowTrainingPresetSave();Base("training-save","훈련 설정 저장","실제 캐릭터의 장비와 현재 행동은 유지됩니다");
            Note(content,"시험한 행동 설정과 장비 참조를 저장합니다. 나중에 성소의 행동 설계에서 불러올 수 있습니다. 기존 슬롯을 고르면 해당 설정을 교체합니다.",21,150,pale);
            for(int i=0;i<HuntEdict.PresetSlots;i++){int slot=i;var presets=game.Store.Data.Hero.presets;BigButton(content,Loc.F("슬롯 {0} · {1}", i+1, (slot<presets.Count&&BuildEditing.HasPreset(presets[slot])?presets[slot].name:"비어 있음")),()=>ShowPresetNameDialog(slot,game.Combat.State.build,name=>game.SaveTrainingPreset(slot,name),ShowResult));}
            FooterButton(0,1,"훈련 결과로",ShowResult);
        }
        void Confirm(string message,Action action)
        {
            game.ExitIdle();
            var modal=Box("Confirm",root,new Color(0,0,0,.83f));Stretch(modal);var card=Box("Dialog",modal,panel);card.anchorMin=new Vector2(.08f,.35f);card.anchorMax=new Vector2(.92f,.65f);card.offsetMin=card.offsetMax=Vector2.zero;
            var text=Label(card,message,25,pale,TextAnchor.MiddleCenter);text.rectTransform.anchorMin=new Vector2(.06f,.35f);text.rectTransform.anchorMax=new Vector2(.94f,.94f);text.rectTransform.offsetMin=text.rectTransform.offsetMax=Vector2.zero;
            void Close(){ContentWindowHost.Detach(modal);modal.gameObject.SetActive(false);Destroy(modal.gameObject);}
            ContentWindowHost.Attach(modal,Close);
            var cancel=Button(card,"취소",Close);var ok=Button(card,"확인",()=>{game.Audio?.Play(SoundCue.Confirm);Close();action();},new Color(.45f,.28f,.12f));
            foreach(var b in new[]{cancel,ok}){var r=(RectTransform)b.transform;r.anchorMin=new Vector2(b==cancel?.05f:.52f,.05f);r.anchorMax=new Vector2(b==cancel?.48f:.95f,.27f);r.offsetMin=r.offsetMax=Vector2.zero;}
        }
        public void ShowToast(string text)
        {
            if(game.DisplayDimmed){RefreshIdleSummary(true);return;}
            if(toast==null)return;toast.text=Loc.T(text);toastTime=5;toastFrame.gameObject.SetActive(!string.IsNullOrEmpty(text));overlay.SetAsLastSibling();
            if(Page=="battle"){toastFrame.gameObject.SetActive(false);UpdateBattleBrief();return;}
            Canvas.ForceUpdateCanvases();var size=toastFrame.sizeDelta;size.y=Mathf.Max(Page=="battle"?80:62,toast.preferredHeight+16);toastFrame.sizeDelta=size;
        }
        void Update()
        {
            if(game.Store!=null&&shownStoreRevision!=game.Store.Revision){shownStoreRevision=game.Store.Revision;RefreshHud();}
            TickGlobalHud();
            if(game.DisplayDimmed){RefreshIdleSummary();return;}
            if(idleIntroductionOpen)ReflowIdleIntroduction();
            using var sample=PresentationMetrics.UI.Auto();
            PresentationMetrics.HudCalls++;
            bool commonWasOpen=CommonPanelOpen;
            UpdatePresetDialog();
            UpdateCommonPanel();
            ReflowInventory();
            ReflowComparisonEquipment();
            ReflowComparisonPages();
            ReflowBattleHud();
            ReflowEdictRows();
            ReflowRunes();
            RefreshPlaza();
            ReflowResultRows();
            ReflowHistory();
            RefreshRepeatStatus();
            if(commonWasOpen||CommonPanelOpen)return;
            hudClock+=Time.unscaledDeltaTime;if(hudClock>.15f){hudClock=0;RefreshHud();}
            RefreshGuideHint();
            if(toast!=null&&toastTime>0){toastTime-=Time.unscaledDeltaTime;if(toastTime<=0){toast.text="";toastFrame.gameObject.SetActive(false);}}
        }
    }
}
