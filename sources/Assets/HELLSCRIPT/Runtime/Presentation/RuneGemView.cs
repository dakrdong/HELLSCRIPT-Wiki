using System.Collections.Generic;
using System.Linq;
using Hellscript.Runes;
using UnityEngine;
namespace Hellscript
{
    // Shared slab renderer for the board, storage, inspector, drag image, legend and practice.
    public sealed class RuneGemView:MonoBehaviour
    {
        readonly Dictionary<string,RuneGemMesh> layers=new Dictionary<string,RuneGemMesh>();
        RectTransform geometry;public RectTransform Geometry=>geometry;
        public float Scale {get;private set;}=1;public Vector2 CameraCenter;
        public RectTransform Rect=>(RectTransform)transform;
        static Color C(string s)=>RuneBoardArt.Color(s);
        static Color Alpha(Color c,float a){c.a*=a;return c;}
        public static RuneGemView Create(UnityEngine.Transform parent,string name)
        {var r=UiLayout.Rect(name,parent);UiLayout.Stretch(r);var view=r.gameObject.AddComponent<RuneGemView>();view.geometry=UiLayout.Rect("Gem geometry",r);view.geometry.anchorMin=view.geometry.anchorMax=view.geometry.pivot=new Vector2(.5f,.5f);view.geometry.sizeDelta=new Vector2(2000,2000);return view;}
        RuneGemMesh Layer(int order,string texture="")
        {
            string key=order.ToString("D2")+"/"+texture;
            if(!layers.TryGetValue(key,out var layer))
            {var r=UiLayout.Rect(key,geometry);UiLayout.Stretch(r);layer=r.gameObject.AddComponent<RuneGemMesh>();layer.raycastTarget=false;layer.texture=texture==""?RuneV13Art.Atlas:RuneBoardArt.Texture(texture);layers[key]=layer;}
            layer.gameObject.SetActive(true);return layer;
        }
        void Begin(){foreach(var layer in layers.Values){layer.ResetMesh();layer.gameObject.SetActive(false);}}
        void End(){int n=0;foreach(var p in layers.OrderBy(p=>p.Key,System.StringComparer.Ordinal)){p.Value.transform.SetSiblingIndex(n++);p.Value.Finish();}}
        public void SetCamera(Vector2 center,float scale){CameraCenter=center;Scale=scale;geometry.localScale=Vector3.one*scale;geometry.anchoredPosition=-center*scale;}
        public void Fit(IEnumerable<HexCell> cells,float pad=0)
        {var ps=cells.Select(RuneGemGeometry.Pixel).ToArray();var min=new Vector2(ps.Min(p=>p.x)-38,ps.Min(p=>p.y)-40);var max=new Vector2(ps.Max(p=>p.x)+38,ps.Max(p=>p.y)+40);SetCamera((min+max)/2,Mathf.Clamp(Mathf.Min((Rect.rect.width-pad)/(max.x-min.x),(Rect.rect.height-pad)/(max.y-min.y)),.18f,2.4f));}
        public Vector2 ScreenCell(HexCell c)=>RectTransformUtility.WorldToScreenPoint(null,geometry.TransformPoint(RuneGemGeometry.Pixel(c)));
        public Vector2 LocalBoard(Vector2 screen){RectTransformUtility.ScreenPointToLocalPointInRectangle(geometry,screen,null,out var p);return p;}
        public HexCell CellAt(Vector2 screen)
        {var p=LocalBoard(screen);float q=p.x/36,r=-p.y/41.5692194f-q*.5f,s=-q-r;int a=Mathf.RoundToInt(q),b=Mathf.RoundToInt(r),c=Mathf.RoundToInt(s);if(Mathf.Abs(a-q)>Mathf.Abs(b-r)&&Mathf.Abs(a-q)>Mathf.Abs(c-s))a=-b-c;else if(Mathf.Abs(b-r)>Mathf.Abs(c-s))b=-a-c;return new HexCell(a,b);}
        void Tile(string texture,HexCell cell,Vector2 offset,float radius=24,float alpha=1,int order=0)
        {var center=RuneGemGeometry.Pixel(cell)+offset;if(texture=="Tiles/bed-socket-a"||texture=="Tiles/bed-slab-a"||texture=="Tiles/bed-slab-b")Layer(order-1).Hex(center,radius*.97f,new Color(.027f,.027f,.027f,alpha));float shade=RuneBoardArt.IsPaintedDim(texture)?.72f:1;Layer(order,texture).Hex(center,radius,new Color(shade,shade,shade,alpha),RuneBoardArt.UV(texture));}
        void Glyph(HexCell cell,string name,Color color,float size=21,Vector2 offset=default,int order=2)
        {Layer(order).Quad(RuneGemGeometry.Pixel(cell)+offset,Vector2.one*size,color,RuneV13Art.Glyph("b-"+name));}
        void LitGlyph(HexCell cell,string name,int type,bool elite,float alpha=1)
        {
            var material=RuneBoardArt.Palette(type,elite);var dark=C(material.dark);bool bright=elite||type<0||type==2||type==4;var baseColor=bright?new Color(dark.r*.45f,dark.g*.45f,dark.b*.45f,.94f*alpha):Alpha(C(material.bright),.36f*alpha);
            for(int i=0;i<8;i++){float a=i*Mathf.PI/4;Glyph(cell,name,baseColor,23,new Vector2(Mathf.Cos(a),Mathf.Sin(a))*(bright?1.2f:1.5f)+Vector2.up*3,10);}
            Glyph(cell,name,Alpha(C("#fffdf4"),alpha),23,Vector2.up*3,11);
        }
        void Engraved(HexCell cell,string name,int type,float alpha)
        {var material=RuneBoardArt.Palette(type);Glyph(cell,name,Alpha(C(material.bright),.5f*alpha),21,new Vector2(.5f,2.2f),10);var dark=C(material.dark);Glyph(cell,name,new Color(dark.r*.7f,dark.g*.7f,dark.b*.7f,.98f*alpha),21,Vector2.up*3,11);}
        public void Board(RuneBoardSession session,RuneCandidate preview=null,string hidden=null,int focus=0)
        {
            Begin();var board=session.Editor.Board;var placements=session.Editor.DraftPlacements.Where(p=>p.InstanceId!=hidden).ToArray();var occupied=placements.SelectMany(p=>p.OccupiedCells.Select(c=>(c,p))).ToDictionary(v=>v.c,v=>v.p);var valid=session.Editor.ValidateDraft();
            foreach(var cell in board.Cells)
            {
                bool open=board.IsOpen(cell,6),sealedRegion=cell.RegionGrade>0&&!session.Progress.order.Contains(cell.RegionGrade),unlock=RuneMasteryProgress.CanUnlock(session.Progress,cell.Coordinate);var pos=RuneGemGeometry.Pixel(cell.Coordinate);var mat=RuneBoardArt.Palette(cell.AbilityType,cell.IsElite);
                Tile("Tiles/"+(open?"bed-socket-a":sealedRegion?"bed-slab-a":"bed-slab-b"),cell.Coordinate,Vector2.zero);
                var ink=C(mat.ink);if(open)Layer(1).Outline(RuneGemGeometry.Hex(pos,20.8f),cell.IsElite?1.5f:1.15f,Alpha(ink,cell.IsElite?.75f:.5f));
                if(unlock)Layer(1).Outline(RuneGemGeometry.Hex(pos,21),1.3f,C("#f2d9a4"),true);
                if(!occupied.ContainsKey(cell.Coordinate)&&cell.Ability.Meaning!="root")Glyph(cell.Coordinate,cell.Glyph,open?Alpha(ink,.9f):unlock?Alpha(C("#d9c79c"),.8f):Alpha(C(sealedRegion?"#7c776d":"#8e8576"),sealedRegion?.2f:.34f));
            }
            for(int region=0;region<7;region++)foreach(var loop in RuneGemGeometry.Contours(board.Cells.Where(c=>c.RegionGrade==region).Select(c=>c.Coordinate).ToArray()))
            {bool open=region==0||session.Progress.order.Contains(region);var poly=RuneGemGeometry.Inset(loop,1);float w=Mathf.Clamp(2.1f*Scale,1.2f,2.8f)/Scale;Layer(2).Outline(poly,w+2/Scale,RuneBoardSkin.Border);Layer(3).Outline(poly,w,open?(region==focus?RuneBoardSkin.Accent:RuneBoardSkin.Gold):(region==focus?RuneBoardSkin.Faint:RuneBoardSkin.Muted));}
            foreach(var p in placements)Piece(p.OccupiedCells,p.Piece.Type,session.SelectedId==p.InstanceId,false,false,board,valid.IsRuneValid(p.InstanceId));
            var root=board.Cells.First(c=>c.Ability.Meaning=="root");Piece(new[]{root.Coordinate},-1,false,false,false,board);
            foreach(var a in session.LegalAnchors())
            {var pos=RuneGemGeometry.Pixel(a.Cell);if(a.Perfect){Layer(12).Circle(pos,7.4f,Alpha(C("#f2d9a4"),.16f));Ring(pos,7.4f,1.6f,C("#f2d9a4"));}Layer(12).Circle(pos,4.7f,C("#0b1d0a"));Layer(12).Circle(pos,3.7f,C("#8fd47a"));}
            if(session.SelectedCell.HasValue)Layer(12).Outline(RuneGemGeometry.Hex(RuneGemGeometry.Pixel(session.SelectedCell.Value),22.6f),1.8f,C("#f2d9a4"));
            if(preview?.Placement!=null)Piece(preview.Placement.OccupiedCells,preview.Placement.Piece.Type,false,true,!preview.Ok,board,preview.Ok);
            End();
        }
        void Ring(Vector2 p,float radius,float width,Color color)
        {var poly=Enumerable.Range(0,32).Select(i=>p+new Vector2(Mathf.Cos(i*Mathf.PI/16),Mathf.Sin(i*Mathf.PI/16))*radius).ToArray();Layer(12).Outline(poly,width,color);}
        void Piece(IReadOnlyList<HexCell> cells,int type,bool selected,bool ghost,bool invalid,RuneBoardDefinition board=null,bool valid=true,float alpha=1,bool forceDim=false,System.Func<HexCell,bool> litFor=null)
        {
            var mat=RuneBoardArt.Palette(type);float opacity=alpha*(ghost?(invalid?.8f:.88f):1);var offset=Vector2.up*3;var loops=RuneGemGeometry.Contours(cells);
            foreach(var loop in loops)
            {
                var body=RuneGemGeometry.Inset(loop,1.1f);var top=body.Select(p=>p+offset).ToArray();
                if(!ghost){Layer(4).Polygon(body.Select(p=>p+new Vector2(1.2f,-7.4f)).ToArray(),new Color(0,0,0,.55f*alpha),new Color(0,0,0,.55f*alpha));Layer(4).Outline(body.Select(p=>p+new Vector2(1.2f,-7.4f)).ToArray(),4,new Color(0,0,0,.12f*alpha));Layer(5).Polygon(body.Select(p=>p-Vector2.up*2).ToArray(),Alpha(C(mat.dark),alpha),Alpha(C("#090706"),alpha));}
                Layer(6).Polygon(top,Alpha(C(mat.bright),opacity),Alpha(C(mat.dark),opacity));
            }
            foreach(var c in cells)
            {
                RuneBoardCell cell=null;board?.TryGetCell(c,out cell);bool root=type<0,elite=cell?.IsElite==true,lit=!forceDim&&valid&&(board==null||root||elite||cell.AbilityType==type);if(litFor!=null)lit&=litFor(c);var face=RuneBoardArt.Palette(type,elite);var pos=RuneGemGeometry.Pixel(c)+offset;
                Tile(RuneBoardArt.Gem(type,elite,!lit),c,offset,23.15f,opacity,7);
                if(lit&&board!=null){var bright=C(face.bright);Layer(8).Circle(pos,19,Alpha(bright,.22f*opacity),Alpha(bright,0));}
                if(!lit)Layer(8).Outline(RuneGemGeometry.Hex(pos,22.7f),1.6f,Alpha(C(face.ink),.95f*opacity));
                if(invalid)Layer(8).Hex(pos,23.3f,Alpha(C("#8a2216"),.4f*opacity));
                if(cell!=null){if(lit)LitGlyph(c,cell.Glyph,type,elite,opacity);else Engraved(c,cell.Glyph,type,opacity);}
                if(elite)Layer(12).Outline(RuneGemGeometry.Hex(pos,21.4f),1.1f,Alpha(C("#ffe7a8"),(lit?.85f:.35f)*opacity));
            }
            foreach(var loop in loops)
            {
                var top=RuneGemGeometry.Inset(loop,1.1f).Select(p=>p+offset).ToArray();
                Layer(9).Outline(top.Select(p=>p+new Vector2(-.5f,.6f)).ToArray(),1,Alpha(C(mat.bright),.55f*opacity));
                Layer(9).Outline(RuneGemGeometry.Inset(loop,2.4f).Select(p=>p+offset).ToArray(),1.2f,Alpha(C(mat.ink),.85f*opacity));
                Layer(9).Outline(top,ghost?1.9f:2,Alpha(C(ghost?(invalid?"#e5826c":mat.bright):"#080605"),opacity),ghost);
                if(selected){Layer(12).Outline(top,6,Alpha(C("#f2d9a4"),.35f));Layer(12).Outline(top,2.6f,C("#f2d9a4"));}
            }
            if(ghost){var p=RuneGemGeometry.Pixel(cells[0])+offset;Layer(14).Circle(p,4.7f,Alpha(C("#0b1d0a"),opacity));Layer(14).Circle(p,3.7f,Alpha(C("#8fd47a"),opacity));}
        }
        public void Icon(RunePiece piece,int rotation,bool selected=false,float alpha=1,string state="stored")
        {Begin();var cells=piece.Shape.RotatedCells(rotation);Piece(cells,piece.Type,selected,state=="preview"||state=="invalid",state=="invalid",null,true,alpha,state=="dim");End();FitIcon(cells);}
        void FitIcon(IReadOnlyList<HexCell> cells)
        {
            var p=cells.Select(RuneGemGeometry.Pixel).ToArray();var min=new Vector2(p.Min(v=>v.x)-27,p.Min(v=>v.y)-28);var max=new Vector2(p.Max(v=>v.x)+27,p.Max(v=>v.y)+27);
            SetCamera((min+max)/2,Mathf.Min((Rect.rect.width-4)/(max.x-min.x),(Rect.rect.height-4)/(max.y-min.y)));
        }
        public void Practice(RunePracticeModel model,RunePlacement preview=null,string hidden=null)
        {
            Begin();foreach(var c in model.BoardCells){Tile("Tiles/bed-socket-a",c,Vector2.zero);Layer(1).Outline(RuneGemGeometry.Hex(RuneGemGeometry.Pixel(c),21),1,C("#7a6444"));}
            foreach(var p in model.FixedPlacements.Concat(model.Placements.Values).Where(p=>p.InstanceId!=hidden))Piece(p.OccupiedCells,p.Piece.Type,p.InstanceId==model.SelectedId,false,false,litFor:c=>!model.AbilityGrades.TryGetValue(c,out var t)||t==p.Piece.Type);
            foreach(var a in model.AbilityGrades){string glyph=RuneV13Catalog.Data.abilities.First(v=>v.color==RuneV13Catalog.TypeIds[a.Value]).glyph;var cover=model.Placements.Values.FirstOrDefault(p=>p.OccupiedCells.Contains(a.Key));if(model.IsAbilityActive(a.Key))LitGlyph(a.Key,glyph,a.Value,false);else if(cover!=null)Engraved(a.Key,glyph,cover.Piece.Type,1);else Glyph(a.Key,glyph,C(RuneBoardArt.Palette(a.Value).ink));}
            if(model.SelectedId!=null&&(!model.Placements.ContainsKey(model.SelectedId)||hidden!=null))foreach(var c in model.GetCandidateCells(model.SelectedId)){Layer(12).Circle(RuneGemGeometry.Pixel(c),3.4f,C("#8fd47a"));}
            if(preview!=null)Piece(preview.OccupiedCells,preview.Piece.Type,false,true,!model.IsValid(preview));End();
        }
        public void Slot(RuneBoardCell cell,bool lit=false,string state="open")
        {
            if(cell.Ability.Meaning=="root"&&state=="open")lit=true;
            Begin();var zero=new HexCell(0,0);Tile("Tiles/"+(state=="sealed"?"bed-slab-a":state=="closed"||state=="unlock"?"bed-slab-b":"bed-socket-a"),zero,Vector2.zero);
            if(lit||state=="dim"||state=="stored"||state=="selected"||state=="preview"||state=="invalid"){Tile(RuneBoardArt.Gem(cell.AbilityType,cell.IsElite,state=="dim"),zero,Vector2.up*3,23,1,7);if(lit)LitGlyph(zero,cell.Glyph,cell.AbilityType,cell.IsElite);else if(state=="dim")Engraved(zero,cell.Glyph,cell.AbilityType,1);}
            else Glyph(zero,cell.Glyph,Alpha(C(RuneBoardArt.Palette(cell.AbilityType,cell.IsElite).ink),state=="sealed"?.2f:state=="closed"?.34f:.9f));
            if(state=="selected")Layer(12).Outline(RuneGemGeometry.Hex(Vector2.up*3,23),2.6f,RuneBoardSkin.Accent);
            if(state=="preview"||state=="invalid")Layer(12).Outline(RuneGemGeometry.Hex(Vector2.up*3,23),1.9f,state=="invalid"?RuneBoardSkin.Danger:RuneBoardSkin.Accent,true);
            if(state=="invalid")Layer(12).Hex(Vector2.up*3,22,new Color(.54f,.13f,.08f,.4f));
            if(state=="unlock")Layer(12).Outline(RuneGemGeometry.Hex(Vector2.zero,21),1.3f,C("#f2d9a4"),true);
            if(state=="anchor"){Ring(Vector2.zero,7.4f,1.6f,C("#f2d9a4"));Layer(12).Circle(Vector2.zero,3.7f,C("#8fd47a"));}
            End();Fit(new[]{zero});
        }
    }
}
