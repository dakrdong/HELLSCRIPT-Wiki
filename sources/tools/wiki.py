#!/usr/bin/env python3
"""Build a local, source-grounded HELLSCRIPT wiki. Never writes game assets."""
from __future__ import annotations
import argparse
import datetime as dt
import hashlib
import html
import http.server
import json
import re
import shutil
import sys
import tempfile
import xml.etree.ElementTree as ET
from pathlib import Path
from urllib.parse import quote, unquote, urlsplit

ROOT = Path(__file__).resolve().parents[1]
SITE = ROOT / 'Wiki/site'
CORE = 'Assets/HELLSCRIPT/Runtime/Core/'
ART = 'Assets/HELLSCRIPT/Resources/Art/'
DESIGN = 'Docs/Design/'
IMPL = 'Docs/Implementation/'
CLASSES = ['전사', '궁수', '마법사']
SLOTS = ['무기', '머리', '몸통', '손', '발', '허리', '목걸이', '반지']
NOW = dt.datetime.now(dt.timezone(dt.timedelta(hours=9))).isoformat(timespec='seconds')
INPUTS = {}

def read(path):
    data = (ROOT / path).read_bytes()
    INPUTS[str(path)] = hashlib.sha256(data).hexdigest()
    return data.decode('utf-8-sig')

def digest(data):
    return hashlib.sha256(data).hexdigest()

def save(path, obj):
    path.parent.mkdir(parents=True, exist_ok=True)
    content = json.dumps(obj, ensure_ascii=False, indent=2) + '\n'
    if not path.exists() or path.read_text() != content:
        temporary = path.with_suffix(path.suffix + '.tmp')
        temporary.write_text(content)
        temporary.replace(path)

def page_id(path):
    return Path(path).stem.lower().removeprefix('hellscript_').replace('_', '-')

def slug(text):
    return re.sub(r'[^\w\s-]', '', text.lower()).strip().replace(' ', '-')

def split_args(text):
    result, start, depth, quoted, escaped = [], 0, 0, False, False
    for i, char in enumerate(text):
        if quoted:
            if escaped: escaped = False
            elif char == '\\': escaped = True
            elif char == '"': quoted = False
        elif char == '"': quoted = True
        elif char in '([{': depth += 1
        elif char in ')]}': depth -= 1
        elif char == ',' and depth == 0:
            result.append(text[start:i].strip()); start = i + 1
    if quoted or depth: raise ValueError('Unbalanced C# arguments')
    result.append(text[start:].strip())
    return result

def scalar(text):
    text = text.strip()
    if text.startswith('"') and text.endswith('"'): return json.loads(text)
    if text in ('true', 'false'): return text == 'true'
    if re.fullmatch(r'-?(?:\d+\.?\d*|\.\d+)[fFdD]?', text):
        value = float(text.rstrip('fFdD')); return int(value) if value.is_integer() else value
    return text

def stat_numbers(path):
    source = read(path)
    start = source.index('public enum StatId')
    block = source[start:source.index('}', start)]
    numbers = {m.group(1): int(m.group(2)) for m in re.finditer(r'(\w+)\s*=\s*(\d+)', block)}
    if not numbers: raise ValueError('StatId carries no numbered members')
    return numbers

def constructors(path, name):
    source = read(path); result = []
    for match in re.finditer(r'new\s+' + re.escape(name) + r'\(', source):
        start = match.end(); depth = 1; quoted = False; escaped = False; end = start
        for end in range(start, len(source)):
            c = source[end]
            if quoted:
                if escaped: escaped = False
                elif c == '\\': escaped = True
                elif c == '"': quoted = False
            elif c == '"': quoted = True
            elif c == '(': depth += 1
            elif c == ')':
                depth -= 1
                if depth == 0: break
        if depth: raise ValueError('Unclosed constructor: ' + path)
        result.append(([scalar(x) for x in split_args(source[start:end])], source[:match.start()].count('\n') + 1))
    return result

def string_array(path, name):
    source = read(path)
    match = re.search(r'\b' + re.escape(name) + r'\s*=\s*\{(.*?)\};', source, re.S)
    if not match: raise ValueError('Missing string array ' + name)
    return [json.loads(x) for x in re.findall(r'"(?:\\.|[^"\\])*"', match[1])]

def source_ref(path, line=1):
    if path not in INPUTS: read(path)
    return {'path': path, 'line': line}

def record(identifier, name, category, summary, fields, path, line=1, status='현재 정의', **extra):
    return dict(id=str(identifier), name=name, category=category, summary=summary, fields=fields,
                source=source_ref(path, line), status=status, **extra)

def db(identifier, name, description, rows, **extra):
    if len({r['id'] for r in rows}) != len(rows): raise ValueError('Duplicate record in ' + identifier)
    return dict(id=identifier, name=name, description=description, rows=rows, **extra)

def table_rows(path):
    rows = {}; headings = []; header = None
    for n, line in enumerate(read(path).splitlines(), 1):
        if line.startswith('#'): headings = [line.lstrip('# ').strip()]
        if not line.startswith('|'): header = None; continue
        cells = [s.strip() for s in line.strip().strip('|').split('|')]
        if header is None: header = cells; continue
        if all(re.fullmatch(r':?-+:?', c.replace(' ', '')) for c in cells): continue
        identifier = cells[0].split(' ')[0]
        if re.fullmatch(r'(?:[A-Z]+\d+|WARRIOR|RANGER|MAGE)', identifier):
            rows.setdefault(identifier, dict(cells=cells, fields=dict(zip(header,cells)), line=n, section=headings))
    return rows

