"""Bind five new 2D field illustrations without changing gameplay or model assets."""
import hashlib
import json
import struct

PREFIX = 'Wiki/assets/field-items-20261016/'
MANIFEST = PREFIX + 'manifest.json'
IDS = {'CH01','CH02','CH03','SH01','SH02'}
DEPENDENCIES = {'Docs/Design/HELLSCRIPT_Rift_Exploration_Detail.md','Assets/HELLSCRIPT/Runtime/Core/RiftFieldContent.cs'}


def _binary(api, path, sha, size=None):
    if (not isinstance(path,str) or not path.startswith(PREFIX) or not path.endswith('.png')
            or any(p in ('', '.', '..') for p in path.split('/')) or '\\' in path):
        raise ValueError('Unsafe field2D artwork path')
    file=api.ROOT/path
    if not file.resolve().is_relative_to(api.ROOT.resolve()):raise ValueError('Field2D path escapes root')
    raw=file.read_bytes();actual=hashlib.sha256(raw).hexdigest()
    if actual!=sha or api.INPUTS.get(path,actual)!=actual:raise ValueError('Field2D artwork bytes changed: '+path)
    if raw[:8]!=b'\x89PNG\r\n\x1a\n' or raw[12:16]!=b'IHDR':raise ValueError('Field2D artwork is not PNG')
    if size and list(struct.unpack('>II',raw[16:24]))!=size:raise ValueError('Field2D PNG dimensions changed')
    api.INPUTS[path]=actual
    return raw


def prepare(api, rows):
    api.read('tools/wiki_field_2d.py')
    manifest=json.loads(api.read(MANIFEST))
    entries=manifest.get('items',[])
    if (manifest.get('schemaVersion')!=1 or manifest.get('scope')!='2D artwork only'
            or manifest.get('status')!='ROOT_2D_VISUAL_PASS' or len(entries)!=5
            or {e['id'] for e in entries}!=IDS or set(rows)!=IDS
            or manifest.get('generationSurface')!='built-in image_gen'
            or manifest.get('generationModel')!='unknown' or manifest.get('threeDOperations')!=0 or set(manifest.get('dependencies',{}))!=DEPENDENCIES):
        raise ValueError('Changed five-item field2D artwork unit')
    producer=manifest['producerReceipt']
    if producer['path']!=PREFIX+'producer-receipt.json':raise ValueError('Invalid field2D producer receipt')
    api.read(producer['path'])
    if api.INPUTS[producer['path']]!=producer['sha256']:raise ValueError('Changed field2D producer receipt')
    for path,sha in manifest['dependencies'].items():
        api.read(path)
        if api.INPUTS[path]!=sha:raise ValueError('Field2D source semantics changed: '+path)
    plans=[]
    for entry in entries:
        identifier=entry['id'];icon=entry['icon'];master=entry['master']
        if icon['path']!=PREFIX+identifier+'.png' or icon['size']!=[512,512]:raise ValueError('Field2D icon identity changed')
        if master['path']!=PREFIX+'masters/'+identifier+'.png':raise ValueError('Field2D master identity changed')
        _binary(api,icon['path'],icon['sha256'],icon['size']);_binary(api,master['path'],master['sha256'],master['size'])
        authorities=entry['sourceEvidence']
        expected={PREFIX+folder+'/'+identifier+suffix for folder,suffix in [('prompts','.txt'),('provenance','.json'),('qa','.json')]}
        if len(authorities)!=3 or {a['path'] for a in authorities}!=expected:raise ValueError('Field2D source evidence inventory changed')
        for authority in authorities:
            api.read(authority['path'])
            if api.INPUTS[authority['path']]!=authority['sha256']:raise ValueError('Field2D source evidence changed')
        refs=[MANIFEST,icon['path'],master['path'],producer['path'],*manifest['dependencies'],*sorted(expected)]
        fields={'2D 원화 상태 / Artwork status':'제작 완료 · 게임 연결 검증 별도 / Artwork complete; runtime integration verification separate',
                '2D 원화 의미 / Artwork identity':entry['meaning'],
                '2D 원화 SHA-256 / Artwork SHA-256':icon['sha256'],
                '2D 원본 SHA-256 / Master SHA-256':master['sha256'],
                '생성 모델 / Verified model':'미확인 / Unknown',
                '그림 범위 / Image scope':'상자·성소의 독립 2D 원화. 게임 동작·저장·모델은 수정하지 않음. / Standalone2D field artwork; gameplay, save and model assets unchanged.'}
        plans.append({'id':identifier,'name':rows[identifier].get('name',identifier),'summary':entry['meaning'],'image':{'file':'evidence/field-items/'+identifier+'.png','role':'field-item-artwork'},'previewSource':icon['path'],'fields':fields,'source':api.source_ref(MANIFEST),'refs':[api.source_ref(p) for p in refs]})
    return plans
