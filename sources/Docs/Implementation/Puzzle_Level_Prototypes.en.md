# Puzzle tutorial L2/L5/L7 simulation prototypes

> 2026-10-08: The level numbers, windups and cooldowns described here record the earlier build that adjusted monster patterns. The current levels follow [Native monster patterns](Puzzle_Tutorial_Native_Patterns.en.md).

Created: 2026-10-07 · [한국어](Puzzle_Level_Prototypes.md)

**Stage3 gate passed: 79 passed, 0 failed, 0 skipped.** Apply the approved Warrior L2 edge answer and shared retreat healing policy; stages4–9 follow. This PR contains test-only arenas and shared potion policy. The runner, hub, XP and graduation are subsequent stages. No PR merge or public wiki deployment.

## Real pipeline and final tuning

Keep the new GameStore hero's actual starter weapon (attack20), then use HuntEdictEditSession → real learning/equipment → CommitHuntEdict → CombatSimulation.Tick(0.05). Seed77123; L2/L5/L7 start at levels2/4/5 with dodge probability0. Correct choices kill every enemy with at least10% HP within90 seconds; wrong choices genuinely die with enemies remaining. No forced loss, artificial healing, PitRooted or PitUnbeaten.

- L2: five N02 HP65/attack3/speed3.6; N14 HP550/attack maxHP×1.3/speed2.7/initialCD2/warning1.8. Warrior edge positioning also wins; Ranger/Mage edge positioning dies. Warrior dodge answers also use balanced survival.
- L5: four N07, class HP234/215/500 and attack maxHP×0.43/0.326/0.326, speed2.2, initialCD2, subsequentCD1.5, warning0.2. Real physical mitigation produces approximately25% HP per hit. Potions stay at60%; survival-first45/75 wins, balanced30/55 and emergency20/40 die. Include aggressive style inheritance through the actual new-account scope filter.
- L7: hero(-7,-7), N10+E07(7,7), class HP900/1750/1750, attack50/speed2.3. Four N01 HP70/attack3/speed2.7 at0/12 seconds. Keep first4 seconds, period10, delay5, pull8m/s, radius2.5. Existing reserveSkill=ON reserves an assigned mobility skill for emergencies; the unassigned baseline has no valid reserved skill. Also cover distance positioning/edge surround response and enemies on four sides. Every winning case survives at least three E07 releases with zero E07 damage.

## Shared retreat healing

While the same hero/run owns active retreat memory and HP remains below return HP, use max(potionHP, returnHP) as the potion threshold. Pass the assessed threshold to the existing real TryUsePotion executor. Existing toggle, stock, cooldown and heal amount still govern execution. Inactive memory, another hero/run, disabled potions or a live cooldown cannot grant extra healing. This applies to regular combat too, with updated KO/EN help.

## Validation and reuse

L2 42 and L5 21 passed in stage3-owned-kit-matrix.xml. Its three L7 auxiliary failures were fixed, then all16 L7 cases passed in stage3-l7-reserved-kit.xml. Reuse unchanged L2/L5 results. The [final gate JSON](../../Artifacts/PuzzleTutorial/20261006/stage3-gate-validation.json) records reports, actual save paths, settings and state hashes for every row.

The unchanged EdictResponse62/GravityCollapse19/IncomingForecast88/Potion19 scopes passed all188 checks in stage3-approved-policy.xml and are reused. That report originally had248/258 passing; its ten L5 failures were subsequently repaired. No full EditMode suite, runtime smoke, player build, browser or physical mobile checks ran at this stage; final integrated validation runs once.

The initial after-aggressive cases applied the full recipe and overwrote potions to40%. The claim that this represented the actual new-account window was wrong. CombatStyleScope preserves the learned60% potion threshold; the current matrix uses that real filter. Preserve the original70-case matrix, reports and states as historical evidence, alongside the [tuning history](../../Artifacts/PuzzleTutorial/20261006/stage3-recovery-progress.json).

Artifacts stay under Artifacts/PuzzleTutorial/20261006 for open PR review, with retention review on2026-10-13. Permanent deletion is not authorized. The following matrix is current.

