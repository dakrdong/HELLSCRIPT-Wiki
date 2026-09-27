#!/usr/bin/env python3
"""Render HELLSCRIPT's procedural sound-effect bank.

    python3 tools/generate_sfx.py            # render WAVs, Unity metas and bank.json
    python3 tools/generate_sfx.py --check    # re-render in memory and verify files + code references
    python3 tools/generate_sfx.py --only ui. # render a subset (prefix match) for iteration
    python3 tools/generate_sfx.py --sheets DIR --audition DIR   # spectrogram sheets / listening page

Needs numpy + scipy (matplotlib only for --sheets). Everything is synthesised from seeded DSP
recipes in tools/sfx: no recordings, samples, speech engines or third-party game audio.
"""
import argparse
import hashlib
import html
import json
import re
import shutil
import sys
import wave
from pathlib import Path

import numpy as np

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / "tools/sfx"))
import dsp  # noqa: E402
import bank  # noqa: E402
import recipes_system, recipes_heroes, recipes_world  # noqa: E402,F401

OUT = ROOT / "Assets/HELLSCRIPT/Resources/Audio/Sfx"
CODE = ROOT / "Assets/HELLSCRIPT/Runtime"
LONG = 2.0  # seconds; longer clips import as Vorbis, shorter as ADPCM


def clip_name(sid, v):
    return re.sub(r"[^A-Za-z0-9]+", "_", sid) + f"_{v + 1}"


def guid(rel):
    return hashlib.md5(("hellscript-sfx:" + rel).encode()).hexdigest()


def folder_meta(path):
    rel = path.relative_to(ROOT).as_posix()
    meta = path.with_name(path.name + ".meta")
    if not meta.exists():
        meta.write_text(f"fileFormatVersion: 2\nguid: {guid(rel)}\nfolderAsset: yes\nDefaultImporter:\n  externalObjects: {{}}\n"
                        "  userData: \n  assetBundleName: \n  assetBundleVariant: \n")


def audio_meta(path, seconds):
    rel = path.relative_to(ROOT).as_posix()
    long = seconds > LONG
    return (f"fileFormatVersion: 2\nguid: {guid(rel)}\nAudioImporter:\n  externalObjects: {{}}\n  serializedVersion: 8\n"
            f"  defaultSettings:\n    serializedVersion: 2\n    loadType: 1\n    sampleRateSetting: 0\n    sampleRateOverride: 44100\n"
            f"    compressionFormat: {1 if long else 2}\n    quality: {0.7 if long else 1}\n    conversionMode: 0\n    preloadAudioData: 1\n"
            "  platformSettingOverrides: {}\n  forceToMono: 1\n  normalize: 0\n  loadInBackground: 0\n  ambisonic: 0\n  3D: 0\n"
            "  userData: \n  assetBundleName: \n  assetBundleVariant: \n")


def pcm(x):
    return np.round(np.clip(x, -1, 1) * 32767).astype("<i2")


def render(entry, v):
    rng = np.random.default_rng(bank.seed(entry["id"], v))
    x = np.asarray(entry["fn"](rng, v), float)
    if not np.all(np.isfinite(x)) or dsp.peak(x) < 1e-6:
        raise ValueError(f"{entry['id']} variant {v} rendered silence or NaN")
    return pcm(dsp.master(x, entry["lufs"], trim=-54 if entry["cat"] in ("flow", "boss", "loot") else -46))


def write_wav(path, data):
    with wave.open(str(path), "wb") as w:
        w.setnchannels(1); w.setsampwidth(2); w.setframerate(dsp.SR); w.writeframes(data.tobytes())


def read_wav(path):
    with wave.open(str(path), "rb") as w:
        return np.frombuffer(w.readframes(w.getnframes()), "<i2")


def stats(data):
    x = data.astype(float) / 32767
    return {"seconds": round(len(x) / dsp.SR, 3), "peakDb": round(20 * np.log10(max(1e-9, dsp.peak(x))), 2),
            "loudness": round(float(dsp.loudness(x)), 1), "dc": round(float(np.mean(x)), 5),
            "clipped": int(np.sum(np.abs(data) >= 32767)), "edges": [int(data[0]), int(data[-1])],
            "sha256": hashlib.sha256(data.tobytes()).hexdigest()}


