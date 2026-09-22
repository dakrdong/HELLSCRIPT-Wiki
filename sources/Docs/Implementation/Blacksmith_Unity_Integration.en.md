# Blacksmith resources and Unity integration

Date: 2026-09-22

[한국어](Blacksmith_Unity_Integration.md)

## Scope and ownership

The existing town blacksmith opens the native uGUI forge through the interaction button or PC E key inside the existing 2.8 m range. Affix reroll, slot upgrades and equipment enhancement use actual `GameStore` equipment, character progression and account currency. Crafting retains its existing screen. Awakening, masterworking, sockets and invested-material ledgers remain intact. Persistence uses the project's existing local development adapter.

Integration starts from the committed native inventory and two-weapon-slot work on `codex/inventory-play-shortcut`. Uncommitted files in the user's original checkout are preserved. Browser demonstration items, currency and storage are never imported into the account.

## Resources and responsive UI

Reference sizes are portrait 440×956, landscape 956×440, and PC 16:9, 16:10 and 21:9. Landscape has three exactly equal columns. Portrait list/detail navigation slides horizontally for 0.2 seconds. Dialogs fill the safe-area frame at a constant size with internally scrolling content. Landscape growth details put five milestone rows on the left, and level input, slider and vertical milestone buttons on the right. Milestone buttons show a three-second unlock tooltip. Busy workstation indicators pulse over a 1.5-second cycle.

Both item tabs share the inventory's grade palette for lists, details and dialogs. Unique effects do not create a separate grade. Korean and English use the existing language and reading-scale settings.

The [resource manifest](../../Prototypes/Blacksmith/UnityResources/manifest.json) records sources, hashes and import settings for 11 slot emblems, 16 function icons and 3 currency icons. Thirty alpha-validated RGBA PNGs are rendered from repository SVG sources, without new AI-generated or downloaded art. SVG sources are in `Prototypes/Blacksmith/UnityResources/`; Unity images are in `Resources/Art/Blacksmith/`. `BlacksmithArtImporter` applies Default Texture2D, transparent alpha, no mipmaps, Clamp, Bilinear, no compression and a 512 px maximum. Regenerate with `node tools/blacksmith/export-resources.cjs`.

Equipment uses the existing equipment atlas; the character uses the selected class's existing `CharacterFigure`. Empty slot emblems use opacity 0.38. Existing `StorageSurface` panels implement selection, disabled, locked and busy states. Main colors are background `#191c16`/`#10120f`, panel `#24271d`/`#191c16`, accent `#d8b879`, body `#e7e5d7`, muted `#9b9e8e`. See the Korean specification for the shared grade palette and the manifest for exact assets.

The handoff includes [Unity growth data](../../Assets/HELLSCRIPT/Resources/BlacksmithSlots.json), [balance calculations](../../Prototypes/Blacksmith/UnityResources/balance-reference.json), [layout styles](../../Prototypes/Blacksmith/UnityResources/styles.json) and the [English text table](../../Assets/HELLSCRIPT/Resources/Localization/en.txt). The [prototype README](../../Prototypes/Blacksmith/README.md) covers the standalone HTML reference.

## Affix reroll

Use actual `ItemGenerator.RerollPool` and `Reroll` rules: equipment part, affix side, duplicate groups, class weights and quality ranges. Only a successfully paid roll permanently sets `rerollSlotId`. The existing fixed price is `50 × L × (L+9)` gold; repeat count does not increase it.

Automatic reroll stops immediately on any selected target. The result shows the affix, value and possible range, with stop or continue using remaining attempts. Waiting never spends again. Closing, leaving NPC range or exiting stops automation; every successful individual roll is already saved. Automation does not resume after relaunch. The locked summary is inline, e.g. `Attack Speed +7.76% (95.2%)`, using `(value−minimum)/(maximum−minimum)×100`; a fixed range is 100%.

## Equipment enhancement

Maximum +100; instant guaranteed success; gold only. Per-step increases are attack +2 for weapons/non-shield off-hands, armor +2 for helmets/shields, armor +3 for chest armor, attack speed +0.08 percentage points for hands, movement speed +0.05 points for feet, maximum life +8 for belts, all elemental resistance +0.04 points for necklaces, and critical chance +0.02 points for rings. Awakening and masterwork multiply only the unenhanced base, followed by the flat enhancement bonus.

For item level `L` and destination enhancement `N`, cost is `ceil((100L+10L²) × (1+0.05(N−1)+0.0025(N−1)²))`, evaluated with checked integer arithmetic. Quotes and transactions share `GearEnhancement`. Bundles sum individual steps. +10 requires the whole sum, truncated only by the +100 cap. Max purchases the affordable consecutive prefix. There is no cost-table button or dialog.

A Lv.30 item costs 12,000 for +1, 365,430 for the +100 step, 155,550 for +0→+10, and 14,020,500 total for +0→+100. Starting with 3,000,000 gold, +10 followed by Max reaches +53 with 90,300 remaining.

## Slot upgrades

`Resources/BlacksmithSlots.json` is the Unity progression source. Bonuses begin on their unlock level: `rate × (level−unlock+1)`; `once` rewards apply once. All ten slots start at Lv.1 and cap at Lv.100, with new bonuses at 25/50/75 and a final reward at 100. They apply without equipment, including the empty off-hand of a two-handed weapon.

