#!/usr/bin/env python3
"""Author reward-box catalog and original vector artwork. No AI or third-party pixels."""
import json
import hashlib
import re
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parents[1]
ART = ROOT / 'Assets/HELLSCRIPT/Resources/Art/RewardBoxes'
SOURCE = ROOT / 'Docs/Art/RewardBoxes'
DATA = ROOT / 'Assets/HELLSCRIPT/Resources/Data/RewardBoxes.json'


def catalog():
    boxes = []
    def add(id, ko, en, kind, **kw):
        row = dict(id=id, nameKo=ko, nameEn=en, kind=kind, amount=1, slot=-1,
                   rarity=0, tier=0, gemId='', grade=0, size=0, formula='', icon=id)
        row.update(kw); boxes.append(row)
    slots = [('weapon','무기','Weapon'),('helm','머리','Helm'),('body','몸통','Chest'),
             ('gloves','장갑','Gloves'),('boots','신발','Boots'),('belt','허리','Belt'),
             ('amulet','목걸이','Amulet'),('ring','반지','Ring')]
    for rarity, key, ko, en in [(2,'rare','희귀','Rare'),(3,'legendary','전설','Legendary')]:
        add(key+'-equipment',ko+' 장비 랜덤 상자',en+' Random Equipment Box','equipment',rarity=rarity)
        for slot,(suffix,sko,sen) in enumerate(slots):
            add(key+'-'+suffix,ko+' '+sko+' 랜덤 상자',en+' Random '+sen+' Box','equipment',rarity=rarity,slot=slot)
    add('set-body','직업 세트 몸통 상자','Class Set Chest Box','equipment',rarity=3,slot=2,setOnly=True)
    add('awakened-helm','각성 전설 머리 상자','Awakened Legendary Helm Box','equipment',rarity=3,slot=1,awakened=True)
    add('awakened-weapon','각성 전설 무기 상자','Awakened Legendary Weapon Box','equipment',rarity=3,slot=0,awakened=True)
    gems = [('G01','홍옥','Ruby'),('G02','청옥','Sapphire'),('G03','황옥','Topaz'),
            ('G04','취옥','Emerald'),('G05','자수정','Amethyst'),('G06','금강석','Diamond')]
    for tier in range(1,7):
        add(f'gem-choice-t{tier}',f'{tier}단계 보석 선택 상자 · 10개',f'T{tier} Gem Choice Box · 10','gem',tier=tier,amount=10)
        for gid,ko,en in gems:
            add(f'gem-{gid.lower()}-t{tier}',f'{tier}단계 {ko} 상자 · 10개',f'T{tier} {en} Box · 10','gem',gemId=gid,tier=tier,amount=10)
    for kind,ko,en,amounts in [('stones','강화석','Enhancement Stone',[30,100,500,3000]),
                              ('materials','제작 재료','Crafting Material',[50,200,1500]),
                              ('gold','골드','Gold',[10000,20400,100000,1000000]),
                              ('premium','심연 주화','Abyssal Coin',[100,200,300,500,1000,2000])]:
        for amount in amounts:
            add(f'{kind}-{amount}',f'{ko} 상자 · {amount:,}개',f'{en} Box · {amount:,}',kind,amount=amount)
    add('rift-stones','균열 최초 강화석 상자','Rift First-clear Stone Box','stones',formula='rift-stones')
    add('ten-materials','10단계 달성 재료 상자','Ten-tier Material Box','materials',formula='ten-materials')
    add('fifty-stones','50단계 달성 강화석 상자','Fifty-tier Stone Box','stones',formula='fifty-stones')
    add('core-choice','장비 코어 선택 상자 · 10개','Equipment Core Choice Box · 10','cores',amount=10)
    for slot,(suffix,ko,en) in enumerate(slots):
        add('core-'+suffix,ko+' 코어 상자 · 10개',en+' Core Box · 10','cores',amount=10,slot=slot)
    add('rune-starter','입문 룬 상자 · 5종','Starter Rune Box · Five Types','rune',amount=5,grade=0,size=1,eachType=True)
    for grade in [4,5,6]:
        add(f'rune-g{grade}',f'G{grade} 5칸 룬 상자',f'G{grade} Five-cell Rune Box','rune',grade=grade,size=5)
    def grant(id,count=1,quality=0):return dict(boxId=id,count=count,minimumQuality=quality)
    milestones = {
        1:[grant('rare-weapon',quality=5000)],3:[grant('materials-50')],5:[grant('stones-30')],
        10:[grant('gem-choice-t1',2),grant('premium-100')],
        15:[grant('rune-starter')],30:[grant('legendary-weapon',quality=5000),grant('gem-choice-t2')],
        35:[grant('gold-20400')],
        50:[grant('premium-200')],80:[grant('materials-200'),grant('gold-10000')],
        100:[grant('awakened-helm',quality=6000),grant('gem-choice-t3'),grant('premium-300')],
        120:[grant('core-choice')],200:[grant('set-body',quality=6000),grant('premium-500')],
        300:[grant('gem-choice-t4'),grant('rune-g4'),grant('premium-500')],
        500:[grant('gem-choice-t5'),grant('rune-g5'),grant('premium-1000')],650:[grant('rune-g6')],
        750:[grant('gem-choice-t6'),grant('premium-1000')],
        1000:[grant('gold-1000000'),grant('materials-1500'),grant('stones-3000'),
              grant('awakened-weapon',quality=9000),grant('gem-choice-t6',2),grant('premium-2000')]}
    balance=json.loads((ROOT/'Docs/Design/Balance1000/parameters.json').read_text())
    points=[dict(stage=p[0],level=int(p[3])) for p in balance['milestones']]
    return dict(version=1,boxes=boxes,itemLevels=points,
                firstClearRules=[dict(every=1,grant=grant('rift-stones')),dict(every=10,grant=grant('ten-materials')),dict(every=50,grant=grant('fifty-stones'))],
                milestones=[dict(stage=k,grants=v) for k,v in sorted(milestones.items())])


