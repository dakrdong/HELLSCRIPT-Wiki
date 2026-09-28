#!/usr/bin/env python3
"""Offline import and provenance checks for authorized ElevenLabs game audio.

This tool never authenticates, calls a generation API, or spends credits.
Original selected MP3s are retained; WAV conversion does not restore lost source detail.
"""
import argparse
import hashlib
import json
import re
import shutil
import subprocess
import sys
import tempfile
import wave
from pathlib import Path

import numpy as np
from scipy import signal

ROOT = Path(__file__).resolve().parents[1]
MANIFEST = ROOT / "AudioSources/ElevenLabs/manifest.json"
SOURCE_DIR = MANIFEST.parent / "Sources"
SFX = ROOT / "Assets/HELLSCRIPT/Resources/Audio/Sfx"
MUSIC = ROOT / "Assets/HELLSCRIPT/Resources/Audio/Music"
sys.path.insert(0, str(ROOT / "tools/sfx"))
import dsp
import bank

PROCESSING_VERSION = "elevenlabs-game-master-v1"
MODEL_IDS = {"sfx": "eleven_text_to_sound_v2", "music": "eleven_music_v2"}
SFX_EDIT_PROFILES = {"ui.slider": "single-tick-v1", "ui.tab": "immediate-tab-v2"}


