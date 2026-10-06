# Town ground and environment rebuild (dark gothic fortress town)

Written 2026-10-06 · [한국어](Town_Dark_Gothic_Environment.md)

The town's ground and surroundings were rebuilt as a dark, heavy fortress town. Service positions (`TownLayout`), the walkable area and the existing tests are untouched: only the **presentation** changes. The ground is a four-layer PBR terrain shader; the wall, towers, gatehouses, cathedral, forest, braziers, props and the four south-row shops are 16 models generated with Tencent Hunyuan3D. Diablo IV was only a reference for mood and for how a fortress town is composed; none of that game's images, models, textures, names or logos are used.

## At a glance

| Item | Content |
|---|---|
| Ground | `TownGround.shader`: four layers (street cobble, plaza slab, packed mud, forest litter) plus a splat map built at runtime. Eight layer textures at 512 px |
| Environment models | 16: eight buildings (wall module, watchtower, gatehouse, cathedral, four south-row shops) and eight props. 65,498 triangles in total |
| Not changed | Service positions, the walkable area, tutorial and UI text. The only change is the collision rectangles of the four south-row shops, which now follow their model footprints (below) |
| Repository growth | about 37.8 MiB (FBX 6.9 + model PNG 27.0 + ground PNG 3.9) |
| Build growth | This work's share of the macOS development build, from the Build Report: +30.5 MiB (textures +26.8, meshes +3.6). The about +5.4 MiB of tutorial portraits from main that the same build merged is not counted |
| Android estimate | about 12.3 MiB of textures (ASTC 6x6) plus about 3.6 MiB of meshes. **Calculated**; no Android build was run |
| Validation | world-art check 0 problems, **full Edit Mode 5,228 of 5,229 pass, 0 fail, 1 skipped**, three town smokes pass, matched before/after town idle measurement (below) |
| Status | Development candidate. Hunyuan terms of use and release-art approval are still open (`productionApproved:false`) |

## Ground

`Resources/TownGround.shader` ("HELLSCRIPT/Town Ground") samples by the **town layout coordinates** (layout plane xz), not by UVs, so tile axes follow the streets.

| Layer | Splat channel | Tile size | Where |
|---|---|---:|---|
| `TownCobble` street cobble | R | 2.6 m | main street, north-south spine, service forecourts, around the well |
| `TownSlab` plaza slab | G | 4 m | the rift portal plaza |
| `TownMud` packed mud | B | 3 m | street shoulders, training yard, campfire spot, the road outside the gates |
| `TownForest` forest litter | the rest | 3.5 m | all other ground |
| contact shade | A | | at the foot of buildings and walls |

- `TownGroundMask.cs` builds the splat map at runtime from `TownLayout` (service positions and building rectangles) and `TownScenery.cs` (wall, plaza and yard numbers): 512x420, about 0.44 m per texel. The shader warps the lookup with noise so street edges are ragged. If a building rectangle changes, its forecourt follows.
- The mud and forest layers blend in a rotated, scaled second copy through a large-scale noise to hide repetition. Cobble and slab do not: on a regular stone grid the rotated copy read as a second, ghost grid laid over the first.
- Each layer has height blending (stones and leaves poke through at borders), wet patches (darker and glossier), large-scale tone drift, contact shade and a rune ring around the portal (a slowly breathing glow).
- There is no shadow caster pass (a flat sheet shadows nothing); the depth and normal passes keep it in SSAO. `WorldShaderTests` checks this contract and the Metal, Vulkan and GLES3 compiles.

### Making the layer textures (`tools/hunyuan_ground_tiles.py`)

