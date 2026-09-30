# Training Ground Implementation

Updated: 2026-09-30 · Written 2026-09-29 · [한국어](Training_Ground.md)

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
| Screens | `TrainingGroundWindow` (`.Skills`, `.Enemies`, `.Hero`, `.Picker`), `TrainingGroundResultWindow`, `GameUI.TrainingGround`, `TrainingChartView`, `TrainingDpsChart` | Lobby (skill column, enemy column, training-yard panel, bottom bar), enemy picker and result windows, edict shortcuts, the battle DPS panel (foldable), pause dialog and Esc, and the two-run DPS chart with skill marks and an inspection tooltip |
| Skill cast log | `TrainingGroundRunState.casts`, `CombatSimulation.RecordTrainingCasts`, `TrainingGround.MarkedOnGraph` and `CastsIn` | Dates every release the combat statistics count (to the middle of its tick). Decides which skills get an icon on the graph (cooldown of 3 s or more, ultimates included) and groups casts by second for the tooltip. |
| Shared parts | `UiIconButton`, `UiDropdown`, `RiftSkillShareView` | Label-free icon buttons (pencil, fold, remove), drop-downs (tier, count) and the skill damage card shared with the rift result (with an optional pencil button). |
| Art | `Resources/Art/TrainingGround/Icons`, `TrainingGroundArtImporter`, `ResourceTextureBudget` | Four white pictograms drawn by Codex (pencil, book, remove X, crown), tinted in the game. [Image provenance](../Art/TrainingGround/training-ground-icons-manifest.json) |

The shared window (`ContentWindowView`), fonts and colours (`UiFonts`, `UiTheme`), skill icons (`SkillIconView`), the slider (`SettingsStepSlider`) and Hunt Edict editing (`HuntEdictWindow`, `GameStore.CommitHuntEdict`) are reused as they are; no new canvas, font or edict save path was added. The landscape lobby's two independently scrolling columns are recorded in the [shared UI contract](Shared_UI_Contract.en.md#training-ground).

`RiftSkillShareView` is the skill card made by the rift result rework (PR 40, not merged yet). So that the training result uses the same card, the same file (same content and GUID) is in this work too, with only an optional `edit` argument that adds a pencil button at the card's top right and a `MinHeight` helper on top. When both works are merged the file can be resolved to either side (this one is the superset).

## Behaviour