| Slot | Lv.1 bonus at cap | Lv.25 bonus at cap | Lv.50 bonus at cap | Lv.75 bonus at cap | Lv.100 reward |
| --- | --- | --- | --- | --- | --- |
| Main hand | Attack 500 | Crit chance 7.6 pp | Crit damage 25.5 pp | Attack speed 5.2 pp | All skills +1 |
| Off hand | Armor 300 | All resistance 7.6 pp | Damage reduction 4.08 pp | Maximum life 5.2% | Life regeneration 5/s |
| Head | Armor 200 | Resource 38 | Cooldown reduction 5.1 pp | Control duration reduction 5.2 pp | All attributes 20 |
| Chest | Life 2,000 | Armor 228 | All resistance 5.1 pp | Damage reduction 2.6 pp | Maximum life 10% |
| Hands | Attack 200 | Attack speed 7.6 pp | Crit chance 2.55 pp | Crit damage 7.8 pp | Lucky hit 5 pp |
| Feet | Armor 150 | Movement speed 7.6 pp | Dodge 4.08 pp | Stamina 13 | Stamina regeneration 2/s |
| Neck | All attributes 50 | All resistance 7.6 pp | Cooldown reduction 5.1 pp | Resource 13 | Maximum life 5% |
| Waist | Life 1,000 | Healing received 15.2% | Life regeneration 5.1/s | Resource cost reduction 2.6 pp | Potion healing 25% |
| Left ring | Attack 150 | Crit chance 3.8 pp | Crit damage 15.3 pp | Attack speed 2.6 pp | All attributes 15 |
| Right ring | Resource 20 | Resource generation 3.8/s | Resource cost reduction 5.1 pp | Lucky hit 3.9 pp | Cooldown reduction 3 pp |

Progress is per character. The account shares two free workstations and sequential permanent unlocks costing 10/50/250 diamonds. A character cannot have duplicate jobs for one slot.

Destination-level cost/time starts at 10 stones and 5 minutes for 1→2. Add 5 stones/5 minutes per destination level 3–24, 10/10 minutes at 25–49, 20/30 minutes at 50–74, 50/1 hour at 75–99, and 500/24 hours at 100. The [full table](../../Prototypes/Blacksmith/slot-enhancement-costs.csv) is included.

One slot from Lv.1 to 100 costs **64,115 stones** and **54 days 50 minutes** of work. The final 99→100 step costs **2,620 stones** and **2 days 19 hours 35 minutes**. All ten total **641,150 stones** and **540 days 8 hours 20 minutes**. With uninterrupted work and sufficient materials, two stations take **270 days 4 hours 10 minutes**, five take **108 days 1 hour 40 minutes**; actual completion is longer when jobs are not immediately started.

Starting deducts stones and saves a UTC completion timestamp. Completion settles exactly once, including while away or offline. Finish Now recalculates `floor(max(0,finishUTC−nowUTC)/60)` diamonds at click time; 59 seconds or less is free. A battle retains its entry snapshot through equipment changes, build changes, level gains and reload. Completed growth applies to the next battle.

## Enhancement-stone acquisition

`AccountSave.enhancementStones` is separate from materials, shared by the account, initially zero. Salvage awards common 0, magic 1, rare 5, set 10, ordinary legendary 10, and non-set catalog-unique-effect equipment 15. Check set membership first, then `ItemCatalog.Unique(item.special)`; never add legendary and unique rewards together. Single, bulk and automatic salvage share `BlacksmithCatalog.SalvageStones`; existing materials, cores and invested-material refunds remain intact. Bosses and sweeps grant `10+2×reward stage`, using the actual played stage or the existing `highestClear` sweep stage.

## Persistence and combat integration

Schema 8 retains old enhancement +5 as new +5 and recalculates its base ability. Affix locks, sockets, awakening, masterwork, affix boosts and investment/refund records survive. Masterwork requires enhancement **at least** +5.

Account state adds stones, versioned workstation state and jobs. Character state adds a versioned ten-level slot array. Jobs contain ID, hero ID, slot ID, workstation, source/target levels, paid stones and UTC start/finish timestamps. Missing legacy fields initialize to zero stones, two workstations, no jobs and Lv.1 slots. Invalid levels, duplicate jobs, unsupported future versions and inconsistent timestamps are rejected.

Quotes retain full item fingerprints, source/target levels and total price. `GameStore.Transact` rechecks ownership, unlock, town state, current balance and quote before one atomic save. Request receipts prevent duplicate application. Timed settlement uses completed job IDs. Failed saves retain original data.

`AllResistancePercent` is distinct from rating-based resistance. It adds to elemental mitigation under the existing 70% cap and does not affect physical armor. Attribute sheets and equipment comparison share the same formulas. Main-hand Lv.100 adds one effective rank to learned actives and equipped passives, while invested ranks remain capped at five and point spending remains unchanged. Effective rank calculations allow six. Rune bonuses remain separate.

Follow the inventory's [shared equipment comparison contract](../Design/Equipment_Comparison_Rules.en.md). `ItemComparison` uses the forge's base-stat identities and units; `EquipmentComparisonView` and `ItemTooltip` display the same values in inventory, shops and storage. Inventory previews during combat use that battle's frozen slot levels. Shared auto-selection settings and salvage protection checks remain intact.

## Validation and publication

See [validation record](Blacksmith_Validation.en.md) for executed checks and limitations. HTML checks, Edit Mode tests and native macOS development-player input/save/screenshots are separate evidence. Desktop windows at mobile ratios and synthetic Unity inputs are not physical-mobile touch verification.

Publish the wiki only after this branch is merged to `main`. The merging task performs generation, checks, public deployment and logged-out verification; never publish from this feature branch.
