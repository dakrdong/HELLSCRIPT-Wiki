# Forest settlement and title entry flow

Date: 2026-09-14

[한국어](Forest_Settlement.md)

## Purpose and entry flow

The town is a settlers' refuge in a dark conifer forest. Timber and plaster buildings, stone foundations, pitched roofs, warm windows and lanterns, supply crates, a well and outdoor training targets distinguish its services. The forest-settlement mood draws on the user's Diablo IV reference without importing that game's images or models. Buildings, residents and the portal use replaceable, project-native 3D geometry. The hero shares the existing game model.

Boot → title → select character → **Enter town** → arrive in the village. The title does not create movement state. Character selection uses the existing three characters and local save. A suspended rift keeps its owning character selected and can be resumed at the village portal.

Move with the circular joystick in the lower left. Releasing it stops movement; a second finger cannot steal pointer ownership. Settings, the destination guide and loss of application focus reset input. Desktop also supports WASD, arrow keys and E to interact. Ground taps and destination selection provide optional walking routes around buildings.

The user's September 14 instruction replaces the older **no town joystick** and **automatically open on arrival** rules. Automatic combat behavior remains unchanged.

## Layout and scale

Coordinates follow the fixed camera: +X is screen-right, +Y is farther into the scene. Camera rotation is fixed. The main east-west road spans 110 m, taking about 20 seconds at 5.5 m/s. Walkable bounds are 110 × 70 m. Detours around buildings can take longer.

| Service | NPC or target position | Placement and behavior |
|---|---|---|
| Warehouse | (-37, 13) | Keeper in front of a warehouse with crates and barrels; opens shared equipment storage. |
| Equipment merchant | (-14, 16) | Awning and equipment display; basic purchases, equipment sales and the existing unidentified shop. |
| Blacksmith | (14, 17) | Chimney, hearth and anvil; equipment selection, enhancement, affix rerolling and crafting. |
| Training | (37, 9) | Instructor in front of an outdoor target yard; training and build comparison are available before the first rift. |
| Gem merchant | (-32, -24) | Gem display; tier 1 purchases and existing socket/storage services. |
| Rune merchant | (21, -24) | Rune-block display; basic G0 purchases and the existing rune board. |
| Rift portal | (8, 0) | Slightly right of the village center; orange/gold rings, rotating sigils, sparks, a transparent veil and warm light. Opens stage selection and rift entry. |

Residents stand in front of the camera-facing side of their buildings. Navigation and manual collision share building footprints. Movement substeps prevent wall tunnelling during long frames. Diagonal input cannot exceed normal walking speed.

## Interaction and occlusion

Within 2.8 m, a resident's speech label and fixed right-hand action card identify the service. Pressing the action button explicitly opens it; proximity alone never opens a page or spends currency. The closest eligible target wins. The card stays 22 UI units from the safe area's right edge, at 35% of its height from the bottom, in both portrait and landscape.

Parallel orthographic rays toward the hero's torso and head detect obstructing buildings. Walls, roof and attached props fade together to 18% opacity. Leaving the obstruction restores opacity and shadows. Fading changes rendering only; building collision remains active.

## Purchases and progression

New shops sell basic, guaranteed stock. Existing high-tier drops, unidentified purchases, crafting unlocks, gem fusion and rune progression retain their own rules. Prices are initial tuning values.

| Stock | Price | Destination |
|---|---|---|
| Common equipment | 140 + item level × 40 gold | Selected character's bag |
| Seven tier 1 gem types | 350 gold each | Shared gem storage |
| G0 1 / 2 / 3-cell blocks | 300 / 1,200 / 2,700 gold | Shared rune inventory |

Equipment level follows the selected hero's highest actual clear, with a minimum of 1 and the existing item-level maximum. Purchases show their item and price before a confirmation dialog. Suspended rifts, insufficient currency, full storage and invalid stock prevent transactions. Existing staged-save transactions preserve both memory and disk on failed writes. Replaying the same request cannot award goods or charge twice. Buying a first gem qualifies as the existing legitimate gem acquisition unlock.

## Ownership and validation

This extends the existing `GameController → GameUI / WorldView → TownWalk` ownership chain. No new package, scene replacement or prefab replacement is required. Movement remains session-only; the character save schema is unchanged.

Validated on September 14, 2026 with Unity 6000.6.0f1. All 79 focused Edit Mode checks passed, and the macOS development build succeeded with zero errors. A separate test save verified title gating and character selection; joystick center, motion, release, pointer ownership and disable reset; all seven services with explicit interaction; portrait, landscape and 130% UI scale; opaque buildings in front, fading behind and restoration; equipment/gem/rune purchase confirmation; fresh-account training; and real rift entry. The native run ended with `HELLSCRIPT_FOREST_TOWN_RUNTIME_OK`.

Thirteen existing failures discovered during the full-suite run were reproduced with the same results in a separate checkout of baseline commit `1ad2d84`. They concern reward expectations and save-schema expectations and remain outside this change. The full project test suite is therefore not reported as passing. Preserved evidence includes the [validation summary](ForestTownEvidence/validation.json), [focused test results](ForestTownEvidence/editmode-results.xml), [runtime result](ForestTownEvidence/runtime-result.txt), and [baseline failures](ForestTownEvidence/baseline-failures.json).

Village art currently uses replaceable basic 3D meshes. Production model and texture refinement, physical iOS/Android touch feel, thermals and frame performance remain follow-up work.

## Runtime screenshots

- [Village overview](ForestTownEvidence/forest-town-overview.png)

- [Title with character selection and entry](ForestTownEvidence/forest-town-title.png)

- [Merchant speech label and landscape interaction button](ForestTownEvidence/forest-town-landscape.png)

- [Portrait interaction in English](ForestTownEvidence/forest-town-portrait-en.png)

- [Building transparency reveals the hero behind it](ForestTownEvidence/forest-town-occlusion.png)
