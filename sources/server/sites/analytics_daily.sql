-- Read-only, anonymous aggregate. Bind from_ms, to_ms (exclusive), as_of_ms as UTC milliseconds.
-- QA is excluded. Both ordinary namespaces are normalized internally; no identifier is output.
-- An offline client clock outside [received - 30 days, received + 5 minutes] uses receive time.
WITH normalized AS (
  SELECT substr(account_id,instr(account_id,':')+1) uid,session_id,kind,source,currency,seconds,amount,
    CASE WHEN client_utc_ms BETWEEN received_utc_ms-2592000000 AND received_utc_ms+300000
      THEN client_utc_ms ELSE received_utc_ms END observed_ms,
    CASE WHEN client_utc_ms BETWEEN received_utc_ms-2592000000 AND received_utc_ms+300000 THEN 0 ELSE 1 END clock_fallback
  FROM player_events WHERE identity_kind IN ('player','google') AND received_utc_ms<=:as_of_ms
    AND received_utc_ms>=:from_ms-605100000
), events AS (
  SELECT *,date(observed_ms/1000.0,'unixepoch') day FROM normalized
  WHERE observed_ms>=:from_ms-604800000 AND observed_ms<:to_ms
), active_days AS (
  SELECT uid,day,sum(seconds) seconds FROM events WHERE kind='activity' GROUP BY uid,day
), active_totals AS (
  SELECT a.day,count(*) dau,sum(a.seconds) foreground_seconds,
    sum(CASE WHEN EXISTS(SELECT 1 FROM active_days p WHERE p.uid=a.uid AND p.day<a.day
      AND p.day>=date(a.day,'-7 days')) THEN 1 ELSE 0 END) returning_7d
  FROM active_days a GROUP BY a.day
), session_slices AS (
  SELECT day,uid,session_id,sum(seconds) seconds FROM events WHERE kind='activity' GROUP BY day,uid,session_id
), sessions AS (
  SELECT day,count(*) active_session_slices,avg(seconds) mean_session_slice_seconds FROM session_slices GROUP BY day
), daily AS (
  SELECT day,count(DISTINCT CASE WHEN kind='session_start' THEN uid END) signed_in_accounts,
    sum(CASE WHEN kind='activity' AND source='combat' THEN seconds ELSE 0 END) combat_action_seconds,
    sum(CASE WHEN kind='rift_enter' THEN 1 ELSE 0 END) rift_entries,
    sum(CASE WHEN kind='currency_spent' AND currency='gold' THEN amount ELSE 0 END) gold_spent,
    sum(CASE WHEN kind='currency_spent' AND currency='abyssal_coin' THEN amount ELSE 0 END) abyssal_coin_spent,
    sum(CASE WHEN kind='currency_spent' AND currency='enhancement_stone' THEN amount ELSE 0 END) enhancement_stones_spent,
    sum(CASE WHEN kind='currency_spent' AND currency='material' THEN amount ELSE 0 END) materials_spent,
    sum(CASE WHEN kind='collection_gap' THEN amount ELSE 0 END) reported_event_loss,
    sum(clock_fallback) clock_fallback_events
  FROM events WHERE observed_ms>=:from_ms GROUP BY day
), run_base AS (
  SELECT substr(account_id,instr(account_id,':')+1) uid,run_id,outcome,boss_defeated,stage,simulation_seconds,attendance_seconds,
    received_utc_ms,
    CASE WHEN completed_utc_ms BETWEEN received_utc_ms-2592000000 AND received_utc_ms+300000
      THEN completed_utc_ms ELSE received_utc_ms END observed_ms
  FROM runs WHERE identity_kind IN ('player','google') AND received_utc_ms<=:as_of_ms AND received_utc_ms>=:from_ms-300000
), unique_runs AS (
  SELECT *,row_number() OVER(PARTITION BY uid,run_id ORDER BY received_utc_ms) ordinal FROM run_base
), combats AS (
  SELECT date(observed_ms/1000.0,'unixepoch') day,count(*) finished_rifts,
    sum(CASE WHEN outcome='victory' AND boss_defeated=1 THEN 1 ELSE 0 END) cleared_rifts,
    sum(simulation_seconds) rift_simulation_seconds,sum(attendance_seconds) rift_attendance_seconds
  FROM unique_runs WHERE ordinal=1 AND observed_ms>=:from_ms AND observed_ms<:to_ms GROUP BY day
), days AS (SELECT day FROM daily UNION SELECT day FROM combats)
SELECT days.day,coalesce(a.dau,0) dau,coalesce(a.returning_7d,0) returning_7d,
  coalesce(d.signed_in_accounts,0) signed_in_accounts,
  coalesce(s.active_session_slices,0) active_session_slices,round(coalesce(s.mean_session_slice_seconds,0),3) mean_session_slice_seconds,
  round(coalesce(a.foreground_seconds,0),3) foreground_seconds,round(coalesce(d.combat_action_seconds,0),3) combat_action_seconds,
  coalesce(d.rift_entries,0) rift_entries,coalesce(c.finished_rifts,0) finished_rifts,coalesce(c.cleared_rifts,0) cleared_rifts,
  coalesce(c.rift_simulation_seconds,0) rift_simulation_seconds,coalesce(c.rift_attendance_seconds,0) rift_attendance_seconds,
  coalesce(d.gold_spent,0) gold_spent,coalesce(d.abyssal_coin_spent,0) abyssal_coin_spent,
  coalesce(d.enhancement_stones_spent,0) enhancement_stones_spent,coalesce(d.materials_spent,0) materials_spent,
  coalesce(d.reported_event_loss,0) reported_event_loss,coalesce(d.clock_fallback_events,0) clock_fallback_events
FROM days LEFT JOIN active_totals a USING(day) LEFT JOIN sessions s USING(day)
  LEFT JOIN daily d USING(day) LEFT JOIN combats c USING(day) ORDER BY days.day;
