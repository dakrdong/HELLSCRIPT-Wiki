#!/usr/bin/env python3
"""Build HELLSCRIPT's procedural 3D world art with headless Blender.

    python3 tools/generate_world_art.py                     # build every recipe into the dry-run folder Builds/WorldArt
    python3 tools/generate_world_art.py --install           # build into Assets/HELLSCRIPT/Resources/World
    python3 tools/generate_world_art.py --only N0 --jobs 4  # id prefix subset, 4 Blender processes at once
    python3 tools/generate_world_art.py --previews DIR      # also write <id>.preview.png + contact-sheet.png (never in Assets)
    python3 tools/generate_world_art.py --check             # verify the installed World/ folder (no Blender needed)
    python3 tools/generate_world_art.py --check --out DIR   # verify a dry-run folder

Recipes are tools/art3d/recipes_<group>.py modules with ASSETS = [dict(id, family, fn)] (ASSETS.md "Recipe
interface"). Each asset is built in its own `blender -b --factory-startup` process by tools/art3d/build_one.py
into a staging folder, then moved into <out>/<family>/. The runner hashes the files, merges the group's
<family>/manifest_<group>.json and writes minimal Unity metas with deterministic GUIDs when they are missing.
Everything is seeded geometry and shader graphs written in this repository: no scans, downloads, AI
generators or third-party game assets.

Rebuilds: hs3d pins the FBX exporter's clock and object UIDs and hs3d.sphere's face order, so a recipe whose own
geometry steps are deterministic rebuilds a byte-identical FBX (the samples do). Cycles bakes on the Metal GPU drift by
a few levels on under 0.1% of pixels, so a rebuild can change a PNG's sha256; --check compares files with their manifest,
not with a fresh rebuild.

World/Fx also holds the single-PNG VFX textures of tools/generate_fx_textures.py (VFX.md). --check only verifies the
Fx files a manifest claims (a manifest there needs no _A/_N pair); unclaimed Fx files just need a meta, and code may
reference them by name.
"""
import argparse
import hashlib
import json
import re
import shutil
import struct
import subprocess
import sys
import tempfile
import time
from concurrent.futures import ThreadPoolExecutor
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
ART3D = ROOT / "tools/art3d"
WORLD = ROOT / "Assets/HELLSCRIPT/Resources/World"
WORLD_REL = "Assets/HELLSCRIPT/Resources/World"
DRY_RUN = ROOT / "Builds/WorldArt"
CODE = ROOT / "Assets/HELLSCRIPT/Runtime"
BLENDER = "/opt/homebrew/bin/blender"
REVISION = "hellscript-art3d-v1"
PROVENANCE = "Procedurally authored in Blender from tools/art3d recipes; no external assets or generators"
# Manifests written by importers of externally generated models (tools/import_hunyuan_town.py) carry their own revision and provenance.
IMPORT_REVISIONS = ("hellscript-hunyuan-town-v1",)
FAMILY = re.compile(r"^(Characters|Bosses|Props|Town|Fx|Fields/F\d+)$")
ASSET_ID = re.compile(r"^[A-Z][A-Za-z0-9_]*$")
# PLAN.md §4 hard limits: budget class -> (max triangles, max texture edge).
BUDGETS = {
    "hero": (6000, 512), "weapon": (1000, 512), "enemy": (5000, 512), "boss": (14000, 1024), "npc": (6000, 512),
    "prop_small": (800, 512), "prop_large": (2500, 512), "building": (6000, 1024), "floor": (50, 1024), "wall": (50, 512),
    "fx": (800, 1024),
}
BIG_TOWN = ("Building_", "Gatehouse", "Portal_Ring", "Aspect_Runestone")
# Check complete literal paths; dynamically constructed paths are covered by the runtime asset tests.
CODE_REF = re.compile(r'"(?:World/)?((?:Characters|Bosses|Props|Town|Fx|Fields/F\d+)/[A-Za-z0-9_]+)"(?!\s*\+)')
OUTPUTS = (".fbx", "_A.png", "_N.png")
ROLES = {"fbx": ".fbx", "albedo": "_A.png", "normal": "_N.png"}  # role-keyed hashes, as manifest_enemies_a.json writes them
FX = "Fx"


