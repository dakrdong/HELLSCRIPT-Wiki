"""Collect registered current art without treating historical attachments as current assets."""
import json
import re
import shutil
import subprocess
import tempfile
import wiki_art_retirement
import wiki_town_eight
from pathlib import Path, PurePosixPath


RESOURCES = 'Assets/HELLSCRIPT/Resources/'
ART = RESOURCES + 'Art/'
ARCHIVE_MANIFEST = 'tools/wiki_retired_art_sources.json'
RETIRED = tuple(ART + folder + '/' for folder in (
    'ClassLegendaryIcons', 'ClassSetIcons', 'EquipmentVariants', 'Jeweler/Gems', 'Jeweler/Potions'))
EQUIPMENT_RECEIPT = 'tools/casual_equipment_art_receipt.json'
PRESERVED_DOCUMENTS = (
    'Docs/Art/Art_Transition_20261014.md',
    'Docs/Art/Art_Transition_20261014.en.md',
    'Docs/Art/Art_Transition_20261014_Asset_Index.json',
)


def source_bytes(api, path, optional=False):
    safe_relative(path)
    if wiki_art_retirement.retired_path(api.ROOT,path):
        if optional: return None
        raise ValueError('Retired wiki source: ' + path)
    file = api.ROOT / path
    if not file.resolve().is_relative_to(api.ROOT.resolve()):
        raise ValueError('Wiki source escapes repository: ' + path)
    if file.is_file():
        return file.read_bytes()
    if not hasattr(api, '_historical_art'):
        api._historical_art = {}
    if path in api._historical_art:
        return api._historical_art[path]
    manifest_file = api.ROOT / ARCHIVE_MANIFEST
    manifest = json.loads(api.read(ARCHIVE_MANIFEST)) if manifest_file.is_file() else {'files': []}
    matches = [x for x in manifest['files'] if x['path'] == path]
    if len(matches) > 1:
        raise ValueError('Duplicate historical art source: ' + path)
    entry = matches[0] if matches else None
    if entry:
        safe_relative(path)
        allowed = path in PRESERVED_DOCUMENTS
        if not allowed or not isinstance(entry.get('commit'), str) or not re.fullmatch(r'[0-9a-f]{40}', entry['commit']):
            raise ValueError('Invalid historical art source: ' + path)
        archive_path = 'Wiki/assets/preserved-documents/20261014/' + PurePosixPath(path).name
        if entry.get('archivePath') != archive_path:
            raise ValueError('Invalid preserved document archive: ' + path)
        archive_file = api.ROOT / archive_path
        if not archive_file.is_file() or not archive_file.resolve().is_relative_to(api.ROOT.resolve()):
            raise ValueError('Missing or unsafe preserved document archive: ' + path)
        raw = archive_file.read_bytes()
        if len(raw) != entry['bytes'] or api.digest(raw) != entry['sha256']:
            raise ValueError('Missing or changed historical art object: ' + path)
        if path in PRESERVED_DOCUMENTS:
            api.INPUTS[archive_path] = entry['sha256']
        api._historical_art[path] = raw
        return raw
    if optional:
        return None
    raise ValueError('Missing wiki source: ' + path)


def archived_sources(api):
    used = getattr(api, '_historical_art', {})
    if not used:
        return {}
    manifest = json.loads(api.read(ARCHIVE_MANIFEST))
    return {x['path']: {k: x[k] for k in ('commit', 'sha256', 'bytes', 'archivePath', 'retirementScope', 'retiredOn') if k in x}
            for x in manifest['files'] if x['path'] in used and not wiki_art_retirement.retired_path(api.ROOT,x['path'])}


def safe_relative(value):
    if (not isinstance(value, str) or not value or value.startswith('/')
            or '\\' in value or re.match(r'^[a-zA-Z][a-zA-Z0-9+.-]*:', value)
            or any(part in ('', '.', '..') for part in value.split('/'))
            or any(ord(char) < 32 or ord(char) == 127 for char in value)):
        raise ValueError('Unsafe wiki resource path: ' + str(value))
    return PurePosixPath(value)


