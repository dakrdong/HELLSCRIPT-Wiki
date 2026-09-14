# HELLSCRIPT Organic Rift Geometry and Nearby Boss Summoning

**Follow-up:** version 7 density and route limits are documented in [Compact Rift Layouts](Compact_Rift_Expansion.en.md). Version 6 measurements and evidence below remain as historical records.

Date: 2026-09-14 · Map version: 6

[한국어](Organic_Rift_Expansion.md)

## Goal and reference scope

Replace repeated rectangular rooms and right-angle corridors with asymmetric spaces and flowing passages. An additional central room and two intersecting transverse routes make exploration more than one lap around the perimeter. Remove dedicated boss arenas and summon the boss near the player when the kill meter fills.

Blizzard’s [Diablo IV Quarterly Update—March 2022](https://news.blizzard.com/en-us/article/23788294/diablo-iv-quarterly-updatemarch-2022) describes combining authored tile sets for randomized dungeons and using transition scenes between sets. Those public principles informed this implementation; Diablo IV’s private generator code or exact algorithm was neither obtained nor copied. Attached maps provide shape and connectivity references. Their place names, bosses and quest annotations are not independent game requirements.

## Required generation rules

| Area | New-rift rule |
|---|---|
| Room outline | A softly varied elliptical base preserves combat, chest and doorway clearances and saves an asymmetric contour. |
| Room composition | Add one central room to six through eight perimeter rooms, giving seven through nine rooms in total. The central room has independent entrances and exits. |
| Circulation | Every room has at least two independent exits. Removing any one corridor or room must leave the remaining regions connected. A tour must visit every room once and return to the entrance. Dead-end wings and two loops joined by a single passage are forbidden. |
| Transverse routes | Connect four distinct perimeter rooms through two transverse routes, with one route continuing through the central room. Both routes meet at a traversable X outside the rooms. |
| Crossing geometry | Each route has nine metres on either side of the crossing and meets the other at 60–120 degrees. Endpoint-only T junctions and parallel overlaps do not qualify. Random candidates and fixed fallbacks use the same validator. |
| Doorways | Reserve inward sockets before connecting the perimeter. Supplement sockets on clear sides when needed. |
| Passages | Remove unnecessary grid turns and introduce smooth bends. Four-metre doorway necks widen gradually to about 6.3 metres. |
| Boss space | Generate no dedicated arena, antechamber or boss gate. Arena templates RM06 and RM12 remain available for legacy compatibility only. |

The central room lies within ten metres of the ring’s geometric centre and holds real combat encounters, including elites. New rifts have no dead-end wings; rewards are distributed among rooms on circulation routes. Template DB dimensions are placement envelopes; the saved contour determines the actual floor.

Four consecutive perimeter rooms A/B/C/D receive connections A–C and B–central–D. Retaining the perimeter ring supports a full tour through A→C→B→central→D and back to the entrance through the remaining perimeter. [RiftCirculation.cs](../../Assets/HELLSCRIPT/Runtime/Core/RiftCirculation.cs) independently checks the actual room/corridor graph for random candidates and fixed fallbacks. This prevents topology from forcing a return along the same passage; voluntary revisits and loot/combat pursuit may still occur.

## Summoning and progression

Normal kills add one meter point and elite kills add five. At 100 points, the run enters the boss phase once and searches nearby for a summon location. New rifts do not generate the former seal, essence-carrier or offering objectives; objective completion cannot summon a boss ahead of the meter. Chests, shrines and cursed-chest events remain.

A summon point must be 6–18 metres from the player, with an actual travel path of at most 26 metres. Selection checks the boss’s 1.2-metre landing clearance, connected floor, living enemies, hostile ground effects and attack telegraphs. A point across a wall is rejected if it requires a long detour. Temporary crowding keeps combat running and retries every 0.25 seconds instead of faulting the run.

Candidate ordering uses a separate derivation of the map seed without consuming combat or reward RNG. Actual spawning uses the established enemy creation, attacks and rewards. The saved boss identifier prevents duplicate summons after restart. Map markers and automatic pursuit follow the actual boss position.

Budgets remain 110 normal enemies, eight elites, a 100-point meter, the existing time limit and boss rewards. Five through seven chests are distributed using total rooms minus two, while combined chest gold/materials and two item rewards remain unchanged. This is not a balance adjustment.

## Shared geometry and ownership

[RiftGenerator.cs](../../Assets/HELLSCRIPT/Runtime/Core/RiftGenerator.cs) owns generation order and placement. [RiftCrossRoutes.cs](../../Assets/HELLSCRIPT/Runtime/Core/RiftCrossRoutes.cs) creates and validates the central room and crossing. [RiftOrganicGeometry.cs](../../Assets/HELLSCRIPT/Runtime/Core/RiftOrganicGeometry.cs) produces room contours and passage points and widths.

[RiftSurface.cs](../../Assets/HELLSCRIPT/Runtime/Core/RiftSurface.cs) supplies the same polygon floor to movement, sight, projectiles and landing in [RiftNavigation.cs](../../Assets/HELLSCRIPT/Runtime/Core/RiftNavigation.cs), world rendering in [RiftFloorMesh.cs](../../Assets/HELLSCRIPT/Runtime/Presentation/RiftFloorMesh.cs), and map rendering in [RiftMinimap.cs](../../Assets/HELLSCRIPT/Runtime/Presentation/RiftMinimap.cs). Diagonal corridor bounding boxes do not become floor. Pathfinding validates movement between grid nodes as well as the nodes themselves.

A shallow rock lip follows exposed edges, excluding internal overlap boundaries. Generated meshes belong to their GameObjects’ lifetimes. Exploration concealment remains. [RiftBossPlacement.cs](../../Assets/HELLSCRIPT/Runtime/Core/RiftBossPlacement.cs) chooses nearby summon locations; [CombatSimulation.Rift.cs](../../Assets/HELLSCRIPT/Runtime/Core/CombatSimulation.Rift.cs) owns spawning and restoration.

## Saves and compatibility

New maps save version 6 and `roamingBoss`, room `outline` and `central`, and passage `widths` and `crossing`. Resume loads saved geometry rather than regenerating it from the seed. Versions 1–4 retain their rectangular floor rules; version 5 retains its saved curved floor. Existing arenas, gates and objective progression also remain intact. New rules apply to newly entered normal rifts.

Explicit `forcedObjective` or `arenaBoss` development calls can create version-5 arena/objective layouts for legacy regression tests and tools. Normal entry screens do not expose this option.

## Validation

All 21 final targeted Edit Mode checks passed in Unity 6000.6.0f1: nineteen checks for geometry, circulation, nearby summoning, fallbacks and room roles, plus encounter/reward budgets across 24 seeds and movement for six builds across three classes. Regression cases reject dead ends, loops joined by a single corridor, loops sharing only one room, and graphs without a complete room tour. All [33 localization checks](OrganicRiftEvidence/localization-results.xml) also passed. One obsolete source entry moved to the legacy section without changing any translation keys or values.

- Seeds 914001–914036 cover both themes and six/seven/eight perimeter rooms. Every map has an additional central room, a traversable X and a tour through every room back to the entrance, with no dead-end wings or dedicated boss arena. Fixed fallbacks: 0; aggregate room area reduction versus rectangular bounds: 16.45%.
- Checks cover 1.2 m passage clearance, spawn/chest reachability, exact saved-geometry round trips, version-4 restoration and movement/sight/projectile/landing rejection outside diagonal floors. A fixed contour reproduces the precision issue that formerly blocked short steps across shared floor edges.
- Room, doorway, junction and passage samples offer nearby summon space. Tests cover no summon at 99, one summon after the final meter kill, waiting/retrying around danger, saves before and after summoning, and duplicate prevention.
- The macOS development build succeeded with zero build errors. An actual process restart preserved map, health, time and RNG. At 1× speed, automatic exploration/combat opened 5 chests; meter 100 summoned a boss 10.00 m from the player, and the rift cleared in 141.05 seconds.
- The fixture is stage 1, seed 91367, a level-30 Warrior, eight rare item-level-30 pieces, a 30 m chest detour and the post-boss chest policy. It proves functional flow, not ordinary-account difficulty or clear rates.

Evidence: [six generated maps](OrganicRiftEvidence/generated-maps.png), [world rendering](OrganicRiftEvidence/world.png), [expanded map](OrganicRiftEvidence/full-map.png), [nearby boss summon](OrganicRiftEvidence/roaming-boss.png), and [clear screen](OrganicRiftEvidence/rift-result.png). The expanded map temporarily reveals exploration while paused, then restores the original discovery state. The summon screenshot also briefly pauses to refresh the UI.

Original records: [Edit Mode results](OrganicRiftEvidence/editmode-results.xml), [final movement checks](OrganicRiftEvidence/final-geometry-results.xml), [post-fix movement and budget checks](OrganicRiftEvidence/post-fix-flow-results.xml), [generation summary](OrganicRiftEvidence/generation-summary.txt), [sample map manifests](OrganicRiftEvidence/sample-layouts.json), [short-step regression contour](../../Assets/HELLSCRIPT/Tests/Editor/Fixtures/organic-seam.json), [runtime result](OrganicRiftEvidence/runtime-rift-smoke.txt), [summon location](OrganicRiftEvidence/boss-spawn.json), [saved map](OrganicRiftEvidence/layout-seed-91367.json), [validation conditions and source hashes](OrganicRiftEvidence/validation.json), [geometry tests](../../Assets/HELLSCRIPT/Tests/Editor/OrganicRiftTests.cs), and [summoning tests](../../Assets/HELLSCRIPT/Tests/Editor/RoamingBossTests.cs).

## Scope and remaining checks

No Unity packages, scenes, prefabs or existing image assets change. This is a geometry and progression improvement using the current art and combat systems. Physical mobile performance, the entire seed space and long-term balance remain separate validation tasks.

For new rift structure and boss entry, this record supersedes the previous rules in [dungeon composition](../Design/HELLSCRIPT_Dungeon_Composition_Detail.md) and [rift exploration](../Design/HELLSCRIPT_Rift_Exploration_Detail.md). Historical objective behaviour and earlier validation remain in [objective chains](Objective_Chains_Expansion.en.md) and [procedural rifts](Rift_Expansion.md).
