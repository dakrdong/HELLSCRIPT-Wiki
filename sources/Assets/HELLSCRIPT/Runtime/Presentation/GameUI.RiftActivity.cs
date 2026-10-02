using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    // A read-only HUD on the battle canvas. Folding and position are independent of the DPS panel.
    public sealed partial class GameUI
    {
        RectTransform activityHud,activityBody;
        RiftActivityPanel activityPanel;
        Text activityTitle;
        Button activityFold;
        DpsHudDrag activityDrag;
        bool activityFolded;
        float activityShownTime=-1;
        public RectTransform ActivityHud=>activityHud;

        void BuildRiftActivityHud(RunState run)
        {
            if(run.training>=0||run.tutorial)return;
            activityHud=Box("Rift activity panel",root,UiTheme.Tint(UiTheme.PanelHex,.86f));activityHud.GetComponent<Image>().raycastTarget=false;
            activityTitle=Label(activityHud,"활동 시간",16,pale);activityTitle.name="activity-drag-handle";activityTitle.raycastTarget=true;
            activityDrag=activityTitle.gameObject.AddComponent<DpsHudDrag>();
            activityDrag.Bind(activityHud,game.ActivityPosition,()=>ShowToast(Loc.T("그래프 위치를 기기에 저장하지 못했습니다. 다시 끌어 놓아 저장을 재시도해 주세요.")),()=>BattleHudTopOnPage-8);
            activityFold=UiIconButton.CreateGlyph(activityHud,"rift-activity-fold","chevron",0,0,30,30,ToggleActivityFold,flip:!activityFolded);
            activityPanel=RiftActivityPanel.Create(activityHud);activityBody=(RectTransform)activityPanel.transform;
            activityShownTime=-1;
        }
        void PlaceRiftActivityHud(float w)
        {
            if(activityHud==null)return;
            bool wide=w>=720;float pw=wide?Mathf.Min(320,w*.3f):w-24,ph=activityFolded?42:216,top=124;
            if(!wide&&trainingHud!=null)top=-trainingHud.anchoredPosition.y+trainingHud.rect.height+8;
            if(wide&&bossHud!=null&&bossHud.gameObject.activeSelf&&bossHud.anchoredPosition.x<pw+12)
                top=Mathf.Max(top,-bossHud.anchoredPosition.y+bossHud.rect.height+8);
            Place(activityHud,12,top,pw,ph);Place((RectTransform)activityFold.transform,8,6,30,30);
            Place(activityTitle.rectTransform,46,6,pw-54,30);
            activityBody.gameObject.SetActive(!activityFolded);
            Place(activityBody,10,42,pw-20,ph-48);activityPanel.Layout(pw-20);
            activityDrag.Restore();
        }
        void ToggleActivityFold()
        {
            activityFolded=!activityFolded;activityShownTime=-1;
            var mark=activityFold.transform.Find("Icon");if(mark!=null)mark.localRotation=Quaternion.Euler(0,0,activityFolded?0:180);
            battleSize=Vector2.zero;ReflowBattleHud();RefreshRiftActivityHud(game.Combat.State);
        }
        void RefreshRiftActivityHud(RunState run)
        {
            if(activityHud==null||activityFolded||activityShownTime>=0&&run.time-activityShownTime<.15f)return;
            activityShownTime=run.time;activityPanel.Show(run.statistics?.feedback);
        }
    }
}
