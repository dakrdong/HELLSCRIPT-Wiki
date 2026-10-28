"""Accept a complete approved art import without rewriting historical provenance."""
import json
import re

RECEIPT = 'tools/casual_art_adoption_receipt_20261015.json'
EXPECTED_COUNT = 604  # Exact root-approved import unit; runtime/release approval remains separate.
ADOPTED_SOURCE_STATUS = '개발 적용 · 출시 승인 별도 / Development; release approval separate'
CLASS_GLYPH_PATHS = {
    18: 'Assets/HELLSCRIPT/Resources/Art/CharacterPlaza/crest_warrior.png',
    19: 'Assets/HELLSCRIPT/Resources/Art/CharacterPlaza/crest_ranger.png',
    20: 'Assets/HELLSCRIPT/Resources/Art/CharacterPlaza/crest_mage.png',
}
CLASS_GLYPH_GAME_UI = 'Assets/HELLSCRIPT/Runtime/Presentation/GameUI.cs'


def _index(api):
    if EXPECTED_COUNT < 1:
        raise ValueError('Current art attestation is inactive until final union freeze')
    if not (api.ROOT / RECEIPT).is_file():
        raise ValueError('Missing final current art receipt')
    cached = getattr(api, '_current_art_receipt', None)
    if cached is not None:
        return cached
    receipt = json.loads(api.read(RECEIPT))
    if receipt.get('schemaVersion') != 1 or receipt.get('adoption') != 'approved_for_import':
        raise ValueError('Current art receipt is not approved for import')
    entries = receipt.get('records', [])
    if EXPECTED_COUNT < 1 or len(entries) != EXPECTED_COUNT:
        raise ValueError('Incomplete current art receipt')
    result = {}; guids = set()
    for entry in entries:
        path = entry['path']
        if ((not path.startswith('Assets/HELLSCRIPT/Resources/Art/')
                and not re.fullmatch(r'Assets/HELLSCRIPT/Art/NpcPortraitMirrors/[a-z0-9-]+-mirror\.png', path))
                or not path.endswith('.png')
                or '\\' in path or '..' in path.split('/') or path in result):
            raise ValueError('Invalid or duplicate approved art path: ' + path)
        if (not re.fullmatch('[0-9a-f]{64}', entry.get('sha256', ''))
                or entry['sha256'] != entry.get('approvalSha256')
                or not re.fullmatch('[0-9a-f]{32}', entry.get('guid') or '')
                or not re.fullmatch('[0-9a-f]{64}', entry.get('meta_sha256') or '')):
            raise ValueError('Incomplete approved art attestation: ' + path)
        if entry['guid'] in guids:
            raise ValueError('Duplicate approved art GUID: ' + path)
        previous=entry.get('previous_sha256')
        if (previous is None and not entry.get('newAssetEvidence')) or (previous is not None
                and not re.fullmatch('[0-9a-f]{64}', previous)):
            raise ValueError('Missing approved art previous/new-source evidence: ' + path)
        guids.add(entry['guid'])
        target = api.ROOT / path
        if not target.resolve().is_relative_to(api.ROOT.resolve()):
            raise ValueError('Approved art target escapes repository: ' + path)
        if not target.is_file():
            raise ValueError('Missing approved art target: ' + path)
        raw = target.read_bytes()
        if api.digest(raw) != entry['sha256'] or len(raw) != entry['bytes']:
            raise ValueError('Incomplete or changed approved art import: ' + path)
        meta = api.read(path + '.meta')
        guid = re.search(r'^guid: (\w+)', meta, re.M)
        if not guid or guid[1] != entry['guid'] or api.INPUTS[path + '.meta'] != entry['meta_sha256']:
            raise ValueError('Approved art metadata changed: ' + path)
        api.INPUTS[path] = entry['sha256']
        result[path] = entry
    aliases = {}
    for alias in receipt.get('pathAliases', []):
        old = alias.get('previous_path', ''); current = alias.get('path')
        if (not old.startswith('Assets/HELLSCRIPT/Resources/Art/') or not old.endswith('.png')
                or '\\' in old or '..' in old.split('/') or old in aliases or current not in result
                or not re.fullmatch('[0-9a-f]{64}', alias.get('previous_sha256', ''))):
            raise ValueError('Invalid or duplicate current art path alias: ' + old)
        aliases[old] = alias
    # Cache only after the complete unit and every existing/new GUID/alias is verified.
    api._current_art_path_aliases = aliases
    api._current_art_manifest = receipt
    api._current_art_receipt = result
    return result


