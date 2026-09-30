# Sites player log collection

Updated: 2026-10-01

[한국어](Sites_Player_Logs.md) · [Combat records](Combat_Journal_Server.en.md) · [Live operations](Live_Operations.en.md)

## Responsibilities

The collector is https://hellscript-player-logs.hoosung.chatgpt.site. A Sites Worker validates requests, stores original combat payloads in R2, and stores searchable summaries/conflict audits in D1. Google authentication and live operations continue on Railway. The authentication origin used to bind local account saves stays stable.

Existing Railway telemetry, operations and account databases remain intact. Earlier records remain there; records received by the new collector belong to Sites. This moves collection while retaining authentication and operations separately.

## Game connection

Version 2 of Resources/Data/ServerConnection.json uses **baseUrl** for authentication/live operations and **telemetryBaseUrl** for collection. Both must be HTTPS origins without credentials, paths or queries. Version 1 retains the single-origin behavior.

After Google sign-in opens the correct account store, the uploader binds to that account ID and reads only a matching session token from memory. A new login session for another account cannot upload the previous store's outbox before the store switches. Expiry and sign-out also stop new transmissions. Ordinary guests and the web guest edition retain local history without automatic server collection.

Editor/development builds may use a separate QA file whose **baseUrl** matches Sites and whose **kind** is hellscript-qa-telemetry. Operator keys, QA keys and the owner's private dispatch token never belong in game resources, saves, Git or the public wiki.

## Authentication and receipts

Sites introspects the Bearer token at the existing server's **GET /v1/telemetry/session**. Verified google: or qa: identities own records, independently of the client-claimed localAccountId. Email and display name are not collector identities. Authentication failures, timeouts and redirects fail closed.

**POST /v1/combat-runs** hashes the original bytes with SHA-256. Equal account/run ID/bytes returns the same receipt. Changed bytes return 409 without replacing the original. R2 storage, committed D1 metadata and an R2 storage check precede accepted/runId/payloadHash. The game deletes only the acknowledged outbox file; its recent 100-run player history is separate.

Limits are 16 MiB per payload, 20,000 events/drops per list, JSON depth 64, and the exact integer maximum 2^53−1. Per-account budgets are 120 requests and 64 MiB per minute; excess returns 429. Interrupted writes and retries never receive an early acknowledgment.

There are no public log reads, reward grants or save mutation endpoints. Site audience and game-token authorization are separate. Direct external game uploads require an internet-accessible Site while upload authorization and private log storage remain enforced.

## Deployment and operations

- server/sites/_worker.js implements the Worker.
- Reuse .openai/hosting.json's project ID and DB/LOGS bindings. Keep environment values and secrets out of that file.
- Set HELLSCRIPT_AUTH_ORIGIN through Sites runtime configuration and redeploy after changes.
- server/sites/publish.py exports only this service, verifies the pushed full SHA, and packages the Worker as dist/index.js. Unity sources and local credentials are excluded.
- Saving and deploying are separate; every deployment URL is production. Preserve project identity/audience and await terminal success.
- /healthz checks origin configuration, D1 schema reads and R2 access. Real authorization/uploads require separate verification.
- The homepage shows the live /healthz result in Korean and English, with a manual retry button. It replaces the placeholder that remained after sign-in and distinguishes public API access from required game authentication for uploads.

[Sites documentation](https://learn.chatgpt.com/docs/sites) specifies a 10 GB D1 limit per Site and no fixed R2 storage limit. Account-level usage limits also apply.

## Verification

Node tests use real SQLite queries for duplicates/conflicts, original bytes, interrupted storage, retries, revocation, validation boundaries and rejected redirects. Authentication-server tests cover QA/Google identity separation and revocation without granting legacy QA/operator authority.

verify_cloud.py uses synthetic payloads and a separately issued QA file to verify real Sites receipts and rejected conflicts, anonymous requests, invalid records and read routes. A private dispatch token stays only in process memory.

RuntimeTelemetrySitesSmoke completes a real rift in an isolated native save, uses the standard uploader, restarts it and resends identical bytes. It verifies outbox removal and retained player history. This is development evidence, distinct from physical-mobile testing.

## Source and current verification state

[Collector](../../server/sites/_worker.js) · [Status page code](../../server/sites/public/status.js) · [Node tests](../../server/sites/test_worker.mjs) · [Publisher](../../server/sites/publish.py) · [API verification](../../server/sites/verify_cloud.py) · [Game connection](../../Assets/HELLSCRIPT/Runtime/Presentation/GameServerConnection.cs) · [Native uploader verification](../../Assets/HELLSCRIPT/Runtime/Presentation/RuntimeTelemetrySitesSmoke.cs)

- Node 10/10 passed: original bytes, duplicates, conflicts, interrupted storage, revocation, request limits and rejected redirects. [Result](../../Artifacts/Validation/sites-player-logs-20260930/worker-tests.txt)
- Ten API checks passed on the owner-private deployment: matching first/duplicate receipts, changed payload 409, invalid provenance 422, anonymous/invalid credentials 401, listing 405 and individual read 404. D1 contains one synthetic run with its matching SHA-256. [Result](../../Artifacts/Validation/sites-player-logs-20260930/private-api.json)
- Public API access was enabled following the owner's explicit approval on 2026-10-01. Ten public API checks passed without a site access token or login cookies. Anonymous/invalid game credentials return 401, changed bytes return 409 and invalid provenance returns 422. [Public API result](../../Artifacts/Validation/sites-player-logs-20260930/public-api.json)
- An actual macOS development player used an isolated save and dedicated QA account, won rift stage 1 and uploaded 1,157,436 original bytes. Restarting the uploader and resending identical bytes produced the same acknowledgment, with no pending files and one retained local record. D1 contains exactly one matching run ID and SHA-256. [Native uploader result](../../Artifacts/Validation/sites-player-logs-20260930/public-native/native-uploader.json) · [D1 evidence](../../Artifacts/Validation/sites-player-logs-20260930/public-storage-proof.json)
- Account-token binding was verified by the earlier 41/41 focused checks and the affected connection checks at 11/11. Public access required no game-code changes, so the existing final development build, including optimization and daily content, was reused. Its executable SHA matched the build evidence, and Assets/Packages/ProjectSettings were unchanged since source ebea961a. Passing checks and builds were not repeated. Real Google-account uploads and physical mobile devices were not used in this verification. [Verification summary](../../Artifacts/Validation/sites-player-logs-20260930/validation-summary.json)
- The deployed status page was verified in Korean and English, including manual retry and a real healthy response. Healthy describes the collector/storage response and does not establish direct ordinary game uploads.
- The API verifier explicitly identifies itself as HELLSCRIPT-Telemetry-Verification/1.0. Sites rejected urllib's generic agent with Cloudflare 1010; actual Unity requests were accepted. Game authorization and hosting security settings were retained. The existing local development QA file retained its credential and a private backup while its collector origin changed to Sites.
- Authentication, operations and Sites [CI](https://github.com/dakrdong/HELLSCRIPT/actions/runs/36729003860) passed on main commit 3c33af8b. [D1 evidence](../../Artifacts/Validation/sites-player-logs-20260930/storage-proof.json) exposes only synthetic run IDs and hashes, excluding account IDs and tokens.
