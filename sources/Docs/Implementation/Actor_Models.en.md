# Hero, Enemy and Boss Models on Screen

Updated: 2026-09-28 · [한국어](Actor_Models.md)

The 3D models made in stage 1 of the [dark gothic overhaul](Dark_Gothic_Overhaul_Stage1.en.md) now appear in fights and in town. This is the fifth item of stage 2. A body with a model is drawn with it; a body without one keeps the placeholder shapes it had. No game rule or value changed.

## Models on screen

| Body | Skeleton | Notes |
| --- | --- | --- |
| Hero: Warrior, Ranger, Mage | Biped | The Warrior carries a sword and shield, the Ranger a bow and quiver, the Mage a staff, each on the model's attachment points. |
| Golden Goblin | Biped | |
| N01 Chained Corpse, N03 Skeleton Archer, N04 Plague Servant, N05 Funeral Priest | Biped | |
| N02 Grave Hound | Quadruped | |
| N06 Bloated Pilgrim | Blob | |
| N07 Ironclad Wraith | Hover | |
| B01 Graveyard Executioner | Biped | |
| B02 Chorister of the End | Hover | |
| B03 Devourer of the Rift | Quadruped | |

N08–N20, B04 Dune Tyrant and B05 Hoarfrost Matriarch have no models yet and fight as placeholder shapes. Once their model files are added they switch to them without a code change.

## Motion and effects

- Each model gets an [`ActorRig`](../../Assets/HELLSCRIPT/Runtime/Presentation/ActorRig.cs) that turns the action phases into body motion: walking, winding up (a casting pose for casters), charging, travelling attacks, striking, recovering, boss stagger and hit flinches. The hero also takes its windup, channel, movement-skill and strike poses.
- A killed enemy collapses for one second and then dissolves. Placeholder shapes still dissolve at once.
- Hit flashes, the rim that heats up during a windup, monster-tier colours and the blink fade reach models exactly as they reach placeholder shapes. The model material's cool rim stays on while those effects run.
- The models are slimmer than the placeholder shapes and looked small at the fight camera's distance, so heroes, regular enemies and the goblin are drawn at 1.25 times their modelled height. Bosses stand 4.3–5 m and keep their size. The health bar sits above the model's head.
- Placeholder bosses were scaled 2× wide and 1.7× tall. Model bosses are not, so their leap height, burrow depth and lunge are scaled by the same factors.
- Elites now wear the monster-tier effect from the effects library (a sigil under the feet and a tier-coloured rim) instead of the purple ring. The ring comes back only when the effects library fails to load.
- The hero walks as a model in town too.

## Hero silhouette behind walls

The golden silhouette that shows the hero behind a wall or pillar now covers the model too. On the first try it also painted every spot where the hero's own arm, shield or cape covered another part of the hero, and the whole hero looked washed-out yellow. Two changes fixed it:

- The hero's materials leave a stencil mark (bit 64) on the pixels they draw, and the silhouette is drawn only where that mark is missing, that is, only where something else covers the hero.
- The silhouette is pulled 0.75 m toward the camera, so nothing within 0.75 m in front of the hero makes a silhouette. The placeholder hero, used when a model is missing, no longer tints where its shield overlaps its body either.

## Verification

Checked on 2026-09-28 in a cloned project and a macOS development build.

- **Full EditMode suite**: 4,554 of 4,602 passed. 47 of the 48 failures are in the pre-overhaul baseline. The other one, the `ObjectiveProgressTests` offering exploration check, went over its 180 s limit (190 s in the full run, 201 s when rerun alone). Both times other Unity builds and Blender bakes were running; in item 4 it passed in 168 s. The check only runs the simulation core, which this item does not change. Its running so close to the limit is filed as a separate task. [Summary](ActorModelsEvidence/editmode-full-summary.txt)
- **New check in `WorldArtTests`**: all three heroes, the golden goblin, N01–N07 and B01–B03 resolve their models with manifest facts, and every hero weapon's attachment point exists on the model. N20 and B05, which have no model yet, still ask for the right model names.
- **`-hellscriptArtGallerySmoke` passed (54 captures)**: in the new "hero behind a wall" scene the hero is fully hidden by a 4.6 m wall and its golden silhouette shows on the wall. The same scene checks that the hero's materials carry the stencil mark. [Result](ActorModelsEvidence/runtime-art-gallery-smoke.txt)
  - [Regular enemies](ActorModelsEvidence/enemies-n01-n07.png): the N01–N07 models, and placeholder shapes for N08–N12, which have no models yet
  - [Bosses](ActorModelsEvidence/bosses-b01-b03-b04.png): the B01–B03 models and the placeholder B04
  - [Elites and the hero behind a wall](ActorModelsEvidence/elites-and-hero-behind-wall.png): the elites' tier effect and the silhouette of the hidden hero
  - [The hero in a room and in town](ActorModelsEvidence/hero-room-and-town.png)
  - [Silhouette before and after the fix](ActorModelsEvidence/hero-silhouette-before-after.png): the first wiring on the left, the fix on the right
- **Attack effect and boss movement smokes passed**: models wind up, strike, flinch and die (collapse, then dissolve), and the B01 model rises into the air during its execution leap. [Attack effects](ActorModelsEvidence/runtime-attack-fx-smoke.txt), [boss movement](ActorModelsEvidence/runtime-boss-motion-smoke.txt), [captures](ActorModelsEvidence/windup-hit-death-leap.png)
- **Tutorial smoke passed**: across its two runs it went through real combat, the tutorial boss kill, the town arrival, and the first rift entry and result. [Result](ActorModelsEvidence/runtime-tutorial-smoke.txt)
- **Re-checked after merging main**: after merging the main that brought the staged tutorial, a fresh build passed the tutorial smoke (both runs) and the gallery smoke (54 captures) again. [Tutorial result](ActorModelsEvidence/runtime-tutorial-smoke-after-main-merge.txt)
- **Other smokes**: the warning gauge, new enemy and boss kit smokes and the first stage of the enemy and boss smokes passed. They ran on the build just before the gallery scene was added, whose game code equals the final build's. The two relaunch stages of the enemy smoke and the plaza smoke failed; they failed with the same messages on builds from before the models were wired.

## Not verified

- Nothing was seen on a physical device or in an Android build.
- The mobile cost of skinned meshes and the stencil was not measured.
- N08–N20, B04–B05, field floors, walls and props, chests, and town buildings and NPCs are not on screen as models yet.
- This is not a human quality review.
