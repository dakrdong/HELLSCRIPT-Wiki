# Puzzle tutorial: native monster patterns (2026-10-08)

Korean: [몬스터 고유 패턴 원칙](Puzzle_Tutorial_Native_Patterns.md)

Owner decision: **a level's answer is never made by changing a monster's attack pattern.** Only monster level (stats), health, damage, count and spawn time may be tuned. The earlier build forced outcomes with a 0.1 s windup, a 300 s repeat cooldown, zero move speed and twelve monsters at 1000 HP, and the owner pointed out that "attacks once and never again" looks cheap. The previous level-design records (core, content, operations, graduation) stay as history of that build; this document is the reference for the current levels.

## Removed levers

| Lever | Before | Now |
| --- | --- | --- |
| Windup override `preparation` | L2, L3, L4, L5, L10, L12-L15 | field deleted, the monster's own windup |
| Repeat cooldown override `repeatCooldown` | L3, L4, L5, L6, L10, L12-L15 (300 s in L3) | field deleted, its own cooldown |
| Opening cooldown `cooldown`, `cooldownOffset` | every level | fields deleted, the same draw a normal rift spawn makes |
| Move speed `speed` | L4 1, L6 0 (standing still), L14 0.5 and others | field deleted, the monster's own speed |
| Windup injected inside tests | `PuzzleLevelTests` prototypes | file deleted, replaced by real-level checks |

Kept: `kind`, `count`, `elite`, `boss`, `afterClear`, `pauseBefore`, `at`, `position`, `offset`, `health`, `attack` (`attackMaxHp`). The start edict (`start`, `presets`) is the hero's side of the level, not a monster pattern.

## Current design per level

Each level teaches one thing. From the start state the hero really dies or times out; saving the answer really wins. Numbers live in `PuzzleTutorial.json`; results are EditMode simulations (seed 77123).

| Level | Teaches | Why the start state fails | Answer |
| --- | --- | --- | --- |
| 1 | Learn and slot the first skill | (cannot start without the skill) | slot the skill, defeat four hounds |
| 2 | Dodging the big attack | the sandworm hits the hero's spot after its warning for 130% of max HP; with dodging off the hero dies in 3-8 s | `Avoid all warnings` (no damage for all three classes) |
| 3 | Turning automatic potions on | the start edict has potions, dodging and retreat off; six marauders' attacks land and the hero dies | any `Automatic potion` setting (`Extra survival supplies` recommended) |
| 4 | A pressing style | the start is `Careful`, which dodges even the small hits of 14 weak corpses and times out at 60 s | `Aggressive` (or `Balanced`) |
| 5 | Falling back | retreat is off, so the hero stands against four slow, heavy iron wraiths and dies | `Survival first` + `Retreat on foot` (other retreat settings clear too) |
| 6 | Dangerous enemies first | the hero cuts the nearest corpses while two archers shoot from afar and kill it (Warrior, Ranger) | `Dangerous enemies first` |
| 7 | Slotting an escape skill | the sorcerer's `Gravity Collapse` cannot be walked out of and the explosion kills | `Chosen escape skill` (class movement skill) |
| 8 | How a skill is used | a condition that waits for a group never fires with one enemy left, so time runs out | single or strong-enemy usage |
| 9 | Filtering loot | picking up everything fills the bag, going far steps into the hazard | `Rare and better` + `Nearby loot only` |
| 10 | Cursed chests | opening at 50% HP cannot clear the four echoes in 12 s; never opening misses the goal | `Attempt with a safety margin` |
| 11 | Bag cleanup | returning, ignoring or keeping everything cannot reach the goal (80 gold, one free slot) | replace + sell normal/magic + protect invested |
| 12 | Auto Equip | with it off the weaker weapon cannot kill the elite in 45 s | Auto Equip on + same weapon type |
| 13 | Repeat hunting | one run, same stage or holding the failed stage misses the gold target | next stage after a win, one stage down after a loss |
| 14 | Equipment enhancement | heavy melee blows that cannot be dodged hit a hero with no extra defence and kill it (dodging, retreat and potions are off at the start) | in the blacksmith that opens after the first loss, `Enhance max` on the armor (+15 or more is needed, the trial gold reaches +30) |
| 15 | Three faces of danger | turning off any of ground zones, ranged direct hits or instant areas kills | all three `Always avoid` + cancelling allowed |
| 16 | Graduation | the boss kills the unconfigured hero | everything learned before and the tempered armor |

## Class exceptions and known limits

