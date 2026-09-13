# Weapon Rune Mastery and Global Layout Presets

Version 1.0 · 2026-09-13 · User requirements confirmed; implementation and verification in progress. [한국어](HELLSCRIPT_Rune_Mastery.md)

## Purpose and ownership

Rune Growth is an account-wide weapon mastery puzzle. Abilities belong to board cells; owned hex pieces connect and activate them. A board belongs to a weapon category, not an equipment instance. Upgrading to a better item of the same category preserves the board.

The user explicitly requires editing at any time, free removal and reuse on another board, no simultaneous reuse of one physical rune, and exactly five presets that each store every weapon board together. There is no permanent binding. Low value tiers contain basic stats; high tiers contain critical, speed and skill enhancements; the highest tier contains skill levels.

The category mapping, account sharing, unlock and reward cadence, and values below are implementation choices that complete the requested feature. They remain initial balance rather than proof of long-term tuning. This request supersedes the older exclusion of rune combinations for this feature only.

## Categories and scope

| Category | Existing item bases | High-tier stats | Featured skills |
| --- | --- | --- | --- |
| Sword | B01 | Attack speed, critical chance, physical damage | W02 Leap |
| Greatsword | B02 | Close damage, critical damage, damage to controlled targets | W01 Whirlwind |
| Axe | B03 | Damage to injured targets, critical damage, physical damage | W03 Crushing Strike |
| Bow | B04, B05 | Attack speed, damage to healthy targets, vulnerable damage | A02 Multishot |
| Crossbow | B06 | Distant damage, critical chance, vulnerable damage | A01 Piercing Shot |
| Staff | B07, B08, B09 | Fire, cold, lightning damage, cooldown reduction | M01 Fireball, M02 Blizzard, M03 Chain Lightning |

Only the currently equipped weapon category contributes. Selecting another board in the editor does not equip that weapon. With no weapon, no board contributes. Basic stats on other categories are inactive too. Every hero shares the inventory, boards and five preset slots. A separate mastery XP tree is not introduced.

## Geometry and three distinct grades

Each category has one fixed 427-cell board, using PackBound's seven 61-cell hex regions and original starts. There are 2,562 cells across six boards. Equipment acquisition never rerolls a board.

Region grades G0–G6 unlock at account-best real rift clears 0, 5, 10, 15, 20, 25 and 30. Any hero's clear counts. Rune grades G0–G6 correspond to Slate, Ivory, Green, Sky, Violet, Gold and Rose. A rune cannot exceed the unlocked grade.

A cell's value tier is independent of its region and required rune color. An unlocked G0 region can contain a tier-6 ability. Cells are ordered by distance from the region center, making basic stats predominantly inner nodes and high-tier abilities outer nodes.

| Value tier | Content | Cells per region |
| --- | --- | --- |
| 0–2 | Attack power, strength, dexterity, intelligence, willpower, life, all resistance, armor | 37 |
| 3–4 | Critical, speed and category-specific stats | 12 |
| 5 | Featured-skill bonus damage or resource cost reduction | 9 |
| 6 | Featured-skill level +1 | 3 |

Every central start grants attack power. A skill without a resource cost, such as Leap, never receives cost-reduction nodes. High-tier skill enhancements are working damage and cost modifiers; no unimplemented projectile, summon or behavior effects are advertised.

## Values and skill levels

The value multiplier is `1 + tier × 0.25`. Attack power grants `0.4 × multiplier`, life grants `2 × multiplier`, and other basic stats grant `1 × multiplier`. Rune attack power is added to weapon base power before the existing primary-attribute calculation. Derived attributes use the existing character sheet rules.

Critical chance, attack speed and cooldown reduction grant `0.12 × multiplier` percentage points. Other high-tier percentage stats grant `0.5 × multiplier` points. Identical contributions add before HELLSCRIPT's existing caps. PackBound combat caps are not imported.

A skill damage node grants +3%, capped at +60% from runes. A skill cost node grants 2 percentage points, capped at 30 points from runes and the existing 50% total cost-reduction cap. A learned skill has base level 1 and may gain up to five rune levels. Each extra level adds 10% to that skill's damage coefficient. Level damage and skill bonus damage add, up to +110%. Levels do not implicitly alter range, control or duration. Unlearned skills remain level 0 and are not unlocked by runes.

## Placement rules

The original 34 shapes are retained: 1, 1, 3, 7 and 22 shapes at sizes 1–5. Rotate in 60-degree increments; mirroring is unsupported.

Each color must cover its own start. Separate same-color pieces cannot share an edge and connect when at least one cell pair is at hex distance two. All pieces must reach the start through the whole layout. Disconnected cycles are invalid and invalid pieces cannot relay connections.