def manifest(entries, clips):
    sounds = []
    for e in entries:
        sounds.append({"id": e["id"], "ko": e["ko"], "en": e["en"], "category": e["cat"], "clips": [c for c, _ in clips[e["id"]]],
                       "volume": e["gain"], "pitch": e["pitch"], "cooldown": e["cooldown"], "priority": e["priority"],
                       "limit": e["limit"], "group": e["group"]})
    return {"revision": bank.REVISION, "sampleRate": dsp.SR, "sounds": sounds}


CODE_ID = re.compile(r'(?:Fx|Play|PlayLater)\(\s*"([a-z][A-Za-z0-9_.]*[A-Za-z0-9_])"\s*[,)]')


def code_ids():
    found = {}
    for path in CODE.rglob("*.cs"):
        for m in CODE_ID.finditer(path.read_text(encoding="utf-8")):
            found.setdefault(m.group(1), path.relative_to(ROOT).as_posix())
    return found


def dynamic_ids():
    """Ids the runtime composes from game data (kept in sync with GameAudio.Routes)."""
    ids = set()
    for mat in recipes_system.MATERIALS:
        ids |= {"item.pick." + mat, "item.equip." + mat}
    for eid in recipes_world.ENEMIES:
        ids |= {f"enemy.{eid}.{k}" for k in ("windup", "attack", "death")}
    for c in recipes_heroes.VOICE:
        ids |= {f"vo.{c}.{k}" for k in ("effort", "big", "battlecry", "hurt", "death", "lowhp")} | {"char.select." + c}
    skills = json.loads((ROOT / "Assets/HELLSCRIPT/Resources/Data/ClassSkills.json").read_text(encoding="utf-8"))
    for skill in skills["skills"] if isinstance(skills, dict) else skills:
        if skill.get("category") in ("active", "ultimate"):
            ids.add("skill." + skill["id"])
    for b in recipes_world.BOSSES:
        ids.add(f"boss.{b}.spawn")
    for k in recipes_world.BOSS_ATTACKS:
        ids |= {f"boss.{k}.windup", f"boss.{k}"}
    return ids


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--check", action="store_true")
    ap.add_argument("--only", default="")
    ap.add_argument("--sheets", default="")
    ap.add_argument("--audition", default="")
    ap.add_argument("--audit", default="", help="write per-clip peak, loudness, DC, clipping and hash JSON")
    args = ap.parse_args()
    entries = [e for e in bank.REG.values() if e["id"].startswith(args.only)]
    clips, rendered, problems = {}, {}, []
    for e in entries:
        clips[e["id"]] = []
        for v in range(e["variants"]):
            data = render(e, v)
            name = clip_name(e["id"], v)
            clips[e["id"]].append((name, data))
            rendered[name] = (e, data)
    audit = {}
    for name, (e, data) in rendered.items():
        s = stats(data)
        audit[name] = s
        if s["clipped"] or abs(s["dc"]) > 0.01 or s["edges"] != [0, 0] or s["peakDb"] > -0.9:
            problems.append(f"{name}: {s}")
    bank_json = manifest(entries, {k: [(n, d) for n, d in v] for k, v in clips.items()})
    known = set(bank.REG)
    for sid, where in code_ids().items():
        if sid not in known:
            problems.append(f"code references unknown sound '{sid}' ({where})")
    for sid in dynamic_ids() - known:
        problems.append(f"runtime route has no sound '{sid}'")

    if args.check:
        for name, (e, data) in rendered.items():
            path = OUT / e["cat"] / (name + ".wav")
            if not path.exists() or not np.array_equal(read_wav(path), data):
                problems.append(f"{path.relative_to(ROOT)} differs from its recipe")
        on_disk = json.loads((OUT / "bank.json").read_text(encoding="utf-8")) if (OUT / "bank.json").exists() else None
        if not args.only and on_disk != bank_json:
            problems.append("bank.json differs from the registry")
    if not args.check and not args.sheets and not args.audition and not args.audit:
        OUT.mkdir(parents=True, exist_ok=True)
        for p in (OUT.parent, OUT):
            folder_meta(p)
        keep = set()
        for name, (e, data) in rendered.items():
            folder = OUT / e["cat"]
            folder.mkdir(exist_ok=True)
            folder_meta(folder)
            path = folder / (name + ".wav")
            write_wav(path, data)
            meta = path.with_name(path.name + ".meta")
            if not meta.exists():
                meta.write_text(audio_meta(path, len(data) / dsp.SR))
            keep.add(path)
        if not args.only:
            for old in OUT.rglob("*.wav"):
                if old not in keep:
                    old.unlink(); old.with_name(old.name + ".meta").unlink(missing_ok=True)
            for folder in [p for p in OUT.iterdir() if p.is_dir()]:
                if not any(folder.iterdir()):
                    folder.rmdir(); folder.with_name(folder.name + ".meta").unlink(missing_ok=True)
            (OUT / "bank.json").write_text(json.dumps(bank_json, ensure_ascii=False, indent=1) + "\n", encoding="utf-8")
            meta = OUT / "bank.json.meta"
            if not meta.exists():
                meta.write_text(f"fileFormatVersion: 2\nguid: {guid('bank.json')}\nTextScriptImporter:\n  externalObjects: {{}}\n"
                                "  userData: \n  assetBundleName: \n  assetBundleVariant: \n")
    if args.audit:
        Path(args.audit).write_text(json.dumps({"revision": bank.REVISION, "clips": audit}, indent=1) + "\n", encoding="utf-8")
    if args.sheets:
        sheets(Path(args.sheets), entries, clips)
    if args.audition:
        audition(Path(args.audition), entries, clips)
    total = sum(len(d) for _, (_, d) in rendered.items()) * 2
    print(f"{len(entries)} sounds, {len(rendered)} clips, {total / 1e6:.1f} MB PCM, {len(problems)} problems")
    for p in problems:
        print("  " + p)
    return 1 if problems else 0


