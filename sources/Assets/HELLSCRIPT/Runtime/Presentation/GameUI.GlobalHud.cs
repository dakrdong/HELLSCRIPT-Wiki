using System;
using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    public sealed partial class GameUI
    {
        GlobalHudView globalHud;
        GlobalHudSnapshot hudPreview;
        float globalClock;
        RectTransform hudPanel;
        Rect hudPanelSafe;
        float hudPanelFactor;
        RectTransform hudCard;
        Text hudHeading;
        RectTransform hudClose;
        public GlobalHudView GlobalHud=>globalHud;
        bool ReserveGlobalHudSpace=>Page!="runes"&&Page!="battle"&&Page!="plaza"&&Page!="title"&&Page!="characters"&&Page!="recovery";
        void InitializeGlobalHud()
        {
            if(game.Store==null)return;
            var obj=new GameObject("Global observation HUD",typeof(RectTransform));obj.transform.SetParent(transform,false);
            globalHud=obj.AddComponent<GlobalHudView>();globalHud.Initialize(font,null);
            globalHud.EffectSelected=effect=>ShowHudInformation(effect.name,EffectDescription(effect),effect.id);
            globalHud.SlotSelected=ShowHudSlot;
            globalHud.AllEffectsSelected=ShowHudEffects;
            globalHud.EdictSelected=()=>
            {
                if(game.TutorialActive&&!GuideActive)
                {
                    if(!PrologueEdictOpen){ShowToast(Loc.T("목소리가 부를 때 사냥 칙령을 열 수 있습니다."));return;}
                    if(game.Combat.State.tutorialPhase==ProloguePhase.EdictLesson){ShowEdictSkillTree();return;}
                }
                ToggleEdictQuick();
            };globalHud.PotionSettingsSelected=TogglePotionQuick;globalHud.SkillTapped=SkillSlotTapped;globalHud.SkillHeld=SkillSlotHeld;globalHud.PotionTapped=PotionSlotTapped;globalHud.PotionHeld=PotionSlotHeld;
        }
        static string EffectDescription(HudEffectState effect)=>effect.description+"\n"+(effect.permanent?Loc.T("상시"):Loc.F("남은 시간 {0}초",GlobalHudSnapshot.TimeLabel(effect.remaining)))+"\n"+Loc.F("중첩 {0} · 출처 {1}",effect.stacks,effect.id);
        void RefreshGlobalHud()
        {
            if(globalHud==null)return;
            bool visible=!game.DisplayDimmed&&Page!="runes"&&Page!="title"&&Page!="characters"&&Page!="recovery";
            globalHud.gameObject.SetActive(visible||hudPreview!=null);
            if(globalHud.gameObject.activeSelf)globalHud.SetSnapshot(hudPreview??GlobalHudSnapshot.Read(game),InterfaceFactor,hudPreview==null&&ReserveGlobalHudSpace,LiveJournalHudInset);
            SyncEdictQuick();
        }
        void TickGlobalHud()
        {
            if(hudPanel!=null&&(hudPanelSafe!=UiSafeArea.Current||hudPanelFactor!=InterfaceFactor))ReflowHudPanel();
            globalClock+=Time.unscaledDeltaTime;
            if(globalClock>=.1f){globalClock=0;RefreshGlobalHud();}
        }
        public void SetHudPreview(GlobalHudSnapshot snapshot)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            hudPreview=snapshot;RefreshGlobalHud();
#endif
        }
        void AddSkillInspectionIcon(Transform parent,string id)
        {
            if(!SkillIconAssets.TryResolve(id,out bool passive,out var sprite))return;
            var row=Row(parent,72);var icon=SkillIconView.Create(row,passive);Place(icon.Rect,0,2,64,64);icon.SetCasualHudFrame(status:passive);icon.SetSprite(sprite);
        }
        void ShowHudInformation(string title,string body,string skillId=null)
        {ShowHudPanel(title,parent=>{AddSkillInspectionIcon(parent,skillId);Note(parent,body,20,Mathf.Max(120,body.Split('\n').Length*32),pale);});}
        void ShowHudSlot(HudSlotState slot)
        {
            string description=slot.description+(slot.count>=0?"\n"+Loc.F("보유 수량 {0}",slot.count):"")+(slot.remaining>0?"\n"+Loc.F("재사용 대기 {0}초",GlobalHudSnapshot.TimeLabel(slot.remaining)):"");
            ShowHudPanel(slot.name,parent=>
            {
                AddSkillInspectionIcon(parent,slot.id);
                Note(parent,description,20,Mathf.Max(150,description.Split('\n').Length*36),pale);
                if(slot.id!=null&&slot.id.StartsWith("PE-",StringComparison.Ordinal)&&game.Active&&slot.count>0)
                {
                    var use=BigButton(parent,"지금 사용",()=>{if(game.Combat.TryUseUtilitySlot(slot.id)){game.Save();CloseHudPanel();RefreshGlobalHud();}},true);
                    use.name="jeweler-use-potion";use.interactable=slot.remaining<=0;
                }
                if(slot.id!=null&&PotionCatalog.IsUtility(slot.id)&&game.Combat?.State.training>=0&&!game.ComparisonRun)
                {
                    Note(parent,"훈련용 보조 물약 선택 · 실제 재고는 변경하지 않습니다.",18,72,gold);
                    foreach(var def in PotionCatalog.All)if(def.duration>0)
                    {string id=def.id;BigButton(parent,GemElixirs.Name(def),()=>{if(game.Combat.EquipTrainingPotion(id)){CloseHudPanel();RefreshGlobalHud();}else ShowToast("활성 보조 효과가 끝난 뒤 물약을 변경해 주세요.");});}
                }
            });
        }
        void ShowHudEffects()
        {
            var effects=globalHud?.Snapshot?.ObservationEffects();if(effects==null)return;
            ShowHudPanel("현재 상태",parent=>{foreach(var effect in effects){AddSkillInspectionIcon(parent,effect.id);Note(parent,effect.name+"\n"+EffectDescription(effect),19,150,pale);}});
        }
        Action hudPanelClosed;
        // Every HUD inspection window (effect and slot information, potion and skill popups) is casual.
        void ShowHudPanel(string title,Action<RectTransform> build,Action onClose=null,UiSkin skin=null)
        {
            skin=skin??UiSkin.Casual;
            CloseHudPanel();hudPanelClosed=onClose;
            hudPanel=Rect("HUD inspection",transform);var c=hudPanel.gameObject.AddComponent<Canvas>();c.renderMode=RenderMode.ScreenSpaceOverlay;c.sortingOrder=108;
            ApplyHudPanelScaler(hudPanel.gameObject.AddComponent<CanvasScaler>());hudPanel.gameObject.AddComponent<GraphicRaycaster>();ContentWindowHost.Attach(hudPanel,CloseHudPanel);
            var backdrop=Button(hudPanel,"",CloseHudPanel,new Color(.01f,.015f,.02f,.48f));backdrop.name="hud-details-backdrop";UiTheme.Backdrop(backdrop);Stretch((RectTransform)backdrop.transform);
            var card=Box("HUD inspection card",hudPanel,UiTheme.Panel);
            var heading=Label(card,title,24,gold);
            var close=Button(card,"×",CloseHudPanel,Color.clear);close.name="hud-details-close";
            hudCard=card;hudHeading=heading;hudClose=(RectTransform)close.transform;ReflowHudPanel();
            var vp=Rect("HUD detail scroll",card);Inset(vp,16,16,64,18);vp.gameObject.AddComponent<Image>().color=Color.clear;vp.gameObject.AddComponent<RectMask2D>();
            var body=Rect("HUD detail body",vp);body.anchorMin=new Vector2(0,1);body.anchorMax=Vector2.one;body.pivot=new Vector2(.5f,1);body.sizeDelta=Vector2.zero;
            var v=body.gameObject.AddComponent<VerticalLayoutGroup>();v.spacing=8;v.childControlWidth=true;v.childForceExpandWidth=true;v.childControlHeight=true;v.childForceExpandHeight=false;
            body.gameObject.AddComponent<ContentSizeFitter>().verticalFit=ContentSizeFitter.FitMode.PreferredSize;
            var sr=vp.gameObject.AddComponent<ScrollRect>();sr.horizontal=false;sr.movementType=ScrollRect.MovementType.Clamped;sr.content=body;sr.viewport=vp;build(body);
            if(skin!=null)
            {
                card.GetComponent<Image>().color=Color.clear;
                var face=UiLayout.Rect("Casual inspection surface",card);UiLayout.Stretch(face);face.SetAsFirstSibling();
                face.gameObject.AddComponent<UiWindowFrame>().SetSkin(skin);face.GetComponent<Image>().raycastTarget=false;
                foreach(var image in body.GetComponentsInChildren<Image>())
                    if(image.transform.parent==body&&image.GetComponent<Button>()==null)image.color=Color.clear;
                foreach(var label in card.GetComponentsInChildren<Text>())skin.ApplyLabel(label,label==heading||label.GetComponentInParent<Button>()!=null);
                // The close button is built as an Item button, whose face draws nothing at rest; Quiet lets the coral CasualClose face show.
                ((UiButton)close).Configure(UiButtonRole.Quiet);
                foreach(var button in card.GetComponentsInChildren<UiButton>())
                {
                    skin.ApplyChrome(button,button==close);
                    var border=button.GetComponent<UIRectBorder>();if(border!=null)border.enabled=false;
                }
                close.GetComponentInChildren<Text>().color=skin.White;
            }
        }
        // The approved inspection plates draw the phone boards at 0.72 of the PC unit (card 360 px wide), not at the shared 0.5 scale.
        void ApplyHudPanelScaler(CanvasScaler scaler)
        {
            ApplyScaler(scaler,true);var layout=globalHud?.Layout;if(layout==null||layout.landscape&&!layout.compact)return;
            var safe=UiSafeArea.Current;float reference=layout.landscape?440f/450:440f/405;
            scaler.scaleFactor=UiTheme.Scale(safe)/reference*.72f*InterfaceFactor;
        }
        void ReflowHudPanel()
        {
            var c=hudPanel.GetComponent<Canvas>();ApplyHudPanelScaler(hudPanel.GetComponent<CanvasScaler>());Canvas.ForceUpdateCanvases();
            hudPanelSafe=UiSafeArea.Current;hudPanelFactor=InterfaceFactor;
            float scale=c.scaleFactor,w=Mathf.Min(500,hudPanelSafe.width/scale-24),h=Mathf.Min(620,hudPanelSafe.height/scale-32);
            // The approved plate's face is centred and its 4 px shadow hangs below, so the whole rect sits half the shadow lower.
            hudCard.anchorMin=hudCard.anchorMax=hudCard.pivot=new Vector2(.5f,.5f);hudCard.anchoredPosition=(hudPanelSafe.center-new Vector2(Screen.width,Screen.height)*.5f)/scale-new Vector2(0,2/scale);hudCard.sizeDelta=new Vector2(w,h);
            // The close button is 44 px on every board; on the 0.72 phone plates that is more canvas units, set 10 units in from the corner.
            float unit=hudPanelSafe.width>0&&globalHud?.Layout!=null&&(!globalHud.Layout.landscape||globalHud.Layout.compact)?1/.72f:1;
            Place(hudHeading.rectTransform,20,12,w-84,38);Place(hudClose,w-10-44*unit,8,44*unit,44*unit);
        }
        void CloseHudPanel()
        {
            if(hudPanel==null)return;var old=hudPanel;hudPanel=null;var closed=hudPanelClosed;hudPanelClosed=null;ContentWindowHost.Detach(old);old.gameObject.SetActive(false);Destroy(old.gameObject);closed?.Invoke();
        }
    }
}