def build_databases():
    import yaml
    tables = table_rows(DESIGN + 'HELLSCRIPT_Content_Catalog.md')
    catalog_path = CORE + 'GameCatalog.cs'; item_path = CORE + 'Itemization.cs'
    data = []
    skills_path = 'Assets/HELLSCRIPT/Resources/GameCatalog.asset'
    asset_text = read(skills_path)
    asset = yaml.safe_load('\n'.join(x for x in asset_text.splitlines() if not x.startswith(('%', '---'))))['MonoBehaviour']['skills']
    definitions = {a[0]:(a,line) for a,line in constructors(catalog_path,'SkillDefinition')}
    rows = []
    for s in asset:
        a, line = definitions[s['id']]
        for field,index in [('name',1),('unlock',4),('cooldown',5),('cost',6),('coefficient',7),('range',8),('radius',9),('duration',10),('icon',11),('description',12)]:
            if s[field] != a[index]: raise ValueError(f'Skill asset/source mismatch: {s["id"]}.{field}')
        fields = {'직업':CLASSES[s['heroClass']], '해금 레벨':s['unlock'], 'CD (초)':s['cooldown'],
                  '자원 비용':str(s['cost'])+(' / 0.25초 유료 틱' if s['id']=='W01' else ' / 1회'),
                  '기본 계수 (D 배수)':s['coefficient'], '거리 필드 range (m)':s['range'],
                  '공간 필드 radius':s['radius'], '시간 필드 duration (초)':s['duration'],
                  '필드 해석':'radius·duration의 역할은 스킬별 판정에서 확인합니다. 대상 수·폭·효과 시간·동작 시간을 공통 반경·지속시간으로 해석하지 않습니다.',
                  '효과 설명':s['description']}
        rows.append(record(s['id'],s['name'],CLASSES[s['heroClass']],s['description'],fields,skills_path,
                           next(i for i,x in enumerate(asset_text.splitlines(),1) if x.strip()=='- id: '+s['id']),
                           image={'file':'SkillAtlas.png','cell':s['icon']},refs=[source_ref(catalog_path,line)],related=['action-expansion','effect-expansion']))
    data.append(db('skills','액티브 스킬','현재 에셋과 생성 코드가 일치하는 액티브 18종입니다. 수치는 개발 시험값입니다.',rows))
    unlock_path = 'Assets/HELLSCRIPT/Resources/ContentUnlocks.json'
    unlocks = json.loads(read(unlock_path))
    unlock_rows = [record(f['id'],f['name'],'계정 공통',f['guide'],
        {'조건': '마을 도착부터 언제든 이용 가능' if f['id']=='UL01_TRAIN' else f"최고 실제 보상 확정 클리어 >= {f['stage']}단",
         '조기 해금':f['early'] or '없음','권한 유지':'소비·안내 건너뛰기 후에도 유지','수치 상태':unlocks['status']},unlock_path,
        status='테스트 초안',related=['content-unlock-expansion','content-unlock-spec-v0.1']) for f in unlocks['features']]
    data.append(db('content-unlocks','콘텐츠 해금','실제 게임의 공통 JSON을 읽습니다. 보석 획득·소켓·합성은 로컬 플레이에 연결했으며, 서버 동기화는 후속 범위입니다.',unlock_rows))
    passive_slots = ' / '.join(f'Lv.{level}: {i+1}개' for i,level in enumerate(unlocks['passiveLevels']))+' (적법한 기존 슬롯 보존)'
    names=string_array(catalog_path,'Passives'); desc=string_array(catalog_path,'PassiveDescriptions'); rows=[]
    for i,(name,description) in enumerate(zip(names,desc)):
        identifier=['WP','AP','MP'][i//6]+f'{i%6+1:02}'
        rows.append(record(identifier,name,CLASSES[i//6],description,{'직업':CLASSES[i//6],'효과':description,'직업별 장착 한도':passive_slots},catalog_path,
                           read(catalog_path)[:read(catalog_path).index(json.dumps(description,ensure_ascii=False))].count('\n')+1,related=['passive-combat-detail','passive-expansion']))
    data.append(db('passives','패시브','현재 게임 설명과 패시브 개발 기록을 연결합니다.',rows))
    rows=[]
    for i,key in enumerate(['WARRIOR','RANGER','MAGE']):
        t=tables[key]; rows.append(record(key,CLASSES[i],CLASSES[i],t['cells'][-1],t['fields'],DESIGN+'HELLSCRIPT_Content_Catalog.md',t['line'],status='기획·구현 기록',image={'file':'SkillAtlas.png','cell':18+i},related=['unity-implementation-plan','player-training-detail']))
    data.append(db('heroes','직업·기본 공격','기획의 직업 기본값입니다. 실제 최종 능력치는 레벨·장비·효과 계산 결과를 따릅니다.',rows))
    rows=[]
    for a,line in constructors(CORE+'BehaviorRules.cs','ConditionDefinition'):
        if not isinstance(a[0],int): continue
        comp={'AtMost':'이하','AtLeast':'이상','Present':'있음 / 가능','Absent':'없음','Soon':'완료 임박','Inside':'안','Outside':'밖','Equal':'일치','Between':'범위'}
        ops=' / '.join(comp[str(x).split('.')[-1]] for x in a[7:])
        rows.append(record(f'SC{a[0]:02}',a[1],'행동 조건',f'{ops} · {a[3]}–{a[4]} {a[2]}',
            {'단위':a[2] or '목록 선택','최솟값':a[3],'최댓값':a[4],'증가 단위':a[5],'기본값':a[6],'비교':ops},CORE+'BehaviorRules.cs',line,related=['rule-expansion','gameplay-completion-checklist']))
    data.append(db('conditions','행동 조건','SC01–SC22의 입력 범위와 비교 연산을 현재 코드에서 추출했습니다.',rows))
    rows=[]
    preset_names=['회오리 생존','도약 분쇄','관통 거리유지','맹독 매복','서리 장판','연쇄 제어']
    for i,key in enumerate(['PW01','PW02','PA01','PA02','PM01','PM02']):
        t=tables[key]; rows.append(record(key,preset_names[i],CLASSES[i//2],t['cells'][4],t['fields'],DESIGN+'HELLSCRIPT_Content_Catalog.md',t['line'],status='기획·구현 기록',refs=[source_ref(CORE+'BehaviorPresets.cs')],related=['build-integration-detail','build-integration-expansion']))
    data.append(db('builds','추천 빌드','6개 추천 빌드의 기획 요약입니다. 실제 규칙 생성은 BehaviorPresets의 현재 소스를 따릅니다.',rows))
    rows=[]
    for a,line in constructors(item_path,'ItemBaseDefinition'):
        a=a+[0]*(9-len(a)); c=CLASSES[a[4]] if a[4]>=0 else '공용'
        rows.append(record(a[0],a[1],SLOTS[a[3]],f'{c} · {SLOTS[a[3]]} · 기본값 {a[5]}',
          {'직업':c,'부위':SLOTS[a[3]],'기본 주 수치':a[5],'기본 비물리 저항':a[6],'공격속도 보정':a[7],
           '수치 해석':'아이템 레벨과 강화가 적용되기 전의 베이스 값입니다. 주 수치의 능력치 종류는 부위별 적용 코드를 따릅니다.'},item_path,line,image={'file':'EquipmentAtlas.png','cell':a[2]},related=['itemization-detail','itemization-expansion']))
    data.append(db('items','장비 베이스','B01–B24의 베이스·직업·부위·기본 수치와 실제 아틀라스 아이콘입니다.',rows))
    # The stat table is the one place a stat's name and unit are written, so both the attribute
    # database and the affix database read it rather than keeping a list of their own.
    attribute_path = CORE + 'Attributes.cs'
    numbers = stat_numbers(attribute_path)
    PAGES = {'Core':'핵심', 'Off':'공격', 'Def':'방어', 'Res':'자원', 'Util':'유틸리티'}
    FORMS = {'Flat':'고정값', 'Pct':'백분율 (%)', 'Met':'거리 (m)', 'Sec':'초당'}
    INERT = {'CrowdControlDuration':'영웅은 군중 제어를 당하지 않아 줄일 대상이 없습니다.',
             'MaximumStamina':'디아블로4 본편 캐릭터 시트에 없는 줄이며, 이 게임에는 소모할 곳이 없습니다.',
             'StaminaRegeneration':'디아블로4 본편 캐릭터 시트에 없는 줄이며, 이 게임에는 소모할 곳이 없습니다.'}
    statnames={}; rows=[]
    for a,line in constructors(attribute_path,'StatDefinition'):
        key=str(a[0]).split('.')[-1]; index=numbers[key]; statnames[index]=a[1]
        cap=a[4] if len(a)>4 else 0
        inert=INERT.get(key)
        rows.append(record(f'ST{index:02d}',a[1],PAGES[str(a[2])],
          ('이 줄은 아직 적용되는 곳이 없습니다. '+inert) if inert else f'{PAGES[str(a[2])]} 분류의 {FORMS[str(a[3])]} 항목입니다.',
          {'속성 번호':index,'분류':PAGES[str(a[2])],'단위':FORMS[str(a[3])],
           '시트 표시 단위':'실제 이동 속도 (m/s)' if key=='MovementSpeed' else FORMS[str(a[3])],
           '상한':cap if cap else '이 줄 자체에는 상한이 없습니다. 소비하는 계산이 범위를 정합니다.',
           '전투 적용':inert if inert else '전투 계산에 연결되어 있습니다.',
           '번호 보존':'0–23번은 저장 파일이 적어 둔 접사 번호이므로 뜻을 바꾸지 않습니다.' if index<24 else '24번부터는 디아블로4 캐릭터 시트를 옮기며 새로 붙인 번호입니다.'},
          attribute_path,line,related=['attribute-system-detail','attribute-system-expansion']))
    data.append(db('attributes','능력치','디아블로4 캐릭터 시트에서 옮긴 57개 속성입니다. 분류·단위·상한과 저장 번호를 표시합니다.',rows))
    rows=[]
    for a,line in constructors(item_path,'AffixDefinition'):
        rows.append(record(a[0],statnames[a[1]],'접두사' if a[2]=='P' else '접미사',a[3]+' · '+statnames[a[1]],
          {'올리는 능력치':statnames[a[1]],'속성 번호':a[1],'명칭 구성':a[3],'중복 그룹':a[4],'기본 가중치':a[5],'최소 기준값':a[6],'최대 기준값':a[7],
           '평면 수치':a[8],'허용 부위':' · '.join(SLOTS[i] for i in a[9:]),
           '레벨 보정':'평면 수치는 (1 + 0.03 × (레벨 − 1)) / 1.87을 곱합니다.' if a[8] else '수치 범위에 레벨 배수를 곱하지 않습니다.',
           '추첨 주의':'가중치는 상대값입니다. 직업 선호 옵션은 별도 가중치를 적용합니다.'},item_path,line,related=['itemization-detail','attribute-system-expansion']))
    data.append(db('affixes','접두사·접미사','접사 54종의 현재 분류, 올리는 능력치와 허용 부위입니다. 전투에 적용되는 속성마다 하나씩 있습니다.',rows))
    legends=[]
    for a,line in constructors(item_path,'UniqueItemDefinition'):
        if not re.fullmatch(r'L[WAMC]\d{2}',str(a[0])): continue
        c=CLASSES[a[2]] if a[2]>=0 else '공용'
        legends.append(record(a[0],a[1],c,a[5],{'직업':c,'부위':SLOTS[a[3]],'상대 가중치':a[4],
          '효과':a[5],'요구 스킬':a[6] if len(a)>6 else '없음','아이콘':'사용한 베이스 아이콘을 공유합니다. 독립 전설 아트가 아닙니다.'},item_path,line,related=['itemization-detail','legendary-expansion']))
    data.append(db('legendaries','개별 전설','전설 15종의 현재 설명입니다. 가중치 100 / 20 / 1은 드롭 백분율이 아닙니다.',legends))
    sets=[]; pieces=[]
    for a,line in constructors(item_path,'SetDefinition'):
        c=CLASSES[['HeroClass.Warrior','HeroClass.Ranger','HeroClass.Mage'].index(a[2])]
        sets.append(record(a[0],a[1],c,a[3]+' '+a[4],{'직업':c,'2세트':a[3],'4세트':a[4],'장착 부위':'머리 · 몸통 · 손 · 발'},item_path,line,related=['itemization-detail','charge-expansion']))
        for s in range(1,5):
            pieces.append(record(a[0]+str(s),a[1]+'의 '+['','관','갑옷','손아귀','걸음'][s],c,f'{SLOTS[s]} · {a[1]}',
                {'직업':c,'부위':SLOTS[s],'세트 ID':a[0],'상대 가중치':100,'2세트':a[3],'4세트':a[4],
                 '생성 규칙':'현재 BuildUniques의 세트별 1–4부위 생성식을 재현합니다. 해당 부위의 전설과 동시에 장착할 수 없습니다.'},item_path,145,refs=[source_ref(item_path,line)],related=['itemization-detail']))
    data.append(db('sets','세트 효과','현재 6세트입니다. 초기 카탈로그의 3세트 기록은 당시 범위로 보존합니다.',sets))
    data.append(db('set-items','세트 장비','6세트 × 머리·몸통·손·발의 24종입니다. 세트 정의 자체는 아이템 수에 추가하지 않습니다.',pieces))
    for key,name,prefix,count,name_index,code,related in [
        ('enemies','일반 몬스터','N',12,1,'EnemyCombat.cs','enemy-expansion'),
        ('elites','정예 특성','E',6,1,'CombatSimulation.Enemies.cs','enemy-expansion'),
        ('bosses','보스','BOSS',3,1,'BossCombat.cs','boss-expansion')]:
        rows=[]
        for i in range(1,count+1):
            ident=f'{prefix}{i:02}'; t=tables[ident]
            rows.append(record(ident,t['cells'][name_index],name,' · '.join(t['cells'][2:]),t['fields'],DESIGN+'HELLSCRIPT_Content_Catalog.md',t['line'],
                status='기획·구현 기록',refs=[source_ref(CORE+code)],related=[related],resource='procedural-enemies'))
        data.append(db(key,name,'기획 표의 패턴·설계 의도입니다. 후속 구현의 세부 수치와 예외는 연결된 개발 기록을 우선 확인합니다.',rows))
    rows=[]
    for a,line in constructors(CORE+'RiftLayout.cs','RoomTemplate'):
        rows.append(record(a[0],a[1],'묘지' if int(a[0][2:])<=6 else '성채',f'{a[2]} × {a[3]}m · '+('보스 배치 가능' if a[4] else '일반 방'),
          {'가로 (m)':a[2],'세로 (m)':a[3],'보스 배치':a[4],'출입구 좌표 정의':a[5],'적 무리 좌표 정의':a[6],'상자 앵커 정의':a[7],
           '좌표 기준':'방 로컬 좌표입니다. 실제 생성 지도에서 회전·배치한 좌표와 구분합니다.'},CORE+'RiftLayout.cs',line,related=['rift-exploration-detail','rift-expansion'],resource='procedural-world'))
    data.append(db('rooms','방 템플릿','현재 12개 방의 크기·보스 허용·연결구와 배치 앵커를 코드에서 추출했습니다.',rows))
    fieldtables=table_rows(DESIGN+'HELLSCRIPT_Rift_Exploration_Detail.md');rows=[]
    for ident in ['CH01','CH02','CH03','SH01','SH02']:
        t=fieldtables[ident];rows.append(record(ident,t['cells'][0].removeprefix(ident+' '),'상자' if ident.startswith('CH') else '성소',
            ' · '.join(t['cells'][1:]),t['fields'],DESIGN+'HELLSCRIPT_Rift_Exploration_Detail.md',t['line'],status='기획·구현 기록',
            refs=[source_ref(CORE+'RiftFieldContent.cs')],related=['field-expansion','rift-exploration-detail'],resource='procedural-interactions'))
    data.append(db('field','상자·성소','CH01–CH03과 SH01–SH02입니다. 초기 기획의 구현 순서와 현재 적용 여부는 개발 기록을 함께 봅니다.',rows))
    expected={'content-unlocks':9,'skills':18,'passives':18,'heroes':3,'conditions':22,'builds':6,'items':24,'attributes':57,'affixes':54,'legendaries':15,'sets':6,'set-items':24,'enemies':12,'elites':6,'bosses':3,'rooms':12,'field':5}
    for table in data:
        if len(table['rows']) != expected[table['id']]: raise ValueError('Review changed catalog count: '+table['id'])
    import wiki_runes
    data.extend(wiki_runes.build(argparse.Namespace(read=read, record=record, source_ref=source_ref, db=db, constructors=constructors)))
    import wiki_potions
    data.extend(wiki_potions.build(argparse.Namespace(read=read, record=record, source_ref=source_ref, db=db)))
    return data

def build_resources(databases):
    from PIL import Image
    rows=[]
    uses={'Sanctuary.png':'성소·결과 화면의 배경','SkillAtlas.png':'액티브 18개·직업 3개와 기존 장비용 3셀',
          'EquipmentAtlas.png':'B01–B24 장비 목록·상세 아이콘','RiftStone.png':'균열 바닥 재질'}
    sizes={}
    for filename in sorted(uses):
        path=ART+filename; raw=(ROOT/path).read_bytes(); INPUTS[path]=digest(raw)
        with Image.open(ROOT/path) as im:
            sizes[filename]=im.size
            alpha=im.getchannel('A') if 'A' in im.getbands() else None
            histogram=alpha.histogram() if alpha else None
            fields={'원본 경로':path,'해상도':f'{im.width} × {im.height}','색상 모드':im.mode,
                    '파일 크기 (바이트)':len(raw),'완전 투명 픽셀':histogram[0] if histogram else 0,
                    '알파 범위':str(alpha.getextrema()) if alpha else '알파 채널 없음',
                    '사용처':uses[filename],'SHA-256':digest(raw),'제작 방법':'Codex 내장 image_gen (기존 기록)',
                    '출처 확인':'기존 생성 기록에 gpt-image / 2.0 확인이 있습니다.' if filename in ('EquipmentAtlas.png','RiftStone.png') else '기존 기록에서 생성 모델명을 확인하지 못했습니다.',
                    '승인 상태':'개발용 임시 자산입니다. 출시 승인·외곽선 시각 검수 완료를 뜻하지 않습니다.'}
        meta=read(path+'.meta'); fields['Unity GUID']=re.search(r'^guid: (\w+)',meta,re.M)[1]
        # Binary source references are recorded without decoding the original image.
        rows.append(record('asset-'+Path(filename).stem.lower(),filename,'이미지 원본',uses[filename],fields,
                   IMPL+'Asset_Provenance.md',status='임시 사용',image={'file':filename},assetPath=path,
                   refs=[source_ref('Assets/HELLSCRIPT/Runtime/Presentation/'+('WorldView.Rift.cs' if filename=='RiftStone.png' else 'GameUI.cs'))],
                   related=['asset-provenance','equipment-atlas-prompt' if filename=='EquipmentAtlas.png' else 'rift-stone-prompt' if filename=='RiftStone.png' else 'image-prompts']))
    storage_path=ART+'GlobalHUD/menu-storage.png'
    storage_raw=(ROOT/storage_path).read_bytes();INPUTS[storage_path]=digest(storage_raw)
    storage_meta=read(storage_path+'.meta')
    storage_provenance=json.loads(read(IMPL+'TownHudEvidence/provenance.json'))
    rows.append(record('ui-menu-storage','창고 바로가기 아이콘','콘텐츠 바로가기 아이콘',
        '사용자가 첨부한 투명 PNG를 마을·전투의 실제 창고 바로가기에 적용했습니다.',
        {'원본 경로':storage_path,'해상도':'1254 × 1254','색상 모드':'RGBA','제작 방법':'사용자 첨부 원본을 변경 없이 복사',
         '완전 투명 픽셀':storage_provenance['fullyTransparentPixels'],'SHA-256':digest(storage_raw),
         'Unity GUID':re.search(r'^guid: (\w+)',storage_meta,re.M)[1],'사용처':'GameUI.ContentDock → ShowStorage'},
        IMPL+'Town_Hud_Responsive.md',status='사용 중',image={'file':'GlobalHUD/menu-storage.png'},assetPath=storage_path,
        refs=[source_ref('Assets/HELLSCRIPT/Runtime/Presentation/GameUI.ContentDock.cs')],related=['town-hud-responsive','play-content-dock']))
    byid={d['id']:d for d in databases}
    for group in ['skills','heroes','items']:
        for item in byid[group]['rows']:
            image=item['image'];cell=image['cell'];width,height=sizes[image['file']]
            item['resource']='icon-'+item['id']
            rows.append(record('icon-'+item['id'],item['name']+' 아이콘','장비 아이콘' if group=='items' else '스킬·직업 아이콘',
                f'{item["id"]} · {image["file"]} · {cell//6+1}행 {cell%6+1}열',
                {'콘텐츠 ID':item['id'],'원본':image['file'],'셀 번호 (0부터)':cell,'행·열 (1부터)':f'{cell//6+1}행 {cell%6+1}열',
                 '영역 크기':f'{width//6} × {height//4}', 'Unity UV':f'({cell%6}/6, 1−({cell//6}+1)/4, 1/6, 1/4)',
                 '등록 방식':'독립 파일이 아닌 아틀라스 영역입니다. 원본 알파를 그대로 사용합니다.'},
                 'Assets/HELLSCRIPT/Runtime/Presentation/'+('GameUI.Items.cs' if group=='items' else 'GameUI.cs'),
                 17 if group=='items' else 72,status='임시 사용',image=image,content={'db':group,'id':item['id']},related=['resource-guide']))
    for cell in [21,22,23]:
        rows.append(record(f'legacy-cell-{cell}','기존 장비 표시 셀 '+str(cell-20),'미사용 참고 셀',
            'SkillAtlas의 원래 장비 표시용 영역입니다. 현재 장비는 EquipmentAtlas를 사용합니다.',
            {'셀 번호 (0부터)':cell,'현재 장비 아이콘':'EquipmentAtlas.png / B01–B24','사용 상태':'현재 GameUI 호출에서 이 셀의 사용을 확인하지 못했습니다.'},
            IMPL+'Asset_Provenance.md',status='참고 보존',image={'file':'SkillAtlas.png','cell':cell}))
    for ident,name,path,purpose in [
        ('unity-catalog','게임 콘텐츠 카탈로그','Assets/HELLSCRIPT/Resources/GameCatalog.asset','액티브 스킬의 직렬화된 Unity 카탈로그입니다.'),
        ('unity-material','런타임 셰이더 재질','Assets/HELLSCRIPT/Resources/Materials/RuntimeShader.mat','런타임 표현에 사용하는 Unity 재질 자산입니다.'),
        ('unity-scene','HELLSCRIPT 시작 씬','Assets/HELLSCRIPT/Scenes/Hellscript.unity','게임의 기존 시작 씬입니다.')]:
        raw=read(path);meta=read(path+'.meta')
        rows.append(record(ident,name,'Unity 자산',purpose,
            {'경로':path,'Unity GUID':re.search(r'^guid: (\w+)',meta,re.M)[1],
             '역할':purpose,'SHA-256':digest((ROOT/path).read_bytes()),
             '변경 기준':'기존 프로젝트 구성 도구와 Unity 자산 소유 규칙을 따릅니다. 위키는 조회 사본만 만듭니다.'},
            path,status='현재 등록',refs=[source_ref('Assets/HELLSCRIPT/Editor/ProjectBuilder.cs')],related=['playable-build','resource-guide']))
    for group in ['heroes','enemies','bosses','rooms','field']:
        for item in byid[group]['rows']:
            kind={'heroes':'영웅 모델','enemies':'일반 적 모델','bosses':'보스 모델','rooms':'방·환경','field':'상호작용 오브젝트'}[group]
            code='WorldView.Rift.cs' if group in ('rooms','field') else 'WorldView.cs'
            ident='model-'+item['id'];item['resource']=ident
            rows.append(record(ident,item['name']+' 표현',kind,'기본 메시를 조합하는 임시 런타임 표현입니다.',
                {'콘텐츠 ID':item['id'],'표현 방식':'기본 메시·재질·런타임 배치','독립 모델·프리팹':'이 콘텐츠의 정식 모델·애니메이션 파일은 등록되지 않았습니다.',
                 '후속 제작':'콘텐츠별 식별·상태·공격 예고를 보존하는 정식 아트를 제작하고 기기에서 검증합니다.'},
                'Assets/HELLSCRIPT/Runtime/Presentation/'+code,status='임시 사용',content={'db':group,'id':item['id']},related=['resource-guide']))
    for ident,name,summary,path in [
        ('procedural-enemies','적·정예 위험 형상','적의 위험 외곽선과 생명 연결·전방 방벽을 LineRenderer로 표시합니다.','WorldView.Enemies.cs'),
        ('procedural-effects','전투 효과·위험 예고','LineRenderer와 기본 메시로 타격·장판·사선을 표시합니다.','WorldView.Actions.cs'),
        ('procedural-ui','게임 UI·게이지·미니맵','uGUI로 상태·게이지·버튼·텍스트를 구성합니다.','GameUI.cs')]:
        rows.append(record(ident,name,'코드 생성 표현',summary,{'제작 방식':summary,'후속 제작':'작은 화면의 대비·형태 구분과 실제 판정 시점을 검증합니다.'},'Assets/HELLSCRIPT/Runtime/Presentation/'+path,status='임시 사용',related=['resource-guide']))
    for ident,name,summary,path,extra,related,follow in [
        ('procedural-battle-hud','전투 HUD 배치','화면 크기·보스 표시에 따라 HUD 영역과 카메라 표시 사각형을 계산하고 같은 HUD 객체를 재배치합니다.','BattleHudLayout.cs',['GameUI.BattleLayout.cs'],['battle-layout-expansion','screen-layout-detail'],'성소·사냥 칙령·결과·훈련 등 나머지 화면의 재배치와 모바일 실기기의 회전·밀도·터치·노치 검증이 남아 있습니다.'),
        ('procedural-exploration','탐색 안개·미니맵','방문 기록과 고정 관찰 범위에 따라 생성 지형을 방·통로 단위로 표시하고 미니맵을 그립니다. 카메라 크기로 탐색 기록을 추가하지 않습니다.','WorldView.Exploration.cs',['RiftMinimap.cs'],['battle-layout-expansion','rift-exploration-detail'],'탐색 안개 경계의 미술적 개선과 실제 기기의 렌더링 비용 측정이 남아 있습니다.'),
        ('procedural-inventory','장비 목록·비교 화면','화면 너비에 따라 목록·상세·비교를 한 영역·두 영역·세 영역으로 배치하고 필터·정렬·일괄 정리 미리보기를 표시합니다.','InventoryLayout.cs',['GameUI.InventoryLayout.cs'],['inventory-management-expansion','itemization-detail'],'실제 모바일 밀도·터치 검증과 도감·상점 등 나머지 장비 화면의 재배치가 남아 있습니다.'),
        ('procedural-settings','톱니바퀴 설정·실제 플레이 화면','가로 화면은 오른쪽 반쪽 메뉴와 왼쪽의 실제 마을·사냥터 플레이어, 세로 화면은 전체 메뉴를 표시합니다. X·배경 닫기, 화면·소리·언어·캐릭터 변경, 비율 10종과 글자 크기·가시거리 각각 50~150%를 제공합니다.','GameUI.ScreenSettings.cs',['GameUI.SettingsFrame.cs','SettingsControls.cs','WorldView.Settings.cs','GameUI.ViewDistance.cs'],['settings-revision','screen-settings-expansion','reading-size-expansion','touch-and-safe-area-expansion'],'글자 크기와 가시거리는 각각 5%씩 조절하며 캐릭터 변경 후 마을로 돌아갑니다. 모바일 실기기 방향 전환과 터치는 후속 검증 대상입니다.'),
        ('procedural-guide','첫 플레이 안내 화면','성소·게임 안내·결과에서 여는 아홉 단계 안내와 진행 수 표시입니다. 숨기기는 추천 표시만 끄고 기록을 바꾸지 않습니다.','GameUI.Guide.cs',[],['first-play-expansion','first-play-detail'],'신규 사용자의 첫 15분 관찰과 이해도 검사는 자동 검사로 대체하지 않습니다.'),
        ('procedural-growth','성장·레벨업 화면','레벨업 기록, 능력치·해금 표시와 현재 레벨 추천안의 미리보기·명시적 적용 화면입니다.','GameUI.Growth.cs',[],['growth-expansion','growth-detail'],'여러 시드에서의 실제 성장 구간 비교와 화면 검증이 남아 있습니다.'),
        ('procedural-comparison','훈련 A/B 비교 화면','훈련 선택, B 설정 편집, 조건·변경 목록과 A/B 결과·기록 화면입니다.','GameUI.Comparison.cs',[],['training-comparison-expansion','training-comparison-detail'],'장비 교체 훈련과 사냥 칙령 슬롯 이관은 별도 후속 범위입니다.'),
        ('procedural-rules','행동 규칙·프리셋 편집 화면','행동 규칙 편집, 판단 기록 열람과 프리셋 5칸 저장 대화창을 uGUI로 구성합니다.','GameUI.Rules.cs',['GameUI.Presets.cs'],['rule-expansion','hunt-edict-expansion','screen-layout-expansion'],'핵심 옵션 v0.2의 새 편집 화면과 공유 코드 화면은 개발 중입니다.'),
        ('procedural-combat-info','전투 효과·적 정보 열람 화면','전투 효과 목록과 적·정예·보스의 전투 정보를 읽는 열람 화면입니다.','GameUI.Effects.cs',['GameUI.Enemies.cs'],['effect-expansion','enemy-expansion'],'작은 화면에서 긴 목록의 가독성과 실제 판정 시점의 표시를 검증합니다.'),
        ('procedural-plaza','숲속 정착민 마을·NPC·균열 포탈','110×70m 숲속 마을에 건물 앞 NPC 6명과 중앙 오른쪽 주황·금빛 포탈을 둡니다. 원형 조이스틱, 우측 고정 상호작용, 건물 충돌·우회와 가림 투명화를 제공합니다.','WorldView.Town.cs',['GameUI.Plaza.cs','GameUI.TownServices.cs','TownJoystick.cs','TownBuildingFade.cs'],['forest-settlement','town-plaza-expansion','resource-guide'],'교체 가능한 3D 메시이며 영웅은 기존 모델을 공유합니다. 모바일 실기기의 터치·성능 검증과 미술 품질 개선이 남아 있습니다.')]:
        rows.append(record(ident,name,'코드 생성 표현',summary,{'제작 방식':summary,'구현 파일':' · '.join([path]+extra),'후속 제작':follow},
            'Assets/HELLSCRIPT/Runtime/Presentation/'+path,status='임시 사용',refs=[source_ref('Assets/HELLSCRIPT/Runtime/Presentation/'+x) for x in extra],related=related+['resource-guide']))
    rows.append(record('font-runtime','한국어 런타임 글꼴','글꼴','운영체제의 한국어 글꼴을 우선 사용합니다.',
        {'글꼴 후보':'Apple SD Gothic Neo, Malgun Gothic, Noto Sans CJK KR, Noto Sans CJK, Droid Sans Fallback, Arial',
         '재배포':'OS 글꼴 파일을 프로젝트에 복사하지 않았습니다.','후속 확인':'Android 배포 글꼴·사용권·긴 문구 표시를 확인합니다.'},
        'Assets/HELLSCRIPT/Runtime/Presentation/GameUI.cs',31,status='임시 사용',related=['asset-provenance']))
    for ident,name,summary in [
        ('todo-town','숲속 마을의 미술 품질·실기기 검증','타이틀, 원형 조이스틱, 건물 앞 NPC 6명, 주황·금빛 포탈, 수동 상호작용과 건물 가림 투명화를 구현했습니다. 미술 품질 개선과 모바일 실기기 검증이 남아 있습니다.'),
        ('todo-audio','전투·보상·환경 음향','타격·피격·회피·획득·보스 예고·결과의 정식 음원과 재생 규칙을 연결해야 합니다.'),
        ('todo-animation','정식 캐릭터·적 애니메이션','기존 코드 동작을 기준으로 준비·공격·회피·피격·사망의 정식 애니메이션을 제작해야 합니다.'),
        ('todo-gems','보석 데이터·아이콘','선정한 단일 패치의 종류·등급·수치를 검증한 뒤 HELLSCRIPT의 8부위에 적용해야 합니다.')]:
        rows.append(record(ident,name,'제작 과제',summary,{'현재 공백':summary,'상태 해석':'기획에 필요한 제작 과제이며 완성 자산 수에 포함하지 않습니다.'},
                           DESIGN+'HELLSCRIPT_Gameplay_Completion_Checklist.md',status='제작 대기',related=['resource-guide','remaining-development-backlog']))
    barb='Assets/HELLSCRIPT/Art/Characters/Barbarian/'
    for ident,name,path,model,use in [
        ('char-barbarian-model','야만전사 3D 모델',barb+'Barbarian.prefab','Tripo P1 Multi-View (model3d-tripo-p1-multiview)','정면·후면·좌측면 참고 이미지를 입력해 만든 첫 캐릭터 3D 생성 결과입니다. 시작 씬에 확인용으로 배치했습니다.'),
        ('char-barbarian-front','야만전사 정면 참고 이미지',barb+'Barbarian_Reference_v1.png','Gemini 3.0 Pro (gemini-3.0-pro)','정면 T자세 참고 이미지입니다. 3D 생성의 입력으로 사용했습니다.'),
        ('char-barbarian-back','야만전사 후면 참고 이미지',barb+'Barbarian_Reference_Back.png','Gemini 3.0 Pro (gemini-3.0-pro)','후면 참고 이미지입니다. 3D 생성의 입력으로 사용했습니다.'),
        ('char-barbarian-left','야만전사 좌측면 참고 이미지',barb+'Barbarian_Reference_Left.png','Gemini 3.0 Pro (gemini-3.0-pro)','좌측면 참고 이미지입니다. 3D 생성의 입력으로 사용했습니다.'),
        ('char-barbarian-right','야만전사 우측면 참고 이미지',barb+'Barbarian_Reference_Right.png','Gemini 3.0 Pro (gemini-3.0-pro)','우측면 참고 이미지입니다. 이번 3D 생성의 입력에는 포함되지 않았습니다.')]:
        raw=(ROOT/path).read_bytes();INPUTS[path]=digest(raw);meta=read(path+'.meta')
        rows.append(record(ident,name,'캐릭터 원본',use,
            {'경로':path,'파일 크기 (바이트)':len(raw),'Unity GUID':re.search(r'^guid: (\w+)',meta,re.M)[1],'SHA-256':digest(raw),
             '생성 모델':model,'배경 제거':'적용함' if path.endswith('.png') else '해당 없음','지정 시드':'없음',
             '용도':use,'승인 상태':'개발용 임시 자산입니다. 골격·애니메이션·판정 연결과 출시 아트 승인을 마치지 않았습니다.',
             '전투 연결':'기존 전투의 영웅 표현을 이 모델로 교체하지 않았습니다. 시작 씬의 확인용 배치입니다.'},
            IMPL+'Asset_Provenance.md',status='임시 사용',assetPath=path,related=['asset-provenance','resource-guide']))
    rows.append(record('todo-character-rig','캐릭터 골격·애니메이션 연결','제작 과제','생성한 야만전사 모델에 골격과 준비·공격·회피·피격·사망 애니메이션을 붙이고 실제 판정 시점에 연결해야 합니다.',
        {'현재 표현':'골격 없는 기본 몸체를 조합한 런타임 표현을 그대로 사용합니다. 생성 모델은 시작 씬에 확인용으로만 배치했습니다.',
         '현재 공백':'골격, 애니메이션, 전투 상태 연결과 나머지 두 직업의 모델이 남아 있습니다.',
         '상태 해석':'기획에 필요한 제작 과제이며 완성 자산 수에 포함하지 않습니다.'},IMPL+'Asset_Provenance.md',status='제작 대기',related=['asset-provenance','resource-guide']))
    rows.append(record('todo-fog-art','탐색 안개 경계 미술','제작 과제','실시간 시야와 회색 지형 기억을 구현했습니다. 안개 경계의 미술적 개선과 실제 기기의 성능 측정은 남아 있습니다.',
        {'현재 표현':'실제 바닥과 차폐 판정을 공유하는 0.35m 격자의 360도 시야, 누적 탐색 마스크와 플레이어 중심 윤곽 지도를 사용합니다. 미탐색 지형은 검은색, 시야 밖의 탐색 지형은 회색이며 오브젝트는 현재 시야에서만 표시합니다.','현재 공백':'안개 경계의 미술적 개선과 모바일 실기기의 렌더링 비용 측정이 남아 있습니다.',
         '상태 해석':'기능 구현과 별개인 후속 미술·기기 검증 과제이며 완성 이미지 자산 수에 포함하지 않습니다.'},IMPL+'Rift_Visibility_Expansion.md',status='제작 대기',related=['rift-visibility-expansion','resource-guide']))
    return db('resources','리소스 DB','이미지 원본 4개, 아틀라스 영역, 코드로 만드는 표현과 제작 과제를 함께 관리합니다. 영역·표현·과제는 독립 이미지 파일이 아닙니다.',rows)

def build_evidence():
    rows=[]
    paths=sorted((ROOT/'Artifacts/Validation').glob('*editmode*.xml'))
    for path in paths:
        relative=str(path.relative_to(ROOT));raw=read(relative);root=ET.fromstring(raw)
        tokens=path.stem.split('-');cut=next((i for i,t in enumerate(tokens) if t in ('editmode','final')),len(tokens))
        stage='-'.join(tokens[:cut]) or '초기 빌드';variant='-'.join(t for t in tokens[cut:] if t!='editmode') or '단일 보고서'
        rows.append(record(path.stem,path.name,'Unity Edit Mode',f'{root.get("passed")} / {root.get("total")} 통과 · {root.get("result")} · {stage}',
            {'검사 단계':stage,'보고서 구분':variant,'보고된 결과':root.get('result'),'전체 검사':root.get('total'),'통과':root.get('passed'),'실패':root.get('failed'),
             '건너뜀':root.get('skipped'),'검사 종료 (UTC)':root.get('end-time'),'소요 시간 (초)':root.get('duration'),
             '해석':'저장된 보고서를 읽었습니다. 이번 위키 정리에서 Unity 검사를 새로 실행하지 않았습니다. 같은 단계의 여러 보고서는 수정 전후의 기록이며 서로 합산하지 않습니다.'},relative,
            status='당시 통과' if root.get('result')=='Passed' else '당시 실패',related=['current-status'],endTime=root.get('end-time')))
    rows.sort(key=lambda r:r['endTime'],reverse=True)
    return db('validation','검증 기록','보존된 Edit Mode 보고서 전체입니다. 각 항목에 검사 단계와 종료 시각을 표시하며, 실패를 수정하기 전의 보고서도 그대로 남깁니다. 검사 수를 합산하지 않고 현재 게임 전체의 검증 완료로 해석하지 않습니다.',rows)

PAGE_META={
 'class-set-reference':('장비와 빌드','직업별 세트 24종 기획·레퍼런스','디아블로 4 부적 세트를 참고한 직업별 8종, 개별 장비 105개와 효과 60단계의 게임 미반영 검토안입니다.'),
 'class-set-reference.en':('장비와 빌드','24 class set concepts and references','Design-only Diablo IV-inspired catalog: eight sets per class, 105 pieces and 60 bonus tiers.'),
 'rune-mastery':('장비와 빌드','무기별 룬 성장','여섯 무기 보드, 자유 회수, 전체 배치 프리셋 5칸과 기본·고급·최상위 능력 등급을 정리합니다.'),
 'rune-mastery.en':('장비와 빌드','Weapon rune mastery','Six weapon boards, reusable runes, five global presets and graded mastery abilities.'),
 'rune-mastery-implementation':('후속 개발 기록','무기별 룬 성장 구현 기록','PackBound 이식, 무기별 전투 적용, 저장·합성·프리셋과 macOS 검증 결과입니다.'),
 'rune-v13-implementation':('후속 개발 기록','룬 보드 v13 적용 기록','첨부 v13의 259칸 보드·다섯 색·숙련도·화면을 실제 게임에 적용한 기록과 검증 근거입니다.'),
 'rune-v13-implementation.en':('후속 개발 기록','Rune board v13 implementation','Native integration of the supplied 259-slot boards, five colors, mastery, UI and verification evidence.'),
 'hunt-edict-detail':('기획과 범위','사냥 칙령: 저장·공유','명시적 저장·되돌리기·공유 코드·프리셋 5칸과 스킬 설정 보존의 설계안입니다.'),
 'hunt-edict-global-options':('기획과 범위','사냥 칙령: 전역 옵션','회피·생존·위치·대상·획득·정리·탐색·반복의 8개 분류와 연결 관계입니다.'),
 'hunt-edict-skill-options':('기획과 범위','사냥 칙령: 스킬 화면','스킬별 핵심 옵션과 전역 판단의 경계, 장착·저장·간소화 화면 구조입니다.'),
 'hunt-edict-skill-option-catalog':('기획과 범위','사냥 칙령: 스킬별 핵심 옵션','모든 액티브·기본 공격을 자동 사용 포함 4~5개로 줄인 개별 설계와 패시브 자동 반영 기준입니다.'),
 'hunt-edict-implementation-contract':('기획과 범위','사냥 칙령: 구현 계약','저장·공유와 실행 연결의 구현 순서, 핵심 옵션 v0.2 간소화의 우선 적용과 HED1 기록의 보존 범위입니다.'),
 'project-overview':('프로젝트','프로젝트 개요','게임 방향, 플레이 순환과 기획·DB를 읽는 순서를 정리합니다.'),
 'current-status':('프로젝트','현재 개발 현황','현재 구현, 초기 기록과의 차이, 검사 근거와 남은 과제를 연결합니다.'),
 'unity-implementation-plan':('기획과 범위','상위 구현 기획','확정 정책, 게임 규칙, 화면 흐름과 공통 수식의 기준입니다.'),
 'screen-layout-detail':('기획과 범위','화면 비율·방향 대응','모바일 가로·세로, 방향 설정, PC의 가로 구성 확장과 전환 중 상태 보존을 정리한 개발 전 기획입니다.'),
 'speed-access-detail':('기획과 범위','배속 잠금·시간 사용권','현재 1배속 제공과 고배속 잠금, 향후 월 구독·이벤트 쿠폰, 사용 시간 누적과 차감 기준안입니다.'),
 'development-expansion-plan':('기획과 범위','후속 개발 기획','장비 발견과 행동 수정이 균열 탐험으로 이어지는 후속 개발 범위입니다.'),
 'content-catalog':('기획과 범위','초기 콘텐츠 카탈로그','기존 178개 정의를 보존한 검토용 초안입니다. 현재 수량은 콘텐츠 DB를 확인합니다.'),
 'gameplay-completion-checklist':('기획과 범위','게임 완성 체크리스트','성장·행동·전투·화면·온라인·기기 품질의 완료 기준을 모았습니다.'),
 'remaining-development-backlog':('기획과 범위','남은 개발 작업표','현재 이어서 만들 기능과 검증 기준, 소유자 선택이 필요한 항목입니다.'),
 'itemization-detail':('장비와 빌드','장비·빌드 상세안','접두·접미, 전설·6세트, 파밍·강화·분해·재설정의 상세 규칙입니다.'),
 'gem-socket-detail':('장비와 빌드','보석·소켓 상세안','보석 7종과 6단계, 부위별 소켓 5개, 합성·장착·분리 비용과 드롭 규칙입니다.'),
 'item-quality-detail':('장비와 빌드','장비 품질 상세안','각성 품질, 상위 접사, 걸작 12단계와 기존 성장 곡선에 미치는 실제 크기입니다.'),
 'itemization-open-items':('장비와 빌드','장비 미결 항목 정리','카탈로그가 남긴 장비 미결 6건의 결론과 전설 후보군·확률 계산입니다.'),
 'build-integration-detail':('장비와 빌드','빌드 통합 검사 기준','같은 부위의 장비 충돌을 피하고 6개 빌드의 역할을 비교합니다.'),
 'rift-exploration-detail':('균열과 탐험','균열·탐험 상세안','방 생성, 연결, 시야, 탐색, 상자·성소와 보상의 계약입니다.'),
 'dungeon-composition-detail':('균열과 탐험','던전 구성 상세안','방의 역할과 곁방, 목표 사슬 3종과 보스 관문, 구간별 전투 예산입니다.'),
 'idle-mode-detail':('균열과 탐험','절전 방치 모드','실제 균열 사냥을 유지하는 절전 화면, 잠깐 보기, 연속 복귀와 배터리 검증의 설계안입니다.'),
 'enemy-combat-detail':('전투와 성장','일반 적·정예 상세안','일반 적 12종과 정예 6특성의 행동과 위험 형상을 정의합니다.'),
 'boss-combat-detail':('전투와 성장','보스 전투 상세안','보스 3종의 패턴, 후반 변화와 안전한 회피 경로를 정의합니다.'),
 'passive-combat-detail':('전투와 성장','패시브 전투 상세안','패시브의 시간·HP 경계와 실제 장비 조합을 확인합니다.'),
 'player-training-detail':('전투와 성장','플레이어 훈련 상세안','실제 캐릭터 복사본, 해금·보상 격리와 60초 훈련의 기준입니다.'),
 'growth-detail':('전투와 성장','성장·저레벨 추천 상세안','전투 중 경험치 정산, 레벨업 시 HP 비율 유지, 성장 기록과 저레벨 추천 행동의 기준입니다.'),
 'first-play-detail':('전투와 성장','첫 플레이 안내 상세안','실제 캐릭터로 첫 균열·장비 비교·규칙 수정·훈련·재도전을 안내하는 단계와 기록 경계입니다. 보상을 지급하거나 완료를 위조하지 않습니다.'),
 'training-comparison-detail':('전투와 성장','훈련 A/B 비교 상세안','같은 캐릭터·훈련 환경에서 설정 A와 B의 행동·결과 차이를 보여 주는 기준입니다. 한 표본을 승률로 표시하지 않습니다.'),
 'resource-guide':('리소스와 운영','리소스 제작·관리','현재 이미지, 아틀라스 영역, 임시 표현과 정식 제작 과제를 정리합니다.'),
 'wiki-maintenance':('리소스와 운영','위키·DB 갱신 규칙','원본과 조회본의 관계, 버전 보존, 상태 표시와 갱신 방법입니다.'),
 'asset-provenance':('리소스와 운영','임시 리소스 출처','이미지 생성 경로와 모델 확인 범위, 자체 제작과 글꼴 사용 기록입니다.'),
 'image-prompts':('리소스와 운영','배경·스킬 생성 프롬프트','기존 성소 배경과 스킬 아틀라스의 생성 프롬프트를 보존합니다.'),
 'equipment-atlas-prompt':('리소스와 운영','장비 아이콘 생성 기록','B01–B24의 원본 생성·투명도·6열×4행 아틀라스 기록입니다.'),
 'rift-stone-prompt':('리소스와 운영','균열 바닥 생성 기록','균열 바닥 PNG의 생성 출처와 Unity 가져오기 설정입니다.'),
 'playable-build':('초기 개발 기록','첫 플레이 빌드','최초 빌드의 기능과 당시 미완료 목록을 보존합니다. 후속 개발로 달라진 항목이 있습니다.'),
 'validation-report':('초기 개발 기록','첫 빌드 검증','최초 13개 검사와 macOS 플레이 확인의 당시 기록입니다.'),
}
DEV_SUMMARY={
 'itemization':'접두·접미·전설·6세트와 장비 처리·저장 연결을 기록합니다.',
 'rift':'방 템플릿 기반 자동 생성 지도와 상자·탐색의 첫 연결입니다.',
 'field':'상자·성소·저주 상자의 탐색·상호작용·정산·복원 기록입니다.',
 'action':'스킬 준비·발사·채널·착지, 투사체와 덫의 독립 상태를 기록합니다.',
 'rule':'22조건, 규칙별 목표·이동과 실행·차단 이유를 공용화했습니다.',
 'effect':'피해·보호막·상태 출처·보스 제압과 복원 계약을 통합했습니다.',
 'charge':'세트 충전과 다음 유료 스킬 할인 효과의 예약·소비·만료를 기록합니다.',
 'outcome':'전염의 원 표적, 보스 제어 만료와 같은 틱의 사망 정산을 정리합니다.',
 'area':'눈보라 고정·추적과 중독·서리 노출의 시간 처리를 기록합니다.',
 'enemy':'일반 적 12종·정예 6특성의 개별 행동과 실제 위험 영역을 연결했습니다.',
 'boss':'보스별 패턴·후반 변화·피난 경로와 HUD·저장을 구현했습니다.',
 'passive':'패시브 설명·시간·HP 경계, 실제 조합과 원소 교차 복원을 보완했습니다.',
 'build-integration':'실제 8부위의 6빌드 비교와 벽 모서리 이동·서리 추천 규칙을 보완했습니다.',
 'legendary':'전사 전설 LW01–LW03의 강제 이동·몸체·착지·단일 대상 경계를 검증했습니다.',
 'player-training':'실제 캐릭터 훈련·해금·60초 종료와 프리셋 저장·복원의 후속 작업을 기록합니다.',
 'growth':'경험치를 전투 틱의 정산 경계에 모아 반영하고, 레벨업 시 HP 비율 유지와 저레벨 추천 행동을 연결했습니다.',
 'first-play':'성소·게임 안내·결과에서 여는 아홉 단계 첫 플레이 안내와 계정·영웅별 기록 저장을 구현했습니다.',
 'training-comparison':'같은 조건의 행동 A/B 비교, 변경 목록과 결과 저장을 구현하고 macOS 실행으로 검증했습니다.',
 'current-build-save':'현재 사냥 설정의 적용을 검증→파일 기록→실제 반영 순서로 바꿔 저장 실패 시의 불일치를 막았습니다.',
 'action-continuity':'설정을 저장한 뒤에도 이미 시작한 공격·스킬을 시작 당시 설정으로 마무리하도록 보존했습니다.',
 'hunt-edict':'로컬 프리셋 5칸 연결과 HED1 기초 모델·가져오기 미리보기의 기록입니다. 핵심 옵션 v0.2의 화면·전투 연결은 개발 중입니다.',
 'hunt-edict-v2':'핵심 옵션 89개의 원본 데이터, 공유 형식, 부분 가져오기와 구형 간소화 미리보기를 구현했습니다.',
 'edict-aim':'연쇄·다중 사격의 명중 예측을 실제 실행과 맞추고 W03·A01·A02·M03의 조준 후보를 구현했습니다.',
 'edict-survival':'감지한 공격의 예상 HP 손실과 전역 회피·긴급 생존 조건의 판단 계산을 추가했습니다.',
 'edict-response':'생존 요청을 기존 스킬 실행·보행·물약에 전달하는 경로와 전투별 상태 저장을 확장했습니다.',
 'screen-layout':'프리셋 대화창부터 시작한 화면 배치 대응의 첫 단계 기록입니다. 설정·전투·장비 화면의 후속 기록으로 이어집니다.',
 'screen-settings':'상단 설정·안내 창, 기기별 방향 설정과 창을 여닫을 때의 전투·반복·초안 보존을 구현했습니다.',
 'battle-layout':'화면 크기별 전투 HUD 배치, 카메라 표시 영역과 미탐색 정보 가림을 macOS 창 16개 크기에서 검증했습니다.',
 'inventory-management':'장비 목록·상세·비교의 영역 전환, 필터·정렬, 획득 순번과 일괄 판매·분해 미리보기를 연결했습니다.',
 'speed-access':'현재 1배속만 제공하는 잠금 정책을 구현하고 기존 고배속 저장값을 1배속으로 읽도록 정리했습니다.',
 'defeat-analysis':'사망 직전 구간의 피격원·스킬 불발을 읽는 자료 구조와 결과 화면을 추가했습니다. 틱 기록을 남기는 호출은 아직 전투 루프에 연결되지 않았습니다.',
 'balance-runner':'6개 추천 빌드를 여러 시드에서 자동으로 돌려 클리어율·사망 원인을 모으는 러너와 안정성 검사를 추가했습니다. 실제 대규모 실행 기록은 없습니다.',
 'edict-storage':'사냥 칙령 v0.2 원본을 영웅 저장에 보관하고 구형 저장을 이관하는 경로를 만들었습니다. 전투 연결은 다음 단계입니다.',
 'edict-drive':'저장된 원본을 정기 판단에서 읽어 전역 생존 대응을 실행합니다. 명시적으로 켠 영웅에게만 적용되며 조준·순서·전역 정책은 아직 연결하지 않았습니다.',
 'edict-share':'원본을 공유 코드로 내보내고 받은 코드를 검사·비교한 뒤 적용하는 화면과 저장 경로를 만들었습니다. 항목별 편집 화면은 다음 단계입니다.',
 'edict-editor':'전역 설정·사용 스킬·공격 순서·스킬 옵션을 화면에서 직접 고치는 편집기와 한국어 표시 이름을 만들었습니다. 편집한 값 가운데 전투가 읽는 것은 아직 전역 생존 대응뿐입니다.',
 'edict-policy':'전역 설정의 대상·위치·물약·전리품·가방·탐색·반복을 기존 전투가 읽는 값에 연결했습니다. 꺼진 상태의 동작은 그대로이며 공격 순서와 조준은 다음 단계입니다.',
 'edict-order':'공통 공격 순서·공통 행동 우선순위·스킬별 자동 사용을 규칙 판단 순서에 연결했습니다. 자동 사용을 끈 스킬은 사유와 함께 건너뛰며 조준 연결은 다음 단계입니다.',
 'edict-aim-wire':'네 스킬의 조준 계획을 시전 전에 묻고 조준점과 대상을 시전 명령에 전달합니다. 용도가 충족되지 않으면 규칙을 건너뛰고 사유를 기록합니다. P0 전투 연결의 마지막 단계입니다.',
 'town-plaza':'초기 보행 광장의 개발 이력입니다. 현행 타이틀·원형 조이스틱·NPC 6명·중앙 오른쪽 포탈·우측 고정 상호작용과 건물 투명화는 숲속 정착민 마을 문서를 따릅니다.',
 'dungeon-composition':'던전 구성 1·2단계입니다. 방에 역할을 주고 곁방을 넣었으며, 5단계부터 봉인석 셋을 두고 보스 관문을 달았습니다. 관문은 봉인을 모두 풀거나 처치 게이지가 100에 닿으면 열립니다. 목표가 10단계 난이도에 준 영향을 측정해 함께 적었습니다.',
 'reading-size':'글자 크기를 100%부터 140%까지 고르는 설정을 추가했습니다. 고정 좌표 위의 글자가 잘리지 않도록 화면 구성 전체를 같은 비율로 키우며, 선택은 기기 파일에만 저장합니다.',
 'touch-and-safe-area':'20:9 화면에서 화면 밖으로 밀려나 있던 조작 20곳을 찾아 고치고, 주요 조작을 80단위로 키웠습니다. 배경이 노치까지 덮고 머리말·바닥 색이 여백 띠로 이어집니다. 밀집한 세 화면의 크기는 측정만 해 두었습니다.',
 'runtime-smoke-repair':'한 번에 돌린 런타임 스모크에서 실패 6건의 원인을 가리고 고쳤습니다. 배속 잠금 뒤 남은 시간 예산과 거짓 증거 문구, 낡은 고정값과 버튼 이름, 읽음 표시 비교가 원인이며 게임 코드는 바꾸지 않았습니다.',
 'localization':'설정에서 한국어·영어를 고르게 하고, 게임의 한국어 문장 전체에 대응하는 영어 문장을 넣었습니다. 한국어 원문을 열쇠로 쓰며, 글자를 놓는 한 함수와 조립하는 자리에서 선택한 언어를 읽습니다.',
}

def link_target(target, source, page_paths):
    parsed=urlsplit(target)
    if parsed.scheme: return target if parsed.scheme in ('http','https','mailto') else None
    if target.startswith('#/'):
        return target
    fragment=unquote(parsed.fragment)
    if not parsed.path: dest=source
    else:
        dest=str((ROOT/source).parent.joinpath(unquote(parsed.path)).resolve().relative_to(ROOT))
    if dest in page_paths:
        return '#/page/'+page_paths[dest]+('?section='+quote(fragment) if fragment else '')
    if (ROOT/dest).is_dir(): return None
    if (ROOT/dest).is_file():
        INPUTS[dest]=digest((ROOT/dest).read_bytes())
        return 'sources/'+quote(dest,safe='/')+('#'+fragment if fragment else '')
    raise ValueError(f'Unresolved local Markdown link: {source} -> {target}')

def inline(text, source, page_paths):
    # Tokenize code and links before escaping prose. Raw HTML is always escaped.
    pattern=r'`([^`]+)`|\[([^\]]+)\]\(([^\s)]+)\)'
    result=[];pos=0
    for m in re.finditer(pattern,text):
        result.append(html.escape(text[pos:m.start()]))
        if m[1] is not None: result.append('<code>'+html.escape(m[1])+'</code>')
        else:
            target=link_target(m[3],source,page_paths)
            result.append('<a href="'+html.escape(target,quote=True)+'">'+html.escape(m[2])+'</a>' if target else html.escape(m[2]))
        pos=m.end()
    result.append(html.escape(text[pos:]));value=''.join(result)
    value=re.sub(r'\*\*(.+?)\*\*',r'<strong>\1</strong>',value)
    value=re.sub(r'(?<!\*)\*([^*]+)\*(?!\*)',r'<em>\1</em>',value)
    return value

def render_markdown(body, source, page_paths):
    lines=body.splitlines();result=[];headings=[];used={};i=0
    def fmt(s): return inline(s,source,page_paths)
    while i<len(lines):
        line=lines[i];s=line.strip()
        if not s:i+=1;continue
        if s.startswith('```'):
            code=[];i+=1
            while i<len(lines) and not lines[i].strip().startswith('```'):code.append(lines[i]);i+=1
            result.append('<pre><code>'+html.escape('\n'.join(code))+'</code></pre>');i+=1;continue
        m=re.match(r'^(#{1,6}) (.+)$',s)
        if m:
            level=len(m[1]);title=m[2];base=slug(title);count=used.get(base,0);used[base]=count+1;ident=base+('-'+str(count) if count else '')
            if level>1:result.append(f'<h{level} id="{html.escape(ident)}">{fmt(title)}</h{level}>')
            if level==2:headings.append({'id':ident,'title':title})
            i+=1;continue
        if s.startswith('|') and i+1<len(lines) and re.fullmatch(r'[\s|:-]+',lines[i+1]):
            def cells(x):return [c.strip() for c in x.strip().strip('|').split('|')]
            headers=cells(s);i+=2;rows=[]
            while i<len(lines) and lines[i].lstrip().startswith('|'):
                row=cells(lines[i]);rows.append('<tr>'+''.join('<td>'+fmt(c)+'</td>' for c in row)+'</tr>');i+=1
            result.append('<div class="table-scroll"><table><thead><tr>'+''.join('<th scope="col">'+fmt(c)+'</th>' for c in headers)+'</tr></thead><tbody>'+''.join(rows)+'</tbody></table></div>');continue
        if re.match(r'^(?:[-*]|\d+\.)\s',s):
            ordered=bool(re.match(r'^\d+\.',s));tag='ol' if ordered else 'ul';items=[]
            while i<len(lines) and re.match(r'^(?:[-*]|\d+\.)\s',lines[i].strip()):
                item=re.sub(r'^(?:[-*]|\d+\.)\s+','',lines[i].strip())
                item=re.sub(r'^\[ \]\s*','☐ ',item);item=re.sub(r'^\[[xX]\]\s*','☑ ',item)
                items.append('<li>'+fmt(item)+'</li>');i+=1
            result.append('<'+tag+'>'+''.join(items)+'</'+tag+'>');continue
        if s.startswith('>'):
            result.append('<blockquote>'+fmt(s.lstrip('> '))+'</blockquote>');i+=1;continue
        if re.fullmatch(r'[-*_]{3,}',s):result.append('<hr>');i+=1;continue
        paragraph=[s];i+=1
        while i<len(lines) and lines[i].strip() and not re.match(r'^(?:#{1,6} |\||```|>|[-*] |\d+\. )',lines[i].strip()):
            paragraph.append(lines[i].strip());i+=1
        result.append('<p>'+fmt(' '.join(paragraph))+'</p>')
    return ''.join(result),headings

def build_pages():
    files=sorted((ROOT/'Docs').rglob('*.md'))+sorted((ROOT/'Wiki/content').glob('*.md'))
    metadata=json.loads(read('Wiki/page-metadata.json'))
    paths={str(p.relative_to(ROOT)):page_id(p) for p in files};pages=[]
    if len(set(paths.values()))!=len(paths):raise ValueError('Duplicate page ID')
    for file in files:
        path=str(file.relative_to(ROOT));body=read(path);ident=paths[path]
        title=next(x[2:] for x in body.splitlines() if x.startswith('# '))
        default_summary=DEV_SUMMARY.get(ident.removesuffix('-expansion'),'원본의 정책·구현 범위와 검증 내용을 보존합니다.')
        category,short,summary=PAGE_META.get(ident,('후속 개발 기록',title.removeprefix('HELLSCRIPT '),default_summary))
        public=path.startswith('Wiki/public-content/')
        if public: category,summary='공개 안내',metadata.get(ident,{}).get('summary','게임의 기본 개념을 소개합니다.')
        status='현재 정리' if path.startswith('Wiki/content/') else '당시 기록' if ident in ('playable-build','validation-report') else '기획 초안' if ident=='content-catalog' else '기획 기준' if '/Design/' in path else '개발 기록'
        if ident in ('idle-mode-detail','class-set-reference','class-set-reference.en'):status='기획 검토안'
        if public:status='공개 안내'
        notice='이 문서는 최초 빌드 당시 기록입니다. 최신 상태는 현재 개발 현황과 후속 개발 기록을 확인하세요.' if status=='당시 기록' else '기존 178개 정의를 보존한 초기 카탈로그입니다. 세트 등 현재 수량은 DB와 후속 명세를 따릅니다.' if ident=='content-catalog' else ''
        date_match=re.search(r'(?:정리 기준일|갱신일|작성일)[: ]+(\d{4}-\d{2}-\d{2})',body)
        date=date_match[1] if date_match else '2026-09-09'
        history_path=ROOT/'Wiki/history'/f'{ident}.json';versions=json.loads(history_path.read_text()) if history_path.exists() else []
        hash_value=digest(body.encode())
        if not versions or versions[-1]['hash']!=hash_value:
            versions.append({'revision':len(versions)+1,'date':date,'capturedAt':NOW,'hash':hash_value,'body':body})
            save(history_path,versions)
        rendered=[]
        for version in versions:
            markup,headings=render_markdown(version['body'],path,paths)
            rendered.append(dict(version,html=markup,headings=headings))
        page=dict(id=ident,title=title,shortTitle=short,category=category,summary=summary,status=status,notice=notice,source=path,
                  tags=[category,short],versions=rendered,**{k:v for k,v in rendered[-1].items()})
        topics={'edict':'사냥 칙령','rift':'균열','dungeon':'균열','gate':'균열','objective':'균열','item':'장비','inventory':'장비','build':'빌드','skill':'스킬','passive':'패시브','combat':'전투','screen':'화면','layout':'화면','localization':'현지화','validation':'검증','review':'검증','wiki':'위키'}
        page['tags']=list(dict.fromkeys([category]+[tag for key,tag in topics.items() if key in ident]+metadata.get(ident,{}).get('tags',[])))
        page['createdAt']=versions[0].get('capturedAt',versions[0]['date'])
        page['updatedAt']=versions[-1].get('capturedAt',versions[-1]['date'])
        page['visibility']='public' if public else 'internal'
        pages.append(page)
    categories=['프로젝트','기획과 범위','장비와 빌드','균열과 탐험','전투와 성장','리소스와 운영','후속 개발 기록','초기 개발 기록','공개 안내']
    pages.sort(key=lambda p:(categories.index(p['category']),list(PAGE_META).index(p['id']) if p['id'] in PAGE_META else 999,p['title']))
    return pages

def public_dataset(dataset):
    # A read-only mirror: preserve all pages, history and database records.
    return {**dataset, 'audience':'public', 'readOnly':True}


def build_public(dataset):
    public=public_dataset(dataset)
    target=SITE/'public'
    if target.exists() and not (target/'.wiki-generated').exists():
        raise ValueError('Refusing to replace unmanaged public directory')
    with tempfile.TemporaryDirectory(dir=SITE,prefix='.public-build-') as temporary:
        stage=Path(temporary)/'public';stage.mkdir()
        (stage/'.nojekyll').write_text('')
        shell=(SITE/'index.html').read_text().replace('HELLSCRIPT 개발 위키','HELLSCRIPT 공개 위키').replace('DEVELOPMENT WIKI','PUBLIC WIKI').replace('HELLSCRIPT 기획, 개발 기록과 리소스 데이터베이스','HELLSCRIPT 전체 문서와 데이터베이스 · 읽기 전용')
        shell=shell.replace('src="data.js"','src="data.js?v='+digest(json.dumps(public,ensure_ascii=False).encode())[:16]+'"')
        (stage/'index.html').write_text(shell)
        for name in ['app.js','app.css']:shutil.copyfile(SITE/name,stage/name)
        shutil.copytree(SITE/'icons',stage/'icons')
        shutil.copytree(SITE/'media',stage/'media')
        # Copy only current collected source snapshots, not stale files from prior builds.
        for source in dataset['inputManifest']:
            original=SITE/'sources'/source
            if original.is_file():
                destination=stage/'sources'/source
                destination.parent.mkdir(parents=True,exist_ok=True)
                shutil.copyfile(original,destination)
        save(stage/'catalog.json',{'generatedAt':dataset['generatedAt'],'databases':dataset['databases']})
        save(stage/'data.json',public)
        (stage/'data.js').write_text('window.HELLSCRIPT_WIKI='+json.dumps(public,ensure_ascii=False).replace('</','<\\/')+';\n')
        (stage/'.wiki-generated').write_text('Generated by tools/wiki.py; publish only this directory.\n')
        if target.exists():shutil.rmtree(target)
        stage.rename(target)

def build():
    INPUTS.clear();pages=build_pages();databases=build_databases();resources=build_resources(databases)
    databases.insert(0,resources);databases.append(build_evidence())
    # Include the generator and UI in the evidence manifest, so check also finds stale tooling.
    for path in ['tools/wiki.py','Wiki/site/index.html','Wiki/site/app.js','Wiki/site/app.css']:read(path)
    dataset={'schemaVersion':1,'generatedAt':NOW,'pages':pages,'databases':databases,
             'inputManifest':dict(sorted(INPUTS.items()))}
    old=SITE/'data.json'
    if old.exists():
        previous=json.loads(old.read_text())
        if previous.get('inputManifest')==dataset['inputManifest']:dataset['generatedAt']=previous['generatedAt']
    SITE.mkdir(parents=True,exist_ok=True)
    for path,expected in INPUTS.items():
        actual=(ROOT/path).read_bytes()
        if digest(actual)!=expected:raise ValueError('Source changed during collection; build again: '+path)
        if path.startswith(('Docs/','Wiki/content/','Assets/','Artifacts/','tools/')) or ('/' not in path and path.endswith('.md')):
            target=SITE/'sources'/path;target.parent.mkdir(parents=True,exist_ok=True);target.write_bytes(actual)
    for file in (ROOT/ART).glob('*.png'):
        target=SITE/'media'/file.name;target.parent.mkdir(parents=True,exist_ok=True);shutil.copyfile(file,target)
    (SITE/'media/GlobalHUD').mkdir(parents=True,exist_ok=True)
    shutil.copyfile(ROOT/ART/'GlobalHUD/menu-storage.png',SITE/'media/GlobalHUD/menu-storage.png')
    report=validate(dataset)
    save(SITE/'data.json',dataset)
    (SITE/'data.js').write_text('window.HELLSCRIPT_WIKI='+json.dumps(dataset,ensure_ascii=False).replace('</','<\\/')+';\n')
    save(SITE/'catalog.json',{'generatedAt':dataset['generatedAt'],'databases':databases})
    save(ROOT/'Wiki/validation.json',report)
    build_public(dataset)
    print(json.dumps(report,ensure_ascii=False,indent=2))

def validate(dataset):
    pageids={p['id'] for p in dataset['pages']};dbids={d['id'] for d in dataset['databases']};links=0;images=0
    resourceids={r['id'] for d in dataset['databases'] if d['id']=='resources' for r in d['rows']}
    for page in dataset['pages']:
        for target in re.findall(r'href="([^"]+)"',page['html']):
            target=html.unescape(target);parts=urlsplit(target)
            if target.startswith('#/page/'):
                dest=target.split('/page/')[1].split('?')[0]
                if dest not in pageids:raise ValueError('Missing page '+target)
                if '?section=' in target:
                    anchor=unquote(target.split('?section=',1)[1]);destpage=next(p for p in dataset['pages'] if p['id']==dest)
                    if 'id="'+html.escape(anchor)+'"' not in destpage['html']:raise ValueError('Missing section '+target)
            if target.startswith('sources/') and not (SITE/unquote(parts.path)).is_file():raise ValueError('Missing source copy '+target)
            links+=1
    for database in dataset['databases']:
        if len({r['id'] for r in database['rows']})!=len(database['rows']):raise ValueError('Duplicate IDs')
        for row in database['rows']:
            for ref in [row['source']]+row.get('refs',[]):
                if not (SITE/'sources'/ref['path']).is_file():raise ValueError('Missing record source '+ref['path'])
                if ref['line']<1:raise ValueError('Invalid source line')
            for p in row.get('related',[]):
                if p not in pageids:raise ValueError('Missing related page '+p)
            if row.get('resource') and row['resource'] not in resourceids:raise ValueError('Missing resource relation '+row['resource'])
            content=row.get('content')
            if content:
                if content['db'] not in dbids:raise ValueError('Missing DB')
                if content['id'] not in {r['id'] for d in dataset['databases'] if d['id']==content['db'] for r in d['rows']}:raise ValueError('Missing content relation')
            image=row.get('image')
            if image:
                if not (SITE/'media'/image['file']).is_file():raise ValueError('Missing image')
                if digest((SITE/'media'/image['file']).read_bytes())!=dataset['inputManifest'][ART+image['file']]:raise ValueError('Image bytes changed in wiki copy')
                if 'cell' in image and not 0<=image['cell']<24:raise ValueError('Invalid atlas cell')
                images+=1
    stale=[p for p,h in dataset['inputManifest'].items() if not (ROOT/p).is_file() or digest((ROOT/p).read_bytes())!=h]
    if stale:raise ValueError('Stale wiki source: '+', '.join(stale))
    for p,h in dataset['inputManifest'].items():
        copy=SITE/'sources'/p
        if copy.is_file() and digest(copy.read_bytes())!=h:raise ValueError('Source snapshot mismatch '+p)
    return {'result':'passed','generatedAt':dataset['generatedAt'],'pages':len(dataset['pages']),
            'databases':len(dataset['databases']),'records':sum(len(d['rows']) for d in dataset['databases']),
            'recordsByDatabase':{d['id']:len(d['rows']) for d in dataset['databases']},'documentLinks':links,
            'imageReferences':images,'uniqueImageFiles':len(list((ROOT/ART).glob('*.png'))),
            'sourceFiles':len(dataset['inputManifest']),'gameExecution':'not performed; preserved reports only'}

def check():
    dataset=json.loads((SITE/'data.json').read_text())
    result=validate(dataset)
    if json.loads((SITE/'public/data.json').read_text())!=public_dataset(dataset):raise ValueError('Stale public wiki')
    for name in ['app.js','app.css']:
        if (SITE/name).read_bytes()!=(SITE/'public'/name).read_bytes():raise ValueError('Stale public UI: '+name)
    for source in dataset['inputManifest']:
        original=SITE/'sources'/source
        if original.is_file() and (not (SITE/'public/sources'/source).is_file() or original.read_bytes()!=(SITE/'public/sources'/source).read_bytes()):raise ValueError('Stale public source: '+source)
    for original in (SITE/'media').glob('*'):
        if original.is_file() and (not (SITE/'public/media'/original.name).is_file() or original.read_bytes()!=(SITE/'public/media'/original.name).read_bytes()):raise ValueError('Stale public media: '+original.name)
    result['publicPages']=len(public_dataset(dataset)['pages'])
    print(json.dumps(result,ensure_ascii=False,indent=2))

def main():
    parser=argparse.ArgumentParser();parser.add_argument('command',choices=['build','check','serve']);parser.add_argument('--port',type=int,default=4175);args=parser.parse_args()
    if args.command=='serve':
        from functools import partial
        handler=partial(http.server.SimpleHTTPRequestHandler,directory=str(SITE))
        server=http.server.ThreadingHTTPServer(('127.0.0.1',args.port),handler)
        print(f'HELLSCRIPT Wiki: http://127.0.0.1:{args.port}/#/tree',flush=True);server.serve_forever()
    elif args.command=='build': build()
    else: check()

if __name__=='__main__': main()
