# Action priority panel and shared decision order · 2026-10-11

`common.order` now connects the dedicated quick editor to actual combat decisions. It reuses the shared frame, drag cards, projected labels and forecasts from combat position/distance (PR 93) and target selection (PR 95). The reference is the Korean/English local `edict-order-panel/design/hud-quick-edit-mockup.dc.html`.

## Behavior

Emergency survival is evaluated first, outside the editable list. The default list is **Dodge → Buff upkeep → Pickup → Attack → Objectives → Regroup → Explore**. Ordinary dodge and dodge holds follow that order: a ready action above Dodge can execute first. Physical action locks, cast eligibility and existing emergency constraints remain enforced.

Pickup uses the `PRIORITY_LOOT` rank. `PRIORITY` loot becomes available during combat when Pickup is above Attack, and only after combat when below it. A priority rarity does not promote other rarities. `AFTER_COMBAT` retains its clear-field condition; `PASSING` never creates a detour. Chests and shrines remain within Explore and use `explore.order`, including the existing access point for a discovery already in progress.

Buff upkeep covers legacy W06 Shout, M05 Shield and A06 Shadow, native self buffs, and timed utility potions. A candidate needs at most `max(1.5 seconds, 15% of actual duration)` remaining and either a perceived enemy or ongoing combat. Automatic OFF, cooldown, resource, the original automatic condition and potion stock still apply. Refresh replaces the same effect without stacking. Potions refresh the same potion ID. Remaining time is discarded when the new duration starts. Shield caps and other-shield restrictions remain enforced.

Putting Objectives above Attack permits approaching observed seals, carriers, offerings and altars while enemies are present. Actual seal and offering progress retains `FieldSafe`. An already reached objective that cannot safely progress yields to a ready attack instead of repeatedly waiting at the same point. Switching to a higher action cancels the operation under its existing progress-retention rules. Ties use IDs.

Regroup waits in place out of combat while HP is below `survival.returnHpPercent` or resource is below 50%. Recovery of both values or a new enemy ends the wait. A 15-second cap measured from the original start prevents immediately restarting in the same depleted state. Combat or recovery resets that cap. `RunState.edictAction` v1 saves the start, waiting, exhaustion, selected category and walking state. Resume preserves its deadline; invalid versions, times or combinations are rejected without rewriting the input.

Native automatic skills compete as BUFF or ATTACK candidates rather than casting ahead of the order. Out-of-range native attacks approach through the existing shared movement policy.

## Panel and preview

The shared fixed-height, safe-area and right-side landscape frame contains the nameplate, description, fixed gold emergency notice, seven cards, Undo and Play/Pause. Drag, keyboard and arrow moves reuse the existing components. Draft changes update ranks, Running, hero labels, pickup timing and world paths. Drop saves once; cancel restores the saved preview without saving. The quick list also shows its current first priority.

The hero label reads `Now · N · action`. Pickup, objectives and exploration use dashed actual paths; attacks use a gold arrow; Buff upkeep and Regroup emphasize the hero ring. Dashes include path corners rather than crossing walls. Pickup labels follow its rank relative to Attack. No new Canvas, external dependency or raster asset is added.

`PrepareEdictActionPreview()` prepares seven candidates when the panel opens or commits. `PreviewEdictAction(order)` reads only cached candidates and the draft, returning a struct without allocations or RunState writes on repeated calls. Objective, chest, shrine and exploration owners accept `commit:false` to avoid changing progress, saved goals or repathing. Execution uses the same candidates and rank selection.

The always-visible HUD change (PR 91) must retain `quickLive` on integration: town editing saves hero settings without enabling battle previews or playback controls.

The integration candidate includes the always-visible HUD, monster codex, registry/pipeline branches and the completed eight-monster batch `ac5cacf6`. Incoming model, portrait and metadata blobs are preserved. W16 retains its slow/root reaction before shield expiry. Initial M05 use retains its original engage/prevent purpose; only an existing owned shield qualifies for upkeep. Native category execution uses its original class owner and consumes a ready opening only when the action actually starts.

Shared decision repairs retain paid actions, prevent a lower-ranked action from cutting off dodge arrival, and recheck selected rules against current targets. Rejected aim and response reasons remain in decision records. Movement still uses `HeroPace` and computes walking from the final wall-constrained displacement. Town editing retains `quickLive` and saves settings through the existing owner.

## Saves and share codes

The global version is **v8**. `Version7Global` freezes the prior six choices and positional HED2 layout. Migration validates the old version first, removes EMERGENCY and AFTER_COMBAT_LOOT, preserves the relative order of DODGE/PRIORITY_LOOT/ATTACK/EXPLORE, then inserts BUFF after DODGE, OBJECTIVE after ATTACK and REGROUP before EXPLORE.

