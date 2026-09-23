# HELLSCRIPT equipment-linked class skill design

As of 2026-09-22 · **Approved design / ability validation in progress / general release and UI integration pending**

[한국어](HELLSCRIPT_Class_Skills.md) · [Source JSON](ClassSkills/catalog.json) · [Validation report](ClassSkills/validation.json)

## Proposed counts and navigation

Each class receives **16 normal actives, 19 passives and 2 ultimates: 37 skills**. Counting ultimates as active abilities gives 18 actives and 19 passives. BASIC, ranks and item-generated attacks are not additional skills.

| Class | Normal actives | Passives | Ultimates | Total | Catalogue |
|---|---:|---:|---:|---:|---|
| Warrior | 16 | 19 | 2 | 37 | [Warrior skills and builds](HELLSCRIPT_Warrior_Skills.en.md) |
| Ranger | 16 | 19 | 2 | 37 | [Ranger skills and builds](HELLSCRIPT_Ranger_Skills.en.md) |
| Mage | 16 | 19 | 2 | 37 | [Mage skills and builds](HELLSCRIPT_Mage_Skills.en.md) |
| Total | 48 | 57 | 6 | **111** | [Equipment matrix](HELLSCRIPT_Skill_Equipment.en.md) |

Preserve the existing six actives and six passives per class: 36 existing IDs and effects. Add ten normal actives, thirteen passives and two ultimates per class, creating 75 new designs. The labels “Existing runtime retained” and “New proposal” distinguish these scopes. Track runtime implementation and executed evidence in the [implementation record](../Implementation/Class_Skill_Runtime.en.md); general player access awaits UI integration.

## Reference findings

Checked in the Codex in-app browser on 2026-09-22. Counts exclude variants, runes, upgrades and ranks.

| Game | Verified structure | Design consequence |
|---|---|---|
| Diablo II | Barbarian, Amazon and Sorceress each have three visible trees of ten nodes. These are historical community guides updated in 2022, not a current-client balance audit. | Use warcries/masteries, bow/decoy and elemental specialization, with loadout choices despite a broad catalogue. |
| Diablo III | Official guide row counts: Barbarian 23 actives/19 passives; Demon Hunter 24/19; Wizard 26/18. | Separate offense, mobility, defense and resource tools, with enough selectable passives to enable equipment conditions. |
| Diablo IV | A Blizzard developer article describes the April 28, 2026 rework in terms of changed elements, reach and attack forms, using fire, wave and ranged Rend variants. | Use pulls, bleed cash-outs, splitting arrows and orb charging instead of only damage multipliers. Do not report old tree counts as current counts. |
| DoE: Dungeon of Exile | HUNT GAMES' store description confirms one-handed play, class skills/talent trees, and damage, immunity and recovery combinations. Individual skill specifications and per-class active/passive counts are not disclosed. | Use observable automatic-use conditions, resource recovery and defensive support. Do not invent unverified DoE skill names or counts. |

Eighteen actives are fewer than Diablo III's 23–26: HELLSCRIPT can preserve its six anchors and fill equipment gaps with ten new normal roles and two ultimates. Eighteen passives follow the reference's 18–19 scale, while three equipped slots retain meaningful choices. This is a design judgment based on the current equipment, not an experimentally proven optimum.

