using Hellscript.Runes;
using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    // The catalogue owns type identity; board geometry still owns shape and rotation.
    public static class RuneItemArt
    {
        public static Sprite ForType(int type)=>type<0||type>=RuneV13Catalog.TypeIds.Length?null:
            Resources.Load<Sprite>("Art/ItemIcons/rune-"+RuneV13Catalog.TypeIds[type]);
        public static Image Create(Transform parent,OwnedRune rune,float x,float y,float size)
        {
            var r=UiLayout.Rect("Rune artwork",parent);UiLayout.Place(r,x,y,size,size);
            var image=r.gameObject.AddComponent<Image>();image.sprite=ForType(rune.type);image.preserveAspect=true;image.raycastTarget=false;
            // Keep the actual polyhex visible without substituting a generic shape for an owned rune.
            var shape=UiLayout.Rect("Rune shape",r);UiLayout.Place(shape,0,size*.57f,size*.43f,size*.43f);
            var graphic=shape.gameObject.AddComponent<RuneBoardGraphic>();graphic.icon=true;graphic.iconPadding=2;graphic.raycastTarget=false;
            graphic.piece=new RunePiece(rune.id,rune.grade,RuneMasteryCatalog.ShapeById(rune.shapeId),rune.type);
            graphic.rotation=RuneV13Art.StorageRotation(rune.shapeId);
            return image;
        }
        public static void AddTypeBadge(RuneBoardGraphic shape)
        {
            if(shape?.piece==null)return;
            var r=UiLayout.Rect("Rune type artwork",shape.transform);
            r.anchorMin=new Vector2(.68f,.68f);r.anchorMax=Vector2.one;r.pivot=Vector2.one*.5f;r.offsetMin=r.offsetMax=Vector2.zero;
            var image=r.gameObject.AddComponent<Image>();image.sprite=ForType(shape.piece.Type);image.preserveAspect=true;image.raycastTarget=false;
        }
    }
}
