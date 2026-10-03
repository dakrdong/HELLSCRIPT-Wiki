# Measured CPU costs and conservative optimization

Validated: 2026-10-02 · [한국어](CPU_Optimization_20261002.md)

Starting from current baseline `94e11019`, this change skips world rendering behind covered menu screens and duplicate town UI refreshes. The original checkout and user's running Unity were preserved. Implementation and validation use one isolated checkout on local branch `codex/cpu-ponytail-lite`. Development instrumentation is `3fc1033a`; production logic is `91aceb5f`. No remote push, main merge or public deployment was performed, as requested.

## Web CPU and frame pacing

The game already sets `Application.targetFrameRate=60` and runs in the background. Nevertheless, actual Chrome development-player FPS ranged around 60–120 with identical configured60/VSync0. Title baseline repeats ran at 102.2–120.2FPS; after repeats ran around60FPS. Consequently the Renderer CPU percentage drop33.21→16.36% cannot be credited as a logic improvement.

The installed Unity6000.6.0f1 generated JavaScript and runtime state were inspected. One diagnostic window observed `screenAnimationFrameRate`60→120 and Emscripten rAF `timingMode=1`, `timingValue`1→2, averaging73.35 actual FPS. The engine's refresh-rate estimate and frame-skipping ratio changed during measurement. A matching configured cap plus a short warm-up did not control actual web frames. No OS display settings or user browser were changed. [Recorded diagnosis](CPUOptimization20261002Evidence/web-pacing-diagnosis.json)

The final controlled comparison completed **24 captures**, three before/after repeats for each of four states. Both use Chrome headless with actual Apple M3 Pro ANGLE Metal GPU, GPU compositing/WebGL enabled and software rasterization disabled. Virtual screen800×600, actual rendered viewport/canvas1280×720/DPR1. The selected private page remained visible/focused at both boundaries and reported no visibility/focus changes. Engine timing stayed mode0/16.6667ms on both builds. Actual FPS ranges remain disclosed; this is a controlled development-backend result, not a guarantee for the foreground shipping browser.

| State | Actual FPS ranges before→after | Renderer ms/frame | Renderer CPU % | GPU process CPU % |
| --- | --- | ---: | ---: | ---: |
| Menu | 59.3~59.6 → 58.6~59.7 | 3.062 → 2.425 | 18.24 → 14.22 | 10.66 → 8.32 |
| Town idle | 59.3~59.5 → 58.0~59.1 | 4.194 → 3.950 | 24.85 → 22.91 | 13.99 → 13.30 |
| Inventory | 58.7~59.3 → 58.9~59.2 | 3.874 → 3.823 | 22.94 → 22.51 | 12.91 → 12.93 |
| Combat | 57.5~58.1 → 57.7~58.1 | 6.707 → 6.642 | 38.92 → 38.56 | 13.94 → 13.84 |

| State | GC KB/frame | Collections/20s | Mean interval ms | p95 interval ms |
| --- | ---: | ---: | ---: | ---: |
| Menu | 11.4 → 11.5 | 7 → 7 | 16.84 → 17.05 | 18.00 → 18.00 |
| Town idle | 41.7 → 38.4 | 21 → 20 | 16.85 → 16.97 | 18.00 → 19.00 |
| Inventory | 36.2 → 36.6 | 12 → 12 | 16.89 → 16.98 | 18.00 → 18.00 |
| Combat | 122.8 → 122.6 | 32 → 31 | 17.23 → 17.22 | 21.00 → 21.00 |

| State | Managed MiB | Unity allocated MiB | Renderer RSS range MiB | JS heap MiB |
| --- | ---: | ---: | --- | ---: |
| Menu | 10.5 → 10.5 | 64.7 → 64.5 | 1282.3~1301.1 → 942.0~1299.2 | 8.8 → 8.5 |
| Town idle | 12.5 → 13.2 | 89.6 → 89.9 | 469.2~1464.4 → 1057.9~1451.5 | 9.2 → 9.6 |
| Inventory | 17.6 → 17.6 | 91.0 → 91.4 | 1441.9~1469.9 → 1441.6~1458.3 | 8.7 → 9.1 |
| Combat | 29.2 → 29.3 | 88.8 → 89.1 | 1516.4~1604.5 → 1016.8~1591.9 | 9.2 → 9.2 |

