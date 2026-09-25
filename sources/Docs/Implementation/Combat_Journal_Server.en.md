# Rift combat journal and server analytics

Updated: 2026-09-25

Korean: [균열 전투 기록과 서버 분석 수집](Combat_Journal_Server.md) · [Live operations and configuration releases](Live_Operations.en.md)

## Player experience

The rift entry screen exposes **Battle records**, a newest-first account-wide list of the last 100 runs with an **All / Victories only / Defeats only** dropdown. Rows show device-local completion time and UTC offset, class, stage, combat duration and termination reason. Legacy timestamps and outcomes remain unknown when absent; localized result strings are never used to invent an outcome.

Defeats include death, timeout and returning before defeating the boss, distinguished by their termination reasons. Leaving loot after defeating the boss remains a victory. Training creates neither owned rift history nor analytics deliveries.

During battle a live text panel sits above the persistent bottom HUD. Narrow screens retain as many complete recent messages as fit and abbreviate the preview when necessary. Completed text logs are chronological and paged in blocks of 100 entries. Existing summaries, final-five-second damage/blocked-action analysis and historical equipment/configuration remain available.

The journal joins observed movement/stop reasons, targets/destinations, rules, selected actions, cast phases, automatic/player triggers, cooldown/resource/range/position waits, hits, shield absorption, deaths, bosses, chests, potions, rewards and configuration changes. Enemy attack/hazard telegraphs retain preparation time and aim, with interruption and enrage events. Damage records retain source/target/root-cast IDs and actual HP loss separately from overkill. Death summaries use the observed final window's HP loss, largest damage source and blocked-skill reasons. Missing evidence is never reconstructed as fact.

## Ownership and persistence

`CombatSimulation` records through `CombatJournal` at existing decision, movement, damage and reward owners. UI only observes. Completed `CombatHistory` snapshots own their data. Journals were introduced in save schema 15; the current account schema is **16**, including live configuration and first-clear reward preservation. Legacy records remain readable.

`GameStore` commits the account first. `CombatJournalArchive` then creates derived files; failed account saves create no delivery. Export failures retain full records in the account for retry. Successful exports remove detailed events from the account's record, loading them from the per-run file when requested. A crash after an ACK and before the next account save retries the canonical file's exact bytes.

| Location | Purpose and retention |
| --- | --- |
| `<save directory>/combat-records/<runId>.json` | One JSON file per completed battle, containing readable messages and structured evidence. Only the latest 100 account records are retained. |
| Account `records` | Last 100 summaries, reviews and configuration snapshots; unexported records also retain their events. |
| `<save directory>/combat-outbox/<runId>.json` | Independent delivery queue; removal requires a matching server acknowledgment. Local 100-run pruning does not delete queued uploads. |
| `*.json.rejected` | Preserved server-rejected bodies, allowing later runs to proceed. |

Detailed events are capped at 20,000 per run. The start and newest window survive; omitted counts and sequence gaps are explicit, while full aggregates remain exact. Movement records use the actual movement override and distinguish survival responses from skill positioning; stale decisions are not reused as current triggers. Movement is sampled on state changes and every two seconds, repeated identical decisions every four seconds. This is not a frame-by-frame position replay. Event `damageEvents`, `rules`, `builds` and `edicts` are zero-or-one arrays so Unity does not materialize absent inline snapshots on reload. Files use compact JSON.

The outbox, including rejected evidence, is limited to 1,000 files or 256 MiB. Player files are still created when delivery capacity is exhausted, and unexported source data remains retryable within the latest 100 runs. If such a record ages out before export, `combatTelemetryLossCount` and subsequent `LOCAL_EXPORT_LOSS` explicitly report the gap. Collection must not be described as complete in that case.

## Analytics and trust

| Category | Evidence |
| --- | --- |
| Identity | Server-authenticated account ID, separate local account identifier, hero ID/class/start and end level, local attempt ordinal. No real name or advertising identifier is collected. |
| Environment/content | App version, build GUID, platform, save/item/map versions, stage, map fingerprint and seed. Configuration source, release version, hash and the admitted settings are retained. |
| Timing/progression | Client UTC start/end, simulation duration, existing attendance/fatigue duration, resume count, observed start; server receipt time is generated independently. |
| Loadout | Initial build/skills/ranks/options, Hunt Edict, equipped item instances, slot levels, rune configuration, potion state and final configuration; configuration-change events also capture the build. |
| Outcomes/rewards | Outcome/reason, kills/boss completion, awarded gold, equipment/rune counts, claimed/ignored drop states and exact item snapshots. Gold uses `RiftEarnings`, not wallet deltas. |
| Combat/integrity | Event sequence, cast/target/rule IDs, position, HP/resource, damage calculation inputs/results, final-window analysis and truncation/resume/export/clock trust markers. |

Client payloads are `client_observed`. SHA-256 provides retry byte identity, not anti-cheat authenticity. The receiver binds ownership to authentication, deduplicates account/run IDs and audits conflicting bodies. Suspicious speed, outcome or reward reports are investigation signals, never automatic sanctions.

