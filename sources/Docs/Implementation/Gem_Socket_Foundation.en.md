# Gem effects, socket protection and persistence

> 2026-09-23: The jeweler supersedes the original 3:1, gold-cost fusion below with 5:1 upgrades and 1:5 downgrades at no gold cost. Historical foundation evidence is retained. See [Jeweler runtime](Jeweler_Runtime.en.md).

Updated: 2026-09-23. The current catalog has **six gems and six tiers**, with Skull retired. See the [current design](../Design/HELLSCRIPT_Gem_Socket_Detail.en.md). The seven-gem implementation and results below describe the historical 2026-09-13 baseline, not the current content count.

Date: 2026-09-13 · [한국어](Gem_Socket_Foundation.md)

Status: effects, equipment protection, persistence and inspection UI are implemented. All 2,380 final Editor tests and three native launch/restart/recovery processes passed. This is not a completion record for gem acquisition, storage ownership or the entire service flow.

## Implemented behavior

`GemCatalog` implements the adopted HELLSCRIPT table of seven gems, six tiers and three equipment categories. Rare, legendary and set weapons, helmets, body armor, necklaces and rings can have one socket each. Gloves, boots, belts, normal/magic gear, duplicate sockets and invalid socket indexes are rejected. Existing affixes retain their original values.

Only equipped gems affect the character. Weapon elemental bonuses enter the existing additive damage bucket; Skull critical damage adds to the 150% baseline. HP, resource generation, movement, resistance, potion healing and barrier generation use the existing derived stats. Skull armor percentages multiply armor after strength, base equipment, upgrades and affixes. Unequipping an item removes its stat contribution while its gem stays installed.

Armor Sapphire shares the existing 50% buff reduction ceiling. It does not also enter the ordinary sheet reduction stat. Armor Emerald multiplies periodic damage reduction within the existing total reduction ceiling. Current targets are repeating enemy hazard zones and repeating legacy ground effects, not single explosions, melee hits or projectiles. No new attached hero poison or other status system was added.

Forecasts and live damage share the gem reduction and periodic classification. A repeated legacy ground effect retains that classification through its final tick and a save/reload. The forecast still covers announced attacks and existing effects; it does not predict future enemy decisions or random dodge outcomes.

## Protection and inspection

Gemmed gear is excluded from individual sale/salvage, bulk disposal and automatic replacement that would discard an existing item. Socket changes invalidate an older bulk confirmation snapshot.

The existing `bag.protectSocketed` setting now provides extra protection against automatic warehouse moves. Disabling it does not permit destructive sale or salvage of an installed gem. Warehouse transfers preserve the socket and its gem. This reconciles optional extra protection with the design's requirement to remove a gem before destroying its item.

Equipment details show the socket, gem type/tier and the effect for that slot after affixes. Replacement comparisons use gem-inclusive derived values and explicitly describe the outgoing gem contribution. Separate Sapphire/Emerald reductions appear on the character sheet. Korean and English are supplied through the existing responsive inventory layout. No scene, prefab or new art asset is required.

The inventory footer reserves space from measured translated button heights. Compact layouts use shorter Back and Storage captions. The installed-gem status also uses a distinct localization key from the insertion action.

## Save and recovery

`Item.sockets` stores index, gem ID and tier. Missing legacy lists load as empty. Item content version is now 3; saving socketed gear also records at least version 3. Older players that do not understand sockets stop at their existing newer-item-version guard instead of silently removing socket fields.

An unknown gem ID or invalid tier becomes an empty socket while retaining its equipment. Before writing the repaired account, the exact original file is archived separately as `*.gem-recovery-*.json`. Failure to preserve that original stops loading. Unsupported equipment/socket structure also stops loading and preserves the original rather than truncating sockets or silently falling back to an older account. Successful repair produces a localized startup notice without exposing internal paths.

Existing staged account transactions and object identity rules remain. This phase does not invent an account-owned or character-owned gem container while that product choice is pending.

## Fusion and reward data

`GemStacks` provides ownership-independent stack arithmetic: 999 per stack, fill matching stacks before using another slot, atomic exchange, and three-to-one fusion. Insufficient inputs, gold or output space leave all inputs intact. The chosen account transaction owner still needs to be connected.

The fusion formula remains `300 × 3^(k−1)`. Its previously incorrect derived total was corrected from 36,300 to **364,500 gold** for 243 tier-one gems to become one tier-six gem. The five stages each spend 72,900 gold in total. Individual operation costs are 900, 2,700, 8,100, 24,300 and 72,900 gold.

