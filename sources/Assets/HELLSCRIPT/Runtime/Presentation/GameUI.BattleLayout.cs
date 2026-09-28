using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    public sealed partial class GameUI
    {
        RectTransform battleWorld;
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
            AddContentDock(58,44);AddEventButton();
            AddBossHud(header);bossHud.GetComponent<Image>().color=Color.clear;
            AddRiftMinimap(run);BuildLiveJournal();RefreshHud();ReflowBattleHud();
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
            // Header lines that share rows with a landscape map panel stop short of it.
            float End(float y,float h)=>panel.width>0&&panel.yMin<y+h&&panel.yMax>y?panel.xMin-8:w-18;
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
            float bossWidth=Mathf.Min(430,narrow?w-36:w*.46f),bossX=(w-bossWidth)*.5f,bossY=w<600?(narrow?212:176):116;
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
