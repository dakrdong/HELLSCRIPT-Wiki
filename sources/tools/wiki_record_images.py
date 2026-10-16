"""Attach existing, collected current art to content records without inventing previews.

Call after resources have been collected. images is an ordered exact-item bundle;
image remains a separate first-image copy for old consumers. No pixel generation.
"""
import json
import re

ART = 'Assets/HELLSCRIPT/Resources/Art/'
REGISTRY = 'Assets/HELLSCRIPT/Resources/Monsters/MonsterRegistry.json'
PORTRAITS = 'Docs/Art/Monsters/Casual20261014/validation/portrait-assets.json'
CURRENT_PORTRAITS = 'Docs/Art/Monsters/portrait-manifest.json'
APPROVAL = 'tools/casual_art_adoption_receipt_20261015.json'
POTIONS = 'Assets/HELLSCRIPT/Resources/Data/Potions.json'
BOXES = 'Assets/HELLSCRIPT/Resources/Data/RewardBoxes.json'
ATTENDANCE = 'Assets/HELLSCRIPT/Runtime/Core/Attendance.cs'
PRESENTATION = 'Assets/HELLSCRIPT/Runtime/Presentation/'
EXPECTED = {'enemies':20, 'bosses':5, 'field':5, 'rune-shapes':34,
            'potions':44, 'attendance':35, 'reward-boxes':148, 'rift-first-boxes':1000}


def _unique(values, key, name):
    result = {v[key]: v for v in values}
    if len(result) != len(values):
        raise ValueError('Duplicate ' + name)
    return result


def _same_ids(rows, ids, name):
    if set(rows) != set(ids):
        raise ValueError('Changed ' + name + ' identities')