class Canvas:
    """Identical authored vector primitives exported to SVG and supersampled PNG."""
    def __init__(self):
        self.image=Image.new('RGBA',(1024,1024));self.draw=ImageDraw.Draw(self.image);self.svg=[]
    def polygon(self,points,color):
        self.draw.polygon([(int(x*4),int(y*4)) for x,y in points],fill=color)
        self.svg.append('<polygon points="'+' '.join(f'{x},{y}' for x,y in points)+'" fill="'+color+'"/>')
    def rect(self,x,y,w,h,color):self.polygon([(x,y),(x+w,y),(x+w,y+h),(x,y+h)],color)
    def line(self,points,color,width=3):
        self.draw.line([(int(x*4),int(y*4)) for x,y in points],fill=color,width=width*4,joint='curve')
        self.svg.append('<polyline points="'+' '.join(f'{x},{y}' for x,y in points)+f'" stroke="{color}" stroke-width="{width}" fill="none" stroke-linejoin="round"/>')
    def ellipse(self,x,y,w,h,color):
        self.draw.ellipse((x*4,y*4,(x+w)*4,(y+h)*4),fill=color)
        self.svg.append(f'<ellipse cx="{x+w/2}" cy="{y+h/2}" rx="{w/2}" ry="{h/2}" fill="{color}"/>')


