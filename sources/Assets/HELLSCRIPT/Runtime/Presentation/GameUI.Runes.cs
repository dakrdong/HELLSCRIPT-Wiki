using System;
using System.Collections.Generic;
using System.Linq;
using Hellscript.Runes;
using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    public sealed partial class GameUI
    {
        RuneInventory runeInventory;RuneBoardEditor runeEditor;RuneBoardGraphic runeCanvas;
        RuneGrowthState runeSource;string runeWeapon="sword",runeSelected;int runeRotation,runeColor=-1,runeSize;
        bool runeWasPaused,runeSession,runeViewing,runePortrait;RectTransform runeStorage,runeDetails,runeStage,runeModal;
        Text runeDetail,runeStorageCount;Button runeSave,runeUndoButton,runeRevertButton,runeRotateButton,runeColorFilter,runeSizeFilter; readonly Text[] runeTypeCounts=new Text[5];RunePracticeModel runePractice;
        readonly HashSet<string> runeFusionSelection=new HashSet<string>();int runeFusionGrade;float runeRevealUntil;
        readonly Dictionary<string,Vector3> runeViews=new Dictionary<string,Vector3>();
        readonly Dictionary<string,int> runeStorageRotations=new Dictionary<string,int>();
        readonly List<RuneGrowthState> runeUndo=new List<RuneGrowthState>();
        readonly HashSet<int> runeSizes=new HashSet<int>{1,2,3,4,5};
        Font runeSerif;
        Vector2 runeScreen;float runeInterface;float runeUnit=1;int runeFocus;bool runeOverview;HexCell runeNode=new HexCell(0,0);
        static Color RuneBg=>RuneV13Art.Color("#1b1e16");
        static Color RuneLine=>RuneV13Art.Color("#655a3d");
        static Color RuneText=>RuneV13Art.Color("#d8cdb4");
        RuneWeaponProgress RuneProgress=>RuneMasteryProgress.Get(runeSource,runeWeapon);
        void ReflowRunes()
        {if(Page=="runes"&&runeSession&&!CommonPanelOpen&&(runeScreen!=new Vector2(Screen.width,Screen.height)||runeInterface!=InterfaceFactor))ShowRunes();}
        RectTransform RuneBox(string name,Transform parent,Color? fill=null)
        {var box=Box(name,parent,fill??RuneBg);box.gameObject.AddComponent<UIRectBorder>().color=RuneLine;return box;}
        Text RuneTextAt(Transform parent,string text,float x,float y,float w,float h,int size=14,TextAnchor align=TextAnchor.MiddleLeft,Color? color=null)
        {var t=Label(parent,text,Mathf.RoundToInt(size*runeUnit*InterfaceFactor),color??RuneText,align);Place(t.rectTransform,x,y,w,h);t.horizontalOverflow=HorizontalWrapMode.Wrap;t.resizeTextForBestFit=true;t.resizeTextMinSize=Mathf.Max(7,Mathf.RoundToInt(size*runeUnit*.9f));t.resizeTextMaxSize=Mathf.RoundToInt(size*runeUnit*InterfaceFactor);if(size>=15){runeSerif=UiFonts.Body;t.font=runeSerif;}return t;}
        Button RuneButton(Transform parent,string name,string text,Action action,float x,float y,float w,float h,bool selected=false)
        {
            var b=Button(parent,text,action,selected?RuneV13Art.Color("#393622"):RuneBg);b.name=name;Place((RectTransform)b.transform,x,y,w,h);
            var t=b.GetComponentInChildren<Text>();t.fontSize=Mathf.RoundToInt(11*runeUnit*InterfaceFactor);t.resizeTextForBestFit=true;t.resizeTextMinSize=Mathf.Max(7,Mathf.RoundToInt(10*runeUnit));t.resizeTextMaxSize=t.fontSize;
            UiTheme.Choice(b,selected);t.color=RuneText;var border=b.GetComponent<UIRectBorder>();border.color=selected?RuneV13Art.Color("#cfb77c"):RuneLine;return b;
        }
        void RuneIcon(Transform parent,string glyph,float x,float y,float size,Color? color=null)
        {var r=Rect("Icon "+glyph,parent);Place(r,x,y,size,size);var raw=r.gameObject.AddComponent<RawImage>();raw.texture=RuneV13Art.Atlas;raw.uvRect=RuneV13Art.Glyph("g-"+glyph);raw.color=color??RuneText;raw.raycastTarget=false;}
        public void ShowRunes()
        {
            if(!RequireContent(ContentUnlocks.Rune))return;
            if(game.ComparisonRun){if(game.Active)ShowComparisonConditions();ShowToast(TrainingComparisonSession.RuneLockedMessage);return;}
            if(runeCanvas!=null&&runeCanvas.editor!=null){runeViews[runeWeapon]=new Vector3(runeCanvas.pan.x,runeCanvas.pan.y,runeCanvas.zoom);runeFocus=runeCanvas.focusRegion;runeOverview=runeCanvas.overview;}
            if(!runeSession){runeSession=true;runeWasPaused=game.Combat?.State.paused??false;runeWeapon=RuneMasteryCatalog.EquippedWeapon(game.Store.Data.Hero)??"sword";ReloadRuneDraft();}
            if(game.Combat!=null)game.Combat.State.paused=true;
            pageRepaint=ShowRunes;Base("runes","룬 보드","",responsive:true);runeModal=null;
            header.gameObject.SetActive(false);footer.gameObject.SetActive(false);content.parent.gameObject.SetActive(false);
            runeScreen=new Vector2(Screen.width,Screen.height);runeInterface=InterfaceFactor;Canvas.ForceUpdateCanvases();
            float availableWidth=root.rect.width,height=root.rect.height;bool portrait=availableWidth<height;runePortrait=portrait;runeDetails=null;runeRotateButton=null;runeColorFilter=null;runeSizeFilter=null;
            float width=availableWidth;
            runeUnit=Mathf.Clamp(portrait?width/480f:width/1280f,.4f,1.6f);
            float s=runeUnit,margin=6*s,head=44*s,foot=46*s,bodyY=head+margin,bodyH=height-head-foot-margin*2;
            var background=Box("Rune background",root,RuneBg);Stretch(background);background.SetSiblingIndex(0);
            var frame=RuneBox("Rune v13 frame",root);Place(frame,(availableWidth-width)*.5f,0,width,height);frame.SetSiblingIndex(1);
            var title=RuneBox("Rune title",frame);Place(title,margin,margin,width-margin*2,head-margin);
            RuneTextAt(title,"HELLSCRIPT",10*s,0,90*s,head-margin,12);
            RuneButton(title,"rune-help","가이드",ShowRuneGuide,105*s,3*s,64*s,head-margin-6*s);
            RuneTextAt(title,"룬 보드",width*.36f,0,width*.27f,head-margin,21,TextAnchor.MiddleCenter);
            RuneButton(title,"rune-codex","도감",ShowRuneCodex,width-margin*2-132*s,3*s,44*s,head-margin-6*s);
            RuneButton(title,"rune-info","ⓘ",ShowRuneInformation,width-margin*2-84*s,3*s,34*s,head-margin-6*s);
            RuneButton(title,"닫기","×",CloseRunes,width-margin*2-46*s,3*s,36*s,head-margin-6*s);
            float rail=portrait?0:88*s,weaponH=portrait?106*s:0;
            var weapons=Rect("Weapon rail",frame);Place(weapons,margin,bodyY,portrait?width-2*margin:rail,portrait?weaponH:bodyH);
            for(int i=0;i<6;i++)
            {
                string weapon=RuneMasteryCatalog.Weapons[i];float ww=portrait?weapons.rect.width/6:rail-6*s,wh=portrait?weaponH-5*s:Mathf.Min(82*s,bodyH/6-5*s);
                var b=RuneButton(weapons,"rune-weapon-"+weapon,"",()=>SwitchRuneWeapon(weapon),portrait?i*ww:0,portrait?0:i*(wh+5*s),ww-3*s,wh,weapon==runeWeapon);
                float artSize=portrait?32*s:46*s;var image=Rect("Weapon art",b.transform);Place(image,(ww-3*s-artSize)/2,3*s,artSize,artSize);var raw=image.gameObject.AddComponent<RawImage>();raw.texture=Resources.Load<Texture2D>("Runes/V13/Weapons/"+weapon);raw.raycastTarget=false;
                RuneTextAt(b.transform,RuneMasteryCatalog.Name(weapon),3*s,portrait?36*s:wh*.59f,ww-9*s,19*s,12,TextAnchor.MiddleCenter);
                var progress=RuneMasteryProgress.Get(runeSource,weapon);
                RuneTextAt(b.transform,"Lv. "+progress.level,3*s,portrait?56*s:wh*.80f,ww-9*s,12*s,9,TextAnchor.MiddleCenter);
                if(portrait)
                {
                    RuneTextAt(b.transform,progress.level==41?Loc.T("최고 레벨"):progress.xp+" / "+progress.level*100+" XP",3*s,68*s,ww-9*s,11*s,8,TextAnchor.MiddleCenter).name="Weapon tab XP";
                    var track=RuneBox("Weapon tab XP track",b.transform);Place(track,7*s,81*s,ww-17*s,3*s);
                    var fill=Box("Weapon tab XP fill",track,RuneV13Art.Color("#bda970"));Stretch(fill);fill.anchorMax=new Vector2(progress.level==41?1:progress.xp/(progress.level*100f),1);
                    RuneTextAt(b.transform,Loc.F("포인트 {0}",RuneMasteryProgress.Points(progress)),3*s,86*s,ww-9*s,13*s,9,TextAnchor.MiddleCenter).name="Weapon tab points";
                }
            }
            float workX=margin+rail,workW=width-margin*2-rail,workY=bodyY+weaponH,workH=bodyH-weaponH;
            // Reserve three complete square rows using the same slot width and spacing as the storage grid.
            float storageSlot=(workW-18*s-4*s*7)/8;
            float portraitStorageH=76*s+storageSlot*3+4*s*2+8*s;
            float boardW=portrait?workW:workW*.5f,boardH=portrait?workH-portraitStorageH:workH;
            var boardPanel=RuneBox("Rune board panel",frame);Place(boardPanel,workX,workY,boardW,boardH);
            if(!portrait)
            {
                var mastery=RuneBox("Weapon mastery",boardPanel);Place(mastery,0,0,boardW,56*s);
                RuneTextAt(mastery,Loc.F("{0} 숙련도",RuneMasteryCatalog.Name(runeWeapon)),10*s,3*s,boardW*.36f,24*s,16);
                RuneTextAt(mastery,RuneV13Catalog.Data.weapons.First(w=>w.id==runeWeapon).theme,10*s,28*s,boardW*.39f,18*s,10);
                RuneTextAt(mastery,"Lv. "+RuneProgress.level,boardW*.4f,7*s,70*s,24*s,14);
                RuneTextAt(mastery,RuneProgress.level==41?Loc.T("최고 레벨"):RuneProgress.xp+" / "+RuneProgress.level*100+" XP",boardW*.62f,10*s,boardW*.22f,20*s,10,TextAnchor.MiddleRight);
                var xp=RuneBox("Mastery XP track",mastery);Place(xp,boardW*.4f,36*s,boardW*.44f,4*s);var xpFill=Box("Mastery XP",xp,RuneV13Art.Color("#bda970"));Stretch(xpFill);xpFill.anchorMax=new Vector2(RuneProgress.level==41?1:RuneProgress.xp/(RuneProgress.level*100f),1);
                RuneTextAt(mastery,"슬롯 포인트",boardW*.85f,4*s,boardW*.14f,15*s,9,TextAnchor.MiddleCenter);
                RuneTextAt(mastery,RuneMasteryProgress.Points(RuneProgress).ToString(),boardW*.85f,19*s,boardW*.14f,30*s,22,TextAnchor.MiddleCenter);
            }
            float toolsY=portrait?0:56*s;
            var tools=Rect("Board tools",boardPanel);Place(tools,0,toolsY,boardW,34*s);
            RuneTextAt(tools,RuneV13Catalog.RegionName(runeFocus)+"  "+RuneMasteryProgress.RegionCount(RuneProgress,runeFocus)+" / 37",10*s,0,boardW*.4f,34*s,14);
            float tw=34*s,tx=boardW-222*s;
            RuneButton(tools,"rune-edit",runeViewing?"보기":"편집",()=>{runeViewing=!runeViewing;runeSelected=null;ShowRunes();},tx,2*s,60*s,30*s,!runeViewing);
            RuneButton(tools,"rune-zoom-out","−",()=>runeCanvas.Zoom(-.2f),tx+65*s,2*s,tw,30*s);
            RuneButton(tools,"rune-zoom-in","+",()=>runeCanvas.Zoom(.2f),tx+103*s,2*s,tw,30*s);
            RuneButton(tools,"rune-fit","⌖",()=>runeCanvas.Focus(runeFocus),tx+141*s,2*s,tw,30*s);
            RuneButton(tools,"rune-overview","지도",()=>{runeOverview=true;runeCanvas.Focus(runeFocus,true);},tx+179*s,2*s,38*s,30*s);
            float stageY=toolsY+34*s;
            runeStage=Rect("rune-board-viewport",boardPanel);Place(runeStage,1,stageY,boardW-2,boardH-stageY-(portrait?28:60)*s);runeStage.gameObject.AddComponent<RectMask2D>();
            runeCanvas=MakeRuneGraphic(runeStage,"rune-board");runeCanvas.editor=runeEditor;runeCanvas.viewOnly=runeViewing;runeCanvas.focusRegion=runeFocus;runeCanvas.overview=runeOverview;
            runeCanvas.piece=runeSelected==null?null:runeInventory.FindOwned(runeSelected);runeCanvas.rotation=runeRotation;
            if(runeViews.TryGetValue(runeWeapon,out var view)){runeCanvas.pan=new Vector2(view.x,view.y);runeCanvas.zoom=view.z;}
            runeCanvas.inputBlocked=()=>runeModal!=null||CommonPanelOpen;runeCanvas.rotateSelected=RotateRune;runeCanvas.clearSelected=()=>{runeSelected=null;runeCanvas.piece=null;runeCanvas.Refresh();RenderRuneDetail();};runeCanvas.inspect=InspectRuneCell;runeCanvas.beforeChange=PushRuneUndo;runeCanvas.changed=RuneChanged;runeCanvas.rejected=ShowToast;runeCanvas.Refresh();
            CreateRuneRegionMarkers();CreateRuneMinimap();
            if(!portrait)
            {
                var regions=Rect("Region navigation",boardPanel);Place(regions,0,boardH-60*s,boardW,32*s);
                for(int i=0;i<7;i++){int region=i;var b=RuneButton(regions,"rune-region-"+i,RuneV13Catalog.RegionName(i),()=>FocusRuneRegion(region),i*boardW/7,0,boardW/7-1,32*s,region==runeFocus);b.GetComponentInChildren<Text>().fontSize=Mathf.RoundToInt(11*s);}
            }
            var counts=Rect("Rune type activation",boardPanel);Place(counts,0,boardH-28*s,boardW,28*s);
            var effects=RuneEffects();for(int i=0;i<5;i++){int type=i;var count=RuneButton(counts,"rune-effects-type-"+i,"",()=>ShowRuneEffects(type),i*boardW/5+2*s,0,boardW/5-4*s,26*s);runeTypeCounts[i]=count.GetComponentInChildren<Text>();runeTypeCounts[i].color=RuneBoardGraphic.TypeColor(i);}
            var storagePanel=RuneBox("Rune storage panel",frame);float storageW=portrait?workW:workW-boardW,storageH=portrait?workH-boardH:workH;
            runeCanvas.storageDropTarget=storagePanel;runeCanvas.recover=()=>{runeSelected=runeCanvas.piece?.InstanceId;runeRotation=runeCanvas.rotation;RemoveRune();};
            Place(storagePanel,portrait?workX:workX+boardW,portrait?workY+boardH:workY,storageW,storageH);
            RuneTextAt(storagePanel,"룬 보관함",10*s,3*s,portrait?130*s:storageW*.5f,30*s,17);
            RuneButton(storagePanel,"rune-effects","활성 효과",ShowRuneEffects,storageW-92*s,5*s,82*s,27*s);
            if(portrait)
            {
                runeColorFilter=RuneButton(storagePanel,"rune-color-filter","",()=>ShowRuneFilter(false),storageW-318*s,5*s,110*s,27*s);
                runeSizeFilter=RuneButton(storagePanel,"rune-size-filter","",()=>ShowRuneFilter(true),storageW-204*s,5*s,108*s,27*s);
                runeRotateButton=RuneButton(storagePanel,"rune-rotate","60° 회전",RotateRune,storageW-110*s,39*s,100*s,29*s,true);
                UpdateRuneFilterLabels();
            }
            else
            {
                float filtersY=38*s;
                for(int i=-1;i<5;i++){int type=i;RuneButton(storagePanel,"rune-color-"+i,i<0?"전체":RuneV13Catalog.TypeName(i),()=>{runeColor=type;ShowRunes();},8*s+(i+1)*(storageW-16*s)/6,filtersY,(storageW-16*s)/6-3*s,30*s,type==runeColor);}
                for(int i=0;i<=5;i++){int size=i;bool on=i==0?runeSizes.Count==5:runeSizes.Contains(i);RuneButton(storagePanel,"rune-size-"+i,(on?"☑ ":"☐ ")+(i==0?Loc.T("전부"):Loc.F("{0}칸",i)),()=>{ToggleRuneSize(size);ShowRunes();},8*s+i*(storageW-16*s)/6,filtersY+34*s,(storageW-16*s)/6-2*s,26*s);}
            }
            runeStorageCount=RuneTextAt(storagePanel,"",10*s,(portrait?39:104)*s,storageW-(portrait?130:20)*s,portrait?29*s:20*s,10);
            float detailH=portrait?0:214*s,gridY=(portrait?76:129)*s;
            var storageView=Rect("Rune storage viewport",storagePanel);Place(storageView,9*s,gridY,storageW-18*s,Mathf.Max(36*s,storageH-gridY-detailH-5*s));storageView.gameObject.AddComponent<Image>().color=Color.clear;storageView.gameObject.AddComponent<RectMask2D>();
            var scroll=storageView.gameObject.AddComponent<ScrollRect>();scroll.horizontal=false;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.viewport=storageView;
            runeStorage=Rect("Rune storage",storageView);runeStorage.anchorMin=new Vector2(0,1);runeStorage.anchorMax=Vector2.one;runeStorage.pivot=new Vector2(.5f,1);runeStorage.sizeDelta=Vector2.zero;scroll.content=runeStorage;
            var grid=runeStorage.gameObject.AddComponent<GridLayoutGroup>();grid.constraint=GridLayoutGroup.Constraint.FixedColumnCount;grid.constraintCount=8;float slot=(storageView.rect.width-4*s*7)/8;grid.cellSize=new Vector2(slot,slot);grid.spacing=new Vector2(4*s,4*s);
            runeStorage.gameObject.AddComponent<ContentSizeFitter>().verticalFit=ContentSizeFitter.FitMode.PreferredSize;
            if(!portrait){runeDetails=RuneBox("Selection details",storagePanel);Place(runeDetails,0,storageH-detailH,storageW,detailH);}
            var bottom=RuneBox("Rune save bar",frame);Place(bottom,margin,height-foot,width-margin*2,foot-margin);
            if(!portrait)RuneTextAt(bottom,"개방과 활성은 별개입니다.",10*s,0,width*.43f,foot-margin,10);
            float bw=bottom.rect.width;
            RuneButton(bottom,"rune-presets","프리셋",ShowRunePresets,bw-304*s,4*s,66*s,foot-margin-8*s);
            (runeUndoButton=RuneButton(bottom,"rune-undo","↶",UndoRunes,bw-234*s,4*s,34*s,foot-margin-8*s)).interactable=runeUndo.Count>0;
            (runeRevertButton=RuneButton(bottom,"rune-revert","되돌리기",()=>Confirm(Loc.T("모든 무기 보드의 미저장 변경을 버릴까요?"),()=>{ReloadRuneDraft();ShowRunes();}),bw-196*s,4*s,86*s,foot-margin-8*s)).interactable=RuneDirty;
            runeSave=RuneButton(bottom,"rune-save","변경 저장",SaveRuneDraft,bw-106*s,4*s,100*s,foot-margin-8*s,true);
            UiTheme.Button(runeSave,true);
            overlay.SetAsLastSibling();UpdateRuneContents();RenderRuneDetail();
        }
        RuneBoardGraphic MakeRuneGraphic(RectTransform parent,string name){var r=Rect(name,parent);Stretch(r);return r.gameObject.AddComponent<RuneBoardGraphic>();}
        void ReloadRuneDraft()
        {runeSource=RuneGrowth.Copy(game.Store.Data.runes);runeInventory=RuneGrowth.Restore(runeSource,6);runeEditor=runeInventory.BeginEdit(runeWeapon);runeSelected=null;runeRotation=0;runeUndo.Clear();runeStorageRotations.Clear();}
        static string RuneLayoutKey(List<MasteryRunePlacement> placements)=>JsonUtility.ToJson(new RunePreset{placements=placements.OrderBy(p=>p.runeId,StringComparer.Ordinal).ToList()});
        bool RuneDirty=>runeEditor!=null&&(runeEditor.HasDraftChanges||RuneLayoutKey(RuneGrowth.Capture(runeInventory))!=RuneLayoutKey(game.Store.Data.runes.placements)||JsonUtility.ToJson(new RuneGrowthState{presets=runeSource.presets})!=JsonUtility.ToJson(new RuneGrowthState{presets=game.Store.Data.runes.presets})||runeSource.mastery.Any(p=>JsonUtility.ToJson(p)!=JsonUtility.ToJson(RuneMasteryProgress.Get(game.Store.Data.runes,p.weapon))));
        bool StageRuneBoard(){if(runeEditor.TryCommit(out _))return true;ShowToast("현재 보드의 연결을 먼저 고치거나 되돌려 주세요.");return false;}
        void SwitchRuneWeapon(string weapon)
        {if(!StageRuneBoard())return;runeCanvas=null;runeWeapon=weapon;runeEditor=runeInventory.BeginEdit(weapon);runeSelected=null;runeRotation=0;runeNode=new HexCell(0,0);runeFocus=0;runeOverview=false;runeViews.Remove(weapon);ShowRunes();}
        void PushRuneUndo(){var copy=RuneGrowth.Copy(runeSource);copy.placements=RuneGrowth.Capture(runeInventory);runeUndo.Add(copy);if(runeUndo.Count>50)runeUndo.RemoveAt(0);}
        void UndoRunes(){if(runeUndo.Count==0)return;runeSource=runeUndo.Last();runeUndo.RemoveAt(runeUndo.Count-1);runeInventory=RuneGrowth.Restore(runeSource,6);runeEditor=runeInventory.BeginEdit(runeWeapon);runeSelected=null;ShowRunes();}
        void RuneChanged(){runeSelected=runeCanvas.piece?.InstanceId;runeRotation=runeCanvas.rotation;if(runeSelected!=null)runeStorageRotations[runeSelected]=runeRotation;if(runeEditor.ValidateDraft().IsValid)runeEditor.TryCommit(out _);UpdateRuneContents();RenderRuneDetail();}
        RuneBoardEffects RuneEffects()=>RuneEffectEvaluator.Evaluate(runeEditor.Board,6,runeEditor.DraftPlacements,runeEditor.ValidateDraft(),runeWeapon,true);
        void UpdateRuneContents()
        {
            if(runeCanvas==null||runeStorage==null)return;foreach(Transform child in runeStorage){child.gameObject.SetActive(false);Destroy(child.gameObject);}
            var stored=runeInventory.OwnedPieces.Where(p=>runeInventory.GetCommittedOwner(p.InstanceId)==null&&!runeEditor.IsInDraft(p.InstanceId)).ToArray();
            var filtered=runeInventory.OwnedPieces.GroupBy(p=>new{p.Shape.Id,p.Type}).SelectMany(g=>g.OrderBy(p=>p.Grade).ThenBy(p=>p.InstanceId,StringComparer.Ordinal).Select((p,copy)=>new{piece=p,copy})).Where(v=>stored.Contains(v.piece)&&(runeColor<0||v.piece.Type==runeColor)&&runeSizes.Contains(v.piece.Shape.Size)).OrderBy(v=>v.copy).ThenBy(v=>v.piece.Shape.Size).ThenBy(v=>v.piece.Type).ThenBy(v=>v.piece.Shape.Id).Select(v=>v.piece).ToArray();
            float slot=runeStorage.GetComponent<GridLayoutGroup>().cellSize.x;
            foreach(var p in filtered)
            {
                var piece=p;int turns=runeStorageRotations.TryGetValue(piece.InstanceId,out var pose)?pose:RuneV13Art.StorageRotation(piece.Shape.Id);
                var b=RuneButton(runeStorage,"rune-card-"+p.InstanceId,"",()=>{if(runeViewing){ShowToast("룬 편집은 편집 모드에서 가능합니다.");return;}runeSelected=piece.InstanceId;runeRotation=turns;runeCanvas.piece=piece;runeCanvas.rotation=turns;UpdateRuneContents();RenderRuneDetail();},0,0,slot,slot,runeSelected==p.InstanceId);
                var graphic=MakeRuneGraphic((RectTransform)b.transform,"Rune block");graphic.icon=true;graphic.piece=piece;graphic.rotation=turns;graphic.raycastTarget=false;
                RuneTextAt(b.transform,p.Shape.Size.ToString(),slot-14*runeUnit,slot-15*runeUnit,12*runeUnit,13*runeUnit,9,TextAnchor.MiddleCenter);
                var drag=b.gameObject.AddComponent<RuneStorageDrag>();drag.board=runeCanvas;drag.piece=piece;drag.rotation=turns;
            }
            runeStorageCount.text=Loc.F("보관 {0}개 · 표시 {1}개 · 8열",stored.Length,filtered.Length);
            var validation=runeEditor.ValidateDraft();
            var effects=RuneEffects();for(int i=0;i<5;i++){int type=i;runeTypeCounts[i].text=Loc.T(RuneV13Catalog.TypeName(i))+" "+effects.Nodes.Count(n=>n.IsPreviewActive&&n.Cell.AbilityType==type);}
            runeUndoButton.interactable=runeUndo.Count>0;runeRevertButton.interactable=RuneDirty;
            runeSave.interactable=validation.IsValid&&RuneDirty&&runeSource.revision==game.Store.Data.runes.revision;runeCanvas.Refresh();
        }
        void InspectRuneCell(HexCell coordinate)
        {if(!runeEditor.Board.TryGetCell(coordinate,out var cell))return;runeNode=coordinate;if(runeViewing||!runeEditor.DraftPlacements.Any(p=>p.OccupiedCells.Contains(coordinate))){runeSelected=null;runeCanvas.piece=null;}RenderRuneDetail();if(runePortrait&&(runeViewing||!runeEditor.DraftPlacements.Any(p=>p.OccupiedCells.Contains(coordinate))))ShowRuneAbility(cell);}
        static string RuneAbilityLine(RuneBoardCell cell)=>cell.Ability.Meaning=="root"?Loc.T("공용 시작점"):cell.IsElite?RuneMasteryCatalog.AbilityName(cell.Ability.Meaning):RuneMasteryCatalog.AbilityName(cell.Ability.Meaning)+" +"+cell.Value.ToString("0.##")+cell.Unit;
        RectTransform RuneDetailScroll(float height)
        {
            float s=runeUnit;var viewport=Rect("Selection scroll",runeDetails);Place(viewport,8*s,29*s,runeDetails.rect.width-16*s,runeDetails.rect.height-69*s);
            viewport.gameObject.AddComponent<Image>().color=Color.clear;viewport.gameObject.AddComponent<RectMask2D>();var scroll=viewport.gameObject.AddComponent<ScrollRect>();scroll.horizontal=false;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.viewport=viewport;
            var body=Rect("Selection content",viewport);body.anchorMin=new Vector2(0,1);body.anchorMax=Vector2.one;body.pivot=new Vector2(.5f,1);body.sizeDelta=new Vector2(0,height);scroll.content=body;return body;
        }
        void RenderRuneDetail()
        {
            if(runeRotateButton!=null)runeRotateButton.gameObject.SetActive(!runeViewing&&runeSelected!=null&&!runeEditor.IsInDraft(runeSelected)&&runeInventory.GetCommittedOwner(runeSelected)==null);
            if(runeDetails==null)return;foreach(Transform child in runeDetails){child.gameObject.SetActive(false);Destroy(child.gameObject);}float s=runeUnit,w=runeDetails.rect.width,h=runeDetails.rect.height;
            RuneTextAt(runeDetails,"선택 정보",10*s,4*s,w*.4f,19*s,10);
            RuneButton(runeDetails,"rune-clear-selection","선택 해제",()=>{runeSelected=null;runeCanvas.piece=null;runeNode=new HexCell(0,0);runeCanvas.Refresh();RenderRuneDetail();},w-80*s,3*s,74*s,22*s);
            float contentW=w-16*s;
            var selected=runeSelected==null?null:runeInventory.FindOwned(runeSelected);
            if(selected!=null)
            {
                float tile=Mathf.Min(128*s,h-75*s),textX=tile+22*s;
                var pickup=RuneButton(runeDetails,"rune-detail-pickup","",()=>{},9*s,31*s,tile,tile,true);pickup.interactable=!runeViewing;
                pickup.GetComponent<Image>().color=RuneV13Art.Color("#343b2b");
                var icon=Rect("Selected rune",pickup.transform);Place(icon,5*s,2*s,tile-10*s,tile-26*s);var g=MakeRuneGraphic(icon,"Selected shape");g.icon=true;g.piece=selected;g.rotation=runeRotation;g.raycastTarget=false;
                var drag=pickup.gameObject.AddComponent<RuneStorageDrag>();drag.board=runeCanvas;drag.piece=selected;drag.rotation=runeRotation;
                RuneTextAt(pickup.transform,"끌어서 배치",2*s,tile-24*s,tile-4*s,20*s,10,TextAnchor.MiddleCenter);
                RuneTextAt(runeDetails,RuneV13Catalog.TypeName(selected.Type)+" · G"+selected.Grade+" · "+Loc.F("{0}칸",selected.Shape.Size),textX,32*s,w-textX-9*s,27*s,15);
                runeDetail=RuneTextAt(runeDetails,runeEditor.IsInDraft(selected.InstanceId)?"배치 중 · 이동하거나 회수할 수 있습니다.":"보관 중 · 배치할 칸을 선택하세요.",textX,62*s,w-textX-9*s,31*s,10);runeDetail.name="rune-detail";
                var nodes=RuneEffects().Nodes.Where(n=>n.CoveringRuneId==selected.InstanceId).ToArray();
                string note=Loc.F("활성 {0}칸 · 불일치 {1}칸",nodes.Count(n=>n.IsPreviewActive),nodes.Count(n=>!n.IsPreviewActive));
                RuneTextAt(runeDetails,note,textX,97*s,w-textX-9*s,23*s,11);
                RuneTextAt(runeDetails,"밝은 문양은 활성, 블록과 같은 색의 어두운 문양은 비활성입니다.",textX,123*s,w-textX-9*s,h-164*s,10);
                RuneButton(runeDetails,"rune-rotate","60° 회전",RotateRune,9*s,h-36*s,(w-26*s)/2,29*s).interactable=!runeViewing;
                RuneButton(runeDetails,"rune-remove","룬 회수",RemoveRune,17*s+(w-26*s)/2,h-36*s,(w-26*s)/2,29*s).interactable=!runeViewing&&runeEditor.IsInDraft(selected.InstanceId);return;
            }
            var body=RuneDetailScroll(150*s);
            runeEditor.Board.TryGetCell(runeNode,out var cell);cell??=runeEditor.Board.Cells[0];var node=RuneEffects().Nodes.First(n=>n.Cell.Coordinate.Equals(cell.Coordinate));
            RuneIcon(body,cell.Glyph,3*s,4*s,36*s,cell.AbilityType<0?RuneText:RuneBoardGraphic.TypeColor(cell.AbilityType));
            RuneTextAt(body,RuneAbilityLine(cell),47*s,0,contentW-51*s,35*s,14);
            RuneTextAt(body,Loc.F("{0} 영역 · 능력 등급 {1}",RuneV13Catalog.RegionName(cell.RegionGrade),cell.NumericGrade),47*s,34*s,contentW-51*s,20*s,9);
            string state=cell.Ability.Meaning=="root"?"다섯 색의 공용 기점 · 룬을 덮을 수 없습니다.":!runeEditor.Board.IsOpen(cell,6)?"미개방 · 개방해도 능력은 켜지지 않습니다.":node.IsPreviewActive?"색 일치 · 능력 활성":node.CoveringRuneId!=null?"색 불일치 · 연결 통로로만 사용합니다.":"개방된 빈 칸 · 같은 색 룬으로 활성화합니다.";
            runeDetail=RuneTextAt(body,state,2*s,58*s,contentW-4*s,32*s,10);runeDetail.name="rune-detail";
            var description=RuneTextAt(body,RuneV13Catalog.ForMeaning(cell.Ability.Meaning).description,2*s,94*s,contentW-4*s,100*s,11);
            description.resizeTextForBestFit=false;float descriptionH=description.preferredHeight+5*s;description.rectTransform.sizeDelta=new Vector2(contentW-4*s,descriptionH);body.sizeDelta=new Vector2(0,94*s+descriptionH);
            RuneButton(runeDetails,"rune-ability-details","능력 상세",()=>ShowRuneAbility(cell),9*s,h-36*s,(w-26*s)/2,29*s);
            if(!runeEditor.Board.IsOpen(cell,6)&&cell.RegionGrade>0&&!RuneProgress.order.Contains(cell.RegionGrade))
                RuneButton(runeDetails,"rune-region-preview","영역 미리보기",()=>ShowRuneRegion(cell.RegionGrade),17*s+(w-26*s)/2,h-36*s,(w-26*s)/2,29*s);
            else if(!runeEditor.Board.IsOpen(cell,6))RuneButton(runeDetails,"rune-unlock","1P · 칸 개방",()=>UnlockRuneCell(cell.Coordinate),17*s+(w-26*s)/2,h-36*s,(w-26*s)/2,29*s).interactable=!runeViewing&&RuneMasteryProgress.CanUnlock(RuneProgress,cell.Coordinate);
            else RuneButton(runeDetails,"rune-detail-codex","능력 도감",ShowRuneCodex,17*s+(w-26*s)/2,h-36*s,(w-26*s)/2,29*s);
        }
        void RotateRune()
        {
            if(runeSelected==null||runeCanvas.Dragging)return;int next=(runeRotation+1)%6;var p=runeEditor.DraftPlacements.FirstOrDefault(p=>p.InstanceId==runeSelected);
            if(p!=null){if(!runeEditor.ValidatePlacement(p.InstanceId,p.Anchor,next).IsValid){ShowToast("회전하면 연결이 끊기거나 다른 룬과 겹칩니다.");return;}PushRuneUndo();runeEditor.TryRotateDraft(p.InstanceId,1,out _);runeEditor.TryCommit(out _);}
            runeRotation=next;runeStorageRotations[runeSelected]=next;runeCanvas.rotation=next;UpdateRuneContents();RenderRuneDetail();
        }
        void RemoveRune()
        {
            if(runeSelected==null)return;var remaining=runeEditor.DraftPlacements.Where(p=>p.InstanceId!=runeSelected).ToArray();
            if(!RunePlacementValidator.Validate(runeEditor.Board,6,remaining).IsValid){ShowToast("이 블록을 빼면 연결이 끊어집니다. 바깥 블록부터 회수하세요.");return;}
            PushRuneUndo();runeStorageRotations[runeSelected]=runeRotation;runeEditor.TryRemoveDraft(runeSelected,out _);runeEditor.TryCommit(out _);UpdateRuneContents();RenderRuneDetail();
        }
        void RebuildRuneOpenCells()
        {runeSource.placements=RuneGrowth.Capture(runeInventory);runeInventory=RuneGrowth.Restore(runeSource,6);runeEditor=runeInventory.BeginEdit(runeWeapon);ShowRunes();}
        void UnlockRuneCell(HexCell coordinate){if(runeViewing||!RuneMasteryProgress.CanUnlock(RuneProgress,coordinate))return;PushRuneUndo();RuneMasteryProgress.Unlock(RuneProgress,coordinate);RebuildRuneOpenCells();}
        void FocusRuneRegion(int region){runeCanvas=null;runeFocus=region;runeOverview=false;runeViews.Remove(runeWeapon);runeEditor.Board.TryGetStart(region,out runeNode);runeSelected=null;ShowRunes();}
        void SaveRuneDraft()
        {if(!StageRuneBoard())return;if(!game.Store.CommitRuneBoardState(RuneGrowth.Capture(runeInventory),runeSource.mastery,runeSource.revision,game.Active?game.Combat:null,runeSource.presets)){ShowToast(game.Store.Error);return;}ReloadRuneDraft();ShowRunes();ShowToast("모든 무기의 개방 경로와 룬 배치를 저장했습니다.");}
        void CloseRunes(){if(RuneDirty){Confirm(Loc.T("저장하지 않은 전체 룬 배치를 버리고 닫을까요?"),ExitRunes);return;}ExitRunes();}
        void ExitRunes(){runeSession=false;runeInventory=null;runeEditor=null;runeCanvas=null;runeUndo.Clear();if(game.Combat!=null)game.Combat.State.paused=runeWasPaused;if(game.Active)ShowBattle();else ShowTown();}
        void ShowRunePresets()
        {
            if(!StageRuneBoard())return;var body=RuneDialog("전체 배치 프리셋 · 5칸");
            RuneParagraph(body,"프리셋 5칸은 모든 무기의 배치를 함께 저장합니다. 룬을 복제하거나 숙련도·개방 경로를 되돌리지 않습니다.");
            for(int i=0;i<5;i++)
            {
                int slot=i;var preset=runeSource.presets[i];RuneParagraph(body,Loc.F("슬롯 {0} · {1} · 룬 {2}개",i+1,preset.occupied?(string.IsNullOrEmpty(preset.name)?Loc.T("전체 무기 배치"):preset.name):Loc.T("비어 있음"),preset.placements.Count),17);
                var row=Row(body,40*runeUnit);row.GetComponent<Image>().color=RuneBg;
                var save=Button(row,"등록",()=>NameRunePreset(slot));save.name="rune-preset-save-"+slot;Across(save,0,3,0,38*runeUnit);
                var load=Button(row,"불러오기",()=>LoadRuneDraftPreset(slot));load.name="rune-preset-load-"+slot;Across(load,1,3,0,38*runeUnit);load.interactable=preset.occupied;
                var clear=Button(row,"비우기",()=>{PushRuneUndo();runeSource.presets[slot]=new RunePreset();ShowRunes();ShowRunePresets();});clear.name="rune-preset-clear-"+slot;Across(clear,2,3,0,38*runeUnit);clear.interactable=preset.occupied;
                foreach(var button in row.GetComponentsInChildren<Button>()){button.GetComponent<Image>().color=RuneV13Art.Color("#353426");button.GetComponentInChildren<Text>().fontSize=Mathf.RoundToInt(12*runeUnit);}
            }
            RuneParagraph(body,"프리셋 변경도 변경 저장을 눌러야 보존됩니다.");
        }
        void NameRunePreset(int slot)
        {
            var body=RuneDialog("전체 배치 등록");RuneParagraph(body,Loc.F("슬롯 {0}에 모든 무기의 현재 배치를 등록합니다.",slot+1));
            var row=Row(body,42*runeUnit);var input=row.gameObject.AddComponent<InputField>();input.characterLimit=40;var text=Label(row,"",Mathf.RoundToInt(16*runeUnit),RuneText);Inset(text.rectTransform);input.textComponent=text;
            input.text=string.IsNullOrEmpty(runeSource.presets[slot].name)?Loc.T("각인 배치"):runeSource.presets[slot].name;
            RuneDialogAction(body,"등록",()=>{PushRuneUndo();runeSource.presets[slot]=new RunePreset{occupied=true,name=input.text.Trim(),placements=RuneGrowth.Capture(runeInventory)};ShowRunes();ShowRunePresets();}).name="rune-preset-confirm";
        }
        void LoadRuneDraftPreset(int slot)
        {
            var preset=runeSource.presets[slot];try{RuneGrowth.Validate(runeSource,preset.placements,6);}catch(Exception){ShowToast("프리셋의 룬이 없거나 현재 개방 경로에 맞지 않아 적용하지 않았습니다.");return;}
            PushRuneUndo();runeSource.placements=preset.placements.Select(RuneGrowth.Copy).ToList();runeInventory=RuneGrowth.Restore(runeSource,6);runeEditor=runeInventory.BeginEdit(runeWeapon);runeSelected=null;ShowRunes();
        }
        void ShowRuneFusion()
        {
            if(!StageRuneBoard())return;
            if(RuneDirty){ShowToast("합성 전에 전체 배치를 저장하거나 되돌려 주세요.");return;}
            runeFusionSelection.RemoveWhere(id=>!game.Store.Data.runes.owned.Any(r=>r.id==id&&!r.pending));
            pageRepaint=ShowRuneFusion;Base("rune-fusion","룬 합성","같은 색·등급·크기 두 개로 성장 · 실패와 수수료 없음");
            var grades=Row(content,76);for(int g=0;g<7;g++){int grade=g;var b=Button(grades,"G"+g,()=>{runeFusionGrade=grade;runeFusionSelection.Clear();ShowRuneFusion();},g==runeFusionGrade?gold:panel);UiTheme.Choice(b,g==runeFusionGrade);Across(b,g,7,2,70);}
            Note(content,"1~4칸 두 개는 같은 등급의 다음 크기가 됩니다. 5칸 두 개는 다음 등급 1칸이 됩니다. 모양은 달라도 되며, 한 번에 최대 100쌍을 합성합니다. 같은 색끼리만 짝을 지으며 결과에도 그 색을 유지합니다.",20,115,pale);
            var rates=BigButton(content,"룬 드롭 확률",ShowRuneDropRates);rates.name="rune-drop-rates";
            var state=game.Store.Data.runes;
            for(int size=1;size<=5;size++)
            {
                int rowSize=size;var owned=state.owned.Where(r=>r.grade==runeFusionGrade&&!r.pending&&RuneMasteryCatalog.ShapeById(r.shapeId).Size==rowSize&&!RuneReshape.ProtectedRune(state,r.id)).ToArray();
                var pending=state.owned.Where(r=>r.pending&&r.sourceGrade==runeFusionGrade&&r.sourceSize==rowSize).ToArray();
                Note(content,Loc.F("{0}칸 · 보관 {1}개 · 선택 {2}개 · 미수령 {3}개",size,owned.Length,owned.Count(r=>runeFusionSelection.Contains(r.id)),pending.Length),22,62,gold);
                Note(content,RuneEconomy.FusionOutput(runeFusionGrade,size,out int nextGrade,out int nextSize)?Loc.F("G{0} {1}칸 ×2 → G{2} {3}칸 ×1 · 성공률 100%",runeFusionGrade,size,nextGrade,nextSize):Loc.T("최종 단계 · 더 이상 합성할 수 없습니다."),20,70,pale);
                var row=Row(content,80);
                Across(Button(row,"한 쌍 선택",()=>{foreach(var r in owned.Where(r=>!runeFusionSelection.Contains(r.id)).GroupBy(r=>r.type).FirstOrDefault(g=>g.Count()>=2)?.Take(Math.Min(2,200-runeFusionSelection.Count))??Enumerable.Empty<OwnedRune>())runeFusionSelection.Add(r.id);ShowRuneFusion();}),0,3,2,74);
                Across(Button(row,"행 선택 해제",()=>{foreach(var r in owned)runeFusionSelection.Remove(r.id);ShowRuneFusion();}),1,3,2,74);
                var fuse=Button(row,"이 행 합성",()=>FuseSelectedRunes(owned.Where(r=>runeFusionSelection.Contains(r.id)).Select(r=>r.id).ToArray()));Across(fuse,2,3,2,74);fuse.interactable=RuneAscension.Validate(state,owned.Where(r=>runeFusionSelection.Contains(r.id)).Select(r=>r.id).ToArray())=="";
                var shapes=Rect("Fusion shapes",content);var grid=shapes.gameObject.AddComponent<GridLayoutGroup>();grid.cellSize=new Vector2(142,112);grid.spacing=new Vector2(8,8);grid.constraint=GridLayoutGroup.Constraint.FixedColumnCount;grid.constraintCount=4;shapes.gameObject.AddComponent<RuneAdaptiveGrid>();shapes.gameObject.AddComponent<ContentSizeFitter>().verticalFit=ContentSizeFitter.FitMode.PreferredSize;
                foreach(var group in owned.GroupBy(r=>new{r.shapeId,r.type}))
                {
                    var same=group.ToArray();var b=Button(shapes,Loc.F("모양 {0} ×{1}",RuneMasteryCatalog.ShapeById(group.Key.shapeId).ShapeNumber,group.Count()),()=>{var pick=same.FirstOrDefault(r=>!runeFusionSelection.Contains(r.id));if(pick!=null&&runeFusionSelection.Count<200)runeFusionSelection.Add(pick.id);else foreach(var r in same)runeFusionSelection.Remove(r.id);ShowRuneFusion();});var shapeRect=Rect("Fusion shape",b.transform);Span(shapeRect,4,4,4,68);var graphic=shapeRect.gameObject.AddComponent<RuneBoardGraphic>();graphic.icon=true;graphic.piece=new RunePiece(group.Key.shapeId,runeFusionGrade,RuneMasteryCatalog.ShapeById(group.Key.shapeId),group.Key.type);graphic.raycastTarget=false;var label=b.GetComponentInChildren<Text>();label.fontSize=16;Span(label.rectTransform,3,74,3,34);
                }
                if(pending.Length>0)
                {
                    Note(content,string.Join(" · ",pending.Select(r=>Loc.F("G{0} · {1}칸 · 모양 {2}",r.grade,RuneMasteryCatalog.ShapeById(r.shapeId).Size,RuneMasteryCatalog.ShapeById(r.shapeId).ShapeNumber))),19,72,pale);
                    var receive=BigButton(content,"이 등급의 모든 결과 수령",()=>{if(Time.unscaledTime<runeRevealUntil)return;if(game.Store.ClaimRunes(runeFusionGrade,state.revision)){ReloadRuneDraft();ShowRuneFusion();}else ShowToast(game.Store.Error);});receive.name="rune-claim";receive.interactable=Time.unscaledTime>=runeRevealUntil;
                    if(!receive.interactable)StartCoroutine(EnableRuneClaim(receive));
                }
            }
            FooterButton(0,2,"선택한 모든 행 합성",()=>FuseSelectedRunes(runeFusionSelection.ToArray()),true);FooterButton(1,2,"룬 보드로",()=>{runeFusionSelection.Clear();ShowRunes();});
        }
        void ShowRuneDropRates()
        {
            if(!StageRuneBoard())return;
            pageRepaint=ShowRuneDropRates;Base("rune-drop-rates","룬 드롭 확률","균열 단계가 높을수록 큰 룬이 등장합니다.");
            Note(content,Loc.F("일반 몬스터 {0}% · 정예 몬스터 {1}% · 보스 {2}개 확정",RuneEconomy.DropPercent(RiftRewardSource.Normal),RuneEconomy.DropPercent(RiftRewardSource.Elite),RuneEconomy.DropCount(RiftRewardSource.Boss)),23,95,gold);
            Note(content,"일반·정예는 처치마다 최대 1개입니다. 룬은 전용 보관함으로 자동 수령하며 장비 가방 공간을 쓰지 않습니다. 훈련과 소환 몬스터에서는 나오지 않습니다.",21,150,pale);
            Note(content,"아래 크기 확률은 룬 드롭에 성공했을 때의 비율입니다. 보스의 룬도 같은 표를 따릅니다. 획득 등급·능력 색·보드 능력 등급은 서로 다릅니다. 각 능력 색의 확률은 20%입니다.",21,125,pale);
            foreach(var band in RuneEconomy.Bands)
            {
                string range=band.MaximumStage==int.MaxValue?Loc.F("{0}단계 이상",band.MinimumStage):Loc.F("{0}~{1}단계",band.MinimumStage,band.MaximumStage);
                Note(content,Loc.F("{0} · G{1}",range,band.Grade),24,65,gold);
                Note(content,string.Join(" · ",Enumerable.Range(1,5).Select(s=>Loc.F("{0}칸 {1}%",s,band.SizePercent[s-1]))),20,90,pale);
            }
            Note(content,"모양은 선택된 크기 안에서 동일한 확률로 결정됩니다. 합성은 같은 등급·크기 두 개로 진행하며, 배치 중인 룬은 먼저 회수해야 합니다.",21,125,pale);
            FooterButton(0,2,"룬 합성",ShowRuneFusion);FooterButton(1,2,"룬 보드로",ShowRunes);
            foreach(var text in content.GetComponentsInChildren<Text>().Where(t=>t.transform.parent.name=="Text row"))text.gameObject.AddComponent<RuneTextFit>();
        }
        System.Collections.IEnumerator EnableRuneClaim(Button button){yield return new WaitForSecondsRealtime(Mathf.Max(0,runeRevealUntil-Time.unscaledTime));if(button!=null)button.interactable=true;}
        void FuseSelectedRunes(string[] ids)
        {
            if(game.Store.FuseRunes(ids,game.Store.Data.runes.revision)){runeFusionSelection.Clear();runeRevealUntil=Time.unscaledTime+.6f;ReloadRuneDraft();ShowRuneFusion();}else ShowToast(game.Store.Error);
        }
        void ShowRunePractice()
        {
            pageRepaint=ShowRunePractice;Base("rune-practice","룬 배치 연습",Loc.F("{0} / 5 · 실제 룬과 저장에는 영향을 주지 않습니다.",runePractice.Page+1));
            string[] hints={"같은 색의 서로 다른 조각 사이에는 한 칸 간격이 필요합니다.","조각의 모든 칸이 보드 안에 있어야 하며, 같은 색 조각과 맞닿으면 안 됩니다.","다른 색의 조각끼리는 맞닿을 수 있지만 겹칠 수 없습니다.","조각을 회전해서 같은 색의 능력 칸 두 개를 덮어 보세요.","두 색의 능력 칸을 각각 두 개씩 켜 보세요. 다른 색을 덮으면 능력이 켜지지 않습니다."};
            Note(content,hints[runePractice.Page],22,115,pale);
            var panel=Row(content,410);panel.gameObject.AddComponent<RectMask2D>();var board=MakeRuneGraphic(panel,"rune-practice-board");board.practice=runePractice;
            if(runePractice.SelectedId!=null){board.piece=runePractice.Pieces.First(p=>p.InstanceId==runePractice.SelectedId);board.rotation=runePractice.RotationOf(board.piece.InstanceId);}
            board.changed=()=>{if(board.piece!=null)runePractice.Select(board.piece.InstanceId);ShowRunePractice();};board.rejected=ShowToast;board.Refresh();
            var pieces=Row(content,112);int i=0;foreach(var p in runePractice.Pieces)
            {
                var piece=p;var b=Button(pieces,"",()=>{runePractice.Select(piece.InstanceId);ShowRunePractice();});b.name="practice-piece-"+i;Across(b,i++,runePractice.Pieces.Count,2,108);
                var graphic=MakeRuneGraphic((RectTransform)b.transform,"Practice shape");graphic.icon=true;graphic.piece=piece;graphic.rotation=runePractice.RotationOf(piece.InstanceId);graphic.raycastTarget=false;
                var drag=b.gameObject.AddComponent<RuneStorageDrag>();drag.board=board;drag.piece=piece;drag.rotation=graphic.rotation;
            }
            if(runePractice.CanRotate)BigButton(content,"선택한 룬 회전",()=>{runePractice.RotateSelected();ShowRunePractice();});
            var reset=BigButton(content,"이 페이지 초기화",()=>{runePractice.Reset();ShowRunePractice();});reset.interactable=runePractice.ResetAvailable;
            if(runePractice.Page>=3)Note(content,runePractice.GoalReached?"능력 활성화 목표를 달성했습니다.":"색과 회전을 바꾸며 능력 칸을 연결해 보세요.",22,80,gold);
            var nav=Row(content,80);var previous=Button(nav,"이전",()=>{runePractice.GoToPage(runePractice.Page-1);ShowRunePractice();});Across(previous,0,2,2,74);previous.interactable=runePractice.Page>0;
            var next=Button(nav,"다음",()=>{runePractice.GoToPage(runePractice.Page+1);ShowRunePractice();});Across(next,1,2,2,74);next.interactable=runePractice.Page<4;
            FooterButton(0,1,"룬 보드로",ShowRunes);
        }
    }
}
