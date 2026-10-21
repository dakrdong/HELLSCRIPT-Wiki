using System;
using System.Linq;
using Hellscript.Runes;
using UnityEngine;
using UnityEngine.UI;
namespace Hellscript
{
    public sealed partial class RuneBoardWindow
    {
        static string[] LegendStates={"lit","dim","open","unlock","closed","anchor"};
        // sec-head: the blue section number and the title on one baseline (Jua 1.05b, number .8 of it spaced .06em).
        void DrawSectionHead(Transform p,float x,float top,string no,string title,string right=null,bool showTitle=true)
        {
            float tf=1.05f*b,nf=.8f*tf,baseline=top+.95f*b;
            var n=Run(p,no,x,baseline,Width(no,nf,400,true)+8,nf,RuneCasual.Blue,400,true);Spaced(n,.06f*nf);float nw=Width(no,nf,400,true)+.06f*nf*no.Length+.5f*tf;
            if(showTitle)Run(p,title,x+nw,baseline,Width(title,tf,400,true)+8,tf,Ink,400,true);
            if(right!=null){float rw=Width(right,.83f*b,700);Run(p,right,((RectTransform)p).rect.width-3-.65f*b-rw,baseline,rw+4,.83f*b,Faint,700,false,TextAnchor.MiddleRight);}
        }
        RectTransform DrawStorageBox(float x,float y,float w,float h)
        {
            float pad=K(.65f,.55f,.65f)*b,gap=K(.45f,.3f,.45f)*b,padY=Land?.45f*b:.65f*b;
            var box=Card(shell.Frame,"Rune storage panel",x,y,w,h,18,3,5,RuneCasual.Cream);
            var lit=kit.Clip(box,"Storage glow",3,3,w-6,h-6);kit.Glow(lit,(w-6)/2,0,(w-6)*.9f,(h-6)*.4f,Color.white,.7f);
            kit.Dashed(box,5,5,w-10,h-10,13,2,RuneCasual.Alpha(Ink,.13f),6,6);
            float cx=3+pad,cw=w-2*(3+pad),cy=3+padY,headH=1.335f*b;
            var all=Session.Stored.ToArray();
            DrawSectionHead(box,cx,cy,"02",T("storage.title"),all.Length+" / "+Session.Inventory.OwnedPieces.Count);
            float ty=cy+headH+gap,tabH=K(51.3f,29.8f,51.7f),tcw=(cw-4)/6;
            var tabs=Card(box,"Rune type tabs",cx,ty,cw,tabH,12,2,0);
            for(int i=-1;i<5;i++)
            {
                int type=i,count=type<0?all.Length:all.Count(pc=>pc.Type==type);bool on=type==color;float tx=2+(i+1)*tcw;
                var cell=kit.Box(tabs,"rune-color-"+i,tx,2,tcw,tabH-4,on?RuneCasual.YellowTop:Color.clear,Color.clear,0,0,0,on?RuneCasual.Yellow:Color.clear);
                if(i>=0)kit.Box(cell,"Divider",0,0,2,tabH-4,RuneCasual.Alpha(Ink,.15f),Color.clear,0,0,0);
                float lf=.8f*b,sf=.8f*lf,pic=(Land?1.25f:1.45f)*lf;Color ink=type<0?RuneCasual.Text2:RuneCasual.RingInk[type];
                bool label=!Land||type<0;float stack=(type>=0?pic+.05f*lf:0)+(label?lf*1.15f+.05f*lf:0)+sf*1.15f,top=(tabH-4-stack)/2;
                if(Wide||Port){top=type<0?(tabH-4-lf*1.15f-.05f*lf-sf*1.15f)/2:2;}
                float yy=top;
                if(type>=0){Pic(cell,"emblem-"+(type==3?"critical":TypeName(type)),(tcw-pic)/2,yy,pic,ink);yy+=pic+.05f*lf;}
                if(label){Line(cell,type<0?T("storage.all"):TypeShort(type),0,yy,lf*1.15f,tcw,lf,on?Ink:RuneCasual.Text2,400,true,TextAnchor.UpperCenter);yy+=lf*1.15f+.05f*lf;}
                Line(cell,count.ToString(),0,yy,sf*1.15f,tcw,sf,Faint,700,false,TextAnchor.UpperCenter);
                Press(cell,"rune-color-"+i,()=>{color=type;storageOffset=0;RequestRefresh();});
            }
            float chipsY=ty+tabH+gap,chipH=K(24.4f,20.8f,24.6f),cf=K(.82f,.76f,.82f)*b,cg=.3f*b,chipW=(cw-5*cg)/6;
            for(int i=0;i<=5;i++)
            {
                int size=i;bool on=i==0?sizes.Count==0:sizes.Contains(i);string label=i==0?T("storage.allSizes"):Cells(i);
                var chip=on?Card(box,"rune-size-"+i,cx+i*(chipW+cg),chipsY,chipW,chipH,10,2,2,RuneCasual.YellowTop,Ink,RuneCasual.Yellow):Card(box,"rune-size-"+i,cx+i*(chipW+cg),chipsY,chipW,chipH,10,2,2,Color.white,RuneCasual.Edge);
                Mid(chip,label,0,chipH/2,chipW,cf,on?Ink:RuneCasual.Text2,700,false,TextAnchor.MiddleCenter);
                Press(chip,"rune-size-"+i,()=>{if(size==0)sizes.Clear();else if(!sizes.Add(size))sizes.Remove(size);storageOffset=0;RequestRefresh();});
            }
            float gy=chipsY+chipH+gap,gx=cx,gw=cw,vh=h-3-padY-gy;float tg=.45f*b,minTile=K(4.1f,4.2f,4.9f)*b,inner=gw-.2f*b;
            int columns=Mathf.Max(1,Mathf.FloorToInt((inner+tg)/(minTile+tg)));float tile=(inner-tg*(columns-1))/columns;
            var scroll=Scroll(box,"Rune storage scroll",gx,gy,gw,vh,out var content);scroll.onValueChanged.AddListener(_=>storageOffset=content.anchoredPosition.y);
            var stacks=Session.Stacks().Where(s=>(color<0||s.Piece.Type==color)&&(sizes.Count==0||sizes.Contains(s.Piece.Shape.Size))).ToArray();
            float top0=.15f*b;content.sizeDelta=new Vector2(0,top0+Mathf.Ceil(stacks.Length/(float)columns)*(tile+tg)+.4f*b);content.anchoredPosition=new Vector2(0,Mathf.Min(storageOffset,Mathf.Max(0,content.sizeDelta.y-vh)));
            for(int i=0;i<stacks.Length;i++)
            {
                var stack=stacks[i];var piece=stack.Pieces.FirstOrDefault(pc=>pc.InstanceId==Session.SelectedId)??stack.Piece;int rotation=Session.StoredRotation(piece);bool chosen=stack.Pieces.Any(pc=>pc.InstanceId==Session.SelectedId);
                float tx=i%columns*(tile+tg),ty2=top0+i/columns*(tile+tg);var ink=RuneCasual.RingInk[RuneCasual.Slot(piece.Type)];
                var tileBox=chosen?Card(content,"rune-card-"+piece.InstanceId,tx,ty2,tile,tile,12,3,3,RuneCasual.YellowTop,Ink,RuneCasual.Yellow):Card(content,"rune-card-"+piece.InstanceId,tx,ty2,tile,tile,12,2,3,Color.white,RuneCasual.Edge,RuneCasual.Mist);
                if(chosen)kit.Box(tileBox,"Chosen ring",-4,-4,tile+8,tile+8,Color.clear,RuneCasual.Alpha(RuneCasual.Yellow,.45f),16,4,0);
                var art=UiLayout.Rect("Rune block",tileBox);float ip=chosen?3+.2f*b:2+.2f*b;UiLayout.Place(art,ip,ip,tile-2*ip,tile-2*ip);var gem=RuneGemView.Create(art,"Rune gem");gem.Icon(piece,rotation,Session.SelectedId==piece.InstanceId);
                var size=Run(tileBox,piece.Shape.Size.ToString(),.4f*b+(chosen?1:0),ty2*0+.15f*b+.78f*b*.8f+(chosen?1:0),30,.78f*b,ink,400,true);size.name="Rune size";
                if(stack.Pieces.Length>1){string cnt="×"+stack.Pieces.Length;float cw2=Width(cnt,.9f*b,400,true);var c=Run(tileBox,cnt,tile-.4f*b-cw2-(chosen?1:0),tile-.1f*b-.9f*b*.2f-(chosen?1:0),cw2+4,.9f*b,Ink,400,true);Outline(c,Color.white);}
                Press(tileBox,"rune-card-"+piece.InstanceId,()=>Select(piece.InstanceId));tileBox.GetComponent<UiButton>().Configure(UiButtonRole.Item,chosen);tileBox.GetComponent<UiButton>().Face.gameObject.SetActive(false);
                var drag=tileBox.gameObject.AddComponent<RuneBoardStoragePointer>();drag.Owner=this;drag.Piece=piece;drag.Source=gem;
            }
            if(stacks.Length==0)Mid(content,T("storage.empty"),0,3*b,gw,.9f*b,Faint,700,false,TextAnchor.MiddleCenter);
            return box;
        }
        static void Outline(Text t,Color c)
        {var v=new D4Shadow[4];int i=0;foreach(var p in new[]{new Vector2(1,0),new Vector2(-1,0),new Vector2(0,1),new Vector2(0,-1)})v[i++]=new D4Shadow{x=p.x,y=p.y,color="SteelFFFFFFA1000"};t.gameObject.AddComponent<SkillTreeD4TextShadow>().Apply(v,1);}
        string ShapeName(RunePiece piece)=>piece.Shape.Size==1?T("shape.seed"):piece.Shape.Size==2?T("shape.pair"):Loc.F("모양 {0}",piece.Shape.ShapeNumber);
        static string AbilityLine(RuneBoardCell c)=>c.Ability.Meaning=="root"?Loc.T("공용 시작점"):RuneMasteryCatalog.AbilityName(c.Ability.Meaning)+(c.IsElite?"":" +"+c.Value.ToString("0.##")+Loc.T(c.Unit));