def preview_images(row):
    """Validate ordered exact bundles; keep legacy single-image and atlas consumers."""
    image = row.get('image')
    if image is not None:
        if not isinstance(image, dict) or 'file' not in image:
            raise ValueError('Invalid wiki preview image')
        safe_relative(image['file'])
    if 'images' not in row:
        return [image] if image is not None else []
    pictures = row['images']
    if not isinstance(pictures, list) or not 1 <= len(pictures) <= 9:
        raise ValueError('Wiki preview bundle requires 1 to 9 images')
    for item in pictures:
        if (not isinstance(item, dict) or not {'file', 'assetPath', 'label'} <= set(item)
                or set(item) - {'file', 'assetPath', 'label', 'count'}):
            raise ValueError('Invalid wiki preview bundle item')
        safe_relative(item['file']); safe_relative(item['assetPath'])
        if (not item['file'].endswith('.png') or not item['assetPath'].startswith(RESOURCES)
                or not item['assetPath'].endswith('.png')
                or not isinstance(item['label'], str) or not item['label'].strip()
                or ('count' in item and (type(item['count']) is not int or item['count'] <= 0))):
            raise ValueError('Invalid exact PNG preview bundle item')
    if (image is None or set(image) != {'file'} or image['file'] != pictures[0]['file']
            or row.get('assetPath') != pictures[0]['assetPath'] or row.get('previewSource')):
        raise ValueError('Primary preview must be a separate first bundle image copy')
    return pictures


def check_png_preview(raw, file):
    if len(raw) < 24 or raw[:8] != b'\x89PNG\r\n\x1a\n' or raw[12:16] != b'IHDR':
        raise ValueError('Exact preview is not PNG: ' + file)


def image_sources(databases):
    """A media key keeps the frontend contract; assetPath owns each original's bytes."""
    paths = {}
    for database in databases:
        for row in database['rows']:
            for image in preview_images(row):
                file = image['file']
                source = image['assetPath'] if 'images' in row else row.get('assetPath')
                evidence = row.get('previewSource')
                if evidence:
                    safe_relative(evidence)
                    role = image.get('role')
                    allowed = (
                        role == 'runtime-model-evidence' and row.get('category') == '현재 모델'
                        and evidence.startswith('Wiki/assets/runtime-models/')
                        or role == 'derived-rune-shape-preview' and (database.get('id') == 'rune-shapes'
                        or database.get('id') == 'resources' and re.fullmatch(r'preview-rune-rune-[1-5]-[0-9]{2}', row.get('id','')))
                        and evidence.startswith('Wiki/assets/rune-shapes-20261016/')
                        or role == 'field-item-artwork' and (database.get('id') == 'field'
                        or database.get('id') == 'resources' and re.fullmatch(r'field-art-(?:CH0[1-3]|SH0[1-2])', row.get('id','')))
                        and evidence.startswith('Wiki/assets/field-items-20261016/'))
                    if source or not allowed or not evidence.endswith('.png'):
                        raise ValueError(('Invalid runtime model evidence preview: ' if role=='runtime-model-evidence' or row.get('category')=='현재 모델' else 'Invalid verified evidence/artwork preview: ') + evidence)
                    if file in paths and paths[file] != evidence:
                        raise ValueError('Conflicting preview source: ' + file)
                    paths[file] = evidence
                if source:
                    safe_relative(source)
                    if not source.startswith(RESOURCES):
                        raise ValueError('Preview outside runtime resources: ' + source)
                    if file in paths and paths[file] != source:
                        raise ValueError('Conflicting preview source: ' + file)
                    paths[file] = source
    # Unowned legacy singles inherit a collected resource's mapping before Art/file fallback.
    for database in databases:
        for row in database['rows']:
            for image in preview_images(row):
                file = image['file']; paths.setdefault(file, ART + file)
    return paths


