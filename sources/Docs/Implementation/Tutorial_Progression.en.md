# Tutorial implementation and validation

Updated: 2026-09-25

The [design](../Design/Tutorial_Flow_Design.en.md) connects 38 guide groups to the existing game loop: dedicated map, real inventory equipment, town, rifts, Hunt Edict, skill changes and content practice.

## Ownership

`AccountGuide` owns versioned read, practiced, deferred and hidden records. `Tutorials` defines the 38 groups and resolves unlocks from the existing content catalog. `TutorialProgress` observes committed gameplay. Existing `HeroGuide.completed` fields preserve old-save compatibility; the player-facing entrypoint is the shared journal.

`CombatSimulation.Tutorial` owns a fixed three-room map with real enemies and a boss. `GameStore.Tutorials` owns map admission, once-only common armor and completion. `GameController.Tutorials` routes the first account character and restores the saved checkpoint. Tutorial runs carry an explicit flag, use actual-level stats and remain outside ordinary rift economy, fatigue, progression, records and offline supply baselines. Required armor cannot be unprotected, unequipped or transferred to storage before completion. Rewardless replay uses an independent level 1 account copy.

## Saves and transactions

Save schema 17 preserves released live-operations schema 16 saves and adds tutorial definition version 1. Legacy accounts receive an exemption, not fabricated completion. Equivalent actual entry/result/retry, owned training and genuine equipment comparison facts migrate; fallback comparison explanations do not count as practice; the old combined read/skip acknowledgement migrates only as read. Support receipts and armor grants do not reset when copy changes. New account identity is initialized before the first save and stays stable across restart.

`GameStore.Transact` adopts guide state only after disk success. Practice quotes capture the selected owned item, choice and current prerequisites. NPC distance and domain validation run again at execution. Support, consumption, actual result and the support receipt commit atomically. Cancelling, stale selection, capacity failure or disk failure retains support.

| Practice | One supported operation | Existing domain owner |
| --- | --- | --- |
| Enhance | Owned +0 → +1, gold | `GearEnhancement` |
| Rare craft | One selected slot, gold and materials | `ContentServices.Purchase` |
| Slot upgrade | Level 1 → 2, stones, normal 300 seconds | `BlacksmithCatalog.Start` |
| Gem fusion | Five selected T1 → one T2 | `Jeweler.Convert` |
| Rune | One G0 single-cell rune, gold | `TownTrade.BuyRune` |
| Unidentified item | One slot, current price and normal probabilities | `GambleShop` |
| Reroll | One actual affix, one attempt, gold | `Economy.Reroll` |
| Elixir | One selected T1 gem → one potion | `Jeweler.Craft` |
| Masterwork | Actual +5 equipment, masterwork 0 → 1 | `ItemQuality.Advance` |
| Core craft | Ten slot cores, zero premium investment | `CoreCrafting` |

Training, offline supplies, aspect imprinting and sweeping use existing free operations and limits. Tutorial support does not invent prerequisites, levels, legendary items, aspects or additional sweep attempts.

## UI and sequencing

All new text uses the existing Korean keys and English localization table. Equipment and encounter variants have separate, localized explanations.

`TutorialJournalWindow` was scaffolded with `tools/new_content_ui.py`. `ContentWindowView` owns the safe-area frame, navigation, scrolling body and actions. Actual inventory and shared details/comparison retain ownership of equipment UI. `TutorialAnchorRing` attaches to logical targets and follows their transforms through reflow. The journal records reading separately from actual execution.

Mandatory map explanations pause safely. Regular rifts show nonblocking tips; full guidance stays in town. First content usage offers its guide with read, later and hide actions. Active practice takes priority. Attendance recording and offline settlement continue while their automatic popups are deferred during the first map, initial departure preparation and active combat.

Implementation order: owners and isolated main worktree; versioned state and atomic support; map and checkpoint recovery; core gameplay facts; unlock and contextual guides; Edit Mode and native macOS acceptance; documentation/wiki generation and branch delivery. Public wiki deployment occurs only from merged main.

## Validation status

Final validation uses Unity 6000.6.0f1 and main commit `32baf0fcb3d0d8872eb9188c699604b52e6a432f`. The initial full audit used `bdf659139c7705f59047eccd57111b07eccf1d66`. The original checkout's 408 art metadata changes were preserved. A separate project with matching game sources built the macOS player without touching the user's running Editor.

