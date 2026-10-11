# Target selection panel and AI (2026-10-11)

The battle Hunt Edict sheet provides Priority, Chase distance/time, and Treasure goblin chase tabs. It shares the position panel's fixed size, safe area, landscape placement, footer and camera framing. Only the body scrolls. Rotation preserves the active tab and undo history. There is no new Canvas or persistence path.

Category chips drag horizontally; priority cards drag vertically. The shared order list reads actual card centers, preserves the grab offset, shifts other cards and updates world ranks from the unsaved draft. Release commits once through the existing transaction. Cancel, arrows and keyboard movement are supported. The full editor uses the same drag owner.

Priority replaces observed enemy health bars with rank badges and classification tags. The first target receives a gold dashed ring and attack arrow. Chase shows the actual metre radius and a demonstration countdown; the countdown does not advance saved combat. Goblin options control priority, flight path and all-out rays. Labels and lines are pooled. Closing or playing restores health bars, camera and graphs. No new raster assets are needed.

The HUD exposes Position, Target and Action order. Surround and Track remain in the full catalog, where every global option occurs exactly once. Callers derive the action group's index after filtering.

The comparator applies boss encounter policy, goblin priority, category order, each category's first matching item, squared distance and ID. Selection, higher-priority checks and caller-owned rank lists share it. Lowest HP means fraction, and Lowest HP and Nearest each identify one candidate with deterministic ties. Slow does not count as crowd control; DOT uses actual W09 and Poisoned effects. Dangerous casting describes the threat independently of the hero's available interrupts. A11 lures Idle enemies, which do not count as attacking the hero.

Class automatic skills first use global priority, then the same comparator over physically reachable candidates if the preferred target is out of range or behind a wall. Cast conditions remain enforced.

Goblin priority overrides Until death outside boss encounter overrides. Unlimited goblin chase is independent of priority and overrides Stop on lost sight. Search extrapolates only observed position and flight direction, never hidden bodies or flight goals. Death and successful escape end it. All-out applies only to the current goblin, relaxing ordinary reservations, ELITE/GROUP and use conditions while retaining emergency response, cooldowns, resources, Auto OFF, physical reach and opening WALK.

## Persistence v7

Immutable v1–v6 definitions and positional HED2 wire order remain unchanged. v7 removes six old target options and appends four priority permutations plus two goblin toggles; there are still 156 globals. Distance is 5–50m, default 30; time is 5–30s, default 20. Fresh priority/chase toggles are ON and all-out is OFF. Existing goblin priority values remain intact.

| Previous setting | Migration |
| --- | --- |
| Enabled BOSS / ELITE specials | Prefix Grade in their previous order |
| Enabled SUPPORT / RANGED | Prefix Type; expand RANGED to RANGED, CASTER |
| LOW_HP / NEAREST | Prefix the matching Status |
| Interrupt caster ON | Prefix CASTING ahead of LOW_HP/NEAREST |
| HIGH_HP / DENSE | Use the default Status order; no equivalent exists |
| Categories | GRADE, TYPE, STATUS |
| Chase bounds | Validate the old integer bounds, then clamp to the new bounds |
| Old finite goblin limits | Remove them and set unlimited goblin chase ON |

Remaining entries follow their default order. One canonical path upgrades heroes, legacyEdict, presets and HED2/HED3/HED4/HED5 without editing the input. Unknown fields, duplicate options, invalid permutations and invalid old values still fail. Frozen v1 channel policies migrate their own saved settings to v2 independently of current hero settings. Unlimited getters return infinity; serialized policies do not store it.

## Validation and retention

Focused tests cover classification, ties, physical fallback, goblin behavior, chase, frozen policies and save migration. Final integrated validation runs the full Edit Mode suite and native macOS development acceptance across KO/EN, portrait, landscape and three PC sizes. Results and actual screenshots will be recorded after these checks complete; pending checks are not passing evidence.

HIGH_HP/DENSE and old finite goblin chase are intentional lossy migrations. ATTACKING means a non-Idle action with a current hero sighting. Passive, recommended-edict and puzzle differences must be reported under matching conditions. The task artifact manifest records inputs, reports, logs, screenshots and build retention. macOS evidence does not establish physical-mobile or WebGL-player behavior. Permanent removal requires separate approval.
