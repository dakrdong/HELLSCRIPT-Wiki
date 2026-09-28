#!/usr/bin/env python3
"""Validate and preview the final HELLSCRIPT sound bank without generating audio.

    python3 tools/generate_sfx.py --check
    python3 tools/generate_sfx.py --write-bank   # update metadata only
    python3 tools/generate_sfx.py --audition Artifacts/Audio

The historical command name is retained. Retired synthesis cannot be invoked.
Use elevenlabs_audio.py --rebuild to restore a final master from its retained source.
"""
import argparse
import hashlib
import html
import json
import re
import sys
import wave
from pathlib import Path

import numpy as np

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / "tools/sfx"))
import dsp
import bank
import elevenlabs_audio

OUT = ROOT / "Assets/HELLSCRIPT/Resources/Audio/Sfx"
CODE = ROOT / "Assets/HELLSCRIPT/Runtime"


def clip_name(sid, v):
    return re.sub(r"[^A-Za-z0-9]+", "_", sid) + f"_{v + 1}"


def read_wav(path):
    with wave.open(str(path), "rb") as stream:
        if (stream.getnchannels(), stream.getsampwidth(), stream.getframerate()) != (1, 2, dsp.SR):
            raise ValueError("Invalid SFX format: " + str(path))
        return np.frombuffer(stream.readframes(stream.getnframes()), "<i2")


def stats(data):
    x = data.astype(float) / 32767
    return {"seconds": round(len(x) / dsp.SR, 3), "peakDb": round(20 * np.log10(max(1e-9, dsp.peak(x))), 2),
            "loudness": round(float(dsp.loudness(x)), 1), "dc": round(float(np.mean(x)), 5),
            "clipped": int(np.sum(np.abs(data) >= 32767)), "edges": [int(data[0]), int(data[-1])],
            "sha256": hashlib.sha256(data.tobytes()).hexdigest()}


def manifest(entries, clips):
    sounds = []
    for e in entries:
        row = {"id": e["id"], "ko": e["ko"], "en": e["en"], "category": e["cat"],
               "clips": [name for name, _ in clips[e["id"]]], "volume": e["gain"], "pitch": e["pitch"],
               "cooldown": e["cooldown"], "priority": e["priority"], "limit": e["limit"], "group": e["group"]}
        if e.get("sharedCue"):
            row["sharedCue"] = e["sharedCue"]
        sounds.append(row)
    return {"revision": bank.REVISION, "sampleRate": dsp.SR, "sounds": sounds,
            "sourceManifest": elevenlabs_audio.MANIFEST.relative_to(ROOT).as_posix()}


CODE_ID = re.compile(r'(?:Fx|Play|PlayLater)\(\s*"([a-z][A-Za-z0-9_.]*[A-Za-z0-9_])"\s*[,)]')


def code_ids():
    found = {}
    for path in CODE.rglob("*.cs"):
        for match in CODE_ID.finditer(path.read_text(encoding="utf-8")):
            found.setdefault(match.group(1), path.relative_to(ROOT).as_posix())
    return found


