# Generated-rift exit and shrine approach corrections

Date: 2026-09-13 · [한국어](Edict_Field_Expansion.md)

Status: **Corrected exit selection and shrine approach loops, completed the controlled 360-run comparison, and verified the full suite and native persistence on the latest main including Rune mastery.**

## Runtime correction

This continues `8e9e527` under the [exploration specification](../Design/HELLSCRIPT_Rift_Exploration_Detail.md) and [screen-layout specification](../Design/HELLSCRIPT_Screen_Layout_Detail.md). Claude's attributes, affixes and balance history remain on main. A fresh local/remote/worktree check found only `main`; see the [branch record](../../Artifacts/Validation/EdictField/branch-status.json).

The completed [Rune mastery work](Rune_Mastery_Implementation.md), `d0c8e25`, reached main and the public wiki during this phase. Its code, documents and databases are preserved and included in final full-suite/player validation. The 360-run controlled experiment below pins the pre-Rune base to isolate the field corrections. The combined main is separately checked with 24 representative runs, the full suite and the native player.

Previously, left/right wall following added a turn angle to the path-distance score only for exits in the current room. Remote visited rooms escaped that penalty and could win over a valid local junction. `RiftExploration.Goal` now orders eligible current-room exits by the specified turn direction. Only when no eligible local exit remains does it return to the nearest reachable frontier.

Existing committed routes, observation-point priority, locked boss entrances, unreachable/deferred exits and nearest-area distance ordering are preserved. Combat numbers, movement speed, generated geometry, boss conditions, clocks, rewards and serialized fields did not change. Seven of ten focused route cases failed before the correction; all ten pass afterward, including both directions, corridor lengths, blocked/deferred/locked branches, visited-frontier fallback and saved route commitment.

The subsequent 360-run audit exposed four shrine approach/cancel loops ending in timeouts. A generated access point sits 1.35m from the shrine; a 0.25m arrival tolerance could begin invocation outside the actual 1.5m use radius, causing immediate cancellation and another approach. `CombatSimulation.TickShrine` now requires both arrival and actual use range before invoking. The use radius, 0.5-second invocation and 20-second blessing are unchanged.

Six focused shrine cases moved from four failures before the correction to all passing afterward. Both shrine definitions cover saved approach restoration, real movement followed by exactly one use, and cancellation without reward after an actual range exit during invocation. The complete 360-run comparison was rerun after this correction.

## 360 matched runs

Two presets per class each ran 30 seed indices under legacy rules and fresh core defaults: 180 matched pairs. Every pair used tier 10, level 30, eight legal item-level-30 equipment slots and the current 54 affixes. Map, encounter, character sheet and equipment matched. Core defaults explicitly kept global survival `OFF`.

| Build | Legacy clears | Core-default clears | Core timeouts | Core other failures | Core clear median (s) |
|---|---:|---:|---:|---:|---:|
| Warrior · Whirlwind survival | 30/30 | 30/30 | 0 | 0 | 189.8 |
| Warrior · Leap/Rend | 30/30 | 30/30 | 0 | 0 | 172.8 |
| Ranger · Pierce spacing | 30/30 | 30/30 | 0 | 0 | 179.1 |
| Ranger · Poison ambush | 30/30 | 29/30 | 0 | 1 | 188.9 |
| Mage · Frost field | 29/30 | 30/30 | 0 | 0 | 192.6 |
| Mage · Chain control | 20/30 | 24/30 | 6 | 0 | 239.8 |

Overall clears were **169/180** for legacy and **173/180** for core defaults. Navigation errors were 0 and 0 respectively. See [all outcomes and paired rows](../../Artifacts/Validation/EdictField/field-summary.json). Both modes use the corrected route implementation; this comparison cannot isolate the route fix or an individual skill. Clear-only times have survivor bias and must be read with failures.

The separate 24-run before/after sample retained 22 clears in each version. Ranger Pierce, seed index 1, improved from a core timeout at 300 seconds to a clear around 254 seconds, with boss arrival falling from roughly 277 to 191 seconds. Mage Chain Control at index 1 moved in the opposite direction: a core clear around 206 seconds became a timeout. The same success/failure swaps occurred in legacy mode. No universal speed or win-rate gain is claimed; the correction establishes the specified junction ordering.

Observer tests compare entire serialized rift outcomes with and without observation in both modes, and account for every tick exactly once. Acting, moving, recovering, stationary-with-target and stationary-without-target are mutually exclusive end-of-step labels in that order. Movement can include combat movement; these buckets observe state rather than identify causal time losses.

