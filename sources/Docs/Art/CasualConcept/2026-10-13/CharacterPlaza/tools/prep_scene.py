#!/usr/bin/env python3
"""Lift the six heroes out of a painted town scene and write the layers the character screen needs.

  swiftc -O tools/lift.swift -o tools/lift                      # macOS Vision subject lifting, built once
  python3 tools/prep_scene.py tools/spec_landscape.json --out /tmp/plaza --format png
  python3 tools/prep_scene.py tools/spec_portrait.json  --out /tmp/plaza --format png

Per picture (landscape, portrait) this writes into --out:
  <pic>_bg.<fmt>           the painted scene itself (heroes included)
  <pic>_cut_<hero>.<fmt>   the hero lifted from the scene, exact scene pixels, cropped to its own bounds
  <pic>_patch_<hero>.<fmt> the clean plate inside the hero's grown, feathered mask; laid over the painted hero it hides it
                           while a pose picture (cheer, battle-ready) stands in its place
  scn_<pic>.json           picture size, per hero: cut box, head and feet anchors, patch box, cheer box (feet x, feet y, height),
                           plus the portrait camera framing per class (focus = x, y, zoom, screen x)

Rules of the lift (see the spec files): a cut-out that touches its crop edge is clipped (a hood or hat tip), so that side is
grown and lifted again unless listed in "nogrow"; only the part connected to the seed point is kept; pixels claimed by two heroes
go to the nearer one (lower feet); "exclude" boxes remove townsfolk touching a hero. Everything is mockup-grade processing:
the cut-outs are segmented, not native alpha, so register them as replaceable development art (productionApproved false).
"""
import argparse, json, os, subprocess, sys
from collections import deque
from PIL import Image, ImageFilter
import numpy as np

HERE = os.path.dirname(os.path.abspath(__file__)); ART = os.path.dirname(HERE)   # .../CharacterPlaza
ap = argparse.ArgumentParser()
ap.add_argument('spec'); ap.add_argument('--out', default=os.path.join(ART, 'build')); ap.add_argument('--format', choices=('png', 'webp'), default='png')
ap.add_argument('--lift', default=os.path.join(HERE, 'lift'))
args = ap.parse_args()
if not os.path.exists(args.lift): sys.exit('lift binary missing: swiftc -O %s -o %s' % (os.path.join(HERE, 'lift.swift'), args.lift))
spec = json.load(open(args.spec)); pic = spec['pic']; OUT = args.out; FMT = args.format; TMP = os.path.join(OUT, '_lift_tmp')
os.makedirs(TMP, exist_ok=True)
rel = lambda p: p if os.path.isabs(p) else os.path.join(ART, p)
master = Image.open(rel(spec['master'])).convert('RGB'); W, H = master.size
clean = Image.open(rel(spec['clean'])).convert('RGB').resize((W, H), Image.LANCZOS) if spec.get('clean') else None


def save(im, name):
    path = os.path.join(OUT, '%s.%s' % (name, FMT))
    im.save(path, **({'optimize': True} if FMT == 'png' else {'quality': 92, 'method': 6}))
    return '%s.%s' % (name, FMT)


def seed_component(mask, sx, sy):
    """Keep only the part of the mask connected to the seed (hairline gaps bridged), so a passer-by lifted with the hero drops out."""
    m = np.array(Image.fromarray((mask * 255).astype('uint8')).filter(ImageFilter.MaxFilter(5))) > 0
    hh, ww = m.shape; sx, sy = min(max(sx, 0), ww - 1), min(max(sy, 0), hh - 1)
    if not m[sy, sx]:
        ys, xs = np.where(m); i = int(np.argmin((ys - sy) ** 2 + (xs - sx) ** 2)); sy, sx = int(ys[i]), int(xs[i])
    seen = np.zeros_like(m); seen[sy, sx] = True; q = deque([(sy, sx)])
    while q:
        y, x = q.popleft()
        for ny, nx in ((y + 1, x), (y - 1, x), (y, x + 1), (y, x - 1)):
            if 0 <= ny < hh and 0 <= nx < ww and m[ny, nx] and not seen[ny, nx]:
                seen[ny, nx] = True; q.append((ny, nx))
    return seen & mask


bg_file = save(master, '%s_bg' % pic)