A Hunyuan "material ball" GLB holds three 4096 px maps that are **an exact 4x4 repeat of one 1024 px tile** (the tool checks that the maps are identical at shifts of 1024, 2048 and 3072), and that tile does not wrap (its edge difference is 1.5 to 5.6 times the interior's).

1. Cut the 1024 tile, roll it half a period and take the area around the seam from the original tile. The boundary follows a minimum-error path, so it runs along stone joints and pebbles; colour, normal and roughness share the same path.
2. Grade saturation and per-channel gain for the night look and turn roughness into smoothness (alpha).
3. Reduce 1024 -> 512 px with an exact 2x2 box filter: colour in linear light, smoothness as data, normals as plain vectors (the source's next mip level).

Seam ratio (boundary difference over interior difference, 1.0 is ideal): cobble 3.67 -> 1.12, plaza 5.64 -> 2.68, mud 1.54 -> 0.99, forest 1.51 -> 1.02 (horizontal, in `reduction_report.json`). The plaza slab is a regular grid, so it keeps the most visible horizontal seam.

```bash
python3 tools/hunyuan_ground_tiles.py --out <work dir> --raw <raw GLB dir>
python3 tools/import_hunyuan_town_env.py --ground <work dir>
```

## Environment models

`Building_*` and `Prop_*` under `Resources/World/Town/`. The front faces the camera (-Z) and the origin is the footprint centre on the ground. The code is `WorldView.TownEnvironment.cs`; a missing model leaves its spot empty.

| Model | Height | Triangles | Texture | Placement |
|---|---:|---:|---:|---|
| `Building_WallSegment` | 5.5 m (29.3 long) | 6,000 | 1024 | perimeter wall module, stretched along its length only, four runs |
| `Building_WatchTower` | 7.6 m | 3,500 | 512 | the four corners |
| `Building_Gatehouse` | 12.2 m | 6,000 | 1024 | the two east and west gates (outside is the front) |
| `Building_Cathedral` | 27.3 m | 6,000 | 1024 | skyline beyond the north wall |
| `Building_Gambler` | 9.6 m | 6,000 | 1024 | gambler's shop (south row) |
| `Building_GemMerchant` | 9.6 m | 6,000 | 1024 | gem merchant's shop (south row) |
| `Building_RuneMerchant` | 9.6 m | 5,999 | 1024 | rune merchant's shop (south row) |
| `Building_RuneMaster` | 9.6 m | 5,999 | 1024 | rune master's workshop (south row) |
| `Prop_Pine` | 10.0 m | 2,500 | 512 | forest ring outside the wall |
| `Prop_DeadTree` | 8.35 m | 2,500 | 512 | forest edge and a few spots inside |
| `Prop_Brazier` | 1.15 m | 2,500 | 256 | four around the plaza, six along the streets; registered as real light emitters |
| `Prop_Banner` | 5.6 m | 2,500 | 512 | eight banners by the plaza and the gates |
| `Prop_Cart` | 1.79 m | 2,500 | 512 | two |
| `Prop_Goods` | 1.35 m | 2,500 | 512 | barrels, crates and sacks, four spots |
| `Prop_Firewood` | 1.65 m | 2,500 | 512 | two |
| `Prop_Boulders` | 2.2 m | 2,500 | 512 | four spots at the town corners |

The wall, towers and gatehouses can hide the hero from the camera side, so they are registered with the same `TownBuildingFade` as the houses. The four shops are found by the existing `WorldView.TownBuildingArt` (`Building_<service name>`), so the building code did not change. The pines come from `TownTreeSpots()` (seeded; outside the wall, with the gate roads and the cathedral site left empty). The old cone pines and the palisade are not built when the models are installed.

### The four south-row shops

The gambler's, gem, rune shops and the rune workshop were primitive log cabins. They are now models of the same family as the three building models (blacksmith, warehouse, weapon stall). The prompts differ per shop (card-and-dice sign, gem-shaped sign and crystals, carved rune beams, a round tower with a rune circle) and all ask for "one single building, no ground, no background". The exact wording and the SHA-256 of the originals are in `DarkGothicTownEvidence/hunyuan-generation-log.json`.

- **Base plate removed.** The generator stands a building on a thin stone plate (0.1-0.25 m thick, 0.6-0.9 m wider than the building). Used as is, a pale slab lies on the town ground. `sink` in `hunyuan_static_prep.py` lowers the model until the plate's top is at the ground and cuts what is below (it cuts the original before baking, so the bake still matches). The house rises straight out of the ground and barrels and crates stand on it.
- **Roof colour.** The generator paints slate roofs deep blue. `mute` pulls only the blue hue band (205-255 degrees) toward a mean grey and darkens it. The violet (rune), teal (gem) and amber (window) glows are other hues and stay. Lowering saturation everywhere would kill exactly those glows.
- **Collision rectangle.** The front edge stays at y -20.5 (3.5 m behind the attendant's spot at -24); width and depth follow the model footprint. Walkability is decided by rectangles only, so only the rectangles changed.

| Shop | Before (width x depth) | After | Model footprint |
|---|---|---|---|
| Gambler | 11 x 9 | 8.3 x 6.2 | 8.28 x 6.18 |
| Gem merchant | 11 x 9 | 5.3 x 8.6 | 5.33 x 8.64 |
| Rune merchant | 12 x 9 | 6.6 x 8.9 | 6.60 x 8.96 |
| Rune master | 11 x 9 | 9.3 x 6.7 | 9.19 x 6.62 |

`TownModelTests.StationBuildingModelsFillTheirCollisionFootprint` checks all seven buildings (blacksmith, warehouse, weapon stall, four shops): the rectangle must match the footprint within 0.5 m.

### From the original to the game tier

The originals were generated on the Hunyuan web (HY3D-V3.1) on 2026-10-06: text-to-geometry (50,000 faces) then texture painting. One GLB is 20-43 MiB (50,000 faces, three 4096 px maps).

1. `tools/hunyuan_static_prep.py` (Blender): decimates a copy to the PLAN section 4 triangle budget (props 2,500, buildings 6,000), keeps the generator's UVs, and bakes colour, roughness and the tangent normal **from the original** into new textures. It writes an in-game GLB (one material: colour, ORM, normal). It can grade the night look: a tone curve (`gamma`), saturation (`sat`), per-channel gain (`gain`) and `mute` for one hue band only. It cuts the watchtower body out of its walled compound (`crop`) and removes the base plate of a building (`sink`). The per-model settings are in `DarkGothicTownEvidence/bake-configs/`. The cathedral came out near white on the front and near black on the roof, so `gamma 0.5` with a gain of 0.6 puts both into one albedo range.
2. `tools/import_hunyuan_town_env.py`: reuses the town model importer (`install` in `tools/import_hunyuan_town.py`). It sizes to the height, turns the front to the camera and writes the deterministic FBX, `_A.png` (alpha = smoothness), `_N.png`, the manifest `manifest_town_env.json` and deterministic metas.

```bash
blender --background --factory-startup --python tools/hunyuan_static_prep.py -- <config>.json   # per model
python3 tools/import_hunyuan_town_env.py --source <in-game GLB dir>                             # town-object/<id>.glb inside
```

## Size and texture reduction

The first drop used 1024 px ground layers and some 1024 px props; it was then reduced.

| Item | Before | After |
|---|---|---|
| Eight ground layers (repository PNG) | 14.2 MB | 3.9 MB |
| Eight ground layers (PC/web BC7/ETC2, with mips) | 10.7 MB | 2.7 MB |
| Eight ground layers (Android ASTC 6x6, calculated) | 4.8 MB | 1.2 MB |
| Environment textures, 12 models (PC) | 18.0 MB | 13.5 MB |
| All new models (macOS development build, growth over its base) | +32.0 MB (1024 px, pre-merge base 603.1) | +19.5 MB (reduced, pre-merge) -> +18.4 MB (12 models, re-baked to the budgets and merged: main 630.1 -> 648.5) -> **+30.5 MiB** (with the four south-row shops, 16 models and the ground: eight more 1024 px BC7 maps add +12.0 MiB) |

A ground tile is 2.6-4 m, so 512 px keeps a texel at least as large as a screen pixel at the default zoom (1080p). **The 1024 and 512 builds were compared at the same spots** (`DarkGothicTownEvidence/texture-reduction/`).

- Default zoom 1600x900: the ground's mean brightness, contrast and detail (Laplacian variance) change within the difference between two captures of the same build (run-to-run noise).
- Closest zoom on a 3840x2160 frame (the worst case): cobble is unchanged (detail within +-0.1%); plaza slab loses 10.6-17.7% of fine detail and forest litter 1.4-28.2%. Side-by-side 1:1 crops show almost no visible difference. Mud was not captured under this condition (its texel density is in the same range as cobble, so no change is calculated, but that is not a measurement).

`WorldArtImporter.MaxTextureSize` and a `World/Fields/Town/` row in `ResourceTextureBudget` cap these layers at 512 (test `TextureCapsFollowTheWorldArtBudget`).

## Performance: matched before/after on the town idle view

As the [performance review rules](Performance_Review_Rules.en.md) ask, the macOS development players of the base (`origin/main` 945fa46f) and of this branch were run under the same conditions, alternating, three pairs (six runs). Apple M3 Pro, 1280x720 window, target 60 FPS, quality PC, the hero at the town's start point, about 4 s of warm-up, then a 20 s sample (about 1,190 frames). Both players carry the same measurement-tool fix (below). Raw samples, hashes and method: `DarkGothicTownEvidence/performance/`.

| Metric (median [range]) | Before | After |
|---|---:|---:|
| Process CPU, ms per frame | 5.80 [4.96-5.86] | 6.17 [6.07-6.26] |
| Process CPU, % of one core | 34.6 [29.6-34.9] | 36.4 [35.9-36.6] |
| Actual FPS (cap 60) | 59.58 [59.52-59.63] | 58.97 [58.45-59.14] |
| Mean / p95 frame interval, ms | 16.79 / 18.58 | 16.96 / 18.99 |
| GC allocation, KB per frame, collections per 20 s | 81.2, 63 | 81.4, 64 |
| Managed heap, Unity allocated, MiB | 14.6, 148.8 | 14.2, 143.7 |
| Process RSS, MiB | 546 [544-548] | 558 [460-572] |
| Triangles drawn per frame (shadow passes included) | 989,304 | 832,472 |
| SetPass calls per frame | 70.5 | 70.5 |

Reading: on this view CPU rose by about **0.37 ms per frame (+6%)**, about +1.8 points of one core. All three "after" runs are above all three "before" runs (one "before" run was unusually low, 4.96) and all three adjacent pairs point the same way. The frame rate stays near the 60 cap, about 0.6 FPS lower. GC allocation is the same. Triangles fell (the primitive houses and cone pines drew more). This is one fixed town idle view; it is not read as a logic or memory improvement.

- **Not measured:** other viewpoints (the forest ring along the north wall, the gates), WebGL, mobile, battery, GPU time. This player returned 0 for the draw call and batch counters, so draw calls are not reported (only SetPass and triangles).
- **Conditions:** the system load average at the start of the runs was 7.5 to 9.3 (other sessions' processes). Alternating the order reduces order bias; the load was not controlled.
- **The first attempt was discarded.** The measurement tool called `EnterPlaza()` from the title screen, and the title takes the page back unless the session is signed in, so all six first runs measured the **title screen** (the profile's `page` was `title`, a camera render of about 1 microsecond). The town and inventory modes of `RuntimePresentationProfile` now sign in as the offline QA guest, start through `character-start` and throw if the page is not the one asked for. The base and the after player use the same file.

## Repository contract

Everything under `Resources/World` must pass `tools/generate_world_art.py --check`: manifests with hashes, PLAN section 4 budgets, deterministic meta GUIDs, no stale files. Models are recorded in `manifest_town_env.json` (group `town_env`, revision `hellscript-hunyuan-town-v1`), the ground in `Fields/Town/manifest_town_ground.json`. The town model test (`TownModelTests`) counts manifest entries instead of a fixed number.

## Validation

| Check | Result |
|---|---|
| `python3 tools/generate_world_art.py --check` | 159 assets, 0 problems |
| `python3 tools/test_world_art.py` | 26 pass |
| **Full Edit Mode** (merged tree 019f59cf, faithful clone, 38 min) | **5,228 of 5,229 pass, 0 fail, 1 skipped** (the opt-in measurement tool `ClassSkillPassiveMeasurement`). Only one development-only profile-tool file changed afterwards, and Edit Mode was not rerun (`DarkGothicTownEvidence/edit-mode-full-run.json`) |
| Focused Edit Mode after merging `main` 6fdbd50b (PR #60: tutorial store, smoke and test changes only) | four tutorial classes, `Town*`, `WorldArt`, `WorldShader`, `BuildResource`, `FieldEnvironment`, `WorldLighting`: 160/160 pass. The full suite was not rerun |
| Town look smoke `-hellscriptTownLookSmoke` (macOS development build of the merged tree ee06e32d, 1600x900) | Passes. A capture smoke: 16 fixed spots, 2 whole-town views, 3 closest-zoom 4K shots and the 4 shops = 25 captures; its assertions are only town entry and the portal's presence |
| Town models smoke `-hellscriptTownModelsSmoke` (same build, landscape 1600x900, portrait 900x1600) | Passes (17 captures each). It now checks by mesh name that all seven station buildings (blacksmith, warehouse, weapon stall, four shops) are models, and reaches all ten services |
| Town idle view before/after performance | the table above: CPU +0.37 ms per frame, 60 FPS cap kept |
| Same-spot comparison | `DarkGothicTownEvidence/compare/` (earlier main 4fc52dba versus this branch, six captures; main's town code did not change in between) |

## Known limits and unverified items

- Android and iOS build size (the values above are calculated), real devices and WebGL were not measured. Performance was measured on one town idle view of the macOS development player (the forest ring and gate viewpoints, draw calls: not measured). Mobile and web could see a larger CPU increase from the same 16 models and 10 lights and need their own measurement.
- Of the runtime smokes only the three town smokes were run. The other smokes cover screens this change does not touch and were not run.
- The three limits of the earlier record (plaza slab ghost grid, pine/boulder/cathedral colour, four south houses still primitives) are resolved: the slab layer's overlay is off, the pines, boulders and cathedral were re-baked, and the four houses are models. The slab has no repetition relief now, so a 4 m tile repeats across the plaza (about 30 x 21 m); the large-scale tone drift and the wet patches hide the repeat.
- The existing warehouse and blacksmith building models (the 20-model town work) keep their saturated blue roofs and moss green. The four new shops have the blue pulled out, so the warehouse and blacksmith look more vivid next to them. Those models were not touched.
- The boulder cluster still reads as smooth rounded lumps (the original shape); only its colour was pressed to grey.
- A shop model is open where its base plate was cut (at ground height). It is below the ground and invisible, but lifting the model would show a hole in its underside.
- A few thin slivers of the cut-off wall remain at the foot of the watchtower (not visible at game size).
- The Hunyuan terms of use (version 2026.2.6, sections 6.3-6.7) do not assume approval for public distribution. They need review before release.

## Provenance

- Generated on the Hunyuan web (hy3d.tencent.ai, HY3D-V3.1) on 2026-10-06 with the user's account: 16 models (text-to-geometry at 50,000 faces plus texture painting) and four ground materials (material ball). Prompts and the raw GLBs' SHA-256 and sizes are in `DarkGothicTownEvidence/hunyuan-generation-log.json`; the hashes of the processed in-game GLBs are in the manifest `source`. **The 24 raw GLBs (about 708 MiB) and the 12 original 1024 px texture files (19 MiB) were deleted at the user's request ("delete the originals since the lightened files exist")** once the processed models were confirmed installed by hash. Regenerating them costs Hunyuan quota and does not give the same mesh. The 16 lightened in-game GLBs (41 MB) are kept outside the repository in `AssetDeliverables/Hunyuan/2026-10-06/town-environment/game-tier/`. Re-grading from the original colour (a new grade would be applied on top of the already graded in-game maps) and the exact 1024 px ground tiles are no longer possible.
- The shader, mask, placement code and conversion tools were written in this repository. No outside models, textures or scans are used.
- Evidence: `DarkGothicTownEvidence/` (`after/` 13 final captures, `compare/` six comparisons with the earlier main, `texture-reduction/` reduction measurements, scripts and 1:1 comparison crops, `performance/` the town idle before/after measurement, `bake-configs/` per-model bake settings, `edit-mode-full-run.json`).
