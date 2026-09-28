using System;
using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    // The shared shell owns the bottom dock, safe area and input. Only the greeting scrolls.
    public static class NpcDialogueWindow
    {
        public static ContentWindowView Open(Transform parent,NpcProfile profile,Func<float> readingScale,Action<int> service,Action closed)
        {
            return ContentWindowView.Open(parent,profile.name,EquipmentViewSource.Catalog,readingScale,view=>
            {
                float reading=readingScale?.Invoke()??1,w=view.Width,h=view.Frame.rect.height;
                bool narrow=w<600,merchant=profile.Station==TownStation.Merchant||profile.Station==TownStation.Gambler;
                float portraitHeight=h-(narrow?60:16),portraitWidth=Mathf.Min(narrow?88:120,portraitHeight*2/3);
                float textX=portraitWidth+24,textW=w-textX-12,bodyY=42,bodyH=h-bodyY-50;
                UiLayout.Place(view.Title.rectTransform,textX,2,textW-44,40);
                view.Navigation.gameObject.SetActive(!narrow);
                if(!narrow)
                {
                    float nameWidth=view.Title.preferredWidth+14;
                    UiLayout.Place(view.Title.rectTransform,textX,2,nameWidth,40);
                    UiLayout.Place(view.Navigation,textX+nameWidth,9,textW-nameWidth-44,26);
                    UiLayout.Text(view.Navigation,"NPC role",Loc.T(profile.Role),0,0,view.Navigation.rect.width,26,Mathf.RoundToInt(UiTheme.Caption*reading),UiTheme.Muted);
                }
                UiLayout.Place(view.Scroll.viewport,textX,bodyY,textW-10,bodyH);
                view.Scroll.scrollSensitivity=18*reading;
                var texture=Resources.Load<Texture2D>(profile.portrait);
                var art=UiLayout.Rect("NPC portrait "+profile.id,view.Frame);
                float imageW=texture!=null?Mathf.Min(portraitWidth,portraitHeight*texture.width/texture.height):portraitWidth;
                float imageH=texture!=null?imageW*texture.height/texture.width:portraitHeight;
                UiLayout.Place(art,12+(portraitWidth-imageW)/2,h-(narrow?52:8)-imageH,imageW,imageH);
                var image=art.gameObject.AddComponent<RawImage>();image.texture=texture;image.raycastTarget=false;
                if(texture==null)UiLayout.Text(view.Frame,"Missing portrait",Loc.T("초상화를 불러올 수 없습니다."),12,42,portraitWidth,48,UiTheme.Body,UiTheme.Muted);
                var greeting=UiLayout.Text(view.Body,"NPC greeting",Loc.T(profile.greeting),0,0,textW-10,48,Mathf.RoundToInt(UiTheme.Body*reading),UiTheme.Text);
                greeting.lineSpacing=1.16f;greeting.supportRichText=false;
                Canvas.ForceUpdateCanvases();
                var textLayout=greeting.gameObject.AddComponent<LayoutElement>();
                // Keep the final wrapped line reachable at every dynamic-font pixel scale.
                textLayout.preferredHeight=textLayout.minHeight=Mathf.Ceil(greeting.preferredHeight+greeting.fontSize*greeting.lineSpacing)+4;
                var rail=UiLayout.Rect("Dialogue scroll indicator",view.Frame);UiLayout.Place(rail,w-16,bodyY,3,bodyH);
                rail.gameObject.AddComponent<Image>().color=UiTheme.Panel;
                var thumb=UiLayout.Rect("Scroll thumb",rail);UiLayout.Stretch(thumb);thumb.gameObject.AddComponent<Image>().color=UiTheme.Gold;
                var bar=rail.gameObject.AddComponent<Scrollbar>();bar.handleRect=thumb;bar.targetGraphic=thumb.GetComponent<Image>();bar.direction=Scrollbar.Direction.BottomToTop;
                view.Scroll.verticalScrollbar=bar;view.Scroll.verticalScrollbarVisibility=ScrollRect.ScrollbarVisibility.AutoHide;
                float actionsX=narrow?12:textX,actionsW=w-actionsX-12;
                UiLayout.Place(view.Actions,actionsX,h-48,actionsW,40);
                int count=profile.Station.HasValue?(merchant?3:2):1;float actionW=(actionsW-(count-1)*UiTheme.Gap)/count;
                if(profile.Station.HasValue)
                {
                    view.Button(view.Actions,merchant?"구매":TownLayout.Station(profile.Station.Value).action,0,0,actionW,40,()=>{view.Close();service?.Invoke(0);},true).name="npc-service";
                    if(merchant)view.Button(view.Actions,"판매",actionW+UiTheme.Gap,0,actionW,40,()=>{view.Close();service?.Invoke(1);}).name="npc-sell";
                }
                view.Button(view.Actions,"대화를 마친다",(count-1)*(actionW+UiTheme.Gap),0,actionW,40,view.Close).name="npc-goodbye";
            },closed,maximumSize:new Vector2(760,220),bottomDock:true);
        }
    }
}