Menu Renderer CPU/frame decreased **20.8%**, with non-overlapping repeat ranges3.047–3.110→2.365–2.499ms. Town CPU ranges overlap3.878–4.220→3.769–4.344ms; two of three pairs increased, so its lower median receives no CPU credit. Inventory/combat ranges also overlap. Town allocation decreased approximately7.9% in this environment; memory/frame-time gains are not claimed. CPU percentages caused by reducing120FPS to60FPS are excluded from these logic results.

[Controlled before](CPUOptimization20261002Evidence/web-controlled-before-detailed.json) · [After](CPUOptimization20261002Evidence/web-controlled-after-detailed.json) · [Comparison](CPUOptimization20261002Evidence/web-controlled-comparison.json) · [Immutable binary/HTML hashes](CPUOptimization20261002Evidence/controlled-build-binaries.json)

The old baseline output was replaced at22:14+0900: HTML profiling arguments and JS exports disappeared, and WASM/data hashes changed. No attribution is made without evidence. Native and after-Web hashes still matched. Exact3fc1033a development baseline was rebuilt into a unique path; unchanged after output was frozen with a COW copy. All HTML/loaders/framework/data/WASM hashes matched after24 final captures. Failed missing-report attempts receive no CPU credit; original historical captures remain separate. Final source was restored identically to91aceb5f, already covered by final EditMode. Analytics code/beacons were absent from both local samples.

The additional comparison holds timer mode0 /16.6667ms in generated development benchmark JavaScript only. This forced scheduling is absent from production source. Only logic costs under the same pacing can receive credit; changing the frame policy itself is separate. Actual FPS, visibility/focus changes, estimated rAF rate, scheduling mode/value, screen, viewport and DPR are recorded.

## Implementation

- Title and character selection reuse `World.SuspendPresentation()` to suspend the covered world camera while retaining their animated UI. Town/dungeon creation calls existing `ResumeCamera()` to restore gameplay rendering.
- The duplicated `RefreshPlaza()` in Controller Update and UI Update becomes one UI LateUpdate after movement. Existing immediate action refreshes remain.
- Interaction, Buy and Sell buttons receive their final active state once, removing false→true activation churn. Portal priority, NPC conversations and service actions remain.

Combat ticks, balance, save/reward validation, resolution, quality and production FPS policy are unchanged. Existing TitleAtmosphere/TitleScreen/CharacterSelection and GameUI.Plaza own these displays; no separate presentation system was added.

## Native development-player results

Medians of three20-second repeats per state. CPU100% means one occupied core. GC KB uses1000 bytes.

| State | CPU ms/frame | CPU % | GC KB/frame | Collections/20s |
| --- | ---: | ---: | ---: | ---: |
| Menu | 3.881 → 3.012 | 23.04 → 17.83 | 12.1 → 13.8 | 8 → 8 |
| Town idle | 5.330 → 5.288 | 31.62 → 31.50 | 61.8 → 55.4 | 53 → 47 |
| Inventory | 5.194 → 5.180 | 30.79 → 30.86 | 48.1 → 48.5 | 29 → 30 |
| Combat | 8.245 → 8.135 | 48.46 → 47.61 | 186.0 → 190.8 | 76 → 75 |

Menu CPU/frame decreased **22.4%**. Three-repeat ranges do not overlap:3.662–3.976→2.946–3.177ms. Menu triangle counters dropped approximately31,496→953 while preserving the title shown below. Town allocation decreased **10.4%**, but its CPU change is within noise and receives no CPU credit. Inventory/combat CPU changes also receive no credit. Menu and combat allocation medians increased.

| State | Actual FPS | Mean interval ms | p95 interval ms |
| --- | ---: | ---: | ---: |
| Menu | 59.89 → 59.83 | 16.69 → 16.71 | 16.93 → 16.91 |
| Town idle | 59.52 → 59.52 | 16.80 → 16.80 | 17.76 → 17.82 |
| Inventory | 59.62 → 59.47 | 16.77 → 16.81 | 17.46 → 17.52 |
| Combat | 58.65 → 58.66 | 17.04 → 17.04 | 21.59 → 21.61 |

Frame intervals are differences between consecutive actual frame timestamps. There is no consistent improvement in p95 or memory.