The existing `AuthoritativeRiftVerifier` replays from a server-owned admission snapshot, clock and seed. Its completed `server_replay` copy is included in `HuntAuthorityCheckpoint.combatTelemetry` and committed by the same CAS as account/replay cursors. Client HTTP ingestion rejects this provenance. An internal host adapter must pass only committed checkpoints into `Repository.attach_authority`.

## Server integration

The public server origin is `https://hellscript-production.up.railway.app`. Only that origin belongs in `Resources/Data/ServerConnection.json`; `GameServerConnection` connects published-configuration reads. Combat uploads in development builds and the Editor require a separately issued QA session file, either `qa-session.json` in the device save directory or a path supplied with `-hellscriptQaSessionFile`. Its version, upload purpose, target server and per-tester token are checked before it is passed as `ICombatTelemetrySession` to `GameController.Telemetry.Configure`. Real file contents and private paths do not belong in the repository or public wiki.

QA upload tokens and web operator keys are separate credentials. The QA file does not replace production login or short-lived token issuance, and this development-file connection path is excluded from ordinary release builds. Credentials are not included in account saves or game assets. Without an upload session, combat files remain in the local outbox. Loopback HTTP is allowed only for development.

`CombatTelemetryUploader` removes a file only when `accepted`, `runId` and the exact byte `payloadHash` match. Timeouts, authentication and transient failures use exponential retries capped at 300 seconds. Redirects are disabled.

`server/combat_telemetry.py` owns shared SQLite validation and persistence; the deployed host runs `server/telemetry_wsgi.py` through Gunicorn. `/v1/combat-runs` authenticates, checks bounds/schema/sequence/time/summary shape and commits runs/events/drops atomically before acknowledging. `server/combat_analytics.sql` provides account/hero/stage outcomes, clear duration/gold, skill damage, loot and authority-evidence queries, including comparisons by configuration release.

On the same host, `/v1/liveops/current` exposes only published configuration for reading, while `/ops` provides authenticated draft, publication and restoration workflows. New settings are fixed in the next rift's admission snapshot. Active battles are unchanged and claimed first-clear rewards are not issued again. Unclaimed rewards promised on the first successful clear keep their composition. See [Live operations](Live_Operations.en.md) for available controls and workflow.

