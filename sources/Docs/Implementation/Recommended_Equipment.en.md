# Gear score and automatic equipment

Updated: 2026-10-01

[한국어](Recommended_Equipment.md)

Newly acquired equipment is immediately equipped when it meets the player's rules. Displaced gear is stored, salvaged or sold according to the common setting. Open **Hunt Edict → Auto Equip → Common settings**, turn **Auto-equip recommended gear** ON, and **Save**. The feature starts disabled for both new and existing accounts. Turning Hunt Edict itself off also disables automatic equipment.

## Player settings

Recommended settings choose higher-score weapons of the same type, higher-score armor and accessories, and store replaced gear in the warehouse. Head, chest, hands, feet, belt, necklace, left ring and right ring have independent settings.

| Equipment | Selection | Replacement rule |
| --- | --- | --- |
| Weapons | Same weapon type · higher score | Same category, such as sword, axe or greatsword, with a higher resulting weapon loadout score |
| Weapons | Any weapon type · higher score | Higher resulting weapon loadout score, regardless of category |
| Weapons | Do not replace | No automatic weapon or off-hand replacement |
| Armor/accessories | Equip if score is higher | Higher intrinsic score |
| Armor/accessories | Equip if selected options improve | Every selected option is strictly higher; a lower score is allowed |
| Armor/accessories | Higher score + preserve selected options | Higher score and every selected option is at least as high |
| Armor/accessories | Do not replace | No automatic replacement for that slot |

**Add comparison option** uses the same per-slot option catalog as salvage, including the primary stat. Primary choices use a label such as `Base stat · Armor` to distinguish them from affixes. Selected rows show the current value and a remove action. An option-based rule with no selected options never replaces gear. A missing candidate option fails the rule; an option absent on current gear has a baseline of zero. Comparisons use the two-decimal values shown in details.

Only eligible ring slots are considered. The largest score improvement wins, with the left ring first on an exact tie. Empty slots still honor their configured rule. The same-type rule also checks the currently equipped type when filling a free hand. Off-hand equipment never automatically removes the only attacking weapon. Saving does not rescan the bag: only subsequent acquisitions are evaluated. Storage transfers and shop buybacks do not count as new acquisitions.

## Replaced equipment and full warehouse

Under **Auto Equip → Common settings → Replaced equipment**, choose **Store in the warehouse / Salvage / Sell**. Warehouse storage is the default. Only gear displaced by a successful replacement is processed, including both hands when equipping a two-handed weapon.

The **Warehouse full settings** shortcut opens **Bag & Cleanup → When storage is full**. Auto Equip and ordinary automatic cleanup share these rules:

| Setting | Choice | Behavior |
| --- | --- | --- |
| Admission | Sell the equipment with the lowest price | Sell the cheapest eligible warehouse item and store the incoming item |
| Admission | Sell the equipment with the lowest score | Use the shared gear score to choose the lowest eligible item |
| Admission | Sell the equipment stored earliest | Use warehouse deposit order, not acquisition order |
| Admission | Do not store the new equipment | Default: keep the incoming equipment in the bag |
| Repeat hunting | Pause hunting until the warehouse is cleared | Finish the current battle and hold the next while the warehouse is full |
| Repeat hunting | Continue hunting using the storage rule | Default: warehouse fullness alone does not stop repetition |

- Free slots in other unlocked tabs are used before any sale. Price/score ties use deposit order, then stable item ID.
- Locked, equipped, gemmed and any character's preset-referenced items cannot be automatically sold or salvaged. Existing enhancement, named effect, set and legendary-disposal protections also apply.
- If no safe sale is possible or currency/material limits reject disposal, the displaced item stays in the bag. Overflow tabs remain withdrawal-only; several items are never sold to admit one item.
- Pending warehouse storage survives saves and restarts. Retrying from the result screen after making space retries storage. While Auto Equip is enabled, grade-based cleanup does not sell or salvage pending items.
- Continue still respects bag space, goals and other stop conditions. Full-bag behavior is configured immediately below.
- Manual moves and drag/drop never trigger sales. Depositing or withdrawing and redepositing updates age; sorting and moving between warehouse tabs do not. Old saves initialize missing deposit history once using their existing warehouse list order.

## Score and shared details

Actual item details show **Gear score** below the name and rarity. Inventory, shop, storage, rewards and training share `ItemTooltip` and `ItemDetailView`, so the same item has the same score everywhere. Comparisons keep equipped gear on the left and candidates on the right, and show the score before and after replacement. Catalog range definitions do not show a fabricated rolled score.

The score is an intrinsic growth indicator, not a build-specific DPS prediction. It uses actual base/affix values, including level scaling, enhancement, awakening, greater affixes and masterwork through the existing stat owners. Rarity does not add a second bonus. Legendary/set powers, socketed gems and class/rune synergies are excluded.

