# WebGL optimization stages 0–1: results and next development plan

Updated: 2026-10-07 · [Korean report](WebGL_Optimization_20261006.md)

## Main integration and public release — 2026-10-07

The owner authorized main integration and public deployment. [PR #65](https://github.com/dakrdong/HELLSCRIPT/pull/65) is merged; the game source is `1147406775f39406c99223042b46fdb6c71803a1`. This also includes the later town-environment and tutorial re-entry changes from main `c0b33f3f`. Performance tables below remain the **pinned October 6 development-player comparison**, not a new measurement of this release player.

| Item | Result and scope |
| --- | --- |
| Integrated code | Ten Stage 1 owners remain byte-identical to the previous final source. Development profiling preserves latest main's real guest/button entry and uses the selected hero class. [Integration evidence](WebGLOptimization20261006Evidence/Release20261007/source-integration.json) |
| Affected validation | Optimization, stored-text, tutorial and town EditMode: **52 passed / 0 failed / 0 skipped**. Previously passing full tests and smokes are reused for unchanged coverage; this is not a full-suite rerun. [Results](WebGLOptimization20261006Evidence/Release20261007/integration-tests.json) |
| Generated evidence and CI | Regenerated the skill-tree source anchor from line 130 to 132 with the existing generator; retain the first CI failure. Subsequent PR/main Class skill data and Shared UI contract checks passed. [Main CI](WebGLOptimization20261006Evidence/Release20261007/main-ci.json) |
| Release WebGL | Unity 6000.6.0f1, IL2CPP, zero errors, no Development option, benchmark arguments or profiling page. **7 published files / 259,348,573 bytes**, reconstructed from 11 chunks with identical SHA-256. Loader tests 5/5 and package tests 5/5. [Build](WebGLOptimization20261006Evidence/Release20261007/web-build.json) · [Package](WebGLOptimization20261006Evidence/Release20261007/package-validation.json) |
| Primary player | [hellscript-game.github.io](https://hellscript-game.github.io/) · deployment `ec200f40` · [Pages success](https://github.com/hellscript-game/hellscript-game.github.io/actions/runs/37519716373) |
| Previous player | [Existing-save address](https://dakrdong.github.io/HELLSCRIPT-Web/) · deployment `cfe60e14` · [Pages success](https://github.com/dakrdong/HELLSCRIPT-Web/actions/runs/37519746846), attempt 2 of the same commit. Cancelled only the owned waiting run and retried once; approval/security policies unchanged. |
| Actual public interaction | Logged-out macOS in-app browser: primary loading, Start, Guest, Warrior selection, KO/EN town and KO restoration. Previous address: loading, Start and actual Unity title. Both live build-info manifests match the source and seven file hashes. Zero console errors; primary retains one known URP FSR warning and eight Content-Length warnings. [Browser receipt](WebGLOptimization20261006Evidence/Release20261007/public-browser.json) |
| Original work protection | Safely fast-forwarded original main while preserving all 15 dirty entries and the modified Actors file hash. [Receipt](WebGLOptimization20261006Evidence/Release20261007/original-main-update.json) |

Pinned all **6,083** Assets/Packages/ProjectSettings inputs in the [release source receipt](WebGLOptimization20261006Evidence/Release20261007/release-source-sha256.json). Preserved then restored generated build changes in four settings/asset paths. The public wiki mirrors the full merged-main documents, history, databases, images and evidence at the [read-only address](https://dakrdong.github.io/HELLSCRIPT-Wiki/#/tree); final Pages/document/database verification receipts are retained in the task-owned `release-20261007` directory.

This is a default-text-size PC functional release check. It is not a new complete combat/reward/save smoke, release-performance A/B, 30-minute soak or physical Android/iPhone validation. **Stages 2–6 remain unstarted and dense-combat stable 60 FPS remains unachieved.** Retain comparison players and raw data through the October 13 review. Add this release, reconstruction and generated wiki copies to the lifecycle manifest; exact-path permanent cleanup remains pending authorization.

![Published Korean town](WebGLOptimization20261006Evidence/Release20261007/public-game-ko.jpg)

![Published English town](WebGLOptimization20261006Evidence/Release20261007/public-game-en.jpg)

## October 6 development validation record

The following preserves the original implementation, fixed-condition measurements and validation boundaries. Later integration/publication status is in the October 7 addendum above.


## Result and scope

Implemented profiling and the six immediate optimizations on `codex/webgl-optimization-stage0-1`. Latest main `945fa46f` was adopted through `c53a144a`; baseline tooling is `4b1c1916`. Measured optimized players use this parent plus the preserved Stage 1 runtime patch and source hash receipt. A pre-existing shared stored-message translation bug found during final smoke was also repaired and checked in separate final development players. Measured players and final validation players have distinct source/binary receipts and are not substituted for each other. Historical players from the older main are excluded from comparisons.

Nineteen WebGL conditions have three interleaved before/after samples each: **114 valid samples**. Save bytes decreased by **62.0–67.4%** and per-frame GC allocation by **43.2–77.7%**, using condition medians; all 19 repeated ranges are disjoint. Frame p95 and actual FPS improved in 18/19 conditions. A valid slow optimized Warrior/OFF/4× sample was retained, so frame improvement in that condition is not accepted.

**Dense combat p95 remains above 16.7 ms. Stable 60 FPS and removal of all stutters are not achieved.** Native macOS development players and the Codex in-app WebGL development browser are the tested platforms. Physical Android/iPhone, ordinary Chrome/Safari/Firefox, release builds and a 30-minute soak are unverified.

Preserved 20 Hz combat, 0.2-second decisions, 3-second autosaves, 0.15-second HUD refresh, RNG, event order, rewards, transaction validation, schema 21 and server V1. Stages 2–6 and V2 storage are not implemented. PR creation, feature-to-main integration and public wiki publishing require later owner confirmation under the final prompt.

## Implementation and invalidation

- Compact account JSON changes whitespace only. Atomic replacement, backup, archive synchronization, validation and cadence remain. Pretty primary files and corrupt-primary/pretty-backup recovery are covered.
- Empty traps reset alive-enemy trap timers. Empty blizzards retain the original dt condition and remove the matching hero M02 area statuses. Nonempty sorting remains unchanged. Moving the captured blizzard body into the active-effects helper removed an actual empty-path closure allocation.
- Without a living nonboss aura source, reset sources and append removal events in the original order. All three refresh calls per tick remain.
- Minimap dirtying compares displayed chest, observation/expiry/visibility, shrine, visited seal, carrier, offering/altar, gate, boss, expanded and layout state. Movement and fog revision paths remain. Stable state reuses the list.
- Live journal keys use displayed row sequence bounds/count, journal identity, language, size and current action; explicit changes still invalidate. Hidden preview events remain in the stored journal. EventLimit eviction is covered.
- Cache boss definition identifiers, preserving the first enum mapping. Resolve translated names dynamically. The entire AttackName path, including ClassSkills.Find, is not claimed to allocate zero.
- Repair the pre-existing shared stored-message translator: joined complete messages ending in a target identifier must reach their full template. Preserve exact standalone ordinary/named entity lookup and serialized event bytes. No performance effect is claimed for this later presentation repair.

## Validation and limitations

The original focused baseline passed 90 tests. The first Stage 1 focused run passed 116/118; two new strict GC checks failed. The owner authorized diagnosis and repair while retaining criteria. The boss test needed warming of its exact measurement delegate; the blizzard fast path had a real closure allocation. The repaired empty-path check and five equivalence cases passed, as did the boss check. No tolerances were loosened or checks removed.

Latest-main simulation before/after captures each passed their explicit test. All **108** class/edict/density/scenario combinations have byte-identical complete RunState/journal dumps, including RNG, events, outcomes and rewards. Only wall-clock/build identifiers and presentation IDs are canonicalized, with the RNG-relevant development run ID fixed. Unsupported zero thread-allocation counters are not used as proof; strict Unity GC constraints have a positive allocation control.

| Validation | Result | Scope and evidence |
| --- | --- | --- |
| Final full unfiltered EditMode suite, once | **5,221 passed / 0 failed / 2 skipped** | Total 5,223. Executed before the later translation repair; reuse success for unchanged coverage. The skipped tests are explicit measurement tools; the WebGL capture tool passed separate before/after runs. |
| Affected stored-text and complete-journal tests after repair | **25 passed / 0 failed / 0 skipped** | Targets -1/0/53, ordinary/named boss identities, natural battle/defeat logs and unchanged serialized events |
| Full state/journal comparison | All 108 cases identical | Latest-main baseline, fixed inputs, exact bytes and SHA-256 |
| Shared UI source ownership | Passed | `check_ui_contract.py` |
| UI contract tool tests | 11 passed | `test_ui_contract.py` |
| UI refresh rules | Passed | 12 guarded bindings, 161 inventoried UI sources |
| macOS minimap smoke | Passed | 94 PASS entries, 129 screenshots; KO/EN, portrait/landscape and PC aspect ratios, small screens and simulated safe area, synthetic pointer state changes |
| macOS live-journal smoke | **Passed after repair** | 24 compact/expanded KO/EN cases at 440×956, 956×440, 1600×900, 1600×1000, 2100×900 and 512×288, default text size; immutable journal, row capacity, clipping/overlap and synthetic potion input/state restoration |
| macOS UI refresh smoke | Passed | 26 page/language combinations, 78 screenshots; three trials of five unchanged saves, real mutation, held input and child deferral. Synchronous Save+binding+Canvas observations do not prove full-frame or every popup/scroll/rapid-navigation stability. |
| Final macOS and WebGL development builds | Both succeeded, zero errors | Separate final outputs containing the translation repair; raw web 7 files, instrumented variant 8 files, native 323 files; all 4,042 pinned final source files match |
| Final local WebGL combat/log/save | KO/EN functional proof | Each: 120 combat ticks, two writes, nine IDBFS callbacks without error, full-journal POST 204, no runtime exception. Actual canvas 1920×1080/DPR 2 excludes these runs from matched performance comparisons. |
| Post-measurement integrity | Passed | Both web players: 8 files each; both native players: 323 files each. Runtime source matches. Two build-generated ProjectSettings files were restored to repository values. |
| Local wiki generation and checks | Passed | Python: 12 tests. Node: 532 pages, 33 databases, 4,589 details, matching read-only mirrors and date-only rendering. Actual results retained in `final-document-validation.json`. No public publication. |

The initial missing-localization smoke failure was reproduced in the immutable baseline player. The owner instructed continuation with unchanged criteria. The first repair failed an existing named-boss translation case (72/73 passed); retaining exact standalone entity lookup fixed it. All 25 directly affected tests and the 24-case journal smoke then passed. Preserve the original failure and both repair attempts.

The later change affects presentation translation only. The successful full EditMode suite, minimap smoke and static UI checks were reused, not rerun or represented as a complete rerun of the final source. Only affected translation tests, the failed journal smoke, the remaining UI refresh smoke and separate final development builds were executed. Stages 0–1 implementation and this stated validation scope are complete; long-run, mobile and release performance remain unverified.

The final KO WebGL run observed about 119 FPS (reported rAF rate 60, timing divisor 1); EN observed about 59 FPS (reported rate 120, divisor 2). Both had DPR 2. The reported rate alone does not establish actual pacing. Preserve both strict rejection reasons, visibility/resolution/focus mismatch and insufficient workload: **neither run was added to the 114 performance samples**. They support local language/combat/log/storage functionality only.

![Final native English portrait journal](WebGLOptimization20261006Evidence/final-journal-portrait-en.png)

![Final local WebGL English journal](WebGLOptimization20261006Evidence/final-web-journal-en.png)

The first wiki build stopped on existing ignored historical input missing from this worktree. Materialized only the 283 referenced original validation attachments (122,724,222 bytes) and verified identical SHA-256; did not modify originals or copy project/build/cache directories. Historical input restoration is not a newly passed test. Preserve the first build log and the exact source/destination receipt.

A 20-second **baseline** expanded-log diagnostic included normal completion and auto-repeat admission delays. Its run ID advanced, and only 293 combat ticks occurred. The active-combat criterion was retained and this sample excluded from improvement comparisons. Its 13 writes and 68,498,579 cumulative bytes include transactions/results/re-entry; they are not an autosave count. Separating active time from normal repeat waits is required for long-window acceptance. The 60-second bounded sampler and longer duration flags are implemented; a 30-minute soak remains unrun.

The known second EdictProgressionSmoke attendance stall was not rerun because it is outside the changed acceptance paths. No enlarged-text runs, physical mobile tests, public-player smoke, PR, main integration or public deployment were performed.

## Measurement discipline

Web conditions: same seed/gear/map, fixed development run identities, IL2CPP, 1280×720, DPR 1, Mobile quality, ordinary 60 Hz rAF, no fixed timer; six-second capture after one-second warmup. Every valid web window has two writes and no observed IDBFS callback error. Additional enemies are development-only on the validated original map; production LiveOps and placement safety remain.

The native comparison has 12 samples: Mage/OFF/1× and Mage/ON/4×, three before/after launches each. Order is before/after, after/before, before/after. Builds, tests and measurements run serially. Hashes were pinned after development instrumentation and checked again afterward.

These are combined optimizations; the contribution of each edit has not been isolated by separate A/B players. Per-frame GC and marker statistics include the observed FPS difference and are not matched-pacing CPU claims. Keep mean/p95/p99/max/totals/counts and time-axis spikes. Zero p95/p99 for sparse saves does not mean zero cost. Do not sum nested markers. Web CDP ProcessTime lacks independent renderer-process verification; native CPU is actual whole-process CPU, but differing actual FPS prevents accepting matched-pacing CPU/frame reductions. Marker duration is not the same as pure process CPU.

Managed heap ranges overlap: before 29.8–36.2 MiB, after 29.3–34.8 MiB. Overall memory reduction is not accepted. Unity allocation, native RSS, JS heap and collections remain separate evidence.

### WebGL comparison by condition

Ranges cover three samples each. MB = 1,000,000 bytes; KiB = 1,024 bytes. Saves are per call and GC is the per-frame mean. Verdict is frame p95; save-byte and GC ranges are disjoint in every row.

| Condition | Actual FPS before to after | Save MB/call before to after | GC KiB/frame before to after | Frame p95 ms before to after | Verdict |
| --- | --- | --- | --- | --- | --- |
| Mage OFF 1x | 59.60~59.68 → 60.02~60.06 | 2.13~2.16 → 0.69~0.71 | 124.52~124.69 → 40.93~41.28 | 23.00~24.00 → 22.00~22.00 | Accepted |
| Mage OFF 2x | 59.39~59.50 → 59.84~59.86 | 2.84~2.84 → 0.97~0.97 | 208.60~210.05 → 58.28~58.60 | 26.00~28.00 → 22.00~24.00 | Accepted |
| Mage OFF 4x | 54.98~55.89 → 59.34~59.45 | 4.14~4.14 → 1.45~1.46 | 401.67~406.16 → 92.17~92.41 | 36.00~38.00 → 24.00~25.00 | Accepted |
| Mage ON 1x | 59.67~59.68 → 59.90~60.05 | 2.15~2.25 → 0.75~0.75 | 166.22~167.62 → 82.87~83.34 | 23.00~24.00 → 22.00~23.00 | Accepted |
| Mage ON 2x | 59.26~59.75 → 59.96~60.01 | 2.99~2.99 → 0.98~0.99 | 251.82~254.32 → 99.98~100.76 | 26.00~27.00 → 22.00~23.00 | Accepted |
| Mage ON 4x | 51.24~55.31 → 59.35~59.57 | 4.38~4.38 → 1.56~1.56 | 455.82~494.14 → 138.26~139.40 | 39.00~41.00 → 25.00~26.00 | Accepted |
| Mage ON 4x expanded | 49.78~55.10 → 59.22~59.56 | 4.16~4.38 → 1.56~1.56 | 467.00~514.81 → 147.72~149.37 | 38.00~42.00 → 25.00~25.00 | Accepted |
| Ranger OFF 1x | 59.68~59.70 → 60.00~60.07 | 2.17~2.26 → 0.75~0.75 | 125.64~126.87 → 48.93~49.00 | 23.00~23.00 → 22.00~22.00 | Accepted |
| Ranger OFF 2x | 59.27~59.46 → 60.01~60.06 | 2.96~3.05 → 0.99~1.05 | 211.29~212.10 → 71.38~72.17 | 26.00~27.00 → 23.00~23.00 | Accepted |
| Ranger OFF 4x | 54.99~55.72 → 59.72~59.73 | 4.43~4.54 → 1.63~1.63 | 408.06~410.41 → 118.96~119.18 | 36.00~37.00 → 24.00~25.00 | Accepted |
| Ranger ON 1x | 59.53~59.68 → 60.06~60.07 | 2.22~2.26 → 0.71~0.75 | 193.54~193.97 → 109.78~110.07 | 25.00~25.00 → 23.00~23.00 | Accepted |
| Ranger ON 2x | 58.50~58.92 → 59.80~60.01 | 2.83~3.00 → 1.00~1.03 | 283.19~287.07 → 130.59~131.29 | 29.00~32.00 → 24.00~27.00 | Accepted |
| Ranger ON 4x | 45.50~51.81 → 58.17~58.24 | 4.25~4.33 → 1.54~1.54 | 520.00~589.76 → 171.88~172.54 | 42.00~55.00 → 27.00~28.00 | Accepted |
| Warrior OFF 1x | 59.52~59.68 → 60.00~60.06 | 2.19~2.26 → 0.72~0.75 | 123.75~124.78 → 40.87~41.73 | 23.00~23.00 → 22.00~22.00 | Accepted |
| Warrior OFF 2x | 59.11~59.59 → 59.85~60.02 | 3.09~3.11 → 1.08~1.08 | 206.57~208.20 → 58.14~58.15 | 26.00~27.00 → 22.00~22.00 | Accepted |
| Warrior OFF 4x | 53.41~56.21 → 43.09~59.55 | 4.27~4.63 → 1.56~1.67 | 394.37~410.20 → 90.24~121.62 | 37.00~38.00 → 24.00~46.00 | Not accepted |
| Warrior ON 1x | 59.52~59.60 → 60.00~60.06 | 2.30~2.35 → 0.72~0.76 | 174.92~175.94 → 91.44~91.47 | 24.00~25.00 → 23.00~24.00 | Accepted |
| Warrior ON 2x | 58.81~59.34 → 59.52~60.02 | 2.93~3.11 → 1.08~1.08 | 260.42~263.52 → 109.91~110.84 | 28.00~28.00 → 24.00~25.00 | Accepted |
| Warrior ON 4x | 52.50~56.94 → 59.35~59.58 | 4.58~4.58 → 1.65~1.67 | 445.86~483.62 → 145.42~146.48 | 38.00~39.00 → 26.00~26.00 | Accepted |

### Marker mean / p95 / p99 / maximum

Units are ms/frame. Mean/p95/p99 are medians of three sample statistics; maximum is the largest across all three. Do not sum nested markers. JSON retains ranges, totals and counts.

#### HELLSCRIPT.Save

| Condition | Before mean / p95 / p99 / maximum | After mean / p95 / p99 / maximum |
| --- | --- | --- |
| Mage OFF 1x | 0.133 / 0.000 / 0.000 / 25.300 | 0.082 / 0.000 / 0.000 / 15.600 |
| Mage OFF 2x | 0.172 / 0.000 / 0.000 / 32.600 | 0.099 / 0.000 / 0.000 / 18.800 |
| Mage OFF 4x | 0.248 / 0.000 / 0.000 / 42.200 | 0.139 / 0.000 / 0.000 / 25.600 |
| Mage ON 1x | 0.140 / 0.000 / 0.000 / 26.800 | 0.089 / 0.000 / 0.000 / 16.800 |
| Mage ON 2x | 0.179 / 0.000 / 0.000 / 33.600 | 0.104 / 0.000 / 0.000 / 19.000 |
| Mage ON 4x | 0.281 / 0.000 / 0.000 / 48.800 | 0.149 / 0.000 / 0.000 / 27.700 |
| Mage ON 4x expanded | 0.285 / 0.000 / 0.000 / 49.700 | 0.148 / 0.000 / 0.000 / 27.600 |
| Ranger OFF 1x | 0.132 / 0.000 / 0.000 / 26.200 | 0.090 / 0.000 / 0.000 / 19.900 |
| Ranger OFF 2x | 0.182 / 0.000 / 0.000 / 34.400 | 0.110 / 0.000 / 0.000 / 20.700 |
| Ranger OFF 4x | 0.271 / 0.000 / 0.000 / 50.600 | 0.150 / 0.000 / 0.000 / 29.100 |
| Ranger ON 1x | 0.135 / 0.000 / 0.000 / 26.300 | 0.088 / 0.000 / 0.000 / 17.200 |
| Ranger ON 2x | 0.175 / 0.000 / 0.000 / 35.100 | 0.105 / 0.000 / 0.000 / 20.900 |
| Ranger ON 4x | 0.360 / 0.000 / 0.000 / 59.100 | 0.152 / 0.000 / 0.000 / 29.700 |
| Warrior OFF 1x | 0.134 / 0.000 / 0.000 / 27.400 | 0.090 / 0.000 / 0.000 / 18.200 |
| Warrior OFF 2x | 0.191 / 0.000 / 0.000 / 37.000 | 0.113 / 0.000 / 0.000 / 22.200 |
| Warrior OFF 4x | 0.276 / 0.000 / 0.000 / 58.100 | 0.161 / 0.000 / 0.000 / 160.900 |
| Warrior ON 1x | 0.140 / 0.000 / 0.000 / 27.800 | 0.091 / 0.000 / 0.000 / 17.500 |
| Warrior ON 2x | 0.179 / 0.000 / 0.000 / 38.000 | 0.118 / 0.000 / 0.000 / 23.600 |
| Warrior ON 4x | 0.285 / 0.000 / 0.000 / 54.900 | 0.153 / 0.000 / 0.000 / 29.400 |

#### HELLSCRIPT.Journal.Append

| Condition | Before mean / p95 / p99 / maximum | After mean / p95 / p99 / maximum |
| --- | --- | --- |
| Mage OFF 1x | 0.002 / 0.000 / 0.100 / 0.500 | 0.003 / 0.000 / 0.100 / 0.400 |
| Mage OFF 2x | 0.003 / 0.000 / 0.100 / 0.400 | 0.003 / 0.000 / 0.100 / 0.500 |
| Mage OFF 4x | 0.003 / 0.000 / 0.100 / 0.500 | 0.003 / 0.000 / 0.100 / 0.400 |
| Mage ON 1x | 0.004 / 0.000 / 0.100 / 0.300 | 0.003 / 0.000 / 0.100 / 0.200 |
| Mage ON 2x | 0.004 / 0.000 / 0.100 / 0.300 | 0.004 / 0.000 / 0.100 / 0.400 |
| Mage ON 4x | 0.008 / 0.000 / 0.200 / 0.400 | 0.006 / 0.000 / 0.200 / 0.400 |
| Mage ON 4x expanded | 0.007 / 0.000 / 0.200 / 0.600 | 0.006 / 0.000 / 0.200 / 0.500 |
| Ranger OFF 1x | 0.006 / 0.100 / 0.100 / 0.300 | 0.007 / 0.000 / 0.200 / 0.300 |
| Ranger OFF 2x | 0.007 / 0.000 / 0.200 / 0.300 | 0.009 / 0.100 / 0.200 / 0.400 |
| Ranger OFF 4x | 0.014 / 0.100 / 0.300 / 0.700 | 0.013 / 0.100 / 0.300 / 0.700 |
| Ranger ON 1x | 0.005 / 0.000 / 0.100 / 0.200 | 0.005 / 0.000 / 0.100 / 0.300 |
| Ranger ON 2x | 0.008 / 0.000 / 0.200 / 0.500 | 0.006 / 0.000 / 0.200 / 0.600 |
| Ranger ON 4x | 0.011 / 0.100 / 0.200 / 0.400 | 0.008 / 0.000 / 0.200 / 0.500 |
| Warrior OFF 1x | 0.004 / 0.000 / 0.100 / 0.300 | 0.004 / 0.000 / 0.100 / 0.400 |
| Warrior OFF 2x | 0.006 / 0.000 / 0.200 / 0.500 | 0.005 / 0.000 / 0.100 / 0.500 |
| Warrior OFF 4x | 0.011 / 0.000 / 0.300 / 0.600 | 0.011 / 0.000 / 0.300 / 1.600 |
| Warrior ON 1x | 0.005 / 0.000 / 0.100 / 0.300 | 0.004 / 0.000 / 0.100 / 0.400 |
| Warrior ON 2x | 0.008 / 0.000 / 0.200 / 0.400 | 0.009 / 0.000 / 0.200 / 0.700 |
| Warrior ON 4x | 0.011 / 0.100 / 0.300 / 0.600 | 0.011 / 0.000 / 0.300 / 0.600 |

#### HELLSCRIPT.Visibility.Update

| Condition | Before mean / p95 / p99 / maximum | After mean / p95 / p99 / maximum |
| --- | --- | --- |
| Mage OFF 1x | 0.372 / 3.300 / 3.700 / 7.400 | 0.366 / 3.200 / 3.600 / 4.000 |
| Mage OFF 2x | 0.352 / 3.200 / 3.700 / 4.500 | 0.348 / 3.200 / 3.600 / 6.500 |
| Mage OFF 4x | 0.362 / 3.200 / 3.600 / 5.800 | 0.367 / 3.200 / 3.600 / 4.100 |
| Mage ON 1x | 0.235 / 3.400 / 3.700 / 3.800 | 0.235 / 3.300 / 3.700 / 7.800 |
| Mage ON 2x | 0.228 / 3.400 / 3.700 / 5.100 | 0.252 / 3.400 / 3.700 / 4.100 |
| Mage ON 4x | 0.243 / 3.400 / 3.700 / 4.600 | 0.228 / 3.400 / 3.700 / 4.100 |
| Mage ON 4x expanded | 0.280 / 3.400 / 3.800 / 7.700 | 0.228 / 3.400 / 3.700 / 3.800 |
| Ranger OFF 1x | 0.362 / 3.500 / 4.500 / 4.700 | 0.348 / 3.500 / 4.400 / 4.800 |
| Ranger OFF 2x | 0.356 / 3.500 / 4.500 / 6.200 | 0.350 / 3.500 / 4.600 / 5.000 |
| Ranger OFF 4x | 0.353 / 3.600 / 4.500 / 5.500 | 0.338 / 3.500 / 4.500 / 6.400 |
| Ranger ON 1x | 0.230 / 3.200 / 3.400 / 3.600 | 0.234 / 3.200 / 3.500 / 3.900 |
| Ranger ON 2x | 0.255 / 3.300 / 3.600 / 5.000 | 0.236 / 3.300 / 3.600 / 7.300 |
| Ranger ON 4x | 0.287 / 3.300 / 4.400 / 6.700 | 0.244 / 3.300 / 3.800 / 6.400 |
| Warrior OFF 1x | 0.447 / 3.600 / 4.200 / 5.000 | 0.430 / 3.600 / 4.100 / 5.000 |
| Warrior OFF 2x | 0.452 / 3.900 / 4.400 / 7.700 | 0.440 / 3.800 / 4.100 / 5.000 |
| Warrior OFF 4x | 0.425 / 3.800 / 4.100 / 5.000 | 0.379 / 3.900 / 4.800 / 28.400 |
| Warrior ON 1x | 0.405 / 3.600 / 4.100 / 5.300 | 0.393 / 3.600 / 4.100 / 5.100 |
| Warrior ON 2x | 0.416 / 3.700 / 4.100 / 6.000 | 0.416 / 3.800 / 4.400 / 7.000 |
| Warrior ON 4x | 0.453 / 3.800 / 4.100 / 5.200 | 0.393 / 3.600 / 4.000 / 5.000 |

#### HELLSCRIPT.Minimap.Rebuild

| Condition | Before mean / p95 / p99 / maximum | After mean / p95 / p99 / maximum |
| --- | --- | --- |
| Mage OFF 1x | 0.888 / 4.200 / 4.500 / 8.600 | 0.473 / 4.100 / 4.200 / 5.000 |
| Mage OFF 2x | 0.860 / 4.200 / 4.500 / 7.200 | 0.441 / 4.100 / 4.200 / 5.300 |
| Mage OFF 4x | 0.909 / 4.200 / 4.300 / 6.200 | 0.471 / 4.100 / 4.200 / 5.500 |
| Mage ON 1x | 0.690 / 4.100 / 4.300 / 6.000 | 0.284 / 4.000 / 4.100 / 8.700 |
| Mage ON 2x | 0.700 / 4.200 / 4.300 / 7.400 | 0.294 / 4.000 / 4.200 / 4.400 |
| Mage ON 4x | 0.815 / 4.200 / 4.700 / 6.900 | 0.277 / 4.100 / 4.300 / 5.100 |
| Mage ON 4x expanded | 0.770 / 4.200 / 4.300 / 6.400 | 0.278 / 4.000 / 4.300 / 5.600 |
| Ranger OFF 1x | 0.774 / 4.100 / 4.200 / 4.900 | 0.385 / 4.100 / 4.200 / 5.200 |
| Ranger OFF 2x | 0.805 / 4.200 / 4.700 / 5.600 | 0.394 / 4.100 / 4.200 / 4.700 |
| Ranger OFF 4x | 0.823 / 4.200 / 4.300 / 4.900 | 0.384 / 4.100 / 4.300 / 8.500 |
| Ranger ON 1x | 0.726 / 4.200 / 4.400 / 6.200 | 0.304 / 4.100 / 4.400 / 5.700 |
| Ranger ON 2x | 0.743 / 4.200 / 4.700 / 9.000 | 0.296 / 4.000 / 4.400 / 6.000 |
| Ranger ON 4x | 0.891 / 4.700 / 6.100 / 8.100 | 0.301 / 4.100 / 4.500 / 6.600 |
| Warrior OFF 1x | 0.899 / 4.200 / 4.600 / 5.700 | 0.502 / 4.200 / 4.800 / 6.800 |
| Warrior OFF 2x | 0.920 / 4.200 / 4.600 / 6.200 | 0.486 / 4.100 / 4.500 / 5.900 |
| Warrior OFF 4x | 0.947 / 4.200 / 5.300 / 8.600 | 0.402 / 4.100 / 4.500 / 15.800 |
| Warrior ON 1x | 0.866 / 4.200 / 4.300 / 5.600 | 0.443 / 4.100 / 4.200 / 4.800 |
| Warrior ON 2x | 0.837 / 4.200 / 4.300 / 6.400 | 0.454 / 4.100 / 4.300 / 6.200 |
| Warrior ON 4x | 0.916 / 4.200 / 4.300 / 4.800 | 0.442 / 4.000 / 4.200 / 4.500 |

#### HELLSCRIPT.Edict.Decision

| Condition | Before mean / p95 / p99 / maximum | After mean / p95 / p99 / maximum |
| --- | --- | --- |
| Mage OFF 1x | 0.067 / 0.600 / 0.800 / 5.700 | 0.065 / 0.600 / 0.700 / 5.400 |
| Mage OFF 2x | 0.112 / 1.000 / 1.400 / 9.400 | 0.109 / 1.000 / 1.300 / 9.400 |
| Mage OFF 4x | 0.189 / 1.700 / 2.100 / 16.900 | 0.206 / 2.100 / 2.400 / 17.500 |
| Mage ON 1x | 0.324 / 3.600 / 4.400 / 6.400 | 0.345 / 3.600 / 4.800 / 7.300 |
| Mage ON 2x | 0.399 / 4.200 / 5.400 / 11.600 | 0.394 / 4.100 / 5.400 / 11.400 |
| Mage ON 4x | 0.754 / 6.400 / 7.800 / 34.200 | 0.645 / 5.900 / 8.600 / 30.000 |
| Mage ON 4x expanded | 0.703 / 6.000 / 8.300 / 29.200 | 0.659 / 6.000 / 7.900 / 31.400 |
| Ranger OFF 1x | 0.079 / 0.800 / 1.100 / 4.200 | 0.079 / 0.800 / 1.100 / 4.500 |
| Ranger OFF 2x | 0.119 / 1.200 / 1.400 / 6.600 | 0.121 / 1.200 / 1.500 / 8.300 |
| Ranger OFF 4x | 0.229 / 2.200 / 2.700 / 11.900 | 0.223 / 2.300 / 2.800 / 12.700 |
| Ranger ON 1x | 0.593 / 4.100 / 14.900 / 19.100 | 0.578 / 4.100 / 14.100 / 19.700 |
| Ranger ON 2x | 0.832 / 5.200 / 19.600 / 25.600 | 0.818 / 5.300 / 19.600 / 24.300 |
| Ranger ON 4x | 1.760 / 9.900 / 35.300 / 39.100 | 1.243 / 7.700 / 30.200 / 34.500 |
| Warrior OFF 1x | 0.056 / 0.600 / 0.900 / 1.400 | 0.059 / 0.600 / 0.900 / 1.300 |
| Warrior OFF 2x | 0.100 / 1.000 / 1.600 / 1.900 | 0.097 / 1.000 / 1.600 / 2.700 |
| Warrior OFF 4x | 0.226 / 2.000 / 3.900 / 4.100 | 0.220 / 2.000 / 4.100 / 17.700 |
| Warrior ON 1x | 0.374 / 3.900 / 5.200 / 8.600 | 0.376 / 4.000 / 5.400 / 8.000 |
| Warrior ON 2x | 0.463 / 5.000 / 6.800 / 10.100 | 0.484 / 5.300 / 7.100 / 14.500 |
| Warrior ON 4x | 0.640 / 6.900 / 9.100 / 10.800 | 0.609 / 6.900 / 9.000 / 10.800 |

#### HELLSCRIPT.Combat.Tick.Enemies

| Condition | Before mean / p95 / p99 / maximum | After mean / p95 / p99 / maximum |
| --- | --- | --- |
| Mage OFF 1x | 0.254 / 0.800 / 0.900 / 3.600 | 0.131 / 0.500 / 0.600 / 2.900 |
| Mage OFF 2x | 0.669 / 2.200 / 2.900 / 3.900 | 0.257 / 1.000 / 1.300 / 2.000 |
| Mage OFF 4x | 1.977 / 5.900 / 6.000 / 10.800 | 0.475 / 1.800 / 2.100 / 3.000 |
| Mage ON 1x | 0.220 / 0.800 / 0.900 / 2.300 | 0.119 / 0.500 / 0.900 / 1.100 |
| Mage ON 2x | 0.567 / 1.900 / 2.100 / 3.100 | 0.194 / 0.800 / 1.000 / 1.700 |
| Mage ON 4x | 2.044 / 6.000 / 6.800 / 11.500 | 0.377 / 1.600 / 1.800 / 2.300 |
| Mage ON 4x expanded | 1.966 / 6.000 / 6.700 / 14.600 | 0.386 / 1.600 / 1.800 / 3.000 |
| Ranger OFF 1x | 0.233 / 0.800 / 1.100 / 1.700 | 0.125 / 0.500 / 0.800 / 1.300 |
| Ranger OFF 2x | 0.599 / 2.000 / 2.300 / 3.400 | 0.214 / 0.900 / 1.200 / 2.200 |
| Ranger OFF 4x | 1.910 / 5.800 / 7.700 / 10.600 | 0.426 / 1.700 / 2.500 / 4.100 |
| Ranger ON 1x | 0.209 / 0.700 / 1.100 / 1.700 | 0.093 / 0.400 / 0.600 / 1.400 |
| Ranger ON 2x | 0.610 / 1.900 / 2.900 / 5.800 | 0.201 / 0.800 / 1.200 / 2.600 |
| Ranger ON 4x | 2.731 / 7.800 / 9.100 / 17.600 | 0.388 / 1.400 / 2.300 / 9.200 |
| Warrior OFF 1x | 0.185 / 0.700 / 0.900 / 2.800 | 0.073 / 0.400 / 0.500 / 2.600 |
| Warrior OFF 2x | 0.508 / 1.800 / 2.100 / 4.300 | 0.127 / 0.700 / 0.900 / 3.200 |
| Warrior OFF 4x | 1.869 / 5.700 / 6.400 / 10.900 | 0.272 / 1.500 / 2.000 / 20.100 |
| Warrior ON 1x | 0.191 / 0.700 / 1.000 / 3.200 | 0.079 / 0.400 / 0.500 / 2.600 |
| Warrior ON 2x | 0.549 / 2.000 / 2.300 / 4.700 | 0.160 / 0.800 / 1.700 / 2.800 |
| Warrior ON 4x | 1.806 / 5.600 / 6.100 / 10.400 | 0.259 / 1.400 / 1.800 / 3.500 |

### Native actual process CPU

| Condition | Actual FPS before to after | CPU ms/frame before to after | Verdict |
| --- | --- | --- | --- |
| Mage OFF 1x | 55.14~55.80 → 56.56~58.43 | 9.52~9.90 → 7.90~8.72 | CPU improvement withheld for different actual pacing |
| Mage ON 4x | 44.77~47.78 → 54.32~55.44 | 15.52~15.95 → 10.45~11.19 | CPU improvement withheld for different actual pacing |


## Next development proposal for a game centered on recorded combat

Keep event production/preservation, incremental live aggregates, visible text formatting and durable writes separate. Reduce repeated processing rather than omitting events or delaying explanations.

| Priority | Work proposed, not implemented | Evidence and acceptance |
| --- | --- | --- |
| Stage 2 first | Explicit DamageEvent copies, incremental RecordGraphCasts, remove full-journal JSON copies in RiftResult.SyncDisposals, format needed log rows, reuse text measurements and existing ID lookup owners | Save spikes remain despite whitespace removal. Repeat/result/transaction paths need separate cost attribution. Require complete state/event equivalence, live 0.15-second KO/EN presentation and three matched actual-player samples. |
| Stage 3 next | Measured per-tick perception/LOS/planner reuse, aura collection, derived live-enemy list, minimap window culling | Ranger/ON/4× remains near 58.2 FPS with p95 27–28 ms. Define movement/death/visibility/obstacle/gate invalidation and retain RNG/action/event/reward equality. The decision marker includes more than the planner. |
| Stage 5 afterward | Separate rig/MPB/health-bar and first-use shader/glyph/pool costs; distinct OptimizeSpeed/LTO A/B | World/UI costs remain, but one slow sample does not prove a GPU cause. Require release and physical-device measurements at matching quality/DPR. |
| Stage 4 approval track | Lossless event chunks for the latest 100 battles, checkpoints/page reads, storage-blocked pause at a safe tick boundary, server V2 | Current EventLimit/V1 preservation is not completion of the 100-battle requirement. Approve 4A and D1/D2/D5/D7 before implementation, then test capacity failure, ACK/deduplication, rollback, restart and losslessness. |

D3 catch-up policy and D6 slimming were not applied. D4 is resolved as no available mobile hardware. Start further stages only after owner direction.

## Evidence and artifact disposition

Compact summaries, simulation equivalence and before/after SHA-256 manifests live in [WebGLOptimization20261006Evidence](WebGLOptimization20261006Evidence/web-comparison.json). Raw frames, full dumps, binaries, failed attempts, recipes, XML and logs are under the managed worktree's `Builds/Optimization20261006`; lifecycle review is 2026-10-13. Dedicated scripts and the literal in-app capture source preserve reproduction commands without launching external Chrome.

Current comparison players, raw frames, full dumps, test XML/logs and failed-run evidence are retained through review on 2026-10-13. Both comparison servers and the final validation server (57710), in-app task tabs and native players are stopped; the viewport override is reset and no task runtime processes remain. Keep the managed worktree for review and reproduction. No permanent deletion occurred.

The exact pending deletion-approval batch is listed below. MiB denotes logical file size, not exclusive APFS usage or reclaimed space. Protect sibling logs/hashes/dumps, original source/assets, user changes, current comparison players, Library/AI tool state and shared caches. Some superseded binaries lack exact historical source receipts, so identical reconstruction after deletion is not guaranteed.

| Exact path | MiB |
| --- | ---: |
| `/Users/t8g-2410-pn-005/.codex/worktrees/webgl-optimization-stage0-1/HELLSCRIPT/Builds/Optimization20261006/validation/blocked-admission/Web` | 301.94 |
| `/Users/t8g-2410-pn-005/.codex/worktrees/webgl-optimization-stage0-1/HELLSCRIPT/Builds/Optimization20261006/validation/blocked-admission/HELLSCRIPT.app` | 629.95 |
| `/Users/t8g-2410-pn-005/.codex/worktrees/webgl-optimization-stage0-1/HELLSCRIPT/Builds/Optimization20261006/validation/blocked-rune-intro/Web` | 305.19 |
| `/Users/t8g-2410-pn-005/.codex/worktrees/webgl-optimization-stage0-1/HELLSCRIPT/Builds/Optimization20261006/validation/blocked-rune-intro/HELLSCRIPT.app` | 635.30 |
| `/Users/t8g-2410-pn-005/.codex/worktrees/webgl-optimization-stage0-1/HELLSCRIPT/Builds/Optimization20261006/validation/pre-main-adoption-before/Web` | 301.95 |
| `/Users/t8g-2410-pn-005/.codex/worktrees/webgl-optimization-stage0-1/HELLSCRIPT/Builds/Optimization20261006/validation/pre-main-adoption-before/HELLSCRIPT.app` | 629.95 |
| `/Users/t8g-2410-pn-005/.codex/worktrees/webgl-optimization-stage0-1/HELLSCRIPT/Builds/Optimization20261006/validation/pre-main-adoption-before/HELLSCRIPT_BackUpThisFolder_ButDontShipItWithYourGame` | 0.11 |
| `/Users/t8g-2410-pn-005/.codex/worktrees/webgl-optimization-stage0-1/HELLSCRIPT/Data` | 0.47 |

The `artifact-disposition.json` receipt preserves free space immediately before/after disposition on the same volume, timestamp and byte values. No paths were removed and no reclaimed space is claimed. Cleanup is pending explicit permanent-deletion approval; no background cleaner was installed.

The original investigation and development-plan files are updated with current execution status. Stop after this stages 0–1 report; do not automatically begin later work or publication.

Final source/build [integrity](WebGLOptimization20261006Evidence/final-source-integrity.json), translation [repair attempts](WebGLOptimization20261006Evidence/translation-repair-receipt.json), repaired native [smoke results](WebGLOptimization20261006Evidence/smoke-repaired-results.json) and separate WebGL [functional proof](WebGLOptimization20261006Evidence/final-web-functional.json) are retained.

After recording actual checks and precise pacing observations in the report, only wiki generation/link validation was refreshed. The Python/Node wiki tests, full game suite and successful smokes were reused. Wiki tools and UI behavior are unchanged.
