#!/usr/bin/env python3
"""Collect the current 144 skills using the wiki input API, without loading artwork."""
import copy,hashlib,json
TREE='Assets/HELLSCRIPT/Resources/Data/SkillTreeD4.json'
CLASS='Assets/HELLSCRIPT/Resources/Data/ClassSkills.json'
CORE='Assets/HELLSCRIPT/Runtime/Core/'
PRES='Assets/HELLSCRIPT/Runtime/Presentation/'
SOURCES=[TREE,CLASS,CORE+'ClassSkills.cs',CORE+'SkillTreeD4/SkillTreeD4.Data.cs',CORE+'SkillTreeD4/SkillTreeD4.Definitions.cs',CORE+'SkillTreeD4/SkillTreeD4.Rules.cs',CORE+'SkillTreeD4/SkillTreeD4.Loadout.cs',CORE+'SkillTreeD4/CombatSimulation.D4Framework.cs',CORE+'SkillTreeD4/CombatSimulation.D4Warrior.cs',CORE+'SkillTreeD4/CombatSimulation.D4Ranger.cs',CORE+'SkillTreeD4/CombatSimulation.D4Mage.cs',CORE+'SkillTreeD4/CombatSimulation.D4Equipment.cs',PRES+'WorldView.SkillFx.cs',PRES+'WorldView.Actions.cs',PRES+'WorldView.AttackFx.cs']
NAMES=['전사','궁수','마법사']

