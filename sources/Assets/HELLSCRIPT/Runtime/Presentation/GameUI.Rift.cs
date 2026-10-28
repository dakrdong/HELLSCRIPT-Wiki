using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    public sealed partial class GameUI
    {
        RiftMinimap riftMinimap;
        // Kept by reference: on the rebuild frame root still holds the previous page's panel until it is destroyed.
        RectTransform riftMinimapPanel;
        Text chestCountText,fieldStatusText;
        Vector2Int chestShown=new Vector2Int(-1,-1);string chestShownLanguage="";
        public ContentWindowView ExpandedRiftMap=>riftMapWindow;
        bool riftMapWasPaused;ContentWindowView riftMapWindow;int minimapZoom;UiButton minimapZoomIn,minimapZoomOut;
        void AddRiftMinimap(RunState run)
        {
            riftMinimapPanel=null;if(run.layout.legacy)return;AddRiftOverlay(run);
            var holder=Box("Rift minimap",root,new Color(.025f,.035f,.055f,.91f));riftMinimapPanel=holder;
            if(CasualCombatHud)CasualBattleHudStyle.Panel(holder,true);
            holder.anchorMin=holder.anchorMax=new Vector2(1,1);holder.pivot=new Vector2(1,1);holder.anchoredPosition=new Vector2(-18,-275);holder.sizeDelta=new Vector2(212,281);
            var viewport=Box("Map viewport",holder,Color.clear);Place(viewport,8,8,196,196);viewport.gameObject.AddComponent<RectMask2D>();
            var graphic=new GameObject("Explored geometry",typeof(RectTransform),typeof(CanvasRenderer),typeof(RiftMinimap));graphic.transform.SetParent(viewport,false);Stretch((RectTransform)graphic.transform);
            riftMinimap=graphic.GetComponent<RiftMinimap>();riftMinimap.Configure(run,game.World.RiftFog);riftMinimap.RestoreView(minimapZoom,Vector2.zero);riftMinimap.raycastTarget=false;
            // The casual HUD shows the count as plain header text (PlaceCasualChestCount); the legacy panel keeps it under its map.
            chestCountText=Label(CasualCombatHud?header:holder,"",15,gold,TextAnchor.MiddleCenter);chestShown=new Vector2Int(-1,-1);
            if(CasualCombatHud)chestCountText.raycastTarget=false;else Place(chestCountText.rectTransform,4,205,204,28);
            fieldStatusText=Label(holder,"",15,pale,TextAnchor.MiddleCenter);Place(fieldStatusText.rectTransform,4,234,204,42);
            var button=holder.gameObject.AddComponent<UiButton>();button.targetGraphic=holder.GetComponent<Image>();button.Configure(UiButtonRole.Item);button.onClick.AddListener(ShowRiftMap);
            var skin=UiSkin.Casual;
            minimapZoomOut=UiIconButton.CreateGlyph(holder,"rift-minimap-zoom-out","minus",0,0,44,44,()=>SetMinimapZoom(minimapZoom-1),tint:skin.Navy);
            minimapZoomIn=UiIconButton.CreateGlyph(holder,"rift-minimap-zoom-in","plus",0,0,44,44,()=>SetMinimapZoom(minimapZoom+1),tint:skin.Navy);
            skin.ApplyChrome(minimapZoomOut);skin.ApplyChrome(minimapZoomIn);RefreshMinimapZoomButtons();
        }
        void SetMinimapZoom(int level)
        {
            minimapZoom=Mathf.Clamp(level,0,1);riftMinimap?.SetZoom(minimapZoom);RefreshMinimapZoomButtons();
        }
        void RefreshMinimapZoomButtons()
        {if(minimapZoomOut!=null)minimapZoomOut.interactable=minimapZoom>0;if(minimapZoomIn!=null)minimapZoomIn.interactable=minimapZoom<1;}
        void PlaceMinimapZoom(RectTransform panel,bool shown,float bottom)
        {
            if(minimapZoomIn==null||minimapZoomOut==null)return;
            minimapZoomIn.gameObject.SetActive(shown);minimapZoomOut.gameObject.SetActive(shown);if(!shown)return;
            float hit=44/BattleCanvasScale,gap=4/BattleCanvasScale,left=Mathf.Max(0,(panel.rect.width-hit*2-gap)/2);
            Place((RectTransform)minimapZoomOut.transform,left,Mathf.Max(0,bottom-hit-2/BattleCanvasScale),hit,hit);
            Place((RectTransform)minimapZoomIn.transform,left+hit+gap,Mathf.Max(0,bottom-hit-2/BattleCanvasScale),hit,hit);
        }
        // The panel keeps the chest counter, a navigation fault and the tap into the expanded map in every
        // map style; only the corner style draws the map inside it. It sits right of the left column (the
        // events and adventure guide buttons end at 132), below the header text or beside the menu button,
        // and 8 above the HUD/log stack. Returns the panel rectangle in top-left page units; cap limits the map side.
        Rect ReflowRiftMinimap(RectTransform mini,bool narrow,float cap,out float map)
        {
            if(CasualCombatHud)return PlaceCasualMinimap(mini,out map);
            Vector2 size=root.rect.size,hero=HeroOnPage();bool landscape=size.x>size.y,corner=game.MapDisplay.Mode==MapDisplayMode.Corner;var run=game.Combat.State;
            float top=landscape?88:narrow?158:122,limit=BattleHudTopOnPage-8;
            // The counter never wraps: the panel is at least as wide as its widest reading in this rift.
            float need=TextWidth(chestCountText,Loc.F("상자 {0} / {1}",run.layout.chests.Count,run.layout.chests.Count))+8;
            // The panel stays left of the content dock column (dockIcon wide, 12 from the right edge). A portrait lane
            // too narrow for the counter or for a map as large as the portrait cap reaches under that column
            // instead, right of the hero; the panel is then drawn under the header so the dock folds out over it.
            float column=24+dockIcon,lane=size.x-column-140;bool reach=!landscape&&(lane-8<size.x*.34f||lane<need);
            float width=reach?Mathf.Max(need,Mathf.Min(168,size.x-12-Mathf.Max(140,hero.x+HeroSideClearance))):Mathf.Min(168,lane);
            // A landscape panel spans the hero's rows, so it also stays right of the hero at the centre.
            if(landscape)width=Mathf.Max(need,Mathf.Min(width,size.x-column-hero.x-HeroSideClearance));
            // A navigation fault is laid out as one line first, then takes as many lines as fit; the rest ends
            // in an ellipsis and the whole message is on the expanded map.
            string fault=string.IsNullOrEmpty(run.navigationError)?"":Loc.T(run.navigationError);
            float line=fault==""?0:Mathf.Ceil(TextHeight(fieldStatusText,fault.Substring(0,1),width))+1,status=line;
            float under=Mathf.Floor(Mathf.Min(160,width-8,landscape?size.y*.4f:size.x*.34f,limit-top-33-status));
            // A landscape phone at a large text size has no room under the header: the panel moves beside
            // the menu button and its map goes right of the text, as tall as the gap above the live log
            // allows with thin margins. The text column keeps room for a two-digit counter; a navigation
            // fault needs the whole width.
            bool raised=limit-top<28+status||landscape&&corner&&status==0&&under<40;if(raised)top=8;
            float x=size.x-(raised?76+dockIcon:reach?12:column)-width;
            if(line>0){status=Mathf.Max(line,Mathf.Min(Mathf.Ceil(TextHeight(fieldStatusText,fault,width))+1,limit-top-28));under=Mathf.Min(under,Mathf.Floor(limit-top-33-status));}
            // A panel over the hero's column ends above the hero, as the boss status does.
            if(x<hero.x+HeroSideClearance)cap=Mathf.Min(cap,hero.y-HeroClearance-top-(raised?4:33+status));
            map=!corner?0:raised?(status>0?0:Mathf.Min(limit-top,width-116)):under;
            map=Mathf.Floor(Mathf.Min(map,cap));if(map<(raised?36:40))map=0;
            mini.gameObject.SetActive(true);mini.GetComponent<Image>().color=new Color(.02f,.03f,.04f,.45f);
            int order=header.GetSiblingIndex();if(reach!=(mini.GetSiblingIndex()<order))mini.SetSiblingIndex(order);
            bool beside=raised&&map>0;float y=beside?(map-16)*.5f:map>0?map+9:4,text=beside?width-map-6:width;
            var bounds=new Rect(x,top,width,beside?map+4:y+24+status);Place(mini,bounds.x,bounds.y,bounds.width,bounds.height);
            var viewport=mini.Find("Map viewport") as RectTransform;viewport.gameObject.SetActive(map>0);Place(viewport,beside?width-map-2:(width-map)*.5f,beside?2:4,map,map);
            Place(chestCountText.rectTransform,0,y,text,20);Place(fieldStatusText.rectTransform,0,y+21,width,Mathf.Max(1,status-1));fieldStatusText.gameObject.SetActive(line>0);
            fieldStatusText.text=fault;
            while(fieldStatusText.preferredHeight>status&&fieldStatusText.text.Length>8)
                fieldStatusText.text=fieldStatusText.text.Substring(0,fieldStatusText.text.Length-(fieldStatusText.text.EndsWith("…")?2:1))+"…";
            PlaceMinimapZoom(mini,map>0,beside?map+2:map+4);
            return bounds;
        }
        // Rewritten only when a number or the language changes; the header row is measured again when it does.
        void RefreshChestCount(RunState run)
        {
            if(chestCountText==null)return;
            int opened=0,total=run.layout.chests.Count;for(int i=0;i<total;i++)if(run.layout.chests[i].phase==ChestPhase.Opened)opened++;
            if(chestShown.x==opened&&chestShown.y==total&&chestShownLanguage==Loc.Language)return;
            chestShown=new Vector2Int(opened,total);chestShownLanguage=Loc.Language;
            chestCountText.text=Loc.F("상자 {0} / {1}",opened,total);battleSize=Vector2.zero;
        }
        static float TextWidth(Text t,string value)=>t.cachedTextGeneratorForLayout.GetPreferredWidth(value,t.GetGenerationSettings(Vector2.zero))/t.pixelsPerUnit;
        static float TextHeight(Text t,string value,float width)=>t.cachedTextGeneratorForLayout.GetPreferredHeight(value,t.GetGenerationSettings(new Vector2(width,0)))/t.pixelsPerUnit;
        public void ShowRiftMap()
        {
            if(game.Combat==null||riftMapWindow!=null)return;
            var run=game.Combat.State;if(run.layout==null)return;
            riftMapWasPaused=run.paused;run.paused=true;
            riftMapWindow=RiftMapWindow.Open(transform,run,game.World?.RiftFog,()=>InterfaceFactor,()=>RiftMapNotes(run),
                ()=>{bool ok=game.Combat.RetryRiftFault();game.Save();ShowToast(ok?"저장된 상태에서 다시 진행할 수 있습니다.":run.navigationError);riftMapWindow?.Repaint();},
                ()=>{riftMapWindow=null;if(game.Combat?.State==run)run.paused=riftMapWasPaused;});
        }
        void CloseRiftMap(){riftMapWindow?.Close();riftMapWindow=null;}
        static string RiftMapNotes(RunState run)
        {
            var lines=new List<string>{Loc.T("▲ 영웅   × 보스   ◇ 정예   + 성소\n□ 상자 · 잠금 표시: 봉인 · 빨간 빗금: 저주\n체크 표시는 개봉 완료를 뜻합니다.")};
            if(run.layout.objective==RiftObjectiveKind.Seals)lines.Add(Loc.T("육각형은 발견한 봉인석입니다. 보라색은 경비 중, 금색은 파괴 가능, 초록색 체크는 파괴 완료를 뜻합니다."));
            if(run.layout.objective==RiftObjectiveKind.EssenceCarriers)lines.Add(Loc.T("이중 마름모는 정수 운반자의 마지막 관측 위치입니다. 초록색은 처치 완료를 뜻합니다."));
            if(run.layout.objective==RiftObjectiveKind.Offerings)lines.Add(Loc.T("삼각형은 제물, 받침대는 제단입니다. 경비를 처치한 뒤 제물을 회수하고 3개를 제단에 바칩니다."));
            if(run.layout.objective!=RiftObjectiveKind.None)lines.Add(ObjectiveProgress(run.layout));
            if(run.layout.gates.Count>0)
            {
                lines.Add(Loc.T(run.layout.gateOpen?"보스 관문 · 열림":"보스 관문 · 닫힘"));
                lines.Add(Loc.T("빗장은 발견한 관문입니다. 보라색은 닫힘, 초록색은 열림을 뜻합니다."));
            }
            lines.Add(Loc.F("발견한 방 {0} / {1}\n개봉한 상자 {2} / {3}",run.visited.Count,run.layout.rooms.Count,run.layout.chests.Count(c=>c.phase==ChestPhase.Opened),run.layout.chests.Count));
            foreach(var shrine in run.layout.shrines.Where(s=>s.discovered))
                lines.Add(Loc.F("{0}\n{1}",Loc.T(shrine.definitionId=="SH01"?"길잡이의 성소 · 비전투 이동 +15%":"결의의 성소 · 피해 감소 +10%p"),Loc.T(shrine.phase==ShrinePhase.Used?"사용 완료":shrine.phase==ShrinePhase.Exhausted?"사용 종료":"사용 시 20초 지속")));
            foreach(var evt in run.layout.events.Where(e=>run.layout.chests.Any(c=>c.id==e.chestId&&c.discovered)))
                lines.Add(Loc.F("저주 상자 · {0}",evt.phase==RiftEventPhase.Active?Loc.F("잔향 진압 {0:0.0}초 남음",evt.remaining):Loc.T(evt.phase==RiftEventPhase.Succeeded?"진압 성공":evt.phase==RiftEventPhase.Failed?"진압 실패":evt.phase==RiftEventPhase.Cancelled?"이벤트 종료":"참여 설정과 남은 시간 조건에 따라 시작")));
            if(!string.IsNullOrEmpty(run.navigationError))lines.Add(Loc.T(run.navigationError));
            return string.Join("\n\n",lines);
        }
    }
}
