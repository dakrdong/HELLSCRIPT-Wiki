# Physical rift gate passage

Date: September 12, 2026 · Previous stage: [targeting and pursuit](Edict_Targeting_Expansion.en.md)

The follow-up [essence carriers and offerings](Objective_Chains_Expansion.en.md) implements both objective flows, persistence, visuals and objective/timing records in results. The scope and remaining work below describe the earlier gate-passage stage.

## Scope

Connect the gates in sections 6, 8, 10 and 11 of the [dungeon composition specification](../Design/HELLSCRIPT_Dungeon_Composition_Detail.md) to movement, perception and persistence. Previously, a closed gate only removed the boss arena from exploration candidates; the passages stayed open. Four tests against the previous implementation failed movement, sight, projectile and closed-arena reachability checks.

Claude's separate attribute, growth and combat-balance work remains excluded. The existing two seals, 1.5 combat-second breaking duration, 100-meter gate alternative and current objective-path generation threshold are unchanged. Gate passage is validated before adding essence carriers and offerings.

## Implementation

- New objective maps save one gate at every connected boss-arena entrance, covering the other side of the ring and additional links.
- Closed gates block walking, sight, projectiles and landing on the barrier. Non-arena rooms, seal access points and enough enemies to fill the meter remain accessible.
- Completing the seals or reaching 100 meter applies the same open state to navigation. Cached failed paths are discarded so the newly opened passage can be used immediately.
- Existing version 1 and 2 maps retain their original passage geometry. Loading does not insert new obstacles around previously saved positions. Newly generated maps use version 3.
- Generation checks validate both phases: objectives and the meter alternative before opening; every encounter and boss position after opening.
- The runtime model uses original primitive pillars, an iron-bound door and a status light. The closed door has a purple light; opening removes its blocking slab and turns the light green. Only discovered gates appear in the world and minimap. Drawing never discovers content.
- The actual closed-to-open transition emits one notice. Loading an open checkpoint or redrawing the screen does not replay it. The map has Korean and English gate status and legend text.

## Validation

Validated with Unity 6000.6.0f1. The latest player is `Builds/macOS-GatePassage/HELLSCRIPT.app`; evidence is under `Artifacts/Validation/GatePassage`.

| Check | Result and evidence |
|---|---|
| Reproduction | All four new tests failed against the old implementation. `before.xml` |
| Initial correction | All 44 gate, rift, seal and field checks passed. `first-fixed.xml` |
| Full Editor suite | **1,261/1,261 passed**, no failures or skips. September 12, 09:56:53–10:00:44 UTC. `full.xml` |
| New gate cases | 18 cases covering 100 maps across both themes and 6/7/8 rooms, all six fixed fallbacks, closed/open passage, both opening conditions, one notice, old saves and discovery. |
| Final build | Successful, zero build errors and normal exit. `build.log` |
| Native processes | Initial launch and two separate restarts all passed with an isolated account. `native-results.json` |
| Screens | 12 artifacts across Korean/English and 1920×1080, 1280×720, 720×1280 and 640×360. This is not every language/size combination. |

The native sequence saved a closed gate, restored it closed, opened it through the meter, saved it open, then restarted again and automatically entered the arena. The first restart's opening sequence matched a control simulation with the same inputs. Resizing and map redraw preserved gate, exploration and combat state, including the original pause state on return.

[Closed gate](../../Artifacts/Validation/GatePassage/Evidence/02-closed-gate-wide.png) · [Open gate](../../Artifacts/Validation/GatePassage/Evidence/02-opened-wide.png) · [Arena entry after restart](../../Artifacts/Validation/GatePassage/Evidence/02-open-resume-arena-portrait.png)

Only the native harness `RuntimeGateSmoke.cs` changed after the full suite, to wait for discovery after crossing the threshold and add compact-window captures. All production sources still match the passing suite. The final build and native runs include those harness edits. Both snapshots are retained in `tested-source-manifest.json` and `final-source-manifest.json`.

Audited 200 C# files and preserved all 296 baseline metadata, scenes, serialized assets, settings, package and design files. Four new meta files belong to this phase; there are no duplicate GUIDs or missing C# metas. See `validation-summary.json`.

## Remaining scope

Essence carriers, offerings, and objective type/timing in the results screen remain separate work. These native scenarios isolate passage behavior; they do not validate physical mobile touch, sustained performance or full-game balance. This record does not claim completion of all three objectives or the full game.
