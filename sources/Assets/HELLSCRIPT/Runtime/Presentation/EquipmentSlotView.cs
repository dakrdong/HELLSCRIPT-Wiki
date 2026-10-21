using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    // Rendering only: click, drag and transaction ownership stay with the containing content.
    public sealed class EquipmentSlotView:MonoBehaviour
    {
        public string ItemId {get;private set;}
        public UiSkin Skin {get;private set;}
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
        public static RectTransform Create(Transform parent,Item item,float x,float y,float size,Font font=null,bool selected=false,bool equipped=false,bool showSelection=false,string label=null,UiSkin skin=null)
        {
            var r=UiLayout.Rect(item==null?"Empty equipment slot":"Equipment "+item.id,parent);UiLayout.Place(r,x,y,size,size);
            Paint(r,item,font,selected,equipped,showSelection,skin);if(!string.IsNullOrEmpty(label))Caption(r,label,font,skin);return r;
        }
        public static StorageSurface Paint(RectTransform root,Item item,Font font=null,bool selected=false,bool equipped=false,bool showSelection=false,UiSkin skin=null,CellRecipe recipe=null)
        {
            if(recipe!=null)return PaintMock(root,item,font,selected,showSelection,skin,recipe);
            var view=root.GetComponent<EquipmentSlotView>();if(view==null)view=root.gameObject.AddComponent<EquipmentSlotView>();view.ItemId=item?.id;view.Skin=skin;
            font=font??skin?.BodyFont;
            var frame=root.GetComponent<StorageSurface>();if(frame==null)frame=root.gameObject.AddComponent<StorageSurface>();float size=root.rect.width;
            frame.Paint(item==null?UiTheme.PanelHex:selected?UiTheme.BorderHex:UiTheme.HoverHex,item==null?UiTheme.PanelHex:UiTheme.RaisedHex,selected?UiTheme.GoldHex:item==null?UiTheme.BorderHex:EquipmentGradePalette.Hex[Storage.Grade(item)]);frame.inset=true;frame.SetSkin(skin,selected?skin?.Gold:item==null?skin?.Edge:StorageSurface.Hex(EquipmentGradePalette.Hex[Storage.Grade(item)]),radius:8,shadow:2);
            if(item==null)return frame;
            Icon(root,item,3,3,size-6,skin:skin);
            var level=UiLayout.Text(root,"Equipment level",item.enhancement>0?"+"+item.enhancement+" · "+item.level:"Lv."+item.level,2,equipped?size-26:size-14,size-5,12,8,skin?.Navy??UiTheme.Text,font);level.alignment=TextAnchor.MiddleRight;
            if(item.locked)Glyph(root,"Equipment locked","lock",size-15,2,12,skin?.Navy??UiTheme.Gold);
            if(equipped)Glyph(root,"Equipment worn","check",2,2,11,UiTheme.Success);
            if(item.slot==0)UiLayout.Text(root,"Equipment hands",EquipmentSlots.TwoHanded(item)?"2H":EquipmentSlots.Offhand(item)?"OFF":"1H",16,2,24,12,7,skin?.Navy??UiTheme.Gold,font);
            if(showSelection)
            {
                var check=UiLayout.Rect("Equipment selection",root);UiLayout.Place(check,size-17,2,15,15);var s=check.gameObject.AddComponent<StorageSurface>();s.Paint(UiTheme.PanelHex,UiTheme.PanelHex,UiTheme.GoldHex);s.SetSkin(skin,radius:3,shadow:0);s.raycastTarget=false;
                if(selected)Glyph(check,"Selected","check",1,1,13,skin?.Navy??UiTheme.Gold);
            }
            return frame;
        }

        // The slot recipe of the casual canvases: every inset scales with the cell (art 86% at 7%/5%, level .17, chips .14/.2/.22).
        static StorageSurface PaintMock(RectTransform root,Item item,Font font,bool selected,bool showSelection,UiSkin skin,CellRecipe recipe)
        {
            var view=root.GetComponent<EquipmentSlotView>();if(view==null)view=root.gameObject.AddComponent<EquipmentSlotView>();view.ItemId=item?.id;view.Skin=skin;
            font=font??skin?.BodyFont;var navy=skin?.Navy??UiTheme.Text;
            var frame=root.GetComponent<StorageSurface>();if(frame==null)frame=root.gameObject.AddComponent<StorageSurface>();float size=root.rect.width;
            frame.shadowOutside=true;
            if(item==null){frame.SetSkin(skin,UiTheme.MockVacantRing,recipe.Radius,2,0,skin?.White,skin?.White);return frame;}
            if(selected)frame.SetSkin(skin,navy,recipe.Radius,3,0,UiTheme.MockHoverTop,skin?.Gold);
            else frame.SetSkin(skin,recipe.Ring(item),recipe.Radius,2,2,skin?.Cream,skin?.Cream);
            Icon(root,item,size*.07f,size*.05f,size*.86f,skin:skin);
            int levelSize=Mathf.RoundToInt(size*.17f);float lineHeight=levelSize*1.3f;
            var level=UiLayout.Text(root,"Equipment level",item.enhancement>0?"+"+item.enhancement+" · "+item.level:"Lv."+item.level,0,size-2-lineHeight,size-4,lineHeight,levelSize,navy,font);level.alignment=TextAnchor.LowerRight;
            level.gameObject.AddComponent<SkillTreeD4TextShadow>().Apply(recipe.Halo,1);
            if(item.slot==0)
            {
                int handSize=Mathf.RoundToInt(size*.14f);string hands=EquipmentSlots.TwoHanded(item)?"2H":EquipmentSlots.Offhand(item)?"OFF":"1H";
                var chip=UiLayout.Rect("Equipment hands",root);UiLayout.Place(chip,3,3,handSize*(hands=="OFF"?2.3f:1.6f)+8,handSize*1.25f);
                var face=chip.gameObject.AddComponent<StorageSurface>();face.SetSkin(skin,Color.clear,5,0,0,new Color(1,1,1,.9f),new Color(1,1,1,.9f));face.raycastTarget=false;
                var label=UiLayout.Text(chip,"Equipment hands text",hands,0,0,chip.rect.width,chip.rect.height,handSize,navy,font);label.alignment=TextAnchor.MiddleCenter;
            }
            float badge=size*.22f;
            if(showSelection)
            {
                var check=UiLayout.Rect("Equipment selection",root);UiLayout.Place(check,size-3-badge,3,badge,badge);var s=check.gameObject.AddComponent<StorageSurface>();s.raycastTarget=false;
                s.SetSkin(skin,selected?navy:skin?.Edge,5,2,0,selected?skin?.Gold:skin?.White,selected?skin?.Gold:skin?.White);
                if(selected)Glyph(check,"Selected","check",badge*.11f,badge*.11f,badge*.78f,navy);
            }
            else if(item.locked)
            {
                float lock_=size*.2f;var pin=UiLayout.Rect("Equipment locked",root);UiLayout.Place(pin,size-3-lock_,3,lock_,lock_);var s=pin.gameObject.AddComponent<StorageSurface>();s.raycastTarget=false;
                s.SetSkin(skin,navy,5,1.5f,0,skin?.Gold,skin?.Gold);Glyph(pin,"lock","lock",lock_*.14f,lock_*.14f,lock_*.72f,navy);
            }
            return frame;
        }
        public static void Caption(RectTransform root,string label,Font font=null,UiSkin skin=null)
        {
            float size=root.rect.width;var t=UiLayout.Text(root,"Equipment position",label,2,size-13,size-4,12,8,skin?.Navy??UiTheme.Gold,font??skin?.BodyFont);t.alignment=TextAnchor.MiddleCenter;
            t.resizeTextForBestFit=true;t.resizeTextMinSize=6;t.resizeTextMaxSize=8;
        }
        public static RectTransform Icon(Transform parent,Item item,float x,float y,float size,float opacity=1,UiSkin skin=null)
        {
            var r=UiLayout.Rect("Equipment art",parent);UiLayout.Place(r,x,y,size,size);
            if(item==null)return r;
            var dedicated=EquipmentArt.ForItem(item,skin?.EquipmentArt);
            if(dedicated!=null)
            {var image=r.gameObject.AddComponent<RawImage>();image.texture=dedicated;image.raycastTarget=false;image.color=new Color(1,1,1,opacity);}
            else
            {var glyph=r.gameObject.AddComponent<StorageGlyph>();glyph.symbol=EquipmentSlots.Kind(item).ToString().ToLowerInvariant();var c=UiTheme.Gold;c.a=opacity;glyph.color=c;glyph.raycastTarget=false;}
            return r;
        }
        public static void Glyph(Transform parent,string name,string symbol,float x,float y,float size,Color color)
        {var r=UiLayout.Rect(name,parent);UiLayout.Place(r,x,y,size,size);var glyph=r.gameObject.AddComponent<StorageGlyph>();glyph.symbol=symbol;glyph.color=color;glyph.raycastTarget=false;}
    }
    // Per-window inputs of the shared mockup slot recipe: corner radius, ring colour by item, and the white text halo.
    public sealed class CellRecipe
    {
        public readonly float Radius;readonly System.Func<Item,Color> ring;
        public readonly D4Shadow[] Halo;
        public CellRecipe(float radius,System.Func<Item,Color> ring)
        {
            Radius=radius;this.ring=ring;var rings=new System.Collections.Generic.List<D4Shadow>();
            for(int i=0;i<8;i++)rings.Add(new D4Shadow{x=Mathf.Cos(i*Mathf.PI/4),y=Mathf.Sin(i*Mathf.PI/4),color="SteelFFFFFFA1000"});
            Halo=rings.ToArray();
        }
        public Color Ring(Item item)=>ring(item);
    }
    [RequireComponent(typeof(CanvasRenderer))]
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
