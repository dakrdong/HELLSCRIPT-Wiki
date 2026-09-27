# Prepared operational items and rewards

Updated: 2026-09-26 · [한국어](Operations_Rewards.md)

This is a functional adaptation of operational item families from Diablo Immortal, II, III and IV to HELLSCRIPT's actual ownership and transaction model. It is not a copy of every weapon or temporary seasonal item, nor an exhaustive inventory of the latest 2026 seasons. The dated official examples below establish patterns, not current prices or drop rates. Names, artwork and reward amounts are HELLSCRIPT definitions.

## Implementation and activation

There are **148 usable reward-box definitions**: the original 96 plus 52 new wrappers (8 base potions, 36 gem elixirs, 2 equipment slot choices, 3 missing set slots and 3 lower rune grades). These wrappers reuse existing effects. **22 explicit grant packages** are prepared; their quantities are initial templates, not a finalized economic balance. **30 reserved concepts** record missing consumers and are rejected by the grant API. They do not have implemented inventories or spending behavior.

Nothing distributes automatically. Attendance, default drops and first-clear schedules stay as they were. A package named daily or weekly does not implement eligibility, scheduling or a daily cap. Its owning campaign must supply those rules.

## Research-to-game decisions

| Function | Reference pattern | HELLSCRIPT decision |
| --- | --- | --- |
| General currency | Gold across the four games | Reuse the gold wallet and boxes. |
| Reward/exchange currency | Immortal Hilts/event exchange; D3 Blood Shards | Reuse free Abyssal Coins for immediate awards; reserve dedicated event and merit tokens. |
| Purchase and market balances | Immortal Platinum/Orbs; D4 Platinum | Reserve separate ledgers; free coins are not purchase credit. |
| Crafting supplies | D3 act materials; D4 salvage; Immortal scrap/dust | Reuse Ancient Keystones, enhancement stones and slot cores. |
| Gems and runes | D2 gems/jewels/runes; D3 conversion; Immortal runes | Reuse six gem families and the existing rune board; add G1–G3 boxes. No new jewel or runeword subsystem. |
| Potions | D2 restorative potions; D4 elixirs | Wrap all 44 existing potions; reuse the potion/effect owners. |
| Gear and sets | D2 sets/uniques; D3 caches; Immortal event gear | Add explicit slot selection and missing set slots. Affixes, powers and set identity still use the existing generator. |
| Power collection | D3 extraction; D4 aspect codex | Retain the current aspect owner. |
| Refinement and recipes | D2 sockets; D3 cube; D4 tempering/masterworking | Reserve vouchers, catalysts and manuals until current transactions can consume them safely. |
| Deterministic acquisition | Immortal pearls and event exchanges | Existing cores already serve part of this role; reserve a distinct fragment only if justified. |
| Keys and summoning | D2 keys/organs; D3 rift keys/machines; D4 summons; Immortal keys/crests | Reserve challenge keys, boss seals/fragments, vault keys and modifier sigils. Require admission/refund and reward ownership. |
| Seasons and journeys | D4 Favor/Ashes; Immortal pass points | Reserve seasonal tokens, journey points and blessings until expiry, rollover and claim rules exist. |
| Returning players | Immortal returner rewards | Prepare a returning supply package; do not invent eligibility or grant automatic levels. |
| Convenience consumables | D2 repair/scrolls/ammo; D4 reset scrolls | Do not add artificial costs to free features. Reserve time/capacity vouchers for existing transactions. |
| Additional power systems | D2 charms; Immortal legendary gems/familiars | Reserve concepts pending comparison with runes/aspects and explicit combat owners. |
| Cosmetic/social rewards | Immortal/D4 cosmetics; HELLSCRIPT proposals | Reserve cosmetics and titles; guild/PvP tokens are proposals, not implemented modes. |
| Operational compensation | Adapted reward-packaging patterns | Prepare maintenance, incident, participation, completion and anniversary packages. These are original templates. |