def copy_media(api, databases):
    """Replace only the generated media tree, leaving the prior tree until staging succeeds."""
    paths = image_sources(databases)
    gallery_files = {image['file'] for table in databases for row in table['rows']
                     if 'images' in row for image in preview_images(row)}
    api.SITE.mkdir(parents=True, exist_ok=True)
    with tempfile.TemporaryDirectory(dir=api.SITE, prefix='.media-build-') as temporary:
        stage = Path(temporary) / 'media'; stage.mkdir()
        for file, source in paths.items():
            original = api.ROOT / source
            if not original.resolve().is_relative_to(api.ROOT.resolve()):
                raise ValueError('Wiki preview escapes repository: ' + source)
            raw = original.read_bytes()
            if file in gallery_files: check_png_preview(raw, file)
            if api.INPUTS.get(source) != api.digest(raw):
                raise ValueError('Uncollected or changed preview: ' + source)
            target = stage / file; target.parent.mkdir(parents=True, exist_ok=True)
            target.write_bytes(raw)
        previous = api.SITE / '.media-previous'
        if previous.exists():
            raise ValueError('Previous media recovery still exists: ' + str(previous))
        target = api.SITE / 'media'
        if target.exists():
            target.rename(previous)
        try:
            stage.rename(target)
        except BaseException:
            if previous.exists():
                previous.rename(target)
            raise
        if previous.exists():
            shutil.rmtree(previous)


def _binary(api, path):
    raw = (api.ROOT / path).read_bytes()
    api.INPUTS[path] = api.digest(raw)
    return raw


def _image_row(api, path, identifier=None, category=None, source=None, fields=None, status='등록된 게임 리소스'):
    from PIL import Image
    raw = _binary(api, path)
    meta_path = path + '.meta'; meta = api.read(meta_path)
    guid = re.search(r'^guid: (\w+)', meta, re.M)
    if not guid:
        raise ValueError('Missing Unity GUID: ' + path)
    with Image.open(api.ROOT / path) as image:
        details = {'원본 경로 / Source': path, '해상도 / Size': f'{image.width} × {image.height}',
                   '색상 모드 / Mode': image.mode, '파일 크기 / Bytes': len(raw),
                   'SHA-256': api.digest(raw), 'Unity GUID': guid[1],
                   '상태 해석 / Scope': '등록과 실제 적용·출시 승인을 구분합니다. / Registration is not release approval.'}
    details.update(fields or {})
    media = path.removeprefix(ART) if path.startswith(ART) else '_resources/' + path.removeprefix(RESOURCES)
    ident = identifier or 'asset-current-' + re.sub(r'[^a-zA-Z0-9_-]', '-', media)
    category = category or '현재 이미지 · ' + str(PurePosixPath(media).parent)
    return api.record(ident, Path(path).stem, category,
                      '현재 등록된 게임 이미지 원본 / Current registered game artwork', details,
                      source or path, status=status, image={'file': media}, assetPath=path,
                      refs=[api.source_ref(meta_path)])


def _equipment(api, databases):
    receipt = json.loads(api.read(EQUIPMENT_RECEIPT)); rows = []; by_catalog = {}
    for entry in receipt['records']:
        path = entry['destination']; raw = _binary(api, path)
        if api.digest(raw) != entry['destination_sha256'] or len(raw) != entry['bytes']:
            raise ValueError('Approved equipment source changed: ' + entry['id'])
        row = _image_row(api, path, 'equipment-art-' + entry['id'], '장비 정본 원화', EQUIPMENT_RECEIPT,
                         {'콘텐츠 ID / Catalog IDs': entry['catalog_ids'], '분류 / Group': entry['group'],
                          '사용 범위 / Source status': entry['source_status'], '승인 근거 / Approval': receipt['approval_receipt']},
                         '승인 원화 · 사용 범위 별도 / Approved art; scoped usage')
        if row['fields']['Unity GUID'] != entry['guid']:
            raise ValueError('Approved equipment GUID changed: ' + entry['id'])
        rows.append(row)
        for key in entry['catalog_ids']:
            if key in by_catalog:
                raise ValueError('Duplicate equipment catalog binding: ' + key)
            by_catalog[key] = row
    for database in databases:
        if database['id'] not in ('items', 'legendaries', 'sets', 'set-items'):
            continue
        for content in database['rows']:
            art = by_catalog.get(content['id'])
            if not art:
                raise ValueError('Missing canonical equipment art: ' + content['id'])
            content['image'] = dict(art['image']); content['resource'] = art['id']
            if '아이콘' in content['fields']:
                content['fields']['아이콘'] = '공통 정본 장비 원화를 사용합니다. / Shared canonical equipment artwork.'
    return rows


