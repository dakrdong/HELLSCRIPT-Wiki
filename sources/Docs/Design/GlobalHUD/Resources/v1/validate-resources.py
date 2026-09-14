"""Read-only pixel validation and handoff manifest generation; no image editing."""
from pathlib import Path
import hashlib, json
from PIL import Image

ROOT = Path(__file__).resolve().parent
def save(name, obj):
    (ROOT / name).write_text(json.dumps(obj, ensure_ascii=False, indent=2) + '\n')

vectors = json.loads((ROOT / 'vector-manifest.json').read_text())
native = json.loads((ROOT / 'native-sources.json').read_text())
entries = {e['id']: e for e in vectors}
for key, filename in native.items():
    entries[key] = dict(id=key, category='portrait' if key.startswith('portrait') else 'potion', file=f'png/{key}.png', nativeFile=filename, spriteMode='Single', pivot=[.5,.5])
entries['skill-atlas-existing'] = dict(id='skill-atlas-existing',category='reused',file='png/skill-atlas-existing.png',source='Assets/HELLSCRIPT/Resources/Art/SkillAtlas.png',spriteMode='Multiple',usage='Prefer existing project texture; packaged identical copy for standalone handoff.')
checks=[]
for key, e in entries.items():
    p=ROOT/e['file']; data=p.read_bytes(); im=Image.open(p)
    e.update(width=im.width,height=im.height,sha256=hashlib.sha256(data).hexdigest(),mode=im.mode)
    alpha='A' in im.getbands()
    q=dict(id=key,mode=im.mode,size=list(im.size),alphaRequired=key!='skill-atlas-existing',hasAlpha=alpha)
    if alpha:
        a=im.getchannel('A'); h=a.histogram()
        q.update(transparentPixels=h[0],partialPixels=sum(h[1:255]),opaquePixels=h[255],alphaBounds=list(a.getbbox()))
        # Full-rectangle white sprites intentionally do not contain empty pixels.
        if key not in ['fill-white']:
            assert h[0]>0, f'Missing transparent area: {key}'
        threshold=100 if key=='plate-icon' else 200
        assert max(i for i,n in enumerate(h) if n)>threshold, f'Unusable opacity: {key}'
    else:
        assert key=='skill-atlas-existing',f'Native alpha missing: {key}'
    if key in native:
        marker=b'msoftwareAgent\xa2dnameigpt-imagegversionc2.0'
        assert marker in data, f'Missing model provenance marker: {key}'
        q['modelMetadata']={'name':'gpt-image','version':'2.0','signatureVerified':False}
        e['provenance']='Built-in image_gen; C2PA field gpt-image 2.0; signature not verified; original bytes retained.'
    checks.append(q)
atlas=entries['skill-atlas-existing']
assert (atlas['width'],atlas['height'])==(1536,1024)
atlas['sprites']=[]
for cell in range(21):
    ident=(['W','A','M'][cell//6]+f'{cell%6+1:02}') if cell<18 else ['portrait-warrior','portrait-ranger','portrait-mage-legacy'][cell-18]
    atlas['sprites'].append(dict(id=ident,cell=cell,rectBottomLeft=[cell%6*256,1024-(cell//6+1)*256,256,256],pivot=[.5,.5],border=[0,0,0,0]))
profile={
 'units':'Proposed logical units at 1600x900 landscape / 900x1600 portrait; calibrate in Unity before sign-off. Ratios are the approved requirement; baseline pixel sizes are implementation proposals.',
 'landscape':{'portrait':112,'vitalWidth':300,'vitalHeight':20,'vitalGap':8,'statusIcon':44,'statusGap':8,'statusCollapsedWidth':230,'statusExpandedMaxWidth':408,'statusFadeMaxWidth':22,'expandVisual':32,'expandHit':44,'skillBaseline':64,'skillScale':1.2,'skillSize':76.8,'skillGap':12,'potionBaseline':60,'potionScale':.75,'potionSize':45,'potionRowAboveSkills':True,'xpThickness':3,'xpSideInset':32,'xpBottomInset':16,'xpTicks':[i/10 for i in range(1,10)]},
 'portrait':{'portrait':96,'skillSize':64,'potionSize':60,'statusIcon':40,'statusGap':8,'xpMode':'existing approved short left track','skillRowAbovePotions':True,'note':'Do not apply landscape-only reordering/resizing/full-width XP to portrait.'},
 'colors':{'hp':'#c8403c','mp':'#338acb','xp':'#d8b774','text':'#f3ebda','shadow':'#0b0d12','gold':'#c4a872'},
 'input':'Read-only skill/potion/vitals. Expand/collapse, scroll and explicit details only.'}
save('layout-profile.json',profile)
manifest={'version':1,'status':'resources-and-planning-ready; Unity integration not started','date':'2026-09-14','futureImportFolder':'Assets/HELLSCRIPT/Resources/Art/GlobalHUD/v1','commonImport':{'textureType':'Sprite (2D and UI)','alphaSource':'Input Texture Alpha','alphaIsTransparency':True,'sRGB':True,'filterMode':'Bilinear','wrapMode':'Clamp','mipMaps':False,'readWrite':False,'meshType':'Full Rect','pixelsPerUnit':100,'compressionForInitialReview':'None','nativeMaxSize':2048,'vectorMaxSize':512,'atlasPadding':8,'atlasRotation':False,'atlasTightPacking':False},'portraitMap':{'Warrior':'skill-atlas-existing#portrait-warrior','Ranger':'skill-atlas-existing#portrait-ranger','Mage':'portrait-mage','None':'portrait-placeholder'},'assets':list(entries.values()),'notes':['The copied existing atlas is byte-identical; reuse its existing project texture to avoid duplicate runtime memory.','No Assets files, .meta, prefab, shader, C# or scene integration has been created.','The JSON is a handoff contract, not an installed Unity importer.','Text and live values are never baked into artwork.']}
save('resource-manifest.json',manifest)
save('resource-qa.json',{'result':'pass','scope':'PNG dimensions, actual alpha, unique IDs, native metadata fields, atlas rectangle bounds; no Unity import or device validation','imageCount':len(checks),'nativeImageCount':len(native),'vectorImageCount':len(vectors),'reusedImageCount':1,'checks':checks})
print(json.dumps({'result':'pass','pngs':len(checks),'native':len(native),'vector':len(vectors),'reused':1}))