def artwork(box):
    c=Canvas();kind=box['kind'];slot=box['slot'];gem=box['gemId']
    accents={'G01':'#eb6663','G02':'#60a6ed','G03':'#f1cd64','G04':'#6fc896','G05':'#b892e7','G06':'#d9e8dc'}
    accent=accents.get(gem,'#b8c1d2') if kind=='gem' else '#7b8ce6' if kind=='premium' else '#bf899d' if kind=='rune' else '#dbb970'
    if kind=='equipment':
        palette=re.findall(r'"([0-9a-f]{6})"',(ROOT/'Assets/HELLSCRIPT/Runtime/Presentation/EquipmentGradePalette.cs').read_text())
        accent='#'+palette[4 if box.get('setOnly') else box['rarity']]
    dark='#121a19';gold='#9a7b4b';light='#ecd9a2'
    c.polygon([(27,106),(27,73),(58,47),(195,47),(226,79),(226,187),(191,218),(61,218),(27,188)],dark)
    c.polygon([(34,105),(34,77),(62,54),(191,54),(218,82),(218,108)],gold)
    c.polygon([(42,99),(42,82),(66,62),(186,62),(210,85),(210,101)],'#39413a')
    c.polygon([(42,99),(69,72),(183,72),(210,98)],'#485148')
    c.polygon([(34,115),(219,115),(215,184),(188,210),(65,210),(34,184)],gold)
    c.polygon([(42,123),(209,123),(207,180),(184,201),(70,201),(43,180)],'#282e28')
    for y in [143,169,189]:c.line([(45,y),(208,y)],'#414538',2)
    for x in [61,184]:
        c.rect(x,68,10,134,gold);c.rect(x+3,73,3,125,light)
        for y in [84,132,184]:c.ellipse(x+2,y,5,5,'#322c22')
    c.line([(36,108),(218,108)],light,3)
    c.polygon([(89,107),(126,88),(163,107),(163,161),(126,184),(89,161)],dark)
    c.polygon([(94,109),(126,93),(158,109),(158,157),(126,177),(94,157)],accent)
    c.polygon([(101,114),(126,101),(151,114),(151,153),(126,168),(101,153)],'#25302e')
    if kind=='equipment':
        if slot==0:
            c.polygon([(125,110),(132,108),(131,143),(124,149),(120,144)],light);c.line([(113,145),(139,145)],accent,4);c.line([(126,147),(126,159)],light,5)
        elif slot==1:
            c.polygon([(111,145),(111,126),(117,116),(135,116),(143,126),(143,145),(132,142),(127,151),(122,142)],light);c.rect(118,133,17,5,dark)
        elif slot==2 or slot==-1:
            c.polygon([(112,117),(119,112),(126,119),(134,112),(143,117),(149,131),(139,134),(138,153),(116,153),(115,134),(105,131)],light)
        elif slot==3:
            c.polygon([(111,121),(117,118),(122,130),(122,112),(130,113),(132,133),(139,125),(146,130),(140,147),(132,155),(115,149)],light)
        elif slot==4:
            c.polygon([(115,113),(132,114),(131,139),(146,146),(145,156),(110,156),(110,142)],light)
        elif slot==5:
            c.rect(107,126,40,16,light);c.rect(118,122,18,24,accent);c.rect(123,128,8,12,dark)
        elif slot==6:
            c.line([(110,117),(111,134),(126,149),(143,134),(144,117)],light,3);c.polygon([(126,137),(134,148),(126,159),(118,148)],accent)
        elif slot==7:
            c.ellipse(113,126,29,29,light);c.ellipse(118,131,19,19,'#25302e');c.polygon([(126,111),(136,120),(126,130),(116,120)],accent)
    elif kind=='gem':
        c.polygon([(111,115),(140,115),(148,128),(126,158),(104,128)],accent)
        c.line([(111,115),(118,130),(126,158),(134,130),(140,115)],light,2);c.line([(104,128),(148,128)],light,2)
    elif kind in ('gold','premium'):
        c.ellipse(108,115,37,39,accent);c.ellipse(113,120,27,29,light);c.polygon([(127,123),(135,135),(127,149),(119,135)],accent)
    elif kind=='rune':
        c.polygon([(126,111),(145,121),(145,144),(126,155),(108,144),(108,121)],accent);c.line([(126,118),(116,135),(135,135),(126,148)],light,3)
    elif kind=='cores':
        c.polygon([(126,112),(145,133),(126,155),(107,133)],accent);c.polygon([(126,122),(135,133),(126,144),(117,133)],light)
    else:
        c.polygon([(110,145),(108,127),(119,114),(135,116),(146,134),(137,153),(120,157)],accent);c.line([(119,115),(122,137),(137,153)],light,2)
    if box.get('awakened') or box.get('setOnly'):
        c.polygon([(114,39),(120,48),(127,34),(135,48),(142,39),(139,57),(117,57)],accent)
    marks=box['tier'] if kind=='gem' else box['grade'] if kind=='rune' else 0
    for n in range(marks):c.rect(126-marks*5+n*10,189,6,8,accent)
    # A colored wax tab identifies choice packs. Counts are supplied by the UI, never baked tiny text.
    if kind in ('gem','cores') and (not gem if kind=='gem' else slot<0):
        for n,color in enumerate(list(accents.values())[:3]):c.polygon([(184+n*7,155),(191+n*7,155),(191+n*7,181),(187+n*7,176),(184+n*7,181)],color)
    return c


def main():
    db=catalog();ART.mkdir(parents=True,exist_ok=True);SOURCE.mkdir(parents=True,exist_ok=True);DATA.parent.mkdir(parents=True,exist_ok=True)
    DATA.write_text(json.dumps(db,ensure_ascii=False,indent=2)+'\n')
    manifest=[];tiles=[]
    for row in db['boxes']:
        c=artwork(row);png=c.image.resize((256,256),Image.Resampling.LANCZOS);path=ART/(row['id']+'.png');png.save(path)
        (SOURCE/(row['id']+'.svg')).write_text('<svg xmlns="http://www.w3.org/2000/svg" width="256" height="256" viewBox="0 0 256 256">'+''.join(c.svg)+'</svg>\n')
        alpha=png.getchannel('A');assert alpha.getextrema()==(0,255)
        assert all(alpha.getpixel((x,y))==0 for x,y in [(0,0),(255,0),(0,255),(255,255)])
        manifest.append(dict(id=row['id'],width=256,height=256,mode=png.mode,transparentPixels=alpha.histogram()[0],sha256=hashlib.sha256(path.read_bytes()).hexdigest()))
        tile=Image.new('RGB',(180,198),'#181c19');tile.paste(png.resize((156,156)),(12,0),png.resize((156,156)))
        ImageDraw.Draw(tile).text((6,163),row['id'],fill='#ded4b6');tiles.append(tile)
    sheet=Image.new('RGB',(180*8,198*((len(tiles)+7)//8)),'#181c19')
    for n,tile in enumerate(tiles):sheet.paste(tile,(n%8*180,n//8*198))
    sheet.save(SOURCE/'contact-sheet.png')
    (SOURCE/'manifest.json').write_text(json.dumps(dict(provenance='Code-authored original vector artwork; no image model used',generator='tools/generate_reward_boxes.py',icons=manifest),indent=2)+'\n')
    print(f'{len(db["boxes"])} box definitions, SVG originals and transparent PNG sprites generated.')


if __name__=='__main__':main()