def _models(api, databases, existing):
    rows = []; replaced = set(); by_db = {d['id']: d for d in databases}
    monsters_path = RESOURCES + 'Monsters/MonsterRegistry.json'
    monsters = json.loads(api.read(monsters_path))['monsters']
    portraits = {m['art']: ART + m['portrait'].removeprefix('Art/') + '.png'
                 for m in monsters if m.get('art') and m.get('portrait')}
    hero_images = {r['id'].removeprefix('dialogue-portrait-hero-'): r['image'] for r in existing
                   if r['id'].startswith('dialogue-portrait-hero-')}
    root = api.ROOT / (RESOURCES + 'World/HunyuanReady')
    for file in sorted(root.rglob('ready_recipe.json')):
        path = file.relative_to(api.ROOT).as_posix(); recipe = json.loads(api.read(path)); folder = file.parent
        prefab = folder / 'Model.prefab'
        if not prefab.is_file():
            raise ValueError('READY recipe has no prefab: ' + path)
        prefab_path = prefab.relative_to(api.ROOT).as_posix(); _binary(api, prefab_path)
        key = recipe['art'].split('/')[-1]
        identifier = 'model-ready-' + recipe['art'].replace('/', '-')
        fields = {'모델 경로 / Prefab': prefab_path, '정본 키 / Art': recipe['art'],
                  'LOD 삼각형 / Triangles': recipe['triangles'], 'FBX SHA-256': recipe['fbxSHA256'],
                  '높이 / Height': recipe['height'], '형태 / Archetype': recipe['archetype'],
                  '검증 범위 / Scope': '설치·레시피 기록이며 실제 실행 검증 완료를 뜻하지 않습니다. / Installed recipe; not fresh runtime proof.'}
        provenance = api.ROOT / ('Assets/HELLSCRIPT/PreparedModels/HunyuanReady/' + key + '/provenance.json')
        refs = [api.source_ref(prefab_path)]
        if provenance.is_file():
            refs.append(api.source_ref(provenance.relative_to(api.ROOT).as_posix()))
        extra = {}; content_key = key; group = 'enemies'
        if key.startswith('Hero_'):
            role = key.split('_')[1].lower(); group = 'heroes'; content_key = role.upper()
            if role in hero_images:
                extra['image'] = hero_images[role]
        else:
            portrait = portraits.get(recipe['art'])
            if portrait and (api.ROOT / portrait).is_file():
                _binary(api, portrait); extra['image'] = {'file': portrait.removeprefix(ART)}
            if recipe['art'].startswith('Bosses/'):
                group = 'bosses'; content_key = 'BOSS' + key[1:] if re.fullmatch(r'B\d\d', key) else key
        row = api.record(identifier, key, '현재 모델', '설치된 Hunyuan READY 모델 / Installed Hunyuan READY model',
                         fields, path, status='설치됨 · 실행 증거 별도 / Installed; runtime evidence separate', refs=refs, **extra)
        content = next((r for r in by_db.get(group, {}).get('rows', []) if r['id'] == content_key), None)
        if content:
            row['content'] = {'db': group, 'id': content_key}
            if not key.endswith('_F'):
                content['resource'] = identifier; replaced.add('model-' + content_key)
        rows.append(row)
    return rows, replaced



