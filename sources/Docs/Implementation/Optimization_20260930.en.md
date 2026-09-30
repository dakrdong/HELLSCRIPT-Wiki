# September30 optimization, forge and daily-quest integration

Updated on: 2026-10-01

Game changes and optimizations are integrated into actual main. Full EditMode, native and both builds completed after the last game-code change; no new regression was found, while47 known failures remain explicitly reported. Public access for the Sites collector still awaits direct user approval.

The initial local baseline was `4bd93ff3`. Separate worktrees preserved user and other owners' changes. Completed Claude forge `c92a7022` was integrated after explicit user confirmation. Existing main `94075e05` and Sites `3c33af8b` were identified by ancestry and included once. No reset/clean, forced checkout/push, branch or worktree deletion was used.

Core changes are in `69b37249`: reusable live-ID and removal buffers in WorldView; per-view outline point buffers preserving the old allocation-returning API; fixed-arity2/10 Combine overloads preserving sequential clamp and floating-point behavior; six unused128px legacy weapon images moved outside Resources with original bytes/GUID/meta preserved. Active512px weapon resources retain their quality. GUID, string/dynamic paths, saves/presets and editor/build consumers were audited.

`a4b577a8` removes forge ticker-snapshot, milestone-list and per-segment ring-array allocations while preserving callback snapshot semantics and exact point order. `946c3eaa` adds [five daily quests](Daily_Quests.en.md), success-only events, persisted manual claims, exactly100 coins, shared attendance midnight and KO/EN event UI. Development acceptance fixtures were corrected for actual admission, supported enhancement amounts and canvas rendering before pointer input without removing assertions. Final game commit `ebea961a` preserves both attendance automatic popup tracks when the daily page rolls over at midnight, strengthens native coverage, and updates the schema18 preservation test to current schema20 while retaining all equipment/rift/automation assertions.

The controlled **core-only** size comparison uses identical Unity6000.6.0f1/macOS/Mono/build options and scene. It predates later forge/Sites/daily content; the reductions do not describe the final app with added content.

| Delivered file bytes | Before | After | Reduction |
|---|---:|---:|---:|
| Development | 567,559,679 | 567,485,532 | 74,147 / 0.013064% |
| Release | 257,255,161 | 257,189,471 | 65,690 / 0.025535% |

Packed assets fell99,008B and texture totals104,352B; these measures are different and must not be added together. The six source PNGs totaling212,212B remain in the project. [Build evidence](OptimizationEvidence20260930/release-build-comparison.json), [move manifest](OptimizationEvidence20260930/legacy-rune-moves.json).

The final app with added content contains Development **567,685,593B** and Release **257,269,998B**, +125,914B/+14,837B from the initial baseline. Both builds succeeded with0errors. This separate forge/Sites/daily-inclusive result is not the controlled core-only reduction. [Final builds/source](OptimizationEvidence20260930/final-builds.json).

Native profiling used three sequential Development captures per side on AppleM3Pro/Mac15,7, Unity6000.6.0f1/Mono, PC1280×720/60cap/vsync0, identical synthetic stage30 Mage/seed93171/gear RNG9132026/save/LiveOps/map hashes, three-second warmup and20-second samples. Medians across runs are below. They compare core optimization, not subsequent integrated features.

| Metric | Before | After | Change |
|---|---:|---:|---:|
| World.Present mean | 465,678.254ns | 434,400.020ns | -6.7167% |
| Combat.Tick mean | 848,540.387ns | 752,190.292ns | -11.3548% |
| Mean GC allocated/frame | 193,151.533B | 189,537.327B | -1.8711% |
| Frame p50 | 16.73306ms | 16.70258ms | -0.1821% |
| Frame p95 | 21.57783ms | 21.33321ms | -1.1337% |
| Median peak RSS | 621.15625MiB | 630.56250MiB | **+1.5143%** |

Frames are capped, so no FPS, uncapped performance or battery claim follows. RSS increased and is distinct from Unity allocations; this is not whole-process memory reduction. GPU/render-thread and draw/batch counters were unavailable. Physical mobile, thermals and natural-play distributions remain unmeasured; host scheduling and short samples limit inference. [Profile summary and raw-source hashes](OptimizationEvidence20260930/native-profile-summary.json).