| State | Managed MiB | Unity allocated MiB | Player RSS MiB |
| --- | ---: | ---: | ---: |
| Menu | 14.7 → 14.6 | 126.5 → 126.0 | 467.4 → 466.8 |
| Town idle | 15.5 → 15.5 | 153.5 → 153.6 | 534.7 → 536.5 |
| Inventory | 19.6 → 19.5 | 155.1 → 155.4 | 550.2 → 549.4 |
| Combat | 30.8 → 31.2 | 154.2 → 154.4 | 616.0 → 620.0 |

[Before measurements](CPUOptimization20261002Evidence/native-before-detailed.json) · [After](CPUOptimization20261002Evidence/native-after-detailed.json) · [Comparison](CPUOptimization20261002Evidence/native-comparison.json)

| Title | Before | After |
| --- | --- | --- |
| 1280×720 | ![Title before](CPUOptimization20261002Evidence/menu-before.png) | ![Title after](CPUOptimization20261002Evidence/menu-after.png) |

## Remaining costs

Web combat baseline marker mean medians were BehaviourUpdate1.293ms/frame, World.Present0.565ms, Combat.Tick0.461ms and UGUI.Rendering.UpdateBatches0.574ms. These regions overlap and cannot be summed into total CPU. WaitForTargetFPS3.794ms and Main Thread wall time include waiting. Town still processes approximately339,423 triangles with Mobile quality. Actual town/combat rendering has different requirements from a world hidden behind a covered menu.

Moving town refresh to LateUpdate changes BehaviourUpdate/UI.Update marker coverage. Marker changes alone are not CPU-improvement evidence; conclusions use whole Renderer/player CPU.

Actual-player CPU is separate from Editor CPU. Original Editor PID55085 remained running; per-window CPU ranges are in the JSON. Native baseline ranges were approximately140–189%, after116–185%. Editor activity is not game-optimization evidence. Dedicated Web GPU/browser CPU remains separate from Renderer CPU.

## Method and Ponytail

macOS26.6.2, Mac15,7 / Apple M3 Pro,12CPU cores,36GiB RAM, Unity6000.6.0f1. Native development player uses Mono/PC quality; WebGL development player uses IL2CPP/Mobile quality. These platforms are not compared directly. Chrome154.0.8037.93, actual Metal GPU, canvas1280×720/DPR1. Standard captures use3-second warm-up,20-second windows and three repeats per state.

An isolated MageLv30 save uses eight rare+5 equipped items, fixed UTC1790899200, gear seed9132026, encounter seed93171, stage30, autoRepeat/edict disabled. Fixture SHA256 is `81dd0abc25c1d46416523dae026233e3beba1aedd2e977d95dc46dc5983dfad5`; workload SHA256 is `5381209ced8858655d03efff3a77444db0d3fed9097947d8ec258ede007239f5`; combat map fingerprint is `cb352198`. Normal admission and live-operations paths are retained. Profiling requires a development build, explicit opt-in and an empty isolated save directory.

Native CPU is the actual app's `Process.TotalProcessorTime` window delta. Web CPU uses CDP `Performance.ProcessTime` between BEGIN/END `console.timeStamp` events and verifies agreement with that Renderer `SystemInfo.getProcessInfo` CPU delta within25ms. GC/frame is `GC Allocated In Frame`; collections are `GC.CollectionCount(0)`. Managed heap, Unity allocated, process RSS and JS heap are distinct. Web Draw Calls/Batches0 and GC.Collect marker0 are not evidence of zero work.

