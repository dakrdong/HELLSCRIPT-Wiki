using System;
using UnityEngine;

namespace Hellscript
{
    public sealed partial class GameUI
    {
        bool TownService(string page,string title,string subtitle,Action repaint)
        {
            if(game.Active)return false;
            pageRepaint=repaint;Base(page,title,subtitle);
            Note(content,Loc.F("보유 골드 {0:N0}",game.Store.Data.gold),23,60,gold);
            if(game.Store.Data.suspendedRun!=null)Note(content,"진행 중인 균열을 먼저 완료하면 거래할 수 있습니다.",21,88,gold);
            return true;
        }
        void TownPurchase(string name,string prompt,int cost,Func<string,bool> purchase,Action repaint,bool room=true)
        {
            string request=Guid.NewGuid().ToString("N");
            var b=BigButton(content,prompt,()=>Confirm(prompt,()=>
            {
                bool success=purchase(request);repaint();ShowToast(success?"구매한 물품을 보관했습니다.":game.Store.Error);
            }),true);
            b.name=name;b.interactable=game.Store.Data.suspendedRun==null&&game.Store.Data.gold>=cost&&room;
        }
        public void ShowTownMerchant()
        {
            if(!TownService("town-merchant","장비 상인","장비 구매 · 판매",ShowTownMerchant))return;
            Note(content,"기본 장비를 골드로 구매할 수 있습니다. 장비를 판매하려면 판매할 장비를 선택하세요.",21,100,pale);
            Cycle(content,"선택 부위",GameCatalog.Slots,selectedSlot,i=>{selectedSlot=i;ShowTownMerchant();});
            int slot=selectedSlot,cost=TownTrade.EquipmentPrice(game.Store.Data.Hero);
            TownPurchase("town-buy-equipment",Loc.F("일반 {0} 구매 · {1:N0} 골드",GameCatalog.Slots[slot],cost),cost,r=>game.Store.BuyTownEquipment(r,slot),ShowTownMerchant,Economy.FreeSlots(game.Store.Data.Hero)>0);
            BigButton(content,"장비 판매 · 가방 열기",()=>ShowBag());
            BigButton(content,"미확인 장비 · 제작 상점",ShowShop);
            FooterButton(0,1,"마을로 돌아가기",ShowTown);
        }
        public void ShowTownSmith()
        {
            if(!TownService("town-smith","대장간","장비 재련 · 강화 · 제작",ShowTownSmith))return;
            Note(content,"작업할 장비를 선택한 뒤 강화하거나 옵션을 재설정하세요. 각 작업의 해금 조건과 비용은 장비 상세에 표시됩니다.",21,130,pale);
            BigButton(content,"장비 재련 · 강화할 장비 선택",()=>ShowBag(),true);
            BigButton(content,"장비 제작",ShowShop);
            FooterButton(0,1,"마을로 돌아가기",ShowTown);
        }
        public void ShowTownGemShop()
        {
            if(!TownService("town-gems","보석 상인","보석 구매 · 소켓 관리",ShowTownGemShop))return;
            Note(content,"1단계 보석을 판매합니다. 구매한 보석은 계정 보관함에 들어가며, 같은 보석을 모아 합성할 수 있습니다.",21,110,pale);
            foreach(var gem in GemCatalog.Gems)
            {
                string id=gem.id;TownPurchase("town-buy-gem-"+id,Loc.F("{0} 구매 · {1:N0} 골드",GemInventory.Name(id,1),TownTrade.GemPrice),TownTrade.GemPrice,r=>game.Store.BuyTownGem(r,id),ShowTownGemShop,GemInventory.CanAdd(game.Store.Data,new GemStack{gemId=id,tier=1,count=1}));
            }
            BigButton(content,"보석 보관함 · 소켓 관리",ShowGemMenu);
            FooterButton(0,1,"마을로 돌아가기",ShowTown);
        }
        public void ShowTownRuneShop()
        {
            if(!TownService("town-runes","룬 상인","룬 블록 구매 · 룬 배치",ShowTownRuneShop))return;
            Note(content,"G0 등급의 기본 룬 블록을 판매합니다. 구입한 블록은 룬 보유 목록에서 바로 배치할 수 있습니다.",21,110,pale);
            for(int n=1;n<=3;n++)
            {
                int size=n,cost=TownTrade.RunePrice(size);
                TownPurchase("town-buy-rune-"+size,Loc.F("G0 · {0}칸 룬 블록 구매 · {1:N0} 골드",size,cost),cost,r=>game.Store.BuyTownRune(r,size),ShowTownRuneShop);
            }
            BigButton(content,"룬 보드 · 배치와 합성",ShowRunes);
            FooterButton(0,1,"마을로 돌아가기",ShowTown);
        }
    }
}