## Passed 79-case matrix

| Level | Class | Policy | Expected/actual | HP / max (%) | Seconds | E07 / mobility | Result |
|---|---|---|---|---|---|---|---|
| L2 | Warrior | baseline | DEAD | 0.0/365 (0.0%) | 4.25 | 0 / 0 | Passed |
| L2 | Warrior | direct-only | DEAD | 0.0/365 (0.0%) | 4.30 | 0 / 0 | Passed |
| L2 | Warrior | ground-only | DEAD | 0.0/365 (0.0%) | 4.25 | 0 / 0 | Passed |
| L2 | Warrior | position-CENTER | DEAD | 0.0/365 (0.0%) | 9.95 | 0 / 0 | Passed |
| L2 | Warrior | position-DISTANCE | DEAD | 0.0/365 (0.0%) | 4.30 | 0 / 0 | Passed |
| L2 | Warrior | position-EDGE | WIN | 365.0/365 (100.0%) | 25.50 | 0 / 0 | Passed |
| L2 | Warrior | position-ORBIT | DEAD | 0.0/365 (0.0%) | 4.15 | 0 / 0 | Passed |
| L2 | Warrior | potion-balanced | DEAD | 0.0/365 (0.0%) | 4.25 | 0 / 0 | Passed |
| L2 | Warrior | potion-careful | DEAD | 0.0/365 (0.0%) | 4.25 | 0 / 0 | Passed |
| L2 | Warrior | potion-frugal | DEAD | 0.0/365 (0.0%) | 4.25 | 0 / 0 | Passed |
| L2 | Warrior | win-all | WIN | 365.0/365 (100.0%) | 29.15 | 0 / 0 | Passed |
| L2 | Warrior | win-attack | WIN | 340.5/365 (93.3%) | 23.05 | 0 / 0 | Passed |
| L2 | Warrior | win-balanced | WIN | 340.5/365 (93.3%) | 23.05 | 0 / 0 | Passed |
| L2 | Warrior | win-survival | WIN | 329.5/365 (90.3%) | 25.30 | 0 / 0 | Passed |
| L2 | Ranger | baseline | DEAD | 0.0/298 (0.0%) | 4.30 | 0 / 0 | Passed |
| L2 | Ranger | direct-only | DEAD | 0.0/298 (0.0%) | 4.40 | 0 / 0 | Passed |
| L2 | Ranger | ground-only | DEAD | 0.0/298 (0.0%) | 4.30 | 0 / 0 | Passed |
| L2 | Ranger | position-CENTER | DEAD | 0.0/298 (0.0%) | 4.25 | 0 / 0 | Passed |
| L2 | Ranger | position-DISTANCE | DEAD | 0.0/298 (0.0%) | 4.30 | 0 / 0 | Passed |
| L2 | Ranger | position-EDGE | DEAD | 0.0/298 (0.0%) | 4.25 | 0 / 0 | Passed |
| L2 | Ranger | position-ORBIT | DEAD | 0.0/298 (0.0%) | 4.25 | 0 / 0 | Passed |
| L2 | Ranger | potion-balanced | DEAD | 0.0/298 (0.0%) | 4.30 | 0 / 0 | Passed |
| L2 | Ranger | potion-careful | DEAD | 0.0/298 (0.0%) | 4.30 | 0 / 0 | Passed |
| L2 | Ranger | potion-frugal | DEAD | 0.0/298 (0.0%) | 4.30 | 0 / 0 | Passed |
| L2 | Ranger | win-all | WIN | 298.0/298 (100.0%) | 25.35 | 0 / 0 | Passed |
| L2 | Ranger | win-attack | WIN | 280.3/298 (94.0%) | 16.40 | 0 / 0 | Passed |
| L2 | Ranger | win-balanced | WIN | 280.3/298 (94.0%) | 16.40 | 0 / 0 | Passed |
| L2 | Ranger | win-survival | WIN | 273.2/298 (91.7%) | 17.60 | 0 / 0 | Passed |
| L2 | Mage | baseline | DEAD | 0.0/265 (0.0%) | 4.30 | 0 / 0 | Passed |
| L2 | Mage | direct-only | DEAD | 0.0/265 (0.0%) | 4.40 | 0 / 0 | Passed |
| L2 | Mage | ground-only | DEAD | 0.0/265 (0.0%) | 4.30 | 0 / 0 | Passed |
| L2 | Mage | position-CENTER | DEAD | 0.0/265 (0.0%) | 4.25 | 0 / 0 | Passed |
| L2 | Mage | position-DISTANCE | DEAD | 0.0/265 (0.0%) | 4.30 | 0 / 0 | Passed |
| L2 | Mage | position-EDGE | DEAD | 0.0/265 (0.0%) | 4.25 | 0 / 0 | Passed |
| L2 | Mage | position-ORBIT | DEAD | 0.0/265 (0.0%) | 4.25 | 0 / 0 | Passed |
| L2 | Mage | potion-balanced | DEAD | 0.0/265 (0.0%) | 4.30 | 0 / 0 | Passed |
| L2 | Mage | potion-careful | DEAD | 0.0/265 (0.0%) | 4.30 | 0 / 0 | Passed |
| L2 | Mage | potion-frugal | DEAD | 0.0/265 (0.0%) | 4.30 | 0 / 0 | Passed |
| L2 | Mage | win-all | WIN | 265.0/265 (100.0%) | 20.70 | 0 / 0 | Passed |
| L2 | Mage | win-attack | WIN | 243.7/265 (92.0%) | 19.05 | 0 / 0 | Passed |
| L2 | Mage | win-balanced | WIN | 243.7/265 (92.0%) | 19.05 | 0 / 0 | Passed |
| L2 | Mage | win-survival | WIN | 250.8/265 (94.6%) | 19.75 | 0 / 0 | Passed |
| L5 | Warrior | balanced-after-aggressive | DEAD | 0.0/435 (0.0%) | 3.70 | 0 / 0 | Passed |
| L5 | Warrior | baseline | DEAD | 0.0/435 (0.0%) | 3.70 | 0 / 0 | Passed |
| L5 | Warrior | baseline-after-aggressive | DEAD | 0.0/435 (0.0%) | 3.70 | 0 / 0 | Passed |
| L5 | Warrior | emergency | DEAD | 0.0/435 (0.0%) | 3.70 | 0 / 0 | Passed |
| L5 | Warrior | emergency-after-aggressive | DEAD | 0.0/435 (0.0%) | 3.70 | 0 / 0 | Passed |
| L5 | Warrior | win-careful | WIN | 435.0/435 (100.0%) | 43.30 | 0 / 1 | Passed |
| L5 | Warrior | win-careful-after-aggressive | WIN | 435.0/435 (100.0%) | 43.50 | 0 / 1 | Passed |
| L5 | Ranger | balanced-after-aggressive | DEAD | 0.0/354 (0.0%) | 3.70 | 0 / 0 | Passed |
| L5 | Ranger | baseline | DEAD | 0.0/354 (0.0%) | 3.70 | 0 / 0 | Passed |
| L5 | Ranger | baseline-after-aggressive | DEAD | 0.0/354 (0.0%) | 3.70 | 0 / 0 | Passed |
| L5 | Ranger | emergency | DEAD | 0.0/354 (0.0%) | 3.70 | 0 / 0 | Passed |
| L5 | Ranger | emergency-after-aggressive | DEAD | 0.0/354 (0.0%) | 3.70 | 0 / 0 | Passed |
| L5 | Ranger | win-careful | WIN | 354.0/354 (100.0%) | 46.05 | 0 / 0 | Passed |
| L5 | Ranger | win-careful-after-aggressive | WIN | 354.0/354 (100.0%) | 45.25 | 0 / 0 | Passed |
| L5 | Mage | balanced-after-aggressive | DEAD | 0.0/315 (0.0%) | 3.70 | 0 / 0 | Passed |
| L5 | Mage | baseline | DEAD | 0.0/315 (0.0%) | 3.70 | 0 / 0 | Passed |
| L5 | Mage | baseline-after-aggressive | DEAD | 0.0/315 (0.0%) | 3.70 | 0 / 0 | Passed |
| L5 | Mage | emergency | DEAD | 0.0/315 (0.0%) | 3.70 | 0 / 0 | Passed |
| L5 | Mage | emergency-after-aggressive | DEAD | 0.0/315 (0.0%) | 3.70 | 0 / 0 | Passed |
| L5 | Mage | win-careful | WIN | 315.0/315 (100.0%) | 46.45 | 0 / 0 | Passed |
| L5 | Mage | win-careful-after-aggressive | WIN | 315.0/315 (100.0%) | 46.35 | 0 / 0 | Passed |
| L7 | Warrior | attack-role | DEAD | 0.0/470 (0.0%) | 9.50 | 1 / 0 | Passed |
| L7 | Warrior | baseline | DEAD | 0.0/470 (0.0%) | 9.50 | 1 / 0 | Passed |
| L7 | Warrior | unequipped | DEAD | 0.0/470 (0.0%) | 9.50 | 1 / 0 | Passed |
| L7 | Warrior | win-escape | WIN | 401.6/470 (85.5%) | 62.25 | 6 / 6 | Passed |
| L7 | Warrior | win-escape-distance | WIN | 374.7/470 (79.7%) | 65.05 | 6 / 6 | Passed |
| L7 | Warrior | win-escape-four-sides | WIN | 359.8/470 (76.5%) | 65.95 | 6 / 6 | Passed |
| L7 | Ranger | baseline | DEAD | 0.0/382 (0.0%) | 29.00 | 3 / 2 | Passed |
| L7 | Ranger | unequipped | DEAD | 0.0/382 (0.0%) | 9.00 | 1 / 0 | Passed |
| L7 | Ranger | win-escape | WIN | 382.0/382 (100.0%) | 39.75 | 4 / 4 | Passed |
| L7 | Ranger | win-escape-distance | WIN | 315.7/382 (82.7%) | 35.75 | 3 / 3 | Passed |
| L7 | Ranger | win-escape-four-sides | WIN | 308.6/382 (80.8%) | 43.10 | 4 / 4 | Passed |
| L7 | Mage | baseline | DEAD | 0.0/340 (0.0%) | 9.00 | 1 / 0 | Passed |
| L7 | Mage | unequipped | DEAD | 0.0/340 (0.0%) | 9.00 | 1 / 0 | Passed |
| L7 | Mage | win-escape | WIN | 315.5/340 (92.8%) | 51.65 | 5 / 5 | Passed |
| L7 | Mage | win-escape-distance | WIN | 257.7/340 (75.8%) | 51.45 | 5 / 5 | Passed |
| L7 | Mage | win-escape-four-sides | WIN | 304.4/340 (89.5%) | 44.90 | 4 / 4 | Passed |

