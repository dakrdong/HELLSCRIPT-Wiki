# HELLSCRIPT Elite Abilities at the Rune Region Centres

Date: 2026-09-17
작성일: 2026-09-17

## Background

This finishes the work [Locked Rune Region Preview](Rune_Region_Preview.en.md) left open. That change carried over only the structure for reading a locked region's centre hex from real data, and recorded that the six elite abilities the mock-up puts there did not exist in the game. Those six are now in the ability data, on the board and in combat.

Their names, numbers and conditions are taken verbatim from the definitions in `source/data-and-assets.js` inside `HELLSCRIPT-RuneBoard-Elite-Preview-v13.zip`. Nothing was designed anew.

## How regions were matched to elites

The mock-up's six named regions and the game's six colour grades have different names. They were not paired in listed order but by **the direction each board's coordinates point in**. The mock-up and the game share one hex layout and differ only in radius (37 hexes per mock-up region, 61 in the game). Measuring the bearing of each region centre pairs all six within 1.04°.

| Game region | Centre | Bearing | Mock-up region | Elite |
|---|---|---|---|---|
| G1 Ivory | (5, −9) | 56.33° | Spread (55.28°) | Chain Collapse |
| G2 Green | (−5, 9) | 236.33° | Guard (235.28°) | Unyielding Ward |
| G3 Sky | (−9, 4) | 176.33° | Assault (175.28°) | Combat Heat |
| G4 Violet | (−4, −5) | 116.33° | Precision (115.28°) | Exposed Weakness |
| G5 Gold | (9, −4) | 356.33° | Technique (355.28°) | Skill Inheritance |
| G6 Rose | (4, 5) | 296.33° | Flow (295.28°) | Cycle Core |

The board's own centre (G0) is `elite: null` in the mock-up too, so it keeps the attack power it granted.

All six weapons use the same table. An elite is a property of the region, not of the weapon.

## The abilities

Each one is a whole effect rather than an amount. It therefore carries no value tier and no unit, and appears in the selection note and the active-ability summary as a name and its wording, with no number.

| Elite | Effect |
|---|---|
| Combat Heat | +1 Offensive per direct hit. +2% direct damage per stack, up to 5. Gained at most every 0.5s. All stacks end 4s after the last direct hit |
| Exposed Weakness | A direct critical leaves the target taking +10% direct damage from you for 4s. Reapplying refreshes only the duration |
| Chain Collapse | A direct-hit kill bursts for 2.5m: 30% of attack basis D, in the element of the killing hit. 1s internal cooldown |
| Skill Inheritance | +1 level to equipped active skills. Combined with per-skill level runes, up to +5 |
| Cycle Core | Every 50 resource actually spent cuts 1s from the equipped active with the longest remaining cooldown. 3s internal cooldown |
| Unyielding Ward | Crossing from above 35% life to 35% or below raises a barrier worth 20% of maximum life for 4s. 30s internal cooldown |

## Implementation

The abilities reach combat through the same channel as unique item effects. A rune contribution puts a name such as `ELITE_HEAT` into `HeroStats.specials`, and combat reads it with `Stats.specials.Contains(...)` — exactly what the unique items `LW04` and `LM03` already do.

| Elite | Where it hangs |
|---|---|
| Combat Heat | Stacks gained in `EliteDirectHit`, damage applied in `ConditionalBonus` |
| Exposed Weakness | `EliteDirectHit` refreshes the target's `eliteExposed`, `ConditionalBonus` applies the damage |
| Chain Collapse | `EliteCascade`, which strikes through `ApplyOutgoing` rather than `Hit` |
| Skill Inheritance | Inside `HeroStats` construction, after the per-skill level runes |
| Cycle Core | `ConsumeCostEffects`, which only ever receives what was actually paid |
| Unyielding Ward | The life threshold crossing in `Hurt`, beside the existing `LW04` check |

Several things are deliberate.

- **A hit is never multiplied by the stack it grants.** The damage is calculated first and the stack or exposure is added afterwards.
- **Chain Collapse does not use `Hit`.** `Hit` rolls criticals, overpower and lucky hits, and reaches this hook again. Striking through `ApplyOutgoing` makes the four things the specification forbids structurally impossible.
- **"Direct" is defined in one place.** `DirectHit(kind)` is true for `Direct` and `Basic` only. Damage over time, thorns, shadow and spread neither create stacks and exposure nor benefit from them.
- **`ConsumeCostEffects` only receives real payments.** It returns immediately on `paid<=0`, so free casts and resource regeneration accumulate nothing for Cycle Core.
- **A 50 that lands during the internal cooldown is still spent.** The specification says it is not held back, so the `while` subtracts before it checks. With nothing to trim the internal cooldown is not armed, because nothing was done.
- **The new state lives where it is saved.** It sits in `CombatEffectState` and `EnemyState`, so it persists with the run: editing and saving the board mid-combat neither fires the ward nor resets its wait. The stack timestamp starts at `-1000` so the first hit of a run already earns a stack, and a save written before these fields existed keeps that initialiser.