## Use and ownership

The source catalogs are [RewardBoxes.json](../../Assets/HELLSCRIPT/Resources/Data/RewardBoxes.json) and [OperationsRewards.json](../../Assets/HELLSCRIPT/Resources/Data/OperationsRewards.json). Run `python3 tools/operations_rewards.py list` or `check`.

```sh
python3 tools/operations_rewards.py preview --package maintenance-v1 --stage 1
```

This prints a review-only first-clear row. It neither publishes nor grants anything. First-clear rewards remain account-once per stage: they cannot deliver maintenance compensation to players who already claimed that stage. New first-clear configurations require the updated client and server catalog to be deployed together. Do not publish new box IDs to old clients first.

Domain code can call `GameStore.GrantOperationsPackage(deliveryId, packageId, sourceStage)`. Use a stable entitlement ID such as `maintenance-20260926`, never a new random ID per retry. Retries cannot duplicate a delivery; reusing the ID with a different package, payload or stage fails. Stage 1 is the baseline for new accounts. Higher delivery stages cannot exceed the account's highest recorded normal clear, and each package enforces its minimum stage, including the stage-30 legendary gate. Suspended runs block grants and opening.

A single staged save commits boxes and the delivery receipt before notifying views. Delivery can wait in box storage; opening validates bag/gem capacity and the 9,999 potion cap. Failure preserves all inputs. The receipt survives fully consumed boxes. Account wallets/gems/runes retain their existing owners. The opening hero receives gear and potions; granting potions does not equip or consume them. Existing generation, quality and shared detail UI remain authoritative. Keep issued box and revisioned package definitions immutable; add a new `-v2` package when its promise changes.

This is a **local account transaction**, not a remote operator or mass-delivery API. It does not implement authenticated mail, coupons, server eligibility, cross-device deduplication or purchase receipts. Google sign-in does not make local grants authoritative. Those release paths require server authorization and an entitlement ledger.

## Ready packages

