# Combat positioning panel and movement AI (2026-10-11)

The battle HUD positioning group uses the reference's First Engagement and During Combat tabs. Opening choices are Rush In, Stay Put, Keep Distance and Lure to Chokepoint; combat choices are Pack Center, Stay Put, Keep Distance and Edge. Shared slots, fixed panel height and fixed navigation/undo/play leave only the description and sub-options scrollable when necessary.

## UI and persistence

This is an adapter in `GameUI.EdictSheet`, using `UiSafeArea`, `UiTheme`, `UiFonts`, the existing quick-edit Canvas and transaction. Landscape avoids the settings button. Rotation preserves the selected tab and undo history. Distances span 0–10 m in 0.5 m steps. Drag updates the displayed world radius; release saves through `SheetApply` and `GameStore.CommitHuntEdict`. Stay in my area is a combat-only toggle.

Global v6 appends `position.engageApproach=SKILL`, `position.engageDistanceSource=BASIC_RANGE`, `position.engageDistance=2` and `position.lureMode=NEARBY` without changing v5 option IDs or positional order. Documents and HED2/HED3/HED4/HED5 envelopes retain strict checks against their original version before migration. ORBIT becomes DISTANCE; the hidden clockwise value survives. Hero, legacyEdict and preset copies share canonical migration. Opening and combat distance choices are independent.

## Movement and previews

`ClassSkills.BasicReach` supplies Warrior 2 m and Ranger/Mage 10 m. Opening Walk blocks offensive Leap and support predictions based on Leap while preserving emergency evasions. Opening distance moves both inward and outward with a ±0.75 m deadband.

Chokepoints derive from narrow corridor widths, connected doors and junction entrances. Only explored candidates qualify, sorted by distance, x and y. Known selects the nearest; Scout uses unvisited-exit ranking until it discovers a candidate; Nearby holds position without a known candidate within 8 m. Arrival faces back toward the approach and holds. Incoming damage or the time cap ends the opening. Pack Center finds a legal gap near the observed centroid within 5 m of the target; Edge approaches the outside of the enemy farthest from that centroid. Existing safety, collision, path and transaction checks remain active.

The expected marker ends at the detached simulation's `ForecastHeroPath` endpoint. Reused lines, rings, sprites and shared world labels show the gold dashed expected ring, hero distance ring/range label, known chokepoint or scouting direction, nearby search boundary, and the red observed-pack ring. Close and Play hide previews.

The four supplied PNGs are registered byte for byte. The [asset manifest](../Art/EdictPosition/edict-position-icons-manifest.json) retains hashes, alpha metrics, unknown model provenance and `productionApproved: false`. Mobile uses 128 px ASTC 4×4 and desktop 256 px. Faint alpha dots are preserved for visual review.

## Behavior differences and provisional constants

Default ORBIT becomes fixed 2 m DISTANCE. A stationary target no longer produces continuous circling after the desired distance is reached, changing movement, firing angles, afterimage accumulation and associated passive measurements. The stored `mobile-crossfire` preset ID survives; its label and description now describe 4 m distance keeping and effects triggered by actual distance-adjustment movement.

The planned provisional values are named constants: narrow width 4 m, nearby radius 8 m, opening cap 10 s. Review them with authored chokepoint metadata and corresponding regressions when the owner confirms new values. A saved 0 m distance retains normal actor collision clearance.


Opening Approach stops at basic attack reach independently of combat distance. Stand does not approach an out-of-range target; Edge holds an outer gap rather than circling. Skill regression fixtures and winning puzzle inputs now explicitly select approach/evasion where required. The previous assumption that Edge alone continuously avoids dangerous attacks no longer describes the new mode.

The paired measurement uses level 40, rank 1 class skills, item level 30, no set/legendary gear, a group scenario, 20 seconds and two seeds. Each of three representative builds is measured with all passives and with each of three passives removed: 24 samples before and 24 after.

| Default recommended build | Before mean damage | After mean damage | Change | Survival |
| --- | ---: | ---: | ---: | --- |
| Warrior `BUILD_REF_SW01` | 9,184.2 | 9,509.1 | +3.5% | 2/2 → 2/2 |
| Ranger `BUILD_REF_SA01` | 1,858.6 | 1,852.2 | −0.3% | 2/2 → 2/2 |
| Mage `BUILD_REF_SM03` | 13,882.7 | 13,462.2 | −3.0% | 2/2 → 2/2 |

Paired damage differences against removing a passive are AP09 54.5→60.8, MP01 2,364.5→2,350.6 and MP14 −70.0→−207.9. These depend on positioning and enemy motion; they are not a general balance verdict. Raw [before](EdictPositionEvidence20261011/passive-before.json) and [after](EdictPositionEvidence20261011/passive-after.json) results are retained.

Final-validation corrections reuse the shared owners: BASIC_RANGE combat movement closes inside actual attack reach, legacy simplification imports pass through strict versioned migration, the guide explicitly uses range distance, and Warrior puzzle level 7 explicitly uses pack center. Keyboard selection survives saves and rotation. The PREVENT shield example uses an imminent ranged attack; an early ground telegraph can correctly trigger walking before damage enters shield lookahead.

With the new center movement, puzzle level 15 can clear with one omitted hazard: Warrior without direct dodge takes 526 cumulative HP loss (780 max HP), and Mage without area dodge takes 306 (390 max HP). Their corresponding all-dodge samples take zero HP loss. Tests verify these actual costs and terminal outcomes rather than assuming every omitted single policy necessarily kills the hero.

## Validation and artifact lifecycle

The merged-main full Edit Mode run executed once: 5,742 cases, 5,722 passed, 16 failed and four explicitly skipped. After repairing the connected regressions, all 419 failed/directly affected cases and new regressions passed, including every original failure. Unchanged passing coverage was reused. The 24 passive samples reproduced the same results on the final combat source. Fifteen shared UI contract regressions and static refresh checks (14 bindings/176 sources) pass.

The native macOS development player normally resumed an isolated saved hunt. The final position preview passed all ten KO/EN matrices at 440×956, 956×440, 1600×900, 1600×1000 and 2100×900: persistence, undo, rotation, keyboard focus, fixed geometry, visible hero/endpoint/labels and play/close restoration (exit 0, 31 captures). Actual sheet, graph and top/bottom HUD occlusion was corrected with the existing world camera's viewport and bounds. Floating graphs are hidden only during the preview, with the normal camera/graphs restored on play, close, dimming or page changes.

The previously failed outside tap and remaining four-view panels passed independently (exit 0, 13 captures). Shared Play/Pause captions are distinct from the Renewal equipment affix; two English views passed the caption regression (exit 0, five captures). Unchanged passing potion matrices and skill/potion equip coverage were reused. [Validation summary and source hashes](EdictPositionEvidence20261011/validation-summary.json) record the scope. Physical iOS/Android, the WebGL game player and production live-ops connectivity were not validated.

Actual development player captures:

![KO portrait](EdictPositionEvidence20261011/ko-portrait-distance.png)
![KO landscape](EdictPositionEvidence20261011/ko-landscape-distance.png)
![EN portrait](EdictPositionEvidence20261011/en-portrait-distance.png)
![EN landscape](EdictPositionEvidence20261011/en-landscape-distance.png)
![KO desktop](EdictPositionEvidence20261011/pc-ko-distance.png)

Task outputs are recorded in `validation-codex/artifact-lifecycle.json`. Preserve source, original icons, reports and captures; permanent removal requires approval.
