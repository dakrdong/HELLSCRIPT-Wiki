# Rift Stage Rarity: Implementation and Validation Record

Work date: 2026-09-12. [한국어](Rift_Rarity_Expansion.md). See the [detailed design](../Design/HELLSCRIPT_Rift_Rarity_Detail.en.md) for the formula and complete representative probability table.

## 1. Baseline and actual changes

The user designated their separately maintained `Downloads/HELLSCRIPT_GDD_v0.1.md` and `Downloads/HELLSCRIPT_Content_v0.1.xlsx` as the design baselines for this task. Both originals include the fixed reroll-cost update dated 2026-09-12. They are saved at the following project paths, where the stage-dependent probability changes are applied.

| Type | Project path and change location |
|---|---|
| GDD | `Docs/Design/HELLSCRIPT_GDD_v0.1.md`: rift rarity reward rules and economy table |
| Excel | `Docs/Data/HELLSCRIPT_Content_v0.1.xlsx`: economy/reward rows and stage probability verification table |
| Existing project specifications | `Docs/Design/HELLSCRIPT_Unity_Implementation_Plan.md` section 9.1 and reroll costs; `HELLSCRIPT_Itemization_Detail.md` sections 8.1 and 8.3; `HELLSCRIPT_Item_Quality_Detail.md` chapter 9 expected values |
| Shared formula | `Assets/HELLSCRIPT/Runtime/Core/RiftRarity.cs`: probability calculation and selection using source and current stage |
| Parameters | `Assets/HELLSCRIPT/Runtime/Core/RiftRarityBalance.cs`: baseline probabilities, legendary caps, shared growth rate, and data validation |
| Actual combat | `Assets/HELLSCRIPT/Runtime/Core/CombatSimulation.cs`: normal/elite kill rewards and `BossClear` |
| Sweeps and costs | `Assets/HELLSCRIPT/Runtime/Core/Economy.cs`: `Sweep`, `RerollGold` |
| Interface | `Assets/HELLSCRIPT/Runtime/Presentation/GameUI.RiftRewards.cs`, `GameUI.RiftKeeper.cs`, `GameUI.cs`, `Resources/Localization/en.txt` |
| Checks | `Assets/HELLSCRIPT/Tests/Editor/RiftRarityTests.cs`, `ItemizationTests.cs`, `Runtime/Presentation/RuntimeRaritySmoke.cs` |

Other existing working-directory changes were preserved. Reward functions were connected to the existing processing flow; this feature did not change scenes, prefabs, packages, or game settings. The interface uses the existing Korean source-text keys and English translation table.

## 2. Calculation and preserved behavior

Define `t=max(0,R−1)`, `g=t/(t+49)`, and `legendary=p0+(cap−p0)×g`. Legendary baselines/caps are 1%/10% for normal enemies, 5%/25% for elites, and 15%/45% for bosses. Non-legendary grades are reduced proportionally using `baseline probability×(1−legendary)/(1−p0)`. All values are draft balance values for HELLSCRIPT.

Combat uses `State.stage`; sweeps use the relevant hero's `highestClear`. The 2% normal-enemy equipment-drop chance, one elite draw, three boss/sweep draws, item-level range, existing enhancement/dismantling/shop/crafting costs, gold and material amounts, individual legendary weights, and inclusion of sets are preserved. The new formula is not connected to shop, crafting, chest, or offline reward paths. Stored `Item` objects, `bossRewarded`, sweep receipts, and transaction structures remain intact, so finalized items are not rerolled during collection or reconnection.

## 3. Handling differences between code and documents

- Existing normal, elite, and boss probabilities and draw counts matched between the supplied files and code. They were retained as the stage 1 baselines.
- The code and existing project specifications still contained cost increases based on reroll count. The latest rule in the supplied files takes precedence, so the cost was corrected to `500×L+50×L×(L−1)`, where `L=max(1,item level)`. Cost is independent of reroll count; at L=30, every reroll costs 58,500 gold. The coefficients remain a test draft.
- The item-level cap of 60 specified in the project's earlier detailed item design is absent from the actual generation code. As requested, this task preserves the existing code's level calculation and documents the difference.
- The legendary generator applies class, slot, and individual member weights. `requiredSkill` is not currently an unlock filter for the generation pool. This feature changes neither the unlock policy nor the legendary/set lists added by later content expansions.
- The supplied GDD and workbook assign weight 1 to LW04, LA04 and LM04; the later project code and itemization design already use 10. The request explicitly preserves individual legendary weights, so the existing runtime value 10 remains, and unrelated baseline content rows were not overwritten. Stage rarity adjustment happens before this separate member selection.
- The supplied GDD proposes holding finalized sweep bundles when the bag is full; existing code instead rejects a sweep before drawing when fewer than three slots are free. The request preserves existing storage and duplicate-prevention behavior, so this change does not add pending sweep storage. Already-generated, unclaimed combat items were verified to survive saving and reconnecting unchanged.

## 4. Executed validation

Another Unity check was running in the shared working directory, so source, packages, and settings were copied to `/tmp/hellscript-rarity-work/unity` for validation. SHA256 hashes of the four reward Core files and two test files under validation match their originals. The Unity version is 6000.6.0f1, and the project's Test Framework version is 1.8.0.

### Unity Edit Mode

Across `RiftRarityTests`, `ItemizationTests`, `CoreTests`, `LocalizationTests`, and `OutcomeTests`, **115 tests passed with zero failures**. Of these, 28 are new rarity checks. Evidence is retained in `Artifacts/Validation/RiftRarity/focused.xml` and `focused.log`.

