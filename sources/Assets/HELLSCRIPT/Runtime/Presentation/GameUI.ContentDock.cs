using System;
using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    public sealed partial class GameUI
    {
        bool contentDockOpen;
        ContentDockView contentDock;
        // Content shortcuts fold out beneath the settings gear on the play screens (plaza and battle).
        // Character opens the owned-equipment screen and remembers the play screen it came from.
        void AddContentDock(float top,float size)
        {
            const float gap=6;
            var dock=Rect("Content dock",header);Right(dock,12,top,size,size);
            var view=dock.gameObject.AddComponent<ContentDockView>();contentDock=view;
            var items=Rect("Shortcuts",dock);Place(items,0,0,size,0);items.gameObject.AddComponent<RectMask2D>();
            int count=0;
            void Add(string name,string sprite,Action action)
            {
                var b=Button(items,name,action,sprite==null?new Color(.07f,.085f,.1f,.92f):Color.clear);Place((RectTransform)b.transform,0,count++*(size+gap),size,size);
                if(sprite!=null)b.targetGraphic=Emblem(b,sprite);
                else{var label=b.GetComponentInChildren<Text>();label.fontSize=Mathf.RoundToInt(size*.3f);label.color=gold;}
            }
            Add("캐릭터","menu-character",ShowPlayInventory);
            Add("사냥 칙령","menu-hunt-edict",ShowEdictEditor);
            Add("룬 보드","menu-rune-board",ShowRunes);
            Add("창고","menu-storage",ShowStorage);
            var toggle=Button(dock,"콘텐츠 메뉴",()=>{view.Set(!view.Open);contentDockOpen=view.Open;},Color.clear);toggle.targetGraphic=Emblem(toggle,"menu-toggle");
            view.Configure(items,(RectTransform)toggle.transform,size,count*(size+gap));view.Snap(contentDockOpen);
        }
        Image Emblem(Button button,string sprite)
        {
            var border=button.GetComponent<UIRectBorder>();if(border!=null)border.enabled=false;
            button.GetComponentInChildren<Text>().text="";
            var art=Rect("Emblem",button.transform);Stretch(art);var image=art.gameObject.AddComponent<Image>();
            image.sprite=Resources.Load<Sprite>("Art/GlobalHUD/"+sprite);image.preserveAspect=true;return image;
        }
    }
    // Opening: the arrow slides down uncovering the shortcuts, then turns over to point up.
    // Folding runs the same two steps in reverse. Real time, so a paused battle still animates.
    public sealed class ContentDockView:MonoBehaviour
    {
        public const float Seconds=.22f;
        RectTransform items,toggle;float size,reach,slide,turn;
        public bool Open {get;private set;}
        public RectTransform Items=>items;
        public RectTransform Toggle=>toggle;
        public float Reach=>reach;
        public bool Settled=>Open?slide>=1&&turn>=1:slide<=0&&turn<=0;
        public void Configure(RectTransform shortcuts,RectTransform arrow,float buttonSize,float travel)
        {
            items=shortcuts;toggle=arrow;size=buttonSize;reach=travel;
            toggle.anchorMin=toggle.anchorMax=new Vector2(0,1);toggle.pivot=new Vector2(.5f,.5f);toggle.sizeDelta=new Vector2(size,size);
        }
        public void Set(bool open)=>Open=open;
        public void Reflow(float buttonSize,float gap)
        {
            if(Mathf.Approximately(size,buttonSize)&&Mathf.Approximately(reach,items.childCount*(buttonSize+gap)))return;
            size=buttonSize;reach=items.childCount*(size+gap);
            for(int i=0;i<items.childCount;i++)UiLayout.Place((RectTransform)items.GetChild(i),0,i*(size+gap),size,size);
            toggle.sizeDelta=new Vector2(size,size);Apply();
        }
        public void Snap(bool open){Open=open;slide=turn=open?1:0;Apply();}
        void Update()
        {
            if(Settled)return;float step=Time.unscaledDeltaTime/Seconds;
            if(Open){if(slide<1)slide=Mathf.Min(1,slide+step);else turn=Mathf.Min(1,turn+step);}
            else{if(turn>0)turn=Mathf.Max(0,turn-step);else slide=Mathf.Max(0,slide-step);}
            Apply();
        }
        void Apply()
        {
            items.sizeDelta=new Vector2(size,slide*reach);
            toggle.anchoredPosition=new Vector2(size*.5f,-(slide*reach+size*.5f));
            toggle.localRotation=Quaternion.Euler(0,0,180*turn);
        }
    }
}
