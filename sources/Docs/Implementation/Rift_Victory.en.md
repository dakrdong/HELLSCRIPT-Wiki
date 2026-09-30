# Rift victory result window

Integration update, 2026-09-28: game implementation and refinements are included in the [all-worktree integration](All_Work_Integration_20260928.en.md). Branch-only and documentation-only publication statements below are historical scope records; this update and the integration record describe the current state.

Updated: 2026-09-30 · first written 2026-09-27 · [한국어](Rift_Victory.md)

## Layout rework on 2026-09-30

The `Prototypes/RiftVictory` layout the user approved on 2026-09-30 now drives the game's `RiftVictoryWindow`. The features are unchanged; only layout and presentation moved. Reward, retry, stop, return, Hunt Edict editing and the text log keep their existing transactions and windows. Where the sections below describe the layout, this section replaces them.

**Section titles.** Battle review and Growth & loot use bold bright gold (`UiTheme.GoldBright`, `FontStyle.Bold`) after a gilt lozenge, larger than the body text; on wide screens the section numbers (01, 02) are gold as well. The typeface stays the shared `UiFonts.Body`; only the weight changed. A Korean serif face is not installed on every platform and would need a bundled font file, so it was not used this time.

**Portrait (440×956).** From the top: rift record, battle review, then growth and loot. The battle review heading carries Details next to it and Edit Hunt Edict and Text log on the right, above total damage, damage taken and kills. The growth heading shows the next unlock on its right, above one card with level and gold on a single row. Only the loot list scrolls; everything else stays fixed. Shared `UiHeaderRule` gilt dividers separate the three sections. The repeat readout, the three actions and the retention note are docked at the bottom. Portrait no longer shows skill cards on the page; they moved to a tab of the details window.

**Landscape and PC (956×440, 1600×900, 1600×1000, 2100×900).** Four columns: rift record | growth + battle review | loot list + next unlock | a vertical action rail. The review column holds the 2×2 skills with the ultimate below, then Edit Hunt Edict and Text log. The rail holds the repeat readout, Claim reward (largest), Try again and Stop auto repeat (Return to sanctuary once stopped). The rail is 10% of the screen width (88–104).

**Skill cards (`RiftSkillShareView`).** Larger skill icons, with the gauges starting beside them. The narrow wide-screen grid uses short labels (Normal, Boss, HP shield, Applied…); the details window uses full labels. At larger text sizes each label stays on one line with its value and a real bar below it. Values come from the existing per-target and per-role `CombatStatistics` totals; the cards are not buttons.

**Battle details (`RiftReviewDetailWindow`).** Opened by Details. It was created with the new-window template (`tools/new_content_ui.py`) and uses `ContentWindowView` with `EquipmentViewSource.BattleSnapshot`. Portrait switches between objective (for example Break seals), skill damage share and damage taken tabs. Landscape and PC already show the skill cards on the page, so the window shows the objective step times beside the incoming-damage Top lists without tabs; at 956×440 and 100% it fits without scrolling. The Top-list drawing moved to `IncomingDamageRanking`, which replaces `IncomingDamageWindow`; ranking and shares still come from `CombatStatistics.TopIncoming`.

**Objective step times.** When a seal breaks, an essence carrier dies or an offering is picked up, `RiftObjectives.RecordStep` stores that combat second in `RiftLayout.objectiveSteps`. The result and the combat record keep them in `RiftObjectiveRecord.steps`, so they survive saving and resuming. The display reads "Seal 1 / 2 broken 31.2s". A step the kill meter skipped by opening the gate first is "Not reached". Steps from records made before this change, or finished before a resume from such a save, show "No record"; no time is invented. Offerings record the pickup; the altar delivery is the gate-open time.

**Repeat readout.** No words:
- hourglass + seconds until the next run
- the repeat-count icon (a runner inside two chasing arrows) + finished runs (×3)
- the real elapsed time of the repeat session as HH:MM:SS (a second row on the rail, on the right of the same row in portrait)

