using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    public sealed partial class GameUI
    {
        SettingsStepSlider viewDistanceSlider;Text viewDistanceValue,viewDistanceMessage;Button viewDistanceRetry;
        void BuildViewDistanceControl(Transform body)
        {
            var card=CasualSettingsStyle.Card(body,"View distance card");CasualSettingsStyle.Header(card,"가시거리","section-view-distance",out viewDistanceValue);
            CommonNote(card,"50%에서는 가까이, 150%에서는 멀리 보입니다. 기본값은 100%이며 5%씩 조절합니다.",19);
            var row=Rect("View distance choices",card);row.gameObject.AddComponent<LayoutElement>().preferredHeight=110;
            var minus=Button(row,"",()=>SetViewDistance(game.ViewDistance.Percent-5));minus.name="settings-distance-down";RoundDistanceButton(minus,"control-minus");Place((RectTransform)minus.transform,0,14,48,68);
            var plus=Button(row,"",()=>SetViewDistance(game.ViewDistance.Percent+5));plus.name="settings-distance-up";RoundDistanceButton(plus,"control-plus");Right((RectTransform)plus.transform,0,14,48,68);
            var area=Box("settings-distance-slider",row,Color.clear);Span(area,56,14,56,68);viewDistanceSlider=area.gameObject.AddComponent<SettingsStepSlider>();viewDistanceSlider.minValue=0;viewDistanceSlider.maxValue=20;viewDistanceSlider.wholeNumbers=true;
            CasualSettingsStyle.Slider(viewDistanceSlider,UiSkin.Casual.Gold,UiSkin.Casual.GoldShade,true);
            // Start on the stored value and ignore no-op callbacks (the Editor fires one on layout), so a fresh slider at step 0 can never save 50%.
            viewDistanceSlider.SetValueWithoutNotify((game.ViewDistance.Percent-50)/5);
            viewDistanceSlider.onValueChanged.AddListener(step=>{int percent=50+Mathf.RoundToInt(step)*5;if(percent!=game.ViewDistance.Percent)SetViewDistance(percent);});
            foreach(var tick in new[]{(0f,"50%"),(.5f,"100%"),(1f,"150%")})
            {
                var text=Label(row,tick.Item2,18,tick.Item1==.5f?UiSkin.Casual.OrangeInk:UiSkin.Casual.Muted,TextAnchor.MiddleCenter);text.font=UiSkin.Casual.DisplayFont;
                text.rectTransform.anchorMin=text.rectTransform.anchorMax=new Vector2(tick.Item1,0);text.rectTransform.pivot=new Vector2(tick.Item1,0);text.rectTransform.sizeDelta=new Vector2(64,24);text.rectTransform.anchoredPosition=new Vector2(tick.Item1==0?68:tick.Item1==1?-68:0,0);
            }
            viewDistanceMessage=CasualSettingsStyle.Notice(card,"",true);viewDistanceRetry=SettingsButton(viewDistanceMessage.transform.parent,"저장 재시도",()=>SetViewDistance(game.ViewDistance.Percent),UiButtonRole.Destructive);
            SavedNote(card,"가시거리 설정은 이 기기에 저장됩니다.");
        }
        void RoundDistanceButton(Button button,string id)
        {
            CasualSettingsStyle.Button(button,UiButtonRole.Icon);button.GetComponent<Image>().color=Color.clear;
            var art=Rect(id,button.transform);Stretch(art);var image=art.gameObject.AddComponent<Image>();image.sprite=Resources.Load<Sprite>("Art/GlobalHUD/Casual/"+id);image.preserveAspect=true;image.raycastTarget=false;
        }
        void SetViewDistance(int percent){game.ApplyViewDistance(percent);RefreshViewDistanceControl();}
        void RefreshViewDistanceControl()
        {
            if(viewDistanceSlider==null)return;var distance=game.ViewDistance;viewDistanceSlider.SetValueWithoutNotify((distance.Percent-50)/5);CasualSettingsStyle.SetText(viewDistanceValue,distance.Percent+"%");
            CasualSettingsStyle.SetText(viewDistanceMessage,Loc.T(distance.Message));CasualSettingsStyle.Active(viewDistanceMessage.transform.parent.gameObject,distance.Message!="");CasualSettingsStyle.Active(viewDistanceRetry.gameObject,distance.CanRetrySave);
        }
    }
}
