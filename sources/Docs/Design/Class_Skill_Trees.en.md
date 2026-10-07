# Class skill trees and progression

Updated: 2026-10-07 · [한국어](Class_Skill_Trees.md)

Each class now has **16 normal actives, 19 passives and two ultimates: 37 skills**. We reviewed direct effect dependencies across the original 108 skills and split each dual-ultimate support passive into two dedicated passives. One addition per class brings the total to 111.

[Warrior HTML](../../Prototypes/SkillTree/Warrior.html) · [Ranger HTML](../../Prototypes/SkillTree/Ranger.html) · [Mage HTML](../../Prototypes/SkillTree/Mage.html)


**Native integration:** [Hunt Edict](../Implementation/Hunt_Edict_Skill_Tree.en.md) uses four normal active slots plus a separate ultimate slot, with all unlocked passives always active. The original HTML slot model below is historical prototype context, not the current game rule.

Native update, 2026-09-28: the first three actives of every class unlock at level 1. Zero-rank allocation and the starting-point refund after an explicit reset follow the [runtime integration record](../Implementation/Hunt_Edict_Skill_Tree.en.md). The free-base-rank description below documents older builds and the HTML prototype.

Native update, 2026-09-29: the skill catalog now also unlocks all three first-row actives at level 1. A new character starts with no learned skill and uses only basic attacks; the one point it has at level 1 learns one first-row active. Existing characters keep their allocation.

## 2026-10-07: no more "this or that" prerequisites

The user pointed out that choice prerequisites ("unlock skill A or skill B") make the skill tree messier. The ten skills that had one (`oneOf`: Warrior WP11, WP17; Ranger AP11, AP12, AP13, AP16; Mage MP07, MP08, MP10, MP11) now require **one skill**. When an effect reads several skills, the one already on the same branch is required and the others stay as optional targets mentioned only in the effect text.

- Nine of them already had the chosen skill in their parent chain, so who can unlock what, and when, is unchanged. Only Ranger AP12 lost its trap condition (only Retreat Leap A04 is needed). Traps sit in another branch and could not be linked; without a trap the effect simply never triggers.
- The `optional` table in `audit-dependencies.cjs` adds the required skill and the optional targets and compares the total with the direct targets in the source text; the build refuses any new choice prerequisite.
- The ultimate gate ("one skill from the preceding stage") has no branch link and is a design rule, so it is unchanged.

## 2026-10-06 escape unlocks

Ranger Retreat Leap (A04) and Mage Teleport (M04) open at hero **level 5**. Warrior Leap Slam (W02) remains at level 1. The two movement roots move into their first bands while descendant levels and prerequisites remain unchanged. [Implementation](../Implementation/Escape_Skill_Unlock_Level.en.md).

## 2026-10-05: branch parents and a clean link layout

The user asked for the links in the skill tree to stop looking arbitrary and to read like the Diablo 2, 3 and 4 trees, with a clear before-and-after order. Until now 70 independent skills (about 23 per class) opened on level alone and hung from the branch trunk, while links existed only for the few skills whose effects read each other. This request replaces the earlier rule below that shared branch membership is not a dependency: **branches now have parent skills**.

- **One parent per skill.** Every skill except the ultimates has one parent (`lineage`) in its own branch. Each class has four starting skills (Warrior W01, W02, W03, W05; Ranger A01, A02, A03, A04; Mage M01, M02, M03, M04). The result is a tree that only forks and never merges: each of the 93 skills that has a parent draws exactly one link.
- **No unlock level changes.** A parent never opens later than its child, so the level table is untouched and the only new rule is that the parent must be open. It does not require spending points on the parent first (the same open-or-not test the effect prerequisites already use).
- **Effect prerequisites are audited as before.** The direct relations (`all`; no `oneOf` since 2026-10-07) are still compared with the source text by the independent validator. The structural parent is a separate field and never counts as effect evidence; when an effect prerequisite already is the parent it is not recorded twice. An effect prerequisite that is an ancestor of the parent is not drawn again (transitive reduction), so Hold Formation WP16 hangs from its nearby parent instead of a distant Whirlwind W01.
- **Hand-set layout, tool-checked.** The socket grid in `tree-design.cjs` (branch × stage × row, two columns per row) sets positions, and `tree-layout.cjs` refuses to build when a link would cross a skill, run upward, leave its branch or share a socket. The HTML planner and the game (`row`, `col`, `links` and `via` in `ClassSkillTree.json`) use the same layout.
- **Ultimates keep the shared gate.** They have no branch link, only "level 40 plus one preceding-stage skill". Only each ultimate's dedicated passive hangs directly below it.
- **Effect targets in another branch are not linked.** A direct target in a different branch, such as the trap requirement of Prepared Escape AP12, is listed in the detail panel only.

