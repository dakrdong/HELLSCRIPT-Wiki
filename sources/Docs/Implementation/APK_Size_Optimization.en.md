# APK size optimization

Updated: 2026-09-27

The Android development APK built on 2026-09-26 was 2.1 GB (2,096,429,914 bytes). The game ships almost no sound or model data; import settings and unused content made it large. This work removes those causes without changing game behaviour or screens.

## Results

Both sides were built from the same commit (`a1bc4825`). Sizes are whole APK files in bytes, with MB = 10^6 bytes; see the [APK size table](APKSizeOptimizationEvidence/apk-sizes.json).

| Build | Before | After |
| --- | --- | --- |
| Development (`Development`, LZ4) | 2,097,620,190 B (2.1 GB) | 88,376,002 B (88 MB) |
| Release (LZ4HC) | — | 60,072,342 B (60 MB) |

The 2026-09-27 integrated release APK, including sound, operational rewards and first-play improvements, is 70,585,750 bytes (70.6 MB). The table above remains a comparison of the same feature scope; the [integrated build evidence](MainIntegration20260927Evidence/android.json) describes the additional-content build separately.

| APK entry (compressed) | Before, development | After, development | After, release |
| --- | --- | --- | --- |
| Asset bundle `data.unity3d` | 2,028.7 MB | 34.3 MB | 31.9 MB |
| `libil2cpp.so` | 34.0 MB | 20.9 MB | 11.2 MB |
| `libunity.so` | 21.4 MB | 21.0 MB | 9.7 MB |
| `global-metadata.dat` | 8.2 MB | 6.8 MB | 3.0 MB |
| `classes.dex` | 3.0 MB | 3.0 MB | 2.4 MB |

## Causes

- The Unity build report listed 6.0 GB of textures. Every image in a `Resources` folder ships, whether or not code loads it.
- No code loaded the 123 aspect emblems (`ClassAspectIcons`). Their tool-written minimal metas were a legacy format, so each imported at about 32 MB, 3.9 GB in total.
- The 252 legendary and skill icons (141+111) shipped their 1254 px masters uncompressed at up to 2048 px, and the 129 set icons shipped their 1024 px masters uncompressed at 1024 px. Nothing owned those folders' import settings; the art tools wrote `Uncompressed` metas. The gem, reward-box, attendance, blacksmith and HUD folders had importers that forced `Uncompressed`; their docs said compression was off "for initial review, compare on the target device later".
- `com.unity.ai.inference` (Sentis) was a direct dependency the game never used. Its `Resources` shaders added 28.5 MB, and the App UI package it pulled in ran initialization code at player start.
- In development builds, `com.unity.pipeline` writes a link.xml that preserves 95 assemblies whole, so development APKs carry more code than release builds. The package stays because it is the Editor connection fallback; a release build path was added instead.

## Changes

| Item | Change | Game impact |
| --- | --- | --- |
| Aspect emblems | Deleted the 123 copies and metas in `Resources/Art/ClassAspectIcons`. The masters stay byte-identical in `Docs/Art/ClassAspectIcons/native/`; `prepare_assets.py` and the [manifest](../Art/ClassAspectIcons/manifest.json) point there | None (nothing loads them) |
| Android texture budget | Added `Assets/HELLSCRIPT/Editor/AndroidTextureBudget.cs`, which sets an Android-only maximum size and ASTC format per folder. The feature importers keep owning the Default settings, so on a Standalone target the Editor, EditMode tests and macOS smokes see the same textures as before. An Editor switched to Android shows the budgeted versions | Android images are sized to their display size |
| Hunt Edict skins | Moved the skin atlas, controls and `resource-manifest.json`, which are not loaded at runtime, to `Assets/HELLSCRIPT/Art/HuntEdict/` with their GUIDs. The glyphs the game uses stay in `Resources` | None |
| Rune glyph atlas | `RuneV13ArtImporter` turns off NPOT scaling, so the 1536×2016 atlas is no longer stretched to 2048×2048. Lookups use normalized UVs, so positions are unchanged | None |
| Packages | Removed `com.unity.ai.inference` (with App UI), `com.unity.timeline`, `com.unity.ai.navigation`, `com.unity.collab-proxy` and 12 unreferenced engine modules (cloth, vehicles, wind, umbra, vectorgraphics, tetgen, timelinefoundation, adaptiveperformance, terrainphysics, unitywebrequesttexture/assetbundle/www) | None |
| Modules kept | ParticleSystem (MCP tools), Video, Terrain and audio requests (AI Assistant), Director and Accessibility (URP Editor), XR and Analytics (test framework and others) | — |
| Template leftovers | Deleted `Assets/Scenes/SampleScene.unity`, `Assets/TutorialInfo` and `Assets/Readme.asset`; cleaned the build scene list and the App UI config reference | None |
| Android player settings | Managed stripping level Low; IL2CPP code generation "Faster (smaller) builds" | Generic code may run slightly slower (not measured on a device) |
| Build entry point | Added `ProjectBuilder.BuildAndroid`: release (LZ4HC) by default, development (LZ4) with `-hellscriptDevelopment` for smokes | — |