- **Mage in L6:** the fireball weighs explosion efficiency before the global target, so the Mage also clears with `Nearest enemy first`. Warrior and Ranger need `Dangerous enemies first`.
- **Warrior in L15:** turning off only instant areas can be survived by walking, so the Warrior's worm has 1400 HP to keep it fighting the worm long enough; then the hero dies. The margin is narrow, recheck when numbers change.
- **Accepted second clear in L15:** for the Warrior and the Mage, `Maintain attacks` plus finishing the current attack also clears (the Ranger dies to the heavier crossbow hits); avoiding ground zones and only the big hits is enough against the monsters' own patterns.
- **L5 retreat:** `Emergency retreat` and `Balanced survival` clear as well; what blocks the level is having retreat off.
- **`Balanced avoidance` (big hits):** against L2's sandworm blow (98% of max HP) the Ranger and Mage die with this setting while `Avoid all warnings` dodges it. Logged as a question about the game's dodge decision; not changed here.

## Traps found in the monsters' own AI

- The hero dodges telegraphed attacks by itself. Levels that need damage (L3, L5, L6, L10) start with dodging off.
- A standing Warrior (reach 2 m) cannot hit 8 m archers, chargers or crossbowmen. Keep enemies close to the hero or use only reachable kinds.
- The fanatic charger backs off after charging and never ends an exchange with a standing Warrior (removed from L3).
- The priest's 3 m heal radius against its 6 m approach distance, plus area skills hitting the priest too, make a healer-first level come out differently per class, so none is used.
- A hero's single-target damage is low (roughly 20-40 per second), so raising one monster's health a lot leaves the Warrior unable to finish in time.

## The equipment-enhancement level (L14)

The owner's third example, "a difficulty that needs more defence, solved with equipment enhancement", is now the sixteenth level. It uses the hub's `Blacksmith` (PR #78); there is no separate maintenance window.

- **Flow:** L14 starts with dodging and retreat off. The first attempt dies to four iron wraiths' heavy melee blows. Once the first loss is recorded, only the blacksmith's `Equipment enhancement` opens (`ContentUnlocks.Has` asks `PuzzleTutorial.ForgeOpen`; no unlock is granted to the account) and 8,100 trial gold is paid out. Temper the armor and try again; on the win the unspent gold is taken back (restored from `forgeBaseGold`), the tempered armor stays with the hero, and the blacksmith shuts again. A replay hands the starting hero a fresh trial purse.
- **Numbers (defence and damage only):** each enhancement step on the worn armor piece adds +3 armor. Starting armor is 192 for the Warrior and 56 for the Ranger and Mage, and incoming damage is cut by armor/(armor+110). With the wraiths' damage set to 0.275 (Warrior) and 0.42 (Ranger, Mage) of max HP, the hero dies untempered and still dies at +10, and wins from +15. The 8,100 trial gold reaches +30, so one `Enhance max` leaves 12% (Warrior), 27% (Ranger) and 27% (Mage) of HP. Cumulative cost on a level-1 armor piece is 1,429 for +10, 3,931 for +20 and 8,055 for +30.
- **Maintenance P2 removed:** the non-combat +1 weapon enhancement with 110 trial gold (before L12) is replaced by this level. The resistance-gear comparison P1 (before L10) stays.
- **Renumbering:** Three faces of danger is now L15 and Graduation is L16. The tempered armor carries into the later levels, so L15 and L16 were retuned.
- **Hints:** there is no edict for the Commander to write here, so the four hints end at the blacksmith and there is no instructor setup.

## Verification and tools

- Probes: `PuzzleContentTests.Probe` (L1-L8) and `PuzzleOperationTests.ProbeOperations` (L10, L12-L15) read a JSON plan from `PUZZLE_PROBE_FILE`, tune health, damage and counts in memory only, and print per-class results. They are ignored without the variable.
- Early focused checks (13 puzzle, tutorial and translation classes): 363 of 371 passed; the 6 failures were old-design expectations (L1 hero-level check, L14 attack-finish combination) and were corrected.
- Full Edit Mode (final state with the equipment-enhancement level): 5648 of 5652 passed, 2 skipped (the probes). The 2 failures (`MeasureEveryDesignBuild`, `Capture`) are measurement and capture tools that fail without their output-folder environment variable and are unrelated to this change.
- macOS development-build smoke (`-hellscriptTutorialSmoke`, 1280x720, disposable save path, Warrior): all 16 levels cleared through the UI, the first attempts at L2, L7 and L14 failed from real damage, +30 armor through the blacksmith window after the first loss, maintenance P1, graduation and Anton's first greeting, ending in `HELLSCRIPT_TUTORIAL_RUNTIME_SMOKE_OK`. One resolution and one class only.
- Not checked on a physical mobile device.