# pass 1: lift every hero
lifted = {}
for k, h in spec['heroes'].items():
    x0, y0, x1, y1 = h['crop']; sx, sy = h['seed']; nogrow = set(h.get('nogrow', []))
    for attempt in range(6):
        cp = os.path.join(TMP, '%s_%s_crop.png' % (pic, k)); lp = os.path.join(TMP, '%s_%s_lift.png' % (pic, k))
        master.crop((x0, y0, x1, y1)).save(cp)
        r = subprocess.run([args.lift, cp, lp, str(sx - x0), str(sy - y0)], capture_output=True, text=True)
        if r.returncode != 0: sys.exit('lift failed %s: %s %s' % (k, r.stdout, r.stderr))
        rgba = np.array(Image.open(lp).convert('RGBA'))
        for ex0, ey0, ex1, ey1 in h.get('exclude', []):                    # townsfolk touching the hero (picture pixels)
            rgba[max(0, ey0 - y0):max(0, ey1 - y0), max(0, ex0 - x0):max(0, ex1 - x0), 3] = 0
        keep = seed_component(rgba[:, :, 3] > 8, sx - x0, sy - y0)
        rgba[:, :, 3] = np.where(keep, rgba[:, :, 3], 0); a = rgba[:, :, 3]
        ys, xs = np.where(a > 8)
        g = {'left': xs.min() <= 0 and x0 > 0, 'top': ys.min() <= 0 and y0 > 0, 'right': xs.max() + 1 >= x1 - x0 and x1 < W, 'bottom': ys.max() + 1 >= y1 - y0 and y1 < H}
        g = {s: v and s not in nogrow for s, v in g.items()}
        if not any(g.values()): break
        if attempt == 5: print('  still touching after growing', k, [s for s, v in g.items() if v]); break
        x0, y0, x1, y1 = max(0, x0 - 40 * g['left']), max(0, y0 - 40 * g['top']), min(W, x1 + 40 * g['right']), min(H, y1 + 40 * g['bottom'])
    full = np.zeros((H, W, 4), np.uint8); full[y0:y1, x0:x1] = rgba
    lifted[k] = {'rgba': full, 'crop': [int(x0), int(y0), int(x1), int(y1)]}

# pass 2: pixels claimed by two heroes belong to the nearer one (lower feet in the picture)
feet_y = {k: int(np.where(v['rgba'][:, :, 3] > 8)[0].max()) for k, v in lifted.items()}
claimed = np.zeros((H, W), bool)
for k in sorted(lifted, key=lambda k: feet_y[k], reverse=True):          # nearest first keeps its pixels
    a = lifted[k]['rgba'][:, :, 3]; mine = a > 8
    lifted[k]['rgba'][:, :, 3] = np.where(claimed & mine, 0, a); claimed |= mine

# pass 3: cut-outs, anchors and cheer patches
out = {'W': W, 'H': H, 'bg': bg_file, 'heroes': {}, 'focus': spec.get('focus', {})}
for k, h in spec['heroes'].items():
    full = lifted[k]['rgba']; a = full[:, :, 3]; ys, xs = np.where(a > 8)
    cx, cy, cx1, cy1 = int(xs.min()), int(ys.min()), int(xs.max()) + 1, int(ys.max()) + 1
    cut = Image.fromarray(full[cy:cy1, cx:cx1]); cut_file = save(cut, '%s_cut_%s' % (pic, k))
    ca = np.array(cut)[:, :, 3] > 40
    tx = np.where(ca[: max(2, int(ca.shape[0] * .12))])[1]; bxs = np.where(ca[int(ca.shape[0] * .9):])[1]
    head = h.get('head') or [int(cx + tx.mean()), int(cy)]
    feet = h.get('feet') or [int(cx + bxs.mean()), int(cy + cut.height)]
    ch = h.get('cheerH') or int(cut.height * h.get('cheerK', 1.0))
    e = {'cut': {'file': cut_file, 'x': cx, 'y': cy, 'w': cut.width, 'h': cut.height}, 'head': head, 'feet': feet, 'cheer': [feet[0], feet[1], ch]}
    if clean is not None:
        m = Image.fromarray(((a > 8) * 255).astype('uint8')).filter(ImageFilter.MaxFilter(13)).filter(ImageFilter.GaussianBlur(3))
        mb = np.array(m); ys2, xs2 = np.where(mb > 4); px0, py0, px1, py1 = int(xs2.min()), int(ys2.min()), int(xs2.max()) + 1, int(ys2.max()) + 1
        region = clean.crop((px0, py0, px1, py1)).convert('RGBA'); region.putalpha(m.crop((px0, py0, px1, py1)))
        e['patch'] = {'file': save(region, '%s_patch_%s' % (pic, k)), 'x': px0, 'y': py0, 'w': region.width, 'h': region.height}
    out['heroes'][k] = e
    print(k, 'crop', lifted[k]['crop'], 'cut', cx, cy, cut.width, cut.height)
json.dump(out, open(os.path.join(OUT, 'scn_%s.json' % pic), 'w'), indent=1)
