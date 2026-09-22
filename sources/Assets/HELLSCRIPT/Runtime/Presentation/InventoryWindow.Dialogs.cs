using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    public sealed partial class InventoryWindow
    {
        public void Dismiss(){CloseRangeHelp();CloseFilter();Clear(overlays);dialog=null;popover=null;dialogKind=null;detailId=null;SalvagePlan=null;}
        void Modal(string kind,string title,float w,float h)
        {
            Dismiss();dialogKind=kind;
            var shade=Btn(overlays,"",0,0,width,height,Dismiss);shade.name="inventory-dialog-backdrop";shade.GetComponent<StorageSurface>().Paint("050805ce","050805ce");
            dialog=Panel(overlays,"Inventory dialog "+kind,"302b1f","1b1e15","a68b59");Place(dialog,(width-w)/2,(height-h)/2,w,h);
            bool itemWindow=kind=="detail"||kind=="compare";
            Txt(dialog,title,14,0,w-(itemWindow?130:58),39,16,gold);Btn(dialog,"×",w-34,4,27,28,Dismiss,false,19).name="inventory-dialog-close";Rule(dialog,40,w);
            if(itemWindow)RangeToggle(dialog,w-102,5,"inventory-detail-range-toggle");
        }
        void ItemInfo(Transform parent,Item item,float w,ref float y,bool concise=false,bool stackedHeader=false)
        {
            var shared=ItemDetailView.Append(parent,item,w-12,font,textScale,EquipmentViewSource.Owned,icon:!concise);float h=shared.sizeDelta.y;UiLayout.Place(shared,6,y,w-12,h);y+=h+4;
            if(!item.equipped&&!InventorySalvagePlan.Eligible(store.Data,item))Paragraph(parent,"잠금·장착·보석·프리셋 보호 장비는 분해에서 제외됩니다.",12,ref y,w-24,UiTheme.Caption,muted);
        }
        public void ShowDetail(string id)
        {
            var item=FindItem(id);if(item==null)return;if(!store.ReviewInventoryItem(id))Toast(store.Error);float w=landscape?360:width-16,h=height-16;
            Modal("detail","아이템 정보",w,h);detailId=id;Place(dialog,width-w-8,8,w,h);
            Scroll(dialog,"Item information",8,45,w-16,h-148,out var content);float y=8;ItemInfo(content,item,w-22,ref y);content.sizeDelta=new Vector2(0,y+8);
            float fy=h-94;Rule(dialog,fy,w);
            var lockButton=Btn(dialog,item.locked?"잠금 해제":"잠금",w-96,fy+9,84,34,()=>
            {
                if(store.SetInventoryLock(Guid.NewGuid().ToString("N"),id,!item.locked)){selected.Remove(id);Repaint();ShowDetail(id);}else Toast(store.Error);
            },item.locked,11);lockButton.name="inventory-lock";
            Btn(dialog,item.equipped?"장착 해제":"장착",12,fy+9,w-118,34,()=>{if(item.equipped)Unequip(id);else RequestEquip(id);},true,12).name="inventory-equip";
            if(!item.equipped)Btn(dialog,"장착 비교",12,fy+51,112,29,()=>ShowComparison(id),false,11).name="inventory-compare";
            Btn(dialog,"닫기",w-96,fy+51,84,29,Dismiss,false,11).name="inventory-detail-close";
        }
        void RequestEquip(string id)
        {
            var item=FindItem(id);if(item==null)return;var choices=EquipmentSlots.Targets(Hero,item);
            if(choices.Length>1&&choices.All(index=>EquipmentSlots.At(Hero,item.slot,index)!=null)){ShowComparison(id);return;}
            Equip(id);
        }
        public bool Equip(string id,int index=-1)
        {
            bool ok=store.EquipInventoryItem(Guid.NewGuid().ToString("N"),id,index);
            if(ok){equipmentChanged?.Invoke();selected.Remove(id);Dismiss();Repaint();Toast("장비를 교체했습니다.");}
            else ShowError(store.Error);return ok;
        }
        public bool Unequip(string id)
        {
            bool ok=store.UnequipInventoryItem(Guid.NewGuid().ToString("N"),id);
            if(ok){equipmentChanged?.Invoke();Dismiss();Repaint();Toast("장비를 소지품으로 옮겼습니다.");}else ShowError(store.Error);return ok;
        }
        void ShowError(string value)
        {
            Toast(value);
            if(dialog!=null){ClosePopover();popover=Panel(overlays,"Inventory notice","3d2b1d","292317","ae8256");float w=Mathf.Min(width-32,330);Place(popover,(width-w)/2,height/2-75,w,150);Txt(popover,value,14,12,w-28,84,12,red);Btn(popover,"확인",w-91,106,77,31,ClosePopover,true).name="inventory-notice-close";}
        }
        public void ShowComparison(string id)
        {
            var item=FindItem(id);if(item==null||item.equipped)return;if(!store.ReviewInventoryItem(id,true))Toast(store.Error);
            float w=width-16,h=landscape?422:height-32;Modal("compare","장착 비교",w,h);detailId=id;
            var targets=EquipmentSlots.Targets(Hero,item);
            var host=Rect("Inventory comparison body",dialog);Place(host,12,46,w-24,h-99);
            EquipmentComparisonView.Create(host,EquipmentPreviewHero(Hero),item,store.Data.runes,font,atlas,textScale);
            float bw=(w-24-(targets.Length-1)*8)/targets.Length;
            for(int n=0;n<targets.Length;n++){int index=targets[n];Btn(dialog,Loc.F("{0}에 장착",EquipmentSlots.Label(item.slot,index)),12+n*(bw+8),h-43,bw,32,()=>Equip(id,index),true,11).name="inventory-equip-"+index;}
        }
        public void ShowStats()
        {
            float w=landscape?630:width-16,h=height-24;Modal("stats","전체 능력치",w,h);
            var stats=EquipmentStats(Hero);Scroll(dialog,"All attributes",10,47,w-20,h-96,out var content);float y=6;
            string[] pages={"핵심 능력치","공격 능력치","방어 능력치","자원 능력치","유틸리티 능력치"};
            foreach(StatPage page in Enum.GetValues(typeof(StatPage)))
            {
                Paragraph(content,pages[(int)page],10,ref y,w-46,14,gold);
                foreach(var stat in StatCatalog.Page(page))
                {
                    var row=Rect("attribute-"+(int)stat.id,content);Place(row,10,y,w-46,29);
                    var name=Txt(row,stat.Name,0,0,(w-46)*.66f,29,11,pale);var value=Txt(row,stat.FormatSheet(stats.Sheet(stat.id)),(w-46)*.66f,0,(w-46)*.34f,29,11,gold,TextAnchor.MiddleRight);
                    float rowHeight=Mathf.Max(29,Mathf.Max(name.preferredHeight,value.preferredHeight)+5);Place(row,10,y,w-46,rowHeight);name.rectTransform.sizeDelta=new Vector2((w-46)*.66f,rowHeight);value.rectTransform.sizeDelta=new Vector2((w-46)*.34f,rowHeight);y+=rowHeight;
                    if(stat.id==StatId.CrowdControlDuration||stat.id==StatId.MaximumStamina||stat.id==StatId.StaminaRegeneration)Paragraph(content,"이 게임에는 아직 적용되는 곳이 없습니다",10,ref y,w-46,9,muted);
                }
                y+=9;
            }
            content.sizeDelta=new Vector2(0,y);Txt(dialog,Loc.F("전체 {0}개 속성",StatCatalog.Count),14,h-43,w-116,32,10,muted);Btn(dialog,"닫기",w-99,h-43,85,32,Dismiss,false).name="inventory-stats-close";
        }
        public void ShowSalvage(bool bulk)
        {
            float w=landscape?752:width-16,h=landscape?414:616;
            Modal(bulk?"bulk":"selected",bulk?"일괄 분해":"선택 분해",w,h);
            float top=48;
            if(bulk)
            {
                Txt(dialog,"상점과 공유하는 자동 선택 설정",14,top,w-182,28,11,gold);
                Btn(dialog,"자동 선택 설정",w-160,top,146,29,()=>OpenAutoSettings(true),false,11).name="salvage-settings";
                top+=34;var settings=Hero.equipmentShop.auto;
                Txt(dialog,Loc.F("등급 {0}개 · 장비 종류 {1}개 · 제외 옵션 적용",ShopAutoSelect.Grades.Where((g,n)=>(settings.gradeMask&(1<<n))!=0).Count(),settings.types.Count(t=>t.sell)),14,top,w-28,28,10,muted);
                top+=34;
            }
            else {Txt(dialog,"선택한 아이템을 분해하시겠습니까?",14,top,w-28,30,14,gold);top+=38;}
            var items=Hero.inventory.Where(i=>InventorySalvagePlan.Eligible(store.Data,i)&&(bulk?ShopAutoSelect.MatchesFilters(i,Hero.equipmentShop.auto):selected.Contains(i.id))).OrderBy(i=>i.storageSlot).ToArray();
            SalvagePlan=new InventorySalvagePlan(store.Data,items.Select(i=>i.id));
            Txt(dialog,Loc.F("분해 대상 · {0}개",items.Length),14,top,w-28,23,12,pale);top+=26;
            // Always three rows. Empty filters never shrink the modal; additional rows scroll inside this area.
            int columns=landscape?11:6;float size=landscape?54:52,step=size+6,gh=3*step-6;
            var scroll=Scroll(dialog,"Salvage target slots",14,top,w-28,gh,out var grid);grid.name="Salvage target content";
            float left=(w-34-(columns*step-6))/2;
            for(int n=0;n<Math.Max(columns*3,items.Length);n++)
            {
                var item=n<items.Length?items[n]:null;var cell=DrawCell(grid,item,left+n%columns*step,n/columns*step,size,false);
                var marker=cell.GetComponent<InventoryCell>();marker.inspectOnly=true;
            }
            grid.sizeDelta=new Vector2(0,Mathf.Ceil(Math.Max(columns*3,items.Length)/(float)columns)*step-6);
            top+=gh+5;
            // Landscape uses the fixed right footer strip; portrait leaves breathing room below the three rows.
            float fy=h-49;
            Txt(dialog,Loc.F("재료 {0:N0} · 코어 {1} · 강화석 {2:N0} · 위상 {3}",SalvagePlan.Materials,SalvagePlan.Cores,SalvagePlan.Stones,SalvagePlan.Aspects),14,Mathf.Min(top,fy-22),w-28,21,11,gold);
            if(!landscape)Txt(dialog,"아이템을 누르면 정보를 확인할 수 있습니다.",14,top+26,w-28,27,10,muted);
            Rule(dialog,fy-4,w);
            Btn(dialog,"취소",14,fy,82,34,Dismiss,false,11).name="salvage-cancel";
            var plan=SalvagePlan;
            var confirm=Btn(dialog,Loc.F("{0}개 분해",items.Length),w-164,fy,150,34,()=>
            {
                if(store.DismantleInventory(plan)){selected.Clear();selecting=false;Dismiss();Repaint();Toast("선택한 아이템을 분해했습니다.");}
                else ShowError(store.Error);
            },true,12);confirm.name="salvage-confirm";confirm.interactable=items.Length>0;
        }
        void ClosePopover(){CloseRangeHelp();if(popover!=null){popover.gameObject.SetActive(false);Destroy(popover.gameObject);}popover=null;}
        public void Inspect(string id)
        {
            var item=FindItem(id);if(item==null)return;ClosePopover();
            float w=Mathf.Min(width-32,300),h=landscape?258:300;popover=Panel(overlays,"Item quick information","343023","202419","b59660");Place(popover,(width-w)/2,(height-h)/2,w,h);
            Txt(popover,"아이템 정보",12,0,w-118,31,12,gold);Btn(popover,"×",w-31,3,26,25,ClosePopover,false,17).name="salvage-info-close";
            RangeToggle(popover,w-99,2,"inventory-quick-range-toggle");
            Scroll(popover,"Quick information scroll",7,35,w-14,h-42,out var content);float y=7;ItemInfo(content,item,w-20,ref y);content.sizeDelta=new Vector2(0,y+6);
        }
    }
}
