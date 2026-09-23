# Class skill abilities and equipment integration

Updated: 2026-09-23 · [한국어](Class_Skill_Runtime.md)

**Status: native Hunt Edict skill tree integrated with player persistence and combat.**

This work implements the [approved skill design](../Design/HELLSCRIPT_Class_Skills.en.md). The player UI and adoption path now follow the integration record linked below. The 2026-09-23 follow-up adds three dedicated ultimate passives and icons, with progression prerequisites recorded separately in the HTML skill tree.


**Current player UI:** [Hunt edict skill tree integration](Hunt_Edict_Skill_Tree.en.md) supersedes the initial validation-only loadout described below. Player builds use four normal actives and one separate ultimate, all unlocked passives, level 40 progression and unified persistence. Version 1–2 details below document compatibility for earlier validation snapshots.

## Implemented scope

Warrior, Ranger and Mage each have 16 normal actives, 19 passives and two ultimates. The original 36 skills retain their indices and calculation paths. The 75 additions use stable string IDs. Eighteen new legendaries and 24 new sets with 105 pieces and 60 bonus thresholds are registered separately from weighted drops.

Integrated player configurations enable level 40 and new skills. Earlier development-only configurations retain their compatibility path. The level-40 investment budget is 39 points, with no retroactive award of experience discarded at the old cap.

## Persistence and combat rules

A saved loadout includes ID-based investments, four normal slots, three passive slots, one selected ultimate, use policies, automatic-use flags and action priority. The old 18 active indices remain W01–W06, A01–A06 and M01–M06. HED4 carries new IDs, ranks and ultimate selection; HED1–HED3 import remains available.

Ultimates unlock at level 40, always occupy metadata positions 36 and 37, and are mutually exclusive. Selection changes are allowed in town. Their shared cooldown starts at 60 seconds and cannot fall below 45 seconds after reduction. Save, restore and selection changes preserve its remaining duration.

Every cast and derived effect retains an origin skill and cast ID. Direct hits, periodic damage, splits, summons and item damage use separate paths. Warrior brands do not inherit Ranger vulnerability, skill bleed is separate from set bleed, and the original Shadow Arrow is distinct from Shadow Pursuit.

## UI contract

- `ClassSkills.For`: 37 ordered definitions per class, with Korean and English names and descriptions.
- `ReadClassSkills`: unlock and equipment state, invested and bonus ranks, costs, cooldowns and calculated values.
- `ClassSkillLoadout.Validate` and `GameStore.CommitClassSkills`: validation and atomic adoption.
- `InspectClassSkill` and `TryCastClassSkill`: readiness/reason and actual execution.
- `ReadClassSkillEffects`: detached read copies of active buffs, placements and charges.
- `ClassSkillChanged`: skill, source, cast, target and position events for later presentation integration.

## 2026-09-23 dedicated ultimate support

Ultimate support now has separate IDs. WP18/AP18/MP18 only affect the first ultimate, while WP19/AP19/MP19 only affect the second. Magnitudes and rank caps are retained. Existing runtime passive unlock levels remain unchanged for save compatibility; HTML level-40 prerequisites are a separate progression proposal.

`ClassSkillLoadout` version 2 validates the old 36 entries before migrating to 37. Selecting the second ultimate moves P18 investment and equipment to P19, resetting P18 to rank 1 without adding points or slots. The first ultimate keeps the old investment. HED4 integrity and unknown-field rejection remain. New icons use the existing `SkillIconAssets` resolver.

This focused Edit Mode run passed 352 tests. The macOS player verified all six matching ultimate/passive effects and seven save-restoration cases across separate processes. See the [current validation record](../../Prototypes/SkillTree/evidence/validation-summary.json), [Edit Mode results](../../Prototypes/SkillTree/evidence/ultimate-split-editmode.xml) and [native receipts](../../Prototypes/SkillTree/evidence/native/manifest.json). Counts below document earlier runs and are not added to this run.

## Validation record

The final focused ability/policy/localization run passed all 369 tests. Its 336 skill-specific cases include all 90 build/scenario fights, the 18 policy comparisons, reward restoration and ward-shield preparation. The initial focused Edit Mode run passed 319 tests and the corrected compatibility run passed 440. Coverage includes actual effects and rejection conditions for 36 added actives/ultimates, equipped-versus-unequipped outcomes for 36 new passives, 18 legendaries, 60 set thresholds, and existing legendary/rank/save regressions. Focused runs overlap; their counts are not added to one another or to the full suite.

