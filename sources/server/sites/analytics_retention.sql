-- Anonymous UTC calendar D1/D7 retention of NEW ACCOUNTS, not installs or pre-existing guests.
-- Bind from_ms, to_ms, as_of_ms and collection_start_ms (actual activation time).
-- Existing accounts created before collection are excluded; immature cohorts are not failures.
WITH accounts AS (
  SELECT id,date(created,'unixepoch') cohort_day FROM auth_accounts
  WHERE created*1000>=max(:from_ms,:collection_start_ms) AND created*1000<:to_ms AND created*1000<=:as_of_ms
), active AS (
  SELECT DISTINCT substr(account_id,instr(account_id,':')+1) uid,
    date((CASE WHEN client_utc_ms BETWEEN received_utc_ms-2592000000 AND received_utc_ms+300000
      THEN client_utc_ms ELSE received_utc_ms END)/1000.0,'unixepoch') day
  FROM player_events WHERE identity_kind IN ('player','google') AND kind='activity' AND received_utc_ms<=:as_of_ms
)
SELECT cohort_day,count(*) created_accounts,
  sum(CASE WHEN date(:as_of_ms/1000.0,'unixepoch')>=date(cohort_day,'+2 days') THEN 1 ELSE 0 END) matured_d1_accounts,
  sum(CASE WHEN date(:as_of_ms/1000.0,'unixepoch')>=date(cohort_day,'+2 days')
    AND EXISTS(SELECT 1 FROM active WHERE uid=accounts.id AND day=date(cohort_day,'+1 day')) THEN 1 ELSE 0 END) retained_d1_accounts,
  sum(CASE WHEN date(:as_of_ms/1000.0,'unixepoch')>=date(cohort_day,'+8 days') THEN 1 ELSE 0 END) matured_d7_accounts,
  sum(CASE WHEN date(:as_of_ms/1000.0,'unixepoch')>=date(cohort_day,'+8 days')
    AND EXISTS(SELECT 1 FROM active WHERE uid=accounts.id AND day=date(cohort_day,'+7 days')) THEN 1 ELSE 0 END) retained_d7_accounts
FROM accounts GROUP BY cohort_day ORDER BY cohort_day;
