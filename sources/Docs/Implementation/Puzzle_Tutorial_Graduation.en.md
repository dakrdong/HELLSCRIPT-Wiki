# Puzzle tutorial graduation — stage 8

Written 2026-10-07. [한국어](Puzzle_Tutorial_Graduation.md) · [Operating puzzles](Puzzle_Tutorial_Operations.en.md) · [Values and hashes](Evidence/Puzzle_Graduation_Matrix.json)

L14/L15 use real combat and graduation uses the saved first clears of all fifteen lessons. Implementation of the approved L4 N09 addition and stage 9 native/final acceptance remain. This is not complete tutorial acceptance.

## Combat lessons

L14 presents N04/N10 persistent ground zones, N14 instant areas and N08/N09 direct strikes in sequence. Always dodge each category, allow cancellation and use a movement skill when needed. All classes win within ninety seconds. Turning off any category or using a forty-percent per-hit threshold causes actual death, including accumulated small ground-zone hits. The attack preset combined with FINISH also loses; this does not establish that FINISH alone always loses.

L15 runs actual B01 boss warnings, enrage and attacks alongside priests and three archer waves. Warriors use Maintain attacks; Rangers and Mages evade all warnings. Combine balanced survival, dangerous-support targeting, ample potions, a designated movement skill and cancellation. Initial Warrior/Ranger compositions exceeded the limit because of priest self-healing and repeated cancellation. Class-specific enemy HP was adjusted through real simulations; outcomes and hero HP are never forced.

| Trial | Warrior | Ranger | Mage |
|---|---|---|---|
| L14 answer | Won 71.70s / HP 780 | Won 86.75s / HP 438 | Won 55.50s / HP 390 |
| L14 ground OFF | Died 3.90s | Died 3.90s | Died 4.40s |
| L14 area OFF | Died 40.65s | Died 35.50s | Died 12.90s |
| L14 direct OFF | Died 67.05s | Died 48.70s | Died 24.75s |
| L14 forty-percent threshold | Died 3.90s | Died 3.90s | Died 4.40s |
| L14 attack preset + FINISH | Died 53.75s | Died 53.50s | Died 21.70s |
| L15 answer | Won 68.10s / HP 349.4 | Won 43.90s / HP 438 | Won 69.00s / HP 390 |
| L15 baseline | Died 9.90s | Died 7.75s | Died 6.50s |

## Ownership and saving

`PuzzleTutorial.GraduationReady` checks the selected hero, tutorial owner, fifteen prepared/winning records and exact result receipts, with no suspended rift. `GameStore.CompleteTutorialRun` can graduate after restart without a live simulation. One atomic receipt awards 1,000 gold and a level-seven rare item. Existing `Economy`, `EquipmentRecommendation` and `EquipmentAutomation` handle automatic equipping and displaced-item storage.

The learned edict and skill allocation remain the hero default. Actual cumulative XP is 3,000, level seven with 675 remaining XP, and fifteen scroll pages. Completed journal rows open isolated, rewardless replays. The shared repeat-settings gate opens with L13; presets and sharing open at L15. Learned options persist after graduation. C01/F01 town onboarding precedes C05/F06/H07 skill recommendations. The actual shop cap with highest rift zero is three.

## Early rifts with the graduation edict

Three heroes actually completed the tutorial and graduation transaction before normal `CombatSimulation` runs with a fixed seed rule. Their graduation gear and edicts remain unchanged, with actual potion preparation, admission, chest and loot transactions and natural XP. No extra skill investment, purchases or discarded failures. Times are 0.05-second fixed-tick game time, not native input, population win rates or real-time fatigue measurements.

| Class | Rift 1 | Rift 2 | Rift 3 | Rift 4 | Rift 5 |
|---|---|---|---|---|---|
| Warrior | 102.40s / L8 | 104.90s / L9 | 118.60s / L9 | 123.00s / L10 | 139.45s / L10 |
| Ranger | 123.30s / L8 | 142.00s / L9 | 125.45s / L9 | 251.81s / L10 | 158.76s / L10 |
| Mage | 132.50s / L8 | 146.40s / L9 | 152.50s / L9 | 148.55s / L10 | 186.71s / L10 |

Initially Warrior R4 timed out alive with 540.1 HP and 500.4 boss HP remaining. The owner chose to preserve XP and improve Warrior progression, then requested higher base health and armor so it fights through small hits. XP remains unchanged. Base HP rises from 300 + 35×(level−1) to 450 + 50×(level−1). Strength still supplies two armor per point and gear contributions remain intact; Warrior gains innate armor of 60 + 4×(level−1). Ranger and Mage stats remain unchanged.

L15 Warriors learn and save the existing Maintain attacks preset: always evade ground hazards, and evade area/direct hits from 25% expected HP loss after actual mitigation. Balanced survival still handles lethal damage and potions. Normal-rift timeouts now record Duration through the shared outcome owner. A graduated Warrior timeout recommendation opens actual dodge settings to review cancellations and thresholds; it does not overwrite choices.

Warriors now clear all five rifts under the same seed rule, taking actual damage and using 8/9/9/8/11 potions. Gear, maps, growth and edicts interact; speed changes are not attributed to health or one setting alone. Earlier Warrior evidence remains under before-tank filenames. Earlier L8 onboarding and current graduated L10 measurements have different starting growth, edicts and seed paths.

## Validation boundaries

Forty affected Warrior checks and one fresh first-eight-clear journey were validated after the health/armor change. L3 small/heavy hits and L5 first-volley damage were retuned for Warriors so incorrect potion timing and retreat conditions still cause actual death. All 258 unique stage-8 focused cases have a passing latest result. Failed attempts and correction reports remain preserved.

L14 focused tests: eighteen passed. Ranger/Mage L15 answer and baseline and the repaired Warrior answer and baseline produced actual outcomes. Graduation, preset sharing/storage, failed writes, replay, restoration and affected shared-owner checks are recorded separately. The initial graduation test incorrectly counted only bag growth despite automatic storage; the corrected check verifies total ownership and original IDs. Restoration compares HP, resource, position, RNG and every actual simulation field exactly, excluding aggregated statistics. Unity double telemetry parsing changes its final binary digit; bit-for-bit aggregate telemetry persistence is not established.

The full EditMode suite and macOS development-player smoke run once in stage 9 after final code and latest-main integration. Physical mobile and public wiki publication have not been performed. Feature branches produce PRs only; they are not merged or published. Original user changes remain preserved.
