#!/usr/bin/env python3
"""Import the authored Character Plaza layers without running Unity.

Run the preserved prep_scene.py for both specs into --layers first. This script
requires exact agreement with scene_data.json, preserves original pose PNG bytes,
bakes GPU-only hit masks and writes stable resource paths and deterministic GUIDs.
--from-webp is an explicit fallback for systems without macOS Vision; its lossy
source provenance is recorded. --verify performs read-only checks of all outputs.
"""
import argparse
import base64
import hashlib
import json
import math
from pathlib import Path
import shutil
import uuid

import numpy as np
from PIL import Image

ROOT = Path(__file__).resolve().parents[1]
ART = ROOT / 'Docs/Art/CasualConcept/2026-10-13/CharacterPlaza'
DESIGN = ROOT / 'Docs/Design/CharacterPlaza'
DEST = ROOT / 'Assets/HELLSCRIPT/Resources/Art/CharacterPlaza'
RECEIPT = ROOT / 'tools/character_plaza_art_receipt.json'
PREFIX = 'Art/CharacterPlaza/'
IDS = ('warrior_m', 'warrior_f', 'archer_m', 'archer_f', 'mage_m', 'mage_f')
CLASSES = {'warrior': 'Warrior', 'archer': 'Ranger', 'mage': 'Mage'}
ALPHA_THRESHOLD = 8


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def write_json(path, data):
    path.write_text(json.dumps(data, indent=2, ensure_ascii=False) + '\n')


def resource(path):
    return PREFIX + path.stem


def guid(path):
    return uuid.uuid5(uuid.NAMESPACE_URL, 'hellscript/character-plaza/' + path.name).hex


def meta(path, cap=2048, folder=False):
    header = 'fileFormatVersion: 2\nguid: ' + guid(path) + '\n'
    if folder:
        body = 'folderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n'
    elif path.suffix == '.json':
        body = 'TextScriptImporter:\n  externalObjects: {}\n'
    else:
        body = '''TextureImporter:
  internalIDToNameTable: []
  externalObjects: {}
  serializedVersion: 13
  mipmaps:
    enableMipMap: 0
    sRGBTexture: 1
  isReadable: 0
  streamingMipmaps: 0
  maxTextureSize: CAP
  textureSettings:
    serializedVersion: 2
    filterMode: 1
    aniso: 1
    wrapU: 1
    wrapV: 1
    wrapW: 1
  nPOTScale: 0
  compressionQuality: 100
  spriteMode: 0
  alphaUsage: 1
  alphaIsTransparency: 1
  textureType: 0
  textureShape: 1
  platformSettings:
'''.replace('CAP', str(cap))
        # Android ASTC_6x6 = 50; desktop BC7 = 25; WebGL ETC2_RGBA8 = 47.
        for target, fmt, override in [('DefaultTexturePlatform', -1, 0), ('Standalone', 25, 1), ('Android', 50, 1), ('WebGL', 47, 1)]:
            body += f'''  - serializedVersion: 4
    buildTarget: {target}
    maxTextureSize: {cap}
    resizeAlgorithm: 0
    textureFormat: {fmt}
    textureCompression: 1
    compressionQuality: 100
    crunchedCompression: 0
    allowsAlphaSplitting: 0
    overridden: {override}
'''
    Path(str(path) + '.meta').write_text(header + body + '  userData:\n  assetBundleName:\n  assetBundleVariant:\n')


def hit_mask(image):
    alpha = image.convert('RGBA').getchannel('A').resize((64, 64), Image.Resampling.BOX)
    bits = np.array(alpha).ravel() > ALPHA_THRESHOLD
    return base64.b64encode(np.packbits(bits, bitorder='little').tobytes()).decode('ascii')


def manifest_records(node):
    if isinstance(node, dict):
        if isinstance(node.get('sha256'), str):
            yield node
        for value in node.values():
            yield from manifest_records(value)
    elif isinstance(node, list):
        for value in node:
            yield from manifest_records(value)


def original_inventory():
    manifests = [(p, list(manifest_records(json.loads(p.read_text())))) for p in sorted(ART.glob('manifest_*.json'))]
    result = []
    for path in sorted(ART.glob('*.png')):
        digest = sha(path)
        matches = [(p, r) for p, records in manifests for r in records if r['sha256'] == digest]
        assert matches, f'Original PNG has no matching authored manifest: {path.name}'
        p, record = matches[-1]
        image = Image.open(path)
        result.append({'path': str(path.relative_to(ROOT)), 'sha256': digest, 'bytes': path.stat().st_size,
                       'width': image.width, 'height': image.height, 'manifest': str(p.relative_to(ROOT)),
                       'generatedSource': record.get('source_path'), 'productionApproved': False})
    return result


