using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Hellscript
{
    public sealed partial class InventoryWindow : MonoBehaviour
    {
        GameStore store;GameCatalog catalog;Font font;Texture2D atlas;Action closed,equipmentChanged;Func<float> readingScale;
        RectTransform canvasRoot,frame,body,overlays,dialog,dialogBody,popover,filterMenu;ScrollRect bagScroll;
        Rect safe;float width,height,scale,textScale,bagOffset;bool ready,landscape;string language;
        int category,grades=31;InventoryOrder order=InventoryOrder.Equipment;string filterKind;
        static readonly InventoryOrder[] Orders={InventoryOrder.Equipment,InventoryOrder.Newest,InventoryOrder.Oldest,InventoryOrder.Grade,InventoryOrder.Level};
        static readonly string[] OrderLabels={"위치순","최근순","오래된 순","등급순","레벨순"};
        EquipmentShopWindow autoSettings;
        ContentWindowView rewardBoxesWindow;
        bool selecting;readonly HashSet<string> selected=new HashSet<string>();
        string detailId,dialogKind;Text status;float messageUntil;string message="";
        readonly Color ink=UiTheme.Background,gold=UiTheme.Gold,pale=UiTheme.Text,muted=UiTheme.Muted,green=UiTheme.Success,red=UiTheme.Danger;
        static readonly string[] GradeNames={"일반","마법","레어","전설","세트"};
        static readonly string[] GradeColors=EquipmentGradePalette.Hex;
        HeroSave Hero=>store.Data.Hero;
        public bool Landscape=>landscape;
        public bool Selecting=>selecting;
        public int SelectionCount=>selected.Count;
        public float SlotSize=>UiTheme.SlotSize(landscape);
        public RectTransform FrameRect=>frame;
        public RectTransform DialogRect=>dialog;
        public ScrollRect BagScroll=>bagScroll;
        public InventorySalvagePlan SalvagePlan {get;private set;}
        HeroSave EquipmentPreviewHero(HeroSave hero)
        {
            var run=store.Data.suspendedRun;
            if(run?.heroId==hero.id&&run.slotLevels!=null){hero=RuneGrowth.Copy(hero);hero.slotProgress=new SlotProgress{levels=(int[])run.slotLevels.Clone()};}
            return hero;
        }
        HeroStats EquipmentStats(HeroSave hero)=>new HeroStats(EquipmentPreviewHero(hero),false,store.Data.runes);
        public static InventoryWindow Open(Transform parent,GameStore store,GameCatalog catalog,Font font,Func<float> readingScale,Action closed,Action equipmentChanged)
        {
            var go=new GameObject("Inventory",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            go.transform.SetParent(parent,false);var canvas=go.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=310;
            var view=go.AddComponent<InventoryWindow>();view.store=store;view.catalog=catalog;view.font=font;view.readingScale=readingScale;view.closed=closed;view.equipmentChanged=equipmentChanged;
            view.canvasRoot=(RectTransform)go.transform;view.atlas=Resources.Load<Texture2D>("Art/EquipmentAtlas");view.ready=true;view.Reflow();ContentWindowHost.Attach(view,view.Escape);StoreViewBinding.Attach(view,store,view.RefreshCommittedView,()=>view.dialog==null||view.dialogKind=="wallet");return view;
        }
        void Update()
        {
            if(!ready)return;
            if(safe!=UiSafeArea.Current||language!=Loc.Language||Mathf.Abs(textScale-readingScale())>.001f)Reflow();
            if(messageUntil>0&&Time.unscaledTime>messageUntil){messageUntil=0;message="";UpdateStatus();}
        }
        public void Close(){if(!ready)return;CancelDrag();ready=false;rewardBoxesWindow?.Close();if(autoSettings!=null)autoSettings.Close();ContentWindowHost.Detach(this);closed?.Invoke();Destroy(gameObject);}
        public void RequestClose(){if(portalResume!=null){if(!portalResume()){string error=portalError();ShowError(error!=""?error:store.Error);}}else Close();}
        public void Escape(){if(ghost!=null){CancelDrag();return;}if(rangeHelp!=null){CloseRangeHelp();return;}if(filterMenu!=null){CloseFilter();return;}if(popover!=null){ClosePopover();return;}if(dialog!=null){Dismiss();return;}if(selecting){ToggleSelection();return;}RequestClose();}
        void Reflow()
        {
            string kind=dialogKind,id=detailId,filter=filterKind;RememberScroll();CancelDrag();
            safe=UiSafeArea.Current;language=Loc.Language;textScale=readingScale();landscape=safe.width>safe.height;
            width=landscape?800:405;height=landscape?450:720;scale=UiTheme.Scale(safe);
            var scaler=GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ConstantPixelSize;scaler.scaleFactor=scale;
            Clear(canvasRoot);dialog=null;popover=null;filterMenu=null;filterKind=null;
            var backdrop=Panel(canvasRoot,"Inventory backdrop","080a08","080a08");Stretch(backdrop);
            frame=Panel(canvasRoot,"Inventory frame","22231b","141710","856840");frame.anchorMin=frame.anchorMax=Vector2.zero;frame.pivot=Vector2.zero;
            frame.anchoredPosition=(safe.position+(safe.size-new Vector2(width,height)*scale)/2)/scale;frame.sizeDelta=new Vector2(width,height);frame.gameObject.AddComponent<RectMask2D>();
            body=Rect("Inventory content",frame);Stretch(body);overlays=Rect("Inventory overlays",frame);Stretch(overlays);
            Draw();
            if(kind=="detail"&&FindItem(id)!=null)ShowDetail(id);else if(kind=="compare"&&FindItem(id)!=null)ShowComparison(id);
            else if(kind=="stats")ShowStats();else if(kind=="bulk")ShowSalvage(true);else if(kind=="selected")ShowSalvage(false);
            else if(kind=="potion")ShowPotionSettings();else if(kind=="potion-picker")ShowPotionPicker(potionSlotIndex);
            else if(kind=="wallet")ShowWallet();
            else if(filter!=null)ShowFilter(filter);
        }
        void RememberScroll(){if(bagScroll!=null)bagOffset=bagScroll.content.anchoredPosition.y;}
        public void Repaint(){Repaint(false);}
        void Repaint(bool resetScroll){CancelDrag();RememberScroll();if(resetScroll)bagOffset=0;Clear(body);Draw();}
        void Draw()
        {
            var header=Panel(body,"Header","342c20","211e16","80643d");Place(header,0,0,width,34);
            Txt(header,"HELLSCRIPT",12,0,95,34,10,gold);
            Txt(header,"가방",width/2-55,0,110,34,20,pale,TextAnchor.MiddleCenter);
            Btn(header,"×",width-34,2,30,30,RequestClose,false,20).name="inventory-close";
            var wallet=Panel(body,"Wallet summary","28261b","191c14","655237");Place(wallet,0,34,width,WalletSummaryHeight);
            DrawWalletSummary(wallet);
            float contentTop=34+WalletSummaryHeight;
            float footerHeight=portalResume==null?28:92;
            float characterWidth=landscape?258:width,characterHeight=landscape?height-contentTop-(portalResume==null?28:0):238;
            DrawCharacter(0,contentTop,characterWidth,characterHeight);
            float bagTop=contentTop+(landscape?0:characterHeight);
            DrawBag(landscape?258:0,bagTop,landscape?542:width,height-bagTop-footerHeight);
            float footerLeft=portalResume!=null&&landscape?258:0;
            var footer=Panel(body,"Footer","28261b","191c14","655237");Place(footer,footerLeft,height-footerHeight,width-footerLeft,footerHeight);
            if(portalResume!=null){DrawPortalFooter(footer);RestoreBagScroll();return;}
            float footerLine=0;
            Btn(footer,"전체 재화",8,footerLine+2,112,24,ShowWallet,false,10).name="inventory-wallet";
            Btn(footer,"보상 상자",124,footerLine+2,105,24,()=>{rewardBoxesWindow?.Close();rewardBoxesWindow=RewardBoxesWindow.Open(transform.parent,store,readingScale,()=>rewardBoxesWindow=null);},false,10).name="inventory-reward-boxes";
            var classLabel=Txt(footer,Loc.T(catalog.classNames[(int)Hero.heroClass])+" · Lv."+Hero.level,width-168,footerLine,96,28,10,muted,TextAnchor.MiddleRight);
            classLabel.resizeTextForBestFit=true;classLabel.resizeTextMinSize=8;classLabel.resizeTextMaxSize=classLabel.fontSize;
            RangeToggle(footer,width-64,footerLine,"inventory-range-toggle");
            RestoreBagScroll();
        }
        void RestoreBagScroll(){Canvas.ForceUpdateCanvases();bagScroll.content.anchoredPosition=new Vector2(0,Mathf.Clamp(bagOffset,0,Mathf.Max(0,bagScroll.content.rect.height-bagScroll.viewport.rect.height)));}
        void DrawCharacter(float x,float y,float w,float h)
        {
            var panel=Panel(body,"Character equipment","26271d","171b13","5e5039");Place(panel,x,y,w,h);
            var target=panel.gameObject.AddComponent<InventoryDropTarget>();target.window=this;target.slot=-1;
            Txt(panel,catalog.classNames[(int)Hero.heroClass],12,2,125,28,15,pale);
            Btn(panel,"전체 능력치",w-111,3,99,25,ShowStats,false,10).name="inventory-stats";
            float equipmentHeight=landscape?328:230;
            var equipment=CharacterEquipmentView.Create(panel,Hero,w,equipmentHeight,landscape,EquipmentViewSource.Owned,(host,p)=>EquipmentCell(host,p.slot,p.index,p.rect.x,p.rect.y),weaponTray:landscape);equipment.transform.SetAsFirstSibling();
            var weapon=CharacterEquipmentView.Positions(w,equipmentHeight,landscape,weaponTray:landscape).First(p=>p.slot==0&&p.index==1).rect;
            if(EquipmentSlots.TwoHanded(EquipmentSlots.At(Hero,0,0)))
            {var link=Panel(panel,"Two hand link","ba9a5e","ba9a5e");Place(link,weapon.x-UiTheme.Gap,weapon.y+SlotSize/2,UiTheme.Gap,3);link.GetComponent<Image>().raycastTarget=false;}
            DrawPotionSlots(panel,w,weapon);
            var stats=EquipmentStats(Hero);float sw=(w-24)/3;
            foreach(var entry in new[]{("공격 기준",stats.damage),("방어도",stats.armor),("최대 HP",stats.hp)}.Select((v,n)=>(v,n)))
            {Txt(panel,entry.v.Item1,12+sw*entry.n,h-25,sw*.48f,23,9,muted);Txt(panel,entry.v.Item2.ToString("0"),12+sw*entry.n+sw*.48f,h-25,sw*.52f,23,13,gold);}
        }
        void EquipmentCell(Transform parent,int slot,int index,float x,float y)
        {
            var item=EquipmentSlots.At(Hero,slot,index);var r=DrawCell(parent,item,x,y,SlotSize,false);
            r.name="equipment-"+slot+"-"+index;var drop=r.gameObject.AddComponent<InventoryDropTarget>();drop.window=this;drop.slot=slot;drop.index=index;
            if(item==null)Txt(r,EquipmentSlots.Label(slot,index),2,3,SlotSize-4,SlotSize-6,9,muted,TextAnchor.MiddleCenter);
            else
            {
                var label=Panel(r,"Equipment label","0c100cdf","0c100cdf");Place(label,1,SlotSize-13,SlotSize-2,12);label.GetComponent<Image>().raycastTarget=false;
                var caption=Txt(label,slot==0&&EquipmentSlots.TwoHanded(item)?"양손 점유":EquipmentSlots.Label(slot,index),0,0,SlotSize-2,12,8,gold,TextAnchor.MiddleCenter);caption.resizeTextForBestFit=true;caption.resizeTextMinSize=6;caption.resizeTextMaxSize=caption.fontSize;
                if(index==1&&EquipmentSlots.TwoHanded(item))foreach(var image in r.GetComponentsInChildren<RawImage>())image.color=new Color(1,1,1,.35f);
            }
        }
        IEnumerable<Item> VisibleItems()=>Storage.Order(Hero.inventory.Where(i=>!i.equipped&&(!selecting||!i.locked)&&(category==0||Storage.Category(i)==category)&&(grades&(1<<Storage.Grade(i)))!=0),order);
        void DrawBag(float x,float y,float w,float h)
        {
            var panel=Panel(body,"Bag panel","1f221a","181b14");Place(panel,x,y,w,h);var drop=panel.gameObject.AddComponent<InventoryDropTarget>();drop.window=this;drop.slot=-2;
            Txt(panel,"소지품",14,0,94,24,12,pale).name="inventory-bag-title";Txt(panel,Loc.F("{0} / {1}",Hero.inventory.Count(i=>!i.equipped),Hero.capacity),w-113,0,99,24,11,gold,TextAnchor.MiddleRight);
            string[] categories={"전체","무기","방어구","장신구"};float tab=(w-28)/4;
            for(int n=0;n<4;n++){int c=n;var button=Btn(panel,categories[n],14+n*tab,24,tab-3,24,()=>{category=c;Repaint(true);},false,11);button.name="inventory-category-"+n;UiTheme.Choice(button,category==n);}
            float filterWidth=(w-36)/2;
            FilterButton(panel,grades==31?Loc.T("모든 등급"):Loc.F("등급 · {0}개",GradeNames.Where((g,n)=>(grades&(1<<n))!=0).Count()),14,52,filterWidth,"grade");
            FilterButton(panel,Loc.F("정렬 · {0}",OrderLabels[Array.IndexOf(Orders,order)]),22+filterWidth,52,filterWidth,"order");
            const float gridTop=84,bottom=42;float gh=h-gridTop-bottom;
            bagScroll=Scroll(panel,"Inventory slots",14,gridTop,w-28,gh,out var grid);
            int columns=UiTheme.Columns(w-32,landscape);float step=SlotSize+UiTheme.Gap,gridWidth=columns*step-UiTheme.Gap,left=(w-32-gridWidth)/2;
            var items=VisibleItems().ToArray();bool fixedPositions=category==0&&grades==31&&order==InventoryOrder.Equipment;
            int count=fixedPositions?Math.Max(Hero.capacity,items.Select(i=>i.storageSlot+1).DefaultIfEmpty(0).Max()):Math.Max(columns*3,items.Length);
            for(int n=0;n<count;n++)
            {
                var item=fixedPositions?items.FirstOrDefault(i=>i.storageSlot==n):n<items.Length?items[n]:null;
                var r=DrawCell(grid,item,left+n%columns*step,n/columns*step,SlotSize,true);r.name="bag-slot-"+n;
            }
            grid.sizeDelta=new Vector2(0,Mathf.Ceil(count/(float)columns)*step-6);
            float by=h-bottom;var rail=Panel(panel,"Inventory actions","24251b","1a1c14","5a4c32");Place(rail,14,by,w-28,bottom-5);
            float bw=(w-48)/3;
            Btn(rail,selecting?"분해 취소":"분해",0,2,bw,34,ToggleSelection,false,11).name="inventory-dismantle";
            Btn(rail,selecting?"자동 선택":"일괄 분해",bw+10,2,bw,34,()=>{if(selecting)AutoSelect();else ShowSalvage(true);},false,11).name="inventory-bulk";
            var action=Btn(rail,Loc.T("선택 분해")+" ("+selected.Count+")",(bw+10)*2,2,bw,34,()=>ShowSalvage(false),true,11);action.name="inventory-selected";UiTheme.Button(action,destructive:true);action.gameObject.SetActive(selecting&&selected.Count>0);
            var notice=Panel(panel,"Inventory toast","27271bf5","171b13f5","655237");Place(notice,18,h-bottom-34,w-36,30);
            notice.GetComponent<Image>().raycastTarget=false;
            status=Txt(notice,"",8,2,w-52,26,10,pale);status.name="inventory-status";WalletFit(status);UpdateStatus();
        }
        void UpdateStatus()
        {
            if(status==null)return;
            status.text=message!=""?Loc.T(message):"";
            status.transform.parent.gameObject.SetActive(message!="");
        }
        public void Toast(string value){message=value;messageUntil=Time.unscaledTime+3.2f;UpdateStatus();}
        public void AutoSelect()
        {
            selected.Clear();foreach(string id in InventorySalvagePlan.AutoSelect(store.Data))selected.Add(id);
            selecting=true;Dismiss();Repaint();
        }
        void OpenAutoSettings(bool returnToBulk)
        {
            if(autoSettings!=null)return;
            autoSettings=EquipmentShopWindow.OpenAutoSettings(transform.parent,store,font,readingScale,()=>
            {autoSettings=null;if(!ready)return;if(returnToBulk)ShowSalvage(true);else Repaint();});
        }
        public void ToggleSelection(){selecting=!selecting;selected.Clear();Dismiss();Repaint();}
        Item FindItem(string id)=>Hero.inventory.Find(i=>i.id==id);
        public void SelectItem(string id)
        {
            if(Time.unscaledTime<suppressClickUntil)return;
            var item=FindItem(id);if(item==null)return;
            if(!selecting||item.equipped){GameAudio.Pick(item);ShowDetail(id);return;}
            if(!InventorySalvagePlan.Eligible(store.Data,item)){GameAudio.Refuse();Toast("보호 중인 아이템은 분해할 수 없습니다.");return;}
            GameAudio.Pick(item);
            if(!selected.Remove(id))selected.Add(id);Repaint();
        }
        RectTransform DrawCell(Transform parent,Item item,float x,float y,float size,bool bag)
        {
            var r=EquipmentSlotView.Create(parent,item,x,y,size,font,item!=null&&selected.Contains(item.id),!bag,bag&&selecting&&item!=null&&InventorySalvagePlan.Eligible(store.Data,item));
            var marker=r.gameObject.AddComponent<InventoryCell>();marker.window=this;marker.itemId=item?.id;marker.bag=bag;return r;
        }
        public Button Find(string name)=>GetComponentsInChildren<Button>().FirstOrDefault(b=>b.name==name);
        public InventoryCell Cell(string id,bool bag)=>GetComponentsInChildren<InventoryCell>().FirstOrDefault(c=>c.itemId==id&&c.bag==bag&&c.transform.IsChildOf(body));
    }
}