def expected(api, path, raw, historical_sha):
    """Return new QA only for an exact attested replacement; old guards still run."""
    entry = _index(api).get(path)
    if entry is None:
        return None
    prior_matches = entry.get('previous_sha256') == historical_sha or any(
        a['path'] == path and a['previous_sha256'] == historical_sha
        for a in getattr(api, '_current_art_path_aliases', {}).values())
    if not prior_matches or api.digest(raw) != entry['sha256']:
        raise ValueError('Approved art historical/current source mismatch: ' + path)
    return dict(entry, sourceBytesUnchanged=True)


def decorate(api, rows):
    index = _index(api)
    for row in rows:
        entry = index.get(row.get('assetPath'))
        if entry is None or row.get('source', {}).get('path') == RECEIPT:
            continue
        if entry.get('summary'):row['summary'] = entry['summary']
        fields = row['fields']
        # Preserve the previous collector's fields as explicit historical provenance.
        historical = dict(fields)
        fields['기존 수집기 필드 / Previous collector fields'] = json.dumps(historical, ensure_ascii=False, sort_keys=True)
        if entry['previous_sha256'] is not None:
            fields['이전 원본 SHA-256 / Previous source'] = entry['previous_sha256']
        else:
            fields['신규 원화 / New source'] = '이전 원본 없음; 최종 수입 근거 참조 / No previous source; see final import evidence'
        fields['현재 원화 근거 / Current artwork attestation'] = RECEIPT
        fields['SHA-256'] = entry['sha256']
        fields['Unity GUID'] = entry['guid']
        fields['현재 원화 ID / Current artwork ID'] = entry['id']
        fields['현재 원화 승인 / Current artwork approval'] = '별도 승인 원화 수입 영수증 / Separately approved source import; release approval remains separate'
        if row.get('status') == '임시 사용':
            fields['이전 등록 상태 / Previous lifecycle status'] = row['status']
            row['status'] = ADOPTED_SOURCE_STATUS
        for key in ('출처 확인', '제작 방법', '제작 방법 / Surface'):
            if key in fields:
                fields[key] = entry.get('method', '현재 런타임 PNG 바이트를 검증; 이전 생성 근거는 별도 보존 / Current runtime PNG bytes verified; previous generation provenance preserved separately')
        for key in ('해상도', '해상도 / Size'):
            if key in fields:
                fields[key] = ' × '.join(map(str, entry['size']))
        for key in ('색상 모드', '색상 모드 / Mode'):
            if key in fields:
                fields[key] = entry['mode']
        for key in ('완전 투명 픽셀', '완전 투명 픽셀 / Transparent pixels'):
            if key in fields:
                fields[key] = entry['transparentPixels']
        if '알파 범위' in fields:
            fields['알파 범위'] = str(entry['alphaExtrema'])
        # Current source is the new attestation; old source remains linked unchanged.
        refs = row.setdefault('refs', [])
        old_source = row.get('source')
        if old_source and old_source not in refs:
            refs.append(old_source)
        row['source'] = api.source_ref(RECEIPT)
    return rows


def resolve_path(api, historical_path):
    """Follow only a fully attested runtime path migration; do not fetch old preview bytes."""
    _index(api)
    alias = getattr(api, '_current_art_path_aliases', {}).get(historical_path)
    return alias['path'] if alias else historical_path