def sheets(folder, entries, clips):
    import matplotlib
    matplotlib.use("Agg")
    import matplotlib.pyplot as plt
    from scipy import signal
    folder.mkdir(parents=True, exist_ok=True)
    rows = [(e["id"], clips[e["id"]][0][1]) for e in entries]
    for page in range(0, len(rows), 12):
        chunk = rows[page:page + 12]
        fig, axs = plt.subplots(len(chunk), 1, figsize=(11, 1.55 * len(chunk)))
        for ax, (sid, data) in zip(np.atleast_1d(axs), chunk):
            x = data.astype(float) / 32767
            f, t, Z = signal.stft(x, fs=dsp.SR, nperseg=1024, noverlap=768)
            D = 20 * np.log10(np.abs(Z) + 1e-9)
            ax.pcolormesh(t, f, D, vmin=D.max() - 80, vmax=D.max(), shading="auto", cmap="magma")
            env = np.abs(x)
            ax.plot(np.arange(len(x))[::200] / dsp.SR, env[::200] * 12000, color="cyan", lw=0.4)
            ax.set_ylim(0, 12000); ax.set_yticks([]); ax.set_title(f"{sid}  {len(x) / dsp.SR:.2f}s", fontsize=7, loc="left")
            ax.tick_params(labelsize=6)
        plt.tight_layout(); plt.savefig(folder / f"sheet_{page // 12:02d}.png", dpi=62); plt.close()


def audition(folder, entries, clips):
    """Static listening page: every variant, grouped by category."""
    folder.mkdir(parents=True, exist_ok=True)
    (folder / "clips").mkdir(exist_ok=True)
    groups = {}
    for e in entries:
        groups.setdefault(e["cat"], []).append(e)
        for name, data in clips[e["id"]]:
            write_wav(folder / "clips" / (name + ".wav"), data)
    parts = []
    for cat, items in groups.items():
        parts.append(f"<h2>{html.escape(cat)}</h2><table>")
        for e in items:
            buttons = "".join(f'<button data-src="clips/{n}.wav">{i + 1}</button>' for i, (n, _) in enumerate(clips[e["id"]]))
            parts.append(f"<tr><td><code>{html.escape(e['id'])}</code></td><td>{html.escape(e['ko'])}<br><small>{html.escape(e['en'])}</small></td><td>{buttons}</td></tr>")
        parts.append("</table>")
    page = ("<!doctype html><meta charset=utf-8><title>HELLSCRIPT SFX audition</title><style>body{font:14px system-ui;background:#141210;color:#e9dcc3;margin:24px}"
            "table{border-collapse:collapse;width:100%}td{border-bottom:1px solid #3a322a;padding:6px}button{margin:2px;background:#3b2f22;color:#f3e2bf;border:1px solid #8a6a3c;"
            "border-radius:4px;padding:4px 10px;cursor:pointer}code{color:#e0a95e}</style><h1>HELLSCRIPT SFX</h1>" + "".join(parts) +
            "<script>const a=new Audio();document.addEventListener('click',e=>{const s=e.target.dataset.src;if(s){a.src=s;a.currentTime=0;a.play();}});</script>")
    (folder / "index.html").write_text(page, encoding="utf-8")


if __name__ == "__main__":
    sys.exit(main())
