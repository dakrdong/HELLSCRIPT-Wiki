using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    public sealed partial class ItemDetailView
    {
        bool compact;
        int TypeSize(int value)=>Mathf.RoundToInt(value*Mathf.Clamp(textScale,.5f,2.5f));
        sealed class Row{public ItemTooltipLine line,expanded;public string group;}
        static string Group(string key,bool compact)
        {
            if(key=="name"||key=="type"||key.StartsWith("equipment-score"))return "head";
            if(key=="level"||key=="hands"||key=="material")return "meta";
            if(key=="equipped"||key=="locked")return compact?"details":"meta";
            if(key.StartsWith("main:"))return "main";
            if(key.StartsWith("implicit:"))return "implicit";
            if(key.StartsWith("affix:")||key.StartsWith("gem:"))return "affix";
            if(key=="special"||key=="aspect-salvage")return "power";
            if(key.StartsWith("lost-"))return "lost";
            if(key.StartsWith("sheet-")||key=="set-gain")return "sheet";
            return "details";
        }
        Text Line(Transform parent,ItemTooltipLine line,ItemTooltipLine expanded,float x,float y,float width,int pointSize,out float height,string prefix="",TextAnchor align=TextAnchor.UpperLeft)
        {
            int fs=TypeSize(pointSize);
            var t=UiLayout.Text(parent,line.key,prefix+line.text,x,y,width,1000,fs,StorageSurface.Hex(line.color),font);
            t.lineSpacing=1.12f;t.alignment=align;
            if(compact&&line.key.StartsWith("main:")&&!line.key.EndsWith("-label"))
            {
                // Keep the primary number and its unit together in three-column mobile comparisons.
                string original=t.text;t.text=line.text.Split(' ')[0];float natural=t.preferredWidth;t.text=original;
                if(natural>width)t.fontSize=Mathf.Max(TypeSize(UiTheme.Body),Mathf.FloorToInt(fs*width/natural));
                fs=t.fontSize;
            }
            float measured=line.text==expanded.text?t.preferredHeight:ItemRangeText.Bind(t,prefix+line.text,prefix+expanded.text).PreferredHeight;
            height=Mathf.Max(fs+4,measured+5);UiLayout.Place(t.rectTransform,x,y,width,height);return t;
        }
        float Line(Transform parent,ItemTooltipLine line,ItemTooltipLine expanded,float x,float y,float width,int pointSize,string prefix="",TextAnchor align=TextAnchor.UpperLeft)
        {Line(parent,line,expanded,x,y,width,pointSize,out float h,prefix,align);return h;}
        void Divider(Transform parent,float x,float width,ref float y)
        {
            y+=compact?3:8;var r=UiLayout.Rect("Engraved divider",parent);UiLayout.Place(r,x,y,width,compact?3:9);
            var trim=r.gameObject.AddComponent<ItemCardTrim>();trim.divider=true;trim.color=StorageSurface.Hex(UiTheme.ItemRuleHex);y+=compact?6:15;
        }
        void Section(Transform parent,string label,float x,float width,ref float y,bool rule)
        {
            if(rule)Divider(parent,x,width,ref y);
            var line=new ItemTooltipLine("section-"+label,Loc.T(label),UiTheme.GoldHex);
            y+=Line(parent,line,line,x,y,width,UiTheme.Caption)+(compact?1:5);
        }
        // Diablo IV reads the primary value and its name on one line: "1,435 damage per second  (+236)".
        float Main(Transform parent,ItemTooltipLine line,float x,float y,float w)
        {
            int vs=TypeSize(UiTheme.ItemValue),ls=TypeSize(UiTheme.Body);const float gap=8;
            var value=UiLayout.Text(parent,line.key,line.number??line.value,x,y,w,vs+8,vs,StorageSurface.Hex(line.color),font);
            value.horizontalOverflow=HorizontalWrapMode.Overflow;float vw=Mathf.Ceil(value.preferredWidth)+2,vh=Mathf.Ceil(value.preferredHeight)+2;
            if(vw>w){value.fontSize=Mathf.Max(ls,Mathf.FloorToInt(vs*w/vw));vw=Mathf.Ceil(value.preferredWidth)+2;vh=Mathf.Ceil(value.preferredHeight)+2;}
            UiLayout.Place(value.rectTransform,x,y,vw,vh);
            var label=UiLayout.Text(parent,line.key+"-label",line.label,x,y,w,ls+8,ls,StorageSurface.Hex(UiTheme.TextHex),font);
            label.horizontalOverflow=HorizontalWrapMode.Overflow;float lw=Mathf.Ceil(label.preferredWidth)+2,lh=Mathf.Ceil(label.preferredHeight)+2,rowEnd=vw;float h=vh;
            if(vw+gap+lw<=w){UiLayout.Place(label.rectTransform,x+vw+gap,y+vh-lh-3,lw,lh);rowEnd=vw+gap+lw;}
            else
            {
                label.horizontalOverflow=HorizontalWrapMode.Wrap;lh=Mathf.Ceil(label.preferredHeight)+2;
                UiLayout.Place(label.rectTransform,x,y+vh,w,lh);h+=lh;rowEnd=0;
            }
            if(!string.IsNullOrEmpty(line.difference))
            {
                var diff=UiLayout.Text(parent,"difference:"+line.key,line.difference,x,y,w,ls+8,ls,StorageSurface.Hex(line.color),font);
                diff.horizontalOverflow=HorizontalWrapMode.Overflow;float dw=Mathf.Ceil(diff.preferredWidth)+2,dh=Mathf.Ceil(diff.preferredHeight)+2;
                if(rowEnd>0&&rowEnd+gap+dw<=w)UiLayout.Place(diff.rectTransform,x+rowEnd+gap,y+vh-dh-3,dw,dh);
                else{UiLayout.Place(diff.rectTransform,x,y+h,w,dh);h+=dh;}
            }
            return h+4;
        }
        float Head(RectTransform body,Row[] rows,float cardWidth,float colW,float pad)
        {
            float y=compact?8:14,inner=colW;bool narrow=!compact&&cardWidth<230;
            var name=rows.FirstOrDefault(r=>r.line.key=="name")?.line;
            float art=icon&&(Item!=null||definitionArtwork!=null)?(compact?(cardWidth<160?0:28):narrow?44:68):0;
            if(art>0)
            {
                float artX=narrow?(cardWidth-art)/2:cardWidth-pad-art;
                var mount=UiLayout.Rect("Relic mount",body);UiLayout.Place(mount,artX,y,art,art);
                var backing=mount.gameObject.AddComponent<StorageSurface>();backing.Paint(UiTheme.ItemInsetHex,UiTheme.ItemBottomHex,UiTheme.ItemRuleHex);backing.raycastTarget=false;
                if(Item!=null)EquipmentSlotView.Icon(mount,Item,1,1,art-2);
                else definitionArtwork?.Invoke(mount,art);
                if(narrow)y+=art+9;
            }
            float titleY=y,titleWidth=!narrow&&art>0?inner-art-10:inner;
            if(name!=null)y+=Line(body,name,name,pad,y,titleWidth,compact?UiTheme.Body:narrow?UiTheme.Heading:UiTheme.ItemName)+2;
            var type=rows.FirstOrDefault(r=>r.line.key=="type")?.line;
            if(type!=null)y+=Line(body,type,type,pad,y,titleWidth,UiTheme.Caption);
            if(!narrow&&art>0)y=Mathf.Max(y,titleY+art);
            foreach(var score in rows.Where(r=>r.line.key.StartsWith("equipment-score")).Select(r=>r.line))
                y+=Line(body,score,score,pad,y+(compact?2:5),inner,score.key=="equipment-score"?(compact?UiTheme.Body:UiTheme.Heading+2):UiTheme.Caption)+(compact?2:5);
            if(compact)foreach(var meta in rows.Where(r=>r.group=="meta").Select(r=>r.line))
                y+=Line(body,meta,meta,pad,y+1,inner,UiTheme.Caption)+1;
            return y;
        }
        // Requirement and state sit bottom-right like Diablo IV's "Requires level / Class" footer.
        float Meta(RectTransform body,Row[] rows,float x,float w,float y)
        {
            var meta=rows.Where(r=>r.group=="meta").ToArray();if(compact||meta.Length==0)return y;
            Divider(body,x,w,ref y);
            foreach(var row in meta)y+=Line(body,row.line,row.expanded,x,y,w,UiTheme.Caption,"",TextAnchor.UpperRight)+1;
            return y;
        }
        float Column(RectTransform body,Row[] rows,float x,float w,float y,bool open,params string[] groups)
        {
            string previous=open?null:"identity";var implicits=rows.Where(r=>r.group=="implicit").ToArray();
            foreach(var row in rows)
            {
                string group=row.group;if(System.Array.IndexOf(groups,group)<0)continue;
                var line=row.line;string key=line.key;bool rule=previous!=null;
                if(group!=previous)
                {
                    string heading=group=="affix"?"추가 옵션":group=="power"?"특수 효과":group=="details"?"장비 정보":null;
                    if(heading!=null)Section(body,heading,x,w,ref y,rule);
                    else if(rule&&(group=="lost"||group=="sheet"))Divider(body,x,w,ref y);
                    previous=group;
                }
                if(group=="main"&&!compact&&!string.IsNullOrEmpty(line.value)){y+=Main(body,line,x,y,w);continue;}
                if(group=="implicit"&&!compact)
                {
                    bool last=row==implicits[implicits.Length-1];
                    var t=Line(body,line,row.expanded,x+24,y,w-24,UiTheme.Body,out float lh);
                    var branch=UiLayout.Rect("Stat branch",body);UiLayout.Place(branch,x+4,y,18,lh);
                    var trim=branch.gameObject.AddComponent<ItemCardTrim>();trim.branch=last?2:1;trim.color=StorageSurface.Hex(UiTheme.ItemRuleHex);
                    y+=lh+1;continue;
                }
                bool bullet=group=="affix"||group=="implicit";
                float start=y;bool power=key=="special";
                if(bullet)
                {
                    float d=line.greater?8:5;var mark=UiLayout.Rect("Affix diamond",body);UiLayout.Place(mark,x+(8-d)/2,y+TypeSize(UiTheme.Body)*.4f-(d-5)/2,d,d);
                    var dot=mark.gameObject.AddComponent<Image>();dot.color=line.greater?UiTheme.GoldBright:group=="affix"?UiTheme.Gold:UiTheme.Muted;dot.raycastTarget=false;mark.localRotation=Quaternion.Euler(0,0,45);
                }
                var block=power?UiLayout.Rect("Power inset",body):null;
                if(block!=null){var fill=block.gameObject.AddComponent<StorageSurface>();fill.Paint(UiTheme.ItemTopHex,UiTheme.ItemInsetHex,UiTheme.ItemRuleHex);fill.raycastTarget=false;y+=8;}
                float inset=bullet||power?12:0;
                var emblem=power?EquipmentArt.SetEmblem(Item):null;float powerY=y;
                bool ring=key=="sockets"&&Item?.sockets?.Count>0;
                if(emblem!=null)
                {
                    var emblemRect=UiLayout.Rect("Set emblem",body);UiLayout.Place(emblemRect,x+inset,y,28,28);
                    var image=emblemRect.gameObject.AddComponent<RawImage>();image.texture=emblem;image.raycastTarget=false;inset+=36;
                }
                if(ring)
                {
                    var socket=UiLayout.Rect("Socket mark",body);UiLayout.Place(socket,x,y+1,16,16);
                    var mark=socket.gameObject.AddComponent<ItemCardTrim>();mark.socket=Item.sockets[0].gemId==""||Item.sockets[0].gemId==null?1:2;mark.color=mark.socket==2?UiTheme.GoldBright:UiTheme.Muted;inset=22;
                }
                y+=Line(body,line,row.expanded,x+inset,y,w-inset-(power?8:0),key.EndsWith("heading")?UiTheme.Body:ring?UiTheme.Caption+1:group=="details"?UiTheme.Caption:UiTheme.Body);
                if(emblem!=null)y=Mathf.Max(y,powerY+28);
                if(block!=null){y+=8;UiLayout.Place(block,x,start,w,y-start);}
                y+=group=="affix"?5:2;
            }
            return y;
        }
        float DrawCard(RectTransform body,float width,ItemTooltipLine[] hidden,ItemTooltipLine[] shown)
        {
            const float pad=12,gutter=14;
            var rows=hidden.Select((h,n)=>new Row{line=h,expanded=shown[n],group=Group(h.key,compact)}).ToArray();
            bool wide=!compact&&width>=520&&rows.Any(r=>r.group=="affix"||r.group=="power"||r.group=="lost"||r.group=="sheet");
            float colW=wide?(width-pad*2-gutter)/2:width-pad*2,cardWidth=colW+pad*2;
            var name=rows.FirstOrDefault(r=>r.line.key=="name")?.line;
            Color rarity=StorageSurface.Hex(name?.color??UiTheme.GoldHex);
            var wash=UiLayout.Rect("Rarity wash",body);UiLayout.Place(wash,2,2,width-4,110);
            var gradient=wash.gameObject.AddComponent<StorageSurface>();gradient.top=new Color(rarity.r*.20f,rarity.g*.20f,rarity.b*.20f,.8f);gradient.bottom=Color.clear;gradient.raycastTarget=false;
            float y=Head(body,rows,cardWidth,colW,pad);Divider(body,pad,colW,ref y);
            if(!wide)
            {
                y=Column(body,rows,pad,colW,y,false,"main","implicit","affix","power","details","lost","sheet");
                return Meta(body,rows,pad,colW,y)+12;
            }
            float rightX=pad+colW+gutter,top=14;
            y=Meta(body,rows,pad,colW,Column(body,rows,pad,colW,y,false,"main","implicit","details"));
            float right=Column(body,rows,rightX,colW,top,true,"affix","power","lost","sheet");
            float end=Mathf.Max(y,right)+12;
            var rule=UiLayout.Rect("Column rule",body);UiLayout.Place(rule,pad+colW+gutter/2,top,1,end-top-12);
            var line=rule.gameObject.AddComponent<Image>();line.color=StorageSurface.Hex(UiTheme.ItemRuleHex);line.raycastTarget=false;
            return end;
        }
    }
    // Lightweight vector metalwork. No raster copies, layout participation or input interception.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class ItemCardTrim:MaskableGraphic
    {
        public bool divider;public int branch,socket;
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
            if(branch>0)
            {
                float bx=r.xMin+3,mid=r.center.y;
                Quad(bx,branch==1?r.yMin:mid,1,branch==1?r.height:r.yMax-mid,color);Quad(bx,mid,r.xMax-bx-4,1,color);
                int t=mesh.currentVertCount;mesh.AddVert(new Vector3(r.xMax-5,mid+3),color,Vector2.zero);mesh.AddVert(new Vector3(r.xMax-1,mid+.5f),color,Vector2.zero);mesh.AddVert(new Vector3(r.xMax-5,mid-2),color,Vector2.zero);mesh.AddTriangle(t,t+1,t+2);return;
            }
            if(socket>0)
            {
                var c=r.center;float outer=Mathf.Min(r.width,r.height)/2-.5f,inner=outer-1.8f;const int n=18;
                for(int i=0;i<n;i++)
                {
                    float a0=i*Mathf.PI*2/n,a1=(i+1)*Mathf.PI*2/n;int v=mesh.currentVertCount;
                    mesh.AddVert(c+new Vector2(Mathf.Cos(a0),Mathf.Sin(a0))*outer,color,Vector2.zero);mesh.AddVert(c+new Vector2(Mathf.Cos(a1),Mathf.Sin(a1))*outer,color,Vector2.zero);
                    mesh.AddVert(c+new Vector2(Mathf.Cos(a1),Mathf.Sin(a1))*inner,color,Vector2.zero);mesh.AddVert(c+new Vector2(Mathf.Cos(a0),Mathf.Sin(a0))*inner,color,Vector2.zero);
                    mesh.AddTriangle(v,v+1,v+2);mesh.AddTriangle(v,v+2,v+3);
                }
                if(socket==2)
                {
                    float h=outer*.5f;int v=mesh.currentVertCount;
                    mesh.AddVert(c+new Vector2(-h,0),color,Vector2.zero);mesh.AddVert(c+new Vector2(0,h),color,Vector2.zero);mesh.AddVert(c+new Vector2(h,0),color,Vector2.zero);mesh.AddVert(c+new Vector2(0,-h),color,Vector2.zero);
                    mesh.AddTriangle(v,v+1,v+2);mesh.AddTriangle(v,v+2,v+3);
                }
                return;
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
