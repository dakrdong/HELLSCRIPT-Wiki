# Dark Gothic Overhaul, Stage 1: rendering foundation, six fields, five bosses

Updated: 2026-09-28 · [한국어](Dark_Gothic_Overhaul_Stage1.md)

This is the first merge of the work that turns the whole game toward a dark, heavy action-RPG mood. This stage adds the world shaders, lighting and post-processing, the procedural 3D art tools and the model import and rig code, six rift fields with per-field enemy lineups, and two-phase pattern kits for five bosses. The new 3D models are not wired into the view yet. The effect library, the own attacks of the eight new regular enemies, sounds for the new content and the monster-tier (Magic, Elite, Legendary, Unique) effects continue in the next stage.

## Direction and provenance

The target is the mood Diablo IV conveys: cold moonlight against warm firelight, desaturated earth and stone colours, rough worn materials and weighty effects. It is a mood reference only: **no Diablo IV model, texture, effect, name or logo was extracted or used.** The shaders and post-processing settings were written in this repository, and 3D assets are built by in-repo Blender scripts from seeded geometry and Blender's built-in procedural nodes. No external meshes, textures or scans were downloaded, and no image or 3D generation model was used. The provenance entry is in [Asset provenance](Asset_Provenance.md).

## Contents

| Area | What it contains |
| --- | --- |
| World shaders | [`RiftTerrain.shader`](../../Assets/HELLSCRIPT/Resources/RiftTerrain.shader) becomes the shared world lit shader: normal maps, smoothness in the albedo alpha, main-light shadows, additional lights (Forward on mobile, Forward+ on PC), rim light, hit flash, death dissolve and a broad colour variation. It gains ShadowCaster, DepthOnly and DepthNormals passes and stays SRP Batcher compatible. The fog-of-war rules are unchanged. [`WorldFx.shader`](../../Assets/HELLSCRIPT/Resources/WorldFx.shader) for effects and [`WorldTelegraph.shader`](../../Assets/HELLSCRIPT/Resources/WorldTelegraph.shader) for warning decals are added. The town fade keeps textures and the town ground receives additional lights. |
| Lighting and post-processing | [`FieldLook.cs`](../../Assets/HELLSCRIPT/Runtime/Presentation/FieldLook.cs) holds the lighting presets for the six fields, the town and training. [`WorldLighting.cs`](../../Assets/HELLSCRIPT/Runtime/Presentation/WorldLighting.cs) runs trilight ambient, fog, the moonlight, a warm light that follows the hero, a pool of real point lights handed to the nearest emitters such as braziers (8 on PC, 3 on mobile), gentle flicker and camera shake. Shake is zero in the settings view, during snapped presentation and when motion is reduced. Post-processing uses PC and mobile profiles (ACES tonemapping, bloom, colour adjustments, vignette, split toning, film grain on PC only), with SMAA on PC and FXAA on mobile. The existing emission multiplier drops from ×1.8 to ×0.8 so danger reds stay saturated after tonemapping. |
| Six rift fields | [Six rift fields](../Design/HELLSCRIPT_Rift_Fields.en.md). The field is drawn from its own random stream, so existing seeds keep their rooms, fingerprints and combat random numbers. Each field applies its lighting preset and a floor tint, and the battle heading shows its name. |
| Twenty regular enemies | N13–N20 gain names, fields, roles and stats. Their own attacks are in development; for now they use N01's melee swing. Role checks go through `EnemyCombat.Role`, which gives N01–N12 the same results as before. |
| Five bosses | [Boss combat detail](../Design/HELLSCRIPT_Boss_Combat_Detail.en.md). Every boss has three phase-1 and three phase-2 patterns, and phase 2 starts with a 1.5 s roar. 24 new patterns and the roar are added, and the damage forecast equals the real damage for every pattern. The two new bosses appear from stages 31 and 41. New attacks borrow the closest existing sound for now. |
| Procedural 3D art tools | [`hs3d.py`](../../tools/art3d/hs3d.py) provides geometry, procedural materials, shared atlas baking (2× supersampled colour, roughness and normal, gutter filling, 0.3 m AO reach), previews from the game camera direction and byte-stable FBX export. [`generate_world_art.py`](../../tools/generate_world_art.py) discovers recipe modules, builds each asset in its own Blender process, writes manifests and deterministic-GUID metas, and `--check` verifies files, hashes, budgets and code references. |
| Model import and rig | [`WorldArtImporter.cs`](../../Assets/HELLSCRIPT/Editor/WorldArtImporter.cs) merges the parts of a model with `Pivot_*` bones into one skinned mesh, so a character draws in one call; glow parts are marked in the vertex colour alpha. [`WorldArt.cs`](../../Assets/HELLSCRIPT/Runtime/Presentation/WorldArt.cs) loads models, textures and manifests and builds materials; a missing asset returns null so the existing primitive view keeps working. [`ActorRig.cs`](../../Assets/HELLSCRIPT/Runtime/Presentation/ActorRig.cs) animates walking, idling, windup, attack, recovery, charge, hit and death procedurally for seven skeleton types (biped, quadruped, hover, blob, worm, spider, serpent) without per-frame allocations. |
| Review smoke | [`RuntimeArtGallerySmoke.cs`](../../Assets/HELLSCRIPT/Runtime/Presentation/RuntimeArtGallerySmoke.cs) (`-hellscriptArtGallerySmoke`) captures the rooms of all six fields, every regular enemy, the bosses, elites, effects and the town in one run. The next stage compares against the same list when models and effects are wired in. |

