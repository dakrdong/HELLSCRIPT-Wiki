using System.Linq;
using Hellscript.Runes;
using UnityEngine;
using UnityEngine.UI;
namespace Hellscript
{
    public sealed partial class RuneBoardWindow
    {
        void DrawStorage()
        {
            float w=storagePanel.rect.width,h=storagePanel.rect.height,pad=E(.6f),head=E(2.3f),tabs=E(landscape?2.3f:3.1f),chips=E(landscape?1.7f:2.1f),gap=E(.3f);
            Txt(storagePanel,"02",pad,0,E(1.3f),head,.72f,RuneBoardSkin.Gold,display:true);Txt(storagePanel,T("storage.title"),pad+E(1.5f),0,w*.6f,head,1.05f,RuneBoardSkin.GoldBright,TextAnchor.MiddleLeft,true,display:true);
            var all=Session.Stored.ToArray();Txt(storagePanel,all.Length+" / "+Session.Inventory.OwnedPieces.Count,w*.65f,0,w*.35f-pad,head,.74f,RuneBoardSkin.Muted,TextAnchor.MiddleRight);
            float tabW=(w-pad*2)/6;
            for(int i=-1;i<5;i++)
            {
                int type=i,count=type<0?all.Length:all.Count(p=>p.Type==type);var b=Btn(storagePanel,"rune-color-"+i,"",pad+(i+1)*tabW,head,tabW,tabs,()=>{color=type;storageOffset=0;RequestRefresh();});UiTheme.Choice(b,i==color);
                if(type>=0)RuneBoardArt.CategoryImage(b.transform,type,(tabW-E(1))/2,E(.12f),E(1));
                if(!landscape||type<0)Txt(b.transform,type<0?T("storage.all"):T("type.short."+RuneBoardArt.Types[type]),0,type<0?E(.3f):E(1.04f),tabW,E(.8f),.75f,type==color?RuneBoardSkin.GoldBright:RuneBoardSkin.Muted,TextAnchor.MiddleCenter,true);
                Txt(b.transform,count.ToString(),0,tabs-E(.8f),tabW,E(.7f),.6f,RuneBoardSkin.Muted,TextAnchor.MiddleCenter);
            }
            float y=head+tabs+gap;
            for(int i=0;i<=5;i++){int size=i;var b=Btn(storagePanel,"rune-size-"+i,i==0?T("storage.allSizes"):T("storage.cells",i),pad+i*tabW,y,tabW-gap,chips,()=>{if(size==0)sizes.Clear();else if(!sizes.Add(size))sizes.Remove(size);storageOffset=0;RequestRefresh();});UiTheme.Choice(b,i==0?sizes.Count==0:sizes.Contains(i),false);}
            y+=chips+gap;float vw=w-pad*2,vh=h-y-pad;
            var scroll=Scroll(storagePanel,"Rune storage scroll",pad,y,vw,vh,out var content);scroll.onValueChanged.AddListener(_=>storageOffset=content.anchoredPosition.y);
            int columns=Mathf.Max(1,Mathf.FloorToInt((vw+gap)/(E(landscape?4.2f:4.1f)+gap)));float slot=(vw-gap*(columns-1))/columns;
            var stacks=Session.Stacks().Where(s=>(color<0||s.Piece.Type==color)&&(sizes.Count==0||sizes.Contains(s.Piece.Shape.Size))).ToArray();
            content.sizeDelta=new Vector2(0,Mathf.Ceil(stacks.Length/(float)columns)*(slot+gap));content.anchoredPosition=new Vector2(0,Mathf.Min(storageOffset,Mathf.Max(0,content.sizeDelta.y-vh)));
            for(int i=0;i<stacks.Length;i++)
            {
                var stack=stacks[i];var piece=stack.Pieces.FirstOrDefault(p=>p.InstanceId==Session.SelectedId)??stack.Piece;int rotation=Session.StoredRotation(piece);float x=i%columns*(slot+gap),sy=i/columns*(slot+gap);
                var b=Btn(content,"rune-card-"+piece.InstanceId,"",x,sy,slot,slot,()=>Select(piece.InstanceId));bool chosen=stack.Pieces.Any(p=>p.InstanceId==Session.SelectedId);((UiButton)b).Configure(UiButtonRole.Item,chosen);
                var surface=b.GetComponent<StorageSurface>();surface.SetSkin(shell.Skin,border:chosen?RuneBoardSkin.Accent:RuneBoardSkin.Border,radius:E(.48f),borderWidth:E(.12f),shadow:E(.16f));surface.canvasRenderer.SetAlpha(1);
                var art=UiLayout.Rect("Rune block",b.transform);UiLayout.Place(art,E(.15f),E(.55f),slot-E(.3f),slot-E(1.1f));var gem=RuneGemView.Create(art,"Rune gem");gem.Icon(piece,rotation,Session.SelectedId==piece.InstanceId);
                Txt(b.transform,piece.Shape.Size.ToString(),E(.2f),0,E(1),E(.9f),.76f,RuneBoardArt.Color(RuneV13Catalog.Data.types[piece.Type].ink));
                Txt(b.transform,"G"+piece.Grade,slot-E(1.5f),0,E(1.3f),E(.8f),.55f,RuneBoardSkin.Muted,TextAnchor.MiddleRight);
                if(stack.Pieces.Length>1)Txt(b.transform,"×"+stack.Pieces.Length,E(.2f),slot-E(1),slot-E(.4f),E(.9f),.9f,RuneBoardSkin.GoldBright,TextAnchor.MiddleRight,true);
                var drag=b.gameObject.AddComponent<RuneBoardStoragePointer>();drag.Owner=this;drag.Piece=piece;drag.Source=gem;
            }
            if(stacks.Length==0)Txt(content,T("storage.empty"),0,0,vw,E(3),.85f,RuneBoardSkin.Muted,TextAnchor.MiddleCenter);
        }
        string ShapeName(RunePiece piece)=>piece.Shape.Size==1?T("shape.seed"):piece.Shape.Size==2?T("shape.pair"):Loc.F("모양 {0}",piece.Shape.ShapeNumber);
        static string AbilityLine(RuneBoardCell c)=>c.Ability.Meaning=="root"?Loc.T("공용 시작점"):RuneMasteryCatalog.AbilityName(c.Ability.Meaning)+(c.IsElite?"":" +"+c.Value.ToString("0.##")+Loc.T(c.Unit));
        void DrawInspector()
        {
            float w=inspector.rect.width,h=inspector.rect.height,pad=E(.6f),top=landscape?E(2.2f):E(.45f),tile=E(landscape?3.9f:3.9f),x=pad+tile+E(.6f),tw=w-x-pad;
            if(landscape){Txt(inspector,"03",pad,0,E(1.3f),top,.7f,RuneBoardSkin.Gold,display:true);Txt(inspector,T("insp.title"),pad+E(1.5f),0,w-E(5),top,.95f,RuneBoardSkin.GoldBright,TextAnchor.MiddleLeft,true,display:true);}
            IconButton(inspector,"rune-clear-selection","close",w-E(1.7f),E(.15f),E(1.4f),()=>{Session.ClearSelection();RequestRefresh();});
            var selected=Session.Selected;
            if(selected!=null)
            {
                var box=Panel(inspector,"Rune detail picture",pad,top,tile,tile,RuneBoardSkin.Inset);var art=RuneGemView.Create(box,"rune-detail-pickup");art.Icon(selected,Session.Rotation);
                var drag=box.gameObject.AddComponent<RuneBoardStoragePointer>();box.GetComponent<Image>().raycastTarget=true;drag.Owner=this;drag.Piece=selected;drag.Source=art;
                Txt(inspector,RuneV13Catalog.TypeName(selected.Type)+" · "+ShapeName(selected),x,top,tw-E(1),E(1.3f),1.02f,RuneBoardSkin.GoldBright,TextAnchor.MiddleLeft,true);
                var nodes=Session.Effects().Nodes.Where(n=>n.CoveringRuneId==selected.InstanceId).ToArray();string where=Session.SelectedPlaced?T("insp.lit",nodes.Count(n=>n.IsPreviewActive))+" · "+T("insp.dim",nodes.Count(n=>!n.IsPreviewActive)):T("insp.stored");
                Txt(inspector,T("storage.cells",selected.Shape.Size)+" · G"+selected.Grade+" · "+Session.Rotation*60+"° · "+where,x,top+E(1.4f),tw,E(1.25f),.72f,RuneBoardSkin.Muted);
                float by=landscape?h-E(2.9f):top+E(3),bh=E(2.2f),bx=landscape?pad:x,bw=landscape?(w-pad*3)/2:(tw-E(.3f))/2;
                ActionButton(inspector,"rune-rotate",T("insp.rotate"),"rotate",bx,by,bw,bh,Rotate).interactable=!Session.Viewing;
                if(Session.SelectedPlaced)ActionButton(inspector,"rune-remove",T("insp.recall"),"recall",bx+bw+E(.3f),by,bw,bh,Remove,true).interactable=!Session.Viewing;
                if(landscape)Txt(inspector,T("insp.someDim"),pad,top+tile+E(.3f),w-pad*2,Mathf.Max(E(1),by-top-tile-E(.5f)),.7f,RuneBoardSkin.Muted);
                return;
            }
            if(Session.SelectedCell.HasValue&&Session.Editor.Board.TryGetCell(Session.SelectedCell.Value,out var cell))
            {
                var node=Session.Effects().Nodes.First(n=>n.Cell.Coordinate.Equals(cell.Coordinate));bool open=Session.Editor.Board.IsOpen(cell,6);
                var box=Panel(inspector,"Rune detail picture",pad,top,tile,tile,RuneBoardSkin.Inset);RuneGemView.Create(box,"Rune ability medal").Slot(cell,node.IsPreviewActive);
                var title=Txt(inspector,AbilityLine(cell),x,top,tw-E(.9f),E(1.5f),.94f,RuneBoardSkin.GoldBright,TextAnchor.MiddleLeft,true);title.name="rune-detail-title";title.resizeTextForBestFit=true;title.resizeTextMinSize=Z(.75f);title.resizeTextMaxSize=Z(.94f);title.horizontalOverflow=HorizontalWrapMode.Overflow;
                var covering=node.CoveringRuneId==null?null:Session.Inventory.FindOwned(node.CoveringRuneId);
                bool mismatch=covering!=null&&!node.IsPreviewActive;
                string status=!open?T(cell.RegionGrade>0&&!Session.Progress.order.Contains(cell.RegionGrade)?"slot.sealed":"slot.closed"):
                    mismatch?T("slot.dim")+" · "+RuneV13Catalog.TypeName(covering.Type)+" ≠ "+RuneV13Catalog.TypeName(cell.AbilityType):"";
                float emblem=cell.AbilityType<0?0:E(1.1f);
                if(cell.AbilityType>=0)RuneBoardArt.CategoryImage(inspector,cell.AbilityType,x,top+E(1.45f),E(1));
                Txt(inspector,RuneV13Catalog.RegionName(cell.RegionGrade)+" · "+(cell.Ability.Meaning=="root"?T("slot.root"):cell.IsElite?T("type.elite"):T("slot.tier",cell.NumericGrade))+(status!=""?" · "+status:""),x+emblem,top+E(1.55f),tw-emblem,E(1.2f),.66f,mismatch?RuneBoardSkin.Danger:RuneBoardSkin.Muted);
                if(!landscape)
                {
                    var description=Txt(inspector,Loc.T(RuneV13Catalog.ForMeaning(cell.Ability.Meaning).description),x,top+E(2.65f),tw,E(.85f),.6f,RuneBoardSkin.Muted);
                    description.horizontalOverflow=HorizontalWrapMode.Wrap;
                }
                float by=landscape?h-E(2.9f):top+E(3.7f),bx=landscape?pad:x,bw=landscape?(w-pad*3)/2:(tw-E(.3f))/2;
                Btn(inspector,"rune-region-preview",T("region.info"),bx,by,bw,E(2.2f),()=>{modalRegion=cell.RegionGrade;OpenDialog("region-info");});
                if(!open)Btn(inspector,"rune-unlock",T("slot.open"),bx+bw+E(.3f),by,bw,E(2.2f),()=>Unlock(cell.Coordinate),true).interactable=!Session.Viewing&&RuneMasteryProgress.CanUnlock(Session.Progress,cell.Coordinate);
                else if(cell.Ability.Meaning=="root")Btn(inspector,"rune-next-region",T("region.next"),bx+bw+E(.3f),by,bw,E(2.2f),()=>OpenDialog("regions"));
                if(landscape)Txt(inspector,Loc.T(RuneV13Catalog.ForMeaning(cell.Ability.Meaning).description),pad,top+tile+E(.2f),w-pad*2,Mathf.Max(E(1),by-top-tile-E(.4f)),.72f,RuneBoardSkin.Muted);
                return;
            }
            DrawEmptyInspector(top,w,h);
        }
        void DrawEmptyInspector(float top,float w,float h)
        {
            float y=landscape?top:E(.45f),pad=E(.6f);Txt(inspector,T("insp.emptyTitle"),pad,y,w-E(2.5f),E(1.3f),1,RuneBoardSkin.GoldBright,TextAnchor.MiddleLeft,true,display:true);y+=E(1.4f);
            var sample=Session.Editor.Board.Cells.First(c=>c.AbilityType==0);string[] states={"lit","dim","open","unlock","closed","anchor"};
            for(int i=0;i<6;i++){float sw=(w-pad*2)/6;var r=UiLayout.Rect("Legend "+states[i],inspector);UiLayout.Place(r,pad+i*sw,y,sw,E(2));RuneGemView.Create(r,"Gem swatch").Slot(sample,i==0,states[i]);Txt(inspector,T("legend."+(states[i]=="open"?"empty":states[i]=="unlock"?"can":states[i])),pad+i*sw,y+E(2),sw,E(.8f),.54f,RuneBoardSkin.Muted,TextAnchor.MiddleCenter);}
            Btn(inspector,"rune-practice",T("tutorial.open"),pad,h-E(2.2f),(w-pad*3)*.65f,E(1.9f),()=>OpenDialog("tutorial"));Btn(inspector,"rune-guide",T("view.legend"),pad*2+(w-pad*3)*.65f,h-E(2.2f),(w-pad*3)*.35f,E(1.9f),()=>OpenDialog("legend"));
        }
    }
}
