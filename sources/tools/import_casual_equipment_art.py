#!/usr/bin/env python3
"""Adopt the approved 359 PNG snapshot without Unity or changes to original assets.

--dry-run validates every source and prints the planned additions.
--import validates all inputs/targets first, then creates only absent, owned files.
--verify is portable: checks installed bytes/GUIDs/catalogue/provider using the receipt.
The pinned retirement manifest permits only the approved legacy PNG/meta pairs to be absent.
Add --verify-source to also recheck the local approval ledgers and original PNGs.
Existing differing files, changed approval inputs, symlinks and unknown outputs fail closed.
"""
import argparse
import csv
import hashlib
import io
import json
import os
import re
import shutil
import struct
import sys
import tempfile
import uuid
from collections import Counter
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
INDEX = Path('/Users/t8g-2410-pn-005/Documents/MyProject/HELLSCRIPT/HELLSCRIPT/Docs/Art/Art_Transition_20261014_Asset_Index.json')
DEST = Path('Assets/HELLSCRIPT/Resources/Art/Casual/Equipment')
RESOURCE = 'Art/Casual/Equipment/'
PROVIDER = Path('Assets/HELLSCRIPT/Runtime/Presentation/CasualEquipmentArt.cs')
RECEIPT = Path('tools/casual_equipment_art_receipt.json')
RETIREMENT = Path('tools/casual_resource_retirement_20261015.json')
RETIREMENT_SHA = '13191c3a15999a9bdd169c93c943ae6efe45b33fd0e9a659a9cacb6eea226e75'
CSV_SHA = '4a45ac1e9d7aa279ac59340dba176b88345b97083666a36ec3a2e32401180588'
DIRECTOR_SHA = '8dac73a7b7161dee5a9d38e2cd9b1db94d0d5d33dc338befd57992f0bde76403'
QA_SHA = '619e9fb629ebb03ad0ea78f7bb6f172fb5b0a24c39b59a1798324e33e4d74f4b'
APPROVAL = 'casual-equipment-20261014-359:qa-sha256:' + QA_SHA
GROUPS = {'base': 59, 'legendary': 141, 'set_piece': 129, 'set_emblem': 30}


def require(condition, message):
    if not condition:
        raise ValueError(message)


def sha(data):
    return hashlib.sha256(data).hexdigest()


def checked_path(relative):
    relative = Path(relative)
    require(not relative.is_absolute() and '..' not in relative.parts, f'Unsafe target: {relative}')
    path = ROOT / relative
    require(path.resolve() == path, f'Symlink target refused: {relative}')
    return path


def read_pinned(path, expected):
    data = path.read_bytes()
    require(sha(data) == expected, f'Approval input SHA changed: {path}')
    return data


def meta_path(path):
    return Path(str(path) + '.meta')


def guid_of(path):
    text = path.read_text()
    match = re.search(r'^guid: ([0-9a-f]{32})$', text, re.M)
    require(match is not None, f'Invalid GUID: {path}')
    return match.group(1)


def catalogue_check(records):
    core = (ROOT / 'Assets/HELLSCRIPT/Runtime/Core/Itemization.cs').read_text()
    data = json.loads((ROOT / 'Assets/HELLSCRIPT/Resources/Data/ClassSkills.json').read_text())
    powers = json.loads((ROOT / 'Assets/HELLSCRIPT/Resources/Data/LegendaryPowers.json').read_text())
    bases = set(re.findall(r'new ItemBaseDefinition\("([^"]+)"', core))
    sets = set(re.findall(r'new SetDefinition\("([^"]+)"', core))
    unique = set(re.findall(r'new UniqueItemDefinition\("([^"]+)"', core))
    unique |= {p['id'] for p in powers['powers']} | {g['id'] for g in data['gear']}
    unique |= {s + str(n) for s in sets for n in range(1, 5)}
    emblems = sets | {g['setId'] for g in data['gear'] if g.get('setId')}
    keys = [k for r in records if r['group'] != 'set_emblem' for k in r['catalog_ids']]
    emblem_keys = [r['id'] for r in records if r['group'] == 'set_emblem']
    require(len(keys) == len(set(keys)) == 330, 'Expected 330 distinct item keys, including B28 alias')
    require(set(keys) == bases | unique, 'Approved item IDs differ from the current catalogue')
    require(len(emblem_keys) == len(set(emblem_keys)) == 30 and set(emblem_keys) == emblems,
            'Approved set emblems differ from the current catalogue')
    require(data['playerEnabled'] is False, 'Additional catalogue playerEnabled changed; do not unlock it here')
    require([r['id'] for r in records if r['catalog_ids'] == ['B28', 'B32']] == ['B32'], 'B28/B32 alias changed')