def import_art(args):
    original = original_inventory()
    design = json.loads((DESIGN / 'scene_data.json').read_text())
    DEST.mkdir(parents=True, exist_ok=True)
    meta(DEST, folder=True)
    records = []

    def add(source, name=None, cap=512, decode=False, processing='byte-for-byte copy'):
        target = DEST / (name or source.name)
        if decode:
            Image.open(source).save(target, optimize=True)
        else:
            shutil.copyfile(source, target)
        im = Image.open(target)
        meta(target, cap)
        records.append({'resource': resource(target), 'path': str(target.relative_to(ROOT)),
                        'source': str(source.relative_to(ROOT)), 'sourceSha256': sha(source),
                        'sha256': sha(target), 'bytes': target.stat().st_size, 'width': im.width,
                        'height': im.height, 'mode': im.mode, 'maxTextureSize': cap,
                        'guid': guid(target), 'processing': processing, 'productionApproved': False})
        return target

    for orientation in ('landscape', 'portrait'):
        expected = design[orientation]
        if not args.from_webp:
            actual = json.loads((args.layers / f'scn_{orientation}.json').read_text())
            assert actual == expected, f'Native scene coordinates differ from authored scene_data: {orientation}'
        background = add(ART / f'scene_{orientation}.png', f'{orientation}_bg.png', 2048)
        scene = {'width': expected['W'], 'height': expected['H'], 'background': resource(background), 'heroes': [], 'focus': []}
        for ident in IDS:
            h = expected['heroes'][ident]
            entry = {'artId': ident, 'headX': h['head'][0], 'headY': h['head'][1],
                     'feetX': h['feet'][0], 'feetY': h['feet'][1], 'cheerHeight': h['cheer'][2]}
            for kind in ('cut', 'patch'):
                rect = h[kind]
                source = DESIGN / 'assets' / (Path(rect['file']).stem + '.webp') if args.from_webp else args.layers / rect['file']
                target = add(source, rect['file'], decode=args.from_webp,
                             processing='Lossy authored WebP decoded to PNG; Vision-derived development layer' if args.from_webp else 'Native macOS Vision extraction; development layer')
                im = Image.open(target)
                assert im.size == (rect['w'], rect['h'])
                entry[kind] = {'resource': resource(target), 'x': rect['x'], 'y': rect['y'], 'width': rect['w'], 'height': rect['h']}
                if kind == 'cut':
                    entry[kind]['hitMask'] = hit_mask(im)
            scene['heroes'].append(entry)
        scene['focus'] = [{'heroClass': key, 'x': val[0], 'y': val[1], 'zoom': val[2], 'screenX': val[3]} for key, val in expected['focus'].items()]
        path = DEST / f'scene_{orientation}.json'
        write_json(path, scene)
        meta(path)

    art = {'poses': [], 'crests': []}
    for ident in IDS:
        poses = {'artId': ident}
        for kind in ('cheer', 'fight'):
            target = add(ART / f'{kind}_{ident}.png', cap=1024)
            im = Image.open(target)
            bounds = Image.fromarray((np.array(im.getchannel('A')) > ALPHA_THRESHOLD).astype('uint8') * 255).getbbox()
            x, y, right, bottom = bounds
            poses[kind] = {'resource': resource(target), 'uvX': x / im.width, 'uvY': 1 - bottom / im.height,
                           'uvWidth': (right - x) / im.width, 'uvHeight': (bottom - y) / im.height,
                           'width': right - x, 'height': bottom - y, 'hitMask': hit_mask(im.crop(bounds))}
        art['poses'].append(poses)
    for hero_class in ('Warrior', 'Ranger', 'Mage'):
        source = DESIGN / 'assets' / f'crest_{hero_class.lower()}.webp'
        target = add(source, source.stem + '.png', 256, decode=True, processing='Authored crest crop WebP decoded losslessly to PNG; original crests.png retained in Docs')
        art['crests'].append({'heroClass': hero_class, 'resource': resource(target)})
    path = DEST / 'art.json'
    write_json(path, art)
    meta(path)
    rgba_bytes = sum(r['width'] * r['height'] * 4 for r in records)
    astc_bytes = 0
    for r in records:
        scale = min(1, r['maxTextureSize'] / max(r['width'], r['height']))
        astc_bytes += math.ceil(round(r['width'] * scale) / 6) * math.ceil(round(r['height'] * scale) / 6) * 16
    write_json(RECEIPT, {'schema': 1, 'sourceArtCommit': '13a1ce7322ae69f5b518503b52598fbadf7ee5b8',
                        'productionApproved': False, 'originals': original, 'records': records,
                        'sceneCoordinates': '12/12 exact authored scene_data matches', 'alphaMask': {'size': 64, 'threshold': ALPHA_THRESHOLD, 'rows': 'top-to-bottom', 'bitOrder': 'little'},
                        'poseCrop': 'Original PNG bytes preserved; UV crop to alpha > 8 avoids faint generated background speckles',
                        'guidPolicy': 'UUIDv5 of hellscript/character-plaza/<filename>',
                        'runtimePngCount': len(records), 'runtimePngBytes': sum(r['bytes'] for r in records),
                        'uncompressedRgbaBytes': rgba_bytes, 'estimatedAndroidAstc6x6Bytes': astc_bytes,
                        'budgetRowsRequested': [{'prefix': 'Art/CharacterPlaza/landscape_bg', 'max': 2048}, {'prefix': 'Art/CharacterPlaza/portrait_bg', 'max': 2048},
                                                {'prefix': 'Art/CharacterPlaza/cheer_', 'max': 1024}, {'prefix': 'Art/CharacterPlaza/fight_', 'max': 1024},
                                                {'prefix': 'Art/CharacterPlaza/crest_', 'max': 256}, {'prefix': 'Art/CharacterPlaza/', 'max': 512}],
                        'recipe': 'python3 tools/prepare_character_plaza_art.py --from-webp' if args.from_webp else 'python3 tools/prepare_character_plaza_art.py --layers <prep_scene output>',
                        'layerSource': 'authored WebP fallback' if args.from_webp else 'native macOS Vision regeneration',
                        'validation': 'Static only; Unity import, native presentation and texture memory remain pending',
                        'preservation': 'Original art, old CharacterSelection textures, metadata and shared consumers preserved'})


