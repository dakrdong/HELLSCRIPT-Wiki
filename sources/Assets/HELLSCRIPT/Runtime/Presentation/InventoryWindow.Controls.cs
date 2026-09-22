using System;
using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    public sealed partial class InventoryWindow
    {
        static Color ColorOf(string hex)=>StorageSurface.Hex(hex);
        RectTransform Rect(string name,Transform parent){var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);return r;}
        static void Clear(Transform parent){foreach(Transform child in parent){child.gameObject.SetActive(false);Destroy(child.gameObject);}}
        static void Stretch(RectTransform r){r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;}
        static void Place(RectTransform r,float x,float y,float w,float h){r.anchorMin=r.anchorMax=new Vector2(0,1);r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);}
        RectTransform Panel(Transform parent,string name,string top,string bottom,string edge="00000000")
        {var r=Rect(name,parent);r.gameObject.AddComponent<StorageSurface>().Paint(top,bottom,edge);return r;}
        Text Txt(Transform parent,string value,float x,float y,float w,float h,int size=11,Color? color=null,TextAnchor align=TextAnchor.MiddleLeft)
        {
            var r=Rect("Text",parent);Place(r,x,y,w,h);var t=r.gameObject.AddComponent<Text>();t.font=font;t.text=Loc.T(value);t.color=color??pale;t.fontSize=Mathf.RoundToInt(size*Mathf.Clamp(textScale,.8f,1.5f));
            t.alignment=align;t.supportRichText=false;t.horizontalOverflow=HorizontalWrapMode.Wrap;t.verticalOverflow=VerticalWrapMode.Truncate;t.raycastTarget=false;return t;
        }
        Button Btn(Transform parent,string label,float x,float y,float w,float h,Action click,bool primary=false,int size=11)
        {
            var r=Panel(parent,label,primary?"71352a":"343428",primary?"482219":"25261c",primary?"b18b5a":"786342");Place(r,x,y,w,h);r.GetComponent<StorageSurface>().inset=true;
            var b=r.gameObject.AddComponent<Button>();b.targetGraphic=r.GetComponent<Image>();var colors=b.colors;colors.highlightedColor=new Color(1.2f,1.13f,1.03f);colors.pressedColor=new Color(.75f,.72f,.64f);b.colors=colors;
            var t=Txt(r,label,4,0,w-8,h,size,pale,TextAnchor.MiddleCenter);t.resizeTextForBestFit=true;t.resizeTextMinSize=8;t.resizeTextMaxSize=t.fontSize;b.onClick.AddListener(()=>click());return b;
        }
        void Glyph(Transform parent,string symbol,float x,float y,float size,Color color)
        {var r=Rect(symbol,parent);Place(r,x,y,size,size);var glyph=r.gameObject.AddComponent<StorageGlyph>();glyph.symbol=symbol;glyph.color=color;glyph.raycastTarget=false;}
        void Check(Transform parent,float x,float y,float size,bool selected)
        {var r=Panel(parent,"Selection checkbox",selected?"c4a774":"13180f",selected?"c4a774":"13180f","c0a475");Place(r,x,y,size,size);r.GetComponent<Image>().raycastTarget=false;if(selected)Glyph(r,"check",1,1,size-2,ink);}
        void FilterButton(Transform parent,string label,float x,float y,float w,string kind)
        {
            var button=Btn(parent,label,x,y,w,28,()=>ShowFilter(kind),false,10);button.name="inventory-"+kind;
            Place(button.GetComponentInChildren<Text>().rectTransform,8,0,w-33,28);Glyph(button.transform,"chevron",w-23,6,16,gold);
        }
        void CloseFilter()
        {if(filterMenu!=null){filterMenu.gameObject.SetActive(false);Destroy(filterMenu.gameObject);}filterMenu=null;filterKind=null;}
        void ShowFilter(string kind)
        {
            CloseFilter();filterKind=kind;var anchor=(RectTransform)Find("inventory-"+kind).transform;
            var corners=new Vector3[4];anchor.GetWorldCorners(corners);var bottom=frame.InverseTransformPoint(corners[0]);
            float w=anchor.rect.width,h=kind=="grade"?232:200;
            filterMenu=Rect("Inventory filter layer",overlays);Stretch(filterMenu);
            var shade=Btn(filterMenu,"",0,0,width,height,CloseFilter);shade.name="inventory-filter-backdrop";shade.GetComponent<StorageSurface>().Paint("00000000","00000000");
            var menu=Panel(filterMenu,"Inventory filter list","343023","202419","a68b59");
            Place(menu,Mathf.Clamp(bottom.x,8,width-w-8),Mathf.Clamp(frame.rect.yMax-bottom.y+4,8,height-h-8),w,h);
            Txt(menu,kind=="grade"?"등급":"정렬",10,0,w-48,30,12,gold);
            Btn(menu,"×",w-31,3,26,25,CloseFilter,false,17).name="inventory-filter-close";
            for(int n=0;n<5;n++)
            {
                int index=n,bit=1<<n;bool chosen=kind=="grade"?(grades&bit)!=0:order==Orders[n];
                var row=Btn(menu,kind=="grade"?GradeNames[n]:OrderLabels[n],8,32+n*32,w-16,29,()=>
                {
                    if(kind=="grade"){if(grades==bit)return;grades^=bit;Repaint(true);RefreshGradeFilter();}
                    else {order=Orders[index];CloseFilter();Repaint(true);}
                },chosen,11);row.name="inventory-"+kind+"-option-"+n;
                Place(row.GetComponentInChildren<Text>().rectTransform,29,0,w-53,29);row.GetComponentInChildren<Text>().alignment=TextAnchor.MiddleLeft;
                if(kind=="grade"){Check(row.transform,7,7,15,chosen);row.interactable=grades!=bit;}
                else if(chosen)Glyph(row.transform,"check",7,6,16,gold);
            }
            if(kind=="grade")Btn(menu,"모든 등급",8,197,w-16,27,()=>{grades=31;Repaint(true);RefreshGradeFilter();},false,10).name="inventory-grade-all";
        }
        void RefreshGradeFilter()
        {
            // Keep the list and its input targets alive while the bag results change underneath it.
            for(int n=0;n<GradeNames.Length;n++)
            {
                int bit=1<<n;bool chosen=(grades&bit)!=0;var row=Find("inventory-grade-option-"+n);row.interactable=grades!=bit;
                row.GetComponent<StorageSurface>().Paint(chosen?"71352a":"343428",chosen?"482219":"25261c",chosen?"b18b5a":"786342");
                var check=(RectTransform)row.transform.Find("Selection checkbox");check.GetComponent<StorageSurface>().Paint(chosen?"c4a774":"13180f",chosen?"c4a774":"13180f","c0a475");
                if(check.childCount>0)check.GetChild(0).gameObject.SetActive(chosen);else if(chosen)Glyph(check,"check",1,1,13,ink);
            }
        }
        void DrawIcon(Transform parent,Item item,float x,float y,float size)
        {
            if(ItemCatalog.Base(item).legacyIndex>=24){Glyph(parent,EquipmentSlots.Kind(item).ToString().ToLowerInvariant(),x+size*.12f,y+size*.12f,size*.76f,gold);return;}
            var r=Rect("Equipment art",parent);Place(r,x,y,size,size);var image=r.gameObject.AddComponent<RawImage>();image.texture=atlas;int index=ItemCatalog.AtlasIndex(item);
            image.uvRect=new Rect(index%6/6f,1-(index/6+1)/4f,1/6f,1/4f);image.raycastTarget=false;
        }
        ScrollRect Scroll(Transform parent,string name,float x,float y,float w,float h,out RectTransform content)
        {
            var root=Panel(parent,name,"12160f","12160f");Place(root,x,y,w,h);
            var viewport=Rect("Viewport",root);Place(viewport,0,0,w-6,h);viewport.gameObject.AddComponent<RectMask2D>();var fill=viewport.gameObject.AddComponent<Image>();fill.color=Color.clear;
            content=Rect("Content",viewport);content.anchorMin=new Vector2(0,1);content.anchorMax=Vector2.one;content.pivot=new Vector2(.5f,1);content.sizeDelta=Vector2.zero;
            var scroll=root.gameObject.AddComponent<ScrollRect>();scroll.horizontal=false;scroll.vertical=true;scroll.viewport=viewport;scroll.content=content;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=26;
            var track=Panel(root,"Scrollbar track","2b2d20","2b2d20");Place(track,w-4,0,3,h);
            var thumb=Panel(track,"Scrollbar thumb","907b51","907b51");Stretch(thumb);
            var bar=track.gameObject.AddComponent<Scrollbar>();bar.handleRect=thumb;bar.targetGraphic=thumb.GetComponent<Image>();bar.direction=Scrollbar.Direction.BottomToTop;
            scroll.verticalScrollbar=bar;scroll.verticalScrollbarVisibility=ScrollRect.ScrollbarVisibility.AutoHide;return scroll;
        }
        Text Paragraph(Transform parent,string value,float x,ref float y,float w,int size=11,Color? color=null)
        {
            var t=Txt(parent,value,x,y,w,1000,size,color);float h=Mathf.Max(size+5,t.preferredHeight+6);Place(t.rectTransform,x,y,w,h);y+=h;return t;
        }
        void Rule(Transform parent,float y,float w){var line=Panel(parent,"Rule","5c4e33","5c4e33");Place(line,10,y,w-20,1);line.GetComponent<Image>().raycastTarget=false;}
    }
}