def validate_records(records):
    require(len(records) == 359 and len({r['id'] for r in records}) == 359, 'Expected 359 unique masters')
    require(Counter(r['group'] for r in records) == GROUPS, 'Approved equipment group counts changed')
    require(len({r['source_sha256'] for r in records}) == 359, 'Unexpected duplicate equipment PNG')
    for r in records:
        require(re.fullmatch(r'[A-Z0-9_]+', r['id']), f'Invalid ID: {r["id"]}')
        require(r['destination'] == str(DEST / (r['id'] + '.png')), 'Unexpected destination')
        require(r['resource'] == RESOURCE + r['id'], 'Unexpected Resources path')
        require(r['source_sha256'] == r['destination_sha256'], 'Source/destination SHA mismatch in receipt')
    catalogue_check(records)


def source_snapshot(index_path):
    from PIL import Image  # Only source inspection needs Pillow; portable --verify uses stdlib.
    index_bytes = index_path.read_bytes()
    group = json.loads(index_bytes)['groups']['equipment']
    source_root = Path(group['local_root'])
    csv_path = source_root / 'delivery/HELLSCRIPT-equipment-359.csv'
    director_path = Path(group['director_ledger'])
    qa_path = Path(group['primary_file_ledger'])
    csv_bytes = read_pinned(csv_path, CSV_SHA)
    director = json.loads(read_pinned(director_path, DIRECTOR_SHA))['groups']['equipment']
    qa = json.loads(read_pinned(qa_path, QA_SHA))
    passed = {r['asset_id']: r for r in director['passed']}
    checked = {r['id']: r for r in qa['records'] if r['group'] == 'equipment'}
    require(len(passed) == len(checked) == 359 and not director['needs_correction'], 'Incomplete equipment approval')
    records, payloads = [], {}
    for row in csv.DictReader(io.StringIO(csv_bytes.decode('utf-8-sig'))):
        ident = row['id']
        require(row['relative_png'] == f'icons/{ident}.png', f'Unexpected source path: {ident}')
        source = source_root / row['relative_png']
        require(str(source) == passed[ident]['path'] == checked[ident]['path'], f'Approval path differs: {ident}')
        require(passed[ident]['director_verdict'] == 'pass', f'Not approved: {ident}')
        data = source.read_bytes()
        digest = sha(data)
        require(digest == row['sha256'] == passed[ident]['sha256'] == checked[ident]['sha256']
                == checked[ident]['expected_sha256'], f'Approval SHA mismatch: {ident}')
        with Image.open(io.BytesIO(data)) as image:
            image.load()
            require(image.format == 'PNG' and image.mode == 'RGBA' and image.size == (512, 512)
                    and image.getchannel('A').getextrema() == (0, 255), f'Unexpected PNG/alpha: {ident}')
        records.append(dict(id=ident, group=row['group'], catalog_ids=row['catalog_ids'].split('|'),
                            source_status=row['source_status'], source_path=str(source), source_sha256=digest,
                            destination=str(DEST / (ident + '.png')), destination_sha256=digest,
                            resource=RESOURCE + ident, bytes=len(data), width=512, height=512))
        payloads[str(DEST / (ident + '.png'))] = data
    records.sort(key=lambda r: r['id'])
    validate_records(records)
    sources = dict(asset_index=dict(path=str(index_path), sha256=sha(index_bytes)),
                   csv=dict(path=str(csv_path), sha256=CSV_SHA),
                   director=dict(path=str(director_path), sha256=DIRECTOR_SHA),
                   qa=dict(path=str(qa_path), sha256=QA_SHA))
    return records, payloads, sources


