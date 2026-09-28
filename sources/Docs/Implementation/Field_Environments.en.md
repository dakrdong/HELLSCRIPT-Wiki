# Field Environments: Floors, Walls, Obstacles and Decor

Updated: 2026-09-28 · [한국어](Field_Environments.md)

Rift rooms and passages are now dressed with each field's own 3D assets. This is the sixth item of stage 2 of the [dark gothic overhaul](Dark_Gothic_Overhaul_Stage1.en.md). It uses the Forgotten Graveyard, Ruined Fortress and Scorched Desert assets made in stage 1 (F0–F2). The Dank Cavern, Dusk Grassland and Snowbound Highland (F3–F5) have no assets yet: they keep the tinted shared stone floor and placeholder shapes, and only their walls follow the new rules. No game rule, collision or sight rule changed.

## What changed

- **Floors**: the field's floor tile (4 m repeat) with its normal map. A broad colour variation on top keeps the repeat from showing.
- **Edges**: the low stone lip at the floor's edge wears the field's trim tile.
- **Walls**: edges that face away from the camera get 3.3–4.4 m walls whose tops vary by position, so they look broken. Where other floor lies close behind a wall, the wall stops at a height that keeps that floor in view. Camera-side edges keep only the low lip, so nothing hides the room. Corners meet their neighbours without gaps.
- **Wall columns and torches**: the field's wall columns stand about every 5 m along tall walls, and every second one carries a torch with a warm light.
- **Obstacles**: graves, pillars, rubble, altars, sarcophagi, statues, walls, racks and braziers use the field models, fitted to the obstacle's gameplay footprint and height and turned 90 degrees where that suits the shape. Braziers give light.
- **Decor**: up to eight small field props per room line the inside of its edge, kept clear of obstacles, doors, chests, altars, boss points and the start. Their places come from the map's decoration seed, so a rift always looks the same.
- **Chests**: the wood, iron and cursed chest models, whose lids open on a hinge at the back. The seal shows as a glowing ring on the floor and goes away when the chest opens.
- **Ambient particles**: each field gets its own drifting particles (graveyard mist, fortress ash and embers, desert sand, cavern spores, grassland pollen, highland snow). The old ring maps such as training use their theme's particles.

## Fog of war

Walls, wall columns, torches and decor are remembered like the floor: black where unexplored and grey where already seen. Walls and their tops stay within 0.7 m of the floor's edge, inside the sight system's allowance. Obstacle models appear and vanish with sight renderer by renderer, as the placeholder shapes did.

## Verification

Checked on 2026-09-28 in a cloned project and a macOS development build.

- **Focused Edit Mode checks after integration**: all 157 passed, covering environment, audio, new-hero defaults, shared UI, world art, shaders and effects. [Integration evidence](CompletedIntegrationEvidence/editmode-summary.txt)
- **`FieldEnvironmentTests` (new)**: walls over 3 m stand only on edges facing away from the camera, and camera-side edges keep the low lip. With other floor close behind, a wall stops at a height that keeps that floor in view. In an irregular room every wall and lip vertex stays within 0.7 m of the floor's edge. Each obstacle kind and shape asks for the right field model, and field content (chests) keeps its own view.
- **Targeted suites**: the new tests plus the world art, shader, effects, sight, floor surface and warning gauge suites, 129 tests, all passed.
- **`-hellscriptArtGallerySmoke` passed (57 captures)**: each field room capture reports its wall columns, torches, decor and obstacle models. [Result](FieldEnvironmentsEvidence/runtime-art-gallery-smoke.txt)
  - Forgotten Graveyard: 130 wall columns, 18 torches, 31 decor, all 19 obstacles as models
  - Ruined Fortress: 181 wall columns, 35 torches, 48 decor, all 17 obstacles as models
  - Scorched Desert: 130 wall columns, 20 torches, 33 decor, all 15 obstacles as models
  - Dank Cavern, Dusk Grassland, Snowbound Highland: no assets yet, so all zero (placeholder shapes)
  - [Three rooms with the fog lifted](FieldEnvironmentsEvidence/revealed-graveyard-fortress-desert.png): for the evidence only, the fog was painted open so floors, walls and decor beyond sight show. No game rule changed.
  - [Graveyard close-ups](FieldEnvironmentsEvidence/graveyard-torch-props-columns.png): a wall column carrying a burning torch and its light, a grave obelisk with bone decor, and a row of columns along a tall wall with a broken top
  - [Fields without assets](FieldEnvironmentsEvidence/start-rooms-cavern-grassland-highland.png): the tinted shared stone floor with walls under the new rules
- **Fixed on the way**: when a rift was cleared and a new one built in the same frame, the new rift borrowed the dying effects library, so torch flames rendered magenta and the ambient particles vanished. Clearing the world now drops that reference, and the captures show no magenta pixels afterwards.

## Not verified

- Nothing was seen on a physical device or in an Android build.
- F3–F5 have no floor, wall, obstacle or decor models yet.
- The mobile cost of the wall lights and particles was not measured.
- The grey of already-seen areas was not re-tuned for the new floors.
- This is not a human quality review.