def guid(rel):
    return hashlib.md5(("hellscript-art3d:" + rel).encode()).hexdigest()


def sha256(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def png_size(path):
    with open(path, "rb") as f:
        head = f.read(24)
    if head[:8] != b"\x89PNG\r\n\x1a\n":
        raise ValueError(f"{path} is not a PNG")
    return struct.unpack(">II", head[16:24])


def budget_class(family, aid, info=None):
    """PLAN §4 budget for an asset; a recipe may name another class with a 'budget' key."""
    if (info or {}).get("budget"):
        return info["budget"]
    if family == "Characters":
        return "hero" if aid.startswith("Hero_") else "enemy"
    if family == "Bosses":
        return "boss"
    if family == "Props":
        return "weapon" if aid.startswith("Weapon_") else "prop_large"
    if family == "Town":
        return "npc" if aid.startswith("Npc_") else "building" if aid.startswith(BIG_TOWN) else "prop_large"
    if family == "Fx":
        return "fx"
    if aid.startswith(("Floor", "Wall", "Trim")):  # Fields/F<n> tile sets Floor/Wall/Trim_A|_N (F<n>_Wall is an obstacle)
        return "floor" if aid.startswith("Floor") else "wall"
    return "prop_small" if re.sub(r"^F\d+_", "", aid).startswith("Decor") else "prop_large"


def asset_rel(out, path):
    """Unity path the file has once installed, so dry-run and installed metas share GUIDs."""
    rel = Path(path).relative_to(out).as_posix()
    return WORLD_REL if rel == "." else f"{WORLD_REL}/{rel}"


def write_meta(out, path):
    meta = Path(str(path) + ".meta")
    if meta.exists():
        return False
    body = ("folderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n"
            if Path(path).is_dir() else "")
    meta.write_text(f"fileFormatVersion: 2\nguid: {guid(asset_rel(out, path))}\n{body}")
    return True


def visible(out, installed=False):
    """Every file and folder Unity imports under out (the installed World/ root itself too); Unity ignores dot names.
    A dry-run root gets no meta, so the folder stays self-contained when it is copied or moved."""
    out = Path(out)
    return [out] * installed + [p for p in sorted(out.rglob("*")) if not any(part.startswith(".") for part in p.relative_to(out).parts)]


def write_metas(out, installed=False):
    for p in visible(out, installed):
        if p.suffix != ".meta":
            write_meta(out, p)


def entry_files(aid, entry):
    """{file name: sha256} from an entry's sha256/files map; keys may be full names, suffixes like '_A.png' or roles."""
    listed = entry.get("sha256") or entry.get("files") or {}
    if isinstance(listed, list):
        listed = {n: "" for n in listed}
    listed = {ROLES.get(k, k): v for k, v in listed.items()}
    return {(aid + k if k.startswith((".", "_")) else k): v for k, v in listed.items()}


def manifest_path(folder, group):
    return Path(folder) / f"manifest_{group}.json"


def merge_manifest(folder, group, family, results, replace=False, revision=REVISION, generator="tools/generate_world_art.py", provenance=PROVENANCE):
    """Merge {id: entry} into <folder>/manifest_<group>.json; replace drops (and deletes the files of) ids not rebuilt."""
    path = manifest_path(folder, group)
    old = json.loads(path.read_text()).get("models", {}) if path.exists() else {}
    models = dict(results) if replace else dict(old, **results)
    kept = {n for aid, e in models.items() for n in entry_files(aid, e)}
    for other in Path(folder).glob("manifest*.json"):  # files another group's manifest owns now (an id that moved) stay
        if other != path:
            kept |= {n for aid, e in json.loads(other.read_text()).get("models", {}).items() for n in entry_files(aid, e)}
    for aid, e in old.items():
        if aid not in models:
            for name in entry_files(aid, e):
                if name not in kept:
                    for p in (Path(folder) / name, Path(folder) / (name + ".meta")):
                        p.unlink(missing_ok=True)
    data = {"revision": revision, "group": group, "family": family, "generator": generator,
            "productionApproved": False, "provenance": provenance, "models": dict(sorted(models.items()))}
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(data, indent=2, ensure_ascii=False) + "\n")
    return data