def provider_source(records):
    def literals(group):
        ids = [r['id'] for r in records if (r['group'] == 'set_emblem') == group]
        chunks = [' '.join(ids[i:i + 10]) for i in range(0, len(ids), 10)]
        return '\n                '.join('"' + s + (' " +' if i < len(chunks) - 1 else '"')
                                           for i, s in enumerate(chunks))
    return '''// Generated by tools/import_casual_equipment_art.py from the approved 359-master receipt.
// Shared default artwork; catalogue, unlock and save state remain unchanged.
using System;
using System.Collections.Generic;

namespace Hellscript
{
    public static class CasualEquipmentArt
    {
        public const string ApprovalReceipt="''' + APPROVAL + '''";
        const string Root="''' + RESOURCE + '''";
        public static EquipmentArtProvider Provider {get;}=Create();
        static EquipmentArtProvider Create()
        {
            var items=new Dictionary<string,string>(StringComparer.Ordinal);
            Add(items,
                ''' + literals(False) + ''');
            items.Add("B28",Root+"B32");
            var emblems=new Dictionary<string,string>(StringComparer.Ordinal);
            Add(emblems,
                ''' + literals(True) + ''');
            return new EquipmentArtProvider(ApprovalReceipt,items,emblems);
        }
        static void Add(Dictionary<string,string> map,string ids)
        {foreach(string id in ids.Split(' '))map.Add(id,Root+id);}
    }
}
'''


def texture_meta(guid):
    text = f'''fileFormatVersion: 2
guid: {guid}
TextureImporter:
  internalIDToNameTable: []
  externalObjects: {{}}
  serializedVersion: 13
  mipmaps:
    enableMipMap: 0
    sRGBTexture: 1
  isReadable: 0
  streamingMipmaps: 0
  maxTextureSize: 512
  textureSettings:
    serializedVersion: 2
    filterMode: 1
    aniso: 1
    mipBias: 0
    wrapU: 1
    wrapV: 1
    wrapW: 1
  nPOTScale: 0
  spriteMode: 0
  alphaUsage: 1
  alphaIsTransparency: 1
  textureType: 0
  textureShape: 1
  platformSettings:
'''
    for target, fmt, override in [('DefaultTexturePlatform', -1, 0), ('Standalone', 25, 1), ('Android', 50, 1), ('WebGL', 47, 1)]:
        text += f'''  - serializedVersion: 4
    buildTarget: {target}
    maxTextureSize: 512
    resizeAlgorithm: 0
    textureFormat: {fmt}
    textureCompression: {override}
    compressionQuality: {100 if target in ('Standalone', 'WebGL') else 50}
    crunchedCompression: 0
    allowsAlphaSplitting: 0
    overridden: {override}
    ignorePlatformSupport: 0
    androidETC2FallbackOverride: 0
    forceMaximumCompressionQuality_BC6H_BC7: 0
'''
    return text + '  userData:\n  assetBundleName:\n  assetBundleVariant:\n'


def validate_texture_meta(path):
    text = path.read_text()
    for indent, key, value in [(2, 'textureType', 0), (2, 'spriteMode', 0), (2, 'alphaUsage', 1),
                             (2, 'alphaIsTransparency', 1), (2, 'nPOTScale', 0), (2, 'isReadable', 0),
                             (4, 'enableMipMap', 0), (4, 'sRGBTexture', 1), (4, 'filterMode', 1),
                             (4, 'wrapU', 1), (4, 'wrapV', 1), (4, 'wrapW', 1)]:
        require(re.search(r'^' + ' ' * indent + key + ': ' + str(value) + r'$', text, re.M), f'Unexpected {key}: {path}')
    for target, fmt, override in [('DefaultTexturePlatform', -1, 0), ('Standalone', 25, 1), ('Android', 50, 1), ('WebGL', 47, 1)]:
        block = re.search(r'^    buildTarget: ' + target + r'\n(.*?)(?=^  [-a-zA-Z]|\Z)', text, re.M | re.S)
        require(block is not None, f'Missing platform {target}: {path}')
        for key, value in [('maxTextureSize', 512), ('textureFormat', fmt), ('overridden', override)]:
            require(re.search(r'^    ' + key + ': ' + str(value) + r'$', block[1], re.M), f'Unexpected {target}/{key}: {path}')