| Check | Result |
| --- | --- |
| Final focused Edit Mode regression | 538 / 538 passed, 0 skipped after latest-main integration, covering tutorials, save migration, transaction failures, existing content, live operations and asynchronous admission |
| Shared UI ownership | Passed; validator tests 9 / 9 |
| Native macOS development build | Succeeded, 0 build errors |
| Final native runtime | Passed separate-process restore, equipment, boss, town, first rift, edict and new skill saves, owned training/fresh retry, supported enhancement and rewardless replay. The new skill unlocked through actual first-rift growth; no level fixture was used. |
| Layout matrix | Intro, journal and supported practice each cover 20 combinations: 440×956, 956×440, 1600×900, 1600×1000, 2100×900 × KO/EN × 100%/150% text |
| Physical Android/iOS | Not verified; macOS synthetic pointer and window-size evidence is separate |
| Human 4–6 minute target and comprehension | Not verified |
| Main integration | [PR #11](https://github.com/dakrdong/HELLSCRIPT/pull/11) merged on 2026-09-25 as `613e95689a8cb35c9a57503137d20606d5fe38fb`. The merged game code matches validated feature commit `85206553191054b8bfe724fec009f076b21d1102`. |
| Public wiki release path | Generate the complete read-only mirror of documents, history, databases, images and source evidence from merged main. The [public wiki](https://dakrdong.github.io/HELLSCRIPT-Wiki/#/page/tutorial-progression.en) requires successful Pages deployment and logged-out verification of the actual pages. |

### Broad regression audit

Before the latest live-operations integration, the full audit ran 3,923 cases: 3,875 passed and 48 failed. This is not reported as a green full suite.

Four feature-related failures were corrected and are covered by the final 538 cases: absent/foreign edicts must remain inactive without preventing combat, Unity component null checks, and the highlight graphic's required `CanvasRenderer` declaration.

Independent copies of both the original base and latest main reproduced 43 failures with identical test names: 36 `ActionContinuityTests`, 4 `CurrentBuildSaveTests`, and 3 `RestoreFidelityTests`, all concerning combat-journal JSON equality. Those combat journal implementations are outside this change. The remaining unseeded dungeon assertion was separately reproduced on base main at stage 6, seed 2: one optional goblin raises its count from the expected 150 to 151; excluding the goblin gives 150.

Evidence: [comparison](../../Artifacts/Validation/tutorial-progression/regression-comparison.json), [full audit](../../Artifacts/Validation/tutorial-progression/editmode-full-audit.xml), [original baseline](../../Artifacts/Validation/tutorial-progression/editmode-baseline.xml), [original goblin result](../../Artifacts/Validation/tutorial-progression/baseline-goblin.txt), [latest-main goblin result](../../Artifacts/Validation/tutorial-progression/baseline-goblin-current.txt), and [reproducer source](../../Artifacts/Validation/tutorial-progression/baseline-goblin-current-probe.cs.txt). The final scope is [538 integrated cases](../../Artifacts/Validation/tutorial-progression/editmode-integrated.xml). Earlier [440](../../Artifacts/Validation/tutorial-progression/editmode-focused.xml) and [66](../../Artifacts/Validation/tutorial-progression/editmode-additional.xml) runs are history, not additional final coverage.

### Native acceptance

`RuntimeTutorialSmoke` runs only in a development player with the explicit smoke flag, disposable save path and evidence path. It checks real EventSystem raycasts before synthetic pointer clicks. Combat wins and tutorial completion are not injected. The first process exits at the actual armor checkpoint; a second process uses the same save with `-hellscriptTutorialResume`. It checks exactly one armor, comparison/equip, real boss death and town arrival, first-rift results, committed edict/skill changes, owned training, matching fresh rift, supported enhancement and rewardless replay. Late-content UI uses an explicit highest-clear-120 fixture, not a natural-progression claim.

The [build result](../../Artifacts/Validation/tutorial-progression/build-result.txt) and [source comparison](../../Artifacts/Validation/tutorial-progression/source-parity.json) accompany the runtime evidence. Generated art import metadata, material version metadata, Editor settings and unrelated floating-point evidence churn are excluded from the feature commit.

The Korean [implementation record](Tutorial_Progression.md) contains the detailed ownership table.

Integration uses tutorial schema 17 to preserve released schema 16 accounts and their pinned live-operations runs. Starting replay cancels pending rift admission. Rift guidance describes the time limit fixed at admission. The [latest-main baseline](../../Artifacts/Validation/tutorial-progression/editmode-baseline-current.xml) is retained separately.

The final runtime fetched and pinned an HTTP [loopback release of shipped defaults](../../Artifacts/Validation/tutorial-progression/loopback-liveops-release.json) for normal admission. This does not verify the public cloud or telemetry uploads. Evidence includes [phase one](../../Artifacts/Validation/tutorial-progression/phase-one.txt), [restarted flow](../../Artifacts/Validation/tutorial-progression/runtime-tutorial-smoke.txt), [save summary](../../Artifacts/Validation/tutorial-progression/runtime-summary.json), and [screenshot hashes](../../Artifacts/Validation/tutorial-progression/screenshots-manifest.json). All 60 layout combinations passed automated geometry checks, with representative portrait, landscape and wide-PC captures visually reviewed.

![Actual owned armor comparison and equip](../../Artifacts/Validation/tutorial-progression/screenshots/actual-equipment-comparison.png)

![Korean 150% portrait support](../../Artifacts/Validation/tutorial-progression/screenshots/support-440x956-ko-150.png)

![English 150% landscape support](../../Artifacts/Validation/tutorial-progression/screenshots/support-956x440-en-150.png)

### Original captures by profile

| Resolution | Language | Text | Intro | Journal | Support |
| --- | --- | --- | --- | --- | --- |
| 440×956 | KO | 100% | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/intro-440x956-ko-100.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/journal-440x956-ko-100.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/support-440x956-ko-100.png) |
| 440×956 | KO | 150% | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/intro-440x956-ko-150.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/journal-440x956-ko-150.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/support-440x956-ko-150.png) |
| 440×956 | EN | 100% | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/intro-440x956-en-100.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/journal-440x956-en-100.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/support-440x956-en-100.png) |
| 440×956 | EN | 150% | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/intro-440x956-en-150.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/journal-440x956-en-150.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/support-440x956-en-150.png) |
| 956×440 | KO | 100% | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/intro-956x440-ko-100.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/journal-956x440-ko-100.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/support-956x440-ko-100.png) |
| 956×440 | KO | 150% | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/intro-956x440-ko-150.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/journal-956x440-ko-150.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/support-956x440-ko-150.png) |
| 956×440 | EN | 100% | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/intro-956x440-en-100.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/journal-956x440-en-100.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/support-956x440-en-100.png) |
| 956×440 | EN | 150% | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/intro-956x440-en-150.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/journal-956x440-en-150.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/support-956x440-en-150.png) |
| 1600×900 | KO | 100% | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/intro-1600x900-ko-100.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/journal-1600x900-ko-100.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/support-1600x900-ko-100.png) |
| 1600×900 | KO | 150% | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/intro-1600x900-ko-150.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/journal-1600x900-ko-150.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/support-1600x900-ko-150.png) |
| 1600×900 | EN | 100% | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/intro-1600x900-en-100.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/journal-1600x900-en-100.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/support-1600x900-en-100.png) |
| 1600×900 | EN | 150% | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/intro-1600x900-en-150.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/journal-1600x900-en-150.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/support-1600x900-en-150.png) |
| 1600×1000 | KO | 100% | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/intro-1600x1000-ko-100.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/journal-1600x1000-ko-100.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/support-1600x1000-ko-100.png) |
| 1600×1000 | KO | 150% | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/intro-1600x1000-ko-150.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/journal-1600x1000-ko-150.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/support-1600x1000-ko-150.png) |
| 1600×1000 | EN | 100% | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/intro-1600x1000-en-100.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/journal-1600x1000-en-100.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/support-1600x1000-en-100.png) |
| 1600×1000 | EN | 150% | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/intro-1600x1000-en-150.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/journal-1600x1000-en-150.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/support-1600x1000-en-150.png) |
| 2100×900 | KO | 100% | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/intro-2100x900-ko-100.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/journal-2100x900-ko-100.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/support-2100x900-ko-100.png) |
| 2100×900 | KO | 150% | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/intro-2100x900-ko-150.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/journal-2100x900-ko-150.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/support-2100x900-ko-150.png) |
| 2100×900 | EN | 100% | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/intro-2100x900-en-100.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/journal-2100x900-en-100.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/support-2100x900-en-100.png) |
| 2100×900 | EN | 150% | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/intro-2100x900-en-150.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/journal-2100x900-en-150.png) | [PNG](../../Artifacts/Validation/tutorial-progression/screenshots/support-2100x900-en-150.png) |

Additional scenes:

[Armor checkpoint](../../Artifacts/Validation/tutorial-progression/screenshots/armor-checkpoint.png) · [Boss cleared](../../Artifacts/Validation/tutorial-progression/screenshots/real-boss-cleared.png) · [First-rift preparation](../../Artifacts/Validation/tutorial-progression/screenshots/first-rift-preparation.png) · [Edict saved](../../Artifacts/Validation/tutorial-progression/screenshots/edict-saved.png) · [New skill equipped](../../Artifacts/Validation/tutorial-progression/screenshots/new-skill-equipped.png) · [Support review](../../Artifacts/Validation/tutorial-progression/screenshots/support-review.png) · [Rewardless replay](../../Artifacts/Validation/tutorial-progression/screenshots/rewardless-replay.png)
