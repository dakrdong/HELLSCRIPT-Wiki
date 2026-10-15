#!/usr/bin/env python3
"""Read-only Rift integration invariants and source RGB checks; no Unity proof.

Usage: python3 audit-shared-followup.py CHECKOUT [BASE] [COMMON_SHA]
The earlier audit provides only parsing/math helpers; its top-level audit is not run.
"""
import ast
import hashlib
import json
from pathlib import Path
import re
import struct
import subprocess
import sys

root = Path(sys.argv[1]).resolve()
base = sys.argv[2] if len(sys.argv) > 2 else '2e193c336c6176d80b9ddbdff7269246bf2590fb'
common = sys.argv[3] if len(sys.argv) > 3 else '0ccba80eef0da4703cb7ffcd1fb8e22848002b3d'
prefix = 'Assets/HELLSCRIPT/Runtime/Presentation/'
names = ['GameUI.GearRecommendations.cs', 'RiftRecommendationWindow.cs', 'RiftGearSettingsWindow.cs']
helper = Path(__file__).with_name('audit-partial-implementation.py')
parsed = ast.parse(helper.read_text())
selected = [node for node in parsed.body if isinstance(node, ast.FunctionDef) and node.name in {'cmap_codes', 'rgb', 'luminance', 'contrast'}]
exec(compile(ast.Module(body=selected, type_ignores=[]), str(helper), 'exec'))

def calls(source, pattern):
    result = []
    for match in re.finditer(pattern, source):
        pos = start = match.end()
        stack, quote, escaped, args = ['('], None, False, []
        while pos < len(source) and stack:
            char = source[pos]
            if quote:
                if escaped:
                    escaped = False
                elif char == '\\':
                    escaped = True
                elif char == quote:
                    quote = None
            elif char in ('"', "'"):
                quote = char
            elif source[pos:pos + 2] == '//':
                end = source.find('\n', pos)
                pos = len(source) if end < 0 else end
                continue
            elif char in '([{':
                stack.append(char)
            elif char in ')]}':
                stack.pop()
                if not stack:
                    args.append(source[start:pos].strip())
            elif char == ',' and stack == ['(']:
                args.append(source[start:pos].strip())
                start = pos + 1
            pos += 1
        result.append(args)
    return result

def git(*args):
    return subprocess.check_output(['git', *args], cwd=root)

def snapshot(revision, path):
    return git('show', revision + ':' + path)

contents = {name: (root / (prefix + name)).read_bytes() for name in names}
report = {'head': git('rev-parse', 'HEAD').decode().strip(), 'base': base, 'common': common,
          'scope': 'Read-only static source and source RGB audit, not C# compilation or runtime validation', 'files': {}}
