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
            AddRiftMinimap(run);if(game.TrainingGroundRun)BuildTrainingHud();BuildLiveJournal();RefreshHud();ReflowBattleHud();
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
            var mini=riftMinimapPanel;var panel=default(Rect);float map=0;if(mini!=null)panel=ReflowRiftMinimap(mini,narrow,float.MaxValue,out map);
            var medals=ReflowBattleMedallions(w,panel);PlaceTrainingHud(w,narrow,medals);
            // Header lines that share rows with a landscape map panel or the medallions stop short of them.
            float End(float y,float h)=>Mathf.Min(panel.width>0&&panel.yMin<y+h&&panel.yMax>y?panel.xMin-8:w-18,medals.width>0&&medals.yMin<y+h&&medals.yMax>y?medals.xMin-8:w-18);
            Place(header,0,0,w,narrow?154:116);
            headerTitle.text=BattleHeading(game.Combat.State);
            Place(headerTitle.rectTransform,18,8,Mathf.Min(textWidth,End(8,28)-18),28);headerTitle.fontSize=narrow?16:21;
            Place(headerSubtitle.rectTransform,18,38,Mathf.Min(w-36,End(38,22)-18),22);headerSubtitle.fontSize=narrow?12:13;
            Place(timerText.rectTransform,18,63,narrow?w-36:90,22);
            Place(meterText.rectTransform,narrow?18:112,narrow?87:63,narrow?w-36:Mathf.Max(100,Mathf.Min(w-240,End(63,24)-112)),24);meterText.fontSize=narrow?13:15;
            Place(actionText.rectTransform,18,narrow?112:88,Mathf.Min(w-36,End(narrow?112:88,22)-18),22);
            var settings=header.GetComponentsInChildren<Button>().Single(b=>b.name=="설정·안내");Place((RectTransform)settings.transform,w-56,8,44,44);
            var menu=header.GetComponentsInChildren<Button>().Single(b=>b.name=="관찰 메뉴");Place((RectTransform)menu.transform,w-108,8,44,44);
            // On narrow fields the event button has its own row above the boss status.
            float bossWidth=Mathf.Min(430,narrow?w-36:w*.46f),bossX=(w-bossWidth)*.5f,bossY=Mathf.Max(w<720?(narrow?218:176):116,medals.height>0?medals.yMax+6:0,TrainingHudBottom(w));
            // The boss status stays clear of the map panel: beside it when 240 remain right of the left
            // column (the events and adventure guide buttons end at 132), otherwise below it.
            bool below=panel.width>0&&bossX+bossWidth>panel.xMin-8&&bossY<panel.yMax+8;
            if(below&&panel.xMin-148>=240){bossWidth=Mathf.Min(bossWidth,panel.xMin-148);bossX=Mathf.Max(140,panel.xMin-8-bossWidth);below=false;}
            Place(bossHud,bossX,bossY,bossWidth,66);
            Place(bossTitle.rectTransform,0,0,bossWidth,25);Place(bossActionLabel.rectTransform,0,27,bossWidth,30);
            Place((RectTransform)bossHealthFill.transform.parent,0,62,bossWidth,4);ReflowBossText();
            // In a boss fight a map of 40 or more that ends the panel above the status's own row keeps the
            // status in place (landscape below 600 wide).
            float fit=map-(panel.yMax+8-bossY);
            if(below&&boss&&fit>=40)panel=ReflowRiftMinimap(mini,narrow,fit,out map);
            else if(below)
            {
                // Otherwise the status goes below the panel and stays clear above the hero, so the map shrinks
                // to fit; only when less than 40 would remain does the panel leave it out until the fight ends.
                float over=panel.yMax+8+Mathf.Max(bossHud.rect.height,battleBossHeight)-(HeroOnPage().y-HeroClearance);
                if(boss&&over>0&&map>0)panel=ReflowRiftMinimap(mini,narrow,map-over,out map);
                bossY=Mathf.Max(bossY,panel.yMax+8);bossHud.anchoredPosition=new Vector2(bossX,-bossY);
            }
            UpdateBattleBrief();
        }
        // The power saving and escape medallions, side by side with a caption under each. A wide field has them
        // at the top centre, the band ending above the timer and kill row at 63. A narrow one has them in the row
        // under the header, between the left column (the events and adventure guide buttons end at 132) and the
        // map panel, where the gap closes and captions wrap to fit.
        Rect ReflowBattleMedallions(float w,Rect panel)
        {
            if(powerSavingButton==null)return default;
            powerSavingButton.interactable=game.CanEnterIdle;
            var pair=new[]{(powerSavingButton,powerSavingCaption),(escapeButton,escapeCaption)}.Where(p=>p.Item1.gameObject.activeSelf).ToArray();
            if(pair.Length==0)return default;
            foreach(var p in pair)p.Item2.fontSize=12;
            float natural=pair.Max(p=>Mathf.Ceil(TextWidth(p.Item2,p.Item2.text))+4),line=pair.Max(p=>Mathf.Ceil(TextHeight(p.Item2,p.Item2.text,natural))+2);
            bool row=w<720;float top=row?118:4,left=row?140:0,side=row?52:Mathf.Clamp(63-top-line,32,44);
            float right=!row?w:panel.width>0&&panel.yMin<top+side+48&&panel.yMax>top?panel.xMin-8:w-68;
            // Each caption spans its medallion and half of each neighbouring gap, less 2 on either side.
            float gap=Mathf.Max(30,natural-side+4);if(row)gap=Mathf.Clamp((right-left+4)/pair.Length-side,12,gap);
            float x=(left+right-pair.Length*side-(pair.Length-1)*gap)*.5f,cw=side+gap-4,bottom=top+side;
            var area=UnityEngine.Rect.MinMaxRect(x-(gap-4)*.5f,top,x+pair.Length*(side+gap)-gap+(gap-4)*.5f,top+side);
            foreach(var (button,label) in pair)
            {
                Place((RectTransform)button.transform,x,top,side,side);
                float ch=Mathf.Ceil(TextHeight(label,label.text,cw))+2;Place(label.rectTransform,x-(gap-4)*.5f,top+side,cw,ch);
                bottom=Mathf.Max(bottom,top+side+ch);x+=side+gap;
            }
            area.yMax=bottom;return area;
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
            if(battleWorld!=null){battleWorld.gameObject.SetActive(false);Destroy(battleWorld.gameObject);}
            battleWorld=null;battleSize=Vector2.zero;BattleViewport=UiSafeArea.FrameNormalized;
        }
    }
}
