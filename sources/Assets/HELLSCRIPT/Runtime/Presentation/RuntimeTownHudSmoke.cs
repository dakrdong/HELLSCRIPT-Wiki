using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Hellscript
{
    // Drives the production controls and movement state with an isolated save in a development player.
    public sealed class RuntimeTownHudSmoke:MonoBehaviour
    {
        GameController game;string output;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void Boot()
        {
            if(!Debug.isDebugBuild||!Environment.GetCommandLineArgs().Contains("-hellscriptTownHudSmoke"))return;
            Application.runInBackground=true;
            Application.logMessageReceived+=(m,s,t)=>{if(t==LogType.Exception)Application.Quit(1);};
            new GameObject("Town HUD acceptance").AddComponent<RuntimeTownHudSmoke>();
        }
        static void Require(bool ok,string message){if(!ok)throw new InvalidOperationException(message);}
        static Rect Pixels(RectTransform rect)
        {var corners=new Vector3[4];rect.GetWorldCorners(corners);return Rect.MinMaxRect(corners[0].x,corners[0].y,corners[2].x,corners[2].y);}
        static bool Clickable(Button button)
        {
            var hits=new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=Pixels((RectTransform)button.transform).center},hits);
            return hits.Count>0&&hits[0].gameObject.GetComponentInParent<Button>()==button;
        }
        IEnumerator Capture(string name)
        {
            Canvas.ForceUpdateCanvases();yield return new WaitForEndOfFrame();
            var image=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(output,name+".png"),image.EncodeToPNG());Destroy(image);
        }
        void CheckNpcNames()
        {
            var rects=game.UI.GetComponentsInChildren<RectTransform>(true);
            foreach(var station in TownLayout.Stations)
            {
                var bubble=rects.Single(r=>r.name=="NPC bubble "+station.id);
                var title=bubble.Find("Content name").GetComponent<Text>();
                Require(title.text==Loc.T(station.name)&&!title.raycastTarget,"Wrong content label: "+station.id);
                Require(title.preferredHeight<=title.rectTransform.rect.height+1,"Content label wrapped or clipped: "+station.id);
                if(string.IsNullOrEmpty(station.npcName))continue;
                var name=bubble.Find("NPC name").GetComponent<Text>();
                Require(name.text==Loc.T(station.npcName)&&name.fontSize<title.fontSize&&!name.raycastTarget,"Wrong personal name or size: "+station.id);
                Require(Pixels(title.rectTransform).yMin>=Pixels(name.rectTransform).yMax-1,"Personal name is not below content: "+station.id);
                Require(name.preferredHeight<=name.rectTransform.rect.height+1,"Personal name clipped: "+station.id);
                var npc=GameObject.Find("NPC "+station.id);
                Require(npc!=null&&Vector3.Distance(npc.transform.position,WorldView.TownPoint(station.NpcPosition))<.01f,"Missing or misplaced attendant: "+station.id);
            }
            foreach(var resident in TownLayout.Residents)
            {
                var bubble=rects.Single(r=>r.name=="Resident bubble "+resident.id);
                Require(bubble.GetComponentsInChildren<Text>(true).Length==1&&bubble.GetComponentInChildren<Text>(true).text==Loc.T(resident.name),"Resident has a content title or wrong name: "+resident.id);
                Require(bubble.GetComponentInChildren<Button>(true)==null,"Resident acquired a service action.");
                var npc=GameObject.Find("Resident "+resident.id);
                Require(npc!=null&&Vector3.Distance(npc.transform.position,WorldView.TownPoint(resident.position))<.01f,"Missing or misplaced resident: "+resident.id);
                Require(TownLayout.Walkable(resident.position)&&TownLayout.Nearby(resident.position)==null,"Resident overlaps a building or service.");
            }
        }
        IEnumerator VisitNamedNpcs()
        {
            foreach(string language in new[]{"ko","en"})
            {
                game.ApplyLanguage(language);Screen.SetResolution(language=="ko"?1600:900,language=="ko"?900:1600,FullScreenMode.Windowed);
                yield return new WaitForSecondsRealtime(.6f);
                foreach(var station in TownLayout.Stations.Where(s=>!string.IsNullOrEmpty(s.npcName)))
                {
                    game.RequestStation(station.id);for(int n=0;n<1000&&game.Town.Walking;n++)game.Town.Tick(.05f);
                    yield return new WaitForSecondsRealtime(.6f);game.UI.RefreshPlaza();Canvas.ForceUpdateCanvases();
                    Require(game.Town.Nearby==station.id,"Named station is unreachable: "+station.id);CheckNpcNames();
                    var bubble=game.UI.GetComponentsInChildren<RectTransform>().Single(r=>r.name=="NPC bubble "+station.id);
                    Require(bubble.gameObject.activeInHierarchy,"Nearby nameplate is hidden.");
                    var bounds=Pixels(bubble);Require(UiSafeArea.Current.Contains(bounds.min)&&UiSafeArea.Current.Contains(bounds.max),"Nameplate escaped the safe area.");
                    yield return Capture("npc-"+station.id+"-"+language);
                    string buttonName=station.id==TownStation.Merchant?"town-merchant-buy":"town-interact";
                    var button=game.UI.GetComponentsInChildren<Button>().Single(b=>b.name==buttonName);
                    Require(!game.UI.EquipmentShopOpen&&!game.UI.StorageOpen&&!game.UI.RuneMasterOpen,"A service opened on arrival.");
                    ExecuteEvents.Execute(button.gameObject,new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left},ExecuteEvents.pointerClickHandler);yield return null;
                    bool opened=station.id==TownStation.Blacksmith?game.UI.BlacksmithOpen:station.id==TownStation.Merchant?game.UI.EquipmentShopOpen:station.id==TownStation.Warehouse?game.UI.StorageOpen:station.id==TownStation.RuneMaster?game.UI.RuneMasterOpen:game.UI.Page=="rift-keeper";
                    Require(opened,"Named NPC opened the wrong service: "+station.id);
                    game.UI.CloseStorage();game.UI.CloseEquipmentShop();game.UI.CloseRuneMaster();game.UI.ShowTown();yield return null;
                }
                foreach(var resident in TownLayout.Residents)
                {
                    game.Town.RequestPoint(resident.position);for(int n=0;n<1000&&game.Town.Walking;n++)game.Town.Tick(.05f);
                    yield return new WaitForSecondsRealtime(.6f);game.UI.RefreshPlaza();
                    Require(Vector2.Distance(game.Town.Position,resident.position)<.1f&&game.Town.Nearby==null,"Resident route changed service state.");
                    Require(game.UI.GetComponentsInChildren<RectTransform>().Any(r=>r.name=="Resident bubble "+resident.id),"Nearby resident name is hidden.");
                    yield return Capture("resident-"+resident.id+"-"+language);
                }
                Require(Loc.MissingCount==0,"Missing NPC translation: "+string.Join(";",Loc.Missing));
            }
            File.WriteAllText(Path.Combine(output,"npc-runtime.txt"),"PASS: five named services opened through production pointer-click handlers in Korean/landscape and English/portrait; three named residents reached without service actions; live NPC placement, content above smaller personal names, text fit, safe-area bounds and translations checked. Desktop simulated input; physical mobile not tested.\n");
            game.EnterPlaza(true);game.ApplyLanguage("ko");yield return null;
        }
        void CheckLayout(string name)
        {
            CheckNpcNames();
            var stick=game.UI.GetComponentInChildren<TownJoystick>();var rect=Pixels((RectTransform)stick.transform);
            var safe=UiSafeArea.Current;var hud=game.UI.GlobalHud.Layout;
            Require(Mathf.Abs(rect.width-Mathf.Min(safe.width,safe.height)*.2f)<2,"Joystick did not scale with safe area: "+name);
            Require(Mathf.Abs(rect.width-rect.height)<1&&rect.yMin>safe.yMin+hud.status.yMax*hud.scale,"Joystick shape or HUD clearance: "+name);
            Require(rect.xMin>=safe.xMin&&rect.xMax<=safe.xMax&&rect.yMax<=safe.yMax,"Joystick escaped safe area: "+name);
            if(!hud.landscape)Require(Mathf.Abs(rect.center.x-safe.center.x)<1,"Portrait joystick is not centred: "+name);
            foreach(var obstacle in hud.actives.Concat(hud.passives).Append(hud.potionTray))
            {var bounds=hud.Pixels(obstacle);bounds.position+=safe.position;Require(!rect.Overlaps(bounds),"Joystick overlaps controls: "+name);}
            var style=game.UI.GlobalHud.Style;
            float baseline=hud.landscape?Mathf.Max(style.skillBottom,style.xpBottom+style.xpThickness+style.captionHeight+8):style.skillBottom;
            Require(Mathf.Abs(hud.actives[0].y-baseline)<.01f,"Action row is not bottom anchored: "+name);
            var tray=game.UI.GlobalHud.transform.Find("HUD safe area/Potion tray").GetComponent<StorageSurface>();
            Require(tray.isActiveAndEnabled&&tray.top.a>.5f&&!tray.raycastTarget,"Potion backing missing or blocks input.");
            for(int i=0;i<3;i++)Require(Clickable(game.UI.GlobalHud.transform.Find("HUD safe area/Potion "+i).GetComponent<Button>()),"Potion input is obscured: "+name);
            foreach(var potion in hud.potions)foreach(var skill in hud.actives.Concat(hud.passives))
                Require(potion.yMin>skill.yMax,"Potion below skill: "+name);
            var texts=game.UI.GetComponentsInChildren<Text>();
            Require(!texts.Any(t=>t.text.Contains("조이스틱으로 이동")||t.text.Contains("중앙 길 횡단")||t.text==Loc.T("시설 안내")||t.text==Loc.T("메뉴")),"Hidden town chrome is visible.");
            var header=game.UI.GetComponentsInChildren<RectTransform>().Single(t=>t.name=="Header");
            Require(!header.GetComponent<Image>().enabled&&header.rect.height<=44,"Header background or height regressed.");
            var title=header.GetComponentsInChildren<Text>().Single(t=>t.text==Loc.T("잿빛 숲 · 정착민 마을"));
            Require(title.fontSize<=18&&title.GetComponent<Outline>()!=null,"Compact title missing.");
            Require(title.preferredWidth<=title.rectTransform.rect.width+1&&title.preferredHeight<=title.rectTransform.rect.height+1,"Town title clipped: "+name);
            var plate=header.Find("Town title backing").GetComponent<StorageSurface>();
            Require(plate.top.a>0&&plate.top.a<1&&!plate.raycastTarget&&plate.rectTransform.rect.width<header.rect.width,"Translucent title backing missing.");
            var dock=game.UI.GetComponentInChildren<ContentDockView>();dock.Snap(true);Canvas.ForceUpdateCanvases();
            foreach(var button in dock.GetComponentsInChildren<Button>().Append(header.GetComponentsInChildren<Button>().Single(b=>b.name=="설정·안내")))
            {
                Require(!button.GetComponent<UIRectBorder>().enabled,"Shortcut still has a rectangular border.");
                Require(Clickable(button),"Shortcut input is obscured: "+button.name+" / "+name);
                var bounds=Pixels((RectTransform)button.transform);
                Require(bounds.xMin>=safe.xMin&&bounds.xMax<=safe.xMax&&bounds.yMax<=safe.yMax&&bounds.yMin>=safe.yMin+hud.potionTray.yMax*hud.scale,"Shortcut escaped its safe column: "+name);
            }
            foreach(var bubble in game.UI.GetComponentsInChildren<RectTransform>(true).Where(t=>t.name.StartsWith("NPC bubble ")))
            {
                Require(bubble.GetComponentInChildren<Image>(true)==null,"NPC background still exists.");
                Require(bubble.GetComponentInChildren<Text>(true).GetComponent<Outline>()!=null,"NPC outline missing.");
            }
            if(name=="1600x900-100")Require(game.UI.GetComponentsInChildren<RectTransform>().Any(t=>t.name=="NPC bubble Merchant"),"Visible merchant name was culled by the old header bounds.");
            File.AppendAllText(Path.Combine(output,"geometry.txt"),name+" "+Screen.width+"x"+Screen.height+" pad="+rect+" potion="+hud.potions[0]+" PASS\n");
        }
        IEnumerator Start()
        {
            var args=Environment.GetCommandLineArgs();int save=Array.IndexOf(args,"-hellscriptSavePath"),shots=Array.IndexOf(args,"-hellscriptScreenshots");
            Require(save>=0&&shots>=0,"Use isolated save and evidence directories.");output=args[shots+1];Directory.CreateDirectory(output);
            yield return new WaitForSecondsRealtime(1);game=FindAnyObjectByType<GameController>();Require(game?.Store!=null,"Game failed to initialize.");
            game.EnterPlaza(true);UiSafeArea.AspectRatio=0;game.ApplyLanguage("ko");
            game.InterfaceScale.Apply(100);game.UI.ApplyInterfaceScale();yield return VisitNamedNpcs();
            foreach(string language in new[]{"ko","en"})
            foreach(var size in new[]{new Vector2Int(1600,900),new Vector2Int(1600,1000),new Vector2Int(2100,900),new Vector2Int(900,1600),new Vector2Int(956,440),new Vector2Int(440,956),new Vector2Int(640,360),new Vector2Int(360,640)})
            foreach(int percent in new[]{50,100,150})
            {
                game.ApplyLanguage(language);Screen.SetResolution(size.x,size.y,FullScreenMode.Windowed);game.InterfaceScale.Apply(percent);game.UI.ApplyInterfaceScale();
                yield return new WaitForSecondsRealtime(.5f);game.UI.RefreshHud();yield return null;
                Require(Screen.width==size.x&&Screen.height==size.y,"Resolution request did not apply.");
                string name=size.x+"x"+size.y+"-"+percent+"-"+language;CheckLayout(name);
                var stick=game.UI.GetComponentInChildren<TownJoystick>();var visibility=stick.GetComponent<CanvasGroup>();
                Require(Mathf.Abs(visibility.alpha-.3f)<.001f,"Initial joystick opacity.");
                var center=Pixels((RectTransform)stick.transform).center;
                var pointer=new PointerEventData(EventSystem.current){pointerId=7,position=center,button=PointerEventData.InputButton.Left};
                ExecuteEvents.Execute(stick.gameObject,pointer,ExecuteEvents.pointerDownHandler);Require(visibility.alpha==1,"Held joystick opacity.");
                pointer.position=center+Vector2.right*Pixels((RectTransform)stick.transform).width*.3f;
                ExecuteEvents.Execute(stick.gameObject,pointer,ExecuteEvents.dragHandler);var before=game.Town.Position;
                yield return new WaitForSecondsRealtime(.2f);Require(game.Town.Position.x>before.x,"Joystick did not move the real player.");
                stick.OnPointerUp(new PointerEventData(EventSystem.current){pointerId=8});Require(visibility.alpha==1&&stick.Value.x>0,"Second pointer stole movement.");
                if(percent==100&&size.x==1600)yield return Capture("landscape-held");
                ExecuteEvents.Execute(stick.gameObject,pointer,ExecuteEvents.pointerUpHandler);before=game.Town.Position;
                yield return new WaitForSecondsRealtime(.15f);
                Require(stick.Value==Vector2.zero&&stick.Knob.anchoredPosition==Vector2.zero&&Mathf.Abs(visibility.alpha-.3f)<.001f,"Release did not reset input and opacity.");
                Require(game.Town.Position==before,"Player continued moving after release.");
                if(percent==100||percent==150&&size.x==440)yield return Capture(name);
            }
            Screen.SetResolution(1600,900,FullScreenMode.Windowed);game.InterfaceScale.Apply(100);game.UI.ApplyInterfaceScale();
            UiSafeArea.Simulate(90,30,12,24);yield return new WaitForSecondsRealtime(.6f);game.UI.RefreshHud();yield return null;CheckLayout("safe-area");yield return Capture("safe-area");
            UiSafeArea.StopSimulating();Screen.SetResolution(440,956,FullScreenMode.Windowed);UiSafeArea.Simulate(0,34,0,44);
            yield return new WaitForSecondsRealtime(.6f);game.UI.RefreshHud();yield return null;CheckLayout("portrait-safe-area");yield return Capture("portrait-safe-area");
            Screen.SetResolution(1600,900,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(.6f);
            UiSafeArea.StopSimulating();game.ApplyLanguage("en");yield return new WaitForSecondsRealtime(.3f);CheckLayout("english");
            var dock=game.UI.GetComponentInChildren<ContentDockView>();dock.Snap(true);
            var storage=dock.Items.GetComponentsInChildren<Button>().Single(b=>b.name=="창고");
            Require(storage.GetComponentInChildren<Text>().text==""&&storage.targetGraphic is Image art&&art.sprite!=null&&art.sprite.name=="menu-storage","Attached warehouse icon missing.");
            dock.Toggle.GetComponent<Button>().onClick.Invoke();yield return new WaitForSecondsRealtime(.6f);
            Require(dock.Settled&&!dock.Open&&!Clickable(storage),"Folded shortcut remains clickable.");
            dock.Toggle.GetComponent<Button>().onClick.Invoke();yield return new WaitForSecondsRealtime(.6f);
            Require(dock.Settled&&dock.Open&&Clickable(storage),"Reopened shortcut is inaccessible.");
            yield return Capture("storage-shortcut");storage.onClick.Invoke();Require(game.UI.StorageOpen,"Storage shortcut did not open real storage.");
            game.UI.CloseStorage();game.EnterPlaza();game.ApplyLanguage("ko");yield return null;
            var pad=game.UI.GetComponentInChildren<TownJoystick>();var eventData=new PointerEventData(EventSystem.current){pointerId=2,position=Pixels((RectTransform)pad.transform).center+Vector2.right*20};pad.OnPointerDown(eventData);
            game.UI.GetComponentsInChildren<Button>().Single(b=>b.name=="설정·안내").onClick.Invoke();yield return null;
            Require(game.UI.CommonPanelOpen&&pad.Value==Vector2.zero&&Mathf.Abs(pad.GetComponent<CanvasGroup>().alpha-.3f)<.001f,"Settings did not stop movement.");
            File.WriteAllText(Path.Combine(output,"runtime.txt"),"PASS: 48 resolution/scale/language combinations, safe area, bottom-anchored actions, potion tray, centred portrait joystick, borderless shortcut buttons, title backing, production movement/release/second-pointer ownership, idle 0.3 / active 1.0 opacity, NPC names, original storage icon and real storage action. Desktop simulated pointer input; physical mobile not tested.\n");
            Debug.Log("HELLSCRIPT_TOWN_HUD_SMOKE_OK");Application.Quit(0);
        }
    }
}
