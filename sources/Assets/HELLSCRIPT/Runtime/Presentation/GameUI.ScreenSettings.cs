using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Hellscript
{
    public sealed partial class GameUI
    {
        enum SettingsSection { Screen,Sound,Language,Character,Account,Help,Combat }
        SettingsSection commonTab;
        RectTransform commonModal,commonSafe,commonCard,commonTabs,commonWorld;Button commonClose,commonBackdrop;
        RectTransform screenPane,languagePane,characterPane,helpPane,combatPane,commonGem;
        Text commonHeading,aspectMessage,languageMessage,characterMessage,characterNotice;
        Vector2 commonScreenSize;Rect commonSafeArea;float commonKeyboardTop;bool commonLaidOut;
        readonly List<(CanvasGroup group,float alpha)> commonBackgrounds=new List<(CanvasGroup,float)>();
        readonly List<(string id,Button button)> aspectButtons=new List<(string,Button)>();
        readonly List<(string code,Button button)> languageButtons=new List<(string,Button)>();
        readonly List<(int index,Button button)> characterButtons=new List<(int,Button)>();
        public bool CommonPanelOpen=>windowHost!=null&&windowHost.BlocksGameplay||commonModal!=null||hudPanel!=null||idleIntroductionOpen||PlayInventoryOpen||EquipmentShopOpen||BlacksmithOpen||RuneMasterOpen||GearRecommendationDetails!=null;
        public bool BlocksRepeat=>RiftVictory?.RetryCounting==true||runeSession||CommonPanelOpen||presetModal!=null||root!=null&&root.Find("Confirm")!=null;
        // The title and the character screen, where the hero is chosen before the game begins.
        public bool EntryPage=>Page=="title"||Page=="characters";
        public void ShowScreenSettings()=>ShowCommonPanel(false);
        public void ShowCombatOverview(){ShowCommonPanel(false);SelectCombatTab();}
        void ShowHelp()=>ShowCommonPanel(true);
        void ShowCommonPanel(bool help)
        {
            game.ExitIdle();ResetTownInput();game.Town?.Cancel();
            if(commonModal!=null){SelectSettingsTab(help?SettingsSection.Help:SettingsSection.Screen);return;}
            commonBackgrounds.Clear();foreach(var existing in GetComponentsInChildren<Canvas>())
            {if(!existing.TryGetComponent<CanvasGroup>(out var group))group=existing.gameObject.AddComponent<CanvasGroup>();commonBackgrounds.Add((group,group.alpha));}
            commonModal=Rect("Screen and help modal",transform);var canvas=commonModal.gameObject.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=120;canvas.pixelPerfect=true;
            SettingsScaler(commonModal.gameObject.AddComponent<CanvasScaler>());commonModal.gameObject.AddComponent<GraphicRaycaster>();ContentWindowHost.Attach(commonModal,CloseCommonPanel);
            commonBackdrop=Button(commonModal,"",CloseCommonPanel,new Color(.025f,.035f,.049f,1));commonBackdrop.name="settings-backdrop";UiTheme.Backdrop(commonBackdrop);Stretch((RectTransform)commonBackdrop.transform);commonBackdrop.transition=Selectable.Transition.None;
            commonSafe=Rect("Common safe area",commonModal);Stretch(commonSafe);
            commonWorld=Rect("Settings world area",commonSafe);commonWorld.anchorMin=Vector2.zero;commonWorld.anchorMax=new Vector2(.5f,1);commonWorld.offsetMin=commonWorld.offsetMax=Vector2.zero;
            // The mockup's .bgtip: a cream pill with an ink outline, 8 / 20 px padding, 24 px above the bottom of the world half.
            var tip=Rect("Settings world hint",commonWorld);tip.anchorMin=tip.anchorMax=new Vector2(.5f,0);tip.pivot=new Vector2(.5f,0);tip.anchoredPosition=new Vector2(0,24);
            var tipLayout=tip.gameObject.AddComponent<HorizontalLayoutGroup>();tipLayout.padding=new RectOffset(22,22,10,10);tipLayout.childControlWidth=tipLayout.childControlHeight=true;tipLayout.childForceExpandWidth=tipLayout.childForceExpandHeight=false;
            var tipFit=tip.gameObject.AddComponent<ContentSizeFitter>();tipFit.horizontalFit=tipFit.verticalFit=ContentSizeFitter.FitMode.PreferredSize;CasualSettingsStyle.Surface(tip,UiSkin.Casual.Cream,UiSkin.Casual.Navy,20).outside=true;
            var hint=Label(tip,"배경을 누르면 닫힙니다.",Mathf.RoundToInt(SettingsLook.Fs+1),UiSkin.Casual.Navy,TextAnchor.MiddleCenter);hint.font=UiSkin.Casual.DisplayFont;hint.horizontalOverflow=HorizontalWrapMode.Overflow;
            commonCard=Box("Common dialog",commonSafe,Color.white);CasualBattleHudStyle.Panel(commonCard);commonCard.GetComponent<Image>().color=Color.clear;commonCard.anchorMin=commonCard.anchorMax=commonCard.pivot=new Vector2(.5f,.5f);
            commonHeading=Label(commonCard,"설정",26,UiSkin.Casual.Gold,TextAnchor.MiddleCenter);commonHeading.font=UiSkin.Casual.DisplayFont;CasualBattleHudStyle.Outline(commonHeading,2);
            commonGem=Rect("Settings top gem",commonCard);CasualSettingsStyle.Surface(commonGem,UiSkin.Casual.Gold,UiSkin.Casual.Navy,18);
            commonTabs=Rect("Common categories",commonCard);
            // The tutorial keeps its hero, so the character tab is gone while it runs; signing out, the way to another hero, moves to the account tab.
            bool heroLocked=!EntryPage&&Tutorials.Mandatory(game.Store.Data);characterButtons.Clear();characterPane=null;characterNotice=characterMessage=null;
            foreach(var entry in new[]{(SettingsSection.Screen,"화면"),(SettingsSection.Sound,"소리"),(SettingsSection.Language,"언어"),(SettingsSection.Character,"캐릭터 변경"),(SettingsSection.Account,"계정")})
            {
                if(heroLocked&&entry.Item1==SettingsSection.Character)continue;
                var tab=entry.Item1;var b=Button(commonTabs,entry.Item2,()=>SelectSettingsTab(tab));b.name="settings-tab-"+tab;CasualSettingsStyle.Button(b,UiButtonRole.Tab);((UiButton)b).Face.ShadowOutside=true;
                b.GetComponentInChildren<Text>().fontSize=18;b.gameObject.AddComponent<SettingsTabView>().Initialize(b,"tab-"+tab.ToString().ToLowerInvariant());
            }
            BuildDisplayPane();BuildSoundPane();BuildLanguagePane();if(!heroLocked)BuildCharacterPane();BuildAccountPane(heroLocked);BuildInformationPanes();
            commonClose=Button(commonCard,"×",CloseCommonPanel);commonClose.name="settings-close";CasualSettingsStyle.Button(commonClose,UiButtonRole.Quiet,true);commonClose.GetComponentInChildren<Text>().fontSize=24;
            commonLaidOut=false;SelectSettingsTab(help?SettingsSection.Help:SettingsSection.Screen);ReflowCommonPanel();
        }
        // One canvas unit is one pixel of the settings mockup on every board.
        void SettingsScaler(CanvasScaler scaler){scaler.uiScaleMode=CanvasScaler.ScaleMode.ConstantPixelSize;scaler.scaleFactor=UiMock.CanvasScale(UiSafeArea.Current)*InterfaceFactor;}
        void BuildDisplayPane()
        {
            screenPane=CommonScroll("화면 설정 본문",out var body);BuildViewDistanceControl(body);BuildMapDisplayControl(body);
            var card=CasualSettingsStyle.Card(body,"Screen ratio card");CasualSettingsStyle.Header(card,"화면 비율","tab-screen",out _);
            CommonNote(card,"비율을 선택하면 바로 적용합니다. PC에서는 창 크기를 맞추고, 모바일에서는 화면을 회전한 뒤 선택한 비율로 표시합니다.",19);aspectButtons.Clear();
            var automatic=SettingsButton(card,"화면에 맞춤",()=>game.ApplyAspect("auto"),icon:"button-fit",chip:true);automatic.name="settings-aspect-auto";aspectButtons.Add(("auto",automatic));
            for(int side=0;side<2;side++)
            {
                CasualSettingsStyle.Sub(card,side==0?"가로 비율":"세로 비율");var choices=SettingsGrid(card,"Aspect choices "+side);
                foreach(string id in side==0?DisplayAspect.Landscape:DisplayAspect.Portrait)
                {var button=Button(choices,id,()=>game.ApplyAspect(id));button.name="settings-aspect-"+id;CasualSettingsStyle.Button(button,UiButtonRole.Option);((UiButton)button).Face.ShadowOutside=true;button.GetComponentInChildren<Text>().fontSize=Mathf.RoundToInt(SettingsLook.Fs+2);button.gameObject.AddComponent<SettingsRatioIcon>().Initialize(id);aspectButtons.Add((id,button));}
            }
            aspectMessage=CommonNote(card,"",18,UiSkin.Casual.Muted);SavedNote(card,"화면 설정은 이 기기에 저장됩니다.");
        }
        void BuildLanguagePane()
        {
            languagePane=CommonScroll("언어 설정 본문",out var body);var card=CasualSettingsStyle.Card(body,"Language card");CasualSettingsStyle.Header(card,"언어","tab-language",out _);
            CommonNote(card,"사용할 언어를 선택하세요. 선택한 언어는 이 기기에 저장됩니다.",20);languageButtons.Clear();
            foreach(string code in new[]{"en","ko"})
            {
                var button=BigButton(card,LanguageOptions.Find(code).NativeName,()=>game.ApplyLanguage(code));button.name="settings-language-"+code;
                CasualSettingsStyle.DecorateChoice(button,"tab-language");var badge=button.transform.Find("tab-language");
                var glyph=Label(badge,"",Mathf.RoundToInt(SettingsLook.Gem*.42f),UiSkin.Casual.Navy,TextAnchor.MiddleCenter);glyph.text=code=="en"?"EN":LanguageOptions.Find(code).NativeName.Substring(0,1);UiSkin.Casual.ApplyLabel(glyph);glyph.color=UiSkin.Casual.White;CasualBattleHudStyle.Outline(glyph);
                languageButtons.Add((code,button));
            }
            languageMessage=CommonNote(card,"",19,UiSkin.Casual.Muted);
        }
        void BuildCharacterPane()
        {
            characterPane=CommonScroll("캐릭터 변경 본문",out var body);var card=CasualSettingsStyle.Card(body,"Character card");CasualSettingsStyle.Header(card,"캐릭터 변경","tab-character",out _);
            var title=SettingsButton(card,"타이틀로 이동",()=>{if(!game.ReturnToTitle())characterMessage.text=Loc.T(string.IsNullOrEmpty(game.Store.Error)?game.Notice:game.Store.Error);},icon:"button-exit");title.name="settings-title";
            CommonNote(card,"현재 진행을 저장하고 로그아웃합니다. 타이틀에서 다른 계정으로 로그인할 수 있습니다.",19,UiSkin.Casual.Muted);
            // The mockup keeps the notice, the hero cards and the message together in one white card.
            var heroCard=CasualSettingsStyle.Card(body,"Hero choice card");
            characterNotice=CasualSettingsStyle.Notice(heroCard,"",size:20);characterButtons.Clear();
            var heroes=Rect("Settings heroes",heroCard);var grid=heroes.gameObject.AddComponent<SettingsHeroGrid>();grid.cellHeight=SettingsLook.HeroCard;
            foreach(int index in new[]{0,3,1,4,2,5})
            {
                if(!HeroSlots.IsCreated(game.Store.Data,index))continue;var hero=game.Store.Data.heroes[index];
                var button=BigButton(heroes,Loc.T(game.catalog.classNames[(int)hero.heroClass]),()=>{if(!game.ChangeCharacterFromSettings(index)&&characterMessage!=null)characterMessage.text=Loc.T(string.IsNullOrEmpty(game.Store.Error)?game.Notice:game.Store.Error);});
                button.name="settings-character-"+index;CasualSettingsStyle.DecorateChoice(button,"",hero:true);
                var seal=Rect("Hero seal",button.transform);seal.SetSiblingIndex(2);var image=seal.gameObject.AddComponent<Image>();image.preserveAspect=true;image.raycastTarget=false;
                image.sprite=globalHud!=null?globalHud.Portrait(hero.heroClass,hero.gender):Resources.Load<Sprite>("Art/GlobalHUD/Casual/seal-"+(hero.heroClass==HeroClass.Warrior?"warrior":hero.heroClass==HeroClass.Ranger?"ranger":"mage")+"-"+(hero.gender==HeroGender.Female?"f":"m"));
                var slot=seal.gameObject.AddComponent<LayoutElement>();slot.minWidth=slot.preferredWidth=SettingsLook.Seal;slot.minHeight=slot.preferredHeight=SettingsLook.Seal;
                var text=button.transform.Find("Choice text");
                var gender=CasualSettingsStyle.Icon(button.transform,hero.gender==HeroGender.Female?"gender-female":"gender-male");gender.gameObject.AddComponent<LayoutElement>().ignoreLayout=true;Right(gender.rectTransform,14,10,22,22);
                var levelRow=Rect("Hero level row",text);var levelLayout=levelRow.gameObject.AddComponent<HorizontalLayoutGroup>();levelLayout.childControlWidth=levelLayout.childControlHeight=true;levelLayout.childForceExpandWidth=levelLayout.childForceExpandHeight=false;
                var pill=Rect("Hero level pill",levelRow);var pillSize=pill.gameObject.AddComponent<LayoutElement>();CasualSettingsStyle.Surface(pill,UiSkin.Casual.White,UiSkin.Casual.Navy,14);
                var level=Label(pill,$"Lv.{hero.level}",Mathf.RoundToInt(SettingsLook.Fsm),UiSkin.Casual.OrangeInk,TextAnchor.MiddleCenter);level.name="Hero level";level.font=UiSkin.Casual.DisplayFont;level.horizontalOverflow=HorizontalWrapMode.Overflow;pillSize.minWidth=pillSize.preferredWidth=level.preferredWidth+22;pillSize.minHeight=pillSize.preferredHeight=SettingsLook.Fsm*SettingsLook.JuaLine+6;
                characterButtons.Add((index,button));
            }
            characterMessage=CasualSettingsStyle.Notice(heroCard,"",true,size:19);
        }
        float SettingsTouchHeight=>SettingsLook.Btn;
        // The mockup's .pbtn (or .chip when chip is set): min height from the board, Jua at --fs plus 3 (plus 2), the icon 1.25 times the font, icon and text one centred group 8 px apart.
        Button SettingsButton(Transform parent,string text,System.Action action,UiButtonRole role=UiButtonRole.Action,string icon=null,bool chip=false)
        {
            var button=BigButton(parent,text,action);float height=chip?SettingsLook.Chip:SettingsTouchHeight;var touch=button.GetComponent<LayoutElement>();touch.minHeight=touch.preferredHeight=height;CasualSettingsStyle.Button(button,role);((UiButton)button).Face.ShadowOutside=true;
            var label=button.GetComponentInChildren<Text>();label.fontSize=Mathf.RoundToInt(SettingsLook.Fs+(chip?2:3));label.horizontalOverflow=HorizontalWrapMode.Overflow;
            if(icon!=null)
            {
                var image=CasualSettingsStyle.Icon(button.transform,icon);float size=label.fontSize*1.25f,group=size+8+label.preferredWidth;
                image.rectTransform.anchorMin=image.rectTransform.anchorMax=image.rectTransform.pivot=new Vector2(.5f,.5f);image.rectTransform.sizeDelta=new Vector2(size,size);image.rectTransform.anchoredPosition=new Vector2(-group/2+size/2,0);
                label.rectTransform.anchorMin=label.rectTransform.anchorMax=label.rectTransform.pivot=new Vector2(.5f,.5f);label.rectTransform.sizeDelta=new Vector2(label.preferredWidth+4,height);label.rectTransform.anchoredPosition=new Vector2(group/2-label.preferredWidth/2,0);label.alignment=TextAnchor.MiddleCenter;
            }
            return button;
        }
        // The mockup's .save line: a 15 px glyph and the muted note, 6 px apart, line-height 1.4.
        Text SavedNote(Transform parent,string text)
        {
            var row=Rect("Saved on device",parent);var layout=row.gameObject.AddComponent<HorizontalLayoutGroup>();layout.spacing=6;layout.childAlignment=TextAnchor.MiddleLeft;layout.childControlWidth=layout.childControlHeight=true;layout.childForceExpandWidth=layout.childForceExpandHeight=false;
            var icon=CasualSettingsStyle.Icon(row,"saved-device");var size=icon.gameObject.AddComponent<LayoutElement>();size.minWidth=size.preferredWidth=15;size.minHeight=size.preferredHeight=15;
            var label=CommonNote(row,text,18,UiSkin.Casual.Muted);SettingsLook.LineHeight(label,1.4f);label.GetComponent<LayoutElement>().flexibleWidth=1;return label;
        }
        RectTransform SettingsGrid(Transform body,string name)
        {var r=Rect(name,body);r.gameObject.AddComponent<SettingsChoiceGrid>().MinimumCellHeight=SettingsLook.Chip;return r;}
        RectTransform CommonScroll(string name,out RectTransform body)
        {
            var frame=Box(name,commonCard,Color.clear);var scroll=frame.gameObject.AddComponent<ScrollRect>();scroll.horizontal=false;scroll.movementType=ScrollRect.MovementType.Clamped;
            var viewport=Rect("Viewport",frame);Stretch(viewport);viewport.pivot=new Vector2(0,1);viewport.gameObject.AddComponent<RectMask2D>();scroll.viewport=viewport;
            body=Rect("Content",viewport);body.anchorMin=new Vector2(0,1);body.anchorMax=Vector2.one;body.pivot=new Vector2(.5f,1);body.sizeDelta=Vector2.zero;
            var layout=body.gameObject.AddComponent<VerticalLayoutGroup>();layout.childControlWidth=layout.childControlHeight=layout.childForceExpandWidth=true;layout.childForceExpandHeight=false;layout.spacing=SettingsLook.Gap;layout.padding=new RectOffset(2,8,2,12);
            body.gameObject.AddComponent<ContentSizeFitter>().verticalFit=ContentSizeFitter.FitMode.PreferredSize;scroll.content=body;
            // The mockup's 12 px scrollbar (track #efe6cf, gold thumb with an ink outline) takes its width from the body only when the content overflows.
            var barRoot=Rect("Scrollbar",frame);barRoot.anchorMin=new Vector2(1,0);barRoot.anchorMax=Vector2.one;barRoot.pivot=new Vector2(1,.5f);barRoot.sizeDelta=new Vector2(12,0);barRoot.anchoredPosition=Vector2.zero;
            CasualSettingsStyle.Surface(barRoot,UiSkin.Casual.EdictScrollTrack,Color.clear,7);
            var thumb=Rect("Scrollbar thumb",barRoot);Stretch(thumb);CasualSettingsStyle.Surface(thumb,UiSkin.Casual.GoldShade,UiSkin.Casual.Navy,7);
            var bar=barRoot.gameObject.AddComponent<Scrollbar>();bar.handleRect=thumb;bar.direction=Scrollbar.Direction.BottomToTop;scroll.verticalScrollbar=bar;
            scroll.verticalScrollbarVisibility=ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;scroll.verticalScrollbarSpacing=0;return frame;
        }
        Text CommonNote(Transform parent,string text,int size=22,Color? color=null)
        {return CasualSettingsStyle.Note(parent,text,size,color.HasValue?UiSkin.Casual.TextColor(color.Value):UiSkin.Casual.Navy,display:color.HasValue&&color.Value==gold);}
        RectTransform[] SettingsPanes=>new[]{screenPane,soundPane,languagePane,characterPane,accountPane,helpPane,combatPane};
        void SelectCombatTab()=>SelectSettingsTab(SettingsSection.Combat);
        void SelectSettingsTab(SettingsSection section)
        {
            if(SettingsPanes[(int)section]==null)section=SettingsSection.Screen;commonTab=section;commonSound=section==SettingsSection.Sound;
            for(int i=0;i<SettingsPanes.Length;i++)if(SettingsPanes[i]!=null)SettingsPanes[i].gameObject.SetActive(i==(int)section);
            foreach(var b in commonTabs.GetComponentsInChildren<Button>()){bool chosen=b.name=="settings-tab-"+section;UiTheme.Choice(b,chosen);b.GetComponent<SettingsTabView>().Select(chosen);}
            commonHeading.text=Loc.T(section==SettingsSection.Help?"게임 안내":section==SettingsSection.Combat?"전투 상태":"설정");commonScreenSize=Vector2.zero;RefreshScreenSettings();ReflowCommonPanel();if(commonSound)RefreshAudioSettings();
        }
        void RefreshScreenSettings()
        {
            if(commonModal==null)return;
            foreach(var choice in aspectButtons)CasualSettingsStyle.Choice(choice.button,choice.id==game.Aspect);
            aspectMessage.text=Loc.T(string.IsNullOrEmpty(game.AspectMessage)?Loc.F("현재 비율 · {0}",game.Aspect=="auto"?Loc.T("화면에 맞춤"):game.Aspect):game.AspectMessage);
            RefreshMapDisplayControl();RefreshViewDistanceControl();
            foreach(var choice in languageButtons)CasualSettingsStyle.Choice(choice.button,choice.code==game.Language.Language);
            languageMessage.text=Loc.T(string.IsNullOrEmpty(game.Language.Message)?game.LanguageLoadNotice:game.Language.Message);CasualSettingsStyle.Active(languageMessage.gameObject,languageMessage.text!="");
            foreach(var choice in characterButtons){bool chosen=choice.index==game.Store.Data.selectedHero;choice.button.interactable=game.CanChangeCharacterFromSettings&&!chosen;CasualSettingsStyle.Choice(choice.button,chosen,!game.CanChangeCharacterFromSettings);}
            if(characterNotice!=null)CasualSettingsStyle.NoticeState(characterNotice,!game.CanChangeCharacterFromSettings);
            if(characterMessage!=null)CasualSettingsStyle.Active(characterMessage.transform.parent.gameObject,characterMessage.text!="");
            if(characterNotice!=null)characterNotice.text=Loc.T(game.TutorialActive?"성소에 도착한 뒤 캐릭터를 바꿀 수 있습니다.":game.CanChangeCharacterFromSettings?"캐릭터를 변경하면 마을로 돌아갑니다. 진행 중인 사냥과 반복 사냥은 종료되며, 이미 획득한 장비와 보상은 보존됩니다.":"균열 사냥 중에는 캐릭터를 변경할 수 없습니다.");
            RefreshAccountPane();
        }
        void ReflowCommonPanel()
        {
            if(commonModal==null)return;var size=new Vector2(Screen.width,Screen.height);var safe=UiSafeArea.Current;var keyboard=TouchScreenKeyboard.visible?TouchScreenKeyboard.area:new Rect();
            float bottom=keyboard.width>0&&keyboard.xMin<safe.xMax&&keyboard.xMax>safe.xMin?Mathf.Clamp(keyboard.yMax,safe.yMin,safe.yMax):safe.yMin;
            if(commonLaidOut&&size==commonScreenSize&&safe==commonSafeArea&&Mathf.Abs(bottom-commonKeyboardTop)<1)return;
            Canvas.ForceUpdateCanvases();var scroll=SettingsPanes[(int)commonTab].GetComponent<ScrollRect>();var reading=DialogReadingAnchor.Capture(scroll);
            commonScreenSize=size;commonSafeArea=safe;commonKeyboardTop=bottom;commonSafe.anchorMin=new Vector2(safe.xMin/Mathf.Max(1,size.x),bottom/Mathf.Max(1,size.y));commonSafe.anchorMax=new Vector2(safe.xMax/Mathf.Max(1,size.x),safe.yMax/Mathf.Max(1,size.y));commonSafe.offsetMin=commonSafe.offsetMax=Vector2.zero;
            Canvas.ForceUpdateCanvases();var available=commonSafe.rect.size;bool landscape=UiSafeArea.Frame.width>UiSafeArea.Frame.height;
            float scale=UiMock.CanvasScale(safe)*InterfaceFactor;commonModal.GetComponent<CanvasScaler>().scaleFactor=scale;float edge=SettingsLook.Margin;float w=Mathf.Max(1,available.x*(landscape?.5f:1)-edge*2),h=Mathf.Max(1,available.y-edge*2);commonCard.sizeDelta=new Vector2(w,h);commonCard.anchoredPosition=new Vector2(landscape?available.x*.25f:0,0);
            bool showWorld=landscape&&game.World!=null&&game.World.HasPresentedPlayer;
            commonWorld.gameObject.SetActive(showWorld);commonBackdrop.interactable=landscape;commonBackdrop.image.color=showWorld?Color.clear:new Color(.025f,.035f,.049f,landscape?.75f:1);
            foreach(var gate in commonBackgrounds)if(gate.group!=null&&gate.group.transform!=aspectMask)gate.group.alpha=showWorld?0:gate.alpha;
            if(showWorld)game.World.ShowSettingsWorld(new Rect(commonSafe.anchorMin.x,commonSafe.anchorMin.y,(commonSafe.anchorMax.x-commonSafe.anchorMin.x)*.5f,commonSafe.anchorMax.y-commonSafe.anchorMin.y));
            else game.World?.RestoreSettingsWorld();
            // The mockup's frame: 3 px border and the board padding, a header of the round-button height, a rule, the tab row, then the scrolling body.
            bool compact=SettingsLook.Board==UiBoard.Landscape;float rb=SettingsLook.Rb,inner=SettingsLook.Border+SettingsLook.Pad,gap=SettingsLook.Gap,tabHeight=SettingsLook.TabHeight;
            var tabs=commonTabs.GetComponentsInChildren<Button>();float sepY=inner+rb+gap-2,tabsY=sepY+1+gap,top=tabsY+tabHeight+gap+6;
            Span(commonHeading.rectTransform,inner+rb+10,inner,inner+rb+10,rb);commonHeading.fontSize=Mathf.RoundToInt(SettingsLook.Title);Right((RectTransform)commonClose.transform,inner,inner,rb,rb);
            var divider=commonCard.Find("Settings divider");if(divider==null)divider=HuntEdictDecor.Create(commonCard,"Settings divider",0,0,10,1,"header-divider",UiSkin.Casual);Place((RectTransform)divider,inner,sepY,w-2*inner,1);
            Place(commonTabs,inner,tabsY,w-2*inner,tabHeight);
            commonGem.anchorMin=commonGem.anchorMax=new Vector2(.5f,1);commonGem.pivot=new Vector2(.5f,.5f);commonGem.sizeDelta=new Vector2(18,18);commonGem.anchoredPosition=new Vector2(0,1);commonGem.gameObject.SetActive(!compact);
            foreach(var grid in commonModal.GetComponentsInChildren<SettingsChoiceGrid>(true))grid.MinimumCellHeight=SettingsLook.Chip;
            foreach(var tab in tabs)tab.GetComponentInChildren<Text>().fontSize=Mathf.RoundToInt(SettingsLook.TabFont);SettingsTabView.Arrange(commonTabs,w-2*inner,tabHeight,!landscape,compact,gap/2);
            foreach(var pane in SettingsPanes)if(pane!=null)Place(pane,inner,top,w-2*inner+6,Mathf.Max(1,h-top-inner));Canvas.ForceUpdateCanvases();
            Canvas.ForceUpdateCanvases();if(commonLaidOut)reading?.Restore();
            // A thumb first clipped below the short landscape viewport must rebuild when it becomes visible.
            foreach(var slider in commonModal.GetComponentsInChildren<Slider>())slider.handleRect.GetComponent<Graphic>().SetVerticesDirty();
            commonLaidOut=true;
        }
        public void CloseCommonPanel()
        {
            if(commonModal==null)return;game.Audio?.SavePreferences();game.World?.RestoreSettingsWorld();var old=commonModal;commonModal=null;ContentWindowHost.Detach(old);old.gameObject.SetActive(false);Destroy(old.gameObject);
            foreach(var background in commonBackgrounds)if(background.group!=null)background.group.alpha=background.alpha;commonBackgrounds.Clear();
        }
        void UpdateCommonPanel()
        {
            if(root!=null)ApplySafeArea();if(commonModal==null)return;ReflowCommonPanel();RefreshScreenSettings();if(commonSound)RefreshAudioSettings();

        }
    }
}