| ID | Purpose | Minimum delivery stage | Box grants |
| --- | --- | ---: | --- |
| `welcome-v1` | New Adventurer Supplies | 1 | `gold-10000` × 1; `potion-ph01` × 1; `potion-pm01` × 1 |
| `daily-supplies-v1` | Daily Supplies | 1 | `stones-30` × 1; `potion-ph01` × 1 |
| `weekly-supplies-v1` | Weekly Supplies | 10 | `gold-10000` × 1; `stones-100` × 1; `gem-choice-t1` × 1 |
| `maintenance-v1` | Maintenance Compensation | 1 | `premium-100` × 1; `potion-ph01` × 1 |
| `incident-v1` | Incident Compensation | 1 | `premium-300` × 1; `gold-10000` × 1; `stones-100` × 1 |
| `returning-v1` | Returning Adventurer Supplies | 10 | `rare-slot-choice` × 1 (quality ≥ 50%); `gem-choice-t1` × 1; `potion-ph01` × 2; `potion-pm01` × 2 |
| `event-participation-v1` | Event Participation Rewards | 1 | `premium-100` × 1; `gold-10000` × 1 |
| `event-completion-v1` | Event Completion Rewards | 30 | `legendary-slot-choice` × 1 (quality ≥ 50%); `premium-300` × 1 |
| `anniversary-v1` | Anniversary Gift | 10 | `premium-500` × 1; `gem-choice-t2` × 1; `rare-slot-choice` × 1 (quality ≥ 60%) |
| `blacksmith-v1` | Blacksmith Supplies | 30 | `materials-200` × 1; `stones-100` × 1; `core-choice` × 1 |
| `gem-apprentice-v1` | Gem Apprentice Supplies | 10 | `gem-choice-t1` × 2 |
| `gem-adept-v1` | Gem Adept Supplies | 100 | `gem-choice-t3` × 1 |
| `rune-apprentice-v1` | Rune Apprentice Supplies | 15 | `rune-starter` × 1; `rune-g1` × 1 |
| `rune-adept-v1` | Rune Adept Supplies | 100 | `rune-g2` × 1; `rune-g3` × 1 |
| `legendary-hunt-v1` | Legendary Hunt Rewards | 30 | `legendary-slot-choice` × 1 (quality ≥ 50%); `stones-100` × 1 |
| `class-set-v1` | Class Set Supplies | 200 | `set-helm` × 1 (quality ≥ 60%); `set-body` × 1 (quality ≥ 60%); `set-gloves` × 1 (quality ≥ 60%); `set-boots` × 1 (quality ≥ 60%) |
| `combat-potions-v1` | Combat Potion Supplies | 1 | `potion-ph01` × 2; `potion-pm01` × 2; `potion-pu04` × 1 |
| `utility-potions-v1` | Utility Potion Supplies | 1 | `potion-pu01` × 1; `potion-pu02` × 1; `potion-pu03` × 1; `potion-pu04` × 1; `potion-pu05` × 1; `potion-pu06` × 1 |
| `elixir-t1-v1` | T1 Complete Elixir Supplies | 10 | `elixir-g01-t1` × 1; `elixir-g02-t1` × 1; `elixir-g03-t1` × 1; `elixir-g04-t1` × 1; `elixir-g05-t1` × 1; `elixir-g06-t1` × 1 |
| `elixir-t3-v1` | T3 Complete Elixir Supplies | 100 | `elixir-g01-t3` × 1; `elixir-g02-t3` × 1; `elixir-g03-t3` × 1; `elixir-g04-t3` × 1; `elixir-g05-t3` × 1; `elixir-g06-t3` × 1 |
| `elixir-t6-v1` | T6 Complete Elixir Supplies | 750 | `elixir-g01-t6` × 1; `elixir-g02-t6` × 1; `elixir-g03-t6` × 1; `elixir-g04-t6` × 1; `elixir-g05-t6` × 1; `elixir-g06-t6` × 1 |
| `milestone-v1` | Progression Milestone Rewards | 30 | `legendary-slot-choice` × 1 (quality ≥ 50%); `gem-choice-t2` × 1; `rune-g1` × 1 |

## Reserved concepts and required consumers

These cannot be granted. Implement and verify the consumer before introducing a new executable definition.

