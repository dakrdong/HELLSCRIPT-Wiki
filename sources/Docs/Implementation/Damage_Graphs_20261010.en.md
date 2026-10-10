# Battle damage graphs (2026-10-10)

[한국어](Damage_Graphs_20261010.md)

Two damage-taken graphs sit under the DPS panel in a rift battle. They show at which HP a potion has to be drunk to survive. Each line reads "average damage per second · share of the hero's full HP"; with 100 HP and 30 damage per second that is 30%.

- **Monster damage graph** (always shown): the five monster kinds that hurt the hero most recently, newest first. The per-second average is a kind's damage in the last 10 seconds (`DamageGraph.MonsterWindow`) divided by the time it has been hitting (at least 3, at most 10 seconds), so a kind that just arrived is not under-read. Boss damage is not included.
- **Boss damage graph** (only while a boss is alive): the boss seal and name, then its five strongest attacks, highest damage first, each with its icon. The average is the damage of the whole fight so far divided by the time since the boss first hit the hero (at least 3 seconds).
- **Bars**: full at 50% of the hero's HP per second; gold from 20%, red from 40%.
- **Data**: `CombatStatistics.recentIncoming` (the last minute of hits, not saved) and the existing cumulative `incomingSources` feed `DamageGraph`. After a saved fight is resumed the recent list refills from empty, while the first boss-hit time is saved with cumulative damage so resuming preserves its per-second rate. Legacy saves without that time use the recorded observation start.
- **Art**: five boss seals and 33 attack icons made with Codex (`Resources/Art/Boss/`, 256 px; prompts, hashes and manifest in `Docs/Art/BossDamageGraph/`). Attack ids match what `BossCombat` records (`BOSS01_SLAM` and so on, plus the shared `BOSS_BASIC`, `BOSS_ROAR`, `LEGACY_BOSS`).
- **Scope**: ordinary rift battles. Training and tutorial battles keep their existing panels (the tutorial's "damage taken per monster kind" from level 2).
- **Feature-development verification**: `DamageGraphTests` (3) and 173 related statistics, localization and HUD tests; the macOS development-build smoke `RuntimeDamageGraphSmoke` at 440x956, 1600x900 and 956x440. Not verified: how the numbers feel in a real boss fight, physical devices.

## Integration validation (2026-10-10)

The integrated full Edit Mode run passed 5,688 tests and failed one HUD resource-registration check. Moving five unapproved Edict icons out of game Resources into candidate storage resolved it. A regression also reproduced the missing boss clock in legacy saves; normalization now uses their recorded observation interval. All 55 focused HUD, damage graph and statistics checks then passed. Four full-run exclusions are opt-in measurement and content-authoring tools. The full suite was not repeated.
