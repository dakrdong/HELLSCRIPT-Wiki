# Build resources and startup memory optimization

Verified: 2026-09-28 · [한국어](Build_Optimization_20260928.md)

This change removes duplicate images, oversized texture imports, release-only QA overhead and eager sound loading while preserving current game content. Baseline source: `672e4a0ceb87359fb9489bf8a0c96ff27f4a23fd`. Before and after use Unity 6000.6.0f1 and matching platform/release options. The earlier [APK optimization report](APK_Size_Optimization.en.md) measured a different content snapshot.

## Measured results

| Measurement | Before | After | Reduction |
| --- | ---: | ---: | ---: |
| macOS arm64 release app | 801,359,644 B (764.2 MiB) | 232,688,023 B (221.9 MiB) | 71.0% |
| Android arm64 release APK | 176,155,058 B (168.0 MiB) | 138,223,218 B (131.8 MiB) | 21.5% |
| macOS release Runtime DLL | 4,482,048 B | 2,831,872 B | 36.8% |
| Unity allocated memory at title | 145,622,355 B | 131,711,258 B | 9.6% |
| Sound effects loaded at title | 454 / 11,321,668 B | 0 / 0 B | Loaded on demand |

App size sums logical file sizes inside `.app`; APK size covers the entire file. A MiB is 1,048,576 bytes. These are not filesystem allocation or zipped-app sizes. Memory is one matching development-player sample on Apple M3 Pro at 956×440, three seconds after startup. It is not process RSS or a prediction for every battle.

[Build sizes and largest asset groups](BuildOptimization20260928Evidence/build-sizes.json) · [Startup memory](BuildOptimization20260928Evidence/startup-memory.json)

## Changes

- Consolidated 148 reward PNGs into 85 pixel-unique images and removed 63 duplicate PNG/meta pairs. All 148 definitions, amounts, grant rules, save IDs and SVG originals remain. Only display `icon` references are shared. Catalog validation rejects missing or cyclic references; removed images had no serialized GUID references.
- Extended `AndroidTextureBudget` into `ResourceTextureBudget`. Standalone imports use high-quality BC7 with display-appropriate caps. Android ASTC budgets now cover NPCs, equipment variants, potions and rune atlases. Source PNGs were not recompressed. Thin HUD borders and masks retain their existing settings.
- Scaled potion crop coordinates from the 1254px master to actual imported dimensions. The 512px imports preserve crop proportions and bounds. Pixel-addressed title, character and rune atlases retain their authored resolution.
- Corrected 160 world PNGs that old metadata imported as Cubemaps. `WorldArtImporter` explicitly requests Texture2D so materials can resolve their albedo maps. Meshes, pivots, readability rules and source pixels remain unchanged.
- Replaced eager loading of 454 effects with a path index and on-demand cache. Mute, power-saving, cooldown and voice-budget rejection happen before loading. Shared cues reuse the original clip. All 283 cues, 454 effects and four streaming music tracks remain.
- Guarded 111 runtime validation files with `UNITY_EDITOR || DEVELOPMENT_BUILD`. Android release initialization hooks dropped from 106 to two actual game hooks. Editor/development validation remains available. Small MonoScript metadata entries in the build report are distinct from compiled test code.
- Added `ProjectBuilder.BuildMacRelease` with LZ4HC and Low managed stripping, restoring the previous stripping setting afterward. Existing `BuildMac` stays a development build. Builds now emit `.size.json` reports for future regression tracking.

The audit included dynamic Resources strings, catalogs and shared paths. Used sounds were retained. Packages and template assets removed by earlier work are not counted again.

## Validation

- **107/107 focused Unity Edit Mode tests passed**, with zero failed or skipped: image aliases, reward/save rules, lazy audio, potion bounds, world textures and importer idempotence. [Individual results](BuildOptimization20260928Evidence/editmode.json)
- Native development-player checks resolved **283 cues / 454 clips**, all three potion crops, world Texture2D assets and material albedo.
- Matching isolated combat fixtures ran for about 20 seconds each and both completed **400 simulation ticks**. Both copies used the same completed-guide state to avoid a tutorial pause. Frame counts are not evidence of an FPS improvement. [Combat results](BuildOptimization20260928Evidence/combat.json)
- Actual release-player clicks covered guest login, Warrior creation, three kills, inventory, item detail and equipping armor. Defense changed from 68 to 92 and the saved armor was equipped. Only the QA copy's app name, bundle ID and signature changed to avoid other running players; its player Data files hash-match the measured release. [Native acceptance](BuildOptimization20260928Evidence/native-release.json)
- Compared 13 GPU-rendered image samples. The largest mean difference was the storage icon at 2.86/255; downsampling smooths the previous fine aliasing. Source PNGs and transparency remain. [Image metrics](BuildOptimization20260928Evidence/image-comparison.json)
- Art checks passed for 111 skill images and 129 set images. Existing skill-art heuristic warnings remain, with no new validation errors. Shared UI contracts and all nine Python contract tests passed.
- Live-operations server catalog/default-grant tests were also run. Shared display images do not change reward IDs or grant semantics.

| Image | Before | After |
| --- | --- | --- |
| Storage | ![Before storage](BuildOptimization20260928Evidence/baseline-Art_GlobalHUD_menu-storage.png) | ![After storage](BuildOptimization20260928Evidence/optimized-Art_GlobalHUD_menu-storage.png) |
| Potion | ![Before potion](BuildOptimization20260928Evidence/baseline-Art_GlobalHUD_potion-hp.png) | ![After potion](BuildOptimization20260928Evidence/optimized-Art_GlobalHUD_potion-hp.png) |

[Release combat](BuildOptimization20260928Evidence/release-ui.png) · [Item detail](BuildOptimization20260928Evidence/release-detail.png) · [Equipped inventory](BuildOptimization20260928Evidence/release-equipped.png)

## Scope and reproduction

The full regression suite was not rerun. Earlier full-suite results in the APK report are separate from these 107 focused tests. No Unity MCP instance was connected, so verification used the installed batch editor and isolated native players. Android release compilation succeeded; device installation, visual quality, frame performance and battery consumption were not measured. Desktop observations are not mobile performance claims.

```bash
Unity -batchmode -nographics -projectPath <checkout> -executeMethod Hellscript.Editor.ProjectBuilder.BuildMacRelease -hellscriptBuildOutput <output>/HELLSCRIPT.app -quit
Unity -batchmode -nographics -buildTarget Android -projectPath <checkout> -executeMethod Hellscript.Editor.ProjectBuilder.BuildAndroid -hellscriptBuildOutput <output>/HELLSCRIPT.apk -quit
python3 tools/compare_build_size.py <before>.size.json <after>.size.json
```

The comparison tool rejects different platforms or build options. Use `BuildMac` or `BuildAndroid -hellscriptDevelopment` for development acceptance. Keep both outputs beside their reports for direct byte comparisons. Accounts, saves, logs and full build reports remain in local `Artifacts/BuildOptimization/`; published evidence contains measurements and screenshots without account identifiers.