| Class | Count |
| --- | --- |
| Starting skills (no parent) | 12 |
| Effect prerequisite only (the effect target is the parent) | 18 |
| Structural parent only | 58 |
| Effect prerequisite and structural parent | 17 |
| Ultimates (shared gate) | 6 |

## Dedicated ultimate support

| Class | Ultimate | Dedicated passive | Effect |
| --- | --- | --- | --- |
| Warrior | War of the Ancestors W17 | Ancestral Legacy WP18 | Resources on ancestor hits |
| Warrior | Titan's Judgment W18 | Titan's Bulwark WP19 · new | Barrier per directly hit enemy |
| Ranger | Killing Rain A17 | Silent Execution AP18 | Extra damage to a marked first-wave target |
| Ranger | Shadow Pursuit A18 | Relentless Pursuit AP19 · new | Two extra seconds and two extra total procs |
| Mage | Triune Collapse M17 | Archmage's Testament MP18 | Resources on a third-stage hit |
| Mage | Sage Incarnate M18 | Sage's Ward MP19 · new | Add 10% maximum HP to its barrier |

All six passives require **level 40 and unlocked base rank 1 of their own ultimate** in the HTML planner. The other ultimate cannot unlock them. Each is placed immediately below its matching ultimate. Existing magnitudes are divided between dedicated effects without duplication or increases.

## Progression and dependency rules

Levels 1–9 establish foundations, 10–19 survival and resource management, 20–29 status and placed-effect combinations, 30–39 advanced builds, and 40 ultimates and their dedicated passives.

**35 skills use level plus direct effect targets/triggers** and **six ultimates use the shared stage gate**. The other 70 independent skills now also need their branch parent to be open, except the 12 starting skills (section above). General mark, barrier, health, or BASIC conditions do not force an arbitrary source skill. Equipment-only additions do not become base skill prerequisites.

General passives with multiple actual targets accept one unlocked target. Prepared Escape AP12 additionally requires Retreat Leap A04, whose cooldown it reduces, and one triggering trap. The default view draws the 93 branch links from the section above, and a selected skill additionally lights its effect prerequisites and followers. Details list every actual target and keep the structural parent apart from effect prerequisites, which are never mixed.

Ultimates retain **level 40 plus any unlocked skill from the same class's preceding level 30–39 stage**. Any active, passive or branch qualifies without equipping it. No individual candidate wires are drawn for this shared gate.

Base rank 1 unlocks free when conditions are met. Earn one upgrade point per level after level 1, reaching 39 at level 40. Normal skills cap at rank 5 and ultimates at rank 1. No extra prerequisite ranks or arbitrary cumulative investment gates apply. Equipment limits remain four actives, three passives and one ultimate; passive slots open at levels 3, 6 and 10.

## Complete dependency audit

The [audit](../../Prototypes/SkillTree/evidence/dependency-audit.json) records all 111 original effects, referenced targets, exclusions and unlock classifications. An [independent validator](../../Prototypes/SkillTree/audit-dependencies.cjs) extracts named skills and IDs from source descriptions separately from the configured tree. Explicit negative references are excluded; aliases and runtime-only links carry owner-code evidence.

Mutation tests remove the Ancestral Legacy prerequisite, omit one of multiple direct targets, and remove a runtime burning source, verifying each omission fails validation. Trap Hunter AP04 directly extends Venom Trap A03 roots. Briar Trap A09 does not apply that passive in its root path and is not connected.

