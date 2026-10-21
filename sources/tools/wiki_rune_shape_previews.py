"""Deterministic derived2D Rune shape previews, never runtime screenshot evidence.

Reuses the current painted attack tile and RuneGemView stored-icon geometry.
The representative palette is an explicit example, not an inherent shape property.
No Unity, Blender, network, AI image generation or runtime-resource writes.
The exterior type emblem is removed; its former bounds only preserve Wiki framing.
Derived presentation also omits the offset outer glint that floats at Wiki scale.
"""
import argparse
import hashlib
import json
import math
import re
from pathlib import Path

MANIFEST = 'Wiki/assets/rune-shapes-20261016/manifest.json'
PREFIX = 'Wiki/assets/rune-shapes-20261016/'
ROLE = 'derived-rune-shape-preview'
SHAPES = 'Assets/HELLSCRIPT/Resources/Runes/shapes.json'
CATALOG = 'Docs/Implementation/Rune_Mastery_Catalog.json'
V13 = 'Assets/HELLSCRIPT/Resources/Runes/V13/catalog.json'
VIEW = 'Assets/HELLSCRIPT/Runtime/Presentation/RuneGemView.cs'
MESH = 'Assets/HELLSCRIPT/Runtime/Presentation/RuneGemMesh.cs'
ART = 'Assets/HELLSCRIPT/Runtime/Presentation/RuneBoardArt.cs'
THEME = 'Assets/HELLSCRIPT/Runtime/Presentation/UiTheme.cs'
ROTATION = 'Assets/HELLSCRIPT/Runtime/Presentation/RuneV13Art.cs'
TILE = 'Assets/HELLSCRIPT/Resources/Art/RuneBoard/Casual/Tiles/gem-attack-a.png'
GLYPHS = 'Assets/HELLSCRIPT/Resources/Runes/V13/glyphs.json'
ATLAS = 'Assets/HELLSCRIPT/Resources/Runes/V13/glyph-atlas.png'
SOURCES = [SHAPES, CATALOG, V13, VIEW, MESH, ART, THEME, ROTATION,
           'Assets/HELLSCRIPT/Runtime/Runes/RuneDefinitions.cs',
           'Assets/HELLSCRIPT/Runtime/Runes/RuneMasteryCatalog.cs',
           'Assets/HELLSCRIPT/Runtime/Runes/RuneBoardSession.cs',
           'Assets/HELLSCRIPT/Runtime/Presentation/RuneBoardWindow.Panels.cs',
           'Assets/HELLSCRIPT/Runtime/Presentation/RuneBoardWindow.cs', TILE, GLYPHS, ATLAS]


def digest(raw): return hashlib.sha256(raw).hexdigest()
def require(ok, message):
    if not ok: raise ValueError(message)
def add(a,b): return (a[0]+b[0],a[1]+b[1])
def sub(a,b): return (a[0]-b[0],a[1]-b[1])
def mul(a,n): return (a[0]*n,a[1]*n)
def cross(a,b): return a[0]*b[1]-a[1]*b[0]
def norm(a): return mul(a,1/math.hypot(*a))
def pixel(c): return (36*c[0],-41.5692194*(c[1]+c[0]*.5))
def vertex(p,r,i): return add(p,(math.cos(i*math.pi/3)*r,math.sin(i*math.pi/3)*r))
def rotate(c,n):
    q,r=c
    for _ in range(n%6): q,r=-r,q+r
    return q,r
def color(hex,a=1): return tuple(int(hex[i:i+2],16)/255 for i in (1,3,5))+(a,)
def shifted(points,delta): return [add(p,delta) for p in points]
def contours(cells):
    edges={}
    key=lambda p:(round(p[0]*1000),round(p[1]*1000))
    for cell in cells:
        p=pixel(cell)
        for i in range(6):
            a,b=vertex(p,24,i),vertex(p,24,(i+1)%6);k=tuple(sorted((key(a),key(b))))
            if k in edges: del edges[k]
            else: edges[k]=(a,b)
    remaining={key(a):(a,b) for a,b in edges.values()};loops=[]
    while remaining:
        a,b=next(iter(remaining.values()));start=key(a);points=[]
        for _ in range(len(cells)*6+1):
            points.append(a);del remaining[key(a)]
            if key(b)==start or key(b) not in remaining: break
            a,b=remaining[key(b)]
        require(len(points)>=3,'Broken contour');loops.append(points)
    return loops
