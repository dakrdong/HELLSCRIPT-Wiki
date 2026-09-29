using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    // Shared artwork lookup only; account balances and transactions belong to callers.
    public static class CurrencyIconView
    {
        static readonly string[] CoreNames={"weapon","head","chest","hands","feet","belt","amulet","ring"};
        public static string CoreIcon(int slot)=>slot>=0&&slot<CoreNames.Length?"core-"+CoreNames[slot]:null;
        public static Image Create(Transform parent,string icon,float x,float y,float size)
        {
            var sprite=Resources.Load<Sprite>("Art/GlobalHUD/currency-"+icon);
            if(sprite==null)return null;
            var r=UiLayout.Rect(icon=="abyssal-coin"?"Abyssal Coin":"currency-icon-"+icon,parent);
            r.anchorMin=r.anchorMax=new Vector2(0,1);r.pivot=new Vector2(.5f,.5f);
            r.anchoredPosition=new Vector2(x+size/2,-y-size/2);r.sizeDelta=Vector2.one*size;
            var image=r.gameObject.AddComponent<Image>();image.sprite=sprite;image.preserveAspect=true;image.raycastTarget=false;
            return image;
        }
    }
}