def sha256(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def read_manifest():
    if not MANIFEST.exists(): return {"revision": PROCESSING_VERSION, "assets": {}}
    document = json.loads(MANIFEST.read_text(encoding="utf-8"))
    if document.get("revision") != PROCESSING_VERSION: raise ValueError("Unsupported audio provenance revision")
    return document


def rooted(relative):
    path = (ROOT / relative).resolve()
    if not path.is_relative_to(ROOT): raise ValueError("Audio manifest path leaves project")
    return path


def validate_record(asset, record):
    problems = []
    try:
        target = rooted(asset); source = rooted(record["source"])
        kind = record["kind"]
        if kind not in MODEL_IDS or record.get("model_id") != MODEL_IDS[kind] or record.get("provider") != "ElevenLabs": problems.append("Unverified provider or model identity")
        if not target.is_relative_to(SFX if kind == "sfx" else MUSIC): problems.append("Unexpected runtime audio path")
        if not source.is_relative_to(SOURCE_DIR): problems.append("Unexpected archived source path")
        if not record.get("generation_id") or not record.get("prompt") or not record.get("parameters"): problems.append("Incomplete generation provenance")
        if kind == "sfx" and record.get("processing", {}).get("edit_profile") != SFX_EDIT_PROFILES.get(record.get("cue")): problems.append("Stale cue editing profile")
        if not source.is_file() or sha256(source) != record.get("source_sha256"): problems.append("Original source missing or changed")
        if not target.is_file() or sha256(target) != record.get("asset_sha256"): problems.append("Runtime audio missing or changed")
        if target.is_file():
            with wave.open(str(target), "rb") as stream:
                channels = 1 if kind == "sfx" else 2
                if (stream.getnchannels(), stream.getsampwidth(), stream.getframerate()) != (channels, 2, 48000): problems.append("Runtime audio must be 48 kHz PCM16 with the expected channel count")
                if stream.getnframes() == 0: problems.append("Runtime audio is empty")
                if abs(stream.getnframes() / stream.getframerate() - record.get("seconds", -1)) > 1/48000: problems.append("Recorded duration differs from runtime audio")
    except (KeyError, ValueError, OSError, wave.Error) as error:
        problems.append(str(error))
    return problems


def decode(path, channels):
    ffmpeg = shutil.which("ffmpeg")
    if not ffmpeg: raise RuntimeError("ffmpeg is required for offline audio import")
    result = subprocess.run([ffmpeg, "-v", "error", "-i", str(path), "-f", "f32le", "-acodec", "pcm_f32le", "-ar", "48000", "-ac", str(channels), "pipe:1"], capture_output=True, check=True)
    samples = np.frombuffer(result.stdout, "<f4").reshape(-1, channels).astype(float)
    if not len(samples) or not np.all(np.isfinite(samples)) or np.max(np.abs(samples)) < 1e-5:
        raise ValueError("Source is empty, non-finite, or silent")
    return samples


def master_sfx(samples, target_loudness, cue=None):
    audio = dsp.hp(samples[:, 0], 28, 2)
    source_start = 0
    profile = SFX_EDIT_PROFILES.get(cue)
    if profile:
        envelope = np.sqrt(np.mean(audio[:len(audio)//48*48].reshape(-1, 48)**2, axis=1))
        if not len(envelope) or envelope.max() < 1e-5: raise ValueError("No usable UI transient")
        if cue == "ui.slider":
            # A generated ratchet can contain several clicks. Keep one 30 ms tick,
            # shorter than the existing 50 ms slider cooldown, without resynthesis.
            source_start = max(0, int(np.argmax(envelope))*48-240)
            audio = audio[source_start:source_start+1440]
        else:
            # Quiet page noise must not delay the tab's tactile attack.
            source_start = max(0, int(np.flatnonzero(envelope >= envelope.max()*.4)[0])*48-144)
            audio = audio[source_start:]
    active = np.flatnonzero(np.abs(audio) > max(.00002, np.max(np.abs(audio)) * .002))
    if not len(active): raise ValueError("No usable SFX signal")
    start = max(0, int(active[0]) - 144)
    end = min(len(audio), int(active[-1]) + 1440)
    audio = dsp.fade(audio[start:end], .0003, min(.025, (end-start)/48000*.2))
    loudness = float(dsp.loudness(audio))
    peak = float(np.max(np.abs(audio)))
    gain_db = min(target_loudness-loudness, -2-20*np.log10(peak), 24)
    audio *= 10**(gain_db/20)
    true_peak = float(np.max(np.abs(signal.resample_poly(audio, 4, 1))))
    if true_peak > 10**(-2/20): audio *= 10**(-2/20)/true_peak
    audio[0] = audio[-1] = 0
    processing = {"trim_start_samples": source_start+start, "trim_end_samples": len(samples)-source_start-end, "gain_db": round(float(gain_db), 6), "target_short_term_loudness": target_loudness, "ceiling_dbtp": -2, "nonlinear_limiting": False}
    if profile: processing["edit_profile"] = profile
    return audio[:, None], processing


def write_pcm(path, audio):
    with wave.open(str(path), "wb") as stream:
        stream.setnchannels(audio.shape[1]); stream.setsampwidth(2); stream.setframerate(48000)
        stream.writeframes(np.round(np.clip(audio,-1,1)*32767).astype("<i2").tobytes())


def master_music(samples, crossfade_seconds, target_lufs):
    frames = int(round(crossfade_seconds*48000))
    if len(samples) < frames*4: raise ValueError("Music is too short for the requested loop splice")
    audio = signal.sosfilt(signal.butter(2, 28, fs=48000, btype="highpass", output="sos"), samples, axis=0)
    weight = (.5-.5*np.cos(np.linspace(0,np.pi,frames)))[:, None]
    splice = audio[-frames:]*(1-weight)+audio[:frames]*weight
    audio = np.concatenate([audio[frames:-frames], splice])
    # Both joins are contiguous: the final splice approaches source[frames-1], then wraps to source[frames].
    with tempfile.TemporaryDirectory(prefix="hellscript-music-") as temp:
        scan_path = Path(temp)/"scan.wav"
        safety = min(1, .98/float(np.max(np.abs(audio))))
        write_pcm(scan_path, audio*safety)
        scan = subprocess.run([shutil.which("ffmpeg"), "-hide_banner", "-i", str(scan_path), "-af", "loudnorm=I="+str(target_lufs)+":TP=-2:LRA=11:print_format=json", "-f", "null", "-"], capture_output=True, text=True, check=True)
        measured = json.JSONDecoder().raw_decode(scan.stderr[scan.stderr.rfind("{"):])[0]
        gain_db = min(target_lufs-float(measured["input_i"]), -2.5-float(measured["input_tp"]))
        audio *= safety * 10**(gain_db/20)
    return audio, {"crossfade_seconds":crossfade_seconds, "loop_boundary":"continuous source samples", "target_integrated_lufs":target_lufs, "gain_db_after_safety":round(gain_db,6), "safety_gain":safety, "source_lufs_after_safety":float(measured["input_i"]), "ceiling_dbtp_before_encoding":-2.5, "listening_review":"pending"}


def adopt(index_path, selection_path):
    index = json.loads(index_path.read_text())
    by_id = {x["generation_id"]:x for x in index}
    selection = json.loads(selection_path.read_text())
    document = read_manifest()
    catalog = {s["id"]:s for s in json.loads((SFX/"bank.json").read_text())["sounds"]}
    wanted = []
    for cue, ids in selection["sfx"].items():
        entry=catalog[cue]
        if bank.REG[cue].get("sharedCue"):
            raise ValueError("Shared cues reuse a final master and cannot import duplicate audio: "+cue)
        if len(ids)!=len(entry["clips"]) or len(set(ids))!=len(ids): raise ValueError("Each runtime variation requires a distinct source: "+cue)
        for name, source_id in zip(entry["clips"], ids):
            wanted.append((cue,"sfx",SFX/entry["category"]/(name+".wav"),by_id[source_id],{}))
    for cue, choice in selection.get("music",{}).items():
        if cue not in {"title","sanctuary","rift","boss"}: raise ValueError("Unknown music context")
        wanted.append(("music."+cue,"music",MUSIC/("music_"+cue+".wav"),by_id[choice["generation_id"]],choice))
    # Preflight every selected input before replacing even one runtime asset.
    for cue,kind,target,record,choice in wanted:
        source=index_path.parent/record["file"]
        if record["cue"]!=cue or record["model_id"]!=MODEL_IDS[kind]: raise ValueError("Source identity mismatch: "+cue)
        if not re.fullmatch(r"[A-Za-z0-9_-]{1,128}", record["generation_id"]): raise ValueError("Invalid source generation identifier")
        if not source.is_file() or sha256(source)!=record["sha256"]: raise ValueError("Source hash mismatch: "+cue)
    SOURCE_DIR.mkdir(parents=True,exist_ok=True)
    for cue,kind,target,record,choice in wanted:
        key=target.relative_to(ROOT).as_posix()
        previous=document["assets"].get(key)
        if (previous and previous["generation_id"]==record["generation_id"]
            and previous.get("selection",{})==choice
            and (kind!="sfx" or previous["processing"].get("target_short_term_loudness")==bank.REG[cue]["lufs"])
            and not validate_record(key,previous)): continue
        source=index_path.parent/record["file"]
        archived=SOURCE_DIR/(record["generation_id"]+".mp3")
        if archived.exists() and sha256(archived)!=record["sha256"]: raise ValueError("Archived original changed")
        if not archived.exists(): shutil.copyfile(source,archived)
        samples=decode(source,1 if kind=="sfx" else 2)
        audio,processing=master_sfx(samples,bank.REG[cue]["lufs"],cue) if kind=="sfx" else master_music(samples,choice.get("crossfade_seconds",2.5),choice.get("target_lufs",-21))
        target.parent.mkdir(parents=True,exist_ok=True)
        with tempfile.TemporaryDirectory(prefix="hellscript-audio-") as temp:
            pcm=Path(temp)/"master.wav";write_pcm(pcm,audio)
            output=pcm
            # Unity applies the sole runtime Vorbis encode; do not add another lossy intermediate.
            staging=ROOT/"Artifacts/ElevenLabs/ImportStaging"/target.name
            staging.parent.mkdir(parents=True,exist_ok=True)
            shutil.copyfile(output,staging);staging.replace(target)
        document["assets"][key]={"cue":cue,"kind":kind,"provider":"ElevenLabs","model_id":record["model_id"],"generation_id":record["generation_id"],"prompt":record["prompt"],"parameters":record["parameters"],"source":archived.relative_to(ROOT).as_posix(),"source_codec":record["codec"],"source_sample_rate":record["sample_rate"],"source_channels":record["channels"],"source_sha256":record["sha256"],"asset_sha256":sha256(target),"processing":processing,"selection":choice,"seconds":round(len(audio)/48000,6),"listening_review":"pending"}
        temporary=MANIFEST.with_suffix('.tmp');temporary.write_text(json.dumps(document,ensure_ascii=False,indent=2)+'\n');temporary.replace(MANIFEST)
        print(json.dumps({"imported":key,"seconds":round(len(audio)/48000,3)}),flush=True)
    bank_path=SFX/"bank.json"
    bank_data=json.loads(bank_path.read_text(encoding="utf-8"))
    bank_data["sourceManifest"]=MANIFEST.relative_to(ROOT).as_posix()
    bank_path.write_text(json.dumps(bank_data,ensure_ascii=False,indent=1)+'\n',encoding="utf-8")
    return document


def rebuild():
    """Reproduce selected masters from committed sources, without account access or credits."""
    document=read_manifest()
    catalog=json.loads((SFX/'bank.json').read_text())['sounds']
    selection={'sfx':{},'music':{}};index=[]
    for entry in catalog:
        if bank.REG[entry['id']].get('sharedCue'):continue
        keys=[(SFX/entry['category']/(name+'.wav')).relative_to(ROOT).as_posix() for name in entry['clips']]
        records=[document['assets'][key] for key in keys if key in document['assets']]
        if not records:raise ValueError('Final cue has no adopted source: '+entry['id'])
        if len(records)!=len(keys):raise ValueError('Cannot rebuild a partially adopted cue: '+entry['id'])
        selection['sfx'][entry['id']]=[record['generation_id'] for record in records]
    for asset,record in document['assets'].items():
        index.append({'cue':record['cue'],'generation_id':record['generation_id'],'model_id':record['model_id'],
                      'prompt':record['prompt'],'parameters':record['parameters'],'file':str(rooted(record['source'])),
                      'sha256':record['source_sha256'],'codec':record['source_codec'],
                      'sample_rate':record['source_sample_rate'],'channels':record['source_channels']})
        if record['kind']=='music':selection['music'][record['cue'].removeprefix('music.')]=record['selection']
    with tempfile.TemporaryDirectory(prefix='hellscript-audio-selection-') as directory:
        index_path=Path(directory)/'index.json';selection_path=Path(directory)/'selection.json'
        index_path.write_text(json.dumps(index));selection_path.write_text(json.dumps(selection))
        return adopt(index_path,selection_path)


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--index',type=Path);parser.add_argument('--selection',type=Path);parser.add_argument('--check',action='store_true')
    parser.add_argument('--rebuild',action='store_true',help='Reproduce selected masters from the committed source manifest; no generation or API calls')
    args=parser.parse_args()
    if args.rebuild:
        if args.check or args.index or args.selection:parser.error('--rebuild cannot be combined with another operation')
        document=rebuild();print(json.dumps({'assets':len(document['assets']),'source':'committed originals','generation_calls':0}));return 0
    if args.check:
        document=read_manifest();problems=[]
        for asset,record in document['assets'].items(): problems.extend(asset+': '+p for p in validate_record(asset,record))
        catalog=json.loads((SFX/'bank.json').read_text())['sounds']
        expected={(SFX/bank.source_entry(bank.REG[entry['id']])['cat']/(name+'.wav')).relative_to(ROOT).as_posix() for entry in catalog for name in entry['clips']}
        expected.update((MUSIC/('music_'+context+'.wav')).relative_to(ROOT).as_posix() for context in ('title','sanctuary','rift','boss'))
        missing=sorted(expected-document['assets'].keys())
        for asset in missing:problems.append('Missing final asset: '+asset)
        for asset in sorted(document['assets'].keys()-expected):problems.append('Unexpected adopted asset: '+asset)
        actual={p.relative_to(ROOT).as_posix() for folder in (SFX,MUSIC) for p in folder.rglob('*.wav')}
        for asset in sorted(actual-expected):problems.append('Unreferenced runtime audio: '+asset)
        sources={rooted(record['source']) for record in document['assets'].values()}
        for source in sorted(set(SOURCE_DIR.glob('*.mp3'))-sources):problems.append('Unreferenced retained original: '+str(source.relative_to(ROOT)))
        generations=[record.get('generation_id') for record in document['assets'].values()]
        if len(set(generations)) != len(generations):problems.append('A source generation was reused for distinct runtime variations')
        if not document['assets']:problems.append('No adopted assets to validate')
        print(json.dumps({'assets':len(document['assets']),'expected_assets':len(expected),'complete':not missing and not problems,'missing_final_assets':missing,'problems':problems},ensure_ascii=False,indent=2))
        return bool(problems)
    if not args.index or not args.selection:parser.error('--index and --selection are required for import')
    adopt(args.index,args.selection)
    return 0


if __name__=='__main__':sys.exit(main())