The evidence records [90 fights across 30 builds](ClassSkillEvidence/Build_Comparison.en.md) and [18 comparisons changing only use policies](ClassSkillEvidence/Policy_Comparison.en.md). Spacing, charging position and recovery goals alter actual damage, incoming damage, movement and charging time. This does not certify equal strength for every option or final balance.

The macOS development build completed with zero errors. Seven cases restored ultimate preparation, arrow travel, orb expiry, charging, deliberate leap spacing and bleeding combat before/after a rewarded kill across two separate player processes. Damage, resource, health, shared cooldown, RNG, events, rewards and policy progress match uninterrupted combat. The reward cases each settled one kill, one item drop and 30 gold actually collected; another claim after a collected checkpoint and restart added no gold. Discrete fields match exactly; the largest observed floating-point difference was approximately 1.42e-14. The [native evidence manifest](ClassSkillEvidence/native/manifest.json) preserves expected/actual receipts and source hashes.

The final full Edit Mode run recorded 3,273 passed, 2 failed and 0 skipped out of 3,275. All 331 skill-specific tests passed. One missing localization-table entry was added and passed the focused localization suite. The remaining failure concerns existing UI Graphic CanvasRenderer declarations. Files containing those five types are byte-identical to the design baseline; this task made no presentation edits. The full suite is not reported as passed. Both the [complete report](ClassSkillEvidence/final-editmode.xml) and [baseline UI comparison](ClassSkillEvidence/pre-existing-ui-test.json) are preserved.

[Final focused acceptance](ClassSkillEvidence/acceptance-editmode.xml) · [Reward/localization follow-up](ClassSkillEvidence/rewards-final-editmode.xml) · [Initial focus](ClassSkillEvidence/sets-editmode.xml) · [Corrected compatibility](ClassSkillEvidence/compatibility-fix-editmode.xml) · [Earlier full run](ClassSkillEvidence/all-editmode.xml) · [Policy reason correction](ClassSkillEvidence/policy-reasons-editmode.xml) · [18 policy comparisons](ClassSkillEvidence/experiments-editmode.xml)

## Runtime data and compatibility layer

`tools/class_skill_runtime.py` compiles the approved design, `runtime_parameters.json` and `use_options.json`. `ClassSkills.json` contains bilingual names, rank-one descriptions, numeric parameters, automatic-use conditions and equipment/set IDs. `ReadClassSkills` returns invested-rank calculations separately; equipment-granted ranks do not consume invested points.

The original eighteen `GameCatalog` actives and their `SkillEffects` calculations remain intact. Original full-file fingerprints remain historical provenance. Files extended with save fields or lookup paths are compared against the original constructor arguments in `legacy_definitions.json`, rather than updating hashes to conceal changed legacy values.

New items are never inserted into ordinary accounts. They use actual generation, ownership, equipment and persistence paths but are excluded from weighted drops. Set counts require distinct equipped IDs of the correct class and slot. Explicit higher-tier replacements do not add both bonuses.

## Save application and event delivery

Save schema 7 and HED4 retain stable-ID investments, use policies, automatic order, the selected ultimate and legacy edict options. Earlier HED4 payloads without the options field are still accepted, while unknown fields are rejected. Unknown IDs, fields, versions and corrupted codes are rejected. Unsupported ability saves preserve the original file rather than silently falling back over it.

Suspended combat stores the shared ultimate cooldown, preparations, projectile progress, summon/placement deadlines, charges, remaining spacing attempts, policy observations, source-owned DOT ledgers, internal cooldowns and RNG. Active actions reconnect to their recorded cast ID on resume. Replacing an orb or summon grants no natural-expiry reward.

`ClassSkillChanged` is delivered at a completed combat-update boundary. An observer requesting a save does not capture a partially resolved hit before its DOT is created. Future UI listeners should display these events and must never replay their damage or rewards.

## Automatic combat adjustment

New actions use stable-ID order and design conditions. Original actions retain Hunt Edict aiming, movement, recovery and gathering policies. Attack approach precedes exploration when the observed target is out of range; survival, gathering and loot keep their existing priority.

