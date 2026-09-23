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
            ShowEquipmentShop(EquipmentShopTab.Buy);
        }
        public void ShowTownSmith()=>ShowBlacksmith();
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