Different colors may touch but cannot overlap. Foreign-color starts cannot be covered. A wrong-color ordinary cell may form a path but does not activate its ability. An intervening different-color piece does not sever a link. Locked or nonexistent intermediate cells cannot bridge a link.

Removing a piece returns it to the shared inventory. A valid staged removal permits moving it to another board in the same editing session. Combat uses only the saved global layout. If removing or rotating a bridge disconnects pieces, highlight the invalid layout and block saving or changing boards until repaired or reverted.

## Screens and editing

Enter Rune Growth from the sanctuary shortcut or Growth screen, including during combat. Six category buttons appear above the selected board. Global presets, fusion and five-page practice are separate destinations.

The board supports selection, tap placement, drag movement, 60-degree rotation, removal, zoom, pan and recentering. Show legal candidate coverage, exact landing preview, invalid pieces, required colors and starts. Storage filters by grade and size. Cell details distinguish value tier, required rune grade, region unlock and inactive reasons. The summary distinguishes preview from saved effects. Save, revert and close remain fixed at the bottom. Closing with unsaved layouts requires confirmation.

Invalid or offscreen drops preserve the old placement. Navigation and cancelled pointers cancel dragging. Korean and English, portrait and landscape, and text sizing follow HELLSCRIPT's UI.

## Combat and persistence

Opening the editor pauses combat and the rift timer; closing restores the prior pause state. Automatic repeat waits while the editor is open. A successful save refreshes current weapon stats without increasing health or resource. If a maximum decreases, clamp to that maximum. Do not reset cooldowns, actions, projectiles, ground effects or random streams.

Released projectiles and existing ground effects retain their captured rune attack, element, critical, skill and conditional bonuses. New effects capture new values at the existing effect-creation boundary. An unreleased preparing action follows the game's existing release-time calculation. Channelled actions use new values on the next normal paid tick.

Validate copied state before writing. Publish changes to the live account only after the file write succeeds. Failed writes preserve ownership, layouts, presets, combat and the original file. Stale editors cannot overwrite a newer revision.

## Five global presets

Each slot stores real rune IDs, categories, coordinates and rotations across all boards. Empty boards remain empty after replacement. Equipped items, hero selection, skill loadout, Hunt Edict, rune ownership and pending fusion results are not preset contents.

The same rune may be referenced in several alternative presets but appears at most once in the active global layout. Saving uses the committed global layout and confirms overwriting. Loading confirms replacing all boards, then validates every rune and board atomically. Missing consumed runes, ownership, grade, duplicate, region or connection errors reject the entire load. Never substitute similar runes or partially apply a preset. Clearing a slot preserves owned runes and active layouts. Save or revert unsaved edits before preset operations.

## Acquisition and fusion

New accounts and migrated saves receive twelve G0 single-hex runes and one G0 sample at each size 2–5, exactly once. Existing gear, currency and progress are preserved. Each real rift boss grants four runes. Reward grade is `min(6, floor(rift tier / 5))`; size is uniformly 1–5, and shape is uniform within that size. Use a separate deterministic reward stream and prevent duplicate grants for one run. Training, idle rewards and sweeps do not grant runes.

Fuse two stored runes of the same grade and size; shapes may differ. Sizes 1–4 become the next size at the same grade. Size 5 becomes one hex at the next grade. G6 size 5 is final. Fusion always succeeds when valid and has no fee or destruction risk. Placed or unclaimed runes cannot be materials. Up to 100 pairs may be fused together; odd material counts or invalid rows reject the complete operation.

Save results as pending before revealing them. Claim all results for the selected source grade together, including results that advanced to the next grade. A row with unclaimed results cannot fuse again. Closing or restarting preserves actual results. Unconfirmed material selections are discarded without consuming runes.

## Reuse and verification

Reuse PackBound's geometry, shapes, rotation, graph validation, draft and ownership model, activation evaluator, aggregation and isolated 19-cell five-page practice model. [Import provenance](../Implementation/Rune_Growth_Import.json) records source hashes. The source project is unchanged. Its 54-item abilities, random-blueprint loader, combat owner and scene are not imported; HELLSCRIPT uses its own UGUI and persistence adapters.

Verify geometry, connectivity, grade separation, category scoping, reuse, all five global presets, missing-rune rejection, fusion, reward idempotency, failed writes, reload, combat continuity, actual high-tier skill effects and Korean/English portrait/landscape screens. [Implementation evidence](../Implementation/Rune_Mastery_Implementation.md) distinguishes completed checks from remaining work.