- Checked stage 1 baselines; treating negative and zero inputs as stage 1; monotonicity across stages 1–10,000; and probability ranges and totals at representative boundaries, stage one million, and `int.MaxValue`.
- Checked legendary, rare-or-better, and magic-or-better cumulative probabilities; ordering between sources; forbidden grades; adjustable parameters; and rejection of invalid data.
- Generated normal, elite, and boss kill rewards through the actual combat simulation's public `Tick`. Compared results that distinguish current stage 10 from highest-clear stage 50, along with random-state consumption.
- Tested sweeps while distinguishing the selected hero's highest-clear stage 50, another hero's stage 300, and a remaining stage 10 rift. Verified three draws, reward amounts, no first-clear or record updates, and rejection of duplicate requests.
- Saved and loaded unclaimed boss equipment on the ground, then checked that results and reward random state stayed unchanged after changing the highest-clear record and the stage requested for a new simulation. Repeated collection and reconnection did not duplicate rewards.
- Checked that offline settlement grants no equipment, reroll costs do not depend on reroll count, and insufficient funds leave existing state intact.

### Experiments using fixed random seeds

The tolerance for each sample was specified in advance as `max(0.001, 6×sqrt(p×(1−p)/N))`, measured in probability units. Grades with 0% probability must have exactly zero occurrences.

| Experiment | Samples and random seed | Result |
|---|---|---|
| Rarity by source | 3 sources × stages 1, 10, 50, and 1000 × 100,000 draws each; seed=20260912 | All tested distributions across 1,200,000 draws were within tolerance. Maximum absolute deviation was approximately 0.267464 percentage points. |
| Normal-enemy equipment drops | 50,000 kills each at stages 1 and 1000; seed=918273 | 1,075 drops (2.15%) and 1,080 drops (2.16%), both within tolerance of the 2% baseline. |
| Legendary member LW04 | 20,000 warrior necklaces already selected as legendary; seed=112681 | 1,846 occurrences (9.23%), within tolerance of the existing weight ratio 10/110=9.0909%. |

These results validate mathematical probabilities and automated selection. They do not measure actual players' farming distributions over extended play.

### macOS execution and Excel

The macOS development build **succeeded with zero errors**. An isolated test account in the native player verified the keeper button, Korean and English, 720×1280 and 1280×720 windows, no clipped labels or missing translations, and no account mutation from viewing chances. An active stage 10 remained the displayed stage despite a best record of 50 and selected stage 1. Automated UI checks and five saved screenshots were reviewed. Evidence includes `evidence/runtime-rarity-smoke.txt`, `evidence/player.log`, PNG files and `build.log`. This is a UI/flow fixture, not an extended manual combat playtest.

The workbook preserves the original 24 sheets and 23 tables and adds `24_균열등급확률`, bringing the total to 25 sheets. Only 12 existing explanatory cells were changed; the C27/EC13 fixed reroll rules remain identical to the source. Checks covered all 24 source/stage combinations and totals; linked results after changing growth span 49→98, normal legendary cap 10→12%, and baseline legendary 1→2%; and negative, zero, 1, 2, 49, 50, 51, one million and maximum Int32 stages. Defaults were restored before final recalculation and saving. Formula errors: zero. Rendered sheets were checked for layout. Recalculation was tested in the Artifact engine, not the native Microsoft Excel application.

## 5. Remaining limitations

Android/iOS device runs, extended economy balancing, and manual playtesting by the user have not been performed. The probability function was checked at very high stages, but this does not constitute unlimited-range validation of the complete monster-stat, currency, or item-stat systems at those stages. Changing parameters in Excel updates its verification table; runtime code data must still be updated separately. Existing local-save authority and duplicate-reward prevention were preserved; no new server-authority system was added.

## 6. Existing design updates and verification of the selected commit

Following the commit request on 2026-09-12, existing documents were checked again. The implementation plan and content catalog now link the supplied reference files at their project locations and describe the current reward policy. The rift exploration detail now covers stage-dependent normal, elite and boss rarity, the exclusion of chests, and sweeps using the selected hero's highest stage cleared in combat. Wiki status and related document histories were also updated.

The acquisition estimates in [itemization open items](../Design/HELLSCRIPT_Itemization_Open_Items.md) based on a fixed 15% legendary chance are labeled historical and recalculated using the current stage 30 formula. The legendary chance per boss equipment draw is approximately 26.153846%. With a necklace slot probability of 1/8 and the existing target member weight of 10/110, the chance of at least one target item per boss clear is approximately 0.888961%. This implies an average of 112.49 clears, or approximately 7.50 hours assuming four minutes per clear. These are theoretical boss-only values. The old combined field, elite and chest estimate is marked as requiring recalculation from current route counts. Individual legendary weights were not changed.

Because unrelated uncommitted features share the working directory, only the selected changes for this feature were exported into a separate Unity project and verified again. For the shared `CombatSimulation.cs`, `GameUI.cs` and English translation table, only this feature's changes are included. SHA256 hashes for all 16 selected code, test, localization and metadata files match the source under validation.

- All 115 relevant Unity Edit Mode tests passed again, with zero failures or skipped tests.
- The macOS development build from that source succeeded with zero errors.
- The new native player passed Korean/English, portrait/landscape, active stage 10 versus highest-clear stage 50, and account-immutability checks. All five saved screenshots were reviewed.
- Evidence is retained under `Artifacts/Validation/RiftRarity/publish/`: `publish-focused.xml`, `publish-focused.log`, `publish-build.log`, `publish-source-hashes.json` and `evidence/`. This path remains excluded from Git under existing project rules.