| Dependent skill | Required activation | At least one direct source |
| --- | --- | --- |
| Landing Stance WP03 | Leap Slam W02 | — |
| Endless Spin WP02 | Whirlwind W01 | — |
| Lingering Shout WP10 | Battle Shout W06 | — |
| Wound Tracking WP08 | Raking Wound W09 | — |
| Stone Landing WP11 | — | Leap Slam W02, Resolute Advance W11 |
| Blood Reclamation W10 | Raking Wound W09 | — |
| Combat Preparation WP12 | Crushing Blow W03 | — |
| Battlefield Pressure WP14 | Ground Slam W04 | — |
| Hold Formation WP16 | Whirlwind W01 | — |
| Battle Reserve WP17 | — | Crushing Blow W03, Ground Slam W04 |
| Ancestral Legacy WP18 | War of the Ancestors W17 | — |
| Titan's Bulwark WP19 | Titan's Judgment W18 | — |
| Piercing Gaze AP03 | Piercing Shot A01 | — |
| Escaping Step AP02 | Retreat Leap A04 | — |
| Focused Aim AP13 | — | Piercing Shot A01, Patient Shot A08 |
| Trap Hunter AP04 | Venom Trap A03 | — |
| Alchemical Practice AP11 | — | Venom Trap A03, Briar Trap A09, Frost Snare A10 |
| Prepared Escape AP12 | Retreat Leap A04 | Venom Trap A03, Briar Trap A09, Frost Snare A10 |
| Decoy Tactics AP14 | Decoy Projection A11 | — |
| Seamless Reload AP16 | — | Smoke Cover A12, Hunt Preparation A16 |
| Pathfinder AP17 | Watch Ballista A15 | — |
| Silent Execution AP18 | Killing Rain A17 | — |
| Relentless Pursuit AP19 | Shadow Pursuit A18 | — |
| Dense Burn MP01 | Fireball M01 | — |
| Deep Chill MP02 | Blizzard M02 | — |
| Chain of Lightning MP03 | Chain Lightning M03 | — |
| Ignition Feedback MP07 | — | Ember Lance M07, Firewall M08 |
| Scorched Armor MP08 | — | Ember Lance M07, Firewall M08 |
| Ice Reverberation MP10 | — | Frost Nova M06, Glacial Lance M09 |
| Capacitor Orb M11 | Chain Lightning M03 | — |
| Reclaim Charge MP12 | Capacitor Orb M11 | — |
| Storm Spear M12 | Chain Lightning M03 | — |
| Overcharge MP11 | — | Chain Lightning M03, Storm Spear M12 |
| Archmage's Testament MP18 | Triune Collapse M17 | — |
| Sage's Ward MP19 | Sage Incarnate M18 | — |

## Ownership and persistence

Names, effects and runtime values are generated from `Docs/Design/ClassSkills/catalog.json` and `runtime_parameters.json` into `ClassSkills.json`. The split effects are implemented in actual combat code. HTML levels and dependencies remain a separate `tree-design.cjs` proposal; they are not applied wholesale to game unlock rules. Existing runtime unlock levels are preserved for save compatibility; new P19 skills are registered at level 40. General player release remains disabled.

Old game saves and HED4 documents containing 36 rank entries migrate to 37. When the second ultimate is selected, the old P18 investment and equipment move to P19 without adding points or slots. HTML exports follow the same migration. Migration produces a detached copy and leaves the source unchanged.

Browser storage uses `HELLSCRIPT_SKILL_TREE_PLANNER_V1`, separate from game accounts. The preview supports class-specific allocations, level-reduction refunds, undo, JSON export/import, search, kind filters, Korean/English, and portrait/landscape layouts. It adds no game window, scene, prefab or font owner. Game UI integration must use existing `ClassSkillReadModel`, `ClassSkillLoadout`, `GameStore` and shared UI components.

Descriptions, prerequisites and follow-up skills display localized skill names. Ice Echo shows Frost Nova and Glacial Lance instead of `M06` and `M09`; `BASIC` displays Basic Attack. Nodes and detail headings omit development IDs. Korean particles follow the resolved name's final consonant. Source data, saved IDs and graph relationships stay intact; names are resolved only for presentation.

## Verification and regeneration

Run `python3 tools/class_skills.py build`, `python3 tools/class_skill_runtime.py build`, then `node Prototypes/SkillTree/build.cjs` to regenerate documentation, runtime data and HTML data. [Graph results](../../Prototypes/SkillTree/evidence/graph-validation.json) establish reachability of all 111 skills at their unlock levels. [Model checks](../../Prototypes/SkillTree/evidence/engine-tests.txt) pass 39 cases, including matching versus opposite ultimates, direct alternatives and old-save migration.

[Browser checks](../../Prototypes/SkillTree/evidence/browser-validation.json) cover 111 icons, actual actions and persistence, ten language/viewport combinations and offline HTML. See the [ultimate links](../../Prototypes/SkillTree/evidence/warrior-split-ultimates.png) and [icon comparison](../Art/ClassSkillIcons/qa/ultimate-support-64-dark.png). Actual Unity/macOS counts and scope are recorded in the [final validation summary](../../Prototypes/SkillTree/evidence/validation-summary.json). No physical mobile testing was performed. Public wiki publication accompanies a later main merge.
