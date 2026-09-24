#!/usr/bin/env python3
"""Validate and summarize the production-core early-rift playthrough, without rerolling it."""
import argparse
from collections import Counter, defaultdict
import hashlib
import json
from pathlib import Path


def read_lines(path):
    return [json.loads(line) for line in path.read_text().splitlines() if line] if path.exists() else []


def summarize(directory, root):
    header = json.loads((directory / "summary.json").read_text())
    rows = read_lines(directory / "attempts.jsonl")
    actions = read_lines(directory / "actions.jsonl")
    boxes = read_lines(directory / "box-equipment.jsonl")
    initial = json.loads((directory / "initial-account.json").read_text())
    final = json.loads((directory / "final-account.json").read_text())
    hero = final["heroes"][final["selectedHero"]]
    target = header["target"]
    initial_hero = initial["heroes"][initial["selectedHero"]]
    proposal = json.loads((root / "Docs/Design/Balance1000/projection.json").read_text())["rows"][:target]
    assert header["status"] == "complete", header["status"]
    assert header["reloadVerified"]
    assert initial_hero["level"] == 1 and initial_hero["xp"] == 0
    assert initial_hero["highestClear"] == 0
    assert all(initial.get(k, 0) == 0 for k in ("gold", "materials", "enhancementStones", "premium"))
    assert len(rows) == header["attempts"]
    assert abs(sum(r["riftSeconds"] for r in rows) - header["riftSeconds"]) < 1e-6
    failures = [r for r in rows if r["outcome"] != "Cleared"]
    assert len(failures) == header["failures"]
    assert sum(r["outcome"] == "HeroDeath" for r in rows) == header["deaths"]
    assert abs(sum(r["riftSeconds"] for r in failures) - header["failedRiftSeconds"]) < 1e-6
    assert sorted(final["rewardBoxes"]["claimedStages"]) == list(range(1, target + 1))
    assert all(any(b["stage"] == stage and b["milliseconds"] > 0 for b in hero["riftProgress"]["best"]) for stage in range(1, target + 1))
    by_stage = defaultdict(list)
    acquired, generated, sources, resources = Counter(), Counter(), defaultdict(Counter), Counter()
    income, expense, town_sources = Counter(), Counter(), defaultdict(Counter)
    currency_keys = ("gold", "materials", "stones", "premium", "gems", "runes")
    seed_stages = []
    levels = []
    total_normal = total_elite = 0
    measurement_notes = []
    for r in rows:
        assert r["seed"] == header["seed"] + r["number"] * 1337
        assert r["stage"] <= r["before"]["highestClear"] + 1
        assert r["combatSeconds"] <= r["riftSeconds"] + .1  # float timer accumulation tolerance
        run = json.loads((directory / f"run-{r['number']:03}.json").read_text())
        # The initial observer counted dead=true on an escaped goblin as a kill. The saved
        # production run is authoritative: escape sets health=0/dead=true but grants no kill.
        if run.get("goblin", {}).get("escaped") and r["goblinKills"]:
            measurement_notes.append(f"Attempt {r['number']}: excluded the escaped goblin from the observer kill count; production kills, outcomes, time and rewards are unchanged.")
            r["goblinKills"] -= 1
        unknown = [item for item in r["equipment"] if item["source"] == "unknown"]
        if unknown:
            # BossClear places exactly three final drops beside the hero, rather than at the
            # dead boss. No new enemies or equipment drops are generated in the Looting phase.
            assert r["bossRewarded"] and len(unknown) == 3
            assert {i["id"] for i in unknown} == {i["id"] for i in run["drops"][-3:]}
            for item in unknown:
                item["source"] = "boss"
        assert run["kills"] == r["kills"]
        assert abs(run["riftAttendance"]["elapsedMs"] / 1000 - r["riftSeconds"]) < 1e-6
        assert sum((r["normalKills"], r["eliteKills"], r["addKills"], r["goblinKills"], int(r["bossRewarded"]))) == r["kills"]
        for item in r["equipment"]:
            rarity = ("common", "magic", "rare", "legendary")[item["item"]["rarity"]]
            generated[rarity] += 1
            if item["claimed"]:
                acquired[rarity] += 1
                sources[item["source"]][rarity] += 1
        for resource in r["resources"]:
            if resource["claimed"]:
                resources[("gold", "materials", "cores", "gems", "stones")[resource["kind"]]] += resource["amount"]
        for k in currency_keys:
            delta = r["afterCombat"]["wallet"][k] - r["before"]["wallet"][k]
            assert delta >= 0, (r["number"], k, delta)
            income[k] += delta
        start = r["afterCombat"]["wallet"]
        for action in (a for a in actions if a["attempt"] == r["number"]):
            current = action["after"]["wallet"]
            for k in currency_keys:
                delta = current[k] - start[k]
                if delta > 0:
                    income[k] += delta
                    town_sources[action["action"]][k] += delta
                if delta < 0:
                    expense[k] -= delta
                    town_sources[action["action"]]["spent_" + k] -= delta
            start = current
        assert all(start[k] == r["afterTown"]["wallet"][k] for k in currency_keys)
        by_stage[r["stage"]].append(r)
        total_normal += r["normalKills"]
        total_elite += r["eliteKills"]
        seed_stages.append({k: r[k] for k in ("number", "stage", "seed", "map", "objective", "outcome")})
        levels.extend({"attempt": r["number"], **g} for g in r["growth"])
    # Initial town actions can activate the standard starter potion stock; no currency is granted.
    assert all(a["after"]["wallet"] == header["initial"]["wallet"] for a in actions if a["attempt"] == 0)
    for k in currency_keys:
        assert header["initial"]["wallet"][k] + income[k] - expense[k] == header["final"]["wallet"][k], (k, income[k], expense[k])
    # Every acquired equipment object must still be owned or have a logged, successful disposal.
    acquired_ids = {i["item"]["id"] for r in rows for i in r["equipment"] if i["claimed"]}
    initial_ids = {i["id"] for i in initial_hero["inventory"]}
    box_ids = {i["id"] for i in boxes}
    disposed_ids = {a["detail"] for a in actions if a["action"] in ("sell", "salvage")}
    final_ids = {i["id"] for i in hero["inventory"]}
    # Crafted gear may have been equipped and later replaced/disposed. Preserve its
    # provenance from intermediate snapshots, not just the final inventory.
    observed_items = hero["inventory"] + [i for a in actions for i in a["after"]["equipment"]]
    crafted = {i["id"]: i for i in observed_items if i.get("acquisitionKind") == "craft"}
    craft_ids = set(crafted)
    assert len(craft_ids) == sum(a["action"] == "rare-craft" for a in actions)
    assert sorted(i["slot"] for i in crafted.values()) == sorted(int(a["detail"]) for a in actions if a["action"] == "rare-craft")
    assert acquired_ids | initial_ids | box_ids | craft_ids == final_ids | disposed_ids
    stages = []
    for stage, attempts in sorted(by_stage.items()):
        cleared = next(r for r in attempts if r["outcome"] == "Cleared")
        first_clear_gear = Counter(("common", "magic", "rare", "legendary")[i["item"]["rarity"]] for i in cleared["equipment"] if i["claimed"])
        stages.append({"stage": stage, "attempts": len(attempts), "failures": sum(r["outcome"] != "Cleared" for r in attempts),
            "total_seconds": sum(r["riftSeconds"] for r in attempts), "failed_seconds": sum(r["riftSeconds"] for r in attempts if r["outcome"] != "Cleared"),
            "first_clear_seconds": cleared["riftSeconds"], "first_clear_cumulative_seconds": cleared["cumulativeRiftSeconds"],
            "level_after_clear": cleared["afterCombat"]["level"], "attack_after_town": cleared["afterTown"]["attack"],
            "equipped_legendary": cleared["afterTown"]["equippedLegendary"], "first_clear_equipment": dict(first_clear_gear)})
    weighted_expected = Counter()
    for r in rows:
        design = proposal[r["stage"] - 1]["rewards"]["sources"]
        for source, trials in (("normal", r["normalKills"]), ("elite", r["eliteKills"]), ("boss", 3 if r["bossRewarded"] else 0)):
            profile = design[source]
            for rarity, probability in zip(("common", "magic", "rare", "legendary"), profile["rarity_probabilities"]):
                weighted_expected[rarity] += trials * profile["equipment_chance_per_trial"] * probability
    measurement_notes.append("The initial observer matched enemy positions; boss drops spawn beside the hero. Source attribution is corrected against BossClear's final-three-drop contract; no reward or outcome is changed.")
    if target >= 10:
        measurement_notes.append("Both tier-1 choice boxes belonged to one stack; the opener selected G01 for both (20 diamond gems). The original policy's G01/G03 wording must not be read as ten of each.")
    report = {"scope": header["policy"], "verified": True, "target": target, "hero_index": initial["selectedHero"], "crafted_items": list(crafted.values()), "stages": stages,
        "totals": {k: header[k] for k in ("attempts", "clears", "failures", "deaths", "timeouts", "farms", "days", "riftSeconds", "failedRiftSeconds", "wallSeconds", "calendarRestSeconds")},
        "initial": header["initial"], "final": header["final"], "rift_equipment_generated": dict(generated), "rift_equipment_acquired": dict(acquired),
        "rift_equipment_by_source": {k: dict(v) for k, v in sources.items()}, "box_equipment": boxes,
        "claimed_ground_resources": dict(resources), "gross_income": dict(income), "expenditure": dict(expense),
        "town_transactions": {k: dict(v) for k, v in town_sources.items()},
        "normal_kills": total_normal, "elite_kills": total_elite, "growth_events": levels,
        "proposal_same_observed_normal_elite_boss_kills": dict(weighted_expected),
        "proposal_r10" if target == 10 else "proposal_target": {k: proposal[-1][k] for k in ("day", "hero_level", "item_level", "attack_power", "hp", "cumulative_attempts", "cumulative_successes", "proposal_equipped_coverage", "gold_earned", "materials_earned", "stones_earned", "gems_earned")},
        "encounters": seed_stages, "measurement_notes": measurement_notes,
        "files": {p.name: hashlib.sha256(p.read_bytes()).hexdigest() for p in directory.iterdir() if p.is_file() and p.suffix in (".json", ".jsonl") and p.name != "analysis.json"}}
    return report


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("directory", type=Path)
    parser.add_argument("--output", type=Path)
    args = parser.parse_args()
    report = summarize(args.directory, Path(__file__).resolve().parents[1])
    out = args.output or args.directory / "analysis.json"
    out.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n")
    print(json.dumps({"verified": report["verified"], "totals": report["totals"], "equipment": report["rift_equipment_acquired"], "output": str(out)}, ensure_ascii=False))