def validate_listing(assets):
    problems, seen = [], set()
    for a in assets:
        aid, family = a.get("id", "?"), a.get("family", "")
        if not ASSET_ID.match(aid):
            problems.append(f"{aid}: id must be English PascalCase")
        if (family, aid) in seen:
            problems.append(f"{family}/{aid}: duplicate id")
        seen.add((family, aid))
        if not FAMILY.match(family):
            problems.append(f"{aid}: unknown family '{family}'")
        if budget_class(family, aid, a) not in BUDGETS:
            problems.append(f"{aid}: unknown budget '{a.get('budget')}'")
    return problems


def manifests(out):
    return {p: json.loads(p.read_text()) for p in sorted(Path(out).rglob("manifest*.json"))}


def strip_comments(line):
    """Drop a // comment unless the // sits inside a string literal (an odd number of quotes before it)."""
    at = line.find("//")
    while at >= 0 and line.count('"', 0, at) % 2:
        at = line.find("//", at + 2)
    return line if at < 0 else line[:at]


def code_refs(code=CODE):
    refs = {}
    for path in sorted(Path(code).rglob("*.cs")):
        source = "\n".join(strip_comments(line) for line in path.read_text(encoding="utf-8").splitlines())
        for m in CODE_REF.finditer(source):
            refs.setdefault(m.group(1), path.name)
    return refs


def check(out, installed=False, code=CODE):
    """Everything --check verifies; returns a list of problems (empty = pass)."""
    out, problems, owner = Path(out), [], {}
    if not out.exists():
        return [f"{out} does not exist"] if not installed else [f"code references {r} but {out} is missing" for r in code_refs(code)]
    found = manifests(out)
    for path, data in found.items():
        folder, family = path.parent, path.parent.relative_to(out).as_posix()
        owner[path] = path.name
        if data.get("productionApproved") is not False:
            problems.append(f"{family}/{path.name}: manifest must say productionApproved false")
        if data.get("revision") not in (REVISION, *IMPORT_REVISIONS):
            problems.append(f"{family}/{path.name}: revision is not {REVISION}")
        for aid, e in data.get("models", {}).items():
            where, cls = f"{family}/{aid}", budget_class(family, aid, e)
            tris_max, tex_max = BUDGETS.get(cls, (0, 0))
            if cls not in BUDGETS:
                problems.append(f"{where}: unknown budget '{cls}'")
            files = entry_files(aid, e)
            need = ([aid + ".fbx"] if e.get("tris", 0) > 0 else []) + ([aid + "_A.png", aid + "_N.png"] if e.get("texture") and family != FX else [])
            problems += [f"{where}: manifest lacks {n}" for n in need if n not in files]
            if not files:
                problems.append(f"{where}: no files recorded")
            if e.get("tris", 0) > tris_max:
                problems.append(f"{where}: {e['tris']} triangles over the {cls} budget {tris_max}")
            for name, digest in files.items():
                p = folder / name
                if p in owner and owner[p] != path.name:
                    problems.append(f"{where}: {name} is also listed by {owner[p]}")
                owner[p] = path.name
                if not p.exists():
                    problems.append(f"{where}: missing {name}")
                    continue
                if sha256(p) != digest:
                    problems.append(f"{where}: {name} sha256 differs from the manifest")
                if p.suffix == ".png":
                    try:
                        w, h = png_size(p)
                    except (ValueError, struct.error):
                        problems.append(f"{where}: {name} is not a PNG")
                        continue
                    if w & (w - 1) or h & (h - 1):
                        problems.append(f"{where}: {name} is {w}x{h}, not a power of two")
                    if max(w, h) > tex_max:
                        problems.append(f"{where}: {name} is {w}x{h}, over the {cls} texture budget {tex_max}")
    fx_names = set()
    for p in visible(out, installed):
        if p.suffix == ".meta":
            if not Path(str(p)[:-5]).exists():
                problems.append(f"stale meta {p.relative_to(out)}")
            continue
        meta, want = Path(str(p) + ".meta"), guid(asset_rel(out, p))
        foreign = p not in owner and p.relative_to(out).parts[:1] == (FX,)  # generate_fx_textures.py's file or folder
        if foreign:
            fx_names.add(p.name.split(".")[0])
        if not meta.exists():
            problems.append(f"missing meta for {asset_rel(out, p)}")
        elif not foreign and f"guid: {want}" not in meta.read_text():
            problems.append(f"{asset_rel(out, p)}.meta guid is not the deterministic {want}")
        if p.is_file() and p not in owner and not foreign:
            problems.append(f"stale file {p.relative_to(out)} is in no manifest")
    if installed:
        known = {}
        for p in owner:
            known.setdefault(p.parent.relative_to(out).as_posix(), set()).add(p.name.split(".")[0])
        known.setdefault(FX, set()).update(fx_names)
        for ref, where in code_refs(code).items():
            family, _, name = ref.rpartition("/")
            if name not in known.get(family, ()):
                problems.append(f"{where} references World/{ref}, which no manifest provides")
    return problems


