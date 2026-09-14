# HELLSCRIPT real-time visibility and exploration maps

Date: 2026-09-14

[한국어](Rift_Visibility_Expansion.md)

## Play rules

Visibility covers 360 degrees within 12 metres of the player. The saved dungeon floor, walls, corners and sight-blocking obstacles determine line of sight. Entering a room no longer discloses its entire geometry: the visible portion can be revealed through a doorway before entry.

Unseen terrain is black. Currently visible terrain retains its original colour. Previously seen terrain outside current sight remains dark grey. Only floors and fixed boundary walls are remembered in the world view; enemies, chests, shrines and decorative props require current sight. A floor counts as explored when seen, even if the player has not stepped on it.

## Overlay map and settings

The overlay keeps a player marker at the centre of the actual battle viewport. Translucent lines show explored exterior walls and passage outlines. The map follows the displayed player position every frame and aligns with the oblique camera. HUD controls and modal settings remain above it. Map graphics do not receive mouse or touch input.

Settings → Screen → Show overlay map is enabled by default. Changes apply immediately and are saved in the device-only hellscript-overlay-map-v1.json file. Exploration continues while the overlay is hidden. The existing minimap and expanded map use the same detailed exploration mask. Discovered point-of-interest markers are retained in those existing maps; current enemy locations require current observation.

## Ownership and implementation

This feature is separate from dungeon generation and circulation rules. It reads the saved floor and occlusion geometry from [RiftSurface.cs](../../Assets/HELLSCRIPT/Runtime/Core/RiftSurface.cs) and [RiftNavigation.cs](../../Assets/HELLSCRIPT/Runtime/Core/RiftNavigation.cs), without regenerating the map.

[RiftVisibility.cs](../../Assets/HELLSCRIPT/Runtime/Core/RiftVisibility.cs) maintains current visibility and cumulative discovery on a 0.35-metre grid. It updates after simulation movement and gate state changes. Camera aspect ratio, camera zoom and the overlay preference do not change knowledge. Teleports reveal the destination only, without tracing an imaginary explored route between endpoints.

The existing visited-room list remains the input for autonomous exploration. A separate discovery field is serialized with RunState; runtime caches are excluded. New rifts start with an empty record. Older saves migrate the rooms they had already revealed and passages whose two endpoint rooms were visited. Legacy training and fixed fields retain their existing world visibility handling.

[RiftFogView.cs](../../Assets/HELLSCRIPT/Runtime/Presentation/RiftFogView.cs) and the terrain shader apply the shared mask to the world. [RiftAutomap.cs](../../Assets/HELLSCRIPT/Runtime/Presentation/RiftAutomap.cs) draws the centred overlay. Texture and cloned material allocations belong to the dungeon root and are released with it. The overlay moves its cached outline mesh instead of rebuilding it every frame. Visibility queries cache nearby occluders while retaining the same exact floor clipping and obstacle intersection rules.

## Validation

All 64 targeted Edit Mode checks passed in an isolated Unity 6000.6.0f1 project. Coverage includes doorway visibility, occlusion, remembered terrain, save restoration, legacy discovery migration, teleportation, gate changes, agreement with authoritative navigation line of sight, and preference write failure/retry. The suite also covers exploration routes, localization and view distance. See the [test results](RiftVisibilityEvidence/editmode-results.xml).

The macOS development build succeeded with zero build errors. A native gameplay run verified hidden props, the centred noninteractive overlay, a pointer-operated toggle, both languages and orientations, and combat in an actual generated dungeon. A separate native launch restored discovery and the disabled-overlay preference. Existing screen-settings smoke tests also passed: ten aspect ratios, all 21 reading sizes and view distances, slider/wheel/button input, pause/resume, and persistence after a separate process restart.

At a 0.35-metre grid and 12-metre sight radius, 60 updates on one generated map averaged 2.85 ms. The entire grid contained 200,604 cells. A short native gameplay window recorded about 51.90 FPS including screenshot overhead. These are local macOS development observations, not a target-device performance guarantee. Physical mobile performance and long-session memory/frame-time profiling across multiple seeds remain unverified.

A controlled two-room, passage and pillar fixture documents [doorway sight](RiftVisibilityEvidence/01-doorway.png), [room interior](RiftVisibilityEvidence/02-room.png), [remembered terrain](RiftVisibilityEvidence/03-remembered.png), [Korean settings](RiftVisibilityEvidence/04-settings-ko.png), [hidden overlay](RiftVisibilityEvidence/05-overlay-off.png), [English portrait settings](RiftVisibilityEvidence/06-settings-en-portrait.png), [portrait overlay](RiftVisibilityEvidence/07-overlay-portrait.png), and [restart restoration](RiftVisibilityEvidence/09-restart.png). The [generated dungeon screenshot](RiftVisibilityEvidence/08-generated.png) uses the actual dungeon generation rules.

Evidence: [native run](RiftVisibilityEvidence/runtime.txt), [native restart](RiftVisibilityEvidence/restart.txt), [existing screen-settings smoke](RiftVisibilityEvidence/settings-runtime.txt), [screen-settings restart](RiftVisibilityEvidence/settings-restart.txt), [conditions, measurements and source hashes](RiftVisibilityEvidence/validation.json), [visibility tests](../../Assets/HELLSCRIPT/Tests/Editor/RiftVisibilityTests.cs), and [native smoke implementation](../../Assets/HELLSCRIPT/Runtime/Presentation/RuntimeVisibilitySmoke.cs).

## Relationship to previous rules

This record replaces the room-wide disclosure rules in the [rift exploration design](../Design/HELLSCRIPT_Rift_Exploration_Detail.md) and [rift implementation record](Rift_Expansion.md) with detailed current visibility and cumulative discovery. The supplied screenshot is a visual reference for a translucent player-centred outline map. Its game data and captions are not additional requirements.

Generation, circulation and boss rules from the [organic rift implementation](Organic_Rift_Expansion.en.md) remain in use. This work leaves the other task's generator, floor geometry and navigation code unchanged, integrating through the handed-over world, minimap and simulation entry points. Unity packages, scenes, prefabs and existing image assets are unchanged.
