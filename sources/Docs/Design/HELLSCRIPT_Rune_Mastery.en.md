# Weapon Rune Growth — v13 Implementation Design

갱신일: 2026-09-23 · Version 13 · [한국어](HELLSCRIPT_Rune_Mastery.md)

## Authority and scope

The user's [v13 reference package](RuneV13/reference-v13.zip) defines the native Unity board, appearance and placement rules. Its README deployment instructions are reference material, not execution authority. [Provenance](RuneV13/provenance.json) records source hashes.

This replaces the previous 427-slot board, 61-slot regions, clear-based unlocking and seven grade-linked colors. It reuses PackBound's 34 polyhex shapes, rotations, editing transactions, shared inventory and HELLSCRIPT's existing drop/fusion economy. The reference's 160 demo runes and XP debug controls are not granted to players.

Sword, Greatsword, Axe, Bow, Crossbow and Staff each have their own board. Only the equipped weapon type's board contributes to combat, including its common attributes. Runes can be freely retrieved and moved to another weapon. A physical rune ID can occupy only one board at a time.

## Board and progression

Each weapon has 259 fixed slots: seven regions of 37, totaling 1,554 across six weapons. The regions are Core, Assault, Precision, Impact, Technique, Flow and Guard. Coordinates, ability assignments, values, tiers and glyphs are imported directly from the reference.

Each weapon starts at mastery level 1 with 19 central slots open. The cap is level 41. Advancing from level L requires L × 100 XP and grants six slot points. Opening a slot costs one point and must extend the open component connected to the origin. Complete all 37 slots in the current region before choosing another.

Choosing an outer region opens its elite center for free. That center is neither an unlock-path seed nor a rune connection origin. Completing the board costs 234 points, alongside 19 initial slots and six free centers. Level 40 can fully open the board; level 41 leaves six spare points.

The reference did not specify live kill XP. The initial integration grants only the weapon in use 5/25/100 XP for ordinary/elite/boss kills, multiplied by `1 + floor(rift stage / 5)`. Training and summoned enemies do not count. Receipts prevent duplicate grants. Hero XP bonuses do not affect weapon mastery.

## Color, ability tier and acquisition grade

The five ability colors are Attack, Magic, Support, Critical and Skill Bonus. Matching the block and slot color activates the ability; elite slots accept every color. G0–G6 acquisition grade controls drop size and fusion progression, independently of color and board unlocking. A low-grade rune can activate an open high-tier slot when its color matches.

Ability tiers follow the source's 1–4 classification: basic attributes and life at lower tiers, critical/attack speed and skill bonuses at higher tiers, and skill levels at the highest tier. Exact values and caps are available in the in-game codex and [Unity catalog](../Implementation/Rune_Mastery_Catalog.json). The three source-disabled attributes—hero control resistance, maximum stamina and stamina regeneration—remain unassigned.

| Weapon | Specialized skill | Additional modifiers |
| --- | --- | --- |
| Sword | W02 Leap Slam | Damage, level, cooldown, radius |
| Greatsword | W01 Whirlwind | Damage, level, resource cost, radius |
| Axe | W03 Crushing Blow | Damage, level, resource cost, radius |
| Bow | A02 Multishot | Damage, level, resource cost, range |
| Crossbow | A01 Piercing Shot | Damage, level, resource cost, targets |
| Staff | M01 Fireball, M02 Blizzard, M03 Chain Lightning | Damage, level, resource cost, and radius/duration/targets respectively |

Direct damage excludes damage over time, thorns and triggered effects. Multiple-target damage requires at least three distinct enemies in the same strike of one cast; separate chain hops are not combined into one strike. Area damage augments existing area attacks and ground skills without creating new areas. Outgoing control duration affects ordinary and elite enemies without increasing boss stagger. Ground-bound slow retains existing exit/expiration rules.

Skill damage runes cap at +60%. Dedicated skill cost reduction caps at 30 percentage points and combined cost reduction at 50%. Already learned skills gain at most five rune levels, each adding 10% to their damage coefficient. Inheritance shares this level cap. Radius, cooldown, duration and target count change only through their corresponding modifiers.

## Placement and connectivity

Leave the shared central origin empty. Each color starts with a block adjacent to it. Separate blocks of the same color cannot share an edge. A valid gap has two common neighboring slots, both open. A straight axial distance-two jump does not connect.

