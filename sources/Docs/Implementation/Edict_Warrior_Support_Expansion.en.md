# Warrior Iron Wall, Battle Shout and melee basic edicts

Date: 2026-09-13 · [한국어](Edict_Warrior_Support_Expansion.md)

Status: **All 13 remaining core options are connected: four for Iron Wall, five for Battle Shout and four for melee basic attacks. The Warrior's six active skills and basic attack now use independent policies for all 30 core options.**

## Scope and ownership

This continues [Ground Slam](Edict_Slam_Expansion.en.md) from `fe047ca`, following the [option catalog](../Design/HELLSCRIPT_Hunt_Edict_Skill_Option_Catalog.md) and [screen layout](../Design/HELLSCRIPT_Screen_Layout_Detail.md). Claude's attribute, affix and balance work remains integrated. The merged work branch was already removed; development continues on `main`.

The saved v0.2 document feeds `EdictRuleOrder.ForSimulation`, read-only `CombatSimulation` planners and the existing preparation, release and recovery pipeline. BASIC requires no active slot. Missing, disabled or duplicate legacy rows cannot override a core option or produce duplicate actions; original rows remain unchanged. Characters with v0.2 disabled retain legacy behavior.

## Policies and actual effects

| Action | Connected behavior |
|---|---|
| Iron Wall · four options | Automatic use; emergency, prevention or elite-entry timing; other shields; actual Leap Defense. A new elite/boss engagement gets one opening check. Unavailable shielding does not delay attacks. A direct global emergency bypasses waits for other defenses while preserving the own-W05 restriction and total shield cap. |
| Battle Shout · five options | Automatic use; resource, damage or both; encounter group; buff timing; out-of-combat recovery. Resource decisions inspect the selected attack in the real order and its actual discounted cost. Cooldown, range or purpose failures are not resource shortages. Resource recovery ignores buff-only encounter/reach restrictions. With no perceived enemies it requires explicit permission and resource at most 60. |
| Melee basic · four options | Automatic use; fill, resource or finish role; global, current, nearest or low-HP target; hold until an earned Restraint charge. Finish filters to HP at most 30% before target selection. Holding requires actual LC02, releases unusable targets and cannot block higher-priority active or survival actions. |

Iron Wall retains 0.2 s preparation, 30% max HP multiplied by real shield generation, 4 s duration and 14 s cooldown. WP05 and shields from other sources use actual stats and remaining amounts. Release rechecks the combined cap if another shield appears during preparation.

Shout grants 40 resource and a six-second 20% damage buff only after 0.35 s preparation, discarding recovery above the actual resource maximum. It neither cancels nor overlaps Whirlwind. Survival cancellation before release grants nothing. Buff reach checks real target geometry without recursively requiring Shout, resource or charges from a later attack.

A melee basic grants 10 resource and LC02 progress only on an actual hit within 2 m. Death, range or line-of-sight misses emit an action-miss record without rewards. A Warrior using this basic with the stand policy now closes the old 2–2.5 m dead zone. Existing WP01/WP04/WP06 hit and kill effects still require their real equipped passives.

## Persistence and UI

Committed preparation completes once even if automatic use is subsequently switched off. Existing save fields restore preparation, recovery, the elite opening check and actual LC02 hit/charge state; no extra reservation or save field was introduced.

All three screens have KO/EN explanations with a scrolling body and fixed Back, Revert and Save/apply footer. The final player cycled all 13 options through actual UI button events and compared fresh disk reads with the selected values.

## Evidence and remaining work

| Check | Result |
|---|---|
| Full Unity Editor suite | **1,981 passed**, zero failures or skips. Exact execution timestamps are preserved in the summary. |
| New support/basic fixture | **79 passed**, including cap/emergency behavior, exact affordability and HP boundaries, action locks, targeting, misses and restoration. |
| Final macOS player | **Five independent processes passed**: shield preparation and release, Shout preparation and recovery, first basic hit, saved second hit, and the actual third-hit charge consumed by Crushing Blow. |
| Actual controlled values | Shield 372; resource 40 after Shout, 70 after three hits and 57.5 after the actual 12.5-cost discounted Crush. These are fixture observations, not prescribed balance values. |
| Visual review | **17 screenshots reviewed**, covering KO/EN, 1280×720, 720×1280, 640×360 and 140% text. The small landscape capture checks the new note and fixed footer, not every option or device combination. |

Earlier focused failures exposed a test array-editing error and fixtures that assumed the old behavior. Tests now distinguish committed preparation from future target selection, and an ended encounter from an actual delayed miss. Exact payer and finisher thresholds were also corrected and tested. Earlier results remain in the evidence directory.

See the [validation summary](../../Artifacts/Validation/EdictWarriorSupport/validation-summary.json), [visual review](../../Artifacts/Validation/EdictWarriorSupport/visual-review.json), [source/player comparison](../../Artifacts/Validation/EdictWarriorSupport/validated-source.json) and [final macOS player](../../Builds/macOS-EdictWarriorSupport/HELLSCRIPT.app). Verification used isolated project copies and dedicated saves. Seventeen unrelated working files were preserved; scenes, prefabs and packages were not changed.

Remaining work includes Ranger policies, long-run survival/growth/farming behavior in generated rifts and mobile-device validation. This phase proves functionality, persistence and UI integration; it does not establish improved balance.