def blender(args, timeout=1800):
    cmd = [BLENDER, "-b", "--factory-startup", "--python-exit-code", "1", "-P", str(ART3D / "build_one.py"), "--"] + args
    return subprocess.run(cmd, capture_output=True, text=True, timeout=timeout, cwd=ROOT)


def tagged(proc, tag):
    line = next((l for l in proc.stdout.splitlines() if l.startswith(tag + " ")), None)
    return json.loads(line[len(tag) + 1:]) if line else None


def list_assets():
    proc = blender(["--list"], timeout=300)
    rows = tagged(proc, "HS3D_LIST")
    if proc.returncode or rows is None:
        raise SystemExit("recipe listing failed:\n" + (proc.stdout + proc.stderr)[-3000:])
    return rows


def staged_files(stage):
    return sorted(p.name for p in Path(stage).iterdir()
                  if p.is_file() and not p.name.startswith(("manifest", ".")) and p.suffix != ".meta" and ".preview" not in p.name)


def build_asset(asset, out, previews):
    """Build into a fresh staging folder, then move the outputs into <out>/<family>/ (a failure leaves out untouched)."""
    start = time.time()
    with tempfile.TemporaryDirectory(prefix="hs3d-") as stage:
        args = [asset["recipe"], asset["id"], stage] + (["--preview", str(previews)] if previews else [])
        try:
            proc = blender(args)
        except subprocess.TimeoutExpired as e:
            return asset, None, time.time() - start, f"timed out after {e.timeout}s"
        info = tagged(proc, "HS3D_RESULT")
        names = staged_files(stage)
        if proc.returncode or info is None or not names:
            return asset, None, time.time() - start, (proc.stdout + proc.stderr)[-3000:] or "no output files"
        folder = Path(out) / asset["family"]
        folder.mkdir(parents=True, exist_ok=True)
        for name in names:
            shutil.move(str(Path(stage) / name), str(folder / name))
        info.pop("files", None)
        info.pop("sha256", None)
        albedo = folder / f"{asset['id']}_A.png"
        if not info.get("texture") and albedo.name in names:  # ASSETS.md recipes need not report their bake size
            info["texture"] = max(png_size(albedo))
        info.update(recipe=asset["recipe"], revision=REVISION, budget=budget_class(asset["family"], asset["id"], dict(asset, **info)),
                    sha256={n: sha256(folder / n) for n in names})
    return asset, info, time.time() - start, ""


