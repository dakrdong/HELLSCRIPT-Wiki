#!/usr/bin/env python3
"""Narrow source evidence, not a substitute for Unity compile/render/input tests."""
import collections
import hashlib
import json
from pathlib import Path
import re
import subprocess

ROOT = Path(__file__).resolve().parents[3]
BASE = '2e193c336c6176d80b9ddbdff7269246bf2590fb'
COMMON_BASE = '0ccba80eef0da4703cb7ffcd1fb8e22848002b3d'
PRESENTATION = Path('Assets/HELLSCRIPT/Runtime/Presentation')

def git(*args):
    return subprocess.check_output(['git', *args], cwd=ROOT, text=True)

def calls(source, pattern):
    """Extract balanced call text, respecting escaped string and char literals."""
    result = []
    for match in re.finditer(pattern + r'\s*\(', source):
        depth, quote, escaped = 1, None, False
        end = match.end()
        while depth:
            char = source[end]
            if escaped:
                escaped = False
            elif quote:
                if char == '\\': escaped = True
                elif char == quote: quote = None
            elif char in ('"', "'"): quote = char
            elif char == '(': depth += 1
            elif char == ')': depth -= 1
            end += 1
        result.append(re.sub(r'\s+', '', source[match.start():end]))
    return result

def main():
    paths = sorted(p for p in git('ls-tree', '-r', '--name-only', BASE, str(PRESENTATION)).splitlines()
                   if re.search(r'/BlacksmithWindow(?:\.[A-Za-z.]+)?\.cs$', p))
    assert len(paths) == 14, paths
    checks, inputs = {}, {}
    for name, pattern in [('store_calls', r'\bstore\.\w+'),
                          ('rect_placement', r'\b(?:UiLayout\.)?Place'),
                          ('localization', r'\bLoc\.(?:T|F)'),
                          ('click_listeners', r'\bonClick\.AddListener')]:
        count = 0
        for path in paths:
            old, new = git('show', BASE + ':' + path), (ROOT / path).read_text()
            before, after = calls(old, pattern), calls(new, pattern)
            assert before == after, (name, path)
            count += len(after)
            inputs[path] = hashlib.sha256(new.encode()).hexdigest()
        checks[name] = {'equal': True, 'calls': count}
    for path in paths:
        old, new = git('show', BASE + ':' + path), (ROOT / path).read_text()
        sizes = r'(?:resizeTextMinSize|resizeTextMaxSize|fontSize)\s*=\s*[^;]+;'
        assert re.findall(sizes, old) == re.findall(sizes, new), ('font_sizes', path)
    checks['font_size_and_existing_best_fit_assignments'] = 'unchanged'
    # Gameplay comparisons retain the original baseline; ownership excludes the
    # separately approved common-owner API commit merged into this branch.
    changed = git('diff', '--name-only', COMMON_BASE).splitlines()
    owned_new = {str(PRESENTATION / 'BlacksmithWindow.Casual.cs'), str(PRESENTATION / 'BlacksmithWindow.Casual.cs.meta'),
                 'Assets/HELLSCRIPT/Tests/Editor/BlacksmithCasualSkinTests.cs', 'Assets/HELLSCRIPT/Tests/Editor/BlacksmithCasualSkinTests.cs.meta'}
    assert all(path in set(paths) | owned_new for path in changed if path.startswith('Assets/')), changed
    checks['changed_existing_assets'] = changed
    palette = {v['id'] for v in json.loads((ROOT / 'Assets/HELLSCRIPT/Resources/Data/SkillTreeD4Looks.json').read_text())['palette']}
    skin = (ROOT / PRESENTATION / 'BlacksmithWindow.Casual.cs').read_text()
    screen = '\n'.join((ROOT / path).read_text() for path in paths)
    equipment = calls(screen, r'EquipmentSlotView\.(?:Create|Paint|Icon)')
    details = calls(screen, r'ItemDetailView\.(?:Create|AppendCatalog)')
    assert len(equipment) == 13 and len(details) == 2, (len(equipment), len(details))
    assert all('skin:skin' in call for call in equipment + details)
    assert 'ForgeTextInk' not in screen + skin
    for filename in ['BlacksmithWindow.Controls.cs', 'BlacksmithWindow.Cores.cs']:
        assert 'skin.ApplyChrome(b)' in (ROOT / PRESENTATION / filename).read_text()
    checks['common_skin_wiring'] = {'equipment_calls': len(equipment), 'detail_calls': len(details),
                                    'legacy_contrast_shim_removed': True}
    tokens = set(re.findall(r'(?:Hex|D4Token)\("([^"]+)"\)', skin))
    assert tokens <= palette, tokens - palette
    checks['generated_palette_tokens_present'] = sorted(tokens)
    inputs[str(PRESENTATION / 'BlacksmithWindow.Casual.cs')] = hashlib.sha256(skin.encode()).hexdigest()
    report = {'result': 'passed', 'base': BASE, 'ownership_base': COMMON_BASE, 'checks': checks, 'inputs': inputs,
              'limits': 'Text measurements depend on the new fonts. Equal Place call arguments do not prove equal rendered rectangles, wrapping, clipping or behavior. Unity is not run by this script.'}
    output = Path(__file__).with_name('source-invariants.json')
    output.write_text(json.dumps(report, ensure_ascii=False, indent=2) + '\n')
    print('PASS: 14 existing screen files; store calls, placement calls, localization, click listeners and font-size assignments preserved; palette IDs exist.')

if __name__ == '__main__':
    main()
