# Consumable reward boxes and rift first clears

Updated: 2026-09-24 · Implemented on the development branch

[Rift content unlocks](HELLSCRIPT_Rift_Content_Unlocks.en.md) now appear in the first-clear list and tier detail. Services unlock when the clear record is confirmed, independently of claiming boxes.

[한국어](HELLSCRIPT_Reward_Boxes.md) · [Drop economy](HELLSCRIPT_Rift_Rewards_1000.en.md) · [Runtime catalog](../../Assets/HELLSCRIPT/Resources/Data/RewardBoxes.json) · [All icons](../Art/RewardBoxes/contact-sheet.png)

There are **96 consumable reward boxes**. Open Reward Boxes from the inventory footer; inspect and claim packages from First-clear Rewards in rift entry. Boxes belong to the account. Equipment is generated for the class opening the box.

## Catalog

| Family | Definitions | Contents |
|---|---:|---|
| Rare equipment | 9 | Random equipment, or weapon/helm/chest/gloves/boots/belt/amulet/ring |
| Legendary equipment | 9 | The same choices with guaranteed legendary rarity |
| Special equipment | 3 | Class-set chest, awakened legendary helm, awakened legendary weapon |
| Fixed-color gems | 36 | Six colors × six tiers, ten identical gems per box |
| Choice gems | 6 | One per tier, ten gems of one explicitly selected color |
| Fixed currency | 17 | Stones 30/100/500/3000; materials 50/200/1500; gold 10000/20400/100000/1000000; coins 100/200/300/500/1000/2000 |
| Stage-scaled currency | 3 | Every-tier stones, every-ten-tier materials, every-fifty-tier stones |
| Equipment cores | 9 | Eight fixed slots and one slot-choice box, ten cores each |
| Runes | 4 | Five G0 one-cell types, or one G4/G5/G6 five-cell rune |

Random equipment selects each of eight slots with 12.5% probability. Fixed-slot boxes remain in that slot. Weapon boxes exclude shields, orbs, arrows and other offhands. Legendary identity uses the existing released, class-compatible weighted pool. Sets are part of legendary rarity. The set box randomly selects a released chest set item for the opening class. Unreleased expansion definitions are excluded.

Item level is fixed from the source rift progression curve, capped at 60; delaying opening does not improve it. The box RNG and opening sequence persist. Results become owned only after the save commits. Awakened boxes guarantee awakening, with no additional greater-affix guarantee. Minimum quality applies to **affix rolls**, not fixed base stats.

## Account-once rewards

Every tier R grants a box with `2 + floor(R/50)` stones. Multiples of ten add `20 + floor(R/20)` materials; multiples of fifty add `50 + floor(R/2)` stones. Milestones below add their packages.

Every new box package is once per account per exact tier. A normal-clear best-time record from any hero proves eligibility. Boss-defeat-only abandonment flags, a highest-tier value and sweeps do not. Existing exact normal-clear records remain eligible; an old stage with no such record can be completed again.

Existing character-once automatic first-clear gold `1000+100R` and materials `10+floor(R/5)` are not paid again on claiming boxes. The broader proposal's reduced material formula `10+floor(R/10)`, repeat-drop quantities and the full enemy curve remain proposed. The R1–5 tutorial tuning and R30 rarity gate are implemented; see [early progression](HELLSCRIPT_Early_Rift_Balance.en.md).

| Rift | Additional boxes |
|---|---|
| 1 | Rare weapon, 50% minimum affix quality |
| 3 | 50 materials |
| 5 | 30 stones |
| 10 | Two T1 gem-choice boxes; 100 coins |
| 15 | Five G0 one-cell rune types |
| 30 | Legendary weapon, 50% minimum affix quality; one T2 gem-choice box |
| 35 | 20,400 gold for the newly opened reroll service |
| 50 | 200 coins |
| 80 | 200 materials; 10,000 gold |
| 100 | Awakened legendary helm, 60% minimum affix quality; T3 gem-choice box; 300 coins |
| 120 | Ten selected-slot cores |
| 200 | Random released class-set chest, 60% minimum affix quality; 500 coins |
| 300 | T4 gem-choice box; G4 five-cell rune; 500 coins |
| 500 | T5 gem-choice box; G5 five-cell rune; 1,000 coins |
| 650 | G6 five-cell rune |
| 750 | T6 gem-choice box; 1,000 coins |
| 1000 | Awakened legendary weapon, 90% minimum affix quality; two T6 gem-choice boxes; 1,000,000 gold; 1,500 materials; 3,000 stones; 2,000 coins |

Across tiers 1–1000, these boxes yield **five equipment items (one rare, four legendary), 90 gems, eight runes, ten cores, 1,030,400 gold, 6,250 materials, 20,800 stones and 5,600 coins**. Existing automatic rewards, repeat drops, salvage and offline income are separate.

The two-hour, 180-day forecast does not spend these coins. Buying fatigue or speed changes that scenario. The reference gem allocation uses ten diamonds and ten rubies at R10, then diamonds for later choice packs; no pack is counted for multiple colors.

## Persistence and presentation

Schema 12 adds `AccountSave.rewardBoxes`, owning remaining boxes, RNG state, opening counters and claimed tiers. The old hero-local reserved presentation field does not own new claims. `GameStore.ClaimRiftFirstRewards` and `GameStore.OpenRewardBox` stage changes, validate, write the save and then adopt state. Request identity binds hero, box, count and choice. Open one to ten boxes at once. Insufficient gear/gem capacity, overflow or disk failure cancels the entire operation, preserving boxes. Claiming and opening require being outside a suspended rift.

Unknown box definitions/versions or malformed current-schema state stop loading while preserving the original file rather than silently rolling back to a stale backup. See [domain tests](../../Assets/HELLSCRIPT/Tests/Editor/RewardBoxTests.cs) and [runtime acceptance](../../Assets/HELLSCRIPT/Runtime/Presentation/RuntimeRewardBoxSmoke.cs).

All 96 icons are original code-authored vectors extending the existing metal-and-wood chest style. Rarity accents read `EquipmentGradePalette`. No image generation model was used. Editable SVGs are under `Docs/Art/RewardBoxes`; 256×256 RGBA sprites are under `Assets/HELLSCRIPT/Resources/Art/RewardBoxes`. The [manifest](../Art/RewardBoxes/manifest.json) records hashes and transparent pixels; the [authoring script](../../tools/generate_reward_boxes.py) exports both formats from the same primitives.

The UI uses `ContentWindowView` and `StoreViewBinding`, a pooled 1,000-tier list, five filters, eight boxes per page, and shared `ItemDetailView` with `RewardSnapshot` for equipment results. [Verification evidence](../Implementation/RewardBoxEvidence/Reward_Box_Verification.md) separates branch implementation, macOS testing, physical-device coverage and publication status.