This connection supports QA collection and configuration distribution. Production account authentication, server-owned admission/seed/reward storage and the replay worker remain separate integrations. [Live operations verification](Live_Operations.en.md#verification-record) distinguishes actual deployment from local validation.

## Railway deployment for a small QA group

The root `Dockerfile` runs telemetry ingestion, configuration APIs and the operator UI with Python 3.12 and Gunicorn 26.2.0. Unity remains a separate game client. `.dockerignore` allows only the required server code, UI assets and `RewardBoxes.json` for validation; other game assets, account saves, the development wiki and local credentials are excluded. Normal access logging never prints combat payloads or authorization headers.

Connect the Railway service to the repository's `main` branch and configure the following values. This table defines the integrated host configuration; each new deployment must still be verified after release.

| Setting | Value and reason |
| --- | --- |
| Root Directory / Dockerfile | Repository root / `Dockerfile`. Do not set a Unity build command. |
| Start command | Leave empty to use the image's Gunicorn command. It listens on `0.0.0.0:$PORT`, defaulting to 8080. |
| Volume | Attach persistent storage at `/data`. Preserve `/data/hellscript/telemetry.sqlite`, `liveops.sqlite` and each database's WAL. |
| Healthcheck | GET `/healthz`. Checks both databases without exposing authentication data. |
| Replicas | 1. Do not share the SQLite volume across replicas. |
| Restart | On failure, at most five retries. Invalid configuration must remain visible. |
| Automatic deployment | `main`. If Watch Paths are configured, include `server/**`, `Assets/HELLSCRIPT/Resources/Data/RewardBoxes.json`, `Dockerfile` and `.dockerignore`. |
| Upload authentication | Store a JSON mapping of strong tokens to QA account IDs in the secret `HELLSCRIPT_TELEMETRY_TOKENS` variable. |
| Operator authentication | Use separate key/operator bindings in `HELLSCRIPT_OPS_TOKENS` and set `HELLSCRIPT_OPS_ORIGIN` to the exact HTTPS origin. Upload tokens cannot be reused as operator keys. |

Issue separate tester tokens, for example with `secrets.token_urlsafe(32)`. Keys must contain 32–256 letters, digits, underscores or hyphens. QA account IDs allow 1–128 of those characters plus dots and colons; do not use real names or email addresses. Never embed a shared token in the game or commit real credentials to source or the public wiki. Add a replacement token for the same account before removing the old token to rotate gradually. This QA mapping does not replace production login and short-lived token issuance.

The adapter uses Railway's injected `RAILWAY_VOLUME_MOUNT_PATH`. On Railway it refuses to start without this variable or when the database path is outside the volume. Local runs require an explicit absolute `HELLSCRIPT_TELEMETRY_DATA_DIR`. An empty or invalid upload-credential map also fails startup. Missing operator keys keep the operator API closed. Combat requests are authenticated before JSON content-type, 16 MiB body and schema checks. ACK follows the database commit. Combat-payload queries and player reward-grant APIs are not public. Anonymous configuration reads and authenticated changes use separate routes; the public wiki contains neither an editing API nor administrator credentials.

For the first deployment, verify its HTTPS domain and `/healthz`, then use labeled synthetic QA data to exercise authentication rejection, persistence and identical retries. Verify the same ACK and database row after redeployment before connecting tester uploads. Real game data is not sent until the client's `ICombatTelemetrySession` is configured. Redeployment with a SQLite volume may cause brief downtime; the client outbox retries.

During setup, this Railway trial account's Backups screen required Pro, so platform backups were not enabled and the plan was not upgraded. Persistent storage and successful restart retention do not establish automatic backup coverage. Operators must arrange an available backup method and verify restoration separately. File-based backups of each running SQLite database must use its backup API; copying only the database while omitting its WAL can lose data. Server records are retained separately from the player's latest-100 limit. Monitor storage and billing alerts, and define long-term retention and deletion as an operating policy. Automatic sanctions or confirmed rewards still require server-owned admission data and replay.

`.github/workflows/telemetry.yml` runs Python tests for ingestion, deployment and operator APIs, Node editor-model checks and the Docker image build. Coverage includes separated credentials, ACK-after-commit, repeated uploads, draft/publication conflicts and process-restart retention. Final counts and Railway, game and public-wiki delivery results belong in [Live operations verification](Live_Operations.en.md#verification-record). An image build alone does not prove deployment or tester uploads.

References: [Railway volumes](https://docs.railway.com/volumes/reference), [Infrastructure as Code](https://docs.railway.com/infrastructure-as-code). Legacy `railway.toml`/`railway.json` configuration is deprecated and unavailable to new services, so this deployment does not introduce either file. Import the actual project into the current IaC system if configuration management becomes necessary.

## Verification and delivery

Focused results and macOS evidence live in `CombatJournalEvidence/`. The original dirty checkout and its running Editor are preserved; batch verification and development builds use an isolated checkout from current `main`. macOS synthetic pointer evidence is separate from physical-mobile validation.

Feature-branch documentation is published when merged. Public wiki deployment must run from validated `main`.

### Results from the original journal implementation

The following evidence was collected when combat journals were introduced. Later configuration and QA-connection integration is recorded separately in [Live operations verification](Live_Operations.en.md#verification-record).

- Unity Edit Mode passed the 488/488 expanded regression run, 155/155 journal/enemy/boss/localization follow-up and 137/137 final natural-death/localization/movement-response run. These overlap and must not be summed into a whole-project count. See [validation](CombatJournalEvidence/validation.json) and [final death tests](CombatJournalEvidence/editmode-defeat.xml).
- The macOS development build succeeded with zero build errors. Normal Update first advanced the live journal, followed by direct fixed-step simulation. An unforced stage-one victory reported approximately 102.8 simulation seconds, 43 kills, 2,276 gold and five collected items. Attendance and simulation duration are separately recorded; this is not a claim of 102.8 seconds of real-time play.
- Portrait 440×956, landscape 956×440 and PC 1600×900, 1600×1000 and 2100×900 were checked in Korean/English at 100%/140% text size: 20 live-HUD and 20 history cases with rendered-text and HUD-overlap assertions.
- One actual run and 99 labeled list fixtures exercised 100 rows. EventSystem raycasts and pointer down/up/click opened the rift records entry, all three result filters and chronological details. Synthetic list fixtures were removed before the final save.
- A separate application process restored the full journal. Edit Mode covered 101st-record pruning without deleting delivery evidence, failed saves, full queues, ACK/crash recovery and rejected-body/crash recovery.
- Unity client → localhost HTTP receiver → SQLite commit → matching ID/SHA-256 ACK → outbox removal passed. Server tests passed 9/9 and all five analytics queries executed against the received run. [Runtime evidence](CombatJournalEvidence/runtime.json) records file size and hash.
- Shared-UI contract checks and 9/9 regression tests, wiki build/check, 10/10 Python wiki tests and JavaScript wiki checks passed. Physical mobile and production service integration are outside the verified scope.

![Large Korean live journal](CombatJournalEvidence/live-portrait-ko-large.png)

![Large English battle records](CombatJournalEvidence/records-landscape-en-large.png)

[Chronological text](CombatJournalEvidence/chronological-log.png) · [Defeat filter fixtures](CombatJournalEvidence/defeat-filter-fixtures.png) · [Separate-process restore](CombatJournalEvidence/restart-records.png)

Implementation sources: [CombatJournal.cs](../../Assets/HELLSCRIPT/Runtime/Core/CombatJournal.cs) · [combat_telemetry.py](../../server/combat_telemetry.py) · [combat_analytics.sql](../../server/combat_analytics.sql) · [SHA-256](CombatJournalEvidence/source-digests.json)
