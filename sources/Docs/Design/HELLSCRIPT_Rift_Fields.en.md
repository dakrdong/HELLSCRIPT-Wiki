# Six Rift Fields

Updated: 2026-09-28 · [한국어](HELLSCRIPT_Rift_Fields.md)

Each time the hero enters a rift, one field is chosen. The field decides the room template set, the regular enemy lineup, the name in the battle heading, and the lighting, fog and post-processing mood. The existing Forgotten Graveyard and Ruined Fortress are joined by Scorched Desert, Dank Cavern, Dusk Grassland and Snowbound Highland, six in total. Enemy behaviour follows the [regular enemy and elite detail](HELLSCRIPT_Enemy_Combat_Detail.md) and [content catalog §11](HELLSCRIPT_Content_Catalog.md#11-일반-적-20종); room composition follows the [dungeon composition detail](HELLSCRIPT_Dungeon_Composition_Detail.md).

## Selection rule

- The room template set (the former "theme") is chosen exactly as before, from the rift seed's choice stream: only the graveyard set up to stage 10, and the fortress set too from stage 11.
- The field is drawn evenly from the three fields of that set with a separate stream, `Derive(seed,"field")`. The graveyard set holds Forgotten Graveyard, Dank Cavern and Dusk Grassland; the fortress set holds Ruined Fortress, Scorched Desert and Snowbound Highland.
- The same seed always gives the same field. Choosing the field changes no room layout, map fingerprint, pack placement or combat random number, so the server replay gives the same result.
- Stages 1–10 offer Forgotten Graveyard, Dank Cavern and Dusk Grassland; from stage 11 all six fields appear.
- The tutorial rift is always Forgotten Graveyard. Training and the developer ring map use the set number as the field.
- A rift saved before fields existed has no field value and uses its set number as the field (`field=-1` → `Field=theme`).

## Fields and lineups

Slots are pack roles: 0 melee (most of a pack, and every elite), 1 charger, 2 ranged, 3 area, 4 support, 5 death burst. Up to stage 5 only slot 0 appears.

| Field | Room set | From | Mood | 0 melee | 1 charger | 2 ranged | 3 area | 4 support | 5 burst |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Forgotten Graveyard | graveyard | stage 1 | wet flagstones and moss, pale moonlight, teal fog | N01 Chained Corpse | N02 Grave Hound | N03 Skeleton Archer | N04 Plague Servant | N05 Funeral Priest | N06 Bloated Pilgrim |
| Ruined Fortress | fortress | stage 11 | charred granite and braziers, smoky orange fog | N07 Ironclad Wraith | N08 Fanatic Charger | N09 Blackiron Crossbowman | N10 Emberfire Sorcerer | N11 Bellringer | N12 Rift Shardling |
| Scorched Desert | fortress | stage 11 | sandstone half buried in sand, low amber sun | N13 Dune Marauder | N14 Burrowing Sandworm | N03 Skeleton Archer | N10 Emberfire Sorcerer | N11 Bellringer | N06 Bloated Pilgrim |
| Dank Cavern | graveyard | stage 1 | wet black rock and puddles, cyan fungal light | N15 Cave Ghoul | N02 Grave Hound | N03 Skeleton Archer | N04 Plague Servant | N05 Funeral Priest | N16 Sporebloat |
| Dusk Grassland | graveyard | stage 1 | trampled grass and standing stones, rose dusk and violet fog | N17 Thornhorn Brute | N02 Grave Hound | N18 Briar Witch | N10 Emberfire Sorcerer | N05 Funeral Priest | N12 Rift Shardling |
| Snowbound Highland | fortress | stage 11 | snow-packed stone and ice, cold white sun | N19 Frostbitten Revenant | N08 Fanatic Charger | N09 Blackiron Crossbowman | N20 Rime Caller | N11 Bellringer | N12 Rift Shardling |

Seal waves and cursed-chest echoes pick their enemies from the same table. Boss adds follow each boss's own rule (Chorister N01, Dune Tyrant N13, Hoarfrost Matriarch N19).

## Eight new regular enemies

N13–N20 fight with their own attacks (2026-09-28). Every attack lands on the tick its warning gauge is full; tunnels and leaps land on the tick they arrive. Their sounds and dedicated models come in the next stage.

| ID | Name | Field | Role | HP / attack multiplier / speed | Attack |
| --- | --- | --- | --- | --- | --- |
| N13 | Dune Marauder | Scorched Desert | melee | 1 / 1 / 2.7 | After a 0.65 s warning, cuts a 2.3 m, 110° arc and cuts again 0.35 s later (0.6× each, 1.8 s cooldown). |
| N14 | Burrowing Sandworm | Scorched Desert | charger | 0.8 / 1.1 / 3.0 | Warns a 2.2 m circle at the hero's position within 8 m for 0.9 s, tunnels there at 12 m/s and erupts (1.4×, 6 s cooldown). |
| N15 | Cave Ghoul | Dank Cavern | melee | 1 / 1 / 2.7 | Swings like N01 within 2.5 m; from 2.5–4.5 m it warns for 0.7 s, leaps for 0.45 s to 1 m short of the hero and hits a 1.6 m radius (0.9×, 5 s cooldown). |
| N16 | Sporebloat | Dank Cavern | death burst | 1.1 / 0.6 / 2.0 | Swings like N01; 0.6 s after death, leaves a 2.6 m poison spore cloud for 4 s (0.25× every 0.5 s) that slows the hero. |
| N17 | Thornhorn Brute | Dusk Grassland | melee | 1 / 1 / 2.7 | After a 0.9 s warning, whirls through a 2.1 m radius around itself (1.1×, 1.65 s cooldown). |
| N18 | Briar Witch | Dusk Grassland | ranged | 0.75 / 0.85 / 2.3 | From 9 m, warns a 9 m × 0.7 m thorn lance for 0.8 s, then raises it (0.85×) and slows the hero for 1.5 s (3 s cooldown). |
| N19 | Frostbitten Revenant | Snowbound Highland | melee | 1 / 1 / 2.7 | After a 0.65 s warning, a 2.4 m, 120° cold cleave (1.0×) that slows the hero for 1.5 s (1.6 s cooldown). |
| N20 | Rime Caller | Snowbound Highland | area | 0.8 / 1.0 / 2.3 | Warns a frost ring from 1.6 m to 3.6 m for 1.4 s, centred 2.6 m from the hero toward the caster, then deals cold damage (1.3×) and slows for 2 s (6 s cooldown). |

The new melee-slot enemies (N13, N15, N17, N19) use N01's stat budget, and their cooldowns keep damage per second within ±15% of N01 (checked by `NewEnemyAttackTests`).

## Current presentation

- The battle heading shows the field name (for example `Scorched Desert · Fill the kill gauge to summon the boss`).
- Each field applies its own lighting, fog and post-processing preset, and tints the existing rift stone floor.
- Field-specific floor, wall, obstacle and decoration models and the monster models are being built with the procedural art tools and will be wired into the view in the next stage.

## Verification

`FieldTests` checks that a seed always gives the same field, the field distribution per set over 300 seeds, the stage 1–10 restriction, unchanged themes, rooms and fingerprints for existing seeds, how old saves resolve the field, the lineup per field, and that the new enemies' temporary melee path agrees between the simulation, the threat display and the damage forecast. The server replay (`AuthoritativeRiftTests`) and the natural stage-1 runs must pass as well.
