using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    public sealed partial class ItemDetailView
    {
        int TypeSize(int value)=>Mathf.RoundToInt(value*Mathf.Clamp(textScale,.5f,2.5f));
        float Line(Transform parent,ItemTooltipLine line,ItemTooltipLine expanded,float x,float y,float width,int pointSize,string prefix="")
        {
            int fs=TypeSize(pointSize);
            var t=UiLayout.Text(parent,line.key,prefix+line.text,x,y,width,1000,fs,StorageSurface.Hex(line.color),font);
            t.lineSpacing=1.12f;
            if(line.key.StartsWith("main:")&&!line.key.EndsWith("-label"))
            {
                // Keep the primary number and its unit together in three-column mobile comparisons.
                string original=t.text;t.text=line.text.Split(' ')[0];float natural=t.preferredWidth;t.text=original;
                if(natural>width)t.fontSize=Mathf.Max(TypeSize(UiTheme.Body),Mathf.FloorToInt(fs*width/natural));
                fs=t.fontSize;
            }
            float measured=line.text==expanded.text?t.preferredHeight:ItemRangeText.Bind(t,prefix+line.text,prefix+expanded.text).PreferredHeight;
            float h=Mathf.Max(fs+4,measured+5);UiLayout.Place(t.rectTransform,x,y,width,h);return h;
        }
        void Divider(Transform parent,float width,ref float y)
        {
            y+=8;var r=UiLayout.Rect("Engraved divider",parent);UiLayout.Place(r,12,y,width-24,9);
            var trim=r.gameObject.AddComponent<ItemCardTrim>();trim.divider=true;trim.color=StorageSurface.Hex(UiTheme.ItemRuleHex);y+=15;
        }
        void Section(Transform parent,string label,float width,ref float y)
        {
            Divider(parent,width,ref y);
            var line=new ItemTooltipLine("section-"+label,Loc.T(label),UiTheme.GoldHex);
            y+=Line(parent,line,line,12,y,width-24,UiTheme.Caption)+5;
        }
        float DrawCard(RectTransform body,float width,ItemTooltipLine[] hidden,ItemTooltipLine[] shown)
        {
            float pad=12,y=14,inner=Mathf.Max(24,width-pad*2);bool narrow=width<230;
            var name=hidden.FirstOrDefault(l=>l.key=="name");
            Color rarity=StorageSurface.Hex(name?.color??UiTheme.GoldHex);
            var wash=UiLayout.Rect("Rarity wash",body);UiLayout.Place(wash,2,2,width-4,110);
            var gradient=wash.gameObject.AddComponent<StorageSurface>();gradient.top=new Color(rarity.r*.20f,rarity.g*.20f,rarity.b*.20f,.8f);gradient.bottom=Color.clear;gradient.raycastTarget=false;
            float art=icon&&Item!=null?(narrow?44:68):0;
            if(art>0)
            {
                float artX=narrow?(width-art)/2:width-pad-art;
                var mount=UiLayout.Rect("Relic mount",body);UiLayout.Place(mount,artX,y,art,art);
                var backing=mount.gameObject.AddComponent<StorageSurface>();backing.Paint(UiTheme.ItemInsetHex,UiTheme.ItemBottomHex,UiTheme.ItemRuleHex);backing.raycastTarget=false;
                EquipmentSlotView.Icon(mount,Item,1,1,art-2);
                if(narrow)y+=art+9;
            }
            float titleY=y,titleWidth=!narrow&&art>0?inner-art-10:inner;
            if(name!=null)y+=Line(body,name,name,pad,y,titleWidth,narrow?UiTheme.Heading:UiTheme.ItemName)+2;
            var type=hidden.FirstOrDefault(l=>l.key=="type");
            if(type!=null)y+=Line(body,type,type,pad,y,titleWidth,UiTheme.Caption);
            if(!narrow&&art>0)y=Mathf.Max(y,titleY+art);
            foreach(var metadata in hidden.Where(l=>l.key=="level"||l.key=="hands"))
                y+=Line(body,metadata,metadata,pad,y+3,inner,UiTheme.Caption)+3;
            Divider(body,width,ref y);
            string previous="identity";
            for(int n=0;n<hidden.Length;n++)
            {
                var line=hidden[n];string key=line.key;
                if(key=="name"||key=="type"||key=="level"||key=="hands")continue;
                string group=key.StartsWith("main:")||key.StartsWith("implicit:")?"base":
                    key.StartsWith("affix:")||key.StartsWith("gem:")?"affixes":
                    key=="special"||key=="aspect-salvage"?"power":key.StartsWith("lost-")?"lost":
                    key.StartsWith("sheet-")||key=="set-gain"?"sheet":"details";
                if(group!=previous)
                {
                    if(group=="affixes")Section(body,"추가 옵션",width,ref y);
                    else if(group=="power")Section(body,"특수 효과",width,ref y);
                    else if(group=="details")Section(body,"장비 정보",width,ref y);
                    else if(group=="lost"||group=="sheet")Divider(body,width,ref y);
                    previous=group;
                }
                if(key.StartsWith("main:")&&!string.IsNullOrEmpty(line.value))
                {
                    var label=new ItemTooltipLine(key+"-label",line.label,UiTheme.MutedHex);
                    y+=Line(body,label,label,pad,y,inner,UiTheme.Caption);
                    var value=new ItemTooltipLine(key,line.value,line.color);var expanded=new ItemTooltipLine(key,shown[n].value,line.color);
                    y+=Line(body,value,expanded,pad,y,inner,UiTheme.ItemValue)+3;continue;
                }
                bool bullet=group=="affixes"||key.StartsWith("implicit:");
                if(bullet)
                {
                    var mark=UiLayout.Rect("Affix diamond",body);UiLayout.Place(mark,pad,y+TypeSize(UiTheme.Body)*.4f,5,5);
                    var dot=mark.gameObject.AddComponent<Image>();dot.color=group=="affixes"?UiTheme.Gold:UiTheme.Muted;dot.raycastTarget=false;mark.localRotation=Quaternion.Euler(0,0,45);
                }
                float start=y;bool power=key=="special";
                var block=power?UiLayout.Rect("Power inset",body):null;
                if(block!=null){var fill=block.gameObject.AddComponent<StorageSurface>();fill.Paint(UiTheme.ItemTopHex,UiTheme.ItemInsetHex,UiTheme.ItemRuleHex);fill.raycastTarget=false;y+=8;}
                float inset=bullet||power?12:0;
                var emblem=power?EquipmentArt.SetEmblem(Item):null;float powerY=y;
                if(emblem!=null)
                {
                    var emblemRect=UiLayout.Rect("Set emblem",body);UiLayout.Place(emblemRect,pad+inset,y,28,28);
                    var image=emblemRect.gameObject.AddComponent<RawImage>();image.texture=emblem;image.raycastTarget=false;inset+=36;
                }
                y+=Line(body,line,shown[n],pad+inset,y,inner-inset-(power?8:0),key.EndsWith("heading")?UiTheme.Body:group=="details"?UiTheme.Caption:UiTheme.Body);
                if(emblem!=null)y=Mathf.Max(y,powerY+28);
                if(block!=null){y+=8;UiLayout.Place(block,pad,start,inner,y-start);}
                y+=group=="affixes"?5:2;
            }
            return y+12;
        }
    }
    // Lightweight vector metalwork. No raster copies, layout participation or input interception.
    public sealed class ItemCardTrim:MaskableGraphic
    {
        public bool divider;
        protected override void Awake(){base.Awake();raycastTarget=false;}
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();var r=GetPixelAdjustedRect();
            void Quad(float x,float y,float w,float h,Color tint)
            {
                if(w<=0||h<=0)return;int n=mesh.currentVertCount;
                mesh.AddVert(new Vector3(x,y),tint,Vector2.zero);mesh.AddVert(new Vector3(x,y+h),tint,Vector2.zero);
                mesh.AddVert(new Vector3(x+w,y+h),tint,Vector2.zero);mesh.AddVert(new Vector3(x+w,y),tint,Vector2.zero);
                mesh.AddTriangle(n,n+1,n+2);mesh.AddTriangle(n,n+2,n+3);
            }
            if(divider)
            {
                float x=r.center.x,y=r.center.y;Quad(r.xMin,y,x-r.xMin-6,1,color);Quad(x+6,y,r.xMax-x-6,1,color);
                int n=mesh.currentVertCount;
                mesh.AddVert(new Vector3(x-3,y),color,Vector2.zero);mesh.AddVert(new Vector3(x,y+3),color,Vector2.zero);
                mesh.AddVert(new Vector3(x+3,y),color,Vector2.zero);mesh.AddVert(new Vector3(x,y-3),color,Vector2.zero);mesh.AddTriangle(n,n+1,n+2);mesh.AddTriangle(n,n+2,n+3);return;
            }
            var faint=color;faint.a=.30f;
            Quad(r.xMin+3,r.yMin+3,r.width-6,1,faint);Quad(r.xMin+3,r.yMax-4,r.width-6,1,faint);
            Quad(r.xMin+3,r.yMin+3,1,r.height-6,faint);Quad(r.xMax-4,r.yMin+3,1,r.height-6,faint);
            foreach(float x in new[]{r.xMin+1,r.xMax-14})foreach(float y in new[]{r.yMin+1,r.yMax-3})Quad(x,y,13,2,color);
            foreach(float x in new[]{r.xMin+1,r.xMax-3})foreach(float y in new[]{r.yMin+1,r.yMax-14})Quad(x,y,2,13,color);
        }
    }
}
