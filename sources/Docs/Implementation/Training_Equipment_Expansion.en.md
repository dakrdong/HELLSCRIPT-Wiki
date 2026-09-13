# Owned equipment in A/B training

Updated: 2026-09-13 · [한국어](Training_Equipment_Expansion.md)

Status: desktop implementation and validation passed: 2,515 full-suite tests, native initial/restart checks and 13 reviewed screenshots. Physical mobile validation remains outstanding.

## Owned equipment and isolated drafts

The training A/B path now lets the player select B equipment after completing A. It captures the selected hero's bag and the shared warehouse at comparison start. Only those owned item IDs are eligible; class, required level and one item per slot are enforced. Empty slots are permitted. Items in another hero's personal bag are excluded. Using gear acquired later requires a new comparison.

Full item values are frozen, including affixes, enhancement, awakening, greater affixes, masterwork, gems and protection flags. Selecting warehouse gear borrows a copy without moving it or consuming live bag capacity. Reading an item does not mark the actual item as reviewed. Narrow windows switch between list and detail; wide windows show both. Fixed footer actions, draft preservation and Korean/English strings follow the screen-layout design.

## Actual combat and active behavior

The old A/B account constructor omitted account rune boards. Both attempts now use the same captured boards, and a changed weapon uses its own board's contributions. Later live inventory or rune changes cannot replace this baseline. Attempts start with fresh HP, resources, cooldowns and effects in the same seeded environment.

The growth screen can no longer open rune editing during a comparison. The rune menu shows fixed conditions and restores the previous pause state on return. The rune service also rejects comparison mutations before saving the live account or replacing combat stats. Ordinary training and rift rules are preserved.

Heroes using hunt edicts edit the actual B edict through the existing editor. Legacy users retain behavior and skill editing. Both can edit passives. Inactive legacy settings are not described as active edict behavior changes. Future skills already present in the default edict remain inactive until unlocked; newly assigning a locked skill is rejected. In-run equipment/level/edict mutation and abandonment invalidate the attempt.

## Recorded evidence and explicit reuse

Version 2 stores each attempt's complete equipped items, build, active edict and the shared rune snapshot. Results distinguish equipment changes from behavior changes and retain item values even if live items later change. Version 1 keeps its original interpretation without inventing missing rune data. Incomplete or unsupported records are preserved and shown as unavailable.

Saving B to a build preset now references B's actual item IDs; previously the controller always used A's IDs. Saving never equips borrowed warehouse items. Subsequent preset loading checks ownership and missing items. Existing presets hold equipment references, passives and legacy behavior; they do not archive a full hunt edict. A separate action loads the selected edict into the actual editor for explicit review and application.

## Validation and limits

After the rune guard fix, all 100 focused equipment/rune/localization checks passed again, including rejected comparison writes and preserved ordinary-training behavior.

The 118 focused checks cover frozen ownership/runes, slot/class/level guards, agreement with independently equipped combat runs, legendary/set/quality/gem snapshots, active edicts, historical records and localization. Initial fixture errors and a serialization rounding assertion were corrected and retained in validation logs. The final full suite passed 2,515 tests. Build8 passed initial and independent restart runs using topmost-raycast-checked UGUI events. Equipment selection/cancellation, active edict changes, explicit result and B-equipment preset saving, real-edict preview/discard, KO/EN, 140% text and portrait/wide/short windows were checked in 13 screenshots. A clipping defect after text scaling was fixed. The final full suite, focused rune regression, native build and current owned sources have identical hashes.

Native trials retain the current 1x player policy. Three simulation speeds are tested internally. This is controlled owned-equipment validation, not proof of natural progression balance, physical Android input, prolonged device behavior or new-player comprehension. Broader evasion analysis and real-session farming efficiency remain separate work.

Evidence: [full-suite XML](../../Artifacts/Validation/training-equipment-editmode.xml), [validation summary](../../Artifacts/Validation/TrainingEquipment/validation-summary.json), [source hashes](../../Artifacts/Validation/TrainingEquipment/validated-source.json), [large-text equipment screenshot](../../Artifacts/Validation/TrainingEquipment/Native5/10-saved-equipment-en-large.png).