        // ---- inspector: legend (nothing chosen), a block (stored or placed) or a slot
        RectTransform DrawInspectorBox(float x,float y,float w,float h)
        {
            float pad=K(.6f,.55f,.65f)*b,padY=Port?.45f*b:Land?.45f*b:.65f*b;var box=Card(shell.Frame,"Rune inspector",x,y,w,h,18,3,5,RuneCasual.Paper);
            kit.Dashed(box,5,5,w-10,h-10,13,2,RuneCasual.Alpha(Ink,.13f),6,6);
            float cx=3+pad,cw=w-2*(3+pad),cy=3+padY,headH=1.335f*b,gap=K(.3f,.3f,.45f)*b;var piece=Session.Selected;bool cell=piece==null&&Session.SelectedCell.HasValue;
            if(!Port)DrawSectionHead(box,cx,cy,"03",T("insp.title"));
            // deselect
            if(piece!=null||cell)
            {
                float tw=Width(T("insp.clear"),.82f*b,700),bw=tw+.82f*b*1.5f+8;var d=kit.Box(box,"rune-clear-selection",w-3-.65f*b+4-bw,cy-3,bw,22.9f*b/14,Color.clear,Color.clear,0,0,0);
                Pic(d,"close",4,(d.rect.height-.82f*b)/2,.82f*b,RuneCasual.Blue);Mid(d,T("insp.clear"),4+.82f*b+.4f*b,d.rect.height/2,tw+4,.82f*b,RuneCasual.Blue,700);Press(d,"rune-clear-selection",()=>{Session.ClearSelection();RequestRefresh();});
            }
            float top=Port?cy:cy+headH+gap,bottom=h-3-padY,btnH=K(25f,30.8f,39.2f),btnF=K(.82f*b,b,b);
            if(piece!=null)DrawPieceInspector(box,cx,top,cw,bottom,btnH,piece);
            else if(cell)DrawCellInspector(box,cx,top,cw,bottom,btnH);
            else DrawEmptyInspector(box,cx,top,cw,bottom,btnH);
            return box;
        }
        void InspectorArt(Transform box,float x,float y,float size,Action<RectTransform> draw,bool round=false)
        {
            var art=round?kit.Box(box,"Rune detail picture",x,y,size,size,Color.white,Ink,size/2,3,3,RuneCasual.Mist):kit.Box(box,"Rune detail picture",x,y,size,size,RuneCasual.SkyTop,Ink,14,2,0,RuneCasual.SkyTop);
            draw(art);
        }
        void DrawEmptyInspector(Transform box,float cx,float top,float cw,float bottom,float btnH)
        {
            float art=K(3.9f,3.3f,4.8f)*b,gx=cx+art+(Land?.55f:.75f)*b;float tf=K(1.1f,.95f,1.1f)*b,sf=K(.76f,.76f,.76f)*b,hh=tf*1.25f,sh=sf*1.5f;
            if(!Port)
            {
                var a=InspectorBox(box,cx,top,art);var sample=Session.Editor.Board.Cells.First(c=>c.AbilityType==0);var gem=RuneGemView.Create(a,"Legend art");gem.Slot(sample,false,"open");
                float mt=top+(art-(hh+.2f*b+sh))/2;Line(box,T("insp.emptyTitle"),gx,mt,hh,cw-(gx-cx),tf,Ink,400,true);Line(box,T("insp.emptySub"),gx,mt+hh+.2f*b,sh,cw-(gx-cx),sf,Faint,700);
            }
            else
            {
                Line(box,T("insp.emptyTitle"),cx,top+.05f*b,hh,cw*.6f,tf,Ink,400,true);Line(box,T("insp.emptySub"),cx,top+.05f*b+hh+.2f*b,sh,cw*.7f,sf,Faint,700);
            }
            // legend swatches (wrap at the card width)
            float sw=K(1.9f*.76f,1.82f*.76f*1.0f,1.82f)*b*(Port?1.0f:1.0f);if(Port)sw=1.9f*sf;else sw=Land?1.7f*sf*1.5f:25.5f*b/14f;
            float lx=cx,ly=Port?top+hh+sh+.5f*b:top+art+.45f*b;float lf=.76f*b,lg=.8f*b,rowGap=.2f*b;var sample2=Session.Editor.Board.Cells.First(c=>c.AbilityType==0);
            for(int i=0;i<6;i++)
            {
                string st=LegendStates[i];string label=Title(T("legend."+(st=="open"?"empty":st=="unlock"?"can":st)));float lw=Width(label,lf,700),item=sw+.3f*lf+lw;
                if(lx+item>cx+cw+.5f&&lx>cx){lx=cx;ly+=sw+rowGap;}
                var r=UiLayout.Rect("Legend "+st,box);UiLayout.Place(r,lx,ly,sw,sw);RuneGemView.Create(r,"Gem swatch").Slot(sample2,i==0,st);
                Mid(box,label,lx+sw+.3f*lf,ly+sw/2,lw+4,lf,RuneCasual.Text2,700);lx+=item+lg;
            }
            // actions
            float by=bottom-btnH,bw=Port?Width(T("tutorial.open"),K(.82f,1f,1f)*b,400,true)+2.2f*b+K(1.15f,1.15f,1.15f)*b:(cw-.5f*b)/2;
            if(Port){ActionCard(box,"rune-practice",T("tutorial.open"),"book",cw-bw+cx,23.9f*b/13.86f,bw,btnH,()=>OpenDialog("tutorial"));}
            else{ActionCard(box,"rune-practice",T("tutorial.open"),"book",cx,by,bw,btnH,()=>OpenDialog("tutorial"));ActionCard(box,"rune-guide",T("guide.open"),"help",cx+bw+.5f*b,by,bw,btnH,()=>OpenDialog("legend"));}
        }
        RectTransform InspectorBox(Transform box,float x,float y,float size)=>kit.Box(box,"Rune detail picture",x,y,size,size,RuneCasual.SkyTop,Ink,14,2,0,RuneCasual.SkyTop);
        UiButton ActionCard(Transform p,string name,string label,string pic,float x,float y,float w,float h,Action a,bool enabled=true,bool warm=false,bool primary=false,float font=0,bool danger=false)
        {
            float f=font>0?font:K(.82f,1f,1f)*b,ps=1.15f*f;Color fill=Color.white,edge=Ink,end=Color.white,ink=Ink,pc=Ink;float border=2,shadow=3;
            if(!enabled){fill=end=RuneCasual.DisabledFill;edge=RuneCasual.DisabledEdge;ink=pc=RuneCasual.DisabledInk;shadow=0;}
            else if(warm){fill=RuneCasual.YellowTop;end=RuneCasual.Yellow;}
            else if(danger){fill=RuneCasual.CoralTop;end=RuneCasual.Coral;border=3;ink=pc=Color.white;}
            else if(primary){fill=RuneCasual.GreenTop;end=RuneCasual.Green;border=3;ink=pc=Color.white;}
            var card=kit.Box(p,name,x,y,w,h,fill,edge,12,border,shadow,end);float lw=Width(label,f,400,true),total=(pic!=null?ps+.5f*f:0)+lw,sx=(w-total)/2;
            if(pic!=null){Pic(card,pic,sx,(h-ps)/2,ps,pc);sx+=ps+.5f*f;}
            var t=Mid(card,label,sx,h/2,lw+6,f,ink,400,true);if((primary||danger)&&enabled)Outline(t,Ink);
            return Press(card,name,a,enabled);
        }
        void DrawPieceInspector(Transform box,float cx,float top,float cw,float bottom,float btnH,RunePiece piece)
        {
            float art=K(3.9f,3.3f,4.8f)*b,gx=cx+art+(Land?.55f:.75f)*b,tf=K(1.1f,.95f,1.1f)*b,sf=.76f*b,hh=tf*1.25f;var a=InspectorBox(box,cx,top,art);
            var gem=RuneGemView.Create(a,"rune-detail-pickup");gem.Icon(piece,Session.Rotation);var press=a.gameObject.AddComponent<RuneBoardStoragePointer>();a.GetComponent<Image>().raycastTarget=true;press.Owner=this;press.Piece=piece;press.Source=gem;
            var nodes=Session.Effects().Nodes.Where(n=>n.CoveringRuneId==piece.InstanceId).ToArray();int on=nodes.Count(n=>n.IsPreviewActive),off=nodes.Length-on;bool placed=Session.SelectedPlaced;
            string typeName=RuneV13Catalog.TypeName(piece.Type),shape="· "+ShapeName(piece);float mt=top+(art-(hh+.2f*b+sf*1.5f))/2;
            float tw=Width(typeName,tf,400,true);Line(box,typeName,gx,mt,hh,tw+4,tf,RuneCasual.TextInk[RuneCasual.Slot(piece.Type)],400,true);Line(box,shape,gx+tw+.35f*tf,mt,hh,Width(shape,tf,400,true)+4,tf,Ink,400,true);
            string sub=Cells(piece.Shape.Size)+" · "+Session.Rotation*60+"° · "+(placed?"":T("insp.stored"));float sx=gx;
            if(placed){string a1=English?"On: "+on:T("insp.lit",on),a2=English?"Off: "+off:T("insp.dim",off);Line(box,Cells(piece.Shape.Size)+" · "+Session.Rotation*60+"° ·",sx,mt+hh+.2f*b,sf*1.5f,300,sf,Faint,700);sx+=Width(Cells(piece.Shape.Size)+" · "+Session.Rotation*60+"° ·",sf,700)+.35f*sf;Line(box,a1,sx,mt+hh+.2f*b,sf*1.5f,200,sf,RuneCasual.GreenInk,700);sx+=Width(a1,sf,700)+.35f*sf;Line(box,"·",sx,mt+hh+.2f*b,sf*1.5f,20,sf,Faint,700);sx+=Width("·",sf,700)+.35f*sf;Line(box,a2,sx,mt+hh+.2f*b,sf*1.5f,200,sf,off>0?RuneCasual.OrangeInk:Faint,700);}
            else Line(box,sub,gx,mt+hh+.2f*b,sf*1.5f,cw-(gx-cx),sf,Faint,700);
            float by=bottom-btnH,bw=(cw-.5f*b)/2,y=top+art+.45f*b;
            if(!Port)
            {
                string note=placed?(off>0?T("insp.someDim"):T("insp.allLit")):T("insp.hintPlace");float nf=.84f*b;
                if(note!=""){float nh=Wrap(box,note,cx,y,cw,nf,1.6f,RuneCasual.Text2,400);y+=nh+.45f*b;}
                if(placed&&!Land)
                {
                    string move=T("insp.moveHint");float mf=.77f*b;Line(box,move,cx,y,mf*1.5f,cw,mf,Faint,700);y+=mf*1.5f+.35f*b;
                    float rowH=26.4f*b/14f,pitch=rowH+.25f*.8f*b,view=by-.45f*b-y;
                    if(view>rowH*.6f)
                    {
                        var list=Scroll(box,"Rune effect list",cx,y,cw,view,out var content);content.sizeDelta=new Vector2(0,nodes.Length*pitch);float rf=.8f*b;
                        for(int i=0;i<nodes.Length;i++)
                        {
                            var n=nodes[i];bool lit=n.IsPreviewActive;var row=kit.Box(content,"Rune effect "+i,0,i*pitch,cw,rowH,Color.white,lit?Ink:RuneCasual.DisabledEdge,10,2,0);
                            var gl=UiLayout.Rect("Glyph",row);UiLayout.Place(gl,2+.5f*rf,(rowH-1.5f*rf)/2,1.5f*rf,1.5f*rf);var raw=gl.gameObject.AddComponent<RawImage>();raw.texture=RuneV13Art.Atlas;raw.uvRect=RuneV13Art.Glyph("b-"+n.Cell.Glyph);raw.color=lit?RuneCasual.OrangeInk:RuneCasual.Gray;raw.raycastTarget=false;
                            string nm=RuneMasteryCatalog.AbilityName(n.Cell.Ability.Meaning),val=n.Cell.IsElite?"":"+"+n.Cell.Value.ToString("0.##")+Loc.T(n.Cell.Unit),tag=lit?(English?"on":T("insp.on")):(English?"off":T("insp.off"));
                            float tw2=Width(tag,.85f*rf,700);Mid(row,nm,2+.5f*rf+1.5f*rf+.5f*rf,rowH/2,cw-100,rf,Ink,700);
                            Mid(row,tag,cw-2-.5f*rf-tw2,rowH/2,tw2+4,.85f*rf,lit?RuneCasual.GreenInk:RuneCasual.Gray,700,false,TextAnchor.MiddleRight);
                            float vw=Width(val,rf,400,true);Mid(row,val,cw-2-.5f*rf-tw2-.5f*rf-vw,rowH/2,vw+4,rf,RuneCasual.OrangeInk,400,true,TextAnchor.MiddleRight);
                        }
                    }
                }
            }
            ActionCard(box,"rune-rotate",T("insp.rotate"),"rotate",cx,by,bw,btnH,Rotate,!Session.Viewing);
            ActionCard(box,"rune-remove",English?"Recall":T("insp.recall"),"recall",cx+bw+.5f*b,by,bw,btnH,Remove,!Session.Viewing&&placed,placed);
        }
        float Wrap(Transform p,string s,float x,float y,float w,float fs,float lh,Color c,int weight=400)
        {var t=kit.Txt(p,s,x,y,w,1000,fs,c,weight,false,TextAnchor.UpperLeft,true);SettingsLook.LineHeight(t,lh);t.horizontalOverflow=HorizontalWrapMode.Wrap;float h=t.GetComponent<SettingsParagraph>().preferredHeight;UiLayout.Place(t.rectTransform,x,y,w,h);return h;}
        void DrawCellInspector(Transform box,float cx,float top,float cw,float bottom,float btnH)
        {
            if(!Session.Editor.Board.TryGetCell(Session.SelectedCell.Value,out var cell))return;var node=Session.Effects().Nodes.First(n=>n.Cell.Coordinate.Equals(cell.Coordinate));bool open=Session.Editor.Board.IsOpen(cell,6);
            float art=K(54f/13.86f,3.3f,4.8f)*b,gx=cx+art+(Land?.55f:.75f)*b,tf=K(1.1f,.95f,1.1f)*b,sf=.76f*b,hh=tf*1.25f;
            var a=kit.Box(box,"Rune detail picture",cx,top,art,art,Color.white,node.IsPreviewActive?RuneCasual.Yellow:RuneCasual.RingInk[RuneCasual.Slot(cell.AbilityType,cell.IsElite)],art/2,3,3,RuneCasual.Mist);RuneGemView.Create(a,"Rune ability medal").Slot(cell,node.IsPreviewActive);
            var covering=node.CoveringRuneId==null?null:Session.Inventory.FindOwned(node.CoveringRuneId);bool mismatch=covering!=null&&!node.IsPreviewActive;
            string status=!open?T(cell.RegionGrade>0&&!Session.Progress.order.Contains(cell.RegionGrade)?"slot.sealed":"slot.closed"):mismatch?T("slot.dim")+" · "+RuneV13Catalog.TypeName(covering.Type)+" ≠ "+RuneV13Catalog.TypeName(cell.AbilityType):"";
            float mt=top+(art-(hh+.2f*b+sf*1.5f))/2;var title=Line(box,AbilityLine(cell),gx,mt,hh,cw-(gx-cx)-4,tf,Ink,400,true);title.name="rune-detail-title";title.horizontalOverflow=HorizontalWrapMode.Overflow;
            string sub=RuneV13Catalog.RegionName(cell.RegionGrade)+" · "+(cell.Ability.Meaning=="root"?T("slot.root"):cell.IsElite?T("type.elite"):T("slot.tier",cell.NumericGrade))+(status!=""?" · "+status:"");Line(box,sub,gx,mt+hh+.2f*b,sf*1.5f,cw-(gx-cx),sf,mismatch?RuneCasual.CoralInk:Faint,700);
            float by=bottom-btnH,bw=(cw-.5f*b)/2;
            if(!Port&&!Land)Wrap(box,Loc.T(RuneV13Catalog.ForMeaning(cell.Ability.Meaning).description),cx,top+art+.45f*b,cw,.84f*b,1.6f,RuneCasual.Text2,400);
            ActionCard(box,"rune-region-preview",T("region.info"),null,cx,by,bw,btnH,()=>{modalRegion=cell.RegionGrade;OpenDialog("region-info");});
            if(!open)ActionCard(box,"rune-unlock",T("slot.open"),"unlock",cx+bw+.5f*b,by,bw,btnH,()=>Unlock(cell.Coordinate),!Session.Viewing&&RuneMasteryProgress.CanUnlock(Session.Progress,cell.Coordinate),true);
            else if(cell.Ability.Meaning=="root")ActionCard(box,"rune-next-region",T("region.next"),null,cx+bw+.5f*b,by,bw,btnH,()=>OpenDialog("regions"));
        }

