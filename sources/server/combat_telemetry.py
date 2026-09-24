#!/usr/bin/env python3
"""Authenticated, durable reference receiver for HELLSCRIPT client observations.

Run behind a TLS/authenticated service in production. This development host binds
loopback and uses an operator-provided token-to-account map. It never grants loot.
"""
import argparse
import hashlib
import hmac
import json
import math
import os
import re
import sqlite3
import time
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
from pathlib import Path

MAX_BODY = 16 * 1024 * 1024
ID = re.compile(r"[0-9a-f]{32}\Z")


class Rejected(Exception):
    def __init__(self, status, code):
        self.status, self.code = status, code


def validate_tree(value, depth=0):
    if depth > 64:
        raise Rejected(422, "nesting")
    if isinstance(value, float) and not math.isfinite(value):
        raise Rejected(422, "nonfinite")
    if isinstance(value, (dict, list)):
        for child in value.values() if isinstance(value, dict) else value:
            validate_tree(child, depth + 1)


def validate(record):
    validate_tree(record)
    if not isinstance(record, dict) or not ID.fullmatch(str(record.get("id", ""))):
        raise Rejected(422, "run_id")
    j = record.get("journal")
    if not isinstance(j, dict) or j.get("version") != 1:
        raise Rejected(422, "schema")
    if j.get("trust") != "client_observed" or j.get("archived") is not False:
        raise Rejected(422, "provenance")
    if j.get("outcome") not in ("victory", "defeat"):
        raise Rejected(422, "outcome")
    for key in ("heroId", "localAccountId"):
        if not ID.fullmatch(str(j.get(key, ""))):
            raise Rejected(422, "identity")
    for key, minimum, maximum in (("stage", 1, 100000), ("sequence", 0, 10000000),
                                 ("omittedEvents", 0, 10000000), ("attempt", 1, 2**53),
                                 ("earnedGold", 0, 2**53), ("kills", 0, 10000000)):
        value = j.get(key)
        if type(value) is not int or not minimum <= value <= maximum:
            raise Rejected(422, "counter_" + key)
    if record.get("stage") != j["stage"] or record.get("kills") != j["kills"]:
        raise Rejected(422, "summary_mismatch")
    for key in ("equipmentCollected", "runesAwarded", "bossesKilled", "resumes", "priorUnexportedLossCount"):
        if key in j and (type(j[key]) is not int or not 0 <= j[key] <= 2**53):
            raise Rejected(422, "counter_" + key)
    if "bossDefeated" in j and type(j["bossDefeated"]) is not bool:
        raise Rejected(422, "boss_result")
    if "finish" in j and (not isinstance(j["finish"], str) or len(j["finish"]) > 128):
        raise Rejected(422, "finish")
    for key in ("simulationSeconds", "attendanceSeconds", "observedFrom", "walkingDistance", "maxHealth"):
        if type(j.get(key)) not in (int, float) or not math.isfinite(j[key]) or j[key] < 0:
            raise Rejected(422, "number_" + key)
    for key in ("startedUtcMs", "completedUtcMs"):
        if type(j.get(key)) is not int or not 0 <= j[key] <= 2**53:
            raise Rejected(422, "clock")
    events = j.get("events")
    if not isinstance(events, list) or len(events) > 20000:
        raise Rejected(422, "event_budget")
    last_sequence, last_time = 0, -1
    for event in events:
        if not isinstance(event, dict) or type(event.get("sequence")) is not int or event["sequence"] <= last_sequence:
            raise Rejected(422, "event_sequence")
        seconds = event.get("seconds")
        if type(seconds) not in (int, float) or not math.isfinite(seconds) or seconds < max(0, last_time) or seconds > j["simulationSeconds"] + .1:
            raise Rejected(422, "event_time")
        for key in ("kind", "message"):
            if not isinstance(event.get(key), str) or len(event[key]) > 8192:
                raise Rejected(422, "event_text")
        for key in ("source", "trigger"):
            if key in event and (not isinstance(event[key], str) or len(event[key]) > 512):
                raise Rejected(422, "event_identifier")
        for key in ("hp", "resource"):
            if key in event and (type(event[key]) not in (int, float) or not math.isfinite(event[key])):
                raise Rejected(422, "event_number")
        for key in ("target", "actionId"):
            if key in event and (type(event[key]) is not int or not -1 <= event[key] <= 2**31-1):
                raise Rejected(422, "event_reference")
        for key in ("damageEvents", "rules", "builds", "edicts"):
            if key in event and (not isinstance(event[key], list) or len(event[key]) > 1 or any(not isinstance(v, dict) for v in event[key])):
                raise Rejected(422, "event_snapshot")
        last_sequence, last_time = event["sequence"], seconds
    if events and last_sequence != j["sequence"] or j["sequence"] - len(events) != j["omittedEvents"]:
        raise Rejected(422, "event_gap")
    for key in ("equipmentDrops", "resourceDrops"):
        if not isinstance(j.get(key), list) or len(j[key]) > 20000 or any(not isinstance(d, dict) for d in j[key]):
            raise Rejected(422, "reward_budget")
    signals = ["CLIENT_OBSERVATION_UNVERIFIED"]
    if j["startedUtcMs"] == 0 or j["observedFrom"] > 0:
        signals.append("PARTIAL_RUN")
    if j["completedUtcMs"] < j["startedUtcMs"]:
        signals.append("CLIENT_CLOCK_ROLLBACK")
    if j["simulationSeconds"] > j["attendanceSeconds"] * 1.5 + 1:
        signals.append("REPORTED_SPEED_REQUIRES_AUTHORITY_CHECK")
    if j["outcome"] == "victory" and not j.get("bossDefeated"):
        signals.append("VICTORY_WITHOUT_REPORTED_BOSS")
    if j["omittedEvents"]:
        signals.append("EVENT_GAP")
    if j.get("priorUnexportedLossCount", 0):
        signals.append("LOCAL_EXPORT_LOSS")
    # These are review signals. A client can forge any field, including clocks and hashes.
    return signals