The PRIORITY_LOOT position wins even when the removed after-combat action appeared earlier. The two removed ranks cannot be recovered. Per-rarity pickup modes remain intact. Unknown, duplicate and missing values are rejected before removal can hide them; input documents remain unchanged.

Hero documents, native `legacyEdict`, presets and HED2–HED5 use the same migration. Raw v5 puzzle fixtures remain historical input. Combat presets put Attack above Pickup; recovery does the reverse; survival uses the default. The combat preset is named “Fight then Collect”. E20 and English copy are updated.

## Validation record

Final scoped coverage has no unresolved failures. Native screen acceptance remains pending because the user cannot currently unlock the Mac.

- The final full Edit Mode suite was run once on `f2f81f86`: 6,015 cases, 5,881 passed, 130 failed and four explicit authoring/measurement skips. Repairs reran only failed and directly affected coverage. Completed XML combined by full test name yields **2,068 unique scoped cases: 2,066 passed, two explicit authoring skips, zero unresolved failures**. All 130 original failures have actual follow-up. Unchanged passing results are reused; merging or updating documentation does not trigger another full suite.
- The scope includes migration/share codes, all historical orders, priority pickup, ordinary/emergency survival, upkeep, objective safety, regroup/resume, native skills, target/field behavior, imported monsters, localization, tutorial/puzzle outcomes and store bindings. Final arrival and native-opening regressions passed. Selected category and walking state survive save/resume; old v1 state may omit the category, while an unknown category is rejected with the original checkpoint intact.
- Shared UI contract validation and its 15 regressions passed. Refresh validation passed for 15 bindings and 181 UI sources. The registry tool regressions passed all ten cases. Source and XML receipts preserve commit, dirty-input hashes, exact filters, environment and generated material patches.
- The earlier 189-case candidate remains historical evidence. Current integration XML, chronological reuse ledger and recipes are under `integration-repair-20261012/coverage-ledger-final.json`; the old report is not presented as the final candidate's full result.
- No combat balance, potion, reward or tutorial-enemy values were tuned to clear assertions. The early L10 chest fixture uses the already-unlocked return-HP setting of 50% to isolate immediate opening. Separate three-class tests preserve the actual default order and its recovery effect below. The action-order setting stays locked until graduation.

The passive comparison uses the preceding target-selection result, identical BUILD_REF_SW01/SA01/SM03, two seeds, 20 seconds, group and no gear. All 24 before/after rows survived. Full-build mean damage is:

| Build | Before | After | Change |
|---|---:|---:|---:|
| BUILD_REF_SW01 | 9,509.06 | 8,319.46 | -12.51% |
| BUILD_REF_SA01 | 1,852.24 | 1,852.24 | 0.00% |
| BUILD_REF_SM03 | 13,863.67 | 13,863.67 | 0.00% |

For Warrior, W14 starts changed from four to three and BASIC from 17 to 18 at both seeds. These are measured action-order results in a short simulation, not whole-frame or device performance. No balancing adjustment was applied. Exact row keys, hashes and cast counts are retained in `passive-order-comparison.json`.

L10 `after` with immediate opening fails for all three classes (event starts at 0.25–0.30s). With the actual default order, Regroup delays Explore until 15.25–15.30s:

| Class | Result with default Regroup | Event start | Finish |
|---|---|---:|---:|
| Warrior | Failed | 15.30s | 27.30s |
| Ranger | Cleared | 15.25s | 25.35s |
| Mage | Cleared | 15.30s | 26.65s |

The `safe` setting still clears all classes after actual healing and the potion cooldown; skipping misses the chest goal. Results are derived from the real event, chest and saved reward owners, with no forced win/loss. Other repaired tutorial/puzzle cases retain their authored outcomes.

`RuntimeBattleQuickEditSmoke -hellscriptOrderOnly` is implemented for KO/EN at 440×956, 956×440, 1600×900, 1600×1000 and 2100×900, including drag/cancel/single-save, keyboard/arrows, safe areas, rotation, Running, pickup wording and playback. **The final native smoke and screenshots have not run.** A final development player is retained outside worktrees for that pending verification. Local source documents are updated; public wiki/site publishing remains on hold.

Artifacts retain XML, logs, source hashes, the original save and recovery manifests through review on 2026-10-26. Managed checkouts are archived only after main integration and dependency checks. No permanent deletion or routine change is authorized.

## Defaults to review

- Whether to retain the 1.5-second / 15% buff-refresh threshold.
- Whether to retain Regroup's 50% resource threshold and 15-second cap.
- Lowering ordinary Dodge below Attack can delay leaving danger when an attack is ready. Emergency survival is never lowered.

Physical mobile devices, the WebGL player, production server persistence and three matched before/after CPU measurements are outside this document's macOS validation scope.