def legacy_snapshot(records):
    paths = set()
    for r in records:
        ident = r['id']
        if r['group'] == 'base':
            tail = 'EquipmentAtlas.png' if int(ident[1:]) <= 24 else f'EquipmentVariants/{ident}.png'
        else:
            tail = ('ClassLegendaryIcons/' if r['group'] == 'legendary' else 'ClassSetIcons/') + ident + '.png'
        paths.add('Assets/HELLSCRIPT/Resources/Art/' + tail)
    return [dict(path=p, sha256=sha((ROOT / p).read_bytes()), guid=guid_of(meta_path(ROOT / p))) for p in sorted(paths)]


def known_guids():
    owners = {}
    for meta in (ROOT / 'Assets').rglob('*.meta'):
        if meta.is_file():
            match = re.search(r'^guid: ([0-9a-f]{32})$', meta.read_text(), re.M)
            if match:
                owners.setdefault(match[1], []).append(str(meta.relative_to(ROOT)))
    return owners


def additions_check(records):
    allowed = {r['id'] + suffix for r in records for suffix in ('.png', '.png.meta')}
    path = checked_path(DEST)
    if path.exists():
        require(path.is_dir(), f'Expected directory: {path}')
        require(not {p.name for p in path.iterdir()} - allowed, 'Unknown files in the owned equipment directory; preserving them')


def verify_legacy_retirement(receipt):
    """Only pinned PNG/meta pairs may be absent; replacements always remain exact."""
    manifest = json.loads(read_pinned(checked_path(RETIREMENT), RETIREMENT_SHA))
    require(manifest['schema_version'] == 1, 'Unknown retirement manifest schema')
    require(manifest['equipment_approval_receipt'] == receipt['approval_receipt'] == APPROVAL,
            'Retirement approval differs from the original receipt')
    original = read_pinned(checked_path(RECEIPT), manifest['equipment_receipt_sha256'])
    require(json.loads(original) == receipt, 'Original import receipt changed')
    legacy = {r['path']: r for r in receipt['preserved_legacy']}
    atlas = 'Assets/HELLSCRIPT/Resources/Art/EquipmentAtlas.png'
    equipment = set(legacy) - {atlas}
    require(len(equipment) == 335 and atlas in legacy, 'Original legacy inventory changed')
    jeweler = {f'Assets/HELLSCRIPT/Resources/Art/Jeweler/{folder}/{kind}-G{gem:02d}-{tier}.png'
               for folder, kind in [('Gems', 'gem'), ('Potions', 'elixir')]
               for gem in range(1, 7) for tier in range(1, 7)}
    entries = manifest['entries']
    paths = [r['old']['path'] for r in entries]
    require(len(paths) == len(set(paths)) == 407 and set(paths) == equipment | jeweler,
            'Retirement manifest must contain exactly the approved 407 legacy images')

    def verify_asset(row):
        path = checked_path(row['path'])
        require(row['meta_path'] == row['path'] + '.meta', 'Unexpected metadata path')
        meta = checked_path(row['meta_path'])
        data, metadata = path.read_bytes(), meta.read_bytes()
        require(len(data) == row['bytes'] and sha(data) == row['sha256'], f'Resource bytes changed: {path}')
        require(len(metadata) == row['meta_bytes'] and sha(metadata) == row['meta_sha256'], f'Resource metadata changed: {meta}')
        require(guid_of(meta) == row['guid'], f'Resource GUID changed: {meta}')

    retained = removed = 0
    for entry in entries:
        old, replacement = entry['old'], entry['replacement']
        filename = Path(old['path']).name
        target = ('Assets/HELLSCRIPT/Resources/Art/Casual/Equipment/' if old['path'] in equipment
                  else 'Assets/HELLSCRIPT/Resources/Art/JewelerWindow/') + filename
        require(entry['id'] == Path(filename).stem and replacement['path'] == target,
                'Retirement replacement must keep the approved identity')
        if old['path'] in equipment:
            require(old['sha256'] == legacy[old['path']]['sha256'] and old['guid'] == legacy[old['path']]['guid'],
                    'Retirement entry differs from original import provenance')
        require(old['meta_path'] == old['path'] + '.meta', 'Unexpected legacy metadata path')
        verify_asset(replacement)
        image, meta = checked_path(old['path']), checked_path(old['meta_path'])
        require(image.exists() == meta.exists(), f'Partial legacy PNG/meta removal: {old["path"]}')
        if image.exists():
            verify_asset(old)
            retained += 1
        else:
            removed += 1
    # The original atlas is outside retirement and remains the generic fallback.
    row = legacy[atlas]
    require(sha(checked_path(atlas).read_bytes()) == row['sha256']
            and guid_of(checked_path(atlas + '.meta')) == row['guid'], 'Original equipment atlas changed or missing')
    return dict(legacy_preserved=1 + sum(1 for r in entries if r['old']['path'] in equipment
                                       and checked_path(r['old']['path']).exists()),
                retirement_images_retained=retained, retirement_images_removed=removed,
                retirement_replacements_verified=len(entries))