The potion-help LocalizationTests and StoredLocalizationTests also passed52 with0 failures and0 skipped.

## Original failed matrix — historical results

This is the pre-approval **70-case result:58 passed,12 failed,0 skipped**, not the current gate. `win-*` names test policies, not UI labels. Warrior EDGE's old wrong-answer expectation and40% full-recipe inheritance do not describe current policy.

| Level | Class | Policy | Expected | Actual | HP / max (%) | Seconds | Result |
|---|---|---|---|---|---|---|---|
| L2 | Warrior | baseline | DEAD | DEAD | 0.0/365 (0.0%) | 4.25 | Passed |
| L2 | Warrior | direct-only | DEAD | DEAD | 0.0/365 (0.0%) | 4.30 | Passed |
| L2 | Warrior | ground-only | DEAD | DEAD | 0.0/365 (0.0%) | 4.25 | Passed |
| L2 | Warrior | position-CENTER | DEAD | DEAD | 0.0/365 (0.0%) | 9.95 | Passed |
| L2 | Warrior | position-DISTANCE | DEAD | DEAD | 0.0/365 (0.0%) | 4.30 | Passed |
| L2 | Warrior | position-EDGE | DEAD | WIN | 365.0/365 (100.0%) | 25.50 | **Failed** |
| L2 | Warrior | position-ORBIT | DEAD | DEAD | 0.0/365 (0.0%) | 4.15 | Passed |
| L2 | Warrior | potion-balanced | DEAD | DEAD | 0.0/365 (0.0%) | 4.25 | Passed |
| L2 | Warrior | potion-careful | DEAD | DEAD | 0.0/365 (0.0%) | 4.25 | Passed |
| L2 | Warrior | potion-frugal | DEAD | DEAD | 0.0/365 (0.0%) | 4.25 | Passed |
| L2 | Warrior | win-all | WIN | WIN | 365.0/365 (100.0%) | 29.15 | Passed |
| L2 | Warrior | win-attack | WIN | WIN | 340.5/365 (93.3%) | 23.05 | Passed |
| L2 | Warrior | win-balanced | WIN | WIN | 340.5/365 (93.3%) | 23.05 | Passed |
| L2 | Warrior | win-survival | WIN | WIN | 329.5/365 (90.3%) | 25.30 | Passed |
| L2 | Ranger | baseline | DEAD | DEAD | 0.0/298 (0.0%) | 4.30 | Passed |
| L2 | Ranger | direct-only | DEAD | DEAD | 0.0/298 (0.0%) | 4.40 | Passed |
| L2 | Ranger | ground-only | DEAD | DEAD | 0.0/298 (0.0%) | 4.30 | Passed |
| L2 | Ranger | position-CENTER | DEAD | DEAD | 0.0/298 (0.0%) | 4.25 | Passed |
| L2 | Ranger | position-DISTANCE | DEAD | DEAD | 0.0/298 (0.0%) | 4.30 | Passed |
| L2 | Ranger | position-EDGE | DEAD | DEAD | 0.0/298 (0.0%) | 4.25 | Passed |
| L2 | Ranger | position-ORBIT | DEAD | DEAD | 0.0/298 (0.0%) | 4.25 | Passed |
| L2 | Ranger | potion-balanced | DEAD | DEAD | 0.0/298 (0.0%) | 4.30 | Passed |
| L2 | Ranger | potion-careful | DEAD | DEAD | 0.0/298 (0.0%) | 4.30 | Passed |
| L2 | Ranger | potion-frugal | DEAD | DEAD | 0.0/298 (0.0%) | 4.30 | Passed |
| L2 | Ranger | win-all | WIN | WIN | 298.0/298 (100.0%) | 25.35 | Passed |
| L2 | Ranger | win-attack | WIN | WIN | 280.3/298 (94.0%) | 16.40 | Passed |
| L2 | Ranger | win-balanced | WIN | WIN | 280.3/298 (94.0%) | 16.40 | Passed |
| L2 | Ranger | win-survival | WIN | WIN | 273.2/298 (91.7%) | 17.60 | Passed |
| L2 | Mage | baseline | DEAD | DEAD | 0.0/265 (0.0%) | 4.30 | Passed |
| L2 | Mage | direct-only | DEAD | DEAD | 0.0/265 (0.0%) | 4.40 | Passed |
| L2 | Mage | ground-only | DEAD | DEAD | 0.0/265 (0.0%) | 4.30 | Passed |
| L2 | Mage | position-CENTER | DEAD | DEAD | 0.0/265 (0.0%) | 4.25 | Passed |
| L2 | Mage | position-DISTANCE | DEAD | DEAD | 0.0/265 (0.0%) | 4.30 | Passed |
| L2 | Mage | position-EDGE | DEAD | DEAD | 0.0/265 (0.0%) | 4.25 | Passed |
| L2 | Mage | position-ORBIT | DEAD | DEAD | 0.0/265 (0.0%) | 4.25 | Passed |
| L2 | Mage | potion-balanced | DEAD | DEAD | 0.0/265 (0.0%) | 4.30 | Passed |
| L2 | Mage | potion-careful | DEAD | DEAD | 0.0/265 (0.0%) | 4.30 | Passed |
| L2 | Mage | potion-frugal | DEAD | DEAD | 0.0/265 (0.0%) | 4.30 | Passed |
| L2 | Mage | win-all | WIN | WIN | 265.0/265 (100.0%) | 20.70 | Passed |
| L2 | Mage | win-attack | WIN | WIN | 243.7/265 (92.0%) | 19.05 | Passed |
| L2 | Mage | win-balanced | WIN | WIN | 243.7/265 (92.0%) | 19.05 | Passed |
| L2 | Mage | win-survival | WIN | WIN | 250.8/265 (94.6%) | 19.75 | Passed |
| L5 | Warrior | baseline | DEAD | WIN | 372.1/435 (85.6%) | 57.95 | **Failed** |
| L5 | Warrior | baseline-after-aggressive | DEAD | WIN | 309.3/435 (71.1%) | 30.55 | **Failed** |
| L5 | Warrior | emergency | DEAD | WIN | 372.1/435 (85.6%) | 57.95 | **Failed** |
| L5 | Warrior | emergency-after-aggressive | DEAD | WIN | 309.3/435 (71.1%) | 30.55 | **Failed** |
| L5 | Warrior | win-careful | WIN | WIN | 372.1/435 (85.6%) | 57.95 | Passed |
| L5 | Warrior | win-careful-after-aggressive | WIN | TIMEOUT | 263.6/435 (60.6%) | 90.00 | **Failed** |
| L5 | Ranger | baseline | DEAD | TIMEOUT | 148.6/354 (42.0%) | 90.00 | **Failed** |
| L5 | Ranger | baseline-after-aggressive | DEAD | DEAD | 0.0/354 (0.0%) | 28.65 | Passed |
| L5 | Ranger | emergency | DEAD | WIN | 148.6/354 (42.0%) | 86.65 | **Failed** |
| L5 | Ranger | emergency-after-aggressive | DEAD | DEAD | 0.0/354 (0.0%) | 28.65 | Passed |
| L5 | Ranger | win-careful | WIN | TIMEOUT | 251.3/354 (71.0%) | 90.00 | **Failed** |
| L5 | Ranger | win-careful-after-aggressive | WIN | TIMEOUT | 251.3/354 (71.0%) | 90.00 | **Failed** |
| L5 | Mage | baseline | DEAD | DEAD | 0.0/315 (0.0%) | 39.00 | Passed |
| L5 | Mage | baseline-after-aggressive | DEAD | DEAD | 0.0/315 (0.0%) | 36.10 | Passed |
| L5 | Mage | emergency | DEAD | DEAD | 0.0/315 (0.0%) | 39.00 | Passed |
| L5 | Mage | emergency-after-aggressive | DEAD | DEAD | 0.0/315 (0.0%) | 36.10 | Passed |
| L5 | Mage | win-careful | WIN | TIMEOUT | 190.6/315 (60.5%) | 90.00 | **Failed** |
| L5 | Mage | win-careful-after-aggressive | WIN | TIMEOUT | 223.6/315 (71.0%) | 90.00 | **Failed** |
| L7 | Warrior | attack-role | DEAD | DEAD | 0.0/470 (0.0%) | 9.50 | Passed |
| L7 | Warrior | baseline | DEAD | DEAD | 0.0/470 (0.0%) | 9.50 | Passed |
| L7 | Warrior | unequipped | DEAD | DEAD | 0.0/470 (0.0%) | 9.50 | Passed |
| L7 | Warrior | win-escape | WIN | WIN | 401.6/470 (85.5%) | 67.15 | Passed |
| L7 | Ranger | baseline | DEAD | DEAD | 0.0/382 (0.0%) | 29.00 | Passed |
| L7 | Ranger | unequipped | DEAD | DEAD | 0.0/382 (0.0%) | 9.00 | Passed |
| L7 | Ranger | win-escape | WIN | WIN | 377.5/382 (98.8%) | 39.85 | Passed |
| L7 | Mage | baseline | DEAD | DEAD | 0.0/340 (0.0%) | 9.00 | Passed |
| L7 | Mage | unequipped | DEAD | DEAD | 0.0/340 (0.0%) | 9.00 | Passed |
| L7 | Mage | win-escape | WIN | WIN | 326.6/340 (96.1%) | 45.05 | Passed |
