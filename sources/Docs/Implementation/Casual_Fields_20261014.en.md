# Casual rift field skins — 2026-10-14

[한국어](Casual_Fields_20261014.md)

Updated on: 2026-10-15

The F4 meadow was validated first, followed by F0–F5. Runtime assets, lighting, fog and ambient particles now use bright StyleD palettes. Seeds, room layouts, obstacle collision/height, AI, combat, rewards and save keys are unchanged. Dedicated boss arenas, training, the legacy ring, town and the monster pipeline are outside this change.

## Runtime evidence

These are real game views from the macOS Unity 6000.6.0f1 Editor. A naturally generated seed opens each field. For readability, the hero is staged at a walkable point beside a live pack; actual combat ticks produce the paused enemy warning. This is visual/flow evidence, not natural progression, balance or device-performance evidence. Unexplored camera background remains black.

| Field | Palette | Entry | Live attack warning | Seed |
| --- | --- | --- | --- | --- |
| F0 graveyard | Pale stone, moss, flowers | [Entry](CasualFieldEvidence20261014/F0-entry.jpg) | [Warning](CasualFieldEvidence20261014/F0-telegraph.jpg) | 19 |
| F1 fortress | Cream stone, warm sunlight | [Entry](CasualFieldEvidence20261014/F1-entry.jpg) | [Warning](CasualFieldEvidence20261014/F1-telegraph.jpg) | 1 |
| F2 desert | Warm sand, sky blue | [Entry](CasualFieldEvidence20261014/F2-entry.jpg) | [Warning](CasualFieldEvidence20261014/F2-telegraph.jpg) | 3 |
| F3 cavern | Purple stone, bright crystals | [Entry](CasualFieldEvidence20261014/F3-entry.jpg) | [Warning](CasualFieldEvidence20261014/F3-telegraph.jpg) | 16 |
| F4 meadow | Grass green, cream walls, pollen | [Entry](CasualFieldEvidence20261014/F4-entry.jpg) | [Warning](CasualFieldEvidence20261014/F4-telegraph.jpg) | 17 |
| F5 plateau | Bright snow, pale blue | [Entry](CasualFieldEvidence20261014/F5-entry.jpg) | [Warning](CasualFieldEvidence20261014/F5-telegraph.jpg) | 2 |

All captures are 1889×1543; the hero and red warnings remain distinguishable. F5's large blue snow blotches have lower contrast. Missing kit obstacle kinds reuse their template family's repainted models. The shared bounds-fitting path still fits the gameplay footprint and height.

## Owners and storage

![Actual attack telegraphs in the bright F4 grassland](CasualFieldEvidence20261014/F4-telegraph.jpg)

`FieldLook.cs` and `WorldFx.Persistent.cs` own the six daytime looks and pollen/leaves/crystal motes/gentle snow. Town and Legacy presets are unchanged. `WorldView.Rift.cs` and `WorldView.Props.cs` connect shared tiles, brighten rectangular tutorial passages and resolve field art before template-family art. Resources keep existing names/GUIDs; `_A` alpha is matte smoothness 26/255 and `_N` is a flat tangent normal. `ResourceTextureBudget.cs` explicitly caps each field at 512px; palette atlases are 64px and constant normals 4px.

The [installer](../../tools/art3d/install_casual_fields.py) uses the procedural kit already preserved on main. It verifies source hashes, normal/albedo output and existing metadata. No external generation API or new package is used. [Measured source sizes and hashes](CasualFieldEvidence20261014/asset-checks.json): 47,237,697 → 4,362,081 bytes, a **40.9 MiB reduction**, including new F3–F5 assets. This is not APK, web or installed-player size. All 168 existing GUIDs remain intact; reinstalling all six fields produces identical bytes. There are 81 models and 198 textures. Dark originals remain recoverable in Git history.

## Tutorial wording audit

Inspected GuideTutorial, CombatSimulation.Guide, Tutorials, TutorialChapters, FirstPlayGuide, GameUI.Tutorial/Guide owners, TutorialCinematic, WorldView.Prologue and Localization/en.txt. Current guide dialogue, toasts and instructions do not assume dark scenery and remain unchanged. Guide illustration text uses the same localization owner; no separate atmosphere illustration was added.

