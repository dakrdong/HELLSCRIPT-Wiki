# HELLSCRIPT Web player

Updated: 2026-09-28 · [한국어](Web_Build.md)

The Web player is a separate deployment of the Unity game. Only executable build artifacts are published to `dakrdong/HELLSCRIPT-Web`; private profiles and the development project are not copied. Game source remains in the existing repository and the public wiki keeps its separate deployment.

Public player: [HELLSCRIPT Web](https://dakrdong.github.io/HELLSCRIPT-Web/). Deployment status is available through the deployment runs linked below.

## Playing and saving

- The Web edition starts as a guest. Native Google sign-in uses operating-system callbacks and is not offered by the Web UI. This release does not provide account linking or cross-device save synchronization.
- Progress and display/language preferences live in this browser's site storage. Unity's `autoSyncPersistentDataPath` preserves the existing file-based save transactions and backups. Clearing browser data deletes the Web save.
- Leaving the tab invokes the existing pause, save and resume flow. Time spent suspended by the browser is not treated as real-time combat. The existing in-game power-saving screen remains available while the game tab is visible.
- This edition uses the game rules bundled with the build. It does not connect to native authentication or the operator server's same-origin APIs. Server security and authentication are not changed.
- Browsers supporting Web Locks allow one game tab at the same address to reduce save conflicts. The loader presents storage and loading failures in Korean and English.

## Building and publishing

Use Unity 6000.6.0f1 with Web Build Support installed.

```bash
Unity -batchmode -nographics -buildTarget WebGL -projectPath <checkout> \
  -executeMethod Hellscript.Editor.WebPlayerBuild.BuildGitHubPages \
  -hellscriptBuildOutput <output>/Web -logFile <output>/web-build.log -quit
python3 tools/package_web_build.py package <output>/Web <deployment-checkout> --revision <source-commit>
```

`WebPlayerBuild` uses a release player, WebAssembly DiskSizeLTO and IL2CPP size optimization, Low managed stripping, WebGL 2, one thread and gzip compression. Decompression fallback supports static hosting without custom encoding headers. `ResourceTextureBudget` owns Web texture overrides while retaining original images, GUIDs and other platform settings. Release Web builds also remove the performance-test package's generated machine-information resources and verify their absence in the completed report.

Browsers cannot read operating-system fonts. `UiFonts` uses a preloaded Nanum Gothic font on Web. The unchanged Google Fonts release and SIL Open Font License 1.1 are stored in `Assets/HELLSCRIPT/ThirdParty/Fonts/`. The font stays outside `Resources` and is preloaded only by the Web build so native player size does not increase. Public deployments include `ThirdPartyNotices.txt`.

`Assets/WebGLTemplates/HELLSCRIPT/` owns the responsive loader and browser lifecycle. Existing owners still control the game canvas, safe area, reading scale and combat. Aspect selection fits the game area without forcing the browser window's resolution.

Copy `tools/package_web_build.py` to `assemble_web.py` in the deployment repository and `tools/web_pages_workflow.yml` to `.github/workflows/pages.yml`. Set Pages to GitHub Actions. Files larger than a Git-friendly size are stored as chunks of at most 48 MiB. The workflow verifies size and SHA-256 and reconstructs the original files before uploading the Pages artifact. Browsers receive ordinary Unity build files.

## Android APK

After validating the Web build, use the existing `ProjectBuilder.BuildAndroid` on the same `main` source. With no development flag, it produces a release player with LZ4HC compression. Keep the existing application ID and signing configuration.

```bash
Unity -batchmode -nographics -buildTarget Android -projectPath <checkout> \
  -executeMethod Hellscript.Editor.ProjectBuilder.BuildAndroid \
  -hellscriptBuildOutput <output>/HELLSCRIPT.apk -logFile <output>/android-build.log -quit
```

Successful packaging, package/signature checks and installation/play on a physical Android device are separate validation claims.

## Verification record

Built from the integrated `main` with Unity 6000.6.0f1 on 2026-09-28. The [validation summary](WebBuildEvidence/validation.json) includes the results and APK hash.

| Check | Result |
| --- | --- |
| Unity Edit Mode | 59 resource, bundled-font, title and localization tests passed |
| Shared UI, packaging, Web loader | 9, 5 and 5 tests passed respectively |
| Web build | Release, zero errors, 163,844,655 download bytes (about 164 MB) |
| Browser interaction | Guest entry, Warrior selection, tutorial combat, loot, equipment and town entry verified |
| Browser persistence | Reloaded after equipping armor; the same equipped item and defense 92 were restored |
| Layout and language | 440×956, 956×440, 1280×720, 1440×900 and 1680×720; Korean, English and 140% reading size checked |
| macOS compatibility | Native startup, 283 sound cues/454 clips, potion crops and world texture checks passed |
| APK | Release, zero errors, 142,832,282 bytes (about 143 MB), ARM64, Android 8.0/API 26 minimum, target API 36 |
| APK signature | Existing Android Debug certificate verified with APK v2; for direct installation, with store signing separate |

No browser runtime errors were observed. Unity URP still reports an unsupported FSR upscaling shader; the listed screens and interactions were checked in the rendered player. The first Web build hit Unity Bee's six-pass graph regeneration limit; an incremental retry succeeded with no source changes.

Public deployment commits and status are available in [deployment runs](https://github.com/dakrdong/HELLSCRIPT-Web/actions) and the public site's `build-info.json`. Browser evidence is from the macOS in-app browser. Installation, gameplay and performance on physical Android/iOS devices were not tested in this task.

![Korean Web title](WebBuildEvidence/web-title.png)

![Web combat and bottom HUD](WebBuildEvidence/web-combat.png)

![Equipped armor and defense 92 restored after reload](WebBuildEvidence/web-save-restored.png)