def _class_glyph_code_mask(source):
    """Keep byte offsets/newlines while masking C# comments and literals."""
    # Based on the existing UI guard's literal/comment grammar, with fail-closed input.
    token = re.compile(
        rb'//[^\r\n]*|/\*[\s\S]*?\*/|(?:\$@|@\$|@)"(?:[^"]|"")*"'
        rb'|\$?"(?:\\[^\r\n]|[^"\\\r\n])*"|\'(?:\\[^\r\n]|[^\'\\\r\n])*\'')
    masked = bytearray(source); at = 0
    while at < len(source):
        if source.startswith((b'"""', b'$"""'), at):
            raise ValueError('Ambiguous C# source: raw string literal')
        match = token.match(source, at)
        if match:
            raw = match.group()
            if raw.startswith((b'$"', b'$@"', b'@$"')):
                depth = 0
                for brace in re.findall(rb'\{\{|\}\}|[{}]', raw):
                    if len(brace) == 2:
                        continue
                    depth += 1 if brace == b'{' else -1
                    if depth < 0:
                        break
                if depth:
                    raise ValueError('Ambiguous C# source: unsupported interpolation')
            masked[at:match.end()] = re.sub(rb'[^\r\n]', b' ', raw)
            at = match.end()
        else:
            if (source[at] in (34, 39) or source.startswith(
                    (b'/*', b'@"', b'$"', b'$@"', b'@$"'), at)):
                raise ValueError('Ambiguous C# source: unfinished comment or literal')
            at += 1
    code = bytes(masked)
    if re.search(rb'(?m)^[ \t]*#', code):
        raise ValueError('Ambiguous C# source: preprocessor directive')
    return code


def class_glyph_source_reuse(api, baseline, current):
    """Prove complete code declarations before explicit source-pin renewal.

    Unsupported lexical input fails closed. This does not approve art or relax
    the strict seven-file current-source guard.
    """
    declarations = {
        'Rect': rb'RectTransform Rect\(string name,Transform parent\)',
        'Place': rb'void Place\(RectTransform r,float x,float y,float w,float h\)',
        'Icon': rb'void Icon\(Transform parent,int index,float x,float y,float size\)',
    }
    def complete(source, code, name, declaration):
        members = re.findall(rb'\b(?:RectTransform|void)\s+' + name.encode() + rb'\s*\(', code)
        starts = list(re.finditer(rb'^        ' + declaration + rb'\n        \{', code, re.M))
        if len(members) != 1 or len(starts) != 1:
            raise ValueError('Missing or ambiguous committed class3 method: ' + name)
        start = starts[0].start(); end = starts[0].end(); depth = 1
        while end < len(code) and depth:
            if code[end] == 123:
                depth += 1
            elif code[end] == 125:
                depth -= 1
            end += 1
        if depth:
            raise ValueError('Ambiguous C# source: unfinished method ' + name)
        return source[start:end], source[:start].count(b'\n') + 1

    before_code = _class_glyph_code_mask(baseline)
    after_code = _class_glyph_code_mask(current)
    proof = []
    for name, declaration in declarations.items():
        before, baseline_line = complete(baseline, before_code, name, declaration)
        raw, current_line = complete(current, after_code, name, declaration)
        if before != raw:
            raise ValueError('Changed committed class3 method: ' + name)
        proof.append(dict(name=name, sha256=api.digest(raw), bytes=len(raw),
                          baselineLine=baseline_line, currentLine=current_line,
                          completeMethodByteEquality=True, codeOnlyDeclaration=True))
    return proof


