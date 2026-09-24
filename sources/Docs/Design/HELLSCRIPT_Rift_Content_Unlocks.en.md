# Rift-based content unlock schedule

Updated: 2026-09-24 · Implemented on the development branch

[한국어](HELLSCRIPT_Rift_Content_Unlocks.md) · [Runtime catalog](../../Assets/HELLSCRIPT/Resources/ContentUnlocks.json) · [First-clear boxes](HELLSCRIPT_Reward_Boxes.en.md) · [Combat balance proposal](HELLSCRIPT_Balance_1000.en.md)

The first-clear reward list now shows which gameplay services unlock at each rift tier. `ContentUnlocks.json` owns the schedule used by descriptions, entry screens and actual transactions.

| Rift cleared | Content | Purpose and remaining requirements |
|---:|---|---|
| Start | Training, equipment management, Hunt Edict, basic recovery potions, selling, salvaging and storage | Keep essential controls available. Skills retain their hero-level and tree prerequisites. |
| 1 | Equipment enhancement and offline rewards | Introduce the first gear improvement. Gold is required; offline accrual begins at activation. |
| 3 | Slot-targeted rare crafting | Fill missing equipment slots using the existing gold and material costs. |
| 5 | Equipment-slot upgrades | Persistent growth independent of gear replacement. Stones and a free workstation are required. |
| 10 | Gem purchase, sockets, fusion and downgrade | Align with the first legendary weapon and gem boxes. Fusion remains 5:1; downgrading returns five of the preceding tier. |
| 15 | Rune placement, reshape, fusion, purchase and weapon mastery | Introduce boards; kill-based mastery XP starts here. |
| 25 | Unidentified equipment purchase | Add targeted random purchases; the basic equipment shop remains available from the start. |
| 35 | Affix rerolling | Improve useful affixes after the initial gear churn, retaining one-line locking and gold costs. |
| 40 | Aspect upgrades and imprinting | Apply collected legendary powers to equipped gear; salvage collection works earlier. |
| 60 | Sweep and gem-potion crafting | Add repeat rewards and consumables. Sweeps still require the selected hero's clear record and daily allowance; potions consume matching gems. |
| 80 | Masterworking | Requires enhancement +5, materials, gold and the account masterwork cap. |
| 120 | Targeted legendary/set core crafting | Spend ten same-slot cores to fill build gaps. |

The catalog contains 14 permanent service entitlements. R100 awakened boxes, R200 set boxes, R300/500/650 rune boxes and R750/1000 gem boxes are item milestones, not new menu gates. Natural awakening and rune drop curves, masterwork caps and combat stats remain outside this change.

Latest update: [introductory rifts and rarity gates](HELLSCRIPT_Early_Rift_Balance.en.md). Unidentified purchases open at R25 and can award Legendary/Set items; natural rift rewards begin at R30; rerolls open at R35.

## First-clear popup

Each pooled row shows box quantity, claim state and services unlocked at that tier. The detail view shows current content access, instructions, the next unlock and the box rewards. Content unlocks automatically when the account clear record is confirmed; claiming a box is not required.

The existing combat owner records `highestClear` at boss reward settlement. Training and sweep do not advance this record. Existing in-combat and portal restrictions still apply. Box claims separately require an exact normal-finish best-time record, so abandoning post-boss loot collection may unlock services while leaving the box unclaimable until a normal completion. The popup displays these two states separately.

## Persistence and execution

New accounts cannot bypass R10 gems or R120 core crafting by acquiring stock early. Stock is retained. Runes and aspects can be collected early; rune editing and mastery XP begin at R15, aspect upgrades and imprinting at R40. Access is shared across heroes, while sweep eligibility and reward tier remain hero-specific.

Unlock v1 migrates once to v2, preserving released stage privileges, early gem/core entitlements, initially available rune/aspect services, slot/masterwork access formerly coupled to enhancement, and potion crafting formerly coupled to gems. No equipment, currency, rune, aspect or preset is removed. The original save is archived with `.before-content-unlocks-v2.json`; schema 13 rejects missing or unsupported current unlock versions without overwriting the source. Guide acknowledgments remain separate from access.

The existing `ContentUnlocks`, `GameStore`, `ContentServices`, `Jeweler`, `ItemQuality`, `AspectStone` and `RuneGrowth` owners enforce the gates. Locked operations do not spend stock or mutate layouts or RNG. Forge tabs explain their locks; rune/aspect entry points use the same condition text.

[Verification evidence](../Implementation/RiftUnlockEvidence/Rift_Content_Unlock_Verification.md) records executed tests and screenshots. This does not claim physical-mobile tests or a natural R1000 playthrough. Main integration and public wiki deployment occur after branch integration.