- **Entry:** The plaza's training ground, **Training ground** in the sanctuary menu and **Training ground** at the rift keeper open the lobby. The three fixed trainings and the **A/B comparison under identical conditions** buttons are gone. Class practice (tutorial H17) still opens from the tutorial list and keeps the A/B machinery underneath.
- **Lobby:** Built like the mockup (2026-09-30). Landscape screens put the two section headings (`01 Skills`, `02 Enemies` with their counts and `+ Add enemy`) in the fixed navigation row, and the two columns scroll on their own. From 21:9 (900 units wide) a training-yard panel (backdrop, title, this setup's best and previous clear) joins on the left; portrait stacks that panel, the skills and the enemies on one page.
  - **Skills:** The four equipped actives and the ultimate, each with a checkbox, the current edict summary and a label-free **pencil icon button** (edit edict); a skill whose edict has automatic use off shows a warning. The **Hunt Edict settings** shortcut is a wide gilt button with a book pictogram docked under the skill column (right under the heading in portrait, with the basic-attack note). On landscape screens all five equipped skills fit without scrolling (larger text sizes scroll).
  - **Enemies:** The rift tier is one line: −/+ buttons, a slider and the tier value, from tier 1 to the highest clear + 1, with −10/+10 buttons once more than 40 tiers are open. Dragging the slider only moves the value; the lobby is rebuilt on release. Up to five kinds, in low rows: a borderless × on the portrait's top-right corner, name and role with the real HP and attack, and for a normal monster a **tier drop-down** and a **count drop-down** (1–20) side by side on the right. A boss row says `Always 1`. When the row is narrow the drop-downs move under the text. A Rare row adds its elite traits and a Magic or Legendary row a note that its stats match Normal.
  - **Bottom bar:** Landscape keeps the summary (tier, kinds, count, total HP) and this setup's best and previous clear on the left and **Begin the fight**, with its subtitle (`Timed until every enemy falls`), on the right; portrait puts the summary in the navigation row. The setup is saved when the lobby closes or a run starts.
- **Battle:** A training runs on a copy of the account and leaves no rewards, XP or rift records. The header shows the elapsed time and the enemies left. The DPS panel shows the last 3 seconds, the change against the previous successful run at the same second, both runs' chart, and average, peak and total damage. On the chart, every cast of a skill with a cooldown of 3 seconds or more (ultimates included) puts that skill's icon on the line. An **arrow button** at the panel's top left folds it to one live-DPS line and unfolds it again; the choice lasts while the app stays open. The pause button, Esc and the observation menu stop combat time. **Stop training** in the pause dialog and the escape medallion go back to the lobby without a record. Editing the Hunt Edict or behaviour is blocked during a run, because an edited run could not be compared as the same setup.
- **Result:** A success shows the clear time, the change against the previous successful run in seconds and percent, first/faster/slower/same, a new best, the average DPS change and both runs' chart. Pointing at the chart with a mouse or pressing it with a finger shows a crosshair and a tooltip with both runs' DPS in that second and the **names of the skills cast in that second** (Whirlwind and Crushing Strike too, although they get no icon); after a touch it stays until the next touch elsewhere, and a vertical swipe scrolls the page. A failure shows **No time** with the moment the hero fell (or the time limit), and the enemies left. Damage by skill is the rift result's card: four skills in two columns, the ultimate across, each with a large icon, name, casts, gauges for the share of damage done to normal monsters and to bosses, and a pencil icon button (edit edict). A skill that was not used is dimmed and says `Not used`. The edict changes since the previous run follow. Edicts saved on the result apply from **Start again**, and the result says how many are waiting.
- **Tutorials:** The training ground tutorial (UL01_TRAIN) and its unlock guide now describe the new training ground. A training that ends on its own (success, death or the time limit) counts for the training tutorial and the first-play guide's training step; a stopped one does not.

## Existing smokes changed

- `RuntimeTutorialSmoke`: the F07 step trains through the lobby's **Begin the fight** instead of the old training button.
- `RuntimeFirstPlaySmoke`: step 7 starts the training from the lobby.
- `RuntimeComparisonSmoke`: opens the A/B comparison picker directly instead of through the training page.
- `RuntimeTouchLayoutSmoke`: the training ground is no longer a page, so it left the page sweep; the new smoke checks its layout.
- `RuntimeTrainingSmoke`: removed; it tested the fixed-training page that no longer exists.

## Verification

### 2026-09-30 mockup port

Verification again ran on an APFS clone of this work folder in Unity 6000.6.0f1; the Editor the user had open and the real account save were not used. The full Edit Mode suite and the macOS development build's training ground smoke ran on the final state with `main` merged up to `513b940b`. After the suite started only the smoke code (the phone-width DPS strip step), the documents and the evidence changed.

- **12 new Edit Mode tests.** `TrainingGroundTests` gained 3 (skill releases are logged with their time and agree with the statistics, only skills on a cooldown of 3 s or more get a graph icon, casts group by whole second) and the new `TrainingGroundUiTests` has 9: the graph's skill icons (only marked skills, only recorded seconds, inside the chart, only the latest in a long fight), the tooltip (the skills of that second, also those without an icon, both runs' values, inside the chart at both ends), an empty graph, the drop-down, the icon button and the card's pencil (equipped skills only).
- **Related tests pass.** Training ground (35), its screen parts (9), localization, saved-log localization, shared UI, combat journal and build resources all pass. The localization tests caught one duplicate key and seven orphaned entries (removed wording), which were fixed.
- **Full Edit Mode suite.** 4,896 tests ran, 4,849 passed and 47 failed (about 32 minutes). The 47 failures are exactly the list that already failed in the 2026-09-26 `main` full run (`ActionContinuityTests`, `CurrentBuildSaveTests`, `RestoreFidelityTests` and others), so this work adds none; all 44 training ground tests pass. The commit tested is `4ee7d96c`, after the cast-mark cap.
- **Training ground smoke, 51 lines pass.** Driven by real uGUI raycasts and pointer press, release and click. [Interaction record](TrainingGroundEvidence/runtime.txt)
  - Lobby: all five equipped skills fit without scrolling at 16:9 landscape; pencil buttons, the Hunt Edict shortcut under the skills, tier and count drop-downs, the boss's `Always 1` and the four Codex pictograms are there. A drop-down opens by tap and an entry is chosen (the long count list is scrolled first); pressing the slider raises the rift tier and the value reaches the lobby on release.
  - Battle: folding the graph leaves one live-DPS line and unfolding brings it back. At phone width (420×900, text 150%) the strip under the header folds and unfolds.
  - Result: skill cards with normal and boss gauges and a pencil each, four skill icons on the graph, and a tooltip at a hovered second that names the skill and disappears when the pointer leaves. Restarting with the same edict repeated the clear time exactly (27.75 s).
  - Screen sizes: the lobby and the result each in 20 combinations (portrait 440×956, landscape 956×440, PC 1600×900, 1600×1000 and 2100×900 × Korean and English × text 100% and 150%): fixed buttons inside the window and reached by the pointer (retried for a few frames right after a rebuild), no clipped text in scrolling areas, no Korean left in English. The training-yard panel appears only on portrait and 21:9.
