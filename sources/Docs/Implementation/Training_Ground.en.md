# Training Ground Implementation

Updated: 2026-09-29 · [한국어](Training_Ground.md)

This records how the [training ground rework](../Design/HELLSCRIPT_Training_Ground_Rework.en.md) was built into the game. The rules come from the design document and the screens from the [HTML mockup](../../Prototypes/TrainingGround/README.md); this page covers the owning code, the behaviour, the existing features that changed, and the verification.

## Ownership

| Area | Owning code | Responsibility |
|---|---|---|
| Rules and records | `TrainingGround` | Setup validation, the same-setup key, the seed, elite traits per row, the 3-second DPS average, the result summary, the previous and best run per setup (the 12 most recent setups) and the edict change list |
| Combat mode | `CombatSimulation.TrainingGround`, hooks in the constructor, `SpawnEnemy`, `SettleCombatOutcome` and `Deal` | Training index 4, the account copy, packs in the start room, the roaming boss arrival, success and failure, damage per second, automatic use off for unchecked skills |
| Enemy stats | `CombatSimulation.EnemyStats` | The single formula every spawn uses; rifts, the training ground and the lobby display share it |
| Elite traits | `RiftGenerator.RollEliteTraits`, `RollElitePair` | The rift generator's rule in the same draw order |
| Saving | `GameStore.SaveTrainingGroundSetup`, `RecordTrainingGround`, `HeroSave.trainingGround` | The lobby setup and successful runs in the hero save; a failed save restores the previous state |
| Flow | `GameController.TrainingGround` | Starts through the rift's own `BeginRun` path with a live-ops snapshot, records the end, returns to the lobby on stop or setup change, and shows the Magic and Legendary tier effects |
| Screens | `TrainingGroundWindow` (with `.Picker`), `TrainingGroundResultWindow`, `GameUI.TrainingGround`, `TrainingDpsChart` | Lobby, enemy picker and result windows, edict shortcuts, the battle DPS panel, pause dialog and Esc, and the two-run DPS chart |

The shared window (`ContentWindowView`), fonts and colours (`UiFonts`, `UiTheme`), skill icons (`SkillIconView`) and Hunt Edict editing (`HuntEdictWindow`, `GameStore.CommitHuntEdict`) are reused as they are; no new canvas, font or edict save path was added. The landscape lobby's two independently scrolling columns are recorded in the [shared UI contract](Shared_UI_Contract.en.md#training-ground).

## Behaviour

- **Entry:** The plaza's training ground, **Training ground** in the sanctuary menu and **Training ground** at the rift keeper open the lobby. The three fixed trainings and the **A/B comparison under identical conditions** buttons are gone. Class practice (tutorial H17) still opens from the tutorial list and keeps the A/B machinery underneath.
- **Lobby:** Shows the four equipped actives and the ultimate, each with a checkbox and an **Edit edict** button; a skill whose edict has automatic use off shows a warning, and the lobby says so when the recommended defaults are on. The rift tier runs from 1 to the highest clear + 1. Up to five kinds of enemies, each row showing its real HP and attack, and a Rare row its elite traits in advance. The setup is saved when the lobby closes or a run starts.
- **Battle:** A training runs on a copy of the account and leaves no rewards, XP or rift records. The header shows the elapsed time and the enemies left. The DPS panel shows the last 3 seconds, the change against the previous successful run at the same second, both runs' chart, and average, peak and total damage. The pause button, Esc and the observation menu stop combat time. **Stop training** in the pause dialog and the escape medallion go back to the lobby without a record. Editing the Hunt Edict or behaviour is blocked during a run, because an edited run could not be compared as the same setup.
- **Result:** A success shows the clear time, the change against the previous successful run in seconds and percent, first/faster/slower/same, a new best, the average DPS change and both runs' chart. A failure shows **No time** with the moment the hero fell (or the time limit), and the enemies left. Both show damage share, casts, the current edict and an **Edit edict** button per skill, and the edict changes since the previous run. Edicts saved on the result apply from **Start again**, and the result says how many are waiting.
- **Tutorials:** The training ground tutorial (UL01_TRAIN) and its unlock guide now describe the new training ground. A training that ends on its own (success, death or the time limit) counts for the training tutorial and the first-play guide's training step; a stopped one does not.

