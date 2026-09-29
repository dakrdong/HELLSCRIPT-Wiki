# HELLSCRIPT Training Ground Rework

Updated: 2026-09-29 · Written: 2026-09-28 · Status: rules confirmed by the user, HTML mockup and native implementation done · [한국어](HELLSCRIPT_Training_Ground_Rework.md)

[Native implementation and verification](../Implementation/Training_Ground.en.md) · [HTML mockup and verification](../../Prototypes/TrainingGround/README.md) · [Previous player training design](HELLSCRIPT_Player_Training_Detail.md) · [Previous A/B comparison design](HELLSCRIPT_Training_Comparison_Detail.md)

## 1. Purpose

The training ground lets a player keep a setup fixed, change only the Hunt Edict, and see how the fight changes. The player chooses a rift tier, the enemies and the skills to use, fights, and compares clear time and average DPS with the previous successful run of the same setup. It replaces the three fixed trainings (single, pack, hazard) and the A/B comparison screen.

Every number and rule matches real combat. Enemy stats, attacks, patterns and elite traits, and the hero's skills, Hunt Edict, potions, hits and death are all computed by the real combat simulation. No training-only combat rule or multiplier is introduced.

## 2. Setup

| Item | Rule |
| --- | --- |
| Skills | Shows the four equipped actives and the ultimate. A skill whose checkbox is off is not used in this training. The checkbox is training-only and never changes the saved loadout or Hunt Edict. A skill whose edict has automatic use off is not used even when checked, and its row shows a warning. |
| Edit edict | Each skill has an **Edit edict** button that opens the existing per-skill Hunt Edict editor (quick presets; Custom for automatic use, use policy and detailed options). Saving updates the current Hunt Edict, which applies to training, rifts and repeat hunts alike. |
| Hunt Edict settings | Opens the Skills tab of the Hunt Edict window, where skills are swapped or invested and attack settings and global edicts are changed. |
| Rift tier | Tier 1 up to the highest clear + 1, the same range as rift entry. Monsters have the stats of a real rift at that tier. |
| Enemies | Up to 5 kinds, bosses and normal monsters combined. Only one boss kind, always a single boss; picking another boss swaps it. Each normal monster kind takes 1–20 and one row per kind. |
| Monster tiers | Normal monsters take a Normal, Magic, Rare or Legendary tier. Rare equals a real rift elite. Magic and Legendary are visual tiers in real combat, so their stats match Normal. |
| Records | Shows the best and previous successful clear of the current setup. |

## 3. Monster stats

Enemies are created through the rift's own path (`CombatSimulation.SpawnEnemy` and its stat function `CombatSimulation.EnemyStats`). The formula as of 2026-09-28:

- HP = 70 × 1.08^(tier−1) × kind HP factor; attack = 18 × 1.055^(tier−1) × kind attack factor.
- Bosses take HP ×45 and attack ×3 times their per-boss profile, with the same first- and second-phase patterns as in rifts.
- Tiers 1–5 carry the introductory rift reduction. Live-ops multipliers are the same as in rifts.
- Rare (elite) takes HP ×3 and attack ×1.5 and receives elite traits by `RiftGenerator`'s rule: one of homing flame, ice ring, volley barrier or rage buildup, with corpse blast added to the options when the kind has 4 or more monsters. From tier 20 (`ContentUnlocks.doubleEliteStage`) there is a 50% chance of a second trait, and homing flame and ice ring never come together. A row of two elites becomes a life-linked pair a quarter of the time. Traits are drawn per row (`TrainingGround.EliteTraits`), so changing another row never rerolls them and the lobby can show them in advance.

The game code is the single source of the formula. When the rift formula changes, the training ground changes with it; the formula is never copied into the training feature.

## 4. Battle

- The same setup always starts with the same placement and seed. Repeating it with the same edict gives the same result, so any difference comes from the edict.
- The HUD shows live DPS over the last 3 seconds, its change against the previous successful run at the same second, a chart of damage per second for this run and the previous run, average and peak DPS, total damage, enemies left, boss HP and hero HP.
- **Pause** stops combat time; paused time never counts toward the clear time. The pause screen offers Resume and Stop training; a stopped run is not recorded.
- Each normal kind waits as one pack in the start room. With a boss, it appears once every normal monster is down, 6–18 m from the hero like a roaming rift boss.
- With a boss, killing it is a success; without one, killing every normal monster is. Adds the boss summons never count, as in rifts. The run fails when the hero falls or the rift's time limit (live-ops setting, 300 s by default) runs out.

## 5. Result

| Result | Shown |
| --- | --- |
| Success | Clear time, difference from the previous successful run in seconds and percent (first, faster, slower or same), new best, average DPS change, peak DPS, total damage and the two-run DPS chart. |
| Failure | Clear time shows `No time`, with the moment the hero fell and the enemies left. A failed run is neither recorded nor compared. |
| Both | Damage share, casts and current edict summary per skill with an **Edit edict** button, and the Hunt Edict changes since the previous successful run. The footer offers **Change training setup** (back to the lobby) and **Start again** (same setup). Edicts saved on the result screen apply when the run starts again. |

## 6. Same setup and records

A setup is the hero, hero level, equipped items, rift tier, enemies (kinds, tiers and counts) and checked skills. The Hunt Edict is the variable under test and is not part of it.

Records keep the previous successful run and the best run per hero and setup: time, average and peak DPS, a per-second damage summary and the Hunt Edict used. Only successful runs are saved.

Training grants no XP, gold, materials or equipment and never changes rift records. The only lasting changes are Hunt Edict edits the player saves and the training records.

## 7. Screens and languages

Portrait 440×956, landscape 956×440 and PC 16:9, 16:10 and 21:9 must keep fixed actions and scroll areas apart. Every string has Korean and English text and larger reading sizes are supported. The 25 enemy portraits and the training-yard art were generated with Codex; see [the art manifest](../Art/TrainingGround/art-manifest.json).