- **UI style smoke** (1440×810 Korean) passes; the training lobby of a new hero without skills (four empty slots, no ultimate) is drawn correctly too. It ran on the build before the icon cap and was not repeated after it (the cap does not touch the lobby).
- **Shared UI contract check** `check_ui_contract.py` and its 11 tests pass; the first run made me replace two colour literals in the hero panel with `UiTheme` tokens.
- **The HTML mockup** was rebuilt with `node Prototypes/TrainingGround/build.cjs` (only the source hashes changed) and its 183 headless Chrome checks pass.

Found and fixed while running the smoke: the turned fold arrow vanished (rotation pivot), drop-down entries below the visible part could not be pressed (the smoke scrolls the list), overlapping text on the English 150% result cards (one column now), the folded DPS panel's comparison text cut off (two rows), the start button's subtitle cut off at large text, the side panel showing on a phone held sideways (now only at a ratio of 2.2 or more) and the old fixed-skill fixture (new accounts start without skills).

The three sections below record the first implementation of 2026-09-29.

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
- Battle: [PC](TrainingGroundEvidence/battle-1600x900-ko-100.png) · [PC, panel folded](TrainingGroundEvidence/battle-folded-1600x900-ko-100.png) · [phone-width strip, 150%](TrainingGroundEvidence/battle-420x900-ko-150.png) · [strip folded](TrainingGroundEvidence/battle-420x900-folded-ko-150.png) · [portrait](TrainingGroundEvidence/battle-440x956-ko-100.png) · [landscape English 150%](TrainingGroundEvidence/battle-956x440-en-150.png) · [paused](TrainingGroundEvidence/paused-1600x900-ko-100.png)
- Result: [graph tooltip](TrainingGroundEvidence/result-tooltip-1600x900-ko-100.png) · [first clear](TrainingGroundEvidence/result-first-1600x900-ko-100.png) · [same time](TrainingGroundEvidence/result-same-1600x900-ko-100.png) · [edict waiting](TrainingGroundEvidence/result-pending-1600x900-ko-100.png) · [after the edict change](TrainingGroundEvidence/result-changed-1600x900-ko-100.png) · [portrait Korean](TrainingGroundEvidence/result-440x956-ko-100.png) · [portrait English 150%](TrainingGroundEvidence/result-440x956-en-150.png) · [landscape Korean](TrainingGroundEvidence/result-956x440-ko-100.png) · [landscape English 150%](TrainingGroundEvidence/result-956x440-en-150.png) · [16:10 English](TrainingGroundEvidence/result-1600x1000-en-100.png) · [21:9](TrainingGroundEvidence/result-2100x900-ko-100.png)

![Training ground lobby](TrainingGroundEvidence/lobby-1600x900-ko-100.png)

![Live DPS during a training](TrainingGroundEvidence/battle-1600x900-ko-100.png)

![Result after an edict change](TrainingGroundEvidence/result-changed-1600x900-ko-100.png)

### Not verified

Not checked in the 2026-09-30 port:

- Pressing and swiping the result graph with a finger. The smoke used mouse-move style synthetic events only; the touch path (`ExtendedPointerEventData`) and the hand-off of a vertical swipe to the page scroll did not run.
- A drop-down flipping upward at the bottom screen edge; every row the smoke chose is near the top.
- The phone-width DPS strip was seen only in Korean at 150%. English or 100% strips and a folded landscape at 150% were not captured.
- Android and iOS builds and how the four new icons compress (ASTC 4×4 at 128 px; the macOS build does not use that compression).
- Resolving `RiftSkillShareView` when this work and PR 40 merge (the same file and four duplicated translation lines) is left to that merge.
- Older smokes not re-run for this work: `RuntimeFirstPlaySmoke` (the start button's name is unchanged), `RuntimeTutorialSmoke`, `RuntimeComparisonSmoke` and `RuntimeTouchLayoutSmoke`; they already failed on the 2026-09-29 baseline for other reasons.

Not checked in the first implementation of 2026-09-29:

- No phone or tablet was used; portrait and landscape were checked only as macOS window sizes. No Android or iOS build was made.
- Esc pausing was not checked with a real key press, only the pause button that calls the same handler. Ending a training through the escape medallion is not in the smoke either.
- A full real-time fight was not watched separately, because the smoke advances ticks directly.
- `RuntimeTutorialSmoke` stops at F06 before reaching the F07 training step, because of main's skill tree change (`7dc11ff1`); the same cause fails one `TutorialProgressionTests` test on main. It is unrelated to this work and was left alone.
- The edited `RuntimeFirstPlaySmoke`, `RuntimeComparisonSmoke` and `RuntimeTouchLayoutSmoke` already failed for other reasons in the 2026-09-26 baseline, so they were not rerun.
- Performance (frame rate) of long fights at high tiers with a weak hero was not measured.