def verify(receipt):
    require(receipt['approval_receipt'] == APPROVAL, 'Unexpected approval receipt')
    require(receipt['sources']['csv']['sha256'] == CSV_SHA and receipt['sources']['director']['sha256'] == DIRECTOR_SHA
            and receipt['sources']['qa']['sha256'] == QA_SHA, 'Unexpected approval hashes')
    records = receipt['records']
    validate_records(records)
    additions_check(records)
    expected_metas = {r['destination'] + '.meta' for r in records}
    expected_metas |= {str(meta_path(DEST.parent)), str(meta_path(DEST)), str(meta_path(PROVIDER))}
    require(set(receipt['guids']) == expected_metas, 'Receipt GUID inventory differs from the approved targets')
    owners = known_guids()
    for r in records:
        path = checked_path(r['destination'])
        data = path.read_bytes()
        require(sha(data) == r['source_sha256'] and len(data) == r['bytes'], f'Installed PNG differs: {path}')
        require(data[:8] == b'\x89PNG\r\n\x1a\n' and struct.unpack('>II', data[16:24]) == (512, 512), f'Invalid PNG: {path}')
        require(r['guid'] == receipt['guids'][r['destination'] + '.meta'], f'Record GUID differs: {path}')
        validate_texture_meta(meta_path(path))
    for relative, guid in receipt['guids'].items():
        path = checked_path(relative)
        require(guid_of(path) == guid and owners.get(guid) == [relative], f'GUID differs/duplicates: {relative}')
    require(checked_path(PROVIDER).read_text() == provider_source(records), 'Provider map differs from the receipt')
    legacy = verify_legacy_retirement(receipt)
    return dict(pngs=len(records), item_keys=330, emblems=30, logical_png_bytes=sum(r['bytes'] for r in records),
                guid_count=len(receipt['guids']), **legacy)


def prepare(records, payloads, sources):
    additions_check(records)
    receipt_path = checked_path(RECEIPT)
    saved = json.loads(receipt_path.read_text()) if receipt_path.exists() else None
    owners = known_guids()
    guids, outputs = {}, dict(payloads)
    outputs[str(PROVIDER)] = provider_source(records).encode()
    folders = [DEST.parent, DEST]
    for relative in folders:
        path = checked_path(relative)
        require(not path.exists() or path.is_dir(), f'Expected directory: {path}')
    for relative in [str(meta_path(p)) for p in folders] + [str(meta_path(PROVIDER))] + [r['destination'] + '.meta' for r in records]:
        path = checked_path(relative)
        if path.exists():
            guid = guid_of(path)
            require(owners.get(guid) == [relative], f'Duplicate GUID: {relative}')
        else:
            guid = saved['guids'][relative] if saved else uuid.uuid4().hex
            require(guid not in owners, f'New GUID collides: {relative}')
            owners[guid] = [relative]
        guids[relative] = guid
        if relative.endswith('.png.meta'):
            if path.exists():
                validate_texture_meta(path)
            outputs[relative] = texture_meta(guid).encode()
        elif relative.endswith('.cs.meta'):
            outputs[relative] = f'fileFormatVersion: 2\nguid: {guid}\n'.encode()
        else:
            if path.exists():
                require('folderAsset: yes' in path.read_text(), f'Expected folder meta: {relative}')
            outputs[relative] = (f'fileFormatVersion: 2\nguid: {guid}\nfolderAsset: yes\nDefaultImporter:\n'
                                 '  externalObjects: {}\n  userData:\n  assetBundleName:\n  assetBundleVariant:\n').encode()
    for r in records:
        r['guid'] = guids[r['destination'] + '.meta']
    receipt = dict(schema_version=1, approval_receipt=APPROVAL, sources=sources, records=records, guids=guids,
                   provider=str(PROVIDER), preserved_legacy=legacy_snapshot(records),
                   artifact_lifecycle=dict(owner='equipment_api_boundary', kind='source-deliverable',
                                           source_commit='cdb9c140fbf8b778bd3c0bbc0fbcc68a5d9e3fa8', created_date='2026-10-14',
                                           generation_command='python3 tools/import_casual_equipment_art.py --import',
                                           destination=str(DEST), expected_png_bytes=sum(r['bytes'] for r in records),
                                           retention='Retain with source; no temporary build/cache is created.',
                                           verification='python3 tools/import_casual_equipment_art.py --verify --verify-source',
                                           unity_validation='Not performed by this offline import tool.'))
    if saved:
        require(receipt == saved, 'Existing receipt differs; refusing to overwrite approval or GUID records')
    outputs[str(RECEIPT)] = (json.dumps(receipt, ensure_ascii=False, indent=2) + '\n').encode()
    pending = {}
    for relative, data in outputs.items():
        path = checked_path(relative)
        if path.exists():
            require(path.is_file(), f'Expected file: {relative}')
            # Unity may expand valid .meta serialization; preserve it rather than rewriting.
            if not relative.endswith('.meta'):
                require(path.read_bytes() == data, f'Existing file differs; refusing to overwrite: {relative}')
        else:
            pending[relative] = data
    return receipt, pending, len(outputs) - len(pending)


