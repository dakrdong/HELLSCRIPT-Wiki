"""Exact withdrawn wiki attachments; historical text and current artwork stay intact."""
from functools import lru_cache
import json
from pathlib import Path, PurePosixPath
from urllib.parse import unquote, urlsplit

REGISTRY = 'tools/wiki_retired_art_sources.json'
ADDITIONAL_REGISTRY = 'tools/wiki_pre_casual_art_retirement.json'
ART = 'Assets/HELLSCRIPT/Resources/Art/'
LEGACY_ROOT = 'Docs/Implementation/D4LegacyArchive20261013/'
PROTOTYPE_PREFIXES = ('Prototypes/SkillTree/', LEGACY_ROOT + 'Prototypes/SkillTree/')
PAGE_SOURCES = frozenset((
    LEGACY_ROOT + 'Prototypes/SkillTree/Skill_Tree_Preview.md',
))
PAGE_IDS = frozenset(('skill-tree-preview',))
ATLAS_CELLS = frozenset(('asset-skillatlas', 'legacy-cell-21', 'legacy-cell-22', 'legacy-cell-23'))
# These two visual attachments embed withdrawn artwork; keep their source code in Git.
EMBEDDED_ART_ATTACHMENTS = frozenset((
    'Prototypes/HuntEdict/HELLSCRIPT-HuntEdict.html',
    LEGACY_ROOT + 'Prototypes/HuntEdict/HELLSCRIPT-HuntEdict.html',
    'Prototypes/RuneBoard/HELLSCRIPT-RuneBoard.html',
    LEGACY_ROOT + 'Prototypes/RuneBoard/HELLSCRIPT-RuneBoard.html',
))
OLD_ATTACHMENT_PREFIXES = (
    'Docs/Art/Gems/2026-09-23/',
    'Docs/Design/GlobalHUD/Resources/v1/',
)


@lru_cache(maxsize=8)
def _registered_paths(root, modified, size):
    manifest = json.loads((Path(root) / REGISTRY).read_text())
    if type(manifest.get('schemaVersion')) is not int or manifest['schemaVersion'] != 1 or not isinstance(manifest.get('files'), list):
        raise ValueError('Invalid retired wiki art registry')
    paths = []
    for entry in manifest['files']:
        path = entry['path']
        if not path.endswith(('.png', '.png.meta')):
            continue
        if not path.startswith(ART) or PurePosixPath(path).as_posix() != path or '..' in PurePosixPath(path).parts or '\\' in path:
            raise ValueError('Unsafe retired wiki art path: ' + path)
        paths.append(path)
    if len(paths) != len(set(paths)):
        raise ValueError('Duplicate retired wiki art path')
    return frozenset(paths)


@lru_cache(maxsize=8)
def _additional_paths(root, modified, size):
    manifest = json.loads((Path(root) / ADDITIONAL_REGISTRY).read_text())
    if (set(manifest) != {'schemaVersion', 'paths'} or type(manifest['schemaVersion']) is not int
            or manifest['schemaVersion'] != 1 or not isinstance(manifest['paths'], list)):
        raise ValueError('Invalid additional retired wiki art paths')
    paths = manifest['paths']
    for path in paths:
        if (not isinstance(path, str) or not path.endswith(('.png', '.png.meta'))
                or PurePosixPath(path).is_absolute() or PurePosixPath(path).as_posix() != path
                or '..' in PurePosixPath(path).parts or '\\' in path):
            raise ValueError('Unsafe additional retired wiki art path')
    if len(paths) != len(set(paths)):
        raise ValueError('Duplicate additional retired wiki art path')
    return frozenset(paths)


def retired_path(root, path):
    path = unquote(str(path))
    if path.startswith('sources/'):
        path = path[len('sources/'):]
    if path in PAGE_SOURCES or path in EMBEDDED_ART_ATTACHMENTS or path.startswith(PROTOTYPE_PREFIXES):
        return True
    # The withdrawn prototype's compatibility copies never become current game art.
    if path.startswith(LEGACY_ROOT + ART + 'ClassSkillIcons/'):
        return True
    for prefix in OLD_ATTACHMENT_PREFIXES:
        suffixes = ('.png', '.png.meta', '.svg') if prefix.endswith('/v1/') else ('.png', '.png.meta')
        if path.startswith(prefix) and path.endswith(suffixes):
            return True
    if not path.endswith(('.png', '.png.meta')):
        return False
    root = Path(root).resolve()
    registered = False
    if path.startswith(ART):
        info = (root / REGISTRY).stat()
        registered = path in _registered_paths(str(root), info.st_mtime_ns, info.st_size)
    additional = root / ADDITIONAL_REGISTRY
    if additional.is_file():
        info = additional.stat()
        return path in _additional_paths(str(root), info.st_mtime_ns, info.st_size) or registered
    return registered


def retired_page(page):
    return page.get('id') in PAGE_IDS or page.get('source') in PAGE_SOURCES


def retired_target(root, target):
    parsed = urlsplit(target)
    if target.startswith('#/page/'):
        return parsed.fragment.split('/page/', 1)[1].split('?', 1)[0] in PAGE_IDS
    path = unquote(parsed.path)
    parts = path.strip('/').split('/')
    if parsed.hostname == 'raw.githubusercontent.com' and len(parts) >= 4 and parts[:2] in (
            ['dakrdong', 'HELLSCRIPT'], ['dakrdong', 'HELLSCRIPT-Wiki']):
        path = '/'.join(parts[3:])
    elif parsed.hostname == 'github.com' and len(parts) >= 5 and parts[:2] in (
            ['dakrdong', 'HELLSCRIPT'], ['dakrdong', 'HELLSCRIPT-Wiki']) and parts[2] in ('blob', 'raw'):
        path = '/'.join(parts[4:])
    elif parsed.hostname == 'dakrdong.github.io' and parts[:1] == ['HELLSCRIPT-Wiki']:
        path = '/'.join(parts[1:])
    elif parsed.scheme:
        return False
    return retired_path(root, path)


def retained_rows(root, rows):
    return [row for row in rows if row.get('id') not in ATLAS_CELLS
            and not retired_path(root, row.get('assetPath', ''))
            and not retired_path(root, row.get('source', {}).get('path', ''))]


def retained_databases(root, databases):
    """Keep content identities while removing withdrawn pictures and their download links."""
    removed = {row['id'] for table in databases if table['id'] == 'resources'
               for row in table['rows'] if not retained_rows(root, [row])}
    tables = []
    for table in databases:
        rows = retained_rows(root, table['rows']) if table['id'] == 'resources' else table['rows']
        kept = []
        for row in rows:
            if retired_path(root, row.get('source', {}).get('path', '')):
                raise ValueError('Content requires a non-retired factual source: ' + row['id'])
            clean = dict(row)
            image = row.get('image')
            if image and retired_path(root, row.get('assetPath') or ART + image['file']):
                clean.pop('image')
            if retired_path(root, row.get('assetPath', '')):
                clean.pop('assetPath')
            if 'refs' in row:
                clean['refs'] = [ref for ref in row['refs'] if not retired_path(root, ref['path'])]
            if 'related' in row:
                clean['related'] = [ident for ident in row['related'] if ident not in PAGE_IDS]
            if row.get('resource') in removed:
                clean.pop('resource')
            kept.append(clean)
        tables.append(dict(table, rows=kept))
    return tables