Paused states (a detail window open, stopped, awaiting save) turn muted; states that need action turn gold and keep the existing retry button. At the user's request the two icons were drawn by Codex's built-in image generation as white single-colour PNGs (`Resources/Art/RiftResult`) that the game tints. A runner and a cycle icon first stood side by side; the user then asked for one merged symbol that reads as the repeat count. The tool did not report a model, so they are recorded as candidates. A vector glyph covers a missing texture. Prompts, source paths and alpha measurements are in the [generation record](../Art/RiftResult/rift-result-icons-manifest.json). `RiftResultArtImporter` sets transparency and mipmaps, and `ResourceTextureBudget` caps every platform at 128.

The HTML prototype uses the same icons and layout. Validation is recorded in [Validation on 2026-09-30](#validation-on-2026-09-30) below.


The approved `Prototypes/RiftVictory/HELLSCRIPT-RiftVictory.html` composition now feeds the actual normal-rift victory and failure results. Four regular skills occupy a 2×2 grid; the ultimate spans the next row. Failed rifts use the same result adapter and open the existing failure analysis; training retains its existing result route.

## Validation on 2026-09-30

The full Edit Mode suite ran on commit `47a68801`, which merged `main`'s item art work (#41, `9a22067e`). `main` then gained the skill combat preview work (5 commits, `ee9f9055`), merged in the final commit `e3c4731c`; the full suite was not re-run there. Instead, 1,234 focused Edit Mode tests where the two merges meet and the smokes were run again. Everything was tested in Unity 6000.6.0f1 batch mode on a copy of the worktree, with a macOS development build. Each smoke used an isolated `-hellscriptSavePath`; the user's save was neither read nor written. Input was synthetic uGUI events; no physical iOS/Android device was used. The screenshots below were retaken from the final commit's build. [Validation summary](RiftVictoryLayoutEvidence/validation.json)

- **Full Edit Mode**: commit `47a68801` ran 4,752 tests, 4,705 passed, 47 failed, 0 skipped. Every failure is in the existing baseline (47 tests that already fail on `main` before this change); 0 failures are outside it.
- **Focused Edit Mode**: commit `47a54f24` ran 190 and `9ada7d88` ran 74 (texture budgets, localization, buttons, result records); all 190 passed, including the localization table, stored-log translation (`CombatJournalTests`, `StoredLocalizationTests`), texture budgets, objective progress, seals and result records. Four new tests cover seal, carrier and offering step times and check that older records and resumed runs never invent a time.
- **Focused Edit Mode on the final commit `e3c4731c`**: 1,234 tests, 1,227 passed, 7 failed (all in the existing baseline, 0 new). It covers the areas this work changed (translation, texture budgets, objective progress, seals, result records, idle hunt status) together with what the second merge touched (skill presets and previews, edict responses, class skills, rift flow, world effects, save restore). The full suite was not re-run after this merge.
- **Shared UI contract**: `check_ui_contract.py` passes; `test_ui_contract.py` 11 tests pass.
- **Rift result smoke** (`-hellscriptRiftVictorySmoke`): victory, restart, utility skills and failure all pass. In 20 layouts (5 aspect ratios × Korean/English × 100/120%) it checks the portrait stack, the four wide columns and the vertical action rail, both dividers, that the repeat readout uses only icons and numbers (including HH:MM:SS), clipped text, missing translations and the safe area. In the same 20 layouts the battle details show the objective step times, boss Top 3 and monster Top 5 with their shares, portrait tab switching, the tab-free wide layout, and no scrolling at 956×440 and 100%. Loot scrolling and collapsing, the shared detail for salvaged/discarded items, stop → return (the countdown turns to "—"), reward, Hunt Edict and text log, save-failure recovery and a separate-process restart also pass. [Victory](RiftVictoryLayoutEvidence/victory-runtime.txt) · [Utility](RiftVictoryLayoutEvidence/utility-runtime.txt) · [Failure](RiftVictoryLayoutEvidence/defeat-runtime.txt) · [Restart](RiftVictoryLayoutEvidence/restart.txt)
- **Smokes on the final commit**: victory, restart and failure, `main`'s item art smoke (20 layouts, 6 set details, 3 wallet layouts and the secondary screens) and the skill combat preview smoke passed. The utility skill smoke failed once in that batch: at portrait 440×956, Korean, 120%, the "상세 보기" button's text height of 21.17 exceeded the allowed 20+1 because the label folded onto two lines and showed only "상세". Five re-runs of the same build (two idle, three under artificial CPU load) all passed, so it could not be reproduced. It looks like an intermittent problem that depends on when the text width is measured; the cause was not found.
- **Objective and gate smokes**: `-hellscriptObjectiveSmoke`, `-hellscriptChainSmoke` (carrier and offering) and `-hellscriptGateSmoke` fail, and a build of unchanged `main` (a6612e09) fails them with the same exceptions. Their fresh-save fixtures start the mandatory tutorial map and stop before any objective code runs, so they do not cover the step times; the Edit Mode tests above and the rift result fixture do.
- A natural rift played to the end was not run this time (the smokes are fixtures that use the real reward and completion paths).

![Portrait result](RiftVictoryLayoutEvidence/layout-440x956-ko-100.png)

[Portrait, English 120%](RiftVictoryLayoutEvidence/layout-440x956-en-120.png) · [Landscape](RiftVictoryLayoutEvidence/layout-956x440-ko-100.png) · [Landscape, English 120%](RiftVictoryLayoutEvidence/layout-956x440-en-120.png) · [PC](RiftVictoryLayoutEvidence/layout-1600x900-ko-100.png) · [21:9, English 120%](RiftVictoryLayoutEvidence/layout-2100x900-en-120.png) · [Landscape details](RiftVictoryLayoutEvidence/details-956x440-ko-100.png) · [PC details](RiftVictoryLayoutEvidence/details-1600x900-en-120.png) · [Portrait seal tab](RiftVictoryLayoutEvidence/details-objective-440x956-ko-100.png) · [Portrait skill tab](RiftVictoryLayoutEvidence/details-skills-440x956-ko-100.png) · [Portrait damage-taken tab](RiftVictoryLayoutEvidence/details-incoming-440x956-en-120.png) · [Utility skills](RiftVictoryLayoutEvidence/utility-956x440-en-120.png) · [Failure](RiftVictoryLayoutEvidence/defeat-1600x900-ko-100.png)

## Publication scope and implementation status

**The game implementation is verified on its feature branch and is still awaiting integration into game `main`.** The behavior, screenshots and validation below describe [implementation commit `1acade60`](https://github.com/dakrdong/HELLSCRIPT/commit/1acade602665ee46f62b4bb95ca90bcbe3920a4f). They do not indicate that these changes are already active in the current main game.

The requested public wiki update brings only the Korean/English documents, document history and validation evidence into `main`. The public game databases, shared UI contract and `sources/Assets/` continue to reflect merged `main`. New windows and telemetry described here belong to the feature implementation, not the current public database. Other threads and ongoing 3D work are excluded.

## Shared layout and ownership

`GameUI.ShowResult` routes normal victories and failures to the generated `RiftVictoryWindow` entry point, using `ContentWindowView` and `EquipmentViewSource.RewardSnapshot`. Record, growth/loot and battle review use three columns in landscape/PC and stack in portrait. Only the source-grouped loot list scrolls independently; repeat status and the three primary actions remain fixed.

The adapter reuses sanctuary, equipment, gem, rune and skill artwork, `UiTheme`, `UiFonts`, `EquipmentSlotView`, `ItemDetailView`, `SkillIconView`, `JewelerArt`, `RuneV13Art` and common buttons. No new raster or private font is generated. Shared slot sizes/gaps determine responsive column counts. Large decorative numerals stay fixed; reading scale applies to captions and controls.

A completed result is a page. It registers with `ContentWindowHost` using `blocksGameplay:false`, retaining stack order, background input and Back ownership. Nested detail/reward/comparison windows use normal blocking, so the repeat countdown runs on the result and pauses while inspecting details.

## Data boundaries

Equipment source is recorded at the actual monster/boss/chest reward boundary. Rune grants snapshot the actual awarded `OwnedRune`, preserving reward receipts and RNG consumption. Resource drops retain their source and collection state; gold stays in the fixed growth summary. Successful salvage, full-bag replacement, sale and automatic warehouse transfer record their outcomes against the current run's item IDs. Rejected mutations and failed writes do not alter outcomes. Successful staged transactions synchronize disposition metadata back to the live simulation and completed result without replacing earlier-run history.

A successful pickup separates the acquisition snapshot from the mutable inventory object, so later equip/enhance changes cannot alter the recorded item.

Field monsters, boss monsters, chests and objects form collapsible groups. Disposed equipment retains its original slot and artwork under a dark overlay labelled Salvaged or Discarded; sold/warehouse outcomes are also distinguished. Every gear slot opens the shared `ItemDetailView` with its stats, affixes and unique effect, including salvaged and discarded snapshots, without equip/recover actions. Gems show the authoritative `GemCatalog` slot effects and quantity; runes show their actual type, grade and shape. Empty object groups stay empty; prototype samples never become game rewards.

Older saves keep unknown source/outcome values. Source is never inferred from position, item name or current inventory. Immutable combat archive and telemetry bytes are not rewritten; the committed account result and `pendingResult` own post-combat cleanup metadata.

Combat-time bests use a separate `RiftEntryProgress.combatBest`; entry-screen real-time bests remain unchanged. A run captures the previous best on entry, and normal completion updates once. Display and deltas use integer tenths. First, improved, retained, abandoned and unavailable-record states are distinct. Legacy seeding uses identifiable retained victories for the same character and tier; it does not claim complete all-time history or relabel an old clear as the first clear.

Skills read the battle build, never current editor choices. The five fixed slots use recorded releases, damage and non-use checks from `CombatStatistics`/`CombatFeedback`; empty slots retain their positions. Full details still include basic attacks, passives and equipment effects.

## Actions

Stop repeat calls `StopAutoRepeat` and becomes Return to sanctuary in place. Claim reward uses the existing `RewardBoxesWindow` grant transaction, then opens reward-box inventory after claiming. Retry enters the same tier through existing fatigue, supplies and save validation. Failed result saves block departure/reward actions and provide retry; retrying a stopped result never re-enables repetition. Edit Hunt Edict opens the existing `HuntEdictWindow` skill-policy editor directly. The countdown pauses while editing and closing returns to the same result. Existing `HuntEdictEditSession` / `GameStore.CommitHuntEdict` own draft saving; the finished battle snapshot stays unchanged. Text logs and previous-run comparison remain read-only; full battle details remain available through the existing combat-record window. The first-play guide and duplicate first-reward action are absent from this page.

The inventory and Rift result now share the entire `ItemDetailPopup` shell, including the title, close and range controls, fixed footer and scrolling card viewport. `ItemDetailView` owns equipment fields; a result-specific summary popup is not used. Disposed snapshots stay inspectable without equip or recovery transactions.

## Role-specific contributions and text log

`ClassSkills.json` defines each skill's result metric. Healing uses actual HP restored divided by all-source HP restoration; resource skills use actual resource restored divided by all-source resource restoration; protection uses damage prevented or absorbed by the skill divided by all-source prevented or absorbed damage. Basic attacks, regeneration, potions, gear and passives remain in the relevant denominators. Excess recovery and unused shield capacity do not count. Hybrid skills display their primary role; Brand of Challenge displays protection and resource shares separately. Single recovery/protection roles display the actual amount on the second row. Movement uses measured distance; marking uses application counts.

Protection follows disjoint stages: sheet defense, class guards, leap defense, block and consumed shields. Concurrent class guards share their actually applied, capped mitigation proportionally. Only successful additional block rolls belong to the block skill; telemetry draws no extra RNG. Dodge and perfect block belong to the all-source denominator. Protected HP represents prevented incoming damage, not a counterfactual survival estimate capped at remaining HP. `CombatTelemetry` version 4 preserves cumulative counters independently of effect-event retention; older records show “No record” for unavailable contributions.

`CombatLogWindow` uses `ContentWindowView` with `EquipmentViewSource.BattleSnapshot`. It reads archives through `GameStore.ReadCombatRecord`, displays timestamp/message rows in a scrolling body with 100-row pagination, and provides only Close in its fixed footer. The old “Back to battle summary” action is removed. The underlying result remains mounted, closing returns to that same result, and account/equipment/rewards remain unchanged. The modal pauses the repeat countdown. Missing archives retain the saved-summary/legacy-log fallback with an explicit message.

## Incoming damage and source rankings

Battle review displays outgoing damage, incoming damage and kills together. Details below incoming damage opens `IncomingDamageWindow`, a `ContentWindowView` adapter with an explicit `BattleSnapshot` source, scrolling body and fixed Close action. Closing reveals the same underlying result.

Incoming damage uses the existing post-defense final damage metric, including damage absorbed by shields. It is distinct from actual HP lost. The actual `RecordDamage` boundary captures the source; cumulative totals do not depend on retained event samples.

- **Boss attack Top 3:** group by boss type and attack ID, then select the three highest damage groups. Repeated hits from the same attack combine. Percentages use only the sum of the selected three groups.
- **Monster Top 5:** group nonboss enemies by type across instances and attacks. Bosses are excluded; ordinary summoned adds are included. Show only monster name, damage and share. Percentages use only the sum of the selected five groups.
- Stable IDs break ties. Only positive-damage groups appear; missing ranks are not filled. Environmental or unidentified damage is explained separately and excluded from both ranking denominators.
- Pre-v4 saves retain their existing totals but show an unavailable-record message for incomplete source data. Recent logs never stand in for a whole battle. Inspection, ranking and closing are read-only.

## Latest refinement validation — 2026-09-27

[Refinement evidence](RiftResultRefinementsEvidence/validation.json) is separate from the initial implementation record below.

- **138 focused Edit Mode cases passed**, zero failures/skips. Coverage includes capped recovery, natural/channel mana attribution, nonduplicated protection, unchanged block RNG, full-battle source aggregates, top-only denominators and old-save compatibility. Only portrait layout and fixture geometry checks changed afterward; the final build and native acceptance were rerun.
- Shared ownership and **nine UI contract tests** passed. The final macOS development build succeeded with zero compilation errors. Batch tooling was used because no Editor instance was connected; this is not a live Editor Console-zero claim.
- Victory, utility-skill victory and defeat each passed 20 ratio/language/text combinations: **60 result layouts and 60 incoming-detail layouts**. Checks covered top limits, displayed-list percentage denominators, translations, text and Close bounds. Text logs passed **40 layouts** across the two victory fixtures.
- Portrait spacing now exposes one complete loot-slot row. The five skills and fixed actions remain visible. Scroll/accordion, shared item details, repeat controls, Hunt Edict and original-result return were verified. [Victory](RiftResultRefinementsEvidence/victory-runtime.txt) · [Utility](RiftResultRefinementsEvidence/utility-runtime.txt) · [Defeat](RiftResultRefinementsEvidence/defeat-runtime.txt)
- An [independent restart](RiftResultRefinementsEvidence/restart.txt) preserved incoming ranks, best time, item dispositions and stopped repetition. After activating the native macOS window, CUA pointer/wheel input verified incoming Details/Close, scrolling the text log to its end and closing, and the shared item popup with a compact footer and no leftover inner slot selection frame. [Pointer evidence](RiftResultRefinementsEvidence/native-pointer.json)
- Standalone HTML rebuilt with 331 bilingual strings and 16 unchanged PNGs and passed syntax checks. Current HTML rendering was not verified because the in-app browser rejected local URL navigation; no other browser or alternate-surface workaround was used.

All saves were isolated. Synthetic safe areas and macOS input do not constitute physical iOS/Android acceptance. The game implementation is committed and pushed on `codex/rift-result-refinements`. Game integration and documentation publication are tracked separately.

![Incoming damage details](RiftResultRefinementsEvidence/incoming-pc.png)

[Portrait result](RiftResultRefinementsEvidence/result-portrait-ko-120.png) · [Utility landscape English 120%](RiftResultRefinementsEvidence/utility-landscape-en-120.png) · [Defeat](RiftResultRefinementsEvidence/failure-pc.png) · [Text log](RiftResultRefinementsEvidence/log-pc.png) · [Shared item detail](RiftResultRefinementsEvidence/native-pointer-common-item.png)

## Validation

The latest refinements are on `codex/rift-result-refinements`, containing only this conversation’s work. Other feature branches and ongoing 3D work are excluded. Game integration remains deferred; this document and its evidence are published separately to the wiki. The records below describe the initial implementation.

Verified on 2026-09-27 with Unity 6000.6.0f1 in the isolated worktree. No connected Editor instance was available, so the established batch-test and `ProjectBuilder.BuildMac` paths were used. `-hellscriptSavePath` isolated both account and display settings from the user's saves.

- Shared UI ownership and all nine contract tests passed.
- The focused regression run passed 162 Edit Mode cases. After strengthening pickup snapshot isolation, all 23 result/exploration cases passed. These comprise 173 distinct cases, with zero failures/skips, not the entire 4,000+ case suite. [Summary](RiftVictoryEvidence/validation.json)
- The macOS development build completed with zero compilation errors. Native acceptance covered 20 combinations of 440×956, 956×440, 1600×900, 1600×1000 and 2100×900, Korean/English and 100%/120% text. Checks covered skill geometry, fixed controls, text height and missing translations, using simulated safe insets.
- uGUI raycasts verified the topmost target before synthetic pointer/wheel events. Acceptance covered loot scroll/accordion, salvaged/discarded shared item details, repeat pause/resume, Stop→Sanctuary, rewards, the Hunt Edict policy editor, text logs and failed-save protections. [Runtime results](RiftVictoryEvidence/runtime.txt)
- An independent process verified persisted source/disposition, previous combat best and stopped repeat state. [Reload results](RiftVictoryEvidence/restart.txt)
- HTML validation covered the same 20 base layouts and 297 bilingual strings. Equipment tooltip/gem-effect examples were exported from the shared Unity owners without separate stat calculations.

Development fixture values and synthetic macOS input do not prove natural combat completion, physical touch or iOS/Android device behavior. Physical mobile acceptance was not performed. The initial implementation is on `codex/rift-victory-preview`; refinements are on `codex/rift-result-refinements`. Only the documentation and evidence enter `main` for publication; game integration remains a separate task.

![PC result](RiftVictoryEvidence/pc-ko.png)

[Portrait](RiftVictoryEvidence/portrait-ko.png) · [Landscape English 120%](RiftVictoryEvidence/landscape-en-120.png) · [Discarded item detail](RiftVictoryEvidence/discarded-detail.png) · [Hunt Edict destination](RiftVictoryEvidence/hunt-edict-tab.png)

### Shared popup follow-up

The shared-popup revision passed 69 focused Edit Mode cases. A focused native inventory run passed five ratios × KO/EN at normal and 150% text, including card contents, range geometry, comparison, lock/unlock/equip and save reload. [Popup runtime result](RiftVictoryEvidence/popup-result.txt). HTML popup bounds, internal scrolling and fixed footers passed all 20 ratio/language/text combinations; range rows retain their heights when toggled.

Two broader existing smoke runs were incomplete and are not counted as full passes: `ItemPolishAcceptance` passed ten detail/comparison layouts before the drag-cancellation account-equality assertion failed; `RangeAcceptance` passed inventory detail/help/Ctrl and storage checks before a null shop target at line 81. Their causes were not isolated against the unchanged baseline. See the validation summary for these limits.

Native macOS CUA pointer input additionally verified discarded-item inspection, the global range toggle/help and Escape behavior, and Hunt Edict navigation/return. [Pointer evidence](RiftVictoryEvidence/native-pointer.json) · [Shared popup screenshot](RiftVictoryEvidence/native-common-item-detail.png).


## Result refinements on 2026-09-27

Both cleared and failed ordinary rifts use the shared result page; training retains its existing route. Failure uses a dark red treatment of the existing sanctuary artwork, a prominent “Rift Failed” heading, a failure mark and the actual termination reason. It preserves the previous best and opens the existing failure analysis instead of offering a clear reward.

`ItemDetailPopup.SetFooter` owns footer measurement and action layout for inventory and rift inspection. Callers pass context, notes and callbacks only. A kept item has its source/status and Close in one row; disposal notes add only their measured height. Header, ranges, scrolling item details and footer share the same owner.

The four regular skill cards and full-width ultimate are noninteractive. They show separate shares of damage to normal monsters and bosses, each with its own all-source denominator. Normal includes nonboss elites and adds. Basic attacks, passives and equipment effects remain in the denominator; skill-owned secondary and periodic damage contributes to that skill. Boss classification is captured at the outgoing damage event owner. `CombatTelemetry` v4 persists both aggregates beyond the 2,000-event retention limit and across restore. An undamaged category shows 0%; old records without a complete split show “No record” instead of invented percentages. Cast counts and unused status remain visible; detailed decision causes remain in combat records.
