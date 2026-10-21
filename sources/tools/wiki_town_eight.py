"""Expose the exact eight adopted Town models with their existing Editor proof."""
import json
import re

ROOT = 'Assets/HELLSCRIPT/Resources/World/Town/'
MANIFEST = ROOT + 'manifest_town_hunyuan.json'
PROOF = 'Wiki/assets/runtime-models/town-eight/receipt.json'
EVIDENCE = 'Docs/Implementation/TownEightCasualEvidence/receipt.json'
NAMES = {
    'Npc_JakeBokun': '제이크 보쿤 / Jake Bokun',
    'Npc_AntonJindark': '안톤 진다르크 / Anton Jindark',
    'Npc_TurkGarbi': '터크 가르비 / Turk Garbi',
    'Npc_PyonyaNermwen': '표냐 네르웬 / Pyonya Nermwen',
    'Prop_Barrel': '나무 통 / Barrel',
    'Prop_BannerPost': '깃발 기둥 / Banner post',
    'Prop_Signpost': '방향 표지판 / Signpost',
    'Prop_FenceSegment': '울타리 구간 / Fence segment',
}


def build(api, binary, safe_relative):
    if not (api.ROOT / PROOF).is_file():
        return []
    api.read('tools/wiki_town_eight.py')
    proof = json.loads(api.read(PROOF))
    manifest = json.loads(api.read(MANIFEST))
    expected_validation = dict(EditModePassed=23, EditModeFailed=0, NPCChecks=8,
                               dialogues=8, services=6, pointerActions=24,
                               outerFenceSegments=136, wholeNUnit=0,
                               nativeBuilds=0, deviceTests=0, runtimeErrors=0)
    if (proof.get('schemaVersion') != 1 or proof.get('status') != 'TOWN8_FOCUSED_EDITOR_PASS'
            or proof.get('modelCount') != 8 or set(proof.get('models', {})) != set(NAMES)
            or proof.get('validation') != expected_validation
            or not re.fullmatch('[0-9a-f]{40}', proof.get('integratedSourceCommit', ''))
            or proof.get('sourceProof') != EVIDENCE or not proof.get('sourcePixelsUnchanged')
            or proof.get('imageEdits') != 0):
        raise ValueError('Changed focused Town8 proof contract')
    source = json.loads(api.read(EVIDENCE))
    if (api.INPUTS[EVIDENCE] != proof['sourceProofSHA256']
            or source.get('state') != 'FOCUSED_TOWN_EIGHT_PASS'
            or set(source.get('modelIds', [])) != set(NAMES)):
        raise ValueError('Town8 source proof changed')
    expected_paths = {ROOT + key + suffix for key in NAMES for suffix in ('.fbx', '_A.png', '_N.png')}
    if set(proof.get('runtimeFiles', {})) != expected_paths:
        raise ValueError('Town8 runtime inventory changed')
    source_files = {r['target']: r for r in source.get('runtimeFiles', [])}
    if set(source_files) != expected_paths or any(
            source_files[path]['sha256'] != proof['runtimeFiles'][path]['sha256']
            or source_files[path]['bytes'] != proof['runtimeFiles'][path]['bytes']
            for path in expected_paths):
        raise ValueError('Town8 file authority differs from integrated source proof')
    if (source.get('EditMode', {}).get('status') != 'PASS'
            or source.get('EditMode', {}).get('total') != 23
            or not source.get('editorClosed', {}).get('closed')):
        raise ValueError('Town8 integrated gate incomplete')
    output = []
    for key, label in NAMES.items():
        item = proof['models'][key]; model = manifest['models'].get(key)
        if model != item.get('facts') or model.get('tris') != (5998 if key.startswith('Npc_') else 2498):
            raise ValueError('Town8 model facts changed: ' + key)
        names = {key + suffix for suffix in ('.fbx', '_A.png', '_N.png')}
        if set(model.get('sha256', {})) != names:
            raise ValueError('Town8 model file set changed: ' + key)
        refs = [api.source_ref(PROOF), api.source_ref(EVIDENCE)]
        guids = {}
        for filename, sha in model['sha256'].items():
            path = ROOT + filename
            if api.digest(binary(api, path)) != sha or proof['runtimeFiles'][path]['sha256'] != sha:
                raise ValueError('Town8 runtime bytes changed: ' + path)
            meta = api.read(path + '.meta')
            if api.digest(meta.encode()) != proof['runtimeFiles'][path]['metaSha256']:
                raise ValueError('Town8 meta changed: ' + path)
            guid = re.search(r'^guid: ([0-9a-f]{32})$', meta, re.M)
            if guid is None:
                raise ValueError('Town8 GUID missing: ' + path)
            guids[filename] = guid[1]
            refs += [api.source_ref(path), api.source_ref(path + '.meta')]
        preview = item['preview']['path']; safe_relative(preview)
        if not preview.startswith('Wiki/assets/runtime-models/town-eight/') or not preview.endswith('.png'):
            raise ValueError('Invalid Town8 evidence path')
        if api.digest(binary(api, preview)) != item['preview']['sha256']:
            raise ValueError('Town8 screenshot changed: ' + key)
        refs.append(api.source_ref(preview))
        fields = {'모델 경로 / Model': ROOT + key + '.fbx', '삼각형 / Triangles': model['tris'],
                  '높이 / Height': model['height'], '텍스처 / Texture': model['texture'],
                  '원점 / Origin': model['origin'], '정면 / Front': model['front'],
                  '파일 SHA-256 / Files': model['sha256'], 'Unity GUID': guids,
                  '실행 검증 커밋 / Runtime source': proof['integratedSourceCommit'],
                  '미리보기 / Preview': item['preview']['scope'],
                  '검증 범위 / Scope': '집중 Editor 검사23건·가로/세로 NPC4 대화와 서비스3·소품4. 새 player/실기기/성능 검증 미실행. / Focused Editor23 checks, four NPC dialogues and three services in both orientations, four props. New player/device/performance tests not run.',
                  '출시 승인 / Production approved': manifest['productionApproved']}
        output.append(api.record('model-town-' + key, label, '현재 모델',
                      '실제 Editor 검수한 캐주얼 마을 모델 / Casual Town model verified in the actual Editor',
                      fields, MANIFEST, status='Editor 검증 완료 · 출시 승인 별도 / Editor verified; release approval separate',
                      refs=refs, previewSource=preview,
                      image={'file':'evidence/models/town-eight/' + key + '.png', 'role':'runtime-model-evidence'}))
    return output
