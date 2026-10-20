using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    public sealed partial class GameUI
    {
        static readonly UiSkin PlazaSkin=UiSkin.Casual.WithInteractionStates();
        Text plazaStatus,plazaActionText,plazaServiceName,plazaServiceDetail,plazaNpcName;
        RectTransform plazaAction,plazaJoystickRect,plazaGuide,plazaTitlePlate,plazaPortalBubble;
        CasualNoticeEntrance plazaEntrance;
        Button plazaSettings;
        StorageSurface plazaTitleSurface,plazaGuideSurface;
        TownCircleGraphic plazaJoystickRim;
        RectTransform plazaKnobFace,plazaKnobDrop;
        float plazaTitleUnit;
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
            plazaGuideSurface=PlazaSurface(plazaGuide,16,3,4);
            for(int i=0;i<TownLayout.Stations.Length;i++)
            {
                var s=TownLayout.Stations[i];var b=Button(plazaGuide,s.name,()=>{plazaGuide.gameObject.SetActive(false);game.RequestStation(s.id);});Place((RectTransform)b.transform,8,8+i*53,284,46);b.name="town-route-"+s.id;PlazaButton(b);b.GetComponentInChildren<Text>().fontSize=18;
            }
            plazaJoystickRect=Rect("Town joystick",root);plazaJoystickRect.anchorMin=plazaJoystickRect.anchorMax=Vector2.zero;plazaJoystickRect.pivot=new Vector2(0,0);plazaJoystickRect.anchoredPosition=new Vector2(28,38);plazaJoystickRect.sizeDelta=new Vector2(168,168);
            var skin=UiSkin.Casual;
            var pad=plazaJoystickRect.gameObject.AddComponent<TownCircleGraphic>();pad.color=skin.CreamShade*new Color(1,1,1,.85f);
            var rim=Rect("Joystick rim",plazaJoystickRect);Stretch(rim);plazaJoystickRim=rim.gameObject.AddComponent<TownCircleGraphic>();plazaJoystickRim.InnerRatio=.96f;plazaJoystickRim.color=skin.Navy;plazaJoystickRim.raycastTarget=false;
            var stitch=Rect("Joystick stitch",plazaJoystickRect);Stretch(stitch);var joystickTrim=stitch.gameObject.AddComponent<TownPlazaDecoration>();joystickTrim.joystick=true;joystickTrim.raycastTarget=false;
            var knob=Rect("Joystick knob",plazaJoystickRect);knob.anchorMin=knob.anchorMax=knob.pivot=new Vector2(.5f,.5f);knob.sizeDelta=new Vector2(64,64);
            plazaKnobDrop=PlazaCircle("Joystick knob drop",knob,Vector2.zero,Vector2.one,skin.Navy);
            PlazaCircle("Joystick knob ring",knob,Vector2.zero,Vector2.one,skin.Navy);
            plazaKnobFace=PlazaCircle("Joystick knob face",knob,Vector2.zero,Vector2.one,skin.Gold);
            PlazaCircle("Joystick knob shine",knob,new Vector2(.22f,.52f),new Vector2(.52f,.82f),skin.White*new Color(1,1,1,.75f));
            plazaJoystick=plazaJoystickRect.gameObject.AddComponent<TownJoystick>();plazaJoystick.Knob=knob;
            foreach(var s in TownLayout.Stations)
                plazaBubbles[s.id]=CreateTownBubble("NPC bubble "+s.id,s.name,s.npcName,true);
            foreach(var resident in TownLayout.Residents)
                plazaResidentBubbles[resident.id]=CreateTownBubble("Resident bubble "+resident.id,"",resident.name,true);
            plazaPortalBubble=CreateTownBubble("Return portal label","복귀 포탈","",true);
            plazaAction=Box("Town interaction card",root,UiTheme.Tint(UiTheme.InsetHex,.97f));plazaAction.anchorMin=plazaAction.anchorMax=new Vector2(1,.35f);plazaAction.pivot=new Vector2(1,.5f);plazaAction.anchoredPosition=new Vector2(-22,0);plazaAction.sizeDelta=new Vector2(250,232);
            var edge=Box("Interaction gold edge",plazaAction,gold);Place(edge,0,0,3,232);edge.gameObject.SetActive(false);
            plazaAction.GetComponent<Image>().color=Color.clear;
            plazaEntrance=plazaAction.gameObject.AddComponent<CasualNoticeEntrance>();
            var shadow=Rect("Town card soft shadow",plazaAction);Stretch(shadow);var softShadow=shadow.gameObject.AddComponent<TownPlazaDecoration>();softShadow.shadowOnly=true;softShadow.raycastTarget=false;
            var frame=Rect("Casual interaction surface",plazaAction);Stretch(frame);var cardFrame=frame.gameObject.AddComponent<UiWindowFrame>();cardFrame.SetSkin(skin,borderWidth:3,shadow:4);cardFrame.raycastTarget=false;
            var trim=Rect("Town card stitch and gem",plazaAction);Stretch(trim);trim.gameObject.AddComponent<TownPlazaDecoration>().raycastTarget=false;
            plazaServiceSource=null;plazaServiceName=Label(plazaAction,"",22,skin.Navy);plazaServiceName.name="Service name";Place(plazaServiceName.rectTransform,15,10,222,28);skin.ApplyLabel(plazaServiceName);plazaServiceName.horizontalOverflow=HorizontalWrapMode.Overflow;
            plazaNpcName=Label(plazaAction,"",14,skin.PlazaSecondaryInk);plazaNpcName.name="NPC name";Place(plazaNpcName.rectTransform,15,39,222,20);plazaNpcName.font=UiFonts.CasualWeight(700);
            plazaServiceDetail=Label(plazaAction,"",15,skin.PlazaDetailInk);plazaServiceDetail.name="Service detail";Place(plazaServiceDetail.rectTransform,15,64,222,42);plazaServiceDetail.font=skin.BodyFont;plazaServiceDetail.alignment=TextAnchor.UpperLeft;
            // The shared font measurer uses two local units per point: reserve exactly 21 here.
            SkillTreeD4Wrap.SetLineHeight(plazaServiceDetail,10.5);
            plazaTalk=Button(plazaAction,"대화하기",game.TalkTown);plazaTalk.name="town-talk";Place((RectTransform)plazaTalk.transform,12,112,226,44);PlazaButton(plazaTalk);
            var talkIcon=Rect("Talk icon",plazaTalk.transform);var talkGlyph=talkIcon.gameObject.AddComponent<StorageGlyph>();talkGlyph.symbol="talk";talkGlyph.color=skin.Navy;talkGlyph.raycastTarget=false;
            var interact=Button(plazaAction,"대화하기",game.InteractTown);interact.name="town-interact";Place((RectTransform)interact.transform,12,168,226,52);PlazaButton(interact,true);plazaActionText=interact.GetComponentInChildren<Text>();
            plazaInteract=interact;
            plazaBuy=Button(plazaAction,"구매",()=>game.InteractEquipmentMerchant(EquipmentShopTab.Buy));plazaBuy.name="town-merchant-buy";Place((RectTransform)plazaBuy.transform,12,168,110,52);PlazaButton(plazaBuy,true);
            plazaSell=Button(plazaAction,"판매",()=>game.InteractEquipmentMerchant(EquipmentShopTab.Sell));plazaSell.name="town-merchant-sell";Place((RectTransform)plazaSell.transform,128,168,110,52);PlazaButton(plazaSell);
            BuildTownQuest();overlay.SetAsLastSibling();RefreshPlaza();
        }
        RectTransform CreateTownBubble(string id,string contentName,string npcName,bool plazaSkin=false)
        {
            bool hasContent=!string.IsNullOrEmpty(contentName),hasName=!string.IsNullOrEmpty(npcName);
            var bubble=Rect(id,root);bubble.anchorMin=bubble.anchorMax=Vector2.zero;bubble.pivot=new Vector2(.5f,0);
            bubble.sizeDelta=new Vector2(208,hasContent&&hasName?54:32);
            if(hasContent)
            {
                var title=Label(bubble,contentName,22,plazaSkin?UiSkin.Casual.Gold:gold,TextAnchor.MiddleCenter);title.name="Content name";title.fontStyle=plazaSkin?FontStyle.Normal:FontStyle.Bold;
                if(plazaSkin)title.font=UiSkin.Casual.LabelFont(title.text);
                Place(title.rectTransform,0,0,208,32);OutlinePlazaText(title,plazaSkin?2:1,plazaSkin?3:0,plazaSkin);
            }
            if(hasName)
            {
                var name=Label(bubble,npcName,hasContent?14:16,plazaSkin?UiSkin.Casual.White:pale,TextAnchor.MiddleCenter);name.name="NPC name";if(plazaSkin)name.font=UiFonts.CasualWeight(700);
                Place(name.rectTransform,0,hasContent?32:0,208,hasContent?22:32);OutlinePlazaText(name,casual:plazaSkin);
            }
            return bubble;
        }
        void PositionTownBubble(RectTransform bubble,Vector3 screen,bool town=true,float minimumTextScale=0)
        {
            var safe=UiSafeArea.Current;float canvasScale=root.parent.GetComponent<Canvas>().scaleFactor;
            float scale=globalHud?.Layout?.scale??canvasScale;
            if(town&&globalHud?.Layout!=null)scale=TownPlazaLayout.BubbleScale(globalHud.Layout);
            scale=Mathf.Max(scale,minimumTextScale);
            if(bubble==plazaPortalBubble)scale=Mathf.Max(scale,14f/22);
            // Only town labels opt into the mobile floor; edict preview labels keep their own ratio.
            bubble.localScale=Vector3.one*(scale/Mathf.Max(.001f,canvasScale));
            float headerPixels=header.rect.height*canvasScale;
            if(town&&plazaTitlePlate!=null)headerPixels=Mathf.Max(headerPixels,(-plazaTitlePlate.anchoredPosition.y+plazaTitlePlate.rect.height)*canvasScale);
            bool visible=screen.z>0&&screen.x>safe.xMin+8*scale&&screen.x<safe.xMax-8*scale
                &&screen.y>safe.yMin+32*scale&&screen.y+bubble.rect.height*scale<safe.yMax-headerPixels;
            bubble.gameObject.SetActive(visible&&(!town||!TownNavigationOpen));
            if(!visible||!RectTransformUtility.ScreenPointToLocalPointInRectangle(root,new Vector2(screen.x,screen.y),null,out var local))return;
            var position=local-root.rect.min;float half=bubble.rect.width*bubble.localScale.x/2,padding=8*bubble.localScale.x;
            position.x=Mathf.Clamp(position.x,half+padding,Mathf.Max(half+padding,root.rect.width-half-padding));bubble.anchoredPosition=position;
        }
        void CompactPlazaHeader()
        {
            header.sizeDelta=new Vector2(0,44);header.GetComponent<Image>().enabled=false;
            headerApron.gameObject.SetActive(false);headerSubtitle.gameObject.SetActive(false);
            header.Find("Gold divider").gameObject.SetActive(false);
            headerTitle.name="Title";headerTitle.fontSize=18;headerTitle.horizontalOverflow=HorizontalWrapMode.Overflow;Span(headerTitle.rectTransform,14,4,64,32);UiSkin.Casual.ApplyLabel(headerTitle);
            plazaTitlePlate=Rect("Town title backing",header);plazaTitlePlate.SetAsFirstSibling();
            plazaTitleUnit=0;plazaTitleSurface=plazaTitlePlate.gameObject.AddComponent<StorageSurface>();plazaTitleSurface.SetSkin(UiSkin.Casual,radius:16,borderWidth:2,shadow:3);plazaTitleSurface.raycastTarget=false;
            var settings=header.GetComponentInChildren<Button>();plazaSettings=settings;Right((RectTransform)settings.transform,4,0,44,44);
            settings.GetComponent<Image>().color=Color.clear;
            settings.GetComponent<UIRectBorder>().enabled=false;
            CasualSettingsStyle.Button(settings);((UiButton)settings).Face.SetRound(true);
            PlazaSkin.ApplyChrome((UiButton)settings);
            ((UiButton)settings).SetCasualAppearance(borderWidth:2,shadow:3,edge:UiSkin.Casual.Navy);
            ReflowPlazaGear(44);
        }
        static void OutlinePlazaText(Text label,float width=1,float drop=0,bool casual=false)
        {
            if(!casual)
            {
                var outline=label.gameObject.AddComponent<Outline>();outline.effectColor=UiTheme.Tint(UiTheme.VoidHex);
                outline.effectDistance=new Vector2(1,-1);outline.useGraphicAlpha=true;return;
            }
            CasualBattleHudStyle.Outline(label,width);
            if(drop<=0)return;
            var shadow=label.gameObject.AddComponent<Shadow>();shadow.effectColor=UiSkin.Casual.Navy;shadow.effectDistance=new Vector2(0,-drop);shadow.useGraphicAlpha=true;
        }
        static void PlazaButton(Button button,bool primary=false)
        {
            CasualSettingsStyle.Button(button,primary?UiButtonRole.Primary:UiButtonRole.Action);
            PlazaSkin.ApplyChrome((UiButton)button);
            ((UiButton)button).SetCasualAppearance(radius:14,borderWidth:primary?3:2,shadow:3,edge:UiSkin.Casual.Navy);
            var label=button.GetComponentInChildren<Text>();label.fontSize=20;
            if(primary){var shadow=label.gameObject.AddComponent<Shadow>();shadow.effectColor=UiSkin.Casual.Navy;shadow.effectDistance=new Vector2(0,-2);}
        }
        static StorageSurface PlazaSurface(RectTransform panel,float radius,float border,float shadow)
        {
            panel.GetComponent<Image>().color=Color.clear;
            var face=UiLayout.Rect("Casual town surface",panel);Stretch(face);face.SetAsFirstSibling();
            var surface=face.gameObject.AddComponent<StorageSurface>();surface.SetSkin(UiSkin.Casual,radius:radius,borderWidth:border,shadow:shadow);surface.raycastTarget=false;return surface;
        }
        static RectTransform PlazaCircle(string name,RectTransform parent,Vector2 min,Vector2 max,Color tint)
        {
            var r=UiLayout.Rect(name,parent);r.anchorMin=min;r.anchorMax=max;r.offsetMin=r.offsetMax=Vector2.zero;
            var circle=r.gameObject.AddComponent<TownCircleGraphic>();circle.color=tint;circle.raycastTarget=false;return r;
        }
        void ReflowPlazaGear(float size)
        {
            var face=((UiButton)plazaSettings).Face.rectTransform;
            face.gameObject.SetActive(false);
            var gear=(RectTransform)plazaSettings.transform.Find("Settings gear");gear.offsetMin=gear.offsetMax=Vector2.zero;
        }
        void ReflowPlazaGuide(GlobalHudLayout layout,float unit)
        {
            var bounds=TownPlazaLayout.GuideBounds(layout);var position=new Vector2(bounds.x*unit,-bounds.y*unit);var zoom=Vector3.one*unit;
            if(plazaGuide.sizeDelta==bounds.size&&plazaGuide.anchoredPosition==position&&plazaGuide.localScale==zoom)return;
            Place(plazaGuide,bounds.x*unit,bounds.y*unit,bounds.width,bounds.height);plazaGuide.localScale=zoom;
            bool mobile=!layout.landscape||layout.compact;float row=mobile?(layout.landscape?30:40):46,gap=mobile?5:7,inset=mobile?8:11;
            var buttons=plazaGuide.GetComponentsInChildren<Button>(true);
            for(int i=0;i<buttons.Length;i++)
            {Place((RectTransform)buttons[i].transform,inset,inset+i*(row+gap),bounds.width-2*inset,row);buttons[i].GetComponentInChildren<Text>().fontSize=mobile?(layout.landscape?13:15):18;}
            plazaGuideSurface.SetSkin(UiSkin.Casual,radius:16,borderWidth:mobile?2:3,shadow:4);
        }
        // The mockup's talk button is a centred row: a 1.05em chat glyph, 6px, then the label.
        void ReflowPlazaTalk()
        {
            var label=plazaTalk.GetComponentInChildren<Text>();var icon=plazaTalk.transform.Find("Talk icon") as RectTransform;if(label==null||icon==null)return;
            var button=(RectTransform)plazaTalk.transform;float size=label.fontSize*1.05f,gap=6,group=size+gap+label.preferredWidth;
            label.rectTransform.offsetMin=new Vector2((size+gap)/2,label.rectTransform.offsetMin.y);label.rectTransform.offsetMax=new Vector2((size+gap)/2,label.rectTransform.offsetMax.y);
            Place(icon,(button.rect.width-group)/2,(button.rect.height-size)/2,size,size);
        }
        string plazaServiceSource;
        void PlazaService(string value)
        {
            if(plazaServiceSource==value)return;plazaServiceSource=value;
            if(plazaAction.gameObject.activeSelf&&Application.isPlaying)plazaEntrance.Restart();
            var skin=UiSkin.Casual;plazaServiceName.font=skin.LabelFont(value);
            float Width(string text)=>plazaServiceName.cachedTextGeneratorForLayout.GetPreferredWidth(text,plazaServiceName.GetGenerationSettings(Vector2.zero))/plazaServiceName.pixelsPerUnit;
            float available=plazaServiceName.rectTransform.rect.width;
            if(Width(value)>available)
            {
                // Select the shared fallback before measuring the suffix; never shrink the service title.
                plazaServiceName.font=skin.LabelFont(value+"…");int length=value.Length;
                while(length>0&&Width(value.Substring(0,length).TrimEnd()+"…")>available)length--;
                value=value.Substring(0,length).TrimEnd()+"…";
            }
            plazaServiceName.text=value;
        }
        void PlazaActionLabel(string value)
        {
            if(plazaActionText.text==value)return;plazaActionText.text=value;plazaActionText.font=UiSkin.Casual.LabelFont(value);
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
                bool mobile=!layout.landscape||layout.compact;
                float titleScale=layout.presentationScale/Mathf.Max(.001f,scale);
                float joystickBorder=(mobile?2:3)*titleScale,joystickDrop=(mobile?2:4)*titleScale;
                plazaJoystickRim.SetRingWidth(joystickBorder);
                plazaKnobFace.offsetMin=Vector2.one*joystickBorder;plazaKnobFace.offsetMax=-plazaKnobFace.offsetMin;
                plazaKnobDrop.anchoredPosition=new Vector2(0,-joystickDrop);
                float hudScale=layout.scale/Mathf.Max(.001f,scale);
                float gap=6*hudScale,edge=12*hudScale;
                float dockLimit=(safe.height-layout.potionBounds.yMax*layout.scale)/scale;
                float iconSize=DockIconSize(dockLimit,true);
                var settingsRect=(RectTransform)plazaSettings.transform;
                Right(settingsRect,edge,edge,iconSize/titleScale,iconSize/titleScale);settingsRect.localScale=Vector3.one*titleScale;ReflowPlazaGear(iconSize/titleScale);
                float dockTop=edge+iconSize+gap,dockRight=edge;
                // Town has no edict preview to reserve above the shortcut column.
                Right((RectTransform)contentDock.transform,dockRight,dockTop,iconSize,iconSize);contentDock.Reflow(iconSize,gap);
                ReflowEventButton();
                if(plazaTitleUnit!=titleScale)
                {plazaTitleUnit=titleScale;plazaTitleSurface.SetSkin(UiSkin.Casual,radius:16*titleScale,borderWidth:2*titleScale,shadow:3*titleScale);}
                ReflowPlazaGuide(layout,titleScale);
                headerTitle.fontSize=Mathf.Max(1,Mathf.RoundToInt((mobile?14:18)*titleScale));
                float titleHeight=(mobile?28:32)*titleScale,padding=12*titleScale;
                float titleWidth=Mathf.Min(headerTitle.preferredWidth+padding*2,root.rect.width-iconSize-36*titleScale);
                Place(plazaTitlePlate,8*titleScale,4*titleScale,titleWidth,titleHeight+3*titleScale);Place(headerTitle.rectTransform,8*titleScale+padding,4*titleScale,titleWidth-2*padding,titleHeight);
                RefreshTownQuest(iconSize,edge,titleScale,mobile,!layout.landscape);
                float cardScale=TownPlazaLayout.CardScale(layout,portal)/Mathf.Max(.001f,scale);
                plazaEntrance.SetRestingScale(Vector3.one*cardScale);
                float hudTop=layout.potions.Concat(layout.actives).Max(r=>r.yMax)*layout.scale/scale;
                var joystick=new Rect(plazaJoystickRect.anchoredPosition,plazaJoystickRect.sizeDelta);
                var bounds=TownPlazaLayout.CardBounds(root.rect.size,cardHeight,cardScale,dockRight+iconSize+gap,hudTop,joystick,!layout.landscape,12/scale);
                plazaEntrance.SetRestingPosition(new Vector2(bounds.xMax-root.rect.width,bounds.center.y-root.rect.height*.35f));
            }
            plazaStatus.text=walk.Destination.HasValue?Loc.F("{0}으로 이동 중 · {1:0.0}m",TownLayout.Station(walk.Destination.Value).name,walk.Remaining):Loc.F("{0} Lv.{1} · 중앙 길 횡단 약 20초",game.catalog.classNames[(int)game.Store.Data.Hero.heroClass],game.Store.Data.Hero.level);
            // The card rises and fades in each time it appears, and again when it moves on to another service.
            bool showCard=(portal||near.HasValue||npc!=null)&&!TownNavigationOpen&&!CommonPanelOpen;
            if(showCard&&!plazaAction.gameObject.activeSelf&&Application.isPlaying)plazaEntrance.Restart();
            plazaAction.gameObject.SetActive(showCard);
            plazaTalk.gameObject.SetActive(!portal&&npc!=null);ReflowPlazaTalk();
            bool merchant=!portal&&(near==TownStation.Merchant||near==TownStation.Gambler);
            plazaInteract.gameObject.SetActive(portal||near.HasValue&&!merchant);
            plazaBuy.gameObject.SetActive(merchant);plazaSell.gameObject.SetActive(merchant);
            plazaInteract.name=portal?"town-portal-resume":"town-interact";
            Place((RectTransform)plazaInteract.transform,12,portal?112:168,226,portal?44:52);
            if(portal)
            {
                PlazaService(Loc.T("복귀 포탈"));plazaNpcName.text="";
                plazaServiceDetail.text=Loc.F("균열 {0:00}단계 · 이전 전투를 이어갑니다.",game.Store.Data.suspendedRun.stage);
                PlazaActionLabel(Loc.T("균열로 복귀"));
            }
            else if(near.HasValue)
            {
                var s=TownLayout.Station(near.Value);PlazaService(Loc.T(s.name));plazaNpcName.text=Loc.T(s.npcName);plazaServiceDetail.text=Loc.T(s.service);PlazaActionLabel(Loc.T(s.action));
            }
            if(!portal&&npc!=null&&!npc.Station.HasValue)
            {PlazaService(Loc.T(npc.Role));plazaNpcName.text=Loc.T(npc.name);plazaServiceDetail.text="";}
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
            if(station==TownStation.RiftKeeper&&game.Store.Data.guide.townQuest==TownQuest.TalkRiftKeeper){ResetTownInput();game.Town?.Cancel();ShowAntonGossip();return;}
            if(station==TownStation.RiftKeeper&&TownQuest.RiftLocked(game.Store.Data.guide)){ResetTownInput();game.Town?.Cancel();ShowAntonLock();return;}
            if(station==TownQuest.ForgeStation&&game.Store.Data.guide.townQuest==TownQuest.EnhanceWeapon&&TownQuest.Weapon(game.Store.Data.Hero)!=null){ResetTownInput();game.Town?.Cancel();ShowForgeLesson();return;}
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
