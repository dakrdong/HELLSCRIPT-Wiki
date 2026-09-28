"""Final audio catalog. Metadata only; retired procedural sounds cannot be regenerated."""
import json
from pathlib import Path

CATALOG = Path(__file__).resolve().parents[2] / "AudioSources/ElevenLabs/sfx-catalog.json"
DOCUMENT = json.loads(CATALOG.read_text(encoding="utf-8"))
REVISION = DOCUMENT["revision"]
ROUTES = DOCUMENT["routes"]
REG = {entry["id"]: entry for entry in DOCUMENT["sounds"]}
if len(REG) != len(DOCUMENT["sounds"]):
    raise ValueError("Duplicate sound identifier")


def source_entry(entry):
    """Shared cues reference a final source directly, never a copy or an alias chain."""
    source = REG[entry.get("sharedCue", entry["id"])]
    if source.get("sharedCue"):
        raise ValueError("Shared audio must reference a source cue directly")
    if source["variants"] != entry["variants"]:
        raise ValueError("Shared audio variation count differs from its source")
    return source


for entry in REG.values():
    source_entry(entry)
