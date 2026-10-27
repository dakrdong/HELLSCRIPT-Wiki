# Hunt edict "pack centre" movement — fixing the hero fighting in front of the pack (2026-10-24)

[한국어](Edict_Pack_Center_20261024.md). User test (main `a045c626`) report 1: "In the combat-position tutorial, after choosing 'pack centre' the hero should fight from the centre of the enemies judged to be the pack. Now he fights in front of it. Fix the 'pack centre' feature, not a hard-coded tutorial. Pack centre → attack, by priority, the enemy nearest the centre."

## Cause (confirmed by reproduction)

The three hypotheses (the pack being defined as 5 m around the target, target priority, the `reach` condition) were not the cause. The goal geometry (`EdictPackCenterGoal`) was correct: in the tutorial gallery its goal always pointed 2.5 m ahead at a legal point.

The real cause is **the attack animation hiding the walk**. Decisions run every 0.2 s before movement, and one basic attack keeps the hero busy for about a second (0.4 prepare + 0.6 recovery; `MoveHeroTowardGoal` returns while he is busy). As soon as any enemy was in reach the next attack started on the same tick, so the hero walked only "until the first enemy was in reach" and fought from there. The pack centre was a goal during the approach only.

Measured (`PackCenterProbe`, same simulation, 0.05 s ticks):

| Situation | Before: strike position (from the pack centre) | After |
| --- | --- | --- |
| Six still enemies, Warrior, 20 s | all 19 hits from 4.4 m, 3.8 m walked, busy 380/400 ticks | median 1.2 m (only the first approach hit from 4.4 m), 7.0 m walked |
| Same pack, Mage | 8.4 m, never moved (busy 400/400 ticks) | median 1.2 m, 7.2 m walked |
| Same pack, Ranger | 0.7–8.4 m mixed | median 0.7 m |
| Rooted Warrior (4 enemies in reach) | - | no walking, 5 hits in 6 s (attacks are not held back) |
| Tutorial gallery, 3 archers | median 1.7 m (hit the first archer from 2.98 m for about 8 s) | 11 hits, 0.5–0.9 m (median 0.9 m), 2.6 m walked |

## Rule (a global feature fix, no tutorial-specific code)

1. **Walk first, then strike.** With `position.mode == CENTER` (including a rule's movement override), `PackCentreWalk` checks the centre goal just before a basic attack starts (`StartHeroAction`, `index<0`). If the legal stand point nearest the centre is farther than 0.5 m the attack waits one decision (0.2 s) and the hero walks along the existing movement path on the same tick. On arrival he attacks.
2. **Never stand still.** A cycle that brought him less than 0.2 m closer (rooted, blocked, a pack backing off) releases the attack. A second call within the same decision counts once.
3. **Skills are not delayed.** Skills keep their own positioning, leaps and channel movement (Whirlwind moves toward the same centre goal while channelling). The first-engagement approach (`EngagementOpening`) is untouched, so the first hit lands where he approached to and he walks to the centre from then on.
4. **The pack definition is unchanged**: observed enemies within 5 m of the selected target, centre = the legal stand point nearest their centroid. It now lives in one place, `EdictPack`, shared by the centre, the edge and the HUD preview (the copy in `EdictForecast` is gone).
5. **Target: priority first, among equals the enemy nearest the pack centre.** `EdictTargetPolicy.Select` gained an `origin`. With `CENTER`, and only once the hero stands within 3.2 m of the centroid (a legal stand point lies at most 2.7 m from it), rule and class-skill target selection measures from the centroid of the pack around the current target (`PackTargetOrigin`); the `NEAREST` status uses the same origin. While he is still walking in, or cannot walk, he measures from himself as before, so an enemy in reach is never passed over for one he cannot hit. The HUD priority list and the other position modes stay hero-based. Keeping a target until death (`UNTIL_DEATH`) still wins. For projectile classes the first body hit becomes the "low HP" enemy, which the default status order prefers; that is the rule working as written (priority first).

## Performance

The centre goal used to build LINQ arrays, a `List` and sorting delegates on every call; it now uses reusable lists (`packMembers`, `packPoints`, 74 slots) and a cached comparer. The candidate count is unchanged (centre 1 + hero 1 + ring 72) and so is the result order (distance², distance² to the hero, x, y). The new code adds one call per decision (5 Hz). The caller still builds one array of observed enemies per call. Device performance was not measured.

## Left alone / user decisions

- **Edge** movement has the same root cause (the attack animation hides the walk) but was not requested and is not gated. One line in `PackCentreWalk` (`Center` → `Center||Edge`) would apply the same fix; walking around the pack to its edge can take long, so it needs a decision.
- Skill starts are not delayed, so a skill-only build walks to the centre only for the basic attacks between skills.
- The first hit right after the first-engagement approach lands where he approached to (the meaning of "approach at once" is kept).
- Save: two values were added to `EdictFieldState` (`packWalkAt`, `packWalkDistance`; older saves load the defaults). No strings, save keys or edict format changed.

## Validation

Run: `EdictFieldPolicyTests` (whole class: fixed pack for three classes, determinism, rooted hero, other modes not held back, older save, centre target), `GuideHazardYardTests` (including the real tutorial gallery stage `ThePackCentreLessonStrikesTheGalleryPackFromItsMiddle`), `GuideTutorialTests`, `EdictWarriorAttackTests`, `EdictMageBasicTests`, `EdictRangerTests`, `HuntEdictQuickMenuTests` (CENTER forecast), `EdictTargetIntegrationTests`, `MonsterEdictTargetTests`, `EdictTargetMigrationTests`. 420 of 421 passed; the one failure was a test error (its expectation ignored the priority, low HP first), so it moved into a dedicated test that sets "nearest" first in the status order, and all six `PackCentre` tests passed (the runtime code did not change between the two runs). Not run: full Edit Mode, a player build, `RuntimeGuideTutorialSmoke` (needs a development player build; no UI changed). The tutorial evidence is the tick record of the real gallery stage playthrough (`gallery-centre.csv`), which uses the same simulation and the same edict commit path. Measurement CSVs are written under `HELLSCRIPT_GUIDE_TRACE_DIR`.
