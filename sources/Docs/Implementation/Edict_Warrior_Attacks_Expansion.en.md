# Warrior Whirlwind, leap and Crushing Blow hunt edicts

Date: September 13, 2026 · [한국어](Edict_Warrior_Attacks_Expansion.md)

Status: **All 13 W01-W03 core options are connected. All 1,847 tests and 11 native processes passed.**

## Scope and baseline

Claude's 57 attributes and 54 affixes remain on main through integration `92d5812`. This phase starts at Mage BASIC revision `47294f4` and independently connects all 13 W01-W03 core options. Follow the [skill catalog](../Design/HELLSCRIPT_Hunt_Edict_Skill_Option_Catalog.md) and [screen layout specification](../Design/HELLSCRIPT_Screen_Layout_Detail.md). W04-W06 and Warrior BASIC remain separate follow-up work.

Executable candidates are derived from the document; missing, duplicate, disabled or conditional legacy rows cannot silently override it. Saved legacy rules are preserved and remain active when edict mode is off. Damage coefficients, affix weights and cooldowns are unchanged.

## Whirlwind

- Start conditions use the actual 2.5 m self area: one enemy, at least two, or an elite/boss. They apply only at start; losing one group member does not end the spin.
- Starting costs nothing. Each 0.25 s tick pays its actual cost, retaining base cost six, SW2, WP02 after two seconds, LC02 and the existing 50% reduction cap. LC02 is consumed only on the first paid tick.
- Three-second mode completes the normal twelfth tick before ending and reevaluating all actions. Earlier resource, empty-area, settings and survival stops still apply.
- Global positioning, following, outer-group circling and standing use valid routes and global risk/pursuit limits. Standing and movement before the first paid tick cannot manufacture SW4 walking charge.
- Charge linking requires real SW4, W02 and a live charge. It ignores only the current channel lock while checking leap role, minimum distance, cooldown and landing eligibility. Failure keeps spinning; success ends normally before a fresh decision. It reserves neither the next action nor the charge.

## Leap, landing preference and Crushing Blow

The attack minimum measures distance to the target, while actual travel remains 2-8 m. Valid terrain, line of sight and global risk are checked. Target/edge placement includes the preferred enemy in the base 2.5 m landing hit; dense placement maximizes enemies in that base area. Escape-only leaps cannot engage, and emergency-reserved leaps remain unavailable for ordinary attacks. Emergency escape ignores the attack minimum.

Leap costs zero and has an eight-second base cooldown. SW4 is reserved at actual takeoff after 0.15 s preparation, not at selection. Invalid takeoff preserves the charge. Settings changes and restarts during preparation/flight retain the committed landing point and preference; airborne main actions cannot be replaced.

The landing preference affects one subsequent ordinary attack. An unavailable W01/W03 does not cause waiting or prevent the leap itself. It cannot preempt earlier global survival, dodge, collection or shout actions. A save immediately after landing retains the preference; choosing the next ordinary attack consumes it.

W03 selects the configured perceived target, including strict lowest HP fraction, and keeps it inside the forward 100-degree, 3 m cone. It searches for multiple hits or skips an impossible isolated shot. Actual SWB4 charge and LC02 discount are consumed at cast start, without refunds on cancellation or misses. WP03, reserved SW4 landing damage and SWB4 charge after a real W02 base hit use the existing effect owners.

## Persistence and UI corrections

Unity may deserialize an absent optional landing preference as an empty object. Treat it as absent instead of rejecting old saves; unsupported future versions remain rejected. Channel elapsed time, next tick, paid cost, walking distance and charges persist. An identical refresh is distinguished from a real settings change.

Native screenshots exposed stale English travel/finishing text after switching back to Korean. HUD and combat details now describe the current action phase in the selected language instead of reusing stored display text. Preparation, flight, channeling and recovery support both directions without language selection mutating combat state. Completion also sets a completion message so an idle interval cannot retain the preceding language. English names match the established `Leap Slam` and `Crushing Blow` entries.