The Android texture budget gains rules for `World/`: bosses, fields and effects up to 1024, everything else 512, ASTC 6×6.

## What changes in the game

- Rifts and the town get post-processing and the new lighting. A warm light follows the hero, and braziers and lanterns receive real lights.
- Every rift has a field; its name, lighting, fog and floor colour differ. Dank Cavern and Dusk Grassland appear from stage 1, Scorched Desert and Snowbound Highland from stage 11.
- Bosses roar below 50% HP and then use their phase-2 patterns. The Dune Tyrant appears from stage 31 and the Hoarfrost Matriarch from stage 41.
- Heroes, enemies, bosses and rooms are still drawn with the existing primitive shapes.

## Building and reproducing

```sh
python3 tools/generate_world_art.py --only Sample --out Builds/WorldArt   # build the two samples with Blender (not installed)
python3 tools/generate_world_art.py --check                               # verify the installed World/ assets (no Blender)
python3 tools/test_world_art.py                                           # runner unit tests
```

Unity tests and smokes run in a cloned project instead of the original, which the Editor keeps locked.

```sh
Unity -batchmode -nographics -projectPath <clone> -runTests -testPlatform EditMode -testResults <results.xml>
Unity -batchmode -projectPath <clone> -executeMethod Hellscript.Editor.ProjectBuilder.BuildMac -hellscriptBuildOutput <build>/HELLSCRIPT.app -quit
HELLSCRIPT -hellscriptArtGallerySmoke -hellscriptSavePath <save> -hellscriptScreenshots <shots> -screen-fullscreen 0 -screen-width 1600 -screen-height 900
HELLSCRIPT -hellscriptBossSmoke -hellscriptBossResume 4 -hellscriptSavePath <save> -hellscriptScreenshots <shots> -screen-fullscreen 0 -screen-width 1600 -screen-height 900
```

## Verification

Checked on 2026-09-28 in a cloned project at integration commit `c6856548`.

- **macOS development build**: `BuildMac` succeeded with 0 errors. [Build result](DarkGothicStage1Evidence/build-result.txt)
- **Tutorial smoke**: the first launch and the `-hellscriptTutorialResume` relaunch both exited with code 0, covering the real boss kill, the town arrival and 20 resolution, language and text-size layouts. [Result](DarkGothicStage1Evidence/runtime-tutorial-smoke.txt), [real boss cleared](DarkGothicStage1Evidence/tutorial-real-boss-cleared.png)
- **Art gallery smoke**: exit code 0, 39 captures. [Result](DarkGothicStage1Evidence/runtime-art-gallery-smoke.txt)
  - Rooms of the six fields: [Forgotten Graveyard](DarkGothicStage1Evidence/gallery-01-room-field0.png), [Ruined Fortress](DarkGothicStage1Evidence/gallery-02-room-field1.png), [Scorched Desert](DarkGothicStage1Evidence/gallery-03-room-field2.png), [Dank Cavern](DarkGothicStage1Evidence/gallery-04-room-field3.png), [Dusk Grassland](DarkGothicStage1Evidence/gallery-05-room-field4.png), [Snowbound Highland](DarkGothicStage1Evidence/gallery-06-room-field5.png), [side by side](DarkGothicStage1Evidence/fields-sheet.png)
  - Phone aspect ratios: [landscape 956×440](DarkGothicStage1Evidence/gallery-07-room-field0-956x440.png), [portrait 440×956](DarkGothicStage1Evidence/gallery-08-room-field1-440x956.png)
  - Twenty regular enemies: [N01–N06](DarkGothicStage1Evidence/gallery-09-enemies-00-05.png), [N07–N12](DarkGothicStage1Evidence/gallery-10-enemies-06-11.png), [N13–N18](DarkGothicStage1Evidence/gallery-11-enemies-12-17.png), [N19–N20](DarkGothicStage1Evidence/gallery-12-enemies-18-19.png)
  - Five bosses: [Executioner](DarkGothicStage1Evidence/gallery-13-boss-0.png), [Chorister](DarkGothicStage1Evidence/gallery-14-boss-1.png), [Devourer](DarkGothicStage1Evidence/gallery-15-boss-2.png), [Dune Tyrant](DarkGothicStage1Evidence/gallery-16-boss-3.png), [Hoarfrost Matriarch](DarkGothicStage1Evidence/gallery-17-boss-4.png), [elites](DarkGothicStage1Evidence/gallery-18-elites.png), [town](DarkGothicStage1Evidence/gallery-38-town-portal.png)
