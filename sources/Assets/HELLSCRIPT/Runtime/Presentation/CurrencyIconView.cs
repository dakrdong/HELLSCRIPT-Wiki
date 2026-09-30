using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    // Shared artwork lookup only; account balances and transactions belong to callers.
    public static class CurrencyIconView
    {
        static readonly string[] CoreNames={"weapon","head","chest","hands","feet","belt","amulet","ring"};
        public static string CoreIcon(int slot)=>slot>=0&&slot<CoreNames.Length?"core-"+CoreNames[slot]:null;
        public static Sprite ForIcon(string icon)=>Resources.Load<Sprite>(icon switch
        {
            "coin"=>"Art/Attendance/reward-gold",
            "vault"=>"Art/ItemIcons/materials",
            _=>"Art/GlobalHUD/currency-"+icon
        });
        public static Sprite ForResource(RiftResourceDrop drop)=>drop==null?null:drop.kind switch
        {
            RiftResourceKind.Gem=>JewelerArt.ForGem(drop.gemId,drop.tier),
            RiftResourceKind.Gold=>ForIcon("coin"),
            RiftResourceKind.Material=>ForIcon("vault"),
            RiftResourceKind.EnhancementStone=>ForIcon("enhancement-stone"),
            RiftResourceKind.Core=>ForIcon(CoreIcon(drop.coreIndex)),
            _=>null
        };
        public static Image Resource(Transform parent,RiftResourceDrop drop,float x,float y,float size)
            =>Draw(parent,"Resource artwork",ForResource(drop),x,y,size);
        public static Image Create(Transform parent,string icon,float x,float y,float size)
            =>Draw(parent,icon=="abyssal-coin"?"Abyssal Coin":"currency-icon-"+icon,ForIcon(icon),x,y,size);
        static Image Draw(Transform parent,string name,Sprite sprite,float x,float y,float size)
        {
            if(sprite==null)return null;
            var r=UiLayout.Rect(name,parent);
            r.anchorMin=r.anchorMax=new Vector2(0,1);r.pivot=new Vector2(.5f,.5f);
            r.anchoredPosition=new Vector2(x+size/2,-y-size/2);r.sizeDelta=Vector2.one*size;
            var image=r.gameObject.AddComponent<Image>();image.sprite=sprite;image.preserveAspect=true;image.raycastTarget=false;
            return image;
        }
    }
}