The surrounded prologue uses “here” instead of darkness. Korean sentences themselves are shipped keys, so Loc.T revises source display and its English translation without changing that key. Four puzzle hooks audited initially were retired with the puzzle tutorial by main PR #132. Integration removes their display mappings and regression cases and does not restore retired code, data or translations. One current prologue display correction remains. Its regression case verifies the existing key, both languages and Korean fallback. Graveyard/monster/item names and display dimming retain their identity/function.

## Validation status

The initial F4 focused EditMode run passed 17/17. Subsequent shared-model refinements compiled and appear in the six runtime captures; final tests cover them. Twelve final entry/combat images and black-background/live-windup assertions passed. Installer self-checks, identical reinstallation, GUID preservation and diff checks passed.

The [full final EditMode suite](CasualFieldEvidence20261014/editmode-final-summary.json) on `bd785e8e` passed 5,778 tests with zero failures and three existing diagnostic exclusions in 6,007.41 seconds, including all 18 art regressions. Field art, generator, loader and engine/package inputs stayed unchanged on latest main, so this proof and the 12 captures are reused.

The [latest-main focused run](CasualFieldEvidence20261014/latest-main-focused-summary.json) covered 698 cases in owners changed by PR #132: save, guide, combat, equipment, wording and UI. It passed 697 and found one retired-warrior-save guide-entry failure: an existing mapHero prevented the initial edict baseline from being accepted. The shared validator now permits only the exact guide baseline for its owned mandatory warrior, retaining other option locks. [Directly affected regressions](CasualFieldEvidence20261014/guide-migration-regression-summary.json) passed 120/120 after the fix, covering all three retired-save classes, fresh/mapped owner locks, guide progression and save boundaries. Other successful cases and the full suite were reused.

Final UI contract/regressions passed [15/15](CasualFieldEvidence20261014/ui-contract-delivery-summary.json); static refresh validation passed for 14 bindings and 184 UI sources. The shared validator fix changes no UI-contract receipt inputs; only changed development-fixture inputs needed final confirmation. [Reuse boundaries](CasualFieldEvidence20261014/validation-reuse.json).

The [macOS development player](CasualFieldEvidence20261014/native-summary.json) passed first-process field entry, a separate process restart and the fresh guide on the same final binary. At 1280×720, the field used the shrine, opted in/saved through the current edict window, automatically defeated four unchanged echoes in 7.60 seconds, excluded echo economy rewards and delivered the chest reward once. Map, HP, RNG, enemies and shrine duration survived restart. [Field report](CasualFieldEvidence20261014/native-field/runtime-field-smoke.txt), [opened chest](CasualFieldEvidence20261014/native-field/07-cursed-chest-opened.png).

The [guide report](CasualFieldEvidence20261014/native-guide/runtime-guide-tutorial-smoke.txt) passed Korean 440×956 fresh-guest warrior entry, Velina dialogue, Whirlwind/position/target/treasure/gathering lessons, real gold/gear, one graduation and town arrival. It uses macOS synthetic EventSystem input and accelerated actual fixed ticks; physical touch, ordinary play speed and human comprehension remain untested. All 73 guide screenshots are retained.

Startup search-index, async-admission, shader-compilation, reused-checkpoint and staging failures remain in the task validation folder; unfinished replacement frames are excluded from final evidence.

Updating the old field harness for D4 required edict mode, the shared editor entry and discovery/regroup priorities. Captured-state diagnostics isolated the failures; the complete shrine/echo/chest flow passed 1/1 before repeating the affected player flow. Enemy HP, the twelve-second limit, rewards and restart assertions were unchanged. Five development builds were required by fixture changes; [diagnostics and reproduction sources](CasualFieldEvidence20261014/field-fixture-diagnostics.zip) are retained. Successful full and related suites were reused.

Physical mobile, web runtime, matched before/after performance and APK size remain unverified. No fields remain to install; merge-candidate validation is complete. [PR #128](https://github.com/dakrdong/HELLSCRIPT/pull/128) records main integration. Final deployment records retain wiki/Pages and artifact disposition proof.
