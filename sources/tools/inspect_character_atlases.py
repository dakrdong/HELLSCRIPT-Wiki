"""Inspect native alpha and write UV/foot/hit metadata without altering raster pixels."""
import base64, hashlib, json
from pathlib import Path
import numpy as np
from PIL import Image
from scipy import ndimage

ROOT=Path(__file__).resolve().parents[1]
ART=ROOT/'Assets/HELLSCRIPT/Resources/Art/CharacterSelection'

def inspect(name,columns,rows):
    path=ART/(name+'.png'); image=Image.open(path)
    assert image.mode=='RGBA', 'Native RGBA is required; no background removal fallback.'
    alpha=np.asarray(image)[:,:,3]; assert (alpha==0).mean()>.35
    labels,_=ndimage.label(alpha>127); counts=np.bincount(labels.ravel()); ids=np.argsort(counts[1:])[-6:]+1
    assert all(counts[i]>10000 for i in ids)
    bodies=[]
    for i in ids:
        yy,xx=np.where(labels==i)
        box=(int(xx.min()),int(yy.min()),int(xx.max()+1),int(yy.max()+1))
        assert box[0]>0 and box[1]>0 and box[2]<image.width and box[3]<image.height
        # Anchor horizontally to the torso, not whichever boot is lowest in a staggered stance.
        middle=(yy>box[1]+(box[3]-box[1])*.42)&(yy<box[1]+(box[3]-box[1])*.7)
        feet_x=float(np.median(xx[middle]))
        bodies.append((box,feet_x))
    bodies.sort(key=lambda entry:(int((entry[0][1]+entry[0][3])/2/(image.height/rows)),entry[0][0]))
    heights=np.array([b[0][3]-b[0][1] for b in bodies]);cv=float(heights.std()/heights.mean());assert cv<.08,cv
    frames=[]
    for box,feet in bodies:
        x=max(0,box[0]-4);y=max(0,box[1]-4);right=min(image.width,box[2]+4);bottom=min(image.height,box[3]+4)
        w=right-x;h=bottom-y
        mask=np.zeros((64,64),dtype=bool)
        for my in range(64):
            for mx in range(64):
                patch=alpha[y+my*h//64:y+(my+1)*h//64,x+mx*w//64:x+(mx+1)*w//64]
                mask[my,mx]=patch.size>0 and patch.max()>64
        bits=np.packbits(mask.ravel(),bitorder='little').tobytes()
        frames.append(dict(x=x/image.width,y=1-bottom/image.height,width=w/image.width,height=h/image.height,
                           pixelsWide=w,pixelsHigh=h,footX=(feet-x)/w,footY=(box[3]-y)/h,
                           hitMask=base64.b64encode(bits).decode()))
    metadata=dict(columns=columns,rows=rows,referenceHeight=float(heights.mean()),frames=frames)
    (ART/(name+'.atlas.json')).write_text(json.dumps(metadata,indent=2)+'\n')
    return dict(asset=str(path.relative_to(ROOT)),size=list(image.size),mode=image.mode,sha256=hashlib.sha256(path.read_bytes()).hexdigest(),
                nativeTransparentFraction=float((alpha==0).mean()),frames=6,bodyHeightCV=cv,edgeClippedFrames=0,boxes=[b[0] for b in bodies],
                processing='Original PNG unchanged; connected-component inspection only; explicit UV bounds and foot anchors; 64x64 alpha hit masks.')

if __name__=='__main__':
    reports=[inspect('Warrior',2,3),inspect('Mage',2,3),inspect('Ranger',3,2)]
    out=ROOT/'Docs/Implementation/CharacterSelectionEvidence/art-qc.json';out.write_text(json.dumps(reports,indent=2)+'\n')
    print(json.dumps([{k:v for k,v in r.items() if k!='boxes'} for r in reports],indent=2))
