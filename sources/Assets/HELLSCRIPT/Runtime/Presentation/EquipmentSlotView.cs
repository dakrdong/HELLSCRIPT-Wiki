using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    // Rendering only: click, drag and transaction ownership stay with the containing content.
    public sealed class EquipmentSlotView:MonoBehaviour
    {
        public string ItemId {get;private set;}
        EquipmentDropHighlight dropHighlight;
        public bool DropHighlighted=>dropHighlight!=null&&dropHighlight.gameObject.activeSelf;
        public void SetDropHighlight(bool enabled,bool hovered=false)
        {
            if(enabled&&dropHighlight==null)
            {
                var r=UiLayout.Rect("Eligible equipment drop",transform);UiLayout.Stretch(r);
                dropHighlight=r.gameObject.AddComponent<EquipmentDropHighlight>();
            }
            if(dropHighlight==null)return;
            dropHighlight.gameObject.SetActive(enabled);dropHighlight.hovered=hovered;dropHighlight.transform.SetAsLastSibling();
        }
        public static RectTransform Create(Transform parent,Item item,float x,float y,float size,Font font=null,bool selected=false,bool equipped=false,bool showSelection=false,string label=null)
        {
            var r=UiLayout.Rect(item==null?"Empty equipment slot":"Equipment "+item.id,parent);UiLayout.Place(r,x,y,size,size);
            Paint(r,item,font,selected,equipped,showSelection);if(!string.IsNullOrEmpty(label))Caption(r,label,font);return r;
        }
        public static StorageSurface Paint(RectTransform root,Item item,Font font=null,bool selected=false,bool equipped=false,bool showSelection=false)
        {
            var view=root.GetComponent<EquipmentSlotView>();if(view==null)view=root.gameObject.AddComponent<EquipmentSlotView>();view.ItemId=item?.id;
            var frame=root.GetComponent<StorageSurface>();if(frame==null)frame=root.gameObject.AddComponent<StorageSurface>();float size=root.rect.width;
            frame.Paint(item==null?"151811":selected?"443b26":"27271c",item==null?"191d14":"1d2117",selected?UiTheme.GoldHex:item==null?"414332":EquipmentGradePalette.Hex[Storage.Grade(item)]);frame.inset=true;
            if(item==null)return frame;
            Icon(root,item,3,3,size-6);
            var level=UiLayout.Text(root,"Equipment level",item.enhancement>0?"+"+item.enhancement+" · "+item.level:"Lv."+item.level,2,equipped?size-26:size-14,size-5,12,8,UiTheme.Text,font);level.alignment=TextAnchor.MiddleRight;
            if(item.locked)Glyph(root,"Equipment locked","lock",size-15,2,12,UiTheme.Gold);
            if(equipped)Glyph(root,"Equipment worn","check",2,2,11,UiTheme.Success);
            if(item.slot==0)UiLayout.Text(root,"Equipment hands",EquipmentSlots.TwoHanded(item)?"2H":EquipmentSlots.Offhand(item)?"OFF":"1H",16,2,24,12,7,UiTheme.Gold,font);
            if(showSelection)
            {
                var check=UiLayout.Rect("Equipment selection",root);UiLayout.Place(check,size-17,2,15,15);var s=check.gameObject.AddComponent<StorageSurface>();s.Paint("151811","151811",UiTheme.GoldHex);s.raycastTarget=false;
                if(selected)Glyph(check,"Selected","check",1,1,13,UiTheme.Gold);
            }
            return frame;
        }
        public static void Caption(RectTransform root,string label,Font font=null)
        {
            float size=root.rect.width;var t=UiLayout.Text(root,"Equipment position",label,2,size-13,size-4,12,8,UiTheme.Gold,font);t.alignment=TextAnchor.MiddleCenter;
            t.resizeTextForBestFit=true;t.resizeTextMinSize=6;t.resizeTextMaxSize=8;
        }
        public static RectTransform Icon(Transform parent,Item item,float x,float y,float size,float opacity=1)
        {
            var r=UiLayout.Rect("Equipment art",parent);UiLayout.Place(r,x,y,size,size);
            if(item==null)return r;
            var dedicated=EquipmentArt.ForItem(item);
            if(dedicated!=null)
            {var image=r.gameObject.AddComponent<RawImage>();image.texture=dedicated;image.raycastTarget=false;image.color=new Color(1,1,1,opacity);}
            else if(ItemCatalog.Base(item).legacyIndex>=24)
            {var glyph=r.gameObject.AddComponent<StorageGlyph>();glyph.symbol=EquipmentSlots.Kind(item).ToString().ToLowerInvariant();var c=UiTheme.Gold;c.a=opacity;glyph.color=c;glyph.raycastTarget=false;}
            else
            {var image=r.gameObject.AddComponent<RawImage>();image.texture=Resources.Load<Texture2D>("Art/EquipmentAtlas");int n=ItemCatalog.AtlasIndex(item);image.uvRect=new Rect(n%6/6f,1-(n/6+1)/4f,1/6f,1/4f);image.raycastTarget=false;image.color=new Color(1,1,1,opacity);}
            return r;
        }
        public static void Glyph(Transform parent,string name,string symbol,float x,float y,float size,Color color)
        {var r=UiLayout.Rect(name,parent);UiLayout.Place(r,x,y,size,size);var glyph=r.gameObject.AddComponent<StorageGlyph>();glyph.symbol=symbol;glyph.color=color;glyph.raycastTarget=false;}
    }
    public sealed class EquipmentDropHighlight:MaskableGraphic
    {
        public bool hovered;
        protected override void Awake(){base.Awake();raycastTarget=false;}
        void Update(){SetVerticesDirty();}
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();var r=GetPixelAdjustedRect();var tint=UiTheme.Drop;
            tint.a=hovered?1:.83f+.12f*Mathf.Sin(Time.unscaledTime*4);
            void Quad(float x,float y,float w,float h,float alpha)
            {
                int n=mesh.currentVertCount;var c=tint;c.a*=alpha;
                mesh.AddVert(new Vector3(x,y),c,Vector2.zero);mesh.AddVert(new Vector3(x,y+h),c,Vector2.zero);
                mesh.AddVert(new Vector3(x+w,y+h),c,Vector2.zero);mesh.AddVert(new Vector3(x+w,y),c,Vector2.zero);
                mesh.AddTriangle(n,n+1,n+2);mesh.AddTriangle(n,n+2,n+3);
            }
            float edge=hovered?3:2;
            Quad(r.xMin,r.yMin,r.width,edge,1);Quad(r.xMin,r.yMax-edge,r.width,edge,1);
            Quad(r.xMin,r.yMin,edge,r.height,1);Quad(r.xMax-edge,r.yMin,edge,r.height,1);
            Quad(r.xMin+edge,r.yMin+edge,r.width-edge*2,4,.18f);Quad(r.xMin+edge,r.yMax-edge-4,r.width-edge*2,4,.18f);
            Quad(r.xMin+edge,r.yMin+edge,4,r.height-edge*2,.18f);Quad(r.xMax-edge-4,r.yMin+edge,4,r.height-edge*2,.18f);
        }
    }
}