- Diablo II: [Barbarian](https://www.diablo-2.net/classes/barbarian/), [Amazon](https://www.diablo-2.net/classes/amazon/), [Sorceress](https://www.diablo-2.net/classes/sorceress/).
- Diablo III: [Barbarian actives](https://eu.diablo3.blizzard.com/en-us/class/barbarian/active/)/[passives](https://eu.diablo3.blizzard.com/en-us/class/barbarian/passive/), [Demon Hunter actives](https://eu.diablo3.blizzard.com/en-us/class/demon-hunter/active/)/[passives](https://eu.diablo3.blizzard.com/en-us/class/demon-hunter/passive/), [Wizard actives](https://eu.diablo3.blizzard.com/en-us/class/wizard/active/)/[passives](https://eu.diablo3.blizzard.com/en-us/class/wizard/passive/).
- Diablo IV: [Blizzard's skill-tree explanation](https://news.xbox.com/en-us/2026/04/22/diablo-4-skill-tree-overhaul/).
- Dungeon of Exile: [Official Google Play description](https://play.google.com/store/apps/details?id=com.hg.dungeonofexile.gl&hl=ko).

Names, wording and values are written for HELLSCRIPT. Shared mechanical inspiration does not import the source game's values, rune system or platform behavior.

## Equipment-led class roles

| Class | Equipment requirements | New support roles | Example |
|---|---|---|---|
| Warrior | Sustained Whirlwind, leap/heavy attack sequences, high resource, barriers/blocks and bleed cash-outs. | Brand/hook, independent bleed/reclamation, advance/stance, grouping and resource reserve. | W07 enables LW22's Marked predicate. W04 consumes Red Scars bleed; W10 consumes only W09 bleed. |
| Ranger | Original shots on one mark, BASIC preparation, post-retreat echoes, poison traps and imbued shots. | Successive/aimed shots, root/frost traps, decoy/smoke, poison/split arrows, ballista/preparation. | A13 supplies poison outside traps. A06 charges and set shot counters retain their exact original eligibility. |
| Mage | Repeated Fireball, Blizzard Frostbite, Chain Lightning procs, three-element cycles and barrier/resource recovery. | Firewall, cold projectiles, capacitor/spear, ward/reclaim, elemental discounts and grouping. | M09 supplies Freeze for LM37. The Triune set still requires actual M01/M02/M03 casts. |

All 120 existing class legendaries and three shared items have links. **Six new legendary concepts per class, 18 total**, fill gaps for new skill pairs. They do not remove existing items or alter owned gear; their drop weights are undecided. The [implementation record](../Implementation/Class_Skill_Runtime.en.md) tracks runtime coverage for all 141 linked records.

The design includes all 24 new set concepts and 60 bonus tiers, plus six existing sets. There are **30 full-set examples**, each with four normal skills, three passives and one ultimate. Proposed legendaries do not overlap the set's slots. Validation conservatively uses one item per slot category, without relying on additional rings or off-hand slots. Every new normal active and every passive/ultimate appears in at least one example.

## Loadouts, unlocks and exclusive ultimates

1. Retain up to four normal actives and three passives. BASIC remains a separate fallback when resources run out. Propose one dedicated ultimate slot; the slot and HUD expansion are not implemented yet.
2. Preserve existing unlock levels and spent points. New normal actives unlock in pairs at levels 22/26/30/34/38. New passives unlock in groups of three at 22/26/30/34. Both ultimates appear at level 40 in the same final tier. Level 40 is enabled only for development validation profiles.
3. The complete view orders normal actives 01–16, passives 17–35, then ultimates 36–37. In an active-only view, ultimates are entries 17 and 18 and remain last even when other sorting is applied.
4. Before level 40, neither ultimate can be selected. Afterwards, select exactly one, and only that ability can be learned, equipped or cast. An unselected eligible hero is prompted before departure; loading a save itself must remain possible.
5. Reject presets containing both ultimates, a foreign-class ultimate or an ultimate in a normal slot at UI, save-application, share-code import, run-start and cast boundaries. Equipment, passives and automatic procs cannot cast the unselected ability.
6. Switch only in town. Preserve a class-wide next-ultimate-ready timestamp through swaps, unequips and save restoration. Both base cooldowns are 60s, with a 45s effective minimum after ordinary reduction. Direct cooldown refunds target only explicitly named normal skills.
7. Switching ends the previous ultimate's buffs, summons and charges. Point allocation cannot retain both. Ultimates are fixed rank one and do not receive additional skill points.

| Class | Full entry 36 / active 17 | Full entry 37 / active 18 | Choice |
|---|---|---|---|
| Warrior | War of the Ancestors `W17` | Titan's Judgment `W18` | Sustained support/reduction versus immediate area impact. |
| Ranger | Killing Rain `A17` | Shadow Pursuit `A18` | Fixed-area clearing versus mobile shooting support. |
| Mage | Triune Collapse `M17` | Sage Incarnate `M18` | Short elemental burst versus barrier and resource-efficient rotation. |

## Values and event ownership

- `D` is the attack basis used by the existing primary-stat/offensive calculation. D100% means D×1, not a copy of final damage. Values are rank one. The existing 36 skills retain `SkillEffects` scaling.
- New normal skills have five ranks. Each additional rank adds 10% of the base direct/periodic damage, resource/HP restoration and barrier amount. New passives also have five ranks, increasing damage, critical, guard, speed, restoration and cost-reduction magnitudes by the same rule. Do not scale reach, area, control time, duration, cooldown, internal interval, target/charge caps, thresholds or fixed cooldown-refund seconds. Structural ultimate-passive changes such as +2s/+2 procs remain fixed.
- Share existing caps: 50% cost reduction, 40% normal cooldown reduction, 75% critical chance, 50% speed bonuses, and the established barrier/healing limits. Legendary damage uses the existing legendary additive pool; set/passive damage adds once to its corresponding pool. Block chance uses the existing block roll/cap and never guarantees success.
- Distinguish original casts, direct hits, original field ticks and secondary damage. New normal/ultimate source events may evaluate eligible existing `*` rules; exact-ID items still require their actual named skill. Echoes, splits, summons and secondary damage never recursively generate item/set/BASIC/resource-on-hit procs. Only explicitly listed ultimate-item/passive hooks observe ancestor/echo events, within their stated caps.
- W07 supplies a Warrior-owned Marked state with zero damage-amplification magnitude. Separate presence from amplification so it cannot borrow A05's damage bonus. A06 retains its existing BASIC/A01/A02 rules; do not automatically extend charges to new shots, ballista or ultimates.
- Unless specified otherwise, new area secondary damage selects up to five visible enemies within 3m, ordered by distance then enemy ID. W12 counters use a 90-degree front arc. M11's expiry burst uses the orb center, 3m and five targets. M18 cycle bursts use 2.5m and five targets.
- Fields and summons first tick after one complete interval. A17 explicitly starts its first pulse at cast time and then runs every 0.5s, six pulses total. Exceeding a same-skill placement cap replaces the oldest of that skill. Existing Poison Trap remains capped at two; A09/A10/A11/A15 each have a separate cap of one. Replacement/scene exit grants no natural-expiry reward.
- Store damage-over-time by source and target, preserving next-tick cadence on refresh. W10 consumes only W09; Red Scars W04 consumes only its set ledger. Apply offensive amplification once at storage and target mitigation once at damage resolution.
- Stun/freeze/root/pull use existing boss stagger rules. Stagger alone is not Freeze. Explicit exceptions such as MP10 accepting valid stagger credit are stated individually. Mark, poison and slow are distinct predicates.
- Per-cast effects do not multiply by arrows, pierces or chain visits. The first valid consumer reserves/spends one charge. Distinguish cancellation, misses, death, cap overflow and retroactive consumption of newly created charges. Preserve the existing set-specific reservation policy.
- Existing legendary pending damage retains its snapshot policy. New sets retain their rule of removing set-owned buffs, pending attacks and periodic ledgers when the piece threshold is lost. Do not replace these with one policy. New skill/item effects store source IDs; unequipping removes active buffs/counts while retaining internal cooldown deadlines.

## Runtime and UI integration boundary

The JSON under `Docs/Design` is retained as the design source. Its `runtimeEnabled=false` means this file is not executed directly. Compile runtime definitions into the separate `Resources/Data/ClassSkills.json`; preserve indices 0–17 as a compatibility layer and use stable IDs for new abilities. The [implementation record](../Implementation/Class_Skill_Runtime.en.md) tracks executable evidence. Icons, HUD and general drops in the list below belong to later UI integration.

1. Expand lookup/storage around stable string IDs. Preserve global active indices 0–17 as the original class's W01–W06/A01–A06/M01–M06. Map passive indices 0–5 and the twelve-rank array to the same original IDs. Initialize new unlocks/ranks separately; reject rather than silently discard foreign, duplicated or unknown IDs.
2. Make Hunt Edict, presets, sharing, run restoration, cooldowns, icons and HUD consume the same definitions. Preserve old settings/loadouts; prompt an eligible migrated hero to choose an ultimate in town. This design does not imply completed icons or animations.
3. Implement Warrior marks/owned bleed, new traps/decoy/ballista, capacitor/summon state, shared ultimate cooldown and source ledgers. Reuse world collision/movement and observed target/hazard information for automatic-use decisions.
4. Register the eighteen legendary concepts for actual items, events, bilingual text, drops and saves only after those hooks exist. Preserve six old sets; retain separate candidate IDs for the twenty-four new sets. Do not auto-convert owned gear or claim current powers already support new abilities.
5. Compare no-gear, rare, old legendary and set builds at equal item level, quality, upgrade and combat duration. Measure kills, resource downtime, survival, boss stagger and proc counts; compare sustained and burst ultimates over the same time window.

## Generation and validation

The [catalogue tool](../../tools/class_skills.py) generates class lists and the equipment matrix from JSON. Run `python3 tools/class_skills.py build` and `python3 tools/class_skills.py check` after data edits. Keep this rules document and its Korean version aligned.

Checks cover equal counts, order 1–37, two final ultimates, class/slot/level restrictions, rejected dual/unselected/misplaced ultimates, preserved existing skills/source fingerprints, 141 item links, 30 sets/60 new bonus tiers, and required skills/slot conflicts in thirty builds. This is **design-data and selection-contract consistency validation**, not Unity execution or balance proof for the new skills.