`EquipmentScore.Version = 2`:

- Primary contribution: `100 × actual primary value / reference value`, multiplied by base attack-speed factor for attacking weapons.
- Fixed resistance: `25 × actual fixed resistance / 35`.
- Fixed base properties: `25 × actual fixed property / affix reference max for the same stat`. Includes material armor/dodge and shield block. Each intrinsic property and rolled affix contributes once.
- Each affix: `25 × actual value / affix definition max`. This denominator is the catalog reference, not the current item's level-adjusted maximum, so level growth remains visible.
- The total is rounded to one decimal for both display and score comparisons. Equal displayed scores do not trigger a score-based replacement.

| Primary stat | Reference |
| --- | ---: |
| Weapon attack | 20 |
| Head armor | 20 |
| Chest armor | 40 |
| Hand attack speed | 2 |
| Foot movement speed | 3 |
| Belt life | 160 |
| Necklace all resistance | 2 |
| Ring critical chance | 1 |

Weapon replacement compares the whole hand configuration. Dual-wield attack damage and base speed are averaged as in `HeroStats`; offensive off-hand attack is added. Shields use an armor reference of 24. Affixes and fixed resistance count once per equipped item; a two-handed weapon counts once. Therefore the **weapon loadout score** can differ from the sum of individual item scores.

When the master switch is OFF, all settings below it are dimmed and unavailable without losing their values. Replaced equipment uses an anchored three-entry list. The full-warehouse shortcut appears only for warehouse storage. The old equipment-score help control has been removed; shared item details still show the score.

## Protection and acquisition

- **Auto-equip exclusions** lists all ten physical positions with checkboxes. A checked position cannot be automatically replaced, filled or indirectly evicted by a two-handed weapon or incompatible off-hand change. Manual equipment is unaffected. Legendary/set-effect preservation is no longer a separate automatic replacement condition.
- Locked candidates and locked equipped items never auto replace. `EquipmentSlots.Plan` remains authoritative for class, level, hand compatibility, two-handed occupancy and bag capacity. Gems remain on displaced equipment.
- Natural loot, reward boxes, gambling/crafting and other `Economy.AddItem` acquisitions participate, along with new shop purchases. Evaluation occurs after ownership is established and equipment changes use `EquipmentSlots.Equip`.
- Real hunt pickups use `GameStore.CommitRecommendedLoot` to save acquisition, equipment, claimed flag and transaction receipt atomically. A save failure leaves ownership/equipment unchanged and pauses hunting for retry.
- Combat stats refresh after replacement without healing, resource refill or cooldown reset. Only values above the new maximum are clamped. Active potion effects are included in that maximum.

## Diablo references