class Repository:
    def __init__(self, path):
        self.path = str(path)
        with self.connect() as db:
            db.executescript("""
                CREATE TABLE IF NOT EXISTS runs (
                  account_id TEXT NOT NULL, run_id TEXT NOT NULL, payload_hash TEXT NOT NULL,
                  received_utc_ms INTEGER NOT NULL, hero_id TEXT NOT NULL, stage INTEGER NOT NULL,
                  outcome TEXT NOT NULL, finish TEXT, started_utc_ms INTEGER, completed_utc_ms INTEGER,
                  simulation_seconds REAL, attendance_seconds REAL, earned_gold INTEGER, kills INTEGER,
                  boss_defeated INTEGER, equipment_collected INTEGER, signals_json TEXT NOT NULL,
                  payload TEXT NOT NULL, PRIMARY KEY(account_id, run_id));
                CREATE INDEX IF NOT EXISTS runs_account_time ON runs(account_id, received_utc_ms);
                CREATE INDEX IF NOT EXISTS runs_stage_outcome ON runs(stage, outcome);
                CREATE TABLE IF NOT EXISTS events (
                  account_id TEXT, run_id TEXT, sequence INTEGER, seconds REAL, kind TEXT, source TEXT,
                  trigger TEXT, target INTEGER, action_id INTEGER, hp REAL, resource REAL, detail_json TEXT,
                  PRIMARY KEY(account_id, run_id, sequence));
                CREATE TABLE IF NOT EXISTS drops (
                  account_id TEXT, run_id TEXT, category TEXT, ordinal INTEGER, claimed INTEGER,
                  ignored INTEGER, detail_json TEXT, PRIMARY KEY(account_id, run_id, category, ordinal));
                CREATE TABLE IF NOT EXISTS authority_evidence (
                  account_id TEXT, run_id TEXT, revision INTEGER, evidence_json TEXT,
                  PRIMARY KEY(account_id, run_id));
                CREATE TABLE IF NOT EXISTS audit (
                  received_utc_ms INTEGER, account_id TEXT, run_id TEXT, code TEXT, payload_hash TEXT);
            """)

    def connect(self):
        db = sqlite3.connect(self.path, timeout=10)
        db.execute("PRAGMA journal_mode=WAL")
        db.execute("PRAGMA synchronous=FULL")
        return db

    def ingest(self, account_id, raw, now_ms=None):
        if len(raw) > MAX_BODY:
            raise Rejected(413, "body_budget")
        digest = hashlib.sha256(raw).hexdigest()
        try:
            def invalid_constant(_):
                raise ValueError("non-finite JSON")
            record = json.loads(raw.decode("utf-8"), parse_constant=invalid_constant)
        except (ValueError, UnicodeError, RecursionError):
            raise Rejected(400, "json")
        try:
            signals = validate(record)
        except (TypeError, KeyError, OverflowError, AttributeError):
            raise Rejected(422, "shape")
        j, run_id = record["journal"], record["id"]
        now_ms = int(time.time() * 1000) if now_ms is None else now_ms
        with self.connect() as db:
            db.execute("BEGIN IMMEDIATE")
            previous = db.execute("SELECT payload_hash FROM runs WHERE account_id=? AND run_id=?", (account_id, run_id)).fetchone()
            if previous:
                if previous[0] != digest:
                    db.execute("INSERT INTO audit VALUES(?,?,?,?,?)", (now_ms, account_id, run_id, "PAYLOAD_CONFLICT", digest))
                    db.commit()
                    raise Rejected(409, "payload_conflict")
            else:
                db.execute("INSERT INTO runs VALUES(?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)", (
                    account_id, run_id, digest, now_ms, j["heroId"], j["stage"], j["outcome"], j.get("finish"),
                    j["startedUtcMs"], j["completedUtcMs"], j["simulationSeconds"], j["attendanceSeconds"],
                    j["earnedGold"], j["kills"], bool(j.get("bossDefeated")), j.get("equipmentCollected", 0),
                    json.dumps(signals), raw.decode("utf-8")))
                db.executemany("INSERT INTO events VALUES(?,?,?,?,?,?,?,?,?,?,?,?)", [
                    (account_id, run_id, e["sequence"], e["seconds"], e["kind"], e.get("source"), e.get("trigger"),
                     e.get("target"), e.get("actionId"), e.get("hp"), e.get("resource"), json.dumps(e, ensure_ascii=False)) for e in j["events"]])
                db.executemany("INSERT INTO drops VALUES(?,?,?,?,?,?,?)", [
                    (account_id, run_id, category, i, bool(d.get("claimed")), bool(d.get("ignored")), json.dumps(d, ensure_ascii=False))
                    for category in ("equipmentDrops", "resourceDrops") for i, d in enumerate(j[category])])
        return {"accepted": True, "runId": run_id, "payloadHash": digest}

    def attach_authority(self, account_id, run_id, revision, evidence):
        """Server-internal call only, after a trusted IHuntAuthorityRepository CAS commit.

        No HTTP route exposes this method. Evidence must be server-owned replay results,
        admission seed/configuration, lease, sequence/ticks, rewards and rejection codes.
        """
        with self.connect() as db:
            db.execute("INSERT INTO authority_evidence VALUES(?,?,?,?) ON CONFLICT(account_id,run_id) DO UPDATE SET revision=excluded.revision,evidence_json=excluded.evidence_json WHERE excluded.revision>authority_evidence.revision",
                       (account_id, run_id, revision, json.dumps(evidence, ensure_ascii=False)))


