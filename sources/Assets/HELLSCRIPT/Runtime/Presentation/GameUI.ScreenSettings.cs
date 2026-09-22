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
        enum SettingsSection { Screen,Sound,Language,Character,Help,Combat }
        SettingsSection commonTab;
        RectTransform commonModal,commonSafe,commonCard,commonTabs,commonWorld;Button commonClose,commonBackdrop;
        RectTransform screenPane,languagePane,characterPane,helpPane,combatPane,scaleChoiceRow;
        Text commonHeading,scaleValue,scaleMessage,aspectMessage,languageMessage,characterMessage;
        SettingsStepSlider scaleSlider;Button scaleRetry;GameObject commonPreviousSelection;
        Vector2 commonScreenSize;Rect commonSafeArea;float commonKeyboardTop;bool commonLaidOut;
        readonly List<(CanvasGroup group,bool interactable,bool raycasts,float alpha)> commonInputGates=new List<(CanvasGroup,bool,bool,float)>();
        readonly List<(string id,Button button)> aspectButtons=new List<(string,Button)>();
        readonly List<(string code,Button button)> languageButtons=new List<(string,Button)>();
        public bool CommonPanelOpen=>commonModal!=null||hudPanel!=null||idleIntroductionOpen||PlayInventoryOpen||EquipmentShopOpen||BlacksmithOpen||RuneMasterOpen;
        public bool BlocksRepeat=>runeSession||CommonPanelOpen||presetModal!=null||root!=null&&root.Find("Confirm")!=null;
        public void ShowScreenSettings()=>ShowCommonPanel(false);
        public void ShowCombatOverview(){ShowCommonPanel(false);SelectCombatTab();}
        void ShowHelp()=>ShowCommonPanel(true);
        void ShowCommonPanel(bool help)
        {
            game.ExitIdle();ResetTownInput();game.Town?.Cancel();
            if(commonModal!=null){SelectSettingsTab(help?SettingsSection.Help:SettingsSection.Screen);return;}
            commonPreviousSelection=EventSystem.current?.currentSelectedGameObject;EventSystem.current?.SetSelectedGameObject(null);commonInputGates.Clear();
            foreach(var existing in GetComponentsInChildren<Canvas>())
            {if(!existing.TryGetComponent(out CanvasGroup group))group=existing.gameObject.AddComponent<CanvasGroup>();commonInputGates.Add((group,group.interactable,group.blocksRaycasts,group.alpha));group.interactable=group.blocksRaycasts=false;}
            commonModal=Rect("Screen and help modal",transform);var canvas=commonModal.gameObject.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=120;canvas.pixelPerfect=true;
            ApplyScaler(commonModal.gameObject.AddComponent<CanvasScaler>(),true);commonModal.gameObject.AddComponent<GraphicRaycaster>();
            commonBackdrop=Button(commonModal,"",CloseCommonPanel,new Color(.025f,.035f,.049f,1));commonBackdrop.name="settings-backdrop";Stretch((RectTransform)commonBackdrop.transform);commonBackdrop.transition=Selectable.Transition.None;
            commonSafe=Rect("Common safe area",commonModal);Stretch(commonSafe);
            commonWorld=Rect("Settings world area",commonSafe);commonWorld.anchorMin=Vector2.zero;commonWorld.anchorMax=new Vector2(.5f,1);commonWorld.offsetMin=commonWorld.offsetMax=Vector2.zero;
            var hint=Label(commonWorld,"배경을 누르면 닫힙니다.",17,pale,TextAnchor.MiddleCenter);hint.rectTransform.anchorMin=new Vector2(0,0);hint.rectTransform.anchorMax=new Vector2(1,0);hint.rectTransform.sizeDelta=new Vector2(-24,40);hint.rectTransform.anchoredPosition=new Vector2(0,28);
            commonCard=Box("Common dialog",commonSafe,ink);commonCard.anchorMin=commonCard.anchorMax=commonCard.pivot=new Vector2(.5f,.5f);
            commonHeading=Label(commonCard,"설정",26,gold);Span(commonHeading.rectTransform,18,8,18,40);commonTabs=Rect("Common categories",commonCard);
            foreach(var entry in new[]{(SettingsSection.Screen,"화면"),(SettingsSection.Sound,"소리"),(SettingsSection.Language,"언어"),(SettingsSection.Character,"캐릭터 변경")})
            {var tab=entry.Item1;var b=Button(commonTabs,entry.Item2,()=>SelectSettingsTab(tab));b.name="settings-tab-"+tab;b.GetComponentInChildren<Text>().fontSize=18;}
            BuildDisplayPane();BuildSoundPane();BuildLanguagePane();BuildCharacterPane();BuildInformationPanes();
            commonClose=Button(commonCard,"X",CloseCommonPanel);commonClose.name="settings-close";commonClose.GetComponentInChildren<Text>().fontSize=24;
            commonLaidOut=false;SelectSettingsTab(help?SettingsSection.Help:SettingsSection.Screen);ReflowCommonPanel();
        }
        void BuildDisplayPane()
        {
            screenPane=CommonScroll("화면 설정 본문",out var body);BuildViewDistanceControl(body);BuildOverlayMapControl(body);CommonNote(body,"화면 비율",24,gold);
            CommonNote(body,"비율을 선택하면 바로 적용합니다. PC에서는 창 크기를 맞추고, 모바일에서는 화면을 회전한 뒤 선택한 비율로 표시합니다.",19);aspectButtons.Clear();
            var automatic=BigButton(body,"화면에 맞춤",()=>game.ApplyAspect("auto"));automatic.name="settings-aspect-auto";aspectButtons.Add(("auto",automatic));
            for(int side=0;side<2;side++)
            {
                CommonNote(body,side==0?"가로 비율":"세로 비율",21,gold);var choices=SettingsGrid(body,"Aspect choices "+side);
                foreach(string id in side==0?DisplayAspect.Landscape:DisplayAspect.Portrait)
                {var b=Button(choices,id,()=>game.ApplyAspect(id));b.name="settings-aspect-"+id;b.GetComponentInChildren<Text>().fontSize=18;aspectButtons.Add((id,b));}
            }
            aspectMessage=CommonNote(body,"",18,muted);CommonNote(body,"글자 크기",24,gold);
            CommonNote(body,"50~150% 범위에서 5%씩 조절합니다. 슬라이더를 움직이거나 스크롤하고, − / + 버튼으로 한 단계씩 바꿀 수 있습니다.",19);
            scaleChoiceRow=Rect("Text size choices",body);scaleChoiceRow.gameObject.AddComponent<LayoutElement>().preferredHeight=124;
            scaleValue=Label(scaleChoiceRow,"",24,gold,TextAnchor.MiddleCenter);Span(scaleValue.rectTransform,60,0,60,36);
            var minus=Button(scaleChoiceRow,"−",()=>SetReadingStep(-1));Place((RectTransform)minus.transform,0,0,48,44);minus.name="settings-scale-down";
            var plus=Button(scaleChoiceRow,"+",()=>SetReadingStep(1));Right((RectTransform)plus.transform,0,0,48,44);plus.name="settings-scale-up";
            var area=Box("settings-scale-slider",scaleChoiceRow,Color.clear);Span(area,0,48,0,68);scaleSlider=area.gameObject.AddComponent<SettingsStepSlider>();scaleSlider.minValue=0;scaleSlider.maxValue=20;scaleSlider.wholeNumbers=true;
            var track=Box("Track",area,new Color(.09f,.12f,.15f));track.anchorMin=new Vector2(0,.5f);track.anchorMax=new Vector2(1,.5f);track.sizeDelta=new Vector2(-36,8);
            var fill=Box("Fill",track,gold);Stretch(fill);fill.GetComponent<Image>().raycastTarget=false;scaleSlider.fillRect=fill;
            var travel=Rect("Handle area",area);travel.anchorMin=new Vector2(0,.5f);travel.anchorMax=new Vector2(1,.5f);travel.sizeDelta=new Vector2(-36,0);
            var handle=Box("Handle",travel,pale);handle.anchorMin=handle.anchorMax=new Vector2(0,.5f);handle.sizeDelta=new Vector2(32,44);scaleSlider.handleRect=handle;scaleSlider.targetGraphic=handle.GetComponent<Image>();
            scaleSlider.onValueChanged.AddListener(step=>{game.ApplyInterfaceScale(50+Mathf.RoundToInt(step)*5);RefreshScreenSettings();});
            scaleSlider.Released=()=>{ApplyInterfaceScale();Canvas.ForceUpdateCanvases();DialogReadingAnchor.Show(scaleChoiceRow);};
            scaleMessage=CommonNote(body,"",18,muted);scaleRetry=BigButton(body,"저장 재시도",()=>{game.InterfaceScale.RetrySave();RefreshScreenSettings();});CommonNote(body,"화면·글자 크기 설정은 이 기기에 저장됩니다.",18,muted);
        }
        void SetReadingStep(int delta){game.ApplyInterfaceScale(game.InterfaceScale.Percent+delta*5);RefreshScreenSettings();Canvas.ForceUpdateCanvases();DialogReadingAnchor.Show(scaleChoiceRow);}
        void BuildLanguagePane()
        {
            languagePane=CommonScroll("언어 설정 본문",out var body);CommonNote(body,"언어",24,gold);CommonNote(body,"사용할 언어를 선택하세요. 선택한 언어는 이 기기에 저장됩니다.",20);languageButtons.Clear();
            foreach(string code in new[]{"en","ko"}){var b=BigButton(body,LanguageOptions.Find(code).NativeName,()=>game.ApplyLanguage(code));b.name="settings-language-"+code;languageButtons.Add((code,b));}
            languageMessage=CommonNote(body,"",19,muted);
        }
        void BuildCharacterPane()
        {
            characterPane=CommonScroll("캐릭터 변경 본문",out var body);CommonNote(body,"캐릭터 변경",24,gold);
            CommonNote(body,"캐릭터를 변경하면 마을로 돌아갑니다. 진행 중인 사냥과 반복 사냥은 종료되며, 이미 획득한 장비와 보상은 보존됩니다.",20);
            foreach(int index in new[]{0,2,1})
            {
                var hero=game.Store.Data.heroes[index];bool selected=game.Store.Data.selectedHero==index;
                var b=BigButton(body,Loc.F("{0} · Lv.{1}",game.catalog.classNames[(int)hero.heroClass],hero.level),()=>{if(!game.ChangeCharacterFromSettings(index)&&characterMessage!=null)characterMessage.text=Loc.T(game.Store.Error);},selected);
                b.name="settings-character-"+index;b.interactable=!selected;
            }
            characterMessage=CommonNote(body,"",19,gold);
        }
        RectTransform SettingsGrid(Transform body,string name)
        {var r=Rect(name,body);r.gameObject.AddComponent<SettingsChoiceGrid>();return r;}
        RectTransform CommonScroll(string name,out RectTransform body)
        {
            var frame=Box(name,commonCard,panel);var scroll=frame.gameObject.AddComponent<ScrollRect>();scroll.horizontal=false;scroll.movementType=ScrollRect.MovementType.Clamped;
            var viewport=Rect("Viewport",frame);Stretch(viewport);viewport.gameObject.AddComponent<RectMask2D>();scroll.viewport=viewport;
            body=Rect("Content",viewport);body.anchorMin=new Vector2(0,1);body.anchorMax=Vector2.one;body.pivot=new Vector2(.5f,1);body.sizeDelta=Vector2.zero;
            var layout=body.gameObject.AddComponent<VerticalLayoutGroup>();layout.childControlWidth=layout.childControlHeight=layout.childForceExpandWidth=true;layout.childForceExpandHeight=false;layout.spacing=14;layout.padding=new RectOffset(18,18,18,18);
            body.gameObject.AddComponent<ContentSizeFitter>().verticalFit=ContentSizeFitter.FitMode.PreferredSize;scroll.content=body;return frame;
        }
        Text CommonNote(Transform parent,string text,int size=22,Color? color=null)
        {var label=Label(parent,text,size,color??pale);label.alignment=TextAnchor.UpperLeft;label.gameObject.AddComponent<LayoutElement>().minHeight=28;return label;}
        RectTransform[] SettingsPanes=>new[]{screenPane,soundPane,languagePane,characterPane,helpPane,combatPane};
        void SelectCombatTab()=>SelectSettingsTab(SettingsSection.Combat);
        void SelectSettingsTab(SettingsSection section)
        {
            if(SettingsPanes[(int)section]==null)section=SettingsSection.Screen;commonTab=section;commonSound=section==SettingsSection.Sound;
            for(int i=0;i<SettingsPanes.Length;i++)if(SettingsPanes[i]!=null)SettingsPanes[i].gameObject.SetActive(i==(int)section);
            foreach(var b in commonTabs.GetComponentsInChildren<Button>())b.GetComponent<Image>().color=b.name=="settings-tab-"+section?gold*.4f:panel;
            commonHeading.text=Loc.T(section==SettingsSection.Help?"게임 안내":section==SettingsSection.Combat?"전투 상태":"설정");commonScreenSize=Vector2.zero;RefreshScreenSettings();ReflowCommonPanel();if(commonSound)RefreshAudioSettings();
        }
        void RefreshScreenSettings()
        {
            if(commonModal==null)return;
            foreach(var choice in aspectButtons)choice.button.GetComponent<Image>().color=choice.id==game.Aspect?gold*.4f:new Color(.14f,.17f,.2f);
            aspectMessage.text=Loc.T(string.IsNullOrEmpty(game.AspectMessage)?Loc.F("현재 비율 · {0}",game.Aspect=="auto"?Loc.T("화면에 맞춤"):game.Aspect):game.AspectMessage);
            RefreshOverlayMapControl();RefreshViewDistanceControl();var scale=game.InterfaceScale;scaleSlider.SetValueWithoutNotify((scale.Percent-50)/5);scaleValue.text=scale.Percent+"%";scaleMessage.text=string.IsNullOrEmpty(scale.Message)?Loc.T(game.InterfaceScaleLoadNotice):scale.Message;scaleRetry.gameObject.SetActive(scale.CanRetrySave);
            foreach(var choice in languageButtons)choice.button.GetComponent<Image>().color=choice.code==game.Language.Language?gold*.4f:new Color(.14f,.17f,.2f);
            languageMessage.text=Loc.T(string.IsNullOrEmpty(game.Language.Message)?game.LanguageLoadNotice:game.Language.Message);
        }
        void ReflowCommonPanel()
        {
            if(commonModal==null)return;var size=new Vector2(Screen.width,Screen.height);var safe=UiSafeArea.Current;var keyboard=TouchScreenKeyboard.visible?TouchScreenKeyboard.area:new Rect();
            float bottom=keyboard.width>0&&keyboard.xMin<safe.xMax&&keyboard.xMax>safe.xMin?Mathf.Clamp(keyboard.yMax,safe.yMin,safe.yMax):safe.yMin;
            if(commonLaidOut&&size==commonScreenSize&&safe==commonSafeArea&&Mathf.Abs(bottom-commonKeyboardTop)<1)return;
            Canvas.ForceUpdateCanvases();var scroll=SettingsPanes[(int)commonTab].GetComponent<ScrollRect>();var reading=DialogReadingAnchor.Capture(scroll);
            commonScreenSize=size;commonSafeArea=safe;commonKeyboardTop=bottom;commonSafe.anchorMin=new Vector2(safe.xMin/Mathf.Max(1,size.x),bottom/Mathf.Max(1,size.y));commonSafe.anchorMax=new Vector2(safe.xMax/Mathf.Max(1,size.x),safe.yMax/Mathf.Max(1,size.y));commonSafe.offsetMin=commonSafe.offsetMax=Vector2.zero;
            Canvas.ForceUpdateCanvases();var available=commonSafe.rect.size;bool landscape=UiSafeArea.Frame.width>UiSafeArea.Frame.height;
            float w=Mathf.Max(1,available.x*(landscape?.5f:1)),h=Mathf.Max(1,available.y);commonCard.sizeDelta=new Vector2(w,h);commonCard.anchoredPosition=new Vector2(landscape?available.x*.25f:0,0);
            bool showWorld=landscape&&game.World!=null&&game.World.HasPresentedPlayer;
            commonWorld.gameObject.SetActive(showWorld);commonBackdrop.interactable=landscape;commonBackdrop.image.color=showWorld?Color.clear:new Color(.025f,.035f,.049f,landscape?.75f:1);
            foreach(var gate in commonInputGates)if(gate.group!=null&&gate.group.transform!=aspectMask)gate.group.alpha=showWorld?0:gate.alpha;
            if(showWorld)game.World.ShowSettingsWorld(new Rect(commonSafe.anchorMin.x,commonSafe.anchorMin.y,(commonSafe.anchorMax.x-commonSafe.anchorMin.x)*.5f,commonSafe.anchorMax.y-commonSafe.anchorMin.y));
            else game.World?.RestoreSettingsWorld();
            bool shortWindow=h<400;float heading=56,tabHeight=shortWindow?40:48;int columns=w<620?2:4,rows=Mathf.CeilToInt(4f/columns);float top=heading+rows*tabHeight+8;
            Span(commonHeading.rectTransform,18,8,74,40);commonHeading.fontSize=shortWindow?22:26;Right((RectTransform)commonClose.transform,8,6,44,44);Place(commonTabs,12,heading,w-24,rows*tabHeight);
            var tabs=commonTabs.GetComponentsInChildren<Button>();
            for(int i=0;i<tabs.Length;i++){float cell=(w-24)/columns;Place((RectTransform)tabs[i].transform,(i%columns)*cell+3,(i/columns)*tabHeight,cell-6,tabHeight-6);tabs[i].GetComponentInChildren<Text>().fontSize=shortWindow?16:18;}
            foreach(var pane in SettingsPanes)if(pane!=null)Place(pane,12,top,w-24,Mathf.Max(1,h-top-12));Canvas.ForceUpdateCanvases();
            Canvas.ForceUpdateCanvases();if(commonLaidOut)reading?.Restore();
            // A thumb first clipped below the short landscape viewport must rebuild when it becomes visible.
            foreach(var slider in commonModal.GetComponentsInChildren<Slider>())slider.handleRect.GetComponent<Graphic>().SetVerticesDirty();
            commonLaidOut=true;
        }
        public void CloseCommonPanel()
        {
            if(commonModal==null)return;game.Audio?.SavePreferences();game.World?.RestoreSettingsWorld();var old=commonModal;commonModal=null;old.gameObject.SetActive(false);Destroy(old.gameObject);
            foreach(var gate in commonInputGates)if(gate.group!=null){gate.group.interactable=gate.interactable;gate.group.blocksRaycasts=gate.raycasts;gate.group.alpha=gate.alpha;}commonInputGates.Clear();
            if(commonPreviousSelection!=null&&commonPreviousSelection.activeInHierarchy)EventSystem.current?.SetSelectedGameObject(commonPreviousSelection);commonPreviousSelection=null;
        }
        void UpdateCommonPanel()
        {
            if(root!=null)ApplySafeArea();if(commonModal==null)return;ReflowCommonPanel();RefreshScreenSettings();if(commonSound)RefreshAudioSettings();
            if(Keyboard.current?.escapeKey.wasPressedThisFrame==true)CloseCommonPanel();
        }
    }
}