def contact_sheet(folder, ids, cell=256, columns=4):
    try:
        from PIL import Image, ImageDraw
    except ImportError:
        print("  (PIL missing: no contact sheet)")
        return
    shots = [(i, Image.open(p).convert("RGB")) for i in ids for p in [Path(folder) / f"{i}.preview.png"] if p.exists()]
    if not shots:
        return
    width = max(img.width * cell // img.height for _, img in shots)
    rows = (len(shots) + columns - 1) // columns
    sheet = Image.new("RGB", (width * columns, (cell + 18) * rows), (12, 13, 17))
    draw = ImageDraw.Draw(sheet)
    for n, (i, img) in enumerate(shots):
        x, y = n % columns * width, n // columns * (cell + 18)
        sheet.paste(img.resize((img.width * cell // img.height, cell)), (x, y + 18))
        draw.text((x + 4, y + 3), i, fill=(230, 200, 150))
    sheet.save(Path(folder) / "contact-sheet.png")


def report(problems):
    for p in problems:
        print("  " + p)
    return 1 if problems else 0


def main():
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("--check", action="store_true")
    ap.add_argument("--install", action="store_true", help="build into Assets/HELLSCRIPT/Resources/World")
    ap.add_argument("--out", default="", help="output (or checked) World root; default Builds/WorldArt, or World/ with --install/--check")
    ap.add_argument("--only", default="", help="asset id prefix")
    ap.add_argument("--jobs", type=int, default=2)
    ap.add_argument("--previews", default="", help="folder for <id>.preview.png renders (never inside Assets)")
    args = ap.parse_args()
    out = WORLD if args.install else Path(args.out).resolve() if args.out else WORLD if args.check else DRY_RUN
    installed = out == WORLD
    if args.check:
        problems = check(out, installed)
        count = sum(len(d.get("models", {})) for d in manifests(out).values()) if out.exists() else 0
        print(f"checked {out}: {count} assets, {len(problems)} problems")
        return report(problems)
    previews = Path(args.previews).resolve() if args.previews else None
    if previews and ROOT / "Assets" in [previews] + list(previews.parents):
        raise SystemExit("previews must stay outside Assets/")
    assets = list_assets()
    problems = validate_listing(assets)
    if problems:
        return report(problems)
    assets = [a for a in assets if a["id"].startswith(args.only)]
    if installed:
        skipped = [a["id"] for a in assets if a.get("install") is False]
        assets = [a for a in assets if a.get("install") is not False]
        if skipped:
            print("not installing pipeline samples: " + ", ".join(skipped))
    started = time.time()
    with ThreadPoolExecutor(max(1, args.jobs)) as pool:
        done = list(pool.map(lambda a: build_asset(a, out, previews), assets))
    groups, failed = {}, set()
    for asset, info, seconds, log in done:
        key = (asset["recipe"][len("recipes_"):], asset["family"])
        if info is None:
            failed.add(key)
            print(f"FAILED {asset['family']}/{asset['id']} after {seconds:.1f}s\n{log}")
            continue
        groups.setdefault(key, {})[asset["id"]] = info
        print(f"  {asset['family'] + '/' + asset['id']:<32} {seconds:6.1f}s  {info.get('tris', 0):>6} tris  tex {info.get('texture', 0)}")
    for (group, family), results in groups.items():  # a complete group build owns its manifest: drop ids it no longer makes
        merge_manifest(out / family, group, family, results, replace=not args.only and (group, family) not in failed)
    if out.exists():
        write_metas(out, installed)
    if previews:
        contact_sheet(previews, [a["id"] for a in assets])
    problems = check(out, installed) if out.exists() else []
    print(f"{sum(map(len, groups.values()))}/{len(assets)} assets built into {out} in {time.time() - started:.1f}s, "
          f"{len(assets) - sum(map(len, groups.values()))} failed, {len(problems)} problems")
    return report(problems) or (1 if failed else 0)


if __name__ == "__main__":
    sys.exit(main())