def publish_exclusive(path, data, staging):
    """Publish only complete bytes; link fails atomically if another writer created the target.

    The link is from our temporary bytes, never from an original asset. Removing the
    staging name leaves the installed asset independent (one link). A killed process
    can leave at most one staged file under tools, never a partial final-path file.
    """
    temporary = staging / uuid.uuid4().hex
    try:
        with temporary.open('xb') as stream:
            stream.write(data)
            stream.flush()
            os.fsync(stream.fileno())
        path.parent.mkdir(parents=True, exist_ok=True)
        os.link(temporary, path)
    finally:
        temporary.unlink(missing_ok=True)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    mode = parser.add_mutually_exclusive_group(required=True)
    mode.add_argument('--dry-run', action='store_true')
    mode.add_argument('--import', dest='adopt', action='store_true')
    mode.add_argument('--verify', action='store_true')
    parser.add_argument('--verify-source', action='store_true')
    parser.add_argument('--asset-index', type=Path, default=INDEX)
    args = parser.parse_args()
    if args.verify:
        receipt = json.loads(checked_path(RECEIPT).read_text())
        result = verify(receipt)
        if args.verify_source:
            records, _, sources = source_snapshot(args.asset_index)
            expected = [{k: v for k, v in r.items() if k != 'guid'} for r in receipt['records']]
            require(records == expected and sources == receipt['sources'], 'Source snapshot differs from receipt')
        print(json.dumps(dict(status='PASS', **result, source_verified=args.verify_source)))
        return
    records, payloads, sources = source_snapshot(args.asset_index)
    receipt, pending, skipped = prepare(records, payloads, sources)
    summary = dict(pngs=359, logical_png_bytes=sum(r['bytes'] for r in records), files_to_create=len(pending),
                   identical_files_skipped=skipped, destination=str(DEST), receipt=str(RECEIPT))
    if args.dry_run:
        print(json.dumps(dict(status='DRY_RUN_PASS', **summary)))
        return
    require(shutil.disk_usage(ROOT).free > sum(len(v) for v in pending.values()) + 16 * 1024 * 1024, 'Insufficient free space')
    # Write the immutable approval/lifecycle manifest first, so partial runs can resume its exact GUIDs.
    order = ([str(RECEIPT)] if str(RECEIPT) in pending else []) + sorted(k for k in pending if k != str(RECEIPT))
    if pending:
        # Same filesystem, outside Assets. Normal completion removes this one temporary directory.
        with tempfile.TemporaryDirectory(prefix='.casual-equipment-import-', dir=ROOT / 'tools') as staging:
            for relative in order:
                publish_exclusive(checked_path(relative), pending[relative], Path(staging))
    result = verify(receipt)
    print(json.dumps({'status': 'IMPORTED_PASS', **summary, **result}))


if __name__ == '__main__':
    try:
        main()
    except (ValueError, OSError, KeyError) as exc:
        print('FAIL:', exc, file=sys.stderr)
        sys.exit(1)