The catalog contains the proposed 1.5% normal and 8% elite rates, two guaranteed boss gems and stage-dependent tier rolls. Same seeds produce the same rolls and never yield tier four or higher. **These data are not yet wired to rift or sweep payouts.** Chest and offline payouts have not gained gems.

## Verification

The Unity 6000.6.0f1 [final full suite](../../Artifacts/Validation/GemSockets/Full3.xml) passed **2,380/2,380 tests**. The matching `Builds/macOS-GemSockets/HELLSCRIPT.app` passed initial launch, a separate restart and invalid-gem recovery, each exiting with code 0. The archived original matched the corrupt fixture byte for byte. The earlier 378-test focused run is an intermediate record; the full suite is the final verdict.

All **nine final screenshots** were directly reviewed: Korean/English, portrait, short landscape, 140% text, equipment comparisons, attributes and recovery. See the [verification summary](../../Artifacts/Validation/GemSockets/validation-summary.json), [visual review](../../Artifacts/Validation/GemSockets/visual-review.json) and [source/binary hashes](../../Artifacts/Validation/GemSockets/validated-source.json). Nineteen unrelated existing file changes were preserved. Only main remains locally, remotely and as a working tree; Claude's merged work remains included.

The paired native-player comparison used the same Warrior, equipment, stage 10 and seed 73551. Damage totals are run-wide telemetry.

| Condition | Result | Kills | Combat seconds | Damage dealt | Damage received |
|---|---|---:|---:|---:|---:|
| Empty sockets | Cleared | 115 | 230.66 | 27,853.23 | 506.09 |
| Tier-six gems in five slots | Cleared | 117 | 222.31 | 29,114.83 | 303.27 |

Native checks explicitly seed a level-30 Warrior, legal rare gear and gems into an isolated save, ignore new equipment when the bag is full and advance combat at a fixed time step. Outcomes, HP, enemies and rewards are not injected. This is not evidence of gem acquisition. The [paired sample](../../Artifacts/Validation/GemSockets/Native4/combat-comparison.json) does not establish balance or farming efficiency. Physical mobile touch and performance have not been verified.

Implementation and execution evidence: [effect catalog](../../Assets/HELLSCRIPT/Runtime/Core/GemCatalog.cs), [stack/fusion arithmetic](../../Assets/HELLSCRIPT/Runtime/Core/GemStacks.cs), [Editor tests](../../Assets/HELLSCRIPT/Tests/Editor/GemSocketTests.cs), [native fixture](../../Assets/HELLSCRIPT/Runtime/Presentation/RuntimeGemSocketSmoke.cs), [final build log](../../Artifacts/Validation/GemSockets/Build4.log).

| Directly reviewed final screen | Original |
|---|---|
| Korean gem details | [Open image](../../Artifacts/Validation/GemSockets/Native4/initial-01-socket-ko.png) |
| English portrait, 140% text | [Open image](../../Artifacts/Validation/GemSockets/Native4/initial-02-socket-en-portrait.png) |
| English short landscape, 140% text | [Open image](../../Artifacts/Validation/GemSockets/Native4/initial-03-socket-en-short-landscape.png) |
| English equipment comparison | [Open image](../../Artifacts/Validation/GemSockets/Native4/initial-04-comparison-en.png) |
| Korean gem attributes | [Open image](../../Artifacts/Validation/GemSockets/Native4/initial-05-attributes-ko.png) |
| English gem attributes | [Open image](../../Artifacts/Validation/GemSockets/Native4/initial-06-attributes-en.png) |
| Gem retained after separate restart | [Open image](../../Artifacts/Validation/GemSockets/Native4/restart-01-restored-socket-en.png) |
| Original-preserving recovery notice | [Open image](../../Artifacts/Validation/GemSockets/Native4/recover-01-recovery-notice-en.png) |
| Empty socket with equipment preserved | [Open image](../../Artifacts/Validation/GemSockets/Native4/recover-02-recovered-empty-socket.png) |

## Pending owner decision and integration

Account-wide gem ownership conflicts with the existing character equipment-bag slot rule. The recommended option is a **separate shared gem bag with 50 initial slots and 999 gems per stack**. The alternative is character equipment-bag storage with sharing through the account warehouse. The user has not yet confirmed either option.

After that decision, remaining work covers actual container ownership/capacity/persistence, idempotent rift and sweep rewards, full-bag handling, and the Reroller's socket creation, insertion, replacement, removal and fusion screens with quoted costs and retries. Opening gem selection from an empty socket, equipment quality and longer farming measurements also remain.
