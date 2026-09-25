-- Observed client reports. Never use these queries to grant rewards or automatically ban an account.
-- Deduplicated by authenticated account_id + run_id at ingestion.
SELECT account_id, hero_id, stage, COUNT(*) AS attempts,
       SUM(outcome='victory') AS victories, SUM(outcome='defeat') AS defeats,
       AVG(CASE WHEN outcome='victory' THEN simulation_seconds END) AS clear_simulation_seconds,
       AVG(CASE WHEN outcome='victory' THEN attendance_seconds END) AS clear_attendance_seconds,
       SUM(earned_gold) AS reported_gold, SUM(boss_defeated) AS reported_boss_kills
FROM runs GROUP BY account_id, hero_id, stage;

SELECT r.hero_id, e.source, e.kind, COUNT(*) AS observations,
       SUM(COALESCE(json_extract(e.detail_json, '$.damageEvents[0].hpLoss'),0)) AS actual_hp_damage
FROM events e JOIN runs r USING(account_id, run_id)
WHERE e.kind IN ('HIT_DEALT','HIT_TAKEN','DECISION') GROUP BY r.hero_id,e.source,e.kind;

SELECT account_id,run_id,category,
       SUM(claimed) AS claimed, SUM(ignored) AS ignored, COUNT(*) AS dropped
FROM drops GROUP BY account_id,run_id,category;

-- Initial loadout, gear, potions and rune configuration remain in immutable source payloads.
SELECT account_id,run_id,hero_id,stage,
       json_extract(payload,'$.journal.initialBuild.classSkills') AS initial_skills,
       json_extract(payload,'$.review.finalBuild.classSkills') AS final_skills,
       json_extract(payload,'$.journal.initialEquipment') AS equipment,
       json_extract(payload,'$.journal.initialPotions') AS potions
FROM runs;

-- Trusted evidence is written ONLY by the authoritative service's committed replay adapter.
SELECT r.account_id,r.run_id,r.signals_json,a.revision,a.evidence_json
FROM runs r LEFT JOIN authority_evidence a USING(account_id,run_id)
WHERE r.signals_json <> '["CLIENT_OBSERVATION_UNVERIFIED"]';

-- Compare balance releases without mixing cached/bundled and published configurations.
-- The reference is still client-observed; the hash is an identifier, not an anti-cheat signature.
SELECT COALESCE(c.source,'legacy') AS config_source,c.version,c.config_hash,r.stage,
       COUNT(*) AS attempts,SUM(r.outcome='victory') AS victories,
       AVG(r.simulation_seconds) AS average_seconds,AVG(r.earned_gold) AS average_gold
FROM runs r LEFT JOIN run_configuration c USING(account_id,run_id)
GROUP BY c.source,c.version,c.config_hash,r.stage;