def dynamic_ids():
    ids = set()
    for material in bank.ROUTES["materials"]:
        ids |= {"item.pick." + material, "item.equip." + material}
    for enemy in bank.ROUTES["enemies"]:
        ids |= {f"enemy.{enemy}.{part}" for part in ("windup", "attack", "death")}
    for hero in bank.ROUTES["heroes"]:
        ids |= {f"vo.{hero}.{part}" for part in ("effort", "big", "battlecry", "hurt", "death", "lowhp")} | {"char.select." + hero}
    skills = json.loads((ROOT / "Assets/HELLSCRIPT/Resources/Data/ClassSkills.json").read_text(encoding="utf-8"))
    for skill in skills["skills"] if isinstance(skills, dict) else skills:
        if skill.get("category") in ("active", "ultimate"):
            ids.add("skill." + skill["id"])
    for boss in bank.ROUTES["bosses"]:
        ids.add(f"boss.{boss}.spawn")
    for attack in bank.ROUTES["boss_attacks"]:
        ids |= {f"boss.{attack}.windup", f"boss.{attack}"}
    return ids


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--check", action="store_true")
    parser.add_argument("--write-bank", action="store_true", help="Write validated metadata without modifying any audio")
    parser.add_argument("--only", default="")
    parser.add_argument("--sheets", default="")
    parser.add_argument("--audition", default="")
    parser.add_argument("--audit", default="")
    args = parser.parse_args()
    if not (args.check or args.write_bank or args.sheets or args.audition or args.audit):
        print("Procedural generation is retired. Use --check or elevenlabs_audio.py --rebuild.")
        return 2
    if args.write_bank and args.only:
        parser.error("--write-bank must include the complete catalog")
    if not elevenlabs_audio.MANIFEST.exists():
        print("Final audio source provenance is missing; refusing to synthesize a replacement.")
        return 2
    entries = [e for e in bank.REG.values() if e["id"].startswith(args.only)]
    imported = elevenlabs_audio.read_manifest()["assets"]
    clips, rendered, problems = {}, {}, []
    for entry in entries:
        source = bank.source_entry(entry)
        clips[entry["id"]] = []
        for variant in range(source["variants"]):
            name = clip_name(source["id"], variant)
            path = OUT / source["cat"] / (name + ".wav")
            key = path.relative_to(ROOT).as_posix()
            if name not in rendered:
                record = imported.get(key)
                if not record:
                    problems.append(key + ": final source provenance is missing")
                else:
                    problems.extend(key + ": " + problem for problem in elevenlabs_audio.validate_record(key, record))
                    if record["cue"] != source["id"] or record["kind"] != "sfx":
                        problems.append(key + ": provenance refers to a different source cue")
                try:
                    data = read_wav(path)
                    if not len(data):
                        raise ValueError("Empty audio")
                except (OSError, ValueError, wave.Error) as error:
                    problems.append(key + ": " + str(error))
                    data = np.zeros(1, dtype="<i2")
                rendered[name] = (source, data)
            clips[entry["id"]].append((name, rendered[name][1]))
    audit = {}
    for name, (entry, data) in rendered.items():
        measured = stats(data)
        audit[name] = measured
        if measured["clipped"] or abs(measured["dc"]) > .01 or measured["edges"] != [0, 0] or not -80 <= measured["peakDb"] <= -.9:
            problems.append(f"{name}: {measured}")
    bank_json = manifest(entries, clips)
    known = set(bank.REG)
    for sid, where in code_ids().items():
        if sid not in known:
            problems.append(f"code references unknown sound '{sid}' ({where})")
    for sid in dynamic_ids() - known:
        problems.append(f"runtime route has no sound '{sid}'")
    if not args.only:
        expected = {OUT / entry["cat"] / (name + ".wav") for name, (entry, _) in rendered.items()}
        problems.extend("Unreferenced runtime audio: " + str(path.relative_to(ROOT)) for path in sorted(set(OUT.rglob("*.wav")) - expected))
    if args.check and not args.only:
        on_disk = json.loads((OUT / "bank.json").read_text(encoding="utf-8"))
        if on_disk != bank_json:
            problems.append("bank.json differs from the final catalog")
    if args.audit:
        Path(args.audit).write_text(json.dumps({"revision": bank.REVISION, "clips": audit}, indent=1) + "\n", encoding="utf-8")
    if not problems:
        if args.write_bank:
            (OUT / "bank.json").write_text(json.dumps(bank_json, ensure_ascii=False, indent=1) + "\n", encoding="utf-8")
        if args.sheets:
            sheets(Path(args.sheets), entries, clips)
        if args.audition:
            audition(Path(args.audition), entries, clips)
    total = sum(len(data) for _, data in rendered.values()) * 2
    print(f"{len(entries)} sounds, {len(rendered)} unique clips, {total / 1e6:.1f} MB PCM, {len(problems)} problems")
    for problem in problems:
        print("  " + problem)
    return int(bool(problems))


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
            source = bank.source_entry(e)
            target = (OUT / source["cat"] / (name + ".wav")).resolve()
            link = folder / "clips" / (name + ".wav")
            if link.is_symlink():
                link.unlink()
            elif link.exists():
                raise ValueError("Audition output already contains a regular audio file: " + str(link))
            link.symlink_to(target)
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