## Existing smokes changed

- `RuntimeTutorialSmoke`: the F07 step trains through the lobby's **Begin the fight** instead of the old training button.
- `RuntimeFirstPlaySmoke`: step 7 starts the training from the lobby.
- `RuntimeComparisonSmoke`: opens the A/B comparison picker directly instead of through the training page.
- `RuntimeTouchLayoutSmoke`: the training ground is no longer a page, so it left the page sweep; the new smoke checks its layout.
- `RuntimeTrainingSmoke`: removed; it tested the fixed-training page that no longer exists.

## Verification

Verification ran on an APFS clone of this work folder in Unity 6000.6.0f1. The Unity Editor the user had open and the real account save were not used. The full Edit Mode suite first ran with `main` merged up to `ac3c5183`. After `main`'s blacksmith navigation work (`b1989282`) was merged and the defect that run found was fixed, the related tests, the full suite, the macOS build and the training ground smoke were run again on the final state.

### Edit Mode tests

- The 32 new `TrainingGroundTests` passed. They check that:
  - enemy stats at each tier equal what a real rift at that tier spawns (tiers 1, 4, 10 and 37, all 20 normal kinds and 5 bosses), with the rift's live-ops balance;
  - elite traits follow the rift rule, with seven draws pinned in both this suite and the HTML mockup's, and two-elite rows can become life-linked pairs;
  - the same setup starts with the same placement and repeats the same fight; the boss arrives once every normal monster is down, and its death is a success;
  - death and the time limit fail and are not recorded, and unchecked skills stay equipped but are never cast (also under the recommended defaults);
  - damage per second adds up to the damage dealt and averages over 3 seconds; the previous and best run, the 12-setup limit, the setup key, the edict change list and refused setups work; and training changes no rewards or rift progress.
- The related tests passed: 147 before the last merge (localization, shared UI, the hunt edict overview, player training and the training ground) and 170 after it (localization, shared UI, player training, training comparison and the training ground).
- The full Edit Mode suite passed 4,661 of 4,711 tests. 47 of the 50 failures also failed in the full run on `main` on 2026-09-26 (47 of 3,971). One (`TutorialProgressionTests.EdictAndNewSkillRequireCommittedChangeThenMatchingTrainingAndFreshRift`) fails on `main` too, because of `main`'s skill tree change (`7dc11ff1`); the [blacksmith work's base comparison](ForgeNavigationEvidence/baseline-skill-failure.xml) records the same failure.
- The other two (the combat log translation tests in `CombatJournalTests` and `StoredLocalizationTests`) were a defect of this work. The lobby template `{0}/{1} 사용` matched the whole stored combat line `회오리 · 실행 확정 / 자원 0 사용`, so `실행 확정` and `자원 0` stayed in Korean on English screens. A stored line is translated with a template that matches the whole line first, and this two-hole template with no fixed text before its first hole took over lines that used to be split at ` · ` and translated piece by piece. The templates now begin and end with fixed text (`스킬 {0}/{1}개` and `적 {0}/{1}종`). After the fix, 226 related tests including those two passed, and the training ground smoke passed again on a new build.
- A second full run on the final state passed 4,663 of 4,711 tests. The 48 remaining failures are the 47 baseline failures and the tutorial test from `main` above.

### macOS development build and runtime smoke

The macOS development build completed without errors. The new `RuntimeTrainingGroundSmoke` passed end to end through real uGUI raycasts and pointer down, up and click. [Run log](TrainingGroundEvidence/runtime.txt)