def inset(points,d):
    out=[]
    for i,p in enumerate(points):
        a=norm(sub(p,points[i-1]));b=norm(sub(points[(i+1)%len(points)],p))
        out.append(add(p,mul((-a[1]-b[1],a[0]+b[0]),d/(1+a[0]*b[0]+a[1]*b[1]))))
    return out


class Mesh:
    def __init__(self): self.layers={};self.framing_points=[]
    def triangle(self,order,points,colors,uv=None,texture=None):
        self.layers.setdefault((order,texture or ''),[]).append((points,colors,uv))
    def polygon(self,order,points,light,dark):
        lo,hi=min(p[1] for p in points),max(p[1] for p in points)
        colors=[tuple(dark[c]+(light[c]-dark[c])*(0 if hi==lo else (p[1]-lo)/(hi-lo)) for c in range(4)) for p in points]
        indices=list(range(len(points)));guard=0
        while len(indices)>2 and guard<len(points)**2:
            guard+=1;found=False
            for i,b in enumerate(indices):
                a,c=indices[i-1],indices[(i+1)%len(indices)]
                if cross(sub(points[b],points[a]),sub(points[c],points[b]))<=.00001: continue
                if any(k not in (a,b,c) and cross(sub(points[b],points[a]),sub(points[k],points[a]))>-.00001 and cross(sub(points[c],points[b]),sub(points[k],points[b]))>-.00001 and cross(sub(points[a],points[c]),sub(points[k],points[c]))>-.00001 for k in indices): continue
                self.triangle(order,[points[k] for k in (a,b,c)],[colors[k] for k in (a,b,c)]);indices.pop(i);found=True;break
            require(found,'Ear clipping failed')
    def outline(self,order,points,width,shade):
        pairs=[]
        for i,p in enumerate(points):
            a=norm(sub(p,points[i-1]));b=norm(sub(points[(i+1)%len(points)],p))
            n=mul((-a[1]-b[1],a[0]+b[0]),width*.5/max(.1,1+a[0]*b[0]+a[1]*b[1]))
            if math.hypot(*n)>width*2: n=mul(norm(n),width*2)
            pairs.append((sub(p,n),add(p,n)))
        for i,(a,d) in enumerate(pairs):
            b,c=pairs[(i+1)%len(pairs)]
            self.triangle(order,[a,b,c],[shade]*3);self.triangle(order,[a,c,d],[shade]*3)
    def tile(self,center,radius,uv,texture):
        points=[center]+[vertex(center,radius,i) for i in range(6)]
        coords=[(uv[0]+uv[2]/2,uv[1]+uv[3]/2)]
        for p in points[1:]:
            d=sub(p,center);coords.append((uv[0]+uv[2]*(.5+d[0]/(radius*2)),uv[1]+uv[3]*(.5+d[1]/(radius*1.7320508))))
        for i in range(6):
            ix=[0,1+i,1+(i+1)%6];self.triangle(7,[points[k] for k in ix],[(1,1,1,1)]*3,[coords[k] for k in ix],texture)


def attack_palette(source):
    body=source.split('public static class RuneCasual',1)[1]
    def first(name):
        match=re.search(r'\b'+name+r'\s*=\s*\{([^}]+)\}',body)
        values=re.findall(r'H\("([0-9a-fA-F]{6})"\)',match[1]) if match else []
        require(len(values)==7,'Review changed RuneCasual palette: '+name)
        return '#'+values[0]
    ink=re.search(r'\bInk\s*\{get;\}\s*=H\("([0-9a-fA-F]{6})"\)',body)
    require(ink is not None,'Review changed RuneCasual ink')
    return {'fill':first('Fill'),'dark':first('Dark'),'ink':first('RingInk'),'outline':'#'+ink[1]}


