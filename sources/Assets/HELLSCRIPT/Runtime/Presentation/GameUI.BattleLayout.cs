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
        string fullBattleAction="";
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
            AddContentDock(58,44);
            AddBossHud(header);bossHud.GetComponent<Image>().color=Color.clear;
            AddRiftMinimap(run);RefreshHud();ReflowBattleHud();
        }
        void ReflowBattleHud()
        {
            if(Page!="battle"||battleWorld==null||game.Combat==null)return;
            Vector2 size=root.rect.size;Rect safe=UiSafeArea.Current;
            if(size==battleSize&&safe==battleSafe)return;
            battleSize=size;battleSafe=safe;
            // The selected aspect frame is the camera region. The HUD only observes it.
            BattleViewport=UiSafeArea.FrameNormalized;
            battleWorld.anchorMin=BattleViewport.min;battleWorld.anchorMax=BattleViewport.max;battleWorld.offsetMin=battleWorld.offsetMax=Vector2.zero;
            float w=size.x;bool narrow=w<360;float textWidth=w-(narrow?132:170);
            Place(header,0,0,w,narrow?154:116);
            headerTitle.text=BattleHeading(game.Combat.State);
            Place(headerTitle.rectTransform,18,8,textWidth,28);headerTitle.fontSize=narrow?16:21;
            Place(headerSubtitle.rectTransform,18,38,w-36,22);headerSubtitle.fontSize=narrow?12:13;
            Place(timerText.rectTransform,18,63,narrow?w-36:90,22);
            Place(meterText.rectTransform,narrow?18:112,narrow?87:63,narrow?w-36:Mathf.Max(100,w-240),24);meterText.fontSize=narrow?13:15;
            Place(actionText.rectTransform,18,narrow?112:88,w-36,22);
            var settings=header.GetComponentsInChildren<Button>().Single(b=>b.name=="설정·안내");Place((RectTransform)settings.transform,w-56,8,44,44);
            var menu=header.GetComponentsInChildren<Button>().Single(b=>b.name=="관찰 메뉴");Place((RectTransform)menu.transform,w-108,8,44,44);
            float bossWidth=Mathf.Min(430,narrow?w-36:w*.46f);Place(bossHud,(w-bossWidth)*.5f,narrow?146:116,bossWidth,66);
            Place(bossTitle.rectTransform,0,0,bossWidth,25);Place(bossActionLabel.rectTransform,0,27,bossWidth,30);
            Place((RectTransform)bossHealthFill.transform.parent,0,62,bossWidth,4);ReflowBossText();
            var mini=root.Find("Rift minimap") as RectTransform;
            if(mini!=null)
            {
                mini.gameObject.SetActive(size.x>size.y&&w>=720);mini.GetComponent<Image>().color=new Color(.02f,.03f,.04f,.45f);
                // Sits left of the content dock column (44 wide, 12 from the right edge).
                Place(mini,w-236,64,168,220);Place(mini.Find("Map viewport") as RectTransform,4,4,160,160);
                Place(chestCountText.rectTransform,0,169,168,20);Place(fieldStatusText.rectTransform,0,190,168,30);
            }
            UpdateBattleBrief();
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