The official [Diablo III 2.1.2 notes](https://news.blizzard.com/en-us/article/17561388/patch-2-1-2-now-live) describe Recovery as a combined estimate using healing, life and toughness, and Ancient legendary items with increased stat ranges. The official [Diablo IV Season 4 Loot Reborn article](https://news.blizzard.com/en-us/article/24077223/galvanize-your-legend-in-season-4-loot-reborn) emphasizes readable upgrades, simpler affixes, greater affixes and masterworking.

HELLSCRIPT adapts the idea of a readable growth indicator alongside detailed option comparisons. The weights and automatic-equipment policy here are HELLSCRIPT-specific, not a claimed official universal auto-equip formula from either Diablo game. Historical item-power caps are not treated as current rules.

## Ownership and compatibility

[EquipmentScore](../../Assets/HELLSCRIPT/Runtime/Core/EquipmentScore.cs) owns scoring. [EquipmentRecommendation](../../Assets/HELLSCRIPT/Runtime/Core/EquipmentRecommendation.cs) owns policy, while [GameStore.RecommendedEquipment](../../Assets/HELLSCRIPT/Runtime/Core/GameStore.RecommendedEquipment.cs) owns atomic hunt pickup.

`HeroSave.inventory` and `HeroSave.edict` hold live ownership/settings; `HuntEdictEditSession.Draft` owns edits; `CombatSimulation.Stats` holds combat-derived values. Details use copied items with an explicit `EquipmentViewSource` and never mutate saves. The existing Hunt Edict window receives a [tab adapter](../../Assets/HELLSCRIPT/Runtime/Presentation/HuntEdictWindow.Equipment.cs), retaining shared scrolling, fixed Save/Revert and theme. No independent content window is introduced.

Automatic storage and replacement disposal reuse [EquipmentAutomation.cs](../../Assets/HELLSCRIPT/Runtime/Core/EquipmentAutomation.cs), `EdictCleanupPolicy`, `Economy` and `Storage` transactions. `Item.warehouseOrder` and `AccountSave.warehouseSequence` persist deposit order; `Item.pendingAutoStorage` persists deferred storage.

Global edict version 5 contains 152 settings, adding physical-slot exclusions. The deprecated effect-preservation field remains known only for save/share compatibility. Versions 1, 2, 3 and 4 are validated and upgraded with safe new defaults while preserving previous choices. Old HED2/HED3/HED4/HED5 codes retain their original canonical-form and hash validation; unknown JSON fields are rejected. Scores are derived rather than duplicated in saved items.

## Initial recommendation validation

The following evidence covers commit `c866f140`, before warehouse handling. Extension validation is recorded separately below.

Verified with Unity 6000.6.0f1. After the Editor shut down, no MCP instance remained; verification used batch Unity against this project and the existing `ProjectBuilder.BuildMac` entry point.

- Related Edit Mode suite: **785 passed, 0 failed, 0 skipped**, including 45 recommendation tests for scores, option rules, hand combinations, locks, effect protection, old share codes, save failures and combat continuity. This is not a claim of a full-project suite pass. [Raw results](RecommendedEquipmentEvidence/edit-mode.xml)
- macOS development build succeeded with zero build errors. The native player passed 20 settings combinations and 10 detail/comparison combinations at 440×956, 956×440, 1600×900, 1600×1000 and 2100×900 in Korean and English. Settings used 100%/150% text; details used 150%.
- uGUI raycasts and pointer events exercised all three weapon modes, eight independent slots, option add/remove, draft/revert/save/reopen. The real loot loop equipped an upgrade, refreshed stats without healing or resetting cooldowns, and saved the claimed flag atomically. [Native checks](RecommendedEquipmentEvidence/runtime.txt)
- A fresh process restored rules, equipped gear and claimed flags. [Restart evidence](RecommendedEquipmentEvidence/restart.txt)
- Shared UI ownership and its nine tests passed. [Validation summary](RecommendedEquipmentEvidence/validation.json)

Representative captures: [portrait settings](RecommendedEquipmentEvidence/settings-440x956-ko.png), [landscape settings](RecommendedEquipmentEvidence/settings-956x440-en.png), [portrait comparison](RecommendedEquipmentEvidence/details-440x956-ko.png), [PC comparison](RecommendedEquipmentEvidence/details-2100x900-en.png).

Native acceptance used isolated account fixtures and synthetic pointer input. These are macOS results; physical touch, mobile devices and device performance were not tested. Feature-branch evidence does not establish a main merge or public wiki deployment.

## Warehouse extension validation

- **297 related Edit Mode checks passed**, with zero failures/skips, including 31 storage and 48 recommendation cases. Coverage includes disposal, all admission orders, protection/caps/overflow, deferred retries, manual re-equipping, rollback/idempotence and version 1–3 share migrations. [Raw results](RecommendedStorageEvidence/edit-mode.xml)
- After shortening the group summary, **53 overlapping UI/localization checks were rerun** successfully; these are not 53 additional unique tests. [UI recheck](RecommendedStorageEvidence/ui-edit-mode.xml)
- The macOS development build succeeded with zero errors and passed **57 native checks**, combining the existing recommendation/detail coverage with the new settings. Five aspect ratios, KO/EN, 100%/150% settings text and 150% details were checked. The 20 new setting combinations cover pointer choices, warehouse shortcut, text bounds, fixed Save/Revert and reopening. [Native checks](RecommendedStorageEvidence/runtime.txt)
- Real pickup transactions verified warehouse storage, sale, salvage, eviction, currencies and file reload. Repeat stop/continue and space-cleared conditions were checked. A fresh process restored settings, equipped gear, claimed flags and warehouse order. [Restart](RecommendedStorageEvidence/restart.txt), [summary](RecommendedStorageEvidence/validation.json)
- CoplayDev MCP reported no connected Editor, so validation used batch Unity in the isolated worktree and the existing macOS player workflow. Physical mobile input and device performance were not tested. Public wiki deployment remains a merged-main step.

Captures: [portrait disposal](RecommendedStorageEvidence/displaced-440x956-ko.png), [landscape disposal](RecommendedStorageEvidence/displaced-956x440-en.png), [portrait admission](RecommendedStorageEvidence/warehouse-admission-440x956-ko.png), [landscape admission](RecommendedStorageEvidence/warehouse-admission-956x440-en.png), [PC repeat settings](RecommendedStorageEvidence/warehouse-1600x1000-en.png).

## Rift recommendation offers and tab activation

See [Hunt Edict control refinement](Hunt_Edict_Control_Refinement.en.md) for per-part loot offers, saved acknowledgement, shared details/comparison and per-tab single-choice activation. Earlier evidence below retains its original historical scope; text-size variants are no longer part of acceptance.