invariants = []
for name, data in contents.items():
    before, after = snapshot(base, prefix + name).decode(), data.decode()
    checks = {}
    for label, pattern, selector in [
        ('place', r'\b(?:UiLayout\.)?Place\(', lambda a: a),
        ('text_geometry_size', r'UiLayout\.Text\(', lambda a: a[:2] + a[3:8]),
        ('button_geometry', r'v\.Button\(', lambda a: a[:6]),
        ('tab_geometry', r'v\.Tab\(', lambda a: a[:6]),
        ('button_actions', r'v\.Button\(', lambda a: a[6:]),
        ('tab_actions', r'v\.Tab\(', lambda a: a[6:]),
        ('number_end_edit', r'input\.onEndEdit\.AddListener\(', lambda a: a),
        ('slider_callback', r'slider\.onValueChanged\.AddListener\(', lambda a: a),
    ]:
        left, right = [selector(a) for a in calls(before, pattern)], [selector(a) for a in calls(after, pattern)]
        checks[label] = {'before': len(left), 'after': len(right), 'equal': left == right}
        invariants.append(left == right)
    pattern = r'(?:\b|\.)(?:fontSize|resizeTextMinSize|resizeTextMaxSize|sizeDelta|anchorMin|anchorMax|anchoredPosition|pivot)\s*=([^;]+);'
    left, right = re.findall(pattern, before), re.findall(pattern, after)
    checks['direct_geometry_font_assignments'] = {'before': len(left), 'after': len(right), 'equal': left == right}
    invariants.append(left == right)
    if name.startswith('GameUI.'):
        marker = '        // A timed card'
        checks['timer_transaction_tail_byte_equal'] = before[before.index(marker):] == after[after.index(marker):]
        invariants.append(checks['timer_transaction_tail_byte_equal'])
    # Optional skin must be the only added argument to existing shared view constructors.
    for label, pattern, font_index in [
        ('window', r'ContentWindowView\.Open\(', None),
        ('comparison', r'EquipmentComparisonView\.Create\(', 4),
        ('detail', r'ItemDetailView\.Append\(', 3),
        ('slot', r'EquipmentSlotView\.Create\(', 5),
    ]:
        left, right = calls(before, pattern), calls(after, pattern)
        pairs = []
        for old, new in zip(left, right):
            raw = [arg for arg in new if not arg.startswith('skin:')]
            normalized_old, normalized_new = old[:], raw[:]
            if font_index is not None:
                if font_index == len(normalized_new):
                    normalized_new.append('<omitted-optional-font>')
                normalized_old[font_index] = normalized_new[font_index] = '<presentation-font>'
            if label == 'window':
                normalized_old[4] = normalized_new[4] = '<content-builder-checked-separately>'
            pairs.append({'legacy_arguments_equal_except_font': normalized_old == normalized_new,
                          'skin': [arg for arg in new if arg.startswith('skin:')]})
        checks[label + '_call_contract'] = {'before': len(left), 'after': len(right), 'pairs': pairs}
        invariants.append(len(left) == len(right) and all(p['legacy_arguments_equal_except_font'] and len(p['skin']) == 1 for p in pairs))
    report['files'][name] = {'sha256': hashlib.sha256(data).hexdigest(), 'checks': checks}

common_paths = git('diff', '--name-only', base, common).decode().splitlines()
report['common_files'] = {}
for path in common_paths:
    present = (root / path).is_file()
    data = (root / path).read_bytes() if present else snapshot('HEAD', path)
    same = data == snapshot(common, path)
    report['common_files'][path] = {'sha256': hashlib.sha256(data).hexdigest(), 'equal_to_common_commit': same,
                                   'read_from': 'working tree' if present else 'HEAD blob (excluded by sparse checkout)'}
    invariants.append(same)
report['common_ancestor'] = subprocess.run(['git', 'merge-base', '--is-ancestor', common, 'HEAD'], cwd=root).returncode == 0
invariants.append(report['common_ancestor'])

look = json.loads(snapshot(base, 'Assets/HELLSCRIPT/Resources/Data/SkillTreeD4Looks.json'))
palette = {row['id']: [row[c] for c in ('r', 'g', 'b')] for row in look['palette']}
skin = (root / (prefix + 'UiSkin.cs')).read_text()
token_ids = re.findall(r'D4Token\("([^"]+)"\)', skin)
report['common_palette_tokens'] = {name: name in palette for name in token_ids}
invariants.extend(report['common_palette_tokens'].values())
cream, shade, navy = (palette[k] for k in ['IvoryFFFAF0A1000', 'IvoryFFF6E3A1000', 'Steel2A3150A1000'])

def on_light(value):
    paper = luminance(shade)
    if (paper + .05) / (luminance(value) + .05) >= 4.5:
        return value
    low, high = 0, 1
    for _ in range(12):
        t = (low + high) * .5
        candidate = [c + (n - c) * t for c, n in zip(value, navy)]
        if (paper + .05) / (luminance(candidate) + .05) >= 4.5:
            high = t
        else:
            low = t
    return [c + (n - c) * high for c, n in zip(value, navy)]