def piece_mesh(cells,palette,tile_uv):
    m=Mesh();loops=contours(cells)
    for loop in loops:
        body=inset(loop,1.1);top=shifted(body,(0,3))
        m.polygon(4,shifted(body,(1.2,-7.4)),color(palette['outline'],.22),color(palette['outline'],.22))
        wall=shifted(body,(0,-2))
        m.polygon(5,wall,color(palette['dark']),color(palette['dark']))
        m.outline(5,wall,1.4,color(palette['outline']))
        m.polygon(6,top,color(palette['fill']),color(palette['fill']))
    for c in cells:m.tile(add(pixel(c),(0,3)),23.15,tile_uv,TILE)
    for loop in loops:
        top=shifted(inset(loop,1.1),(0,3))
        # Omit the offset outer glint for derived Wiki framing; keep normal ink/black rims.
        m.outline(9,shifted(inset(loop,2.4),(0,3)),1.2,color(palette['ink'],.8))
        m.outline(9,top,2.2,color(palette['outline']))
    # Preserve the previous Wiki body scale/centre without rendering the retired emblem.
    # These four corners affect framing only: no triangles, UVs or texture sampling.
    retired_center=add(pixel(cells[0]),(-14.88,22.12))
    m.framing_points=[add(retired_center,d) for d in [(-4.5,-4.5),(4.5,-4.5),(4.5,4.5),(-4.5,4.5)]]
    return m,loops


def raster(mesh,textures,edge=512,supersample=2):
    import numpy as np
    from PIL import Image
    n=edge*supersample;image=np.zeros((n,n,4),dtype=np.float32)
    points=[p for triangles in mesh.layers.values() for tri in triangles for p in tri[0]]
    geometry_lo=np.min(points,axis=0);geometry_hi=np.max(points,axis=0)
    framing_points=points+mesh.framing_points
    lo=np.min(framing_points,axis=0);hi=np.max(framing_points,axis=0);center=(lo+hi)/2
    scale=(edge-48)/max(hi-lo);scale*=supersample
    def project(p):return np.array([(p[0]-center[0])*scale+n/2,n/2-(p[1]-center[1])*scale])
    for (_,texture),triangles in sorted(mesh.layers.items()):
        for ps,cs,uv in triangles:
            p=np.asarray([project(v) for v in ps]);c=np.asarray(cs);uv=np.asarray(uv) if uv else None
            area=cross(p[1]-p[0],p[2]-p[0])
            if abs(area)<1e-8:continue
            if area<0:
                p[[1,2]]=p[[2,1]];c[[1,2]]=c[[2,1]];area=-area
                if uv is not None:uv[[1,2]]=uv[[2,1]]
            x0,y0=np.maximum(np.floor(p.min(axis=0)).astype(int),0);x1,y1=np.minimum(np.ceil(p.max(axis=0)).astype(int),n)
            if x0>=x1 or y0>=y1:continue
            xx,yy=np.meshgrid(np.arange(x0,x1)+.5,np.arange(y0,y1)+.5)
            edges=[];inside=np.ones(xx.shape,dtype=bool)
            for a,b in [(p[1],p[2]),(p[2],p[0]),(p[0],p[1])]:
                d=b-a;e=d[0]*(yy-a[1])-d[1]*(xx-a[0]);edges.append(e/area);inside&=(e>1e-9)|((abs(e)<=1e-9)&((d[1]<0)or(d[1]==0 and d[0]>0)))
            if not inside.any():continue
            weights=np.stack(edges,axis=-1);rgba=weights@c
            if texture:
                tex=textures[texture];v=weights@uv;h,w=tex.shape[:2];sx=np.clip(v[...,0]*w-.5,0,w-1);sy=np.clip((1-v[...,1])*h-.5,0,h-1)
                ix=np.floor(sx).astype(int);iy=np.floor(sy).astype(int);fx=(sx-ix)[...,None];fy=(sy-iy)[...,None]
                sampled=(tex[iy,ix]*(1-fx)+tex[iy,np.minimum(ix+1,w-1)]*fx)*(1-fy)+(tex[np.minimum(iy+1,h-1),ix]*(1-fx)+tex[np.minimum(iy+1,h-1),np.minimum(ix+1,w-1)]*fx)*fy
                rgba*=sampled
            dst=image[y0:y1,x0:x1];a=rgba[...,3:4];rgb=rgba[...,:3]*a+dst[...,:3]*(1-a);alpha=a+dst[...,3:4]*(1-a)
            dst[inside]=np.concatenate((rgb,alpha),axis=-1)[inside]
    alpha=image[...,3:4];straight=np.concatenate((np.divide(image[...,:3],alpha,out=np.zeros_like(image[...,:3]),where=alpha>0),alpha),axis=-1)
    result=Image.fromarray(np.rint(np.clip(straight,0,1)*255).astype('uint8'),'RGBA').resize((edge,edge),Image.Resampling.LANCZOS)
    return result,{'geometryBounds':[geometry_lo.tolist(),geometry_hi.tolist()],'framingBounds':[lo.tolist(),hi.tolist()],'canvas':edge,'padding':24,'scale':scale/supersample,'sampling':'CPU bilinear,2x supersampling,LANCZOS downsample; sRGB alpha blend; not GPU-compressed pixels'}