def collect(api,databases):
    pins={};texts={}
    for path in SOURCES:
        text=api.read(path);texts[path]=text
        pins[path]=api.INPUTS[path]  # The API pins original bytes, including any BOM.
    tree=json.loads(texts[TREE]);old=json.loads(texts[CLASS]);defs={s['id']:s for s in old['skills']}
    # This runs after attach_current_skill_images; all 144 icons are attested already.
    db=next(d for d in databases if d['id']=='class-abilities')
    if {r['id'] for r in db['rows']}!=set(defs):raise ValueError('Current class-abilities must contain exactly the 144 skill IDs')
    current={r['id']:r for r in db['rows']};images={key:r.get('image') for key,r in current.items()}
    def proof(path,needle):
        lines=texts[path].splitlines();hits=[i+1 for i,t in enumerate(lines) if needle in t]
        if not hits:raise ValueError('Missing skill source definition: '+needle)
        return {'path':path,'line':hits[0],'sha256':pins[path],'match':needle}
    def evidence(skill):
        out=[]
        for path in texts:
            if not path.endswith('.cs'):continue
            ls=texts[path].splitlines();lines=[i+1 for i,t in enumerate(ls) if '"'+skill+'"' in t]
            if lines:out.append({'path':path,'lines':lines,'sha256':pins[path]})
        return out
    records=[];edges=[];p_edges=[]
    for c in tree['classes']:
        hero=c['heroClass'];tiers={t['tier']:t for t in c['tiers']}
        for a in c['actives']:
            d=defs[a['id']];tier=tiers[a['tier']]
            common={'heroClass':hero,'classKo':NAMES[hero],'classEn':c['hero'],'tier':a['tier'],'tierKo':tier['ko'],'tierEn':tier['en'],'gatePoints':tier['gate'],'icon':images.get(a['id']),'source':proof(TREE,'"id": "'+a['id']+'"'),'metadataSource':proof(CLASS,'"id": "'+a['id']+'"'),'runtimeEvidence':evidence(a['id']),'observedRuntime':'STATIC_SOURCE_ONLY_NOT_NEW_PLAYTEST'}
            base_rec={**common,'id':a['id'],'nameKo':a['ko'],'nameEn':a['en'],'category':'ultimate' if a['ultimate'] else 'active','descriptionKo':a['descKo'],'descriptionEn':a['descEn'],'roleKo':a['roleKo'],'maxRank':a['maxRank'],'stats':a['stats'],'element':d.get('element',''),'automaticKo':d.get('automatic',''),'automaticEn':d.get('automaticEn',''),'modifierIds':[m['id'] for m in a['modifiers']],'parentId':None,'linkedIds':[]}
            records.append(base_rec)
            for index,m in enumerate(a['modifiers'],1):
                if m['id']!=a['id']+'-'+str(index):raise ValueError('Unexpected modifier ID/order: '+m['id'])
                md=defs[m['id']]
                records.append({**common,'id':m['id'],'nameKo':m['ko'],'nameEn':m['en'],'category':'modifier','descriptionKo':m['shapeKo'],'descriptionEn':m['shapeEn'],'effectKo':m['effectKo'],'effectEn':m['effectEn'],'shapeKo':m['shapeKo'],'shapeEn':m['shapeEn'],'value':m['value'],'modifierIndex':index,'maxRank':a['maxRank'],'parentId':a['id'],'modifierIds':[],'linkedIds':[],'icon':images.get(m['id']),'source':proof(TREE,'"id": "'+m['id']+'"'),'metadataSource':proof(CLASS,'"id": "'+m['id']+'"'),'automaticKo':md.get('automatic',''),'automaticEn':md.get('automaticEn',''),'effectDefinitionStatus':'CURRENT_GAME_DATA','visualVerification':'FORM_AND_BEHAVIOR_DESCRIPTION_DEFINED; NO_PER_MODIFIER_RENDER_ACCEPTANCE_CLAIM','visualNoteKo':'동작·형태 변화는 현재 게임 정의입니다. 개별 보조 전용 VFX가 모두 별도로 구현·화면 검수됐다는 뜻은 아닙니다.','visualNoteEn':'Form and behavior follow current game data. This does not claim a dedicated VFX or a new visual test for every modifier.'})
                edges.append({'base':a['id'],'modifier':m['id'],'index':index,'heroClass':hero})
        for p in c['passives']:
            records.append({'heroClass':hero,'classKo':NAMES[hero],'classEn':c['hero'],'id':p['id'],'nameKo':p['ko'],'nameEn':p['en'],'category':'passive','descriptionKo':p['effectKo'],'descriptionEn':p['effectEn'],'effectKo':p['effectKo'],'effectEn':p['effectEn'],'keyPassive':p['key'],'unlockLevel':p['unlockLevel'],'maxRank':defs[p['id']]['maxRank'],'value':p['value'],'parentId':None,'modifierIds':[],'linkedIds':p['linkedActives'],'icon':images.get(p['id']),'source':proof(TREE,'"id": "'+p['id']+'"'),'metadataSource':proof(CLASS,'"id": "'+p['id']+'"'),'runtimeEvidence':evidence(p['id']),'observedRuntime':'STATIC_SOURCE_ONLY_NOT_NEW_PLAYTEST'})
            p_edges.extend({'passive':p['id'],'active':a,'heroClass':hero} for a in p['linkedActives'])
    ids={r['id'] for r in records}
    if len(records)!=144 or len(ids)!=144 or ids!=set(defs) or len(edges)!=84 or len(p_edges)!=6:raise ValueError('Current skill catalog count/relations changed; review schema and UI counts')
    for r in records:
        if not all(i in ids for i in r['linkedIds']+r['modifierIds']+([r['parentId']] if r['parentId'] else [])):raise ValueError('Broken skill relation: '+r['id'])
        if not r['icon'] or r['icon'].get('cell') is not None:raise ValueError('Missing current single icon: '+r['id'])
        r['unlockLevel']=current[r['id']]['fields']['해금 레벨']
        if r['category'] in ('modifier','passive'):
            r['rankGrowth']={'fromRank':1,'toRank':r['maxRank'],'from':r['value']['from'],'to':r['value']['to'],'unit':r['value']['unit'],'ranks':r['value']['ranks']}
        else:r['rankGrowth']=None
    return {'schemaVersion':1,'scope':'CURRENT_SOURCE_SKILL_DATABASE','counts':{'all':144,'active':24,'ultimate':6,'modifier':84,'passive':30,'baseToModifier':84,'passiveToActive':6},'classes':[{'id':c['heroClass'],'ko':NAMES[c['heroClass']],'en':c['hero']} for c in tree['classes']],'rules':tree['rules'],'rows':records,'modifierRelations':edges,'passiveRelations':p_edges,'sourcePins':pins,'catalogSource':{'database':'class-abilities','rows':len(db['rows']),'images':'already attested current individual icons'},'runtimeGate':{'enabledByDefault':'static bool requested=true;' in texts[CORE+'SkillTreeD4/SkillTreeD4.Data.cs'],'requiresLoadoutVersion':5,'historicalPlayerEnabledField':old['playerEnabled'],'note':'ClassSkills.playerEnabled alone is not the D4 runtime gate; D4Combat uses SkillTreeD4.Enabled and loadout.version==5.'},'omittedLegacyIds':'No legacyRemap or retired atlas IDs are turned into playable records. Historical 18-frame catalog is not the current skill list.','verification':'Data/source relationship only. No Unity or combat execution in this task.'}


FILES=('skill-details.json','skill-details.js')
UI_FILES=('skill-db.js','skill-db.css')

def bundle_metadata(bundle):
    return {'schemaVersion':1,'json':FILES[0],'script':FILES[1],
            'counts':bundle['counts'],'sourcePins':bundle['sourcePins'],
            'historicalFrameCatalogSha256':hashlib.sha256(json.dumps(bundle['historicalFrameCatalog'],ensure_ascii=False,sort_keys=True,separators=(',',':')).encode()).hexdigest()}

