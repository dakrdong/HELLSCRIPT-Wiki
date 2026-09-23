# HELLSCRIPT gems and sockets

Updated: 2026-09-23 · [한국어](HELLSCRIPT_Gem_Socket_Detail.md)

The owner selected six gemstone types and removed Skull. The existing six tiers remain, giving **36 type/tier combinations**. G01–G06 retain their saved IDs and effect values. G07 is retired and is never rolled, installed or fused as new content.

## Types and effects

| ID | Gem | Element | Weapon | Armor: helmet/body | Jewelry: necklace/ring |
|---|---|---|---|---|---|
| G01 | Ruby | Fire | Fire damage | Maximum life | Fire resistance |
| G02 | Sapphire | Cold | Cold damage | Buff damage reduction | Cold resistance |
| G03 | Topaz | Lightning | Lightning damage | Resource regeneration | Lightning resistance |
| G04 | Emerald | Poison | Poison damage | Periodic damage reduction | Poison resistance |
| G05 | Amethyst | Shadow | Shadow damage | Movement speed | Shadow resistance |
| G06 | Diamond | Physical | Physical damage | Barrier generation | All resistance |

Skull's critical damage, armor percentage and potion-healing gem effects are removed. Other sources of those stats remain governed by their existing systems.

## Six tiers

| Tier | Korean name | Acquisition |
|---|---|---|
| 1 | 부서진 | Rift drops |
| 2 | 흐린 | Rift drops or fusion |
| 3 | 맑은 | Rift drops or fusion |
| 4 | 벼려진 | Fusion only |
| 5 | 완전한 | Fusion only |
| 6 | 왕관의 | Fusion only |

The current jeweler upgrades five gems of the same type and tier into one gem of the next tier, and downgrades one gem into five of the previous tier. Neither operation costs gold or has a failure roll. Tier six requires 3,125 tier-one gems. One, five, or all operations can be selected. Insufficient ingredients or output space leaves the transaction unchanged. See [Jeweler runtime](../Implementation/Jeweler_Runtime.en.md).

## Effect values by tier

| Effect | 1 | 2 | 3 | 4 | 5 | 6 |
|---|---:|---:|---:|---:|---:|---:|
| Every weapon gem: matching damage increase (%) | 3 | 5 | 8 | 12 | 17 | 23 |
| Ruby armor: maximum life (%) | 2 | 3.5 | 5 | 7 | 9.5 | 12 |
| Sapphire armor: buff damage reduction (%) | 1 | 1.5 | 2.5 | 3.5 | 5 | 6.5 |
| Topaz armor: resource per second | 0.4 | 0.7 | 1.1 | 1.6 | 2.2 | 3 |
| Emerald armor: periodic damage reduction (%) | 3 | 5 | 8 | 11 | 15 | 20 |
| Amethyst armor: movement speed (%) | 1 | 1.5 | 2.5 | 3.5 | 4.5 | 6 |
| Diamond armor: barrier generation (%) | 3 | 5 | 8 | 12 | 16 | 21 |
| G01–G05 jewelry: matching resistance | 8 | 14 | 22 | 32 | 45 | 60 |
| Diamond jewelry: all resistance | 4 | 7 | 11 | 16 | 22 | 30 |

Weapon damage enters the existing additive bucket. Buff reduction and movement retain their existing 50% ceilings. Barrier generation does not increase the maximum-life-based barrier cap. Resistance uses the current nonphysical mitigation formula and 70% ceiling.

## Sockets and services

Eligible rare, legendary and set weapons, helmets, body armor, necklaces and rings allow one socket each. Gloves, boots, belts, normal and magic equipment do not. Opening a socket costs `2,000 × max(1,item level)` gold and 30 ordinary materials. Insertion into an empty socket is free. Replacing or removing an installed gem costs `500 × max(1,item level)` gold and returns the old gem to shared storage.

Installed gems protect equipment from destructive sale, salvage and automatic replacement. Optional extra protection also prevents automatic warehouse movement; disabling that option never permits destroying the gem. Transactions stage changes on an account copy and commit only after successful persistence; duplicate request IDs cannot apply twice.

## Drops and storage

All six types have an equal 1/6 chance. Normal enemies have a 1.5% chance to drop one gem; elites have an 8% chance. Boss clears and sweeps award two. Chests do not award gems. Stages 1–9 drop tier one; 10–19 use 70% tier one / 30% tier two; 20–29 use 70% tier two / 30% tier three; 30+ drop tier three. Tier four and above require fusion.

Shared account storage has a provisional 50-slot default and a 999-gem stack limit. Type and tier define a stack; overflow uses additional slots. The remaining farming and balance measurements are deferred until owner-directed playtesting.

## Existing saves

On load, valid G07 stacks, installed sockets and gem pickups in active or completed repeat-hunt runs become G06 Diamonds at the same tier and quantity. Stack boundaries, equipment identity, pickup ownership flags and existing currency are preserved. Claimed pickups are not awarded again. The exact original save is archived before the migrated account can be saved. Subsequent loads do not repeat the migration.

Unknown gem IDs or invalid socket tiers retain the existing empty-socket recovery rule with an archived original. Invalid quantities and unsupported socket structures still stop loading rather than silently deleting owned data.

## Images and validation

[Six representative icon candidates](../Art/Gems/2026-09-23/Gem_Icon_Candidates.md) accompany this revision. They are not 36 tier-specific images. Native transparent alpha is preserved; the built-in image tool did not report its model identity, so these images are review candidates rather than verified production assets.

Validate all 36 combinations across three equipment effect categories, seed replay and drop bounds, rejected G07 transactions, migration/restart idempotence, full storage preservation, and original-file archival. Historical seven-gem test counts remain historical evidence and must not be reused as proof of this revision.
