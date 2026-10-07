from datetime import datetime, timezone
from pathlib import Path
from xml.etree import ElementTree as ET
import hashlib
import json
import re
import shutil

root = Path(__file__).resolve().parents[3]
out = Path(__file__).resolve().parent
inputs = {2: "stage3-l2-final.xml", 5: "stage3-tuning-one.xml", 7: "stage3-l2-l7-final.xml"}
pattern = re.compile(r"PUZZLE L(\d+) (\w+) ([\w-]+): (WIN|DEAD|TIMEOUT) hp=([\d.]+)/([\d.]+) \(([\d.]+) %\) time=([\d.]+)s E07=(\d+) escape=(\d+)")
rows = []
for level, filename in list(inputs.items()) + [(5, "stage3-l5-style.xml")]:
    for case in ET.parse(out / filename).getroot().iter("test-case"):
        match = pattern.search(case.findtext("output", ""))
        if not match or int(match[1]) != level:
            continue
        rows.append(dict(level=level, hero=match[2], policy=match[3], expected="WIN" if match[3].startswith("win-") else "DEAD",
                         outcome=match[4], hp=float(match[5]), maxHp=float(match[6]), hpPercent=float(match[7]), seconds=float(match[8]),
                         gravityReleases=int(match[9]), movementActions=int(match[10]), testResult=case.get("result"),
                         report=filename, test=case.get("fullname"), message=match[0]))
assert len(rows) == 70 and len({(r["level"], r["hero"], r["policy"]) for r in rows}) == 70
assert all(r["testResult"] in ("Passed", "Failed") for r in rows)
rows.sort(key=lambda r: (r["level"], ["Warrior", "Ranger", "Mage"].index(r["hero"]), r["policy"]))
integration = ET.parse(out / "stage3-integration.xml").getroot()
assert int(integration.get("failed")) == 0 and int(integration.get("total")) == 25
source = root / "Assets/HELLSCRIPT/Tests/Editor/PuzzleLevelTests.cs"
evidence = out / "stage3-evidence"
evidence.mkdir(exist_ok=True)
samples = []
for row in rows:
    matches = [p for p in (out / "prototype-saves").glob("*/result.txt") if p.read_text().strip() == row["message"].removeprefix("PUZZLE ")]
    original = max(matches, key=lambda p: p.stat().st_mtime).with_name("state.json")
    account = json.loads(original.with_name("hellscript-local-v1.json").read_text())
    hero = account["heroes"][account["selectedHero"]]
    row["savedGlobals"] = {o["id"]: o["value"] for o in hero["edict"]["global"] if o["id"] in ["survival.potionHpPercent", "survival.lowHpPercent", "survival.returnHpPercent", "dodge.area.policy", "dodge.direct.policy", "position.mode"]}
    row["originalSave"] = str(original.parent)
    if (row["level"], row["hero"], row["policy"]) not in [(2, "Warrior", "position-EDGE"), (5, "Ranger", "win-careful"), (5, "Mage", "win-careful"),
                                                          (5, "Warrior", "win-careful-after-aggressive"), (5, "Ranger", "win-careful-after-aggressive"), (5, "Mage", "win-careful-after-aggressive")]:
        continue
    dest = evidence / f"L{row['level']}-{row['hero']}-{row['policy']}.json"
    shutil.copyfile(original, dest)
    state = json.loads(dest.read_text())
    samples.append(dict(case=row["test"], original=str(original), path=str(dest), sha256=hashlib.sha256(dest.read_bytes()).hexdigest(),
                        finalResponse=state["edictResponse"], potions=[s for s in state["logs"] if "[POTION]" in s],
                        lastAttack=max((a["time"] for a in state["actionEvents"] if a["kind"] == "ACTION_START"), default=None)))
summary = dict(createdAt=datetime.now(timezone.utc).isoformat(), sourceBase="add270cb", sourceSha256=hashlib.sha256(source.read_bytes()).hexdigest(),
               unity="6000.6.0f1", mode="EditMode", seed=77123, fullSuiteRun=False, runtimeSmokeRun=False, merged=False, publicWikiPublished=False,
               gate="FAILED: L2 additional valid movement answer; L5 retreat/recovery mismatch and ineffective wrong choices",
               matrix=dict(total=70, passed=sum(r["testResult"] == "Passed" for r in rows), failed=sum(r["testResult"] == "Failed" for r in rows), skipped=0),
               integration={k: int(integration.get(k)) for k in ["total", "passed", "failed", "skipped"]},
               reuse=[dict(scope="L5 authored starting kit", report=inputs[5], reason="Stats, learned skills and policies unchanged; the added inherited-style branch does not execute for these original cases."),
                      dict(scope="L7", report=inputs[7], reason="L7 fixture and saved policies unchanged; subsequent edits affect L2 warnings/preset naming and add a separate inherited-style L5 branch.")],
               rows=rows, samples=samples)
(out / "stage3-validation.json").write_text(json.dumps(summary, ensure_ascii=False, indent=2) + "\n")
print(json.dumps(dict(matrix=summary["matrix"], integration=summary["integration"], samples=len(samples)), ensure_ascii=False))