def generate(root,output):
    from PIL import Image,ImageDraw
    import numpy as np
    root,output=Path(root),Path(output);require(not output.exists() or not any(output.iterdir()),'Never overwrite an existing preview bundle/attempt')
    source_before={p:digest((root/p).read_bytes()) for p in SOURCES}
    docs=json.loads((root/CATALOG).read_text());runtime=json.loads((root/SHAPES).read_text());v13=json.loads((root/V13).read_text());art=(root/ART).read_text();rot=(root/ROTATION).read_text()
    shapes=docs['shapes'];actual={s['id']:s for s in runtime['shapes']};require(len(shapes)==len(actual)==34,'Expected exact34 catalog')
    rotations={i:int(n) for i,n in re.findall(r'"(rune-\d-\d\d)"=>(\d)',rot)}
    require(rotations=={'rune-2-01':2,'rune-3-01':2,'rune-3-02':1,'rune-4-06':4,'rune-5-05':1},'Review changed storage rotations')
    uv=lambda key:[float(v.rstrip('f')) for v in re.search(r'\{"'+re.escape(key)+r'",new Rect\(([^)]+)\)\}',art).group(1).split(',')]
    tile_uv=uv('Tiles/gem-attack-a')
    textures={TILE:np.asarray(Image.open(root/TILE).convert('RGBA'),dtype=np.float32)/255}
    glyphs=json.loads((root/GLYPHS).read_text());atlas=Image.open(root/ATLAS).convert('RGBA');i=len(glyphs['ids']);columns=glyphs['columns'];rows=math.ceil((i+1)/columns)
    white=atlas.getpixel((int(((i%columns)+.5)/columns*atlas.width),int(((i//columns)+.5)/rows*atlas.height)))
    require(white==(255,255,255,255),'Runtime primitive atlas sample is no longer opaque white')
    require(any(t['id']=='attack' for t in v13['types']),'Missing attack type')
    palette=attack_palette((root/THEME).read_text());output.mkdir(parents=True,exist_ok=True);records=[]
    for s in shapes:
        r=actual[s['id']];require(r['shapeNumber']==s['number'] and r['cells']==s['cells'] and len(r['cells'])==s['size'],'Runtime/document shape mismatch: '+s['id'])
        cells=[(c['q'],c['r']) for c in s['cells']];turn=rotations.get(s['id'],0);display=[rotate(c,turn) for c in cells]
        require([rotate(c,6) for c in cells]==cells,'Rotation roundtrip mismatch');mesh,loops=piece_mesh(display,palette,tile_uv)
        require(len(loops)==1,'Expected single connected contour');image,frame=raster(mesh,textures);file=output/(s['id']+'.png');image.save(file,optimize=True);raw=file.read_bytes();bbox=image.getchannel('A').getbbox();require(bbox and min(bbox[:2])>=16 and max(bbox[2:])<=496,'Clipped preview')
        records.append({'id':s['id'],'size':s['size'],'number':s['number'],'cells':s['cells'],'rotationCW':turn,'displayCells':[{'q':q,'r':r} for q,r in display],'source':PREFIX+file.name,'file':'evidence/rune-shapes/'+file.name,'role':ROLE,'sha256':digest(raw),'bytes':len(raw),'width':512,'height':512,'alphaBounds':bbox,'frame':frame,'meshTriangles':sum(map(len,mesh.layers.values())),'textureApplications':len(cells),'typeIdExample':'attack','gradeExample':0,'state':'stored-unselected-lit'} )
    require(source_before=={p:digest((root/p).read_bytes()) for p in SOURCES},'Rune input changed during generation; preserve this attempt')
    pins=[{'path':p,'sha256':source_before[p],'bytes':(root/p).stat().st_size} for p in SOURCES]
    pins.append({'path':'tools/wiki_rune_shape_previews.py','sha256':digest(Path(__file__).read_bytes()),'bytes':Path(__file__).stat().st_size})
    manifest={'schemaVersion':1,'role':ROLE,'scope':'Derived2D shape previews, not runtime screenshots/new paintings','renderContract':'RuneGemView.Icon/Piece(board=null,selected=false,type=0,state=stored), current painted PNG/UV and exact axial geometry/storage default rotation','paletteQualification':'Attack palette example only; shape IDs support five types and no type is assigned by shape','runtimePixelEqualityClaim':False,'presentation':{'omitDetachedOffsetGlint':True,'omittedRuntimeCall':'Layer(9).Outline(Inset(loop,2.4) + (0,3) + (-.5,.6), 1, white alpha .6)','omitExteriorTypeEmblem':True,'framingPreserved':True,'framingOnlyPoints':'Retired 9x9 type-emblem corners preserve prior Wiki body scale/centre; never rendered or texture-sampled','preserved':['exact cells','storage rotation','painted tile UV','shared ink rims','shadow','Wiki body scale and centre']},'framing':'Rendered geometry plus retired exterior type-emblem corner bounds define framingBounds with24px Wiki padding; geometryBounds describes rendered geometry only; original cell-based game FitIcon is unchanged','gpuDifferences':['Source PNG vs Unity imported/compressed texture','CPU sRGB blend/bilinear/supersample vs GPU color-space/shader sampling','Wiki framing uses framingBounds including retired-emblem corners, not original FitIcon crop','The offset outer glint is omitted for derived presentation; the retired exterior type emblem is absent from both current runtime and previews'],'primitiveWhiteRgba':white,'sourcePins':pins,'textureUV':{TILE:tile_uv},'records':records}
    (output/'manifest.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2)+'\n')
    sheet=Image.new('RGB',(7*144,5*164),'#f1f5f6');draw=ImageDraw.Draw(sheet)
    for n,r in enumerate(records):
        thumb=Image.open(output/(r['id']+'.png')).convert('RGBA');thumb.thumbnail((140,140));x=(n%7)*144+(144-thumb.width)//2;y=(n//7)*164;sheet.paste(thumb,(x,y),thumb);draw.text((n%7*144+5,y+143),r['id'],fill='#28324e')
    sheet.save(output/'contact-sheet.png');return manifest


def prepare(api,shapes,rows):
    """Validate everything and return nonmutating deltas for root's final apply loop."""
    def valid_bounds(box):
        return isinstance(box,list) and len(box)==2 and all(isinstance(p,list) and len(p)==2 and all(type(v) in (int,float) and math.isfinite(v) for v in p) for p in box) and all(box[0][d]<box[1][d] for d in range(2))
    def binary(path):
        full=api.ROOT/path
        require(full.resolve().is_relative_to(api.ROOT.resolve()),'Rune input escapes ROOT: '+path)
        return full.read_bytes()
    manifest_raw=binary(MANIFEST);manifest_sha=digest(manifest_raw)
    require(api.INPUTS.get(MANIFEST,manifest_sha)==manifest_sha,'Previously collected Rune manifest differs')
    manifest=json.loads(manifest_raw);presentation=manifest.get('presentation',{})
    require(manifest.get('schemaVersion')==1 and manifest.get('role')==ROLE and manifest.get('runtimePixelEqualityClaim') is False and isinstance(presentation,dict) and presentation.get('omitDetachedOffsetGlint') is True and presentation.get('omitExteriorTypeEmblem') is True and presentation.get('framingPreserved') is True,'Invalid derived Rune manifest/presentation')
    records=manifest['records'];lookup={r['id']:r for r in records};canonical={s['id']:s for s in shapes}
    require(len(records)==len(lookup)==len(rows)==len(canonical)==34 and set(lookup)==set(rows)==set(canonical),'Changed exact34 Rune IDs')
    expected=set(SOURCES+['tools/wiki_rune_shape_previews.py']);pins=manifest['sourcePins']
    require(len(pins)==len(expected) and {p['path'] for p in pins}==expected,'Incomplete/duplicate Rune source pins')
    collected={MANIFEST:manifest_sha}
    for pin in pins:
        raw=binary(pin['path']);actual=digest(raw)
        require(actual==pin['sha256'] and len(raw)==pin['bytes'],'Changed Rune rendering input: '+pin['path'])
        require(api.INPUTS.get(pin['path'],actual)==actual,'Previously collected Rune input differs: '+pin['path']);collected[pin['path']]=actual
    rotations={i:int(n) for i,n in re.findall(r'"(rune-\d-\d\d)"=>(\d)',binary(ROTATION).decode())}
    plans=[]
    for identifier,record in lookup.items():
        shape=canonical[identifier];path=PREFIX+identifier+'.png'
        require(re.fullmatch(r'rune-[1-5]-\d\d',identifier) and record['source']==path and record['file']=='evidence/rune-shapes/'+identifier+'.png' and record['role']==ROLE,'Invalid Rune preview path/role')
        require(record['cells']==shape['cells'] and record['size']==shape['size'] and record['number']==shape['number'] and record['typeIdExample']=='attack','Changed shape identity/representative palette')
        turns=rotations.get(identifier,0)
        require(record['rotationCW']==turns and record['displayCells']==[dict(zip(('q','r'),rotate((c['q'],c['r']),turns))) for c in shape['cells']],'Changed stored-icon orientation')
        frame=record.get('frame')
        require(isinstance(frame,dict),'Invalid Rune preview framing: '+identifier)
        geometry=frame.get('geometryBounds');framing=frame.get('framingBounds')
        require(valid_bounds(geometry) and valid_bounds(framing) and all(framing[0][d]<=geometry[0][d] and geometry[1][d]<=framing[1][d] for d in range(2)),'Invalid Rune preview framing: '+identifier)
        raw=binary(path);actual=digest(raw)
        require(raw[:8]==b'\x89PNG\r\n\x1a\n' and raw[12:16]==b'IHDR' and raw[16:24]==b'\x00\x00\x02\x00\x00\x00\x02\x00' and actual==record['sha256'] and len(raw)==record['bytes'],'Changed Rune preview bytes: '+identifier)
        require(api.INPUTS.get(path,actual)==actual,'Previously collected Rune preview differs: '+path);collected[path]=actual
        row=rows[identifier];expected_image={'file':record['file'],'role':ROLE}
        require(row.get('image') in (None,expected_image) and row.get('previewSource') in (None,path) and not row.get('assetPath') and not row.get('images'),'Conflicting existing Rune preview: '+identifier)
        plans.append({'id':identifier,'previewSource':path,'image':{'file':record['file'],'role':ROLE},'fields':{'미리보기 / Preview':'현재 2D 타일·정확한 형태 좌표로 합성한 파생 미리보기입니다. 실제 게임 화면 캡처가 아닙니다. / Derived from current 2D tiles and exact shape coordinates; not a runtime screenshot.','표현색 / Example palette':'공격색 예시 · 모양은 5종 색상 공통 / Attack palette example; shape supports all five types','표시 회전 / Display rotation':record['rotationCW']*60},'refs':[api.source_ref(MANIFEST),api.source_ref(SHAPES),api.source_ref(VIEW),api.source_ref(MESH),api.source_ref(TILE)]})
    api.INPUTS.update(collected)
    return plans


if __name__=='__main__':
    parser=argparse.ArgumentParser(description=__doc__);parser.add_argument('--root',type=Path,required=True);parser.add_argument('--output',type=Path,required=True);args=parser.parse_args();m=generate(args.root,args.output);print(json.dumps({'generated':len(m['records']),'role':ROLE,'runtimeScreenshots':0,'runtimePaintingsCreated':0}))
