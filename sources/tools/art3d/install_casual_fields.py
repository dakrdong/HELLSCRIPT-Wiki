"""Install a reviewed procedural field kit without replacing existing Unity GUIDs.

Usage: python3 tools/art3d/install_casual_fields.py F4 [F0 ... F5]
The original meshes/textures remain recoverable in Git. No gameplay data is written.
"""
import hashlib
import json
import shutil
import sys
from pathlib import Path

from PIL import Image, ImageColor

ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / "Docs/Art/CasualConcept/2026-10-14/Field"
TARGET = ROOT / "Assets/HELLSCRIPT/Resources/World/Fields"


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def recolor(path, palette):
    # Retain the UV-authored silhouette/details of props absent from the small kit,
    # using three matte colour bands instead of dirty baked shading.
    with Image.open(path) as original:
        gray = original.convert("L")
        gray.thumbnail((512, 512))
        hist = gray.histogram()
        total = sum(hist)
        cuts = []
        count = 0
        for value, amount in enumerate(hist):
            count += amount
            if not cuts and count >= total * .25:
                cuts.append(value)
            if len(cuts) == 1 and count >= total * .75:
                cuts.append(value)
        colors = [ImageColor.getrgb(palette[k]) for k in ("stone2", "stone", "light")]
        channels = [gray.point([colors[0 if v <= cuts[0] else 1 if v <= cuts[1] else 2][c]
                               for v in range(256)]) for c in range(3)]
        Image.merge("RGBA", (*channels, Image.new("L", gray.size, 26))).save(path)


def install(field):
    if field not in [f"F{n}" for n in range(6)]:
        raise ValueError(f"Unknown field: {field}")
    source, target = SOURCE / field, TARGET / field
    kit = json.loads((source / "manifest_fields_casual.json").read_text())
    for model in kit["models"].values():
        for name, expected in model["sha256"].items():
            if sha(source / name) != expected:
                raise ValueError(f"Unreviewed source bytes: {field}/{name}")
    for tile in ("Floor", "Wall", "Trim"):
        if not (source / f"{tile}_A.png").is_file():
            raise ValueError(f"Missing tile: {field}/{tile}")
    target.mkdir(parents=True, exist_ok=True)
    metas = {p.name: p.read_bytes() for p in target.glob("*.meta")}
    before = sum(p.stat().st_size for p in target.iterdir() if p.suffix in (".png", ".fbx", ".json"))
    runtime_manifest = target / "manifest_fields_casual.json"
    prior = json.loads(runtime_manifest.read_text()) if runtime_manifest.exists() else {}
    legacy = {p.name for p in target.glob("*_A.png")} - {p.name for p in source.glob("*_A.png")}
    for name in sorted(legacy - set(prior.get("recolored", []))):
        recolor(target / name, kit["palette"])
    for path in sorted(source.iterdir()):
        if path.suffix in (".fbx", ".png"):
            shutil.copyfile(path, target / path.name)
    if field == "F5":
        # Soften the candidate's blue snow blotches while retaining its periodic texture.
        with Image.open(target / "Floor_A.png") as snow:
            base = Image.new("RGBA", snow.size, (*ImageColor.getrgb(kit["palette"]["floor"][0]), 26))
            Image.blend(base, snow, .25).save(target / "Floor_A.png")
    # Flat tangent normals deliberately keep the loader's _A/_N contract. 4px is
    # enough for a constant map; alpha on _A remains smoothness, never transparency.
    for albedo in target.glob("*_A.png"):
        Image.new("RGBA", (4, 4), (128, 128, 255, 255)).save(target / (albedo.stem[:-2] + "_N.png"))
    # Existing extra props retain their meshes, footprints and GUIDs, including glow
    # masks. Their emission uses the new field's lantern/crystal colour.
    for path in target.glob("manifest*.json"):
        if path == runtime_manifest:
            continue
        manifest = json.loads(path.read_text())
        for model in manifest.get("models", {}).values():
            if model.get("glow"):
                model["glow"] = kit["palette"]["glow"]
            for name in model.get("sha256", {}):
                if (target / name).exists():
                    model["sha256"][name] = sha(target / name)
                    if name.endswith("_A.png"):
                        with Image.open(target / name) as image:
                            model["texture"] = image.width
        path.write_text(json.dumps(manifest, indent=2, ensure_ascii=False) + "\n")
    installed = {**kit, "revision": "hellscript-casual-field-runtime-v1", "recolored": sorted(legacy)}
    for model in installed["models"].values():
        for name in model["sha256"]:
            model["sha256"][name] = sha(target / name)
    installed["files"] = {p.name: sha(p) for p in sorted(target.iterdir()) if p.suffix in (".png", ".fbx")}
    runtime_manifest.write_text(json.dumps(installed, indent=2, ensure_ascii=False) + "\n")
    # Self-check the actual output, not only the copy command's success.
    for name, expected in installed["files"].items():
        assert sha(target / name) == expected, name
    for name, original in metas.items():
        assert (target / name).read_bytes() == original, f"Unity GUID changed: {name}"
    for albedo in target.glob("*_A.png"):
        with Image.open(albedo) as image:
            assert image.mode == "RGBA" and image.getchannel("A").getextrema() == (26, 26), albedo
        with Image.open(target / (albedo.stem[:-2] + "_N.png")) as image:
            assert image.getextrema() == ((128, 128), (128, 128), (255, 255), (255, 255)), albedo
    after = sum(p.stat().st_size for p in target.iterdir() if p.suffix in (".png", ".fbx", ".json"))
    print(f"{field}: {len(installed['files'])} assets, {len(legacy)} recolored, {before} -> {after} bytes ({after-before:+d})")


if __name__ == "__main__":
    if len(sys.argv) < 2:
        raise SystemExit(__doc__)
    for field in sys.argv[1:]:
        install(field)