theme = (root / (prefix + 'UiTheme.cs')).read_text()
colors = {name: rgb(re.search(name + r'Hex="([0-9a-f]{6})"', theme)[1]) for name in ['Success', 'Danger']}
colors.update({'grade_' + value: rgb(value) for value in re.findall(r'"([0-9a-f]{6})"', (root / (prefix + 'EquipmentGradePalette.cs')).read_text())})
colors['heading'] = palette['Steel8FD3FFA1000']
colors['active_muted_body'] = palette['Steel6B7393A1000']
report['contrast'] = {'method': 'Source RGB OnLight 12-step search on cream gradient endpoints; no framebuffer/device certification', 'semantic_text': {}}
for name, value in colors.items():
    result = {'cream': contrast(on_light(value), cream), 'cream_shade': contrast(on_light(value), shade)}
    report['contrast']['semantic_text'][name] = result
    invariants.append(min(result.values()) >= 4.5)
report['contrast']['muted_body'] = {'cream': contrast(on_light(palette['Steel6B7393A1000']), cream), 'cream_shade': contrast(on_light(palette['Steel6B7393A1000']), shade)}
report['contrast']['comparison_tab_navy'] = {bg: contrast(navy, palette[bg]) for bg in ['GoldFFD23FA1000', 'GoldFFC21AA1000', 'IvoryFFFAF0A1000']}
report['fonts'] = {}
for filename in ['Jua-SkillSubset.ttf', 'NotoSansKR-Skill400.ttf', 'NotoSansKR-Skill700.ttf', 'NotoSansKR-Skill900.ttf']:
    data = snapshot(common, 'Assets/HELLSCRIPT/Resources/Fonts/CasualSkill/' + filename)
    codes = cmap_codes(data)
    missing = sorted(set(c for c in '+ 조건 옵션 추가' if ord(c) not in codes))
    report['fonts'][filename] = {'sha256': hashlib.sha256(data).hexdigest(), 'fullwidth_plus': 0xFF0B in codes, 'ascii_plus': 43 in codes, 'corrected_label_missing': missing}
    invariants.append(not missing)
    if filename.startswith('Noto'):
        invariants.append(0xFF0B in codes)

window, settings = (contents[name].decode() for name in names[1:])
report['integration'] = {
    'cached_approved_art_scope': 'public static UiSkin Current {get;}=UiSkin.Casual.WithEquipmentArt(CasualEquipmentArt.Provider);' in window,
    'no_screen_local_art_map': 'new EquipmentArtProvider' not in window,
    'no_remaining_minimal_override': all('UiButtonChrome.Minimal' not in data.decode() for data in contents.values()),
    'localization_key_retained': '"＋ 조건 옵션 추가"' in settings,
    'original_plus_restored': ".Replace('＋','+')" not in settings,
    'tab_uses_common_navy': 'v.Skin.ApplyLabel(compare.GetComponentInChildren<Text>());' not in window and 'Skin!=null?Skin.Navy:' in (root / (prefix + 'ContentWindowView.cs')).read_text(),
    'no_redundant_muted_adjustment': all('OnLight(skin.Muted)' not in data.decode() for data in contents.values()),
    'inputs_unchanged_during_audit': all((root / (prefix + name)).read_bytes() == data for name, data in contents.items()),
}
invariants.extend(report['integration'].values())
report['known_limits'] = ['Shared detail geometry is restored in source; native wrap, layout and six geometry cases remain unexecuted', 'Approved art is connected in source; Unity import/loading/rendering remains unverified', 'No Unity execution, C# compilation, rendered geometry/input/scroll/save or device validation', 'Preserved best-fit and ellipsis are legacy behavior, not proof of readable rendering']
report['passed'] = all(invariants)
print(json.dumps(report, ensure_ascii=False, indent=2))
sys.exit(0 if report['passed'] else 1)