def _town_models(api):
    """Publish only the adopted Mark representative, with hash-bound actual Editor proof."""
    manifest_path = RESOURCES + 'World/Town/manifest_town_hunyuan.json'
    if not (api.ROOT / manifest_path).is_file():
        return []
    manifest = json.loads(api.read(manifest_path))
    key = 'Npc_MarkKus'
    model = manifest['models'][key]
    proof_path = 'Wiki/assets/runtime-models/mark-kus/receipt.json'
    proof = json.loads(api.read(proof_path))
    if (proof.get('status') != 'MARK1_FOCUSED_EDITOR_PASS'
            or proof.get('runtime', {}).get('status') != 'PASSED'
            or proof.get('geometry', {}).get('triangles') != model['tris']
            or proof.get('geometry', {}).get('height') != model['height']
            or proof.get('geometry', {}).get('footY') != 0
            or not proof.get('geometry', {}).get('staticMesh')
            or proof.get('other11NPCReplaced') != 0):
        raise ValueError('Missing or invalid focused Town model proof: ' + key)
    names = {key + '.fbx', key + '_A.png', key + '_N.png'}
    if set(model['sha256']) != names or set(proof.get('runtimeFiles', {})) != {RESOURCES + 'World/Town/' + name for name in names}:
        raise ValueError('Invalid Mark runtime file inventory')
    refs = [api.source_ref(proof_path)]
    hashes = {}; guids = {}
    for filename, expected in model['sha256'].items():
        path = RESOURCES + 'World/Town/' + filename
        raw = _binary(api, path)
        if api.digest(raw) != expected or proof['runtimeFiles'].get(path, {}).get('sha256') != expected:
            raise ValueError('Town model source differs from focused proof: ' + path)
        meta = path + '.meta'
        meta_text = api.read(meta)
        if api.digest(meta_text.encode()) != proof['runtimeFiles'][path]['metaSha256']:
            raise ValueError('Town model meta differs from focused proof: ' + meta)
        guid = re.search(r'^guid: (\w+)', meta_text, re.M)
        if not guid:
            raise ValueError('Missing Town model GUID: ' + meta)
        hashes[filename] = expected; guids[filename] = guid[1]
        refs += [api.source_ref(path), api.source_ref(meta)]
    preview = proof['preview']['path']; safe_relative(preview)
    if not preview.startswith('Wiki/assets/runtime-models/mark-kus/') or not preview.endswith('.png'):
        raise ValueError('Invalid Mark evidence preview path')
    if api.digest(_binary(api, preview)) != proof['preview']['sha256']:
        raise ValueError('Changed Mark evidence screenshot')
    refs.append(api.source_ref(preview))
    fields = {'모델 경로 / Model': RESOURCES + 'World/Town/' + key + '.fbx',
              '삼각형 / Triangles': model['tris'], '높이 / Height': model['height'],
              '텍스처 / Texture': model['texture'], '원점 / Origin': model['origin'],
              '정면 / Front': model['front'], '파일 SHA-256 / Files': hashes, 'Unity GUID': guids,
              '제작 근거 / Source': model['source'], '실행 검증 커밋 / Runtime source': proof['integratedSourceCommit'],
              '검증 범위 / Scope': '실제 Editor 마을·대화·대장간, 가로·세로. 다른 NPC11·플레이어 빌드는 미포함. / Actual Editor Town, dialogue and forge in both orientations; excludes other11 NPCs and player build.',
              '미리보기 / Preview': '실제 Editor 근접 화면이며 원화나 독립 3D 뷰어가 아닙니다. / Actual Editor close-up, not standalone artwork or a 3D viewer.',
              '출시 승인 / Production approved': manifest['productionApproved']}
    return [api.record('model-town-' + key, '마르크 쿠스 / Mark Kus', '현재 모델',
                       '캐주얼 마을 NPC 대표 모델 / Casual Town NPC representative', fields, manifest_path,
                       status='Editor 검증 완료 · 출시 승인 별도 / Editor verified; release approval separate',
                       refs=refs, previewSource=preview,
                       image={'file': 'evidence/models/mark-kus.png', 'role': 'runtime-model-evidence'})]