## Validation

| Check | Final result |
|---|---|
| Full Unity Editor suite | **1,847 passed**, zero failures/skips; 2026-09-12 20:38:12Z to 2026-09-12 20:43:04Z UTC, 292.25 seconds. |
| Warrior coverage | **82 cases**, covering independent candidates, geometry, actual cost/charges, emergency roles, landing preference, persistence and language switching. |
| Native Warrior | **Five processes passed**, including four independent restarts. |
| Mage regression on the same app | Three BASIC and three Nova processes passed, checking shared persistence, movement and action display. |
| Screens | **41 screenshots**; the 15 Warrior images were visually reviewed. Korean/English at 1280×720, 720×1280 and 640×360, including 140% text. All 41 passed their harness text/footer/translation checks; this is not every size/language combination or physical mobile QA. |
| Source/assets | Compare 261 C# files plus English strings with the final test copy; 184 runtime C# files plus strings with the player build copy. No missing C# metas or duplicate GUIDs; preserve 17 unrelated changes. |

Use isolated Unity projects. Preserve the initial empty-object restore failure, ambiguous harness-button match and pre/post language evidence. Final evidence is `Full4`, `Build4`, `Native4` and paired `Before12`/`AfterRelease12`. All 12 diagnostic rows also remained identical across the final display correction.

Native evidence uses a controlled training fixture and dedicated save directory, with four independent restarts:

1. Save a paid channel and partial real walking progress, then resume to earn an actual SW4 charge.
2. Save leap preparation before charge reservation.
3. Reserve at takeoff, change the preference through the real UI, and save during flight.
4. Resume the same W02/SW4 landing and WP03 defense once, then save the pending original preference.
5. Restore that W03 preference, select it for one ordinary attack, pay 25 resource and record the actual hit.

Actual SWB4 landing-hit/charge/cast consumption and a subsequent miss are covered in Editor tests, not represented as native coverage of every item combination. The first native attempt stopped because the harness matched both an option heading and another button's selected value; selection now matches the heading. Initial failed evidence remains available.

## Paired generated-rift observations

Compare two recommended Warrior builds, two seeds and three separate modes: 12 runs per revision, 24 total. Both revisions use level-30 heroes, eight legal item-level-30 pieces and matched stage-20 maps, initial enemies, gear and sheets. Modes are legacy rules, default core policies and explicit charge linking. The linked Whirlwind build selects charge-end plus W01 after landing; the other build selects actual crush charge plus W03 after landing. Global survival retains its defaults; no invented recommended survival setup is substituted.

| Mode | Before: clear/death/timeout | After: clear/death/timeout | Observation |
|---|---|---|---|
| Legacy | 1/0/3 | 1/0/3 | All four complete rows are identical. |
| Core default | 2/0/2 | 2/1/1 | SW4 takeoff reservations 0→4; real SWB4 charges 6→29. |
| Explicit linking | 2/2/0 | 2/2/0 | Normal Whirlwind ends 0→3; SW4 reservations 0→5. |

Clear counts are unchanged and default mode introduces one death. Channel interruptions increase from 36 to 50 in default mode and from 36 to 59 with explicit linking. These are functional observations, not difficulty or class-ranking conclusions. Remaining work includes W04-W06/BASIC, survival and interruption causes, natural progression/farming value and physical mobile validation.

Keep the [validation summary](../../Artifacts/Validation/EdictWarriorAttacks/validation-summary.json), [paired comparison](../../Artifacts/Validation/EdictWarriorAttacks/comparison.json), [validated source](../../Artifacts/Validation/EdictWarriorAttacks/validated-source.json) and [macOS app](../../Builds/macOS-EdictWarriorAttacks/HELLSCRIPT.app). Scenes, prefabs, packages and 17 unrelated uncommitted changes are preserved.