| ID | Item | Required work |
| --- | --- | --- |
| `event-token-reserved` | Event Seal | Requires an event exchange, limits and an end-of-event policy. |
| `season-token-reserved` | Season Token | Requires season ownership and end-of-season settlement. |
| `honor-token-reserved` | Merit Medal | Requires merit eligibility and exchange limits. |
| `guild-token-reserved` | Guild Contribution Token | Requires guild ownership and contribution validation. |
| `pvp-token-reserved` | Arena Token | Requires server-validated PvP results and an exchange. |
| `market-credit-reserved` | Market Credit | Requires a market, escrow, fees and reversal handling. |
| `paid-credit-reserved` | Purchased Currency | Requires purchase receipts, refunds and separation from free grants. |
| `rift-key-reserved` | Challenge Rift Key | Requires a keyed activity and failed-entry refunds. |
| `boss-key-reserved` | Boss Seal | Requires boss summoning and participant reward ownership. |
| `boss-fragment-reserved` | Boss Seal Fragment | Requires fragment conversion and a boss-seal consumer. |
| `treasure-key-reserved` | Vault Key | Requires a vault activity and reward settlement. |
| `rift-modifier-reserved` | Rift Modifier Sigil | Requires admission snapshots and a reward modifier contract. |
| `guarantee-fragment-reserved` | Guaranteed Crafting Fragment | Must establish a distinct role from existing equipment cores. |
| `reforge-voucher-reserved` | Affix Reforge Voucher | Requires a cost-substitution contract in existing reforge transactions. |
| `socket-voucher-reserved` | Socket Voucher | Requires existing socket protection and cost checks. |
| `upgrade-catalyst-reserved` | Awakening Catalyst | Requires awakening acquisition and probability guarantees. |
| `crafting-manual-reserved` | Crafting Manual | Requires account recipe unlocks and duplicate handling. |
| `charm-reserved` | Guardian Charm | Requires a role distinct from existing runes and aspects. |
| `legendary-gem-reserved` | Legendary Gem | Requires explicit combat and progression rules beyond normal gems. |
| `respec-voucher-reserved` | Skill Reset Voucher | Do not charge for current free editing; revisit only if a paid reset exists. |
| `fatigue-voucher-reserved` | Exploration Time Voucher | Requires an explicit contract with current recovery limits and carryover. |
| `speed-voucher-reserved` | Focused Exploration Voucher | Requires integration with 1.5x admission cost and cancellation. |
| `forge-voucher-reserved` | Forge Time Voucher | Requires existing forge completion and duplicate-use protection. |
| `stash-voucher-reserved` | Stash Expansion Voucher | Requires a transaction consistent with already purchased capacity. |
| `xp-boost-reserved` | Training Blessing | Requires XP timing, stacking, expiry and run persistence. |
| `loot-boost-reserved` | Abundance Blessing | Requires stacking rules with existing elixirs and live-ops multipliers. |
| `cosmetic-token-reserved` | Cosmetic Voucher | Requires cosmetic collection, equip and duplicate exchange. |
| `title-token-reserved` | Title Voucher | Requires title ownership, presentation and eligibility. |
| `pet-token-reserved` | Companion Contract | Requires companion content and progression/combat owners. |
| `battle-pass-reserved` | Journey Points | Requires a reward track, schedule and claim protection. |

## Official evidence

Read in the logged-out Codex in-app browser on 2026-09-26. These historical examples inform the design; HELLSCRIPT proposals are labeled separately.

- [D2 아이템 분류 / Item families](https://classic.battle.net/diablo2exp/items/): 보석·주얼·룬·부적·제작·세트 등 원형 / gems, jewels, runes, charms, crafting and sets.
- [D2 큐브 조합 / Horadric Cube](https://classic.battle.net/diablo2exp/items/cube.shtml): 합성·재생 물약·소켓·수리 / conversion, rejuvenation, sockets and repair.
- [D2 기본 아이템 / Basic items](https://classic.battle.net/DIABLO2EXP/ITEMS/basics.shtml): 열쇠·장기·정수·면죄의 징표의 존재 / keys, organs, essences and Token of Absolution.
- [D3 패치 2.3.0 / Patch 2.3.0](https://news.blizzard.com/en-us/article/19859662/patch-2-3-0-now-live), 2015-08-25: 보관함·핏빛 파편·막별 재료·대균열석·지옥불 장치 / caches, shards, act materials, Greater Rift keys and Infernal Machines.
- [D4 전리품의 재탄생 / Loot Reborn](https://news.blizzard.com/en-us/article/24077223/galvanize-your-legend-in-season-4-loot-reborn), 2024-05-01: 위상·제작법·담금질·명품화·소환 재료·시즌 보상 / aspects, manuals, tempering, masterworking, summoning and seasonal rewards.
- [이모탈 Crucible of Justice](https://news.blizzard.com/en-gb/article/24135095/wield-untold-power-in-crucible-of-justice), 2024-09-09: 열쇠·문장·진주·행사 교환·복귀·재료 / keys, crests, pearls, event exchange, returners and materials.
- [이모탈 Forgotten Nightmares](https://news.blizzard.com/en-us/article/23827589/explore-a-new-piece-of-sanctuary-in-forgotten-nightmares), 2022-09-21: 백금·영원의 보주·무료/유료 패스·외형 / platinum, orbs, free/paid passes and cosmetics.

## Validation status

See [Operations reward validation](Operations_Rewards_Validation.en.md). Branch implementation, main integration, public wiki publication and player release are separate states.