One other block covering both intermediate slots blocks that bridge. Two separate blocks covering one slot each do not. Different colors may touch but never overlap. A mismatched slot may relay connectivity without activating its ability. Outer elite centers still require a connection from the central origin.

Placement, rotation and retrieval validate the entire layout. An invalid operation preserves the previous placement. Retrieve outer dependent blocks before their bridge. Six 60-degree rotations are supported; mirroring is not.

## Native interface and saving

Landscape uses a left weapon rail, central board and right storage. Portrait stacks the weapon strip, board and storage. The interface imports the supplied weapon illustrations, 328 SVG symbols, colors, continuous block contours and gold borders. Storage has eight columns, five color filters and multiple simultaneous size filters.

Portrait omits the region-button strip below the board and the selection-detail panel. Regions remain accessible through the minimap. Two dropdown lists beside the storage title select color and sizes from one to five cells; sizes support multiple simultaneous selections. Selecting a stored block shows only a Rotate 60° action in storage. Each rotation updates the block in its inventory slot, and dragging preserves its displayed orientation and grabbed cell. Drag placed blocks back to storage to recover them. Tapping an empty board slot opens its ability and unlock actions. Space reclaimed from the controls goes to the placement board; landscape retains its region strip and detail panel.

The interface includes mastery/points, region preview, map/minimap, pan/zoom/fit, view/edit mode, active effects, codex, guide, selected-slot details, rotation/retrieval, undo/revert and save. Inventory and dialog contents scroll independently.

Editing pauses combat. Selecting a board does not equip a weapon. Save changes writes all weapons' unlock paths, layouts and preset changes together. Closing an unsaved draft prompts before discarding. Undo retains the latest 50 changes.

Exactly five presets store every weapon's rune IDs, coordinates and rotations together. They exclude mastery, unlock paths, ownership, equipment and skill settings. Registering, naming and clearing presets remain draft changes until Save changes. Loading validates the whole preset atomically; missing runes or invalid connectivity reject the complete load. Presets never duplicate runes.

Failed disk writes or stale revisions do not change the live account or combat. Saving does not refill life/resource, reset cooldowns or restart actions. Already created attacks retain their captured offensive rune bonuses.

## Drops and fusion

Ordinary enemies drop one rune at 2%; elites at 20%. Actual rift bosses grant four runes. Training, summoned enemies and sweeps are excluded. Runes enter their dedicated storage automatically and use no equipment bag space. Each color has a 20% chance; shapes are uniform within the selected size.

| Rift stage | Acquisition grade | 1 cell | 2 cells | 3 cells | 4 cells | 5 cells |
| --- | --- | ---: | ---: | ---: | ---: | ---: |
| 1–4 | G0 | 100% | 0% | 0% | 0% | 0% |
| 5–9 | G1 | 80% | 20% | 0% | 0% | 0% |
| 10–14 | G2 | 45% | 40% | 15% | 0% | 0% |
| 15–19 | G3 | 15% | 35% | 35% | 15% | 0% |
| 20–24 | G4 | 5% | 15% | 40% | 30% | 10% |
| 25–29 | G5 | 0% | 5% | 30% | 40% | 25% |
| 30+ | G6 | 0% | 0% | 20% | 40% | 40% |

These size probabilities are conditional on a successful drop. A stage 10–14 ordinary enemy therefore has a 0.3% chance to drop a three-cell rune.

Fuse two runes of equal grade and size: sizes 1–4 become the next size at the same grade; two five-cell runes become a one-cell rune at the next grade. G6 five-cell runes are final. There is no failure or fee; batch up to 100 pairs. Equal colors are paired first and preserved; remaining mixed-color pairs produce each color with 20% probability. Results are saved pending claim and cannot be placed until claimed. Placed and unclaimed runes are ineligible materials.

The existing one-time starter grant remains twelve G0 singles plus one example each of sizes 2–5. Those examples are a starter-grant exception, not low-stage monster drops.

## Migration and practice

Migration preserves every owned rune's ID, grade, shape and quantity. Legacy grade deterministically maps to one of five colors. Previously earned region access maps to equivalent completed regions and mastery. Compatible coordinates remain placed; incompatible runes return to storage. The complete original layout and presets are retained verbatim in the save's `legacyV1` field.

The five-page tutorial uses isolated practice pieces. As specified by the reference, its fixed blocks act as roots and it omits third-piece gap occlusion. Actual weapon boards use the shared central origin and full occlusion rules.