## What was deliberately not carried over

The mock-up says an elite is **activated by a rune of any colour**. That was not carried into the game.

In the mock-up, connection starts from one central point only, and an elite is a hex that "accepts every colour but is not a new start point". In the game each region has its own start hex, and only a rune of that region's colour may cover it (`ForeignStart`). Relaxing that would let a foreign-coloured rune occupy the centre while being unable to seed that region's connection, which turns the region into a trap.

So the start rule was left alone and the elite was placed on top of it. An elite therefore lights up with its own region's colour. The v13 instructions also said not to change the connection rules. Reshaping the board topology into the mock-up's single central start would re-decide the validity of every saved layout, and is separate work.

Ability icons are still absent. Rune board abilities have no artwork, and adding icons would mean drawing one per ability.

## What was left alone

Board coordinates, region unlock rules, colour requirement checks, connection and spacing checks, the save schema and its keys, rune placement, removal, rotation and dragging, storage, fusion, presets and the placement practice are unchanged. Board drawing is unchanged too: a centre is still drawn as its teal cross.

Not one ordinary hex changed its ability, number or assignment. Only the 36 centres — six weapons × six regions — changed, and the difference in the [exported catalogue](Rune_Mastery_Catalog.json) is exactly those 36 hexes' `meaning` and `value`, 72 lines.

## Verification

Unity Edit Mode, whole suite, run on a copy of the repository.

The new `RuneEliteTests` exercises all six through the real combat code. It does not restate the arithmetic and compare; it calls the real `Hit`, `Hurt` and `ConsumeCostEffects`.

- Across six weapons × six regions each centre is a different elite, and different weapons agree on what a given region grants. No elite is reachable anywhere but a region centre. The board's own centre still grants attack power.
- Covering a centre puts that elite into `specials` and adds no attack power and no stat line. With nothing placed, no elite is on the sheet.
- Combat Heat: the hit that grants a stack does not carry it. A second hit inside the half second deals more but grants nothing. One stack is +2%, five are +10%, and five is the ceiling. A periodic tick neither grants nor sustains stacks. All stacks leave four seconds after the last direct hit and the damage returns to plain.
- Exposed Weakness: an ordinary hit exposes nothing. A critical exposes for four seconds and gives +10%. It does not travel to a second target. It never applies the vulnerable mark. Reapplying refreshes the time. It ends after four seconds.
- Chain Collapse: inside 2.5m is hit, beyond it is not. The burst is not a critical, is 30% of basis D, and uses the killing hit's element. A second kill inside the second does not burst; after the second it bursts again.
- Cycle Core: only the longest carried cooldown loses a second, and a skill the hero does not carry is untouched. A 50 during the internal cooldown trims nothing and is not banked. 30 + 20 make one trim. A zero payment accumulates nothing. With nothing to trim the internal cooldown is not armed.
- Unyielding Ward: nothing above 35%. Crossing raises 20% of maximum life for four seconds, which then expires. A second crossing inside thirty seconds is ignored; after thirty seconds it answers again.
- On a board with no elite, none of the above happens.

The existing `AbilityTiersDistinguishBaseStatsSpecialtiesAndSkillLevels` gained an elite branch: an elite is not an amount, so its value tier is 0. The existing branches are unchanged.

The macOS development build's runtime smoke `-hellscriptRuneSmoke` was extended as well. Its screenshot fixture already unlocks every region for a moment, so at the same point it temporarily grants one G1 rune and actually covers the G1 centre of the equipped weapon's board. In Korean portrait and English landscape it reads the elite's name and `This ability is active.`, then starts combat and confirms that elite is in `Stats.specials` and is not counted as attack power, then removes the placement, the rune and the clear again.

The evidence is the [Korean portrait screen](RuneEliteEvidence/12-elite-centre-active-ko.png), the [English landscape screen](RuneEliteEvidence/13-elite-centre-active-en-landscape.png), the [whole Edit Mode report](RuneEliteEvidence/editmode-full.xml), the [run log](RuneEliteEvidence/runtime.txt) and the [smoke output](RuneEliteEvidence/smoke-result.txt).

The smoke exits as a failure for a reason that predates this work: one global observation HUD `Shield` label is flagged on every screen. The [run log](RuneEliteEvidence/runtime.txt) records how that was confirmed.

The checks ran on a macOS development build only. This was not seen on real mobile hardware, and balance was not measured. The QA results inside the mock-up ZIP describe the browser mock-up and were not used as evidence here.

## Remaining work

Lighting an elite with any colour needs the board topology changed, so it is left open. Ability icons, buying slots with points and a padlock-popup screen also belong to the separate effort that brings the whole rune board in line with the mock-up.

The six abilities carry the mock-up's draft numbers. They have not been measured in real combat.