def verify():
    receipt = json.loads(RECEIPT.read_text())
    assert len(receipt['records']) == 41
    for r in receipt['originals']:
        assert sha(ROOT / r['path']) == r['sha256']
    seen = set()
    for r in receipt['records']:
        p = ROOT / r['path']
        assert sha(p) == r['sha256']
        assert sha(ROOT / r['source']) == r['sourceSha256']
        assert r['guid'] not in seen
        seen.add(r['guid'])
        text = Path(str(p) + '.meta').read_text()
        assert 'isReadable: 0' in text and 'enableMipMap: 0' in text and 'guid: ' + r['guid'] in text
    design = json.loads((DESIGN / 'scene_data.json').read_text())
    for orientation in ('landscape', 'portrait'):
        scene = json.loads((DEST / f'scene_{orientation}.json').read_text())
        assert len(scene['heroes']) == 6
        assert [scene['width'], scene['height']] == [design[orientation]['W'], design[orientation]['H']]
        assert scene['background'] == PREFIX + orientation + '_bg'
        assert scene['focus'] == [{'heroClass': key, 'x': val[0], 'y': val[1], 'zoom': val[2], 'screenX': val[3]} for key, val in design[orientation]['focus'].items()]
        for h in scene['heroes']:
            expected = design[orientation]['heroes'][h['artId']]
            for kind in ('cut', 'patch'):
                assert [h[kind][k] for k in ('x', 'y', 'width', 'height')] == [expected[kind][k] for k in ('x', 'y', 'w', 'h')]
            assert [h['headX'], h['headY']] == expected['head']
            assert [h['feetX'], h['feetY'], h['cheerHeight']] == expected['cheer']
            assert len(base64.b64decode(h['cut']['hitMask'])) == 512
            p = DEST / (h['cut']['resource'].split('/')[-1] + '.png')
            assert hit_mask(Image.open(p)) == h['cut']['hitMask']
    art = json.loads((DEST / 'art.json').read_text())
    assert len(art['poses']) == 6 and len(art['crests']) == 3
    for poses in art['poses']:
        for kind in ('cheer', 'fight'):
            p = poses[kind]
            assert len(base64.b64decode(p['hitMask'])) == 512
            assert all(0 <= p[k] <= 1 for k in ('uvX', 'uvY', 'uvWidth', 'uvHeight'))
            texture = Image.open(DEST / (p['resource'].split('/')[-1] + '.png'))
            bounds = Image.fromarray((np.array(texture.getchannel('A')) > ALPHA_THRESHOLD).astype('uint8') * 255).getbbox()
            assert [p['width'], p['height']] == [bounds[2] - bounds[0], bounds[3] - bounds[1]]
            assert hit_mask(texture.crop(bounds)) == p['hitMask']
    for crest in art['crests']:
        assert (DEST / (crest['resource'].split('/')[-1] + '.png')).is_file()
    assert not list(DEST.glob('clean_*.png')), 'Whole clean plates must not ship in Resources'
    print(json.dumps({'result': 'pass', 'originalHashes': len(receipt['originals']), 'runtimePngs': 41, 'sceneHeroes': 12,
                      'gpuOnlyHitMasks': 24, 'runtimeBytes': receipt['runtimePngBytes'], 'unityExecuted': False}))


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--layers', type=Path, default=ROOT / 'validation/character-plaza-art/native')
    parser.add_argument('--from-webp', action='store_true')
    parser.add_argument('--verify', action='store_true')
    args = parser.parse_args()
    args.layers = args.layers.resolve()
    if not args.verify:
        import_art(args)
    verify()
