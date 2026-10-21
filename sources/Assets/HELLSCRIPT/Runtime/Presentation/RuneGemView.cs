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
        // ---- the casual rune board look (HELLSCRIPT 룬 보드 · 캐주얼): flat beds with a ring and a glyph, raised blocks with the ink outline
        static Color Ink=>RuneCasual.Ink;
        static Color Fill(int type,bool elite=false)=>RuneCasual.Fill[RuneCasual.Slot(type,elite)];
        static Color RingInk(int type,bool elite=false)=>RuneCasual.RingInk[RuneCasual.Slot(type,elite)];
        static Color Wall(int type,bool elite=false)=>RuneCasual.Dark[RuneCasual.Slot(type,elite)];
        const float Lift=3,Depth=5,BedRadius=23.4f;
        void Tile(string texture,HexCell cell,Vector2 offset,float radius=24,float alpha=1,int order=0)
        {var center=RuneGemGeometry.Pixel(cell)+offset;float shade=RuneBoardArt.IsPaintedDim(texture)?.72f:1;Layer(order,texture).Hex(center,radius,new Color(shade,shade,shade,alpha),RuneBoardArt.UV(texture));}
        void Glyph(HexCell cell,string name,Color color,float size=21,Vector2 offset=default,int order=2)
        {Layer(order).Quad(RuneGemGeometry.Pixel(cell)+offset,Vector2.one*size,color,RuneV13Art.Glyph("b-"+name));}
        // A lit glyph is white with an ink under-stroke, a dim one is engraved: a white lip and the dark of the block.
        void LitGlyph(HexCell cell,string name,int type,bool elite,float alpha=1,float size=23,Vector2? at=null)
        {
            var shift=at??Vector2.up*Lift;
            for(int i=0;i<8;i++){float a=i*Mathf.PI/4;Glyph(cell,name,Alpha(Ink,.94f*alpha),size,new Vector2(Mathf.Cos(a),Mathf.Sin(a))*1.45f+shift,10);}
            Glyph(cell,name,Alpha(Color.white,alpha),size,shift,11);
        }
        void Engraved(HexCell cell,string name,int type,float alpha)
        {Glyph(cell,name,new Color(1,1,1,.6f*alpha),21,new Vector2(.5f,Lift+.8f),10);var dark=Wall(type);dark.a=.9f*alpha;Glyph(cell,name,dark,21,Vector2.up*Lift,11);}
        void Bed(HexCell cell,string status)
        {
            var pos=RuneGemGeometry.Pixel(cell);Color fill=status=="sealed"?RuneCasual.BedSealed:status=="closed"?RuneCasual.BedClosed:RuneCasual.BedOpen,edge=status=="sealed"?RuneCasual.BedSealedEdge:status=="closed"?RuneCasual.BedClosedEdge:RuneCasual.BedOpenEdge;
            Layer(0).Hex(pos,BedRadius,fill);Layer(1).Outline(RuneGemGeometry.Hex(pos,BedRadius),1.2f,edge);
        }
        void Floor(RuneBoardCell cell,string status,bool can,bool glyph=true,HexCell? at=null)
        {
            var spot=at??cell.Coordinate;var pos=RuneGemGeometry.Pixel(spot);int type=cell.AbilityType;bool elite=cell.IsElite;Bed(spot,status);
            if(status=="open")
            {
                if(elite){HexRing(pos,20.8f,1.6f,Alpha(RuneCasual.EliteInk,.85f));HexRing(pos,17.4f,.9f,Alpha(RuneCasual.Amber,.5f));}else HexRing(pos,20.8f,1.3f,Alpha(RingInk(type),.7f));
                if(glyph)Glyph(spot,cell.Glyph,Alpha(elite?RuneCasual.OrangeInk:RingInk(type),.9f),21);
            }
            else if(status=="closed")
            {
                if(can){Layer(1).Hex(pos,BedRadius-3,Alpha(RuneCasual.Yellow,.22f));Layer(1).Outline(RuneGemGeometry.Hex(pos,BedRadius-3),1.4f,RuneCasual.Orange,true);}
                if(glyph)Glyph(spot,cell.Glyph,Alpha(can?RuneCasual.OrangeInk:RuneCasual.Edge,can?.85f:.5f),20);
            }
            else if(status=="sealed"){if(glyph)Glyph(spot,cell.Glyph,Alpha(RuneCasual.Edge,.25f),19);}
            else HexRing(pos,BedRadius-3,1.4f,Alpha(RuneCasual.Edge,.7f));
        }
        public void Board(RuneBoardSession session,RuneCandidate preview=null,string hidden=null,int focus=0)
        {
            Begin();var board=session.Editor.Board;var placements=session.Editor.DraftPlacements.Where(p=>p.InstanceId!=hidden).ToArray();var occupied=placements.SelectMany(p=>p.OccupiedCells.Select(c=>(c,p))).ToDictionary(v=>v.c,v=>v.p);var valid=session.Editor.ValidateDraft();
            foreach(var cell in board.Cells)
            {
                bool open=board.IsOpen(cell,6),sealedRegion=cell.RegionGrade>0&&!session.Progress.order.Contains(cell.RegionGrade),unlock=RuneMasteryProgress.CanUnlock(session.Progress,cell.Coordinate);
                string status=cell.Ability.Meaning=="root"?"root":open?"open":sealedRegion?"sealed":"closed";
                Floor(cell,status,unlock,!occupied.ContainsKey(cell.Coordinate)&&cell.Ability.Meaning!="root");
            }
            float line=Mathf.Clamp(2.1f*Scale,1.2f,2.8f)/Scale,shell=2/Scale;
            for(int region=0;region<7;region++)foreach(var loop in RuneGemGeometry.Contours(board.Cells.Where(c=>c.RegionGrade==region).Select(c=>c.Coordinate).ToArray()))
            {
                bool open=region==0||session.Progress.order.Contains(region),focused=region==focus;var poly=RuneGemGeometry.Inset(loop,1);
                Layer(2).Outline(poly,line+shell,Alpha(Ink,open?.9f:.35f));
                Layer(3).Outline(poly,line,open?(focused?RuneCasual.Amber:StorageSurface.Hex("93bfe3")):(focused?RuneCasual.Gray:RuneCasual.DisabledEdge),!open);
            }
            foreach(var p in placements)Piece(p.OccupiedCells,p.Piece.Type,session.SelectedId==p.InstanceId,false,false,board,valid.IsRuneValid(p.InstanceId),emblem:true);
            var root=board.Cells.First(c=>c.Ability.Meaning=="root");Piece(new[]{root.Coordinate},-1,false,false,false,null,true,1,false,null,2.4f,4f);
            Glyph(root.Coordinate,"root",Ink,27+5,Vector2.up*2.4f,13);Glyph(root.Coordinate,"root",Color.white,27,Vector2.up*2.4f,14);
            foreach(var a in session.LegalAnchors())
            {var pos=RuneGemGeometry.Pixel(a.Cell);if(a.Perfect){Layer(12).Circle(pos,7.4f,Alpha(RuneCasual.Yellow,.3f));Ring(pos,7.4f,1.6f,RuneCasual.OrangeInk);}Layer(12).Circle(pos,4.9f,Ink);Layer(12).Circle(pos,3.7f,RuneCasual.Green);}
            if(session.SelectedCell.HasValue){var at=RuneGemGeometry.Pixel(session.SelectedCell.Value);Layer(12).Outline(RuneGemGeometry.Hex(at,BedRadius-1.4f),4,Alpha(RuneCasual.Yellow,.95f));Layer(12).Outline(RuneGemGeometry.Hex(at,BedRadius-1.4f),1.6f,Ink);}
            if(preview?.Placement!=null)Piece(preview.Placement.OccupiedCells,preview.Placement.Piece.Type,false,true,!preview.Ok,board,preview.Ok);
            End();
        }
        void HexRing(Vector2 p,float radius,float width,Color color,int layer=1,bool dashed=false)=>Layer(layer).Outline(RuneGemGeometry.Hex(p,radius),width,color,dashed);
        void Ring(Vector2 p,float radius,float width,Color color,int layer=12)
        {var poly=Enumerable.Range(0,32).Select(i=>p+new Vector2(Mathf.Cos(i*Mathf.PI/16),Mathf.Sin(i*Mathf.PI/16))*radius).ToArray();Layer(layer).Outline(poly,width,color);}
        void Piece(IReadOnlyList<HexCell> cells,int type,bool selected,bool ghost,bool invalid,RuneBoardDefinition board=null,bool valid=true,float alpha=1,bool forceDim=false,System.Func<HexCell,bool> litFor=null,float lift=Lift,float depth=Depth,bool emblem=false,string glyphName=null,bool? litOverride=null)
        {
            float opacity=alpha*(ghost?(invalid?.82f:.9f):1);var up=Vector2.up*lift;var loops=RuneGemGeometry.Contours(cells);var fill=Fill(type);var ink=RingInk(type);
            foreach(var loop in loops)
            {
                var body=RuneGemGeometry.Inset(loop,1.1f);var top=body.Select(p=>p+up).ToArray();
                if(!ghost)
                {
                    var cast=body.Select(p=>p+new Vector2(1.2f,-(depth+2.4f))).ToArray();Layer(4).Polygon(cast,Alpha(Ink,.22f*alpha),Alpha(Ink,.22f*alpha));
                    var wall=body.Select(p=>p-Vector2.up*(depth-lift)).ToArray();Layer(5).Polygon(wall,Alpha(Wall(type),alpha),Alpha(Wall(type),alpha));Layer(5).Outline(wall,1.4f,Alpha(Ink,alpha));
                }
                Layer(6).Polygon(top,Alpha(fill,opacity),Alpha(fill,opacity));
            }
            foreach(var c in cells)
            {
                RuneBoardCell cell=null;board?.TryGetCell(c,out cell);bool root=type<0,elite=cell?.IsElite==true,lit=!forceDim&&valid&&(board==null||root||elite||cell.AbilityType==type);if(litFor!=null)lit&=litFor(c);if(litOverride.HasValue)lit=litOverride.Value;var pos=RuneGemGeometry.Pixel(c)+up;
                Tile(RuneBoardArt.Gem(type,elite,!lit),c,up,23.15f,opacity,7);
                if(lit&&(board!=null||glyphName!=null)){Layer(8).Circle(pos,19,new Color(1,1,1,.22f*opacity),new Color(1,1,1,0));}
                if(!lit&&(board!=null||glyphName!=null))Layer(8).Outline(RuneGemGeometry.Hex(pos,22.6f),1.9f,Alpha(RingInk(type,elite),(ghost?.8f:1f)*opacity));
                if(invalid)Layer(8).Hex(pos,24.4f,Alpha(RuneCasual.Coral,.35f*opacity));
                string glyph=cell?.Glyph??glyphName;
                if(glyph!=null&&(board!=null||glyphName!=null)){if(lit)LitGlyph(c,glyph,type,elite,opacity);else Engraved(c,glyph,type,opacity);}
                if(elite)Layer(12).Outline(RuneGemGeometry.Hex(pos,21.4f),1.3f,Alpha(RuneCasual.Yellow,(lit?.95f:.45f)*opacity));
            }
            foreach(var loop in loops)
            {
                var top=RuneGemGeometry.Inset(loop,1.1f).Select(p=>p+up).ToArray();
                Layer(9).Outline(RuneGemGeometry.Inset(loop,2.4f).Select(p=>p+up+new Vector2(-.5f,.6f)).ToArray(),1,new Color(1,1,1,(ghost?.5f:.6f)*opacity));
                Layer(9).Outline(RuneGemGeometry.Inset(loop,2.4f).Select(p=>p+up).ToArray(),1.2f,Alpha(ink,(ghost?.5f:.8f)*opacity));
                Layer(9).Outline(top,ghost?2f:2.2f,Alpha(invalid?RuneCasual.Coral:Ink,opacity),ghost);
                if(selected){Layer(12).Outline(top,6.5f,Alpha(RuneCasual.Yellow,.95f));Layer(12).Outline(top,2.2f,Ink);}
            }
            if(emblem&&cells.Count>0&&type>=0&&type<5){var p0=RuneGemGeometry.Pixel(cells[0])+up+new Vector2(-24*.62f+4.5f,41.569f*.46f-4.5f);Layer(13,"Casual/Pictos/pic-emblem-"+(type==3?"critical":RuneBoardArt.Types[type])).Quad(p0,Vector2.one*9,new Color(1,1,1,.95f),new Rect(0,0,1,1));}
            if(ghost){var p=RuneGemGeometry.Pixel(cells[0])+up;Layer(14).Circle(p,4.9f,Alpha(Ink,opacity));Layer(14).Circle(p,3.7f,Alpha(RuneCasual.Green,opacity));}
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
            Begin();foreach(var c in model.BoardCells){Bed(c,"open");HexRing(RuneGemGeometry.Pixel(c),20.8f,1,RuneCasual.Edge);}
            foreach(var p in model.FixedPlacements.Concat(model.Placements.Values).Where(p=>p.InstanceId!=hidden))Piece(p.OccupiedCells,p.Piece.Type,p.InstanceId==model.SelectedId,false,false,litFor:c=>!model.AbilityGrades.TryGetValue(c,out var t)||t==p.Piece.Type);
            foreach(var a in model.AbilityGrades){string glyph=RuneV13Catalog.Data.abilities.First(v=>v.color==RuneV13Catalog.TypeIds[a.Value]).glyph;var cover=model.Placements.Values.FirstOrDefault(p=>p.OccupiedCells.Contains(a.Key));if(model.IsAbilityActive(a.Key))LitGlyph(a.Key,glyph,a.Value,false);else if(cover!=null)Engraved(a.Key,glyph,cover.Piece.Type,1);else Glyph(a.Key,glyph,RingInk(a.Value));}
            if(model.SelectedId!=null&&(!model.Placements.ContainsKey(model.SelectedId)||hidden!=null))foreach(var c in model.GetCandidateCells(model.SelectedId)){var pos=RuneGemGeometry.Pixel(c);Layer(12).Circle(pos,4.4f,Ink);Layer(12).Circle(pos,3.4f,RuneCasual.Green);}
            if(preview!=null)Piece(preview.OccupiedCells,preview.Piece.Type,false,true,!model.IsValid(preview));End();
        }
        // One hexagon drawn with the real renderers (legend, state guide, inspector medal): open/unlock/closed/sealed beds, or a block in the lit/dim/stored/selected/preview/invalid state.
        public void Slot(RuneBoardCell cell,bool lit=false,string state="open")
        {
            if(cell.Ability.Meaning=="root"&&state=="open")lit=true;
            Begin();var zero=new HexCell(0,0);int type=cell.AbilityType;
            bool block=lit||state=="dim"||state=="stored"||state=="selected"||state=="preview"||state=="invalid";
            if(state=="sealed"||state=="closed"||state=="unlock")Floor(cell,state=="unlock"?"closed":state,state=="unlock",true,zero);else{if(!block)Floor(cell,"open",false,true,zero);else Bed(zero,"open");}
            if(block)
            {
                bool ghost=state=="preview"||state=="invalid";bool stored=state=="stored"||state=="selected";
                Piece(new[]{zero},type,state=="selected",ghost,state=="invalid",null,true,1,false,null,Lift,Depth,false,stored?null:cell.Glyph,stored?(bool?)null:(lit&&state!="dim"));
            }
            if(state=="anchor"){Layer(12).Circle(Vector2.zero,7.4f,Alpha(RuneCasual.Yellow,.3f));Ring(Vector2.zero,7.4f,1.6f,RuneCasual.OrangeInk);Layer(12).Circle(Vector2.zero,4.9f,Ink);Layer(12).Circle(Vector2.zero,3.7f,RuneCasual.Green);}
            End();Fit(new[]{zero});
        }
    }
}