- **Lobby:** skill checkbox, rift tier ±, tier and count, the picker's boss swap and five-kind cap, removing an enemy, and both edict shortcuts.
- **Battle:** the live DPS panel, combat time standing still while paused, and resuming.
- **Result and records:** the first run cleared in 30.40 s and was recorded. Started again with the same edict, the run repeated the exact clear time and reported no edict change. An edict saved on the result showed as waiting, applied on the next start and appeared in the change list.
- **Screen sizes:** the result and the lobby each passed 20 combinations (portrait 440×956, landscape 956×440 and PC 1600×900, 1600×1000 and 2100×900 × Korean and English × 100% and 150% text): fixed buttons inside the window and under the pointer, no clipped text in scrolling areas, and no Korean left on English screens.
- **Stop training:** the pause dialog's **Stop training** returned to the lobby without a record.

The smoke advances combat ticks directly to stay short. Its first runs found and fixed the result window's first-render error, the clipped result edict line in English at 150% and the clipped basic attack row.

### Screens

- Lobby: [PC Korean](TrainingGroundEvidence/lobby-1600x900-ko-100.png) · [portrait Korean](TrainingGroundEvidence/lobby-440x956-ko-100.png) · [portrait English 150%](TrainingGroundEvidence/lobby-440x956-en-150.png) · [landscape Korean](TrainingGroundEvidence/lobby-956x440-ko-100.png) · [landscape English 150%](TrainingGroundEvidence/lobby-956x440-en-150.png) · [16:10 English](TrainingGroundEvidence/lobby-1600x1000-en-100.png) · [21:9](TrainingGroundEvidence/lobby-2100x900-ko-100.png) · [enemy picker](TrainingGroundEvidence/picker-1600x900-ko-100.png)
- Battle: [PC](TrainingGroundEvidence/battle-1600x900-ko-100.png) · [portrait](TrainingGroundEvidence/battle-440x956-ko-100.png) · [landscape English 150%](TrainingGroundEvidence/battle-956x440-en-150.png) · [paused](TrainingGroundEvidence/paused-1600x900-ko-100.png)
- Result: [first clear](TrainingGroundEvidence/result-first-1600x900-ko-100.png) · [same time](TrainingGroundEvidence/result-same-1600x900-ko-100.png) · [edict waiting](TrainingGroundEvidence/result-pending-1600x900-ko-100.png) · [after the edict change](TrainingGroundEvidence/result-changed-1600x900-ko-100.png) · [portrait Korean](TrainingGroundEvidence/result-440x956-ko-100.png) · [portrait English 150%](TrainingGroundEvidence/result-440x956-en-150.png) · [landscape Korean](TrainingGroundEvidence/result-956x440-ko-100.png) · [landscape English 150%](TrainingGroundEvidence/result-956x440-en-150.png) · [16:10 English](TrainingGroundEvidence/result-1600x1000-en-100.png) · [21:9](TrainingGroundEvidence/result-2100x900-ko-100.png)

![Training ground lobby](TrainingGroundEvidence/lobby-1600x900-ko-100.png)

![Live DPS during a training](TrainingGroundEvidence/battle-1600x900-ko-100.png)

![Result after an edict change](TrainingGroundEvidence/result-changed-1600x900-ko-100.png)

### Not verified

- No phone or tablet was used; portrait and landscape were checked only as macOS window sizes. No Android or iOS build was made.
- Esc pausing was not checked with a real key press, only the pause button that calls the same handler. Ending a training through the escape medallion is not in the smoke either.
- A full real-time fight was not watched separately, because the smoke advances ticks directly.
- `RuntimeTutorialSmoke` stops at F06 before reaching the F07 training step, because of main's skill tree change (`7dc11ff1`); the same cause fails one `TutorialProgressionTests` test on main. It is unrelated to this work and was left alone.
- The edited `RuntimeFirstPlaySmoke`, `RuntimeComparisonSmoke` and `RuntimeTouchLayoutSmoke` already failed for other reasons in the 2026-09-26 baseline, so they were not rerun.
- Performance (frame rate) of long fights at high tiers with a weak hero was not measured.