def build(api, databases, existing):
    rows = _equipment(api, databases)
    represented = {r.get('assetPath') for r in existing + rows if r.get('image') or r.get('historicalOnly')}
    # These are runtime Resources, not original artwork, scratch output or historical archives.
    roots = [api.ROOT / ART, api.ROOT / (RESOURCES + 'DailyQuestCasual'), api.ROOT / (RESOURCES + 'UI/PotionQuickCasual')]
    for root in roots:
        for file in sorted(root.rglob('*.png')):
            path = file.relative_to(api.ROOT).as_posix()
            if path in represented or path.startswith(RETIRED) or wiki_art_retirement.retired_path(api.ROOT,path):
                continue
            rows.append(_image_row(api, path)); represented.add(path)
    model_rows, replaced = _models(api, databases, existing)
    rows.extend(model_rows)
    rows.extend(_town_models(api))
    rows.extend(wiki_town_eight.build(api, _binary, safe_relative))
    for row in rows:
        if row.get('assetPath', '').startswith(ART + 'CasualAspectStone/'):
            row['id'] = 'asset-aspect-LW02'; row['category'] = '위상 정본 원화'
            replaced.add('asset-aspect-LW02')
    for file in sorted((api.ROOT / (RESOURCES + 'World/Fields')).glob('F*/manifest_fields_casual.json')):
        path = file.relative_to(api.ROOT).as_posix(); data = json.loads(api.read(path)); name = file.parent.name
        rows.append(api.record('field-art-' + name, name, '현재 필드 아트', '필드 정본 모델·재질 집합 / Current field art set',
                              {'모델 / Models': sorted(data['models']), '파일 수 / Files': len(data['files']),
                               '출시 승인 / Production approved': data['productionApproved'], 'revision': data['revision']},
                              path, status='개발 적용 · 출시 승인 별도 / Development; release approval separate'))
    fonts = api.ROOT / (RESOURCES + 'Fonts/CasualSkill')
    for file in sorted(fonts.glob('*')):
        if file.suffix.lower() not in ('.ttf', '.otf'):
            continue
        path = file.relative_to(api.ROOT).as_posix(); raw = _binary(api, path)
        rows.append(api.record('font-current-' + file.stem, file.stem, '번들 글꼴', '캐주얼 UI 번들 글꼴 / Bundled casual UI font',
                              {'경로 / Path': path, '파일 크기 / Bytes': len(raw), 'SHA-256': api.digest(raw)}, path,
                              status='등록된 게임 리소스'))
    replaced.add('todo-character-rig')
    retained = [r for r in existing if r['id'] not in replaced and (not r['id'].startswith('legacy-cell-') or r.get('historicalOnly'))
                and not r.get('assetPath', '').startswith(RETIRED)]
    for row in retained:
        if row['id'].startswith('model-'):
            row['summary'] = '런타임 대체 표현 기록 / Runtime fallback representation'
            row['fields']['독립 모델·프리팹'] = '현재 모델·필드 정본 항목과 런타임 적용 범위를 함께 확인합니다. / See current model and field records.'
    return wiki_art_retirement.retained_rows(api.ROOT, retained + rows)