Installed [upstream Ponytail](https://github.com/DietrichGebert/ponytail) **4.10.0 SKILL.md** was manually applied at lite intensity. Exact SHA256: `1316a2f3f95741d2300b116fe0c2d81ce4a9568656ed0a62643f54aaf09957f2`. Review preserved requirements/safety checks and reused existing helpers while removing repetition in small changes. Existing camera/refresh helpers were sufficient without a new cache layer or UI rewrite. No installer, Node hooks or global instructions were executed/changed. This is not a claimed official CLI audit pass, and does not replace performance or correctness validation.

## Validation and limits

Full EditMode ran after the final game-code change. Baseline and final both report **5,041 total / 4,913 passed / 128 failed / 0 skipped**, with identical failing names: **0 new failures, 0 resolved existing failures**. No checks were weakened. Shared UI contract validation and its 11 Python checks also passed. [Full comparison](CPUOptimization20261002Evidence/editmode-comparison.json) · [Existing failure details](CPUOptimization20261002Evidence/editmode-baseline-failures.json) · [Build outcomes](CPUOptimization20261002Evidence/build-outcomes.json)

Native/WebGL development builds succeeded before and after. Four actual native scenarios passed: idle runtime and independent restart; town portal runtime and independent restart. Portal validation covers KO/EN,440×956/956×440/PC16:9·16:10·21:9, actual raycast input, save failure/retry, identical run/time/HP/resource/position/RNG/cooldowns/enemies, no new admission cost, original speed and advancing resumed simulation. Idle checks restore camera/audio/frame policies and preserve gear/language/save. Only default text size is used.

Three existing smoke failures reproduce identically in the baseline: general `Town build button missing`; characters `characters-back` below44px; NPC `Wrong service turk-garbi page=plaza`. New covered-camera and first NPC activation-stability checks passed before those failure points; the complete smoke suite is not reported as passing. Portal runner setup failures from a missing evidence flag/checkpoint copy were corrected and rerun, retaining initial failures. [Classified runtime results](CPUOptimization20261002Evidence/runtime-comparison.json) · [Portal](CPUOptimization20261002Evidence/town-portal-runtime.txt) · [Restart](CPUOptimization20261002Evidence/town-portal-reload.txt) · [Idle](CPUOptimization20261002Evidence/idle-runtime.txt)

Older30935f37/attendance-obscured captures,8191-byte truncated Unity JSON, reused quit-page failures and scheduler diagnostics concurrent with final EditMode are excluded. Full JSON is retrieved from the development player's existing virtual-FS report through identical benchmark JS. No physical mobile device was connected; mobile CPU/battery, release-player CPU and Safari/Firefox are not validated. A UnityCLI startup wait was bypassed with the matching installed Editor; causality between Bee errors and keychain approval remains unconfirmed. [Conditions/exclusions](CPUOptimization20261002Evidence/method.json)

## Reproduction

Instrumented baseline source is `3fc1033a`, optimized source `91aceb5f`. Use the same profiling on both development builds, without concurrent builds/tests during CPU captures. `Unity` means installed6000.6.0f1 Editor.

```bash
Unity -batchmode -nographics -quit -projectPath "$CHECKOUT" -buildTarget StandaloneOSX -executeMethod Hellscript.Editor.ProjectBuilder.BuildMac -hellscriptBuildOutput "$OUTPUT/HELLSCRIPT.app" -logFile "$OUTPUT/build.log"
python3 tools/profile_native_cpu.py "$OUTPUT/HELLSCRIPT.app/Contents/MacOS/HELLSCRIPT" "$CAPTURES" --editor-pid "$USER_EDITOR_PID"
Unity -batchmode -nographics -quit -projectPath "$CHECKOUT" -buildTarget WebGL -executeMethod Hellscript.Editor.WebPlayerBuild.BuildGitHubPages -hellscriptDevelopment -hellscriptBuildOutput "$WEB_OUTPUT" -logFile "$WEB_OUTPUT/build.log"
python3 tools/prepare_web_cpu_profile.py "$WEB_BEFORE" before
python3 tools/prepare_web_cpu_profile.py "$WEB_AFTER" after
PROFILE_HEADLESS=1 PROFILE_FIXED_FPS=60 PROFILE_EDITOR_PID="$USER_EDITOR_PID" python3 tools/profile_web_cpu_batch.py "$WEB_BEFORE" before "$WEB_CAPTURES" --after-build "$WEB_AFTER"
python3 tools/summarize_cpu_profiles.py "$WEB_CAPTURES/before" "$RESULTS/before.json"
python3 tools/summarize_cpu_profiles.py "$WEB_CAPTURES/after" "$RESULTS/after.json"
```

Apply benchmark JS once to fresh development output, never distribution output. Each sample owns a separate Chromium profile, and the runner only closes its own child. Repeat2 reverses before/after order; no concurrent Unity/browser runs are used. Complete original JSON/XML/logs/images remain in workspace `evidence/`; the repository contains compact linked evidence.
