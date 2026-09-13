using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    public sealed partial class GameUI
    {
        Text plazaStatus;

        // A walkable view of the sanctuary. Every station opens a service the menu already offers, at
        // the same cost and with the same rules; the walk is presentation, never a gate.
        public void ShowPlaza()
        {
            pageRepaint=()=>ShowPlaza();Base("plaza","잿빛 성소 · 광장","목적지를 누르면 영웅이 걸어가고, 도착하면 그곳의 서비스가 열립니다",battle:true);
            plazaStatus=Label(root,"",21,pale);Place(plazaStatus.rectTransform,26,118,900,40);plazaStatus.transform.SetSiblingIndex(root.childCount-2);
            var stations=TownLayout.Stations;
            for(int i=0;i<stations.Length;i++){var station=stations[i];FooterButton(i,stations.Length+1,station.name,()=>game.RequestStation(station.id));}
            FooterButton(stations.Length,stations.Length+1,"메뉴",()=>{game.Town?.Cancel();ShowTown();});
            RefreshPlaza();
        }

        public void RefreshPlaza()
        {
            if(Page!="plaza"||plazaStatus==null||game.Town==null)return;
            var walk=game.Town;
            if(walk.Walking){var station=TownLayout.Station(walk.Destination.Value);plazaStatus.text=Loc.F("{0}으로 이동 중 · {1:0.0}m · {2}", station.name, walk.Remaining, station.service);}
            else plazaStatus.text=Loc.T("하단의 목적지를 누르세요. 메뉴 바로가기와 같은 서비스, 같은 비용입니다.");
        }

        // Arrival opens the same screen the menu button would; only the note differs.
        public void OpenStation(TownStation station)
        {
            switch(station)
            {
                case TownStation.Blacksmith:ShowBag();ShowToast(Loc.T("대장장이 · 강화·일반 제작·분해는 장비 화면에서 진행합니다.")+"\n"+Loc.T("걸작은 장비 상세의 ‘걸작 · 진행과 초기화’에서 이용할 수 있습니다."));break;
                case TownStation.Reroller:ShowBag();ShowToast("접사 재설정은 장비 상세의 ‘한 줄 재설정’에서, 보석은 가방 목록의 ‘보석 보관함’에서 이용하세요.");break;
                case TownStation.Merchant:ShowShop();break;
                case TownStation.RiftKeeper:ShowRiftKeeper();break;
                case TownStation.Warehouse:OpenInventory(true,false);break;
            }
        }
    }
}
