-- Read-only anonymous 24-hour prologue funnel. Same UTC bind parameters as analytics_daily.sql.
-- Cohort = first observed committed tutorial_step 0. Existing completed or mid-tutorial saves
-- are left-censored and excluded. A missing completion is a dropout only after 24 hours mature.
WITH events AS (
  SELECT substr(account_id,instr(account_id,':')+1) uid,step,
    CASE WHEN client_utc_ms BETWEEN received_utc_ms-2592000000 AND received_utc_ms+300000
      THEN client_utc_ms ELSE received_utc_ms END observed_ms
  FROM player_events WHERE identity_kind IN ('player','google') AND kind='tutorial_step' AND received_utc_ms<=:as_of_ms
), starts AS (
  SELECT uid,min(observed_ms) started_ms FROM events WHERE step=0 GROUP BY uid
), cohorts AS (
  SELECT * FROM starts WHERE started_ms>=:from_ms AND started_ms<:to_ms
), progress AS (
  SELECT c.uid,c.started_ms,max(CASE WHEN e.step=100 THEN 1 ELSE 0 END) completed,
    max(CASE WHEN e.step BETWEEN 0 AND 6 THEN e.step ELSE 0 END) furthest_phase
  FROM cohorts c LEFT JOIN events e ON e.uid=c.uid AND e.observed_ms BETWEEN c.started_ms AND c.started_ms+86400000
  GROUP BY c.uid,c.started_ms
)
SELECT date(started_ms/1000.0,'unixepoch') cohort_day,count(*) observed_starters,
  sum(CASE WHEN started_ms+86400000<=:as_of_ms THEN 1 ELSE 0 END) matured_starters,
  sum(CASE WHEN started_ms+86400000<=:as_of_ms AND completed=1 THEN 1 ELSE 0 END) completed_within_24h,
  sum(CASE WHEN started_ms+86400000<=:as_of_ms AND completed=0 THEN 1 ELSE 0 END) incomplete_after_24h,
  sum(CASE WHEN started_ms+86400000<=:as_of_ms AND completed=0 AND furthest_phase=0 THEN 1 ELSE 0 END) stopped_at_phase_0,
  sum(CASE WHEN started_ms+86400000<=:as_of_ms AND completed=0 AND furthest_phase=1 THEN 1 ELSE 0 END) stopped_at_phase_1,
  sum(CASE WHEN started_ms+86400000<=:as_of_ms AND completed=0 AND furthest_phase=2 THEN 1 ELSE 0 END) stopped_at_phase_2,
  sum(CASE WHEN started_ms+86400000<=:as_of_ms AND completed=0 AND furthest_phase=3 THEN 1 ELSE 0 END) stopped_at_phase_3,
  sum(CASE WHEN started_ms+86400000<=:as_of_ms AND completed=0 AND furthest_phase=4 THEN 1 ELSE 0 END) stopped_at_phase_4,
  sum(CASE WHEN started_ms+86400000<=:as_of_ms AND completed=0 AND furthest_phase=5 THEN 1 ELSE 0 END) stopped_at_phase_5,
  sum(CASE WHEN started_ms+86400000<=:as_of_ms AND completed=0 AND furthest_phase=6 THEN 1 ELSE 0 END) stopped_at_phase_6
FROM progress GROUP BY cohort_day ORDER BY cohort_day;
