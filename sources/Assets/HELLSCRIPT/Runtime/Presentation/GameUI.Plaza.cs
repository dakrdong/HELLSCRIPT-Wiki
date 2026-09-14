using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    public sealed partial class GameUI
    {
        Text plazaStatus,plazaActionText,plazaServiceName,plazaServiceDetail;
        RectTransform plazaAction,plazaJoystickRect,plazaGuide;
        TownJoystick plazaJoystick;
        readonly Dictionary<TownStation,RectTransform> plazaBubbles=new Dictionary<TownStation,RectTransform>();
        readonly Dictionary<TownStation,Text> plazaBubbleLabels=new Dictionary<TownStation,Text>();
        public Vector2 TownMovement=>Page=="plaza"&&!CommonPanelOpen&&plazaJoystick!=null?plazaJoystick.Value:Vector2.zero;
        public void ResetTownInput()=>plazaJoystick?.ResetInput();
        public bool TownNavigationOpen=>Page=="plaza"&&plazaGuide!=null&&plazaGuide.gameObject.activeSelf;
        public void ShowTitle()
        {
            game.Town?.Cancel();game.World.ClearDungeon();pageRepaint=ShowTitle;Base("title","HELLSCRIPT","잿빛 숲 너머, 당신의 이야기가 시작됩니다",art:true,responsive:true);
            Note(content,"캐릭터 선택하기",28,56,gold);
            for(int i=0;i<game.Store.Data.heroes.Count;i++)
            {
                int index=i;var h=game.Store.Data.heroes[i];bool selected=i==game.Store.Data.selectedHero;
                var b=BigButton(content,Loc.F("{0} · Lv.{1}\n{2}",game.catalog.classNames[(int)h.heroClass],h.level,selected?"선택한 캐릭터":"이 캐릭터 선택"),()=>game.SelectHero(index),selected);
                b.name="title-character-"+i;b.interactable=game.Store.Data.suspendedRun==null;
            }
            Note(content,"입장하면 마을에 도착합니다. 원형 조이스틱으로 움직여 주민들을 만나 보세요.",21,110,pale);
            if(game.Store.Data.suspendedRun!=null)Note(content,"진행 중인 균열의 캐릭터로 입장합니다. 마을 포탈에서 이어갈 수 있습니다.",20,90,gold);
            FooterButton(0,1,"입장하기",()=>game.EnterPlaza(true),true);footer.GetComponentInChildren<Button>().name="title-enter";
        }
        public void ShowPlaza()
        {
            pageRepaint=ShowPlaza;Base("plaza","잿빛 숲 · 정착민 마을","조이스틱으로 이동 · 주민에게 가까이 가서 대화하세요",battle:true);
            footer.gameObject.SetActive(false);footerApron.gameObject.SetActive(false);plazaBubbles.Clear();plazaBubbleLabels.Clear();
            plazaStatus=Label(root,"",17,pale);Span(plazaStatus.rectTransform,22,90,20,40);
            var guide=Button(root,"시설 안내",ToggleTownGuide);Place((RectTransform)guide.transform,20,140,126,52);
            var menu=Button(root,"메뉴",()=>{game.Town.Cancel();ShowTownMenu();});Place((RectTransform)menu.transform,156,140,98,52);
            plazaGuide=Box("Town destinations",root,ink);Place(plazaGuide,20,204,300,390);plazaGuide.gameObject.SetActive(false);
            for(int i=0;i<TownLayout.Stations.Length;i++)
            {
                var s=TownLayout.Stations[i];var b=Button(plazaGuide,s.name,()=>{plazaGuide.gameObject.SetActive(false);game.RequestStation(s.id);});Place((RectTransform)b.transform,8,8+i*53,284,46);b.name="town-route-"+s.id;
            }
            plazaJoystickRect=Rect("Town joystick",root);plazaJoystickRect.anchorMin=plazaJoystickRect.anchorMax=Vector2.zero;plazaJoystickRect.pivot=new Vector2(0,0);plazaJoystickRect.anchoredPosition=new Vector2(28,38);plazaJoystickRect.sizeDelta=new Vector2(168,168);
            var pad=plazaJoystickRect.gameObject.AddComponent<TownCircleGraphic>();pad.color=new Color(.035f,.047f,.04f,.64f);
            var rim=Rect("Joystick rim",plazaJoystickRect);Stretch(rim);var rimGraphic=rim.gameObject.AddComponent<TownCircleGraphic>();rimGraphic.InnerRatio=.96f;rimGraphic.color=new Color(.67f,.6f,.43f,.8f);rimGraphic.raycastTarget=false;
            var knob=Rect("Joystick knob",plazaJoystickRect);knob.anchorMin=knob.anchorMax=knob.pivot=new Vector2(.5f,.5f);knob.sizeDelta=new Vector2(64,64);
            var knobGraphic=knob.gameObject.AddComponent<TownCircleGraphic>();knobGraphic.color=new Color(.57f,.52f,.4f,.9f);knobGraphic.raycastTarget=false;
            plazaJoystick=plazaJoystickRect.gameObject.AddComponent<TownJoystick>();plazaJoystick.Knob=knob;
            foreach(var s in TownLayout.Stations)
            {
                var bubble=Box("NPC bubble "+s.id,root,new Color(.035f,.044f,.035f,.9f));bubble.GetComponent<Image>().raycastTarget=false;
                bubble.anchorMin=bubble.anchorMax=Vector2.zero;bubble.pivot=new Vector2(.5f,0);bubble.sizeDelta=new Vector2(156,32);
                var label=Label(bubble,s.name,15,gold,TextAnchor.MiddleCenter);Inset(label.rectTransform,4,4,2,2);
                plazaBubbleLabels[s.id]=label;
                var tail=Box("Speech tail",bubble,new Color(.035f,.044f,.035f,.9f));tail.GetComponent<Image>().raycastTarget=false;tail.anchorMin=tail.anchorMax=new Vector2(.5f,0);tail.sizeDelta=new Vector2(10,10);tail.anchoredPosition=new Vector2(0,-4);tail.localRotation=Quaternion.Euler(0,0,45);
                plazaBubbles[s.id]=bubble;
            }
            plazaAction=Box("Town interaction card",root,new Color(.045f,.053f,.042f,.97f));plazaAction.anchorMin=plazaAction.anchorMax=new Vector2(1,.35f);plazaAction.pivot=new Vector2(1,.5f);plazaAction.anchoredPosition=new Vector2(-22,0);plazaAction.sizeDelta=new Vector2(250,164);
            var edge=Box("Interaction gold edge",plazaAction,gold);Place(edge,0,0,3,164);
            plazaServiceName=Label(plazaAction,"",22,gold);Place(plazaServiceName.rectTransform,15,10,222,28);
            plazaServiceDetail=Label(plazaAction,"",15,muted);Place(plazaServiceDetail.rectTransform,15,42,222,42);
            var interact=Button(plazaAction,"대화하기",game.InteractTown,new Color(.4f,.27f,.12f));interact.name="town-interact";Place((RectTransform)interact.transform,12,94,226,58);plazaActionText=interact.GetComponentInChildren<Text>();
            overlay.SetAsLastSibling();RefreshPlaza();
        }
        void ToggleTownGuide(){plazaGuide.gameObject.SetActive(!plazaGuide.gameObject.activeSelf);ResetTownInput();game.Town.Cancel();}
        public void RefreshPlaza()
        {
            if(Page!="plaza"||plazaStatus==null||game.Town==null)return;
            var walk=game.Town;var near=walk.Nearby;
            plazaStatus.text=walk.Destination.HasValue?Loc.F("{0}으로 이동 중 · {1:0.0}m",TownLayout.Station(walk.Destination.Value).name,walk.Remaining):Loc.F("{0} Lv.{1} · 중앙 길 횡단 약 20초",game.catalog.classNames[(int)game.Store.Data.Hero.heroClass],game.Store.Data.Hero.level);
            plazaAction.gameObject.SetActive(near.HasValue&&!TownNavigationOpen&&!CommonPanelOpen);
            if(near.HasValue)
            {
                var s=TownLayout.Station(near.Value);plazaServiceName.text=Loc.T(s.name);plazaServiceDetail.text=Loc.T(s.service);plazaActionText.text=Loc.T(s.action);
            }
            // Right edge + normalized safe-area height stays identical in portrait and landscape.
            foreach(var pair in plazaBubbles)
            {
                var screen=game.World.TownStationScreen(pair.Key);bool visible=screen.z>0&&screen.x>12&&screen.x<Screen.width-12&&screen.y>230&&screen.y<Screen.height-175;
                pair.Value.gameObject.SetActive(visible&&!TownNavigationOpen);
                bool close=near==pair.Key;var station=TownLayout.Station(pair.Key);
                pair.Value.sizeDelta=new Vector2(156,close?60:32);
                plazaBubbleLabels[pair.Key].text=Loc.T(station.name)+(close?"\n"+Loc.T(station.action):"");
                if(visible&&RectTransformUtility.ScreenPointToLocalPointInRectangle(root,new Vector2(screen.x,screen.y),null,out var local))pair.Value.anchoredPosition=local-root.rect.min;
            }
            if(CommonPanelOpen)ResetTownInput();
        }
        public void OpenStation(TownStation station)
        {
            ResetTownInput();game.Town?.Cancel();
            switch(station)
            {
                case TownStation.Blacksmith:case TownStation.Reroller:ShowTownSmith();break;
                case TownStation.Merchant:ShowTownMerchant();break;
                case TownStation.RiftKeeper:ShowRiftKeeper();break;
                case TownStation.Warehouse:OpenInventory(true,false);break;
                case TownStation.Training:ShowTraining();break;
                case TownStation.GemMerchant:ShowTownGemShop();break;
                case TownStation.RuneMerchant:ShowTownRuneShop();break;
            }
        }
    }
}
