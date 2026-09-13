# Temporary audio and minimal skill presentation

Date: 2026-09-13 · [한국어](Audio_Skill_Presentation.md)

Following the owner's priority of finishing gameplay, this batch fills the missing audio and makes skill shapes recognizable with primitives, lines and short body/weapon motions. It creates no new image assets, production models or animation packs. Combat numbers and reward randomness remain unchanged.

## Audio and settings

Temporary cues now cover UI selection/confirmation, class attack preparation/release/impact, critical hits, player damage, movement, protection, enemy danger warnings, boss entry, chests, ordinary and legendary/set loot, deaths, victory and failure. Sanctuary and rift use different 16-second ambient loops with a volume crossfade.

All temporary audio is original synthesis in project code, with no third-party songs, recordings or samples. Production audio replacement remains future work.

The **Audio** tab in Settings provides master, music and effects volume, mute, effect preview and defaults. Changes apply immediately and save to a device-only file independently of characters and active runs. Failed saves can be retried. Up to eight effects play concurrently; danger warnings and major rewards take priority. Playback speed never changes pitch. App suspension pauses audio and saves preferences. Resuming a saved run does not replay its historical sounds, and muting preserves visual danger markers and notices.

## Skill shapes

Views follow actual positions, projectiles, areas and remaining status durations. Presentation geometry creates no additional damage or collision rules.

| Skill | Minimal presentation |
| --- | --- |
| W01 Whirlwind | Spinning body and sweeping arc |
| W02 Leap Slam | Existing airborne leap plus landing shockwave |
| W03 Crush | Raised weapon, swing and forward 100-degree sector |
| W04 Ground Slam | Outward ring and short ground shards |
| W05 Iron Wall | Wire shield while shield health remains |
| W06 Battle Shout | Outward rings and active buff ring |
| A01 Pierce | Elongated arrow with short trail, following its actual direction |
| A02 Multishot | Three arrows following the actual spread |
| A03 Poison Trap | Green core and range ring; heavier border after triggering |
| A04 Retreat | Existing backward leap and small takeoff/landing rings |
| A05 Hunter's Mark | Purple diamond above the actually marked enemy |
| A06 Shadow Arrow | Orbiting orbs matching remaining charges; purple empowered arrows |
| M01 Fireball | Orange orb and trail, explosion at the actual impact point |
| M02 Blizzard | Small rotating frost pieces following the actual area |
| M03 Chain Lightning | Brief jagged lines between actual hit positions |
| M04 Teleport | Departure/arrival rings and a short arrival flash |
| M05 Elemental Shield | Blue wire shield that follows remaining protection |
| M06 Frost Nova | Outward blue ring and frost shards |

Basic attacks also distinguish a melee swing, arrow and magic bolt. Preparation and recovery add a short body/weapon lean. Pausing freezes movement and effects; returning from dimmed presentation shows current combat state. Persistent views end with their underlying effects or when leaving the field.

## Checks and next priorities

Compilation, the macOS build, volume controls/restart and short play with all three classes passed. The initial oversized slider handle was corrected and checked in the final build. This batch performs no large regression suite or repeated balance experiment. Balance tuning, production audio/art and physical mobile audio/touch checks remain later work.

Evidence: [runtime summary](../../Artifacts/Validation/AudioSkills/validation-summary.json).

![Audio settings](../../Artifacts/Validation/AudioSkills/Native2/02-audio-en.png)
