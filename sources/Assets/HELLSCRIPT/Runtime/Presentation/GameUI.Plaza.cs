using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    public sealed partial class GameUI
    {
        Text plazaStatus,plazaActionText,plazaServiceName,plazaServiceDetail,plazaNpcName;
        RectTransform plazaAction,plazaJoystickRect,plazaGuide,plazaTitlePlate,plazaPortalBubble;
        Button plazaSettings;
        TownJoystick plazaJoystick;
        Button plazaInteract,plazaBuy,plazaSell,plazaTalk;
        readonly Dictionary<TownStation,RectTransform> plazaBubbles=new Dictionary<TownStation,RectTransform>();
        readonly Dictionary<string,RectTransform> plazaResidentBubbles=new Dictionary<string,RectTransform>();
        public Vector2 TownMovement=>Page=="plaza"&&!CommonPanelOpen&&plazaJoystick!=null?plazaJoystick.Value:Vector2.zero;
        public void ResetTownInput(){if(plazaJoystick!=null)plazaJoystick.ResetInput();}
        public bool TownNavigationOpen=>Page=="plaza"&&plazaGuide!=null&&plazaGuide.gameObject.activeSelf;
        public void ShowPlaza()
        {
            pageRepaint=ShowPlaza;Base("plaza","잿빛 숲 · 정착민 마을","조이스틱으로 이동 · 주민에게 가까이 가서 대화하세요",battle:true);
            CompactPlazaHeader();AddContentDock(48,52);AddEventButton();
            footer.gameObject.SetActive(false);footerApron.gameObject.SetActive(false);plazaBubbles.Clear();plazaResidentBubbles.Clear();
            plazaStatus=Label(root,"",17,pale);Span(plazaStatus.rectTransform,22,90,20,40);
            var guide=Button(root,"시설 안내",ToggleTownGuide);Place((RectTransform)guide.transform,20,140,126,52);
            var menu=Button(root,"메뉴",()=>{game.Town.Cancel();ShowTownMenu();});Place((RectTransform)menu.transform,156,140,98,52);
            // Keep the existing route controller available while removing its on-screen chrome.
            plazaStatus.gameObject.SetActive(false);guide.gameObject.SetActive(false);menu.gameObject.SetActive(false);
            plazaGuide=Box("Town destinations",root,ink);Place(plazaGuide,20,204,300,16+TownLayout.Stations.Length*53);plazaGuide.gameObject.SetActive(false);
            for(int i=0;i<TownLayout.Stations.Length;i++)
            {
                var s=TownLayout.Stations[i];var b=Button(plazaGuide,s.name,()=>{plazaGuide.gameObject.SetActive(false);game.RequestStation(s.id);});Place((RectTransform)b.transform,8,8+i*53,284,46);b.name="town-route-"+s.id;
            }
            plazaJoystickRect=Rect("Town joystick",root);plazaJoystickRect.anchorMin=plazaJoystickRect.anchorMax=Vector2.zero;plazaJoystickRect.pivot=new Vector2(0,0);plazaJoystickRect.anchoredPosition=new Vector2(28,38);plazaJoystickRect.sizeDelta=new Vector2(168,168);
            var pad=plazaJoystickRect.gameObject.AddComponent<TownCircleGraphic>();pad.color=UiTheme.Tint(UiTheme.InsetHex);
            var rim=Rect("Joystick rim",plazaJoystickRect);Stretch(rim);var rimGraphic=rim.gameObject.AddComponent<TownCircleGraphic>();rimGraphic.InnerRatio=.96f;rimGraphic.color=new Color(.67f,.6f,.43f,1);rimGraphic.raycastTarget=false;
            var knob=Rect("Joystick knob",plazaJoystickRect);knob.anchorMin=knob.anchorMax=knob.pivot=new Vector2(.5f,.5f);knob.sizeDelta=new Vector2(64,64);
            var knobGraphic=knob.gameObject.AddComponent<TownCircleGraphic>();knobGraphic.color=new Color(.57f,.52f,.4f,1);knobGraphic.raycastTarget=false;
            plazaJoystick=plazaJoystickRect.gameObject.AddComponent<TownJoystick>();plazaJoystick.Knob=knob;
            foreach(var s in TownLayout.Stations)
                plazaBubbles[s.id]=CreateTownBubble("NPC bubble "+s.id,s.name,s.npcName);
            foreach(var resident in TownLayout.Residents)
                plazaResidentBubbles[resident.id]=CreateTownBubble("Resident bubble "+resident.id,"",resident.name);
            plazaPortalBubble=CreateTownBubble("Return portal label","복귀 포탈","");
            plazaAction=Box("Town interaction card",root,UiTheme.Tint(UiTheme.InsetHex,.97f));plazaAction.anchorMin=plazaAction.anchorMax=new Vector2(1,.35f);plazaAction.pivot=new Vector2(1,.5f);plazaAction.anchoredPosition=new Vector2(-22,0);plazaAction.sizeDelta=new Vector2(250,232);
            var edge=Box("Interaction gold edge",plazaAction,gold);Place(edge,0,0,3,232);
            plazaServiceName=Label(plazaAction,"",22,gold);Place(plazaServiceName.rectTransform,15,10,222,28);
            plazaNpcName=Label(plazaAction,"",14,pale);plazaNpcName.name="NPC name";Place(plazaNpcName.rectTransform,15,39,222,20);
            plazaServiceDetail=Label(plazaAction,"",15,muted);Place(plazaServiceDetail.rectTransform,15,64,222,42);
            plazaTalk=Button(plazaAction,"대화하기",game.TalkTown);plazaTalk.name="town-talk";Place((RectTransform)plazaTalk.transform,12,112,226,44);
            var interact=Button(plazaAction,"대화하기",game.InteractTown,new Color(.4f,.27f,.12f));interact.name="town-interact";Place((RectTransform)interact.transform,12,168,226,52);plazaActionText=interact.GetComponentInChildren<Text>();
            plazaInteract=interact;
            plazaBuy=Button(plazaAction,"구매",()=>game.InteractEquipmentMerchant(EquipmentShopTab.Buy),new Color(.4f,.27f,.12f));plazaBuy.name="town-merchant-buy";Place((RectTransform)plazaBuy.transform,12,168,110,52);
            plazaSell=Button(plazaAction,"판매",()=>game.InteractEquipmentMerchant(EquipmentShopTab.Sell));plazaSell.name="town-merchant-sell";Place((RectTransform)plazaSell.transform,128,168,110,52);
            overlay.SetAsLastSibling();RefreshPlaza();
        }
        RectTransform CreateTownBubble(string id,string contentName,string npcName)
        {
            bool hasContent=!string.IsNullOrEmpty(contentName),hasName=!string.IsNullOrEmpty(npcName);
            var bubble=Rect(id,root);bubble.anchorMin=bubble.anchorMax=Vector2.zero;bubble.pivot=new Vector2(.5f,0);
            bubble.sizeDelta=new Vector2(208,hasContent&&hasName?54:32);
            if(hasContent)
            {
                var title=Label(bubble,contentName,22,gold,TextAnchor.MiddleCenter);title.name="Content name";title.fontStyle=FontStyle.Bold;
                Place(title.rectTransform,0,0,208,32);OutlinePlazaText(title);
            }
            if(hasName)
            {
                var name=Label(bubble,npcName,hasContent?14:16,pale,TextAnchor.MiddleCenter);name.name="NPC name";
                Place(name.rectTransform,0,hasContent?32:0,208,hasContent?22:32);OutlinePlazaText(name);
            }
            return bubble;
        }
        void PositionTownBubble(RectTransform bubble,Vector3 screen)
        {
            var safe=UiSafeArea.Current;float canvasScale=root.parent.GetComponent<Canvas>().scaleFactor;
            float scale=globalHud?.Layout?.scale??canvasScale;
            if(bubble==plazaPortalBubble)scale=Mathf.Max(scale,14f/22);
            // World labels follow the HUD's screen ratio, even though the page canvas uses pixels.
            // Scale the entire bubble so text, line spacing, outlines and bounds stay in proportion.
            bubble.localScale=Vector3.one*(scale/Mathf.Max(.001f,canvasScale));
            bool visible=screen.z>0&&screen.x>safe.xMin+8*scale&&screen.x<safe.xMax-8*scale
                &&screen.y>safe.yMin+32*scale&&screen.y+bubble.rect.height*scale<safe.yMax-header.rect.height*canvasScale;
            bubble.gameObject.SetActive(visible&&!TownNavigationOpen);
            if(!visible||!RectTransformUtility.ScreenPointToLocalPointInRectangle(root,new Vector2(screen.x,screen.y),null,out var local))return;
            var position=local-root.rect.min;float half=bubble.rect.width*bubble.localScale.x/2,padding=8*bubble.localScale.x;
            position.x=Mathf.Clamp(position.x,half+padding,Mathf.Max(half+padding,root.rect.width-half-padding));bubble.anchoredPosition=position;
        }
        void CompactPlazaHeader()
        {
            header.sizeDelta=new Vector2(0,44);header.GetComponent<Image>().enabled=false;
            headerApron.gameObject.SetActive(false);headerSubtitle.gameObject.SetActive(false);
            header.Find("Gold divider").gameObject.SetActive(false);
            headerTitle.fontSize=18;Span(headerTitle.rectTransform,14,4,64,32);OutlinePlazaText(headerTitle);
            plazaTitlePlate=Rect("Town title backing",header);plazaTitlePlate.SetAsFirstSibling();
            var plate=plazaTitlePlate.gameObject.AddComponent<StorageSurface>();plate.Paint(UiTheme.BackgroundHex+"d4",UiTheme.BackgroundHex+"a8");plate.raycastTarget=false;
            var settings=header.GetComponentInChildren<Button>();plazaSettings=settings;Right((RectTransform)settings.transform,4,0,44,44);
            settings.GetComponent<Image>().color=Color.clear;
            settings.GetComponent<UIRectBorder>().enabled=false;
            var gear=(RectTransform)settings.transform.Find("Settings gear");gear.offsetMin=Vector2.one*12;gear.offsetMax=-Vector2.one*12;
        }
        static void OutlinePlazaText(Text label)
        {
            var outline=label.gameObject.AddComponent<Outline>();outline.effectColor=UiTheme.Tint(UiTheme.VoidHex);
            outline.effectDistance=new Vector2(1,-1);outline.useGraphicAlpha=true;
        }
        void ToggleTownGuide(){plazaGuide.gameObject.SetActive(!plazaGuide.gameObject.activeSelf);ResetTownInput();game.Town.Cancel();}
        public void RefreshPlaza()
        {
            if(Page!="plaza"||plazaStatus==null||game.Town==null)return;
            var walk=game.Town;var near=walk.Nearby;var npc=NpcProfiles.Nearby(walk.Position);
            bool portal=game.NearTownPortal;
            float cardHeight=portal||npc!=null&&!npc.Station.HasValue?168:232;
            plazaAction.sizeDelta=new Vector2(250,cardHeight);
            ((RectTransform)plazaAction.Find("Interaction gold edge")).sizeDelta=new Vector2(3,cardHeight);
            if(globalHud?.Layout!=null&&plazaJoystickRect!=null)
            {
                var layout=globalHud.Layout;float scale=root.parent.GetComponent<Canvas>().scaleFactor;
                var safe=UiSafeArea.Current;
                plazaJoystick.Reflow(TownJoystick.Bounds(safe.width,safe.height,layout),scale);
                float cardScale=layout.scale/Mathf.Max(.001f,scale);
                float gap=UiTheme.Gap*cardScale,edge=12*cardScale;
                float iconSize=DockIconSize((safe.height-layout.potionBounds.yMax*layout.scale)/scale,true);
                Right((RectTransform)plazaSettings.transform,edge,edge,iconSize,iconSize);CompactGear(plazaSettings,iconSize);
                Right((RectTransform)contentDock.transform,edge,edge+iconSize+gap,iconSize,iconSize);contentDock.Reflow(iconSize,gap);
                ReflowEventButton();
                headerTitle.fontSize=18;float fullTitleWidth=headerTitle.preferredWidth;
                float titleWidth=Mathf.Min(fullTitleWidth+24,root.rect.width-iconSize-3*edge);
                headerTitle.fontSize=Mathf.Clamp(Mathf.FloorToInt(18*(titleWidth-24)/Mathf.Max(1,fullTitleWidth)),10,18);
                Place(plazaTitlePlate,8,4,titleWidth,32);Place(headerTitle.rectTransform,20,4,titleWidth-24,32);
                // Keep the return action readable and at least 44 screen pixels tall in narrow views.
                if(portal)cardScale=Mathf.Max(cardScale,1/Mathf.Max(.001f,scale));
                plazaAction.localScale=Vector3.one*cardScale;
                float hudTop=layout.potions.Concat(layout.actives).Max(r=>r.yMax)*layout.scale/scale;
                float halfHeight=plazaAction.rect.height*cardScale/2;
                float center=Mathf.Min(root.rect.height-44-halfHeight,Mathf.Max(root.rect.height*.35f,hudTop+14*cardScale+halfHeight));
                float cardRight=Mathf.Max(22*cardScale,edge+iconSize+gap);
                plazaAction.anchoredPosition=new Vector2(-cardRight,center-root.rect.height*.35f);
            }
            plazaStatus.text=walk.Destination.HasValue?Loc.F("{0}으로 이동 중 · {1:0.0}m",TownLayout.Station(walk.Destination.Value).name,walk.Remaining):Loc.F("{0} Lv.{1} · 중앙 길 횡단 약 20초",game.catalog.classNames[(int)game.Store.Data.Hero.heroClass],game.Store.Data.Hero.level);
            plazaAction.gameObject.SetActive((portal||near.HasValue||npc!=null)&&!TownNavigationOpen&&!CommonPanelOpen);
            plazaTalk.gameObject.SetActive(!portal&&npc!=null);
            bool merchant=!portal&&(near==TownStation.Merchant||near==TownStation.Gambler);
            plazaInteract.gameObject.SetActive(portal||near.HasValue&&!merchant);
            plazaBuy.gameObject.SetActive(merchant);plazaSell.gameObject.SetActive(merchant);
            plazaInteract.name=portal?"town-portal-resume":"town-interact";
            Place((RectTransform)plazaInteract.transform,12,portal?112:168,226,portal?44:52);
            if(portal)
            {
                plazaServiceName.text=Loc.T("복귀 포탈");plazaNpcName.text="";
                plazaServiceDetail.text=Loc.F("균열 {0:00}단계 · 이전 전투를 이어갑니다.",game.Store.Data.suspendedRun.stage);
                plazaActionText.text=Loc.T("균열로 복귀");
            }
            else if(near.HasValue)
            {
                var s=TownLayout.Station(near.Value);plazaServiceName.text=Loc.T(s.name);plazaNpcName.text=Loc.T(s.npcName);plazaServiceDetail.text=Loc.T(s.service);plazaActionText.text=Loc.T(s.action);
            }
            if(!portal&&npc!=null&&!npc.Station.HasValue)
            {plazaServiceName.text=Loc.T(npc.Role);plazaNpcName.text=Loc.T(npc.name);plazaServiceDetail.text="";}
            // Preserve the right inset while lifting interactions above actual HUD controls.
            foreach(var pair in plazaBubbles)
                PositionTownBubble(pair.Value,game.World.TownStationScreen(pair.Key));
            foreach(var resident in TownLayout.Residents)
                PositionTownBubble(plazaResidentBubbles[resident.id],game.World.TownNpcScreen(resident.position));
            if(game.TownPortalAvailable)PositionTownBubble(plazaPortalBubble,game.World.TownReturnPortalScreen());
            else plazaPortalBubble.gameObject.SetActive(false);
            if(CommonPanelOpen)ResetTownInput();
        }
        public void OpenStation(TownStation station)
        {
            if(game.Town?.Nearby==station){if(station==TownStation.RiftKeeper)Tutorials.Performed(game.Store.Data,"F01");
                string hint=station==TownStation.Warehouse?"H04":station==TownStation.Merchant||station==TownStation.Gambler?"H02":null;if(hint!=null)Tutorials.Discover(game.Store.Data,hint);}
            ResetTownInput();game.Town?.Cancel();
            switch(station)
            {
                case TownStation.Blacksmith:case TownStation.Reroller:ShowTownSmith();break;
                case TownStation.Merchant:ShowTownMerchant();break;
                case TownStation.Gambler:ShowEquipmentShop(EquipmentShopTab.Buy,true);break;
                case TownStation.RiftKeeper:ShowRiftKeeper();break;
                case TownStation.Warehouse:ShowStorage();break;
                case TownStation.Training:ShowTraining();break;
                case TownStation.GemMerchant:ShowTownGemShop();break;
                case TownStation.RuneMerchant:ShowTownRuneShop();break;
                case TownStation.RuneMaster:ShowRuneMaster();break;
                case TownStation.AspectStone:ShowAspectStone();break;
            }
        }
    }
}
