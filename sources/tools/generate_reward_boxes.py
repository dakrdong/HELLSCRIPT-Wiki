#!/usr/bin/env python3
"""Rebuild the native-art registry and QA contact sheet without drawing runtime art or changing rewards."""
import json
from pathlib import Path
from PIL import Image, ImageDraw
from adopt_generated_game_art import inspect

ROOT = Path(__file__).resolve().parents[1]
ART = ROOT / 'Assets/HELLSCRIPT/Resources/Art/RewardBoxes'
SOURCE = ROOT / 'Docs/Art/RewardBoxes'
DATA = ROOT / 'Assets/HELLSCRIPT/Resources/Data/RewardBoxes.json'
PLAN = 'Docs/Art/GeneratedGameArt/requests.json'


def main():
    db = json.loads(DATA.read_text())
    requests = {Path(target).stem: row for row in json.loads((ROOT / PLAN).read_text())['requests']
                if row['category'] == 'reward-box' for target in row['targets']}
    icons, tiles = [], []
    for box in db['boxes']:
        resource = box['icon']
        request = requests[resource]
        assert request['status'] == 'generated', request['id']
        path = ART / (resource + '.png')
        metadata = inspect(path, True)
        assert metadata['sha256'] == request['sha256'], path
        icons.append(dict(id=box['id'], resourceId=resource, requestId=request['id'], **metadata))
        # Read-only QA composition; the native PNG masters are never resampled or overwritten.
        tile = Image.new('RGB', (180, 198), '#181c19')
        with Image.open(path) as native:
            preview = native.copy(); preview.thumbnail((156, 156))
            tile.paste(preview, ((180-preview.width)//2, 0), preview)
        ImageDraw.Draw(tile).text((6, 163), box['id'], fill='#ded4b6')
        tiles.append(tile)
    sheet = Image.new('RGB', (180*8, 198*((len(tiles)+7)//8)), '#181c19')
    for n, tile in enumerate(tiles):
        sheet.paste(tile, (n%8*180, n//8*198))
    sheet.save(SOURCE / 'contact-sheet.png')
    manifest = dict(provenance='Built-in image_gen native PNG; model provenance unavailable (candidate_model_unknown)',
                    generator='tools/generate_reward_boxes.py (registry and QA only)', requestManifest=PLAN,
                    model='unknown', productionApproved=False, archivedVectors='Historical SVGs only; not runtime masters',
                    icons=icons)
    (SOURCE / 'manifest.json').write_text(json.dumps(manifest, ensure_ascii=False, indent=2) + '\n')
    print(len(icons), 'unchanged box definitions;', len(requests), 'native sprite paths registered')


if __name__ == '__main__':
    main()