M17 originally required an elemental-cycle buff that some specified builds could not produce. Such builds now require an observed elite/boss or four enemies in the area. Builds able to produce the buff still wait for it. The bilingual design records the adjustment.

## Use policies and controlled experiments

The [policy design](../Design/Class_Skill_Use_Policies.en.md) defines 164 choices across 55 groups. Fifty-four actives/ultimates expose timing, target, positioning and waiting choices; Mana Reclaim also exposes a recovery goal. The 57 passives compete for the existing three passive slots.

W02 Leap Slam gains damage from actual travel distance. Players can leap from the current distance or spend at most two seconds creating space. M13 Mana Reclaim regenerates mana at ten times the ordinary rate at rank one while stationary. It has no cooldown or fixed duration. Movement or another valid attack cancels it; damage alone does not. Players choose holding versus retreating and resuming attacks at 50% versus 90% mana.

Spacing uses only perceived enemies and danger with existing wall/body checks. Attempts yield after at most two seconds, preventing endless retreat. The global emergency policy remains authoritative. DES_LM44 retains a six-second shield retrigger cooldown across channel restarts. Deep preparation can wait until 1.5s of concentration after reaching its mana goal when the item is ready and the hero is inside their own ward. Short recovery resumes offense immediately; an unavailable equipment cooldown adds no wait.

`ClassSkillOptions.For` returns bilingual choices, gains, costs and observation keys. `WithOption` creates a detached draft; `CommitClassSkills` saves it in town. `ReadClassSkillPolicies` returns detached deferral reasons, accumulated waiting, spacing distance, charging time and actual mana recovered. Different skills' waits can overlap and must not be summed into total offensive downtime.

## Contract for the UI task

1. Read definitions using `ClassSkills.For(heroClass)` and `ReadClassSkills()`: order, unlocks, invested and bonus ranks, and computed parameters. Do not put ultimates in normal-active slots.
2. Read choices and tradeoffs with `ClassSkillOptions.For`, edit a detached `ClassSkillLoadout` with `WithOption`, call `Validate`, then save through `CommitClassSkills` in town. Do not overwrite new choices through legacy index arrays.
3. Use `InspectClassSkill` for manual readiness and `InspectClassSkillAutomatic` for automatic deferral reasons. Codes include Korean and English explanations.
4. Send a skill ID, target and aim to `TryCastClassSkill`. Read cooldowns and detached effect copies; `ReadClassSkillDots` exposes owned DOT ledgers without granting mutable access to combat state.
5. Notify movement through `NotifyClassSkillMovementRequested` to cancel stationary charging. Read observations with `ReadClassSkillPolicies`.
6. Bind presentation to `ClassSkillChanged` IDs and event origin: cast, direct damage, periodic damage, summon, split, echo and equipment-derived damage.

## Validation environment and release conditions

Work runs in the isolated `codex/class-skill-runtime` worktree, without using ordinary accounts or the primary Unity Editor. Validation entry points require a development environment and an explicitly marked validation hero.

Thirty builds use level 40, rank-one skills, level-30 items, eight legal equipment slots, a fixed seed and a sixty-second window. The comparison arena uses actual equipped stats and combat calculations while suppressing rewards. Separate reward-restoration probes enable the real reward path on an isolated validation account. Single-enemy, eight-enemy and boss scenarios record damage, resource downtime, survival and casts. These are initial-value execution checks, not final balance or mobile-performance approval.

The macOS development player exits and restarts as a separate process to restore an ultimate preparation, an arrow in flight, an orb before expiry, mana charging and deliberate leap spacing. Receipts retain damage, resource, health, shared cooldown, RNG, event counts and rewards. Art, UI, input interaction and physical mobile devices are outside this validation scope.

After UI rules are finalized, connect these APIs and separately approve drop weights before enabling level-40 progression, new selections and new equipment acquisition together. Public wiki generation, checks and deployment occur when this branch is merged into `main`.

## Image production handoff

As requested, four separate tasks received stable IDs and effect descriptions for skill, legendary, set and aspect imagery. Per-family production totals, review results and remaining items are recorded in the handoff document. The [handoff record](../Art/Skill_Resource_Delegation.en.md) defines scope and ownership. Image completion and UI placement remain separate from combat acceptance.