def _class_glyph_routes(api, receipt, approved):
    """Fresh-check exactly three committed crest reuses; never attest external art."""
    rows=receipt.get('classGlyphAliases',[]);freeze=receipt.get('classGlyphSourceFreeze',{})
    if (not isinstance(rows,list) or len(rows)!=3
            or any(not isinstance(r,dict) or type(r.get('index')) is not int for r in rows)
            or {r['index'] for r in rows}!=set(CLASS_GLYPH_PATHS)):
        raise ValueError('Incomplete exact committed class3 routes')
    if (not isinstance(freeze,dict) or freeze.get('schemaVersion')!=1 or freeze.get('state')!='VERIFIED_EXISTING_COMMITTED_CLASS3_REUSE'
            or not re.fullmatch('[0-9a-f]{64}',freeze.get('sourceFreezeManifestSha256') or '')
            or not re.fullmatch('[0-9a-f]{40}',freeze.get('sourceHead') or '')):
        raise ValueError('Missing committed class3 source freeze')
    expected=set(CLASS_GLYPH_PATHS.values())
    expected|={p+'.meta' for p in CLASS_GLYPH_PATHS.values()};expected.add(CLASS_GLYPH_GAME_UI)
    files=freeze.get('files',[])
    if (not isinstance(files,list) or len(files)!=7
            or any(not isinstance(r,dict) for r in files)
            or {r.get('path') for r in files}!=expected
            or any(not re.fullmatch('[0-9a-f]{64}',r.get('sha256') or '') for r in files)):
        raise ValueError('Incomplete exact committed class3 source freeze files')
    pins={r['path']:r['sha256'] for r in files};actual={}
    for path,expected_hash in pins.items():
        target=api.ROOT/path
        if not target.is_file() or target.resolve()!=api.ROOT.resolve()/path:
            raise ValueError('Missing or unsafe committed class3 source: '+path)
        raw=target.read_bytes()
        if api.digest(raw)!=expected_hash:
            raise ValueError('Changed committed class3 frozen source: '+path)
        api.INPUTS[path]=expected_hash;actual[path]=raw
    guids={entry['guid'] for entry in approved.values()};routes={}
    for row in rows:
        path=CLASS_GLYPH_PATHS[row['index']]
        if (row.get('path')!=path or path in approved
                or row.get('sha256')!=pins[path] or row.get('meta_sha256')!=pins[path+'.meta']
                or type(row.get('bytes')) is not int or row['bytes']!=len(actual[path])
                or not re.fullmatch('[0-9a-f]{32}',row.get('guid') or '') or row['guid'] in guids
                or re.findall(r'^guid: ([0-9a-f]{32})\s*$',actual[path+'.meta'].decode('utf-8'),re.M)!=[row['guid']]):
            raise ValueError('Unattested exact committed class3 identity: '+path)
        guids.add(row['guid']);routes[row['index']]=path
    return routes


def apply_legacy_images(api, databases):
    """Move live legacy18/class3 DB previews off SkillAtlas without altering content IDs."""
    index = _index(api)
    if not index:
        return False
    receipt = api._current_art_manifest
    routes = {}
    for field, expected in [('legacyActiveAliases', set(range(18)))]:
        rows = receipt.get(field, [])
        if len(rows) != len(expected) or {r.get('index') for r in rows} != expected:
            raise ValueError('Incomplete live atlas replacement routes: ' + field)
        for row in rows:
            if row.get('path') not in index or row['index'] in routes:
                raise ValueError('Unattested live atlas replacement: ' + field)
            routes[row['index']] = row['path']
    routes.update(_class_glyph_routes(api, receipt, index))
    for database in databases:
        if database['id'] not in ('skills', 'heroes'):
            continue
        for row in database['rows']:
            image = row.get('image', {})
            if image.get('file') != 'SkillAtlas.png':
                continue
            cell = image.get('cell')
            if cell not in routes:
                raise ValueError('Unmapped live atlas cell: ' + str(cell))
            path = routes[cell]
            row['image'] = {'file': path.removeprefix('Assets/HELLSCRIPT/Resources/Art/')}
            row['assetPath'] = path
    return True


def attach_current_skill_images(api, databases, resources):
    """Connect all144 ability rows to the exact current PNG and existing resource record."""
    index = _index(api)
    if not index:
        return
    skills = {entry['id']: entry for entry in index.values() if entry.get('family') == 'current-skill'}
    table = next((d for d in databases if d['id'] == 'class-abilities'), None)
    if table is None:
        raise ValueError('Missing current class ability table')
    rows = table['rows']
    if len(skills) != 144 or {r['id'] for r in rows} != set(skills):
        raise ValueError('Incomplete current144 ability/image identity map')
    by_path = {}
    for resource in resources:
        if resource.get('image') and resource.get('assetPath'):
            by_path.setdefault(resource['assetPath'], resource['id'])
    if any(entry['path'] not in by_path for entry in skills.values()):
        raise ValueError('Missing current ability resource link')
    for row in rows:
        entry = skills[row['id']]; path = entry['path']
        row['image'] = {'file': path.removeprefix('Assets/HELLSCRIPT/Resources/Art/')}
        row['assetPath'] = path; row['resource'] = by_path[path]