# Presentation-only roles: exact current paths, never a frame/name prefix rule.
FUNCTIONAL_IMAGE_ROLES = {
    ART + 'RuneBoard/Icons/emblem-attack-a.png': ('9px 룬 유형 식별 표식 / Tiny rune type indicator', '작은 보드 기능 기호이며 큰 분류는 별도 원화를 사용합니다. / Tiny board function glyph; large categories use separate painted artwork.'),
    ART + 'RuneBoard/Icons/emblem-critical-b.png': ('9px 룬 유형 식별 표식 / Tiny rune type indicator', '작은 보드 기능 기호이며 큰 분류는 별도 원화를 사용합니다. / Tiny board function glyph; large categories use separate painted artwork.'),
    ART + 'RuneBoard/Icons/emblem-magic-a.png': ('9px 룬 유형 식별 표식 / Tiny rune type indicator', '작은 보드 기능 기호이며 큰 분류는 별도 원화를 사용합니다. / Tiny board function glyph; large categories use separate painted artwork.'),
    ART + 'RuneBoard/Icons/emblem-skill-a.png': ('9px 룬 유형 식별 표식 / Tiny rune type indicator', '작은 보드 기능 기호이며 큰 분류는 별도 원화를 사용합니다. / Tiny board function glyph; large categories use separate painted artwork.'),
    ART + 'RuneBoard/Icons/emblem-support-a.png': ('9px 룬 유형 식별 표식 / Tiny rune type indicator', '작은 보드 기능 기호이며 큰 분류는 별도 원화를 사용합니다. / Tiny board function glyph; large categories use separate painted artwork.'),
    ART + 'GlobalHUD/Casual/mask-circle.png': ('원형 클리핑 마스크 / Circular clipping mask', '원형 자르기와 진행 게이지에 쓰는 기능 이미지입니다. / Functional circle mask for clipping and gauges.'),
    ART + 'GlobalHUD/Casual/mask-square.png': ('사각 클리핑 마스크 / Square clipping mask', '아이콘 자르기와 쿨다운에 쓰는 기능 이미지입니다. / Functional square mask for icons and cooldowns.'),
    ART + 'GlobalHUD/Casual/fill-vital.png': ('게이지 채움 / Gauge fill', '게임이 색과 채움 비율을 적용하는 게이지 바탕입니다. / Tintable gauge fill; not a painted border.'),
    ART + 'GlobalHUD/Casual/panel-cream.png': ('분할 확장 패널 바탕 / Sliced panel base', '모서리를 보존해 늘려 쓰는 UI 패널 바탕입니다. / Stretchable UI panel base with preserved corners.'),
    ART + 'GlobalHUD/Casual/panel-white.png': ('분할 확장 패널 바탕 / Sliced panel base', '게임이 색을 입혀 쓰는 UI 패널 기본 바탕입니다. / Tintable UI panel base.'),
    ART + 'GlobalHUD/Casual/plate-icon.png': ('상태 아이콘 바탕 / Status icon backing', '상태 아이콘 아래에 놓고 색과 페이드를 적용하는 바탕입니다. / Tintable backing for effect icons; not a clipping mask.'),
    ART + 'GlobalHUD/Casual/ring-state.png': ('위험·사망 상태 겹침 / Danger and death overlay', '체력 위험·사망 상태를 색으로 표시하는 겹침 이미지입니다. / Tintable state overlay; not a clipping mask.'),
    ART + 'GlobalHUD/Casual/xp-tick.png': ('경험치 눈금 / Experience tick', '경험치 진행 눈금용 기능 이미지입니다. / Functional experience tick.'),
    ART + 'GlobalHUD/Casual/xp-tick-major.png': ('경험치 주요 눈금 / Major experience tick', '경험치 주요 눈금용 기능 이미지입니다. / Functional major experience tick.'),
    ART + 'GlobalHUD/fill-white.png': ('단색 채움 / Solid fill', '게임이 색을 입혀 막대와 면을 채우는 기본 이미지입니다. / Tintable solid-fill primitive.'),
    ART + 'GlobalHUD/mask-diamond.png': ('마름모 마스크 / Diamond mask', '마름모형 자르기용 도형 리소스입니다. / Diamond clipping primitive; registration does not prove current use.'),
    ART + 'GlobalHUD/mask-edge-smooth.png': ('가장자리 마스크 / Edge mask', '가장자리 처리를 위한 기능 이미지입니다. / Functional edge mask.'),
    ART + 'GlobalHUD/portrait-placeholder.png': ('초상화 대체 이미지 / Portrait placeholder', '전용 초상화가 없을 때 쓰는 공통 대체 이미지입니다. / Shared missing-portrait fallback, not a character illustration.'),
    ART + 'GlobalHUD/xp-cap.png': ('경험치 끝단 표식 / Experience cap', '경험치 끝단 표식용 기능 리소스입니다. / Experience-cap primitive; registration does not prove current use.'),
}


def annotate_resource_roles(rows):
    """Label rendering primitives without changing media, identity, scope or provenance."""
    for row in rows:
        role = FUNCTIONAL_IMAGE_ROLES.get(row.get('assetPath'))
        if role is None:
            continue
        row['category'] = 'UI 기능 이미지'
        row['summary'] = role[1]
        row['fields']['표현 용도 / Visual role'] = role[0]
        row['fields']['완성 원화 여부 / Standalone illustration'] = 'UI 기능용 이미지이며 독립 장식 원화가 아닙니다. / UI rendering primitive, not a standalone illustration.'
    return rows