## Matched shrine correction comparison

The same 360 wall-corrected runs were compared before and after the shrine arrival change. Only that runtime condition differs; equipment, character sheets, maps and encounters match. Legacy clears changed **167→169/180** and core-default clears **171→173/180**. The four confirmed looping cases are listed by reproducible seed index:

| Build, seed and mode | Before shrine correction | After shrine correction |
|---|---|---|
| Warrior · Whirlwind survival · 15 · legacy | TimeLimit · 300.0s | Cleared · 182.5s |
| Warrior · Leap/Rend · 5 · core-default | TimeLimit · 300.0s | Cleared · 153.1s |
| Mage · Chain control · 9 · core-default | TimeLimit · 300.0s | Cleared · 240.6s |
| Warrior · Whirlwind survival · 24 · legacy | TimeLimit · 300.0s | Cleared · 205.4s |

The character kept moving during these loops, so navigation-error counts stayed zero. Discovery, kills, action logs and position samples were inspected together instead of treating a zero counter as proof of correct exploration. The [original loop evidence](../../Artifacts/Validation/EdictField/shrine-loop-baseline.json) and all matched outcomes are preserved. Remaining timeouts and deaths are not claimed as resolved.

## Native persistence and screen verification

The Rune-integrated representative comparison covers six builds, seed indices 0 and 1 and both rule modes. With no Rune placements, every combat summary field—outcome, time, damage, HP, equipment, sheet and skill statistics—matches the controlled experiment. Initial enemy states were also exported: removing only the two verified-empty newly serialized Rune damage fields produces identical combat seeds, boss kinds and all enemy states. See the [integration comparison](../../Artifacts/Validation/EdictField/integration-check.json). This does not validate every Rune layout or repeat all 360 conditions.

A dedicated save used a legally equipped level-30 Ranger with highest clear 9, entering the actual tier-10 rift with seed `55755`. Actual fixed combat steps ran without overriding position, enemies, map, routes, HP, clocks or rewards.

1. After natural combat and room discovery, the application saved an in-progress exit route at 43.8 seconds and quit.
2. A fresh process verified exact time, position, destination, health, resource, discovery, exploration state and both random streams, then defeated the actual boss at 253.6 seconds and committed the character's first-clear reward.
3. After returning to town and saving, a third process verified unchanged inventory, currency, first-clear and transaction records, with no replayed rift or duplicate rewards.

The fixture started with 16 unplaced starter runes and received four actual boss-reward runes, reaching 20. A fresh process verifies the entire Rune ownership, placement, preset and reward-receipt state unchanged, including exactly one boss reward receipt.

All five screenshots were directly reviewed, including 1280×720 Korean landscape, 720×1280 English portrait and 140% text. Footer safe-area containment and missing localization were checked. See the [natural route](../../Artifacts/Validation/EdictField/Native3/checkpoint-01-natural-exit-ko.png), [restored English route](../../Artifacts/Validation/EdictField/Native3/resume-01-restored-route-en.png) and [English clear result](../../Artifacts/Validation/EdictField/Native3/resume-03-clear-result-en.png).

The test helper advances fixed combat ticks directly with the ordinary update loop disabled. The result's “Real 0.0s” reflects that unadvanced wall-clock counter; this is not a real-time pacing or speed-option test.

## Evidence and remaining work

All **2,120 Unity Editor tests** and **three independent native launches/restarts** passed. See the [validation summary](../../Artifacts/Validation/EdictField/validation-summary.json), [visual review](../../Artifacts/Validation/EdictField/visual-review.json) and [source/player hashes](../../Artifacts/Validation/EdictField/validated-source.json). Player: `Builds/macOS-EdictField/HELLSCRIPT.app`.

The controlled field experiment preserves `8e9e527` and its field corrections in a [separate immutable source record](../../Artifacts/Validation/EdictField/field-source.json). The final full suite and player use the same `d0c8e25` Rune-integrated inputs plus this phase's owned files. The Rune commit is preserved; unrelated settings and wiki authoring/tool changes are not staged. No scene, prefab or package was changed by this phase. The evidence also records six unrelated Rune source copies briefly entering the early pre-fix reproduction clone, their removal and subsequent pinned verification.

Remaining work includes exploration/combat pressure in timeout layouts, actual legendary/set utility through natural growth and farming, the first fifteen minutes, and physical-device touch/layout/performance. These maximum-level equipped fixtures do not establish natural progression or whole-game completion. A separate repeated-exit cycle detector is not claimed as completed by this correction.
