using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Hellscript
{
    // The Hunt Edict tab sheet of the battle HUD: one panel above the icon row that shows a tab's groups as chips and the options of the chosen
    // group as live controls (choice chips, a slider with steppers, multi-select chips, an order list). Every change is saved at once through the
    // edict transaction and read back, so dependent options appear or go.
    public sealed partial class GameUI
    {
        sealed class PointerRelease:MonoBehaviour,IPointerDownHandler,IPointerUpHandler,ISubmitHandler,IDeselectHandler
        {
            public Action Released;bool pointerHeld;
            public void OnPointerDown(PointerEventData e)=>pointerHeld=true;
            public void OnPointerUp(PointerEventData e){pointerHeld=false;Released?.Invoke();}
            public void OnSubmit(BaseEventData e){if(!pointerHeld)Released?.Invoke();}
            public void OnDeselect(BaseEventData e){if(!pointerHeld)Released?.Invoke();}
        }
        string sheetTab="";int sheetGroup;
        RectTransform sheetCard,sheetContent;ScrollRect sheetScroll;Button sheetUndoButton,sheetPlayButton;
        readonly List<HuntEdictLoadout> sheetUndo=new List<HuntEdictLoadout>();
        bool sheetPlaying;float sheetInner;

        void CloseEdictSheet()
        {
            sheetTab="";sheetEntranceKey="";sheetCard=null;sheetContent=null;sheetScroll=null;sheetUndoButton=sheetPlayButton=null;sheetUndo.Clear();sheetPlaying=false;
            game.World?.HideEdictForecast();
        }
        // The tab icons are drawn smaller than the Hunt Edict button they fan out from.
        const float TabIconScale=.8f;
        static string EdictTabArt(string tab)=>"menu-edict-"+(tab=="autoEquip"?"auto-equip":tab);
        static string EdictTabTitle(string tab)=>tab=="autoEquip"?"추천 장비":HuntEdictUiCatalog.TabTitles[Array.IndexOf(HuntEdictUiCatalog.Tabs,tab)];
        bool EdictTabAvailable(string tab)=>tab=="skills"||tab=="repeat"||tab=="autoEquip"||tab=="presets"&&EdictPresetsWindow.Available(game.Store.Data)||HuntEdictHudRows.Groups(tab,game.Store.Data,game.Store.Data.Hero).Count>0;
        // The icon row: one icon per tab. A tab with nothing shown to the player yet says so instead of opening empty.
        void BuildEdictTabIcons()
        {
            if(globalHud?.Layout!=null){BuildCasualEdictTabIcons();return;}
            float tile=Mathf.Max(24,Mathf.Min(quickAnchor.width*TabIconScale,(Screen.width/quickScale-quickAnchor.xMax-16)/HuntEdictHudRows.Tabs.Length-8)),x=quickAnchor.xMax+8;
            foreach(string tab in HuntEdictHudRows.Tabs)
            {
                string t=tab;bool open=sheetTab==t;
                var b=Button(quickRoot,"",()=>OpenEdictSheet(t),open?gold*.4f:Color.clear);b.name="edict-sheet-tab-"+t;
                QuickRect((RectTransform)b.transform,x,quickAnchor.y+(quickAnchor.height-tile)*.5f,tile,tile);
                var art=Rect("Icon",b.transform);Stretch(art);var image=art.gameObject.AddComponent<Image>();image.sprite=globalHud.Sprite(EdictTabArt(t));image.preserveAspect=true;image.raycastTarget=false;
                image.color=EdictTabAvailable(t)?Color.white:new Color(1,1,1,.4f);
                b.targetGraphic=image;
                x+=tile+8;
            }
        }
        void OpenEdictSheet(string tab)
        {
            if(tab=="skills"){CloseEdictSheetUi();ShowEdictSkillTree();return;}
            if(tab=="repeat"){CloseEdictSheetUi();ShowRepeatSettings();return;}
            if(tab=="autoEquip"){CloseEdictSheetUi();OpenGearRecommendationSettings();return;}
            var data=game.Store.Data;var hero=data.Hero;
            if(tab=="presets"){CloseEdictSheetUi();ShowEdictShare();return;}
            if(sheetTab==tab){CloseEdictSheetUi();return;}
            if(HuntEdictHudRows.Locked(tab,data)==""&&HuntEdictHudRows.Groups(tab,data,hero).Count==0){ShowToast(Loc.T("진행 후 공개됩니다."));return;}
            CloseEdictSheetUi();sheetTab=tab;sheetGroup=0;positionList=tab=="combat"||tab=="survival"&&SurvivalMenu(HuntEdictHudRows.Groups(tab,data,hero)).Count>0;positionOpening=true;targetTab=0;targetCategory="GRADE";targetDraft=null;sheetUndo.Clear();sheetPlaying=false;
            BuildEdictSheet();RefreshEdictTabIcons();ShowSheetForecast();
        }
        void CloseEdictSheetUi()
        {
            if(sheetCard!=null){var old=sheetCard;sheetCard=null;old.gameObject.SetActive(false);Destroy(old.gameObject);}
            sheetTab="";sheetEntranceKey="";sheetUndo.Clear();game.World?.HideEdictForecast();RefreshEdictTabIcons();
            if(game.Combat!=null&&sheetPlaying){game.Combat.State.paused=true;}sheetPlaying=false;
        }
        void RefreshEdictTabIcons()
        {
            foreach(var old in quickRoot.GetComponentsInChildren<Button>(true).Where(b=>b.name.StartsWith("edict-sheet-tab-",StringComparison.Ordinal)).ToList()){old.gameObject.SetActive(false);Destroy(old.gameObject);}
            BuildEdictTabIcons();
        }
        void BuildEdictSheet()
        {
            var data=game.Store.Data;var hero=data.Hero;
            var available=HuntEdictHudRows.Groups(sheetTab,data,hero);
            sheetPosition=sheetTab=="combat"&&!positionList&&sheetGroup<available.Count&&available[sheetGroup].ids.Contains("position.engage")&&
                HuntEdictProgression.Visible(data,hero,"position.engage")&&HuntEdictProgression.Visible(data,hero,"position.mode");
            sheetTarget=sheetTab=="combat"&&!positionList&&sheetGroup<available.Count&&available[sheetGroup].ids.Contains("target.priorityGrade");
            sheetOrder=sheetTab=="combat"&&!positionList&&sheetGroup<available.Count&&available[sheetGroup].ids.Contains("common.order");
            sheetDodge=sheetTab=="survival"&&!positionList&&sheetGroup<available.Count&&available[sheetGroup].ids.Contains("dodge.ground.policy");
            sheetHazard=sheetTab=="survival"&&!positionList&&sheetGroup<available.Count&&available[sheetGroup].ids.Contains("dodge.control");
            sheetExplore=sheetTab=="explore"&&available.Count>0&&HuntEdictHudRows.Locked(sheetTab,data)=="";
            sheetCatalog=available.Count>0&&!positionList&&!sheetPosition&&!sheetTarget&&!sheetOrder&&!sheetDodge&&!sheetHazard&&!sheetExplore;
            if((sheetTab=="combat"||sheetTab=="survival")&&positionList){BuildCombatEntryList(available);return;}
            if(DodgePanel){BuildCasualCompatibleEdictPanel(()=>BuildDodgePanel(data,hero));return;}
            if(HazardPanel){BuildCasualCompatibleEdictPanel(()=>BuildHazardPanel(data,hero));return;}
            if(OrderPanel){BuildCasualCompatibleEdictPanel(()=>BuildOrderPanel(data,hero));return;}
            if(PositionPanel){BuildCasualCompatibleEdictPanel(()=>BuildPositionPanel(data,hero));return;}
            if(TargetPanel){BuildCasualCompatibleEdictPanel(()=>BuildTargetPanel(data,hero));return;}
            if(ExplorePanel){BuildCasualCompatibleEdictPanel(()=>BuildExplorePanel(data,hero));return;}
            if(CatalogPanel){BuildCasualCompatibleEdictPanel(()=>BuildCatalogPanel(data,hero));return;}
        }
        void SelectSheetGroup(int index)
        {
            sheetGroup=index;positionList=false;RebuildEdictSheet();
        }
        // Rebuilt from the saved edict after every change; the scroll position stays where the player was.
        void BuildSheetRows()
        {
            if(sheetContent==null)return;
            if(OrderPanel){BuildOrderBody();return;}
            if(DodgePanel){BuildDodgeBody();return;}
            if(HazardPanel){BuildHazardBody();return;}
            if(ExplorePanel){BuildExploreBody();return;}
            if(CatalogPanel){BuildCatalogBody();return;}
            if(PositionPanel){BuildPositionBody();return;}
            if(TargetPanel){BuildTargetBody();return;}
        }
        void FinishEdictBody(float keep)
        {
            Canvas.ForceUpdateCanvases();LayoutRebuilder.ForceRebuildLayoutImmediate(sheetContent);
            sheetContent.anchoredPosition=new Vector2(0,keep);sheetUndoButton.interactable=sheetUndo.Count>0;
            // Fit after measuring the new rows, including removals, before restoring focus or drawing the preview.
            sheetScroll.GetComponent<EdictPopupLayout>()?.Refresh();
        }
        string SheetChoiceText(EdictOptionDefinition d,string code)=>EdictCasualRows.ChoiceText(d,code);
        static bool SheetNumberStacks(float inner,float unit)=>inner<166*unit;
        // A slider over the declared steps with minus and plus; dragging shows the value and the release saves it.
        void AddSheetNumber(RectTransform card,HuntEdictHudRows.Row row,Action<string,string> apply=null)
        {
            bool compact=PositionPanel||TargetPanel||DodgePanel||HazardPanel||ExplorePanel||CatalogPanel;float u=compact?positionUnit:1;apply=apply??SheetApply;
            string suffix=compact?CompactNumberSuffix(row.definition.id):"";
            var d=row.definition;decimal min=(decimal)d.min,step=(decimal)d.step;int steps=Mathf.Max(1,(int)Math.Round((decimal)(d.max-d.min)/step));
            string Format(int i)=>(min+i*step).ToString("0.###",CultureInfo.InvariantCulture);
            int current=Mathf.Clamp((int)Math.Round((decimal.Parse(row.value,CultureInfo.InvariantCulture)-min)/step),0,steps);
            var line=Rect("Number",card);line.gameObject.AddComponent<LayoutElement>().preferredHeight=40*u;
            if(compact)QuickRect(line,0,TargetPanel||DodgePanel||ExplorePanel?14*u:0,sheetInner,36*u);
            var minus=Button(line,"−",()=>{if(current>0)apply(d.id,Format(current-1));},null);minus.name="edict-sheet-minus-"+d.id;minus.interactable=row.editable;QuickRect((RectTransform)minus.transform,0,2*u,34*u,34*u);
            var plus=Button(line,"+",()=>{if(current<steps)apply(d.id,Format(current+1));},null);plus.name="edict-sheet-plus-"+d.id;plus.interactable=row.editable;
            if(compact){PositionButtonStyle(minus);PositionButtonStyle(plus);}
            var valueText=Label(line,Format(current)+suffix,Mathf.RoundToInt((compact?UiTheme.Body:18)*u),gold,TextAnchor.MiddleCenter);valueText.name="edict-sheet-value-"+d.id;
            float inner=sheetInner-(compact?0:20),valueW=54*u;QuickRect((RectTransform)plus.transform,inner-34*u,2*u,34*u,34*u);QuickRect(valueText.rectTransform,inner-34*u-valueW-4*u,0,valueW,40*u);
            bool stacked=SheetNumberStacks(inner,u);
            if(stacked)
            {
                line.GetComponent<LayoutElement>().preferredHeight=100*u;
                if(compact)QuickRect(line,0,TargetPanel||DodgePanel||ExplorePanel?14*u:0,sheetInner,100*u);
                float buttonW=Mathf.Min(34*u,Mathf.Max(u,(inner-8*u)/2));
                QuickRect((RectTransform)minus.transform,0,32*u,buttonW,34*u);QuickRect((RectTransform)plus.transform,inner-buttonW,32*u,buttonW,34*u);
                QuickRect(valueText.rectTransform,0,0,inner,26*u);
                var cardSize=card.GetComponent<LayoutElement>();if(compact&&cardSize!=null)cardSize.preferredHeight=Mathf.Max(cardSize.preferredHeight,line.anchoredPosition.y+100*u);
            }
            var track=Box("edict-sheet-slider-"+d.id,line,UiTheme.Tint(UiTheme.EdgeHex,.7f));QuickRect(track,stacked?8*u:44*u,stacked?78*u:12*u,stacked?Mathf.Max(u,inner-16*u):inner-valueW-92*u,16*u);
            var fill=Box("Fill",track,UiTheme.Tint(UiTheme.GoldHex,.55f));fill.GetComponent<Image>().raycastTarget=false;fill.anchorMin=Vector2.zero;fill.anchorMax=new Vector2(current/(float)steps,1);fill.offsetMin=fill.offsetMax=Vector2.zero;
            var area=Rect("Slide area",track);Stretch(area);var knob=Box("Knob",area,gold);knob.anchorMin=knob.anchorMax=new Vector2(0,.5f);knob.pivot=new Vector2(.5f,.5f);knob.sizeDelta=new Vector2(14,26)*u;knob.GetComponent<Image>().raycastTarget=false;
            var hit=track.GetComponent<Image>();var slider=track.gameObject.AddComponent<DialogSlider>();slider.ScrollOnVerticalDrag=true;slider.targetGraphic=hit;slider.transition=Selectable.Transition.None;slider.handleRect=knob;slider.direction=Slider.Direction.LeftToRight;
            slider.minValue=0;slider.maxValue=steps;slider.wholeNumbers=true;slider.SetValueWithoutNotify(current);slider.interactable=row.editable;int shown=current;
            slider.onValueChanged.AddListener(x=>{shown=(int)x;valueText.text=Format(shown)+suffix;fill.anchorMax=new Vector2(shown/(float)steps,1);if(PositionPanel)game.World?.UpdateEdictDistancePreview((float)(min+shown*step));else if(TargetPanel){targetDraft=HuntEdictV2Editing.WithGlobal(targetDraft??HuntEdictLoadout.FromHero(game.Store.Data.Hero).edict,d.id,Format(shown));ShowSheetForecast();}});
            StyleSheetNumber(minus,plus,valueText,hit,fill.GetComponent<Image>(),knob.GetComponent<Image>());
            bool committed=false;track.gameObject.AddComponent<PointerRelease>().Released=()=>{if(!committed&&shown!=current){committed=true;apply(d.id,Format(shown));}};
        }
        // Endpoint labels follow the actual track, including the stacked narrow layout.
        void AddSheetNumberBounds(RectTransform row,float inner,float u,string minimum,string maximum)
        {
            var track=(RectTransform)row.GetComponentInChildren<Slider>().transform;var number=(RectTransform)track.parent;float bound=Mathf.Max(u,(track.rect.width-4*u)/2);
            var min=Label(row,minimum,Mathf.RoundToInt(UiTheme.Caption*u),UiTheme.D4Muted);var max=Label(row,maximum,Mathf.RoundToInt(UiTheme.Caption*u),UiTheme.D4Muted,TextAnchor.MiddleRight);
            foreach(var text in new[]{min,max}){QuickRect(text.rectTransform,0,0,bound,14*u);EdictText(text);text.horizontalOverflow=HorizontalWrapMode.Wrap;}
            float h=Mathf.Max(14*u,Mathf.Ceil(Mathf.Max(min.preferredHeight,max.preferredHeight))+2*u);
            if(SheetNumberStacks(inner,u))
            {
                track.anchoredPosition=new Vector2(track.anchoredPosition.x,70*u+h+2*u);float height=track.anchoredPosition.y+24*u;
                number.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,height);number.GetComponent<LayoutElement>().preferredHeight=height;
            }
            float y=number.anchoredPosition.y+track.anchoredPosition.y-h;
            if(y<0){number.anchoredPosition+=Vector2.up*(-y);y=0;}
            float left=number.anchoredPosition.x+track.anchoredPosition.x;QuickRect(min.rectTransform,left,y,bound,h);QuickRect(max.rectTransform,left+track.rect.width-bound,y,bound,h);
            var size=row.GetComponent<LayoutElement>();if(size!=null)size.preferredHeight=Mathf.Max(size.preferredHeight,number.anchoredPosition.y+Mathf.Max(number.rect.height,number.GetComponent<LayoutElement>().preferredHeight));
        }
        bool SheetCommit(HuntEdictLoadout next)
        {
            var hero=game.Store.Data.Hero;var session=new HuntEdictEditSession(hero);
            try{session.ReplaceDraft(next);}catch(ArgumentException e){ShowToast(e.Message);return false;}
            if(game.Store.CommitHuntEdict(session,game.catalog,quickLive?game.Combat:null,true))return true;
            ShowToast(game.Store.Error);return false;
        }
        void SheetApply(string id,string value)
        {
            string focus=SheetFocusName();
            var before=HuntEdictLoadout.FromHero(game.Store.Data.Hero);HuntEdictLoadout next;
            try{next=HuntEdictQuickMenu.With(before,id,value);}catch(ArgumentException e){ShowToast(e.Message);return;}
            if(SheetCommit(next)){sheetUndo.Add(before);if(sheetUndo.Count>30)sheetUndo.RemoveAt(0);}
            BuildSheetRows();RestoreSheetFocus(focus);if(id.StartsWith("position.",StringComparison.Ordinal)||id.StartsWith("target.",StringComparison.Ordinal)||id=="common.order")ShowSheetForecast();
        }
        void UndoSheet()
        {
            string focus=SheetFocusName();
            if(sheetUndo.Count==0)return;var previous=sheetUndo[sheetUndo.Count-1];
            if(SheetCommit(previous))sheetUndo.RemoveAt(sheetUndo.Count-1);
            BuildSheetRows();RestoreSheetFocus(focus);ShowSheetForecast();
        }
        string SheetFocusName()
        {
            var selected=EventSystem.current?.currentSelectedGameObject;
            return selected!=null&&sheetCard!=null&&selected.transform.IsChildOf(sheetCard)?selected.name:null;
        }
        void RestoreSheetFocus(string name)
        {
            if(string.IsNullOrEmpty(name)||sheetCard==null||EventSystem.current==null)return;
            var target=sheetCard.GetComponentsInChildren<Selectable>().FirstOrDefault(s=>s.name==name&&s.IsActive()&&s.IsInteractable());
            if(target==null)target=sheetUndoButton!=null&&sheetUndoButton.interactable?sheetUndoButton:sheetCard.GetComponentsInChildren<Button>().FirstOrDefault(b=>b.name=="edict-sheet-close");
            if(target!=null)EventSystem.current.SetSelectedGameObject(target.gameObject);
        }
        // While paused, the floor shows where the hero walks with the saved position settings.
        void ShowSheetForecast()
        {
            game.World?.ConfigureEdictSkin(EdictSkin);
            if(!quickLive){game.World?.HideEdictForecast();return;}
            var groups=HuntEdictHudRows.Groups(sheetTab,game.Store.Data,game.Store.Data.Hero);
            bool position=!sheetPlaying&&PositionPanel&&game.Combat!=null&&sheetGroup<groups.Count&&groups[sheetGroup].ids.Contains("position.engage");
            if(!sheetPlaying&&OrderPanel&&game.Active&&game.Combat!=null){ShowOrderForecast();return;}
            if(!sheetPlaying&&TargetPanel&&game.Combat!=null){ShowTargetForecast();return;}
            if(!position){game.World?.HideEdictForecast();return;}
            try{var preview=game.Combat.ForecastPosition(HuntEdictLoadout.FromHero(game.Store.Data.Hero),positionOpening);FrameEdictWorld(preview);game.World.ShowEdictPositionForecast(preview);}catch(ArgumentException){game.World.HideEdictForecast();}
        }
        // Play lets the battle run with the panel open, to watch what the change does; pressing it again pauses.
        void ToggleSheetPlay()
        {
            if(!quickLive)return;
            sheetPlaying=!sheetPlaying;game.Combat.State.paused=!sheetPlaying;
            sheetPlayButton.GetComponentInChildren<Text>().text=HuntEdictUiCatalog.Playback(!sheetPlaying);
            if(sheetPlaying)game.World?.HideEdictForecast();else ShowSheetForecast();
        }
    }
}
