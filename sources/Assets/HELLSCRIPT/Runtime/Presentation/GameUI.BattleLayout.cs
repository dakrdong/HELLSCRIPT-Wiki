using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    public sealed partial class GameUI
    {
        RectTransform battleWorld;
        Button powerSavingButton,escapeButton;Text powerSavingCaption,escapeCaption;
        Vector2 battleSize;
        Rect battleSafe;
        // Width of the gear and the content shortcuts, in page units; the map panel and the header lines keep clear of that column.
        float dockIcon=44;
        float battleJournalTop,battleBossHeight;
        string battleFault="";
        bool battleBoss;
        string fullBattleAction="";
        // The hero's head reaches about 48 page units above its feet; 8 more keep a gap. Beside it, 20 clears its body.
        public const float HeroClearance=56,HeroSideClearance=20;
        public Rect BattleViewport {get;private set;}=new Rect(0,0,1,1);
        public float BattleViewHeight=>Page=="battle"&&battleWorld!=null?battleWorld.rect.height:Screen.height;
        void BuildBattleHud(RunState run)
        {
            battleWorld=Rect("Combat viewport",root.parent);battleWorld.SetAsFirstSibling();
            header.GetComponent<Image>().color=Color.clear;
            foreach(var child in header.GetComponentsInChildren<Image>())if(child.name=="Gold divider")child.enabled=false;
            footer.gameObject.SetActive(false);headerApron.gameObject.SetActive(false);footerApron.gameObject.SetActive(false);
            meterText=Label(header,"",15,pale);timerText=Label(header,"",18,gold);actionText=Label(header,"",14,gold);
            var menu=Button(header,"☰",ShowObservationMenu,Color.clear);menu.name="관찰 메뉴";
            // Medallions from the HUD menu family, each captioned underneath; the mandatory road has no way out.
            powerSavingButton=Button(header,"절전 모드",game.RequestIdle,Color.clear);powerSavingButton.targetGraphic=Emblem(powerSavingButton,"menu-power-saving");
            powerSavingButton.name="power-saving-entry";
            powerSavingButton.gameObject.SetActive(run.training<0&&!run.tutorial);
            string escapeLabel=run.training<0&&!run.tutorial?"포탈로 복귀":"탈출";
            escapeButton=Button(header,escapeLabel,ConfirmEscape,Color.clear);escapeButton.targetGraphic=Emblem(escapeButton,"menu-escape");
            escapeButton.name="battle-escape";escapeButton.gameObject.SetActive(!run.tutorial||run.tutorialReplay);
            powerSavingCaption=Label(header,"절전 모드",13,pale,TextAnchor.UpperCenter);powerSavingCaption.gameObject.SetActive(powerSavingButton.gameObject.activeSelf);
            escapeCaption=Label(header,escapeLabel,13,pale,TextAnchor.UpperCenter);escapeCaption.gameObject.SetActive(escapeButton.gameObject.activeSelf);
            AddContentDock(58,44);AddEventButton();
            AddBossHud(header);bossHud.GetComponent<Image>().color=Color.clear;
            AddRiftMinimap(run);incomingHud=null;
            // The puzzle tutorial shows the live DPS graph and the damage taken per monster kind from level 2.
            if(game.TrainingGroundRun||run.training<0&&!run.tutorial||PuzzleGraphs(run))BuildTrainingHud(run);
            if(PuzzleGraphs(run))BuildIncomingHud();
            BuildRiftActivityHud(run);BuildLiveJournal();RefreshHud();ReflowBattleHud();
        }
        void ReflowBattleHud()
        {
            if(Page!="battle"||battleWorld==null||game.Combat==null)return;
            Vector2 size=root.rect.size;Rect safe=UiSafeArea.Current;
            // The map panel stops above the live log, which moves with the HUD and its own toggle.
            string fault=game.Combat.State.navigationError??"";bool boss=bossHud!=null&&bossHud.gameObject.activeSelf;float journal=liveJournalPanel!=null?liveJournalPanel.anchoredPosition.y:0;
            // The boss status text changes during a fight; its tallest height at this size is kept, so the map
            // shrinks once when the status grows and does not breathe with the text.
            float bossHeight=boss?bossHud.rect.height:0;
            if(size==battleSize&&safe==battleSafe&&journal==battleJournalTop&&fault==battleFault&&boss==battleBoss&&bossHeight<=battleBossHeight)return;
            battleBossHeight=boss&&battleBoss&&size==battleSize&&safe==battleSafe?Mathf.Max(battleBossHeight,bossHeight):bossHeight;
            battleSize=size;battleSafe=safe;battleJournalTop=journal;battleFault=fault;battleBoss=boss;
            // The selected aspect frame is the camera region. The HUD only observes it.
            BattleViewport=UiSafeArea.FrameNormalized;
            battleWorld.anchorMin=BattleViewport.min;battleWorld.anchorMax=BattleViewport.max;battleWorld.offsetMin=battleWorld.offsetMax=Vector2.zero;
            float w=size.x;bool narrow=w<360;float textWidth=w-(narrow?132:170);
            var hud=globalHud?.Layout;
            dockIcon=hud==null?44:DockIconSize(root.rect.height-(LiveJournalHudInset+hud.potionBounds.yMax*hud.scale)/BattleCanvasScale,false);
            var mini=riftMinimapPanel;var panel=default(Rect);float map=0;if(mini!=null)panel=ReflowRiftMinimap(mini,narrow,float.MaxValue,out map);
            var medals=ReflowBattleMedallions(w,panel);
            // Header lines that share rows with a landscape map panel or the medallions stop short of them.
            float End(float y,float h)=>Mathf.Min(panel.width>0&&panel.yMin<y+h&&panel.yMax>y?panel.xMin-8:w-18,medals.width>0&&medals.yMin<y+h&&medals.yMax>y?medals.xMin-8:w-18);
            Place(header,0,0,w,narrow?154:116);
            headerTitle.text=BattleHeading(game.Combat.State);
            Place(headerTitle.rectTransform,18,8,Mathf.Min(textWidth,End(8,28)-18),28);headerTitle.fontSize=narrow?16:21;
            Place(headerSubtitle.rectTransform,18,38,Mathf.Min(w-36,End(38,22)-18),22);headerSubtitle.fontSize=narrow?12:13;
            Place(timerText.rectTransform,18,63,narrow?w-36:90,22);
            Place(meterText.rectTransform,narrow?18:112,narrow?87:63,narrow?w-36:Mathf.Max(100,Mathf.Min(w-240,End(63,24)-112)),24);meterText.fontSize=narrow?13:15;
            Place(actionText.rectTransform,18,narrow?112:88,Mathf.Min(w-36,End(narrow?112:88,22)-18),22);
            // Gear and content dock fold out beneath each other in one column, the size of the skill icons.
            var settings=header.GetComponentsInChildren<Button>().Single(b=>b.name=="설정·안내");Right((RectTransform)settings.transform,12,8,dockIcon,dockIcon);CompactGear(settings,dockIcon);
            Right((RectTransform)contentDock.transform,12,14+dockIcon,dockIcon,dockIcon);contentDock.Reflow(dockIcon,6);ReflowEventButton();
            var menu=header.GetComponentsInChildren<Button>().Single(b=>b.name=="관찰 메뉴");Place((RectTransform)menu.transform,medals.width>0?medals.xMin-52:w-64-dockIcon,8,44,44);
            // Boss status owns the top centre. On phones it gets the first clear row under the header,
            // with symmetric width beside the map; neither the map nor a moved graph may push it downward.
            float bossY=w>=1200?8:narrow?158:120,bossWidth=Mathf.Min(430,w-36);
            if(panel.width>0&&panel.yMin<bossY+Mathf.Max(66,battleBossHeight)&&panel.yMax>bossY)
                bossWidth=Mathf.Min(bossWidth,Mathf.Max(180,2*(panel.xMin-8-w*.5f)));
            Place(bossHud,(w-bossWidth)*.5f,bossY,bossWidth,66);
            bossTitle.alignment=TextAnchor.UpperCenter;bossActionLabel.alignment=TextAnchor.UpperCenter;ReflowBossText();
            PlaceTrainingHud(w,narrow,medals,game.TrainingGroundRun?default:panel);
            PlaceIncomingHud();
            PlaceRiftActivityHud(w);
            UpdateBattleBrief();
        }
        // Keep the captioned shortcuts immediately left of Settings, with the observation menu to their left.
        Rect ReflowBattleMedallions(float w,Rect panel)
        {
            if(powerSavingButton==null)return default;
            powerSavingButton.interactable=game.CanEnterIdle;
            var pair=new[]{(powerSavingButton,powerSavingCaption),(escapeButton,escapeCaption)}.Where(p=>p.Item1.gameObject.activeSelf).ToArray();
            if(pair.Length==0)return default;
            const float side=40,cell=84,top=8;
            float left=w-20-dockIcon-pair.Length*cell,bottom=top+side;
            for(int i=0;i<pair.Length;i++)
            {
                var (button,label)=pair[i];label.fontSize=12;float x=left+i*cell;
                Place((RectTransform)button.transform,x+(cell-side)*.5f,top,side,side);
                float height=Mathf.Ceil(TextHeight(label,label.text,cell-4))+2;
                Place(label.rectTransform,x+2,top+side,cell-4,height);bottom=Mathf.Max(bottom,top+side+height);
            }
            return new Rect(left,top,pair.Length*cell,bottom-top);
        }
        // The hero stands at the centre of the camera frame; in page units from the top-left of the safe area.
        Vector2 HeroOnPage()
        {
            var safe=UiSafeArea.Current;var frame=UiSafeArea.Frame;float k=root.rect.width/Mathf.Max(1,safe.width);
            return new Vector2((frame.center.x-safe.xMin)*k,(safe.yMax-frame.center.y)*k);
        }
        void UpdateBattleBrief()
        {
            if(actionText==null)return;
            actionText.text=fullBattleAction;
            while(actionText.preferredHeight>actionText.rectTransform.rect.height+1&&actionText.text.Length>8)
                actionText.text=actionText.text.Substring(0,actionText.text.Length-(actionText.text.EndsWith("…")?2:1))+"…";
        }
        void ClearBattleLayout()
        {
            ClearGearRecommendations();
            trainingHud=null;
            activityHud=null;
            if(battleWorld!=null){battleWorld.gameObject.SetActive(false);Destroy(battleWorld.gameObject);}
            battleWorld=null;battleSize=Vector2.zero;dockIcon=44;BattleViewport=UiSafeArea.FrameNormalized;
        }
    }
}
