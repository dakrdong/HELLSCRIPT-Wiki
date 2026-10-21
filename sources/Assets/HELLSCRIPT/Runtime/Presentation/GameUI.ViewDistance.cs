using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    public sealed partial class GameUI
    {
        SettingsStepSlider viewDistanceSlider;Text viewDistanceValue,viewDistanceMessage;Button viewDistanceRetry;
        void BuildViewDistanceControl(Transform body)
        {
            CommonNote(body,"가시거리",24,gold);
            CommonNote(body,"50%에서는 가까이, 150%에서는 멀리 보입니다. 기본값은 100%이며 5%씩 조절합니다.",19);
            var row=Rect("View distance choices",body);row.gameObject.AddComponent<LayoutElement>().preferredHeight=124;
            viewDistanceValue=Label(row,"",24,gold,TextAnchor.MiddleCenter);Span(viewDistanceValue.rectTransform,60,0,60,36);
            var minus=Button(row,"−",()=>SetViewDistance(game.ViewDistance.Percent-5));Place((RectTransform)minus.transform,0,0,48,44);minus.name="settings-distance-down";
            var plus=Button(row,"+",()=>SetViewDistance(game.ViewDistance.Percent+5));Right((RectTransform)plus.transform,0,0,48,44);plus.name="settings-distance-up";
            var area=Box("settings-distance-slider",row,Color.clear);Span(area,0,48,0,68);viewDistanceSlider=area.gameObject.AddComponent<SettingsStepSlider>();viewDistanceSlider.minValue=0;viewDistanceSlider.maxValue=20;viewDistanceSlider.wholeNumbers=true;
            var track=Box("Track",area,new Color(.09f,.12f,.15f));track.anchorMin=new Vector2(0,.5f);track.anchorMax=new Vector2(1,.5f);track.sizeDelta=new Vector2(-36,8);
            var fill=Box("Fill",track,gold);Stretch(fill);fill.GetComponent<Image>().raycastTarget=false;viewDistanceSlider.fillRect=fill;
            var travel=Rect("Handle area",area);travel.anchorMin=new Vector2(0,.5f);travel.anchorMax=new Vector2(1,.5f);travel.sizeDelta=new Vector2(-36,0);
            var handle=Box("Handle",travel,pale);handle.anchorMin=handle.anchorMax=new Vector2(0,.5f);handle.sizeDelta=new Vector2(32,44);viewDistanceSlider.handleRect=handle;viewDistanceSlider.targetGraphic=handle.GetComponent<Image>();
            viewDistanceSlider.onValueChanged.AddListener(step=>SetViewDistance(50+Mathf.RoundToInt(step)*5));
            viewDistanceMessage=CommonNote(body,"",18,muted);viewDistanceRetry=BigButton(body,"저장 재시도",()=>SetViewDistance(game.ViewDistance.Percent));
            CommonNote(body,"가시거리 설정은 이 기기에 저장됩니다.",18,muted);
        }
        void SetViewDistance(int percent){game.ApplyViewDistance(percent);RefreshViewDistanceControl();}
        void RefreshViewDistanceControl()
        {
            if(viewDistanceSlider==null)return;var distance=game.ViewDistance;viewDistanceSlider.SetValueWithoutNotify((distance.Percent-50)/5);viewDistanceValue.text=distance.Percent+"%";
            viewDistanceMessage.text=Loc.T(distance.Message);viewDistanceMessage.gameObject.SetActive(distance.Message!="");viewDistanceRetry.gameObject.SetActive(distance.CanRetrySave);
        }
    }
}
