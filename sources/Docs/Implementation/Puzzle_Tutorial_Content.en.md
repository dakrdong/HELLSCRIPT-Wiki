# Puzzle tutorial content — stage 5

> 2026-10-08: The level numbers, windups and cooldowns described here record the earlier build that adjusted monster patterns. The current levels follow [Native monster patterns](Puzzle_Tutorial_Native_Patterns.en.md).

Written 2026-10-07. Implements mandatory tutorial v5 L3–L8, disclosure restrictions, scouting, actual damage reviews, the hint ladder and A/B comparisons using copies of the real hero. **The final L4 configuration and acceptance criteria are updated in the [stage 9 record](Puzzle_Tutorial_Final_Acceptance.en.md).** This is not a completed-tutorial report.

Only disclosed options may be displayed or committed. The track permits rank-one learning/equipment; skill policy editing starts at L8 and common attack order at L15. The instructor commits a real edit session. Reviews separate real deaths from duration failures and include only actual HP lost during the final five seconds. Saved hints survive restart and level four allows the instructor to apply settings.

L3 uses actual double attacks at 0/11/22 seconds and the 20-second potion cooldown. L5 uses real walking retreats and healing. L6 separates the priest guards from the archer group and proves actual support healing in every case; only authored W02/A01 aim policies follow the global target. L7 incorrect escape setups die to real E07 explosions; correct setups survive at least three cycles with zero explosion HP loss. Four-sided surround fixtures use valid landing space at a 1.6m radius. L8 compares W03/A01/M03 pack/link conditions against single-target/elite policies with identical enemies, gear and seed; comparisons leave the account and rewards unchanged.

## L4 decision history

In stage 9, the owner approved N09 and then clarified that alternative actual clears are accepted. L4 recommends Careful→Aggressive as the easier, more efficient approach; any lawful actual clear receives the same progress and XP. The following paragraph is the historical stage 5 pending-decision record.

Preserves N04×2+N10×2 followed by N01×12 and existing temperament semantics. Aggressive still dodges all ground hazards, so Careful→Aggressive is not yet the only winning path. A 0.2-second warning alternative also produced incorrect wins and a Ranger correct-answer timeout, and was rejected. All three instructor paths win, but L4 acceptance is incomplete. The proposed N09 direct attacker is awaiting the owner's decision. Outcomes are never substituted with forced deaths or choice checks.

## Checks and boundaries

61 saved-choice combat cases for L3/L5/L6/L7/L8, three persisted review/hint cases and three real comparisons passed. 71 disclosure/editing/localization checks passed (reuse 70 successful results and rerun only the one failing check after removing an unnecessary numeric-only translation). Shared UI/static refresh results are recorded separately. Full Edit Mode and native macOS smoke run once at final integrated stage 9. Native screens and physical mobile are not yet validated.

Evidence: [accepted matrix](../../Artifacts/PuzzleTutorial/20261006/stage5-accepted-matrix.json). Retain tuning failure XML/logs and synthetic saves in the same task directory. `artifact-lifecycle.json` records measured size and review on 2026-10-13; permanent deletion is not approved. Public wiki publishing remains with the main merge owner.