def replace_current_database(databases,bundle):
    """Replace only the current skills view; preserve the class-abilities database."""
    database=next(d for d in databases if d['id']=='skills')
    current={r['id']:r for d in databases if d['id']=='class-abilities' for r in d['rows']}
    rows=[]
    categories={'active':'액티브','ultimate':'궁극기','modifier':'보조 스킬','passive':'패시브'}
    for detail in bundle['rows']:
        row=copy.deepcopy(current[detail['id']])
        row.update(name=detail['nameKo'],summary=detail['descriptionKo'],
                   category=detail['classKo']+' · '+categories[detail['category']],source=detail['source'])
        row['refs']=[ref for ref in row.get('refs',[]) if ref['path']!=TREE]+[{'path':CLASS,'line':1}]
        row['fields'].update({'해금 레벨':detail['unlockLevel'],'기본 스킬':detail['parentId'] or '',
            '연결 보조':detail['modifierIds'],'연계 스킬':detail['linkedIds'],
            '보조 효과':detail.get('effectKo',''),'동작·형태 변화':detail.get('shapeKo',''),
            '등급 성장 수치':detail.get('rankGrowth'),'기본 등급 수치':detail.get('stats',[]),
            '수치 화살표 의미':'보조/패시브 1등급 → 최대등급 성장; 기본 → 보조 효과 차이가 아님'})
        # A current ClassSkills base parameter, retained for existing range-reference links.
        params=row['fields'].get('수치',{})
        if detail['category'] in ('active','ultimate') and 'rangeMeters' in params:
            row['fields']['거리 필드 range (m)']=params['rangeMeters']
        rows.append(row)
    database.update(name='스킬 · 보조 스킬 144종',
        description='현행 3직업 액티브·궁극기30, 파생 보조84, 패시브30의 수치와 연결 관계입니다.',rows=rows)

def serialized(bundle):
    return {FILES[0]:json.dumps(bundle,ensure_ascii=False,indent=2)+'\n',
            FILES[1]:'window.HELLSCRIPT_SKILL_DETAILS='+json.dumps(bundle,ensure_ascii=False,separators=(',',':')).replace('</','<\\/')+';\n'}

def write_bundle(site,bundle):
    for name,text in serialized(bundle).items():
        target=site/name
        if not target.exists() or target.read_text()!=text:
            temporary=target.with_suffix(target.suffix+'.tmp');temporary.write_text(text);temporary.replace(target)

def prepare(api,databases):
    previous=copy.deepcopy(next(d for d in databases if d['id']=='skills')['rows'])
    if len(previous)!=18:raise ValueError('Expected the preserved 18-frame catalog before current skills collection')
    bundle=collect(api,databases)
    bundle['historicalFrameCatalog']={'status':'HISTORICAL_FRAME_REFERENCE_ONLY_NOT_CURRENT_SKILL_LIST','rows':previous}
    replace_current_database(databases,bundle)
    return bundle

def check(api,dataset):
    metadata=dataset.get('skillDetails')
    if not isinstance(metadata,dict):raise ValueError('Current skill bundle metadata missing; rebuild wiki')
    bundle=json.loads((api.SITE/FILES[0]).read_text())
    expected=collect(api,dataset['databases'])
    # Preserve historical reference material without silently treating it as current rows.
    historical=bundle.get('historicalFrameCatalog',{})
    if historical.get('status')!='HISTORICAL_FRAME_REFERENCE_ONLY_NOT_CURRENT_SKILL_LIST' or len(historical.get('rows',[]))!=18:
        raise ValueError('Historical 18-frame reference missing')
    expected['historicalFrameCatalog']=historical
    if bundle!=expected or metadata!=bundle_metadata(expected):raise ValueError('Stale current skill bundle')
    if any(dataset['inputManifest'].get(path)!=sha for path,sha in expected['sourcePins'].items()):
        raise ValueError('Current skill source pin missing/stale')
    original=next(d for d in dataset['databases'] if d['id']=='skills')
    candidate=copy.deepcopy(dataset['databases']);replace_current_database(candidate,expected)
    if original!=next(d for d in candidate if d['id']=='skills'):raise ValueError('Stale canonical skills database')
    for name,text in serialized(expected).items():
        if not (api.SITE/name).is_file() or (api.SITE/name).read_text()!=text:raise ValueError('Stale skill sidecar: '+name)
    return {'rows':144,'baseToModifier':84,'passiveToActive':6,'sources':len(expected['sourcePins'])}

def public_files(dataset):
    if not dataset.get('skillDetails'):
        if any(d['id']=='skills' for d in dataset['databases']):raise ValueError('Skills database requires a generated current bundle')
        return ()  # Small non-skill exporter unit fixtures.
    return UI_FILES+FILES