def handler(repository, tokens):
    class Handler(BaseHTTPRequestHandler):
        def log_message(self, *_):
            pass  # Never echo authorization headers or player data to console logs.

        def reply(self, status, body):
            data = json.dumps(body).encode()
            self.send_response(status)
            self.send_header("Content-Type", "application/json")
            self.send_header("Content-Length", str(len(data)))
            self.end_headers()
            self.wfile.write(data)

        def do_POST(self):
            self.connection.settimeout(15)
            if self.path != "/v1/combat-runs":
                return self.reply(404, {"error": "route"})
            authorization = self.headers.get("Authorization", "")
            account = next((a for token, a in tokens.items() if hmac.compare_digest(authorization, "Bearer " + token)), None)
            if account is None:
                return self.reply(401, {"error": "authentication"})
            try:
                length = int(self.headers.get("Content-Length", "0"))
                if not 0 < length <= MAX_BODY or self.headers.get("Transfer-Encoding"):
                    raise Rejected(413, "body_budget")
                raw = self.rfile.read(length)
                if len(raw) != length:
                    raise Rejected(400, "incomplete_body")
                result = repository.ingest(account, raw)
                self.reply(200, result)
            except Rejected as error:
                self.reply(error.status, {"error": error.code})
            except (ValueError, TimeoutError):
                self.reply(400, {"error": "request"})
            except sqlite3.Error:
                self.reply(503, {"error": "storage_unavailable"})
    return Handler


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--database", type=Path, required=True)
    parser.add_argument("--port", type=int, default=8789)
    args = parser.parse_args()
    tokens = json.loads(os.environ.get("HELLSCRIPT_TELEMETRY_TOKENS", "{}"))
    if not tokens or any(len(t) < 24 or not a for t, a in tokens.items()):
        parser.error("Configure HELLSCRIPT_TELEMETRY_TOKENS with strong token-to-account bindings.")
    args.database.parent.mkdir(parents=True, exist_ok=True)
    ThreadingHTTPServer(("127.0.0.1", args.port), handler(Repository(args.database), tokens)).serve_forever()


if __name__ == "__main__":
    main()