| Level | Class | Choice | Result | HP | Seconds |
|---|---|---|---|---|---|
| L3 | Warrior | baseline | Failed | 0.00/365 | 23.25 |
| L3 | Warrior | potion25 | Failed | 0.00/365 | 23.25 |
| L3 | Warrior | potion40 | Failed | 0.00/365 | 23.25 |
| L3 | Warrior | answer | Cleared | 87.63/365 | 34.10 |
| L3 | Ranger | baseline | Failed | 0.00/298 | 23.50 |
| L3 | Ranger | potion25 | Failed | 0.00/298 | 23.50 |
| L3 | Ranger | potion40 | Failed | 0.00/298 | 23.50 |
| L3 | Ranger | answer | Cleared | 47.79/298 | 32.00 |
| L3 | Mage | baseline | Failed | 0.00/265 | 24.80 |
| L3 | Mage | potion25 | Failed | 0.00/265 | 24.80 |
| L3 | Mage | potion40 | Failed | 0.00/265 | 24.80 |
| L3 | Mage | answer | Cleared | 69.28/265 | 32.50 |
| L5 | Warrior | baseline | Failed | 0.00/435 | 3.70 |
| L5 | Warrior | emergency | Failed | 0.00/435 | 3.70 |
| L5 | Warrior | balanced | Failed | 0.00/435 | 3.70 |
| L5 | Warrior | answer | Cleared | 435.00/435 | 43.30 |
| L5 | Mage | baseline | Failed | 0.00/315 | 3.70 |
| L5 | Mage | emergency | Failed | 0.00/315 | 3.70 |
| L5 | Mage | balanced | Failed | 0.00/315 | 3.70 |
| L5 | Mage | answer | Cleared | 315.00/315 | 46.45 |
| L6 | Warrior | baseline | Failed | 430.36/435 | 90.00 |
| L6 | Warrior | nearest | Failed | 430.36/435 | 90.00 |
| L6 | Warrior | pack | Failed | 430.36/435 | 90.00 |
| L6 | Warrior | answer | Cleared | 435.00/435 | 66.30 |
| L6 | Mage | baseline | Failed | 315.00/315 | 90.00 |
| L6 | Mage | nearest | Failed | 315.00/315 | 90.00 |
| L6 | Mage | pack | Failed | 315.00/315 | 90.00 |
| L6 | Mage | answer | Cleared | 315.00/315 | 73.25 |
| L7 | Warrior | baseline | Failed | 0.00/470 | 9.50 |
| L7 | Warrior | unequipped | Failed | 0.00/470 | 9.50 |
| L7 | Warrior | attack-role | Failed | 0.00/470 | 9.50 |
| L7 | Warrior | answer | Cleared | 414.50/470 | 45.50 |
| L7 | Warrior | distance | Cleared | 388.00/470 | 63.45 |
| L7 | Warrior | four-sides | Cleared | 355.98/470 | 67.25 |
| L7 | Mage | baseline | Failed | 0.00/340 | 9.00 |
| L7 | Mage | unequipped | Failed | 0.00/340 | 9.00 |
| L7 | Mage | answer | Cleared | 304.40/340 | 35.75 |
| L7 | Mage | distance | Cleared | 313.24/340 | 36.90 |
| L7 | Mage | four-sides | Cleared | 297.59/340 | 35.35 |
| L8 | Warrior | baseline | Failed | 452.95/470 | 90.00 |
| L8 | Warrior | pack | Failed | 452.95/470 | 90.00 |
| L8 | Warrior | answer | Cleared | 434.98/470 | 59.15 |
| L8 | Mage | baseline | Failed | 287.99/340 | 90.00 |
| L8 | Mage | pack | Failed | 287.99/340 | 90.00 |
| L8 | Mage | answer | Cleared | 330.02/340 | 83.05 |
| L6 | Ranger | baseline | Failed | 354.00/354 | 90.00 |
| L6 | Ranger | nearest | Failed | 354.00/354 | 90.00 |
| L6 | Ranger | pack | Failed | 354.00/354 | 90.00 |
| L6 | Ranger | answer | Cleared | 354.00/354 | 84.80 |
| L5 | Ranger | baseline | Failed | 0.00/354 | 3.70 |
| L5 | Ranger | emergency | Failed | 0.00/354 | 3.70 |
| L5 | Ranger | balanced | Failed | 0.00/354 | 3.70 |
| L5 | Ranger | answer | Cleared | 354.00/354 | 46.05 |
| L7 | Ranger | baseline | Failed | 0.00/382 | 19.00 |
| L7 | Ranger | unequipped | Failed | 0.00/382 | 9.00 |
| L7 | Ranger | answer | Cleared | 382.00/382 | 35.10 |
| L7 | Ranger | distance | Cleared | 326.72/382 | 31.15 |
| L7 | Ranger | four-sides | Cleared | 382.00/382 | 31.85 |
| L8 | Ranger | baseline | Failed | 306.10/382 | 90.00 |
| L8 | Ranger | pack | Failed | 306.10/382 | 90.00 |
| L8 | Ranger | answer | Cleared | 326.06/382 | 77.55 |
