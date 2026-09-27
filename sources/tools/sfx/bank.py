"""Sound registry, shared rooms and mix helpers used by the recipe modules."""
import hashlib

import numpy as np

import dsp

REVISION = "hellscript-sfx-v1"
REG = {}

# category -> default (loudness target, cooldown, priority, simultaneous limit, pitch jitter)
CATEGORY = {
    "ui": (-25, 0.04, 70, 2, 0.02),
    "item": (-22, 0.05, 65, 2, 0.03),
    "economy": (-21, 0.05, 60, 3, 0.03),
    "town": (-19, 0.08, 75, 2, 0.02),
    "flow": (-15, 0.5, 95, 1, 0.0),
    "hero": (-19, 0.06, 55, 3, 0.04),
    "voice": (-19, 0.3, 85, 1, 0.03),
    "skill": (-17, 0.05, 60, 3, 0.02),
    "hit": (-23, 0.035, 25, 4, 0.06),
    "enemy": (-21, 0.08, 35, 3, 0.05),
    "boss": (-15, 0.2, 90, 2, 0.02),
    "field": (-19, 0.2, 70, 2, 0.02),
    "loot": (-19, 0.08, 75, 2, 0.02),
}


def sfx(sid, ko, en, cat, variants=1, lufs=None, cooldown=None, priority=None, limit=None, pitch=None, group="", gain=1.0):
    d = CATEGORY[cat]
    def deco(fn):
        if sid in REG:
            raise ValueError("duplicate sound id " + sid)
        REG[sid] = dict(id=sid, ko=ko, en=en, cat=cat, variants=variants,
                        lufs=d[0] if lufs is None else lufs, cooldown=d[1] if cooldown is None else cooldown,
                        priority=d[2] if priority is None else priority, limit=d[3] if limit is None else limit,
                        pitch=d[4] if pitch is None else pitch, group=group, gain=gain, fn=fn)
        return fn
    return deco


def seed(*parts):
    h = hashlib.sha256(("|".join(map(str, (REVISION,) + parts))).encode()).digest()
    return int.from_bytes(h[:8], "little")


# ------------------------------------------------------------------ rooms
_IRS = {}
ROOMS = {  # rt60, predelay, damping, early reflections, size, brightness
    "tiny": (0.18, 0.003, 0.7, 5, 0.3, 7000),
    "small": (0.45, 0.006, 0.6, 7, 0.6, 8000),
    "hall": (1.5, 0.014, 0.55, 9, 1.0, 7500),
    "crypt": (2.0, 0.02, 0.5, 10, 1.3, 6000),
    "vast": (2.8, 0.03, 0.45, 10, 1.8, 5000),
}


def ir(name):
    if name not in _IRS:
        rt, pre, damp, early, size, bright = ROOMS[name]
        _IRS[name] = dsp.make_ir(np.random.default_rng(seed("room", name)), rt, pre, damp, early, size, brightness=bright)
    return _IRS[name]


def room(x, name="hall", wet=0.2, lowcut=160):
    return dsp.reverb(x, ir(name), wet, 1.0, lowcut)


# ------------------------------------------------------------------ mix helpers
def mix(dur, *layers):
    """layers: (signal, gain) or (signal, gain, offset_seconds). The buffer grows to fit every layer."""
    need = max([dsp.n_of(dur)] + [int(round((l[2] if len(l) > 2 else 0.0) * dsp.SR)) + len(l[0]) for l in layers])
    out = np.zeros(need)
    for layer in layers:
        sig, gain = layer[0], layer[1]
        at = layer[2] if len(layer) > 2 else 0.0
        dsp.place(out, sig, at, gain)
    return out


def add(a, b):
    out = np.zeros(max(len(a), len(b)))
    out[:len(a)] += a
    out[:len(b)] += b
    return out


def jit(rng, v, spread=0.06):
    """Deterministic per-variant multiplier around 1."""
    return 1 + rng.uniform(-spread, spread)