- **Boss pattern smoke**: all 30 patterns of the five bosses and the five roars warned and then released on their preparation time (within half a step between the warning and the first release). [Result](DarkGothicStage1Evidence/runtime-boss-kit-smoke.txt)
  - Captures: [Executioner roar](DarkGothicStage1Evidence/bosskit-k1-0-Roar-warning.png), [Execution Leap](DarkGothicStage1Evidence/bosskit-k1-4-ExecutionLeap-warning.png), [Requiem Choir](DarkGothicStage1Evidence/bosskit-k2-4-RequiemChoir-warning.png), [Devouring Pull status](DarkGothicStage1Evidence/bosskit-k3-6-DevouringPull-status.png), [Quicksand Maelstrom](DarkGothicStage1Evidence/bosskit-k4-4-QuicksandMaelstrom-warning.png), [Blizzard Veil](DarkGothicStage1Evidence/bosskit-k5-4-BlizzardVeil-warning.png), [Shatter Fan](DarkGothicStage1Evidence/bosskit-k5-6-ShatterFan-warning.png)
- **Full EditMode suite**: 4,248 of 4,295 passed and 47 failed. All 47 failures are in the pre-overhaul baseline; there are no new failures. [Summary](DarkGothicStage1Evidence/editmode-full-summary.txt)
- **UI contract, world art and wiki checks**: `check_ui_contract.py`, `test_ui_contract.py` (9), `test_world_art.py` (24), `generate_world_art.py --check` and `wiki.py build` and `check` passed.
- **Visual check**: the six field captures were compared side by side. Each field has its own light colour and floor tint, and the room outline and the hero's surroundings read in all of them. Dank Cavern and Ruined Fortress are the darkest. Dusk Grassland still tints the shared stone floor, so it reads more like brown paving than grass; it will be reviewed again when the field floor models are wired in.

## Not verified

- Nothing was seen on a physical mobile device or in an Android build. The mobile shaders were only compiled offline for Vulkan, GLES3 and iOS; the mobile post path (low-quality bloom, FXAA) and device performance were not measured.
- There is no settings screen yet to turn camera shake and flashes off; the game only reads the device value `hellscript.reduce-motion`.
- The tutorial smoke failed 2 of 5 times on the lighting workstream's build, at steps unrelated to lighting (a one-frame race in the armour grant, the wait for the training result). The cause was not found.
- Under the new lighting, remembered terrain (explored but not in sight) looks darker than before. It will be re-tuned when the field floor models are wired in.
- This is not a human quality review. The work was checked from captures and measurements; the final judgement is the user's.

## Relation to earlier work and next stage

The presentation rules of the [forest settlement](Forest_Settlement.md) and of the rift sight and remembered terrain work are kept; only lighting and shaders change. The next stage continues on a new branch:

- Wire the 3D models being built on the work branches into the view: three heroes with weapons, twenty regular enemies, five bosses, floors, walls, obstacles and decorations for six fields, shared props such as chests, altars and gates, and the town's buildings, portal and NPCs.
- Build the effect library: skills, enemy and boss attacks, filled warning decals, hits and deaths, loot beams, field ambience and the monster-tier effects.
- Add the own attacks of N13–N20 and sounds for the new content.
