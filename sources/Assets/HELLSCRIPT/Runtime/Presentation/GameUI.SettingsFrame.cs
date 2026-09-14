using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    public sealed partial class GameUI
    {
        RectTransform aspectMask;
        void UpdateAspectMask()
        {
            if(aspectMask==null)
            {
                aspectMask=Rect("Aspect frame",transform);var canvas=aspectMask.gameObject.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=110;
                foreach(string name in new[]{"Left","Right","Bottom","Top"}){var bar=Box(name,aspectMask,Color.black);bar.GetComponent<Image>().raycastTarget=false;}
            }
            var f=UiSafeArea.FrameNormalized;
            var rects=new[]{new Rect(0,0,f.xMin,1),new Rect(f.xMax,0,1-f.xMax,1),new Rect(f.xMin,0,f.width,f.yMin),new Rect(f.xMin,f.yMax,f.width,1-f.yMax)};
            for(int i=0;i<4;i++)
            {var r=(RectTransform)aspectMask.GetChild(i);r.anchorMin=rects[i].min;r.anchorMax=rects[i].max;r.offsetMin=r.offsetMax=Vector2.zero;}
        }
        void MakeSettingsIcon(Button button)
        {
            button.GetComponentInChildren<Text>().text="";
            var icon=Rect("Settings gear",button.transform);Stretch(icon);icon.offsetMin=Vector2.one*12;icon.offsetMax=-Vector2.one*12;
            var graphic=icon.gameObject.AddComponent<SettingsGearGraphic>();graphic.color=pale;graphic.raycastTarget=false;
        }
    }
}