def attach(api, databases):
    """Return category coverage; mutate only preview metadata/refs, preserving model links."""
    tables = _unique(databases, 'id', 'database')
    rows = {}
    for name, count in EXPECTED.items():
        if name not in tables or len(tables[name]['rows']) != count:
            raise ValueError('Changed preview table count: ' + name)
        rows[name] = _unique(tables[name]['rows'], 'id', name)
    if 'resources' not in tables:
        raise ValueError('Collect resources before attaching record images')
    resource_paths = {}
    for resource in tables['resources']['rows']:
        if resource.get('assetPath') and resource.get('image'):
            resource_paths.setdefault(resource['assetPath'], resource)
    approval = json.loads(api.read(APPROVAL))
    if (approval.get('schemaVersion') != 1 or approval.get('adoption') != 'approved_for_import'
            or len(approval.get('records', [])) != 603):
        raise ValueError('Changed current art approval unit')
    approved = _unique(approval['records'], 'path', 'approved art path')
    registry = _unique(json.loads(api.read(REGISTRY))['monsters'], 'id', 'monster')
    portrait_proof = json.loads(api.read(PORTRAITS))
    if portrait_proof.get('passed') is not True or portrait_proof.get('count') != 35:
        raise ValueError('Missing current portrait proof')
    historical_portraits = _unique(portrait_proof['items'], 'id', 'portrait proof')
    if (len(historical_portraits) != 35 or any(not re.fullmatch('[0-9a-f]{64}', p.get('sha256', ''))
            for p in historical_portraits.values())):
        raise ValueError('Changed portrait proof count or historical hash')
    # The preserved proof predates the lavender boss backdrops and later StyleD
    # model-matched replacements. Current runtime bytes have their own manifest.
    current_portraits = json.loads(api.read(CURRENT_PORTRAITS))
    portraits = current_portraits.get('items')
    if (current_portraits.get('generationSurface') != 'built-in image_gen'
            or not isinstance(portraits, dict) or len(portraits) != 35):
        raise ValueError('Missing current runtime portrait authority')
    _same_ids(portraits, historical_portraits, 'current/historical portrait')
    _same_ids(portraits, registry, 'portrait/registry')
    for identifier, entry in portraits.items():
        if (entry.get('file') != ART+'TrainingGround/Portraits/'+identifier+'.png'
                or not re.fullmatch('[0-9a-f]{64}', entry.get('sha256', ''))):
            raise ValueError('Invalid current runtime portrait authority: ' + identifier)
    # Dependencies own mapping rules; the unchanged runtime remains authoritative.
    dependencies = [PRESENTATION + x for x in
                    ('PotionArt.cs', 'AttendanceArt.cs', 'AttendanceWindow.cs',
                     'WorldView.Rift.cs', 'RuneItemArt.cs')]
    dependencies += ['Assets/HELLSCRIPT/Runtime/Core/MonsterRegistry.cs']
    for path in dependencies:
        api.read(path)
    plans = []
    missing = []

    def image(path, authority, label=None, count=None):
        if (not isinstance(path, str) or not path.startswith(ART) or not path.endswith('.png')
                or '\\' in path or any(p in ('', '.', '..') for p in path.split('/'))):
            raise ValueError('Unsafe current preview path: ' + str(path))
        resource = resource_paths.get(path)
        sha = api.INPUTS.get(path)
        if (resource is None or not re.fullmatch('[0-9a-f]{64}', sha or '')
                or resource['image'].get('file') != path[len(ART):]
                or not (api.ROOT / path).is_file()):
            raise ValueError('Missing collected current preview: ' + path)
        if authority.get('sha256') != sha:
            raise ValueError('Current preview differs from approval: ' + path)
        item = {'file':path[len(ART):], 'assetPath':path}
        if label is not None:
            item['label'] = label
        if count is not None:
            if type(count) is not int or count <= 0:
                raise ValueError('Invalid preview grant count')
            item['count'] = count
        return item

    def current(path, label=None, count=None):
        if path not in approved:
            raise ValueError('Preview has no current approval: ' + path)
        return image(path, approved[path], label, count)

    def plan(table, identifier, items, refs, bundle=False):
        row = rows[table][identifier]
        expected = {'file':items[0]['file']}
        if row.get('image') is not None and row['image'] != expected:
            raise ValueError('Existing record preview differs: ' + identifier)
        if row.get('assetPath') not in (None, items[0]['assetPath']):
            raise ValueError('Existing record source differs: ' + identifier)
        if row.get('images') is not None and row['images'] != items:
            raise ValueError('Existing record bundle differs: ' + identifier)
        plans.append((table, row, items, refs, bundle))

    for table, prefix, count in [('enemies', 'N', 20), ('bosses', 'BOSS', 5)]:
        _same_ids(rows[table], [prefix + f'{n:02}' for n in range(1, count+1)], table)
        for identifier in rows[table]:
            monster_id = identifier if table == 'enemies' else 'B' + identifier[4:]
            definition = registry[monster_id]
            path = 'Assets/HELLSCRIPT/Resources/' + definition['portrait'] + '.png'
            if definition['portrait'] != 'Art/TrainingGround/Portraits/' + monster_id:
                raise ValueError('Review changed monster portrait mapping: ' + monster_id)
            plan(table, identifier, [image(path, portraits[monster_id])],
                 [REGISTRY, PORTRAITS, CURRENT_PORTRAITS, 'Assets/HELLSCRIPT/Runtime/Core/MonsterRegistry.cs'])

    potions = json.loads(api.read(POTIONS))
    base = _unique(potions['items'], 'id', 'base potion')
    elixirs = {'PE-G%02d-%d' % (g,t) for g in range(1,7) for t in range(1,7)}
    if potions.get('version') != 1 or len(base) != 8:
        raise ValueError('Changed base potion catalog')
    _same_ids(rows['potions'], set(base) | elixirs, 'potion')
    for identifier, row in rows['potions'].items():
        if identifier in base:
            effect = base[identifier]['effect']
            icon = 'potion-hp' if effect == 'hp' else 'potion-mp' if effect == 'resource' else 'potion-utility'
            path = ART + 'GlobalHUD/Casual/' + icon + '.png'
        else:
            match = re.fullmatch('PE-(G0[1-6])-([1-6])', identifier)
            if row['fields']['등급 / Grade'] != int(match[2]):
                raise ValueError('Elixir grade differs from identity: ' + identifier)
            path = ART + 'JewelerWindow/elixir-' + match[1] + '-' + match[2] + '.png'
        plan('potions', identifier, [current(path)], [POTIONS, APPROVAL, PRESENTATION+'PotionArt.cs'])

    attendance_source = api.read(ATTENDANCE)
    weekly = re.findall(r'new AttendanceReward\(AttendanceRewardKind\.(\w+),(\d+)(?:,(\d+))?\)', attendance_source)
    multipliers = re.search(r'monthlyMultipliers=\{([0-9,]+)\}', attendance_source)
    chests = re.search(r'monthlyChests=\{([0-9,]+)\}', attendance_source)
    if len(weekly) != 7 or not multipliers or not chests:
        raise ValueError('Changed attendance presentation catalog')
    multipliers = list(map(int, multipliers[1].split(',')))
    chests = list(map(int, chests[1].split(',')))
    if len(multipliers) != 4 or len(chests) != 4:
        raise ValueError('Changed monthly attendance catalog')
    _same_ids(rows['attendance'], ['attendance-%s-%02d' % (track,day)
              for track,length in [('weekly',7),('monthly',28)] for day in range(1,length+1)], 'attendance')
    singles = {'Coins':'GlobalHUD/currency-abyssal-coin.png', 'Gold':'Attendance/reward-gold.png',
               'Cores':'Attendance/reward-random-core.png', 'LegendaryChest':'Attendance/reward-legendary-chest.png'}
    for identifier, row in rows['attendance'].items():
        _, track, day = identifier.split('-'); day = int(day)
        kind, amount, tier = weekly[(day-1)%7]; amount = int(amount); tier = int(tier or 0)
        if track == 'monthly':
            amount = chests[(day-1)//7] if kind == 'LegendaryChest' else amount*multipliers[(day-1)//7]
        if (row['fields']['수량 / Amount'] != amount
                or row['fields']['단계 / Tier'] != (tier if tier else '-')):
            raise ValueError('Attendance reward differs from runtime: ' + identifier)
        if kind == 'Gems':
            # Same three-colour composition as AttendanceWindow.Icon, not three distinct grants.
            items = [current(ART+'JewelerWindow/gem-'+gem+'-'+str(tier)+'.png',
                     '무작위 보석 표시 / Random gem presentation: '+gem+' T'+str(tier)
                     +' · 전체 수량 / total ×'+str(amount)) for gem in ['G03','G02','G01']]
        elif kind in singles:
            items = [current(ART+singles[kind])]
        else:
            raise ValueError('Unmapped attendance reward: ' + kind)
        plan('attendance', identifier, items, [ATTENDANCE, APPROVAL,
             PRESENTATION+'AttendanceArt.cs', PRESENTATION+'AttendanceWindow.cs'], kind == 'Gems')

    reward_data = json.loads(api.read(BOXES))
    boxes = _unique(reward_data['boxes'], 'id', 'reward box')
    _same_ids(rows['reward-boxes'], boxes, 'reward box')
    for identifier, definition in boxes.items():
        plan('reward-boxes', identifier, [current(ART+'RewardBoxes/'+definition['icon']+'.png')],
             [BOXES, APPROVAL])
    _same_ids(rows['rift-first-boxes'], ['rift-first-'+str(n) for n in range(1,1001)], 'first-clear')
    for stage in range(1,1001):
        identifier = 'rift-first-'+str(stage)
        grants = [r['grant'] for r in reward_data['firstClearRules'] if stage%r['every']==0]
        grants += next((m['grants'] for m in reward_data['milestones'] if m['stage']==stage), [])
        if not grants or rows['rift-first-boxes'][identifier]['fields']['지급 데이터'] != grants:
            raise ValueError('First-clear grants differ from runtime: ' + identifier)
        items = [current(ART+'RewardBoxes/'+boxes[g['boxId']]['icon']+'.png',
                 boxes[g['boxId']]['nameKo']+' / '+boxes[g['boxId']]['nameEn']+' ×'+str(g['count']),
                 g['count']) for g in grants]
        plan('rift-first-boxes', identifier, items, [BOXES, APPROVAL], True)

    shapes_path = 'Docs/Implementation/Rune_Mastery_Catalog.json'
    shapes = json.loads(api.read(shapes_path))['shapes']
    _same_ids(rows['rune-shapes'], [s['id'] for s in shapes], 'rune shape')
    _same_ids(rows['field'], ['CH01','CH02','CH03','SH01','SH02'], 'field')
    import wiki_rune_shape_previews
    import wiki_field_2d
    derived = [('rune-shapes', item) for item in wiki_rune_shape_previews.prepare(api, shapes, rows['rune-shapes'])]
    derived += [('field', item) for item in wiki_field_2d.prepare(api, rows['field'])]
    resource_rows = tables['resources']['rows']
    resource_ids = _unique(resource_rows, 'id', 'resource')
    derived_resources = []
    for table, item in derived:
        row = rows[table][item['id']]
        if 'source' not in item:
            item['source']=api.source_ref(wiki_rune_shape_previews.MANIFEST)
        if 'name' not in item:
            shape=next(s for s in shapes if s['id']==item['id'])
            item['name']=row.get('name',str(shape['size'])+'칸 · 모양 '+str(shape['number']))
        if (row.get('image') not in (None,item['image']) or row.get('images')
                or row.get('assetPath') or row.get('previewSource') not in (None,item['previewSource'])):
            raise ValueError('Existing derived preview differs: '+item['id'])
        for key,value in item['fields'].items():
            if key in row['fields'] and row['fields'][key] != value:
                raise ValueError('Existing derived preview facts differ: '+item['id'])
        resource_id = ('preview-rune-' if table == 'rune-shapes' else 'field-art-') + item['id']
        resource = dict(id=resource_id, name=item.get('name',item['id']),
                        category='룬 형태 미리보기' if table=='rune-shapes' else '필드 2D 원화',
                        summary=item.get('summary','정확한 형태의 2D 미리보기 / Shape-specific 2D preview'),
                        fields=dict(item['fields']), source=item['source'], refs=list(item['refs']),
                        status='2D 미리보기 제작 완료 / 2D preview complete' if table=='rune-shapes' else
                               '2D 원화 제작 완료 · 게임 연결 검증 별도 / 2D artwork complete; runtime integration verification separate',
                        content={'db':table,'id':item['id']}, image=dict(item['image']),previewSource=item['previewSource'])
        if resource_id in resource_ids and resource_ids[resource_id] != resource:
            raise ValueError('Existing preview resource differs: '+resource_id)
        derived_resources.append((resource_id,resource))

    # All identities/sources pass before any record is changed.
    receipt = {name:{'rows':len(rows[name]), 'imageRows':0, 'bundleRows':0,
                     'multipleImageRows':0, 'missing':[]} for name in EXPECTED}
    for table, row, items, refs, bundle in plans:
        row['image'] = {'file':items[0]['file']}
        row['assetPath'] = items[0]['assetPath']
        if bundle:
            row['images'] = [dict(item) for item in items]
        # Keep model-ready-* and other existing semantic/model links intact.
        row.setdefault('resource', resource_paths[items[0]['assetPath']]['id'])
        for path in refs + [item['assetPath'] for item in items]:
            reference = api.source_ref(path)
            if reference not in row.setdefault('refs', []):
                row['refs'].append(reference)
        receipt[table]['imageRows'] += 1
        receipt[table]['bundleRows'] += int(bundle)
        receipt[table]['multipleImageRows'] += int(len(items)>1)
    for (table,item),(resource_id,resource) in zip(derived,derived_resources):
        row=rows[table][item['id']]
        row['image']=dict(item['image']);row['previewSource']=item['previewSource']
        row['fields'].update(item['fields']);row.pop('imageMissing',None)
        row.setdefault('resource',resource_id)
        for reference in item['refs']:
            if reference not in row.setdefault('refs',[]):row['refs'].append(reference)
        if resource_id not in resource_ids:resource_rows.append(resource)
        receipt[table]['imageRows']+=1
    for table, row, reason in missing:
        row['imageMissing'] = reason
        receipt[table]['missing'].append(row['id'])
    return {'schemaVersion':1, 'state':'CURRENT_RECORD_IMAGES_ATTACHED_WITH_DERIVED_2D_PREVIEWS',
            'categories':receipt, 'missingExactPreviews':len(missing)}