Verified Unity Recorder microtests count **allocation events**, not bytes: outlines60k calls370k→0 (244.0278→169.036ms), Combine200k calls200k→0 (26.8665→6.6243ms), ring1k calls48k→0 (31.5021→32.7178ms), tickers10k calls10k→0 (3.2377→3.0395ms), milestones100k calls100k→0 (8.0396→0.6553ms). The ring did not improve CPU time. Three meshes match original bytes and101 independently captured milestone values match exactly. A100-projectile/25-trap cleanup model changes5,375 equality checks/two temporary key arrays into125 hash lookups+125 adds/no temporary key arrays; these are model operation counts, not player CPU measurements. [Forge evidence](OptimizationEvidence20260930/forge-micro-comparison.json).

Fresh baseline reproduction confirmed the [existing47 failures](OptimizationEvidence20260930/known-failures-47.names.txt). The **pre-daily** integrated full EditMode suite completed4,904 total/4,857 passed/47 existing failed/0skipped on September30 16:38–17:08UTC, with exactly the same failure names and no new regressions. It is not the final daily-inclusive suite. Core focused158/158, forge focused200/200, daily/attendance/sales/potions/enhancement/localization focused170/170 and UI contract11/11 passed. Development builds succeeded with0errors. The final daily-inclusive full suite completed **4,931 total / 4,884 passed / exactly47 known failures / 0skipped**, September30 19:06:33–19:35:40UTC, 1,747.237seconds, after final game/main integration. Failure names exactly match fresh baseline reproduction, with0new failures. [Summary](OptimizationEvidence20260930/final-editmode-summary.json), [raw XML](OptimizationEvidence20260930/optimization-20260930-final-editmode.xml). Final daily/attendance/storage focused78/78 passed. The intermediate48th failure was an outdated schema19 expectation, corrected to current schema20 while retaining every preservation assertion; that earlier raw report remains available in the validation DB.

Final native **12/12smokes and16/16launches passed**, independently verified from actual commands, logs, reports, isolated saves and captures. Daily initial28.7s/fresh restart4.8s, attendance56.5s/fresh restart6.5s;12KO/EN viewport combinations, raycast/drag, midnight popup preservation, duplicate protection/exact100coins, clock rollback, empty write failure, four forge smokes and world/combat/repeat flows. [Results](OptimizationEvidence20260930/final-native-results.json), [evidence verification](OptimizationEvidence20260930/final-native-verification.json).

Pre-daily native9/10smokes,11/12launches passed. BattleLayout's old fixture selected an open tutorial map and failed to find a wall; the exact exception also occurred in the baseline player. A real-map/asynchronous-admission fixture subsequently passed. New Daily fixture corrections use two supported+1 enhancements and rendered pointer timing, retaining all assertions.

Server validation passed82/82 in an isolated Python3.12 environment with the existing declared requirements, Sites worker10/10 and operations web15/15. The initial systemPython3.9 run lacked CacheControl and skipped gunicorn; that environment failure is separate from code regressions. Global Python and dependency definitions were unchanged.

The existing owner-published Sites v7 deployment/source`40439a3b` was observed successful. Its custom audience remains one allowed user/no external visitors; Bearer/private datastore and Railway auth/ops/database boundaries are preserved. No new credentials, deployment or access change was made here. **Public game access and direct synthetic upload await the user's approval requested by the parent.** Historic private synthetic tests, fresh local/CI tests and public ingress verification are separate scopes.

Deferred scopes include unmeasured mass texture downscaling/compression, forge JobSignature/CoreQualityGauge redesign, uncapped/mobile/battery testing, real Google login and real-user log transmission. Independent branches/worktrees and old measurements are preserved. Revert optimizations through reviewed inverse commits; move legacy assets back with their GUID/meta rather than deleting them. Schema20 daily saves must remain compatible on rollback; do not drop claimed state or downgrade their schema. Full logs/XML/raw profiles/player/source hashes and restore copies remain in the local final report's evidence paths. Builds use ProjectBuilder.BuildMac/BuildMacRelease; native flags always specify isolated saves/evidence. Synthetic data is not inserted into real player saves or production log records.

Actual main game code is `ebea961a937c4c5ed59791f46c73683a7d33d62b`. All six local and three real remote branches are ancestors.175 changed paths and the six PNG/meta/GUID pairs passed pre/postimage checks;5,244 game input files match the independent validation worktree, and no feature/optimization remains worktree-only. [Applied paths](OptimizationEvidence20260930/main-source-applied-paths.txt). Documentation/wiki commits follow this unchanged game source. The local `evidence/FINAL-REPORT.md` records final remote SHA, CI, Pages and fresh logged-out browser evidence. The server tree matches prior remote94075, so its path-filtered CI is not counted as a newly triggered run.
