using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    // Shared, display-only bottle sprites. Keep source PNGs, alpha and GUIDs intact.
    public static class PotionArt
    {
        static readonly Dictionary<string,Sprite> bottles=new Dictionary<string,Sprite>();
        static readonly Dictionary<string,Sprite> casualHudBottles=new Dictionary<string,Sprite>();
        public static string IconId(PotionDefinition def)=>def.Crafted?"elixir-"+def.gemId+"-"+def.grade:def.effect=="hp"?"potion-hp":def.effect=="resource"?"potion-mp":"potion-utility";
        public static Sprite Bottle(string id,bool casualHud=false)
        {
            bool useCasual=casualHud&&(id=="potion-hp"||id=="potion-mp"||id=="potion-utility");
            var cache=useCasual?casualHudBottles:bottles;
            if(cache.TryGetValue(id,out var sprite)&&sprite!=null)return sprite;
            if(id.StartsWith("elixir-",System.StringComparison.Ordinal))return bottles[id]=Resources.Load<Sprite>("Art/JewelerWindow/"+id);
            var source=Resources.Load<Sprite>("Art/GlobalHUD/Casual/"+id);if(source==null)return null;
            // Both roles use the approved whole canvas; HUD layout alone applies its 66% slot width.
            sprite=Sprite.Create(source.texture,new Rect(0,0,source.texture.width,source.texture.height),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);sprite.name=id;
            cache[id]=sprite;return sprite;
        }
        public static Image Draw(Transform parent,PotionDefinition def,float x,float y,float size)
        {
            var rect=UiLayout.Rect("Potion art",parent);UiLayout.Place(rect,x,y,size,size);
            // Image.preserveAspect uses the pivot to place its quad inside the square.
            rect.pivot=new Vector2(.5f,.5f);rect.anchoredPosition+=new Vector2(size*.5f,-size*.5f);
            var image=rect.gameObject.AddComponent<Image>();image.sprite=Bottle(IconId(def));image.preserveAspect=true;image.raycastTarget=false;
            if(def.duration>0&&!def.Crafted)
            {
                var glyph=UiLayout.Rect("Potion effect",parent);UiLayout.Place(glyph,x,y+size-14,14,14);
                var icon=glyph.gameObject.AddComponent<Image>();icon.sprite=GlobalHudView.LoadArt(def.icon);icon.raycastTarget=false;
            }
            return image;
        }
    }
}
