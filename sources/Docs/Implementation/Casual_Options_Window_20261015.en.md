# Casual shared settings window

2026-10-15 · [한국어](Casual_Options_Window_20261015.md)

`codex/casual-options` starts from latest `origin/main` `af716fba66cfeb2cb210c5a549779eb68bc5efe2`. All 24 references and the live Claude canvas were inspected in the verified Codex in-app browser. The dirty primary checkout is untouched. Do not merge this PR before user approval.

The shared Screen, Audio, Language, Character and Account bodies, Game guide and Combat state now use cream chrome, white cards, yellow selected tabs, marker sliders and fresh PNG art. Existing preference, save, retry, identity and character-change owners remain authoritative. Hunt Edict, gear recommendation and shop options, and their gear entry buttons are outside this scope.

| Decision | Result |
|---|---|
| 1 · Composition and text | Keep existing font sizes and bundled Jua/Noto Sans KR. Tabs use one row: icon+label on PC, stacked in portrait, labels only in short landscape. Close and tabs retain at least 44 screen pixels. Card heights and scrolling differ from the reference's smaller text measurements. |
| 2 · Sliders | Retain Slider/SettingsStepSlider, 5% distance steps, wheel, −/+ and audio owners. The mute checkbox and bar fade follow the actual preference. |
| 3 · Plate | Float 8–12px inside the existing safe-area right-half/full-portrait ownership. Preserve keyboard, reading anchor and handle refresh. Separate the portrait header gem/title and update ratio-chip height after rotation. |
| 4 · Left world | Keep the actual town, training or rift scene and camera; no mockup background or added gradient. |
| 5 · Art | The user's follow-up rejected SVG quality and explicitly requested redrawn icons/map thumbnails, overriding the SVG-derivative direction. Generate 25 icons and three thumbnails of the same battlefield. A further user revision removes every hero/monster and distinguishes an upper-right minimap, a central translucent map and no map. Retain SVG only as mockup history. |
| 6 · Scrollbar | No added scrollbar. |

`CasualSettingsStyle` and settings layout parts are shared by the bodies. Missing colours are named `UiTheme.Settings*` tokens referenced through `UiSkin.Casual`; there is no global theme switch. Reuse existing hero seals, −/+ art and panel nine-slices. Build the window when opened and update existing values/choice states through the current refresh owners.

UID, pending-count and combat-state pills place their text above the background. Long UIDs wrap before the copy button. The current hero remains selected during a rift; other heroes show grey locked cards. The tutorial uses four tabs and places Return to title at the top of Account.

Keep [native originals](../Design/Options/Casual/assets/generated/), [generation prompts/provenance](../Design/Options/Casual/assets/image-generation.json) and [derivative dimensions/alpha/hashes](../Design/Options/Casual/assets/sprite-manifest.json). See the [art contact sheet](CasualOptionsEvidence/art-contact.jpg) and [three revised maps](CasualOptionsEvidence/map-options-v2.jpg). The built-in generator exposes neither model selection nor returned model version: **actual model unknown**. No verified `gpt-image-2` claim.

Validate native PNG alpha and fully transparent pixels before resizing icons to 128×128; map thumbnails are 256×164. No background removal, chroma key or SVG conversion. The 28 runtime PNGs total 1,029,406 bytes; raw RGBA equivalent is 2,142,208 bytes. Their estimated total with the configured 4×4 block compression is 535,552 bytes, not measured GPU memory. No performance improvement is claimed.

| Validation | Result and boundary |
|---|---|
| Actual game UI | Editor Game View: 440×956, 956×440, 1280×720, 1600×900, 2100×900 × KO/EN × seven bodies plus rift, save-failure and tutorial states. [112 captures and 24 reference pairs](CasualOptionsEvidence/README.en.md). Phone margins are simulated on Mac; not physical-device proof. |
| Input/state | Actual uGUI raycasts for tabs, mute, language, close and backdrop; 0.45/1 bar alpha; actual file-write failure and the three existing retry handlers; rejected rift character changes; tutorial four-tab layout. Final English choice reports zero missing translations. |
| Related EditMode | Run 106 cases across CasualSettingsTests, SettingsRevisionTests, CasualUiSkinTests, LocalizationTests and StoreViewBindingTests. Repair three localization failures and rerun only those three, all passed. The initial 27-case focused UI run also passed. Reuse unaffected successful coverage. [Receipt](CasualOptionsEvidence/validation-summary.json). |
| Static contracts | Final `check_ui_contract.py --verify`: 15 regressions passed. `check_ui_refresh.py`: 14 bindings/195 UI sources passed. |
| Compilation | Unity 6000.6.0f1 Editor compilation passed. Repair two initial LayoutGroup API arguments. No scene/prefab YAML edits. |
| Full EditMode/native build/smokes | **Not run on this branch.** The UI integration owner runs these once on the final integrated candidate after approved inputs freeze. Coordinate to avoid duplicate heavy jobs on this Mac. |
| Unverified | Physical mobile, WebGL, actual Google authentication/HTTP, CPU/GC and before/after binary performance. The displayed UID is a synthetic visual fixture, not authentication proof. |

The integrator can combine existing `-hellscriptScreenSettingsSmoke` with new `-hellscriptCasualOptionsSmoke` to run 110 native option captures. Require isolated `-hellscriptSavePath <dir>` and `-hellscriptScreenshots <dir>`. Reuse the integrator's same development binary; no additional build is needed.

Task-owned output paths, measured sizes, recipes, dependencies and retention are in `Builds/Validation/CasualOptions/artifact-lifecycle.json`. Retain source art and referenced evidence in the PR. Retain this worktree's import Library, logs and rejected early captures through review; reassess on 2026-10-22 or merge, whichever comes first. No native test binary was created, no permanent deletion was performed, and other tasks' outputs were untouched. Verify the owned Editor path and clean state, close it gracefully, then hand Unity usage back to the integrator.

No wiki generation/check/publication on this work branch. The approved merger handles affected wiki inputs and final integration gates. Processing/wait/retry time was not separately instrumented; the lifecycle manifest records elapsed time and actual check counts.