        // ---- the fixed dock: save state, presets, undo, revert, save
        void DrawDock(float y,float h)
        {
            var dock=kit.Box(shell.Frame,"Rune action bar",0,y,width,h,new Color(1,1,1,.25f),Color.clear,0,0,0,new Color(1,1,1,.7f));kit.Rule(dock,16,0,width-32,RuneCasual.Alpha(Ink,.12f));
            float bh=K(38.8f,30.8f,39.2f),by=(h-bh)/2,f=b,ps=1.15f*f,gap=Port?.5f*b:.5f*b,padX=1.1f*f,x;bool dirty=Session.Dirty;
            string[] names={"rune-presets","rune-undo","rune-revert","rune-save"};string[] labels={T("presets.short"),UndoLabel,T("dock.revert"),T("dock.save")};string[] pics={"presets","undo","revert","save"};
            bool[] on={true,Session.UndoCount>0,dirty,dirty};float[] w=new float[4];
            for(int i=0;i<4;i++)w[i]=(i==3&&on[3]?6:4)+2*padX+ps+.5f*f+Width(labels[i],f,400,true);
            if(Port){w[0]=w[1]=w[2]=K(89.8f,0,0);w[3]=133f;}
            float total=w.Sum()+3*gap;x=Port?width-.6f*b-total:width-.9f*b-total;
            if(!Port){float sf=.86f*b;bool d=dirty;var dot=kit.Box(dock,"Save dot",.9f*b,(h-.7f*b)/2,.7f*b,.7f*b,d?RuneCasual.Yellow:RuneCasual.Green,Ink,.35f*b,2,0);Mid(dock,T(d?"dock.dirty":"dock.saved"),.9f*b+.7f*b+.55f*b,h/2,300,sf,d?RuneCasual.OrangeInk:RuneCasual.Text2,700);}
            string[] actions={"presets","undo","revert","save"};
            for(int i=0;i<4;i++)
            {
                int n=i;var btn=ActionCard(dock,names[i],labels[i],pics[i],x,by,w[i],bh,()=>{if(n==0)OpenDialog("presets");else if(n==1){Session.Undo();RequestRefresh();}else if(n==2)OpenDialog("revert");else Save();},on[i],false,i==3,b);x+=w[i]+gap;
            }
        }
    }
}