The Android budget per folder covers the largest on-screen size at the 150% interface scale, except legendary icons: the 569 px gamble reveal magnifies the 512 px texture slightly.

| Folder | Android maximum | Format | Largest display size |
| --- | --- | --- | --- |
| Legendary, skill and set icons | 512 | ASTC 6×6 | 569 px (legendary gamble reveal), 384 px, 396 px |
| Gems and elixirs (`Jeweler`) | 256 | ASTC 6×6 | 185 px |
| Attendance rewards | 256 | ASTC 4×4 | 224 px |
| Reward boxes | 256 | ASTC 4×4 | 242 px |
| Blacksmith | 512 (source size) | ASTC 4×4 | 114 px |
| HUD potions and Abyssal Coin | source size | ASTC 6×6 | Not reduced: `PotionArt` crops them with 1254 px rects and a smoke checks the coin width |
| HUD portrait and menu emblems | 512 | ASTC 6×6 | 301 px |

Small HUD parts such as frames, masks and status icons stay uncompressed to protect 9-slice borders and mask edges; they are small.

## Verification

- **Android builds**: the release and development APKs built without errors or compile errors.
- **EditMode**: before and after, 47 of 3,983 tests fail, and the failure lists are identical. All 47 failed before this change ([EditMode comparison](APKSizeOptimizationEvidence/editmode-comparison.json)).
- **Android image comparison**: each Android-imported texture was drawn at its display size the way the game draws it, before and after. The Apple M3 Pro GPU samples ASTC natively, so these are the real compressed results. Mean brightness moved by less than 0.5/255 in every folder. Images that were only compressed mostly score 38–56 dB PSNR and are hard to tell apart. The exception is the 5 HUD menu icons (kept at 512 px, shown at 264 px): 35.7 dB average, 26.7 dB minimum. Downscaled icons score 26–33 dB: the old path drew the 1254 px master without mipmaps and aliased, and the downscaled texture loses that noise and looks slightly softer. In the gamble reveal, where a legendary icon grows to 569 px, the 512 px texture is magnified slightly ([numbers](APKSizeOptimizationEvidence/texture-comparison.json)).
  - [Legendary icons before/after](APKSizeOptimizationEvidence/legendary-before-after.png)
  - [Warrior skill icons before/after](APKSizeOptimizationEvidence/skill-warrior-before-after.png)
  - [Attendance rewards before/after](APKSizeOptimizationEvidence/attendance-before-after.png)
  - [2× zoom comparison](APKSizeOptimizationEvidence/zoom-2x-before-after.png)
  - [Rune glyph atlas before/after (UV space)](APKSizeOptimizationEvidence/glyph-atlas-before-after.png)
- **Art validators**: `validate_assets.py` for `ClassSkillIcons` and `ClassSetIcons` now skips the Android block; both pass.
- **macOS runtime smokes**: the same 102 smokes ran on macOS development builds before (`a1bc4825`) and after the change. Both builds pass 6, fail 94 and skip 2, with the same outcome and the same failure line for every smoke. The passing smokes are character selection, class skills, equipment art, presets, title and tutorial. The 94 failures predate this change: the smoke fixtures were never updated after the tutorial requirement, live-ops admission wait and content-unlock gates were added. Google login was skipped because it needs a real account sign-in, and Hunt Edict because it runs the same check as the skill-tree smoke ([smoke comparison](APKSizeOptimizationEvidence/smoke-comparison.json)).

## Not verified

- The APKs were not installed or run on an Android device or emulator; none was connected. Image quality was checked only through the macOS GPU comparison.
- The runtime cost of IL2CPP "Faster (smaller) builds" was not measured.
- Release builds turn off everything gated on `Debug.isDebugBuild`: smokes, the QA session and developer tools. Use development builds for smokes and QA.

## Remaining options

- Raising legendary icons to 1024 would keep the gamble reveal at or above source resolution, for about 50 MB more APK.
- 76 of the 148 reward-box icons form 13 byte-identical groups (63 redundant copies). Sharing one file per group needs the `icon==id` rule, the generator and the server schema to change together, and saves about 4 MB after compression, so it was left alone.
- R8 minification, turning off the Unity splash logo and excluding smoke files from release builds each save a few MB at most and need a device check; they were not done.
- The macOS development build keeps its Default settings and is 2.4 GB. The same budget could be applied to Standalone if needed.
- `applicationIdentifier` is still the template value (`com.UnityTechnologies.com.unity.template.urpblank`). It must change before a store release, but changing it moves the local save location of existing installs, so it was not touched here.

## Rebuilding

When the Unity Editor holds the project, build on an APFS clone. Do not commit the `ProjectSettings.asset`, `UnityConnectSettings.asset` and URP asset changes that batch mode makes.

```bash
Unity -batchmode -nographics -buildTarget Android -projectPath <clone> -executeMethod Hellscript.Editor.ProjectBuilder.BuildAndroid -hellscriptBuildOutput Builds/Android/HELLSCRIPT.apk -quit
```

For a smoke-capable development APK, add `-hellscriptDevelopment`.
